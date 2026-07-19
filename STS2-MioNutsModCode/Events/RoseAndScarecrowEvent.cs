using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.ValueProps;

namespace STS2_MioNutsMod.STS2_MioNutsModCode.Events;

public sealed class RoseAndScarecrowEvent : STS2_MioNutsModEvent
{
    private const string CifkaRelicTypeName = "MoreDollRelics.src.Relics.CifkaDoll";
    private const int SecondActIndex = 1;

    public override bool IsShared => false;
    protected override IEnumerable<DynamicVar> CanonicalVars => [];

    public override bool IsAllowed(IRunState runState)
    {
        return runState is not null
               && runState.CurrentActIndex <= SecondActIndex
               && GetCifkaCanonical() is not null;
    }

    protected override IReadOnlyList<EventOption> GenerateInitialOptions()
    {
        var canonical = GetCifkaCanonical();
        var hasRelic = Owner is not null && FindOwnedCifkaRelic(Owner) is not null;
        var relicHoverTips = canonical is null
            ? Array.Empty<IHoverTip>()
            : HoverTipFactory.FromRelic(canonical);

        return
        [
            CreateSafeOption(
                "TAKE",
                canonical is null ? null : Snatch,
                relicHoverTips),
            CreateSafeOption(
                "GIVE",
                canonical is null || !hasRelic ? null : Offer,
                relicHoverTips),
            CreateSafeOption("LEAVE", Leave, Array.Empty<IHoverTip>())
        ];
    }

    private async Task Snatch()
    {
        if (Owner?.Creature is null)
            return;

        var canonical = GetCifkaCanonical();
        if (canonical is null)
        {
            SetEventFinished(PageDescription("FAILED"));
            return;
        }

        await RelicCmd.Obtain(canonical.ToMutable(), Owner);
        await CreatureCmd.Damage(
            new BlockingPlayerChoiceContext(),
            Owner.Creature,
            13m,
            ValueProp.Unblockable | ValueProp.Unpowered,
            null,
            null);

        ModifyRandomNonStarterCards(costDelta: 1, requireCostAboveZero: false, maxCount: 2);
        SetEventFinished(PageDescription("TAKE_DONE"));
    }

    private async Task Offer()
    {
        if (Owner is null)
            return;

        var owned = FindOwnedCifkaRelic(Owner);
        if (owned is null)
        {
            SetEventFinished(PageDescription("FAILED"));
            return;
        }

        await RelicCmd.Remove(owned);
        await PlayerCmd.GainGold(300m, Owner, false);

        ModifyRandomNonStarterCards(costDelta: -1, requireCostAboveZero: true, maxCount: 2);
        SetEventFinished(PageDescription("GIVE_DONE"));
    }

    private async Task Leave()
    {
        if (Owner?.Creature is null)
            return;

        await CreatureCmd.Damage(
            new BlockingPlayerChoiceContext(),
            Owner.Creature,
            2m,
            ValueProp.Unblockable | ValueProp.Unpowered,
            null,
            null);
        SetEventFinished(PageDescription("LEAVE_DONE"));
    }

    private void ModifyRandomNonStarterCards(int costDelta, bool requireCostAboveZero, int maxCount)
    {
        if (Owner is null || maxCount <= 0)
            return;

        var deck = PileType.Deck.GetPile(Owner).Cards;
        var candidates = deck.Where(card =>
                !card.IsBasicStrikeOrDefend &&
                !card.EnergyCost.CostsX &&
                (!requireCostAboveZero || card.EnergyCost.Canonical > 0))
            .ToList();

        if (candidates.Count == 0)
            return;

        int count = Math.Min(maxCount, candidates.Count);
        for (int i = 0; i < count; i++)
        {
            int index = Rng.NextInt(candidates.Count);
            var target = candidates[index];
            candidates.RemoveAt(index);

            int oldCost = target.EnergyCost.Canonical;
            int newCost = Math.Max(0, oldCost + costDelta);
            if (newCost == oldCost)
                continue;

            target.EnergyCost.SetCustomBaseCost(newCost);
            target.DynamicVars.RecalculateForUpgradeOrEnchant();
        }
    }

    private static RelicModel? GetCifkaCanonical()
    {
        return ModelDb.AllRelics.FirstOrDefault(relic => relic.GetType().FullName == CifkaRelicTypeName);
    }

    private static RelicModel? FindOwnedCifkaRelic(Player player)
    {
        return player.Relics.FirstOrDefault(relic => relic.GetType().FullName == CifkaRelicTypeName);
    }

    private EventOption CreateSafeOption(string optionSuffix, Func<Task>? onChosen, IEnumerable<IHoverTip> hoverTips)
    {
        var textKey = InitialOptionKey(optionSuffix);
        var title = GetOptionTitle(textKey) ?? new LocString("events", textKey + ".title");
        var description = GetOptionDescription(textKey) ?? new LocString("events", textKey + ".description");

        return new EventOption(this, onChosen, title, description, textKey, hoverTips);
    }
}

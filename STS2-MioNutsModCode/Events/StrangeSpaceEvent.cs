using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Runs;
using STS2_MioNutsMod.STS2_MioNutsModCode.Extensions;
using STS2_MioNutsMod.STS2_MioNutsModCode.Potions;

namespace STS2_MioNutsMod.STS2_MioNutsModCode.Events;

public sealed class StrangeSpaceEvent : STS2_MioNutsModEvent
{
    public override string? CustomInitialPortraitPath => "events.png".EventImagePath();
    public override bool IsShared => true;
    protected override IEnumerable<DynamicVar> CanonicalVars => [];

    public override bool IsAllowed(IRunState runState)
    {
        return runState is not null && runState.Players.Count > 1;
    }

    protected override IReadOnlyList<EventOption> GenerateInitialOptions()
    {
        return
        [
            CreateSafeOption("REACH_OUT", ReachOut, [HoverTipFactory.FromPotion<StereotypePotion>()]),
            CreateSafeOption("WATCH_IMAGE", WatchImage, Array.Empty<IHoverTip>())
        ];
    }

    private async Task ReachOut()
    {
        if (Owner is null)
            return;

        await PotionCmd.TryToProcure(ModelDb.Potion<StereotypePotion>().ToMutable(), Owner);
        SetEventFinished(PageDescription("REACH_OUT_DONE"));
    }

    private async Task WatchImage()
    {
        if (Owner is null)
            return;

        var candidates = ModelDb.AllCards
            .Where(card =>
                card.MultiplayerConstraint == CardMultiplayerConstraint.MultiplayerOnly &&
                card.CanBeGeneratedInCombat &&
                card.Type is CardType.Attack or CardType.Skill or CardType.Power &&
                card.Rarity is CardRarity.Common or CardRarity.Uncommon or CardRarity.Rare)
            .DistinctBy(card => card.Id)
            .ToList();
        if (candidates.Count == 0)
        {
            SetEventFinished(PageDescription("WATCH_IMAGE_DONE"));
            return;
        }

        // Shared events use one team-wide EventModel.Rng seed; use per-player Rewards RNG
        // so each player gets an independent 3-card offer.
        var playerRng = Owner.PlayerRng.Rewards;
        var pool = candidates.OrderBy(_ => playerRng.NextInt(1_000_000)).Take(3).ToList();
        var options = pool
            .Select(card => new CardCreationResult(Owner.RunState.CreateCard(card, Owner)))
            .ToList();
        var prefs = new CardSelectorPrefs(
            L10NLookup($"{Id.Entry}.pages.INITIAL.options.WATCH_IMAGE.selectionScreenPrompt"),
            1)
        {
            Cancelable = false
        };

        var selected = (await CardSelectCmd.FromSimpleGridForRewards(
            new BlockingPlayerChoiceContext(),
            options,
            Owner,
            prefs)).FirstOrDefault();
        if (selected is not null)
            CardCmd.PreviewCardPileAdd(await CardPileCmd.Add(selected, PileType.Deck), 1.2f, CardPreviewStyle.EventLayout);

        SetEventFinished(PageDescription("WATCH_IMAGE_DONE"));
    }

    private EventOption CreateSafeOption(string optionSuffix, Func<Task>? onChosen, IEnumerable<IHoverTip> hoverTips)
    {
        var textKey = InitialOptionKey(optionSuffix);
        var title = GetOptionTitle(textKey) ?? new LocString("events", textKey + ".title");
        var description = GetOptionDescription(textKey) ?? new LocString("events", textKey + ".description");
        return new EventOption(this, onChosen, title, description, textKey, hoverTips);
    }
}

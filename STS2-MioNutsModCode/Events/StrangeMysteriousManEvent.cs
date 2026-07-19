using System.Reflection;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Gold;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.ValueProps;
namespace STS2_MioNutsMod.STS2_MioNutsModCode.Events;

public sealed class StrangeMysteriousManEvent : STS2_MioNutsModEvent
{
    private enum FirstSpecialOutcome
    {
        Heal30 = 0,
        UpgradeOne = 1,
        Potion = 2,
        Relic = 3
    }

    private enum SecondSpecialOutcome
    {
        Heal30 = 0,
        UpgradeTwoLose8 = 1,
        RemoveOneCommonOrHigher = 2,
        Potion = 3,
        TwoRelicsTwoCurses = 4
    }

    private const int FirstActIndex = 0;
    private const int ThirdActIndex = 2;
    private static readonly FieldInfo? VisitedEventIdsField =
        typeof(RunState).GetField("_visitedEventIds", BindingFlags.Instance | BindingFlags.NonPublic);

    public override bool IsShared => false;
    protected override IEnumerable<DynamicVar> CanonicalVars => [];

    private bool IsSecondMeeting => GetEncounterCount(Owner?.RunState) >= 2;

    public override LocString InitialDescription =>
        L10NLookup($"{Id.Entry}.pages.{(IsSecondMeeting ? "INITIAL_SECOND" : "INITIAL_FIRST")}.description");

    public override bool IsAllowed(IRunState runState)
    {
        if (runState is null || !runState.Players.Any(player => player.Gold >= 150))
            return false;
        if (runState.CurrentActIndex is not (FirstActIndex or ThirdActIndex))
            return false;

        int seenCount = GetEncounterCount(runState);
        if (seenCount == 0)
            return true;
        if (seenCount == 1)
            return runState.CurrentActIndex == ThirdActIndex;

        return false;
    }

    protected override IReadOnlyList<EventOption> GenerateInitialOptions()
    {
        string pageKey = IsSecondMeeting ? "INITIAL_SECOND" : "INITIAL_FIRST";
        bool canRemoveSixCards = HasEnoughRemovableCards(6);
        return
        [
            CreateSafeOption(pageKey, "OPTION_1", IsSecondMeeting ? SecondRareCard : FirstRareCard, Array.Empty<IHoverTip>()),
            CreateSafeOption(pageKey, "OPTION_2", IsSecondMeeting ? SecondSpecialCards : FirstRelic, Array.Empty<IHoverTip>()),
            CreateSafeOption(pageKey, "OPTION_3", canRemoveSixCards ? RemoveCardsWithAllGold : null, Array.Empty<IHoverTip>()),
            IsSecondMeeting
                ? CreateSecondSpecialOption(pageKey)
                : CreateFirstSpecialOption(pageKey)
        ];
    }

    private async Task FirstRareCard()
    {
        if (Owner?.Creature is null)
            return;

        await LoseHp(10m);
        await AddRandomRareCard(upgrade: false);
        SetEventFinished(PageDescription("FIRST_OPTION_1_DONE"));
        AllowSecondEncounterIfNeeded();
    }

    private async Task FirstRelic()
    {
        if (Owner?.Creature is null)
            return;

        await CreatureCmd.LoseMaxHp(new BlockingPlayerChoiceContext(), Owner.Creature, 10m, false);
        await ObtainRandomRelic(1, allowCommon: false);
        SetEventFinished(PageDescription("FIRST_OPTION_2_DONE"));
        AllowSecondEncounterIfNeeded();
    }

    private async Task RemoveCardsWithAllGold()
    {
        if (Owner is null)
            return;

        if (Owner.Gold > 0)
            await PlayerCmd.LoseGold(Owner.Gold, Owner, GoldLossType.Stolen);

        var removed = (await CardSelectCmd.FromDeckForRemoval(
            Owner,
            new CardSelectorPrefs(CardSelectorPrefs.RemoveSelectionPrompt, 6, 6))).ToList();
        if (removed.Count > 0)
            await CardPileCmd.RemoveFromDeck(removed);

        SetEventFinished(PageDescription(IsSecondMeeting ? "SECOND_OPTION_3_DONE" : "FIRST_OPTION_3_DONE"));
        AllowSecondEncounterIfNeeded();
    }

    private async Task ResolveFirstRandomSpecial(FirstSpecialOutcome outcome)
    {
        if (Owner?.Creature is null)
            return;

        switch (outcome)
        {
            case FirstSpecialOutcome.Heal30:
                await HealPercent(0.30m);
                break;
            case FirstSpecialOutcome.UpgradeOne:
                UpgradeRandomDeckCards(1);
                break;
            case FirstSpecialOutcome.Potion:
                await ObtainRandomPotion();
                break;
            default:
                await ObtainRandomRelic(1);
                break;
        }

        SetEventFinished(PageDescription("FIRST_OPTION_4_DONE"));
        AllowSecondEncounterIfNeeded();
    }

    private async Task SecondRareCard()
    {
        if (Owner?.Creature is null)
            return;

        await LoseHp(15m);
        await AddRandomRareCard(upgrade: true);
        SetEventFinished(PageDescription("SECOND_OPTION_1_DONE"));
    }

    private async Task SecondSpecialCards()
    {
        if (Owner?.Creature is null)
            return;

        await CreatureCmd.LoseMaxHp(new BlockingPlayerChoiceContext(), Owner.Creature, 20m, false);
        await AddRandomAncientCards(3);
        SetEventFinished(PageDescription("SECOND_OPTION_2_DONE"));
    }

    private async Task ResolveSecondRandomSpecial(SecondSpecialOutcome outcome)
    {
        if (Owner?.Creature is null)
            return;

        switch (outcome)
        {
            case SecondSpecialOutcome.Heal30:
                await HealPercent(0.30m);
                break;
            case SecondSpecialOutcome.UpgradeTwoLose8:
                UpgradeRandomDeckCards(2);
                await LoseHp(8m);
                break;
            case SecondSpecialOutcome.RemoveOneCommonOrHigher:
                await RemoveRandomCommonOrHigherCard();
                break;
            case SecondSpecialOutcome.Potion:
                await ObtainRandomPotion();
                break;
            default:
                await ObtainRandomRelic(2, allowCommon: true, excludeSmithingRelics: true);
                await AddRandomCursesToPile(2);
                break;
        }

        SetEventFinished(PageDescription("SECOND_OPTION_4_DONE"));
    }

    private async Task LoseHp(decimal amount)
    {
        if (Owner?.Creature is null || amount <= 0m)
            return;

        await CreatureCmd.Damage(
            new BlockingPlayerChoiceContext(),
            Owner.Creature,
            amount,
            ValueProp.Unblockable | ValueProp.Unpowered,
            null,
            null);
    }

    private async Task HealPercent(decimal percent)
    {
        if (Owner?.Creature is null || percent <= 0m)
            return;

        decimal healAmount = Math.Max(1m, Math.Ceiling(Owner.Creature.MaxHp * percent));
        await CreatureCmd.Heal(Owner.Creature, healAmount);
    }

    private async Task AddRandomRareCard(bool upgrade)
    {
        if (Owner is null)
            return;

        CardCreationOptions options = CardCreationOptions
            .ForNonCombatWithDefaultOdds(
                [Owner.Character.CardPool],
                card => card.Rarity == CardRarity.Rare)
            .WithFlags(CardCreationFlags.NoCardPoolModifications);

        var card = CardFactory.CreateForReward(Owner, 1, options).FirstOrDefault()?.Card;
        if (card is null)
            return;

        if (upgrade)
            CardCmd.Upgrade(card, CardPreviewStyle.None);

        CardCmd.PreviewCardPileAdd(await CardPileCmd.Add(card, PileType.Deck), 1.2f, CardPreviewStyle.EventLayout);
    }

    private async Task AddRandomAncientCards(int count)
    {
        if (Owner is null || count <= 0)
            return;

        var ancientCards = ModelDb.AllCards
            .Where(card => card.Rarity == CardRarity.Ancient)
            .DistinctBy(card => card.Id)
            .ToList();
        if (ancientCards.Count == 0)
            return;

        int addCount = Math.Min(count, ancientCards.Count);
        for (int i = 0; i < addCount; i++)
        {
            int index = Rng.NextInt(ancientCards.Count);
            var canonical = ancientCards[index];
            ancientCards.RemoveAt(index);
            var card = Owner.RunState.CreateCard(canonical, Owner);
            CardCmd.PreviewCardPileAdd(await CardPileCmd.Add(card, PileType.Deck), 1.2f, CardPreviewStyle.EventLayout);
        }
    }

    private async Task ObtainRandomRelic(int count, bool allowCommon = true, bool excludeSmithingRelics = false)
    {
        if (Owner is null || count <= 0)
            return;

        static bool IsSmithingRelic(RelicModel relic) => relic is Whetstone or WarPaint;
        Func<RelicModel, bool> filter = excludeSmithingRelics
            ? relic => !IsSmithingRelic(relic)
            : _ => true;

        for (int i = 0; i < count; i++)
        {
            var relic = allowCommon
                ? RelicFactory.PullNextRelicFromFront(
                        Owner,
                        RelicFactory.RollRarity(Owner),
                        filter)
                    .ToMutable()
                : RelicFactory.PullNextRelicFromFront(
                        Owner,
                        Rng.NextInt(2) == 0 ? RelicRarity.Uncommon : RelicRarity.Rare,
                        filter)
                    .ToMutable();
            await RelicCmd.Obtain(relic, Owner);
        }
    }

    private async Task ObtainRandomPotion()
    {
        if (Owner is null)
            return;

        var potion = PotionFactory
            .CreateRandomPotionOutOfCombat(Owner, Owner.RunState.Rng.CombatPotionGeneration)
            .ToMutable();
        await PotionCmd.TryToProcure(potion, Owner);
    }

    private void UpgradeRandomDeckCards(int count)
    {
        if (Owner is null || count <= 0)
            return;

        var candidates = PileType.Deck.GetPile(Owner).Cards.Where(card => card.IsUpgradable).ToList();
        for (int i = 0; i < count && candidates.Count > 0; i++)
        {
            int index = Rng.NextInt(candidates.Count);
            var card = candidates[index];
            candidates.RemoveAt(index);
            CardCmd.Upgrade(card, CardPreviewStyle.EventLayout);
        }
    }

    private async Task RemoveRandomCommonOrHigherCard()
    {
        if (Owner is null)
            return;

        var candidates = PileType.Deck.GetPile(Owner).Cards
            .Where(card =>
                card.IsRemovable &&
                card.Rarity is CardRarity.Common or CardRarity.Uncommon or CardRarity.Rare)
            .ToList();
        if (candidates.Count == 0)
            return;

        int index = Rng.NextInt(candidates.Count);
        await CardPileCmd.RemoveFromDeck([candidates[index]]);
    }

    private async Task AddRandomCursesToPile(int count)
    {
        if (Owner is null || count <= 0)
            return;

        var curses = ModelDb.AllCards
            .Where(card => card.Type == CardType.Curse)
            .DistinctBy(card => card.Id)
            .ToList();
        if (curses.Count == 0)
            return;

        var targetPile = Owner.Creature?.CombatState is null ? PileType.Deck : PileType.Hand;
        for (int i = 0; i < count; i++)
        {
            var canonical = curses[Rng.NextInt(curses.Count)];
            var created = Owner.RunState.CreateCard(canonical, Owner);
            await CardPileCmd.Add(created, targetPile);
        }
    }

    private bool HasEnoughRemovableCards(int count)
    {
        if (Owner is null || count <= 0)
            return false;

        return PileType.Deck.GetPile(Owner).Cards.Count(card => card.IsRemovable) >= count;
    }

    private int GetEncounterCount(IRunState? runState)
    {
        if (runState is null)
            return 0;

        int count = 0;
        foreach (var mapPoint in runState.MapPointHistory)
        {
            count += mapPoint.SelectMany(entry => entry.Rooms)
                .Count(room => room.ModelId == Id);
        }

        return count;
    }

    private void AllowSecondEncounterIfNeeded()
    {
        if (Owner?.RunState is not RunState runState || IsSecondMeeting)
            return;
        if (VisitedEventIdsField?.GetValue(runState) is not HashSet<ModelId> visited)
            return;

        visited.Remove(Id);
    }

    private EventOption CreateSafeOption(string pageKey, string optionSuffix, Func<Task>? onChosen, IEnumerable<IHoverTip> hoverTips)
    {
        var textKey = $"{Id.Entry}.pages.{pageKey}.options.{optionSuffix}";
        var title = GetOptionTitle(textKey) ?? new LocString("events", textKey + ".title");
        var description = GetOptionDescription(textKey) ?? new LocString("events", textKey + ".description");
        return new EventOption(this, onChosen, title, description, textKey, hoverTips);
    }

    private EventOption CreateFirstSpecialOption(string pageKey)
    {
        var outcome = (FirstSpecialOutcome)Rng.NextInt(4);
        string suffix = outcome switch
        {
            FirstSpecialOutcome.Heal30 => "OPTION_4_HEAL",
            FirstSpecialOutcome.UpgradeOne => "OPTION_4_UPGRADE",
            FirstSpecialOutcome.Potion => "OPTION_4_POTION",
            _ => "OPTION_4_RELIC"
        };

        string displayKey = $"{Id.Entry}.pages.{pageKey}.options.{suffix}";
        string textKey = $"{Id.Entry}.pages.{pageKey}.options.OPTION_4";
        var title = GetOptionTitle(displayKey) ?? new LocString("events", displayKey + ".title");
        var description = GetOptionDescription(displayKey) ?? new LocString("events", displayKey + ".description");
        return new EventOption(this, () => ResolveFirstRandomSpecial(outcome), title, description, textKey, Array.Empty<IHoverTip>());
    }

    private EventOption CreateSecondSpecialOption(string pageKey)
    {
        var outcome = (SecondSpecialOutcome)Rng.NextInt(5);
        string suffix = outcome switch
        {
            SecondSpecialOutcome.Heal30 => "OPTION_4_HEAL",
            SecondSpecialOutcome.UpgradeTwoLose8 => "OPTION_4_UPGRADE_HP",
            SecondSpecialOutcome.RemoveOneCommonOrHigher => "OPTION_4_REMOVE",
            SecondSpecialOutcome.Potion => "OPTION_4_POTION",
            _ => "OPTION_4_RELIC_CURSE"
        };

        string displayKey = $"{Id.Entry}.pages.{pageKey}.options.{suffix}";
        string textKey = $"{Id.Entry}.pages.{pageKey}.options.OPTION_4";
        var title = GetOptionTitle(displayKey) ?? new LocString("events", displayKey + ".title");
        var description = GetOptionDescription(displayKey) ?? new LocString("events", displayKey + ".description");
        return new EventOption(this, () => ResolveSecondRandomSpecial(outcome), title, description, textKey, Array.Empty<IHoverTip>());
    }
}

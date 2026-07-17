using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Gold;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Runs;
using STS2RitsuLib.Scaffolding.Content;
using STS2_MioNutsMod.STS2_MioNutsModCode.Cards;

namespace STS2_MioNutsMod.STS2_MioNutsModCode.Events;

public sealed class IronAndBeastEvent : ModEventTemplate
{
    private const int ThirdActIndex = 2;

#pragma warning disable RITSU013
    public override string? CustomInitialPortraitPath => "res://images/events/trial_started.png";
#pragma warning restore RITSU013
    public override bool IsShared => false;
    public override bool IsAllowed(IRunState runState) => runState is not null && runState.CurrentActIndex == ThirdActIndex;

    protected override IEnumerable<DynamicVar> CanonicalVars => [];

    protected override IReadOnlyList<EventOption> GenerateInitialOptions() =>
    [
        CreateSafeOption(
            "BEAST",
            ReachForBeast,
            HoverTipFactory.FromCardWithCardHoverTips<MoonlitGazeCard>(false)),
        CreateSafeOption(
            "HUMAN",
            ReachForHuman,
            HoverTipFactory.FromCardWithCardHoverTips<NonexistenceCard>(false)),
        CreateSafeOption("MOCK", MockBoth, Array.Empty<IHoverTip>())
    ];

    private async Task ReachForBeast()
    {
        if (Owner is null || Owner.Creature is null)
            return;

        var selected = (await CardSelectCmd.FromDeckForRemoval(
            player: Owner,
            prefs: new CardSelectorPrefs(CardSelectorPrefs.RemoveSelectionPrompt, 2),
            filter: _ => true)).ToList();

        if (selected.Count > 0)
            await CardPileCmd.RemoveFromDeck(selected);

        await CreatureCmd.LoseMaxHp(new BlockingPlayerChoiceContext(), Owner.Creature, 8m, false);
        await CardPileCmd.Add(Owner.RunState.CreateCard<MoonlitGazeCard>(Owner), PileType.Deck);

        SetEventFinished(PageDescription("BEAST_DONE"));
    }

    private async Task ReachForHuman()
    {
        if (Owner is null)
            return;

        if (Owner.Gold > 0)
            await PlayerCmd.LoseGold(Owner.Gold, Owner, GoldLossType.Stolen);

        await CardPileCmd.Add(Owner.RunState.CreateCard<NonexistenceCard>(Owner), PileType.Deck);
        SetEventFinished(PageDescription("HUMAN_DONE"));
    }

    private async Task MockBoth()
    {
        if (Owner is null)
            return;

        await CardPileCmd.Add(Owner.RunState.CreateCard<Normality>(Owner), PileType.Deck);
        await RelicCmd.Obtain(CreateFilteredRelic(RelicRarity.Common), Owner);
        await RelicCmd.Obtain(CreateFilteredRelic(RelicRarity.Uncommon), Owner);
        await RelicCmd.Obtain(CreateFilteredRelic(RelicRarity.Rare), Owner);
        SetEventFinished(PageDescription("MOCK_DONE"));
    }

    private RelicModel CreateFilteredRelic(RelicRarity rarity)
    {
        if (Owner is null)
            throw new InvalidOperationException("Owner is null when creating relic.");

        return RelicFactory.PullNextRelicFromFront(
            Owner,
            rarity,
            relic => relic is not Whetstone && relic is not WarPaint).ToMutable();
    }

    private EventOption CreateSafeOption(string optionSuffix, Func<Task>? onChosen, IEnumerable<IHoverTip> hoverTips)
    {
        var textKey = InitialOptionKey(optionSuffix);
        var title = GetOptionTitle(textKey) ?? new LocString("events", textKey + ".title");
        var description = GetOptionDescription(textKey) ?? new LocString("events", textKey + ".description");

        return new EventOption(this, onChosen, title, description, textKey, hoverTips);
    }

}

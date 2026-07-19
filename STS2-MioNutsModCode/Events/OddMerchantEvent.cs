using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Gold;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using STS2_MioNutsMod.STS2_MioNutsModCode.Cards;

namespace STS2_MioNutsMod.STS2_MioNutsModCode.Events;

public sealed class OddMerchantEvent : STS2_MioNutsModEvent
{
    public override bool IsAllowed(IRunState runState)
    {
        if (runState?.MapPointHistory is null)
            return false;

        return runState.MapPointHistory.Any(actHistory =>
            actHistory.Any(entry => entry.Rooms.Any(room => room.RoomType == RoomType.Shop)));
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => [new GoldVar(100)];

    protected override IReadOnlyList<EventOption> GenerateInitialOptions() =>
    [
        new EventOption(this, SellCards, InitialOptionKey("SELL"), Array.Empty<IHoverTip>()),
        new EventOption(this, TakeLoan, InitialOptionKey("LOAN"), HoverTipFactory.FromCardWithCardHoverTips<LoanCurseCard>(false))
    ];

    private async Task SellCards()
    {
        if (Owner is null)
            return;

        var selected = (await CardSelectCmd.FromDeckForRemoval(
            player: Owner,
            prefs: new CardSelectorPrefs(CardSelectorPrefs.RemoveSelectionPrompt, 2),
            filter: card => !card.IsBasicStrikeOrDefend)).ToList();

        await CardPileCmd.RemoveFromDeck(selected);

        await PlayerCmd.GainGold(100m, Owner, false);
        SetEventFinished(PageDescription("SELL_DONE"));
    }

    private async Task TakeLoan()
    {
        if (Owner is null)
            return;

        if (Owner.Gold > 0)
            await PlayerCmd.LoseGold(Owner.Gold, Owner, GoldLossType.Stolen);

        await CardPileCmd.AddCurseToDeck<LoanCurseCard>(Owner);

        SetEventFinished(PageDescription("LOAN_DONE"));
    }
}

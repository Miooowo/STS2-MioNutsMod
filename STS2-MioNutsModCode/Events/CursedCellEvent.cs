using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Runs;
using STS2_MioNutsMod.STS2_MioNutsModCode.Relics;

namespace STS2_MioNutsMod.STS2_MioNutsModCode.Events;

public sealed class CursedCellEvent : STS2_MioNutsModEvent
{
    private const int DarkHarborActIndex = 0;
    public override bool IsShared => false;
    protected override IEnumerable<DynamicVar> CanonicalVars => [];

    public override bool IsAllowed(IRunState runState)
    {
        return runState is not null && runState.CurrentActIndex == DarkHarborActIndex;
    }

    protected override IReadOnlyList<EventOption> GenerateInitialOptions()
    {
        return
        [
            CreateSafeOption(
                "BORROW_SWORD",
                BorrowSword,
                HoverTipFactory.FromRelic<CursedCellRelic>()),
            CreateSafeOption(
                "LEARN_ARCANE",
                LearnArcane,
                HoverTipFactory.FromRelic<CellRegenerationRelic>())
        ];
    }

    private async Task BorrowSword()
    {
        if (Owner is null)
            return;

        await RelicCmd.Obtain<CursedCellRelic>(Owner);
        SetEventFinished(PageDescription("BORROW_SWORD_DONE"));
    }

    private async Task LearnArcane()
    {
        if (Owner is null)
            return;

        await RelicCmd.Obtain<CellRegenerationRelic>(Owner);
        SetEventFinished(PageDescription("LEARN_ARCANE_DONE"));
    }

    private EventOption CreateSafeOption(string optionSuffix, Func<Task>? onChosen, IEnumerable<IHoverTip> hoverTips)
    {
        var textKey = InitialOptionKey(optionSuffix);
        var title = GetOptionTitle(textKey) ?? new LocString("events", textKey + ".title");
        var description = GetOptionDescription(textKey) ?? new LocString("events", textKey + ".description");
        return new EventOption(this, onChosen, title, description, textKey, hoverTips);
    }
}

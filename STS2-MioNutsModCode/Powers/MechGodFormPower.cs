using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace STS2_MioNutsMod.STS2_MioNutsModCode.Powers;

public sealed class MechGodFormPower : STS2_MioNutsModPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;
    public override bool ShouldReceiveCombatHooks => true;

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.Static(StaticHoverTip.Evoke),
        HoverTipFactory.FromPower<FocusPower>()
    ];

    public override async Task AfterOrbEvoked(PlayerChoiceContext choiceContext, OrbModel orb, IEnumerable<Creature> targets)
    {
        if (Owner?.Player is null || orb.Owner != Owner.Player)
            return;

        await PowerCmd.Apply<MechGodTemporaryFocusPower>(choiceContext, Owner, 1m, Owner, null, false);
    }
}

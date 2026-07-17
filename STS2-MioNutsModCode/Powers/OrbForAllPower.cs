using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.HoverTips;

namespace STS2_MioNutsMod.STS2_MioNutsModCode.Powers;

public sealed class OrbForAllPower : STS2_MioNutsModPower
{
    private static int MirrorSuppressionDepth;
    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [
        HoverTipFactory.Static(StaticHoverTip.Channeling),
    ];

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;
    public override bool ShouldReceiveCombatHooks => true;

    public override async Task AfterOrbChanneled(PlayerChoiceContext choiceContext, Player player, OrbModel orb)
    {
        if (Owner?.Player is null || CombatState is null)
            return;
        if (player.Creature?.CombatState != CombatState || player.Creature.Side != Owner.Side)
            return;
        if (player != Owner.Player || !player.Creature.IsAlive)
            return;
        if (MirrorSuppressionDepth > 0)
            return;

        var canonicalOrb = ModelDb.DebugOrb(orb.GetType());
        if (canonicalOrb is null)
            return;

        MirrorSuppressionDepth++;
        try
        {
            foreach (var teammate in CombatState.Players.Where(p =>
                         p != player &&
                         p.Creature.IsAlive &&
                         p.Creature.Side == Owner.Side))
            {
                var clonedOrb = canonicalOrb.ToMutable();
                await OrbCmd.Channel(choiceContext, clonedOrb, teammate);
            }
        }
        finally
        {
            MirrorSuppressionDepth--;
        }
    }
}

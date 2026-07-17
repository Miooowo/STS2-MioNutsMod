using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace STS2_MioNutsMod.STS2_MioNutsModCode.Powers;

public sealed class TeachAndLearnPower : STS2_MioNutsModPower
{
    private static int MirrorSuppressionDepth;

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;
    public override bool ShouldReceiveCombatHooks => true;

    public override async Task AfterPowerAmountChanged(PlayerChoiceContext choiceContext, PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
    {
        if (Owner?.Player is null || CombatState is null)
            return;
        if (MirrorSuppressionDepth > 0 || amount <= 0m)
            return;
        if (power.Owner != Owner)
            return;
        if (power is TeachAndLearnPower)
            return;
        if (cardSource is null || cardSource.Type != CardType.Power || cardSource.Owner != Owner.Player)
            return;

        var canonicalPower = ModelDb.DebugPower(power.GetType());
        if (canonicalPower is null)
            return;

        MirrorSuppressionDepth++;
        try
        {
            foreach (var teammate in CombatState.Players.Where(p =>
                         p != Owner.Player &&
                         p.Creature.IsAlive &&
                         p.Creature.Side == Owner.Side))
            {
                var clonedPower = canonicalPower.ToMutable();
                await PowerCmd.Apply(choiceContext, clonedPower, teammate.Creature, amount, Owner, cardSource, false);
            }
        }
        finally
        {
            MirrorSuppressionDepth--;
        }
    }
}

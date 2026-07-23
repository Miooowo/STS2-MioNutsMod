using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace STS2_MioNutsMod.STS2_MioNutsModCode.Powers;

public sealed class PurpleFormPower : STS2_MioNutsModPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;
    public override bool ShouldReceiveCombatHooks => true;

    public override async Task AfterDamageReceived(
        PlayerChoiceContext choiceContext,
        Creature target,
        DamageResult result,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource)
    {
        if (CombatState is null || !target.IsAlive || target.CombatState != CombatState)
            return;
        if (result.TotalDamage <= 0)
            return;

        await PowerCmd.Apply<DoomPower>(choiceContext, target, result.TotalDamage, Owner, cardSource, false);
        if (result.UnblockedDamage > 0 && target.IsAlive)
            await CreatureCmd.Heal(target, result.UnblockedDamage);
    }

    public override async Task BeforeSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (CombatState is null || side != Owner.Side)
            return;

        var doomedEnemies = CombatState.HittableEnemies
            .Where(enemy => enemy.GetPower<DoomPower>()?.Amount >= enemy.CurrentHp)
            .ToList();
        if (doomedEnemies.Count == 0)
            return;

        await DoomPower.DoomKill(doomedEnemies);
    }
}

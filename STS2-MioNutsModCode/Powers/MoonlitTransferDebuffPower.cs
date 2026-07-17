using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace STS2_MioNutsMod.STS2_MioNutsModCode.Powers;

public sealed class MoonlitTransferDebuffPower : STS2_MioNutsModPower
{
    private static readonly string[] NonTransferableDebuffIdPrefixes =
    [
        "FORECASTED_",
        "OMNISCIENCE_"
    ];

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;
    public override bool ShouldReceiveCombatHooks => true;

    public override async Task BeforeSideTurnEnd(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IEnumerable<Creature> creatures)
    {
        if (Owner is null || Owner.CombatState is null)
            return;

        if (!creatures.Contains(Owner))
            return;

        var debuff = Owner.Powers.FirstOrDefault(IsTransferableDebuff);
        if (debuff is null)
            return;

        var amount = debuff.Amount;
        if (amount <= 0m)
            return;

        var enemies = Owner.CombatState.HittableEnemies;
        if (enemies.Count == 0)
            return;

        await PowerCmd.Apply(choiceContext, debuff, Owner, -amount, Owner, null, true);
        foreach (var enemy in enemies)
            await PowerCmd.Apply(choiceContext, debuff.ToMutable(), enemy, amount, Owner, null, false);
    }

    private static bool IsTransferableDebuff(PowerModel power)
    {
        if (power.Type != PowerType.Debuff || power.Amount <= 0m)
            return false;

        var entry = power.Id.Entry;
        return !NonTransferableDebuffIdPrefixes.Any(prefix =>
            entry.StartsWith(prefix, StringComparison.OrdinalIgnoreCase));
    }
}

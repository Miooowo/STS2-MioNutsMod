using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace STS2_MioNutsMod.STS2_MioNutsModCode.Powers;

public sealed class StarGodFormPower : STS2_MioNutsModPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;
    public override bool ShouldReceiveCombatHooks => true;

    public override async Task AfterDamageGiven(
        PlayerChoiceContext choiceContext,
        Creature? dealer,
        DamageResult result,
        ValueProp props,
        Creature target,
        CardModel? cardSource)
    {
        if (Owner?.Player is null || dealer != Owner || result.TotalDamage <= 0)
            return;
        if (cardSource?.Owner != Owner.Player || cardSource.Type != CardType.Attack)
            return;

        await PlayerCmd.GainStars(1m, Owner.Player);
    }
}

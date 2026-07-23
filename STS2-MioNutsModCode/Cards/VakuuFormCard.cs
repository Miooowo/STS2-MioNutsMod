using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2_MioNutsMod.STS2_MioNutsModCode.Powers;

namespace STS2_MioNutsMod.STS2_MioNutsModCode.Cards;

[RegisterCard(typeof(IroncladCardPool))]
public sealed class VakuuFormCard : STS2_MioNutsModCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<VakuuFormPower>(4m)];
    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [HoverTipFactory.ForEnergy(this)];

    public VakuuFormCard() : base(3, CardType.Power, CardRarity.Rare, TargetType.Self)
    {
    }

    protected override Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (Owner?.Creature is null)
            return Task.CompletedTask;

        return PowerCmd.Apply<VakuuFormPower>(
            choiceContext,
            Owner.Creature,
            DynamicVars["VakuuFormPower"].BaseValue,
            Owner.Creature,
            this,
            false);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["VakuuFormPower"].UpgradeValueBy(3m);
    }
}

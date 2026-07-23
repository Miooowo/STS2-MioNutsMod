using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Cards;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2_MioNutsMod.STS2_MioNutsModCode.Powers;

namespace STS2_MioNutsMod.STS2_MioNutsModCode.Cards;

[RegisterCard(typeof(TokenCardPool))]
public sealed class GenshinFormCard : STS2_MioNutsModCard
{
    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
        HoverTipFactory.FromCardWithCardHoverTips<RollingBoulder>()
            .Concat(HoverTipFactory.FromCardWithCardHoverTips<GiantRock>())
            .Concat(HoverTipFactory.FromCardWithCardHoverTips<Offering>());

    public GenshinFormCard() : base(1, CardType.Power, CardRarity.Token, TargetType.Self)
    {
    }

    protected override Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (Owner?.Creature is null)
            return Task.CompletedTask;

        return PowerCmd.Apply<GenshinFormPower>(choiceContext, Owner.Creature, 1m, Owner.Creature, this, false);
    }

    protected override void OnUpgrade()
    {
    }
}

using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;

namespace STS2_MioNutsMod.STS2_MioNutsModCode.Cards;

[RegisterCard(typeof(ColorlessCardPool))]
public sealed class NoTurningCard : STS2_MioNutsModCard
{
    public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.MultiplayerOnly;
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];
    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [HoverTipFactory.FromPower<EnergyNextTurnPower>()];

    public NoTurningCard() : base(2, CardType.Skill, CardRarity.Uncommon, TargetType.AnyAlly)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (Owner?.Creature is null)
            return;
        ArgumentNullException.ThrowIfNull(cardPlay.Target, nameof(cardPlay.Target));
        if (cardPlay.Target.Player is null)
            return;

        var targetPlayer = cardPlay.Target.Player;
        int remainingHandCards = PileType.Hand.GetPile(targetPlayer).Cards.Count;
        if (remainingHandCards > 0)
        {
            await CreatureCmd.GainBlock(cardPlay.Target, remainingHandCards * 3m, ValueProp.Move, cardPlay);
            await PowerCmd.Apply<EnergyNextTurnPower>(choiceContext, Owner.Creature, remainingHandCards, Owner.Creature, this);
        }

        PlayerCmd.EndTurn(targetPlayer, canBackOut: false);
    }

    protected override void OnUpgrade()
    {
    }
}

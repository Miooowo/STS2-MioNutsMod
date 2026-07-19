using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.CardPools;
using STS2RitsuLib.Interop.AutoRegistration;

namespace STS2_MioNutsMod.STS2_MioNutsModCode.Cards;

[RegisterCard(typeof(SilentCardPool))]
public sealed class FullAssaultCard : STS2_MioNutsModCard
{
    public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.MultiplayerOnly;
    // public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];
    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [
        HoverTipFactory.FromCard<DieDieDieCard>(IsUpgraded),
    ];

    public FullAssaultCard() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.None)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (Owner?.Creature?.CombatState is null)
            return;

        foreach (var player in Owner.Creature.CombatState.Players.Where(p => p.Creature.IsAlive))
        {
            var created = Owner.Creature.CombatState.CreateCard<DieDieDieCard>(player);
            if (IsUpgraded)
                CardCmd.Upgrade(created);

            await CardPileCmd.AddGeneratedCardToCombat(created, PileType.Hand, Owner);
        }
    }

    protected override void OnUpgrade()
    {
    }
}

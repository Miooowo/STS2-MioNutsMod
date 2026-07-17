using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;

namespace STS2_MioNutsMod.STS2_MioNutsModCode.Cards;

[RegisterCard(typeof(RegentCardPool))]
public sealed class AllHandsCrashCard : STS2_MioNutsModCard
{
    public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.MultiplayerOnly;
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new CalculationBaseVar(21m),
        new ExtraDamageVar(21m),
        new CalculatedDamageVar(ValueProp.Move).WithMultiplier((card, _) =>
            card.Owner.Creature?.CombatState?.Players.Count(player => player != card.Owner && player.Creature.IsAlive) ?? 0),
    ];
    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [
        HoverTipFactory.FromCard<Debris>(),
    ];

    public AllHandsCrashCard() : base(2, CardType.Attack, CardRarity.Rare, TargetType.AllEnemies)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (Owner?.Creature?.CombatState is null)
            return;
        var combatState = Owner.Creature.CombatState;

        await DamageCmd.Attack(DynamicVars.CalculatedDamage)
            .FromCard(this, cardPlay)
            .TargetingAllOpponents(combatState)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(choiceContext);

        foreach (var player in Owner.Creature.CombatState.Players.Where(p => p.Creature.IsAlive))
        {
            int slotsToFill = CardPile.MaxCardsInHand - PileType.Hand.GetPile(player).Cards.Count;
            if (slotsToFill <= 0)
                continue;

            var debrisCards = new List<CardModel>(slotsToFill);
            for (int i = 0; i < slotsToFill; i++)
                debrisCards.Add(combatState.CreateCard<Debris>(player));

            await CardPileCmd.AddGeneratedCardsToCombat(debrisCards, PileType.Hand, Owner);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.CalculationBase.UpgradeValueBy(5m);
        DynamicVars.ExtraDamage.UpgradeValueBy(3m);
    }
}

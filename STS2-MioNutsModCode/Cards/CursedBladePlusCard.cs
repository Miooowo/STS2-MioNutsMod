using System.Linq;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;

namespace STS2_MioNutsMod.STS2_MioNutsModCode.Cards;

[RegisterCard(typeof(CurseCardPool))]
public sealed class CursedBladePlusCard : STS2_MioNutsModCard
{
    private bool _hasStunnedThisCombat;

    public override int MaxUpgradeLevel => 0;
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Innate, CardKeyword.Retain];
    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [StunIntent.GetStaticHoverTip()];
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new CalculationBaseVar(45m),
        new ExtraDamageVar(6m),
        new CalculatedDamageVar(ValueProp.Move).WithMultiplier((card, _) =>
            card.Owner.PlayerCombatState?.AllCards.Count(c =>
                !ReferenceEquals(c, card) &&
                c.Type == CardType.Curse) ?? 0)
    ];

    public CursedBladePlusCard() : base(0, CardType.Curse, CardRarity.Curse, TargetType.AnyEnemy)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Target is null)
            return;

        await DamageCmd.Attack(DynamicVars.CalculatedDamage)
            .FromCard(this, cardPlay)
            .Targeting(cardPlay.Target)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(choiceContext);

        if (_hasStunnedThisCombat)
            return;

        _hasStunnedThisCombat = true;
        await CreatureCmd.Stun(cardPlay.Target);
    }

    public override Task AfterCombatEnd(CombatRoom _)
    {
        _hasStunnedThisCombat = false;
        return Task.CompletedTask;
    }
}

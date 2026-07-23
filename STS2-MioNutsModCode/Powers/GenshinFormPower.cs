using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;

namespace STS2_MioNutsMod.STS2_MioNutsModCode.Powers;

public sealed class GenshinFormPower : STS2_MioNutsModPower
{
    private static int TransformDepth;

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;
    public override bool ShouldReceiveCombatHooks => true;

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
        HoverTipFactory.FromCardWithCardHoverTips<RollingBoulder>()
            .Concat(HoverTipFactory.FromCardWithCardHoverTips<GiantRock>())
            .Concat(HoverTipFactory.FromCardWithCardHoverTips<Offering>());

    public override async Task AfterApplied(Creature? applier, CardModel? cardSource)
    {
        await TransformOwnedCombatCards();
    }

    public override async Task AfterCurrentHpChanged(Creature creature, decimal delta)
    {
        if (Owner?.Player is null || creature != Owner || delta >= 0m || CombatState is null)
            return;

        decimal healAmount = Math.Abs(delta);
        foreach (var teammate in CombatState.Players.Where(p =>
                     p != Owner.Player &&
                     p.Creature.IsAlive &&
                     p.Creature.Side == Owner.Side))
        {
            await CreatureCmd.Heal(teammate.Creature, healAmount);
        }
    }

    private async Task TransformOwnedCombatCards()
    {
        if (Owner?.Player?.PlayerCombatState is null)
            return;

        var ownerPlayer = Owner.Player;
        var piles = new[]
        {
            PileType.Hand.GetPile(ownerPlayer),
            PileType.Draw.GetPile(ownerPlayer),
            PileType.Exhaust.GetPile(ownerPlayer)
        };

        foreach (var pile in piles)
        foreach (var card in pile.Cards.ToList())
            await TransformCardSafely(card);
    }

    private async Task TransformCardSafely(CardModel card)
    {
        // Some combat cards (especially temporary/special cards) are marked un-transformable.
        // If we don't skip them, one exception can interrupt the remaining pile traversal.
        if (!card.IsTransformable)
            return;

        try
        {
            await TransformCard(card);
        }
        catch
        {
            // Keep best-effort behavior: skip problematic cards and continue transforming others.
        }
    }

    private async Task TransformCard(CardModel card)
    {
        if (Owner?.Player is null || TransformDepth > 0 || card.Pile is null)
            return;
        var ownerPlayer = Owner.Player;
        var cardScope = card.CardScope;
        if (cardScope is null)
            return;
        if (card is RollingBoulder or GiantRock or Offering)
            return;

        CardModel? replacement = card.Type switch
        {
            CardType.Power => cardScope.CreateCard<RollingBoulder>(ownerPlayer),
            CardType.Attack => cardScope.CreateCard<GiantRock>(ownerPlayer),
            CardType.Skill => cardScope.CreateCard<Offering>(ownerPlayer),
            _ => null
        };
        if (replacement is null)
            return;

        TransformDepth++;
        try
        {
            await CardCmd.Transform(card, replacement);
        }
        finally
        {
            TransformDepth--;
        }
    }
}

using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace STS2_MioNutsMod.STS2_MioNutsModCode.Powers;

public sealed class OperationFormPower : STS2_MioNutsModPower
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

        await CardPileCmd.Draw(choiceContext, result.TotalDamage, Owner.Player);
    }

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (Owner?.Player is null || cardPlay.Card.Owner != Owner.Player || cardPlay.Card.Type != CardType.Skill)
            return;

        int discardCount = Math.Max(0, cardPlay.Card.DynamicVars.Block.IntValue);
        if (discardCount <= 0)
            return;

        var handCards = PileType.Hand.GetPile(Owner.Player).Cards.ToList();
        if (handCards.Count == 0)
            return;

        int take = Math.Min(discardCount, handCards.Count);
        var rng = Owner.Player.RunState.Rng.CombatCardGeneration;
        var selected = handCards
            .OrderBy(_ => rng.NextInt(1_000_000))
            .Take(take)
            .ToList();
        if (selected.Count > 0)
            await CardCmd.Discard(choiceContext, selected);
    }

    public override async Task AfterShuffle(PlayerChoiceContext choiceContext, Player shuffler)
    {
        if (Owner?.Player is null || CombatState is null || shuffler != Owner.Player)
            return;

        foreach (var teammate in CombatState.Players.Where(p =>
                     p.Creature.IsAlive &&
                     p.Creature.Side == Owner.Side))
        {
            await PlayerCmd.GainEnergy(3m, teammate);
            await CardPileCmd.Draw(choiceContext, 3m, teammate);
        }
    }
}

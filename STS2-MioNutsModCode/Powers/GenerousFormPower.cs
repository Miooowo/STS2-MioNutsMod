using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.ValueProps;

namespace STS2_MioNutsMod.STS2_MioNutsModCode.Powers;

public sealed class GenerousFormPower : STS2_MioNutsModPower
{
    private static int GiftDepth;

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
        if (Owner?.Player is null || dealer?.Player is null || CombatState is null)
            return;
        if (dealer == Owner || dealer.Side != Owner.Side || result.TotalDamage <= 0)
            return;
        if (cardSource?.Type != CardType.Attack || cardSource.Owner != dealer.Player)
            return;

        await ForgeCmd.Forge(result.TotalDamage, Owner.Player, this);
    }

    public override async Task AfterForge(decimal amount, Player forger, AbstractModel? source)
    {
        if (Owner?.Player is null || CombatState is null || GiftDepth > 0 || amount <= 0m)
            return;
        // Ally-damage forge uses this power as source; do not filter it out.
        // GiftDepth already prevents recursive card grants.
        if (forger != Owner.Player)
            return;

        GiftDepth++;
        try
        {
            foreach (var teammate in CombatState.Players.Where(p =>
                         p != Owner.Player &&
                         p.Creature.IsAlive &&
                         p.Creature.Side == Owner.Side))
            {
                if (teammate.Creature.CombatState is null)
                    continue;
                var card = teammate.Creature.CombatState.CreateCard<Largesse>(teammate);
                CardCmd.Upgrade(card);
                CardCmd.ApplyKeyword(card, CardKeyword.Exhaust);
                await CardPileCmd.AddGeneratedCardToCombat(card, PileType.Hand, teammate);
            }
        }
        finally
        {
            GiftDepth--;
        }
    }
}

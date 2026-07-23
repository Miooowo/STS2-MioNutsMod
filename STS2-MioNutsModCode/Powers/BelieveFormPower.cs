using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Orbs;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;

namespace STS2_MioNutsMod.STS2_MioNutsModCode.Powers;

public sealed class BelieveFormPower : STS2_MioNutsModPower
{
    private static int MirrorDepth;
    private static int RewardDepth;

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;
    public override bool ShouldReceiveCombatHooks => true;

    public override async Task AfterOrbChanneled(PlayerChoiceContext choiceContext, Player player, OrbModel orb)
    {
        if (Owner?.Player is null || CombatState is null || player != Owner.Player || MirrorDepth > 0)
            return;
        if (player.Creature.Side != Owner.Side)
            return;

        var allies = CombatState.Players
            .Where(p => p != player && p.Creature.IsAlive && p.Creature.Side == Owner.Side)
            .ToList();
        if (allies.Count == 0)
            return;

        var teammate = allies[Owner.Player.RunState.Rng.CombatCardGeneration.NextInt(allies.Count)];
        var canonicalOrb = ModelDb.DebugOrb(orb.GetType());
        if (canonicalOrb is null)
            return;

        MirrorDepth++;
        try
        {
            await OrbCmd.Channel(choiceContext, canonicalOrb.ToMutable(), teammate);
        }
        finally
        {
            MirrorDepth--;
        }
    }

    public override async Task AfterModifyingOrbPassiveTriggerCount(OrbModel orb)
    {
        if (CombatState is null || RewardDepth > 0)
            return;
        if (orb.Owner?.Creature?.CombatState != CombatState || orb.Owner.Creature.Side != Owner.Side)
            return;

        RewardDepth++;
        try
        {
            var player = orb.Owner;
            var card = player.Creature.CombatState.CreateCard<BelieveInYou>(player);
            CardCmd.Upgrade(card);
            CardCmd.ApplyKeyword(card, CardKeyword.Exhaust);
            await CardPileCmd.AddGeneratedCardToCombat(card, PileType.Hand, player);
        }
        finally
        {
            RewardDepth--;
        }
    }
}

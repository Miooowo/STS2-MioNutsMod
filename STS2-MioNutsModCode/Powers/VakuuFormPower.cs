using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.Random;

namespace STS2_MioNutsMod.STS2_MioNutsModCode.Powers;

public sealed class VakuuFormPower : STS2_MioNutsModPower
{
    private const int MaxCardsToPlay = 13;

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override bool ShouldReceiveCombatHooks => true;

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [HoverTipFactory.ForEnergy(this)];

    // Same approach as PyrePower: raise max energy so turn-start refill includes the bonus.
    public override decimal ModifyMaxEnergy(Player player, decimal amount)
    {
        if (Owner?.Player is null || player != Owner.Player)
            return amount;
        return amount + 1m;
    }

    public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (Owner?.Player is null || side != Owner.Side || !participants.Contains(Owner))
            return;

        decimal healPercent = Math.Max(1m, Amount);
        decimal healAmount = Math.Max(1m, Math.Floor(Owner.MaxHp * healPercent / 100m));
        await CreatureCmd.Heal(Owner, healAmount);
    }

    public override async Task AfterAutoPrePlayPhaseEnteredLate(PlayerChoiceContext choiceContext, Player player)
    {
        if (Owner?.Player is null || player != Owner.Player || CombatState is null)
            return;

        var combatPlayer = Owner.Player.PlayerCombatState;
        if (combatPlayer is null)
            return;

        using (CardSelectCmd.PushSelector(new VakuuCardSelector()))
        {
            int cardsPlayed = 0;
            int startTurn = combatPlayer.TurnNumber;
            for (; cardsPlayed < MaxCardsToPlay; cardsPlayed++)
            {
                if (CombatManager.Instance.IsOverOrEnding)
                    break;
                if (CombatManager.Instance.IsPlayerReadyToEndTurn(player))
                    break;
                if (combatPlayer.TurnNumber != startTurn)
                    break;

                CardModel? card = PileType.Hand.GetPile(Owner.Player).Cards.FirstOrDefault(c => c.CanPlay());
                if (card is null)
                    break;

                Creature? target = GetTarget(card, CombatState);
                await card.SpendResources();
                await CardCmd.AutoPlay(choiceContext, card, target, AutoPlayType.Default, skipXCapture: true);
            }

            if (cardsPlayed == 0)
                return;

            LocString line = cardsPlayed >= MaxCardsToPlay
                ? new LocString("relics", "WHISPERING_EARRING.warning")
                : new LocString("relics", "WHISPERING_EARRING.approval");
            TalkCmd.Play(line, Owner, VfxColor.Purple);
        }
    }

    private Creature? GetTarget(CardModel card, ICombatState combatState)
    {
        Rng combatTargets = Owner.Player!.RunState.Rng.CombatTargets;
        return card.TargetType switch
        {
            TargetType.AnyEnemy => combatState.HittableEnemies.FirstOrDefault(),
            TargetType.AnyAlly => combatTargets.NextItem(combatState.Allies.Where(c =>
                c is { IsAlive: true, IsPlayer: true } && c != Owner)),
            TargetType.AnyPlayer => Owner,
            _ => null
        };
    }
}

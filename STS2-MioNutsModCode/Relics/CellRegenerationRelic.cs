using System.Linq;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;
using STS2RitsuLib.Interop.AutoRegistration;
using MegaCrit.Sts2.Core.Models.RelicPools;

namespace STS2_MioNutsMod.STS2_MioNutsModCode.Relics;

[RegisterRelic(typeof(EventRelicPool))]
public sealed class CellRegenerationRelic : STS2_MioNutsModRelic
{
    private decimal _storedLife;
    private bool _triggeredThisTurn;

    public override RelicRarity Rarity => RelicRarity.Event;
    public override bool ShowCounter => true;
    public override int DisplayAmount => (int)Math.Floor(Math.Max(0m, StoredLife));

    private decimal StoredLife
    {
        get => _storedLife;
        set
        {
            AssertMutable();
            _storedLife = Math.Max(0m, value);
            Status = _storedLife > 0m ? RelicStatus.Active : RelicStatus.Normal;
            InvokeDisplayAmountChanged();
        }
    }

    private bool TriggeredThisTurn
    {
        get => _triggeredThisTurn;
        set
        {
            AssertMutable();
            _triggeredThisTurn = value;
        }
    }

    public override Task BeforeCombatStart()
    {
        StoredLife = 0m;
        TriggeredThisTurn = false;
        return Task.CompletedTask;
    }

    public override Task AfterCombatEnd(CombatRoom _)
    {
        StoredLife = 0m;
        TriggeredThisTurn = false;
        return Task.CompletedTask;
    }

    public override Task BeforeSideTurnStart(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IReadOnlyList<Creature> participants,
        ICombatState combatState)
    {
        if (participants.Contains(Owner.Creature))
            TriggeredThisTurn = false;
        return Task.CompletedTask;
    }

    public override Task AfterCurrentHpChanged(Creature creature, decimal delta)
    {
        if (creature != Owner.Creature || !CombatManager.Instance.IsInProgress || delta >= 0m)
            return Task.CompletedTask;

        StoredLife += -delta;
        return Task.CompletedTask;
    }

    public override async Task BeforeSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (!participants.Contains(Owner.Creature) || StoredLife <= 0m)
            return;

        StoredLife = Math.Floor(StoredLife * 0.7m);
        await Task.CompletedTask;
    }

    public override async Task AfterAttack(PlayerChoiceContext choiceContext, AttackCommand command)
    {
        if (StoredLife <= 0m)
            return;
        if (TriggeredThisTurn)
            return;
        if (command.ModelSource is not CardModel card || card.Owner != Owner || card.Type != CardType.Attack)
            return;
        if (Owner.Creature.IsDead)
            return;

        decimal oldStored = StoredLife;
        decimal newStored = Math.Floor(oldStored * 0.5m);
        decimal healAmount = oldStored - newStored;
        StoredLife = newStored;
        TriggeredThisTurn = true;
        if (healAmount <= 0m)
            return;

        Flash();
        await CreatureCmd.Heal(Owner.Creature, healAmount);
    }
}

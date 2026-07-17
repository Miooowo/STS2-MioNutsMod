using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace STS2_MioNutsMod.STS2_MioNutsModCode.Powers;

public sealed class NonexistenceAuraPower : STS2_MioNutsModPower
{
    private static readonly string[] ExcludedPowerIdPrefixes =
    [
        "FORECASTED_",
        "OMNISCIENCE_"
    ];

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;
    public override bool ShouldReceiveCombatHooks => true;

    public override decimal ModifyPowerAmountGivenMultiplicative(
        PowerModel canonicalPower,
        Creature target,
        decimal amount,
        Creature? applier,
        CardModel? cardSource)
    {
        if (Owner is null || applier != Owner || target != Owner || amount <= 0m)
            return 1m;
        if (ExcludedPowerIdPrefixes.Any(prefix =>
                canonicalPower.Id.Entry.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)))
            return 1m;

        return 2m;
    }

    public override Task AfterCardEnteredCombat(CardModel card)
    {
        ApplyPowerDiscount(card);
        return Task.CompletedTask;
    }

    public override Task AfterCardDrawn(PlayerChoiceContext choiceContext, CardModel card, bool fromHandDraw)
    {
        ApplyPowerDiscount(card);
        return Task.CompletedTask;
    }

    public override Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (Owner is null || cardPlay.Card.Type != CardType.Power)
            return Task.CompletedTask;

        return CreatureCmd.GainBlock(Owner, 8m, ValueProp.Move, cardPlay, false);
    }

    private static void ApplyPowerDiscount(CardModel card)
    {
        if (card.Type != CardType.Power || card.EnergyCost.CostsX)
            return;

        int canonical = card.EnergyCost.Canonical;
        if (canonical <= 0)
            return;

        card.EnergyCost.SetCustomBaseCost(canonical - 1);
        card.DynamicVars.RecalculateForUpgradeOrEnchant();
    }
}

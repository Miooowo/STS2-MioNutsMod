using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models.CardPools;
using STS2RitsuLib.Interop.AutoRegistration;

namespace STS2_MioNutsMod.STS2_MioNutsModCode.Cards;

[RegisterCard(typeof(NecrobinderCardPool))]
public sealed class TombRaiderGangCard : STS2_MioNutsModCard
{
    private static readonly LocString SelectionPrompt = new("cards", "STS2_MIO_NUTS_MOD_CARD_TOMB_RAIDER_GANG_CARD.selectionPrompt");

    public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.MultiplayerOnly;
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    public TombRaiderGangCard() : base(1, CardType.Skill, CardRarity.Rare, TargetType.AllAllies)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (Owner?.Creature?.CombatState is null)
            return;

        foreach (var player in Owner.Creature.CombatState.Players.Where(p => p.Creature.IsAlive))
        {
            var exhaustPile = PileType.Exhaust.GetPile(player);
            if (exhaustPile.Cards.Count == 0)
                continue;

            var selected = (await CardSelectCmd.FromCombatPile(
                choiceContext,
                exhaustPile,
                player,
                new CardSelectorPrefs(SelectionPrompt, 1),
                c => c is not TombRaiderGangCard)).FirstOrDefault();

            if (selected is not null)
                await CardPileCmd.Add(selected, PileType.Hand);
        }
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}

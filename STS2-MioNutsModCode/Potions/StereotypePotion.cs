using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Potions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Characters;
using MegaCrit.Sts2.Core.Models.PotionPools;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2_MioNutsMod.STS2_MioNutsModCode.Cards;

namespace STS2_MioNutsMod.STS2_MioNutsModCode.Potions;

[RegisterPotion(typeof(EventPotionPool))]
public sealed class StereotypePotion : STS2_MioNutsModPotion
{
    public override PotionRarity Rarity => PotionRarity.Rare;
    public override PotionUsage Usage => PotionUsage.CombatOnly;
    public override TargetType TargetType => TargetType.Self;

    protected override async Task OnUse(PlayerChoiceContext choiceContext, Creature? target)
    {
        var player = Owner;
        if (player?.Creature?.CombatState is null)
            return;

        CardModel? card = player.Character switch
        {
            Ironclad => (CardModel)player.Creature.CombatState.CreateCard<GenshinFormCard>(player),
            Silent => (CardModel)player.Creature.CombatState.CreateCard<OperationFormCard>(player),
            Regent => (CardModel)player.Creature.CombatState.CreateCard<GenerousFormCard>(player),
            Necrobinder => (CardModel)player.Creature.CombatState.CreateCard<PurpleFormCard>(player),
            Defect => (CardModel)player.Creature.CombatState.CreateCard<BelieveFormCard>(player),
            _ => null
        };
        if (card is null)
            return;

        card.SetToFreeThisTurn();
        await CardPileCmd.AddGeneratedCardToCombat(card, PileType.Hand, player);
    }
}

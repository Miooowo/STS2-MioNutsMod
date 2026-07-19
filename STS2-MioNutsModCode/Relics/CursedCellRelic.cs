using System.Linq;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.RelicPools;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2_MioNutsMod.STS2_MioNutsModCode.Cards;

namespace STS2_MioNutsMod.STS2_MioNutsModCode.Relics;

[RegisterRelic(typeof(EventRelicPool))]
public sealed class CursedCellRelic : STS2_MioNutsModRelic
{
    public override RelicRarity Rarity => RelicRarity.Event;
    public override bool HasUponPickupEffect => true;
    protected override IEnumerable<IHoverTip> AdditionalHoverTips => HoverTipFactory.FromCardWithCardHoverTips<CursedBladePlusCard>();

    public override async Task AfterObtained()
    {
        var card = Owner.RunState.CreateCard<CursedBladePlusCard>(Owner);
        CardCmd.PreviewCardPileAdd([await CardPileCmd.Add(card, PileType.Deck)], 1.2f, CardPreviewStyle.EventLayout);
    }

    public override async Task AfterDamageReceived(
        PlayerChoiceContext choiceContext,
        Creature target,
        DamageResult result,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource)
    {
        if (target != Owner.Creature || result.UnblockedDamage <= 0 || target.IsDead)
            return;

        bool hasCursedBlade = Owner.PlayerCombatState?.AllCards.Any(card => card is CursedBladePlusCard)
                              ?? Owner.Deck.Cards.Any(card => card is CursedBladePlusCard);
        if (!hasCursedBlade)
            return;

        Flash();
        await CreatureCmd.Kill(target);
    }
}

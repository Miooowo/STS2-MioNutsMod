using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.CardPools;
using STS2RitsuLib.Interop.AutoRegistration;

namespace STS2_MioNutsMod.STS2_MioNutsModCode.Cards;

[RegisterCard(typeof(TokenCardPool))]
public sealed class GenerousDonationPlusCard : STS2_MioNutsModCard
{
    protected override IEnumerable<IHoverTip> AdditionalHoverTips => HoverTipFactory.FromForge();

    public GenerousDonationPlusCard() : base(0, CardType.Skill, CardRarity.Token, TargetType.None)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (Owner is null)
            return;

        await ForgeCmd.Forge(6m, Owner, this);
    }

    protected override void OnUpgrade()
    {
    }
}

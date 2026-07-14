using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Gold;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;

namespace STS2_MioNutsMod.STS2_MioNutsModCode.Cards;

[RegisterCard(typeof(CurseCardPool))]
public sealed class LoanCurseCard : STS2_MioNutsModCard
{
    public LoanCurseCard() : base(-1, CardType.Curse, CardRarity.Curse, TargetType.None)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => [];
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Unplayable, CardKeyword.Innate];

    public override bool HasTurnEndInHandEffect => true;

    public override async Task AfterCardDrawn(PlayerChoiceContext choiceContext, CardModel card, bool fromHandDraw)
    {
        await base.AfterCardDrawn(choiceContext, card, fromHandDraw);
        if (!ReferenceEquals(card, this) || Owner is null)
            return;

        await PlayerCmd.GainGold(50m, Owner, false);
    }

    protected override async Task OnTurnEndInHand(PlayerChoiceContext choiceContext)
    {
        if (Owner is null)
            return;

        decimal loss = Math.Min(20m, Owner.Gold);
        if (loss > 0)
            await PlayerCmd.LoseGold(loss, Owner, GoldLossType.Stolen);
    }

    protected override Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay) => Task.CompletedTask;
}

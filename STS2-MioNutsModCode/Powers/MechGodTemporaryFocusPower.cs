using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2_MioNutsMod.STS2_MioNutsModCode.Cards;

namespace STS2_MioNutsMod.STS2_MioNutsModCode.Powers;

[RegisterPower]
public sealed class MechGodTemporaryFocusPower : TemporaryFocusPower
{
    public override AbstractModel OriginModel => ModelDb.Card<MechGodFormCard>();
}

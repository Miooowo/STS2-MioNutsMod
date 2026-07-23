using STS2RitsuLib.Scaffolding.Content;
using STS2_MioNutsMod.STS2_MioNutsModCode.Extensions;

namespace STS2_MioNutsMod.STS2_MioNutsModCode.Potions;

public abstract class STS2_MioNutsModPotion : ModPotionTemplate
{
    public override PotionAssetProfile AssetProfile => new(
        ImagePath: $"{GetType().Name}.png".PotionImagePath(),
        OutlinePath: $"{GetType().Name}_outline.png".PotionOutlinePath()
    );
}

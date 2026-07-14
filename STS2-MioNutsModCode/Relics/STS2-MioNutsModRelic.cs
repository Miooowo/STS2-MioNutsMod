using STS2_MioNutsMod.STS2_MioNutsModCode.Extensions;
using STS2RitsuLib.Scaffolding.Content;

namespace STS2_MioNutsMod.STS2_MioNutsModCode.Relics;

/// <summary>
/// This is the base class for your mod's relics, which is set up to load the relic's images from your mod's resources.
/// When creating a relic, right click the Relics folder and create a new file with the Custom Relic template.
/// This will generate a class that extends this one.
/// You can also just create the class manually; just make sure to inherit from this class.
/// </summary>
public abstract class STS2_MioNutsModRelic : ModRelicTemplate
{
    //STS2_MioNutsMod/images/relics
    public override RelicAssetProfile AssetProfile => new(
        IconPath: $"{GetType().Name}.png".RelicImagePath(),
        IconOutlinePath: $"{GetType().Name}_outline.png".RelicImagePath(),
        BigIconPath: $"{GetType().Name}.png".BigRelicImagePath()
    );
}
using STS2_MioNutsMod.STS2_MioNutsModCode.Extensions;
using MegaCrit.Sts2.Core.Entities.Cards;
using STS2RitsuLib.Scaffolding.Content;

namespace STS2_MioNutsMod.STS2_MioNutsModCode.Cards;

/// <summary>
/// This is the base class for your mod's cards, which is set up to load the card's images from your mod's resources.
/// When creating a card, right click the Cards folder and create a new file with the Custom Card template.
/// This will generate a class that extends this one.
/// You can also just create the class manually; just make sure to inherit from this class.
/// </summary>
public abstract class STS2_MioNutsModCard(int cost, CardType type, CardRarity rarity, TargetType target) :
    ModCardTemplate(cost, type, rarity, target)
{
    //Image size:
    //Normal art: 1000x760 (Using 500x380 should also work, it will simply be scaled.)
    //Full art: 606x852
    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"{GetType().Name}.png".CardImagePath(),
        BetaPortraitPath: $"beta/{GetType().Name}.png".CardImagePath()
    );
}
using Godot;

namespace STS2_MioNutsMod.STS2_MioNutsModCode.Extensions;

//Mostly utilities to get asset paths.
public static class StringExtensions
{
    private static string JoinRes(params string[] parts)
    {
        return Path.Join(parts).Replace("\\", "/");
    }

    public static string ImagePath(this string path)
    {
        return JoinRes(MainFile.ResPath, "images", path);
    }

    public static string CardImagePath(this string path)
    {
        path = JoinRes(MainFile.ResPath, "images", "card_portraits", path);
        if (ResourceLoader.Exists(path)) return path;
        
        MainFile.Logger.Info("Could not find card image path: " + path);
        return JoinRes(MainFile.ResPath, "images", "card_portraits", "card.png");
    }

    public static string BigCardImagePath(this string path)
    {
        path = JoinRes(MainFile.ResPath, "images", "card_portraits", "big", path);
        if (ResourceLoader.Exists(path)) return path;
        
        MainFile.Logger.Info("Could not find big card image path: " + path);
        return JoinRes(MainFile.ResPath, "images", "card_portraits", "big", "card.png");
    }

    public static string PowerImagePath(this string path)
    {
        path = JoinRes(MainFile.ResPath, "images", "powers", path);
        if (ResourceLoader.Exists(path)) return path;
        
        MainFile.Logger.Info("Could not find power image path: " + path);
        return JoinRes(MainFile.ResPath, "images", "powers", "power.png");
    }

    public static string BigPowerImagePath(this string path)
    {
        path = JoinRes(MainFile.ResPath, "images", "powers", "big", path);
        if (ResourceLoader.Exists(path)) return path;
        
        MainFile.Logger.Info("Could not find big power image path: " + path);
        return JoinRes(MainFile.ResPath, "images", "powers", "big", "power.png");
    }

    public static string RelicImagePath(this string path)
    {
        path = JoinRes(MainFile.ResPath, "images", "relics", path);
        if (ResourceLoader.Exists(path)) return path;
        
        MainFile.Logger.Info("Could not find relic image path: " + path);
        return JoinRes(MainFile.ResPath, "images", "relics", "relic.png");
    }

    public static string BigRelicImagePath(this string path)
    {
        path = JoinRes(MainFile.ResPath, "images", "relics", "big", path);
        if (ResourceLoader.Exists(path)) return path;
        
        MainFile.Logger.Info("Could not find big relic image path: " + path);
        return JoinRes(MainFile.ResPath, "images", "relics", "big", "relic.png");
    }

    public static string CharacterUiPath(this string path)
    {
        return JoinRes(MainFile.ResPath, "images", "charui", path);
    }

    public static string EventImagePath(this string path)
    {
        path = JoinRes(MainFile.ResPath, "images", "events", path);
        if (ResourceLoader.Exists(path)) return path;

        MainFile.Logger.Info("Could not find event image path: " + path);
        return JoinRes(MainFile.ResPath, "images", "events", "events.png");
    }

    public static string PotionImagePath(this string path)
    {
        path = JoinRes(MainFile.ResPath, "images", "potions", path);
        if (ResourceLoader.Exists(path)) return path;

        MainFile.Logger.Info("Could not find potion image path: " + path);
        return "res://images/potions/energy_potion.png";
    }

    public static string PotionOutlinePath(this string path)
    {
        path = JoinRes(MainFile.ResPath, "images", "potions", path);
        if (ResourceLoader.Exists(path)) return path;

        MainFile.Logger.Info("Could not find potion outline path: " + path);
        return "res://images/potions/energy_potion_outline.png";
    }
}
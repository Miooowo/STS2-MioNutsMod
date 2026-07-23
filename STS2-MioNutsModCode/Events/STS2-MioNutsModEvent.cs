using System.Text.RegularExpressions;
using Godot;
using STS2RitsuLib.Scaffolding.Content;

namespace STS2_MioNutsMod.STS2_MioNutsModCode.Events;

public abstract class STS2_MioNutsModEvent : ModEventTemplate
{
    private static readonly Regex PascalBoundary = new("(?<!^)([A-Z])", RegexOptions.Compiled);
    private static readonly string[] PortraitExtensions = [".png", ".jpg", ".webp"];
    private static string JoinRes(params string[] parts) => Path.Join(parts).Replace("\\", "/");

    public override string? CustomInitialPortraitPath => ResolvePortraitPath();

    private string ResolvePortraitPath()
    {
        string baseName = GetType().Name;
        if (baseName.EndsWith("Event", StringComparison.Ordinal))
            baseName = baseName[..^"Event".Length];

        string snakeBase = PascalBoundary.Replace(baseName, "_$1").ToLowerInvariant() + "_event";
        foreach (var ext in PortraitExtensions)
        {
            string path = JoinRes(MainFile.ResPath, "images", "events", snakeBase + ext);
            if (ResourceLoader.Exists(path))
                return path;
        }

        MainFile.Logger.Info("Could not find event image path for event: " + GetType().Name);
        return JoinRes(MainFile.ResPath, "images", "events", "events.png");
    }
}

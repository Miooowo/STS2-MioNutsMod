using System.Reflection;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Modding;
using STS2RitsuLib;
using STS2RitsuLib.Interop;
using STS2_MioNutsMod.STS2_MioNutsModCode.Events;

namespace STS2_MioNutsMod.STS2_MioNutsModCode;

[ModInitializer(nameof(Initialize))]
public partial class MainFile : Node
{
    public const string ModId = "STS2_MioNutsMod"; //Used for resource filepath
    public const string ResPath = $"res://{ModId}";

    public static MegaCrit.Sts2.Core.Logging.Logger Logger { get; } = new(ModId, MegaCrit.Sts2.Core.Logging.LogType.Generic);

    public static void Initialize()
    {
        //If you want to use scripts defined in your mod for Godot scenes, uncomment the following line.
        //Godot.Bridge.ScriptManagerBridge.LookupScriptsInAssembly(Assembly.GetExecutingAssembly());
        
        Harmony harmony = new(ModId);

        harmony.PatchAll();

        var assembly = Assembly.GetExecutingAssembly();
        RitsuLibFramework.EnsureGodotScriptsRegistered(assembly, Logger);
        ModTypeDiscoveryHub.RegisterModAssembly(ModId, assembly);

        RitsuLibFramework.CreateContentPack(ModId)
            .SharedEvent<OddMerchantEvent>()
            .Apply();
    }
}

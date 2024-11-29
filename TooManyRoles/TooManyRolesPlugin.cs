using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using Reactor;
using Reactor.Utilities;

namespace TooManyRoles;

//[BepInDependency(Submerged.GUID, BepInDependency.DependencyFlags.SoftDependency)]
[BepInPlugin(Id, "TooManyRoles", "1.0.0")]
[BepInProcess("Among Us.exe")]
[BepInDependency(ReactorPlugin.Id)]
public partial class TooManyRolesPlugin : BasePlugin {
    public const string Id = "TooManyRoles";

    public Harmony Harmony { get; } = new(Id);

    public static ManualLogSource logger;

    public override void Load()
    {
        logger = Log;
        
        SettingsHandler.Instance = new SettingsHandler();
        SettingsHandler.Load();

        Harmony.PatchAll();
    }
}

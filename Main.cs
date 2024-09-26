using Alpalis.AdminManager.Models;
using Alpalis.UtilityServices.API;
using Cysharp.Threading.Tasks;
using HarmonyLib;
using Microsoft.Extensions.Logging;
using OpenMod.API.Permissions;
using OpenMod.API.Plugins;
using OpenMod.Unturned.Plugins;
using System;

[assembly:
    PluginMetadata("Alpalis.AdminManager",
        DisplayName = "Alpalis AdminManager Plugin",
        Description = "Manage your server better than before.",
        Author = "Pandetthe",
        Website = "https://alpalis.github.io/docs/plugins/adminmanager/about")]

namespace Alpalis.AdminManager;

public sealed class Main(
    ILogger<Main> logger,
    IConfigurationManager configurationManager,
    IPermissionRegistry permissionRegistry,
    IServiceProvider serviceProvider) : OpenModUnturnedPlugin(serviceProvider)
{
    #region Member variables
    private readonly ILogger<Main> m_Logger = logger;

    private readonly IConfigurationManager m_ConfigurationManager = configurationManager;

    private readonly IPermissionRegistry m_PermissionRegistry = permissionRegistry;

    private readonly Harmony m_Harmony = new("alpalis.adminmanager");
    #endregion Member variables

    protected override async UniTask OnLoadAsync()
    {
        m_Harmony.PatchAll();

        m_PermissionRegistry.RegisterPermission(this, "chatoverride", "Allows to write on chat even when chat is disabled or you are muted.");
        m_PermissionRegistry.RegisterPermission(this, "freecam", "Allows to enable free cam.");
        m_PermissionRegistry.RegisterPermission(this, "specstats", "Allows to enable spectating stats.");
        m_PermissionRegistry.RegisterPermission(this, "workzone", "Allows to enable workzone.");

        await m_ConfigurationManager.LoadConfigAsync<Config>(this);

        m_Logger.LogInformation("Plugin started successfully!");
    }

    protected override UniTask OnUnloadAsync()
    {
        m_Harmony.UnpatchAll();
        m_Logger.LogInformation("Plugin disabled successfully!");
        return UniTask.CompletedTask;
    }
}

using Alpalis.AdminManager.Models;
using Alpalis.UtilityServices.API;
using Cysharp.Threading.Tasks;
using HarmonyLib;
using Microsoft.Extensions.Logging;
using OpenMod.API.Permissions;
using OpenMod.API.Plugins;
using OpenMod.Unturned.Plugins;
using System;

#region NuGet Assembly Data
[assembly:
    PluginMetadata("Alpalis.AdminManager", Author = "Pandetthe",
        DisplayName = "Alpalis AdminManager Plugin",
        Website = "https://alpalis.eu")]
#endregion Nuget Assembly Data

namespace Alpalis.AdminManager
{
    public class Main : OpenModUnturnedPlugin
    {
        #region Member Variables
        private readonly ILogger<Main> m_Logger;
        private readonly IConfigurationManager m_ConfigurationManager;
        private readonly IPermissionRegistry m_PermissionRegistry;
        private readonly Harmony m_Harmony;
        #endregion Member Variables

        #region Class Constructor
        public Main(
            ILogger<Main> logger,
            IConfigurationManager configurationManager,
            IPermissionRegistry permissionRegistry,
            IServiceProvider serviceProvider) : base(serviceProvider)
        {
            m_Logger = logger;
            m_ConfigurationManager = configurationManager;
            m_PermissionRegistry = permissionRegistry;
            m_Harmony = new("alpalis.adminmanager");
        }
        #endregion Class Constructor

        protected override async UniTask OnLoadAsync()
        {
            // Harmony patches
            m_Harmony.PatchAll();

            // Permission registration
            m_PermissionRegistry.RegisterPermission(this, "chatoverride", "Allows to write on chat when is disabled or is muted.");

            // Configuration load
            await m_ConfigurationManager.LoadConfig(this, new Config());

            // Plugin load logging
            m_Logger.LogInformation("Plugin started successfully!");
        }

        protected override async UniTask OnUnloadAsync()
        {
            // Harmony patches
            m_Harmony.UnpatchAll();

            // Plugin unload logging
            m_Logger.LogInformation("Plugin disabled successfully!");
        }
    }
}

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
    PluginMetadata("Alpalis.AdminManager", Author = "Pandetthe",
        DisplayName = "Alpalis AdminManager Plugin",
        Website = "https://alpalis.github.io/docs/")]

namespace Alpalis.AdminManager
{
    public class Main : OpenModUnturnedPlugin
    {
        private readonly ILogger<Main> m_Logger;
        private readonly IConfigurationManager m_ConfigurationManager;
        private readonly IPermissionRegistry m_PermissionRegistry;
        private readonly Harmony m_Harmony;

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

        protected override async UniTask OnLoadAsync()
        {
            m_Harmony.PatchAll();

            m_PermissionRegistry.RegisterPermission(this, "chatoverride", "Allows to write on chat even when chat is disabled or you are muted.");

            await m_ConfigurationManager.LoadConfigAsync<Config>(this);

            m_Logger.LogInformation("Plugin started successfully!");
        }

        protected override async UniTask OnUnloadAsync()
        {
            m_Harmony.UnpatchAll();

            m_Logger.LogInformation("Plugin disabled successfully!");
        }
    }
}

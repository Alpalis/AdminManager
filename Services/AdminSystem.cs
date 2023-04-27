using Alpalis.AdminManager.API;
using Alpalis.AdminManager.Models;
using Alpalis.UtilityServices.API;
using Alpalis.UtilityServices.API.Events;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenMod.API.Commands;
using OpenMod.API.Eventing;
using OpenMod.API.Ioc;
using OpenMod.API.Permissions;
using OpenMod.API.Plugins;
using OpenMod.API.Prioritization;
using OpenMod.Unturned.Users;
using SDG.Unturned;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Alpalis.AdminManager.Services
{
    [ServiceImplementation(Lifetime = ServiceLifetime.Singleton, Priority = Priority.Normal)]
    public class AdminSystem : IAdminSystem
    {
        private readonly IGodSystem m_GodSystem;
        private readonly IVanishSystem m_VanishSystem;
        private readonly IConfigurationManager m_ConfigurationManager;
        private readonly Main m_Plugin;
        private readonly ILogger<AdminSystem> m_Logger;
        private readonly IEventBus m_EventBus;
        private readonly IPermissionChecker m_PermissionChecker;

        public AdminSystem(
            IGodSystem godSystem,
            IVanishSystem vanishSystem,
            IConfigurationManager configurationManager,
            IPluginAccessor<Main> plugin,
            ILogger<AdminSystem> logger,
            IEventBus eventBus,
            IPermissionChecker permissionChecker)
        {
            m_GodSystem = godSystem;
            m_VanishSystem = vanishSystem;
            m_ConfigurationManager = configurationManager;
            m_Plugin = plugin.Instance!;
            m_Logger = logger;
            m_EventBus = eventBus;
            AdminModes = new();
            m_PermissionChecker = permissionChecker;
        }

        private HashSet<ulong> AdminModes { get; set; }

        public async Task<bool> ToggleAdminMode(UnturnedUser user)
        {
            Config config = m_ConfigurationManager.GetConfig<Config>(m_Plugin);
            if (config.DisableAdminMode)
                throw new Exception("Adminmode is disabled!");
            SteamPlayer sPlayer = user.Player.SteamPlayer;
            CSteamID steamID = sPlayer.playerID.steamID;
            bool adminMode = IsInAdminMode(steamID);
            SwitchAdminModeEvent @event = new(sPlayer, !adminMode);
            m_EventBus.EmitAsync(m_Plugin, this, @event);
            sPlayer.player.look.sendFreecamAllowed(!adminMode && await m_PermissionChecker.CheckPermissionAsync(user, "Alpalis.AdminManager:freecam") == PermissionGrantResult.Grant);
            sPlayer.player.look.sendSpecStatsAllowed(!adminMode && await m_PermissionChecker.CheckPermissionAsync(user, "Alpalis.AdminManager:specstats") == PermissionGrantResult.Grant);
            sPlayer.player.look.sendWorkzoneAllowed(!adminMode && await m_PermissionChecker.CheckPermissionAsync(user, "Alpalis.AdminManager:workzone") == PermissionGrantResult.Grant);
            if (adminMode)
            {
                m_Logger.LogDebug(string.Format("The player {0} ({1}) disabled adminmode",
                    sPlayer.playerID.characterName, steamID));
                m_GodSystem.DisableGodMode(sPlayer);
                m_VanishSystem.DisableVanishMode(sPlayer);
                AdminModes.Remove(steamID.m_SteamID);
                return false;
            }
            m_Logger.LogDebug(string.Format("The player {0} ({1}) enabled adminmode",
                sPlayer.playerID.characterName, steamID));
            AdminModes.Add(steamID.m_SteamID);
            return true;
        }

        public bool IsInAdminMode(CSteamID steamID)
        {
            Config config = m_ConfigurationManager.GetConfig<Config>(m_Plugin);
            return config.DisableAdminMode || AdminModes.Contains(steamID.m_SteamID);
        }

        public bool IsInAdminMode(ICommandActor actor)
        {
            Config config = m_ConfigurationManager.GetConfig<Config>(m_Plugin);
            return config.DisableAdminMode || actor.GetType() != typeof(UnturnedUser) || IsInAdminMode(((UnturnedUser)actor).SteamId);
        }

        public bool IsAdminModeDisabled()
        {
            Config config = m_ConfigurationManager.GetConfig<Config>(m_Plugin);
            return config.DisableAdminMode;
        }
    }
}

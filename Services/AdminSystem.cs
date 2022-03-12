using Alpalis.AdminManager.API;
using Alpalis.AdminManager.Models;
using Alpalis.UtilityServices.API;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenMod.API.Commands;
using OpenMod.API.Ioc;
using OpenMod.API.Plugins;
using OpenMod.API.Prioritization;
using OpenMod.Unturned.Users;
using SDG.Unturned;
using Steamworks;
using System.Collections.Generic;

namespace Alpalis.AdminManager.Services
{
    [ServiceImplementation(Lifetime = ServiceLifetime.Singleton, Priority = Priority.Normal)]
    public class AdminSystem : IAdminSystem
    {
        #region Member Variables
        private readonly IGodSystem m_GodSystem;
        private readonly IVanishSystem m_VanishSystem;
        private readonly IUIManager m_UIManager;
        private readonly IConfigurationManager m_ConfigurationManager;
        private readonly Main m_Plugin;
        private readonly ILogger<AdminSystem> m_Logger;
        #endregion Member Variables

        #region Class Constructor
        public AdminSystem(
            IGodSystem godSystem,
            IVanishSystem vanishSystem,
            IUIManager uiManager,
            IConfigurationManager configurationManager,
            IPluginAccessor<Main> plugin,
            ILogger<AdminSystem> logger)
        {
            m_GodSystem = godSystem;
            m_VanishSystem = vanishSystem;
            m_UIManager = uiManager;
            m_ConfigurationManager = configurationManager;
            m_Plugin = plugin.Instance!;
            m_Logger = logger;
            AdminModes = new();
        }
        #endregion Class Constructor

        private HashSet<string> AdminModes { get; set; }

        public bool ToggleAdminMode(SteamPlayer sPlayer)
        {
            CSteamID steamID = sPlayer.playerID.steamID;
            if (IsInAdminMode(steamID))
            {
                m_Logger.LogDebug(string.Format("The player {0} ({1}) disabled AdminMode.",
                    sPlayer.playerID.characterName, steamID));
                m_GodSystem.DisableGodMode(sPlayer);
                m_VanishSystem.DisableVanishMode(sPlayer);
                m_UIManager.StopSideUI(sPlayer, m_ConfigurationManager.GetConfig<Config>(m_Plugin).AdminUIID);
                AdminModes.Remove(steamID.ToString());
                return false;
            }
            m_Logger.LogDebug(string.Format("The player {0} ({1}) enabled AdminMode.",
                sPlayer.playerID.characterName, steamID));
            m_UIManager.RunSideUI(sPlayer, m_ConfigurationManager.GetConfig<Config>(m_Plugin).AdminUIID,
                m_ConfigurationManager.GetConfig<Config>(m_Plugin).AdminUIKey);
            AdminModes.Add(steamID.ToString());
            return true;
        }

        public bool IsInAdminMode(CSteamID steamID)
        {
            if (AdminModes.Contains(steamID.ToString()))
                return true;
            return false;
        }

        public bool IsInAdminMode(ICommandActor actor)
        {
            if (actor.GetType() != typeof(UnturnedUser)) return true;
            if (IsInAdminMode(((UnturnedUser)actor).SteamId)) return true;
            return false;
        }
    }
}

using Alpalis.AdminManager.API;
using Alpalis.AdminManager.Models;
using Alpalis.UtilityServices.API;
using Cysharp.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenMod.API.Ioc;
using OpenMod.API.Plugins;
using OpenMod.API.Prioritization;
using SDG.Unturned;
using Steamworks;
using System.Collections.Generic;

namespace Alpalis.AdminManager.Services
{
    [ServiceImplementation(Lifetime = ServiceLifetime.Singleton, Priority = Priority.Normal)]
    public class GodSystem : IGodSystem
    {
        #region Member Variables
        private readonly IUIManager m_UIManager;
        private readonly IConfigurationManager m_ConfigurationManager;
        private readonly Main m_Plugin;
        private readonly ILogger<GodSystem> m_Logger;
        #endregion Member Variables

        #region Class Constructor
        public GodSystem(
            IUIManager uiManager,
            IConfigurationManager configurationManager,
            IPluginAccessor<Main> plugin,
            ILogger<GodSystem> logger)
        {
            m_UIManager = uiManager;
            m_ConfigurationManager = configurationManager;
            m_Plugin = plugin.Instance!;
            m_Logger = logger;
            GodModes = new();
        }
        #endregion Class Constructor

        private HashSet<string> GodModes { get; set; }

        public async UniTask EnableGodMode(SteamPlayer sPlayer)
        {
            CSteamID steamID = sPlayer.playerID.steamID;
            if (IsInGodMode(steamID)) return;
            m_Logger.LogDebug(string.Format("The player {0} ({1}) enabled GodMode.",
                sPlayer.playerID.characterName, steamID));
            GodModes.Add(steamID.ToString());
            m_UIManager.RunSideUI(sPlayer, m_ConfigurationManager.GetConfig<Config>(m_Plugin).GodUIID,
                m_ConfigurationManager.GetConfig<Config>(m_Plugin).GodUIKey);
        }

        public async UniTask DisableGodMode(SteamPlayer sPlayer)
        {
            CSteamID steamID = sPlayer.playerID.steamID;
            if (!IsInGodMode(steamID)) return;
            m_Logger.LogDebug(string.Format("The player {0} ({1}) disabled GodMode.",
                sPlayer.playerID.characterName, steamID));
            GodModes.Remove(steamID.ToString());
            m_UIManager.StopSideUI(sPlayer, m_ConfigurationManager.GetConfig<Config>(m_Plugin).GodUIID);
        }

        public bool IsInGodMode(CSteamID steamID)
        {
            if (GodModes.Contains(steamID.ToString()))
                return true;
            return false;
        }
    }
}

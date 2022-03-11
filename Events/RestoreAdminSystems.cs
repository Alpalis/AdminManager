using Alpalis.AdminManager.API;
using Alpalis.AdminManager.Models;
using Alpalis.UtilityServices.API;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using OpenMod.API.Eventing;
using OpenMod.Core.Eventing;
using OpenMod.UnityEngine.Extensions;
using OpenMod.Unturned.Players.Connections.Events;
using SDG.Unturned;
using Steamworks;
using System.Drawing;
using System.Threading.Tasks;

namespace Alpalis.AdminManager.Events
{
    public class RestoreAdminSystems : IEventListener<UnturnedPlayerConnectedEvent>
    {
        #region Member Variables
        private readonly IAdminSystem m_AdminSystem;
        private readonly IVanishSystem m_VanishSystem;
        private readonly IGodSystem m_GodSystem;
        private readonly IUIManager m_UIManager;
        private readonly IConfigurationManager m_ConfigurationManager;
        private readonly Main m_Plugin;
        private readonly IStringLocalizer m_StringLocalizer;
        private readonly ILogger<RestoreAdminSystems> m_Logger;
        #endregion Member Variables

        #region Class Constructor
        public RestoreAdminSystems(
            IAdminSystem adminSystem,
            IVanishSystem vanishSystem,
            IGodSystem godSystem,
            IUIManager uiManager,
            IConfigurationManager configurationManager,
            Main plugin,
            IStringLocalizer stringLocalizer,
            ILogger<RestoreAdminSystems> logger)
        {
            m_AdminSystem = adminSystem;
            m_VanishSystem = vanishSystem;
            m_GodSystem = godSystem;
            m_UIManager = uiManager;
            m_ConfigurationManager = configurationManager;
            m_Plugin = plugin;
            m_StringLocalizer = stringLocalizer;
            m_Logger = logger;
        }
        #endregion Class Constructor

        [EventListener(Priority = EventListenerPriority.Normal)]
        public async Task HandleEventAsync(object? sender, UnturnedPlayerConnectedEvent @event)
        {
            SteamPlayer sPlayer = @event.Player.SteamPlayer;
            CSteamID steamID = @event.Player.SteamId;
            if (m_AdminSystem.IsInAdminMode(steamID))
            {
                m_Logger.LogDebug(string.Format("AdminMode has been restored for the player {0} ({1}).",
                    sPlayer.playerID.characterName, steamID));
                m_UIManager.RunSideUI(sPlayer, m_ConfigurationManager.GetConfig<Config>(m_Plugin).AdminUIID,
                    m_ConfigurationManager.GetConfig<Config>(m_Plugin).AdminUIKey);
                ChatManager.serverSendMessage(m_StringLocalizer["adminmode:recover"],
                    Color.DarkRed.ToUnityColor(),
                    null, sPlayer, EChatMode.SAY, null, true);
            }

            if (m_VanishSystem.IsInVanishMode(steamID))
            {
                m_Logger.LogDebug(string.Format("VanishMode has been restored for the player {0} ({1}).",
                    sPlayer.playerID.characterName, steamID));
                m_UIManager.RunSideUI(sPlayer, m_ConfigurationManager.GetConfig<Config>(m_Plugin).VanishUIID,
                    m_ConfigurationManager.GetConfig<Config>(m_Plugin).VanishUIKey);
                sPlayer.player.movement.canAddSimulationResultsToUpdates = false;
                ChatManager.serverSendMessage(m_StringLocalizer["vanish_mode:recover"],
                    Color.DarkRed.ToUnityColor(),
                    null, sPlayer, EChatMode.SAY, null, true);
            }

            if (m_GodSystem.IsInGodMode(steamID))
            {
                m_Logger.LogDebug(string.Format("GodMode has been restored for the player {0} ({1}).",
                    sPlayer.playerID.characterName, steamID));
                m_UIManager.RunSideUI(sPlayer, m_ConfigurationManager.GetConfig<Config>(m_Plugin).GodUIID,
                    m_ConfigurationManager.GetConfig<Config>(m_Plugin).GodUIKey);
                ChatManager.serverSendMessage(m_StringLocalizer["god_mode:recover"],
                    Color.DarkRed.ToUnityColor(),
                    null, sPlayer, EChatMode.SAY, null, true);
            }
            return;
        }
    }
}

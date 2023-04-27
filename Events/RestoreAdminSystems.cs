using Alpalis.AdminManager.API;
using Alpalis.AdminManager.Models;
using Alpalis.UtilityServices.API;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using OpenMod.API.Eventing;
using OpenMod.API.Permissions;
using OpenMod.Core.Eventing;
using OpenMod.UnityEngine.Extensions;
using OpenMod.Unturned.Players.Connections.Events;
using OpenMod.Unturned.Users.Events;
using SDG.Unturned;
using Steamworks;
using System.Drawing;
using System.Threading.Tasks;

namespace Alpalis.AdminManager.Events
{
    public class RestoreAdminSystems : IEventListener<UnturnedUserConnectedEvent>
    {
        private readonly IAdminSystem m_AdminSystem;
        private readonly IVanishSystem m_VanishSystem;
        private readonly IGodSystem m_GodSystem;
        private readonly IFlySystem m_FlySystem;
        private readonly IStringLocalizer m_StringLocalizer;
        private readonly ILogger<RestoreAdminSystems> m_Logger;
        private readonly IPermissionChecker m_PermissionChecker;

        public RestoreAdminSystems(
            IAdminSystem adminSystem,
            IVanishSystem vanishSystem,
            IGodSystem godSystem,
            IFlySystem flySystem,
            IStringLocalizer stringLocalizer,
            ILogger<RestoreAdminSystems> logger,
            IPermissionChecker permissionChecker)
        {
            m_AdminSystem = adminSystem;
            m_VanishSystem = vanishSystem;
            m_GodSystem = godSystem;
            m_FlySystem = flySystem;
            m_StringLocalizer = stringLocalizer;
            m_Logger = logger;
            m_PermissionChecker = permissionChecker;
        }

        [EventListener(Priority = EventListenerPriority.Normal)]
        public async Task HandleEventAsync(object? sender, UnturnedUserConnectedEvent @event)
        {
            SteamPlayer sPlayer = @event.User.Player.SteamPlayer;
            CSteamID steamID = @event.User.Player.SteamId;
            bool inAdminMode = m_AdminSystem.IsInAdminMode(steamID);

            if (inAdminMode && !m_AdminSystem.IsAdminModeDisabled())
            {
                m_Logger.LogDebug(string.Format("Adminmode has been restored for the player {0} ({1})",
                    sPlayer.playerID.characterName, steamID));
                ChatManager.serverSendMessage(string.Format("{0}{1}", m_StringLocalizer["modes:prefix"],
                    m_StringLocalizer["modes:recover:adminmode"]),
                    Color.DarkRed.ToUnityColor(),
                    null, sPlayer, EChatMode.SAY, null, true);
            }

            sPlayer.player.look.sendFreecamAllowed(inAdminMode && await m_PermissionChecker.CheckPermissionAsync(@event.User, "Alpalis.AdminManager:freecam") == PermissionGrantResult.Grant);
            sPlayer.player.look.sendSpecStatsAllowed(inAdminMode && await m_PermissionChecker.CheckPermissionAsync(@event.User, "Alpalis.AdminManager:specstats") == PermissionGrantResult.Grant);
            sPlayer.player.look.sendWorkzoneAllowed(inAdminMode && await m_PermissionChecker.CheckPermissionAsync(@event.User, "Alpalis.AdminManager:workzone") == PermissionGrantResult.Grant);

            if (m_VanishSystem.IsInVanishMode(steamID))
            {
                m_Logger.LogDebug(string.Format("Vanishmode has been restored for the player {0} ({1})",
                    sPlayer.playerID.characterName, steamID));
                sPlayer.player.movement.canAddSimulationResultsToUpdates = false;
                ChatManager.serverSendMessage(string.Format("{0}{1}", m_StringLocalizer["modes:prefix"],
                    m_StringLocalizer["modes:recover:vanishmode"]),
                    Color.DarkRed.ToUnityColor(),
                    null, sPlayer, EChatMode.SAY, null, true);
            }

            if (m_GodSystem.IsInGodMode(steamID))
            {
                m_Logger.LogDebug(string.Format("Godmode has been restored for the player {0} ({1})",
                    sPlayer.playerID.characterName, steamID));
                ChatManager.serverSendMessage(string.Format("{0}{1}", m_StringLocalizer["modes:prefix"],
                    m_StringLocalizer["modes:recover:godmode"]),
                    Color.DarkRed.ToUnityColor(),
                    null, sPlayer, EChatMode.SAY, null, true);
            }

            if (m_FlySystem.IsInFlyMode(steamID))
            {
                m_Logger.LogDebug(string.Format("Flymode has been restored for the player {0} ({1})",
                    sPlayer.playerID.characterName, steamID));
                sPlayer.player.movement.sendPluginGravityMultiplier(0);
                ChatManager.serverSendMessage(string.Format("{0}{1}", m_StringLocalizer["modes:prefix"],
                    m_StringLocalizer["modes:recover:flymode"]),
                    Color.DarkRed.ToUnityColor(),
                    null, sPlayer, EChatMode.SAY, null, true);
            }
        }
    }
}

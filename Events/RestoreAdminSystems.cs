﻿using Alpalis.AdminManager.API;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using OpenMod.API.Eventing;
using OpenMod.API.Permissions;
using OpenMod.Core.Eventing;
using OpenMod.UnityEngine.Extensions;
using OpenMod.Unturned.Users.Events;
using SDG.Unturned;
using Steamworks;
using System.Drawing;
using System.Threading.Tasks;

namespace Alpalis.AdminManager.Events;

public sealed class RestoreAdminSystems(
    IAdminSystem adminSystem,
    IVanishSystem vanishSystem,
    IGodSystem godSystem,
    IFlySystem flySystem,
    IStringLocalizer stringLocalizer,
    ILogger<RestoreAdminSystems> logger,
    IPermissionChecker permissionChecker) : IEventListener<UnturnedUserConnectedEvent>
{
    private readonly IAdminSystem m_AdminSystem = adminSystem;
    private readonly IVanishSystem m_VanishSystem = vanishSystem;
    private readonly IGodSystem m_GodSystem = godSystem;
    private readonly IFlySystem m_FlySystem = flySystem;
    private readonly IStringLocalizer m_StringLocalizer = stringLocalizer;
    private readonly ILogger<RestoreAdminSystems> m_Logger = logger;
    private readonly IPermissionChecker m_PermissionChecker = permissionChecker;

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

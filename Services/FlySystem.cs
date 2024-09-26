using Alpalis.AdminManager.API;
using Cysharp.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenMod.API.Ioc;
using OpenMod.API.Prioritization;
using SDG.Unturned;
using Steamworks;
using System.Collections.Generic;

namespace Alpalis.AdminManager.Services;

[ServiceImplementation(Lifetime = ServiceLifetime.Singleton, Priority = Priority.Normal)]
public sealed class FlySystem(
    ILogger<FlySystem> logger) : IFlySystem
{
    private readonly ILogger<FlySystem> m_Logger = logger;

    private HashSet<ulong> FlyModes { get; set; } = [];

    public UniTask EnableFlyMode(SteamPlayer sPlayer)
    {
        CSteamID steamID = sPlayer.playerID.steamID;
        if (IsInFlyMode(steamID)) return UniTask.CompletedTask;
        m_Logger.LogDebug(string.Format("The player {0} ({1}) enabled flymode",
            sPlayer.playerID.characterName, steamID));
        FlyModes.Add(steamID.m_SteamID);
        sPlayer.player.movement.sendPluginGravityMultiplier(0);
        return UniTask.CompletedTask;
    }

    public UniTask DisableFlyMode(SteamPlayer sPlayer)
    {
        CSteamID steamID = sPlayer.playerID.steamID;
        if (!IsInFlyMode(steamID)) return UniTask.CompletedTask;
        m_Logger.LogDebug(string.Format("The player {0} ({1}) disabled flymode",
            sPlayer.playerID.characterName, steamID));
        FlyModes.Remove(steamID.m_SteamID);
        sPlayer.player.movement.sendPluginGravityMultiplier(1);
        return UniTask.CompletedTask;
    }

    public void FlyUp(SteamPlayer sPlayer)
    {
        sPlayer.player.movement.sendPluginGravityMultiplier(sPlayer.player.movement.speed * 0.1f);
    }

    public void FlyIdle(SteamPlayer sPlayer)
    {
        sPlayer.player.movement.sendPluginGravityMultiplier(0);
    }

    public void FlyDown(SteamPlayer sPlayer)
    {
        sPlayer.player.movement.sendPluginGravityMultiplier(sPlayer.player.movement.speed * -0.1f);
    }

    public bool IsInFlyMode(CSteamID steamID) => FlyModes.Contains(steamID.m_SteamID);
}

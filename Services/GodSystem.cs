using Alpalis.AdminManager.API;
using Alpalis.AdminManager.Patches;
using Cysharp.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenMod.API.Ioc;
using OpenMod.API.Prioritization;
using SDG.Unturned;
using Steamworks;
using System;
using System.Collections.Generic;

namespace Alpalis.AdminManager.Services;

[ServiceImplementation(Lifetime = ServiceLifetime.Singleton, Priority = Priority.Normal)]
public sealed class GodSystem : IGodSystem, IDisposable
{
    private readonly ILogger<GodSystem> m_Logger;

    public GodSystem(
        ILogger<GodSystem> logger)
    {
        m_Logger = logger;
        StatUpdatingPatch.OnStatUpdating += OnStatUpdating;
    }

    private HashSet<ulong> GodModes { get; set; } = [];

    public UniTask EnableGodMode(SteamPlayer sPlayer)
    {
        CSteamID steamID = sPlayer.playerID.steamID;
        if (IsInGodMode(steamID)) return UniTask.CompletedTask;
        m_Logger.LogDebug(string.Format("The player {0} ({1}) enabled GodMode.",
            sPlayer.playerID.characterName, steamID));
        GodModes.Add(steamID.m_SteamID);
        return UniTask.CompletedTask;
    }

    public UniTask DisableGodMode(SteamPlayer sPlayer)
    {
        CSteamID steamID = sPlayer.playerID.steamID;
        if (!IsInGodMode(steamID)) return UniTask.CompletedTask;
        m_Logger.LogDebug(string.Format("The player {0} ({1}) disabled GodMode.",
            sPlayer.playerID.characterName, steamID));
        GodModes.Remove(steamID.m_SteamID);
        return UniTask.CompletedTask;
    }

    public bool IsInGodMode(CSteamID steamID) => GodModes.Contains(steamID.m_SteamID);

    private bool OnStatUpdating(PlayerLife player) => IsInGodMode(player.channel.owner.playerID.steamID);

    public void Dispose()
    {
        StatUpdatingPatch.OnStatUpdating -= OnStatUpdating;
    }
}

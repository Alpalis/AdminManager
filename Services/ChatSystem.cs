using Alpalis.AdminManager.API;
using Microsoft.Extensions.DependencyInjection;
using OpenMod.API.Ioc;
using OpenMod.Unturned.Users;
using SDG.Unturned;
using Steamworks;
using System;
using System.Collections.Generic;

namespace Alpalis.AdminManager.Services;

[ServiceImplementation(Lifetime = ServiceLifetime.Singleton)]
public sealed class ChatSystem : IChatSystem
{
    private bool ChatDisabled { get; set; } = false;

    private HashSet<ulong> MutedPlayers { get; set; } = [];

    public bool DisableChat()
    {
        if (!ChatDisabled)
        {
            ChatDisabled = true;
            return true;
        }
        return false;
    }

    public bool EnableChat()
    {
        if (ChatDisabled)
        {
            ChatDisabled = false;
            return true;
        }
        return false;
    }

    public bool IsChatDisabled() => ChatDisabled;

    public bool IsMuted(CSteamID steamID) => MutedPlayers.Contains(steamID.m_SteamID);

    public bool Mute(UnturnedUser user, TimeSpan time) => Mute(user.Player.SteamPlayer, time);

    public bool Mute(SteamPlayer sPlayer, TimeSpan _)
    {
        CSteamID steamID = sPlayer.playerID.steamID;
        if (IsMuted(steamID))
            return false;
        MutedPlayers.Add(steamID.m_SteamID);
        return true;
    }
}

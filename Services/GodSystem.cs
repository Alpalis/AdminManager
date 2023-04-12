using Alpalis.AdminManager.API;
using Alpalis.AdminManager.Models;
using Alpalis.AdminManager.Patches;
using Alpalis.UtilityServices.API;
using Cysharp.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenMod.API.Ioc;
using OpenMod.API.Plugins;
using OpenMod.API.Prioritization;
using SDG.Unturned;
using Steamworks;
using System;
using System.Collections.Generic;

namespace Alpalis.AdminManager.Services
{
    [ServiceImplementation(Lifetime = ServiceLifetime.Singleton, Priority = Priority.Normal)]
    public class GodSystem : IGodSystem, IDisposable
    {
        private readonly ILogger<GodSystem> m_Logger;

        public GodSystem(
            ILogger<GodSystem> logger)
        {
            m_Logger = logger;
            GodModes = new();
            StatUpdatingPatch.OnStatUpdating += OnStatUpdating;
        }

        private HashSet<ulong> GodModes { get; set; }

        public async UniTask EnableGodMode(SteamPlayer sPlayer)
        {
            CSteamID steamID = sPlayer.playerID.steamID;
            if (IsInGodMode(steamID)) return;
            m_Logger.LogDebug(string.Format("The player {0} ({1}) enabled GodMode.",
                sPlayer.playerID.characterName, steamID));
            GodModes.Add(steamID.m_SteamID);
            //m_UIManager.RunSideUI(sPlayer, m_ConfigurationManager.GetConfig<Config>(m_Plugin).GodUIID,
            //    m_ConfigurationManager.GetConfig<Config>(m_Plugin).GodUIKey);
        }

        public async UniTask DisableGodMode(SteamPlayer sPlayer)
        {
            CSteamID steamID = sPlayer.playerID.steamID;
            if (!IsInGodMode(steamID)) return;
            m_Logger.LogDebug(string.Format("The player {0} ({1}) disabled GodMode.",
                sPlayer.playerID.characterName, steamID));
            GodModes.Remove(steamID.m_SteamID);
            //m_UIManager.StopSideUI(sPlayer, m_ConfigurationManager.GetConfig<Config>(m_Plugin).GodUIID,
            //    m_ConfigurationManager.GetConfig<Config>(m_Plugin).GodUIKey, "GodMode", 750);
        }

        public bool IsInGodMode(CSteamID steamID)
        {
            if (GodModes.Contains(steamID.m_SteamID))
                return true;
            return false;
        }

        private bool OnStatUpdating(PlayerLife player)
        {
            return IsInGodMode(player.channel.owner.playerID.steamID);
        }

        public void Dispose()
        {
            StatUpdatingPatch.OnStatUpdating -= OnStatUpdating;
        }
    }
}

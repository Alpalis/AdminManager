﻿using Alpalis.AdminManager.API;
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
    public class VanishSystem : IVanishSystem
    {
        private readonly ILogger<VanishSystem> m_Logger;

        public VanishSystem(
            ILogger<VanishSystem> logger)
        {
            m_Logger = logger;
            VanishModes = new();
        }

        private HashSet<ulong> VanishModes { get; set; }

        public async UniTask EnableVanishMode(SteamPlayer sPlayer)
        {
            CSteamID steamID = sPlayer.playerID.steamID;
            if (IsInVanishMode(steamID)) return;
            m_Logger.LogDebug(string.Format("The player {0} ({1}) enabled vanishmode",
                sPlayer.playerID.characterName, steamID));
            VanishModes.Add(steamID.m_SteamID);
            //m_UIManager.RunSideUI(sPlayer, m_ConfigurationManager.GetConfig<Config>(m_Plugin).VanishUIID,
            //    m_ConfigurationManager.GetConfig<Config>(m_Plugin).VanishUIKey);
            sPlayer.player.movement.canAddSimulationResultsToUpdates = false;
        }

        public async UniTask DisableVanishMode(SteamPlayer sPlayer)
        {
            CSteamID steamID = sPlayer.playerID.steamID;
            if (!IsInVanishMode(steamID)) return;
            m_Logger.LogDebug(string.Format("The player {0} ({1}) disabled vanishmode",
                sPlayer.playerID.characterName, steamID));
            VanishModes.Remove(steamID.m_SteamID);
            //m_UIManager.StopSideUI(sPlayer, m_ConfigurationManager.GetConfig<Config>(m_Plugin).VanishUIID,
            //    m_ConfigurationManager.GetConfig<Config>(m_Plugin).VanishUIKey, "VanishMode", 750);
            sPlayer.player.movement.canAddSimulationResultsToUpdates = true;
            PlayerLook look = sPlayer.player.look;
            PlayerMovement movement = sPlayer.player.movement;
            movement.updates.Add(new PlayerStateUpdate(movement.move, look.angle, look.rot));
        }

        public bool IsInVanishMode(CSteamID steamID)
        {
            if (VanishModes.Contains(steamID.m_SteamID))
                return true;
            return false;
        }
    }
}

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
    public class FlySystem : IFlySystem
    {
        private readonly ILogger<FlySystem> m_Logger;

        public FlySystem(
            ILogger<FlySystem> logger)
        {
            m_Logger = logger;
            FlyModes = new();
        }

        private HashSet<ulong> FlyModes { get; set; }

        public async UniTask EnableFlyMode(SteamPlayer sPlayer)
        {
            CSteamID steamID = sPlayer.playerID.steamID;
            if (IsInFlyMode(steamID)) return;
            m_Logger.LogDebug(string.Format("The player {0} ({1}) enabled flymode",
                sPlayer.playerID.characterName, steamID));
            FlyModes.Add(steamID.m_SteamID);
            sPlayer.player.movement.sendPluginGravityMultiplier(0);
            //m_UIManager.RunSideUI(sPlayer, m_ConfigurationManager.GetConfig<Config>(m_Plugin).FlyUIID,
            //    m_ConfigurationManager.GetConfig<Config>(m_Plugin).FlyUIKey);
        }

        public async UniTask DisableFlyMode(SteamPlayer sPlayer)
        {
            CSteamID steamID = sPlayer.playerID.steamID;
            if (!IsInFlyMode(steamID)) return;
            m_Logger.LogDebug(string.Format("The player {0} ({1}) disabled flymode",
                sPlayer.playerID.characterName, steamID));
            FlyModes.Remove(steamID.m_SteamID);
            sPlayer.player.movement.sendPluginGravityMultiplier(1);
            //m_UIManager.StopSideUI(sPlayer, m_ConfigurationManager.GetConfig<Config>(m_Plugin).FlyUIID,
            //    m_ConfigurationManager.GetConfig<Config>(m_Plugin).FlyUIKey, "FlyMode", 750);
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

        public bool IsInFlyMode(CSteamID steamID)
        {
            if (FlyModes.Contains(steamID.m_SteamID))
                return true;
            return false;
        }
    }
}

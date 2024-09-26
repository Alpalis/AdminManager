using Alpalis.AdminManager.API;
using Cysharp.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenMod.API.Ioc;
using OpenMod.API.Prioritization;
using SDG.Unturned;
using Steamworks;
using System.Collections.Generic;

namespace Alpalis.AdminManager.Services
{
    [ServiceImplementation(Lifetime = ServiceLifetime.Singleton, Priority = Priority.Normal)]
    public sealed class VanishSystem(
        ILogger<VanishSystem> logger) : IVanishSystem
    {
        private readonly ILogger<VanishSystem> m_Logger = logger;

        private HashSet<ulong> VanishModes { get; set; } = [];

        public UniTask EnableVanishMode(SteamPlayer sPlayer)
        {
            CSteamID steamID = sPlayer.playerID.steamID;
            if (IsInVanishMode(steamID)) return UniTask.CompletedTask;
            m_Logger.LogDebug(string.Format("The player {0} ({1}) enabled vanishmode",
                sPlayer.playerID.characterName, steamID));
            VanishModes.Add(steamID.m_SteamID);
            sPlayer.player.movement.canAddSimulationResultsToUpdates = false;
            return UniTask.CompletedTask;
        }

        public UniTask DisableVanishMode(SteamPlayer sPlayer)
        {
            CSteamID steamID = sPlayer.playerID.steamID;
            if (!IsInVanishMode(steamID)) return UniTask.CompletedTask;
            m_Logger.LogDebug(string.Format("The player {0} ({1}) disabled vanishmode",
                sPlayer.playerID.characterName, steamID));
            VanishModes.Remove(steamID.m_SteamID);
            sPlayer.player.movement.canAddSimulationResultsToUpdates = true;
            PlayerLook look = sPlayer.player.look;
            PlayerMovement movement = sPlayer.player.movement;
            movement.updates.Add(new PlayerStateUpdate(movement.move, look.angle, look.rot));
            return UniTask.CompletedTask;
        }

        public bool IsInVanishMode(CSteamID steamID)
        {
            if (VanishModes.Contains(steamID.m_SteamID))
                return true;
            return false;
        }
    }
}

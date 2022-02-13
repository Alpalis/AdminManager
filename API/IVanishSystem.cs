using Cysharp.Threading.Tasks;
using OpenMod.API.Ioc;
using SDG.Unturned;
using Steamworks;

namespace Alpalis.AdminManager.API
{
    /// <summary>
    /// Interface for managing player's vanish mode.
    /// </summary>
    [Service]
    public interface IVanishSystem
    {
        /// <summary>
        /// Enables the player's vanish mode.
        /// </summary>
        /// <param name="sPlayer">SteamPlayer of player</param>
        UniTask EnableVanishMode(SteamPlayer sPlayer);

        /// <summary>
        /// Disables the player's vanish mode.
        /// </summary>
        /// <param name="sPlayer">SteamPlayer of player</param>
        UniTask DisableVanishMode(SteamPlayer sPlayer);

        /// <summary>
        /// Checks if the player is in vanish mode.
        /// </summary>
        /// <param name="steamID">CSteamID of player</param>
        /// <returns>Returns true if the player is in vanish mode and false if not.</returns>
        bool IsInVanishMode(CSteamID steamID);
    }
}

using Cysharp.Threading.Tasks;
using OpenMod.API.Ioc;
using SDG.Unturned;
using Steamworks;

namespace Alpalis.AdminManager.API
{
    /// <summary>
    /// Interface for managing player's GodModes.
    /// </summary>
    [Service]
    public interface IGodSystem
    {
        /// <summary>
        /// Enables the player's god mode.
        /// </summary>
        /// <param name="sPlayer">SteamPlayer of player</param>
        UniTask EnableGodMode(SteamPlayer sPlayer);

        /// <summary>
        /// Disables the player's god mode.
        /// </summary>
        /// <param name="sPlayer">SteamPlayer of player</param>
        UniTask DisableGodMode(SteamPlayer sPlayer);

        /// <summary>
        /// Checks if the player is in god mode.
        /// </summary>
        /// <param name="steamID">CSteamID of player</param>
        /// <returns>Returns true if the player is in god mode and false if not.</returns>
        bool IsInGodMode(CSteamID steamID);
    }
}

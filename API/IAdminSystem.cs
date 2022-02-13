using OpenMod.API.Commands;
using OpenMod.API.Ioc;
using SDG.Unturned;
using Steamworks;

namespace Alpalis.AdminManager.API
{
    /// <summary>
    /// Interface for managing player's admin mode.
    /// </summary>
    [Service]
    public interface IAdminSystem
    {
        /// <summary>
        /// Sets the state of the player's admin mode.
        /// </summary>
        /// <param name="sPlayer">SteamPlayer</param>
        /// <returns>Returns true if the player is in admin mode and false if not.</returns>
        bool ToggleAdminMode(SteamPlayer sPlayer);

        #region IsInAdminMode
        /// <summary>
        /// Checks if the player is in admin mode.
        /// </summary>
        /// <param name="steamID">SteamID of player</param>
        /// <returns>Returns true if the player is in admin mode and false if not.</returns>
        bool IsInAdminMode(CSteamID steamID);

        /// <summary>
        /// Checks if the player is in admin mode.
        /// </summary>
        /// <param name="steamID">SteamID of player</param>
        /// <returns>Returns true if the player is in admin mode and false if not.</returns>
        bool IsInAdminMode(ICommandActor actor);
        #endregion IsInAdminMode
    }
}

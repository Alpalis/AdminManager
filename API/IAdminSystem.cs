using OpenMod.API.Commands;
using OpenMod.API.Ioc;
using OpenMod.Unturned.Users;
using Steamworks;
using System.Threading.Tasks;

namespace Alpalis.AdminManager.API;

/// <summary>
/// Interface for managing player's admin mode.
/// </summary>
[Service]
public interface IAdminSystem
{
    /// <summary>
    /// Sets the state of the player's admin mode.
    /// </summary>
    /// <param name="user">Unturned user</param>
    /// <returns>Returns true if the player is in admin mode and false if not.</returns>
    Task<bool> ToggleAdminMode(UnturnedUser user);

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

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    bool IsAdminModeDisabled();
}

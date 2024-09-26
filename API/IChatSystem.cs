using OpenMod.API.Ioc;
using Steamworks;

namespace Alpalis.AdminManager.API;

/// <summary>
/// Service for managing chat system.
/// </summary>
[Service]
public interface IChatSystem
{
    /// <summary>
    /// Enables writing on chat.
    /// </summary>
    bool EnableChat();

    /// <summary>
    /// Disables writing on chat.
    /// </summary>
    bool DisableChat();

    /// <summary>
    /// Checks if chat is disabled.
    /// </summary>
    /// <returns>Returns true if chat is disabled, otherwise false.</returns>
    bool IsChatDisabled();

    /// <summary>
    /// Checks if user is muted.
    /// </summary>
    /// <param name="steamID">StreamID of the player.</param>
    /// <returns>Returns true if player is muted, otherwise false.</returns>
    bool IsMuted(CSteamID steamID);
}

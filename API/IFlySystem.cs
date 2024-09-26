using Cysharp.Threading.Tasks;
using OpenMod.API.Ioc;
using SDG.Unturned;
using Steamworks;

namespace Alpalis.AdminManager.API;

/// <summary>
/// Interface for managing player's FlyModes.
/// </summary>
[Service]
public interface IFlySystem
{
    /// <summary>
    /// Enables player's fly mode.
    /// </summary>
    /// <param name="sPlayer">Selected player.</param>
    UniTask EnableFlyMode(SteamPlayer sPlayer);

    /// <summary>
    /// Disables player's fly mode.
    /// </summary>
    /// <param name="sPlayer">Selected player.</param>
    UniTask DisableFlyMode(SteamPlayer sPlayer);

    /// <summary>
    /// Makes player start flying up.
    /// </summary>
    /// <param name="sPlayer">Player that will fly up.</param>
    void FlyUp(SteamPlayer sPlayer);

    /// <summary>
    /// Makes player start hovering in the air.
    /// </summary>
    /// <param name="sPlayer">Player that will hover in the air.</param>
    void FlyIdle(SteamPlayer sPlayer);

    /// <summary>
    /// Makes player start flying down.
    /// </summary>
    /// <param name="sPlayer">Player that will fly down.</param>
    void FlyDown(SteamPlayer sPlayer);

    /// <summary>
    /// Checks if the player is in fly mode.
    /// </summary>
    /// <param name="steamID">CSteamID of player</param>
    /// <returns>Returns true if the player is in fly mode and false if not.</returns>
    bool IsInFlyMode(CSteamID steamID);
}

using Cysharp.Threading.Tasks;
using OpenMod.API.Ioc;
using SDG.Unturned;
using Steamworks;

namespace Alpalis.AdminManager.API
{
    /// <summary>
    /// Interface for managing player's FlyModes.
    /// </summary>
    [Service]
    public interface IFlySystem
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sPlayer"></param>
        /// <returns></returns>
        UniTask EnableFlyMode(SteamPlayer sPlayer);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sPlayer"></param>
        /// <returns></returns>
        UniTask DisableFlyMode(SteamPlayer sPlayer);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sPlayer"></param>
        void FlyUp(SteamPlayer sPlayer);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sPlayer"></param>
        void FlyIdle(SteamPlayer sPlayer);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sPlayer"></param>
        void FlyDown(SteamPlayer sPlayer);

        /// <summary>
        /// Checks if the player is in fly mode.
        /// </summary>
        /// <param name="steamID">CSteamID of player</param>
        /// <returns>Returns true if the player is in fly mode and false if not.</returns>
        bool IsInFlyMode(CSteamID steamID);
    }
}

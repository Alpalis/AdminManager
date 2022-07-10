using OpenMod.API.Ioc;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alpalis.AdminManager.API
{
    [Service]
    public interface IChatSystem
    {
        /// <summary>
        /// 
        /// </summary>
        bool EnableChat();

        /// <summary>
        /// 
        /// </summary>
        bool DisableChat();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        bool IsChatDisabled();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="steamID"></param>
        /// <returns></returns>
        bool IsMuted(CSteamID steamID);
    }
}

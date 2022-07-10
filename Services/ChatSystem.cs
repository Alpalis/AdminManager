using Alpalis.AdminManager.API;
using Microsoft.Extensions.DependencyInjection;
using OpenMod.API.Ioc;
using OpenMod.API.Permissions;
using Steamworks;

namespace Alpalis.AdminManager.Services
{
    [ServiceImplementation(Lifetime = ServiceLifetime.Singleton)]
    public class ChatSystem : IChatSystem
    {
        #region Class Constructor
        public ChatSystem()
        {
            ChatDisabled = false;
        }
        #endregion Class Constructor

        private bool ChatDisabled { get; set; }

        public bool DisableChat()
        {
            if (ChatDisabled == false)
            {
                ChatDisabled = true;
                return true;
            }
            return false;
        }

        public bool EnableChat()
        {
            if (ChatDisabled == true)
            {
                ChatDisabled = false;
                return true;
            }
            return false;
        }

        public bool IsChatDisabled() => ChatDisabled;

        public bool IsMuted(CSteamID steamID) => false;
    }
}

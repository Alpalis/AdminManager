using Alpalis.AdminManager.API;
using Alpalis.AdminManager.Models;
using Alpalis.UtilityServices.API;
using Alpalis.UtilityServices.API.Enums;
using Alpalis.UtilityServices.Events;
using Microsoft.Extensions.Localization;
using OpenMod.API.Eventing;
using OpenMod.API.Permissions;
using OpenMod.API.Plugins;
using OpenMod.Core.Eventing;
using OpenMod.Unturned.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alpalis.AdminManager.Events
{
    public class CancelChatMessage : IEventListener<CanSendMessageEvent>
    {
        #region Member Variables
        private readonly IChatSystem m_ChatSystem;
        private readonly IAdminSystem m_AdminSystem;
        private readonly IPermissionChecker m_PermissionChecker;
        private readonly IUnturnedUserDirectory m_UnturnedUserDirectory;
        private readonly IStringLocalizer m_StringLocalizer;
        private readonly IConfigurationManager m_ConfigurationManager;
        private readonly Main m_Plugin;
        #endregion Member Variables

        #region Class Constructor
        public CancelChatMessage(
            IChatSystem chatSystem,
            IAdminSystem adminSystem,
            IPermissionChecker permissionChecker,
            IUnturnedUserDirectory unturnedUserDirectory,
            IStringLocalizer stringLocalizer,
            IConfigurationManager configurationManager,
            IPluginAccessor<Main> plugin)
        {
            m_ChatSystem = chatSystem;
            m_AdminSystem = adminSystem;
            m_PermissionChecker = permissionChecker;
            m_UnturnedUserDirectory = unturnedUserDirectory;
            m_StringLocalizer = stringLocalizer;
            m_ConfigurationManager = configurationManager;
            m_Plugin = plugin.Instance!;
        }
        #endregion Class Constructor

        [EventListener(Priority = EventListenerPriority.Normal)]
        public async Task HandleEventAsync(object? sender, CanSendMessageEvent @event)
        {
            if (await m_PermissionChecker.CheckPermissionAsync(m_UnturnedUserDirectory.GetUser(@event.Player.Player),
                "Alpalis.AdminManager:chatoverride") == PermissionGrantResult.Grant)
                if (m_AdminSystem.IsInAdminMode(@event.Player.SteamId)) return;
            Config config = m_ConfigurationManager.GetConfig<Config>(m_Plugin);
            if (m_ChatSystem.IsChatDisabled())
            {
                @event.IsCancelled = true;
                @event.Reason = ECancelMessageReason.ChatDisabled;
                @event.Message = string.Format("{0}{1}",
                    config.MessagePrefix ? m_StringLocalizer["chat_message:prefix"] : "",
                    m_StringLocalizer["chat_message:chat_disabled"]);
                return;
            }
            else if (m_ChatSystem.IsMuted(@event.Player.SteamId))
            {
                @event.IsCancelled = true;
                @event.Reason = ECancelMessageReason.Muted;
                @event.Message = string.Format("{0}{1}",
                    config.MessagePrefix ? m_StringLocalizer["chat_message:prefix"] : "",
                    m_StringLocalizer["chat_message:muted"]);
                return;
            }
            
        }
    }
}

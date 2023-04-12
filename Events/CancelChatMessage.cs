using Alpalis.AdminManager.API;
using Alpalis.UtilityServices.API.Enums;
using Alpalis.UtilityServices.API.Events;
using Microsoft.Extensions.Localization;
using OpenMod.API.Eventing;
using OpenMod.API.Permissions;
using OpenMod.Core.Eventing;
using OpenMod.Unturned.Users;
using System.Threading.Tasks;

namespace Alpalis.AdminManager.Events
{
    public class CancelChatMessage : IEventListener<CanSendMessageEvent>
    {
        private readonly IChatSystem m_ChatSystem;
        private readonly IAdminSystem m_AdminSystem;
        private readonly IPermissionChecker m_PermissionChecker;
        private readonly IUnturnedUserDirectory m_UnturnedUserDirectory;
        private readonly IStringLocalizer m_StringLocalizer;

        public CancelChatMessage(
            IChatSystem chatSystem,
            IAdminSystem adminSystem,
            IPermissionChecker permissionChecker,
            IUnturnedUserDirectory unturnedUserDirectory,
            IStringLocalizer stringLocalizer)
        {
            m_ChatSystem = chatSystem;
            m_AdminSystem = adminSystem;
            m_PermissionChecker = permissionChecker;
            m_UnturnedUserDirectory = unturnedUserDirectory;
            m_StringLocalizer = stringLocalizer;
        }

        [EventListener(Priority = EventListenerPriority.Normal)]
        public async Task HandleEventAsync(object? sender, CanSendMessageEvent @event)
        {
            if (await m_PermissionChecker.CheckPermissionAsync(m_UnturnedUserDirectory.GetUser(@event.Player.Player),
                "Alpalis.AdminManager:chatoverride") == PermissionGrantResult.Grant)
                if (m_AdminSystem.IsInAdminMode(@event.Player.SteamId)) return;
            if (m_ChatSystem.IsChatDisabled())
            {
                @event.IsCancelled = true;
                @event.Reason = ECancelMessageReason.ChatDisabled;
                @event.Message = string.Format("{0}{1}",
                    m_StringLocalizer["chat_message:prefix"],
                    m_StringLocalizer["chat_message:chat_disabled"]);
                return;
            }
            else if (m_ChatSystem.IsMuted(@event.Player.SteamId))
            {
                @event.IsCancelled = true;
                @event.Reason = ECancelMessageReason.Muted;
                @event.Message = string.Format("{0}{1}",
                    m_StringLocalizer["chat_message:prefix"],
                    m_StringLocalizer["chat_message:muted"]);
                return;
            }

        }
    }
}

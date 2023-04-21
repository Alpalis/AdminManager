using Alpalis.AdminManager.API;
using Cysharp.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OpenMod.API.Commands;
using OpenMod.Core.Commands;
using OpenMod.Unturned.Commands;
using OpenMod.Unturned.Users;
using SDG.Unturned;
using System;

namespace Alpalis.AdminManager.Commands
{
    [Command("sudo")]
    [CommandSyntax("<player> <message/command>")]
    [CommandDescription("Allows to force another player to send message or command.")]
    public class SudoCommand : UnturnedCommand
    {
        private readonly IStringLocalizer m_StringLocalizer;
        private readonly IAdminSystem m_AdminSystem;

        public SudoCommand(
            IStringLocalizer stringLocalizer,
            IAdminSystem adminSystem,
            IServiceProvider serviceProvider) : base(serviceProvider)
        {
            m_StringLocalizer = stringLocalizer;
            m_AdminSystem = adminSystem;
        }

        protected override async UniTask OnExecuteAsync()
        {
            if (Context.Parameters.Length != 2)
                throw new CommandWrongUsageException(Context);
            if (!m_AdminSystem.IsInAdminMode(Context.Actor))
                throw new UserFriendlyException(string.Format("{0}{1}",
                     m_StringLocalizer["sudo_command:prefix"],
                     m_StringLocalizer["sudo_command:error_adminmode"]));
            if (!Context.Parameters.TryGet(0, out UnturnedUser? targetUser) || targetUser == null)
                throw new UserFriendlyException(string.Format("{0}{1}",
                    Context.Actor.GetType() == typeof(UnturnedUser) ? m_StringLocalizer["sudo_command:prefix"] : "",
                    m_StringLocalizer["sudo_command:error_player"]));
            if (!Context.Parameters.TryGet(1, out string? data) || string.IsNullOrEmpty(data))
                throw new UserFriendlyException(string.Format("{0}{1}",
                    Context.Actor.GetType() == typeof(UnturnedUser) ? m_StringLocalizer["sudo_command:prefix"] : "",
                    m_StringLocalizer["sudo_command:error_message"]));

            await UniTask.SwitchToMainThread();

            // NO EQUIVALENT METHOD
            ChatManager.instance.askChat(targetUser.SteamId, (byte)EChatMode.GLOBAL, data);
            PrintAsync(string.Format("{0}{1}",
                Context.Actor.GetType() == typeof(UnturnedUser) ? m_StringLocalizer["sudo_command:prefix"] : "",
                m_StringLocalizer["sudo_command:succeed"]));
        }
    }
}

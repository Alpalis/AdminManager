using Alpalis.AdminManager.API;
using Cysharp.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OpenMod.API.Commands;
using OpenMod.Core.Commands;
using OpenMod.Core.Console;
using OpenMod.UnityEngine.Extensions;
using OpenMod.Unturned.Commands;
using OpenMod.Unturned.Users;
using SDG.Unturned;
using System;
using System.Drawing;

namespace Alpalis.AdminManager.Commands.Chat;

public sealed class AnnouncementCommand
{
    [Command("announcement")]
    [CommandDescription("Allows to send announcements.")]
    [CommandSyntax("<message>")]
    [CommandActor(typeof(ConsoleActor))]
    public sealed class Console(
        IStringLocalizer stringLocalizer,
        IServiceProvider serviceProvider) : UnturnedCommand(serviceProvider)
    {
        private readonly IStringLocalizer m_StringLocalizer = stringLocalizer;

        protected override async UniTask OnExecuteAsync()
        {
            if (Context.Parameters.Count == 0)
                throw new CommandWrongUsageException(Context);
            string message = string.Join(" ", Context.Parameters).Replace("</Color>", "").Replace("<color=", "");
            if (message == "") throw new UserFriendlyException(m_StringLocalizer["announcement_command:error_null_message"]);
            await UniTask.SwitchToMainThread();
            ChatManager.serverSendMessage(string.Format("{0}{1}",
                     m_StringLocalizer["announcement_command:prefix"],
                     message), Color.White.ToUnityColor(), null, null, EChatMode.GLOBAL, null, true);
            await PrintAsync(m_StringLocalizer["announcement_command:succeed"]);
        }
    }

    [Command("announcement")]
    [CommandDescription("Send announcements.")]
    [CommandSyntax("<message>")]
    [CommandActor(typeof(UnturnedUser))]
    public sealed class Unturned(
        IAdminSystem adminSystem,
        IStringLocalizer stringLocalizer,
        IServiceProvider serviceProvider) : UnturnedCommand(serviceProvider)
    {
        private readonly IAdminSystem m_AdminSystem = adminSystem;
        private readonly IStringLocalizer m_StringLocalizer = stringLocalizer;

        protected override async UniTask OnExecuteAsync()
        {
            if (Context.Parameters.Count == 0)
                throw new CommandWrongUsageException(Context);
            UnturnedUser user = (UnturnedUser)Context.Actor;
            if (!m_AdminSystem.IsInAdminMode(user))
                throw new UserFriendlyException(string.Format("{0}{1}",
                     m_StringLocalizer["announcement_command:prefix"],
                     m_StringLocalizer["announcement_command:error_adminmode"]));
            string message = string.Join(" ", Context.Parameters).Replace("</Color>", "").Replace("<color=", "");
            if (message == "") throw new UserFriendlyException(m_StringLocalizer["annonucement_command:error_null_message"]);
            await UniTask.SwitchToMainThread();
            ChatManager.serverSendMessage(string.Format("{0}{1}",
                     m_StringLocalizer["announcement_command:prefix"],
                     message), Color.White.ToUnityColor(), null, null, EChatMode.GLOBAL, null, true);
            await PrintAsync(string.Format("{0}{1}",
                     m_StringLocalizer["announcement_command:prefix"],
                     m_StringLocalizer["announcement_command:succeed"]));
        }
    }
}

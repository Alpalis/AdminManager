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

namespace Alpalis.AdminManager.Commands.Chat
{
    public class AnnoucementCommand
    {
        [Command("annoucement")]
        [CommandDescription("Allows to send annoucements.")]
        [CommandSyntax("<message>")]
        [CommandActor(typeof(ConsoleActor))]
        public class Console : UnturnedCommand
        {
            private readonly IStringLocalizer m_StringLocalizer;

            public Console(
                IStringLocalizer stringLocalizer,
                IServiceProvider serviceProvider) : base(serviceProvider)
            {
                m_StringLocalizer = stringLocalizer;
            }

            protected override async UniTask OnExecuteAsync()
            {
                if (Context.Parameters.Count == 0)
                    throw new CommandWrongUsageException(Context);
                string message = string.Join(" ", Context.Parameters).Replace("</Color>", "").Replace("<color=", "");
                if (message == "") throw new UserFriendlyException(m_StringLocalizer["annoucement_command:error_null_message"]);
                await UniTask.SwitchToMainThread();
                ChatManager.serverSendMessage(string.Format("{0}{1}",
                         m_StringLocalizer["annoucement_command:prefix"],
                         message), Color.White.ToUnityColor(), null, null, EChatMode.GLOBAL, null, true);
                PrintAsync(m_StringLocalizer["annoucement_command:succeed:executor"]);
            }
        }

        [Command("annoucement")]
        [CommandDescription("Send annoucements.")]
        [CommandSyntax("<message>")]
        [CommandActor(typeof(UnturnedUser))]
        public class Unturned : UnturnedCommand
        {
            private readonly IAdminSystem m_AdminSystem;
            private readonly IStringLocalizer m_StringLocalizer;

            public Unturned(
                IAdminSystem adminSystem,
                IStringLocalizer stringLocalizer,
                IServiceProvider serviceProvider) : base(serviceProvider)
            {
                m_AdminSystem = adminSystem;
                m_StringLocalizer = stringLocalizer;
            }

            protected override async UniTask OnExecuteAsync()
            {
                if (Context.Parameters.Count == 0)
                    throw new CommandWrongUsageException(Context);
                UnturnedUser user = (UnturnedUser)Context.Actor;
                if (!m_AdminSystem.IsInAdminMode(user))
                    throw new UserFriendlyException(string.Format("{0}{1}",
                         m_StringLocalizer["annoucement_command:prefix"],
                         m_StringLocalizer["annoucement_command:error_adminmode"]));
                string message = string.Join(" ", Context.Parameters).Replace("</Color>", "").Replace("<color=", "");
                if (message == "") throw new UserFriendlyException(m_StringLocalizer["annoucement_command:error_null_message"]);
                await UniTask.SwitchToMainThread();
                ChatManager.serverSendMessage(string.Format("{0}{1}",
                         m_StringLocalizer["annoucement_command:prefix"],
                         message), Color.White.ToUnityColor(), null, null, EChatMode.GLOBAL, null, true);
                PrintAsync(string.Format("{0}{1}",
                         m_StringLocalizer["annoucement_command:prefix"],
                         m_StringLocalizer["annoucement_command:succeed:executor"]));
            }
        }
    }
}

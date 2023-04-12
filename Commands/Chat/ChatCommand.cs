﻿using Alpalis.AdminManager.API;
using Cysharp.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OpenMod.API.Commands;
using OpenMod.Core.Commands;
using OpenMod.UnityEngine.Extensions;
using OpenMod.Unturned.Commands;
using OpenMod.Unturned.Users;
using SDG.Unturned;
using System;
using System.Drawing;

namespace Alpalis.AdminManager.Commands.Chat
{
    public class ChatCommand
    {
        [Command("chat")]
        [CommandSyntax("<clear/enable/disable>")]
        [CommandDescription("Manages server's chat.")]
        public class Root : UnturnedCommand
        {
            public Root(
                IServiceProvider serviceProvider) : base(serviceProvider)
            {
            }

            protected override async UniTask OnExecuteAsync()
            {
                throw new CommandWrongUsageException(Context);
            }
        }

        [Command("clear")]
        [CommandDescription("Clears server's chat.")]
        [CommandParent(typeof(Root))]
        public class Clear : UnturnedCommand
        {
            private readonly IAdminSystem m_AdminSystem;
            private readonly IStringLocalizer m_StringLocalizer;

            public Clear(
                IAdminSystem adminSystem,
                IStringLocalizer stringLocalizer,
                IServiceProvider serviceProvider) : base(serviceProvider)
            {
                m_AdminSystem = adminSystem;
                m_StringLocalizer = stringLocalizer;
            }

            protected override async UniTask OnExecuteAsync()
            {
                if (Context.Parameters.Count != 0)
                    throw new CommandWrongUsageException(Context);
                if (Context.Actor is UnturnedUser user && !m_AdminSystem.IsInAdminMode(user))
                    throw new UserFriendlyException(string.Format("{0}{1}",
                         m_StringLocalizer["chat_command:prefix"],
                         m_StringLocalizer["chat_command:error_adminmode"]));
                string message = m_StringLocalizer[string.Format("chat_command:clear:succeed:{0}",
                    Context.Actor is UnturnedUser ? "player" : "console"), Context.Actor is UnturnedUser user1 ? new
                    {
                        PlayerName = user1.Player.SteamPlayer.playerID.playerName,
                        CharacterName = user1.Player.SteamPlayer.playerID.characterName,
                        NickName = user1.Player.SteamPlayer.playerID.nickName,
                        SteamID = user1.Player.SteamId,
                    } : new { }];
                await UniTask.SwitchToMainThread();
                for (int i = 0; i < 100; i++)
                    ChatManager.serverSendMessage(" ", Color.White.ToUnityColor(), null, null, EChatMode.GLOBAL, null, false);
                ChatManager.serverSendMessage(new string('▒', (int)(message.Length * 0.75)), Color.Gray.ToUnityColor(), null, null, EChatMode.GLOBAL, null, false);
                ChatManager.serverSendMessage(message, Color.White.ToUnityColor(), null, null, EChatMode.GLOBAL, null, true);
                ChatManager.serverSendMessage(new string('▒', (int)(message.Length * 0.75)), Color.Gray.ToUnityColor(), null, null, EChatMode.GLOBAL, null, false);
                PrintAsync(string.Format("{0}{1}",
                    Context.Actor is UnturnedUser ? m_StringLocalizer["chat_command:prefix"] : "",
                    m_StringLocalizer["chat_command:clear:succeed:executor"]));
            }
        }

        [Command("disable")]
        [CommandDescription("Disables server's chat.")]
        [CommandParent(typeof(Root))]
        public class Disable : UnturnedCommand
        {
            private readonly IAdminSystem m_AdminSystem;
            private readonly IStringLocalizer m_StringLocalizer;
            private readonly IChatSystem m_ChatSystem;

            public Disable(
                IAdminSystem adminSystem,
                IStringLocalizer stringLocalizer,
                IChatSystem chatSystem,
                IServiceProvider serviceProvider) : base(serviceProvider)
            {
                m_AdminSystem = adminSystem;
                m_StringLocalizer = stringLocalizer;
                m_ChatSystem = chatSystem;
            }

            protected override async UniTask OnExecuteAsync()
            {
                if (Context.Parameters.Count != 0)
                    throw new CommandWrongUsageException(Context);
                if (Context.Actor is UnturnedUser user && !m_AdminSystem.IsInAdminMode(user))
                    throw new UserFriendlyException(string.Format("{0}{1}",
                         m_StringLocalizer["chat_command:prefix"],
                         m_StringLocalizer["chat_command:error_adminmode"]));
                if (!m_ChatSystem.DisableChat())
                    throw new UserFriendlyException(string.Format("{0}{1}",
                         Context.Actor is UnturnedUser ? m_StringLocalizer["chat_command:prefix"] : "",
                         m_StringLocalizer["chat_command:disable:error_disabled"]));
                await UniTask.SwitchToMainThread();
                ChatManager.serverSendMessage(string.Format("{0}{1}",
                        m_StringLocalizer["chat_command:prefix"],
                        m_StringLocalizer[string.Format("chat_command:disable:succeed:{0}",
                        Context.Actor is UnturnedUser ? "player" : "console"), Context.Actor is UnturnedUser user1 ? new
                        {
                            PlayerName = user1.Player.SteamPlayer.playerID.playerName,
                            CharacterName = user1.Player.SteamPlayer.playerID.characterName,
                            NickName = user1.Player.SteamPlayer.playerID.nickName,
                            SteamID = user1.Player.SteamId
                        } : new { }]), Color.White.ToUnityColor(), null, null, EChatMode.GLOBAL, null, true);
                PrintAsync(string.Format("{0}{1}",
                    Context.Actor is UnturnedUser ? m_StringLocalizer["chat_command:prefix"] : "",
                    m_StringLocalizer["chat_command:disable:succeed:executor"]));
            }
        }

        [Command("enable")]
        [CommandDescription("Enables server's chat.")]
        [CommandParent(typeof(Root))]
        public class Enable : UnturnedCommand
        {
            private readonly IAdminSystem m_AdminSystem;
            private readonly IStringLocalizer m_StringLocalizer;
            private readonly IChatSystem m_ChatSystem;

            public Enable(
                IAdminSystem adminSystem,
                IStringLocalizer stringLocalizer,
                IChatSystem chatSystem,
                IServiceProvider serviceProvider) : base(serviceProvider)
            {
                m_AdminSystem = adminSystem;
                m_StringLocalizer = stringLocalizer;
                m_ChatSystem = chatSystem;
            }

            protected override async UniTask OnExecuteAsync()
            {
                if (Context.Parameters.Count != 0)
                    throw new CommandWrongUsageException(Context);
                if (Context.Actor is UnturnedUser user && !m_AdminSystem.IsInAdminMode(user))
                    throw new UserFriendlyException(string.Format("{0}{1}",
                         m_StringLocalizer["chat_command:prefix"],
                         m_StringLocalizer["chat_command:error_adminmode"]));
                if (!m_ChatSystem.EnableChat())
                    throw new UserFriendlyException(string.Format("{0}{1}",
                         Context.Actor is UnturnedUser ? m_StringLocalizer["chat_command:prefix"] : "",
                         m_StringLocalizer["chat_command:enable:error_enabled"]));
                await UniTask.SwitchToMainThread();
                ChatManager.serverSendMessage(string.Format("{0}{1}",
                        m_StringLocalizer["chat_command:prefix"],
                        m_StringLocalizer[string.Format("chat_command:enable:succeed:{0}",
                        Context.Actor is UnturnedUser ? "player" : "console"), Context.Actor is UnturnedUser user1 ? new
                        {
                            PlayerName = user1.Player.SteamPlayer.playerID.playerName,
                            CharacterName = user1.Player.SteamPlayer.playerID.characterName,
                            NickName = user1.Player.SteamPlayer.playerID.nickName,
                            SteamID = user1.Player.SteamId
                        } : new { }]), Color.White.ToUnityColor(), null, null, EChatMode.GLOBAL, null, true);
                PrintAsync(string.Format("{0}{1}",
                    Context.Actor is UnturnedUser ? m_StringLocalizer["chat_command:prefix"] : "",
                    m_StringLocalizer["chat_command:enable:succeed:executor"]));
            }
        }
    }
}

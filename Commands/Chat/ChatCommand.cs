using Alpalis.AdminManager.API;
using Alpalis.AdminManager.Models;
using Alpalis.UtilityServices.API;
using Cysharp.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OpenMod.API.Commands;
using OpenMod.API.Plugins;
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
        #region Commad Parameters
        [Command("chat")]
        [CommandSyntax("<clear/enable/disable>")]
        [CommandDescription("Command to manage chat.")]
        #endregion Command Parameters
        public class ChatRoot : UnturnedCommand
        {
            #region Class Constructor
            public ChatRoot(
                IServiceProvider serviceProvider) : base(serviceProvider)
            {
            }
            #endregion Class Constructor

            protected override async UniTask OnExecuteAsync()
            {
                throw new CommandWrongUsageException(Context);
            }
        }

        #region Commad Parameters
        [Command("clear")]
        [CommandDescription("Command to clear chat.")]
        [CommandParent(typeof(ChatRoot))]
        #endregion Command Parameters
        public class Clear : UnturnedCommand
        {
            #region Member Variables
            private readonly IAdminSystem m_AdminSystem;
            private readonly IStringLocalizer m_StringLocalizer;
            private readonly IConfigurationManager m_ConfigurationManager;
            private readonly IIdentityManagerImplementation m_IdentityManagerImplementation;
            private readonly Main m_Plugin;
            #endregion Member Variables

            #region Class Constructor
            public Clear(
                IAdminSystem adminSystem,
                IStringLocalizer stringLocalizer,
                IConfigurationManager configurationManager,
                IIdentityManagerImplementation identityManagerImplementation,
                IPluginAccessor<Main> plugin,
                IServiceProvider serviceProvider) : base(serviceProvider)
            {
                m_AdminSystem = adminSystem;
                m_StringLocalizer = stringLocalizer;
                m_ConfigurationManager = configurationManager;
                m_IdentityManagerImplementation = identityManagerImplementation;
                m_Plugin = plugin.Instance!;
            }
            #endregion Class Constructor

            protected override async UniTask OnExecuteAsync()
            {
                if (Context.Parameters.Count != 0)
                    throw new CommandWrongUsageException(Context);
                Config config = m_ConfigurationManager.GetConfig<Config>(m_Plugin);
                if (Context.Actor is UnturnedUser user && !m_AdminSystem.IsInAdminMode(user))
                    throw new UserFriendlyException(string.Format("{0}{1}",
                         config.MessagePrefix ? m_StringLocalizer["chat_command:prefix"] : "",
                         m_StringLocalizer["chat_command:error_adminmode"]));
                string message = m_StringLocalizer[string.Format("chat_command:clear:succeed:{0}",
                    Context.Actor is UnturnedUser ? "player" : "console"), Context.Actor is UnturnedUser user1 ? new
                    {
                        PlayerName = user1.Player.SteamPlayer.playerID.playerName,
                        CharacterName = user1.Player.SteamPlayer.playerID.characterName,
                        NickName = user1.Player.SteamPlayer.playerID.nickName,
                        SteamID = user1.Player.SteamId,
                        ID = m_IdentityManagerImplementation.GetIdentity(user1.Player.SteamId)
                    } : new { }];
                await UniTask.SwitchToMainThread();
                for (int i = 0; i < 100; i++)
                    ChatManager.serverSendMessage(" ", Color.White.ToUnityColor(), null, null, EChatMode.GLOBAL, null, false);
                ChatManager.serverSendMessage(new string('▒', (int)(message.Length * 0.75)), Color.Gray.ToUnityColor(), null, null, EChatMode.GLOBAL, null, false);
                ChatManager.serverSendMessage(message, Color.White.ToUnityColor(), null, null, EChatMode.GLOBAL, null, true);
                ChatManager.serverSendMessage(new string('▒', (int)(message.Length * 0.75)), Color.Gray.ToUnityColor(), null, null, EChatMode.GLOBAL, null, false);
                PrintAsync(string.Format("{0}{1}",
                    config.MessagePrefix && Context.Actor is UnturnedUser ? m_StringLocalizer["chat_command:prefix"] : "",
                    m_StringLocalizer["chat_command:clear:succeed:executor"]));
            }
        }

        #region Commad Parameters
        [Command("disable")]
        [CommandDescription("Command to disable chat.")]
        [CommandParent(typeof(ChatRoot))]
        #endregion Command Parameters
        public class Disable : UnturnedCommand
        {
            #region Member Variables
            private readonly IAdminSystem m_AdminSystem;
            private readonly IStringLocalizer m_StringLocalizer;
            private readonly IConfigurationManager m_ConfigurationManager;
            private readonly IIdentityManagerImplementation m_IdentityManagerImplementation;
            private readonly IChatSystem m_ChatSystem;
            private readonly Main m_Plugin;
            #endregion Member Variables

            #region Class Constructor
            public Disable(
                IAdminSystem adminSystem,
                IStringLocalizer stringLocalizer,
                IConfigurationManager configurationManager,
                IIdentityManagerImplementation identityManagerImplementation,
                IChatSystem chatSystem,
                IPluginAccessor<Main> plugin,
                IServiceProvider serviceProvider) : base(serviceProvider)
            {
                m_AdminSystem = adminSystem;
                m_StringLocalizer = stringLocalizer;
                m_ConfigurationManager = configurationManager;
                m_IdentityManagerImplementation = identityManagerImplementation;
                m_ChatSystem = chatSystem;
                m_Plugin = plugin.Instance!;
            }
            #endregion Class Constructor

            protected override async UniTask OnExecuteAsync()
            {
                if (Context.Parameters.Count != 0)
                    throw new CommandWrongUsageException(Context);
                Config config = m_ConfigurationManager.GetConfig<Config>(m_Plugin);
                if (Context.Actor is UnturnedUser user && !m_AdminSystem.IsInAdminMode(user))
                    throw new UserFriendlyException(string.Format("{0}{1}",
                         config.MessagePrefix ? m_StringLocalizer["chat_command:prefix"] : "",
                         m_StringLocalizer["chat_command:error_adminmode"]));
                if (!m_ChatSystem.DisableChat())
                    throw new UserFriendlyException(string.Format("{0}{1}",
                         config.MessagePrefix && Context.Actor is UnturnedUser ? m_StringLocalizer["chat_command:prefix"] : "",
                         m_StringLocalizer["chat_command:disable:error_disabled"]));
                await UniTask.SwitchToMainThread();
                ChatManager.serverSendMessage(string.Format("{0}{1}",
                        config.MessagePrefix ? m_StringLocalizer["chat_command:prefix"] : "",
                        m_StringLocalizer[string.Format("chat_command:disable:succeed:{0}",
                        Context.Actor is UnturnedUser ? "player" : "console"), Context.Actor is UnturnedUser user1 ? new
                        {
                            PlayerName = user1.Player.SteamPlayer.playerID.playerName,
                            CharacterName = user1.Player.SteamPlayer.playerID.characterName,
                            NickName = user1.Player.SteamPlayer.playerID.nickName,
                            SteamID = user1.Player.SteamId,
                            ID = m_IdentityManagerImplementation.GetIdentity(user1.Player.SteamId)
                        } : new { }]), Color.White.ToUnityColor(), null, null, EChatMode.GLOBAL, null, true);
                PrintAsync(string.Format("{0}{1}",
                    config.MessagePrefix && Context.Actor is UnturnedUser ? m_StringLocalizer["chat_command:prefix"] : "",
                    m_StringLocalizer["chat_command:disable:succeed:executor"]));
            }
        }

        #region Commad Parameters
        [Command("enable")]
        [CommandDescription("Command to enable chat.")]
        [CommandParent(typeof(ChatRoot))]
        #endregion Command Parameters
        public class Enable : UnturnedCommand
        {
            #region Member Variables
            private readonly IAdminSystem m_AdminSystem;
            private readonly IStringLocalizer m_StringLocalizer;
            private readonly IConfigurationManager m_ConfigurationManager;
            private readonly IIdentityManagerImplementation m_IdentityManagerImplementation;
            private readonly IChatSystem m_ChatSystem;
            private readonly Main m_Plugin;
            #endregion Member Variables

            #region Class Constructor
            public Enable(
                IAdminSystem adminSystem,
                IStringLocalizer stringLocalizer,
                IConfigurationManager configurationManager,
                IIdentityManagerImplementation identityManagerImplementation,
                IChatSystem chatSystem,
                IPluginAccessor<Main> plugin,
                IServiceProvider serviceProvider) : base(serviceProvider)
            {
                m_AdminSystem = adminSystem;
                m_StringLocalizer = stringLocalizer;
                m_ConfigurationManager = configurationManager;
                m_IdentityManagerImplementation = identityManagerImplementation;
                m_ChatSystem = chatSystem;
                m_Plugin = plugin.Instance!;
            }
            #endregion Class Constructor

            protected override async UniTask OnExecuteAsync()
            {
                if (Context.Parameters.Count != 0)
                    throw new CommandWrongUsageException(Context);
                Config config = m_ConfigurationManager.GetConfig<Config>(m_Plugin);
                if (Context.Actor is UnturnedUser user && !m_AdminSystem.IsInAdminMode(user))
                    throw new UserFriendlyException(string.Format("{0}{1}",
                         config.MessagePrefix ? m_StringLocalizer["chat_command:prefix"] : "",
                         m_StringLocalizer["chat_command:error_adminmode"]));
                if (!m_ChatSystem.EnableChat())
                    throw new UserFriendlyException(string.Format("{0}{1}",
                         config.MessagePrefix && Context.Actor is UnturnedUser ? m_StringLocalizer["chat_command:prefix"] : "",
                         m_StringLocalizer["chat_command:enable:error_enabled"]));
                await UniTask.SwitchToMainThread();
                ChatManager.serverSendMessage(string.Format("{0}{1}",
                        config.MessagePrefix ? m_StringLocalizer["chat_command:prefix"] : "",
                        m_StringLocalizer[string.Format("chat_command:enable:succeed:{0}",
                        Context.Actor is UnturnedUser ? "player" : "console"), Context.Actor is UnturnedUser user1 ? new
                        {
                            PlayerName = user1.Player.SteamPlayer.playerID.playerName,
                            CharacterName = user1.Player.SteamPlayer.playerID.characterName,
                            NickName = user1.Player.SteamPlayer.playerID.nickName,
                            SteamID = user1.Player.SteamId,
                            ID = m_IdentityManagerImplementation.GetIdentity(user1.Player.SteamId)
                        } : new { }]), Color.White.ToUnityColor(), null, null, EChatMode.GLOBAL, null, true);
                PrintAsync(string.Format("{0}{1}",
                    config.MessagePrefix && Context.Actor is UnturnedUser ? m_StringLocalizer["chat_command:prefix"] : "",
                    m_StringLocalizer["chat_command:enable:succeed:executor"]));
            }
        }
    }
}

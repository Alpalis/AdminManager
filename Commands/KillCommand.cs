using Alpalis.AdminManager.API;
using Alpalis.AdminManager.Models;
using Alpalis.UtilityServices.API;
using Alpalis.UtilityServices.Services;
using Cysharp.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OpenMod.API.Commands;
using OpenMod.API.Permissions;
using OpenMod.API.Plugins;
using OpenMod.Core.Commands;
using OpenMod.Core.Console;
using OpenMod.Core.Permissions;
using OpenMod.Unturned.Commands;
using OpenMod.Unturned.Users;
using SDG.Unturned;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alpalis.AdminManager.Commands
{
    public class KillCommand
    {
        #region Commad Parameters
        [Command("kill")]
        [CommandSyntax("[player]")]
        [CommandDescription("Command to kill yourself or another player.")]
        [RegisterCommandPermission("other", Description = "Allows to kill other people.")]
        [CommandActor(typeof(UnturnedUser))]
        #endregion Command Parameters
        public class KillUnturned : UnturnedCommand
        {
            #region Member Variables
            private readonly IStringLocalizer m_StringLocalizer;
            private readonly IConfigurationManager m_ConfigurationManager;
            private readonly IAdminSystem m_AdminSystem;
            private readonly IIdentityManagerImplementation m_IdentityManagerImplementation;
            private readonly Main m_Plugin;
            #endregion Member Variables

            #region Class Constructor
            public KillUnturned(
                IStringLocalizer StringLocalizer,
                IConfigurationManager configurationManager,
                IAdminSystem adminSystem,
                IdentityManagerImplementation identityManagerImplementation,
                IPluginAccessor<Main> plugin,
                IServiceProvider serviceProvider) : base(serviceProvider)
            {
                m_StringLocalizer = StringLocalizer;
                m_ConfigurationManager = configurationManager;
                m_AdminSystem = adminSystem;
                m_IdentityManagerImplementation = identityManagerImplementation;
                m_Plugin = plugin.Instance!;
            }
            #endregion Class Constructor

            protected override async UniTask OnExecuteAsync()
            {
                if (Context.Parameters.Count != 0 && Context.Parameters.Count != 1)
                    throw new CommandWrongUsageException(Context);
                Config config = m_ConfigurationManager.GetConfig<Config>(m_Plugin);
                UnturnedUser user = (UnturnedUser)Context.Actor;
                if (!m_AdminSystem.IsInAdminMode(user))
                    throw new UserFriendlyException(string.Format("{0}{1}",
                         config.MessagePrefix ? m_StringLocalizer["kill_command:prefix"] : "",
                         m_StringLocalizer["kill_command:error_adminmode"]));
                if (Context.Parameters.Count == 0)
                {
                    await user.Player.KillAsync();
                    user.PrintMessageAsync(string.Format("{0}{1}",
                         config.MessagePrefix ? m_StringLocalizer["kill_command:prefix"] : "",
                         m_StringLocalizer["kill_command:yourself"]));
                    return;
                }
                if (await CheckPermissionAsync("other") != PermissionGrantResult.Grant)
                    throw new NotEnoughPermissionException(Context, "other");
                if (!Context.Parameters.TryGet(0, out UnturnedUser? targetUser) || targetUser == null)
                    throw new UserFriendlyException(string.Format("{0}{1}",
                        config.MessagePrefix ? m_StringLocalizer["kill_command:prefix"] : "",
                        m_StringLocalizer["kill_command:error_player"]));
                await targetUser.Player.KillAsync();
                SteamPlayer sPlayer = user.Player.SteamPlayer;
                ushort? identity = m_IdentityManagerImplementation.GetIdentity(user.SteamId);
                SteamPlayer targetSPlayer = targetUser.Player.SteamPlayer;
                ushort? targetIdentity = m_IdentityManagerImplementation.GetIdentity(targetUser.SteamId);
                targetUser.PrintMessageAsync(string.Format("{0}{1}",
                         config.MessagePrefix ? m_StringLocalizer["kill_command:prefix"] : "",
                         m_StringLocalizer["kill_command:somebody:player", new
                         {
                             PlayerName = sPlayer.playerID.playerName,
                             CharacterName = sPlayer.playerID.characterName,
                             NickName = sPlayer.playerID.nickName,
                             SteamID = user.SteamId,
                             ID = identity,
                         }]));
                user.PrintMessageAsync(string.Format("{0}{1}",
                         config.MessagePrefix ? m_StringLocalizer["kill_command:prefix"] : "",
                         m_StringLocalizer["kill_command:somebody:executor", new
                         {
                             PlayerName = targetSPlayer.playerID.playerName,
                             CharacterName = targetSPlayer.playerID.characterName,
                             NickName = targetSPlayer.playerID.nickName,
                             SteamID = targetUser.SteamId,
                             ID = targetIdentity,
                         }]));
            }
        }

        #region Commad Parameters
        [Command("kill")]
        [CommandSyntax("<player>")]
        [CommandDescription("Command to kill another player.")]
        [CommandActor(typeof(ConsoleActor))]
        #endregion Command Parameters
        public class KillConsole : UnturnedCommand
        {
            #region Member Variables
            private readonly IStringLocalizer m_StringLocalizer;
            private readonly IConfigurationManager m_ConfigurationManager;
            private readonly IAdminSystem m_AdminSystem;
            private readonly IIdentityManagerImplementation m_IdentityManagerImplementation;
            private readonly Main m_Plugin;
            #endregion Member Variables

            #region Class Constructor
            public KillConsole(
                IStringLocalizer StringLocalizer,
                IConfigurationManager configurationManager,
                IAdminSystem adminSystem,
                IdentityManagerImplementation identityManagerImplementation,
                IPluginAccessor<Main> plugin,
                IServiceProvider serviceProvider) : base(serviceProvider)
            {
                m_StringLocalizer = StringLocalizer;
                m_ConfigurationManager = configurationManager;
                m_AdminSystem = adminSystem;
                m_IdentityManagerImplementation = identityManagerImplementation;
                m_Plugin = plugin.Instance!;
            }
            #endregion Class Constructor

            protected override async UniTask OnExecuteAsync()
            {
                if (Context.Parameters.Count != 1)
                    throw new CommandWrongUsageException(Context);
                Config config = m_ConfigurationManager.GetConfig<Config>(m_Plugin);
                UnturnedUser user = (UnturnedUser)Context.Actor;
                if (!Context.Parameters.TryGet(0, out UnturnedUser? targetUser) || targetUser == null)
                    throw new UserFriendlyException(m_StringLocalizer["kill_command:error_player"]);
                await targetUser.Player.KillAsync();
                SteamPlayer sPlayer = user.Player.SteamPlayer;
                ushort? identity = m_IdentityManagerImplementation.GetIdentity(user.SteamId);
                SteamPlayer targetSPlayer = targetUser.Player.SteamPlayer;
                ushort? targetIdentity = m_IdentityManagerImplementation.GetIdentity(targetUser.SteamId);
                targetUser.PrintMessageAsync(m_StringLocalizer["kill_command:somebody:console"]);
                user.PrintMessageAsync(m_StringLocalizer["kill_command:somebody:executor", new
                {
                    PlayerName = targetSPlayer.playerID.playerName,
                    CharacterName = targetSPlayer.playerID.characterName,
                    NickName = targetSPlayer.playerID.nickName,
                    SteamID = targetUser.SteamId,
                    ID = targetIdentity,
                }]);
            }
        }
    }
}

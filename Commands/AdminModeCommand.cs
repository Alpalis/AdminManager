using Alpalis.AdminManager.API;
using Alpalis.AdminManager.Models;
using Alpalis.UtilityServices.API;
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

namespace Alpalis.AdminManager.Commands
{
    public class AdminModeCommand
    {
        #region Commad Parameters
        [Command("adminmode")]
        [CommandSyntax("get/switch")]
        [CommandDescription("Command to manage the admin mode.")]
        #endregion Command Parameters
        public class AdminMode : UnturnedCommand
        {
            #region Class Constructor
            public AdminMode(
                IServiceProvider serviceProvider) : base(serviceProvider)
            {
            }
            #endregion Class Constructor

            protected override async UniTask OnExecuteAsync()
            {
                throw new CommandWrongUsageException(Context);
            }

        }

        #region Switch
        #region Commad Parameters
        [Command("switch")]
        [CommandSyntax("[steamID]")]
        [CommandDescription("Command to turn on and off the admin mode.")]
        [RegisterCommandPermission("other", Description = "Allows to switch admin mode of other player.")]
        [CommandActor(typeof(UnturnedUser))]
        [CommandParent(typeof(AdminMode))]
        #endregion Command Parameters
        public class SwitchUnturned : UnturnedCommand
        {
            #region Member Variables
            private readonly IAdminSystem m_AdminSystem;
            private readonly IStringLocalizer m_StringLocalizer;
            private readonly IConfigurationManager m_ConfigurationManager;
            private readonly IIdentityManagerImplementation m_IdentityManagerImplementation;
            private readonly Main m_Plugin;
            #endregion Member Variables

            #region Class Constructor
            public SwitchUnturned(
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
                Config config = m_ConfigurationManager.GetConfig<Config>(m_Plugin);
                UnturnedUser user = (UnturnedUser)Context.Actor;
                if (Context.Parameters.Count == 0)
                {
                    await UniTask.SwitchToMainThread();
                    PrintAsync(string.Format("{0}{1}", (config.MessagePrefix ? m_StringLocalizer["adminmode_command:prefix"] : ""),
                        m_StringLocalizer[string.Format("adminmode_command:switch:yourself:{0}",
                        m_AdminSystem.ToggleAdminMode(user.Player.SteamPlayer) ? "enabled" : "disabled")]));
                    return;
                }
                if (Context.Parameters.Count != 1)
                    throw new CommandWrongUsageException(Context);
                if (await CheckPermissionAsync("other") != PermissionGrantResult.Grant)
                    throw new NotEnoughPermissionException(Context, "other");
                if (!m_AdminSystem.IsInAdminMode(user))
                    throw new UserFriendlyException(string.Format("{0}{1}",
                         config.MessagePrefix ? m_StringLocalizer["adminmode_command:prefix"] : "",
                         m_StringLocalizer["adminmode_command:error_adminmode"]));
                if (!Context.Parameters.TryGet(0, out UnturnedUser? targetUser) || targetUser == null)
                    throw new UserFriendlyException(string.Format("{0}{1}",
                        config.MessagePrefix ? m_StringLocalizer["adminmode_command:prefix"] : "",
                        m_StringLocalizer["adminmode_command:error_player"]));
                SteamPlayer targetSPlayer = targetUser.Player.SteamPlayer;
                CSteamID targetSteamID = targetSPlayer.playerID.steamID;
                ushort? targetIdentity = m_IdentityManagerImplementation.GetIdentity(targetSteamID);
                SteamPlayer sPlayer = user.Player.SteamPlayer;
                CSteamID steamID = sPlayer.playerID.steamID;
                ushort? identity = m_IdentityManagerImplementation.GetIdentity(steamID);
                await UniTask.SwitchToMainThread();
                bool result = m_AdminSystem.ToggleAdminMode(targetUser.Player.SteamPlayer);
                targetUser.PrintMessageAsync(string.Format("{0}{1}", (config.MessagePrefix ? m_StringLocalizer["adminmode_command:prefix"] : ""),
                    m_StringLocalizer[string.Format("adminmode_command:switch:somebody:player:{0}",
                    result ? "enabled" : "disabled"), new
                    {
                        PlayerName = sPlayer.playerID.playerName,
                        CharacterName = sPlayer.playerID.characterName,
                        NickName = sPlayer.playerID.nickName,
                        SteamID = steamID,
                        ID = identity
                    }]));
                PrintAsync(string.Format("{0}{1}", (config.MessagePrefix ? m_StringLocalizer["adminmode_command:prefix"] : ""),
                    m_StringLocalizer[string.Format("adminmode_command:switch:somebody:executor:{0}",
                    result ? "enabled" : "disabled"), new
                    {
                        PlayerName = targetSPlayer.playerID.playerName,
                        CharacterName = targetSPlayer.playerID.characterName,
                        NickName = targetSPlayer.playerID.nickName,
                        SteamID = targetSteamID,
                        ID = targetIdentity
                    }]));
            }
        }

        #region Commad Parameters
        [Command("switch")]
        [CommandSyntax("<player>")]
        [CommandDescription("Command to turn on and off the admin mode.")]
        [CommandActor(typeof(ConsoleActor))]
        [CommandParent(typeof(AdminMode))]
        #endregion Command Parameters
        public class SwitchConsole : UnturnedCommand
        {
            #region Member Variables
            private readonly IAdminSystem m_AdminSystem;
            private readonly IStringLocalizer m_StringLocalizer;
            private readonly IConfigurationManager m_ConfigurationManager;
            private readonly IIdentityManagerImplementation m_IdentityManagerImplementation;
            private readonly Main m_Plugin;
            #endregion Member Variables

            #region Class Constructor
            public SwitchConsole(
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
                Config config = m_ConfigurationManager.GetConfig<Config>(m_Plugin);
                if (Context.Parameters.Length != 1)
                    throw new CommandWrongUsageException(Context);
                if (!Context.Parameters.TryGet(0, out UnturnedUser? user) || user == null)
                    throw new UserFriendlyException(string.Format("{0}{1}",
                        config.MessagePrefix ? m_StringLocalizer["adminmode_command:prefix"] : "",
                        m_StringLocalizer["adminmode_command:error_player"]));
                await UniTask.SwitchToMainThread();
                bool result = m_AdminSystem.ToggleAdminMode(user.Player.SteamPlayer);
                user.PrintMessageAsync(string.Format("{0}{1}", (config.MessagePrefix ? m_StringLocalizer["adminmode_command:prefix"] : ""),
                    m_StringLocalizer[string.Format("adminmode_command:switch:somebody:console:{0}",
                    result ? "enabled" : "disabled")]));
                SteamPlayer sPlayer = user.Player.SteamPlayer;
                CSteamID steamID = sPlayer.playerID.steamID;
                ushort? identity = m_IdentityManagerImplementation.GetIdentity(steamID);
                PrintAsync(m_StringLocalizer[string.Format("adminmode_command:switch:somebody:executor:{0}", result ? "enabled" : "disabled"), new
                {
                    PlayerName = sPlayer.playerID.playerName,
                    CharacterName = sPlayer.playerID.characterName,
                    NickName = sPlayer.playerID.nickName,
                    SteamID = steamID,
                    ID = identity
                }]);
            }
        }
        #endregion Switch

        #region Get
        #region Commad Parameters
        [Command("get")]
        [CommandSyntax("[player]")]
        [CommandDescription("Command to get state of your or player's admin mode.")]
        [RegisterCommandPermission("other", Description = "Allows to get admin mode of other player.")]
        [CommandActor(typeof(UnturnedUser))]
        [CommandParent(typeof(AdminMode))]
        #endregion Command Parameters
        public class GetUnturned : UnturnedCommand
        {
            #region Member Variables
            private readonly IAdminSystem m_AdminSystem;
            private readonly IStringLocalizer m_StringLocalizer;
            private readonly IIdentityManagerImplementation m_IdentityManagerImplementation;
            private readonly IConfigurationManager m_ConfigurationManager;
            private readonly Main m_Plugin;
            #endregion Member Variables

            #region Class Constructor
            public GetUnturned(
                IAdminSystem adminSystem,
                IStringLocalizer stringLocalizer,
                IIdentityManagerImplementation identityManagerImplementation,
                IConfigurationManager configurationManager,
                IPluginAccessor<Main> plugin,
                IServiceProvider serviceProvider) : base(serviceProvider)
            {
                m_AdminSystem = adminSystem;
                m_StringLocalizer = stringLocalizer;
                m_IdentityManagerImplementation = identityManagerImplementation;
                m_ConfigurationManager = configurationManager;
                m_Plugin = plugin.Instance!;
            }
            #endregion Class Constructor

            protected override async UniTask OnExecuteAsync()
            {
                Config config = m_ConfigurationManager.GetConfig<Config>(m_Plugin);
                UnturnedUser user = (UnturnedUser)Context.Actor;
                bool result = m_AdminSystem.IsInAdminMode(user.SteamId);
                if (Context.Parameters.Length == 0)
                {
                    PrintAsync(string.Format("{0}{1}",
                        config.MessagePrefix ? m_StringLocalizer["adminmode_command:prefix"] : "",
                        m_StringLocalizer[string.Format("adminmode_command:get:yourself:{0}", result ? "enabled" : "disabled")]));
                    return;
                }
                if (Context.Parameters.Length != 1)
                    throw new CommandWrongUsageException(Context);
                if (await CheckPermissionAsync("other") != PermissionGrantResult.Grant)
                    throw new NotEnoughPermissionException(Context, "other");
                if (!result)
                    throw new UserFriendlyException(string.Format("{0}{1}",
                         config.MessagePrefix ? m_StringLocalizer["adminmode_command:prefix"] : "",
                         m_StringLocalizer["adminmode_command:error_adminmode"]));
                if (!Context.Parameters.TryGet(0, out UnturnedUser? targetUser) || targetUser == null)
                    throw new UserFriendlyException(string.Format("{0}{1}",
                        config.MessagePrefix ? m_StringLocalizer["adminmode_command:prefix"] : "",
                        m_StringLocalizer["adminmode_command:error_player"]));
                SteamPlayer sPlayer = targetUser.Player.SteamPlayer;
                CSteamID steamID = sPlayer.playerID.steamID;
                ushort? identity = m_IdentityManagerImplementation.GetIdentity(steamID);
                bool resultTarget = m_AdminSystem.IsInAdminMode(steamID);
                PrintAsync(string.Format("{0}{1}", config.MessagePrefix ? m_StringLocalizer["adminmode_command:prefix"] : "",
                    m_StringLocalizer[string.Format("adminmode_command:get:somebody:{0}", resultTarget ? "enabled" : "disabled"), new
                    {
                        PlayerName = sPlayer.playerID.playerName,
                        CharacterName = sPlayer.playerID.characterName,
                        NickName = sPlayer.playerID.nickName,
                        SteamID = steamID,
                        ID = identity
                    }]));
            }
        }

        #region Commad Parameters
        [Command("get")]
        [CommandSyntax("<player>")]
        [CommandDescription("Command to get state of player's admin mode.")]
        [CommandActor(typeof(ConsoleActor))]
        [CommandParent(typeof(AdminMode))]
        #endregion Command Parameters
        public class GetConsole : UnturnedCommand
        {
            #region Member Variables
            private readonly IAdminSystem m_AdminSystem;
            private readonly IStringLocalizer m_StringLocalizer;
            private readonly IIdentityManagerImplementation m_IdentityManagerImplementation;
            #endregion Member Variables

            #region Class Constructor
            public GetConsole(
                IAdminSystem adminSystem,
                IStringLocalizer stringLocalizer,
                IIdentityManagerImplementation identityManagerImplementation,
                IServiceProvider serviceProvider) : base(serviceProvider)
            {
                m_AdminSystem = adminSystem;
                m_StringLocalizer = stringLocalizer;
                m_IdentityManagerImplementation = identityManagerImplementation;
            }
            #endregion Class Constructor

            protected override async UniTask OnExecuteAsync()
            {
                if (Context.Parameters.Length != 1)
                    throw new CommandWrongUsageException(Context);
                if (!Context.Parameters.TryGet(0, out UnturnedUser? user) || user == null)
                    throw new UserFriendlyException(m_StringLocalizer["adminmode_command:error_player"]);
                SteamPlayer sPlayer = user.Player.SteamPlayer;
                CSteamID steamID = sPlayer.playerID.steamID;
                ushort? identity = m_IdentityManagerImplementation.GetIdentity(steamID);
                bool result = m_AdminSystem.IsInAdminMode(steamID);
                PrintAsync(m_StringLocalizer[string.Format("adminmode_command:get:somebody:{0}", result ? "enabled" : "disabled"), new
                {
                    PlayerName = sPlayer.playerID.playerName,
                    CharacterName = sPlayer.playerID.characterName,
                    NickName = sPlayer.playerID.nickName,
                    SteamID = steamID,
                    ID = identity
                }]);
            }
        }
        #endregion Get
    }
}

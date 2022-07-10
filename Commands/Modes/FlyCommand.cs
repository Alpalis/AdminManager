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

namespace Alpalis.AdminManager.Commands.Modes
{
    public class FlyCommand
    {
        #region Commad Parameters
        [Command("fly")]
        [CommandSyntax("<get/switch>")]
        [CommandDescription("Command to manage the fly modes.")]
        #endregion Command Parameters
        public class FlyRoot : UnturnedCommand
        {
            #region Class Constructor
            public FlyRoot(
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
        [CommandSyntax("[player]")]
        [CommandDescription("Command to turn on and off the fly mode.")]
        [RegisterCommandPermission("other", Description = "Allows to switch fly mode of other player.")]
        [CommandActor(typeof(UnturnedUser))]
        [CommandParent(typeof(FlyRoot))]
        #endregion Command Parameters
        public class SwitchUnturned : UnturnedCommand
        {
            #region Member Variables
            private readonly IFlySystem m_FlySystem;
            private readonly IAdminSystem m_AdminSystem;
            private readonly IStringLocalizer m_StringLocalizer;
            private readonly IConfigurationManager m_ConfigurationManager;
            private readonly IIdentityManagerImplementation m_IdentityManagerImplementation;
            private readonly Main m_Plugin;
            #endregion Member Variables

            #region Class Constructor
            public SwitchUnturned(
                IFlySystem flySystem,
                IAdminSystem adminSystem,
                IStringLocalizer stringLocalizer,
                IConfigurationManager configurationManager,
                IIdentityManagerImplementation identityManagerImplementation,
                IPluginAccessor<Main> plugin,
                IServiceProvider serviceProvider) : base(serviceProvider)
            {
                m_FlySystem = flySystem;
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
                if (!m_AdminSystem.IsInAdminMode(user))
                    throw new UserFriendlyException(string.Format("{0}{1}",
                         config.MessagePrefix ? m_StringLocalizer["fly_command:prefix"] : "",
                         m_StringLocalizer["fly_command:error_adminmode"]));
                SteamPlayer sPlayer = user.Player.SteamPlayer;
                if (Context.Parameters.Length == 0)
                {
                    bool result = m_FlySystem.IsInFlyMode(user.SteamId);
                    await UniTask.SwitchToMainThread();
                    if (result)
                        m_FlySystem.DisableFlyMode(sPlayer);
                    else
                        m_FlySystem.EnableFlyMode(sPlayer);
                    PrintAsync(string.Format("{0} {1}", config.MessagePrefix ? m_StringLocalizer["fly_command:prefix"] : "",
                        m_StringLocalizer[string.Format("fly_command:switch:yourself:{0}",
                        result ? "disabled" : "enabled")]));
                    return;
                }
                if (Context.Parameters.Length != 1)
                    throw new CommandWrongUsageException(Context);
                if (await CheckPermissionAsync("other") != PermissionGrantResult.Grant)
                    throw new NotEnoughPermissionException(Context, "other");
                if (!Context.Parameters.TryGet(0, out UnturnedUser? targetUser) || targetUser == null)
                    throw new UserFriendlyException(string.Format("{0}{1}",
                        config.MessagePrefix ? m_StringLocalizer["fly_command:prefix"] : "",
                        m_StringLocalizer["fly_command:error_player"]));
                SteamPlayer targetSPlayer = targetUser.Player.SteamPlayer;
                CSteamID targetSteamID = targetSPlayer.playerID.steamID;
                ushort? targetIdentity = m_IdentityManagerImplementation.GetIdentity(targetSteamID);
                ushort? identity = m_IdentityManagerImplementation.GetIdentity(user.SteamId);
                bool targetResult = m_FlySystem.IsInFlyMode(targetSteamID);
                await UniTask.SwitchToMainThread();
                if (targetResult)
                    m_FlySystem.DisableFlyMode(targetSPlayer);
                else
                    m_FlySystem.EnableFlyMode(targetSPlayer);
                targetUser.PrintMessageAsync(string.Format("{0} {1}", config.MessagePrefix ? m_StringLocalizer["fly_command:prefix"] : "",
                    m_StringLocalizer[string.Format("fly_command:switch:somebody:player:{0}",
                    targetResult ? "disabled" : "enabled"), new
                    {
                        PlayerName = sPlayer.playerID.playerName,
                        CharacterName = sPlayer.playerID.characterName,
                        NickName = sPlayer.playerID.nickName,
                        SteamID = user.SteamId,
                        ID = identity
                    }]));
                PrintAsync(string.Format("{0} {1}", config.MessagePrefix ? m_StringLocalizer["fly_command:prefix"] : "",
                    m_StringLocalizer[string.Format("fly_command:switch:somebody:executor:{0}",
                    targetResult ? "disabled" : "enabled"), new
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
        [CommandDescription("Command to turn on and off the fly modes.")]
        [CommandActor(typeof(ConsoleActor))]
        [CommandParent(typeof(FlyRoot))]
        #endregion Command Parameters
        public class SwitchConsole : UnturnedCommand
        {
            #region Member Variables
            private readonly IFlySystem m_FlySystem;
            private readonly IStringLocalizer m_StringLocalizer;
            private readonly IConfigurationManager m_ConfigurationManager;
            private readonly IIdentityManagerImplementation m_IdentityManagerImplementation;
            private readonly Main m_Plugin;
            #endregion Member Variables

            #region Class Constructor
            public SwitchConsole(
                IFlySystem flySystem,
                IStringLocalizer stringLocalizer,
                IConfigurationManager configurationManager,
                IIdentityManagerImplementation identityManagerImplementation,
                IPluginAccessor<Main> plugin,
                IServiceProvider serviceProvider) : base(serviceProvider)
            {
                m_FlySystem = flySystem;
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
                    throw new UserFriendlyException(m_StringLocalizer["fly_command:error_player"]);
                SteamPlayer sPlayer = user.Player.SteamPlayer;
                CSteamID steamID = sPlayer.playerID.steamID;
                ushort? identity = m_IdentityManagerImplementation.GetIdentity(steamID);
                bool result = m_FlySystem.IsInFlyMode(user.SteamId);
                await UniTask.SwitchToMainThread();
                if (result)
                    m_FlySystem.DisableFlyMode(sPlayer);
                else
                    m_FlySystem.EnableFlyMode(sPlayer);
                user.PrintMessageAsync(string.Format("{0} {1}", config.MessagePrefix ? m_StringLocalizer["fly_command:prefix"] : "",
                    m_StringLocalizer[string.Format("fly_command:switch:somebody:console:{0}",
                    result ? "disabled" : "enabled")]));
                PrintAsync(m_StringLocalizer[string.Format("fly_command:switch:somebody:executor:{0}",
                    result ? "disabled" : "enabled"), new
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
        [CommandDescription("Command to get state of your or player's fly modes.")]
        [RegisterCommandPermission("other", Description = "Allows to get fly modes state of other player.")]
        [CommandActor(typeof(UnturnedUser))]
        [CommandParent(typeof(FlyRoot))]
        #endregion Command Parameters
        public class GetUnturned : UnturnedCommand
        {
            #region Member Variables
            private readonly IAdminSystem m_AdminSystem;
            private readonly IConfigurationManager m_ConfigurationManager;
            private readonly IFlySystem m_FlySystem;
            private readonly IStringLocalizer m_StringLocalizer;
            private readonly IIdentityManagerImplementation m_IdentityManagerImplementation;
            private readonly Main m_Plugin;
            #endregion Member Variables

            #region Class Constructor
            public GetUnturned(
                IAdminSystem adminSystem,
                IConfigurationManager configurationManager,
                IFlySystem flySystem,
                IStringLocalizer stringLocalizer,
                IIdentityManagerImplementation identityManagerImplementation,
                IPluginAccessor<Main> plugin,
                IServiceProvider serviceProvider) : base(serviceProvider)
            {
                m_AdminSystem = adminSystem;
                m_ConfigurationManager = configurationManager;
                m_FlySystem = flySystem;
                m_StringLocalizer = stringLocalizer;
                m_IdentityManagerImplementation = identityManagerImplementation;
                m_Plugin = plugin.Instance!;
            }
            #endregion Class Constructor

            protected override async UniTask OnExecuteAsync()
            {
                Config config = m_ConfigurationManager.GetConfig<Config>(m_Plugin);
                UnturnedUser user = (UnturnedUser)Context.Actor;
                if (!m_AdminSystem.IsInAdminMode(user))
                    throw new UserFriendlyException(string.Format("{0}{1}",
                         config.MessagePrefix ? m_StringLocalizer["fly_command:prefix"] : "",
                         m_StringLocalizer["fly_command:error_adminmode"]));
                if (Context.Parameters.Length == 0)
                {
                    PrintAsync(string.Format("{0}{1}",
                        config.MessagePrefix ? m_StringLocalizer["fly_command:prefix"] : "",
                        m_StringLocalizer[string.Format("fly_command:get:yourself:{0}",
                        m_FlySystem.IsInFlyMode(user.SteamId) ? "enabled" : "disabled")]));
                    return;
                }
                if (Context.Parameters.Length != 1)
                    throw new CommandWrongUsageException(Context);
                if (await CheckPermissionAsync("other") != PermissionGrantResult.Grant)
                    throw new NotEnoughPermissionException(Context, "other");
                if (!Context.Parameters.TryGet(0, out UnturnedUser? targetUser) || targetUser == null)
                    throw new UserFriendlyException(string.Format("{0}{1}",
                        config.MessagePrefix ? m_StringLocalizer["fly_command:prefix"] : "",
                        m_StringLocalizer["fly_command:error_player"]));
                SteamPlayer sPlayer = targetUser.Player.SteamPlayer;
                CSteamID steamID = sPlayer.playerID.steamID;
                ushort? identity = m_IdentityManagerImplementation.GetIdentity(steamID);
                PrintAsync(string.Format("{0}{1}", config.MessagePrefix ? m_StringLocalizer["fly_command:prefix"] : "",
                    m_StringLocalizer[string.Format("fly_command:get:somebody:{0}",
                    m_FlySystem.IsInFlyMode(steamID) ? "enabled" : "disabled"), new
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
        [CommandDescription("Command to get state of player's fly modes.")]
        [CommandActor(typeof(ConsoleActor))]
        [CommandParent(typeof(FlyRoot))]
        #endregion Command Parameters
        public class GetConsole : UnturnedCommand
        {
            #region Member Variables
            private readonly IFlySystem m_FlySystem;
            private readonly IStringLocalizer m_StringLocalizer;
            private readonly IIdentityManagerImplementation m_IdentityManagerImplementation;
            #endregion Member Variables

            #region Class Constructor
            public GetConsole(
                IFlySystem flySystem,
                IStringLocalizer stringLocalizer,
                IIdentityManagerImplementation identityManagerImplementation,
                IServiceProvider serviceProvider) : base(serviceProvider)
            {
                m_FlySystem = flySystem;
                m_StringLocalizer = stringLocalizer;
                m_IdentityManagerImplementation = identityManagerImplementation;
            }
            #endregion Class Constructor

            protected override async UniTask OnExecuteAsync()
            {
                if (Context.Parameters.Length != 1)
                    throw new CommandWrongUsageException(Context);
                if (!Context.Parameters.TryGet(0, out UnturnedUser? user) || user == null)
                    throw new UserFriendlyException(m_StringLocalizer["fly_command:error_player"]);
                SteamPlayer sPlayer = user.Player.SteamPlayer;
                CSteamID steamID = sPlayer.playerID.steamID;
                ushort? identity = m_IdentityManagerImplementation.GetIdentity(steamID);
                bool result = m_FlySystem.IsInFlyMode(steamID);
                PrintAsync(m_StringLocalizer[string.Format("fly_command:get:somebody:{0}", result ? "enabled" : "disabled"), new
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

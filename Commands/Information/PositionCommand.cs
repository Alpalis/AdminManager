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

namespace Alpalis.AdminManager.Commands.Information
{
    public class PositionCommand
    {
        #region Command Parameters
        [Command("position")]
        [CommandSyntax("[player]")]
        [CommandDescription("Command that shows your or somebody's position.")]
        [RegisterCommandPermission("other", Description = "Allows to get position of other player.")]
        [CommandActor(typeof(UnturnedUser))]
        #endregion Command Parameters
        public class PositionUnturned : UnturnedCommand
        {
            #region Member Variables
            private readonly Main m_Plugin;
            private readonly IConfigurationManager m_ConfigurationManager;
            private readonly IStringLocalizer m_StringLocalizer;
            private readonly IAdminSystem m_AdminSystem;
            private readonly IIdentityManagerImplementation m_IdentityManagerImplementation;
            #endregion Member Variables

            #region Class Constructor
            public PositionUnturned(
                IPluginAccessor<Main> plugin,
                IConfigurationManager configurationManager,
                IStringLocalizer stringLocalizer,
                IAdminSystem adminSystem,
                IIdentityManagerImplementation identityManagerImplementation,
                IServiceProvider serviceProvider) : base(serviceProvider)
            {
                m_Plugin = plugin.Instance!;
                m_ConfigurationManager = configurationManager;
                m_StringLocalizer = stringLocalizer;
                m_AdminSystem = adminSystem;
                m_IdentityManagerImplementation = identityManagerImplementation;
            }
            #endregion Class Constructor

            protected override async UniTask OnExecuteAsync()
            {
                Config config = m_ConfigurationManager.GetConfig<Config>(m_Plugin);
                UnturnedUser user = (UnturnedUser)Context.Actor;
                if (!m_AdminSystem.IsInAdminMode(user))
                    throw new UserFriendlyException(string.Format("{0}{1}",
                         config.MessagePrefix ? m_StringLocalizer["position_command:prefix"] : "",
                         m_StringLocalizer["position_command:error_adminmode"]));
                if (Context.Parameters.Count == 0)
                {
                    PrintAsync(m_StringLocalizer["position_command:succeed:yourself", new
                    {
                        X = user.Player.Transform.Position.X,
                        Y = user.Player.Transform.Position.Y,
                        Z = user.Player.Transform.Position.Z
                    }]);
                    return;
                }
                if (Context.Parameters.Count != 1)
                    throw new CommandWrongUsageException(Context);
                if (await CheckPermissionAsync("other") != PermissionGrantResult.Grant)
                    throw new NotEnoughPermissionException(Context, "other");
                if (!Context.Parameters.TryGet(0, out UnturnedUser? targetUser) || targetUser == null)
                    throw new UserFriendlyException(string.Format("{0}{1}",
                         config.MessagePrefix ? m_StringLocalizer["position_command:prefix"] : "",
                         m_StringLocalizer["position_command:error_player"]));
                SteamPlayer targetSPlayer = targetUser.Player.SteamPlayer;
                CSteamID targetSteamID = targetSPlayer.playerID.steamID;
                ushort? targetIdentity = m_IdentityManagerImplementation.GetIdentity(targetSteamID);
                PrintAsync(string.Format("{0}{1}",
                         config.MessagePrefix ? m_StringLocalizer["position_command:prefix"] : "",
                         m_StringLocalizer["position_command:succeed:somebody", new
                         {
                             X = targetUser.Player.Transform.Position.X,
                             Y = targetUser.Player.Transform.Position.Y,
                             Z = targetUser.Player.Transform.Position.Z,
                             PlayerName = targetSPlayer.playerID.playerName,
                             CharacterName = targetSPlayer.playerID.characterName,
                             NickName = targetSPlayer.playerID.nickName,
                             SteamID = targetSteamID,
                             ID = targetIdentity
                         }]));
            }
        }

        #region Command Parameters
        [Command("position")]
        [CommandSyntax("<player>")]
        [CommandDescription("Command that shows somebody's position.")]
        [CommandActor(typeof(ConsoleActor))]
        #endregion Command Parameters
        public class PositionConsole : UnturnedCommand
        {
            #region Member Variables
            private readonly IStringLocalizer m_StringLocalizer;
            private readonly IIdentityManagerImplementation m_IdentityManagerImplementation;
            #endregion Member Variables

            #region Class Constructor
            public PositionConsole(
                IStringLocalizer stringLocalizer,
                IIdentityManagerImplementation identityManagerImplementation,
                IServiceProvider serviceProvider) : base(serviceProvider)
            {
                m_StringLocalizer = stringLocalizer;
                m_IdentityManagerImplementation = identityManagerImplementation;
            }
            #endregion Class Constructor

            protected override async UniTask OnExecuteAsync()
            {
                if (Context.Parameters.Length != 1)
                    throw new CommandWrongUsageException(Context);
                if (!Context.Parameters.TryGet(0, out UnturnedUser? targetUser) || targetUser == null)
                    throw new UserFriendlyException(m_StringLocalizer["position_command:error_player"]);
                SteamPlayer targetSPlayer = targetUser.Player.SteamPlayer;
                CSteamID targetSteamID = targetSPlayer.playerID.steamID;
                ushort? targetIdentity = m_IdentityManagerImplementation.GetIdentity(targetSteamID);
                PrintAsync(m_StringLocalizer["position_command:succeed:somebody", new
                {
                    X = targetUser.Player.Transform.Position.X,
                    Y = targetUser.Player.Transform.Position.Y,
                    Z = targetUser.Player.Transform.Position.Z,
                    PlayerName = targetSPlayer.playerID.playerName,
                    CharacterName = targetSPlayer.playerID.characterName,
                    NickName = targetSPlayer.playerID.nickName,
                    SteamID = targetSteamID,
                    ID = targetIdentity
                }]);
            }
        }
    }
}

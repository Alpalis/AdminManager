using Alpalis.AdminManager.API;
using Cysharp.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OpenMod.API.Commands;
using OpenMod.API.Permissions;
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
    public class RotationCommand
    {
        [Command("rotation")]
        [CommandSyntax("[player]")]
        [CommandDescription("Shows your or somebody's rotation.")]
        [RegisterCommandPermission("other", Description = "Allows to get rotation of other player.")]
        [CommandActor(typeof(UnturnedUser))]
        public class Unturned : UnturnedCommand
        {
            private readonly IStringLocalizer m_StringLocalizer;
            private readonly IAdminSystem m_AdminSystem;

            public Unturned(
                IStringLocalizer stringLocalizer,
                IAdminSystem adminSystem,
                IServiceProvider serviceProvider) : base(serviceProvider)
            {
                m_StringLocalizer = stringLocalizer;
                m_AdminSystem = adminSystem;
            }

            protected override async UniTask OnExecuteAsync()
            {
                UnturnedUser user = (UnturnedUser)Context.Actor;
                if (!m_AdminSystem.IsInAdminMode(user))
                    throw new UserFriendlyException(string.Format("{0}{1}",
                         m_StringLocalizer["rotation_command:prefix"],
                         m_StringLocalizer["rotation_command:error_adminmode"]));
                if (Context.Parameters.Count == 0)
                {
                    PrintAsync(m_StringLocalizer["rotation_command:succeed:yourself", new
                    {
                        user.Player.Transform.Rotation.X,
                        user.Player.Transform.Rotation.Y,
                        user.Player.Transform.Rotation.Z,
                        user.Player.Transform.Rotation.W
                    }]);
                    return;
                }
                if (Context.Parameters.Count != 1)
                    throw new CommandWrongUsageException(Context);
                if (await CheckPermissionAsync("other") != PermissionGrantResult.Grant)
                    throw new NotEnoughPermissionException(Context, "other");
                if (!Context.Parameters.TryGet(0, out UnturnedUser? targetUser) || targetUser == null)
                    throw new UserFriendlyException(string.Format("{0}{1}",
                         m_StringLocalizer["rotation_command:prefix"],
                         m_StringLocalizer["rotation_command:error_player"]));
                SteamPlayer targetSPlayer = targetUser.Player.SteamPlayer;
                CSteamID targetSteamID = targetSPlayer.playerID.steamID;
                PrintAsync(string.Format("{0}{1}",
                         m_StringLocalizer["rotation_command:prefix"],
                         m_StringLocalizer["rotation_command:succeed:somebody", new
                         {
                             targetUser.Player.Transform.Rotation.X,
                             targetUser.Player.Transform.Rotation.Y,
                             targetUser.Player.Transform.Rotation.Z,
                             targetUser.Player.Transform.Rotation.W,
                             PlayerName = targetSPlayer.playerID.playerName,
                             CharacterName = targetSPlayer.playerID.characterName,
                             NickName = targetSPlayer.playerID.nickName,
                             SteamID = targetSteamID
                         }]));
            }
        }

        [Command("rotation")]
        [CommandSyntax("<player>")]
        [CommandDescription("Shows somebody's rotation.")]
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
                if (Context.Parameters.Length != 1)
                    throw new CommandWrongUsageException(Context);
                if (!Context.Parameters.TryGet(0, out UnturnedUser? targetUser) || targetUser == null)
                    throw new UserFriendlyException(m_StringLocalizer["rotation_command:error_player"]);
                SteamPlayer targetSPlayer = targetUser.Player.SteamPlayer;
                CSteamID targetSteamID = targetSPlayer.playerID.steamID;
                PrintAsync(m_StringLocalizer["rotation_command:succeed:somebody", new
                {
                    targetUser.Player.Transform.Rotation.X,
                    targetUser.Player.Transform.Rotation.Y,
                    targetUser.Player.Transform.Rotation.Z,
                    targetUser.Player.Transform.Rotation.W,
                    PlayerName = targetSPlayer.playerID.playerName,
                    CharacterName = targetSPlayer.playerID.characterName,
                    NickName = targetSPlayer.playerID.nickName,
                    SteamID = targetSteamID
                }]);
            }
        }
    }
}

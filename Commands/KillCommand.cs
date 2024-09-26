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
using System;

namespace Alpalis.AdminManager.Commands;

public sealed class KillCommand
{
    [Command("kill")]
    [CommandSyntax("[player]")]
    [CommandDescription("Kills you or another player.")]
    [RegisterCommandPermission("other", Description = "Allows to kill other player.")]
    [CommandActor(typeof(UnturnedUser))]
    public sealed class Unturned(
        IStringLocalizer StringLocalizer,
        IAdminSystem adminSystem,
        IServiceProvider serviceProvider) : UnturnedCommand(serviceProvider)
    {
        private readonly IStringLocalizer m_StringLocalizer = StringLocalizer;
        private readonly IAdminSystem m_AdminSystem = adminSystem;

        protected override async UniTask OnExecuteAsync()
        {
            if (Context.Parameters.Count != 0 && Context.Parameters.Count != 1)
                throw new CommandWrongUsageException(Context);
            UnturnedUser user = (UnturnedUser)Context.Actor;
            if (!m_AdminSystem.IsInAdminMode(user))
                throw new UserFriendlyException(string.Format("{0}{1}",
                     m_StringLocalizer["kill_command:prefix"],
                     m_StringLocalizer["kill_command:error_adminmode"]));
            if (Context.Parameters.Count == 0)
            {
                await user.Player.KillAsync();
                await user.PrintMessageAsync(string.Format("{0}{1}",
                     m_StringLocalizer["kill_command:prefix"],
                     m_StringLocalizer["kill_command:yourself"]));
                return;
            }
            if (await CheckPermissionAsync("other") != PermissionGrantResult.Grant)
                throw new NotEnoughPermissionException(Context, "other");
            if (!Context.Parameters.TryGet(0, out UnturnedUser? targetUser) || targetUser == null)
                throw new UserFriendlyException(string.Format("{0}{1}",
                    m_StringLocalizer["kill_command:prefix"],
                    m_StringLocalizer["kill_command:error_player"]));
            await targetUser.Player.KillAsync();
            SteamPlayer sPlayer = user.Player.SteamPlayer;
            SteamPlayer targetSPlayer = targetUser.Player.SteamPlayer;
            await targetUser.PrintMessageAsync(string.Format("{0}{1}",
                     m_StringLocalizer["kill_command:prefix"],
                     m_StringLocalizer["kill_command:somebody:player", new
                     {
                         PlayerName = sPlayer.playerID.playerName,
                         CharacterName = sPlayer.playerID.characterName,
                         NickName = sPlayer.playerID.nickName,
                         SteamID = user.SteamId
                     }]));
            await PrintAsync(string.Format("{0}{1}",
                     m_StringLocalizer["kill_command:prefix"],
                     m_StringLocalizer["kill_command:somebody:executor", new
                     {
                         PlayerName = targetSPlayer.playerID.playerName,
                         CharacterName = targetSPlayer.playerID.characterName,
                         NickName = targetSPlayer.playerID.nickName,
                         SteamID = targetUser.SteamId
                     }]));
        }
    }

    [Command("kill")]
    [CommandSyntax("<player>")]
    [CommandDescription("Kills another player.")]
    [CommandActor(typeof(ConsoleActor))]
    public sealed class Console(
        IStringLocalizer StringLocalizer,
        IServiceProvider serviceProvider) : UnturnedCommand(serviceProvider)
    {
        private readonly IStringLocalizer m_StringLocalizer = StringLocalizer;

        protected override async UniTask OnExecuteAsync()
        {
            if (Context.Parameters.Count != 1)
                throw new CommandWrongUsageException(Context);
            if (!Context.Parameters.TryGet(0, out UnturnedUser? targetUser) || targetUser == null)
                throw new UserFriendlyException(m_StringLocalizer["kill_command:error_player"]);
            await targetUser.Player.KillAsync();
            SteamPlayer targetSPlayer = targetUser.Player.SteamPlayer;
            await targetUser.PrintMessageAsync(string.Format("{0}{1}", m_StringLocalizer["kill_command:prefix"],
                m_StringLocalizer["kill_command:somebody:console"]));
            await PrintAsync(m_StringLocalizer["kill_command:somebody:executor", new
            {
                PlayerName = targetSPlayer.playerID.playerName,
                CharacterName = targetSPlayer.playerID.characterName,
                NickName = targetSPlayer.playerID.nickName,
                SteamID = targetUser.SteamId
            }]);
        }
    }
}

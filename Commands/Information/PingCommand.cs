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

namespace Alpalis.AdminManager.Commands.Information;

public sealed class PingCommand
{
    [Command("ping")]
    [CommandSyntax("[player]")]
    [CommandDescription("Shows your or somebody's ping.")]
    [RegisterCommandPermission("other", Description = "Allows to get ping of other player.")]
    [CommandActor(typeof(UnturnedUser))]
    public sealed class Unturned(
        IStringLocalizer stringLocalizer,
        IServiceProvider serviceProvider) : UnturnedCommand(serviceProvider)
    {
        private readonly IStringLocalizer m_StringLocalizer = stringLocalizer;

        protected override async UniTask OnExecuteAsync()
        {
            UnturnedUser user = (UnturnedUser)Context.Actor;
            if (Context.Parameters.Count == 0)
            {
                await PrintAsync(string.Format("{0}{1}",
                     m_StringLocalizer["ping_command:prefix"],
                     m_StringLocalizer["ping_command:succeed:yourself", new
                     {
                         Ping = (user.Player.SteamPlayer.ping * 1000).ToString("0.00")
                     }]));
                return;
            }
            if (Context.Parameters.Count != 1)
                throw new CommandWrongUsageException(Context);
            if (await CheckPermissionAsync("other") != PermissionGrantResult.Grant)
                throw new NotEnoughPermissionException(Context, "other");
            if (!Context.Parameters.TryGet(0, out UnturnedUser? targetUser) || targetUser == null)
                throw new UserFriendlyException(string.Format("{0}{1}",
                     m_StringLocalizer["ping_command:prefix"],
                     m_StringLocalizer["ping_command:error_player"]));
            SteamPlayer targetSPlayer = targetUser.Player.SteamPlayer;
            CSteamID targetSteamID = targetSPlayer.playerID.steamID;
            await PrintAsync(string.Format("{0}{1}",
                     m_StringLocalizer["ping_command:prefix"],
                     m_StringLocalizer["ping_command:succeed:somebody", new
                     {
                         Ping = (targetSPlayer.ping * 1000).ToString("0.00"),
                         PlayerName = targetSPlayer.playerID.playerName,
                         CharacterName = targetSPlayer.playerID.characterName,
                         NickName = targetSPlayer.playerID.nickName,
                         SteamID = targetSteamID
                     }]));
        }
    }

    [Command("ping")]
    [CommandSyntax("<player>")]
    [CommandDescription("Shows somebody's ping.")]
    [CommandActor(typeof(ConsoleActor))]
    public sealed class Console(
        IStringLocalizer stringLocalizer,
        IServiceProvider serviceProvider) : UnturnedCommand(serviceProvider)
    {
        private readonly IStringLocalizer m_StringLocalizer = stringLocalizer;

        protected override async UniTask OnExecuteAsync()
        {
            if (Context.Parameters.Length != 1)
                throw new CommandWrongUsageException(Context);
            if (!Context.Parameters.TryGet(0, out UnturnedUser? targetUser) || targetUser == null)
                throw new UserFriendlyException(m_StringLocalizer["ping_command:error_player"]);
            SteamPlayer targetSPlayer = targetUser.Player.SteamPlayer;
            CSteamID targetSteamID = targetSPlayer.playerID.steamID;
            await PrintAsync(m_StringLocalizer["ping_command:succeed:somebody", new
            {
                Ping = (targetSPlayer.ping * 1000).ToString("0.00"),
                PlayerName = targetSPlayer.playerID.playerName,
                CharacterName = targetSPlayer.playerID.characterName,
                NickName = targetSPlayer.playerID.nickName,
                SteamID = targetSteamID
            }]);
        }
    }
}

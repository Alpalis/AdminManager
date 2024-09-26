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

namespace Alpalis.AdminManager.Commands;

public sealed class RequestUrlCommand
{
    [Command("requesturl")]
    [CommandSyntax("<url> [player] [message]")]
    [CommandDescription("Sends a request to player with clickable URL.")]
    [RegisterCommandPermission("other", Description = "Allows to send a request to another player.")]
    [CommandActor(typeof(UnturnedUser))]
    public sealed class RequestUrlUnturned(
        IAdminSystem adminSystem,
        IStringLocalizer stringLocalizer,
        IServiceProvider serviceProvider) : UnturnedCommand(serviceProvider)
    {
        private readonly IAdminSystem m_AdminSystem = adminSystem;
        private readonly IStringLocalizer m_StringLocalizer = stringLocalizer;

        protected override async UniTask OnExecuteAsync()
        {
            if (Context.Parameters.Count < 1 || Context.Parameters.Count > 3)
                throw new CommandWrongUsageException(Context);
            UnturnedUser user = (UnturnedUser)Context.Actor;
            if (!Context.Parameters.TryGet(0, out string? url) || url == null)
                throw new UserFriendlyException(string.Format("{0}{1}",
                     m_StringLocalizer["requesturl_command:prefix"],
                     m_StringLocalizer["requesturl_command:error_url"]));
            if (Context.Parameters.Count == 1)
            {
                await UniTask.SwitchToMainThread();
                user.Player.Player.sendBrowserRequest(string.Empty, url);
                await PrintAsync(string.Format("{0}{1}",
                    m_StringLocalizer["requesturl_command:prefix"],
                    m_StringLocalizer["requesturl_command:succeed:yourself"]));
                return;
            }
            if (await CheckPermissionAsync("other") != PermissionGrantResult.Grant)
                throw new NotEnoughPermissionException(Context, "other");
            if (!m_AdminSystem.IsInAdminMode(user))
                throw new UserFriendlyException(string.Format("{0}{1}",
                     m_StringLocalizer["requesturl_command:prefix"],
                     m_StringLocalizer["requesturl_command:error_adminmode"]));
            if (!Context.Parameters.TryGet(1, out UnturnedUser? targetUser) || targetUser == null)
                throw new UserFriendlyException(string.Format("{0}{1}",
                    m_StringLocalizer["requesturl_command:prefix"],
                    m_StringLocalizer["requesturl_command:error_player"]));
            string message = Context.Parameters.Count == 3 ? Context.Parameters[2] : string.Empty;
            SteamPlayer targetSPlayer = targetUser.Player.SteamPlayer;
            CSteamID targetSteamID = targetSPlayer.playerID.steamID;
            SteamPlayer sPlayer = user.Player.SteamPlayer;
            CSteamID steamID = sPlayer.playerID.steamID;
            await UniTask.SwitchToMainThread();
            targetUser.Player.Player.sendBrowserRequest(string.Empty, url);
            await targetUser.PrintMessageAsync(string.Format("{0}{1}", m_StringLocalizer["requesturl_command:prefix"],
                m_StringLocalizer[string.Format("requesturl_command:succeed:somebody:player:{0}",
                Context.Parameters.Count == 3 ? "withmessage" : "withoutmessage"), new
                {
                    PlayerName = sPlayer.playerID.playerName,
                    CharacterName = sPlayer.playerID.characterName,
                    NickName = sPlayer.playerID.nickName,
                    SteamID = steamID,
                    Message = message,
                    URL = url
                }]));
            await PrintAsync(string.Format("{0}{1}", m_StringLocalizer["requesturl_command:prefix"],
                m_StringLocalizer[string.Format("requesturl_command:succeed:somebody:executor:{0}",
                Context.Parameters.Count == 3 ? "withmessage" : "withoutmessage"), new
                {
                    PlayerName = targetSPlayer.playerID.playerName,
                    CharacterName = targetSPlayer.playerID.characterName,
                    NickName = targetSPlayer.playerID.nickName,
                    SteamID = targetSteamID,
                    Message = message,
                    URL = url
                }]));
        }
    }

    [Command("requesturl")]
    [CommandSyntax("<url> <player> [message]")]
    [CommandDescription("Sends a request to player with clickable URL.")]
    [CommandActor(typeof(ConsoleActor))]
    public sealed class RequestUrlConsole(
        IStringLocalizer stringLocalizer,
        IServiceProvider serviceProvider) : UnturnedCommand(serviceProvider)
    {
        private readonly IStringLocalizer m_StringLocalizer = stringLocalizer;

        protected override async UniTask OnExecuteAsync()
        {
            if (Context.Parameters.Count != 2 && Context.Parameters.Count != 3)
                throw new CommandWrongUsageException(Context);
            if (!Context.Parameters.TryGet(0, out string? url) || url == null)
                throw new UserFriendlyException(m_StringLocalizer["requesturl_command:error_url"]);
            if (!Context.Parameters.TryGet(1, out UnturnedUser? targetUser) || targetUser == null)
                throw new UserFriendlyException(m_StringLocalizer["requesturl_command:error_player"]);
            string message = Context.Parameters.Count == 3 ? Context.Parameters[2] : string.Empty;
            SteamPlayer targetSPlayer = targetUser.Player.SteamPlayer;
            CSteamID targetSteamID = targetSPlayer.playerID.steamID;
            await UniTask.SwitchToMainThread();
            targetUser.Player.Player.sendBrowserRequest(string.Empty, url);
            await targetUser.PrintMessageAsync(string.Format("{0}{1}", m_StringLocalizer["requesturl_command:prefix"],
                m_StringLocalizer[string.Format("requesturl_command:succeed:somebody:console:{0}",
                Context.Parameters.Count == 3 ? "withmessage" : "withoutmessage"), new
                {
                    Message = message,
                    URL = url
                }]));
            await PrintAsync(m_StringLocalizer[string.Format("requesturl_command:succeed:somebody:executor:{0}",
                Context.Parameters.Count == 3 ? "withmessage" : "withoutmessage"), new
                {
                    PlayerName = targetSPlayer.playerID.playerName,
                    CharacterName = targetSPlayer.playerID.characterName,
                    NickName = targetSPlayer.playerID.nickName,
                    SteamID = targetSteamID,
                    Message = message,
                    URL = url
                }]);
        }
    }
}

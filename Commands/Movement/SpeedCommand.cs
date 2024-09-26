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

namespace Alpalis.AdminManager.Commands.Movement;

public sealed class SpeedCommand
{
    [Command("speed")]
    [CommandSyntax("<multipler> [player]")]
    [CommandDescription("Sets speed.")]
    [RegisterCommandPermission("other", Description = "Allows to set speed of other player.")]
    [CommandActor(typeof(UnturnedUser))]
    public sealed class Unturned(
        IAdminSystem adminSystem,
        IStringLocalizer stringLocalizer,
        IServiceProvider serviceProvider) : UnturnedCommand(serviceProvider)
    {
        private readonly IAdminSystem m_AdminSystem = adminSystem;
        private readonly IStringLocalizer m_StringLocalizer = stringLocalizer;

        protected override async UniTask OnExecuteAsync()
        {
            UnturnedUser user = (UnturnedUser)Context.Actor;
            if (!m_AdminSystem.IsInAdminMode(user))
                throw new UserFriendlyException(string.Format("{0}{1}",
                     m_StringLocalizer["speed_command:prefix"],
                     m_StringLocalizer["speed_command:error_adminmode"]));
            if (Context.Parameters.Count != 1 && Context.Parameters.Count != 2)
                throw new CommandWrongUsageException(Context);
            if (!Context.Parameters.TryGet(0, out float multipler))
                throw new UserFriendlyException(string.Format("{0}{1}",
                    m_StringLocalizer["speed_command:prefix"],
                    m_StringLocalizer["speed_command:error_multipler"]));
            if (Context.Parameters.Count == 1)
            {
                await UniTask.SwitchToMainThread();
                user.Player.Player.movement.sendPluginSpeedMultiplier(multipler);
                await PrintAsync(string.Format("{0}{1}",
                    m_StringLocalizer["speed_command:prefix"],
                    m_StringLocalizer["speed_command:yourself", new { Multipler = multipler }]));
                return;
            }
            if (await CheckPermissionAsync("other") != PermissionGrantResult.Grant)
                throw new NotEnoughPermissionException(Context, "other");
            if (!Context.Parameters.TryGet(1, out UnturnedUser? targetUser) || targetUser == null)
                throw new UserFriendlyException(string.Format("{0}{1}",
                    m_StringLocalizer["speed_command:prefix"],
                    m_StringLocalizer["speed_command:error_player"]));
            SteamPlayer targetSPlayer = targetUser.Player.SteamPlayer;
            CSteamID targetSteamID = targetSPlayer.playerID.steamID;
            SteamPlayer sPlayer = user.Player.SteamPlayer;
            CSteamID steamID = sPlayer.playerID.steamID;
            await UniTask.SwitchToMainThread();
            targetUser.Player.Player.movement.sendPluginSpeedMultiplier(multipler);
            await targetUser.PrintMessageAsync(string.Format("{0}{1}",
                m_StringLocalizer["speed_command:prefix"],
                m_StringLocalizer["speed_command:somebody:player", new
                {
                    PlayerName = sPlayer.playerID.playerName,
                    CharacterName = sPlayer.playerID.characterName,
                    NickName = sPlayer.playerID.nickName,
                    SteamID = steamID,
                    Multipler = multipler
                }]));
            await PrintAsync(string.Format("{0}{1}",
                m_StringLocalizer["speed_command:prefix"],
                m_StringLocalizer["speed_command:somebody:executor", new
                {
                    PlayerName = targetSPlayer.playerID.playerName,
                    CharacterName = targetSPlayer.playerID.characterName,
                    NickName = targetSPlayer.playerID.nickName,
                    SteamID = targetSteamID,
                    Multipler = multipler
                }]));
        }
    }

    [Command("speed")]
    [CommandSyntax("<multipler> <player>")]
    [CommandDescription("Set speed.")]
    [CommandActor(typeof(ConsoleActor))]
    public sealed class Console(
        IStringLocalizer stringLocalizer,
        IServiceProvider serviceProvider) : UnturnedCommand(serviceProvider)
    {
        private readonly IStringLocalizer m_StringLocalizer = stringLocalizer;

        protected override async UniTask OnExecuteAsync()
        {
            if (Context.Parameters.Count != 2)
                throw new CommandWrongUsageException(Context);
            if (!Context.Parameters.TryGet(0, out float multipler))
                throw new UserFriendlyException(m_StringLocalizer["speed_command:error_multipler"]);
            if (!Context.Parameters.TryGet(1, out UnturnedUser? user) || user == null)
                throw new UserFriendlyException(m_StringLocalizer["speed_command:error_player"]);
            SteamPlayer sPlayer = user.Player.SteamPlayer;
            CSteamID steamID = sPlayer.playerID.steamID;
            await UniTask.SwitchToMainThread();
            user.Player.Player.movement.sendPluginSpeedMultiplier(multipler);
            await user.PrintMessageAsync(string.Format("{0}{1}",
                m_StringLocalizer["speed_command:prefix"],
                m_StringLocalizer["speed_command:somebody:console", new { Multipler = multipler }]));
            await PrintAsync(m_StringLocalizer["speed_command:somebody:executor", new
            {
                PlayerName = sPlayer.playerID.playerName,
                CharacterName = sPlayer.playerID.characterName,
                NickName = sPlayer.playerID.nickName,
                SteamID = steamID,
                Multipler = multipler
            }]);
        }
    }
}

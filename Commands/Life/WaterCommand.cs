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

namespace Alpalis.AdminManager.Commands.Life;

public sealed class WaterCommand
{
    [Command("water")]
    [CommandSyntax("[player]")]
    [CommandDescription("Allows to set max value of your or somebody's water.")]
    [RegisterCommandPermission("other", Description = "Allows to set max value of other player's water.")]
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
                     m_StringLocalizer["water_command:prefix"],
                     m_StringLocalizer["water_command:error_adminmode"]));
            if (Context.Parameters.Count == 0)
            {
                await UniTask.SwitchToMainThread();
                user.Player.Player.life.serverModifyWater(100);
                await PrintAsync(string.Format("{0}{1}",
                    m_StringLocalizer["water_command:prefix"],
                    m_StringLocalizer["water_command:yourself"]));
                return;
            }
            if (Context.Parameters.Count != 1)
                throw new CommandWrongUsageException(Context);
            if (await CheckPermissionAsync("other") != PermissionGrantResult.Grant)
                throw new NotEnoughPermissionException(Context, "other");
            if (!Context.Parameters.TryGet(0, out UnturnedUser? targetUser) || targetUser == null)
                throw new UserFriendlyException(string.Format("{0}{1}",
                    m_StringLocalizer["water_command:prefix"],
                    m_StringLocalizer["water_command:error_player"]));
            SteamPlayer targetSPlayer = targetUser.Player.SteamPlayer;
            CSteamID targetSteamID = targetSPlayer.playerID.steamID;
            SteamPlayer sPlayer = user.Player.SteamPlayer;
            CSteamID steamID = sPlayer.playerID.steamID;
            await UniTask.SwitchToMainThread();
            targetUser.Player.Player.life.serverModifyWater(100);
            await targetUser.PrintMessageAsync(string.Format("{0}{1}",
                m_StringLocalizer["water_command:prefix"],
                m_StringLocalizer["water_command:somebody:player", new
                {
                    PlayerName = sPlayer.playerID.playerName,
                    CharacterName = sPlayer.playerID.characterName,
                    NickName = sPlayer.playerID.nickName,
                    SteamID = steamID
                }]));
            await PrintAsync(string.Format("{0}{1}",
                m_StringLocalizer["water_command:prefix"],
                m_StringLocalizer["water_command:somebody:executor", new
                {
                    PlayerName = targetSPlayer.playerID.playerName,
                    CharacterName = targetSPlayer.playerID.characterName,
                    NickName = targetSPlayer.playerID.nickName,
                    SteamID = targetSteamID
                }]));
        }
    }

    [Command("water")]
    [CommandSyntax("<player>")]
    [CommandDescription("Allows to set max value of somebody's water.")]
    [CommandActor(typeof(ConsoleActor))]
    public sealed class Console(
        IStringLocalizer stringLocalizer,
        IServiceProvider serviceProvider) : UnturnedCommand(serviceProvider)
    {
        private readonly IStringLocalizer m_StringLocalizer = stringLocalizer;

        protected override async UniTask OnExecuteAsync()
        {
            if (Context.Parameters.Count != 1)
                throw new CommandWrongUsageException(Context);
            if (!Context.Parameters.TryGet(0, out UnturnedUser? user) || user == null)
                throw new UserFriendlyException(m_StringLocalizer["water_command:error_player"]);
            SteamPlayer sPlayer = user.Player.SteamPlayer;
            CSteamID steamID = sPlayer.playerID.steamID;
            await UniTask.SwitchToMainThread();
            user.Player.Player.life.serverModifyWater(100);
            await user.PrintMessageAsync(string.Format("{0}{1}",
                m_StringLocalizer["water_command:prefix"],
                m_StringLocalizer["water_command:somebody:console"]));
            await PrintAsync(m_StringLocalizer["water_command:somebody:executor", new
            {
                PlayerName = sPlayer.playerID.playerName,
                CharacterName = sPlayer.playerID.characterName,
                NickName = sPlayer.playerID.nickName,
                SteamID = steamID
            }]);
        }
    }
}

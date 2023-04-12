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
    public class SalvageSpeedCommand
    {
        [Command("salvagespeed")]
        [CommandSyntax("<multipler> [player]")]
        [CommandDescription("Command to set speed of salvage.")]
        [RegisterCommandPermission("other", Description = "Allows to set salvage speed of other player.")]
        [CommandActor(typeof(UnturnedUser))]
        public class SalvageSpeedUnturned : UnturnedCommand
        {
            private readonly IAdminSystem m_AdminSystem;
            private readonly IStringLocalizer m_StringLocalizer;

            public SalvageSpeedUnturned(
                IAdminSystem adminSystem,
                IStringLocalizer stringLocalizer,
                IServiceProvider serviceProvider) : base(serviceProvider)
            {
                m_AdminSystem = adminSystem;
                m_StringLocalizer = stringLocalizer;
            }

            protected override async UniTask OnExecuteAsync()
            {
                UnturnedUser user = (UnturnedUser)Context.Actor;
                if (!m_AdminSystem.IsInAdminMode(user))
                    throw new UserFriendlyException(string.Format("{0}{1}",
                         m_StringLocalizer["salvagespeed_command:prefix"],
                         m_StringLocalizer["salvagespeed_command:error_adminmode"]));
                if (Context.Parameters.Count != 1 && Context.Parameters.Count != 2)
                    throw new CommandWrongUsageException(Context);
                if (!Context.Parameters.TryGet(0, out float multipler))
                    throw new UserFriendlyException(string.Format("{0}{1}",
                        m_StringLocalizer["salvagespeed_command:prefix"],
                        m_StringLocalizer["salvagespeed_command:error_multipler"]));
                if (Context.Parameters.Count == 1)
                {
                    await UniTask.SwitchToMainThread();
                    user.Player.Player.interact.sendSalvageTimeOverride(multipler);
                    PrintAsync(string.Format("{0}{1}",
                        m_StringLocalizer["salvagespeed_command:prefix"],
                        m_StringLocalizer["salvagespeed_command:yourself", new { Multipler = multipler }]));
                    return;
                }
                if (await CheckPermissionAsync("other") != PermissionGrantResult.Grant)
                    throw new NotEnoughPermissionException(Context, "other");
                if (!Context.Parameters.TryGet(1, out UnturnedUser? targetUser) || targetUser == null)
                    throw new UserFriendlyException(string.Format("{0}{1}",
                        m_StringLocalizer["salvagespeed_command:prefix"],
                        m_StringLocalizer["salvagespeed_command:error_player"]));
                SteamPlayer targetSPlayer = targetUser.Player.SteamPlayer;
                CSteamID targetSteamID = targetSPlayer.playerID.steamID;
                SteamPlayer sPlayer = user.Player.SteamPlayer;
                CSteamID steamID = sPlayer.playerID.steamID;
                await UniTask.SwitchToMainThread();
                targetUser.Player.Player.interact.sendSalvageTimeOverride(multipler);
                targetUser.PrintMessageAsync(string.Format("{0}{1}",
                    m_StringLocalizer["salvagespeed_command:prefix"],
                    m_StringLocalizer["salvagespeed_command:somebody:player", new
                    {
                        PlayerName = sPlayer.playerID.playerName,
                        CharacterName = sPlayer.playerID.characterName,
                        NickName = sPlayer.playerID.nickName,
                        SteamID = steamID,
                        Multipler = multipler
                    }]));
                PrintAsync(string.Format("{0}{1}",
                    m_StringLocalizer["salvagespeed_command:prefix"],
                    m_StringLocalizer["salvagespeed_command:somebody:executor", new
                    {
                        PlayerName = targetSPlayer.playerID.playerName,
                        CharacterName = targetSPlayer.playerID.characterName,
                        NickName = targetSPlayer.playerID.nickName,
                        SteamID = targetSteamID,
                        Multipler = multipler
                    }]));
            }
        }

        [Command("salvagespeed")]
        [CommandSyntax("<multipler> <player>")]
        [CommandDescription("Command to set speed of salvage.")]
        [CommandActor(typeof(ConsoleActor))]
        public class SpeedConsole : UnturnedCommand
        {
            private readonly IStringLocalizer m_StringLocalizer;

            public SpeedConsole(
                IStringLocalizer stringLocalizer,
                IServiceProvider serviceProvider) : base(serviceProvider)
            {
                m_StringLocalizer = stringLocalizer;
            }

            protected override async UniTask OnExecuteAsync()
            {
                if (Context.Parameters.Count != 2)
                    throw new CommandWrongUsageException(Context);
                if (!Context.Parameters.TryGet(0, out float multipler))
                    throw new UserFriendlyException(m_StringLocalizer["salvagespeed_command:error_multipler"]);
                if (!Context.Parameters.TryGet(1, out UnturnedUser? user) || user == null)
                    throw new UserFriendlyException(m_StringLocalizer["salvagespeed_command:error_player"]);
                SteamPlayer sPlayer = user.Player.SteamPlayer;
                CSteamID steamID = sPlayer.playerID.steamID;
                await UniTask.SwitchToMainThread();
                user.Player.Player.movement.sendPluginSpeedMultiplier(multipler);
                user.PrintMessageAsync(string.Format("{0}{1}",
                    m_StringLocalizer["salvagespeed_command:prefix"],
                    m_StringLocalizer["salvagespeed_command:somebody:console", new { Multipler = multipler }]));
                PrintAsync(m_StringLocalizer["salvagespeed_command:somebody:executor", new
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
}

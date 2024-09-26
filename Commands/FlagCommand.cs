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

public sealed class FlagCommand
{
    [Command("flag")]
    [CommandSyntax("<get/set>")]
    [CommandDescription("Manages flags.")]
    public sealed class Root(IServiceProvider serviceProvider) : UnturnedCommand(serviceProvider)
    {
        protected override UniTask OnExecuteAsync()
        {
            throw new CommandWrongUsageException(Context);
        }
    }

    [Command("set")]
    [CommandSyntax("<flag> <value> [player]")]
    [CommandDescription("Allows to set your or somebody's flag value.")]
    [CommandActor(typeof(UnturnedUser))]
    [CommandParent(typeof(Root))]
    [RegisterCommandPermission("other", Description = "Allows to set flag value of other player.")]
    public sealed class SetUnturned(
        IStringLocalizer stringLocalizer,
        IAdminSystem adminSystem,
        IServiceProvider serviceProvider) : UnturnedCommand(serviceProvider)
    {
        private readonly IStringLocalizer m_StringLocalizer = stringLocalizer;
        private readonly IAdminSystem m_AdminSystem = adminSystem;

        protected override async UniTask OnExecuteAsync()
        {
            if (Context.Parameters.Length > 3 || Context.Parameters.Length < 2)
                throw new CommandWrongUsageException(Context);
            UnturnedUser user = (UnturnedUser)Context.Actor;
            if (!m_AdminSystem.IsInAdminMode(user))
                throw new UserFriendlyException(string.Format("{0}{1}",
                     m_StringLocalizer["flag_command:prefix"],
                     m_StringLocalizer["flag_command:error_adminmode"]));
            if (!Context.Parameters.TryGet(0, out ushort flag))
                throw new UserFriendlyException(string.Format("{0}{1}",
                     m_StringLocalizer["flag_command:prefix"],
                     m_StringLocalizer["flag_command:error_flag"]));
            if (!Context.Parameters.TryGet(1, out short value))
                throw new UserFriendlyException(string.Format("{0}{1}",
                     m_StringLocalizer["flag_command:prefix"],
                     m_StringLocalizer["flag_command:error_value"]));

            if (Context.Parameters.Length == 2)
            {
                await UniTask.SwitchToMainThread();
                user.Player.Player.quests.sendSetFlag(flag, value);
                await PrintAsync(string.Format("{0}{1}",
                    m_StringLocalizer["flag_command:prefix"],
                    m_StringLocalizer["flag_command:set:succeed:yourself", new
                    {
                        Flag = flag,
                        Value = value
                    }]));
                return;
            }
            if (await CheckPermissionAsync("other") != PermissionGrantResult.Grant)
                throw new NotEnoughPermissionException(Context, "other");
            if (!Context.Parameters.TryGet(2, out UnturnedUser? targetUser) || targetUser == null)
                throw new UserFriendlyException(string.Format("{0}{1}",
                     m_StringLocalizer["flag_command:prefix"],
                     m_StringLocalizer["flag_command:error_player"]));

            await UniTask.SwitchToMainThread();
            SteamPlayer sPlayer = user.Player.SteamPlayer;
            targetUser.Player.Player.quests.sendSetFlag(flag, value);
            await targetUser.PrintMessageAsync(string.Format("{0}{1}",
                m_StringLocalizer["flag_command:prefix"],
                m_StringLocalizer["flag_command:set:succeed:somebody:player", new
                {
                    PlayerName = sPlayer.playerID.playerName,
                    CharacterName = sPlayer.playerID.characterName,
                    NickName = sPlayer.playerID.nickName,
                    SteamID = user.SteamId,
                    Flag = flag,
                    Value = value
                }]));
            SteamPlayer targetSPlayer = targetUser.Player.SteamPlayer;
            await PrintAsync(string.Format("{0}{1}",
                m_StringLocalizer["flag_command:prefix"],
                m_StringLocalizer["flag_command:set:succeed:yourself:executor", new
                {
                    PlayerName = targetSPlayer.playerID.playerName,
                    CharacterName = targetSPlayer.playerID.characterName,
                    NickName = targetSPlayer.playerID.nickName,
                    SteamID = targetUser.SteamId,
                    Flag = flag,
                    Value = value
                }]));
        }
    }

    [Command("set")]
    [CommandSyntax("<flag> <value> <player>")]
    [CommandDescription("Allows to set somebody's flag value.")]
    [CommandActor(typeof(ConsoleActor))]
    [CommandParent(typeof(Root))]
    public sealed class SetConsole(
        IStringLocalizer stringLocalizer,
        IServiceProvider serviceProvider) : UnturnedCommand(serviceProvider)
    {
        private readonly IStringLocalizer m_StringLocalizer = stringLocalizer;

        protected override async UniTask OnExecuteAsync()
        {
            if (Context.Parameters.Length != 3)
                throw new CommandWrongUsageException(Context);
            if (!Context.Parameters.TryGet(0, out ushort flag))
                throw new UserFriendlyException(m_StringLocalizer["flag_command:error_flag"]);
            if (!Context.Parameters.TryGet(1, out short value))
                throw new UserFriendlyException(m_StringLocalizer["flag_command:error_value"]);

            if (!Context.Parameters.TryGet(2, out UnturnedUser? targetUser) || targetUser == null)
                throw new UserFriendlyException(m_StringLocalizer["flag_command:error_player"]);
            await UniTask.SwitchToMainThread();
            targetUser.Player.Player.quests.sendSetFlag(flag, value);
            await targetUser.PrintMessageAsync(string.Format("{0}{1}",
                m_StringLocalizer["flag_command:prefix"],
                m_StringLocalizer["flag_command:set:succeed:somebody:console", new
                {
                    Flag = flag,
                    Value = value
                }]));
            SteamPlayer targetSPlayer = targetUser.Player.SteamPlayer;
            await PrintAsync(m_StringLocalizer["flag_command:set:succeed:yourself:executor", new
            {
                PlayerName = targetSPlayer.playerID.playerName,
                CharacterName = targetSPlayer.playerID.characterName,
                NickName = targetSPlayer.playerID.nickName,
                SteamID = targetUser.SteamId,
                Flag = flag,
                Value = value
            }]);
        }
    }

    [Command("get")]
    [CommandSyntax("<flag> [player]")]
    [CommandDescription("Allows to get your or somebody's flag value.")]
    [CommandActor(typeof(UnturnedUser))]
    [CommandParent(typeof(Root))]
    [RegisterCommandPermission("other", Description = "Allows to get flag value of other player.")]
    public sealed class GetUnturned(
        IStringLocalizer stringLocalizer,
        IAdminSystem adminSystem,
        IServiceProvider serviceProvider) : UnturnedCommand(serviceProvider)
    {
        private readonly IStringLocalizer m_StringLocalizer = stringLocalizer;
        private readonly IAdminSystem m_AdminSystem = adminSystem;

        protected override async UniTask OnExecuteAsync()
        {
            if (Context.Parameters.Length > 2 || Context.Parameters.Length < 1)
                throw new CommandWrongUsageException(Context);
            UnturnedUser user = (UnturnedUser)Context.Actor;
            if (!m_AdminSystem.IsInAdminMode(user))
                throw new UserFriendlyException(string.Format("{0}{1}",
                     m_StringLocalizer["flag_command:prefix"],
                     m_StringLocalizer["flag_command:error_adminmode"]));
            if (!Context.Parameters.TryGet(0, out ushort flag))
                throw new UserFriendlyException(string.Format("{0}{1}",
                     m_StringLocalizer["flag_command:prefix"],
                     m_StringLocalizer["flag_command:error_flag"]));
            short value;
            if (Context.Parameters.Length == 2)
            {
                await UniTask.SwitchToMainThread();
                if (!user.Player.Player.quests.getFlag(flag, out value))
                    throw new UserFriendlyException(string.Format("{0}{1}",
                        m_StringLocalizer["flag_command:prefix"],
                        m_StringLocalizer["flag_command:error_flag_missing"]));
                await PrintAsync(string.Format("{0}{1}",
                    m_StringLocalizer["flag_command:prefix"],
                    m_StringLocalizer["flag_command:get:succeed:yourself", new
                    {
                        Flag = flag,
                        Value = value
                    }]));
                return;
            }
            if (await CheckPermissionAsync("other") != PermissionGrantResult.Grant)
                throw new NotEnoughPermissionException(Context, "other");
            if (!Context.Parameters.TryGet(1, out UnturnedUser? targetUser) || targetUser == null)
                throw new UserFriendlyException(string.Format("{0}{1}",
                     m_StringLocalizer["flag_command:prefix"],
                     m_StringLocalizer["flag_command:error_player"]));

            await UniTask.SwitchToMainThread();
            if (!targetUser.Player.Player.quests.getFlag(flag, out value))
                throw new UserFriendlyException(string.Format("{0}{1}",
                    m_StringLocalizer["flag_command:prefix"],
                    m_StringLocalizer["flag_command:error_flag_missing"]));
            SteamPlayer targetSPlayer = targetUser.Player.SteamPlayer;
            await PrintAsync(string.Format("{0}{1}",
                m_StringLocalizer["flag_command:prefix"],
                m_StringLocalizer["flag_command:get:succeed:somebody", new
                {
                    PlayerName = targetSPlayer.playerID.playerName,
                    CharacterName = targetSPlayer.playerID.characterName,
                    NickName = targetSPlayer.playerID.nickName,
                    SteamID = targetUser.SteamId,
                    Flag = flag,
                    Value = value
                }]));
        }
    }

    [Command("get")]
    [CommandSyntax("<flag> <player>")]
    [CommandDescription("Allows to get somebody's flag value.")]
    [CommandActor(typeof(ConsoleActor))]
    [CommandParent(typeof(Root))]
    public sealed class GetConsole(
        IStringLocalizer stringLocalizer,
        IServiceProvider serviceProvider) : UnturnedCommand(serviceProvider)
    {
        private readonly IStringLocalizer m_StringLocalizer = stringLocalizer;

        protected override async UniTask OnExecuteAsync()
        {
            if (Context.Parameters.Length != 2)
                throw new CommandWrongUsageException(Context);
            if (!Context.Parameters.TryGet(0, out ushort flag))
                throw new UserFriendlyException(m_StringLocalizer["flag_command:error_flag"]);
            if (!Context.Parameters.TryGet(1, out UnturnedUser? targetUser) || targetUser == null)
                throw new UserFriendlyException(m_StringLocalizer["flag_command:error_player"]);
            await UniTask.SwitchToMainThread();
            if (!targetUser.Player.Player.quests.getFlag(flag, out short value))
                throw new UserFriendlyException(m_StringLocalizer["flag_command:error_flag_missing"]);
            SteamPlayer targetSPlayer = targetUser.Player.SteamPlayer;
            await PrintAsync(m_StringLocalizer["flag_command:get:succeed:somebody", new
            {
                PlayerName = targetSPlayer.playerID.playerName,
                CharacterName = targetSPlayer.playerID.characterName,
                NickName = targetSPlayer.playerID.nickName,
                SteamID = targetUser.SteamId,
                Flag = flag,
                Value = value
            }]);
        }
    }
}

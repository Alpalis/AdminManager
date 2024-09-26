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

public sealed class UIEffectCommand
{
    [Command("effectui")]
    [CommandAlias("effui")]
    [CommandDescription("Displays UI effect.")]
    [CommandSyntax("<id> <key> [player]")]
    [CommandActor(typeof(UnturnedUser))]
    [RegisterCommandPermission("other", Description = "Allows to display UI effect for other player.")]
    public sealed class Unturned(
        IStringLocalizer stringLocalizer,
        IAdminSystem adminSystem,
        IServiceProvider serviceProvider) : UnturnedCommand(serviceProvider)
    {
        private readonly IStringLocalizer m_StringLocalizer = stringLocalizer;
        private readonly IAdminSystem m_AdminSystem = adminSystem;

        protected override async UniTask OnExecuteAsync()
        {
            if (Context.Parameters.Count < 2 || Context.Parameters.Count > 3)
                throw new CommandWrongUsageException(Context);
            UnturnedUser user = (UnturnedUser)Context.Actor;
            if (!m_AdminSystem.IsInAdminMode(user))
                throw new UserFriendlyException(string.Format("{0}{1}",
                     m_StringLocalizer["effectui_command:prefix"],
                     m_StringLocalizer["effectui_command:error_adminmode"]));
            if (!Context.Parameters.TryGet(0, out ushort id))
                throw new UserFriendlyException(string.Format("{0}{1}",
                     m_StringLocalizer["effectui_command:prefix"],
                     m_StringLocalizer["effectui_command:error_id"]));
            if (!Context.Parameters.TryGet(1, out short key))
                throw new UserFriendlyException(string.Format("{0}{1}",
                     m_StringLocalizer["effectui_command:prefix"],
                     m_StringLocalizer["effectui_command:error_key"]));
            if (Context.Parameters.Count == 2)
            {
                await UniTask.SwitchToMainThread();
                EffectManager.sendUIEffect(id, key, user.Player.Player.channel.GetOwnerTransportConnection(), true);
                await PrintAsync(string.Format("{0}{1}",
                     m_StringLocalizer["effectui_command:prefix"],
                     m_StringLocalizer["effectui_command:succeed:yourself", new
                     {
                         EffectID = id,
                         EffectKey = key
                     }]));
                return;
            }
            if (await CheckPermissionAsync("other") != PermissionGrantResult.Grant)
                throw new NotEnoughPermissionException(Context, "other");
            if (!Context.Parameters.TryGet(2, out UnturnedUser? targetUser) || targetUser == null)
                throw new UserFriendlyException(string.Format("{0}{1}",
                    m_StringLocalizer["effectui_command:prefix"],
                    m_StringLocalizer["effectui_command:error_player"]));
            await UniTask.SwitchToMainThread();
            EffectManager.sendUIEffect(id, key, targetUser.Player.Player.channel.GetOwnerTransportConnection(), true);
            SteamPlayer targetSPlayer = targetUser.Player.SteamPlayer;
            CSteamID targetSteamID = targetSPlayer.playerID.steamID;
            SteamPlayer sPlayer = user.Player.SteamPlayer;
            CSteamID steamID = sPlayer.playerID.steamID;
            await targetUser.PrintMessageAsync(string.Format("{0}{1}",
                m_StringLocalizer["effectui_command:prefix"],
                m_StringLocalizer["effectui_command:succeed:somebody:player", new
                {
                    PlayerName = sPlayer.playerID.playerName,
                    CharacterName = sPlayer.playerID.characterName,
                    NickName = sPlayer.playerID.nickName,
                    SteamID = steamID,
                    EffectID = id,
                    EffectKey = key
                }]));
            await PrintAsync(string.Format("{0}{1}",
                m_StringLocalizer["effectui_command:prefix"],
                m_StringLocalizer["effectui_command:succeed:somebody:executor", new
                {
                    PlayerName = targetSPlayer.playerID.playerName,
                    CharacterName = targetSPlayer.playerID.characterName,
                    NickName = targetSPlayer.playerID.nickName,
                    SteamID = targetSteamID,
                    EffectID = id,
                    EffectKey = key
                }]));
        }
    }

    [Command("effectui")]
    [CommandAlias("effui")]
    [CommandDescription("Displays UI effect.")]
    [CommandSyntax("<id> <key> <player>")]
    [CommandActor(typeof(ConsoleActor))]
    public sealed class Console(
        IStringLocalizer stringLocalizer,
        IServiceProvider serviceProvider) : UnturnedCommand(serviceProvider)
    {
        private readonly IStringLocalizer m_StringLocalizer = stringLocalizer;

        protected override async UniTask OnExecuteAsync()
        {
            if (Context.Parameters.Count != 3)
                throw new CommandWrongUsageException(Context);
            if (!Context.Parameters.TryGet(0, out ushort id))
                throw new UserFriendlyException(m_StringLocalizer["effectui_command:error_id"]);
            if (!Context.Parameters.TryGet(1, out short key))
                throw new UserFriendlyException(m_StringLocalizer["effectui_command:error_key"]);
            if (!Context.Parameters.TryGet(2, out UnturnedUser? targetUser) || targetUser == null)
                throw new UserFriendlyException(m_StringLocalizer["effectui_command:error_player"]);
            await UniTask.SwitchToMainThread();
            EffectManager.sendUIEffect(id, key, targetUser.Player.Player.channel.GetOwnerTransportConnection(), true);
            SteamPlayer targetSPlayer = targetUser.Player.SteamPlayer;
            CSteamID targetSteamID = targetSPlayer.playerID.steamID;
            await targetUser.PrintMessageAsync(string.Format("{0}{1}",
                m_StringLocalizer["effectui_command:prefix"],
                m_StringLocalizer["effectui_command:succeed:somebody:console", new
                {
                    EffectID = id,
                    EffectKey = key
                }]));
            await PrintAsync(m_StringLocalizer["effectui_command:succeed:somebody:executor", new
            {
                PlayerName = targetSPlayer.playerID.playerName,
                CharacterName = targetSPlayer.playerID.characterName,
                NickName = targetSPlayer.playerID.nickName,
                SteamID = targetSteamID,
                EffectID = id,
                EffectKey = key
            }]);
        }
    }
}

using Alpalis.AdminManager.API;
using Alpalis.UtilityServices.Helpers;
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

public sealed class ItemCommand
{
    [Command("item")]
    [CommandAlias("i")]
    [CommandAlias("give")]
    [CommandSyntax("<item> [amount] [player]")]
    [CommandDescription("Gives you or somebody items.")]
    [CommandActor(typeof(UnturnedUser))]
    [RegisterCommandPermission("other", Description = "Allows to give items other people.")]
    public sealed class Unturned(
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
            if (!m_AdminSystem.IsInAdminMode(user))
                throw new UserFriendlyException(string.Format("{0}{1}",
                     m_StringLocalizer["item_command:prefix"],
                     m_StringLocalizer["item_command:error_adminmode"]));
            if (!Context.Parameters.TryGet(0, out string? itemName) || itemName == null ||
                !UnturnedAssetHelper.GetItem(itemName, out ItemAsset itemAsset))
                throw new UserFriendlyException(string.Format("{0}{1}",
                     m_StringLocalizer["item_command:prefix"],
                     m_StringLocalizer["item_command:error_null"]));
            Item item = new(itemAsset.id, EItemOrigin.ADMIN);
            await UniTask.SwitchToMainThread();
            if (Context.Parameters.Count == 1)
            {
                user.Player.Player.inventory.forceAddItem(item, true);
                await PrintAsync(string.Format("{0}{1}",
                     m_StringLocalizer["item_command:prefix"],
                     m_StringLocalizer["item_command:succeed:yourself:one",
                     new { ItemName = itemAsset.itemName, ItemID = itemAsset.id }]));
                return;
            }
            if (!Context.Parameters.TryGet(1, out ushort itemAmount) || itemAmount == 0)
                throw new UserFriendlyException(string.Format("{0}{1}",
                m_StringLocalizer["item_command:prefix"],
                m_StringLocalizer["item_command:error_null_number"]));
            if (itemAmount > 100)
                throw new UserFriendlyException(string.Format("{0}{1}",
                m_StringLocalizer["item_command:prefix"],
                m_StringLocalizer["item_command:error_high_number"]));
            if (Context.Parameters.Count == 2)
            {
                for (int i = 0; i < itemAmount; i++)
                    user.Player.Player.inventory.forceAddItem(item, true);
                await PrintAsync(string.Format("{0}{1}",
                     m_StringLocalizer["item_command:prefix"],
                     m_StringLocalizer["item_command:succeed:yourself:many",
                     new { ItemName = itemAsset.itemName, ItemID = itemAsset.id, ItemAmount = itemAmount }]));
                return;
            }
            if (await CheckPermissionAsync("other") != PermissionGrantResult.Grant)
                throw new NotEnoughPermissionException(Context, "other");
            if (!Context.Parameters.TryGet(2, out UnturnedUser? targetUser) || targetUser == null)
                throw new UserFriendlyException(string.Format("{0}{1}",
                    m_StringLocalizer["item_command:prefix"],
                    m_StringLocalizer["item_command:error_player"]));
            for (int i = 0; i < itemAmount; i++)
                targetUser.Player.Player.inventory.forceAddItem(item, true);
            SteamPlayer targetSPlayer = targetUser.Player.SteamPlayer;
            CSteamID targetSteamID = targetSPlayer.playerID.steamID;
            SteamPlayer sPlayer = user.Player.SteamPlayer;
            CSteamID steamID = sPlayer.playerID.steamID;
            await targetUser.PrintMessageAsync(string.Format("{0}{1}",
                m_StringLocalizer["item_command:prefix"],
                m_StringLocalizer["item_command:succeed:somebody:player", new
                {
                    PlayerName = sPlayer.playerID.playerName,
                    CharacterName = sPlayer.playerID.characterName,
                    NickName = sPlayer.playerID.nickName,
                    SteamID = steamID,
                    ItemName = itemAsset.itemName,
                    ItemID = itemAsset.id,
                    ItemAmount = itemAmount
                }]));
            await PrintAsync(string.Format("{0}{1}",
                m_StringLocalizer["item_command:prefix"],
                m_StringLocalizer["item_command:succeed:somebody:executor", new
                {
                    PlayerName = targetSPlayer.playerID.playerName,
                    CharacterName = targetSPlayer.playerID.characterName,
                    NickName = targetSPlayer.playerID.nickName,
                    SteamID = targetSteamID,
                    ItemName = itemAsset.itemName,
                    ItemID = itemAsset.id,
                    ItemAmount = itemAmount
                }]));
        }
    }

    [Command("item")]
    [CommandAlias("i")]
    [CommandAlias("give")]
    [CommandSyntax("<item> <amount> <player>")]
    [CommandDescription("Gives you or somebody items.")]
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
            if (!Context.Parameters.TryGet(0, out string? itemName) || itemName == null ||
                !UnturnedAssetHelper.GetItem(itemName, out ItemAsset itemAsset))
                throw new UserFriendlyException(m_StringLocalizer["item_command:error_null"]);
            Item item = new(itemAsset.id, EItemOrigin.ADMIN);
            if (!Context.Parameters.TryGet(1, out ushort itemAmount) || itemAmount == 0)
                throw new UserFriendlyException(m_StringLocalizer["item_command:error_null_number"]);
            if (itemAmount > 100)
                throw new UserFriendlyException(m_StringLocalizer["item_command:error_high_number"]);
            if (!Context.Parameters.TryGet(2, out UnturnedUser? targetUser) || targetUser == null)
                throw new UserFriendlyException(m_StringLocalizer["item_command:error_player"]);
            await UniTask.SwitchToMainThread();
            for (int i = 0; i < itemAmount; i++)
                targetUser.Player.Player.inventory.forceAddItem(item, true);
            SteamPlayer targetSPlayer = targetUser.Player.SteamPlayer;
            CSteamID targetSteamID = targetSPlayer.playerID.steamID;
            await targetUser.PrintMessageAsync(string.Format("{0}{1}",
                m_StringLocalizer["item_command:prefix"],
                m_StringLocalizer["item_command:succeed:somebody:console", new
                {
                    ItemName = itemAsset.itemName,
                    ItemID = itemAsset.id,
                    ItemAmount = itemAmount
                }]));
            await PrintAsync(m_StringLocalizer["item_command:succeed:somebody:executor", new
            {
                PlayerName = targetSPlayer.playerID.playerName,
                CharacterName = targetSPlayer.playerID.characterName,
                NickName = targetSPlayer.playerID.nickName,
                SteamID = targetSteamID,
                ItemName = itemAsset.itemName,
                ItemID = itemAsset.id,
                ItemAmount = itemAmount
            }]);
        }
    }
}

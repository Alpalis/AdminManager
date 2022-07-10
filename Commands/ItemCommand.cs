﻿using Alpalis.AdminManager.API;
using Alpalis.UtilityServices.API;
using Alpalis.UtilityServices.Helpers;
using Cysharp.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OpenMod.API.Commands;
using OpenMod.Core.Commands;
using OpenMod.Core.Console;
using OpenMod.Unturned.Commands;
using OpenMod.Unturned.Users;
using SDG.Unturned;
using System;
using System.Linq;
using Alpalis.AdminManager.Models;
using OpenMod.Core.Permissions;
using OpenMod.API.Plugins;
using OpenMod.API.Permissions;
using Steamworks;

namespace Alpalis.AdminManager.Commands
{
    public class ItemCommand
    {
        #region Command Parameters
        [Command("item")]
        [CommandAlias("i")]
        [CommandSyntax("<item> [amount] [player]")]
        [CommandDescription("Command to give items.")]
        [CommandActor(typeof(UnturnedUser))]
        [RegisterCommandPermission("other", Description = "Allows to give items other people.")]
        #endregion Command Parameters
        public class ItemUnturned : UnturnedCommand
        {
            #region Member Variables
            private readonly IAdminSystem m_AdminSystem;
            private readonly IStringLocalizer m_StringLocalizer;
            private readonly IIdentityManagerImplementation m_IdentityManagerImplementation;
            private readonly IConfigurationManager m_ConfigurationManager;
            private readonly Main m_Plugin;
            #endregion Member Variables

            #region Class Constructor
            public ItemUnturned(
                IAdminSystem adminSystem,
                IStringLocalizer stringLocalizer,
                IIdentityManagerImplementation identityManagerImplementation,
                IConfigurationManager configurationManager,
                IPluginAccessor<Main> plugin,
                IServiceProvider serviceProvider) : base(serviceProvider)
            {
                m_AdminSystem = adminSystem;
                m_StringLocalizer = stringLocalizer;
                m_IdentityManagerImplementation = identityManagerImplementation;
                m_ConfigurationManager = configurationManager;
                m_Plugin = plugin.Instance!;
            }
            #endregion Class Constructor

            protected override async UniTask OnExecuteAsync()
            {
                if (Context.Parameters.Count < 1 || Context.Parameters.Count > 3)
                    throw new CommandWrongUsageException(Context);
                Config config = m_ConfigurationManager.GetConfig<Config>(m_Plugin);
                UnturnedUser user = (UnturnedUser)Context.Actor;
                if (!m_AdminSystem.IsInAdminMode(user))
                    throw new UserFriendlyException(string.Format("{0}{1}",
                         config.MessagePrefix ? m_StringLocalizer["item_command:prefix"] : "",
                         m_StringLocalizer["item_command:error_adminmode"]));
                if (!Context.Parameters.TryGet(0, out string? itemName) || itemName == null ||
                    !UnturnedAssetHelper.GetItem(itemName, out ItemAsset itemAsset))
                    throw new UserFriendlyException(string.Format("{0}{1}",
                         config.MessagePrefix ? m_StringLocalizer["item_command:prefix"] : "",
                         m_StringLocalizer["item_command:error_null"]));
                Item item = new(itemAsset.id, EItemOrigin.ADMIN);
                await UniTask.SwitchToMainThread();
                if (Context.Parameters.Count == 1)
                {
                    user.Player.Player.inventory.forceAddItem(item, true);
                    PrintAsync(string.Format("{0}{1}",
                         config.MessagePrefix ? m_StringLocalizer["item_command:prefix"] : "",
                         m_StringLocalizer["item_command:succeed:yourself:one",
                         new { ItemName = itemAsset.itemName, ItemID = itemAsset.id }]));
                    return;
                }
                if (!Context.Parameters.TryGet(1, out ushort itemAmount) || itemAmount == 0)
                    throw new UserFriendlyException(string.Format("{0}{1}",
                    config.MessagePrefix ? m_StringLocalizer["item_command:prefix"] : "",
                    m_StringLocalizer["item_command:error_null_number"]));
                if (itemAmount > 100)
                    throw new UserFriendlyException(string.Format("{0}{1}",
                    config.MessagePrefix ? m_StringLocalizer["item_command:prefix"] : "",
                    m_StringLocalizer["item_command:error_high_number"]));
                if (Context.Parameters.Count == 2)
                {
                    for (int i = 0; i < itemAmount; i++)
                        user.Player.Player.inventory.forceAddItem(item, true);
                    PrintAsync(string.Format("{0}{1}",
                         config.MessagePrefix ? m_StringLocalizer["item_command:prefix"] : "",
                         m_StringLocalizer["item_command:succeed:yourself:many",
                         new { ItemName = itemAsset.itemName, ItemID = itemAsset.id, ItemAmount = itemAmount }]));
                    return;
                }
                if (await CheckPermissionAsync("other") != PermissionGrantResult.Grant)
                    throw new NotEnoughPermissionException(Context, "other");
                if (!Context.Parameters.TryGet(2, out UnturnedUser? targetUser) || targetUser == null)
                    throw new UserFriendlyException(string.Format("{0}{1}",
                        config.MessagePrefix ? m_StringLocalizer["item_command:prefix"] : "",
                        m_StringLocalizer["item_command:error_player"]));
                for (int i = 0; i < itemAmount; i++)
                    targetUser.Player.Player.inventory.forceAddItem(item, true);
                SteamPlayer targetSPlayer = targetUser.Player.SteamPlayer;
                CSteamID targetSteamID = targetSPlayer.playerID.steamID;
                ushort? targetIdentity = m_IdentityManagerImplementation.GetIdentity(targetSteamID);
                SteamPlayer sPlayer = user.Player.SteamPlayer;
                CSteamID steamID = sPlayer.playerID.steamID;
                ushort? identity = m_IdentityManagerImplementation.GetIdentity(steamID);
                targetUser.PrintMessageAsync(string.Format("{0}{1}",
                    config.MessagePrefix ? m_StringLocalizer["item_command:prefix"] : "",
                    m_StringLocalizer["item_command:succeed:somebody:player", new
                    {
                        PlayerName = sPlayer.playerID.playerName,
                        CharacterName = sPlayer.playerID.characterName,
                        NickName = sPlayer.playerID.nickName,
                        SteamID = steamID,
                        ID = identity,
                        ItemName = itemAsset.itemName,
                        ItemID = itemAsset.id,
                        ItemAmount = itemAmount
                    }]));
                PrintAsync(string.Format("{0}{1}",
                    config.MessagePrefix ? m_StringLocalizer["item_command:prefix"] : "",
                    m_StringLocalizer["item_command:succeed:somebody:executor", new
                    {
                        PlayerName = targetSPlayer.playerID.playerName,
                        CharacterName = targetSPlayer.playerID.characterName,
                        NickName = targetSPlayer.playerID.nickName,
                        SteamID = targetSteamID,
                        ID = targetIdentity,
                        ItemName = itemAsset.itemName,
                        ItemID = itemAsset.id,
                        ItemAmount = itemAmount
                    }]));
            }
        }

        #region Command Parameters
        [Command("item")]
        [CommandAlias("i")]
        [CommandSyntax("<item> <amount> <player>")]
        [CommandDescription("Command to give items.")]
        [CommandActor(typeof(ConsoleActor))]
        #endregion Command Parameters
        public class ItemConsole : UnturnedCommand
        {
            #region Member Variables
            private readonly IStringLocalizer m_StringLocalizer;
            private readonly IIdentityManagerImplementation m_IdentityManagerImplementation;
            private readonly IConfigurationManager m_ConfigurationManager;
            private readonly Main m_Plugin;
            #endregion Member Variables

            #region Class Constructor
            public ItemConsole(
                IStringLocalizer stringLocalizer,
                IIdentityManagerImplementation identityManagerImplementation,
                IConfigurationManager configurationManager,
                IPluginAccessor<Main> plugin,
                IServiceProvider serviceProvider) : base(serviceProvider)
            {
                m_StringLocalizer = stringLocalizer;
                m_IdentityManagerImplementation = identityManagerImplementation;
                m_ConfigurationManager = configurationManager;
                m_Plugin = plugin.Instance!;
            }
            #endregion Class Constructor

            protected override async UniTask OnExecuteAsync()
            {
                if (Context.Parameters.Count != 3)
                    throw new CommandWrongUsageException(Context);
                Config config = m_ConfigurationManager.GetConfig<Config>(m_Plugin);
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
                ushort? targetIdentity = m_IdentityManagerImplementation.GetIdentity(targetSteamID);
                targetUser.PrintMessageAsync(string.Format("{0}{1}",
                    config.MessagePrefix ? m_StringLocalizer["item_command:prefix"] : "",
                    m_StringLocalizer["item_command:succeed:somebody:console", new
                    {
                        ItemName = itemAsset.itemName,
                        ItemID = itemAsset.id,
                        ItemAmount = itemAmount
                    }]));
                PrintAsync(m_StringLocalizer["item_command:succeed:somebody:executor", new
                    {
                        PlayerName = targetSPlayer.playerID.playerName,
                        CharacterName = targetSPlayer.playerID.characterName,
                        NickName = targetSPlayer.playerID.nickName,
                        SteamID = targetSteamID,
                        ID = targetIdentity,
                        ItemName = itemAsset.itemName,
                        ItemID = itemAsset.id,
                        ItemAmount = itemAmount
                    }]);
            }
        }
    }
}

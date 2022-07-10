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
using System.Collections.Generic;
using UnityEngine;

namespace Alpalis.AdminManager.Commands
{
    public class ClearCommand
    {
        #region ClearRoot
        #region Command Parameters
        [Command("clear")]
        [CommandSyntax("<items/vehicles/inventory>")]
        [CommandDescription("Command to clear items, vehicles or inventories.")]
        [CommandActor(typeof(UnturnedUser))]
        #endregion Command Parameters
        public class ClearRootUnturned : UnturnedCommand
        {
            #region Class Constructor
            public ClearRootUnturned(
                IServiceProvider serviceProvider) : base(serviceProvider)
            {
            }
            #endregion Class Constructor

            protected override async UniTask OnExecuteAsync()
            {
                throw new CommandWrongUsageException(Context);
            }

        }

        #region Command Parameters
        [Command("clear")]
        [CommandSyntax("<inventory>")]
        [CommandDescription("Command to clear inventories.")]
        [CommandActor(typeof(ConsoleActor))]
        #endregion Command Parameters
        public class ClearRootConsole : UnturnedCommand
        {
            #region Class Constructor
            public ClearRootConsole(
                IServiceProvider serviceProvider) : base(serviceProvider)
            {
            }
            #endregion Class Constructor

            protected override async UniTask OnExecuteAsync()
            {
                throw new CommandWrongUsageException(Context);
            }

        }
        #endregion ClearRoot

        #region Inventory
        #region Command Parameters
        [Command("inventory")]
        [CommandSyntax("[player]")]
        [CommandDescription("Command to clear inventories.")]
        [CommandActor(typeof(UnturnedUser))]
        [RegisterCommandPermission("other", Description = "Allows to clear inventory of other player.")]
        [CommandParent(typeof(ClearRootUnturned))]
        #endregion Command Parameters
        public class InventoryUnturned : UnturnedCommand
        {
            #region Member Variables
            private readonly IAdminSystem m_AdminSystem;
            private readonly IStringLocalizer m_StringLocalizer;
            private readonly IConfigurationManager m_ConfigurationManager;
            private readonly IIdentityManagerImplementation m_IdentityManagerImplementation;
            private readonly Main m_Plugin;
            #endregion Member Variables

            #region Class Constructor
            public InventoryUnturned(
                IAdminSystem adminSystem,
                IStringLocalizer stringLocalizer,
                IConfigurationManager configurationManager,
                IIdentityManagerImplementation identityManagerImplementation,
                IPluginAccessor<Main> plugin,
                IServiceProvider serviceProvider) : base(serviceProvider)
            {
                m_AdminSystem = adminSystem;
                m_StringLocalizer = stringLocalizer;
                m_ConfigurationManager = configurationManager;
                m_IdentityManagerImplementation = identityManagerImplementation;
                m_Plugin = plugin.Instance!;
            }
            #endregion Class Constructor

            protected override async UniTask OnExecuteAsync()
            {
                if (Context.Parameters.Count != 0 && Context.Parameters.Count != 1)
                    throw new CommandWrongUsageException(Context);
                Config config = m_ConfigurationManager.GetConfig<Config>(m_Plugin);
                UnturnedUser user = (UnturnedUser)Context.Actor;
                if (!m_AdminSystem.IsInAdminMode(user))
                    throw new UserFriendlyException(string.Format("{0}{1}",
                         config.MessagePrefix ? m_StringLocalizer["clear_command:prefix"] : "",
                         m_StringLocalizer["clear_command:error_adminmode"]));
                if (Context.Parameters.Count == 0)
                {
                    await UniTask.SwitchToMainThread();
                    user.Player.Player.ClearInventory();
                    PrintAsync(string.Format("{0}{1}",
                        config.MessagePrefix ? m_StringLocalizer["clear_command:prefix"] : "",
                        m_StringLocalizer["clear_command:inventory:succeed:yourself"]));
                    return;
                }
                if (await CheckPermissionAsync("other") != PermissionGrantResult.Grant)
                    throw new NotEnoughPermissionException(Context, "other");
                if (!Context.Parameters.TryGet(0, out UnturnedUser? targetUser) || targetUser == null)
                    throw new UserFriendlyException(string.Format("{0}{1}",
                        config.MessagePrefix ? m_StringLocalizer["clear_command:prefix"] : "",
                        m_StringLocalizer["clear_command:inventory:error_player"]));
                SteamPlayer targetSPlayer = targetUser.Player.SteamPlayer;
                CSteamID targetSteamID = targetSPlayer.playerID.steamID;
                ushort? targetIdentity = m_IdentityManagerImplementation.GetIdentity(targetSteamID);
                SteamPlayer sPlayer = user.Player.SteamPlayer;
                CSteamID steamID = sPlayer.playerID.steamID;
                ushort? identity = m_IdentityManagerImplementation.GetIdentity(steamID);
                await UniTask.SwitchToMainThread();
                targetUser.Player.Player.ClearInventory();
                targetUser.PrintMessageAsync(string.Format("{0}{1}",
                    config.MessagePrefix ? m_StringLocalizer["clear_command:prefix"] : "",
                    m_StringLocalizer["clear_command:inventory:succeed:somebody:player", new
                    {
                        PlayerName = sPlayer.playerID.playerName,
                        CharacterName = sPlayer.playerID.characterName,
                        NickName = sPlayer.playerID.nickName,
                        SteamID = steamID,
                        ID = identity
                    }]));
                PrintAsync(string.Format("{0}{1}",
                    config.MessagePrefix ? m_StringLocalizer["clear_command:prefix"] : "",
                    m_StringLocalizer["clear_command:inventory:succeed:somebody:executor", new
                    {
                        PlayerName = targetSPlayer.playerID.playerName,
                        CharacterName = targetSPlayer.playerID.characterName,
                        NickName = targetSPlayer.playerID.nickName,
                        SteamID = targetSteamID,
                        ID = targetIdentity
                    }]));
            }
        }

        #region Command Parameters
        [Command("inventory")]
        [CommandSyntax("<player>")]
        [CommandDescription("Command to clear inventories.")]
        [CommandActor(typeof(ConsoleActor))]
        [CommandParent(typeof(ClearRootConsole))]
        #endregion Command Parameters
        public class InventoryConsole : UnturnedCommand
        {
            #region Member Variables
            private readonly IAdminSystem m_AdminSystem;
            private readonly IStringLocalizer m_StringLocalizer;
            private readonly IConfigurationManager m_ConfigurationManager;
            private readonly IIdentityManagerImplementation m_IdentityManagerImplementation;
            private readonly Main m_Plugin;
            #endregion Member Variables

            #region Class Constructor
            public InventoryConsole(
                IAdminSystem adminSystem,
                IStringLocalizer stringLocalizer,
                IConfigurationManager configurationManager,
                IIdentityManagerImplementation identityManagerImplementation,
                IPluginAccessor<Main> plugin,
                IServiceProvider serviceProvider) : base(serviceProvider)
            {
                m_AdminSystem = adminSystem;
                m_StringLocalizer = stringLocalizer;
                m_ConfigurationManager = configurationManager;
                m_IdentityManagerImplementation = identityManagerImplementation;
                m_Plugin = plugin.Instance!;
            }
            #endregion Class Constructor

            protected override async UniTask OnExecuteAsync()
            {
                if (Context.Parameters.Count != 1)
                    throw new CommandWrongUsageException(Context);
                Config config = m_ConfigurationManager.GetConfig<Config>(m_Plugin);
                if (!Context.Parameters.TryGet(0, out UnturnedUser? targetUser) || targetUser == null)
                    throw new UserFriendlyException(m_StringLocalizer["clear_command:inventory:error_player"]);
                SteamPlayer targetSPlayer = targetUser.Player.SteamPlayer;
                CSteamID targetSteamID = targetSPlayer.playerID.steamID;
                ushort? targetIdentity = m_IdentityManagerImplementation.GetIdentity(targetSteamID);
                await UniTask.SwitchToMainThread();
                targetUser.Player.Player.ClearInventory();
                targetUser.PrintMessageAsync(string.Format("{0}{1}",
                    config.MessagePrefix ? m_StringLocalizer["clear_command:prefix"] : "",
                    m_StringLocalizer["clear_command:inventory:succeed:somebody:console"]));
                PrintAsync(m_StringLocalizer["clear_command:inventory:succeed:somebody:executor", new
                    {
                        PlayerName = targetSPlayer.playerID.playerName,
                        CharacterName = targetSPlayer.playerID.characterName,
                        NickName = targetSPlayer.playerID.nickName,
                        SteamID = targetSteamID,
                        ID = targetIdentity
                    }]);
            }
        }
        #endregion Inventory

        #region Command Parameters
        [Command("items")]
        [CommandSyntax("<distance>")]
        [CommandDescription("Command to clear items laying on the ground.")]
        [CommandActor(typeof(UnturnedUser))]
        [CommandParent(typeof(ClearRootUnturned))]
        #endregion Command Parameters
        public class ItemsUnturned : UnturnedCommand
        {
            #region Member Variables
            private readonly IAdminSystem m_AdminSystem;
            private readonly IStringLocalizer m_StringLocalizer;
            private readonly IConfigurationManager m_ConfigurationManager;
            private readonly Main m_Plugin;
            #endregion Member Variables

            #region Class Constructor
            public ItemsUnturned(
                IAdminSystem adminSystem,
                IStringLocalizer stringLocalizer,
                IConfigurationManager configurationManager,
                IPluginAccessor<Main> plugin,
                IServiceProvider serviceProvider) : base(serviceProvider)
            {
                m_AdminSystem = adminSystem;
                m_StringLocalizer = stringLocalizer;
                m_ConfigurationManager = configurationManager;
                m_Plugin = plugin.Instance!;
            }
            #endregion Class Constructor

            protected override async UniTask OnExecuteAsync()
            {
                if (Context.Parameters.Count != 1)
                    throw new CommandWrongUsageException(Context);
                Config config = m_ConfigurationManager.GetConfig<Config>(m_Plugin);
                UnturnedUser user = (UnturnedUser)Context.Actor;
                if (!m_AdminSystem.IsInAdminMode(user))
                    throw new UserFriendlyException(string.Format("{0}{1}",
                         config.MessagePrefix ? m_StringLocalizer["clear_command:prefix"] : "",
                         m_StringLocalizer["clear_command:error_adminmode"]));
                if (!Context.Parameters.TryGet(0, out float distance) || distance <= 0)
                    throw new UserFriendlyException(string.Format("{0}{1}",
                        config.MessagePrefix ? m_StringLocalizer["clear_command:prefix"] : "",
                        m_StringLocalizer["clear_command:error_distance"]));
                await UniTask.SwitchToMainThread();
                List<RegionCoordinate> regions = new();
                Regions.getRegionsInRadius(user.Player.Player.transform.position, distance * distance, regions);
                int amount = 0;
                foreach (RegionCoordinate region in regions)
                {
                    ItemRegion itemRegion = ItemManager.regions[region.x, region.y];
                    itemRegion.lastRespawn = Time.realtimeSinceStartup;
                    amount += itemRegion.items.Count;
                    ItemManager.askClearRegionItems(region.x, region.y);
                }
                if (amount == 0)
                    throw new UserFriendlyException(string.Format("{0}{1}",
                        config.MessagePrefix ? m_StringLocalizer["clear_command:prefix"] : "",
                        m_StringLocalizer["clear_command:items:error_none", new
                        {
                            Amount = amount
                        }]));
                PrintAsync(string.Format("{0}{1}", config.MessagePrefix ? m_StringLocalizer["clear_command:prefix"] : "",
                    m_StringLocalizer["clear_command:items:succeed", new
                    {
                        Amount = amount
                    }]));
            }
        }

        #region Command Parameters
        [Command("vehicle")]
        [CommandSyntax("<distance> [clearLocked]")]
        [CommandDescription("Command to clear vehicles.")]
        [CommandActor(typeof(UnturnedUser))]
        [CommandParent(typeof(ClearRootUnturned))]
        #endregion Command Parameters
        public class VehiclesUnturned : UnturnedCommand
        {
            #region Member Variables
            private readonly IAdminSystem m_AdminSystem;
            private readonly IStringLocalizer m_StringLocalizer;
            private readonly IConfigurationManager m_ConfigurationManager;
            private readonly Main m_Plugin;
            #endregion Member Variables

            #region Class Constructor
            public VehiclesUnturned(
                IAdminSystem adminSystem,
                IStringLocalizer stringLocalizer,
                IConfigurationManager configurationManager,
                IPluginAccessor<Main> plugin,
                IServiceProvider serviceProvider) : base(serviceProvider)
            {
                m_AdminSystem = adminSystem;
                m_StringLocalizer = stringLocalizer;
                m_ConfigurationManager = configurationManager;
                m_Plugin = plugin.Instance!;
            }
            #endregion Class Constructor

            protected override async UniTask OnExecuteAsync()
            {
                if (Context.Parameters.Count != 1)
                    throw new CommandWrongUsageException(Context);
                Config config = m_ConfigurationManager.GetConfig<Config>(m_Plugin);
                UnturnedUser user = (UnturnedUser)Context.Actor;
                if (!m_AdminSystem.IsInAdminMode(user))
                    throw new UserFriendlyException(string.Format("{0}{1}",
                         config.MessagePrefix ? m_StringLocalizer["clear_command:prefix"] : "",
                         m_StringLocalizer["clear_command:error_adminmode"]));
                if (!Context.Parameters.TryGet(0, out float distance) || distance <= 0)
                    throw new UserFriendlyException(string.Format("{0}{1}",
                        config.MessagePrefix ? m_StringLocalizer["clear_command:prefix"] : "",
                        m_StringLocalizer["clear_command:error_distance"]));
                await UniTask.SwitchToMainThread();
                List<InteractableVehicle> vehicles = new();
                VehicleManager.getVehiclesInRadius(user.Player.Player.transform.position, distance * distance, vehicles);
                int amount = 0;
                foreach (InteractableVehicle vehicle in vehicles)
                {
                    if (vehicle.asset.engine == EEngine.TRAIN) continue;
                    amount++;
                    VehicleManager.askVehicleDestroy(vehicle);
                }
                if (amount == 0)
                    throw new UserFriendlyException(string.Format("{0}{1}",
                        config.MessagePrefix ? m_StringLocalizer["clear_command:prefix"] : "",
                        m_StringLocalizer["clear_command:vehicles:error_none", new
                        {
                            Amount = amount
                        }]));
                PrintAsync(string.Format("{0}{1}", config.MessagePrefix ? m_StringLocalizer["clear_command:prefix"] : "",
                    m_StringLocalizer["clear_command:vehicles:succeed", new
                    {
                        Amount = amount
                    }]));
            }
        }
    }
}

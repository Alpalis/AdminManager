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
using System.Collections.Generic;
using UnityEngine;

namespace Alpalis.AdminManager.Commands
{
    public class ClearCommand
    {
        [Command("clear")]
        [CommandSyntax("<items/vehicles/inventory>")]
        [CommandDescription("Clears items, vehicles or inventories.")]
        [CommandActor(typeof(UnturnedUser))]
        public class RootUnturned : UnturnedCommand
        {
            public RootUnturned(
                IServiceProvider serviceProvider) : base(serviceProvider)
            {
            }

            protected override async UniTask OnExecuteAsync()
            {
                throw new CommandWrongUsageException(Context);
            }

        }

        [Command("clear")]
        [CommandSyntax("<inventory>")]
        [CommandDescription("Clears inventories.")]
        [CommandActor(typeof(ConsoleActor))]
        public class RootConsole : UnturnedCommand
        {
            public RootConsole(
                IServiceProvider serviceProvider) : base(serviceProvider)
            {
            }

            protected override async UniTask OnExecuteAsync()
            {
                throw new CommandWrongUsageException(Context);
            }

        }

        [Command("inventory")]
        [CommandSyntax("[player]")]
        [CommandDescription("Clears inventories.")]
        [CommandActor(typeof(UnturnedUser))]
        [RegisterCommandPermission("other", Description = "Allows to clear inventory of other player.")]
        [CommandParent(typeof(RootUnturned))]
        public class InventoryUnturned : UnturnedCommand
        {
            private readonly IAdminSystem m_AdminSystem;
            private readonly IStringLocalizer m_StringLocalizer;

            public InventoryUnturned(
                IAdminSystem adminSystem,
                IStringLocalizer stringLocalizer,
                IServiceProvider serviceProvider) : base(serviceProvider)
            {
                m_AdminSystem = adminSystem;
                m_StringLocalizer = stringLocalizer;
            }

            protected override async UniTask OnExecuteAsync()
            {
                if (Context.Parameters.Count != 0 && Context.Parameters.Count != 1)
                    throw new CommandWrongUsageException(Context);
                UnturnedUser user = (UnturnedUser)Context.Actor;
                if (!m_AdminSystem.IsInAdminMode(user))
                    throw new UserFriendlyException(string.Format("{0}{1}",
                         m_StringLocalizer["clear_command:prefix"],
                         m_StringLocalizer["clear_command:error_adminmode"]));
                if (Context.Parameters.Count == 0)
                {
                    await UniTask.SwitchToMainThread();
                    user.Player.Player.ClearInventory();
                    PrintAsync(string.Format("{0}{1}",
                        m_StringLocalizer["clear_command:prefix"],
                        m_StringLocalizer["clear_command:inventory:succeed:yourself"]));
                    return;
                }
                if (await CheckPermissionAsync("other") != PermissionGrantResult.Grant)
                    throw new NotEnoughPermissionException(Context, "other");
                if (!Context.Parameters.TryGet(0, out UnturnedUser? targetUser) || targetUser == null)
                    throw new UserFriendlyException(string.Format("{0}{1}",
                        m_StringLocalizer["clear_command:prefix"],
                        m_StringLocalizer["clear_command:inventory:error_player"]));
                SteamPlayer targetSPlayer = targetUser.Player.SteamPlayer;
                CSteamID targetSteamID = targetSPlayer.playerID.steamID;
                SteamPlayer sPlayer = user.Player.SteamPlayer;
                CSteamID steamID = sPlayer.playerID.steamID;
                await UniTask.SwitchToMainThread();
                targetUser.Player.Player.ClearInventory();
                targetUser.PrintMessageAsync(string.Format("{0}{1}",
                    m_StringLocalizer["clear_command:prefix"],
                    m_StringLocalizer["clear_command:inventory:succeed:somebody:player", new
                    {
                        PlayerName = sPlayer.playerID.playerName,
                        CharacterName = sPlayer.playerID.characterName,
                        NickName = sPlayer.playerID.nickName,
                        SteamID = steamID
                    }]));
                PrintAsync(string.Format("{0}{1}",
                    m_StringLocalizer["clear_command:prefix"],
                    m_StringLocalizer["clear_command:inventory:succeed:somebody:executor", new
                    {
                        PlayerName = targetSPlayer.playerID.playerName,
                        CharacterName = targetSPlayer.playerID.characterName,
                        NickName = targetSPlayer.playerID.nickName,
                        SteamID = targetSteamID
                    }]));
            }
        }

        [Command("inventory")]
        [CommandSyntax("<player>")]
        [CommandDescription("Clears inventories.")]
        [CommandActor(typeof(ConsoleActor))]
        [CommandParent(typeof(RootConsole))]
        public class InventoryConsole : UnturnedCommand
        {
            private readonly IStringLocalizer m_StringLocalizer;

            public InventoryConsole(
                IStringLocalizer stringLocalizer,
                IServiceProvider serviceProvider) : base(serviceProvider)
            {
                m_StringLocalizer = stringLocalizer;
            }

            protected override async UniTask OnExecuteAsync()
            {
                if (Context.Parameters.Count != 1)
                    throw new CommandWrongUsageException(Context);
                if (!Context.Parameters.TryGet(0, out UnturnedUser? targetUser) || targetUser == null)
                    throw new UserFriendlyException(m_StringLocalizer["clear_command:inventory:error_player"]);
                SteamPlayer targetSPlayer = targetUser.Player.SteamPlayer;
                CSteamID targetSteamID = targetSPlayer.playerID.steamID;
                await UniTask.SwitchToMainThread();
                targetUser.Player.Player.ClearInventory();
                targetUser.PrintMessageAsync(string.Format("{0}{1}",
                    m_StringLocalizer["clear_command:prefix"],
                    m_StringLocalizer["clear_command:inventory:succeed:somebody:console"]));
                PrintAsync(m_StringLocalizer["clear_command:inventory:succeed:somebody:executor", new
                {
                    PlayerName = targetSPlayer.playerID.playerName,
                    CharacterName = targetSPlayer.playerID.characterName,
                    NickName = targetSPlayer.playerID.nickName,
                    SteamID = targetSteamID
                }]);
            }
        }

        [Command("items")]
        [CommandSyntax("<distance>")]
        [CommandDescription("Clears items laying on the ground.")]
        [CommandActor(typeof(UnturnedUser))]
        [CommandParent(typeof(RootUnturned))]
        public class ItemsUnturned : UnturnedCommand
        {
            private readonly IAdminSystem m_AdminSystem;
            private readonly IStringLocalizer m_StringLocalizer;

            public ItemsUnturned(
                IAdminSystem adminSystem,
                IStringLocalizer stringLocalizer,
                IServiceProvider serviceProvider) : base(serviceProvider)
            {
                m_AdminSystem = adminSystem;
                m_StringLocalizer = stringLocalizer;
            }

            protected override async UniTask OnExecuteAsync()
            {
                if (Context.Parameters.Count != 1)
                    throw new CommandWrongUsageException(Context);
                UnturnedUser user = (UnturnedUser)Context.Actor;
                if (!m_AdminSystem.IsInAdminMode(user))
                    throw new UserFriendlyException(string.Format("{0}{1}",
                         m_StringLocalizer["clear_command:prefix"],
                         m_StringLocalizer["clear_command:error_adminmode"]));
                if (!Context.Parameters.TryGet(0, out float distance) || distance <= 0)
                    throw new UserFriendlyException(string.Format("{0}{1}",
                        m_StringLocalizer["clear_command:prefix"],
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
                        m_StringLocalizer["clear_command:prefix"],
                        m_StringLocalizer["clear_command:items:error_none", new
                        {
                            Amount = amount
                        }]));
                PrintAsync(string.Format("{0}{1}", m_StringLocalizer["clear_command:prefix"],
                    m_StringLocalizer["clear_command:items:succeed", new
                    {
                        Amount = amount
                    }]));
            }
        }

        [Command("vehicles")]
        [CommandSyntax("<distance> [clearLocked]")]
        [CommandDescription("Clears vehicles.")]
        [CommandActor(typeof(UnturnedUser))]
        [CommandParent(typeof(RootUnturned))]
        public class VehiclesUnturned : UnturnedCommand
        {
            private readonly IAdminSystem m_AdminSystem;
            private readonly IStringLocalizer m_StringLocalizer;

            public VehiclesUnturned(
                IAdminSystem adminSystem,
                IStringLocalizer stringLocalizer,
                IServiceProvider serviceProvider) : base(serviceProvider)
            {
                m_AdminSystem = adminSystem;
                m_StringLocalizer = stringLocalizer;
            }

            protected override async UniTask OnExecuteAsync()
            {
                if (Context.Parameters.Count != 1)
                    throw new CommandWrongUsageException(Context);
                UnturnedUser user = (UnturnedUser)Context.Actor;
                if (!m_AdminSystem.IsInAdminMode(user))
                    throw new UserFriendlyException(string.Format("{0}{1}",
                         m_StringLocalizer["clear_command:prefix"],
                         m_StringLocalizer["clear_command:error_adminmode"]));
                if (!Context.Parameters.TryGet(0, out float distance) || distance <= 0)
                    throw new UserFriendlyException(string.Format("{0}{1}",
                        m_StringLocalizer["clear_command:prefix"],
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
                        m_StringLocalizer["clear_command:prefix"],
                        m_StringLocalizer["clear_command:vehicles:error_none", new
                        {
                            Amount = amount
                        }]));
                PrintAsync(string.Format("{0}{1}", m_StringLocalizer["clear_command:prefix"],
                    m_StringLocalizer["clear_command:vehicles:succeed", new
                    {
                        Amount = amount
                    }]));
            }
        }
    }
}

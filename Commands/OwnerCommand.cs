using Alpalis.AdminManager.API;
using Cysharp.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OpenMod.API.Commands;
using OpenMod.Core.Commands;
using OpenMod.Unturned.Commands;
using OpenMod.Unturned.Users;
using SDG.Unturned;
using System;
using UnityEngine;

namespace Alpalis.AdminManager.Commands
{
    [Command("owner")]
    [CommandDescription("Allows to check owner of object you're looking at.")]
    [CommandActor(typeof(UnturnedUser))]
    public class OwnerCommand : UnturnedCommand
    {
        private readonly IAdminSystem m_AdminSystem;
        private readonly IStringLocalizer m_StringLocalizer;

        public OwnerCommand(
            IAdminSystem adminSystem,
            IStringLocalizer StringLocalizer,
            IServiceProvider serviceProvider) : base(serviceProvider)
        {
            m_AdminSystem = adminSystem;
            m_StringLocalizer = StringLocalizer;
        }

        protected override async UniTask OnExecuteAsync()
        {
            if (Context.Parameters.Count != 0)
                throw new CommandWrongUsageException(Context);
            UnturnedUser user = (UnturnedUser)Context.Actor;
            if (!m_AdminSystem.IsInAdminMode(user))
                throw new UserFriendlyException(string.Format("{0}{1}",
                     m_StringLocalizer["owner_command:prefix"],
                     m_StringLocalizer["owner_command:error_adminmode"]));
            await UniTask.SwitchToMainThread();
            Transform aim = user.Player.Player.look.aim;
            RaycastInfo raycast = DamageTool.raycast(new(aim.position, aim.forward), 8f, RayMasks.BARRICADE | RayMasks.STRUCTURE | RayMasks.VEHICLE);
            if (raycast == null)
                throw new UserFriendlyException(m_StringLocalizer["owner_command:prefix"]);
            InteractableVehicle vehicle = raycast.collider.GetComponent<InteractableVehicle>();
            if (vehicle != null)
            {
                PrintAsync(string.Format("{0}{1}", m_StringLocalizer["owner_command:prefix"],
                    m_StringLocalizer["owner_command:succeed:vehicle", new
                    {
                        SteamID = vehicle.lockedOwner
                    }]));
                return;
            }
            BarricadeDrop bDrop = BarricadeManager.FindBarricadeByRootTransform(raycast.transform);
            if (bDrop != null)
            {
                PrintAsync(string.Format("{0}{1}", m_StringLocalizer["owner_command:prefix"],
                    m_StringLocalizer["owner_command:succeed:barricade", new
                    {
                        SteamID = bDrop.GetServersideData().owner
                    }]));
                return;
            }
            StructureDrop sDrop = StructureManager.FindStructureByRootTransform(raycast.transform);
            if (sDrop != null)
            {
                PrintAsync(string.Format("{0}{1}", m_StringLocalizer["owner_command:prefix"],
                    m_StringLocalizer["owner_command:succeed:structure", new
                    {
                        SteamID = sDrop.GetServersideData().owner
                    }]));
                return;
            }
            throw new UserFriendlyException(string.Format("{0}{1}",
                     m_StringLocalizer["owner_command:prefix"],
                     m_StringLocalizer["owner_command:error_null"]));
        }

    }
}

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

namespace Alpalis.AdminManager.Commands;

[Command("destroy")]
[CommandDescription("Destroys object you're looking at.")]
[CommandActor(typeof(UnturnedUser))]
public sealed class DestroyCommand(
    IAdminSystem adminSystem,
    IStringLocalizer StringLocalizer,
    IServiceProvider serviceProvider) : UnturnedCommand(serviceProvider)
{
    private readonly IAdminSystem m_AdminSystem = adminSystem;
    private readonly IStringLocalizer m_StringLocalizer = StringLocalizer;

    protected override async UniTask OnExecuteAsync()
    {
        if (Context.Parameters.Count != 0)
            throw new CommandWrongUsageException(Context);
        UnturnedUser user = (UnturnedUser)Context.Actor;
        if (!m_AdminSystem.IsInAdminMode(user))
            throw new UserFriendlyException(string.Format("{0}{1}",
                 m_StringLocalizer["destroy_command:prefix"],
                 m_StringLocalizer["destroy_command:error_adminmode"]));
        await UniTask.SwitchToMainThread();
        PlayerLook look = user.Player.Player.look;
        RaycastInfo raycast = DamageTool.raycast(new(look.aim.position, look.aim.forward), 8f,
            RayMasks.BARRICADE | RayMasks.STRUCTURE | RayMasks.VEHICLE)
            ?? throw new UserFriendlyException(string.Format("{0}{1}",
                     m_StringLocalizer["destroy_command:prefix"],
                     m_StringLocalizer["destroy_command:error_null"]));
        InteractableVehicle vehicle = raycast.vehicle;
        if (vehicle != null)
        {
            VehicleManager.askVehicleDestroy(vehicle);
            await PrintAsync(string.Format("{0}{1}", m_StringLocalizer["destroy_command:prefix"],
                m_StringLocalizer["destroy_command:succeed:vehicle"]));
            return;
        }
        BarricadeDrop bDrop = BarricadeManager.FindBarricadeByRootTransform(raycast.transform);
        if (bDrop != null && BarricadeManager.tryGetRegion(raycast.transform, out byte bx, out byte by, out ushort plant, out _))
        {
            BarricadeManager.destroyBarricade(bDrop, bx, by, plant);
            await PrintAsync(string.Format("{0}{1}", m_StringLocalizer["destroy_command:prefix"],
                m_StringLocalizer["destroy_command:succeed:barricade"]));
            return;
        }
        StructureDrop sDrop = StructureManager.FindStructureByRootTransform(raycast.transform);
        if (sDrop != null && StructureManager.tryGetRegion(raycast.transform, out byte sx, out byte sy, out _))
        {
            StructureManager.destroyStructure(sDrop, sx, sy, Vector3.zero);
            await PrintAsync(string.Format("{0}{1}", m_StringLocalizer["destroy_command:prefix"],
                m_StringLocalizer["destroy_command:succeed:structure"]));
            return;
        }
        throw new UserFriendlyException(string.Format("{0}{1}",
                 m_StringLocalizer["destroy_command:prefix"],
                 m_StringLocalizer["destroy_command:error_null"]));
    }

}

using Alpalis.AdminManager.API;
using Cysharp.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OpenMod.API.Commands;
using OpenMod.Core.Commands;
using OpenMod.Unturned.Commands;
using OpenMod.Unturned.Users;
using SDG.Unturned;
using System;

namespace Alpalis.AdminManager.Commands;

[Command("repair")]
[CommandDescription("Allows to repair the object you're looking at or current vehicle.")]
[CommandActor(typeof(UnturnedUser))]
public sealed class RepairCommand(
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
                 m_StringLocalizer["repair_command:prefix"],
                 m_StringLocalizer["repair_command:error_adminmode"]));
        await UniTask.SwitchToMainThread();
        InteractableVehicle currentVehicle = user.Player.Player.movement.getVehicle();
        if (currentVehicle != null)
        {
            RepairVehicle(currentVehicle);
            await PrintAsync(string.Format("{0}{1}", m_StringLocalizer["refuel_command:prefix"],
                m_StringLocalizer["refuel_command:succeed:vehicle:current", new { Vehicle = currentVehicle.name }]));
            return;
        }
        PlayerLook look = user.Player.Player.look;
        RaycastInfo raycast = DamageTool.raycast(new(look.aim.position, look.aim.forward), 8f, RayMasks.DAMAGE_SERVER | RayMasks.VEHICLE);
        if (raycast.vehicle != null)
        {
            RepairVehicle(raycast.vehicle);
            await PrintAsync(string.Format("{0}{1}", m_StringLocalizer["refuel_command:prefix"],
                m_StringLocalizer["refuel_command:succeed:vehicle:looking_at", new { Vehicle = raycast.vehicle.name }]));
            return;
        }
        if (raycast.transform == null)
            throw new UserFriendlyException(string.Format("{0}{1}",
                     m_StringLocalizer["refuel_command:prefix"],
                     m_StringLocalizer["refuel_command:error_null"]));
        BarricadeDrop bDrop = BarricadeManager.FindBarricadeByRootTransform(raycast.transform);
        if (bDrop != null)
        {
            BarricadeData bData = bDrop.GetServersideData();
            bData.barricade.health = bData.barricade.asset.health;
            bData.barricade.askRepair(bData.barricade.asset.health);
            await PrintAsync(string.Format("{0}{1}", m_StringLocalizer["refuel_command:prefix"],
                m_StringLocalizer["refuel_command:succeed:vehicle:looking_at"]));
            return;
        }
        StructureDrop sDrop = StructureManager.FindStructureByRootTransform(raycast.transform);
        if (sDrop != null)
        {
            StructureData sData = sDrop.GetServersideData();
            sData.structure.askRepair(sData.structure.asset.health);
            await PrintAsync(string.Format("{0}{1}", m_StringLocalizer["refuel_command:prefix"],
                m_StringLocalizer["refuel_command:succeed:vehicle:looking_at"]));
            return;
        }
        throw new UserFriendlyException(string.Format("{0}{1}",
             m_StringLocalizer["refuel_command:prefix"],
             m_StringLocalizer["refuel_command:error_null"]));
    }

    private void RepairVehicle(InteractableVehicle vehicle)
    {
        if (!vehicle.usesHealth)
            return;
        if (vehicle.isRepaired)
            return;
        ushort maxHealth = vehicle.asset.health;
        vehicle.health = maxHealth;
        VehicleManager.sendVehicleHealth(vehicle, maxHealth);
    }
}

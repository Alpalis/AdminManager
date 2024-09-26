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

[Command("more")]
[CommandSyntax("[amount]")]
[CommandDescription("Allows to duplicate the item you are holding.")]
[CommandActor(typeof(UnturnedUser))]
public sealed class MoreCommand(
    IAdminSystem adminSystem,
    IStringLocalizer stringLocalizer,
    IServiceProvider serviceProvider) : UnturnedCommand(serviceProvider)
{
    private readonly IAdminSystem m_AdminSystem = adminSystem;
    private readonly IStringLocalizer m_StringLocalizer = stringLocalizer;

    protected override async UniTask OnExecuteAsync()
    {
        if (Context.Parameters.Count != 0 && Context.Parameters.Count != 1)
            throw new CommandWrongUsageException(Context);
        UnturnedUser user = (UnturnedUser)Context.Actor;
        if (!m_AdminSystem.IsInAdminMode(user))
            throw new UserFriendlyException(string.Format("{0}{1}",
                 m_StringLocalizer["more_command:prefix"],
                 m_StringLocalizer["more_command:error_adminmode"]));
        PlayerEquipment equipment = user.Player.Player.equipment;
        if (equipment.itemID == 0)
            throw new UserFriendlyException(m_StringLocalizer["more_command:error_null"]);
        Item item = new(equipment.itemID, EItemOrigin.ADMIN);
        await UniTask.SwitchToMainThread();
        if (Context.Parameters.Count == 0)
        {
            user.Player.Player.inventory.forceAddItem(item, true);
            await PrintAsync(string.Format("{0}{1}",
                 m_StringLocalizer["more_command:prefix"],
                 m_StringLocalizer["more_command:succeed:one",
                 new { ItemName = equipment.asset.itemName, ItemID = equipment.asset.id }]));
            return;
        }
        if (!Context.Parameters.TryGet(0, out ushort itemAmount) || itemAmount == 0)
            throw new UserFriendlyException(string.Format("{0}{1}",
            m_StringLocalizer["more_command:prefix"],
            m_StringLocalizer["more_command:error_null_number"]));
        if (itemAmount > 100)
            throw new UserFriendlyException(string.Format("{0}{1}",
            m_StringLocalizer["more_command:prefix"],
            m_StringLocalizer["more_command:error_high_number"]));
        for (int i = 0; i < itemAmount; i++)
            user.Player.Player.inventory.forceAddItem(item, true);
        await PrintAsync(string.Format("{0}{1}",
             m_StringLocalizer["more_command:prefix"],
             m_StringLocalizer["more_command:succeed:many",
             new { ItemName = equipment.asset.itemName, ItemID = equipment.asset.id, ItemAmount = itemAmount }]));
    }
}

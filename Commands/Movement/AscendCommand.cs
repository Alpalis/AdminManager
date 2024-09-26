using Alpalis.AdminManager.API;
using Cysharp.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OpenMod.API.Commands;
using OpenMod.Core.Commands;
using OpenMod.Unturned.Commands;
using OpenMod.Unturned.Users;
using System;
using UnityEngine;

namespace Alpalis.AdminManager.Commands.Movement;

[Command("ascend")]
[CommandAlias("up")]
[CommandSyntax("[distance]")]
[CommandDescription("Teleports you up.")]
[CommandActor(typeof(UnturnedUser))]
public sealed class AscendCommand(
    IStringLocalizer stringLocalizer,
    IAdminSystem adminSystem,
    IServiceProvider serviceProvider) : UnturnedCommand(serviceProvider)
{
    private readonly IStringLocalizer m_StringLocalizer = stringLocalizer;
    private readonly IAdminSystem m_AdminSystem = adminSystem;

    protected override async UniTask OnExecuteAsync()
    {
        UnturnedUser user = (UnturnedUser)Context.Actor;
        if (!m_AdminSystem.IsInAdminMode(user))
            throw new UserFriendlyException(string.Format("{0}{1}",
                 m_StringLocalizer["ascend_command:prefix"],
                 m_StringLocalizer["ascend_command:error_adminmode"]));
        if (Context.Parameters.Count > 1)
            throw new CommandWrongUsageException(Context);
        float distance = 10f;
        if (Context.Parameters.Count == 1 && !Context.Parameters.TryGet(0, out distance))
            throw new UserFriendlyException(string.Format("{0}{1}",
                m_StringLocalizer["ascend_command:prefix"],
                m_StringLocalizer["ascend_command:error_distance"]));
        Vector3 position = user.Player.Player.transform.position;
        position.y += distance;
        await user.Player.Player.TeleportToLocationUnsafeAsync(position);
        await PrintAsync(string.Format("{0}{1}",
            m_StringLocalizer["ascend_command:prefix"],
            m_StringLocalizer["ascend_command:succeed", new { Distance = distance }]));
    }
}

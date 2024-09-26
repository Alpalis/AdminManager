using Alpalis.AdminManager.API;
using Cysharp.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OpenMod.API.Commands;
using OpenMod.Core.Commands;
using OpenMod.Unturned.Commands;
using OpenMod.Unturned.Users;
using SDG.Unturned;
using System;

namespace Alpalis.AdminManager.Commands.Time;

[Command("cycle")]
[CommandSyntax("<lenght>")]
[CommandDescription("Sets length of day/night cycle.")]
public sealed class CycleCommand(
    IStringLocalizer StringLocalizer,
    IAdminSystem adminSystem,
    IServiceProvider serviceProvider) : UnturnedCommand(serviceProvider)
{
    private readonly IStringLocalizer m_StringLocalizer = StringLocalizer;
    private readonly IAdminSystem m_AdminSystem = adminSystem;

    protected override async UniTask OnExecuteAsync()
    {
        if (Context.Parameters.Count != 1)
            throw new CommandWrongUsageException(Context);
        if (!m_AdminSystem.IsInAdminMode(Context.Actor))
            throw new UserFriendlyException(string.Format("{0}{1}",
                 m_StringLocalizer["cycle_command:prefix"],
                 m_StringLocalizer["cycle_command:error_adminmode"]));
        if (!Context.Parameters.TryGet(0, out uint lenght))
            throw new UserFriendlyException(string.Format("{0}{1}",
                Context.Actor.GetType() == typeof(UnturnedUser) ? m_StringLocalizer["cycle_command:prefix"] : "",
                m_StringLocalizer["cycle_command:error_lenght"]));
        await UniTask.SwitchToMainThread();
        LightingManager.cycle = lenght;
        await PrintAsync(string.Format("{0}{1}", Context.Actor.GetType() == typeof(UnturnedUser) ? m_StringLocalizer["cycle_command:prefix"] : "",
            m_StringLocalizer["cycle_command:succeed"]));
    }
}

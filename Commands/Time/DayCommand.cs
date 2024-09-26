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

[Command("day")]
[CommandDescription("Sets day time.")]
public sealed class DayCommand(
    IStringLocalizer StringLocalizer,
    IAdminSystem adminSystem,
    IServiceProvider serviceProvider) : UnturnedCommand(serviceProvider)
{
    private readonly IStringLocalizer m_StringLocalizer = StringLocalizer;
    private readonly IAdminSystem m_AdminSystem = adminSystem;

    protected override async UniTask OnExecuteAsync()
    {
        if (Context.Parameters.Count != 0)
            throw new CommandWrongUsageException(Context);
        if (!m_AdminSystem.IsInAdminMode(Context.Actor))
            throw new UserFriendlyException(string.Format("{0}{1}",
                 m_StringLocalizer["day_command:prefix"],
                 m_StringLocalizer["day_command:error_adminmode"]));
        await UniTask.SwitchToMainThread();
        LightingManager.time = (uint)(LightingManager.cycle * LevelLighting.transition);
        await PrintAsync(string.Format("{0}{1}", Context.Actor.GetType() == typeof(UnturnedUser) ? m_StringLocalizer["day_command:prefix"] : "",
            m_StringLocalizer["day_command:succeed"]));
    }
}

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

[Command("exit")]
[CommandDescription("Allows you fast exit the server.")]
[CommandActor(typeof(UnturnedUser))]
public sealed class ExitCommand(
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
        UnturnedUser user = (UnturnedUser)Context.Actor;
        if (!m_AdminSystem.IsInAdminMode(user))
            throw new UserFriendlyException(string.Format("{0}{1}",
                 m_StringLocalizer["exit_command:prefix"],
                 m_StringLocalizer["exit_command:error_adminmode"]));
        await UniTask.SwitchToMainThread();
        Provider.kick(user.SteamId, m_StringLocalizer["exit_command:message"]);
    }
}

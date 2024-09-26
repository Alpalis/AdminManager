using Alpalis.AdminManager.API;
using Alpalis.UtilityServices.Helpers;
using Cysharp.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OpenMod.API.Commands;
using OpenMod.Core.Commands;
using OpenMod.Unturned.Commands;
using OpenMod.Unturned.Users;
using SDG.Unturned;
using System;
using System.Net;

namespace Alpalis.AdminManager.Commands;

[Command("join")]
[CommandDescription("Allows you to join directly to another server.")]
[CommandSyntax("<ip/domain> <port> [password]")]
[CommandActor(typeof(UnturnedUser))]
public sealed class JoinCommand(
    IStringLocalizer StringLocalizer,
    IAdminSystem adminSystem,
    IServiceProvider serviceProvider) : UnturnedCommand(serviceProvider)
{
    private readonly IStringLocalizer m_StringLocalizer = StringLocalizer;
    private readonly IAdminSystem m_AdminSystem = adminSystem;

    protected override async UniTask OnExecuteAsync()
    {
        if (Context.Parameters.Count != 2 && Context.Parameters.Count != 3)
            throw new CommandWrongUsageException(Context);
        UnturnedUser user = (UnturnedUser)Context.Actor;
        if (!m_AdminSystem.IsInAdminMode(user))
            throw new UserFriendlyException(string.Format("{0}{1}",
                 m_StringLocalizer["join_command:prefix"],
                 m_StringLocalizer["join_command:error_adminmode"]));
        if (!Context.Parameters.TryGet(0, out string? ipString) || ipString == null)
            throw new UserFriendlyException(string.Format("{0}{1}",
                 m_StringLocalizer["join_command:prefix"],
                 m_StringLocalizer["join_command:error_ip"]));
        IPAddress[]? addresses = null;
        try
        {
            addresses = Dns.GetHostAddresses(ipString);
        }
        catch (Exception) { }
        if (addresses == null || addresses.Length == 0)
            throw new UserFriendlyException(string.Format("{0}{1}",
                 m_StringLocalizer["join_command:prefix"],
                 m_StringLocalizer["join_command:error_ip"]));
        if (!Context.Parameters.TryGet(1, out ushort port))
            throw new UserFriendlyException(string.Format("{0}{1}",
                 m_StringLocalizer["join_command:prefix"],
                 m_StringLocalizer["join_command:error_port"]));
        string? password = Context.Parameters.Count == 3 ? Context.Parameters[2] : default;
        await UniTask.SwitchToMainThread();
        uint ipUInt = IPAddressHelper.GetUIntFromIPAddress(addresses[0]);
        user.Player.Player.sendRelayToServer(ipUInt, port, password, false);
    }
}

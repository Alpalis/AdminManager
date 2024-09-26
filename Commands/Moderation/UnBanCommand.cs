using Alpalis.AdminManager.API;
using Cysharp.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OpenMod.API.Commands;
using OpenMod.Core.Commands;
using OpenMod.Unturned.Commands;
using OpenMod.Unturned.Users;
using SDG.Unturned;
using Steamworks;
using System;

namespace Alpalis.AdminManager.Commands.Moderation;

[Command("unban")]
[CommandSyntax("<player's steamID>")]
[CommandDescription("Allows to unban players.")]
public sealed class UnBanCommand(
    IStringLocalizer stringLocalizer,
    IAdminSystem adminSystem,
    IServiceProvider serviceProvider) : UnturnedCommand(serviceProvider)
{
    private readonly IStringLocalizer m_StringLocalizer = stringLocalizer;
    private readonly IAdminSystem m_AdminSystem = adminSystem;

    protected override async UniTask OnExecuteAsync()
    {
        if (Context.Parameters.Length != 1)
            throw new CommandWrongUsageException(Context);
        if (!m_AdminSystem.IsInAdminMode(Context.Actor))
            throw new UserFriendlyException(string.Format("{0}{1}",
                m_StringLocalizer["unban_command:prefix"],
                m_StringLocalizer["unban_command:error_adminmode"]));
        if (!Context.Parameters.TryGet(0, out ulong steamID))
            throw new UserFriendlyException(string.Format("{0}{1}",
                Context.Actor.GetType() == typeof(UnturnedUser) ? m_StringLocalizer["unban_command:prefix"] : "",
                m_StringLocalizer["unban_command:error_steamid"]));
        CSteamID formatedSteamID = new(steamID);
        if (!formatedSteamID.IsValid())
            throw new UserFriendlyException(string.Format("{0}{1}",
                Context.Actor.GetType() == typeof(UnturnedUser) ? m_StringLocalizer["unban_command:prefix"] : "",
                m_StringLocalizer["unban_command:error_steamid"]));
        if (!Provider.requestUnbanPlayer(Context.Actor is UnturnedUser user ? user.SteamId : CSteamID.Nil, formatedSteamID))
            throw new UserFriendlyException(string.Format("{0}{1}",
                Context.Actor.GetType() == typeof(UnturnedUser) ? m_StringLocalizer["unban_command:prefix"] : "",
                m_StringLocalizer["unban_command:error_null"]));
        await PrintAsync(string.Format("{0}{1}",
            Context.Actor.GetType() == typeof(UnturnedUser) ? m_StringLocalizer["unban_command:prefix"] : "",
            m_StringLocalizer["unban_command:succeed", new
            {
                SteamID = formatedSteamID
            }]));
    }
}

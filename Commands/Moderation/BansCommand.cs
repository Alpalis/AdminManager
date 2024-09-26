using Alpalis.AdminManager.API;
using Alpalis.UtilityServices.Helpers;
using Cysharp.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OpenMod.API.Commands;
using OpenMod.Core.Commands;
using OpenMod.Unturned.Commands;
using OpenMod.Unturned.Users;
using SDG.Unturned;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Alpalis.AdminManager.Commands.Moderation;

[Command("bans")]
[CommandDescription("Displays list of bans.")]
public sealed class BansCommand(
    IAdminSystem adminSystem,
    IStringLocalizer stringLocalizer,
    IServiceProvider serviceProvider) : UnturnedCommand(serviceProvider)
{
    private readonly IAdminSystem m_AdminSystem = adminSystem;
    private readonly IStringLocalizer m_StringLocalizer = stringLocalizer;

    protected override async UniTask OnExecuteAsync()
    {
        if (Context.Parameters.Length != 0)
            throw new CommandWrongUsageException(Context);
        if (!m_AdminSystem.IsInAdminMode(Context.Actor))
            throw new UserFriendlyException(string.Format("{0}{1}",
                m_StringLocalizer["bans_command:prefix"],
                m_StringLocalizer["bans_command:error_adminmode"]));
        await PrintAsync(string.Format("{0}{1}",
            Context.Actor.GetType() == typeof(UnturnedUser) ? m_StringLocalizer["bans_command:prefix"] : "", 
            m_StringLocalizer["bans_command:succeed:title"]));
        if (SteamBlacklist.list.Count == 0)
        {
            await PrintAsync(m_StringLocalizer["bans_command:succeed:empty"]);
            return;
        }
        await UniTask.SwitchToMainThread();
        foreach (SteamBlacklistID ban in SteamBlacklist.list)
        {
            FieldInfo fieldInfo = typeof(SteamBlacklistID).GetField("hwids", BindingFlags.NonPublic | BindingFlags.Instance);
            object? value = fieldInfo.GetValue(ban);
            List<byte[]> hwids = value == null ? [] : [.. ((byte[][])value)];
            if (ban.judgeID == CSteamID.Nil)
            {
                await PrintAsync(m_StringLocalizer["bans_command:succeed:list:console", new
                {
                    SteamID = ban.playerID,
                    IP = IPAddressHelper.GetIPAddressFromUInt(ban.ip),
                    Time = DateTimeEx.FromUtcUnixTimeSeconds(ban.banned),
                    Duration = ban.duration,
                    Reason = ban.reason,
                    HWIDs = string.Join(", ", hwids.Select(x => Hash.toString(x)))
                }]);
                continue;
            }
            await PrintAsync(m_StringLocalizer["bans_command:succeed:list:player", new
            {
                CallerID = ban.judgeID,
                SteamID = ban.playerID,
                IP = IPAddressHelper.GetIPAddressFromUInt(ban.ip),
                Time = DateTimeEx.FromUtcUnixTimeSeconds(ban.banned),
                Duration = ban.duration,
                Reason = ban.reason,
                HWIDs = string.Join(", ", hwids.Select(x => Hash.toString(x)))
            }]);
        }
    }
}

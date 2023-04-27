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
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Alpalis.AdminManager.Commands.Moderation
{
    [Command("bans")]
    [CommandDescription("Displays list of bans.")]
    public class BansCommand : UnturnedCommand
    {
        private readonly IAdminSystem m_AdminSystem;
        private readonly IStringLocalizer m_StringLocalizer;

        public BansCommand(
            IAdminSystem adminSystem,
            IStringLocalizer stringLocalizer,
            IServiceProvider serviceProvider) : base(serviceProvider)
        {
            m_AdminSystem = adminSystem;
            m_StringLocalizer = stringLocalizer;
        }

        protected override async UniTask OnExecuteAsync()
        {
            if (Context.Parameters.Length != 0)
                throw new CommandWrongUsageException(Context);
            if (!m_AdminSystem.IsInAdminMode(Context.Actor))
                throw new UserFriendlyException(string.Format("{0}{1}",
                    m_StringLocalizer["bans_command:prefix"],
                    m_StringLocalizer["bans_command:error_adminmode"]));
            PrintAsync(string.Format("{0}{1}",
                Context.Actor.GetType() == typeof(UnturnedUser) ? m_StringLocalizer["bans_command:prefix"] : "", 
                m_StringLocalizer["bans_command:succeed:title"]));
            if (SteamBlacklist.list.Count == 0)
            {
                PrintAsync(m_StringLocalizer["bans_command:succeed:empty"]);
                return;
            }
            await UniTask.SwitchToMainThread();
            foreach (SteamBlacklistID ban in SteamBlacklist.list)
            {
                FieldInfo fieldInfo = typeof(SteamBlacklistID).GetField("hwids", BindingFlags.NonPublic | BindingFlags.Instance);
                object? value = fieldInfo.GetValue(ban);
                List<byte[]> hwids = value == null? new List<byte[]>() : ((byte[][])value).ToList();
                if (ban.judgeID == CSteamID.Nil)
                {
                    PrintAsync(m_StringLocalizer["bans_command:succeed:list:console", new
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
                PrintAsync(m_StringLocalizer["bans_command:succeed:list:player", new
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
}

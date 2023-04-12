﻿using Alpalis.AdminManager.API;
using Cysharp.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OpenMod.API.Commands;
using OpenMod.Core.Commands;
using OpenMod.Unturned.Commands;
using OpenMod.Unturned.Users;
using SDG.Unturned;
using System;

namespace Alpalis.AdminManager.Commands
{
    public class TimeCommand
    {
        [Command("time")]
        [CommandSyntax("<get/set>")]
        [CommandDescription("Manages overworld time.")]
        public class Root : UnturnedCommand
        {
            public Root(
                IServiceProvider serviceProvider) : base(serviceProvider)
            {
            }

            protected override async UniTask OnExecuteAsync()
            {
                throw new CommandWrongUsageException(Context);
            }
        }

        [Command("get")]
        [CommandDescription("Gets overworld time.")]
        [CommandParent(typeof(Root))]
        public class Get : UnturnedCommand
        {
            private readonly IStringLocalizer m_StringLocalizer;
            private readonly IAdminSystem m_AdminSystem;

            public Get(
                IStringLocalizer stringLocalizer,
                IAdminSystem adminSystem,
                IServiceProvider serviceProvider) : base(serviceProvider)
            {
                m_StringLocalizer = stringLocalizer;
                m_AdminSystem = adminSystem;
            }

            protected override async UniTask OnExecuteAsync()
            {
                if (Context.Parameters.Count != 0)
                    throw new CommandWrongUsageException(Context);
                if (!m_AdminSystem.IsInAdminMode(Context.Actor))
                    throw new UserFriendlyException(string.Format("{0}{1}",
                         m_StringLocalizer["time_command:prefix"],
                         m_StringLocalizer["time_command:error_adminmode"]));
                PrintAsync(string.Format("{0}{1}", Context.Actor.GetType() == typeof(UnturnedUser) ? m_StringLocalizer["time_command:prefix"] : "",
                    m_StringLocalizer["time_command:get", new
                    {
                        Time = LightingManager.time
                    }]));
            }
        }

        [Command("set")]
        [CommandSyntax("<time>")]
        [CommandDescription("Sets overworld time.")]
        [CommandParent(typeof(Root))]
        public class Set : UnturnedCommand
        {
            private readonly IStringLocalizer m_StringLocalizer;
            private readonly IAdminSystem m_AdminSystem;

            public Set(
                IStringLocalizer StringLocalizer,
                IAdminSystem adminSystem,
                IServiceProvider serviceProvider) : base(serviceProvider)
            {
                m_StringLocalizer = StringLocalizer;
                m_AdminSystem = adminSystem;
            }

            protected override async UniTask OnExecuteAsync()
            {
                if (Context.Parameters.Count != 1)
                    throw new CommandWrongUsageException(Context);
                if (!m_AdminSystem.IsInAdminMode(Context.Actor))
                    throw new UserFriendlyException(string.Format("{0}{1}",
                         m_StringLocalizer["time_command:prefix"],
                         m_StringLocalizer["time_command:error_adminmode"]));
                await UniTask.SwitchToMainThread();
                if (!Context.Parameters.TryGet(0, out uint time))
                    throw new UserFriendlyException(string.Format("{0}{1}",
                        Context.Actor.GetType() == typeof(UnturnedUser) ? m_StringLocalizer["time_command:prefix"] : "",
                        m_StringLocalizer["time_command:set:error_time"]));
                if (time >= 3600)
                    throw new UserFriendlyException(string.Format("{0}{1}",
                        Context.Actor.GetType() == typeof(UnturnedUser) ? m_StringLocalizer["time_command:prefix"] : "",
                        m_StringLocalizer["time_command:set:error_maxtime"]));
                LightingManager.time = time;
                PrintAsync(string.Format("{0}{1}", Context.Actor.GetType() == typeof(UnturnedUser) ? m_StringLocalizer["time_command:prefix"] : "",
                    m_StringLocalizer["time_command:set:succeed", new
                    {
                        Time = time
                    }]));
            }
        }
    }
}

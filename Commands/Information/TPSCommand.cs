using Cysharp.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OpenMod.Core.Commands;
using OpenMod.Unturned.Commands;
using OpenMod.Unturned.Users;
using SDG.Unturned;
using System;

namespace Alpalis.AdminManager.Commands.Information
{
    [Command("tps")]
    [CommandDescription("Shows server TPS.")]
    public class TPSCommand : UnturnedCommand
    {
        private readonly IStringLocalizer m_StringLocalizer;

        public TPSCommand(
            IStringLocalizer stringLocalizer,
            IServiceProvider serviceProvider) : base(serviceProvider)
        {
            m_StringLocalizer = stringLocalizer;
        }

        protected override async UniTask OnExecuteAsync()
        {
            int tps = Provider.debugTPS;
            PrintAsync(string.Format("{0}{1}",
                Context.Actor.GetType() == typeof(UnturnedUser) ? m_StringLocalizer["tps_command:prefix"] : "",
                m_StringLocalizer["tps_command:succeed", new { TPS = tps }]));
        }
    }
}

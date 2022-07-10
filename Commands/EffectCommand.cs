using Cysharp.Threading.Tasks;
using OpenMod.Core.Commands;
using OpenMod.Core.Permissions;
using OpenMod.Unturned.Commands;
using OpenMod.Unturned.Users;
using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alpalis.AdminManager.Commands
{
    public class EffectCommand
    {
        [Command("effect")]
        [CommandAlias("eff")]
        [CommandDescription("Spawn an effect on your position")]
        [CommandSyntax("<id> [player/x] [y] [z]")]
        [CommandActor(typeof(UnturnedUser))]
        [RegisterCommandPermission("other", Description = "Allows to spawn offect on somebodys position.")]
        public class EffectUnturned :UnturnedCommand
        {
            #region Class Constructor
            public EffectUnturned(
                IServiceProvider serviceProvider) : base(serviceProvider)
            {
            }
            #endregion Class Constructor

            protected override async UniTask OnExecuteAsync()
            {
                await UniTask.SwitchToMainThread();
                EffectManager.sendUIEffect(29001, 1, ((UnturnedUser)Context.Actor).Player.Player.channel.GetOwnerTransportConnection(), true);
            }
        }

        public class EffectConsole
        {
        }
    }
}

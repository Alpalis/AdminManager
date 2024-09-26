using Cysharp.Threading.Tasks;
using OpenMod.Core.Commands;
using OpenMod.Core.Permissions;
using OpenMod.Unturned.Commands;
using OpenMod.Unturned.Users;
using SDG.Unturned;
using System;

namespace Alpalis.AdminManager.Commands;

public sealed class EffectCommand
{
    [Command("effect")]
    [CommandAlias("eff")]
    [CommandDescription("Spawn an effect on your position")]
    [CommandSyntax("<id> [player/x] [y] [z]")]
    [CommandActor(typeof(UnturnedUser))]
    [RegisterCommandPermission("other", Description = "Allows to spawn offect on somebodys position.")]
    public sealed class EffectUnturned(
        IServiceProvider serviceProvider) : UnturnedCommand(serviceProvider)
    {
        protected override UniTask OnExecuteAsync()
        {
            // to rework from another repo
            throw new NotImplementedException();
            //await UniTask.SwitchToMainThread();
            //EffectManager.sendUIEffect(29001, 1, ((UnturnedUser)Context.Actor).Player.Player.channel.GetOwnerTransportConnection(), true);
        }
    }

    public class EffectConsole
    {
    }
}

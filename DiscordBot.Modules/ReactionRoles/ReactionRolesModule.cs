using Autofac;
using DiscordBot.Framework.Contract.Boot;
using DiscordBot.Framework.Contract.Modularity;
using DiscordBot.Framework.Contract.TimedAction;

namespace DiscordBot.Modules.ReactionRoles;

public class ReactionRolesModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterType<ReactionRoleBootStep>().As<IBootStep>();
        builder.RegisterType<SetupReactionRoles>().As<ITimedAction>();
        builder.RegisterType<ReactionRoleCommands>().As<IGuildModule>();
        builder.RegisterType<ReactionRoleManager>().SingleInstance();
    }
}
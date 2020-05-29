using System;
using Akka.Actor;
using Akka.DI.Core;
using Akka.Pattern;

namespace gamemaster
{
    public static class ActorExtensions
    {
        public static IActorRef ChildWithBackoffSupervision<T>(this IUntypedActorContext context, string name = nameof(T), int minBackoff = 1, int maxBackoff = 3)
            where T : ActorBase
        {
            var childProps = context.System.DI().Props<T>();
            return context.ActorOf(
                Props.Create(() =>
                        new BackoffSupervisor(childProps,
                            name,
                            TimeSpan.FromSeconds(minBackoff),
                            TimeSpan.FromSeconds(maxBackoff),
                            0.1),
                    SupervisorStrategy.StoppingStrategy));
        }

        public static IActorRef ChildWithBackoffSupervision<T>(this IUntypedActorContext context, Props childProps, string name = nameof(T),
            int minBackoff = 1,
            int maxBackoff = 3)
            where T : ActorBase
        {
            return context.ActorOf(
                Props.Create(() =>
                        new BackoffSupervisor(childProps,
                            name,
                            TimeSpan.FromSeconds(minBackoff),
                            TimeSpan.FromSeconds(maxBackoff),
                            0.1),
                    SupervisorStrategy.StoppingStrategy));
        }

        public static IActorRef ChildWithBackoffSupervision(this IUntypedActorContext context, Props childProps, string name, int minBackoff = 1,
            int maxBackoff = 3)
        {
            return context.ActorOf(
                Props.Create(() =>
                        new BackoffSupervisor(childProps,
                            name,
                            TimeSpan.FromSeconds(minBackoff),
                            TimeSpan.FromSeconds(maxBackoff),
                            0.1),
                    SupervisorStrategy.StoppingStrategy));
        }
    }
}
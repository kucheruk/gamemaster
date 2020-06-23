using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using Akka.Actor;
using Akka.DI.Core;
using Microsoft.Extensions.DependencyInjection;

namespace gamemaster
{
    /// <summary>
    ///     Defines services used by the Akka.Actor.ActorSystem extension system to create actors using
    ///     Microsoft.Extensions.DependencyInjection
    /// </summary>
    internal class MicrosoftDependencyResolver : IDependencyResolver
    {
        private readonly ConditionalWeakTable<ActorBase, IServiceScope> _references;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ActorSystem _system;
        private readonly ConcurrentDictionary<string, Type> _typeCache;


        public MicrosoftDependencyResolver(IServiceScopeFactory scopeFactory, ActorSystem system)
        {
            _system = system ?? throw new ArgumentNullException(nameof(system));
            _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));

            _system.AddDependencyResolver(this);

            _typeCache = new ConcurrentDictionary<string, Type>(StringComparer.OrdinalIgnoreCase);
            _references = new ConditionalWeakTable<ActorBase, IServiceScope>();
        }

        public Type GetType(string actorName)
        {
            if (string.IsNullOrWhiteSpace(actorName))
            {
                throw new ArgumentException(
                    "Argument cannot be null, empty, or entirely composed of whitespace: 'actorName'.",
                    nameof(actorName));
            }

            var typeValue = actorName.GetTypeValue();
            _typeCache.TryAdd(actorName, typeValue);
            return _typeCache[actorName];
        }

        public Props Create<TActor>()
            where TActor : ActorBase
        {
            return Create(typeof(TActor));
        }

        public Props Create(Type actorType)
        {
            var props = _system.GetExtension<DIExt>().Props(actorType);
            return props;
        }


        public Func<ActorBase> CreateActorFactory(Type actorType)
        {
            if (actorType == null)
            {
                throw new ArgumentNullException(nameof(actorType));
            }

            return () =>
            {
                var scope = _scopeFactory.CreateScope();
                var actorRef = scope.ServiceProvider.GetService(actorType);
                var actor = actorRef as ActorBase;

                _references.Add(actor ?? throw new InvalidOperationException(), scope);

                return actor;
            };
        }


        public void Release(ActorBase actor)
        {
            if (actor == null)
            {
                throw new ArgumentNullException(nameof(actor));
            }

            if (_references.TryGetValue(actor, out var scope))
            {
                scope.Dispose();
                _references.Remove(actor);
            }
        }
    }
}
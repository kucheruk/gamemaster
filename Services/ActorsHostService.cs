using System.Threading;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.DI.Core;
using gamemaster.Actors;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace gamemaster.Services
{
    public class ActorsHostService : IHostedService
    {
        private readonly ILogger<ActorsHostService> _logger;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly MessageRouter _router;
        private ActorSystem _as;
        private MicrosoftDependencyResolver _resolver;
        private IActorRef _supervisor;

        public ActorsHostService(IServiceScopeFactory scopeFactory, MessageRouter router,
            ILogger<ActorsHostService> logger)
        {
            _scopeFactory = scopeFactory;
            _router = router;
            _logger = logger;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _as = ActorSystem.Create("bright", @"akka { stdout-loglevel = INFO
loglevel = DEBUG
log-config-on-start = on
loggers = [""Akka.Logger.Serilog.SerilogLogger, Akka.Logger.Serilog""]
actor { debug { lifecycle = on
unhandled = on } } }");
            _router.RegisterSystem(_as);
            _resolver = new MicrosoftDependencyResolver(_scopeFactory, _as);
            _logger.LogInformation("Starting up with actor dependency resolver of {Type}", _resolver.GetType());
            _supervisor = _as.ActorOf(_as.DI().Props<GamemasterSupervisor>()
                .WithSupervisorStrategy(SupervisorStrategy.DefaultStrategy));

            return Task.CompletedTask;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await _as.Terminate();
        }
    }
}
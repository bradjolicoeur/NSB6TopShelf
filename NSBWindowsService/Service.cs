using Common.Logging;
using NServiceBus;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSBWindowsService
{
    internal class Service
    {
        IEndpointInstance endpoint;

        #region "Logging..."

        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #endregion

        private IDependency _dependency { get; }
        public Service(IDependency dependency)
        {
            _dependency = dependency;
        }

        public bool Start()
        {
            log.Debug("--------------------------------");
            log.Info("Sample Service Started...");
            AsyncOnStart().GetAwaiter().GetResult();
            log.Debug("--------------------------------");
            return true;
        }
        async Task AsyncOnStart()
        {
            try
            {
                var endpointConfiguration = new EndpointConfiguration("MyWindowsService");

                //TODO: optionally choose a different serializer
                // https://docs.particular.net/nservicebus/serialization/
                endpointConfiguration.UseSerialization<NewtonsoftSerializer>();

                endpointConfiguration.DefineCriticalErrorAction(OnCriticalError);

                //TODO: this is to prevent accidentally deploying to production without considering important actions
                if (Environment.UserInteractive && Debugger.IsAttached)
                {
                    //TODO: For production use select a durable transport.
                    // https://docs.particular.net/transports/
                    endpointConfiguration.UseTransport<LearningTransport>();

                    //TODO: For production use select a durable persistence.
                    // https://docs.particular.net/persistence/
                    endpointConfiguration.UsePersistence<LearningPersistence>();

                    //TODO: For production use script the installation.
                    endpointConfiguration.EnableInstallers();
                }
                endpoint = await NServiceBus.Endpoint.Start(endpointConfiguration)
                    .ConfigureAwait(false);

            }
            catch (Exception exception)
            {
                Exit("Failed to start", exception);
            }
        }

        Task OnCriticalError(ICriticalErrorContext context)
        {
            //TODO: Decide if shutting down the process is the best response to a critical error
            // https://docs.particular.net/nservicebus/hosting/critical-errors
            var fatalMessage = $"The following critical error was encountered:\n{context.Error}\nProcess is shutting down.";
            Exit(fatalMessage, context.Exception);
            return Task.FromResult(0);
        }

        void Exit(string failedToStart, Exception exception)
        {
            log.Fatal(failedToStart, exception);
            //TODO: When using an external logging framework it is important to flush any pending entries prior to calling FailFast
            // https://docs.particular.net/nservicebus/hosting/critical-errors#when-to-override-the-default-critical-error-action

            //TODO: https://docs.particular.net/nservicebus/hosting/windows-service#installation-restart-recovery
            Environment.FailFast(failedToStart, exception);
        }


        public bool Stop()
        {
            log.Info("Service shutting down");
            endpoint?.Stop().GetAwaiter().GetResult();
            //TODO: perform any shutdown operations
            return true;
        }

    }
}

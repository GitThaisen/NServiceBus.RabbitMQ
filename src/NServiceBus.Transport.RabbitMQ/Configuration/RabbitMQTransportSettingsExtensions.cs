﻿namespace NServiceBus
{
    using System;
    using System.Security.Cryptography.X509Certificates;
    using Configuration.AdvancedExtensibility;
    using RabbitMQ.Client.Events;
    using Transport.RabbitMQ;

    /// <summary>
    /// Adds access to the RabbitMQ transport config to the global Transports object.
    /// </summary>
    public static class RabbitMQTransportSettingsExtensions
    {
        /// <summary>
        /// Registers a custom routing topology.
        /// </summary>
        /// <param name="transportExtensions"></param>
        /// <param name="topologyFactory">The function used to create the routing topology instance. The parameter of the function indicates whether exchanges and queues declared by the routing topology should be durable.</param>
        public static TransportExtensions<RabbitMQTransport> UseRoutingTopology(this TransportExtensions<RabbitMQTransport> transportExtensions, Func<bool, IRoutingTopology> topologyFactory)
        {
            transportExtensions.GetSettings().Set(topologyFactory);
            return transportExtensions;
        }

        /// <summary>
        /// Uses the conventional routing topology.
        /// </summary>
        /// <param name="transportExtensions"></param>
        public static TransportExtensions<RabbitMQTransport> UseConventionalRoutingTopology(this TransportExtensions<RabbitMQTransport> transportExtensions) =>
            transportExtensions.UseRoutingTopology(durable => new ConventionalRoutingTopology(durable));

        /// <summary>
        /// Uses the conventional routing topology, where receiving queues support priority.
        /// </summary>
        /// <param name="transportExtensions"></param>
        /// <param name="maxPriority">Max priority on queue. 0 - 9</param>
        public static TransportExtensions<RabbitMQTransport> UseConventionalRoutingTopologyWithPriority(this TransportExtensions<RabbitMQTransport> transportExtensions, int maxPriority) =>
            transportExtensions.UseRoutingTopology(durable => new ConventionalRoutingTopology(durable, maxPriority));

        
        /// <summary>
        /// Uses the direct routing topology with the specified conventions.
        /// </summary>
        /// <param name="transportExtensions"></param>
        /// <param name="routingKeyConvention">The routing key convention.</param>
        /// <param name="exchangeNameConvention">The exchange name convention.</param>
        public static TransportExtensions<RabbitMQTransport> UseDirectRoutingTopology(this TransportExtensions<RabbitMQTransport> transportExtensions, Func<Type, string> routingKeyConvention = null, Func<string> exchangeNameConvention = null)
        {
            if (routingKeyConvention == null)
            {
                routingKeyConvention = DefaultRoutingKeyConvention.GenerateRoutingKey;
            }

            if (exchangeNameConvention == null)
            {
                exchangeNameConvention = () => "amq.topic";
            }

            transportExtensions.UseRoutingTopology(durable => new DirectRoutingTopology(new DirectRoutingTopology.Conventions(exchangeNameConvention, routingKeyConvention), durable));

            return transportExtensions;
        }

        /// <summary>
        /// Allows the user to control how the message ID is determined. Mostly useful when doing native integration with non-NSB endpoints.
        /// </summary>
        /// <param name="transportExtensions"></param>
        /// <param name="customIdStrategy">The user-defined strategy for giving the message a unique ID.</param>
        /// <returns></returns>
        public static TransportExtensions<RabbitMQTransport> CustomMessageIdStrategy(this TransportExtensions<RabbitMQTransport> transportExtensions, Func<BasicDeliverEventArgs, string> customIdStrategy)
        {
            transportExtensions.GetSettings().Set(SettingsKeys.CustomMessageIdStrategy, customIdStrategy);
            return transportExtensions;
        }

        /// <summary>
        /// Overrides the default time to wait before triggering a circuit breaker that initiates the endpoint shutdown procedure when the message pump's connection to the broker is lost and cannot be recovered.
        /// </summary>
        /// <param name="transportExtensions"></param>
        /// <param name="waitTime">The time to wait before triggering the circuit breaker.</param>
        public static TransportExtensions<RabbitMQTransport> TimeToWaitBeforeTriggeringCircuitBreaker(this TransportExtensions<RabbitMQTransport> transportExtensions, TimeSpan waitTime)
        {
            transportExtensions.GetSettings().Set(SettingsKeys.TimeToWaitBeforeTriggeringCircuitBreaker, waitTime);
            return transportExtensions;
        }

        /// <summary>
        /// Specifies whether publisher confirms should be used when sending messages.
        /// </summary>
        /// <param name="transportExtensions"></param>
        /// <param name="usePublisherConfirms">Specifies whether publisher confirms should be used when sending messages.</param>
        /// <returns></returns>
        public static TransportExtensions<RabbitMQTransport> UsePublisherConfirms(this TransportExtensions<RabbitMQTransport> transportExtensions, bool usePublisherConfirms)
        {
            transportExtensions.GetSettings().Set(SettingsKeys.UsePublisherConfirms, usePublisherConfirms);
            return transportExtensions;
        }

        /// <summary>
        /// Specifies the multiplier to apply to the maximum concurrency value to calculate the prefetch count.
        /// </summary>
        /// <param name="transportExtensions"></param>
        /// <param name="prefetchMultiplier">The multiplier value to use in the prefetch calculation.</param>
        public static TransportExtensions<RabbitMQTransport> PrefetchMultiplier(this TransportExtensions<RabbitMQTransport> transportExtensions, int prefetchMultiplier)
        {
            if (prefetchMultiplier <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(prefetchMultiplier), "The prefetch multiplier must be greater than zero.");
            }

            transportExtensions.GetSettings().Set(SettingsKeys.PrefetchMultiplier, prefetchMultiplier);
            return transportExtensions;
        }

        /// <summary>
        /// Overrides the default prefetch count calculation with the specified value.
        /// </summary>
        /// <param name="transportExtensions"></param>
        /// <param name="prefetchCount">The prefetch count to use.</param>
        public static TransportExtensions<RabbitMQTransport> PrefetchCount(this TransportExtensions<RabbitMQTransport> transportExtensions, ushort prefetchCount)
        {
            transportExtensions.GetSettings().Set(SettingsKeys.PrefetchCount, prefetchCount);
            return transportExtensions;
        }

        /// <summary>
        /// Specifies the certificates to use for client authentication when connecting to the broker via TLS.
        /// </summary>
        /// <param name="transportExtensions"></param>
        /// <param name="clientCertificates">The collection of certificates to use for client authentication.</param>
        /// <returns></returns>
        public static TransportExtensions<RabbitMQTransport> SetClientCertificates(this TransportExtensions<RabbitMQTransport> transportExtensions, X509CertificateCollection clientCertificates)
        {
            transportExtensions.GetSettings().Set(SettingsKeys.ClientCertificates, clientCertificates);
            return transportExtensions;
        }

        /// <summary>
        /// Disables all remote certificate validation when connecting to the broker via TLS.
        /// </summary>
        /// <param name="transportExtensions"></param>
        /// <returns></returns>
        public static TransportExtensions<RabbitMQTransport> DisableRemoteCertificateValidation(this TransportExtensions<RabbitMQTransport> transportExtensions)
        {
            transportExtensions.GetSettings().Set(SettingsKeys.DisableRemoteCertificateValidation, true);
            return transportExtensions;
        }

        /// <summary>
        /// Specifies that an external authentication mechanism should be used for client authentication.
        /// </summary>
        /// <param name="transportExtensions"></param>
        /// <returns></returns>
        public static TransportExtensions<RabbitMQTransport> UseExternalAuthMechanism(this TransportExtensions<RabbitMQTransport> transportExtensions)
        {
            transportExtensions.GetSettings().Set(SettingsKeys.UseExternalAuthMechanism, true);
            return transportExtensions;
        }

        /// <summary>
        /// Gets the delayed delivery settings.
        /// </summary>
        /// <param name="transportExtensions"></param>
        /// <returns></returns>
        public static DelayedDeliverySettings DelayedDelivery(this TransportExtensions<RabbitMQTransport> transportExtensions) => new DelayedDeliverySettings(transportExtensions.GetSettings());

        /// <summary>
        /// Specifies whether exchanges and queues should be declared as durable or not.
        /// </summary>
        /// <param name="transportExtensions"></param>
        /// <param name="useDurableExchangesAndQueues">Specifies whether exchanges and queues should be declared as durable or not.</param>
        /// <returns></returns>
        public static TransportExtensions<RabbitMQTransport> UseDurableExchangesAndQueues(this TransportExtensions<RabbitMQTransport> transportExtensions, bool useDurableExchangesAndQueues)
        {
            transportExtensions.GetSettings().Set(SettingsKeys.UseDurableExchangesAndQueues, useDurableExchangesAndQueues);
            return transportExtensions;
        }
    }
}
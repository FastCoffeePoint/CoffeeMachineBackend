﻿using Confluent.Kafka;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Serilog;

namespace Cmb.Common.Kafka;

public class KafkaConsumerBackgroundJob<TEvent> : BackgroundService where TEvent : class, IEvent
{
    private readonly IConsumer<Null, TEvent> _consumer;
    private readonly IServiceProvider _serviceProvider;
    private readonly string _topic;

    public KafkaConsumerBackgroundJob(IOptionsMonitor<KafkaOptions> configuration,
        IServiceProvider serviceProvider)
    {
        var consumerConfiguration = configuration.CurrentValue.Consumers
            .FirstOrDefault(u => u.Topics
                .Any(v => v.Events
                    .Any(j => j == TEvent.Name)))
            ?? throw new Exception($"Consumer configuration for the event {TEvent.Name} wasn't registered");

        var topic = consumerConfiguration.Topics.FirstOrDefault(u => u.Events.Any(v => v == TEvent.Name));
        _topic = topic?.Name  ?? throw new Exception($"Consumer configuration for the event {TEvent.Name} wasn't registered");
        
        var consumerConfig = new ConsumerConfig
        {
            BootstrapServers = consumerConfiguration.BootstrapServers,
            GroupId = consumerConfiguration.GroupId,
            AutoOffsetReset = AutoOffsetReset.Latest,
            EnableAutoCommit = false,
            EnableAutoOffsetStore = false,
        };
        
        _consumer = new ConsumerBuilder<Null, TEvent>(consumerConfig)
                    .SetValueDeserializer(new DefaultJsonDeserializer<TEvent>())
                    .Build();
        
        _serviceProvider = serviceProvider;
    }
    
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        //TODO: looks ugly, but BackgroundService locks the app, because we have _consumer.Consume(...) doesn't hava an async version.
        // Perhaps I should user Hangfire and this call won't fuck my mind at all.
        Task.Run(() => Process(stoppingToken), stoppingToken); 
        return Task.CompletedTask;
    }

    private async Task Process(CancellationToken stoppingToken)
    {
        _consumer.Subscribe(_topic);

        while (!stoppingToken.IsCancellationRequested)
        {
            ConsumeResult<Null, TEvent> consumeResult = null;
            try
            {
                consumeResult = _consumer.Consume(stoppingToken);
                
                var appEvent = consumeResult.Message.Value;
                if (appEvent == null)
                {
                    Log.Warning("PERSON KAFKA CONSUMER ERROR: A message equals null, topic - {0}, message type - {1}", 
                        _topic, typeof(TEvent).ToString());
                    continue;
                }
                
                var handler = _serviceProvider.GetService<KafkaEventHandler<TEvent>>();
                if (handler == null)
                    throw new Exception($"Event handler for {typeof(TEvent)} wasn't registered");

                var shouldCommit = await handler.Handle(appEvent);

                if(shouldCommit)
                    _consumer.Commit();
            }
            catch (Exception e)
            {
                Log.Error(e, "PERSON KAFKA CONSUMER ERROR: message can't be handled because of an exception, topic - {0}, message type - {1}, message - {2}", 
                    _topic, typeof(TEvent).ToString(), consumeResult?.Message);
            }
        }

        _consumer.Close();
    }
}

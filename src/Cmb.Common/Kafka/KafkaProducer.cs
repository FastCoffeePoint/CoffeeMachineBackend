﻿using System.Text;
using System.Text.Json;
using Confluent.Kafka;
namespace Cmb.Common.Kafka;

public interface IKafkaProducer
{
    Task Push<T>(T appEvent) where T: IEvent;
}

public class KafkaProducer : IKafkaProducer
{
    private readonly ProducerConfiguration _configuration;
    private readonly IProducer<Ignore, string> _producer;
    public KafkaProducer(ProducerConfiguration configuration)
    {
        var producerConfig = new ProducerConfig { BootstrapServers = configuration.BootstrapServers };
        _producer = new ProducerBuilder<Ignore, string>(producerConfig)
            .SetKeySerializer(new DefaultJsonSerializer<Ignore>())
            .Build();
        _configuration = configuration;
    }

    public async Task Push<T>(T appEvent) where T: IEvent
    {
        var jsonEvent = JsonSerializer.Serialize(appEvent);
        var kafkaMessage = new Message<Ignore, string>
        {
            Headers = 
            [
                new Header(IEvent.EventTypeHeaderName, Encoding.UTF8.GetBytes(T.Name)),
                new Header(IEvent.AudienceHeaderName, Encoding.UTF8.GetBytes(_configuration.Audience))
            ],
            Value = jsonEvent
        };

        await _producer.ProduceAsync(_configuration.Topic, kafkaMessage);
    }
}

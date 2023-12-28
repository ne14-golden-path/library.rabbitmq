// <copyright file="RabbitMqProducer.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace ne14.library.rabbitmq;

using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using RabbitMQ.Client;

/// <summary>
/// A RabbitMQ producer.
/// </summary>
/// <typeparam name="T">The message type.</typeparam>
public abstract class RabbitMqProducer<T> : ProducerBase, ITypedMqProducer<T>
{
    private readonly RabbitMqSession session;
    private readonly JsonSerializerOptions jsonOpts = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
    };

    /// <summary>
    /// Initializes a new instance of the <see cref="RabbitMqProducer{T}"/> class.
    /// </summary>
    /// <param name="session">The session.</param>
    protected RabbitMqProducer(RabbitMqSession session)
    {
        this.session = session;
        this.session.Channel.ExchangeDeclare(this.ExchangeName, ExchangeType.Fanout, true, false);
    }

    /// <summary>
    /// Gets the exchange name.
    /// </summary>
    public abstract string ExchangeName { get; }

    /// <inheritdoc/>
    public async void Produce(T message)
    {
        var json = JsonSerializer.Serialize(message, this.jsonOpts);
        await this.ProduceAsync(json);
    }

    /// <inheritdoc/>
    protected override async Task ProduceInternal(string message)
    {
        await Task.CompletedTask;
        var bytes = Encoding.UTF8.GetBytes(message);
        this.session.Channel.BasicPublish(this.ExchangeName, string.Empty, null, bytes);
    }
}

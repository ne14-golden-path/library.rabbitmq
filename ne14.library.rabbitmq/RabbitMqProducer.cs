// <copyright file="RabbitMqProducer.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace ne14.library.rabbitmq;

using System.Text;
using System.Text.Json;
using RabbitMQ.Client;

/// <summary>
/// A RabbitMQ producer.
/// </summary>
/// <typeparam name="T">The message type.</typeparam>
public abstract class RabbitMqProducer<T> : ITypedMqProducer<T>
{
    private readonly RabbitMqSession session;

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
    public void Produce(T message)
    {
        var json = JsonSerializer.Serialize(message);
        var bytes = Encoding.UTF8.GetBytes(json);
        this.session.Channel.BasicPublish(this.ExchangeName, string.Empty, null, bytes);
    }
}

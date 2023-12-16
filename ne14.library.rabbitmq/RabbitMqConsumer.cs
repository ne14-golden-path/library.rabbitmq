// <copyright file="RabbitMqConsumer.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace ne14.library.rabbitmq;

using System.Threading.Tasks;

/// <summary>
/// A RabbitMQ consumer.
/// </summary>
/// <typeparam name="T">The message type.</typeparam>
public abstract class RabbitMqConsumer<T> : IMqConsumer<T>
{
    /// <inheritdoc/>
    public abstract Task Consume(T message);
}

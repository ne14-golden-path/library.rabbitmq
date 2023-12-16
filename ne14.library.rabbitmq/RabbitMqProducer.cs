// <copyright file="RabbitMqProducer.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace ne14.library.rabbitmq;

using System.Threading.Tasks;

/// <summary>
/// A RabbitMQ producer.
/// </summary>
/// <typeparam name="T">The message type.</typeparam>
public abstract class RabbitMqProducer<T> : IMqProducer<T>
{
    /// <inheritdoc/>
    public abstract Task Produce(T message);
}

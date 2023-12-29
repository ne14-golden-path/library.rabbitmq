// <copyright file="ITypedMqConsumer.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace ne14.library.rabbitmq.Consumer;

using System.Threading.Tasks;

/// <summary>
/// A message consumer.
/// </summary>
/// <typeparam name="T">The message type.</typeparam>
public interface ITypedMqConsumer<in T> : IMqConsumer
{
    /// <summary>
    /// Consumes a message.
    /// </summary>
    /// <param name="message">The message.</param>
    /// <param name="context">The consumer context.</param>
    /// <returns>Async task.</returns>
    public Task Consume(T message, ConsumerContext context);
}

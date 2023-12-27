// <copyright file="ITypedMqConsumer.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace ne14.library.rabbitmq;

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
    /// <param name="messageId">The message id.</param>
    /// <param name="message">The message.</param>
    /// <param name="attempt">The attempt number.</param>
    /// <returns>Async task.</returns>
    public Task Consume(object messageId, T message, int attempt);
}

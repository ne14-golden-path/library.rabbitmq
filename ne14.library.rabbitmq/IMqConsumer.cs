// <copyright file="IMqConsumer.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace ne14.library.rabbitmq;

using System.Threading.Tasks;

/// <summary>
/// A message consumer.
/// </summary>
/// <typeparam name="T">The message type.</typeparam>
public interface IMqConsumer<in T>
{
    /// <summary>
    /// Consumes a message.
    /// </summary>
    /// <param name="message">The message.</param>
    /// <returns>Async task.</returns>
    public Task Consume(T message);
}

// <copyright file="IMqConsumer.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace ne14.library.rabbitmq.Consumer;

using System.Threading.Tasks;

/// <summary>
/// That which consumes mq messages.
/// </summary>
public interface IMqConsumer
{
    /// <summary>
    /// Consumes a message.
    /// </summary>
    /// <param name="messageBytes">The raw message bytes.</param>
    /// <param name="context">The consumer context.</param>
    /// <returns>Async task.</returns>
    public Task ConsumeAsync(byte[] messageBytes, ConsumerContext context);
}

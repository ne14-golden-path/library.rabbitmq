// <copyright file="IMqProducer.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace ne14.library.rabbitmq.Producer;

using System.Threading.Tasks;

/// <summary>
/// That which produces mq messages.
/// </summary>
public interface IMqProducer
{
    /// <summary>
    /// Produces a raw message.
    /// </summary>
    /// <param name="message">The message.</param>
    /// <returns>Async task.</returns>
    public Task ProduceAsync(string message);
}

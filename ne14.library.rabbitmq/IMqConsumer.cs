﻿// <copyright file="IMqConsumer.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace ne14.library.rabbitmq;

using System.Threading.Tasks;

/// <summary>
/// That which consumes mq messages.
/// </summary>
public interface IMqConsumer
{
    /// <summary>
    /// Consumes a raw message.
    /// </summary>
    /// <param name="messageId">The message id.</param>
    /// <param name="json">The raw message json.</param>
    /// <param name="attempt">The attempt number.</param>
    /// <returns>Async task.</returns>
    public Task ConsumeAsync(object messageId, string json, int attempt);
}

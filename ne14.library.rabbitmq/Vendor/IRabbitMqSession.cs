// <copyright file="IRabbitMqSession.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace ne14.library.rabbitmq.Vendor;

using RabbitMQ.Client;

/// <summary>
/// A rabbit mq session.
/// </summary>
public interface IRabbitMqSession
{
    /// <summary>
    /// Gets the channel.
    /// </summary>
    public IModel Channel { get; }
}

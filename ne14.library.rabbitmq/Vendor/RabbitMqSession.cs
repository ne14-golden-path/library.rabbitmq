// <copyright file="RabbitMqSession.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace ne14.library.rabbitmq.Vendor;

using System;
using RabbitMQ.Client;

/// <summary>
/// Rabbit MQ session.
/// </summary>
public sealed class RabbitMqSession : IRabbitMqSession, IDisposable
{
    private readonly IConnection connection;

    /// <summary>
    /// Initializes a new instance of the <see cref="RabbitMqSession"/> class.
    /// </summary>
    /// <param name="factory">The connection factory.</param>
    public RabbitMqSession(IConnectionFactory factory)
    {
        this.connection = factory?.CreateConnection() ?? throw new ArgumentNullException(nameof(factory));
        this.Channel = this.connection.CreateModel();
    }

    /// <summary>
    /// Gets the channel.
    /// </summary>
    public IModel Channel { get; }

    /// <inheritdoc/>
    public void Dispose()
    {
        GC.SuppressFinalize(this);
        this.Channel.Close();
        this.connection.Close();
        this.connection.Dispose();
    }
}

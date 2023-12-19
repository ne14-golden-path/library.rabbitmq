// <copyright file="RabbitMqSession.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace ne14.library.rabbitmq;

using System;
using RabbitMQ.Client;

/// <summary>
/// Rabbit MQ session.
/// </summary>
public sealed class RabbitMqSession : IDisposable
{
    private readonly IConnection connection;

    /// <summary>
    /// Initializes a new instance of the <see cref="RabbitMqSession"/> class.
    /// </summary>
    /// <param name="username">The user name.</param>
    /// <param name="password">The password.</param>
    /// <param name="hostname">The host name.</param>
    public RabbitMqSession(string username, string password, string hostname)
    {
        var factory = new ConnectionFactory
        {
            UserName = username,
            Password = password,
            HostName = hostname,
        };

        this.connection = factory.CreateConnection();
        this.Channel = this.connection.CreateModel();
    }

    /// <summary>
    /// Gets the channel.
    /// </summary>
    internal IModel Channel { get; }

    /// <inheritdoc/>
    public void Dispose()
    {
        GC.SuppressFinalize(this);
        this.Channel.Close();
        this.connection.Close();
        this.connection.Dispose();
    }
}

// <copyright file="RabbitMqConsumer.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace ne14.library.rabbitmq;

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ne14.library.rabbitmq.Exceptions;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

/// <summary>
/// A RabbitMQ consumer.
/// </summary>
/// <typeparam name="T">The message type.</typeparam>
public abstract class RabbitMqConsumer<T> : ConsumerBase, ITypedMqConsumer<T>
{
    private readonly RabbitMqSession session;
    private readonly AsyncEventingBasicConsumer consumer;
    private readonly Regex kebabCaseRegex = new("(?<!^)([A-Z][a-z]|(?<=[a-z])[A-Z0-9])");
    private readonly JsonSerializerOptions jsonOpts = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    private string? consumerTag;

    /// <summary>
    /// Initializes a new instance of the <see cref="RabbitMqConsumer{T}"/> class.
    /// </summary>
    /// <param name="session">The session.</param>
    protected RabbitMqConsumer(RabbitMqSession session)
    {
        this.session = session;
        this.AppName = Assembly.GetCallingAssembly().GetName().Name;
        this.QueueName = this.ToKebabCase($"q-{this.AppName}-{this.ExchangeName}");

        var queueArgs = new Dictionary<string, object> { ["x-queue-type"] = "quorum" };
        this.session.Channel.ExchangeDeclare(this.ExchangeName, ExchangeType.Fanout, true, false);
        this.session.Channel.QueueDeclare(this.QueueName, true, false, false, queueArgs);
        this.session.Channel.QueueBind(this.QueueName, this.ExchangeName, string.Empty);

        this.consumer = new AsyncEventingBasicConsumer(this.session.Channel);
    }

    /// <summary>
    /// Gets the app name.
    /// </summary>
    public virtual string AppName { get; }

    /// <summary>
    /// Gets the exchange name.
    /// </summary>
    public abstract string ExchangeName { get; }

    /// <summary>
    /// Gets the queue name.
    /// </summary>
    public string QueueName { get; }

    /// <inheritdoc/>
    public abstract Task Consume(object messageId, T message, int attempt);

    /// <inheritdoc/>
    protected override async Task StartInternal()
    {
        await Task.CompletedTask;
        if (this.consumerTag == null)
        {
            this.consumer.Received += this.HandleAsync;
            this.consumerTag = this.session.Channel.BasicConsume(this.QueueName, false, this.consumer);
        }
    }

    /// <inheritdoc/>
    protected override async Task StopInternal()
    {
        await Task.CompletedTask;
        this.consumer.Received -= this.HandleAsync;
        if (this.consumerTag != null)
        {
            this.session.Channel.BasicCancel(this.consumerTag);
            this.consumerTag = null;
        }
    }

    /// <inheritdoc/>
    protected override async Task ConsumeInternal(object messageId, string json, int attempt)
    {
        T? typedMessage;
        try
        {
            typedMessage = JsonSerializer.Deserialize<T>(json, this.jsonOpts);
        }
        catch (Exception ex)
        {
            throw new PermanentFailureException("Error parsing json", ex);
        }

        if (typedMessage != null)
        {
            await this.Consume(messageId, typedMessage, attempt);
        }
    }

    /// <inheritdoc/>
    protected override async Task OnConsumeSuccess(object messageId, string json, int attempt)
    {
        await Task.CompletedTask;
        this.session.Channel.BasicAck((ulong)messageId, false);
    }

    /// <inheritdoc/>
    protected override async Task OnConsumeFailure(object messageId, string json, int attempt, bool retry)
    {
        await Task.CompletedTask;
        this.session.Channel.BasicNack((ulong)messageId, false, requeue: retry);
    }

    private string ToKebabCase(string str)
        => this.kebabCaseRegex.Replace(str, "-$1").Trim().ToLower();

    private async Task HandleAsync(object sender, BasicDeliverEventArgs args)
    {
        var headers = args.BasicProperties.Headers ?? new Dictionary<string, object>();
        var hasCount = headers.TryGetValue("x-delivery-count", out var countObject);
        var attempt = hasCount && int.TryParse(countObject.ToString(), out var count) ? count + 1 : 1;
        var json = Encoding.UTF8.GetString(args.Body.ToArray());
        await this.ConsumeAsync(args.DeliveryTag, json, attempt);
    }
}

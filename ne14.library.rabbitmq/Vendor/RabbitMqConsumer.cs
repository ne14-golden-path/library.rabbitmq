// <copyright file="RabbitMqConsumer.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace ne14.library.rabbitmq.Vendor;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ne14.library.rabbitmq.Consumer;
using ne14.library.rabbitmq.Exceptions;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

/// <summary>
/// A RabbitMQ consumer.
/// </summary>
/// <typeparam name="T">The message type.</typeparam>
public abstract class RabbitMqConsumer<T> : ConsumerBase, ITypedMqConsumer<T>
{
    private const string DefaultRoute = "DEFAULT";
    private const string Tier1Route = "TIER1_RETRY";
    private const string Tier2Route = "TIER2_DLQ";

    private readonly IRabbitMqSession session;
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
    protected RabbitMqConsumer(IRabbitMqSession session)
    {
        this.session = session;
        this.AppName = Assembly.GetCallingAssembly().GetName().Name;

        // Main handler queue
        this.QueueName = this.ToKebabCase($"q-{this.AppName}-{this.ExchangeName}");
        var mainQArgs = new Dictionary<string, object>
        {
            ["x-dead-letter-exchange"] = this.ExchangeName,
            ["x-dead-letter-routing-key"] = Tier1Route,
        };
        this.session.Channel.ExchangeDeclare(this.ExchangeName, ExchangeType.Direct, true);
        this.session.Channel.QueueDeclare(this.QueueName, true, false, false, mainQArgs);
        this.session.Channel.QueueBind(this.QueueName, this.ExchangeName, DefaultRoute);

        // Tier 1 Failure: Retry
        var tier1Queue = this.QueueName + "_" + Tier1Route;
        var retryQArgs = new Dictionary<string, object>
        {
            ["x-dead-letter-exchange"] = this.ExchangeName,
            ["x-dead-letter-routing-key"] = DefaultRoute,
        };
        this.session.Channel.QueueDeclare(tier1Queue, true, false, false, retryQArgs);
        this.session.Channel.QueueBind(tier1Queue, this.ExchangeName, Tier1Route);

        // Tier 2 Failure: Dead-letter
        var tier2Queue = this.QueueName + "_" + Tier2Route;
        this.session.Channel.QueueDeclare(tier2Queue, true, false, false);
        this.session.Channel.QueueBind(tier2Queue, this.ExchangeName, Tier2Route);

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
    public abstract Task Consume(T message, ConsumerContext context);

    /// <inheritdoc/>
    protected override Task StartInternal()
    {
        if (this.consumerTag == null)
        {
            // Stryker disable once Assignment
            this.consumer.Received += this.HandleAsync;
            this.consumerTag = this.session.Channel.BasicConsume(this.QueueName, false, this.consumer);
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    protected override Task StopInternal()
    {
        // Stryker disable once Assignment
        this.consumer.Received -= this.HandleAsync;
        if (this.consumerTag != null)
        {
            this.session.Channel.BasicCancel(this.consumerTag);
            this.consumerTag = null;
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    protected override async Task ConsumeInternal(string json, ConsumerContext context)
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
            await this.Consume(typedMessage, context);
        }
    }

    /// <inheritdoc/>
    protected override Task OnConsumeSuccess(string json, ConsumerContext context)
    {
        context = context ?? throw new ArgumentNullException(nameof(context));
        this.session.Channel.BasicAck((ulong)context.DeliveryId, false);
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    protected override Task OnConsumeFailure(string json, ConsumerContext context, bool retry)
    {
        context = context ?? throw new ArgumentNullException(nameof(context));

        if (retry)
        {
            this.session.Channel.BasicNack((ulong)context.DeliveryId, false, false);
        }
        else
        {
            var ogBytes = Encoding.UTF8.GetBytes(json);
            this.session.Channel.BasicAck((ulong)context.DeliveryId, false);
            this.session.Channel.BasicPublish(this.ExchangeName, Tier2Route, null, ogBytes);
        }

        return Task.CompletedTask;
    }

    private string ToKebabCase(string str)
        => this.kebabCaseRegex.Replace(str, "-$1").Trim().ToLower();

    [ExcludeFromCodeCoverage]
    private async Task HandleAsync(object sender, BasicDeliverEventArgs args)
    {
        var attempt = 1L;
        var bornOn = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var headers = args.BasicProperties.Headers ?? new Dictionary<string, object>();
        if (headers.TryGetValue("x-death", out var death) && death is List<object> deathList)
        {
            var dicto = (Dictionary<string, object>)deathList[0];
            attempt = dicto.TryGetValue("count", out var countObj) ? (long)countObj : attempt;
            bornOn = dicto.TryGetValue("time", out var timeObj) ? ((AmqpTimestamp)timeObj).UnixTime : bornOn;
        }

        var context = new ConsumerContext(bornOn, attempt, args.DeliveryTag);
        await this.ConsumeAsync(args.Body.ToArray(), context);
    }
}

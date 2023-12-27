// <copyright file="RabbitMqConsumer.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace ne14.library.rabbitmq;

using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
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
    private string? consumerTag;

    /// <summary>
    /// Initializes a new instance of the <see cref="RabbitMqConsumer{T}"/> class.
    /// </summary>
    /// <param name="session">The session.</param>
    protected RabbitMqConsumer(RabbitMqSession session)
    {
        this.session = session;
        this.session.Channel.ExchangeDeclare(this.ExchangeName, ExchangeType.Fanout, true, false);
        this.session.Channel.QueueDeclare(this.QueueName, true, false, false);
        this.session.Channel.QueueBind(this.QueueName, this.ExchangeName, string.Empty);
        this.consumer = new AsyncEventingBasicConsumer(this.session.Channel);
    }

    /// <summary>
    /// Gets the app name.
    /// </summary>
    public abstract string AppName { get; }

    /// <summary>
    /// Gets the exchange name.
    /// </summary>
    public abstract string ExchangeName { get; }

    /// <summary>
    /// Gets the queue name.
    /// </summary>
    public string QueueName => $"q-{this.AppName}-{this.ExchangeName}";

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
        var typedMessage = JsonSerializer.Deserialize<T>(json);
        await this.Consume(messageId, typedMessage!, attempt);
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

    private async Task HandleAsync(object sender, BasicDeliverEventArgs args)
    {
        var hasCount = args.BasicProperties.Headers.TryGetValue("x-delivery-count", out var countObject);
        var attempt = hasCount && int.TryParse(countObject.ToString(), out var count) ? count + 1 : 1;
        var json = Encoding.UTF8.GetString(args.Body.ToArray());
        await this.ConsumeAsync(args.DeliveryTag, json, attempt);
    }
}

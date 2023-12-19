// <copyright file="RabbitMqConsumer.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace ne14.library.rabbitmq;

using System.Diagnostics;
using System.Text.Json;
using System.Threading.Tasks;
using ne14.library.rabbitmq.Exceptions;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

/// <summary>
/// A RabbitMQ consumer.
/// </summary>
/// <typeparam name="T">The message type.</typeparam>
public abstract class RabbitMqConsumer<T> : IMqConsumer<T>
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
        this.Subscribe();
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
    public abstract Task Consume(T message, int attemptNumber);

    /// <summary>
    /// Starts listening.
    /// </summary>
    public void Subscribe()
    {
        Debug.WriteLine("Subscribing...");

        if (this.consumerTag == null)
        {
            this.consumer.Received += this.HandleAsync;
            this.consumerTag = this.session.Channel.BasicConsume(this.QueueName, false, this.consumer);

            Debug.WriteLine("Subscribed!");
        }
        else
        {
            Debug.WriteLine("Could not subscribe; consumer tag is already populated.");
        }
    }

    /// <summary>
    /// Stops listening.
    /// </summary>
    public void Unsubscribe()
    {
        Debug.WriteLine("Unsubscribing...");

        this.consumer.Received -= this.HandleAsync;
        if (this.consumerTag != null)
        {
            this.session.Channel.BasicCancel(this.consumerTag);
            this.consumerTag = null;

            Debug.WriteLine("Unsubscribed");
        }
        else
        {
            Debug.WriteLine("Could not unsubscribe; consumer tag is not populated.");
        }
    }

    private async Task HandleAsync(object sender, BasicDeliverEventArgs args)
    {
        if (this.consumerTag == null)
        {
            Debug.WriteLine("Refusing to consumer; consumer tag is not populated.");
        }

        try
        {
            var message = JsonSerializer.Deserialize<T>(args.Body.ToArray());
            var attempt = 1 + (int)args.BasicProperties.Headers["x-delivery-count"];
            await this.Consume(message!, attempt);
            this.session.Channel.BasicAck(args.DeliveryTag, false);

            Debug.WriteLine("Message ACK'ed.");
        }
        catch (TransientFailureException)
        {
            this.session.Channel.BasicNack(args.DeliveryTag, false, requeue: true);

            Debug.WriteLine("Message NACK'ed temporarily.");
        }
        catch
        {
            this.session.Channel.BasicNack(args.DeliveryTag, false, requeue: false);

            Debug.WriteLine("Message NACK'ed permanently.");
        }
    }
}

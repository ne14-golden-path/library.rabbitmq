// <copyright file="ProducerBase.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace ne14.library.rabbitmq;

using System.Threading.Tasks;

/// <summary>
/// Base producer implementation.
/// </summary>
public abstract class ProducerBase : IMqProducer
{
    /// <inheritdoc/>
    public async Task ProduceAsync(string message)
    {
        await this.OnProducing(message);
        await this.ProduceInternal(message);
        await this.OnProduced(message);
    }

    /// <summary>
    /// Internal implementation for producing a message.
    /// </summary>
    /// <param name="message">The message.</param>
    /// <returns>Async task.</returns>
    protected abstract Task ProduceInternal(string message);

    /// <summary>
    /// Fired when a message is about to be produced.
    /// </summary>
    /// <param name="message">The message.</param>
    /// <returns>Async task.</returns>
    protected virtual Task OnProducing(string message) => Task.CompletedTask;

    /// <summary>
    /// Fired when a message has been successfully produced.
    /// </summary>
    /// <param name="message">The message.</param>
    /// <returns>Async task.</returns>
    protected virtual Task OnProduced(string message) => Task.CompletedTask;
}

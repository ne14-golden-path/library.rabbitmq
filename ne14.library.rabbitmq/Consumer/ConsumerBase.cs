// <copyright file="ConsumerBase.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace ne14.library.rabbitmq.Consumer;

using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using ne14.library.rabbitmq.Exceptions;

/// <summary>
/// Base consumer implementation.
/// </summary>
public abstract class ConsumerBase : IMqConsumer, IHostedService
{
    /// <inheritdoc/>
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await this.OnServiceStarting();
        await this.StartInternal();
        await this.OnServiceStarted();
    }

    /// <inheritdoc/>
    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await this.OnServiceStopping();
        await this.StopInternal();
        await this.OnServiceStopped();
    }

    /// <inheritdoc/>
    public async Task ConsumeAsync(byte[] messageBytes, ConsumerContext context)
    {
        var json = Encoding.UTF8.GetString(messageBytes);
        await this.OnConsuming(json, context);
        try
        {
            await this.ConsumeInternal(json, context);
        }
        catch (Exception ex)
        {
            var doRetry = await this.DoRetry(ex, json, context);
            await this.OnConsumeFailure(json, context, doRetry);
            return;
        }

        await this.OnConsumeSuccess(json, context);
    }

    /// <summary>
    /// Internal handler for service start.
    /// </summary>
    /// <returns>Async task.</returns>
    protected abstract Task StartInternal();

    /// <summary>
    /// Internal handler for service stop.
    /// </summary>
    /// <returns>Async task.</returns>
    protected abstract Task StopInternal();

    /// <summary>
    /// Internal handler for consuming a message.
    /// </summary>
    /// <param name="json">The raw message.</param>
    /// <param name="context">The consumer context.</param>
    /// <returns>Async task.</returns>
    protected abstract Task ConsumeInternal(string json, ConsumerContext context);

    /// <summary>
    /// Fired when a message is received and about to be consumed.
    /// </summary>
    /// <param name="json">The raw message.</param>
    /// <param name="context">The consumer context.</param>
    /// <returns>Async task.</returns>
    protected virtual Task OnConsuming(string json, ConsumerContext context) => Task.CompletedTask;

    /// <summary>
    /// Fired when consumer code completed without error.
    /// </summary>
    /// <param name="json">The raw message.</param>
    /// <param name="context">The consumer context.</param>
    /// <returns>Async task.</returns>
    protected abstract Task OnConsumeSuccess(string json, ConsumerContext context);

    /// <summary>
    /// Fired when consumer code threw an exception.
    /// </summary>
    /// <param name="json">The raw message.</param>
    /// <param name="context">The consumer context.</param>
    /// <param name="retry">Whether the message should be re-queued.</param>
    /// <returns>Async task.</returns>
    protected abstract Task OnConsumeFailure(string json, ConsumerContext context, bool retry);

    /// <summary>
    /// Used to determine whether a message should be retried.
    /// </summary>
    /// <param name="ex">The exception.</param>
    /// <param name="json">The raw message.</param>
    /// <param name="context">The consumer context.</param>
    /// <returns>Whether to retry.</returns>
    protected virtual Task<bool> DoRetry(Exception ex, string json, ConsumerContext context)
    {
        var retVal = ex is not PermanentFailureException;
        return Task.FromResult(retVal);
    }

    /// <summary>
    /// Fired when the service is about to start running.
    /// </summary>
    /// <returns>Async task.</returns>
    protected virtual Task OnServiceStarting() => Task.CompletedTask;

    /// <summary>
    /// Fired when the service has started running.
    /// </summary>
    /// <returns>Async task.</returns>
    protected virtual Task OnServiceStarted() => Task.CompletedTask;

    /// <summary>
    /// Fired when the service is about to stop running.
    /// </summary>
    /// <returns>Async task.</returns>
    protected virtual Task OnServiceStopping() => Task.CompletedTask;

    /// <summary>
    /// Fired when the service has stopped running.
    /// </summary>
    /// <returns>Async task.</returns>
    protected virtual Task OnServiceStopped() => Task.CompletedTask;
}

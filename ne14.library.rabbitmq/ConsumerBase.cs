// <copyright file="ConsumerBase.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace ne14.library.rabbitmq;

using System;
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
    public async Task ConsumeAsync(object messageId, string json, int attempt)
    {
        await this.OnConsuming(messageId, json, attempt);
        try
        {
            await this.ConsumeInternal(messageId, json, attempt);
            await this.OnConsumeSuccess(messageId, json, attempt);
        }
        catch (Exception ex)
        {
            var doRetry = await this.DoRetry(ex, attempt, json);
            await this.OnConsumeFailure(messageId, json, attempt, doRetry);
        }
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
    /// <param name="messageId">The message id.</param>
    /// <param name="json">The raw message json.</param>
    /// <param name="attempt">The attempt number.</param>
    /// <returns>Async task.</returns>
    protected abstract Task ConsumeInternal(object messageId, string json, int attempt);

    /// <summary>
    /// Fired when a message is received and about to be consumed.
    /// </summary>
    /// <param name="messageId">The message id.</param>
    /// <param name="json">The raw message json.</param>
    /// <param name="attempt">The attempt number.</param>
    /// <returns>Async task.</returns>
    protected virtual Task OnConsuming(object messageId, string json, int attempt) => Task.CompletedTask;

    /// <summary>
    /// Fired when consumer code completed without error.
    /// </summary>
    /// <param name="messageId">The message id.</param>
    /// <param name="json">The raw message json.</param>
    /// <param name="attempt">The attempt number.</param>
    /// <returns>Async task.</returns>
    protected abstract Task OnConsumeSuccess(object messageId, string json, int attempt);

    /// <summary>
    /// Fired when consumer code threw an exception.
    /// </summary>
    /// <param name="messageId">The message id.</param>
    /// <param name="json">The raw message json.</param>
    /// <param name="attempt">The attempt number.</param>
    /// <param name="retry">Whether the message should be re-queued.</param>
    /// <returns>Async task.</returns>
    protected abstract Task OnConsumeFailure(object messageId, string json, int attempt, bool retry);

    /// <summary>
    /// Used to determine whether a message should be retried.
    /// </summary>
    /// <param name="ex">The exception.</param>
    /// <param name="attempt">The attempt number.</param>
    /// <param name="json">The raw message json.</param>
    /// <returns>Whether to retry.</returns>
    protected virtual async Task<bool> DoRetry(Exception ex, int attempt, string json)
    {
        await Task.CompletedTask;
        return ex is not PermanentFailureException;
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

// <copyright file="ConsumerContext.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace ne14.library.rabbitmq.Consumer;

/// <summary>
/// Implementation relating to the mq consumer context.
/// </summary>
public record ConsumerContext
{
    /// <summary>
    /// Gets the message id.
    /// </summary>
    public object MessageId { get; init; } = default!;

    /// <summary>
    /// Gets the attempt number.
    /// </summary>
    public int AttemptNumber { get; init; }
}

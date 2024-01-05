// <copyright file="ConsumerContext.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace ne14.library.rabbitmq.Consumer;

/// <summary>
/// Implementation relating to the mq consumer context.
/// </summary>
/// <param name="BornOn">Unix time the message was first received.</param>
/// <param name="AttemptNumber">The attempt number.</param>
/// <param name="DeliveryId">The delivery id (for broken correlation).</param>
public record ConsumerContext(
    long BornOn,
    long AttemptNumber,
    object DeliveryId);
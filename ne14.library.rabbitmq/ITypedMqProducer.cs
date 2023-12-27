// <copyright file="ITypedMqProducer.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace ne14.library.rabbitmq;

/// <summary>
/// A message producer.
/// </summary>
/// <typeparam name="T">The message type.</typeparam>
public interface ITypedMqProducer<in T>
{
    /// <summary>
    /// Produces a message.
    /// </summary>
    /// <param name="message">The message.</param>
    public void Produce(T message);
}

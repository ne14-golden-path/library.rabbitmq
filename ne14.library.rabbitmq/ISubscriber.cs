// <copyright file="ISubscriber.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace ne14.library.rabbitmq;

using System.Threading.Tasks;

/// <summary>
/// That which may (un)subscribe.
/// </summary>
public interface ISubscriber
{
    /// <summary>
    /// Subscribes.
    /// </summary>
    /// <returns>Async task.</returns>
    public Task Subscribe();

    /// <summary>
    /// Unsubscribes.
    /// </summary>
    /// <returns>Async task.</returns>
    public Task Unsubscribe();
}

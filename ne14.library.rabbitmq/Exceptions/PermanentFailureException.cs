﻿// <copyright file="PermanentFailureException.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace ne14.library.rabbitmq.Exceptions;

using System;

/// <summary>
/// A transient error.
/// </summary>
public class PermanentFailureException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PermanentFailureException"/> class.
    /// </summary>
    public PermanentFailureException()
    { }

    /// <summary>
    /// Initializes a new instance of the <see cref="PermanentFailureException"/> class.
    /// </summary>
    /// <param name="message">The message.</param>
    public PermanentFailureException(string message)
        : base(message)
    { }

    /// <summary>
    /// Initializes a new instance of the <see cref="PermanentFailureException"/> class.
    /// </summary>
    /// <param name="message">The message.</param>
    /// <param name="innerException">The underlying exception.</param>
    public PermanentFailureException(string message, Exception innerException)
        : base(message, innerException)
    { }
}

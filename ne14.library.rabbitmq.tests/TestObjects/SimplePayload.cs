// <copyright file="SimplePayload.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace ne14.library.rabbitmq.tests.TestObjects;

public class SimplePayload
{
    public string? Foo { get; set; }

    public bool? SimulateRetry { get; set; }
}

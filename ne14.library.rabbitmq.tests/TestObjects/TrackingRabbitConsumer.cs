// <copyright file="TrackingRabbitConsumer.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace ne14.library.rabbitmq.tests.TestObjects;

using System.Collections.ObjectModel;
using ne14.library.rabbitmq.Consumer;
using ne14.library.rabbitmq.Exceptions;
using ne14.library.rabbitmq.Vendor;

public class TrackingRabbitConsumer(IRabbitMqSession session)
    : RabbitMqConsumer<SimplePayload>(session)
{
    public Collection<string> Lifecycle { get; } = [];

    public override string ExchangeName => "tracking-thing";

    public override Task Consume(SimplePayload message, ConsumerContext context)
    {
        return message.SimulateRetry switch
        {
            true => throw new TransientFailureException(),
            false => throw new PermanentFailureException(),
            _ => Task.CompletedTask,
        };
    }

    protected override Task OnConsuming(string json, ConsumerContext context)
    {
        this.Lifecycle.Add("consuming");
        return Task.CompletedTask;
    }

    protected override async Task OnConsumeFailure(string json, ConsumerContext context, bool retry)
    {
        await base.OnConsumeFailure(json, context, retry);
        this.Lifecycle.Add("unconsumed");
    }

    protected override async Task OnConsumeSuccess(string json, ConsumerContext context)
    {
        await base.OnConsumeSuccess(json, context);
        this.Lifecycle.Add("consumed");
    }

    protected override Task OnServiceStarting()
    {
        this.Lifecycle.Add("starting");
        return Task.CompletedTask;
    }

    protected override Task OnServiceStarted()
    {
        this.Lifecycle.Add("started");
        return Task.CompletedTask;
    }

    protected override Task OnServiceStopping()
    {
        this.Lifecycle.Add("stopping");
        return Task.CompletedTask;
    }

    protected override Task OnServiceStopped()
    {
        this.Lifecycle.Add("stopped");
        return Task.CompletedTask;
    }
}

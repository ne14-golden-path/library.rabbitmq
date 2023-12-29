// <copyright file="SimpleRabbitProducer.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace ne14.library.rabbitmq.tests.TestObjects;

using System.Collections.ObjectModel;
using System.Threading.Tasks;
using ne14.library.rabbitmq.Vendor;

public class SimpleRabbitProducer(IRabbitMqSession session)
    : RabbitMqProducer<SimplePayload>(session)
{
    public Collection<string> Lifecycle { get; } = [];

    public override string ExchangeName => "simple-thing";

    protected override Task OnProducing(string message)
    {
        this.Lifecycle.Add("producing");
        return Task.CompletedTask;
    }

    protected override Task OnProduced(string message)
    {
        this.Lifecycle.Add("produced");
        return Task.CompletedTask;
    }
}

// <copyright file="BasicRabbitConsumer.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace ne14.library.rabbitmq.tests.TestObjects;

using ne14.library.rabbitmq.Consumer;
using ne14.library.rabbitmq.Vendor;

public class BasicRabbitConsumer(IRabbitMqSession session)
    : RabbitMqConsumer<SimplePayload>(session)
{
    public override string ExchangeName => "basic-thing";

    public override Task Consume(SimplePayload message, ConsumerContext context)
        => Task.CompletedTask;
}

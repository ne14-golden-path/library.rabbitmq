// <copyright file="BasicRabbitProducer.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace ne14.library.rabbitmq.tests.TestObjects;

using ne14.library.rabbitmq.Vendor;

public class BasicRabbitProducer(IRabbitMqSession session)
    : RabbitMqProducer<SimplePayload>(session)
{
    public override string ExchangeName => "basic-thing";
}

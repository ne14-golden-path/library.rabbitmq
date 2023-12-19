// <copyright file="UnitTest1.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace ne14.library.rabbitmq.tests;

public class UnitTest1
{
    [Fact]
    public async Task Test1()
    {
        using var sesh = new RabbitMqSession("guest", "guest", "localhost");
        _ = new TestApp1Consumer(sesh);
        _ = new TestApp2Consumer(sesh);

        var testProducer = new TestProducer(sesh);
        testProducer.Produce(new("hello, world!"));

        await Task.CompletedTask;
        1.Should().Be(1);
    }

    private record TestPayload(string? Greeting);

    private class TestApp1Consumer(RabbitMqSession session) : RabbitMqConsumer<TestPayload>(session)
    {
        public override string AppName => "app1";

        public override string ExchangeName => "unit-testing";

        public override async Task Consume(TestPayload message, int attemptNumber)
        {
            await Task.CompletedTask;
        }
    }

    private class TestApp2Consumer(RabbitMqSession session) : RabbitMqConsumer<TestPayload>(session)
    {
        public override string AppName => "app2";

        public override string ExchangeName => "unit-testing";

        public override async Task Consume(TestPayload message, int attemptNumber)
        {
            await Task.CompletedTask;
        }
    }

    private class TestProducer(RabbitMqSession session) : RabbitMqProducer<TestPayload>(session)
    {
        public override string ExchangeName => "unit-testing";
    }
}
// <copyright file="RabbitMqProducerTests.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace ne14.library.rabbitmq.tests.Vendor;

using System.Collections.ObjectModel;
using System.Text;
using ne14.library.rabbitmq.Producer;
using ne14.library.rabbitmq.tests.TestObjects;
using ne14.library.rabbitmq.Vendor;
using RabbitMQ.Client;

/// <summary>
/// Tests for the <see cref="RabbitMqProducer{T}"/> class.
/// </summary>
public class RabbitMqProducerTests
{
    [Fact]
    public void Ctor_WithSession_DeclaresExchange()
    {
        // Arrange & Act
        var sut = GetSut<BasicRabbitProducer>(out var mockChannel);

        // Assert
        mockChannel.Verify(
            m => m.ExchangeDeclare(
                sut.ExchangeName,
                It.IsAny<string>(),
                true,
                false,
                It.IsAny<IDictionary<string, object>>()));
    }

    [Fact]
    public void Produce_WithPayload_PublishesMessage()
    {
        // Arrange
        var sut = GetSut<BasicRabbitProducer>(out var mockChannel);
        var msg = new SimplePayload { Foo = "bar" };
        var expectedBytes = string.Join(',', Encoding.UTF8.GetBytes("{\r\n  \"foo\": \"bar\"\r\n}"));

        // Act
        sut.Produce(msg);

        // Assert
        mockChannel.Verify(
            m => m.BasicPublish(
                sut.ExchangeName,
                string.Empty,
                It.IsAny<bool>(),
                It.IsAny<IBasicProperties>(),
                It.Is<ReadOnlyMemory<byte>>(bytes => string.Join(',', bytes.ToArray()) == expectedBytes)));
    }

    [Fact]
    public void Produce_WithLifecycleHandlers_CallsHandlers()
    {
        // Arrange
        var sut = GetSut<TrackingRabbitProducer>(out var mockChannel);
        var expected = new Collection<string> { "producing", "produced" };

        // Act
        sut.Produce(new());

        // Assert
        sut.Lifecycle.Should().BeEquivalentTo(expected, opts => opts.WithStrictOrdering());
    }

    private static T GetSut<T>(out Mock<IModel> mockChannel)
        where T : ProducerBase
    {
        mockChannel = new Mock<IModel>();
        var mockSession = new Mock<IRabbitMqSession>();
        mockSession.Setup(m => m.Channel).Returns(mockChannel.Object);

        return (T)Activator.CreateInstance(typeof(T), mockSession.Object)!;
    }
}

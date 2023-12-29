// <copyright file="RabbitMqConsumerTests.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace ne14.library.rabbitmq.tests.Vendor;

using System.Collections.ObjectModel;
using System.Text;
using System.Text.Json;
using ne14.library.rabbitmq.Consumer;
using ne14.library.rabbitmq.tests.TestObjects;
using ne14.library.rabbitmq.Vendor;
using RabbitMQ.Client;

/// <summary>
/// Tests for the <see cref="RabbitMqConsumer{T}"/> class.
/// </summary>
public class RabbitMqConsumerTests
{
    [Fact]
    public void Ctor_WithSession_DeclaresExchange()
    {
        // Arrange & Act
        var sut = GetSut<SimpleRabbitConsumer>(out var mockChannel);

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
    public void Ctor_WithSession_DeclaresQueue()
    {
        // Arrange
        var expectedArgs = new Dictionary<string, object> { ["x-queue-type"] = "quorum" };

        // Act
        var sut = GetSut<SimpleRabbitConsumer>(out var mockChannel);

        // Assert
        mockChannel.Verify(
            m => m.QueueDeclare(
                sut.QueueName,
                true,
                false,
                false,
                expectedArgs));
    }

    [Fact]
    public void Ctor_WithSession_BindsQueue()
    {
        // Arrange & Act
        var sut = GetSut<SimpleRabbitConsumer>(out var mockChannel);

        // Assert
        mockChannel.Verify(m => m.QueueBind(sut.QueueName, sut.ExchangeName, string.Empty, null));
    }

    [Fact]
    public void Ctor_WithSession_SetsQueueName()
    {
        // Arrange & Act
        var sut = GetSut<SimpleRabbitConsumer>(out _);
        const string expected = "q-ne-14.library.rabbitmq.tests-simple-thing";

        // Assert
        sut.QueueName.Should().Be(expected);
    }

    [Fact]
    public async Task StartAsync_WithSession_StartsListener()
    {
        // Arrange
        var sut = GetSut<SimpleRabbitConsumer>(out var mockChannel);

        // Act
        await sut.StartAsync(CancellationToken.None);

        // Assert
        mockChannel.Verify(
            m => m.BasicConsume(
                sut.QueueName,
                false,
                It.IsAny<string>(),
                It.IsAny<bool>(),
                It.IsAny<bool>(),
                It.IsAny<IDictionary<string, object>>(),
                It.IsAny<IBasicConsumer>()));
    }

    [Fact]
    public async Task StopAsync_WithSession_StopsListener()
    {
        // Arrange
        var sut = GetSut<SimpleRabbitConsumer>(out var mockChannel);
        await sut.StartAsync(CancellationToken.None); // generate consumer tag

        // Act
        await sut.StopAsync(CancellationToken.None);

        // Assert
        mockChannel.Verify(m => m.BasicCancel(It.IsAny<string>()));
    }

    [Fact]
    public async Task ConsumeAsync_Success_AcksMessage()
    {
        // Arrange
        var sut = GetSut<SimpleRabbitConsumer>(out var mockChannel);
        var bytes = ToBytes(new SimplePayload { Foo = "bar" });
        const ulong messageId = 40;

        // Act
        await sut.ConsumeAsync(bytes, new() { MessageId = messageId });

        // Assert
        mockChannel.Verify(m => m.BasicAck(messageId, false));
    }

    [Fact]
    public async Task ConsumeAsync_TransientFailure_RetriesMessage()
    {
        // Arrange
        var sut = GetSut<SimpleRabbitConsumer>(out var mockChannel);
        var bytes = ToBytes(new SimplePayload { Foo = "bar", SimulateRetry = true });
        const ulong messageId = 41;

        // Act
        await sut.ConsumeAsync(bytes, new() { MessageId = messageId });

        // Assert
        mockChannel.Verify(m => m.BasicNack(messageId, false, true));
    }

    [Fact]
    public async Task ConsumeAsync_PermanentFailure_AbandonsMessage()
    {
        // Arrange
        var sut = GetSut<SimpleRabbitConsumer>(out var mockChannel);
        var bytes = ToBytes(new SimplePayload { Foo = "bar", SimulateRetry = false });
        const ulong messageId = 42;

        // Act
        await sut.ConsumeAsync(bytes, new() { MessageId = messageId });

        // Assert
        mockChannel.Verify(m => m.BasicNack(messageId, false, false));
    }

    [Fact]
    public async Task ConsumeAsync_InvalidJson_AbandonsMessage()
    {
        // Arrange
        var sut = GetSut<SimpleRabbitConsumer>(out var mockChannel);
        var bytes = ToBytes(new { Foo = "bar", SimulateRetry = "sheep" });
        const ulong messageId = 43;

        // Act
        await sut.ConsumeAsync(bytes, new() { MessageId = messageId });

        // Assert
        mockChannel.Verify(m => m.BasicNack(messageId, false, false));
    }

    [Fact]
    public async Task ConsumeAsync_MixedCaseJsonProperty_StillRecognisedAsInvalid()
    {
        // Arrange
        var sut = GetSut<SimpleRabbitConsumer>(out var mockChannel);
        var bytes = ToBytes(new { Foo = "bar", simUlATeReTRy = "sheep" });
        const ulong messageId = 44;

        // Act
        await sut.ConsumeAsync(bytes, new() { MessageId = messageId });

        // Assert
        mockChannel.Verify(m => m.BasicNack(messageId, false, false));
    }

    [Fact]
    public async Task ConsumeAsync_SuccessNoContext_ThrowsArgNull()
    {
        // Arrange
        var sut = GetSut<SimpleRabbitConsumer>(out var mockChannel);
        var bytes = ToBytes(new SimplePayload());

        // Act
        var act = () => sut.ConsumeAsync(bytes, null!);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>()
            .WithMessage("Value cannot be null. (Parameter 'context')");
    }

    [Fact]
    public async Task ConsumeAsync_FailureNoContext_ThrowsArgNull()
    {
        // Arrange
        var sut = GetSut<SimpleRabbitConsumer>(out var mockChannel);
        var bytes = ToBytes(new SimplePayload { SimulateRetry = true });

        // Act
        var act = () => sut.ConsumeAsync(bytes, null!);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>()
            .WithMessage("Value cannot be null. (Parameter 'context')");
    }

    [Fact]
    public async Task LifecycleMethods_WithHandler_CapturesExpected()
    {
        // Arrange
        var sut = GetSut<SimpleRabbitConsumer>(out var mockChannel);
        var bytes = ToBytes(new SimplePayload());
        var expected = new Collection<string> { "starting", "started", "consuming", "stopping", "stopped" };

        // Act
        await sut.StartAsync(CancellationToken.None);
        await sut.ConsumeAsync(bytes, new() { MessageId = 1ul });
        await sut.StopAsync(CancellationToken.None);

        // Assert
        sut.Lifecycle.Should().BeEquivalentTo(expected, opts => opts.WithStrictOrdering());
    }

    private static T GetSut<T>(out Mock<IModel> mockChannel)
        where T : ConsumerBase
    {
        mockChannel = new Mock<IModel>();
        var mockSession = new Mock<IRabbitMqSession>();
        mockSession.Setup(m => m.Channel).Returns(mockChannel.Object);
        mockChannel
            .Setup(m => m.BasicConsume(
                It.IsAny<string>(),
                It.IsAny<bool>(),
                It.IsAny<string>(),
                It.IsAny<bool>(),
                It.IsAny<bool>(),
                It.IsAny<IDictionary<string, object>>(),
                It.IsAny<IBasicConsumer>()))
            .Returns("fake-consumer-tag");

        return (T)Activator.CreateInstance(typeof(T), mockSession.Object)!;
    }

    private static byte[] ToBytes(object obj)
        => Encoding.UTF8.GetBytes(JsonSerializer.Serialize(obj));
}

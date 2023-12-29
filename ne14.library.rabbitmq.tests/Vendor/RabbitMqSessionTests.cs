// <copyright file="RabbitMqSessionTests.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace ne14.library.rabbitmq.tests.Vendor;

using ne14.library.rabbitmq.Vendor;
using RabbitMQ.Client;

/// <summary>
/// Tests for the <see cref="RabbitMqSession"/> class.
/// </summary>
public class RabbitMqSessionTests
{
    [Fact]
    public void Ctor_NullFactory_ThrowsException()
    {
        // Arrange & Act
        var act = () => new RabbitMqSession(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithMessage("Value cannot be null. (Parameter 'factory')");
    }

    [Fact]
    public void Ctor_WithFactory_PopulatesChannel()
    {
        // Arrange & Act
        using var sut = GetSut(out var mockChannel, out _);

        // Assert
        sut.Channel.Should().Be(mockChannel.Object);
    }

    [Fact]
    public void Dispose_WhenCalled_ClosesChannelAndConnection()
    {
        // Arrange
        var sut = GetSut(out var mockChannel, out var mockConnection);

        // Act
        sut.Dispose();

        // Assert
        mockChannel.Verify(m => m.Close());
        mockConnection.Verify(m => m.Close());
    }

    private static RabbitMqSession GetSut(out Mock<IModel> mockChannel, out Mock<IConnection> mockConnection)
    {
        mockChannel = new Mock<IModel>();
        mockConnection = new Mock<IConnection>();
        var mockFactory = new Mock<IConnectionFactory>();
        mockConnection.Setup(m => m.CreateModel()).Returns(mockChannel.Object);
        mockFactory.Setup(m => m.CreateConnection()).Returns(mockConnection.Object);

        return new(mockFactory.Object);
    }
}

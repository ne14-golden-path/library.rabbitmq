// <copyright file="PermanentFailureExceptionTests.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace ne14.library.rabbitmq.tests.Exceptions;

using ne14.library.rabbitmq.Exceptions;

/// <summary>
/// Tests for the <see cref="PermanentFailureException"/> class.
/// </summary>
public class PermanentFailureExceptionTests
{
    [Fact]
    public void Ctor_Paramless_HasDefaultMessage()
    {
        // Arrange & Act
        var ex = new PermanentFailureException();

        // Assert
        ex.Message.Should().Contain(nameof(PermanentFailureException));
    }

    [Fact]
    public void Ctor_WithMessage_RetainsMessage()
    {
        // Arrange
        const string message = "woop";

        // Act
        var ex = new PermanentFailureException(message);

        // Assert
        ex.Message.Should().Be(message);
    }

    [Fact]
    public void Ctor_WithException_RetainsInnerException()
    {
        // Arrange
        var inner = new ArithmeticException();

        // Act
        var ex = new PermanentFailureException("oh noes", inner);

        // Assert
        ex.InnerException.Should().Be(inner);
    }
}

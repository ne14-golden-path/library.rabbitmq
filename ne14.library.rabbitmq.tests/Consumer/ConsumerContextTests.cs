// <copyright file="ConsumerContextTests.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace ne14.library.rabbitmq.tests.Consumer;

using ne14.library.rabbitmq.Consumer;

/// <summary>
/// Tests for the <see cref="ConsumerContext"/> class.
/// </summary>
public class ConsumerContextTests
{
    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "StyleCop.CSharp.ReadabilityRules",
        "SA1101:Prefix local calls with this",
        Justification = "Legitimate record syntax")]
    public void Ctor_ComparableReferences_PassEquality()
    {
        // Arrange
        var ref1 = new ConsumerContext { AttemptNumber = 1, MessageId = 1 };

        // Act
        var ref2 = ref1 with { MessageId = 2 };
        var ref3 = ref2 with { MessageId = 1 };

        // Assert
        ref1.Should().NotBe(ref2);
        ref1.Should().Be(ref3);
    }
}

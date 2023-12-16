// <copyright file="UnitTest1.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace ne14.library.rabbitmq.tests;

public class UnitTest1
{
    [Fact]
    public void Test1()
    {
        _ = new Mock<IMqConsumer<object>>();
        1.Should().Be(1);
    }
}
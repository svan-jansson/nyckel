using FluentAssertions;
using Nyckel.Core;
using Nyckel.Core.ValueObjects;
using System;
using Xunit;

namespace Nyckel.Tests;

public class InMemoryTests
{
    [Theory]
    [InlineData("Abc")]
    [InlineData(123)]
    [InlineData(false)]
    [InlineData(123.2)]
    public void Supports_basic_CRUD_operations(object input)
    {
        var sut = new InMemory();
        var updated = new { A = 1, B = 2 };
        var key = Key.Create("my-key");

        // Create
        sut
            .Set(key, Value.Create(input))
            .Filter(value => value.Get() == input)
            .IsSome()
            .Should()
            .BeTrue();

        // Read
        sut
           .Get(key)
           .Filter(value => value.Get() == input)
           .IsSome()
           .Should()
           .BeTrue();

        // Update
        sut
            .Set(key, Value.Create(updated))
            .Filter(value => value.Get() == updated)
            .IsSome()
            .Should()
            .BeTrue();

        sut
           .Get(key)
           .Filter(value => value.Get() == updated)
           .IsSome()
           .Should()
           .BeTrue();

        // Delete
        sut
           .Delete(key)
           .Filter(value => value.Get() == updated)
           .IsSome()
           .Should()
           .BeTrue();

        sut
           .Get(key)
           .IsNone()
           .Should()
           .BeTrue();
    }

    [Fact]
    public void Can_iterate_over_entries()
    {
        var sut = new InMemory();
        var expectedKey = Key.Create("my-key");
        var expectedValue = Value.Create(new { A = 1, B = 2 });
        var called = false;

        sut.Set(expectedKey, expectedValue);

        sut.Map((key, value) =>
        {
            called = true;
            key.Should().Be(expectedKey);
            value.Should().Be(expectedValue);

            return true;
        });

        called.Should().BeTrue();
        sut.Count().Should().Be(1);
    }
}
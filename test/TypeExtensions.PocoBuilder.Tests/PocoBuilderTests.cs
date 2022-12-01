using System.Reflection;
using FluentAssertions;
using Xunit;

namespace TypeExtensions.PocoBuilder.Tests;

public class TestClass
{
    public string Property { get; }

    public TestClass(string property)
    {
        Property = property;
    }
}

public sealed class SealedTestClass
{
    public string Property { get; }

    public SealedTestClass(string property)
    {
        Property = property;
    }
}

internal class InternalTestClass
{
    public string Property { get; }

    public InternalTestClass(string property)
    {
        Property = property;
    }
}

internal class InternalTestAttribute : Attribute
{
}

public class TestAttributeWithoutParameters : Attribute
{
}

public class TestAttributeWithParameters : Attribute
{
    public string Parameter { get; }

    public TestAttributeWithParameters(string parameter)
    {
        Parameter = parameter;
    }
}

public class PocoTypeBuilderTests
{
    private class PrivateTestClass
    {
        public string Property { get; }

        public PrivateTestClass(string property)
        {
            Property = property;
        }
    }

    private class PrivateTestAttribute : Attribute
    {
    }

    [Fact]
    public void Builds_poco_type_from_public_type()
    {
        //Arrange
        var pocoTypeBuilder = new PocoTypeBuilder<TestClass>();

        //Act
        var pocoType = pocoTypeBuilder
            .Property(type => type.Property)
            .Build();

        //Assert
        pocoType.Should().NotBeNull();

        pocoType.GetRuntimeProperty("Property")
            .Should()
            .NotBeNull();
    }

    [Fact]
    public void Builds_poco_type_from_sealed_type()
    {
        //Arrange
        var pocoTypeBuilder = new PocoTypeBuilder<SealedTestClass>();

        //Act
        var pocoType = pocoTypeBuilder
            .Property(type => type.Property)
            .Build();

        //Assert
        pocoType.Should().NotBeNull();

        pocoType.GetRuntimeProperty("Property")
            .Should()
            .NotBeNull();
    }

    [Fact]
    public void Builds_poco_type_from_internal_type()
    {
        //Arrange
        var pocoTypeBuilder = new PocoTypeBuilder<InternalTestClass>();

        //Act
        var pocoType = pocoTypeBuilder
            .Property(type => type.Property)
            .Build();

        //Assert
        pocoType.Should().NotBeNull();

        pocoType.GetRuntimeProperty("Property")
            .Should()
            .NotBeNull();
    }

    [Fact]
    public void Builds_poco_type_from_private_type()
    {
        //Arrange
        var pocoTypeBuilder = new PocoTypeBuilder<PrivateTestClass>();

        //Act
        var pocoType = pocoTypeBuilder
            .Property(type => type.Property)
            .Build();

        //Assert
        pocoType.Should().NotBeNull();

        pocoType.GetRuntimeProperty("Property")
            .Should()
            .NotBeNull();
    }

    [Fact]
    public void Builds_poco_type_with_custom_name()
    {
        //Arrange
        const string pocoTypeCustomName = "CustomName";
        var pocoTypeBuilder = new PocoTypeBuilder<TestClass>(pocoTypeCustomName);

        //Act
        var pocoType = pocoTypeBuilder
            .Build();

        //Assert
        pocoType.Should().NotBeNull();
        pocoType.Name.Should().Be(pocoTypeCustomName);
    }

    [Fact]
    public void Adds_attribute_to_poco_type()
    {
        //Arrange
        var pocoTypeBuilder = new PocoTypeBuilder<TestClass>();

        //Act
        var pocoType = pocoTypeBuilder
            .AddAttribute<TestAttributeWithoutParameters>()
            .Build();

        //Assert
        Attribute.GetCustomAttribute(
                pocoType,
                typeof(TestAttributeWithoutParameters))
            .Should()
            .NotBeNull();
    }

    [Fact]
    public void Adds_attribute_to_poco_type_property()
    {
        //Arrange
        var pocoTypeBuilder = new PocoTypeBuilder<TestClass>();

        //Act
        var pocoType = pocoTypeBuilder
            .Property(
                instance => instance.Property,
                propertyBuilder => propertyBuilder.AddAttribute<TestAttributeWithoutParameters>())
            .Build();

        //Assert
        Attribute.GetCustomAttribute(
                pocoType.GetRuntimeProperty("Property")!,
                typeof(TestAttributeWithoutParameters))
            .Should()
            .NotBeNull();
    }

    [Fact]
    public void Adds_attribute_with_parameters_to_poco_type()
    {
        //Arrange
        var pocoTypeBuilder = new PocoTypeBuilder<TestClass>();

        //Act
        var pocoType = pocoTypeBuilder
            .AddAttribute<TestAttributeWithParameters>(
                new object[]
                {
                    "value"
                })
            .Build();

        //Assert
        Attribute.GetCustomAttribute(
                pocoType,
                typeof(TestAttributeWithParameters))
            .Should()
            .NotBeNull();
    }

    [Fact]
    public void Adds_attribute_with_parameters_to_poco_type_property()
    {
        //Arrange
        var pocoTypeBuilder = new PocoTypeBuilder<TestClass>();

        //Act
        var pocoType = pocoTypeBuilder
            .Property(
                instance => instance.Property,
                propertyBuilder => propertyBuilder.AddAttribute<TestAttributeWithParameters>(
                    new object[]
                    {
                        "value"
                    }))
            .Build();

        //Assert
        Attribute.GetCustomAttribute(
                pocoType.GetRuntimeProperty("Property")!,
                typeof(TestAttributeWithParameters))
            .Should()
            .NotBeNull();
    }

    [Fact]
    public void Does_not_add_attribute_without_parameters_to_poco_type_when_parameters_are_set()
    {
        //Arrange
        var pocoTypeBuilder = new PocoTypeBuilder<TestClass>();

        //Act
        var addingAttributeAction = () => pocoTypeBuilder
            .AddAttribute<TestAttributeWithoutParameters>(
                new object[]
                {
                    "value"
                });

        //Assert
        addingAttributeAction.Should()
            .Throw<ArgumentException>()
            .WithMessage(
                "Attribute of type: TestAttributeWithoutParameters "
                + "has no constructor with parameters of "
                + "types: String. (Parameter 'attributeCtorParams')");
    }

    [Fact]
    public void Does_not_add_attribute_without_parameters_to_poco_type_property_when_parameters_are_set()
    {
        //Arrange
        var pocoTypeBuilder = new PocoTypeBuilder<TestClass>();

        //Act
        var addingAttributeAction = () => pocoTypeBuilder
            .Property(
                instance => instance.Property,
                propertyBuilder => propertyBuilder.AddAttribute<TestAttributeWithoutParameters>(
                    new object[]
                    {
                        "value"
                    }));

        //Assert
        addingAttributeAction.Should()
            .Throw<ArgumentException>()
            .WithMessage(
                "Attribute of type: TestAttributeWithoutParameters "
                + "has no constructor with parameters of "
                + "types: String. (Parameter 'attributeCtorParams')");
    }

    [Fact]
    public void Does_not_add_attribute_to_poco_type_when_attribute_is_internal()
    {
        //Arrange
        var pocoTypeBuilder = new PocoTypeBuilder<TestClass>();

        //Act
        var addingAttributeAction = () => pocoTypeBuilder
            .AddAttribute<InternalTestAttribute>();

        //Assert
        addingAttributeAction.Should()
            .Throw<ArgumentException>()
            .WithMessage(
                "Attribute of type InternalTestAttribute is not public. "
                + "Cannot use it to extend type property. (Parameter 'TAttribute')");
    }

    [Fact]
    public void Does_not_add_attribute_to_poco_type_when_attribute_is_private()
    {
        //Arrange
        var pocoTypeBuilder = new PocoTypeBuilder<TestClass>();

        //Act
        var addingAttributeAction = () => pocoTypeBuilder
            .AddAttribute<PrivateTestAttribute>();

        //Assert
        addingAttributeAction.Should()
            .Throw<ArgumentException>()
            .WithMessage(
                "Attribute of type PrivateTestAttribute is not public. "
                + "Cannot use it to extend type property. (Parameter 'TAttribute')");
    }
}
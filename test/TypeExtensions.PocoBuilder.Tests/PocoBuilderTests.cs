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

    public string PropertyParameter { get; set; } = null!;

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
    public void Builds_poco_type()
    {
        //Arrange
        var pocoTypeBuilder = new PocoTypeBuilder("PocoType");

        //Act
        var pocoType = pocoTypeBuilder
            .Property("Property", typeof(string))
            .Build();

        //Assert
        pocoType.Should().NotBeNull();
        pocoType.Name.Should().Be("PocoType");

        var property = pocoType.GetRuntimeProperty("Property");

        property
            .Should()
            .NotBeNull();

        property!.Name.Should().Be("Property");
        property.PropertyType.Should().Be(typeof(string));
    }
    
    [Fact]
    public void Builds_poco_type_with_inheritance()
    {
        //Arrange
        var pocoTypeBuilder = new PocoTypeBuilder("PocoType", baseTypeToInheritFrom: typeof(TestClass));

        //Act
        var pocoType = pocoTypeBuilder
            .Build();

        //Assert
        pocoType.Should().NotBeNull();
        pocoType.IsSubclassOf(typeof(TestClass)).Should().BeTrue();

        var property = pocoType.GetRuntimeProperty("Property");

        property
            .Should()
            .NotBeNull();

        property!.Name.Should().Be("Property");
        property.PropertyType.Should().Be(typeof(string));
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
        pocoType.Name.Should().Be("TestClass");

        var property = pocoType.GetRuntimeProperty("Property");

        property
            .Should()
            .NotBeNull();

        property!.Name.Should().Be("Property");
        property.PropertyType.Should().Be(typeof(string));
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
        pocoType.Name.Should().Be("SealedTestClass");

        var property = pocoType.GetRuntimeProperty("Property");

        property
            .Should()
            .NotBeNull();

        property!.Name.Should().Be("Property");
        property.PropertyType.Should().Be(typeof(string));
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
        pocoType.Name.Should().Be("InternalTestClass");

        var property = pocoType.GetRuntimeProperty("Property");

        property
            .Should()
            .NotBeNull();

        property!.Name.Should().Be("Property");
        property.PropertyType.Should().Be(typeof(string));
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
        pocoType.Name.Should().Be("PrivateTestClass");

        var property = pocoType.GetRuntimeProperty("Property");

        property
            .Should()
            .NotBeNull();

        property!.Name.Should().Be("Property");
        property.PropertyType.Should().Be(typeof(string));
    }

    [Fact]
    public void Builds_poco_type_with_custom_name()
    {
        //Arrange
        const string pocoTypeCustomName = "TypeName";
        var pocoTypeBuilder = new PocoTypeBuilder(pocoTypeCustomName);

        //Act
        var pocoType = pocoTypeBuilder
            .Build();

        //Assert
        pocoType.Should().NotBeNull();
        pocoType.Name.Should().Be(pocoTypeCustomName);
    }

    [Fact]
    public void Builds_poco_type_custom_assembly_name()
    {
        //Arrange
        const string assemblyCustomName = "AssemblyName";
        var pocoTypeBuilder = new PocoTypeBuilder("TypeName", pocoTypeAssemblyName: assemblyCustomName);

        //Act
        var pocoType = pocoTypeBuilder
            .Build();

        //Assert
        pocoType.Should().NotBeNull();
        pocoType.Assembly.GetName().Name.Should().Be(assemblyCustomName);
    }

    [Fact]
    public void Adds_attribute_to_poco_type()
    {
        //Arrange
        var pocoTypeBuilder = new PocoTypeBuilder("PocoType");

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
    public void Adds_attribute_to_poco_type_typed_property()
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
    public void Adds_attribute_to_poco_type_property()
    {
        //Arrange
        var pocoTypeBuilder = new PocoTypeBuilder("PocoType");

        //Act
        var pocoType = pocoTypeBuilder
            .Property(
                "Property",
                typeof(string),
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
        var pocoTypeBuilder = new PocoTypeBuilder("PocoType");

        //Act
        var pocoType = pocoTypeBuilder
            .AddAttribute<TestAttributeWithParameters>(
                new object[]
                {
                    "value1"
                },
                new Dictionary<string, object>
                {
                    {
                        "PropertyParameter", "value2"
                    }
                })
            .Build();

        //Assert
        var attribute = Attribute.GetCustomAttribute(
            pocoType,
            typeof(TestAttributeWithParameters)) as TestAttributeWithParameters;

        attribute
            .Should()
            .NotBeNull();

        attribute!.Parameter.Should().Be("value1");
        attribute.PropertyParameter.Should().Be("value2");
    }

    [Fact]
    public void Adds_attribute_with_parameters_to_poco_type_typed_property()
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
                        "value1"
                    },
                    new Dictionary<string, object>
                    {
                        {
                            "PropertyParameter", "value2"
                        }
                    }))
            .Build();

        //Assert
        var attribute = Attribute.GetCustomAttribute(
            pocoType.GetRuntimeProperty("Property")!,
            typeof(TestAttributeWithParameters)) as TestAttributeWithParameters;

        attribute
            .Should()
            .NotBeNull();

        attribute!.Parameter.Should().Be("value1");
        attribute.PropertyParameter.Should().Be("value2");
    }

    [Fact]
    public void Adds_attribute_with_parameters_to_poco_type_property()
    {
        //Arrange
        var pocoTypeBuilder = new PocoTypeBuilder("PocoType");

        //Act
        var pocoType = pocoTypeBuilder
            .Property(
                "Property",
                typeof(string),
                propertyBuilder => propertyBuilder.AddAttribute<TestAttributeWithParameters>(
                    new object[]
                    {
                        "value1"
                    },
                    new Dictionary<string, object>
                    {
                        {
                            "PropertyParameter", "value2"
                        }
                    }))
            .Build();

        //Assert
        var attribute = Attribute.GetCustomAttribute(
            pocoType.GetRuntimeProperty("Property")!,
            typeof(TestAttributeWithParameters)) as TestAttributeWithParameters;

        attribute
            .Should()
            .NotBeNull();

        attribute!.Parameter.Should().Be("value1");
        attribute.PropertyParameter.Should().Be("value2");
    }

    [Fact]
    public void Does_not_add_attribute_without_parameters_to_poco_type_when_parameters_are_set()
    {
        //Arrange
        var pocoTypeBuilder = new PocoTypeBuilder("PocoType");

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
    public void Does_not_add_attribute_with_parameters_to_poco_type_when_parameters_are_not_set()
    {
        //Arrange
        var pocoTypeBuilder = new PocoTypeBuilder("PocoType");

        //Act
        var addingAttributeAction = () => pocoTypeBuilder
            .AddAttribute<TestAttributeWithParameters>();

        //Assert
        addingAttributeAction.Should()
            .Throw<ArgumentException>()
            .WithMessage(
                "Attribute of type: TestAttributeWithParameters "
                + "has no constructor without parameters. (Parameter 'attributeCtorParams')");
    }

    [Fact]
    public void Does_not_add_attribute_with_parameters_to_poco_type_when_property_parameter_does_not_exist()
    {
        //Arrange
        var pocoTypeBuilder = new PocoTypeBuilder("PocoType");

        //Act
        var addingAttributeAction = () => pocoTypeBuilder
            .AddAttribute<TestAttributeWithParameters>(
                new object[]
                {
                    "value"
                },
                new Dictionary<string, object>
                {
                    {
                        "Test", "value2"
                    }
                });

        //Assert
        addingAttributeAction.Should()
            .Throw<ArgumentException>()
            .WithMessage(
                "Attribute of type: TestAttributeWithParameters has no properties "
                + "with names: Test. (Parameter 'attributePropertiesValues')");
    }

    [Fact]
    public void Does_not_add_attribute_with_parameters_to_poco_type_when_property_parameter_value_has_invalid_type()
    {
        //Arrange
        var pocoTypeBuilder = new PocoTypeBuilder("PocoType");

        //Act
        var addingAttributeAction = () => pocoTypeBuilder
            .AddAttribute<TestAttributeWithParameters>(
                new object[]
                {
                    "value"
                },
                new Dictionary<string, object>
                {
                    {
                        "PropertyParameter", 1
                    }
                });

        //Assert
        addingAttributeAction.Should()
            .Throw<ArgumentException>()
            .WithMessage("Constant does not match the defined type.");
    }

    [Fact]
    public void Does_not_add_attribute_to_poco_type_when_attribute_is_internal()
    {
        //Arrange
        var pocoTypeBuilder = new PocoTypeBuilder("PocoType");

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
        var pocoTypeBuilder = new PocoTypeBuilder("PocoType");

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
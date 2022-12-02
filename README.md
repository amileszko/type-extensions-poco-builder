# POCO Builder

[![CI](https://github.com/amileszko/type-extensions-poco-builder/actions/workflows/release.yml/badge.svg)](https://github.com/amileszko/type-extensions-poco-builder/actions/workflows/release.yml)
[![NuGet](http://img.shields.io/nuget/vpre/TypeExtensions.PocoBuilder.svg?label=NuGet)](https://www.nuget.org/packages/TypeExtensions.PocoBuilder)
\
[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=amileszko_type-extensions-poco-builder&metric=alert_status)](https://sonarcloud.io/summary/new_code?id=amileszko_type-extensions-poco-builder)
[![Coverage](https://sonarcloud.io/api/project_badges/measure?project=amileszko_type-extensions-poco-builder&metric=coverage)](https://sonarcloud.io/summary/new_code?id=amileszko_type-extensions-poco-builder)
[![Security Rating](https://sonarcloud.io/api/project_badges/measure?project=amileszko_type-extensions-poco-builder&metric=security_rating)](https://sonarcloud.io/summary/new_code?id=amileszko_type-extensions-poco-builder)
[![Maintainability Rating](https://sonarcloud.io/api/project_badges/measure?project=amileszko_type-extensions-poco-builder&metric=sqale_rating)](https://sonarcloud.io/summary/new_code?id=amileszko_type-extensions-poco-builder)
[![Reliability Rating](https://sonarcloud.io/api/project_badges/measure?project=amileszko_type-extensions-poco-builder&metric=reliability_rating)](https://sonarcloud.io/summary/new_code?id=amileszko_type-extensions-poco-builder)

### What is POCO Builder?

POCO builder is a light-weight library that creates POCO (Plain Old CLR Object) type at runtime from any compile time definied type. 
All properies defined in POCO type has automatically created getters and setters and can be extended with attributes just like the type itself.

### How do I get started?

Just install NuGet package and use PocoTypeBuilder generic class to create new POCO type. For example:

```csharp
//Define test class to create POCO type from
public class TestClass
{
    public string Property { get; }

    public TestClass(string property)
    {
        Property = property;
    }
}

//Define test attribute to extend POCO type and its property
public class TestAttribute : Attribute
{
}

//Create POCO type builder for test class
var pocoTypeBuilder = new PocoTypeBuilder<TestClass>();

//Extend build POCO type and its property with attribute
pocoTypeBuilder
    .AddAttribute<TestAttribute>()
    .Property(type => type.Property,
        propertyBuilder => propertyBuilder.AddAttribute<TestAttribute>());
        
//Build POCO type
var pocoType = pocoTypeBuilder.Build();

//Access POCO type property through reflection.
var propertyInfo = pocoType.GetRuntimeProperty("Property");
```

You can check out more examples in test project.

### Contribution

Welcome to join in and feel free to contribute by creating an Issue or Pull Request.

### License

The project is under [MIT license](https://opensource.org/licenses/MIT).

using System.Reflection.Emit;
using TypeExtensions.PocoBuilder.Utils;

namespace TypeExtensions.PocoBuilder;

public sealed class PocoTypePropertyBuilder
{
    private readonly PropertyBuilder propertyBuilder;

    internal PocoTypePropertyBuilder(PropertyBuilder propertyBuilder)
    {
        this.propertyBuilder = propertyBuilder;
    }

    public PocoTypePropertyBuilder AddAttribute<TAttribute>(
        object[]? attributeCtorParams = null,
        Dictionary<string, object>? attributePropertiesValues = null)
        where TAttribute : Attribute
    {
        propertyBuilder.SetCustomAttribute(
            AttributeUtils.CreateCustomAttributeBuilder<TAttribute>(attributeCtorParams, attributePropertiesValues));

        return this;
    }
}
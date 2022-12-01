using System.Reflection;
using System.Reflection.Emit;

namespace TypeExtensions.PocoBuilder.Utils;

internal static class AttributeUtils
{
    public static CustomAttributeBuilder CreateCustomAttributeBuilder<TAttribute>(object[]? attributeCtorParams = null)
        where TAttribute : Attribute
    {
        attributeCtorParams ??= Array.Empty<object>();

        var attributeCtorParamsTypes = attributeCtorParams
            .Select(ctorParam => ctorParam.GetType())
            .ToArray();

        var attributeType = typeof(TAttribute);

        if (!attributeType.Attributes.HasFlag(TypeAttributes.Public)
            || attributeType.Attributes.HasFlag(TypeAttributes.NestedPrivate))
        {
            throw new ArgumentException(
                $"Attribute of type {attributeType.Name} is not public. Cannot use it to extend type property.",
                nameof(TAttribute));
        }

        var ctorInfo = attributeType.GetConstructor(attributeCtorParamsTypes);

        if (ctorInfo == null)
        {
            throw new ArgumentException(
                $"Attribute of type: {attributeType.Name} has no constructor"
                + $" with parameters of types: {string.Join(", ", attributeCtorParamsTypes.Select(type => type.Name))}.",
                nameof(attributeCtorParams));
        }

        return new CustomAttributeBuilder(ctorInfo, attributeCtorParams);
    }
}
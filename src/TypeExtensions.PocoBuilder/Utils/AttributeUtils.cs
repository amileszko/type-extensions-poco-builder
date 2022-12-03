using System.Reflection;
using System.Reflection.Emit;

namespace TypeExtensions.PocoBuilder.Utils;

internal static class AttributeUtils
{
    public static CustomAttributeBuilder CreateCustomAttributeBuilder<TAttribute>(
        object[]? attributeCtorParams = null,
        Dictionary<string, object>? attributePropertiesValues = null)
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
            if (attributeCtorParamsTypes.Any())
            {
                throw new ArgumentException(
                    $"Attribute of type: {attributeType.Name} has no constructor"
                    + $" with parameters of types: {string.Join(", ", attributeCtorParamsTypes.Select(type => type.Name))}.",
                    nameof(attributeCtorParams));
            }

            throw new ArgumentException(
                $"Attribute of type: {attributeType.Name} has no constructor without parameters.",
                nameof(attributeCtorParams));
        }

        if (attributePropertiesValues == null)
        {
            return new CustomAttributeBuilder(ctorInfo, attributeCtorParams);
        }

        var attributePropertiesToSet = attributePropertiesValues
            .Select(
                propertyValue => new
                {
                    PropertyInfo = attributeType.GetProperty(propertyValue.Key),
                    PropertyName = propertyValue.Key,
                    PropertyValue = propertyValue.Value
                })
            .ToList();

        var missingProperties = attributePropertiesToSet
            .Where(property => property.PropertyInfo == null)
            .ToList();

        if (missingProperties.Any())
        {
            throw new ArgumentException(
                $"Attribute of type: {attributeType.Name} has no properties with names: "
                + $"{string.Join(", ", missingProperties.Select(property => property.PropertyName))}.",
                nameof(attributePropertiesValues));
        }

        return new CustomAttributeBuilder(
            ctorInfo,
            attributeCtorParams,
            attributePropertiesToSet.Select(property => property.PropertyInfo!).ToArray(),
            attributePropertiesToSet.Select(property => property.PropertyValue).ToArray());
    }
}
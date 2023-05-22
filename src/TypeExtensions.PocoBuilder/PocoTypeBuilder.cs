using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using TypeExtensions.PocoBuilder.Utils;

namespace TypeExtensions.PocoBuilder;

public class PocoTypeBuilder
{
    protected readonly TypeBuilder typeBuilder;

    private const MethodAttributes getterSetterAttributes =
        MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig;

    public PocoTypeBuilder(string pocoTypeName, string? pocoTypeAssemblyName = null, Type? baseTypeToInheritFrom = null)
    {
        typeBuilder = TypeBuilderCreator.CreateTypeBuilder(pocoTypeName, pocoTypeAssemblyName, baseTypeToInheritFrom);
    }

    public PocoTypeBuilder AddAttribute<TAttribute>(
        object[]? attributeCtorParams = null,
        Dictionary<string, object>? attributePropertiesValues = null)
        where TAttribute : Attribute
    {
        typeBuilder.SetCustomAttribute(
            AttributeUtils.CreateCustomAttributeBuilder<TAttribute>(attributeCtorParams, attributePropertiesValues));

        return this;
    }

    public PocoTypeBuilder Property(
        string propertyName,
        Type propertyType,
        Func<PocoTypePropertyBuilder, PocoTypePropertyBuilder>? pocoPropertyBuilder = null)
    {
        var propertyBuilder = DefinePropertyBuilder(propertyName, propertyType);

        pocoPropertyBuilder?.Invoke(new PocoTypePropertyBuilder(propertyBuilder));

        return this;
    }

    protected PropertyBuilder DefinePropertyBuilder(string propertyName, Type propertyType)
    {
        var field = typeBuilder.DefineField(
            $"_{propertyName.ToCamelCase()}",
            propertyType,
            FieldAttributes.Private);

        var property = typeBuilder.DefineProperty(
            propertyName,
            PropertyAttributes.HasDefault,
            propertyType,
            null);

        GenerateGetter(field, property);
        GenerateSetter(field, property);

        return property;
    }

    private void GenerateGetter(FieldInfo field, PropertyBuilder propertyBuilder)
    {
        var getMethodBuilder = typeBuilder.DefineMethod(
            $"get_{propertyBuilder.Name}",
            getterSetterAttributes,
            propertyBuilder.PropertyType,
            null);

        var getMethodGenerator = getMethodBuilder.GetILGenerator();

        getMethodGenerator.Emit(OpCodes.Ldarg_0);
        getMethodGenerator.Emit(OpCodes.Ldfld, field);
        getMethodGenerator.Emit(OpCodes.Ret);

        propertyBuilder.SetGetMethod(getMethodBuilder);
    }

    private void GenerateSetter(FieldInfo field, PropertyBuilder propertyBuilder)
    {
        var setMethodBuilder =
            typeBuilder.DefineMethod(
                $"set_{propertyBuilder.Name}",
                getterSetterAttributes,
                typeof(void),
                new[]
                {
                    propertyBuilder.PropertyType
                });

        var setMethodGenerator = setMethodBuilder.GetILGenerator();

        setMethodGenerator.Emit(OpCodes.Ldarg_0);
        setMethodGenerator.Emit(OpCodes.Ldarg_1);
        setMethodGenerator.Emit(OpCodes.Stfld, field);
        setMethodGenerator.Emit(OpCodes.Ret);

        propertyBuilder.SetSetMethod(setMethodBuilder);
    }

    public Type Build() => typeBuilder.CreateTypeInfo()!.AsType();
}

public sealed class PocoTypeBuilder<TBaseType> : PocoTypeBuilder
{
    private readonly Type baseType;

    public PocoTypeBuilder(
        string? pocoTypeName = null,
        string? pocoTypeAssemblyName = null,
        bool inheritFromBaseType = false) : base(
        pocoTypeName ?? typeof(TBaseType).Name,
        pocoTypeAssemblyName,
        inheritFromBaseType ? typeof(TBaseType) : null)
    {
        baseType = typeof(TBaseType);
    }

    public PocoTypeBuilder<TBaseType> Property<TBaseTypeProperty>(
        Expression<Func<TBaseType, TBaseTypeProperty>> propertySelector,
        Func<PocoTypePropertyBuilder, PocoTypePropertyBuilder>? pocoPropertyBuilder = null)
    {
        var propertyName = propertySelector.GetPropertyName();

        var propertyBuilder = baseType.GetRuntimeProperties()
            .Where(property => property.Name == propertyName)
            .Select(property => DefinePropertyBuilder(property.Name, property.PropertyType))
            .SingleOrDefault();

        if (propertyBuilder == null)
        {
            throw new ArgumentException(
                $"Type {baseType.Name} has no property with name: {propertyName}.",
                nameof(propertySelector));
        }

        pocoPropertyBuilder?.Invoke(new PocoTypePropertyBuilder(propertyBuilder));

        return this;
    }
}
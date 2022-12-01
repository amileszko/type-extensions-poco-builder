using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using TypeExtensions.PocoBuilder.Utils;

namespace TypeExtensions.PocoBuilder;

public sealed class PocoTypeBuilder<TBaseType>
{
    private readonly Type baseType;
    private readonly TypeBuilder typeBuilder;

    private const MethodAttributes getterSetterAttributes =
        MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig;

    public PocoTypeBuilder(string? pocoTypeName = null)
    {
        baseType = typeof(TBaseType);
        typeBuilder = CreateTypeBuilder(pocoTypeName);
    }

    public PocoTypeBuilder<TBaseType> AddAttribute<TAttribute>(object[]? attributeCtorParams = null)
        where TAttribute : Attribute
    {
        typeBuilder.SetCustomAttribute(AttributeUtils.CreateCustomAttributeBuilder<TAttribute>(attributeCtorParams));

        return this;
    }

    public PocoTypeBuilder<TBaseType> Property<TBaseTypeProperty>(
        Expression<Func<TBaseType, TBaseTypeProperty>> propertySelector,
        Func<PocoTypePropertyBuilder, PocoTypePropertyBuilder>? pocoPropertyBuilder = null)
    {
        var propertyName = propertySelector.GetPropertyName();

        var propertyBuilder = baseType.GetRuntimeProperties()
            .Where(property => property.Name == propertyName)
            .Select(DefinePropertyBuilder)
            .Single();

        if (propertyBuilder == null)
        {
            throw new ArgumentException(
                $"Type {baseType.Name} " + $"has no property with name: {propertyName}.",
                nameof(propertySelector));
        }

        pocoPropertyBuilder?.Invoke(new PocoTypePropertyBuilder(propertyBuilder));

        return this;
    }

    private TypeBuilder CreateTypeBuilder(string? pocoTypeName)
    {
        var assemblyName = new AssemblyName("DynamicAssembly");
        var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
        var moduleBuilder = assemblyBuilder.DefineDynamicModule(assemblyName.Name!);

        return moduleBuilder.DefineType(pocoTypeName ?? baseType.Name, TypeAttributes.Public);
    }

    private PropertyBuilder DefinePropertyBuilder(PropertyInfo propertyInfo)
    {
        var field = typeBuilder.DefineField(
            $"_{propertyInfo.Name.ToCamelCase()}",
            propertyInfo.PropertyType,
            FieldAttributes.Private);

        var property = typeBuilder.DefineProperty(
            propertyInfo.Name,
            PropertyAttributes.HasDefault,
            propertyInfo.PropertyType,
            null);

        GenerateGetter(field, property);
        GenerateSetter(field, property);

        return property;
    }

    private void GenerateGetter(FieldBuilder field, PropertyBuilder propertyBuilder)
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

    private void GenerateSetter(FieldBuilder field, PropertyBuilder propertyBuilder)
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
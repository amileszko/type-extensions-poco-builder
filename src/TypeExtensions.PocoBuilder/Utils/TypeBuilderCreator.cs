using System.Reflection;
using System.Reflection.Emit;

namespace TypeExtensions.PocoBuilder.Utils;

internal static class TypeBuilderCreator
{
    public static TypeBuilder CreateTypeBuilder(
        string pocoTypeName,
        string? pocoTypeAssemblyName = null,
        Type? baseTypeToInheritFrom = null)
    {
        var assemblyName = new AssemblyName(pocoTypeAssemblyName ?? "DynamicAssembly");
        var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
        var moduleBuilder = assemblyBuilder.DefineDynamicModule(assemblyName.Name!);

        var typeBuilder = moduleBuilder.DefineType(pocoTypeName, TypeAttributes.Public);

        if (baseTypeToInheritFrom != null)
        {
            typeBuilder.SetParent(baseTypeToInheritFrom);

            var baseConstructors = baseTypeToInheritFrom.GetConstructors();

            foreach (var baseConstructor in baseConstructors)
            {
                var parameters = baseConstructor.GetParameters();
                var parameterTypes = parameters.Select(parameterInfo => parameterInfo.ParameterType).ToArray();

                var constructorBuilder = typeBuilder.DefineConstructor(
                    MethodAttributes.Public,
                    CallingConventions.Standard,
                    parameterTypes);

                for (var parameterNumber = 0; parameterNumber < parameters.Length; parameterNumber++)
                {
                    constructorBuilder.DefineParameter(
                        parameterNumber + 1,
                        parameters[parameterNumber].Attributes,
                        parameters[parameterNumber].Name);
                }

                var ilGenerator = constructorBuilder.GetILGenerator();

                ilGenerator.Emit(OpCodes.Ldarg_0);

                for (var parameterNumber = 0; parameterNumber < parameters.Length; parameterNumber++)
                {
                    ilGenerator.Emit(OpCodes.Ldarg, parameterNumber + 1);
                }

                ilGenerator.Emit(OpCodes.Call, baseConstructor);
                ilGenerator.Emit(OpCodes.Ret);
            }
        }

        return typeBuilder;
    }
}
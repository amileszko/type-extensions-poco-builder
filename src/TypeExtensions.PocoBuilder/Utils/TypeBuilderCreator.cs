using System.Reflection;
using System.Reflection.Emit;

namespace TypeExtensions.PocoBuilder.Utils;

internal static class TypeBuilderCreator
{
    public static TypeBuilder CreateTypeBuilder(string pocoTypeName)
    {
        var assemblyName = new AssemblyName("DynamicAssembly");
        var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
        var moduleBuilder = assemblyBuilder.DefineDynamicModule(assemblyName.Name!);

        return moduleBuilder.DefineType(pocoTypeName, TypeAttributes.Public);
    }
}
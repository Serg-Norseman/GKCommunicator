// Original author: AKFish
// Original home: https://github.com/akfish/ProxyEmitter
// Original license: unknown
// Comment: short version for narrow purposes (for GK)

using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;

namespace GKCore.Proxy
{
    /// <summary>
    /// Helper class
    /// </summary>
    internal static class ProxyUtils
    {
        /// <summary>
        /// Gets the <see cref="AppDomain"/> of current thread
        /// </summary>
        public static AppDomain CurrentDomain
        {
            get { return Thread.GetDomain(); }
        }

        #region Global Methods

        public static Attribute GetCustomAttribute(MemberInfo element, Type attributeType, bool inherit)
        {
            Attribute[] customAttributes = Attribute.GetCustomAttributes(element, attributeType, inherit);
            if (customAttributes == null || customAttributes.Length == 0) {
                return null;
            }
            if (customAttributes.Length == 1) {
                return customAttributes[0];
            }
            throw new AmbiguousMatchException("");
        }

        public static T GetCustomAttribute<T>(MemberInfo element) where T : Attribute
        {
            return (T)(GetCustomAttribute(element, typeof(T), false));
        }

        /// <summary>
        /// Creates an <see cref="AssemblyBuilder"/> instance within <see cref="CurrentDomain"/> 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="access"></param>
        /// <returns></returns>
        public static AssemblyBuilder GetAssemblyBuilder(string name, AssemblyBuilderAccess access = AssemblyBuilderAccess.RunAndSave)
        {
            var aname = new AssemblyName(name);
            AppDomain currentDomain = AppDomain.CurrentDomain; 
            AssemblyBuilder builder = currentDomain.DefineDynamicAssembly(aname, access);
            return builder;
        }

        /// <summary>
        /// Creates a <see cref="ModuleBuilder"/> instance within <paramref name="asmBuilder"/>
        /// </summary>
        /// <param name="asmBuilder"></param>
        /// <param name="moduleName"></param>
        /// <returns></returns>
        public static ModuleBuilder GetModule(AssemblyBuilder asmBuilder, String moduleName)
        {
            ModuleBuilder builder = asmBuilder.DefineDynamicModule(moduleName, true);
            return builder;
        }

        /// <summary>
        /// Creates an <see cref="EnumBuilder"/> instance within <paramref name="modBuilder"/>
        /// </summary>
        /// <param name="modBuilder"></param>
        /// <param name="enumName"></param>
        /// <returns></returns>
        public static EnumBuilder GetEnum(ModuleBuilder modBuilder, string enumName)
        {
            EnumBuilder builder = modBuilder.DefineEnum(enumName, TypeAttributes.Public, typeof(Int32));
            return builder;
        }

        /// <summary>
        /// Create a <see cref="TypeBuilder"/> instance within <paramref name="modBuilder"/>
        /// </summary>
        /// <param name="modBuilder"></param>
        /// <param name="className"></param>
        /// <param name="parent"></param>
        /// <param name="interfaces"></param>
        /// <returns></returns>
        public static TypeBuilder GetType(ModuleBuilder modBuilder, string className, Type parent = null, Type[] interfaces = null)
        {
            if (parent == null)
                parent = typeof(Object);
            TypeBuilder builder = modBuilder.DefineType(className, TypeAttributes.Public, parent, interfaces);
            return builder;
        }

        public static TypeBuilder GetType(ModuleBuilder modBuilder, string className, params string[] genericparameters)
        {
            TypeBuilder builder = modBuilder.DefineType(className, TypeAttributes.Public);
            GenericTypeParameterBuilder[] genBuilders = builder.DefineGenericParameters(genericparameters);

            foreach (GenericTypeParameterBuilder genBuilder in genBuilders) { // We take each generic type T : class, new()
                genBuilder.SetGenericParameterAttributes(GenericParameterAttributes.ReferenceTypeConstraint | GenericParameterAttributes.DefaultConstructorConstraint);
                //genBuilder.SetInterfaceConstraints(interfaces);
            }

            return builder;
        }

        public static MethodBuilder GetMethod(TypeBuilder typeBuilder, string methodName, MethodAttributes attributes)
        {
            MethodBuilder builder = typeBuilder.DefineMethod(
                                        methodName,
                                        attributes);
            return builder;
        }

        public static MethodBuilder GetMethod(TypeBuilder typeBuilder, string methodName, MethodAttributes attributes, Type returnType, params Type[] parameterTypes)
        {
            MethodBuilder builder = typeBuilder.DefineMethod(
                                        methodName,
                                        attributes,
                                        CallingConventions.HasThis,
                                        returnType,
                                        parameterTypes);
            return builder;
        }

        public static ConstructorBuilder GetConstructor(TypeBuilder typeBuilder, MethodAttributes attributes, params Type[] parameterTypes)
        {
            return typeBuilder.DefineConstructor(attributes, CallingConventions.HasThis, parameterTypes);
        }

        public static MethodBuilder GetMethod(TypeBuilder typeBuilder, string methodName, MethodAttributes attributes, Type returnType, string[] genericParameters, params Type[] parameterTypes)
        {
            MethodBuilder builder = typeBuilder.DefineMethod(
                                        methodName,
                                        attributes,
                                        CallingConventions.HasThis,
                                        returnType, parameterTypes);

            GenericTypeParameterBuilder[] genBuilders = builder.DefineGenericParameters(genericParameters);

            foreach (GenericTypeParameterBuilder genBuilder in genBuilders) { // We take each generic type T : class, new()
                genBuilder.SetGenericParameterAttributes(GenericParameterAttributes.ReferenceTypeConstraint | GenericParameterAttributes.DefaultConstructorConstraint);
                //genBuilder.SetInterfaceConstraints(interfaces);
            }
            return builder;
        }

        #endregion
    }
}

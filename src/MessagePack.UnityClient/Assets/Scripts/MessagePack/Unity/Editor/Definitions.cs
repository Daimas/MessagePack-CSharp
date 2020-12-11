using System.Text.RegularExpressions;

namespace MessagePack.Unity
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public interface IResolverRegisterInfo
    {
        string FullName { get; }
        string FormatterName { get; }
    }

    public class ObjectSerializationInfo : IResolverRegisterInfo
    {
        public Type Type { get; set; }
        public string Name => (Type.IsGenericType ? Type.Name : GetMinimallyQualifiedClassName(Type)).Replace('+', '.');
        public string FullName => Type.GetRealFullName();
        public string Namespace { get; set; }

        public GenericTypeParameterInfo[] GenericTypeParameters { get; set; }

        public bool IsIntKey { get; set; }
        public bool IsClass { get; set; }
        public bool IsOpenGenericType { get; set; }

        public bool IsStringKey => !IsIntKey;
        public bool IsStruct => !IsClass;

        public MemberSerializationInfo[] ConstructorParameters { get; set; }
        public MemberSerializationInfo[] Members { get; set; }

        public bool HasIMessagePackSerializationCallbackReceiver { get; set; }
        public bool NeedsCastOnBefore { get; set; }
        public bool NeedsCastOnAfter { get; set; }

        public string FormatterName => (Namespace == null ? Name : Namespace + "." + Name) + $"Formatter" + (IsOpenGenericType ? $"<{string.Join(",", GenericTypeParameters.Select(x => x.Name))}>" : string.Empty);

        public int WriteCount => IsStringKey ? Members.Count(x => x.IsReadable) : MaxKey;

        public int MaxKey => Members.Where(x => x.IsReadable).Select(x => x.IntKey).DefaultIfEmpty(-1).Max();

        public MemberSerializationInfo GetMember(int index)
        {
            return Members.FirstOrDefault(x => x.IntKey == index);
        }

        public string GetConstructorString()
        {
            var args = string.Join(", ", ConstructorParameters.Select(x => "__" + x.Name + "__"));
            return $"{FullName}({args})";
        }

        private static string GetMinimallyQualifiedClassName(Type type)
        {
            var name = type.DeclaringType is object ? GetMinimallyQualifiedClassName(type.DeclaringType) + "_" : string.Empty;
            name += type.Name;
            name = name.Replace(".", "_");
            name = name.Replace("<", "_");
            name = name.Replace(">", "_");
            name = Regex.Replace(name, @"\[([,])*\]", match => $"Array{match.Length - 1}");
            return name;
        }
    }

    public class GenericTypeParameterInfo
    {
        public string Name { get; }
        public string Constraints { get; }
        public bool HasConstraints { get; }

        public GenericTypeParameterInfo(string name, string constraints)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Constraints = constraints ?? throw new ArgumentNullException(nameof(name));
            HasConstraints = !string.IsNullOrEmpty(constraints);
        }
    }

    public class MemberSerializationInfo
    {
        public bool IsProperty { get; set; }
        public bool IsField { get; set; }
        public bool IsWritable { get; set; }
        public bool IsReadable { get; set; }

        public int IntKey { get; set; }
        public string StringKey { get; set; }

        public Type Type { get; set; }
        public string Name { get; set; }
        public string FullTypeName => Type.GetRealFullName();
        public string ShortTypeName => Type.Name.Replace('+', '.');
        public string CustomFormatterTypeName { get; set; }

        private readonly HashSet<Type> primitiveTypes = new HashSet<Type>(ShouldUseFormatterResolverHelper.PrimitiveTypes);

        public string GetSerializeMethodString()
        {
            if (CustomFormatterTypeName != null)
            {
                return $"this.__{Name}CustomFormatter__.Serialize(ref writer, value.{Name}, options)";
            }
            else if (primitiveTypes.Contains(Type))
            {
                return $"writer.Write(value.{Name})";
            }
            else
            {
                return $"formatterResolver.GetFormatterWithVerify<{FullTypeName}>().Serialize(ref writer, value.{Name}, options)";
            }
        }

        public string GetDeserializeMethodString()
        {
            if (CustomFormatterTypeName != null)
            {
                return $"this.__{Name}CustomFormatter__.Deserialize(ref reader, options)";
            }
            else if (primitiveTypes.Contains(Type))
            {
                string suffix = Type == typeof(byte[]) ? "?.ToArray()" : string.Empty;
                return $"reader.Read{ShortTypeName.Replace("[]", "s")}()" + suffix;
            }
            else
            {
                return $"formatterResolver.GetFormatterWithVerify<{FullTypeName}>().Deserialize(ref reader, options)";
            }
        }
    }

    public class EnumSerializationInfo : IResolverRegisterInfo
    {
        public Type Type;
        public string Namespace { get; set; }
        public string Name => Type.Name.Replace(".", "_");
        public string FullName => Type.GetRealFullName();
        public string UnderlyingType { get; set; }

        public string FormatterName => (Namespace == null ? Name : Namespace + "." + Name) + "Formatter";
    }

    public class GenericSerializationInfo : IResolverRegisterInfo, IEquatable<GenericSerializationInfo>
    {
        public Type Type;
        public string FullName => Type.GetRealFullName();

        public string FormatterName { get; set; }

//        public string FormatterName => FormatterType.GetRealFullName();
        public bool IsOpenGenericType { get; set; }

        public bool Equals(GenericSerializationInfo other) => FullName.Equals(other.FullName);

        public override int GetHashCode() => FullName.GetHashCode();
    }

    public class UnionSerializationInfo : IResolverRegisterInfo
    {
        public string Namespace { get; set; }
        public string Name { get; set; }
        public string FullName { get; set; }

        public string FormatterName => (Namespace == null ? Name : Namespace + "." + Name) + "Formatter";

        public UnionSubTypeInfo[] SubTypes { get; set; }
    }

    public class UnionSubTypeInfo
    {
        public string Type { get; set; }
        public int Key { get; set; }
    }

    public static class ShouldUseFormatterResolverHelper
    {
        /// <devremarks>
        /// Keep this list in sync with DynamicObjectTypeBuilder.IsOptimizeTargetType.
        /// </devremarks>
        internal static readonly Type[] PrimitiveTypes =
        {
            typeof(short),
            typeof(int),
            typeof(long),
            typeof(ushort),
            typeof(uint),
            typeof(ulong),
            typeof(float),
            typeof(double),
            typeof(bool),
            typeof(byte),
            typeof(sbyte),
            typeof(char),
            typeof(byte[]),

            // Do not include types that resolvers are allowed to modify.
            ////"global::System.DateTime",  // OldSpec has no support, so for that and perf reasons a .NET native DateTime resolver exists.
            ////"string", // https://github.com/Cysharp/MasterMemory provides custom formatter for string interning.
        };

        public static bool ShouldUseFormatterResolver(MemberSerializationInfo[] infos)
        {
            foreach (var memberSerializationInfo in infos)
            {
                if (memberSerializationInfo.CustomFormatterTypeName == null && Array.IndexOf(PrimitiveTypes, memberSerializationInfo.Type) == -1)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
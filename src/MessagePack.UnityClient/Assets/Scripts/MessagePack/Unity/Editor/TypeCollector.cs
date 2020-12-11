using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using MessagePack.Formatters;
using MessagePack.Internal;

namespace MessagePack.Unity.Editor
{
    public class MessagePackGeneratorResolveFailedException : Exception
    {
        public MessagePackGeneratorResolveFailedException(string message)
            : base(message)
        {
        }
    }

    public class TypeCollector
    {
        private readonly HashSet<Type> embeddedTypes = new HashSet<Type>(new[]
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
            typeof(decimal),
            typeof(char),
            typeof(string),
            typeof(object),
            typeof(System.Guid),
            typeof(System.TimeSpan),
            typeof(System.DateTime),
            typeof(System.DateTimeOffset),

            typeof(MessagePack.Nil),

            // and arrays
            typeof(short[]),
            typeof(int[]),
            typeof(long[]),
            typeof(ushort[]),
            typeof(uint[]),
            typeof(ulong[]),
            typeof(float[]),
            typeof(double[]),
            typeof(bool[]),
            typeof(byte[]),
            typeof(sbyte[]),
            typeof(decimal[]),
            typeof(char[]),
            typeof(string[]),
            typeof(System.DateTime[]),
            typeof(System.ArraySegment<byte>),
            typeof(System.ArraySegment<byte>?),

            // extensions
            typeof(UnityEngine.Vector2),
            typeof(UnityEngine.Vector3),
            typeof(UnityEngine.Vector4),
            typeof(UnityEngine.Quaternion),
            typeof(UnityEngine.Color),
            typeof(UnityEngine.Bounds),
            typeof(UnityEngine.Rect),
            typeof(UnityEngine.AnimationCurve),
            typeof(UnityEngine.RectOffset),
            typeof(UnityEngine.Gradient),
            typeof(UnityEngine.WrapMode),
            typeof(UnityEngine.GradientMode),
            typeof(UnityEngine.Keyframe),
            typeof(UnityEngine.Matrix4x4),
            typeof(UnityEngine.GradientColorKey),
            typeof(UnityEngine.GradientAlphaKey),
            typeof(UnityEngine.Color32),
            typeof(UnityEngine.LayerMask),
            typeof(UnityEngine.Vector2Int),
            typeof(UnityEngine.Vector3Int),
            typeof(UnityEngine.RangeInt),
            typeof(UnityEngine.RectInt),
            typeof(UnityEngine.BoundsInt),
        });

        private readonly Dictionary<Type, Type> knownGenericTypes = new Dictionary<Type, Type>
        {
#pragma warning disable SA1509 // Opening braces should not be preceded by blank line
            { typeof(System.Collections.Generic.List<>), typeof(MessagePack.Formatters.ListFormatter<>) },
            { typeof(System.Collections.Generic.LinkedList<>), typeof(MessagePack.Formatters.LinkedListFormatter<>) },
            { typeof(System.Collections.Generic.Queue<>), typeof(MessagePack.Formatters.QueueFormatter<>) },
            { typeof(System.Collections.Generic.Stack<>), typeof(MessagePack.Formatters.StackFormatter<>) },
            { typeof(System.Collections.Generic.HashSet<>), typeof(MessagePack.Formatters.HashSetFormatter<>) },
            { typeof(System.Collections.ObjectModel.ReadOnlyCollection<>), typeof(MessagePack.Formatters.ReadOnlyCollectionFormatter<>) },
            { typeof(System.Collections.Generic.IList<>), typeof(MessagePack.Formatters.InterfaceListFormatter2<>) },
            { typeof(System.Collections.Generic.ICollection<>), typeof(MessagePack.Formatters.InterfaceCollectionFormatter2<>) },
            { typeof(System.Collections.Generic.IEnumerable<>), typeof(MessagePack.Formatters.InterfaceEnumerableFormatter<>) },
            { typeof(System.Collections.Generic.Dictionary<,>), typeof(MessagePack.Formatters.DictionaryFormatter<,>) },
            { typeof(System.Collections.Generic.IDictionary<,>), typeof(MessagePack.Formatters.InterfaceDictionaryFormatter<,>) },
            { typeof(System.Collections.Generic.SortedDictionary<,>), typeof(MessagePack.Formatters.SortedDictionaryFormatter<,>) },
            { typeof(System.Collections.Generic.SortedList<,>), typeof(MessagePack.Formatters.SortedListFormatter<,>) },
            { typeof(System.Linq.ILookup<,>), typeof(MessagePack.Formatters.InterfaceLookupFormatter<,>) },
            { typeof(System.Linq.IGrouping<,>), typeof(MessagePack.Formatters.InterfaceGroupingFormatter<,>) },
            { typeof(System.Collections.ObjectModel.ObservableCollection<>), typeof(MessagePack.Formatters.ObservableCollectionFormatter<>) },
            { typeof(System.Collections.ObjectModel.ReadOnlyObservableCollection<>), typeof(MessagePack.Formatters.ReadOnlyObservableCollectionFormatter<>) },
            { typeof(System.Collections.Generic.IReadOnlyList<>), typeof(MessagePack.Formatters.InterfaceReadOnlyListFormatter<>) },
            { typeof(System.Collections.Generic.IReadOnlyCollection<>), typeof(MessagePack.Formatters.InterfaceReadOnlyCollectionFormatter<>) },
            { typeof(System.Collections.Generic.ISet<>), typeof(MessagePack.Formatters.InterfaceSetFormatter<>) },
            { typeof(System.Collections.Concurrent.ConcurrentBag<>), typeof(MessagePack.Formatters.ConcurrentBagFormatter<>) },
            { typeof(System.Collections.Concurrent.ConcurrentQueue<>), typeof(MessagePack.Formatters.ConcurrentQueueFormatter<>) },
            { typeof(System.Collections.Concurrent.ConcurrentStack<>), typeof(MessagePack.Formatters.ConcurrentStackFormatter<>) },
            { typeof(System.Collections.ObjectModel.ReadOnlyDictionary<,>), typeof(MessagePack.Formatters.ReadOnlyDictionaryFormatter<,>) },
            { typeof(System.Collections.Generic.IReadOnlyDictionary<,>), typeof(MessagePack.Formatters.InterfaceReadOnlyDictionaryFormatter<,>) },
            { typeof(System.Collections.Concurrent.ConcurrentDictionary<,>), typeof(MessagePack.Formatters.ConcurrentDictionaryFormatter<,>) },
            { typeof(System.Lazy<>), typeof(MessagePack.Formatters.LazyFormatter<>) },
            //{ typeof(System.Threading.Tasks<>), typeof(MessagePack.Formatters.TaskValueFormatter<>) },

            { typeof(System.Tuple<>), typeof(MessagePack.Formatters.TupleFormatter<>) },
            { typeof(System.Tuple<,>), typeof(MessagePack.Formatters.TupleFormatter<>) },
            { typeof(System.Tuple<,,>), typeof(MessagePack.Formatters.TupleFormatter<>) },
            { typeof(System.Tuple<,,,>), typeof(MessagePack.Formatters.TupleFormatter<>) },
            { typeof(System.Tuple<,,,,>), typeof(MessagePack.Formatters.TupleFormatter<>) },
            { typeof(System.Tuple<,,,,,>), typeof(MessagePack.Formatters.TupleFormatter<>) },
            { typeof(System.Tuple<,,,,,,>), typeof(MessagePack.Formatters.TupleFormatter<>) },
            { typeof(System.Tuple<,,,,,,,>), typeof(MessagePack.Formatters.TupleFormatter<>) },

            { typeof(System.ValueTuple<>), typeof(MessagePack.Formatters.ValueTupleFormatter<>) },
            { typeof(System.ValueTuple<,>), typeof(MessagePack.Formatters.ValueTupleFormatter<>) },
            { typeof(System.ValueTuple<,,>), typeof(MessagePack.Formatters.ValueTupleFormatter<>) },
            { typeof(System.ValueTuple<,,,>), typeof(MessagePack.Formatters.ValueTupleFormatter<>) },
            { typeof(System.ValueTuple<,,,,>), typeof(MessagePack.Formatters.ValueTupleFormatter<>) },
            { typeof(System.ValueTuple<,,,,,>), typeof(MessagePack.Formatters.ValueTupleFormatter<>) },
            { typeof(System.ValueTuple<,,,,,,>), typeof(MessagePack.Formatters.ValueTupleFormatter<>) },
            { typeof(System.ValueTuple<,,,,,,,>), typeof(MessagePack.Formatters.ValueTupleFormatter<>) },

            { typeof(System.Collections.Generic.KeyValuePair<,>), typeof(MessagePack.Formatters.KeyValuePairFormatter<,>) },
            { typeof(System.Threading.Tasks.ValueTask<>), typeof(MessagePack.Formatters.KeyValuePairFormatter<,>) },
            { typeof(System.ArraySegment<>), typeof(MessagePack.Formatters.ArraySegmentFormatter<>) },
#pragma warning restore SA1509 // Opening braces should not be preceded by blank line
        };

        private readonly bool isForceUseMap;
        private readonly bool requireIgnoreAttributes;
        private HashSet<string> externalIgnoreTypeNames;

        private HashSet<Type> alreadyCollected = new HashSet<Type>();
        private List<ObjectSerializationInfo> collectedObjectInfo = new List<ObjectSerializationInfo>();
        private List<EnumSerializationInfo> collectedEnumInfo = new List<EnumSerializationInfo>();
        private List<GenericSerializationInfo> collectedGenericInfo = new List<GenericSerializationInfo>();
        private List<UnionSerializationInfo> collectedUnionInfo = new List<UnionSerializationInfo>();

        public TypeCollector(bool isForceUseMap, string[] ignoreTypeNames, bool requireIgnoreAttributes)
        {
            this.isForceUseMap = isForceUseMap;
            this.requireIgnoreAttributes = requireIgnoreAttributes;
            externalIgnoreTypeNames = new HashSet<string>(ignoreTypeNames ?? Array.Empty<string>());
        }

        // EntryPoint
        public (ObjectSerializationInfo[] ObjectInfo, EnumSerializationInfo[] EnumInfo, GenericSerializationInfo[] GenericInfo, UnionSerializationInfo[] UnionInfo) Collect()
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            var allTypes = assemblies.SelectMany(a => a.GetTypes()).ToList();
            var targetTypes = allTypes
                .Concat(allTypes.SelectMany(t => t.GetNestedTypes())) // TODO Do recursive search of nested types
                .Where(t => t.IsPublic || t.IsNestedPublic)
                .Where(t =>
                    (t.IsInterface && t.GetCustomAttribute<UnionAttribute>() != null)
                    || (t.IsClass && t.IsAbstract && t.GetCustomAttribute<UnionAttribute>() != null)
                    || (t.GetCustomAttribute<MessagePackObjectAttribute>() != null))
                .ToList();

            foreach (var type in targetTypes)
                CollectCore(type);

            return (
                collectedObjectInfo.OrderBy(x => x.FullName).ToArray(),
                collectedEnumInfo.OrderBy(x => x.FullName).ToArray(),
                collectedGenericInfo.Distinct().OrderBy(x => x.FullName).ToArray(),
                collectedUnionInfo.OrderBy(x => x.FullName).ToArray());
        }

        private void CollectCore(Type type)
        {
            if (!alreadyCollected.Add(type))
            {
                return;
            }

            if (embeddedTypes.Contains(type))
            {
                return;
            }

            if (externalIgnoreTypeNames.Contains(type.FullName))
            {
                return;
            }

            if (type.IsArray)
            {
                CollectArray(type);
                return;
            }

            if (!IsAllowAccessibility(type))
            {
                return;
            }

            var customFormatterAttr = type.GetCustomAttribute<MessagePackFormatterAttribute>();
            if (customFormatterAttr != null)
            {
                return;
            }

            if (type.IsEnum)
            {
                CollectEnum(type);
                return;
            }

            if (type.IsGenericType)
            {
                CollectGeneric(type);
                return;
            }

            // TODO
            //if (type.TupleUnderlyingType != null) {
            //    CollectGeneric(type.TupleUnderlyingType);
            //    return;
            //}

            if (type.IsInterface || (type.IsClass && type.IsAbstract))
            {
                CollectUnion(type);
                return;
            }

            CollectObject(type);
        }

        private void CollectEnum(Type type)
        {
            var info = new EnumSerializationInfo
            {
                Type = type,
                Namespace = type.Namespace,
                UnderlyingType = type.GetEnumUnderlyingType().Name,
            };

            collectedEnumInfo.Add(info);
        }

        private void CollectUnion(Type type)
        {
            var unionAttrs = type.GetCustomAttributes<UnionAttribute>().ToArray();
            if (unionAttrs.Length == 0)
            {
                throw new MessagePackGeneratorResolveFailedException("Serialization Type must mark UnionAttribute." + " type: " + type.FullName);
            }

            // 0, Int  1, SubType
            var info = new UnionSerializationInfo
            {
                Name = type.Name,
                Namespace = type.Namespace,
                FullName = type.FullName,
                SubTypes = unionAttrs.Select(x => new UnionSubTypeInfo
                {
                    Key = x.Key,
                    Type = x.SubType.FullName,
                }).OrderBy(x => x.Key).ToArray(),
            };

            collectedUnionInfo.Add(info);
        }

        private void CollectGenericUnion(Type type)
        {
            var unionAttrs = type.GetCustomAttributes<UnionAttribute>().ToArray();
            if (unionAttrs.Length == 0)
            {
                return;
            }

            var subTypes = unionAttrs.Select(x => x.SubType).ToArray();
            foreach (var unionType in subTypes)
            {
                if (alreadyCollected.Contains(unionType) == false)
                {
                    CollectCore(unionType);
                }
            }
        }

        private void CollectArray(Type array)
        {
            var elemType = array.GetElementType();
            CollectCore(elemType);

            var info = new GenericSerializationInfo
            {
                Type = array,
                IsOpenGenericType = false //elemType is ITypeParameterSymbol,
            };

            var arrayRank = array.GetArrayRank();
            if (arrayRank == 1)
            {
                info.FormatterName = typeof(ArrayFormatter<>).MakeGenericType(elemType).GetRealFullName();
            }
            else if (arrayRank == 2)
            {
                info.FormatterName = typeof(TwoDimensionalArrayFormatter<>).MakeGenericType(elemType).GetRealFullName();
            }
            else if (arrayRank == 3)
            {
                info.FormatterName = typeof(ThreeDimensionalArrayFormatter<>).MakeGenericType(elemType).GetRealFullName();
            }
            else if (arrayRank == 4)
            {
                info.FormatterName = typeof(FourDimensionalArrayFormatter<>).MakeGenericType(elemType).GetRealFullName();
            }
            else
            {
                throw new InvalidOperationException("does not supports array dimension, " + info.FullName);
            }

            collectedGenericInfo.Add(info);
        }

        private void CollectGeneric(Type type)
        {
            var genericType = type.GetGenericTypeDefinition();
            var isOpenGenericType = true; //type.GenericTypeArguments.Any(x => x is ITypeParameterSymbol);

            // special case
            if (type == typeof(ArraySegment<byte>) || type == typeof(ArraySegment<byte>?))
            {
                return;
            }

            // nullable
            if (Nullable.GetUnderlyingType(type) != null)
            {
                CollectCore(type.GenericTypeArguments[0]);

                if (!embeddedTypes.Contains(type.GenericTypeArguments[0]))
                {
                    var info = new GenericSerializationInfo
                    {
                        Type = type,
                        FormatterName = typeof(NullableFormatter<>).MakeGenericType(type.GenericTypeArguments[0]).GetRealFullName(),
                        IsOpenGenericType = isOpenGenericType,
                    };

                    collectedGenericInfo.Add(info);
                }

                return;
            }

            // collection
            if (knownGenericTypes.TryGetValue(genericType, out var formatterType))
            {
                foreach (var item in type.GenericTypeArguments)
                {
                    CollectCore(item);
                }

                var typeArgs = type.GenericTypeArguments;
                var info = new GenericSerializationInfo
                {
                    Type = type,
                    FormatterName = formatterType.MakeGenericType(typeArgs).GetRealFullName(),
                    IsOpenGenericType = isOpenGenericType,
                };

                collectedGenericInfo.Add(info);

                if (genericType == typeof(System.Linq.ILookup<,>))
                {
                    var formatter = knownGenericTypes[typeof(System.Linq.IGrouping<,>)];
                    var groupingInfo = new GenericSerializationInfo
                    {
                        Type = typeof(System.Linq.IGrouping<,>).MakeGenericType(typeArgs),
                        FormatterName = formatter.MakeGenericType(typeArgs).GetRealFullName(),
                        IsOpenGenericType = isOpenGenericType,
                    };

                    collectedGenericInfo.Add(groupingInfo);

                    formatter = knownGenericTypes[typeof(System.Collections.Generic.IEnumerable<>)];
                    typeArgs = new[] { type.GenericTypeArguments[1] };

                    var enumerableInfo = new GenericSerializationInfo
                    {
                        Type = typeof(System.Collections.Generic.IEnumerable<>).MakeGenericType(typeArgs),
                        FormatterName = formatter.MakeGenericType(typeArgs).GetRealFullName(),
                        IsOpenGenericType = isOpenGenericType,
                    };

                    collectedGenericInfo.Add(enumerableInfo);
                }

                return;
            }

            // Generic types
            if (type.IsGenericTypeDefinition)
            {
                CollectGenericUnion(type);
                CollectObject(type);
                return;
            }
            else
            {
                // Collect substituted types for the properties and fields.
                // NOTE: It is used to register formatters from nested generic type.
                //       However, closed generic types such as `Foo<string>` are not registered as a formatter.
                GetObjectInfo(type);
            }

            // Collect substituted types for the type parameters (e.g. Bar in Foo<Bar>)
            foreach (var item in type.GenericTypeArguments)
            {
                CollectCore(item);
            }

            var formatterBuilder = new StringBuilder();
            formatterBuilder.Append("global::");
            if (!string.IsNullOrEmpty(type.Namespace))
            {
                formatterBuilder.Append(type.Namespace + ".");
            }

            formatterBuilder.Append(type.Name);
            formatterBuilder.Append("Formatter<");
            formatterBuilder.Append(string.Join(", ", type.GenericTypeArguments.Select(x => x.FullName)));
            formatterBuilder.Append(">");

            var genericSerializationInfo = new GenericSerializationInfo
            {
                Type = type,
                FormatterName = formatterBuilder.ToString(),
                IsOpenGenericType = isOpenGenericType,
            };

            collectedGenericInfo.Add(genericSerializationInfo);
        }

        private void CollectObject(Type type)
        {
            ObjectSerializationInfo info = GetObjectInfo(type);
            collectedObjectInfo.Add(info);
        }

        private ObjectSerializationInfo GetObjectInfo(Type type)
        {
            TypeInfo ti = type.GetTypeInfo();
            var isClass = ti.IsClass || ti.IsInterface || ti.IsAbstract;
            var isStruct = ti.IsValueType;

            MessagePackObjectAttribute contractAttr = ti.GetCustomAttribute<MessagePackObjectAttribute>();
            DataContractAttribute dataContractAttr = ti.GetCustomAttribute<DataContractAttribute>();

            if (contractAttr == null)
            {
                throw new MessagePackGeneratorResolveFailedException("Serialization Object must mark MessagePackObjectAttribute." + " type: " + type.FullName);
            }

            var isIntKey = true;
            var intMembers = new Dictionary<int, MemberSerializationInfo>();
            var stringMembers = new Dictionary<string, MemberSerializationInfo>();

            if (isForceUseMap || contractAttr.KeyAsPropertyName)
            {
                // All public members are serialize target except [Ignore] member.
                isIntKey = false;

                var hiddenIntKey = 0;

                // Group the properties and fields by name to qualify members of the same name
                // (declared with the 'new' keyword) with the declaring type.
                IEnumerable<IGrouping<string, MemberInfo>> membersByName = type.GetRuntimeProperties()
                    .Concat(type.GetRuntimeFields().Cast<MemberInfo>())
                    .OrderBy(m => m.DeclaringType, OrderBaseTypesBeforeDerivedTypes.Instance)
                    .GroupBy(m => m.Name);

                foreach (var memberGroup in membersByName)
                {
                    foreach (MemberInfo item in memberGroup)
                    {
                        if (IgnoreMember(item))
                        {
                            continue;
                        }

                        var customFormatterAttr = item.GetCustomAttribute<MessagePackFormatterAttribute>();

                        Type memberType = null;
                        if (item is PropertyInfo prop)
                            memberType = prop.PropertyType;
                        else if (item is FieldInfo field)
                            memberType = field.FieldType;

                        MemberSerializationInfo member;
                        if (item is PropertyInfo property)
                        {
                            if (property.IsIndexer())
                            {
                                continue;
                            }

                            MethodInfo getMethod = property.GetGetMethod(true);
                            MethodInfo setMethod = property.GetSetMethod(true);

                            member = new MemberSerializationInfo
                            {
                                IsReadable = (getMethod != null) && getMethod.IsPublic && !getMethod.IsStatic,
                                IsWritable = (setMethod != null) && setMethod.IsPublic && !setMethod.IsStatic,
                                StringKey = item.Name,
                                IsProperty = true,
                                IsField = false,
                                Type = memberType,
                                Name = item.Name,
                                CustomFormatterTypeName = customFormatterAttr?.FormatterType.FullName,
                            };
                        }
                        else if (item is FieldInfo field)
                        {
                            if (item.GetCustomAttribute<System.Runtime.CompilerServices.CompilerGeneratedAttribute>(true) != null)
                            {
                                continue;
                            }

                            if (field.IsStatic)
                            {
                                continue;
                            }

                            member = new MemberSerializationInfo
                            {
                                IsReadable = field.IsPublic,
                                IsWritable = field.IsPublic && !field.IsInitOnly,
                                StringKey = item.Name,
                                IsProperty = false,
                                IsField = true,
                                Type = memberType,
                                Name = item.Name,
                                CustomFormatterTypeName = customFormatterAttr?.FormatterType.FullName,
                            };
                        }
                        else
                        {
                            throw new MessagePackSerializationException("unexpected member type");
                        }

                        if (!member.IsReadable && !member.IsWritable)
                        {
                            continue;
                        }

                        member.IntKey = hiddenIntKey++;
                        if (isIntKey)
                        {
                            intMembers.Add(member.IntKey, member);
                        }
                        else
                        {
                            stringMembers.Add(member.StringKey, member);
                        }

                        CollectCore(memberType); // recursive collect
                    }
                }
            }
            else
            {
                // Only KeyAttribute members
                var searchFirst = true;
                var hiddenIntKey = 0;

                foreach (PropertyInfo item in GetAllProperties(type))
                {
                    if (IgnoreMember(item))
                    {
                        continue;
                    }

                    if (item.IsIndexer())
                    {
                        continue; // .tt files don't generate good code for this yet: https://github.com/neuecc/MessagePack-CSharp/issues/390
                    }

                    var customFormatterAttr = item.GetCustomAttribute<MessagePackFormatterAttribute>();
                    MethodInfo getMethod = item.GetGetMethod(true);
                    MethodInfo setMethod = item.GetSetMethod(true);

                    var member = new MemberSerializationInfo
                    {
                        IsReadable = (getMethod != null) && getMethod.IsPublic && !getMethod.IsStatic,
                        IsWritable = (setMethod != null) && setMethod.IsPublic && !setMethod.IsStatic,
                        IsProperty = true,
                        IsField = false,
                        Type = item.PropertyType,
                        Name = item.Name,
                        CustomFormatterTypeName = customFormatterAttr?.FormatterType.FullName,
                    };
                    if (!member.IsReadable && !member.IsWritable)
                    {
                        continue;
                    }


                    KeyAttribute key;
                    if (contractAttr != null)
                    {
                        // MessagePackObjectAttribute
                        key = item.GetCustomAttribute<KeyAttribute>(true);
                        if (key == null)
                        {
                            if (requireIgnoreAttributes)
                                throw new MessagePackGeneratorResolveFailedException("all public members must mark KeyAttribute or IgnoreMemberAttribute." + " type: " + type.FullName + " member:" + item.Name);
                            continue;
                        }

                        if (key.IntKey == null && key.StringKey == null)
                        {
                            throw new MessagePackGeneratorResolveFailedException("both IntKey and StringKey are null." + " type: " + type.FullName + " member:" + item.Name);
                        }
                    }
                    else
                    {
                        // DataContractAttribute
                        DataMemberAttribute pseudokey = item.GetCustomAttribute<DataMemberAttribute>(true);
                        if (pseudokey == null)
                        {
                            // This member has no DataMemberAttribute nor IgnoreMemberAttribute.
                            // But the type *did* have a DataContractAttribute on it, so no attribute implies the member should not be serialized.
                            continue;
                        }

                        // use Order first
                        if (pseudokey.Order != -1)
                        {
                            key = new KeyAttribute(pseudokey.Order);
                        }
                        else if (pseudokey.Name != null)
                        {
                            key = new KeyAttribute(pseudokey.Name);
                        }
                        else
                        {
                            key = new KeyAttribute(item.Name); // use property name
                        }
                    }

                    if (searchFirst)
                    {
                        searchFirst = false;
                        isIntKey = key.IntKey != null;
                    }
                    else
                    {
                        if ((isIntKey && key.IntKey == null) || (!isIntKey && key.StringKey == null))
                        {
                            throw new MessagePackGeneratorResolveFailedException("all members key type must be same." + " type: " + type.FullName + " member:" + item.Name);
                        }
                    }

                    if (isIntKey)
                    {
                        member.IntKey = key.IntKey.Value;
                        if (intMembers.ContainsKey(member.IntKey))
                        {
                            throw new MessagePackGeneratorResolveFailedException("key is duplicated, all members key must be unique." + " type: " + type.FullName + " member:" + item.Name);
                        }

                        intMembers.Add(member.IntKey, member);
                    }
                    else
                    {
                        member.StringKey = key.StringKey;
                        if (stringMembers.ContainsKey(member.StringKey))
                        {
                            throw new MessagePackGeneratorResolveFailedException("key is duplicated, all members key must be unique." + " type: " + type.FullName + " member:" + item.Name);
                        }

                        member.IntKey = hiddenIntKey++;
                        stringMembers.Add(member.StringKey, member);
                    }

                    CollectCore(item.PropertyType); // recursive collect
                }

                foreach (FieldInfo item in GetAllFields(type))
                {
                    if (IgnoreMember(item))
                    {
                        continue;
                    }

                    if (item.GetCustomAttribute<System.Runtime.CompilerServices.CompilerGeneratedAttribute>(true) != null)
                    {
                        continue;
                    }

                    if (item.IsStatic)
                    {
                        continue;
                    }

                    var customFormatterAttr = item.GetCustomAttribute<MessagePackFormatterAttribute>();

                    var member = new MemberSerializationInfo
                    {
                        IsReadable = item.IsPublic,
                        IsWritable = item.IsPublic && !item.IsInitOnly,
                        IsProperty = true,
                        IsField = false,
                        Type = item.FieldType,
                        Name = item.Name,
                        CustomFormatterTypeName = customFormatterAttr?.FormatterType.FullName,
                    };
                    if (!member.IsReadable && !member.IsWritable)
                    {
                        continue;
                    }

                    KeyAttribute key;
                    if (contractAttr != null)
                    {
                        // MessagePackObjectAttribute
                        key = item.GetCustomAttribute<KeyAttribute>(true);
                        if (key == null)
                        {
                            if (requireIgnoreAttributes)
                                throw new MessagePackGeneratorResolveFailedException("all public members must mark KeyAttribute or IgnoreMemberAttribute." + " type: " + type.FullName + " member:" + item.Name);
                            continue;
                        }

                        if (key.IntKey == null && key.StringKey == null)
                        {
                            throw new MessagePackGeneratorResolveFailedException("both IntKey and StringKey are null." + " type: " + type.FullName + " member:" + item.Name);
                        }
                    }
                    else
                    {
                        // DataContractAttribute
                        DataMemberAttribute pseudokey = item.GetCustomAttribute<DataMemberAttribute>(true);
                        if (pseudokey == null)
                        {
                            // This member has no DataMemberAttribute nor IgnoreMemberAttribute.
                            // But the type *did* have a DataContractAttribute on it, so no attribute implies the member should not be serialized.
                            continue;
                        }

                        // use Order first
                        if (pseudokey.Order != -1)
                        {
                            key = new KeyAttribute(pseudokey.Order);
                        }
                        else if (pseudokey.Name != null)
                        {
                            key = new KeyAttribute(pseudokey.Name);
                        }
                        else
                        {
                            key = new KeyAttribute(item.Name); // use property name
                        }
                    }

                    if (searchFirst)
                    {
                        searchFirst = false;
                        isIntKey = key.IntKey != null;
                    }
                    else
                    {
                        if ((isIntKey && key.IntKey == null) || (!isIntKey && key.StringKey == null))
                        {
                            throw new MessagePackGeneratorResolveFailedException("all members key type must be same." + " type: " + type.FullName + " member:" + item.Name);
                        }
                    }

                    if (isIntKey)
                    {
                        member.IntKey = key.IntKey.Value;
                        if (intMembers.ContainsKey(member.IntKey))
                        {
                            throw new MessagePackGeneratorResolveFailedException("key is duplicated, all members key must be unique." + " type: " + type.FullName + " member:" + item.Name);
                        }

                        intMembers.Add(member.IntKey, member);
                    }
                    else
                    {
                        member.StringKey = key.StringKey;
                        if (stringMembers.ContainsKey(member.StringKey))
                        {
                            throw new MessagePackGeneratorResolveFailedException("key is duplicated, all members key must be unique." + " type: " + type.FullName + " member:" + item.Name);
                        }

                        member.IntKey = hiddenIntKey++;
                        stringMembers.Add(member.StringKey, member);
                    }

                    CollectCore(item.FieldType); // recursive collect
                }
            }

            // GetConstructor
            IEnumerator<ConstructorInfo> ctorEnumerator = null;
            ConstructorInfo ctor = ti.DeclaredConstructors.SingleOrDefault(x => x.GetCustomAttribute<SerializationConstructorAttribute>(false) != null);
            if (ctor == null)
            {
                ctorEnumerator =
                    ti.DeclaredConstructors.Where(x => !x.IsStatic && x.IsPublic).OrderByDescending(x => x.GetParameters().Length)
                        .GetEnumerator();

                if (ctorEnumerator.MoveNext())
                {
                    ctor = ctorEnumerator.Current;
                }
            }

            // struct allows null ctor
            if (ctor == null && !isStruct)
            {
                throw new MessagePackGeneratorResolveFailedException("can't find public constructor. type:" + type.FullName);
            }

            var constructorParameters = new List<MemberSerializationInfo>();
            if (ctor != null)
            {
                ILookup<string, KeyValuePair<string, MemberSerializationInfo>> constructorLookupByKeyDictionary = stringMembers.ToLookup(x => x.Key, x => x, StringComparer.OrdinalIgnoreCase);
                ILookup<string, KeyValuePair<string, MemberSerializationInfo>> constructorLookupByMemberNameDictionary = stringMembers.ToLookup(x => x.Value.Name, x => x, StringComparer.OrdinalIgnoreCase);
                do
                {
                    constructorParameters.Clear();
                    var ctorParamIndex = 0;
                    foreach (ParameterInfo item in ctor.GetParameters())
                    {
                        MemberSerializationInfo paramMember;
                        if (isIntKey)
                        {
                            if (intMembers.TryGetValue(ctorParamIndex, out paramMember))
                            {
                                if ((item.ParameterType == paramMember.Type ||
                                     item.ParameterType.GetTypeInfo().IsAssignableFrom(paramMember.Type))
                                    && paramMember.IsReadable)
                                {
                                    constructorParameters.Add(paramMember);
                                }
                                else
                                {
                                    if (ctorEnumerator != null)
                                    {
                                        ctor = null;
                                        continue;
                                    }
                                    else
                                    {
                                        throw new MessagePackGeneratorResolveFailedException("can't find matched constructor parameter, parameterType mismatch. type:" + type.FullName + " parameterIndex:" + ctorParamIndex + " paramterType:" + item.ParameterType.Name);
                                    }
                                }
                            }
                            else
                            {
                                if (ctorEnumerator != null)
                                {
                                    ctor = null;
                                    continue;
                                }
                                else
                                {
                                    throw new MessagePackGeneratorResolveFailedException("can't find matched constructor parameter, index not found. type:" + type.FullName + " parameterIndex:" + ctorParamIndex);
                                }
                            }
                        }
                        else
                        {
                            // Lookup by both string key name and member name
                            IEnumerable<KeyValuePair<string, MemberSerializationInfo>> hasKey = constructorLookupByKeyDictionary[item.Name];
                            IEnumerable<KeyValuePair<string, MemberSerializationInfo>> hasKeyByMemberName = constructorLookupByMemberNameDictionary[item.Name];

                            var lenByKey = hasKey.Count();
                            var lenByMemberName = hasKeyByMemberName.Count();

                            var len = lenByKey;

                            // Prefer to use string key name unless a matching string key is not found but a matching member name is
                            if (lenByKey == 0 && lenByMemberName != 0)
                            {
                                len = lenByMemberName;
                                hasKey = hasKeyByMemberName;
                            }

                            if (len != 0)
                            {
                                if (len != 1)
                                {
                                    if (ctorEnumerator != null)
                                    {
                                        ctor = null;
                                        continue;
                                    }
                                    else
                                    {
                                        throw new MessagePackGeneratorResolveFailedException("duplicate matched constructor parameter name:" + type.FullName + " parameterName:" + item.Name + " paramterType:" + item.ParameterType.Name);
                                    }
                                }

                                paramMember = hasKey.First().Value;
                                if (item.ParameterType.IsAssignableFrom(paramMember.Type) && paramMember.IsReadable)
                                {
                                    constructorParameters.Add(paramMember);
                                }
                                else
                                {
                                    if (ctorEnumerator != null)
                                    {
                                        ctor = null;
                                        continue;
                                    }
                                    else
                                    {
                                        throw new MessagePackGeneratorResolveFailedException("can't find matched constructor parameter, parameterType mismatch. type:" + type.FullName + " parameterName:" + item.Name + " paramterType:" + item.ParameterType.Name);
                                    }
                                }
                            }
                            else
                            {
                                if (ctorEnumerator != null)
                                {
                                    ctor = null;
                                    continue;
                                }
                                else
                                {
                                    throw new MessagePackGeneratorResolveFailedException("can't find matched constructor parameter, index not found. type:" + type.FullName + " parameterName:" + item.Name);
                                }
                            }
                        }

                        ctorParamIndex++;
                    }
                } while (TryGetNextConstructor(ctorEnumerator, ref ctor));

                if (ctor == null)
                {
                    constructorParameters.Clear();
                    //throw new MessagePackGeneratorResolveFailedException("can't find matched constructor. type:" + type.FullName);
                }
            }

            var hasSerializationConstructor = type.GetInterface(nameof(IMessagePackSerializationCallbackReceiver)) != null;
            var needsCastOnBefore = true;
            var needsCastOnAfter = true;
            if (hasSerializationConstructor)
            {
                needsCastOnBefore = type.GetMethod("OnBeforeSerialize") != null;
                needsCastOnAfter = type.GetMethod("OnAfterDeserialize") != null;
            }

            return new ObjectSerializationInfo
            {
                Type = type,
                IsClass = isClass,
                IsOpenGenericType = type.IsGenericType,
                GenericTypeParameters = type.IsGenericType
                    ? type.GenericTypeArguments.Select(ToGenericTypeParameterInfo).ToArray()
                    : Array.Empty<GenericTypeParameterInfo>(),
                ConstructorParameters = constructorParameters.ToArray(),
                IsIntKey = isIntKey,
                Members = isIntKey ? intMembers.Values.ToArray() : stringMembers.Values.ToArray(),
                Namespace = type.Namespace,
                HasIMessagePackSerializationCallbackReceiver = hasSerializationConstructor,
                NeedsCastOnAfter = needsCastOnAfter,
                NeedsCastOnBefore = needsCastOnBefore,
            };
        }

        private bool IgnoreMember(MemberInfo member) {
            return member.GetCustomAttribute<IgnoreMemberAttribute>(true) != null
                || member.GetCustomAttribute<IgnoreDataMemberAttribute>(true) != null
                || (!requireIgnoreAttributes && member.GetCustomAttribute<KeyAttribute>() == null);
        }

        private static GenericTypeParameterInfo ToGenericTypeParameterInfo(Type typeParameter)
        {
            var strConstraints = new List<string>();

            // `notnull`, `unmanaged`, `class`, `struct` constraint must come before any constraints.
            if ((typeParameter.GenericParameterAttributes & GenericParameterAttributes.NotNullableValueTypeConstraint) != 0)
            {
                strConstraints.Add("notnull");
            }

            if ((typeParameter.GenericParameterAttributes & GenericParameterAttributes.ReferenceTypeConstraint) != 0)
            {
                strConstraints.Add("class");
            }

            if ((typeParameter.GenericParameterAttributes & GenericParameterAttributes.NotNullableValueTypeConstraint) != 0)
            {
                strConstraints.Add("struct");
            }

            // constraint types (IDisposable, IEnumerable ...)
            foreach (var t in typeParameter.GetGenericParameterConstraints())
            {
                strConstraints.Add(t.FullName);
            }

            // `new()` constraint must be last in constraints.
            if ((typeParameter.GenericParameterAttributes & GenericParameterAttributes.DefaultConstructorConstraint) != 0)
            {
                strConstraints.Add("new()");
            }

            return new GenericTypeParameterInfo(typeParameter.FullName, string.Join(", ", strConstraints));
        }

        private static IEnumerable<FieldInfo> GetAllFields(Type type)
        {
            if (type.BaseType is object)
            {
                foreach (var item in GetAllFields(type.BaseType))
                {
                    yield return item;
                }
            }

            // with declared only
            foreach (var item in type.GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
            {
                yield return item;
            }
        }

        private static IEnumerable<PropertyInfo> GetAllProperties(Type type)
        {
            if (type.BaseType is object)
            {
                foreach (var item in GetAllProperties(type.BaseType))
                {
                    yield return item;
                }
            }

            // with declared only
            foreach (var item in type.GetProperties(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
            {
                yield return item;
            }
        }

        private static bool TryGetNextConstructor(IEnumerator<ConstructorInfo> ctorEnumerator, ref ConstructorInfo ctor)
        {
            if (ctorEnumerator == null || ctor != null)
            {
                return false;
            }

            if (ctorEnumerator.MoveNext())
            {
                ctor = ctorEnumerator.Current;
                return true;
            }
            else
            {
                ctor = null;
                return false;
            }
        }

        private bool IsAllowAccessibility(Type type)
        {
            do
            {
                if (!type.IsPublic && !type.IsNestedPublic)
                    return false;
                type = type.DeclaringType;
            } while (type != null);

            return true;
        }

        private class OrderBaseTypesBeforeDerivedTypes : IComparer<Type>
        {
            internal static readonly OrderBaseTypesBeforeDerivedTypes Instance = new OrderBaseTypesBeforeDerivedTypes();

            private OrderBaseTypesBeforeDerivedTypes()
            {
            }

            public int Compare(Type x, Type y)
            {
                return
                    x.IsEquivalentTo(y) ? 0 :
                    x.IsAssignableFrom(y) ? -1 :
                    y.IsAssignableFrom(x) ? 1 :
                    0;
            }
        }
    }
}
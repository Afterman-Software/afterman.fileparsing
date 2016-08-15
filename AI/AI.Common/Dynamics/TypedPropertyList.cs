using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace AI.Common.Dynamics
{
    public static class TypedPropertyList
    {
        private static Dictionary<string, List<PropertyInfo>> propertyListDictionary = new Dictionary<string, List<PropertyInfo>>();

        public static List<PropertyInfo> GetPropertyList(Type type, bool orderByFieldOrderAttribute = false, bool includeReferenceTypes = false)
        {
            string key = type.ToString() + ";" + orderByFieldOrderAttribute.ToString() + ";" + includeReferenceTypes.ToString();
            if (propertyListDictionary.ContainsKey(key))
            {
                return propertyListDictionary[key];
            }
            else
            {
                {
                    var typePropertiesQuery = (from p in type.GetProperties()
                                               select p);
                    if (!includeReferenceTypes)
                        typePropertiesQuery = (from p in typePropertiesQuery
                                               where IsScalarType(p.PropertyType)
                                               select p);
                    List<PropertyInfo> typeProperties = typePropertiesQuery.ToList();
                    if (orderByFieldOrderAttribute)
                    {
                        typeProperties = typeProperties.Where(p => p.GetCustomAttributes(typeof(FieldOrderAttribute), false).Length > 0).ToList();
                        typeProperties = typeProperties.OrderBy(p => ((FieldOrderAttribute)p.GetCustomAttribute(typeof(FieldOrderAttribute), false)).FieldOrder).ToList();
                    }
                    propertyListDictionary.Add(key, typeProperties);
                    return typeProperties;
                }
            }
        }

        private static readonly HashSet<Type> scalarTypes = LoadScalarTypes();
        private static readonly HashSet<Type> numericTypes = LoadNumericTypes();

        private static HashSet<Type> LoadScalarTypes()
        {
            HashSet<Type> set = new HashSet<Type>() 
							  { 
								//reference types
								typeof(String),
								typeof(Byte[]),
								//value types
								typeof(Byte),
								typeof(Int16),
								typeof(Int32),
								typeof(Int64),
								typeof(SByte),
								typeof(UInt16),
								typeof(UInt32),
								typeof(UInt64),
								typeof(Single),
								typeof(Double),
								typeof(Decimal),
								typeof(DateTime),
								typeof(Guid),
								typeof(Boolean),
								typeof(TimeSpan),
								//pointer types
								typeof(IntPtr),
								typeof(UIntPtr),
								//nullable value types
								typeof(Byte?),
								typeof(Int16?),
								typeof(Int32?),
								typeof(Int64?),
								typeof(SByte?),
								typeof(UInt16?),
								typeof(UInt32?),
								typeof(UInt64?),
								typeof(Single?),
								typeof(Double?),
								typeof(Decimal?),
								typeof(DateTime?),
								typeof(Guid?),
								typeof(Boolean?),
								typeof(TimeSpan?),
								//nullable pointer types
								typeof(IntPtr?),
								typeof(UIntPtr?)
							  };
            return set;
        }

        private static HashSet<Type> LoadNumericTypes()
        {
            HashSet<Type> set = new HashSet<Type>() 
							  { 
								//numeric value types
								typeof(Byte),
								typeof(Int16),
								typeof(Int32),
								typeof(Int64),
								typeof(SByte),
								typeof(UInt16),
								typeof(UInt32),
								typeof(UInt64),
								typeof(Single),
								typeof(Double),
								typeof(Decimal),
								//pointer types
								typeof(IntPtr),
								typeof(UIntPtr),
								//numeric nullable value types
								typeof(Byte?),
								typeof(Int16?),
								typeof(Int32?),
								typeof(Int64?),
								typeof(SByte?),
								typeof(UInt16?),
								typeof(UInt32?),
								typeof(UInt64?),
								typeof(Single?),
								typeof(Double?),
								typeof(Decimal?),
								//pointer types
								typeof(IntPtr),
								typeof(UIntPtr)
							  };
            return set;
        }

        public static bool IsScalarType(Type t)
        {
            return scalarTypes.Contains(t);
        }

        public static bool IsNumericType(Type t)
        {
            return numericTypes.Contains(t);
        }
    }
}
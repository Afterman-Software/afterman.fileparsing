using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using AI.Common.Dynamics;

namespace AI.Common.Extensions.Sys
{
    public static class MbroHelper
    {
        private static PropertyInfo _idProp = typeof(MarshalByRefObject).GetProperty("Identity", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);

        private static List<Type> _ignoreTypes = new List<Type>() { typeof(Type), typeof(ValueType) };
        private static List<string> _ignoreTypeNames = new List<string>() { "system.runtimetype" };
        private static List<Type> _ignoreBaseTypes = new List<Type>() { typeof(Enum) };

        public static void SetNullIdentity(object objGraph)
        {
            if (objGraph == null || _ignoreTypes.Contains(objGraph.GetType()) || _ignoreTypeNames.Contains(objGraph.GetType().FullName.ToLowerInvariant()) || _ignoreBaseTypes.Contains(objGraph.GetType().BaseType) || TypedPropertyList.IsScalarType(objGraph.GetType()))
                return;

            if (objGraph is MarshalByRefObject)
            {
                _idProp.SetValue(objGraph, null);
            }
            List<PropertyInfo> propList = TypedPropertyList.GetPropertyList(objGraph.GetType(), false, true);
            foreach (PropertyInfo prop in propList)
            {
                if (!prop.PropertyType.Namespace.ToLowerInvariant().StartsWith("system.reflection"))
                {
                    object propValue = prop.GetValue(objGraph);
                    if (propValue != null && !_ignoreTypes.Contains(propValue.GetType()) && !_ignoreTypeNames.Contains(propValue.GetType().FullName.ToLowerInvariant()) && !_ignoreBaseTypes.Contains(propValue.GetType().BaseType) && !TypedPropertyList.IsScalarType(propValue.GetType()))
                    {
                        if (propValue is IEnumerable)
                        {
                            foreach (object propValueX in (IEnumerable)propValue)
                            {
                                SetNullIdentity(propValueX);
                            }
                        }
                        else
                        {
                            SetNullIdentity(propValue);
                        }
                    }
                }
            }
        }
    }
}
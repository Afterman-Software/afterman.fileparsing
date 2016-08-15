using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace AI.Common.Dynamics
{
    public static class TypePropertyMixer
    {
        public static TResult GetSingleObjectFromMultipleObjects<TResult>(object[] objectsToMix, bool firstInWins = true) where TResult : class, new()
        {
            List<string> usedProperties = new List<string>();
            List<PropertyInfo> properties = TypedPropertyList.GetPropertyList(typeof(TResult));
            if (properties == null)
                return null;

            TResult result = new TResult();
            foreach (object oToMix in objectsToMix)
            {
                List<PropertyInfo> mixProperties = TypedPropertyList.GetPropertyList(oToMix.GetType());
                foreach (PropertyInfo pi in mixProperties)
                {
                    if (pi.CanRead && ((firstInWins && !usedProperties.Contains(pi.Name)) || (!firstInWins)))
                    {
                        PropertyInfo property = properties.Where(p => p.CanWrite && p.PropertyType == pi.PropertyType && p.Name == pi.Name).SingleOrDefault();
                        if (property == null)
                            continue;

                        object value = pi.GetMethod.Invoke(oToMix, null);

                        property.SetMethod.Invoke(result, new object[] { value });

                        usedProperties.Add(property.Name);
                    }
                }
            }

            return result;
        }

        public static List<object> GetMultipleObjectsFromSingleObject(object objectToDeconstruct, Type[] typesToUse)
        {
            List<object> result = new List<object>();

            List<PropertyInfo> properties = TypedPropertyList.GetPropertyList(objectToDeconstruct.GetType());
            if (properties == null)
                return null;

            bool found = false;
            foreach (PropertyInfo pi in properties)
            {
                found = false;
                if (pi.CanRead)
                {
                    foreach (Type t in typesToUse)
                    {
                        List<PropertyInfo> mixableProperties = TypedPropertyList.GetPropertyList(t);
                        foreach (PropertyInfo mpi in mixableProperties)
                        {
                            if (mpi.CanWrite && mpi.PropertyType == pi.PropertyType && mpi.Name == pi.Name)
                            {
                                object newObject = null;
                                if (result.Count(o => o.GetType() == t) == 1)
                                {
                                    newObject = result.SingleOrDefault(o => o.GetType() == t);
                                }
                                else
                                {
                                    newObject = t.GetConstructor(Type.EmptyTypes).Invoke(null);
                                }

                                object value = pi.GetMethod.Invoke(objectToDeconstruct, null);

                                mpi.SetMethod.Invoke(newObject, new object[] { value });

                                if (result.Count(o => o.GetType() == t) == 0)
                                {
                                    result.Add(newObject);
                                }

                                found = true;
                                break;
                            }
                        }
                        if (found)
                            break;
                    }
                }
            }

            return result;
        }
    }
}
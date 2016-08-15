using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AI.Common.Extensions.Sys
{
    public static class ReflectionHelper
    {
        public static IEnumerable<Type> GetAllTypes<T>()
        {
            var name = String.Empty;
            var list = new List<Type>();
            try
            {
                foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
                {
                    name = asm.FullName;
                    if (name.Contains("Rhinos")) continue;
                        foreach (var type in asm.GetTypes())
                        {
                            if (typeof(T).IsAssignableFrom(type) && type != typeof(T))
                            {
                                list.Add(type);
                            }
                        }
                   
                }
            }
            catch (Exception e)
            {
                System.Diagnostics.EventLog.WriteEntry("Application", e.ToString() + "***** NAME -> " + name);

                throw;
            }
           
            return list;
        }

        public static IEnumerable<Type> GetAllTypesByName(string typeName)
        {
            var name = String.Empty;
            var list = new List<Type>();
            try
            {
                foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
                {
                    name = asm.FullName;
                    if (name.Contains("Rhinos")) continue;
                    foreach (var type in asm.GetTypes())
                    {
                        if (type.Name.Trim() == typeName.Trim())
                        {
                            list.Add(type);
                        }
                    }

                }
            }
            catch (Exception e)
            {
                System.Diagnostics.EventLog.WriteEntry("Application", e.ToString() + "***** NAME -> " + name);

                throw;
            }

            return list;
        }

        public static IEnumerable<Type> GetAllTypesByFullName(string fullTypeName)
        {
            var name = String.Empty;
            var list = new List<Type>();
            try
            {
                foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
                {
                    name = asm.FullName;
                    if (name.Contains("Rhinos")) continue;
                    foreach (var type in asm.GetTypes())
                    {
                        if (type.FullName.Trim() == fullTypeName.Trim())
                        {
                            list.Add(type);
                        }
                    }

                }
            }
            catch (Exception e)
            {
                System.Diagnostics.EventLog.WriteEntry("Application", e.ToString() + "***** NAME -> " + name);

                throw;
            }

            return list;
        }
    }
}

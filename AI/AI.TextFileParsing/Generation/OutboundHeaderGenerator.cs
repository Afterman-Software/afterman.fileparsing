using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using AI.Common.Dynamics;

namespace AI.TextFileParsing.Generation
{
    public class OutboundHeaderGenerator : IOutboundHeaderGenerator
    {
        private class FieldOrderHolder
        {
            public FieldOrderAttribute FieldOrder;
            public PropertyInfo Property;
        }
        public IList<string> GetHeaders(object recordPartType)
        {
            var type = recordPartType.GetType();
            var propertyList = new List<FieldOrderHolder>();
            foreach (var property in type.GetProperties())
            {
                var attr = property.GetCustomAttribute<FieldOrderAttribute>();
                if (null != attr)
                {
                    propertyList.Add(new FieldOrderHolder()
                    {
                        FieldOrder = attr,
                        Property = property,
                    });
                }
            }
            propertyList.Sort(
                (attribute, orderAttribute) => { return attribute.FieldOrder.FieldOrder.CompareTo(attribute.FieldOrder.FieldOrder); });
            return propertyList.ConvertAll(x => x.FieldOrder.DisplayName ?? x.Property.Name).ToList();
        }
    }
}

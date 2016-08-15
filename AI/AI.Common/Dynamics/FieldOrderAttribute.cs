using System;
using System.Reflection;

namespace AI.Common.Dynamics
{
    public class FieldOrderAttribute : Attribute
    {
        int _fieldOrder = 0;
        private string _displayName;

        public FieldOrderAttribute(int fieldOrder)
        {
            _fieldOrder = fieldOrder;
        }

        public FieldOrderAttribute(int fieldOrder, string displayName)
        {
            _fieldOrder = fieldOrder;
            _displayName = displayName;
        }


        public int FieldOrder
        {
            get
            {
                return _fieldOrder;
            }
        }

        public string DisplayName
        {
            get
            {
                return _displayName;
            }
            set { _displayName = value; }
        }

        
    }

    public static class FieldOrderExtensions
    {
        public static string GetPropertyDisplayName(this PropertyInfo propertyInfo)
        {
            var attr = propertyInfo.GetCustomAttribute<FieldOrderAttribute>();
            if (null == attr)
                return propertyInfo.Name;
            return attr.DisplayName ?? propertyInfo.Name;
        }
    }
}
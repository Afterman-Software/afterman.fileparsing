using System;
using System.Reflection;

namespace AI.Common.Dynamics
{
	public class CreateablePropertyInfo : PropertyInfo
	{
		private string _name;
		private bool _canRead;
		private bool _canWrite;
		private Type _propertyType;

		private PropertyInfo _prototypePropertyInfo;

		public CreateablePropertyInfo(string name, bool canRead, bool canWrite, Type propertyType)
			: base()
		{
			_name = name;
			_canRead = canRead;
			_canWrite = canWrite;
			_propertyType = propertyType;
		}

		public CreateablePropertyInfo(string name, Type propertyType)
			: base()
		{
			_name = name;
			_canRead = true;
			_canWrite = true;
			_propertyType = propertyType;
		}

		public CreateablePropertyInfo(PropertyInfo prototypePropertyInfo)
			: base()
		{
			_prototypePropertyInfo = prototypePropertyInfo;
			_name = _prototypePropertyInfo.Name;
			_canRead = _prototypePropertyInfo.CanRead;
			_canWrite = _prototypePropertyInfo.CanWrite;
			_propertyType = _prototypePropertyInfo.PropertyType;
		}

		public override string Name
		{
			get
			{
				return _name;
			}
		}

		public override bool CanRead
		{
			get
			{
				return _canRead;
			}
		}

		public override bool CanWrite
		{
			get
			{
				return _canWrite;
			}
		}

		public override Type PropertyType
		{
			get
			{
				return _propertyType;
			}
		}

		public PropertyInfo ToPropertyInfo()
		{
			if (_prototypePropertyInfo != null)
				return _prototypePropertyInfo;
			else
				return this;
		}


		//implement base
		public override PropertyAttributes Attributes
		{
			get
			{
				if (_prototypePropertyInfo != null)
					return _prototypePropertyInfo.Attributes;
				else
					return PropertyAttributes.None;
			}
		}

		public override MethodInfo[] GetAccessors(bool nonPublic)
		{
			if (_prototypePropertyInfo != null)
				return _prototypePropertyInfo.GetAccessors(nonPublic);
			else
				return new MethodInfo[] { };

		}

		public override MethodInfo GetGetMethod(bool nonPublic)
		{
			if (_prototypePropertyInfo != null)
				return _prototypePropertyInfo.GetGetMethod(nonPublic);
			else
				return null;
		}

		public override ParameterInfo[] GetIndexParameters()
		{
			if (_prototypePropertyInfo != null)
				return _prototypePropertyInfo.GetIndexParameters();
			else
				return new ParameterInfo[] { };
		}

		public override MethodInfo GetSetMethod(bool nonPublic)
		{
			if (_prototypePropertyInfo != null)
				return _prototypePropertyInfo.GetSetMethod(nonPublic);
			else
				return null;
		}

		public override object GetValue(object obj, BindingFlags invokeAttr, Binder binder, object[] index, global::System.Globalization.CultureInfo culture)
		{
			if (_prototypePropertyInfo != null)
				return _prototypePropertyInfo.GetValue(obj, invokeAttr, binder, index, culture);
			else
				return null;
		}

		public override void SetValue(object obj, object value, BindingFlags invokeAttr, Binder binder, object[] index, global::System.Globalization.CultureInfo culture)
		{
			if (_prototypePropertyInfo != null)
				_prototypePropertyInfo.SetValue(obj, value, invokeAttr, binder, index, culture);
		}

		public override Type DeclaringType
		{
			get
			{
				if (_prototypePropertyInfo != null)
					return _prototypePropertyInfo.DeclaringType;
				else
					return null;
			}
		}

		public override object[] GetCustomAttributes(Type attributeType, bool inherit)
		{
			if (_prototypePropertyInfo != null)
				return _prototypePropertyInfo.GetCustomAttributes(attributeType, inherit);
			else
				return new object[] { };
		}

		public override object[] GetCustomAttributes(bool inherit)
		{
			if (_prototypePropertyInfo != null)
				return _prototypePropertyInfo.GetCustomAttributes(inherit);
			else
				return new object[] { };
		}

		public override bool IsDefined(Type attributeType, bool inherit)
		{
			if (_prototypePropertyInfo != null)
				return _prototypePropertyInfo.IsDefined(attributeType, inherit);
			else
				return false;
		}

		public override Type ReflectedType
		{
			get
			{
				if (_prototypePropertyInfo != null)
					return _prototypePropertyInfo.ReflectedType;
				else
					return null;
			}
		}
	}
}
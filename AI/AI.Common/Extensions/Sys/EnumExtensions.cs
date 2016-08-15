using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;


public static class EnumExtensions
{
	public static string GetDisplayDescription(this Enum enumValue)
	{
		var attr = enumValue.GetAttribute<DisplayAttribute>();
		if (attr != null)
			return attr.Description;
		else
			return enumValue.ToString();
	}

    public static bool In<T>(this Enum enumValue, params T[] vals)
    {
        if (null == enumValue)
            return false;
        foreach (var val in vals)
        {
            if (enumValue.Equals(val))
                return true;
        }
        return false;
    }

	public static string GetDisplayName(this Enum enumValue)
	{
		var attr = enumValue.GetAttribute<DisplayAttribute>();
		if (attr != null)
			return attr.Name;
		else
			return enumValue.ToString();
	}

	public static string Wordify(this Enum enumValue)
	{
		Regex r = new Regex("(?<=[a-z])(?<x>[A-Z])|(?<=.)(?<x>[A-Z])(?=[a-z])");
		return r.Replace(Enum.GetName(enumValue.GetType(), enumValue), " ${x}");
	}

	public static IList<string> GetAllMembers(this Enum @enum)
	{
		var list = new List<string>();
		var names = Enum.GetNames(@enum.GetType());
		list.AddRange(names);
		return list;
	}

	public static string GetDisplayShortName(this Enum enumValue)
	{
		var attr = enumValue.GetAttribute<DisplayAttribute>();
		if (attr != null)
			return attr.ShortName;
		else
			return enumValue.ToString();
	}

	public static string GetDisplayGroupName(this Enum enumValue)
	{
		var attr = enumValue.GetAttribute<DisplayAttribute>();
		if (attr != null)
			return attr.GroupName;
		else
			return enumValue.ToString();
	}

	public static TAttribute GetAttribute<TAttribute>(this Enum enumValue) where TAttribute : Attribute
	{
		var field = enumValue.GetType().GetField(enumValue.ToString());
	    if (field == null)
	        return null;

		var attrs = field.GetCustomAttributes(typeof(TAttribute), false);
		if (attrs.Length > 0)
			return (TAttribute)attrs[0];
		
		return null;
	}
}
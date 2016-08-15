using System.Collections.Generic;
using System.Linq;


public static class ListTExtensions
{
    public static bool IsNullOrEmpty<T>(this IEnumerable<T> list)
    {
        if (list == null || list.Count() == 0)
            return true;
        return false;
    }

    public static bool HasSameContent<T>(this List<T> list, List<T> compareList)
    {
        if (list == null && compareList != null)
            return false;
        if (list != null && compareList == null)
            return false;
        if (list.Count != compareList.Count)
            return false;

        for (int i = 0; i < list.Count; i++)
        {
            if (!compareList.Contains(list[i]))
                return false;
        }

        return true;
    }
}


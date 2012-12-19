using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public static class Extensions
{
    public static Transform Search(this Transform target, string name)
    {
        if (target.name == name) return target;

        for (int i = 0; i < target.childCount; ++i)
        {
            var result = Search(target.GetChild(i), name);

            if (result != null) return result;
        }

        return null;
    }
    public static List<Transform> SearchAll(this Transform target, string name)
    {
		List<Transform> r = new List<Transform>();
		
        if (target.name == name)
			r.Add(target);

        for (int i = 0; i < target.childCount; ++i)
        {
            r.AddRange(SearchAll(target.GetChild(i), name));
        }

        return r;
    }
}
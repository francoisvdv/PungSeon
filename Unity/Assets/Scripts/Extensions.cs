using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System;

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

    public static bool TryGetKey<K, V>(this IDictionary<K, V> instance, V value, out K key)
    {
        foreach (var entry in instance)
        {
            if (!entry.Value.Equals(value))
            {
                continue;
            }
            key = entry.Key;
            return true;
        }
        key = default(K);
        return false;
    }

    public static IPEndPoint GetIPEndPoint(this TcpClient instance)
    {
        return (IPEndPoint)instance.Client.RemoteEndPoint;
    }

    //checks if the value contains the provided type
    public static bool Has<T>(this System.Enum type, T value)
    {
        try
        {
            return (((int)(object)type & (int)(object)value) == (int)(object)value);
        }
        catch
        {
            return false;
        }
    }

    //checks if the value is only the provided type
    public static bool Is<T>(this System.Enum type, T value)
    {
        try
        {
            return (int)(object)type == (int)(object)value;
        }
        catch
        {
            return false;
        }
    }

    //appends a value
    public static T Add<T>(this System.Enum type, T value)
    {
        try
        {
            return (T)(object)(((int)(object)type | (int)(object)value));
        }
        catch (Exception ex)
        {
            throw new ArgumentException(
                string.Format(
                    "Could not append value from enumerated type '{0}'.",
                    typeof(T).Name
                    ), ex);
        }
    }

    //completely removes the value
    public static T Remove<T>(this System.Enum type, T value)
    {
        try
        {
            return (T)(object)(((int)(object)type & ~(int)(object)value));
        }
        catch (Exception ex)
        {
            throw new ArgumentException(
                string.Format(
                    "Could not remove value from enumerated type '{0}'.",
                    typeof(T).Name
                    ), ex);
        }
    }
}
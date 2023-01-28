using System.Text.Json;

namespace Mango.Utility;

/// <summary>
/// By default class <see cref="Microsoft.AspNetCore.Builder.SessionMiddlewareExtensions.UseSession"/>  
/// can save data only as numbers or strings. Classs <see cref="Utility.SessionExtentions"/>  helps to extend it`s functionality 
/// by enabling convert conplex objects into Json format and vice versa.
/// </summary>
public static class SessionExtentions
{
    public static void Set<T>(this ISession session, string key, T value)
    {
        session.SetString(key, JsonSerializer.Serialize(value));
    }

    public static T Get<T>(this ISession session, string key)
    {
        var value = session.GetString(key);

        return value == null ? default : JsonSerializer.Deserialize<T>(value);
    }


}

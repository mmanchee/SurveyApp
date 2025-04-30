using System.Text.Json;
using Microsoft.AspNetCore.Http;

namespace SurveyApp.Web.Helpers
{
    public static class SessionExtensions
    {
        // Store complex objects in session as JSON
        public static void SetObjectAsJson<T>(this ISession session, string key, T value)
        {
            session.SetString(key, JsonSerializer.Serialize(value));
        }

        // Retrieve complex objects from session JSON
        public static T? GetObjectFromJson<T>(this ISession session, string key)
        {
            var value = session.GetString(key);
            return value == null ? default : JsonSerializer.Deserialize<T>(value);
        }
    }
}
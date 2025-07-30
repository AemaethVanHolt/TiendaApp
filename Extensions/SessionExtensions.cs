using System.Text.Json;
using Microsoft.AspNetCore.Http;

namespace TiendaApp.Extensions
{
    public static class SessionExtensions // Esta clase extiende la funcionalidad de la sesi�n para almacenar y recuperar objetos como JSON
    {
        public static void SetObjectAsJson<T>(this ISession session, string key, T value) // M�todo para almacenar un objeto en la sesi�n como JSON
        {
            session.SetString(key, JsonSerializer.Serialize(value)); // Serializa el objeto a JSON y lo almacena en la sesi�n con la clave especificada
        }

        public static T? GetObjectFromJson<T>(this ISession session, string key)
        {
            var value = session.GetString(key);
            return value == null ? default : JsonSerializer.Deserialize<T>(value);
        }
    }
}

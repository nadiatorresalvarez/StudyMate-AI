namespace StudyMateAI.Infrastructure.Configuration
{
    /// <summary>
    /// Opciones de configuración para Google Authentication
    /// Desacopladas de la configuración general mediante el patrón Options de .NET
    /// </summary>
    public class GoogleAuthOptions
    {
        /// <summary>
        /// Nombre de la sección en appsettings.json
        /// </summary>
        public const string SectionName = "GoogleAuth";

        /// <summary>
        /// Google OAuth 2.0 Client ID desde Google Cloud Console
        /// Ejemplo: "519517973496-6qtam58eeshie6g1ig88ublmqfb46kdh.apps.googleusercontent.com"
        /// </summary>
        public string ClientId { get; set; } = string.Empty;

        /// <summary>
        /// Validación: verificar que ClientId esté configurado
        /// </summary>
        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(ClientId))
            {
                throw new InvalidOperationException(
                    $"GoogleAuth:ClientId no está configurado en appsettings.json. " +
                    $"Por favor, establece esta variable en la sección '{SectionName}'.");
            }
        }
    }
}

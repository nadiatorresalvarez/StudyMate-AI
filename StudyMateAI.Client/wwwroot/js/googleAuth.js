/**
 * Google Authentication Interop
 * Maneja la inicialización del botón de Google Sign-In
 * y la comunicación con C# a través de JavaScript Interop
 */

window.googleAuth = {
    /**
     * Inicializa Google Sign-In y renderiza el botón
     * @param {any} dotnetHelper - Referencia al componente Blazor
     * @param {string} clientId - Google Client ID desde Google Cloud Console
     */
    initialize: function (dotnetHelper, clientId) {
        try {
            // Verificar que Google GSI está cargado
            if (typeof google === 'undefined' || !google.accounts) {
                console.error('Google Identity Services no está cargado');
                return;
            }

            // Configurar e inicializar Google Sign-In
            google.accounts.id.initialize({
                client_id: clientId,
                callback: (response) => {
                    // Callback ejecutado cuando el usuario selecciona su cuenta
                    if (response.credential) {
                        console.log('Token de Google recibido');
                        // Invocar método en C#
                        dotnetHelper.invokeMethodAsync('LoginCallback', response.credential)
                            .catch(err => console.error('Error en LoginCallback:', err));
                    }
                },
                error_callback: () => {
                    console.error('Error en inicialización de Google');
                    dotnetHelper.invokeMethodAsync('LoginError', 'Error al inicializar Google Sign-In')
                        .catch(err => console.error('Error en LoginError:', err));
                }
            });

            // Renderizar el botón de Google
            const buttonContainer = document.getElementById("google-button-container");
            if (buttonContainer) {
                google.accounts.id.renderButton(
                    buttonContainer,
                    {
                        theme: "outline",
                        size: "large",
                        width: "100%",
                        text: "signin_with"
                    }
                );
            } else {
                console.error('Contenedor "google-button-container" no encontrado');
            }
        } catch (error) {
            console.error('Error al inicializar Google Auth:', error);
        }
    },

    /**
     * Limpia la sesión de Google
     */
    logout: function () {
        try {
            if (typeof google !== 'undefined' && google.accounts) {
                google.accounts.id.disableAutoSelect();
            }
        } catch (error) {
            console.error('Error al desconectar de Google:', error);
        }
    }
};

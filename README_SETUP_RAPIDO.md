# ⚡ Guía Rápida de Configuración - Autenticación con Google

## 🎯 Objetivo Final
Implementar login transparente con Google para StudyMate AI en 5 minutos.

---

## ✅ Lista de Verificación Rápida

### 1️⃣ Google Cloud Console (5 minutos)

```
[ ] Ir a https://console.cloud.google.com/
[ ] Crear proyecto llamado "StudyMate-AI"
[ ] Buscar "Google Identity Services" y habilitar
[ ] Ir a Credenciales → Crear Credenciales → ID Cliente OAuth
[ ] Seleccionar "Aplicación Web"
[ ] Agregar URIs autorizados:
    - http://localhost:5041
    - http://localhost:5041/login
[ ] Copiar el "ID de cliente" (el string largo con .apps.googleusercontent.com)
```

### 2️⃣ Código Frontend (2 minutos)

```csharp
// En Pages/Auth/Login.razor, línea ~65
googleClientId = "TU_GOOGLE_CLIENT_ID_AQUI.apps.googleusercontent.com";
```

### 3️⃣ NuGet Package (1 minuto)

```bash
cd StudyMateAI.Client
dotnet add package Blazored.LocalStorage
```

### 4️⃣ Backend Funcionando (Verificar)

```bash
cd StudyMateAI
dotnet run

# Verificar que POST /api/auth/google-login responde:
# http://localhost:5000 (o 5071 según tu configuración)
```

### 5️⃣ Cliente Funcionando (Verificar)

```bash
cd StudyMateAI.Client
dotnet run

# Abrir: http://localhost:5041/login
# Deberías ver el botón de Google
```

---

## 🔑 Dónde Poner tu Google Client ID

### Opción A: Hardcodear (Temporal)
**Archivo:** `StudyMateAI.Client/Pages/Auth/Login.razor`

```csharp
// Busca esta línea (aproximadamente línea 65):
googleClientId = "REEMPLAZA_CON_TU_GOOGLE_CLIENT_ID.apps.googleusercontent.com";

// Reemplázala con tu ID:
googleClientId = "123456789-abcdefghijklmnopqrstuvwxyz.apps.googleusercontent.com";
```

### Opción B: Usar appsettings.json (Recomendado)
**Archivo:** `StudyMateAI.Client/wwwroot/appsettings.json`

```json
{
  "googleAuth": {
    "clientId": "TU_GOOGLE_CLIENT_ID_AQUI.apps.googleusercontent.com"
  }
}
```

**Cargar en Login.razor:**
```csharp
// Agregar en OnAfterRenderAsync
var config = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .Build();
googleClientId = config["googleAuth:clientId"];
```

---

## 🧪 Testing Rápido

### Paso 1: Iniciar APIs

**Terminal 1 - Backend:**
```bash
cd "d:\Ciclo_6\StudyMateAI\StudyMate-AI\StudyMateAI"
dotnet run
# Debe estar en http://localhost:5000 (o el puerto que veas)
```

**Terminal 2 - Cliente:**
```bash
cd "d:\Ciclo_6\StudyMateAI\StudyMate-AI\StudyMateAI.Client"
dotnet run
# Debe estar en http://localhost:5041
```

### Paso 2: Prueba de Login

1. Abre `http://localhost:5041/login` en el navegador
2. Deberías ver:
   - Icono de escuela
   - Título "StudyMate AI"
   - **Botón de Google Sign-In**

3. Haz clic en el botón:
   - Se abre un popup de Google
   - Selecciona tu cuenta
   - Acepta los permisos

4. Espera redirección:
   - Deberías ir a `http://localhost:5041/` (dashboard)
   - Ya debes estar autenticado

### Paso 3: Verificar

Abre la consola del navegador (F12):

```javascript
// En la consola, verifica:
localStorage.getItem('jwtToken')  // Debe devolver un token
localStorage.getItem('userEmail') // Debe devolver tu email
```

---

## ❌ Solución de Problemas Rápida

### "El botón de Google no aparece"
```
✅ Solución: Verifica que en index.html está:
   <script src="https://accounts.google.com/gsi/client" async defer></script>
```

### "Error invalid_client"
```
✅ Solución: Tu Google Client ID es inválido
   - Copia nuevamente desde Google Cloud Console
   - Verifica que no tiene espacios
```

### "Error redirect_uri_mismatch"
```
✅ Solución: Tu URL no está autorizada en Google
   - Agregar en Google Cloud Console → Credenciales:
     http://localhost:5041
     http://localhost:5041/login
```

### "401 Unauthorized en API"
```
✅ Solución: El JWT no se envía correctamente
   - Verifica que CustomAuthStateProvider.SetAuthHeaders() se ejecuta
   - Abre F12 > Network y verifica el header Authorization
```

### "CORS error"
```
✅ Solución: El backend debe aceptar la URL del cliente
   - Verifica en StudyMateAI/Program.cs la configuración de CORS
   - Debe permitir: http://localhost:5041
```

---

## 🔑 Estructura de Carpetas Nueva

```
StudyMateAI.Client/
├── Auth/
│   ├── CustomAuthStateProvider.cs    ✅ ACTUALIZADO
│   └── JwtParser.cs                 ✅ NUEVO
├── Pages/
│   └── Auth/
│       └── Login.razor              ✅ NUEVO
├── Services/
│   ├── Interfaces/                  ✅ NUEVO
│   │   ├── IAuthService.cs
│   │   ├── IDocumentService.cs
│   │   └── ISubjectService.cs
│   └── Implementations/             ✅ NUEVO
│       ├── AuthService.cs
│       ├── DocumentService.cs
│       └── SubjectService.cs
├── DTOs/
│   └── Auth/
│       ├── GoogleLoginDto.cs        ✅ NUEVO
│       ├── AuthResponseDto.cs       ✅ ACTUALIZADO
│       └── AuthRequestDto.cs
├── Program.cs                        ✅ ACTUALIZADO
├── wwwroot/
│   ├── index.html                   ✅ ACTUALIZADO
│   ├── appsettings.json             ✅ NUEVO
│   └── js/
│       └── googleAuth.js            ✅ NUEVO
```

---

## 📚 Archivos de Documentación

```
StudyMateAI/
├── GUIA_GOOGLE_CLIENT_ID.md         ✅ COMPLETA (Pasos detallados)
├── REPORTE_AUTENTICACION_GOOGLE.md  ✅ COMPLETA (Cambios realizados)
└── README_SETUP_RAPIDO.md           ✅ ESTE ARCHIVO (Quick reference)
```

---

## 🚀 Comando para Instalar Paquetes

```bash
# Si aún no has instalado Blazored.LocalStorage
cd "d:\Ciclo_6\StudyMateAI\StudyMate-AI\StudyMateAI.Client"
dotnet add package Blazored.LocalStorage
```

---

## ⏱️ Tiempo Total de Setup

| Tarea | Tiempo |
|-------|--------|
| Obtener Google Client ID | 5 min |
| Actualizar código frontend | 2 min |
| Instalar paquetes | 1 min |
| Testing | 3 min |
| **TOTAL** | **~11 minutos** |

---

## 📞 Soporte Rápido

| Problema | Archivo a revisar |
|----------|------------------|
| "No veo cambios" | Program.cs |
| "Botón no aparece" | wwwroot/index.html |
| "Error de Google" | Pages/Auth/Login.razor |
| "No se guarda token" | Auth/CustomAuthStateProvider.cs |
| "API rechaza requests" | Backend/Program.cs (CORS) |

---

**Última actualización:** Diciembre 2024  
**Versión:** 1.0.0  
**⏱️ Lectura:** 5 minutos

# 📱 StudyMateAI.Client - Capa de Presentación (Blazor WebAssembly)

## 📋 Tabla de Contenidos

1. [¿Qué es StudyMateAI.Client?](#qué-es-studymateai-client)
2. [¿Rompe la Arquitectura Limpia?](#rompe-la-arquitectura-limpia)
3. [Arquitectura y Posición en Clean Architecture](#arquitectura-y-posición-en-clean-architecture)
4. [¿Por qué Blazor WebAssembly?](#por-qué-blazor-webassembly)
5. [Estructura del Proyecto](#estructura-del-proyecto)
6. [Cómo Ejecutar la Aplicación](#cómo-ejecutar-la-aplicación)
7. [Configuración y Puertos](#configuración-y-puertos)
8. [Componentes Principales](#componentes-principales)
9. [Flujo de Comunicación con el Backend](#flujo-de-comunicación-con-el-backend)
10. [Autenticación y Autorización](#autenticación-y-autorización)
11. [Servicios del Cliente](#servicios-del-cliente)
12. [DTOs (Data Transfer Objects)](#dtos-data-transfer-objects)
13. [Tecnologías Utilizadas](#tecnologías-utilizadas)
14. [Troubleshooting](#troubleshooting)

---

## 🎯 ¿Qué es StudyMateAI.Client?

**StudyMateAI.Client** es la **capa de presentación (Presentation Layer)** de la aplicación StudyMateAI. Es una aplicación web construida con **Blazor WebAssembly** que proporciona la interfaz de usuario (UI) para que los usuarios interactúen con el sistema.

### Propósito Principal

- ✅ Proporcionar una interfaz de usuario moderna y responsiva
- ✅ Comunicarse con la API backend (`StudyMateAI`) mediante HTTP
- ✅ Gestionar el estado de autenticación del usuario
- ✅ Manejar la navegación y el enrutamiento de la aplicación
- ✅ Presentar datos de forma visual e interactiva

---

## 🏗️ ¿Rompe la Arquitectura Limpia?

### ❌ **NO, NO ROMPE LA ARQUITECTURA LIMPIA**

La capa `StudyMateAI.Client` es **parte esencial** de Clean Architecture. Es la **capa más externa** del sistema y cumple el rol de **Presentation Layer**.

### Diagrama de Clean Architecture

```
┌─────────────────────────────────────────────────────────┐
│           CAPA DE PRESENTACIÓN (EXTERNA)                │
│  ┌───────────────────────────────────────────────────┐  │
│  │     StudyMateAI.Client (Blazor WebAssembly)      │  │
│  │  - Componentes UI (Razor)                        │  │
│  │  - Servicios HTTP                                │  │
│  │  - Autenticación del Cliente                     │  │
│  └───────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────┘
                        ↕ HTTP/REST API
┌─────────────────────────────────────────────────────────┐
│           CAPA DE APLICACIÓN                            │
│  ┌───────────────────────────────────────────────────┐  │
│  │     StudyMateAI.Application                      │  │
│  │  - Casos de Uso                                  │  │
│  │  - DTOs                                          │  │
│  │  - Validadores                                   │  │
│  └───────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────┘
                        ↕ Interfaces
┌─────────────────────────────────────────────────────────┐
│           CAPA DE DOMINIO (NÚCLEO)                      │
│  ┌───────────────────────────────────────────────────┐  │
│  │     StudyMateAI.Domain                            │  │
│  │  - Entidades                                      │  │
│  │  - Interfaces                                     │  │
│  │  - Lógica de Negocio                             │  │
│  └───────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────┘
                        ↕ Implementaciones
┌─────────────────────────────────────────────────────────┐
│           CAPA DE INFRAESTRUCTURA                       │
│  ┌───────────────────────────────────────────────────┐  │
│  │     StudyMateAI.Infrastructure                    │  │
│  │  - Base de Datos                                  │  │
│  │  - Servicios Externos (Gemini API)                │  │
│  │  - Adaptadores                                    │  │
│  └───────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────┘
```

### Principios de Clean Architecture Respetados

✅ **Dependencias hacia adentro**: El Client depende de la API, pero la API NO depende del Client  
✅ **Separación de responsabilidades**: Cada capa tiene un propósito específico  
✅ **Independencia de frameworks**: El dominio no depende de Blazor  
✅ **Testabilidad**: Cada capa puede probarse independientemente  

---

## 🎨 Arquitectura y Posición en Clean Architecture

### Capas del Sistema

| Capa | Proyecto | Responsabilidad |
|------|----------|-----------------|
| **Presentation** | `StudyMateAI.Client` | Interfaz de usuario, consumo de API |
| **Application** | `StudyMateAI.Application` | Casos de uso, lógica de aplicación |
| **Domain** | `StudyMateAI.Domain` | Entidades, reglas de negocio |
| **Infrastructure** | `StudyMateAI.Infrastructure` | Base de datos, servicios externos |
| **API/Controllers** | `StudyMateAI` | Endpoints REST, configuración |

### Flujo de Datos

```
Usuario → Blazor Client → HTTP Request → API Controllers
                                              ↓
                                    Application Layer
                                              ↓
                                    Domain Layer
                                              ↓
                                    Infrastructure Layer
                                              ↓
                                    Base de Datos / APIs Externas
```

---

## 🚀 ¿Por qué Blazor WebAssembly?

### Ventajas

1. **✅ C# End-to-End**: Mismo lenguaje en frontend y backend
2. **✅ Componentes Reutilizables**: Sistema de componentes similar a React/Vue
3. **✅ Rendimiento**: Ejecución en el navegador (WebAssembly)
4. **✅ Type Safety**: Tipado fuerte en toda la aplicación
5. **✅ Ecosistema .NET**: Reutilización de librerías y código
6. **✅ Hot Reload**: Desarrollo rápido con recarga en caliente

### Desventajas

- ⚠️ Tamaño inicial de descarga mayor que JavaScript puro
- ⚠️ Requiere .NET Runtime en el navegador (descarga automática)

---

## 📁 Estructura del Proyecto

```
StudyMateAI.Client/
│
├── 📄 Program.cs                    # Configuración inicial y servicios
├── 📄 App.razor                     # Componente raíz y enrutamiento
├── 📄 _Imports.razor                # Directivas using globales
│
├── 📂 Auth/                         # Autenticación
│   └── CustomAuthStateProvider.cs   # Proveedor de estado de autenticación
│
├── 📂 Components/                   # Componentes reutilizables
│   ├── RedirectToLogin.razor
│   ├── SubjectDialog.razor
│   └── UploadDialog.razor
│
├── 📂 DTOs/                         # Objetos de transferencia de datos
│   ├── Auth/
│   ├── Document/
│   ├── Flashcards/
│   ├── Profile/
│   ├── Subject/
│   └── Summary/
│
├── 📂 Layout/                        # Layouts de la aplicación
│   ├── LoginLayout.razor            # Layout para páginas de login
│   ├── MainLayout.razor             # Layout principal
│   └── NavMenu.razor                # Menú de navegación
│
├── 📂 Pages/                        # Páginas de la aplicación
│   ├── Home.razor
│   ├── Login.razor
│   ├── Subjects.razor
│   ├── Documents.razor
│   ├── DocumentDetail.razor
│   └── Profile.razor
│
├── 📂 Services/                     # Servicios HTTP del cliente
│   ├── AuthService.cs               # Autenticación
│   ├── SubjectService.cs            # Gestión de materias
│   ├── DocumentService.cs           # Gestión de documentos
│   ├── StudyService.cs              # Servicios de estudio
│   └── ProfileService.cs            # Perfil de usuario
│
├── 📂 Properties/
│   └── launchSettings.json          # Configuración de ejecución
│
└── 📂 wwwroot/                      # Archivos estáticos
    ├── index.html                   # Página HTML principal
    ├── css/
    ├── lib/                         # Librerías (Bootstrap)
    └── sample-data/
```

---

## 🏃 Cómo Ejecutar la Aplicación

### Prerrequisitos

- ✅ .NET 9.0 SDK instalado
- ✅ Backend API (`StudyMateAI`) ejecutándose en `http://localhost:5071`
- ✅ Navegador web moderno (Chrome, Edge, Firefox)

### Pasos para Ejecutar

#### 1. **Asegúrate de que el Backend esté ejecutándose**

```bash
# En una terminal, navega al proyecto API
cd StudyMateAI

# Ejecuta el backend
dotnet run

# Verifica que esté corriendo en http://localhost:5071
```

#### 2. **Ejecuta el Cliente Blazor**

```bash
# En otra terminal, navega al proyecto Client
cd StudyMateAI.Client

# Ejecuta el cliente
dotnet run

# O usa el perfil específico
dotnet run --launch-profile http
```

#### 3. **Abre el navegador**

La aplicación se abrirá automáticamente en `http://localhost:5041`

### Comando Completo (Una Línea)

```bash
cd StudyMateAI.Client && dotnet run
```

### Ejecutar con Perfil Específico

```bash
# Perfil HTTP
dotnet run --launch-profile http

# Perfil HTTPS (si está configurado)
dotnet run --launch-profile https
```

---

## ⚙️ Configuración y Puertos

### Puertos por Defecto

| Aplicación | Puerto | URL |
|------------|--------|-----|
| **Backend API** | `5071` | `http://localhost:5071` |
| **Blazor Client** | `5041` | `http://localhost:5041` |

### Configuración de la URL del API

La URL del backend se configura en `Program.cs`:

```csharp
var apiUrl = "http://localhost:5071";
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(apiUrl) });
```

### Cambiar el Puerto del Cliente

Edita `Properties/launchSettings.json`:

```json
{
  "profiles": {
    "http": {
      "applicationUrl": "http://localhost:5041"  // Cambia aquí
    }
  }
}
```

### Configuración CORS en el Backend

El backend debe permitir solicitudes desde el cliente. En `StudyMateAI/Program.cs`:

```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: "AllowBlazorClient",
        policy =>
        {
            policy.WithOrigins("http://localhost:5041")  // Puerto del cliente
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
        });
});
```

---

## 🧩 Componentes Principales

### 1. **App.razor** - Componente Raíz

```razor
<CascadingAuthenticationState>
    <Router AppAssembly="@typeof(App).Assembly">
        <Found Context="routeData">
            <AuthorizeRouteView RouteData="@routeData" DefaultLayout="@typeof(MainLayout)">
                <NotAuthorized>
                    @if (context.User.Identity?.IsAuthenticated != true)
                    {
                        <RedirectToLogin />
                    }
                </NotAuthorized>
            </AuthorizeRouteView>
        </Found>
    </Router>
</CascadingAuthenticationState>
```

**Responsabilidades:**
- Configura el enrutamiento de la aplicación
- Maneja la autenticación a nivel de aplicación
- Redirige usuarios no autenticados al login

### 2. **Program.cs** - Configuración de Servicios

```csharp
var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");

var apiUrl = "http://localhost:5071";
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(apiUrl) });

// Servicios registrados
builder.Services.AddMudServices();
builder.Services.AddBlazoredLocalStorage();
builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthStateProvider>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<SubjectService>();
// ... más servicios
```

**Responsabilidades:**
- Registra todos los servicios de la aplicación
- Configura HttpClient con la URL del backend
- Configura autenticación y autorización

### 3. **CustomAuthStateProvider** - Autenticación

Maneja el estado de autenticación del usuario:

- Lee tokens JWT del almacenamiento local
- Proporciona estado de autenticación a los componentes
- Actualiza el estado cuando el usuario inicia/cierra sesión

---

## 🔄 Flujo de Comunicación con el Backend

### 1. **Inicialización**

```
Usuario abre navegador
    ↓
Blazor WebAssembly se descarga y ejecuta
    ↓
CustomAuthStateProvider verifica token en LocalStorage
    ↓
Si hay token → Usuario autenticado
Si no hay token → Usuario anónimo
```

### 2. **Petición HTTP Típica**

```
Componente Razor
    ↓
Inyecta Servicio (ej: SubjectService)
    ↓
Servicio hace HTTP Request al Backend
    ↓
HttpClient agrega token JWT en headers (si existe)
    ↓
Backend API procesa request
    ↓
Backend retorna respuesta JSON
    ↓
Servicio deserializa respuesta
    ↓
Componente actualiza UI
```

### 3. **Ejemplo: Obtener Materias**

```csharp
// En un componente Razor
@inject SubjectService SubjectService

@code {
    private List<SubjectResponseDto> subjects = new();

    protected override async Task OnInitializedAsync()
    {
        // El servicio hace la petición HTTP
        subjects = await SubjectService.GetAll();
    }
}
```

```csharp
// SubjectService.cs
public async Task<List<SubjectResponseDto>> GetAll()
{
    // HttpClient ya tiene el token JWT en los headers
    return await _http.GetFromJsonAsync<List<SubjectResponseDto>>("api/Subjects") 
           ?? new List<SubjectResponseDto>();
}
```

---

## 🔐 Autenticación y Autorización

### Flujo de Autenticación

1. **Login con Google**
   ```
   Usuario → Login.razor → AuthService.Login(googleToken)
       ↓
   POST /api/Auth/google-login
       ↓
   Backend valida token de Google
       ↓
   Backend retorna JWT token
       ↓
   Cliente guarda token en LocalStorage
       ↓
   CustomAuthStateProvider actualiza estado
       ↓
   Usuario autenticado → Redirige a Home
   ```

2. **Mantenimiento de Sesión**
   - Token JWT se guarda en `LocalStorage` (clave: `authToken`)
   - En cada petición HTTP, el token se envía en el header `Authorization: Bearer {token}`
   - `CustomAuthStateProvider` lee el token al iniciar la app

3. **Logout**
   ```
   Usuario → Logout
       ↓
   AuthService.Logout()
       ↓
   Elimina token de LocalStorage
       ↓
   CustomAuthStateProvider marca usuario como anónimo
       ↓
   Redirige a Login
   ```

### Protección de Rutas

Las rutas protegidas usan el atributo `[Authorize]` o el componente `<AuthorizeView>`:

```razor
@page "/subjects"
@attribute [Authorize]

<h3>Materias</h3>
<!-- Solo usuarios autenticados pueden ver esto -->
```

---

## 🛠️ Servicios del Cliente

### 1. **AuthService**

**Propósito**: Gestionar autenticación del usuario

**Métodos:**
- `Login(string googleToken)`: Autentica con Google
- `Logout()`: Cierra sesión

**Uso:**
```csharp
@inject AuthService AuthService

await AuthService.Login(googleToken);
```

### 2. **SubjectService**

**Propósito**: Operaciones CRUD de materias

**Métodos:**
- `GetAll()`: Obtiene todas las materias
- `Create(CreateSubjectDto)`: Crea una materia
- `Update(int id, UpdateSubjectDto)`: Actualiza una materia
- `Delete(int id)`: Elimina una materia

### 3. **DocumentService**

**Propósito**: Gestión de documentos

**Métodos:**
- `GetAll()`: Obtiene todos los documentos
- `GetById(int id)`: Obtiene un documento específico
- `Upload(MultipartFormDataContent)`: Sube un documento
- `Delete(int id)`: Elimina un documento

### 4. **StudyService**

**Propósito**: Servicios de estudio (resúmenes, flashcards, etc.)

**Métodos:**
- `GenerateSummary(int documentId)`: Genera resumen
- `GenerateFlashcards(int documentId)`: Genera flashcards
- `GenerateQuiz(int documentId)`: Genera cuestionario

### 5. **ProfileService**

**Propósito**: Gestión del perfil de usuario

**Métodos:**
- `GetProfile()`: Obtiene perfil del usuario
- `UpdateProfile(UpdateUserProfileRequest)`: Actualiza perfil

---

## 📦 DTOs (Data Transfer Objects)

Los DTOs están organizados por funcionalidad:

```
DTOs/
├── Auth/
│   ├── AuthRequestDto.cs
│   ├── AuthResponseDto.cs
│   └── UserProfileDto.cs
├── Subject/
│   ├── CreateSubjectDto.cs
│   ├── UpdateSubjectDto.cs
│   ├── SubjectResponseDto.cs
│   └── ArchiveSubjectDto.cs
├── Document/
│   └── DocumentResponseDto.cs
├── Summary/
│   └── GenerateBriefSummaryResponseDto.cs
└── Flashcards/
    └── FlashcardResponseDto.cs
```

**Propósito**: Transferir datos entre el cliente y el backend de forma estructurada y tipada.

---

## 🎨 Tecnologías Utilizadas

| Tecnología | Versión | Propósito |
|------------|---------|-----------|
| **Blazor WebAssembly** | 9.0 | Framework de UI |
| **MudBlazor** | 7.0.0 | Componentes UI modernos |
| **Blazored.LocalStorage** | 4.5.0 | Almacenamiento local del navegador |
| **Microsoft.AspNetCore.Components.Authorization** | 9.0.0 | Autenticación y autorización |
| **DocumentFormat.OpenXml** | 3.0.1 | Manipulación de documentos Office |
| **Bootstrap** | (incluido) | Estilos base |

---

## 🔧 Troubleshooting

### Problema: "Failed to fetch" o errores CORS

**Solución:**
1. Verifica que el backend esté ejecutándose en `http://localhost:5071`
2. Verifica que la política CORS en el backend incluya `http://localhost:5041`
3. Revisa la consola del navegador para más detalles

### Problema: Token no se guarda o se pierde

**Solución:**
1. Verifica que `Blazored.LocalStorage` esté registrado en `Program.cs`
2. Revisa la consola del navegador para errores de JavaScript
3. Verifica que el navegador permita almacenamiento local

### Problema: La aplicación no carga

**Solución:**
1. Limpia y reconstruye el proyecto:
   ```bash
   dotnet clean
   dotnet build
   dotnet run
   ```
2. Verifica que .NET 9.0 esté instalado: `dotnet --version`
3. Revisa la consola del navegador para errores

### Problema: No se conecta al backend

**Solución:**
1. Verifica que la URL en `Program.cs` sea correcta: `http://localhost:5071`
2. Verifica que el backend esté ejecutándose
3. Prueba acceder directamente a `http://localhost:5071/swagger` en el navegador

### Problema: Errores de compilación

**Solución:**
1. Restaura paquetes NuGet:
   ```bash
   dotnet restore
   ```
2. Verifica que todas las referencias estén correctas
3. Revisa los errores específicos en la salida de compilación

---

## 📚 Recursos Adicionales

- [Documentación oficial de Blazor WebAssembly](https://learn.microsoft.com/es-es/aspnet/core/blazor/)
- [MudBlazor Documentation](https://mudblazor.com/)
- [Clean Architecture Principles](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)

---

## 📝 Notas Importantes

1. **El cliente NO tiene acceso directo a la base de datos** - Todo pasa por la API
2. **El cliente NO contiene lógica de negocio** - Solo presenta datos y envía comandos
3. **El cliente es independiente** - Puede ser reemplazado por otra tecnología (React, Vue, etc.) sin afectar el backend
4. **CORS es esencial** - El backend debe permitir solicitudes desde el cliente

---

**Última actualización**: Diciembre 2024  
**Versión**: 1.0.0


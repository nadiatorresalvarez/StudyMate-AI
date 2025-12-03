# Arquitectura Desacoplada - Google Authentication Options

## 📋 Descripción

La configuración de Google Authentication ahora sigue el **patrón Options de .NET**, logrando una arquitectura completamente desacoplada donde:

1. **appsettings.json** → Almacena la configuración
2. **Program.cs** → Lee y registra en Dependency Injection
3. **AuthService** → Recibe la configuración inyectada

---

## 🏗️ Flujo de Configuración

### Antes (Acoplado)
```
appsettings.json
    ↓
AuthService → IConfiguration["GoogleAuth:ClientId"]
```

❌ **Problema**: AuthService estaba acoplado a IConfiguration

---

### Después (Desacoplado) ✅
```
appsettings.json
    ↓
Program.cs (Configure<GoogleAuthOptions>)
    ↓
DI Container (IOptions<GoogleAuthOptions>)
    ↓
AuthService → GoogleAuthOptions.ClientId
```

✅ **Ventaja**: AuthService solo conoce `GoogleAuthOptions`, no `IConfiguration`

---

## 📁 Archivos Creados/Modificados

### 1. **GoogleAuthOptions.cs** (NUEVO)
**Ubicación:** `StudyMateAI.Infrastructure/Configuration/GoogleAuthOptions.cs`

```csharp
public class GoogleAuthOptions
{
    public const string SectionName = "GoogleAuth";
    public string ClientId { get; set; } = string.Empty;
    
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(ClientId))
            throw new InvalidOperationException("GoogleAuth:ClientId no está configurado");
    }
}
```

**Responsabilidades:**
- Encapsular configuración de Google Auth
- Validación de valores requeridos
- Documentación clara de propiedades

---

### 2. **Program.cs** (MODIFICADO)
**Cambio clave:**

```csharp
// Registrar GoogleAuthOptions desde appsettings.json
builder.Services.Configure<GoogleAuthOptions>(
    builder.Configuration.GetSection(GoogleAuthOptions.SectionName));
```

**Ubicación en flujo:**
- Después de `AddApplicationServices()` y `AddInfrastructureServices()`
- Antes de crear la aplicación

---

### 3. **AuthService.cs** (MODIFICADO)

**Antes:**
```csharp
public AuthService(
    IUserRepository userRepository,
    IUnitOfWork unitOfWork,
    IConfiguration configuration,
    dbContextStudyMateAI dbContext)
{
    _configuration = configuration;
}

public async Task<(User, string)> AuthenticateWithGoogleAsync(string idToken)
{
    var payload = await GoogleJsonWebSignature.ValidateAsync(idToken, 
        new GoogleJsonWebSignature.ValidationSettings
        {
            Audience = new[] { _configuration["GoogleAuth:ClientId"] }  // ❌ Acoplado
        });
}
```

**Después:**
```csharp
public AuthService(
    IUserRepository userRepository,
    IUnitOfWork unitOfWork,
    IConfiguration configuration,
    IOptions<GoogleAuthOptions> googleAuthOptions,  // ✅ Inyectado
    dbContextStudyMateAI dbContext)
{
    _googleAuthOptions = googleAuthOptions.Value;
    _googleAuthOptions.Validate();  // ✅ Validación temprana
}

public async Task<(User, string)> AuthenticateWithGoogleAsync(string idToken)
{
    var payload = await GoogleJsonWebSignature.ValidateAsync(idToken,
        new GoogleJsonWebSignature.ValidationSettings
        {
            Audience = new[] { _googleAuthOptions.ClientId }  // ✅ Desacoplado
        });
}
```

---

## ✅ Beneficios de esta Arquitectura

| Aspecto | Antes | Después |
|--------|-------|---------|
| **Acoplamiento** | AuthService acoplado a IConfiguration | Desacoplado via GoogleAuthOptions |
| **Testabilidad** | Difícil mockear configuración | Fácil mockear IOptions<GoogleAuthOptions> |
| **Type Safety** | String keys sin validación | Propiedades fuertemente tipadas |
| **Validación** | Sin validación | Validación en constructor |
| **Mantenibilidad** | Strings mágicos esparcidos | Un único lugar (GoogleAuthOptions) |
| **Documentación** | Implícita en código | Explícita en clase dedicada |

---

## 🔧 Cómo Extender para Otros Providers

Ahora es fácil agregar más OAuth providers siguiendo el mismo patrón:

```csharp
// 1. Crear clase de opciones
public class MicrosoftAuthOptions
{
    public const string SectionName = "MicrosoftAuth";
    public string ClientId { get; set; }
    public string ClientSecret { get; set; }
}

// 2. Registrar en Program.cs
builder.Services.Configure<MicrosoftAuthOptions>(
    builder.Configuration.GetSection(MicrosoftAuthOptions.SectionName));

// 3. Usar en servicios
public class MicrosoftAuthService
{
    private readonly MicrosoftAuthOptions _options;
    
    public MicrosoftAuthService(IOptions<MicrosoftAuthOptions> options)
    {
        _options = options.Value;
    }
}
```

---

## 📝 appsettings.json

```json
{
  "GoogleAuth": {
    "ClientId": "519517973496-6qtam58eeshie6g1ig88ublmqfb46kdh.apps.googleusercontent.com"
  },
  "JwtSettings": {
    "Key": "UNA_CLAVE_SECRETA_MUY_LARGA_Y_SEGURA_DE_MINIMO_32_CARACTERES",
    "Issuer": "StudyMateAI",
    "Audience": "StudyMateAI"
  }
}
```

---

## 🧪 Testing Unitario (Ejemplo)

```csharp
[Test]
public async Task AuthenticateWithGoogle_WithValidToken_ReturnsUser()
{
    // Arrange
    var mockOptions = Options.Create(new GoogleAuthOptions 
    { 
        ClientId = "test-client-id.apps.googleusercontent.com" 
    });
    
    var authService = new AuthService(
        mockUserRepository.Object,
        mockUnitOfWork.Object,
        mockConfiguration.Object,
        mockOptions,  // ✅ Fácil de mockear
        mockDbContext.Object);
    
    // Act & Assert
}
```

---

## 🚀 Próximas Mejoras

1. **Almacenamiento seguro**: User Secrets (solo dev)
2. **Environment-specific**: appsettings.Production.json
3. **Validación adicional**: Agregar más propiedades opcionales
4. **Rate limiting**: Agregar configuración de límites
5. **Refresh tokens**: Extender GoogleAuthOptions con duración de tokens

---

## 📞 Resumen de Cambios

✅ Creado: `GoogleAuthOptions.cs`  
✅ Modificado: `Program.cs` - Registrar GoogleAuthOptions  
✅ Modificado: `AuthService.cs` - Usar IOptions<GoogleAuthOptions>  
✅ Build: **SUCCESS** (0 errores)

La arquitectura ahora es **desacoplada, testeable y mantenible**.

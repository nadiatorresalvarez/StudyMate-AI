# 🚀 Guía Rápida - Ejecutar StudyMateAI.Client

## ⚡ Inicio Rápido (3 Pasos)

### Paso 1: Ejecutar el Backend API

```bash
# Terminal 1
cd StudyMateAI
dotnet run
```

✅ El backend estará disponible en: `http://localhost:5071`

### Paso 2: Ejecutar el Cliente Blazor

```bash
# Terminal 2
cd StudyMateAI.Client
dotnet run
```

✅ El cliente se abrirá automáticamente en: `http://localhost:5041`

### Paso 3: Abrir en el Navegador

Si no se abre automáticamente, navega a: **http://localhost:5041**

---

## 🔍 Verificar que Todo Funciona

### 1. Backend API
- Abre: `http://localhost:5071/swagger`
- Deberías ver la documentación de Swagger

### 2. Cliente Blazor
- Abre: `http://localhost:5041`
- Deberías ver la página de inicio o login

---

## ⚙️ Configuración de Puertos

### Cambiar Puerto del Cliente

Edita `StudyMateAI.Client/Properties/launchSettings.json`:

```json
{
  "profiles": {
    "http": {
      "applicationUrl": "http://localhost:TU_PUERTO_AQUI"
    }
  }
}
```

### Cambiar Puerto del Backend

Edita `StudyMateAI/Properties/launchSettings.json`:

```json
{
  "profiles": {
    "http": {
      "applicationUrl": "http://localhost:TU_PUERTO_AQUI"
    }
  }
}
```

**IMPORTANTE**: Si cambias el puerto del backend, también debes actualizar la URL en `StudyMateAI.Client/Program.cs`:

```csharp
var apiUrl = "http://localhost:TU_PUERTO_AQUI";
```

Y actualizar la política CORS en `StudyMateAI/Program.cs`:

```csharp
policy.WithOrigins("http://localhost:PUERTO_DEL_CLIENTE")
```

---

## 🐛 Solución de Problemas Comunes

### Error: "Failed to fetch"

**Causa**: El backend no está ejecutándose o CORS no está configurado.

**Solución**:
1. Verifica que el backend esté corriendo: `http://localhost:5071/swagger`
2. Verifica que los puertos coincidan con la configuración

### Error: "Cannot connect to the server"

**Causa**: La URL del API en `Program.cs` es incorrecta.

**Solución**:
1. Abre `StudyMateAI.Client/Program.cs`
2. Verifica que `apiUrl` apunte al puerto correcto del backend

### La aplicación no carga

**Solución**:
```bash
cd StudyMateAI.Client
dotnet clean
dotnet restore
dotnet build
dotnet run
```

---

## 📋 Checklist Pre-Ejecución

Antes de ejecutar, verifica:

- [ ] .NET 9.0 SDK instalado (`dotnet --version`)
- [ ] Backend API compila sin errores
- [ ] Cliente Blazor compila sin errores
- [ ] Base de datos configurada (si aplica)
- [ ] Variables de entorno configuradas (si aplica)

---

## 🎯 Comandos Útiles

### Limpiar y Reconstruir Todo

```bash
# Backend
cd StudyMateAI
dotnet clean
dotnet restore
dotnet build

# Cliente
cd ../StudyMateAI.Client
dotnet clean
dotnet restore
dotnet build
```

### Ver Versión de .NET

```bash
dotnet --version
```

### Ver Puertos en Uso (Windows)

```powershell
netstat -ano | findstr :5071
netstat -ano | findstr :5041
```

---

## 📚 Documentación Completa

Para más detalles, consulta: `StudyMateAI.Client/README.md`

---

**¿Problemas?** Revisa la sección de Troubleshooting en el README completo.


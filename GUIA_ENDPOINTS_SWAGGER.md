|  |  |  |
| - | - | - |

# Guía Completa: Uso de Endpoints en Swagger - StudyMate AI

## 📋 Tabla de Contenidos

1. [Requisitos Previos](#requisitos-previos)
2. [Acceso a Swagger](#acceso-a-swagger)
3. [Configuración de Autenticación](#configuración-de-autenticación)
4. [Endpoints por Módulo](#endpoints-por-módulo)
5. [Consideraciones Importantes](#consideraciones-importantes)
6. [Flujo de Trabajo Completo](#flujo-de-trabajo-completo)
7. [Solución de Problemas](#solución-de-problemas)

---

## 🔧 Requisitos Previos

### 1. **Base de Datos Configurada**

- La aplicación usa Entity Framework Core con SQL Server (Railway)
- Asegúrate de tener las migraciones aplicadas
- Verifica la conexión en `appsettings.Development.json`

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=hopper.proxy.rlwy.net;Port=43704;Database=railway;User=root;Password=..."
}
```

### 2. **Credenciales de Google OAuth**

- Se requiere un `Google Client ID` válido
- Está configurado en: `appsettings.Development.json` → `GoogleAuth.ClientId`
- Solo se puede autenticar usuarios con cuentas Google registradas

```json
"GoogleAuth": {
  "ClientId": "519517973496-6qtam58eeshie6g1ig88ublmqfb46kdh.apps.googleusercontent.com"
}
```

### 3. **API Gemini Configurada** (para generación de contenido)

- Necesitas una `API Key` de Google Gemini
- Está en: `appsettings.Development.json` → `Gemini.ApiKey`
- Se usa para generar flashcards, quizzes y resúmenes automáticamente

```json
"Gemini": {
  "ApiKey": "AIzaSyAZu_xHD6hg1QnyXI33N6s6O5RzeBTZnQ",
  "Model": "gemini-2.0-flash"
}
```

### 4. **JWT Token** (para autenticación)

- La configuración JWT está en: `appsettings.Development.json` → `JwtSettings`
- La clave debe tener mínimo 32 caracteres
- Los tokens se obtienen después del login con Google

```json
"JwtSettings": {
  "Key": "UNA_CLAVE_SECRETA_MUY_LARGA_Y_SEGURA_DE_MINIMO_32_CARACTERES",
  "Issuer": "StudyMateAI",
  "Audience": "StudyMateAI"
}
```

### 5. **Puerto y Protocolo**

- URL de desarrollo: `http://localhost:5000` o `https://localhost:5001`
- En desarrollo se aceptan peticiones HTTP
- En producción se requiere HTTPS

---

## 📱 Acceso a Swagger

### **Paso 1: Iniciar la Aplicación**

Ejecuta la aplicación desde Visual Studio o con:

```bash
cd StudyMateAI
dotnet run
```

### **Paso 2: Abrir Swagger UI**

La interfaz de Swagger está configurada para abrir automáticamente:

- **URL**: `http://localhost:5000/` (en desarrollo)
- O accede directamente a: `http://localhost:5000/swagger/v1/swagger.json` (JSON)

### **Paso 3: Interfaz Visual**

Verás una interfaz intuitiva con todos los endpoints organizados por controlador:

- **Auth** (Autenticación)
- **Subjects** (Materias)
- **Documents** (Documentos)
- **Flashcards** (Tarjetas de estudio)
- **Quiz** (Pruebas)
- **Study** (Estudio)
- **Summaries** (Resúmenes)

---

## 🔐 Configuración de Autenticación

### **Importante: Todos los endpoints (excepto `/api/auth/google-login`) requieren JWT Token**

### **Paso 1: Obtener Token JWT**

#### Opción A: Login con Google (Recomendado)

**Endpoint**: `POST /api/auth/google-login`

**Request Body**:

```json
{
  "googleIdToken": "eyJhbGciOiJSUzI1NiIsImtpZCI6IjEyMzQ1Njc4OTAiLCJ0eXAiOiJKV1QifQ..."
}
```

**Cómo obtener el Google ID Token:**

1. Usa Google Sign-In en tu aplicación frontend
2. El SDK de Google te proporciona automáticamente el `id_token`
3. Envía ese token a este endpoint

**Response (Éxito - 200)**:

```json
{
  "id": 1,
  "email": "usuario@gmail.com",
  "name": "Juan Pérez",
  "profilePicture": "https://...",
  "jwtToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "optional_token"
}
```

**Response (Error - 401)**:

```json
{
  "message": "Token de Google no válido: Invalid certificate thumbprint"
}
```

### **Paso 2: Configurar Token en Swagger**

1. En la esquina superior derecha de Swagger, haz clic en el botón **"Authorize"** (🔓)
2. Se abrirá un modal con un campo de entrada
3. Copia el JWT token completo (sin "Bearer")
4. Pégalo en el campo de entrada

**Formato correcto en Swagger**:

```
eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c
```

**Token Structure**:

```
Header.Payload.Signature
```

El token incluye:

- `sub` (NameIdentifier) = ID del usuario (necesario para todas las operaciones)
- `email` = Email del usuario
- `exp` = Fecha de expiración

### **Paso 3: Verificar Autorización**

Todos los endpoints excepto `/api/auth/google-login` mostrarán un candado 🔒 en Swagger.

Si el token no está configurado:

- **Response**: 401 Unauthorized
- **Message**: "El esquema de autenticación. ha expirado."

---

## 📚 Endpoints por Módulo

### **1. AUTENTICACIÓN (Auth)**

#### 1.1 Login con Google

- **Método**: `POST`
- **Ruta**: `/api/auth/google-login`
- **Autenticación**: ❌ No requerida
- **Purpose**: Registrar o iniciar sesión un usuario

**Parámetros**:

```json
{
  "googleIdToken": "string (requerido)"
}
```

**Ejemplos de Respuesta**:

✅ **Éxito (200)**:

```json
{
  "id": 1,
  "email": "user@gmail.com",
  "name": "User Name",
  "profilePicture": "https://lh3.googleusercontent.com/...",
  "jwtToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
}
```

❌ **Error (401 - Token Inválido)**:

```json
{
  "message": "Token de Google no válido: Invalid certificate thumbprint"
}
```

---

### **2. MATERIAS (Subjects)**

Todos los endpoints de Subjects requieren JWT token ✅

#### 2.1 Obtener Todas las Materias

- **Método**: `GET`
- **Ruta**: `/api/subjects`
- **Autenticación**: ✅ Requerida
- **Purpose**: Listar todas las materias del usuario (activas y archivadas)

**Query Parameters**: Ninguno

**Response (200)**:

```json
[
  {
    "id": 1,
    "name": "Matemáticas",
    "description": "Álgebra y Cálculo",
    "color": "#FF5733",
    "icon": "calculator",
    "orderIndex": 1,
    "isArchived": false,
    "createdAt": "2024-12-01T10:30:00Z"
  },
  {
    "id": 2,
    "name": "Historia",
    "description": "Historia Moderna",
    "color": "#33FF57",
    "icon": "book",
    "orderIndex": 2,
    "isArchived": false,
    "createdAt": "2024-12-01T10:30:00Z"
  }
]
```

---

#### 2.2 Obtener Materias Activas

- **Método**: `GET`
- **Ruta**: `/api/subjects/active`
- **Autenticación**: ✅ Requerida
- **Purpose**: Listar solo materias **no archivadas**

**Response (200)**:

```json
[
  {
    "id": 1,
    "name": "Matemáticas",
    "description": "Álgebra y Cálculo",
    "color": "#FF5733",
    "icon": "calculator",
    "orderIndex": 1,
    "isArchived": false,
    "createdAt": "2024-12-01T10:30:00Z"
  }
]
```

---

#### 2.3 Obtener Materia por ID

- **Método**: `GET`
- **Ruta**: `/api/subjects/{id}`
- **Autenticación**: ✅ Requerida

**Path Parameters**:

- `id` (int): ID de la materia

**Response (200)**:

```json
{
  "id": 1,
  "name": "Matemáticas",
  "description": "Álgebra y Cálculo",
  "color": "#FF5733",
  "icon": "calculator",
  "orderIndex": 1,
  "isArchived": false,
  "createdAt": "2024-12-01T10:30:00Z"
}
```

**Response (404)**:

```json
{
  "message": "Materia no encontrada"
}
```

---

#### 2.4 Crear Nueva Materia

- **Método**: `POST`
- **Ruta**: `/api/subjects`
- **Autenticación**: ✅ Requerida

**Request Body**:

```json
{
  "name": "Física",
  "description": "Mecánica Clásica",
  "color": "#3366FF",
  "icon": "atom",
  "orderIndex": 3
}
```

**Validaciones**:

- `name`: Requerido, máx 100 caracteres
- `description`: Opcional, máx 500 caracteres
- `color`: Formato hexadecimal (#RRGGBB)
- `icon`: Nombre válido del icono
- `orderIndex`: Número positivo

**Response (201)**:

```json
{
  "id": 3,
  "name": "Física",
  "description": "Mecánica Clásica",
  "color": "#3366FF",
  "icon": "atom",
  "orderIndex": 3,
  "isArchived": false,
  "createdAt": "2024-12-01T11:00:00Z"
}
```

---

#### 2.5 Actualizar Materia

- **Método**: `PUT`
- **Ruta**: `/api/subjects/{id}`
- **Autenticación**: ✅ Requerida

**Path Parameters**:

- `id` (int): ID de la materia

**Request Body**:

```json
{
  "name": "Física Avanzada",
  "description": "Mecánica Cuántica",
  "color": "#3366FF",
  "icon": "atom",
  "orderIndex": 3,
  "isArchived": false
}
```

**Response (200)**:

```json
{
  "id": 3,
  "name": "Física Avanzada",
  "description": "Mecánica Cuántica",
  "color": "#3366FF",
  "icon": "atom",
  "orderIndex": 3,
  "isArchived": false,
  "createdAt": "2024-12-01T11:00:00Z"
}
```

---

#### 2.6 Eliminar Materia

- **Método**: `DELETE`
- **Ruta**: `/api/subjects/{id}`
- **Autenticación**: ✅ Requerida

**Path Parameters**:

- `id` (int): ID de la materia

**Query Parameters**:

- `force` (bool): Si es `true`, elimina incluso si tiene documentos. Defecto: `false`

**Response (200)**:

```json
{
  "message": "Materia eliminada exitosamente"
}
```

**Response (400 - Contiene documentos)**:

```json
{
  "message": "La materia contiene documentos. Use force=true para eliminarla de todas formas."
}
```

---

#### 2.7 Archivar/Desarchivar Materia

- **Método**: `PATCH`
- **Ruta**: `/api/subjects/{id}/archive`
- **Autenticación**: ✅ Requerida

**Path Parameters**:

- `id` (int): ID de la materia

**Request Body**:

```json
{
  "isArchived": true
}
```

**Response (200)**:

```json
{
  "id": 1,
  "name": "Matemáticas",
  "description": "Álgebra y Cálculo",
  "color": "#FF5733",
  "icon": "calculator",
  "orderIndex": 1,
  "isArchived": true,
  "createdAt": "2024-12-01T10:30:00Z"
}
```

---

### **3. DOCUMENTOS (Documents)**

Todos los endpoints de Documents requieren JWT token ✅

#### 3.1 Obtener Todos los Documentos

- **Método**: `GET`
- **Ruta**: `/api/documents`
- **Autenticación**: ✅ Requerida

**Response (200)**:

```json
[
  {
    "id": 1,
    "title": "Capítulo 1 - Introducción",
    "originalFileName": "capitulo1.pdf",
    "content": "Lorem ipsum dolor sit amet...",
    "status": "Completed",
    "subjectId": 1,
    "uploadedAt": "2024-12-01T14:30:00Z",
    "processedAt": "2024-12-01T14:45:00Z",
    "fileSize": 2048576
  }
]
```

---

#### 3.2 Obtener Documento por ID

- **Método**: `GET`
- **Ruta**: `/api/documents/{id}`
- **Autenticación**: ✅ Requerida

**Path Parameters**:

- `id` (int): ID del documento

**Response (200)**:

```json
{
  "id": 1,
  "title": "Capítulo 1 - Introducción",
  "originalFileName": "capitulo1.pdf",
  "content": "Lorem ipsum dolor sit amet...",
  "status": "Completed",
  "subjectId": 1,
  "uploadedAt": "2024-12-01T14:30:00Z",
  "processedAt": "2024-12-01T14:45:00Z",
  "fileSize": 2048576
}
```

---

#### 3.3 Obtener Documentos por Materia

- **Método**: `GET`
- **Ruta**: `/api/documents/subject/{subjectId}`
- **Autenticación**: ✅ Requerida

**Path Parameters**:

- `subjectId` (int): ID de la materia

**Response (200)**:

```json
[
  {
    "id": 1,
    "title": "Capítulo 1",
    "status": "Completed",
    "subjectId": 1,
    "uploadedAt": "2024-12-01T14:30:00Z"
  },
  {
    "id": 2,
    "title": "Capítulo 2",
    "status": "Pending",
    "subjectId": 1,
    "uploadedAt": "2024-12-01T15:00:00Z"
  }
]
```

---

#### 3.4 Obtener Documentos por Estado

- **Método**: `GET`
- **Ruta**: `/api/documents/status/{status}`
- **Autenticación**: ✅ Requerida

**Path Parameters**:

- `status` (string): Estados válidos: `Pending`, `Completed`, `Failed`

**Response (200)**:

```json
[
  {
    "id": 1,
    "title": "Capítulo 1",
    "status": "Completed",
    "subjectId": 1,
    "uploadedAt": "2024-12-01T14:30:00Z"
  }
]
```

---

#### 3.5 Crear Documento con URL Pública

- **Método**: `POST`
- **Ruta**: `/api/documents`
- **Autenticación**: ✅ Requerida
- **Purpose**: Crear un documento referenciando una URL pública (no descarga el archivo)

**Request Body** (Todos los campos requeridos):

```json
{
  "fileName": "documento_procesado",
  "originalFileName": "documento_original.pdf",
  "fileType": "PDF",
  "fileUrl": "https://ejemplo.com/documentos/archivo.pdf",
  "subjectId": 1,
  "extractedText": "Contenido extraído del documento (opcional)",
  "fileSizeKb": 2048,
  "pageCount": 10,
  "language": "es"
}
```

**Parámetros Requeridos**:

- `fileName` (string, 1-255 caracteres): Nombre del documento en el sistema
- `originalFileName` (string, 1-255 caracteres): Nombre original del archivo
- `fileType` (string): Solo valores permitidos: `PDF`, `DOCX`, `PPTX`, `TXT`
- `fileUrl` (string): URL válida y pública del archivo
- `subjectId` (int): ID de la materia (debe existir y pertenecer al usuario)

**Parámetros Opcionales**:

- `extractedText` (string): Texto ya extraído del documento
- `fileSizeKb` (int): Tamaño del archivo en KB
- `pageCount` (int): Número de páginas (para PDF)
- `language` (string): Código de idioma (ej: "es", "en", "fr")

**Validaciones**:

- ✅ La materia debe existir y pertenecer al usuario
- ✅ `fileType` debe ser uno de los 4 tipos permitidos
- ✅ `fileUrl` debe ser una URL válida y accesible públicamente
- ✅ `fileSizeKb` y `pageCount` deben ser > 0 si se especifican

**Response (201 - Éxito)**:

```json
{
  "id": 5,
  "fileName": "documento_procesado",
  "originalFileName": "documento_original.pdf",
  "fileType": "PDF",
  "fileUrl": "https://ejemplo.com/documentos/archivo.pdf",
  "subjectId": 1,
  "content": null,
  "processingStatus": "Pending",
  "uploadedAt": "2024-12-01T16:00:00Z",
  "processedAt": null
}
```

**Response (400 - Validación fallida)**:

```json
{
  "errors": {
    "fileType": ["Tipo de archivo no válido. Debe ser: PDF, DOCX, PPTX o TXT"],
    "fileUrl": ["La URL del archivo no es válida"]
  }
}
```

**Response (400 - Materia no existe)**:

```json
{
  "message": "La materia no existe o no pertenece al usuario"
}
```

**Ejemplo Completo - cURL**:

```bash
curl -X POST http://localhost:5000/api/documents \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "fileName": "calculus_chapter_5",
    "originalFileName": "Cap5_Derivadas.pdf",
    "fileType": "PDF",
    "fileUrl": "https://cdn.ejemplo.com/calculus/chapter5.pdf",
    "subjectId": 1,
    "extractedText": "En este capítulo estudiamos las derivadas...",
    "fileSizeKb": 3072,
    "pageCount": 25,
    "language": "es"
  }'
```

---

#### 3.6 Subir Archivo Local (Multipart)

- **Método**: `POST`
- **Ruta**: `/api/documents/upload`
- **Autenticación**: ✅ Requerida
- **Content-Type**: `multipart/form-data`
- **Límite de tamaño**: 20 MB

**Form Parameters**:

- `file` (file): Archivo a subir (PDF, Word, TXT, etc.)
- `subjectId` (int): ID de la materia

**Pasos en Swagger**:

1. Haz clic en el botón "Try it out"
2. Selecciona un archivo con el botón "Choose File"
3. Ingresa el `subjectId`
4. Haz clic en "Execute"

**Response (201)**:

```json
{
  "id": 4,
  "title": "documento_subido.pdf",
  "originalFileName": "documento_subido.pdf",
  "status": "Pending",
  "subjectId": 1,
  "uploadedAt": "2024-12-01T16:30:00Z",
  "fileSize": 5242880
}
```

**Response (400 - Archivo no válido)**:

```json
{
  "message": "El archivo debe ser PDF, Word o TXT"
}
```

---

#### 3.7 Actualizar Documento

- **Método**: `PUT`
- **Ruta**: `/api/documents/{id}`
- **Autenticación**: ✅ Requerida

**Path Parameters**:

- `id` (int): ID del documento

**Request Body**:

```json
{
  "title": "Título actualizado",
  "content": "Contenido actualizado...",
  "subjectId": 1
}
```

**Response (200)**:

```json
{
  "id": 1,
  "title": "Título actualizado",
  "content": "Contenido actualizado...",
  "status": "Completed",
  "subjectId": 1
}
```

---

#### 3.8 Actualizar Estado de Procesamiento

- **Método**: `PATCH`
- **Ruta**: `/api/documents/{id}/processing-status`
- **Autenticación**: ✅ Requerida

**Path Parameters**:

- `id` (int): ID del documento

**Request Body**:

```json
{
  "status": "Completed"
}
```

**Estados válidos**: `Pending`, `Completed`, `Failed`

**Response (200)**:

```json
{
  "id": 1,
  "title": "Capítulo 1",
  "status": "Completed",
  "processedAt": "2024-12-01T16:45:00Z"
}
```

---

#### 3.9 Eliminar Documento

- **Método**: `DELETE`
- **Ruta**: `/api/documents/{id}`
- **Autenticación**: ✅ Requerida

**Path Parameters**:

- `id` (int): ID del documento

**Response (200)**:

```json
{
  "message": "Documento eliminado exitosamente"
}
```

---

## 📊 Comparativa: Dos Formas de Crear Documentos

### **Opción 1: URL Pública (POST /api/documents)**

| Aspecto                       | Detalles                                                |
| ----------------------------- | ------------------------------------------------------- |
| **Cuándo usar**        | Cuando tienes el archivo en un servidor público o CDN  |
| **Requisitos**          | URL pública y accesible del archivo                    |
| **Tipo de archivo**     | PDF, DOCX, PPTX, TXT (especificar tipo explícitamente) |
| **Límite de tamaño**  | Sin límite técnico (depende del servidor remoto)      |
| **Tiempo de respuesta** | Instantáneo (no descarga el archivo)                   |
| **Almacenamiento**      | No descarga localmente, solo guarda metadatos           |
| **Mejor para**          | Archivos grandes, referencias externas, automatización |

**Ejemplo de uso**:

```bash
# Si tienes documentos en Google Drive, OneDrive, o un CDN
POST /api/documents
{
  "fileName": "apuntes",
  "originalFileName": "apuntes_completos.pdf",
  "fileType": "PDF",
  "fileUrl": "https://drive.google.com/uc?export=download&id=1ABC123...",
  "subjectId": 1,
  "fileSizeKb": 5120,
  "pageCount": 50,
  "language": "es"
}
```

---

### **Opción 2: Archivo Local (POST /api/documents/upload)**

| Aspecto                       | Detalles                                           |
| ----------------------------- | -------------------------------------------------- |
| **Cuándo usar**        | Cuando subes archivos desde tu computadora         |
| **Requisitos**          | Archivo físico en tu máquina local               |
| **Tipo de archivo**     | PDF, DOCX, PPTX, TXT (se detecta automáticamente) |
| **Límite de tamaño**  | Máximo 20 MB                                      |
| **Tiempo de respuesta** | Depende del tamaño del archivo                    |
| **Almacenamiento**      | Se descarga y procesa localmente                   |
| **Mejor para**          | Uso manual en Swagger, pruebas rápidas            |

**Ejemplo de uso**:

```bash
# Subir un archivo desde tu máquina
POST /api/documents/upload
FormData:
  - file: [archivo.pdf de tu carpeta Downloads]
  - subjectId: 1
```

---

### **Comparativa Lado a Lado**

```
┌─────────────────────────────────────────────────────────────────────┐
│                    CREAR DOCUMENTO                                  │
├─────────────────────────────────────────────────────────────────────┤
│                                                                       │
│  Opción 1: URL Pública              Opción 2: Archivo Local         │
│  ───────────────────────            ─────────────────────           │
│  POST /api/documents                POST /api/documents/upload      │
│                                                                       │
│  Requiere:                          Requiere:                       │
│  ✓ URL pública válida               ✓ Archivo en computadora        │
│  ✓ Tipo de archivo explícito        ✓ Tamaño < 20 MB              │
│  ✓ Metadatos opcionales             ✓ Tipos: PDF, DOCX, PPTX, TXT │
│                                                                       │
│  Ventajas:                          Ventajas:                       │
│  • Sin límite de tamaño             • Interfaz gráfica en Swagger   │
│  • Automático/API-friendly          • Detección automática de tipo   │
│  • Almacenamiento remoto            • Procesamiento en servidor      │
│  • Ideal para automatización        • Ideal para pruebas manuales    │
│                                                                       │
│  Desventajas:                       Desventajas:                    │
│  • Requiere URL pública             • Límite de 20 MB               │
│  • No descarga el contenido         • Más lento (sube el archivo)    │
│  • Metadata manual                  • No automatizable desde frontend│
│                                                                       │
└─────────────────────────────────────────────────────────────────────┘
```

---

### **¿Cuál elegir?**

**Elige URL Pública si**:

```
✓ Tienes un servidor o CDN con tus documentos
✓ Quieres una API automatizada
✓ Los archivos son muy grandes (> 20 MB)
✓ Integras con Google Drive, OneDrive, etc.
```

**Elige Archivo Local si**:

```
✓ Estás probando en Swagger UI
✓ Archivos pequeños (< 20 MB)
✓ Necesitas interfaz gráfica
✓ El archivo está en tu computadora
```

---

### **Flujo Recomendado por Escenario**

**Escenario A: Testing Manual en Swagger**

```
1. Descarga un PDF de ejemplo
2. Crea una materia (POST /api/subjects)
3. Sube el PDF (POST /api/documents/upload)
4. Espera procesamiento (GET /api/documents/{id})
5. Genera flashcards (POST /api/flashcards/generate/{documentId})
```

**Escenario B: Integración con Frontend Web**

```
1. Usuario selecciona archivo en el navegador
2. Frontend sube a tu servidor/CDN
3. Obtiene URL pública del archivo
4. Llamada API: POST /api/documents con FileUrl
5. Sistema registra el documento
```

**Escenario C: Automatización Backend**

```
1. Sistema genera PDF automáticamente
2. Lo sube a Google Drive / Azure / S3
3. Obtiene URL pública
4. POST /api/documents con metadata
5. Sin intervención manual
```

---

### **4. FLASHCARDS (Tarjetas de Estudio)**

Todos los endpoints de Flashcards requieren JWT token ✅

#### 4.1 Generar Flashcards desde Documento

- **Método**: `POST`
- **Ruta**: `/api/flashcards/generate/{documentId}`
- **Autenticación**: ✅ Requerida
- **API Requerida**: Gemini API

**Path Parameters**:

- `documentId` (int): ID del documento a procesar

**Request Body**: Vacío (GET automático del contenido)

**Response (200)**:

```json
[
  {
    "id": 1,
    "documentId": 1,
    "question": "¿Qué es el álgebra?",
    "answer": "Es la rama de la matemática que estudia...",
    "difficulty": "Easy",
    "createdAt": "2024-12-01T17:00:00Z"
  },
  {
    "id": 2,
    "documentId": 1,
    "question": "¿Cuál es la fórmula cuadrática?",
    "answer": "x = (-b ± √(b²-4ac)) / 2a",
    "difficulty": "Medium",
    "createdAt": "2024-12-01T17:00:00Z"
  }
]
```

**Response (403 - No autorizado)**:

```json
{
  "message": "No tienes permisos para acceder a este documento"
}
```

**Response (400 - Error en generación)**:

```json
{
  "message": "Error procesando el documento con Gemini"
}
```

---

#### 4.2 Crear Flashcard Manual

- **Método**: `POST`
- **Ruta**: `/api/flashcards/{documentId}`
- **Autenticación**: ✅ Requerida

**Path Parameters**:

- `documentId` (int): ID del documento

**Request Body**:

```json
{
  "question": "¿Cuál es la capital de Francia?",
  "answer": "París",
  "difficulty": "Easy"
}
```

**Response (200)**:

```json
{
  "id": 3,
  "documentId": 1,
  "question": "¿Cuál es la capital de Francia?",
  "answer": "París",
  "difficulty": "Easy",
  "createdAt": "2024-12-01T17:30:00Z"
}
```

---

#### 4.3 Actualizar Flashcard

- **Método**: `PUT`
- **Ruta**: `/api/flashcards/{flashcardId}`
- **Autenticación**: ✅ Requerida

**Path Parameters**:

- `flashcardId` (int): ID de la flashcard

**Request Body**:

```json
{
  "question": "¿Cuál es la capital de España?",
  "answer": "Madrid",
  "difficulty": "Easy"
}
```

**Response (200)**:

```json
{
  "id": 3,
  "documentId": 1,
  "question": "¿Cuál es la capital de España?",
  "answer": "Madrid",
  "difficulty": "Easy",
  "updatedAt": "2024-12-01T17:45:00Z"
}
```

---

#### 4.4 Eliminar Flashcard

- **Método**: `DELETE`
- **Ruta**: `/api/flashcards/{flashcardId}`
- **Autenticación**: ✅ Requerida

**Path Parameters**:

- `flashcardId` (int): ID de la flashcard

**Response (200)**:

```json
{
  "message": "Flashcard eliminada correctamente"
}
```

---

#### 4.5 Registrar Revisión de Flashcard

- **Método**: `POST`
- **Ruta**: `/api/flashcards/review/{flashcardId}`
- **Autenticación**: ✅ Requerida
- **Purpose**: Registrar el desempeño del usuario al revisar una flashcard

**Path Parameters**:

- `flashcardId` (int): ID de la flashcard

**Request Body**:

```json
{
  "quality": 4
}
```

**Calidad**: Rango 0-5 (SM-2 algorithm)

- 0: No reconocer (muy difícil)
- 1: Reconocer con dificultad
- 2: Correcta con vacilación
- 3: Correcta con esfuerzo
- 4: Correcta fácilmente
- 5: Correcta instantáneamente

**Response (200)**:

```json
{
  "message": "Review registrada correctamente"
}
```

---

### **5. QUIZZES (Pruebas)**

Todos los endpoints de Quiz requieren JWT token ✅

#### 5.1 Generar Quiz desde Documento

- **Método**: `POST`
- **Ruta**: `/api/quiz/generate/{documentId}`
- **Autenticación**: ✅ Requerida
- **API Requerida**: Gemini API

**Path Parameters**:

- `documentId` (int): ID del documento

**Request Body**:

```json
{
  "questionCount": 10,
  "difficulty": "Medium"
}
```

**Parámetros**:

- `questionCount`: 5-50 preguntas
- `difficulty`: `Easy`, `Medium`, `Hard`

**Response (200)**:

```json
{
  "id": 1,
  "documentId": 1,
  "title": "Quiz: Capítulo 1",
  "questionCount": 10,
  "difficulty": "Medium",
  "questions": [
    {
      "id": 1,
      "question": "¿Qué es el álgebra?",
      "options": [
        "Una rama de matemática",
        "Un idioma",
        "Una ciencia",
        "Un arte"
      ],
      "correctAnswer": 0,
      "explanation": "El álgebra estudia..."
    }
  ],
  "createdAt": "2024-12-01T18:00:00Z"
}
```

---

#### 5.2 Obtener Quiz para Intento

- **Método**: `GET`
- **Ruta**: `/api/quiz/{quizId}/for-attempt`
- **Autenticación**: ✅ Requerida
- **Purpose**: Obtener el quiz sin las respuestas correctas (para mostrar al usuario)

**Path Parameters**:

- `quizId` (int): ID del quiz

**Response (200)**:

```json
{
  "id": 1,
  "documentId": 1,
  "title": "Quiz: Capítulo 1",
  "questionCount": 10,
  "difficulty": "Medium",
  "questions": [
    {
      "id": 1,
      "question": "¿Qué es el álgebra?",
      "options": [
        "Una rama de matemática",
        "Un idioma",
        "Una ciencia",
        "Un arte"
      ]
    }
  ]
}
```

---

#### 5.3 Enviar Intento de Quiz

- **Método**: `POST`
- **Ruta**: `/api/quiz/{quizId}/attempts`
- **Autenticación**: ✅ Requerida

**Path Parameters**:

- `quizId` (int): ID del quiz

**Request Body**:

```json
{
  "answers": [
    {
      "questionId": 1,
      "selectedOption": 0
    },
    {
      "questionId": 2,
      "selectedOption": 2
    }
  ]
}
```

**Response (200)**:

```json
{
  "attemptId": 1
}
```

---

#### 5.4 Evaluar Intento de Quiz

- **Método**: `POST`
- **Ruta**: `/api/quiz/attempts/{attemptId}/evaluate`
- **Autenticación**: ✅ Requerida
- **Purpose**: Calcular puntuación y comparar respuestas

**Path Parameters**:

- `attemptId` (int): ID del intento

**Request Body**: Vacío

**Response (200)**:

```json
{
  "attemptId": 1,
  "quizId": 1,
  "score": 8,
  "totalQuestions": 10,
  "percentage": 80,
  "correct": 8,
  "incorrect": 2,
  "startedAt": "2024-12-01T18:05:00Z",
  "completedAt": "2024-12-01T18:15:00Z",
  "detailedResults": [
    {
      "questionId": 1,
      "question": "¿Qué es el álgebra?",
      "userAnswer": 0,
      "correctAnswer": 0,
      "isCorrect": true,
      "explanation": "El álgebra estudia..."
    }
  ]
}
```

---

#### 5.5 Obtener Resultado de Intento

- **Método**: `GET`
- **Ruta**: `/api/quiz/attempts/{attemptId}`
- **Autenticación**: ✅ Requerida

**Path Parameters**:

- `attemptId` (int): ID del intento

**Response (200)**:

```json
{
  "attemptId": 1,
  "quizId": 1,
  "score": 8,
  "totalQuestions": 10,
  "percentage": 80,
  "detailedResults": [...]
}
```

---

#### 5.6 Obtener Historial de Quiz

- **Método**: `GET`
- **Ruta**: `/api/quiz/attempts/history`
- **Autenticación**: ✅ Requerida

**Query Parameters** (Opcionales):

- `documentId` (int): Filtrar por documento
- `quizId` (int): Filtrar por quiz

**Response (200)**:

```json
{
  "totalAttempts": 5,
  "averageScore": 78.5,
  "attempts": [
    {
      "attemptId": 1,
      "quizId": 1,
      "score": 8,
      "totalQuestions": 10,
      "percentage": 80,
      "completedAt": "2024-12-01T18:15:00Z"
    },
    {
      "attemptId": 2,
      "quizId": 1,
      "score": 9,
      "totalQuestions": 10,
      "percentage": 90,
      "completedAt": "2024-12-02T10:00:00Z"
    }
  ]
}
```

---

## ⚠️ Consideraciones Importantes

### **1. Autenticación JWT**

- **Expiración**: Verifica la fecha de expiración en el token
- **Renovación**: Si expira, debes hacer login nuevamente
- **Scope**: El token contiene el `userId`, se usa automáticamente en todas las peticiones

### **2. Límites de Recursos**

| Recurso         | Límite     | Nota                                |
| --------------- | ----------- | ----------------------------------- |
| Tamaño archivo | 20 MB       | Configurado en `RequestSizeLimit` |
| Materias        | Sin límite | Pero recomendado < 50               |
| Documentos      | Sin límite | Considera el almacenamiento         |
| Flashcards      | Sin límite | Por documento                       |
| Preguntas Quiz  | 5-50        | Validado en request                 |

### **3. Validaciones Previas**

Antes de usar ciertos endpoints:

**Para subir documentos**:

- ✅ Usuario autenticado (JWT válido)
- ✅ Materia debe existir y pertenecer al usuario
- ✅ Archivo < 20 MB
- ✅ Formato válido (PDF, Word, TXT)

**Para generar Flashcards/Quiz**:

- ✅ Usuario autenticado
- ✅ Documento debe existir y pertenecer al usuario
- ✅ Documento debe tener contenido
- ✅ API Gemini debe estar disponible

**Para eliminar materia**:

- ✅ Usuario autenticado
- ✅ Sin documentos (a menos que uses `force=true`)

### **4. Errores Comunes y Soluciones**

| Error               | Causa                           | Solución                        |
| ------------------- | ------------------------------- | -------------------------------- |
| 401 Unauthorized    | Token faltante o expirado       | Actualiza el token en Authorize  |
| 403 Forbidden       | Recurso no pertenece al usuario | Verifica que uses tu propio ID   |
| 404 Not Found       | Recurso no existe               | Verifica el ID del recurso       |
| 400 Bad Request     | Validación fallida             | Revisa el cuerpo de la petición |
| 500 Internal Server | Error en la API                 | Revisa los logs del servidor     |

### **5. Orden de Operaciones Recomendado**

1. **Login**

   ```
   POST /api/auth/google-login
   ```
2. **Crear Materia**

   ```
   POST /api/subjects
   ```
3. **Subir Documento**

   ```
   POST /api/documents/upload
   ```
4. **Generar Flashcards o Quiz**

   ```
   POST /api/flashcards/generate/{documentId}
   POST /api/quiz/generate/{documentId}
   ```
5. **Revisar Flashcards**

   ```
   POST /api/flashcards/review/{flashcardId}
   ```
6. **Hacer Quiz**

   ```
   POST /api/quiz/{quizId}/attempts
   POST /api/quiz/attempts/{attemptId}/evaluate
   ```

---

## 🎯 Flujo de Trabajo Completo

### **Escenario: Crear un curso y estudiar**

#### **Paso 1: Autenticación**

```bash
1. Abre Swagger en http://localhost:5000
2. Click en "Authorize" (arriba a la derecha)
3. Obtén un Google ID Token de tu aplicación frontend
4. Pégalo en el modal de Authorize
5. Click en "Authorize"
```

#### **Paso 2: Crear Materia**

```bash
POST /api/subjects
{
  "name": "Cálculo I",
  "description": "Límites, Derivadas e Integrales",
  "color": "#2E86AB",
  "icon": "function",
  "orderIndex": 1
}
```

**Respuesta esperada**: ID de la materia creada (ej: `id: 1`)

#### **Paso 3: Subir Documento**

```bash
POST /api/documents/upload
Parámetros:
- file: [selecciona un PDF o Word]
- subjectId: 1 [ID de la materia creada]
```

**Respuesta esperada**: ID del documento (ej: `id: 1`, `status: "Pending"`)

#### **Paso 4: Esperar Procesamiento**

```bash
GET /api/documents/1
```

Espera hasta que `status` sea `"Completed"`

#### **Paso 5: Generar Flashcards**

```bash
POST /api/flashcards/generate/1
```

**Respuesta**: Lista de flashcards generadas automáticamente

#### **Paso 6: Revisar Flashcards**

```bash
POST /api/flashcards/review/1
{
  "quality": 4
}
```

#### **Paso 7: Generar Quiz**

```bash
POST /api/quiz/generate/1
{
  "questionCount": 10,
  "difficulty": "Medium"
}
```

**Respuesta**: Quiz con 10 preguntas

#### **Paso 8: Hacer el Quiz**

```bash
1. GET /api/quiz/1/for-attempt
   [Obtiene las preguntas sin respuestas]

2. POST /api/quiz/1/attempts
   {
     "answers": [
       {"questionId": 1, "selectedOption": 0},
       ...
     ]
   }
   [Retorna attemptId: 1]

3. POST /api/quiz/attempts/1/evaluate
   [Calcula la puntuación]
```

**Respuesta**: Resultado con puntuación y explicaciones

---

## 🔍 Solución de Problemas

### **Problema: 401 Unauthorized en todos los endpoints**

**Síntomas**:

```json
{
  "message": "El esquema de autenticación. ha expirado."
}
```

**Soluciones**:

1. Abre Swagger nuevamente
2. Click en "Authorize"
3. Verifica que el token esté pegado sin "Bearer"
4. Si sigue sin funcionar, obtén un nuevo token con `POST /api/auth/google-login`

---

### **Problema: 404 en GET /api/subjects/1**

**Síntomas**: "Materia no encontrada"

**Soluciones**:

1. Verifica que la materia exista: `GET /api/subjects`
2. Usa uno de los IDs de la lista
3. Asegúrate de que la materia pertenezca a tu usuario

---

### **Problema: Error al generar Flashcards/Quiz**

**Síntomas**:

```json
{
  "message": "Error procesando el documento con Gemini"
}
```

**Soluciones**:

1. Verifica que `Gemini.ApiKey` sea válida en `appsettings.Development.json`
2. Verifica que el documento tenga contenido
3. Revisa los logs de la aplicación
4. Intenta con un documento más pequeño primero

---

### **Problema: 413 Payload Too Large al subir archivo**

**Síntomas**: El archivo es demasiado grande

**Soluciones**:

1. El límite es 20 MB
2. Comprime el archivo antes
3. Divide el contenido en varios documentos

---

### **Problema: Token de Google inválido en login**

**Síntomas**:

```json
{
  "message": "Token de Google no válido: Invalid certificate thumbprint"
}
```

**Soluciones**:

1. Verifica que uses un ID Token válido (no access token)
2. El ID Token no debe estar expirado
3. Verifica que el `ClientId` en `appsettings.Development.json` sea correcto
4. Obtén un nuevo token de Google

---

## 📝 Checklist Antes de Probar

- [X] Aplicación ejecutándose (`dotnet run`)
- [X] Base de datos conectada y accesible
- [X] Swagger abierto en `http://localhost:5000`
- [X] Google OAuth Client ID válido
- [X] Gemini API Key válida
- [X] JWT Token obtenido y configurado en Authorize
- [X] Materias creadas
- [X] Documentos subidos
- [X] Documentos procesados (status = "Completed")

---

## 🚀 Recursos Adicionales

- **Documentación Swagger**: `http://localhost:5000/swagger/v1/swagger.json`
- **Configuración**: `appsettings.Development.json`
- **Migraciones**: `initial_migration.sql`
- **Controllers**: `/StudyMateAI/Controllers/`

---

**Última actualización**: 1 de Diciembre de 2024
**Versión API**: 1.0

# 📚 Documentación Completa de Endpoints - StudyMate AI API

**Versión:** 1.0.0  
**Última actualización:** Diciembre 2024  
**Base URL (Desarrollo):** `http://localhost:5000`  
**Base URL (Producción):** `https://api.studymateai.com`

---

## 📋 Tabla de Contenidos

1. [Configuración Requerida](#configuración-requerida)
2. [Autenticación](#autenticación)
3. [Endpoints por Módulo](#endpoints-por-módulo)
   - [Auth (Autenticación)](#módulo-auth)
   - [Subjects (Materias)](#módulo-subjects)
   - [Documents (Documentos)](#módulo-documents)
   - [Summaries (Resúmenes)](#módulo-summaries)
   - [Flashcards (Tarjetas de Estudio)](#módulo-flashcards)
   - [Quiz (Cuestionarios)](#módulo-quiz)
   - [Study (Estudio)](#módulo-study)
   - [Maps (Mapas Conceptuales y Mentales)](#módulo-maps)
   - [Profile (Perfil)](#módulo-profile)
4. [Códigos de Estado HTTP](#códigos-de-estado-http)
5. [Manejo de Errores](#manejo-de-errores)
6. [Consumo desde Cliente Blazor WASM](#consumo-desde-cliente-blazor-wasm)
7. [Mejores Prácticas](#mejores-prácticas)

---

## 🔧 Configuración Requerida

### 1. **Variables de Entorno (appsettings.json)**

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Port=3309;Database=studymateai;User=root;Password=...;"
  },
  "GoogleAuth": {
    "ClientId": "tu-google-client-id.apps.googleusercontent.com"
  },
  "Gemini": {
    "ApiKey": "tu-api-key-de-gemini",
    "Model": "gemini-2.0-flash"
  },
  "JwtSettings": {
    "Key": "tu-clave-secreta-de-minimo-32-caracteres",
    "Issuer": "StudyMateAI",
    "Audience": "StudyMateAI"
  },
  "ReportSettings": {
    "LogoPath": "images/logo-studymate.png",
    "WatermarkOpacity": 0.15
  }
}
```

### 2. **Configuración CORS en el Cliente (Blazor)**

El servidor está configurado para aceptar peticiones desde `http://localhost:5041` (Puerto del cliente Blazor WASM).

**Política de CORS Configurada:**
- **Origen:** `http://localhost:5041`
- **Headers permitidos:** Todos
- **Métodos permitidos:** GET, POST, PUT, DELETE, PATCH
- **Credenciales:** Sí (importante para JWT)

### 3. **Swagger/OpenAPI**

Acceso a la documentación interactiva:
- **URL:** `http://localhost:5000` (en desarrollo)
- **Swagger UI:** Se abre automáticamente en la raíz
- **Especificación OpenAPI:** `/swagger/v1/swagger.json`

---

## 🔐 Autenticación

### Modelo de Autenticación: JWT Bearer Token

Todos los endpoints (excepto `/api/auth/google-login`) requieren autenticación JWT.

### Flujo de Autenticación

```
1. Cliente obtiene Google ID Token (Google OAuth 2.0)
   ↓
2. Cliente envía Google ID Token al endpoint POST /api/auth/google-login
   ↓
3. Servidor valida el token de Google
   ↓
4. Servidor genera un JWT Token y lo retorna
   ↓
5. Cliente almacena el JWT Token en localStorage
   ↓
6. En cada petición, cliente envía el JWT en el header Authorization
```

### Envío del JWT en Headers

```http
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

### Implementación en Cliente Blazor

```csharp
// En el servicio HTTP (HttpClient con interceptor)
public async Task<T> GetAsync<T>(string url)
{
    var token = await localStorageService.GetItemAsStringAsync("jwtToken");
    
    var request = new HttpRequestMessage(HttpMethod.Get, url);
    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
    
    var response = await httpClient.SendAsync(request);
    response.EnsureSuccessStatusCode();
    
    return await response.Content.ReadAsAsync<T>();
}
```

---

## 📡 Endpoints por Módulo

---

## MÓDULO AUTH

### POST `/api/auth/google-login`

**Descripción:** Inicia sesión o registra un nuevo usuario usando Google OAuth.

**Autenticación:** ❌ No requerida

**Request Body:**
```json
{
  "googleIdToken": "eyJhbGciOiJSUzI1NiIsImtpZCI6IjEyMyIsInR5cCI6IkpXVCJ9..."
}
```

**Parámetros:**
| Campo | Tipo | Requerido | Descripción |
|-------|------|-----------|-------------|
| `googleIdToken` | String | ✅ Sí | ID Token obtenido de Google OAuth |

**Response 200 OK:**
```json
{
  "jwtToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "user": {
    "id": 1,
    "name": "Juan Pérez",
    "email": "juan@example.com",
    "profilePicture": "https://example.com/photo.jpg",
    "educationLevel": "University"
  }
}
```

**Response 400 Bad Request:**
```json
{
  "message": "Token de Google es requerido."
}
```

**Response 401 Unauthorized:**
```json
{
  "message": "Token de Google no válido: Invalid token"
}
```

**Response 500 Internal Server Error:**
```json
{
  "message": "Error interno: ...",
  "details": "..."
}
```

**Casos de Uso:**
- ✅ Registro de nuevos usuarios con Google
- ✅ Login de usuarios existentes
- ✅ Refresh de sesión

**Ejemplo cURL:**
```bash
curl -X POST http://localhost:5000/api/auth/google-login \
  -H "Content-Type: application/json" \
  -d '{"googleIdToken":"eyJhbGciOiJSUzI1NiIsImtpZCI6IjEyMyIsInR5cCI6IkpXVCJ9..."}'
```

---

## MÓDULO SUBJECTS

**Autenticación:** ✅ Requerida (JWT Bearer Token)

### GET `/api/subjects`

**Descripción:** Obtiene todas las materias del usuario (activas y archivadas).

**Response 200 OK:**
```json
[
  {
    "id": 1,
    "name": "Matemáticas",
    "description": "Cálculo y álgebra",
    "color": "#3B82F6",
    "icon": "📐",
    "orderIndex": 0,
    "isArchived": false,
    "createdAt": "2024-12-01T10:30:00Z",
    "documentCount": 5
  },
  {
    "id": 2,
    "name": "Historia",
    "description": "Historia mundial",
    "color": "#F59E0B",
    "icon": "📚",
    "orderIndex": 1,
    "isArchived": true,
    "createdAt": "2024-11-15T14:20:00Z",
    "documentCount": 3
  }
]
```

---

### GET `/api/subjects/active`

**Descripción:** Obtiene solo las materias activas (no archivadas).

**Response 200 OK:**
```json
[
  {
    "id": 1,
    "name": "Matemáticas",
    "description": "Cálculo y álgebra",
    "color": "#3B82F6",
    "icon": "📐",
    "orderIndex": 0,
    "isArchived": false,
    "createdAt": "2024-12-01T10:30:00Z",
    "documentCount": 5
  }
]
```

---

### GET `/api/subjects/{id}`

**Descripción:** Obtiene una materia específica por ID.

**Parámetros de Ruta:**
| Parámetro | Tipo | Descripción |
|-----------|------|-------------|
| `id` | Integer | ID de la materia |

**Response 200 OK:**
```json
{
  "id": 1,
  "name": "Matemáticas",
  "description": "Cálculo y álgebra",
  "color": "#3B82F6",
  "icon": "📐",
  "orderIndex": 0,
  "isArchived": false,
  "createdAt": "2024-12-01T10:30:00Z",
  "documentCount": 5
}
```

**Response 404 Not Found:**
```json
{
  "message": "Materia no encontrada"
}
```

---

### POST `/api/subjects`

**Descripción:** Crea una nueva materia.

**Request Body:**
```json
{
  "name": "Biología",
  "description": "Estudio de organismos vivos",
  "color": "#10B981",
  "icon": "🧬"
}
```

**Parámetros:**
| Campo | Tipo | Requerido | Validación | Descripción |
|-------|------|-----------|-----------|-------------|
| `name` | String | ✅ Sí | 3-100 caracteres | Nombre de la materia |
| `description` | String | ❌ No | máx 500 caracteres | Descripción opcional |
| `color` | String | ❌ No | Formato hex (#RGB o #RRGGBB) | Color para UI |
| `icon` | String | ❌ No | máx 50 caracteres | Emoji o icono |

**Response 201 Created:**
```json
{
  "id": 3,
  "name": "Biología",
  "description": "Estudio de organismos vivos",
  "color": "#10B981",
  "icon": "🧬",
  "orderIndex": 2,
  "isArchived": false,
  "createdAt": "2024-12-03T15:45:00Z",
  "documentCount": 0
}
```

**Response 400 Bad Request:**
```json
{
  "name": ["El nombre es obligatorio"],
  "color": ["El color debe ser un código hexadecimal válido (ej: #3B82F6)"]
}
```

---

### PUT `/api/subjects/{id}`

**Descripción:** Actualiza una materia existente.

**Parámetros de Ruta:**
| Parámetro | Tipo | Descripción |
|-----------|------|-------------|
| `id` | Integer | ID de la materia a actualizar |

**Request Body:**
```json
{
  "name": "Biología Avanzada",
  "description": "Estudio profundo de organismos vivos",
  "color": "#06B6D4",
  "icon": "🔬",
  "orderIndex": 1,
  "isArchived": false
}
```

**Response 200 OK:**
```json
{
  "id": 3,
  "name": "Biología Avanzada",
  "description": "Estudio profundo de organismos vivos",
  "color": "#06B6D4",
  "icon": "🔬",
  "orderIndex": 1,
  "isArchived": false,
  "createdAt": "2024-12-03T15:45:00Z",
  "documentCount": 2
}
```

**Response 404 Not Found:**
```json
{
  "message": "Materia no encontrada"
}
```

---

### PATCH `/api/subjects/{id}/archive`

**Descripción:** Archiva o desarchiva una materia.

**Parámetros de Ruta:**
| Parámetro | Tipo | Descripción |
|-----------|------|-------------|
| `id` | Integer | ID de la materia |

**Request Body:**
```json
{
  "isArchived": true
}
```

**Response 200 OK:**
```json
{
  "id": 1,
  "name": "Matemáticas",
  "isArchived": true,
  "...": "..."
}
```

---

### DELETE `/api/subjects/{id}`

**Descripción:** Elimina una materia.

**Parámetros de Ruta:**
| Parámetro | Tipo | Descripción |
|-----------|------|-------------|
| `id` | Integer | ID de la materia |

**Query Parameters:**
| Parámetro | Tipo | Default | Descripción |
|-----------|------|---------|-------------|
| `force` | Boolean | `false` | Si `true`, elimina incluso si contiene documentos |

**Response 200 OK:**
```json
{
  "message": "Materia eliminada exitosamente"
}
```

**Response 400 Bad Request:**
```json
{
  "message": "La materia contiene documentos. Use force=true para eliminarla de todas formas."
}
```

**Response 404 Not Found:**
```json
{
  "message": "Materia no encontrada"
}
```

---

## MÓDULO DOCUMENTS

**Autenticación:** ✅ Requerida (JWT Bearer Token)

### GET `/api/documents`

**Descripción:** Obtiene todos los documentos del usuario.

**Response 200 OK:**
```json
[
  {
    "id": 1,
    "title": "Capítulo 1: Introducción a la Física",
    "fileName": "fisica_cap1.pdf",
    "fileSize": 2048576,
    "processingStatus": "Completed",
    "uploadDate": "2024-12-01T10:30:00Z",
    "subjectId": 1,
    "subjectName": "Física",
    "hasFlashcards": true,
    "hasQuiz": true,
    "hasSummary": true,
    "extractedText": "Lorem ipsum dolor sit amet..."
  }
]
```

---

### GET `/api/documents/{id}`

**Descripción:** Obtiene un documento específico.

**Parámetros de Ruta:**
| Parámetro | Tipo | Descripción |
|-----------|------|-------------|
| `id` | Integer | ID del documento |

**Response 200 OK:**
```json
{
  "id": 1,
  "title": "Capítulo 1: Introducción a la Física",
  "fileName": "fisica_cap1.pdf",
  "fileSize": 2048576,
  "processingStatus": "Completed",
  "uploadDate": "2024-12-01T10:30:00Z",
  "subjectId": 1,
  "subjectName": "Física",
  "hasFlashcards": true,
  "hasQuiz": true,
  "hasSummary": true,
  "extractedText": "Lorem ipsum dolor sit amet..."
}
```

**Response 404 Not Found:**
```json
{
  "message": "Documento no encontrado"
}
```

---

### GET `/api/documents/subject/{subjectId}`

**Descripción:** Obtiene todos los documentos de una materia específica.

**Parámetros de Ruta:**
| Parámetro | Tipo | Descripción |
|-----------|------|-------------|
| `subjectId` | Integer | ID de la materia |

**Response 200 OK:** Array de documentos

---

### GET `/api/documents/status/{status}`

**Descripción:** Obtiene documentos filtrados por estado de procesamiento.

**Parámetros de Ruta:**
| Parámetro | Tipo | Valores Válidos | Descripción |
|-----------|------|-----------------|-------------|
| `status` | String | `Pending`, `Completed`, `Failed` | Estado del procesamiento |

**Response 200 OK:** Array de documentos filtrados

---

### GET `/api/documents/{documentId}/flashcards`

**Descripción:** Obtiene todas las flashcards de un documento específico.

**Parámetros de Ruta:**
| Parámetro | Tipo | Descripción |
|-----------|------|-------------|
| `documentId` | Integer | ID del documento |

**Response 200 OK:**
```json
[
  {
    "id": 1,
    "question": "¿Qué es la velocidad?",
    "answer": "La velocidad es la razón de cambio de la posición...",
    "documentId": 1,
    "isStarred": false,
    "nextReviewDate": "2024-12-04T10:30:00Z"
  }
]
```

---

### POST `/api/documents`

**Descripción:** Crea un nuevo documento manualmente.

**Request Body:**
```json
{
  "title": "Nuevo Documento de Estudio",
  "subjectId": 1,
  "content": "Contenido del documento..."
}
```

**Response 201 Created:**
```json
{
  "id": 5,
  "title": "Nuevo Documento de Estudio",
  "fileName": "documento-5.txt",
  "processingStatus": "Pending",
  "uploadDate": "2024-12-03T16:00:00Z",
  "subjectId": 1,
  "..."
}
```

---

### POST `/api/documents/upload`

**Descripción:** Carga un archivo de documento (PDF, Word, etc.).

**Formato:** `multipart/form-data`

**Request Parameters:**
| Campo | Tipo | Requerido | Descripción |
|-------|------|-----------|-------------|
| `file` | File | ✅ Sí | Archivo a cargar (máx 20 MB) |
| `subjectId` | Integer | ✅ Sí | ID de la materia |

**Tipos de archivo soportados:**
- PDF (`.pdf`)
- Microsoft Word (`.docx`, `.doc`)
- Texto plano (`.txt`)
- OpenDocument (`.odt`)

**cURL Example:**
```bash
curl -X POST http://localhost:5000/api/documents/upload \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -F "file=@documento.pdf" \
  -F "subjectId=1"
```

**Response 201 Created:**
```json
{
  "id": 2,
  "title": "documento.pdf",
  "fileName": "documento.pdf",
  "fileSize": 2048576,
  "processingStatus": "Pending",
  "uploadDate": "2024-12-03T16:05:00Z",
  "subjectId": 1
}
```

**Response 400 Bad Request:**
```json
{
  "message": "El archivo debe ser menor a 20 MB"
}
```

---

### PUT `/api/documents/{id}`

**Descripción:** Actualiza metadatos de un documento.

**Parámetros de Ruta:**
| Parámetro | Tipo | Descripción |
|-----------|------|-------------|
| `id` | Integer | ID del documento |

**Request Body:**
```json
{
  "title": "Título Actualizado",
  "subjectId": 2,
  "extractedText": "Contenido actualizado..."
}
```

**Response 200 OK:** Documento actualizado

---

### PATCH `/api/documents/{id}/processing-status`

**Descripción:** Actualiza el estado de procesamiento de un documento.

**Parámetros de Ruta:**
| Parámetro | Tipo | Descripción |
|-----------|------|-------------|
| `id` | Integer | ID del documento |

**Request Body:**
```json
{
  "processingStatus": "Completed"
}
```

**Estados válidos:** `Pending`, `Processing`, `Completed`, `Failed`

**Response 200 OK:** Documento actualizado

---

### DELETE `/api/documents/{id}`

**Descripción:** Elimina un documento.

**Parámetros de Ruta:**
| Parámetro | Tipo | Descripción |
|-----------|------|-------------|
| `id` | Integer | ID del documento a eliminar |

**Response 200 OK:**
```json
{
  "message": "Documento eliminado exitosamente"
}
```

**Response 404 Not Found:**
```json
{
  "message": "Documento no encontrado o no autorizado"
}
```

---

## MÓDULO SUMMARIES

**Autenticación:** ✅ Requerida (JWT Bearer Token)

### POST `/api/summaries/generate/{documentId}`

**Descripción:** Genera un resumen breve del documento usando IA (Gemini).

**Parámetros de Ruta:**
| Parámetro | Tipo | Descripción |
|-----------|------|-------------|
| `documentId` | Integer | ID del documento a resumir |

**Response 200 OK:**
```json
{
  "summaryId": 1,
  "summaryText": "Este documento trata sobre los fundamentos de la física. Comienza explicando los conceptos básicos de movimiento y energía...",
  "documentId": 1,
  "createdAt": "2024-12-03T16:15:00Z"
}
```

**Response 403 Forbidden:**
```json
{
  "message": "No tienes permiso para acceder a este documento"
}
```

**Response 400 Bad Request:**
```json
{
  "message": "El documento no contiene texto para resumir"
}
```

---

### POST `/api/summaries/generate-detailed/{documentId}`

**Descripción:** Genera un resumen detallado del documento.

**Parámetros de Ruta:**
| Parámetro | Tipo | Descripción |
|-----------|------|-------------|
| `documentId` | Integer | ID del documento |

**Response 200 OK:** Similar a resumen breve pero con más detalle

---

### POST `/api/summaries/generate-key-concepts/{documentId}`

**Descripción:** Extrae los conceptos clave del documento.

**Parámetros de Ruta:**
| Parámetro | Tipo | Descripción |
|-----------|------|-------------|
| `documentId` | Integer | ID del documento |

**Response 200 OK:**
```json
{
  "summaryId": 3,
  "summaryText": "• Movimiento: Cambio de posición en el tiempo\n• Energía: Capacidad de realizar trabajo\n• Velocidad: Razón de cambio de posición\n...",
  "documentId": 1,
  "createdAt": "2024-12-03T16:20:00Z"
}
```

---

### GET `/api/summaries/{resumenId}/download`

**Descripción:** Descarga un resumen en formato Word (.docx).

**Parámetros de Ruta:**
| Parámetro | Tipo | Descripción |
|-----------|------|-------------|
| `resumenId` | Integer | ID del resumen a descargar |

**Response 200 OK:**
- **Content-Type:** `application/vnd.openxmlformats-officedocument.wordprocessingml.document`
- **Content:** Archivo binario Word (`.docx`)
- **Headers:**
  ```
  Content-Disposition: attachment; filename="Resumen-Titulo-Del-Documento.docx"
  ```

**Response 403 Forbidden:**
```json
{
  "message": "No tienes permiso para descargar este resumen"
}
```

**Response 404 Not Found:**
```json
{
  "message": "Resumen no encontrado"
}
```

**Response 500 Internal Server Error:**
```json
{
  "message": "Error al generar el reporte.",
  "details": "Error interno detallado..."
}
```

---

## MÓDULO FLASHCARDS

**Autenticación:** ✅ Requerida (JWT Bearer Token)

### POST `/api/flashcards/generate/{documentId}`

**Descripción:** Genera flashcards automáticamente desde un documento.

**Parámetros de Ruta:**
| Parámetro | Tipo | Descripción |
|-----------|------|-------------|
| `documentId` | Integer | ID del documento |

**Response 200 OK:**
```json
[
  {
    "id": 1,
    "question": "¿Qué es la velocidad?",
    "answer": "La velocidad es la razón de cambio de la posición con respecto al tiempo.",
    "documentId": 1,
    "isStarred": false,
    "nextReviewDate": null
  },
  {
    "id": 2,
    "question": "¿Cuál es la diferencia entre velocidad y rapidez?",
    "answer": "La velocidad es vectorial (dirección + magnitud), mientras que la rapidez es escalar (solo magnitud).",
    "documentId": 1,
    "isStarred": false,
    "nextReviewDate": null
  }
]
```

---

### POST `/api/flashcards/{documentId}`

**Descripción:** Crea una flashcard manualmente.

**Parámetros de Ruta:**
| Parámetro | Tipo | Descripción |
|-----------|------|-------------|
| `documentId` | Integer | ID del documento asociado |

**Request Body:**
```json
{
  "question": "¿Qué es la aceleración?",
  "answer": "La aceleración es la razón de cambio de la velocidad con respecto al tiempo."
}
```

**Response 200 OK:**
```json
{
  "id": 3,
  "question": "¿Qué es la aceleración?",
  "answer": "La aceleración es la razón de cambio de la velocidad con respecto al tiempo.",
  "documentId": 1,
  "isStarred": false,
  "nextReviewDate": null
}
```

---

### PUT `/api/flashcards/{flashcardId}`

**Descripción:** Actualiza una flashcard.

**Parámetros de Ruta:**
| Parámetro | Tipo | Descripción |
|-----------|------|-------------|
| `flashcardId` | Integer | ID de la flashcard |

**Request Body:**
```json
{
  "question": "¿Qué es la aceleración (actualizado)?",
  "answer": "La aceleración mide qué tan rápido cambia la velocidad..."
}
```

**Response 200 OK:** Flashcard actualizada

---

### DELETE `/api/flashcards/{flashcardId}`

**Descripción:** Elimina una flashcard.

**Parámetros de Ruta:**
| Parámetro | Tipo | Descripción |
|-----------|------|-------------|
| `flashcardId` | Integer | ID de la flashcard a eliminar |

**Response 200 OK:**
```json
{
  "message": "Flashcard eliminada correctamente"
}
```

---

### POST `/api/flashcards/review/{flashcardId}`

**Descripción:** Registra una revisión de flashcard (para el algoritmo de espaciado).

**Parámetros de Ruta:**
| Parámetro | Tipo | Descripción |
|-----------|------|-------------|
| `flashcardId` | Integer | ID de la flashcard |

**Request Body:**
```json
{
  "quality": 4
}
```

**Valores de `quality`:**
| Valor | Significado | Descripción |
|-------|-------------|-------------|
| `0` | Total olvido | No acertaste |
| `1` | Respuesta incorrecta | Respuesta incorrecta |
| `2` | Respuesta difícil | Te costó trabajo recordarla |
| `3` | Respuesta aceptable | Correcta con dudas |
| `4` | Respuesta fácil | Correcta fácilmente |
| `5` | Respuesta muy fácil | Muy fácil de recordar |

**Response 200 OK:**
```json
{
  "message": "Review registrada correctamente"
}
```

---

## MÓDULO QUIZ

**Autenticación:** ✅ Requerida (JWT Bearer Token)

### POST `/api/quiz/generate/{documentId}`

**Descripción:** Genera un cuestionario automáticamente desde un documento.

**Parámetros de Ruta:**
| Parámetro | Tipo | Descripción |
|-----------|------|-------------|
| `documentId` | Integer | ID del documento |

**Request Body:**
```json
{
  "questionCount": 10,
  "difficulty": "Medium"
}
```

**Parámetros:**
| Campo | Tipo | Valores Válidos | Default | Descripción |
|-------|------|-----------------|---------|-------------|
| `questionCount` | Integer | 5-50 | 10 | Número de preguntas |
| `difficulty` | String | `Easy`, `Medium`, `Hard` | `Medium` | Nivel de dificultad |

**Response 200 OK:**
```json
{
  "quizId": 1,
  "documentId": 1,
  "title": "Quiz: Introducción a la Física",
  "difficulty": "Medium",
  "questionCount": 10,
  "questions": [
    {
      "questionId": 1,
      "questionText": "¿Qué es la velocidad?",
      "questionType": "MultipleChoice",
      "options": [
        {
          "text": "La razón de cambio de posición"
        },
        {
          "text": "Una medida de rapidez"
        },
        {
          "text": "La aceleración"
        },
        {
          "text": "La energía"
        }
      ]
    }
  ]
}
```

---

### GET `/api/quiz/{quizId}/for-attempt`

**Descripción:** Obtiene un quiz listo para intentar (sin respuestas correctas visibles).

**Parámetros de Ruta:**
| Parámetro | Tipo | Descripción |
|-----------|------|-------------|
| `quizId` | Integer | ID del quiz |

**Response 200 OK:** Similar al anterior pero sin información de respuestas correctas

---

### POST `/api/quiz/{quizId}/attempts`

**Descripción:** Envía un intento de quiz (respuestas del usuario).

**Parámetros de Ruta:**
| Parámetro | Tipo | Descripción |
|-----------|------|-------------|
| `quizId` | Integer | ID del quiz |

**Request Body:**
```json
{
  "answeredQuestions": [
    {
      "questionId": 1,
      "selectedOptionIndex": 0
    },
    {
      "questionId": 2,
      "selectedOptionIndex": 2
    }
  ]
}
```

**Response 200 OK:**
```json
{
  "attemptId": 1
}
```

---

### POST `/api/quiz/attempts/{attemptId}/evaluate`

**Descripción:** Evalúa un intento de quiz y calcula la puntuación.

**Parámetros de Ruta:**
| Parámetro | Tipo | Descripción |
|-----------|------|-------------|
| `attemptId` | Integer | ID del intento |

**Response 200 OK:**
```json
{
  "attemptId": 1,
  "quizId": 1,
  "userId": 1,
  "correctAnswers": 8,
  "totalQuestions": 10,
  "score": 80,
  "completedAt": "2024-12-03T16:30:00Z",
  "questionResults": [
    {
      "questionId": 1,
      "isCorrect": true,
      "selectedOptionIndex": 0,
      "correctOptionIndex": 0
    },
    {
      "questionId": 2,
      "isCorrect": false,
      "selectedOptionIndex": 1,
      "correctOptionIndex": 2
    }
  ]
}
```

---

### GET `/api/quiz/attempts/{attemptId}`

**Descripción:** Obtiene el resultado detallado de un intento anterior.

**Parámetros de Ruta:**
| Parámetro | Tipo | Descripción |
|-----------|------|-------------|
| `attemptId` | Integer | ID del intento |

**Response 200 OK:** Resultado del intento (mismo formato que `evaluate`)

---

### GET `/api/quiz/attempts/history`

**Descripción:** Obtiene el historial de intentos de quiz del usuario.

**Query Parameters:**
| Parámetro | Tipo | Descripción |
|-----------|------|-------------|
| `documentId` | Integer (opcional) | Filtrar por documento específico |
| `quizId` | Integer (opcional) | Filtrar por quiz específico |

**Response 200 OK:**
```json
{
  "attempts": [
    {
      "attemptId": 1,
      "quizId": 1,
      "quizTitle": "Quiz: Introducción a la Física",
      "score": 80,
      "correctAnswers": 8,
      "totalQuestions": 10,
      "completedAt": "2024-12-03T16:30:00Z"
    },
    {
      "attemptId": 2,
      "quizId": 1,
      "quizTitle": "Quiz: Introducción a la Física",
      "score": 90,
      "correctAnswers": 9,
      "totalQuestions": 10,
      "completedAt": "2024-12-03T17:00:00Z"
    }
  ]
}
```

---

### GET `/api/quiz/{quizId}/download`

**Descripción:** Descarga un cuestionario en formato PDF.

**Parámetros de Ruta:**
| Parámetro | Tipo | Descripción |
|-----------|------|-------------|
| `quizId` | Integer | ID del cuestionario a descargar |

**Response 200 OK:**
- **Content-Type:** `application/pdf`
- **Content:** Archivo binario PDF
- **Headers:**
  ```
  Content-Disposition: attachment; filename="Cuestionario-Titulo-Del-Quiz.pdf"
  ```

**Response 404 Not Found:**
```json
{
  "message": "Quiz no encontrado"
}
```

---

## MÓDULO STUDY

**Autenticación:** ✅ Requerida (JWT Bearer Token)

### GET `/api/study/next`

**Descripción:** Obtiene la siguiente flashcard a estudiar según el algoritmo de espaciado.

**Response 200 OK:**
```json
{
  "flashcardId": 1,
  "question": "¿Qué es la velocidad?",
  "answer": null,
  "difficulty": 3,
  "documentId": 1,
  "documentTitle": "Capítulo 1",
  "nextReviewDate": "2024-12-05T10:00:00Z"
}
```

**Response 200 OK (Sin tarjetas):**
```json
null
```

---

### GET `/api/study/dashboard`

**Descripción:** Obtiene las estadísticas de estudio del usuario.

**Response 200 OK:**
```json
{
  "totalFlashcards": 50,
  "flashcardsReviewed": 25,
  "flashcardsPendingToday": 8,
  "progressPercentage": 50.0,
  "averageScore": 3.5,
  "studyStreak": 5,
  "lastStudyDate": "2024-12-03T16:00:00Z",
  "subjectsProgress": [
    {
      "subjectId": 1,
      "subjectName": "Física",
      "totalFlashcards": 20,
      "reviewedFlashcards": 12,
      "progressPercentage": 60.0
    },
    {
      "subjectId": 2,
      "subjectName": "Química",
      "totalFlashcards": 30,
      "reviewedFlashcards": 13,
      "progressPercentage": 43.3
    }
  ]
}
```

---

## MÓDULO MAPS

**Autenticación:** ❌ No requerida (actualmente)

### POST `/api/maps/mindmap/generate`

**Descripción:** Genera un mapa mental jerárquico desde el contenido.

**Request Body:**
```json
{
  "title": "Estructura de la Célula",
  "content": "Contenido del documento sobre la célula..."
}
```

**Response 200 OK:**
```json
{
  "mindMapId": 1,
  "title": "Estructura de la Célula",
  "nodes": [
    {
      "id": "node-1",
      "label": "Célula",
      "level": 0,
      "children": ["node-2", "node-3"]
    },
    {
      "id": "node-2",
      "label": "Núcleo",
      "level": 1,
      "children": ["node-4"]
    },
    {
      "id": "node-3",
      "label": "Citoplasma",
      "level": 1,
      "children": []
    },
    {
      "id": "node-4",
      "label": "ADN",
      "level": 2,
      "children": []
    }
  ]
}
```

---

### POST `/api/maps/conceptmap/generate`

**Descripción:** Genera un mapa conceptual (red de conceptos).

**Request Body:**
```json
{
  "title": "Conceptos de Física",
  "content": "Contenido del documento..."
}
```

**Response 200 OK:**
```json
{
  "conceptMapId": 1,
  "title": "Conceptos de Física",
  "nodes": [
    {
      "id": "concept-1",
      "label": "Movimiento"
    },
    {
      "id": "concept-2",
      "label": "Velocidad"
    },
    {
      "id": "concept-3",
      "label": "Aceleración"
    }
  ],
  "edges": [
    {
      "source": "concept-1",
      "target": "concept-2",
      "relationship": "se mide por"
    },
    {
      "source": "concept-2",
      "target": "concept-3",
      "relationship": "cambia con"
    }
  ]
}
```

---

### GET `/api/maps/mindmap/{id}`

**Descripción:** Obtiene un mapa mental específico.

**Parámetros de Ruta:**
| Parámetro | Tipo | Descripción |
|-----------|------|-------------|
| `id` | Integer | ID del mapa mental |

**Response 200 OK:** Estructura del mapa mental

---

### GET `/api/maps/conceptmap/{id}`

**Descripción:** Obtiene un mapa conceptual específico.

**Parámetros de Ruta:**
| Parámetro | Tipo | Descripción |
|-----------|------|-------------|
| `id` | Integer | ID del mapa conceptual |

**Response 200 OK:** Estructura del mapa conceptual

---

### DELETE `/api/maps/mindmap/{id}`

**Descripción:** Elimina un mapa mental.

**Parámetros de Ruta:**
| Parámetro | Tipo | Descripción |
|-----------|------|-------------|
| `id` | Integer | ID del mapa mental |

**Response 204 No Content:** Sin cuerpo

---

### DELETE `/api/maps/conceptmap/{id}`

**Descripción:** Elimina un mapa conceptual.

**Parámetros de Ruta:**
| Parámetro | Tipo | Descripción |
|-----------|------|-------------|
| `id` | Integer | ID del mapa conceptual |

**Response 204 No Content:** Sin cuerpo

---

## MÓDULO PROFILE

**Autenticación:** ✅ Requerida (JWT Bearer Token)

### GET `/api/profile`

**Descripción:** Obtiene el perfil del usuario autenticado.

**Response 200 OK:**
```json
{
  "id": 1,
  "name": "Juan Pérez",
  "email": "juan@example.com",
  "profilePicture": "https://example.com/photo.jpg",
  "educationLevel": "University",
  "createdAt": "2024-12-01T10:30:00Z",
  "lastLoginAt": "2024-12-03T15:45:00Z"
}
```

---

### PUT `/api/profile`

**Descripción:** Actualiza el perfil del usuario.

**Request Body:**
```json
{
  "name": "Juan Pablo Pérez",
  "educationLevel": "Highschool"
}
```

**Parámetros:**
| Campo | Tipo | Requerido | Validación | Descripción |
|-------|------|-----------|-----------|-------------|
| `name` | String | ✅ Sí | 3-50 caracteres | Nuevo nombre |
| `educationLevel` | String | ❌ No | `University`, `Highschool`, `Professional` | Nivel educativo |

**Response 200 OK:**
```json
{
  "message": "Perfil actualizado correctamente"
}
```

**Response 400 Bad Request:**
```json
{
  "name": ["El nombre debe tener entre 3 y 50 caracteres"]
}
```

---

## 📊 Códigos de Estado HTTP

| Código | Nombre | Descripción |
|--------|--------|-------------|
| `200` | OK | Solicitud exitosa |
| `201` | Created | Recurso creado exitosamente |
| `204` | No Content | Solicitud exitosa sin cuerpo de respuesta |
| `400` | Bad Request | Solicitud inválida (errores de validación) |
| `401` | Unauthorized | Token JWT faltante o inválido |
| `403` | Forbidden | Usuario no autorizado para esta acción |
| `404` | Not Found | Recurso no encontrado |
| `500` | Internal Server Error | Error interno del servidor |

---

## 🚨 Manejo de Errores

### Estructura de Errores Estándar

**Para errores de validación (400):**
```json
{
  "field": ["Error message 1", "Error message 2"],
  "anotherField": ["Error message"]
}
```

**Para otros errores (403, 404, 500):**
```json
{
  "message": "Descripción del error",
  "details": "Detalles adicionales (solo en desarrollo)"
}
```

### Manejo de Errores en Cliente Blazor

```csharp
public async Task<Result<T>> GetAsync<T>(string url)
{
    try
    {
        var token = await localStorage.GetItemAsStringAsync("jwtToken");
        var httpClient = httpClientFactory.CreateClient("AuthorizedClient");
        httpClient.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Bearer", token);

        var response = await httpClient.GetAsync(url);

        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsAsync<T>();
            return Result<T>.Success(content);
        }

        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            // Token expirado - Redirigir a login
            navigationManager.NavigateTo("/login");
            return Result<T>.Failure("Sesión expirada");
        }

        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return Result<T>.Failure("Recurso no encontrado");
        }

        var errorContent = await response.Content.ReadAsStringAsync();
        return Result<T>.Failure(errorContent);
    }
    catch (HttpRequestException ex)
    {
        return Result<T>.Failure($"Error de conexión: {ex.Message}");
    }
}
```

---

## 🎯 Consumo desde Cliente Blazor WASM

### Arquitectura Recomendada

```
StudyMateAI.Client/
├── Services/
│   ├── ApiClientBase.cs (Base para todos los clientes)
│   ├── AuthService.cs
│   ├── SubjectService.cs
│   ├── DocumentService.cs
│   ├── SummaryService.cs
│   ├── FlashcardService.cs
│   ├── QuizService.cs
│   └── StudyService.cs
├── Auth/
│   ├── CustomAuthStateProvider.cs
│   └── AuthenticationMessageHandler.cs
└── DTOs/
    ├── Auth/
    ├── Documents/
    └── ... (espejar estructura del backend)
```

### 1. **Cliente HTTP Base (ApiClientBase.cs)**

```csharp
public abstract class ApiClientBase
{
    protected readonly HttpClient HttpClient;
    protected readonly ILocalStorageService LocalStorageService;
    protected const string ApiBaseUrl = "http://localhost:5000/api";

    protected ApiClientBase(HttpClient httpClient, ILocalStorageService localStorage)
    {
        HttpClient = httpClient;
        LocalStorageService = localStorage;
    }

    protected async Task<T?> GetAsync<T>(string endpoint)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, $"{ApiBaseUrl}/{endpoint}");
        return await SendAsync<T>(request);
    }

    protected async Task<T?> PostAsync<T>(string endpoint, object? body = null)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, $"{ApiBaseUrl}/{endpoint}");
        if (body != null)
        {
            request.Content = new StringContent(
                JsonSerializer.Serialize(body),
                Encoding.UTF8,
                "application/json"
            );
        }
        return await SendAsync<T>(request);
    }

    protected async Task<T?> SendAsync<T>(HttpRequestMessage request)
    {
        var token = await LocalStorageService.GetItemAsStringAsync("jwtToken");
        if (!string.IsNullOrEmpty(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        try
        {
            var response = await HttpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    // Manejar token expirado
                    await LocalStorageService.RemoveItemAsync("jwtToken");
                    // Redirigir a login si es necesario
                }
                throw new HttpRequestException($"Error {response.StatusCode}");
            }

            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<T>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        catch (Exception ex)
        {
            // Logging
            Console.Error.WriteLine($"Error en {request.RequestUri}: {ex.Message}");
            throw;
        }
    }
}
```

### 2. **Servicio de Documentos**

```csharp
public interface IDocumentService
{
    Task<List<DocumentResponseDto>> GetAllAsync();
    Task<DocumentResponseDto?> GetByIdAsync(int id);
    Task<List<DocumentResponseDto>> GetBySubjectAsync(int subjectId);
    Task<DocumentResponseDto> UploadAsync(IBrowserFile file, int subjectId);
    Task DeleteAsync(int id);
}

public class DocumentService : ApiClientBase, IDocumentService
{
    public DocumentService(HttpClient httpClient, ILocalStorageService localStorage)
        : base(httpClient, localStorage)
    {
    }

    public async Task<List<DocumentResponseDto>> GetAllAsync()
    {
        return await GetAsync<List<DocumentResponseDto>>("documents") ?? [];
    }

    public async Task<DocumentResponseDto?> GetByIdAsync(int id)
    {
        return await GetAsync<DocumentResponseDto>($"documents/{id}");
    }

    public async Task<List<DocumentResponseDto>> GetBySubjectAsync(int subjectId)
    {
        return await GetAsync<List<DocumentResponseDto>>($"documents/subject/{subjectId}") ?? [];
    }

    public async Task<DocumentResponseDto> UploadAsync(IBrowserFile file, int subjectId)
    {
        using var form = new MultipartFormDataContent();
        var fileContent = new StreamContent(file.OpenReadStream());
        form.Add(fileContent, "file", file.Name);
        form.Add(new StringContent(subjectId.ToString()), "subjectId");

        var token = await LocalStorageService.GetItemAsStringAsync("jwtToken");
        var request = new HttpRequestMessage(HttpMethod.Post, $"{ApiBaseUrl}/documents/upload")
        {
            Content = form
        };

        if (!string.IsNullOrEmpty(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        var response = await HttpClient.SendAsync(request);
        var content = await response.Content.ReadAsStringAsync();

        return JsonSerializer.Deserialize<DocumentResponseDto>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        })!;
    }

    public async Task DeleteAsync(int id)
    {
        await PostAsync<object>($"documents/{id}", null); // Cambiar a DELETE en versión mejorada
    }
}
```

### 3. **Proveedor de Autenticación (CustomAuthStateProvider.cs)**

```csharp
public class CustomAuthStateProvider : AuthenticationStateProvider
{
    private readonly ILocalStorageService _localStorageService;
    private readonly HttpClient _httpClient;

    public CustomAuthStateProvider(ILocalStorageService localStorage, HttpClient httpClient)
    {
        _localStorageService = localStorage;
        _httpClient = httpClient;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var token = await _localStorageService.GetItemAsStringAsync("jwtToken");

        if (string.IsNullOrEmpty(token))
        {
            return new AuthenticationState(new ClaimsPrincipal());
        }

        var handler = new JwtSecurityTokenHandler();
        try
        {
            var jwtToken = handler.ReadJwtToken(token);

            if (jwtToken.ValidTo < DateTime.UtcNow)
            {
                // Token expirado
                await LogoutAsync();
                return new AuthenticationState(new ClaimsPrincipal());
            }

            var identity = new ClaimsIdentity(jwtToken.Claims, "jwt");
            var user = new ClaimsPrincipal(identity);

            return new AuthenticationState(user);
        }
        catch
        {
            await LogoutAsync();
            return new AuthenticationState(new ClaimsPrincipal());
        }
    }

    public async Task LoginAsync(string token)
    {
        await _localStorageService.SetItemAsStringAsync("jwtToken", token);
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    public async Task LogoutAsync()
    {
        await _localStorageService.RemoveItemAsync("jwtToken");
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }
}
```

### 4. **Registración en Program.cs del Cliente**

```csharp
builder.Services.AddAuthorizationCore();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthStateProvider>();

builder.Services.AddScoped<HttpClient>();
builder.Services.AddScoped<IDocumentService, DocumentService>();
builder.Services.AddScoped<ISubjectService, SubjectService>();
builder.Services.AddScoped<ISummaryService, SummaryService>();
// ... más servicios

builder.Services.AddBlazoredLocalStorage();
```

---

## ✨ Mejores Prácticas

### 1. **Manejo de Tokens JWT**

```csharp
// ✅ CORRECTO: Usar HttpClient con interceptor automático
public class AuthorizedHttpClientHandler : DelegatingHandler
{
    private readonly ILocalStorageService _localStorage;

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var token = await _localStorage.GetItemAsStringAsync("jwtToken");
        
        if (!string.IsNullOrEmpty(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        return await base.SendAsync(request, cancellationToken);
    }
}

// Registrar
builder.Services.AddScoped<AuthorizedHttpClientHandler>();
builder.Services.AddHttpClient("AuthorizedClient")
    .AddHttpMessageHandler<AuthorizedHttpClientHandler>();
```

### 2. **Serialización JSON**

```csharp
// ✅ CORRECTO: Usar JsonSerializerOptions consistentes
var options = new JsonSerializerOptions
{
    PropertyNameCaseInsensitive = true,
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    WriteIndented = true
};

var json = JsonSerializer.Serialize(obj, options);
```

### 3. **Manejo de Archivos (Descargas)**

```csharp
// Ejemplo: Descargar resumen como Word
public async Task DownloadSummaryAsWordAsync(int resumenId, string fileName)
{
    try
    {
        var token = await _localStorage.GetItemAsStringAsync("jwtToken");
        var request = new HttpRequestMessage(HttpMethod.Get, 
            $"{ApiBaseUrl}/summaries/{resumenId}/download");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _httpClient.SendAsync(request);
        
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsByteArrayAsync();
            
            // Usar JS interop para descargar
            await JS.InvokeVoidAsync("downloadFile", fileName, content);
        }
    }
    catch (Exception ex)
    {
        // Manejar error
    }
}

// En JavaScript (wwwroot/app.js)
window.downloadFile = function(fileName, contentBytes) {
    const url = URL.createObjectURL(new Blob([contentBytes]));
    const link = document.createElement('a');
    link.href = url;
    link.download = fileName;
    link.click();
    URL.revokeObjectURL(url);
};
```

### 4. **Patrón de Resultado Genérico**

```csharp
public class Result<T>
{
    public bool IsSuccess { get; set; }
    public T? Data { get; set; }
    public string? ErrorMessage { get; set; }

    public static Result<T> Success(T data) => 
        new() { IsSuccess = true, Data = data };

    public static Result<T> Failure(string message) => 
        new() { IsSuccess = false, ErrorMessage = message };
}

// Uso
var result = await documentService.GetAllAsync();
if (result.IsSuccess)
{
    documents = result.Data;
}
else
{
    errorMessage = result.ErrorMessage;
}
```

### 5. **Paginación Recomendada (Mejora Futura)**

```csharp
public class PaginatedRequest
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

public class PaginatedResponse<T>
{
    public List<T> Items { get; set; } = [];
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public bool HasNextPage => (PageNumber * PageSize) < TotalCount;
}
```

### 6. **Caching en Cliente**

```csharp
public class CachedDocumentService : IDocumentService
{
    private readonly IDocumentService _innerService;
    private readonly Dictionary<int, DocumentResponseDto> _cache = [];
    private DateTime _cacheExpiry = DateTime.MinValue;

    public async Task<DocumentResponseDto?> GetByIdAsync(int id)
    {
        if (_cache.TryGetValue(id, out var cached) && DateTime.UtcNow < _cacheExpiry)
        {
            return cached;
        }

        var doc = await _innerService.GetByIdAsync(id);
        if (doc != null)
        {
            _cache[id] = doc;
            _cacheExpiry = DateTime.UtcNow.AddMinutes(5);
        }

        return doc;
    }

    public void InvalidateCache()
    {
        _cache.Clear();
        _cacheExpiry = DateTime.MinValue;
    }
}
```

---

## 🔐 Seguridad

### Headers de Seguridad Importantes

```csharp
// En Program.cs del servidor
app.UseHsts();
app.UseHttpsRedirection();

// Headers de seguridad
app.Use(async (context, next) =>
{
    context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Add("X-Frame-Options", "DENY");
    context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");
    await next();
});
```

### Validación CSRF (Para formularios)

```csharp
// El cliente debe enviar un token CSRF en headers
builder.Services.AddAntiforgery(options => 
{
    options.HeaderName = "X-CSRF-TOKEN";
});
```

---

## 📞 Soporte y Contacto

- **Email:** support@studymateai.com
- **Documentación:** http://localhost:5000 (Swagger UI)
- **Issues:** GitHub Issues del repositorio
- **Team:** StudyMate AI Development Team

---

**Fin de la Documentación**

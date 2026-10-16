# Contexto de Arquitectura y Especificación: AsistenteIA

**Aplicación:** Asistente Académico Móvil con IA ("AsistenteIA")  
**Institución:** Universidad de San Buenaventura  
**Asignatura:** Programación Avanzada y Aplicada | VI Semestre  
**Docente:** Mg. Sandra Esperanza Sánchez Barragán  
**Desarrollador:** Juan Rodríguez  
**Tecnologías:** .NET MAUI 10, C# 13, XAML, MVVM (`CommunityToolkit.Mvvm`), SQLite (`sqlite-net-pcl`), REST API (Pollinations AI).  
**Directorio raíz:** `C:\Users\juand\source\repos\Proyecto Final`

---

## 1. Arquitectura General y Patrón MVVM

La solución está estructurada bajo una separación estricta de responsabilidades:

```text
AsistenteIA/
├── Models/
│   ├── ChatModels.cs            # DTOs para la API REST de chat completions (OpenAI Compatible)
│   └── ConversationRecord.cs    # Entidad SQLite con campos [PrimaryKey, AutoIncrement]
├── Services/
│   ├── ApiKeyService.cs         # Almacenamiento seguro en SecureStorage (sin claves quemadas)
│   ├── AiService.cs             # Consumo HTTP POST, timeout 60s, validación 5-2000 chars, CancellationToken
│   └── DatabaseService.cs       # Operaciones CRUD asíncronas sobre SQLite (asistente.db3)
├── ViewModels/
│   ├── HomeViewModel.cs         # Accesos directos y navegación
│   ├── ChatViewModel.cs         # Gestión de prompt, plantillas académicas, streaming de estado y guardado
│   ├── HistoryViewModel.cs      # Búsqueda dinámica, filtrado por categorías y eliminación confirmada
│   ├── DetailViewModel.cs       # Visualización completa y alternado de estado "Verificada" (actualización SQLite)
│   └── SettingsViewModel.cs     # Gestión de API Key, selector de modelo y diagnóstico de conexión en vivo
├── Views/
│   ├── HomePage.xaml            # Pantalla de inicio con decálogo ético y tarjetas de acceso
│   ├── ChatPage.xaml            # Pantalla de consulta con carrusel de plantillas y área de respuesta
│   ├── HistoryPage.xaml         # Colección de consultas guardadas con búsqueda y badges visuales
│   ├── DetailPage.xaml          # Detalle y control de verificación ética con fuentes
│   └── SettingsPage.xaml        # Pantalla de credenciales con IsPassword="True" y prueba de conexión
├── Resources/
│   └── Styles/
│       ├── Colors.xaml          # Paleta accesible (WCAG AA): Primary (#1E3A8A), Secondary (#0D9488), Accent (#D97706)
│       └── Styles.xaml          # Estilos de botones, bordes, entradas y etiquetas
├── AppShell.xaml                # Navegación TabBar (4 pestañas) y registro de ruta para DetailPage
└── MauiProgram.cs               # Configuración de Inyección de Dependencias (DI)
```

---

## 2. Flujo Completo de Datos (Demostración de Sustentación)

El flujo de información requerido para la sustentación institucional sigue la cadena:
$$\text{XAML (Vista)} \longrightarrow \text{ViewModel (Bindings y RelayCommand)} \longrightarrow \text{AiService / ApiKeyService} \longrightarrow \text{Serialización JSON (POST)} \longrightarrow \text{DatabaseService (SQLite CRUD)}$$

---

## 3. Modelo de Datos SQLite (`Conversations`)

Archivo físico de base de datos: `FileSystem.AppDataDirectory/asistente.db3`

| Campo | Tipo | Restricción | Descripción |
| :--- | :--- | :--- | :--- |
| `Id` | `int` | PrimaryKey, AutoIncrement | Identificador único de la consulta |
| `Prompt` | `string` | NotNull | Pregunta o instrucción formulada (5 a 2000 caracteres) |
| `Answer` | `string` | NotNull | Respuesta devuelta por el modelo de IA |
| `CreatedAt` | `DateTime` | Default UtcNow | Marca de tiempo de realización |
| `Category` | `string` | Default "General" | Categoría académica (Código, Bases de Datos, Algoritmos, etc.) |
| `IsVerified` | `bool` | Default false | Estado de contrastación ética con fuentes bibliográficas oficiales |

---

## 4. Plantillas Académicas Predefinidas

1. **Depuración y Análisis de Código:**
   > *"Analiza el siguiente fragmento de código en [Lenguaje], identifica posibles errores sintácticos, de lógica o vulnerabilidades de seguridad, y explica cómo corregirlos paso a paso sin dar la solución completa de inmediato..."*
2. **Optimización de Consultas SQL y Modelado Relacional:**
   > *"Explica cómo optimizar la siguiente consulta SQL para [PostgreSQL/MySQL/SQL Server] o sugiere el diseño relacional normalizado (3FN) para la siguiente necesidad académica..."*
3. **Explicación de Algoritmos y Complejidad (Big-O):**
   > *"Explica el funcionamiento y la complejidad temporal y espacial (notación Big-O) del siguiente algoritmo o estructura de datos con un ejemplo didáctico..."*
4. **Resumen Conceptual y Síntesis:**
   > *"Explica de forma clara, didáctica y con ejemplos aplicados los conceptos fundamentales de..."*

---

## 5. Matriz de Casos de Prueba (P01 al P10)

| Caso | Condición de Prueba | Implementación Técnica | Resultado Esperado |
| :---: | :--- | :--- | :--- |
| **P01** | Prompt válido y API Key válida | `AiService.AskAsync` POST a `chat/completions` | Respuesta visible en tarjeta y botón guardar habilitado. |
| **P02** | Prompt vacío o menor a 5 caracteres | Validación previa en `ChatViewModel.AskAsync` | Alerta de validación sin emitir tráfico de red HTTP. |
| **P03** | Sin conexión a Internet | Captura de `HttpRequestException` | Mensaje descriptivo en UI sin cierre inesperado (sin crash). |
| **P04** | API Key inválida o revocada | Manejo de HTTP 401 en `AiService` | Notificación clara de autenticación sin revelar la clave. |
| **P05** | Tiempo de espera agotado (> 60s) | `HttpClient.Timeout = TimeSpan.FromSeconds(60)` | Cancelación ordenada y mensaje de tiempo agotado. |
| **P06** | Cancelación voluntaria | `CancellationTokenSource.Cancel()` en `CancelCommand` | Solicitud abortada de inmediato y UI reactiva. |
| **P07** | Guardar conversación en base de datos | `DatabaseService.SaveConversationAsync` | Registro insertado en la tabla SQLite `Conversations`. |
| **P08** | Eliminar conversación del historial | `DisplayAlertAsync` de confirmación + `DeleteAsync` | Registro eliminado y lista actualizada reactivamente. |
| **P09** | Reinicio de la aplicación | Conexión persistente a `asistente.db3` | Los registros se cargan automáticamente desde el almacenamiento local. |
| **P10** | Pantalla pequeña o rotación | `ScrollView`, layouts elásticos y `Grid` responsivo | Todos los controles se mantienen legibles y accesibles. |

---

## 6. Matriz de Uso Responsable y Mitigación de Riesgos de IA

1. **Alucinaciones o Respuestas Inexactas:** Advertencia permanente visible en la interfaz y función obligatoria en `DetailPage` para alternar el estado a *"Verificada con fuentes académicas confiables"*.
2. **Privacidad de Datos:** Advertencia explícita en el inicio y configuración sobre la no inclusión de contraseñas ni datos sensibles en el prompt.
3. **Seguridad de Credenciales:** Empleo estricto de `SecureStorage` del sistema operativo, exclusión de claves en código duro y enmascaramiento con `IsPassword="True"`.
4. **Dependencia de Proveedor:** Formato de solicitud y respuesta desacoplado y compatible con OpenAI / Pollinations.
5. **Sesgo o Contenido Inadecuado:** Mensaje de sistema restrictivo delimitando el comportamiento a propósitos pedagógicos.
6. **Transparencia Académica:** Registro histórico de preguntas completas y categorías para fines de auditoría del aprendizaje.

# Asistente Académico Móvil con IA (.NET MAUI) - "AsistenteIA"

Proyecto Final de **Programación Avanzada y Aplicada | VI Semestre**  
**Universidad de San Buenaventura**  
**Docente:** Mg. Sandra Esperanza Sánchez Barragán  
**Estudiante / Desarrollador:** Juan Rodríguez (`jdrodriguezy@academia.usbbog.edu.co`)  
**Motor de IA:** Google Gemini (Google AI Pro / REST API) con soporte para endpoints OpenAI y Pollinations.  
**Tecnologías:** .NET MAUI 10, C# 13, XAML, MVVM (`CommunityToolkit.Mvvm`), SQLite (`sqlite-net-pcl`), REST API.

---

## 🚀 Requisitos y Configuración del Entorno

1. **Visual Studio Community 2022 o superior** con la carga de trabajo:
   - *"Desarrollo de la interfaz de usuario de aplicaciones multiplataforma de .NET" (.NET MAUI)*
2. **SDK de .NET 10** instalado.
3. **Clave de API de Google Gemini (Google AI Pro):**
   - Obtén tu clave gratuita o vinculada a tu plan Pro en [Google AI Studio](https://aistudio.google.com/apikey).
4. Paquetes NuGet principales (ya configurados en el proyecto):
   - `CommunityToolkit.Mvvm` (v8.4.0)
   - `sqlite-net-pcl` (v1.9.172)
   - `SQLitePCLRaw.bundle_green` (v2.1.10)

---

## 💻 Instrucciones de Compilación y Ejecución

### Opción A: Desde Visual Studio
1. Abrir la solución `Proyecto Final.slnx` en Visual Studio.
2. Seleccionar como proyecto de inicio: **`AsistenteIA`**.
3. En el selector de destino, escoger:
   - **Windows Machine** para depuración rápida de escritorio.
   - **Android Emulator / Dispositivo Físico** para probar la experiencia móvil.
4. Presionar `F5` o el botón de ejecución.

### Opción B: Desde la Línea de Comandos (.NET CLI)

```powershell
# Compilar y ejecutar para Windows Machine
dotnet build "AsistenteIA\AsistenteIA.csproj" -f net10.0-windows10.0.19041.0 -c Debug
dotnet run --project "AsistenteIA\AsistenteIA.csproj" -f net10.0-windows10.0.19041.0

# Compilar para Android (arm64 o x64)
dotnet build "AsistenteIA\AsistenteIA.csproj" -f net10.0-android -r android-arm64 -c Debug
```

---

## 📱 Estructura de Pantallas y Funcionalidades

1. **Inicio (`HomePage`):**
   - Propósito académico del sistema potenciado por Google Gemini.
   - Accesos directos a "Consultar IA", "Historial" y "Configuración".
   - Decálogo visible de uso ético y prevención de alucinaciones.

2. **Consultar IA (`ChatPage`):**
   - Entrada multilínea (`Editor`) con validación de longitud (5 a 2000 caracteres, RF02).
   - Carrusel de plantillas predefinidas:
     - *1. Depuración de Código (Code Review & Debugging)*
     - *2. Optimización SQL y Modelado Relacional (Database Helper 3FN)*
     - *3. Explicación de Algoritmos y Complejidad (Big-O Notation)*
     - *4. Resumen Conceptual y Síntesis*
   - Asincronía con control de reingreso (`IsBusy`), botón Cancelar (`CancellationTokenSource`), indicador visual de carga y mensaje de estado.
   - Respuestas generadas por **Gemini 2.5 Pro** con opción de guardado directo en SQLite local.

3. **Historial (`HistoryPage`):**
   - Lista interactiva de conversaciones guardadas en SQLite (`asistente.db3`).
   - Búsqueda en tiempo real por texto o por categoría.
   - Badge visual indicando si la respuesta ha sido verificada o está pendiente de contrastar.
   - Eliminación rápida con diálogo de confirmación previa.

4. **Detalle (`DetailPage`):**
   - Visualización íntegra de la pregunta, respuesta, categoría y fecha.
   - Control interactivo para alternar el estado ético **"✓ Verificada con fuentes académicas confiables"**, actualizando el registro en SQLite.
   - Opción de eliminar el registro y volver a la navegación anterior.

5. **Configuración y Acerca de (`SettingsPage`):**
   - Entrada de API Key con `IsPassword="True"` almacenada de forma segura en `SecureStorage`.
   - Selector de modelos:
     - `gemini-2.5-pro` (Por defecto / Recomendado para Google AI Pro)
     - `gemini-2.5-flash` (Alta velocidad y precisión)
     - `gemini-2.0-flash`
     - `gemini-1.5-pro`
     - `gemini-1.5-flash`
     - Modelos de respaldo (`openai/gpt-5.4-nano`, `mistral/mistral-large`)
   - Botón **"Probar Conexión con Gemini"** para diagnóstico en vivo con reporte de latencia en milisegundos.
   - Botón para eliminar la clave con confirmación previa.
   - Declaración de privacidad y ficha técnica del proyecto.

---

## 🔒 Matriz de Seguridad y Ética de IA

- **Almacenamiento seguro:** No existen claves quemadas en código ni repositorios. Todo secreto pasa por `SecureStorage`.
- **Mitigación de alucinaciones:** Sistema de marcado y verificación ética obligatoria para el estudiante.
- **Privacidad y minimización:** Ningún dato personal es almacenado ni transmitido fuera de la consulta académica directa.
- **Resiliencia de red:** Captura exhaustiva de errores HTTP (400, 401, 403, 404, 429, 500) y tiempos de espera (timeout 60s).

---

## 📋 Verificación de Casos de Prueba (P01 - P10)

Todos los 10 casos de prueba exigidos por la rúbrica institucional fueron diseñados, codificados y verificados:
- **P01:** Consulta válida con respuesta visible y botón de guardado activo.
- **P02:** Validación de prompt vacío o < 5 caracteres sin consumo de red.
- **P03:** Manejo resiliente sin conexión / modo avión (`HttpRequestException`).
- **P04:** Manejo de clave no configurada o inválida (HTTP 401).
- **P05:** Cancelación ordenada por timeout de 60 segundos.
- **P06:** Cancelación fluida a demanda mediante botón Cancelar.
- **P07:** Inserción y persistencia en la tabla SQLite `Conversations`.
- **P08:** Eliminación de conversación con diálogo confirmatorio.
- **P09:** Persistencia demostrada tras reiniciar la aplicación.
- **P10:** Diseño responsivo y accesible adaptable a diferentes tamaños de pantalla y orientación.

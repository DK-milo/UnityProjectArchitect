# Unity Project Architect - Manual del Usuario

**Versión 0.3.0** - Guía Completa para Documentación y Organización de Proyectos

## 📋 Índice

1. [Comenzando](#comenzando)
2. [Características Principales](#características-principales)
3. [Integración con Unity Editor](#integración-con-unity-editor)
4. [Generación de Documentación](#generación-de-documentación)
5. [Análisis de Proyecto](#análisis-de-proyecto)
6. [Sistema de Plantillas](#sistema-de-plantillas)
7. [Exportación y Compartir](#exportación-y-compartir)
8. [Configuración](#configuración)
9. [Solución de Problemas](#solución-de-problemas)
10. [Uso Avanzado](#uso-avanzado)

---

## Comenzando

### Instalación

**Método 1: Package Manager (Recomendado)**
1. Abre Unity Package Manager (`Window > Package Manager`)
2. Haz clic en el botón `+` → `Add package from git URL`
3. Ingresa la URL del paquete o navega al archivo `.unitypackage`
4. Espera a que Unity importe y compile el paquete

**Método 2: Importación Manual**
1. Descarga el archivo `.unitypackage`
2. En Unity: `Assets > Import Package > Custom Package`
3. Selecciona el archivo descargado e importa todo el contenido

### Primer Uso

1. Después de la instalación, ve a `Window > Unity Project Architect > Main Window`
2. El paquete detectará automáticamente tu proyecto
3. Verás una pantalla de bienvenida con opciones de configuración

### Configuración Inicial

1. **Crear Project Data Asset**:
   - Haz clic en "Create New Project Data Asset"
   - Elige una ubicación en tu carpeta Assets
   - El asset almacena toda la documentación y configuraciones del proyecto

2. **Configurar Tipo de Proyecto**:
   - Selecciona tu tipo de proyecto (Juego 2D, Juego 3D, VR, Móvil, etc.)
   - Esto optimiza las plantillas y análisis para tus necesidades específicas

3. **Configuración AI Opcional**:
   - Para características mejoradas, agrega tu clave API de Claude en Settings
   - Las características AI funcionan junto con la creación manual de documentación

---

## Características Principales

### 📖 Generación de Documentación

**6 Secciones de Documentación Estándar:**

1. **Descripción General**: Resumen del proyecto, objetivos y características clave
2. **Arquitectura del Sistema**: Estructura técnica y patrones de diseño
3. **Modelo de Datos**: ScriptableObjects, flujo de datos y relaciones
4. **Especificación API**: Documentación de interfaces y ejemplos de uso
5. **Historias de Usuario**: Requisitos de características y criterios de aceptación
6. **Tickets de Trabajo**: Tareas de implementación y hoja de ruta de desarrollo

**Métodos de Generación:**
- **Con IA**: Generación inteligente de contenido basada en tu proyecto
- **Basado en Plantillas**: Contenido pre-estructurado que puedes personalizar
- **Manual**: Control completo sobre la creación y edición de contenido

### 🎯 Análisis de Proyecto

**Características de Análisis Integral:**
- **Análisis de Estructura**: Organización de carpetas y estructura de archivos
- **Análisis de Scripts**: Patrones de código y dependencias
- **Análisis de Assets**: Uso de recursos y oportunidades de optimización
- **Detección de Arquitectura**: Identificar patrones de diseño y decisiones arquitectónicas

**Insights en Tiempo Real:**
- Recomendaciones de rendimiento
- Sugerencias de organización
- Cumplimiento de mejores prácticas
- Identificación de deuda técnica

### 🏗️ Sistema de Plantillas

**Plantillas Pre-construidas:**
- Proyecto Unity General
- Juego Móvil 2D
- Juego PC 3D
- Experiencia VR
- Proyecto Educativo
- Proyecto Prototipo

**Creación de Plantillas Personalizadas:**
- Diseña tus propias estructuras de carpetas
- Configura plantillas de escenas
- Guarda para compartir con el equipo
- Importa/exporta configuraciones

---

## Integración con Unity Editor

### Ventana Principal (`Ctrl+Shift+P`)

**Acceso**: `Window > Unity Project Architect > Main Window`

**Secciones:**
1. **Encabezado**: Resumen del proyecto e información de versión
2. **Configuración del Proyecto**: Gestión del Project Data Asset
3. **Secciones de Documentación**: Habilitar/deshabilitar secciones, generar contenido
4. **Opciones de Exportación**: Elegir formatos de salida y ubicaciones
5. **Acciones**: Analizar proyecto, actualizar datos, abrir Template Creator

**Controles Clave:**
- **Generate**: Crear contenido para secciones individuales
- **Generate All Enabled**: Generación por lotes para secciones seleccionadas
- **Export Markdown/PDF**: Exportar documentación en formato elegido
- **Analyze Project**: Ejecutar análisis integral del proyecto
- **Template Creator**: Abrir ventana de diseño de plantillas

### Creador de Plantillas (`Ctrl+Shift+T`)

**Acceso**: `Window > Unity Project Architect > Template Creator`

**Características:**
1. **Información de Plantilla**: Nombre, descripción y tipo de proyecto
2. **Estructura de Carpetas**: Diseñar jerarquías de directorios
3. **Configuración de Escenas**: Configurar escenas predeterminadas
4. **Acciones**: Guardar, cargar y previsualizar plantillas

**Flujo de Trabajo:**
1. Ingresa nombre y descripción de la plantilla
2. Agrega rutas de carpetas (ej., "Scripts/Managers", "Art/Textures")
3. Configura plantillas de escenas con nombres y rutas
4. Guarda plantilla para reutilizar
5. Previsualiza antes de aplicar a proyectos

### Integración de Menús

**Menú Tools**:
- Ventana Principal
- Creador de Plantillas
- Análisis Rápido
- Exportar Documentación (Markdown/PDF)
- Configuraciones
- Acerca de

**Menú Window**:
- Acceso directo a ventanas principales
- Organizado bajo "Unity Project Architect"

**Menú Contextual de Assets** (Clic derecho en carpetas):
- Analizar Carpeta Seleccionada
- Aplicar Plantilla a Carpeta
- Generar Documentación para Asset

**Menú Help**:
- Documentación
- Reportar Problema
- Solicitar Característica

---

## Generación de Documentación

### Configurando Documentación

1. **Habilitar Secciones**:
   - En la Ventana Principal, marca las secciones que deseas generar
   - Cada sección tiene un toggle, contador de palabras e indicador de estado
   - Las marcas verdes indican secciones completadas

2. **Configurar Contenido**:
   - Haz clic en "Generate" para secciones individuales
   - Usa "Generate All Enabled" para procesamiento por lotes
   - El contenido se genera basado en el análisis de tu proyecto

3. **Revisar y Editar**:
   - El contenido generado aparece en el Project Data Asset
   - Edita contenido directamente en el Inspector
   - El contenido se actualiza automáticamente en formatos de exportación

### Secciones de Documentación Explicadas

**Descripción General**:
- Resumen del proyecto y propuesta de valor
- Características clave y puntos de venta únicos
- Información de audiencia objetivo y plataforma
- Cronología de desarrollo e información del equipo

**Arquitectura del Sistema**:
- Resumen de arquitectura técnica
- Patrones de diseño utilizados en el proyecto
- Relaciones de componentes y dependencias
- Consideraciones de rendimiento y escalabilidad

**Modelo de Datos**:
- Definiciones y uso de ScriptableObject
- Flujo de datos y gestión de estado
- Estrategias de serialización y persistencia
- Arquitectura de base de datos o sistema de guardado

**Especificación API**:
- Interfaces públicas y su uso
- Firmas de métodos y parámetros
- Ejemplos de uso y mejores prácticas
- Pautas de integración para miembros del equipo

**Historias de Usuario**:
- Requisitos de características en formato de historia de usuario
- Criterios de aceptación y definición de terminado
- Niveles de prioridad y orden de implementación
- Consideraciones de experiencia del usuario

**Tickets de Trabajo**:
- Tareas de implementación y requisitos técnicos
- Correcciones de errores y elementos de deuda técnica
- Tareas de pruebas y aseguramiento de calidad
- Actividades de despliegue y mantenimiento

---

## Análisis de Proyecto

### Ejecutando Análisis

**Análisis Automático**:
- Se ejecuta cuando abres la Ventana Principal
- Se actualiza cuando cambian los archivos del proyecto
- Proporciona insights en tiempo real

**Análisis Manual**:
- Haz clic en "Analyze Project" en la Ventana Principal
- Usa "Quick Analysis" desde el menú Tools
- Clic derecho en carpetas para análisis dirigido

### Entendiendo Resultados de Análisis

**Sección de Métricas**:
- Tiempo de análisis y estado de completitud
- Métricas de tamaño y complejidad del proyecto
- Estadísticas de conteo de archivos y organización

**Sección de Insights**:
- Insights codificados por colores (Info: Azul, Advertencia: Amarillo, Crítico: Rojo)
- Observaciones de arquitectura
- Recomendaciones de rendimiento
- Sugerencias de organización

**Sección de Recomendaciones**:
- Sugerencias basadas en prioridad (Alta, Media, Baja)
- Estimaciones de esfuerzo para implementación
- Próximos pasos accionables
- Cumplimiento de mejores prácticas

### Actuando sobre Recomendaciones

1. **Revisar Recomendaciones**: Prioriza basado en impacto y esfuerzo
2. **Aplicar Acciones**: Haz clic en botones "Apply" para correcciones automatizadas
3. **Seguir Progreso**: Monitorea implementación a través de actualizaciones de análisis
4. **Validar Cambios**: Re-ejecuta análisis para ver mejoras

---

## Sistema de Plantillas

### Usando Plantillas Pre-construidas

1. **Acceder Plantillas**:
   - Abre ventana Template Creator
   - Navega plantillas disponibles en el dropdown
   - Previsualiza estructura de plantilla

2. **Aplicar Plantillas**:
   - Selecciona plantilla que coincida con tu tipo de proyecto
   - Haz clic en "Apply Template"
   - Elige carpeta objetivo o aplica a todo el proyecto

3. **Personalizar Aplicación**:
   - Selecciona qué carpetas crear
   - Elige si fusionar con estructura existente
   - Configura opciones de generación de escenas

### Creando Plantillas Personalizadas

**Proceso de Diseño de Plantillas**:

1. **Definir Información**:
   - Nombre de plantilla (ej., "Juego Móvil 2D")
   - Descripción explicando caso de uso
   - Clasificación de tipo de proyecto

2. **Diseñar Estructura de Carpetas**:
   - Agrega rutas de carpetas una por una
   - Usa barras diagonales para jerarquía (ej., "Scripts/Managers")
   - Considera convenciones estándar de Unity

3. **Configurar Escenas**:
   - Agrega plantillas de escenas con nombres y rutas
   - Marca una escena como predeterminada
   - Configura jerarquía de escena si es necesario

4. **Guardar y Probar**:
   - Guarda plantilla en carpeta Assets/Templates
   - Prueba aplicación en proyectos nuevos
   - Comparte con miembros del equipo

**Mejores Prácticas de Plantillas**:
- Sigue convenciones de nomenclatura de Unity
- Crea agrupaciones lógicas (Scripts, Art, Audio, etc.)
- Incluye carpetas estándar (Prefabs, Materials, Scenes)
- Considera escalabilidad del proyecto
- Documenta propósito y uso de plantilla

### Compartiendo Plantillas

**Exportar Plantillas**:
- Guarda assets de plantillas en control de versiones
- Exporta configuraciones de plantillas como archivos .asset
- Comparte plantillas a través de paquetes Unity

**Importar Plantillas**:
- Carga archivos .asset de plantillas en tu proyecto
- Las plantillas aparecen automáticamente en Template Creator
- Los miembros del equipo pueden acceder a plantillas compartidas

---

## Exportación y Compartir

### Formatos de Exportación

**Exportación Markdown**:
- Documentación lista para GitHub con tabla de contenidos
- Íconos emoji para atractivo visual
- Bloques de código y formato preservado
- Adecuado para archivos README y wikis

**Exportación PDF**:
- Formato de documento profesional
- Encabezados y secciones con estilo
- Adecuado para stakeholders y paquetes de documentación
- Formato listo para imprimir

**Unity Assets**:
- Integración con ScriptableObject
- Referencia y acceso en el editor
- Vinculado a datos del proyecto
- Controlado por versión con el proyecto

### Configuración de Exportación

**Configuraciones de Salida**:
1. Elige formato de exportación (Markdown/PDF)
2. Selecciona ubicación de salida
3. Configura nombre de archivo y metadatos
4. Elige secciones a incluir

**Exportación por Lotes**:
- Exporta todas las secciones habilitadas de una vez
- Mantiene formato consistente
- Generación automática de tabla de contenidos
- Seguimiento de progreso durante exportación

### Compartiendo Documentación

**Colaboración en Equipo**:
- Incluye Project Data Assets en control de versiones
- Exporta documentación a ubicaciones compartidas
- Usa convenciones de nomenclatura consistentes
- Actualizaciones regulares y sincronización

**Compartir Externo**:
- Exportaciones PDF para stakeholders
- Markdown para documentación técnica
- Hosting online de contenido exportado
- Integración con herramientas de gestión de proyectos

---

## Configuración

### Configuraciones de Proyecto

**Acceso**: `Edit > Project Settings > Unity Project Architect`

**Configuración AI**:
- Configuración de clave API de Claude
- Preferencias de generación de contenido
- Configuraciones de idioma y estilo
- Controles de límite de tasa y uso

**Configuraciones de Exportación**:
- Ubicaciones de salida predeterminadas
- Convenciones de nomenclatura de archivos
- Preferencias de formato
- Personalización de plantillas

**Configuraciones de Análisis**:
- Frecuencia de análisis y triggers
- Tipos de archivos y carpetas incluidas
- Umbrales de rendimiento
- Reglas de análisis personalizadas

### Project Data Asset

**Configuración del Inspector**:
- Metadatos del proyecto (nombre, descripción, versión)
- Información del equipo y contactos
- Configuraciones de sección de documentación
- Preferencias de plantillas

**Configuraciones Avanzadas**:
- Plantillas de documentación personalizadas
- Reglas de exclusión de análisis
- Personalización de exportación
- Preferencias de integración

---

## Solución de Problemas

### Problemas Comunes

**El Paquete No Carga**:
- Verifica compatibilidad de versión de Unity (2023.3+)
- Revisa consola para errores de compilación
- Reimporta paquete si es necesario
- Reinicia Unity Editor

**La Generación de Documentación Falla**:
- Verifica que Project Data Asset esté configurado correctamente
- Verifica que secciones habilitadas tengan información requerida del proyecto
- Revisa consola para mensajes de error específicos
- Intenta generar secciones individuales primero

**Problemas de Exportación**:
- Asegúrate de que el directorio de salida existe y es escribible
- Verifica que el nombre de archivo no contenga caracteres inválidos
- Verifica espacio suficiente en disco
- Intenta formatos de exportación diferentes

**Problemas de Análisis**:
- Proyectos grandes pueden tomar más tiempo para analizar
- Excluye carpetas innecesarias del análisis
- Verifica archivos corruptos o inaccesibles
- Reinicia análisis si parece atascado

### Optimización de Rendimiento

**Proyectos Grandes**:
- Usa análisis selectivo en carpetas específicas
- Excluye salidas de build y archivos temporales
- Configura configuraciones de análisis para tu tamaño de proyecto
- Considera dividir proyectos grandes en módulos

**Uso de Memoria**:
- Cierra ventanas de documentación no utilizadas
- Limpia caché de análisis periódicamente
- Monitorea uso de memoria de Unity durante análisis
- Reinicia Unity si el uso de memoria se vuelve excesivo

### Obteniendo Ayuda

**Ayuda Integrada**:
- Tooltips al pasar el mouse sobre elementos UI
- Mensajes de estado en la Ventana Principal
- Indicadores de progreso durante operaciones
- Mensajes de error con guía específica

**Recursos Externos**:
- Documentación y guías del paquete
- GitHub Issues para reportes de errores
- Foros comunitarios y discusiones
- Envíos de solicitudes de características

---

## Uso Avanzado

### Integración con Scripting

**Acceder Project Data en Código**:
```csharp
// Encontrar Project Data Asset
var projectData = FindObjectOfType<UnityProjectDataAsset>();

// Acceder secciones de documentación
foreach (var section in projectData.ProjectData.DocumentationSections)
{
    Debug.Log($"Sección: {section.SectionType}, Estado: {section.Status}");
}
```

**Análisis Personalizado**:
- Extiende capacidades de análisis con scripts personalizados
- Agrega métricas e insights específicos del proyecto
- Integra con herramientas de desarrollo existentes
- Automatiza actualizaciones de documentación

### Automatización

**Integración CI/CD**:
- Genera documentación como parte del proceso de build
- Valida completitud de documentación
- Exporta documentación a ubicaciones de despliegue
- Dispara análisis en cambios de código

**Flujos de Trabajo Personalizados**:
- Crea pipelines de documentación automatizados
- Programa actualizaciones regulares de análisis
- Integra con sistemas de gestión de proyectos
- Notificación y reportes personalizados

### Flujos de Trabajo de Equipo

**Estandarización**:
- Establece estándares de documentación del equipo
- Crea bibliotecas de plantillas compartidas
- Define procesos de análisis y revisión
- Mantiene estructuras de proyecto consistentes

**Colaboración**:
- Revisiones regulares de documentación
- Responsabilidad compartida para actualizaciones de contenido
- Integración con control de versiones
- Compartir plantillas entre equipos

---

## Consejos y Mejores Prácticas

### Documentación

1. **Comenzar Temprano**: Inicia documentación cuando la estructura del proyecto esté definida
2. **Actualizar Regularmente**: Mantén la documentación actualizada con el desarrollo
3. **Ser Específico**: Incluye detalles concretos y ejemplos
4. **Revisar Frecuentemente**: Las revisiones regulares del equipo mantienen la calidad
5. **Exportar Regularmente**: Mantén la documentación externa sincronizada

### Organización de Proyecto

1. **Usar Plantillas**: Comienza con plantillas de proyecto apropiadas
2. **Seguir Convenciones**: Apégate a los estándares de nomenclatura de Unity y del equipo
3. **Análisis Regular**: Ejecuta análisis después de cambios importantes
4. **Actuar sobre Insights**: Aborda recomendaciones prontamente
5. **Compartir Estándares**: Mantén consistencia del equipo

### Colaboración en Equipo

1. **Plantillas Compartidas**: Crea y mantén biblioteca de plantillas del equipo
2. **Estándares de Documentación**: Establece pautas de contenido y formato
3. **Revisiones Regulares**: Programa sesiones de revisión de documentación
4. **Control de Versiones**: Incluye todos los datos del proyecto en control de fuente
5. **Entrenamiento**: Asegúrate de que los miembros del equipo entiendan la herramienta

---

**¿Necesitas más ayuda?** Revisa el README del paquete, issues de GitHub, o foros comunitarios para soporte adicional y ejemplos.

**¡Feliz documentación!** 🚀
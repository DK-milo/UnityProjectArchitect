# Unity Project Architect - Manual de Usuario

## Descripción General
Unity Project Architect es un paquete para Unity Editor potenciado por IA que te ayuda a crear documentación profesional de proyectos y organizar tus proyectos de Unity con plantillas inteligentes.

## Lo Que Puedes Hacer

### 📁 **Organización de Proyectos**
- **Aplicar Plantillas de Proyecto**: Elige entre plantillas predefinidas (General, Móvil 2D, PC 3D) para crear instantáneamente estructuras de carpetas organizadas
- **Crear Plantillas Personalizadas**: Guarda la estructura actual de tu proyecto como una plantilla reutilizable para futuros proyectos
- **Gestión Inteligente de Carpetas**: Organiza automáticamente los assets en carpetas estándar de Unity (Scripts, Prefabs, Materials, etc.)

### 📖 **Generación de Documentación**
- **6 Secciones Estándar de Documentación**: Genera automáticamente documentación completa del proyecto incluyendo:
  - Descripción General del Producto
  - Arquitectura del Sistema
  - Modelo de Datos
  - Especificación de API
  - Historias de Usuario
  - Tickets de Trabajo
- **Contenido Potenciado por IA**: Usa la integración con Claude API para generación inteligente de documentación
- **Opciones de Exportación**: Exporta documentación a formatos Markdown o PDF

### 🎯 **Gestión de Proyectos**
- **Análisis de Proyecto**: Analiza la estructura del proyecto existente y genera insights
- **Seguimiento de Progreso**: Monitorea la finalización de documentación y el estado de organización del proyecto
- **Colaboración en Equipo**: Estandariza estructuras de proyecto entre miembros del equipo

## Cómo Usar

### 1. **Instalación**
1. Abre Unity Package Manager
2. Agrega el paquete desde URL de git o importa el `.unitypackage`
3. El paquete aparecerá en `Window > Unity Project Architect`

### 2. **Configuración Inicial**
1. Ve a `Window > Unity Project Architect`
2. Crea o carga un asset `ProjectData` para tu proyecto
3. Configura tu tipo de proyecto (2D, 3D, VR, Móvil, etc.)
4. (Opcional) Agrega tu clave API de Claude para funciones de IA

### 3. **Aplicar una Plantilla de Proyecto**
1. Abre la ventana de Project Architect
2. Haz clic en la pestaña "Templates"
3. Selecciona una plantilla que coincida con tu tipo de proyecto:
   - **General Unity Project**: Carpetas estándar para cualquier proyecto de Unity
   - **Mobile 2D Game**: Optimizado para juegos móviles 2D con carpetas de sprites y UI
   - **PC 3D Game**: Configuración completa 3D con carpetas de modelos, shaders y lighting
4. Haz clic en "Apply Template"
5. El plugin creará la estructura de carpetas y organizará tu proyecto

### 4. **Generar Documentación**
1. En la ventana de Project Architect, ve a la pestaña "Documentation"
2. Configura qué secciones quieres generar
3. Para generación potenciada por IA:
   - Asegúrate de que la clave API de Claude esté configurada
   - Personaliza los prompts para cada sección
4. Haz clic en "Generate Documentation"
5. Revisa y edita el contenido generado
6. Exporta a Markdown o PDF cuando esté listo

### 5. **Crear Plantillas Personalizadas**
1. Organiza la estructura de tu proyecto como desees
2. Ve a la pestaña "Templates"
3. Haz clic en "Create Template from Current Project"
4. Nombra tu plantilla y agrega descripción
5. Guarda para reutilizar en futuros proyectos

## Características Principales

### ✅ **Lo Que el Plugin Crea**
- Estructuras de carpetas organizadas
- Escenas de Unity con configuración básica
- Archivos de definición de ensamblados
- Documentación completa del proyecto
- Assets de plantillas para reutilizar

### ❌ **Lo Que el Plugin NO Crea**
- Scripts de C# o código de gameplay
- Componentes MonoBehaviour
- Mecánicas de juego o lógica
- Assets de arte o contenido

## Requisitos

### **Requisitos Mínimos**
- Unity 6 (6000.0.0f1+)
- Sin dependencias externas

### **Opcional para Funciones Mejoradas**
- Clave API de Claude para documentación potenciada por IA
- Conexión a internet para funciones de IA

## Consejos para Mejores Resultados

1. **Empezar Temprano**: Aplica plantillas al crear nuevos proyectos para mejor organización
2. **Personalizar Plantillas**: Crea plantillas específicas del proyecto para el flujo de trabajo de tu equipo
3. **Documentación Regular**: Actualiza la documentación conforme evoluciona tu proyecto
4. **Prompts de IA**: Personaliza los prompts de IA para que coincidan con las necesidades específicas de tu proyecto
5. **Estándares de Equipo**: Usa plantillas consistentes en tu equipo para mejor colaboración

## Soporte

Para problemas, solicitudes de funciones o contribuciones, visita el repositorio del proyecto o contacta al equipo de desarrollo.

---

*Unity Project Architect - Simplificando la gestión de proyectos Unity y documentación desde 2025*
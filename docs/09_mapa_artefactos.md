# Mapa de artefactos

## Carpetas principales y proposito
- `Class/`: logica de negocio y clases Nomad (.cs). Contiene modulos de NucleusRH y NucleusWF.
- `Form/`: definiciones de formularios (UI) en XML por modulo.
- `Workflow/`: definiciones de workflows (etapas, formularios, eventos).
- `WebCV/`: portal web de postulantes (HTML, JS, templates de consultas).
- `Config/`: configuraciones de modulos, dependencias y recursos.
- `Database/`: definiciones y scripts para base de datos.
- `Interfaces/`: interfaces de entrada o intercambio.
- `InterfacesOut/`: generador de interfaces de salida y templates.
- `Reports/`: definiciones de reportes.
- `Html/`: reportes HTML y gadgets.
- `Templates/`: templates base para workflows y parametros.
- `Resources/`: recursos por modulo.
- `Dictionary/`: diccionarios de mensajes.
- `Menu/`: definiciones de menus.
- `Event/`: eventos por modulo.
- `Server/`: recursos web y controles.

## Ejemplos de rutas clave
- Workflows: `Workflow/NucleusRH/Base/Vacaciones/Solicitud.WF.xml`
- Clases de workflow: `Class/NucleusRH/Base/Vacaciones/lib_v11.WFSolicitud.SOLICITUD.NomadClass.cs`
- WebCV: `WebCV/Templates/Pages/Login.htm`
- Interfaces out: `InterfacesOut/Source/Generico/Program.cs`
- Base de datos: `Database/base11desa.xml`

## Notas
- La estructura es modular por dominio (Personal, Organizacion, Liquidacion, etc.).
- Los modulos listados en `Config/NucleusRH/Base/Application.xml` definen el build y los sufijos de cada modulo.

## Fuentes
- `Config/NucleusRH/Base/Application.xml`
- `Workflow/NucleusRH/Base/Vacaciones/Solicitud.WF.xml`
- `Class/NucleusRH/Base/Vacaciones/lib_v11.WFSolicitud.SOLICITUD.NomadClass.cs`
- `WebCV/Templates/Pages/Login.htm`
- `InterfacesOut/Source/Generico/Program.cs`
- `Database/base11desa.xml`

# Configuracion del sistema

Esta guia resume donde se configura el sistema segun los archivos del repositorio (configuracion declarativa).

## 1) Modulos y dependencias
- **Modulos habilitados**: `Config/NucleusRH/Base/Application.xml` define los modulos, sufijos y dependencias de build (incluye Portal_Empleado y NucleusWF).
- **Dependencias base**: `Config/Base.DEPENDENCES.xml` lista modulos NucleusRH base.
- **Dependencias WF**: `Config/NucleusWF.Base.DEPENDENCES.xml` define dependencias del motor de workflows.

## 2) Recursos runtime y rutas
`Config/NucleusRH/Base/Resource.cfg.xml` centraliza rutas y parametros:
- Paths de logs y salida (`rhliq-log-path`, `nomad-log-path`, `method-log-path`).
- Paths de reportes y temporales (`nomad-report-html-outpath`, `httpengine-private-path`).
- Paths de interfaces (`nomad-interface-outpath`).
- Formularios y menu inicial (`start-form`, `start-form-menu`).
- Portal: `start-form-portal` y `start-form-menu-portal`.

## 3) Menus y navegacion
- **Menu principal**: `Menu/NucleusRH/Base/Base.menu.xml` (y otros `*.menu.xml` por modulo).
- **Menu portal**: `Menu/NucleusRH/Base/Base.portal.menu.xml` (autogestion) y `Menu/NucleusRH/Base/POR.menu.xml` (admin portal).
- **Menu WF**: `Menu/NucleusWF/Base/WRK.menu.xml` (definicion/organigramas/integracion).

## 4) Workflows
- **Workflows por modulo RH**: `Workflow/NucleusRH/Base/*/*.WF.xml` + clases `Class/NucleusRH/Base/*/lib_v11.WFSolicitud.*.cs`.
- **Motor WF (definicion/ejecucion)**: `Class/NucleusWF/Base/Definicion/*.cs` y `Class/NucleusWF/Base/Ejecucion/*.cs`.
- **Acceso en portal**: requiere organigramas y roles (ver `Document/NucleusRH/Base/Portal_Empleado/Acceso_Portal.wiki`).

## 5) Formularios y reportes
- **Formularios**: `Form/**` (UI de acciones, filtros y ABMs).
- **Reportes**: `Html/**` (reportes HTML, gadgets y consolas). Portal usa `Html/NucleusRH/Base/Portal_Empleado/*.rpt.XML` y NucleusWF usa `Html/NucleusWF/Base/**`.

## 6) Portal del Empleado
- **Contenido**: Novedades y tipos en `Class/NucleusRH/Base/Portal_Empleado/*.NomadClass.XML`.
- **Flows**: `Form/NucleusRH/Base/Portal_Empleado/Novedades/flow.XML`.
- **Gadgets**: `Html/NucleusRH/Base/Portal_Empleado/NovedadesEmpresa*.GEN.GADGET.XML`.
- **Paginas de portal**: ver `Document/NucleusRH/Base/Portal_Empleado/PagWebPortal.wiki`.

## 7) Interfaces e integraciones
- **Interfaces de salida**: templates en `InterfacesOut/Definitions/*.xml` y generador en `InterfacesOut/Source/Generico/*`.
- **Interfaces de entrada**: definiciones en `Interfaces/NucleusRH/Base/**`.

## 8) Base de datos
- Definiciones y scripts en `Database/*.xml` (ej. `Database/base11desa.xml`).
- Los DDOs definen tablas/atributos en `Class/**.NomadClass.XML` (ej. `POR02_NOVEDADES`, `POR01_TIPOSNOVEDADES`).

## Fuentes
- `Config/NucleusRH/Base/Application.xml`
- `Config/NucleusRH/Base/Resource.cfg.xml`
- `Config/Base.DEPENDENCES.xml`
- `Config/NucleusWF.Base.DEPENDENCES.xml`
- `Menu/NucleusRH/Base/*.menu.xml`
- `Menu/NucleusWF/Base/WRK.menu.xml`
- `Workflow/NucleusRH/Base/*/*.WF.xml`
- `Class/NucleusWF/Base/**`
- `Form/**`
- `Html/**`
- `InterfacesOut/**`
- `Interfaces/**`
- `Database/**`

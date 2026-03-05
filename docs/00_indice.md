# Indice de documentacion del proyecto

Esta documentacion se genero a partir del codigo y archivos del repositorio. El objetivo es describir que hace el sistema, sus flujos, integraciones, casos de uso y casos de prueba. Toda la informacion esta basada en archivos existentes en este repositorio.

## Mapa de documentos
- `docs/01_vision_general.md`: vision, alcance funcional y modulos principales.
- `docs/02_arquitectura_y_componentes.md`: arquitectura tecnica, componentes y diagrama de integracion.
- `docs/03_flujos_y_workflows.md`: flujos operativos y workflows (Vacaciones, Datos Personales, Reclamos).
- `docs/04_webcv.md`: flujos de WebCV (registro, login, postulacion, recupero de clave).
- `docs/05_interfaces_e_integraciones.md`: interfaces in/out y servicios de integracion.
- `docs/06_modelo_datos.md`: modelo de datos conceptual basado en clases y workflows.
- `docs/07_casos_de_uso.md`: casos de uso por actor.
- `docs/08_casos_de_prueba.md`: casos de prueba funcionales.
- `docs/09_mapa_artefactos.md`: mapa de carpetas y artefactos clave.
- `docs/10_portal_empleado.md`: portal del empleado, flujos y configuracion.
- `docs/11_nucleus_wf.md`: motor de workflows, definicion y ejecucion.
- `docs/12_configuracion.md`: guia de configuracion general.

## Alcance del analisis
- Estructura general revisada: `Class`, `Form`, `Workflow`, `WebCV`, `Config`, `Database`, `Interfaces`, `InterfacesOut`, `Reports`, `Templates`, `Html`, `Resources`, `Dictionary`, `Menu`, `Event`, `Server`.
- Flujos detallados en archivos concretos listados en la seccion de fuentes.
- No se ejecuto el sistema ni la base de datos; el analisis es estatico.

## Fuentes clave (paths)
- `Document/about.wiki`
- `Document/indice.wiki`
- `Config/NucleusRH/Base/Application.xml`
- `Config/NucleusRH/Base/Resource.cfg.xml`
- `Database/base11desa.xml`
- `Workflow/NucleusRH/Base/Vacaciones/Solicitud.WF.xml`
- `Workflow/NucleusRH/Base/Personal/Solicitud.WF.xml`
- `Workflow/NucleusRH/Base/QuejasyReclamos/Reclamo.WF.xml`
- `Class/NucleusRH/Base/Vacaciones/lib_v11.WFSolicitud.SOLICITUD.NomadClass.cs`
- `Class/NucleusRH/Base/Personal/lib_v11.WFSolicitud.SOLICITUD.cs`
- `Class/NucleusRH/Base/QuejasyReclamos/lib_v11.WFReclamos.RECLAMO.cs`
- `Class/NucleusRH/Base/SeleccionDePostulantes/lib_v11.CVs.CV.NomadClass.cs`
- `WebCV/Templates/Pages/*.htm`
- `WebCV/Scripts/controls.js`
- `InterfacesOut/Source/Generico/Program.cs`
- `InterfacesOut/Source/Generico/Classes/clsGenericWriter.cs`
- `InterfacesOut/Source/Generico/Classes/clsFunctions.cs`
- `InterfacesOut/Definitions/*.xml`
- `Interfaces/NucleusRH/Base/Liquidacion/*.XML`
- `Interfaces/NucleusRH/Base/Tiempos_Trabajados/Liquidacion/ArchivoHoras.XML`

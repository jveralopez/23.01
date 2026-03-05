# Vision general del sistema

## Proposito
El sistema es una solucion de RRHH orientada a la gestion integral del capital humano. Segun `Document/about.wiki`, la aplicacion es web, basada en .NET, con mensajeria en XML y soporte para bases de datos relacionales (SQL Server / Oracle).

## Caracteristicas observadas
- Aplicacion web con presentacion en browser (formularios en `Form` y contenido HTML en `Html`/`WebCV`).
- Arquitectura modular y parametrizable (ver `Config/NucleusRH/Base/Application.xml`).
- Workflows configurables para procesos clave (ver `Workflow/NucleusRH/Base/*/*.WF.xml`).
- Integraciones via servicios del framework Nomad (SQLService, FileServiceIO, StoreService, BINService, OutputMails).

## Modulos principales (segun configuracion)
`Config/NucleusRH/Base/Application.xml` define los modulos de build y su sufijo. A modo de resumen:
- Personal, Organizacion, Liquidacion, Tiempos_Trabajados, Vacaciones.
- Control_de_Visitas, Evaluacion, MedicinaLaboral, Accidentabilidad, Capacitacion.
- Gestion_de_Postulantes, SeleccionDePostulantes, Portal_Empleado.
- Presupuesto, Tesoreria, Seguridad, Configuracion.
- NucleusWF (Definicion y Ejecucion de workflows).

## Documentacion interna existente
El repositorio contiene documentacion previa en `Document/` (por ejemplo `Document/indice.wiki` y `Document/about.wiki`). Esta documentacion se complementa y organiza en los archivos `docs/*.md`.

## Alcance funcional general
Basado en nombres de carpetas, workflows y clases:
- Gestion del personal y legajos.
- Gestion de tiempos trabajados y procesamiento de horas.
- Solicitudes (vacaciones y cambios de datos personales) con aprobacion.
- Reclamos y seguimiento por etapas.
- Gestion de postulantes y CVs, con portal WebCV.
- Interfaces de integracion para exportar datos (bancos, legales, sindicatos, etc.).

## Fuentes
- `Document/about.wiki`
- `Document/indice.wiki`
- `Config/NucleusRH/Base/Application.xml`

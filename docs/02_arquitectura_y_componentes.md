# Arquitectura y componentes

## Capas y componentes principales
- **UI WebCV**: paginas HTML y JS en `WebCV/Templates/Pages` y `WebCV/Scripts`. Ejecuta llamadas a metodos Nomad via `ExecuteNomadMethod` (ejemplo en `WebCV/Templates/Pages/Login.htm`).
- **UI Nomad (Forms)**: definiciones de formularios en `Form/` y reportes HTML en `Html/`.
- **Servicios de negocio**: clases .NET en `Class/NucleusRH` y `Class/NucleusWF` (ejemplo: `Class/NucleusRH/Base/SeleccionDePostulantes/lib_v11.CVs.CV.NomadClass.cs`).
- **Workflows**: definiciones XML en `Workflow/` y logica asociada en clases `WFSolicitud`/`WFReclamos`.
- **Datos**: definiciones de base y scripts en `Database/` (ejemplo `Database/base11desa.xml`).
- **Integraciones**: interfaces de entrada/salida en `Interfaces/` y `InterfacesOut/`.

## Diagrama de integracion (alto nivel)
```mermaid
flowchart LR
  Usuario[Usuario] --> WebCV[WebCV HTML/JS]
  Usuario --> NomadUI[Nomad UI Forms]

  WebCV -->|ExecuteNomadMethod| NomadSrv[Servicios Nomad (NucleusRH.Base)]
  NomadUI --> NomadSrv

  NomadSrv --> WF[NucleusWF Base]
  NomadSrv --> DB[(DB SQL Server / Oracle)]
  NomadSrv --> Mail[OutputMails]
  NomadSrv --> FileSvc[FileServiceIO / BINService]
  NomadSrv --> StoreSvc[StoreService]

  IntOut[InterfacesOut Generico] --> DB
  IntOut --> FileSvc
```

## Notas tecnicas
- El framework Nomad aparece en referencias .NET y en llamadas como `NomadProxy`, `NomadEnvironment`, `SQLService`, `FileServiceIO`.
- El repositorio contiene definiciones de interfaces de salida para bancos, legales y sindicatos en `InterfacesOut/Definitions`.
- Los workflows son configurables por XML y se invocan desde clases especificas (ejemplos en `Class/NucleusRH/Base/Vacaciones` y `Class/NucleusRH/Base/QuejasyReclamos`).

## Fuentes
- `WebCV/Templates/Pages/Login.htm`
- `Class/NucleusRH/Base/SeleccionDePostulantes/lib_v11.CVs.CV.NomadClass.cs`
- `Workflow/NucleusRH/Base/Vacaciones/Solicitud.WF.xml`
- `Workflow/NucleusRH/Base/Personal/Solicitud.WF.xml`
- `Workflow/NucleusRH/Base/QuejasyReclamos/Reclamo.WF.xml`
- `InterfacesOut/Source/Generico/Program.cs`
- `InterfacesOut/Source/Generico/Classes/clsGenericWriter.cs`
- `Database/base11desa.xml`

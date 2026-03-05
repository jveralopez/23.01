# Flujos y workflows

Este documento resume workflows observados en `Workflow/` y su logica en clases `WFSolicitud` y `WFReclamos`.

## Workflow: Solicitud de Vacaciones
Fuente: `Workflow/NucleusRH/Base/Vacaciones/Solicitud.WF.xml` y `Class/NucleusRH/Base/Vacaciones/lib_v11.WFSolicitud.SOLICITUD.NomadClass.cs`.

### Estados y transiciones
```mermaid
stateDiagram-v2
  [*] --> SOLICITAR
  SOLICITAR --> PENDAPROB
  SOLICITAR_NI --> PENDAPROB
  PENDAPROB --> FINALIZADA: aprobar
  PENDAPROB --> RESOLICITAR: rechazar
  RESOLICITAR --> PENDAPROB
  RESOLICITAR --> FINALIZADA: eliminar
```

### Secuencia (aprobar solicitud)
```mermaid
sequenceDiagram
  actor Usuario
  participant Form as Form Solicitud
  participant WF as WorkflowInstancias
  participant WFClass as WFSolicitud.SOLICITUD
  participant PER as LegajoVacaciones.PERSONAL_EMP
  participant TX as Transaction

  Usuario->>Form: Aprueba solicitud
  Form->>WF: Evento APROBAR
  WF->>WFClass: Aprobar(XMLData)
  WFClass->>PER: AgregarSolicitud / AprobarSolicitud
  WFClass->>TX: Save(objPER)
  WFClass->>WF: PassTo(FINALIZADA)
```

## Workflow: Cambio de Datos Personales
Fuente: `Workflow/NucleusRH/Base/Personal/Solicitud.WF.xml` y `Class/NucleusRH/Base/Personal/lib_v11.WFSolicitud.SOLICITUD.cs`.

### Estados y transiciones
```mermaid
stateDiagram-v2
  [*] --> SOLICITAR
  SOLICITAR --> PENDAPROB
  SOLICITAR_NI --> PENDAPROB
  PENDAPROB --> APROBADA
  PENDAPROB --> RECHAZADA
```

### Secuencia (enviar y aprobar)
```mermaid
sequenceDiagram
  actor Empleado
  participant Form as Form Solicitud
  participant WF as WorkflowInstancias
  participant WFClass as WFSolicitud.SOLICITUD
  participant LEG as Personal.Legajo.PERSONAL

  Empleado->>Form: Enviar cambios
  Form->>WFClass: EnviarSolicitud(XMLData)
  WFClass->>WF: PassTo(PENDAPROB)
  
  actor Aprobador
  Aprobador->>Form: Aprobar
  Form->>WFClass: AprobarSolicitud(XMLData, XMLList)
  WFClass->>LEG: Actualiza datos personales
  WFClass->>WF: Cambia etapa (APROBADA)
```

## Workflow: Reclamos
Fuente: `Workflow/NucleusRH/Base/QuejasyReclamos/Reclamo.WF.xml` y `Class/NucleusRH/Base/QuejasyReclamos/lib_v11.WFReclamos.RECLAMO.cs`.

### Estados y transiciones
```mermaid
stateDiagram-v2
  [*] --> Inicial
  Inicial --> PendClasif
  PendClasif --> PendResol
  PendResol --> Resuelto
  Resuelto --> PendConf
  PendConf --> Final
```

### Secuencia (guardar y notificar)
```mermaid
sequenceDiagram
  actor Usuario
  participant Form as Form Reclamo
  participant WFClass as WFReclamos.RECLAMO
  participant Mail as OutputMails
  participant WF as WorkflowInstancias

  Usuario->>Form: Crea/actualiza reclamo
  Form->>WFClass: Save(DDO, WFInstancia, Etapa, Historico)
  WFClass->>Mail: Envia mensajes pendientes
  WFClass->>WF: Pasar a etapa o grabar
```

## Observaciones comunes
- Los workflows se definen en XML con etapas, formularios asociados y acciones.
- La logica de negocio se implementa en clases C# que operan sobre DDOs y transacciones Nomad.

## Fuentes
- `Workflow/NucleusRH/Base/Vacaciones/Solicitud.WF.xml`
- `Workflow/NucleusRH/Base/Personal/Solicitud.WF.xml`
- `Workflow/NucleusRH/Base/QuejasyReclamos/Reclamo.WF.xml`
- `Class/NucleusRH/Base/Vacaciones/lib_v11.WFSolicitud.SOLICITUD.NomadClass.cs`
- `Class/NucleusRH/Base/Personal/lib_v11.WFSolicitud.SOLICITUD.cs`
- `Class/NucleusRH/Base/QuejasyReclamos/lib_v11.WFReclamos.RECLAMO.cs`

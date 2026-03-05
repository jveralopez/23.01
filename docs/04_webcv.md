# WebCV (Portal de postulantes)

## Objetivo
WebCV es el portal web para postulantes. Contiene paginas HTML, scripts JS y templates de consultas para cargar y gestionar CVs.

## Artefactos principales
- Paginas HTML en `WebCV/` y `WebCV/Templates/Pages/` (ejemplos: `Login.htm`, `Registro.htm`, `CargaCV_*`).
- Scripts en `WebCV/Scripts/` (ejemplo: `controls.js`).
- Consultas en `WebCV/Templates/Queries/` (listados de tipos, provincias, idiomas, etc.).

## Flujos principales

### Login
Fuente: `WebCV/Templates/Pages/Login.htm` y `Class/NucleusRH/Base/SeleccionDePostulantes/lib_v11.CVs.CV.NomadClass.cs`.
```mermaid
sequenceDiagram
  actor Postulante
  participant Browser as WebCV Login.htm
  participant Server as CV (Nomad Method)
  participant DB as Query loginUsuario

  Postulante->>Browser: Ingresa documento y clave
  Browser->>Server: ExecuteNomadMethod ValidatePass
  Server->>DB: Query loginUsuario
  DB-->>Server: Resultado
  Server-->>Browser: OK o error
```

### Registro de CV
Fuente: `Class/NucleusRH/Base/SeleccionDePostulantes/lib_v11.CVs.CV.NomadClass.cs` (metodo `SaveRegis`).
```mermaid
sequenceDiagram
  actor Postulante
  participant Browser as WebCV Registro
  participant Server as CV.SaveRegis
  participant Mail as OutputMails
  participant DB as Persistencia

  Postulante->>Browser: Completa datos
  Browser->>Server: SaveRegis(DATA)
  Server->>DB: Save CV
  Server->>Mail: Envia mail de registro
  Server-->>Browser: OK o error
```

### Recupero de clave
Fuente: `Class/NucleusRH/Base/SeleccionDePostulantes/lib_v11.CVs.CV.NomadClass.cs` (metodo `RemindPass`).
```mermaid
sequenceDiagram
  actor Postulante
  participant Browser as WebCV Login.htm
  participant Server as CV.RemindPass
  participant Mail as OutputMails
  participant DB as Query getPass

  Postulante->>Browser: Solicita recupero
  Browser->>Server: RemindPass(PARAM)
  Server->>DB: Query getPass
  Server->>Mail: Envia clave
  Server-->>Browser: OK o error
```

### Postulacion a aviso
Fuente: `WebCV/Templates/Pages/Login.htm` y `Class/NucleusRH/Base/SeleccionDePostulantes/lib_v11.CVs.CV.NomadClass.cs` (metodo `Postular`).
```mermaid
sequenceDiagram
  actor Postulante
  participant Browser as WebCV
  participant Server as CV.Postular
  participant DB as Persistencia
  participant Mail as OutputMails

  Postulante->>Browser: Postular a aviso
  Browser->>Server: Postular(DATA)
  Server->>DB: Guarda POSTULACION
  Server->>Mail: Envia confirmacion (si aplica)
  Server-->>Browser: OK o error
```

## Observaciones
- El portal consume metodos Nomad desde el navegador (`ExecuteNomadMethod`).
- El dominio de CV maneja datos personales, experiencias, idiomas y postulaciones.

## Fuentes
- `WebCV/Templates/Pages/Login.htm`
- `WebCV/Templates/Pages/Registro.htm`
- `WebCV/Templates/Pages/CargaCV_*.htm`
- `WebCV/Scripts/controls.js`
- `Class/NucleusRH/Base/SeleccionDePostulantes/lib_v11.CVs.CV.NomadClass.cs`

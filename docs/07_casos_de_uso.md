# Casos de uso

## Actores
- **Empleado**: solicita vacaciones y cambios de datos personales.
- **Aprobador/RRHH**: aprueba o rechaza solicitudes y reclamos.
- **Postulante**: registra CV, actualiza datos y se postula a avisos.
- **Administrador**: configura parametros, interfaces y reportes.
- **Sistema**: ejecuta workflows, notificaciones y procesos batch.

## Casos de uso principales

### 1. Solicitar vacaciones
- **Actor**: Empleado
- **Disparador**: completa formulario de solicitud.
- **Flujo**: crea solicitud, pasa a PENDAPROB.
- **Resultado**: solicitud registrada y en espera de aprobacion.
- **Fuentes**: `Workflow/NucleusRH/Base/Vacaciones/Solicitud.WF.xml`, `Class/NucleusRH/Base/Vacaciones/lib_v11.WFSolicitud.SOLICITUD.NomadClass.cs`.

### 2. Aprobar solicitud de vacaciones
- **Actor**: Aprobador/RRHH
- **Disparador**: aprueba en etapa PENDAPROB.
- **Flujo**: agrega solicitud en legajo, aprueba y finaliza workflow.
- **Resultado**: solicitud aprobada y finalizada.

### 3. Solicitar cambio de datos personales
- **Actor**: Empleado
- **Disparador**: completa formulario de cambios.
- **Flujo**: compara valores, conserva solo cambios, pasa a PENDAPROB.
- **Resultado**: solicitud en espera de aprobacion.
- **Fuentes**: `Workflow/NucleusRH/Base/Personal/Solicitud.WF.xml`, `Class/NucleusRH/Base/Personal/lib_v11.WFSolicitud.SOLICITUD.cs`.

### 4. Aprobar cambio de datos personales
- **Actor**: Aprobador/RRHH
- **Disparador**: aprueba solicitud.
- **Flujo**: actualiza legajo y domicilio fiscal.
- **Resultado**: datos personales actualizados y solicitud finalizada.

### 5. Registrar CV
- **Actor**: Postulante
- **Disparador**: completa formulario de registro en WebCV.
- **Flujo**: guarda CV y envia mail de registro.
- **Resultado**: CV creado y mail enviado.
- **Fuentes**: `Class/NucleusRH/Base/SeleccionDePostulantes/lib_v11.CVs.CV.NomadClass.cs`.

### 6. Login en WebCV
- **Actor**: Postulante
- **Disparador**: ingresa documento y clave.
- **Flujo**: valida credenciales y retorna OK o error.
- **Resultado**: sesion iniciada o rechazo.
- **Fuentes**: `WebCV/Templates/Pages/Login.htm`, `Class/NucleusRH/Base/SeleccionDePostulantes/lib_v11.CVs.CV.NomadClass.cs`.

### 7. Postular a aviso
- **Actor**: Postulante
- **Disparador**: selecciona aviso.
- **Flujo**: registra postulacion, valida duplicados, envia mail.
- **Resultado**: postulacion registrada.

### 8. Crear/derivar reclamo
- **Actor**: Empleado / Aprobador
- **Disparador**: crea reclamo o lo deriva a otra etapa.
- **Flujo**: guarda historico y notifica mensajes.
- **Resultado**: reclamo con historial actualizado.
- **Fuentes**: `Workflow/NucleusRH/Base/QuejasyReclamos/Reclamo.WF.xml`, `Class/NucleusRH/Base/QuejasyReclamos/lib_v11.WFReclamos.RECLAMO.cs`.

### 9. Generar interfaces de salida
- **Actor**: Sistema / Administrador
- **Disparador**: ejecucion de interfaz generica.
- **Flujo**: carga template + parametros, ejecuta query y genera archivo.
- **Resultado**: archivo exportado en FileService.
- **Fuentes**: `InterfacesOut/Source/Generico/Program.cs`, `InterfacesOut/Source/Generico/Classes/clsGenericWriter.cs`.

### 10. Exportar horas de liquidacion
- **Actor**: Sistema
- **Disparador**: ejecucion de interface de liquidacion.
- **Flujo**: consulta horas y genera archivo de salida.
- **Resultado**: archivo de interfaz con horas por legajo.
- **Fuentes**: `Interfaces/NucleusRH/Base/Tiempos_Trabajados/Liquidacion/ArchivoHoras.XML`.

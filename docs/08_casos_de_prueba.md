# Casos de prueba

Los siguientes casos de prueba se basan en flujos y metodos observados en el codigo. Son funcionales y pueden ejecutarse manualmente o automatizarse.

| ID | Caso | Precondicion | Pasos | Resultado esperado |
| --- | --- | --- | --- | --- |
| CP-01 | Login WebCV exitoso | CV existente con clave valida | 1) Abrir Login 2) Ingresar doc y clave 3) Click Ingresar | Respuesta OK y acceso a WebCV | 
| CP-02 | Login WebCV fallido | CV inexistente o clave invalida | 1) Abrir Login 2) Ingresar doc/clave invalidos | Mensaje de error y sin acceso | 
| CP-03 | Registro de CV | Datos completos y validos | 1) Abrir Registro 2) Completar datos 3) Enviar | CV guardado y mail de registro | 
| CP-04 | Recupero de clave | CV registrado con email | 1) Abrir Login 2) Click recuperar clave 3) Ingresar doc | Mail enviado con clave y respuesta OK | 
| CP-05 | Postulacion a aviso | CV logueado y aviso disponible | 1) Seleccionar aviso 2) Postular | Postulacion creada y mensaje OK | 
| CP-06 | Postulacion duplicada | CV ya postulado al aviso | 1) Repetir postulacion | Error: ya registrado a esa oferta | 
| CP-07 | Solicitud de vacaciones | Empleado con legajo valido | 1) Crear solicitud 2) Enviar | Solicitud en PENDAPROB | 
| CP-08 | Aprobacion de vacaciones | Solicitud en PENDAPROB | 1) Aprobar | Solicitud finalizada y registrada en legajo | 
| CP-09 | Rechazo de vacaciones | Solicitud en PENDAPROB | 1) Rechazar | Solicitud pasa a RESOLICITAR | 
| CP-10 | Cambio de datos personales | Empleado con datos base | 1) Editar datos 2) Enviar solicitud | Solicitud en PENDAPROB con cambios | 
| CP-11 | Aprobacion cambio datos | Solicitud en PENDAPROB | 1) Aprobar | Datos personales actualizados | 
| CP-12 | Crear reclamo | Usuario autenticado | 1) Crear reclamo 2) Guardar | Reclamo creado con historico | 
| CP-13 | Derivar reclamo | Reclamo en PendClasif | 1) Derivar a PendResol | Etapa cambiada y mensaje registrado | 
| CP-14 | Generar interfaz generica | Template y parametros disponibles | 1) Ejecutar Generico.exe con UUID/def | Archivo generado y guardado en FileService | 
| CP-15 | Exportar horas liquidacion | Liquidacion existente | 1) Ejecutar interfaz ArchivoHoras | Archivo de salida con horas por legajo | 

## Fuentes
- `WebCV/Templates/Pages/Login.htm`
- `Class/NucleusRH/Base/SeleccionDePostulantes/lib_v11.CVs.CV.NomadClass.cs`
- `Workflow/NucleusRH/Base/Vacaciones/Solicitud.WF.xml`
- `Class/NucleusRH/Base/Vacaciones/lib_v11.WFSolicitud.SOLICITUD.NomadClass.cs`
- `Workflow/NucleusRH/Base/Personal/Solicitud.WF.xml`
- `Class/NucleusRH/Base/Personal/lib_v11.WFSolicitud.SOLICITUD.cs`
- `Workflow/NucleusRH/Base/QuejasyReclamos/Reclamo.WF.xml`
- `Class/NucleusRH/Base/QuejasyReclamos/lib_v11.WFReclamos.RECLAMO.cs`
- `InterfacesOut/Source/Generico/Program.cs`
- `Interfaces/NucleusRH/Base/Tiempos_Trabajados/Liquidacion/ArchivoHoras.XML`

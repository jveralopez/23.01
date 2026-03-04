using System;
using System.Xml;
using System.IO;
using System.Collections;

using System.Reflection;
using Microsoft.CSharp;
using System.CodeDom.Compiler;

using Nomad.NSystem.Document;
using Nomad.NSystem.Proxy;
using Nomad.NSystem.Base;
using Nomad.NSystem.Functions;
using System.Text;
using System.Net;
namespace NucleusProject.Base.Project.Solicitudes
{

  public partial class SOLICITUD
  {

   const string tmpOpen = "{#";
    const string tmpClose = "#}";

    /// <summary>
    /// Retorna una Hashtable con el asunto y el texto del mail solicitado
    /// </summary>
    /// <returns></returns>
    public static Hashtable GetMails(string pstrCode, SOLICITUD pobjSolicitud, NomadXML pxmlInfo)
    {
      NomadXML xmlParams;
      NomadXML xmlResult;
      Hashtable htaResult;

      //Crea el objeto de Parametros
      xmlParams = new NomadXML("PARAM");
      xmlParams.SetAttr("cod", pstrCode);

      //Ejecuta el query
      xmlResult = NomadEnvironment.QueryNomadXML(NucleusProject.Base.Project.Textos_Mails.TEXTO_MAIL.Resources.QRY_Mails, xmlParams.ToString());
      xmlResult = xmlResult.FirstChild();

      htaResult = new Hashtable();

      //Reemplaza los valores
      for (NomadXML xmlMail = xmlResult.FirstChild(); xmlMail != null; xmlMail = xmlMail.Next())
      {
        htaResult.Add(xmlMail.GetAttr("c_texto_mail"), GetMailText(xmlMail.GetAttr("t_texto_mail"), pobjSolicitud, pxmlInfo));
      }

      return htaResult;

    }

    /// <summary>
    /// Retorna el texto del mail realizando los replace correspondientes
    /// </summary>
    /// <param name="pstrText">Texto a modificar</param>
    /// <param name="pobjSolicitud">Objeto Solicitud</param>
    /// <param name="pxmlInfo"></param>
    /// <returns></returns>
    private static string GetMailText(string pstrText, SOLICITUD pobjSolicitud, NomadXML pxmlInfo)
    {
      string strContent;
      string strBefore;
      string strToReplace;
      NomadXML xmlParams;
      System.Text.StringBuilder sbResult = new System.Text.StringBuilder();
      int intControl = 0;

      try
      {
        strContent = pstrText;

        ////////////////OBTENER DATOS QUE NO SON DE LA SOLICITUD
        string f_fecha="";
        string f_fecha_hora="";
        string hs_pend="";
        string hs_pend_param = "";
        string d_email_responsable="";
        NucleusProject.Base.Project.Aplicaciones.APLICACION objAplicacion=null;
        NucleusProject.Base.Project.Clientes.CLIENTE objCliente=null;
        NucleusProject.Base.Project.Proyectos.PROYECTO objProyecto=null;
        NucleusRH.Base.Personal.Legajo.PERSONAL objPersonal=null;

        //Obtener Modulo y Aplicacion
        NucleusProject.Base.Project.Aplicaciones.MODULO objModulo = NucleusProject.Base.Project.Aplicaciones.MODULO.Get(pobjSolicitud.oi_modulo);
        if (objModulo!=null)
          objAplicacion = NucleusProject.Base.Project.Aplicaciones.APLICACION.Get(objModulo.oi_aplicacion);

        //Obtener Contacto y Cliente
        NucleusProject.Base.Project.Clientes.CONTACTO objContacto = NucleusProject.Base.Project.Clientes.CONTACTO.Get(pobjSolicitud.oi_contacto);
        if (objContacto!=null)
        {
          objCliente = NucleusProject.Base.Project.Clientes.CLIENTE.Get(objContacto.oi_cliente);
          //Obtener Horas pendientes:
          xmlParams = new NomadXML("DATA");
          xmlParams.SetAttr("oi_cliente", objCliente.Id);
          if (objCliente!=null)
          {
            NomadXML MyXML=NomadProxy.GetProxy().SQLService().GetXML(SOLICITUD.Resources.qry_hrs_pendientes, xmlParams.ToString());
            NomadLog.Debug("MyXML:: " + MyXML.ToString());
            f_fecha=MyXML.GetAttr("f_fecha");
            f_fecha_hora=MyXML.GetAttr("f_fecha_hora");
            hs_pend=MyXML.GetAttr("hs_pend");
            hs_pend_param=MyXML.GetAttr("est");
          }
        }

        //Obtener Tarea y Proyecto
        NucleusProject.Base.Project.Tareas.TAREA objTarea = NucleusProject.Base.Project.Tareas.TAREA.Get(NomadEnvironment.QueryValue("PRY02_TAREAS","oi_tarea","oi_solicitud",pobjSolicitud.Id,"",false));
        if (objTarea!=null)
        {
          NomadLog.Info("objTarea:: " + objTarea.SerializeAll());
          objProyecto = NucleusProject.Base.Project.Proyectos.PROYECTO.Get(objTarea.oi_proyecto);
        }

        //Obtener Error
        NucleusProject.Base.Project.Errores.ERROR objError = NucleusProject.Base.Project.Errores.ERROR.Get(pobjSolicitud.oi_error);

        //Obtengo el Recurso
        NucleusProject.Base.Project.Recursos.RECURSO objRecurso = NucleusProject.Base.Project.Recursos.RECURSO.Get(pobjSolicitud.oi_recurso);
        if (objRecurso!=null)
        {
          d_email_responsable=objRecurso.d_email;
          objPersonal = NucleusRH.Base.Personal.Legajo.PERSONAL.Get(objRecurso.oi_personal);
           if (objPersonal!=null)
           {
            if (objPersonal.oi_usuario_sistema!="")
            {
              Nomad.Base.Login.Entidades.ENTIDAD MyETTY=Nomad.Base.Login.Entidades.ENTIDAD.Get(objPersonal.oi_usuario_sistema);
              if (MyETTY!=null)
                foreach(Nomad.Base.Login.Entidades.MAIL MyMAIL in MyETTY.MAILS)
                {
                  if (!MyMAIL.PRINCIPAL) continue;
                    d_email_responsable=MyMAIL.EMAIL;
                }
            }
          }
        }

        string estado="";
        switch (pobjSolicitud.c_estado)
        {
            case "1":
                estado = "PENDIENTE";
                break;
            case "2":
                estado = "DERIVADO A PMO";
                break;
            case "3":
                estado = "DERIVADO PARA ESTIMACION/DESARROLLO";
                break;
            case "4":
                estado = "DERIVADO A CLIENTE";
                break;
            case "5":
                estado = "CERRADO";
                break;
            case "6":
                estado = "CERRADO POR EL CLIENTE";
                break;
            case "7":
                estado = "RECHAZADO";
                break;
            case "8":
                estado = "TRABAJANDO";
                break;
            case "9":
                estado = "PRESUPUESTADO";
                break;
            default:
                estado = "NO DEFINIDO";
                break;
        }

        //Busca los sectores a reemplazar
        while (strContent != "" || intControl < 50)
        {

          strBefore = SubStringBefore(strContent, tmpOpen);
          strContent = SubStringAfter(strContent, tmpOpen);

          sbResult.Append(strBefore);

          if (strContent != "") {
            strToReplace = SubStringBefore(strContent, tmpClose);
            strContent = SubStringAfter(strContent, tmpClose);

            if (strToReplace != "")
            {
              //Se realiza el reemplazo del texto por lo indicado
              switch (strToReplace.ToUpper())
              {
                case "SIS.FECHA":
                  sbResult.Append(DateTime.Now.ToString("dd/MM/yyyy"));
                  break;

                case "SIS.HORA":
                  sbResult.Append(DateTime.Now.ToString("HH:mm"));
                  break;

                //Código de la solicitud
                case "SOL.CODIGO":
                  if (pobjSolicitud != null) sbResult.Append(pobjSolicitud.c_solicitud);
                  break;

                //Descripción de la solicitud
                case "SOL.DESCRIPCION":
                  if (pobjSolicitud != null) sbResult.Append(pobjSolicitud.d_solicitud);
                  break;

                //Criticidad (error) de la solicitud
                case "SOL.CRITICIDAD":
                  if (objError != null) sbResult.Append(objError.d_nombre);
                  break;

                 //La solicitud se incluye en el base
                 case "SOL.BASE":
                  if (pobjSolicitud != null) sbResult.Append(pobjSolicitud.l_inc_a_base);
                  break;

                //Estado de la solicitud
                case "SOL.ESTADO":
                  if (pobjSolicitud != null) sbResult.Append(estado);
                  break;

                //Detalle de la solicitud
                case "SOL.DETALLE":
                  if (pobjSolicitud != null) sbResult.Append(pobjSolicitud.o_detalle);
                  break;

                //Observaciones de la solicitud
                case "SOL.OBSERVACIONES":
                  if (pobjSolicitud != null) sbResult.Append(pobjSolicitud.o_observaciones);
                  break;

                //Código del contacto
                case "CON.CODIGO":
                  if (objContacto != null) sbResult.Append(objContacto.c_contacto);
                  break;

                //Descripcion del contacto
                case "CON.DESCRIPCION":
                  if (objContacto != null) sbResult.Append(objContacto.d_nom_y_ape);
                  break;

                //eMail de contacto
                case "CON.MAIL":
                  if (objContacto != null) sbResult.Append(objContacto.d_email);
                  break;

                //Código del cliente
                case "CLI.CODIGO":
                  if (objCliente != null) sbResult.Append(objCliente.c_cliente);
                  break;

                //Descripción del cliente
                case "CLI.DESCRIPCION":
                  if (objCliente != null) sbResult.Append(objCliente.d_nom_cliente);
                  break;

                //Descripción del responsable del cliente
                case "CLI.RESPONSABLE":
                  if (objCliente != null) sbResult.Append(objCliente.d_nom_contacto);
                  break;

                //eMail del responsable del cliente
                case "CLI.MAIL":
                  if (objCliente != null) sbResult.Append(objCliente.d_email);
                  break;

                //Horas pendientes de soporte de clientes
                case "CLI.HORASPENDIENTES":
                  if (objCliente != null) sbResult.Append(hs_pend);
                  break;

                //Condición de Horas pendientes de soporte de clientes
                case "CLI.HORASPENDCOND":
                  if (objCliente != null) sbResult.Append(hs_pend_param);
                  break;

                //Código del proyecto
                case "PRO.CODIGO":
                  if (objProyecto != null) sbResult.Append(objProyecto.c_proyecto);
                  break;

                //Descripción del proyecto
                case "PRO.DESCRIPCION":
                  if (objProyecto != null) sbResult.Append(objProyecto.d_proyecto);
                  break;

                //Código del recurso
                case "REC.CODIGO":
                  if (objPersonal != null) sbResult.Append(objRecurso.c_recurso);
                  break;

                //Descripción del recurso
                case "REC.DESCRIPCION":
                  if (objPersonal != null) sbResult.Append(objPersonal.d_ape_y_nom);
                  break;

                //eMail del recurso
                case "REC.MAIL":
                  if (pxmlInfo != null) sbResult.Append(d_email_responsable);
                  break;

                //Código de modulo
                case "MOD.CODIGO":
                  if (objModulo != null) sbResult.Append(objModulo.c_modulo);
                  break;

                //Descripción de modulo
                case "MOD.DESCRIPCION":
                  if (objModulo != null) sbResult.Append(objModulo.d_nombre);
                  break;

                //Código de aplicación
                case "APL.CODIGO":
                  if (objAplicacion != null) sbResult.Append(objAplicacion.c_aplicacion);
                  break;

                //Descripción de aplicación
                case "APL.DESCRIPCION":
                  if (objAplicacion != null) sbResult.Append(objAplicacion.d_aplicacion);
                  break;
              }

            }

          }

          intControl++;
        }

      } catch {
        return "";
      }

      return sbResult.ToString();

    }

    private static string SubStringBefore(string pSource, string pTarget)
    {
      string strResult;
      int intPos;

      intPos = pSource.IndexOf(pTarget);
      if (intPos >= 0)
        strResult = pSource.Substring(0, intPos);
      else
        strResult = pSource;

      return strResult;
    }

    private static string SubStringAfter(string pSource, string pTarget)
    {
      string strResult = "";
      int intPos;

      intPos = pSource.IndexOf(pTarget);
      if (intPos >= 0)
      {
        intPos = intPos + pTarget.Length;
        strResult = pSource.Substring(intPos, pSource.Length - intPos);
      }

      return strResult;
    }
    
    public static void Guardar(SOLICITUD pobjSolicitud, NucleusProject.Base.Project.Tareas.TAREA pobjTarea, bool estado)
    {
        NomadEnvironment.GetTrace().Info("SOLICITUD: " + pobjSolicitud.ToString());
        NomadEnvironment.GetTrace().Info("TAREA: " + pobjTarea.ToString());
        NomadEnvironment.GetTrace().Info("ESTADO: " + estado.ToString());

        NomadLog.Info("-------- Comienza Guardar --------");

        string strStep = "Seteando Parámetros";
        System.Text.StringBuilder IDS = new System.Text.StringBuilder();
        NomadXML PARAM = new NomadXML("PARAMS");
        PARAM.SetAttr("xval", pobjTarea.oi_pry_imputa);

        NomadTransaction objTran1 = new NomadTransaction();
        try
        {
            objTran1.Begin();
            NomadLog.Info("-------- Grabando Solicitud --------");
            strStep = "Grabando Solicitud";
            objTran1.SaveRefresh(pobjSolicitud);

            objTran1.Commit();
        }
        catch (Exception ex)
        {
            NomadLog.Info("error : " + ex.Message);
            NomadException nmdEx = NomadException.NewInternalException("NucleusProject.Base.Project.Solicitudes.SOLICITUD.Guardar()", ex);
            nmdEx.SetValue("step", strStep);
            objTran1.Rollback();
            throw nmdEx;

        }

        NomadTransaction objTran2 = new NomadTransaction();
        try
        {
            objTran2.Begin();
            NomadLog.Info("-------- Recuperando Solicitud --------");
            strStep = "Recuperando Solicitud";
            NucleusProject.Base.Project.Solicitudes.SOLICITUD DDO = NucleusProject.Base.Project.Solicitudes.SOLICITUD.Get(pobjSolicitud.Id);
            IDS.Append(DDO.Id);

            //Necesito el oi del recurso que esta logueado
            NomadXML log = Nomad.NSystem.Base.NomadEnvironment.QueryNomadXML(NucleusProject.Base.Project.Solicitudes.SOLICITUD.Resources.qry_recurso_log, "");

            //Si se crea una tarea con la solicitud
            if (!pobjTarea.oi_proyectoNull)
            {
                NucleusProject.Base.Project.Tareas.TAREA_TRAZA objTraza1 = NucleusProject.Base.Project.Tareas.TAREA_TRAZA.New();
                NucleusProject.Base.Project.Tareas.TAREA_TRAZA objTraza2 = NucleusProject.Base.Project.Tareas.TAREA_TRAZA.New();

                strStep = "Seteando Tarea";

                pobjTarea.c_tarea = DDO.c_solicitud;
                pobjTarea.d_tarea = DDO.d_solicitud;
                pobjTarea.f_inicio = DDO.f_solicitud;
                pobjTarea.oi_recurso = DDO.oi_recurso;
                pobjTarea.o_tarea = DDO.o_detalle;
                pobjTarea.oi_solicitud = DDO.Id;
                pobjTarea.f_limite = pobjTarea.f_fin_est;

                strStep = "Seteando Traza1";

                objTraza1.c_estado = "I";
                objTraza1.c_causa = "Creacion de la tarea";
                objTraza1.oi_recurso_asi = DDO.oi_recurso;
                objTraza1.oi_recurso = log.FirstChild().GetAttr("oi_recurso");

                //Si el estado de la Solicitud no es Rechazado/Cerrado/CerradoPorElCliente
                if (!estado)
                {
                    strStep = "Seteando Traza2";
                    pobjTarea.c_estado = "T";
                    objTraza2.c_estado = "T";
                    objTraza2.c_causa = "Activada";
                    objTraza2.oi_recurso_asi = DDO.oi_recurso;
                    objTraza2.oi_recurso = log.FirstChild().GetAttr("oi_recurso");
                }
                else
                {
                    strStep = "Seteando Traza2";
                    pobjTarea.c_estado = "F";
                    objTraza2.c_estado = "F";
                    objTraza2.c_causa = "Finalizada";
                    objTraza2.oi_recurso_asi = DDO.oi_recurso;
                    objTraza2.oi_recurso = log.FirstChild().GetAttr("oi_recurso");
                }

                strStep = "Asignado Trazas a la Tarea";

                pobjTarea.TAREAS_TRAZA.Add(objTraza1);
                pobjTarea.TAREAS_TRAZA.Add(objTraza2);

                NomadLog.Info("-------- Grabando Tarea --------");
                strStep = "Grabando Tarea";
                objTran2.SaveRefresh(pobjTarea);

                strStep = "Cambiando estado proyectos padres";

                NomadXML RESULT = Nomad.NSystem.Base.NomadEnvironment.QueryNomadXML(NucleusProject.Base.Project.Solicitudes.SOLICITUD.Resources.qry_proy_padre, PARAM.ToString());
                NucleusProject.Base.Project.Proyectos.PROYECTO.CambiarEstadoPadre(RESULT);
            }

            objTran2.Commit();

           strStep = "LLamada a Evento para envío de mails";
           NucleusRH.Base.Configuracion.LogEventos.LOGEVENTO.Call("NucleusProject.Base.Project.Solicitudes.add_sol", IDS.ToString());
        }
        catch (Exception ex)
        {
            NomadException nmdEx = NomadException.NewInternalException("NucleusProject.Base.Project.Solicitudes.SOLICITUD.Guardar()", ex);
            nmdEx.SetValue("step", strStep);
            objTran2.Rollback();
            throw nmdEx;

        }
    }

    public static void CerrarTareas_Sol(Nomad.NSystem.Proxy.NomadXML param)
    {

        NomadEnvironment.GetTrace().Info("XML: " + param.ToString());

        Nomad.NSystem.Proxy.NomadXML tareas;

        if (param.isDocument == true)
            tareas = param.FirstChild().FirstChild();
        else
            tareas = param.FirstChild();

        NomadEnvironment.GetTrace().Info("Tareas: " + tareas.ToString());

        //Recorrer los ids de tarea
        for (NomadXML tarea = tareas.FirstChild(); tarea != null; tarea = tarea.Next())
        {
            //NomadEnvironment.GetTrace().Info("Tarea: " + tarea.ToString());

            // Por cada id obtenido crear objeto
            NucleusProject.Base.Project.Tareas.TAREA objTarea = NucleusProject.Base.Project.Tareas.TAREA.Get(tarea.GetAttrString("id"));

            NomadEnvironment.GetTrace().Info("Tarea: " + objTarea.c_tarea);

            // y setearle estado, recurso logueado y fecha de fin TAREA
            objTarea.f_fin = DateTime.Now;
            //NomadEnvironment.GetTrace().Info("Fecha Fin: " + objTarea.f_fin);
            objTarea.o_motivo = tarea.GetAttrString("o_motivo");
            objTarea.c_estado = "F";
            //NomadEnvironment.GetTrace().Info("Estado: " + objTarea.c_estado);

            NomadEnvironment.GetTrace().Info("Objeto Tarea: " + objTarea.SerializeAll());

            //NomadEnvironment.GetTrace().Info("Recurso q cambio la tarea (se setea en traza): " + recurso_logueado);

            //crear objeto traza, setearle los atributos necesarios
            NucleusProject.Base.Project.Tareas.TAREA_TRAZA objTraza = new NucleusProject.Base.Project.Tareas.TAREA_TRAZA();

            //objTraza.oi_tarea = objTarea.id; no es necesario ya que lo hace automatico al hacerle add
            objTraza.c_estado = "F";
            objTraza.f_fecha_tarea = DateTime.Now;
            objTraza.c_causa = "Finalizada";

            //JPR - No se pasa más el recurso como parámetro
            string oiPersonal = NomadEnvironment.QueryValue("PER01_PERSONAL", "oi_personal", "oi_usuario_sistema", NomadProxy.GetProxy().GetEtty().GetAttr("id"), "", true);
            string oiRecurso = NomadEnvironment.QueryValue("PRY05_RECURSOS", "oi_recurso", "oi_personal", oiPersonal, "", true);

            objTraza.oi_recurso = oiRecurso;

            objTraza.oi_recurso_asi = objTarea.oi_recurso;
            NomadEnvironment.GetTrace().Info("Objeto Traza: " + objTraza.SerializeAll());

            //agregarlo a la coleccion en tarea
            objTarea.TAREAS_TRAZA.Add(objTraza);

            //hacer el save de cada tarea
            NomadEnvironment.GetCurrentTransaction().Save(objTarea);

            /*catch (Exception e)
            {
                error = error + " - " + e.Message;
                NomadEnvironment.GetTrace().Info(error);
            } */

        }

    }
    public static void CerrarSolicitudes(Nomad.NSystem.Proxy.NomadXML solicitudesSeleccionadas)
    {

        NomadLog.Info("-------- Comienza CerrarSolicitudes --------");

        Nomad.NSystem.Proxy.NomadXML row = null;
        NucleusProject.Base.Project.Solicitudes.SOLICITUD objSolicitud;

        for (row = solicitudesSeleccionadas.FirstChild().FirstChild(); row != null; row = row.Next())
        {
            NomadLog.Info("row:: " + row.GetAttrString("id"));
            objSolicitud = NucleusProject.Base.Project.Solicitudes.SOLICITUD.Get(row.GetAttrString("id"));
            objSolicitud.o_motivo = row.GetAttrString("o_motivo");
            NomadLog.Info("objSolicitud:: " + objSolicitud.SerializeAll());
            objSolicitud.Cerrar();
        }

    }
    public void Cerrar()
    {

        NomadLog.Info("-------- Comienza Cerrar --------");
        NomadXML MyXML;
        if (this.c_estado != "5")
        {
            NomadLog.Debug("Cerrando solicitud:: " + this.c_solicitud);
            this.c_estado = "5";
            this.f_estado = DateTime.Now;
            NomadEnvironment.GetCurrentTransaction().Save(this);
            MyXML = NomadProxy.GetProxy().SQLService().GetXML(SOLICITUD.Resources.qry_tareas_solicitud, this.SerializeAll());
            NomadLog.Debug("Tareas a cerrar:: " + MyXML.ToString());

            if (MyXML.FirstChild().GetAttr("flag") != "0")
                NucleusProject.Base.Project.Solicitudes.SOLICITUD.CerrarTareas_Sol(MyXML);
        }
        else
            NomadLog.Debug("La solicitud:: " + this.c_solicitud + "ya se encuentra cerrada");

    }
    public void send_mails_solicitud()
    {

        NomadLog.Info("-------- Comienza send_mails_solicitudes --------");

        //Obtener Contacto y Cliente
        NucleusProject.Base.Project.Clientes.CONTACTO objContacto = NucleusProject.Base.Project.Clientes.CONTACTO.Get(this.oi_contacto);
        string d_email_contacto = "";
        string d_email = "";

        if (objContacto != null)
        {
            d_email_contacto = objContacto.d_email;
            NucleusProject.Base.Project.Clientes.CLIENTE objCliente = NucleusProject.Base.Project.Clientes.CLIENTE.Get(objContacto.oi_cliente);

            //Obtener Mail del responsable en el cliente:
            if (objCliente != null)
                d_email = objCliente.d_email;
        }

        //Obtengo el Recurso
        string d_email_responsable = "";
        NucleusProject.Base.Project.Recursos.RECURSO objRecurso = NucleusProject.Base.Project.Recursos.RECURSO.Get(this.oi_recurso);
        if (objRecurso != null)
        {
            d_email_responsable = objRecurso.d_email;
            NucleusRH.Base.Personal.Legajo.PERSONAL objPersonal = NucleusRH.Base.Personal.Legajo.PERSONAL.Get(objRecurso.oi_personal);
            if (objPersonal != null)
            {
                if (objPersonal.oi_usuario_sistema != "")
                {
                    Nomad.Base.Login.Entidades.ENTIDAD MyETTY = Nomad.Base.Login.Entidades.ENTIDAD.Get(objPersonal.oi_usuario_sistema);
                    if (MyETTY != null)
                        foreach (Nomad.Base.Login.Entidades.MAIL MyMAIL in MyETTY.MAILS)
                        {
                            if (!MyMAIL.PRINCIPAL) continue;
                            d_email_responsable = MyMAIL.EMAIL;
                        }
                }
            }
        }

        //Obtengo los datos del mail a enviar al personal de Nucleus
        NomadXML xmlInfo_NUC = new NomadXML("PARAM");
        Hashtable htaMail_NUC = GetMails("SOLNEWNUC", this, xmlInfo_NUC);

        string ASUNTO_NUC = htaMail_NUC.ContainsKey("SOLNEWNUC_ASUNTO") ? (string)htaMail_NUC["SOLNEWNUC_ASUNTO"] : "-";
        string CONTENIDO_NUC = htaMail_NUC.ContainsKey("SOLNEWNUC_TEXTO") ? (string)htaMail_NUC["SOLNEWNUC_TEXTO"] : "-";

        this.send_mail_responsable(d_email_responsable, ASUNTO_NUC, CONTENIDO_NUC);

        //Obtengo los datos del mail a enviar al cliente
        if (this.l_envio_mail)
        {
            string mails = d_email_contacto + ";" + d_email;

            NomadXML xmlInfo_CLI = new NomadXML("PARAM");
            Hashtable htaMail_CLI = GetMails("SOLNEWCLI", this, xmlInfo_CLI);

            string ASUNTO_CLI = htaMail_CLI.ContainsKey("SOLNEWCLI_ASUNTO") ? (string)htaMail_CLI["SOLNEWCLI_ASUNTO"] : "-";
            string CONTENIDO_CLI = htaMail_CLI.ContainsKey("SOLNEWCLI_TEXTO") ? (string)htaMail_CLI["SOLNEWCLI_TEXTO"] : "-";

            this.send_mail_solicitante(mails, ASUNTO_CLI, CONTENIDO_CLI);
        }

    }
    public void send_mail_responsable(string d_email_responsable, string asunto, string cuerpo)
    {

        NomadLog.Info("-------- Comienza send_mail_responsable --------");

        Nomad.Base.Mail.Mails.MAIL objMAIL = Nomad.Base.Mail.Mails.MAIL.Crear();

        //Agregar los PARAMETROS
        objMAIL.REMITENTE_DES = "Soporte NucleusRH";
        objMAIL.REMITENTE_DIR = "soporte@nucleussa.com.ar";

        //Asunto
        objMAIL.ASUNTO = asunto;
        objMAIL.PRIORIDAD = 5;
        objMAIL.TIPO = "W";
        objMAIL.CUERPO = cuerpo;

        //Agregar destinatario
        Nomad.Base.Mail.Mails.MAIL.AgregarDestinatario(objMAIL, d_email_responsable, "");

        //Envio el MAIL
        objMAIL.Enviar();

    }
    public void send_mail_solicitante(string mails, string asunto, string cuerpo)
    {

        NomadLog.Info("-------- Comienza send_mail_solicitante --------");

        Nomad.Base.Mail.Mails.MAIL objMAIL = Nomad.Base.Mail.Mails.MAIL.Crear();

        //Agregar los PARAMETROS
        objMAIL.REMITENTE_DES = "Soporte NucleusRH";
        objMAIL.REMITENTE_DIR = "soporte@nucleussa.com.ar";

        //Asunto
        objMAIL.ASUNTO = asunto;
        objMAIL.PRIORIDAD = 5;
        objMAIL.TIPO = "W";
        objMAIL.CUERPO = cuerpo;

        //Agregar destinatario
        NomadLog.Debug("mails:: " + mails);
        string[] arrMails = mails.Split(';');
        if (arrMails.Length > 0)
        {
            for (int i = 0; arrMails.Length > i; i++)
            {
                NomadLog.Debug("arrMails[" + i.ToString() + "]:: " + arrMails[i].ToString());
                if (arrMails[i] != "")
                    Nomad.Base.Mail.Mails.MAIL.AgregarDestinatario(objMAIL, arrMails[i].ToString(), "");
            }
        }

        //Envio el MAIL
        objMAIL.Enviar();

    }
    public static void alertas_solicitudes()
    {

        NomadLog.Info("-------- Comienza alertas solicitudes --------");

        NomadXML MyXMLSol = NomadProxy.GetProxy().SQLService().GetXML(SOLICITUD.Resources.alertaSolicitudes, "");
        NomadLog.Debug("MyXMLSol:: " + MyXMLSol.ToString());
        for (NomadXML rowSolicitud = MyXMLSol.FirstChild(); rowSolicitud != null; rowSolicitud = rowSolicitud.Next())
        {
            NomadLog.Debug("rowSolicitud:: " + rowSolicitud.ToString());
            NucleusProject.Base.Project.Solicitudes.SOLICITUD objSolicitud = NucleusProject.Base.Project.Solicitudes.SOLICITUD.Get(rowSolicitud.GetAttr("id"));

            //Obtener Contacto y Cliente
            NucleusProject.Base.Project.Clientes.CONTACTO objContacto = NucleusProject.Base.Project.Clientes.CONTACTO.Get(objSolicitud.oi_contacto);
            string c_contacto = "";
            string d_nom_y_ape = "";
            string d_email_contacto = "";
            string c_cliente = "";
            string d_nom_cliente = "";
            string d_nom_contacto = "";
            string d_email = "";
            string f_fecha = "";
            string hs_pend = "";
            string f_fecha_hora = "";

            if (objContacto != null)
            {
                c_contacto = objContacto.c_contacto;
                d_nom_y_ape = objContacto.d_nom_y_ape;
                d_email_contacto = objContacto.d_email;
                NucleusProject.Base.Project.Clientes.CLIENTE objCliente = NucleusProject.Base.Project.Clientes.CLIENTE.Get(objContacto.oi_cliente);

                //Obtener Horas pendientes:
                if (objCliente != null)
                {
                    c_cliente = objCliente.c_cliente;
                    d_nom_cliente = objCliente.d_nom_cliente;
                    d_nom_contacto = objCliente.d_nom_contacto;
                    d_email = objCliente.d_email;
                    NomadXML MyXML = NomadProxy.GetProxy().SQLService().GetXML(SOLICITUD.Resources.qry_hrs_pendientes, "<DATA oi_cliente=\"" + objCliente.Id + "\"/>");
                    NomadLog.Debug("MyXML:: " + MyXML.ToString());
                    //f_fecha=MyXML.GetAttr("f_fecha");
                    f_fecha = rowSolicitud.GetAttr("f_solicitud");
                    f_fecha_hora = MyXML.GetAttr("f_fecha_hora");
                    hs_pend = MyXML.GetAttr("hs_pend");
                }
            }

            //Obtener Modulo y Aplicacion
            string c_aplicacion = "";
            string d_aplicacion = "";
            string d_nombre_modulo = "";
            NucleusProject.Base.Project.Aplicaciones.MODULO objModulo = NucleusProject.Base.Project.Aplicaciones.MODULO.Get(objSolicitud.oi_modulo);

            if (objModulo != null)
            {
                d_nombre_modulo = objModulo.d_nombre;
                NucleusProject.Base.Project.Aplicaciones.APLICACION objAplicacion = NucleusProject.Base.Project.Aplicaciones.APLICACION.Get(objModulo.oi_aplicacion);
                if (objAplicacion != null)
                {
                    c_aplicacion = objAplicacion.c_aplicacion;
                    d_aplicacion = objAplicacion.d_aplicacion;
                }
            }

            //Obtener Tarea y Proyecto
            string c_proyecto = "";
            string d_proyecto = "";

            NucleusProject.Base.Project.Tareas.TAREA objTarea = NucleusProject.Base.Project.Tareas.TAREA.Get(NomadEnvironment.QueryValue("PRY02_TAREAS", "oi_tarea", "oi_solicitud", objSolicitud.Id, "", false));
            if (objTarea != null)
            {
                NomadLog.Info("objTarea:: " + objTarea.SerializeAll());
                NucleusProject.Base.Project.Proyectos.PROYECTO objProyecto = NucleusProject.Base.Project.Proyectos.PROYECTO.Get(objTarea.oi_proyecto);

                if (objProyecto != null)
                {
                    c_proyecto = objProyecto.c_proyecto;
                    d_proyecto = objProyecto.d_proyecto;
                }
            }

            //Obtener Error
            string c_error = "";
            string d_nombre = "";
            NucleusProject.Base.Project.Errores.ERROR objError = NucleusProject.Base.Project.Errores.ERROR.Get(objSolicitud.oi_error);
            if (objError != null)
            {
                c_error = objError.c_error;
                d_nombre = objError.d_nombre;
            }

            //Obtengo el Recurso
            string d_email_responsable = "";
            NucleusProject.Base.Project.Recursos.RECURSO objRecurso = NucleusProject.Base.Project.Recursos.RECURSO.Get(objSolicitud.oi_recurso);
            if (objRecurso != null)
            {
                d_email_responsable = objRecurso.d_email;
                NucleusRH.Base.Personal.Legajo.PERSONAL objPersonal = NucleusRH.Base.Personal.Legajo.PERSONAL.Get(objRecurso.oi_personal);
                if (objPersonal != null)
                {
                    if (objPersonal.oi_usuario_sistema != "")
                    {
                        Nomad.Base.Login.Entidades.ENTIDAD MyETTY = Nomad.Base.Login.Entidades.ENTIDAD.Get(objPersonal.oi_usuario_sistema);
                        if (MyETTY != null)
                            foreach (Nomad.Base.Login.Entidades.MAIL MyMAIL in MyETTY.MAILS)
                            {
                                if (!MyMAIL.PRINCIPAL) continue;
                                d_email_responsable = MyMAIL.EMAIL;
                            }
                    }
                }
            }

            string estado = "";
            switch (objSolicitud.c_estado)
            {
                case "1":
                    estado = "PENDIENTE";
                    break;
                case "2":
                    estado = "DERIVADO A PMO";
                    break;
                case "3":
                    estado = "DERIVADO PARA ESTIMACION/DESARROLLO";
                    break;
                case "4":
                    estado = "DERIVADO A CLIENTE";
                    break;
                case "5":
                    estado = "CERRADO";
                    break;
                case "6":
                    estado = "CERRADO POR EL CLIENTE";
                    break;
                case "7":
                    estado = "RECHAZADO";
                    break;
                default:
                    estado = "NO DEFINIDO";
                    break;
            }

            string asunto = "Area Soporte - Sol. " + objSolicitud.c_solicitud + " - E/D - Cliente: " + c_cliente + " - " + d_nom_cliente + " - " + objSolicitud.d_solicitud;
            string cuerpo = "** Código Solicitud: **" + objSolicitud.c_solicitud + @"\\ " +
                                    "** Fecha: **" + f_fecha + @"\\ " +
                                    "** Usuario Emisor: **[" + c_contacto + "] " + d_nom_y_ape + " (" + d_email_contacto + @")\\ " +
                                    "** Contacto Responsable: **" + d_nom_contacto + " (" + d_email + @")\\ " +
                                    "** Cliente: **[" + c_cliente + "] " + d_nom_cliente + @"\\ " +
                                    "** Proyecto: **[" + c_proyecto + @"] " + d_proyecto + @"\\ " +
                                    "** Módulo: **[" + c_aplicacion + @"] " + d_aplicacion + " ~ " + d_nombre_modulo + @"\\ " +
                                    "** Detalle: **" + objSolicitud.d_solicitud + @"\\ " +
                                    "** Criticidad: **[" + c_error + @"] " + d_nombre + @"\\ " +
                //"** Incorporación al Base: **" + (this.l_inc_a_base=="1")?"SI":"NO" + "\\" +
                                    "** Estado: **" + estado + @"\\ " +
                                    "** Horas Pendientes: **" + hs_pend + "\n\n\n\n" +
                                    @"**~~~~~~~Mail Original~~~~~~~** " + "\n\n\n\n" +
                                    objSolicitud.o_detalle + @"\\ " +
                                    objSolicitud.o_observaciones;

            objSolicitud.send_mail_responsable(d_email_responsable, asunto, cuerpo);
        }

    }
    public static void CambiarResponsables(Nomad.NSystem.Proxy.NomadXML solicitudesSeleccionadas, Nomad.NSystem.Proxy.NomadXML recurso)
    {

        NomadLog.Info("-------- Comienza cambiar recurso --------");

        for (NomadXML rowSolicitud = solicitudesSeleccionadas.FirstChild().FirstChild(); rowSolicitud != null; rowSolicitud = rowSolicitud.Next())
        {
            NomadLog.Debug("rowSolicitud:: " + rowSolicitud.ToString());
            NucleusProject.Base.Project.Solicitudes.SOLICITUD objSolicitud = NucleusProject.Base.Project.Solicitudes.SOLICITUD.Get(rowSolicitud.GetAttr("id"));

            //Recurso anterior
            NucleusProject.Base.Project.Recursos.RECURSO objRecursoAnt = NucleusProject.Base.Project.Recursos.RECURSO.Get(objSolicitud.oi_recurso);
            NomadLog.Debug("objRecursoAnt:: " + objRecursoAnt.ToString());

            //Obtengo el Recurso Anterior
            string d_email_responsable_ant = "";
            if (objRecursoAnt != null)
            {
                d_email_responsable_ant = objRecursoAnt.d_email;
                NucleusRH.Base.Personal.Legajo.PERSONAL objPersonalAnt = NucleusRH.Base.Personal.Legajo.PERSONAL.Get(objRecursoAnt.oi_personal);
                NomadLog.Debug("objPersonalAnt:: " + objPersonalAnt.ToString());
                if (objPersonalAnt != null)
                {
                    if (objPersonalAnt.oi_usuario_sistema != "")
                    {
                        try
                        {
                            Nomad.Base.Login.Entidades.ENTIDAD MyETTYAnt = Nomad.Base.Login.Entidades.ENTIDAD.Get(objPersonalAnt.oi_usuario_sistema);

                            NomadLog.Debug("MyETTYAnt:: " + MyETTYAnt.ToString());
                            if (MyETTYAnt != null)
                                foreach (Nomad.Base.Login.Entidades.MAIL MyMAIL in MyETTYAnt.MAILS)
                                {
                                    if (!MyMAIL.PRINCIPAL) continue;
                                    d_email_responsable_ant = MyMAIL.EMAIL;
                                }

                        }
                        catch (Exception ex)
                        {
                            throw new Exception("Se ha producido un error al intentar recuperar el mail del Recurso anterior, ya que este posee una entidad inválida\n" + ex.Message);
                        }
                    }
                }
            }

            objSolicitud.oi_recurso = recurso.FirstChild().GetAttr("oi_recurso");

            //Obtengo el Recurso
            string d_email_responsable = "";
            NucleusProject.Base.Project.Recursos.RECURSO objRecurso = NucleusProject.Base.Project.Recursos.RECURSO.Get(recurso.FirstChild().GetAttr("oi_recurso"));
            if (objRecurso != null)
            {
                d_email_responsable = objRecurso.d_email;
                NucleusRH.Base.Personal.Legajo.PERSONAL objPersonal = NucleusRH.Base.Personal.Legajo.PERSONAL.Get(objRecurso.oi_personal);
                if (objPersonal != null)
                {
                    if (objPersonal.oi_usuario_sistema != "")
                    {
                        try
                        {
                            Nomad.Base.Login.Entidades.ENTIDAD MyETTY = Nomad.Base.Login.Entidades.ENTIDAD.Get(objPersonal.oi_usuario_sistema);
                            if (MyETTY != null)
                                foreach (Nomad.Base.Login.Entidades.MAIL MyMAIL in MyETTY.MAILS)
                                {
                                    if (!MyMAIL.PRINCIPAL) continue;
                                    d_email_responsable = MyMAIL.EMAIL;
                                }
                        }
                        catch (Exception ex)
                        {
                            throw new Exception("Se ha producido un error al intentar recuperar el mail del Recurso anterior, ya que este posee una entidad inválida\n" + ex.Message);
                        }
                    }
                }
            }

            //Obtengo los datos del mail a enviar al personal de Nucleus
            NomadXML xmlInfo_NUC = new NomadXML("PARAM");
            Hashtable htaMail_NUC = GetMails("SOLNEWNUC", objSolicitud, xmlInfo_NUC);

            string ASUNTO_NUC = htaMail_NUC.ContainsKey("SOLNEWNUC_ASUNTO") ? (string)htaMail_NUC["SOLNEWNUC_ASUNTO"] : "-";
            string CONTENIDO_NUC = htaMail_NUC.ContainsKey("SOLNEWNUC_TEXTO") ? (string)htaMail_NUC["SOLNEWNUC_TEXTO"] : "-";

            objSolicitud.send_mail_responsable(d_email_responsable, ASUNTO_NUC, CONTENIDO_NUC);
            NomadEnvironment.GetCurrentTransaction().Save(objSolicitud);

            //QRY que recupera datos del usuario logueado
            NomadXML MyXML = NomadProxy.GetProxy().SQLService().GetXML(SOLICITUD.Resources.qry_proxy, "<DATA />");
            NomadLog.Debug("MyXML:: " + MyXML.ToString());

            //ALERTA CAMBIO DE RESPONSABLE - MAIL a RESP ANT
            NomadLog.Debug("--PARAMETROS ALERTA CAMBIO RESPONSABLE ANTERIOR---");
            NomadLog.Debug("d_email_responsable_ant:: " + d_email_responsable_ant.ToString());
            NomadLog.Debug("oi_recurso_ant: " + objRecursoAnt.id.ToString());
            NomadLog.Debug("oi_recurso: " + objRecurso.id.ToString());
            NomadLog.Debug("codUsr: " + MyXML.GetAttr("COD_USR_EMISOR").ToString());
            NomadLog.Debug("desUsr: " + MyXML.GetAttr("DES_USR_EMISOR").ToString());
            NomadLog.Debug("mailUsr: " + MyXML.GetAttr("MAIL_USR_EMISOR").ToString());

            //Evento para ejecutar alerta
            System.Text.StringBuilder IDS = new System.Text.StringBuilder();
            IDS.Append(objSolicitud.id);
            NomadLog.Debug("IDS: " + IDS.ToString());
            NucleusRH.Base.Configuracion.LogEventos.LOGEVENTO.Call("NucleusProject.Base.Project.Solicitudes.chg_responsable", IDS.ToString(), objRecursoAnt.id.ToString(), objRecurso.id.ToString(), d_email_responsable_ant.ToString(), MyXML.GetAttr("COD_USR_EMISOR").ToString(), MyXML.GetAttr("DES_USR_EMISOR").ToString(), MyXML.GetAttr("MAIL_USR_EMISOR").ToString());
        }

    }
    public static void CerrarSolicitud(int oi_solicitud, string o_motivo)
    {

        NomadXML MyXML = NomadProxy.GetProxy().SQLService().GetXML(SOLICITUD.Resources.TAREAS, "<DATA oi_solicitud=\"" + oi_solicitud + "\"/>");
        NomadLog.Debug("SQL TAREAS return " + MyXML.ToString());

        if (MyXML.GetAttrInt("cant") == 0)
        {
            SOLICITUD MySOL = SOLICITUD.Get(oi_solicitud);
            if (o_motivo != "")
                MySOL.o_motivo = o_motivo;

            MySOL.f_estado = DateTime.Now;

            if (MyXML.GetAttrInt("cant-F") == 0)
            {
                NomadLog.Debug("Rechazando Solicitud " + oi_solicitud);
                MySOL.c_estado = "7";
            }
            else
            {
                NomadLog.Debug("Cerrando Solicitud " + oi_solicitud);
                MySOL.c_estado = "5";
            }

            NomadEnvironment.GetCurrentTransaction().Save(MySOL);
        }

    }

    public static void ReporteAtencionAlCliente(ref Nomad.NSystem.Proxy.NomadXML DOC, Nomad.NSystem.Proxy.NomadXML Idsel)
    {
        NomadLog.Debug("----------------------------------------------------------");
        NomadLog.Debug("------------------AtencionAlCliente V1-----------------");
        NomadLog.Debug("----------------------------------------------------------");

        NomadLog.Debug("ParametroDOC: " + DOC.ToString());
        NomadLog.Debug("ParametroIDsel: " + Idsel.ToString());

        string strStep = "";
        try
        {
            strStep = "RECORRO-IDSel";

            //Bandera para evaluar si debo combinar los dos documentos
            bool blMergear = false;

            if (Idsel.isDocument)
            {
                if (Idsel.FirstChild().ChildLength != 0)
                {
                    blMergear = true;
                }
            }
            else
            {
                if (Idsel.ChildLength != 0)
                {
                    blMergear = true;
                }
            }

            if (blMergear)
            {
                StringBuilder strAttrID = new StringBuilder("IN ( ");

                for (NomadXML row = Idsel.FirstChild().FirstChild(); row != null; row = row.Next())
                {
                    string strID = row.GetAttrString("id");

                    strStep = "CONCATENO-IDS-" + strID;

                    strAttrID = strAttrID.Append(strID + ",");

                }

                strAttrID = strAttrID.Remove(strAttrID.Length - 1, 1);
                strAttrID = strAttrID.Append(" )");

                //IDs="IN(1,2)"
                string strAttrID2 = strAttrID.ToString();

                strStep = "SETEO-IDS-DOC";

                DOC.FirstChild().SetAttr("IDs", strAttrID2);
                string resultado = DOC.FirstChild().GetAttrString("IDs");
            }
        }
        catch (Exception ex)
        {
            NomadException nmdEx = new NomadException("NucleusProject.Base.Project.Solicitudes.AtencionAlCliente()", ex);
            nmdEx.SetValue("Step", strStep);

            throw nmdEx;
        }
    }

    public static void DerivarSolicitudes(Nomad.NSystem.Proxy.NomadXML solicitudesSeleccionadas)
    {

        NomadLog.Info("-------- Comienza DerivarSolicitudes --------");

        Nomad.NSystem.Proxy.NomadXML row = null;
        NucleusProject.Base.Project.Solicitudes.SOLICITUD objSolicitud;

        for (row = solicitudesSeleccionadas.FirstChild().FirstChild(); row != null; row = row.Next())
        {
            NomadLog.Info("row:: " + row.GetAttrString("id"));
            objSolicitud = NucleusProject.Base.Project.Solicitudes.SOLICITUD.Get(row.GetAttrString("id"));
            NomadLog.Info("objSolicitud:: " + objSolicitud.SerializeAll());

            if (objSolicitud.c_estado != "4")
            {
                NomadLog.Debug("Derivando solicitud:: " + objSolicitud.c_solicitud);
                objSolicitud.c_estado = "4";
                objSolicitud.f_estado = DateTime.Now;
                objSolicitud.o_motivo = row.GetAttrString("o_motivo");
                NomadEnvironment.GetCurrentTransaction().Save(objSolicitud);
            }
            else
                NomadLog.Debug("La solicitud:: " + objSolicitud.c_solicitud + "ya se encuentra derivada al cliente");
        }

    }

    public static void RechazarSolicitudes(Nomad.NSystem.Proxy.NomadXML solicitudesSeleccionadas)
    {

        NomadLog.Info("-------- Comienza RechazarSolicitudes --------");

        Nomad.NSystem.Proxy.NomadXML row = null;
        NucleusProject.Base.Project.Solicitudes.SOLICITUD objSolicitud;

        for (row = solicitudesSeleccionadas.FirstChild().FirstChild(); row != null; row = row.Next())
        {
            NomadLog.Info("row:: " + row.GetAttrString("id"));
            objSolicitud = NucleusProject.Base.Project.Solicitudes.SOLICITUD.Get(row.GetAttrString("id"));
            objSolicitud.o_motivo = row.GetAttrString("o_motivo");
            NomadLog.Info("objSolicitud:: " + objSolicitud.SerializeAll());
            objSolicitud.Rechazar();
        }

    }
    public void Rechazar()
    {

        NomadLog.Info("-------- Comienza Rechazar --------");
        NomadXML MyXML;
        if (this.c_estado != "7")
        {
            NomadLog.Debug("Rechazando solicitud:: " + this.c_solicitud);
            this.c_estado = "7";
            this.f_estado = DateTime.Now;
            NomadEnvironment.GetCurrentTransaction().Save(this);
            MyXML = NomadProxy.GetProxy().SQLService().GetXML(SOLICITUD.Resources.qry_tareas_solicitud, this.SerializeAll());
            NomadLog.Debug("Tareas a rechazar:: " + MyXML.ToString());

            if (MyXML.FirstChild().GetAttr("flag") != "0")
                NucleusProject.Base.Project.Solicitudes.SOLICITUD.AnularTareas_Sol(MyXML);
        }
        else
            NomadLog.Debug("La solicitud:: " + this.c_solicitud + "ya se encuentra rechazada");

    }

    public static void AnularTareas_Sol(Nomad.NSystem.Proxy.NomadXML param)
    {

        NomadEnvironment.GetTrace().Info("XML: " + param.ToString());

        Nomad.NSystem.Proxy.NomadXML tareas;

        if (param.isDocument == true)
            tareas = param.FirstChild().FirstChild();
        else
            tareas = param.FirstChild();

        NomadEnvironment.GetTrace().Info("Tareas: " + tareas.ToString());

        //Recorrer los ids de tarea
        for (NomadXML tarea = tareas.FirstChild(); tarea != null; tarea = tarea.Next())
        {
            // Por cada id obtenido crear objeto
            NucleusProject.Base.Project.Tareas.TAREA objTarea = NucleusProject.Base.Project.Tareas.TAREA.Get(tarea.GetAttrString("id"));

            NomadEnvironment.GetTrace().Info("Tarea: " + objTarea.c_tarea);

            // y setearle estado, recurso logueado y fecha de fin TAREA
            objTarea.f_fin = DateTime.Now;
            objTarea.o_motivo = tarea.GetAttrString("o_motivo");
            objTarea.c_estado = "A";

            NomadEnvironment.GetTrace().Info("Objeto Tarea: " + objTarea.SerializeAll());

            //crear objeto traza, setearle los atributos necesarios
            NucleusProject.Base.Project.Tareas.TAREA_TRAZA objTraza = new NucleusProject.Base.Project.Tareas.TAREA_TRAZA();

            objTraza.c_estado = "A";
            objTraza.f_fecha_tarea = DateTime.Now;
            objTraza.c_causa = "Anulada";

            string oiPersonal = NomadEnvironment.QueryValue("PER01_PERSONAL", "oi_personal", "oi_usuario_sistema", NomadProxy.GetProxy().GetEtty().GetAttr("id"), "", true);
            string oiRecurso = NomadEnvironment.QueryValue("PRY05_RECURSOS", "oi_recurso", "oi_personal", oiPersonal, "", true);

            objTraza.oi_recurso = oiRecurso;

            objTraza.oi_recurso_asi = objTarea.oi_recurso;
            NomadEnvironment.GetTrace().Info("Objeto Traza: " + objTraza.SerializeAll());

            //agregarlo a la coleccion en tarea
            objTarea.TAREAS_TRAZA.Add(objTraza);

            //hacer el save de cada tarea
            NomadEnvironment.GetCurrentTransaction().Save(objTarea);

        }

    }

  }
}



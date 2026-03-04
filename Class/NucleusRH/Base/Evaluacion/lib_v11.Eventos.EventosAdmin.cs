using System;
using System.Xml;
using System.Collections;

using Nomad.NSystem.Proxy;
using Nomad.NSystem.Functions;
using Nomad.NSystem.Base;

using NucleusRH.Base.Evaluacion.Eventos;
using Nomad.Base.Mail.OutputMails;

namespace NucleusRH.Base.Evaluacion
{

    public class EventosAdmin
    {

        private NomadProxy m_Proxy;

        private string m_strOi_Evento;
        private string m_strOi_Empresa;
        private string m_strOi_Clase_org;
        private string m_strC_clase_eval;
        private string m_strPeople_List;

        private BatchService objBatch;
        private TraceService objTrace;

        private NomadTrace trace;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="pobjProxy"></param>
        public EventosAdmin(NomadProxy pobjProxy)
        {
            m_Proxy = pobjProxy;
            objBatch = this.m_Proxy.Batch();
            objTrace = objBatch.Trace;
            trace = NomadEnvironment.GetTrace();
        }

        /// <summary>
        /// Crea las evaliaciones para un EVENTO en particular
        /// </summary>
        /// <param name="pstrParams"></param>
        /// <returns></returns>
        public string CrearEvaluaciones(string pstrParams)
        {
            string strGeneralResult = "";
            string strPeopleResult = "";
            string strPersonalList = "-1";
            EVENTO objEvento = new EVENTO();

            bool bolPreLoadOk = false;

            XmlDocument xmlPers = new XmlDocument();
            XmlDocument xmlResp = new XmlDocument();

            try
            {
                this.objBatch.SetProgress(0);

                //Parsea los valores ----------------------------------------------------------------
                this.objTrace.Add("ifo", "Parseando los parámetros de entrada.", "CrearEvaluaciones");
                trace.Debug("Parseando los parámetros de entrada.");
                ParseParams(pstrParams);
                this.objBatch.SetProgress(5);

                //Obtiene datos del EVENTO -----------------------------------------------------------
                this.objTrace.Add("ifo", "Obteniendo datos del Evento.", "CrearEvaluaciones");
                trace.Debug("Obteniendo datos del Evento.");
                objEvento = EVENTO.Get(this.m_strOi_Evento);
                this.m_strOi_Empresa = objEvento.oi_empresa;
                this.m_strC_clase_eval = objEvento.c_clase_eval;
                this.objBatch.SetProgress(10);

                //Obtiene el OI de Clase Organizativa -------------------------------------------------
                this.objTrace.Add("ifo", "Obteniendo datos de la Clase Organizativa.", "CrearEvaluaciones");
                trace.Debug("Obteniendo datos de la Clase Organizativa.");
                GetOrgClass();
                this.objBatch.SetProgress(15);

                // Obtiene los querys -----------------------------------------------------------------
                //En este primer query están los datos de las personas asociadas con su estructura
                //Tambien desde aqui puede verse cual es la estructura padre de una estructura de una persona en particular
                string strTemp;
                this.objTrace.Add("ifo", "Obteniendo datos del Personal y las Estructuras asociadas.", "CrearEvaluaciones");
                trace.Debug("Obteniendo datos del Personal y las Estructuras asociadas.");
                strTemp = GetPersonalEmpData();
                xmlPers.LoadXml(strTemp);

                if (xmlPers.DocumentElement.ChildNodes.Count == 0)
                    throw new Exception("No se encontraron personas...");

                //En este query están los datos de todos los responsables de la clase organizativa en cuestión
                strTemp = GetRespData();
                xmlResp.LoadXml(strTemp);

                if (xmlResp.DocumentElement.ChildNodes.Count == 0)
                    throw new Exception("No se encontraron responsables...");

                bolPreLoadOk = true;
                this.objBatch.SetProgress(20);

            }
            catch (Exception ex)
            {
                this.objTrace.Add("err", ex.Message, "CrearEvaluaciones/Catch1");
                trace.Debug("ERROR: " + ex.Message);
                strGeneralResult = "<RESULTADO resultado=\"ERR\" errdesc=\"Error en la seccion PreLoad. " + ex.Message + "\" />";
            }

            /*--------------------------------------------------------------------------------------------*/
            /*--------------------------------------------------------------------------------------------*/
            /*--------------------------------------------------------------------------------------------*/
            /*--------------------------------------------------------------------------------------------*/
            /*--------------------------------------------------------------------------------------------*/
            /*--------------------------------------------------------------------------------------------*/

            if (bolPreLoadOk)
            {

                this.objTrace.Add("ifo", "Comienza a crear las evaluaciones.", "CrearEvaluaciones");
                trace.Debug("Comienza a crear las evaluaciones.");

                strGeneralResult = "<RESULTADO resultado=\"OK\" errdesc=\"\" />";
                try
                {
                    bool bolHasError = false;
                    bool bolSomeWell = false;

                    //Recorre las personas y crea una evaluacion por cada una
                    int intChildCount = xmlPers.DocumentElement.ChildNodes.Count;
                    int intCounter = 0;
                    foreach (XmlElement xelPer in xmlPers.DocumentElement.ChildNodes)
                    {
                        intCounter++;

                        //Se agrega el atributo errdesc para poder entragarlo en el resultado
                        xelPer.Attributes.Append(xmlPers.CreateAttribute("errdesc"));
                        try
                        {
                            this.objTrace.Add("ifo", "Creando evaluación para el legajo '" + xelPer.GetAttribute("e_numero_legajo") + "'.", "CrearEvaluaciones");
                            trace.Debug("Creando evaluacion para : " + xelPer.GetAttribute("oi_personal_emp"));

                            //Acumula los PersonalEmps
                            strPersonalList = strPersonalList + ", " + xelPer.GetAttribute("oi_personal_emp");
                            CreateEvaluation(xelPer, xmlResp);

                            //Indica que al menos una evaluación se pudo crear correctamente.
                            bolSomeWell = true;

                        }
                        catch (Exception ex)
                        {
                            this.objTrace.Add("err", ex.Message, "CrearEvaluaciones/Catch2");
                            trace.Debug("ERROR: " + ex.Message);
                            xelPer.Attributes["errdesc"].Value = ex.Message;
                            bolHasError = true;
                        }

                        strPeopleResult = strPeopleResult + xelPer.OuterXml;
                        this.objBatch.SetProgress(((80 / intChildCount) * intCounter) + 20);

                    }

                    if (bolHasError)
                        strGeneralResult = "<RESULTADO resultado=\"ERR\" errdesc=\"Se produjeron errores con algunas personas.\" />";
                    else
                        strGeneralResult = "<RESULTADO resultado=\"OK\" errdesc=\"\" />";

                    if (bolSomeWell)
                    {
                        //Si alguna evaluación está correcta pone el estado del Evento en Iniciado
                        //objEvento.c_estado = "I";
                        //NomadEnvironment.GetCurrentTransaction().Save(objEvento);

                        if (objEvento.l_notif_ini == true)
                        {
                            //Envia los mails a los evaluadores
                            this.objTrace.Add("ifo", "Enviando mails a los evaluadores.", "CrearEvaluaciones");
                            SendMails(objEvento, strPersonalList);
                        }

                    }

                }
                catch (Exception ex)
                {
                    this.objTrace.Add("err", ex.Message, "CrearEvaluaciones/Catch3");
                    trace.Debug("ERROR: " + ex.Message);
                    strGeneralResult = "<RESULTADO resultado=\"ERR\" errdesc=\"Error en la seccion Begin/Commit. " + ex.Message + "\" />";
                }
            }

            string strResult = "<DATOS>" +
                                    strGeneralResult +
                                    "<PERSONAS>" +
                                        strPeopleResult +
                                    "</PERSONAS>" +
                                "</DATOS>";

            trace.Debug("RESULTADO: " + strResult);

            //this.objTrace.Add("ifo", "Proceso finalizado.", "CrearEvaluaciones");
            this.objBatch.SetProgress(100);

            return strResult;

        }

        /// <summary>
        /// Obtiene los valores dentro del XML de parámetros
        /// </summary>
        /// <param name="pstrParams"></param>
        /// <returns></returns>
        public void ParseParams(string pstrParams)
        {
            XmlDocument xmlParams;
            XmlElement xelElement;

            //Parsea los valores ----------------------------------------------------------------
            if (pstrParams.Length == 0)
                throw new Exception("Parámetros de entrada inválidos.");

            xmlParams = new XmlDocument();
            xmlParams.LoadXml(pstrParams);

            if (xmlParams.DocumentElement.Name == "PERSONAS")
                xmlParams.LoadXml(pstrParams);

            xelElement = (XmlElement)xmlParams.DocumentElement.ChildNodes.Item(0);

            //Analiza el elemento DATOS/FILTRO
            if (xelElement.Name != "FILTRO")
                throw new Exception("No se encontró el elemento 'FILTRO'.");

            //Obtiene el OI_EVENTO
            if (xelElement.HasAttribute("oi_evento"))
            {
                m_strOi_Evento = xelElement.Attributes["oi_evento"].Value;
            }
            else
            {
                throw new Exception("No se encontró el atributo 'oi_evento' dentro del elemento 'FILTRO' en los parámetros.");
            }

            //Obtiene el elemento PERSONAS
            if (xelElement.ChildNodes.Count > 0)
            {
                XmlElement xelPersonas = (XmlElement)xelElement.ChildNodes.Item(0);

                //Genera la lista de personas para poder ser utilizada en los queries
                this.m_strPeople_List = "";
                foreach (XmlElement xelPersona in xelPersonas.ChildNodes)
                {
                    this.m_strPeople_List = this.m_strPeople_List + xelPersona.Attributes["id"].Value + ", ";
                }

                if (m_strPeople_List.Length > 2)
                    m_strPeople_List = m_strPeople_List.Substring(0, m_strPeople_List.Length - 2);
                else
                    throw new Exception("No se entrontraron personas dentro del elemento PERSONAS.");

            }
            else
            {
                throw new Exception("No se entrontró el elemento PERSONAS dentro de los parámetros.");
            }

        }

        /// <summary>
        /// Crea una evalucion
        /// </summary>
        /// <param name="pxelPer">Elemento que contiene los datos de la persona</param>
        /// <param name="pxmlPers">Documento XML que contiene la lista estructuras con sus responsables</param>
        /// <returns></returns>
        private string CreateEvaluation(XmlElement pxelPer, XmlDocument pxmlResps)
        {
            string strTipoEval;
            string strOi_Evaluador = "";
            string strOi_pos_Evaluador = "";
            string strOi_Supervisor = "";
            string strOi_pos_Supervisor = "";
            string strEstructSup;

            XmlNode xnoResp;

            NucleusRH.Base.Evaluacion.Evaluacion.EVALUACION objEva = new NucleusRH.Base.Evaluacion.Evaluacion.EVALUACION();

            //Pregunta si es responsable de su area para obtener el tipo de evaluación
            //strTipoEval = pxelPer.GetAttribute("l_responsable") == "1" ? "GER" : "IND";
            strTipoEval = pxelPer.GetAttribute("l_responsable") == "1" ? "1" : "2";

            //Obtiene los datos del EVALUADOR ---------------------------------------------------------------------
            trace.Debug("Obtiene los datos del EVALUADOR");
            //Obtiene la estructura padre de la PERSONA y la busca en la lista de estructuras y responsables
            strEstructSup = pxelPer.GetAttribute("oi_estr_padre");
            xnoResp = pxmlResps.SelectSingleNode("Resps/Resp[@oi_estructura = '" + strEstructSup + "']");
            if (((XmlElement)xnoResp).HasAttribute("oi_personal_emp"))
            {
                strOi_Evaluador = ((XmlElement)xnoResp).GetAttribute("oi_personal_emp");
                strOi_pos_Evaluador = ((XmlElement)xnoResp).GetAttribute("oi_posicion_ult");
            }
            else
            {
                trace.Debug("Buscando responsable 2 niveles arriba");
                XmlNode xnoParent;
                xnoParent = pxmlResps.SelectSingleNode("Resps/Resp[@oi_estructura = '" + ((XmlElement)xnoResp).GetAttribute("oi_estr_padre") + "']/@oi_estructura");
                if (xnoParent != null)
                    strEstructSup = xnoParent.Value;

                xnoResp = pxmlResps.SelectSingleNode("Resps/Resp[@oi_estructura = '" + strEstructSup + "']");
                if (((XmlElement)xnoResp).HasAttribute("oi_personal_emp"))
                {
                    strOi_Evaluador = ((XmlElement)xnoResp).GetAttribute("oi_personal_emp");
                    strOi_pos_Evaluador = ((XmlElement)xnoResp).GetAttribute("oi_posicion_ult");
                }
            }

            if (strOi_Evaluador == "" || strOi_pos_Evaluador == "")
            {
                throw new Exception("No se encontraron los datos del evaluador.");
            }

            //Obtiene los datos del SUPERVISOR ---------------------------------------------------------------------
            trace.Debug("Obtiene los datos del SUPERVISOR");
            //Obtiene la estructura padre del EVALUADOR y la busca en la lista de estructuras y responsables
            strEstructSup = ((XmlElement)xnoResp).GetAttribute("oi_estr_padre");
            xnoResp = pxmlResps.SelectSingleNode("Resps/Resp[@oi_estructura = '" + strEstructSup + "']");
            if (((XmlElement)xnoResp).HasAttribute("oi_personal_emp"))
            {
                strOi_Supervisor = ((XmlElement)xnoResp).GetAttribute("oi_personal_emp");
                strOi_pos_Supervisor = ((XmlElement)xnoResp).GetAttribute("oi_posicion_ult");
            }
            else
            {
                trace.Debug("Buscando supervisor 2 niveles arriba");
                XmlNode xnoParent;
                xnoParent = pxmlResps.SelectSingleNode("Resps/Resp[@oi_estructura = '" + ((XmlElement)xnoResp).GetAttribute("oi_estr_padre") + "']/@oi_estructura");
                if (xnoParent != null)
                    strEstructSup = xnoParent.Value;

                xnoResp = pxmlResps.SelectSingleNode("Resps/Resp[@oi_estructura = '" + strEstructSup + "']");
                if (((XmlElement)xnoResp).HasAttribute("oi_personal_emp"))
                {
                    strOi_Supervisor = ((XmlElement)xnoResp).GetAttribute("oi_personal_emp");
                    strOi_pos_Supervisor = ((XmlElement)xnoResp).GetAttribute("oi_posicion_ult");
                }
            }

            //Completa los valores de la evaluacion -------------------------------------------------------------
            trace.Debug("Crea la evaluación");
            objEva.oi_personal_emp = pxelPer.GetAttribute("oi_personal_emp");
            objEva.oi_evento = this.m_strOi_Evento;
            objEva.oi_pos_evaluado = pxelPer.GetAttribute("oi_posicion_ult");
            objEva.oi_evaluador = strOi_Evaluador;
            objEva.oi_pos_evaluador = strOi_pos_Evaluador;
            objEva.c_tipo_evaluacion = this.m_strC_clase_eval == "1" ? strTipoEval : this.m_strC_clase_eval; //Si es 1 puede ser GER|IND, si es diferente se guarda lo que viene
            objEva.c_estado = "AB";

            if (strOi_Supervisor != "" && strOi_pos_Supervisor != "")
            {
                objEva.oi_supervisor = strOi_Supervisor;
                objEva.oi_pos_supervisor = strOi_pos_Supervisor;
            }

            trace.Debug("Completa los hijos de la evaluación.");
            if (pxelPer.GetAttribute("oi_posicion_ult") == "" || pxelPer.GetAttribute("oi_posicion_ult") == null)
            {
                throw new Exception("El legajo no tiene cargada la Posición.");
            }
            CompleteChildren(objEva, objEva.c_tipo_evaluacion, objEva.oi_pos_evaluado);

            try
            {
                NomadEnvironment.GetCurrentTransaction().Save(objEva);
            }
            catch (Exception ex)
            {
                trace.Debug("Se produjo un ERROR al intentar insertar/actualizar el DDO de Evaluaciones.");
                trace.Debug("ERROR :" + ex.Message);
                trace.Debug("DDO: " + objEva.SerializeAll());
                throw ex;
            }

            return "";

        }

        /// <summary>
        /// Completa los objetivos y las competencias
        /// </summary>
        /// <param name="xelPer"></param>
        /// <param name="xmlPers"></param>
        /// <returns></returns>
        private void CompleteChildren(NucleusRH.Base.Evaluacion.Evaluacion.EVALUACION pobjEva, string pstrClass, string pstrOi_Posicion)
        {
            NucleusRH.Base.Evaluacion.Evaluacion.COM_EVALU objC;
            NucleusRH.Base.Evaluacion.Evaluacion.OBJ_EVALU objO;
            NucleusRH.Base.Evaluacion.Evaluacion.APT_COM objAptitud;
            NucleusRH.Base.Organizacion.Competencias.COMPETENCIA objCompetencia;

            XmlNodeList xnlChildren;

            XmlDocument xmlChildrenOC = new XmlDocument();

            //string strC_Tipo_Eval = "";
            string strQueryResult;
            string strPonderacion;

            //Obtiene los documentos para filtrar luego los objetivos y las competencias
            trace.Debug("Obtiene los documentos para filtrar luego los objetivos y las competencias '" + pstrClass + "'");
            if (pstrClass == "0")
            {
                //Busca los objetivos y las competencias desde la base de datos para una posición en particular
                strQueryResult = GetChildrenPos(pstrOi_Posicion);
                xmlChildrenOC.LoadXml(strQueryResult);

            }
            else
            {
                //Busca los objetivos y las competencias para IND, GER o las ADICIONALES DEL USUARIO en particular
                strQueryResult = GetChildrenClass(pstrClass);
                xmlChildrenOC.LoadXml(strQueryResult);
            }

            //Busca las COMPENTENCIAS -----------------------------------------------------------------
            xnlChildren = xmlChildrenOC.SelectNodes("/children/child[@tipo = 'COM']");

            //Agrega las COMPENTENCIAS ----------
            trace.Debug("Agrega las COMPENTENCIAS");
            foreach (XmlNode xnoChild in xnlChildren)
            {
                objC = new NucleusRH.Base.Evaluacion.Evaluacion.COM_EVALU();
                objC.oi_competencia = ((XmlElement)xnoChild).GetAttribute("oi_competencia");

                strPonderacion = ((XmlElement)xnoChild).GetAttribute("n_ponderacion");
                strPonderacion = strPonderacion == "" ? "0" : strPonderacion;
                objC.n_ponderacion = StringUtil.str2dbl(strPonderacion);

                //Obtiene la Competencia
                objCompetencia = objC.Getoi_competencia();
                //Recorre las aptitudes de la codificadora de compentecias
                foreach (NucleusRH.Base.Organizacion.Competencias.APTIT_COMP objACOMP in objCompetencia.APTIT_COMP)
                {
                    objAptitud = new NucleusRH.Base.Evaluacion.Evaluacion.APT_COM();

                    objAptitud.oi_aptit_comp = objACOMP.Id;
                    objAptitud.n_ponderacion = objACOMP.n_ponderacion;

                    //Agrega la aptitud a la competencia
                    objC.APT_COM.Add(objAptitud);
                }

                //Agrega la Competencia a la evaluación
                pobjEva.COM_EVALU.Add(objC);

                //Obtiene el código de tipo de EVALUACIÓN
                //strC_Tipo_Eval = ((XmlElement) xnoChild).GetAttribute("c_tipo_eval");

            }

            //Busca los OBJETIVOS -----------------------------------------------------------------
            xnlChildren = xmlChildrenOC.SelectNodes("/children/child[@tipo = 'OBJ']");

            //Agrega los OBJETIVOS ----------
            trace.Debug("Agrega los OBJETIVOS");
            foreach (XmlNode xnoChild in xnlChildren)
            {
                objO = new NucleusRH.Base.Evaluacion.Evaluacion.OBJ_EVALU();
                //objO.c_objetivo = ((XmlElement)xnoChild).GetAttribute("c_objetivo");
                objO.d_objetivo = ((XmlElement)xnoChild).GetAttribute("d_objetivo");

                strPonderacion = ((XmlElement)xnoChild).GetAttribute("n_ponderacion");
                strPonderacion = strPonderacion == "" ? "0" : strPonderacion;
                objO.n_ponderacion = StringUtil.str2dbl(strPonderacion);

                pobjEva.OBJ_EVALU.Add(objO);
            }

            //Completa el tipo de EVALUACION en caso de estar icompleto
            //pobjEva.c_tipo_evaluacion = pobjEva.c_tipo_evaluacion != "" ? pobjEva.c_tipo_evaluacion : strC_Tipo_Eval;

        }

        /* --------------------------------------------------------------------------------------------- */
        /* QUERIES ------------------------------------------------------------------------------------- */
        /* --------------------------------------------------------------------------------------------- */

        /// <summary>
        /// Obtiene el codigo de clase organizativa
        /// </summary>
        /// <returns></returns>
        private void GetOrgClass()
        {

            string strQuery;
            string strResult = "";

            strQuery =
            "<qry:main xmlns:qry=\"objects\" doc=\"PARAM\">" +
                "<qry:element name=\"objects\">" +
                    "<qry:insert-select name=\"qryClase\" />" +
                "</qry:element>" +
            "</qry:main>" +

            "<qry:select xmlns:qry=\"qryClase\" name=\"qryClase\" >" +
                "<qry:xquery>" +
                    "for $f in sql('" +
                        "SELECT " +
                        "c.oi_clase_org " +

                        "FROM ORG03_EMPRESAS AS e, " +
                        "ORG02_CLASES_ORG AS c " +

                        "WHERE " +
                        "c.c_estado = \\'V\\' AND " +
                        "c.c_clase_org = e.c_empresa + \\' POS\\' AND " +
                        "e.oi_empresa = " + this.m_strOi_Empresa +

                    "')/ROWS/ROW" +
                "</qry:xquery>" +

                "<qry:out>" +
                    "<qry:element name=\"object\">" +
                        "<qry:attribute value=\"$f/@oi_clase_org\" name=\"oi_clase_org\" />" +
                    "</qry:element>" +
                "</qry:out>" +

            "</qry:select>";

            //Ejecuta el query
            SQLService sqlService = m_Proxy.SQLService();
            strResult = sqlService.Get(strQuery, "");

            if (strResult.EndsWith("/>"))
            {
                throw new Exception("No se encontraron los datos de la Unidad Organizativa del evento.");
            }
            else
            {
                //Obtiene el valor de los atributos
                XmlDocument xmlObjects = new XmlDocument();
                XmlElement xelObject;

                xmlObjects.LoadXml(strResult);

                xelObject = (XmlElement)xmlObjects.DocumentElement.ChildNodes[0];
                this.m_strOi_Clase_org = xelObject.GetAttribute("oi_clase_org");
            }

        }

        /// <summary>
        /// Envia los mails a los evaluadores de un evento
        /// </summary>
        /// <returns></returns>
        private void SendMails(EVENTO pobjEvento, string pstrPersonalList)
        {
            string strQuery;
            string strResult = "";

            //Crea el objeto de Parametros
            NomadXML objQParams = new NomadXML("DATOS");
            objQParams.SetAttr("oi_evento", pobjEvento.Id);
            objQParams.SetAttr("oi_personal_emps", pstrPersonalList);

            //Ejecuta el query
            strResult = m_Proxy.SQLService().Get(EVENTO.Resources.qryEvaluadoresEvaluados, objQParams.ToString());

            trace.Debug("Inicia el envio de mails.");
            trace.Debug("Resultado query " + strResult);

            if (!strResult.EndsWith("/>"))
            {

                //Obtiene el valor de los atributos
                XmlDocument xmlEvaluators = new XmlDocument();
                xmlEvaluators.LoadXml(strResult);

                MAIL objMail = null;
                DESTINATARIO objDest;
                bool blnBeginStarted = false;
                string strLastEvaluator = "";
                string strPersonalList = "";
                string strMsg = "Se ha creado el evento evaluativo {d_evento}\n" +
                                "Usted se encuentra como evaluador de las siguientes personas:\n\n" +
                                "{personal_list}\n\n" +
                                "Recuerde ingresar al sistema y cargar los resultados.";

                //Recorre los evaluadores y les envia un mail
                foreach (XmlElement xelRegis in xmlEvaluators.DocumentElement.ChildNodes)
                {

                    if (strLastEvaluator != xelRegis.GetAttribute("oi_evaluador"))
                    {
                        //Cambió el evaluador.

                        strLastEvaluator = xelRegis.GetAttribute("oi_evaluador");

                        //Existia un mail sin enviar
                        if (objMail != null)
                        {
                            //if (!blnBeginStarted)
                            //    NomadEnvironment.GetCurrentTransaction().Begin(); //Inicializa la TRANSACCIÓN

                            blnBeginStarted = true;

                            objMail.CONTENIDO = strMsg.Replace("{d_evento}", pobjEvento.d_evento);
                            objMail.CONTENIDO = objMail.CONTENIDO.Replace("{personal_list}", strPersonalList);
                            strPersonalList = "";

                            NomadEnvironment.GetCurrentTransaction().Save(objMail);
                        }

                        if (xelRegis.GetAttribute("oi_usuario_evaluador") != null && xelRegis.GetAttribute("oi_usuario_evaluador") != "")
                        {
                            //Crea un nuevo mail
                            trace.Debug("Crea mail para " + xelRegis.GetAttribute("oi_usuario_evaluador"));
                            objMail = new MAIL();
                            objDest = new DESTINATARIO();

                            objDest.ENTIDAD = xelRegis.GetAttribute("oi_usuario_evaluador");
                            objMail.DESTINATARIOS.Add(objDest);

                            //Completa el mensaje
                            objMail.DESDE_APLICACION = "NUCLEUS-RH";
                            objMail.REMITENTE = "NUCLEUS-RH";
                            objMail.FECHA_CREACION = DateTime.Now;
                            objMail.ASUNTO = "NucleusRH - Notificacion de Evaluación Inicializada.";
                        }

                    }

                    strPersonalList = strPersonalList + xelRegis.GetAttribute("legajo_evaluado") + " - " + xelRegis.GetAttribute("nom_evaluado") + "\n";

                }

                //Existia un mail sin enviar
                if (objMail != null)
                {
                    //if (!blnBeginStarted)
                    //    NomadEnvironment.GetCurrentTransaction().Begin(); //Inicializa la TRANSACCIÓN

                    blnBeginStarted = true;

                    objMail.CONTENIDO = strMsg.Replace("{d_evento}", pobjEvento.d_evento);
                    objMail.CONTENIDO = objMail.CONTENIDO.Replace("{personal_list}", strPersonalList);

                    NomadEnvironment.GetCurrentTransaction().Save(objMail);
                }
                /*
                //Existia un mail sin enviar
                if (blnBeginStarted)
                {
                    //Realiza el commit de la TRANSACCIÓN para enviar todos los mails
                    try
                    {
                        NomadEnvironment.GetCurrentTransaction().Commit();

                    }
                    catch (Exception ex)
                    {
                        NomadEnvironment.GetCurrentTransaction().Rollback();
                        throw new Exception("No se pudieron enviar los mails a los Evaluadores. " + ex.Message);
                    }
                }*/

            }

        }

        /// <summary>
        /// Obtiene los datos de la persona y de su estructura
        /// </summary>
        /// <returns></returns>
        private string GetPersonalEmpData()
        {

            string strQuery;
            string strResult = "";

            strQuery =
            "<qry:main xmlns:qry=\"objects\" doc=\"PARAM\">" +
                "<qry:element name=\"Pers_Estructs\">" +
                    "<qry:insert-select name=\"qryEvent\" />" +
                "</qry:element>" +
            "</qry:main>" +

            "<qry:select xmlns:qry=\"qryEvent\" name=\"qryEvent\" >" +
                "<qry:xquery>" +
                    "for $f in sql('" +
                        "SELECT " +
                            "p.oi_personal_emp, " +
                            "p.oi_posicion_ult, " +
                            "p.e_numero_legajo, " +
                            "ep.l_responsable, " +
                            "ep.oi_estructura, " +
                            "e.oi_estr_padre " +

                        "FROM  " +
                            "PER02_PERSONAL_EMP AS p, " +
                            "ORG02_ESTRUC_PERS AS ep,  " +
                            "ORG02_ESTRUCTURAS AS e " +

                        "JOIN " +
                            "p.oi_personal_emp INNER ep.oi_personal_emp, " +
                            "ep.oi_estructura INNER e.oi_estructura " +

                        "WHERE " +
                            "ep.oi_clase_org = " + this.m_strOi_Clase_org + " AND " +
                            "ep.oi_personal_emp IN (" + this.m_strPeople_List + ") " +

                    "')/ROWS/ROW" +
                "</qry:xquery>" +

                "<qry:out>" +
                    "<qry:element name=\"Per_Estruct\">" +
                        "<qry:attribute value=\"$f/@oi_personal_emp\" name=\"oi_personal_emp\" />" +
                        "<qry:attribute value=\"$f/@oi_posicion_ult\" name=\"oi_posicion_ult\" />" +
                        "<qry:attribute value=\"$f/@e_numero_legajo\" name=\"e_numero_legajo\" />" +
                        "<qry:attribute value=\"$f/@l_responsable\" name=\"l_responsable\" />" +
                        "<qry:attribute value=\"$f/@oi_estructura\" name=\"oi_estructura\" />" +
                        "<qry:attribute value=\"$f/@oi_estr_padre\" name=\"oi_estr_padre\" />" +
                    "</qry:element>" +
                "</qry:out>" +

            "</qry:select>";

            //Ejecuta el query
            SQLService sqlService = m_Proxy.SQLService();
            NomadEnvironment.GetTrace().Info("Query --> " + strQuery);
            strResult = sqlService.Get(strQuery, "");

            return strResult;

        }

        /// <summary>
        /// Obtiene los datos de las personas responsables y de su estructura
        /// </summary>
        /// <returns></returns>
        private string GetRespData()
        {

            string strQuery;
            string strResult = "";

            strQuery =
                "<qry:main xmlns:qry=\"objects\" doc=\"PARAM\">" +
                                "<qry:element name=\"Resps\">" +
                                    "<qry:insert-select name=\"qryClass\"/>" +
                                "</qry:element>" +
                            "</qry:main>" +
                            "<qry:select xmlns:qry=\"qryEvent\" name=\"qryClass\">" +
                                "<qry:xquery>" +
                                    "for $f in sql(' " +
                                    "SELECT  " +
                                        "c.oi_estructura_org  " +

                                    "FROM  " +
                                        "ORG02_CLASES_ORG AS c  " +

                                    "WHERE  " +
                                        "c.oi_clase_org = " + this.m_strOi_Clase_org +

                                "')/ROWS/ROW" +
                                "</qry:xquery>" +
                                "<qry:out>" +
                                    "<qry:element name=\"Resp\">" +
                                        "<qry:attribute value=\"$f/@oi_estructura_org\" name=\"oi_estructura\"/>" +
                                        "<qry:insert-select doc-path=\".\" name=\"qryPers\"/>" +
                                    "</qry:element>" +
                                    "<qry:insert-select doc-path=\"./Resp\" name=\"qryEstruc\"/>" +
                                "</qry:out>" +
                            "</qry:select>" +
                            "<qry:select xmlns:qry=\"qryEvent\" doc=\"PARAM\" name=\"qryEstruc\">" +
                                "<qry:xquery>" +
                                    "for $f in sql(' " +
                                    "SELECT  " +
                                        "e.oi_estructura," +
                                        "e.oi_estr_padre  " +

                                    "FROM   " +
                                        "ORG02_ESTRUCTURAS AS e  " +

                                    "WHERE  " +
                                        "e.oi_estr_padre = {#PARAM:@oi_estructura}" +

                                "')/ROWS/ROW" +
                                "</qry:xquery>" +
                                "<qry:out>" +
                                    "<qry:element name=\"Resp\">" +
                                        "<qry:attribute value=\"$f/@oi_estructura\" name=\"oi_estructura\"/>" +
                                        "<qry:attribute value=\"$f/@oi_estr_padre\" name=\"oi_estr_padre\"/>" +
                                        "<qry:insert-select doc-path=\".\" name=\"qryPers\"/>" +
                                    "</qry:element>" +
                                    "<qry:insert-select doc-path=\"$f\" name=\"qryEstruc\"/>" +
                                "</qry:out>" +
                            "</qry:select>" +
                            "<qry:select xmlns:qry=\"qryPers\" doc=\"PARAM\" name=\"qryPers\">" +
                                "<qry:xquery>" +
                                    "for $f in sql(' " +
                                    "SELECT  " +
                                        "p.oi_personal_emp, " +
                                        "p.oi_posicion_ult " +

                                    "FROM   " +
                                        "PER02_PERSONAL_EMP AS p,  " +
                                        "ORG02_ESTRUC_PERS AS ep  " +

                                                "JOIN  " +
                                                        "p.oi_personal_emp INNER ep.oi_personal_emp  " +

                                    "WHERE  " +
                                        "ep.oi_estructura = {#PARAM:@oi_estructura} AND  " +
                                        "ep.l_responsable = 1  " +

                                "')/ROWS/ROW" +
                                "</qry:xquery>" +
                                "<qry:out>" +
                                    "<qry:attribute value=\"$f/@oi_personal_emp\" name=\"oi_personal_emp\"/>" +
                                    "<qry:attribute value=\"$f/@oi_posicion_ult\" name=\"oi_posicion_ult\"/>" +
                                "</qry:out>" +
                            "</qry:select>";

            //Ejecuta el query
            SQLService sqlService = m_Proxy.SQLService();
            strResult = sqlService.Get(strQuery, "");

            return strResult;

        }

        /// <summary>
        /// Obtiene los objetivos y las compentencias para los tipos IND, GEN y los ADICIONALES por le USUARIO
        /// </summary>
        /// <returns></returns>
        private string GetChildrenClass(string pstrOi_Tipo_Eval)
        {

            string strQuery;
            string strResult = "";

            strQuery =
            "<qry:main xmlns:qry=\"objects\" doc=\"PARAM\">" +
                "<qry:element name=\"children\">" +
                    "<qry:insert-select name=\"qryEvent\" />" +
                "</qry:element>" +
            "</qry:main>" +

            "<qry:select xmlns:qry=\"qryEvent\" name=\"qryEvent\" >" +
                "<qry:xquery>" +
                    "for $f in sql('" +
                        "SELECT " +
                            "\\'COM\\' AS tipo, " +
                            "t.c_tipo_eval AS c_tipo_eval, " +
                            "c.n_ponderacion AS n_ponderacion, " +
                            "c.oi_competencia, " +
                            "\\'-\\' AS c_objetivo, " +
                            "\\'-\\' AS d_objetivo " +
                        "FROM " +
                            "EVA04_TIPOS_EVA AS t, " +
                            "EVA04_TPO_EVA_COM AS c " +
                        "JOIN " +
                            "t.oi_tipo_eval INNER c.oi_tipo_eval  " +
                        "WHERE " +
                            "t.oi_tipo_eval = " + pstrOi_Tipo_Eval + " " +

                        "UNION " +

                        "SELECT " +
                            "\\'OBJ\\' AS tipo, " +
                            "t.c_tipo_eval, " +
                            "o.n_ponderacion AS n_ponderacion, " +
                            "0 AS oi_competencia, " +
                            "o.c_objetivo, " +
                            "o.d_objetivo " +
                        "FROM " +
                            "EVA04_TIPOS_EVA AS t, " +
                            "EVA04_TPO_EVA_OBJ AS o " +
                        "JOIN " +
                            "t.oi_tipo_eval INNER o.oi_tipo_eval " +
                        "WHERE " +
                                                        "t.oi_tipo_eval = " + pstrOi_Tipo_Eval + " " +

                    "')/ROWS/ROW" +
                "</qry:xquery>" +

                "<qry:out>" +
                    "<qry:element name=\"child\">" +
                        "<qry:attribute value=\"$f/@tipo\" name=\"tipo\" />" +
                        "<qry:attribute value=\"$f/@c_tipo_eval\" name=\"c_tipo_eval\" />" +
                        "<qry:attribute value=\"$f/@n_ponderacion\" name=\"n_ponderacion\" />" +
                        "<qry:attribute value=\"$f/@oi_competencia\" name=\"oi_competencia\" />" +
                        "<qry:attribute value=\"$f/@c_objetivo\" name=\"c_objetivo\" />" +
                        "<qry:attribute value=\"$f/@d_objetivo\" name=\"d_objetivo\" />" +
                    "</qry:element>" +
                "</qry:out>" +

            "</qry:select>";

            //Ejecuta el query
            NomadEnvironment.GetTrace().Info("GetChildrenClass Query --> " + strQuery);
            SQLService sqlService = m_Proxy.SQLService();
            strResult = sqlService.Get(strQuery, "");

            //NomadEnvironment.GetTrace().Info("GetChildrenClass Query --> " + strQuery );
            NomadEnvironment.GetTrace().Info("GetChildrenClass Result --> " + strResult);

            return strResult;

        }

        /// <summary>
        /// Obtiene los objetivos y las compentencias para el tipo POS
        /// </summary>
        /// <returns></returns>
        private string GetChildrenPos(string pstrOi_Pos)
        {

            string strQuery;
            string strResult = "";

            strQuery =
            "<qry:main xmlns:qry=\"objects\" doc=\"PARAM\">" +
                "<qry:element name=\"children\">" +
                    "<qry:insert-select name=\"qryEvent\" />" +
                "</qry:element>" +
            "</qry:main>" +

            "<qry:select xmlns:qry=\"qryEvent\" name=\"qryEvent\" >" +
                "<qry:xquery>" +
                    "for $f in sql('" +

                        "SELECT " +
                            "\\'COM\\' AS tipo, " +
                            "c.oi_competencia, " +
                            "c.n_ponderacion AS n_ponderacion, " +
                            "\\'-\\' AS c_objetivo, " +
                            "\\'-\\' AS d_objetivo " +
                        "FROM " +
                            "ORG04_POSICIONES AS p, " +
                            "ORG04_POSIC_COMP AS c " +
                        "JOIN " +
                            "p.oi_posicion INNER c.oi_posicion " +
                        "WHERE " +
                            "p.oi_posicion = " + pstrOi_Pos + " " +
                            "AND c.l_evaluacion = 1 " +

                        "UNION " +

                        "SELECT " +
                            "\\'COM\\' AS tipo, " +
                            "c.oi_competencia , " +
                            "c.n_ponderacion AS n_ponderacion, " +
                            "\\'-\\' AS c_objetivo, " +
                            "\\'-\\' AS d_objetivo " +
                        "FROM " +
                            "ORG04_POSICIONES AS po, " +
                            "ORG04_PUESTOS AS pu, " +
                            "ORG04_PUESTO_COMP AS c " +
                        "JOIN " +
                            "po.oi_puesto INNER pu.oi_puesto, " +
                            "pu.oi_puesto INNER c.oi_puesto " +
                        "WHERE " +
                            "po.oi_posicion = " + pstrOi_Pos + " " +
                            "AND c.l_evaluacion = 1 " +
                            "AND c.oi_competencia NOT IN (SELECT c.oi_competencia FROM ORG04_POSICIONES AS p, ORG04_POSIC_COMP AS c JOIN p.oi_posicion INNER c.oi_posicion WHERE p.oi_posicion = " + pstrOi_Pos + " ) " +

                        "UNION  " +

                        "SELECT " +
                            "\\'OBJ\\' AS tipo, " +
                            "0 AS oi_competencia, " +
                            "0 AS n_ponderacion, " +
                            "o.e_objetivo AS c_objetivo, " +
                            "o.o_objetivo AS d_objetivo " +
                        "FROM " +
                            "ORG04_POSICIONES AS p, " +
                            "ORG04_POSIC_OBJ AS o " +
                        "JOIN " +
                            "p.oi_posicion INNER o.oi_posicion " +
                        "WHERE " +
                            "p.oi_posicion = " + pstrOi_Pos + " " +
                            "AND o.l_evaluacion = 1 " +

                        "UNION " +

                        "SELECT " +
                            "\\'OBJ\\' AS tipo, " +
                            "0 AS oi_competencia, " +
                            "0 AS n_ponderacion, " +
                            "o.e_objetivo AS c_objetivo, " +
                            "o.o_objetivo AS d_objetivo " +
                        "FROM " +
                            "ORG04_POSICIONES AS po, " +
                            "ORG04_PUESTOS AS pu, " +
                            "ORG04_PUESTO_OBJ AS o " +
                        "JOIN " +
                            "po.oi_puesto INNER pu.oi_puesto, " +
                            "pu.oi_puesto INNER o.oi_puesto " +
                        "WHERE " +
                            "o.l_evaluacion = 1 " +
                            "AND po.oi_posicion =  " + pstrOi_Pos + " " +

                    "')/ROWS/ROW" +
                "</qry:xquery>" +

                "<qry:out>" +
                    "<qry:element name=\"child\">" +
                        "<qry:attribute value=\"$f/@c_tipo_eval\" name=\"c_tipo_eval\" />" +
                        "<qry:attribute value=\"$f/@tipo\" name=\"tipo\" />" +
                        "<qry:attribute value=\"$f/@n_ponderacion\" name=\"n_ponderacion\" />" +
                        "<qry:attribute value=\"$f/@oi_competencia\" name=\"oi_competencia\" />" +
                        "<qry:attribute value=\"$f/@c_objetivo\" name=\"c_objetivo\" />" +
                        "<qry:attribute value=\"$f/@d_objetivo\" name=\"d_objetivo\" />" +
                    "</qry:element>" +
                "</qry:out>" +

            "</qry:select>";

            //Ejecuta el query
            NomadEnvironment.GetTrace().Info("GetChildrenPos Query --> " + strQuery);
            SQLService sqlService = m_Proxy.SQLService();
            strResult = sqlService.Get(strQuery, "");

            NomadEnvironment.GetTrace().Info("GetChildrenPos Result --> " + strResult);

            return strResult;

        }

    }
}



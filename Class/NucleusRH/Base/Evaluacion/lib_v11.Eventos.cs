using System;
using System.Xml;
using System.IO;
using System.Collections;

using Nomad.NSystem.Document;
using Nomad.NSystem.Proxy;
using Nomad.NSystem.Base;
using Nomad.NSystem.Functions;

namespace NucleusRH.Base.Evaluacion.Eventos
{

    //////////////////////////////////////////////////////////////////////////////////
    //Clase Evento Evaluativo
    public partial class EVENTO 
    {
        public static void InicializarEvaluaciones(Nomad.NSystem.Document.NmdXmlDocument pobjParams)
        {

            NucleusRH.Base.Evaluacion.EventosAdmin objEAdmin = new NucleusRH.Base.Evaluacion.EventosAdmin(Nomad.NSystem.Base.NomadEnvironment.GetProxy());

            string resultado = objEAdmin.CrearEvaluaciones(pobjParams.ToString());

        }
        public static void BlanquearEvaluaciones(Nomad.NSystem.Document.NmdXmlDocument PARAM)
        {


            NucleusRH.Base.Evaluacion.Evaluacion.EVALUACION eval;
            Nomad.NSystem.Document.NmdXmlDocument xmlEval = (Nomad.NSystem.Document.NmdXmlDocument)PARAM.GetFirstChildDocument().GetFirstChildDocument();
            Nomad.NSystem.Document.NmdXmlDocument evaluacion;
            string mess = "";
            bool err = false;

            NomadEnvironment.GetBatch().Trace.Add("IFO", "Iniciando blanqueo de Evaluciones", "Blanquear Evaluaciones");
            for (evaluacion = (Nomad.NSystem.Document.NmdXmlDocument)xmlEval.GetFirstChildDocument(); evaluacion != null; evaluacion = (Nomad.NSystem.Document.NmdXmlDocument)xmlEval.GetNextChildDocument())
            {
                try
                {
                    mess = "obteniendo evaluacion...";
                    NomadEnvironment.GetBatch().Trace.Add("IFO", "Legajo: " + evaluacion.GetAttribute("code").Value, "Blanquear Evaluaciones");
                    eval = NucleusRH.Base.Evaluacion.Evaluacion.EVALUACION.Get(evaluacion.GetAttribute("evaluacion").Value);
                    if (eval.c_estado != "CE")
                    {
                        mess = "eliminando evaluacion...";
                        NomadEnvironment.GetCurrentTransaction().Delete(eval);
                    }
                    else
                    {
                        NomadEnvironment.GetBatch().Trace.Add("ERR", "no se pudo eliminar la evaluacion, el estado de la misma es Cerrado...", "Blanquear Evaluaciones");
                        err = true;
                    }
                }
                catch
                {
                    NomadEnvironment.GetBatch().Trace.Add("ERR", "Error " + mess, "Blanquear Evaluaciones");
                    err = true;
                }
            }
            if (err == false)
            {
                Nomad.NSystem.Document.NmdXmlDocument xmlEvento = (Nomad.NSystem.Document.NmdXmlDocument)PARAM.GetFirstChildDocument();
                if (xmlEvento.GetAttribute("band").Value == "1")
                {
                    EVENTO evento = EVENTO.Get(xmlEvento.GetAttribute("oi_evento").Value);
                    evento.c_estado = "A";
                    NomadEnvironment.GetCurrentTransaction().Save(evento);
                }
            }

        }
        public static void CalcularEvaluaciones(string pstrOiEvento)
        {

            //------------------------------------------------------------------------------	
            //Objetos de TRACEO
            BatchService objBatch;
            TraceService objTrace;
            NomadTrace trace;

            objBatch = Nomad.NSystem.Proxy.NomadProxy.GetProxy().Batch();
            objTrace = objBatch.Trace;
            trace = NomadEnvironment.GetTrace();
            //------------------------------------------------------------------------------

            //Definición de las principales variables y objetos
            EVENTO objEvento;
            string strResult;
            XmlDocument xmlEvaluaciones;
            XmlNodeList xnlFiltrados;
            NucleusRH.Base.Evaluacion.Evaluacion.EVALUACION objEvaluacion;
            string strOiE;
            string strEstado;
            string strLegajo;
            bool bolProcess = true;
            bool bolAllRight = true;

            //------------------------------------------------------------------------------
            objTrace.Add("ifo", "Comienza el proceso de Calcuara Evaluaciones.", "CalcularEvaluaciones");
            trace.Debug("Comienza el proceso de Calcuara Evaluaciones.");

            //Obtiene el evento
            try
            {

                objEvento = EVENTO.Get(pstrOiEvento);

                if (objEvento == null)
                    objTrace.Add("err", "Se produjo un error al cargar el EVENTO.", "CalcularEvaluaciones");

            }
            catch (Exception ex)
            {
                objTrace.Add("err", "Se produjo un error al cargar el EVENTO. " + ex.Message + ".", "CalcularEvaluaciones");
                objEvento = null;
            }

            if (objEvento == null) return;

            //Obtiene la lista de evaluaciones
            objTrace.Add("ifo", "Obteniendo las Evaluaciones.", "CalcularEvaluaciones");
            trace.Debug("Obteniendo las Evaluaciones.");
            strResult = Nomad.NSystem.Proxy.NomadProxy.GetProxy().SQLService().Get(EVENTO.Resources.qryEvaluaciones, "<DATA evento=\"" + objEvento.Id + "\" />");
            xmlEvaluaciones = new XmlDocument();
            xmlEvaluaciones.LoadXml(strResult);
            objBatch.SetProgress(5);

            //Pregunta si existen estados diferentes a CE o AN
            xnlFiltrados = xmlEvaluaciones.DocumentElement.SelectNodes("object[@c_estado != 'CE' and @c_estado != 'AN']");
            int intChildCount = xnlFiltrados.Count;
            if (xnlFiltrados.Count > 0)
            {
                string strLegajos = "";
                foreach (XmlNode xnoEvaluacion in xnlFiltrados)
                {
                    strLegajos = strLegajos + ((XmlElement)xnoEvaluacion).GetAttribute("e_numero_legajo") + ", ";
                }

                if (strLegajos.Length > 2)
                    strLegajos = strLegajos.Substring(0, strLegajos.Length - 2);

                objTrace.Add("war", "No puede cerrarse el Evento porque existen legajos cuyas evaluaciones no están cerradas o anuladas. Legajos: " + strLegajos + ".", "CalcularEvaluaciones");
                bolProcess = false;
            }


            if (bolProcess)
            {

                xnlFiltrados = xmlEvaluaciones.DocumentElement.SelectNodes("object[@c_estado = 'CE']");
                intChildCount = xnlFiltrados.Count;
                int intCounter = 0;
                string strEvalResult;

                //Recorre las evaluaciones a cerrar (solo con estados CE)
                foreach (XmlNode xnoEvaluacion in xnlFiltrados)
                {
                    strLegajo = ((XmlElement)xnoEvaluacion).GetAttribute("e_numero_legajo");
                    strOiE = ((XmlElement)xnoEvaluacion).GetAttribute("oi_evaluacion");

                    objEvaluacion = NucleusRH.Base.Evaluacion.Evaluacion.EVALUACION.Get(strOiE);
                    strEvalResult = objEvaluacion.Calcular();

                    if (strEvalResult == "")
                        objTrace.Add("ifo", "La evaluación para el legajo '" + strLegajo + "' se proceso correctamente.", "CalcularEvaluaciones");
                    else
                    {
                        bolAllRight = false;
                        objTrace.Add("err", "Se produjo un error al cerrar la evaluación para el legajo '" + strLegajo + "'. " + strResult + ".", "CalcularEvaluaciones");
                    }

                    objBatch.SetProgress(((95 / intChildCount) * intCounter) + 5);

                }

                if (bolAllRight)
                {
                    try
                    {
                        //Si no existieron problemas cambia el estado al esvento y lo guarda.
                        objEvento.c_estado = "C";
                        Nomad.NSystem.Base.NomadEnvironment.GetCurrentTransaction().SaveRefresh((NomadObject)objEvento);

                    }
                    catch (Exception ex)
                    {
                        objTrace.Add("err", "Se produjo un error al intentar grabar el Evento. " + ex.Message + ".", "CalcularEvaluaciones");
                    }
                }

            }


            objTrace.Add("ifo", "Proceso finalizado.", "CalcularEvaluaciones");
            objBatch.SetProgress(100);

            return;

        }
        public static void AdministrarLegajos(Nomad.NSystem.Document.NmdXmlDocument param, string evento)
        {

            Nomad.NSystem.Proxy.BatchService batch = NomadEnvironment.GetBatch();

            batch.Trace.Add("IFO", "Comienza el proceso", "Administrar Legajos");
            batch.Trace.Add("IFO", "Buscando Legajos", "Administrar Legajos");
            //ARMO UN "HASHTABLE" CON TODAS LAS PERSONAS QUE ESTAN INICIALIZADAS
            Hashtable ht = new Hashtable();
            ht = NomadEnvironment.QueryHashtable(NucleusRH.Base.Evaluacion.Eventos.EVENTO.Resources.QRY_ADMINISTRAR, "<DATO oi_evento=\"" + evento + "\"/>", "oi_personal_emp", false);

            //GUARDO EL XML QUE ENTRA EN UN NOMADXML
            NomadXML xmldoc = new NomadXML(param.ToString());
            xmldoc = xmldoc.FirstChild();

            //RECORRO LA HASH Y PREGUNTO SI EN ELLA HAY IDS QUE NO ESTAN EN LOS ID QUE ENTRAN
            //DE SER ASI, ESTOS IDS HAY QUE ELIMINARLOS
            foreach (string value in ht.Keys)
            {
                if (xmldoc.FindElement2("ROW", "id", value) == null)
                {
                    NucleusRH.Base.Evaluacion.Evaluacion.EVALUACION ddoPER;
                    ddoPER = NucleusRH.Base.Evaluacion.Evaluacion.EVALUACION.Get(((NomadXML)ht[value]).GetAttr("oi_evaluacion"));

                    if (ddoPER.c_estado != "CE")
                    {
                        NomadEnvironment.GetCurrentTransaction().Delete(ddoPER);
                        batch.Trace.Add("IFO", "Eliminando evaluacion de " + ((NomadXML)ht[value]).GetAttr("persona"), "Administrar Legajos");
                    }
                    else
                    {
                        batch.Trace.Add("IFO", "No se elimina la evaluacion de " + ((NomadXML)ht[value]).GetAttr("persona") + " porque está cerrada", "Administrar Legajos");
                    }
                }
            }


            //RECORRO LOS IDS QUE ENTRAN Y PREGUNTO SI ALGUN ID NO ESTA EN LA HASH, 
            //PORQ DE SER ASI HAY QUE AGREGARLO
            string idList = "";
            for (NomadXML xmlCur = xmldoc.FirstChild(); xmlCur != null; xmlCur = xmlCur.Next())
            {
                if (!ht.ContainsKey(xmlCur.GetAttr("id")))
                    idList += "<ROW id=\"" + xmlCur.GetAttr("id") + "\"/>";
            }

            if (idList != "")
            {
                batch.Trace.Add("IFO", "Creando evaluaciones", "Administrar Legajos");
                Nomad.NSystem.Document.NmdXmlDocument paramIn = new Nomad.NSystem.Document.NmdXmlDocument("<DATOS><FILTRO oi_evento=\"" + evento + "\"><ROWS>" + idList + "</ROWS></FILTRO></DATOS>");
                NomadEnvironment.GetTrace().Info(paramIn.DocumentToString());

                NucleusRH.Base.Evaluacion.EventosAdmin objEAdmin = new NucleusRH.Base.Evaluacion.EventosAdmin(NomadEnvironment.GetProxy());
                string resultado = objEAdmin.CrearEvaluaciones(paramIn.ToString());
            }

            // Setea el estado del evento
            batch.Trace.Add("IFO", "Actualizando el evento evaluativo", "Administrar Legajos");
            ht = NomadEnvironment.QueryHashtable(NucleusRH.Base.Evaluacion.Eventos.EVENTO.Resources.QRY_ADMINISTRAR, "<DATO oi_evento=\"" + evento + "\"/>", "oi_personal_emp", false);
            NucleusRH.Base.Evaluacion.Eventos.EVENTO ddoEvento;
            ddoEvento = NucleusRH.Base.Evaluacion.Eventos.EVENTO.Get(evento);
            if (ht.Count > 0)
                ddoEvento.c_estado = "I";
            else
                ddoEvento.c_estado = "A";

            //GUARDO
            NomadEnvironment.GetCurrentTransaction().Save(ddoEvento);
            batch.Trace.Add("IFO", "Proceso Finalizado", "Administrar Legajos");

        }
    }
}

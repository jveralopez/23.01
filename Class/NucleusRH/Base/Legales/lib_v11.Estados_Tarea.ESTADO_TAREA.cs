using System;
using System.Xml;
using System.IO;
using System.Collections;

using Nomad.NSystem.Document;
using Nomad.NSystem.Proxy;
using Nomad.NSystem.Base;
using Nomad.NSystem.Functions;
using NucleusRH.Base.Configuracion.Progs;

namespace NucleusRH.Base.Legales.Estados_Tarea
{

    //////////////////////////////////////////////////////////////////////////////////
    //Clase Estados TAREA
    public partial class ESTADO_TAREA
    {
        public static void SAVE_ESTADO_TAREA(Nomad.NSystem.Proxy.NomadXML DATOS_TAREA)
        {
            NomadLog.Debug("------------------------------------------------------------------------------------------");
            NomadLog.Debug("SAVE_ESTADO_TAREA(ref Nomad.NSystem.Proxy.NomadXML DATOS_TAREA)");
            NomadLog.Debug("------------------------------------------------------------------------------------------");

            string strStep = "";
            NomadTransaction objTrans = null;
            try
            {
                strStep = "INICIANDO";
                NucleusRH.Base.Legales.Tareas.HISEST_TAREA objEstadoTarea = new NucleusRH.Base.Legales.Tareas.HISEST_TAREA();
                //HISEST_TAREA hace referencia a la tabla LEG03_HIST_ESTADOS

                strStep = "RECUPERANDO-TAREA";
                DATOS_TAREA = DATOS_TAREA.FirstChild();
                string xmlTarea = DATOS_TAREA.GetAttr("oi_tarea");
                NucleusRH.Base.Legales.Tareas.TAREA objTarea = NucleusRH.Base.Legales.Tareas.TAREA.Get(xmlTarea);

                //DATOS_TAREA --> contiene los datos de entrada
                strStep = "SETEANDO-ESTADO";
                objEstadoTarea.f_hisest_tarea = DateTime.Now;
                objEstadoTarea.o_hisest_tarea = DATOS_TAREA.GetAttr("cambio_estado_obs");
                objEstadoTarea.oi_est_tar_ant = DATOS_TAREA.GetAttr("oi_estado_tarea");
                objEstadoTarea.oi_estado_tarea = DATOS_TAREA.GetAttr("oi_estado_tarea_nuevo");
                objEstadoTarea.usuario_cambia = NomadProxy.GetProxy().UserName;

                strStep = "AGREGANDO-ESTADO-EN-TAREA";
                ///Seteo el ultimo estado de la carpeta
                objTarea.oi_estado_tarea = objEstadoTarea.oi_estado_tarea;
                objTarea.HIST_ESTADOS.Add(objEstadoTarea);

                strStep = "PERSISTIENDO";
                objTrans = new NomadTransaction();
                strStep = "PERSISTIENDO-BEGIN";
                objTrans.Begin();
                strStep = "PERSISTIENDO-SAVE";
                objTrans.Save(objTarea);
                strStep = "PERSISTIENDO-COMMIT";
                objTrans.Commit();

                //Para actualizar estado CV
                //Carpetas.CARPETA.evePostSaveCarpeta(objCarpeta.id);

            }
            catch (Exception ex)
            {
                NomadException nmdEx = NomadException.NewInternalException("NucleusRH.Base.Legales.Estados_Tarea.ESTADO.SAVE_ESTADO_TAREA()", ex);
                nmdEx.SetValue("step", strStep);

                if (objTrans != null)
                    objTrans.Rollback();

                throw nmdEx;

            }

        }

        //public static void RECHAZA_PLF_xTAREA(Nomad.NSystem.Proxy.NomadXML DDO_HIST_ESTADO, string strOiCarpeta)
        public static void RECHAZA_PLF_xTAREA(Nomad.NSystem.Proxy.NomadXML pedidos, string observaciones)
        {
            //NomadLog.Debug("------------------------------------------------------------------------------------------");
            //NomadLog.Debug("RECHAZA_PLF_xTAREA(ref Nomad.NSystem.Proxy.NomadXML DDO_HIST_ESTADO, string strOiCarpeta)");
            //NomadLog.Debug("------------------------------------------------------------------------------------------");

            NomadLog.Debug("------------------------------------------------------------------------------------------");
            NomadLog.Debug("RECHAZA_PLF_xTAREA(ref Nomad.NSystem.Proxy.NomadXML DATOS_TAREA)");
            NomadLog.Debug("------------------------------------------------------------------------------------------");

            string strStep = "";
            string strOiPLF;
            ArrayList ROWS;
            NomadTransaction objTrans = null;
            try
            {
                strStep = "INICIANDO";
                NucleusRH.Base.Legales.Tareas.HISEST_TAREA objEstadoTarea = new NucleusRH.Base.Legales.Tareas.HISEST_TAREA();
                //HISEST_TAREA hace referencia a la tabla LEG03_HIST_ESTADOS [[[[REVISAR, PARA SABER QUE HACE]]]]

                strStep = "RECUPERANDO-TAREA";
                //DATOS_TAREA = DATOS_TAREA.FirstChild();
                ROWS = pedidos.FirstChild().GetChilds();
                //-----------------Lucho--------------------------------------
                //recupero los hijos

                foreach (NomadXML r in ROWS)
                {
                    strOiPLF = r.GetAttr("oi_pedido_lf");
                    string XML = "<DATA oi_pedido_lf='"+strOiPLF+"'/>";
                    //QUERY
                    NomadXML xmlOIItems = NomadEnvironment.QueryNomadXML(PedidosLiqFinal.PEDIDO_LF.Resources.getTareaxPedidoLF, XML);

                    xmlOIItems = xmlOIItems.FirstChild();
                    string xmlTarea = xmlOIItems.GetAttr("oi_tarea");
                    if (xmlTarea != "" && xmlTarea != null && xmlTarea != string.Empty)
                    //if (xmlTarea != "" || xmlTarea != string.Empty)
                    //if (xmlTarea != "")
                    {
                        //si no tiene tarea debe pasar el siguiente hijo
                        NucleusRH.Base.Legales.Tareas.TAREA objTarea = NucleusRH.Base.Legales.Tareas.TAREA.Get(xmlTarea);

                        strStep = "SETEANDO-ESTADO";
                        objEstadoTarea.f_hisest_tarea = DateTime.Now;
                        objEstadoTarea.o_hisest_tarea = observaciones;
                        objEstadoTarea.oi_est_tar_ant = xmlOIItems.GetAttr("oi_estado_tarea");
                        objEstadoTarea.oi_estado_tarea = "4";
                        objEstadoTarea.usuario_cambia = NomadProxy.GetProxy().UserName;

                        strStep = "AGREGANDO-ESTADO-EN-TAREA";
                        ///Seteo el ultimo estado de la tarea
                        objTarea.oi_estado_tarea = objEstadoTarea.oi_estado_tarea;
                        objTarea.HIST_ESTADOS.Add(objEstadoTarea);

                        strStep = "PERSISTIENDO";
                        objTrans = new NomadTransaction();
                        strStep = "PERSISTIENDO-BEGIN";
                        objTrans.Begin();
                        strStep = "PERSISTIENDO-SAVE";
                        objTrans.Save(objTarea);
                        strStep = "PERSISTIENDO-COMMIT";
                        objTrans.Commit();

                        //Para actualizar estado CV
                        //Carpetas.CARPETA.evePostSaveCarpeta(objCarpeta.id);
                    }
                 }
            }
            catch (Exception ex)
            {
                NomadException nmdEx = NomadException.NewInternalException("NucleusRH.Base.Legales.Estados_Tarea.ESTADO.SAVE_ESTADO_TAREA()", ex);
                nmdEx.SetValue("step", strStep);

                if (objTrans != null)
                    objTrans.Rollback();

                throw nmdEx;
            }
        }
    }
}


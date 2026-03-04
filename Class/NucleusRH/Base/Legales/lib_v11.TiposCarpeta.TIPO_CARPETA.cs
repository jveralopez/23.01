using System;
using System.Xml;
using System.IO;
using System.Collections;

using Nomad.NSystem.Document;
using Nomad.NSystem.Proxy;
using Nomad.NSystem.Base;
using Nomad.NSystem.Functions;

namespace NucleusRH.Base.Legales.TiposCarpeta
{

    //////////////////////////////////////////////////////////////////////////////////
    //Clase Tipos de Carpetas
    public partial class TIPO_CARPETA 
    {
        public static NucleusRH.Base.Legales.Carpetas.CARPETA SAVE_TIPO_CARPETA(Nomad.NSystem.Proxy.NomadXML DDO_HIST_TIPO_CAR, string strOiCarpeta)
        {
            NomadLog.Debug("------------------------------------------------------------------------------------------");
            NomadLog.Debug("SAVE_TIPO_CARPETA(ref Nomad.NSystem.Proxy.NomadXML DDO_HIST_TIPO_CAR, string strOiCarpeta)");
            NomadLog.Debug("------------------------------------------------------------------------------------------");

            string strStep = "";
            NomadTransaction objTrans = null;
            try
            {
                strStep = "INICIANDO";
                //Armo el parámetro
                //DDO_HIST_TIPO_CAR = DDO_HIST_TIPO_CAR.FirstChild();

                //como el cambio de tipo ahora deriva en la pantalla de edición de carpeta, esta validación se realiza en el método NucleusRH.Base.Legales.Carpetas.CARPETA.SAVE ()
                //(mcg 05.2016)

                //NomadXML xmlParam = new NomadXML("PARAMS");
                //xmlParam.SetAttr("oi_carpeta", strOiCarpeta);
                //xmlParam.SetAttr("oi_tipo_carpeta", DDO_HIST_TIPO_CAR.GetAttr("oi_tipo_car_nu"));
                ////Ejecuto el recurso para validar los requerimientos del tipo de carpeta
                //NomadXML XML = Nomad.NSystem.Base.NomadEnvironment.QueryNomadXML(NucleusRH.Base.Legales.TiposCarpeta.TIPO_CARPETA.Resources.getReqTipo, xmlParam.ToString());
                ////Armo el parámetro del mensaje
                //strStep = "MENSAJE";
                //if (XML.FirstChild().GetAttr("c_req_atr_proc") == "1" || XML.FirstChild().GetAttr("c_req_rad_tram") == "1" || XML.FirstChild().GetAttr("c_req_obj_carpeta") == "1" || XML.FirstChild().GetAttr("c_req_carga_monto") == "1")
                //{
                //    string msg = "\n";
                //    if (XML.FirstChild().GetAttr("c_req_atr_proc") == "1")
                //    {
                //        msg += "- ATRIBUTO PROCESAL \n";
                //    }
                //    if (XML.FirstChild().GetAttr("c_req_rad_tram") == "1")
                //    {
                //        msg += "- RADICACION \n";
                //    }
                //    if (XML.FirstChild().GetAttr("c_req_obj_carpeta") == "1")
                //    {
                //        msg += "- OBJETO CARPETA \n";
                //    }
                //    if (XML.FirstChild().GetAttr("c_req_carga_monto") == "1")
                //    {
                //        msg += "- MONTO";
                //    }

                //    //Mensaje de validación de requerimientos del tipo de carpeta
                //    throw NomadException.NewMessage("Legales.CARPETA.CARP-TIPO-VAL", msg);
                //}

                NucleusRH.Base.Legales.Carpetas.HIST_TIPO_CAR objTipoCarpeta = new NucleusRH.Base.Legales.Carpetas.HIST_TIPO_CAR();

                strStep = "RECUPERANDO-CARPETA";
                NucleusRH.Base.Legales.Carpetas.CARPETA objCarpeta = NucleusRH.Base.Legales.Carpetas.CARPETA.Get(strOiCarpeta);

                strStep = "SETEANDO-TIPO-CARPETA";
                objTipoCarpeta.f_hist_tipo_car = DateTime.Now;
                objTipoCarpeta.o_hist_tipo_car = DDO_HIST_TIPO_CAR.GetAttr("o_hist_tipo_car");
                objTipoCarpeta.oi_tipo_car_ant = DDO_HIST_TIPO_CAR.GetAttr("oi_tipo_car_ant");
                objTipoCarpeta.oi_tipo_car_nu = DDO_HIST_TIPO_CAR.GetAttr("oi_tipo_car_nu");
                objTipoCarpeta.usuario_cambia = NomadProxy.GetProxy().UserName;
                
                strStep = "AGREGANDO-TIPO-EN-CARPETA";
                //Seteo el ultimo tipo de la carpeta 
                objCarpeta.oi_tipo_carpeta = objTipoCarpeta.oi_tipo_car_nu;
                objCarpeta.HIST_TIPO_CAR.Add(objTipoCarpeta);

                //::nuevo:: al cambiar el tipo cambiar el estado de la carpeta (mcg 02.2016)

                if (DDO_HIST_TIPO_CAR.GetAttr("oi_est_car_ant") != DDO_HIST_TIPO_CAR.GetAttr("oi_est_car_nu"))
                {
                NucleusRH.Base.Legales.Carpetas.HISEST_CARPETA objEstadoCarpeta = new NucleusRH.Base.Legales.Carpetas.HISEST_CARPETA();
                
                strStep = "SETEANDO-ESTADO";
                objEstadoCarpeta.f_hisest_carpeta = objTipoCarpeta.f_hist_tipo_car; //para que cambio de tipo y de estado tengan la misma hora
                objEstadoCarpeta.o_hisest_carpeta = DDO_HIST_TIPO_CAR.GetAttr("o_hist_tipo_car"); //para que cambio de tipo y estado tengan el mismo motivo
                objEstadoCarpeta.oi_est_car_ant = DDO_HIST_TIPO_CAR.GetAttr("oi_est_car_ant");
                objEstadoCarpeta.oi_estado_carpeta = DDO_HIST_TIPO_CAR.GetAttr("oi_est_car_nu");
                objEstadoCarpeta.usuario_cambia = NomadProxy.GetProxy().UserName;

                strStep = "AGREGANDO-ESTADO-EN-CARPETA";
                ///Seteo el último estado de la carpeta 
                objCarpeta.oi_estado_carpeta = objEstadoCarpeta.oi_estado_carpeta;               
                objCarpeta.HIST_ESTADOS.Add(objEstadoCarpeta);          
                }


                strStep = "PERSISTIENDO";
                objTrans = new NomadTransaction();
                strStep = "PERSISTIENDO-BEGIN";
                objTrans.Begin();
                strStep = "PERSISTIENDO-SAVE";
                objTrans.SaveRefresh(objCarpeta);
                strStep = "PERSISTIENDO-COMMIT";
                objTrans.Commit();

                //Para actualizar estado CV
                Carpetas.CARPETA.evePostSaveCarpeta(objCarpeta.id);

                return objCarpeta;
            }
            catch (Exception ex)
            {
                NomadException nmdEx = NomadException.NewInternalException("NucleusRH.Base.Legales.Estados.ESTADO.SAVE_TIPO_CARPETA()", ex);
                nmdEx.SetValue("step", strStep);
                if (objTrans != null)
                    objTrans.Rollback();

                throw nmdEx;

            }

             
        }
    }
}

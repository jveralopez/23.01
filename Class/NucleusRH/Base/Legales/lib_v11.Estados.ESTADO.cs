using System;
using System.Xml;
using System.IO;
using System.Collections;

using Nomad.NSystem.Document;
using Nomad.NSystem.Proxy;
using Nomad.NSystem.Base;
using Nomad.NSystem.Functions;

namespace NucleusRH.Base.Legales.Estados
{

    //////////////////////////////////////////////////////////////////////////////////
    //Clase Estados Carpetas
    public partial class ESTADO 
    {
        public static void SAVE_ESTADO_CARPETA(Nomad.NSystem.Proxy.NomadXML DDO_HIST_ESTADO, string strOiCarpeta)
        {
            NomadLog.Debug("------------------------------------------------------------------------------------------");
            NomadLog.Debug("SAVE_ESTADO_CARPETA(ref Nomad.NSystem.Proxy.NomadXML DDO_HIST_ESTADO, string strOiCarpeta)");
            NomadLog.Debug("------------------------------------------------------------------------------------------");


            string strStep = "";
            NomadTransaction objTrans = null;
            try
            {
                strStep = "INICIANDO";
                NucleusRH.Base.Legales.Carpetas.HISEST_CARPETA objEstadoCarpeta = new NucleusRH.Base.Legales.Carpetas.HISEST_CARPETA();

                strStep = "RECUPERANDO-CARPETA";
                NucleusRH.Base.Legales.Carpetas.CARPETA objCarpeta = NucleusRH.Base.Legales.Carpetas.CARPETA.Get(strOiCarpeta);
                DDO_HIST_ESTADO = DDO_HIST_ESTADO.FirstChild();


                strStep = "SETEANDO-ESTADO";
                objEstadoCarpeta.f_hisest_carpeta = DateTime.Now;
                objEstadoCarpeta.o_hisest_carpeta = DDO_HIST_ESTADO.GetAttr("o_hisest_carpeta");
                objEstadoCarpeta.oi_est_car_ant = DDO_HIST_ESTADO.GetAttr("oi_est_car_ant");
                objEstadoCarpeta.oi_estado_carpeta = DDO_HIST_ESTADO.GetAttr("oi_estado_carpeta");
                objEstadoCarpeta.usuario_cambia = NomadProxy.GetProxy().UserName;

                strStep = "AGREGANDO-ESTADO-EN-CARPETA";
                ///Seteo el ultimo estado de la carpeta 
                objCarpeta.oi_estado_carpeta = objEstadoCarpeta.oi_estado_carpeta;               
                objCarpeta.HIST_ESTADOS.Add(objEstadoCarpeta);

                strStep = "PERSISTIENDO";
                objTrans = new NomadTransaction();
                strStep = "PERSISTIENDO-BEGIN";
                objTrans.Begin();
                strStep = "PERSISTIENDO-SAVE";
                objTrans.Save(objCarpeta);
                strStep = "PERSISTIENDO-COMMIT";
                objTrans.Commit();

                //Para actualizar estado CV
                Carpetas.CARPETA.evePostSaveCarpeta(objCarpeta.id);
                
            }
            catch (Exception ex)
            {
                NomadException nmdEx = NomadException.NewInternalException("NucleusRH.Base.Legales.Estados.ESTADO.SAVE_ESTADO_CARPETA()", ex);
                nmdEx.SetValue("step", strStep);

                if (objTrans != null)
                    objTrans.Rollback();

                throw nmdEx;

            }  

            
        }

    }
}

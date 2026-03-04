using System;
using System.Xml;
using System.IO;
using System.Collections;

using Nomad.NSystem.Document;
using Nomad.NSystem.Proxy;
using Nomad.NSystem.Base;
using Nomad.NSystem.Functions;

namespace NucleusRH.Base.Personal.InterfaceEgreso
{

    //////////////////////////////////////////////////////////////////////////////////
    //Clase Interface Egreso
    public partial class EgresoLegajos
    {
        public static void Egreso(string oi_empresa)
        {
            int Linea = 0, Errores = 0;
            string avance = "";
            NomadBatch objBatch;
            objBatch = NomadBatch.GetBatch("Iniciando...", "Egreso de Legajos");

            NomadXML IDList = new NomadXML();
            NucleusRH.Base.Personal.InterfaceEgreso.EgresoLegajos objRead;
            DateTime fCompare = new DateTime(1900, 1, 1);

            IDList.SetText(NomadProxy.GetProxy().SQLService().Get(Resources.QRY_REGISTROS, ""));

            ArrayList lista = (ArrayList)IDList.FirstChild().GetElements("ROW");
            //RECORRO LOS IDS QUE ENTRAN Y PREGUNTO SI ALGUN ID NO ESTA EN LA HASH, PORQ DE SER ASI HAY QUE AGREGARLO
            for (int xml = 0; xml < lista.Count; xml++)
            {
                Linea++;
                NomadXML row = (NomadXML)lista[xml];
                objBatch.SetPro(0, 100, lista.Count, xml);
                objBatch.SetMess("Incorporando registro " + (xml + 1) + " de " + lista.Count);

                //Inicio la Transaccion
                try
                {
                    avance = "Error leyendo la línea, verifique el formato y cantidad de campos.";
                    objRead = NucleusRH.Base.Personal.InterfaceEgreso.EgresoLegajos.Get(row.GetAttr("id"));

                    ///////////
                    avance = "Error obteniendo el legajo";
                    string oiPEREMP = NomadEnvironment.QueryValue("PER02_PERSONAL_EMP", "oi_personal_emp", "e_numero_legajo", objRead.e_numero_legajo, "PER02_PERSONAL_EMP.oi_empresa = " + oi_empresa, false);
                    string oiMOTEGR = NomadEnvironment.QueryValue("PER22_MOT_EG_PER", "oi_motivo_eg_per", "c_motivo_eg_per", objRead.c_motivo_egreso, "", true);
                    if (string.IsNullOrEmpty(oiPEREMP))
                        throw new Exception("El legajo '" + objRead.e_numero_legajo + "' no existe en la empresa filtrada.");

                    avance = "Error egresando el legajo. ";
                    if (string.IsNullOrEmpty(oiMOTEGR))
                        throw new Exception("El motivo de egreso '" + objRead.c_motivo_egreso + "' es inexistente.");

                    NucleusRH.Base.Personal.LegajoEmpresa.PERSONAL_EMP leg = NucleusRH.Base.Personal.LegajoEmpresa.PERSONAL_EMP.Get(oiPEREMP);
                    if (!leg.f_egresoNull)
                        throw new Exception("El legajo '" + objRead.e_numero_legajo + "' ya está egresado.");
                    leg.PreEgreso_Personal();

                    NucleusRH.Base.Personal.LegajoEmpresa.INGRESOS_PER ingreso = null;
                    foreach (NucleusRH.Base.Personal.LegajoEmpresa.INGRESOS_PER ing in leg.INGRESOS_PER)
                        if (ing.f_egresoNull)
                            ingreso = ing;

                    if (ingreso != null)
                    {
                        ingreso.f_egreso = objRead.f_egreso;
                        ingreso.oi_motivo_eg_per = oiMOTEGR;
                    }
                    leg.Egreso_Personal(objRead.f_egreso, "", oiMOTEGR);
                    ///////////
                }
                catch (Exception e)
                {
                    objBatch.Err("Linea: " + Linea.ToString() + " - " + avance + " - " + e.Message);
                    Errores++;
                }
            }

            objBatch.Log("Registros Procesados: " + Linea.ToString() + " - Importados: " + (Linea - Errores).ToString());
            objBatch.Log("Finalizado...");
        }
    }
}

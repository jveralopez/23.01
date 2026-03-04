using System;
using System.Xml;
using System.IO;
using System.Collections;

using Nomad.NSystem.Document;
using Nomad.NSystem.Proxy;
using Nomad.NSystem.Base;
using Nomad.NSystem.Functions;

namespace NucleusRH.Base.Migracion.Personal.IngresosLegajo
{

    //////////////////////////////////////////////////////////////////////////////////
    //Clase Ingresos por Legajo
    public partial class INGRESO_PER 
    {
        public static void ImportarIngresosLegajos()
        {

            int Linea = 0, Errores = 0;

            NomadBatch objBatch;
            objBatch = NomadBatch.GetBatch("Iniciando...", "Importación de Ingresos por Legajos");

            Hashtable htPARENTS = new Hashtable();            
            NomadXML IDList = new NomadXML();
            NucleusRH.Base.Migracion.Personal.IngresosLegajo.INGRESO_PER objRead;
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
                    objRead = NucleusRH.Base.Migracion.Personal.IngresosLegajo.INGRESO_PER.Get(row.GetAttr("id"));
                    string oiEMP = "", oiMC = "";

                    //Valido atributos obligatorios
                    if (objRead.c_empresa == "")
                    {
                        NomadEnvironment.GetBatch().Trace.Add("err", "No se especificó la Empresa, se rechaza el registro '" + objRead.e_nro_legajo + " - " + objRead.f_ingreso.ToString("dd/MM/yyyy") + "' - Linea: " + Linea.ToString(), "Importación de Ingresos por Legajos");
                        Errores++;
                        continue;
                    }
                    if (objRead.e_nro_legajo == "")
                    {
                        NomadEnvironment.GetBatch().Trace.Add("err", "No se especificó el Número de Legajo, se rechaza el registro '" + objRead.e_nro_legajo + " - " + objRead.f_ingreso.ToString("dd/MM/yyyy") + "' - Linea: " + Linea.ToString(), "Importación de Ingresos por Legajos");
                        Errores++;
                        continue;
                    }
                    if (objRead.f_ingresoNull || objRead.f_ingreso < fCompare)
                    {
                        NomadEnvironment.GetBatch().Trace.Add("err", "No se especificó la Fecha de Ingreso, se rechaza el registro '" + objRead.e_nro_legajo + " - " + objRead.f_ingreso.ToString("dd/MM/yyyy") + "' - Linea: " + Linea.ToString(), "Importación de Ingresos por Legajos");
                        Errores++;
                        continue;
                    }

                    //Recupero la Empresa
                    oiEMP = NomadEnvironment.QueryValue("ORG03_EMPRESAS", "oi_empresa", "c_empresa", objRead.c_empresa, "", true);
                    if (oiEMP == null)
                    {
                        objBatch.Err("La Empresa no existe, se rechaza el registro '" + objRead.e_nro_legajo + " - " + objRead.f_ingreso.ToString("dd/MM/yyyy") + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }

                    //Recupero el Motivo de Cambio
                    if (objRead.c_motivo_eg_per != "")
                    {
                        oiMC = NomadEnvironment.QueryValue("PER22_MOT_EG_PER", "oi_motivo_eg_per", "c_motivo_eg_per", objRead.c_motivo_eg_per, "", true);
                        if (oiMC == null)
                        {
                            objBatch.Err("El Motivo de Egreso no existe, se rechaza el registro '" + objRead.e_nro_legajo + " - " + objRead.f_ingreso.ToString("dd/MM/yyyy") + "' - Linea: " + Linea.ToString());
                            Errores++;
                            continue;
                        }
                    }

                    //Cargo el Padre  	
                    NucleusRH.Base.Personal.LegajoEmpresa.PERSONAL_EMP DDOLEG = NucleusRH.Base.Migracion.Interfaces.INTERFACE.GetLegajo(objRead.e_nro_legajo, oiEMP, htPARENTS);
                    if (DDOLEG == null)
                    {
                        objBatch.Err("El Legajo no existe en la Empresa, se rechaza el registro '" + objRead.e_nro_legajo + " - " + objRead.f_ingreso.ToString("dd/MM/yyyy") + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }

                    //Me fijo si ya existe un registro de ingresos en la fecha especificada para la persona  	
                    if (DDOLEG.INGRESOS_PER.GetByAttribute("f_ingreso", objRead.f_ingreso) != null)
                    {
                        objBatch.Err("Ya existe un Ingreso de Ingresos en el Legajo para la Fecha, '" + objRead.e_nro_legajo + " - " + objRead.f_ingreso.ToString("dd/MM/yyyy") + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }

                    //Creo el Ingerso en el Legajo			
                    NucleusRH.Base.Personal.LegajoEmpresa.INGRESOS_PER DDOINGPER;
                    DDOINGPER = new NucleusRH.Base.Personal.LegajoEmpresa.INGRESOS_PER();

                    DDOINGPER.f_ingreso = objRead.f_ingreso;
                    DDOINGPER.f_egreso = objRead.f_egreso;
                    DDOINGPER.f_egresoNull = objRead.f_egresoNull;
                    DDOINGPER.f_fin_contrato = objRead.f_fin_contrato;
                    DDOINGPER.f_fin_contratoNull = objRead.f_fin_contratoNull;
                    DDOINGPER.o_egreso = objRead.o_egreso;
                    if (oiMC != "") DDOINGPER.oi_motivo_eg_per = oiMC;

                    //Agrego el Ingreso
                    DDOLEG.INGRESOS_PER.Add(DDOINGPER);
                }
                catch (Exception e)
                {
                    objBatch.Err("Error desconocido en registro, por favor verifique el formato del archivo y la correcta asignación de los campos - Linea: " + Linea.ToString() + " - " + e.Message);
                    Errores++;
                }
            }

            try
            {
                NucleusRH.Base.Migracion.Interfaces.INTERFACE.Grabar(htPARENTS);
            }
            catch (Exception e)
            {
                objBatch.Err("Error al grabar - " + e.Message);
                Errores = Linea;
            }

            objBatch.Log("Registros Procesados: " + Linea.ToString() + " - Importados: " + (Linea - Errores).ToString());
            objBatch.Log("Finalizado...");

        }
    }
}

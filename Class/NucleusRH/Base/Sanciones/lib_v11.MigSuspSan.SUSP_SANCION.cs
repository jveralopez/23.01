using System;
using System.Xml;
using System.IO;
using System.Collections;

using Nomad.NSystem.Document;
using Nomad.NSystem.Proxy;
using Nomad.NSystem.Base;
using Nomad.NSystem.Functions;

namespace NucleusRH.Base.Sanciones.MigSuspSan
{

    //////////////////////////////////////////////////////////////////////////////////
    //Clase Legajos de Sanción
    public partial class SUSP_SANCION 
    {
        public static void ImportarSuspSan()
        {

            //Instancio la Persona
            NucleusRH.Base.Sanciones.LegajoSanciones.PERSONAL_EMP DDOLEG = null;

            //Instancio el DICTADO
            int Linea = 0, Errores = 0;
            string oiPERANT = "";

            NomadBatch objBatch;
            objBatch = NomadBatch.GetBatch("Iniciando...", "Importación de Suspensión de Sanciones");

            NomadXML IDList = new NomadXML();
            NucleusRH.Base.Sanciones.MigSuspSan.SUSP_SANCION objRead;

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
                    objRead = NucleusRH.Base.Sanciones.MigSuspSan.SUSP_SANCION.Get(row.GetAttr("id"));

                    //Valido atributos obligatorios  	                                             
                    if (objRead.c_empresa == "")
                    {
                        objBatch.Err("No se especificó la Empresa, se rechaza el registro '" + objRead.c_empresa + " - " + objRead.e_numero_legajo + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }
                    if (objRead.e_numero_legajoNull)
                    {
                        objBatch.Err("No se especificó el número de legajo, se rechaza el registro '" + objRead.c_empresa + " - " + objRead.e_numero_legajo + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }
                    if (objRead.f_fechahora_sancNull)
                    {
                        objBatch.Err("No se especificó la fecha hora de la Sanción, se rechaza el registro '" + objRead.c_empresa + " - " + objRead.e_numero_legajo + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }
                    if (objRead.f_fechahora_iniNull)
                    {
                        objBatch.Err("No se especificó la fecha hora de Inicio de la suspensión, se rechaza el registro '" + objRead.c_empresa + " - " + objRead.e_numero_legajo + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }
                    if (objRead.f_fechahora_finNull)
                    {
                        objBatch.Err("No se especificó la fecha hora de fin de la suspensión, se rechaza el registro '" + objRead.c_empresa + " - " + objRead.e_numero_legajo + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }
                    if (objRead.e_dias_suspNull)
                    {
                        objBatch.Err("No se especificaron los diás de la suspensión, se rechaza el registro '" + objRead.c_empresa + " - " + objRead.e_numero_legajo + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }

                    //Recupero los OI de los códigos ingresados
                    string oiEMP = "", oiSAN = "", oiPER = "", oiPEREMP = "";

                    //Recupero la empresa
                    oiEMP = NomadEnvironment.QueryValue("ORG03_EMPRESAS", "oi_empresa", "c_empresa", objRead.c_empresa, "", true);
                    if (oiEMP == null)
                    {
                        objBatch.Err("La empresa no existe, se rechaza el registro '" + objRead.c_empresa + " - " + objRead.e_numero_legajo + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }

                    //Recupero el Legajo en la Empresa
                    oiPEREMP = NomadEnvironment.QueryValue("PER02_PERSONAL_EMP", "oi_personal_emp", "e_numero_legajo", objRead.e_numero_legajo.ToString(), "PER02_PERSONAL_EMP.oi_empresa = " + oiEMP, true);
                    if (oiPEREMP == null)
                    {
                        objBatch.Err("El Legajo no existe en la Empresa, se rechaza el registro '" + objRead.c_empresa + " - " + objRead.e_numero_legajo + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }

                    //Recuperar la sancion 
                    string param = "<DATOS oi_personal_emp = \"" + oiPEREMP + "\" f_fechahora_sanc= \"" + objRead.f_fechahora_sanc.ToString("yyyyMMddHHmmss") + "\" />";
                    NomadXML xml_sancion = null;

                    xml_sancion = new Nomad.NSystem.Proxy.NomadXML(NomadEnvironment.QueryString(NucleusRH.Base.Sanciones.MigSuspSan.SUSP_SANCION.Resources.QRY_SANCION, param));

                    if (xml_sancion.FirstChild().GetAttr("oi_sancion") == "")
                    {
                        objBatch.Err("La Sanción no existe, se rechaza el registro '" + objRead.c_empresa + " - " + objRead.e_numero_legajo + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }

                    if (oiPERANT != oiPEREMP)
                    {
                        if (DDOLEG == null || DDOLEG.Id != oiPEREMP)
                        {
                            if (DDOLEG != null)
                            {
                                //Grabo
                                try
                                {
                                    NomadEnvironment.GetCurrentTransaction().Save(DDOLEG);
                                }
                                catch (Exception e)
                                {
                                    objBatch.Err("Error al grabar registro " + objRead.c_empresa + " - " + objRead.e_numero_legajo + "' - Linea: " + Linea.ToString() + " - " + e.Message);
                                    Errores++;
                                }
                            }
                        }
                        oiPERANT = oiPEREMP;
                        DDOLEG = NucleusRH.Base.Sanciones.LegajoSanciones.PERSONAL_EMP.Get(oiPEREMP);
                    }

                    NucleusRH.Base.Sanciones.LegajoSanciones.SANCION_PER ddoSAN = (NucleusRH.Base.Sanciones.LegajoSanciones.SANCION_PER)DDOLEG.SANCION_PER.GetById(xml_sancion.FirstChild().GetAttr("oi_sancion_per"));

                    //Me fijo si ya existe un registro de Suspensión para la persona  	
                    if (ddoSAN.SUSP_PER.GetByAttribute("f_fechahora_ini", objRead.f_fechahora_ini) != null)
                    {
                        objBatch.Err("Ya existe la Suspensión en el Legajo, '" + objRead.c_empresa + " - " + objRead.e_numero_legajo + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }

                    //Creo la Suspensión   
                    NucleusRH.Base.Sanciones.LegajoSanciones.SUSPENSION_PER DDOSUSPSAN;
                    DDOSUSPSAN = new NucleusRH.Base.Sanciones.LegajoSanciones.SUSPENSION_PER();
                    DDOSUSPSAN.f_fechahora_ini = objRead.f_fechahora_ini;
                    DDOSUSPSAN.f_fechahora_fin = objRead.f_fechahora_fin;
                    DDOSUSPSAN.e_dias_susp = objRead.e_dias_susp;

                    //Agrego la Suspensión
                    ddoSAN.SUSP_PER.Add(DDOSUSPSAN);

                    //Agrego la Sanción

                }
                catch (Exception e)
                {
                    objBatch.Err("Error desconocido en registro, por favor verifique el formato del archivo y la correcta asignación de los campos - Linea: " + Linea.ToString() + " - " + e.Message);
                    Errores++;
                }
            }

            if (DDOLEG != null)
            {
                //Grabo
                try
                {
                    NomadEnvironment.GetCurrentTransaction().Save(DDOLEG);
                }
                catch (Exception e)
                {
                    objBatch.Err("Error al grabar registro - Linea: " + Linea.ToString() + " - " + e.Message);
                    Errores++;
                }
            }

            objBatch.Log("Registros Procesados: " + Linea.ToString() + " - Importados: " + (Linea - Errores).ToString());
            objBatch.Log("Finalizado...");

        }

    }
}

using System;
using System.Xml;
using System.IO;
using System.Collections;

using Nomad.NSystem.Document;
using Nomad.NSystem.Proxy;
using Nomad.NSystem.Base;
using Nomad.NSystem.Functions;

namespace NucleusRH.Base.Sanciones.MigLegSan
{

    //////////////////////////////////////////////////////////////////////////////////
    //Clase Legajos de Sanción
    public partial class LEGAJO_SANCION 
    {
        public static void ImportarLegSan()
        {

            //Instancio la Persona
            NucleusRH.Base.Sanciones.LegajoSanciones.PERSONAL_EMP DDOLEG = null;

            //Instancio el DICTADO
            int Linea = 0, Errores = 0;
            string oiPERANT = "";

            NomadBatch objBatch;
            objBatch = NomadBatch.GetBatch("Iniciando...", "Importación de Legajos Sanciones");

            NomadXML IDCur;
            NomadXML IDList = new NomadXML();
            NucleusRH.Base.Sanciones.MigLegSan.LEGAJO_SANCION objRead;

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
                    objRead = NucleusRH.Base.Sanciones.MigLegSan.LEGAJO_SANCION.Get(row.GetAttr("id"));

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
                    if (objRead.c_motivo_sancion == "")
                    {
                        objBatch.Err("No se especificó el motivo de la Sanción, se rechaza el registro '" + objRead.c_empresa + " - " + objRead.e_numero_legajo + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }
                    if (objRead.c_sancion == "")
                    {
                        objBatch.Err("No se especificó la Sanción, se rechaza el registro '" + objRead.c_empresa + " - " + objRead.e_numero_legajo + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }

                    //Recupero los OI de los códigos ingresados
                    string oiEMP = "", oiMOTSAN = "", oiSAN = "", oiPER = "", oiPEREMP = "";

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

                    //Recuperar el motivo de la sancion
                    if (objRead.c_motivo_sancion != "")
                    {
                        oiMOTSAN = NomadEnvironment.QueryValue("SAN01_MOTIVOS_SANC", "oi_motivo_sancion", "c_motivo_sancion", objRead.c_motivo_sancion, "", true);
                        if (oiMOTSAN == null)
                        {
                            objBatch.Err("El motivo de la sanción no existe, se rechaza el registro '" + objRead.c_empresa + " - " + objRead.e_numero_legajo + "' - Linea: " + Linea.ToString());
                            Errores++;
                            continue;
                        }
                    }

                    //Recuperar la sancion
                    if (objRead.c_sancion != "")
                    {
                        oiSAN = NomadEnvironment.QueryValue("SAN02_SANCIONES", "oi_sancion", "c_sancion", objRead.c_sancion, "", true);
                        if (oiSAN == null)
                        {
                            objBatch.Err("La sanción no existe, se rechaza el registro '" + objRead.c_empresa + " - " + objRead.e_numero_legajo + "' - Linea: " + Linea.ToString());
                            Errores++;
                            continue;
                        }
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

                    //Me fijo si ya existe un registro de Sanción en la fecha especificada para la persona  	
                    if (DDOLEG.SANCION_PER.GetByAttribute("f_fechahora_sanc", objRead.f_fechahora_sanc) != null)
                    {
                        objBatch.Err("Ya existe una Sanción en el Legajo para la Fecha, '" + objRead.c_empresa + " - " + objRead.e_numero_legajo + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }

                    //Creo la Sanción   
                    NucleusRH.Base.Sanciones.LegajoSanciones.SANCION_PER DDOSAN;
                    DDOSAN = new NucleusRH.Base.Sanciones.LegajoSanciones.SANCION_PER();
                    DDOSAN.oi_personal_emp = int.Parse(oiPEREMP);
                    DDOSAN.oi_motivo_sancion = oiMOTSAN;
                    DDOSAN.oi_sancion = oiSAN;
                    DDOSAN.f_fechahora_sanc = objRead.f_fechahora_sanc;
                    //DDOSAN.f_fechahora_sancNull = objRead.f_fechahora_sancNull;
                    DDOSAN.o_sancion = objRead.o_sancion;
                    DDOSAN.f_estado = DateTime.Now;
                    DDOSAN.c_estado = "A";
                    DDOSAN.e_dias_susp_real = objRead.e_dias_susp_real;

                    //Agrego la Sanción
                    DDOLEG.SANCION_PER.Add(DDOSAN);
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

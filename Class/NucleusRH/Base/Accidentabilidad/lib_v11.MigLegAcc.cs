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

namespace NucleusRH.Base.Accidentabilidad.MigLegAcc
{

    //////////////////////////////////////////////////////////////////////////////////
    //Clase Legajos de Accidentes
    public partial class LEGAJO_ACCIDENTE : Nomad.NSystem.Base.NomadObject
    {
        public static void ImportarLegAcc()
        {

            //Instancio la Persona
            NucleusRH.Base.Accidentabilidad.LegajoAccidentes.PERSONAL_EMP DDOLEG = null;

            //Instancio el DICTADO
            int Linea = 0, Errores = 0;
            string oiPERANT = "";

            NomadBatch objBatch;
            objBatch = NomadBatch.GetBatch("Iniciando...", "Importación de Legajos Accidentes");

            NomadXML IDCur;
            NomadXML IDList = new NomadXML();
            NucleusRH.Base.Accidentabilidad.MigLegAcc.LEGAJO_ACCIDENTE objRead;

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
                    objRead = NucleusRH.Base.Accidentabilidad.MigLegAcc.LEGAJO_ACCIDENTE.Get(row.GetAttr("id"));

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
                    if (objRead.f_fechahora_accNull)
                    {
                        objBatch.Err("No se especificó la fecha hora del Accidente, se rechaza el registro '" + objRead.c_empresa + " - " + objRead.e_numero_legajo + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }
                    if (objRead.c_tipo_siniestro == "")
                    {
                        objBatch.Err("No se especificó el Tipo de siniestro, se rechaza el registro '" + objRead.c_empresa + " - " + objRead.e_numero_legajo + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }

                    //Recupero los OI de los c&#243;digos ingresados
                    string oiEMP = "", oiACTINS = "", oiTIPACC = "", oiPER = "", oiPEREMP = "", oiAGCAU = "", oiCONPEL = "",
                                 oiFACCON = "", oiINCAPA = "", oiREGCUE = "", oiNATLES = "", oiELEPP = "", oiOCUPA = "", oiTIPSIN = "", oiLIC = "";

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
                    //Recupero el tipo de accidente
                    if (objRead.c_tipo_acc != "")
                    {
                        oiTIPACC = NomadEnvironment.QueryValue("ACC10_TIPOS_ACC", "oi_tipo_acc", "c_tipo_acc", objRead.c_tipo_acc, "", true);
                        if (oiTIPACC == null)
                        {
                            objBatch.Err("El tipo de accidente no existe, se rechaza el registro '" + objRead.c_empresa + " - " + objRead.e_numero_legajo + "' - Linea: " + Linea.ToString());
                            Errores++;
                            continue;
                        }
                    }
                    //Recupero el Acto inseguridad
                    if (objRead.c_acto_inseg != "")
                    {
                        oiACTINS = NomadEnvironment.QueryValue("ACC02_ACTOS_INSEG", "oi_acto_inseg", "c_acto_inseg", objRead.c_acto_inseg, "", true);
                        if (oiACTINS == null)
                        {
                            objBatch.Err("El acto de inseguridad no existe, se rechaza el registro '" + objRead.c_empresa + " - " + objRead.e_numero_legajo + "' - Linea: " + Linea.ToString());
                            Errores++;
                            continue;
                        }
                    }
                    //Recupero el Agente causante
                    if (objRead.c_agente_caus != "")
                    {
                        oiAGCAU = NomadEnvironment.QueryValue("ACC03_AGENTES_CAUS", "oi_agente_caus", "c_agente_caus", objRead.c_agente_caus, "", true);
                        if (oiAGCAU == null)
                        {
                            objBatch.Err("El agente causante no existe, se rechaza el registro '" + objRead.c_empresa + " - " + objRead.e_numero_legajo + "' - Linea: " + Linea.ToString());
                            Errores++;
                            continue;
                        }
                    }
                    //Recupero la condición peligrosa
                    if (objRead.c_condic_pelig != "")
                    {
                        oiCONPEL = NomadEnvironment.QueryValue("ACC04_CONDIC_PELIG", "oi_condic_pelig", "c_condic_pelig", objRead.c_condic_pelig, "", true);
                        if (oiCONPEL == null)
                        {
                            objBatch.Err("La condición peligrosa no existe, se rechaza el registro '" + objRead.c_empresa + " - " + objRead.e_numero_legajo + "' - Linea: " + Linea.ToString());
                            Errores++;
                            continue;
                        }
                    }
                    //Recupero el factor de contagio
                    if (objRead.c_factor_cont != "")
                    {
                        oiFACCON = NomadEnvironment.QueryValue("ACC05_FACTOR_CONT", "oi_factor_cont", "c_factor_cont", objRead.c_factor_cont, "", true);
                        if (oiFACCON == null)
                        {
                            objBatch.Err("El factor de contagio no existe, se rechaza el registro '" + objRead.c_empresa + " - " + objRead.e_numero_legajo + "' - Linea: " + Linea.ToString());
                            Errores++;
                            continue;
                        }
                    }
                    //Recupero la región del cuerpo afectada
                    if (objRead.c_reg_cuerpo != "")
                    {
                        oiREGCUE = NomadEnvironment.QueryValue("ACC09_REG_CUERPO", "oi_reg_cuerpo", "c_reg_cuerpo", objRead.c_reg_cuerpo, "", true);
                        if (oiREGCUE == null)
                        {
                            objBatch.Err("La región del cuerpo no existe, se rechaza el registro '" + objRead.c_empresa + " - " + objRead.e_numero_legajo + "' - Linea: " + Linea.ToString());
                            Errores++;
                            continue;
                        }
                    }
                    //Recupero la naturaleza de la lesión
                    if (objRead.c_natur_lesion != "")
                    {
                        oiNATLES = NomadEnvironment.QueryValue("ACC09_REG_CUERPO", "oi_natur_lesion", "c_natur_lesion", objRead.c_natur_lesion, "", true);
                        if (oiNATLES == null)
                        {
                            objBatch.Err("La naturaleza de la lesión no existe, se rechaza el registro '" + objRead.c_empresa + " - " + objRead.e_numero_legajo + "' - Linea: " + Linea.ToString());
                            Errores++;
                            continue;
                        }
                    }
                    //Recupero la Incapacidad 
                    if (objRead.c_incapacidad != "")
                    {
                        oiINCAPA = NomadEnvironment.QueryValue("ACC06_INCAPACIDAD", "oi_incapacidad", "c_incapacidad", objRead.c_incapacidad, "", true);
                        if (oiINCAPA == null)
                        {
                            objBatch.Err("La incapacidad no existe, se rechaza el registro '" + objRead.c_empresa + " - " + objRead.e_numero_legajo + "' - Linea: " + Linea.ToString());
                            Errores++;
                            continue;
                        }
                    }
                    //Recupero el elemento
                    if (objRead.c_elemento_pp != "")
                    {
                        oiELEPP = NomadEnvironment.QueryValue("ORG07_ELEMENTOS_PP", "oi_elemento_pp", "c_elemento_pp", objRead.c_elemento_pp, "", true);
                        if (oiELEPP == null)
                        {
                            objBatch.Err("El elemento no existe, se rechaza el registro '" + objRead.c_empresa + " - " + objRead.e_numero_legajo + "' - Linea: " + Linea.ToString());
                            Errores++;
                            continue;
                        }
                    }
                    //Recupero la ocupación
                    if (objRead.c_ocupacion != "")
                    {
                        oiOCUPA = NomadEnvironment.QueryValue("ACC08_OCUPACIONES", "oi_ocupacion", "c_ocupacion", objRead.c_ocupacion, "", true);
                        if (oiOCUPA == null)
                        {
                            objBatch.Err("La ocupación no existe, se rechaza el registro '" + objRead.c_empresa + " - " + objRead.e_numero_legajo + "' - Linea: " + Linea.ToString());
                            Errores++;
                            continue;
                        }
                    }
                    //Recupero el tipo de siniestro
                    if (objRead.c_tipo_siniestro != "")
                    {
                        oiTIPSIN = NomadEnvironment.QueryValue("ACC13_TIP_SINIESTRO", "oi_tipo_siniestro", "c_tipo_siniestro", objRead.c_tipo_siniestro, "", true);
                        if (oiTIPSIN == null)
                        {
                            objBatch.Err("El tipo de siniestro no existe, se rechaza el registro '" + objRead.c_empresa + " - " + objRead.e_numero_legajo + "' - Linea: " + Linea.ToString());
                            Errores++;
                            continue;
                        }
                    }
                    //Recupero la licencia
                    if (objRead.c_licencia != "")
                    {
                        oiLIC = NomadEnvironment.QueryValue("PER02_LICEN_PER", "oi_licencia_per", "oi_licencia_per", objRead.c_licencia, "", true);
                        if (oiLIC == null)
                        {
                            objBatch.Err("La licencia no existe, se rechaza el registro '" + objRead.c_empresa + " - " + objRead.e_numero_legajo + "' - Linea: " + Linea.ToString());
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
                        DDOLEG = NucleusRH.Base.Accidentabilidad.LegajoAccidentes.PERSONAL_EMP.Get(oiPEREMP);
                    }

                    //Me fijo si ya existe un registro de Accidente en la fecha especificada para la persona  	
                    if (DDOLEG.ACCIDENTES.GetByAttribute("f_fechahora_acc", objRead.f_fechahora_acc) != null)
                    {
                        objBatch.Err("Ya existe un Accidente en el Legajo para la Fecha, '" + objRead.c_empresa + " - " + objRead.e_numero_legajo + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }

                    //Creo el Accidente
                    NucleusRH.Base.Accidentabilidad.LegajoAccidentes.ACCIDENTE DDOACC;
                    DDOACC = new NucleusRH.Base.Accidentabilidad.LegajoAccidentes.ACCIDENTE();
                    DDOACC.oi_personal_emp = int.Parse(oiPEREMP);
                    DDOACC.oi_acto_inseg = oiACTINS;
                    DDOACC.oi_agente_caus = oiAGCAU;
                    DDOACC.oi_condic_pelig = oiCONPEL;
                    DDOACC.oi_elemento_pp = oiELEPP;
                    DDOACC.oi_factor_cont = oiFACCON;
                    DDOACC.oi_incapacidad = oiINCAPA;
                    DDOACC.oi_licencia = int.Parse(oiLIC);
                    DDOACC.oi_natur_lesion = oiNATLES;
                    DDOACC.oi_ocupacion = oiOCUPA;
                    DDOACC.oi_reg_cuerpo = oiREGCUE;
                    DDOACC.oi_tipo_acc = oiTIPACC;
                    DDOACC.f_fechahora_acc = objRead.f_fechahora_acc;
                    DDOACC.f_fechahora_alta = objRead.f_fechahora_alta;
                    DDOACC.f_fechahora_altaNull = objRead.f_fechahora_altaNull;
                    DDOACC.f_fechahora_baja = objRead.f_fechahora_baja;
                    DDOACC.f_fechahora_bajaNull = objRead.f_fechahora_bajaNull;
                    DDOACC.f_fechahora_revis = objRead.f_fechahora_revis;
                    DDOACC.f_fechahora_revisNull = objRead.f_fechahora_revisNull;
                    //DDOACC.e_condicion_riesgo = objRead.e_condicion_riesgo;
                    DDOACC.e_dias_perdidos = objRead.e_dias_perdidos;
                    DDOACC.o_accidente = objRead.o_accidente;
                    DDOACC.o_accion_aconsej = objRead.o_accion_aconsej;
                    DDOACC.n_porc_grado = objRead.n_porc_grado;
                    DDOACC.e_valor_grado = objRead.e_valor_grado;
                    if (objRead.l_denunciadoNull)
                        DDOACC.l_denunciado = false;
                    DDOACC.l_denunciado = objRead.l_denunciado;

                    //Agrego el Accidente
                    DDOLEG.ACCIDENTES.Add(DDOACC);
                }
                catch (Exception e)
                {
                    objBatch.Err("Error desconocido en registro, por favor verifique el formato del archivo y la correcta asignaci&#243;n de los campos - Linea: " + Linea.ToString() + " - " + e.Message);
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

using System;
using System.Xml;
using System.IO;
using System.Collections;

using Nomad.NSystem.Document;
using Nomad.NSystem.Proxy;
using Nomad.NSystem.Base;
using Nomad.NSystem.Functions;

namespace NucleusRH.Base.MedicinaLaboral.MigLegMed
{

    //////////////////////////////////////////////////////////////////////////////////
    //Clase Legajos de Medicina
    public partial class LEGAJO_MEDICINA
    {
        public static void ImportarLegMed()
        {

            //Instancio la Persona
            NucleusRH.Base.MedicinaLaboral.LegajoMedicina.PERSONAL_EMP DDOLEG = null;

            //Instancio el DICTADO
            int Linea = 0, Errores = 0;
            string oiPERANT = "";

            NomadBatch objBatch;
            objBatch = NomadBatch.GetBatch("Iniciando...", "Importación de Legajos Medicina");

            NomadXML IDCur;
            NomadXML IDList = new NomadXML();
            NucleusRH.Base.MedicinaLaboral.MigLegMed.LEGAJO_MEDICINA objRead;

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
                    objRead = NucleusRH.Base.MedicinaLaboral.MigLegMed.LEGAJO_MEDICINA.Get(row.GetAttr("id"));
					
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
                    if (objRead.f_fechahora_consNull)
                    {
                        objBatch.Err("No se especificó la fecha hora del Accidente, se rechaza el registro '" + objRead.c_empresa + " - " + objRead.e_numero_legajo + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }
                    if (objRead.c_motivo_consulta == "")
                    {
                        objBatch.Err("No se especificó el Motivo de consulta, se rechaza el registro '" + objRead.c_empresa + " - " + objRead.e_numero_legajo + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }

                    //Recupero los OI de los c&#243;digos ingresados
                    string oiEMP = "", oiPEREMP = "", oiMOTCON = "", oiENF = "", oiLIC = "", oiLicPER = ""; 					

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
                    //Recupero el motivo de consulta
                    if (objRead.c_motivo_consulta != "")
                    {
                        oiMOTCON = NomadEnvironment.QueryValue("MED05_MOTIVOS_CONS", "oi_motivo_consulta", "c_motivo_consulta", objRead.c_motivo_consulta, "", true);
                        if (oiMOTCON == null)
                        {
                            objBatch.Err("El motivo de consulta no existe, se rechaza el registro '" + objRead.c_empresa + " - " + objRead.e_numero_legajo + "' - Linea: " + Linea.ToString());
                            Errores++;
                            continue;
                        }
                    }
                    //Recupero la enfermedad
                    if (objRead.c_enfermedad != "")
                    {
                        oiENF = NomadEnvironment.QueryValue("MED06_ENFERMEDADES", "oi_enfermedad", "c_enfermedad", objRead.c_enfermedad, "", true);
                        if (oiENF == null)
                        {
                            objBatch.Err("La enfermedad no existe, se rechaza el registro '" + objRead.c_empresa + " - " + objRead.e_numero_legajo + "' - Linea: " + Linea.ToString());
                            Errores++;
                            continue;
                        }
                    }
                    //Recupero la licencia
                    if (objRead.c_licencia != "")
                    {
                        oiLIC = NomadEnvironment.QueryValue("PER16_LICENCIAS", "oi_licencia", "c_licencia", objRead.c_licencia, "", true);
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
						//Traigo el legajo
                        DDOLEG = NucleusRH.Base.MedicinaLaboral.LegajoMedicina.PERSONAL_EMP.Get(oiPEREMP);
                    }

                    //Me fijo si ya existe un registro de Medicina en la fecha especificada para la persona
                    if (DDOLEG.HIST_CLINICA.GetByAttribute("f_fechahora_cons", objRead.f_fechahora_cons) != null)
                    {
                        objBatch.Err("Ya existe una consulta médica en el Legajo para la Fecha, '" + objRead.c_empresa + " - " + objRead.e_numero_legajo + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }
					//Solo si el cod de licencia tiene una E voy a crear una lic en per
					if(objRead.c_licencia != ""){
						//Creo la licencia per
						NucleusRH.Base.Personal.LegajoEmpresa.LICENCIA_PER ddoLicPer = NucleusRH.Base.Personal.LegajoEmpresa.LICENCIA_PER.New();
						ddoLicPer.oi_licencia = oiLIC;					
						ddoLicPer.f_inicio = objRead.f_baja;
						if (!objRead.f_prev_altaNull)
							ddoLicPer.f_fin = objRead.f_prev_alta.AddDays(-1);
						if (!objRead.f_altaNull)
							ddoLicPer.f_fin = objRead.f_alta.AddDays(-1);
						ddoLicPer.e_cant_dias = ddoLicPer.f_fin.Subtract(ddoLicPer.f_inicio).Days + 1;
						ddoLicPer.e_anio_corresp = ddoLicPer.f_inicio.Year;
						ddoLicPer.l_bloqueada = false;
						ddoLicPer.l_interfaz = false;
						ddoLicPer.l_habiles = false;
						if (objRead.o_consulta_per != "")
							ddoLicPer.o_licencia_per = objRead.o_consulta_per;
						
						//Validar Licencia
						try
						{
							NucleusRH.Base.Personal.LegajoEmpresa.LICENCIA_PER.ValidarAltaLicencia
							(oiPEREMP, oiLIC, ddoLicPer.f_inicio, ddoLicPer.f_fin, ddoLicPer.e_cant_dias, ddoLicPer.e_anio_corresp);
						}
						catch (Exception e)
						{
							switch (e.Message)
							{
							case "LegajoEmpresa.LICENCIA_PER.ERR-FECHA":
								objBatch.Err("La fecha de fin debe ser mayor o igual a la fecha de inicio. - Linea: " + Linea.ToString());
								return;
							case "LegajoEmpresa.LICENCIA_PER.ERR-SOLAP":
								objBatch.Err("Existe un solapamiento de fechas con otras Licencias cargadas para el Legajo - Linea: " + Linea.ToString());
								return;
							case "Personal.NOVEDAD.ERR-LICENCIA":
								objBatch.Err("Existe un solapamiento de fechas con Novedades cargadas para el Legajo - Linea: " + Linea.ToString());
								return;
							case "LegajoEmpresa.LICENCIA_PER.ERR-MIN-ANTIG":
								objBatch.Err("El legajo tiene una antigüedad inferior al mínimo requerido - Linea: " + Linea.ToString());
								return;
							case "LegajoEmpresa.LICENCIA_PER.ERR-MAX-DIAS":
								objBatch.Err("La cantidad de días consecutivos supera el máximo - Linea: " + Linea.ToString());
								return;
							case "LegajoEmpresa.LICENCIA_PER.ERR-MAX-ANIO":
								objBatch.Err("La cantidad de días supera el saldo anual restante - Linea: " + Linea.ToString());
								return;
							default:
								objBatch.Err("Error desconocido");
								return;
							}
						}
						//Agregar Licencia
						DDOLEG.LICEN_PER.Add(ddoLicPer);
	
						//Guardar Legajo
						NomadEnvironment.GetCurrentTransaction().SaveRefresh(DDOLEG);
						objBatch.SetMess("Se guardo la licencia en el modulo personal");	
						
						//Obtengo el oi de la licencia_per generada						
						NomadXML xmlParam = new NomadXML("PARAM");
						xmlParam.SetAttr("oi_personal_emp", oiPEREMP);
						xmlParam.SetAttr("f_inicio", ddoLicPer.f_inicio);
						NomadXML xmlqry = NomadEnvironment.QueryNomadXML(NucleusRH.Base.Personal.LegajoEmpresa.PERSONAL_EMP.Resources.QRY_LICEN_PER, xmlParam.ToString());
						oiLicPER = xmlqry.FirstChild().GetAttr("oi_licencia_per");
						objBatch.SetMess("oiLicPER -- " + oiLicPER);
					}	
                    //Creo la consulta médica
                    NucleusRH.Base.MedicinaLaboral.LegajoMedicina.CONSULTA_PER DDOCON;
                    DDOCON = new NucleusRH.Base.MedicinaLaboral.LegajoMedicina.CONSULTA_PER();
                    DDOCON.oi_personal_emp = int.Parse(oiPEREMP);
                    DDOCON.oi_motivo_consulta = oiMOTCON;
                    DDOCON.oi_enfermedad = oiENF;
                    if(oiLicPER!="")
                      DDOCON.oi_licencia =  int.Parse(oiLicPER);
                    DDOCON.f_fechahora_cons = objRead.f_fechahora_cons;
                    DDOCON.f_alta = objRead.f_alta;
                    DDOCON.f_altaNull = objRead.f_altaNull;
                    DDOCON.f_baja = objRead.f_baja;
                    DDOCON.f_bajaNull = objRead.f_bajaNull;
                    DDOCON.f_prev_alta = objRead.f_prev_alta;
                    DDOCON.f_prev_altaNull = objRead.f_prev_altaNull;
                    DDOCON.o_consulta_per = objRead.o_consulta_per;
					DDOCON.e_dias_perdidos = objRead.e_dias_perdidos;

                    //Agrego La consulta
                    DDOLEG.HIST_CLINICA.Add(DDOCON);
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


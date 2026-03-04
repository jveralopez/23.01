using System;
using System.Xml;
using System.IO;
using System.Collections;

using Nomad.NSystem.Document;
using Nomad.NSystem.Proxy;
using Nomad.NSystem.Base;
using Nomad.NSystem.Functions;

namespace NucleusRH.Base.Migracion.Personal.Personas
{

    //////////////////////////////////////////////////////////////////////////////////
    //Clase Persona
    public partial class PERSONAL 
    {
        public static void ImportarPersonas()
        {
            int Linea = 0, Errores = 0;

            NomadBatch objBatch;
            objBatch = NomadBatch.GetBatch("Iniciando...", "Importación de Personas");
            
            NomadXML IDList = new NomadXML();
            NucleusRH.Base.Migracion.Personal.Personas.PERSONAL objRead;            

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
                    objRead = NucleusRH.Base.Migracion.Personal.Personas.PERSONAL.Get(row.GetAttr("id"));

                    //Valido atributos obligatorios  	                                             
                    if (objRead.d_apellido == "")
                    {
                        objBatch.Err("No se especificó el Apellido, se rechaza el registro '" + objRead.c_tipo_documento + objRead.c_nro_documento + " - " + objRead.d_apellido + ", " + objRead.d_nombres + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }
                    if (objRead.d_nombres == "")
                    {
                        objBatch.Err("No se especificó el Nombre, se rechaza el registro '" + objRead.c_tipo_documento + objRead.c_nro_documento + " - " + objRead.d_apellido + ", " + objRead.d_nombres + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }
                    if (objRead.c_tipo_documento == "")
                    {
                        objBatch.Err("No se especificó el Tipo de Documento, se rechaza el registro '" + objRead.c_tipo_documento + objRead.c_nro_documento + " - " + objRead.d_apellido + ", " + objRead.d_nombres + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }
                    if (objRead.c_nro_documento == "")
                    {
                        objBatch.Err("No se especificó el Número de Documento, se rechaza el registro '" + objRead.c_tipo_documento + objRead.c_nro_documento + " - " + objRead.d_apellido + ", " + objRead.d_nombres + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }
                    if (objRead.c_sexo != "M" && objRead.c_sexo != "F" && objRead.c_sexo != "m" && objRead.c_sexo != "f")
                    {
                        objBatch.Err("No se especificó el Sexo, se rechaza el registro '" + objRead.c_tipo_documento + objRead.c_nro_documento + " - " + objRead.d_apellido + ", " + objRead.d_nombres + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }

                    //Recupero los OI de los códigos ingresados
                    string oiTIPODOC = "", oiGS = "", oiLOCNAC = "", oiNAC = "", oiIDI = "", oiESTCIV = "", oiESTJUB = "", oiLOCRES = "", oiAFJP = "";

                    if (objRead.c_tipo_documento != "")
                    {
                        oiTIPODOC = NomadEnvironment.QueryValue("PER20_TIPOS_DOC", "oi_tipo_documento", "c_tipo_documento", objRead.c_tipo_documento, "", true);
                        if (oiTIPODOC == null)
                        {
                            objBatch.Err("El Tipo de Documento no existe, se rechaza el registro '" + objRead.c_tipo_documento + objRead.c_nro_documento + " - " + objRead.d_apellido + ", " + objRead.d_nombres + "' - Linea: " + Linea.ToString());
                            Errores++;
                            continue;
                        }
                    }

                    if (objRead.c_grupo_sanguineo != "")
                    {
                        oiGS = NomadEnvironment.QueryValue("PER10_GRUPOS_SANG", "oi_grupo_sanguineo", "c_grupo_sanguineo", objRead.c_grupo_sanguineo, "", true);
                        if (oiGS == null)
                        {
                            objBatch.Err("El Grupo Sanguineo no existe, se rechaza el registro '" + objRead.c_tipo_documento + objRead.c_nro_documento + " - " + objRead.d_apellido + ", " + objRead.d_nombres + "' - Linea: " + Linea.ToString());
                            Errores++;
                            continue;
                        }
                    }

                    if (objRead.c_localidad_nac != "")
                    {
                        oiLOCNAC = NomadEnvironment.QueryValue("ORG19_LOCALIDADES", "oi_localidad", "c_localidad", objRead.c_localidad_nac, "", true);
                        if (oiLOCNAC == null)
                        {
                            objBatch.Err("La Localidad de Nacimiento no existe, se rechaza el registro '" + objRead.c_tipo_documento + objRead.c_nro_documento + " - " + objRead.d_apellido + ", " + objRead.d_nombres + "' - Linea: " + Linea.ToString());
                            Errores++;
                            continue;
                        }
                    }

                    if (objRead.c_nacionalidad != "")
                    {
                        oiNAC = NomadEnvironment.QueryValue("ORG12_NACIONALID", "oi_nacionalidad", "c_nacionalidad", objRead.c_nacionalidad, "", true);
                        if (oiNAC == null)
                        {
                            objBatch.Err("La Nacionalidad no existe, se rechaza el registro '" + objRead.c_tipo_documento + objRead.c_nro_documento + " - " + objRead.d_apellido + ", " + objRead.d_nombres + "' - Linea: " + Linea.ToString());
                            Errores++;
                            continue;
                        }
                    }

                    if (objRead.c_idioma != "")
                    {
                        oiIDI = NomadEnvironment.QueryValue("ORG20_IDIOMAS", "oi_idioma", "c_idioma", objRead.c_idioma, "", true);
                        if (oiIDI == null)
                        {
                            objBatch.Err("El Idioma no existe, se rechaza el registro '" + objRead.c_tipo_documento + objRead.c_nro_documento + " - " + objRead.d_apellido + ", " + objRead.d_nombres + "' - Linea: " + Linea.ToString());
                            Errores++;
                            continue;
                        }
                    }

                    if (objRead.c_estado_civil != "")
                    {
                        oiESTCIV = NomadEnvironment.QueryValue("ORG22_EST_CIVIL", "oi_estado_civil", "c_estado_civil", objRead.c_estado_civil, "", true);
                        if (oiESTCIV == null)
                        {
                            objBatch.Err("El Estado Civil no existe, se rechaza el registro '" + objRead.c_tipo_documento + objRead.c_nro_documento + " - " + objRead.d_apellido + ", " + objRead.d_nombres + "' - Linea: " + Linea.ToString());
                            Errores++;
                            continue;
                        }
                    }

                    if (objRead.c_estado_jubi != "")
                    {
                        oiESTJUB = NomadEnvironment.QueryValue("PER14_ESTADOS_JUBI", "oi_estado_jubi", "c_estado_jubi", objRead.c_estado_jubi, "", true);
                        if (oiESTJUB == null)
                        {
                            objBatch.Err("El Estado Jubilatorio no existe, se rechaza el registro '" + objRead.c_tipo_documento + objRead.c_nro_documento + " - " + objRead.d_apellido + ", " + objRead.d_nombres + "' - Linea: " + Linea.ToString());
                            Errores++;
                            continue;
                        }
                    }

                    if (objRead.c_localidad_res != "")
                    {
                        oiLOCRES = NomadEnvironment.QueryValue("ORG19_LOCALIDADES", "oi_localidad", "c_localidad", objRead.c_localidad_res, "", true);
                        if (oiLOCRES == null)
                        {
                            objBatch.Err("La Localidad de Residencia no existe, se rechaza el registro '" + objRead.c_tipo_documento + objRead.c_nro_documento + " - " + objRead.d_apellido + ", " + objRead.d_nombres + "' - Linea: " + Linea.ToString());
                            Errores++;
                            continue;
                        }
                    }

                    if (objRead.c_afjp != "")
                    {
                        oiAFJP = NomadEnvironment.QueryValue("PER03_AFJP", "oi_afjp", "c_afjp", objRead.c_afjp, "", true);
                        if (oiAFJP == null)
                        {
                            objBatch.Err("La AFJP no existe, se rechaza el registro '" + objRead.c_tipo_documento + objRead.c_nro_documento + " - " + objRead.d_apellido + ", " + objRead.d_nombres + "' - Linea: " + Linea.ToString());
                            Errores++;
                            continue;
                        }
                    }

                    if(objRead.c_nro_cuil != "")
                    {
                        bool bndMasc = objRead.c_sexo.ToUpper() == "M" ? true : false;

                        System.Text.RegularExpressions.Regex rgx = new System.Text.RegularExpressions.Regex(@"^([0-9])*$");
                        if (rgx.IsMatch(objRead.c_nro_cuil))
                        {
                            if(objRead.c_nro_cuil.Length == 11)
                            {
                                int lngSexo = int.Parse(objRead.c_nro_cuil.Substring(0, 2));
                                int lngVerificador = int.Parse(objRead.c_nro_cuil.Substring(objRead.c_nro_cuil.Length - 1, 1));
                                string strSexoyDocumento = objRead.c_nro_cuil.Substring(0, 10);

                                if (intDigVerificador(strSexoyDocumento) == lngVerificador)
                                {
                                    string oiPersona = NomadEnvironment.QueryValue("PER01_PERSONAL", "oi_personal", "c_nro_cuil", objRead.c_nro_cuil, "", false);
                                    if(oiPersona != "")
                                    {
                                        objBatch.Err("El CUIT/CUIL ingresado ya se encuentra registrado en otra Persona. Se rechaza el registro '" + objRead.c_tipo_documento + objRead.c_nro_documento + " - " + objRead.d_apellido + ", " + objRead.d_nombres + "' - Linea: " + Linea.ToString());
                                        Errores++;
                                        continue;
                                    }
                                    else
                                    {
                                        string cuit2 = lngSexo + "-" + objRead.c_nro_cuil.Substring(2, 8) + "-" + lngVerificador;
                                        oiPersona = NomadEnvironment.QueryValue("PER01_PERSONAL", "oi_personal", "c_nro_cuil", cuit2, "", false);
                                        if (oiPersona != "")
                                        {
                                            objBatch.Err("El CUIT/CUIL ingresado ya se encuentra registrado en otra Persona. Se rechaza el registro '" + objRead.c_tipo_documento + objRead.c_nro_documento + " - " + objRead.d_apellido + ", " + objRead.d_nombres + "' - Linea: " + Linea.ToString());
                                            Errores++;
                                            continue;
                                        }
                                    }
                                }
                                else
                                {
                                    objBatch.Err("El dígito verificador del CUIT/CUIL es incorrecto. Se rechaza el registro '" + objRead.c_tipo_documento + objRead.c_nro_documento + " - " + objRead.d_apellido + ", " + objRead.d_nombres + "' - Linea: " + Linea.ToString());
                                    Errores++;
                                    continue;
                                }
                            }
                            else
                            {
                                objBatch.Err("El largo del CUIT/CUIL es incorrecto. Se rechaza el registro '" + objRead.c_tipo_documento + objRead.c_nro_documento + " - " + objRead.d_apellido + ", " + objRead.d_nombres + "' - Linea: " + Linea.ToString());
                                Errores++;
                                continue;
                            }
                        }
                        else
                        {
                            objBatch.Err("El CUIT/CUIL no puede contener caracteres especiales, solo números. Se rechaza el registro '" + objRead.c_tipo_documento + objRead.c_nro_documento + " - " + objRead.d_apellido + ", " + objRead.d_nombres + "' - Linea: " + Linea.ToString());
                            Errores++;
                            continue;
                        }
                    }


                    //Me fijo si ya existe la Persona  	
                    string oiVal = NomadEnvironment.QueryValue("PER01_PERSONAL", "oi_personal", "c_personal", objRead.c_tipo_documento + objRead.c_nro_documento, "", true);
                    if (oiVal != null)
                    {
                        objBatch.Err("Ya existe un registro para la Persona, se rechaza el registro '" + objRead.c_tipo_documento + objRead.c_nro_documento + " - " + objRead.d_apellido + ", " + objRead.d_nombres + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }
                    else
                    {
                        //Creo la Persona
                        NucleusRH.Base.Personal.Legajo.PERSONAL DDOPER;
                        DDOPER = new NucleusRH.Base.Personal.Legajo.PERSONAL();

                        DDOPER.c_nro_buzo = objRead.c_nro_buzo;
                        DDOPER.c_nro_calzado = objRead.c_nro_calzado;
                        DDOPER.c_nro_camisa = objRead.c_nro_camisa;
                        DDOPER.c_nro_campera = objRead.c_nro_campera;
                        DDOPER.c_nro_chomba = objRead.c_nro_chomba;
                        DDOPER.c_nro_cuil = objRead.c_nro_cuil;
                        DDOPER.c_nro_documento = objRead.c_nro_documento;
                        DDOPER.c_nro_jubilacion = objRead.c_nro_jubilacion;
                        DDOPER.c_nro_pantalon = objRead.c_nro_pantalon;
                        DDOPER.c_personal = objRead.c_tipo_documento + objRead.c_nro_documento;
                        DDOPER.c_sexo = objRead.c_sexo;
                        DDOPER.d_ape_y_nom = objRead.d_apellido + ", " + objRead.d_nombres;
                        DDOPER.d_apellido = objRead.d_apellido;
                        DDOPER.d_apellido_materno = objRead.d_apellido_materno;
                        DDOPER.d_email = objRead.d_email;
                        DDOPER.d_nombres = objRead.d_nombres;
                        DDOPER.f_casamiento = objRead.f_casamiento;
                        DDOPER.f_casamientoNull = objRead.f_casamientoNull;
                        DDOPER.f_desde_afjp = objRead.f_desde_afjp;
                        DDOPER.f_desde_afjpNull = objRead.f_desde_afjpNull;
                        DDOPER.f_estado_jubi = objRead.f_estado_jubi;
                        DDOPER.f_estado_jubiNull = objRead.f_estado_jubiNull;
                        DDOPER.f_fallecimiento = objRead.f_fallecimiento;
                        DDOPER.f_fallecimientoNull = objRead.f_fallecimientoNull;
                        DDOPER.f_ingreso_pais = objRead.f_ingreso_pais;
                        DDOPER.f_ingreso_paisNull = objRead.f_ingreso_paisNull;
                        DDOPER.f_nacim = objRead.f_nacim;
                        DDOPER.f_nacimNull = objRead.f_nacimNull;
                        DDOPER.l_jubilado = objRead.l_jubilado;
                        DDOPER.o_personal = objRead.o_personal;
                        if (oiAFJP != "") DDOPER.oi_afjp = oiAFJP;
                        if (oiESTCIV != "") DDOPER.oi_estado_civil = oiESTCIV;
                        if (oiESTJUB != "") DDOPER.oi_estado_jubi = oiESTJUB;
                        if (oiGS != "") DDOPER.oi_grupo_sanguineo = oiGS;
                        if (oiIDI != "") DDOPER.oi_idioma = oiIDI;
                        if (oiLOCNAC != "") DDOPER.oi_local_nacim = oiLOCNAC;
                        if (oiLOCRES != "") DDOPER.oi_localidad = oiLOCRES;
                        if (oiNAC != "") DDOPER.oi_nacionalidad = oiNAC;
                        if (oiTIPODOC != "") DDOPER.oi_tipo_documento = oiTIPODOC;
                        DDOPER.te_celular = objRead.te_celular;


                        //Creo un registro de AFJP en caso que la misma este ingresando
                        if (objRead.c_afjp != "")
                        {
                            NucleusRH.Base.Personal.Legajo.AFJP_PER DDOAFJPER;
                            DDOAFJPER = new NucleusRH.Base.Personal.Legajo.AFJP_PER();
                            DDOAFJPER.oi_afjp = oiAFJP;
                            if (!objRead.f_desde_afjpNull)
                            {
                                DDOAFJPER.f_ingreso = objRead.f_desde_afjp;
                            }
                            else
                            {
                                DDOAFJPER.f_ingreso = DateTime.Now.Date;
                            }
                            DDOPER.AFJP_PER.Add(DDOAFJPER);
                        }

                        //Creo un registro de DOCUMENTOS para la persona
                        NucleusRH.Base.Personal.Legajo.DOCUMENTO_PER DDODOCPER;
                        DDODOCPER = new NucleusRH.Base.Personal.Legajo.DOCUMENTO_PER();
                        DDODOCPER.c_documento = objRead.c_nro_documento;
                        DDODOCPER.oi_tipo_documento = oiTIPODOC;
                        DDOPER.DOCUM_PER.Add(DDODOCPER);

                        //Grabo
                        try
                        {
                            NomadEnvironment.GetCurrentTransaction().Save(DDOPER);
                            NomadEnvironment.QueryValueChange("PER01_PERSONAL", "oi_personal", "c_personal", objRead.c_tipo_documento + objRead.c_nro_documento, "","1", true);
                        }
                        catch (Exception e)
                        {
                            objBatch.Err("Error al grabar registro " + objRead.c_tipo_documento + objRead.c_nro_documento + " - " + objRead.d_apellido + ", " + objRead.d_nombres + "' - Linea: " + Linea.ToString() + " - " + e.Message);
                            Errores++;
                        }
                    }
                }
                catch (Exception e)
                {
                    objBatch.Err("Error desconocido en registro, por favor verifique el formato del archivo y la correcta asignación de los campos - Linea: " + Linea.ToString() + " - " + e.Message);
                    Errores++;
                }
            }

            objBatch.Log("Registros Procesados: " + Linea.ToString() + " - Importados: " + (Linea - Errores).ToString());
            objBatch.Log("Finalizado...");

        }

        private static int intDigVerificador(string strSexoyDocumento)
        {
            int dblSuma;
            int lngResto;
            int intDigVerificador;

            dblSuma = int.Parse(strSexoyDocumento.Substring(0, 1)) * 5;
            dblSuma = dblSuma + int.Parse(strSexoyDocumento.Substring(1, 1)) * 4;
            dblSuma = dblSuma + int.Parse(strSexoyDocumento.Substring(2, 1)) * 3;
            dblSuma = dblSuma + int.Parse(strSexoyDocumento.Substring(3, 1)) * 2;
            dblSuma = dblSuma + int.Parse(strSexoyDocumento.Substring(4, 1)) * 7;
            dblSuma = dblSuma + int.Parse(strSexoyDocumento.Substring(5, 1)) * 6;
            dblSuma = dblSuma + int.Parse(strSexoyDocumento.Substring(6, 1)) * 5;
            dblSuma = dblSuma + int.Parse(strSexoyDocumento.Substring(7, 1)) * 4;
            dblSuma = dblSuma + int.Parse(strSexoyDocumento.Substring(8, 1)) * 3;
            dblSuma = dblSuma + int.Parse(strSexoyDocumento.Substring(9, 1)) * 2;

            lngResto = dblSuma % 11;
            if (lngResto == 0)
            {
                return 0;
            }
            else
            {
                return (11 - lngResto);
            }
        }

    }
}

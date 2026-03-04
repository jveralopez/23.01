using System;
using System.Xml;
using System.IO;
using System.Collections;

using Nomad.NSystem.Document;
using Nomad.NSystem.Proxy;
using Nomad.NSystem.Base;
using Nomad.NSystem.Functions;

namespace NucleusRH.Base.Migracion.Personal.Familiares
{

    //////////////////////////////////////////////////////////////////////////////////
    //Clase Familiares
    public partial class FAMILIAR_PER 
    {
        public static void ImportarFamiliares()
        {

            int Linea = 0, Errores = 0;
            string oiPERANT = "";

            NomadBatch objBatch;
            objBatch = NomadBatch.GetBatch("Iniciando...", "Importación de Familiares");

            Hashtable htPARENTS = new Hashtable();            
            NomadXML IDList = new NomadXML();
            NucleusRH.Base.Migracion.Personal.Familiares.FAMILIAR_PER objRead;            

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
                    objRead = NucleusRH.Base.Migracion.Personal.Familiares.FAMILIAR_PER.Get(row.GetAttr("id"));
                    if (objRead.c_persona == "")
                    {
                        objBatch.Err("No se especificó la Persona, se rechaza el registro '" + objRead.c_familiar + " - " + objRead.d_apellido + ", " + objRead.d_nombres + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }
                    if (objRead.c_familiar == "")
                    {
                        objBatch.Err("No se especificó el Código, se rechaza el registro '" + objRead.c_familiar + " - " + objRead.d_apellido + ", " + objRead.d_nombres + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }
                    if (objRead.c_tipo_familiar == "")
                    {
                        objBatch.Err("No se especificó el Tipo de Familiar, se rechaza el registro '" + objRead.c_familiar + " - " + objRead.d_apellido + ", " + objRead.d_nombres + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }
                    if (objRead.d_apellido == "")
                    {
                        objBatch.Err("No se especificó el Apellido, se rechaza el registro '" + objRead.c_familiar + " - " + objRead.d_apellido + ", " + objRead.d_nombres + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }
                    if (objRead.d_nombres == "")
                    {
                        objBatch.Err("No se especificó el Nombre, se rechaza el registro '" + objRead.c_familiar + " - " + objRead.d_apellido + ", " + objRead.d_nombres + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }

                    //Recupero los OI de los códigos ingresados
                    string oiPER = "", oiTIPOFAM = "", oiOCUFAM = "", oiNESC = "", oiGESC = "", oiEST = "", oiUTPO = "", oiESTCIV = "", oiNAC = "", oiTIPODOC = "", oiLOC = ""; ;

                    if (objRead.c_tipo_documento != "")
                    {
                        oiTIPODOC = NomadEnvironment.QueryValue("PER20_TIPOS_DOC", "oi_tipo_documento", "c_tipo_documento", objRead.c_tipo_documento, "", true);
                        if (oiTIPODOC == null)
                        {
                            objBatch.Err("El Tipo de Documento no existe, se rechaza el registro '" + objRead.c_familiar + " - " + objRead.d_apellido + ", " + objRead.d_nombres + "' - Linea: " + Linea.ToString());
                            Errores++;
                            continue;
                        }
                    }

                    if (objRead.c_localidad_nac != "")
                    {
                        oiLOC = NomadEnvironment.QueryValue("ORG19_LOCALIDADES", "oi_localidad", "c_localidad", objRead.c_localidad_nac, "", true);
                        if (oiLOC == null)
                        {
                            objBatch.Err("La Localidad de Nacimiento no existe, se rechaza el registro '" + objRead.c_familiar + " - " + objRead.d_apellido + ", " + objRead.d_nombres + "' - Linea: " + Linea.ToString());
                            Errores++;
                            continue;
                        }
                    }

                    if (objRead.c_nacionalidad != "")
                    {
                        oiNAC = NomadEnvironment.QueryValue("ORG12_NACIONALID", "oi_nacionalidad", "c_nacionalidad", objRead.c_nacionalidad, "", true);
                        if (oiNAC == null)
                        {
                            objBatch.Err("La Nacionalidad no existe, se rechaza el registro '" + objRead.c_familiar + " - " + objRead.d_apellido + ", " + objRead.d_nombres + "' - Linea: " + Linea.ToString());
                            Errores++;
                            continue;
                        }
                    }

                    if (objRead.c_estado_civil != "")
                    {
                        oiESTCIV = NomadEnvironment.QueryValue("ORG22_EST_CIVIL", "oi_estado_civil", "c_estado_civil", objRead.c_estado_civil, "", true);
                        if (oiESTCIV == null)
                        {
                            objBatch.Err("El Estado Civil no existe, se rechaza el registro '" + objRead.c_familiar + " - " + objRead.d_apellido + ", " + objRead.d_nombres + "' - Linea: " + Linea.ToString());
                            Errores++;
                            continue;
                        }
                    }

                    if (objRead.c_ocupacion_fam != "")
                    {
                        oiOCUFAM = NomadEnvironment.QueryValue("PER18_OCUP_FAM", "oi_ocupacion_fam", "c_ocupacion_fam", objRead.c_ocupacion_fam, "", true);
                        if (oiOCUFAM == null)
                        {
                            objBatch.Err("La Ocupación del Familiar no existe, se rechaza el registro '" + objRead.c_familiar + " - " + objRead.d_apellido + ", " + objRead.d_nombres + "' - Linea: " + Linea.ToString());
                            Errores++;
                            continue;
                        }
                    }

                    if (objRead.c_tipo_familiar != "")
                    {
                        oiTIPOFAM = NomadEnvironment.QueryValue("PER15_TIPOS_FAM", "oi_tipo_familiar", "c_tipo_familiar", objRead.c_tipo_familiar, "", true);
                        if (oiTIPOFAM == null)
                        {
                            objBatch.Err("El Tipo de Familiar no existe, se rechaza el registro '" + objRead.c_familiar + " - " + objRead.d_apellido + ", " + objRead.d_nombres + "' - Linea: " + Linea.ToString());
                            Errores++;
                            continue;
                        }
                    }

                    if (objRead.c_nivel_escol != "")
                    {
                        oiNESC = NomadEnvironment.QueryValue("PER08_NIVELES_ESC", "oi_nivel_escol", "c_nivel_escol", objRead.c_nivel_escol, "", true);
                        if (oiNESC == null)
                        {
                            objBatch.Err("El Nivel de Escolaridad no existe, se rechaza el registro '" + objRead.c_familiar + " - " + objRead.d_apellido + ", " + objRead.d_nombres + "' - Linea: " + Linea.ToString());
                            Errores++;
                            continue;
                        }
                    }

                    if (objRead.c_grado_escol != "")
                    {
                        oiGESC = NomadEnvironment.QueryValue("PER24_GRADOS_ESCOL", "oi_grado_escol", "c_grado_escol", objRead.c_grado_escol, "", true);
                        if (oiGESC == null)
                        {
                            objBatch.Err("El Grado de Escolaridad no existe, se rechaza el registro '" + objRead.c_familiar + " - " + objRead.d_apellido + ", " + objRead.d_nombres + "' - Linea: " + Linea.ToString());
                            Errores++;
                            continue;
                        }
                    }

                    if (objRead.c_estudio != "")
                    {
                        oiEST = NomadEnvironment.QueryValue("ORG11_ESTUDIOS", "oi_estudio", "c_estudio", objRead.c_estudio, "", true);
                        if (oiEST == null)
                        {
                            objBatch.Err("El Estudio no existe, se rechaza el registro '" + objRead.c_familiar + " - " + objRead.d_apellido + ", " + objRead.d_nombres + "' - Linea: " + Linea.ToString());
                            Errores++;
                            continue;
                        }
                    }

                    if (objRead.c_unidad_tiempo != "")
                    {
                        oiUTPO = NomadEnvironment.QueryValue("ORG25_UNIDADES_TPO", "oi_unidad_tiempo", "c_unidad_tiempo", objRead.c_unidad_tiempo, "", true);
                        if (oiUTPO == null)
                        {
                            objBatch.Err("La Unidad de Tiempo no existe, se rechaza el registro '" + objRead.c_familiar + " - " + objRead.d_apellido + ", " + objRead.d_nombres + "' - Linea: " + Linea.ToString());
                            Errores++;
                            continue;
                        }
                    }

                    //Cargo el Padre  	
                    NucleusRH.Base.Personal.Legajo.PERSONAL DDOPER = NucleusRH.Base.Migracion.Interfaces.INTERFACE.GetPersona(objRead.c_persona, htPARENTS);
                    if (DDOPER == null)
                    {
                        objBatch.Err("La Persona no existe, se rechaza el registro '" + objRead.c_familiar + " - " + objRead.d_apellido + ", " + objRead.d_nombres + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }

                    //Me fijo si ya existe el Familiar en la persona   	 	  	
                    if (DDOPER.FLIARES_PER.GetByAttribute("c_familiar", objRead.c_familiar) != null)
                    {
                        objBatch.Err("Ya existe un registro para el Familiar en la Persona, se rechaza el registro '" + objRead.c_familiar + " - " + objRead.d_apellido + ", " + objRead.d_nombres + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }

                    //Creo el Familiar			
                    NucleusRH.Base.Personal.Legajo.FAMILIAR_PER DDOFLIAR;
                    DDOFLIAR = new NucleusRH.Base.Personal.Legajo.FAMILIAR_PER();

                    //DDOFLIAR.c_familiar = objRead.c_familiar;
                    DDOFLIAR.c_nro_cuil = objRead.c_nro_cuil;
                    DDOFLIAR.c_nro_documento = objRead.c_nro_documento;
                    DDOFLIAR.c_nro_seguro_soc = objRead.c_nro_seguro_soc;
                    DDOFLIAR.c_sexo = objRead.c_sexo;
                    DDOFLIAR.d_ape_y_nom = objRead.d_apellido + ", " + objRead.d_nombres;
                    DDOFLIAR.d_apellido = objRead.d_apellido;
                    DDOFLIAR.d_nombres = objRead.d_nombres;
                    DDOFLIAR.d_resp_exp_doc = objRead.d_resp_exp_doc;
                    DDOFLIAR.e_anio_fin_esc = objRead.e_anio_fin_esc;
                    DDOFLIAR.e_anio_fin_escNull = objRead.e_anio_fin_escNull;
                    DDOFLIAR.e_anio_inic_esc = objRead.e_anio_inic_esc;
                    DDOFLIAR.e_anio_inic_escNull = objRead.e_anio_inic_escNull;
                    DDOFLIAR.e_duracion_estudio = objRead.e_duracion_estudio;
                    DDOFLIAR.e_duracion_estudioNull = objRead.e_duracion_estudioNull;
                    DDOFLIAR.e_periodo_en_curso = objRead.e_periodo_en_curso;
                    DDOFLIAR.e_periodo_en_cursoNull = objRead.e_periodo_en_cursoNull;
                    DDOFLIAR.f_desde_IG = objRead.f_desde_IG;
                    DDOFLIAR.f_desde_IGNull = objRead.f_desde_IGNull;
                    DDOFLIAR.f_fallecimiento = objRead.f_fallecimiento;
                    DDOFLIAR.f_fallecimientoNull = objRead.f_fallecimientoNull;
                    DDOFLIAR.f_hasta_IG = objRead.f_hasta_IG;
                    DDOFLIAR.f_hasta_IGNull = objRead.f_hasta_IGNull;
                    DDOFLIAR.f_nacimiento = objRead.f_nacimiento;
                    DDOFLIAR.f_nacimientoNull = objRead.f_nacimientoNull;
                    DDOFLIAR.l_acargo_af = objRead.l_acargo_af;
                    DDOFLIAR.l_acargo_IG = objRead.l_acargo_IG;
                    DDOFLIAR.l_acargo_os = objRead.l_acargo_os;
                    DDOFLIAR.l_discapacidad = objRead.l_discapacidad;
                    DDOFLIAR.l_reside_pais = objRead.l_reside_pais;
                    DDOFLIAR.l_vive = objRead.l_vive;
                    DDOFLIAR.o_acargo_af = objRead.o_acargo_af;
                    DDOFLIAR.o_acargo_ig = objRead.o_acargo_ig;
                    DDOFLIAR.o_acargo_os = objRead.o_acargo_os;
                    DDOFLIAR.o_familiar = objRead.o_familiar;
                    DDOFLIAR.o_ocupacion = objRead.o_ocupacion;
                    if (oiESTCIV != "") DDOFLIAR.oi_estado_civil = oiESTCIV;
                    if (oiEST != "") DDOFLIAR.oi_estudio = oiEST;
                    if (oiGESC != "") DDOFLIAR.oi_grado_escol = oiGESC;
                    if (oiLOC != "") DDOFLIAR.oi_localidad_nac = oiLOC;
                    if (oiNAC != "") DDOFLIAR.oi_nacionalidad = oiNAC;
                    if (oiNESC != "") DDOFLIAR.oi_nivel_escol = oiNESC;
                    if (oiOCUFAM != "") DDOFLIAR.oi_ocupacion_fam = oiOCUFAM;
                    if (oiTIPODOC != "") DDOFLIAR.oi_tipo_documento = oiTIPODOC;
                    if (oiTIPOFAM != "") DDOFLIAR.oi_tipo_familiar = oiTIPOFAM;
                    if (oiUTPO != "") DDOFLIAR.oi_unidad_tiempo = oiUTPO;

                    //Agrego el Familiar  	
                    DDOPER.FLIARES_PER.Add(DDOFLIAR);
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

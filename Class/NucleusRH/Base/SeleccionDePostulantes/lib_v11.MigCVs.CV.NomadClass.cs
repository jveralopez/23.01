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

namespace NucleusRH.Base.SeleccionDePostulantes.MigCVs
{

    //////////////////////////////////////////////////////////////////////////////////
    //Clase MigCVs
    public partial class CV : Nomad.NSystem.Base.NomadObject
    {
        public static void ImportarCVs()
        {
            int Linea = 0, Errores = 0;

            NomadBatch objBatch;
            objBatch = NomadBatch.GetBatch("Iniciando...", "Importación de CVs");

            NomadXML IDList = new NomadXML();
            NucleusRH.Base.SeleccionDePostulantes.MigCVs.CV objRead;            
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
                    objRead = NucleusRH.Base.SeleccionDePostulantes.MigCVs.CV.Get(row.GetAttr("id"));

                    //Valido atributos obligatorios  	                                             
                    if (objRead.c_cv == "")
                    {
                        objBatch.Err("No se especificó el CV, se rechaza el registro '" + objRead.c_tipo_documento + " - " + objRead.c_nro_doc + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }

                    //Valido atributos obligatorios  	                                             
                    if (objRead.c_tipo_documento == "")
                    {
                        objBatch.Err("No se especificó el Tipo de Documento, se rechaza el registro '" + objRead.c_tipo_documento + " - " + objRead.c_nro_doc + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }
                    if (objRead.c_nro_doc == "")
                    {
                        objBatch.Err("No se especificó el Número de Documento, se rechaza el registro '" + objRead.c_tipo_documento + " - " + objRead.c_nro_doc + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }
                    if (objRead.d_apellido == "")
                    {
                        objBatch.Err("No se especificó el apellido del CV, se rechaza el registro '" + objRead.c_tipo_documento + " - " + objRead.c_nro_doc + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }
                    if (objRead.d_nombres == "")
                    {
                        objBatch.Err("No se especificó el Nombre del CV, se rechaza el registro '" + objRead.c_tipo_documento + " - " + objRead.c_nro_doc + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }
                    if (objRead.c_sexo == "")
                    {
                        objBatch.Err("No se especificó el sexo del CV, se rechaza el registro '" + objRead.c_tipo_documento + " - " + objRead.c_nro_doc + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }
                    if (objRead.f_alta_cvNull || objRead.f_alta_cv < fCompare)
                    {
                        objBatch.Err("No se especificó Fecha de Alta del CV, se rechaza el registro '" + objRead.c_tipo_documento + " - " + objRead.c_nro_doc + "' - Linea: " + Linea.ToString());
                    }

                    //Recupero los OI de los códigos ingresados
                    string oiTIPO = "", oiESTCIV = "", oiPAIS = "", oiSITLAB = "", oiJORLAB = "";

                    if (objRead.c_tipo_documento != "")
                    {
                        oiTIPO = NomadEnvironment.QueryValue("PER20_TIPOS_DOC", "oi_tipo_documento", "c_tipo_documento", objRead.c_tipo_documento, "", true);
                        if (oiTIPO == null)
                        {
                            objBatch.Err("El Tipo de Documento no existe, se rechaza el registro '" + objRead.c_tipo_documento + " - " + objRead.c_nro_doc + "' - Linea: " + Linea.ToString());
                            Errores++;
                            continue;
                        }
                    }

                    if (objRead.c_estado_civil != "")
                    {
                        oiESTCIV = NomadEnvironment.QueryValue("ORG22_EST_CIVIL", "oi_estado_civil", "c_estado_civil", objRead.c_estado_civil, "", true);
                        if (oiESTCIV == null)
                        {
                            objBatch.Err("El estado Civil no existe, se rechaza el registro '" + objRead.c_tipo_documento + " - " + objRead.c_nro_doc + "' - Linea: " + Linea.ToString());
                            Errores++;
                            continue;
                        }
                    }

                    if (objRead.c_pais != "")
                    {
                        oiPAIS = NomadEnvironment.QueryValue("ORG35_PAISES", "oi_pais", "c_pais", objRead.c_pais, "", true);
                        if (oiPAIS == null)
                        {
                            objBatch.Err("El País no existe, se rechaza el registro '" + objRead.c_tipo_documento + " - " + objRead.c_nro_doc + "' - Linea: " + Linea.ToString());
                            Errores++;
                            continue;
                        }
                    }

                    if (objRead.c_tipo_jor_lab != "")
                    {
                        oiJORLAB = NomadEnvironment.QueryValue("SDP16_TIPOS_JORNADA", "c_tipo_jor_lab", "c_tipo_jor_lab", objRead.c_tipo_jor_lab, "", true);
                        if (oiJORLAB == null)
                        {
                            objBatch.Err("El Tipo de Jornada Laboral no existe, se rechaza el registro '" + objRead.c_tipo_documento + " - " + objRead.c_nro_doc + "' - Linea: " + Linea.ToString());
                            Errores++;
                            continue;
                        }
                    }

                    if (objRead.c_sit_laboral != "")
                    {
                        oiSITLAB = NomadEnvironment.QueryValue("SDP17_SIT_LABORALES", "c_sit_laboral", "c_sit_laboral", objRead.c_sit_laboral, "", true);
                        if (oiSITLAB == null)
                        {
                            objBatch.Err("La Situación Laboral no existe, se rechaza el registro '" + objRead.c_tipo_documento + " - " + objRead.c_nro_doc + "' - Linea: " + Linea.ToString());
                            Errores++;
                            continue;
                        }
                    }

                    //Me fijo si ya existe el CV  	
                    string oiVal = NomadEnvironment.QueryValue("SDP01_CV", "oi_cv", "c_cv", objRead.c_cv, "", true);
                    if (oiVal != null)
                    {
                        objBatch.Err("Ya existe un registro para el CV, se rechaza el registro '" + objRead.c_cv + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }
                    else
                    {
                        //Creo el CV
                        NucleusRH.Base.SeleccionDePostulantes.CVs.CV DDOCV;
                        DDOCV = new NucleusRH.Base.SeleccionDePostulantes.CVs.CV();

                        DDOCV.c_cv = objRead.c_cv;
                        DDOCV.c_nro_doc = objRead.c_nro_doc;
                        DDOCV.d_apellido = objRead.d_apellido;
                        DDOCV.d_nombres = objRead.d_nombres;
                        DDOCV.c_sexo = objRead.c_sexo;
                        DDOCV.f_nacim = objRead.f_nacim;
                        DDOCV.f_nacimNull = objRead.f_nacimNull;
                        DDOCV.c_nro_cuil = objRead.c_nro_cuil;
                        DDOCV.d_localidad = objRead.d_localidad;
                        DDOCV.d_calle = objRead.d_calle;
                        DDOCV.c_nro = objRead.c_nro;
                        DDOCV.c_piso = objRead.c_piso;
                        DDOCV.c_departamento = objRead.c_departamento;
                        DDOCV.c_password = objRead.c_password;
                        DDOCV.d_email = objRead.d_email;
                        DDOCV.c_pais_cel = objRead.c_pais_cel;
                        DDOCV.c_area_cel = objRead.c_area_cel;
                        DDOCV.te_celular = objRead.te_celular;
                        DDOCV.c_pais = objRead.c_pais_tel;
                        DDOCV.c_area = objRead.c_area;
                        DDOCV.te_nro = objRead.te_nro;
                        DDOCV.d_ape_y_nom = objRead.d_apellido + ", " + objRead.d_nombres;
                        DDOCV.f_actualizacion = objRead.f_actualizacion;
                        DDOCV.f_alta_cv = objRead.f_alta_cv;
                        DDOCV.c_estado = "A";
                        DDOCV.l_maillist = objRead.l_maillist;
                        DDOCV.c_lic_conduc = objRead.c_lic_conduc;
                        DDOCV.c_vehiculo = objRead.c_vehiculo;
                        DDOCV.n_remuneracion = objRead.n_remuneracion;
                        DDOCV.c_radicarse = objRead.c_radicarse;
                        DDOCV.c_viajar = objRead.c_viajar;
                        DDOCV.f_estado = DateTime.Now;

                        DDOCV.oi_tipo_documento = oiTIPO;
                        DDOCV.oi_estado_civil = oiESTCIV;
                        DDOCV.oi_pais = oiPAIS;

                        DDOCV.oi_sit_laboral = oiSITLAB;
                        DDOCV.oi_tipo_jor_lab = oiJORLAB;

                        //Grabo
                        try
                        {
                            NomadEnvironment.GetCurrentTransaction().SaveRefresh(DDOCV);
                            NomadEnvironment.QueryValueChange("SDP01_CV", "oi_cv", "c_cv", objRead.c_cv, "","1", true);
                        }
                        catch (Exception e)
                        {
                            objBatch.Err("Error al grabar registro " + objRead.c_tipo_documento + " - " + objRead.c_nro_doc + "' - Linea: " + Linea.ToString() + " - " + e.Message);
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

    }
}



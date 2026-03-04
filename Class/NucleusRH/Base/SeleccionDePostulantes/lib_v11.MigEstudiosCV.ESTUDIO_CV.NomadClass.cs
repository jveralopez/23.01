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

namespace NucleusRH.Base.SeleccionDePostulantes.MigEstudiosCV
{

    //////////////////////////////////////////////////////////////////////////////////
    //Clase MigEstudiosCV
    public partial class ESTUDIO_CV : Nomad.NSystem.Base.NomadObject
    {
        public static void ImportarEstudiosCV()
        {
            //Instancio el CV
            NucleusRH.Base.SeleccionDePostulantes.CVs.CV DDOCV = null;

            int Linea = 0, Errores = 0;
            string oiCVANT = "";

            NomadBatch objBatch;
            objBatch = NomadBatch.GetBatch("Iniciando...", "Importación de Estudios");

            NomadXML IDList = new NomadXML();
            NucleusRH.Base.SeleccionDePostulantes.MigEstudiosCV.ESTUDIO_CV objRead;
            //string PersonalOI, LegajoOI;

            IDList.SetText(NomadProxy.GetProxy().SQLService().Get(NucleusRH.Base.SeleccionDePostulantes.MigEstudiosCV.ESTUDIO_CV.Resources.QRY_REGISTROS, ""));

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
                    objRead = NucleusRH.Base.SeleccionDePostulantes.MigEstudiosCV.ESTUDIO_CV.Get(row.GetAttr("id"));

                    //Valido atributos obligatorios  	                                             
                    if (objRead.c_cv == "")
                    {
                        objBatch.Err("No se especificó el CV, se rechaza el registro '" + objRead.c_cv + " - " + objRead.d_carrera + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }

                    //Recupero los OI de los códigos ingresados
                    string oiNIVEST = "", oiUTPO = "", oiCV = "";

                    if (objRead.c_cv != "")
                    {
                        oiCV = NomadEnvironment.QueryValue("SDP01_CV", "oi_cv", "c_cv", objRead.c_cv, "", true);
                        if (oiCV == null)
                        {
                            objBatch.Err("El CV no existe, se rechaza el registro '" + objRead.c_cv + " - " + objRead.d_carrera + "' - Linea: " + Linea.ToString());
                            Errores++;
                            continue;
                        }
                    }

                    if (objRead.c_nivel_estudio != "")
                    {
                        oiNIVEST = NomadEnvironment.QueryValue("PER12_NIVELES_EST", "oi_nivel_estudio", "c_nivel_estudio", objRead.c_nivel_estudio, "", true);
                        if (oiNIVEST == null)
                        {
                            objBatch.Err("El Nivel de Estudio no existe, se rechaza el registro '" + objRead.c_cv + " - " + objRead.d_carrera + "' - Linea: " + Linea.ToString());
                            Errores++;
                            continue;
                        }
                    }

                    if (objRead.c_unidad_tiempo != "")
                    {
                        oiUTPO = NomadEnvironment.QueryValue("ORG25_UNIDADES_TPO", "oi_unidad_tiempo", "c_unidad_tiempo", objRead.c_unidad_tiempo, "", true);
                        if (oiUTPO == null)
                        {
                            objBatch.Err("La Unidad de Tiempo no existe, se rechaza el registro '" + objRead.c_cv + " - " + objRead.d_carrera + "' - Linea: " + Linea.ToString());
                            Errores++;
                            continue;
                        }
                    }

                    if (oiCVANT != oiCV)
                    {
                        if (DDOCV == null || DDOCV.Id != oiCV)
                        {
                            if (DDOCV != null)
                            {
                                //Grabo
                                try
                                {
                                    NomadEnvironment.GetCurrentTransaction().Save(DDOCV);
                                }
                                catch (Exception e)
                                {
                                    objBatch.Err("Error al grabar registro " + DDOCV.c_cv + "' - Linea: " + Linea.ToString() + " - " + e.Message);
                                    Errores++;
                                }
                            }
                        }
                        if (DDOCV != null) oiCVANT = DDOCV.Id; else oiCVANT = oiCV;
                        DDOCV = NucleusRH.Base.SeleccionDePostulantes.CVs.CV.Get(oiCV);
                    }

                    //Creo el Estudio
                    NucleusRH.Base.SeleccionDePostulantes.CVs.ESTUDIO_CV DDOESTCV;
                    DDOESTCV = new NucleusRH.Base.SeleccionDePostulantes.CVs.ESTUDIO_CV();

                    DDOESTCV.oi_nivel_estudio = oiNIVEST;
                    //DDOESTCV.oi_area_est = oiAREAEST;
                    DDOESTCV.oi_unidad_tiempo = oiUTPO;
                    DDOESTCV.d_nom_carrera = objRead.d_carrera;
                    DDOESTCV.e_dur_carrera = objRead.e_duracion;
                    DDOESTCV.d_otro_est_educ = objRead.d_otro_est_educ;
                    DDOESTCV.e_ano_fin = objRead.e_ano_fin;
                    DDOESTCV.c_estado = objRead.c_estado_estudio;
                    DDOESTCV.d_institucion = objRead.d_institucion;
                    DDOESTCV.d_localidad = objRead.d_localidad;
                    DDOESTCV.e_ano_curso = objRead.e_ano_curso;
                    DDOESTCV.e_ano_inicio = objRead.e_ano_inicio;
                    DDOESTCV.e_ult_ano_cursado = objRead.e_ult_ano_cursado;
                    if (objRead.f_actualizNull) DDOESTCV.f_actualizNull = true; else DDOESTCV.f_actualiz = objRead.f_actualiz;
                    DDOESTCV.o_estudio = objRead.o_estudio;

                    //Agrego el Estudio
                    DDOCV.ESTUDIOS_CV.Add(DDOESTCV);
                }
                catch (Exception e)
                {
                    objBatch.Err("Error desconocido en registro, por favor verifique el formato del archivo y la correcta asignación de los campos - Linea: " + Linea.ToString() + " - " + e.Message);
                    Errores++;
                }
            }

            if (DDOCV != null)
            {
                //Grabo
                try
                {
                    NomadEnvironment.GetCurrentTransaction().Save(DDOCV);
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



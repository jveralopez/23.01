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

namespace NucleusRH.Base.SeleccionDePostulantes.MigExperienciaCV
{

    //////////////////////////////////////////////////////////////////////////////////
    //Clase MigExperienciaCV
    public partial class EXPERIENCIA_CV : Nomad.NSystem.Base.NomadObject
    {
        public static void ImportarExperienciasCV()
        {
            //Instancio el CV
            NucleusRH.Base.SeleccionDePostulantes.CVs.CV DDOCV = null;

            int Linea = 0, Errores = 0;
            string oiCVANT = "";

            NomadBatch objBatch;
            objBatch = NomadBatch.GetBatch("Iniciando...", "Importación de Experiencias");

            NomadXML IDList = new NomadXML();
            NucleusRH.Base.SeleccionDePostulantes.MigExperienciaCV.EXPERIENCIA_CV objRead;
            //string PersonalOI, LegajoOI;

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
                    objRead = NucleusRH.Base.SeleccionDePostulantes.MigExperienciaCV.EXPERIENCIA_CV.Get(row.GetAttr("id"));

                    //Valido atributos obligatorios  	                                             
                    if (objRead.c_cv == "")
                    {
                        objBatch.Err("No se especificó el CV, se rechaza el registro '" + objRead.c_cv + " - " + objRead.d_empresa + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }
                    if (objRead.c_puesto_exp == "")
                    {
                        objBatch.Err("No se especificó el Puesto de Experiencia, se rechaza el registro '" + objRead.c_cv + " - " + objRead.d_empresa + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }

                    //Recupero los OI de los códigos ingresados
                    string oiPUEEST = "", oiCV = "";

                    if (objRead.c_cv != "")
                    {
                        oiCV = NomadEnvironment.QueryValue("SDP01_CV", "oi_cv", "c_cv", objRead.c_cv, "", true);
                        if (oiCV == null)
                        {
                            objBatch.Err("El CV no existe, se rechaza el registro '" + objRead.c_cv + " - " + objRead.d_empresa + "' - Linea: " + Linea.ToString());
                            Errores++;
                            continue;
                        }
                    }

                    if (objRead.c_puesto_exp != "")
                    {
                        oiPUEEST = NomadEnvironment.QueryValue("SDP09_PUESTO_EXP", "oi_puesto_exp", "c_puesto_exp", objRead.c_puesto_exp, "", true);
                        if (oiPUEEST == null)
                        {
                            objBatch.Err("El Puesto de Experiencia no existe, se rechaza el registro '" + objRead.c_cv + " - " + objRead.d_empresa + "' - Linea: " + Linea.ToString());
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

                    //Me fijo si ya existe un registro de puesto en la fecha especificada para la persona  	
                    NomadXML existe;
                    NomadXML param = new NomadXML("DATOS");
                    param.SetAttr("oi_cv", oiCV);
                    param.SetAttr("oi_puesto_exp", oiPUEEST);
                    param.SetAttr("f_ingreso", objRead.f_ingreso);

                    existe = NomadEnvironment.QueryNomadXML(EXPERIENCIA_CV.Resources.QRY_EXISTE, param.ToString());

                    if (existe.FirstChild().GetAttr("existe") == "1")
                    {
                        objBatch.Err("Ya existe un registro para la Experiencia en el CV, se rechaza el registro '" + objRead.c_cv + " - " + objRead.d_empresa + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }


                    //Creo la Experiencia
                    NucleusRH.Base.SeleccionDePostulantes.CVs.EXPERIENCIA DDOEXPCV;
                    DDOEXPCV = new NucleusRH.Base.SeleccionDePostulantes.CVs.EXPERIENCIA();

                    DDOEXPCV.oi_puesto_exp = oiPUEEST;
                    DDOEXPCV.d_empresa = objRead.d_empresa;
                    DDOEXPCV.d_actividad = objRead.d_actividad;
                    DDOEXPCV.d_localidad = objRead.d_localidad;
                    DDOEXPCV.l_actual = objRead.l_actual;
                    DDOEXPCV.f_ingreso = objRead.f_ingreso;
                    if (objRead.f_egresoNull) DDOEXPCV.f_egresoNull = true; else DDOEXPCV.f_egreso = objRead.f_egreso;
                    DDOEXPCV.e_experiencia = objRead.e_experiencia;
                    DDOEXPCV.n_ultimo_sueldo = objRead.n_ultimo_sueldo;
                    DDOEXPCV.l_per_cargo = objRead.l_per_cargo;
                    DDOEXPCV.o_tareas = objRead.o_tareas;
                    DDOEXPCV.o_contacto = objRead.o_contacto;

                    //Agrego la Experiencia
                    DDOCV.EXPERIENCIA.Add(DDOEXPCV);
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



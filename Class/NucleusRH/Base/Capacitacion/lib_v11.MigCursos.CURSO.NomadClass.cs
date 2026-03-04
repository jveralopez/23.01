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

namespace NucleusRH.Base.Capacitacion.MigCursos
{
    public partial class CURSO : Nomad.NSystem.Base.NomadObject
    {
        public static void ImportarCursos()
        {
            int Linea = 0, Errores = 0;

            NomadBatch objBatch;
            objBatch = NomadBatch.GetBatch("Iniciando...", "Importación de Cursos");

            NomadXML IDList = new NomadXML();
            NucleusRH.Base.Capacitacion.MigCursos.CURSO objRead;
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
                    objRead = NucleusRH.Base.Capacitacion.MigCursos.CURSO.Get(row.GetAttr("id"));

                    //Valido atributos obligatorios  	                                             
                    if (objRead.c_curso == "")
                    {
                        objBatch.Err("No se especificó el Código del Curso, se rechaza el registro '" + objRead.c_curso + " - " + objRead.d_curso + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }
                    if (objRead.d_curso == "")
                    {
                        objBatch.Err("No se especificó la Descripción del Curso, se rechaza el registro '" + objRead.c_curso + " - " + objRead.d_curso + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }
                    if (objRead.c_area == "")
                    {
                        objBatch.Err("No se especificó el Área del Curso, se rechaza el registro '" + objRead.c_curso + " - " + objRead.d_curso + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }


                    //Recupero los OI de los códigos ingresados
                    string oiTIPCCUR = "", oiCATCUR = "", oiAREACUR = "";

                    if (objRead.c_tipo_curso != "")
                    {
                        oiTIPCCUR = NomadEnvironment.QueryValue("CYD05_TIPOS_CURSO", "oi_tipo_curso", "c_tipo_curso", objRead.c_tipo_curso, "", true);
                        if (oiTIPCCUR == null)
                        {
                            objBatch.Err("El Tipo de Curso no existe, se rechaza el registro '" + objRead.c_curso + " - " + objRead.d_curso + "' - Linea: " + Linea.ToString());
                            Errores++;
                            continue;
                        }
                    }

                    if (objRead.c_categ_curso != "")
                    {
                        oiCATCUR = NomadEnvironment.QueryValue("CYD06_CATEG_CURSO", "oi_categ_curso", "c_categ_curso", objRead.c_categ_curso, "", true);
                        if (oiCATCUR == null)
                        {
                            objBatch.Err("La Categoría del Curso no existe, se rechaza el registro '" + objRead.c_curso + " - " + objRead.d_curso + "' - Linea: " + Linea.ToString());
                            Errores++;
                            continue;
                        }
                    }

                    if (objRead.c_area != "")
                    {
                        oiAREACUR = NomadEnvironment.QueryValue("ORG06_CURSOS", "oi_curso", "c_curso", objRead.c_area, "", true);
                        if (oiAREACUR == null)
                        {
                            objBatch.Err("El Área del Curso no existe, se rechaza el registro '" + objRead.c_curso + " - " + objRead.d_curso + "' - Linea: " + Linea.ToString());
                            Errores++;
                            continue;
                        }
                    }

                    //Me fijo si ya existe el Curso  	
                    string oiVal = NomadEnvironment.QueryValue("CYD01_CURSOS", "oi_curso", "c_curso", objRead.c_curso, "", true);
                    if (oiVal != null)
                    {
                        objBatch.Err("Ya existe un registro para el Curso, se rechaza el registro '" + objRead.c_curso + " - " + objRead.d_curso + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }
                    else
                    {
                        //Creo el CURSO
                        NucleusRH.Base.Capacitacion.Cursos.CURSO DDOCUR;
                        DDOCUR = new NucleusRH.Base.Capacitacion.Cursos.CURSO();

                        DDOCUR.c_curso = objRead.c_curso;
                        DDOCUR.d_curso = objRead.d_curso;
                        DDOCUR.n_duracion_hs = objRead.n_duracion_hs;
                        DDOCUR.o_curso = objRead.o_curso;
                        DDOCUR.o_destinatarios = objRead.o_destinatarios;
                        DDOCUR.o_objetivos = objRead.o_objetivos;
                        DDOCUR.o_requisitos = objRead.o_requisitos;
                        DDOCUR.o_temas = objRead.o_temas;
                        DDOCUR.oi_area = oiAREACUR;
                        DDOCUR.oi_categ_curso = oiCATCUR;
                        DDOCUR.oi_tipo_curso = oiTIPCCUR;

                        //Grabo
                        try
                        {
                            NomadEnvironment.GetCurrentTransaction().SaveRefresh(DDOCUR);
                            NomadEnvironment.QueryValueChange("CYD01_CURSOS", "oi_curso", "c_curso", objRead.c_curso, "","1", true);

                        }
                        catch (Exception e)
                        {
                            objBatch.Err("Error al grabar registro " + objRead.c_curso + " - " + objRead.d_curso + "' - Linea: " + Linea.ToString() + " - " + e.Message);
                            Errores++;
                        }
                    }

                }
                catch (Exception e)
                {
                    objBatch.Err("Error desconocido en registro, por favor verifique el formato del archivo y la correcta asignaci&#243;n de los campos - Linea: " + Linea.ToString() + " - " + e.Message);
                    Errores++;
                }
            }

            objBatch.Log("Registros Procesados: " + Linea.ToString() + " - Importados: " + (Linea - Errores).ToString());
            objBatch.Log("Finalizado...");
        }
    }
}

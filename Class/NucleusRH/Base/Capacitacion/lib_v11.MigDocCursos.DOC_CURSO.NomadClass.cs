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

namespace NucleusRH.Base.Capacitacion.MigDocCursos
{
    public partial class DOC_CURSO : Nomad.NSystem.Base.NomadObject
    {
        public static void ImportarDocCursos()
        {
            int Linea = 0, Errores = 0, Importados = 0;

            Hashtable DDOCURHASH = new Hashtable();

            NomadBatch objBatch;
            objBatch = NomadBatch.GetBatch("Iniciando...", "Importación de Docentes");

            //NomadXML IDCur;
            NomadXML IDList = new NomadXML();
            NucleusRH.Base.Capacitacion.MigDocCursos.DOC_CURSO objRead;
            IDList.SetText(NomadProxy.GetProxy().SQLService().Get(Resources.QRY_REGISTROS, ""));
            ArrayList lista = (ArrayList)IDList.FirstChild().GetElements("ROW");

            //RECORRO LOS IDS QUE ENTRAN Y PREGUNTO SI ALGUN ID NO ESTA EN LA HASH, PORQ DE SER ASI HAY QUE AGREGARLO
            for (int xml = 0; xml < lista.Count; xml++)
            {
                Linea++;
                NomadXML row = (NomadXML)lista[xml];
                objBatch.SetPro(0, 100, lista.Count, xml);
                objBatch.SetMess("Incorporando registro " + (xml + 1) + " de " + lista.Count);

                //Instancio el CURSO
                NucleusRH.Base.Capacitacion.Cursos.CURSO DDOCUR = null;

                //Inicio la Transaccion
                try
                {
                    objRead = NucleusRH.Base.Capacitacion.MigDocCursos.DOC_CURSO.Get(row.GetAttr("id"));

                    //Valido atributos obligatorios
                    if (objRead.c_curso == "")
                    {
                        objBatch.Err("No se especificó el Curso, se rechaza el registro '" + objRead.c_curso + " - " + objRead.c_docente + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }
                    if (objRead.c_docente == "")
                    {
                        objBatch.Err("No se especificó el Docente del Curso, se rechaza el registro '" + objRead.c_curso + " - " + objRead.c_docente + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }

                    //Recupero los OI de los códigos ingresados
                    string oiDOC = "", oiCUR = "";

                    if (objRead.c_curso != "")
                    {
                        oiCUR = NomadEnvironment.QueryValue("CYD01_CURSOS", "oi_curso", "c_curso", objRead.c_curso, "", true);
                        if (oiCUR == null)
                        {
                            objBatch.Err("El Curso no existe, se rechaza el registro '" + objRead.c_curso + " - " + objRead.c_docente + "' - Linea: " + Linea.ToString());
                            Errores++;
                            continue;
                        }
                    }

                    if (objRead.c_docente != "")
                    {
                        oiDOC = NomadEnvironment.QueryValue("CYD04_DOCENTES", "oi_docente", "c_docente", objRead.c_docente, "", true);
                        if (oiDOC == null)
                        {
                            objBatch.Err("El Docente no existe, se rechaza el registro '" + objRead.c_curso + " - " + objRead.c_docente + "' - Linea: " + Linea.ToString());
                            Errores++;
                            continue;
                        }
                    }

                    if (oiCUR != null && oiDOC != null)
                    {
                        if (!DDOCURHASH.ContainsKey(oiCUR)) //si no contiene al curso - recupero de BD
                        {
                            DDOCUR = NucleusRH.Base.Capacitacion.Cursos.CURSO.Get(oiCUR);
                        }
                        else //si ya contiene al curso - recupero de hashtable
                        {
                            DDOCUR = (NucleusRH.Base.Capacitacion.Cursos.CURSO)DDOCURHASH[oiCUR];
                        }

                        //Me fijo que no exista el Docente en el Curso 	  	
                        if (DDOCUR.DOC_CURSO.GetByAttribute("oi_docente", oiDOC) == null)
                        {
                            //Creo el Docente
                            NucleusRH.Base.Capacitacion.Cursos.DOCENTE_CURSO DDODOCCUR;
                            DDODOCCUR = new NucleusRH.Base.Capacitacion.Cursos.DOCENTE_CURSO();

                            DDODOCCUR.oi_docente = oiDOC;

                            //Agrego el Docente
                            DDOCUR.DOC_CURSO.Add(DDODOCCUR);

                            //Guardo la instancia del curso, con su coleccion de docentes en la hashtable
                            DDOCURHASH[oiCUR] = DDOCUR;
                        }

                        else
                        {
                            objBatch.Err("Ya existe un registro para el Docente en el Curso, se rechaza el registro '" + objRead.c_curso + " - " + objRead.c_docente + "' - Linea: " + Linea.ToString());
                            Errores++;
                            continue;
                        }
                    }
                }

                catch (Exception e)
                {
                    objBatch.Err("Error desconocido en registro, por favor verifique el formato del archivo y la correcta asignación de los campos - Linea: " + Linea.ToString() + " - " + e.Message);
                    Errores++;
                }
            }

            //Guardo el curso con sus docentes
            foreach (NucleusRH.Base.Capacitacion.Cursos.CURSO CUR in DDOCURHASH.Values)
            {
                //Grabo
                try
                {
                    NomadEnvironment.GetCurrentTransaction().Save(CUR);
                    Importados++;
                }
                catch (Exception e)
                {
                    objBatch.Err("Error al grabar los registros del curso: " + CUR.c_curso + " - " + e.Message);
                    Errores++;
                }
            }

            objBatch.Log("Registros Procesados: " + Linea.ToString() + " - Se importaron los docentes de: " + Importados.ToString() + " Cursos - Errores: " + Errores.ToString());
            objBatch.Log("Finalizado...");
        }
    }
}



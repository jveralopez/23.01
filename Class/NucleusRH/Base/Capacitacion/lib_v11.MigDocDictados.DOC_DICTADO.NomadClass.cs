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

namespace NucleusRH.Base.Capacitacion.MigDocDictados
{
    public partial class DOC_DICTADO : Nomad.NSystem.Base.NomadObject
    {
        public static void ImportarDocDictados()
        {
            int Linea = 0, Errores = 0, Importados = 0;

            NomadBatch objBatch;
            objBatch = NomadBatch.GetBatch("Iniciando...", "Importación de Docentes de Dictados");

            NomadXML IDList = new NomadXML();
            NucleusRH.Base.Capacitacion.MigDocDictados.DOC_DICTADO objRead;
            IDList.SetText(NomadProxy.GetProxy().SQLService().Get(Resources.QRY_REGISTROS, ""));
            ArrayList lista = (ArrayList)IDList.FirstChild().GetElements("ROW");

            Hashtable DICTADOHASH = new Hashtable();

            //RECORRO LOS IDS QUE ENTRAN Y PREGUNTO SI ALGUN ID NO ESTA EN LA HASH, PORQ DE SER ASI HAY QUE AGREGARLO
            for (int xml = 0; xml < lista.Count; xml++)
            {
                Linea++;
                NomadXML row = (NomadXML)lista[xml];
                objBatch.SetPro(0, 100, lista.Count, xml);
                objBatch.SetMess("Incorporando registro " + (xml + 1) + " de " + lista.Count);

                //Instancio el DICTADO
                NucleusRH.Base.Capacitacion.Dictados.DICTADO DDODIC = null;

                //Inicio la Transaccion
                try
                {
                    objRead = NucleusRH.Base.Capacitacion.MigDocDictados.DOC_DICTADO.Get(row.GetAttr("id"));

                    //Valido atributos obligatorios  	                                             
                    if (objRead.c_curso == "")
                    {
                        objBatch.Err("No se especificó el Curso, se rechaza el registro '" + objRead.c_curso + " - " + objRead.c_dictado + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }
                    if (objRead.c_dictado == "")
                    {
                        objBatch.Err("No se especificó el código del Dictado, se rechaza el registro '" + objRead.c_curso + " - " + objRead.c_dictado + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }
                    if (objRead.c_docente == "")
                    {
                        objBatch.Err("No se especificó Docente del Dictado, se rechaza el registro '" + objRead.c_curso + " - " + objRead.c_dictado + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }

                    //Recupero los OI de los códigos ingresados
                    string oiDIC = "", oiDOC = "", oiCUR = "";

                    if (objRead.c_curso != "")
                    {
                        oiCUR = NomadEnvironment.QueryValue("CYD01_CURSOS", "oi_curso", "c_curso", objRead.c_curso, "", true);
                        if (oiCUR == null)
                        {
                            objBatch.Err("El Curso no existe, se rechaza el registro '" + objRead.c_curso + " - " + objRead.c_dictado + "' - Linea: " + Linea.ToString());
                            Errores++;
                            continue;
                        }
                    }

                    if (objRead.c_dictado != "")
                    {
                        oiDIC = NomadEnvironment.QueryValue("CYD02_DICTADOS", "oi_dictado", "c_dictado", objRead.c_dictado, "CYD02_DICTADOS.oi_curso = " + oiCUR, true);
                        if (oiDIC == null)
                        {
                            objBatch.Err("El Dictado no existe en el Curso, se rechaza el registro '" + objRead.c_curso + " - " + objRead.c_dictado + "' - Linea: " + Linea.ToString());
                            Errores++;
                            continue;
                        }
                    }

                    if (objRead.c_docente != "")
                    {
                        oiDOC = NomadEnvironment.QueryValue("CYD04_DOCENTES", "oi_docente", "c_docente", objRead.c_docente, "", true);
                        if (oiDOC == null)
                        {
                            
                            Errores++;
                            continue;
                        }
                    }

                    if (oiDIC != null && oiDOC != null)
                    {
                        if (!DICTADOHASH.ContainsKey(oiDIC)) //si no contiene - recupero de BD
                        {
                            DDODIC = NucleusRH.Base.Capacitacion.Dictados.DICTADO.Get(oiDIC);
                        }
                        else //Si contiene, recupero de la hashtable
                        {
                            DDODIC = (NucleusRH.Base.Capacitacion.Dictados.DICTADO)DICTADOHASH[oiDIC]; 
                        }

                        //Valido que el docente no exista en el dictado
                        if (DDODIC.DOC_DICTADO.GetByAttribute("oi_docente", oiDOC) == null)
                        {
                            //Creo el Docente
                            NucleusRH.Base.Capacitacion.Dictados.DOCENTE_CURSO DDODOCDIC;
                            DDODOCDIC = new NucleusRH.Base.Capacitacion.Dictados.DOCENTE_CURSO();

                            DDODOCDIC.oi_docente = oiDOC;

                            //Agrego el Docente
                            DDODIC.DOC_DICTADO.Add(DDODOCDIC);

                            //Guardo el dictado con sus docentes en la hashtable
                            DICTADOHASH[oiDIC] = DDODIC;
                        }
                        else
                        {
                            objBatch.Err("Ya existe un registro para el Docente en el Dictado, se rechaza el registro '" + objRead.c_curso + " - " + objRead.c_dictado + "' - Linea: " + Linea.ToString());
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

            //Guardo el dictado con sus docentes
            foreach (NucleusRH.Base.Capacitacion.Dictados.DICTADO DIC in DICTADOHASH.Values)
            {
                //Grabo
                try
                {
                    NomadEnvironment.GetCurrentTransaction().Save(DIC);
                    Importados++;
                }
                catch (Exception e)
                {
                    objBatch.Err("Error al grabar los registros del dictado: " + DIC.c_dictado + " - " + e.Message);
                    Errores++;
                }
            }         

            objBatch.Log("Registros Procesados: " + Linea.ToString() + " - Se importaron los docentes de: " + Importados.ToString() + " Dictados - Errores: " + Errores.ToString());
            objBatch.Log("Finalizado...");
        }
    }
}

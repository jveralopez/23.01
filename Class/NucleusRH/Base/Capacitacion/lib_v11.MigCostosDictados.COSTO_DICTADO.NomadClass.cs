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

namespace NucleusRH.Base.Capacitacion.MigCostosDictados
{
    public partial class COSTO_DICTADO : Nomad.NSystem.Base.NomadObject
    {
        public static void ImportarCostosDictados()
        {
            //Instancio el DICTADO
            NucleusRH.Base.Capacitacion.Dictados.DICTADO DDODIC = null;

            int Linea = 0, Errores = 0;
            string oiDICANT = "";

            NomadBatch objBatch;
            objBatch = NomadBatch.GetBatch("Iniciando...", "Importación de Costos de Dictados");

            NomadXML IDList = new NomadXML();
            NucleusRH.Base.Capacitacion.MigCostosDictados.COSTO_DICTADO objRead;
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
                    objRead = NucleusRH.Base.Capacitacion.MigCostosDictados.COSTO_DICTADO.Get(row.GetAttr("id"));

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
                    if (objRead.c_item_costo == "")
                    {
                        objBatch.Err("No se especificó Item de Costo del Dictado, se rechaza el registro '" + objRead.c_curso + " - " + objRead.c_dictado + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }

                    //Recupero los OI de los códigos ingresados
                    string oiDIC = "", oiITEM = "", oiCUR = "";

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

                    if (objRead.c_item_costo != "")
                    {
                        oiITEM = NomadEnvironment.QueryValue("CYD03_ITEMS_COSTO", "oi_item_costo", "c_item_costo", objRead.c_item_costo, "", true);
                        if (oiITEM == null)
                        {
                            objBatch.Err("El Item de Costo no existe, se rechaza el registro '" + objRead.c_curso + " - " + objRead.c_dictado + "' - Linea: " + Linea.ToString());
                            Errores++;
                            continue;
                        }
                    }

                    if (oiDICANT != oiDIC)
                    {
                        if (DDODIC == null || DDODIC.Id != oiDIC)
                        {
                            if (DDODIC != null)
                            {
                                //Grabo
                                try
                                {
                                    NomadEnvironment.GetCurrentTransaction().Save(DDODIC);
                                }
                                catch (Exception e)
                                {
                                    objBatch.Err("Error al grabar registro " + DDODIC.c_dictado + "' - Linea: " + Linea.ToString() + " - " + e.Message);
                                    Errores++;
                                }
                            }
                        }
                        if (DDODIC != null) oiDICANT = DDODIC.Id; else oiDICANT = oiDIC;
                        DDODIC = NucleusRH.Base.Capacitacion.Dictados.DICTADO.Get(oiDIC);
                    }

                    //Me fijo si ya existe el Item de Costo en el DICTADO 	  	
                    if (DDODIC.CTOS_DICTADO.GetByAttribute("oi_item_costo", oiITEM) != null)
                    {
                        objBatch.Err("Ya existe un registro para el Item de Costo en el Dictado, se rechaza el registro '" + objRead.c_curso + " - " + objRead.c_dictado + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }

                    //Creo el Item de Costo
                    NucleusRH.Base.Capacitacion.Dictados.COSTO_DICTADO DDOCOSDIC;
                    DDOCOSDIC = new NucleusRH.Base.Capacitacion.Dictados.COSTO_DICTADO();

                    DDOCOSDIC.oi_item_costo = oiITEM;
                    DDOCOSDIC.oi_dictado = int.Parse(oiDIC);
                    DDOCOSDIC.n_cantidad = objRead.n_cantidad;
                    DDOCOSDIC.n_costo_total = objRead.n_costo_total;
                    DDOCOSDIC.n_costo_uni = objRead.n_costo_uni;

                    //Agrego el tem de Costo
                    DDODIC.CTOS_DICTADO.Add(DDOCOSDIC);
                }
                catch (Exception e)
                {
                    objBatch.Err("Error desconocido en registro, por favor verifique el formato del archivo y la correcta asignación de los campos - Linea: " + Linea.ToString() + " - " + e.Message);
                    Errores++;
                }
            }

            if (DDODIC != null)
            {
                //Grabo
                try
                {
                    NomadEnvironment.GetCurrentTransaction().Save(DDODIC);
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

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

namespace NucleusRH.Base.Capacitacion.MigCostosCursos
{
    public partial class COSTO_CURSO : Nomad.NSystem.Base.NomadObject
    {
        public static void ImportarCostosCursos()
        {
            //Instancio el CURSO
            NucleusRH.Base.Capacitacion.Cursos.CURSO DDOCUR = null;

            int Linea = 0, Errores = 0;
            string oiCURANT = "";

            NomadBatch objBatch;
            objBatch = NomadBatch.GetBatch("Iniciando...", "Importación de Costos de Cursos");

            NomadXML IDList = new NomadXML();
            NucleusRH.Base.Capacitacion.MigCostosCursos.COSTO_CURSO objRead;

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
                    objRead = NucleusRH.Base.Capacitacion.MigCostosCursos.COSTO_CURSO.Get(row.GetAttr("id"));

                    //Valido atributos obligatorios  	                                             
                    if (objRead.c_curso == "")
                    {
                        objBatch.Err("No se especificó el Curso, se rechaza el registro '" + objRead.c_curso + " - " + objRead.c_item_costo + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }
                    if (objRead.c_item_costo == "")
                    {
                        objBatch.Err("No se especificó el Item de Costo, se rechaza el registro '" + objRead.c_curso + " - " + objRead.c_item_costo + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }

                    //Recupero los OI de los códigos ingresados
                    string oiITEM = "", oiCUR = "";

                    if (objRead.c_curso != "")
                    {
                        oiCUR = NomadEnvironment.QueryValue("CYD01_CURSOS", "oi_curso", "c_curso", objRead.c_curso, "", true);
                        if (oiCUR == null)
                        {
                            objBatch.Err("El Curso no existe, se rechaza el registro '" + objRead.c_curso + " - " + objRead.c_item_costo + "' - Linea: " + Linea.ToString());
                            Errores++;
                            continue;
                        }
                    }

                    if (objRead.c_item_costo != "")
                    {
                        oiITEM = NomadEnvironment.QueryValue("CYD03_ITEMS_COSTO", "oi_item_costo", "c_item_costo", objRead.c_item_costo, "", true);
                        if (oiITEM == null)
                        {
                            objBatch.Err("El Item de Costo no existe, se rechaza el registro '" + objRead.c_curso + " - " + objRead.c_item_costo + "' - Linea: " + Linea.ToString());
                            Errores++;
                            continue;
                        }
                    }


                    if (oiCURANT != oiCUR)
                    {
                        if (DDOCUR == null || DDOCUR.Id != oiCUR)
                        {
                            if (DDOCUR != null)
                            {
                                //Grabo
                                try
                                {
                                    NomadEnvironment.GetCurrentTransaction().Save(DDOCUR);
                                }
                                catch (Exception e)
                                {
                                    objBatch.Err("Error al grabar registro " + DDOCUR.c_curso + "' - Linea: " + Linea.ToString() + " - " + e.Message);
                                    Errores++;
                                }
                            }
                        }
                        if (DDOCUR != null) oiCURANT = DDOCUR.Id; else oiCURANT = oiCUR;
                        DDOCUR = NucleusRH.Base.Capacitacion.Cursos.CURSO.Get(oiCUR);
                    }

                    //Me fijo si ya existe el Item de Costo en el CURSO 	  	
                    if (DDOCUR.COSTOS_CURSO.GetByAttribute("oi_item_costo", oiITEM) != null)
                    {
                        objBatch.Err("Ya existe un registro para el Item de Costo en el Curso, se rechaza el registro '" + objRead.c_curso + " - " + objRead.c_item_costo + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }

                    //Creo el Item de Costo
                    NucleusRH.Base.Capacitacion.Cursos.COSTO_CURSO DDOCOSCUR;
                    DDOCOSCUR = new NucleusRH.Base.Capacitacion.Cursos.COSTO_CURSO();

                    DDOCOSCUR.oi_item_costo = oiITEM;
                    DDOCOSCUR.n_cantidad = objRead.n_cantidad;
                    DDOCOSCUR.n_costo_total = objRead.n_costo_uni;
                    DDOCOSCUR.n_costo_uni = objRead.n_costo_total;
                    //Agrego el Item de Costo
                    DDOCUR.COSTOS_CURSO.Add(DDOCOSCUR);

                }
                catch (Exception e)
                {
                    objBatch.Err("Error desconocido en registro, por favor verifique el formato del archivo y la correcta asignación de los campos - Linea: " + Linea.ToString() + " - " + e.Message);
                    Errores++;
                }
            }

            if (DDOCUR != null)
            {
                //Grabo
                try
                {
                    NomadEnvironment.GetCurrentTransaction().Save(DDOCUR);
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

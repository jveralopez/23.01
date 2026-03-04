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

namespace NucleusRH.Base.SeleccionDePostulantes.MigInformaticaCV
{

    //////////////////////////////////////////////////////////////////////////////////
    //Clase MigInformaticaCV
    public partial class INFORMATICA_CV : Nomad.NSystem.Base.NomadObject
    {
        public static void ImportarInformaticaCV()
        {
            //Instancio el CV
            NucleusRH.Base.SeleccionDePostulantes.CVs.CV DDOCV = null;

            int Linea = 0, Errores = 0;
            string oiCVANT = "";

            NomadBatch objBatch;
            objBatch = NomadBatch.GetBatch("Iniciando...", "Importación de Conocimientos Informáticos");

            NomadXML IDList = new NomadXML();
            NucleusRH.Base.SeleccionDePostulantes.MigInformaticaCV.INFORMATICA_CV objRead;
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
                    objRead = NucleusRH.Base.SeleccionDePostulantes.MigInformaticaCV.INFORMATICA_CV.Get(row.GetAttr("id"));

                    //Valido atributos obligatorios  	                                             
                    if (objRead.c_cv == "")
                    {
                        objBatch.Err("No se especificó el CV, se rechaza el registro '" + objRead.c_cv + " - " + objRead.c_informatica + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }
                    if (objRead.c_informatica == "")
                    {
                        objBatch.Err("No se especificó el Conocimiento Informático del CV, se rechaza el registro '" + objRead.c_cv + " - " + objRead.c_informatica + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }

                    //Recupero los OI de los códigos ingresados
                    string oiCON = "", oiCV = "";

                    if (objRead.c_cv != "")
                    {
                        oiCV = NomadEnvironment.QueryValue("SDP01_CV", "oi_cv", "c_cv", objRead.c_cv, "", true);
                        if (oiCV == null)
                        {
                            objBatch.Err("El CV no existe, se rechaza el registro '" + objRead.c_cv + " - " + objRead.c_informatica + "' - Linea: " + Linea.ToString());
                            Errores++;
                            continue;
                        }
                    }

                    if (objRead.c_informatica != "")
                    {
                        oiCON = NomadEnvironment.QueryValue("SDP08_INFORMATICA", "oi_informatica", "c_informatica", objRead.c_informatica, "", true);
                        if (oiCON == null)
                        {
                            objBatch.Err("El Conocimiento Informático no existe, se rechaza el registro '" + objRead.c_cv + " - " + objRead.c_informatica + "' - Linea: " + Linea.ToString());
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

                    //Me fijo si ya existe el Conocimiento en el CV 	  	
                    if (DDOCV.INFORMATICA.GetByAttribute("oi_informatica", oiCON) != null)
                    {
                        objBatch.Err("Ya existe un registro para el Conocimiento en el CV, se rechaza el registro '" + objRead.c_cv + " - " + objRead.c_informatica + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }

                    //Creo el Conocimiento Informático
                    NucleusRH.Base.SeleccionDePostulantes.CVs.INFORMATICA_CV DDOCONCV;
                    DDOCONCV = new NucleusRH.Base.SeleccionDePostulantes.CVs.INFORMATICA_CV();

                    DDOCONCV.oi_informatica = oiCON;
                    DDOCONCV.c_nivel = objRead.c_nivel;
                    DDOCONCV.o_detalle = objRead.o_detalle;

                    //Agrego el Conocimiento Informático
                    DDOCV.INFORMATICA.Add(DDOCONCV);
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



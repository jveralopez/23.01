using System;
using System.Xml;
using System.IO;
using System.Collections;

using Nomad.NSystem.Document;
using Nomad.NSystem.Proxy;
using Nomad.NSystem.Base;
using Nomad.NSystem.Functions;

namespace NucleusRH.Base.Accidentabilidad.MigTestigos
{

    //////////////////////////////////////////////////////////////////////////////////
    //Clase Testigos
    public partial class TESTIGO     
    {
        public static void ImportarTestigos()
        {

            //Instancio el TESTIGO
            int Linea = 0, Errores = 0;
            string oiPERANT = "";

            NomadBatch objBatch;
            objBatch = NomadBatch.GetBatch("Iniciando...", "Importación de Testigos");

            NomadXML IDCur;
            NomadXML IDList = new NomadXML();
            NucleusRH.Base.Accidentabilidad.MigTestigos.TESTIGO objRead;

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
                    objRead = NucleusRH.Base.Accidentabilidad.MigTestigos.TESTIGO.Get(row.GetAttr("id"));

                    //Valido atributos obligatorios  	                                             
                    if (objRead.d_apellido == "" || objRead.d_nombres == "")
                    {
                        objBatch.Err("No se especificó el nombre o apellido, se rechaza el registro '" + objRead.d_apellido + " - " + objRead.d_nombres + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }

                    if (objRead.e_testigoNull)
                    {
                        objBatch.Err("No se especificó el número del testigo, se rechaza el registro '" + objRead.d_apellido + " - " + objRead.d_nombres + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }

                    //Recupero los OI de los códigos ingresados
                    string oiACC = "", oiTIPO = "", oiTES = "", oiPER = "", oiPEREMP = "";

                    //Recupero el accidente
                    oiACC = NomadEnvironment.QueryValue("ACC01_ACCIDENTES", "oi_accidente", "oi_accidente", objRead.c_accidente, "", true);
                    if (oiACC == null)
                    {
                        objBatch.Err("El accidente no existe, se rechaza el registro '" + objRead.d_apellido + " - " + objRead.d_nombres + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }

                    //Recupero el tipo de documento
                    oiTIPO = NomadEnvironment.QueryValue("PER20_TIPOS_DOC", "oi_tipo_documento", "c_tipo_documento", objRead.c_tipo_documento.ToString(), "", true);
                    if (oiTIPO == null)
                    {
                        objBatch.Err("El tipo de documento no existe, se rechaza el registro '" + objRead.d_apellido + " - " + objRead.d_nombres + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }

                    //Me fijo si ya existe un Testigo para el accidente recuperado
                    oiTES = NomadEnvironment.QueryValue("ACC01_TESTIGOS", "oi_testigo", "e_testigo", objRead.e_testigo.ToString(), "ACC01_TESTIGOS.oi_accidente = " + oiACC, true);
                    if (oiTES != null)
                    {
                        objBatch.Err("El testigo ya existe para el accidente cargado, se rechaza el registro '" + objRead.d_apellido + " - " + objRead.d_nombres + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }
                    else
                    {
                        //Creo el testigo
                        NucleusRH.Base.Accidentabilidad.LegajoAccidentes.TESTIGO DDOTES;
                        DDOTES = new NucleusRH.Base.Accidentabilidad.LegajoAccidentes.TESTIGO();
                        DDOTES.oi_accidente = int.Parse(oiACC);
                        DDOTES.oi_tipo_documento = oiTIPO;
                        DDOTES.c_nro_documento = objRead.c_nro_documento;
                        DDOTES.d_apellido = objRead.d_apellido;
                        DDOTES.d_nombres = objRead.d_nombres;
                        DDOTES.d_domicilio = objRead.d_domicilio;
                        DDOTES.e_testigo = objRead.e_testigo;
                        DDOTES.o_testigo = objRead.o_testigo;

                        //Grabo
                        try
                        {
                            NomadEnvironment.GetCurrentTransaction().Save(DDOTES);
                            NomadEnvironment.QueryValueChange("ACC01_TESTIGOS", "oi_testigo", "e_testigo", objRead.e_testigo.ToString(), "ACC01_TESTIGOS.oi_accidente = " + oiACC, "1", true);
                        }
                        catch (Exception e)
                        {
                            objBatch.Err("Error al grabar registro " + objRead.d_apellido + " - " + objRead.d_nombres + "' - Linea: " + Linea.ToString() + " - " + e.Message);
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

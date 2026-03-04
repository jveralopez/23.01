using System;
using System.Xml;
using System.IO;
using System.Collections;

using Nomad.NSystem.Document;
using Nomad.NSystem.Proxy;
using Nomad.NSystem.Base;
using Nomad.NSystem.Functions;

namespace NucleusRH.Base.Accidentabilidad.MigFacCon
{

    //////////////////////////////////////////////////////////////////////////////////
    //Clase Factores de Contagios
    public partial class FACTOR_CONTAGIO 
    {
        public static void ImportarFacCon()
        {

            int Linea = 0, Errores = 0;

            NomadBatch objBatch;
            objBatch = NomadBatch.GetBatch("Iniciando...", "Importación de Factores de Contagio");

            NomadXML IDCur;
            NomadXML IDList = new NomadXML();
            NucleusRH.Base.Accidentabilidad.MigFacCon.FACTOR_CONTAGIO objRead;
            DateTime fCompare = new DateTime(1900, 1, 1);
            string PersonalOI, LegajoOI;

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
                    objRead = NucleusRH.Base.Accidentabilidad.MigFacCon.FACTOR_CONTAGIO.Get(row.GetAttr("id"));

                    //Valido atributos obligatorios
                    if (objRead.c_factor_cont == "" || objRead.d_factor_cont == "")
                    {
                        objBatch.Err("No se especificó el Factor de contagio, se rechaza el registro '" + objRead.c_factor_cont + " - " + objRead.d_factor_cont + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }

                    //Me fijo si ya existe el factor de contagio
                    string oiVal = NomadEnvironment.QueryValue("ACC05_FACTOR_CONT", "oi_factor_cont", "c_factor_cont", objRead.c_factor_cont, "", true);
                    if (oiVal != null)
                    {
                        objBatch.Err("Ya existe un registro para el Factor de contagio '" + objRead.c_factor_cont + " - " + objRead.d_factor_cont + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }
                    else
                    {
                        //Creo el de contagio
                        NucleusRH.Base.Accidentabilidad.FactoresContribuyentes.FACTOR_CONT DDOFACCON;
                        DDOFACCON = new NucleusRH.Base.Accidentabilidad.FactoresContribuyentes.FACTOR_CONT();

                        DDOFACCON.c_factor_cont = objRead.c_factor_cont;
                        DDOFACCON.d_factor_cont = objRead.d_factor_cont;

                        //Grabo
                        try
                        {
                            NomadEnvironment.GetCurrentTransaction().Save(DDOFACCON);
                            NomadEnvironment.QueryValueChange("ACC05_FACTOR_CONT", "oi_factor_cont", "c_factor_cont", objRead.c_factor_cont, "", "1",true);
                        }
                        catch (Exception e)
                        {
                            objBatch.Err("Error al grabar registro " + objRead.c_factor_cont + " - " + objRead.d_factor_cont + "' - Linea: " + Linea.ToString() + " - " + e.Message);
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

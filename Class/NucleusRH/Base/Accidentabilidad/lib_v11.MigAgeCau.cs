using System;
using System.Xml;
using System.IO;
using System.Collections;

using Nomad.NSystem.Document;
using Nomad.NSystem.Proxy;
using Nomad.NSystem.Base;
using Nomad.NSystem.Functions;

namespace NucleusRH.Base.Accidentabilidad.MigAgeCau
{

    //////////////////////////////////////////////////////////////////////////////////
    //Clase Agentes causantes
    public partial class AGENTE_CAUSANTE 
    {
        public static void ImportarAgeCau()
        {

            int Linea = 0, Errores = 0;

            NomadBatch objBatch;
            objBatch = NomadBatch.GetBatch("Iniciando...", "Importación de Agentes Causantes");

            NomadXML IDCur;
            NomadXML IDList = new NomadXML();
            NucleusRH.Base.Accidentabilidad.MigAgeCau.AGENTE_CAUSANTE objRead;
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
                    objRead = NucleusRH.Base.Accidentabilidad.MigAgeCau.AGENTE_CAUSANTE.Get(row.GetAttr("id"));

                    //Valido atributos obligatorios
                    if (objRead.c_agente_caus == "" || objRead.d_agente_caus == "")
                    {
                        objBatch.Err("No se especificó el Agente causante, se rechaza el registro '" + objRead.c_agente_caus + " - " + objRead.d_agente_caus + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }

                    //Me fijo si ya existe el Agente causante
                    string oiVal = NomadEnvironment.QueryValue("ACC03_AGENTES_CAUS", "oi_agente_caus", "c_agente_caus", objRead.c_agente_caus, "", true);
                    if (oiVal != null)
                    {
                        objBatch.Err("Ya existe un registro para el Agente causante '" + objRead.c_agente_caus + " - " + objRead.d_agente_caus + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }
                    else
                    {
                        //Creo el Agente causante
                        NucleusRH.Base.Accidentabilidad.AgentesCausantes.AGENTE_CAUS DDOAGECAU;
                        DDOAGECAU = new NucleusRH.Base.Accidentabilidad.AgentesCausantes.AGENTE_CAUS();

                        DDOAGECAU.c_agente_caus = objRead.c_agente_caus;
                        DDOAGECAU.d_agente_caus = objRead.d_agente_caus;

                        //Grabo
                        try
                        {
                            NomadEnvironment.GetCurrentTransaction().Save(DDOAGECAU);
                            NomadEnvironment.QueryValueChange("ACC03_AGENTES_CAUS", "oi_agente_caus", "c_agente_caus", objRead.c_agente_caus, "", "1", true);
                        }
                        catch (Exception e)
                        {
                            objBatch.Err("Error al grabar registro " + objRead.c_agente_caus + " - " + objRead.d_agente_caus + "' - Linea: " + Linea.ToString() + " - " + e.Message);
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

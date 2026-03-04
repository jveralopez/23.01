using System;
using System.Xml;
using System.IO;
using System.Collections;

using Nomad.NSystem.Document;
using Nomad.NSystem.Proxy;
using Nomad.NSystem.Base;
using Nomad.NSystem.Functions;

namespace NucleusRH.Base.Accidentabilidad.MigConPel
{

    //////////////////////////////////////////////////////////////////////////////////
    //Clase Condiciones Peligrosas
    public partial class CONDICION_PELIGROSA 
    {
        public static void ImportarConPel()
        {

            int Linea = 0, Errores = 0;

            NomadBatch objBatch;
            objBatch = NomadBatch.GetBatch("Iniciando...", "Importación de Condiciones Peligrosas");

            NomadXML IDCur;
            NomadXML IDList = new NomadXML();
            NucleusRH.Base.Accidentabilidad.MigConPel.CONDICION_PELIGROSA objRead;
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
                    objRead = NucleusRH.Base.Accidentabilidad.MigConPel.CONDICION_PELIGROSA.Get(row.GetAttr("id"));

                    //Valido atributos obligatorios
                    if (objRead.c_condic_pelig == "" || objRead.d_condic_pelig == "")
                    {
                        objBatch.Err("No se especificó la Condición Peligrosa, se rechaza el registro '" + objRead.c_condic_pelig + " - " + objRead.d_condic_pelig + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }

                    //Me fijo si ya existe la Condición Peligrosa
                    string oiVal = NomadEnvironment.QueryValue("ACC04_CONDIC_PELIG", "oi_condic_pelig", "c_condic_pelig", objRead.c_condic_pelig, "", true);
                    if (oiVal != null)
                    {
                        objBatch.Err("Ya existe un registro para la Condición Peligrosa '" + objRead.c_condic_pelig + " - " + objRead.d_condic_pelig + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }
                    else
                    {
                        //Creo la Condición Peligrosa
                        NucleusRH.Base.Accidentabilidad.CondicionesPeligrosas.CONDIC_PELIG DDOCONPEL;
                        DDOCONPEL = new NucleusRH.Base.Accidentabilidad.CondicionesPeligrosas.CONDIC_PELIG();

                        DDOCONPEL.c_condic_pelig = objRead.c_condic_pelig;
                        DDOCONPEL.d_condic_pelig = objRead.d_condic_pelig;

                        //Grabo
                        try
                        {
                            NomadEnvironment.GetCurrentTransaction().Save(DDOCONPEL);
                            NomadEnvironment.QueryValueChange("ACC04_CONDIC_PELIG", "oi_condic_pelig", "c_condic_pelig", objRead.c_condic_pelig, "", "1",true);
                        }
                        catch (Exception e)
                        {
                            objBatch.Err("Error al grabar registro " + objRead.c_condic_pelig + " - " + objRead.d_condic_pelig + "' - Linea: " + Linea.ToString() + " - " + e.Message);
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

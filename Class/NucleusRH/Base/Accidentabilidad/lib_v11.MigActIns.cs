using System;
using System.Xml;
using System.IO;
using System.Collections;

using Nomad.NSystem.Document;
using Nomad.NSystem.Proxy;
using Nomad.NSystem.Base;
using Nomad.NSystem.Functions;

namespace NucleusRH.Base.Accidentabilidad.MigActIns
{

    //////////////////////////////////////////////////////////////////////////////////
    //Clase Actos Inseguros
    public partial class ACTO_INSEGURO 
    {
        public static void ImportarActIns()
        {

            int Linea = 0, Errores = 0;

            NomadBatch objBatch;
            objBatch = NomadBatch.GetBatch("Iniciando...", "Importación de Actos Inseguros");

            NomadXML IDCur;
            NomadXML IDList = new NomadXML();
            NucleusRH.Base.Accidentabilidad.MigActIns.ACTO_INSEGURO objRead;
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
                    objRead = NucleusRH.Base.Accidentabilidad.MigActIns.ACTO_INSEGURO.Get(row.GetAttr("id"));

                    //Valido atributos obligatorios
                    if (objRead.c_acto_inseg == "" || objRead.d_acto_inseg == "")
                    {
                        objBatch.Err("No se especificó el Acto inseguro, se rechaza el registro '" + objRead.c_acto_inseg + " - " + objRead.d_acto_inseg + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }

                    //Me fijo si ya existe el acto inseguro
                    string oiVal = NomadEnvironment.QueryValue("ACC02_ACTOS_INSEG", "oi_acto_inseg", "c_acto_inseg", objRead.c_acto_inseg, "", true);
                    if (oiVal != null)
                    {
                        objBatch.Err("Ya existe un registro para el Acto inseguro '" + objRead.c_acto_inseg + " - " + objRead.d_acto_inseg + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }
                    else
                    {
                        //Creo el Acto inseguro
                        NucleusRH.Base.Accidentabilidad.ActosInseguros.ACTO_INSEG DDOACTINS;
                        DDOACTINS = new NucleusRH.Base.Accidentabilidad.ActosInseguros.ACTO_INSEG();

                        DDOACTINS.c_acto_inseg = objRead.c_acto_inseg;
                        DDOACTINS.d_acto_inseg = objRead.d_acto_inseg;

                        //Grabo
                        try
                        {
                            NomadEnvironment.GetCurrentTransaction().Save(DDOACTINS);
                            NomadEnvironment.QueryValueChange("ACC02_ACTOS_INSEG", "oi_acto_inseg", "c_acto_inseg", objRead.c_acto_inseg, "", "1", true);
                        }
                        catch (Exception e)
                        {
                            objBatch.Err("Error al grabar registro " + objRead.c_acto_inseg + " - " + objRead.d_acto_inseg + "' - Linea: " + Linea.ToString() + " - " + e.Message);
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

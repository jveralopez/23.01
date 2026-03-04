using System;
using System.Xml;
using System.IO;
using System.Collections;

using Nomad.NSystem.Document;
using Nomad.NSystem.Proxy;
using Nomad.NSystem.Base;
using Nomad.NSystem.Functions;

namespace NucleusRH.Base.MedicinaLaboral.MigTipExa
{

    //////////////////////////////////////////////////////////////////////////////////
    //Clase Tipos de examen
    public partial class TIPO_EXAMEN 
    {
        public static void ImportarTipExa()
        {

            int Linea = 0, Errores = 0;

            NomadBatch objBatch;
            objBatch = NomadBatch.GetBatch("Iniciando...", "Importación de Tipos de examen");

            NomadXML IDCur;
            NomadXML IDList = new NomadXML();
            NucleusRH.Base.MedicinaLaboral.MigTipExa.TIPO_EXAMEN objRead;
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
                    objRead = NucleusRH.Base.MedicinaLaboral.MigTipExa.TIPO_EXAMEN.Get(row.GetAttr("id"));

                    //Valido atributos obligatorios
                    if (objRead.c_tipo_examen == "" || objRead.d_tipo_examen == "")
                    {
                        objBatch.Err("No se especificó el tipo de examen, se rechaza el registro '" + objRead.c_tipo_examen + " - " + objRead.d_tipo_examen + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }

                    //Me fijo si ya existe el tipo de examen
                    string oiVal = NomadEnvironment.QueryValue("MED10_TIPOS_EXAMEN", "oi_tipo_examen", "c_tipo_examen", objRead.c_tipo_examen, "", true);
                    if (oiVal != null)
                    {
                        objBatch.Err("Ya existe un registro para el tipo de examen'" + objRead.c_tipo_examen + " - " + objRead.d_tipo_examen + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }
                    else
                    {
                        //Creo el tipo de examen
                        NucleusRH.Base.MedicinaLaboral.TiposExamen.TIPO_EXAMEN DDOTIPEXA;
                        DDOTIPEXA = new NucleusRH.Base.MedicinaLaboral.TiposExamen.TIPO_EXAMEN();

                        DDOTIPEXA.c_tipo_examen = objRead.c_tipo_examen;
                        DDOTIPEXA.d_tipo_examen = objRead.d_tipo_examen;

                        //Grabo
                        try
                        {
                            NomadEnvironment.GetCurrentTransaction().Save(DDOTIPEXA);
                            NomadEnvironment.QueryValueChange("MED10_TIPOS_EXAMEN", "oi_tipo_examen", "c_tipo_examen", objRead.c_tipo_examen, "", "1", true);
                        }
                        catch (Exception e)
                        {
                            objBatch.Err("Error al grabar registro " + objRead.c_tipo_examen + " - " + objRead.d_tipo_examen + "' - Linea: " + Linea.ToString() + " - " + e.Message);
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

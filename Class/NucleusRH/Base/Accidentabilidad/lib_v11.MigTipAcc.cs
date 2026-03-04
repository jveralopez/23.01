using System;
using System.Xml;
using System.IO;
using System.Collections;

using Nomad.NSystem.Document;
using Nomad.NSystem.Proxy;
using Nomad.NSystem.Base;
using Nomad.NSystem.Functions;

namespace NucleusRH.Base.Accidentabilidad.MigTipAcc
{

    //////////////////////////////////////////////////////////////////////////////////
    //Clase Tipos de Accidentes
    public partial class TIPO_ACCIDENTE 
    {
        public static void ImportarTipAcc()
        {

            int Linea = 0, Errores = 0;

            NomadBatch objBatch;
            objBatch = NomadBatch.GetBatch("Iniciando...", "Importación de Tipos de Accidentes");

            NomadXML IDCur;
            NomadXML IDList = new NomadXML();
            NucleusRH.Base.Accidentabilidad.MigTipAcc.TIPO_ACCIDENTE objRead;
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
                    objRead = NucleusRH.Base.Accidentabilidad.MigTipAcc.TIPO_ACCIDENTE.Get(row.GetAttr("id"));

                    //Valido atributos obligatorios
                    if (objRead.c_tipo_acc == "" || objRead.d_tipo_acc == "")
                    {
                        objBatch.Err("No se especificó el Tipo de Accidente, se rechaza el registro '" + objRead.c_tipo_acc + " - " + objRead.d_tipo_acc + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }

                    //Me fijo si ya existe el tipo de siniestro
                    string oiVal = NomadEnvironment.QueryValue("ACC10_TIPOS_ACC", "oi_tipo_acc", "c_tipo_acc", objRead.c_tipo_acc, "", true);
                    if (oiVal != null)
                    {
                        objBatch.Err("Ya existe un registro para el Tipo de Accidente '" + objRead.c_tipo_acc + " - " + objRead.d_tipo_acc + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }
                    else
                    {
                        //Creo el tipo de Accidente
                        NucleusRH.Base.Accidentabilidad.TiposAccidente.TIPO_ACC DDOTIPACC;
                        DDOTIPACC = new NucleusRH.Base.Accidentabilidad.TiposAccidente.TIPO_ACC();

                        DDOTIPACC.c_tipo_acc = objRead.c_tipo_acc;
                        DDOTIPACC.d_tipo_acc = objRead.d_tipo_acc;

                        //Grabo
                        try
                        {
                            NomadEnvironment.GetCurrentTransaction().Save(DDOTIPACC);
                            NomadEnvironment.QueryValueChange("ACC10_TIPOS_ACC", "oi_tipo_acc", "c_tipo_acc", objRead.c_tipo_acc, "", "1", true);
                        }
                        catch (Exception e)
                        {
                            objBatch.Err("Error al grabar registro " + objRead.c_tipo_acc + " - " + objRead.d_tipo_acc + "' - Linea: " + Linea.ToString() + " - " + e.Message);
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

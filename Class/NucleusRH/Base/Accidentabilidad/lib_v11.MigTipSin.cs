using System;
using System.Xml;
using System.IO;
using System.Collections;

using Nomad.NSystem.Document;
using Nomad.NSystem.Proxy;
using Nomad.NSystem.Base;
using Nomad.NSystem.Functions;

namespace NucleusRH.Base.Accidentabilidad.MigTipSin
{

    //////////////////////////////////////////////////////////////////////////////////
    //Clase Tipos de Siniestros
    public partial class TIPO_SINIESTRO 
    {
        public static void ImportarTipSin()
        {

            int Linea = 0, Errores = 0;

            NomadBatch objBatch;
            objBatch = NomadBatch.GetBatch("Iniciando...", "Importación de Tipos de Siniestros");

            NomadXML IDCur;
            NomadXML IDList = new NomadXML();
            NucleusRH.Base.Accidentabilidad.MigTipSin.TIPO_SINIESTRO objRead;
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
                    objRead = NucleusRH.Base.Accidentabilidad.MigTipSin.TIPO_SINIESTRO.Get(row.GetAttr("id"));

                    string oiTIPSIN = "";
                    //Valido atributos obligatorios
                    if (objRead.c_tipo_siniestro == "" || objRead.d_tipo_siniestro == "")
                    {
                        objBatch.Err("No se especificó el Tipo de Siniestro, se rechaza el registro '" + objRead.c_tipo_siniestro + " - " + objRead.d_tipo_siniestro + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }

                    //Me fijo si ya existe el tipo de siniestro
                    string oiVal = NomadEnvironment.QueryValue("ACC13_TIP_SINIESTRO", "oi_tipo_siniestro", "c_tipo_siniestro", objRead.c_tipo_siniestro, "", true);
                    if (oiVal != null)
                    {
                        objBatch.Err("Ya existe un registro para el Tipo de Siniestro '" + objRead.c_tipo_siniestro + " - " + objRead.d_tipo_siniestro + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }
                    else
                    {
                        //Creo el tipo de siniestro
                        NucleusRH.Base.Accidentabilidad.TiposSiniestros.TIPO_SINIESTRO DDOTIPSIN;
                        DDOTIPSIN = new NucleusRH.Base.Accidentabilidad.TiposSiniestros.TIPO_SINIESTRO();

                        DDOTIPSIN.c_tipo_siniestro = objRead.c_tipo_siniestro;
                        DDOTIPSIN.d_tipo_siniestro = objRead.d_tipo_siniestro;

                        //Grabo
                        try
                        {
                            NomadEnvironment.GetCurrentTransaction().Save(DDOTIPSIN);
                            NomadEnvironment.QueryValueChange("ACC13_TIP_SINIESTRO", "oi_tipo_siniestro", "c_tipo_siniestro", objRead.c_tipo_siniestro, "", "1", true);
                        }
                        catch (Exception e)
                        {
                            objBatch.Err("Error al grabar registro " + objRead.c_tipo_siniestro + " - " + objRead.d_tipo_siniestro + "' - Linea: " + Linea.ToString() + " - " + e.Message);
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

using System;
using System.Xml;
using System.IO;
using System.Collections;

using Nomad.NSystem.Document;
using Nomad.NSystem.Proxy;
using Nomad.NSystem.Base;
using Nomad.NSystem.Functions;

namespace NucleusRH.Base.Accidentabilidad.MigOcupa
{

    //////////////////////////////////////////////////////////////////////////////////
    //Clase Ocupaciones
    public partial class OCUPACION 
    {
        public static void ImportarOcupa()
        {

            int Linea = 0, Errores = 0;

            NomadBatch objBatch;
            objBatch = NomadBatch.GetBatch("Iniciando...", "Importación de Ocupaciones");

            NomadXML IDCur;
            NomadXML IDList = new NomadXML();
            NucleusRH.Base.Accidentabilidad.MigOcupa.OCUPACION objRead;
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
                    objRead = NucleusRH.Base.Accidentabilidad.MigOcupa.OCUPACION.Get(row.GetAttr("id"));

                    //Valido atributos obligatorios
                    if (objRead.c_ocupacion == "" || objRead.d_ocupacion == "")
                    {
                        objBatch.Err("No se especificó la Ocupación, se rechaza el registro '" + objRead.c_ocupacion + " - " + objRead.d_ocupacion + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }

                    //Me fijo si ya existe la Ocupación
                    string oiVal = NomadEnvironment.QueryValue("ACC08_OCUPACIONES", "oi_ocupacion", "c_ocupacion", objRead.c_ocupacion, "", true);
                    if (oiVal != null)
                    {
                        objBatch.Err("Ya existe un registro para la Ocupación '" + objRead.c_ocupacion + " - " + objRead.d_ocupacion + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }
                    else
                    {
                        //Creo la Ocupación
                        NucleusRH.Base.Accidentabilidad.Ocupaciones.OCUPACION DDOOCUPA;
                        DDOOCUPA = new NucleusRH.Base.Accidentabilidad.Ocupaciones.OCUPACION();

                        DDOOCUPA.c_ocupacion = objRead.c_ocupacion;
                        DDOOCUPA.d_ocupacion = objRead.d_ocupacion;

                        //Grabo
                        try
                        {
                            NomadEnvironment.GetCurrentTransaction().Save(DDOOCUPA);
                            NomadEnvironment.QueryValueChange("ACC08_OCUPACIONES", "oi_ocupacion", "c_ocupacion", objRead.c_ocupacion, "", "1", true);
                        }
                        catch (Exception e)
                        {
                            objBatch.Err("Error al grabar registro " + objRead.c_ocupacion + " - " + objRead.d_ocupacion + "' - Linea: " + Linea.ToString() + " - " + e.Message);
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

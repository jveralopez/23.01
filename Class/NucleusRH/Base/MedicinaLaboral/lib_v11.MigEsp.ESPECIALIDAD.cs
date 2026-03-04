using System;
using System.Xml;
using System.IO;
using System.Collections;

using Nomad.NSystem.Document;
using Nomad.NSystem.Proxy;
using Nomad.NSystem.Base;
using Nomad.NSystem.Functions;

namespace NucleusRH.Base.MedicinaLaboral.MigEsp
{

    //////////////////////////////////////////////////////////////////////////////////
    //Clase Especialidades
    public partial class ESPECIALIDAD 
    {
        public static void ImportarEsp()
        {

            int Linea = 0, Errores = 0;

            NomadBatch objBatch;
            objBatch = NomadBatch.GetBatch("Iniciando...", "Importación de Especialidades");

            NomadXML IDCur;
            NomadXML IDList = new NomadXML();
            NucleusRH.Base.MedicinaLaboral.MigEsp.ESPECIALIDAD objRead;
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
                    objRead = NucleusRH.Base.MedicinaLaboral.MigEsp.ESPECIALIDAD.Get(row.GetAttr("id"));

                    //Valido atributos obligatorios
                    if (objRead.c_especialidad == "" || objRead.d_especialidad == "")
                    {
                        objBatch.Err("No se especificó la Especialidad, se rechaza el registro '" + objRead.c_especialidad + " - " + objRead.d_especialidad + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }

                    //Me fijo si ya existe la Especialidad
                    string oiVal = NomadEnvironment.QueryValue("MED01_ESPECIALIDAD", "oi_especialidad", "c_especialidad", objRead.c_especialidad, "", true);
                    if (oiVal != null)
                    {
                        objBatch.Err("Ya existe un registro para la Especialidad '" + objRead.c_especialidad + " - " + objRead.d_especialidad + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }
                    else
                    {
                        //Creo el Acto inseguro
                        NucleusRH.Base.MedicinaLaboral.Especialidades.ESPECIALIDAD DDOESP;
                        DDOESP = new NucleusRH.Base.MedicinaLaboral.Especialidades.ESPECIALIDAD();

                        DDOESP.c_especialidad = objRead.c_especialidad;
                        DDOESP.d_especialidad = objRead.d_especialidad;

                        //Grabo
                        try
                        {
                            NomadEnvironment.GetCurrentTransaction().Save(DDOESP);
                            NomadEnvironment.QueryValueChange("MED01_ESPECIALIDAD", "oi_especialidad", "c_especialidad", objRead.c_especialidad, "", "1",true);
                        }
                        catch (Exception e)
                        {
                            objBatch.Err("Error al grabar registro " + objRead.c_especialidad + " - " + objRead.d_especialidad + "' - Linea: " + Linea.ToString() + " - " + e.Message);
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

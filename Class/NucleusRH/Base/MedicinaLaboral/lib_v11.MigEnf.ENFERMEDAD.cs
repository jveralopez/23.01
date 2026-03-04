using System;
using System.Xml;
using System.IO;
using System.Collections;

using Nomad.NSystem.Document;
using Nomad.NSystem.Proxy;
using Nomad.NSystem.Base;
using Nomad.NSystem.Functions;

namespace NucleusRH.Base.MedicinaLaboral.MigEnf
{

    //////////////////////////////////////////////////////////////////////////////////
    //Clase Enfermedades
    public partial class ENFERMEDAD 
    {
        public static void ImportarEnf()
        {

            int Linea = 0, Errores = 0;

            NomadBatch objBatch;
            objBatch = NomadBatch.GetBatch("Iniciando...", "Importación de Enfermedades");

            NomadXML IDCur;
            NomadXML IDList = new NomadXML();
            NucleusRH.Base.MedicinaLaboral.MigEnf.ENFERMEDAD objRead;
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
                    objRead = NucleusRH.Base.MedicinaLaboral.MigEnf.ENFERMEDAD.Get(row.GetAttr("id"));

                    //Valido atributos obligatorios    
                    if (objRead.c_enfermedad == "" || objRead.d_enfermedad == "")
                    {
                        objBatch.Err("No se especificó la Enfermedad, se rechaza el registro '" + objRead.c_enfermedad + " - " + objRead.d_enfermedad + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }

                    //Me fijo si ya existe la Enfermedad
                    string oiVal = NomadEnvironment.QueryValue("MED06_ENFERMEDADES", "oi_enfermedad", "c_enfermedad", objRead.c_enfermedad, "", true);
                    if (oiVal != null)
                    {
                        objBatch.Err("Ya existe un registro para la Enfermedad '" + objRead.c_enfermedad + " - " + objRead.d_enfermedad + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }
                    else
                    {
                        //Creo la Enfermedad
                        NucleusRH.Base.MedicinaLaboral.Enfermedades.ENFERMEDAD DDOENF;
                        DDOENF = new NucleusRH.Base.MedicinaLaboral.Enfermedades.ENFERMEDAD();

                        DDOENF.c_enfermedad = objRead.c_enfermedad;
                        DDOENF.d_enfermedad = objRead.d_enfermedad;

                        //Grabo
                        try
                        {
                            NomadEnvironment.GetCurrentTransaction().Save(DDOENF);
                            NomadEnvironment.QueryValueChange("MED06_ENFERMEDADES", "oi_enfermedad", "c_enfermedad", objRead.c_enfermedad, "", "1", true);
                        }
                        catch (Exception e)
                        {
                            objBatch.Err("Error al grabar registro " + objRead.c_enfermedad + " - " + objRead.d_enfermedad + "' - Linea: " + Linea.ToString() + " - " + e.Message);
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

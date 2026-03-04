using System;
using System.Xml;
using System.IO;
using System.Collections;

using Nomad.NSystem.Document;
using Nomad.NSystem.Proxy;
using Nomad.NSystem.Base;
using Nomad.NSystem.Functions;

namespace NucleusRH.Base.MedicinaLaboral.MigMotMed
{

    //////////////////////////////////////////////////////////////////////////////////
    //Clase Motivos de Medicación
    public partial class MOT_MEDICO 
    {
        public static void ImportarMotMed()
        {

            int Linea = 0, Errores = 0;

            NomadBatch objBatch;
            objBatch = NomadBatch.GetBatch("Iniciando...", "Importación de Motivos de Medicación");

            NomadXML IDCur;
            NomadXML IDList = new NomadXML();
            NucleusRH.Base.MedicinaLaboral.MigMotMed.MOT_MEDICO objRead;
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
                    objRead = NucleusRH.Base.MedicinaLaboral.MigMotMed.MOT_MEDICO.Get(row.GetAttr("id"));

                    //Valido atributos obligatorios            
                    if (objRead.c_motivo_med == "" || objRead.d_motivo_med == "")
                    {
                        objBatch.Err("No se especificó el motivo de medicación, se rechaza el registro '" + objRead.c_motivo_med + " - " + objRead.d_motivo_med + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }

                    //Me fijo si ya existe el motivo de medicación
                    string oiVal = NomadEnvironment.QueryValue("MED09_MOTIVOS_MED", "oi_mot_medico", "c_motivo_med", objRead.c_motivo_med, "", true);
                    if (oiVal != null)
                    {
                        objBatch.Err("Ya existe un registro el motivo de medicación'" + objRead.c_motivo_med + " - " + objRead.d_motivo_med + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }
                    else
                    {
                        //Creo el motivo de medicación
                        NucleusRH.Base.MedicinaLaboral.MotivosMedicacion.MOTIVO_MED DDOMOTMED;
                        DDOMOTMED = new NucleusRH.Base.MedicinaLaboral.MotivosMedicacion.MOTIVO_MED();

                        DDOMOTMED.c_motivo_med = objRead.c_motivo_med;
                        DDOMOTMED.d_motivo_med = objRead.d_motivo_med;

                        //Grabo
                        try
                        {
                            NomadEnvironment.GetCurrentTransaction().Save(DDOMOTMED);
                            NomadEnvironment.QueryValueChange("MED09_MOTIVOS_MED", "oi_mot_medico", "c_motivo_med", objRead.c_motivo_med, "", "1",true);
                        }
                        catch (Exception e)
                        {
                            objBatch.Err("Error al grabar registro " + objRead.c_motivo_med + " - " + objRead.d_motivo_med + "' - Linea: " + Linea.ToString() + " - " + e.Message);
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

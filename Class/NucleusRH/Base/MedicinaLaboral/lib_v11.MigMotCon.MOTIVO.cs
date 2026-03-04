using System;
using System.Xml;
using System.IO;
using System.Collections;

using Nomad.NSystem.Document;
using Nomad.NSystem.Proxy;
using Nomad.NSystem.Base;
using Nomad.NSystem.Functions;

namespace NucleusRH.Base.MedicinaLaboral.MigMotCon
{

    //////////////////////////////////////////////////////////////////////////////////
    //Clase Motivos de consultas
    public partial class MOTIVO 
    {
        public static void ImportarMotCon()
        {

            int Linea = 0, Errores = 0;

            NomadBatch objBatch;
            objBatch = NomadBatch.GetBatch("Iniciando...", "Importación de Motivos de consulta");

            NomadXML IDCur;
            NomadXML IDList = new NomadXML();
            NucleusRH.Base.MedicinaLaboral.MigMotCon.MOTIVO objRead;
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
                    objRead = NucleusRH.Base.MedicinaLaboral.MigMotCon.MOTIVO.Get(row.GetAttr("id"));

                    //Valido atributos obligatorios
                    if (objRead.c_motivo_consulta == "" || objRead.d_motivo_consulta == "")
                    {
                        objBatch.Err("No se especificó el motivo de consulta, se rechaza el registro '" + objRead.c_motivo_consulta + " - " + objRead.d_motivo_consulta + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }

                    //Me fijo si ya existe el motivo de consulta
                    string oiVal = NomadEnvironment.QueryValue("MED05_MOTIVOS_CONS", "oi_motivo_consulta", "c_motivo_consulta", objRead.c_motivo_consulta, "", true);
                    if (oiVal != null)
                    {
                        objBatch.Err("Ya existe un registro para el motivo de consulta'" + objRead.c_motivo_consulta + " - " + objRead.d_motivo_consulta + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }
                    else
                    {
                        //Creo el motivo de consulta
                        NucleusRH.Base.MedicinaLaboral.MotivosConsulta.MOTIVO_CONSULTA DDOMOT;
                        DDOMOT = new NucleusRH.Base.MedicinaLaboral.MotivosConsulta.MOTIVO_CONSULTA();

                        DDOMOT.c_motivo_consulta = objRead.c_motivo_consulta;
                        DDOMOT.d_motivo_consulta = objRead.d_motivo_consulta;

                        //Grabo
                        try
                        {
                            NomadEnvironment.GetCurrentTransaction().Save(DDOMOT);
                            NomadEnvironment.QueryValueChange("MED05_MOTIVOS_CONS", "oi_motivo_consulta", "c_motivo_consulta", objRead.c_motivo_consulta, "", "1",true);
                        }
                        catch (Exception e)
                        {
                            objBatch.Err("Error al grabar registro " + objRead.c_motivo_consulta + " - " + objRead.d_motivo_consulta + "' - Linea: " + Linea.ToString() + " - " + e.Message);
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

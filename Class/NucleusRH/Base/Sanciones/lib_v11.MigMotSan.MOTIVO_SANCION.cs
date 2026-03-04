using System;
using System.Xml;
using System.IO;
using System.Collections;

using Nomad.NSystem.Document;
using Nomad.NSystem.Proxy;
using Nomad.NSystem.Base;
using Nomad.NSystem.Functions;

namespace NucleusRH.Base.Sanciones.MigMotSan
{

    //////////////////////////////////////////////////////////////////////////////////
    //Clase Motivos de Sanción
    public partial class MOTIVO_SANCION 
    {
        public static void ImportarMotSan()
        {

            int Linea = 0, Errores = 0;

            NomadBatch objBatch;
            objBatch = NomadBatch.GetBatch("Iniciando...", "Importación de Motivos de Sanción");

            NomadXML IDCur;
            NomadXML IDList = new NomadXML();
            NucleusRH.Base.Sanciones.MigMotSan.MOTIVO_SANCION objRead;
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
                    objRead = NucleusRH.Base.Sanciones.MigMotSan.MOTIVO_SANCION.Get(row.GetAttr("id"));

                    string oiMOTSAN = "", oiSAN = "";
                    //Valido atributos obligatorios
                    if (objRead.c_motivo_sancion == "" || objRead.d_motivo_sancion == "")
                    {
                        objBatch.Err("No se especificó el motivo de la sanción, se rechaza el registro '" + objRead.c_motivo_sancion + " - " + objRead.d_motivo_sancion + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }

                    //Recupero la sanción   	
                    if (objRead.c_sancion != "")
                    {
                        oiSAN = NomadEnvironment.QueryValue("SAN02_SANCIONES", "oi_sancion", "c_sancion", objRead.c_sancion, "", true);
                        if (oiSAN == null)
                        {
                            objBatch.Err("La sanción no existe, se rechaza el registro '" + objRead.c_motivo_sancion + " - " + objRead.d_motivo_sancion + "' - Linea: " + Linea.ToString());
                            Errores++;
                            continue;
                        }
                    }

                    //Me fijo si ya existe el motivo de sancion
                    string oiVal = NomadEnvironment.QueryValue("SAN01_MOTIVOS_SANC", "oi_motivo_sancion", "c_motivo_sancion", objRead.c_motivo_sancion, "", true);
                    if (oiVal != null)
                    {
                        objBatch.Err("Ya existe un registro para el Motivo de sanción '" + objRead.c_motivo_sancion + " - " + objRead.d_motivo_sancion + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }
                    else
                    {
                        //Creo el motivo de la sancion
                        NucleusRH.Base.Sanciones.Motivos_Sanciones.MOTIVO_SANCION DDOMOTSAN;
                        DDOMOTSAN = new NucleusRH.Base.Sanciones.Motivos_Sanciones.MOTIVO_SANCION();

                        DDOMOTSAN.c_motivo_sancion = objRead.c_motivo_sancion;
                        DDOMOTSAN.d_motivo_sancion = objRead.d_motivo_sancion;
                        DDOMOTSAN.d_motivo_corta = objRead.d_motivo_corta;
                        if (oiSAN != null)
                            DDOMOTSAN.oi_sancion = oiSAN;
                        DDOMOTSAN.e_cant_repeticion = objRead.e_cant_repeticion;

                        //Grabo
                        try
                        {
                            NomadEnvironment.GetCurrentTransaction().Save(DDOMOTSAN);
                            NomadEnvironment.QueryValueChange("SAN01_MOTIVOS_SANC", "oi_motivo_sancion", "c_motivo_sancion", objRead.c_motivo_sancion, "", "1", true);
                        }
                        catch (Exception e)
                        {
                            objBatch.Err("Error al grabar registro " + objRead.c_motivo_sancion + " - " + objRead.d_motivo_sancion + "' - Linea: " + Linea.ToString() + " - " + e.Message);
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

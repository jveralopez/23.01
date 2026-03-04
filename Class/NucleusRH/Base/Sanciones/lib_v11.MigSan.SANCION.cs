using System;
using System.Xml;
using System.IO;
using System.Collections;

using Nomad.NSystem.Document;
using Nomad.NSystem.Proxy;
using Nomad.NSystem.Base;
using Nomad.NSystem.Functions;

namespace NucleusRH.Base.Sanciones.MigSan
{

    //////////////////////////////////////////////////////////////////////////////////
    //Clase Sanciones
    public partial class SANCION 
    {
        public static void ImportarSan()
        {

            int Linea = 0, Errores = 0;

            NomadBatch objBatch;
            objBatch = NomadBatch.GetBatch("Iniciando...", "Importación de Sanciones");

            NomadXML IDCur;
            NomadXML IDList = new NomadXML();
            NucleusRH.Base.Sanciones.MigSan.SANCION objRead;

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
                    objRead = NucleusRH.Base.Sanciones.MigSan.SANCION.Get(row.GetAttr("id"));

                    //Valido atributos obligatorios  	                                             
                    if (objRead.c_sancion == "" || objRead.d_sancion == "")
                    {
                        objBatch.Err("No se especificó la Sanción, se rechaza el registro '" + objRead.c_sancion + " - " + objRead.d_sancion + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }
                    if (objRead.c_tipo_san == "")
                    {
                        objBatch.Err("No se especificó el Tipo de Sanción, se rechaza el registro '" + objRead.c_sancion + " - " + objRead.d_sancion + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }

                    //Recupero los OI de los códigos ingresados
                    string oiTIPSAN = "", oiPER = "", oiEMP = "";

                    //Recuperar la empresa
                    if (objRead.c_tipo_san != "")
                    {
                        oiTIPSAN = NomadEnvironment.QueryValue("SAN05_TIPOS_SAN", "oi_tipo_san", "c_tipo_san", objRead.c_tipo_san, "", true);
                        if (oiTIPSAN == null)
                        {
                            objBatch.Err("El tipo de sanción no existe, se rechaza el registro '" + objRead.c_sancion + " - " + objRead.d_sancion + "' - Linea: " + Linea.ToString());
                            Errores++;
                            continue;
                        }
                    }

                    //Me fijo si ya existe la Sanción
                    string oiVal = NomadEnvironment.QueryValue("SAN02_SANCIONES", "oi_sancion", "c_sancion", objRead.c_sancion, "", true);

                    if (oiVal != null)
                    {
                        objBatch.Err("Ya existe un registro para la Sanción '" + objRead.c_sancion + " - " + objRead.d_sancion + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }
                    else
                    {
                        //Creo la Sanción   
                        NucleusRH.Base.Sanciones.Sanciones.SANCION DDOSAN;
                        DDOSAN = new NucleusRH.Base.Sanciones.Sanciones.SANCION();
                        DDOSAN.c_sancion = objRead.c_sancion;
                        DDOSAN.d_sancion = objRead.d_sancion;
                        DDOSAN.e_dias_max_susp = objRead.e_dias_max_susp;
                        DDOSAN.e_dias_suspension = objRead.e_dias_suspension;
                        if (objRead.l_notificacionNull)
                            DDOSAN.l_notificacion = false;
                        DDOSAN.l_notificacion = objRead.l_notificacion;
                        if (objRead.l_suspensionNull)
                            DDOSAN.l_suspension = false;
                        DDOSAN.l_suspension = objRead.l_suspension;
                        DDOSAN.oi_tipo_san = oiTIPSAN;

                        //Grabo
                        try
                        {
                            NomadEnvironment.GetCurrentTransaction().Save(DDOSAN);
                            NomadEnvironment.QueryValueChange("SAN02_SANCIONES", "oi_sancion", "c_sancion", objRead.c_sancion, "", "1", true);
                        }
                        catch (Exception e)
                        {
                            objBatch.Err("Error al grabar registro " + objRead.c_sancion + " - " + objRead.d_sancion + "' - Linea: " + Linea.ToString() + " - " + e.Message);
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

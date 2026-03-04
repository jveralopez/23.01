using System;
using System.Xml;
using System.IO;
using System.Collections;

using Nomad.NSystem.Document;
using Nomad.NSystem.Proxy;
using Nomad.NSystem.Base;
using Nomad.NSystem.Functions;

namespace NucleusRH.Base.Accidentabilidad.MigRegCue
{

    //////////////////////////////////////////////////////////////////////////////////
    //Clase Regiones del Cuerpo
    public partial class REGION_CUERPO 
    {
        public static void ImportarRegCue()
        {

            int Linea = 0, Errores = 0;

            NomadBatch objBatch;
            objBatch = NomadBatch.GetBatch("Iniciando...", "Importación de Regiones del Cuerpo");

            NomadXML IDCur;
            NomadXML IDList = new NomadXML();
            NucleusRH.Base.Accidentabilidad.MigRegCue.REGION_CUERPO objRead;
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
                    objRead = NucleusRH.Base.Accidentabilidad.MigRegCue.REGION_CUERPO.Get(row.GetAttr("id"));

                    //Valido atributos obligatorios
                    if (objRead.c_reg_cuerpo == "" || objRead.d_reg_cuerpo == "")
                    {
                        objBatch.Err("No se especificó la región del cuerpo, se rechaza el registro '" + objRead.c_reg_cuerpo + " - " + objRead.d_reg_cuerpo + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }

                    //Me fijo si ya existe la región del cuerpo
                    string oiVal = NomadEnvironment.QueryValue("ACC09_REG_CUERPO", "oi_reg_cuerpo", "c_reg_cuerpo", objRead.c_reg_cuerpo, "", true);
                    if (oiVal != null)
                    {
                        objBatch.Err("Ya existe un registro para la región del cuerpo '" + objRead.c_reg_cuerpo + " - " + objRead.d_reg_cuerpo + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }
                    else
                    {
                        //Creo la región del cuerpo
                        NucleusRH.Base.Accidentabilidad.RegionesCuerpo.REG_CUERPO DDOREGCUE;
                        DDOREGCUE = new NucleusRH.Base.Accidentabilidad.RegionesCuerpo.REG_CUERPO();

                        DDOREGCUE.c_reg_cuerpo = objRead.c_reg_cuerpo;
                        DDOREGCUE.d_reg_cuerpo = objRead.d_reg_cuerpo;

                        //Grabo
                        try
                        {
                            NomadEnvironment.GetCurrentTransaction().Save(DDOREGCUE);
                            NomadEnvironment.QueryValueChange("ACC09_REG_CUERPO", "oi_reg_cuerpo", "c_reg_cuerpo", objRead.c_reg_cuerpo, "", "1", true);
                        }
                        catch (Exception e)
                        {
                            objBatch.Err("Error al grabar registro " + objRead.c_reg_cuerpo + " - " + objRead.d_reg_cuerpo + "' - Linea: " + Linea.ToString() + " - " + e.Message);
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

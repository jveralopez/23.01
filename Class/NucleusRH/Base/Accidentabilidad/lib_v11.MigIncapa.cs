using System;
using System.Xml;
using System.IO;
using System.Collections;

using Nomad.NSystem.Document;
using Nomad.NSystem.Proxy;
using Nomad.NSystem.Base;
using Nomad.NSystem.Functions;

namespace NucleusRH.Base.Accidentabilidad.MigIncapa
{

    //////////////////////////////////////////////////////////////////////////////////
    //Clase Incapacidades
    public partial class INCAPACIDAD 
    {
        public static void ImportarIncapa()
        {
#line 1 "Clase.NucleusRH.Base.Accidentabilidad.MigIncapa.INCAPACIDAD.Metodo.ImportarIncapa"
            int Linea = 0, Errores = 0;

            NomadBatch objBatch;
            objBatch = NomadBatch.GetBatch("Iniciando...", "Importación de Incapacidades");

            NomadXML IDCur;
            NomadXML IDList = new NomadXML();
            NucleusRH.Base.Accidentabilidad.MigIncapa.INCAPACIDAD objRead;
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
                    objRead = NucleusRH.Base.Accidentabilidad.MigIncapa.INCAPACIDAD.Get(row.GetAttr("id"));

                    //Valido atributos obligatorios
                    if (objRead.c_incapacidad == "" || objRead.d_incapacidad == "")
                    {
                        objBatch.Err("No se especificó la incapacidad, se rechaza el registro '" + objRead.c_incapacidad + " - " + objRead.d_incapacidad + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }

                    //Me fijo si ya existe la incapacidad
                    string oiVal = NomadEnvironment.QueryValue("ACC06_INCAPACIDAD", "oi_incapacidad", "c_incapacidad", objRead.c_incapacidad, "", true);
                    if (oiVal != null)
                    {
                        objBatch.Err("Ya existe un registro para la la incapacidad '" + objRead.c_incapacidad + " - " + objRead.d_incapacidad + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }
                    else
                    {
                        //Creo la incapacidad
                        NucleusRH.Base.Accidentabilidad.Incapacidades.INCAPACIDAD DDOINCAPA;
                        DDOINCAPA = new NucleusRH.Base.Accidentabilidad.Incapacidades.INCAPACIDAD();

                        DDOINCAPA.c_incapacidad = objRead.c_incapacidad;
                        DDOINCAPA.d_incapacidad = objRead.d_incapacidad;

                        //Grabo
                        try
                        {
                            NomadEnvironment.GetCurrentTransaction().Save(DDOINCAPA);
                            NomadEnvironment.QueryValueChange("ACC06_INCAPACIDAD", "oi_incapacidad", "c_incapacidad", objRead.c_incapacidad, "", "1", true);
                        }
                        catch (Exception e)
                        {
                            objBatch.Err("Error al grabar registro " + objRead.c_incapacidad + " - " + objRead.d_incapacidad + "' - Linea: " + Linea.ToString() + " - " + e.Message);
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
#line default
        }

    }
}

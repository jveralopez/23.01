using System;
using System.Xml;
using System.IO;
using System.Collections;

using Nomad.NSystem.Document;
using Nomad.NSystem.Proxy;
using Nomad.NSystem.Base;
using Nomad.NSystem.Functions;

namespace NucleusRH.Base.MedicinaLaboral.MigExa
{

    //////////////////////////////////////////////////////////////////////////////////
    //Clase Exámenes
    public partial class EXAMEN 
    {
        public static void ImportarExa()
        {

            int Linea = 0, Errores = 0;

            NomadBatch objBatch;
            objBatch = NomadBatch.GetBatch("Iniciando...", "Importación de Exámenes");

            NomadXML IDCur;
            NomadXML IDList = new NomadXML();
            NucleusRH.Base.MedicinaLaboral.MigExa.EXAMEN objRead;
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
                    objRead = NucleusRH.Base.MedicinaLaboral.MigExa.EXAMEN.Get(row.GetAttr("id"));

                    //Valido atributos obligatorios
                    if (objRead.c_examen == "" || objRead.d_examen == "")
                    {
                        objBatch.Err("No se especificó el examen, se rechaza el registro '" + objRead.c_examen + " - " + objRead.d_examen + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }

                    if (objRead.c_tipo_examen == "")
                    {
                        objBatch.Err("No se especificó el Tipo de examen, se rechaza el registro '" + objRead.c_examen + " - " + objRead.d_examen + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }

                    //Recupero los OI de los c&#243;digos ingresados
                    string oiTIPO = "", oiCAT = "";

                    //Recupero el tipo de examen
                    oiTIPO = NomadEnvironment.QueryValue("MED10_TIPOS_EXAMEN", "oi_tipo_examen", "c_tipo_examen", objRead.c_tipo_examen, "", true);
                    if (oiTIPO == null)
                    {
                        objBatch.Err("El tipo de examen no existe, se rechaza el registro '" + objRead.c_examen + " - " + objRead.d_examen + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }
                    //Recupero la categoría de examen
                    oiCAT = NomadEnvironment.QueryValue("MED11_CATEG_EXAMEN", "oi_categ_examen", "c_categ_examen", objRead.c_categ_examen, "", true);
                    if (oiCAT == null)
                    {
                        objBatch.Err("La categoría de examen no existe, se rechaza el registro '" + objRead.c_examen + " - " + objRead.d_examen + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }

                    //Me fijo si ya existe el examen
                    string oiVal = NomadEnvironment.QueryValue("MED01_ESPECIALIDAD", "oi_especialidad", "c_examen", objRead.c_examen, "", true);
                    if (oiVal != null)
                    {
                        objBatch.Err("Ya existe un registro para el examen'" + objRead.c_examen + " - " + objRead.d_examen + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }
                    else
                    {
                        //Creo el examen
                        NucleusRH.Base.MedicinaLaboral.TiposExamen.EXAMEN DDOEXA;
                        DDOEXA = new NucleusRH.Base.MedicinaLaboral.TiposExamen.EXAMEN();

                        DDOEXA.c_examen = objRead.c_examen;
                        DDOEXA.d_examen = objRead.d_examen;
                        DDOEXA.oi_categ_examen = oiCAT;
                        DDOEXA.oi_tipo_examen = int.Parse(oiTIPO);
                        DDOEXA.e_dias_vencimiento = objRead.e_dias_vencimiento;
                        DDOEXA.n_costo = objRead.n_costo;
                        DDOEXA.o_examen = objRead.o_examen;


                        //Grabo
                        try
                        {
                            NomadEnvironment.GetCurrentTransaction().Save(DDOEXA);
                            NomadEnvironment.QueryValueChange("MED01_ESPECIALIDAD", "oi_especialidad", "c_examen", objRead.c_examen, "", "1",true);
                        }
                        catch (Exception e)
                        {
                            objBatch.Err("Error al grabar registro " + objRead.c_examen + " - " + objRead.d_examen + "' - Linea: " + Linea.ToString() + " - " + e.Message);
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

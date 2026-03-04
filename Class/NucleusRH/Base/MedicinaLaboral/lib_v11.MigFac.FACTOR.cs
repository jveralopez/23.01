using System;
using System.Xml;
using System.IO;
using System.Collections;

using Nomad.NSystem.Document;
using Nomad.NSystem.Proxy;
using Nomad.NSystem.Base;
using Nomad.NSystem.Functions;

namespace NucleusRH.Base.MedicinaLaboral.MigFac
{

    //////////////////////////////////////////////////////////////////////////////////
    //Clase Factores
    public partial class FACTOR 
    {
        public static void ImportarFac()
        {

            int Linea = 0, Errores = 0;

            NomadBatch objBatch;
            objBatch = NomadBatch.GetBatch("Iniciando...", "Importación de Factores");

            NomadXML IDCur;
            NomadXML IDList = new NomadXML();
            NucleusRH.Base.MedicinaLaboral.MigFac.FACTOR objRead;
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
                    objRead = NucleusRH.Base.MedicinaLaboral.MigFac.FACTOR.Get(row.GetAttr("id"));

                    //Valido atributos obligatorios            
                    if (objRead.c_factor_examen == "" || objRead.d_factor_examen == "")
                    {
                        objBatch.Err("No se especificó el factor de examen, se rechaza el registro '" + objRead.c_factor_examen + " - " + objRead.d_factor_examen + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }

                    if (objRead.c_examen == "")
                    {
                        objBatch.Err("No se especificó el Examen, se rechaza el registro '" + objRead.c_factor_examen + " - " + objRead.c_factor_examen + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }

                    //Recupero los OI de los c&#243;digos ingresados
                    string oiEXA = "";

                    //Recupero EL EXAMEN
                    oiEXA = NomadEnvironment.QueryValue("ORG03_EMPRESAS", "oi_examen", "c_examen", objRead.c_examen, "", true);
                    if (oiEXA == null)
                    {
                        objBatch.Err("El examen no existe, se rechaza el registro '" + objRead.c_factor_examen + " - " + objRead.d_factor_examen + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }

                    //Me fijo si ya existe el fator
                    string oiVal = NomadEnvironment.QueryValue("MED10_FACTORES", "oi_factor_examen", "c_factor_examen", objRead.c_factor_examen, "", true);
                    if (oiVal != null)
                    {
                        objBatch.Err("Ya existe un registro para el factor de examen '" + objRead.c_factor_examen + " - " + objRead.d_factor_examen + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }
                    else
                    {
                        //Creo el factor de examen
                        NucleusRH.Base.MedicinaLaboral.TiposExamen.FACTOR_EXAMEN DDOFAC;
                        DDOFAC = new NucleusRH.Base.MedicinaLaboral.TiposExamen.FACTOR_EXAMEN();

                        DDOFAC.c_factor_examen = objRead.c_factor_examen;
                        DDOFAC.d_factor_examen = objRead.d_factor_examen;
                        DDOFAC.oi_examen = int.Parse(oiEXA);

                        //Grabo
                        try
                        {
                            NomadEnvironment.GetCurrentTransaction().Save(DDOFAC);
                            NomadEnvironment.QueryValueChange("MED10_FACTORES", "oi_factor_examen", "c_factor_examen", objRead.c_factor_examen, "", "1", true);
                        }
                        catch (Exception e)
                        {
                            objBatch.Err("Error al grabar registro " + objRead.c_factor_examen + " - " + objRead.d_factor_examen + "' - Linea: " + Linea.ToString() + " - " + e.Message);
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

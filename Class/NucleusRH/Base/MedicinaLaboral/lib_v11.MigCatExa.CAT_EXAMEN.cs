using System;
using System.Xml;
using System.IO;
using System.Collections;

using Nomad.NSystem.Document;
using Nomad.NSystem.Proxy;
using Nomad.NSystem.Base;
using Nomad.NSystem.Functions;

namespace NucleusRH.Base.MedicinaLaboral.MigCatExa
{

    //////////////////////////////////////////////////////////////////////////////////
    //Clase Categorías de Exámen
    public partial class CAT_EXAMEN 
    {
        public static void ImportarCatExa()
        {

            int Linea = 0, Errores = 0;

            NomadBatch objBatch;
            objBatch = NomadBatch.GetBatch("Iniciando...", "Importación de Categorías de examen");

            NomadXML IDCur;
            NomadXML IDList = new NomadXML();
            NucleusRH.Base.MedicinaLaboral.MigCatExa.CAT_EXAMEN objRead;
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
                    objRead = NucleusRH.Base.MedicinaLaboral.MigCatExa.CAT_EXAMEN.Get(row.GetAttr("id"));

                    //Valido atributos obligatorios
                    if (objRead.c_categ_examen == "" || objRead.d_categ_examen == "")
                    {
                        objBatch.Err("No se especificó la Categoría de Examen, se rechaza el registro '" + objRead.c_categ_examen + " - " + objRead.d_categ_examen + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }

                    //Me fijo si ya existe la Especialidad
                    string oiVal = NomadEnvironment.QueryValue("MED11_CATEG_EXAMEN", "oi_categ_examen", "c_categ_examen", objRead.c_categ_examen, "", true);
                    if (oiVal != null)
                    {
                        objBatch.Err("Ya existe un registro para la Categoría de Examen '" + objRead.c_categ_examen + " - " + objRead.d_categ_examen + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }
                    else
                    {
                        //Creo la Categoría de Examen
                        NucleusRH.Base.MedicinaLaboral.CategoriasExamen.CATEG_EXAMEN DDOCATEXA;
                        DDOCATEXA = new NucleusRH.Base.MedicinaLaboral.CategoriasExamen.CATEG_EXAMEN();

                        DDOCATEXA.c_categ_examen = objRead.c_categ_examen;
                        DDOCATEXA.d_categ_examen = objRead.d_categ_examen;

                        //Grabo
                        try
                        {
                            NomadEnvironment.GetCurrentTransaction().Save(DDOCATEXA);
                            NomadEnvironment.QueryValueChange("MED11_CATEG_EXAMEN", "oi_categ_examen", "c_categ_examen", objRead.c_categ_examen, "", "1", true);
                        }
                        catch (Exception e)
                        {
                            objBatch.Err("Error al grabar registro " + objRead.c_categ_examen + " - " + objRead.d_categ_examen + "' - Linea: " + Linea.ToString() + " - " + e.Message);
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

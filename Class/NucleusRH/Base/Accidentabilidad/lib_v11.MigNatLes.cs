using System;
using System.Xml;
using System.IO;
using System.Collections;

using System.Reflection;
using Microsoft.CSharp;
using System.CodeDom.Compiler;

using Nomad.NSystem.Document;
using Nomad.NSystem.Proxy;
using Nomad.NSystem.Base;
using Nomad.NSystem.Functions;

namespace NucleusRH.Base.Accidentabilidad.MigNatLes
{

    //////////////////////////////////////////////////////////////////////////////////
    //Clase Naturaleza de lesiones
    public partial class NATURALEZA_LESION : Nomad.NSystem.Base.NomadObject
    {
        public static void ImportarNatLes()
        {

            int Linea = 0, Errores = 0;

            NomadBatch objBatch;
            objBatch = NomadBatch.GetBatch("Iniciando...", "Importación de Naturaleza de lesiones");

            NomadXML IDCur;
            NomadXML IDList = new NomadXML();
            NucleusRH.Base.Accidentabilidad.MigNatLes.NATURALEZA_LESION objRead;
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
                    objRead = NucleusRH.Base.Accidentabilidad.MigNatLes.NATURALEZA_LESION.Get(row.GetAttr("id"));

                    //Valido atributos obligatorios
                    if (objRead.c_natur_lesion == "" || objRead.d_natur_lesion == "")
                    {
                        objBatch.Err("No se especificó la Naturaleza de la lesión, se rechaza el registro '" + objRead.c_natur_lesion + " - " + objRead.d_natur_lesion + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }

                    //Me fijo si ya existe la Naturaleza de la lesión
                    string oiVal = NomadEnvironment.QueryValue("ACC07_NATUR_LESION", "oi_natur_lesion", "c_natur_lesion", objRead.c_natur_lesion, "", true);
                    if (oiVal != null)
                    {
                        objBatch.Err("Ya existe un registro para la Naturaleza de la lesión '" + objRead.c_natur_lesion + " - " + objRead.d_natur_lesion + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }
                    else
                    {
                        //Creo la Naturaleza de la lesión 
                        NucleusRH.Base.Accidentabilidad.NaturalezaLesiones.NATUR_LESION DDONATLES;
                        DDONATLES = new NucleusRH.Base.Accidentabilidad.NaturalezaLesiones.NATUR_LESION();

                        DDONATLES.c_natur_lesion = objRead.c_natur_lesion;
                        DDONATLES.d_natur_lesion = objRead.d_natur_lesion;

                        //Grabo
                        try
                        {
                            NomadEnvironment.GetCurrentTransaction().Save(DDONATLES);
                            NomadEnvironment.QueryValueChange("ACC07_NATUR_LESION", "oi_natur_lesion", "c_natur_lesion", objRead.c_natur_lesion, "", "1", true);
                        }
                        catch (Exception e)
                        {
                            objBatch.Err("Error al grabar registro " + objRead.c_natur_lesion + " - " + objRead.d_natur_lesion + "' - Linea: " + Linea.ToString() + " - " + e.Message);
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

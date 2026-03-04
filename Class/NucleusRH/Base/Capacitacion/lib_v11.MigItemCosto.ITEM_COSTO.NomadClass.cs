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

namespace NucleusRH.Base.Capacitacion.MigItemCosto
{
    public partial class ITEM_COSTO : Nomad.NSystem.Base.NomadObject
    {
        public static void ImportarItemsCostos()
        {
            int Linea = 0, Errores = 0;

            NomadBatch objBatch;
            objBatch = NomadBatch.GetBatch("Iniciando...", "Importación de Items de Costo");

            NomadXML IDList = new NomadXML();
            NucleusRH.Base.Capacitacion.MigItemCosto.ITEM_COSTO objRead;
            
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
                    objRead = NucleusRH.Base.Capacitacion.MigItemCosto.ITEM_COSTO.Get(row.GetAttr("id"));
                    //Me fijo si ya existe el Item de costo
                    string oiVal = NomadEnvironment.QueryValue("CYD03_ITEMS_COSTO", "oi_item_costo", "c_item_costo", objRead.c_item_costo, "", true);
                    
                    if (oiVal != null)
                    {
                        objBatch.Err("Ya existe un registro para el Items de Costo '" + objRead.c_item_costo + " - " + objRead.d_item_costo + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }
                    else
                    {
                        //Creo el Item de Costo
                        NucleusRH.Base.Capacitacion.ItemsCosto.ITEM_COSTO DDOITEM;
                        DDOITEM = new NucleusRH.Base.Capacitacion.ItemsCosto.ITEM_COSTO();

                        if (objRead.c_item_costo == "" || objRead.d_item_costo == "")
                        {
                            objBatch.Err("El Código o la Descripción del Item de Costo no es válido, se rechaza el registro '" + objRead.c_item_costo + " - " + objRead.d_item_costo + "' - Linea: " + Linea.ToString());
                            Errores++;
                            continue;
                        }

                        DDOITEM.c_item_costo = objRead.c_item_costo;
                        DDOITEM.d_item_costo = objRead.d_item_costo;
                        DDOITEM.n_costo = objRead.n_costo;

                        //Grabo
                        try
                        {
                            NomadEnvironment.GetCurrentTransaction().Save(DDOITEM);
                            NomadEnvironment.QueryValueChange("CYD03_ITEMS_COSTO", "oi_item_costo", "c_item_costo", objRead.c_item_costo, "","1", true);
                        }
                        catch (Exception e)
                        {
                            objBatch.Err("Error al grabar registro " + objRead.c_item_costo + " - " + objRead.d_item_costo + "' - Linea: " + Linea.ToString() + " - " + e.Message);
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

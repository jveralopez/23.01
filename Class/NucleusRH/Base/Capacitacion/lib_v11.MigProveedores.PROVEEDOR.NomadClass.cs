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

namespace NucleusRH.Base.Capacitacion.MigProveedores
{
    public partial class PROVEEDOR : Nomad.NSystem.Base.NomadObject
    {
        public static void ImportarProveedores()
        {
            int Linea = 0, Errores = 0;

            NomadBatch objBatch;
            objBatch = NomadBatch.GetBatch("Iniciando...", "Importación de Proveedores");
                        
            NomadXML IDList = new NomadXML();
            NucleusRH.Base.Capacitacion.MigProveedores.PROVEEDOR objRead;
            
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
                    objRead = NucleusRH.Base.Capacitacion.MigProveedores.PROVEEDOR.Get(row.GetAttr("id"));
                    //Me fijo si ya existe el Proveedor
                    string oiVal = NomadEnvironment.QueryValue("CYD07_PROVEEDORES", "oi_proveedor", "c_proveedor", objRead.c_proveedor, "", true);
                    
                    if (oiVal != null)
                    {
                        objBatch.Err("Ya existe un registro para el Proveedor '" + objRead.c_proveedor + " - " + objRead.d_proveedor + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }
                    else
                    {
                        //Creo el Proveedor   
                        NucleusRH.Base.Capacitacion.Proveedores.PROVEEDOR DDOPRO;
                        DDOPRO = new NucleusRH.Base.Capacitacion.Proveedores.PROVEEDOR();

                        if (objRead.c_proveedor == "" || objRead.d_proveedor == "")
                        {
                            objBatch.Err("El Código o la Descripción del Proveedor no es válido, se rechaza el registro '" + objRead.c_proveedor + " - " + objRead.d_proveedor + "' - Linea: " + Linea.ToString());
                            Errores++;
                            continue;
                        }

                        DDOPRO.c_proveedor = objRead.c_proveedor;
                        DDOPRO.d_proveedor = objRead.d_proveedor;
                        DDOPRO.d_direccion = objRead.d_direccion;
                        DDOPRO.c_nro_cuit = objRead.c_nro_cuit;
                        DDOPRO.te_telefono = objRead.te_telefono;
                        DDOPRO.te_fax = objRead.te_fax;

                        //Grabo
                        try
                        {
                            NomadEnvironment.GetCurrentTransaction().Save(DDOPRO);
                            NomadEnvironment.QueryValueChange("CYD07_PROVEEDORES", "oi_proveedor", "c_proveedor", objRead.c_proveedor, "","1", true);
                        }
                        catch (Exception e)
                        {
                            objBatch.Err("Error al grabar registro " + objRead.c_proveedor + " - " + objRead.d_proveedor + "' - Linea: " + Linea.ToString() + " - " + e.Message);
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

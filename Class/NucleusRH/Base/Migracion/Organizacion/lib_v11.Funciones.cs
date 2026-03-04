using System;
using System.Xml;
using System.IO;
using System.Collections;

using Nomad.NSystem.Document;
using Nomad.NSystem.Proxy;
using Nomad.NSystem.Base;
using Nomad.NSystem.Functions;

namespace NucleusRH.Base.Migracion.Organizacion.Funciones
{

    //////////////////////////////////////////////////////////////////////////////////
    //Clase Ubicaciones Org
    public partial class FUNCION
    {
        public static void ImportarFunciones()
        {

            int Linea = 0, Errores = 0;

            NomadBatch objBatch;
            objBatch = NomadBatch.GetBatch("Iniciando...", "Importación de Funciones");

            Hashtable htPARENTS = new Hashtable();
            NomadXML IDList = new NomadXML();
            NucleusRH.Base.Migracion.Organizacion.Funciones.FUNCION objRead;

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
                    //Cargo el registro
                    objRead = NucleusRH.Base.Migracion.Organizacion.Funciones.FUNCION.Get(row.GetAttr("id"));

                    //Me fijo si llegan los atributos obligatorios
                    if (objRead.c_empresaNull)
                    {
                        objBatch.Err("No se especificó el Codigo de la Empresa, se rechaza el registro '" + objRead.c_funcion + " - " + objRead.d_funcion + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }

                    if (objRead.c_funcion == "")
                    {
                        objBatch.Err("No se especificó el Código de la Funcion, se rechaza el registro '" + objRead.c_funcion + " - " + objRead.d_funcion + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }

                    if (objRead.d_funcion == "")
                    {
                        objBatch.Err("No se especificó la descripci�n de la Funcion, se rechaza el registro '" + objRead.c_funcion + " - " + objRead.d_funcion + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }

                    //Cargo el Padre
                    NucleusRH.Base.Organizacion.Empresas.EMPRESA ddoEMP = NucleusRH.Base.Migracion.Interfaces.INTERFACE.GetEmpresa(objRead.c_empresa, htPARENTS);
                    if (ddoEMP == null)
                    {
                        objBatch.Err("El Código de la Empresa no existe, se rechaza el registro '" + objRead.c_funcion + " - " + objRead.d_funcion + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }

                    //Me fijo si ya existe el Ubicación en la Empresa
                    string oiVal = NomadEnvironment.QueryValue("ORG03_FUNCIONES", "oi_funcion", "c_funcion", objRead.c_funcion, "ORG03_FUNCIONES.oi_empresa = " + ddoEMP.Id, true);
                    if (oiVal != null)
                    {
                        objBatch.Err("Ya existe un registro para la Función '" + objRead.c_funcion + " - " + objRead.d_funcion + "Para la empresa: " + objRead.c_empresa + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }
                    else
                    {
                        //Creo la Funcion
                        NucleusRH.Base.Organizacion.Empresas.FUNCION DDOFUNCION = new NucleusRH.Base.Organizacion.Empresas.FUNCION();

                        DDOFUNCION.c_funcion = objRead.c_funcion;
                        DDOFUNCION.d_funcion = objRead.d_funcion;
                        DDOFUNCION.o_funcion = objRead.o_funcion;
                        DDOFUNCION.o_funcionNull = objRead.o_funcionNull;

                        ddoEMP.FUNCIONES.Add(DDOFUNCION);
                        NomadEnvironment.QueryValueChange("ORG03_FUNCIONES", "oi_funcion", "c_funcion", objRead.c_funcion, "ORG03_FUNCIONES.oi_empresa = " + ddoEMP.Id,"1", true);
                    }
                }
                catch (Exception e)
                {
                    objBatch.Err("Error desconocido en registro, por favor verifique el formato del archivo y la correcta asignación de los campos - Linea: " + Linea.ToString() + " - " + e.Message);
                    Errores++;
                }
            }

            try
            {
                NucleusRH.Base.Migracion.Interfaces.INTERFACE.Grabar(htPARENTS);
            }
            catch (Exception e)
            {
                objBatch.Err("Error al grabar - " + e.Message);
                Errores = Linea;
            }

            objBatch.Log("Registros Procesados: " + Linea.ToString() + " - Importados: " + (Linea - Errores).ToString());
            objBatch.Log("Finalizado...");

        }
    }
}



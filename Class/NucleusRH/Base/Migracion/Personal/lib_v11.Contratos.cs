using System;
using System.Xml;
using System.IO;
using System.Collections;

using Nomad.NSystem.Document;
using Nomad.NSystem.Proxy;
using Nomad.NSystem.Base;
using Nomad.NSystem.Functions;

namespace NucleusRH.Base.Migracion.Personal.Contratos
{

    //////////////////////////////////////////////////////////////////////////////////
    //Clase Contratos
    public partial class CONTRATO 
    {
        public static void ImportarContratos()
        {

            int Linea = 0, Errores = 0;

            NomadBatch objBatch;
            objBatch = NomadBatch.GetBatch("Iniciando...", "Importación de Contratos");
            
            NomadXML IDList = new NomadXML();
            NucleusRH.Base.Migracion.Personal.Contratos.CONTRATO objRead;            

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
                    objRead = NucleusRH.Base.Migracion.Personal.Contratos.CONTRATO.Get(row.GetAttr("id"));

                    //Me fijo si xiste el Tipo de Contrato
                    if (objRead.c_tipo_contrato == "")
                    {
                        objBatch.Err("No se especificó el Tipo de Contrato, se rechaza el registro '" + objRead.c_contrato + " - " + objRead.d_contrato + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }

                    string oiTC = NomadEnvironment.QueryValue("PER28_TIPOS_CONTR", "oi_tipo_contrato", "c_tipo_contrato", objRead.c_tipo_contrato, "", true);
                    if (oiTC == null)
                    {
                        objBatch.Err("El Tipo de Contrato no existe, se rechaza el registro '" + objRead.c_contrato + " - " + objRead.d_contrato + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }

                    //Me fijo si ya existe el Contrato  	
                    string oiVal = NomadEnvironment.QueryValue("PER28_CONTRATOS", "oi_contrato", "c_contrato", objRead.c_contrato, "PER28_CONTRATOS.oi_tipo_contrato = " + oiTC, true);
                    if (oiVal != null)
                    {
                        objBatch.Err("Ya existe un registro para el Contrato en el Tipo de Contrato, se rechaza el registro '" + objRead.c_contrato + " - " + objRead.d_contrato + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }
                    else
                    {
                        //Creo el Contrato
                        NucleusRH.Base.Personal.Contratos.CONTRATO DDOCON;
                        DDOCON = new NucleusRH.Base.Personal.Contratos.CONTRATO();

                        if (objRead.c_contrato == "" || objRead.d_contrato == "")
                        {
                            objBatch.Err("El Código o la Descripción del Contrato no es válido, se rechaza el registro '" + objRead.c_contrato + " - " + objRead.d_contrato + "' - Linea: " + Linea.ToString());
                            Errores++;
                            continue;
                        }

                        DDOCON.c_contrato = objRead.c_contrato;
                        DDOCON.d_contrato = objRead.d_contrato;
                        DDOCON.f_inactivo = objRead.f_inactivo;
                        DDOCON.f_inactivoNull = objRead.f_inactivoNull;
                        DDOCON.oi_tipo_contrato = int.Parse(oiTC);
                        DDOCON.o_contrato = objRead.o_contrato;

                        //Grabo
                        try
                        {
                            NomadEnvironment.GetCurrentTransaction().Save(DDOCON);
                            NomadEnvironment.QueryValueChange("PER28_CONTRATOS", "oi_contrato", "c_contrato", objRead.c_contrato, "PER28_CONTRATOS.oi_tipo_contrato = " + oiTC,"1", true);
                        }
                        catch (Exception e)
                        {
                            objBatch.Err("Error al grabar registro " + objRead.c_contrato + " - " + objRead.d_contrato + "' - Linea: " + Linea.ToString() + " - " + e.Message);
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

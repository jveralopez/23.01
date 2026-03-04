using System;
using System.Xml;
using System.IO;
using System.Collections;

using Nomad.NSystem.Document;
using Nomad.NSystem.Proxy;
using Nomad.NSystem.Base;
using Nomad.NSystem.Functions;

namespace NucleusRH.Base.Migracion.Organizacion.Bandas_Salariales
{

    //////////////////////////////////////////////////////////////////////////////////
    //Clase Bandas Salariales
    public partial class BANDA_SAL
    {
        public static void ImportarBandasSalariales()
        {
            int Linea = 0, Errores = 0;

            NomadBatch objBatch;
            objBatch = NomadBatch.GetBatch("Iniciando...", "Importación de Bandas Salariales");
            
            NomadXML IDList = new NomadXML();
            NucleusRH.Base.Migracion.Organizacion.Bandas_Salariales.BANDA_SAL objRead;            

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
                    objRead = NucleusRH.Base.Migracion.Organizacion.Bandas_Salariales.BANDA_SAL.Get(row.GetAttr("id"));
                    //Me fijo si ya existe la Banda Salarial
                    string oiVal = NomadEnvironment.QueryValue("ORG17_BANDAS_SAL", "oi_banda_sal", "c_banda_sal", objRead.c_banda_sal, "", true);
                    if (oiVal != null)
                    {
                        objBatch.Err("Ya existe un registro para la Banda Salarial '" + objRead.c_banda_sal + " - " + objRead.d_banda_sal + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }
                    else
                    {
                        //Creo la Banda Salarial
                        NucleusRH.Base.Organizacion.Bandas_Salariales.BANDA_SAL DDOBS;
                        DDOBS = new NucleusRH.Base.Organizacion.Bandas_Salariales.BANDA_SAL();

                        if (objRead.c_banda_sal == "" || objRead.d_banda_sal == "")
                        {
                            objBatch.Err("El Código o la Descripción de la Banda Salarial no es válido, se rechaza el registro '" + objRead.c_banda_sal + " - " + objRead.d_banda_sal + "' - Linea: " + Linea.ToString());
                            Errores++;
                            continue;
                        }

                        DDOBS.c_banda_sal = objRead.c_banda_sal;
                        DDOBS.d_banda_sal = objRead.d_banda_sal;
                        DDOBS.e_orden = objRead.e_orden;
                        DDOBS.n_sal_maximo = objRead.n_sal_maximo;
                        DDOBS.n_sal_minimo = objRead.n_sal_minimo;
                        DDOBS.o_banda_sal = objRead.o_banda_sal;

                        //Grabo
                        try
                        {
                            NomadEnvironment.GetCurrentTransaction().Save(DDOBS);
                            NomadEnvironment.QueryValueChange("ORG17_BANDAS_SAL", "oi_banda_sal", "c_banda_sal", objRead.c_banda_sal, "","1", true);
                        }
                        catch (Exception e)
                        {
                            objBatch.Err("Error al grabar registro " + objRead.c_banda_sal + " - " + objRead.d_banda_sal + "' - Linea: " + Linea.ToString() + " - " + e.Message);
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

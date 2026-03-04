using System;
using System.Xml;
using System.IO;
using System.Collections;

using Nomad.NSystem.Document;
using Nomad.NSystem.Proxy;
using Nomad.NSystem.Base;
using Nomad.NSystem.Functions;

namespace NucleusRH.Base.Migracion.Organizacion.Niveles_Salariales
{

    //////////////////////////////////////////////////////////////////////////////////
    //Clase Niveles Salariales por Banda
    public partial class NIVEL_SAL 
    {
        public static void ImportarNivelesSalariales()
        {

            int Linea = 0, Errores = 0;

            NomadBatch objBatch;
            objBatch = NomadBatch.GetBatch("Iniciando...", "Importación de Bandas Salariales");

            Hashtable htPARENTS = new Hashtable();            
            NomadXML IDList = new NomadXML();
            NucleusRH.Base.Migracion.Organizacion.Niveles_Salariales.NIVEL_SAL objRead;         

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
                    objRead = NucleusRH.Base.Migracion.Organizacion.Niveles_Salariales.NIVEL_SAL.Get(row.GetAttr("id"));

                    if (objRead.c_nivel_sal == "" || objRead.d_nivel_sal == "")
                    {
                        objBatch.Err("El Código o la Descripción de la Banda Salarial no es válido, se rechaza el registro '" + objRead.c_nivel_sal + " - " + objRead.d_nivel_sal + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }

                    //Cargo el Padre  	
                    NucleusRH.Base.Organizacion.Bandas_Salariales.BANDA_SAL ddoBS = NucleusRH.Base.Migracion.Interfaces.INTERFACE.GetBandaSalarial(objRead.c_banda_sal, htPARENTS);
                    if (ddoBS == null)
                    {
                        objBatch.Err("El Código de Banda Salarial no existe, se rechaza el registro '" + objRead.c_nivel_sal + " - " + objRead.d_nivel_sal + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }

                    //Me fijo si ya existe el Nivel Salarial en la Banda Salarial  	
                    string oiVal = NomadEnvironment.QueryValue("ORG17_NIVELES_SAL", "oi_nivel_sal", "c_nivel_sal", objRead.c_nivel_sal, "ORG17_NIVELES_SAL.oi_banda_sal = " + ddoBS.Id, true);
                    if (oiVal != null)
                    {
                        objBatch.Err("Ya existe un registro para el Nivel Salarial '" + objRead.c_nivel_sal + " - " + objRead.d_nivel_sal + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }
                    else
                    {
                        //Creo el Nivel Salarial			
                        NucleusRH.Base.Organizacion.Bandas_Salariales.NIVEL_SAL DDONIVEL_SAL = new NucleusRH.Base.Organizacion.Bandas_Salariales.NIVEL_SAL();

                        DDONIVEL_SAL.c_nivel_sal = objRead.c_nivel_sal;
                        DDONIVEL_SAL.d_nivel_sal = objRead.d_nivel_sal;
                        DDONIVEL_SAL.e_orden = objRead.e_orden;
                        DDONIVEL_SAL.e_ordenNull = objRead.e_ordenNull;
                        DDONIVEL_SAL.n_sal_maximo = objRead.n_sal_maximo;
                        DDONIVEL_SAL.n_sal_maximoNull = objRead.n_sal_maximoNull;
                        DDONIVEL_SAL.n_sal_minimo = objRead.n_sal_minimo;
                        DDONIVEL_SAL.n_sal_minimoNull = objRead.n_sal_minimoNull;
                        DDONIVEL_SAL.o_nivel_sal = objRead.o_nivel_sal;

                        ddoBS.NIVELES_SAL.Add(DDONIVEL_SAL);
                        NomadEnvironment.QueryValueChange("ORG17_NIVELES_SAL", "oi_nivel_sal", "c_nivel_sal", objRead.c_nivel_sal, "ORG17_NIVELES_SAL.oi_banda_sal = " + ddoBS.Id,"1", true);
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

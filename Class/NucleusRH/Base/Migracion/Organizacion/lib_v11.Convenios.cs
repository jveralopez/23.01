using System;
using System.Xml;
using System.IO;
using System.Collections;

using Nomad.NSystem.Document;
using Nomad.NSystem.Proxy;
using Nomad.NSystem.Base;
using Nomad.NSystem.Functions;

namespace NucleusRH.Base.Migracion.Organizacion.Convenios
{

    //////////////////////////////////////////////////////////////////////////////////
    //Clase Convenios
    public partial class CONVENIO
    {
        public static void ImportarConvenios()
        {
            int Linea = 0, Errores = 0;

            NomadBatch objBatch;
            objBatch = NomadBatch.GetBatch("Iniciando...", "Importación de Convenios");
            
            NomadXML IDList = new NomadXML();
            NucleusRH.Base.Migracion.Organizacion.Convenios.CONVENIO objRead;            

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
                    objRead = NucleusRH.Base.Migracion.Organizacion.Convenios.CONVENIO.Get(row.GetAttr("id"));
                    //Me fijo si ya existe el Convenio  	
                    string oiVal = NomadEnvironment.QueryValue("ORG18_CONVENIOS", "oi_convenio", "c_convenio", objRead.c_convenio, "", true);

                    if (oiVal != null)
                    {
                        objBatch.Err("Ya existe un registro para el Convenio '" + objRead.c_convenio + " - " + objRead.d_convenio + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }
                    else
                    {
                        //Creo el Convenio
                        NucleusRH.Base.Organizacion.Convenios.CONVENIO DDOCONVENIO;
                        DDOCONVENIO = new NucleusRH.Base.Organizacion.Convenios.CONVENIO();

                        if (objRead.c_convenio == "" || objRead.d_convenio == "")
                        {
                            objBatch.Err("El Código o la Descripción del Convenio no es válido, se rechaza el registro '" + objRead.c_convenio + " - " + objRead.d_convenio + "' - Linea: " + Linea.ToString());
                            Errores++;
                            continue;
                        }

                        DDOCONVENIO.c_convenio = objRead.c_convenio;
                        DDOCONVENIO.d_convenio = objRead.d_convenio;
                        DDOCONVENIO.e_min_dias_ant_vac = objRead.e_min_dias_ant_vac;
                        if (objRead.e_min_dias_ant_vacNull) DDOCONVENIO.e_min_dias_ant_vacNull = true;
                        DDOCONVENIO.e_min_dias_trab = objRead.e_min_dias_trab;
                        if (objRead.e_min_dias_trabNull) DDOCONVENIO.e_min_dias_trabNull = true;
                        DDOCONVENIO.o_convenio = objRead.o_convenio;

                        //Grabo
                        try
                        {
                            NomadEnvironment.GetCurrentTransaction().Save(DDOCONVENIO);
                            NomadEnvironment.QueryValueChange("ORG18_CONVENIOS", "oi_convenio", "c_convenio", objRead.c_convenio, "","1", true);
                        }
                        catch (Exception e)
                        {
                            objBatch.Err("Error al grabar registro " + objRead.c_convenio + " - " + objRead.d_convenio + "' - Linea: " + Linea.ToString() + " - " + e.Message);
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

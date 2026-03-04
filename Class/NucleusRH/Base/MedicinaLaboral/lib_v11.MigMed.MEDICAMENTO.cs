using System;
using System.Xml;
using System.IO;
using System.Collections;

using Nomad.NSystem.Document;
using Nomad.NSystem.Proxy;
using Nomad.NSystem.Base;
using Nomad.NSystem.Functions;

namespace NucleusRH.Base.MedicinaLaboral.MigMed
{

    //////////////////////////////////////////////////////////////////////////////////
    //Clase Medicamentos
    public partial class MEDICAMENTO 
    {
        public static void ImportarMed()
        {

            int Linea = 0, Errores = 0;

            NomadBatch objBatch;
            objBatch = NomadBatch.GetBatch("Iniciando...", "Importación de Medicamentos");

            NomadXML IDCur;
            NomadXML IDList = new NomadXML();
            NucleusRH.Base.MedicinaLaboral.MigMed.MEDICAMENTO objRead;
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
                    objRead = NucleusRH.Base.MedicinaLaboral.MigMed.MEDICAMENTO.Get(row.GetAttr("id"));

                    //Valido atributos obligatorios
                    if (objRead.c_medicamento == "" || objRead.d_medicamento == "")
                    {
                        objBatch.Err("No se especificó el Medicamento, se rechaza el registro '" + objRead.c_medicamento + " - " + objRead.d_medicamento + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }

                    //Me fijo si ya existe el Medicamento
                    string oiVal = NomadEnvironment.QueryValue("MED08_MEDICAMENTOS", "oi_medicamento", "c_medicamento", objRead.c_medicamento, "", true);
                    if (oiVal != null)
                    {
                        objBatch.Err("Ya existe un registro para el Medicamento'" + objRead.c_medicamento + " - " + objRead.d_medicamento + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }
                    else
                    {
                        //Creo el Medicamento
                        NucleusRH.Base.MedicinaLaboral.Medicamentos.MEDICAMENTO DDOMED;
                        DDOMED = new NucleusRH.Base.MedicinaLaboral.Medicamentos.MEDICAMENTO();

                        DDOMED.c_medicamento = objRead.c_medicamento;
                        DDOMED.d_medicamento = objRead.d_medicamento;
                        DDOMED.o_medicamento = objRead.o_medicamento;

                        //Grabo
                        try
                        {
                            NomadEnvironment.GetCurrentTransaction().Save(DDOMED);
                            NomadEnvironment.QueryValueChange("MED08_MEDICAMENTOS", "oi_medicamento", "c_medicamento", objRead.c_medicamento, "", "1",true);
                        }
                        catch (Exception e)
                        {
                            objBatch.Err("Error al grabar registro " + objRead.c_medicamento + " - " + objRead.d_medicamento + "' - Linea: " + Linea.ToString() + " - " + e.Message);
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

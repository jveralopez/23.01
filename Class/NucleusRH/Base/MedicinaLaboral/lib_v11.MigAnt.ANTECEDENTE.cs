using System;
using System.Xml;
using System.IO;
using System.Collections;

using Nomad.NSystem.Document;
using Nomad.NSystem.Proxy;
using Nomad.NSystem.Base;
using Nomad.NSystem.Functions;

namespace NucleusRH.Base.MedicinaLaboral.MigAnt
{

    //////////////////////////////////////////////////////////////////////////////////
    //Clase Antecedentes
    public partial class ANTECEDENTE 
    {
        public static void ImportarAnt()
        {

            int Linea = 0, Errores = 0;

            NomadBatch objBatch;
            objBatch = NomadBatch.GetBatch("Iniciando...", "Importación de Antecedentes");

            NomadXML IDCur;
            NomadXML IDList = new NomadXML();
            NucleusRH.Base.MedicinaLaboral.MigAnt.ANTECEDENTE objRead;
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
                    objRead = NucleusRH.Base.MedicinaLaboral.MigAnt.ANTECEDENTE.Get(row.GetAttr("id"));

                    //Valido atributos obligatorios
                    if (objRead.c_antecedente == "" || objRead.d_antecedente == "")
                    {
                        objBatch.Err("No se especificó el Antecedente, se rechaza el registro '" + objRead.c_antecedente + " - " + objRead.d_antecedente + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }
                    if (objRead.c_tipo_antec == "")
                    {
                        objBatch.Err("No se especificó el Tipo de antecedente, se rechaza el registro '" + objRead.c_antecedente + " - " + objRead.d_antecedente + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }

                    //Recupero los OI de los c&#243;digos ingresados
                    string oiTIPANT = "";

                    //Recupero el tipo de antecedente
                    oiTIPANT = NomadEnvironment.QueryValue("MED12_TIPOS_ANTEC", "oi_tipo_antec", "c_tipo_antec", objRead.c_tipo_antec, "", true);
                    if (oiTIPANT == null)
                    {
                        objBatch.Err("El tipo de antecedente no existe, se rechaza el registro '" + objRead.c_antecedente + " - " + objRead.d_antecedente + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }

                    //Me fijo si ya existe el Antecedente
                    string oiVal = NomadEnvironment.QueryValue("MED12_ANTECEDENTES", "oi_antecedente", "c_antecedente", objRead.c_antecedente, "", true);
                    if (oiVal != null)
                    {
                        objBatch.Err("Ya existe un registro para el Antecedente '" + objRead.c_antecedente + " - " + objRead.d_antecedente + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }
                    else
                    {
                        //Creo el Antecedente
                        NucleusRH.Base.MedicinaLaboral.TiposAntecedentes.ANTECEDENTE DDOANT;
                        DDOANT = new NucleusRH.Base.MedicinaLaboral.TiposAntecedentes.ANTECEDENTE();

                        DDOANT.oi_tipo_antec = int.Parse(oiTIPANT);
                        DDOANT.c_antecedente = objRead.c_antecedente;
                        DDOANT.d_antecedente = objRead.d_antecedente;
                        DDOANT.o_antecedente = objRead.o_antecedente;

                        //Grabo
                        try
                        {
                            NomadEnvironment.GetCurrentTransaction().Save(DDOANT);
                            NomadEnvironment.QueryValueChange("MED12_ANTECEDENTES", "oi_antecedente", "c_antecedente", objRead.c_antecedente, "", "1", true);
                        }
                        catch (Exception e)
                        {
                            objBatch.Err("Error al grabar registro " + objRead.c_antecedente + " - " + objRead.d_antecedente + "' - Linea: " + Linea.ToString() + " - " + e.Message);
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

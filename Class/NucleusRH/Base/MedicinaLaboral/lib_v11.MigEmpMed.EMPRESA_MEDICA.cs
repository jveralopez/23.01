using System;
using System.Xml;
using System.IO;
using System.Collections;

using Nomad.NSystem.Document;
using Nomad.NSystem.Proxy;
using Nomad.NSystem.Base;
using Nomad.NSystem.Functions;

namespace NucleusRH.Base.MedicinaLaboral.MigEmpMed
{

    //////////////////////////////////////////////////////////////////////////////////
    //Clase Empresas Médicas
    public partial class EMPRESA_MEDICA
    {
        public static void ImportarEmpMed()
        {

            int Linea = 0, Errores = 0;

            NomadBatch objBatch;
            objBatch = NomadBatch.GetBatch("Iniciando...", "Importación de Empresas Médicas");

            NomadXML IDCur;
            NomadXML IDList = new NomadXML();
            NucleusRH.Base.MedicinaLaboral.MigEmpMed.EMPRESA_MEDICA objRead;
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
                    objRead = NucleusRH.Base.MedicinaLaboral.MigEmpMed.EMPRESA_MEDICA.Get(row.GetAttr("id"));

                    //Valido atributos obligatorios
                    if (objRead.c_empresa_medica == "" || objRead.d_empresa_medica == "")
                    {
                        objBatch.Err("No se especificó la Empresa Médica, se rechaza el registro '" + objRead.c_empresa_medica + " - " + objRead.d_empresa_medica + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }

                    if (objRead.c_localidad == "")
                    {
                        objBatch.Err("No se especificó la localidad de la empresa, se rechaza el registro '" + objRead.c_empresa_medica + " - " + objRead.d_empresa_medica + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }

                    //Recupero los OI de los códigos ingresados
                    string oiLOC = "";

                    //Recupero la empresa
                    oiLOC = NomadEnvironment.QueryValue("ORG19_LOCALIDADES", "oi_localidad", "c_localidad", objRead.c_localidad, "", true);
                    if (oiLOC == null)
                    {
                        objBatch.Err("La localidad no existe, se rechaza el registro '" + objRead.c_empresa_medica + " - " + objRead.d_empresa_medica + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }

                    //Me fijo si ya existe la Empresa Médica
                    string oiVal = NomadEnvironment.QueryValue("MED02_EMPRESAS", "oi_empresa_medica", "c_empresa_medica", objRead.c_empresa_medica, "", true);
                    if (oiVal != null)
                    {
                        objBatch.Err("Ya existe un registro para la Empresa Médica '" + objRead.c_empresa_medica + " - " + objRead.d_empresa_medica + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }
                    else
                    {
                        //Creo la Empresa Médica
                        NucleusRH.Base.MedicinaLaboral.EmpresasMedicas.EMPRESA_MEDICA DDOEMPMED;
                        DDOEMPMED = new NucleusRH.Base.MedicinaLaboral.EmpresasMedicas.EMPRESA_MEDICA();

                        DDOEMPMED.c_empresa_medica = objRead.c_empresa_medica;
                        DDOEMPMED.d_empresa_medica = objRead.d_empresa_medica;
                        DDOEMPMED.oi_localidad = oiLOC;
                        DDOEMPMED.te_empresa = objRead.te_empresa;
                        DDOEMPMED.te_fax = objRead.te_fax;
                        DDOEMPMED.c_nro_cuit = objRead.c_nro_cuit;
                        DDOEMPMED.d_cargo_resp = objRead.d_cargo_resp;
                        DDOEMPMED.d_domicilio = objRead.d_domicilio;
                        DDOEMPMED.d_responsable = objRead.d_responsable;

                        //Grabo
                        try
                        {
                            NomadEnvironment.GetCurrentTransaction().Save(DDOEMPMED);
                            NomadEnvironment.QueryValueChange("MED01_ESPECIALIDAD", "oi_especialidad", "c_empresa_medica", objRead.c_empresa_medica, "", "1", true);
                        }
                        catch (Exception e)
                        {
                            objBatch.Err("Error al grabar registro " + objRead.c_empresa_medica + " - " + objRead.d_empresa_medica + "' - Linea: " + Linea.ToString() + " - " + e.Message);
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


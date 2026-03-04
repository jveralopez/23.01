using System;
using System.Xml;
using System.IO;
using System.Collections;

using Nomad.NSystem.Document;
using Nomad.NSystem.Proxy;
using Nomad.NSystem.Base;
using Nomad.NSystem.Functions;

namespace NucleusRH.Base.MedicinaLaboral.MigMedicos
{

    //////////////////////////////////////////////////////////////////////////////////
    //Clase Médicos
    public partial class MEDICO 
    {
        public static void ImportarMedicos()
        {

            int Linea = 0, Errores = 0;

            NomadBatch objBatch;
            objBatch = NomadBatch.GetBatch("Iniciando...", "Importación de Médicos");

            NomadXML IDCur;
            NomadXML IDList = new NomadXML();
            NucleusRH.Base.MedicinaLaboral.MigMedicos.MEDICO objRead;
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
                    objRead = NucleusRH.Base.MedicinaLaboral.MigMedicos.MEDICO.Get(row.GetAttr("id"));

                    //Valido atributos obligatorios
                    if (objRead.c_tipo_documento == "")
                    {
                        objBatch.Err("No se especificó el tipo de documento, se rechaza el registro '" + objRead.c_tipo_documento + " - " + objRead.c_nro_documento + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }

                    if (objRead.c_nro_documento == "")
                    {
                        objBatch.Err("No se especificó el número de documento, se rechaza el registro '" + objRead.c_tipo_documento + " - " + objRead.c_nro_documento + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }
                    if (objRead.d_apellido == "")
                    {
                        objBatch.Err("No se especificó el apellido, se rechaza el registro '" + objRead.c_tipo_documento + " - " + objRead.c_nro_documento + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }
                    if (objRead.d_nombres == "")
                    {
                        objBatch.Err("No se especificó el nombre, se rechaza el registro '" + objRead.c_tipo_documento + " - " + objRead.c_nro_documento + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }
                    if (objRead.c_especialidad == "")
                    {
                        objBatch.Err("No se especificó la especialidad, se rechaza el registro '" + objRead.c_tipo_documento + " - " + objRead.c_nro_documento + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }

                    //Recupero los OI de los c&#243;digos ingresados
                    string oiTIPO = "", oiLOC = "", oiESP = "", oiEMP = "", oiPER = "";

                    //Recupero el tipo de documento
                    oiTIPO = NomadEnvironment.QueryValue("PER20_TIPOS_DOC", "oi_tipo_documento", "c_tipo_documento", objRead.c_tipo_documento, "", true);
                    if (oiTIPO == null)
                    {
                        objBatch.Err("El tipo de documento no existe, se rechaza el registro '" + objRead.c_tipo_documento + " - " + objRead.c_nro_documento + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }
                    //Recupero la localidad
                    oiLOC = NomadEnvironment.QueryValue("ORG19_LOCALIDADES", "oi_localidad", "c_localidad", objRead.c_localidad, "", true);
                    if (oiLOC == null)
                    {
                        objBatch.Err("La localidad no existe, se rechaza el registro '" + objRead.c_tipo_documento + " - " + objRead.c_nro_documento + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }
                    //Recupero la especialidad
                    oiESP = NomadEnvironment.QueryValue("MED01_ESPECIALIDAD", "oi_especialidad", "c_especialidad", objRead.c_especialidad, "", true);
                    if (oiESP == null)
                    {
                        objBatch.Err("La especialidad no existe, se rechaza el registro '" + objRead.c_tipo_documento + " - " + objRead.c_nro_documento + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }

                    //Recupero la empresa medica
                    oiEMP = NomadEnvironment.QueryValue("MED02_EMPRESAS", "oi_empresa_medica", "c_empresa_medica", objRead.c_empresa_medica, "", true);
                    if (oiEMP == null)
                    {
                        objBatch.Err("La empresa medica no existe, se rechaza el registro '" + objRead.c_tipo_documento + " - " + objRead.c_nro_documento + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }

                    //Me fijo si ya existe el medico
                    oiPER = NomadEnvironment.QueryValue("PER01_PERSONAL", "oi_personal", "c_personal", objRead.c_tipo_documento + objRead.c_nro_documento, "", true);
                    if (oiPER != null)
                    {
                        objBatch.Err("Ya existe un registro para el médico'" + objRead.c_tipo_documento + " - " + objRead.c_nro_documento + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }
                    else
                    {
                        //Creo el medico
                        NucleusRH.Base.MedicinaLaboral.Medicos.MEDICO DDOMED;
                        DDOMED = new NucleusRH.Base.MedicinaLaboral.Medicos.MEDICO();

                        DDOMED.oi_tipo_documento = oiTIPO;
                        DDOMED.oi_empresa_medica = oiEMP;
                        DDOMED.oi_especialidad = oiESP;
                        DDOMED.oi_localidad = oiLOC;
                        DDOMED.c_nro_documento = objRead.c_nro_documento;
                        DDOMED.c_nro_matricula = objRead.c_nro_matricula;
                        DDOMED.d_apellido = objRead.d_apellido;
                        DDOMED.d_nombres = objRead.d_nombres;
                        DDOMED.d_domicilio = objRead.d_domicilio;
                        DDOMED.te_particular = objRead.te_particular;


                        //Grabo
                        try
                        {
                            NomadEnvironment.GetCurrentTransaction().Save(DDOMED);
                            NomadEnvironment.QueryValueChange("PER01_PERSONAL", "oi_personal", "c_personal", objRead.c_tipo_documento + objRead.c_nro_documento, "", "1",true);
                        }
                        catch (Exception e)
                        {
                            objBatch.Err("Error al grabar registro " + objRead.c_tipo_documento + " - " + objRead.c_nro_documento + "' - Linea: " + Linea.ToString() + " - " + e.Message);
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

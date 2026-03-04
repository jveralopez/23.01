using System;
using System.Xml;
using System.IO;
using System.Collections;

using Nomad.NSystem.Document;
using Nomad.NSystem.Proxy;
using Nomad.NSystem.Base;
using Nomad.NSystem.Functions;

namespace NucleusRH.Base.Migracion.Personal.Domicilios
{

    //////////////////////////////////////////////////////////////////////////////////
    //Clase Domicilios Personal
    public partial class DOMIC_PER 
    {
        public static void ImportarDomicilios()
        {

            int Linea = 0, Errores = 0;
            string oiPERANT = "";

            NomadBatch objBatch;
            objBatch = NomadBatch.GetBatch("Iniciando...", "Importación de Domicilios");

            Hashtable htPARENTS = new Hashtable();            
            NomadXML IDList = new NomadXML();
            NucleusRH.Base.Migracion.Personal.Domicilios.DOMIC_PER objRead;            

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
                    objRead = NucleusRH.Base.Migracion.Personal.Domicilios.DOMIC_PER.Get(row.GetAttr("id"));
                    if (objRead.c_persona == "")
                    {
                        objBatch.Err("No se especificó la Persona, se rechaza el registro '" + objRead.c_tipo_domicilio + " - " + objRead.d_calle + ", " + objRead.e_numero + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }
                    if (objRead.c_tipo_domicilio == "")
                    {
                        objBatch.Err("No se especificó el Tipo de Domicilio, se rechaza el registro '" + objRead.c_tipo_domicilio + " - " + objRead.d_calle + ", " + objRead.e_numero + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }
                    if (objRead.d_calle == "")
                    {
                        objBatch.Err("No se especificó la Calle, se rechaza el registro '" + objRead.c_tipo_domicilio + " - " + objRead.d_calle + ", " + objRead.e_numero + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }
                    if (objRead.e_numeroNull)
                    {
                        objBatch.Err("No se especificó el Número, se rechaza el registro '" + objRead.c_tipo_domicilio + " - " + objRead.d_calle + ", " + objRead.e_numero + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }

                    //Recupero los OI de los códigos ingresados
                    string oiPER = "", oiTIPODOM = "", oiLOC = ""; ;

                    if (objRead.c_persona != "")
                    {
                        oiPER = NomadEnvironment.QueryValue("PER01_PERSONAL", "oi_personal", "c_personal", objRead.c_persona, "", true);
                        if (oiPER == null)
                        {
                            objBatch.Err("La Persona no existe, se rechaza el registro '" + objRead.c_tipo_domicilio + " - " + objRead.d_calle + ", " + objRead.e_numero + "' - Linea: " + Linea.ToString());
                            Errores++;
                            continue;
                        }
                    }

                    if (objRead.c_tipo_domicilio != "")
                    {
                        oiTIPODOM = NomadEnvironment.QueryValue("PER09_TIPOS_DOMIC", "oi_tipo_domicilio", "c_tipo_domicilio", objRead.c_tipo_domicilio, "", true);
                        if (oiTIPODOM == null)
                        {
                            objBatch.Err("El Tipo de Domicilio no existe, se rechaza el registro '" + objRead.c_tipo_domicilio + " - " + objRead.d_calle + ", " + objRead.e_numero + "' - Linea: " + Linea.ToString());
                            Errores++;
                            continue;
                        }
                    }

                    if (objRead.c_localidad != "")
                    {
                        oiLOC = NomadEnvironment.QueryValue("ORG19_LOCALIDADES", "oi_localidad", "c_localidad", objRead.c_localidad, "", true);
                        if (oiLOC == null)
                        {
                            objBatch.Err("La Localidad no existe, se rechaza el registro '" + objRead.c_tipo_domicilio + " - " + objRead.d_calle + ", " + objRead.e_numero + "' - Linea: " + Linea.ToString());
                            Errores++;
                            continue;
                        }
                    }

                    //Cargo el Padre  	
                    NucleusRH.Base.Personal.Legajo.PERSONAL DDOPER = NucleusRH.Base.Migracion.Interfaces.INTERFACE.GetPersona(objRead.c_persona, htPARENTS);
                    if (DDOPER == null)
                    {
                        objBatch.Err("La Persona no existe, se rechaza el registro '" + objRead.c_tipo_domicilio + " - " + objRead.d_calle + ", " + objRead.e_numero + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }

                    //Me fijo si ya existe el Tipo de Domicilio en la persona  	  	  	  	
                    if (DDOPER.DOMIC_PER.GetByAttribute("oi_tipo_domicilio", oiTIPODOM) != null)
                    {
                        objBatch.Err("Ya existe un registro para el Tipo de Domicilio en la Persona, se rechaza el registro '" + objRead.c_tipo_domicilio + " - " + objRead.d_calle + ", " + objRead.e_numero + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }

                    //Creo el Domicilio
                    NucleusRH.Base.Personal.Legajo.DOMIC_PER DDODOMPER;
                    DDODOMPER = new NucleusRH.Base.Personal.Legajo.DOMIC_PER();

                    DDODOMPER.c_postal = objRead.c_postal;
                    DDODOMPER.d_calle = objRead.d_calle;
                    DDODOMPER.d_departamento = objRead.d_departamento;
                    DDODOMPER.d_email = objRead.d_email;
                    DDODOMPER.d_entre_calle_1 = objRead.d_entre_calle_1;
                    DDODOMPER.d_entre_calle_2 = objRead.d_entre_calle_2;
                    DDODOMPER.d_partido = objRead.d_partido;
                    DDODOMPER.d_piso = objRead.d_piso;
                    DDODOMPER.e_numero = objRead.e_numero;
                    DDODOMPER.l_domic_fiscal = objRead.l_domic_fiscal;
                    DDODOMPER.o_domicilio = objRead.o_domicilio;
                    if (oiLOC != "") DDODOMPER.oi_localidad = oiLOC;
                    if (oiTIPODOM != "") DDODOMPER.oi_tipo_domicilio = oiTIPODOM;
                    DDODOMPER.te_1 = objRead.te_1;
                    DDODOMPER.te_2 = objRead.te_2;
                    DDODOMPER.te_celular = objRead.te_celular;

                    //Agrego el Domicilio
                    DDOPER.DOMIC_PER.Add(DDODOMPER);
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

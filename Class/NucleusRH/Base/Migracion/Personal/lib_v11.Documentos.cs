using System;
using System.Xml;
using System.IO;
using System.Collections;

using Nomad.NSystem.Document;
using Nomad.NSystem.Proxy;
using Nomad.NSystem.Base;
using Nomad.NSystem.Functions;

namespace NucleusRH.Base.Migracion.Personal.Documentos
{

    //////////////////////////////////////////////////////////////////////////////////
    //Clase Documentos Personal
    public partial class DOCUMENTO_PER 
    {
        public static void ImportarDocumentos()
        {

            int Linea = 0, Errores = 0;
            string oiPERANT = "";

            NomadBatch objBatch;
            objBatch = NomadBatch.GetBatch("Iniciando...", "Importación de Documentos");

            Hashtable htPARENTS = new Hashtable();            
            NomadXML IDList = new NomadXML();
            NucleusRH.Base.Migracion.Personal.Documentos.DOCUMENTO_PER objRead;            

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
                    objRead = NucleusRH.Base.Migracion.Personal.Documentos.DOCUMENTO_PER.Get(row.GetAttr("id"));

                    if (objRead.c_persona == "")
                    {
                        objBatch.Err("No se especificó la Persona, se rechaza el registro '" + objRead.c_tipo_documento + " - " + objRead.c_documento + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }
                    if (objRead.c_tipo_documento == "")
                    {
                        objBatch.Err("No se especificó el Tipo de Documento, se rechaza el registro '" + objRead.c_tipo_documento + " - " + objRead.c_documento + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }
                    if (objRead.c_documento == "")
                    {
                        objBatch.Err("No se especificó el Número de Documento, se rechaza el registro '" + objRead.c_tipo_documento + " - " + objRead.c_documento + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }

                    //Recupero los OI de los códigos ingresados
                    string oiPER = "", oiTIPODOC = "", oiPROV = ""; ;
                    if (objRead.c_tipo_documento != "")
                    {
                        oiTIPODOC = NomadEnvironment.QueryValue("PER20_TIPOS_DOC", "oi_tipo_documento", "c_tipo_documento", objRead.c_tipo_documento, "", true);
                        if (oiTIPODOC == null)
                        {
                            objBatch.Err("El Tipo de Documento no existe, se rechaza el registro '" + objRead.c_tipo_documento + " - " + objRead.c_documento + "' - Linea: " + Linea.ToString());
                            Errores++;
                            continue;
                        }
                    }

                    if (objRead.c_provincia != "")
                    {
                        oiPROV = NomadEnvironment.QueryValue("ORG35_PROVINCIAS", "oi_provincia", "c_provincia", objRead.c_provincia, "", true);
                        if (oiPROV == null)
                        {
                            objBatch.Err("La Provincia no existe, se rechaza el registro '" + objRead.c_tipo_documento + " - " + objRead.c_documento + "' - Linea: " + Linea.ToString());
                            Errores++;
                            continue;
                        }
                    }

                    //Cargo el Padre  	
                    NucleusRH.Base.Personal.Legajo.PERSONAL ddoPER = NucleusRH.Base.Migracion.Interfaces.INTERFACE.GetPersona(objRead.c_persona, htPARENTS);
                    if (ddoPER == null)
                    {
                        objBatch.Err("La Persona no existe, se rechaza el registro '" + objRead.c_tipo_documento + " - " + objRead.c_documento + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }

                    //Me fijo si ya existe el Documento en la persona  	  	  	  	
                    if (ddoPER.DOCUM_PER.GetByAttribute("oi_tipo_documento", oiTIPODOC) != null)
                    {
                        objBatch.Err("Ya existe un registro para el Tipo de Documento en la Persona, se rechaza el registro '" + objRead.c_tipo_documento + " - " + objRead.c_documento + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }

                    //Creo el documento
                    NucleusRH.Base.Personal.Legajo.DOCUMENTO_PER DDODOCPER;
                    DDODOCPER = new NucleusRH.Base.Personal.Legajo.DOCUMENTO_PER();

                    DDODOCPER.c_documento = objRead.c_documento;
                    DDODOCPER.d_origen_documento = objRead.d_origen_documento;
                    DDODOCPER.d_resp_expedicion = objRead.d_resp_expedicion;
                    DDODOCPER.f_vencimiento_doc = objRead.f_vencimiento_doc;
                    DDODOCPER.f_vencimiento_docNull = objRead.f_vencimiento_docNull;
                    DDODOCPER.f_entrega = objRead.f_entrega;
                    DDODOCPER.f_entregaNull = objRead.f_entregaNull;
                    DDODOCPER.o_documento_per = objRead.o_documento_per;
                    if (oiPROV != "") DDODOCPER.oi_provincia = oiPROV;
                    if (oiTIPODOC != "") DDODOCPER.oi_tipo_documento = oiTIPODOC;

                    //Agrego el Documento
                    ddoPER.DOCUM_PER.Add(DDODOCPER);
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



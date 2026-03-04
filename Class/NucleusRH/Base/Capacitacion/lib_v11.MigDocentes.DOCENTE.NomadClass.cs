using System;
using System.Xml;
using System.IO;
using System.Collections;

using System.Reflection;
using Microsoft.CSharp;
using System.CodeDom.Compiler;

using Nomad.NSystem.Document;
using Nomad.NSystem.Proxy;
using Nomad.NSystem.Base;
using Nomad.NSystem.Functions;

namespace NucleusRH.Base.Capacitacion.MigDocentes
{
    public partial class DOCENTE : Nomad.NSystem.Base.NomadObject
    {
        public static void ImportarDocentes()
        {
            int Linea = 0, Errores = 0;

            NomadBatch objBatch;
            objBatch = NomadBatch.GetBatch("Iniciando...", "Importación de Docentes");

            NomadXML IDList = new NomadXML();
            NucleusRH.Base.Capacitacion.MigDocentes.DOCENTE objRead;

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
                    objRead = NucleusRH.Base.Capacitacion.MigDocentes.DOCENTE.Get(row.GetAttr("id"));

                    //Valido atributos obligatorios  	                                             
                    if (objRead.c_docente == "")
                    {
                        objBatch.Err("No se especificó el Código del Docente, se rechaza el registro '" + objRead.c_tipo_documento + " - " + objRead.c_nro_documento + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }
                    if (objRead.c_tipo_documento == "")
                    {
                        objBatch.Err("No se especificó el Tipo de Documento, se rechaza el registro '" + objRead.c_tipo_documento + " - " + objRead.c_nro_documento + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }
                    if (objRead.c_nro_documento == "")
                    {
                        objBatch.Err("No se especificó el Nro de Documento, se rechaza el registro '" + objRead.c_tipo_documento + " - " + objRead.c_nro_documento + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }
                    if (objRead.d_ape_y_nom == "")
                    {
                        objBatch.Err("No se especificó el Apellido y Nombre, se rechaza el registro '" + objRead.c_tipo_documento + " - " + objRead.c_nro_documento + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }

                    //Recupero los OI de los códigos ingresados
                    string oiTIPDOC = "", oiPER = "", oiEMP = "";

                    //Recuperar la empresa
                    if (objRead.c_empresa != "")
                    {
                        oiEMP = NomadEnvironment.QueryValue("ORG03_EMPRESAS", "oi_empresa", "c_empresa", objRead.c_empresa, "", true);
                        if (oiEMP == null)
                        {
                            objBatch.Err("La Empresa no existe, se rechaza el registro '" + objRead.c_tipo_documento + " - " + objRead.c_nro_documento + "' - Linea: " + Linea.ToString());
                            Errores++;
                            continue;
                        }

                        //Recuperar la persona
                        if (objRead.c_personal != "")
                        {
                            oiPER = NomadEnvironment.QueryValue("PER01_PERSONAL", "oi_personal", "c_personal", objRead.c_personal, "", true);
                            if (oiPER == null)
                            {
                                objBatch.Err("La Persona no existe, se rechaza el registro '" + objRead.c_tipo_documento + " - " + objRead.c_nro_documento + "' - Linea: " + Linea.ToString());
                                Errores++;
                                continue;
                            }
                        }
                    }

                    if (objRead.c_tipo_documento != "")
                    {
                        oiTIPDOC = NomadEnvironment.QueryValue("PER20_TIPOS_DOC", "oi_tipo_documento", "c_tipo_documento", objRead.c_tipo_documento, "", true);
                        if (oiTIPDOC == null)
                        {
                            objBatch.Err("El Tipo de Documento no existe, se rechaza el registro '" + objRead.c_tipo_documento + " - " + objRead.c_nro_documento + "' - Linea: " + Linea.ToString());
                            Errores++;
                            continue;
                        }
                    }

                    //Me fijo si ya existe el Docente
                    string oiVal = NomadEnvironment.QueryValue("CYD04_DOCENTES", "oi_docente", "c_docente", objRead.c_docente, "", true);
                    
                    if (oiVal != null)
                    {
                        objBatch.Err("Ya existe un registro para el Docente '" + objRead.c_tipo_documento + " - " + objRead.c_nro_documento + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }
                    else
                    {
                        //Creo el Docente   
                        NucleusRH.Base.Capacitacion.Docentes.DOCENTE DDODOC;
                        DDODOC = new NucleusRH.Base.Capacitacion.Docentes.DOCENTE();
                        DDODOC.c_docente = objRead.c_docente;
                        DDODOC.c_nro_documento = objRead.c_nro_documento;
                        DDODOC.d_ape_y_nom = objRead.d_ape_y_nom;
                        DDODOC.oi_personal = oiPER;
                        DDODOC.d_direccion = objRead.d_direccion;
                        DDODOC.oi_tipo_documento = oiTIPDOC;
                        DDODOC.te_celular = objRead.te_celular;
                        DDODOC.te_particular = objRead.te_particular;

                        //Grabo
                        try
                        {
                            NomadEnvironment.GetCurrentTransaction().Save(DDODOC);
                            NomadEnvironment.QueryValueChange("CYD04_DOCENTES", "oi_docente", "c_docente", objRead.c_docente, "","1", true);
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

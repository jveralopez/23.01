using System;
using System.Xml;
using System.IO;
using System.Collections;

using Nomad.NSystem.Document;
using Nomad.NSystem.Proxy;
using Nomad.NSystem.Base;
using Nomad.NSystem.Functions;

namespace NucleusRH.Base.Migracion.Organizacion.Ubicaciones
{

    //////////////////////////////////////////////////////////////////////////////////
    //Clase Ubicaciones Org
    public partial class UBICACION 
    {
        public static void ImportarUbicaciones()
        {

            int Linea = 0, Errores = 0;

            NomadBatch objBatch;
            objBatch = NomadBatch.GetBatch("Iniciando...", "Importación de Ubicaciones");

            Hashtable htPARENTS = new Hashtable();            
            NomadXML IDList = new NomadXML();
            NucleusRH.Base.Migracion.Organizacion.Ubicaciones.UBICACION objRead;            

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
                    objRead = NucleusRH.Base.Migracion.Organizacion.Ubicaciones.UBICACION.Get(row.GetAttr("id"));
                    string oiLoc, oiJAFIP = "";

                    //Me fijo si llegan los atributos obligatorios
                    if (objRead.c_postal == "")
                    {
                        objBatch.Err("No especificó el Codigo Postal, se rechaza el registro '" + objRead.c_ubicacion + " - " + objRead.d_ubicacion + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }
                    if (objRead.d_calle == "")
                    {
                        objBatch.Err("No especificó la Calle, se rechaza el registro '" + objRead.c_ubicacion + " - " + objRead.d_ubicacion + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }
                    if (objRead.e_numeroNull)
                    {
                        objBatch.Err("No especificó el Numero, se rechaza el registro '" + objRead.c_ubicacion + " - " + objRead.d_ubicacion + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }

                    //Cargo el Padre  	
                    NucleusRH.Base.Organizacion.Empresas.EMPRESA ddoEMP = NucleusRH.Base.Migracion.Interfaces.INTERFACE.GetEmpresa(objRead.c_empresa, htPARENTS);
                    if (ddoEMP == null)
                    {
                        objBatch.Err("El Código de la Empresa no existe, se rechaza el registro '" + objRead.c_ubicacion + " - " + objRead.d_ubicacion + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }

                    //Me fijo si ya existe el Ubicación en la Empresa  	
                    string oiVal = NomadEnvironment.QueryValue("ORG03_UBICACIONES", "oi_ubicacion", "c_ubicacion", objRead.c_ubicacion, "ORG03_UBICACIONES.oi_empresa = " + ddoEMP.Id, true);
                    if (oiVal != null)
                    {
                        objBatch.Err("Ya existe un registro para la Ubicación '" + objRead.c_ubicacion + " - " + objRead.d_ubicacion + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }
                    else
                    {
                        //Busco las FK correspondientes  		
                        //Localidad
                        if (objRead.c_localidadNull)
                        {
                            objBatch.Err("No se especificó la Localidad, se rechaza el registro '" + objRead.c_ubicacion + " - " + objRead.d_ubicacion + "' - Linea: " + Linea.ToString());
                            Errores++;
                            continue;
                        }
                        else
                        {
                            oiLoc = NomadEnvironment.QueryValue("ORG19_LOCALIDADES", "oi_localidad", "c_localidad", objRead.c_localidad, "", true);
                            if (oiLoc == null)
                            {
                                objBatch.Err("No existe la Localidad especificada, se rechaza el registro '" + objRead.c_ubicacion + " - " + objRead.d_ubicacion + "' - Linea: " + Linea.ToString());
                                Errores++;
                                continue;
                            }
                        }

                        //Jurisdiccion AFIP
                        if (objRead.c_juris_afip != "")
                        {
                            oiJAFIP = NomadEnvironment.QueryValue("ORG36_JURIS_AFIP", "oi_juris_afip", "c_juris_afip", objRead.c_juris_afip, "", true);
                            if (oiJAFIP == null)
                            {
                                objBatch.Err("No existe la Jurisdicción AFIP especificada, se rechaza el registro '" + objRead.c_ubicacion + " - " + objRead.d_ubicacion + "' - Linea: " + Linea.ToString());
                                Errores++;
                                continue;
                            }
                        }

                        if (objRead.c_ubicacion == "" || objRead.d_ubicacion == "")
                        {
                            objBatch.Err("El Código o la Descripción de la Empresa no es válido, se rechaza el registro '" + objRead.c_ubicacion + " - " + objRead.d_ubicacion + "' - Linea: " + Linea.ToString());
                            Errores++;
                            continue;
                        }

                        //Creo la Ubicación
                        NucleusRH.Base.Organizacion.Empresas.UBICACION DDOUBICACION = new NucleusRH.Base.Organizacion.Empresas.UBICACION();

                        DDOUBICACION.c_afip = objRead.c_afip;
                        DDOUBICACION.c_estado = objRead.c_estado;
                        DDOUBICACION.c_postal = objRead.c_postal;
                        DDOUBICACION.c_ubicacion = objRead.c_ubicacion;
                        DDOUBICACION.d_calle = objRead.d_calle;
                        DDOUBICACION.d_departamento = objRead.d_departamento;
                        DDOUBICACION.d_ubicacion = objRead.d_ubicacion;
                        DDOUBICACION.e_numero = objRead.e_numero;
                        DDOUBICACION.e_piso = objRead.e_piso;
                        DDOUBICACION.e_pisoNull = objRead.e_pisoNull;
                        DDOUBICACION.n_area = objRead.n_area;
                        DDOUBICACION.n_areaNull = objRead.n_areaNull;
                        DDOUBICACION.n_pais = objRead.n_pais;
                        DDOUBICACION.n_paisNull = objRead.n_paisNull;
                        DDOUBICACION.n_telefono = objRead.n_telefono;
                        DDOUBICACION.n_telefonoNull = objRead.n_telefonoNull;
                        DDOUBICACION.o_ubicacion = objRead.o_ubicacion;
                        if (oiJAFIP != "") DDOUBICACION.oi_juris_afip = oiJAFIP;
                        if (oiLoc != "") DDOUBICACION.oi_localidad = oiLoc;

                        ddoEMP.UBICACIONES.Add(DDOUBICACION);
                        NomadEnvironment.QueryValueChange("ORG03_UBICACIONES", "oi_ubicacion", "c_ubicacion", objRead.c_ubicacion, "ORG03_UBICACIONES.oi_empresa = " + ddoEMP.Id,"1", true);
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

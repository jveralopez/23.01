using System;
using System.Xml;
using System.IO;
using System.Collections;

using Nomad.NSystem.Document;
using Nomad.NSystem.Proxy;
using Nomad.NSystem.Base;
using Nomad.NSystem.Functions;

namespace NucleusRH.Base.Migracion.Personal.CentrosCostosLegajo
{

    //////////////////////////////////////////////////////////////////////////////////
    //Clase Centros de Costos por Legajo
    public partial class CCOSTO_PER 
    {
        public static void ImportarCentrosCostosLegajos()
        {

            int Linea = 0, Errores = 0;

            NomadBatch objBatch;
            objBatch = NomadBatch.GetBatch("Iniciando...", "Importación de Centros de Costos por Legajos");

            Hashtable htPARENTS = new Hashtable();            
            NomadXML IDList = new NomadXML();
            NucleusRH.Base.Migracion.Personal.CentrosCostosLegajo.CCOSTO_PER objRead;
            DateTime fCompare = new DateTime(1900, 1, 1);            

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
                    objRead = NucleusRH.Base.Migracion.Personal.CentrosCostosLegajo.CCOSTO_PER.Get(row.GetAttr("id"));
                    string oiEMP = "", oiMC = "", oiCC;

                    //Valido atributos obligatorios
                    if (objRead.c_empresa == "")
                    {
                        objBatch.Err("No se especificó la Empresa, se rechaza el registro '" + objRead.c_centro_costo + " - " + objRead.f_ingreso.ToString("dd/MM/yyyy") + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }
                    if (objRead.e_nro_legajo == "")
                    {
                        objBatch.Err("No se especificó el Número de Legajo, se rechaza el registro '" + objRead.c_centro_costo + " - " + objRead.f_ingreso.ToString("dd/MM/yyyy") + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }
                    if (objRead.c_centro_costo == "")
                    {
                        objBatch.Err("No se especificó el Centro de Costos, se rechaza el registro '" + objRead.c_centro_costo + " - " + objRead.f_ingreso.ToString("dd/MM/yyyy") + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }
                    if (objRead.f_ingresoNull || objRead.f_ingreso < fCompare)
                    {
                        objBatch.Err("No se especificó la Fecha de Ingreso, se rechaza el registro '" + objRead.c_centro_costo + " - " + objRead.f_ingreso.ToString("dd/MM/yyyy") + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }

                    //Recupero la Empresa  	
                    oiEMP = NomadEnvironment.QueryValue("ORG03_EMPRESAS", "oi_empresa", "c_empresa", objRead.c_empresa, "", true);
                    if (oiEMP == null)
                    {
                        objBatch.Err("La Empresa no existe, se rechaza el registro '" + objRead.c_centro_costo + " - " + objRead.f_ingreso.ToString("dd/MM/yyyy") + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }

                    //Recupero el Centro de Costos
                    oiCC = NomadEnvironment.QueryValue("ORG08_CS_COSTO", "oi_centro_costo", "c_centro_costo", objRead.c_centro_costo, "", true);
                    if (oiCC == null)
                    {
                        objBatch.Err("El Centro de Costos no existe, se rechaza el registro '" + objRead.c_centro_costo + " - " + objRead.f_ingreso.ToString("dd/MM/yyyy") + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }

                    //Recupero el Motivo de Cambio
                    if (objRead.c_motivo_cambio != "")
                    {
                        oiMC = NomadEnvironment.QueryValue("PER05_MOT_CAMBIO", "oi_motivo_cambio", "c_motivo_cambio", objRead.c_motivo_cambio, "", true);
                        if (oiMC == null)
                        {
                            objBatch.Err("El Motivo de Cambio no existe, se rechaza el registro '" + objRead.c_centro_costo + " - " + objRead.f_ingreso.ToString("dd/MM/yyyy") + "' - Linea: " + Linea.ToString());
                            Errores++;
                            continue;
                        }
                    }

                    //Cargo el Padre  	
                    NucleusRH.Base.Personal.LegajoEmpresa.PERSONAL_EMP DDOLEG = NucleusRH.Base.Migracion.Interfaces.INTERFACE.GetLegajo(objRead.e_nro_legajo, oiEMP, htPARENTS);
                    if (DDOLEG == null)
                    {
                        objBatch.Err("El Legajo no existe en la Empresa, se rechaza el registro '" + objRead.c_centro_costo + " - " + objRead.f_ingreso.ToString("dd/MM/yyyy") + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }

                    //Me fijo si ya existe un registro de centro de costo en la fecha especificada para la persona  	
                    if (DDOLEG.CCOSTO_PER.GetByAttribute("f_ingreso", objRead.f_ingreso) != null)
                    {
                        objBatch.Err("Ya existe un Ingreso de Centro de Costos en el Legajo para la Fecha, '" + objRead.c_centro_costo + " - " + objRead.f_ingreso.ToString("dd/MM/yyyy") + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }

                    //Creo Centro de Costos en el Legajo			
                    NucleusRH.Base.Personal.LegajoEmpresa.CCOSTO_PER DDOCCPER;
                    DDOCCPER = new NucleusRH.Base.Personal.LegajoEmpresa.CCOSTO_PER();

                    DDOCCPER.f_egreso = objRead.f_egreso;
                    DDOCCPER.f_egresoNull = objRead.f_egresoNull;
                    DDOCCPER.f_ingreso = objRead.f_ingreso;
                    DDOCCPER.oi_centro_costo = oiCC;
                    DDOCCPER.o_cambio_ccosto = objRead.o_cambio_ccosto;

                    if (oiMC != "") DDOCCPER.oi_motivo_cambio = oiMC;

                    //Agrego el centro de costos
                    DDOLEG.CCOSTO_PER.Add(DDOCCPER);
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

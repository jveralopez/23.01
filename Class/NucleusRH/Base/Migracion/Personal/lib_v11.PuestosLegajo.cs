using System;
using System.Xml;
using System.IO;
using System.Collections;

using Nomad.NSystem.Document;
using Nomad.NSystem.Proxy;
using Nomad.NSystem.Base;
using Nomad.NSystem.Functions;

namespace NucleusRH.Base.Migracion.Personal.PuestosLegajo
{

    //////////////////////////////////////////////////////////////////////////////////
    //Clase Puestos por Legajo
    public partial class PUESTO_PER
    {
        public static void ImportarPuestosLegajos()
        {

            int Linea = 0, Errores = 0;

            NomadBatch objBatch;
            objBatch = NomadBatch.GetBatch("Iniciando...", "Importacion de Puestos por Legajos");

            Hashtable htPARENTS = new Hashtable();            
            NomadXML IDList = new NomadXML();
            NucleusRH.Base.Migracion.Personal.PuestosLegajo.PUESTO_PER objRead;
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
                    objRead = NucleusRH.Base.Migracion.Personal.PuestosLegajo.PUESTO_PER.Get(row.GetAttr("id"));

                    string oiEMP = "", oiMC = "", oiPUE = "";
                    //Valido atributos obligatorios
                    if (objRead.c_empresa == "")
                    {
                        objBatch.Err("No se especifico la Empresa, se rechaza el registro '" + objRead.c_puesto + " - " + objRead.f_ingreso.ToString("dd/MM/yyyy") + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }
                    if (objRead.e_nro_legajo == "")
                    {
                        objBatch.Err("No se especifico el Numero de Legajo, se rechaza el registro '" + objRead.c_puesto + " - " + objRead.f_ingreso.ToString("dd/MM/yyyy") + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }
                    if (objRead.c_puesto == "")
                    {
                        objBatch.Err("No se especifico el Puesto, se rechaza el registro '" + objRead.c_puesto + " - " + objRead.f_ingreso.ToString("dd/MM/yyyy") + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }
                    if (objRead.f_ingresoNull || objRead.f_ingreso < fCompare)
                    {
                        objBatch.Err("No se especifico la Fecha de Ingreso, se rechaza el registro '" + objRead.c_puesto + " - " + objRead.f_ingreso.ToString("dd/MM/yyyy") + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }
                    if (objRead.f_egresoNull || objRead.f_egreso < fCompare)
                    {
                        objBatch.Err("No se especifico la Fecha de Egreso , se rechaza el registro '" + objRead.c_puesto + " - " + objRead.f_ingreso.ToString("dd/MM/yyyy") + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }

                    //Recupero la Empresa
                    oiEMP = NomadEnvironment.QueryValue("ORG03_EMPRESAS", "oi_empresa", "c_empresa", objRead.c_empresa, "", true);
                    if (oiEMP == null)
                    {
                        objBatch.Err("La Empresa no existe, se rechaza el registro '" + objRead.c_puesto + " - " + objRead.f_ingreso.ToString("dd/MM/yyyy") + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }

                    //Recupero el Puesto
                    oiPUE = NomadEnvironment.QueryValue("ORG04_PUESTOS", "oi_puesto", "c_puesto", objRead.c_puesto, "ORG04_PUESTOS.oi_empresa = " + oiEMP, true);
                    if (oiPUE == null)
                    {
                        objBatch.Err("El Puesto no existe en la Empresa, se rechaza el registro '" + objRead.c_puesto + " - " + objRead.f_ingreso.ToString("dd/MM/yyyy") + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }

                    //Recupero el Motivo de Cambio
                    if (objRead.c_motivo_cambio != "")
                    {
                        oiMC = NomadEnvironment.QueryValue("PER05_MOT_CAMBIO", "oi_motivo_cambio", "c_motivo_cambio", objRead.c_motivo_cambio, "", true);
                        if (oiMC == null)
                        {
                            objBatch.Err("El Motivo de Cambio no existe, se rechaza el registro '" + objRead.c_puesto + " - " + objRead.f_ingreso.ToString("dd/MM/yyyy") + "' - Linea: " + Linea.ToString());
                            Errores++;
                            continue;
                        }
                    }

                    //Cargo el Padre
                    NucleusRH.Base.Personal.LegajoEmpresa.PERSONAL_EMP DDOLEG = NucleusRH.Base.Migracion.Interfaces.INTERFACE.GetLegajo(objRead.e_nro_legajo, oiEMP, htPARENTS);
                    if (DDOLEG == null)
                    {
                        objBatch.Err("El Legajo no existe en la Empresa, se rechaza el registro '" + objRead.c_puesto + " - " + objRead.f_ingreso.ToString("dd/MM/yyyy") + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }

                    //Me fijo si ya existe un registro de puesto en la fecha especificada para la persona
                    if (DDOLEG.PUESTO_PER.GetByAttribute("f_ingreso", objRead.f_ingreso) != null)
                    {
                        objBatch.Err("Ya existe un Ingreso de Puesto en el Legajo para la Fecha, '" + objRead.c_puesto + " - " + objRead.f_ingreso.ToString("dd/MM/yyyy") + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }

                    //Creo Puesto en el Legajo
                    NucleusRH.Base.Personal.LegajoEmpresa.PUESTO_PER DDOPUEPER;
                    DDOPUEPER = new NucleusRH.Base.Personal.LegajoEmpresa.PUESTO_PER();

                    DDOPUEPER.f_egreso = objRead.f_egreso;
                    DDOPUEPER.f_egresoNull = objRead.f_egresoNull;
                    DDOPUEPER.f_ingreso = objRead.f_ingreso;
                    DDOPUEPER.oi_puesto = oiPUE;
                    DDOPUEPER.o_cambio_puesto = objRead.o_cambio_puesto;
                    if (oiMC != "") DDOPUEPER.oi_motivo_cambio = oiMC;

                    //Agrego el Puesto
                    DDOLEG.PUESTO_PER.Add(DDOPUEPER);
                }
                catch (Exception e)
                {
                    objBatch.Err("Error desconocido en registro, por favor verifique el formato del archivo y la correcta asignacion de los campos - Linea: " + Linea.ToString() + " - " + e.Message);
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


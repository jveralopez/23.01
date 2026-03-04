using System;
using System.Xml;
using System.IO;
using System.Collections;

using Nomad.NSystem.Document;
using Nomad.NSystem.Proxy;
using Nomad.NSystem.Base;
using Nomad.NSystem.Functions;

namespace NucleusRH.Base.Migracion.Personal.RemuneracionesLegajo
{

    //////////////////////////////////////////////////////////////////////////////////
    //Clase Remuneraciones por Legajo
    public partial class REMUN_PER 
    {
        public static void ImportarRemuneracionesLegajos()
        {

            int Linea = 0, Errores = 0;

            NomadBatch objBatch;
            objBatch = NomadBatch.GetBatch("Iniciando...", "Importación de Remuneraciones por Legajos");

            Hashtable htPARENTS = new Hashtable();            
            NomadXML IDList = new NomadXML();
            NucleusRH.Base.Migracion.Personal.RemuneracionesLegajo.REMUN_PER objRead;
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
                    objRead = NucleusRH.Base.Migracion.Personal.RemuneracionesLegajo.REMUN_PER.Get(row.GetAttr("id"));

                    string oiEMP = "", oiMC = "";
                    //Valido atributos obligatorios
                    if (objRead.c_empresa == "")
                    {
                        objBatch.Err("No se especificó la Empresa, se rechaza el registro '" + objRead.n_remun_per + " - " + objRead.f_desde.ToString("dd/MM/yyyy") + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }
                    if (objRead.e_nro_legajo == "")
                    {
                        objBatch.Err("No se especificó el Número de Legajo, se rechaza el registro '" + objRead.n_remun_per + " - " + objRead.f_desde.ToString("dd/MM/yyyy") + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }
                    if (objRead.n_remun_per == 0d)
                    {
                        objBatch.Err("No se especificó el valor de la Remuneración, se rechaza el registro '" + objRead.n_remun_per + " - " + objRead.f_desde.ToString("dd/MM/yyyy") + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }
                    if (objRead.f_desdeNull || objRead.f_desde < fCompare)
                    {
                        objBatch.Err("No se especificó la Fecha de Ingreso, se rechaza el registro '" + objRead.n_remun_per + " - " + objRead.f_desde.ToString("dd/MM/yyyy") + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }

                    //Recupero la Empresa
                    oiEMP = NomadEnvironment.QueryValue("ORG03_EMPRESAS", "oi_empresa", "c_empresa", objRead.c_empresa, "", true);
                    if (oiEMP == null)
                    {
                        objBatch.Err("La Empresa no existe, se rechaza el registro '" + objRead.n_remun_per + " - " + objRead.f_desde.ToString("dd/MM/yyyy") + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }

                    //Recupero el Motivo de Cambio
                    if (objRead.c_motivo_cambio != "")
                    {
                        oiMC = NomadEnvironment.QueryValue("PER05_MOT_CAMBIO", "oi_motivo_cambio", "c_motivo_cambio", objRead.c_motivo_cambio, "", true);
                        if (oiMC == null)
                        {
                            objBatch.Err("El Motivo de Cambio no existe, se rechaza el registro '" + objRead.n_remun_per + " - " + objRead.f_desde.ToString("dd/MM/yyyy") + "' - Linea: " + Linea.ToString());
                            Errores++;
                            continue;
                        }
                    }

                    //Cargo el Padre  	
                    NucleusRH.Base.Personal.LegajoEmpresa.PERSONAL_EMP DDOLEG = NucleusRH.Base.Migracion.Interfaces.INTERFACE.GetLegajo(objRead.e_nro_legajo, oiEMP, htPARENTS);
                    if (DDOLEG == null)
                    {
                        objBatch.Err("El Legajo no existe en la Empresa, se rechaza el registro '" + objRead.n_remun_per + " - " + objRead.f_desde.ToString("dd/MM/yyyy") + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }

                    //Me fijo si ya existe un registro de remuneracion en la fecha especificada para la persona  	
                    if (DDOLEG.REMUN_PER.GetByAttribute("f_desde", objRead.f_desde) != null)
                    {
                        objBatch.Err("Ya existe un Ingreso de Remunación en el Legajo para la Fecha, '" + objRead.n_remun_per + " - " + objRead.f_desde.ToString("dd/MM/yyyy") + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }

                    //Creo la Remuneracion en el Legajo			
                    NucleusRH.Base.Personal.LegajoEmpresa.REMUN_PER DDOREMUNPER;
                    DDOREMUNPER = new NucleusRH.Base.Personal.LegajoEmpresa.REMUN_PER();

                    DDOREMUNPER.f_desde = objRead.f_desde;
                    DDOREMUNPER.f_hasta = objRead.f_hasta;
                    DDOREMUNPER.f_hastaNull = objRead.f_hastaNull;
                    DDOREMUNPER.n_remun_per = objRead.n_remun_per;
                    DDOREMUNPER.o_remun_per = objRead.o_remun_per;
                    if (oiMC != "") DDOREMUNPER.oi_motivo_cambio = oiMC;

                    //Agrego la remuneracion
                    DDOLEG.REMUN_PER.Add(DDOREMUNPER);
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

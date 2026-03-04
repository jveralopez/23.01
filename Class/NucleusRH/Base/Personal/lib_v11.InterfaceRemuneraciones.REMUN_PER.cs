using System;
using System.Xml;
using System.IO;
using System.Collections;

using Nomad.NSystem.Document;
using Nomad.NSystem.Proxy;
using Nomad.NSystem.Base;
using Nomad.NSystem.Functions;

namespace NucleusRH.Base.Personal.InterfaceRemuneraciones
{

    //////////////////////////////////////////////////////////////////////////////////
    //Clase Interface Remuneraciones
    public partial class REMUN_PER
    {
        public static void ImportarRemuneraciones(string oi_empresa)
        {
            int Linea = 0, Errores = 0;
            string oiMC = "";
            NomadBatch objBatch;
            objBatch = NomadBatch.GetBatch("Iniciando...", "Interface de Remuneraciones");

            NomadXML IDList = new NomadXML();
            NucleusRH.Base.Personal.InterfaceRemuneraciones.REMUN_PER objRead;
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
                    objRead = NucleusRH.Base.Personal.InterfaceRemuneraciones.REMUN_PER.Get(row.GetAttr("id"));
                    string oiPEREMP = "";
                    //Valido atributos obligatorios
                    if (objRead.e_numero_legajo == "")
                    {
                        objBatch.Err("No se especificó el Número de Legajo, se rechaza el registro '" + objRead.e_numero_legajo + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }
                    if (objRead.n_remun_per == 0d)
                    {
                        objBatch.Err("No se especificó el valor de la Remuneración, se rechaza el registro '" + objRead.e_numero_legajo + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }
                    if (objRead.f_desdeNull || objRead.f_desde < fCompare)
                    {
                        objBatch.Err("No se especificó la Fecha de Ingreso, se rechaza el registro '" + objRead.e_numero_legajo + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }

                    //Recupero el Legajo en la Empresa
                    oiPEREMP = NomadEnvironment.QueryValue("PER02_PERSONAL_EMP", "oi_personal_emp", "e_numero_legajo", objRead.e_numero_legajo, "PER02_PERSONAL_EMP.oi_empresa = " + oi_empresa, true);
                    if (oiPEREMP == null)
                    {
                        objBatch.Err("El Legajo no existe en la Empresa, se rechaza el registro '" + objRead.e_numero_legajo + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }

                    NucleusRH.Base.Personal.LegajoEmpresa.PERSONAL_EMP DDOLEG = NucleusRH.Base.Personal.LegajoEmpresa.PERSONAL_EMP.Get(oiPEREMP,false);
                    if (DDOLEG.oi_indic_activo != "1")
                    {
                        objBatch.Err("El Legajo no se encuentra Activo en la Empresa, se rechaza el registro '" + objRead.e_numero_legajo + "' - Linea: " + Linea.ToString());
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
                    else
                    {
                      oiMC = "4";
                    }

                    //Cierro la ultima remuneracion
                    foreach (NucleusRH.Base.Personal.LegajoEmpresa.REMUN_PER REMANT in DDOLEG.REMUN_PER)
                    {
                        if (REMANT.f_hastaNull)
                        {
                            REMANT.f_hasta = objRead.f_desde;
                            REMANT.oi_motivo_cambio = oiMC;
                        }
                    }

                    //Creo la Remuneracion en el Legajo
                    NucleusRH.Base.Personal.LegajoEmpresa.REMUN_PER DDOREMUNPER;
                    DDOREMUNPER = new NucleusRH.Base.Personal.LegajoEmpresa.REMUN_PER();

                    DDOREMUNPER.f_desde = objRead.f_desde;
                    DDOREMUNPER.f_hastaNull = true;
                    DDOREMUNPER.n_remun_per = objRead.n_remun_per;
                    DDOREMUNPER.o_remun_per = objRead.o_remun_per;

                    //Agrego la remuneracion
                    DDOLEG.REMUN_PER.Add(DDOREMUNPER);
                    DDOLEG.n_ult_remun = objRead.n_remun_per;
                    DDOLEG.f_desde_remun = objRead.f_desde;

                    //Grabo
                    try
                    {
                        NomadEnvironment.GetCurrentTransaction().Save(DDOLEG);
                    }
                    catch (Exception e)
                    {
                        if (e.Message == "DB.SQLSERVER.2627")
                        {
                            //Violation of primary key. Handle Exception
                            objBatch.Err("No se pudo actualizar el legajo " + objRead.e_numero_legajo + "' - Linea: " + Linea.ToString() + " - Ya existe un registro de remuneración con la misma fecha desde");
                        }
                        else
                        {
                            objBatch.Err("Error al grabar registro " + objRead.e_numero_legajo + "' - Linea: " + Linea.ToString() + " - " + e.Message);
                        }

                        Errores++;
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


using System;
using System.Xml;
using System.IO;
using System.Collections;

using Nomad.NSystem.Document;
using Nomad.NSystem.Proxy;
using Nomad.NSystem.Base;
using Nomad.NSystem.Functions;

namespace NucleusRH.Base.Personal.InterfacePosiciones
{

    //////////////////////////////////////////////////////////////////////////////////
    //Clase Interface Posiciones
    public partial class POSIC_PER
    {
        public static void ImportarPosiciones()
        {
            int Linea = 0, Errores = 0;

            NomadBatch objBatch;
            objBatch = NomadBatch.GetBatch("Iniciando...", "Migración de Posiciones");

            NomadXML IDList = new NomadXML();
            NucleusRH.Base.Personal.InterfacePosiciones.POSIC_PER objRead;
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
                    objRead = NucleusRH.Base.Personal.InterfacePosiciones.POSIC_PER.Get(row.GetAttr("id"));
                    string oiPEREMP = "", oiEMP = "", oiPOSEMP = "", oiPUEEMP = "";
                    NomadEnvironment.GetTrace().Info("objRead -- " + objRead.SerializeAll());


                    //Valido atributos obligatorios  	
                    if (objRead.c_empresa == "")
                    {
                        objBatch.Err("No se especificó la Empresa, se rechaza el registro '" + objRead.e_numero_legajo + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }
                    if (objRead.e_numero_legajo == "")
                    {
                        objBatch.Err("No se especificó el Número de Legajo, se rechaza el registro '" + objRead.e_numero_legajo + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }
                    if (objRead.c_posicion == "")
                    {
                        objBatch.Err("No se especificó la Posición, se rechaza el registro '" + objRead.e_numero_legajo + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }
                    if (objRead.f_desdeNull || objRead.f_desde < fCompare)
                    {
                        objBatch.Err("No se especificó la Fecha desde Posición, se rechaza el registro '" + objRead.e_numero_legajo + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }

                    //Recupero la Empresa  	   	
                    oiEMP = NomadEnvironment.QueryValue("ORG03_EMPRESAS", "oi_empresa", "c_empresa", objRead.c_empresa, "", true);
                    if (oiEMP == null)
                    {
                        objBatch.Err("La Empresa no existe, se rechaza el registro '" + objRead.e_numero_legajo + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }

                    //Valido que tenga un organigrama vigente la empresa
                    NomadXML vigente = NomadEnvironment.QueryNomadXML(POSIC_PER.Resources.QRY_VIGENTE, "<PARAM oi_empresa=\"" + oiEMP + "\"/>");
                    NomadEnvironment.GetTrace().Info("VIGENTE-- " + vigente.FirstChild().GetAttr("vigente"));

                    if (vigente.FirstChild().GetAttr("vigente") == "0")
                    {
                        objBatch.Err("La empresa no tiene un Organigrama Vigente, se rechaza el registro '" + objRead.e_numero_legajo + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }

                    //Recupero el Legajo en la Empresa
                    oiPEREMP = NomadEnvironment.QueryValue("PER02_PERSONAL_EMP", "oi_personal_emp", "e_numero_legajo", objRead.e_numero_legajo, "PER02_PERSONAL_EMP.oi_empresa = " + oiEMP, true);
                    if (oiPEREMP == null)
                    {
                        objBatch.Err("El Legajo no existe en la Empresa, se rechaza el registro '" + objRead.e_numero_legajo + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }

                    //Recupero el Puesto de la Empresa
                    oiPUEEMP = NomadEnvironment.QueryValue("ORG04_PUESTOS", "oi_puesto", "c_puesto", objRead.c_puesto, "ORG04_PUESTOS.oi_empresa = " + oiEMP, true);
                    if (oiPUEEMP == null)
                    {
                        objBatch.Err("El Puesto no existe en la Empresa, se rechaza el registro '" + objRead.e_numero_legajo + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }

                    //Recupero la Posición del Puesto

                    oiPOSEMP = NomadEnvironment.QueryValue("ORG04_POSICIONES", "oi_posicion", "c_posicion", objRead.c_posicion, "ORG04_POSICIONES.oi_puesto = " + oiPUEEMP, true);
                    if (oiPOSEMP == null)
                    {
                        objBatch.Err("La posición no existe para el Puesto ingresado, se rechaza el registro '" + objRead.e_numero_legajo + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }

                    //Verifico que el legajo este activo en la empresa
                    NucleusRH.Base.Personal.LegajoEmpresa.PERSONAL_EMP DDOLEG = NucleusRH.Base.Personal.LegajoEmpresa.PERSONAL_EMP.Get(oiPEREMP);
                    if (DDOLEG.oi_indic_activo != "1")
                    {
                        objBatch.Err("El Legajo no se encuentra Activo en la Empresa, se rechaza el registro '" + objRead.e_numero_legajo + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }

                    //Verifico si la posición a actualizar es la actual
                    if (oiPOSEMP == DDOLEG.oi_posicion_ult && DDOLEG.f_egresoNull)
                    {
                        objBatch.Err("La posición a actualizar es la actual, se rechaza el registro '" + objRead.e_numero_legajo + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }

                    //Verifico que la fecha desde ingresada sea superior a la fecha desde posicion del legajo
                    if (DDOLEG.f_desde_posicion > objRead.f_desde)
                    {
                        objBatch.Err("La fecha desde de posición indicada es inferior a la última fecha desde de la posición, se rechaza el registro '" + objRead.e_numero_legajo + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }

                    //PUESTO
                    NucleusRH.Base.Personal.LegajoEmpresa.PUESTO_PER DDOPUELEG = new NucleusRH.Base.Personal.LegajoEmpresa.PUESTO_PER();
                    DDOPUELEG.oi_puesto = oiPUEEMP;
                    DDOPUELEG.f_ingreso = objRead.f_desde;

                    DDOLEG.Cambio_Puesto(DDOPUELEG);
                    
                    //POSICION 
                    NucleusRH.Base.Personal.LegajoEmpresa.POSICION_PER DDOPOSICLEG = new NucleusRH.Base.Personal.LegajoEmpresa.POSICION_PER();
                    DDOPOSICLEG.oi_posicion = oiPOSEMP;
                    DDOPOSICLEG.f_ingreso = objRead.f_desde;

                    if (DDOLEG.POSIC_PER.Count > 0)
                    {
                        DDOLEG.Cambio_Posicion(DDOPOSICLEG);
                    }
                    else
                    {
                        DDOLEG.oi_posicion_ult = oiPOSEMP;
                        DDOLEG.f_desde_posicion = objRead.f_desde;

                        DDOLEG.Asignar_Posicion(DDOPOSICLEG);
                    }

                    //Grabo
                    try
                    {
                        NomadEnvironment.GetCurrentTransaction().Save(DDOLEG);
                        if (DDOLEG.POSIC_PER.Count > 0)
                        {
                            NucleusRH.Base.Organizacion.Puestos.POSICION.Cambiar_Pos_Legajo_Sectores(DDOLEG.oi_posicion_ult, row.GetAttr("oi_posicion"), DDOLEG.id.ToString(), objRead.f_desde, "", "");
                        }
                        else
                        {
                            NucleusRH.Base.Organizacion.Puestos.POSICION.AddLegajoSectores(DDOPOSICLEG.oi_posicion, DDOLEG.id.ToString(), objRead.f_desde, new DateTime(1899, 1, 1), "", "");
                        }
                    }
                    catch (Exception e)
                    {
                        objBatch.Err("Error al grabar registro " + objRead.e_numero_legajo + "' - Linea: " + Linea.ToString() + " - " + e.Message);
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

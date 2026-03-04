using System;
using System.Xml;
using System.IO;
using System.Collections;

using Nomad.NSystem.Document;
using Nomad.NSystem.Proxy;
using Nomad.NSystem.Base;
using Nomad.NSystem.Functions;

namespace NucleusRH.Base.Personal.InterfacePlanesObraSocial
{

    //////////////////////////////////////////////////////////////////////////////////
    //Clase Interface Planes Obra Social
    public partial class PLANOS_PER
    {
    public static void ImportarPlanesObraSocial() {

            /*- Si el código de plan de obra social existe en la obra social, se actualizarán los datos del plan existente.
              - Si el cogido del plan de obra social no existe en la obra social se creara un nuevo registro con los datos del archivo.*/

            /*PER06_PLANES_OS > NucleusRH.Base.Personal.Obras_Sociales.PLAN_OS*/

            int Linea = 0, Errores = 0;

            NomadBatch objBatch;
            objBatch = NomadBatch.GetBatch("Iniciando...", "Interface de Planes de Obra Social");

            NomadXML IDList = new NomadXML();
            NucleusRH.Base.Personal.InterfacePlanesObraSocial.PLANOS_PER objRead;

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
                    objRead = NucleusRH.Base.Personal.InterfacePlanesObraSocial.PLANOS_PER.Get(row.GetAttr("id"));

                    /*
                        - El código de obra social no este vacío 
                        - El código del plan no este vacío. 
                        - La descripción del plan no este vacío. 
                        - La obra social exista. 
                        - El separador de decimales en los valores de los monto es '.'. 
                        - El código del plan y la descripci#eben ser igual al registrado en el ABM. Caso contrario, se agregará como plan nuevo. 
                        - Si no existe el código del plan, se agrega como nuevo plan. 
                        - Si la descripción del plan se modifica se actualiza.
                     */


                    //El código de obra social no este vacío
                    if(objRead.c_obra_social == "" || objRead.c_obra_socialNull)
                    {
                        objBatch.Err("No se especificó el Código de la Obra Social. Se rechaza el registro de la línea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }

                    //El cogido del plan no este vacío
                    if (objRead.c_plan_os == "" || objRead.c_plan_osNull)
                    {
                        objBatch.Err("No se especificó el Código del Plan. Se rechaza el registro de la línea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }

                    //La descripción del plan no este vacío
                    if (objRead.d_plan_os == "" || objRead.d_plan_osNull)
                    {
                        objBatch.Err("No se especificó la Descripción del Plan. Se rechaza el registro de la línea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }

                    //Valido el formato de los montos
                    
                    string oiOS;
                    string oiPlan;
                    //Recupero el OI de la OS para validar que la misma exista
                    oiOS = NomadEnvironment.QueryValue("PER06_OBRAS_SOC", "oi_obra_social", "c_obra_social", objRead.c_obra_social, "", true);
                    if (oiOS == null)
                    {
                        objBatch.Err("El Código de la Obra Social no existe. Se rechaza el registro '" + objRead.c_obra_social + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }

                    //Recupero el OI del Plan para determinar si es nuevo o se actualizarán los montos
                    oiPlan = NomadEnvironment.QueryValue("PER06_PLANES_OS", "oi_plan_os", "c_plan_os", objRead.c_plan_os, "PER06_PLANES_OS.oi_obra_social=" + oiOS, true);

                    Obras_Sociales.OBRA_SOCIAL obraSocial = Obras_Sociales.OBRA_SOCIAL.Get(oiOS);
                    Obras_Sociales.PLAN_OS planact;
                    //Existe
                    if(oiPlan!=null)
                    {
                        planact = (Obras_Sociales.PLAN_OS)obraSocial.PLANES_OS.GetById(oiPlan);
                        planact.d_plan_os = objRead.d_plan_os;
                        planact.n_cuota = objRead.n_cuota;
                        planact.n_fam1 = objRead.n_fam1;
                        planact.n_fam2 = objRead.n_fam2;
                        planact.n_adic1 = objRead.n_adic1;
                        planact.n_adic2 = objRead.n_adic2;
                        planact.n_adic3 = objRead.n_adic3;
                        planact.n_adic4 = objRead.n_adic4;
                        planact.n_adic5 = objRead.n_adic5;
                        planact.o_plan_os = objRead.o_plan_os;
                    }
                    //No existe
                    else
                    {
                        planact = new Obras_Sociales.PLAN_OS();
                        planact.oi_obra_social = int.Parse(oiOS);
                        planact.c_plan_os = objRead.c_plan_os;
                        planact.d_plan_os = objRead.d_plan_os;
                        planact.n_cuota = objRead.n_cuota;
                        planact.n_fam1 = objRead.n_fam1;
                        planact.n_fam2 = objRead.n_fam2;
                        planact.n_adic1 = objRead.n_adic1;
                        planact.n_adic2 = objRead.n_adic2;
                        planact.n_adic3 = objRead.n_adic3;
                        planact.n_adic4 = objRead.n_adic4;
                        planact.n_adic5 = objRead.n_adic5;
                        planact.o_plan_os = objRead.o_plan_os;
                        obraSocial.PLANES_OS.Add(planact);
                    }

                    try
                    {
                        NomadEnvironment.GetCurrentTransaction().Save(obraSocial);
                        //NomadEnvironment.GetCurrentTransaction().SaveRefresh(obraSocial);
                    }
                    catch (Exception e)
                    {
                        objBatch.Err("Error al grabar registro Obra Social: '" + objRead.c_obra_social + "' | Plan: '"+ objRead.c_plan_os+"' - Linea: " + Linea.ToString() + " - " + e.Message);
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


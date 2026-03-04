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

namespace NucleusRH.Base.Tiempos_Trabajados.Interfaces
{
    public partial class CAMBIO_TURNOS : Nomad.NSystem.Base.NomadObject
    {
        public static void ImportarCambioTurnos()
        {
            int Linea, Errores = 0;
            NomadXML IDCur;
            NomadXML IDList = new NomadXML();
            NucleusRH.Base.Tiempos_Trabajados.Interfaces.CAMBIO_TURNOS objRead;
            string EmpresaOI, LegajoOI, HorarioOI, EscuadraOI, LegajoReloj, TipoHorario, f_hasta;

            NomadBatch objBatch;
            objBatch = NomadBatch.GetBatch("Iniciando...", "Importacion de Cambio de Turnos");

            IDList.SetText(NomadProxy.GetProxy().SQLService().Get(CAMBIO_TURNOS.Resources.qry_rows, ""));
            
            for (Linea = 1, IDCur = IDList.FindElement("ROWS").FirstChild(); IDCur != null; IDCur = IDCur.Next(), Linea++)
            {
                objBatch.SetPro(0, 100, IDList.FirstChild().ChildLength, Linea);
                objBatch.SetMess("Importando cambio de turnos " + Linea + " de " + IDList.FirstChild().ChildLength);
                NomadLog.Info("0-- " + IDCur.GetAttr("id"));
                objRead = NucleusRH.Base.Tiempos_Trabajados.Interfaces.CAMBIO_TURNOS.Get(IDCur.GetAttr("id"));
                NomadLog.Info("1");
                EscuadraOI = "";

                //Inicio la Transaccion
                try
                {
                    if (objRead.empresa == "")
                    {
                        objBatch.Err("No se especifico la empresa, se rechaza el registro - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }
                    if (objRead.legajoNull)
                    {
                        objBatch.Err("No se especifico el legajo, se rechaza el registro - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }
                    if (objRead.horario == "")
                    {
                        objBatch.Err("No se especifico el horario, se rechaza el registro '" + objRead.empresa + " - " + objRead.legajo + "'  - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }
                    if (objRead.f_desdeNull)
                    {
                        objBatch.Err("No se especifico la Fecha de Inicio, se rechaza el registro '" + objRead.empresa + " - " + objRead.legajo + "'  - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }  
                    EmpresaOI = NomadEnvironment.QueryValue("ORG03_EMPRESAS", "oi_empresa", "c_empresa", objRead.empresa, "", true);
                    if (EmpresaOI == null)
                    {
                        objBatch.Err("La empresa especificada no existe, se rechaza el registro'" + objRead.empresa + " - " + objRead.legajo + "'  - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }

                    LegajoOI = NomadEnvironment.QueryValue("PER02_PERSONAL_EMP", "oi_personal_emp", "e_numero_legajo", objRead.legajo.ToString(), "PER02_PERSONAL_EMP.oi_empresa = " + EmpresaOI, true);
                    if (LegajoOI == null)
                    {
                        objBatch.Err("El legajo especificado no existe, se rechaza el registro'" + objRead.empresa + " - " + objRead.legajo + "'  - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }

                    LegajoReloj = NomadEnvironment.QueryValue("TTA04_PERSONAL", "e_nro_legajo_reloj", "oi_personal_emp", LegajoOI, "", true);
                    if (LegajoReloj == null)
                    {
                        objBatch.Err("El legajo especificado no tiene asignado un número de legajo reloj, se rechaza el registro'" + objRead.empresa + " - " + objRead.legajo + "'  - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }

                    HorarioOI = NomadEnvironment.QueryValue("TTA02_HORARIOS", "oi_horario", "c_horario", objRead.horario.ToString(), "", true);
                    if (HorarioOI == null)
                    {
                        objBatch.Err("El horario especificado no existe, se rechaza el registro'" + objRead.empresa + " - " + objRead.legajo + "'  - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }
                    else
                    {
                        TipoHorario = NomadEnvironment.QueryValue("TTA02_HORARIOS", "d_tipohorario", "oi_horario", HorarioOI, "", true);
                        if (TipoHorario == "R")
                        {
                            if (objRead.escuadra == null)
                            {
                                objBatch.Err("No se especificó la escuadra, se rechaza el registro'" + objRead.empresa + " - " + objRead.legajo + "'  - Linea: " + Linea.ToString());
                                Errores++;
                                continue;
                            }

                            EscuadraOI = NomadEnvironment.QueryValue("TTA02_ESCUADRAS", "oi_escuadra", "c_escuadra", objRead.escuadra, "TTA02_ESCUADRAS.oi_horario = " + HorarioOI, true);
                            if (EscuadraOI == null)
                            {
                                objBatch.Err("La escuadra especificada no es válida, se rechaza el registro'" + objRead.empresa + " - " + objRead.legajo + "'  - Linea: " + Linea.ToString());
                                Errores++;
                                continue;
                            }
                        }
                    }

                    //Seteo la Fecha Hasta según su contenido
                    if (objRead.f_hastaNull)
                        f_hasta = "";
                    else
                        f_hasta = objRead.f_hasta.ToString("yyyyMMdd");

                    NomadLog.Info("2");
                    //INSTANCIO EL HORARIO
                    string rslt = NucleusRH.Base.Tiempos_Trabajados.Horarios.HORARIO.AsignarHorario(LegajoOI, objRead.f_desde.ToString("yyyyMMdd"), f_hasta, HorarioOI, EscuadraOI);
                    objBatch.Log("Linea " + Linea + "= " + rslt);
                }
                catch (Exception e)
                {
                    objBatch.Err("Se ha producido un error: " + e.Message + " - Linea " + Linea.ToString());
                    Errores++;
                }
                    
                    if (Linea % 10 == 0) objBatch.Log("Registros Procesados:" + Linea.ToString() + " - Importados:" + (Linea - Errores).ToString());

            }
            Linea--;
            objBatch.Log("Registros Procesados:" + Linea.ToString() + " - Importados:" + (Linea - Errores).ToString());
            objBatch.Log("Finalizado...");
            
        }
    }
}

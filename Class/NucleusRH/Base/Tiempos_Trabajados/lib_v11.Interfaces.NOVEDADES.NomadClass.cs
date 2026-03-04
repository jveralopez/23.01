using System;
using System.Xml;
using System.IO;
using System.Collections;

using Nomad.NSystem.Document;
using Nomad.NSystem.Proxy;
using Nomad.NSystem.Base;
using Nomad.NSystem.Functions;

namespace NucleusRH.Base.Tiempos_Trabajados.Interfaces
{
    public partial class NOVEDADES
    {
        public static void ImportarNovedades(string oi_terminal)
        {
            int Linea, Errores = 0;
            NomadXML IDCur;
            NomadXML IDList = new NomadXML();
            NucleusRH.Base.Tiempos_Trabajados.Interfaces.NOVEDADES objRead;
            string EstructuraOI, EmpresaOI, LegajoOI, HorarioOI, LegajoReloj;

            NomadBatch objBatch;
            objBatch = NomadBatch.GetBatch("Iniciando...", "Importacion de Novedades");

      EstructuraOI = NomadEnvironment.QueryValue("TTA05_TERMINALES", "oi_estructura", "oi_terminal", oi_terminal, "", true);
      if (EstructuraOI == null)
      {
        objBatch.Err("La terminal especificada no existe, se cancela la importacion");
        return;
      }

            IDList.SetText(NomadProxy.GetProxy().SQLService().Get(NOVEDADES.Resources.qry_rows, ""));

            for (Linea = 1, IDCur = IDList.FindElement("ROWS").FirstChild(); IDCur != null; IDCur = IDCur.Next(), Linea++)
            {
                objBatch.SetPro(0, 100, IDList.FirstChild().ChildLength, Linea);
                objBatch.SetMess("Importando novedades " + Linea + " de " + IDList.FirstChild().ChildLength);
                objRead = NucleusRH.Base.Tiempos_Trabajados.Interfaces.NOVEDADES.Get(IDCur.GetAttr("id"));

                try
                {
                    if (objRead.empresaNull)
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
                    if (objRead.horarioNull)
                    {
                        objBatch.Err("No se especifico el horario, se rechaza el registro '" + objRead.empresa + " - " + objRead.legajo + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }
                    if (objRead.f_desdeNull)
                    {
                        objBatch.Err("No se especifico la Fecha de Inicio, se rechaza el registro '" + objRead.empresa + " - " + objRead.legajo + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }
                    else
                    {
                        if (objRead.f_desde <= new DateTime(2000, 1, 1))
                        {
                            objBatch.Err("La fecha de inicio de la novedad tiene que ser mayor al 01/01/2000, se rechaza el registro '" + objRead.empresa + " - " + objRead.legajo + "' - Linea: " + Linea.ToString());
                            Errores++;
                            continue;
                        }
                    }

                    if (objRead.f_hastaNull)
                    {
                        objBatch.Err("No se especifico la Fecha de Fin, se rechaza el registro '" + objRead.empresa + " - " + objRead.legajo + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }

                    EmpresaOI = NomadEnvironment.QueryValue("ORG03_EMPRESAS", "oi_empresa", "c_empresa", objRead.empresa, "", true);
                    if (EmpresaOI == null)
                    {
                        objBatch.Err("La empresa especificada no existe, se rechaza el registro '" + objRead.empresa + " - " + objRead.legajo + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }

                    LegajoOI = NomadEnvironment.QueryValue("PER02_PERSONAL_EMP", "oi_personal_emp", "e_numero_legajo", objRead.legajo.ToString(), "PER02_PERSONAL_EMP.oi_empresa = " + EmpresaOI, true);
                    if (LegajoOI == null)
                    {
                        objBatch.Err("El legajo especificado no existe, se rechaza el registro '" + objRead.empresa + " - " + objRead.legajo + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }

                    LegajoReloj = NomadEnvironment.QueryValue("TTA04_PERSONAL", "e_nro_legajo_reloj", "oi_personal_emp", LegajoOI, "", true);
                    if (LegajoReloj == null)
                    {
                        objBatch.Err("El legajo especificado no tiene asignado un número de legajo reloj, se rechaza el registro '" + objRead.empresa + " - " + objRead.legajo + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }

                    HorarioOI = NomadEnvironment.QueryValue("TTA01_TIPOHORAS", "oi_tipohora", "c_tipohora", objRead.horario.ToString(), "", true);
                    if (HorarioOI == null)
                    {
                        objBatch.Err("El tipo de hora especificado no existe, se rechaza el registro '" + objRead.empresa + " - " + objRead.legajo + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }

          try
          {
            //Nueva Novedad
            NucleusRH.Base.Tiempos_Trabajados.Personal.NOVEDAD ddoNovPer = NucleusRH.Base.Tiempos_Trabajados.Personal.NOVEDAD.New();
            ddoNovPer.oi_estructura = EstructuraOI;
            ddoNovPer.oi_tipohora = HorarioOI;
            ddoNovPer.f_fecha = NucleusRH.Base.Tiempos_Trabajados.Esperanzaper.ESPERANZAPER.GetDateHope(LegajoOI, objRead.f_desde);
            ddoNovPer.e_horainicio = Convert.ToInt32(objRead.f_desde.Subtract(ddoNovPer.f_fecha).TotalMinutes);
            ddoNovPer.e_horafin = Convert.ToInt32(objRead.f_hasta.Subtract(ddoNovPer.f_fecha).TotalMinutes);
            ddoNovPer.d_novedad = objRead.descripcion;
            ddoNovPer.o_novedad  = objRead.observaciones;
            //Validar y Guardar Novedad
            NucleusRH.Base.Tiempos_Trabajados.Personal.NOVEDAD.AltaNovedad(LegajoOI, ddoNovPer);
          }
          catch (Exception e)
          {
            objBatch.Err("Error: " + e.Message + " - Se rechaza el registro '" + objRead.empresa + " - " + objRead.legajo + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
          }
                }
                catch (Exception e)
                {
                    objBatch.Err("Se ha producido un error: " + e.Message + " - Linea " + Linea.ToString());
                    Errores++;
                }
            }

            Linea--;
            objBatch.Log("Registros Procesados: " + Linea.ToString() + " - Importados: " + (Linea - Errores).ToString());
            objBatch.Log("Finalizado...");
        }
    }
}

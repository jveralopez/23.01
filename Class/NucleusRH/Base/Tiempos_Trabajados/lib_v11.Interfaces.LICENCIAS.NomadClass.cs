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
    public partial class LICENCIAS : Nomad.NSystem.Base.NomadObject
    {
        public static void ImportarLicencias()
        {
            int Linea, Errores = 0;
            NomadXML IDCur;
            NomadXML IDList = new NomadXML();
            NucleusRH.Base.Tiempos_Trabajados.Interfaces.LICENCIAS objRead;
            string EmpresaOI, LegajoOI, LicenciaOI, codLicFliar = "";

            NomadBatch objBatch;
            objBatch = NomadBatch.GetBatch("Iniciando...", "Importacion de Licencias");

            //Obtengo el Parametro de CODIGO de Licencia por Enfermedad Familiar
            NomadXML codLic = NomadEnvironment.QueryNomadXML(NucleusRH.Base.Personal.Licencias.LICENCIA.Resources.QRY_LIC_FLIAR, "");

            //Si el cliente tiene cargado el parametro "Licencia Familiar Enfermo Controlado" controla este tipo de licencia
            if (codLic.FirstChild().GetAttr("d_valor") != "") codLicFliar = codLic.FirstChild().GetAttr("d_valor");

            IDList.SetText(NomadProxy.GetProxy().SQLService().Get(LICENCIAS.Resources.qry_rows, ""));

            for (Linea = 1, IDCur = IDList.FindElement("ROWS").FirstChild(); IDCur != null; IDCur = IDCur.Next(), Linea++)
            {
                objBatch.SetPro(0, 100, IDList.FirstChild().ChildLength, Linea);
                objBatch.SetMess("Importando licencia " + Linea + " de " + IDList.FirstChild().ChildLength);
                objRead = NucleusRH.Base.Tiempos_Trabajados.Interfaces.LICENCIAS.Get(IDCur.GetAttr("id"));

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
                    if (objRead.licenciaNull)
                    {
                        objBatch.Err("No se especifico la licencia, se rechaza el registro '" + objRead.empresa + " - " + objRead.legajo + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }
                    if (objRead.f_desdeNull)
                    {
                        objBatch.Err("No se especifico la Fecha de Inicio, se rechaza el registro '" + objRead.empresa + " - " + objRead.legajo + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
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

                    LicenciaOI = NomadEnvironment.QueryValue("PER16_LICENCIAS", "oi_licencia", "c_enlace", objRead.licencia, "", true);
                    if (LicenciaOI == null)
                    {
                        LicenciaOI = NomadEnvironment.QueryValue("PER16_LICENCIAS", "oi_licencia", "c_licencia", objRead.licencia, "", true);
                        if (LicenciaOI == null)
                        {
                            objBatch.Err("La licencia especificada no existe, se rechaza el registro '" + objRead.empresa + " - " + objRead.legajo + "' - Linea: " + Linea.ToString());
                            Errores++;
                            continue;
                        }
                        else
                        {
                          //Si existe el parametro - el cliente controla licencia por familiar enfermo
                          if(codLicFliar != "")
                          {
                              if (codLicFliar.ToString() == objRead.licencia.ToString())
                              {
                                  objBatch.Err("La licencia especificada es de tipo familiar enfermo, se rechaza el registro '" + objRead.empresa + " - " + objRead.legajo + "' - Linea: " + Linea.ToString());
                                  Errores++;
                                  continue;
                              }
                          }
                        }
                    }
                    else
                    {
                      //Si existe el parametro - el cliente controla licencia por familiar enfermo
                      if(codLicFliar != "")
                      {
                        if (codLicFliar.ToString() == objRead.licencia.ToString())
                          {
                              objBatch.Err("La licencia especificada es de tipo familiar enfermo, se rechaza el registro '" + objRead.empresa + " - " + objRead.legajo + "' - Linea: " + Linea.ToString());
                              Errores++;
                              continue;
                          }
                      }
                    }
          try
          {
            //Nueva Licencia
            NucleusRH.Base.Personal.LegajoEmpresa.LICENCIA_PER ddoLicPer = NucleusRH.Base.Personal.LegajoEmpresa.LICENCIA_PER.New();
            ddoLicPer.oi_licencia = LicenciaOI;
            ddoLicPer.f_inicio = objRead.f_desde;
            ddoLicPer.f_fin = objRead.f_hasta;
            ddoLicPer.e_cant_dias = objRead.f_hasta.Subtract(objRead.f_desde).Days + 1;
            ddoLicPer.e_anio_corresp = objRead.f_desde.Year;
            ddoLicPer.l_bloqueada = false;
            ddoLicPer.l_interfaz = true;
            ddoLicPer.l_habiles = false;
            ddoLicPer.o_licencia_per = objRead.observaciones;

            //Validar y Guardar Licencia
            NucleusRH.Base.Personal.LegajoEmpresa.LICENCIA_PER.AltaLicencia(LegajoOI, ddoLicPer);
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

                if (Linea % 10 == 0) objBatch.Log("Registros Procesados: " + Linea.ToString() + " - Importados: " + (Linea - Errores).ToString());
            }

            Linea--;
            objBatch.Log("Registros Procesados: " + Linea.ToString() + " - Importados: " + (Linea - Errores).ToString());
            objBatch.Log("Finalizado...");
        }
    }
}



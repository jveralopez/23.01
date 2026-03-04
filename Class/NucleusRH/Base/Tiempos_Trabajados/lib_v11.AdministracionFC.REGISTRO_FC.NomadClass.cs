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

namespace NucleusRH.Base.Tiempos_Trabajados.AdministracionFC 
{

  //////////////////////////////////////////////////////////////////////////////////
  //Clase Registro de Francos Compensatorios
  public partial class REGISTRO_FC : Nomad.NSystem.Base.NomadObject
  {
      public static void AprobarFC(Nomad.NSystem.Proxy.NomadXML aprobados)
      {
          //Inicializando variables
          int totRegs, linea = 0, errores = 0, procesados = 0;
          NucleusRH.Base.Tiempos_Trabajados.AdministracionFC.REGISTRO_FC objRegistroFC;
          NucleusRH.Base.Personal.LegajoEmpresa.PERSONAL_EMP objLegajo;
          NucleusRH.Base.Personal.Legajo.PERSONAL objPersona;

          //Instancio el Objeto Batch
          NomadBatch objBatch;
          objBatch = NomadBatch.GetBatch("Aprobación de Francos Compensatorios", "Aprobación de Francos Compensatorios");

          aprobados = aprobados.FirstChild();
          Nomad.NSystem.Proxy.NomadXML registro;
          totRegs = aprobados.ChildLength;

          //Recorriendo la lsita de Registros FC por aprobar
          for (linea=1, registro = aprobados.FirstChild(); registro != null; linea++, registro = registro.Next())
          {
              objBatch.SetPro(0, 100, totRegs, linea);
              objBatch.SetMess("Procesando la Linea " + linea + " de " + totRegs);

              objRegistroFC = NucleusRH.Base.Tiempos_Trabajados.AdministracionFC.REGISTRO_FC.Get(registro.GetAttr("id"));
              objRegistroFC.l_aprob = true;
              objRegistroFC.n_cant_aprob = registro.GetAttrInt("n_cant_aprob");
              objRegistroFC.n_saldo = registro.GetAttrInt("n_cant_aprob");

              try
              {
                  NomadEnvironment.GetCurrentTransaction().SaveRefresh(objRegistroFC);
                  procesados++;
              }
              catch (Exception e)
              {
                  objLegajo = NucleusRH.Base.Personal.LegajoEmpresa.PERSONAL_EMP.Get(objRegistroFC.oi_personal_emp);
                  objPersona = NucleusRH.Base.Personal.Legajo.PERSONAL.Get(objLegajo.oi_personal);
                  objBatch.Err("No se pudo aprobar el Registro FC  del Legajo: "+objLegajo.e_numero_legajo+" - "+objPersona.d_ape_y_nom+" para la fecha "+objRegistroFC.f_registro_fc.ToString("dd/mm/yyyy"));
                  errores++;
              }

          }

          //Resultados
          objBatch.Log("Cantidad de Registros FC aprobados: " + procesados.ToString());
          objBatch.Log("Cantidad de Errores en el Proceso: " + errores.ToString());
          objBatch.Log("Total de Registros procesados: " + totRegs.ToString());
          objBatch.Log("Finalizado...");
      }

      public static void AdministrarFC(Nomad.NSystem.Proxy.NomadXML solicitados)
      {
          //Inicializando variables
          int totRegs, linea = 0, compensaciones = 0, errores=0, cantSaldoMayor=0, cantidadFecha = 0;
          NucleusRH.Base.Tiempos_Trabajados.AdministracionFC.REGISTRO_FC objRegistroFC;
          NucleusRH.Base.Tiempos_Trabajados.AdministracionFC.COMP_FC objCompFC;

          //Instancio el Objeto Batch
          NomadBatch objBatch;
          objBatch = NomadBatch.GetBatch("Administración de Francos Compensatorios", "Administración de Francos Compensatorios");
  
          solicitados = solicitados.FirstChild();
          NomadXML solicitud, registro;
          totRegs = solicitados.ChildLength;

          // Guarda la lista de Registros FC editados
          ArrayList listaRegistrosFC = new ArrayList();
         
          //Para validacion de cantidad total a solicitar
          ArrayList SaldosTotales = new ArrayList();


          //Validar fecha ya registrada en BD para cada legajo - si cantidadFecha==0 pasa la validacion
          for (solicitud = solicitados.FirstChild(); solicitud != null; solicitud = solicitud.Next())
          {
              NomadXML param = new NomadXML("PARAM");
              param.SetAttr("oi_personal_emp", solicitud.GetAttrInt("oi_personal_emp").ToString());
              param.SetAttr("f_comp", solicitud.GetAttrInt("f_comp").ToString());

              NomadXML fechas = new NomadXML();
              fechas.SetText(NomadEnvironment.QueryString(NucleusRH.Base.Tiempos_Trabajados.AdministracionFC.REGISTRO_FC.Resources.INFO_FECHA, param.ToString()));
              fechas = fechas.FirstChild();
              if (fechas.GetAttrInt("value").ToString() == "1")
              {
                  cantidadFecha++;
                  NomadXML legajos = new NomadXML();
                  legajos.SetText(NomadEnvironment.QueryString(NucleusRH.Base.Tiempos_Trabajados.AdministracionFC.REGISTRO_FC.Resources.INFO_LEGAJOS, param.ToString()));
                  legajos = legajos.FirstChild();
                  objBatch.Err("El legajo '" + legajos.GetAttr("e_numero_legajo").ToString() + "' tiene compensaciones registradas en la fecha '" + fechas.GetAttr("fecha_formato") + "' y no puede cargar otra compensación en esa fecha para el Banco de Horas ingresado");
              }
          }


          //Validar fecha repetida en xml para cada legajo
          Hashtable fechas1 = new Hashtable();
          Hashtable fechas2 = new Hashtable(); //para mostrar error - si la fechas2.Count == 0 pasa la validacion

          for (solicitud = solicitados.FirstChild(); solicitud != null; solicitud = solicitud.Next())
          {
              string clave = " ";
              clave = solicitud.GetAttrInt("oi_personal_emp").ToString() + solicitud.GetAttrInt("f_comp").ToString();

             if(!fechas1.ContainsKey(clave)) 
             {
                 fechas1.Add(clave, " ");
             }
             else 
             {
                 if(!fechas2.ContainsKey(solicitud.GetAttrInt("oi_personal_emp").ToString()))
                 {
                     fechas2.Add(solicitud.GetAttrInt("oi_personal_emp").ToString(), " ");
                 }
                 else 
                 {
                     continue;
                 }
             }   
          }

          foreach (string value in fechas2.Keys)
          {
              NomadXML param = new NomadXML("PARAM");
              param.SetAttr("oi_personal_emp", value);
              NomadXML legajos = new NomadXML();
              legajos.SetText(NomadEnvironment.QueryString(NucleusRH.Base.Tiempos_Trabajados.AdministracionFC.REGISTRO_FC.Resources.INFO_LEGAJOS, param.ToString()));
              legajos = legajos.FirstChild();
              objBatch.Err("No se puede repetir la Fecha de Compensación para el Legajo '" + legajos.GetAttr("e_numero_legajo").ToString() + "'");
          }


          //Recorriendo la lista de solicitudes - para validar la cantidad total a solicitar - si cantSaldoMayor == 0 pasa la validacion
          for (solicitud = solicitados.FirstChild(); solicitud != null; solicitud = solicitud.Next())
          {

              NomadXML xmlTotales = new NomadXML("PARAM");

              if (SaldosTotales.Count == 0)
              {
                  xmlTotales.SetAttr("oi_personal_emp", solicitud.GetAttrInt("oi_personal_emp"));
                  xmlTotales.SetAttr("n_cant_comp", solicitud.GetAttrInt("n_cant_comp"));
                  xmlTotales.SetAttr("n_cant_por_comp", solicitud.GetAttrInt("n_cant_por_comp"));
                  SaldosTotales.Add(xmlTotales);
              }
              else
              {
                    bool encontro = false;
                    int indice = 0;
                    for (int xml = 0; xml < SaldosTotales.Count; xml++)
                    {
                        NomadXML row = (NomadXML)SaldosTotales[xml];
                        if(row.GetAttrInt("oi_personal_emp")==solicitud.GetAttrInt("oi_personal_emp"))
                        {
                            indice = xml;
                            encontro = true;
                            break;
                        }
                    }

                    if (!encontro)
                    {
                        xmlTotales.SetAttr("oi_personal_emp", solicitud.GetAttrInt("oi_personal_emp"));
                        xmlTotales.SetAttr("n_cant_comp", solicitud.GetAttrInt("n_cant_comp"));
                        xmlTotales.SetAttr("n_cant_por_comp", solicitud.GetAttrInt("n_cant_por_comp"));
                        SaldosTotales.Add(xmlTotales);
                    }
                    else
                    {
                        NomadXML row = (NomadXML)SaldosTotales[indice];
                        row.SetAttr("n_cant_comp", row.GetAttrInt("n_cant_comp") + solicitud.GetAttrInt("n_cant_comp"));
                        SaldosTotales[indice] = row;
                    }
              }
          }

          for (int xml = 0; xml < SaldosTotales.Count; xml++)
          {
              NomadXML row = (NomadXML)SaldosTotales[xml];
              if(row.GetAttrInt("n_cant_comp")>row.GetAttrInt("n_cant_por_comp"))
              {
                  cantSaldoMayor++;
                  
                  NomadXML param = new NomadXML("PARAM");
                  param.SetAttr("oi_personal_emp", row.GetAttrInt("oi_personal_emp"));
                  NomadXML legajos = new NomadXML();
                  legajos.SetText(NomadEnvironment.QueryString(NucleusRH.Base.Tiempos_Trabajados.AdministracionFC.REGISTRO_FC.Resources.INFO_LEGAJOS, param.ToString()));
                  legajos = legajos.FirstChild();
                  objBatch.Err("Para el legajo '" + legajos.GetAttr("e_numero_legajo").ToString() + "' la cantidad total a solicitar no debe superar el saldo aprobado para el banco de horas");
              }
          }

          //si ningun legajo supera la cantidad de horas
          //si no se repite la f_comp para un legajo
          //si no existe ningun legajo con fecha ya registrada en BD
          if (cantSaldoMayor == 0 && fechas2.Count == 0 && cantidadFecha==0)
          {
              //Recorriendo la lista de solicitudes de compensaciones a tomar
              for (linea = 1, solicitud = solicitados.FirstChild(); solicitud != null; linea++, solicitud = solicitud.Next())
              {
                  objBatch.SetPro(0, 100, totRegs, linea);
                  objBatch.SetMess("Procesando la Linea " + linea + " de " + totRegs);

                  //Parametros
                  NomadXML param = new NomadXML("PARAM");
                  param.SetAttr("oi_personal_emp", solicitud.GetAttrInt("oi_personal_emp"));
                  param.SetAttr("oi_banco_hora", solicitud.GetAttrInt("oi_banco_hora"));
                  NomadXML xmlPorCompensar;

                  //Obtiene el total solicitado
                  int n_cant_comp = solicitud.GetAttrInt("n_cant_comp");

                  //Traigo los Registros FC a Compensar
                  xmlPorCompensar = new NomadXML();
                  xmlPorCompensar.SetText(NomadEnvironment.QueryString(NucleusRH.Base.Tiempos_Trabajados.AdministracionFC.REGISTRO_FC.Resources.FC_A_COMPENSAR, param.ToString()));
                  xmlPorCompensar = xmlPorCompensar.FirstChild();

                  //Lista de Registros editados por esta Compensacion
                  listaRegistrosFC = new ArrayList();

                  //Recorre los registros de los cuales va a tomar esa cantidad
                  for (registro = xmlPorCompensar.FirstChild(); registro != null; registro = registro.Next())
                  {
                      objRegistroFC = NucleusRH.Base.Tiempos_Trabajados.AdministracionFC.REGISTRO_FC.Get(registro.GetAttr("oi_registro_fc"));
                      if (n_cant_comp >= objRegistroFC.n_saldo)
                      {
                          objCompFC = new COMP_FC();
                          objCompFC.n_cant_comp = objRegistroFC.n_saldo;
                          if (objRegistroFC.n_consumidosNull)
                              objRegistroFC.n_consumidos = objCompFC.n_cant_comp;
                          else
                              objRegistroFC.n_consumidos = objRegistroFC.n_consumidos + objCompFC.n_cant_comp;
                          n_cant_comp = n_cant_comp - objRegistroFC.n_saldo;
                          objRegistroFC.n_saldo = 0;
                          objCompFC.f_comp = solicitud.GetAttrDateTime("f_comp");
                          if (solicitud.GetAttr("c_rango") != null) objCompFC.c_rango = solicitud.GetAttr("c_rango"); else objCompFC.c_rangoNull = true;
                          objRegistroFC.COMP_FC.Add(objCompFC);
                          listaRegistrosFC.Add(objRegistroFC);
                      }
                      else
                      {
                          objRegistroFC.n_saldo = objRegistroFC.n_saldo - n_cant_comp;
                          objCompFC = new COMP_FC();
                          objCompFC.f_comp = solicitud.GetAttrDateTime("f_comp");
                          objCompFC.n_cant_comp = n_cant_comp;
                          if (objRegistroFC.n_consumidosNull)
                              objRegistroFC.n_consumidos = objCompFC.n_cant_comp;
                          else
                              objRegistroFC.n_consumidos = objRegistroFC.n_consumidos + objCompFC.n_cant_comp;
                          n_cant_comp = 0;
                          if (solicitud.GetAttr("c_rango") != null) objCompFC.c_rango = solicitud.GetAttr("c_rango"); else objCompFC.c_rangoNull = true;
                          objRegistroFC.COMP_FC.Add(objCompFC);
                          listaRegistrosFC.Add(objRegistroFC);
                      }

                      if (n_cant_comp == 0)
                          break;

                  }

                  //Guardo la lista de Registros FC editados por la compensacion actual
                  try
                  {
                      NomadEnvironment.GetCurrentTransaction().Begin();
                      foreach (Object reg in listaRegistrosFC)
                      {
                          NucleusRH.Base.Tiempos_Trabajados.AdministracionFC.REGISTRO_FC registroFC = (NucleusRH.Base.Tiempos_Trabajados.AdministracionFC.REGISTRO_FC)reg;
                          NomadEnvironment.GetCurrentTransaction().SaveRefresh(registroFC);
                      }
                      NomadEnvironment.GetCurrentTransaction().Commit();
                      compensaciones++;
                  }
                  catch (Exception e)
                  {
                      NomadEnvironment.GetCurrentTransaction().Rollback();
                      objBatch.Err("No se pudo guardar correctamente la compensación del día: " + solicitud.GetAttrDateTime("f_comp").ToString("dd/MM/yyyy") + "\n Error generado: " + e.Message);
                      errores++;
                  }

              }

              //Resultados
              objBatch.Log("Compensaciones generadas correctamente: " + compensaciones.ToString());
              objBatch.Log("Compensaciones no generadas por error: " + errores.ToString());
              objBatch.Log("Total de compensaciones procesadas: " + totRegs.ToString());
              objBatch.Log("Finalizado...");

          }
          else
          {
              objBatch.Err("No se pudo guardar correctamente ninguna de las compensaciones");
          }
      }

      public static void RecalcularFC(Nomad.NSystem.Proxy.NomadXML param)
      {
          //Inicializando variables
          NomadXML xmlfc, xmlRecalc;
          int totRegs, linea = 0, registros = 0, errores = 0, nocalc = 0, n_cant_comp;
          NucleusRH.Base.Tiempos_Trabajados.AdministracionFC.REGISTRO_FC objFC;
          NucleusRH.Base.Tiempos_Trabajados.AdministracionFC.REGISTRO_FC objRegistroFC;

          //Instancio el Objeto Batch
          NomadBatch objBatch;
          objBatch = NomadBatch.GetBatch("Recalcular Francos Compensatorios", "Recalcular Francos Compensatorios");
          if (param.isDocument)
              param = param.FirstChild();
          totRegs = param.ChildLength;

          //Recorriendo la lista de registros para recalcular
          for (linea = 1, xmlfc = param.FirstChild(); xmlfc != null; linea++, xmlfc = xmlfc.Next())
          {
              objBatch.SetPro(0, 100, totRegs, linea);
              objBatch.SetMess("Procesando FC " + linea + " de " + totRegs);
              NomadEnvironment.GetTrace().Info("xmlfc--> :" + xmlfc.ToString());

              objFC = NucleusRH.Base.Tiempos_Trabajados.AdministracionFC.REGISTRO_FC.Get(xmlfc.GetAttr("id"));

              NomadXML par = new NomadXML("PARAM");
              par.SetAttr("oi_personal_emp", objFC.oi_personal_emp);
              par.SetAttr("oi_banco_hora", objFC.oi_banco_hora);
             
              //Obtiene el total solicitado
              n_cant_comp = objFC.n_saldo;
              n_cant_comp = -n_cant_comp;

              par.SetAttr("n_saldo", n_cant_comp);

              //Traigo los Registros FC a Recalcular
              xmlRecalc = new NomadXML();
              xmlRecalc.SetText(NomadEnvironment.QueryString(NucleusRH.Base.Tiempos_Trabajados.AdministracionFC.REGISTRO_FC.Resources.FC_A_RECALCULAR, par.ToString()));
              xmlRecalc = xmlRecalc.FirstChild();
              if (xmlRecalc.GetAttr("noreg") == "1")
              {
                  nocalc++;
                  objBatch.Wrn("No existe saldo para recalcular el registro del día: " + objFC.f_registro_fc.ToString("dd/MM/yyyy") + " con Saldo -" + n_cant_comp.ToString() + ".");
                  continue;
              }

              objRegistroFC = NucleusRH.Base.Tiempos_Trabajados.AdministracionFC.REGISTRO_FC.Get(xmlRecalc.GetAttr("id"));
              objRegistroFC.n_saldo -= n_cant_comp;
              //Asigno la fecha que indica la relación entre registros
              objFC.f_registro_s = objRegistroFC.f_registro_fc;
              objFC.n_saldo = 0;

              //Guardo
              try
              {
                  NomadEnvironment.GetCurrentTransaction().Begin();
                  NomadEnvironment.GetCurrentTransaction().SaveRefresh(objRegistroFC);
                  NomadEnvironment.GetCurrentTransaction().SaveRefresh(objFC);
                  NomadEnvironment.GetCurrentTransaction().Commit();
                  registros++;
                  objBatch.Log("El registro del día: " + objRegistroFC.f_registro_fc.ToString("dd/MM/yyyy") + " con Saldo -" + n_cant_comp.ToString() + " se procesó correctamente.");
              }
              catch (Exception e)
              {
                  NomadEnvironment.GetCurrentTransaction().Rollback();
                  objBatch.Err("No se pudo guardar correctamente el Registro del día: " + objFC.f_registro_fc.ToString("dd/MM/yyyy") + "\n Error generado: " + e.Message);
                  errores++;
              }

          }

          //Resultados
          objBatch.Log("Registros recalculados correctamente: " + registros.ToString());
          objBatch.Log("Registros no recalculados por no disponer de saldo: " + nocalc.ToString());
          objBatch.Log("Registros no recalculados por error: " + errores.ToString());
          objBatch.Log("Total de Registros procesados: " + totRegs.ToString());
          objBatch.Log("Finalizado...");
      }

      public static void BlanquearSaldo(Nomad.NSystem.Proxy.NomadXML param)
      {
          //Inicializando variables
          NomadXML xmlfc;
          int totRegs, linea = 0, registros = 0, errores = 0;
          NucleusRH.Base.Tiempos_Trabajados.AdministracionFC.REGISTRO_FC objFC;

          //Instancio el Objeto Batch
          NomadBatch objBatch;
          objBatch = NomadBatch.GetBatch("Blanquear Saldos", "Blanquear Saldos");

          // Guarda la lista de Registros FC editados
          ArrayList listaRegistrosFC = new ArrayList();
          if (param.isDocument)
              param = param.FirstChild();
          totRegs = param.ChildLength;

          //Lista de Registros a blanquear
          listaRegistrosFC = new ArrayList();

          //Recorriendo la lista de registros para recalcular
          for (linea = 1, xmlfc = param.FirstChild(); xmlfc != null; linea++, xmlfc = xmlfc.Next())
          {
              objBatch.SetPro(0, 100, totRegs, linea);
              objBatch.SetMess("Blanqueando saldo de FC " + linea + " de " + totRegs);

              objFC = NucleusRH.Base.Tiempos_Trabajados.AdministracionFC.REGISTRO_FC.Get(xmlfc.GetAttr("id"));
               
              objFC.n_saldo = 0;
              objFC.f_blanqueo = DateTime.Now;
               
              //Guardo la lista de Registros FC editados por la compensacion actual
              try
              {
                  NomadEnvironment.GetCurrentTransaction().Begin();
                  NomadEnvironment.GetCurrentTransaction().SaveRefresh(objFC);
                  NomadEnvironment.GetCurrentTransaction().Commit();
                  objBatch.Log("El registro del día: " + objFC.f_registro_fc.ToString("dd/MM/yyyy") + " se procesó correctamente.");
                  registros++;
              }
              catch (Exception e)
              {
                  NomadEnvironment.GetCurrentTransaction().Rollback();
                  objBatch.Err("No se pudo guardar correctamente el Registro del día: " + objFC.f_registro_fc.ToString("dd/MM/yyyy") + "\n Error generado: " + e.Message);
                  errores++;
              }

          }

          //Resultados
          objBatch.Log("Registros blanqueados correctamente: " + registros.ToString());
          objBatch.Log("Registros no blanqueados por error: " + errores.ToString());
          objBatch.Log("Total de Registros procesados: " + totRegs.ToString());
          objBatch.Log("Finalizado...");
      }
   
  }

}

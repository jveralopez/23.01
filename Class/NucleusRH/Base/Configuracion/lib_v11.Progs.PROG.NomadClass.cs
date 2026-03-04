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
using System.Threading;

namespace NucleusRH.Base.Configuracion.Progs
{
    //////////////////////////////////////////////////////////////////////////////////
  //Clase Programaciónes
    public partial class PROG : Nomad.NSystem.Base.NomadObject
    {
        public static void Activate(string pOI)
        {
            //Activa la alerta
            NomadLog.Info("Inicia el método PROG.Activate.");

            NucleusRH.Base.Configuracion.Progs.PROG objPRO;

            //Obtiene el DDO y lo activa
            objPRO = NucleusRH.Base.Configuracion.Progs.PROG.Get(pOI);
            if (objPRO == null)
            {
                NomadLog.Error("No se encontró el OI de PROG '" + pOI + "'.");
                return;
            }

            //Cambio el estado y Programo la alerta.
            objPRO.c_estado = "A";
            NucleusRH.Base.Configuracion.Progs.PROG.Schedule(objPRO,DateTime.Now);
        }
        public static void Inactivate(string pOI)
        {
            //Inactiva la alerta
            NomadLog.Info("Inicia el método ALERTA.Inactivate.");

            NucleusRH.Base.Configuracion.Progs.PROG objPRO;

            //Obtiene el DDO y lo inactiva
            objPRO = NucleusRH.Base.Configuracion.Progs.PROG.Get(pOI);

            if (objPRO == null)
            {
                NomadLog.Error("No se encontró el OI de alerta '" + pOI + "'.");
                return;
            }

            objPRO.c_estado = "I";
            objPRO.f_pro_ejecucionNull = true;
            NomadEnvironment.GetCurrentTransaction().Save(objPRO);
        }
        public static void Schedule(NucleusRH.Base.Configuracion.Progs.PROG pobjPROG,DateTime fCalculo)
        {
            //Proxima Corrida
            int countE = 0;
            bool runNext = false;
            System.DateTime dteNext, dteDET, NOW, NOWDATE;

            NOW = fCalculo;
            NOWDATE = NOW.Date;
            dteNext = dteDET = NOW;

            //Recorro los detalles y calculo la proxima Corrida
            if (pobjPROG.c_estado == "A" && (pobjPROG.f_finNull || NOWDATE <= pobjPROG.f_fin))
            {
                if (NOW < pobjPROG.f_inicio) NOW = pobjPROG.f_inicio;

                foreach (PROG_DET objDet in pobjPROG.DETALLES)
                {
                    if (objDet.c_tipo == "E")
            countE++;
          else
                    if (objDet.NextRun(NOW, ref dteDET))
                        if (dteDET < dteNext || !runNext)
                        {
                            dteNext = dteDET;
                            runNext = true;
                        }
                }
            }
            //Si ningun detalle es de tipo E, Programo la Proxima Corrida
            //En caso contrario, no se programa ni se deshabilita
            if (runNext)
            {
                pobjPROG.f_pro_ejecucion = dteNext;

                if (ExisteProgramacion(pobjPROG.id))
                    EliminarProgramacion(pobjPROG.id);
                
                int reintentos = 10;
                for (int intento = 1; intento <= reintentos; intento++)
                {        
                    if (!ExisteProgramacion(pobjPROG.id))
                    {
                        if (intento >= 2)
                        {
                            Thread.Sleep(5000);
                            NomadLog.Debug("Espero 5 segundos");
                        }
                        CrearProgramacion(pobjPROG, dteNext);
                    }                        
                    else
                        break;
                    NomadLog.Debug("Intento creacion de archivo de programacion, intento: " + intento);
            
                }

            }
      else if (countE == 0)
            {
                pobjPROG.f_pro_ejecucionNull = true;
                pobjPROG.c_estado = "I";
            }

            //Guardo el DDO
            NomadEnvironment.GetCurrentTransaction().SaveRefresh(pobjPROG);
        }

        private static void EliminarProgramacion(int oi_programacion)
        {
            File.Delete(NomadProxy.GetProxy().RunPath + "Nomad\\TIMERS\\PROGRAMADOR_" + oi_programacion + ".TIMER.XML");
        }

        private static bool ExisteProgramacion(int oi_programacion)
        {
            return File.Exists(NomadProxy.GetProxy().RunPath + "Nomad\\TIMERS\\PROGRAMADOR_" + oi_programacion + ".TIMER.XML");
        }

        private static void CrearProgramacion(NucleusRH.Base.Configuracion.Progs.PROG pobjPROG, System.DateTime dteNext)
        {
            RPCService objRPC = NomadProxy.GetProxy().RPCService();
            objRPC.AddParam(new RPCParam("pOI", "IN", pobjPROG.Id));
            objRPC.AddParam(new RPCParam("pEVE", "IN", "Nomad.NSystem.Object", "Nomad.NSystem.Proxy.NomadXML", "<EVE/>"));
            objRPC.Execute("Programador_" + pobjPROG.Id, 1, dteNext, "SCHEDULER", "NucleusRH.Base.Configuracion.Progs.PROG", "ExecuteNow");
        }
    public static void Execute(string pOI)
        {
      //Realizo la Ejecucion
      RPCService objRPC = NomadProxy.GetProxy().RPCService();
      objRPC.AddParam(new RPCParam("pOI", "IN", pOI));
      objRPC.AddParam(new RPCParam("pEVE", "IN", "Nomad.NSystem.Object", "Nomad.NSystem.Proxy.NomadXML", "<EVE force-execute=\"1\" />"));
      objRPC.Execute("Programador_TEST_"+pOI, 1, DateTime.Now, "SCHEDULER", "NucleusRH.Base.Configuracion.Progs.PROG", "ExecuteNow");
    }

    public static void ExecuteNow(string pOI, NomadXML pEVE)
    {
      string MyMETHOD="(unknow)";
      int reintentos = 5;

      for (int intento = 1; intento <= reintentos; intento++)
      {
          try
          {
              NomadLog.Debug("Intento: " + intento);
              //Agrego el XML de EVENTO
              if (pEVE != null && pEVE.isDocument) pEVE = pEVE.FirstChild();

              //Obtengo la Programacion
              PROG objPROG = PROG.Get(pOI);
              if (objPROG.c_estado == "I")
              {
                  if (pEVE == null) return;
                  if (!pEVE.GetAttrBool("force-execute")) return;
              }

              MyMETHOD = objPROG.d_class_name + "." + objPROG.d_class_method;

              //Actualizo los campos de corrida
              objPROG.f_ult_ejecucion = DateTime.Now;

              //Obtengo la CLASE
              Type MyClassType = Nomad.Base.Scheduler.Tareas.TAREA.FindType(objPROG.d_class_name + "." + objPROG.d_class_method);
              if (MyClassType == null) throw new Exception("Clase '" + objPROG.d_class_name + "' no encontrada");

              //Programo la Proxima ejecucion
              NucleusRH.Base.Configuracion.Progs.PROG.Schedule(objPROG, DateTime.Now.AddMinutes(5));

              //Metodo
              object[] args = new object[2];
              args[0] = objPROG.oi_class_id;
              args[1] = pEVE;

              NomadLog.Debug("MyMETHOD " + MyMETHOD);

              MyClassType.InvokeMember(objPROG.d_class_method, BindingFlags.Public | BindingFlags.InvokeMethod | BindingFlags.Static, null, null, args);
              break;
          }
          catch (Exception ex)
          {
              NomadLog.Debug("Error despues del InvokeMember");

              NomadException myEX = NomadException.NewInternalException("PROG.ExecuteNow-FAILED", ex);
              myEX.SetValue("pOI", pOI);
              myEX.SetValue("pEVE", pEVE == null ? "(null)" : pEVE.ToString());
              myEX.SetValue("MyMETHOD", MyMETHOD);
              myEX.Dump();

              //Guardo en el LOG
              NucleusRH.Base.Configuracion.LogEventos.LOGEVENTO.Call("NucleusRH.Base.Configuracion.PROG.EXECUTE-ERROR", myEX.Id, MyMETHOD);

              Thread.Sleep(2000);
          }
      }
         
    }
        public static void Save(NucleusRH.Base.Configuracion.Progs.PROG pobjPROG)
        {
            //Actualizo la Descripcion
            if (pobjPROG.DETALLES.Count == 0)
            {
                pobjPROG.d_prog = "No se especifico ninguna programación.";
            } else if (pobjPROG.DETALLES.Count > 1)
            {
                pobjPROG.d_prog = "Programación múltiple, para más información ver el detalle.";
            } else
            {
                pobjPROG.d_prog = ((PROG_DET)pobjPROG.DETALLES[0]).d_prog_det;
            }

            //Actualizo la Programacion
            PROG.Schedule(pobjPROG,DateTime.Now);
        } 

        public static void ReactivateProgs()
        {
            NomadXML xmlOIItems = NomadEnvironment.QueryNomadXML(PROG.Resources.QRY_VENCIDAS,"");
            xmlOIItems =  xmlOIItems.FirstChild();
            PROG objProg;
            DateTime ultimaHora = DateTime.Now;
            for (NomadXML xmlRowOI = xmlOIItems.FirstChild(); xmlRowOI != null; xmlRowOI = xmlRowOI.Next())
            {
                objProg = PROG.Get(xmlRowOI.GetAttr("id"));
                
                ultimaHora = ultimaHora.AddMinutes(2);
                objProg.f_pro_ejecucion = ultimaHora;

                RPCService objRPC = NomadProxy.GetProxy().RPCService();
                objRPC.AddParam(new RPCParam("pOI", "IN", objProg.Id));
                objRPC.AddParam(new RPCParam("pEVE", "IN", "Nomad.NSystem.Object", "Nomad.NSystem.Proxy.NomadXML", "<EVE/>"));
                objRPC.Execute("Programador_" + objProg.Id, 1, ultimaHora, "SCHEDULER", "NucleusRH.Base.Configuracion.Progs.PROG", "ExecuteNow");

                NomadEnvironment.GetCurrentTransaction().Save(objProg);
            }
			
        }

        public static void CheckProgs(NomadXML xmlParam)
        {
            xmlParam = xmlParam.FirstChild();

            int minutosParaPrimerProgramacion = xmlParam.GetAttrInt("minutosParaPrimerProgramacion");
            int minutosEntreProgramaciones = xmlParam.GetAttrInt("minutosEntreProgramaciones");
            int cantidadDeReprogramaciones = xmlParam.GetAttrInt("cantidadDeReprogramaciones");

            DateTime horaUltimaProgramacion = DateTime.Now.AddMinutes(minutosParaPrimerProgramacion);
        
            for(int i=0; i<cantidadDeReprogramaciones;i++)
            {
                
                RPCService objRPC = NomadProxy.GetProxy().RPCService();
                objRPC.Execute("CheckProgs_" + i, 1, horaUltimaProgramacion, "SCHEDULER", "NucleusRH.Base.Configuracion.Progs.PROG", "ReactivateProgs");

                horaUltimaProgramacion = horaUltimaProgramacion.AddMinutes(minutosEntreProgramaciones);
                
            }
        }
    }
}



using System;
using System.Xml;
using System.IO;
using System.Collections;

using Nomad.NSystem.Document;
using Nomad.NSystem.Proxy;
using Nomad.NSystem.Base;
using Nomad.NSystem.Functions;
using System.Diagnostics;
using System.Collections.Generic;

namespace NucleusRH.Base.Liquidacion.LiquidacionDDO
{

    //////////////////////////////////////////////////////////////////////////////////
    //Clase Liquidaciones2
    public partial class LIQUIDACION_DDO
    {
        public static void RemoteStartLiq(string oi_liquidacion, Nomad.NSystem.Proxy.NomadXML ois_liquidar)
        {
            try
            {
                NucleusRH.Base.Liquidacion.RHLiq.RHLiqControl rhliq = new NucleusRH.Base.Liquidacion.RHLiq.RHLiqControl();

                if (!NomadProxy.GetProxy().Lock().LockOBJ("Liquidacion:id=" + oi_liquidacion))
                {
                    NomadBatch MyLOG = new NomadBatch("Liquidacion", "Liquidacion", 0, 100);
                    MyLOG.Err("La liquidación esta siendo **Procesada** en este momento.\\\\ Intente más tarde, si el problema persiste consulte con su administrador.");
                    return;
                }

                string list_oi_per_emp = "";
                for (NomadXML MyCUR = ois_liquidar.FirstChild().FirstChild().FirstChild(); MyCUR != null; MyCUR = MyCUR.Next())
                {
                    list_oi_per_emp += "," + MyCUR.GetAttr("VALUES");
                }

                rhliq.StartLiq(int.Parse(oi_liquidacion), list_oi_per_emp.Substring(1));
                return;
            }
            catch (Exception e)
            {
                NomadException ex = NomadException.NewInternalException("RemoteStartLiq", e);
                ex.SetValue("oi_liquidacion", oi_liquidacion);
                ex.SetValue("ois_liquidar", ois_liquidar.ToString());
                throw ex;
            }

        }

        public static void RemoteGenerarInt(string oi_liquidacion)
        {
            try
            {
                NucleusRH.Base.Liquidacion.RHLiq.RHLiqControl rhliq = new NucleusRH.Base.Liquidacion.RHLiq.RHLiqControl();

                if (!NomadProxy.GetProxy().Lock().LockOBJ("Liquidacion:id=" + oi_liquidacion))
                {
                    NomadBatch MyLOG = new NomadBatch("Liquidacion", "Liquidacion", 0, 100);
                    MyLOG.Err("La liquidación de interface de la liquidacion esta siendo **Procesada** en este momento.\\\\ Intente más tarde, si el problema persiste consulte con su administrador.");
                    return;
                }

                rhliq.GenerarInt(int.Parse(oi_liquidacion));

                return;
            }
            catch (Exception e)
            {
                NomadException ex = NomadException.NewInternalException("RemoteStartLiq", e);
                ex.SetValue("oi_liquidacion", oi_liquidacion);

                throw ex;
            }

        }

        public static void RemoteCerrarLiq(string oi_liquidacion, bool l_delPerNoLiq, bool l_compPerChange, bool l_compConcChange, bool l_compEmpChange, bool l_CerrarLiquidacion, bool l_ActAcumuladores)
        {
            NucleusRH.Base.Liquidacion.RHLiq.RHLiqControl rhliq = new NucleusRH.Base.Liquidacion.RHLiq.RHLiqControl();

            if (!NomadProxy.GetProxy().Lock().LockOBJ("Liquidacion:id=" + oi_liquidacion))
            {
                NomadBatch MyLOG = new NomadBatch("Liquidacion", "Liquidacion", 0, 100);
                MyLOG.Err("La liquidación esta siendo **Procesada** en este momento.\\\\ Intente más tarde, si el problema persiste consulte con su administrador.");
                return;
            }

            rhliq.CloseLiq(int.Parse(oi_liquidacion), l_delPerNoLiq, l_compPerChange, l_compConcChange, l_compEmpChange, l_CerrarLiquidacion, l_ActAcumuladores);
            return;
        }

        public static void RemoteDebug(Nomad.NSystem.Proxy.NomadXML xmlPARAM)
        {
            NucleusRH.Base.Liquidacion.RHLiq.RHLiqControl.DebugLiq(xmlPARAM.FirstChild());
            return;
        }

        public static void RemoteDebugAuto()
        {
            NucleusRH.Base.Liquidacion.RHLiq.RHLiqControl.DebugLiq((new NomadXML("<DATA CODIFTEMP=\"1\" CONCTEMP=\"1\" VARTEMP=\"1\" LIQTEMP=\"1\" EMPTEMP=\"1\" RECTEMP=\"1\" PELTEMP=\"1\" PERTEMP=\"1\" />")).FirstChild());
            return;
        }

    public static void RemoteReopenLiq(string oi_liquidacion, NomadXML ids)
    {
      try
      {
        NucleusRH.Base.Liquidacion.RHLiq.RHLiqControl rhliq = new NucleusRH.Base.Liquidacion.RHLiq.RHLiqControl();

        if (!NomadProxy.GetProxy().Lock().LockOBJ("Liquidacion:id=" + oi_liquidacion))
        {
          NomadBatch MyLOG = new NomadBatch("Liquidacion", "Liquidacion", 0, 100);
          MyLOG.Err("La liquidación esta siendo **Procesada** en este momento.\\\\ Intente más tarde, si el problema persiste consulte con su administrador.");
          return;
        }

        string list_oi_per_emp = "";
        for (NomadXML MyCUR = ids.FirstChild().FirstChild().FirstChild(); MyCUR != null; MyCUR = MyCUR.Next())
        {
          list_oi_per_emp += "," + MyCUR.GetAttr("VALUES");
        }

                rhliq.ReopenLiq(int.Parse(oi_liquidacion), list_oi_per_emp.Substring(1));
        return;
      }
      catch (Exception e)
      {
        NomadException ex = NomadException.NewInternalException("RemoteStartLiq", e);
        ex.SetValue("oi_liquidacion", oi_liquidacion);
        ex.SetValue("ois_liquidar", ids.ToString());
        throw ex;
      }

    }

        public static void DepurarEstado(string oi_liquidacion)
        {
            NomadBatch batch = NomadBatch.GetBatch("Depurar Estado de liquidacion", "Depurar Estado de liquidacion");

            batch.SetMess("Iniciando depuracion de estado de liquidacion");
            NomadXML xmlLiquidacionDDo = NomadEnvironment.GetProxy().SQLService().GetXML("CLASS.NucleusRH.Base.Liquidacion.LiquidacionDDO.LIQUIDACION_DDO.QRY_LIQ_FINAL", "<PARAM oi_liquidacion=\"" + oi_liquidacion + "\" />");

            batch.SetPro(10);
            
            if (xmlLiquidacionDDo.FirstChild() != null && xmlLiquidacionDDo.ChildLength > 1)
            {
                int cant = 0;
                int i = 0;
                for (NomadXML MyCUR = xmlLiquidacionDDo.FirstChild(); MyCUR != null; MyCUR = MyCUR.Next())
                {
                    LIQUIDACION_DDO liqddo = LIQUIDACION_DDO.Get(MyCUR.GetAttr("oi_liquidacion_ddo"));
                    if (i == 0)
                    {
                        liqddo.Estado = "F";
                    }
                    else
                    {
                        liqddo.Estado = "D";
                        cant++;
                    }
                    NomadEnvironment.GetCurrentTransaction().Save(liqddo);
                    i++;
                }
                batch.SetPro(100);
                batch.Log("Se depuraron "+cant+" estados de liquidacion");
                batch.Log("Proceso finalizado con exito");

            } 
          
        }

        public static void ComprimirArchivosRecibo()
        {
            ComprimirArchivosReciboPeriodo("");
        }
        public static void ComprimirArchivosReciboPeriodo(string e_periodo)
        {
            NomadBatch batch = NomadBatch.GetBatch("Comprimir archivos de recibo", "Comprimir archivos de recibo");

            batch.SetMess("Iniciando Comprimir archivos de recibo");

            string e_periodo_parametro = e_periodo;
            int cantidadPeriodosConservar = 6;
            int cantidadPeriodosAComprimir = 1;
            try
            {
                cantidadPeriodosConservar = int.Parse(NomadEnvironment.QueryValue("ORG26_PARAMETROS", "d_valor", "c_parametro", "depurRecPerCon", "", false));
            }
            catch { }
            try
            {
                if(e_periodo_parametro == "")
                    cantidadPeriodosAComprimir = int.Parse(NomadEnvironment.QueryValue("ORG26_PARAMETROS", "d_valor", "c_parametro", "cantPerCompxCorr", "", false));
            }
            catch { }

            DateTime hoy = DateTime.Now;
            DateTime fechaHasta = hoy.AddMonths(-1 * cantidadPeriodosConservar);
          
            int periodoActual = int.Parse(hoy.Year.ToString() + hoy.Month.ToString("00"));
            int periodoHasta = int.Parse(fechaHasta.Year.ToString() + fechaHasta.Month.ToString("00"));
            string directorioRecibos = NomadProxy.GetProxy().RunPath + "NOMAD\\RECIBOS\\" + NomadProxy.GetProxy().AppName + "\\";

            if (e_periodo_parametro == "")
            {
                NomadXML xmlPeriodos = NomadEnvironment.QueryNomadXML(Resources.QRY_PERIODOS, "<DATA periodoHasta='" + periodoHasta + "'  /> ").FirstChild();
                e_periodo = xmlPeriodos.FirstChild().GetAttr("e_periodo");
            }
            
            int cantidad = 0;
            while (e_periodo != "" && cantidad < cantidadPeriodosAComprimir && int.Parse(e_periodo) < periodoHasta)
            {
                if (ComprimirPeriodo(e_periodo))
                {
                    NucleusRH.Base.Liquidacion.RHLiq.RHLiqControl.DebugLiq((new NomadXML("<DATA CODIFTEMP=\"0\" CONCTEMP=\"0\" VARTEMP=\"0\" LIQTEMP=\"0\" EMPTEMP=\"0\" RECTEMP=\"1\" e_periodo=\"" + e_periodo + "\" PELTEMP=\"0\" PERTEMP=\"0\" />")).FirstChild());

                    NomadXML xmlRecibos = NomadEnvironment.QueryNomadXML(Resources.QRY_RECIBOS, "<DATA e_periodo='" + e_periodo + "' /> ").FirstChild();

                    if (xmlRecibos.ChildLength > 0)
                    {
                        
                        Hashtable archivosAComprimir = new Hashtable();
                        for (NomadXML xmlRec = xmlRecibos.FirstChild(); xmlRec != null; xmlRec = xmlRec.Next())
                        {
                            string fileName = xmlRec.GetAttr("oi_tot_liq_per") + ".recibo.xml";
                            if (File.Exists(directorioRecibos + fileName))
                                archivosAComprimir.Add(fileName, fileName);
                                //sourceFileName += fileName + " ";
                        }
                        if (archivosAComprimir.Count > 0 )
                        {
                            //sourceFileName = sourceFileName.Substring(0, sourceFileName.Length - 1);

                            string targetFileName = e_periodo + ".7z";

                            bool resultado = Nomad.Base.Report.GeneralReport.REPORT.Comprimir7Zip(archivosAComprimir, targetFileName, directorioRecibos);

                            if (resultado)
                            {
                                GuardarEnArchivo(e_periodo, "C");

                                for (NomadXML xmlRec = xmlRecibos.FirstChild(); xmlRec != null; xmlRec = xmlRec.Next())
                                {
                                    string fileName = xmlRec.GetAttr("oi_tot_liq_per") + ".recibo.xml ";
                                    if (File.Exists(directorioRecibos + fileName))
                                        File.Delete(directorioRecibos + fileName);
                                }
                            }
                            cantidad++;
                        }
                        
                       
                    }
                    
                }
                if (e_periodo_parametro != "")
                    break;
                int periodo = int.Parse(e_periodo);
                DateTime nuevaFecha = new DateTime(periodo / 100, periodo % 100, 1);
                nuevaFecha = nuevaFecha.AddMonths(1);
                e_periodo = nuevaFecha.Year.ToString() + nuevaFecha.Month.ToString("00");
            }
            if (cantidad == 0)
                BorrarArchivosHuerfanos();
 
        }

        public static void BorrarArchivosHuerfanos()
        {
           NomadBatch batch = NomadBatch.GetBatch("Borrando archivos huerfanos", "Borrando archivos huerfanos");
         
           string[] archivos = Directory.GetFiles(NomadProxy.GetProxy().RunPath + "NOMAD\\RECIBOS\\" + NomadProxy.GetProxy().AppName + "\\","*.xml");
           NomadLog.Debug("Archivos de recibos: "+archivos.Length);

           List<string[]> paquetes = new List<string[]>();

           int totalPorPaquete = archivos.Length < 1000 ? archivos.Length : 1000;
           int cantidadDePaquetes = archivos.Length < 1000 ? 1 :  (archivos.Length / totalPorPaquete) + 1;
           int inicio = 0;
          
           for (int i = 0; i < cantidadDePaquetes; i++)
           {
               string[] segmento = new string[totalPorPaquete];
               Array.Copy(archivos, inicio, segmento, 0, totalPorPaquete);
               paquetes.Add(segmento);
               inicio = totalPorPaquete;
               totalPorPaquete = (archivos.Length - (i + 1) * totalPorPaquete) < 1000 ? (archivos.Length - (i + 1) * totalPorPaquete): 1000;
           }


           int cantTotal = 0;
           foreach (string[] paq in paquetes)
           {
               NomadXML xmlDatos = new NomadXML("DATOS");
               Hashtable arch = new Hashtable();
               foreach (string a in paq)
               {
                   string oi = a.Substring(a.LastIndexOf('\\') + 1, a.Length - a.LastIndexOf('\\') - 12);
                   arch.Add(oi, a);
                   xmlDatos.AddTailElement(new NomadXML("<ROW oi='" + oi + "' />"));
               }
               xmlDatos = NomadEnvironment.QueryNomadXML(Resources.QRY_ARCHIVOS, xmlDatos.ToString()).FirstChild();

               NomadLog.Debug("Datos archivos: " + xmlDatos.ToString());

               int cant = 0;
               for (NomadXML xmld = xmlDatos.FirstChild(); xmld != null; xmld = xmld.Next())
               {
                   if (xmld.GetAttr("eliminar") == "1")
                   {
                       cant+=cantTotal;
                       batch.Log("Borrando archivo: " + xmld.GetAttr("oi"));
                       File.Delete(arch[xmld.GetAttr("oi")].ToString());
                   }
               }
           }
           batch.Log(cantTotal + " archivos huerfanos borrados");
        }

        private static void GuardarEnArchivo(string e_periodo,string tipo)
        {
            string contrario = (tipo == "C") ? "E" : "C";
     
            string directorioRecibos = NomadProxy.GetProxy().RunPath + "NOMAD\\RECIBOS\\" + NomadProxy.GetProxy().AppName + "\\";
            string pathComprimidos = directorioRecibos + "comprimidos.txt";
             
            if (!File.Exists(pathComprimidos))
                File.Create(pathComprimidos).Close();
            bool existe = false;
            string[] comprimidos = File.ReadAllLines(pathComprimidos);
            if (comprimidos.Length > 0)
            {
                foreach (string c in comprimidos)
                {
                    StreamReader reader = new StreamReader(pathComprimidos);
                    string readedData = reader.ReadToEnd();
                    reader.Close();
                    StreamWriter writer = null;       
                    string[] split = c.Split(';');
                    if (split[0] == e_periodo)
                    {
                        existe = true;
                        if (split[1] == contrario)
                        {
                            readedData = readedData.Replace(e_periodo + ";" + contrario, e_periodo + ";" + tipo);
                            writer = new StreamWriter(pathComprimidos, false);
                            writer.Write(readedData);
                            writer.Close();
                        }
                        
                    }  
                }
                if (!existe)
                {
                    StreamWriter writer = new StreamWriter(pathComprimidos, true);
                    writer.WriteLine(e_periodo + ";" + tipo);
                    writer.Close();
                }
            }
            else
            {
                StreamWriter writer = new StreamWriter(pathComprimidos, false);
                writer.WriteLine(e_periodo + ";" + tipo);
                writer.Close();
            }
            
        }

        private static bool ComprimirPeriodo(string e_periodo)
        {
           string directorioRecibos = NomadProxy.GetProxy().RunPath + "NOMAD\\RECIBOS\\" + NomadProxy.GetProxy().AppName + "\\";

           if (File.Exists(directorioRecibos + e_periodo + ".7z"))
           {
               return false;
           }
           else
           {
               string pathComprimidos = directorioRecibos + "comprimidos.txt";
               if (File.Exists(pathComprimidos))
               {
                   string[] comprimidos = File.ReadAllLines(pathComprimidos);
                   foreach (string c in comprimidos)
                   {
                       string[] split = c.Split(';');
                       if (split[0] == e_periodo)
                       {
                           return split[1] == "E"; 
                       }
                   }
               }
           }
           return true;
        }

        public static void DescomprimirArchivosRecibo(string e_periodo)
        {          
            string directorioRecibos = NomadProxy.GetProxy().RunPath + "NOMAD\\RECIBOS\\" + NomadProxy.GetProxy().AppName + "\\";
         
            if(File.Exists(directorioRecibos + e_periodo +".7z"))
            {
                bool resultado = Nomad.Base.Report.GeneralReport.REPORT.Descomprimir7Zip(e_periodo +".7z", directorioRecibos);
                if(resultado)
                {
                    File.Delete(directorioRecibos + e_periodo +".7z");
                    GuardarEnArchivo(e_periodo,"E");
        
                }
            }

        }

        
    }
}


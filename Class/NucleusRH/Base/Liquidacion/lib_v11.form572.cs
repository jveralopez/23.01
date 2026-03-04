using System;
using System.IO;
using System.Xml;
using System.Collections;
using System.Text;
using System.Xml.Xsl;
using System.Xml.XPath;

using Nomad.NSystem.Base;
using Nomad.NSystem.Proxy;
using Nomad.NSystem.Functions;

using NucleusRH.Base.Liquidacion.Liquidacion;

namespace NucleusRH.Base.Liquidacion
{
    public class Form572
    {
        static public void Execute(NomadXML param)
        {
            if (param.isDocument) param = param.FirstChild();
            NomadBatch.Trace("Inicia la generación de Formularios 572.");
            NomadBatch bth = NomadBatch.GetBatch("Formulario572", "Formulario572");

            NomadProxy proxyObject = NomadProxy.GetProxy();

            string reportPath = "";
            if (param.FindElement("DATOS").GetAttr("l_archivo") == "1")
            {
                NomadLog.Info("Guarda en archivos...");
                // Busca la ruta donde se guardaran los archivos PDF
                reportPath = NomadEnvironment.QueryValue("ORG26_PARAMETROS", "d_valor", "c_parametro", "PathForm572", "", true);
                if (String.IsNullOrEmpty(reportPath))
                    throw new Exception("Debe cargar el parámetro PathForm572 con la ruta donde se guardan los Formularios.");
                if (!Directory.Exists(reportPath))
                    throw new Exception("No se encuentra el directorio " + reportPath + ", por favor corrija la ruta o cree dicho directorio.");
                NomadLog.Info("Los formularios se almacenaran en: " + reportPath);
            }
            else
            {
                NomadLog.Info("Reporte por pantalla...");
                reportPath = Path.Combine(proxyObject.RunPath, "temp");
                NomadLog.Info("reportPath::" + proxyObject.RunPath);
            }

            // Query principal
            NomadLog.Info("QUERY PRINCIPAL");
            NomadXML xmlRecibos = NomadEnvironment.QueryNomadXML(LIQUIDACION.Resources.Form572, param.ToString(), false).FirstChild();
            NomadLog.Info("xmlRecibos: " + xmlRecibos.ToString());


            bth.SetPro(30);

            string reportXmlFile = Path.Combine(reportPath, "data.xml");

            if (param.FindElement("DATOS").GetAttr("l_archivo") == "1")
            {
                int x = 0, t = xmlRecibos.ChildLength;
                Nomad.NSystem.Report.NmdFoRender fo = new Nomad.NSystem.Report.NmdFoRender(proxyObject);
                for (NomadXML leg = xmlRecibos.FirstChild(); leg != null; leg = leg.Next())
                {
                    x++; bth.SetPro(30, 90, t, x);
                    try
                    {
                        // crea una instancia del reporte
                        Nomad.NSystem.Report.NmdReport nReport = new Nomad.NSystem.Report.NmdReport("NucleusRH.Base.Liquidacion.Formulario572.form572.rpt", leg.ToString(), proxyObject);

                        string reportPdfFile = Path.Combine(reportPath, "f572_" + leg.GetAttr("cemp").PadLeft(3, '0') + "_" + leg.GetAttr("ubi").PadLeft(6, '0') + "_" + leg.GetAttr("leg").PadLeft(9, '0') + "_" + DateTime.Now.ToString("ddMMyy") + ".pdf");

                        // abre un stream para el archivo de salida intermedio (fo)
                        StreamWriter reportStream = new StreamWriter(reportXmlFile, false);

                        // genera el fo
                        nReport.GenerateReport(reportStream.BaseStream);
                        //reportStream.Flush();
                        reportStream.Close();

                        // transforma el fo a pdf
                        fo.GeneratePDF(reportXmlFile, reportPdfFile);
                    }
                    catch (System.Exception e)
                    {
                        NomadLog.GetTraceBatch().Error(e.Message);
                        NomadLog.Error(e.Message + " -- " + e.StackTrace);
                    }
                }
            }
            else
            {
                // crea una instancia del reporte
                Nomad.NSystem.Report.NmdReport nReport = new Nomad.NSystem.Report.NmdReport("NucleusRH.Base.Liquidacion.Formulario572.form572.rpt", xmlRecibos.ToString(), proxyObject);

                string reportPdfFile = Path.Combine(reportPath, proxyObject.Batch().ID + ".pdf");
                NomadLog.Info("Generando archivo: " + reportPdfFile);

                // abre un stream para el archivo de salida intermedio (fo)
                StreamWriter reportStream = new StreamWriter(reportXmlFile, false);

                // genera el fo
                nReport.GenerateReport(reportStream.BaseStream);
                //reportStream.Flush();
                reportStream.Close();

                // transforma el fo a pdf
                Nomad.NSystem.Report.NmdFoRender fo = new Nomad.NSystem.Report.NmdFoRender(proxyObject);
                fo.GeneratePDF(reportXmlFile, reportPdfFile);
                proxyObject.FileServiceIO().SaveBinFile("temp", proxyObject.Batch().ID + ".pdf", reportPdfFile);
            }
            if (File.Exists(reportXmlFile))
                try
                {
                    File.Delete(reportXmlFile);
                }
                catch (Exception) { }

            bth.SetPro(100);
        }
    }
}
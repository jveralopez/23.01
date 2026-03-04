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
    public class LiquidacionIG
    {
        static public void Execute(string reporte,NomadXML param)
        {
            if (param.isDocument) param = param.FirstChild();
            NomadBatch.Trace("Inicia la generación de Formulario Impuesto a las Ganancias");
            NomadBatch bth = NomadBatch.GetBatch("Formulario Impuesto a las Ganancias", "Formulario Impuesto a las Ganancias");

            NomadProxy proxyObject = NomadProxy.GetProxy();

            string reportPath = "";
            string reportPdfFile = "";
            if (param.FindElement("DATOS").GetAttr("l_archivo") == "1")
            {
                NomadLog.Info("Guarda en archivos...");
                // Busca la ruta donde se guardaran los archivos PDF
                reportPath = NomadEnvironment.QueryValue("ORG26_PARAMETROS", "d_valor", "c_parametro", "PathForm649", "", true);
                if (String.IsNullOrEmpty(reportPath))
                    throw new Exception("Debe cargar el parámetro PathForm649 con la ruta donde se guardan los Formularios.");
                if (!Directory.Exists(reportPath))
                    throw new Exception("No se encuentra el directorio " + reportPath + ", por favor corrija la ruta o cree dicho directorio.");
                NomadLog.Info("Los formularios se almacenaran en: " + reportPath);
            }

            // Query principal
            NomadLog.Info("QUERY PRINCIPAL");
            Nomad.NmdFoRender foRender = new Nomad.NmdFoRender(reporte, param);
            NomadXML result = foRender.QueryDATA();
            NomadLog.Info("result---->: " + result.ToString());

            bth.SetPro(30);

            if (param.FindElement("DATOS").GetAttr("l_archivo") == "1")
            {
                int x = 0, t = result.ChildLength;
                for (NomadXML leg = result.FirstChild(); leg != null; leg = leg.Next())
                {
                    x++; bth.SetPro(30, 90, t, x);
                    try
                    {
                        reportPdfFile = Path.Combine(reportPath, "LiqIG_" + leg.GetAttr("c_empresa").PadLeft(3, '0') + "_" + leg.GetAttr("c_ubicacion").PadLeft(6, '0') + "_" + leg.GetAttr("e_numero_legajo").PadLeft(9, '0') + "_" + DateTime.Now.ToString("ddMMyy") + ".pdf");
                        foRender.Generate(leg, reportPdfFile, Nomad.NmdFoRenderFormat.RENDER_PDF);
                    }
                    catch (System.Exception e)
                    {
                        NomadLog.GetTraceBatch().Error(e.Message);
                        NomadLog.Error(e.Message + " -- " + e.StackTrace);
                    }
                }
            }

            bth.SetPro(100);
        }
    }
}



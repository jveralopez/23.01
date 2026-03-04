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

namespace NucleusRH.Base.Tiempos_Trabajados.Liquidacion_Personas
{

    //////////////////////////////////////////////////////////////////////////////////
    //Clase Procesamiento por Persona
    public partial class CONC_VAL : Nomad.NSystem.Base.NomadObject
    {
        public static void DetalleHorasConceptos(Nomad.NSystem.Proxy.NomadXML paramIN)
        {
            NomadBatch BI = NomadBatch.GetBatch("Detalle de Horas y Conceptos", "Detalle de Horas y Conceptos");
            BI.SetMess("Iniciando la Generacion...");
            BI.Log("Iniciando la Generacion...");
            BI.SetPro(0);

            paramIN = paramIN.FirstChild();
            string outFileName = NomadProxy.GetProxy().Batch().ID + ".htm";
            string outFilePath = NomadProxy.GetProxy().RunPath + "Nomad\\TEMP\\";
            System.IO.StreamWriter ResultFileStream = new System.IO.StreamWriter(outFilePath + "\\" + outFileName, false, System.Text.Encoding.UTF8);

            //generando el RESUMEN
            BI.SetMess("Recopilando Informacion...");
            BI.Log("Recopilando Informacion...");
            BI.SetSubBatch(0, 70);

            NucleusRH.Base.Tiempos_Trabajados.Liquidacion.LIQUIDACION ddoLIQ;
            string liqid = "";
            string liq = "";
            //PROCESAMIENTO
            if (paramIN.GetAttr("oi_liquidacion") != "")
            {
                ddoLIQ = NucleusRH.Base.Tiempos_Trabajados.Liquidacion.LIQUIDACION.Get(paramIN.GetAttr("oi_liquidacion"));
                liqid = ddoLIQ.Id;
                liq = ddoLIQ.c_liquidacion + " - " + ddoLIQ.d_liquidacion;
            }

            //CONCEPTOS
            string oi_conc = "";

            //Lista de Conceptos a PROCESAR
            NomadXML CONC = paramIN.FindElement("CONC");
            if (CONC != null)
            {
                for (NomadXML MyCUR = CONC.FirstChild(); MyCUR != null; MyCUR = MyCUR.Next())
                    if (MyCUR.GetAttr("id") != "")
                        oi_conc += (oi_conc == "" ? "" : ",") + MyCUR.GetAttr("id");
            }

            //xml REPORTE
            NomadXML ReportXML = new NomadXML("<REP oiliq=\"" + liqid + "\" legporhoja=\"" + paramIN.GetAttr("legporhoja") + "\" encabezado=\"" + paramIN.GetAttr("encabezado") + "\" f_desde=\"" + paramIN.GetAttr("f_desde") + "\" f_hasta=\"" + paramIN.GetAttr("f_hasta") + "\" liq=\"" + liq + "\"/>");

            //recorriendo LEGAJOS
            for (NomadXML row = paramIN.FindElement("ROWS").FirstChild(); row != null; row = row.Next())
            {
                NomadXML xmlpres = NucleusRH.Base.Tiempos_Trabajados.Personal.PERSONAL_EMP.GetPresence(row.GetAttr("id"), paramIN.GetAttrDateTime("f_desde"), paramIN.GetAttrDateTime("f_hasta"), true, false, true, true, false, false, false);
                xmlpres.SetAttr("oi_conc", oi_conc);
                xmlpres.SetAttr("oi_liquidacion", liqid);
                NomadEnvironment.GetTrace().Info("GetPresence:" + xmlpres.ToString());

                NomadXML xmldays = NomadEnvironment.QueryNomadXML(NucleusRH.Base.Tiempos_Trabajados.Esperanzaper.ESPERANZAPER.Resources.QRY_RESOLVEDAY, xmlpres.ToString());
                NomadEnvironment.GetTrace().Info("QRY_RESOLVEDAY:" + xmldays.ToString());

                ReportXML.FirstChild().AddXML(xmldays);
            }

            ReportXML.FirstChild().AddXML(CONC);
            ReportXML.FirstChild().SetAttr("f_informe", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"));

            //generando el HTML
            BI.SetPro(70);
            BI.SetMess("Generando el Documento...");
            BI.Log("Generando el Documento...");


            string ReportOut = ReportXML.ToString();
            NomadEnvironment.GetTrace().Info("XMLREP:-- " + ReportOut);

            Nomad.NSystem.Html.NomadHtml objout = new Nomad.NSystem.Html.NomadHtml("NucleusRH.Base.Tiempos_Trabajados.DetalleHorasConceptosAnt.rpt", "", NomadProxy.GetProxy());
            objout.GenerateHtml(ReportOut, ResultFileStream.BaseStream, "");


            //finalizado
            BI.SetPro(100);
        }

    }
}
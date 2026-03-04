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

namespace NucleusRH.Base.Tiempos_Trabajados.Esperanzaper
{

    //////////////////////////////////////////////////////////////////////////////////
    //Clase Esperanza Horaria del Personal
    public partial class ESPERANZAPER : Nomad.NSystem.Base.NomadObject
    {

        //--------------------------------------------------------------------------------------------------------
        // Métodos ESTÁTICOS
        //--------------------------------------------------------------------------------------------------------

        public static DateTime GetDateHope(string oi_personal_emp, DateTime fechaHora)
        {
      DateTime fechaHoraIni = GetDayStart(oi_personal_emp, fechaHora.Date);
      DateTime fechaHoraFin = GetDayStart(oi_personal_emp, fechaHora.AddDays(1).Date);

      //Dia Anterior
      if (fechaHora < fechaHoraIni)
        return fechaHora.AddDays(-1).Date;

      //Dia Actual
      if (fechaHora < fechaHoraFin)
        return fechaHora.Date;

      //Dia Posterior
      return fechaHora.AddDays(1).Date;
        }

    public static DateTime GetDayStart(string oi_personal_emp, DateTime fecha)
    {
      //             |   0
      //.............|...|...................
      //_________|   |_______________________
      //             |_ _ _ _ _ _ _ _ _ _ _ _

      DIA ddoDiaActual = ESPERANZAPER.GetDayHope(fecha, int.Parse(oi_personal_emp));
      if (ddoDiaActual != null && ddoDiaActual.DETALLE.Count != 0)
      {
        DETALLE ddoDetActIni = (DETALLE)ddoDiaActual.DETALLE[0];
        if (ddoDetActIni.e_horainicio < 0)
          return fecha.AddMinutes(ddoDetActIni.e_horainicio);
      }

      //                 0   |
      //.................|...|...............
      //_____________________|   |___________
      //                     |_ _ _ _ _ _ _ _

      DIA ddoDiaAnterior = ESPERANZAPER.GetDayHope(fecha.AddDays(-1), int.Parse(oi_personal_emp));
      if (ddoDiaAnterior != null && ddoDiaAnterior.DETALLE.Count != 0)
      {
        DETALLE ddoDetAntFin = (DETALLE)ddoDiaAnterior.DETALLE[ddoDiaAnterior.DETALLE.Count-1];
        if (ddoDetAntFin.e_horafin > 1440)
          return fecha.AddMinutes(ddoDetAntFin.e_horafin-1440);
      }

      //                 0
      //.................|...................
      //_____________|   |   |_______________
      //                 |_ _ _ _ _ _ _ _ _ _

      return fecha;
    }

        public static Nomad.NSystem.Proxy.NomadXML XmlHope(Nomad.NSystem.Proxy.NomadXML param)
        {
            NomadBatch objBatch;
            objBatch = NomadBatch.GetBatch("Iniciando...", "Calculando Esperanza Horaria");

            objBatch.SetPro(0);

            NomadXML xmldoc = param.FirstChild();

            //GUARDO LAS FECHAS
            DateTime f_des = new DateTime(int.Parse(xmldoc.GetAttr("f_desde").Substring(0, 4)), int.Parse(xmldoc.GetAttr("f_desde").Substring(4, 2)), int.Parse(xmldoc.GetAttr("f_desde").Substring(6, 2)));
            DateTime f_has = new DateTime(int.Parse(xmldoc.GetAttr("f_hasta").Substring(0, 4)), int.Parse(xmldoc.GetAttr("f_hasta").Substring(4, 2)), int.Parse(xmldoc.GetAttr("f_hasta").Substring(6, 2)));

            //CREO EL XML FINAL
            NomadXML xmlres;
            NomadXML xmlPer;
            NomadXML xmlFinal = new NomadXML("<FILTRO><DIAS/><PERS/></FILTRO>");
            xmlFinal = xmlFinal.FirstChild();
            xmlFinal.SetAttr("oi_clase_org", xmldoc.GetAttr("oi_clase_org"));
            xmlFinal.SetAttr("f_desde", xmldoc.GetAttr("f_desde"));
            xmlFinal.SetAttr("f_hasta", xmldoc.GetAttr("f_hasta"));

            NomadXML xmlDia = xmlFinal.FindElement("DIAS");
            for (DateTime dia = xmldoc.GetAttrDateTime("f_desde"); dia <= xmldoc.GetAttrDateTime("f_hasta"); dia = dia.AddDays(1))
            {
                xmlDia.AddTailElement("D").SetAttr("fecha", Nomad.NSystem.Functions.StringUtil.date2str(dia));
            }
            NomadXML xmlPers = xmlFinal.FindElement("PERS");
            //NomadXML xmlSinEsp = xmlFinal.FindElement("SINESP");

            ArrayList lista = (ArrayList)xmldoc.FirstChild().GetElements("ROW");

            //RECORRO LOS IDS QUE ENTRAN Y PREGUNTO SI ALGUN ID NO ESTA EN LA HASH, PORQ DE SER ASI HAY QUE AGREGARLO
            for (int x = 0; x < lista.Count; x++)
            {
                NomadXML xmlCur = (NomadXML)lista[x];

                NomadEnvironment.GetTrace().Info("ID -- " + xmlCur.GetAttr("id"));
                xmlPer = xmlPers.AddTailElement("PER");
                xmlPer.SetAttr("id", xmlCur.GetAttr("id"));

                objBatch.SetPro(0, 100, lista.Count, x);
                objBatch.SetMess("Legajos " + (x + 1) + " de " + lista.Count);

                xmlres = GetRangeHope(f_des, f_has, int.Parse(xmlCur.GetAttr("id")));
                NomadEnvironment.GetTrace().Info("RES -- " + xmlres);

                for (NomadXML xmlDay = xmlres.FirstChild(); xmlDay != null; xmlDay = xmlDay.Next())
                {
                    xmlDay.SetAttr("id", "");
                    xmlDay.SetAttr("e_posicion", "");
                    xmlDay.SetAttr("nmd-status", "");
                    xmlDay.SetAttr("descr", "");
                    xmlDay.SetAttr("oi_turno", xmlDay.FindElement("oi_turno").GetAttr("value"));
                    xmlDay.SetAttr("oi_licencia", xmlDay.FindElement("oi_licencia").GetAttr("value"));
                    while (xmlDay.FirstChild() != null)
                    {
                        xmlDay.DeleteChild(xmlDay.FirstChild());
                    }
                }
                xmlPer.AddXML(xmlres);
            }

            return xmlFinal;
        }

        public static void RepHope(Nomad.NSystem.Proxy.NomadXML param)
        {
            NomadBatch objBatch;
            objBatch = NomadBatch.GetBatch("Iniciando...", "Generando Planilla de Esperanza Horaria");

            objBatch.SetPro(0);
            objBatch.SetSubBatch(0, 90);

            NomadXML xmlhope = XmlHope(param);

            objBatch.SetMess("Ejecutando el Reporte...");
            NomadBatch.Trace(xmlhope.ToString());

            string outFileName = NomadProxy.GetProxy().Batch().ID + ".htm";
            string outFilePath = NomadProxy.GetProxy().RunPath + "Nomad\\TEMP\\";
            NomadBatch.Trace("Generando Reporte HTML en path:'" + outFilePath + outFileName + "'");

			Nomad.NomadHTML nmdHtml = new Nomad.NomadHTML("NucleusRH.Base.Tiempos_Trabajados.EsperanzaLegajo.rpt", xmlhope);
			nmdHtml.GenerateHTML(outFilePath + "\\" + outFileName, System.Text.Encoding.UTF8);
        }

        public static NomadXML GetAssistenciaPersona(string oi_per_emp, string oi_hor, DateTime fd, DateTime fh, string presencias, string licencias, string ausencias, string tardanzas, string anuladas)
        {
            NomadXML xmlpres = NucleusRH.Base.Tiempos_Trabajados.Personal.PERSONAL_EMP.GetPresence(oi_per_emp, fd, fh, true, false, true, true, false, true, false);
            xmlpres.SetAttr("oi_hor", oi_hor);
            xmlpres.SetAttr("presencias", presencias);
            xmlpres.SetAttr("licencias", licencias);
            xmlpres.SetAttr("ausencias", ausencias);
            xmlpres.SetAttr("tardanzas", tardanzas);
            xmlpres.SetAttr("anuladas", anuladas);
            xmlpres.SetAttr("rango", fd != fh ? "1" : "0");

            NomadEnvironment.GetTrace().Info("GetPresence:" + xmlpres.ToString());

            NomadXML xmldays = NomadEnvironment.QueryNomadXML(ESPERANZAPER.Resources.QRY_RESOLVEDAY, xmlpres.ToString());
            NomadEnvironment.GetTrace().Info("QRY_RESOLVEDAY:" + xmldays.ToString());

            return xmldays.FirstChild();
        }

        public static NomadXML GetAssistenciaRango(NomadXML paramIN)
        {
            NomadBatch BI = NomadBatch.GetBatch("Recopilando Informacion", "Recopilando Informacion");

            NomadXML xmlrep = new NomadXML("<REP/>");
            xmlrep.FirstChild().SetAttr("anuladas", paramIN.GetAttr("anuladas"));
            BI.SetMess("Obteniendo informacion...");
            BI.Log("Obteniendo informacion...");
            BI.SetPro(0);

            int cantleg = paramIN.FirstChild().ChildLength;
            int cont = 1;
            for (NomadXML xmlleg = paramIN.FirstChild().FirstChild(); xmlleg != null; xmlleg = xmlleg.Next())
            {
                BI.SetMess("Analizando legajo " + cont.ToString() + "/" + cantleg.ToString() + "...");
                NomadXML legres = GetAssistenciaPersona(xmlleg.GetAttr("id"), "", paramIN.GetAttrDateTime("f_desde"), paramIN.GetAttrDateTime("f_hasta"), paramIN.GetAttr("presencias"), paramIN.GetAttr("licencias"), paramIN.GetAttr("ausencias"), paramIN.GetAttr("tardanzas"), paramIN.GetAttr("anuladas"));
                BI.SetPro(cont / cantleg);
                cont++;
                NomadEnvironment.GetTrace().Info("legres:" + legres.ToString());
                if (legres.ChildLength > 0)
                    xmlrep.FirstChild().AddXML(legres);
            }
            NomadEnvironment.GetTrace().Info("xmlrep:" + xmlrep.ToString());
            return xmlrep;
        }

        public static void CreateReportAssistencia(NomadXML paramIN)
        {
            NomadBatch BI = NomadBatch.GetBatch("Reporte Assistencia", "Reporte Asistencia");
            BI.SetMess("Iniciando la Generacion...");
            BI.Log("Iniciando la Generacion...");
            BI.SetPro(0);

            paramIN = paramIN.FirstChild();
            string outFileName = NomadProxy.GetProxy().Batch().ID + ".htm";
            string outFilePath = NomadProxy.GetProxy().RunPath + "Nomad\\TEMP\\";

            //generando el RESUMEN
            BI.SetMess("Recopilando Informacion...");
            BI.Log("Recopilando Informacion...");
            BI.SetSubBatch(0, 70);
            NomadXML ReportXML = GetAssistenciaRango(paramIN);

            //generando el HTML
            BI.SetPro(75);
            BI.SetMess("Generando el Documento...");
            BI.Log("Generando el Documento...");

            ReportXML.FirstChild().SetAttr("f_desde", paramIN.GetAttrDateTime("f_desde").ToString("dd/MM/yyyy"));
            ReportXML.FirstChild().SetAttr("f_hasta", paramIN.GetAttrDateTime("f_hasta").ToString("dd/MM/yyyy"));
            ReportXML.FirstChild().SetAttr("f_informe", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"));
            ReportXML.FirstChild().SetAttr("chkDetalleHoras", paramIN.GetAttr("chkDetalleHoras"));
            ReportXML.FirstChild().SetAttr("chkPuestoActual", paramIN.GetAttr("chkPuestoActual"));
            ReportXML.FirstChild().SetAttr("chkPosicionActual", paramIN.GetAttr("chkPosicionActual"));
            ReportXML.FirstChild().SetAttr("chkDepartamentoActual", paramIN.GetAttr("chkDepartamentoActual"));
            ReportXML.FirstChild().SetAttr("chkSectorActual", paramIN.GetAttr("chkSectorActual"));
            ReportXML.FirstChild().SetAttr("chkCtroCostoActual", paramIN.GetAttr("chkCtroCostoActual"));
            ReportXML.FirstChild().SetAttr("chkUbicacionActual", paramIN.GetAttr("chkUbicacionActual"));
            ReportXML.FirstChild().SetAttr("chkDetalle", paramIN.GetAttr("chkDetalle"));
            //calcula informacion RESUMEN si el reporte es para un solo dia
            if (paramIN.GetAttr("f_desde") == paramIN.GetAttr("f_hasta"))
            {
                int cpre = 0;
                int caus = 0;
                int ctar = 0;
                int cret = 0;

                for (NomadXML leg = ReportXML.FirstChild().FirstChild(); leg != null && leg.Name == "L"; leg = leg.Next())
                {
                    for (NomadXML day = leg.FirstChild(); day != null && day.Name == "D"; day = day.Next())
                    {
                        if (day.GetAttr("pre") == "1")
                            cpre++;
                        if (day.GetAttr("aus") == "1")
                            caus++;
                        if (day.GetAttr("e1t") == "1")
                            ctar++;
                        if (day.GetAttr("e2t") == "1")
                            cret++;
                    }
                }
                ReportXML.FirstChild().SetAttr("resumen", "Presencias: " + cpre.ToString() + " | Ausencias: " + caus.ToString() + " | Tardanzas: " + ctar.ToString() + " | Retiros Anticipados: " + cret.ToString());
            }

            string ReportOut = ReportXML.ToString();
			string strReportName = "";
            NomadEnvironment.GetTrace().Info("XMLREP:-- " + ReportOut);

            if (ReportXML.FirstChild().GetAttr("f_desde") == ReportXML.FirstChild().GetAttr("f_hasta")) {
				strReportName = "NucleusRH.Base.Tiempos_Trabajados.Asistencias.rpt"; 
            } else {
				strReportName = "NucleusRH.Base.Tiempos_Trabajados.AsistenciasRango.rpt"; 
            }
			
			Nomad.NomadHTML nmdHtml = new Nomad.NomadHTML(strReportName, ReportXML);
			nmdHtml.GenerateHTML(outFilePath + "\\" + outFileName, System.Text.Encoding.UTF8);
			
			
            //finalizado
            BI.SetPro(100);
        }

        public static void CreateReportHorarioDescansoLegal(string reportName, NomadXML paramIN)
        {
            NomadBatch BI = NomadBatch.GetBatch("Reporte Horario y Descanso Legal", "Reporte Horario y Descanso Legal");
            BI.SetMess("Iniciando la Generacion...");
            BI.Log("Iniciando la Generacion...");
            BI.SetPro(0);

            paramIN = paramIN.FirstChild();
            int año = int.Parse(paramIN.GetAttr("e_periodo")) / 100;
            int mes = int.Parse(paramIN.GetAttr("e_periodo")) % 100;

            paramIN.SetAttr("f_desde", new DateTime(año, mes, 1));
            paramIN.SetAttr("f_hasta", new DateTime(año, mes, DateTime.DaysInMonth(año, mes)));
            string outFileName = NomadProxy.GetProxy().Batch().ID + ".htm";
            string outFilePath = NomadProxy.GetProxy().RunPath + "Nomad\\TEMP\\";
            System.IO.StreamWriter ResultFileStream = new System.IO.StreamWriter(outFilePath + "\\" + outFileName, false, System.Text.Encoding.UTF8);

            //generando el RESUMEN
            BI.SetMess("Recopilando Informacion...");
            BI.Log("Recopilando Informacion...");
            BI.SetSubBatch(0, 70);
            NomadXML xmlRespuesta = CalcularReporteHorarioDescansoLegal(paramIN);

            //generando el HTML
            BI.SetPro(75);
            BI.SetMess("Generando el Documento...");
            BI.Log("Generando el Documento...");

            //Nomad.NomadHTML objout = new Nomad.NomadHTML(reportName, "");
            //objout.GenerateHTML(xmlRespuesta.ToString(), ResultFileStream);

            Nomad.NomadHTML objout = new Nomad.NomadHTML(reportName, xmlRespuesta.ToString());
            objout.GenerateHTML(ResultFileStream);

            BI.SetPro(100);
        }

        private static NomadXML CalcularReporteHorarioDescansoLegal(NomadXML paramIN)
        {
            NomadBatch BI = NomadBatch.GetBatch("Recopilando Informacion", "Recopilando Informacion");

            int cantleg = paramIN.FirstChild().ChildLength;
            int cont = 1, i = 0;
            NomadXML xmlrep = new NomadXML("<REP/>");

            NomadXML xmlr = xmlrep.FirstChild();

            xmlr.SetAttr("e_periodo", paramIN.GetAttr("e_periodo"));
            xmlr.SetAttr("empresa", paramIN.GetAttr("empresa"));
            xmlr.SetAttr("chkFechaIngreso", paramIN.GetAttr("chkFechaIngreso"));
            xmlr.SetAttr("chkFuncion", paramIN.GetAttr("chkFuncion"));
            xmlr.SetAttr("chkSueldoBasico", paramIN.GetAttr("chkSueldoBasico"));
            xmlr.SetAttr("chkTipoPersonal", paramIN.GetAttr("chkTipoPersonal"));
            xmlr.SetAttr("chkConCat", paramIN.GetAttr("chkConCat"));

            NomadXML xmlDias = new NomadXML("<DIAS />");

            string periodo = paramIN.GetAttr("e_periodo");
            int maximoDiaDelMes = DateTime.DaysInMonth(int.Parse(periodo.Substring(0, 4)), int.Parse(periodo.Substring(4, 2)));
            xmlDias.FirstChild().SetAttr("e_periodo", paramIN.GetAttr("e_periodo"));
            xmlDias.FirstChild().SetAttr("max_dia_mes", maximoDiaDelMes);
            for (NomadXML xmlleg = paramIN.FirstChild().FirstChild(); xmlleg != null; xmlleg = xmlleg.Next())
            {
                BI.SetMess("Analizando legajo " + cont.ToString() + "/" + cantleg.ToString() + "...");
                //NomadXML legres = Tiempos_Trabajados.Personal.PERSONAL_EMP.GetPresence(xmlleg.GetAttr("id"), paramIN.GetAttrDateTime("f_desde"), paramIN.GetAttrDateTime("f_hasta"), false, true, false, false, false, false, false);

                BI.SetPro(cont / cantleg);
                cont++;

                //Recupero el legajo de TTA para encontrar el oi_calendario
                NucleusRH.Base.Tiempos_Trabajados.Personal.PERSONAL_EMP LegajoTTA = NucleusRH.Base.Tiempos_Trabajados.Personal.PERSONAL_EMP.Get(xmlleg.GetAttr("id"), false);

                //Recupero el/los turnos asignados al legajo dentro de periodo
                NomadXML parametro = new NomadXML("DATOS");
                parametro.SetAttr("oi_personal_emps", xmlleg.GetAttr("id"));
                parametro.SetAttr("f_Inicio", paramIN.GetAttr("f_desde"));
                parametro.SetAttr("f_Fin", paramIN.GetAttr("f_hasta"));

                NomadXML turnos = NomadEnvironment.QueryNomadXML(NucleusRH.Base.Tiempos_Trabajados.Personal.HORARIOPERS.Resources.QRY_DatosHorarios_Legal, parametro.ToString());

                NomadXML nmdXMLPPal = new Nomad.NSystem.Proxy.NomadXML();
                NomadXML legres = nmdXMLPPal.AddHeadElement("Days");

                //Si tiene mas de un turno asignado en el periodo - debo unir los documentos que se desprenden de ejecutar el gethope para cada uno
                for (NomadXML turno = turnos.FirstChild().FirstChild(); turno != null; turno = turno.Next())
                {
                    NomadXML legres1 = NucleusRH.Base.Tiempos_Trabajados.Horarios.clsCreaEsperanzaH.GetHope(turno.GetAttrDateTime("f_fechaInicio"), turno.GetAttrDateTime("f_fechaFin"), int.Parse(turno.GetAttr("oi_horario")), int.Parse(LegajoTTA.oi_calendario_ult), 0);
                    for (NomadXML XMLDIAL = legres1.FirstChild(); XMLDIAL != null; XMLDIAL = XMLDIAL.Next())
                    {
                        legres.AddText(XMLDIAL.ToString());
                    }
                }

                legres = ObligatoriaTipoHora(legres);
                legres = TransformarAEventos(legres);

                legres.SetAttr("oi_per", LegajoTTA.id.ToString());
                legres.SetAttr("fd", paramIN.GetAttrDateTime("f_desde"));
                legres.SetAttr("fh",  paramIN.GetAttrDateTime("f_hasta"));

                legres = NomadEnvironment.QueryNomadXML(ESPERANZAPER.Resources.QRY_HORARIOS_DESC_LEGAL, legres.ToString());

                NomadEnvironment.GetTrace().Info("legres:" + legres.ToString());
                i++;
                legres.FirstChild().SetAttr("numeracion", i);
                if (legres.FirstChild().GetAttr("hayEventos") == "1")
                {
                    NomadXML dia = new NomadXML(legres.FirstChild().FirstChild().ToString());
                    xmlDias.FirstChild().AddTailElement(dia);
                    legres.FirstChild().DeleteChild(legres.FirstChild().FirstChild());
                    xmlr.AddXML(legres);
                }

            }

            xmlDias = NomadEnvironment.QueryNomadXML(ESPERANZAPER.Resources.QRY_MAXIMO_DIAS, xmlDias.ToString());
            xmlr.AddTailElement(xmlDias);
            return xmlrep;

        }

        private static NomadXML ObligatoriaTipoHora(NomadXML legres)
        {

            //Hashtable tipoHora = new Hashtable(); con el oi

            for (NomadXML xmlDia = legres.FirstChild(); xmlDia != null; xmlDia = xmlDia.Next())
            {
                NomadXML esp = xmlDia.FindElement("DETALLE");

                if (xmlDia.FirstChild() != null) xmlDia.SetAttr("oi_turno", xmlDia.FirstChild().GetAttr("value"));

                xmlDia.DeleteChild(xmlDia.FirstChild());

                if (esp == null) continue;

                for (NomadXML detalle = esp.FirstChild(); detalle != null; detalle = detalle.Next())
                {
                    NomadXML tipohora = detalle.FirstChild();
                    NucleusRH.Base.Tiempos_Trabajados.Tipos_Horas.TIPOHORA TH = NucleusRH.Base.Tiempos_Trabajados.Tipos_Horas.TIPOHORA.GetById(tipohora.GetAttr("value"));
                    detalle.SetAttr("obligatorio", TH.Obligatorio);
                }
            }

            return legres;
        }

        private static NomadXML TransformarAEventos(NomadXML legres)
        {
            //Aca pasa lo mismo - se usa el addtailelement - y pincha al recorrerlo - ver el porque no vuelve
            for (NomadXML xmlDia = legres.FirstChild(); xmlDia != null; xmlDia = xmlDia.Next())
            {
                string fecha = xmlDia.GetAttr("f_dia");
                NomadXML esp = xmlDia.FindElement("DETALLE");
                NomadXML eventos = new NomadXML("eventos");

                string ultimaHora = "";

                if (esp == null) continue;

                for (NomadXML detalle = esp.FirstChild(); detalle != null; detalle = detalle.Next())
                {
                    if (detalle.GetAttr("obligatorio") == "1")
                    {

                        if (ultimaHora == detalle.GetAttr("e_horainicio"))
                        {
                            NomadXML eve = eventos.LastChild();
                            eve.SetAttr("m", detalle.GetAttr("e_horafin"));
                        }
                        else
                        {
                            eventos.AddTailElement(GenerarEvento(fecha, "e", detalle.GetAttr("e_horainicio")));
                            eventos.AddTailElement(GenerarEvento(fecha, "s", detalle.GetAttr("e_horafin")));
                        }

                        ultimaHora = detalle.GetAttr("e_horafin");
                    }

                }
                xmlDia.DeleteChild(esp);
                xmlDia.AddTailElement(eventos.ToString());
            }

            return legres;
        }

        private static NomadXML GenerarEvento(string fecha, string tipo, string hora)
        {
            NomadXML eve = new NomadXML("eve");

            eve.SetAttr("f", fecha);
            eve.SetAttr("t", tipo);
            eve.SetAttr("m", hora);
            return eve;
        }

        public static void CreateReportPlanificacionPorPeriodo(string reportName, NomadXML paramIN)
        {
            NomadBatch BI = NomadBatch.GetBatch("Reporte Planificacion por Periodo", "Reporte Planificacion por Periodo");
            BI.SetMess("Iniciando la Generacion...");
            BI.Log("Iniciando la Generacion...");
            BI.SetPro(0);

            paramIN = paramIN.FirstChild();
            int año = int.Parse(paramIN.GetAttr("e_periodo")) / 100;
            int mes = int.Parse(paramIN.GetAttr("e_periodo")) % 100;

            paramIN.SetAttr("f_desde", new DateTime(año,mes, 1));
            paramIN.SetAttr("f_hasta", new DateTime(año,mes, DateTime.DaysInMonth(año,mes)));
            string outFileName = NomadProxy.GetProxy().Batch().ID + ".htm";
            string outFilePath = NomadProxy.GetProxy().RunPath + "Nomad\\TEMP\\";
            System.IO.StreamWriter ResultFileStream = new System.IO.StreamWriter(outFilePath + "\\" + outFileName, false, System.Text.Encoding.UTF8);

            //generando el RESUMEN
            BI.SetMess("Recopilando Informacion...");
            BI.Log("Recopilando Informacion...");
            BI.SetSubBatch(0, 70);
            NomadXML xmlRespuesta = GetReporteHoariosDescansos(paramIN);

            //generando el HTML
            BI.SetPro(75);
            BI.SetMess("Generando el Documento...");
            BI.Log("Generando el Documento...");

            //Nomad.NomadHTML objout = new Nomad.NomadHTML(reportName, "");
            //objout.GenerateHTML(xmlRespuesta.ToString(), ResultFileStream);

            Nomad.NomadHTML objout = new Nomad.NomadHTML(reportName, xmlRespuesta.ToString());
            objout.GenerateHTML(ResultFileStream);

            BI.SetPro(100);
        }

        private static NomadXML GetReporteHoariosDescansos(NomadXML paramIN)
        {
            NomadBatch BI = NomadBatch.GetBatch("Recopilando Informacion", "Recopilando Informacion");

            int cantleg = paramIN.FirstChild().ChildLength;
            int cont = 1,i = 0;
            NomadXML xmlrep = new NomadXML("<REP/>");

            NomadXML xmlr = xmlrep.FirstChild();

            xmlr.SetAttr("e_periodo", paramIN.GetAttr("e_periodo"));
            xmlr.SetAttr("empresa", paramIN.GetAttr("empresa"));
            xmlr.SetAttr("chkFechaIngreso",paramIN.GetAttr("chkFechaIngreso"));
            xmlr.SetAttr("chkFuncion",paramIN.GetAttr("chkFuncion"));
            xmlr.SetAttr("chkSueldoBasico",paramIN.GetAttr("chkSueldoBasico"));
            xmlr.SetAttr("chkTipoPersonal",paramIN.GetAttr("chkTipoPersonal"));
            xmlr.SetAttr("chkConCat", paramIN.GetAttr("chkConCat"));

            NomadXML xmlDias = new NomadXML("<DIAS />");

            string periodo = paramIN.GetAttr("e_periodo");
            int maximoDiaDelMes = DateTime.DaysInMonth(int.Parse(periodo.Substring(0, 4)), int.Parse(periodo.Substring(4, 2)));
            xmlDias.FirstChild().SetAttr("e_periodo", paramIN.GetAttr("e_periodo"));
            xmlDias.FirstChild().SetAttr("max_dia_mes", maximoDiaDelMes);
            for (NomadXML xmlleg = paramIN.FirstChild().FirstChild(); xmlleg != null; xmlleg = xmlleg.Next())
            {
                BI.SetMess("Analizando legajo " + cont.ToString() + "/" + cantleg.ToString() + "...");
                NomadXML legres = Tiempos_Trabajados.Personal.PERSONAL_EMP.GetPresence(xmlleg.GetAttr("id"), paramIN.GetAttrDateTime("f_desde"), paramIN.GetAttrDateTime("f_hasta"), true, false, false, false, false, false, false);

                BI.SetPro(cont / cantleg);
                cont++;

                bool hayEventos = HayEventos(legres);
                if (hayEventos)
                {
                    legres = NomadEnvironment.QueryNomadXML(ESPERANZAPER.Resources.QRY_HORARIOS_DESCANSOS, legres.ToString());

                    NomadEnvironment.GetTrace().Info("legres:" + legres.ToString());
                    i++;
                    legres.FirstChild().SetAttr("numeracion", i);
                    if (legres.FirstChild().GetAttr("hayEventos") == "1")
                    {
                        NomadXML dia = new NomadXML(legres.FirstChild().FirstChild().ToString());
                        xmlDias.FirstChild().AddTailElement(dia);
                        legres.FirstChild().DeleteChild(legres.FirstChild().FirstChild());
                        xmlr.AddXML(legres);
                    }
                }
            }

            xmlDias = NomadEnvironment.QueryNomadXML(ESPERANZAPER.Resources.QRY_MAXIMO_DIAS, xmlDias.ToString());
            xmlr.AddTailElement(xmlDias);
            return xmlrep;
        }

        private static bool HayEventos(NomadXML legres)
        {
            for (NomadXML xmldia = legres.FirstChild(); xmldia != null; xmldia = xmldia.Next())
            {
                NomadXML xmlEvento = xmldia.FindElement("eventos");
                if (xmlEvento != null)
                    return true;
            }
            return false;
        }

        public static void CreateParteDiario(NomadXML paramIN)
        {
            NomadBatch BI = NomadBatch.GetBatch("Parte Diario", "Parte Diario");
            BI.SetMess("Iniciando la Generacion...");
            BI.Log("Iniciando la Generacion...");
            BI.SetPro(0);

            paramIN = paramIN.FirstChild();
            string outFileName = NomadProxy.GetProxy().Batch().ID + ".htm";
            string outFilePath = NomadProxy.GetProxy().RunPath + "Nomad\\TEMP\\";

            //generando el RESUMEN
            BI.SetMess("Recopilando Informacion...");
            BI.Log("Recopilando Informacion...");
            BI.SetSubBatch(0, 70);
            NomadXML ReportXML = new NomadXML("REPORT");

            int cont = 1;

            string paramfic = NomadEnvironment.QueryValue("ORG26_PARAMETROS", "d_valor", "c_parametro", "EveES", "", true);
            Hashtable htpres = new Hashtable();
            int hnow = DateTime.Now.Hour * 60 + DateTime.Now.Minute;

            for (NomadXML xmlval = paramIN.FirstChild().FirstChild(); xmlval != null; xmlval = xmlval.Next())
            {
                string[] ids = xmlval.GetAttr("VALUES").Split(',');
                foreach (string idper in ids)
                {
                    bool esperado = false;
                    int hdesde = -1;
                    int hhasta = -1;

                    BI.SetMess("Analizando registro " + cont.ToString() + "...");

                    NomadXML xmlleg = new NomadXML("ROW");
                    xmlleg.SetAttr("id", idper);
                    xmlleg.SetAttr("paramfic", paramfic);
                    xmlleg.SetAttr("fecha", DateTime.Now.Date.ToString("yyyyMMdd"));
                    xmlleg.SetAttr("fechahora", DateTime.Now.ToString("yyyyMMddHHmmss"));

                    NomadXML xmlPre = NomadEnvironment.QueryNomadXML(ESPERANZAPER.Resources.QRY_ASISTENCIA, xmlleg.ToString());
                    if (xmlPre.FirstChild().GetAttr("presente") != "1" && xmlPre.FirstChild().GetAttr("legajo") != "")
                    {
                        GetEsperado(DateTime.Now.Date, int.Parse(idper), ref esperado, ref hdesde, ref hhasta);
                        if (esperado && hnow >= hdesde && hnow <= hhasta)
                        {
                            int hora = (hdesde / 60);
                            int mins = (int)Math.Round(((double)hdesde / 60 - hdesde / 60) * 60);

                            string strh = ("0" + hora).ToString().Substring(("0" + hora).ToString().Length - 2, 2); ;
                            string strm = ("0" + mins).ToString().Substring(("0" + mins).ToString().Length - 2, 2); ;

                            xmlPre.FirstChild().SetAttr("hdesde", strh + ":" + strm);
                            ReportXML.AddXML(xmlPre);
                        }
                    }
                    else
                        ReportXML.AddXML(xmlPre);
                    cont++;
                }
            }

            //generando el HTML
            BI.SetPro(75);
            BI.SetMess("Generando el Documento...");
            BI.Log("Generando el Documento...");
            ReportXML.SetAttr("f_informe", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"));
			ReportXML.SetAttr("chkPuestoActual",paramIN.GetAttr("chkPuestoActual"));
            ReportXML.SetAttr("chkPosicionActual",paramIN.GetAttr("chkPosicionActual"));
            ReportXML.SetAttr("chkDepartamentoActual",paramIN.GetAttr("chkDepartamentoActual"));
            ReportXML.SetAttr("chkSectorActual",paramIN.GetAttr("chkPosicionActual"));
            ReportXML.SetAttr("chkCtroCostoActual",paramIN.GetAttr("chkCtroCostoActual"));
            ReportXML.SetAttr("chkUbicacionActual",paramIN.GetAttr("chkUbicacionActual"));

			Nomad.NomadHTML nmdHtml = new Nomad.NomadHTML("NucleusRH.Base.Tiempos_Trabajados.ParteDiario.rpt", ReportXML);
			nmdHtml.GenerateHTML(outFilePath + "\\" + outFileName, System.Text.Encoding.UTF8);

            //finalizado
            BI.SetPro(100);
        }

        public static Nomad.NSystem.Proxy.NomadXML GetRangeHope2(DateTime pFromDate, DateTime pToDate, string pOiPersonalEmp)
        {

            NomadXML DAYS = new NomadXML();
            NucleusRH.Base.Tiempos_Trabajados.Turnos.TURNO TUR;

            //Recorro los IDS de legajo y obtengo la esperanza de cada uno
            string[] ids = pOiPersonalEmp.Split(',');
            NomadLog.Debug("ids -- " + ids.ToString());
            NomadXML RET = new NomadXML("LEGS");

            for (int x = 0; x < ids.Length; x++)
            {
                NomadLog.Debug("leg: -- " + x.ToString());
                RET.AddTailElement("LEG").SetAttr("oi_personal_emp",ids[x]);

                DAYS = GetRangeHope(pFromDate, pToDate, Convert.ToInt32(ids[x]));
                NomadLog.Debug("DAYS -- " + DAYS.ToString());

                NucleusRH.Base.Tiempos_Trabajados.Personal.PERSONAL_EMP LEG = NucleusRH.Base.Tiempos_Trabajados.Personal.PERSONAL_EMP.Get(ids[x]);
                for (NomadXML DIA = DAYS.FirstChild(); DIA != null; DIA = DIA.Next())
                {
                    NomadXML ROW = new NomadXML("DIA");

                    ROW.SetAttr("dia", DIA.GetAttr("f_dia"));
                    ROW.SetAttr("oi_turno", DIA.FindElement("oi_turno").GetAttr("value"));
                    ROW.SetAttr("oi_horario", DIA.FindElement("oi_horario").GetAttr("value"));
                    ROW.SetAttr("licencia", DIA.FindElement("oi_licencia").GetAttr("value"));
                    if (ROW.GetAttr("licencia") != "")
                        ROW.SetAttr("licencia", "SI");
                    else
                        ROW.SetAttr("licencia", "NO");

                    foreach (NucleusRH.Base.Tiempos_Trabajados.Personal.NOVEDAD NOV in LEG.NOVEDADES)
                    {
                        if (NOV.f_fecha == DIA.GetAttrDateTime("f_dia"))
                        {
                            ROW.SetAttr("novedad", "SI");
                            break;
                        }
                        else
                            ROW.SetAttr("novedad", "NO");
                    }

                    if (ROW.GetAttr("oi_turno") != "")
                    {
                        TUR = NucleusRH.Base.Tiempos_Trabajados.Turnos.TURNO.Get(ROW.GetAttr("oi_turno"));
                        if(TUR != null)
                            ROW.SetAttr("c_color_t", TUR.c_color);
                    }
                    //ROW.SetAttr("hts:delete", 1);
                    RET.FindElement2("LEG","oi_personal_emp", ids[x]).AddTailElement(ROW);

                }
            }
            NomadLog.Debug("RET -- " + RET.ToString());
          //return GetRangeHope(pFromDate,pToDate,Convert.ToInt32(pOiPersonalEmp));
            return RET;
        }

    /// <summary>
        /// Retorna un XML con la información día a día de la esperanza solicidata.
        /// </summary>
        /// <param name="pFromDate">Fecha desde en el rango solicitado.</param>
        /// <param name="pToDate">Fecha hasta en el rango solicitado.</param>
        /// <param name="pOiPersonalEmp">Oi_PersonalEmp del Legajo.</param>
        /// <returns></returns>
        public static Nomad.NSystem.Proxy.NomadXML GetRangeHope(DateTime pFromDate, DateTime pToDate, int pOiPersonalEmp)
        {
            int intFrom = StringUtil.date2week(pFromDate);
            int intTo = StringUtil.date2week(pToDate);
            ESPERANZAPER objEsp;
            NomadXML nxmDays = new NomadXML("DAYS");

            //Obtiene la esperanza por cada semana y se guarda los dias
            for (int xWeek = intFrom; xWeek <= intTo; xWeek++)
            {
                objEsp = ESPERANZAPER.GetHope(xWeek, pOiPersonalEmp);

                foreach (DIA objDia in objEsp.DIAS)
                {
                    if (objDia.f_dia >= pFromDate && objDia.f_dia <= pToDate)
                    {
                        nxmDays.AddText(objDia.SerializeAll());
                    }
                }
            }

            return nxmDays;
        }

        /// <summary>
        /// Retorna si esta esperado para un dia y la hora desde hasta
        /// </summary>
        /// <param name="pFromDate">Fecha desde en el rango solicitado.</param>
        /// <param name="pToDate">Fecha hasta en el rango solicitado.</param>
        /// <param name="pOiPersonalEmp">Oi_PersonalEmp del Legajo.</param>
        /// <returns></returns>
        public static void GetEsperado(DateTime fecha, int oi_personal_emp, ref bool esperado, ref int hdesde, ref int hhasta)
        {
            ArrayList dia = GetDaysHope(fecha, fecha, oi_personal_emp);
            if (dia.Count > 0)
            {
                DIA ddoDIA = (DIA)dia[0];
                NomadLog.Debug("DIA -- " + ddoDIA.SerializeAll());
                foreach (DETALLE ddoDET in ddoDIA.DETALLE)
                {
                    NucleusRH.Base.Tiempos_Trabajados.Tipos_Horas.TIPOHORA ddoTH = (NucleusRH.Base.Tiempos_Trabajados.Tipos_Horas.TIPOHORA)NomadProxy.GetProxy().CacheGetObj(ddoDET.oi_tipohora);
                    if (ddoTH == null)
                    {
                        ddoTH = NucleusRH.Base.Tiempos_Trabajados.Tipos_Horas.TIPOHORA.Get(ddoDET.oi_tipohora);
                        NomadProxy.GetProxy().CacheAdd(ddoDET.oi_tipohora, ddoTH);
                    }
                    NomadLog.Debug("e_sumariza_aus -- " + ddoTH.e_sumariza_aus);
                    if (ddoTH.e_sumariza_aus == 2 && (!ddoTH.l_requiereaut || ddoDET.l_autorizada))
                    {
                        NomadLog.Debug("esperado -- " + esperado);
                        if (!esperado)
                        {
                            hdesde = ddoDET.e_horainicio;
                            hhasta = ddoDET.e_horafin;
                            esperado = true;
                        }
                        else
                        {
                            if (hdesde > ddoDET.e_horainicio)
                                hdesde = ddoDET.e_horainicio;
                            if (hhasta < ddoDET.e_horafin)
                                hhasta = ddoDET.e_horafin;
                        }
                    }
                }
                NomadLog.Debug("hdesde -- " + hdesde);
                NomadLog.Debug("hhasta -- " + hhasta);
            }
        }

        /// <summary>
        /// Retorna un ArrayList con los días involucrados en la esperanza solicidata.
        /// </summary>
        /// <param name="pFromDate">Fecha desde en el rango solicitado.</param>
        /// <param name="pToDate">Fecha hasta en el rango solicitado.</param>
        /// <param name="pOiPersonalEmp">Oi_PersonalEmp del Legajo.</param>
        /// <returns></returns>
        public static ArrayList GetDaysHope(DateTime pFromDate, DateTime pToDate, int pOiPersonalEmp)
        {
            ArrayList arrResult = new ArrayList();
            int intFrom = StringUtil.date2week(pFromDate);
            int intTo = StringUtil.date2week(pToDate);
            ESPERANZAPER objEsp;

            //Obtiene la esperanza por cada semana y se guarda los dias
            for (int xWeek = intFrom; xWeek <= intTo; xWeek++)
            {
                objEsp = ESPERANZAPER.GetHope(xWeek, pOiPersonalEmp);

                foreach (DIA objDia in objEsp.DIAS)
                {
                    if (objDia.f_dia >= pFromDate && objDia.f_dia <= pToDate)
                    {
                        arrResult.Add(objDia);
                    }
                }
            }
            return arrResult;
        }

        /// <summary>
        /// Retorna la esperanza de un DIA.
        /// </summary>
        /// <param name="date">Fecha de esperanza solicitada.</param>
        /// <param name="pOiPersonalEmp">Oi_PersonalEmp del Legajo.</param>
        /// <returns></returns>
        public static DIA GetDayHope(DateTime date, int pOiPersonalEmp)
        {
            int intWeek = StringUtil.date2week(date);
            ESPERANZAPER objEsp;

            //Obtiene la esperanza por cada semana y se guarda los dias
            objEsp = ESPERANZAPER.GetHope(intWeek, pOiPersonalEmp);

            foreach (DIA objDia in objEsp.DIAS)
                if (objDia.f_dia == date)
                    return objDia;
            return null;
        }

        /// <summary>
        /// Retorna el DDO de la EsperanzaPer solicitada
        /// </summary>
        /// <param name="pWeek">Semana.</param>
        /// <param name="pOiPersonalEmp">Oi_PersonalEmp del Legajo.</param>
        /// <returns></returns>
        public static ESPERANZAPER GetHope(int pWeek, int pOiPersonalEmp)
        {
            ESPERANZAPER objEsperanzaPer;
            string strCacheKey = pWeek.ToString() + "|" + pOiPersonalEmp.ToString();

            //Realiza validaciones simples
            if (pWeek < 0) throw new NomadAppException("El número de semana no puede ser negativo.");
            if (pOiPersonalEmp <= 0) throw new NomadAppException("El OI_PersonalEmp requerido no es válido.");

            //Busca en el CACHE
            Hashtable htaPerHopes = (Hashtable)NomadProxy.GetProxy().CacheGetObj("GetHOPEPer");
            if (htaPerHopes == null)
            {
                htaPerHopes = new Hashtable();
                NomadProxy.GetProxy().CacheAdd("GetHOPEPer", htaPerHopes);
            }
            objEsperanzaPer = (ESPERANZAPER)(htaPerHopes.ContainsKey(strCacheKey) ? htaPerHopes[strCacheKey] : null);

            if (objEsperanzaPer == null)
            {
                //No existe en el CACHE. Se GENERA y luego lo guarda.
                HopeGenerator objHG = new HopeGenerator(pWeek, pOiPersonalEmp);
                objEsperanzaPer = objHG.Get();

                htaPerHopes[strCacheKey] = objEsperanzaPer;
            }

            return objEsperanzaPer;
        }

        //--------------------------------------------------------------------------------------------------------
        // CLASES
        //--------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Objeto que genera la ESPERANZA PERSONAL
        /// </summary>
        public class HopeGenerator
        {
            private DataHash objDataHash;

            /// <summary>
            ///
            /// </summary>
            /// <param name="pWeek"></param>
            /// <param name="pOiPersonalEmp"></param>
            public HopeGenerator(int pWeek, int pOiPersonalEmp)
            {
                objDataHash = new DataHash();
                objDataHash.Oi_personal = pOiPersonalEmp;
                objDataHash.Week = pWeek;
            }

            //--------------------------------------------------------------------------------------------------------
            // Métodos PÚBLICOS
            //--------------------------------------------------------------------------------------------------------

            /// <summary>
            /// Retorna el DDO de la esperanza personal. Se generará automáticamente en caso de no existir o estar desactualizada.
            /// </summary>
            /// <returns></returns>
            public ESPERANZAPER Get()
            {
                ESPERANZAPER objEsperanzaPer = null;

                //Obtiene los datos para el proceso y genera el HASH
                objDataHash.GenerateHash();

                return this.Generate(objEsperanzaPer);
            }

            //--------------------------------------------------------------------------------------------------------
            // Métodos PRIVADOS
            //--------------------------------------------------------------------------------------------------------

            /// <summary>
            /// Genera la esperanza
            /// </summary>
            /// <param name="pobjEsp"></param>
            /// <returns></returns>
            private ESPERANZAPER Generate(ESPERANZAPER pobjEsp)
            {
                DIA objDia;
                DETALLE objDetalle;
                ArrayList arrDetalles;
                ArrayList arrDetallesTemp;
                NucleusRH.Base.Tiempos_Trabajados.Esperanza.ESPERANZA objHorEsp;
                NucleusRH.Base.Tiempos_Trabajados.Esperanza.DIA objHorDia;
                NucleusRH.Base.Tiempos_Trabajados.Esperanza.DETALLE objHorDet;
                NomadXML nmxTEMP;
                ArrayList arrTEMP;
                NomadXML nxmHA;
                NomadXML nxmNov;

                if (pobjEsp == null)
                {
                    //Se debe crear una Semana desde cero. Se realizará un INSERT.
                    pobjEsp = new ESPERANZAPER();
                    pobjEsp.oi_personal_emp = this.objDataHash.Oi_personal.ToString();
                    pobjEsp.e_semana = this.objDataHash.Week;
                    pobjEsp.d_esperanzaper = StringUtil.week2descr(pobjEsp.e_semana);

                }
                else
                {
                    //Ya existe la semana y se regenerará la esperanza. Se realizará un UPDATE.
                    pobjEsp.DIAS.Clear();
                }

                for (NomadXML nxmDay = this.objDataHash.Days.FirstChild(); nxmDay != null; nxmDay = nxmDay.Next())
                {

                    objHorEsp = this.objDataHash.GetEsperanzaHor(int.Parse(nxmDay.GetAttr("id")));
                    if (objHorEsp != null && nxmDay.GetAttr("oi_calendario") != "")
                    {

                        //Crea el nuevo DIA y completa con los valores
                        objDia = new DIA();

                        objDia.f_dia = StringUtil.str2date(nxmDay.GetAttr("fecha"));
                        objDia.oi_calendario = nxmDay.GetAttr("oi_calendario");
                        if (nxmDay.GetAttr("oi_horario") != "") objDia.oi_horario = nxmDay.GetAttr("oi_horario");
                        if (nxmDay.GetAttr("oi_escuadra") != "") objDia.oi_escuadra = nxmDay.GetAttr("oi_escuadra");

                        objHorDia = (NucleusRH.Base.Tiempos_Trabajados.Esperanza.DIA)objHorEsp.DIAS.GetByAttribute("f_dia", objDia.f_dia);

                        //El horario puede venir sin dias. Hace un continue del for de dias.
                        if (objHorDia == null)
                        {
                            //Nomad.NSystem.Base.NomadBatch.Trace("La esperanza horaria no tiene el dia '" + objDia.f_dia.ToString("dd/MM/yyyy") + "' definido");
                            continue;
                        }

                        objDia.c_tipo = objHorDia.c_tipo;
                        objDia.e_posicion = objHorDia.e_posicion;
                        objDia.oi_turno = objHorDia.oi_turno;

                        //Copia el detalle tal cual viene del horário
                        arrDetalles = new ArrayList();
                        for (int intDet = 0; intDet < objHorDia.DETALLE.Count; intDet++)
                        {
                            objHorDet = (NucleusRH.Base.Tiempos_Trabajados.Esperanza.DETALLE)objHorDia.DETALLE[intDet];

                            objDetalle = new DETALLE();
                            objDetalle.e_horainicio = objHorDet.e_horainicio;
                            objDetalle.e_horafin = objHorDet.e_horafin;
                            objDetalle.oi_tipohora = objHorDet.oi_tipohora;
                            objDetalle.oi_estructura = objHorDet.oi_estructura;

                            nmxTEMP = this.objDataHash.GetTipoHora(objHorDet.oi_tipohora);
                            objDetalle.l_autorizada = nmxTEMP.GetAttr("l_RequiereAut") == "0"; //Si la hora requiere autorización va falso, si no true

                            arrDetalles.Add(objDetalle);

                        }

                        // ----------------------------------------------------------
                        // -- SE APLICAN LAS LICENCIAS
                        if (nxmDay.GetAttr("oi_licencia") != "")
                        {
                            objDia.oi_licencia = nxmDay.GetAttr("oi_licencia");

                            for (int intDet = 0; intDet < arrDetalles.Count; intDet++)
                            {
                                objDetalle = (DETALLE)arrDetalles[intDet];

                                //Se obtiene el tipo de hora correspondiente al detalle que cae dentro de la licencia
                                nmxTEMP = this.objDataHash.GetTipoHora(objDetalle.oi_tipohora);

                                //2 = SumarizaEnAusente
                                //Si el tipo de hora sumariza en ausente se reemplaza por el tipo de hora indicado en la licencia
                                if (((NucleusRH.Base.Tiempos_Trabajados.Sumarizaciones)int.Parse(nmxTEMP.GetAttr("e_sumariza_aus"))) == NucleusRH.Base.Tiempos_Trabajados.Sumarizaciones.EnAusenteExedente)
                                    objDetalle.oi_tipohora = nxmDay.GetAttr("oi_tipohoralic");

                            }

                        }

                        // ----------------------------------------------------------
                        // -- SE APLICAN LAS HORAS AUTORIZADAS
                        arrTEMP = this.objDataHash.GetHorasAut(objDia.f_dia);
                        if (arrTEMP.Count > 0)
                        {

                            //Recorre las horas autorizadas para el día.
                            for (int x = 0; x < arrTEMP.Count; x++)
                            {
                                nxmHA = (NomadXML)arrTEMP[x];

                                DETALLE objTEMPDet;
                                arrDetallesTemp = new ArrayList();

                                //Recorre los detalles y los va pasando a un array temporal
                                for (int intDet = 0; intDet < arrDetalles.Count; intDet++)
                                {
                                    objDetalle = (DETALLE)arrDetalles[intDet];

                                    //Pregunta si el detalle se encuentra dentro del rango.
                                    if (objDetalle.e_horafin > int.Parse(nxmHA.GetAttr("e_horainicio"))
                                        && objDetalle.e_horainicio < int.Parse(nxmHA.GetAttr("e_horafin")))
                                    {

                                        //Si la hora ya está autorizada continua con la siguiente
                                        if (objDetalle.l_autorizada)
                                        {
                                            arrDetallesTemp.Add(objDetalle);
                                            continue;
                                        }

                                        objDetalle.l_autorizada = true;

                                        //Según los rangos determina si es necesesario crear nuevos detalles
                                        if (objDetalle.e_horainicio < int.Parse(nxmHA.GetAttr("e_horainicio")))
                                        {
                                            //Se agrega una hora antes
                                            objTEMPDet = new DETALLE();
                                            objTEMPDet.e_horainicio = objDetalle.e_horainicio;
                                            objTEMPDet.e_horafin = int.Parse(nxmHA.GetAttr("e_horainicio"));
                                            objTEMPDet.oi_estructura = objDetalle.oi_estructura;
                                            objTEMPDet.oi_tipohora = objDetalle.oi_tipohora;
                                            objTEMPDet.l_autorizada = false;

                                            //Se cambia de hora inicio a la actual
                                            objDetalle.e_horainicio = objTEMPDet.e_horafin;

                                            //Agrega la hora recientemente creada
                                            arrDetallesTemp.Add(objTEMPDet);
                                        }

                                        //Agrega la hora original
                                        arrDetallesTemp.Add(objDetalle);

                                        //Según los rangos determina si es necesesario crear nuevos detalles
                                        if (objDetalle.e_horafin > int.Parse(nxmHA.GetAttr("e_horafin")))
                                        {
                                            //Se agrega una hora después
                                            objTEMPDet = new DETALLE();
                                            objTEMPDet.e_horainicio = int.Parse(nxmHA.GetAttr("e_horafin"));
                                            objTEMPDet.e_horafin = objDetalle.e_horafin;
                                            objTEMPDet.oi_estructura = objDetalle.oi_estructura;
                                            objTEMPDet.oi_tipohora = objDetalle.oi_tipohora;
                                            objTEMPDet.l_autorizada = false;

                                            //Se cambia de hora fin a la actual
                                            objDetalle.e_horafin = objTEMPDet.e_horainicio;

                                            //Agrega la hora recientemente creada
                                            arrDetallesTemp.Add(objTEMPDet);
                                        }

                                    }
                                    else
                                    {
                                        arrDetallesTemp.Add(objDetalle);
                                    }
                                }

                                arrDetalles = arrDetallesTemp;
                            }
                        }

                        // ----------------------------------------------------------
                        // -- SE APLICAN LAS NOVEDADES
                        arrTEMP = this.objDataHash.GetNovedades(objDia.f_dia);
                        if (arrTEMP.Count > 0)
                        {

                            //Recorre las novedades para el día.
                            for (int x = 0; x < arrTEMP.Count; x++)
                            {
                                objDia.l_afectado_nov = true;

                                nxmNov = (NomadXML)arrTEMP[x];

                                DETALLE objTEMPDet;
                                DETALLE objTEMPDetPres;
                                arrDetallesTemp = new ArrayList();
                                bool bolNovAgregada = false;

                                //Se obtiene el tipo de hora correspondiente a la NOVEDAD
                                nmxTEMP = this.objDataHash.GetTipoHora(nxmNov.GetAttr("oi_tipohora"));

                                //1 = Sumariza (Sumariza en si mismo)
                                if (((NucleusRH.Base.Tiempos_Trabajados.Sumarizaciones)int.Parse(nmxTEMP.GetAttr("e_sumariza_pre"))) == NucleusRH.Base.Tiempos_Trabajados.Sumarizaciones.Sumariza)
                                {
                                    //Es una NOVEDAD de presencia

                                    //Crea el nuevo DETALLE
                                    objTEMPDetPres = new DETALLE();
                                    objTEMPDetPres.e_horainicio = int.Parse(nxmNov.GetAttr("e_horainicio"));
                                    objTEMPDetPres.e_horafin = int.Parse(nxmNov.GetAttr("e_horafin"));
                                    objTEMPDetPres.oi_estructura = nxmNov.GetAttr("oi_estructura");
                                    objTEMPDetPres.oi_tipohora = nxmNov.GetAttr("oi_tipohora");
                                    objTEMPDetPres.l_autorizada = true;

                                    //Recorre los detalles y los va pasando a un array temporal
                                    for (int intDet = 0; intDet < arrDetalles.Count; intDet++)
                                    {
                                        objDetalle = (DETALLE)arrDetalles[intDet];

                                        //Pregunta si la novedad es enteramente anterior al detalle
                                        if (bolNovAgregada == false && objTEMPDetPres.e_horafin <= objDetalle.e_horainicio)
                                        {
                                            bolNovAgregada = true;
                                            arrDetallesTemp.Add(objTEMPDetPres);
                                        }

                                        //Pregunta si el detalle se encuentra solapado con la novedad.
                                        if (objDetalle.e_horafin > objTEMPDetPres.e_horainicio
                                            && objDetalle.e_horainicio < objTEMPDetPres.e_horafin)
                                        {

                                            //Según los rangos determina si es necesesario crear nuevos detalles
                                            if (objDetalle.e_horainicio < objTEMPDetPres.e_horainicio)
                                            {
                                                //Se agrega una hora antes
                                                objTEMPDet = new DETALLE();
                                                objTEMPDet.e_horainicio = objDetalle.e_horainicio;
                                                objTEMPDet.e_horafin = objTEMPDetPres.e_horainicio;
                                                objTEMPDet.oi_estructura = objDetalle.oi_estructura;
                                                objTEMPDet.oi_tipohora = objDetalle.oi_tipohora;
                                                objTEMPDet.l_autorizada = objDetalle.l_autorizada;

                                                //Agrega la hora recientemente creada
                                                arrDetallesTemp.Add(objTEMPDet);
                                            }

                                            //Se agrega el detalle con el rango y tipo de hora de la novedad
                                            if (!bolNovAgregada)
                                            {
                                                bolNovAgregada = true;

                                                //Agrega la hora recientemente creada
                                                arrDetallesTemp.Add(objTEMPDetPres);
                                            }

                                            //Según los rangos determina si es necesesario crear nuevos detalles
                                            if (objDetalle.e_horafin > objTEMPDetPres.e_horafin)
                                            {
                                                //Se agrega una hora después
                                                objTEMPDet = new DETALLE();
                                                objTEMPDet.e_horainicio = objTEMPDetPres.e_horafin;
                                                objTEMPDet.e_horafin = objDetalle.e_horafin;
                                                objTEMPDet.oi_estructura = objDetalle.oi_estructura;
                                                objTEMPDet.oi_tipohora = objDetalle.oi_tipohora;
                                                objTEMPDet.l_autorizada = objDetalle.l_autorizada;

                                                //Agrega la hora recientemente creada
                                                arrDetallesTemp.Add(objTEMPDet);
                                            }

                                        }
                                        else
                                        {
                                            arrDetallesTemp.Add(objDetalle);
                                        }
                                    }

                                    //Si no existian detalles se agrega el de la novedad
                                    if (!bolNovAgregada)
                                        arrDetallesTemp.Add(objTEMPDetPres);

                                }
                                else
                                {
                                    //Es una NOVEDAD de ausencia

                                    //Recorre los detalles y los va pasando a un array temporal
                                    for (int intDet = 0; intDet < arrDetalles.Count; intDet++)
                                    {
                                        objDetalle = (DETALLE)arrDetalles[intDet];

                                        //Pregunta si el detalle se encuentra dentro del rango.
                                        if (objDetalle.e_horafin > int.Parse(nxmNov.GetAttr("e_horainicio"))
                                            && objDetalle.e_horainicio < int.Parse(nxmNov.GetAttr("e_horafin")))
                                        {

                                            //Según los rangos determina si es necesesario crear nuevos detalles
                                            if (objDetalle.e_horainicio < int.Parse(nxmNov.GetAttr("e_horainicio")))
                                            {
                                                //Se agrega un detalle antes (como el que tenía)
                                                objTEMPDet = new DETALLE();
                                                objTEMPDet.e_horainicio = objDetalle.e_horainicio;
                                                objTEMPDet.e_horafin = int.Parse(nxmNov.GetAttr("e_horainicio"));
                                                objTEMPDet.oi_estructura = objDetalle.oi_estructura;
                                                objTEMPDet.oi_tipohora = objDetalle.oi_tipohora;
                                                objTEMPDet.l_autorizada = objDetalle.l_autorizada;

                                                //Se cambia de hora inicio a la actual
                                                objDetalle.e_horainicio = objTEMPDet.e_horafin;

                                                //Agrega la hora recientemente creada
                                                arrDetallesTemp.Add(objTEMPDet);
                                            }

                                            //Solo se tomarán los detalles en los que si falta se suma en AUSENTE
                                            nmxTEMP = this.objDataHash.GetTipoHora(objDetalle.oi_tipohora);
                                            if (((NucleusRH.Base.Tiempos_Trabajados.Sumarizaciones)int.Parse(nmxTEMP.GetAttr("e_sumariza_aus"))) == NucleusRH.Base.Tiempos_Trabajados.Sumarizaciones.EnAusenteExedente)
                                                arrDetallesTemp.Add(objDetalle);

                                            //Según los rangos determina si es necesesario crear nuevos detalles
                                            if (objDetalle.e_horafin > int.Parse(nxmNov.GetAttr("e_horafin")))
                                            {
                                                //Se agrega una hora después
                                                objTEMPDet = new DETALLE();
                                                objTEMPDet.e_horainicio = int.Parse(nxmNov.GetAttr("e_horafin"));
                                                objTEMPDet.e_horafin = objDetalle.e_horafin;
                                                objTEMPDet.oi_estructura = objDetalle.oi_estructura;
                                                objTEMPDet.oi_tipohora = objDetalle.oi_tipohora;
                                                objTEMPDet.l_autorizada = objDetalle.l_autorizada;

                                                //Se cambia de hora fin a la actual
                                                objDetalle.e_horafin = objTEMPDet.e_horainicio;

                                                //Agrega la hora recientemente creada
                                                arrDetallesTemp.Add(objTEMPDet);
                                            }

                                            //Se actualiza el tipo de hora del detalle original
                                            objDetalle.oi_tipohora = nxmNov.GetAttr("oi_tipohora");

                                        }
                                        else
                                        {
                                            arrDetallesTemp.Add(objDetalle);
                                        }

                                    }

                                }

                                arrDetalles = arrDetallesTemp;
                            }
                        }

                        // ----------------------------------------------------------
                        // -- SE AGREGAN LOS DETALLES PARA EL DIA
                        for (int intDet = 0; intDet < arrDetalles.Count; intDet++)
                            objDia.DETALLE.Add((DETALLE)arrDetalles[intDet]);

                        pobjEsp.DIAS.Add(objDia);
                    }

                }

                return pobjEsp;
            }

        }

        public class DataHash
        {

            public int Oi_personal;
            public NomadXML Days;

            private int m_intWeek;
            private DateTime m_dteFechaInicio;
            private DateTime m_dteFechaFin;
            private NomadXML m_nxmDataHash;
            private string m_strHash;

            private Hashtable m_htaHorDay = new Hashtable();
            private Hashtable m_htaTiposHoras = null;
            private Hashtable m_HorasAut = null;
            private Hashtable m_Novedades = null;

            //--------------------------------------------------------------------------------------------------------
            // Métodos PÚBLICOS
            //--------------------------------------------------------------------------------------------------------

            public void GenerateHash()
            {
                NucleusRH.Base.Tiempos_Trabajados.clsPreLoader objPreloader;
                NomadXML objQParams;

                //Obtiene el HASH de la esperanza
                objPreloader = (NucleusRH.Base.Tiempos_Trabajados.clsPreLoader)NomadProxy.GetProxy().CacheGetObj("HopePreLoader");

                if (objPreloader == null)
                {
                    //Ejecuta el query necesario para la obtención de los datos/hash
                    objQParams = new NomadXML("DATOS");
                    objQParams.SetAttr("oi_personal_emp", this.Oi_personal.ToString());
                    objQParams.SetAttr("f_ini", StringUtil.date2str(this.FechaInicio));
                    objQParams.SetAttr("f_fin", StringUtil.date2str(this.FechaFin));
                    this.m_nxmDataHash = NomadEnvironment.QueryNomadXML(ESPERANZAPER.Resources.QRY_HASH, objQParams.ToString(), false);
                    this.m_nxmDataHash = this.m_nxmDataHash.FirstChild();

                }
                else
                {
                    this.m_nxmDataHash = objPreloader.GetHASHLegajo(this.Oi_personal.ToString(), this.Week);
                }
                /*
                      string strH1, strH2, strH3;
                      //Ejecuta el query necesario para la obtención de los datos/hash
                      NomadXML objQParams = new NomadXML("DATOS");
                      objQParams.SetAttr("oi_personal_emp", this.Oi_personal.ToString());
                      objQParams.SetAttr("f_ini", StringUtil.date2str(this.FechaInicio));
                      objQParams.SetAttr("f_fin", StringUtil.date2str(this.FechaFin));
                      this.m_nxmDataHash = NomadEnvironment.QueryNomadXML(ESPERANZAPER.Resources.QRY_HASH, objQParams.ToString(), false);
                      strH1 = this.m_nxmDataHash.ToString(true);
                      NomadEnvironment.GetTrace().Info("HASH1: " + strH1);

                      objQParams = new NomadXML("PARAMS");
                      objQParams.SetAttr("oi_personal_emps", this.Oi_personal.ToString());
                      objQParams.SetAttr("f_ini", StringUtil.date2str(this.FechaInicio));
                      objQParams.SetAttr("f_fin", StringUtil.date2str(this.FechaFin));
                      objQParams.SetAttr("type", "ALL");
                      this.m_nxmDataHash = NomadEnvironment.QueryNomadXML(ESPERANZAPER.Resources.QRY_PRELOADHOPE, objQParams.ToString(), false);
                      strH2 = this.m_nxmDataHash.ToString(true);
                      NomadEnvironment.GetTrace().Info("HASH2: " + strH2);

                      this.m_nxmDataHash = objPreloader.GetHASHLegajo(this.Oi_personal.ToString(), this.Week);
                      strH3 = this.m_nxmDataHash.ToString(true);
                      NomadEnvironment.GetTrace().Info("HASH3: " + strH3);

                      if (strH1 != strH2) {
                        NomadEnvironment.GetTrace().Error("ERROR COMPARANDO: Los H1 y H2 son diferentes");
                      }
                      if (strH2 != strH3) {
                        NomadEnvironment.GetTrace().Error("ERROR COMPARANDO: Los H2 y H3 son diferentes");
                      }
                      if (strH1 != strH3) {
                        NomadEnvironment.GetTrace().Error("ERROR COMPARANDO: Los H1 y H3 son diferentes");
                      }
                */
                //Recorre los días y para cada uno
                int intOi_horario, intOi_calendario, intOi_escuadra;
                NomadXML nxmHor, nxmCal, nxmLic;
                NomadXML nxmDay;
                DateTime dteDayDate;
                NucleusRH.Base.Tiempos_Trabajados.Esperanza.ESPERANZA objHorEsp;

                nxmHor = this.m_nxmDataHash.FindElement("HORARIOS").FirstChild();
                nxmCal = this.m_nxmDataHash.FindElement("CALENDARIOS").FirstChild();
                nxmLic = this.m_nxmDataHash.FindElement("LICENCIAS").FirstChild();
                this.Days = this.m_nxmDataHash.FindElement("DETALLE_DIARIO");

                for (int xDay = 0; xDay < 7; xDay++)
                {
                    dteDayDate = this.FechaInicio.AddDays(xDay);

                    nxmDay = this.Days.AddTailElement("DIA");
                    nxmDay.SetAttr("id", xDay.ToString());
                    nxmDay.SetAttr("fecha", StringUtil.date2str(dteDayDate));

                    //Crea la entrada en el array de DDO de Horarios
                    this.m_htaHorDay.Add(xDay, null);

                    //Agrega los datos del calendario
                    if (nxmCal != null)
                    {
                        if (dteDayDate >= StringUtil.str2date(nxmCal.GetAttr("f_fin")))
                            nxmCal = nxmCal.Next();

                        if (nxmCal != null && dteDayDate >= StringUtil.str2date(nxmCal.GetAttr("f_ini")) && dteDayDate < StringUtil.str2date(nxmCal.GetAttr("f_fin")))
                        {
                            nxmDay.SetAttr("oi_calendario", nxmCal.GetAttr("oi_calendario"));
                        }
                    }

                    //Agrega los datos de la licencia
                    if (nxmLic != null)
                    {
                        if (dteDayDate > StringUtil.str2date(nxmLic.GetAttr("f_fin")))
                            nxmLic = nxmLic.Next();

                        if (nxmLic != null && dteDayDate >= StringUtil.str2date(nxmLic.GetAttr("f_ini")) && dteDayDate <= StringUtil.str2date(nxmLic.GetAttr("f_fin")))
                        {
                            nxmDay.SetAttr("oi_licencia", nxmLic.GetAttr("oi_licencia"));
                            nxmDay.SetAttr("oi_tipohoralic", nxmLic.GetAttr("oi_tipohora"));
                        }
                    }

                    //Agrega los datos del horario
                    if (nxmHor != null)
                    {
                        if (dteDayDate >= StringUtil.str2date(nxmHor.GetAttr("f_fin")))
                            nxmHor = nxmHor.Next();

                        if (nxmHor != null && dteDayDate >= StringUtil.str2date(nxmHor.GetAttr("f_ini")) && dteDayDate < StringUtil.str2date(nxmHor.GetAttr("f_fin")))
                        {
                            nxmDay.SetAttr("oi_horario", nxmHor.GetAttr("oi_horario"));
                            nxmDay.SetAttr("oi_escuadra", nxmHor.GetAttr("oi_escuadra"));

                            intOi_horario = nxmDay.GetAttr("oi_horario") != "" ? int.Parse(nxmDay.GetAttr("oi_horario")) : 0;
                            intOi_calendario = nxmDay.GetAttr("oi_calendario") != "" ? int.Parse(nxmDay.GetAttr("oi_calendario")) : 0;
                            intOi_escuadra = nxmDay.GetAttr("oi_escuadra") != "" ? int.Parse(nxmDay.GetAttr("oi_escuadra")) : 0;

                            if (intOi_calendario == 0)
                            {
                                throw NomadException.NewMessage("Esperanzaper.ESPERANZAPER.ERR-CALENDARIO");
                            }

                            //Obtiene el HASH y se guarda el DDO del Horario
                            objHorEsp = NucleusRH.Base.Tiempos_Trabajados.Horarios.clsCreaEsperanzaH.GetHope(intOi_horario, this.Week, intOi_calendario, intOi_escuadra);
                            this.m_htaHorDay[xDay] = objHorEsp;

                            nxmDay.SetAttr("hash", objHorEsp.Hash);
                        }
                    }
                }

                //Genera el hash
                this.m_strHash = StringUtil.GetMD5(this.m_nxmDataHash.ToString() + "||" + this.GetType().Assembly.GetFiles()[0].Name);
            }

            public NucleusRH.Base.Tiempos_Trabajados.Esperanza.ESPERANZA GetEsperanzaHor(int pDay)
            {
                if (this.m_htaHorDay.ContainsKey(pDay))
                    return (NucleusRH.Base.Tiempos_Trabajados.Esperanza.ESPERANZA)this.m_htaHorDay[pDay];
                else
                    return null;
            }

            /// <summary>
            /// Retorna el tipo de hora pedido
            /// </summary>
            /// <param name="pstrTH">Oi del tipo de hora</param>
            /// <returns></returns>
            public NomadXML GetTipoHora(string pstrTH)
            {

                if (this.m_htaTiposHoras == null)
                {
                    this.m_htaTiposHoras = new Hashtable();

                    //El hash no está creado. Lo genera y lo completa.
                    for (NomadXML nxmTH = this.TiposHoras.FirstChild(); nxmTH != null; nxmTH = nxmTH.Next())
                        this.m_htaTiposHoras.Add(nxmTH.GetAttr("oi_TipoHora"), nxmTH);
                }

                if (this.m_htaTiposHoras.ContainsKey(pstrTH))
                    return (NomadXML)this.m_htaTiposHoras[pstrTH];
                else
                    throw new NomadAppException("Se está pidiendo un tipo de hora no definido en la tabla de tipos de hora. OI_TIPOHORA='" + pstrTH + "'");
            }

            /// <summary>
            /// Retorna un ArrayList con las horas autorizadas para ese día
            /// </summary>
            /// <param name="pDate"></param>
            /// <returns></returns>
            public ArrayList GetHorasAut(DateTime pDate)
            {
                ArrayList arrResult = new ArrayList();
                DateTime dteInicio, dteFin;

                if (this.m_HorasAut == null)
                {
                    ArrayList arrTemp;
                    this.m_HorasAut = new Hashtable();

                    //El hash no está creado. Lo genera y lo completa.
                    for (NomadXML nxmHA = this.HorasAut.FirstChild(); nxmHA != null; nxmHA = nxmHA.Next())
                    {
                        dteInicio = StringUtil.str2date(nxmHA.GetAttr("f_ini"));
                        dteFin = StringUtil.str2date(nxmHA.GetAttr("f_fin"));

                        for (DateTime dteDia = dteInicio; dteDia <= dteFin; dteDia = dteDia.AddDays(1d))
                        {
                            if (this.m_HorasAut.ContainsKey(dteDia.ToString("yyyyMMdd")))
                            {
                                arrTemp = (ArrayList)this.m_HorasAut[dteDia.ToString("yyyyMMdd")];
                            }
                            else
                            {
                                arrTemp = new ArrayList();
                                this.m_HorasAut[dteDia.ToString("yyyyMMdd")] = arrTemp;
                            }
                            arrTemp.Add(nxmHA);
                        }
                    }
                }

                if (this.m_HorasAut.ContainsKey(pDate.ToString("yyyyMMdd")))
                    arrResult = (ArrayList)this.m_HorasAut[pDate.ToString("yyyyMMdd")];

                return arrResult;
            }

            /// <summary>
            /// Retorna un ArrayList con las novedades para ese día
            /// </summary>
            /// <param name="pDate"></param>
            /// <returns></returns>
            public ArrayList GetNovedades(DateTime pDate)
            {
                ArrayList arrResult = new ArrayList();
                string strNovedad;

                if (this.m_Novedades == null)
                {
                    ArrayList arrTemp;
                    this.m_Novedades = new Hashtable();

                    //El hash no está creado. Lo genera y lo completa.
                    for (NomadXML nxmNov = this.Novedades.FirstChild(); nxmNov != null; nxmNov = nxmNov.Next())
                    {
                        strNovedad = nxmNov.GetAttr("f_fecha");

                        if (this.m_Novedades.ContainsKey(strNovedad))
                        {
                            arrTemp = (ArrayList)this.m_Novedades[strNovedad];
                        }
                        else
                        {
                            arrTemp = new ArrayList();
                            this.m_Novedades[strNovedad] = arrTemp;
                        }
                        arrTemp.Add(nxmNov);
                    }
                }

                if (this.m_Novedades.ContainsKey(pDate.ToString("yyyyMMdd")))
                    arrResult = (ArrayList)this.m_Novedades[pDate.ToString("yyyyMMdd")];

                return arrResult;
            }

            //--------------------------------------------------------------------------------------------------------
            // Métodos PRIVADOS
            //--------------------------------------------------------------------------------------------------------

            //--------------------------------------------------------------------------------------------------------
            // Propiedades
            //--------------------------------------------------------------------------------------------------------

            public int Week
            {
                get { return this.m_intWeek; }
                set
                {
                    this.m_intWeek = value;

                    //Obtiene las fechas correspondientes a la semana
                    this.m_dteFechaInicio = StringUtil.week2date(value);
                    this.m_dteFechaFin = this.m_dteFechaInicio.AddDays(6);
                }
            }

            public DateTime FechaInicio
            {
                get { return this.m_dteFechaInicio; }
            }
            public DateTime FechaFin
            {
                get { return this.m_dteFechaFin; }
            }

            public string Hash
            {
                get { return this.m_strHash; }
            }

            public NomadXML HorasAut
            {
                get { return this.m_nxmDataHash.FindElement("HORASAUT"); }
            }

            public NomadXML Novedades
            {
                get { return this.m_nxmDataHash.FindElement("NOVEDADES"); }
            }

            public NomadXML TiposHoras
            {
                get { return this.m_nxmDataHash.FindElement("TIPOSHORAS"); }
            }
        }
    }
}



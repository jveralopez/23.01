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
using System.Collections.Generic;

namespace NucleusRH.Base.Vacaciones.LegajoVacaciones
{

    //////////////////////////////////////////////////////////////////////////////////
    //Clase Legajo - Vacaciones
    public partial class PERSONAL_EMP : NucleusRH.Base.Personal.LegajoEmpresa.PERSONAL_EMP
    {
        public struct datos_licencia
        {
            public string oi_personal_emp;
            public string oi_licencia;
            public DateTime f_inicio;
        }

        public int Dias_Feriado(NucleusRH.Base.Organizacion.Calendario_Feriados.CALENDARIO calendario, DateTime f_desde, DateTime f_hasta)
        {
            int cant_dias_feriado = 0;

            bool[] week = new bool[7] { calendario.l_trab_domingos, calendario.l_trab_lunes, calendario.l_trab_martes, calendario.l_trab_miercoles, calendario.l_trab_jueves, calendario.l_trab_viernes, calendario.l_trab_sabados };

            if (calendario.l_trab_feriados)
                return 0;
            else
            {
                foreach (NucleusRH.Base.Organizacion.Calendario_Feriados.DIA_FERIADO feriado in calendario.DIAS_FERIADOS)
                {
					if(feriado.c_tipo != "NOLAB")
					{
						if (feriado.f_feriado > f_desde && feriado.f_feriado < f_hasta)
						{
							if (week[(int)feriado.f_feriado.DayOfWeek])
							{
								cant_dias_feriado++;
							}
						}
					}   
                }
            }
            NomadEnvironment.GetTrace().Info("Cant Dias Feriados: " + cant_dias_feriado.ToString());
            return cant_dias_feriado;
        }

        public static void Datos_Gantt(Nomad.NSystem.Document.NmdXmlDocument filtros)
        {
            NomadBatch objBatch;
            objBatch = NomadBatch.GetBatch("Iniciando...", "Gantt de Vacaciones");

            objBatch.SetPro(0);

            StringWriter swr = new StringWriter();
            XmlTextWriter xtw = new XmlTextWriter(swr);
            xtw.Formatting = Formatting.None;

            NucleusRH.Base.Organizacion.Calendario_Feriados.CALENDARIO calendario = NucleusRH.Base.Organizacion.Calendario_Feriados.CALENDARIO.Get(filtros.GetAttribute("oi_calendario").Value);
            bool[] week = new bool[7] { calendario.l_trab_domingos, calendario.l_trab_lunes, calendario.l_trab_martes, calendario.l_trab_miercoles, calendario.l_trab_jueves, calendario.l_trab_viernes, calendario.l_trab_sabados };

            // Recurso para obtener las personas de las solicitudes
            // XML de parametros para el query
            string xmlparam = filtros.ToString();

            // Ejecuto el recurso
            Nomad.NSystem.Document.NmdXmlDocument xml_personas = null;
            xml_personas = new Nomad.NSystem.Document.NmdXmlDocument(NomadEnvironment.QueryString(NucleusRH.Base.Vacaciones.LegajoVacaciones.PERSONAL_EMP.Resources.QRY_PERSONAS_SOLIC, xmlparam));
            // FIN Recurso para obtener las personas de las solicitudes

            //Aqui formo el xml con XmlTextWriter
            xtw.WriteStartElement("DATOS");

            //Configuro los colores por estado
            Hashtable colorsTable = new Hashtable();
            ArrayList myColors = new ArrayList();
            myColors.Add("#3EB793");
            myColors.Add("#EAB541");
            myColors.Add("#8C93FF");
            myColors.Add("#E0676D");
            myColors.Add("#F7F171");
            int c=0;

            //INCORPORA LOS ESTADO EN EL REPORTE
            if (filtros.GetAttribute("oi_estado_solic").Value== "-")
            {
                // Ejecuto el recurso
                Nomad.NSystem.Document.NmdXmlDocument xml_estados = null;
                xml_estados = new Nomad.NSystem.Document.NmdXmlDocument(NomadEnvironment.QueryString(NucleusRH.Base.Vacaciones.Estados_Solicitud.ESTADO_SOLIC.Resources.INFO, "<DATA/>"));
                Nomad.NSystem.Document.NmdXmlDocument estado;
                // Cargo los estados con la info del recurso
                xtw.WriteStartElement("ESTADOS");
                for (estado = (Nomad.NSystem.Document.NmdXmlDocument)xml_estados.GetFirstChildDocument(); estado != null; estado = (Nomad.NSystem.Document.NmdXmlDocument)xml_estados.GetNextChildDocument())
                {
                    xtw.WriteStartElement("ESTADO");
                    xtw.WriteAttributeString("c_estado", estado.GetAttribute("COD").Value);
                    xtw.WriteAttributeString("d_estado", estado.GetAttribute("DES").Value);
                    xtw.WriteAttributeString("color",myColors[c].ToString());

                    colorsTable.Add(estado.GetAttribute("COD").Value, myColors[c]);
                    c++;
                    //Si se agregaron mas estados de solicitud repite colores
                    if(c==5)
                      c=0;
                    xtw.WriteEndElement();
                }
                xtw.WriteEndElement();

            }

            Nomad.NSystem.Document.NmdXmlDocument persona;
            for (persona = (Nomad.NSystem.Document.NmdXmlDocument)xml_personas.GetFirstChildDocument(); persona != null; persona = (Nomad.NSystem.Document.NmdXmlDocument)xml_personas.GetNextChildDocument())
            {
                xtw.WriteStartElement("PERSONA");
                xtw.WriteAttributeString("d_ape_y_nom", persona.GetAttribute("d_ape_y_nom").Value);
                xtw.WriteAttributeString("e_numero_legajo", persona.GetAttribute("e_numero_legajo").Value);
                xtw.WriteAttributeString("legajo", persona.GetAttribute("legajo").Value);
                xtw.WriteEndElement();
            }
            DateTime f_hasta = Nomad.NSystem.Functions.StringUtil.str2date(filtros.GetAttribute("f_hasta").Value);
            DateTime fecha = Nomad.NSystem.Functions.StringUtil.str2date(filtros.GetAttribute("f_desde").Value);
            DateTime f_inicio_mes;

            while (fecha <= f_hasta)
            {
                f_inicio_mes = fecha;
                int mes = fecha.Month;
                xtw.WriteStartElement("MES");
                xtw.WriteAttributeString("mes", fecha.ToString("MMM"));
                xtw.WriteAttributeString("anio", fecha.Year.ToString());
                xtw.WriteStartElement("DIAS");
                ArrayList dias_mes = new ArrayList();
                while (fecha.Month == mes && fecha <= f_hasta)
                {
                    xtw.WriteStartElement("DIA");
                    xtw.WriteAttributeString("dia", fecha.Day.ToString());
                    bool feriado = false;
                    string est;

                    // Marca cada dia como; habil, no habil, o feriado
                    foreach (NucleusRH.Base.Organizacion.Calendario_Feriados.DIA_FERIADO dia_feriado in calendario.DIAS_FERIADOS)
                    {
						if(dia_feriado.c_tipo !="NOLAB")
						{
							if (dia_feriado.f_feriado == fecha)
							{
								feriado = true;
							}	
						}
                        
                    }
                    if (!feriado)
                    {
                        if (week[(int)fecha.DayOfWeek])
                        {
                            est = "h";
                        }
                        else
                        {
                            est = "n";
                        }
                    }
                    else
                    {
                        est = "f";
                    }
                    // Fin Marca cada dia como; habil, no habil, o feriado

                    // Marca el dia de hoy
                    if (fecha == DateTime.Today)
                    {
                        est = "hoy";
                    }
                    // Marca el dia de hoy

                    xtw.WriteAttributeString("est", est);
                    dias_mes.Add(est);
                    xtw.WriteEndElement();
                    fecha = fecha.AddDays(1);
                }
                xtw.WriteEndElement();
                xtw.WriteStartElement("PERSONAS");

                for (persona = (Nomad.NSystem.Document.NmdXmlDocument)xml_personas.GetFirstChildDocument(); persona != null; persona = (Nomad.NSystem.Document.NmdXmlDocument)xml_personas.GetNextChildDocument())
                {
                    Nomad.NSystem.Document.NmdXmlDocument solicitudes = (Nomad.NSystem.Document.NmdXmlDocument)persona.GetDocumentByName("SOLICITUDES");
                    xtw.WriteStartElement("PERSONA");
                    fecha = f_inicio_mes;
                    int cont = 0;
                    while (fecha.Month == mes && fecha <= f_hasta)
                    {
                        string sol = "n";
                        string estado = "-";
                        Nomad.NSystem.Document.NmdXmlDocument solicitud;
                        xtw.WriteStartElement("DIA");

                        // Recorro las solicitudes de la persona
                        // Marca cada dia como asignado o no asignado
                        for (solicitud = (Nomad.NSystem.Document.NmdXmlDocument)solicitudes.GetFirstChildDocument(); solicitud != null; solicitud = (Nomad.NSystem.Document.NmdXmlDocument)solicitudes.GetNextChildDocument())
                        {
                            DateTime f_desde_solic = Nomad.NSystem.Functions.StringUtil.str2date(solicitud.GetAttribute("f_desde").Value);
                            DateTime f_hasta_solic = Nomad.NSystem.Functions.StringUtil.str2date(solicitud.GetAttribute("f_hasta").Value);

                            if (fecha >= f_desde_solic && fecha <= f_hasta_solic)
                            {
                                sol = "a";
                                xtw.WriteAttributeString("app",NomadProxy.GetProxy().Application.Name);
                            xtw.WriteAttributeString("oi_solicitud", solicitud.GetAttribute("oi_solicitud").Value);
                                if (filtros.GetAttribute("oi_estado_solic").Value== "-")
                                {
                                    estado=solicitud.GetAttribute("c_estado_solic").Value;
                                    xtw.WriteAttributeString("color", colorsTable[estado].ToString());
                                }
                                break;
                            }
                        }
                        xtw.WriteAttributeString("est", dias_mes[cont].ToString() + sol);
                        xtw.WriteAttributeString("estado", estado);
                        xtw.WriteEndElement();
                        fecha = fecha.AddDays(1);
                        cont++;
                    }
                    xtw.WriteEndElement();
                    objBatch.SetMess("Recorriendo días");
                    objBatch.SetPro(0, 100, cont, DateTime.Compare(fecha, f_hasta));

                }
                xtw.WriteEndElement();
                xtw.WriteEndElement();
            }

            xtw.Flush();
            xtw.Close();

            string myXml = swr.ToString();

            Nomad.NSystem.Document.NmdXmlDocument doc = new Nomad.NSystem.Document.NmdXmlDocument(myXml);

            NomadXML xmlrep = new NomadXML(doc.ToString());

            objBatch.SetMess("Ejecutando el Reporte...");
            NomadBatch.Trace(xmlrep.ToString());
            Nomad.NSystem.Html.NomadHtml nmdHtml = new Nomad.NSystem.Html.NomadHtml("NucleusRH.Base.Vacaciones.GanttVacaciones.rpt", xmlrep.ToString(), NomadProxy.GetProxy());

            string outFileName = NomadProxy.GetProxy().Batch().ID + ".htm";
            string outFilePath = NomadProxy.GetProxy().RunPath + "Nomad\\TEMP\\";
            NomadBatch.Trace("Generando Reporte HTML en path:'" + outFilePath + outFileName + "'");

            StreamWriter sw = new System.IO.StreamWriter(outFilePath + "\\" + outFileName, false, System.Text.Encoding.UTF8);

            nmdHtml.GenerateHtml(sw.BaseStream);

            sw.Close();
        }

        public static Nomad.NSystem.Document.NmdXmlDocument Datos_Notificacion(Nomad.NSystem.Document.NmdXmlDocument personas, bool l_reimprime)
        {
            Nomad.NSystem.Document.NmdXmlDocument persona;
            StringWriter swr = new StringWriter();
            XmlTextWriter xtw = new XmlTextWriter(swr);
            xtw.Formatting = Formatting.None;
            xtw.WriteStartElement("PERSONAS");

            // Obtengo el parametro de dias de anticipacion para notificar
            string xmlparam = "<DATOS c_modulo=\"VAC\" c_parametro=\"NOTIF\"/>";
            Nomad.NSystem.Document.NmdXmlDocument parametro = null;

            // Ejecuto el recurso
            NomadEnvironment.GetTrace().Info("parametro_vacaciones:: " + xmlparam);

            parametro = new Nomad.NSystem.Document.NmdXmlDocument(NomadEnvironment.QueryString(NucleusRH.Base.Organizacion.Parametros.PARAMETRO.Resources.GET_PARAM, xmlparam));

            NomadEnvironment.GetTrace().Info("parametro:: " + parametro.ToString());
            int parametro_vacaciones;
            try
            {
                parametro_vacaciones = int.Parse(parametro.GetAttribute("d_valor").Value);
            }
            catch (Exception e)
            {
                throw new NomadAppException("El parametro NOTIF del modulo Vacaciones no tiene valor");
            }
            
            NomadEnvironment.GetTrace().Info("parametro_vacaciones:: " + parametro_vacaciones);

            // Por cada persona del xml de parametro,
            // si tiene f_inicio_vac ejecuta Notif_Vac_Fecha, sino
            // ejecuta Notif_Vac_Solic
            NomadEnvironment.GetTrace().Info("antes del for:: ");
            for (persona = (Nomad.NSystem.Document.NmdXmlDocument)personas.GetFirstChildDocument().GetFirstChildDocument(); persona != null; persona = (Nomad.NSystem.Document.NmdXmlDocument)personas.GetFirstChildDocument().GetNextChildDocument())
            {
                NomadEnvironment.GetCurrentTransaction().Begin();
                try
                {
                    NomadEnvironment.GetTrace().Info("en el try:: ");

                    if (persona.GetAttribute("f_inicio_vac") != null)
                    {
                        NomadEnvironment.GetTrace().Info("Posee f_inicio_vac");
                        DateTime f_inicio_vac = Nomad.NSystem.Functions.StringUtil.str2date(persona.GetAttribute("f_inicio_vac").Value);
                        TimeSpan a = new TimeSpan();
                        a.Add(f_inicio_vac - DateTime.Today);
                        if (a.TotalDays <= parametro_vacaciones)
                            PERSONAL_EMP.Notif_Vac_Fecha(persona, l_reimprime, ref xtw);
                    }
                    else
                    {
                        NomadEnvironment.GetTrace().Info("No posee f_inicio_vac");
                        PERSONAL_EMP.Notif_Vac_Solic(persona, l_reimprime, ref xtw, parametro_vacaciones);
                    }
                    NomadEnvironment.GetCurrentTransaction().Commit();
                }

                catch (Exception e)
                {
                    NomadEnvironment.GetCurrentTransaction().Rollback();
                    throw new NomadAppException("Error Notificando Vacaciones:: " + e.Message + e.StackTrace);
                }
            }

            // Libera los recursos
            xtw.Flush();
            xtw.Close();

            // Xml para devolver
            string myXml = swr.ToString();
            Nomad.NSystem.Document.NmdXmlDocument resultado = new Nomad.NSystem.Document.NmdXmlDocument(myXml);
            NomadLog.Info("resultado::  " + resultado.ToString());
            return resultado;
        }

        public static void Guardar_Aprobar_Solicitud(Nomad.NSystem.Proxy.NomadXML param)
        {
            try
            {
                param = param.FirstChild();

                NucleusRH.Base.Vacaciones.LegajoVacaciones.PERSONAL_EMP personal = PERSONAL_EMP.Get(param.GetAttr("oi_personal_emp"));

                string SolId = personal.AgregarSolicitud(
                    param.GetAttrDateTime("f_desde_solicitud"),
                    param.GetAttrDateTime("f_hasta_solicitud"),
                    param.GetAttrInt("e_dias_solicitud"),
                    param.GetAttrInt("e_dias_bonif"),
                    param.GetAttr("d_motivo_solic"),
                    param.GetAttr("o_solicitud"),
                    param.GetAttr("l_automatica"),
                    param.GetAttr("l_habiles"),
                    param.GetAttr("d_habiles")
                );

                PERSONAL_EMP.AprobarSolicitud(personal, SolId, param.GetAttrBool("force"));

                NomadEnvironment.GetCurrentTransaction().SaveRefresh(personal);
            }
			catch(NomadAppException e)
            {
				throw e;
			}
			catch(NomadException e)
            {
				throw e;
			}
            catch(Exception e)
            {
                throw new NomadAppException("Error al aprobar y guardar la solicitud.");
            }
        }

        public static string Guardar_Solicitud(Nomad.NSystem.Proxy.NomadXML param)
        {
            param = param.FirstChild();

            NucleusRH.Base.Vacaciones.LegajoVacaciones.PERSONAL_EMP personal = PERSONAL_EMP.Get(param.GetAttr("oi_personal_emp"));

            personal.AgregarSolicitud(
                param.GetAttrDateTime("f_desde_solicitud"),
                param.GetAttrDateTime("f_hasta_solicitud"),
                param.GetAttrInt("e_dias_solicitud"),
                param.GetAttrInt("e_dias_bonif"),
                param.GetAttr("d_motivo_solic"),
                param.GetAttr("o_solicitud"),
                param.GetAttr("l_automatica"),
                param.GetAttr("l_habiles"),
                param.GetAttr("d_habiles")
            );
            NomadEnvironment.GetCurrentTransaction().SaveRefresh(personal);
            return personal.SOLICITUDES[personal.SOLICITUDES.Count - 1].Id;
        }

        public static Nomad.NSystem.Proxy.NomadXML Saldo_Vacaciones(ref Nomad.NSystem.Proxy.NomadXML xmlparam)
        {
            NomadXML MyXML = xmlparam.FirstChild();
            NucleusRH.Base.Vacaciones.LegajoVacaciones.PERSONAL_EMP personal = PERSONAL_EMP.Get(MyXML.GetAttr("oi_personal_emp"));

            //Antiguedad
            MyXML.SetAttr("e_antiguedad", personal.Antiguedad_Anios());

            //Dias Pendientes
            int e_saldo = 0;
            foreach (CTA_CTE_VAC cuenta in personal.CTA_CTE_VAC)
            {
                if (cuenta.c_estado != "F")
                    e_saldo += cuenta.e_dias_pend;
                else
                    e_saldo -= cuenta.e_dias_otorg;
            }

            MyXML.SetAttr("e_saldo", e_saldo);

            return MyXML;
        }

        public static bool DiaLaboral(string oi_personal_emp, DateTime fecha)
        {
            //Obtengo la PERSONA
            NucleusRH.Base.Vacaciones.LegajoVacaciones.PERSONAL_EMP obj_Per = NucleusRH.Base.Vacaciones.LegajoVacaciones.PERSONAL_EMP.Get(oi_personal_emp);
            return obj_Per.DiaLaboral(fecha);
        }

        public static int DiasTrabajables(string oi_personal_emp, DateTime fecha_desde, DateTime fecha_hasta)
        {
            int retval = 0;
            NucleusRH.Base.Vacaciones.LegajoVacaciones.PERSONAL_EMP obj_Per = NucleusRH.Base.Vacaciones.LegajoVacaciones.PERSONAL_EMP.Get(oi_personal_emp);
            for (System.DateTime fecha = fecha_desde; fecha <= fecha_hasta; fecha = fecha.AddDays(1))
                if (obj_Per.DiaLaboral(fecha))
                    retval++;

            return retval;
        }

        /// <summary>
        /// Devuelve un string con 0 y 1 marcando con 1 los días hábiles
        /// </summary>
        /// <param name="fecha_desde"></param>
        /// <param name="fecha_hasta"></param>
        /// <returns></returns>
        public static string DiasTrabajablesString(string oi_personal_emp, DateTime fecha_desde, DateTime fecha_hasta)
        {
            string retval = "";
            NucleusRH.Base.Vacaciones.LegajoVacaciones.PERSONAL_EMP obj_Per = NucleusRH.Base.Vacaciones.LegajoVacaciones.PERSONAL_EMP.Get(oi_personal_emp);
            for (System.DateTime fecha = fecha_desde; fecha <= fecha_hasta; fecha = fecha.AddDays(1))
                if (obj_Per.DiaLaboral(fecha))
                    retval = retval + "1";
                else
                    retval = retval + "0";

            return retval;
        }

        public static bool DiasCorridos(string oi_personal_emp, DateTime fecha)
        {
            NucleusRH.Base.Vacaciones.LegajoVacaciones.PERSONAL_EMP obj_Per = NucleusRH.Base.Vacaciones.LegajoVacaciones.PERSONAL_EMP.Get(oi_personal_emp);
            return obj_Per.DiasCorridos(fecha);
        }

        public static Nomad.NSystem.Proxy.NomadXML RecalcularFechas(string oi_personal_emp, string fecha_desde, string fecha_hasta, string cant_dias)
        {
            NomadXML retval = new NomadXML("PARAM");
            string dias_bonif = "0", l_habiles = "0", d_habiles = "", str_habiles = "";
            try
            {
                int deadtime;
                System.DateTime desde = System.DateTime.Now;
                System.DateTime hasta = System.DateTime.Now;
                int cantDias;

                NucleusRH.Base.Vacaciones.LegajoVacaciones.PERSONAL_EMP obj_Per = NucleusRH.Base.Vacaciones.LegajoVacaciones.PERSONAL_EMP.Get(oi_personal_emp);

                desde = Nomad.NSystem.Functions.StringUtil.str2date(fecha_desde);
                if (fecha_desde != "" && fecha_hasta != "")
                {
                    hasta = Nomad.NSystem.Functions.StringUtil.str2date(fecha_hasta).AddDays(1);

                    cantDias = (int)((TimeSpan)(hasta - desde)).TotalDays;
                    if (cantDias < 1)
                        throw new NomadAppException("La fecha hasta tiene que ser mayor a la fecha fesde.");

                    if (cantDias >= 60)
                        throw new NomadAppException("Hay más de 60 días entre la fecha desde y hasta.");
                }

                if (cant_dias != "")
                {
                    cantDias = int.Parse(cant_dias);
                    if (cantDias < 1)
                        throw new NomadAppException("La cantidad de días tiene que ser mayor o igual a 1.");

                    if (cantDias >= 60)
                        throw new NomadAppException("La cantidad de días tiene que ser menor 60.");
                }

                NomadEnvironment.GetTrace().Info("DESDE -- " + desde.ToString());
                NomadEnvironment.GetTrace().Info("HASTA -- " + hasta.ToString());

                //Calcular la Fecha Hasta
                if (fecha_desde != "" && cant_dias != "")
                {
                    if (obj_Per.DiasCorridos(desde))
                        hasta = desde.AddDays(int.Parse(cant_dias));
                    else
                    {
                        l_habiles = "1";
                        hasta = desde; cantDias = 0;
                        while (cantDias < int.Parse(cant_dias))
                        {
                            if (obj_Per.DiaLaboral(hasta))
                                cantDias++;
                            hasta = hasta.AddDays(1);
                        }

                        NomadEnvironment.GetTrace().Info("CON CANTIDAD DE DIAS: DESDE -- " + desde.ToString() + " HASTA -- " + hasta.ToString());
                        d_habiles = obj_Per.DiasTrabajablesString(desde, hasta);
                    }

                    fecha_hasta = Nomad.NSystem.Functions.StringUtil.date2str(hasta.AddDays(-1));
                }
                //Calcular la Cantidad de Dias
                else if (fecha_desde != "" && fecha_hasta != "")
                {
                    if (obj_Per.DiasCorridos(desde))
                        cant_dias = ((int)((TimeSpan)(hasta - desde)).TotalDays).ToString();
                    else
                    {
                        cant_dias = obj_Per.DiasTrabajables(desde, hasta.AddDays(-1)).ToString();
                        NomadEnvironment.GetTrace().Info("CON FECHA HASTA: DESDE -- " + desde.ToString() + " HASTA -- " + hasta.AddDays(-1).ToString());
                        str_habiles = obj_Per.DiasTrabajablesString(desde, hasta.AddDays(-1));
                        l_habiles = "1";
                        d_habiles = str_habiles;
                    }
                }

                //Recalculo la Fecha de Fin.
                /*if (!obj_Per.DiasCorridos(desde))
                {
                    if (fecha_hasta != "")
                    {
                        hasta = Nomad.NSystem.Functions.StringUtil.str2date(fecha_hasta).AddDays(1);
                        deadtime = 30;
                        while (!obj_Per.DiaLaboral(hasta) && deadtime > 0) { hasta = hasta.AddDays(1); deadtime--; }
                        if (deadtime == 0)
                            throw new NomadAppException("No se puede Verificar la Fecha de Fin.");

                        fecha_hasta = Nomad.NSystem.Functions.StringUtil.date2str(hasta.AddDays(-1));
                    }
                }*/

                //Obtengo dias bonificados
                GetDiasBonif(oi_personal_emp, desde, hasta, ref dias_bonif);

            }
            catch (Exception E)
            {
                retval.SetAttr("ERR", E.Message);
            }

            retval.SetAttr("fecha_desde", fecha_desde);
            retval.SetAttr("fecha_hasta", fecha_hasta);
            retval.SetAttr("cant_dias", cant_dias);
            retval.SetAttr("dias_bonif", dias_bonif);
            retval.SetAttr("l_habiles", l_habiles);
            retval.SetAttr("d_habiles", d_habiles);

            return retval;
        }

        public void BorrarSolicitud(string idSol)
        {
            SOLICITUD ddoSOL = (SOLICITUD)this.SOLICITUDES.GetById(idSol);

            double DiasAnticipados = 0;

            //Si tiene LICENCIA GENERADA (4) pero no tiene Lic Aprobadas x Periodo (error detectado en Austin) no cuenta los dias anticipo y borra directamente
            if (!(ddoSOL.oi_estado_solic == "4" && ddoSOL.SOLIC_PER.Count == 0))
                DiasAnticipados = ddoSOL.e_dias_solicitud - ddoSOL.e_dias_bonif;

            if (ddoSOL.oi_estado_solic != "1" && ddoSOL.oi_estado_solic != "3")
            {
                //Actualiza la cuenta corriente
                foreach (SOLIC_PER ddoSOLPER in ddoSOL.SOLIC_PER)
                {
                    //Obtengo la cuenta corriente correspondiente al personal y período
                    NucleusRH.Base.Vacaciones.LegajoVacaciones.CTA_CTE_VAC ddoCTACTE = (CTA_CTE_VAC)this.CTA_CTE_VAC.GetByAttribute("oi_periodo", ddoSOLPER.oi_periodo);

                    ddoCTACTE.e_dias_otorg -= ddoSOLPER.e_cant_dias;

                    if (ddoCTACTE.c_estado != "F")
                        ddoCTACTE.e_dias_pend += ddoSOLPER.e_cant_dias;
                    DiasAnticipados -= ddoSOLPER.e_cant_dias;
                }

                //Hay Dias Anticipados Actualzar el utlimo Periodo
                if (DiasAnticipados > 0)
                    throw new Exception("Error en los datos");

                if (ddoSOL.oi_estado_solic != "2")
                {
                    //elimina la licencia asociada en personal, si existe
                    NomadXML param = new NomadXML("<PARAM oi_personal_emp=\"" + this.Id + "\" f_inicio=\"" + ddoSOL.f_desde_solicitud.ToString("yyyyMMdd") + "\"/>");
                    NomadXML Result = NomadEnvironment.QueryNomadXML(PERSONAL_EMP.Resources.QRY_LICENCIA, param.ToString()).FirstChild();

                    if (Result.GetAttr("oi_licencia_per") != "")
                    {
                        NucleusRH.Base.Personal.LegajoEmpresa.LICENCIA_PER LIC = (NucleusRH.Base.Personal.LegajoEmpresa.LICENCIA_PER)this.LICEN_PER.GetById(Result.GetAttr("oi_licencia_per"));
                        this.LICEN_PER.Remove(LIC);
                    }
                }
            }
            this.SOLICITUDES.Remove(ddoSOL);
        }

        public override void Antiguedad(DateTime Hasta, ref int anios, ref int meses, ref int dias)
        {
            NomadEnvironment.GetTrace().Info("****** Antiguedad Vacaciones ******");
            anios = 0; meses = 0; dias = 0;
            bool ultimo = true;

            // Empleado Mensualizado
            if (this.TEMP_PER.Count == 0)
            {
                if (!this.f_ant_rec_vacNull)
                {
                  NomadEnvironment.GetTrace().Info("Antiguedad para vacaciones VAC -- " + this.f_ant_rec_vac.ToString("dd/MM/yyyy HH:mm:ss"));
                  Calcular_Tiempo(this.f_ant_rec_vac, Hasta, ultimo, ref anios, ref meses, ref dias);
                }
                else if (!this.f_antiguedad_recNull)
                {
                  NomadEnvironment.GetTrace().Info("Antiguedad para vacaciones PER -- " + this.f_antiguedad_rec.ToString("dd/MM/yyyy HH:mm:ss"));
                  Calcular_Tiempo(this.f_antiguedad_rec, Hasta, ultimo, ref anios, ref meses, ref dias);
                }
                else
                {
                  int reingresos = 0;
                  int cantidad = this.INGRESOS_PER.Count;
                  foreach (NucleusRH.Base.Personal.LegajoEmpresa.INGRESOS_PER ingreso in this.INGRESOS_PER)
                  {
                    //Determino si es la última trayectoria de ingresos, es decir, el último ingreso
                    reingresos ++;

                    if( cantidad == reingresos)
                      ultimo = true;
                    else ultimo = false;

                    if (!ingreso.f_egresoNull)
                       Calcular_Tiempo(ingreso.f_ingreso, ingreso.f_egreso, ultimo, ref anios, ref meses, ref dias);
                    else
                      Calcular_Tiempo(ingreso.f_ingreso, Hasta, ultimo, ref anios, ref meses, ref dias);
                  }
                }
            }
        }

        public static DateTime Get_F_Hasta(string oi_personal_emp, DateTime f_desde, int cant_dias)
        {

            DateTime f_hasta;
            NucleusRH.Base.Vacaciones.LegajoVacaciones.PERSONAL_EMP personal = NucleusRH.Base.Vacaciones.LegajoVacaciones.PERSONAL_EMP.Get(oi_personal_emp);
            bool l_corridos;
            // Contolo si el Legajo tiene días pactados de vacaciones.
            if (personal.e_cant_dias_vacNull)
            {
                // El Legajo no tiene días de vacaciones pactados
                if (personal.oi_categoria_ultNull)
                    throw new NomadAppException("El Legajo no tiene cargada una Categoría ni Días Pactados de Vacaciones");
                // XML de parametros para el query que obtiene los datos de la matriz de vacaciones.
                string xmlparam = "<DATOS oi_categoria=\"" + personal.oi_categoria_ult + "\" e_antiguedad=\"" + personal.Antiguedad_Meses() + "\"/>";

                NomadEnvironment.GetTrace().Info("XMLPARAM: " + xmlparam);

                // Ejecuto el recurso
                Nomad.NSystem.Document.NmdXmlDocument datos_categ = null;
                datos_categ = new Nomad.NSystem.Document.NmdXmlDocument(NomadEnvironment.QueryString(PERSONAL_EMP.Resources.QRY_DATOS_CATEG, xmlparam));

                NomadEnvironment.GetTrace().Info("datos_categ: " + datos_categ.ToString());
                l_corridos = datos_categ.GetAttribute("l_corridos").Value == "1" ? true : false;
            }
            // El legajo tiene días de vacaciones pactados
            else
            {
                // Devulevo el tipo de dia pactado
                l_corridos = !personal.l_habiles;
                NomadEnvironment.GetTrace().Info("Modalidad de vacaciones (l_corridos) por tener días pactados: " + l_corridos.ToString());
            }

            if (l_corridos)
                f_hasta = f_desde.AddDays((double)cant_dias - 1.0);
            else
            {
                NucleusRH.Base.Organizacion.Calendario_Feriados.CALENDARIO calendario;
                calendario = personal.Getoi_calendario_ult();
                bool[] week = new bool[7] { calendario.l_trab_domingos, calendario.l_trab_lunes, calendario.l_trab_martes, calendario.l_trab_miercoles, calendario.l_trab_jueves, calendario.l_trab_viernes, calendario.l_trab_sabados };
                int cont = cant_dias;
                DateTime fecha = f_desde;

                /*
                int dias_habiles = personal.Dias_Habiles(f_desde, f_hasta);
                NucleusRH.Base.Organizacion.Calendario_Feriados.CALENDARIO calendario = personal.Getoi_calendario_ult();
                NomadEnvironment.GetTrace().Info("Dias Habiles" + dias_habiles);

                cant_dias = dias_habiles - cant_dias_feriado;
                */

                int cant_dias_feriado = 0;
                while (cont > 0)
                {
                    if (week[(int)fecha.DayOfWeek])
                    {
                        // Valido que no sea feriado
                        if ((cant_dias_feriado = personal.Dias_Feriado(calendario, fecha.AddDays(-1), fecha.AddDays(1))) == 0)
                            cont--;
                    }
                    fecha = fecha.AddDays(1);
                }
                f_hasta = fecha.AddDays(-1);
            }
            return f_hasta;
        }
        /*
         public static void GAS(ref Nomad.NSystem.Document.NmdXmlDocument param){
           NomadEnvironment.GetTrace().Info("parametro antes: " + param.ToString());

           string oi_personal_emp = param.GetAttribute("oi_personal_emp").Value;
           PERSONAL_EMP personal = PERSONAL_EMP.Get(oi_personal_emp);
           NomadEnvironment.GetTrace().Info("Guardando Solicitud");

           string oi_solicitud = PERSONAL_EMP.Guardar_Solicitud(param);

           NomadEnvironment.GetTrace().Info("Aprobando Solicitud: " + oi_solicitud + " - oi_personal_emp: "+ personal.Id);
           personal = PERSONAL_EMP.Get(oi_personal_emp);
           Nomad.NSystem.Document.NmdXmlDocument resultado = personal.Aprobar_Solicitud(oi_solicitud, "NORMAL");
           bool l_aprobada = resultado.GetAttribute("Resultado").Value == "1"?true:false;

           if (l_aprobada){
              param.GetAttribute("l_aprobada").Value = "1";
              NomadEnvironment.GetCurrentTransaction().Save(personal);
              NomadEnvironment.GetTrace().Info("Persona Guardada");
           }
           else{
             NomadEnvironment.GetTrace().Info("Solicitud no Aprobada");
             param.GetAttribute("l_aprobada").Value = "0";
             param.GetAttribute("dias_sol_exced").Value = resultado.GetAttribute("MaxDiasSol").Value;
             param.GetAttribute("dias_ant_exced").Value = resultado.GetAttribute("MaxDiasAnt").Value;
           }

           param.GetAttribute("oi_solicitud").Value = oi_solicitud;
           NomadEnvironment.GetTrace().Info("parametro despues: " + param.ToString());
         }
         */

        public static void Notif_Vac_Fecha(Nomad.NSystem.Document.NmdXmlDocument xml_persona, bool l_reimprime, ref XmlTextWriter xtw)
        {
            // FALTA MARCAR EL PERIODO COMO NOTIFICADO DESPUES DE NOTIFICAR
            // Calculo el período correspondiente
            string f_notif = xml_persona.GetAttribute("f_notifica").Value;
            DateTime f_inicio_vac = Nomad.NSystem.Functions.StringUtil.str2date(xml_persona.GetAttribute("f_inicio_vac").Value);
            int e_periodo = f_inicio_vac.Year;
            if (f_inicio_vac.Month <= 4 && f_inicio_vac.Day <= 30)
                e_periodo = f_inicio_vac.Year - 1;

            // Obtengo la persona
            string oi_personal_emp = xml_persona.GetAttribute("oi_personal_emp").Value;
            PERSONAL_EMP personal = PERSONAL_EMP.Get(oi_personal_emp);

            // En esta version obtiene el periodo para cada persona pero hay que optimizarlo
            // para que lo haga una sola vez en el metodo datos_notificacion

            // XML de parametros para el query que obtiene el periodo
            DateTime aux_date = new DateTime(e_periodo, f_inicio_vac.Month, f_inicio_vac.Day);
            string xmlparam = "<DATOS fecha=\"" + aux_date.ToString("yyyyMMdd") + "\"/>";
            // Ejecuto el recurso
            Nomad.NSystem.Document.NmdXmlDocument xml_periodo = null;
            xml_periodo = new Nomad.NSystem.Document.NmdXmlDocument(NomadEnvironment.QueryString(NucleusRH.Base.Vacaciones.LegajoVacaciones.PERSONAL_EMP.Resources.QRY_PERIODO, xmlparam));

            NomadEnvironment.GetTrace().Info("Periodo:: " + xml_periodo.ToString());

            //Obtengo la cuenta corriente del periodo correspondiente
            CTA_CTE_VAC cta_cte = (CTA_CTE_VAC)personal.CTA_CTE_VAC.GetByAttribute("oi_periodo", xml_periodo.GetAttribute("oi_periodo").Value);

            if (cta_cte == null)
            {
                NomadEnvironment.GetTrace().Info("Persona:: " + oi_personal_emp + " - No tiene generado el período");
                return;
            }

            if (cta_cte.l_notificada)
            {
                if (!l_reimprime)
                {
                    NomadEnvironment.GetTrace().Info("Persona:: " + oi_personal_emp + " - Ya se le notificó el período");
                    return;
                }
            }

            NomadEnvironment.GetTrace().Info("Persona:: " + oi_personal_emp + " - Emitiendo notificación");

            // Calculo f_hasta con la fecha de inicio y la cantidad de dias
            string f_desde = f_inicio_vac.ToString("yyyyMMdd");
            int cant_dias = cta_cte.e_dias_gen;
            DateTime f_hastaa = PERSONAL_EMP.Get_F_Hasta(oi_personal_emp, f_inicio_vac, cant_dias);
            string f_hasta = f_hastaa.ToString("yyyyMMdd");

            //Datos adicionales de la notificacion
            string f_ingreso = personal.f_ingreso.ToString("yyyyMMdd");
            string f_notifica = f_notif;
            DateTime auxantig = new DateTime(e_periodo, 12, 31);
            string antig = personal.Antiguedad_Anios(auxantig).ToString() + " años " + (personal.Antiguedad_Meses(auxantig) - personal.Antiguedad_Anios(auxantig)*12).ToString() + " meses.";

            xtw.WriteStartElement("PERSONA");
            xtw.WriteAttributeString("oi_personal_emp", oi_personal_emp);
            if (f_notifica != null)
            xtw.WriteAttributeString("f_notifica", f_notifica);
            xtw.WriteAttributeString("f_desde", f_desde);
            xtw.WriteAttributeString("f_hasta", f_hasta);
            xtw.WriteAttributeString("e_cant_dias", cant_dias.ToString());
            xtw.WriteAttributeString("l_notificar", "");

            xtw.WriteAttributeString("f_ingreso", f_ingreso);
            xtw.WriteAttributeString("antig", antig);

            xtw.WriteEndElement();
        }

        public static void Notif_Vac_Solic(Nomad.NSystem.Document.NmdXmlDocument xml_persona, bool l_reimprime, ref XmlTextWriter xtw, int parametro_vacaciones)
        {
          // Obtengo la Persona
          string oi_personal_emp = xml_persona.GetAttribute("oi_personal_emp").Value;

          NucleusRH.Base.Vacaciones.LegajoVacaciones.PERSONAL_EMP personal = NucleusRH.Base.Vacaciones.LegajoVacaciones.PERSONAL_EMP.Get(oi_personal_emp);

          NomadEnvironment.GetTrace().Info("Persona: " + personal.Id + "Buscando Solicitudes");

          // Recupero la solicitud
          NucleusRH.Base.Vacaciones.LegajoVacaciones.SOLICITUD solicitud = (SOLICITUD)personal.SOLICITUDES.GetById(xml_persona.GetAttribute("oi_solicitud").Value);
          NomadEnvironment.GetTrace().Info("solicitud: " + solicitud.ToString());

          // Recupero la notificacion
          NucleusRH.Base.Vacaciones.LegajoVacaciones.NOTIF notificacion = (NOTIF)solicitud.NOTIF.GetById(xml_persona.GetAttribute("oi_notif").Value);
          NomadEnvironment.GetTrace().Info("notificacion: " + notificacion.ToString());

          TimeSpan a = new TimeSpan();
          a.Add(solicitud.f_desde_solicitud - DateTime.Today);
          if (a.TotalDays <= parametro_vacaciones && (solicitud.oi_estado_solic == "2" || solicitud.oi_estado_solic == "4"))
          {
            if (notificacion.l_notificada)
              if (!l_reimprime) return;
            NomadLog.Debug(solicitud.SerializeAll());
            // Obtengo el último período que corresponde a la solicitud
            NucleusRH.Base.Vacaciones.LegajoVacaciones.SOLIC_PER OBJ_solic_per = (SOLIC_PER)solicitud.SOLIC_PER[0];
            NucleusRH.Base.Vacaciones.Periodos.PERIODO OBJ_periodo = OBJ_solic_per.Getoi_periodo();
            NucleusRH.Base.Vacaciones.Periodos.PERIODO OBJ_periodo_NOTIF = notificacion.Getoi_periodo();

            // Obtengo la cuenta corriente correspondiente al personal y período
            NucleusRH.Base.Vacaciones.LegajoVacaciones.CTA_CTE_VAC OBJ_cta_cte_vac = (NucleusRH.Base.Vacaciones.LegajoVacaciones.CTA_CTE_VAC)personal.CTA_CTE_VAC.GetByAttribute("oi_periodo", OBJ_periodo.Id);

            if (OBJ_cta_cte_vac == null)
              throw new Exception("No se puede notificar las vacaciones porque no se encuentra la cuenta corriente del período asociado a la solicitud.");

            // Calculo la fecha de reincorporación
            NomadEnvironment.GetTrace().Info("f_hasta_solicitud: " + solicitud.f_hasta_solicitud );
            DateTime f_reintegro = Get_F_Hasta(personal.Id, solicitud.f_hasta_solicitud.AddDays(1), 1);
            NomadEnvironment.GetTrace().Info("f_reintegro: " + f_reintegro.ToString("dd/MM/yyyy HH:mm:ss"));

            int dias_tomados_ant = OBJ_cta_cte_vac.e_dias_otorg - solicitud.e_dias_solicitud;
            if(dias_tomados_ant<0)
              dias_tomados_ant = 0;

            string f_ingreso = personal.f_ingreso.ToString("dd/MM/yyyy");
            DateTime auxantig = new DateTime(OBJ_periodo.e_ano, 12, 31);
            string antig = personal.Antiguedad_Anios(auxantig).ToString() + " años " + (personal.Antiguedad_Meses(auxantig) - personal.Antiguedad_Anios(auxantig)*12).ToString() + " meses.";

            xtw.WriteStartElement("PERSONA");

            xtw.WriteAttributeString("f_ingreso", f_ingreso);
            xtw.WriteAttributeString("antig", antig);
            if (xml_persona.GetAttribute("f_notifica") != null)
            {
              string f_notifica = xml_persona.GetAttribute("f_notifica").Value;
              xtw.WriteAttributeString("f_notifica", f_notifica);
            }
            xtw.WriteAttributeString("f_reintegro", f_reintegro.ToString("yyyyMMdd"));
            xtw.WriteAttributeString("f_desde", solicitud.f_desde_solicitud.ToString("yyyyMMdd"));
            xtw.WriteAttributeString("f_hasta", solicitud.f_hasta_solicitud.ToString("yyyyMMdd"));
            xtw.WriteAttributeString("e_cant_dias", solicitud.e_dias_solicitud.ToString());
            xtw.WriteAttributeString("d_motivo_solic", solicitud.d_motivo_solic.ToString());
            xtw.WriteAttributeString("l_notificar", "");

            xtw.WriteAttributeString("dias_generados", notificacion.e_dias_gen.ToString());
            xtw.WriteAttributeString("dias_otorgados", notificacion.e_dias_sol.ToString());
            xtw.WriteAttributeString("dias_pendientes", notificacion.e_dias_pend.ToString());
            xtw.WriteAttributeString("periodo", OBJ_periodo_NOTIF.c_periodo);

            xtw.WriteAttributeString("dias_tomados_ant", dias_tomados_ant.ToString());
            xtw.WriteAttributeString("oi_personal_emp", oi_personal_emp);

            NomadEnvironment.GetTrace().Info("SOLIC_PER: " + solicitud.SOLIC_PER.Count);
            if(solicitud.SOLIC_PER.Count>0)
            {
              NomadEnvironment.GetTrace().Info("por aca.... ");
              //int cper = 0;
              //foreach(NucleusRH.Base.Vacaciones.LegajoVacaciones.SOLIC_PER OBJ_soladic in solicitud.SOLIC_PER)
              //{
                //cper++;
                //if(cper<=1) continue;
                NucleusRH.Base.Vacaciones.Periodos.PERIODO OBJ_peradic = NucleusRH.Base.Vacaciones.Periodos.PERIODO.Get(notificacion.oi_periodo.ToString());
                NomadEnvironment.GetTrace().Info("OBJ_peradic: " + OBJ_peradic.ToString());
                xtw.WriteStartElement("PERIODO");
                xtw.WriteAttributeString("periodo", OBJ_peradic.c_periodo);
                xtw.WriteAttributeString("e_ano", OBJ_peradic.e_ano.ToString());
                xtw.WriteAttributeString("cant_dias", notificacion.e_dias_sol.ToString());
                xtw.WriteEndElement();
              //}
            }

            xtw.WriteEndElement();

          }
          // Marco las solicitudes como notificadas
          solicitud.l_notificada = true;
          solicitud.f_notificacion = DateTime.Now;
          notificacion.l_notificada = true;
          notificacion.f_notificacion = DateTime.Now;
          NomadEnvironment.GetCurrentTransaction().SaveRefresh(personal);
        }

        //////////////////////////////////////////////////////////////////////////////////////////////
        // NUEVAS FUNCIONES
        public void Interrumpir_vac(string oi_solicitud, System.DateTime fecha_interrupcion, int e_cant_dias)
        {
            //Obtengo la Solucitud
            NucleusRH.Base.Vacaciones.LegajoVacaciones.SOLICITUD obj_Solic = (SOLICITUD)this.SOLICITUDES.GetById(oi_solicitud);

            //Actualizo la Licencia
            NucleusRH.Base.Personal.LegajoEmpresa.LICENCIA_PER PerLiq = (NucleusRH.Base.Personal.LegajoEmpresa.LICENCIA_PER)this.LICEN_PER.GetByAttribute("f_inicio", obj_Solic.f_desde_solicitud);
            if (PerLiq == null)
                throw new NomadAppException("No se puede interrumpir las vacaciones porque no se encontro la licencia Asociada a la Solicitud de Vacaciones.");

            CancelarSolicitud(oi_solicitud);
            obj_Solic.e_dias_solicitud = e_cant_dias;
            AprobarSolicitud(this, oi_solicitud, true);

            //Actualizo la licencia del Legajo
            PerLiq.f_interrupcion = fecha_interrupcion;
            PerLiq.e_cant_dias = e_cant_dias;

            //Actualizo los periodos
            PerLiq.LIC_PERIODO.Clear();
            int dias = e_cant_dias;
            foreach(NucleusRH.Base.Vacaciones.LegajoVacaciones.SOLIC_PER ddoSP in obj_Solic.SOLIC_PER)
            {
              if(dias<=0) continue;
              NucleusRH.Base.Personal.LegajoEmpresa.LIC_PERIODO ddoLP = new NucleusRH.Base.Personal.LegajoEmpresa.LIC_PERIODO();
              ddoLP.e_anio = ddoSP.e_ano;

              if(ddoSP.e_cant_dias <= dias)
                ddoLP.e_cant = ddoSP.e_cant_dias;
              else
                ddoLP.e_cant = dias;

              PerLiq.LIC_PERIODO.Add(ddoLP);
              dias-=ddoLP.e_cant;
            }

            //Actualizo la Solicitud
            obj_Solic.f_interrupcion = fecha_interrupcion;
            obj_Solic.oi_estado_solic = "5";
            obj_Solic.f_estado_solic = DateTime.Now;

            //Guardo el PERSONAL
            NomadEnvironment.GetCurrentTransaction().SaveRefresh(this);
        }

        public void Cancelar_Solicitud(string oi_solicitud)
        {
            //Llamo al CancelarSolicitud
            this.CancelarSolicitud(oi_solicitud);
            //Guardo el PERSONAL
            NomadEnvironment.GetCurrentTransaction().Save(this);
        }

        public void CancelarSolicitud(string oi_solicitud)
        {
            NucleusRH.Base.Vacaciones.LegajoVacaciones.SOLICITUD obj_Solic = (SOLICITUD)this.SOLICITUDES.GetById(oi_solicitud);
            NucleusRH.Base.Vacaciones.LegajoVacaciones.CTA_CTE_VAC CtaCte;

            int DiasAnticipados;
            DiasAnticipados = obj_Solic.e_dias_solicitud -  obj_Solic.e_dias_bonif;

            //Recorro las Solicitudes por periodo y Actualizo los PERIODOS de cuenta Corriente
            foreach (NucleusRH.Base.Vacaciones.LegajoVacaciones.SOLIC_PER obj_SoliPer in obj_Solic.SOLIC_PER)
            {
                //Obtengo el Periodo y lo ACTUALIZO
                CtaCte = (CTA_CTE_VAC)this.CTA_CTE_VAC.GetByAttribute("oi_periodo", obj_SoliPer.oi_periodo);

        if (CtaCte == null)
          throw new Exception("No se puede cancelar la solicitud porque no se encuentra la cuenta corriente del período asociado a la solicitud.");

                CtaCte.e_dias_otorg -= obj_SoliPer.e_cant_dias;
                if (CtaCte.c_estado != "F")
                    CtaCte.e_dias_pend += obj_SoliPer.e_cant_dias;
                DiasAnticipados -= obj_SoliPer.e_cant_dias;
            }

            //Hay Dias Anticipados Actualzar el utlimo Periodo
            if (DiasAnticipados > 0)
                throw new Exception("Error en los datos");

            //Limpio los periodos por solicitud
            if (obj_Solic.SOLIC_PER.Count > 0)
                obj_Solic.SOLIC_PER.Clear();
            //Limpio las notificaciones por solicitud
            if (obj_Solic.NOTIF.Count > 0)
                obj_Solic.NOTIF.Clear();
            obj_Solic.oi_estado_solic = "1";
        }

        public void Aprobar_Solicitud(string oi_solicitud, bool force)
        {
            NucleusRH.Base.Vacaciones.LegajoVacaciones.PERSONAL_EMP.AprobarSolicitud(this, oi_solicitud, force);
            try
            {
                NomadEnvironment.GetCurrentTransaction().Save(this);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public void Rechazar_Solicitud(string oi_solicitud)
        {
            NucleusRH.Base.Vacaciones.LegajoVacaciones.SOLICITUD obj_solicitud = (SOLICITUD)this.SOLICITUDES.GetById(oi_solicitud);

            //Cambia el Estado de la Solicitud a RECHAZADA.
            obj_solicitud.oi_estado_solic = "3";
            obj_solicitud.f_estado_solic = System.DateTime.Now;
            NomadEnvironment.GetCurrentTransaction().Save(this);

            //Envio de Mail al Empleado informando el rechazo de la solcitud
            Nomad.Base.Mail.OutputMails.MAIL mail_empleado = new Nomad.Base.Mail.OutputMails.MAIL();
            Nomad.Base.Mail.OutputMails.DESTINATARIO empleado = new Nomad.Base.Mail.OutputMails.DESTINATARIO();

            mail_empleado.DESDE_APLICACION=NomadProxy.GetProxy().AppName;
            mail_empleado.FECHA_CREACION=System.DateTime.Now;
            mail_empleado.ASUNTO = "Solicitud de Vacaciones Rechazada";
            mail_empleado.CONTENIDO = "La Solicitud con feche desde: " + obj_solicitud.f_desde_solicitud.ToString("dd/MM/yyyy") + " y fecha hasta: " + obj_solicitud.f_hasta_solicitud.ToString("dd/MM/yyyy") + " fue Rechazada! ";
            mail_empleado.TAGS = "Vacaciones";

            // XML de parametros para el query que obtiene los datos de la matriz de vacaciones.
            NomadXML param = new NomadXML("DATOS");
            param.SetAttr("oi_personal_emp", obj_solicitud.oi_personal_emp);
            // param = <DATOS oi_personal_emp="13899"/>
            // Ejecuto el recurso
            NomadXML Result = NomadEnvironment.QueryNomadXML(PERSONAL_EMP.Resources.QRY_USUARIO_SISTEMA, param.ToString()).FirstChild();

            empleado.ENTIDAD = Result.GetAttr("oi_usuario_sistema").ToString();
            mail_empleado.DESTINATARIOS.Add(empleado);

            //MANDO EL MAIL
            NomadEnvironment.GetCurrentTransaction().Save(mail_empleado);
        }

        public void Liquidar_Solicitud(string oi_solicitud)
        {
            NucleusRH.Base.Vacaciones.LegajoVacaciones.SOLICITUD obj_solicitud = (SOLICITUD)this.SOLICITUDES.GetById(oi_solicitud);

            //Obtengo el Parametro de CODIGO de VACACIONES
            string codLic = NomadEnvironment.QueryValue("ORG26_PARAMETROS", "d_valor", "c_parametro", "CODVAC", "ORG26_PARAMETROS.c_modulo=\\'VAC\\'", false);
            if (codLic == "")
                throw new NomadAppException("No esta definido el Codigo de Licencia de Vacaciones.");

            //Obtengo el Parametro de CODIGO de VACACIONES
            string oiLic = NomadEnvironment.QueryValue("PER16_LICENCIAS", "oi_licencia", "c_licencia", codLic, "", false);
            if (oiLic == "")
                throw new NomadAppException("No esta definido el Codigo de Licencia de Vacaciones no Existe.");

			//Nueva Licencia
			NucleusRH.Base.Personal.LegajoEmpresa.LICENCIA_PER ddoLicPer = NucleusRH.Base.Personal.LegajoEmpresa.LICENCIA_PER.New();
			ddoLicPer.oi_licencia = oiLic;
            ddoLicPer.f_inicio = obj_solicitud.f_desde_solicitud;
            ddoLicPer.f_fin = obj_solicitud.f_hasta_solicitud;
            ddoLicPer.e_cant_dias = obj_solicitud.e_dias_solicitud;
            ddoLicPer.e_anio_corresp = ddoLicPer.f_inicio.Year;
            ddoLicPer.f_carga = obj_solicitud.f_solicitud;
            ddoLicPer.l_interfaz = obj_solicitud.l_interfaz;
            ddoLicPer.l_habiles = obj_solicitud.l_habiles;
            ddoLicPer.d_habiles = obj_solicitud.d_habiles;
            ddoLicPer.o_licencia_per = obj_solicitud.o_solicitud;

            foreach (NucleusRH.Base.Vacaciones.LegajoVacaciones.SOLIC_PER objSP in obj_solicitud.SOLIC_PER)
            {
                NucleusRH.Base.Personal.LegajoEmpresa.LIC_PERIODO ddoLICP = new NucleusRH.Base.Personal.LegajoEmpresa.LIC_PERIODO();
                ddoLICP.e_anio = objSP.e_ano;
                ddoLICP.e_cant = objSP.e_cant_dias;
                ddoLicPer.LIC_PERIODO.Add(ddoLICP);
            }
			
			//Validar Licencia
			NucleusRH.Base.Personal.LegajoEmpresa.LICENCIA_PER.ValidarAltaLicencia
				(this.Id, oiLic, ddoLicPer.f_inicio, ddoLicPer.f_fin, ddoLicPer.e_cant_dias, ddoLicPer.e_anio_corresp);
			
			//Agregar Licencia
			this.LICEN_PER.Add(ddoLicPer);
			
			//Cambiar el Estado de la Solicitud a LIQUIDADA
            obj_solicitud.oi_estado_solic = "4";
            obj_solicitud.f_estado_solic = System.DateTime.Now;

            //Guardar Legajo			
            NomadEnvironment.GetCurrentTransaction().Save(this);
        }

        public string AgregarSolicitud(System.DateTime f_desde, System.DateTime f_hasta, int e_dias, int e_dias_bonif, string motivo, string observacion, string l_automatica, string l_habiles, string d_habiles)
        {
            //Verifico Solapamientos
            foreach (NucleusRH.Base.Vacaciones.LegajoVacaciones.SOLICITUD chkSOL in this.SOLICITUDES)
            {
                if (chkSOL.oi_estado_solic != "1" && chkSOL.oi_estado_solic != "3")
                {
                    System.DateTime FD, FH;
                    FD = chkSOL.f_desde_solicitud;
                    FH = (chkSOL.f_interrupcionNull ? chkSOL.f_hasta_solicitud : chkSOL.f_interrupcion);
                    if ((f_desde >= FD && f_desde <= FH) || (f_hasta >= FD && f_hasta<= FH) || (f_desde <= FH && f_hasta >= FD))
                        throw new NomadAppException("La solicitud se solapa con una solicitud previa.");
                }
            }
			
			//Obtener OI para [V] Vacaciones
			string oiLic = NomadEnvironment.QueryValue("PER16_LICENCIAS", "oi_licencia", "c_licencia", "V", "", false);
			
			//Validar futura Licencia
			NucleusRH.Base.Personal.LegajoEmpresa.LICENCIA_PER.ValidarAltaLicencia
				(this.Id, oiLic, f_desde, f_hasta, e_dias, f_desde.Year);

            //Agrego la Solicitud
            NucleusRH.Base.Vacaciones.LegajoVacaciones.SOLICITUD objSOL = new NucleusRH.Base.Vacaciones.LegajoVacaciones.SOLICITUD();
            objSOL.oi_personal_emp = this.id;
            objSOL.e_dias_solicitud = e_dias;
            objSOL.e_dias_bonif = e_dias_bonif;
            objSOL.f_desde_solicitud = f_desde;
            objSOL.f_hasta_solicitud = f_hasta;
            objSOL.f_estado_solic = System.DateTime.Now;
            objSOL.f_solicitud = System.DateTime.Now;
            objSOL.oi_estado_solic = "1";
            objSOL.l_automatica = l_automatica == "1";
            objSOL.o_solicitud = observacion;
            objSOL.d_motivo_solic = motivo;

            if (l_habiles == "1")
                objSOL.l_habiles = true;
            else
                objSOL.l_habiles = false;
            objSOL.d_habiles = d_habiles;

            this.SOLICITUDES.Add(objSOL);

            //devuelvo el oi_de la nueva solicitud
            return objSOL.Id;
        }

    public static void AprobarSolicitud(PERSONAL_EMP myOBJ, string oi_solicitud, bool force) {

        NomadLog.Debug("ENTRA A APROBARSILICITUD - VERIF SI HAY MAIL");

        NomadXML result = NomadEnvironment.QueryNomadXML(NucleusRH.Base.Personal.LegajoEmpresa.PERSONAL_EMP.Resources.QRY_PARAMETRO_NOTIF,"").FirstChild();

        if(result.FirstChild().GetAttrBool("d_valor"))
        {
          NomadLog.Debug("MANDA MAIL");
          PERSONAL_EMP.AprobarSolicitud(myOBJ, oi_solicitud, force, true);
        }
        else
        {
          NomadLog.Debug("NO MANDA MAIL");
          PERSONAL_EMP.AprobarSolicitud(myOBJ, oi_solicitud, force, false);
        }
    }

        public static void AprobarSolicitud(PERSONAL_EMP myOBJ, string oi_solicitud, bool force, bool pSendMail)
        {
            NucleusRH.Base.Vacaciones.LegajoVacaciones.SOLICITUD obj_solicitud = (SOLICITUD)myOBJ.SOLICITUDES.GetById(oi_solicitud);

            //Verifico la Cuenta CORRIENTE
            if (myOBJ.CTA_CTE_VAC.Count == 0)
                throw new NomadAppException("El Legajo no tiene cuenta corriente de Vacaciones.");

            //Cantidad de Dias Pendientes
            int e_CountPendientes = 0;
            foreach (NucleusRH.Base.Vacaciones.LegajoVacaciones.CTA_CTE_VAC m_CtaCte in myOBJ.CTA_CTE_VAC)
                e_CountPendientes += m_CtaCte.e_dias_pend;

            // VALIDACION DE MAXIMO DIAS ANTICIPADOS y PENDIENTES
            if (!force) // si el tipo de ejecucion no es normal no se validan Máximos
            {
                //Días Solicitados
                if (!myOBJ.e_max_dias_sol_vacNull && obj_solicitud.e_dias_solicitud > myOBJ.e_max_dias_sol_vac)
                    throw new NomadAppException("La Cantidad de Dias Solicitados es Mayor al Permitido por Solicitud");

                //Días anticipados
                if (obj_solicitud.e_dias_solicitud > e_CountPendientes + myOBJ.e_max_dias_vac_ant)
                    throw new NomadAppException("La Cantidad de Dias Solicitados es Mayor a las Anticipadas Permitidas");
            }

            NucleusRH.Base.Vacaciones.LegajoVacaciones.SOLIC_PER objSolPer;
            NucleusRH.Base.Vacaciones.LegajoVacaciones.CTA_CTE_VAC last_Cta_Cte = (NucleusRH.Base.Vacaciones.LegajoVacaciones.CTA_CTE_VAC)myOBJ.CTA_CTE_VAC.GetByAttribute("c_estado", "F");

            if (last_Cta_Cte == null)
                throw new Exception("No se puede aprobar las solicitud porque no se encontró la cuenta conrriente asociada al período FUTURO.");

            int CantDias, DiasOtorgados;
            CantDias = obj_solicitud.e_dias_solicitud - obj_solicitud.e_dias_bonif;

            while (CantDias > 0)
            {
                NucleusRH.Base.Vacaciones.LegajoVacaciones.CTA_CTE_VAC Cta_Cte = null;
                foreach (NucleusRH.Base.Vacaciones.LegajoVacaciones.CTA_CTE_VAC temp in myOBJ.CTA_CTE_VAC)
                    if (temp.e_dias_pend > 0 && (Cta_Cte == null || Cta_Cte.e_ano > temp.e_ano))
                        Cta_Cte = temp;

                if (Cta_Cte != null)
                    DiasOtorgados = Cta_Cte.e_dias_pend < CantDias ? Cta_Cte.e_dias_pend : CantDias;
                else
                {
                    Cta_Cte = last_Cta_Cte;
                    DiasOtorgados = CantDias;
                }

                //Actualizo la Cuenta Corriente
                CantDias -= DiasOtorgados;
                Cta_Cte.e_dias_pend -= (Cta_Cte == last_Cta_Cte?0:DiasOtorgados);
                Cta_Cte.e_dias_otorg += DiasOtorgados;

                //Genero la SOLICITUD por PERIODO
                objSolPer = new SOLIC_PER();
                objSolPer.e_cant_dias = DiasOtorgados;
                objSolPer.f_desde = obj_solicitud.f_desde_solicitud;
                objSolPer.f_hasta = obj_solicitud.f_hasta_solicitud;
                objSolPer.oi_periodo = Cta_Cte.oi_periodo;
                objSolPer.e_ano = Cta_Cte.e_ano;
                NomadLog.Info("SOLIC_PER::: " + objSolPer.ToString());
                obj_solicitud.SOLIC_PER.Add(objSolPer);

                //Creo un registro que guarda una notificación por la aprobación de la solicitud
                NucleusRH.Base.Vacaciones.LegajoVacaciones.NOTIF notif_vac = new NucleusRH.Base.Vacaciones.LegajoVacaciones.NOTIF();

                DateTime f_reintegro = Get_F_Hasta(myOBJ.Id, obj_solicitud.f_hasta_solicitud.AddDays(1), 1);

                notif_vac.e_dias_gen = Cta_Cte.e_dias_gen;
                notif_vac.e_dias_pend = Cta_Cte.e_dias_pend;
                notif_vac.e_dias_sol = DiasOtorgados;
                notif_vac.f_fin = obj_solicitud.f_hasta_solicitud;
                notif_vac.f_inicio = obj_solicitud.f_desde_solicitud;
                notif_vac.f_notificacion = obj_solicitud.f_notificacion;
                notif_vac.l_notificada = obj_solicitud.l_notificada;
                notif_vac.f_reintegro = f_reintegro;
                notif_vac.oi_periodo = Cta_Cte.oi_periodo;
                //notif_vac.oi_solicitud = obj_solicitud.Id;

                NomadLog.Info("notif_vac::: " + notif_vac.ToString());
                obj_solicitud.NOTIF.Add(notif_vac);

            }

            obj_solicitud.oi_estado_solic = "2";
            obj_solicitud.f_estado_solic = System.DateTime.Now;

      if (pSendMail) {
              //Envio de Mail al Empleado inforando la aprobacion de la solcitud
              Nomad.Base.Mail.OutputMails.MAIL mail_empleado = new Nomad.Base.Mail.OutputMails.MAIL();
              Nomad.Base.Mail.OutputMails.DESTINATARIO empleado = new Nomad.Base.Mail.OutputMails.DESTINATARIO();

              mail_empleado.DESDE_APLICACION=NomadProxy.GetProxy().AppName;
              mail_empleado.FECHA_CREACION=System.DateTime.Now;
              mail_empleado.ASUNTO = "Solicitud de Vacaciones Aprobada";
              mail_empleado.CONTENIDO = "La Solicitud con feche desde: " + obj_solicitud.f_desde_solicitud.ToString("dd/MM/yyyy") + " y fecha hasta: " + obj_solicitud.f_hasta_solicitud.ToString("dd/MM/yyyy") + " fue Aprobada! ";
              mail_empleado.TAGS = "Vacaciones";

              // XML de parametros para el query que obtiene los datos de la matriz de vacaciones.
              NomadXML param = new NomadXML("DATOS");
              param.SetAttr("oi_personal_emp", obj_solicitud.oi_personal_emp);

              // Ejecuto el recurso
              NomadXML Result = NomadEnvironment.QueryNomadXML(PERSONAL_EMP.Resources.QRY_USUARIO_SISTEMA, param.ToString()).FirstChild();

              if(Result.GetAttr("oi_usuario_sistema").ToString() != "")
              {
                empleado.ENTIDAD = Result.GetAttr("oi_usuario_sistema").ToString();
                mail_empleado.DESTINATARIOS.Add(empleado);
              }

              //MANDO EL MAIL
              NomadEnvironment.GetCurrentTransaction().Save(mail_empleado);
            }
        }

        public bool DiaLaboral(DateTime fecha)
        {
            bool retval = false;

            //Obtengo el Calendario
            NucleusRH.Base.Organizacion.Calendario_Feriados.CALENDARIO obj_Cal = null;
            foreach (NucleusRH.Base.Personal.LegajoEmpresa.CALENDARIO_PER MyCAL in this.CALENDARIO_PER)
                if ((fecha >= MyCAL.f_desde) && (MyCAL.f_hastaNull || fecha <= MyCAL.f_hasta))
                    obj_Cal = MyCAL.Getoi_calendario();
            if (obj_Cal == null)
                throw new NomadAppException("La persona no tiene un Calendario Definido para la Fecha Especificada.");

            //Calculo si ese Dia TRABAJA.
            if (obj_Cal.DIAS_FERIADOS.GetByAttribute("f_feriado", fecha) != null)
            {
                retval = obj_Cal.l_trab_feriados;
            }
            else
            {
                switch (fecha.DayOfWeek)
                {
                    case System.DayOfWeek.Monday: retval = obj_Cal.l_trab_lunes; break;
                    case System.DayOfWeek.Tuesday: retval = obj_Cal.l_trab_martes; break;
                    case System.DayOfWeek.Wednesday: retval = obj_Cal.l_trab_miercoles; break;
                    case System.DayOfWeek.Thursday: retval = obj_Cal.l_trab_jueves; break;
                    case System.DayOfWeek.Friday: retval = obj_Cal.l_trab_viernes; break;
                    case System.DayOfWeek.Saturday: retval = obj_Cal.l_trab_sabados; break;
                    case System.DayOfWeek.Sunday: retval = obj_Cal.l_trab_domingos; break;
                }
            }

            DiaLaboralExterno(this.Id, fecha, ref retval);
            return retval;
        }

        public bool DiasCorridos(DateTime fecha)
        {
            bool retval = false;

            // Controlo si el Legajo tiene días pactados de vacaciones.
            if (this.e_cant_dias_vacNull)
            {
                if (this.oi_categoria_ultNull)
                    throw new NomadAppException("El Legajo no tiene cargada una Categoría.");

                // XML de parametros para el query que obtiene los datos de la matriz de vacaciones.
                NomadXML param = new NomadXML("DATOS");
                param.SetAttr("oi_categoria", this.oi_categoria_ult);
                param.SetAttr("e_antiguedad", this.Antiguedad_Meses());

                // Ejecuto el recurso
                NomadXML Result = NomadEnvironment.QueryNomadXML(PERSONAL_EMP.Resources.QRY_DATOS_CATEG, param.ToString(), false).FirstChild();

                if (Result.GetAttr("l_corridos")=="")
                    throw new NomadAppException("El convenio no tiene cargada la matriz de vacaciones o la misma no posee celdas para la antigüedad del Legajo");
                retval = Result.GetAttrBool("l_corridos");
            }
            else
            {
                // Devuelve el tipo de dia pactado
                retval = !this.l_habiles;
            }

            return retval;
        }

        public int DiasTrabajables(DateTime fecha_desde, DateTime fecha_hasta)
        {
            int retval = 0;

            for (System.DateTime fecha = fecha_desde; fecha <= fecha_hasta; fecha = fecha.AddDays(1))
                if (this.DiaLaboral(fecha))
                    retval++;

            return retval;
        }

        /// <summary>
        /// Devuelve un string con 0 y 1 marcando con 1 los días hábiles
        /// </summary>
        /// <param name="fecha_desde"></param>
        /// <param name="fecha_hasta"></param>
        /// <returns></returns>
        public string DiasTrabajablesString(DateTime fecha_desde, DateTime fecha_hasta)
        {
            string retval = "";

            for (System.DateTime fecha = fecha_desde; fecha <= fecha_hasta; fecha = fecha.AddDays(1))
                if (this.DiaLaboral(fecha))
                    retval = retval + "1";
                else
                    retval = retval + "0";

            return retval;
        }

        public static void Generar_Vacaciones(Nomad.NSystem.Proxy.NomadXML param, Boolean l_sobreescribe, String oi_periodo)
        {

            NomadLog.Debug("PERSONAL_EMP.Generar_Vacaciones param: " + param.ToString());
            NomadLog.Debug("PERSONAL_EMP.Generar_Vacaciones l_sobreescribe: " + l_sobreescribe);
            NomadLog.Debug("PERSONAL_EMP.Generar_Vacaciones oi_periodo: " + oi_periodo);
            
            NomadBatch objBatch;
            objBatch = NomadBatch.GetBatch("Iniciando...", "Generación Vacaciones");

            Nomad.NSystem.Proxy.NomadXML nxmRows = param.FirstChild();
            int pos = 0;

            //Cantidad de Legajos a generar
            int max = nxmRows.ChildLength;
            objBatch.Log("Cantidad Maxima: " + max.ToString());

            // Obtener el período a generar
            NucleusRH.Base.Vacaciones.Periodos.PERIODO objPeriodo = NucleusRH.Base.Vacaciones.Periodos.PERIODO.Get(oi_periodo);

            //Setear el progress al inicio
            objBatch.SetPro(10);
            string error="";

            // Para cada ROW del XML
            for (NomadXML nxmRow = nxmRows.FirstChild(); nxmRow != null; nxmRow = nxmRow.Next())
            {
                // Obtengo el DDO y calculo las vacaciones
                try
                {
                    pos++;
                    error = "PERSONAL_EMP " + nxmRow.GetAttrString("id");
                    NucleusRH.Base.Vacaciones.LegajoVacaciones.PERSONAL_EMP objLegajo = NucleusRH.Base.Vacaciones.LegajoVacaciones.PERSONAL_EMP.Get(nxmRow.GetAttrString("id"));
                    error = "Legajo: " + objLegajo.descr;
                    objLegajo.Calcular_Vacaciones(l_sobreescribe, objPeriodo);
                    objBatch.SetPro(10, 90, max, pos);
                }
                catch (Exception e)
                {
                    error = error + " - " + e.Message;
                    NomadBatch.Trace(error);
                    objBatch.Err(error);
                }
            }

            //Setear el progress al finalizar
            objBatch.SetPro(100);
        }

        public void Calcular_Vacaciones(Boolean l_sobreescribe, NucleusRH.Base.Vacaciones.Periodos.PERIODO objPeriodo)
        {
            string error = "No se puede relizar el cálculo de vacaciones, ";
            Hashtable solicitudesCanceladas = new Hashtable();
            List<SOLICITUD> lista_soli_can = new List<SOLICITUD>();

            NucleusRH.Base.Vacaciones.LegajoVacaciones.CTA_CTE_VAC objCta_Cte_Futura;
            NucleusRH.Base.Vacaciones.LegajoVacaciones.CTA_CTE_VAC objCta_Cte;
            //NucleusRH.Base.Vacaciones.LegajoVacaciones.CTA_CTE_VAC objCta_Cte_Pasada;

            foreach (NucleusRH.Base.Vacaciones.LegajoVacaciones.SOLICITUD objSolicitud in this.SOLICITUDES)
                foreach (NucleusRH.Base.Vacaciones.LegajoVacaciones.SOLIC_PER objSolic_Per in objSolicitud.SOLIC_PER)
                    if (objSolic_Per.e_ano >= objPeriodo.e_ano)
                        solicitudesCanceladas[objSolicitud.Id] = objSolicitud.oi_estado_solic;

            //Recorrer la hashtable y cancelar this.Cancelar_Solicitud(objSolicitud.Id);
            foreach (string key in solicitudesCanceladas.Keys)
                this.CancelarSolicitud(key);

            foreach (NucleusRH.Base.Vacaciones.LegajoVacaciones.CTA_CTE_VAC Cta_Cte in this.CTA_CTE_VAC)
            {
                if (objPeriodo.e_ano > Cta_Cte.e_ano)
                    Cta_Cte.c_estado = "P";
                else
                    if (objPeriodo.e_ano != Cta_Cte.e_ano && objPeriodo.e_ano + 1 != Cta_Cte.e_ano)
                        throw new Exception(error + "hay períodos generados mayores al que se quiere generar [01]");
            }

            //Validación de Cuenta Corriente
            //Buscar la Cuenta Corriente que tenga c_ano = objPeriodo.c_ano
            objCta_Cte = (NucleusRH.Base.Vacaciones.LegajoVacaciones.CTA_CTE_VAC)this.CTA_CTE_VAC.GetByAttribute("e_ano", objPeriodo.e_ano);

            //Existe la cuenta corriente
            if (objCta_Cte == null)
            {
                //Creo la Cuenta Corriente Actual
                objCta_Cte = new NucleusRH.Base.Vacaciones.LegajoVacaciones.CTA_CTE_VAC();
                objCta_Cte.c_estado = "A";
                objCta_Cte.e_dias_otorg = 0;
                objCta_Cte.e_ano = objPeriodo.e_ano;
                this.CTA_CTE_VAC.Add(objCta_Cte);
                l_sobreescribe = true;
            }

            //Se debe sobreescribir la Cuenta Corriente
            if (!l_sobreescribe && objCta_Cte.c_estado != "F")
                throw new Exception(error + "ya existe una Cuenta Corriente para el año a generar y no se indicó que se debe sobreescribir [02]");

            //Verificar el estado de la cuenta corriente, si es [P] Pasado no se puede realizar el cálculo
            if (objCta_Cte.c_estado == "P")
                throw new Exception(error + "el estado de la Cuenta Corriente para la cual se quiere realizar el cálculo de vacaciones es Pasado [03]");

            if (objCta_Cte.e_dias_otorg != 0)
                throw new Exception(error + "error en los datos [04]");

            objCta_Cte.c_estado = "A";
            objCta_Cte.oi_periodo = objPeriodo.Id;

            objCta_Cte_Futura = (NucleusRH.Base.Vacaciones.LegajoVacaciones.CTA_CTE_VAC)this.CTA_CTE_VAC.GetByAttribute("c_estado", "F");
            if (objCta_Cte_Futura == null)
            {
                //Crear la Cuenta Corriente Futura
                objCta_Cte_Futura = new NucleusRH.Base.Vacaciones.LegajoVacaciones.CTA_CTE_VAC();
                objCta_Cte_Futura.c_estado = "F";
                objCta_Cte_Futura.e_ano = objPeriodo.e_ano + 1;
                objCta_Cte_Futura.e_dias_gen = 0;
                objCta_Cte_Futura.e_dias_pend = 0;
                objCta_Cte_Futura.oi_periodo = "1000";
                this.CTA_CTE_VAC.Add(objCta_Cte_Futura);
            }

            //Generación de Vacaciones
            double dias_generados = 0d;
            string param = "";

            //Tiene días pactados, no se calculan los dias de vacaciones
            if (this.e_cant_dias_vac > 0)
                dias_generados = (double)this.e_cant_dias_vac;
            //No tiene dias pactados
            else
            {
                //Validar que tenga categoría
                if (this.oi_categoria_ultNull)
                    throw new Exception("El Empleado no tiene Días de Vacaciones Pactados o Categoría cargada, no se generan días de vacaciones [05]");

                // Calculo dias trabajados en el año
                int dias_trabajados = this.Dias_Trabajados_Vacaciones(objPeriodo.f_desde_periodo, objPeriodo.f_hasta_periodo);

                // Calculo la antigüedad del Legajo
                int antiguedad = this.Antiguedad_Meses(objPeriodo.f_hasta_periodo);

                // XML de parametros para el query que obtiene los datos de la matriz de vacaciones.
                param = "<DATOS oi_categoria=\"" + this.oi_categoria_ult + "\" e_antiguedad=\"" + antiguedad + "\"/>";

                // Ejecuto el recurso para obtener los datos necesarios para el cálculo de vacaciones
                NomadXML datos_categ = (new NomadXML(NomadEnvironment.QueryString(PERSONAL_EMP.Resources.QRY_DATOS_CATEG, param))).FirstChild();

                // si el query no devuelve nada, es porq la categoria no tiene cargada la matriz de vacaciones,
                // debe volver y seguir con la persona siguiente
                if (datos_categ.GetAttr("e_dias_vac") == "")
                    throw new Exception("El convenio no tiene cargada la matriz de vacaciones o la misma no posee celdas para la antigüedad del Legajo (" + antiguedad.ToString() + "). Legajo Ignorado [06]");

                NucleusRH.Base.Personal.LegajoEmpresa.PERSONAL_EMP oLegajo = NucleusRH.Base.Personal.LegajoEmpresa.PERSONAL_EMP.Get(this.Id);

                // Si no alcanzo a trabajar el minimo de dias que dice el convenio, se genera un dia
                // de vacaciones por cada "n" dias trabajados, (siendo n la proporcion que dice el convenio e_dias_prop)
                // Se saltea este paso si tiene fecha de antiguedad reconocida en el Legajo del Vacaciones o en el Legajo del Personal
                if (dias_trabajados < datos_categ.GetAttrInt("e_min_dias_trab"))
                {
                    if (dias_trabajados < datos_categ.GetAttrInt("e_dias_prop"))
                        throw new Exception("no le corresponden días de vacaciones, la cantidad de días trabajados (" + dias_trabajados.ToString() + ") es menor a la cantidad de días proporcionales (" + datos_categ.GetAttrInt("e_dias_prop").ToString() + ") [07]");
                    NomadLog.Debug("dias_trabajados -- " + dias_trabajados);
                    NomadLog.Debug("e_dias_prop -- " + datos_categ.GetAttrDouble("e_dias_prop"));
                    dias_generados = dias_trabajados / datos_categ.GetAttrDouble("e_dias_prop");
                }
                // Si alcanzo los dias minimos de trabajo,
                // se busca en la matriz de vacaciones con la antiguedad
                else
                    dias_generados = datos_categ.GetAttrDouble("e_dias_vac");

              NomadLog.Debug("redondeo -- " + datos_categ.GetAttr("c_redondeo"));
              NomadLog.Debug("dias_generados 0 -- " + dias_generados);

              //aplico el redondeo segun el valor de la matriz
              if(datos_categ.GetAttr("c_redondeo") == "1")
                dias_generados = Math.Ceiling(dias_generados);
              else if(datos_categ.GetAttr("c_redondeo") == "2")
                dias_generados = Math.Floor(dias_generados);
              else if(datos_categ.GetAttr("c_redondeo") == "3")
                dias_generados = Math.Round(dias_generados, 0);
              NomadLog.Debug("dias_generados 1 -- " + dias_generados);
            }

            objCta_Cte.e_dias_gen = (int)dias_generados;
            objCta_Cte.e_dias_pend = (int)dias_generados;
            objCta_Cte.e_dias_otorg = 0;

            //Recorro la lista de solicitudes canceladas y las agrego a una lista generica
            foreach (string key in solicitudesCanceladas.Keys)
            {
                SOLICITUD objSolicitudAprob = (SOLICITUD)this.SOLICITUDES.GetById(key);
                lista_soli_can.Add(objSolicitudAprob);
            }

            //Ordeno por fecha desde las solicitudes canceladas
            lista_soli_can.Sort(new Comparison<SOLICITUD>(delegate(SOLICITUD a, SOLICITUD b) { return DateTime.Compare((DateTime)a.f_desde_solicitud, (DateTime)b.f_desde_solicitud); }));


            //Recorro la Lista ordenada de las solicitudes canceladas y las apruebo
            foreach (SOLICITUD item in lista_soli_can)
            {
                AprobarSolicitud(this, item.Id, true, false);
                SOLICITUD objSolicitudCancelada = (SOLICITUD)this.SOLICITUDES.GetById(item.Id);
                objSolicitudCancelada.oi_estado_solic = (string)solicitudesCanceladas[item.Id];
            }

            NomadEnvironment.GetCurrentTransaction().Save(this);
        }

        /// <summary>
        /// Dias Trabajados para Vacaciones
        /// Contempla también las fechas de antiguedad a diferencia del metodo en Legajo Empresa
        /// </summary>
        /// <param name="Desde"></param>
        /// <param name="Hasta"></param>
        /// <returns>Cantidad de días Trabajados</returns>
        public int Dias_Trabajados_Vacaciones(DateTime Desde, DateTime Hasta)
        {
            //Recupero el Legajo Empresa
            NucleusRH.Base.Personal.LegajoEmpresa.PERSONAL_EMP oPersonalEmp = NucleusRH.Base.Personal.LegajoEmpresa.PERSONAL_EMP.Get(this.id,true);

            //Si el cálculo de días trabajados debe realizarse con la fecha de ingreso llama al método del Legajo Empresa
            if ((this.f_ant_rec_vacNull || oPersonalEmp.f_ingreso <= this.f_ant_rec_vac) && (oPersonalEmp.f_antiguedad_recNull || oPersonalEmp.f_ingreso <= oPersonalEmp.f_antiguedad_rec))
                return oPersonalEmp.Dias_Trabajados(Desde, Hasta);
            else
                {
                    //Establezco la fecha de antiguedad
                    DateTime fecha_antiguedad;
                    if (this.f_ant_rec_vacNull && !oPersonalEmp.f_antiguedad_recNull) fecha_antiguedad = oPersonalEmp.f_antiguedad_rec;
                    else if (!this.f_ant_rec_vacNull && oPersonalEmp.f_antiguedad_recNull) fecha_antiguedad = this.f_ant_rec_vac;
                    else if (this.f_ant_rec_vac <= oPersonalEmp.f_antiguedad_rec) fecha_antiguedad = this.f_ant_rec_vac;
                    else fecha_antiguedad = oPersonalEmp.f_antiguedad_rec;

                    NomadEnvironment.GetTrace().Info("****** Dias Trabajados ******");
                    DateTime f_desde = DateTime.Now;
                    DateTime f_hasta = DateTime.Now;

                    //Validación de fechas para cáculo de días hábiles - Empleado Mensualizado
                    if (oPersonalEmp.TEMP_PER.Count == 0)
                    {
                        NomadEnvironment.GetTrace().Info("Periodo a calcular: " + Desde.ToString() + " - " + Hasta.ToString());
                        foreach (NucleusRH.Base.Personal.LegajoEmpresa.INGRESOS_PER ingreso in oPersonalEmp.INGRESOS_PER)
                        {
                            NomadEnvironment.GetTrace().Info("Periodo del Legajo " + this.e_numero_legajo + ": " + ingreso.f_ingreso.ToString() + " - " + ingreso.f_egreso.ToString());
                            if (ingreso.f_ingreso > Hasta)
                            {
                                NomadEnvironment.GetTrace().Info("Periodo ignorado por Fecha de Ingreso mayor a Fecha Hasta: " + ingreso.f_ingreso.ToString() + " > " + Hasta.ToString());
                                continue;
                            }
                            else
                            {
                                if (!ingreso.f_egresoNull)
                                {
                                    if (ingreso.f_egreso < Desde)
                                    {
                                        NomadEnvironment.GetTrace().Info("Periodo ignorado por Fecha de Egreso menor a Fecha Desde: " + ingreso.f_egreso.ToString() + " > " + Desde.ToString());
                                        continue;
                                    }
                                    else
                                        f_hasta = ingreso.f_egreso > Hasta ? Hasta : ingreso.f_egreso;
                                }
                                else
                                {
                                    f_hasta = Hasta;
                                }
                                f_desde = ingreso.f_ingreso > Desde ? ingreso.f_ingreso : Desde;
                            }
                        }
                    }
                    else
                        NomadEnvironment.GetBatch().Trace.Add("WRN", "El empleado es de Temporada, no se puede realizar el calculo. Legajo Ignorado", "Generacion Vacaciones");
                    NomadEnvironment.GetTrace().Info("Calculo de dias trabajados con fecha desde: " + f_desde.ToString() + " y fecha hasta: " + f_hasta.ToString());
                    //Cálculo de días hábiles
                    int dias_habiles = this.Dias_Habiles_Vacaciones(fecha_antiguedad, f_hasta);
                    //Cálculo de días de licencia
                    int dias_licencia = oPersonalEmp.Dias_Licencia(Desde, Hasta);
                    //Cálculo de días trabajados
                    int dias_trabajados = dias_habiles - dias_licencia;
                    NomadEnvironment.GetTrace().Info("Dias Habiles: " + dias_habiles);
                    NomadEnvironment.GetTrace().Info("Dias Licencia: " + dias_licencia);
                    return dias_trabajados;
                }
            }

        /// <summary>
        /// Dias Habiles Trabajados tomando en cuenta la fecha de antiguedad reconocida del Legajo
        /// </summary>
        /// <param name="Desde"></param>
        /// <param name="Hasta"></param>
        /// <returns>Cantidad de días hábiles de vacaciones</returns>
        public int Dias_Habiles_Vacaciones(DateTime f_desde, DateTime f_hasta)
        {
            NomadEnvironment.GetTrace().Info("****** Dias Habiles ******");
            NucleusRH.Base.Organizacion.Calendario_Feriados.CALENDARIO calendario;
            int dias_habiles = 0;
            int dias_periodo;
            TimeSpan diff;

            ArrayList calendarios = new ArrayList();
            foreach (NucleusRH.Base.Personal.LegajoEmpresa.CALENDARIO_PER cal in this.CALENDARIO_PER)
            {

                calendario = cal.Getoi_calendario();
                bool[] week = new bool[7] { calendario.l_trab_domingos, calendario.l_trab_lunes, calendario.l_trab_martes, calendario.l_trab_miercoles, calendario.l_trab_jueves, calendario.l_trab_viernes, calendario.l_trab_sabados };
                DateTime f_ini_per;
                DateTime f_fin_per;

                NomadEnvironment.GetTrace().Info("cal.fesde: " + cal.f_desde.ToString());
                NomadEnvironment.GetTrace().Info("f_desde: " + f_desde.ToString());
                NomadEnvironment.GetTrace().Info("cal.hasta: " + cal.f_hasta.ToString());
                NomadEnvironment.GetTrace().Info("f_hasta: " + f_hasta.ToString());

                if (f_hasta > cal.f_desde && (f_desde < cal.f_hasta || cal.f_hastaNull))
                {
                    if (f_desde > cal.f_desde)
                    {
                        f_ini_per = cal.f_desde;
                    }
                    else
                    {
                        f_ini_per = f_desde;
                    }
                    if (f_hasta < cal.f_hasta || cal.f_hastaNull)
                    {
                        f_fin_per = f_hasta;
                    }
                    else
                    {
                        f_fin_per = cal.f_hasta;
                    }
                    NomadEnvironment.GetTrace().Info("f_ini_per: " + f_ini_per.ToString());
                    NomadEnvironment.GetTrace().Info("f_fin_per: " + dias_habiles.ToString());
                    diff = f_fin_per - f_ini_per;
                    dias_periodo = (int)diff.TotalDays + 1;

                    DateTime fecha = f_ini_per;

                    for (int i = 0; i < 7; i++)
                    {
                        if (week[(int)fecha.DayOfWeek])
                        {
                            dias_habiles += (int)(dias_periodo / 7);
                            if (i < (dias_periodo % 7))
                            {
                                dias_habiles++;
                            }
                        }
                        fecha = fecha.AddDays(1);
                    }
                }
                NomadEnvironment.GetTrace().Info("Calendario: " + cal.Code + " - Dias habiles: " + dias_habiles);
            }
            NomadEnvironment.GetTrace().Info("Dias Habiles Total: " + dias_habiles);
            return dias_habiles;
        }

        /// <summary>
        /// Importa la Cuenta Corriente de un Legajo a otro (Generalmente utilizado cuando pasa de una empresa a otra)
        /// </summary>
        /// <param name="oi_personal_emp_origen">Legajo de Origen de donde se toman las Cuentas Corriente</param>
        /// <param name="oi_personal_emp_destino">Legajo de Destino donde se cargarán las Cuentas Corrientes</param>
        public static void ImportarCuentaCorriente(string oi_personal_emp_origen, string oi_personal_emp_destino)
        {
            int anio_actual=0, totRegs, linea = 0, errores = 0, procesados = 0;
            NucleusRH.Base.Vacaciones.LegajoVacaciones.CTA_CTE_VAC cta_cte;
            Hashtable solicitudesCanceladas = new Hashtable();

            //Instancio el Objeto Batch
            NomadBatch objBatch;
            objBatch = NomadBatch.GetBatch("Importación de Cuenta Corriente", "Importación de Cuenta Corriente");

            //Obtengo el Legajo Origen y el Legajo Destino
            NucleusRH.Base.Vacaciones.LegajoVacaciones.PERSONAL_EMP oLegajoOrigen = NucleusRH.Base.Vacaciones.LegajoVacaciones.PERSONAL_EMP.Get(oi_personal_emp_origen,true);
            NucleusRH.Base.Vacaciones.LegajoVacaciones.PERSONAL_EMP oLegajoDestino = NucleusRH.Base.Vacaciones.LegajoVacaciones.PERSONAL_EMP.Get(oi_personal_emp_destino,true);

            totRegs = oLegajoOrigen.CTA_CTE_VAC.Count;
            foreach (NucleusRH.Base.Vacaciones.LegajoVacaciones.CTA_CTE_VAC cuentaCorriente in oLegajoOrigen.CTA_CTE_VAC)
            {
                linea++;
                objBatch.SetPro(0, 100, totRegs, linea);
                objBatch.SetMess("Importando la Linea " + linea + " de " + totRegs);
                NucleusRH.Base.Vacaciones.Periodos.PERIODO objPeriodo = NucleusRH.Base.Vacaciones.Periodos.PERIODO.Get(cuentaCorriente.oi_periodo);

                if (oLegajoDestino.CTA_CTE_VAC.GetByAttribute("e_ano", objPeriodo.e_ano) != null)
                {
                    NucleusRH.Base.Vacaciones.LegajoVacaciones.CTA_CTE_VAC objCtaCteFutura = (NucleusRH.Base.Vacaciones.LegajoVacaciones.CTA_CTE_VAC) oLegajoDestino.CTA_CTE_VAC.GetByAttribute("e_ano", objPeriodo.e_ano);
                    //En caso de que la cuenta corriente que se superpone es una cuenta corriente futuro y la cuenta corriente origen no es una cuenta corriente futuro
                    if (objCtaCteFutura.c_estado == "F" && cuentaCorriente.c_estado!="F")
                    {
                        cta_cte = new NucleusRH.Base.Vacaciones.LegajoVacaciones.CTA_CTE_VAC();
                        cta_cte.oi_periodo = cuentaCorriente.oi_periodo;
                        cta_cte.e_dias_gen = cuentaCorriente.e_dias_gen;
                        //e_dias_otorg
                        if (!cuentaCorriente.e_dias_otorgNull)
                            cta_cte.e_dias_otorg = cuentaCorriente.e_dias_otorg;
                        else
                            cta_cte.e_dias_otorgNull = true;
                        //e_dias_interrup
                        if (!cuentaCorriente.e_dias_interrupNull)
                            cta_cte.e_dias_interrup = cuentaCorriente.e_dias_interrup;
                        else
                            cta_cte.e_dias_interrupNull = true;
                        //e_dias_cta_per_ant
                        if (!cuentaCorriente.e_dias_cta_per_antNull)
                            cta_cte.e_dias_cta_per_ant = cuentaCorriente.e_dias_cta_per_ant;
                        else
                            cta_cte.e_dias_cta_per_antNull = true;

                        cta_cte.e_dias_pend = cuentaCorriente.e_dias_pend;
                        cta_cte.l_notificada = cuentaCorriente.l_notificada;
                        cta_cte.e_ano = cuentaCorriente.e_ano;
                        cta_cte.c_estado = "A";
                        oLegajoDestino.CTA_CTE_VAC.Add(cta_cte);
                        objCtaCteFutura.e_ano = objPeriodo.e_ano + 1;
                        objBatch.Log("Creando la cuenta corriente actual para el año "+objPeriodo.e_ano.ToString());
                        objBatch.Log("Creando la cuenta corriente futura para el año "+(objPeriodo.e_ano+1).ToString());
                        procesados++;
                    }
                    else
                    {
                        objBatch.Err("La cuenta corriente para el periodo " + objPeriodo.d_periodo.ToString() + " ya existe en el Legajo Destino");
                        errores++;
                    }
                }
                else
                {
                    //Si la cuenta corriente a agregar es Futura y el Legajo ya tiene una cuenta corriente futura
                    if (cuentaCorriente.c_estado == "F" && oLegajoDestino.CTA_CTE_VAC.GetByAttribute("c_estado", "F") != null)
                    {
                        //Copio la información de la cuenta corriente futura origen a la cuenta corriente futura destino
                        NucleusRH.Base.Vacaciones.LegajoVacaciones.CTA_CTE_VAC objCtaCteFutura = (NucleusRH.Base.Vacaciones.LegajoVacaciones.CTA_CTE_VAC)oLegajoDestino.CTA_CTE_VAC.GetByAttribute("c_estado", "F");
                        objCtaCteFutura.e_dias_gen = cuentaCorriente.e_dias_gen;
                        //e_dias_otorg
                        if (!cuentaCorriente.e_dias_otorgNull)
                            objCtaCteFutura.e_dias_otorg = cuentaCorriente.e_dias_otorg;
                        else
                            objCtaCteFutura.e_dias_otorgNull = true;
                        //e_dias_interrup
                        if (!cuentaCorriente.e_dias_interrupNull)
                            objCtaCteFutura.e_dias_interrup = cuentaCorriente.e_dias_interrup;
                        else
                            objCtaCteFutura.e_dias_interrupNull = true;
                        //e_dias_cta_per_ant
                        if (!cuentaCorriente.e_dias_cta_per_antNull)
                            objCtaCteFutura.e_dias_cta_per_ant = cuentaCorriente.e_dias_cta_per_ant;
                        else
                            objCtaCteFutura.e_dias_cta_per_antNull = true;

                        objCtaCteFutura.e_dias_pend = cuentaCorriente.e_dias_pend;
                        objCtaCteFutura.l_notificada = cuentaCorriente.l_notificada;
                        //Pone como cuenta corriente futura la que tiene el año más reciente
                        if (cuentaCorriente.e_ano > objCtaCteFutura.e_ano)
                            objCtaCteFutura.e_ano = cuentaCorriente.e_ano;
                        objBatch.Log("Reemplazando la Cuenta Corriente Futura del Destino con la información de la Cuenta Corriente Origen");
                        procesados++;
                    }
                    else
                    {
                        cta_cte = new NucleusRH.Base.Vacaciones.LegajoVacaciones.CTA_CTE_VAC();
                        cta_cte.oi_periodo = cuentaCorriente.oi_periodo;
                        cta_cte.e_dias_gen = cuentaCorriente.e_dias_gen;
                        //e_dias_otorg
                        if (!cuentaCorriente.e_dias_otorgNull)
                            cta_cte.e_dias_otorg = cuentaCorriente.e_dias_otorg;
                        else
                            cta_cte.e_dias_otorgNull = true;
                        //e_dias_interrup
                        if (!cuentaCorriente.e_dias_interrupNull)
                            cta_cte.e_dias_interrup = cuentaCorriente.e_dias_interrup;
                        else
                            cta_cte.e_dias_interrupNull = true;
                        //e_dias_cta_per_ant
                        if (!cuentaCorriente.e_dias_cta_per_antNull)
                            cta_cte.e_dias_cta_per_ant = cuentaCorriente.e_dias_cta_per_ant;
                        else
                            cta_cte.e_dias_cta_per_antNull = true;

                        cta_cte.e_dias_pend = cuentaCorriente.e_dias_pend;
                        cta_cte.l_notificada = cuentaCorriente.l_notificada;
                        cta_cte.e_ano = cuentaCorriente.e_ano;
                        cta_cte.c_estado = cuentaCorriente.c_estado;
                        oLegajoDestino.CTA_CTE_VAC.Add(cta_cte);
                        objBatch.Log("Creando la cuenta corriente para el año "+cuentaCorriente.e_ano);
                        procesados++;
                    }
                }

            }

            //Determinar nuevamente cual es la cuenta corriente Actual
            foreach (NucleusRH.Base.Vacaciones.LegajoVacaciones.CTA_CTE_VAC cuentaCorriente in oLegajoDestino.CTA_CTE_VAC)
            {
                if (cuentaCorriente.c_estado != "F" && cuentaCorriente.e_ano>anio_actual)
                {
                    anio_actual = cuentaCorriente.e_ano;
                    cuentaCorriente.c_estado = "A";
                }
            }

            //Determinan las cuentas corrientes Pasado
            foreach (NucleusRH.Base.Vacaciones.LegajoVacaciones.CTA_CTE_VAC cuentaCorriente in oLegajoDestino.CTA_CTE_VAC)
            {
                if (cuentaCorriente.c_estado != "F" && cuentaCorriente.e_ano != anio_actual)
                    cuentaCorriente.c_estado = "P";
            }

            //Si se generaron nuevas cuentas corrientes se deberan cancelar aquellas solicitudes que tengan periodos generados y volver a aprobar
            //para calcular nuevamente los dias correspondientes a cada periodo
            if(procesados>=1)
                foreach (NucleusRH.Base.Vacaciones.LegajoVacaciones.SOLICITUD objSolicitud in oLegajoDestino.SOLICITUDES)
                    foreach (NucleusRH.Base.Vacaciones.LegajoVacaciones.SOLIC_PER objSolic_Per in objSolicitud.SOLIC_PER)
                        solicitudesCanceladas[objSolicitud.Id] = objSolicitud.oi_estado_solic;

            //Recorrer la hashtable y cancelar this.Cancelar_Solicitud(objSolicitud.Id);
            foreach (string key in solicitudesCanceladas.Keys)
                oLegajoDestino.CancelarSolicitud(key);

            //Aprobar todas las solicitudes de la HashTable
            foreach (string key in solicitudesCanceladas.Keys)
            {
                AprobarSolicitud(oLegajoDestino, key, true, false);
                SOLICITUD objSolicitudAprob = (SOLICITUD)oLegajoDestino.SOLICITUDES.GetById(key);
                objSolicitudAprob.oi_estado_solic = (string)solicitudesCanceladas[key];
            }

            try
            {
                NomadEnvironment.GetCurrentTransaction().Save(oLegajoDestino);
            }
            catch
            {
                throw new NomadAppException("No se pudo importar la cuenta corriente. Existen periodos duplicados para el legajo destino.");
            }

            //Resultados
            objBatch.Log("Cantidad de Cuentas Corrientes importadas correctamente: "+procesados.ToString());
            objBatch.Log("Cantidad de Errores en el Proceso: "+errores.ToString());
            objBatch.Log("Finalizado...");
        }
    }
}



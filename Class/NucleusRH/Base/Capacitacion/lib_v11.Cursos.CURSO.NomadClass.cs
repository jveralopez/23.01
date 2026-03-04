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

namespace NucleusRH.Base.Capacitacion.Cursos
{
    public partial class CURSO : Nomad.NSystem.Base.NomadObject
    {
        public static Nomad.NSystem.Document.NmdXmlDocument Plan_Capacitacion(Nomad.NSystem.Document.NmdXmlDocument filtros)
        {
            StringWriter swr = new StringWriter();
            XmlTextWriter xtw = new XmlTextWriter(swr);
            xtw.Formatting = Formatting.None;

            //Aqui formo el xml con XmlTextWriter
            xtw.WriteStartElement("DATOS");
            Nomad.NSystem.Document.NmdXmlDocument xml_cursos = (Nomad.NSystem.Document.NmdXmlDocument)filtros.GetFirstChildDocument();



            Nomad.NSystem.Document.NmdXmlDocument curso;
            for (curso = (Nomad.NSystem.Document.NmdXmlDocument)xml_cursos.GetFirstChildDocument(); curso != null; curso = (Nomad.NSystem.Document.NmdXmlDocument)xml_cursos.GetNextChildDocument())
            {
                xtw.WriteStartElement("CURSO");
                xtw.WriteAttributeString("c_curso", curso.GetAttribute("c_curso").Value);
                xtw.WriteAttributeString("d_curso", curso.GetAttribute("d_curso").Value);
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
                xtw.WriteAttributeString("mes", fecha.ToString("MMMM"));
                xtw.WriteAttributeString("anio", fecha.Year.ToString());
                xtw.WriteStartElement("DIAS");
                ArrayList dias_mes = new ArrayList();
                while (fecha.Month == mes && fecha <= f_hasta)
                {
                    xtw.WriteStartElement("DIA");
                    xtw.WriteAttributeString("dia", fecha.Day.ToString());

                    string est;

                    // Marca cada dia como habil o no habil   
                    int dayofweek = (int)fecha.DayOfWeek;
                    if (dayofweek == 0 || dayofweek == 6)
                    {
                        est = "n";
                    }
                    else
                    {
                        est = "h";
                    }

                    // Marca el dia de hoy
                    if (fecha == DateTime.Today)
                    {
                        est = "hoy";
                    }

                    xtw.WriteAttributeString("est", est);
                    dias_mes.Add(est);
                    xtw.WriteEndElement();
                    fecha = fecha.AddDays(1);
                }
                xtw.WriteEndElement();
                xtw.WriteStartElement("CURSOS");

                Nomad.NSystem.Document.NmdXmlDocument dictados;
                Nomad.NSystem.Document.NmdXmlDocument dictado;

                for (curso = (Nomad.NSystem.Document.NmdXmlDocument)xml_cursos.GetFirstChildDocument(); curso != null; curso = (Nomad.NSystem.Document.NmdXmlDocument)xml_cursos.GetNextChildDocument())
                {
                    dictados = (Nomad.NSystem.Document.NmdXmlDocument)curso.GetFirstChildDocument();
                    xtw.WriteStartElement("CURSO");

                    fecha = f_inicio_mes;
                    int cont = 0;
                    while (fecha.Month == mes && fecha <= f_hasta)
                    {
                        string dic = "n";
                        xtw.WriteStartElement("DIA");

                        // Recorro los dictados del curso
                        // Marca cada dia como asignado o no asignado
                        for (dictado = (Nomad.NSystem.Document.NmdXmlDocument)dictados.GetFirstChildDocument(); dictado != null; dictado = (Nomad.NSystem.Document.NmdXmlDocument)dictados.GetNextChildDocument())
                        {
                            DateTime f_desde_dictado = Nomad.NSystem.Functions.StringUtil.str2date(dictado.GetAttribute("f_inicio").Value);
                            DateTime f_hasta_dictado = Nomad.NSystem.Functions.StringUtil.str2date(dictado.GetAttribute("f_fin").Value);
                            if (fecha >= f_desde_dictado && fecha <= f_hasta_dictado)
                            {
                                dic = "a";
                                break;
                            }
                        }

                        xtw.WriteAttributeString("est", dias_mes[cont].ToString() + dic);
                        xtw.WriteEndElement();
                        fecha = fecha.AddDays(1);
                        cont++;
                    }
                    xtw.WriteEndElement();
                }
                xtw.WriteEndElement();
                xtw.WriteEndElement();
            }

            xtw.Flush();
            xtw.Close();

            string myXml = swr.ToString();

            Nomad.NSystem.Document.NmdXmlDocument doc = new Nomad.NSystem.Document.NmdXmlDocument(myXml);
            return doc;
        }
    }
}

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

namespace NucleusRH.Base.Organizacion.Calendario_Feriados
{
    //////////////////////////////////////////////////////////////////////////////////
    //Clase Calendario Feriados
    public partial class CALENDARIO : Nomad.NSystem.Base.NomadObject
    {
        public static Nomad.NSystem.Proxy.NomadXML DiasTrabajablesString(Nomad.NSystem.Proxy.NomadXML xmlparam)
        {
            NomadLog.Info("-----------------------------------------------");
            NomadLog.Info("CALENDARIO.DiasTrabajablesString---------------");
            NomadLog.Info("-----------------------------------------------");
            string retval = "";
            xmlparam = xmlparam.FirstChild();
            try
            {
                string oi_personal_emp = xmlparam.GetAttr("oi_personal_emp");
                DateTime f_desde = Nomad.NSystem.Functions.StringUtil.str2date(xmlparam.GetAttr("f_desde"));
                DateTime f_hasta = Nomad.NSystem.Functions.StringUtil.str2date(xmlparam.GetAttr("f_hasta"));

                NucleusRH.Base.Personal.LegajoEmpresa.PERSONAL_EMP obj_Per = NucleusRH.Base.Personal.LegajoEmpresa.PERSONAL_EMP.Get(oi_personal_emp,true);
                NomadLog.Info("Recupero el legajo:" + obj_Per.e_numero_legajo);
                for (System.DateTime fecha = f_desde; fecha <= f_hasta; fecha = fecha.AddDays(1))
                {
                    if (obj_Per.DiaLaboral(fecha))
                        retval = retval + "1";
                    else
                        retval = retval + "0";
                }
                NomadLog.Info("retval:" + retval);
                xmlparam.SetAttr("rta", "1");
                xmlparam.SetAttr("d_laborales", retval);
            }
            catch (NomadException nmdEx)
            {
                xmlparam.SetAttr("rta", "0");
                xmlparam.SetAttr("msg", nmdEx.Message);
                NomadLog.Info("ERROR DiasTrabajablesString:" + nmdEx.Message);
            }
            catch(Exception e)
            {
                xmlparam.SetAttr("rta", "0");
                xmlparam.SetAttr("msg", "Error al recuperar los días trabajables del recurso");
                NomadLog.Info("ERROR DiasTrabajablesString:" + e.Message);
            }
            return xmlparam;
        }

        public static Nomad.NSystem.Proxy.NomadXML DiasTrabajablesRptString(Nomad.NSystem.Proxy.NomadXML xmlparam)
        {
            NomadLog.Info("-----------------------------------------------");
            NomadLog.Info("CALENDARIO.DiasTrabajablesRptString---------------");
            NomadLog.Info("-----------------------------------------------");
            string retval = "";
            xmlparam = xmlparam.FirstChild();
            try
            {
                string c_empresa = xmlparam.GetAttr("c_empresa");
                DateTime f_desde = Nomad.NSystem.Functions.StringUtil.str2date(xmlparam.GetAttr("f_desde"));
                DateTime f_hasta = Nomad.NSystem.Functions.StringUtil.str2date(xmlparam.GetAttr("f_hasta"));

                string oiEMP = "";
                oiEMP = NomadEnvironment.QueryValue("ORG03_EMPRESAS", "oi_empresa", "c_empresa", c_empresa, "", true);
                NucleusRH.Base.Organizacion.Empresas.EMPRESA empresa = NucleusRH.Base.Organizacion.Empresas.EMPRESA.Get(oiEMP, true);
                NomadLog.Info("Recupero Empresa:" + empresa.d_empresa);

                NucleusRH.Base.Organizacion.Calendario_Feriados.CALENDARIO cal = null;
                for (int i = 0; i < empresa.FERIADOS_EMP.Count;i++)
                {
                    NucleusRH.Base.Organizacion.Calendario_Feriados.CALENDARIO calaux = NucleusRH.Base.Organizacion.Calendario_Feriados.CALENDARIO.Get(empresa.FERIADOS_EMP[i].GetAttrObject("oi_calendario").ToString());
                    if (calaux.c_calendario == "STD")
                    {
                        cal = calaux;
                        break;
                    }
                }

                if(cal!=null)
                {
                    for (System.DateTime fecha = f_desde; fecha <= f_hasta; fecha = fecha.AddDays(1))
                    {
                        if(cal.DIAS_FERIADOS.GetByAttribute("f_feriado", fecha) != null)
                        {
                            if(cal.l_trab_feriados)
                                retval = retval + "1";
                            else
                                retval = retval + "0";
                        }
                        else
                        {
                            switch (fecha.DayOfWeek)
                            {
                                case System.DayOfWeek.Monday: retval += cal.l_trab_lunes? "1" : "0"; break;
                                case System.DayOfWeek.Tuesday: retval += cal.l_trab_martes ? "1" : "0"; break;
                                case System.DayOfWeek.Wednesday: retval += cal.l_trab_miercoles ? "1" : "0"; break;
                                case System.DayOfWeek.Thursday: retval += cal.l_trab_jueves ? "1" : "0"; break;
                                case System.DayOfWeek.Friday: retval += cal.l_trab_viernes ? "1" : "0"; break;
                                case System.DayOfWeek.Saturday: retval += cal.l_trab_sabados ? "1" : "0"; break;
                                case System.DayOfWeek.Sunday: retval += cal.l_trab_domingos ? "1" : "0"; break;
                            }
                        }
                    }

                    NomadLog.Info("retval:" + retval);
                    xmlparam.SetAttr("rta", "1");
                    xmlparam.SetAttr("d_laborales", retval);
                }
                else
                {
                    xmlparam.SetAttr("rta", "0");
                    xmlparam.SetAttr("msg", "NO HAY CALENDARIO STD DEFINIDO");
                    NomadLog.Info("ERROR DiasTrabajablesRptString: NO HAY CALENDARIO STD DEFINIDO");
                }

            }
            catch (NomadException nmdEx)
            {
                xmlparam.SetAttr("rta", "0");
                xmlparam.SetAttr("msg", nmdEx.Message);
                NomadLog.Info("ERROR DiasTrabajablesRptString:" + nmdEx.Message);
            }
            catch (Exception e)
            {
                xmlparam.SetAttr("rta", "0");
                xmlparam.SetAttr("msg", "Error al recuperar los días trabajables del recurso");
                NomadLog.Info("ERROR DiasTrabajablesRptString:" + e.Message);
            }

            return xmlparam;
        }
    }
}



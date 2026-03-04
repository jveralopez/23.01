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

using NucleusRH.Base.Tiempos_Trabajados.NovedadesRol;
using NucleusRH.Base.Tiempos_Trabajados.NovedadesSec;
using NucleusRH.Base.Tiempos_Trabajados.Terminales;
using NucleusRH.Base.Tiempos_Trabajados.Personal;
using NucleusRH.Base.Tiempos_Trabajados.Esperanzaper;
using NucleusRH.Base.Tiempos_Trabajados.Liquidacion_Personas;

namespace NucleusRH.Base.Tiempos_Trabajados.NovedadesSec 
{
  //////////////////
  //Clase Novedades
  public partial class NOVEDAD_SEC : Nomad.NSystem.Base.NomadObject
  {
	///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    private static NomadXML GetFechas(string oi_personal_emp, DateTime f_fecha, DateTime f_horadesde, DateTime f_horahasta)
    {
        NomadXML xmlResult = new NomadXML("FECHAS");

        DateTime f_desde = f_fecha;
        f_desde = f_desde.AddHours(f_horadesde.Hour);
        f_desde = f_desde.AddMinutes(f_horadesde.Minute);
        xmlResult.SetAttr("f_desde", f_desde);

        DateTime f_hasta;
        if (f_horadesde < f_horahasta)
            f_hasta = f_fecha;
        else
            f_hasta = f_fecha.AddDays(1);
        f_hasta = f_hasta.AddHours(f_horahasta.Hour);
        f_hasta = f_hasta.AddMinutes(f_horahasta.Minute);
        xmlResult.SetAttr("f_hasta", f_hasta);
        
        //Recupero la fecha que corresponde a las fechas ingresadas        
        DateTime fecha_1 = ESPERANZAPER.GetDateHope(oi_personal_emp, f_desde);
        DateTime fecha_2 = ESPERANZAPER.GetDateHope(oi_personal_emp, f_hasta.AddMinutes(-1));
        xmlResult.SetAttr("fecha_1", fecha_1);
        xmlResult.SetAttr("fecha_2", fecha_2);

        return xmlResult;
    }

	///////////// VALIDACIONES VIEJAS ////////////////////////
    public static NomadXML ValidarNovedad(NomadXML xmlNovedad)
    {
        NomadXML xmlResult = new NomadXML("RESULT");
        xmlResult.SetAttr("valid", 0);

        string oi_personal_emp = xmlNovedad.FirstChild().GetAttr("oi_personal_emp");
        DateTime f_fecha = StringUtil.str2date(xmlNovedad.FirstChild().GetAttr("f_fecha"));
        DateTime f_horadesde = StringUtil.str2date(xmlNovedad.FirstChild().GetAttr("e_horadesde"));
        DateTime f_horahasta = StringUtil.str2date(xmlNovedad.FirstChild().GetAttr("e_horahasta"));

        //Obtener fechas
        NomadXML xmlFechas = GetFechas(oi_personal_emp, f_fecha, f_horadesde, f_horahasta);
        DateTime f_desde = StringUtil.str2date(xmlFechas.GetAttr("f_desde"));
        DateTime f_hasta = StringUtil.str2date(xmlFechas.GetAttr("f_hasta"));
        DateTime fecha_1 = StringUtil.str2date(xmlFechas.GetAttr("fecha_1"));
        DateTime fecha_2 = StringUtil.str2date(xmlFechas.GetAttr("fecha_2"));
        
        //Recuperar el Legajo
        PERSONAL_EMP ddoPerEmp = PERSONAL_EMP.Get(oi_personal_emp);

        //Verificar las Novedades
        foreach (NOVEDAD ddoNovPer in ddoPerEmp.NOVEDADES)
        {
            DateTime novFecDesde = ddoNovPer.f_fecha.AddMinutes(ddoNovPer.e_horainicio);
            DateTime novFecHasta = ddoNovPer.f_fecha.AddMinutes(ddoNovPer.e_horafin);

            if ((f_desde >= novFecDesde && f_desde <= novFecHasta) ||
                (f_hasta >= novFecDesde && f_hasta <= novFecHasta) ||
                (f_desde <= novFecDesde && f_hasta >= novFecHasta))
            {
                xmlResult.SetAttr("alert", "La novedad se superpone con otra novedad ya cargada.");
                return xmlResult;
            }
        }        

        //Si las fechas no son las mismas hay un error
        if (fecha_1 != fecha_2)
        {
            xmlResult.SetAttr("alert", "La novedad se superpone en dos días diferentes según el horario asignado al Legajo.");
            return xmlResult;            
        }
        else
        {
            if (Convert.ToInt32(f_desde.Subtract(fecha_1).TotalMinutes) > Convert.ToInt32(f_hasta.Subtract(fecha_1).TotalMinutes))
            {
                xmlResult.SetAttr("alert", "La hora desde ingresada no puede ser mayor a la hora hasta.");
                return xmlResult;                
            }
        }

        //Verificar si el dia esta bloqueado
        if (LIQUIDACIONPERS.EnLiquidacionCerrada(oi_personal_emp, fecha_1))
        {
            xmlResult.SetAttr("alert", "La fecha indicada corresponde a un procesamiento de horas cerrado.");
            return xmlResult;            
        }        

        xmlResult.SetAttr("valid", 1);

        return xmlResult;
    }

	//////////////////////////////////////////////////
	public static void AltaNovedadSec(NOVEDAD_SEC DDO)
    {
		//Obtener fecha y hora de inicio
		DateTime f_horadesde = StringUtil.str2date(DDO.e_horadesde);
		DateTime f_desde = DDO.f_fecha;
		f_desde = f_desde.AddHours(f_horadesde.Hour);
        f_desde = f_desde.AddMinutes(f_horadesde.Minute);
		
		//Obtener fecha y hora de fin
		DateTime f_horahasta = StringUtil.str2date(DDO.e_horahasta);
		DateTime f_hasta = DDO.f_fecha;
        if (f_horadesde >= f_horahasta) f_hasta = f_hasta.AddDays(1);
        f_hasta = f_hasta.AddHours(f_horahasta.Hour);
        f_hasta = f_hasta.AddMinutes(f_horahasta.Minute);
		
        //Recuperar la Novedad por Rol
        NOVEDAD_ROL ddoNovRol = NOVEDAD_ROL.Get(DDO.oi_novedad_rol);

        //Recuperar la Terminal
        TERMINAL ddoTerminal = TERMINAL.Get(ddoNovRol.oi_terminal);

        //Crear la Novedad
        NOVEDAD ddoNov = new NOVEDAD();
        ddoNov.oi_tipohora = ddoNovRol.oi_tipohora;
        ddoNov.f_fecha = ESPERANZAPER.GetDateHope(DDO.oi_personal_emp, f_desde);
        ddoNov.e_horainicio = Convert.ToInt32(f_desde.Subtract(ddoNov.f_fecha).TotalMinutes);
        ddoNov.e_horafin = Convert.ToInt32(f_hasta.Subtract(ddoNov.f_fecha).TotalMinutes);
        ddoNov.oi_estructura = ddoTerminal.oi_estructura;
        ddoNov.d_novedad = DDO.d_novedad;
        ddoNov.o_novedad = DDO.o_novedad;

		//Validar la Novedad
		NucleusRH.Base.Tiempos_Trabajados.Personal.NOVEDAD.ValidarAltaNovedad(DDO.oi_personal_emp,ddoNov);
		
        //Recuperar el Legajo
        PERSONAL_EMP ddoPerEmp = PERSONAL_EMP.Get(DDO.oi_personal_emp);


        // SE COMENTAN LAS SIGUIENTES LINEAS
		// SE DETECTA QUE REALIZA EL PROCESO EN EL METODO ValidarAltaNovedad
        /*
        //Agregar Novedad a la coleccion
        ddoPerEmp.NOVEDADES.Add(ddoNov);

        //Guardar el Legajo
        NomadEnvironment.GetCurrentTransaction().SaveRefresh(ddoPerEmp);
		*/
		
        //Completar la Novedad con Seguridad        
        DDO.oi_novedad = StringUtil.str2int(ddoPerEmp.NOVEDADES[ddoPerEmp.NOVEDADES.Count - 1].Id);
        DDO.f_crea = DateTime.Now;
        DDO.d_user_crea = NomadProxy.GetProxy().UserEtty;

        //Guardar la Novedad con Seguridad
        NomadEnvironment.GetCurrentTransaction().Save(DDO);
    }
	
	////////////////////////////////////////////
    public static void BajaNovedadSec(string ID)
    {
		//Recuperar Novedad con Seguridad
        NOVEDAD_SEC ddoNovSec = NOVEDAD_SEC.Get(ID);

        //Recuperar Legajo
        PERSONAL_EMP ddoPerEmp = PERSONAL_EMP.Get(ddoNovSec.oi_personal_emp);

        //Borrar Novedad en Legajo
        ddoPerEmp.NOVEDADES.RemoveById(ddoNovSec.oi_novedad.ToString());

        //Guardar Legajo
        NomadEnvironment.GetCurrentTransaction().Save(ddoPerEmp);
		
        //Borrar Novedad con Seguridad
        NomadEnvironment.GetCurrentTransaction().Delete(ddoNovSec);
    }
  }
}
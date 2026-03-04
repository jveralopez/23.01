using System;
using System.Xml;
using System.IO;
using System.Collections;
using System.Collections.Generic;

using NucleusRH.Base.Tiempos_Trabajados.Esperanzaper;
using NucleusRH.Base.Tiempos_Trabajados.Liquidacion_Personas;
using NucleusRH.Base.Personal.LegajoEmpresa;

using Nomad.NSystem.Document;
using Nomad.NSystem.Proxy;
using Nomad.NSystem.Base;
using Nomad.NSystem.Functions;

namespace NucleusRH.Base.Tiempos_Trabajados.Personal
{
    //////////////////////////////////////////////////////////////////////////////////
    //Clase Novedad
    public partial class NOVEDAD
    {
    public static void AltaNovedad(string oi_personal_emp, NOVEDAD ddoNovPer)
    {
      //ValidarAltaNovedad(oi_personal_emp, ddoNovPer.f_fecha, ddoNovPer.e_horainicio, ddoNovPer.e_horafin);
            ValidarAltaNovedad(oi_personal_emp,ddoNovPer);
    }

    public static void AltaNovedadCustom(string oi_personal_emp, NOVEDAD ddoNovPer)
    {
      DateTime f_desde = ddoNovPer.f_fecha.AddMinutes(ddoNovPer.e_horainicio);
      DateTime f_hasta = ddoNovPer.f_fecha.AddMinutes(ddoNovPer.e_horafin);

      ddoNovPer.f_fecha = ESPERANZAPER.GetDateHope(oi_personal_emp, f_desde);
      ddoNovPer.e_horainicio = Convert.ToInt32(f_desde.Subtract(ddoNovPer.f_fecha).TotalMinutes);
      ddoNovPer.e_horafin = Convert.ToInt32(f_hasta.Subtract(ddoNovPer.f_fecha).TotalMinutes);

      AltaNovedad(oi_personal_emp, ddoNovPer);
    }

    //public static void ValidarAltaNovedad(string oi_personal_emp, DateTime f_fecha, int e_horainicio, int e_horafin)
        public static void ValidarAltaNovedad(string oi_personal_emp,NOVEDAD ddoNovPersona)
    {
      //Obtener fechas
            DateTime f_fecha = ddoNovPersona.f_fecha;
            int e_horainicio = ddoNovPersona.e_horainicio;
            int e_horafin = ddoNovPersona.e_horafin;
      DateTime f_desde = f_fecha.AddMinutes(e_horainicio);
      DateTime f_hasta = f_fecha.AddMinutes(e_horafin);
      DateTime fecha_1 = ESPERANZAPER.GetDateHope(oi_personal_emp, f_desde);
      DateTime fecha_2 = ESPERANZAPER.GetDateHope(oi_personal_emp, f_hasta.AddMinutes(-1));

      //Validar que no sean dias diferentes
            if (fecha_1 != fecha_2)
            {
                DateTime DiaEmpieza = ESPERANZAPER.GetDayStart(oi_personal_emp, f_fecha.Date.AddDays(1));
                int minutos = DiaEmpieza.Hour * 60 + DiaEmpieza.Minute + 1440;
                int horaFin = ddoNovPersona.e_horafin;
                ddoNovPersona.e_horafin = minutos;
                AltaNovedad(oi_personal_emp,ddoNovPersona);
                f_desde = f_fecha.AddMinutes(minutos);
                NOVEDAD ddoNovFecha2 = new NOVEDAD();
                ddoNovFecha2.oi_personal_emp = ddoNovPersona.oi_personal_emp;
                ddoNovFecha2.oi_estructura = ddoNovPersona.oi_estructura;
                ddoNovFecha2.oi_tipohora = ddoNovPersona.oi_tipohora;
                ddoNovFecha2.o_novedad = ddoNovPersona.o_novedad;
                ddoNovFecha2.f_fecha = DiaEmpieza;
                ddoNovFecha2.e_horainicio = minutos - 1440;
                ddoNovFecha2.d_novedad = ddoNovPersona.d_novedad;
                ddoNovFecha2.e_horafin = horaFin - 1440;
                ddoNovPersona = ddoNovFecha2;

                ddoNovPersona.f_fecha = ESPERANZAPER.GetDateHope(oi_personal_emp, f_desde);
                ddoNovPersona.e_horainicio = Convert.ToInt32(f_desde.Subtract(ddoNovPersona.f_fecha).TotalMinutes);
                ddoNovPersona.e_horafin = Convert.ToInt32(f_hasta.Subtract(ddoNovPersona.f_fecha).TotalMinutes);

            }
        //throw NomadException.NewMessage("Personal.NOVEDAD.ERR-DIAS-DIF");

      //Validar que hora inicio no sea mayor a hora fin
      if (Convert.ToInt32(f_desde.Subtract(fecha_1).TotalMinutes) > Convert.ToInt32(f_hasta.Subtract(fecha_1).TotalMinutes))
        throw NomadException.NewMessage("Personal.NOVEDAD.ERR-HORA");

      //Validar que el dia no este bloqueado
      if (LIQUIDACIONPERS.EnLiquidacionCerrada(oi_personal_emp, fecha_1))
        throw NomadException.NewMessage("Personal.NOVEDAD.ERR-DIA-BLOQ");

      //Recuperar Legajo
      PERSONAL_EMP ddoPerEmp = PERSONAL_EMP.Get(oi_personal_emp,false);

      //Validar solapamiento con Licencias
      DateTime f_fin_real;
      foreach (LICENCIA_PER ddoLicPer in ddoPerEmp.LICEN_PER)
      {
        if (ddoLicPer.f_interrupcionNull)
          f_fin_real = ddoLicPer.f_fin;
        else
          f_fin_real = ddoLicPer.f_interrupcion;

        if (fecha_1 >= ddoLicPer.f_inicio && fecha_1 <= f_fin_real)
          throw NomadException.NewMessage("Personal.NOVEDAD.ERR-SOLAP-LIC");
      }

      //Validar solapamiento con otras Novedades
      foreach (NOVEDAD ddoNovPer in ddoPerEmp.NOVEDADES)
      {
        DateTime novFecDesde = ddoNovPer.f_fecha.AddMinutes(ddoNovPer.e_horainicio);
        DateTime novFecHasta = ddoNovPer.f_fecha.AddMinutes(ddoNovPer.e_horafin);

        if ((f_desde >= novFecDesde && f_desde <  novFecHasta) ||
          (f_hasta >  novFecDesde && f_hasta <= novFecHasta) ||
          (f_desde <= novFecDesde && f_hasta >= novFecHasta))
            throw NomadException.NewMessage("Personal.NOVEDAD.ERR-SOLAP-NOV");
      }

       GrabarNuevaNovedad(ddoPerEmp, ddoNovPersona);
    }

     public static void GrabarNuevaNovedad(PERSONAL_EMP ddoPerEmp, NOVEDAD ddoNovPer)
    {
      
      //Agregar Novedad al Legajo
      ddoPerEmp.NOVEDADES.Add(ddoNovPer);

      try
      {
        //Guardar Legajo
        NomadEnvironment.GetCurrentTransaction().Save(ddoPerEmp);
      }
      catch (Exception ex)
      {
        throw NomadException.NewMessage("Personal.NOVEDAD.ERR-ALTA", ex.Message);
      }
    }

    public static void BajaNovedad(string oi_novedad)
    {
      ValidarBajaNovedad();
      BorrarNovedad(oi_novedad);
    }

    public static void ValidarBajaNovedad()
    {
      //----- DEFINIR VALIDACIONES -----//
    }

    public static void BorrarNovedad(string oi_novedad)
    {
      //Recuperar Novedad
      NOVEDAD ddoNovPer = NOVEDAD.Get(oi_novedad);

      //Recuperar Legajo
      PERSONAL_EMP ddoPerEmp = PERSONAL_EMP.Get(ddoNovPer.oi_personal_emp);

      //Quitar Novedad del Legajo
      ddoPerEmp.NOVEDADES.RemoveById(oi_novedad);

      //Guardar Legajo
      try
      {
        NomadEnvironment.GetCurrentTransaction().Save(ddoPerEmp);
      }
      catch (Exception ex)
      {
        throw NomadException.NewMessage("Personal.NOVEDAD.ERR-BAJA", ex.Message);
      }
    }

    public static bool ValidarSolapLicencia(string oi_personal_emp, DateTime f_inicio, DateTime f_fin)
    {
      //Recuperar Legajo
            DateTime f_inicio_comparar = f_inicio.AddDays(-7);

            NomadXML xmlNovedades = NomadEnvironment.QueryNomadXML(PERSONAL_EMP.Resources.QRY_Novedades, "<DATA oi_personal_emp='"+oi_personal_emp+"' f_inicio='" + f_inicio_comparar.ToString("yyyyMMdd") + "' />").FirstChild();

            for (NomadXML nov = xmlNovedades.FirstChild(); nov != null; nov = nov.Next())
            {
                DateTime fecNovIni = nov.GetAttrDateTime("f_fecha").AddMinutes(nov.GetAttrInt("e_hora_inicio"));
                DateTime fecNovPer = ESPERANZAPER.GetDateHope(oi_personal_emp, fecNovIni);

                if (f_inicio <= fecNovPer && f_fin >= fecNovPer)
                    throw NomadException.NewMessage("Personal.NOVEDAD.ERR-LICENCIA");
            }

      return true;
    }
    }
}

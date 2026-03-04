using System;
using System.Xml;
using System.IO;
using System.Collections;

using Nomad.NSystem.Document;
using Nomad.NSystem.Proxy;
using Nomad.NSystem.Base;
using Nomad.NSystem.Functions;

namespace NucleusRH.Base.MedicinaLaboral.LegajoMedicina
{

    //////////////////////////////////////////////////////////////////////////////////
    //Clase Datos de Medicina por Personal
    public partial class PERSONAL_EMP
    {
        public static void RegistrarConsulta(Nomad.NSystem.Proxy.NomadXML param, bool genlic)
        {

            NomadBatch objBatch;
            objBatch = NomadBatch.GetBatch("Iniciando...", "Comprobante de Consulta Medica");
            objBatch.SetPro(0);

            //CARGA EL LEGAJO
            NucleusRH.Base.MedicinaLaboral.LegajoMedicina.PERSONAL_EMP ddoPER;
            ddoPER = NucleusRH.Base.MedicinaLaboral.LegajoMedicina.PERSONAL_EMP.Get(param.FirstChild().GetAttr("oi_personal_emp"));

            string oiLicPER = "0";
            if (genlic)
            {
                objBatch.SetMess("Generando Licencia por Enfermedad...");
                if ((param.FirstChild().GetAttr("f_prev_alta") != "" || param.FirstChild().GetAttr("f_alta") != "") && param.FirstChild().GetAttr("f_baja") != "")
                {
                    //Obtengo el Parametro de CODIGO de Enfermedad
                    string codLic = NomadEnvironment.QueryValue("ORG26_PARAMETROS", "d_valor", "c_parametro", "CODMED", "ORG26_PARAMETROS.c_modulo=\\'MED\\'", false);
                    if (codLic == "")
                        throw new NomadAppException("No esta definido el codigo de Licencia por Enfermedad.");

                    //Obtengo el Parametro de CODIGO de Enfermedad
                    string oiLic = NomadEnvironment.QueryValue("PER16_LICENCIAS", "oi_licencia", "c_licencia", codLic, "", false);
                    if (oiLic == "")
                        throw new NomadAppException("No esta definido el codigo de Licencia por Enfermedad.");

          //Nueva Licencia
          NucleusRH.Base.Personal.LegajoEmpresa.LICENCIA_PER ddoLicPer = NucleusRH.Base.Personal.LegajoEmpresa.LICENCIA_PER.New();
          ddoLicPer.oi_licencia = oiLic;
          ddoLicPer.f_inicio = param.FirstChild().GetAttrDateTime("f_baja");
          if (param.FirstChild().GetAttr("f_prev_alta") != "")
                        ddoLicPer.f_fin = param.FirstChild().GetAttrDateTime("f_prev_alta").AddDays(-1);
                    if (param.FirstChild().GetAttr("f_alta") != "")
                        ddoLicPer.f_fin = param.FirstChild().GetAttrDateTime("f_alta").AddDays(-1);
          ddoLicPer.e_cant_dias = ddoLicPer.f_fin.Subtract(ddoLicPer.f_inicio).Days + 1;
          ddoLicPer.e_anio_corresp = ddoLicPer.f_inicio.Year;
          ddoLicPer.l_bloqueada = false;
          ddoLicPer.l_interfaz = false;
          ddoLicPer.l_habiles = false;
          if (param.FirstChild().GetAttr("o_consulta_per") != "")
                        ddoLicPer.o_licencia_per = param.FirstChild().GetAttr("o_consulta_per");

          //Validar Licencia
          try
          {
            NucleusRH.Base.Personal.LegajoEmpresa.LICENCIA_PER.ValidarAltaLicencia
              (ddoPER.Id, oiLic, ddoLicPer.f_inicio, ddoLicPer.f_fin, ddoLicPer.e_cant_dias, ddoLicPer.e_anio_corresp);
          }
          catch (Exception e)
          {
            switch (e.Message)
            {
              case "LegajoEmpresa.LICENCIA_PER.ERR-FECHA":
                objBatch.Err("La fecha de fin debe ser mayor o igual a la fecha de inicio");
                return;
              case "LegajoEmpresa.LICENCIA_PER.ERR-SOLAP":
                objBatch.Err("Existe un solapamiento de fechas con otras Licencias cargadas para el Legajo");
                return;
              case "Personal.NOVEDAD.ERR-LICENCIA":
                objBatch.Err("Existe un solapamiento de fechas con Novedades cargadas para el Legajo");
                return;
              case "LegajoEmpresa.LICENCIA_PER.ERR-MIN-ANTIG":
                objBatch.Err("El legajo tiene una antigüedad inferior al mínimo requerido");
                return;
              case "LegajoEmpresa.LICENCIA_PER.ERR-MAX-DIAS":
                objBatch.Err("La cantidad de días consecutivos supera el máximo");
                return;
              case "LegajoEmpresa.LICENCIA_PER.ERR-MAX-ANIO":
                objBatch.Err("La cantidad de días supera el saldo anual restante");
                return;
              default:
                objBatch.Err("Error desconocido");
                return;
            }
          }

          //Agregar Licencia
          ddoPER.LICEN_PER.Add(ddoLicPer);

          //Guardar Legajo
          NomadEnvironment.GetCurrentTransaction().SaveRefresh(ddoPER);

                    //Consulto el id de la licencia cargada en el legajo
          NomadXML xmlParam = new NomadXML("PARAM");
          xmlParam.SetAttr("oi_personal_emp", ddoPER.Id);
          xmlParam.SetAttr("f_inicio", ddoLicPer.f_inicio);
                    NomadXML xmlqry = NomadEnvironment.QueryNomadXML(PERSONAL_EMP.Resources.QRY_LICEN_PER, xmlParam.ToString());
                    oiLicPER = xmlqry.FirstChild().GetAttr("oi_licencia_per");
                    NomadEnvironment.GetTrace().Info("oiLicPER -- " + oiLicPER);
                }
            }

            objBatch.SetPro(40);
            objBatch.SetMess("Actualizando historia clínica del legajo...");

            NucleusRH.Base.MedicinaLaboral.LegajoMedicina.CONSULTA_PER ddoCONS;
            NucleusRH.Base.MedicinaLaboral.LegajoMedicina.DETALLE_CONS ddoVIS;
            ddoCONS = new NucleusRH.Base.MedicinaLaboral.LegajoMedicina.CONSULTA_PER();
            ddoVIS = new NucleusRH.Base.MedicinaLaboral.LegajoMedicina.DETALLE_CONS();

            if (param.FirstChild().GetAttr("e_dias_perdidos") != "")
                ddoCONS.e_dias_perdidos = param.FirstChild().GetAttrInt("e_dias_perdidos");

            if (param.FirstChild().GetAttr("f_alta") != "")
                ddoCONS.f_alta = param.FirstChild().GetAttrDateTime("f_alta");

            if (param.FirstChild().GetAttr("f_baja") != "")
                ddoCONS.f_baja = param.FirstChild().GetAttrDateTime("f_baja");

            if (param.FirstChild().GetAttr("f_fechahora_cons") != "")
                ddoCONS.f_fechahora_cons = param.FirstChild().GetAttrDateTime("f_fechahora_cons");

            if (param.FirstChild().GetAttr("f_prev_alta") != "")
                ddoCONS.f_prev_alta = param.FirstChild().GetAttrDateTime("f_prev_alta");

            if (param.FirstChild().GetAttr("f_prox_consulta") != "")
                ddoCONS.f_prox_consulta = param.FirstChild().GetAttrDateTime("f_prox_consulta");

            if (param.FirstChild().GetAttr("o_consulta_per") != "")
                ddoCONS.o_consulta_per = param.FirstChild().GetAttr("o_consulta_per");

            if (param.FirstChild().GetAttr("oi_enfermedad") != "")
                ddoCONS.oi_enfermedad = param.FirstChild().GetAttr("oi_enfermedad");

            if (param.FirstChild().GetAttr("oi_motivo_consulta") != "")
                ddoCONS.oi_motivo_consulta = param.FirstChild().GetAttr("oi_motivo_consulta");

            if (param.FirstChild().GetAttr("d_domicilio") != "")
                ddoVIS.d_domicilio = param.FirstChild().GetAttr("d_domicilio");

            if (param.FirstChild().GetAttr("f_fechahora_visita") != "")
            {
                ddoVIS.f_fechahora_visita = param.FirstChild().GetAttrDateTime("f_fechahora_visita");

                if (param.FirstChild().GetAttr("o_detalle_cons") != "")
                    ddoVIS.o_detalle_cons = param.FirstChild().GetAttr("o_detalle_cons");

                if (param.FirstChild().GetAttr("o_medicacion") != "")
                    ddoVIS.o_medicacion = param.FirstChild().GetAttr("o_medicacion");

                if (param.FirstChild().GetAttr("oi_lugar_atencion") != "")
                    ddoVIS.oi_lugar_atencion = param.FirstChild().GetAttr("oi_lugar_atencion");

                if (param.FirstChild().GetAttr("oi_medico") != "")
                    ddoVIS.oi_medico = param.FirstChild().GetAttr("oi_medico");

                if (param.FirstChild().GetAttr("oi_tipo_domicilio") != "")
                    ddoVIS.oi_tipo_domicilio = param.FirstChild().GetAttr("oi_tipo_domicilio");

                ddoCONS.DETALLE_HIST.Add(ddoVIS);
            }

            //Cargar los atributos necesarios para saber si se envia el mail por la alerta
			ddoCONS.l_envio_mail = param.FirstChild().GetAttr("l_envio_mail") == "1";
			ddoCONS.f_creacion = DateTime.Now;

            objBatch.SetPro(60);

            if (oiLicPER != "0")
                ddoCONS.oi_licencia = int.Parse(oiLicPER);
            ddoPER.HIST_CLINICA.Add(ddoCONS);

            NomadEnvironment.GetCurrentTransaction().Save(ddoPER);
            objBatch.SetPro(85);

            //GENERA EL COMPROBANTE
			Nomad.Base.Report.GeneralReport.REPORT.ExecReportHTML("NucleusRH.Base.MedicinaLaboral.Comprobante.rpt", param);			
			objBatch.SetPro(100);
        }

    public void InformarAlta(string consultaid, string licenciaid, DateTime f_alta)
        {

            NucleusRH.Base.MedicinaLaboral.LegajoMedicina.CONSULTA_PER ddoCON = (NucleusRH.Base.MedicinaLaboral.LegajoMedicina.CONSULTA_PER)this.HIST_CLINICA.GetById(consultaid);

            NomadEnvironment.GetTrace().Info(ddoCON.SerializeAll());
            NucleusRH.Base.Personal.LegajoEmpresa.LICENCIA_PER ddoLIC = new NucleusRH.Base.Personal.LegajoEmpresa.LICENCIA_PER();

            ddoCON.f_alta = f_alta;
            if (licenciaid != "" && licenciaid != "0")
            {
                ddoLIC = (NucleusRH.Base.Personal.LegajoEmpresa.LICENCIA_PER)this.LICEN_PER.GetById(licenciaid);
                NomadEnvironment.GetTrace().Info(ddoLIC.SerializeAll());
                ddoLIC.f_fin = f_alta.AddDays(-1);

                TimeSpan a = new TimeSpan();
                a = ddoLIC.f_fin.Subtract(ddoLIC.f_inicio);
                ddoLIC.e_cant_dias = Convert.ToInt32(a.TotalDays);
            }

            NomadEnvironment.GetCurrentTransaction().Save(this);

        }

    //Actualizo la observacion de la licencia con la observacion de la consulta
    public void ActualizarEnfermedad(Nomad.NSystem.Proxy.NomadXML LICDELS)
        {

            //modifico las licencias de las enfermedades modificadas
            foreach (CONSULTA_PER ENF in this.HIST_CLINICA)
            {
                if (!ENF.IsForUpdate)
                    continue;
                if (ENF.oi_licenciaNull)
                    continue;
                NomadEnvironment.GetTrace().Info("ENF UPDATE -- " + ENF.SerializeAll());
                NucleusRH.Base.Personal.LegajoEmpresa.LICENCIA_PER LIC = (NucleusRH.Base.Personal.LegajoEmpresa.LICENCIA_PER)this.LICEN_PER.GetById(ENF.oi_licencia.ToString());
                if (LIC != null)
                {
                    LIC.f_inicio = ENF.f_baja;
                    if (ENF.f_altaNull)
                        LIC.f_fin = ENF.f_prev_alta.AddDays(-1);
                    else
                        LIC.f_fin = ENF.f_alta.AddDays(-1);
                    LIC.e_cant_dias = ENF.e_dias_perdidos;
                    LIC.o_licencia_per = ENF.o_consulta_per;
                }
            }

            //elimino las licencias de las enfermedades eliminadas
            for (NomadXML lic = LICDELS.FirstChild().FirstChild(); lic != null; lic = lic.Next())
            {
                NucleusRH.Base.Personal.LegajoEmpresa.LICENCIA_PER LIC = (NucleusRH.Base.Personal.LegajoEmpresa.LICENCIA_PER)this.LICEN_PER.GetById(lic.GetAttr("id"));
                if (LIC != null)
                    this.LICEN_PER.Remove(LIC);
            }
            NomadEnvironment.GetCurrentTransaction().Save(this);

        }

    }
}



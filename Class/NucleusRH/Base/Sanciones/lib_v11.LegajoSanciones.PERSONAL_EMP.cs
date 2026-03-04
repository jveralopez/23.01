using System;
using System.Xml;
using System.IO;
using System.Collections;

using Nomad.NSystem.Document;
using Nomad.NSystem.Proxy;
using Nomad.NSystem.Base;
using Nomad.NSystem.Functions;

namespace NucleusRH.Base.Sanciones.LegajoSanciones
{

    //////////////////////////////////////////////////////////////////////////////////
    //Clase Datos de Personal para Sanciones
    public partial class PERSONAL_EMP 
    {
        public void AceptarSancion(string idsancion)
        {
            //Obtengo el Parametro de CODIGO de Suspension
            string codLic = NomadEnvironment.QueryValue("org26_parametros", "d_valor", "c_parametro", "CODSAN", "org26_parametros.c_modulo=\\'SAN\\'", false);
            if (codLic == "")
                throw new NomadAppException("No esta definido el codigo de Licencia por Suspension.");

            //Obtengo el Parametro de CODIGO de Suspension
            string oiLic = NomadEnvironment.QueryValue("per16_licencias", "oi_licencia", "c_licencia", codLic, "", false);
            if (oiLic == "")
                throw new NomadAppException("No esta definido el codigo de Licencia por Suspension.");

            SANCION_PER ddoSP = (SANCION_PER)this.SANCION_PER.GetById(idsancion);

            ddoSP.c_estado = "A";
            ddoSP.f_estado = DateTime.Now;

            //Guardo el cambio de Estado de la Licencia
            NomadEnvironment.GetCurrentTransaction().SaveRefresh(this);

        }

        public void InterrumpirSuspension(string idsancion, string idsuspension)
        {

            SANCION_PER ddoSAN = (SANCION_PER)this.SANCION_PER.GetById(idsancion);
            SUSPENSION_PER ddoSP = (SUSPENSION_PER)ddoSAN.SUSP_PER.GetById(idsuspension);

            //consulto el id de la licencia cargada en el legajo
            NomadXML xmlqry = NomadEnvironment.QueryNomadXML(PERSONAL_EMP.Resources.QRY_LICEN_PER, "<PARAM oi_personal_emp=\"" + this.Id + "\" f_inicio=\"" + ddoSP.f_fechahora_ini.ToString("yyyyMMdd") + "\"/>");
            string oiLicPER = xmlqry.FirstChild().GetAttr("oi_licencia_per");
            NomadEnvironment.GetTrace().Info("oiLicPER -- " + oiLicPER);

            if (oiLicPER != "0")
            {
                NucleusRH.Base.Personal.LegajoEmpresa.LICENCIA_PER ddoLIC = (NucleusRH.Base.Personal.LegajoEmpresa.LICENCIA_PER)this.LICEN_PER.GetById(oiLicPER);
                ddoLIC.f_interrupcion = ddoSP.f_fechahora_int;
                TimeSpan a = new TimeSpan();
                a = ddoLIC.f_interrupcion.Subtract(ddoLIC.f_inicio);
                ddoLIC.e_cant_dias = Convert.ToInt32(a.TotalDays);
            }

            NomadEnvironment.GetCurrentTransaction().Save(this);

        }
		
        public static void RegistrarSancion(NucleusRH.Base.Sanciones.LegajoSanciones.PERSONAL_EMP DDOPER, NucleusRH.Base.Sanciones.LegajoSanciones.SANCION_PER DDOSAN, bool genlic)
        {
            NomadXML xmlParam = new NomadXML("PARAM");
			xmlParam.SetAttr("oi_personal_emp", DDOPER.Id);
			xmlParam.SetAttr("f_sancion", DDOSAN.f_fechahora_sanc);
			NomadXML xmlSan = NomadEnvironment.QueryNomadXML(NucleusRH.Base.Sanciones.LegajoSanciones.SANCION_PER.Resources.QRY_SANCION_PER, xmlParam.ToString());
            if(xmlSan.FirstChild().GetAttr("oi_sancion_per") != "0")
                throw new NomadAppException("Error de violación de clave. AK_SANC_PER");
           
			string oiLicPER = "0";
			
			if (genlic)
			{
				//Obtengo el Parametro de CODIGO de Suspension
				string codLic = NomadEnvironment.QueryValue("ORG26_PARAMETROS", "d_valor", "c_parametro", "CODSAN", "ORG26_PARAMETROS.c_modulo=\\'SAN\\'", false);
				if (codLic == "")
					throw new NomadAppException("No esta definido el codigo de Licencia por Suspension.");

				//Obtengo el Parametro de CODIGO de Suspension
				string oiLic = NomadEnvironment.QueryValue("PER16_LICENCIAS", "oi_licencia", "c_licencia", codLic, "", false);
				if (oiLic == "")
					throw new NomadAppException("No esta definido el codigo de Licencia por Suspension.");

				NucleusRH.Base.Personal.LegajoEmpresa.LICENCIA_PER ddoLicPer;
				
				foreach (SUSPENSION_PER ddoSUSP in DDOSAN.SUSP_PER)
				{
					//Nueva Licencia
					ddoLicPer = NucleusRH.Base.Personal.LegajoEmpresa.LICENCIA_PER.New();
					ddoLicPer.oi_licencia = oiLic;
					ddoLicPer.f_inicio = ddoSUSP.f_fechahora_ini;					
					ddoLicPer.f_fin = ddoSUSP.f_fechahora_fin;					
					ddoLicPer.e_cant_dias = ddoLicPer.f_fin.Subtract(ddoLicPer.f_inicio).Days + 1;
					ddoLicPer.e_anio_corresp = ddoLicPer.f_inicio.Year;
					ddoLicPer.l_bloqueada = false;
					ddoLicPer.l_interfaz = false;
					ddoLicPer.l_habiles = false;

					//Validar Licencia
					NucleusRH.Base.Personal.LegajoEmpresa.LICENCIA_PER.ValidarAltaLicencia
						(DDOPER.Id, oiLic, ddoLicPer.f_inicio, ddoLicPer.f_fin, ddoLicPer.e_cant_dias, ddoLicPer.e_anio_corresp);
					
					//Agregar Licencia
					DDOPER.LICEN_PER.Add(ddoLicPer);
				}
				
				//Guardar Legajo
				NomadEnvironment.GetCurrentTransaction().SaveRefresh(DDOPER);
				
				//Por cada suspensión asigno id de licencia asociada
				foreach (SUSPENSION_PER ddoSUSP in DDOSAN.SUSP_PER)
				{
					//Consulto el id de la licencia cargada en el legajo
					xmlParam = new NomadXML("PARAM");
					xmlParam.SetAttr("oi_personal_emp", DDOPER.Id);
					xmlParam.SetAttr("f_inicio", ddoSUSP.f_fechahora_ini);
					NomadXML xmlqry = NomadEnvironment.QueryNomadXML(PERSONAL_EMP.Resources.QRY_LICEN_PER, xmlParam.ToString());
					oiLicPER = xmlqry.FirstChild().GetAttr("oi_licencia_per");

					if (oiLicPER != "0")
						ddoSUSP.oi_licencia = int.Parse(oiLicPER);
				}

			}

			DDOPER.SANCION_PER.Add(DDOSAN);
			NomadEnvironment.GetCurrentTransaction().Save(DDOPER);
        }
		
        public void AnularSancion(string idsancion)
        {

            SANCION_PER ddoSP = (SANCION_PER)this.SANCION_PER.GetById(idsancion);

            foreach (SUSPENSION_PER ddoSUSP in ddoSP.SUSP_PER)
            {
                NucleusRH.Base.Personal.LegajoEmpresa.LICENCIA_PER LIC = (NucleusRH.Base.Personal.LegajoEmpresa.LICENCIA_PER)this.LICEN_PER.GetById(ddoSUSP.oi_licencia.ToString());
                if (LIC != null)
                    this.LICEN_PER.Remove(LIC);

                ddoSUSP.oi_licenciaNull = true;
            }
            ddoSP.c_estado = "L";
            ddoSP.f_estado = DateTime.Now;

            NomadEnvironment.GetCurrentTransaction().SaveRefresh(this);

        }
    }
}

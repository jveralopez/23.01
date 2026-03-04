using System;
using System.Xml;
using System.IO;
using System.Collections;
using System.Collections.Generic;

using Nomad.NSystem.Document;
using Nomad.NSystem.Proxy;
using Nomad.NSystem.Base;
using Nomad.NSystem.Functions;

using NucleusRH.Base.Personal.Licencias;
using NucleusRH.Base.Organizacion.Convenios;

namespace NucleusRH.Base.Personal.LegajoEmpresa
{
    /////////////////////////////
    //Clase Licencia del Personal
    public partial class LICENCIA_PER
    {	
		////////////////////////////////////////////////////////////////////////////
		public static void AltaLicencia(string oi_personal_emp, LICENCIA_PER objLIC)
		{			
			ValidarAltaLicencia(oi_personal_emp, objLIC.oi_licencia, objLIC.f_inicio, objLIC.f_fin, objLIC.e_cant_dias, objLIC.e_anio_corresp);
			GrabarNuevaLicencia(oi_personal_emp, objLIC);
		}
		
		//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		public static void ValidarAltaLicencia(string oi_personal_emp, string oi_licencia, DateTime f_ini, DateTime f_fin, int e_dias, int e_anio)
		{
			//Recuperar Legajo
			PERSONAL_EMP objPER = PERSONAL_EMP.Get(oi_personal_emp);
			
                        if (objPER != null)
                        {

			        ValidacionesIniciales(f_ini, f_fin);			

			        ValidarRestricciones(objPER, oi_licencia, e_dias, e_anio, false);

			        ValidacionesExternas(oi_personal_emp, f_ini, f_fin);

			        ValidarSolapamientos(objPER, f_ini, f_fin);
                        }
                        else
                        {
				throw NomadException.NewMessage("LegajoEmpresa.LICENCIA_PER.ERR-LEG-NOT-FOUND");			   
                        }


		}

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public static void ValidarModLicencia(PERSONAL_EMP objPER, string oi_licencia, DateTime f_ini, DateTime f_fin, int e_dias, int e_anio, List<string> LicModificar)
        {
            ValidacionesIniciales(f_ini, f_fin);
            ValidarRestricciones(objPER, oi_licencia, e_dias, e_anio, false);
            ValidacionesExternas(objPER.id.ToString(), f_ini, f_fin);
            ValidarSolapamientosConExc(objPER, f_ini, f_fin, LicModificar);
        }
		
		///////////////////////////////////////////////////////////////////////////////////////////////////
		public static void ValidarAltaLicenciaSec(string oi_personal_emp, LICENCIA_PER objLIC, bool forzar)
		{	
			//Recuperar Legajo
			PERSONAL_EMP objPER = PERSONAL_EMP.Get(oi_personal_emp);
		
			ValidacionesIniciales(objLIC.f_inicio, objLIC.f_fin);
			ValidacionesExternas(oi_personal_emp, objLIC.f_inicio, objLIC.f_fin);
			ValidarSolapamientos(objPER, objLIC.f_inicio, objLIC.f_fin);
			ValidarRestricciones(objPER, objLIC.oi_licencia, objLIC.e_cant_dias, objLIC.e_anio_corresp, forzar);
		}
		
		/////////////////////////////////////////////////////////////////////////
		private static void ValidacionesIniciales(DateTime f_ini, DateTime f_fin)
		{
			if (f_fin < f_ini)
				throw NomadException.NewMessage("LegajoEmpresa.LICENCIA_PER.ERR-FECHA");
		}
		
		//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		private static void ValidarRestricciones(PERSONAL_EMP objPER, string oi_licencia, int e_dias, int e_anio, bool forzar)
		{
			bool segunConvenio = false;			
			int e_max_dias_lic = 0;
			int e_max_dias_anio = 0;
			int e_min_meses_antig = 0;
			bool tieneMaxDias = false;
			bool tieneMaxAnio = false;
			bool tieneMinAntig = false;
			bool validMaxDias = true;
            bool validMaxAnio = true;
            bool validMinAntig = true;
			
			//Recuperar (Tipo de) Licencia
			LICENCIA ddoLicencia = LICENCIA.Get(oi_licencia);

			//Nuevo (Tipo de) Licencia segun Convenio
			LICENCIAS_CONV ddoLicConv = new LICENCIAS_CONV();	

			//Si el legajo tiene Categoria
			if (!objPER.oi_categoria_ultNull)
			{
				//Obtener OI del Convenio de la Categoria
				string oi_convenio = CATEGORIA.Get(objPER.oi_categoria_ult).oi_convenio.ToString();

				//Si existe, recuperar (Tipo de) Licencia segun Convenio
				foreach (LICENCIAS_CONV licConv in ddoLicencia.LIC_CONV)
				{
					if (licConv.oi_convenio == oi_convenio)
					{
						ddoLicConv = licConv;
						segunConvenio = true;
						break;
					}
				}
			}
			
			//Validar restricciones de (Tipo de) Licencia segun Convenio
			if (segunConvenio)
			{
				if (!ddoLicConv.e_max_dias_licNull)
				{
					tieneMaxDias = true;
					e_max_dias_lic = ddoLicConv.e_max_dias_lic;
				}
				if (!ddoLicConv.e_max_dias_anioNull)
				{
					tieneMaxAnio = true;
					e_max_dias_anio = ddoLicConv.e_max_dias_anio;
				}
				if (!ddoLicConv.e_min_meses_antigNull)
				{
					tieneMinAntig = true;
					e_min_meses_antig = ddoLicConv.e_min_meses_antig;
				}
			}
			//Validar restricciones de (Tipo de) Licencia
			else
			{
				if (!ddoLicencia.e_max_dias_licNull)
				{
					tieneMaxDias = true;
					e_max_dias_lic = ddoLicencia.e_max_dias_lic;
				}
				if (!ddoLicencia.e_max_dias_anioNull)
				{
					tieneMaxAnio = true;
					e_max_dias_anio = ddoLicencia.e_max_dias_anio;
				}
				if (!ddoLicencia.e_min_meses_antigNull)
				{
					tieneMinAntig = true;
					e_min_meses_antig = ddoLicencia.e_min_meses_antig;
				}
			}
			
			//Existe restriccion de minimo de meses de antigüedad
			if (tieneMinAntig)
			{
				int antigLegajo = objPER.Antiguedad_Meses(DateTime.Today);

				if (antigLegajo < e_min_meses_antig)
					validMinAntig = false;
			}
			
			//Existe restriccion de maximo de dias consecutivos
			if (tieneMaxDias)
			{
				if (e_dias > e_max_dias_lic)
					validMaxDias = false;
			}

			//Existe restriccion de maximo de dias en el año
			int saldo = e_max_dias_anio;
			if (tieneMaxAnio)
			{
				foreach (LICENCIA_PER objLIC in objPER.LICEN_PER)
				{
					if (objLIC.oi_licencia == oi_licencia && objLIC.e_anio_corresp == e_anio)
						saldo = saldo - objLIC.e_cant_dias;
				}
				
				if (e_dias > saldo)
				{
					validMaxAnio = false;
					if (saldo < 0) saldo = 0;
				}
			}

			//No tiene el minimo de meses de antigüedad
			if (!validMinAntig)
			{
				if (forzar && !ddoLicencia.l_min_meses_antig || !forzar)
					throw NomadException.NewMessage("LegajoEmpresa.LICENCIA_PER.ERR-MIN-ANTIG", e_min_meses_antig.ToString());
			}

			//Supera el maximo de dias consecutivos
			if (!validMaxDias)
			{
				if (forzar && !ddoLicencia.l_max_dias_lic || !forzar)
					throw NomadException.NewMessage("LegajoEmpresa.LICENCIA_PER.ERR-MAX-DIAS", e_max_dias_lic.ToString());
			}

			//Supera el maximo de dias en el año
			if (!validMaxAnio)
			{	
				if (forzar && !ddoLicencia.l_max_dias_anio || !forzar)
					throw NomadException.NewMessage("LegajoEmpresa.LICENCIA_PER.ERR-MAX-ANIO", saldo.ToString());
			}
		}

		////////////////////////////////////////////////////////////////////////////////////////////////
		private static void ValidacionesExternas(string oi_personal_emp, DateTime f_ini, DateTime f_fin)
		{
			try
            {
                eveValidarAltaExterno(oi_personal_emp, f_ini, f_fin);
            }
            catch (System.Reflection.TargetInvocationException sre)
            {
                throw sre.InnerException;
            }
            catch(Exception e)
            {
                throw;
            }
		}
		
		/////////////////////////////////////////////////////////////////////////////////////////////
		private static void ValidarSolapamientos(PERSONAL_EMP objPER, DateTime f_ini, DateTime f_fin)
		{
            ValidarSolapamientosConExc(objPER, f_ini, f_fin, null); //Para reutilizar los codigos
            /*
            DateTime f_fin_real;			
			foreach (LICENCIA_PER objLIC in objPER.LICEN_PER)
			{
				if (objLIC.f_interrupcionNull)
					f_fin_real = objLIC.f_fin;
				else
					f_fin_real = objLIC.f_interrupcion;
				
				if ((f_ini >= objLIC.f_inicio && f_ini <= f_fin_real) ||
					(f_fin >= objLIC.f_inicio && f_fin <= f_fin_real) ||
					(f_ini <= objLIC.f_inicio && f_fin >= f_fin_real))
						throw NomadException.NewMessage("LegajoEmpresa.LICENCIA_PER.ERR-SOLAP");
			}
           */
		}

        /////////////////////////////////////////////////////////////////////////////////////////////
        private static void ValidarSolapamientosConExc(PERSONAL_EMP objPER, DateTime f_ini, DateTime f_fin, List<string> LicModificar)
        {
            DateTime f_fin_real;
            foreach (LICENCIA_PER objLIC in objPER.LICEN_PER)
            {
                if (objLIC.f_interrupcionNull)
                    f_fin_real = objLIC.f_fin;
                else
                    f_fin_real = objLIC.f_interrupcion;

                //Si el oi_licencia_per recorrido es uno de los a excluir (modificar) hace un continue;                
                if (LicModificar != null && LicModificar.Contains(objLIC.id.ToString())) continue;                

                if ((f_ini >= objLIC.f_inicio && f_ini <= f_fin_real) ||
                    (f_fin >= objLIC.f_inicio && f_fin <= f_fin_real) ||
                    (f_ini <= objLIC.f_inicio && f_fin >= f_fin_real))
					{
						//string exMsg = "Datos del Solapamiento [DatosRH: legajo=" + objPER.e_numero_legajo + " oi_licencia=" + objLIC.oi_licencia.ToString() + " fini=" + objLIC.f_inicio + " ffin=" + f_fin_real + "] <-> [DatosDOC: fini: " + f_ini + " ffin : " + f_fin + "]";
						
						throw NomadException.NewMessage("LegajoEmpresa.LICENCIA_PER.ERR-SOLAP");
					}
            }
        }


		///////////////////////////////////////////////////////////////////////////////////
		public static void GrabarNuevaLicencia(string oi_personal_emp, LICENCIA_PER objLIC)
		{
			//Recuperar Legajo
			PERSONAL_EMP objPER = PERSONAL_EMP.Get(oi_personal_emp);
			
			//Agregar Licencia al Legajo
			objPER.LICEN_PER.Add(objLIC);
			
			try
			{
				//Guardar Legajo
				NomadEnvironment.GetCurrentTransaction().Save(objPER);
            }
            catch (Exception ex)
			{
				throw NomadException.NewMessage("LegajoEmpresa.LICENCIA_PER.ERR-ALTA", ex.Message);
			}			
		}

		///////////////////////////////////////////////////////
		public static void BajaLicencia(string oi_licencia_per)
		{
			ValidarBajaLicencia(oi_licencia_per);
			BorrarLicencia(oi_licencia_per);			
		}

		//////////////////////////////////////////////////////////////
		public static void ValidarBajaLicencia(string oi_licencia_per)
		{
			//----- DEFINIR VALIDACIONES -----//
			
			//Validaciones externas
			//eveValidarBajaExterno();
		}

		/////////////////////////////////////////////////////////
		public static void BorrarLicencia(string oi_licencia_per)
		{
			//Recuperar Licencia
			LICENCIA_PER objLIC = LICENCIA_PER.Get(oi_licencia_per);
			
			//Recuperar Legajo
			PERSONAL_EMP objPER = PERSONAL_EMP.Get(objLIC.oi_personal_emp);
			
			//Quitar Licencia del Legajo
			objPER.LICEN_PER.RemoveById(oi_licencia_per);

			try
			{
				//Guardar Legajo
				NomadEnvironment.GetCurrentTransaction().Save(objPER);
            }
            catch (Exception ex)
			{
				throw NomadException.NewMessage("LegajoEmpresa.LICENCIA_PER.ERR-BAJA", ex.Message);
			}			
		}
    }
}
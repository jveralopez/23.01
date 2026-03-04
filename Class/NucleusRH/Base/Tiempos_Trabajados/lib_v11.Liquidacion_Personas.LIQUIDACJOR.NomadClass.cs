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

    public partial class LIQUIDACJOR : Nomad.NSystem.Base.NomadObject
    {

        public Liquidacion.LIQUIDACION myliq = null;
		
		private string strOIPersonalEmp = "";

        public string OIPersonalEmp
        {
            get { return this.strOIPersonalEmp; }
            set { this.strOIPersonalEmp = value; }
        }

        /// <summary>
        /// Retorna el objeto esperanza del dia de procesamiento
        /// </summary>
		public NucleusRH.Base.Tiempos_Trabajados.Esperanzaper.DIA Esperanza
        {
            get
            { 
				return this.GetEsperanza(this.f_fecjornada);
            }
        }
		
        /// <summary>
        /// Retorna el objeto esperanza del dia solicitado
        /// </summary>		
		private NucleusRH.Base.Tiempos_Trabajados.Esperanzaper.DIA GetEsperanza(DateTime pdteDia) {
			//Valida que el atributo this.strOIPersonalEmp esté cargado
			if (this.strOIPersonalEmp == "")
				throw new Exception("El atributo OI_Peronal_emp no está seteado.");

			return NucleusRH.Base.Tiempos_Trabajados.Esperanzaper.ESPERANZAPER.GetDayHope(pdteDia, int.Parse(this.strOIPersonalEmp));
        }
		
        public DateTime Day
        {
            get
            {
                return f_fecjornada;
            }
        }

        public int iDay
        {
            get
            {
                return f_fecjornada.Year * 10000 + f_fecjornada.Month * 100 + f_fecjornada.Day;
            }
        }

        public DayOfWeek DayOfWeek
        {
            get
            {
                return Day.DayOfWeek;
            }
        }

        /// <summary>
        /// Retorna el código del horario
        /// </summary>        
		public string Horario {
            get {	
				string Result = "";
				
				//No se pudo recuperar el horario
				if (oi_horario == null) throw new Exception("No se pudo recuperar el horario para el legajo. Verifique que el horario no sea NULL.");
				
				Result = NomadEnvironment.QueryValue("TTA02_HORARIOS", "c_horario", "oi_horario", oi_horario.ToString(), "", true);
				
				if (Result == "") throw new Exception("No se pudo recuperar el horario para el legajo. Verifique que el horario '" + oi_horario + "' exista.");
				
				return Result;
            }
        }
        
        /// <summary>
        /// Retorna si el horario actual se corresponde con los indicados. Los valores deben estar separados por punto y coma (;).
        /// </summary>        
		public bool EsHorario(string pHorarios) {
			string Horarios = ";" + pHorarios.ToUpper() + ";";
			return Horarios.IndexOf(";" + Horario.ToUpper() + ";") >= 0;
		}

		
        /// <summary>
        /// Retorna el código del turno
        /// </summary>
        public string Turno {
            get {
                //return NucleusRH.Base.Tiempos_Trabajados.Turnos.TURNO.GetById(oi_turno.ToString()).c_turno;
				string Result = "";
				
				//No se pudo recuperar el turno
				if (oi_turno == null) throw new Exception("No se pudo recuperar el turno para el legajo. Verifique que el turno no sea NULL.");
				
				Result = NomadEnvironment.QueryValue("TTA14_TURNOS", "c_turno", "oi_turno", oi_turno.ToString(), "", true);
				
				if (Result == "") throw new Exception("No se pudo recuperar el turno para el legajo. Verifique que el turno '" + oi_turno + "' exista.");
				
				return Result;
            }
        }

        /// <summary>
        /// Retorna si el turno actual se corresponde con los indicados. Los valores deben estar separados por punto y coma (;).
        /// </summary>        
        public bool EsTurno(string pTurnos)
        {
            return EsTurno(pTurnos, 0);            
        }

        /// <summary>
        /// Retorna si el turno actual se corresponde con los indicados. Los valores deben estar separados por punto y coma (;).
        /// el parametro de dias es un entero que suma o resta dias desde el dia de procesamiento
        /// </summary> 
        public bool EsTurno(string pTurnos, int pIntDia)
        {
            DateTime dteFecha = this.Day.AddDays(pIntDia);
           
            NucleusRH.Base.Tiempos_Trabajados.Esperanzaper.DIA EsperanzaDia = this.GetEsperanza(dteFecha);

            if(EsperanzaDia != null)
            {
                string c_turno = NomadEnvironment.QueryValue("TTA14_TURNOS", "c_turno", "oi_turno", EsperanzaDia.oi_turno.ToString(), "", true);

                if (c_turno == "")
                    throw new Exception("No se pudo recuperar el turno para el legajo. Verifique que el turno '" + EsperanzaDia.oi_turno.ToString() + "' exista.");

                string Turnos = ";" + pTurnos.ToUpper() + ";";
                return Turnos.IndexOf(";" + c_turno.ToUpper() + ";") >= 0;            
            }
            return false;
        }
		
        public int MinutosEsperados
        {
            get
            {
                return GetMinEsperados((string)null);
            }
        }

        public int MinutosPresente
        {
            get
            {
                int retMin = 0;
                foreach (LIQUIDACIONPROC proc in this.LIQUIDACPROC)
                {
                    NucleusRH.Base.Tiempos_Trabajados.Tipos_Horas.TIPOHORA th = proc.GetTipoHora();
                    if (!th.Presencia) continue;
                    retMin += proc.CantMinutos;
                }
                return retMin;
            }
        }

        public bool Feriado
        {
            get
            {
                return (this.c_tipo == "F" || this.c_tipo == "DF");
            }
        }

		/// <summary>
        /// Indica si el día indicado es feriado
        /// </summary>
		public bool EsFeriado(int pintCantDias) {
            Double dblDays = Convert.ToDouble(pintCantDias);
			DateTime dteFecha = this.Day.AddDays(dblDays);
			bool Resultado = false;
			
            try {
				NucleusRH.Base.Tiempos_Trabajados.Esperanzaper.DIA EsperanzaDia = this.GetEsperanza(dteFecha);
				
				Resultado = (EsperanzaDia.c_tipo == "F" || EsperanzaDia.c_tipo == "DF");
				
			} catch (Exception ex) {
				//Se produjo un error desconicido
				NomadBatch.Trace("Se produjo un error en EsFeriado. '" + ex.Message + "'.");
				Resultado = false;
			}
			
			return Resultado;

        }

        /// <summary>
        /// Indica si el día actual es no laborable
        /// </summary>
        public bool NoLaborable
        {
            get{
                return EsNoLaborable(0);
            }
            
        }

        /// <summary>
        /// Indica si el día indicado es no laborable
        /// </summary>
        public bool EsNoLaborable(int pintCantDias)
        {
            Double dblDays = Convert.ToDouble(pintCantDias);
            DateTime dteFecha = this.Day.AddDays(dblDays);
            bool Resultado = false;

            try
            {
                NucleusRH.Base.Tiempos_Trabajados.Esperanzaper.DIA EsperanzaDia = this.GetEsperanza(dteFecha);

                Resultado = EsperanzaDia.c_tipo == "NL";

            }
            catch (Exception ex)
            {
                //Se produjo un error desconicido
                NomadBatch.Trace("Se produjo un error en EsNoLaborable. '" + ex.Message + "'.");
                Resultado = false;
            }

            return Resultado;

        }


        /// <summary>
        /// Retorna los minutos PROCESADOS de un tipo de hora
        /// </summary>
        public int GetMinutos(string tipohora)
        {
            int retMin = 0;
            foreach (LIQUIDACIONPROC proc in this.LIQUIDACPROC)
            {
                if (proc.TipoHora != tipohora) continue;
                retMin += proc.CantMinutos;
            }
            return retMin;
        }

        /// <summary>
        /// Retorna los minutos PROCESADOS de un Array de tipos de hora
        /// </summary>
        public int GetMinutos(string[] arrTiposHora) {
            int minutosProc = 0;
            
            //Recorre los tipos de hora
            for (int ith = 0; ith < arrTiposHora.Length; ith++)
                minutosProc = minutosProc + GetMinutos(arrTiposHora[ith]);
            
            return minutosProc;
        }
        
        /// <summary>
        /// Retorna las horas PROCESADAS de un tipo de hora
        /// </summary>
        public double GetHoras(string tipohora) { return this.GetMinutos(tipohora) / 60.0; }
       
        /// <summary>
        /// Retorna las horas PROCESADAS de un Array de tipos de hora
        /// </summary>
        public double GetHoras(string[] arrTiposHora) { return this.GetMinutos(arrTiposHora) / 60.0; }
        



        
        
        /// <summary>
        /// Retorna los minutos PROCESADOS de un tipo de hora sobre un tipo de hora ESPERADO
        /// </summary>
        public int GetMinutos(string thEsperado, string thProcesado)
        {
            NucleusRH.Base.Tiempos_Trabajados.Esperanzaper.DETALLE objDet;
            ArrayList arrDetalles;
            int minutosProc = 0;
            int ith;
            
            //Obtiene los DETALLES de un tipo de hora esperado en la jornada
            arrDetalles = GetTHDefinido(thEsperado);
            
            //Recorre los tipos de hora y va pidiendo los minutos a cada uno de sus rangos
            for (ith = 0; ith < arrDetalles.Count; ith++) {
                objDet = (NucleusRH.Base.Tiempos_Trabajados.Esperanzaper.DETALLE)arrDetalles[ith];
                minutosProc = minutosProc + GetMinutosRango(thProcesado, objDet.e_horainicio, objDet.e_horafin);
            }
            
            return minutosProc;
        }
		
        /// <summary>
        /// Retorna los minutos PROCESADOS de un Array de tipos de hora
        /// </summary>
        public int GetMinutos(string[] arrTiposHora, string thProcesado) {
            int minutosProc = 0;
            
            //Recorre los tipos de hora
            for (int ith = 0; ith < arrTiposHora.Length; ith++)
                minutosProc = minutosProc + GetMinutos(arrTiposHora[ith], thProcesado);
            
            return minutosProc;
        }

        /// <summary>
        /// Retorna las horas PROCESADAS de un tipo de hora sobre un tipo de hora ESPERADO
        /// </summary>
        public double GetHoras(string thEsperado, string thProcesado) { return this.GetMinutos(thEsperado, thProcesado) / 60.0; }

        /// <summary>
        /// Retorna las horas PROCESADAS de un tipo de hora sobre un tipo de hora ESPERADO
        /// </summary>
        public double GetHoras(string[] thEsperado, string thProcesado) { return this.GetMinutos(thEsperado, thProcesado) / 60.0; }









        /// <summary>
        /// Retorna los minutos PROCESADOS de un tipo de hora sobre un tipo de hora ESPERADO en un rango
        /// </summary>
        public int GetMinutos(string thEsperado, string thProcesado, int horadesde, int horahasta)
        {
			int mindesde = horadesde / 100 * 60 + horadesde % 100;
			int minhasta = horahasta / 100 * 60 + horahasta % 100;
			if (mindesde > minhasta) minhasta += 1440;
			return GetMinutosRango(thEsperado, thProcesado, mindesde, minhasta) + GetMinutosRango(thEsperado, thProcesado, mindesde - 1440, minhasta - 1440) + GetMinutosRango(thEsperado, thProcesado, mindesde + 1440, minhasta + 1440);

        }        

        /// <summary>
        /// Retorna los minutos PROCESADOS de un Array de tipos de hora
        /// </summary>
        public int GetMinutos(string[] arrTiposHora, string thProcesado, int horadesde, int horahasta) {
            int minutosProc = 0;
            
            //Recorre los tipos de hora
            for (int ith = 0; ith < arrTiposHora.Length; ith++)
                minutosProc = minutosProc + GetMinutos(arrTiposHora[ith], thProcesado, horadesde, horahasta);
            
            return minutosProc;
        }

        /// <summary>
        /// Retorna las horas PROCESADAS de un tipo de hora sobre un tipo de hora ESPERADO en un rango
        /// </summary>
        public double GetHoras(string thEsperado, string thProcesado, int horadesde, int horahasta) { return this.GetMinutos(thEsperado, thProcesado, horadesde, horahasta) / 60.0; }
        
        /// <summary>
        /// Retorna las horas PROCESADAS de un tipo de hora sobre un tipo de hora ESPERADO en un rango
        /// </summary>
        public double GetHoras(string[] thEsperado, string thProcesado, int horadesde, int horahasta) { return this.GetMinutos(thEsperado, thProcesado, horadesde, horahasta) / 60.0; }


        


        
        /// <summary>
        /// Retorna los minutos PROCESADOS de un tipo de hora dentro de un rango de horas
        /// </summary>
		public int GetMinutos(string tipohora, int horadesde, int horahasta)
		{
			int mindesde = horadesde / 100 * 60 + horadesde % 100;
			int minhasta = horahasta / 100 * 60 + horahasta % 100;
			if (mindesde > minhasta) minhasta += 1440;
			return GetMinutosRango(tipohora, mindesde, minhasta) + GetMinutosRango(tipohora, mindesde - 1440, minhasta - 1440) + GetMinutosRango(tipohora, mindesde + 1440, minhasta + 1440);
		}

        /// <summary>
        /// Retorna los minutos PROCESADOS de un Array de tipos de hora dentro de un rango en horas
        /// </summary>
        public int GetMinutos(string[] arrTiposHora, int horadesde, int horahasta) {
            int minutosProc = 0;
            
            //Recorre los tipos de hora
            for (int ith = 0; ith < arrTiposHora.Length; ith++)
                minutosProc = minutosProc + GetMinutos(arrTiposHora[ith], horadesde, horahasta);
            
            return minutosProc;
        }

        /// <summary>
        /// Retorna los minutos PROCESADOS de un tipo de hora dentro de un rango de horas
        /// </summary>
        public double GetHoras(string tipohora, int horadesde, int horahasta) { return this.GetMinutos(tipohora, horadesde, horahasta) / 60.0; }

        /// <summary>
        /// Retorna las horas PROCESADAS de un Array de tipos de hora dentro de un rango en horas
        /// </summary>
        public double GetHoras(string[] arrTiposHora, int horadesde, int horahasta) { return this.GetMinutos(arrTiposHora, horadesde, horahasta) / 60.0; }
        
        /// <summary>
        /// Retorna los minutos PROCESADOS de un tipo de hora dentro de un rango
        /// </summary>
        public int GetMinutosRango(string thEsperado, string thProcesado, int mindesde, int minhasta)
        {
            int retMin = 0, ith;
            int minDesdeFinal, minHastaFinal;
            ArrayList arrDetalles;
            NucleusRH.Base.Tiempos_Trabajados.Esperanzaper.DETALLE objDet;
            
            //Obtiene los DETALLES de un tipo de hora esperado en la jornada
            arrDetalles = GetTHDefinido(thEsperado);
            
            //Recorre los tipos de hora y va pidiendo los minutos a cada uno de sus rangos
            for (ith = 0; ith < arrDetalles.Count; ith++) {
                objDet = (NucleusRH.Base.Tiempos_Trabajados.Esperanzaper.DETALLE)arrDetalles[ith];

                minDesdeFinal = (mindesde > objDet.e_horainicio) ? mindesde : objDet.e_horainicio;
                minHastaFinal = (minhasta < objDet.e_horafin) ? minhasta : objDet.e_horafin;
                
                if (minHastaFinal > minDesdeFinal)
                    retMin += GetMinutosRango(thProcesado, minDesdeFinal, minHastaFinal);
                
            }

            return retMin;
        }

        /// <summary>
        /// Retorna los minutos PROCESADOS de un tipo de hora dentro de un rango
        /// </summary>
        public int GetMinutosRango(string[] thEsperado, string thProcesado, int mindesde, int minhasta)
        {
            int minutosProc = 0;
            
            //Recorre los tipos de hora
            for (int ith = 0; ith < thEsperado.Length; ith++)
                minutosProc = minutosProc + GetMinutosRango(thEsperado[ith], thProcesado, mindesde, minhasta);
            
            return minutosProc;
        }

        /// <summary>
        /// Retorna los minutos PROCESADOS de un tipo de hora dentro de un rango
        /// </summary>
        public int GetMinutosRango(string tipohora, int mindesde, int minhasta)
        {
            int retMin = 0;
            foreach (LIQUIDACIONPROC proc in this.LIQUIDACPROC)
            {
                 
                if (proc.TipoHora != tipohora) continue;
                retMin += NucleusRH.Base.Tiempos_Trabajados.RHLiq.LiqConceptosBase.Range(proc.Entrada, proc.Salida, mindesde, minhasta);
            }
            return retMin;
        }

	
        /// <summary>
        /// Retorna los minutos PROCESADOS de un Array de tipos de hora dentro de un rango
        /// </summary>
        public int GetMinutosRango(string[] arrTiposHora, int mindesde, int minhasta) {
            int minutosProc = 0;
            
            //Recorre los tipos de hora
            for (int ith = 0; ith < arrTiposHora.Length; ith++)
                minutosProc = minutosProc + GetMinutosRango(arrTiposHora[ith], mindesde, minhasta);
            
            return minutosProc;
        }


        private int GetMinutosRangoPre(string pstrTHHasta, string pstrTHBuscada, int pintRangoD, int pintRangoH)
        {
            NucleusRH.Base.Tiempos_Trabajados.Esperanzaper.DETALLE objDet;

            //Obtiene los DETALLES de un tipo de hora esperado en la jornada
            ArrayList arrDetalles = GetTHDefinido(pstrTHHasta);
            int minHastaFinal = pintRangoH;

            if (arrDetalles.Count > 0)
            {
                objDet = (NucleusRH.Base.Tiempos_Trabajados.Esperanzaper.DETALLE)arrDetalles[0];
                if (objDet.e_horainicio <= pintRangoH && objDet.e_horainicio > pintRangoD)
                {
                    minHastaFinal = objDet.e_horainicio;
                    return GetMinutosRango(pstrTHBuscada, pintRangoD, minHastaFinal);
                }
                           
            }
            return 0;
        }

        private int GetMinutosRangoPost(string pstrTHDesde, string pstrTHBuscada, int pintRangoD, int pintRangoH)
        {
            NucleusRH.Base.Tiempos_Trabajados.Esperanzaper.DETALLE objDet;

            //Obtiene los DETALLES de un tipo de hora esperado en la jornada
            ArrayList arrDetalles = GetTHDefinido(pstrTHDesde);
            int minDesdeFinal = pintRangoD;

            if (arrDetalles.Count > 0)
            {
                objDet = (NucleusRH.Base.Tiempos_Trabajados.Esperanzaper.DETALLE)arrDetalles[arrDetalles.Count - 1];
                if (objDet.e_horafin < pintRangoH && objDet.e_horafin >= pintRangoD)
                {
                    minDesdeFinal = objDet.e_horafin;
                    return GetMinutosRango(pstrTHBuscada, minDesdeFinal, pintRangoH);
                }
                   
            }

            return 0;
        }

        /// <summary>
        /// Retorna las horas PROCESADAS de un tipo de hora dentro de un rango
        /// </summary>
        public double GetHorasRango(string tipohora, int mindesde, int minhasta) { return this.GetMinutosRango(tipohora, mindesde, minhasta) / 60.0; }

        /// <summary>
        /// Retorna las horas PROCESADAS de un Arrya de tipos de hora dentro de un rango
        /// </summary>
        public double GetHorasRango(string[] arrTiposHora, int mindesde, int minhasta) { return this.GetMinutosRango(arrTiposHora, mindesde, minhasta) / 60.0; }

        /// <summary>
        /// Retorna las horas PROCESADAS de un Arrya de tipos de hora dentro de un rango
        /// </summary>
        public double GetHorasRango(string[] thEsperado, string thProcesado, int mindesde, int minhasta) { return this.GetMinutosRango(thEsperado, thProcesado, mindesde, minhasta) / 60.0; }

        /// <summary>
        /// Retorna las horas PROCESADAS de un tipo de hora dentro de un rango hasta que encuentra el parametro de tipo de hora hasta
        /// </summary>
        /// <param name="pstrTHHasta">tipo de hora hasta la primer aparicion</param>
        /// <param name="pstrTHBuscada">tipo de hora a buscar</param>
        /// <param name="pintRangoD">rango minutos desde</param>
        /// <param name="pintRangoH">rango minutos hasta</param>
        /// <returns></returns>
        public double GetHorasRangoPre(string pstrTHHasta, string pstrTHBuscada, int pintRangoD, int pintRangoH) { return this.GetMinutosRangoPre(pstrTHHasta, pstrTHBuscada, pintRangoD, pintRangoH) / 60.0; }

        /// <summary>
        /// Retorna las horas PROCESADAS de un tipo de hora dentro de un rango desde que encuentra el parametro de tipo de hora desde 
        /// </summary>
        /// <param name="pstrTHDesde">tipo de hora desde la ultima aparicion</param>
        /// <param name="pstrTHBuscada">tipo de hora a buscar</param>
        /// <param name="pintRangoD">rango minutos desde</param>
        /// <param name="pintRangoH">rango minutos hasta</param>
        /// <returns></returns>
        public double GetHorasRangoPost(string pstrTHDesde, string pstrTHBuscada, int pintRangoD, int pintRangoH) { return this.GetMinutosRangoPost(pstrTHDesde, pstrTHBuscada, pintRangoD, pintRangoH) / 60.0; }

        /// <summary>
        /// Retorna los minutos ESPERADOS de un tipo de hora
        /// </summary>
		public int GetMinEsperados(string tipohora)
        {
            int retMin = 0;
            NucleusRH.Base.Tiempos_Trabajados.Esperanzaper.DIA dia = Esperanza;
            foreach (NucleusRH.Base.Tiempos_Trabajados.Esperanzaper.DETALLE det in dia.DETALLE)
            {
                NucleusRH.Base.Tiempos_Trabajados.Tipos_Horas.TIPOHORA th = det.GetTipoHora();
                if (!string.IsNullOrEmpty(tipohora) && th.c_tipohora != tipohora) continue;
                if (!th.Obligatorio) continue;
                retMin += det.Salida - det.Entrada;
            }
            return retMin;
        }

        /// <summary>
        /// Retorna los minutos ESPERADOS de un Arrya de tipos de hora
        /// </summary>
        public int GetMinEsperados(string[] arrTiposHora) {
            int minutosProc = 0;
            
            //Recorre los tipos de hora
            for (int ith = 0; ith < arrTiposHora.Length; ith++)
                minutosProc = minutosProc + GetMinEsperados(arrTiposHora[ith]);
            
            return minutosProc;
        }

        /// <summary>
        /// Retorna las horas ESPERADAS de un tipo de hora
        /// </summary>
        public double GetHorasEsperados(string tipohora) { return this.GetMinEsperados(tipohora) / 60.0; }
        
        /// <summary>
        /// Retorna las horas ESPERADAS de un Array de tipos de hora
        /// </summary>
        public double GetHorasEsperados(string[] arrTiposHora) { return this.GetMinEsperados(arrTiposHora) / 60.0; }






        
        /// <summary>
        /// Retorna los minutos ESPERADOS de un tipo de hora en un rango
        /// </summary>
        public int GetMinEsperadosRango(string tipohora, int mindesde, int minhasta)
        {
            int retMin = 0;
            NucleusRH.Base.Tiempos_Trabajados.Esperanzaper.DIA dia = Esperanza;
            foreach (NucleusRH.Base.Tiempos_Trabajados.Esperanzaper.DETALLE det in dia.DETALLE)
            {
                NucleusRH.Base.Tiempos_Trabajados.Tipos_Horas.TIPOHORA th = det.GetTipoHora();
                if (!string.IsNullOrEmpty(tipohora) && th.c_tipohora != tipohora) continue;
                if (!th.Obligatorio) continue;
                retMin += NucleusRH.Base.Tiempos_Trabajados.RHLiq.LiqConceptosBase.Range(det.Entrada, det.Salida, mindesde, minhasta);
            }
            return retMin;
        }

        /// <summary>
        /// Retorna los minutos ESPERADOS de un tipo de hora en un rango
        /// </summary>
        public int GetMinEsperadosRango(string[] arrTiposHora, int mindesde, int minhasta)  {
            int minutosProc = 0;
            
            //Recorre los tipos de hora
            for (int ith = 0; ith < arrTiposHora.Length; ith++)
                minutosProc = minutosProc + GetMinEsperadosRango(arrTiposHora[ith], mindesde, minhasta);
            
            return minutosProc;
        }
        
        /// <summary>
        /// Retorna las horas ESPERADAS de un tipo de hora en un rango
        /// </summary>
        public double GetHorasEsperadosRango(string tipohora, int mindesde, int minhasta) { return this.GetMinEsperadosRango(tipohora, mindesde, minhasta) / 60.0; }
        
        /// <summary>
        /// Retorna las horas ESPERADAS de un Arrya de tipos de hora en un rango
        /// </summary>
        public double GetHorasEsperadosRango(string[] arrTiposHora, int mindesde, int minhasta) { return this.GetMinEsperadosRango(arrTiposHora, mindesde, minhasta) / 60.0; }

        
        


        /// <summary>
        /// Retorna los minutos ESPERADOS de un tipo de hora en un rango de horas
        /// </summary>
		public int GetMinEsperados(string tipohora, int horadesde, int horahasta)
		{
			int mindesde = horadesde / 100 * 60 + horadesde % 100;
			int minhasta = horahasta / 100 * 60 + horahasta % 100;
			if (mindesde > minhasta) minhasta += 1440;
			return GetMinEsperadosRango(tipohora, mindesde, minhasta) + GetMinEsperadosRango(tipohora, mindesde - 1440, minhasta - 1440) + GetMinEsperadosRango(tipohora, mindesde + 1440, minhasta + 1440);
		}

        /// <summary>
        /// Retorna los minutos ESPERADOS de un Array de tipos de hora en un rango de horas
        /// </summary>
		public int GetMinEsperados(string[] arrTiposHora, int horadesde, int horahasta) {
            int minutosProc = 0;
            
            //Recorre los tipos de hora
            for (int ith = 0; ith < arrTiposHora.Length; ith++)
                minutosProc = minutosProc + GetMinEsperados(arrTiposHora[ith], horadesde, horahasta);
            
            return minutosProc;
        }

        /// <summary>
        /// Retorna las horas ESPERADAS de un tipo de hora en un rango de horas
        /// </summary>
        public double GetHorasEsperados(string tipohora, int horadesde, int horahasta) { return this.GetMinEsperados(tipohora, horadesde, horahasta) / 60.0; }

        /// <summary>
        /// Retorna las horas ESPERADAS de un Array de tipos de hora en un rango de horas
        /// </summary>
        public double GetHorasEsperados(string[] arrTiposHora, int horadesde, int horahasta) { return this.GetMinEsperados(arrTiposHora, horadesde, horahasta) / 60.0; }


        



        
        /// <summary>
        /// Retorna los minutos DEFINIDOS de un tipo de hora
        /// </summary>
        public int GetMinDefinidos(string tipohora)
        {
            int retMin = 0;
            NucleusRH.Base.Tiempos_Trabajados.Esperanzaper.DIA dia = Esperanza;
            foreach (NucleusRH.Base.Tiempos_Trabajados.Esperanzaper.DETALLE det in dia.DETALLE)
            {
                if (det.TipoHora != tipohora) continue;
                retMin += det.Salida - det.Entrada;
            }
            return retMin;
        }

        /// <summary>
        /// Retorna los minutos DEFINIDOS de un Array de tipos de hora
        /// </summary>
		public int GetMinDefinidos(string[] arrTiposHora) {
            int minutosDef = 0;
            
            //Recorre los tipos de hora
            for (int ith = 0; ith < arrTiposHora.Length; ith++)
                minutosDef = minutosDef + GetMinDefinidos(arrTiposHora[ith]);
            
            return minutosDef;
        }
        
        /// <summary>
        /// Retorna las horas DEFINIDAS de un tipo de hora
        /// </summary>
        public double GetHorasDefinidos(string tipohora) { return this.GetMinDefinidos(tipohora) / 60.0; }
        
        /// <summary>
        /// Retorna las horas DEFINIDAS de un Array de tipos de hora
        /// </summary>
        public double GetHorasDefinidos(string[] arrTiposHora) { return this.GetMinDefinidos(arrTiposHora) / 60.0; }



        

        /// <summary>
        /// Retorna los minutos DEFINIDOS de un tipo de hora en un rango
        /// </summary>
        public int GetMinDefinidosRango(string tipohora, int mindesde, int minhasta)
        {
            int retMin = 0;
            NucleusRH.Base.Tiempos_Trabajados.Esperanzaper.DIA dia = Esperanza;
            foreach (NucleusRH.Base.Tiempos_Trabajados.Esperanzaper.DETALLE det in dia.DETALLE)
            {
                if (det.TipoHora != tipohora) continue;
                retMin += NucleusRH.Base.Tiempos_Trabajados.RHLiq.LiqConceptosBase.Range(det.Entrada, det.Salida, mindesde, minhasta);
            }
            return retMin;
        }

        /// <summary>
        /// Retorna los minutos DEFINIDOS de un Array de tipos de hora en un rango
        /// </summary>
		public int GetMinDefinidosRango(string[] arrTiposHora, int mindesde, int minhasta) {
            int minutosDef = 0;
            
            //Recorre los tipos de hora
            for (int ith = 0; ith < arrTiposHora.Length; ith++)
                minutosDef = minutosDef + GetMinDefinidosRango(arrTiposHora[ith], mindesde, minhasta);
            
            return minutosDef;
        }
        
        /// <summary>
        /// Retorna las horas DEFINIDAS de un tipo de hora en un rango
        /// </summary>
        public double GetHorasDefinidosRango(string tipohora, int mindesde, int minhasta) { return this.GetMinDefinidosRango(tipohora, mindesde, minhasta) / 60.0; }
        
        /// <summary>
        /// Retorna las horas DEFINIDAS de un Array de tipos de hora en un rango
        /// </summary>
        public double GetHorasDefinidosRango(string[] arrTiposHora, int mindesde, int minhasta) { return this.GetMinDefinidosRango(arrTiposHora, mindesde, minhasta) / 60.0; }





		
        /// <summary>
        /// Retorna los minutos DEFINIDOS de un tipo de hora en un rango
        /// </summary>
        public int GetMinDefinidos(string tipohora, int horadesde, int horahasta)
		{
			int mindesde = horadesde / 100 * 60 + horadesde % 100;
			int minhasta = horahasta / 100 * 60 + horahasta % 100;
			if (mindesde > minhasta) minhasta += 1440;
			return GetMinDefinidosRango(tipohora, mindesde, minhasta) + GetMinDefinidosRango(tipohora, mindesde - 1440, minhasta - 1440) + GetMinDefinidosRango(tipohora, mindesde + 1440, minhasta + 1440);
		}

        /// <summary>
        /// Retorna los minutos DEFINIDOS de un Array de tipos de hora en un rango
        /// </summary>
		public int GetMinDefinidos(string[] arrTiposHora, int mindesde, int minhasta) {
            int minutosDef = 0;
            
            //Recorre los tipos de hora
            for (int ith = 0; ith < arrTiposHora.Length; ith++)
                minutosDef = minutosDef + GetMinDefinidos(arrTiposHora[ith], mindesde, minhasta);
            
            return minutosDef;
        }
        
        /// <summary>
        /// Retorna las horas DEFINIDAS de un tipo de hora en un rango
        /// </summary>
        public double GetHorasDefinidos(string tipohora, int horadesde, int horahasta) { return this.GetMinDefinidos(tipohora, horadesde, horahasta) / 60.0; }
        
        /// <summary>
        /// Retorna las horas DEFINIDAS de un Array de tipos de hora en un rango
        /// </summary>
        public double GetHorasDefinidos(string[] arrTiposHora, int horadesde, int horahasta) { return this.GetMinDefinidos(arrTiposHora, horadesde, horahasta) / 60.0; }
        
        
        
        
        
        
        
        
        
        
        
        
        /// <summary>
        /// Retorna la cantidad de minutos que existen entre dos horas procesadas. Estas horas prcesadas pueden ser tanto de horas presenciales como de ausencias.
        /// </summary>
        public int GetMinVacio()
        {
            int retMin = 0;
            Liquidacion_Personas.LIQUIDACIONPROC PROCANT = null;
            foreach (Liquidacion_Personas.LIQUIDACIONPROC PROC in this.LIQUIDACPROC)
            {
                if (PROCANT != null && PROC.Entrada > PROCANT.Salida)
                    retMin += PROC.Entrada - PROCANT.Salida;
                PROCANT = PROC;
            }
            return retMin;
        }

        /// <summary>
        /// Retorna la cantidad de horas que existen entre dos horas procesadas. Estas horas prcesadas pueden ser tanto de horas presenciales como de ausencias.
        /// </summary>
        public double GetHorasVacio()
        {
            return this.GetMinVacio() / 60.0;
        }
        
        /// <summary>
        /// Retorna la cantidad de minutos que existen entre el inicio de la primer hora procesada y el fin de la última hora procesada.
        /// </summary>
        public int GetMinIO()
        {
            Liquidacion_Personas.LIQUIDACIONPROC PROCINI = (Liquidacion_Personas.LIQUIDACIONPROC)this.LIQUIDACPROC[0];
            Liquidacion_Personas.LIQUIDACIONPROC PROCFIN = (Liquidacion_Personas.LIQUIDACIONPROC)this.LIQUIDACPROC[this.LIQUIDACPROC.Count - 1];
            return PROCFIN.Salida - PROCINI.Entrada;
        }

        /// <summary>
        /// Retorna la cantidad de horas que existen entre el inicio de la primer hora procesada y el fin de la última hora procesada.
        /// </summary>
        public double GetHorasIO()
        {
            return this.GetMinIO() / 60.0;
        }
        
        /// <summary>
        /// Ijndica si el legajo tiene horario corrido.
        /// </summary>
		public bool Corrido
        {
            get
            {
                return (GetMinVacio() == 0);
            }
        }
        
        /// <summary>
        /// Devuelve, en minutos, la tardanza para el día de la jornada
        /// Solo contabiliza si la primer/as ausencia se corresponde a un tipo de hora de esperanza obligatorio
        /// </summary>
        public int MinTardanza 
        {
            get
            {
                NucleusRH.Base.Tiempos_Trabajados.Tipos_Horas.TIPOHORA objTH;
                int minTardanza = 0;
                TimeSpan tsTardanza;
                DateTime fechaHoraSalidaAnterior = new DateTime();
                bool obligatorios = false;

                if (!this.l_presente) return 0;

                //Recorre los detalles del procesamiento
                foreach (LIQUIDACIONPROC objProc in this.LIQUIDACPROC)
                {
                    
                    //Obtiene el tipo de hora de la esperanza que le corresponde al detalle del procesamiento
                    objTH = objProc.GetTipoHoraEsp();
                    
					//Si no retorna un tipo de hora esperado es que no debia venir pero lo hizo
					if (objTH == null)  break;
					
					if (objTH.Obligatorio) {
                        
                        obligatorios = true;
                        
                        //Obtiene el tipo de hora del detalle del procesamiento
                        objTH = objProc.GetTipoHora();
                        
                        if (objTH.Ausencia) {
                            //si la hora de salida anterior NO obligatoria es igual a la entrada actual, no se toma como tardanza
                            if (fechaHoraSalidaAnterior != objProc.f_fechoraentrada)
                            {
                                tsTardanza = objProc.f_fechorasalida - objProc.f_fechoraentrada;
                                minTardanza = minTardanza + (int)Math.Round(tsTardanza.TotalMinutes);
                            }
                        } else {
                            break;
                        }
                    
                    } else {    
                        fechaHoraSalidaAnterior = objProc.f_fechorasalida;
                        //Si viene un NO OBLIGATORIO después de OBLIGATORIOS se corta la recorrida
                        if (obligatorios) break;
                    }
                    
                }
                
                return minTardanza;
                
            }
            
        }

        /// <summary>
        /// Retorna un booleano indicando si la persona tiene minutos de tardanza.
        /// </summary>
        public bool Tardanza
        {
            get { return this.MinTardanza > 0; }
        }

        /// <summary>
        /// Devuelve, en horas, la tardanza para el día de la jornada
        /// Solo contabiliza si la primer/as ausencia se corresponde a un tipo de hora de esperanza obligatorio
        /// </summary>
        public double HorasTardanza 
        {
            get { return this.MinTardanza / 60.0; }
        }

/*        
        /// <summary>
        /// Indica si el legajo se retiró anticipadamente.
        /// </summary>
        public bool RetiroAnticipado
        {
            get
            {
                if (!this.l_presente) return false;
                NucleusRH.Base.Tiempos_Trabajados.Esperanzaper.DIA dia = Esperanza;
                foreach (NucleusRH.Base.Tiempos_Trabajados.Esperanzaper.DETALLE det in dia.DETALLE)
                {
                    Tipos_Horas.TIPOHORA TH = det.GetTipoHora();
                    if (TH.Obligatorio)
                    {
                        LIQUIDACIONPROC proc = (LIQUIDACIONPROC)this.LIQUIDACPROC.GetByAttribute("f_fechorasalida", this.f_fecjornada.AddMinutes(det.e_horafin));
						if (proc)
							if (proc.GetTipoHora().Ausencia)
								return true;
                    }
                }
                return false;
            }
        }
*/
        /// <summary>
        /// Indica si el legajo se retiró anticipadamente.
        /// </summary>
        public bool RetiroAnticipado
        {
            get {
                return this.MinRetiroAnticipado > 0;
            }
        }		
        
		/// <summary>
        /// Retorna la cantidad de minutos de retiro anticipado de un legajo para la jornada.
        /// </summary>
        public int MinRetiroAnticipado
        {
            get
            {
                int MinutosRetiro;
				int CantDetalles;
				NucleusRH.Base.Tiempos_Trabajados.Esperanzaper.DIA Dia;
				NucleusRH.Base.Tiempos_Trabajados.Esperanzaper.DETALLE Det;
				Tipos_Horas.TIPOHORA TH;
				LIQUIDACIONPROC Proc;
				NomadObjectList DetallesDia;
				
				MinutosRetiro = 0;
				
				//Si el legajo no estuvo presente directamente retorna 0
				if (!this.l_presente) return MinutosRetiro;
				
				try {
					//Obtiene la esperanza del día
					Dia = Esperanza;
					DetallesDia = Dia.DETALLE;
					
					if (DetallesDia != null) {
						CantDetalles = DetallesDia.Count;
						for (int idxDetalle = DetallesDia.Count - 1; idxDetalle >= 0; idxDetalle--) {
							Det = (NucleusRH.Base.Tiempos_Trabajados.Esperanzaper.DETALLE) DetallesDia[idxDetalle];
							TH = Det.GetTipoHora();
							if (TH.Obligatorio) {
								Proc = (LIQUIDACIONPROC)this.LIQUIDACPROC.GetByAttribute("f_fechorasalida", this.f_fecjornada.AddMinutes(Det.e_horafin));
								if (Proc != null)
									if (Proc.GetTipoHora().Ausencia) {
										//Convertido a minutos
										MinutosRetiro = (int) Math.Round (Proc.n_cantidadhs * 60);
										break;
									}
								
								//Solo toma el último obligatorio
								break;
							}
						}
					}
					
				} catch (Exception ex) {
					//Se produjo un error desconicido
					NomadBatch.Trace("Se produjo un error en MinutosRetiro. '" + ex.Message + "'.");
					MinutosRetiro = 0;
				}
				
				
				return MinutosRetiro;

            }
        }

        /// <summary>
        /// Devuelve, en horas, el retiro anticipado para el día de la jornada
        /// </summary>
        public double HorasRetiroAnticipado
        {
            get { return this.MinRetiroAnticipado / 60.0; }
        }
		
        public object ValorVariable(Hashtable myHASH)
        {
			if (!myHASH.ContainsKey(this.iDay)) return null;
            return myHASH[this.iDay];
        }
		
		public bool ValorVariable(Hashtable myHASH, bool defValue)
		{
			if (!myHASH.ContainsKey(this.iDay)) return defValue;
            return (bool)myHASH[this.iDay];
		}
		public int ValorVariable(Hashtable myHASH, int defValue)
		{
			if (!myHASH.ContainsKey(this.iDay)) return defValue;
            return (int)myHASH[this.iDay];
		}
		public double ValorVariable(Hashtable myHASH, double defValue)
		{
			if (!myHASH.ContainsKey(this.iDay)) return defValue;
            return (double)myHASH[this.iDay];
		}
        
        /// <summary>
        /// Retorna un array con los detalles DEFINIDOS de un tipo de hora en particular
        /// </summary>        
        public ArrayList GetTHDefinido(string pstrTH, int pcant) {

            ArrayList arrResult = new ArrayList();
            
            NucleusRH.Base.Tiempos_Trabajados.Esperanzaper.DIA dia = Esperanza;
            foreach (NucleusRH.Base.Tiempos_Trabajados.Esperanzaper.DETALLE det in dia.DETALLE) {
                if (det.TipoHora != pstrTH) continue;
                
                arrResult.Add(det);
                pcant--;
                
                if(pcant == 0) break;
            }
            
            return arrResult;
        }

        /// <summary>
        /// Retorna un array con los detalles DEFINIDOS de un tipo de hora en particular
        /// </summary>        
        public ArrayList GetTHDefinido(string pstrTH) {
            return GetTHDefinido(pstrTH, 0);
            
        }
        
        /// <summary>
        /// Retorna el primer detalle DEFINIDO de un tipo de hora en particular
        /// </summary>        
        public NucleusRH.Base.Tiempos_Trabajados.Esperanzaper.DETALLE GetPrimerTHDefinido(string pstrTH) {
            ArrayList arrResult = GetTHDefinido(pstrTH, 1);
            
            if (arrResult.Count == 0) 
                return null;
            else 
                return (NucleusRH.Base.Tiempos_Trabajados.Esperanzaper.DETALLE) arrResult[0];

        }

        /// <summary>
        /// Realiza el set de los Francos Compensatorios generados para una jornada y un banco de hora
		/// Es decir, acumula tiempo.
        /// </summary>  
        public void SetFC(int cant, string c_banco, bool l_aprobado)
        {
            NucleusRH.Base.Tiempos_Trabajados.Personal.PERSONAL_EMP LEG = Base.Tiempos_Trabajados.Personal.PERSONAL_EMP.Get(this.OIPersonalEmp);
            LEG.SetFC(this.f_fecjornada, cant, c_banco, l_aprobado);

        }

/*
        /// <summary>
        /// Realiza el get de los Francos Compensatorios generados para una jornada y un banco de hora
		/// Es decir, retorna las horas de ausencia por compensación.
        /// </summary>  
        public int GetComp(string c_banco)
        {
            NucleusRH.Base.Tiempos_Trabajados.Personal.PERSONAL_EMP LEG = Base.Tiempos_Trabajados.Personal.PERSONAL_EMP.Get(this.OIPersonalEmp);
            return LEG.GetComp(this.f_fecjornada, c_banco);
        }
*/
        /// <summary>
        /// Realiza el get de los Francos Compensatorios generados para una jornada y un banco de hora
		/// Es decir, retorna las horas de ausencia por compensación.
        /// </summary>  
        public int GetComp(string c_banco)
        {
            
			//Maneja un solo cache por Banco. 
			//Mientras se solicita del mismo legajo lo mantiene. Cuando se cambia el legajo lo vuelve a cargar y lo guarda.
			
NomadLog.Debug("LIQUIDACJOR.GetComp() 1");
			
			Hashtable htaLegajoFC;
			string    strLegajoInCache;
			string    strLEGKey = "TTA.Procesos.GetFCLegajo.LEG-" + c_banco;
			string    strHTAKey = "TTA.Procesos.GetFCLegajo.HTA-" + c_banco;
			string    strFecJornada;

NomadLog.Debug("LIQUIDACJOR.GetComp() 2");
			
			//Verifica si está en CACHE
			strLegajoInCache = NomadProxy.GetProxy().CacheGet(strLEGKey);
NomadLog.Debug("LIQUIDACJOR.GetComp() 3");
			htaLegajoFC = (Hashtable) NomadProxy.GetProxy().CacheGetObj(strHTAKey);
NomadLog.Debug("LIQUIDACJOR.GetComp() 4");			
			
			if (strLegajoInCache != this.strOIPersonalEmp || htaLegajoFC == null) {
				
NomadLog.Debug("LIQUIDACJOR.GetComp() 5");

				//Si no existe ejecuta el query para traer los valores.
				NomadLog.Debug("LIQUIDACJOR.GetComp() - Se crea el cache para el OI_PersonalEmp: '" + this.strOIPersonalEmp + "'.");
				strLegajoInCache = this.OIPersonalEmp;
				htaLegajoFC = Base.Tiempos_Trabajados.Personal.PERSONAL_EMP.GetComps(int.Parse(strLegajoInCache), this.myliq.f_fechainicio, this.myliq.f_fechafin, c_banco);
				
				//Guarda los nuevos objetos en el CACHE
NomadLog.Debug("LIQUIDACJOR.GetComp() 6");
				NomadProxy.GetProxy().CacheAdd(strLEGKey, strLegajoInCache);
NomadLog.Debug("LIQUIDACJOR.GetComp() 7");
				NomadProxy.GetProxy().CacheAdd(strHTAKey, htaLegajoFC);
NomadLog.Debug("LIQUIDACJOR.GetComp() 7");
			}
			
			//Busca si tiene Compnesaciones para el día en particular
			strFecJornada = this.f_fecjornada.ToString("yyyyMMdd");
NomadLog.Debug("LIQUIDACJOR.GetComp() 8");			
			if (htaLegajoFC.ContainsKey(strFecJornada)) {
NomadLog.Debug("LIQUIDACJOR.GetComp() 9");
				NomadXML xmlCOMP = (NomadXML) htaLegajoFC[strFecJornada];
				return xmlCOMP.GetAttrInt("n_cant_comp");
			}
			
			return 0;
        }
		
		
        /// <summary>
        /// Realiza el set de la Compensación de un FC para una jornada y un banco de hora
		/// Es decir, crea detalles de compensación para una ausencia.
        /// </summary>  
        public void SetComp(int cant, string c_banco)
        {
			
			int intComp;
			
			//Recupera la compensación del legajo para el día en cuestión
			intComp = this.GetComp(c_banco);
			
			//Si la cantidad compensada es diferente a la cantidad a compensar debe recalcular las compensaciones 
			if (intComp != cant) {
				
				NomadLog.Debug("LIQUIDACJOR.SetComp() - Cambian las cantidades a compensar respecto al procesamiento anterior para la fecha '" + this.f_fecjornada.ToString("yyyyMMdd") + "'.");
				
				NucleusRH.Base.Tiempos_Trabajados.Personal.PERSONAL_EMP LEG = Base.Tiempos_Trabajados.Personal.PERSONAL_EMP.Get(this.OIPersonalEmp);
				LEG.SetComp(this.f_fecjornada, cant, c_banco);
				
				//¿Actualizar el hash con la nueva compensación?
			}

        }

        /// <summary>
        /// Elimina los Francos Generados para una jornada
        /// </summary>  
        public void DelFC()
        {
            NucleusRH.Base.Tiempos_Trabajados.Personal.PERSONAL_EMP LEG = Base.Tiempos_Trabajados.Personal.PERSONAL_EMP.Get(this.OIPersonalEmp);
            LEG.DelFC(this.f_fecjornada);
        }
		
        /// <summary>
        /// Realiza el set de los Francos Compensatorios generados para una jornada y un banco de hora
		/// Es decir, acumula tiempo.
        /// </summary>  
        public void SetFranco(int cant, string c_banco, bool l_aprobado) {
            NucleusRH.Base.Tiempos_Trabajados.Personal.PERSONAL_EMP LEG = Base.Tiempos_Trabajados.Personal.PERSONAL_EMP.Get(this.OIPersonalEmp);
            LEG.SetFC(this.f_fecjornada, cant, c_banco, l_aprobado);

        }        
    }

}



using System;
using System.Xml;
using System.Collections;

using Nomad.NSystem.Base;
using Nomad.NSystem.Proxy;
using Nomad.NSystem.Functions;

using NucleusRH.Base.Tiempos_Trabajados.Liquidacion_Personas;
using NucleusRH.Base.Tiempos_Trabajados.Esperanzaper;

/*
Notas
03/09/2009 - Se crea la librería de procesos custom para cada cliente.
*/

namespace NucleusRH.Base.Tiempos_Trabajados {

	public class Procesos_Especiales {
		
		public static bool PostCloseDay(LIQUIDACJOR pobjJor) {
			NomadXML xmlParams;
			NomadXML xmlH;
			DateTime dteF1d, dteF1h;
			DateTime dteF2d, dteF2h;
			DateTime dteF3d, dteF3h;
			DateTime dteFd, dteFh;
			DateTime dteDesde;
			DateTime dteHasta;
			double dblDiaHasta;
			double dblCantHoras;
			ArrayList arrProcs;
			LIQUIDACIONPROC objNewProc;
			Hashtable htaHorarios;
			Hashtable htaHorasCambio = new Hashtable();
			NomadProxy objProxy = NomadProxy.GetProxy();
			NomadBatch objBatch;
			
			objBatch = NomadBatch.GetBatch("ProcesosEspeciales", "");
			
			if (pobjJor.Esperanza == null) return true;

			//Valida que sea solo horário libre ----------------------------------------------------------------
			htaHorarios = NomadEnvironment.QueryHashtable(NucleusRH.Base.Tiempos_Trabajados.Horarios.HORARIO.Resources.QRY_Horarios, "", "oi_horario", true);

			if (htaHorarios.ContainsKey(pobjJor.Esperanza.oi_horario))
				xmlH = (NomadXML) htaHorarios[pobjJor.Esperanza.oi_horario];
			else
				throw new Exception ("Es está pidiendo un Horario que no existe. OI_HORARIO='" + pobjJor.Esperanza.oi_horario + "'");

			if (xmlH.GetAttr("d_tipohorario") != "L")
				 return true;
			
			//Obtiene el XML con los parámetros para el método -------------------------------------------------
			{
				string strCacheKey = "ReemplazoHoras.TTA.Params.xml";
				string strH;
				int H, M;
				
				xmlParams = (NomadXML) objProxy.CacheGetObj(strCacheKey);
				if (xmlParams == null) {
					try {
						xmlParams = objProxy.FileServiceIO().LoadFileXML("PARAMS", "ReemplazoHoras.TTA.Params.xml");
					} catch (Exception ex) {
						objBatch.Err("No se encontró el archivo de parámetros 'ReemplazoHoras.TTA.Params.xml' para procesos especiales." + ex.Message);
						return false;
					}
						
					objProxy.CacheAdd(strCacheKey, xmlParams);
				}
				
				//Interpreta los valores
				strH = xmlParams.GetAttr("h_desde");
				H = int.Parse(strH.Split(':')[0]);
				M = int.Parse(strH.Split(':')[1]);
				dteDesde = new DateTime(DateTime.Today.Year, DateTime.Today.Month, DateTime.Today.Day, H, M, 0);
				
				strH = xmlParams.GetAttr("h_hasta");
				H = int.Parse(strH.Split(':')[0]);
				M = int.Parse(strH.Split(':')[1]);
				dteHasta = new DateTime(DateTime.Today.Year, DateTime.Today.Month, DateTime.Today.Day, H, M, 0);
				
				//Obtiene los tipos de horas a cambiar
				for (NomadXML xmlHora = xmlParams.FirstChild(); xmlHora != null; xmlHora = xmlHora.Next()) {
					htaHorasCambio.Add(xmlHora.GetAttr("oi_th_origen"), xmlHora);
				}
				
			}
			
			dteF1d = pobjJor.f_fecjornada.AddDays(-1d).AddHours(dteDesde.Hour).AddMinutes(dteDesde.Minute);
			dteF2d = pobjJor.f_fecjornada.AddHours(dteDesde.Hour).AddMinutes(dteDesde.Minute);
			dteF3d = pobjJor.f_fecjornada.AddDays(1d).AddHours(dteDesde.Hour).AddMinutes(dteDesde.Minute);
			
			dblDiaHasta = dteDesde > dteHasta ? 1 : 0;

			dteF1h = pobjJor.f_fecjornada.AddDays(-1 + (dblDiaHasta * 1)).AddHours(dteHasta.Hour).AddMinutes(dteHasta.Minute);
			dteF2h = pobjJor.f_fecjornada.AddDays(dblDiaHasta * 1).AddHours(dteHasta.Hour).AddMinutes(dteHasta.Minute);
			dteF3h = pobjJor.f_fecjornada.AddDays(1 + (dblDiaHasta * 1)).AddHours(dteHasta.Hour).AddMinutes(dteHasta.Minute);

			//Recorre los detalles de la jornada -------------------------------------------------------------
			arrProcs = new ArrayList();
			dblCantHoras = 0;
			foreach (LIQUIDACIONPROC objProc in pobjJor.LIQUIDACPROC) {
				
				if (!htaHorasCambio.ContainsKey(objProc.oi_tipohora)) continue;
				dteFd = DateTime.MinValue;
				dteFh = DateTime.MinValue;
				
				xmlH = (NomadXML) htaHorasCambio[objProc.oi_tipohora];
				
				//Prepara los rangos para comparar
				if (dteF1d < objProc.f_fechorasalida && dteF1h > objProc.f_fechoraentrada) {
					dteFd = dteF1d; dteFh = dteF1h;
				}
				if (dteF2d < objProc.f_fechorasalida && dteF2h > objProc.f_fechoraentrada) {
					dteFd = dteF2d; dteFh = dteF2h;
				}
				if (dteF3d < objProc.f_fechorasalida && dteF3h > objProc.f_fechoraentrada) {
					dteFd = dteF3d; dteFh = dteF3h;
				}
				
				if (dteFd != DateTime.MinValue) {
					//El detalle cae dentro de uno de los rangos
					//Solo se cambia el tipo de hora a las horas ya existentes. 
					//Si se deben "partir" las horas se crearán nuevos PROCs con las horas originales
					
					//Según los rangos determina si es necesesario crear nuevos detalles

					if (objProc.f_fechoraentrada < dteFd) {
						//Es necesario crear un PROC previo con la hora original
						
						objNewProc = new LIQUIDACIONPROC();
						objNewProc.oi_tipohora      = objProc.oi_tipohora;
						objNewProc.f_fechoraentrada = objProc.f_fechoraentrada;
						objNewProc.f_fechorasalida  = dteFd;
						objNewProc.n_cantidadhs     = ((TimeSpan)objNewProc.f_fechorasalida.Subtract(objNewProc.f_fechoraentrada)).TotalHours;
						
						if (!objProc.e_posicionNull)      objNewProc.e_posicion      = objProc.e_posicion;
						if (!objProc.oi_estructuraNull)   objNewProc.oi_estructura   = objProc.oi_estructura;
						if (!objProc.oi_tipohora_espNull) objNewProc.oi_tipohora_esp = objProc.oi_tipohora_esp;
						
						arrProcs.Add(objNewProc);
						
						objProc.f_fechoraentrada = dteFd;
						dblCantHoras = dblCantHoras + objNewProc.n_cantidadhs;
					}
					
					if (objProc.f_fechorasalida > dteFh) {
						//Es necesario crear un PROC posterior con la hora original
						
						objNewProc = new LIQUIDACIONPROC();
						objNewProc.oi_tipohora      = objProc.oi_tipohora;
						objNewProc.f_fechoraentrada = dteFh;
						objNewProc.f_fechorasalida  = objProc.f_fechorasalida;
						objNewProc.n_cantidadhs     = ((TimeSpan)objNewProc.f_fechorasalida.Subtract(objNewProc.f_fechoraentrada)).TotalHours;
						
						if (!objProc.e_posicionNull)      objNewProc.e_posicion      = objProc.e_posicion;
						if (!objProc.oi_estructuraNull)   objNewProc.oi_estructura   = objProc.oi_estructura;
						if (!objProc.oi_tipohora_espNull) objNewProc.oi_tipohora_esp = objProc.oi_tipohora_esp;
						
						arrProcs.Add(objNewProc);
						
						objProc.f_fechorasalida = dteFh;
						dblCantHoras = dblCantHoras + objNewProc.n_cantidadhs;
					}

					//Cambia el tipo de hora al PROC
					objProc.oi_tipohora = xmlH.GetAttr("oi_th_destino");
					
					//Si existen horas agregadas es porque se modificaron la hora de entrada o salida del detalle original
					//Se recalcula la cantidad de horas
					if (dblCantHoras > 0)
						objProc.n_cantidadhs = objProc.n_cantidadhs - dblCantHoras;
				}
				
			}
			
			//Pregunta si se crearon nuevos detalles
			if (arrProcs.Count > 0)
				for(int x = 0; x < arrProcs.Count; x++) {
					pobjJor.LIQUIDACPROC.Add((LIQUIDACIONPROC)arrProcs[x]);
				}
			
			//return bolResult;
			return true;
		}
		
	}
}

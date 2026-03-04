using System;
using System.Collections;
using System.Text;
using System.Xml;

using Nomad.NSystem.Base;
using Nomad.NSystem.Proxy;
using Nomad.NSystem.Functions;

using NucleusRH.Base.Tiempos_Trabajados.Liquidacion_Personas;
using NucleusRH.Base.Tiempos_Trabajados.Liquidacion;

/*
Histórico
05/06/2015 - Se agrega caché de Tipos de horas y sus coeficientes para franco compensatorio
05/06/2015 - Se agrega caché de parámetros de la DB
*/
namespace NucleusRH.Base.Tiempos_Trabajados {

    public static class CalculoFrancos {

		//Variables de uso interno
		//private NomadProxy m_objProxy;
		
		/*
        public CalculoFrancos() {
            m_objProxy = NomadProxy.GetProxy();
			this.htaResultado = new Hashtable()
        }
		*/
		
        /// <summary>
        /// Calcula la cantidad de horas franco compensatorios para una liquidación en particular y un grupo de LiqPers.
        /// </summary>
        /// <param name="pstrOiLiquidacion">Oi de la liquidación.</param>
        /// <param name="parrOILiqPers">ArrayList con la lista de Oi LiqPers.</param>
        /// <returns></returns>
        public static bool CalcularLiqPer(LIQUIDACIONPERS pobjLPers) 
		{            
            double dblCalculado;                        
            NomadBatch objBatch;
			
            double dblMinutosAc = 0;
            double dblMinutosAcJor = 0;

            objBatch = NomadBatch.GetBatch("CalcularHorasFranco", "CalcularHorasFranco");

            //Recorre las jornadas dentro de la liquidación por persona
            foreach (LIQUIDACJOR objLJor in pobjLPers.LIQUIDACJOR) 
			{

                dblMinutosAcJor = 0;

                //Recorre las horas dentro de la jornada
                foreach (LIQUIDACIONPROC objLProc in objLJor.LIQUIDACPROC) 
				{
					dblCalculado = CalcularPorHora(objLProc);
                    
					dblMinutosAc = dblMinutosAc + dblCalculado;
                    dblMinutosAcJor = dblMinutosAcJor + dblCalculado;					
                }

                objLJor.n_horas_fc = (double) (dblMinutosAcJor / 60); //convertido a horas
            }

            pobjLPers.n_horas_fc = (double) (dblMinutosAc / 60); //convertido a horas

            return true;
        }

        public static double CalcularPorHora(LIQUIDACIONPROC objLProc)
		{			
			Hashtable CoeHoras;
			strParams objParametros;
			double dblMinutosTrab;
            double dblCoeficiente;
			double dblTemp;
			double retval = 0d;
			
			//Obtiene los parametros de la tabla parametros
            objParametros = GetDBParams();
			
			//Obtiene los coeficientes de francos por tipo de hora
			CoeHoras = GetCoeFrancos();
			
			//Solamente contabiliza las horas a tipos de hora con un coeficiente diferente a 0
			if (!CoeHoras.ContainsKey(objLProc.oi_tipohora)) 				
				return 0d;

			dblCoeficiente = (double) CoeHoras[objLProc.oi_tipohora];

			if ( dblCoeficiente != 0d) 
			{
				dblMinutosTrab = objLProc.n_cantidadhs * 60; //convertido a minutos
				retval = dblMinutosTrab * dblCoeficiente;

				if (retval > 0) 
				{

					if (retval < objParametros.MinMinFC) 
					{
						retval = 0;
					} 
					else 
					{

						if (objParametros.MinMaxFC > 0 && retval > 0)
							if (retval > objParametros.MinMaxFC)
								retval = objParametros.MinMaxFC;

						if (objParametros.MinRedondeoFC > 0) 
						{
							dblTemp = retval / objParametros.MinRedondeoFC;
							retval = System.Math.Truncate(dblTemp) * objParametros.MinRedondeoFC;
						}
					}
				}
				else if (retval < 0) 
				{
					if (retval < objParametros.MinMaxFC * -1)
						retval = objParametros.MinMaxFC * -1;

					if (objParametros.MinRedondeoFC > 0) 
					{
						dblTemp = retval / objParametros.MinRedondeoFC;
						retval = System.Math.Truncate(dblTemp) * objParametros.MinRedondeoFC;
					}
				}                        
			}			
			return retval;
		}
		
		/*
		private ArrayList GetLiqPers(string pstrOiLiquidacion, string pstrOIPersonalEmp) {
            ArrayList arrResultado = new ArrayList();
            string strResult = "";
            bool blnAgregar;

			//Crea el objeto de Parametros
            NomadXML objQParams = new NomadXML("DATO");
			objQParams.SetAttr("oi_liquidacion", pstrOiLiquidacion);

            //Ejecuta el query
            strResult = m_objProxy.SQLService().Get(NucleusRH.Base.Tiempos_Trabajados.Liquidacion.LIQUIDACION.Resources.QRY_PERSONAL_EN_LIQ, objQParams.ToString());

            //Recorre los tipos de horas y los agrega en la colección
            XmlTextReader xtrLP = new XmlTextReader(strResult, System.Xml.XmlNodeType.Document, null);
            xtrLP.XmlResolver = null; // ignore the DTD
            xtrLP.WhitespaceHandling = WhitespaceHandling.None;
            xtrLP.Read();

            while (xtrLP.Read()) {
				if (!xtrLP.IsStartElement())
					continue;

                blnAgregar = true;

                if (pstrOIPersonalEmp != null)
                    blnAgregar = xtrLP.GetAttribute("oi_personal_emp") == pstrOIPersonalEmp;

                if (blnAgregar)
                    arrResultado.Add(xtrLP.GetAttribute("oi_LiquidacionPers"));
            }

            return arrResultado;
        }

		*/
		
		
		
        /// <summary>
        /// Trae los parámetros de la base de datos (tabla ORG26_PARAMETROS)
        /// </summary>
        /// <returns></returns>
        private static strParams GetDBParams() {
            strParams objResult;
            string strResult = "";
			string strCacheKey = "TTA.Procesos.GetDBParams";

			//Pregunta si está en el caché ---------------------------------------------------------------------------------------
			object obj = NomadProxy.GetProxy().CacheGetObj(strCacheKey);
			if (obj != null) {
				NomadBatch.Trace("GetDBParams() - Se recupera de caché con clave " + strCacheKey);
				return (strParams) obj;
			}
			
			//No está en el caché ---------------------------------------------------------------------------------------
			objResult = new strParams();

			//Crea el objeto de Parametros
            NomadXML objQParams = new NomadXML("DATOS");
			objQParams.SetAttr("c_modulo", "TTA");
			objQParams.SetAttr("d_clase", "\\'Liquidacion\\'");

            strResult = NucleusRH.Base.Organizacion.Parametros.PARAMETRO.Resources.QRY_Parametros;

            //Ejecuta el query            
			strResult = NomadProxy.GetProxy().SQLService().Get(NucleusRH.Base.Organizacion.Parametros.PARAMETRO.Resources.QRY_Parametros, objQParams.ToString());

            if (strResult.EndsWith("/>")) {
				//Guarda el resultado en el caché ---------------------------------------------------------------------------------------
				NomadProxy.GetProxy().CacheAdd(strCacheKey, objResult);
				NomadBatch.Trace("GetDBParams() - Se agrega objeto vacio a caché con clave " + strCacheKey);
				return objResult;
			}

            XmlTextReader xtrResult = new XmlTextReader(strResult, System.Xml.XmlNodeType.Document, null);
            xtrResult.XmlResolver = null; xtrResult.WhitespaceHandling = WhitespaceHandling.None;

            xtrResult.Read();
            xtrResult.Read();

            //Recorre los registros
            string strTemporalValue;
            while (xtrResult.Name != "params") {
                strTemporalValue = xtrResult.GetAttribute("d_valor") == null ? "0" : xtrResult.GetAttribute("d_valor");

                switch (xtrResult.GetAttribute("c_parametro")) {
                    case "MinMinFC": objResult.MinMinFC = StringUtil.str2dbl(strTemporalValue); break;
                    case "MinMaxFC": objResult.MinMaxFC = StringUtil.str2dbl(strTemporalValue); break;
                    case "MinRedondeoFC": objResult.MinRedondeoFC = int.Parse(strTemporalValue); break;
                }

                xtrResult.Read();
            }

            xtrResult.Close();
			
			//Guarda el resultado en el caché ---------------------------------------------------------------------------------------
			NomadProxy.GetProxy().CacheAdd(strCacheKey, objResult);
			NomadBatch.Trace("GetDBParams() - Se agrega a caché con clave " + strCacheKey);

            return objResult;
        }

		private static Hashtable GetCoeFrancos() {
			string strTemporalValue = "";
			Hashtable htaResultado;
			string strCacheKey = "TTA.Procesos.GetCoeFrancos";
			
			//Pregunta si está en el caché ---------------------------------------------------------------------------------------
			htaResultado = (Hashtable) NomadProxy.GetProxy().CacheGetObj(strCacheKey);
			if (htaResultado != null) {
				NomadBatch.Trace("GetCoeFrancos() - Se recupera de caché con clave " + strCacheKey);
				return htaResultado;
			}
			//No está en el caché ---------------------------------------------------------------------------------------
			htaResultado = new Hashtable();
			
			//Ejecuta el query			
			strTemporalValue =  NomadProxy.GetProxy().SQLService().Get(NucleusRH.Base.Tiempos_Trabajados.Tipos_Horas.TIPOHORA.Resources.QRY_TiposHoras, "");

			//Recorre los tipos de horas y los agrega en la colección
			XmlTextReader xtrTH = new XmlTextReader(strTemporalValue, System.Xml.XmlNodeType.Document, null);
			xtrTH.XmlResolver = null; // ignore the DTD
			xtrTH.WhitespaceHandling = WhitespaceHandling.None;
			xtrTH.Read();

			while (xtrTH.Read()) {

				if (!xtrTH.IsStartElement())
					continue;

				strTemporalValue = xtrTH.GetAttribute("n_coeficiente_fc");
				strTemporalValue = (strTemporalValue == null || strTemporalValue == "") ? "0" : strTemporalValue;

				htaResultado.Add(xtrTH.GetAttribute("oi"), StringUtil.str2dbl(strTemporalValue));
			}

			//Guarda el resultado en el caché ---------------------------------------------------------------------------------------
			NomadProxy.GetProxy().CacheAdd(strCacheKey, htaResultado);
			NomadBatch.Trace("GetCoeFrancos() - Se agrega a caché con clave " + strCacheKey);

			return htaResultado;
		}

        public struct strParams {
            public double MinMinFC;
            public double MinMaxFC;
            public int MinRedondeoFC;
        }

    }

}



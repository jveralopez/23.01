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

  //////////////////////////////////////////////////////////////////////////////////
  //Clase Procesamiento por Persona
    public partial class LIQUIDACIONPERS : Nomad.NSystem.Base.NomadObject
    {

        private string strOIPersonalEmp = "";

        /// <summary>
        /// Retorna el código del OI del legajo.
        /// </summary>
        public string OIPersonalEmp
        {
            get { return this.strOIPersonalEmp; }
            set { this.strOIPersonalEmp = value; }
        }

        /// <summary>
        /// Retorna el Convenio Último de la persona.
		/// El query se cachea y comparte la clave con CategoriaUlt.
        /// </summary>		
        public string ConvenioUlt() {
			return ConvenioCategoriaUlt(true);
		}

        /// <summary>
        /// Retorna si el Convenio Ultimo actual se corresponde con los indicados. Los valores deben estar separados por punto y coma (;).
        /// </summary>        
		public bool EsConvenioUlt(string pConvenios) {
			string strValores = ";" + pConvenios.ToUpper() + ";";
			return strValores.IndexOf(";" + ConvenioUlt().ToUpper() + ";") >= 0;
		}
		
        /// <summary>
        /// Retorna el Convenio Último en la persona.
		/// El query se cachea y comparte la clave con CategoriaUlt.
        /// </summary>		
        public string CategoriaUlt() {
			return ConvenioCategoriaUlt(false);
		}

        /// <summary>
        /// Retorna si la Categoria Ultimo actual se corresponde con los indicados. Los valores deben estar separados por punto y coma (;).
        /// </summary>        
		public bool EsCategoriaUlt(string pCategorias) {
			string strValores = ";" + pCategorias.ToUpper() + ";";
			return strValores.IndexOf(";" + CategoriaUlt().ToUpper() + ";") >= 0;
		}		

        /// <summary>
        /// El query se cachea y comparte la clave con ConvenioUlt y CategoriaUlt.
        /// </summary>		
        private string ConvenioCategoriaUlt(bool pConvenio) {
			NomadXML xmlResult;
            string strClave;
			string strRetorno;
			
			strClave = "PERCONCATEGULT-" + this.oi_personal_emp;
			
			xmlResult = (NomadXML)NomadProxy.GetProxy().CacheGetObj(strClave);

            if (xmlResult == null) {
				//El resultado del Query no está cacheado. Lo ejecuta y lo guarda

				NomadXML xmlQParams;

				//Crea el objeto de Parametros
				xmlQParams = new NomadXML("PARAM");
				xmlQParams.SetAttr("oi_personal_emp", this.oi_personal_emp);

                xmlResult = NomadEnvironment.QueryNomadXML(NucleusRH.Base.Personal.LegajoEmpresa.PERSONAL_EMP.Resources.QRY_CONCATEGULT, xmlQParams.ToString());

				xmlResult = xmlResult.FirstChild();
				
				NomadProxy.GetProxy().CacheAdd("strClave", xmlResult);
				
            }
            
			strRetorno = (pConvenio) ? "c_convenio" : "c_categoria";
			
			return xmlResult.GetAttr(strRetorno);
        }

        
        /// <summary>
        /// Retorna el Convenio Último del legajo.
		/// El query se cachea y comparte la clave con Empresa.
        /// </summary>		
        public string TipoPersonalUlt() {
			NomadXML xmlResult;
			xmlResult = GetDATOS_LEG();
			return xmlResult.GetAttr("C_TIPO_PERSONAL_ULT");
		}

        /// <summary>
        /// Retorna si el Tipo de Personal Ultimo actual se corresponde con los indicados. Los valores deben estar separados por punto y coma (;).
        /// </summary>        
		public bool EsTipoPersonalUlt(string pTiposPersonal) {
			string strValores = ";" + pTiposPersonal.ToUpper() + ";";
			return strValores.IndexOf(";" + TipoPersonalUlt().ToUpper() + ";") >= 0;
		}

        /// <summary>
        /// Retorna el Codigo de Empresa del legajo.
		/// El query se cachea y comparte la clave con TipoPersonalUlt.
        /// </summary>		
        public string Empresa() {
			NomadXML xmlResult;
			xmlResult = GetDATOS_LEG();
			return xmlResult.GetAttr("C_EMP");
		}

        /// <summary>
        /// Retorna si la Empresa del leajo se corresponde con los indicados. Los valores deben estar separados por punto y coma (;).
        /// </summary>        
		public bool EsEmpresa(string pEmpresas) {
			string strValores = ";" + pEmpresas.ToUpper() + ";";
			return strValores.IndexOf(";" + Empresa().ToUpper() + ";") >= 0;
		}
		
		/// <summary>
        /// El query se cachea el tipo de personal ULT y la empresa ULT.
        /// </summary>		
        private NomadXML GetDATOS_LEG() {
			NomadXML xmlResult;
            string strClave;
			
			strClave = "PERTIPOEMPULT-" + this.oi_personal_emp;
			
			xmlResult = (NomadXML)NomadProxy.GetProxy().CacheGetObj(strClave);

            if (xmlResult == null) {
				//El resultado del Query no está cacheado. Lo ejecuta y lo guarda

				NomadXML xmlQParams;

				//Crea el objeto de Parametros
				xmlQParams = new NomadXML("PARAM");
				xmlQParams.SetAttr("oi_personal_emp", this.oi_personal_emp);

                xmlResult = NomadEnvironment.QueryNomadXML(NucleusRH.Base.Personal.LegajoEmpresa.PERSONAL_EMP.Resources.DATOS_LEG_POR_OI, xmlQParams.ToString());

				xmlResult = xmlResult.FirstChild(); //Se ubica en ROWS
				
				xmlResult = xmlResult.FirstChild(); //Se ubica en ROW
				if (xmlResult == null) {
					Nomad.NSystem.Base.NomadAppException nmdAppE;
					nmdAppE = new Nomad.NSystem.Base.NomadAppException("No se encuentra el legajo con OI '" + this.oi_personal_emp + "'.");
					throw nmdAppE;
				}
				
				NomadProxy.GetProxy().CacheAdd("strClave", xmlResult);
				
            }
            
			return xmlResult;
        }
		
        /// <summary>
        /// Retorna el tipo de día.
        /// </summary>
        public string GetDayType(string perid, DateTime fecha)
        {
            ArrayList dia = NucleusRH.Base.Tiempos_Trabajados.Esperanzaper.ESPERANZAPER.GetDaysHope(fecha, fecha, int.Parse(perid));

            if (dia.Count == 0)
            {
                //NomadEnvironment.GetTraceBatch().Warning("El Legajo no tiene turno definido para el Dia "+fecha.ToString("dd/MM/yyyy"));
                return "I";
            }

            NucleusRH.Base.Tiempos_Trabajados.Esperanzaper.DIA ddoDIA = (NucleusRH.Base.Tiempos_Trabajados.Esperanzaper.DIA)dia[0];
            return ddoDIA.c_tipo;
        }

        /// <summary>
        /// Indica si el día es Feriado.
        /// </summary>
        public bool isDayFeriado(string perid, DateTime fecha)
        {
            return (GetDayType(perid, fecha) == "F" || GetDayType(perid, fecha) == "DF");
        }

        public string GetEmpresa(string perid, string liqid)
        {
            Hashtable retval = (System.Collections.Hashtable)NomadProxy.GetProxy().CacheGetObj("PEREMPLIQ");

            if (retval == null)
            {
                retval = new Hashtable();
                //Ejecutando el QUERY
                NomadXML MyXML = NomadEnvironment.QueryNomadXML(NucleusRH.Base.Tiempos_Trabajados.Liquidacion_Personas.LIQUIDACIONPERS.Resources.QRY_PERSONAL_EMP_LIQ, "<DATO oi_liquidacion=\"" + liqid + "\"/>");

                //Cargando la Lista
                retval = new System.Collections.Hashtable();
                for (NomadXML cur = MyXML.FindElement("ROWS").FirstChild(); cur != null; cur = cur.Next())
                    if (cur.Name.ToUpper() == "ROW")
                        retval[cur.GetAttrInt("oi_personal_emp")] = cur.GetAttr("c_empresa");
                NomadProxy.GetProxy().CacheAdd("PEREMPLIQ", retval);
            }
            //Existe
            if (retval.ContainsKey(int.Parse(perid)))
                return retval[int.Parse(perid)].ToString();

            //No encontrado
            return "";
        }

		/// <summary>
		/// Obtiene los datos del procesamiento que se quiere procesar
		/// </summary>
		/// <returns></returns>
		private NomadXML GetDatosProcesamiento() {
			NomadXML xmlResult;
			NomadXML xmlQParams;

			//Crea el objeto de Parametros
			xmlQParams = new NomadXML("DATOS");
			xmlQParams.SetAttr("oi_Liquidacion", this.oi_liquidacion);

			//Ejecuta el query CACHEABLE
			xmlResult = NomadEnvironment.QueryNomadXML(Base.Tiempos_Trabajados.Liquidacion.LIQUIDACION.Resources.QRY_DatosLiquidacion, xmlQParams.ToString(), true);

			return xmlResult.FirstChild();
		}		
        /// <summary>
        /// Retorna una hora “Redondeada” según los parámetros rango1 y rango2.
        /// </summary>
		public double Redondear(double cant, double rango1, double rango2)
        {
            double valred = 0.5;
            double cantred = Math.Floor(cant);
            double cantmins = Math.Round((cant - Math.Floor(cant)) * 60, 0);

            if (cantmins < rango1)
                valred = 0d;
            if (cantmins >= rango2)
                valred = 1;

            return cantred + valred;
        }

        public static void AdministrarLegajos(Nomad.NSystem.Document.NmdXmlDocument param, string liquidacion)
        {
            //ARMO UN "HASHTABLE" CON TODAS LAS PERSONAS QUE ESTAN EN EL PROCESAMIENTO
            Hashtable htliq = new Hashtable();
            htliq = NomadEnvironment.QueryHashtable(NucleusRH.Base.Tiempos_Trabajados.Liquidacion_Personas.LIQUIDACIONPERS.Resources.QRY_ADMINISTRAR, "<DATO oi_liquidacion=\"" + liquidacion + "\"/>", "oi_personal_emp");

            //GUARDO EL XML QUE ENTRA EN UN NOMADXML
            NomadXML xmldoc = new NomadXML(param.ToString());
            xmldoc = xmldoc.FirstChild();

            //Cargo los ids en un nuevo hashtable
            Hashtable htids = new Hashtable();
            for (NomadXML xmlCur = xmldoc.FirstChild(); xmlCur != null; xmlCur = xmlCur.Next())
            {
                string[] strids = xmlCur.GetAttr("VALUES").Split(',');
                foreach (string s in strids)
                    htids.Add(s, "");
            }

            //INICIO UNA TRANSACCION
            NomadEnvironment.GetCurrentTransaction().Begin();
            try
            {
                //RECORRO LA HASH de LIQ Y PREGUNTO SI EN ELLA HAY IDS QUE NO ESTAN EN LA HASH DE IDs
                foreach (string value in htliq.Keys)
                {
                    if (!htids.ContainsKey(value))
                    {
                        NucleusRH.Base.Tiempos_Trabajados.Liquidacion_Personas.LIQUIDACIONPERS ddoLIQPER;
                        ddoLIQPER = NucleusRH.Base.Tiempos_Trabajados.Liquidacion_Personas.LIQUIDACIONPERS.Get(((NomadXML)htliq[value]).GetAttr("oi_liquidacionpers"));
                        NomadEnvironment.GetCurrentTransaction().Delete(ddoLIQPER);
                    }
                }

                foreach (string value in htids.Keys)
                {
                    if (!htliq.ContainsKey(value))
                    {
                        NucleusRH.Base.Tiempos_Trabajados.Liquidacion_Personas.LIQUIDACIONPERS ddoLIQPER;
                        ddoLIQPER = new NucleusRH.Base.Tiempos_Trabajados.Liquidacion_Personas.LIQUIDACIONPERS();
                        ddoLIQPER.oi_liquidacion = liquidacion;
                        ddoLIQPER.oi_personal_emp = value;
                        NomadEnvironment.GetCurrentTransaction().Save(ddoLIQPER);
                    }
                }

                //COMMIT
                NomadEnvironment.GetCurrentTransaction().Commit();

                //CARGO EL PROCESAMIENTO PARA SETEARLE EL ESTADO SEGUN LAS PERSONAS ASOCIADAS AL MISMO
                NucleusRH.Base.Tiempos_Trabajados.Liquidacion.LIQUIDACION ddoLIQ;
                ddoLIQ = NucleusRH.Base.Tiempos_Trabajados.Liquidacion.LIQUIDACION.Get(liquidacion);

                //TIRO UN QRY PARA SABER CUANTAS PERSONAS HAY EN EL PROCESAMIENTO
                NomadXML xmlcount;
                string qryparam = "<DATO oi_liquidacion=\"" + liquidacion + "\"/>";
                xmlcount = new NomadXML();
                xmlcount.SetText(NomadEnvironment.QueryString(NucleusRH.Base.Tiempos_Trabajados.Liquidacion_Personas.LIQUIDACIONPERS.Resources.QRY_CONT_PERSONAS, qryparam));
                NomadEnvironment.GetTrace().Info(xmlcount.ToString());

                //SI LA CANTIDAD ES 0 SETEO EL ESTADO EN ABIERTO
                if (xmlcount.FirstChild().GetAttr("count") == "0")
                    ddoLIQ.c_estado = "abi";
                //CASO CONTRARIO EL ESTADO ES INICIALIZADO
                else
                    ddoLIQ.c_estado = "ini";

                //GUARDO
                NomadEnvironment.GetCurrentTransaction().Save(ddoLIQ);


            }
            catch
            {
                NomadEnvironment.GetCurrentTransaction().Rollback();
                throw;
            }
        }
        public static bool EnLiquidacionCerrada(string par_Id, DateTime par_fecha)
        {
            //PASO LOS PARAMETRO A UN QRY PARA CHEQUEAR QUE EL OI_PERSONAL_EMP NO ESTE EN ALGUNA LIQUIDACION CERRADA
            //CON FEHCHA DE FIN MAYOR A LA FECHA QUE ENTRA

            string param = "<DATO fecha=\"" + Nomad.NSystem.Functions.StringUtil.date2str(par_fecha) + "\" oi_personal_emp=\"" + par_Id + "\"/>";

            NomadEnvironment.GetTrace().Info("docparam -- " + param.ToString());

            Nomad.NSystem.Document.NmdXmlDocument docFLAG = null;
            docFLAG = new Nomad.NSystem.Document.NmdXmlDocument(NomadEnvironment.QueryString(NucleusRH.Base.Tiempos_Trabajados.Liquidacion_Personas.LIQUIDACIONPERS.Resources.QRY_EN_LIQUIDACION, param));

            NomadEnvironment.GetTrace().Info("docFLAG -- " + docFLAG.ToString());

            bool resul = docFLAG.GetAttribute("flag").Value == "1" ? true : false;
            NomadEnvironment.GetTrace().Info("resul -- " + resul.ToString());
            return resul;
        }
        public static void EjecutarLiquidacion(int pintOi_Liquidacion, Nomad.NSystem.Document.NmdXmlDocument pobjParams)
        {
            string strResult = "";
            NucleusRH.Base.Tiempos_Trabajados.Procesos objMain;

            NomadBatch.Trace("Comienza la llamada al PROCESAMIENTO.");

            //Crea el objeto que tiene los métodos principales
            objMain = new NucleusRH.Base.Tiempos_Trabajados.Procesos();

            strResult = objMain.GenerarProcesamiento(pintOi_Liquidacion.ToString(), pobjParams.ToString());
        }
        public static bool LiquidacionOK(string pOI_Personal_Emp, string pOI_Liquidacion)
        {
            NomadEnvironment.GetTrace().Info("**** Comienza el método LiquidacionOk ****");

            Nomad.NSystem.Base.NomadAppException nmdAppE;

            //Obtiene el query a ejecutar desde los recursos
            string strQuery = NucleusRH.Base.Tiempos_Trabajados.Liquidacion_Personas.LIQUIDACIONPERS.Resources.QRY_VERSIONES;

            //Genera el XML de parametros
            string strParametros = "<DATO oi_personal_emp=\"" + pOI_Personal_Emp + "\" oi_liquidacion=\"" + pOI_Liquidacion + "\"/>";

            //Ejecuta el query
            SQLService sqlService = NomadProxy.GetProxy().SQLService();
            string strResultado = sqlService.Get(strQuery, strParametros);


            if (strResultado.EndsWith("/>"))
            {
                nmdAppE = new Nomad.NSystem.Base.NomadAppException("El personal no pertenece a la liquidación o la liquidación no existe.");
                throw nmdAppE;
            }

            XmlDocument xmlResultado = new XmlDocument();
            xmlResultado.LoadXml(strResultado);

            //Obtiene los valores
            string strLiq_ver = ((XmlElement)xmlResultado.DocumentElement.ChildNodes[0]).GetAttribute("liq_ver");
            string strPer_ver = ((XmlElement)xmlResultado.DocumentElement.ChildNodes[0]).GetAttribute("per_ver");
            string strLegajo = ((XmlElement)xmlResultado.DocumentElement.ChildNodes[0]).GetAttribute("e_numero_legajo");

            //Realiza las validaciones--------------------------------------------

            if (strPer_ver == "")
            {
                nmdAppE = new Nomad.NSystem.Base.NomadAppException("El personal con legajo '" + strLegajo + "' no tiene una versión válida.");
                throw nmdAppE;
            }

            if (strLiq_ver != strPer_ver)
            {
                nmdAppE = new Nomad.NSystem.Base.NomadAppException("La liquidación se generó con una versión anterior del personal con legajo '" + strLegajo + "'.");
                throw nmdAppE;
            }

            return true;
        }
        public static void SaveManual(string repid)
        {
            NomadXML xmlDatos = NomadEnvironment.GetProxy().FileServiceIO().LoadFileXML("TEMP", "Procesamiento-" + repid + ".xml");
            NomadLog.Debug("xmlDatos -- " + xmlDatos.ToString());

            string idliq = xmlDatos.FirstChild().FindElement("LEG").GetAttr("oi_liquidacion");
            string idper = xmlDatos.FirstChild().FindElement("LEG").GetAttr("oip");

            //OBTIENE EL PER LIQ
            string idPerLiq = NomadEnvironment.QueryValue("TTA10_LIQUIDACPERS", "oi_liquidacionpers", "oi_liquidacion", idliq, "TTA10_LIQUIDACPERS.oi_personal_emp = " + idper, false);
            LIQUIDACIONPERS ddoLP = LIQUIDACIONPERS.Get(idPerLiq);
            NomadLog.Debug("ddoLP 0 -- " + ddoLP.SerializeAll());

            ArrayList todel = new ArrayList();
            Hashtable ht = new Hashtable();

            //RECORRE LOS CONCEPTOS DE LA PER (BUSCA CUALES MODIFICAR Y CUALES ELIMINAR
            foreach (CONC_VAL ddoCONP in ddoLP.CONC_PER)
            {
                if (ddoCONP.e_claveNull) continue;
                NomadXML xmldet = xmlDatos.FindElement("DETALLE").FindElement2("DET", "fec", ddoCONP.e_clave.ToString());
                if (xmldet == null) continue;
                //PREGUNTA SI EL CONCEPTO FUE MODIFICADO
                if (xmldet.GetAttr("chg").Contains(",conc_" + ddoCONP.oi_concepto + ","))
                {
                    string val = xmldet.GetAttr("conc_" + ddoCONP.oi_concepto);
                    if (val != "")
                    {
                        //TIENE CARGADO VALOR POR LO TANTO ES UNA MODIFICACION
                        if (val.Contains(":"))
                        {
                            double hh = int.Parse(val.Split(':')[0]);
                            double mm = int.Parse(val.Split(':')[1]);

                            ddoCONP.n_valor = Math.Round(hh + (mm / 60d), 2);
                        }
                        else
                            ddoCONP.n_valor = Convert.ToDouble(val);
                    }
                    else
                    {
                        //EL VALOR FUE BORRADO, HAY QUE ELIMINAR EL REGISTRO
                        todel.Add(ddoCONP);
                    }
                }
                //GUARDA LOS REGISTROS ANALIZADOS (MODIFICADOS Y  ELIMINADOS)
                ht["conc_" + ddoCONP.oi_concepto + ddoCONP.e_clave] = "";
            }


            //RECORRE LOS DETALLES DEL ARCHIVO QUE VIENE DE AFUERA
            for (NomadXML xmldet = xmlDatos.FindElement("DETALLE").FirstChild(); xmldet != null; xmldet = xmldet.Next())
            {
                //OBTIENE LOS MARCADOS COMO CAMBIO
                if (xmldet.GetAttr("chg") != "")
                {
                    //ARMA LA LISTA DE CONCEPTOS MODIFICADOS
                    string[] concs = xmldet.GetAttr("chg").Split(',');
                    foreach (string strcon in concs)
                    {
                        if (strcon == "") continue;
                        string con = strcon.Substring(strcon.IndexOf('_') + 1);

                        //SI EL CONCEPTO YA FUE TRATADO ANTES CONTINUA CON EL SIGUIENTE
                        if (ht.ContainsKey("conc_" + con + xmldet.GetAttr("fec"))) continue;

                        //AGREGA EL CONCEPTO
                        CONC_VAL ddoNCP = new CONC_VAL();
                        ddoNCP.oi_concepto = con;
                        ddoNCP.e_clave = int.Parse(xmldet.GetAttr("fec"));
                        ddoNCP.n_valor = 0;

                        string val = xmldet.GetAttr("conc_" + con);
                        if (val != "")
                        {
                            if (val.Contains(":"))
                            {
                                double hh = int.Parse(val.Split(':')[0]);
                                double mm = int.Parse(val.Split(':')[1]);
                                ddoNCP.n_valor = Math.Round(hh + (mm / 60d), 2);
                            }
                            else
                                ddoNCP.n_valor = Convert.ToDouble(val);
                        }

                        NomadLog.Debug("ADD -- " + ddoNCP.SerializeAll());
                        ddoLP.CONC_PER.Add(ddoNCP);
                    }
                }
            }

            //BORRA LOS OBJETOS
            foreach (CONC_VAL ddoCPDEL in todel)
            {
                ddoLP.CONC_PER.Remove(ddoCPDEL);
            }

            //CONCEPTOS MENSUALES
            for (NomadXML xmlmen = xmlDatos.FindElement("MENSUAL").FirstChild(); xmlmen != null; xmlmen = xmlmen.Next())
            {
                NomadLog.Debug("xmlmen -- " + xmlmen.ToString());
                //OBTIENE LOS MARCADOS COMO CAMBIO
                if (xmlmen.GetAttr("chg") == "") continue;

                NomadLog.Debug("oi_concepto -- " + xmlmen.GetAttr("oi_concepto"));

                string idconval = NomadEnvironment.QueryValue("TTA10_CONC_PER", "oi_conc_val", "oi_liquidacionpers", idPerLiq, "TTA10_CONC_PER.oi_concepto = " + xmlmen.GetAttr("oi_concepto") + " AND ISNULL(TTA10_CONC_PER.e_clave)", false);
                NomadLog.Debug("//LA PERSONA CONCEPTO -- " + idconval);

                CONC_VAL ddoCONP;
                if (ddoLP.CONC_PER.GetById(idconval) != null)
                {
                    //LA PERSONA TIENE EL CONCEPTO
                    ddoCONP = (CONC_VAL)ddoLP.CONC_PER.GetById(idconval);
                    if (xmlmen.GetAttr("cant") != "") //MODIFICA
                    {
                        NomadLog.Debug("MODIFICA-- ");
                        if (xmlmen.GetAttr("c_unidad") == "horas")
                        {
                            double hh = int.Parse(xmlmen.GetAttr("cant").Split(':')[0]);
                            double mm = int.Parse(xmlmen.GetAttr("cant").Split(':')[1]);
                            ddoCONP.n_valor = Math.Round(hh + (mm / 60d), 2);
                        }
                        else ddoCONP.n_valor = xmlmen.GetAttrDouble("cant");
                    }
                    else
                    {
                        NomadLog.Debug("ELIMINA-- ");
                        ddoLP.CONC_PER.Remove(ddoCONP); //ELIMINA
                    }
                }
                else if (xmlmen.GetAttr("cant") != "")
                {
                    NomadLog.Debug("AGREGA-- ");
                    //AGREGA
                    ddoCONP = new CONC_VAL();
                    if (xmlmen.GetAttr("c_unidad") == "horas")
                    {
                        double hh = int.Parse(xmlmen.GetAttr("cant").Split(':')[0]);
                        double mm = int.Parse(xmlmen.GetAttr("cant").Split(':')[1]);
                        ddoCONP.n_valor = Math.Round(hh + (mm / 60d), 2);
                    }
                    else ddoCONP.n_valor = xmlmen.GetAttrDouble("cant");
                    ddoCONP.oi_concepto = xmlmen.GetAttr("oi_concepto");

                    ddoLP.CONC_PER.Add(ddoCONP);
                }

            }

            NomadLog.Debug("ddoLP FIN -- " + ddoLP.SerializeAll());

            ddoLP.l_manual = true;
            NomadEnvironment.GetCurrentTransaction().Save(ddoLP);
        }
    }
}



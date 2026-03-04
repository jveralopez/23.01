using System;
using System.Xml;
using System.Collections;
using System.Text;
using System.Reflection;

using Nomad.NSystem.Functions;
using Nomad.NSystem.Proxy;
using Nomad.NSystem.Base;
using NucleusRH.Base.Tiempos_Trabajados;

namespace NucleusRH.Base.Tiempos_Trabajados.RHLiq
{
    public class LiqConceptosBase
    {
        protected NomadProxy proxy;
        protected NomadTrace trace;
        protected NomadTrace traceInt;
        protected Liquidacion.LIQUIDACION myliq;
        protected Personal.PERSONAL_EMP myper;
        protected Liquidacion_Personas.LIQUIDACIONPERS myperliq;
        protected NomadXML variables;
        protected Hashtable Values;
		protected Hashtable BancosFC;
		protected DateTime FechaActual;
		
        public string MD5HASH;

        public LiqConceptosBase()
        {
            this.proxy = NomadProxy.GetProxy();
            this.traceInt = NomadEnvironment.GetTrace();
            this.trace = NomadEnvironment.GetTraceBatch();
            this.Values = new Hashtable();
            this.variables = new NomadXML(proxy.SQLService().Get(Variables.VARIABLE.Resources.qry_variables, "")).FirstChild();
        }

        public void InitializeLiquidacion(Liquidacion.LIQUIDACION myliq)
        {
            this.myliq = myliq;
            IniciarLiquidacion();
        }

        public void InitializePersona(Personal.PERSONAL_EMP myper, Liquidacion_Personas.LIQUIDACIONPERS myperliq)
        {
            this.myper = myper;
            this.myperliq = myperliq;
            IniciarPersona();
        }

        //Iniciar las Variables....
        protected void _InitVar(NomadXML myVAR)
        {
            Object Value = null;

            if (myVAR.GetAttrBool("l_colleccion"))
            {
                Value = new Hashtable();
            }
            else
            {
                switch (myVAR.GetAttr("c_tipo_dato").ToUpper())
                {
                    case "BOOL":
                        Value = myVAR.GetAttrBool("n_valor");
                        break;

                    case "INT":
                        Value = myVAR.GetAttrInt("n_valor");
                        break;

                    case "DOUBLE":
                        Value = myVAR.GetAttrDouble("n_valor");
                        break;

                    case "DATETIME":
                        Value = myVAR.GetAttrDateTime("n_valor");
                        break;
                }
            }

            this.Values[myVAR.GetAttr("c_variable")] = Value;
        }

        //Setear Valores
        protected void _SetVar(NomadXML myVAR, int key, double valor)
        {
            Object Value = null;

            switch (myVAR.GetAttr("c_tipo_dato").ToUpper())
            {
                case "BOOL":
                    Value = (valor == 0 ? false : true);
                    break;

                case "INT":
                    Value = System.Convert.ToInt32(valor);
                    break;

                case "DOUBLE":
                    Value = System.Math.Round(valor, 3);
                    break;
            }

            if (myVAR.GetAttrBool("l_colleccion"))
            {
                ((Hashtable)Values[myVAR.GetAttr("c_variable")])[key] = Value;
            }
            else
                Values[myVAR.GetAttr("c_variable")] = Value;
        }

        protected void _SetVar(NomadXML myVAR, DateTime key, double valor)
        {
            _SetVar(myVAR, int.Parse(key.ToString("yyyyMMdd")), valor);
        }

        protected void _SetVar(NomadXML myVAR, double valor)
        {
            _SetVar(myVAR, 0, valor);
        }

        protected void _SetVar(string code, object valor)
        {
            Values[code] = valor;
        }

        protected double _GetVarDOUBLE(string code)
        {
            return (double)Values[code];
        }

        protected int _GetVarINT(string code)
        {
            return (int)Values[code];
        }

        protected System.DateTime _GetVarDATETIME(string code)
        {
            return (System.DateTime)Values[code];
        }

        protected bool _GetVarBOOL(string code)
        {
            return (bool)Values[code];
        }

        protected Hashtable _GetVarHASHTABLE(string code)
        {
            return (Hashtable)Values[code];
        }

        protected double _GetVar(NomadXML myVAR, int varKey)
        {
            Object Value;

            if (myVAR.GetAttrBool("l_colleccion"))
                Value = ((Hashtable)this.Values[myVAR.GetAttr("c_variable")])[varKey];
            else
                Value = this.Values[myVAR.GetAttr("c_variable")];

            switch (myVAR.GetAttr("c_tipo_dato").ToUpper())
            {
                case "DOUBLE":
                    return (double)Value;
                case "INT":
                    return System.Convert.ToDouble((int)Value);
                case "BOOL":
                    return ((bool)Value ? 1d : 0d);
            }

            return 0d;
        }

        protected double _GetVar(NomadXML myVAR)
        {
            return _GetVar(myVAR, 0);
        }

        protected Hashtable _GetVarCollection(NomadXML myVAR)
        {
            if (myVAR.GetAttrBool("l_colleccion"))
                return ((Hashtable)this.Values[myVAR.GetAttr("c_variable")]);

            return null;
        }

        public void IniciarLiquidacion()
        {
            NomadXML myVAR;

            //Limpiamos todas las variables
            for (myVAR = variables.FirstChild(); myVAR != null; myVAR = myVAR.Next())
                _InitVar(myVAR);

            //Iniciamos las variables de Novedad de Liquidacion
            foreach (Liquidacion.VAL_VAR varval in myliq.VAL_VAR)
            {
                myVAR = this.variables.FindElement2("ROW", "id", varval.oi_variable);
                if (myVAR == null) continue;
                if (myVAR.GetAttr("c_tipo_variable") != "LIQNOV") continue;

                _SetVar(myVAR, varval.e_clave, varval.n_valor);
            }
        }

        public void IniciarPersona()
        {
            NomadXML myVAR;

            //Iniciamos las variables Fijas de Personas
            foreach (Personal.VAL_VAR varval in myper.VAL_VAR)
            {
                myVAR = variables.FindElement2("ROW", "id", varval.oi_variable);
                if (myVAR == null) continue;
                if (myVAR.GetAttr("c_tipo_variable") != "PERFIJ") continue;

                _SetVar(myVAR, varval.e_clave, varval.n_valor);
            }

            //Iniciamos las variables de Acumuladores de Personas
            foreach (Personal.VAL_VAR varval in myper.VAL_VAR)
            {
                myVAR = variables.FindElement2("ROW", "id", varval.oi_variable);
                if (myVAR == null) continue;
                if (myVAR.GetAttr("c_tipo_variable") != "PERACC") continue;

                _SetVar(myVAR, varval.e_clave, varval.n_valor);
            }

            //Iniciamos las variables de Novedad de Personas
            foreach (Liquidacion_Personas.VAL_VAR varval in myperliq.VAL_VAR)
            {
                myVAR = variables.FindElement2("ROW", "id", varval.oi_variable);
                if (myVAR == null) continue;
                if (myVAR.GetAttr("c_tipo_variable") != "PERNOV") continue;

                _SetVar(myVAR, varval.e_clave, varval.n_valor);
            }
			
			this.BancosFC = null;
			SetCurrent(this);
        }
		
		public void SaveBancosFC() {
			if (this.BancosFC == null) return;
			
			foreach (NucleusRH.Base.Tiempos_Trabajados.Personal.clsBancoFC objBancoFC in this.BancosFC.Values) {
				objBancoFC.SaveData();
			}
		}
		
        public static NucleusRH.Base.Tiempos_Trabajados.Personal.clsBancoFC GetBancoFC(string pstrBanco) {
			NucleusRH.Base.Tiempos_Trabajados.Personal.clsBancoFC objBanco;
			
			LiqConceptosBase MyADIC = GetCurrent();
			if (MyADIC.BancosFC == null) {
				MyADIC.BancosFC = new Hashtable();
			}
			
			if (!MyADIC.BancosFC.ContainsKey(pstrBanco))
				MyADIC.BancosFC[pstrBanco] = new NucleusRH.Base.Tiempos_Trabajados.Personal.clsBancoFC(int.Parse(MyADIC.myperliq.oi_personal_emp), pstrBanco, MyADIC.myliq.f_fechainicio, MyADIC.myliq.f_fechafin.AddDays(1));
			
			objBanco = (NucleusRH.Base.Tiempos_Trabajados.Personal.clsBancoFC) MyADIC.BancosFC[pstrBanco];
			objBanco.SetFecha(MyADIC.FechaActual);
			
			return objBanco; 
		}
		
		public void IniciarVariableHoras(Liquidacion_Personas.LIQUIDACJOR myJOR)
        {
            NomadXML myVAR;
            double value;

            //Iniciamos las variables Fijas de Personas
            for (myVAR = variables.FirstChild(); myVAR != null; myVAR = myVAR.Next())
            {
                if (myVAR.GetAttr("c_tipo_variable") != "TIPHOR") continue;
                value = 0;

                if (myJOR != null)
                {
                    foreach (Liquidacion_Personas.LIQUIDACIONPROC proc in myJOR.LIQUIDACPROC)
                    {
                        if (proc.oi_tipohora != myVAR.GetAttr("oi_tipohora")) continue;
                        value += proc.n_cantidadhs;
                    }
                }
                else
                {
                    foreach (Liquidacion_Personas.LIQUIDACJOR xxJOR in myperliq.LIQUIDACJOR)
                        foreach (Liquidacion_Personas.LIQUIDACIONPROC proc in xxJOR.LIQUIDACPROC)
                        {
                            if (proc.oi_tipohora != myVAR.GetAttr("oi_tipohora")) continue;
                            value += proc.n_cantidadhs;
                        }
                }
                _SetVar(myVAR, value);
            }
        }

        public Liquidacion_Personas.CONC_VAL Find_CONC_PER(Liquidacion_Personas.LIQUIDACIONPERS PER, string ConcId, int Clave)
        {
            foreach (Liquidacion_Personas.CONC_VAL CONC in PER.CONC_PER)
            {
                if (CONC.oi_concepto == ConcId && CONC.e_clave == Clave)
                    return CONC;
            }

            return null;
        }

        virtual public void ApplyConceptos(Liquidacion.LIQUIDACION myliq, Personal.PERSONAL_EMP myper, Liquidacion_Personas.LIQUIDACIONPERS myperliq, ArrayList arrayHope)
        {
            throw new Exception("NO IMPLEMENTADO");
        }

        static bool BuscarConcepto(string MyDLLCode, int lineTotal, ref string conceptoId, ref int lineDif)
        {
            int lineTest;
            string[] lineas = MyDLLCode.Split('\n');

            for (lineTest = lineTotal; lineTest > 0; lineTest--)
            {
                lineas[lineTest] = lineas[lineTest].Replace("\t", " ");
                lineas[lineTest] = lineas[lineTest].Replace("\r", " ");
                lineas[lineTest] = lineas[lineTest].Replace("\n", " ");
                lineas[lineTest] = lineas[lineTest].Trim();

                if (lineas[lineTest].StartsWith("//ENDCONC:")) return false;

                if (lineas[lineTest].StartsWith("//BEGCONC:"))
                {
                    conceptoId = lineas[lineTest].Substring(10).Trim();
                    lineDif = lineTotal - lineTest - 3;
                    return true;
                }
            }

            return false;
        }

        static public LiqConceptosBase GetObject()
        {
            int t;
            string MyDLLCode = "";
            string conCALLMES = "";
            string conCALLDIA = "";

            /////////////////////////////////////////////////////////////////////////////////////
            // Genero el Codigo
            NomadXML CONCEPTOS = (new NomadXML(NomadEnvironment.GetProxy().SQLService().Get(Conceptos.CONCEPTO.Resources.qry_conceptos, ""))).FirstChild();
            for (NomadXML myCUR = CONCEPTOS.FirstChild(); myCUR != null; myCUR = myCUR.Next())
            {
                string VAR_CALL = "";
                string VAR_DECLARE = "";
                string VAR_READ = "";
                string VAR_SET = "";

                if (myCUR.GetAttr("coll").ToUpper() == "NO")
                    conCALLMES += "CALL_CONC_" + myCUR.GetAttr("code") + "(myperliq,null,null);\r\n";
                else
                    if (myCUR.GetAttr("coll").ToUpper() == "FECHA")
                        conCALLDIA += "CALL_CONC_" + myCUR.GetAttr("code") + "(myperliq,jor,(Esperanzaper.DIA)dias[jor.f_fecjornada]);\r\n";

                for (NomadXML myVAR = myCUR.FirstChild(); myVAR != null; myVAR = myVAR.Next())
                {
                    if (myVAR.GetAttr("var_coll") == "1")
                    {
                        VAR_CALL += ", " + myVAR.GetAttr("variable");
                        VAR_DECLARE += ", Hashtable " + myVAR.GetAttr("variable");
                        VAR_READ += "Hashtable " + myVAR.GetAttr("variable") + "=_GetVarHASHTABLE(\"" + myVAR.GetAttr("variable") + "\");\r\n";
                        VAR_SET += "_SetVar(\"" + myVAR.GetAttr("variable") + "\", " + myVAR.GetAttr("variable") + ");\r\n";
                    }
                    else
                    {
                        VAR_CALL += ", " + (myVAR.GetAttr("var_mode") == "IN" ? "" : "ref ") + myVAR.GetAttr("variable");
                        VAR_DECLARE += ", " + (myVAR.GetAttr("var_mode") == "IN" ? "" : "ref ") + myVAR.GetAttr("var_type") + " " + myVAR.GetAttr("variable");
                        VAR_READ += myVAR.GetAttr("var_type") + " " + myVAR.GetAttr("variable") + "=_GetVar" + myVAR.GetAttr("var_type").ToUpper() + "(\"" + myVAR.GetAttr("variable") + "\");\r\n";
                        VAR_SET += "_SetVar(\"" + myVAR.GetAttr("variable") + "\", " + myVAR.GetAttr("variable") + ");\r\n";
                    }
                }

                string MyMETHOD = Liquidacion.LIQUIDACION.Resources.liqMethodTmpl;
                MyMETHOD = MyMETHOD.Replace("{id}", myCUR.GetAttr("id"));
                MyMETHOD = MyMETHOD.Replace("{code}", myCUR.GetAttr("code"));
                MyMETHOD = MyMETHOD.Replace("{formula}", myCUR.GetAttr("formula"));

                MyMETHOD = MyMETHOD.Replace("{VAR_DECLARE}", VAR_DECLARE);
                MyMETHOD = MyMETHOD.Replace("{VAR_READ}", VAR_READ);
                MyMETHOD = MyMETHOD.Replace("{VAR_SET}", VAR_SET);
                MyMETHOD = MyMETHOD.Replace("{VAR_CALL}", VAR_CALL);

                MyDLLCode += MyMETHOD;
            }

            MyDLLCode = Liquidacion.LIQUIDACION.Resources.liqConceptosTmpl.Replace("{conCONCEPTO}", MyDLLCode);
            MyDLLCode = MyDLLCode.Replace("{conCALLDIA}", conCALLDIA);
            MyDLLCode = MyDLLCode.Replace("{conCALLMES}", conCALLMES);

            /////////////////////////////////////////////////////////////////////////////////////
            // Genero la DLL
            LiqConceptosBase retval = new LiqConceptosBase();

            string dllMAIN = retval.GetType().Assembly.GetFiles()[0].Name;
            string dllPATH;
            string dllTRG;
            dllPATH = dllMAIN.Substring(0, dllMAIN.LastIndexOf("\\"));
            dllMAIN = dllMAIN.Substring(dllMAIN.LastIndexOf("\\") + 1);

            dllTRG = "TTAConcepto_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".DLL";
            NomadEnvironment.GetTrace().Info(MyDLLCode);

            string[] libs = (dllMAIN + ";NomadObject.dll;NomadBase.dll;NomadDocument.dll;NomadProxy.dll").Split(';');
            for (t = 0; t < libs.Length; t++)
                libs[t] = dllPATH + "\\" + libs[t];

            XmlDocument xmlResult = NCompiler.Compiler.Compile(MyDLLCode, dllPATH, dllTRG, libs);
            if (xmlResult != null)
            {
                Nomad.NSystem.Proxy.NomadXML X = new Nomad.NSystem.Proxy.NomadXML();
                Nomad.NSystem.Proxy.NomadXML E;
                X.SetText(xmlResult.OuterXml);

                for (E = X.FirstChild().FirstChild().FirstChild(); E != null; E = E.Next())
                {
                    string conceptoId = "";
                    int lineTotal, lineDif = 0;

                    lineTotal = int.Parse(E.GetAttr("line"));
                    if (!BuscarConcepto(MyDLLCode, lineTotal, ref conceptoId, ref lineDif))
                    {
                        NomadEnvironment.GetTrace().Error("Error de Generacion: " + E.GetAttr("desc") + " - " + E.GetAttr("line"));
                        NomadEnvironment.GetTraceBatch().Error("Error de Generacion: " + E.GetAttr("desc") + " - " + E.GetAttr("line"));
                    }
                    else
                    {
                        NomadEnvironment.GetTrace().Error("Error en el Concepto " + conceptoId + " - Linea: " + lineDif.ToString() + " - " + E.GetAttr("desc"));
                        NomadEnvironment.GetTraceBatch().Error("Error en el Concepto " + conceptoId + " - Linea: " + lineDif.ToString() + " - " + E.GetAttr("desc"));
                    }
                }
                return null;
            }

            System.Reflection.Assembly MyLIQCode = System.Reflection.Assembly.LoadFrom(dllPATH + "\\" + dllTRG);
            Type typeClass = MyLIQCode.GetType("NucleusRH.Base.Tiempos_Trabajados.RHLiq.LiqConceptos");

            LiqConceptosBase retDLL = (LiqConceptosBase)typeClass.InvokeMember(null, System.Reflection.BindingFlags.CreateInstance, null, null, null);
            retDLL.MD5HASH = Nomad.NSystem.Functions.StringUtil.GetMD5(MyDLLCode);
            return retDLL;
        }

        public void ActualizarAcumuladores()
        {
            NomadXML myVAR;
            ArrayList MyAcc = new ArrayList();

            //Limpio las Variables de Acumuladoras
            foreach (Liquidacion_Personas.VAL_VAR varval in myperliq.VAL_VAR)
            {
                myVAR = variables.FindElement2("ROW", "id", varval.oi_variable);
                if (myVAR == null) continue;
                if (myVAR.GetAttr("c_tipo_variable") == "PERACC") MyAcc.Add(varval);
            }
            foreach (Liquidacion_Personas.VAL_VAR varval in MyAcc)
            {
                myperliq.VAL_VAR.Remove(varval);
            }

            //Acualizamos los Acumuladores
            for (myVAR = variables.FirstChild(); myVAR != null; myVAR = myVAR.Next())
            {
                Hashtable values;
                double value;
                if (myVAR.GetAttr("c_tipo_variable") != "PERACC") continue;

                if (myVAR.GetAttrBool("l_colleccion"))
                {
                    values = _GetVarCollection(myVAR);
                    if (values != null && values.Count > 0)
                    {
                        foreach (int key in values.Keys)
                        {
                            value = _GetVar(myVAR, key);
                            if (value != 0)
                            {
                                Liquidacion_Personas.VAL_VAR varval = new Liquidacion_Personas.VAL_VAR();
                                varval.e_clave = key;
                                varval.n_valor = value;
                                varval.oi_variable = myVAR.GetAttr("id");

                                myperliq.VAL_VAR.Add(varval);
                            }
                        }
                    }

                }
                else
                {
                    value = _GetVar(myVAR);
                    if (value != 0)
                    {
                        Liquidacion_Personas.VAL_VAR varval = new Liquidacion_Personas.VAL_VAR();
                        varval.n_valor = value;
                        varval.oi_variable = myVAR.GetAttr("id");

                        myperliq.VAL_VAR.Add(varval);
                    }
                }
            }
        }

        /// <summary>
        /// Compara dos rangos de fecha / hora, y devuelve la interseccion entre ellos en minutos
        /// </summary>
        /// <param name="fdes">Fecha desde del rango a</param>
        /// <param name="fhas">Fecha hasta del rango a</param>
        /// <param name="rdes">Fecha desde del rango b</param>
        /// <param name="rhas">Fecha hasta del rango b</param>
        /// <returns></returns>
        public static int DateRange(DateTime ades, DateTime ahas, DateTime bdes, DateTime bhas)
        {
            if (ahas <= bdes || bhas <= ades) return 0;

            DateTime d = ades <= bdes ? bdes : ades;
            DateTime h = ahas <= bhas ? ahas : bhas;
            return (int)Math.Round((h - d).TotalMinutes);
        }

        /// <summary>
        /// Compara dos rangos de enteros, y devuelve la interseccion entre ellos
        /// </summary>
        /// <param name="ades">Entero desde del rango a</param>
        /// <param name="ahas">Entero hasta del rango a</param>
        /// <param name="bdes">Entero desde del rango b</param>
        /// <param name="bhas">Entero hasta del rango b</param>
        /// <returns></returns>
        public static int Range(int ades, int ahas, int bdes, int bhas)
        {
            if (ades > ahas || bdes > bhas) throw new Exception("Rango desde / hasta mal definido");

            if (ahas <= bdes || bhas <= ades) return 0;

            int d = ades <= bdes ? bdes : ades;
            int h = ahas <= bhas ? ahas : bhas;
            return h - d;
        }

        public const DayOfWeek Dom = DayOfWeek.Sunday;
        public const DayOfWeek Lun = DayOfWeek.Monday;
        public const DayOfWeek Mar = DayOfWeek.Tuesday;
        public const DayOfWeek Mie = DayOfWeek.Wednesday;
        public const DayOfWeek Jue = DayOfWeek.Thursday;
        public const DayOfWeek Vie = DayOfWeek.Friday;
        public const DayOfWeek Sab = DayOfWeek.Saturday;

        public static void Warning(string msg)
        {
            if (GetLegajo() != "")
                msg += ". - " + GetLegajo();
            if (GetFecha() != "")
                msg += " - " + GetFecha();
            if (GetConcepto() != "")
                msg += " - " + GetConcepto();

            NomadEnvironment.GetTraceBatch().Warning(msg);
        }

        public static void Error(string msg)
        {
            if (GetLegajo() != "")
                msg += ". - " + GetLegajo();
            if (GetFecha() != "")
                msg += " - " + GetFecha();
            if (GetConcepto() != "")
                msg += " - " + GetConcepto();

            NomadEnvironment.GetTraceBatch().Error(msg);
        }

        public static void Log(string msg)
        {
            if (GetLegajo() != "")
                msg += ". - " + GetLegajo();
            if (GetFecha() != "")
                msg += " - " + GetFecha();
            if (GetConcepto() != "")
                msg += " - " + GetConcepto();

            NomadEnvironment.GetTraceBatch().Info(msg);
        }

        protected static string GetLegajo()
        {
            return NomadProxy.GetProxy().CacheGet("LegProc");
        }
        protected static string GetFecha()
        {
            return NomadProxy.GetProxy().CacheGet("FecProc");
        }
        protected static string GetConcepto()
        {
            return NomadProxy.GetProxy().CacheGet("ConcProc");
        }

        protected static void SetLegajo(string leg)
        {
            NomadProxy.GetProxy().CacheAdd("LegProc", leg);
            NomadProxy.GetProxy().CacheAdd("FecProc", "");
            NomadProxy.GetProxy().CacheAdd("ConcProc", "");
        }
        protected static void SetFecha(string fec)
        {
            NomadProxy.GetProxy().CacheAdd("FecProc", fec);
        }
        protected static void SetConcepto(string conc)
        {
            NomadProxy.GetProxy().CacheAdd("ConcProc", conc);
        }

        protected static void SetCurrent(LiqConceptosBase Current)
        {
            NomadProxy.GetProxy().CacheAdd("CurrentProc", Current);
        }
        protected static LiqConceptosBase GetCurrent()
        {
            return (LiqConceptosBase) NomadProxy.GetProxy().CacheGetObj("CurrentProc");
        }
		
		
        protected static double Redondear(double value, int minutos)
        {
			return Redondear(value, minutos, 0);
		}
		
        protected static double Redondear(double value, int minutos, int mode)
        {
			value=value * 60.0 / minutos;
			
            if (mode==0)
			{ //redondear
				value=Math.Round(value);
			} else
            if (mode>0)
			{ //arriba
				value=Math.Ceiling(value);
			} else
			{ //abajo
				value=Math.Floor(value);
			}
			
			return value=value * minutos / 60.0;
        }
    }
}



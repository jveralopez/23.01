using System;
using System.IO;
using System.Xml;
using System.Collections;
using System.Text;

using System.Reflection;
using Microsoft.CSharp;
using System.CodeDom.Compiler;


using Nomad.NSystem.Document;
using Nomad.NSystem.Proxy;
using Nomad.NSystem.Base;
using NucleusRH.Base.Liquidacion;
using NucleusRH.Base.Personal;
using NucleusRH.Base.Organizacion;
using System.Collections.Generic;
using Nomad.NSystem.Functions;

// <refresh srv="nucleusnet" port="1501" service="compile"/>

namespace NucleusRH.Base.Liquidacion.RHLiq
{

	public class RHLiqException:System.ApplicationException
	{
		public RHLiqException()
		{
		}

		public RHLiqException(string msg)
			: base(msg)
		{
		}

		public RHLiqException(string msg, Exception e)
			: base(msg, e)
		{
		}
	}


	public class ReciboDebug
	{
		public Nomad.NSystem.Proxy.NomadXML reciboXML;
		public Nomad.NSystem.Proxy.NomadXML conceptosXML;
		public Nomad.NSystem.Proxy.NomadXML cargosXML;

		public ReciboDebugGV  EN;
		public ReciboDebugGV  EA;
		public ReciboDebugGV  EF;
		public ReciboDebugGVP EPA;
		public ReciboDebugGVP ENR;
		public ReciboDebugGV  VS;

		public List<ReciboDebugCargo> Cargos;

		public ReciboDebug(string Empresa, string Legajo, string Nombre)
		{
			this.reciboXML = new Nomad.NSystem.Proxy.NomadXML("RECIBO");
			this.reciboXML.SetAttr("EMPRESA", Empresa);
			this.reciboXML.SetAttr("LEGAJO", Legajo);
			this.reciboXML.SetAttr("NOMBRE", Nombre);

			this.cargosXML = this.reciboXML.AddTailElement("CARGOS");
			this.Cargos = new List<ReciboDebugCargo>();

			this.EN = new ReciboDebugGV(this.reciboXML.AddTailElement("EN"));
			this.EA = new ReciboDebugGV(this.reciboXML.AddTailElement("EA"));
			this.EF = new ReciboDebugGV(this.reciboXML.AddTailElement("EF"));
			this.EPA = new ReciboDebugGVP(this.reciboXML.AddTailElement("EPA"));
			this.ENR = new ReciboDebugGVP(this.reciboXML.AddTailElement("ENR"));

			this.conceptosXML = this.reciboXML.AddTailElement("CONCEPTOS");
			
			this.VS = new ReciboDebugGV(this.reciboXML.AddTailElement("VS"));
		}


		public ReciboDebugConcepto AddConcepto(string Codigo, string Descripcion, int Perido, int Cargo)
		{
			Nomad.NSystem.Proxy.NomadXML conceptoXML = this.conceptosXML.AddTailElement("CONCEPTO");
			conceptoXML.SetAttr("COD", Codigo);
			conceptoXML.SetAttr("DES", Descripcion);
			conceptoXML.SetAttr("manual", "0");
			conceptoXML.SetAttr("PERIODO", Perido);
			if (Cargo >= 0) conceptoXML.SetAttr("CARGO", "0");
			//			conceptoXML.SetAttr("error", Perido);
			//			conceptoXML.SetAttr("valor", Perido);
			//			conceptoXML.SetAttr("cantidad", Perido);

			return new ReciboDebugConcepto(conceptoXML);
		}

		public ReciboDebugCargo AddCargo(DateTime FIngreso, DateTime FEgreso, string Materia, string Categoria, string CentroCosto)
		{
			Nomad.NSystem.Proxy.NomadXML cargoXML = this.conceptosXML.AddTailElement("CONCEPTO");
			if (FIngreso > DateTime.MinValue) cargoXML.SetAttr("f_ingreso", FIngreso);
			if (FEgreso > DateTime.MinValue) cargoXML.SetAttr("f_egreso", FEgreso);
			cargoXML.SetAttr("Materia", Materia);
			cargoXML.SetAttr("Categoria", Categoria);
			cargoXML.SetAttr("CentroCosto", CentroCosto);

			ReciboDebugCargo retval = new ReciboDebugCargo(cargoXML);
			this.Cargos.Add(retval);
			return retval;
		}

		public static IList<int> SortKeys(System.Collections.Hashtable Valores)
		{
			SortedList<int, int> Sorted = new SortedList<int, int>();
			foreach (int Periodo in Valores.Keys) Sorted[Periodo] = Periodo;
			return Sorted.Keys;
		}

		public static int ToMeses(int pPeriodo)
		{
			int Ano = 0, Mes = 0;
			Ano = pPeriodo / 100;
			Mes = pPeriodo % 100;
			return (Ano * 12) + (Mes - 1);
		}

		public static int ToPeriodo(int pMeses)
		{
			int Ano = 0, Mes = 0;
			Ano = pMeses / 12;
			Mes = pMeses % 12;
			return (Ano * 100) + (Mes + 1);
		}

		public static string GetLabel(int Periodo)
		{
			switch (Periodo % 100)
			{
				case 1: return "ENE-" + (Periodo / 100).ToString();
				case 2: return "FEB-" + (Periodo / 100).ToString(); 
				case 3: return "MAR-" + (Periodo / 100).ToString(); 
				case 4: return "ABR-" + (Periodo / 100).ToString(); 
				case 5: return "MAY-" + (Periodo / 100).ToString(); 
				case 6: return "JUN-" + (Periodo / 100).ToString(); 
				case 7: return "JUL-" + (Periodo / 100).ToString(); 
				case 8: return "AGO-" + (Periodo / 100).ToString(); 
				case 9: return "SEP-" + (Periodo / 100).ToString(); 
				case 10: return "OCT-" + (Periodo / 100).ToString();
				case 11: return "NOV-" + (Periodo / 100).ToString();
				case 12: return "DIC-" + (Periodo / 100).ToString();
				default: return "ERROR";
			}

		}
	}

	public class ReciboDebugCargo
	{
		Nomad.NSystem.Proxy.NomadXML cargoXML;

		public ReciboDebugGV EN;
		public ReciboDebugGV EF;
		public ReciboDebugGVP EA;

		public ReciboDebugCargo(Nomad.NSystem.Proxy.NomadXML cargoXML)
		{
			this.cargoXML = cargoXML;

			this.EN = new ReciboDebugGV(this.cargoXML.AddTailElement("EN"));
			this.EF = new ReciboDebugGV(this.cargoXML.AddTailElement("EF"));
			this.EA = new ReciboDebugGVP(this.cargoXML.AddTailElement("EA"));
		}
	}

	public class ReciboDebugConcepto
	{
		public Nomad.NSystem.Proxy.NomadXML conceptoXML;
		public ReciboDebugParam Params;

		public ReciboDebugConcepto(Nomad.NSystem.Proxy.NomadXML conceptoXML)
		{
			this.conceptoXML = conceptoXML;
			this.Params = new ReciboDebugParam(conceptoXML);
		}
	}

	public class ReciboDebugGV
	{
		public Nomad.NSystem.Proxy.NomadXML gvXML;
		Dictionary<string, Nomad.NSystem.Proxy.NomadXML> varsXML;

		public ReciboDebugGV(Nomad.NSystem.Proxy.NomadXML gvXML)
		{
			this.gvXML = gvXML;
			this.varsXML = new Dictionary<string, Nomad.NSystem.Proxy.NomadXML>();
		}

		public Nomad.NSystem.Proxy.NomadXML GetVariable(string Name, string Tipo)
		{
			Nomad.NSystem.Proxy.NomadXML varXML;
			if (!varsXML.ContainsKey(Name))
			{
				varXML = gvXML.AddTailElement("VAR");
				varXML.SetAttr("nombre", Name);
				varXML.SetAttr("tipo", Tipo);
				varsXML[Name] = varXML;
			}
			else
				varXML = varsXML[Name];

			return varXML;
		}

		void SetValor(string AttrName, string Name, string Tipo, object Valor)
		{
			Nomad.NSystem.Proxy.NomadXML varXML = GetVariable(Name, Tipo);

			switch (Valor.GetType().FullName.ToUpper())
			{
				case "SYSTEM.DOUBLE": varXML.SetAttr(AttrName, ((double)Valor).ToString("0.000").Replace(',', '.')); break;
				case "SYSTEM.INT32": varXML.SetAttr(AttrName, (int)Valor); break;
				case "SYSTEM.BOOLEAN": varXML.SetAttr(AttrName, (bool)Valor); break;
				case "SYSTEM.STRING": varXML.SetAttr(AttrName, (string)Valor); break;
				case "SYSTEM.DATETIME": varXML.SetAttr(AttrName, (DateTime)Valor); break;
			}
		}

		public void SetValorEntrada(string Name, string Tipo, object Valor) { this.SetValor("valor_e", Name, Tipo, Valor); }
		public void SetValorSalida(string Name, string Tipo, object Valor) { this.SetValor("valor_s", Name, Tipo, Valor); }
	}

	public class ReciboDebugGVP
	{
		public Nomad.NSystem.Proxy.NomadXML gvXML;
		Dictionary<string, ReciboDebugGV> varPers;

		public ReciboDebugGVP(Nomad.NSystem.Proxy.NomadXML gvXML)
		{
			this.gvXML = gvXML;
			this.varPers = new Dictionary<string, ReciboDebugGV>();
		}

		public ReciboDebugGV GetVariable(string Name, string Tipo, int periodoLiq)
		{
			ReciboDebugGV retval;
			if (!varPers.ContainsKey(Name))
			{
				Nomad.NSystem.Proxy.NomadXML varXML = gvXML.AddTailElement("VAR");
				varXML.SetAttr("nombre", Name);
				varXML.SetAttr("tipo", Tipo);
				if (periodoLiq > 0)
				{
					varXML.SetAttr("periodo", periodoLiq);
					varXML.SetAttr("perlabel", ReciboDebug.GetLabel(periodoLiq));
				}

				retval = new ReciboDebugGV(varXML);
				this.varPers[Name] = retval;
			}
			else
				retval = varPers[Name];

			return retval;
		}

		void SetValor(Nomad.NSystem.Proxy.NomadXML varXML, string AttrName, object Valor)
		{
			switch (Valor.GetType().FullName.ToUpper())
			{
				case "SYSTEM.DOUBLE": varXML.SetAttr(AttrName, ((double)Valor).ToString("0.000").Replace(',', '.')); break;
				case "SYSTEM.INT32": varXML.SetAttr(AttrName, (int)Valor); break;
				case "SYSTEM.BOOLEAN": varXML.SetAttr(AttrName, (bool)Valor); break;
				case "SYSTEM.STRING": varXML.SetAttr(AttrName, (string)Valor); break;
				case "SYSTEM.DATETIME": varXML.SetAttr(AttrName, (DateTime)Valor); break;
			}
		}


		public void SetValorEntrada(string Name, int anno, int periodoLiq, double Valor1, double Valor2, double Valor3, double Valor4, double Valor5, double Valor6, double Valor7, double Valor8, double Valor9, double Valor10, double Valor11, double Valor12)
		{
			ReciboDebugGV varObject = GetVariable(Name, "", periodoLiq);


			int periodo = anno * 100;
			periodo++; varObject.SetValorEntrada(periodo.ToString(), "", Valor1);
			periodo++; varObject.SetValorEntrada(periodo.ToString(), "", Valor2);
			periodo++; varObject.SetValorEntrada(periodo.ToString(), "", Valor3);
			periodo++; varObject.SetValorEntrada(periodo.ToString(), "", Valor4);
			periodo++; varObject.SetValorEntrada(periodo.ToString(), "", Valor5);
			periodo++; varObject.SetValorEntrada(periodo.ToString(), "", Valor6);
			periodo++; varObject.SetValorEntrada(periodo.ToString(), "", Valor7);
			periodo++; varObject.SetValorEntrada(periodo.ToString(), "", Valor8);
			periodo++; varObject.SetValorEntrada(periodo.ToString(), "", Valor9);
			periodo++; varObject.SetValorEntrada(periodo.ToString(), "", Valor10);
			periodo++; varObject.SetValorEntrada(periodo.ToString(), "", Valor11);
			periodo++; varObject.SetValorEntrada(periodo.ToString(), "", Valor12);

			if (periodoLiq/100 == anno)
			{
				switch(periodoLiq % 100)
				{
					case 1: SetValor(varObject.gvXML, "valor_e", Valor1); break;
					case 2: SetValor(varObject.gvXML, "valor_e", Valor2); break;
					case 3: SetValor(varObject.gvXML, "valor_e", Valor3); break;
					case 4: SetValor(varObject.gvXML, "valor_e", Valor4); break;
					case 5: SetValor(varObject.gvXML, "valor_e", Valor5); break;
					case 6: SetValor(varObject.gvXML, "valor_e", Valor6); break;
					case 7: SetValor(varObject.gvXML, "valor_e", Valor7); break;
					case 8: SetValor(varObject.gvXML, "valor_e", Valor8); break;
					case 9: SetValor(varObject.gvXML, "valor_e", Valor9); break;
					case 10: SetValor(varObject.gvXML, "valor_e", Valor10); break;
					case 11: SetValor(varObject.gvXML, "valor_e", Valor11); break;
					case 12: SetValor(varObject.gvXML, "valor_e", Valor12); break;
				}
			}
		}

		public void SetValorSalida(string Name, System.Collections.Hashtable Valores)
		{
			ReciboDebugGV varObject = GetVariable(Name, "", 0);

			//Actualizo el valor de salida
			int CurPeriodo = varObject.gvXML.GetAttrInt("periodo");
			if (CurPeriodo > 0)
			{
				CurPeriodo = ReciboDebug.ToMeses(CurPeriodo);
				if (Valores.ContainsKey(CurPeriodo))
					SetValor(varObject.gvXML, "valor_s", Valores[CurPeriodo]);
				else
					SetValor(varObject.gvXML, "valor_s", (double)0);
			}

			foreach (int Periodo in ReciboDebug.SortKeys(Valores))
				varObject.SetValorSalida(ReciboDebug.ToPeriodo(Periodo).ToString(), "", Valores[Periodo]);
		}
	}

	public class ReciboDebugParam
	{
		public Nomad.NSystem.Proxy.NomadXML gvXML;
		Dictionary<string, ReciboDebugParam> varsXML;
		int Periodo = 0;

		public ReciboDebugParam(Nomad.NSystem.Proxy.NomadXML gvXML)
		{
			this.gvXML = gvXML;
			this.varsXML = new Dictionary<string, ReciboDebugParam>();
		}

		public ReciboDebugParam GetVariable(string Name, string Tipo)
		{
			ReciboDebugParam retval;
			if (!varsXML.ContainsKey(Name))
			{
				Nomad.NSystem.Proxy.NomadXML varXML = gvXML.AddTailElement("VAR");
				varXML.SetAttr("nombre", Name);
				varXML.SetAttr("tipo", Tipo);

				retval = new ReciboDebugParam(varXML);
				this.varsXML[Name] = retval;
			}
			else
				retval = varsXML[Name];

			return retval;
		}

		void SetValor(string AttrName, string Name, string Tipo, object Valor)
		{
			ReciboDebugParam varXML = GetVariable(Name, Tipo);

			string TypeName = Valor.GetType().FullName.ToUpper();

			System.Collections.Hashtable values = null;
			switch (TypeName)
			{
				case "NUCLEUSRH.BASE.LIQUIDACION.RHLIQ.ACUMPER":
				case "NUCLEUSRH.BASE.LIQUIDACION.RHLIQ.ACUMPERSET":
					values = ((RHLiq.AcumPer)Valor).Values();
					this.SetValor(AttrName, Name, Tipo, ((RHLiq.AcumPer)Valor).Valor);
					varXML.Periodo = ((RHLiq.AcumPer)Valor).Periodo();
					varXML.gvXML.SetAttr("periodo", varXML.Periodo);
					varXML.gvXML.SetAttr("perlabel", ReciboDebug.GetLabel(varXML.Periodo));
					break;

				case "SYSTEM.COLLECTIONS.HASHTABLE":
					values = (System.Collections.Hashtable)Valor;

					if (varXML.Periodo != 0)
					{
						int meses = ReciboDebug.ToMeses(varXML.Periodo);
						if (values.ContainsKey(meses))
							varXML.gvXML.SetAttr(AttrName, ((double)values[meses]).ToString("0.000").Replace(',', '.'));
						else
							varXML.gvXML.SetAttr(AttrName, "0.000");
					}
					break;
			}



			if (values != null)
			{
				foreach (int Periodo in ReciboDebug.SortKeys(values))
					varXML.SetValor(AttrName, ReciboDebug.ToPeriodo(Periodo).ToString(), "", values[Periodo]);
			}
			else
			{
				switch (TypeName)
				{
					case "SYSTEM.DOUBLE": varXML.gvXML.SetAttr(AttrName, ((double)Valor).ToString("0.000").Replace(',', '.')); break;
					case "SYSTEM.INT32": varXML.gvXML.SetAttr(AttrName, (int)Valor); break;
					case "SYSTEM.BOOLEAN": varXML.gvXML.SetAttr(AttrName, (bool)Valor); break;
					case "SYSTEM.STRING": varXML.gvXML.SetAttr(AttrName, (string)Valor); break;
					case "SYSTEM.DATETIME": varXML.gvXML.SetAttr(AttrName, (DateTime)Valor); break;
				}
			}
		}

		public void SetValorEntrada(string Name, string Tipo, object Valor) { this.SetValor("valor_e", Name, Tipo, Valor); }
		public void SetValorSalida(string Name, string Tipo, object Valor) { this.SetValor("valor_s", Name, Tipo, Valor); }
	}


	public class LiqUtilBase
	{
		#region Funciones estaticas para conceptos
		public static void Info(string msg)
		{
			Info(msg, true);
		}

		public static void Err(string msg)
		{
			Err(msg, true);
		}

		public static void Warning(string msg)
		{
			Warning(msg, true);
		}

		public static void Advertencia(string msg)
		{
			Advertencia(msg, true);
		}

		public static void Info(string msg, bool externo)
		{
			if (externo) NomadEnvironment.GetTraceBatch().Info(msg);
			NomadEnvironment.GetTrace().Info(msg);
		}

		public static void Err(string msg, bool externo)
		{
			if (externo) NomadEnvironment.GetTraceBatch().Error(msg);
			NomadEnvironment.GetTrace().Error(msg);
		}

		public static void Warning(string msg, bool externo)
		{
			if (externo) NomadEnvironment.GetTraceBatch().Warning(msg);
			NomadEnvironment.GetTrace().Warning(msg);
		}

		public static double ToHours(int timeloco)
		{
			return (timeloco/100)+((timeloco%100)/60.0);
		}

		public static void Advertencia(string msg, bool externo)
		{
			if (externo) NomadEnvironment.GetTraceBatch().Warning(msg);
			NomadEnvironment.GetTrace().Warning(msg);
		}

		public static double Max(double a, double b) { return a>b?a:b; }
		public static double Min(double a, double b) { return a<b?a:b; }

		public static bool IsDigit(char c) { return (c>='0')&&('9'>=c); }

		public static double Str2Dbl(string d) { return Nomad.NSystem.Functions.StringUtil.str2dbl(d); }
		public static System.DateTime Str2Date(string d) { return Nomad.NSystem.Functions.StringUtil.str2date(d); }

		public static string Dbl2Str(double d) { return Nomad.NSystem.Functions.StringUtil.dbl2str(d); }
		public static string Date2Str(System.DateTime d) { return Nomad.NSystem.Functions.StringUtil.date2str(d); }

		public static string RTrim(string s)
		{
			while (s.Substring(s.Length-1, 1)==" ") s=s.Substring(0, s.Length-1);
			return s;
		}
		public static string LTrim(string s)
		{
			while (s.Substring(0, 1)==" ") s=s.Substring(1, s.Length-1);
			return s;
		}
		public static string Trim(string s) { return s.Trim(); }

		public static int ParseInt(string s) { return Convert.ToInt32(s); }
		public static int parseInt(string s) { return Convert.ToInt32(s); }

		public static int ParseInt(double d) { return Convert.ToInt32(d); }
		public static int parseInt(double d) { return Convert.ToInt32(d); }

		public static double Round(double n, int d) { return System.Math.Round(n, d); }
		public static double round(double n, int d) { return System.Math.Round(n, d); }

		public static int Round(double n) { return Convert.ToInt32(System.Math.Round(n, 0)); }
		public static int round(double n) { return Convert.ToInt32(System.Math.Round(n, 0)); }

		public static double Truncate(double n, int d) { return Round(n-0.5/Math.Pow(10, d), d); }
		public static double truncate(double n, int d) { return Round(n-0.5/Math.Pow(10, d), d); }

		public static double Truncate(double n) { return Truncate(n, 0); }
		public static double truncate(double n) { return Truncate(n, 0); }

		public static int Day(System.DateTime fecha) { return fecha.Day; }
		public static int day(System.DateTime fecha) { return fecha.Day; }

		public static int Month(System.DateTime fecha) { return fecha.Month; }
		public static int month(System.DateTime fecha) { return fecha.Month; }


		public static int Year(System.DateTime fecha) { return fecha.Year; }
		public static int year(System.DateTime fecha) { return fecha.Year; }

		public static System.DateTime Date() { return System.DateTime.Now; }
		public static System.DateTime date() { return System.DateTime.Now; }

		public static System.DateTime Date(int dia, int mes, int anio) { return new System.DateTime(anio, mes, dia); }
		public static System.DateTime date(int dia, int mes, int anio) { return new System.DateTime(anio, mes, dia); }
		public static System.DateTime LastDateMonth(System.DateTime fecha) { return new System.DateTime(fecha.Year, fecha.Month, System.DateTime.DaysInMonth(fecha.Year, fecha.Month)); }
		public static System.DateTime FirstDateMonth(System.DateTime fecha) { return new System.DateTime(fecha.Year, fecha.Month, 1); }

		public static int DiffYears(System.DateTime desde, System.DateTime hasta)
		{
			int r=hasta.Year-desde.Year;
			if (hasta.Month>desde.Month) return r;
			if (hasta.Month<desde.Month) return r-1;
			if (hasta.Day>=desde.Day) return r;
			return r-1;
		}
		public static int DiffMonths(System.DateTime d1, System.DateTime d2)
		{
			int m=d2.Year*12+d2.Month-(d1.Year*12+d1.Month);
			if (d2.Day<d1.Day) return m-1;
			return m;
		}
		public static int DiffDays(System.DateTime d1, System.DateTime d2)
		{
			return Convert.ToInt32(Math.Floor(Math.Abs((d2-d1).TotalDays)));
		}

		public static System.DateTime AddDays(System.DateTime fecha, int n) { return fecha.AddDays(n); }
		public static System.DateTime AddMonths(System.DateTime fecha, int n) { return fecha.AddMonths(n); }
		public static System.DateTime AddYears(System.DateTime fecha, int n) { return fecha.AddYears(n); }

		public static int AddMonths(int per, int n)
		{
			int meses=per/100*12+per%100+n-1;
			return meses/12*100+meses%12+1;
		}
		public static int AddYears(int per, int n) { return per+100*n; }

		public static System.DateTime nulldate=System.DateTime.MinValue;

		public static System.DateTime NullDate() { return System.DateTime.MinValue; }
		public static bool IsNull(System.DateTime fecha) { return (fecha==System.DateTime.MinValue); }
		public static bool IsNull(string cadena) { return (cadena==null||cadena==""); }
		public static bool IsNull(int valor) { return (valor==0); }
		public static bool IsNull(double valor) { return (valor==0D); }

		public static double ToDouble(bool datain) { return datain?1d:0d; }
		public static double ToDouble(double datain) { return datain; }
		public static double ToDouble(int datain) { return (double)datain; }
		public static double ToDouble(string datain) { return Str2Dbl(datain); }
		public static double ToDouble(DateTime datain) { return Str2Dbl(datain.ToString("yyyyMMdd")); }

		public static double ToDouble(object value)
		{
			switch (value.GetType().FullName.ToUpper())
			{
				case "SYSTEM.DOUBLE": return ToDouble((double)value);
				case "SYSTEM.INT32": return ToDouble((int)value);
				case "SYSTEM.BOOLEAN": return ToDouble((bool)value); 
				case "SYSTEM.STRING": return ToDouble((string)value);
				case "SYSTEM.DATETIME": return ToDouble((DateTime)value);
				case "NUCLEUSRH.BASE.LIQUIDACION.RHLIQ.ACUMPERSET":
				case "NUCLEUSRH.BASE.LIQUIDACION.RHLIQ.ACUMPER": return ((NucleusRH.Base.Liquidacion.RHLiq.AcumPer)value).Valor;
			}
			return 0;
		}



		public static int ToMeses(int pPeriodo)
		{
			int Ano=0, Mes=0;
			Ano=pPeriodo/100;
			Mes=pPeriodo%100;
			return (Ano*12)+(Mes-1);
		}

		public static int ToPeriodo(int pMeses)
		{
			int Ano=0, Mes=0;
			Ano=pMeses/12;
			Mes=pMeses%12;
			return (Ano*100)+(Mes+1);
		}

		#endregion




		public static void SetLabelPeriodo(NomadXML conceptoXML, int Periodo)
		{
			conceptoXML.SetAttr("periodo", Periodo);

			switch (Periodo%100)
			{
				case 1: conceptoXML.SetAttr("perlabel", "ENE-"+(Periodo/100).ToString()); break;
				case 2: conceptoXML.SetAttr("perlabel", "FEB-"+(Periodo/100).ToString()); break;
				case 3: conceptoXML.SetAttr("perlabel", "MAR-"+(Periodo/100).ToString()); break;
				case 4: conceptoXML.SetAttr("perlabel", "ABR-"+(Periodo/100).ToString()); break;
				case 5: conceptoXML.SetAttr("perlabel", "MAY-"+(Periodo/100).ToString()); break;
				case 6: conceptoXML.SetAttr("perlabel", "JUN-"+(Periodo/100).ToString()); break;
				case 7: conceptoXML.SetAttr("perlabel", "JUL-"+(Periodo/100).ToString()); break;
				case 8: conceptoXML.SetAttr("perlabel", "AGO-"+(Periodo/100).ToString()); break;
				case 9: conceptoXML.SetAttr("perlabel", "SEP-"+(Periodo/100).ToString()); break;
				case 10: conceptoXML.SetAttr("perlabel", "OCT-"+(Periodo/100).ToString()); break;
				case 11: conceptoXML.SetAttr("perlabel", "NOV-"+(Periodo/100).ToString()); break;
				case 12: conceptoXML.SetAttr("perlabel", "DIC-"+(Periodo/100).ToString()); break;
			}
			
		}

		private static NomadXML GetVariableValor(NomadXML conceptoXML, string varname, string vartype)
		{
			NomadXML varXML;
			varXML=conceptoXML.FindElement2("VAR", "nombre", varname);
			if (varXML==null)
			{
				varXML=conceptoXML.AddTailElement("VAR");
				varXML.SetAttr("nombre", varname);
				varXML.SetAttr("tipo", vartype);
			}
			return varXML;
		}

		private static void SetValorCargo(NomadXML conceptoXML, string varname, int cargo)
		{
			NomadXML varXML=GetVariableValor(conceptoXML, varname, "");
			varXML.SetAttr("cargo", cargo);
		}

		private static void SetValor(NomadXML conceptoXML, string varname, string vartype, string attrname, object value)
		{
			Hashtable values=null;

			NomadXML varXML=GetVariableValor(conceptoXML, varname, vartype);
			switch (value.GetType().FullName.ToUpper())
			{
				case "NUCLEUSRH.BASE.LIQUIDACION.RHLIQ.ACUMPER":
				case "NUCLEUSRH.BASE.LIQUIDACION.RHLIQ.ACUMPERSET":
					values=((RHLiq.AcumPer)value).Values();
					varXML.SetAttr(attrname, ((RHLiq.AcumPer)value).Valor.ToString("0.000").Replace(',','.'));

					SetLabelPeriodo(varXML, ((RHLiq.AcumPer)value).Periodo());
					break;

				case "SYSTEM.COLLECTIONS.HASHTABLE":
					values=(Hashtable)value;

					if (varXML.GetAttrInt("periodo")!=0)
					{
						int meses=ToMeses(varXML.GetAttrInt("periodo"));

						if (values.ContainsKey(meses))
							varXML.SetAttr(attrname, ((double)values[meses]).ToString("0.000").Replace(',', '.'));
						 else
							varXML.SetAttr(attrname, "0.000");
					}
					break;
			}

			if (values!=null)
			{
				foreach (int Periodo in ReciboDebug.SortKeys(values))
					SetValor(varXML, ToPeriodo(Periodo).ToString(), "", attrname, values[Periodo]);
			} else
			{
				switch (value.GetType().FullName.ToUpper())
				{
					case "SYSTEM.DOUBLE": varXML.SetAttr(attrname, ((double)value).ToString("0.000").Replace(',','.')); break;
					case "SYSTEM.INT32": varXML.SetAttr(attrname, (int)value); break;
					case "SYSTEM.BOOLEAN": varXML.SetAttr(attrname, (bool)value); break;
					case "SYSTEM.STRING": varXML.SetAttr(attrname, (string)value); break;
					case "SYSTEM.DATETIME": varXML.SetAttr(attrname, (DateTime)value); break;
				}
			}
		}

		private static void SetValorEntrada(NomadXML conceptoXML, string varname, string vartype, object value)
		{
			SetValor(conceptoXML, varname, vartype, "valor_e", value);
		}
		private static void SetValorEntrada(NomadXML conceptoXML, string varname, int anno, int periodoLiq, object value1, object value2, object value3, object value4, object value5, object value6, object value7, object value8, object value9, object value10, object value11, object value12)
		{
			int periodo=anno*100;

			Hashtable Valores=new Hashtable();
			periodo++; Valores[ToMeses(periodo)]=value1;  
			periodo++; Valores[ToMeses(periodo)]=value2;  
			periodo++; Valores[ToMeses(periodo)]=value3;  
			periodo++; Valores[ToMeses(periodo)]=value4;  
			periodo++; Valores[ToMeses(periodo)]=value5;  
			periodo++; Valores[ToMeses(periodo)]=value6;  
			periodo++; Valores[ToMeses(periodo)]=value7;  
			periodo++; Valores[ToMeses(periodo)]=value8;  
			periodo++; Valores[ToMeses(periodo)]=value9;  
			periodo++; Valores[ToMeses(periodo)]=value10; 
			periodo++; Valores[ToMeses(periodo)]=value11; 
			periodo++; Valores[ToMeses(periodo)]=value12;

			SetLabelPeriodo(GetVariableValor(conceptoXML, varname, ""), periodoLiq);
			SetValor(conceptoXML, varname, "", "valor_e", Valores);
		}
		

		private static void SetValorSalida(NomadXML conceptoXML, string varname, string vartype, object value)
		{
			SetValor(conceptoXML, varname, vartype, "valor_s", value);
		}


		//functiones no ESTATICAS
		protected NomadProxy proxy;
		protected NomadTrace traceInt;
		protected Hashtable decodeError;  //mapa codificador c_error a oi_error
		protected Hashtable decodeItemIG; //mapa codificador c_item_ig a oi_item_ig
		protected int logLevel;

		public LiqUtilBase() 
		{
			this.proxy    = NomadProxy.GetProxy();
			this.traceInt = NomadEnvironment.GetTrace();

			//obtiene y setea el nivel de log
			bool bRes = int.TryParse(NomadEnvironment.QueryValue("ORG26_PARAMETROS", "d_valor", "c_parametro", "log_level", "", false), out this.logLevel);
			if(!bRes) this.logLevel = 0;
		}

		public NomadTrace GetTrace()
		{
			return NomadEnvironment.GetTraceBatch();
		}

		protected Liquidacion.LIQUIDACION myLiq;
		protected LiquidacionDDO.LIQUIDACION_DDO liqddo;
		protected EmpresasDDO.EMPRESA_DDO empresa;
		protected CabeceraDeCodificadoraDDO.CODIF_DDO codifddo;
		protected CabeceraDeConceptosDDO.CAB_CONC_DDO conceptos;
		protected LiqAdicBase liqAdic=new LiqAdicBase();
		protected Hashtable PeriodosRetroactivos=null;
		protected ReciboDebug reciboDEBUG=null;


		protected Hashtable Variables=null;

		protected Hashtable Globales=null;
		protected Hashtable[] Cargos=null;
		protected int Decimales=2;

		public void SendToLog(int level, string msg) 
		{
			if (this.logLevel > 0 && level <= this.logLevel) 
			{	
				this.GetTrace().Info("LogLevelInfo[" + level + "]: " + msg);			
			}
		}

		public void VerificarRango(double Valor, string VariableNombre, string ConceptoNombre) {
			//if (Valor>99999999.99||Valor<-99999999.99) {
			if (Valor>9999999999.99||Valor<-9999999999.99) {
				this.GetTrace().Error("El valor de la variable '"+VariableNombre+"' obtenido en el concepto '"+ConceptoNombre+"' es mayor al Maximo Permitido.");
			}
		}
		public void VerificarRango(object Valor, string VariableNombre, string ConceptoNombre)
		{
			if (Valor.GetType().FullName.ToUpper()=="SYSTEM.DOUBLE")
				VerificarRango((double)Valor, VariableNombre, ConceptoNombre);
		}

		// Carga mapa para decodificar Errores. Para usarse Ej: oi_error = this.decodeError['codigo_error']
		public void LoadDecodeError(LiquidacionDDO.LIQUIDACION_DDO liqddo) 
		{
			NomadXML myXml=new NomadXML();
			NomadXML myCur;

			while(true)
			{
				this.decodeError = new Hashtable();
				myXml.SetText(proxy.SQLService().Get("CLASS.NucleusRH.Base.Liquidacion.LiquidacionDDO.LIQUIDACION_DDO.QRY_ERRORES", ""));
				for (myCur=myXml.FirstChild().FirstChild(); myCur!=null; myCur=myCur.Next())
					this.decodeError[int.Parse(myCur.GetAttr("c_error"))] = myCur.GetAttr("oi_error");

				if (!this.decodeError.ContainsKey(-1))
				{

					Errores.ERROR errEXCEPTION=new Errores.ERROR();
					errEXCEPTION.c_error="-1";
					errEXCEPTION.d_error="Se produjo un ERROR al EJECUTAR EL CONCEPTO";
					NomadEnvironment.GetCurrentTransaction().Save(errEXCEPTION);
				} else if (!this.decodeError.ContainsKey(-2))
				{

					Errores.ERROR errEXCEPTION=new Errores.ERROR();
					errEXCEPTION.c_error="-2";
					errEXCEPTION.d_error="El codigo de ERROR no ENCONTRADO";
					NomadEnvironment.GetCurrentTransaction().Save(errEXCEPTION);
				} else
					return;
			}
		}

		// Carga mapa para decodificar ItemsIG. Para Usarse Ej: oi_item_ig = decodeItemIG['codigo_item']
		public void LoadDecodeItemsIG(LiquidacionDDO.LIQUIDACION_DDO liqddo) 
		{
			NomadXML myXml=new NomadXML();
			NomadXML myCur;
		
			this.decodeItemIG = new Hashtable();
			myXml.SetText(proxy.SQLService().Get("CLASS.NucleusRH.Base.Liquidacion.LiquidacionDDO.LIQUIDACION_DDO.QRY_ITEMS_IG", ""));
		
			for (myCur=myXml.FirstChild().FirstChild(); myCur!=null; myCur=myCur.Next())
				this.decodeItemIG[int.Parse(myCur.GetAttr("c_item_ig"))] = myCur.GetAttr("oi_item_ig");
		}

		//Inicializa el Sistema para Liquidar 'Liquidacion'
		public void InitializeLiquidacion(Liquidacion.LIQUIDACION myLiq, LiquidacionDDO.LIQUIDACION_DDO liqddo, CabeceraDeCodificadoraDDO.CODIF_DDO codifddo, CabeceraDeConceptosDDO.CAB_CONC_DDO conceptos, VariablesPorEjecucionDDO.VAR_EJEC_DDO variables, EmpresasDDO.EMPRESA_DDO empresa)
		{
			this.myLiq=myLiq;
			this.liqddo=liqddo;
			this.codifddo=codifddo;
			this.empresa=empresa;
			this.conceptos=conceptos;

			//cargamos decodificadora de errores
			LoadDecodeError(this.liqddo);

			//cargamos decodificadora de itemsIG
			LoadDecodeItemsIG(this.liqddo);

			//Cargo las Varibles
			this.Variables=new Hashtable();
			foreach (VariablesPorEjecucionDDO.VARIABLE_DDO variable in variables.VARIABLES)
				this.Variables[variable.Codigo]=variable;
		}

		//Inicializa el Sistema para Liquidar 'Legajo'
		public void InitializeLegajo(LegajoEmpresaDDO.PER_EMP_DDO empDDO, PersonalLiquidacionDDO.PER_LIQ_DDO empliqDDO, Recibos.TOT_LIQ_PER recLiq)
		{
			//inicializamos el adicionales de la liquidacion
			LoadLiqVars(empDDO);
			LoadLegVars(empDDO, empliqDDO);
			this.liqAdic.Initialize(this.myLiq, this.liqddo, this.codifddo, this.conceptos, empDDO, empliqDDO, empresa, recLiq);
		}

		public void LoadLiqVars(LegajoEmpresaDDO.PER_EMP_DDO empDDO) 
		{
			//Inicializo Variables
			this.Globales=new Hashtable();
			this.Cargos=new Hashtable[empDDO.Cargos.Count];
			for (int t=0; t<this.Cargos.Length; t++) this.Cargos[t]=new Hashtable();

			foreach (VariablesPorEjecucionDDO.VARIABLE_DDO variable in this.Variables.Values)
			{
				switch(variable.TipoDeVariable)
				{
					case "1": //Novedad empleado
					case "2": //Novedad empresa
					case "3": //Fijo empleado
					case "4": //Fijo empresa
					case "5": //Acumulador Comun
					case "6": //Variable Sistema
					case "7": //Auxiliar
					case "8": //Calculada Conceptos
						if (variable.ValorNull)
							_NullVar(variable.Codigo);
						else
							_SetVar(variable.Codigo, variable.Valor, this.Decimales);
						break;

					case "11": //Novedad por CARGO
					case "12": //Fijo por CARGO
						if (variable.ValorNull)
							_NullVar(variable.Codigo);
						else
							_SetVar(variable.Codigo, variable.Valor, this.Decimales);
						break;

					case "9":  //Acumulador de PERIODO
					case "10": //Novedad retroactiva con PERIODO
						_NullVar(variable.Codigo);
						break;

					case "13": //Acumulador de PERIODO por CARGO
						_NullVar(variable.Codigo);
						break;

					default:
						break;
				}
			}

			//Novedades de Liquidacion
			foreach (LiquidacionDDO.VAL_VARGN_DDO varLiquidacion in this.liqddo.VariablesNovedad)
			{
				if (varLiquidacion.ValorNull)
					_NullVar(varLiquidacion.CodigoVariable);
				else
					_SetVar(varLiquidacion.CodigoVariable, varLiquidacion.Valor, this.Decimales);
			}

			//Variables de Empresas
			foreach (EmpresasDDO.VAL_VARGF_DDO varEmpresa in this.empresa.VariablesFijas)
			{
				if (varEmpresa.ValorNull)
					_NullVar(varEmpresa.CodigoVariable);
				else
					_SetVar(varEmpresa.CodigoVariable, varEmpresa.Valor, this.Decimales);
			}
		}


		//Iniciar con valor VACIO
		private static void _NullVar(Hashtable hValues, VariablesPorEjecucionDDO.VARIABLE_DDO variable, int periodo)
		{
			switch (variable.TipoDeVariable)
			{
				case "1": //Novedad empleado
				case "2": //Novedad empresa
				case "3": //Fijo empleado
				case "4": //Fijo empresa
				case "5": //Acumulador Comun
				case "6": //Variable Sistema
				case "7": //Auxiliar
				case "8": //Calculada Conceptos
				case "11": //Novedad por CARGO
				case "12": //Fijo por CARGO
					switch(variable.TipoDeDato.ToUpper())
					{
						case "BOOL": hValues[variable.Codigo]=false; break;
						case "INT": hValues[variable.Codigo]=(int)0; break;
						case "DOUBLE": hValues[variable.Codigo]=(double)0; break;
						case "STRING": hValues[variable.Codigo]=""; break;
						case "DATETIME": hValues[variable.Codigo]=NullDate(); break;
					}
					break;

				case "9":  //Acumulador de PERIODO
				case "10": //Novedad retroactiva con PERIODO
				case "13": //Acumulador de PERIODO por CARGO
					
					if (!hValues.ContainsKey(variable.Codigo))
						hValues[variable.Codigo]=new Hashtable();

					if (periodo>0)
					{
						periodo = ToMeses(periodo);

						Hashtable loc=(Hashtable)hValues[variable.Codigo];
						switch (variable.TipoDeDato.ToUpper())
						{
							case "BOOL": loc[periodo]=false; break;
							case "INT": loc[periodo]=(int)0; break;
							case "DOUBLE": loc[periodo]=(double)0; break;
							case "STRING": loc[periodo]=""; break;
							case "DATETIME": loc[periodo]=NullDate(); break;
						}
					}
					break;
			}
			return;
		}
		private static void _SetVar(Hashtable hValues, VariablesPorEjecucionDDO.VARIABLE_DDO variable, int periodo, object value, int dec)
		{
			switch (variable.TipoDeVariable)
			{
				case "1": //Novedad empleado
				case "2": //Novedad empresa
				case "3": //Fijo empleado
				case "4": //Fijo empresa
				case "5": //Acumulador Comun
				case "6": //Variable Sistema
				case "7": //Auxiliar
				case "8": //Calculada Conceptos
				case "11": //Novedad por CARGO
				case "12": //Fijo por CARGO
					switch (variable.TipoDeDato.ToUpper())
					{
						case "BOOL": hValues[variable.Codigo]=(bool)value; break;
						case "INT": hValues[variable.Codigo]=(int)value; break;
						case "DOUBLE": hValues[variable.Codigo]=(dec>=0?(double)Math.Round((double)value, dec):(double)value); break;
						case "DATETIME": hValues[variable.Codigo]=(DateTime)value; break;
					}
					break;

				case "9":  //Acumulador de PERIODO
				case "10": //Novedad retroactiva con PERIODO
				case "13": //Acumulador de PERIODO por CARGO
					switch (value.GetType().FullName.ToUpper())
					{
						case "NUCLEUSRH.BASE.LIQUIDACION.RHLIQ.ACUMPER":
						case "NUCLEUSRH.BASE.LIQUIDACION.RHLIQ.ACUMPERSET":
							hValues[variable.Codigo]=((RHLiq.AcumPer)value).Values();
							break;

						default:
							Hashtable loc=(Hashtable)hValues[variable.Codigo];

							periodo = ToMeses(periodo);
							switch (variable.TipoDeDato.ToUpper())
							{
								case "BOOL": loc[periodo]=(bool)value; break;
								case "INT": loc[periodo]=(loc.ContainsKey(periodo)?(int)loc[periodo]:0)+(int)value; break;
								case "DOUBLE": loc[periodo]=(loc.ContainsKey(periodo)?(double)loc[periodo]:0.0)+(dec>=0?(double)Math.Round((double)value, dec):(double)value); break;
								case "DATETIME": loc[periodo]=(DateTime)value; break;
							}
							break;
					}
					break;
			}
			return;
		}
		private static void _SetVar(Hashtable hValues, VariablesPorEjecucionDDO.VARIABLE_DDO variable, int periodo, double value, int dec)
		{
			object MyValue=null;

			switch (variable.TipoDeDato.ToUpper())
			{
				case "BOOL": MyValue=(value==0?false:true); break;
				case "INT": MyValue=(int)Math.Round(value, 0); break;
				case "DOUBLE": MyValue=(dec>=0?(double)Math.Round((double)value, dec):(double)value); break;
				case "DATETIME": MyValue=new DateTime(((int)value)/10000, (((int)value)%10000)/100, ((int)value)%100); break;
			}

			_SetVar(hValues, variable, periodo, MyValue, dec);
			return;
		}

		private static void _AddVar(Hashtable hValues, VariablesPorEjecucionDDO.VARIABLE_DDO variable, int periodo, object value, int dec)
		{
			switch (variable.TipoDeVariable)
			{
				case "1": //Novedad empleado
				case "2": //Novedad empresa
				case "3": //Fijo empleado
				case "4": //Fijo empresa
				case "5": //Acumulador Comun
				case "6": //Variable Sistema
				case "7": //Auxiliar
				case "8": //Calculada Conceptos
				case "11": //Novedad por CARGO
				case "12": //Fijo por CARGO
					switch (variable.TipoDeDato.ToUpper())
					{
						case "BOOL": hValues[variable.Codigo]=(bool)value; break;
						case "INT": hValues[variable.Codigo]=(int)hValues[variable.Codigo]+(int)Math.Round((double)value,0); break;
						case "DOUBLE":
							//hValues[variable.Codigo]=(double)hValues[variable.Codigo]+(dec>=0?(double)Math.Round((double)value, dec):(double)value); break;					
							double num1 = (double) hValues[variable.Codigo];
							double num2;
							double num3;
							
							if(dec>=0)num2=(double)Math.Round((double)value, dec);
							else num2=(double)value;
							
							num3=num1+num2;
							
							if(Math.Abs(num3)>0 && Math.Abs(num3)<0.0001) num3=0;

							hValues[variable.Codigo]=num3;
						break;
						case "DATETIME": hValues[variable.Codigo]=(DateTime)value; break;
					}
					break;

				case "9":  //Acumulador de PERIODO
				case "10": //Novedad retroactiva con PERIODO
				case "13": //Acumulador de PERIODO por CARGO
					Hashtable loc=(Hashtable)hValues[variable.Codigo];

					switch (value.GetType().FullName.ToUpper())
					{
						case "NUCLEUSRH.BASE.LIQUIDACION.RHLIQ.ACUMPER":
						case "NUCLEUSRH.BASE.LIQUIDACION.RHLIQ.ACUMPERSET":
							hValues[variable.Codigo]=((RHLiq.AcumPer)value).Values();
							break;

						default:
							periodo = ToMeses(periodo);
							switch (variable.TipoDeDato.ToUpper())
							{
								case "BOOL": loc[periodo]=(bool)value; break;
								case "INT": loc[periodo]=(loc.ContainsKey(periodo)?(int)loc[periodo]:0)+(int)value; break;
								case "DOUBLE": loc[periodo]=(loc.ContainsKey(periodo)?(double)loc[periodo]:0.0)+(dec>=0?(double)Math.Round((double)value, dec):(double)value); break;
								case "DATETIME": loc[periodo]=(DateTime)value; break;
							}
							break;
					}


					break;
			}
			return;
		}
		private static void _AddVar(Hashtable hValues, VariablesPorEjecucionDDO.VARIABLE_DDO variable, int periodo, double value, int dec)
		{
			object MyValue=null;

			switch (variable.TipoDeDato.ToUpper())
			{
				case "BOOL": MyValue=(value==0?false:true); break;
				case "INT": MyValue=(int)Math.Round(value, 0); break;
				case "DOUBLE": MyValue=(dec>=0?(double)Math.Round((double)value, dec):(double)value); break;
				case "DATETIME": MyValue=new DateTime(((int)value)/10000, (((int)value)%10000)/100, ((int)value)%100); break;
			}

			_AddVar(hValues, variable, periodo, MyValue, dec);
			return;
		}
		private static void _SetVar(Hashtable hValues, VariablesPorEjecucionDDO.VARIABLE_DDO variable, int periodo, double value1, double value2, double value3, double value4, double value5, double value6, double value7, double value8, double value9, double value10, double value11, double value12, int dec)
		{
			double value=0.0;
			object MyValue=null;

			for (int t=1; t<=12; t++)
			{
				switch (t)
				{
					case 1: value=value1; break;
					case 2: value=value2; break;
					case 3: value=value3; break;
					case 4: value=value4; break;
					case 5: value=value5; break;
					case 6: value=value6; break;
					case 7: value=value7; break;
					case 8: value=value8; break;
					case 9: value=value9; break;
					case 10: value=value10; break;
					case 11: value=value11; break;
					case 12: value=value12; break;
				}

				switch (variable.TipoDeDato.ToUpper())
				{
					case "BOOL": MyValue=(value==0?false:true); break;
					case "INT": MyValue=(int)Math.Round(value, 0); break;
					case "DOUBLE": MyValue=(dec>=0?(double)Math.Round((double)value, dec):(double)value); break;
					case "DATETIME": MyValue=new DateTime(((int)value)/10000, (((int)value)%10000)/100, ((int)value)%100); break;
				}

				_SetVar(hValues, variable, periodo*100+t, MyValue, dec); 
			}
			
			return;
		}

		private static object _GetVar(Hashtable hValues, VariablesPorEjecucionDDO.VARIABLE_DDO variable, int Periodo)
		{
			switch (variable.TipoDeVariable)
			{
				case "1": //Novedad empleado
				case "2": //Novedad empresa
				case "3": //Fijo empleado
				case "4": //Fijo empresa
				case "5": //Acumulador Comun
				case "6": //Variable Sistema
				case "7": //Auxiliar
				case "8": //Calculada Conceptos
				case "11": //Novedad por CARGO
				case "12": //Fijo por CARGO
					return hValues[variable.Codigo];

				case "9":  //Acumulador de PERIODO
				case "10": //Novedad retroactiva con PERIODO
				case "13": //Acumulador de PERIODO por CARGO
					Hashtable loc=(Hashtable)hValues[variable.Codigo];

					if (Periodo<0)
						return loc;

					//Acumulador de Periodo
					return new AcumPerSet(loc, Periodo);
			}

			return null;
		}



		private void _SetVar(string varName, int Periodo, int Cargo, object value, int dec)
		{
			VariablesPorEjecucionDDO.VARIABLE_DDO variable=(VariablesPorEjecucionDDO.VARIABLE_DDO)this.Variables[varName];

			if (variable != null)
			{
				//Variables de CARGO
				if (variable.TipoDeVariable == "11" || variable.TipoDeVariable == "12" || variable.TipoDeVariable == "13")
				{
					if (Cargo >= 0)
						_SetVar(this.Cargos[Cargo], variable, Periodo, value, dec);
					else
						for (int idx = 0; idx < this.Cargos.Length; idx++)
							_SetVar(this.Cargos[idx], variable, Periodo, value, dec);
				}
				else
					_SetVar(this.Globales, variable, Periodo, value, dec);
			}
		}
		private void _AddVar(string varName, int Periodo, int Cargo, object value, int dec)
		{
			VariablesPorEjecucionDDO.VARIABLE_DDO variable=(VariablesPorEjecucionDDO.VARIABLE_DDO)this.Variables[varName];

			if (variable != null)
			{
				//Variables de CARGO
				if (variable.TipoDeVariable == "11" || variable.TipoDeVariable == "12" || variable.TipoDeVariable == "13")
				{
					if (Cargo >= 0)
						_AddVar(this.Cargos[Cargo], variable, Periodo, value, dec);
					else
						for (int idx = 0; idx < this.Cargos.Length; idx++)
							_AddVar(this.Cargos[idx], variable, Periodo, value, dec);
				}
				else
					_AddVar(this.Globales, variable, Periodo, value, dec);
			}
		}

		private void _SetVar(string varName, object value, int dec)
		{
			_SetVar(varName, -1, -1, value, dec);
		}
		private void _SetVar(string varName, int Periodo, int Cargo, double value, int dec)
		{
			VariablesPorEjecucionDDO.VARIABLE_DDO variable=(VariablesPorEjecucionDDO.VARIABLE_DDO)this.Variables[varName];

            if (variable != null)
            {
                //Variables de CARGO
                if (variable.TipoDeVariable == "11" || variable.TipoDeVariable == "12" || variable.TipoDeVariable == "13")
                {
                    if (Cargo >= 0)
                        _SetVar(this.Cargos[Cargo], variable, Periodo, value, dec);
                    else
                        for (int idx = 0; idx < this.Cargos.Length; idx++)
                            _SetVar(this.Cargos[idx], variable, Periodo, value, dec);
                }
                else
                    _SetVar(this.Globales, variable, Periodo, value, dec);
            }
			
		}
		private void _SetVar(string varName, double value, int dec)
		{
			_SetVar(varName, -1, -1, value, dec);
		}
		private void _SetVar(string varName, int Periodo, int Cargo, double value1, double value2, double value3, double value4, double value5, double value6, double value7, double value8, double value9, double value10, double value11, double value12, int dec)
		{
			VariablesPorEjecucionDDO.VARIABLE_DDO variable=(VariablesPorEjecucionDDO.VARIABLE_DDO)this.Variables[varName];

            if (variable != null)
            {
                //Variables de CARGO
                if (variable.TipoDeVariable == "11" || variable.TipoDeVariable == "12" || variable.TipoDeVariable == "13")
                {
                    if (Cargo >= 0)
                        _SetVar(this.Cargos[Cargo], variable, Periodo, value1, value2, value3, value4, value5, value6, value7, value8, value9, value10, value11, value12, dec);
                    else
                        for (int idx = 0; idx < this.Cargos.Length; idx++)
                            _SetVar(this.Cargos[idx], variable, Periodo, value1, value2, value3, value4, value5, value6, value7, value8, value9, value10, value11, value12, dec);
                }
                else
                    _SetVar(this.Globales, variable, Periodo, value1, value2, value3, value4, value5, value6, value7, value8, value9, value10, value11, value12, dec);
            }
			
		}
		private void _SetVar(string varName, int Periodo, double value1, double value2, double value3, double value4, double value5, double value6, double value7, double value8, double value9, double value10, double value11, double value12, int dec)
		{
			_SetVar(varName, Periodo, -1, value1, value2, value3, value4, value5, value6, value7, value8, value9, value10, value11, value12, dec);
		}


		private void _NullVar(string varName, int Periodo, int Cargo)
		{
			VariablesPorEjecucionDDO.VARIABLE_DDO variable=(VariablesPorEjecucionDDO.VARIABLE_DDO)this.Variables[varName];

			//Variables de CARGO
			if (variable.TipoDeVariable=="11" || variable.TipoDeVariable=="12" || variable.TipoDeVariable=="13")
			{
				if (Cargo>=0)
					_NullVar(this.Cargos[Cargo], variable, Periodo);
				else
					for (int idx=0; idx<this.Cargos.Length; idx++)
						_NullVar(this.Cargos[idx], variable, Periodo);
			} else
				_NullVar(this.Globales, variable, Periodo);
		}
		private void _NullVar(string varName)
		{
			_NullVar(varName, -1, -1);
		}


		private object _GetVar(string varName, int Periodo, int Cargo)
		{
			VariablesPorEjecucionDDO.VARIABLE_DDO variable=(VariablesPorEjecucionDDO.VARIABLE_DDO)this.Variables[varName];

            if (variable != null)
            {
                //Variables de CARGO
                if (variable.TipoDeVariable == "11" || variable.TipoDeVariable == "12" || variable.TipoDeVariable == "13")
                    return _GetVar(this.Cargos[Cargo], variable, Periodo);
                else
                    return _GetVar(this.Globales, variable, Periodo);
            }
            else
                return null;
			
		}
		private object _GetVar(string varName) { return _GetVar(varName, -1, -1); }




		public void LoadLegVars(LegajoEmpresaDDO.PER_EMP_DDO empDDO, PersonalLiquidacionDDO.PER_LIQ_DDO empliqDDO)
		{
			//Genero el recibo debug
			this.reciboDEBUG = new ReciboDebug(empDDO.Empresa, empDDO.NroLegajo.ToString(), empDDO.ApellidoyNombre);

			//cargamos variables de empleado novedad
			foreach (PersonalLiquidacionDDO.VAL_VAREN_DDO val_varen in empliqDDO.VariablesNovedad)
			{
				_SetVar(val_varen.CodigoVariable, val_varen.Valor, this.Decimales);
				this.reciboDEBUG.EN.SetValorEntrada(val_varen.CodigoVariable, "", val_varen.Valor);
			}

			//cargamos variables de empleado acumulador
			foreach (LegajoEmpresaDDO.VAL_VAREA_DDO val_varea in empDDO.VariablesAcum)
			{
				_SetVar(val_varea.CodigoVariable, val_varea.ValorEntrada, this.Decimales);
				this.reciboDEBUG.EA.SetValorEntrada(val_varea.CodigoVariable, "", val_varea.ValorEntrada);
			}

			//cargamos variables de empleado fijas
			foreach (LegajoEmpresaDDO.VAL_VAREF_DDO val_varef in empDDO.VariablesFijas)
			{
				_SetVar(val_varef.CodigoVariable, val_varef.Valor, this.Decimales);
				this.reciboDEBUG.EF.SetValorEntrada(val_varef.CodigoVariable, "", val_varef.Valor);
			}

			//cargamos variables de empleado acumulador por Periodo
			foreach (LegajoEmpresaDDO.VAL_A_ANNO_DDO val_varap in empDDO.VariablesAnno)
			{
				_SetVar(val_varap.CodigoVariable, val_varap.Periodo, -1, val_varap.ValorEntrada1, val_varap.ValorEntrada2, val_varap.ValorEntrada3, val_varap.ValorEntrada4, val_varap.ValorEntrada5, val_varap.ValorEntrada6, val_varap.ValorEntrada7, val_varap.ValorEntrada8, val_varap.ValorEntrada9, val_varap.ValorEntrada10, val_varap.ValorEntrada11, val_varap.ValorEntrada12, this.Decimales);
				this.reciboDEBUG.EPA.SetValorEntrada(val_varap.CodigoVariable, val_varap.Periodo, this.myLiq.e_periodo, val_varap.ValorEntrada1, val_varap.ValorEntrada2, val_varap.ValorEntrada3, val_varap.ValorEntrada4, val_varap.ValorEntrada5, val_varap.ValorEntrada6, val_varap.ValorEntrada7, val_varap.ValorEntrada8, val_varap.ValorEntrada9, val_varap.ValorEntrada10, val_varap.ValorEntrada11, val_varap.ValorEntrada12);
			}

			//cargamos novedades de empleado por Periodo
			PeriodosRetroactivos=new Hashtable();
			foreach (PersonalLiquidacionDDO.VAL_N_ANNO_DDO val_varnp in empliqDDO.VariablesAnno)
			{
				if (val_varnp.Valor1!=0) this.PeriodosRetroactivos[val_varnp.Periodo*100+1]=1;
				if (val_varnp.Valor2!=0) this.PeriodosRetroactivos[val_varnp.Periodo*100+2]=1;
				if (val_varnp.Valor3!=0) this.PeriodosRetroactivos[val_varnp.Periodo*100+3]=1;
				if (val_varnp.Valor4!=0) this.PeriodosRetroactivos[val_varnp.Periodo*100+4]=1;
				if (val_varnp.Valor5!=0) this.PeriodosRetroactivos[val_varnp.Periodo*100+5]=1;
				if (val_varnp.Valor6!=0) this.PeriodosRetroactivos[val_varnp.Periodo*100+6]=1;
				if (val_varnp.Valor7!=0) this.PeriodosRetroactivos[val_varnp.Periodo*100+7]=1;
				if (val_varnp.Valor8!=0) this.PeriodosRetroactivos[val_varnp.Periodo*100+8]=1;
				if (val_varnp.Valor9!=0) this.PeriodosRetroactivos[val_varnp.Periodo*100+9]=1;
				if (val_varnp.Valor10!=0) this.PeriodosRetroactivos[val_varnp.Periodo*100+10]=1;
				if (val_varnp.Valor11!=0) this.PeriodosRetroactivos[val_varnp.Periodo*100+11]=1;
				if (val_varnp.Valor12!=0) this.PeriodosRetroactivos[val_varnp.Periodo*100+12]=1;

				_SetVar(val_varnp.CodigoVariable, val_varnp.Periodo, -1, val_varnp.Valor1, val_varnp.Valor2, val_varnp.Valor3, val_varnp.Valor4, val_varnp.Valor5, val_varnp.Valor6, val_varnp.Valor7, val_varnp.Valor8, val_varnp.Valor9, val_varnp.Valor10, val_varnp.Valor11, val_varnp.Valor12,this.Decimales);
				this.reciboDEBUG.ENR.SetValorEntrada(val_varnp.CodigoVariable, val_varnp.Periodo, this.myLiq.e_periodo, val_varnp.Valor1, val_varnp.Valor2, val_varnp.Valor3, val_varnp.Valor4, val_varnp.Valor5, val_varnp.Valor6, val_varnp.Valor7, val_varnp.Valor8, val_varnp.Valor9, val_varnp.Valor10, val_varnp.Valor11, val_varnp.Valor12);
			}

			//Cargos
			for (int idx=0; idx<this.Cargos.Length; idx++)
			{
				//Obtengo el CARGO
				LegajoEmpresaDDO.CARGO_DDO MyCARGO=(LegajoEmpresaDDO.CARGO_DDO)empDDO.Cargos[idx];

				ReciboDebugCargo cargoDEBUG = this.reciboDEBUG.AddCargo(
					MyCARGO.FechaIngresoNull ? DateTime.MinValue: MyCARGO.FechaIngreso,
					MyCARGO.FechaEgresoNull ? DateTime.MinValue: MyCARGO.FechaEgreso,
					MyCARGO.Materia,
					"["+MyCARGO.Categoria+"]"+MyCARGO.CategoriaDesc,
					"["+MyCARGO.GrupoCostos+"]"+MyCARGO.GrupoCostosDesc
				);

				//cargamos variables de empleado novedad
				foreach (PersonalLiquidacionDDO.VAREN_CARGO val_varcn in empliqDDO.NovedadesCargo)
				if (MyCARGO.CargoID==val_varcn.CargoID)
				{
					_SetVar(val_varcn.Variable, -1, idx, val_varcn.Valor, this.Decimales);
					cargoDEBUG.EN.SetValorEntrada(val_varcn.Variable, "", val_varcn.Valor);
				}

				//cargamos variables de empleado fijas     
				foreach (LegajoEmpresaDDO.VAREF_CARGO val_varcf in MyCARGO.DatosFijos)
				{
					_SetVar(val_varcf.Variable, -1, idx, val_varcf.Valor, this.Decimales);
					cargoDEBUG.EF.SetValorEntrada( val_varcf.Variable, "", val_varcf.Valor);
				}

				//cargamos variables de empleado acumulador
				foreach (LegajoEmpresaDDO.VARPA_CARGO val_varca in MyCARGO.Acumuladores)
				{
					_SetVar(val_varca.CodigoVariable, val_varca.Periodo, idx, val_varca.ValorEntrada1, val_varca.ValorEntrada2, val_varca.ValorEntrada3, val_varca.ValorEntrada4, val_varca.ValorEntrada5, val_varca.ValorEntrada6, val_varca.ValorEntrada7, val_varca.ValorEntrada8, val_varca.ValorEntrada9, val_varca.ValorEntrada10, val_varca.ValorEntrada11, val_varca.ValorEntrada12, this.Decimales);
					cargoDEBUG.EA.SetValorEntrada(val_varca.CodigoVariable, val_varca.Periodo, this.myLiq.e_periodo, val_varca.ValorEntrada1, val_varca.ValorEntrada2, val_varca.ValorEntrada3, val_varca.ValorEntrada4, val_varca.ValorEntrada5, val_varca.ValorEntrada6, val_varca.ValorEntrada7, val_varca.ValorEntrada8, val_varca.ValorEntrada9, val_varca.ValorEntrada10, val_varca.ValorEntrada11, val_varca.ValorEntrada12);
				}
			}
		}

		public void ActualizarVariablesRecibo(LegajoEmpresaDDO.PER_EMP_DDO empDDO, PersonalLiquidacionDDO.PER_LIQ_DDO empliqDDOperLiq)
		{
			int t;
			Type MyType;
			System.Reflection.PropertyInfo[] MyProps;

			MyType=this.liqAdic.Recibo.GetType();
			MyProps=MyType.GetProperties();
			object[] prms=new object[1];

			for (t=0; t<MyProps.Length; t++)
			if ((MyProps[t].Name.Substring(0, 2)=="n_") && (!MyProps[t].Name.EndsWith("Null")))
			{
				try
				{
					prms[0]=_GetVar("vs_"+MyProps[t].Name.Substring(2));
					MyType.InvokeMember(MyProps[t].Name, BindingFlags.Public|BindingFlags.Instance|BindingFlags.SetProperty, null, this.liqAdic.Recibo, prms);
				} catch (Exception ex)
				{
					this.GetTrace().Error("El Recibo para el legajo "+empDDO.NroLegajo+" - "+empDDO.ApellidoyNombre+" no se pudo asignar la variable 'vs_"+MyProps[t].Name.Substring(2)+"'....", ex);
				}
			}
		}


		public double invokeTime = 0;
		public double executeTime = 0;
		public double initializeTime = 0;
		public double acumnTime = 0;
		public void ExecuteConcepto(CabeceraDeConceptosDDO.CONCEPTO_DDO MyConcepto)
		{

			SendToLog(5,"ExecuteConcepto(COD:" + MyConcepto.Codigo + "-DES" + MyConcepto.Descripcion + "-Secuencia:" + MyConcepto.Secuencia + ")"); 
			ArrayList Periodos=new ArrayList();
			if (MyConcepto.Retroactivo)
			{
				//Busco Variables Retroactivas Asociadas
				for (int i = 1; i <= 12; i++)
					Periodos.Add(ToPeriodo(ToMeses(this.liqddo.Periodo)-i));
			} else
			{
				Periodos.Add(this.liqddo.Periodo);
			}

			//Concepto Liquidado
			Recibos.CONC_LIQ_PER newCONC=null;
			Recibos.CONC_CARGO newCARGO=null;

			//Se ejecuta por cada Periodo en la Lista
			foreach(int Periodo in Periodos)
			{
				//General
				int Error, Cargo;
				double Valor, Cantidad;
				DateTime Now;

				//Ejecucion Manual
				bool EsManual;
				double ValorManual, CantidadManual;

				//Otros objetos
				PersonalLiquidacionDDO.CON_MAN_PER_DDO conc_man_per;
				Recibos.TOT_LIQ_PER Recibo;
				Recibos.ERROR_LIQ newERR;

				//Inicio Variables
				Error=0; Valor=0; Cantidad=0;
				EsManual=false; ValorManual=0; CantidadManual=0;
				Recibo=this.liqAdic.Recibo;
				Cargo=-1;
				if (this.liqAdic.Cargo!=null)
					foreach (LegajoEmpresaDDO.CARGO_DDO tc in this.liqAdic.Legajo.Cargos)
					{
						Cargo++;
						if (tc.CargoID==this.liqAdic.Cargo.CargoID) break;
					}

				//XML de AYUDA
				ReciboDebugConcepto concObj = this.reciboDEBUG.AddConcepto(MyConcepto.Codigo, MyConcepto.Descripcion, Periodo, Cargo);

				//Concepto MANUAL??
				conc_man_per = (PersonalLiquidacionDDO.CON_MAN_PER_DDO)this.liqAdic.LegajoLiq.ConceptosManuales.GetByAttribute("CodigoConcepto", MyConcepto.Codigo);
				if (conc_man_per != null)
				{
					//Concepto MANUAL
					EsManual = true;
					ValorManual = System.Math.Round(conc_man_per.Valor, this.Decimales);
					CantidadManual = System.Math.Round(conc_man_per.Cantidad, this.Decimales);

					concObj.conceptoXML.SetAttr("manual", "1");
					this.GetTrace().Info("El concepto "+MyConcepto.Codigo+" se liquido manualmente.");
				} else if ((this.liqddo.LiquidacionAjuste) && (!MyConcepto.EjecutaAjuste))
				{
					//Caso que la liquidacion sea de ajuste y el concepto no se tiene que ejecutar
					return;
				}


				//Parametros
				int IDX,ADDPARAM;
				object[] VarValues, EntradaValues, ExecuteParams;

				//Recorro las Variables del Concepto
				IDX=0;
				VarValues=new object[MyConcepto.VariablesConcepto.Count];
				EntradaValues=new object[MyConcepto.VariablesConcepto.Count];
				foreach (CabeceraDeConceptosDDO.CONC_VARIAB_DDO varConcepto in MyConcepto.VariablesConcepto)
				{
					switch (varConcepto.TipoParametro)
					{
						case "IN": VarValues[IDX]=_GetVar(varConcepto.CodigoVariable, Periodo, Cargo); break;
						case "LOC":
							switch (varConcepto.TipoDato.ToUpper())
							{
								case "BOOL": VarValues[IDX]=false; break;
								case "INT": VarValues[IDX]=0; break;
								case "DOUBLE": VarValues[IDX]=0.0d; break;
								case "DATETIME": VarValues[IDX]=NullDate(); break;
								case "STRING": VarValues[IDX]=""; break;
								default: throw NomadException.NewInternalException("INVALID LOC-TYPE "+varConcepto.TipoDato);
							}
							break;
						case "OUT": VarValues[IDX]=_GetVar(varConcepto.CodigoVariable, Periodo, Cargo); break;
						case "ACV": VarValues[IDX]=_GetVar(varConcepto.CodigoVariable, Periodo, Cargo); break;
						case "ACC": VarValues[IDX]=_GetVar(varConcepto.CodigoVariable, Periodo, Cargo); break;
					}
					EntradaValues[IDX]=VarValues[IDX]; IDX++;
				}


				//Obtencion de Valores LOCALES
				switch(MyConcepto.Version)
				{
					case "":
					case "1":
						//Version del CONCEPTO 1.0
						break;

					case "2":
						//Version del CONCEPTO 2.0

						//Obtengo los parametros
						ExecuteParams=new object[5+VarValues.Length];
						ExecuteParams[0]=this.liqAdic.Empresa;
						ExecuteParams[1]=this.liqddo;
						ExecuteParams[2]=this.liqAdic.Legajo;
						ExecuteParams[3]=this.liqAdic.Cargo;
						ExecuteParams[4]=this.liqAdic;
						for (IDX=0; IDX<VarValues.Length; IDX++) ExecuteParams[5+IDX]=VarValues[IDX];

						//Ejecuto el PRE CONCEPTO
						try
						{
							Type MyType=this.GetType();

							//Ejecuto el Concepto
							Now = DateTime.Now;
							MyType.InvokeMember("concepto_"+MyConcepto.Codigo+"_loc", BindingFlags.NonPublic|BindingFlags.Static|BindingFlags.InvokeMethod, null, null, ExecuteParams);
							this.invokeTime+= (DateTime.Now - Now).TotalMilliseconds;

							//Inversa
							for (IDX=0; IDX<VarValues.Length; IDX++) VarValues[IDX]=ExecuteParams[5+IDX];
						} catch (Exception e)
						{
							concObj.conceptoXML.SetAttr("error", -1);

							//Agrego el ERROR 
							newERR=new Recibos.ERROR_LIQ();
							newERR.oi_concepto_ddo=MyConcepto.Id;
							newERR.oi_error       =(string)this.decodeError[-1];
							Recibo.ERRORES_LIQ.Add(newERR);

							this.GetTrace().Error("Se produjo un error causado por la ejecucion del codigo del concepto "+MyConcepto.Codigo+": "+MyConcepto.Descripcion+". "+e.ToString());

							NomadException ex = NomadException.NewInternalException("ExecuteConcepto", e);
							ex.SetValue("Extension", "Se produjo un error causado por la ejecucion del codigo del concepto "+MyConcepto.Codigo+": "+MyConcepto.Descripcion+". "+e.ToString());
							ex.Dump();

							return;
						}
						break;

					default:
						throw NomadException.NewInternalException("ExecuteConcepto.VERSION ERROR");

				}

				//Variables de Entrada
				IDX=0;
				foreach (CabeceraDeConceptosDDO.CONC_VARIAB_DDO varConcepto in MyConcepto.VariablesConcepto)
				{
					concObj.Params.SetValorEntrada(varConcepto.Alias==""?varConcepto.CodigoVariable:varConcepto.Alias, varConcepto.TipoParametro, VarValues[IDX]);

					//Margo cuales variables SON de CARGO
					if (Cargo>=0 && varConcepto.TipoParametro!="LOC")
					{
						VariablesPorEjecucionDDO.VARIABLE_DDO variable=(VariablesPorEjecucionDDO.VARIABLE_DDO)this.Variables[varConcepto.Alias==""?varConcepto.CodigoVariable:varConcepto.Alias];
						if (variable.TipoDeVariable=="11" || variable.TipoDeVariable=="12" || variable.TipoDeVariable=="13")
							SetValorCargo(concObj.conceptoXML, varConcepto.Alias==""?varConcepto.CodigoVariable:varConcepto.Alias, Cargo);
					}

					IDX++;
				}
				

				//Obtencion de Valores LOCALES
				switch (MyConcepto.Version)
				{
					case "":
					case "1":
						ADDPARAM=11;

						//Version del CONCEPTO 1.0
						ExecuteParams=new object[ADDPARAM+VarValues.Length];
						ExecuteParams[0]=Valor;
						ExecuteParams[1]=Cantidad;
						ExecuteParams[2]=Error;
						ExecuteParams[3]=ValorManual;
						ExecuteParams[4]=CantidadManual;
						ExecuteParams[5]=EsManual;
						ExecuteParams[6]=this.liqAdic.Empresa;
						ExecuteParams[7]=this.liqddo;
						ExecuteParams[8]=this.liqAdic.Legajo;
						ExecuteParams[9]=this.liqAdic.Cargo;
						ExecuteParams[10]=this.liqAdic;
						for (IDX=0; IDX<VarValues.Length; IDX++) ExecuteParams[11+IDX]=VarValues[IDX];
						break;

					case "2":
						ADDPARAM=6;

						//Version del CONCEPTO 2.0
						ExecuteParams=new object[ADDPARAM+VarValues.Length];
						ExecuteParams[0]=Valor;
						ExecuteParams[1]=Cantidad;
						ExecuteParams[2]=Error;
						ExecuteParams[3]=ValorManual;
						ExecuteParams[4]=CantidadManual;
						ExecuteParams[5]=EsManual;
						for (IDX=0; IDX<VarValues.Length; IDX++) ExecuteParams[6+IDX]=VarValues[IDX];
						break;

					default:
						throw NomadException.NewInternalException("ExecuteConcepto.VERSION ERROR");

				}

				try
				{
					Type MyType=this.GetType();

					//Ejecuto el Concepto
					Now = DateTime.Now;
					MyType.InvokeMember("concepto_"+MyConcepto.Codigo, BindingFlags.NonPublic|BindingFlags.Static|BindingFlags.InvokeMethod, null, null, ExecuteParams);
					this.invokeTime+= (DateTime.Now - Now).TotalMilliseconds;
					for (IDX=0; IDX<VarValues.Length; IDX++) VarValues[IDX]=ExecuteParams[ADDPARAM+IDX];

					//Leo los Parametros de SALIDA
					Valor=(double)ExecuteParams[0];
					Cantidad=(double)ExecuteParams[1];
					Error=(int)ExecuteParams[2];

					//Modifico el Valor porque es Manual
					if (EsManual)
					{
						Valor = ValorManual;
						Cantidad = CantidadManual;
					}

					//Redondeo el Resultado
					Valor=System.Math.Round(Valor, this.Decimales);
					Cantidad=System.Math.Round(Cantidad, this.Decimales);

					//Verifico los Rangos
					VerificarRango(Valor, "Valor", MyConcepto.Codigo);
					VerificarRango(Cantidad, "Cantidad", MyConcepto.Codigo);

					//Actualizo las Variables
					IDX=0;
					foreach (CabeceraDeConceptosDDO.CONC_VARIAB_DDO varConcepto in MyConcepto.VariablesConcepto)
					{
						switch (varConcepto.TipoParametro)
						{
							case "LOC": break;
							case "IN": break;
							case "OUT":
								VerificarRango(VarValues[IDX], varConcepto.CodigoVariable, MyConcepto.Codigo);
								_SetVar(varConcepto.CodigoVariable, Periodo, Cargo, VarValues[IDX], -1);
								break;
							case "ACV":
								_AddVar(varConcepto.CodigoVariable, Periodo, Cargo, Valor, -1);
								break;
							case "ACC":
								_AddVar(varConcepto.CodigoVariable, Periodo, Cargo, Cantidad, -1);
								break;
							
						}
						IDX++;
					}
				} catch (Exception e)
				{
					concObj.conceptoXML.SetAttr("error", -1);

					//Agrego el ERROR 
					newERR=new Recibos.ERROR_LIQ();
					newERR.oi_concepto_ddo=MyConcepto.Id;
					newERR.oi_error       =(string)this.decodeError[-1];
					Recibo.ERRORES_LIQ.Add(newERR);

					this.GetTrace().Error("Se produjo un error causado por la ejecucion del codigo del concepto "+MyConcepto.Codigo+": "+MyConcepto.Descripcion+". "+e.ToString());

					NomadException ex = NomadException.NewInternalException("ExecuteConcepto", e);
					ex.SetValue("Extension", "Se produjo un error causado por la ejecucion del codigo del concepto "+MyConcepto.Codigo+": "+MyConcepto.Descripcion+". "+e.ToString());
					ex.Dump();
					return;
				}

				if (!EsManual && Error != 0)
				{
					//existe codigo de Error?
					if (this.decodeError[Error] == null)
					{
						//Agrego el ERROR 
						newERR=new Recibos.ERROR_LIQ();
						newERR.oi_concepto_ddo=MyConcepto.Id;
						newERR.oi_error       =(string)this.decodeError[-2];
						Recibo.ERRORES_LIQ.Add(newERR);

						this.GetTrace().Error("Valor de Error invalido: Error=" + Error + ". El concepto "+MyConcepto.Codigo+":"+MyConcepto.Descripcion+" retorno un valor de la variable Error desconocido.");
					} else
					{
						//Agrego el ERROR 
						newERR=new Recibos.ERROR_LIQ();
						newERR.oi_concepto_ddo=MyConcepto.Id;
						newERR.oi_error       =(string)this.decodeError[Error];
						Recibo.ERRORES_LIQ.Add(newERR);
					}

					//Error en el concepto
					this.GetTrace().Error("El Concepto "+MyConcepto.Codigo+" genero el Error "+Error);
				}

				concObj.conceptoXML.SetAttr("error", Error);
				concObj.conceptoXML.SetAttr("valor", Valor.ToString("0.000").Replace(',', '.'));
				concObj.conceptoXML.SetAttr("cantidad", Cantidad.ToString("0.000").Replace(',', '.'));

				//Actualizo las Variables	
				IDX=0;
				foreach (CabeceraDeConceptosDDO.CONC_VARIAB_DDO varConcepto in MyConcepto.VariablesConcepto)
				{
					switch (varConcepto.TipoParametro)
					{
						case "LOC": break;
						case "IN": break;
						case "OUT":
							concObj.Params.SetValorSalida(varConcepto.Alias==""?varConcepto.CodigoVariable:varConcepto.Alias, varConcepto.TipoParametro, _GetVar(varConcepto.CodigoVariable, Periodo, Cargo));
							break;
						case "ACV":
							concObj.Params.SetValorSalida(varConcepto.Alias==""?varConcepto.CodigoVariable:varConcepto.Alias, varConcepto.TipoParametro, _GetVar(varConcepto.CodigoVariable, Periodo, Cargo));
							break;
						case "ACC":
							concObj.Params.SetValorSalida(varConcepto.Alias==""?varConcepto.CodigoVariable:varConcepto.Alias, varConcepto.TipoParametro, _GetVar(varConcepto.CodigoVariable, Periodo, Cargo));
							break;

					}
					IDX++;
				}

				//Acumula los valores en el Recibo
				AcumularValoresRecibo(MyConcepto.TipoConcepto, MyConcepto.FiguraEnRecibo, MyConcepto.AcumulaGanancia, MyConcepto.CoeficienteGanancia, Valor, Cantidad, concObj.Params);
				
				//Agrego el Concepto al Recibo
				if (Valor!=0 || Cantidad!=0 || MyConcepto.TipoConcepto=="9")
				{
					if (newCONC!=null)
					{ //Caso del Concepto Retrocactivo
						newCONC.n_valor   +=Valor;
						newCONC.n_cantidad+=Cantidad;

						if (newCARGO!=null)
						{
							newCARGO.n_cantidad+=Cantidad;
							newCARGO.n_valor   +=Valor;
						}
					} else
					if (this.liqAdic.Cargo!=null)
					{
						newCONC=(Recibos.CONC_LIQ_PER)Recibo.CONC_LIQ_PER.GetByAttribute("oi_concepto_ddo", MyConcepto.Id);

						if (newCONC==null)
						{
							//Genero el Concepto
							newCONC=new Recibos.CONC_LIQ_PER();
							newCONC.oi_concepto_ddo=MyConcepto.Id;
							newCONC.n_valor        =Valor;
							newCONC.n_cantidad     =Cantidad;

							//Agrego el Concepto
							Recibo.CONC_LIQ_PER.Add(newCONC);

						} else
						{
							newCONC.n_valor        +=Valor;
							newCONC.n_cantidad     +=Cantidad;
						}

						newCARGO=new Recibos.CONC_CARGO();
						newCARGO.IdxCargo = Cargo;
						newCARGO.n_cantidad=Cantidad;
						newCARGO.n_valor=Valor;

						//Agrego la Informacion del CARGO
						newCONC.CONC_CARGO.Add(newCARGO);
					} else
					{
						//Genero el Concepto
						newCONC=new Recibos.CONC_LIQ_PER();
						newCONC.oi_concepto_ddo=MyConcepto.Id;
						newCONC.n_valor        =Valor;
						newCONC.n_cantidad     =Cantidad;

						//Agrego el Concepto
						Recibo.CONC_LIQ_PER.Add(newCONC);
					}
				}
			}
		}

		// Ejecuta todos los conceptos para el legajo.
		// IMPORTANTE: No ejecute dos veces este metodo en la misma instancia de LiqConceptos.
		public Recibos.TOT_LIQ_PER ApplyConceptos(LegajoEmpresaDDO.PER_EMP_DDO empDDO, PersonalLiquidacionDDO.PER_LIQ_DDO empLiqDDO, Recibos.TOT_LIQ_PER recLiq, Dictionary<string, PropertyInfo> diccionarioPropiedades, Dictionary<string, PropertyInfo> diccionarioJson)
		{
			DateTime Now;
			
			Now = DateTime.Now;
			SendToLog(2,"ApplyConceptos(LEGAJO: " + empDDO.NroLegajo + ")"); 
			InitializeLegajo(empDDO, empLiqDDO, recLiq);
			this.initializeTime += (DateTime.Now - Now).TotalMilliseconds;

			//Etapa PRE CARGO
			SendToLog(2,"ApplyConceptos(Etapa PRE CARGO)"); 			
			this.liqAdic.Cargo=null;
			foreach (CabeceraDeConceptosDDO.CONCEPTO_DDO MyConcepto in this.conceptos.CONCEPTOS)
			{
				if (MyConcepto.Etapa!=0) continue;
				Now = DateTime.Now;
				ExecuteConcepto(MyConcepto);
				this.executeTime += (DateTime.Now - Now).TotalMilliseconds;
			}

			//Etapa CARGO
			SendToLog(2,"ApplyConceptos(Etapa CARGO)"); 
			for (int idx=0; idx<this.Cargos.Length; idx++)
			{
				this.liqAdic.Cargo=(LegajoEmpresaDDO.CARGO_DDO)empDDO.Cargos[idx];
				foreach (CabeceraDeConceptosDDO.CONCEPTO_DDO MyConcepto in this.conceptos.CONCEPTOS)
				{
					if (MyConcepto.Etapa!=1) continue;
					Now = DateTime.Now;
					ExecuteConcepto(MyConcepto);
					this.executeTime += (DateTime.Now - Now).TotalMilliseconds;
				}
			}

			//Etapa POS CARGO
			this.liqAdic.Cargo=null;
			SendToLog(2,"ApplyConceptos(Etapa POST CARGO)"); 
			foreach (CabeceraDeConceptosDDO.CONCEPTO_DDO MyConcepto in this.conceptos.CONCEPTOS)
			{
				if (MyConcepto.Etapa!=2) continue; 
				Now = DateTime.Now;
				ExecuteConcepto(MyConcepto);                 
				this.executeTime += (DateTime.Now - Now).TotalMilliseconds;
			}

			//Actualizacion del RECIVO
			Now = DateTime.Now;

			SendToLog(2,"ApplyConceptos(ActualizarAcumuladores)");												
			ActualizarAcumuladores(empDDO, empLiqDDO,diccionarioJson);
			SendToLog(2,"ApplyConceptos(ActualizarVariablesRecibo)"); 
			ActualizarVariablesRecibo(empDDO, empLiqDDO);
			SendToLog(2,"ApplyConceptos(GuardarVariablesPorGrupoo)"); 
			GuardarVariablesPorGrupo(diccionarioPropiedades);
			this.acumnTime += (DateTime.Now - Now).TotalMilliseconds;

			//Otros campos
			liqAdic.Recibo.c_forma_pago=this.liqddo.FormaDePago!=""?this.liqddo.FormaDePago:empDDO.FormaPago;
			liqAdic.Recibo.ReciboXML=this.reciboDEBUG.reciboXML;
			SendToLog(3,"ReciboXML=" + this.reciboDEBUG.reciboXML);

			return liqAdic.Recibo;
		}


        private void GuardarVariablesPorGrupo(Dictionary<string,PropertyInfo>diccionarioPropiedades)
        {       
            Recibos.VAR_GR_CONC grupoVar = null;
            string grupoActual = null;
            
            foreach (CabeceraDeConceptosDDO.GRUPO_VAR_DDO grupo in this.conceptos.GRUPO_VAR)
            {
                object variable = _GetVar(grupo.Variable,this.myLiq.e_periodo,-1);
                if (variable != null)
                {
                    double valor = ToDouble(variable);

                    if (grupoActual != grupo.Grupo)
                    {
                        if (grupoActual != null)
                        {
                            this.liqAdic.Recibo.VAR_GR_CONC.Add(grupoVar);
                        }
                        grupoVar = new Recibos.VAR_GR_CONC();
                        grupoVar.c_grupo = grupo.Grupo;
                    }

                    diccionarioPropiedades[grupo.Columna].SetValue(grupoVar, valor, null);
                    grupoActual = grupoVar.c_grupo;           
                }
                   
            }     
            if(grupoVar != null)
                this.liqAdic.Recibo.VAR_GR_CONC.Add(grupoVar);
          
        }

		public void ActualizarAcumuladores(LegajoEmpresaDDO.PER_EMP_DDO empDDO, PersonalLiquidacionDDO.PER_LIQ_DDO empliqDDOperLiq,Dictionary<string,PropertyInfo> diccionarioJson)
		{
			//Recibos.VAREA_TOT acum;
            Dictionary<string, double> dicVareaTot = new Dictionary<string,double>();
            Dictionary<string, double> dicVareaAnno = new Dictionary<string, double>();
			//Recibos.VAR_A_ANNO acumP;
			Recibos.VARPA_CARGO acumC;
			string acumK;

			//Limpia los Acumuladores
			this.liqAdic.Recibo.VAREA_TOT.Clear();
			this.liqAdic.Recibo.VARPA_TOT.Clear();
			this.liqAdic.Recibo.VARPA_ANNO.Clear();
            this.liqAdic.Recibo.JSON_VAR.Clear();

			//Hash de Variables Acumulador
			Hashtable VarAcums=new Hashtable();

			//Acualizamos los Acumuladores
			foreach (VariablesPorEjecucionDDO.VARIABLE_DDO variable in this.Variables.Values)
			{
				if (variable.TipoDeVariable == "5")
				{
					dicVareaTot.Add(variable.Codigo, ToDouble(_GetVar(variable.Codigo)));
					this.reciboDEBUG.EA.SetValorSalida(variable.Codigo, "", _GetVar(variable.Codigo));
				}

				if (variable.TipoDeVariable == "6")
				{
					this.reciboDEBUG.VS.SetValorSalida(variable.Codigo, "", _GetVar(variable.Codigo));
				}

				if (variable.TipoDeVariable == "9")
				{
					Hashtable loc=(Hashtable)_GetVar(variable.Codigo);

					//Acumuladores por Periodo
					this.reciboDEBUG.EPA.SetValorSalida(variable.Codigo, loc);

					//Actualizo el Recibo
					foreach(int Periodo in ReciboDebug.SortKeys(loc))
					{
						dicVareaAnno.Add(variable.Codigo+"_"+(ToPeriodo(Periodo)),Math.Round(ToDouble(loc[Periodo]),2));
					}
				}

				if (variable.TipoDeVariable == "13")
				{
					for (int IDX=0; IDX<this.Cargos.Length; IDX++)
					{
						ReciboDebugCargo CARGO=this.reciboDEBUG.Cargos[IDX];
						
						LegajoEmpresaDDO.CARGO_DDO Cargo=(LegajoEmpresaDDO.CARGO_DDO)empDDO.Cargos[IDX];
						Hashtable loc=(Hashtable)_GetVar(variable.Codigo,  -1, IDX);

						//Acumuladores por Periodo
						CARGO.EA.SetValorSalida(variable.Codigo, loc);

						//Actualizo el Recivo
						foreach (int Periodo in ReciboDebug.SortKeys(loc))
						{
							acumC=null;
							acumK=variable.Codigo+"_"+IDX+"_"+(ToPeriodo(Periodo)/100);

							if (VarAcums.ContainsKey(acumK)) acumC=(Recibos.VARPA_CARGO)VarAcums[acumK];
							if (acumC==null)
							{
								acumC=new Recibos.VARPA_CARGO();
								acumC.n_valor_s_1 = 0;
								acumC.n_valor_s_2 = 0;
								acumC.n_valor_s_3 = 0;
								acumC.n_valor_s_4 = 0;
								acumC.n_valor_s_5 = 0;
								acumC.n_valor_s_6 = 0;
								acumC.n_valor_s_7 = 0;
								acumC.n_valor_s_8 = 0;
								acumC.n_valor_s_9 = 0;
								acumC.n_valor_s_10= 0;
								acumC.n_valor_s_11= 0;
								acumC.n_valor_s_12= 0;
								this.liqAdic.Recibo.VARPA_CARGO.Add(acumC);

								acumC.c_variable=variable.Codigo;
								acumC.e_anno=ToPeriodo(Periodo)/100;
								acumC.oi_cargo=Cargo.CargoID;

								VarAcums[acumK]=acumC;
							}

							switch (ToPeriodo(Periodo)%100)
							{
								case 1: acumC.n_valor_s_1 =ToDouble(loc[Periodo]); break;
								case 2: acumC.n_valor_s_2 =ToDouble(loc[Periodo]); break;
								case 3: acumC.n_valor_s_3 =ToDouble(loc[Periodo]); break;
								case 4: acumC.n_valor_s_4 =ToDouble(loc[Periodo]); break;
								case 5: acumC.n_valor_s_5 =ToDouble(loc[Periodo]); break;
								case 6: acumC.n_valor_s_6 =ToDouble(loc[Periodo]); break;
								case 7: acumC.n_valor_s_7 =ToDouble(loc[Periodo]); break;
								case 8: acumC.n_valor_s_8 =ToDouble(loc[Periodo]); break;
								case 9: acumC.n_valor_s_9 =ToDouble(loc[Periodo]); break;
								case 10: acumC.n_valor_s_10=ToDouble(loc[Periodo]); break;
								case 11: acumC.n_valor_s_11=ToDouble(loc[Periodo]); break;
								case 12: acumC.n_valor_s_12=ToDouble(loc[Periodo]); break;
							}
						}

					}
				}
			}

            ConvertirAJson("VAREA_TOT",dicVareaTot,diccionarioJson);
            ConvertirAJson("VARPA_ANNO", dicVareaAnno, diccionarioJson);
    
		}

        public void ConvertirAJson(string tipo, Dictionary<string, double> dicVar,Dictionary<string,PropertyInfo> diccionarioJson)
        {
            Recibos.JSON_VAR jsonvar = new Recibos.JSON_VAR();
            string json = StringUtil.Object2JSON(dicVar);
            jsonvar.c_tipo = tipo;
            string subJson = "";

            for (int i = 0, j = 1; i < json.Length; i += 4000, j++)
            {
                subJson = json.Substring(i, json.Length - i < 4000 ? json.Length - i : 4000);
                diccionarioJson["d_valor_" + j].SetValue(jsonvar, subJson, null);
            }
            this.liqAdic.Recibo.JSON_VAR.Add(jsonvar);
        }

       

        public static Dictionary<string, double> NomadXMLADiccionario(NomadXML xmlJson)
        {
            string json = "";

            for(int i =1; i<=25; i ++)
            {
                json += xmlJson.GetAttr("d_valor_"+i);
            }
            return StringUtil.JSON2Object<Dictionary<string, double>>(json);         
        }

        public static Dictionary<string, double> ConvertirADiccionario(Recibos.TOT_LIQ_PER RE, string tipo, Dictionary<string, PropertyInfo> diccionarioJson)
        {
            Recibos.JSON_VAR jsonVar = (Recibos.JSON_VAR)RE.JSON_VAR.GetByAttribute("c_tipo", tipo);
            string jsonCompleto = "";
            for (int i = 1; i < 26; i++)
            {
                string subJson = diccionarioJson["d_valor_" + i].GetValue(jsonVar, null).ToString();
                if (subJson != "")
                    jsonCompleto += diccionarioJson["d_valor_" + i].GetValue(jsonVar, null);
                else
                    break;
            }
            return StringUtil.JSON2Object<Dictionary<string,double>>(jsonCompleto);                  
        }


		public void AcumularValoresRecibo(string tipoConcepto, string figRecibo, string acumGanancia, double Coef, double valor, double cantidad, ReciboDebugParam concObj)
		{
            try
            {
			    //Leo las Variables
			    double vs_totigd=(double)_GetVar("vs_totigd");
			    double vs_totigh=(double)_GetVar("vs_totigh");
			    double vs_totighnohab=(double)_GetVar("vs_totighnohab");
			    double vs_totighextras=(double)_GetVar("vs_totighextras");
			    double vs_totigextrasex=(double)_GetVar("vs_totigextrasex");
			    double vs_totighexento=(double)_GetVar("vs_totighexento");
			    double vs_totighsac=(double)_GetVar("vs_totighsac");
                
                if (acumGanancia=="D")
			    {
				    concObj.SetValorEntrada("vs_totigd", "AUTO", vs_totigd);
				    vs_totigd+=valor;
				    concObj.SetValorSalida("vs_totigd", "AUTO", vs_totigd);
			    }

			    if (acumGanancia=="H")
			    {
				    concObj.SetValorEntrada("vs_totigh", "AUTO", vs_totigh);
				    vs_totigh+=valor;
				    concObj.SetValorSalida("vs_totigh", "AUTO", vs_totigh);
			    }

			    if (acumGanancia=="P")
			    {
				    concObj.SetValorEntrada("vs_totighnohab", "AUTO", vs_totighnohab);
				    vs_totighnohab+=valor;
				    concObj.SetValorSalida("vs_totighnohab", "AUTO", vs_totighnohab);
			    }

			    if (acumGanancia=="X")
			    {
				    concObj.SetValorEntrada("vs_totighexento", "AUTO", vs_totighexento);
				    vs_totighexento+=valor;
				    concObj.SetValorSalida("vs_totighexento", "AUTO", vs_totighexento);
			    }

			    if (acumGanancia=="S")
			    {
				    concObj.SetValorEntrada("vs_totighsac", "AUTO", vs_totighsac);
				    vs_totighsac+=valor;
				    concObj.SetValorSalida("vs_totighsac", "AUTO", vs_totighsac);
			    }

			    if (acumGanancia=="E")
			    {
				    double valorCoef = valor * Coef;
				
				    concObj.SetValorEntrada("vs_totighextras", "AUTO", vs_totighextras);
				    concObj.SetValorEntrada("vs_totigextrasex", "AUTO", vs_totigextrasex);
				    vs_totighextras+=valorCoef;
				    vs_totigextrasex+=valor - valorCoef;
				    concObj.SetValorSalida("vs_totighextras", "AUTO", vs_totighextras);
				    concObj.SetValorSalida("vs_totigextrasex", "AUTO", vs_totigextrasex);
			    }


			    //Guardo las Variables
			    _SetVar("vs_totigd", vs_totigd, -1);
			    _SetVar("vs_totigh", vs_totigh, -1);
			    _SetVar("vs_totighnohab", vs_totighnohab, -1);
			    _SetVar("vs_totighextras", vs_totighextras, -1);
			    _SetVar("vs_totigextrasex", vs_totigextrasex, -1);
			    _SetVar("vs_totighexento", vs_totighexento, -1);
			    _SetVar("vs_totighsac", vs_totighsac, -1);

            }
            catch (Exception e)
            {
                NomadException ex = NomadException.NewInternalException("AcumularValorRecibo", e);
                ex.SetValue("c_concepto", concObj.gvXML.GetAttr("COD"));
                ex.SetValue("value", valor);
                throw ex;
            }
		}
	}




	public class LiqAdicBase
	{
		#region Propiedades
		private NomadProxy proxy=null;
		private NomadTrace traceInt=null;

		private Liquidacion.LIQUIDACION MyLiq;
		public LiquidacionDDO.LIQUIDACION_DDO liquidacion;
		private LegajoEmpresaDDO.PER_EMP_DDO legajo;
		private LegajoEmpresaDDO.CARGO_DDO cargo;
		private PersonalLiquidacionDDO.PER_LIQ_DDO legajoLiq;
		private EmpresasDDO.EMPRESA_DDO empresa;
		private Recibos.TOT_LIQ_PER recibo;
		private CabeceraDeCodificadoraDDO.CODIF_DDO codificadoras;
		private CabeceraDeConceptosDDO.CAB_CONC_DDO conceptos;
        private int[][] matrizFeriados;
        //private bool[] diaTrabajo = { false, true, true, true, true, true, false, false };

        public Recibos.TOT_LIQ_PER Recibo
		{
			get { return this.recibo; }
		}

		public LegajoEmpresaDDO.PER_EMP_DDO Legajo
		{
            set { this.legajo = value; }
			get { return this.legajo; }
		}

		public LegajoEmpresaDDO.CARGO_DDO Cargo
		{
			get { return this.cargo; }
			set { this.cargo=value; }
		}

		public EmpresasDDO.EMPRESA_DDO Empresa
		{
			get { return this.empresa; }
		}

		public PersonalLiquidacionDDO.PER_LIQ_DDO LegajoLiq
		{
			get { return this.legajoLiq; }
		}


        public int[][] MatrizFeriados
        {
            set { this.matrizFeriados = value; }
            get { return this.matrizFeriados; }
        }
		#endregion

		#region Manejo de egreso
		// Agregado para modificar la Fecha de Egreso durante la ejecución de la liquidación
		private System.DateTime f_egreso;
		private string c_motivo_eg;
		public bool Egresado
		{
			get { return this.c_motivo_eg!=""; }
		}
		public System.DateTime FechaEgreso
		{
			get { return this.c_motivo_eg!=""?this.f_egreso:System.DateTime.MinValue; }
		}
		public string MotivoEgreso
		{
			get { return this.c_motivo_eg; }
		}
		public void Egresar(System.DateTime fecha, int motivo)
		{
			if (motivo>0)
			{
				this.f_egreso=fecha;
				this.c_motivo_eg=motivo.ToString();
			}
		}
		public void NoEgresar()
		{
			this.f_egreso=System.DateTime.MinValue;
			this.c_motivo_eg="";
		}
		#endregion

		public NomadTrace GetTrace()
		{
			return NomadEnvironment.GetTraceBatch();
		}

		//Metodos internos
		public void Initialize(Liquidacion.LIQUIDACION MyLiq, LiquidacionDDO.LIQUIDACION_DDO liquidacion, CabeceraDeCodificadoraDDO.CODIF_DDO codificadoras, CabeceraDeConceptosDDO.CAB_CONC_DDO conceptos, LegajoEmpresaDDO.PER_EMP_DDO legajo, PersonalLiquidacionDDO.PER_LIQ_DDO legajoLiq, EmpresasDDO.EMPRESA_DDO empresa, Recibos.TOT_LIQ_PER recExtern)
		{
			int e_secuencia=((Liquidacion.EJECUCION)MyLiq.EJECUCIONES.GetByAttribute("oi_liquidacion_ddo", liquidacion.Id)).e_secuencia;
			this.proxy=NomadProxy.GetProxy();
			this.traceInt=NomadEnvironment.GetTrace();

			this.MyLiq=MyLiq;
			this.liquidacion=liquidacion;
			this.legajo=legajo;
			this.legajoLiq=legajoLiq;
			this.codificadoras=codificadoras;
			this.empresa=empresa;
			this.conceptos=conceptos;
			this.cargo=null;

			if (recExtern==null)
			{
				this.recibo=new Recibos.TOT_LIQ_PER();
			} else
			{
				this.recibo=recExtern;
				this.recibo.ERRORES_LIQ.Clear();
				this.recibo.AJDIG.Clear();
				this.recibo.VALIG.Clear();
				this.recibo.VAREA_TOT.Clear();
			}

			this.f_egreso=this.legajo.FechaEgreso;
			this.c_motivo_eg=this.legajo.MotivoEgreso;
		}


		#region Funciones auxiliares para conceptos
       
		//Metodos
		public System.DateTime FechaAntiguedad
		{
			get { return this.legajo.FechaCalculoAntiguedadNull?this.legajo.FechaIngreso:this.legajo.FechaCalculoAntiguedad; }
		}

		public int AntiguedadAnios
		{
			get
			{
				System.DateTime ultDiaLiq=LiqUtilBase.LastDateMonth(this.liquidacion.Fecha);
				int anios=LiqUtilBase.DiffYears(this.FechaAntiguedad, ultDiaLiq);
				return anios;
			}
		}

		public int AntiguedadMeses
		{
			get
			{
				System.DateTime ultFecha=!this.Egresado?this.liquidacion.Fecha:this.FechaEgreso;
				int meses=LiqUtilBase.DiffMonths(this.FechaAntiguedad, ultFecha);
				return meses;
			}
		}

		public int AntiguedadDias
		{
			get
			{
				System.DateTime ultDiaLiq=LiqUtilBase.LastDateMonth(this.liquidacion.Fecha);
				int dias=LiqUtilBase.DiffDays(this.FechaAntiguedad, ultDiaLiq);
				return dias;
			}
		}

		public int AntiguedadVacAnios
		{
			get
			{
				System.DateTime ultFecha=!this.Egresado?LiqUtilBase.Date(31, 12, this.liquidacion.Fecha.Year):this.FechaEgreso;
				int anios=LiqUtilBase.DiffYears(this.FechaAntiguedad, ultFecha);
				return anios;
			}
		}

		public int AntiguedadVacDias
		{
			get
			{
				System.DateTime ultFecha=!this.Egresado?LiqUtilBase.Date(31, 12, this.liquidacion.Fecha.Year):this.FechaEgreso;
				int dias=LiqUtilBase.DiffDays(this.FechaAntiguedad, ultFecha);
				return dias;
			}
		}

		public int DiasNoTrabajados(int periodo)
		{
			int retval;
			DateTime minDate=LiqUtilBase.FirstDateMonth(new DateTime(periodo/100, periodo%100, 1));
			DateTime maxDate=LiqUtilBase.LastDateMonth(new DateTime(periodo/100, periodo%100, 1));
			int diasmes = 1 + LiqUtilBase.DiffDays(minDate, maxDate);
			if (this.legajo.FechaIngreso > maxDate) return diasmes;
			if (this.Egresado && this.FechaEgreso < minDate) return diasmes;
			retval = 0;

			if (this.legajo.FechaIngreso>minDate)
				retval+=LiqUtilBase.DiffDays(minDate, this.legajo.FechaIngreso);
			if (this.Egresado&&this.FechaEgreso<maxDate)
				retval+=LiqUtilBase.DiffDays(this.FechaEgreso, maxDate);

			return retval;
		}
		
		public int DiasNoTrabajadosCargo(int periodo)
		{
			VerificarConceptoCargo(); 
			int retval;
			DateTime minDate=LiqUtilBase.FirstDateMonth(new DateTime(periodo/100, periodo%100, 1));
			DateTime maxDate=LiqUtilBase.LastDateMonth(new DateTime(periodo/100, periodo%100, 1));
			int diasmes = 1 + LiqUtilBase.DiffDays(minDate, maxDate);
			if (this.cargo.FechaIngreso > maxDate) return diasmes;
			if (!this.cargo.FechaEgresoNull && this.cargo.FechaEgreso < minDate) return diasmes;
			retval=0;

			if (this.cargo.FechaIngreso > minDate)
				retval += LiqUtilBase.DiffDays(minDate, this.cargo.FechaIngreso);
			if (!this.cargo.FechaEgresoNull && this.cargo.FechaEgreso < maxDate)
				retval += LiqUtilBase.DiffDays(this.cargo.FechaEgreso, maxDate);

			return retval;
		}

		public int DiasTrabajados(int periodo)
		{
			int retval;
			DateTime minDate=LiqUtilBase.FirstDateMonth(new DateTime(periodo/100, periodo%100, 1));
			DateTime maxDate=LiqUtilBase.LastDateMonth(new DateTime(periodo/100, periodo%100, 1));

			retval=1+LiqUtilBase.DiffDays(minDate, maxDate);

			if (this.legajo.FechaIngreso>minDate)
				retval-=LiqUtilBase.DiffDays(minDate, this.legajo.FechaIngreso);
			if (this.Egresado&&this.FechaEgreso<maxDate)
				retval-=LiqUtilBase.DiffDays(this.FechaEgreso, maxDate);

			if (retval<0) retval=0;
			return retval;
		}
		
		public int DiasTrabajadosCargo(int periodo)
		{
			VerificarConceptoCargo(); 
			int retval;
			DateTime minDate=LiqUtilBase.FirstDateMonth(new DateTime(periodo/100, periodo%100, 1));
			DateTime maxDate=LiqUtilBase.LastDateMonth(new DateTime(periodo/100, periodo%100, 1));

			retval=1+LiqUtilBase.DiffDays(minDate, maxDate);

			if (this.cargo.FechaIngreso>minDate)
				retval -= LiqUtilBase.DiffDays(minDate, this.cargo.FechaIngreso);
			if (!this.cargo.FechaEgresoNull && this.cargo.FechaEgreso < maxDate)
				retval -= LiqUtilBase.DiffDays(this.cargo.FechaEgreso, maxDate);

			if (retval<0) retval=0;
			return retval;
		}

		public int DiasTrabajadosLegales(int periodo)
		{
			int diasT=DiasTrabajados(periodo);
			int diasA=DiasNoTrabajados(periodo);

			return (int)Math.Round(30.0*diasT/(diasT+diasA));
		}

		public int DiasTrabajadosLegalesCargo(int periodo)
		{
			VerificarConceptoCargo();
			int diasT = DiasTrabajadosCargo(periodo);
			int diasA = DiasNoTrabajadosCargo(periodo);

			return (int)Math.Round(30.0 * diasT / (diasT + diasA));
		}

		public int DiasTrabajados(int periodoInicial, int periodoFinal)
		{
			int retval=0;
			DateTime Inicial=new DateTime(periodoInicial/100, periodoInicial%100, 1);
			DateTime Final=new DateTime(periodoFinal/100, periodoFinal%100, 1);

			for (DateTime t=Inicial; t<=Final; t=t.AddMonths(1))
				retval+=DiasTrabajados(t.Year*100+t.Month);

			return retval;
		}
		
		public int DiasTrabajadosCargo(int periodoInicial, int periodoFinal)
		{
			VerificarConceptoCargo();
			int retval = 0;
			DateTime Inicial=new DateTime(periodoInicial/100, periodoInicial%100, 1);
			DateTime Final=new DateTime(periodoFinal/100, periodoFinal%100, 1);

			for (DateTime t=Inicial; t<=Final; t=t.AddMonths(1))
				retval+=DiasTrabajadosCargo(t.Year*100+t.Month);

			return retval;
		}

		public int DiasTrabajadosLegal(int periodoInicial, int periodoFinal)
		{
			int retval=0;
			DateTime Inicial=new DateTime(periodoInicial/100, periodoInicial%100, 1);
			DateTime Final=new DateTime(periodoFinal/100, periodoFinal%100, 1);

			for (DateTime t=Inicial; t<=Final; t=t.AddMonths(1))
				retval+=DiasTrabajadosLegales(t.Year*100+t.Month);

			return retval;
		}

		public int DiasTrabajadosLegalCargo(int periodoInicial, int periodoFinal)
		{
			VerificarConceptoCargo();
			int retval = 0;
			DateTime Inicial=new DateTime(periodoInicial/100, periodoInicial%100, 1);
			DateTime Final=new DateTime(periodoFinal/100, periodoFinal%100, 1);

			for (DateTime t=Inicial; t<=Final; t=t.AddMonths(1))
				retval+=DiasTrabajadosLegalesCargo(t.Year*100+t.Month);

			return retval;
		}

		public int DiasNoTrabajados(int periodoInicial, int periodoFinal)
		{
			int retval=0;
			DateTime Inicial=new DateTime(periodoInicial/100, periodoInicial%100, 1);
			DateTime Final=new DateTime(periodoFinal/100, periodoFinal%100, 1);

			for (DateTime t=Inicial; t<=Final; t=t.AddMonths(1))
				retval+=DiasNoTrabajados(t.Year*100+t.Month);

			return retval;
		}

		public int DiasNoTrabajadosCargo(int periodoInicial, int periodoFinal)
		{
			VerificarConceptoCargo();
			int retval = 0;
			DateTime Inicial=new DateTime(periodoInicial/100, periodoInicial%100, 1);
			DateTime Final=new DateTime(periodoFinal/100, periodoFinal%100, 1);

			for (DateTime t=Inicial; t<=Final; t=t.AddMonths(1))
				retval+=DiasNoTrabajadosCargo(t.Year*100+t.Month);

			return retval;
		}

		public int DiasTrabajadosLegales(int periodoInicial, int periodoFinal)
		{
			int retval=0;
			DateTime Inicial=new DateTime(periodoInicial/100, periodoInicial%100, 1);
			DateTime Final=new DateTime(periodoFinal/100, periodoFinal%100, 1);

			for (DateTime t=Inicial; t<=Final; t=t.AddMonths(1))
				retval+=DiasTrabajadosLegales(t.Year*100+t.Month);

			return retval;
		}

		public int DiasTrabajadosLegalesCargo(int periodoInicial, int periodoFinal)
		{
			VerificarConceptoCargo();
			int retval = 0;
			DateTime Inicial=new DateTime(periodoInicial/100, periodoInicial%100, 1);
			DateTime Final=new DateTime(periodoFinal/100, periodoFinal%100, 1);

			for (DateTime t=Inicial; t<=Final; t=t.AddMonths(1))
				retval+=DiasTrabajadosLegalesCargo(t.Year*100+t.Month);

			return retval;
		}

		public double SueldoBasicoPersona()
		{
			//Si esta fuera de convenio
			return this.legajo.UltimaRemuneracion;
		}

		public double SueldoBasicoCategoria()
		{
			CabeceraDeCodificadoraDDO.CATEG_DDO categ=GetCategoria();

			if (categ==null) return -1;
			return categ.ValorSueldoCategoria;
		}

		public double SueldoBasicoCargo()
		{
			VerificarConceptoCargo();
			CabeceraDeCodificadoraDDO.CATEG_DDO categ = GetCategoriaCargo();

			if (categ == null) return -1;
			return categ.ValorSueldoCategoria;
		}

		private void VerificarConceptoCargo()
		{
			if (this.cargo == null)
			{
				this.GetTrace().Error("No se encontró el cargo, verifique que el concepto esté configurado para etapa de ejecución Cargos.");
				throw new Exception();
			}
		}


		public CabeceraDeCodificadoraDDO.CONVENIO_DDO GetConvenioByCode(string Convenio)
		{
			CabeceraDeCodificadoraDDO.CONVENIO_DDO objConvenio;

			objConvenio=(CabeceraDeCodificadoraDDO.CONVENIO_DDO)this.codificadoras.CONVENIOS.GetByAttribute("Convenio", Convenio);
			if (objConvenio==null)
			{
				this.GetTrace().Warning("No se encontro el Convenio: "+Convenio);
				return null;
			}

			return objConvenio;
		}

		public CabeceraDeCodificadoraDDO.CATEG_DDO GetCategByCode(string Convenio, string Categoria)
		{
			CabeceraDeCodificadoraDDO.CONVENIO_DDO objConvenio;
			CabeceraDeCodificadoraDDO.CATEG_DDO objCategoria;

			objConvenio=GetConvenioByCode(Convenio);
			if (objConvenio==null) return null;

			objCategoria=(CabeceraDeCodificadoraDDO.CATEG_DDO)objConvenio.CATEG.GetByAttribute("Categoria", Categoria);
			if (objCategoria==null)
			{
				this.GetTrace().Warning("No se encontro el Convenio-Categoria: "+Convenio+"-"+Categoria);
				return null;
			}

			return objCategoria;
		}

		public CabeceraDeCodificadoraDDO.ANTIG_DDO GetCategAntigByCode(string Convenio, string Categoria, int Antiguedad)
		{
			CabeceraDeCodificadoraDDO.ANTIG_DDO objAntig;
			CabeceraDeCodificadoraDDO.CATEG_DDO objCategoria=GetCategByCode(Convenio, Categoria);
			if (objCategoria==null) return null;

			objAntig=(CabeceraDeCodificadoraDDO.ANTIG_DDO)objCategoria.ANTIG.GetByAttribute("AniosAntiguedad", Antiguedad);
			if (objAntig==null)
			{
				this.GetTrace().Warning("No se encontro el Convenio-Categoria-Antiguedad: "+Convenio+"-"+Categoria+"-"+Antiguedad.ToString());
				return null;
			}

			return objAntig;
		}

		public CabeceraDeCodificadoraDDO.ADICIONAL_DDO GetCategAdicByCode(string Convenio, string Categoria, string CodAdicional)
		{
			CabeceraDeCodificadoraDDO.ADICIONAL_DDO objAdic;
			CabeceraDeCodificadoraDDO.CATEG_DDO objCategoria=GetCategByCode(Convenio, Categoria);
			if (objCategoria==null) return null;

			objAdic=(CabeceraDeCodificadoraDDO.ADICIONAL_DDO)objCategoria.ADICIONALES.GetByAttribute("CodAdicional", CodAdicional);
			if (objAdic==null)
			{
				return null;
			}

			return objAdic;
		}

		public double Monto(string Convenio, string Categoria)
		{
			CabeceraDeCodificadoraDDO.CATEG_DDO objCategoria=GetCategByCode(Convenio, Categoria);
			if (objCategoria==null) return 0;

			return objCategoria.ValorSueldoCategoria;
		}

		public double Monto(string Convenio, string Categoria, string CodAdicional)
		{
			CabeceraDeCodificadoraDDO.ADICIONAL_DDO objAdic=GetCategAdicByCode(Convenio, Categoria, CodAdicional);
			if (objAdic==null) return 0;

			return objAdic.Valor;
		}
		// Funcion que reemplaza Monto(string Convenio, string Categoria, string CodAdicional)
		// para que el nombre sea significativo
		public double AdicionalCategoria(string CodAdicional)
		{
			CabeceraDeCodificadoraDDO.ADICIONAL_DDO objAdic=GetCategAdicByCode(this.legajo.Convenio, this.legajo.UltimaCategoria, CodAdicional);
			if (objAdic==null) return 0;

			return objAdic.Valor;
		}

		public double Monto(string Convenio, string Categoria, int Antiguedad)
		{
			CabeceraDeCodificadoraDDO.ANTIG_DDO objAntig=GetCategAntigByCode(Convenio, Categoria, Antiguedad);
			if (objAntig==null) return 0;
			return objAntig.SueldoBasico;
		}



		public CabeceraDeCodificadoraDDO.CATEG_DDO GetCategoria()
		{
			CabeceraDeCodificadoraDDO.CATEG_DDO categ=GetCategByCode(this.legajo.Convenio, this.legajo.UltimaCategoria);
			if (categ==null)
			{
				this.GetTrace().Warning("No se encontro el Valor para la categoría");
				return null;
			}
			return categ;
		}

		public CabeceraDeCodificadoraDDO.CATEG_DDO GetCategoriaCargo()
		{
			CabeceraDeCodificadoraDDO.CATEG_DDO categ = GetCategByCode(this.cargo.Convenio, this.cargo.Categoria);
			if (categ == null)
			{
				this.GetTrace().Warning("No se encontro el Valor para la categoría");
				return null;
			}
			return categ;
		}

		public ArrayList GetCategorias(string CodigoConvenio)
		{
			CabeceraDeCodificadoraDDO.CONVENIO_DDO convenio=GetConvenioByCode(CodigoConvenio);
			if (convenio==null)
			{
				this.GetTrace().Warning("No se encontro el Convenio");
				return null;
			}

			ArrayList categs=new ArrayList(convenio.CATEG);

			IComparer myComparer=new CategComparer();
			categs.Sort(myComparer);
			return categs;
		}

		private class CategComparer:IComparer
		{
			public int Compare(Object cat1, Object cat2)
			{
				if (((CabeceraDeCodificadoraDDO.CATEG_DDO)cat1).SecuenciaNull&&((CabeceraDeCodificadoraDDO.CATEG_DDO)cat2).SecuenciaNull) return 0;
				if (((CabeceraDeCodificadoraDDO.CATEG_DDO)cat1).SecuenciaNull) return -1;
				if (((CabeceraDeCodificadoraDDO.CATEG_DDO)cat2).SecuenciaNull) return 1;

				int secuencia1=((CabeceraDeCodificadoraDDO.CATEG_DDO)cat1).Secuencia;
				int secuencia2=((CabeceraDeCodificadoraDDO.CATEG_DDO)cat2).Secuencia;

				if (secuencia1==secuencia2)
					return 0;

				if (secuencia1>secuencia2)
					return 1;
				else
					return -1;
			}
		}

		public double SueldoBasicoCategoriaAntiguedad(int antiguedad)
		{
			CabeceraDeCodificadoraDDO.ANTIG_DDO categ=GetCategAntigByCode(this.legajo.Convenio, this.legajo.UltimaCategoria, antiguedad);
			if (categ==null) return -1;

			return categ.SueldoBasico;
		}

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////    
		// LICENCIAS
		//
		public LegajoEmpresaDDO.LICEN_PER_DDO[] ObtenerLicencias(DateTime f_desde, DateTime f_hasta)
		{
			ArrayList retLicencias=new ArrayList();

			foreach (LegajoEmpresaDDO.LICEN_PER_DDO licencia in this.legajo.Licencias)
			{
				if (!licencia.FechaFinNull&&licencia.FechaFin<f_desde) continue;
				if (licencia.FechaInicio>f_hasta) continue;          
				retLicencias.Add(licencia);
			}
			return (LegajoEmpresaDDO.LICEN_PER_DDO[])retLicencias.ToArray(typeof(LegajoEmpresaDDO.LICEN_PER_DDO));
		}

		public LegajoEmpresaDDO.LICEN_PER_DDO[] ObtenerLicencias(int periodo)
		{
			ArrayList retLicencias=new ArrayList();
			DateTime minDate=LiqUtilBase.FirstDateMonth(new DateTime(periodo/100, periodo%100, 1));
			DateTime maxDate=LiqUtilBase.LastDateMonth(new DateTime(periodo/100, periodo%100, 1));


			foreach (LegajoEmpresaDDO.LICEN_PER_DDO licencia in this.legajo.Licencias)
			{
				if (!licencia.FechaFinNull&&licencia.FechaFin<minDate) continue;
				if (licencia.FechaInicio>maxDate) continue;
				retLicencias.Add(licencia);
			}
			return (LegajoEmpresaDDO.LICEN_PER_DDO[])retLicencias.ToArray(typeof(LegajoEmpresaDDO.LICEN_PER_DDO));
		}

		public LegajoEmpresaDDO.LICEN_PER_DDO[] ObtenerLicencias(string CodigoLicencia, int periodo)
		{
			ArrayList retLicencias=new ArrayList();

			foreach (LegajoEmpresaDDO.LICEN_PER_DDO licencia in ObtenerLicencias(periodo))
				if (licencia.Codigo==CodigoLicencia)
					retLicencias.Add(licencia);

			return (LegajoEmpresaDDO.LICEN_PER_DDO[])retLicencias.ToArray(typeof(LegajoEmpresaDDO.LICEN_PER_DDO));
		}

		public LegajoEmpresaDDO.LICEN_PER_DDO[] ObtenerLicencias(string CodigoLicencia, DateTime f_desde, DateTime f_hasta)
		{
			ArrayList retLicencias=new ArrayList();

			foreach (LegajoEmpresaDDO.LICEN_PER_DDO licencia in ObtenerLicencias(f_desde, f_hasta))
				if (licencia.Codigo==CodigoLicencia)
					retLicencias.Add(licencia);

			return (LegajoEmpresaDDO.LICEN_PER_DDO[])retLicencias.ToArray(typeof(LegajoEmpresaDDO.LICEN_PER_DDO));
		}

		public LegajoEmpresaDDO.LICEN_PER_DDO[] ObtenerLicencias(string CodigoLicencia, int periodo, int dia_cierre)
		{
			int periodo_ant=(periodo%100==1)?periodo-89:periodo-1;

			DateTime minDate=new DateTime(periodo_ant/100, periodo_ant%100, dia_cierre+1);
            DateTime maxDate;
            if(dia_cierre == 0)
                maxDate = new DateTime(periodo/100, periodo%100, 1).AddDays(-1);
            else
                maxDate = new DateTime(periodo/100, periodo%100, dia_cierre);

			return ObtenerLicencias(CodigoLicencia, minDate, maxDate);
		}

		public int ObtenerCantidadDiasLicencia(string CodigoLicencia, int periodo)
		{
			DateTime minDate=LiqUtilBase.FirstDateMonth(new DateTime(periodo/100, periodo%100, 1));
			DateTime maxDate=LiqUtilBase.LastDateMonth(new DateTime(periodo/100, periodo%100, 1));
			if (liquidacion.Clase=="Q")
			{
				if (liquidacion.Quincena==1) maxDate=new DateTime(periodo/100, periodo%100, 15);
				else minDate=new DateTime(periodo/100, periodo%100, 16);
			}

			return ObtenerCantidadDiasLicencia(CodigoLicencia, minDate, maxDate);
		}

		public int ObtenerCantidadDiasLicencia(string CodigoLicencia, int periodo, int dia_cierre)
		{
			int retval=0;
			DateTime f_fin, f_ini;

			if (dia_cierre==0) return ObtenerCantidadDiasLicencia(CodigoLicencia, periodo);
			if (dia_cierre>27)
			{
				this.GetTrace().Warning("El día de cierre de las licencias no debe superar el valor 27, se toma 27 por defecto.");
				dia_cierre=27;
			}
			int periodo_ant=(periodo%100==1)?periodo-89:periodo-1;

			DateTime minDate=new DateTime(periodo_ant/100, periodo_ant%100, dia_cierre+1);
			DateTime maxDate=new DateTime(periodo/100, periodo%100, dia_cierre);

			foreach (LegajoEmpresaDDO.LICEN_PER_DDO licencia in ObtenerLicencias(CodigoLicencia, periodo, dia_cierre))
			{
				f_ini=licencia.FechaInicio;
				f_fin=maxDate;
				if (!licencia.FechaFinNull) f_fin=licencia.FechaFin;
				if (!licencia.FechaInterrupcionNull) f_fin=licencia.FechaInterrupcion;
				if (f_fin>maxDate) f_fin=maxDate;
				if (f_ini<minDate) f_ini=minDate;

				retval+=LiqUtilBase.DiffDays(f_ini, f_fin)+1;
			}
			return retval;
		}

		/// <summary>Dias de licencia de tipo CodigoLicencia dentro de las fechas especificadas.</summary>
		public int ObtenerCantidadDiasLicencia(string CodigoLicencia, DateTime f_desde, DateTime f_hasta)
		{
			int retval=0;
			DateTime f_fin, f_ini;
			DateTime minDate=f_desde;
			DateTime maxDate=f_hasta;

			foreach (LegajoEmpresaDDO.LICEN_PER_DDO licencia in ObtenerLicencias(CodigoLicencia, f_desde, f_hasta))
			{
				f_ini=licencia.FechaInicio;
				f_fin=maxDate;
				if (!licencia.FechaFinNull) f_fin=licencia.FechaFin;
				if (!licencia.FechaInterrupcionNull) f_fin=licencia.FechaInterrupcion;
				if (f_fin>maxDate) f_fin=maxDate;
				if (f_ini<minDate) f_ini=minDate;

				retval+=LiqUtilBase.DiffDays(f_ini, f_fin)+1;
			}
			return retval;
		}

		/// <summary>Devuelve los días de licencia dentro del mes o quincena de la liquidación.
		/// <para>
		/// Si la liquidación es Mensual devuelve todos los días de licencia del tipo CodigoLicencia dentro del mes.
		/// Si la liquidación es Quincenal devuelve los días de licencia dentro de la quincena correspondiente a la liquidación.
		/// </para>
		/// </summary>
		public int DiasLicencia(string CodigoLicencia)
		{
			int retval=0;
			DateTime f_fin, f_ini;
			DateTime minDate;
			DateTime maxDate;
			int periodo=liquidacion.Periodo;

			minDate=new DateTime(periodo/100, periodo%100, 1);
			maxDate=LiqUtilBase.LastDateMonth(new DateTime(periodo/100, periodo%100, 1));

			if (liquidacion.Clase=="Q")
			{
				if (liquidacion.Quincena==1)
					maxDate=new DateTime(periodo/100, periodo%100, 15);
				else
					minDate=new DateTime(periodo/100, periodo%100, 16);
			}

			foreach (LegajoEmpresaDDO.LICEN_PER_DDO licencia in ObtenerLicencias(CodigoLicencia, minDate, maxDate))
			{
				f_ini=licencia.FechaInicio;
				f_fin=maxDate;
				if (!licencia.FechaFinNull) f_fin=licencia.FechaFin;
				if (!licencia.FechaInterrupcionNull) f_fin=licencia.FechaInterrupcion;
				if (f_fin>maxDate) f_fin=maxDate;
				if (f_ini<minDate) f_ini=minDate;

				retval+=LiqUtilBase.DiffDays(f_ini, f_fin)+1;
			}
			return retval;
		}

		public int ObtenerCantidadDiasLicenciaTotal(string CodigoLicencia, int periodo)
		{
			int retval=0;
			DateTime f_fin, f_ini;
			DateTime minDate=LiqUtilBase.FirstDateMonth(new DateTime(periodo/100, periodo%100, 1));
			DateTime maxDate=LiqUtilBase.LastDateMonth(new DateTime(periodo/100, periodo%100, 1));

			foreach (LegajoEmpresaDDO.LICEN_PER_DDO licencia in ObtenerLicencias(CodigoLicencia, periodo))
			{
				f_ini=licencia.FechaInicio;
				f_fin=maxDate;
				if (!licencia.FechaFinNull) f_fin=licencia.FechaFin;
				if (!licencia.FechaInterrupcionNull) f_fin=licencia.FechaInterrupcion;
				if ((licencia.FechaFinNull)&&(licencia.FechaInterrupcionNull))
				{
					this.GetTrace().Error("No se encontro fecha de fin para una licencia");
				}

				retval+=LiqUtilBase.DiffDays(f_ini, f_fin)+1;
			}
			return retval;
		}


		//Obtener licencias que inician.....
		public LegajoEmpresaDDO.LICEN_PER_DDO[] ObtenerLicenciasFI(string CodigoLicencia, DateTime f_desde, DateTime f_hasta)
		{
			ArrayList retLicencias=new ArrayList();

			foreach (LegajoEmpresaDDO.LICEN_PER_DDO licencia in this.legajo.Licencias)
			{
				if (licencia.FechaInicio<f_desde) continue;
				if (licencia.FechaInicio>f_hasta) continue;

				if ((CodigoLicencia==null)||(licencia.Codigo==CodigoLicencia))
					retLicencias.Add(licencia);
			}
			return (LegajoEmpresaDDO.LICEN_PER_DDO[])retLicencias.ToArray(typeof(LegajoEmpresaDDO.LICEN_PER_DDO));
		}

		public LegajoEmpresaDDO.LICEN_PER_DDO[] ObtenerLicenciasFI(DateTime f_desde, DateTime f_hasta)
		{
			return ObtenerLicenciasFI(null, f_desde, f_hasta);
		}

		public LegajoEmpresaDDO.LICEN_PER_DDO[] ObtenerLicenciasFI(string CodigoLicencia, int Periodo)
		{
			DateTime minDate=LiqUtilBase.FirstDateMonth(new DateTime(Periodo/100, Periodo%100, 1));
			DateTime maxDate=LiqUtilBase.LastDateMonth(new DateTime(Periodo/100, Periodo%100, 1));

			return ObtenerLicenciasFI(CodigoLicencia, minDate, maxDate);
		}

		public LegajoEmpresaDDO.LICEN_PER_DDO[] ObtenerLicenciasFI(int Periodo)
		{
			DateTime minDate=LiqUtilBase.FirstDateMonth(new DateTime(Periodo/100, Periodo%100, 1));
			DateTime maxDate=LiqUtilBase.LastDateMonth(new DateTime(Periodo/100, Periodo%100, 1));

			return ObtenerLicenciasFI(null, minDate, maxDate);
		}

		//Cantidad de Dias Por Periodo
		public int CantidadDiasLicencia(LegajoEmpresaDDO.LICEN_PER_DDO LIC, int Periodo, int Base)
		{
			DateTime minDate;
			DateTime maxDate=LiqUtilBase.LastDateMonth(new DateTime(Periodo/100, Periodo%100, 1));

			minDate=LIC.FechaInicio;
			if (!LIC.FechaFinNull) maxDate=LIC.FechaFin;
			if (!LIC.FechaInterrupcionNull) maxDate=LIC.FechaInterrupcion;


			//Calculo las fechas
			return CantidadDiasNovedad(minDate, maxDate, Periodo, Base);
		}
		public int CantidadDiasLicencia(LegajoEmpresaDDO.LICEN_PER_DDO LIC, int Periodo)
		{
			return CantidadDiasLicencia(LIC, Periodo, 0);
		}

		public int CantidadDiasNovedad(DateTime FIni, DateTime FFin, int Periodo, int Base)
		{
			DateTime minDate=LiqUtilBase.FirstDateMonth(new DateTime(Periodo/100, Periodo%100, 1));
			DateTime maxDate=LiqUtilBase.LastDateMonth(new DateTime(Periodo/100, Periodo%100, 1));
			int MonthLength=System.DateTime.DaysInMonth(Periodo/100, Periodo%100);

			//Analisa si esta en rango
			if (FIni>maxDate) return 0;
			if (FFin<minDate) return 0;

			//Calcula la fecha virtual de inicio y fin
			if (FIni>minDate) minDate=FIni;
			if (FFin<maxDate) maxDate=FFin;

			//Calculo los Dias Reales
			int retval=LiqUtilBase.DiffDays(minDate, maxDate)+1;

			if (Base!=0) retval=(int)Math.Round((double)retval*(double)Base/(double)MonthLength);
			return retval;
		}
		public int CantidadDiasNovedad(DateTime FIni, DateTime FFin, int Periodo)
		{
			return CantidadDiasNovedad(FIni, FFin, Periodo, 0);
		}

		public int ObtenerCantidadDiasLicenciaTotalFI(string CodigoLicencia, int periodo)
		{
			//Esta la uso para pagar vacaciones. Me devuelve la cantidad de dias total que suman las licencias que empiezan en el periodo
			int retval=0;

			DateTime f_fin, f_ini;
			DateTime minDate=LiqUtilBase.FirstDateMonth(new DateTime(periodo/100, periodo%100, 1));
			DateTime maxDate=LiqUtilBase.LastDateMonth(new DateTime(periodo/100, periodo%100, 1));

			foreach (LegajoEmpresaDDO.LICEN_PER_DDO licencia in ObtenerLicenciasFI(CodigoLicencia, periodo))
			{
				f_ini=licencia.FechaInicio;
				f_fin=maxDate;
				if (!licencia.FechaFinNull) f_fin=licencia.FechaFin;
				if (!licencia.FechaInterrupcionNull) f_fin=licencia.FechaInterrupcion;
				if ((licencia.FechaFinNull)&&(licencia.FechaInterrupcionNull))
				{
					this.GetTrace().Error("No se encontro fecha de fin para una licencia");
				}

				retval+=LiqUtilBase.DiffDays(f_ini, f_fin)+1;
			}
			return retval;
		}

        public int ObtenerCantidadDiasLicenciaTotalFIH(string CodigoLicencia, int periodo, int diaCorte)
        {
            if (CodigoLicencia != "V") return 0;

            //Esta la uso para pagar vacaciones hábiles. Me devuelve la cantidad de dias total que suman las licencias hábiles que empiezan en el periodo
            int retval = 0, cantDias, inicio;

            DateTime f_fin, f_ini, f_ini_lic, f_fin_lic,minDate,maxDate;
            LegajoEmpresaDDO.LICEN_PER_DDO[] licencias;

            if(diaCorte == 0)
            {
                minDate = LiqUtilBase.FirstDateMonth(new DateTime(periodo / 100, periodo % 100, 1));
                maxDate = LiqUtilBase.LastDateMonth(new DateTime(periodo / 100, periodo % 100, 1));

                if (liquidacion.Clase == "Q")
                {
                    if (liquidacion.Quincena == 1)
                        maxDate = new DateTime(periodo / 100, periodo % 100, 15);
                    else
                        minDate = new DateTime(periodo / 100, periodo % 100, 16);
                }
                licencias = ObtenerLicencias(CodigoLicencia,periodo);
            }
            else
            {
                if (diaCorte > 27)
                {
                    this.GetTrace().Warning("El día de cierre de las licencias no debe superar el valor 27, se toma 27 por defecto.");
                    diaCorte = 27;
                }
                //int periodo_ant = (periodo % 100 == 1) ? periodo - 89 : periodo - 1;

                minDate = new DateTime(periodo / 100, periodo % 100, 1);
                maxDate = new DateTime(periodo / 100, periodo % 100, diaCorte!=0 ? diaCorte : 1);
                licencias = ObtenerLicencias(CodigoLicencia,periodo, diaCorte != 0 ? diaCorte : 1);
            }
           
            foreach (LegajoEmpresaDDO.LICEN_PER_DDO licencia in licencias)
            {
                if (!licencia.Habiles) continue;

                f_ini_lic = licencia.FechaInicio;
                f_fin_lic = licencia.FechaFin;

                f_ini = licencia.FechaInicio;
                f_fin = maxDate;
                if (!licencia.FechaFinNull) f_fin = licencia.FechaFin;
                if (!licencia.FechaInterrupcionNull) f_fin = licencia.FechaInterrupcion;
                if (f_fin > maxDate) f_fin = maxDate;
                if (f_ini < minDate) f_ini = minDate;
                if ((licencia.FechaFinNull) && (licencia.FechaInterrupcionNull))
                {
                    this.GetTrace().Error("No se encontro fecha de fin para una licencia");
                }

                if (f_ini <= f_fin)
                {
                    if (f_ini_lic < f_ini)
                    {
                        inicio = LiqUtilBase.DiffDays(f_ini_lic, f_ini);
                    }
                    else
                    {
                        inicio = 0;
                    }
                    cantDias = LiqUtilBase.DiffDays(f_ini, f_fin);

                    string split = licencia.d_habiles.Substring(inicio, cantDias + 1);

                    int cant = 0;
                    foreach (char letra in split)
                    {
                        if (letra == '1')
                            cant++;
                    }

                    retval += cant;
                }
            }

            return retval;
        }

        public int ObtenerCantidadDiasLicenciaTotalFIH(string CodigoLicencia, int periodo)
        {
            return ObtenerCantidadDiasLicenciaTotalFIH(CodigoLicencia,periodo, 0);
        }

		public int ObtenerCantidadDiasLicenciaTotalFI(string CodigoLicencia, int periodo, int periodoCalculo, int Base)
		{
			//Esta la uso para acumular sac.  
			int retval=0;

			foreach (LegajoEmpresaDDO.LICEN_PER_DDO licencia in ObtenerLicenciasFI(CodigoLicencia, periodo))
			{
				retval+=CantidadDiasLicencia(licencia, periodoCalculo, Base);
			}
			return retval;
		}

		public int ObtenerCantidadDiasLicenciaTotalFI(string CodigoLicencia, int periodo, int periodoCalculo)
		{
			return ObtenerCantidadDiasLicenciaTotalFI(CodigoLicencia, periodo, periodoCalculo, 0);
		}

        public int ObtenerCantidadDiasLicenciaLegalesCorte(string CodigoLicencia, int periodo, int dia_cierre, bool contarFebrero)
        {
            DateTime minDate = new DateTime(periodo / 100, periodo % 100, 1);
            DateTime maxDate = new DateTime(periodo / 100, periodo % 100, dia_cierre);

            return CantidadDiasLegalesCorte(CodigoLicencia, periodo, dia_cierre, minDate, maxDate, contarFebrero);
        }

        public int ObtenerCantidadDiasLicenciaLegales(string CodigoLicencia, int periodo, int dia_cierre, bool contarFebrero)
        {
            //int retval = 0;
            //DateTime f_fin, f_ini;
          
            if (dia_cierre > 27)
            {
                this.GetTrace().Warning("El día de cierre de las licencias no debe superar el valor 27, se toma 27 por defecto.");
                dia_cierre = 27;
            }
            int periodo_ant = (periodo % 100 == 1) ? periodo - 89 : periodo - 1;

            DateTime minDate = new DateTime(periodo_ant / 100, periodo_ant % 100, dia_cierre + 1);
            DateTime maxDate;
            //int agregarDias = 1;
            if (dia_cierre != 0)
                maxDate = new DateTime(periodo / 100, periodo % 100, dia_cierre);
            else
            {
                maxDate = new DateTime(periodo / 100, periodo % 100, 1).AddDays(-1);

                if (periodo == 202003)
                    maxDate = maxDate.AddDays(1); 
            }

            return CantidadDiasLegalesCorte(CodigoLicencia, periodo, dia_cierre, minDate, maxDate, contarFebrero);    
            /*foreach (LegajoEmpresaDDO.LICEN_PER_DDO licencia in ObtenerLicencias(CodigoLicencia, periodo, dia_cierre))
            {
                f_ini = licencia.FechaInicio;
                f_fin = maxDate;
                if (!licencia.FechaFinNull) f_fin = licencia.FechaFin;
                if (!licencia.FechaInterrupcionNull) f_fin = licencia.FechaInterrupcion;
                if (f_fin > maxDate) f_fin = maxDate;
                if (f_ini < minDate) f_ini = minDate;

                if (periodo == 202003 && f_fin == maxDate && dia_cierre == 0)
                    agregarDias = 0;

                int diasDiferencia = LiqUtilBase.DiffDays(f_ini, f_fin) + agregarDias;
                //cuando es feberero y la licencia se pasa de febrero a marzo
                if(contarFebrero)
                {
                    if (f_ini.Month <= 2 && f_fin.Month >= 3)
                    {
                        if (DateTime.DaysInMonth(f_fin.Year, 2) == 28)
                            diasDiferencia += 2;
                        if (DateTime.DaysInMonth(f_fin.Year, 2) == 29)
                            diasDiferencia += 1;
                    }
                }

                for (DateTime dia = f_ini; dia <= f_fin; dia = dia.AddDays(1))
                {
                    if(dia.Day == 31)
                        diasDiferencia -= 1;
                }
                    
                retval += diasDiferencia;
            }
            return retval;*/
        }
        public int CantidadDiasLegalesCorte(string CodigoLicencia,int periodo, int dia_cierre,DateTime minDate, DateTime maxDate,bool contarFebrero)
        {
            int retval = 0;
            DateTime f_fin, f_ini;
            int agregarDias = 1;
            foreach (LegajoEmpresaDDO.LICEN_PER_DDO licencia in ObtenerLicencias(CodigoLicencia, periodo, dia_cierre))
            {
                f_ini = licencia.FechaInicio;
                f_fin = maxDate;
                if (!licencia.FechaFinNull) f_fin = licencia.FechaFin;
                if (!licencia.FechaInterrupcionNull) f_fin = licencia.FechaInterrupcion;
                if (f_fin > maxDate) f_fin = maxDate;
                if (f_ini < minDate) f_ini = minDate;

                if (periodo == 202003 && f_fin == maxDate && dia_cierre == 0)
                    agregarDias = 0;

                int diasDiferencia = LiqUtilBase.DiffDays(f_ini, f_fin) + agregarDias;
                //cuando es feberero y la licencia se pasa de febrero a marzo
                if (contarFebrero)
                {
                    if (f_ini.Month <= 2 && f_fin.Month >= 3)
                    {
                        if (DateTime.DaysInMonth(f_fin.Year, 2) == 28)
                            diasDiferencia += 2;
                        if (DateTime.DaysInMonth(f_fin.Year, 2) == 29)
                            diasDiferencia += 1;
                    }
                }

                for (DateTime dia = f_ini; dia <= f_fin; dia = dia.AddDays(1))
                {
                    if (dia.Day == 31)
                        diasDiferencia -= 1;
                }

                retval += diasDiferencia;
            }
            return retval;
        }

        public int ObtenerCantidadDiasLicenciaLegalesHabil(string CodigoLicencia, int periodo, int dia_cierre, bool contarFebrero, bool lav)
        {
   
            if (dia_cierre > 27)
            {
                this.GetTrace().Warning("El día de cierre de las licencias no debe superar el valor 27, se toma 27 por defecto.");
                dia_cierre = 27;
            }
            int periodo_ant = (periodo % 100 == 1) ? periodo - 89 : periodo - 1;

            DateTime minDate = new DateTime(periodo_ant / 100, periodo_ant % 100, dia_cierre + 1);
            DateTime maxDate = new DateTime(periodo / 100, periodo % 100, dia_cierre != 0 ? dia_cierre : 1);

            return ContarDias(CodigoLicencia, periodo, dia_cierre, minDate, maxDate, contarFebrero, lav);
        
        }

        public int ObtenerCantidadDiasLicenciaLegalesHabilConCorte(string CodigoLicencia, int periodo, int dia_cierre, bool contarFebrero, bool lav)
        {
            DateTime minDate = new DateTime(periodo / 100, periodo % 100, 1);
            DateTime maxDate = new DateTime(periodo / 100, periodo % 100, dia_cierre);

            return ContarDias(CodigoLicencia, periodo, dia_cierre, minDate, maxDate, contarFebrero, lav);   
        }

        private int ContarDias(string CodigoLicencia,int periodo ,int dia_cierre,DateTime minDate,DateTime maxDate,bool contarFebrero,bool lav)
        {
            DateTime f_fin, f_ini;
            int retval = 0;
            foreach (LegajoEmpresaDDO.LICEN_PER_DDO licencia in ObtenerLicencias(CodigoLicencia, periodo, dia_cierre != 0 ? dia_cierre : 1))
            {
                f_ini = licencia.FechaInicio;
                f_fin = maxDate;
                if (!licencia.FechaFinNull) f_fin = licencia.FechaFin;
                if (!licencia.FechaInterrupcionNull) f_fin = licencia.FechaInterrupcion;
                if (f_fin > maxDate) f_fin = maxDate;
                if (f_ini < minDate) f_ini = minDate;

                //int diasDiferencia = LiqUtilBase.DiffDays(f_ini, f_fin) + 1;
                //cuando es feberero y la licencia se pasa de febrero a marzo
                int diasFicticios = 0;
                if (contarFebrero)
                {
                    if (f_ini.Month == 2 && f_fin.Month == 3)
                    {
                        if (DateTime.DaysInMonth(f_ini.Year, f_ini.Month) == 28)
                            diasFicticios += 2;
                        if (DateTime.DaysInMonth(f_ini.Year, f_ini.Month) == 29)
                            diasFicticios += 1;
                    }

                    //cuando el rango de fechas tiene al 31
                    if (DateTime.DaysInMonth(f_ini.Year, f_ini.Month) == 31)
                    {
                        DateTime fechacon31 = new DateTime(f_ini.Year, f_ini.Month, 31);
                        if (diasFicticios > 1 && fechacon31 >= f_ini && fechacon31 <= f_fin)
                            diasFicticios -= 1;
                    }
                }

                int cantidadHabiles = diasFicticios;

                cantidadHabiles += ContarDiasHabiles(f_ini, f_fin, lav);

                retval += cantidadHabiles;
            }
            return retval;
        }
        /*public int ContarDiasHabiles(DateTime fechaDesde, DateTime fechaHasta,bool lav)
        {
            int cantidadDias = 0;

            string where = "PER02_PERSONAL_EMP.e_numero_legajo =\\\'"+this.legajo.NroLegajo+"\\\'";
            string oi_per_emp = null;

            oi_per_emp = new NomadXML(NomadEnvironment.QueryString(NucleusRH.Base.Personal.LegajoEmpresa.PERSONAL_EMP.Resources.INFO, "<PARAM OI_PARENT='" + this.legajo.Empresa + "' WHERE=\"" + where + "\" />")).FirstChild().FirstChild().GetAttr("ID");
                      
            NucleusRH.Base.Personal.LegajoEmpresa.PERSONAL_EMP obj_per = NucleusRH.Base.Personal.LegajoEmpresa.PERSONAL_EMP.Get(oi_per_emp,false);

            for (System.DateTime fecha = fechaDesde; fecha <= fechaHasta; fecha = fecha.AddDays(1))
                if (DiaLaboral(fecha, obj_per,lav))
                    cantidadDias++;
                
            return cantidadDias;
		
        }
        */

        public int ContarDiasHabiles(DateTime fechaDesde, DateTime fechaHasta, bool lav)
        {
            if (matrizFeriados == null)
            {
                this.GetTrace().Warning("Adic.ContarDiasHabiles - La matriz de feriados no esta cargada");
                return -1;
            }
            int cantidadDias = 0;

            for(DateTime dia = fechaDesde; dia.Date <= fechaHasta.Date; dia = dia.AddDays(1))
            {
                int m = matrizFeriados.Length - 12;
                if (dia.Year < liquidacion.Periodo / 100)
                    m -= (12 - dia.Month + 1);                 
                else
                    m += dia.Month - 1 ;

                int[] mes = matrizFeriados[m];
                if (Array.IndexOf(mes, dia.Day) < 0 && ((lav && dia.DayOfWeek != DayOfWeek.Saturday && dia.DayOfWeek != DayOfWeek.Sunday) || (!lav && dia.DayOfWeek != DayOfWeek.Sunday)))             
                    cantidadDias++;  
            }
        
            return cantidadDias;
        }
        /*
        public bool DiaLaboral(DateTime fecha,Personal.LegajoEmpresa.PERSONAL_EMP perEmp,bool lav)
        {
            bool retval = false;

            //Obtengo el Calendario
            NucleusRH.Base.Organizacion.Calendario_Feriados.CALENDARIO obj_Cal = null;
            foreach (NucleusRH.Base.Personal.LegajoEmpresa.CALENDARIO_PER MyCAL in perEmp.CALENDARIO_PER)
                if ((fecha >= MyCAL.f_desde) && (MyCAL.f_hastaNull || fecha <= MyCAL.f_hasta))
                    obj_Cal = MyCAL.Getoi_calendario();
            if (obj_Cal == null)
                throw new NomadAppException("La persona no tiene un Calendario Definido para la Fecha Especificada.");

            //Calculo si ese Dia TRABAJA.
            if (obj_Cal.DIAS_FERIADOS.GetByAttribute("f_feriado", fecha) != null)
            {
                retval = obj_Cal.l_trab_feriados;
            }
            else
            {
                switch (fecha.DayOfWeek)
                {
                    case System.DayOfWeek.Monday: retval = obj_Cal.l_trab_lunes; break;
                    case System.DayOfWeek.Tuesday: retval = obj_Cal.l_trab_martes; break;
                    case System.DayOfWeek.Wednesday: retval = obj_Cal.l_trab_miercoles; break;
                    case System.DayOfWeek.Thursday: retval = obj_Cal.l_trab_jueves; break;
                    case System.DayOfWeek.Friday: retval = obj_Cal.l_trab_viernes; break;
                    case System.DayOfWeek.Saturday: retval = !lav; break;
                    case System.DayOfWeek.Sunday: retval = obj_Cal.l_trab_domingos; break;
                }
            }
 
            return retval;
        }
        */
        /* Licencias de cargos */
		
		/// <summary>Devuelve la lista de licencias de cargo del tipo CodigoLicencia que se solapen con el periodo indicado</summary>
		public LegajoEmpresaDDO.LICEN_CARGO_DDO[] ObtenerLicenciasCargo(string CodigoLicencia, DateTime f_desde, DateTime f_hasta)
		{
			VerificarConceptoCargo();
			ArrayList retLicencias = new ArrayList();

			foreach (LegajoEmpresaDDO.LICEN_CARGO_DDO lic in this.cargo.Licencias)
			{
				if (lic.Licencia != CodigoLicencia) continue;
				if (lic.FechaInicio > f_hasta) continue;
				if (!lic.FechaFinNull && lic.FechaFin < f_desde) continue;
				retLicencias.Add(lic);
			}

			return (LegajoEmpresaDDO.LICEN_CARGO_DDO[])retLicencias.ToArray(typeof(LegajoEmpresaDDO.LICEN_CARGO_DDO));
		}
		
		/// <summary>Devuelve la lista de licencias de cargo del tipo CodigoLicencia que comiencen dentro del periodo indicado</summary>
		public LegajoEmpresaDDO.LICEN_CARGO_DDO[] ObtenerLicenciasCargoFI(string CodigoLicencia, DateTime f_desde, DateTime f_hasta)
		{
			VerificarConceptoCargo();
			ArrayList retLicencias = new ArrayList();

			foreach (LegajoEmpresaDDO.LICEN_CARGO_DDO licencia in this.cargo.Licencias)
			{
				if (licencia.Licencia!=CodigoLicencia) continue;
				if (licencia.FechaInicio<f_desde) continue;
				if (licencia.FechaInicio>f_hasta) continue;
				retLicencias.Add(licencia);
			}
			return (LegajoEmpresaDDO.LICEN_CARGO_DDO[])retLicencias.ToArray(typeof(LegajoEmpresaDDO.LICEN_CARGO_DDO));
		}


		/// <summary>Devuelve los días de licencia de cargo dentro del mes o quincena de la liquidación.
		/// <para>
		/// Si la liquidación es Mensual devuelve todos los días de licencia del tipo CodigoLicencia dentro del mes.
		/// Si la liquidación es Quincenal devuelve los días de licencia dentro de la quincena correspondiente a la liquidación.
		/// </para>
		/// </summary>
		public int DiasLicenciaCargo(string CodigoLicencia)
		{
			VerificarConceptoCargo();
			int retval = 0;
			DateTime f_fin, f_ini;
			DateTime minDate;
			DateTime maxDate;
			int periodo=liquidacion.Periodo;

			minDate=new DateTime(periodo/100, periodo%100, 1);
			maxDate=LiqUtilBase.LastDateMonth(new DateTime(periodo/100, periodo%100, 1));

			if (liquidacion.Clase=="Q")
			{
				if (liquidacion.Quincena==1)
					maxDate=new DateTime(periodo/100, periodo%100, 15);
				else
					minDate=new DateTime(periodo/100, periodo%100, 16);
			}

			foreach (LegajoEmpresaDDO.LICEN_CARGO_DDO licencia in ObtenerLicenciasCargo(CodigoLicencia, minDate, maxDate))
			{
				f_ini=licencia.FechaInicio;
				f_fin=maxDate;
				if (!licencia.FechaFinNull) f_fin=licencia.FechaFin;
				if (f_fin>maxDate) f_fin=maxDate;
				if (f_ini<minDate) f_ini=minDate;

				retval+=LiqUtilBase.DiffDays(f_ini, f_fin)+1;
			}
			return retval;
		}

		/// <summary>Devuelve los días de licencia de cargo que inicien dentro del mes o quincena de la liquidación.
		/// <para>
		/// Si la liquidación es Mensual devuelve todos los días de licencia del tipo CodigoLicencia que comiencen dentro del mes.
		/// Si la liquidación es Quincenal devuelve los días de licencia que comiencen en la quincena correspondiente a la liquidación.
		/// </para>
		/// </summary>
		public int DiasLicenciaCargoFI(string CodigoLicencia)
		{
			VerificarConceptoCargo();
			int retval = 0;
			DateTime minDate;
			DateTime maxDate;
			int periodo=liquidacion.Periodo;

			minDate=new DateTime(periodo/100, periodo%100, 1);
			maxDate=LiqUtilBase.LastDateMonth(new DateTime(periodo/100, periodo%100, 1));

			if (liquidacion.Clase=="Q")
			{
				if (liquidacion.Quincena==1)
					maxDate=new DateTime(periodo/100, periodo%100, 15);
				else
					minDate=new DateTime(periodo/100, periodo%100, 16);
			}

			foreach (LegajoEmpresaDDO.LICEN_CARGO_DDO licencia in ObtenerLicenciasCargoFI(CodigoLicencia, minDate, maxDate))
			{
				if (licencia.FechaFinNull) continue;
				retval+=LiqUtilBase.DiffDays(licencia.FechaInicio, licencia.FechaFin)+1;
			}
			return retval;
		}

		/* Fin Licencias de cargos */

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////    
		// ANTICIPOS
		//
    public LegajoEmpresaDDO.ANTICIPO_DDO[] ObtenerAnticipos(string Tipos, string Estados, string LiquidaAutomatica)
    {
      ArrayList retAnticipos=new ArrayList();

       //Analiso el liquida automatico
       if (!string.IsNullOrEmpty(LiquidaAutomatica) && (LiquidaAutomatica.ToUpper() == "S" || LiquidaAutomatica.ToUpper() == "N"))
         LiquidaAutomatica = LiquidaAutomatica.ToUpper();
       else
         LiquidaAutomatica = null;

       //Analiso el tipo
       if (!string.IsNullOrEmpty(Tipos))
         Tipos = ";" + Tipos.ToUpper() + ";";
       else
         Tipos = null;
       
       //Analizo los estados
       if (!string.IsNullOrEmpty(Estados))
         Estados = ";" + Estados.ToUpper() + ";";
       else
         Estados = null;

      foreach (LegajoEmpresaDDO.ANTICIPO_DDO anticipo in this.legajo.Anticipos)
      {
        //Valida el liquida automatica
        if (LiquidaAutomatica != null)
        {
          if (!anticipo.LiquidaAutomatica && LiquidaAutomatica == "S") continue;
          if (anticipo.LiquidaAutomatica && LiquidaAutomatica == "N") continue;
        }

        //Valida el tipo de liquidacion
        if (Tipos != null)
        {
          if (string.IsNullOrEmpty(anticipo.TipoAnticipo)) continue;
          if (!Tipos.Contains(";"+anticipo.TipoAnticipo.ToUpper()+";"))
            continue;
        }

        //Valida la lista de estados
        if (Estados != null)
        {
          if (!Estados.Contains(";"+anticipo.Estado.ToUpper()+";"))
            continue;
        }

        //Agrego el resultado
        retAnticipos.Add(anticipo);
      }
      return (LegajoEmpresaDDO.ANTICIPO_DDO[])retAnticipos.ToArray(typeof(LegajoEmpresaDDO.ANTICIPO_DDO));
    }

		public LegajoEmpresaDDO.ANTICIPO_DDO ObtenerAnticipoOtorgado()
		{
			LegajoEmpresaDDO.ANTICIPO_DDO[] Anticipos = ObtenerAnticiposOtorgados();
			return Anticipos != null && Anticipos.Length > 0 ? Anticipos[0] : null;
		}
		public LegajoEmpresaDDO.ANTICIPO_DDO[] ObtenerAnticiposOtorgados()
		{
			return ObtenerAnticipos(null, "O", "S");;
		}

		public LegajoEmpresaDDO.ANTICIPO_DDO[] ObtenerAnticiposADescontar()
		{
			return ObtenerAnticiposADescontar(0);
		}

		public LegajoEmpresaDDO.ANTICIPO_DDO[] ObtenerAnticiposADescontar(int Periodo)
		{
			ArrayList retAnticipos=new ArrayList();

			foreach (LegajoEmpresaDDO.ANTICIPO_DDO anticipo in this.legajo.Anticipos)
			{
				if (!anticipo.LiquidaAutomatica) continue;
				if ((anticipo.Estado=="L")||(anticipo.Estado=="D"))
					if ((anticipo.Periodo<=Periodo)||(Periodo==0))
						retAnticipos.Add(anticipo);
			}

			return (LegajoEmpresaDDO.ANTICIPO_DDO[])retAnticipos.ToArray(typeof(LegajoEmpresaDDO.ANTICIPO_DDO));
		}

		public LegajoEmpresaDDO.DESC_ANT_DDO[] ObtenerDescuentosAnticipos(LegajoEmpresaDDO.ANTICIPO_DDO anticipo, string estado)
		{
            ArrayList retDesc=new ArrayList();
			foreach (LegajoEmpresaDDO.DESC_ANT_DDO descuento in anticipo.DescuentosAnticipo)
				if (descuento.Estado==estado)
					retDesc.Add(descuento);
			return (LegajoEmpresaDDO.DESC_ANT_DDO[])retDesc.ToArray(typeof(LegajoEmpresaDDO.DESC_ANT_DDO));
		}

		public LegajoEmpresaDDO.DESC_ANT_DDO[] ObtenerDescuentosAnticipos(LegajoEmpresaDDO.ANTICIPO_DDO anticipo, string estado, System.DateTime fechaDesde)
		{
			ArrayList retDesc=new ArrayList();
			foreach (LegajoEmpresaDDO.DESC_ANT_DDO descuento in anticipo.DescuentosAnticipo)
				if (descuento.Estado==estado)
					if (descuento.FechaNull)
						retDesc.Add(descuento);
					else if (descuento.Fecha<=fechaDesde)
						retDesc.Add(descuento);
			return (LegajoEmpresaDDO.DESC_ANT_DDO[])retDesc.ToArray(typeof(LegajoEmpresaDDO.DESC_ANT_DDO));
		}

		public double ObtenerTotalDescuentos(LegajoEmpresaDDO.ANTICIPO_DDO anticipo, string estado)
		{
			double total=0.0;
			foreach (LegajoEmpresaDDO.DESC_ANT_DDO descuento in anticipo.DescuentosAnticipo)
				if (descuento.Estado==estado)
					total+=descuento.Importe;
			return total;
		}

		public double ObtenerTotalDescuentos(LegajoEmpresaDDO.ANTICIPO_DDO anticipo, string estado, System.DateTime fechaDesde)
		{
			double total=0.0;
			foreach (LegajoEmpresaDDO.DESC_ANT_DDO descuento in anticipo.DescuentosAnticipo)
				if (descuento.Estado==estado)
					if (descuento.Fecha<=fechaDesde)
						total+=descuento.Importe;
			return total;
		}

		public double ObtenerTotalAnticipos(LegajoEmpresaDDO.ANTICIPO_DDO[] anticipos, string estado, System.DateTime fechaDesde)
		{
			double total=0.0;
			for (int i=0; i<=anticipos.Length; i++)
				total+=ObtenerTotalDescuentos(anticipos[i], estado, fechaDesde);
			return total;
		}

		public double ObtenerTotalAnticipos(LegajoEmpresaDDO.ANTICIPO_DDO[] anticipos, string estado)
		{
			double total=0.0;
			for (int i=0; i<=anticipos.Length; i++)
				total+=ObtenerTotalDescuentos(anticipos[i], estado);
			return total;
		}

		public void OtorgarAnticipo(LegajoEmpresaDDO.ANTICIPO_DDO anticipo)
		{
			Recibos.ANT_LIQ_PER newDDO=new Recibos.ANT_LIQ_PER();

			newDDO.n_valor=anticipo.Importe;
			newDDO.oi_anticipo_ddo=anticipo.Id;
			this.recibo.ANT_LIQ_PER.Add(newDDO);
		}

		public void DescontarAnticipo(LegajoEmpresaDDO.DESC_ANT_DDO descuento)
		{
			//invertimos el signo, ya que en la dant_tot_liq se almacena los valores a descontar.
			Recibos.DANT_LIQ_PER newDDO=new Recibos.DANT_LIQ_PER();

			newDDO.n_valor=-descuento.Importe;
			newDDO.oi_desc_ant_ddo=descuento.Id;

			this.recibo.DANT_LIQ_PER.Add(newDDO);
		}

        
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////    
        // CONSUMOS
        //
        public LegajoEmpresaDDO.CONSUMO_DDO ObtenerConsumo()
        {
            return ObtenerConsumo("");
        }

        public LegajoEmpresaDDO.CONSUMO_DDO[] ObtenerConsumos()
        {
            return ObtenerConsumos("");
        }

        public void GuardarConsumo(LegajoEmpresaDDO.CONSUMO_DDO consumo)
        {
            Recibos.CON_LIQ_PER newDDO = new Recibos.CON_LIQ_PER();

            newDDO.n_valor = consumo.Importe;
            newDDO.oi_consumo_ddo = consumo.Id;
            
            this.recibo.CON_LIQ_PER.Add(newDDO);
        }

        public LegajoEmpresaDDO.CONSUMO_DDO ObtenerConsumo(string tipoConsumo)
        {
         
            foreach (LegajoEmpresaDDO.CONSUMO_DDO consumo in this.legajo.Consumos)
            {
                if ((consumo.TipoConsumo == tipoConsumo || tipoConsumo == "") && consumo.Estado == "A")
                    return consumo;
            }

            return null;
        }

        public LegajoEmpresaDDO.CONSUMO_DDO[] ObtenerConsumos(string tipoConsumo)
        {
            ArrayList retConsumos = new ArrayList();
             
            foreach (LegajoEmpresaDDO.CONSUMO_DDO consumo in this.legajo.Consumos)
            {
                if((consumo.TipoConsumo == tipoConsumo || tipoConsumo == "") && consumo.Estado == "A")
                    retConsumos.Add(consumo);
            }

            return (LegajoEmpresaDDO.CONSUMO_DDO[])retConsumos.ToArray(typeof(LegajoEmpresaDDO.CONSUMO_DDO));
        }

        public double ObtenerTotalConsumos(LegajoEmpresaDDO.CONSUMO_DDO[] consumos, string tipoConsumo)
        {
            double total = 0.0;
            foreach (LegajoEmpresaDDO.CONSUMO_DDO c in consumos)
                total += c.Importe;
            return total;
        }

        public double ObtenerTotalConsumos(string tipoConsumo)
        {
            LegajoEmpresaDDO.CONSUMO_DDO[] consumos = ObtenerConsumos(tipoConsumo);
            double total = 0.0;
            foreach (LegajoEmpresaDDO.CONSUMO_DDO c in consumos)
                total += c.Importe;
            return total;
        }


		////////////////////////////////////////////////////////////////////////////////////////////////////////////////    
		// IMPUESTO A LA GANANACIA
		//
		public int IDIGperiodo=0;
		public Hashtable IDIG=null;

		public double ImporteIDIG(int anio, int mes, string item_ig)
		{
			if ((IDIG==null)||(IDIGperiodo!=anio*100+mes))
			{
				IDIG=new Hashtable();
				IDIGperiodo=anio*100+mes;

				foreach (CabeceraDeCodificadoraDDO.IDIG_DDO objItem in this.codificadoras.IDIG)
					if ((objItem.Anio==anio)&&(objItem.Mes==mes))
						IDIG[objItem.ItemIG.ToString()]=objItem.Importe;
			}
			if (!IDIG.ContainsKey(item_ig))
			{
				this.GetTrace().Error("El Importe IDIG '"+item_ig+"' para el "+mes.ToString()+"/"+anio.ToString()+" no encontrado...");
				return 0;
			}

			return (double)IDIG[item_ig];
		}


		////////////////////////////////////////////////////////////////////////////////////////////////////////////////    
		// PARAMETROS
		//

		public double ObtenerValorParametro(string nombre, double valorDefault)
		{
			LegajoEmpresaDDO.PARAM_EMP_DDO PARLEG;
			EmpresasDDO.PARAM_EMP_DDO PAREMP;


			PARLEG=(LegajoEmpresaDDO.PARAM_EMP_DDO)legajo.Parametros.GetByAttribute("Parametro", nombre);
			if (PARLEG!=null) return Nomad.NSystem.Functions.StringUtil.str2dbl(PARLEG.Valor);

			PAREMP=(EmpresasDDO.PARAM_EMP_DDO)empresa.Parametros.GetByAttribute("Parametro", nombre);
			if (PAREMP!=null) return Nomad.NSystem.Functions.StringUtil.str2dbl(PAREMP.Valor);

			return valorDefault;
		}

		public double ObtenerValorParametro(string nombre)
		{
			return ObtenerValorParametro(nombre, 0d);
		}

		public bool ExisteValorParametro(string nombre)
		{
			LegajoEmpresaDDO.PARAM_EMP_DDO PARLEG;
			EmpresasDDO.PARAM_EMP_DDO PAREMP;


			PARLEG=(LegajoEmpresaDDO.PARAM_EMP_DDO)legajo.Parametros.GetByAttribute("Parametro", nombre);
			if (PARLEG!=null) return true;

			PAREMP=(EmpresasDDO.PARAM_EMP_DDO)empresa.Parametros.GetByAttribute("Parametro", nombre);
			if (PAREMP!=null) return true;

			return false;
		}

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////    
		// EMBARGOS
		//
		public Recibos.CONC_LIQ_PER ObtenerConcepto(string Concepto)
		{
			CabeceraDeConceptosDDO.CONCEPTO_DDO MyCONC=(CabeceraDeConceptosDDO.CONCEPTO_DDO)conceptos.CONCEPTOS.GetByAttribute("Codigo", Concepto);
			if (MyCONC==null) return null;

			Recibos.CONC_LIQ_PER MyRet=(Recibos.CONC_LIQ_PER)Recibo.CONC_LIQ_PER.GetByAttribute("oi_concepto_ddo", MyCONC.Id);
			if (MyRet==null) return null;

			return MyRet;
		}

		public double ConceptoValor(string Concepto)
		{
			Recibos.CONC_LIQ_PER conc=ObtenerConcepto(Concepto);
			if (conc==null) return 0;
			return conc.n_valor;
		}

		public double ConceptoCantidad(string Concepto)
		{
			Recibos.CONC_LIQ_PER conc=ObtenerConcepto(Concepto);
			if (conc==null) return 0;
			return conc.n_cantidad;
		}

		public double ConceptoValor(LegajoEmpresaDDO.EMBARGO_DDO embargo) { return ConceptoValor(embargo.Concepto); }
		public double ConceptoCantidad(LegajoEmpresaDDO.EMBARGO_DDO embargo) { return ConceptoCantidad(embargo.Concepto); }

		public ArrayList ObtenerEmbargos(string tipoEmbargo)
		{
			ArrayList retval=new ArrayList();

			foreach (LegajoEmpresaDDO.EMBARGO_DDO embargo in legajo.Embargos)
			{
				if (!embargo.LiquidaAutomatico) continue;
				if (embargo.Estado!="A"&&embargo.Estado!="I") continue;
				if (embargo.TipoEmbargo.StartsWith(tipoEmbargo)) retval.Add(embargo);
			}

			return retval;
		}

		public LegajoEmpresaDDO.EMBARGO_DDO ObtenerEmbargoComercial()
		{
			ArrayList retval=ObtenerEmbargos("OC");
			if (retval.Count==0) return null;
			return (LegajoEmpresaDDO.EMBARGO_DDO)retval[0];
		}

		public void DescontarEmbargo(LegajoEmpresaDDO.EMBARGO_DDO embargo, double valor)
		{
			Recibos.EMB_LIQ_PER newDDO=new Recibos.EMB_LIQ_PER();
			newDDO.oi_embargo_ddo=embargo.Id;
			newDDO.n_cantidad=1;
			newDDO.n_valor=valor;

			Recibo.EMB_LIQ_PER.Add(newDDO);
		}

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////    
		// IG
		//
		public Hashtable ITEM_IG=null;

		public CabeceraDeCodificadoraDDO.ITEM_DDO GetItemIG(string codigo)
		{

			if (ITEM_IG==null)
			{
				ITEM_IG=new Hashtable();

				foreach (CabeceraDeCodificadoraDDO.ITEM_DDO objItem in this.codificadoras.ITEMS_IG)
					ITEM_IG[objItem.Codigo]=objItem;
			}

			if (!ITEM_IG.ContainsKey(codigo))
			{
				this.GetTrace().Error("El Codigo de Item IG '"+codigo+"' no encontrado...");
				return null;
			}

			return (CabeceraDeCodificadoraDDO.ITEM_DDO)ITEM_IG[codigo];
		}


		public void AddItemIG(string codigo, double valor, double cantidad)
		{
			CabeceraDeCodificadoraDDO.ITEM_DDO itemIG=GetItemIG(codigo);
			if (itemIG==null) return;

			Recibos.VALIG newDDO=new Recibos.VALIG();
			newDDO.oi_item_ddo=itemIG.Id;
			newDDO.n_cantidad=cantidad;
			newDDO.n_valor=valor;

			Recibo.VALIG.Add(newDDO);
		}


		public double AcumDeducIG(string codigo, DateTime fechaDesde, DateTime fechaHasta)
		{
			double acum=0.0;
			foreach (LegajoEmpresaDDO.DEDUC_IG_DDO deduc in this.legajo.DeduccionesIG)
			{
				DateTime deducFecha=deduc.Fecha;
				if (deduc.ItemIG!=codigo) continue;

				if (deducFecha<fechaDesde) continue;
				if (deducFecha>fechaHasta) continue;
				acum+=deduc.Importe;
			}
			return acum;
		}

		public double AcumDeducIG(string codigo, int anno, int mes)
		{
			DateTime FD=new DateTime(anno, 1, 1);
			DateTime FH=new DateTime(anno, mes, 1);
			FH=FH.AddMonths(1);
			FH=FH.AddDays(-1);

			return AcumDeducIG(codigo, FD, FH);
		}

		public double DeduccionIG(string codigo, int anio, int mes)
		{
			double acum=0.0;
			int per=anio*100+mes;
			int ene=anio*100+1;
			foreach (LegajoEmpresaDDO.DEDUC_IG_DDO deduc in this.legajo.DeduccionesIG)
			{
				if (deduc.ItemIG!=codigo) continue;
				if (deduc.MesDesde>per) continue;
				if (deduc.MesHasta<ene) continue;

				int pd=deduc.MesDesde<ene?ene:deduc.MesDesde;
				int ph=deduc.MesHasta>per?per:deduc.MesHasta;
				acum+=deduc.Importe*(ph-pd+1);
			}
			return acum;
		}

		public int MesesDeduccionIG(string codigo, int anio, int mes)
		{
			int acum=0;
			int per=anio*100+mes;
			int ene=anio*100+1;
			foreach (LegajoEmpresaDDO.DEDUC_IG_DDO deduc in this.legajo.DeduccionesIG)
			{
				if (deduc.ItemIG!=codigo) continue;
				if (deduc.MesDesde>per) continue;
				if (deduc.MesHasta<ene) continue;

				int pd=deduc.MesDesde<ene?ene:deduc.MesDesde;
				int ph=deduc.MesHasta>per?per:deduc.MesHasta;
				acum+=(ph-pd+1);
			}
			return acum;
		}

		public OtrosEmpleadores OtrosEmp
		{
			get
			{
				return new OtrosEmpleadores(legajo);
			}
		}


		public int IGperiodo=0;
		public ArrayList IG=null;

		public CabeceraDeCodificadoraDDO.IMPUESTO_DDO GetIG(double monto, int anio, int mes)
		{
			if ((IG==null)||(IGperiodo!=anio*100+mes))
			{
				IG=new ArrayList();
				IGperiodo=anio*100+mes;

				foreach (CabeceraDeCodificadoraDDO.IMPUESTO_DDO objItem in this.codificadoras.IG)
					if ((objItem.Anio==anio)&&(objItem.Mes==mes))
						IG.Add(objItem);
			}

			foreach (CabeceraDeCodificadoraDDO.IMPUESTO_DDO objItemFilter in IG)
				if (objItemFilter.TotalImporteDesde<=monto&&objItemFilter.TotalImporteHasta>monto)
					return objItemFilter;

			this.GetTrace().Error("El monto de IG '"+monto+"' está fuera del rango permitido...");
			return null;
		}


		//////////////////////////////////////////////////////////
		// ESTRUCTURA ORGANIZATIVA
		//////////////////////////////////////////////////////////
		public bool IncluidoEnNodo(string inEstructura)
		{
			EmpresasDDO.UNIDAD_ORG_DDO unidad_org=(EmpresasDDO.UNIDAD_ORG_DDO)empresa.Organigrama.GetByAttribute("Codigo", inEstructura);
			if (unidad_org!=null)
			{
				EmpresasDDO.ORG_PER_DDO legajo_org=(EmpresasDDO.ORG_PER_DDO)unidad_org.Personas.GetByAttribute("Legajo", legajo.NroLegajo);
				if (legajo_org!=null)
					return true;
			}
			return false;
		}

		public bool IncluidoEnRama(string inEstructura)
		{
			foreach (EmpresasDDO.UNIDAD_ORG_DDO unidad_org in empresa.Organigrama)
			{
				if (unidad_org.Personas.GetByAttribute("Legajo", legajo.NroLegajo)!=null)
					if (buscarEstructura(unidad_org, inEstructura))
						return true;
			}
			return false;
		}

		private bool buscarEstructura(EmpresasDDO.UNIDAD_ORG_DDO unidad_org, string inEstructura)
		{
			if (unidad_org.Codigo==inEstructura)
				return true;
			else
			{
				if (unidad_org.CodPadre!="")
				{
					EmpresasDDO.UNIDAD_ORG_DDO estrPadre=(EmpresasDDO.UNIDAD_ORG_DDO)(empresa.Organigrama.GetByAttribute("Codigo", unidad_org.CodPadre));
					return buscarEstructura(estrPadre, inEstructura);
				}
			}
			return false;
		}

		public string ObtenerNodo()
		{
			foreach (EmpresasDDO.UNIDAD_ORG_DDO unidad_org in empresa.Organigrama)
			{
				if (unidad_org.Personas.GetByAttribute("Legajo", legajo.NroLegajo)!=null)
					return unidad_org.Codigo;
			}
			return "";
		}


		//////////////////////////////////////////////////////////
		// UBICACION
		//////////////////////////////////////////////////////////
		public EmpresasDDO.UBICACION_DDO ObtenerUbicacion(string codUbicacion)
		{
			foreach (EmpresasDDO.UBICACION_DDO ubicacion in empresa.Ubicaciones)
				if (ubicacion.Codigo==codUbicacion)
					return ubicacion;
			return null;
		}

		public EmpresasDDO.UBICACION_DDO ObtenerUbicacion()
		{
			return ObtenerUbicacion(legajo.Ubicacion);
		}
		//////////////////////////////////////////////////////////
		// PLAN DE OBRA SOCIAL
		//////////////////////////////////////////////////////////
		public CabeceraDeCodificadoraDDO.PLAN_OS_DDO ObtenerPlanOS(string codOS, string codPlan)
		{
			foreach (CabeceraDeCodificadoraDDO.PLAN_OS_DDO plan in this.codificadoras.PLANES_OS)
				if (plan.ObraSocial==codOS&&plan.Codigo==codPlan)
					return plan;
			return null;
		}

		public CabeceraDeCodificadoraDDO.PLAN_OS_DDO ObtenerPlanOS()
		{
			return ObtenerPlanOS(legajo.ObraSocial, legajo.PlanOS);
		}

        public bool AfiliadoSindicato
        {
            get{
                return legajo.AfiliadoSindicato;
                /*NomadXML result = NomadEnvironment.QueryNomadXML(NucleusRH.Base.Personal.LegajoEmpresa.PERSONAL_EMP.Resources.QRY_OBTENER_PERSONAL, "<PARAM c_empresa='" + legajo.Empresa + "' e_numero_legajo='" + legajo.NroLegajo + "' />").FirstChild();
                return result.GetAttr("l_afiliado_sind") == "1";*/
            }
        }

        /*public double PorcentajeDeduccionIG(string codigo, int anio, int mes)
        {
            double acum = 0.0;
            int per = anio * 100 + mes;
            int ene = anio * 100 + 1;
            foreach (LegajoEmpresaDDO.DEDUC_IG_DDO deduc in this.legajo.DeduccionesIG)
            {
                if (deduc.ItemIG != codigo) continue;
                if (deduc.MesDesde > per) continue;
                if (deduc.MesHasta < ene) continue;

                int pd = deduc.MesDesde < ene ? ene : deduc.MesDesde;
                int ph = deduc.MesHasta > per ? per : deduc.MesHasta;
                acum += deduc.e_porcentaje_deduccion * (ph - pd + 1);
            }
            return acum;
        }
*/


		#endregion
	}


	public class OtrosEmpleadores
	{
		private LegajoEmpresaDDO.PER_EMP_DDO legajo;
		public OtrosEmpleadores(LegajoEmpresaDDO.PER_EMP_DDO l)
		{
			legajo=l;
		}

        public bool Agente(int anio)
        {
            bool esAgente = false;
            foreach (LegajoEmpresaDDO.OTROS_EMP_DDO otroemp in this.legajo.OtrosEmp)
            {
                if (otroemp.PeriodoAgente != anio) continue;
                esAgente = esAgente || otroemp.EsAgente;
            }
            return esAgente;
        }

		public double Haberes(int anio, int mes)
		{
			double acum=0.0;
			int per=anio*100+mes;
			int ene=anio*100+1;
			foreach (LegajoEmpresaDDO.OTROS_EMP_DDO otroemp in this.legajo.OtrosEmp)
			{
				if (otroemp.Periodo>per) continue;
				if (otroemp.Periodo<ene) continue;

				acum+=otroemp.GanBruta+otroemp.Ajuste+otroemp.RetNoHab;
			}
			return acum;
		}

		//Obra Social
		public double ObraSocial(int anio, int mes)
		{
			double acum=0.0;
			int per=anio*100+mes;
			int ene=anio*100+1;
			foreach (LegajoEmpresaDDO.OTROS_EMP_DDO otroemp in this.legajo.OtrosEmp)
			{
				if (otroemp.Periodo>per) continue;
				if (otroemp.Periodo<ene) continue;

				acum+=otroemp.ObraSocial;
			}
			return acum;
		}
		//Seguridad Social
		public double SegSocial(int anio, int mes)
		{
			double acum=0.0;
			int per=anio*100+mes;
			int ene=anio*100+1;
			foreach (LegajoEmpresaDDO.OTROS_EMP_DDO otroemp in this.legajo.OtrosEmp)
			{
				if (otroemp.Periodo>per) continue;
				if (otroemp.Periodo<ene) continue;

				acum+=otroemp.SegSocial;
			}
			return acum;
		}
		//Sindicato
		public double Sindicato(int anio, int mes)
		{
			double acum=0.0;
			int per=anio*100+mes;
			int ene=anio*100+1;
			foreach (LegajoEmpresaDDO.OTROS_EMP_DDO otroemp in this.legajo.OtrosEmp)
			{
				if (otroemp.Periodo>per) continue;
				if (otroemp.Periodo<ene) continue;

				acum+=otroemp.Sindicato;
			}
			return acum;
		}
		//Retención de ganancias practicada
		public double RetencionesIG(int anio, int mes)
		{
			double acum=0.0;
			int per=anio*100+mes;
			int ene=anio*100+1;
			foreach (LegajoEmpresaDDO.OTROS_EMP_DDO otroemp in this.legajo.OtrosEmp)
			{
				if (otroemp.Periodo>per) continue;
				if (otroemp.Periodo<ene) continue;

				acum+=otroemp.RetGanPract;
			}
			return acum;
		}

        //Conceptos exentos
        public double ExeNoAlc(int anio, int mes)
        {
            double acum = 0.0;
            int per = anio * 100 + mes;
            int ene = anio * 100 + 1;
            foreach (LegajoEmpresaDDO.OTROS_EMP_DDO otroemp in this.legajo.OtrosEmp)
            {
                if (otroemp.Periodo > per) continue;
                if (otroemp.Periodo < ene) continue;

                acum += otroemp.ExeNoAlc;
            }
            return acum;
        }


        //Sueldo Anual Complementario
        public double Sac(int anio, int mes)
        {
            double acum = 0.0;
            int per = anio * 100 + mes;
            int ene = anio * 100 + 1;
            foreach (LegajoEmpresaDDO.OTROS_EMP_DDO otroemp in this.legajo.OtrosEmp)
            {
                if (otroemp.Periodo > per) continue;
                if (otroemp.Periodo < ene) continue;

                acum += otroemp.Sac;
            }
            return acum;
        }

        //Horas Extras Gravadas
        public double HorasExtGr(int anio, int mes)
        {
            double acum = 0.0;
            int per = anio * 100 + mes;
            int ene = anio * 100 + 1;
            foreach (LegajoEmpresaDDO.OTROS_EMP_DDO otroemp in this.legajo.OtrosEmp)
            {
                if (otroemp.Periodo > per) continue;
                if (otroemp.Periodo < ene) continue;

                acum += otroemp.HorasExtGr;
            }
            return acum;
        }

        //Horas Extras Exentas
        public double HorasExtEx(int anio, int mes)
        {
            double acum = 0.0;
            int per = anio * 100 + mes;
            int ene = anio * 100 + 1;
            foreach (LegajoEmpresaDDO.OTROS_EMP_DDO otroemp in this.legajo.OtrosEmp)
            {
                if (otroemp.Periodo > per) continue;
                if (otroemp.Periodo < ene) continue;

                acum += otroemp.HorasExtEx;
            }
            return acum;
        }

        //Material Didactico
        public double MatDid(int anio, int mes)
        {
            double acum = 0.0;
            int per = anio * 100 + mes;
            int ene = anio * 100 + 1;
            foreach (LegajoEmpresaDDO.OTROS_EMP_DDO otroemp in this.legajo.OtrosEmp)
            {
                if (otroemp.Periodo > per) continue;
                if (otroemp.Periodo < ene) continue;

                acum += otroemp.MatDid;
            }
            return acum;
        }
		
		public double Movilidad(int anio, int mes) {
			return OtrosEmpValue("Movilidad", anio, mes);
		}

		public double Viaticos(int anio, int mes) {
			return OtrosEmpValue("Viaticos", anio, mes);
		}

		public double OtrosConAn(int anio, int mes) {
			return OtrosEmpValue("OtrosConAn", anio, mes);
		}

		public double BonosProd(int anio, int mes) {
			return OtrosEmpValue("BonosProd", anio, mes);
		}
		
		public double ConSimNat(int anio, int mes) {
			return OtrosEmpValue("ConSimNat", anio, mes);
		}
		
		public double FallosCaja(int anio, int mes) {
			return OtrosEmpValue("FallosCaja", anio, mes);
		}

		public double RemunExLey(int anio, int mes) {
			return OtrosEmpValue("RemunExLey27549", anio, mes);
		}

		public double SupPartLey(int anio, int mes) {
			return OtrosEmpValue("SupPartLey19101", anio, mes);
		}

		public double TeletrabExento(int anio, int mes) {
			return OtrosEmpValue("TeletrabExento", anio, mes);
		}
		
		public double RetNoHab(int anio, int mes) {
			return OtrosEmpValue("RetNoHab", anio, mes);
		}
		
		public double Ajuste(int anio, int mes) {
			return OtrosEmpValue("Ajuste", anio, mes);
		}
		
		public double SegSocANSES(int anio, int mes) {
			return OtrosEmpValue("SegSocANSES", anio, mes);
		}
		
		public double SegSocCajas(int anio, int mes) {
			return OtrosEmpValue("SegSocCajas", anio, mes);
		}

		public double AjusteRemGravadas(int anio, int mes) {
			return OtrosEmpValue("AjusteRemGravadas", anio, mes);
		}

		public double AjusteRemExeNoAlcanzadas(int anio, int mes) {
			return OtrosEmpValue("AjusteRemExeNoAlcanzadas", anio, mes);
		}

		public double AsignFam(int anio, int mes) {
			return OtrosEmpValue("AsignFam", anio, mes);
		}

		public double IntPrestEmp(int anio, int mes) {
			return OtrosEmpValue("IntPrestEmp", anio, mes);
		}

		public double IndemLey4003(int anio, int mes) {
			return OtrosEmpValue("IndemLey4003", anio, mes);
		}

		public double RemunLey19640(int anio, int mes) {
			return OtrosEmpValue("RemunLey19640", anio, mes);
		}

		public double RemunCctPetro(int anio, int mes) {
			return OtrosEmpValue("RemunCctPetro", anio, mes);
		}

		public double CursosSemin(int anio, int mes) {
			return OtrosEmpValue("CursosSemin", anio, mes);
		}

		public double IndumEquipEmp(int anio, int mes) {
			return OtrosEmpValue("IndumEquipEmp", anio, mes);
		}
		
		public double RemunJudiciales(int anio, int mes) {
			return OtrosEmpValue("RemunJudiciales", anio, mes);
		}

		//Material Didactico
		private double OtrosEmpValue(string pstrValue, int anio, int mes) {
			double acum = 0.0;
			int per = anio * 100 + mes;
			int ene = anio * 100 + 1;
			
			foreach (LegajoEmpresaDDO.OTROS_EMP_DDO otroemp in this.legajo.OtrosEmp) {
				if (otroemp.Periodo > per) continue;
				if (otroemp.Periodo < ene) continue;
				
				switch (pstrValue.ToUpper()) {
					case "MATDID":			acum += otroemp.MatDid; break;
					case "HORASEXTEX":		acum += otroemp.HorasExtEx; break;
					case "HORASEXTGR":		acum += otroemp.HorasExtGr; break;
					case "SAC":				acum += otroemp.Sac; break;
					case "EXENOALC":		acum += otroemp.ExeNoAlc; break;
					case "RETGANPRACT":		acum += otroemp.RetGanPract; break;
					case "SINDICATO":		acum += otroemp.Sindicato; break;
					case "SEGSOCIAL":		acum += otroemp.SegSocial; break;
					case "OBRASOCIAL":		acum += otroemp.ObraSocial; break;
					case "HABERES":			acum += otroemp.GanBruta + otroemp.Ajuste + otroemp.RetNoHab; break;
					case "RETNOHAB":		acum += otroemp.RetNoHab; break;					
					case "AJUSTE":			acum += otroemp.Ajuste; break;					


					//Campos de la v19
					case "MOVILIDAD":			acum += otroemp.Movilidad; break;
					case "VIATICOS":			acum += otroemp.Viaticos; break;
					case "OTROSCONAN":			acum += otroemp.OtrosConAn; break;
					case "BONOSPROD":			acum += otroemp.BonosProd; break;
					case "FALLOSCAJA":			acum += otroemp.FallosCaja; break;
					case "CONSIMNAT":			acum += otroemp.ConSimNat; break;
					case "REMUNEXLEY27549":		acum += otroemp.RemunExLey27549; break;
					case "SUPPARTLEY19101":		acum += otroemp.SupPartLey19101; break;
					case "TELETRABEXENTO":		acum += otroemp.TeletrabExento; break;

					//campos Actualización Legal Interfaz SIRADIG V 1-22-1 - año 2025
					case "SEGSOCANSES":					acum += otroemp.SegSocANSES; break;
					case "SEGSOCCAJAS":					acum += otroemp.segSocCajas; break;
					case "AJUSTEREMGRAVADAS":			acum += otroemp.ajusteRemGravadas; break;
					case "AJUSTEREMEXENOALCANZADAS":	acum += otroemp.ajusteRemExeNoAlcanzadas; break;
					case "ASIGNFAM":					acum += otroemp.asignFam; break;
					case "INTPRESTEMP":					acum += otroemp.intPrestEmp; break;
					case "INDEMLEY4003":				acum += otroemp.indemLey4003; break;
					case "REMUNLEY19640":				acum += otroemp.remunLey19640; break;
					case "REMUNCCTPETRO":				acum += otroemp.remunCctPetro; break;
					case "CURSOSSEMIN":					acum += otroemp.cursosSemin; break;
					case "INDUMEQUIPEMP":				acum += otroemp.indumEquipEmp; break;
					
					//nuevo campo
					case "REMUNJUDICIALES":				acum += otroemp.remunJudiciales; break;					

				}
			}
			
			return acum;
		}
	}

}

namespace NucleusRH.Base.Liquidacion.Recibos 
{
	public partial class CONC_CARGO
	{
		public int IdxCargo;
	}
}
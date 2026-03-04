using System;
using System.IO;
using System.Xml;
using System.Collections;
using System.Text;

using System.Reflection;
using Microsoft.CSharp;
using System.CodeDom.Compiler;

using NucleusRH.Base.Liquidacion;
using Nomad.NSystem.Proxy;
using Nomad.NSystem.Base;
using System.Collections.Generic;
using NucleusRH.Base.Liquidacion.Liquidacion;
using NucleusRH.Base.Liquidacion.LiquidacionDDO;
using Nomad.NSystem.Functions;

namespace NucleusRH.Base.Liquidacion.RHLiq
{
  public class structRecibos
  {
    public int NroLegajo;
    public string MyID;
    public string oi_personal_emp;
    public LegajoEmpresaDDO.PER_EMP_DDO per_emp;
    public PersonalLiquidacionDDO.PER_LIQ_DDO per_liq;
    public Recibos.TOT_LIQ_PER rec;
  }

  public class ListManager
  {
    private ArrayList List;
    private bool FinAdd;
    private bool FinGet;
    private System.Threading.AutoResetEvent Change;
    private NomadException Exception;

    public ListManager()
    {
      this.List = new ArrayList();
      this.FinAdd=false;
      this.FinGet=false;
      this.Exception = null;
      this.Change=new System.Threading.AutoResetEvent(false);
    }

    public int Length
    {
      get
      {
        int retval;
        lock (this.List)
        {
          retval = this.List.Count;
        }
        return retval;
      }
    }

    public void Add(object newItem)
    {
      lock (this.List)
      {
        if (this.FinAdd) throw new Exception("DISABLED");
        if (this.FinGet) throw new Exception("DISABLED");
        this.List.Add(newItem);
        this.Change.Set(); //Cambio la Lista
      }
    }
    public void AddFinish()
    {
      lock (this.List)
      {
        this.FinAdd=true;
        this.Change.Set(); //Finalizo el Agregado
      }
    }
    public bool Finish { get { return this.FinAdd; } }
    public bool isFinishGet { get { return this.FinGet; } }
    public bool isFinishAdd { get { return this.FinAdd; } }

    public NomadException LastException { get { return this.Exception; } }

    public void WaitLength(int length)
    {
      bool wait=true;

      //bucle
      while (wait)
      {
        //tengo que esperar?
        lock (this.List)
        {
          if (this.List.Count>length && !this.FinAdd && !this.FinGet)
            wait=true;
          else
            wait=false;
        }

        //wait?
                if (wait)
                {
                    //GC.Collect();
                    this.Change.WaitOne(1000, false);
                }

      }
    }

    public object Get()
    {
      object retval = null;
      bool wait=true;

      while (wait)
      {
        lock (this.List)
        {
          //Finalizo?
          if (this.FinAdd && this.List.Count==0) return null;
          if (this.FinGet) throw new Exception("DISABLED");

          //Busco el Objeto
          if (this.List.Count>0)
          {
            retval=this.List[0];
            this.List.RemoveAt(0);
            this.Change.Set(); //Finalizo el Agregado
            wait=false;
          } else
          {
            //Espera hasta 1 segundo a que algo se agrege.
            wait=true;
          }
        }

        //wait?
                if (wait)
                {
                    //GC.Collect();
                    this.Change.WaitOne(1000, false);

                }

      }

      return retval;
    }

    public void GetFinish()
    {
      lock (this.List)
      {
        this.FinGet=true;
      }
    }
    public void GetFinish(NomadException Ex)
    {
      lock (this.List)
      {
        this.FinGet=true;
        this.Exception = Ex;
      }
    }
  }

    public class MultiThreadManager
    {
        private NomadProxy Proxy;
        private NomadTrace Trace;
        private Hashtable MyList;
        private Hashtable MyParams;

        public MultiThreadManager()
        {
            NomadLog.Info("CONSTRUCT RunPath " + NomadProxy.GetProxy().RunPath);

            this.Proxy = NomadProxy.GetProxy().Clone();
            this.Proxy.SetRunPath(NomadProxy.GetProxy().RunPath);
            this.Trace = NomadLog.GetTrace();
            this.MyList = new Hashtable();
            this.MyParams = new Hashtable();
        }

        public ListManager GetList(string ListName)
        {
            ListManager retval = null;
            lock (this.MyList)
            {
                if (MyList.ContainsKey(ListName.ToUpper()))
                    retval = (ListManager)MyList[ListName.ToUpper()];
                else
                {
                    retval = new ListManager();
                    MyList[ListName.ToUpper()] = retval;
                }
            }

            return retval;
        }

        ///////////////////////////////////////////////////////////////////////////////////////////
        // PARAMETROS
        public void SetParam(string Name, object Value)
        {
            lock (this.MyParams)
            {
                this.MyParams[Name.ToUpper()] = Value;
            }
        }
        public object GetParam(string Name)
        {
            object retval = "";

            lock (this.MyParams)
            {
                if (MyParams.ContainsKey(Name.ToUpper()))
                    retval = this.MyParams[Name.ToUpper()];
            }

            return retval;
        }
        public string GetParamString(string Name) { return (string)this.GetParam(Name); }
        public int GetParamInt(string Name) { return (int)this.GetParam(Name); }
        public bool GetParamBool(string Name) { return (bool)this.GetParam(Name); }

        ///////////////////////////////////////////////////////////////////////////////////////////
        // OBJETOS BASICOS
        public NomadProxy GetProxy()
        {
            NomadLog.Info("GETPROXY " + this.Proxy.RunPath);

            NomadProxy retval = this.Proxy.Clone();
            retval.SetRunPath(this.Proxy.RunPath);
            return retval;
        }

        public NomadTrace GetTrace()
        {
            return this.Trace;
        }

    //Especiales para el THREAD LIQ
    public LiqUtilBase LiqUtil = null;
        public Dictionary<string, PropertyInfo> propiedadesGrupos;
        public Dictionary<string, PropertyInfo> propiedadesJson;
    }

    public class AcumPer
    {
        public AcumPer(Hashtable arrIn, int periodoActual)
        {
            this.m_arr = arrIn;
            this.m_per = periodoActual;
        }

        override public string ToString()
        {
            string retval, sep;
            int per;

            retval = ""; sep = "";

            foreach (int MyKEY in this.m_arr.Keys)
            {
                per = MyKEY / 12 * 100 + MyKEY % 12 + 1;

                retval += sep + (per.ToString()) + "=" + this.m_arr[MyKEY].ToString();
                sep = ";";
            }

            return retval;
        }

        public static int SPeriodo(int pPeriodo)
        {
            int Ano = 0, Mes = 0;

            if (pPeriodo >= 100000 && pPeriodo <= 999999)
            { //Ano-Mes
                Ano = pPeriodo / 100;
                Mes = pPeriodo % 100;
            }
            else if (pPeriodo >= 10000 && pPeriodo <= 99999)
      { //Ano-Semestre
        Ano=pPeriodo/10;
        Mes=1+((pPeriodo%10)-1)*6;
      } else if (pPeriodo>=1000&&pPeriodo<=9999)
      { //Ano
        Ano=pPeriodo;
        Mes=1;
      }

            return (Ano * 12) + (Mes - 1);
        }

        public int Periodo(int pPeriodo)
        {
            int Ano = 0, Mes = 0;

            //Periodo Relativo
            if (pPeriodo <= 0)
            {
                Ano = this.m_per / 100;
                Mes = this.m_per % 100;

                pPeriodo += (Ano * 12 + Mes - 1);

                Ano = pPeriodo / 12;
                Mes = pPeriodo % 12 + 1;
            }
            else if (pPeriodo >= 100000 && pPeriodo <= 999999)
      { //Ano-Mes
        Ano=pPeriodo/100;
        Mes=pPeriodo%100;
      } else if (pPeriodo>=10000&&pPeriodo<=99999)
      { //Ano-Semestre
        Ano=pPeriodo/10;
        Mes=1+((pPeriodo%10)-1)*6;
      } else if (pPeriodo>=1000&&pPeriodo<=9999)
      { //Ano
        Ano=pPeriodo;
        Mes=1;
      } else if (pPeriodo>=1&&pPeriodo<=12)
      { //Mes
        Ano=this.m_per/100;
        Mes=pPeriodo;
      }

            return (Ano * 12) + (Mes - 1);
        }

        public int PeriodoDesde(int pPeriodo)
        {
            int Ano = 0, Mes = 0;

            //Periodo Relativo
            if (pPeriodo <= 0)
            {
                Ano = this.m_per / 100;
                Mes = this.m_per % 100;

                pPeriodo += (Ano * 12 + Mes - 1);

                Ano = pPeriodo / 12;
                Mes = pPeriodo % 12 + 1;
            }
            else if (pPeriodo >= 100000 && pPeriodo <= 999999)
      { //Ano-Mes
        Ano=pPeriodo/100;
        Mes=pPeriodo%100;
      } else if (pPeriodo>=10000&&pPeriodo<=99999)
      { //Ano-Semestre
        Ano=pPeriodo/10;
        Mes=1+((pPeriodo%10)-1)*6;
      } else if (pPeriodo>=1000&&pPeriodo<=9999)
      { //Ano
        Ano=pPeriodo;
        Mes=1;
      } else if (pPeriodo>=1&&pPeriodo<=12)
      { //Mes
        Ano=this.m_per/100;
        Mes=pPeriodo;
      }

            return (Ano * 100) + (Mes);
        }

        public int PeriodoHasta(int pPeriodo)
        {
            int Ano = 0, Mes = 0;

            //Periodo Relativo
            if (pPeriodo <= 0)
            {
                Ano = this.m_per / 100;
                Mes = this.m_per % 100;

                pPeriodo += (Ano * 12 + Mes - 1);

                Ano = pPeriodo / 12;
                Mes = pPeriodo % 12 + 1;
            }
            else if (pPeriodo >= 100000 && pPeriodo <= 999999)
      { //Ano-Mes
        Ano=pPeriodo/100;
        Mes=pPeriodo%100;
      } else if (pPeriodo>=10000&&pPeriodo<=99999)
      { //Ano-Semestre
        Ano=pPeriodo/10;
        Mes=6+((pPeriodo%10)-1)*6;
      } else if (pPeriodo>=1000&&pPeriodo<=9999)
      { //Ano
        Ano=pPeriodo;
        Mes=12;
      } else if (pPeriodo>=1&&pPeriodo<=12)
      { //Mes
        Ano=this.m_per/100;
        Mes=pPeriodo;
      }

            return (Ano * 100) + (Mes);
        }

        //Operadores
        public static implicit operator double(AcumPer ACValue)
        {
            return ACValue.Valor;
        }

        public double Valor
        {
            get { return this.Sumar(0, 0); }
        }

        //Sumar
        public double Sumar(int Periodo)
        {
            return this.Sumar(this.PeriodoDesde(Periodo), this.PeriodoHasta(Periodo));
        }
        public double Sumar(int PeriodoDesde, int PeriodoHasta)
        {
            double retval = 0;

            PeriodoDesde = Periodo(PeriodoDesde);
            PeriodoHasta = Periodo(PeriodoHasta);

            for (int t = PeriodoDesde; t <= PeriodoHasta; t++)
                if (m_arr.ContainsKey(t))
                    retval += (double)m_arr[t];

            return retval;
        }

        //Maximo
        public double Maximo(int Periodo)
        {
            return this.Maximo(this.PeriodoDesde(Periodo), this.PeriodoHasta(Periodo));
        }
        public double Maximo(int PeriodoDesde, int PeriodoHasta)
        {
            bool first = true;
            double retval = 0;

            PeriodoDesde = Periodo(PeriodoDesde);
            PeriodoHasta = Periodo(PeriodoHasta);

            for (int t = PeriodoDesde; t <= PeriodoHasta; t++)
                if (m_arr.ContainsKey(t))
                    if (first || retval < (double)m_arr[t])
                    {
                        retval = (double)m_arr[t];
                        first = false;
                    }

            return retval;
        }

        //Minimo
        public double Minimo(int Periodo)
        {
            return this.Minimo(this.PeriodoDesde(Periodo), this.PeriodoHasta(Periodo));
        }
        public double Minimo(int PeriodoDesde, int PeriodoHasta)
        {
            bool first = true;
            double retval = 0;

            PeriodoDesde = Periodo(PeriodoDesde);
            PeriodoHasta = Periodo(PeriodoHasta);

            for (int t = PeriodoDesde; t <= PeriodoHasta; t++)
                if (m_arr.ContainsKey(t))
                    if (first || retval > (double)m_arr[t])
                    {
                        retval = (double)m_arr[t];
                        first = false;
                    }

            return retval;
        }

        //Contar
        public int Cantidad(int Periodo)
        {
            return this.Cantidad(this.PeriodoDesde(Periodo), this.PeriodoHasta(Periodo));
        }
        public int Cantidad(int PeriodoDesde, int PeriodoHasta)
        {
            int retval = 0;

            PeriodoDesde = this.Periodo(PeriodoDesde);
            PeriodoHasta = this.Periodo(PeriodoHasta);

            for (int t = PeriodoDesde; t <= PeriodoHasta; t++)
                if (m_arr.ContainsKey(t))
                    retval++;

            return retval;
        }

        //Promedio
        public double Promedio(int Periodo)
        {
            int c = this.Cantidad(Periodo);
            double v = this.Sumar(Periodo);

            return c == 0 ? 0 : v / c;
        }
        public double Promedio(int PeriodoDesde, int PeriodoHasta)
        {
            int c = this.Cantidad(PeriodoDesde, PeriodoHasta);
            double v = this.Sumar(PeriodoDesde, PeriodoHasta);

            return c == 0 ? 0 : v / c;
        }

    public Hashtable Values()
    {
      return this.m_arr;
    }

    public int Periodo()
    {
      return this.m_per;
    }

    public void Set(int Periodo, double Valor)
    {
      this.m_arr[LiqUtilBase.ToMeses(Periodo)] = Valor;
    }

    public void Add(int Periodo, double Valor)
    {
      double val = 0;
      int per = LiqUtilBase.ToMeses(Periodo);
      if (this.m_arr.ContainsKey(per))
        val = (double)this.m_arr[per];
      Set(Periodo, val + Valor);
    }

        protected Hashtable m_arr;
        protected int m_per;
    }

    public class AcumPerSet : AcumPer
    {
        public AcumPerSet(Hashtable arrIn, int periodoActual)
            : base(arrIn, periodoActual)
        {
        }

        public new double Valor
        {
            get
            {
                return this.Sumar(0, 0);
            }

            set
            {
                this.m_arr[this.Periodo(this.m_per)] = value;
            }
        }

    }

    public class RHLiqControl
    {
        //public Hashtable parameters = new Hashtable();
        //public static LiqLogger logger = null;
        private NomadProxy proxy = null;
        private NomadTransaction transaction = null;
        private NomadTrace trace = null;
        private NomadTrace traceInt = null;
        private Liquidacion.LIQUIDACION MyLiq;
        private VariablesPorEjecucionDDO.VAR_EJEC_DDO variables;
        private CabeceraDeCodificadoraDDO.CODIF_DDO codificadoras;
        private CabeceraDeConceptosDDO.CAB_CONC_DDO conceptos;
        private EmpresasDDO.EMPRESA_DDO empresa;
        private LiquidacionDDO.LIQUIDACION_DDO liquidacion;
        private LiquidacionDDO.LIQUIDACION_DDO old_liquidacion;

        public int Secuencia = 0;
        private Hashtable legList;
        private Dictionary<string, PropertyInfo> propiedadesGrupos;
        private Dictionary<string, PropertyInfo> propiedadesJson;
        private Dictionary<string, PropertyInfo> propiedadesDDO;
        private NomadBatch MyBatch;
        private string paso;
        private int legCount;
        private int CantidadLegajos;
        public NomadTrace TraceInternal
        {
            get { return traceInt; }
        }

        public NomadTrace Trace
        {
            get { return trace; }
        }

        public RHLiqControl()
        {
            this.proxy = NomadEnvironment.GetProxy();
            this.transaction = NomadEnvironment.GetCurrentTransaction();
            this.trace = NomadEnvironment.GetTraceBatch();
            this.traceInt = NomadEnvironment.GetTrace();
            this.trace.Source = "Liquidacion";
            this.traceInt.Source = "Liquidacion";
        }

        static string MD5(string strin)
        {
            byte[] outval;
            //string retval = "";
            StringBuilder builder = new StringBuilder();

            System.Security.Cryptography.MD5 md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
            outval = md5.ComputeHash(Encoding.Unicode.GetBytes(strin));

            builder.Append(outval[0].ToString("X").PadLeft(2, '0'));
            builder.Append(outval[1].ToString("X").PadLeft(2, '0'));
            builder.Append(outval[2].ToString("X").PadLeft(2, '0'));
            builder.Append(outval[3].ToString("X").PadLeft(2, '0'));
            builder.Append("-");
            builder.Append(outval[4].ToString("X").PadLeft(2, '0'));
            builder.Append(outval[5].ToString("X").PadLeft(2, '0'));
            builder.Append(outval[6].ToString("X").PadLeft(2, '0'));
            builder.Append(outval[7].ToString("X").PadLeft(2, '0'));
            builder.Append("-");
            builder.Append(outval[8].ToString("X").PadLeft(2, '0'));
            builder.Append(outval[9].ToString("X").PadLeft(2, '0'));
            builder.Append(outval[10].ToString("X").PadLeft(2, '0'));
            builder.Append(outval[11].ToString("X").PadLeft(2, '0'));
            builder.Append("-");
            builder.Append(outval[12].ToString("X").PadLeft(2, '0'));
            builder.Append(outval[13].ToString("X").PadLeft(2, '0'));
            builder.Append(outval[14].ToString("X").PadLeft(2, '0'));
            builder.Append(outval[15].ToString("X").PadLeft(2, '0'));

           /* retval += outval[0].ToString("X").PadLeft(2, '0');
            retval += outval[1].ToString("X").PadLeft(2, '0');
            retval += outval[2].ToString("X").PadLeft(2, '0');
            retval += outval[3].ToString("X").PadLeft(2, '0');
            retval += "-";
            retval += outval[4].ToString("X").PadLeft(2, '0');
            retval += outval[5].ToString("X").PadLeft(2, '0');
            retval += outval[6].ToString("X").PadLeft(2, '0');
            retval += outval[7].ToString("X").PadLeft(2, '0');
            retval += "-";
            retval += outval[8].ToString("X").PadLeft(2, '0');
            retval += outval[9].ToString("X").PadLeft(2, '0');
            retval += outval[10].ToString("X").PadLeft(2, '0');
            retval += outval[11].ToString("X").PadLeft(2, '0');
            retval += "-";
            retval += outval[12].ToString("X").PadLeft(2, '0');
            retval += outval[13].ToString("X").PadLeft(2, '0');
            retval += outval[14].ToString("X").PadLeft(2, '0');
            retval += outval[15].ToString("X").PadLeft(2, '0');

            return retval;*/
            return builder.ToString();
        }

        static string ExecuteQuery(string tableName, string columnName, string columnWhere, string columnValue)
        {
            NomadXML xmlSQL = new NomadXML();

            xmlSQL.SetText(
            NomadEnvironment.QueryString(
            @"<qry:main doc=""SOURCE""><qry:element name=""ROW""><qry:insert-select name=""sql1"" doc-path=""#SOURCE:/PARAM""/></qry:element></qry:main>
        <qry:select xmlns:qry=""nomad.xquery"" name=""sql1"" doc=""SOURCE"">
          <qry:xquery>for $r in sql('select T.{%=#SOURCE:@COLUMN-NAME %} as VALUE FROM {%=#SOURCE:@TABLE-NAME %} as T WHERE T.{%=#SOURCE:@COLUMN-WHERE %}={#SOURCE:@WHERE-VALUE}')/ROWS/ROW</qry:xquery>
          <qry:out><qry:attribute name=""VALUE"" value=""$r/@VALUE""/></qry:out>
        </qry:select>",
              "<PARAM TABLE-NAME=\"" + tableName + "\" COLUMN-NAME=\"" + columnName + "\" COLUMN-WHERE=\"" + columnWhere + "\" WHERE-VALUE=\"" + columnValue + "\" />")
            );
            return xmlSQL.FirstChild().GetAttr("VALUE");
        }

        static Hashtable ExecuteQueryLegajos(string oi_liquidacion)
        {
            NomadXML xmlSQL = new NomadXML();
            Hashtable retval = new Hashtable();

            xmlSQL.SetText(
            NomadEnvironment.QueryString(
              @"<qry:main doc=""SOURCE""><qry:element name=""ROWS""><qry:insert-select name=""PERSONAL"" doc-path=""#SOURCE:/DATA""/></qry:element></qry:main>
        <qry:select doc=""SOURCE"" name=""PERSONAL"" xmlns:qry=""nomad.xquery"">
          <qry:xquery>for $r in SQL('SELECT P.e_numero_legajo FROM LIQ25_PERSONAL_LIQ as L, PER02_PERSONAL_EMP as P JOIN L.oi_personal_emp inner P.oi_personal_emp WHERE L.oi_liquidacion={#SOURCE:@oi_liquidacion}')/ROWS/ROW</qry:xquery>
          <qry:out><qry:insert-element doc-path=""$r""/></qry:out>
        </qry:select>",
              "<DATA oi_liquidacion=\"" + oi_liquidacion + "\"/>")
            );

            for (NomadXML cur = xmlSQL.FirstChild().FirstChild(); cur != null; cur = cur.Next())
                retval.Add(cur.GetAttr("e_numero_legajo"), 1);

            return retval;
        }

        public void ActualizarConceptosInternos()
        {
            CabeceraDeConceptosDDO.CONC_VARIAB_DDO myVARDDO;
            NomadXML myXML = new NomadXML();
            NomadXML myCON, myVAR;

            foreach (NucleusRH.Base.Liquidacion.CabeceraDeConceptosDDO.CONCEPTO_DDO concepto in this.conceptos.CONCEPTOS)
                if (concepto.Interno)
                {
                    //Cargo el CONCEPTO desde el archivo.
                    myXML.ClearXML();
                    myXML.SetText(this.proxy.FileService().LoadFile("Conceptos", concepto.Codigo.ToString() + ".concepto.xml"));

                    myCON = myXML.FirstChild();
                    concepto.Descripcion = myCON.GetAttr("Descripcion");
                    concepto.TipoConcepto = myCON.GetAttr("TipoConcepto");
                    concepto.FiguraEnRecibo = myCON.GetAttr("FiguraEnRecibo");
                    concepto.AcumulaGanancia = myCON.GetAttr("AcumulaGanancia");
                    concepto.TipoAsiento = myCON.GetAttr("TipoAsiento");
                    concepto.Formula = myCON.GetAttr("Formula");

                    concepto.VariablesConcepto.Clear();
                    for (myVAR = myCON.FirstChild().FirstChild(); myVAR != null; myVAR = myVAR.Next())
                    {
                        myVARDDO = new CabeceraDeConceptosDDO.CONC_VARIAB_DDO();
                        myVARDDO.CodigoVariable = myVAR.GetAttr("CodigoVariable");
                        myVARDDO.TipoParametro = myVAR.GetAttr("TipoParametro");
                    }

                }
            NomadLog.Info("en actualizar conceptos: " + this.codificadoras.ToString());
        }

        public void GenerateInitConceptos(StringBuilder varDECLARE, Hashtable varLIST)
        {
            StringBuilder args = new StringBuilder();
            StringBuilder locs = new StringBuilder();
            String tmpl, TypeVar;
            NucleusRH.Base.Liquidacion.VariablesPorEjecucionDDO.VARIABLE_DDO variable;
      variable = null;
      try
      {
        foreach (NucleusRH.Base.Liquidacion.CabeceraDeConceptosDDO.CONCEPTO_DDO concepto in this.conceptos.CONCEPTOS)
        {
          args.Length = 0;
          locs.Length = 0;
          //NomadLog.Info(concepto.VariablesConcepto.ToString());
          //Recorro Variables
          foreach (NucleusRH.Base.Liquidacion.CabeceraDeConceptosDDO.CONC_VARIAB_DDO varConcepto in concepto.VariablesConcepto)
          {
            variable = null;
            TypeVar = varConcepto.TipoDato.ToUpper();
            if (varConcepto.TipoParametro.ToUpper() != "LOC")
            {
              variable = (NucleusRH.Base.Liquidacion.VariablesPorEjecucionDDO.VARIABLE_DDO)varLIST[varConcepto.CodigoVariable];
              if (variable.TipoDeVariable == "9" || variable.TipoDeVariable == "10" || variable.TipoDeVariable == "13") TypeVar = "ARRAY";
            }

            switch (varConcepto.TipoParametro.ToUpper())
            {
              case "OUT":
								if (TypeVar == "ARRAY") args.Append(", ");
                else args.Append(", ref ");
                break;

              case "LOC":
                args.Append(", ref ");
                locs.Append("\r\n\t" + varConcepto.Alias + "=" + varConcepto.t_formula + ";");
                break;

              default:
                args.Append(", ");
                break;
            }

            switch (TypeVar)
            {
              case "BOOL": args.Append("bool "); break;
              case "INT": args.Append("int "); break;
              case "DOUBLE": args.Append("double "); break;
              case "STRING": args.Append("string "); break;
              case "DATETIME": args.Append("DateTime "); break;
              case "ARRAY":
                if (varConcepto.TipoParametro.ToUpper() == "OUT") args.Append("AcumPerSet ");
                else args.Append("AcumPer ");
                break;
              default:
                //AQUI ERROR
                break;
            }
            args.Append(varConcepto.Alias);
          }

          switch (concepto.Version.ToUpper())
          {
            case "":
            case "1":
              tmpl = NucleusRH.Base.Liquidacion.LiquidacionDDO.LIQUIDACION_DDO.Resources.concepto_tmpl_1;
              break;

            case "2":
              tmpl = NucleusRH.Base.Liquidacion.LiquidacionDDO.LIQUIDACION_DDO.Resources.concepto_tmpl_2;
              break;

            default:
              tmpl = "";
              break;
          }

          //Agrego el Codigo
          varDECLARE.AppendFormat(
            tmpl
            , concepto.Codigo
            , concepto.Descripcion
            , concepto.Codigo
            , args.ToString()
            , concepto.Formula.Replace("\n", "\n\t\t\t")
            , locs.ToString().Replace("\n", "\n\t\t\t")
            );
        }
        NomadLog.Info("en GenerateInitConceptos: " + this.codificadoras.ToString());
      } catch (Exception e) {
        NomadLog.Critical("ERROR EN GenerateInitConceptos");
        if (variable != null)
          NomadLog.Critical("variable '" + variable.Codigo + "'");
        NomadLog.Critical(e.Message);
      }
    }

        public void GenerateCallConceptos(StringBuilder varDECLARE, int iEtapa, Hashtable varLIST)
        {
            foreach (NucleusRH.Base.Liquidacion.CabeceraDeConceptosDDO.CONCEPTO_DDO concepto in this.conceptos.CONCEPTOS)
      if (concepto.Etapa==iEtapa)
            {
                //Agrego la Llamada
                varDECLARE.AppendFormat("\r\n\t\t\texec_concepto_{0}(idx);"
                  , concepto.Codigo
                );
            }

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

        public void XMLAddObject(NomadXML myNODO, NomadObject MyOBJ)
        {
            NomadXML MyXML = new NomadXML(MyOBJ.Security.ToString());
            MyXML = MyXML.FirstChild();
            MyXML.AddText(MyOBJ.SerializeAll());
        }

        public static void SaveCacheObject(string FileName, NomadObject MyOBJ)
        {
            MyOBJ.ToFile(FileName);
        }

        public static NomadObject LoadCacheObject(Type objType, string FileName)
        {
            //Cargo el Archivo
            System.IO.StreamReader MyFILEIN = new System.IO.StreamReader(FileName);
            string strDDO = MyFILEIN.ReadToEnd();
            MyFILEIN.Close();

            //Genero el OBJECT
            return Nomad.NSystem.Base.NomadEnvironment.GetObject(objType, strDDO);
        }

        public Hashtable ReadLegList(string oi_liquidacion)
        {
            Hashtable retval = new Hashtable();
            Hashtable auxHash = new Hashtable();
      ArrayList arrRecibosSinLegajo = new ArrayList();

            NomadXML LegList=new NomadXML(NomadEnvironment.QueryString("CLASS.NucleusRH.Base.Liquidacion.LiquidacionDDO.LIQUIDACION_DDO.QRY_VERSION", "<DATA oi_liquidacion=\""+oi_liquidacion+"\" />"));
            if (LegList.isDocument) LegList = LegList.FirstChild();

            //Recorro personas
            for (NomadXML cur = LegList.FindElement("PERSONAS").FirstChild(); cur != null; cur = cur.Next())
            {
                retval[cur.GetAttr("oi_personal_emp")] = cur;
                auxHash[cur.GetAttr("e_numero_legajo")] = cur;
            }

            //Recorro Recibos
            for (NomadXML cur = LegList.FindElement("RECIBOS").FirstChild(); cur != null; cur = cur.Next())
      {
        //Verifica que el recibo pertenezca a un legajo existente
        if (!auxHash.ContainsKey(cur.GetAttr("e_numero_legajo"))) {
          arrRecibosSinLegajo.Add(cur.GetAttr("e_numero_legajo"));
          continue;
        }
        ((NomadXML)auxHash[cur.GetAttr("e_numero_legajo")]).SetAttr("f_cierre", cur.GetAttr("f_cierre"));
      }

      //Existen recibos procesados que no tienen un legajo asociado. Esto puede pasar porque la persona cambió el número de legajo
      if (arrRecibosSinLegajo.Count > 0) {
        //Recorre los legajos con problemas y lo muestra en un mensaje de error
        string strLegajos = "*";
        for (int i = 0; i < arrRecibosSinLegajo.Count; i++) {
          strLegajos = strLegajos + ", " + arrRecibosSinLegajo[i];
        }
        strLegajos = "Números de legajo no encontrados (" + strLegajos.Replace("*, ", "") + "). Po favor verifique que esas personas no hayan cambiado de número de legajo recientemente.";

        throw new NomadAppException("Existen recibos de sueldo con un legajo asociado no encontrado dentro de la Liquidación. " + strLegajos);

      }

      return retval;
        }

        public System.Reflection.Assembly InitializeLiq()
        {
            int mySecu = 0, t;

            NomadBatch MyBatch = NomadBatch.GetBatch("InitializeLiq", "InitializeLiq");

            Hashtable varLIST = new Hashtable();
            StringBuilder conDECLARE = new StringBuilder();

            string strDDO;
            string strHash;
            string auxID = null;
            string Params;
            DateTime myNow;
            TimeSpan myDif;

            NomadXML ROWS = new NomadXML();
            NomadXML REC;

            int Decimales = 2;

            string MyDLLCode = null;

            //Creo las Carpetas para el CACHE.
      string RootPath=NomadProxy.GetProxy().RunPath+"\\CACHE\\"+NomadProxy.GetProxy().AppName+"\\LIQUIDACION";
            if (!System.IO.Directory.Exists(RootPath + "\\LIQ94_CODIF"))
                System.IO.Directory.CreateDirectory(RootPath + "\\LIQ94_CODIF");
            if (!System.IO.Directory.Exists(RootPath + "\\LIQ96_CAB_CONCEPT"))
                System.IO.Directory.CreateDirectory(RootPath + "\\LIQ96_CAB_CONCEPT");

            /////////////////////////////////////////////////////////////////////////////////////
            // Cargo la Lista de Legajos
            myNow = DateTime.Now;
            legList = ReadLegList(this.MyLiq.Id);
            myDif = DateTime.Now - myNow;
            NomadLog.Info("Lista de Legajos: " + myDif.TotalMilliseconds + "ms");

            //Obtengo la Cantidad de Decimales
            try
            {
                NomadLog.Info("Cantidad de Decimales.");
                Decimales = int.Parse(NomadEnvironment.QueryValue("ORG26_PARAMETROS", "d_valor", "c_parametro", "cant_dec_liq", "", false));
            }
            catch { }
            NomadLog.Info("Cantidad de Decimales: " + Decimales);

            /////////////////////////////////////////////////////////////////////////////////////
            // Cargo las Variables
            myNow = DateTime.Now;

            Params = "<FILTRO oi_empresa=\"" + this.MyLiq.oi_empresa + "\" oi_tipo_liq=\"" + this.MyLiq.oi_tipo_liq + "\" />";
            NomadLog.Info("Ejecutar Query: CLASS.NucleusRH.Base.Liquidacion.VariablesPorEjecucionDDO.VAR_EJEC_DDO.CargarVariablesALiquidacion - PARAM:"+Params);
            strDDO=NomadEnvironment.QueryString("CLASS.NucleusRH.Base.Liquidacion.VariablesPorEjecucionDDO.VAR_EJEC_DDO.CargarVariablesALiquidacion", Params);
            strHash = MD5(strDDO);
            auxID = ExecuteQuery("LIQ95_VAR_EJEC", "oi_var_ejec_ddo", "h_hash", strHash);
            if (auxID == "")
            {
                this.variables = (NucleusRH.Base.Liquidacion.VariablesPorEjecucionDDO.VAR_EJEC_DDO)Nomad.NSystem.Base.NomadEnvironment.GetObject(typeof(NucleusRH.Base.Liquidacion.VariablesPorEjecucionDDO.VAR_EJEC_DDO), strDDO);
                this.variables.Hash = strHash;
                NomadEnvironment.GetCurrentTransaction().SaveRefresh(this.variables);
                NomadLog.Info("Guardo la Variables");
            }
            else
                this.variables = (NucleusRH.Base.Liquidacion.VariablesPorEjecucionDDO.VAR_EJEC_DDO)Nomad.NSystem.Base.NomadEnvironment.GetObject(typeof(NucleusRH.Base.Liquidacion.VariablesPorEjecucionDDO.VAR_EJEC_DDO), strDDO.Replace("id=\"%1\"", "id=\"" + auxID + "\""));

            myDif = DateTime.Now - myNow;
            NomadLog.Info("Carga de Variables: " + myDif.TotalMilliseconds + "ms");

            /////////////////////////////////////////////////////////////////////////////////////
            // Generar Codigo
            myNow = DateTime.Now;

            foreach (NucleusRH.Base.Liquidacion.VariablesPorEjecucionDDO.VARIABLE_DDO variable in this.variables.VARIABLES)
                varLIST.Add(variable.Codigo, variable);
            myDif = DateTime.Now - myNow;
            NomadLog.Info("Generacion del Codigo para Variables: " + myDif.TotalMilliseconds + "ms");

            /////////////////////////////////////////////////////////////////////////////////////
            // Analizo la Existencia de las Variables del Sistema
            System.Reflection.PropertyInfo[] MyProps;
            MyProps = typeof(NucleusRH.Base.Liquidacion.Recibos.TOT_LIQ_PER).GetProperties();
            for (t = 0; t < MyProps.Length; t++)
                if ((MyProps[t].Name.Substring(0, 2) == "n_") && (!MyProps[t].Name.EndsWith("Null")))
                {
                    if (!varLIST.ContainsKey("vs_" + MyProps[t].Name.Substring(2)))
                        MyBatch.Err("Variable de Sistema 'vs_" + MyProps[t].Name.Substring(2) + "' no encontrada...");
                }

            /////////////////////////////////////////////////////////////////////////////////////
            // Cargo las Codificadores
            myNow = DateTime.Now;

            Params = "<FILTRO e_anio=\"" + this.MyLiq.f_pago.Year.ToString() + "\" />";
            NomadLog.Info("Ejecutar Query: NucleusRH.Base.Liquidacion.CabeceraDeCodificadoraDDO.CODIF_DDO.GuardarCodificadoras: " + Params);
            strDDO=NomadEnvironment.QueryString("CLASS.NucleusRH.Base.Liquidacion.CabeceraDeCodificadoraDDO.CODIF_DDO.GuardarCodificadoras", Params);
            strHash = MD5(strDDO);
            auxID = ExecuteQuery("LIQ94_CODIF", "oi_codif_ddo", "h_hash", strHash);
            if (auxID == "")
            {
                this.codificadoras = (NucleusRH.Base.Liquidacion.CabeceraDeCodificadoraDDO.CODIF_DDO)Nomad.NSystem.Base.NomadEnvironment.GetObject(typeof(NucleusRH.Base.Liquidacion.CabeceraDeCodificadoraDDO.CODIF_DDO), strDDO);
                this.codificadoras.Hash = strHash;
                NomadEnvironment.GetCurrentTransaction().SaveRefresh(this.codificadoras);

                NomadLog.Info("Guardo la Codificadores");
                SaveCacheObject(RootPath + "\\LIQ94_CODIF\\CACHE_" + this.codificadoras.Id + "_" + this.codificadoras.Hash + ".data.xml", this.codificadoras);

            }
            else
            {
                //NomadLog.Info("en el if antes del save en intialize: " + this.codificadoras.ToString());
                string strFile = RootPath + "\\LIQ94_CODIF\\CACHE_" + auxID + "_" + strHash + ".data.xml";
                NomadLog.Info("strFile: " + strFile);
                if (System.IO.File.Exists(strFile))
                  this.codificadoras = (NucleusRH.Base.Liquidacion.CabeceraDeCodificadoraDDO.CODIF_DDO)LoadCacheObject(typeof(NucleusRH.Base.Liquidacion.CabeceraDeCodificadoraDDO.CODIF_DDO), strFile);
                else
                {
                  this.codificadoras = NucleusRH.Base.Liquidacion.CabeceraDeCodificadoraDDO.CODIF_DDO.Get(auxID);
                  SaveCacheObject(RootPath+"\\LIQ94_CODIF\\CACHE_"+this.codificadoras.Id+"_"+this.codificadoras.Hash+".data.xml", this.codificadoras);
                }

            }

            myDif = DateTime.Now - myNow;
            NomadLog.Info("Carga de Codificadoras: " + myDif.TotalMilliseconds + "ms");

            /////////////////////////////////////////////////////////////////////////////////////
            // Cargo los Conceptos
            myNow = DateTime.Now;

            Params = "<FILTRO oi_empresa=\"" + this.MyLiq.oi_empresa + "\" oi_var_ejec_ddo=\"" + this.variables.Id + "\" oi_tipo_liq=\"" + this.MyLiq.oi_tipo_liq + "\" />";
            NomadLog.Info("Ejecutar Query: NucleusRH.Base.Liquidacion.CabeceraDeConceptosDDO.CAB_CONC_DDO.QryConceptos: " + Params);
            strDDO=NomadEnvironment.QueryString("CLASS.NucleusRH.Base.Liquidacion.CabeceraDeConceptosDDO.CAB_CONC_DDO.QryConceptos", Params);

            strHash = MD5(strDDO);
            auxID = ExecuteQuery("LIQ96_CAB_CONCEPT", "oi_cab_conc_ddo", "h_hash", strHash);
            if (auxID == "")
            {
                //Reconstruir el DDO
                NomadXML MyDDOCAB = new NomadXML(strDDO);
                if (MyDDOCAB.isDocument) MyDDOCAB = MyDDOCAB.FirstChild();

                NomadXML CAB_CONC_DDO = MyDDOCAB.FindElement("CAB_CONC_DDO");
                NomadXML CONCEPTOS = CAB_CONC_DDO.FindElement("CONCEPTOS");
                NomadXML MySQL3 = CAB_CONC_DDO.FindElement("sql3");
                NomadXML MySQL5 = CAB_CONC_DDO.FindElement("sql5");

                //Cargo el diccionario
                Dictionary<string, string> MyConceptosVars = new Dictionary<string, string>();
                Dictionary<string, NomadXML> MyConceptos = new Dictionary<string, NomadXML>();
                for(NomadXML MyCUR = CONCEPTOS.FirstChild(); MyCUR!=null; MyCUR=MyCUR.Next())
                  MyConceptos[MyCUR.GetAttr("oi_concepto")] = MyCUR.AddTailElement("VariablesConcepto");

                //Agrego las variables
                Dictionary<string, NomadXML> LocalVars = new Dictionary<string, NomadXML>();
                for(NomadXML MyCUR = MySQL5.FirstChild(); MyCUR!=null; MyCUR=MyCUR.Next())
                {
                  if (MyCUR.Name == "VARI")
                    LocalVars[MyCUR.GetAttr("codigo")] = MyCUR;
                  if (MyCUR.Name == "TIPO")
                  {
                    string c_tipo_concepto = MyCUR.GetAttr("c_tipo_concepto");
                    string vars = MyCUR.GetAttr("o_acumular").Trim();
                    if (vars == "") continue;

                    for(NomadXML MyCURC = CONCEPTOS.FirstChild(); MyCURC!=null; MyCURC=MyCURC.Next())
                    {
                      if (MyCURC.GetAttr("TipoConcepto") != c_tipo_concepto) continue;
                      NomadXML MyVAR = MyCURC.FirstChild();

                      string oi_concepto = MyCURC.GetAttr("oi_concepto");


                      List<string> ListNames = new List<string>();
                      foreach(string varName in vars.Split(','))
                        ListNames.Add(varName);
                      ListNames.Sort();

                      foreach(string varName in ListNames)
                      {
                        if (MyConceptosVars.ContainsKey(oi_concepto))
                        {
                          if (MyConceptosVars[oi_concepto].Contains(";" + varName + ";")) continue;
                          MyConceptosVars[oi_concepto] = MyConceptosVars[oi_concepto] + varName + ";";
                        } else
                          MyConceptosVars[oi_concepto] = ";" + varName + ";";

                        NomadXML newVAR = MyVAR.AddTailElement("CONC_VARIAB_DDO");
                        newVAR.SetAttr("TipoDato",LocalVars[varName.Trim()].GetAttr("tipo"));
                        newVAR.SetAttr("TipoParametro","ACV");
                        newVAR.SetAttr("Alias",varName.Trim());
                        newVAR.SetAttr("CodigoVariable",varName.Trim());
                        newVAR.SetAttr("nmd-status","~i,");
                        newVAR.SetAttr("id","%1");
                        newVAR.AddTailElement("OIDVariable").SetAttr("value", LocalVars[varName.Trim()].GetAttr("id"));
                      }
                    }
                  }
                }
                CAB_CONC_DDO.DeleteChild(MySQL5);

                //Agrego las variables
                for(NomadXML MyCUR = MySQL3.FirstChild(); MyCUR!=null; MyCUR=MyCUR.Next())
                {
                  string oi_concepto = MyCUR.GetAttr("oi_concepto");
                  string varName = MyCUR.GetAttr("CodigoVariable");

                  if (MyConceptosVars.ContainsKey(oi_concepto))
                  {
                    if (MyConceptosVars[oi_concepto].Contains(";" + varName + ";")) continue;
                    MyConceptosVars[oi_concepto] = MyConceptosVars[oi_concepto] + varName + ";";
                  } else
                    MyConceptosVars[oi_concepto] = ";" + varName + ";";

                  if (MyConceptos.ContainsKey(oi_concepto)) MyConceptos[oi_concepto].AddXML(MyCUR);
                }
                CAB_CONC_DDO.DeleteChild(MySQL3);

                this.conceptos = (NucleusRH.Base.Liquidacion.CabeceraDeConceptosDDO.CAB_CONC_DDO)Nomad.NSystem.Base.NomadEnvironment.GetObject(typeof(NucleusRH.Base.Liquidacion.CabeceraDeConceptosDDO.CAB_CONC_DDO), MyDDOCAB.ToString());
                this.conceptos.Hash = strHash;
                NomadEnvironment.GetCurrentTransaction().SaveRefresh(this.conceptos);
                NomadLog.Info("Guardo la Conceptos");
                SaveCacheObject(RootPath + "\\LIQ96_CAB_CONCEPT\\CACHE_" + this.conceptos.Id + "_" + this.conceptos.Hash + ".data.xml", this.conceptos);
            }
            else
            {
                string strFile = RootPath + "\\LIQ96_CAB_CONCEPT\\CACHE_" + auxID + "_" + strHash + ".data.xml";
                if (System.IO.File.Exists(strFile))
                    this.conceptos = (NucleusRH.Base.Liquidacion.CabeceraDeConceptosDDO.CAB_CONC_DDO)LoadCacheObject(typeof(NucleusRH.Base.Liquidacion.CabeceraDeConceptosDDO.CAB_CONC_DDO), strFile);
                else
                {
                    this.conceptos = NucleusRH.Base.Liquidacion.CabeceraDeConceptosDDO.CAB_CONC_DDO.Get(auxID);
                    SaveCacheObject(RootPath+"\\LIQ96_CAB_CONCEPT\\CACHE_"+this.conceptos.Id+"_"+this.conceptos.Hash+".data.xml", this.conceptos);
                }
            }

            myDif = DateTime.Now - myNow;
            NomadLog.Info("Carga de Conceptos: " + myDif.TotalMilliseconds + "ms");

            /////////////////////////////////////////////////////////////////////////////////////
            // Generar Codigo
            myNow = DateTime.Now;
            ActualizarConceptosInternos();
            GenerateInitConceptos(conDECLARE, varLIST);
            myDif = DateTime.Now - myNow;
            NomadLog.Info("Generacion del Codigo para Conceptos: " + myDif.TotalMilliseconds + "ms");

            /////////////////////////////////////////////////////////////////////////////////////
            // Cargo la Empresa
            myNow = DateTime.Now;

            Params = "<FILTRO oi_empresa=\"" + this.MyLiq.oi_empresa + "\" oi_var_ejec_ddo=\"" + this.variables.Id + "\" oi_codif_ddo=\"" + this.codificadoras.Id + "\" />";
            NomadLog.Info("Ejecutar Query: NucleusRH.Base.Liquidacion.EmpresasDDO.EMPRESA_DDO.QueryEmpresaDDO: " + Params);
            strDDO=NomadEnvironment.QueryString("CLASS.NucleusRH.Base.Liquidacion.EmpresasDDO.EMPRESA_DDO.QueryEmpresaDDO", Params);
            strHash = MD5(strDDO);
            auxID = ExecuteQuery("LIQ93_EMPRESAS", "oi_empresa_ddo", "h_hash", strHash);
            if (auxID == "")
            {
                this.empresa = (NucleusRH.Base.Liquidacion.EmpresasDDO.EMPRESA_DDO)Nomad.NSystem.Base.NomadEnvironment.GetObject(typeof(NucleusRH.Base.Liquidacion.EmpresasDDO.EMPRESA_DDO), strDDO);
                this.empresa.Hash = strHash;
                NomadEnvironment.GetCurrentTransaction().SaveRefresh(this.empresa);
                NomadLog.Info("Guardo la Empresa");
            }
            else
                this.empresa = (NucleusRH.Base.Liquidacion.EmpresasDDO.EMPRESA_DDO)Nomad.NSystem.Base.NomadEnvironment.GetObject(typeof(NucleusRH.Base.Liquidacion.EmpresasDDO.EMPRESA_DDO), strDDO.Replace("id=\"%1\"", "id=\"" + auxID + "\""));

            myDif = DateTime.Now - myNow;
            NomadLog.Info("Carga de Empresa: " + myDif.TotalMilliseconds + "ms");

            /////////////////////////////////////////////////////////////////////////////////////
            // Cargo la Liquidacion
            myNow = DateTime.Now;

            Params = "<FILTRO oi_empresa_ddo=\"" + this.empresa.Id + "\" oi_var_ejec_ddo=\"" + this.variables.Id + "\" oi_cab_conc_ddo=\"" + this.conceptos.Id + "\" oi_codif_ddo=\"" + this.codificadoras.Id + "\" oi_liquidacion=\"" + this.MyLiq.Id + "\"/>";
            NomadLog.Info("Ejecutar Query: NucleusRH.Base.Liquidacion.LiquidacionDDO.LIQUIDACION_DDO.QueryLiquidacion: " + Params);
            strDDO=NomadEnvironment.QueryString("CLASS.NucleusRH.Base.Liquidacion.LiquidacionDDO.LIQUIDACION_DDO.QueryLiquidacion", Params);
            strHash = MD5(strDDO);
            this.liquidacion = (NucleusRH.Base.Liquidacion.LiquidacionDDO.LIQUIDACION_DDO)Nomad.NSystem.Base.NomadEnvironment.GetObject(typeof(NucleusRH.Base.Liquidacion.LiquidacionDDO.LIQUIDACION_DDO), strDDO);
            this.liquidacion.Hash = strHash;
            this.liquidacion.Confidencial = MyLiq.l_confidencial;
            NomadLog.Info("Guardo la Liquidacion");
            NomadEnvironment.GetCurrentTransaction().SaveRefresh(this.liquidacion);

            myDif = DateTime.Now - myNow;
            NomadLog.Info("Carga de Liquidacion: " + myDif.TotalMilliseconds + "ms");

            /////////////////////////////////////////////////////////////////////////////////////
            // Genero el Codigo Fuente
            myNow = DateTime.Now;
            MyDLLCode = NucleusRH.Base.Liquidacion.LiquidacionDDO.LIQUIDACION_DDO.Resources.liqConceptosTmpl;

            MyDLLCode = MyDLLCode.Replace("{conDECLARE}", conDECLARE.ToString());

            MyDLLCode = MyDLLCode.Replace("{DEC}", Decimales.ToString());
            MyDLLCode = MyDLLCode.Replace("#DEC#", Decimales.ToString());

            myDif = DateTime.Now - myNow;
            NomadLog.Info("Genero el Codigo Fuente: " + myDif.TotalMilliseconds + "ms");

            /////////////////////////////////////////////////////////////////////////////////////
            // Genero una nueva Secuencia
            myNow = DateTime.Now;
            NucleusRH.Base.Liquidacion.Liquidacion.EJECUCION myEjecucion;

            foreach (NucleusRH.Base.Liquidacion.Liquidacion.EJECUCION aux in this.MyLiq.EJECUCIONES)
                if (mySecu < aux.e_secuencia)
                {
                    mySecu = aux.e_secuencia;
                    Secuencia = mySecu;
                }

            mySecu++;
            myEjecucion = new NucleusRH.Base.Liquidacion.Liquidacion.EJECUCION();
            myEjecucion.o_ejecuciones = "Ejecucion generada el dia " + DateTime.Now.ToString("dd/MM/yyyy") + " a la hora " + DateTime.Now.ToString("HH:mm") + ".";
            myEjecucion.oi_liquidacion_ddo = this.liquidacion.Id;
            myEjecucion.e_secuencia = mySecu; Secuencia = mySecu;
            this.MyLiq.EJECUCIONES.Add(myEjecucion);
            NomadEnvironment.GetCurrentTransaction().SaveRefresh(this.MyLiq);
            NomadLog.Info("Guardo la Liquidacion Productivo");
            NomadLog.Info("despues de guardar productivo: " + this.codificadoras.ToString());
            myDif = DateTime.Now - myNow;
            NomadLog.Info("Guardo la Secuencia: " + myDif.TotalMilliseconds + "ms");

            /////////////////////////////////////////////////////////////////////////////////////
            //Obtengo la ultima liquidacion
            myNow = DateTime.Now;
            this.old_liquidacion = null;

            REC=proxy.SQLService().GetXML("CLASS.NucleusRH.Base.Liquidacion.LiquidacionDDO.LIQUIDACION_DDO.QRY_LIQ_FINAL", "<PARAM oi_liquidacion=\""+this.MyLiq.Id+"\" />");

            if (REC != null) REC = REC.FirstChild();
            if (REC != null)
            {
                this.old_liquidacion = LiquidacionDDO.LIQUIDACION_DDO.Get(REC.GetAttr("oi_liquidacion_ddo"),false);

                Hashtable LegajosINIT = ExecuteQueryLegajos(this.MyLiq.Id);

                //Agrego los childs
                foreach (LiquidacionDDO.RECIBO rec in this.old_liquidacion.Recibos)
                    if (LegajosINIT.ContainsKey(rec.Legajo.ToString()))
                    {
                        LiquidacionDDO.RECIBO newrec = new LiquidacionDDO.RECIBO();
                        newrec.Legajo = rec.Legajo;
                        newrec.Recibo = rec.Recibo;
                        newrec.Hash = rec.Hash;
                        newrec.Secuencia = rec.Secuencia;
                        this.liquidacion.Recibos.Add(newrec);
                    }
            }

            myDif = DateTime.Now - myNow;
            NomadLog.Info("Generar Estado de Ejecucion: " + myDif.TotalMilliseconds + "ms");

            /////////////////////////////////////////////////////////////////////////////////////
            // Genero la DLL
            myNow = DateTime.Now;
            string dllMAIN = this.GetType().Assembly.GetFiles()[0].Name;
            string dllPATH;
            string dllTRG;
            dllPATH = dllMAIN.Substring(0, dllMAIN.LastIndexOf("\\"));
            dllMAIN = dllMAIN.Substring(dllMAIN.LastIndexOf("\\") + 1);

            dllTRG = "LiqConcepto_" + this.MyLiq.Id + "_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".DLL";
            traceInt.Info(MyDLLCode);

            string[] libs = (dllMAIN + ";NomadObject.dll;NomadBase.dll;NomadDocument.dll;NomadProxy.dll").Split(';');
            for (t = 0; t < libs.Length; t++)
                libs[t] = dllPATH + "\\" + libs[t];

            XmlDocument xmlResult;

#if LIQdebug

            MyDLLCode=MyDLLCode.Replace("\r", "");
            MyDLLCode=MyDLLCode.Replace("\n", "\r\n");

            System.IO.StreamWriter sw=new StreamWriter(Path.Combine(dllPATH, "Conceptos.cs"), false, Encoding.UTF8);
            sw.Write(MyDLLCode);
            sw.Close();

            xmlResult=NCompiler.Compiler.CompileFile(Path.Combine(dllPATH, "Conceptos.cs"), dllPATH, dllTRG, libs, false);

#else
            xmlResult=NCompiler.Compiler.Compile(MyDLLCode, dllPATH, dllTRG, libs);
#endif
            NomadLog.Info("despues ncompiler " + this.codificadoras.ToString());
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
                        trace.Error("Error de Generacion: " + E.GetAttr("desc") + " - " + E.GetAttr("line"));
                    }
                    else
                    {
                        trace.Error("Error en el Concepto " + conceptoId + " - Linea: " + lineDif.ToString() + " - " + E.GetAttr("desc"));
                    }
                }
                return null;
            }
            myDif = DateTime.Now - myNow;
            NomadLog.Info("Generar DLL: " + myDif.TotalMilliseconds + "ms");

            return System.Reflection.Assembly.LoadFrom(dllPATH + "\\" + dllTRG);
        }

        public static void DebugLiq(NomadXML PARAM)
        {
            int t, SEC, SECTOT;
            NomadXML SINUSO, MyCUR, SINUSO2;
            NomadBatch MyBatch = NomadBatch.GetBatch("Depurar", "Depurar");
            NomadProxy proxy = NomadProxy.GetProxy();

            NucleusRH.Base.Liquidacion.LiquidacionDDO.LIQUIDACION_DDO LIQddo;
            NucleusRH.Base.Liquidacion.Liquidacion.LIQUIDACION LPRddo;

            SECTOT = 0; SEC = 0;
            if (PARAM.GetAttr("RECTEMP") == "1") SECTOT++;
            if (PARAM.GetAttr("PELTEMP") == "1") SECTOT++;
            if (PARAM.GetAttr("PERTEMP") == "1") SECTOT++;
            if (PARAM.GetAttr("LIQTEMP") == "1") SECTOT++;
            if (PARAM.GetAttr("EMPTEMP") == "1") SECTOT++;
            if (PARAM.GetAttr("VARTEMP") == "1") SECTOT++;
            if (PARAM.GetAttr("CONCTEMP") == "1") SECTOT++;
            if (PARAM.GetAttr("CODIFTEMP") == "1") SECTOT++;

            /////////////////////////////////////////////////////////////////////////////////////////////////////
            //Depurando Recibos-Liquidacion sin USO.
            MyBatch.SetMess("Depurando Recibos Liquidacion...");
            MyBatch.Log("Depurando Recibos Liquidacion...");

            SINUSO=NomadProxy.GetProxy().SQLService().GetXML("CLASS.NucleusRH.Base.Liquidacion.LiquidacionDDO.LIQUIDACION_DDO.DepurarRecibosLiquidacion", "<DATA RECIBOS=\"1\" />");
            SINUSO = SINUSO.FindElement("RECIBOS");
            for (MyCUR = SINUSO.FirstChild(), t = 1; MyCUR != null; MyCUR = MyCUR.Next(), t++)
            {
                MyBatch.SetMess("Depurando Recibos Liquidacion (" + t + "/" + SINUSO.ChildLength + ")...");
                if (proxy.Lock().LockOBJ("Liquidacion:id=" + MyCUR.GetAttr("oi_liquidacion")))
                {
                    try
                    {
                        NomadLog.Info("LIMPIAR LIQUIDACION_DDO - OI:" + MyCUR.GetAttr("oi_liquidacion_ddo") + " - INFO:" + MyCUR.GetAttr("c_empresa") + "-" + MyCUR.GetAttr("c_liquidacion") + "-" + MyCUR.GetAttr("e_secuencia"));
                        LIQddo = NucleusRH.Base.Liquidacion.LiquidacionDDO.LIQUIDACION_DDO.Get(MyCUR.GetAttr("oi_liquidacion_ddo"));
                        LIQddo.Recibos.Clear();
                        NomadEnvironment.GetCurrentTransaction().Save(LIQddo);
                    }
                    catch (Exception E)
                    {
                        MyBatch.Err("Se produjo un error al tratar de limpiar los recibos de la Liquidacion **" + MyCUR.GetAttr("c_empresa") + "-" + MyCUR.GetAttr("c_liquidacion") + "-" + MyCUR.GetAttr("e_secuencia") + "**.");
                        NomadLog.Error(E.Message, E);
                    }
                    proxy.Lock().UnLockOBJ("Liquidacion:id=" + MyCUR.GetAttr("oi_liquidacion"));
                }
                else
                    MyBatch.Wrn("Fallo bloquear la Liquidacion **" + MyCUR.GetAttr("c_empresa") + "-" + MyCUR.GetAttr("c_liquidacion") + "**.");
            }
            MyBatch.SetPro(10);

            /////////////////////////////////////////////////////////////////////////////////////////////////////
            //Depurando Recibos sin USO.
            if (PARAM.GetAttr("RECTEMP") == "1")
            {
                MyBatch.SetMess("Depurando los Recibos Temporales...");
                MyBatch.Log("Depurando los Recibos Temporales...");

                SINUSO=NomadProxy.GetProxy().SQLService().GetXML("CLASS.NucleusRH.Base.Liquidacion.LiquidacionDDO.LIQUIDACION_DDO.DepurarRecibos", "<DATA SINUSO=\"1\" e_periodo=\""+PARAM.GetAttr("e_periodo")+"\" />");
                SINUSO = SINUSO.FindElement("SIN-USO");
                for (MyCUR = SINUSO.FirstChild(), t = 1; MyCUR != null; MyCUR = MyCUR.Next(), t++)
                {
                    MyBatch.SetPro(10 + SEC * (80 / SECTOT), 10 + (SEC + 1) * (80 / SECTOT), SINUSO.ChildLength, t);

                    MyBatch.SetMess("Depurando los Recibos Temporales (" + t + "/" + SINUSO.ChildLength + ")...");
                    try
                    {
                        NomadLog.Info("ELIMINAR RECIBO - OI:" + MyCUR.GetAttr("oi_tot_liq_per") + " - INFO:" + MyCUR.GetAttr("c_empresa") + "-" + MyCUR.GetAttr("c_liquidacion") + "-" + MyCUR.GetAttr("e_secuencia") + "-" + MyCUR.GetAttr("d_ape_y_nom"));
                        Nomad.NSystem.Base.NomadTransaction.Delete(typeof(NucleusRH.Base.Liquidacion.Recibos.TOT_LIQ_PER), MyCUR.GetAttr("oi_tot_liq_per"));
                        EliminarRecibo(proxy.RunPath + "NOMAD\\RECIBOS\\" + proxy.AppName + "\\" + MyCUR.GetAttr("oi_tot_liq_per") + ".recibo.xml");
                    }
                    catch (Exception E)
                    {
                        MyBatch.Err("Se produjo un error al tratar de eliminar el Recibo **" + MyCUR.GetAttr("c_empresa") + "-" + MyCUR.GetAttr("c_liquidacion") + "-" + MyCUR.GetAttr("e_secuencia") + "-" + MyCUR.GetAttr("d_ape_y_nom") + "**.");
                        NomadLog.Error(E.Message, E);
                    }
                }

                SEC++;
                MyBatch.SetPro(10, 90, SECTOT, SEC);
            }

            /////////////////////////////////////////////////////////////////////////////////////////////////////
            //Depurando Personal Liquidacion sin USO.
            if (PARAM.GetAttr("PELTEMP") == "1")
            {
                MyBatch.SetMess("Depurando los Personal Liquidacion Temporales...");
                MyBatch.Log("Depurando los Personal Liquidacion Temporales...");

                SINUSO=NomadProxy.GetProxy().SQLService().GetXML("CLASS.NucleusRH.Base.Liquidacion.LiquidacionDDO.LIQUIDACION_DDO.DepurarPersonalLiquidacion", "<DATA SINUSO=\"1\" />");
                SINUSO = SINUSO.FindElement("SIN-USO");
                for (MyCUR = SINUSO.FirstChild(), t = 1; MyCUR != null; MyCUR = MyCUR.Next(), t++)
                {
                    MyBatch.SetPro(10 + SEC * (80 / SECTOT), 10 + (SEC + 1) * (80 / SECTOT), SINUSO.ChildLength, t);

                    MyBatch.SetMess("Depurando los Personal Liquidacion Temporales (" + t + "/" + SINUSO.ChildLength + ")...");
                    try
                    {
                        NomadLog.Info("ELIMINAR PERSONAL LIQUIDACION - OI:" + MyCUR.GetAttr("oi_per_liq_ddo") + " - INFO:" + MyCUR.GetAttr("c_empresa") + "-" + MyCUR.GetAttr("c_liquidacion") + "-" + MyCUR.GetAttr("d_ape_y_nom"));
                        Nomad.NSystem.Base.NomadTransaction.Delete(typeof(NucleusRH.Base.Liquidacion.PersonalLiquidacionDDO.PER_LIQ_DDO), MyCUR.GetAttr("oi_per_liq_ddo"));
                    }
                    catch (Exception E)
                    {
                        MyBatch.Err("Se produjo un error al tratar de eliminar el Personal Liquidacion **" + MyCUR.GetAttr("c_empresa") + "-" + MyCUR.GetAttr("c_liquidacion") + "-" + MyCUR.GetAttr("d_ape_y_nom") + "**.");
                        NomadLog.Error(E.Message, E);
                    }
                }

                SEC++;
                MyBatch.SetPro(10, 90, SECTOT, SEC);
            }

            /////////////////////////////////////////////////////////////////////////////////////////////////////
            //Depurando Personal sin USO.
            if (PARAM.GetAttr("PERTEMP") == "1")
            {
                MyBatch.SetMess("Depurando los Personal Temporales...");
                MyBatch.Log("Depurando los Personal Temporales...");

                SINUSO=NomadProxy.GetProxy().SQLService().GetXML("CLASS.NucleusRH.Base.Liquidacion.LiquidacionDDO.LIQUIDACION_DDO.DepurarPersonal", "<DATA SINUSO=\"1\" />");
                SINUSO = SINUSO.FindElement("SIN-USO");
                for (MyCUR = SINUSO.FirstChild(), t = 1; MyCUR != null; MyCUR = MyCUR.Next(), t++)
                {
                    MyBatch.SetPro(10 + SEC * (80 / SECTOT), 10 + (SEC + 1) * (80 / SECTOT), SINUSO.ChildLength, t);

                    MyBatch.SetMess("Depurando los Personal Temporales (" + t + "/" + SINUSO.ChildLength + ")...");
                    try
                    {
                        NomadLog.Info("ELIMINAR PERSONAL - OI:" + MyCUR.GetAttr("oi_per_emp_ddo") + " - INFO:" + MyCUR.GetAttr("c_empresa") + "-" + MyCUR.GetAttr("d_ape_y_nom"));
                        Nomad.NSystem.Base.NomadTransaction.Delete(typeof(NucleusRH.Base.Liquidacion.LegajoEmpresaDDO.PER_EMP_DDO), MyCUR.GetAttr("oi_per_emp_ddo"));
                    }
                    catch (Exception E)
                    {
                        MyBatch.Err("Se produjo un error al tratar de eliminar el Personal **" + MyCUR.GetAttr("c_empresa") + "-" + MyCUR.GetAttr("d_ape_y_nom") + "**.");
                        NomadLog.Error(E.Message, E);
                    }
                }

                SEC++;
                MyBatch.SetPro(10, 90, SECTOT, SEC);
            }

            /////////////////////////////////////////////////////////////////////////////////////////////////////
            //Depurando Ejecucion de Liquidaciones sin USO.
            if (PARAM.GetAttr("LIQTEMP") == "1")
            {
                MyBatch.SetMess("Depurando Ejecuciones de Liquidacion Temporales...");
                MyBatch.Log("Depurando Ejecuciones de Liquidacion Temporales...");

                SINUSO=NomadProxy.GetProxy().SQLService().GetXML("CLASS.NucleusRH.Base.Liquidacion.LiquidacionDDO.LIQUIDACION_DDO.DepurarLiquidacion", "<DATA SINUSO=\"1\" />");
                SINUSO2 = SINUSO.FindElement("LIQ");
                SINUSO = SINUSO.FindElement("EJE");

                for (MyCUR = SINUSO.FirstChild(), t = 1; MyCUR != null; MyCUR = MyCUR.Next(), t++)
                {
                    MyBatch.SetPro(10 + SEC * (80 / SECTOT), 10 + (SEC + 1) * (80 / SECTOT), SINUSO2.ChildLength + SINUSO.ChildLength, t);
                    MyBatch.SetMess("Depurando las Ejecuciones (" + t + "/" + (SINUSO2.ChildLength + SINUSO.ChildLength) + ")...");

                    try
                    {
                        //Obtengo la Liquidacion
                        LPRddo = NucleusRH.Base.Liquidacion.Liquidacion.LIQUIDACION.Get(MyCUR.GetAttr("oi_liquidacion"));

                        //Eliminando las Ejecuciones
                        string[] ids = MyCUR.GetAttr("oi_ejecucion").Split(',');
                        for (int d = 0; d < ids.Length; d++) LPRddo.EJECUCIONES.RemoveById(ids[d]);

                        //Guardando el DDO.
                        NomadLog.Info("ELIMINAR " + ids.Length + " EJECUCIONES - OI:" + MyCUR.GetAttr("oi_liquidacion") + " - INFO:" + MyCUR.GetAttr("c_empresa") + "-" + MyCUR.GetAttr("c_liquidacion"));
                        NomadEnvironment.GetCurrentTransaction().Save(LPRddo);
                    }
                    catch (Exception E)
                    {
                        MyBatch.Err("Se produjo un error al tratar Eliminar las Ejecuciones de **" + MyCUR.GetAttr("c_empresa") + "-" + MyCUR.GetAttr("c_liquidacion") + "**.");
                        NomadLog.Error(E.Message, E);
                    }
                }

                for (MyCUR = SINUSO2.FirstChild(); MyCUR != null; MyCUR = MyCUR.Next(), t++)
                {
                    MyBatch.SetPro(10 + SEC * (80 / SECTOT), 10 + (SEC + 1) * (80 / SECTOT), SINUSO2.ChildLength + SINUSO.ChildLength, t);
                    MyBatch.SetMess("Depurando las Ejecuciones (" + t + "/" + (SINUSO2.ChildLength + SINUSO.ChildLength) + ")...");
                    try
                    {
                        NomadLog.Info("ELIMINAR LIQUIDACION DDO - OI:" + MyCUR.GetAttr("oi_liquidacion_ddo") + " - INFO:" + MyCUR.GetAttr("c_empresa") + "-" + MyCUR.GetAttr("c_liquidacion"));
                        Nomad.NSystem.Base.NomadTransaction.Delete(typeof(NucleusRH.Base.Liquidacion.LiquidacionDDO.LIQUIDACION_DDO), MyCUR.GetAttr("oi_liquidacion_ddo"));
                    }
                    catch (Exception E)
                    {
                        MyBatch.Err("Se produjo un error al tratar Eliminar las Ejecuciones de **" + MyCUR.GetAttr("c_empresa") + "-" + MyCUR.GetAttr("c_liquidacion") + "**.");
                        NomadLog.Error(E.Message, E);
                    }
                }

                SEC++;
                MyBatch.SetPro(10, 90, SECTOT, SEC);
            }

            /////////////////////////////////////////////////////////////////////////////////////////////////////
            //Depurando EMPRESAS sin USO.
            if (PARAM.GetAttr("EMPTEMP") == "1")
            {
                MyBatch.SetMess("Depurando los Empresas Temporales...");
                MyBatch.Log("Depurando los Empresas Temporales...");
        SINUSO=NomadProxy.GetProxy().SQLService().GetXML("CLASS.NucleusRH.Base.Liquidacion.LiquidacionDDO.LIQUIDACION_DDO.DepurarEmpresas", "<DATA SINUSO=\"1\" />");
                SINUSO = SINUSO.FindElement("SIN-USO");

                for (MyCUR = SINUSO.FirstChild(), t = 1; MyCUR != null; MyCUR = MyCUR.Next(), t++)
                {
                    MyBatch.SetPro(10 + SEC * (80 / SECTOT), 10 + (SEC + 1) * (80 / SECTOT), SINUSO.ChildLength, t);

                    MyBatch.SetMess("Depurando los Empresas Temporales (" + t + "/" + SINUSO.ChildLength + ")...");
                    try
                    {
                        NomadLog.Info("ELIMINAR EMPRESAS - OI:" + MyCUR.GetAttr("oi_empresa_ddo") + " - INFO:" + MyCUR.GetAttr("c_empresa"));
                        Nomad.NSystem.Base.NomadTransaction.Delete(typeof(NucleusRH.Base.Liquidacion.EmpresasDDO.EMPRESA_DDO), MyCUR.GetAttr("oi_empresa_ddo"));
                    }
                    catch (Exception E)
                    {
                        MyBatch.Err("Se produjo un error al tratar de eliminar las Empresa **" + MyCUR.GetAttr("c_empresa") + "**.");
                        NomadLog.Error(E.Message, E);
                    }
                }

                SEC++;
                MyBatch.SetPro(10, 90, SECTOT, SEC);
            }

            /////////////////////////////////////////////////////////////////////////////////////////////////////
            //Depurando CONCEPTOS sin USO.
            if (PARAM.GetAttr("CONCTEMP") == "1")
            {
                MyBatch.SetMess("Depurando los Conceptos Temporales...");
                MyBatch.Log("Depurando los Conceptos Temporales...");

        SINUSO=NomadProxy.GetProxy().SQLService().GetXML("CLASS.NucleusRH.Base.Liquidacion.LiquidacionDDO.LIQUIDACION_DDO.DepurarConceptos", "<DATA SINUSO=\"1\" />");
                SINUSO = SINUSO.FindElement("SIN-USO");
                for (MyCUR = SINUSO.FirstChild(), t = 1; MyCUR != null; MyCUR = MyCUR.Next(), t++)
                {
                    MyBatch.SetPro(10 + SEC * (80 / SECTOT), 10 + (SEC + 1) * (80 / SECTOT), SINUSO.ChildLength, t);

                    MyBatch.SetMess("Depurando los Conceptos Temporales (" + t + "/" + SINUSO.ChildLength + ")...");
                    try
                    {
                        NomadLog.Info("ELIMINAR CONCEPTOS - OI:" + MyCUR.GetAttr("oi_cab_conc_ddo"));
                        Nomad.NSystem.Base.NomadTransaction.Delete(typeof(NucleusRH.Base.Liquidacion.CabeceraDeConceptosDDO.CAB_CONC_DDO), MyCUR.GetAttr("oi_cab_conc_ddo"));
                    }
                    catch (Exception E)
                    {
                        MyBatch.Err("Se produjo un error al tratar de eliminar los Conceptos.");
                        NomadLog.Error(E.Message, E);
                    }
                }

                SEC++;
                MyBatch.SetPro(10, 90, SECTOT, SEC);
            }

            /////////////////////////////////////////////////////////////////////////////////////////////////////
            //Depurando CODIFICAODRAS sin USO.
            if (PARAM.GetAttr("CODIFTEMP") == "1")
            {
                MyBatch.SetMess("Depurando los Codificadoras Temporales...");
                MyBatch.Log("Depurando los Codificadoras Temporales...");

        SINUSO=NomadProxy.GetProxy().SQLService().GetXML("CLASS.NucleusRH.Base.Liquidacion.LiquidacionDDO.LIQUIDACION_DDO.DepurarCodificadoras", "<DATA SINUSO=\"1\" />");
                SINUSO = SINUSO.FindElement("SIN-USO");
                for (MyCUR = SINUSO.FirstChild(), t = 1; MyCUR != null; MyCUR = MyCUR.Next(), t++)
                {
                    MyBatch.SetPro(10 + SEC * (80 / SECTOT), 10 + (SEC + 1) * (80 / SECTOT), SINUSO.ChildLength, t);

                    MyBatch.SetMess("Depurando los Codificadoras Temporales (" + t + "/" + SINUSO.ChildLength + ")...");
                    try
                    {
                        NomadLog.Info("ELIMINAR CODIFICADORA - OI:" + MyCUR.GetAttr("oi_codif_ddo"));
                        Nomad.NSystem.Base.NomadTransaction.Delete(typeof(NucleusRH.Base.Liquidacion.CabeceraDeCodificadoraDDO.CODIF_DDO), MyCUR.GetAttr("oi_codif_ddo"));
                    }
                    catch (Exception E)
                    {
                        MyBatch.Err("Se produjo un error al tratar de eliminar las Codificadoras.");
                        NomadLog.Error(E.Message, E);
                    }
                }

                SEC++;
                MyBatch.SetPro(10, 90, SECTOT, SEC);
            }

            /////////////////////////////////////////////////////////////////////////////////////////////////////
            //Depurando VARIABLES sin USO.
            if (PARAM.GetAttr("VARTEMP") == "1")
            {
                MyBatch.SetMess("Depurando los Variables Temporales...");
                MyBatch.Log("Depurando los Variables Temporales...");

        SINUSO=NomadProxy.GetProxy().SQLService().GetXML("CLASS.NucleusRH.Base.Liquidacion.LiquidacionDDO.LIQUIDACION_DDO.DepurarVariables", "<DATA SINUSO=\"1\" />");
                SINUSO = SINUSO.FindElement("SIN-USO");
                for (MyCUR = SINUSO.FirstChild(), t = 1; MyCUR != null; MyCUR = MyCUR.Next(), t++)
                {
                    MyBatch.SetPro(10 + SEC * (80 / SECTOT), 10 + (SEC + 1) * (80 / SECTOT), SINUSO.ChildLength, t);

                    MyBatch.SetMess("Depurando los Variables Temporales (" + t + "/" + SINUSO.ChildLength + ")...");
                    try
                    {
                        NomadLog.Info("ELIMINAR VARIABLES - OI:" + MyCUR.GetAttr("oi_var_ejec_ddo"));
                        Nomad.NSystem.Base.NomadTransaction.Delete(typeof(NucleusRH.Base.Liquidacion.VariablesPorEjecucionDDO.VAR_EJEC_DDO), MyCUR.GetAttr("oi_var_ejec_ddo"));
                    }
                    catch (Exception E)
                    {
                        MyBatch.Err("Se produjo un error al tratar de eliminar las Variables.");
                        NomadLog.Error(E.Message, E);
                    }
                }

                SEC++;
                MyBatch.SetPro(10, 90, SECTOT, SEC);
            }

            MyBatch.SetPro(100);
            MyBatch.Log("Fin.");
        }

        public static void GuardarRecibo(string fullName, NomadXML Recibo)
        {
            //Valido el contenido
            string XMLText = Recibo.ToString();
            if (string.IsNullOrEmpty(XMLText)) throw new Exception("El XML de recibo esta VACIO!!!!");

            //Guardo el archivo.
            StreamWriter objSW = new StreamWriter(fullName, false, System.Text.Encoding.ASCII);
            objSW.Write(XMLText);
            objSW.Close();
        }

        public static void EliminarRecibo(string path)
        {
            if (File.Exists(path))
                File.Delete(path);
        }

        private static void CleanIDs(NomadXML MyXML)
        {
            MyXML.SetAttr("id", "");
            for (NomadXML MyCUR = MyXML.FirstChild(); MyCUR != null; MyCUR = MyCUR.Next())
                CleanIDs(MyCUR);
        }

        public static string ObjectMD5(NomadObject MyOBJ)
        {
            NomadXML MyXML = new NomadXML(MyOBJ.SerializeAll());

            MyXML = MyXML.FirstChild();
            CleanIDs(MyXML);

            return Nomad.NSystem.Functions.StringUtil.GetMD5(MyXML.ToString());
        }

    class InfoRec
    {
      public InfoRec(int Legajo, string Hash, string Id)
      {
        this.Legajo = Legajo;
        this.Hash = Hash;
        this.Id = Id;
      }

      public int Legajo;
      public string Hash;
      public string Id;
    };

    private static void AsyncSaveCache(object MyPar)
    {
      string paso = "";
      NomadProxy MyProxy;
      structRecibos MyOBJ;
      MultiThreadManager MyPARAM = (MultiThreadManager)MyPar;

      TimeSpan myDif;
      DateTime myNow;
      DateTime myNow2;

      double EMPFILE = 0;
      int    EMPCNT = 0;

      double LIQFILE = 0;
      int    LIQCNT = 0;

      double RECFILE = 0;
      int    RECCNT = 0;

      double OTHER = 0;
      int    CNTOTHER = 0;
      double WAIT = 0;

      //Actualizo los Proxys.
      NomadLog.SetTrace(MyPARAM.GetTrace());
      MyProxy = MyPARAM.GetProxy();
      NomadProxy.SetProxy(MyProxy);
      NomadException.SetDumpPath(MyProxy.RunPath + "\\NOMAD\\ERROR");
      ListManager DataCache = MyPARAM.GetList("DataCache");

      //Trabajo
      NomadException ex=null;
      NomadBatch MyBatch = NomadBatch.GetBatch("AsyncSaveCache", "AsyncSaveCache");
      try
      {
        paso = "AsyncSaveCache-CREAR CARPETA DE RECIBOS.";
        //NomadLog.Info("AsyncSaveCache-START.");

        //Creo la carpeta de salida
        if (!Directory.Exists(MyProxy.RunPath + "NOMAD\\RECIBOS\\" + MyProxy.AppName))
            Directory.CreateDirectory(MyProxy.RunPath + "NOMAD\\RECIBOS\\" + MyProxy.AppName);

        //Preparo el CACHE.
        paso = "AsyncSaveCache-PREPARAR CACHE.";
        Nomad.Base.Manual.CACHE MyCache = new Nomad.Base.Manual.CACHE(MyProxy.RunPath + "\\CACHE\\"+MyProxy.AppName+"\\LIQUIDACION");
        MyCache.Prepare(typeof(NucleusRH.Base.Liquidacion.LegajoEmpresaDDO.PER_EMP_DDO));
        MyCache.Prepare(typeof(NucleusRH.Base.Liquidacion.PersonalLiquidacionDDO.PER_LIQ_DDO));


        while (!DataCache.Finish || DataCache.Length>0)
        {
          myNow2 = DateTime.Now;
          paso = "Espera DataCache.Get()";
          MyOBJ = (structRecibos)DataCache.Get();

          if (MyOBJ==null) continue;
          myDif = DateTime.Now - myNow2;
          WAIT+=myDif.TotalMilliseconds;

          try
          {
            myNow2 = DateTime.Now;

            ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            // PERSONAL EMPRESA
            if (MyOBJ.per_emp != null)
            {
              //Archivo
              paso = "AsyncSaveCache-SAVE PERSONAL EMPRESA - CACHE. Legajo: " + MyOBJ.MyID;
              myNow = DateTime.Now;
              MyCache.Save(MyOBJ.per_emp.Hash, MyOBJ.oi_personal_emp, MyOBJ.per_emp);
              myDif = DateTime.Now - myNow;
              EMPFILE+=myDif.TotalMilliseconds;

              //Cuento
              EMPCNT++;
            }

            ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            // PERSONAL LIQUIDACION
            if (MyOBJ.per_liq != null)
            {
              //Archivo
              paso = "AsyncSaveLegajosLiq-SAVE PERSONAL LIQUIDACION - FILE. Legajo: " + MyOBJ.MyID;
              myNow = DateTime.Now;
              MyCache.Save(MyOBJ.per_liq.Hash, MyOBJ.oi_personal_emp, MyOBJ.per_liq);
              myDif = DateTime.Now - myNow;
              LIQFILE+=myDif.TotalMilliseconds;

              //Cuento
              LIQCNT++;
            }

            ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            // Guardar Recibo
            if (MyOBJ.rec != null)
            {
              //Archivo
              paso = "AsyncSaveRecibos-SAVE RECIBO FILE. Legajo: " + MyOBJ.MyID;
              myNow = DateTime.Now;
							GuardarRecibo(MyProxy.RunPath + "NOMAD\\RECIBOS\\" + MyProxy.AppName + "\\" + MyOBJ.rec.Id + ".recibo.xml", MyOBJ.rec.ReciboXML);
              myDif = DateTime.Now - myNow;
              RECFILE+=myDif.TotalMilliseconds;

              //Cuento
              RECCNT++;
            }

            myDif = DateTime.Now - myNow2;
            OTHER+=myDif.TotalMilliseconds;
            CNTOTHER++;
          }
          catch (Exception e)
          {
            ex = NomadException.NewInternalException("AsyncSaveCache", e);
            ex.SetValue("paso", paso);
            MyBatch.Err("El Legajo " + MyOBJ.MyID + " - no se pudo guardar. Codigo de Error:" + ex.Id);
            ex.Dump();
            break;
          }
        }
        DataCache.GetFinish(ex);

        /*
        MyBatch.Log("AsyncSaveCache-FINISH.");
        if (EMPCNT>0) MyBatch.Log("AsyncSaveCache-EMP - CNT:" +  EMPCNT + " - FILE:" + Math.Round(EMPFILE) + "ms (" + Math.Round(EMPFILE/EMPCNT) + "ms)");
        if (LIQCNT>0) MyBatch.Log("AsyncSaveCache-LIQ - CNT:" +  LIQCNT + " - FILE:" + Math.Round(LIQFILE) + "ms (" + Math.Round(LIQFILE/LIQCNT) + "ms)");
        if (RECCNT>0) MyBatch.Log("AsyncSaveCache-REC - CNT:" +  RECCNT + " - FILE:" + Math.Round(RECFILE) + "ms (" + Math.Round(RECFILE/RECCNT) + "ms)");
        MyBatch.Log("AsyncSaveCache-OTHER - CNT:" +  CNTOTHER + " - TIME:" + Math.Round(OTHER) + "ms - WAIT:" + Math.Round(WAIT) + "ms - CALC: " + Math.Round(OTHER-EMPFILE-LIQFILE-RECFILE)+"ms - TOT: " + Math.Round((EMPCNT > 0 ? EMPFILE/EMPCNT : 0)+(LIQCNT>0?LIQFILE/LIQCNT:0)+(RECCNT>0?RECFILE/RECCNT:0)) + "ms");
        */

        NomadLog.Info("AsyncSaveCache-FINISH.");
        return;
      }
      catch (Exception e)
      {
        ex = NomadException.NewInternalException("AsyncSaveCache", e);
        ex.SetValue("paso", paso);
        MyBatch.Err("Error durante el Proceso de Liquidacion. Codigo de Error:" + ex.Id);
        ex.Dump();
        DataCache.GetFinish(ex);
        return;
      }
    }

    private static void AsyncSaveLegajos(object MyPar)
    {
      string paso = "";
      NomadProxy MyProxy;
      structRecibos MyOBJ;
      MultiThreadManager MyPARAM = (MultiThreadManager)MyPar;
      int Legajos = MyPARAM.GetParamInt("Legajos");

      TimeSpan myDif;
      DateTime myNow;
      DateTime myNow2;

      double EMPDB = 0;
      int    EMPCNT = 0;

      double OTHER = 0;
      int    CNTOTHER = 0;
      double WAIT = 0;
      double WAIT2 = 0;
            NomadLog.Info("en asyncsavelegajos: " + MyPARAM.ToString());
      //Actualizo los Proxys.
      NomadLog.SetTrace(MyPARAM.GetTrace());
      MyProxy = MyPARAM.GetProxy();
      NomadProxy.SetProxy(MyProxy);
      NomadException.SetDumpPath(MyProxy.RunPath + "\\NOMAD\\ERROR");
      ListManager LegajosLoaded = MyPARAM.GetList("LegajosLoaded");
      ListManager LegajosSaved = MyPARAM.GetList("LegajosSaved");
      ListManager DataCache = MyPARAM.GetList("DataCache");

      NomadException ex = null;
      NomadBatch MyBatch = NomadBatch.GetBatch("AsyncSaveLegajos", "AsyncSaveLegajos");
      try
      {
        //Conexion alternativa
        using (Nomad.Base.Manual.DB MyDB = new Nomad.Base.Manual.DB())
        {
          paso = "AsyncSaveLegajos-START.";
          NomadLog.Info("AsyncSaveLegajos-START.");

          //Cargo los HOW TO SAVE
          MyDB.Prepare(typeof(NucleusRH.Base.Liquidacion.LegajoEmpresaDDO.PER_EMP_DDO));

          while (!LegajosLoaded.Finish || LegajosLoaded.Length>0)
          {
            myNow2 = DateTime.Now;
            paso = "Espera LegajosLoaded.Get()";
            MyOBJ = (structRecibos)LegajosLoaded.Get();

            if (MyOBJ==null) continue;
            myDif = DateTime.Now - myNow2;
            WAIT+=myDif.TotalMilliseconds;

            try
            {
              myNow2 = DateTime.Now;

              ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
              // PERSONAL EMPRESA
              if (MyOBJ.per_emp != null && MyOBJ.per_emp.IsForInsert)
              {
                //Base de Datos
                paso = "AsyncSaveLegajos-SAVE PERSONAL EMPRESA - DB. Legajo: " + MyOBJ.MyID;
                myNow = DateTime.Now;
                MyDB.Save(MyOBJ.per_emp);
                myDif = DateTime.Now - myNow;
                EMPDB+=myDif.TotalMilliseconds;

                //Archivo
                structRecibos newCache = new structRecibos();
                newCache.NroLegajo = MyOBJ.NroLegajo;
                newCache.MyID = MyOBJ.MyID;
                newCache.oi_personal_emp = MyOBJ.oi_personal_emp;
                newCache.per_emp = MyOBJ.per_emp;
                newCache.per_liq = null;
                newCache.rec = null;
                DataCache.Add(newCache);

                //Cuento
                EMPCNT++;
              }

              //Espera?
              myNow = DateTime.Now;
              LegajosSaved.WaitLength(Legajos);
              myDif = DateTime.Now - myNow;
              WAIT2+=myDif.TotalMilliseconds;

              //Lo agrego a la lista de legajos guardadso
              LegajosSaved.Add(MyOBJ);

              myDif = DateTime.Now - myNow2;
              OTHER+=myDif.TotalMilliseconds;
              CNTOTHER++;

              /*
              if (
                MyOBJ.NroLegajo == 130 ||
                MyOBJ.NroLegajo == 140  ||
                MyOBJ.NroLegajo == 167  ||
                MyOBJ.NroLegajo == 246  ||
                MyOBJ.NroLegajo == 259  ||
                MyOBJ.NroLegajo == 263  ||
                MyOBJ.NroLegajo == 266  ||
                MyOBJ.NroLegajo == 279  ||
                MyOBJ.NroLegajo == 282  ||
                MyOBJ.NroLegajo == 284  ||
                MyOBJ.NroLegajo == 296
              )
              {
                string RootPath=NomadProxy.GetProxy().RunPath+"\\CACHE\\"+NomadProxy.GetProxy().AppName+"\\LIQUIDACION";
                NomadObject DATA;

                //DEBUG DATA - PERSONAL
                DATA = LegajoEmpresaDDO.PER_EMP_DDO.Get(MyOBJ.per_emp.Id, true);
                SaveCacheObject(RootPath + "\\DEBUG_DATA\\PER_EMP_DDO." + MyOBJ.NroLegajo + ".NEW.xml", DATA);
              }
              */
            }
            catch (Exception e)
            {
              ex = NomadException.NewInternalException("AsyncSaveLegajos", e);
              ex.SetValue("paso", paso);
              MyBatch.Err("El Legajo " + MyOBJ.MyID + " - no se pudo guardar. Codigo de Error:" + ex.Id);
              ex.Dump();
              break;
            }
          }
          LegajosLoaded.GetFinish(ex);
          LegajosSaved.AddFinish();

          /*
          MyBatch.Log("AsyncSaveLegajos-FINISH.");
          if (EMPCNT>0) MyBatch.Log("AsyncSaveLegajos-EMP - CNT:" +  EMPCNT + " - DB:" + Math.Round(EMPDB) + "ms (" + Math.Round(EMPDB/EMPCNT) + "ms)");
          MyBatch.Log("AsyncSaveLegajos-OTHER - CNT:" +  CNTOTHER + " - TIME:" + Math.Round(OTHER) + "ms - WAIT:" + Math.Round(WAIT) + "ms - WAIT2:" + Math.Round(WAIT2) + "ms - CALC: " + Math.Round(OTHER-EMPDB-WAIT2)+"ms");
          */

          NomadLog.Info("AsyncSaveLegajos-FINISH.");
          return;
        }
      }
      catch (Exception e)
      {
        ex = NomadException.NewInternalException("AsyncSaveLegajos", e);
        ex.SetValue("paso", paso);
        MyBatch.Err("Error durante el Proceso de Liquidacion. Codigo de Error:" + ex.Id);
        ex.Dump();

        LegajosLoaded.GetFinish(ex);
        LegajosSaved.AddFinish();
        return;
      }
    }

    private static void AsyncSaveLegajosLiq(object MyPar)
    {
      string paso = "";
      NomadProxy MyProxy;
      structRecibos MyOBJ;
      MultiThreadManager MyPARAM = (MultiThreadManager)MyPar;
      int Legajos = MyPARAM.GetParamInt("Legajos");

      TimeSpan myDif;
      DateTime myNow;
      DateTime myNow2;

      double LIQDB = 0;
      int    LIQCNT = 0;

      double OTHER = 0;
      int    CNTOTHER = 0;
      double WAIT = 0;
      double WAIT2 = 0;

      //Actualizo los Proxys.
      NomadLog.SetTrace(MyPARAM.GetTrace());
      MyProxy = MyPARAM.GetProxy();
      NomadProxy.SetProxy(MyProxy);
      NomadException.SetDumpPath(MyProxy.RunPath + "\\NOMAD\\ERROR");
      ListManager LegajosLoaded = MyPARAM.GetList("LegajosSaved");
      ListManager LegajosSaved = MyPARAM.GetList("LegajosLiqSaved");
      ListManager DataCache = MyPARAM.GetList("DataCache");

      NomadException ex = null;
      NomadBatch MyBatch = NomadBatch.GetBatch("AsyncSaveLegajosLiq", "AsyncSaveLegajosLiq");
      try
      {
        //Conexion alternativa
        using (Nomad.Base.Manual.DB MyDB = new Nomad.Base.Manual.DB())
        {
          paso = "AsyncSaveLegajosLiq-START.";
          NomadLog.Info("AsyncSaveLegajosLiq-START.");

          //Cargo los HOW TO SAVE
          MyDB.Prepare(typeof(NucleusRH.Base.Liquidacion.PersonalLiquidacionDDO.PER_LIQ_DDO));
          while (!LegajosLoaded.Finish || LegajosLoaded.Length>0)
          {
            myNow2 = DateTime.Now;
            paso = "Espera LegajosLoaded.Get()";
            MyOBJ = (structRecibos)LegajosLoaded.Get();

            if (MyOBJ==null) continue;
            myDif = DateTime.Now - myNow2;
            WAIT+=myDif.TotalMilliseconds;

            try
            {
              myNow2 = DateTime.Now;

              ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
              // PERSONAL LIQUIDACION
              if (MyOBJ.per_liq != null && MyOBJ.per_liq.IsForInsert)
              {
                if (MyOBJ.per_emp != null) MyOBJ.per_liq.IDPersonalDDO = MyOBJ.per_emp.Id;

                //Base de Datos
                paso = "AsyncSaveLegajosLiq-SAVE PERSONAL LIQUIDACION - DB. Legajo: " + MyOBJ.MyID;
                myNow = DateTime.Now;
                MyDB.Save(MyOBJ.per_liq);
                myDif = DateTime.Now - myNow;
                LIQDB+=myDif.TotalMilliseconds;

                //Archivo
                structRecibos newCache = new structRecibos();
                newCache.NroLegajo = MyOBJ.NroLegajo;
                newCache.MyID = MyOBJ.MyID;
                newCache.oi_personal_emp = MyOBJ.oi_personal_emp;
                newCache.per_emp = null;
                newCache.per_liq = MyOBJ.per_liq;
                newCache.rec = null;
                DataCache.Add(newCache);

                //Cuento
                LIQCNT++;
              }

              //Espera?
              myNow = DateTime.Now;
              LegajosSaved.WaitLength(Legajos);
              myDif = DateTime.Now - myNow;
              WAIT2+=myDif.TotalMilliseconds;

              //Lo agrego a la lista de legajos guardadso
              LegajosSaved.Add(MyOBJ);

              myDif = DateTime.Now - myNow2;
              OTHER+=myDif.TotalMilliseconds;
              CNTOTHER++;

              /*
              if (
                MyOBJ.NroLegajo == 130 ||
                MyOBJ.NroLegajo == 140  ||
                MyOBJ.NroLegajo == 167  ||
                MyOBJ.NroLegajo == 246  ||
                MyOBJ.NroLegajo == 259  ||
                MyOBJ.NroLegajo == 263  ||
                MyOBJ.NroLegajo == 266  ||
                MyOBJ.NroLegajo == 279  ||
                MyOBJ.NroLegajo == 282  ||
                MyOBJ.NroLegajo == 284  ||
                MyOBJ.NroLegajo == 296
              )
              {
                string RootPath=NomadProxy.GetProxy().RunPath+"\\CACHE\\"+NomadProxy.GetProxy().AppName+"\\LIQUIDACION";
                NomadObject DATA;

                //DEBUG DATA - LIQUIDACION
                DATA = PersonalLiquidacionDDO.PER_LIQ_DDO.Get(MyOBJ.per_liq.Id, true);
                SaveCacheObject(RootPath + "\\DEBUG_DATA\\PER_LIQ_DDO." + MyOBJ.NroLegajo + ".NEW.xml", DATA);
              }
              */

            }
            catch (Exception e)
            {
              ex = NomadException.NewInternalException("AsyncSaveLegajosLiq", e);
              ex.SetValue("paso", paso);
              MyBatch.Err("El Legajo " + MyOBJ.MyID + " - no se pudo guardar. Codigo de Error:" + ex.Id);
              ex.Dump();
              break;
            }
          }
          LegajosLoaded.GetFinish(ex);
          LegajosSaved.AddFinish();

          /*
          MyBatch.Log("AsyncSaveLegajosLiq-FINISH.");
          if (LIQCNT>0) MyBatch.Log("AsyncSaveLegajosLiq-LIQ - CNT:" +  LIQCNT + " - DB:" + Math.Round(LIQDB) + "ms (" + Math.Round(LIQDB/LIQCNT) + "ms)");
          MyBatch.Log("AsyncSaveLegajosLiq-OTHER - CNT:" +  CNTOTHER + " - TIME:" + Math.Round(OTHER) + "ms - WAIT:" + Math.Round(WAIT) + "ms - WAIT2:" + Math.Round(WAIT2) + "ms - CALC: " + Math.Round(OTHER-LIQDB-WAIT2)+"ms");
          */

          NomadLog.Info("AsyncSaveLegajosLiq-FINISH.");
          return;
        }
      }
      catch (Exception e)
      {
        ex = NomadException.NewInternalException("AsyncSaveLegajosLiq", e);
        ex.SetValue("paso", paso);
        MyBatch.Err("Error durante el Proceso de Liquidacion. Codigo de Error:" + ex.Id);
        ex.Dump();

        LegajosLoaded.GetFinish(ex);
        LegajosSaved.AddFinish();
        return;
      }
    }

    private static void AsyncSaveRecibos(object MyPar)
    {
      string paso = "";
      NomadProxy MyProxy;
      structRecibos MyOBJ;
      MultiThreadManager MyPARAM = (MultiThreadManager)MyPar;

      TimeSpan myDif;
      DateTime myNow;
      DateTime myNow2;
      string newId;

      double RECDB = 0;
      int    RECCNT = 0;

      double OTHER = 0;
      int    CNTOTHER = 0;
      double WAIT = 0;

      //Actualizo los Proxys.
      NomadLog.SetTrace(MyPARAM.GetTrace());
      MyProxy = MyPARAM.GetProxy();
      NomadProxy.SetProxy(MyProxy);
      NomadException.SetDumpPath(MyProxy.RunPath + "\\NOMAD\\ERROR");
      ListManager RecibosGen   = MyPARAM.GetList("RecibosGen");
      ListManager RecibosSaved = MyPARAM.GetList("RecibosSaved");
      ListManager DataCache = MyPARAM.GetList("DataCache");

      NomadException ex = null;
      NomadBatch MyBatch = NomadBatch.GetBatch("AsyncSaveRecibos", "AsyncSaveRecibos");
      try
      {
        //Conexion alternativa
        using (Nomad.Base.Manual.DB MyDB = new Nomad.Base.Manual.DB())
        {
          paso = "AsyncSaveRecibos-START.";
          NomadLog.Info("AsyncSaveRecibos-START.");

          //Cargo los HOW TO SAVE
          MyDB.Prepare(typeof(NucleusRH.Base.Liquidacion.Recibos.TOT_LIQ_PER));

          while (!RecibosGen.Finish || RecibosGen.Length>0)
          {
            myNow2 = DateTime.Now;
            paso = "Espera RecibosGen.Get()";
            MyOBJ = (structRecibos)RecibosGen.Get();

            if (MyOBJ==null) continue;
            myDif = DateTime.Now - myNow2;
            WAIT+=myDif.TotalMilliseconds;

            try
            {
              myNow2 = DateTime.Now;

              ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
              // RECIBO DE SUELDO
              if (MyOBJ.rec != null)
              {
                paso = "AsyncSaveRecibos-SAVE RECIBO. Legajo: " + MyOBJ.MyID;
                NomadLog.Info("AsyncSaveRecibos-SAVE RECIBO");
                if (MyOBJ.per_emp != null)
                {
                  MyOBJ.rec.oi_per_emp_ddo = MyOBJ.per_emp.Id;

                  paso = "AsyncSaveRecibos-SAVE RECIBO-Anticipos. Legajo: " + MyOBJ.MyID;
                  //Anticipos
                  foreach (NucleusRH.Base.Liquidacion.Recibos.ANT_LIQ_PER Ant in MyOBJ.rec.ANT_LIQ_PER)
                  {
                    if (Ant.oi_anticipo_ddo.StartsWith("T."))
                    {
                      newId = MyOBJ.per_emp.Anticipos[int.Parse(Ant.oi_anticipo_ddo.Split('.')[1])].Id;
                      NomadLog.Info("ANT-FOUND-ID: " + Ant.oi_anticipo_ddo + "-->" + newId);
                      Ant.oi_anticipo_ddo = newId;
                    }
                  }

                  paso = "AsyncSaveRecibos-SAVE RECIBO-DescAnticipos. Legajo: " + MyOBJ.MyID;
                  //Descuento
                  foreach (NucleusRH.Base.Liquidacion.Recibos.DANT_LIQ_PER DAnt in MyOBJ.rec.DANT_LIQ_PER)
                  {
                    if (DAnt.oi_desc_ant_ddo.StartsWith("T."))
                    {
                      newId = ((NucleusRH.Base.Liquidacion.LegajoEmpresaDDO.ANTICIPO_DDO)MyOBJ.per_emp.Anticipos[int.Parse(DAnt.oi_desc_ant_ddo.Split('.')[1])]).DescuentosAnticipo[int.Parse(DAnt.oi_desc_ant_ddo.Split('.')[2])].Id;
                      NomadLog.Info("DANT-FOUND-ID: " + DAnt.oi_desc_ant_ddo + "-->" + newId);
                      DAnt.oi_desc_ant_ddo = newId;
                    }
                  }

                  paso = "AsyncSaveRecibos-SAVE RECIBO-Embargos. Legajo: " + MyOBJ.MyID;
                  //Embargos
                  foreach (NucleusRH.Base.Liquidacion.Recibos.EMB_LIQ_PER Emb in MyOBJ.rec.EMB_LIQ_PER)
                  {
                    if (Emb.oi_embargo_ddo.StartsWith("T."))
                    {
                      newId = MyOBJ.per_emp.Embargos[int.Parse(Emb.oi_embargo_ddo.Split('.')[1])].Id;
                      NomadLog.Info("EMB-FOUND-ID: " + Emb.oi_embargo_ddo + "-->" + newId);
                      Emb.oi_embargo_ddo = newId;
                    }
                  }

                  paso = "AsyncSaveRecibos-SAVE RECIBO-Consumos. Legajo: " + MyOBJ.MyID;
                  //Consumos
                  foreach (NucleusRH.Base.Liquidacion.Recibos.CON_LIQ_PER Con in MyOBJ.rec.CON_LIQ_PER)
                  {
                    if (Con.oi_consumo_ddo.StartsWith("T."))
                    {
                      newId = MyOBJ.per_emp.Consumos[int.Parse(Con.oi_consumo_ddo.Split('.')[1])].Id;
                      NomadLog.Info("CON-FOUND-ID: " + Con.oi_consumo_ddo + "-->" + newId);
                      Con.oi_consumo_ddo = newId;
                    }
                  }

                  paso = "AsyncSaveRecibos-SAVE RECIBO-Cargos. Legajo: " + MyOBJ.MyID;
                  //Cargos
                  foreach (NucleusRH.Base.Liquidacion.Recibos.CONC_LIQ_PER Conc in MyOBJ.rec.CONC_LIQ_PER)
                  {
                    foreach (NucleusRH.Base.Liquidacion.Recibos.CONC_CARGO Cargo in Conc.CONC_CARGO)
                    {
                    Cargo.oi_cargo_ddo=MyOBJ.per_emp.Cargos[Cargo.IdxCargo].Id;
                    }
                  }
                }
                if (MyOBJ.per_liq != null) MyOBJ.rec.oi_per_liq_ddo = MyOBJ.per_liq.Id;

                //Base de Datos
                paso = "AsyncSaveRecibos-SAVE RECIBO DB. Legajo: " + MyOBJ.MyID;
                myNow = DateTime.Now;
                MyOBJ.rec.f_gen = myNow;
                MyDB.Save(MyOBJ.rec);
                myDif = DateTime.Now - myNow;
                RECDB+=myDif.TotalMilliseconds;

                //Archivo
                structRecibos newCache = new structRecibos();
                newCache.NroLegajo = MyOBJ.NroLegajo;
                newCache.MyID = MyOBJ.MyID;
                newCache.oi_personal_emp = MyOBJ.oi_personal_emp;
                newCache.per_emp = null;
                newCache.per_liq = null;
                newCache.rec = MyOBJ.rec;
                DataCache.Add(newCache);

                //Cuento
                RECCNT++;

                //Actualizo la lista de recibos guardados
                InfoRec newRec = new InfoRec(MyOBJ.rec.NroLegajo, MyOBJ.rec.Hash, MyOBJ.rec.Id);
                RecibosSaved.Add(newRec);
              }

              myDif = DateTime.Now - myNow2;
              OTHER+=myDif.TotalMilliseconds;
              CNTOTHER++;
            }
            catch (Exception e)
            {
              ex = NomadException.NewInternalException("AsyncSaveRecibos", e);
              ex.SetValue("paso", paso);
              MyBatch.Err("El Recibo para " + MyOBJ.MyID + " - no se pudo guardar. Codigo de Error:" + ex.Id);
              ex.Dump();
              break;
            }
          }
          RecibosGen.GetFinish(ex);
          RecibosSaved.AddFinish();
          DataCache.AddFinish();

          MyBatch.Log("AsyncSaveRecibos-FINISH.");
          if (RECCNT>0) MyBatch.Log("AsyncSaveRecibos-REC - CNT:" +  RECCNT + "  - DB:" + Math.Round(RECDB) + "ms (" + Math.Round(RECDB/RECCNT) + "ms)");
          MyBatch.Log("AsyncSaveRecibos-OTHER - CNT:" +  CNTOTHER + "  - TIME:" + Math.Round(OTHER) + "ms - WAIT:" + Math.Round(WAIT) + "ms - CALC: " + Math.Round(OTHER-RECDB)+"ms");

          NomadLog.Info("AsyncSaveRecibos-FINISH.");
          return;
        }
      }
      catch (Exception e)
      {
        ex = NomadException.NewInternalException("AsyncSaveRecibos", e);
        ex.SetValue("paso", paso);
        MyBatch.Err("Error durante el Proceso de Liquidacion. Codigo de Error:" + ex.Id);
        ex.Dump();

        RecibosGen.GetFinish(ex);
        RecibosSaved.AddFinish();
        DataCache.AddFinish();
        return;
      }
    }

        private static string GenerateLegText(Hashtable legList, string id)
        {
            NomadXML xmlLEG;
            if (legList == null) return "oi_personal_emp = " + id;
            if (!legList.ContainsKey(id)) return "oi_personal_emp = " + id;

            xmlLEG = (NomadXML)legList[id];
            return "Legajo " + xmlLEG.GetAttr("e_numero_legajo") + " - " + xmlLEG.GetAttr("d_ape_y_nom");
        }

        private static Hashtable LoadLegajosEmpresa(NomadProxy proxy, NomadXML LEGEMPIns, string IDs, string oi_cab_conc_ddo, string oi_empresa_ddo, string oi_var_ejec_ddo, string oi_codif_ddo, string oi_liquidacion, int e_periodo, Hashtable legList, string oi_tipo_liq, string IdsPerEmpDDO, int oi_liq_genera, string IdsTotLiqPer)
        {
            string paso = "", myID = "";
            try
            {
                Hashtable PERSONAS = new Hashtable();
                Hashtable hashVariables = null;
                string Params;
                NomadXML MyRESULT, MyPER, MyCUR, MyCUR2;
                int d;

                for (MyCUR = LEGEMPIns.FirstChild(); MyCUR != null; MyCUR = MyCUR.Next())
                {
                    Params = "<FILTRO oi_personal_emp=\"" + IDs + "\" oi_cab_conc_ddo=\"" + oi_cab_conc_ddo + "\" oi_empresa_ddo=\"" + oi_empresa_ddo + "\" oi_var_ejec_ddo=\"" + oi_var_ejec_ddo + "\" oi_codif_ddo=\"" + oi_codif_ddo + "\" oi_liquidacion=\"" + oi_liquidacion + "\" e_periodo=\"" + e_periodo.ToString() + "\" oi_per_emp_ddo=\"" + (IdsPerEmpDDO!="" ? IdsPerEmpDDO.Substring(1) :"" )+ "\" oi_tot_liq_per=\"" + (IdsTotLiqPer != "" ? IdsTotLiqPer.Substring(1) : "") + "\" oi_liq_genera=\"" + oi_liq_genera + "\" MODO=\"" + MyCUR.GetAttr("sql") + "\" />";
                    MyRESULT = proxy.SQLService().GetXML("CLASS.NucleusRH.Base.Liquidacion.LegajoEmpresaDDO.PER_EMP_DDO." + (oi_tipo_liq == "7" ? "QryLegajoInterface" : "QryLegajoParcial"), Params);

                    switch (MyCUR.GetAttr("path"))
                    {
                        case "": //Agregar Elementos
                            {
                                paso = "QryLegajoParcial-Agregar Elementos-" + MyCUR.GetAttr("sql");
                                for (MyCUR2 = MyRESULT.FirstChild(); MyCUR2 != null; MyCUR2 = MyCUR2.Next())
                                {
                                    myID = GenerateLegText(legList, MyCUR2.GetAttr("oi_personal_emp"));
                                    PERSONAS.Add(MyCUR2.GetAttr("oi_personal_emp"), MyCUR2);
                                }
                            } break;

                        case ".": //Solo Pegar Atributos Adicionales
                            {
                                paso = "QryLegajoParcial-Atributos Adicionales-" + MyCUR.GetAttr("sql");
                                for (MyCUR2 = MyRESULT.FirstChild(); MyCUR2 != null; MyCUR2 = MyCUR2.Next())
                                {
                                    myID = GenerateLegText(legList, MyCUR2.GetAttr("oi_personal_emp"));

                                    MyPER = (NomadXML)PERSONAS[MyCUR2.GetAttr("oi_personal_emp")];
                                    for (d = 0; d < MyCUR2.Attrs.Count; d++)
                                    {
                                        Params = MyCUR2.Attrs[d].ToString();
                                        MyPER.SetAttr(Params, MyCUR2.GetAttr(Params));
                                    }
                                }
                            } break;
                        case "JSON":
                            {
                                //falta buscar los ois variables ddo
                                if(hashVariables == null)
                                    hashVariables = NomadEnvironment.QueryHashtableValue(LegajoEmpresaDDO.PER_EMP_DDO.Resources.QRY_VAREJEC, "<PARAM oi_liquidacion=\'" + oi_liq_genera + "\' />", "c_variable", "oi_variable_ddo", false);
                                NomadXML xmlTemplate = MyRESULT.FindElement("VAL_A_ANNO_DDO"); //hacer variable el nombre del template
                                string prev = "T";
                                string[] keys = MyCUR.GetAttr("key").Split(',');

                                for (MyCUR2 = MyRESULT.FindElement("ROW"); MyCUR2 != null; MyCUR2 = MyCUR2.Next())
                                {
                                    MyPER = (NomadXML)PERSONAS[MyCUR2.GetAttr("oi_personal_emp")];
                                    MyPER = MyPER.FindElement(keys[0]);

                                    if (MyPER != null)
                                    {
                                        Dictionary<string,double> dic = RHLiq.LiqUtilBase.NomadXMLADiccionario(MyCUR2);
                                        Dictionary<string,Dictionary<string,double>> dicFinal = new Dictionary<string,Dictionary<string,double>>();
                                        foreach(string key in dic.Keys)
                                        {
                                            string codigo = key.Substring(0, key.Length - 2);
                                            string mes = key.Substring( key.Length - 2, 2);
                                            double valor = dic[key];

                                            if(!dicFinal.ContainsKey(codigo))
                                            {
                                                dicFinal.Add(codigo,new Dictionary<string,double>());
                                            }
                                            dicFinal[codigo].Add(mes,valor);
                                        }

                                        foreach (string keyVarAno in dicFinal.Keys)
                                        {
                                            string key = keyVarAno.Substring(0, keyVarAno.LastIndexOf("_"));
                                            string anio = keyVarAno.Substring(keyVarAno.LastIndexOf("_") + 1,4);

                                            NomadXML nuevo = new NomadXML(xmlTemplate.ToString()).FirstChild();
                                            nuevo.SetAttr("oi_personal_emp", MyCUR2.GetAttr("oi_personal_emp"));

                                            nuevo.SetAttr("CodigoVariable", key);
                                            nuevo.SetAttr("IDVariable", hashVariables[key].ToString());

                                            foreach (string keyMes in dicFinal[keyVarAno].Keys)
                                            {
                                                nuevo.SetAttr("ValorEntrada" + int.Parse(keyMes), dicFinal[keyVarAno][keyMes]);
                                            }

                                            nuevo.SetAttr("Periodo", anio);
                                            nuevo.SetAttr("id", prev + "." + MyPER.ChildLength);
                                            MyPER.AddXML(nuevo);
                                        }
                                    }
                                }
                            }
                            break;
                            //case tipo JSON dentro se toma lo devuelto por el query (json) y se transforma a diccionario. El query tiene que devolver un template para completar con valores del dic
                            //agregar a MyPer
                            //case tipo DDO el query devuelve el ddo en parte, el case une las partes y lo agrega a MyPer
                        default:
                            {
                                paso = "QryLegajoParcial-Childs-" + MyCUR.GetAttr("path") + "-" + MyCUR.GetAttr("sql");
                                string[] paths = MyCUR.GetAttr("path").Split(',');
                                string[] keys = MyCUR.GetAttr("key").Split(',');
                                string prev = "T";

                                for (MyCUR2 = MyRESULT.FirstChild(); MyCUR2 != null; MyCUR2 = MyCUR2.Next())
                                {
                                    myID = GenerateLegText(legList, MyCUR2.GetAttr("oi_personal_emp"));

                                    MyPER = null;
                                    for (d = 0; d < paths.Length; d++)
                                    {
                                        if (d == 0)
                                        {
                                            MyPER = (NomadXML)PERSONAS[MyCUR2.GetAttr("oi_personal_emp")];
                                            MyPER = MyPER.FindElement(paths[0]);
                                        }
                                        else
                                            if (d == 1)
                                            {
                                                MyPER = MyPER.FindElement2(keys[0], keys[1], MyCUR2.GetAttr(keys[1]));
                                                prev = MyPER.GetAttr("id");
                                                MyPER = MyPER.FindElement(paths[1]);
                                                break;
                                            }
                                            else
                                            {
                                                MyPER = null;
                                            }
                                    }

                                    if (MyPER != null)
                                    {
                                        MyCUR2.SetAttr("id", prev + "." + MyPER.ChildLength);
                                        MyPER.AddXML(MyCUR2);
                                    }
                                }
                            } break;

                    }

                }
                return PERSONAS;
            }
            catch (Exception e)
            {
                NomadException ex = NomadException.NewInternalException("LoadLegajosEmpresa", e);
                ex.SetValue("Paso", paso);
                ex.SetValue("Legajo", myID);
                throw ex;
            }
        }

        private static Hashtable LoadLegajosLiquidacion(NomadProxy proxy, NomadXML LEGEMPIns, string IDs, string oi_cab_conc_ddo, string oi_empresa_ddo, string oi_var_ejec_ddo, string oi_codif_ddo, string oi_liquidacion, int e_periodo, Hashtable legList,string oi_tipo_liq,int oi_liq_genera,string idPerEmpDDO)
        {
            string paso = "", myID = "";
            try
            {
                Hashtable PERSONAS = new Hashtable();
                string Params;
                NomadXML MyRESULT, MyPER, MyCUR, MyCUR2;

                for (MyCUR = LEGEMPIns.FirstChild(); MyCUR != null; MyCUR = MyCUR.Next())
                {
                    paso = "QRYPersonalParcial";
                    Params = "<FILTRO oi_personal_emp= \"" + IDs + "\" oi_tipo_liq=\""+oi_tipo_liq+"\" oi_liq_genera=\"" + oi_liq_genera + "\" oi_per_emp_ddo=\"" + (idPerEmpDDO !="" ? idPerEmpDDO.Substring(1) : "") + "\" oi_cab_conc_ddo=\"" + oi_cab_conc_ddo + "\" oi_empresa_ddo=\"" + oi_empresa_ddo + "\" oi_var_ejec_ddo=\"" + oi_var_ejec_ddo + "\" oi_codif_ddo=\"" + oi_codif_ddo + "\" oi_liquidacion=\"" + oi_liquidacion + "\" e_periodo=\"" + e_periodo.ToString() + "\" MODO=\"" + MyCUR.GetAttr("sql") + "\" />";
                    MyRESULT = proxy.SQLService().GetXML("CLASS.NucleusRH.Base.Liquidacion.PersonalLiquidacionDDO.PER_LIQ_DDO." + (oi_tipo_liq == "7" ? "QRYPersonalInterface" : "QRYPersonalParcial"), Params);

                    switch (MyCUR.GetAttr("path"))
                    {
                        case "": //Agregar Elementos
                            paso = "QRYPersonalParcial-Agregar Elementos-" + MyCUR.GetAttr("sql");
                            for (MyCUR2 = MyRESULT.FirstChild(); MyCUR2 != null; MyCUR2 = MyCUR2.Next())
                            {
                                myID = GenerateLegText(legList, MyCUR2.GetAttr("oi_personal_emp"));
                                PERSONAS.Add(MyCUR2.GetAttr("oi_personal_emp"), MyCUR2);
                            }
                            break;

                        default:
                            {
                                switch (MyCUR.GetAttr("mode"))
                                {
                                    case "":
                                        paso = "QRYPersonalParcial-mode:vacio-" + MyCUR.GetAttr("path") + "-" + MyCUR.GetAttr("sql");
                                        for (MyCUR2 = MyRESULT.FirstChild(); MyCUR2 != null; MyCUR2 = MyCUR2.Next())
                                        {
                                            myID = GenerateLegText(legList, MyCUR2.GetAttr("oi_personal_emp"));
                                            MyPER = (NomadXML)PERSONAS[MyCUR2.GetAttr("oi_personal_emp")];
                                            MyPER = MyPER.FindElement(MyCUR.GetAttr("path"));

                                            MyCUR2.SetAttr("id", "T." + MyPER.ChildLength);
                                            MyPER.AddXML(MyCUR2);
                                        }
                                        break;

                                    case "ALL":
                                        {
                                            paso = "QRYPersonalParcial-mode:all-" + MyCUR.GetAttr("path") + "-" + MyCUR.GetAttr("sql");
                                            foreach (string key in PERSONAS.Keys)
                                            {
                                                MyPER = (NomadXML)PERSONAS[key];
                                                MyPER = MyPER.FindElement(MyCUR.GetAttr("path"));
                                                for (MyCUR2 = MyRESULT.FirstChild(); MyCUR2 != null; MyCUR2 = MyCUR2.Next())
                                                {
                                                    myID = GenerateLegText(legList, MyCUR2.GetAttr("oi_personal_emp"));
                                                    MyCUR2.SetAttr("id", "T." + MyPER.ChildLength);
                                                    MyPER.AddXML(MyCUR2);
                                                }
                                            }
                                            break;
                                        }

                                    case "SUM":
                                        paso = "QRYPersonalParcial-mode:sum-" + MyCUR.GetAttr("path") + "-" + MyCUR.GetAttr("sql");
                                        for (MyCUR2 = MyRESULT.FirstChild(); MyCUR2 != null; MyCUR2 = MyCUR2.Next())
                                        {
                                            myID = GenerateLegText(legList, MyCUR2.GetAttr("oi_personal_emp"));
                                            MyPER = (NomadXML)PERSONAS[MyCUR2.GetAttr("oi_personal_emp")];
                                            MyPER = MyPER.FindElement(MyCUR.GetAttr("path"));
                                            MyPER = MyPER.FindElement2("VAL_VAREN_DDO", MyCUR.GetAttr("key"), MyCUR2.GetAttr(MyCUR.GetAttr("key")));

                                            //Acumulo
                                            MyPER.SetAttr("Valor", MyPER.GetAttrDouble("Valor") + MyCUR2.GetAttrDouble("Valor"));
                                        }
                                        break;
                                }
                            } break;

                    }
                }
                return PERSONAS;
            }
            catch (Exception e)
            {
                NomadException ex = NomadException.NewInternalException("LoadLegajosEmpresa", e);
                ex.SetValue("Paso", paso);
                ex.SetValue("Legajo", myID);
                throw ex;
            }
        }

    class clsLoadLegajos
    {
      Nomad.Base.Manual.CACHE MyCache;
      LIQUIDACION liq;
      NomadXML LEGEMPIns;
      NomadXML LEGLIQIns;
      Hashtable xmlRespuesta;
      List<string> PagesPerIds;
      List<string> PagesInterfaceIds;
      List<string> PagesRecibosIds;

      string oi_cab_conc_ddo;
      string oi_empresa_ddo;
      string oi_var_ejec_ddo;
      string oi_codif_ddo;
      string oi_liquidacion;
      string oi_liquidacion_ddo;
      string oi_liq_genera;
      int e_periodo;
      NomadBatch MyBatch;
      Hashtable legList;
      NomadProxy myProxy;

      //Prepara el objeto de carga y obtiene los parametros principales
      public int Prepare(NomadBatch MyBatch, string oi_cab_conc_ddo, string oi_empresa_ddo, string oi_var_ejec_ddo, string oi_codif_ddo, string oi_liquidacion, string oi_liquidacion_ddo, string oi_liq_genera, int e_periodo, string[] IDs, Hashtable legList, int PageSize)
      {
        //Obtengo el proxy
        NomadProxy MyProxy = NomadProxy.GetProxy();
        this.myProxy = MyProxy.Clone();
        this.myProxy.SetValue("DATA", "pool", "LIQ");
        this.myProxy.SetRunPath(MyProxy.RunPath);

        //Valores principales
        this.MyBatch = MyBatch;
        this.oi_cab_conc_ddo = oi_cab_conc_ddo;
        this.oi_empresa_ddo = oi_empresa_ddo;
        this.oi_var_ejec_ddo = oi_var_ejec_ddo;
        this.oi_codif_ddo = oi_codif_ddo;
        this.oi_liquidacion = oi_liquidacion;
        this.oi_liquidacion_ddo = oi_liquidacion_ddo;
        this.oi_liq_genera = oi_liq_genera;
        this.e_periodo = e_periodo;
        this.legList = legList;

        //Preparo el CACHE.
        this.MyCache = new Nomad.Base.Manual.CACHE(MyProxy.RunPath + "\\CACHE\\"+MyProxy.AppName+"\\LIQUIDACION");
        this.MyCache.Prepare(typeof(NucleusRH.Base.Liquidacion.LegajoEmpresaDDO.PER_EMP_DDO));
        this.MyCache.Prepare(typeof(NucleusRH.Base.Liquidacion.PersonalLiquidacionDDO.PER_LIQ_DDO));

        //Basicos
        this.liq = LIQUIDACION.Get(oi_liquidacion);
        this.LEGEMPIns = MyProxy.SQLService().GetXML("CLASS.NucleusRH.Base.Liquidacion.LegajoEmpresaDDO.PER_EMP_DDO." + (liq.oi_tipo_liq == "7" ? "QryLegajoInterface" : "QryLegajoParcial"), "<FILTRO />");
        this.LEGLIQIns = MyProxy.SQLService().GetXML("CLASS.NucleusRH.Base.Liquidacion.PersonalLiquidacionDDO.PER_LIQ_DDO." + (liq.oi_tipo_liq == "7" ? "QRYPersonalInterface" : "QRYPersonalParcial"), "<FILTRO />");

        //Especial LSD
        if (this.liq.oi_tipo_liq == "7")
          this.xmlRespuesta = NomadEnvironment.QueryHashtable(NucleusRH.Base.Liquidacion.LegajoEmpresaDDO.PER_EMP_DDO.Resources.QRY_BUSCAR_FOTO, "<PARAM oi_liquidacion='" + liq.oi_liq_genera + "' />","oi_personal_emp");
        else
          this.xmlRespuesta = null;

        //Lista de IDS
        string IDLists = "";
        string IDInterfaceList = "";
        string IDRecibosList = "";
        int cnt = 0;

        string perId;
        for (int i = 0; i < IDs.Length; i++)
        {
          //Extraigo el ID
          perId = IDs[i];

          //Busco la Personal en la Lista de XML
          if (!legList.ContainsKey(perId))
          {
            NomadLog.Warning("El OID: " + perId + " no pertenece a la liquidacion...");
            continue;
          }

          NomadXML xmlLEG = (NomadXML)legList[perId];
          if (xmlLEG.GetAttr("f_cierre") != "")
          {
            MyBatch.Wrn("El Legajo " + xmlLEG.GetAttr("e_numero_legajo") + " - " + xmlLEG.GetAttr("d_ape_y_nom") + " tiene el recibo Cerrado.");
            continue;
          }

          IDLists += "," + perId.Trim(); cnt++;
        }

        if (cnt == 0)
        {
          NomadLog.Error("No hay ningun legajo Valido para Liquidar....");
          return 0;
        }

        //Genero la paginas de Arrays
        this.PagesPerIds = new List<string>();
        this.PagesInterfaceIds = new List<string>();
        this.PagesRecibosIds = new List<string>();
        IDs = IDLists.Substring(1).Split(',');

        //Recorro la lista
        IDInterfaceList = "";
        IDRecibosList = "";
        IDLists="";
        cnt=0;
        for (int i = 0; i < IDs.Length; i++)
        {
          //Extraigo el ID
          perId = IDs[i];

          //Lo agrego a la lista
          IDLists += "," + perId.Trim(); cnt++;

          //Especial LSD
          if (liq.oi_tipo_liq == "7")
          {
            NomadXML xmlTemp = new NomadXML(this.xmlRespuesta[perId.Trim()].ToString()).FirstChild();
            IDInterfaceList += "," + (xmlTemp.GetAttr("oi_per_emp_ddo")).Trim();
            IDRecibosList += "," + (xmlTemp.GetAttr("oi_tot_liq_per")).Trim();
          }

          if ((cnt >= PageSize) || (i + 1 >= IDs.Length))
          {
            this.PagesPerIds.Add(IDLists);
            this.PagesInterfaceIds.Add(IDInterfaceList);
            this.PagesRecibosIds.Add(IDRecibosList);
            IDLists = "";
            IDInterfaceList = "";
            IDRecibosList = "";
            cnt = 0;
          }
        }

        //Lista de IDS
        return this.PagesPerIds.Count;
      }

      public int LoadPage(int Page, ListManager LegajosLoaded)
      {
        if (Page<0 ||Page>=PagesPerIds.Count) return 0;

        //Variables utilizadas
        structRecibos MyOBJ;
        string strDDO, strHash;
        int cnt;
        
        //REALIZO LAS CONSULTAS
        string IDLists = PagesPerIds[Page].Substring(1);
        string IDInterfaceList = PagesInterfaceIds[Page];
        string IDRecibosList = PagesRecibosIds[Page];
        Hashtable PERSONAS = LoadLegajosEmpresa(this.myProxy, this.LEGEMPIns, IDLists, this.oi_cab_conc_ddo, this.oi_empresa_ddo, this.oi_var_ejec_ddo, this.oi_codif_ddo, this.oi_liquidacion, this.e_periodo, this.legList, this.liq.oi_tipo_liq, IDInterfaceList, this.liq.oi_liq_genera, IDRecibosList);
        Hashtable PERLIQS = LoadLegajosLiquidacion(this.myProxy, this.LEGLIQIns, IDLists, this.oi_cab_conc_ddo, this.oi_empresa_ddo, this.oi_var_ejec_ddo, this.oi_codif_ddo, this.oi_liquidacion, this.e_periodo, this.legList, this.liq.oi_tipo_liq, this.liq.oi_liq_genera, IDInterfaceList);

        //ARMO LOS DDOS
        string[] subIDs = IDLists.Split(',');

        /////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //Recorro los Legajos y los Construyo.
        cnt=0;
        for (int d = 0; d < subIDs.Length; d++)
        {
          string perId = subIDs[d];
          NomadXML xmlLEG = (NomadXML)legList[perId];
          int nroLegajo = xmlLEG.GetAttrInt("e_numero_legajo");
          string myID = "Legajo " + xmlLEG.GetAttr("e_numero_legajo") + " - " + xmlLEG.GetAttr("d_ape_y_nom");

          //Cargando....
          NomadLog.Info("Cargar Persona: " + myID);
          if (!PERSONAS.ContainsKey(perId))
          {
            NomadLog.Warning("AsyncLoadLegajos-No se Cargo el Legajo Empresa para " + myID);
          }
          else if (!PERLIQS.ContainsKey(perId))
          {
            NomadLog.Warning("AsyncLoadLegajos-No se Cargo el Legajo Liquidacion para " + myID);
          }
          else
          {
            NomadXML xmlLEGEMP = (NomadXML)PERSONAS[perId];
            NomadXML xmlLEGLIQ = (NomadXML)PERLIQS[perId];

            /////////////////////////////////////////////////////////////////////////////////////////////////////////////
            //Creo el Objeto.
            MyOBJ = new structRecibos();
            MyOBJ.NroLegajo = nroLegajo;
            MyOBJ.MyID = myID;
            MyOBJ.oi_personal_emp = perId;
            MyOBJ.per_emp = null;
            MyOBJ.per_liq = null;
            MyOBJ.rec = null;

            bool forceLoad = false;

            /////////////////////////////////////////////////////////////////////////////////////////////////////////////
            //LEGAJO EMPRESA
            strDDO = "<object nmd-status=\"~i,\" class=\"NucleusRH.Base.Liquidacion.LegajoEmpresaDDO.PER_EMP_DDO\" >" + xmlLEGEMP.ToString() + "</object>";
            strHash = MD5("<(" + this.oi_cab_conc_ddo + "-" + this.oi_empresa_ddo + "-" + this.oi_var_ejec_ddo + "-" + this.oi_codif_ddo + "-" + this.oi_liquidacion + ")>" + strDDO);

            MyOBJ.per_emp = liq.c_liquidacion.StartsWith("JOCHE") ? null : (NucleusRH.Base.Liquidacion.LegajoEmpresaDDO.PER_EMP_DDO)MyCache.LoadObject(strHash, typeof(NucleusRH.Base.Liquidacion.LegajoEmpresaDDO.PER_EMP_DDO), perId);
            if (MyOBJ.per_emp == null) { MyOBJ.per_emp = (NucleusRH.Base.Liquidacion.LegajoEmpresaDDO.PER_EMP_DDO)Nomad.NSystem.Base.NomadEnvironment.GetObject(typeof(NucleusRH.Base.Liquidacion.LegajoEmpresaDDO.PER_EMP_DDO), strDDO); forceLoad = true; }
            MyOBJ.per_emp.Hash = strHash;

            /////////////////////////////////////////////////////////////////////////////////////////////////////////////
            //LEGAJO LIQUIDACION
            strDDO = "<object nmd-status=\"~i,\" class=\"NucleusRH.Base.Liquidacion.PersonalLiquidacionDDO.PER_LIQ_DDO\" >" + xmlLEGLIQ.ToString() + "</object>";
            strHash = MD5("<(" + this.oi_cab_conc_ddo + "-" + this.oi_empresa_ddo + "-" + this.oi_var_ejec_ddo + "-" + this.oi_codif_ddo + "-" + this.oi_liquidacion + ")>" + strDDO);

            MyOBJ.per_liq = forceLoad ? null : (NucleusRH.Base.Liquidacion.PersonalLiquidacionDDO.PER_LIQ_DDO)MyCache.LoadObject(strHash, typeof(NucleusRH.Base.Liquidacion.PersonalLiquidacionDDO.PER_LIQ_DDO), perId);
            if (MyOBJ.per_liq == null) MyOBJ.per_liq = (NucleusRH.Base.Liquidacion.PersonalLiquidacionDDO.PER_LIQ_DDO)Nomad.NSystem.Base.NomadEnvironment.GetObject(typeof(NucleusRH.Base.Liquidacion.PersonalLiquidacionDDO.PER_LIQ_DDO), strDDO);
            MyOBJ.per_liq.Hash = strHash;
            MyOBJ.per_liq.IDLiquidacion = oi_liquidacion_ddo;

            /////////////////////////////////////////////////////////////////////////////////////////////////////////////
            //Agrego el Registro a la Lista.
            LegajosLoaded.Add(MyOBJ); cnt++;
          }
        }

        return cnt;
      }
    };

    private static void AsyncLoadLegajos(object MyPar)
    {
      string paso = "Iniciando.";
      MultiThreadManager MyPARAM = (MultiThreadManager)MyPar;

      //Estadisticas
      DateTime myNow;
      double WAIT = 0;
      double PREPARE = 0;
      double LOAD = 0;
      int CNT = 0;

      //Actualizo los Proxys.
      NomadLog.SetTrace(MyPARAM.GetTrace());
      NomadProxy MyProxy = MyPARAM.GetProxy();
      NomadProxy.SetProxy(MyProxy);
      NomadException.SetDumpPath(MyProxy.RunPath + "\\NOMAD\\ERROR");
      ListManager LegajosLoaded = MyPARAM.GetList("LegajosLoaded");

      //Cantidad de legajos por pagina
      int Legajos = MyPARAM.GetParamInt("Legajos");

      //Batch
      NomadBatch MyBatch = NomadBatch.GetBatch("AsyncLoadLegajos", "AsyncLoadLegajos");

      try
      {
        clsLoadLegajos MyLoader = new clsLoadLegajos();

        //Preparar
        paso = "Preparando el proceso de carga.";
        myNow = DateTime.Now;
        int PageCount = MyLoader.Prepare(
          MyBatch,
          MyPARAM.GetParamString("oi_cab_conc_ddo"),
          MyPARAM.GetParamString("oi_empresa_ddo"),
          MyPARAM.GetParamString("oi_var_ejec_ddo"),
          MyPARAM.GetParamString("oi_codif_ddo"),
          MyPARAM.GetParamString("oi_liquidacion"),
          MyPARAM.GetParamString("oi_liquidacion_ddo"),
          MyPARAM.GetParamString("oi_liq_genera"),
          MyPARAM.GetParamInt("e_periodo"),
          MyPARAM.GetParamString("IDS").Split(','),
          (Hashtable)MyPARAM.GetParam("legList"),
          Legajos
        );
        if (PageCount == 0)
        {
          NomadLog.Error("AsyncLoadLegajos-No hay ningun legajo Valido para Liquidar....");
          LegajosLoaded.AddFinish();
          return;
        }
        PREPARE+=(DateTime.Now - myNow).TotalMilliseconds;

        //Recorro las paginas
        for(int p=0; p<PageCount; p++)
        {
          paso = "Cargando pagina " + p + ".";
          myNow = DateTime.Now;
          CNT+=MyLoader.LoadPage(p, LegajosLoaded);
          LOAD+=(DateTime.Now - myNow).TotalMilliseconds;

          //Espera?
          paso = "Esperando...";
          myNow = DateTime.Now;
          LegajosLoaded.WaitLength(Legajos);
          WAIT+=(DateTime.Now - myNow).TotalMilliseconds;
        }

        //Fin de la carga
        paso = "Finalizando.";
        LegajosLoaded.AddFinish();

        //Estadisticas
        MyBatch.Log("AsyncLoadLegajos-FINISH.");
        if (CNT > 0) MyBatch.Log("AsyncLoadLegajos - CNT: " + CNT + " - WAIT: " + Math.Round(WAIT) + "ms - PREPARE: " + Math.Round(PREPARE) + "ms - LOAD: " + Math.Round(LOAD) + "ms (" + Math.Round(LOAD/CNT) + "ms)");

        NomadLog.Info("AsyncLoadLegajos-FINISH.");
        return;
      }
      catch (Exception e)
      {
        NomadException ex = NomadException.NewInternalException("AsyncLoadLegajos", e);
        ex.SetValue("paso", paso);
        MyBatch.Err("Error cargando legajo. Codigo de Error:" + ex.Id);
        ex.Dump();

        //Indico que todo termino
        LegajosLoaded.AddFinish();
        return;
      }
    }

        private static void CrearCarpetaCache(string RootPath, string nombreCarpeta)
        {
            if (!System.IO.Directory.Exists(RootPath + "\\"+nombreCarpeta))
                System.IO.Directory.CreateDirectory(RootPath + "\\"+nombreCarpeta);
        }

    private static void AsyncLiquidar(object MyPar)
    {
      NomadProxy MyProxy;
      structRecibos MyOBJ;
      MultiThreadManager MyPARAM = (MultiThreadManager)MyPar;

      string oi_liquidacion_ddo = MyPARAM.GetParamString("oi_liquidacion_ddo");
      int Secuencia = MyPARAM.GetParamInt("Secuencia");
      int CantidadLegajos = MyPARAM.GetParamInt("Legajos");
      bool Confidencial = MyPARAM.GetParamBool("Confidencial");

      TimeSpan myDif;
      DateTime myNow;
      DateTime myNow2;

      int LegNoRec = 0;
      int CNT = 0;
      double TIME = 0;
      double WAIT1 = 0;
      double WAIT2 = 0;

      //Obtener el LiqUtilBase
      LiqUtilBase MyLIQObj = MyPARAM.LiqUtil;

      //Actualizo los Proxys.
      NomadLog.SetTrace(MyPARAM.GetTrace());
      MyProxy = MyPARAM.GetProxy();
      NomadProxy.SetProxy(MyProxy);
      NomadException.SetDumpPath(MyProxy.RunPath + "\\NOMAD\\ERROR");

      //Listas de trabajo
      ListManager LegajosLiqSaved = MyPARAM.GetList("LegajosLiqSaved");
      ListManager RecibosGen = MyPARAM.GetList("RecibosGen");

      string paso = "Iniciando...";
      Recibos.TOT_LIQ_PER Recibo;
      NomadBatch MyBatch = NomadBatch.GetBatch("AsyncLiquidar", "AsyncLiquidar");
      NomadLog.SetTraceBatch(new NomadBatchTrace(NomadProxy.GetProxy().Batch().Trace, "AsyncLiquidar"));
      NomadException ex = null;

      try
      {
        paso = "AsyncLiquidar-START.";
        NomadLog.Info("AsyncLiquidar-START.");

        DateTime myStart = DateTime.Now;
        while (!LegajosLiqSaved.Finish || LegajosLiqSaved.Length > 0)
        {
          myNow2 = DateTime.Now;
          paso = "Espera LegajosLiqSaved.Get()";
          MyOBJ = (structRecibos)LegajosLiqSaved.Get();

          if (MyOBJ==null) continue;
          myDif = DateTime.Now - myNow2;
          WAIT1+=myDif.TotalMilliseconds;

          paso = "Liquidando Persona: " + MyOBJ.MyID;
          MyBatch.Log("Liquidando Persona: " + MyOBJ.MyID);

          try
          {
            //Aplico los Conceptos
            myNow = DateTime.Now;
            Recibo=MyLIQObj.ApplyConceptos(MyOBJ.per_emp, MyOBJ.per_liq, MyOBJ.rec, MyPARAM.propiedadesGrupos, MyPARAM.propiedadesJson);
            myDif = DateTime.Now - myNow;
            CNT++;
            TIME+=myDif.TotalMilliseconds;

            string strRecHash = ObjectMD5(Recibo);
            paso = "Guarda Recibo para " + MyOBJ.MyID;
            Recibo.Hash = strRecHash;
            Recibo.NroLegajo = MyOBJ.per_emp.NroLegajo;

            if (Recibo.CONC_LIQ_PER.Count != 0)
            {
              //Actualizo los Datos del Recibo
              Recibo.oi_liquidacion_ddo = oi_liquidacion_ddo;
              Recibo.oi_per_emp_ddo = MyOBJ.per_emp.Id;
              Recibo.e_secuencia = Secuencia;
              Recibo.l_confidencial = Confidencial;

              //Espera?
              myNow = DateTime.Now;
              RecibosGen.WaitLength(CantidadLegajos);
              myDif = DateTime.Now - myNow;
              WAIT2+=myDif.TotalMilliseconds;

              //Agregar Recibo
              MyOBJ.rec = Recibo;
              RecibosGen.Add(MyOBJ);
            }
            else
            {
              LegNoRec++;
              MyPARAM.SetParam("LegNoRec", LegNoRec);
              MyBatch.Wrn("El Recibo para el legajo " + MyOBJ.per_emp.NroLegajo + " - " + MyOBJ.per_emp.ApellidoyNombre + " no genero conceptos y no se generara....");
              continue;
            }

          }
          catch (Exception e)
          {
            LegNoRec++;
            MyPARAM.SetParam("LegNoRec", LegNoRec);

            //Error
            ex = NomadException.NewInternalException("AsyncLiquidar", e);
            ex.SetValue("paso", paso);
            MyBatch.Err("El Legajo " + MyOBJ.MyID + " - no se pudo guardar. Codigo de Error:" + ex.Id);
            ex.Dump();
          }
        }
        myDif = DateTime.Now - myStart;
        LegajosLiqSaved.GetFinish(ex);
        RecibosGen.AddFinish();
        /*
        MyBatch.Log("AsyncLiquidar-FINISH.");

        if (CNT > 0)
        {
          MyBatch.Log("ApplyConceptos " + CNT + ": " + Math.Round(TIME) + "ms (" + Math.Round(TIME/CNT)+ "ms)");
          MyBatch.Log("Invoke: " + Math.Round(MyLIQObj.invokeTime) + "ms (" + Math.Round(MyLIQObj.invokeTime/CNT)+ "ms)");
          MyBatch.Log("execute: " + Math.Round(MyLIQObj.executeTime) + "ms (" + Math.Round(MyLIQObj.executeTime/CNT)+ "ms) - DIF: " + Math.Round((MyLIQObj.executeTime-MyLIQObj.invokeTime)/CNT)+ "ms");
          MyBatch.Log("initialize: " + Math.Round(MyLIQObj.initializeTime) + "ms (" + Math.Round(MyLIQObj.initializeTime/CNT)+ "ms)");
          MyBatch.Log("acumn: " + Math.Round(MyLIQObj.acumnTime) + "ms (" + Math.Round((MyLIQObj.acumnTime)/CNT)+ "ms)");
        }
        MyBatch.Log("WAIT1: " + Math.Round(WAIT1) + "ms - WAIT2: " + Math.Round(WAIT2) + "ms -  OTHER: " + Math.Round(myDif.TotalMilliseconds - WAIT1 - WAIT2 - TIME)  + "ms");
        */

        NomadLog.Info("AsyncLiquidar-FINISH.");
        return;
      }
      catch (Exception e)
      {
        ex = NomadException.NewInternalException("AsyncLiquidar", e);
        ex.SetValue("paso", paso);
        MyBatch.Err("Error durante el Proceso de Liquidacion. Codigo de Error:" + ex.Id);
        ex.Dump();

        LegajosLiqSaved.GetFinish(ex);
        RecibosGen.AddFinish();
        return;
      }
    }

    DateTime lastRefreshProgress;
        public void RefreshProgress(DateTime myStart, int legCount, int legSav, NomadBatch MyBatch)
        {
      DateTime Now = DateTime.Now;
      if ((Now - this.lastRefreshProgress).TotalSeconds < 10) return;
      this.lastRefreshProgress=Now;

            double myTim;
            string unit = "min";
            MyBatch.SetPro(10, 90, legCount, legSav + 1);

            //Cargo el Legajo
            myTim = ((TimeSpan)(Now - myStart)).TotalMinutes;
            myTim = (myTim / (legSav + 1)) * (legCount - legSav);
            if (myTim < 2)
            {
                myTim *= 60;
                unit = "seg";
            }

            if (legSav > 10) MyBatch.SetMess("Liquidando Legajos (" + legSav + "/" + legCount + ")-(ETA " + Math.Ceiling(myTim) + " " + unit + ")...");
            else MyBatch.SetMess("Liquidando Legajos (" + legSav + "/" + legCount + ")...");
        }

    public void StartLiq(int idLiquidacion, string idsLiquidar)
    {
      string paso = "Inicio";
      this.MyBatch = NomadBatch.GetBatch("Liquidar", "Liquidar");
      try
      {
        DateTime myNow;
        TimeSpan myDif;
        double WAIT1 = 0;
        double WAIT2 = 0;
        double LOAD = 0;
        double PREPARE = 0;
        int CNT = 0;

        paso = "Comienza la Liquidacion";
        MyBatch.Log("Comienza la Liquidacion...");
        this.MyLiq = NucleusRH.Base.Liquidacion.Liquidacion.LIQUIDACION.Get(idLiquidacion, false);

        string msgerror = ValidacionesIniciales(idsLiquidar);

        if (msgerror != null)
        {
          MyBatch.Err(msgerror);
          return;
        }

        LiqUtilBase MyLIQObj = PrepararLiquidacion(idsLiquidar);

        DateTime myStart;

        //Thread Adicional para guardar los Recibos.....
        MultiThreadManager MyAsync = new MultiThreadManager();
        ListManager LegajosLoaded = MyAsync.GetList("LegajosLoaded");     //Lista de legajos cargados
        ListManager LegajosSaved = MyAsync.GetList("LegajosSaved");       //Lista de legajos guardados
        ListManager LegajosLiqSaved = MyAsync.GetList("LegajosLiqSaved"); //Lista de legajos guardados
        ListManager RecibosGen = MyAsync.GetList("RecibosGen");           //Lista de recibos generados
        ListManager RecibosSaved = MyAsync.GetList("RecibosSaved");       //Lista de recibos guardados
        ListManager DataCache = MyAsync.GetList("DataCache");             //Lista de cache

        //Agrupo los IDs
        MyAsync.SetParam("IDS", idsLiquidar);
        MyAsync.SetParam("legList", this.legList);

        MyAsync.SetParam("oi_cab_conc_ddo", this.conceptos.Id);
        MyAsync.SetParam("oi_empresa_ddo", this.empresa.Id);
        MyAsync.SetParam("c_empresa", this.empresa.Codigo);
        MyAsync.SetParam("oi_var_ejec_ddo", this.variables.Id);
        MyAsync.SetParam("oi_codif_ddo", this.codificadoras.Id);
        MyAsync.SetParam("oi_liquidacion", this.MyLiq.Id);
        MyAsync.SetParam("c_liquidacion", this.MyLiq.c_liquidacion);
        //if (this.MyLiq.oi_liq_genera != null)
        MyAsync.SetParam("oi_liq_genera", this.MyLiq.oi_liq_genera.ToString());
        MyAsync.SetParam("oi_liquidacion_ddo", this.liquidacion.Id);
        MyAsync.SetParam("e_periodo", this.MyLiq.e_periodo);
        MyAsync.SetParam("Legajos", this.CantidadLegajos);
        MyAsync.SetParam("Secuencia", this.Secuencia);
        MyAsync.SetParam("LegNoRec", 0);
        MyAsync.SetParam("Confidencial", MyLiq.l_confidencial);

        //Pagina de carga
        int LoadSizePage = idsLiquidar.Split(',').Length / 10;
        if (LoadSizePage > this.CantidadLegajos * 4) LoadSizePage = this.CantidadLegajos * 4;
        if (LoadSizePage < this.CantidadLegajos) LoadSizePage = this.CantidadLegajos;
                NomadLog.Info("ya preparó y cargó dll " + this.codificadoras.ToString());

        //Especiales
        MyAsync.LiqUtil = MyLIQObj;
        MyAsync.propiedadesGrupos = this.propiedadesGrupos;
        MyAsync.propiedadesJson = this.propiedadesJson;

        //paso = "inicia thread AsyncLoadLegajos";
        //Thread Adicional para Cargar los Legajos.....
        //System.Threading.Thread MyThreadLoadLegajo = new System.Threading.Thread(AsyncLoadLegajos);
        //MyThreadLoadLegajo.Name = "AsyncLoadLegajos";
        //MyThreadLoadLegajo.Start(MyAsync);

        //Preparo la liquidacion
        myNow = DateTime.Now;
        clsLoadLegajos MyLoader = new clsLoadLegajos();
        int PageCurr = 0;
        int PageCount = MyLoader.Prepare(
          MyBatch,
          this.conceptos.Id,
          this.empresa.Id,
          this.variables.Id,
          this.codificadoras.Id,
          this.MyLiq.Id,
          this.liquidacion.Id,
          this.MyLiq.oi_liq_genera.ToString(),
          this.MyLiq.e_periodo,
          idsLiquidar.Split(','),
          this.legList,
          LoadSizePage
        );
        if (PageCount == 0)
        {
          NomadLog.Error("No hay ningun legajo Valido para Liquidar....");
          LegajosLoaded.AddFinish();
          return;
        }
        PREPARE+=(DateTime.Now - myNow).TotalMilliseconds;

        paso = "inicia thread AsyncSaveLegajos";
        //Thread Adicional para Cargar los Legajos.....
        System.Threading.Thread MyThreadSaveLegajo = new System.Threading.Thread(AsyncSaveLegajos);
        MyThreadSaveLegajo.Name = "AsyncSaveLegajos";
        MyThreadSaveLegajo.Start(MyAsync);

        paso = "inicia thread AsyncSaveLegajosLiq";
        //Thread Adicional para Cargar los Legajos.....
        System.Threading.Thread MyThreadSaveLegajoLiq = new System.Threading.Thread(AsyncSaveLegajosLiq);
        MyThreadSaveLegajoLiq.Name = "AsyncSaveLegajosLiq";
        MyThreadSaveLegajoLiq.Start(MyAsync);

        paso = "inicia thread AsyncLiquidar";
        //Thread Adicional para Cargar los Legajos.....
        System.Threading.Thread MyThreadLiquidar = new System.Threading.Thread(AsyncLiquidar);
        MyThreadLiquidar.Name = "AsyncLiquidar";
        MyThreadLiquidar.Start(MyAsync);

        paso = "inicia thread AsyncSaveRecibos";
        //Thread Adicional para Guardar los Recibos.....
        System.Threading.Thread MyThreadSaveRec = new System.Threading.Thread(AsyncSaveRecibos);
        MyThreadSaveRec.Name = "AsyncSaveRecibos";
        MyThreadSaveRec.Start(MyAsync);

        paso = "inicia thread AsyncSaveCache";
        //Thread Adicional para Guardar los Archivos de Cache.....
        System.Threading.Thread MyThreadSaveCache = new System.Threading.Thread(AsyncSaveCache);
        MyThreadSaveCache.Name = "AsyncSaveCache";
        MyThreadSaveCache.Start(MyAsync);

        //Esperando a que finalize la carga.
        int legSav;

        //Actualizando Recibos
        legSav = 0;
        this.lastRefreshProgress = myStart = DateTime.Now;
        while (!RecibosSaved.Finish || RecibosSaved.Length > 0)
        {
          if (RecibosSaved.Length > 0 || PageCurr >= PageCount)
          {
            myNow = DateTime.Now;
            paso = "Busca recibo guardado";
            InfoRec ReciboInfo = (InfoRec)RecibosSaved.Get();
            if (ReciboInfo != null)
            {
              myDif = DateTime.Now - myNow;
              WAIT1+=myDif.TotalMilliseconds;

              //Actualizo el Progreso
              legSav++;
              RefreshProgress(myStart, legCount, legSav + MyAsync.GetParamInt("LegNoRec"), MyBatch);

              //Guardo el recibo
              paso = "Obtiene recibo de legajo " + ReciboInfo.Legajo;
              LiquidacionDDO.RECIBO MyRec = (LiquidacionDDO.RECIBO)this.liquidacion.Recibos.GetByAttribute("Legajo", ReciboInfo.Legajo);

              if (MyRec == null)
              {
                MyRec = new LiquidacionDDO.RECIBO();
                MyRec.Legajo = ReciboInfo.Legajo;
                MyRec.Hash = ReciboInfo.Hash;
                MyRec.Recibo = ReciboInfo.Id;
                MyRec.Secuencia = Secuencia;
                this.liquidacion.Recibos.Add(MyRec);
              }
              else
              {
                MyRec.Hash = ReciboInfo.Hash;
                MyRec.Recibo = ReciboInfo.Id;
                MyRec.Secuencia = Secuencia;
              }
            }
          }
          else if (LegajosLoaded.Length < LoadSizePage)
          {
            //Comienzo a medir a partir de cargar la primer pagina
            myNow = DateTime.Now;
            paso = "Cargando legajos";
            CNT+=MyLoader.LoadPage(PageCurr, LegajosLoaded);
            LOAD+=(DateTime.Now - myNow).TotalMilliseconds;
            if (PageCurr == 0) myStart = DateTime.Now;

            PageCurr++;
            if (PageCurr >= PageCount) LegajosLoaded.AddFinish();
          } else {
            System.Threading.Thread.Sleep(100);
            WAIT2+=100;
          }
        }
        myDif = DateTime.Now - myStart;

        //Contadores
        if (legCount > 0) MyBatch.Log("LiqConceptos " + legCount + ": " + myDif.TotalMilliseconds + "ms (" + Math.Round(myDif.TotalMilliseconds/legCount)+ "ms)");
        if (CNT > 0) MyBatch.Log("LoadLegajos - CNT: " + CNT + " - PREPARE: " + Math.Round(PREPARE) + "ms - LOAD: " + Math.Round(LOAD) + "ms (" + Math.Round(LOAD/CNT) + "ms)");
        MyBatch.Log("LoadLegajos - WAIT1: " + Math.Round(WAIT1) + "ms - WAIT2: " + Math.Round(WAIT2) + "ms - OTHER: " + Math.Round(myDif.TotalMilliseconds - WAIT1 - WAIT2 -LOAD)  + "ms");

        //Inicio la Transaccion
        paso = "Guarda liquidacion";
        MyBatch.SetMess("Guardando la Liquidacion...");
        NomadEnvironment.GetCurrentTransaction().Begin();
        try
        {
          if (this.old_liquidacion != null)
          {
            this.old_liquidacion.Recibos.Clear();
            this.old_liquidacion.Estado = "D";
            NomadEnvironment.GetCurrentTransaction().Save(this.old_liquidacion);
          }
          this.liquidacion.Estado = "F";
          NomadEnvironment.GetCurrentTransaction().Save(this.liquidacion);

          //COMMIT
          NomadEnvironment.GetCurrentTransaction().Commit();
        }
        catch (Exception)
        {
          NomadEnvironment.GetCurrentTransaction().Rollback();
          throw;
        }

        //Esperando al cache
        while (!DataCache.Finish || !DataCache.isFinishGet || DataCache.Length > 0)
          System.Threading.Thread.Sleep(1000);

        //Fin - Valido los errores
        bool WithError = false;
        if (LegajosLoaded.LastException != null) { WithError = true; MyBatch.Err("'LegajosLoaded' finalizo con el error:" + LegajosLoaded.LastException.Id); }
        if (LegajosSaved.LastException != null) { WithError = true; MyBatch.Err("'LegajosSaved' finalizo con el error:" + LegajosSaved.LastException.Id); }
        if (LegajosLiqSaved.LastException != null) { WithError = true; MyBatch.Err("'LegajosLiqSaved' finalizo con el error:" + LegajosLiqSaved.LastException.Id); }
        if (RecibosGen.LastException != null) { WithError = true; MyBatch.Err("'RecibosGen' finalizo con el error:" + RecibosGen.LastException.Id); }
        if (RecibosSaved.LastException != null) { WithError = true; MyBatch.Err("'RecibosSaved' finalizo con el error:" + RecibosSaved.LastException.Id); }
        if (DataCache.LastException != null) { WithError = true; MyBatch.Err("'DataCache' finalizo con el error:" + DataCache.LastException.Id); }

        //Fin
        if (!WithError)
          MyBatch.Log("Liquidación finalizada con éxito.");
      }
      catch (Exception e)
      {
        NomadException ex = NomadException.NewInternalException("RemoteStartLiq", e);
        ex.SetValue("oi_liquidacion", idLiquidacion.ToString());
        ex.SetValue("ois_liquidar", idsLiquidar);
        ex.SetValue("paso", paso);
        MyBatch.Err("Error durante el proceso de Liquidacion. Código de error:" + ex.Id);
        ex.Dump();
        throw ex;
      }
    }

        private LiqUtilBase PrepararLiquidacion(string idsLiquidar)
        {
            //Lista de Personas
            TimeSpan myDif;
            DateTime myNow;

            legCount = idsLiquidar.Split(',').Length;

            //Inicio la Liquidacion
            paso = "Iniciando Liquidacion";
            MyBatch.SetMess("Iniciando Liquidacion (" + legCount + " Legajos) ...");
            MyBatch.Log("Inicio Liquidacion...");
            myNow = DateTime.Now;

            System.Reflection.Assembly MyLIQCode = InitializeLiq();

            myDif = DateTime.Now - myNow;
            NomadLog.Info("InitializeLiq: " + myDif.TotalMilliseconds + "ms");
            if (MyLIQCode == null) return null;
            MyBatch.SetPro(10);

            Type typeClass = MyLIQCode.GetType("NucleusRH.Base.Liquidacion.RHLiq.LiqConceptos");

            //Inicializo la Liquidacion
            LiqUtilBase MyLIQObj = (LiqUtilBase)typeClass.InvokeMember(null, System.Reflection.BindingFlags.CreateInstance, null, null, null);

            MyLIQObj.InitializeLiquidacion(this.MyLiq, this.liquidacion, this.codificadoras, this.conceptos, this.variables, this.empresa);

            this.propiedadesGrupos = GetPropiedadesGrupos();
            this.propiedadesJson = GetPropiedadesJson();

            this.CantidadLegajos = 30;
            //Obtengo la Cantidad de Legajos a Esperar
            try
            {
                NomadLog.Info("Cantidad de Legajos a esperar.");
                this.CantidadLegajos = int.Parse(NomadEnvironment.QueryValue("ORG26_PARAMETROS", "d_valor", "c_parametro", "cant_leg_liq", "", false));
            }
            catch { }

            return MyLIQObj;
        }

        private string ValidacionesIniciales(string idsLiquidar)
        {
            if (idsLiquidar == "")
                return "No hay personas en la liquidacion.";

            switch (this.MyLiq.c_estado)
            {
                case "C": return "La liquidacion esta CERRADA.";
                case "I": return null;
                default: return "La liquidacion no esta INICIALIZADA.";
            }
        }

       /* private static Dictionary<string, PropertyInfo> GetPropiedadesJsonStatic()
        {
            Dictionary<string, PropertyInfo> diccionario = new Dictionary<string, PropertyInfo>();
            for (int i = 1; i < 11; i++)
                diccionario.Add("d_valor_" + i, typeof(Recibos.JSON_VAR).GetProperty("d_valor_" + i));

            return diccionario;
        }*/

        /*private Dictionary<string, PropertyInfo> GetPropiedadesDDO()
        {
            Dictionary<string, PropertyInfo> diccionario = new Dictionary<string, PropertyInfo>();
            for (int i = 1; i < 11; i++)
                diccionario.Add("d_valor_" + i, typeof(LegajoEmpresaDDO.DDO_VAR).GetProperty("d_valor_" + i));

            return diccionario;
        }*/

        private Dictionary<string, PropertyInfo> GetPropiedadesJson()
        {
            Dictionary<string, PropertyInfo> diccionario = new Dictionary<string, PropertyInfo>();
            for (int i = 1; i < 26;i++ )
                diccionario.Add("d_valor_"+i, typeof(Recibos.JSON_VAR).GetProperty("d_valor_"+i));

            return diccionario;
        }

        private Dictionary<string, PropertyInfo> GetPropiedadesGrupos()
        {
            Dictionary<string, PropertyInfo> diccionario = new Dictionary<string, PropertyInfo>();

            foreach (CabeceraDeConceptosDDO.GRUPO_VAR_DDO grupo in this.conceptos.GRUPO_VAR)
            {
                if(!diccionario.ContainsKey(grupo.Columna))
                diccionario.Add(grupo.Columna,typeof(Recibos.VAR_GR_CONC).GetProperty(grupo.Columna));
            }
            return diccionario;
        }

        string xmlHowToLoad_PESONAL_EMP = null;
        private Dictionary<string, NucleusRH.Base.Liquidacion.Legajo_Liquidacion.PERSONAL_EMP> LoadLegajos(string PerIDs, bool RefreshAcum)
        {
          NomadProxy MyProxy = NomadProxy.GetProxy();

          //How To LOAD
          if (xmlHowToLoad_PESONAL_EMP == null)
          {
            using (System.IO.StreamReader MyFile = new System.IO.StreamReader(System.IO.Path.Combine(MyProxy.AppPath, "HowToLoad\\NucleusRH.Base.Liquidacion.Legajo_Liquidacion.PERSONAL_EMP.LOAD.XML"), Encoding.UTF8))
            {
              xmlHowToLoad_PESONAL_EMP = MyFile.ReadToEnd();
              MyFile.Close();
            }

            //NomadLog.Info("xmlHowToLoad_PESONAL_EMP-01:" + xmlHowToLoad_PESONAL_EMP);

            //Genero los IN
            xmlHowToLoad_PESONAL_EMP = xmlHowToLoad_PESONAL_EMP.Replace("t.oi_personal_emp={#PARAM:@id}", "t.oi_personal_emp in ({%= #PARAM:@id %})");
            xmlHowToLoad_PESONAL_EMP = xmlHowToLoad_PESONAL_EMP.Replace("t.id={#PARAM:@id}", "t.id in ({%= #PARAM:@id %})");
            xmlHowToLoad_PESONAL_EMP = xmlHowToLoad_PESONAL_EMP.Replace("t.oi_personal_emp={#PARAM:@VALUE}", "t.oi_personal_emp in ({%= #PARAM:@VALUE %})");
            xmlHowToLoad_PESONAL_EMP = xmlHowToLoad_PESONAL_EMP.Replace("oi_personal_emp={#PARAM:@oi_personal_emp}", "oi_personal_emp in ({%= #PARAM:@VALUE %})");
            //NomadLog.Info("xmlHowToLoad_PESONAL_EMP-02:" + xmlHowToLoad_PESONAL_EMP);

            //Quito el contenedor
            xmlHowToLoad_PESONAL_EMP = xmlHowToLoad_PESONAL_EMP.Substring(xmlHowToLoad_PESONAL_EMP.IndexOf(">")+1);
            xmlHowToLoad_PESONAL_EMP = xmlHowToLoad_PESONAL_EMP.Substring(0, xmlHowToLoad_PESONAL_EMP.LastIndexOf("<"));
            //NomadLog.Info("xmlHowToLoad_PESONAL_EMP-03:" + xmlHowToLoad_PESONAL_EMP);

            //Modifico el SECURITY
            int io = xmlHowToLoad_PESONAL_EMP.LastIndexOf("<qry:out>");
            xmlHowToLoad_PESONAL_EMP = xmlHowToLoad_PESONAL_EMP.Substring(0, io) + "<qry:out><SECURITY><qry:attribute value=\"$r/@id\" name=\"id\"/>" + xmlHowToLoad_PESONAL_EMP.Substring(io+9);
            io = xmlHowToLoad_PESONAL_EMP.LastIndexOf("</qry:out>");
            xmlHowToLoad_PESONAL_EMP = xmlHowToLoad_PESONAL_EMP.Substring(0, io) + "</SECURITY></qry:out>" + xmlHowToLoad_PESONAL_EMP.Substring(io+10);
            xmlHowToLoad_PESONAL_EMP = xmlHowToLoad_PESONAL_EMP.Replace("select t.version", "select t.id, t.version");
            //NomadLog.Info("xmlHowToLoad_PESONAL_EMP-04:" + xmlHowToLoad_PESONAL_EMP);

            //Quito insertselect superfluos
            xmlHowToLoad_PESONAL_EMP = xmlHowToLoad_PESONAL_EMP.Replace("<qry:insert-select doc-path=\".\" name=\"FIND-PER-DESCR\"/>", "");
            xmlHowToLoad_PESONAL_EMP = xmlHowToLoad_PESONAL_EMP.Replace("<qry:insert-select doc-path=\"$r\" name=\"sql2\"/>", "");
            //NomadLog.Info("xmlHowToLoad_PESONAL_EMP-05:" + xmlHowToLoad_PESONAL_EMP);

            //Busco SQL2
            int ioSQL2 = xmlHowToLoad_PESONAL_EMP.IndexOf("\"sql2\"");
            io = xmlHowToLoad_PESONAL_EMP.IndexOf("<qry:out>", ioSQL2);
            xmlHowToLoad_PESONAL_EMP = xmlHowToLoad_PESONAL_EMP.Substring(0, io) + "<qry:out><LIQ_PERSONAL>" + xmlHowToLoad_PESONAL_EMP.Substring(io+9);
            io = xmlHowToLoad_PESONAL_EMP.IndexOf("</qry:out>", ioSQL2);
            xmlHowToLoad_PESONAL_EMP = xmlHowToLoad_PESONAL_EMP.Substring(0, io) + "</LIQ_PERSONAL></qry:out>" + xmlHowToLoad_PESONAL_EMP.Substring(io+10);
            //NomadLog.Info("xmlHowToLoad_PESONAL_EMP-06:" + xmlHowToLoad_PESONAL_EMP);

            //Llamada al SQL2
            io = xmlHowToLoad_PESONAL_EMP.IndexOf("</object>");
            xmlHowToLoad_PESONAL_EMP = xmlHowToLoad_PESONAL_EMP.Substring(0, io) + "<qry:insert-select doc-path=\"$PARAMIN\" name=\"sql2\"/>" + xmlHowToLoad_PESONAL_EMP.Substring(io);
            NomadLog.Info("xmlHowToLoad_PESONAL_EMP-07:" + xmlHowToLoad_PESONAL_EMP);
          }

          //Lista de ids
          string[] IDS = PerIDs.Split(',');
          string[] Childs = "VAL_VAREA,VAL_VARPA,CARGOS,VARPA_CARGO".Split(',');

          //Cargo los Raices
          Dictionary<string, NomadXML> XMLs = new Dictionary<string, NomadXML>();

          NomadXML RESULT = MyProxy.SQLService().GetXML(xmlHowToLoad_PESONAL_EMP, "<ID VALUE=\"" + PerIDs + "\" child=\"\" load-all=\"1\" load-child=\"0\" />");
          if (RESULT.isDocument) RESULT = RESULT.FirstChild();

          //Crea los tags raices
          for(NomadXML MyCUR = RESULT.FirstChild(); MyCUR!=null; MyCUR=MyCUR.Next())
          {
            if (MyCUR.Name == "PERSONAL_EMP")
            {
              string ID = MyCUR.GetAttr("id");

              NomadXML MyDDO = new NomadXML("object");
              MyDDO.SetAttr("id", ID);
              MyDDO.SetAttr("class", "NucleusRH.Base.Liquidacion.Legajo_Liquidacion.PERSONAL_EMP");
              MyDDO.SetAttr("nmd-R", "1");
              MyDDO.SetAttr("nmd-C", "0");
              MyDDO.SetAttr("nmd-A", "1");
              MyDDO.AddText(MyCUR.ToString());

              XMLs[ID] = MyDDO;
              if (RefreshAcum)
              {
                NomadXML MyPER = MyDDO.FirstChild();
                foreach(string CHILD in Childs)
                {
                  XMLs[ID+"-"+CHILD] = MyPER.FindElement(CHILD);
                  XMLs[ID+"-"+CHILD].SetAttr("nmd-ld", "1");
                }
              }
            }
          }

          //Pega los LIQ_PERSONAL
          for(NomadXML MyCUR = RESULT.FirstChild(); MyCUR!=null; MyCUR=MyCUR.Next())
          {
            if (MyCUR.Name == "LIQ_PERSONAL")
            {
              NomadXML MyDDO = XMLs[MyCUR.GetAttr("ID-LIQ07_PERSONAL")];
              NomadXML MyPER = MyDDO.FirstChild();

              //Pego los attributos
              foreach(string AttrName in MyCUR.Attrs)
                MyPER.SetAttr(AttrName, MyCUR.GetAttr(AttrName));

              //Pego los elementos
              for(NomadXML MyCUR2 = MyCUR.FirstChild(); MyCUR2!=null; MyCUR2=MyCUR2.Next())
                MyPER.FindElement(MyCUR2.Name).SetAttr("value", MyCUR2.GetAttr("value"));

            }
          }

          //Pega los security
          for(NomadXML MyCUR = RESULT.FirstChild(); MyCUR!=null; MyCUR=MyCUR.Next())
          {
            if (MyCUR.Name == "SECURITY")
            {
              NomadXML MyDDO = XMLs[MyCUR.GetAttr("id")];
              MyDDO.SetAttr("version", MyCUR.GetAttr("version"));
              MyDDO.SetAttr("policy", MyCUR.GetAttr("policy"));
              MyDDO.SetAttr("new_user", MyCUR.GetAttr("new_user"));
              MyDDO.SetAttr("new_time", MyCUR.GetAttr("new_time"));
              MyDDO.SetAttr("edit_user", MyCUR.GetAttr("edit_user"));
              MyDDO.SetAttr("edit_time", MyCUR.GetAttr("edit_time"));
              MyDDO.SetAttr("del_user", MyCUR.GetAttr("del_user"));
              MyDDO.SetAttr("del_time", MyCUR.GetAttr("del_time"));
            }
          }

          //Cargo los childs masivamente!!!!
          if (RefreshAcum)
          {
            foreach(string CHILD in Childs)
            {
              RESULT = MyProxy.SQLService().GetXML(xmlHowToLoad_PESONAL_EMP, "<ID VALUE=\"" + PerIDs + "\" child=\"" + CHILD + "\" load-all=\"1\" load-child=\"0\" />");
              if (RESULT.isDocument) RESULT = RESULT.FirstChild();

              for(NomadXML MyCUR = RESULT.FirstChild(); MyCUR!=null; MyCUR=MyCUR.Next())
              {
                string ID = MyCUR.GetAttr("oi_personal_emp");
                XMLs[ID+"-"+CHILD].AddText(MyCUR.ToString());
              }
            }
          }

          //Resultado
          Dictionary<string, NucleusRH.Base.Liquidacion.Legajo_Liquidacion.PERSONAL_EMP> retval = new Dictionary<string, NucleusRH.Base.Liquidacion.Legajo_Liquidacion.PERSONAL_EMP>();
          foreach(string ID in IDS)
          {
            NucleusRH.Base.Liquidacion.Legajo_Liquidacion.PERSONAL_EMP PE = new NucleusRH.Base.Liquidacion.Legajo_Liquidacion.PERSONAL_EMP(false);
            PE.Load(XMLs[ID]);
            retval[ID] = PE;
          }

          return retval;
        }

        string xmlHowToLoad_TOT_LIQ_PER = null;

        class RecData {
          public NucleusRH.Base.Liquidacion.Recibos.TOT_LIQ_PER REC;
          public List<Recibos.ANT_LIQ_PER> ANT_LIQ_PER;
          public List<Recibos.DANT_LIQ_PER> DANT_LIQ_PER;
          public List<Recibos.EMB_LIQ_PER> EMB_LIQ_PER;
          public List<Recibos.CON_LIQ_PER> CON_LIQ_PER;
          public List<Recibos.VARPA_CARGO> VARPA_CARGO;

          public Dictionary<string, double> VAREA_TOT;
          public Dictionary<string, double> VARPA_ANNO;

          public Dictionary<string, NomadXML> QRY_ACUM;
          public Dictionary<string, NomadXML> QRY_PER_ACUM;
        };

        private Dictionary<string, RecData> LoadRecibos(string RecIDs, bool LoadACUMN)
        {
          NomadProxy MyProxy = NomadProxy.GetProxy();

          //How To LOAD
          if (xmlHowToLoad_TOT_LIQ_PER == null)
          {
            using (System.IO.StreamReader MyFile = new System.IO.StreamReader(System.IO.Path.Combine(MyProxy.AppPath, "HowToLoad\\NucleusRH.Base.Liquidacion.Recibos.TOT_LIQ_PER.LOAD.XML"), Encoding.UTF8))
            {
              xmlHowToLoad_TOT_LIQ_PER = MyFile.ReadToEnd();
              MyFile.Close();
            }
            xmlHowToLoad_TOT_LIQ_PER = xmlHowToLoad_TOT_LIQ_PER.Replace("t.oi_tot_liq_per={#PARAM:@id}", "t.oi_tot_liq_per in ({%= #PARAM:@id %})");
            xmlHowToLoad_TOT_LIQ_PER = xmlHowToLoad_TOT_LIQ_PER.Replace("t.oi_tot_liq_per={#PARAM:@VALUE}", "t.oi_tot_liq_per in ({%= #PARAM:@VALUE %})");
            xmlHowToLoad_TOT_LIQ_PER = xmlHowToLoad_TOT_LIQ_PER.Substring(xmlHowToLoad_TOT_LIQ_PER.IndexOf(">")+1);
            xmlHowToLoad_TOT_LIQ_PER = xmlHowToLoad_TOT_LIQ_PER.Substring(0, xmlHowToLoad_TOT_LIQ_PER.LastIndexOf("<"));
            NomadLog.Info("xmlHowToLoad_TOT_LIQ_PER:" + xmlHowToLoad_TOT_LIQ_PER);
          }

          string[] IDS = RecIDs.Split(',');
          string MyPERDDOIds = "";
          Dictionary<string, string> IDPer2IDRec = new Dictionary<string, string>();

          //Cargo los Raices
          Dictionary<string, RecData> retval = new Dictionary<string, RecData>();

          //Cargo los raices masivamente!!!!
          NomadXML RESULT = MyProxy.SQLService().GetXML(xmlHowToLoad_TOT_LIQ_PER, "<ID VALUE=\"" + RecIDs+ "\" child=\"\" load-all=\"1\" load-child=\"0\" />");
          if (RESULT.isDocument) RESULT = RESULT.FirstChild();

          for(NomadXML MyCUR = RESULT.FirstChild(); MyCUR!=null; MyCUR=MyCUR.Next())
          {
            string ID = MyCUR.GetAttr("id");

            NomadXML MyDDO = new NomadXML("object");
            MyDDO.SetAttr("id", ID);
            MyDDO.SetAttr("class", "NucleusRH.Base.Liquidacion.Recibos.TOT_LIQ_PER");
            MyDDO.SetAttr("nmd-R", "1");
            MyDDO.SetAttr("nmd-C", "0");
            MyDDO.SetAttr("nmd-A", "1");
            MyDDO.AddText(MyCUR.ToString());

            //Cargo el DDO
            NucleusRH.Base.Liquidacion.Recibos.TOT_LIQ_PER RE = new NucleusRH.Base.Liquidacion.Recibos.TOT_LIQ_PER(false);
            RE.Load(MyDDO);

            //Lista de ID PERSONA RECIBO
            MyPERDDOIds += "," + RE.oi_per_emp_ddo;
            IDPer2IDRec[RE.oi_per_emp_ddo] = ID;

            //Objeto final
            RecData MyRecdata = new RecData();
            MyRecdata.REC = RE;
            MyRecdata.ANT_LIQ_PER = new List<Recibos.ANT_LIQ_PER>();
            MyRecdata.DANT_LIQ_PER = new List<Recibos.DANT_LIQ_PER>();
            MyRecdata.EMB_LIQ_PER = new List<Recibos.EMB_LIQ_PER>();
            MyRecdata.CON_LIQ_PER = new List<Recibos.CON_LIQ_PER>();
            MyRecdata.VARPA_CARGO = new List<Recibos.VARPA_CARGO>();
            MyRecdata.VAREA_TOT = null;
            MyRecdata.VARPA_ANNO = null;
            MyRecdata.QRY_ACUM = new Dictionary<string, NomadXML>();
            MyRecdata.QRY_PER_ACUM  = new Dictionary<string, NomadXML>();

            //Lo agrego al diccionario
            retval[ID] = MyRecdata;
          }

          //Cargo anticipos masivamente!!!!
          RESULT = MyProxy.SQLService().GetXML(xmlHowToLoad_TOT_LIQ_PER, "<ID VALUE=\"" + RecIDs+ "\" child=\"ANT_LIQ_PER\" load-all=\"1\" load-child=\"0\" />");
          if (RESULT.isDocument) RESULT = RESULT.FirstChild();
          for(NomadXML MyCUR = RESULT.FirstChild(); MyCUR!=null; MyCUR=MyCUR.Next())
            retval[MyCUR.GetAttr("oi_tot_liq_per")].ANT_LIQ_PER.Add(new Recibos.ANT_LIQ_PER(MyCUR));

          //Cargo descuentos anticipos masivamente!!!!
          RESULT = MyProxy.SQLService().GetXML(xmlHowToLoad_TOT_LIQ_PER, "<ID VALUE=\"" + RecIDs+ "\" child=\"DANT_LIQ_PER\" load-all=\"1\" load-child=\"0\" />");
          if (RESULT.isDocument) RESULT = RESULT.FirstChild();
          for(NomadXML MyCUR = RESULT.FirstChild(); MyCUR!=null; MyCUR=MyCUR.Next())
            retval[MyCUR.GetAttr("oi_tot_liq_per")].DANT_LIQ_PER.Add(new Recibos.DANT_LIQ_PER(MyCUR));

          //Cargo embargos masivamente!!!!
          RESULT = MyProxy.SQLService().GetXML(xmlHowToLoad_TOT_LIQ_PER, "<ID VALUE=\"" + RecIDs+ "\" child=\"EMB_LIQ_PER\" load-all=\"1\" load-child=\"0\" />");
          if (RESULT.isDocument) RESULT = RESULT.FirstChild();
          for(NomadXML MyCUR = RESULT.FirstChild(); MyCUR!=null; MyCUR=MyCUR.Next())
            retval[MyCUR.GetAttr("oi_tot_liq_per")].EMB_LIQ_PER.Add(new Recibos.EMB_LIQ_PER(MyCUR));

          //Cargo consumos masivamente!!!!
          RESULT = MyProxy.SQLService().GetXML(xmlHowToLoad_TOT_LIQ_PER, "<ID VALUE=\"" + RecIDs+ "\" child=\"CON_LIQ_PER\" load-all=\"1\" load-child=\"0\" />");
          if (RESULT.isDocument) RESULT = RESULT.FirstChild();
          for(NomadXML MyCUR = RESULT.FirstChild(); MyCUR!=null; MyCUR=MyCUR.Next())
            retval[MyCUR.GetAttr("oi_tot_liq_per")].CON_LIQ_PER.Add(new Recibos.CON_LIQ_PER(MyCUR));

          //Cargo consumos masivamente!!!!
          RESULT = MyProxy.SQLService().GetXML(xmlHowToLoad_TOT_LIQ_PER, "<ID VALUE=\"" + RecIDs+ "\" child=\"VARPA_CARGO\" load-all=\"1\" load-child=\"0\" />");
          if (RESULT.isDocument) RESULT = RESULT.FirstChild();
          for(NomadXML MyCUR = RESULT.FirstChild(); MyCUR!=null; MyCUR=MyCUR.Next())
            retval[MyCUR.GetAttr("oi_tot_liq_per")].VARPA_CARGO.Add(new Recibos.VARPA_CARGO(MyCUR));

          //Cargo los JSON masivamente!!!!
          RESULT = MyProxy.SQLService().GetXML(xmlHowToLoad_TOT_LIQ_PER, "<ID VALUE=\"" + RecIDs+ "\" child=\"JSON_VAR\" load-all=\"1\" load-child=\"0\" />");
          if (RESULT.isDocument) RESULT = RESULT.FirstChild();
          for(NomadXML MyCUR = RESULT.FirstChild(); MyCUR!=null; MyCUR=MyCUR.Next())
          {
            //Obtener el JSON
            string jsonCompleto = "";
            for (int i = 1; i < 26; i++)
            {
                string subJson = MyCUR.GetAttr("d_valor_" + i);
                if (subJson != null) jsonCompleto += subJson;
            }

            //Convertir a Diccionario
            if (MyCUR.GetAttr("c_tipo") == "VAREA_TOT")
              retval[MyCUR.GetAttr("oi_tot_liq_per")].VAREA_TOT = StringUtil.JSON2Object<Dictionary<string,double>>(jsonCompleto);
            else if (MyCUR.GetAttr("c_tipo") == "VARPA_ANNO")
              retval[MyCUR.GetAttr("oi_tot_liq_per")].VARPA_ANNO = StringUtil.JSON2Object<Dictionary<string,double>>(jsonCompleto);
          }

          if (LoadACUMN)
          {
            //QUERY DE ACUMULADORES
            NomadXML MySQL = proxy.SQLService().GetXML("CLASS.NucleusRH.Base.Liquidacion.LiquidacionDDO.LIQUIDACION_DDO.QRY_ACUM", "<PARAM oi_per_emp_ddo=\"" + MyPERDDOIds.Substring(1) + "\" />");
            for(NomadXML MyCUR = MySQL.FirstChild(); MyCUR!=null; MyCUR=MyCUR.Next())
            {
              string ID = IDPer2IDRec[MyCUR.GetAttr("oi_per_emp_ddo")];
              retval[ID].QRY_ACUM[MyCUR.GetAttr("c_variable")] = MyCUR;
            }

            //QUERY DE ACUMULADORES
            MySQL = proxy.SQLService().GetXML("CLASS.NucleusRH.Base.Liquidacion.LiquidacionDDO.LIQUIDACION_DDO.QRY_PER_ACUM", "<PARAM oi_per_emp_ddo=\"" + MyPERDDOIds.Substring(1) + "\" />");
            for(NomadXML MyCUR = MySQL.FirstChild(); MyCUR!=null; MyCUR=MyCUR.Next())
            {
              string ID = IDPer2IDRec[MyCUR.GetAttr("oi_per_emp_ddo")];
              retval[ID].QRY_PER_ACUM[MyCUR.GetAttr("c_variable")] = MyCUR;
            }
          }

          //Resultado
          return retval;
        }

        public void CloseLiq(int idLiquidacion, bool deletePerNoLiq, bool compPerChange, bool compConcChange, bool compEmpChange, bool CerrarLiquidacion, bool ActAcumuladores)
        {
            NomadBatch MyBatch = NomadBatch.GetBatch((CerrarLiquidacion ? "Cerrar Liquidacion" : "Verificar Liquidacion"), (CerrarLiquidacion ? "Cerrar Liquidacion" : "Verificar Liquidacion"));
            MyBatch.Log("Cerrando la Liquidacion...");
            NomadLog.Info("Cerrando liquidacion V1....");

            int t = 0;
            int RecibosCerrados = 0;
            int LegajosSinRecibos = 0;
            int RecibosNoCerrados = 0;
            DateTime fcierre = DateTime.Now;
            DateTime myNow, myNow2;

      int cantLegajos = 0;
      bool paramLegCloseLiq = int.TryParse(NomadEnvironment.QueryValue("ORG26_PARAMETROS", "d_valor", "c_parametro", "cant_leg_cierre_liq", "", false), out cantLegajos);
      if(!paramLegCloseLiq || cantLegajos == 0)
        cantLegajos = 100;
      NomadLog.Info("Paginando de a  '" + cantLegajos.ToString() + "' legajos.");

            /////////////////////////////////////////////////////////////////////////////////////////////
            // COMPROBAR EL ESTADO DE LA LIQUIDACION
            NomadLog.Info("Comprobando Estado..."); myNow = DateTime.Now;
            MyBatch.SetMess("Comprobando Estado...");
            this.MyLiq = Liquidacion.LIQUIDACION.Get(idLiquidacion, false);
            switch (this.MyLiq.c_estado)
            {
                case "C":
                    MyBatch.Err("La liquidacion esta CERRADA.");
                    return;

                case "I":
                    break;

                default:
                    MyBatch.Err("La liquidacion no esta INICIALIZADA.");
                    return;
            }
            MyBatch.SetPro(2);

            bool liqInterfaceAFIP = false;
            string c_tipo_liq = MyLiq.Getoi_tipo_liq().c_tipo_liq;

            if (this.MyLiq.oi_tipo_liq == "7")
            {
                liqInterfaceAFIP = true;
                if (!LiquidacionOrigenCerrada())
                {
                    MyBatch.Err("La liquidacion de origen no esta cerrada, cierrela e intente nuevamente");
                    return;
                }

                if (!LiquidacionLSDProcesada())
                {
                    MyBatch.Err("La liquidacion no fue procesada desde el cierre de la liquidacion que le dio origen, procesela e intente el cierre nuevamente");
                    return;
                }
                if (!OrdenCorrectoDeCierre())
                {
                    MyBatch.Err("La liquidacion no puede ser cerrada, el orden de cierre de liquidacion LSD no es correcto");
                    return;
                }
            }
            NomadLog.Info("Comprobando Estado: " + (DateTime.Now - myNow).TotalMilliseconds + "ms");

            /////////////////////////////////////////////////////////////////////////////////////////////
            // CARGANDO LA INFORMACION NECESARIO PARA LA LIQUIDACION
            NomadLog.Info("Obteniendo Informacion Recibos..."); myNow = DateTime.Now;
            MyBatch.SetMess("Obteniendo Informacion Recibos...");
            bool bCerrarLiquidacion = true;
            string Params, strDDO, strHash;
            NomadXML MyCUR, MyCUR2, MyCUR3, MyPARAM, MySQL;
            NomadXML XMLLiquidacion=NomadProxy.GetProxy().SQLService().GetXML("CLASS.NucleusRH.Base.Liquidacion.LiquidacionDDO.LIQUIDACION_DDO.QueryRecibosPersonas", "<FILTRO oi_liquidacion=\""+idLiquidacion+"\" />");
            NomadXML XMLRecibos = XMLLiquidacion.FindElement("RECIBOS");
            NomadXML XMLPersonas = XMLLiquidacion.FindElement("PERSONAS");
            NomadXML XMLEjecuciones = XMLLiquidacion.FindElement("EJECUCIONES");
            NomadXML XMLVariables = XMLLiquidacion.FindElement("VARIABLES");

            //Cargo el diccionario de variables
            Dictionary<string, string> DIC_CVar_2_OIVar = new Dictionary<string, string>();
            Dictionary<string, NomadXML> DIC_OIVar_2_XML = new Dictionary<string, NomadXML>();
            for (MyCUR = XMLVariables.FirstChild(); MyCUR != null; MyCUR = MyCUR.Next())
            {
              DIC_CVar_2_OIVar[MyCUR.GetAttr("c_variable")]=MyCUR.GetAttr("oi_variable");
              DIC_OIVar_2_XML[MyCUR.GetAttr("oi_variable")]=MyCUR;
            }

            NomadLog.Info("RECIBOS:" + XMLRecibos.ToString());
            NomadLog.Info("PERSONAS:" + XMLPersonas.ToString());
            MyBatch.SetPro(4);
            NomadLog.Info("Obteniendo Informacion Recibos: " + (DateTime.Now - myNow).TotalMilliseconds + "ms");

            /////////////////////////////////////////////////////////////////////////////////////////////
            // Eliminando Recibos de las personas eliminanas de la liquidacion
            NomadLog.Info("Eliminando Recibos de liquidacion..."); myNow = DateTime.Now;
            MyBatch.SetMess("Eliminando Recibos de liquidacion...");
            NucleusRH.Base.Liquidacion.LiquidacionDDO.LIQUIDACION_DDO myliqddo = NucleusRH.Base.Liquidacion.LiquidacionDDO.LIQUIDACION_DDO.Get(XMLLiquidacion.GetAttr("oi_liquidacion_ddo"), false);
            bool saveMethod = false;
            for (MyCUR = XMLRecibos.FirstChild(); MyCUR != null; MyCUR = MyCUR.Next())
            {
                MyCUR2 = XMLPersonas.FindElement2("ROW", "e_numero_legajo", MyCUR.GetAttr("e_numero_legajo"));
                if (MyCUR2 == null)
                {
                    MyCUR.SetAttr("del", "1");
                    if (MyCUR.GetAttr("f_cierre") == "")
                    {
                        MyBatch.Log("Eliminando el Recibo del Legajo " + MyCUR.GetAttr("e_numero_legajo") + "-" + MyCUR.GetAttr("d_ape_y_nom"));
                        myliqddo.Recibos.RemoveById(MyCUR.GetAttr("ID"));
                        saveMethod = true;
                    }
                    else
                    {
                        MyBatch.Err("Inconsistencia en los Datos, no se encuentra la persona " + MyCUR.GetAttr("e_numero_legajo") + "-" + MyCUR.GetAttr("d_ape_y_nom") + " pero el recibo existe y esta cerrado.");
                    }
                }
            }
            if (saveMethod)
            {
                try
                {
                    NomadEnvironment.GetCurrentTransaction().Save(myliqddo);
                }
                catch (Exception Ex)
                {
                    MyBatch.Err("No se pudieron eliminar los Recibos de la Liquidacion.");
                    NomadLog.Error("ERROR", Ex);
                    return;
                }
            }
            do
            {
                MyCUR = XMLRecibos.FindElement2("ROW", "del", "1");
                if (MyCUR != null) XMLRecibos.DeleteChild(MyCUR);
            } while (MyCUR != null);
            MyBatch.SetPro(7);
            NomadLog.Info("Eliminando Recibos de liquidacion: " + (DateTime.Now - myNow).TotalMilliseconds + "ms");

            /////////////////////////////////////////////////////////////////////////////////////////////
            // COMPROBAR LEGAJOS NO LIQUIDADOS
            NomadLog.Info("Comprobar Legajos no liquidados..."); myNow = DateTime.Now;
            MyBatch.SetMess(deletePerNoLiq ? "Eliminando Legajos no liquidados..." : "Comprobar Legajos no liquidados...");
            bool exitMethod = false;
            for (MyCUR = XMLPersonas.FirstChild(); MyCUR != null; MyCUR = MyCUR.Next())
            {
                MyCUR2 = XMLRecibos.FindElement2("ROW", "e_numero_legajo", MyCUR.GetAttr("e_numero_legajo"));
                if (MyCUR2 == null)
                {
                    if (deletePerNoLiq)
                    {
                        MyBatch.Wrn("El legajo " + MyCUR.GetAttr("e_numero_legajo") + "-" + MyCUR.GetAttr("d_ape_y_nom") + " no tiene recibo generado - Eliminado.");
                        try
                        {
                            MyCUR.SetAttr("del", "1");
                            NucleusRH.Base.Liquidacion.Personal_Liquidacion.PERSONAL_LIQ MyPER = NucleusRH.Base.Liquidacion.Personal_Liquidacion.PERSONAL_LIQ.Get(MyCUR.GetAttr("ID"));
                            NomadEnvironment.GetCurrentTransaction().Delete(MyPER);
                        }
                        catch (Exception Ex)
                        {
                            MyBatch.Err("El legajo " + MyCUR.GetAttr("e_numero_legajo") + "-" + MyCUR.GetAttr("d_ape_y_nom") + " no se pudo Eliminar.");
                            NomadLog.Error("ERROR", Ex);
                            MyCUR.SetAttr("del", "1");
                            bCerrarLiquidacion = false;
                            LegajosSinRecibos++;
                        }
                    }
                    else
                    {
                        MyBatch.Wrn("El legajo " + MyCUR.GetAttr("e_numero_legajo") + "-" + MyCUR.GetAttr("d_ape_y_nom") + " no tiene recibo generado.");
                        MyCUR.SetAttr("del", "1");
                        bCerrarLiquidacion = false;
                        LegajosSinRecibos++;
                    }
                }
                else
                    if (MyCUR2.GetAttr("f_cierre") != "")
                    {
                        MyCUR.SetAttr("del", "1");
                        MyBatch.Log("El legajo " + MyCUR.GetAttr("e_numero_legajo") + "-" + MyCUR.GetAttr("d_ape_y_nom") + " tiene el recibo CERRADO.");
                        RecibosCerrados++;
                    }
            }
            if (exitMethod) return;
            do
            {
                MyCUR = XMLPersonas.FindElement2("ROW", "del", "1");
                if (MyCUR != null) XMLPersonas.DeleteChild(MyCUR);
            } while (MyCUR != null);
            MyBatch.SetPro(10);
            NomadLog.Info("Comprobar Legajos no liquidados: " + (DateTime.Now - myNow).TotalMilliseconds + "ms");

            //Variables Basicas
            string oi_cab_conc_ddo = myliqddo.IDCabConceptos;
            string oi_empresa_ddo = myliqddo.IDEmpresa;
            string oi_var_ejec_ddo = myliqddo.IDCabVariables;
            string oi_codif_ddo = myliqddo.IDCabCodificadoras;
            string oi_liquidacion = this.MyLiq.Id;
            int e_periodo = this.MyLiq.e_periodo;

            if (!compConcChange)
            {
                MyBatch.Wrn("No se comprobaran los cambios en las Conceptos/Variables/Codificadoras.");
            }
            else
            {
                /////////////////////////////////////////////////////////////////////////////////////////////
                // COMPROBAR EL HASH DE LAS VARIABLES
                NomadLog.Info("Comprobando VARIABLES..."); myNow = DateTime.Now;
                MyBatch.SetMess("Comprobando VARIABLES...");
                MyBatch.SetPro(10);

                Params = "<FILTRO oi_empresa=\"" + this.MyLiq.oi_empresa + "\" oi_tipo_liq=\"" + this.MyLiq.oi_tipo_liq + "\" />";
                NomadLog.Info("Ejecutar Query: NucleusRH.Base.Liquidacion.VariablesPorEjecucionDDO.VAR_EJEC_DDO.CargarVariablesALiquidacion - PARAM:" + Params);
                strDDO=NomadEnvironment.QueryString("CLASS.NucleusRH.Base.Liquidacion.VariablesPorEjecucionDDO.VAR_EJEC_DDO.CargarVariablesALiquidacion", Params);
                strHash = MD5(strDDO);
                if (strHash != NomadEnvironment.QueryValue("LIQ95_VAR_EJEC", "h_hash", "oi_var_ejec_ddo", oi_var_ejec_ddo, "", false))
                {
                    MyBatch.Err("Cambiaron las Definicion de Varibles despues de la ultima ejecución.");
                    exitMethod = true;
                }
                if (exitMethod) return;
                MyBatch.Log("Variables OK.");
                NomadLog.Info("Comprobando VARIABLES: " + (DateTime.Now - myNow).TotalMilliseconds + "ms");

                /////////////////////////////////////////////////////////////////////////////////////////////
                // COMPROBAR EL HASH DE LAS CODIFICADORAS
                NomadLog.Info("Comprobando CODIFICADORAS..."); myNow = DateTime.Now;
                MyBatch.SetMess("Comprobando CODIFICADORAS...");
                MyBatch.SetPro(12);

                Params = "<FILTRO e_anio=\"" + this.MyLiq.f_pago.Year.ToString() + "\" />";
                NomadLog.Info("Ejecutar Query: NucleusRH.Base.Liquidacion.CabeceraDeCodificadoraDDO.CODIF_DDO.GuardarCodificadoras: " + Params);
                strDDO=NomadEnvironment.QueryString("CLASS.NucleusRH.Base.Liquidacion.CabeceraDeCodificadoraDDO.CODIF_DDO.GuardarCodificadoras", Params);
                strHash = MD5(strDDO);
                if (strHash != NomadEnvironment.QueryValue("LIQ94_CODIF", "h_hash", "oi_codif_ddo", oi_codif_ddo, "", false))
                {
                    MyBatch.Err("Cambiaron las Definicion de Codificadoras despues de la ultima ejecución.");
                    exitMethod = true;
                }
                if (exitMethod) return;
                MyBatch.Log("Codificadoras OK.");
                NomadLog.Info("Comprobando CODIFICADORAS: " + (DateTime.Now - myNow).TotalMilliseconds + "ms");

                /////////////////////////////////////////////////////////////////////////////////////////////
                // COMPROBAR EL HASH DE LAS CONCEPTOS
                MyBatch.SetMess("Comprobando CONCEPTOS..."); myNow = DateTime.Now;
                MyBatch.SetPro(14);

                Params = "<FILTRO oi_empresa=\"" + this.MyLiq.oi_empresa + "\" oi_var_ejec_ddo=\"" + oi_var_ejec_ddo + "\" oi_tipo_liq=\"" + this.MyLiq.oi_tipo_liq + "\" />";
                NomadLog.Info("Ejecutar Query: NucleusRH.Base.Liquidacion.CabeceraDeConceptosDDO.CAB_CONC_DDO.QryConceptos " + Params);
                strDDO=NomadEnvironment.QueryString("CLASS.NucleusRH.Base.Liquidacion.CabeceraDeConceptosDDO.CAB_CONC_DDO.QryConceptos", Params);
                strHash = MD5(strDDO);
                if (strHash != NomadEnvironment.QueryValue("LIQ96_CAB_CONCEPT", "h_hash", "oi_cab_conc_ddo", oi_cab_conc_ddo, "", false))
                {
                    MyBatch.Err("Cambiaron las Definicion de Conceptos despues de la ultima ejecución.");
                    exitMethod = true;
                }
                if (exitMethod) return;
                MyBatch.Log("Conceptos OK.");
                NomadLog.Info("Comprobando CONCEPTOS: " + (DateTime.Now - myNow).TotalMilliseconds + "ms");
            }
            MyBatch.SetPro(16);

            if (!compEmpChange)
            {
              MyBatch.Wrn("No se comprobaran los cambios en la Empresa/Liquidacion.");
            }
            else
            {
                /////////////////////////////////////////////////////////////////////////////////////////////
                // COMPROBAR EL HASH DE LAS EMPRESA
                NomadLog.Info("Comprobando EMPRESA..."); myNow = DateTime.Now;
                MyBatch.SetMess("Comprobando EMPRESA...");
                MyBatch.SetPro(16);

                Params = "<FILTRO oi_empresa=\"" + this.MyLiq.oi_empresa + "\" oi_var_ejec_ddo=\"" + oi_var_ejec_ddo + "\" oi_codif_ddo=\"" + oi_codif_ddo + "\" />";
                NomadLog.Info("Ejecutar Query: NucleusRH.Base.Liquidacion.EmpresasDDO.EMPRESA_DDO.QueryEmpresaDDO " + Params);
                strDDO=NomadEnvironment.QueryString("CLASS.NucleusRH.Base.Liquidacion.EmpresasDDO.EMPRESA_DDO.QueryEmpresaDDO", Params);
                strHash = MD5(strDDO);
                if (strHash != NomadEnvironment.QueryValue("LIQ93_EMPRESAS", "h_hash", "oi_empresa_ddo", oi_empresa_ddo, "", false))
                {
                    MyBatch.Err("Cambiaron los Datos de Empresa despues de la ultima ejecución.");
                    exitMethod = true;
                }
                if (exitMethod) return;
                MyBatch.Log("Empresa OK.");
                NomadLog.Info("Comprobando EMPRESA: " + (DateTime.Now - myNow).TotalMilliseconds + "ms");

                /////////////////////////////////////////////////////////////////////////////////////////////
                // COMPROBAR EL HASH DE LAS LIQUIDACION
                MyBatch.SetMess("Comprobando LIQUIDACION..."); myNow = DateTime.Now;
                MyBatch.SetPro(18);

                Params = "<FILTRO oi_empresa_ddo=\"" + oi_empresa_ddo + "\" oi_var_ejec_ddo=\"" + oi_var_ejec_ddo + "\" oi_cab_conc_ddo=\"" + oi_cab_conc_ddo + "\" oi_codif_ddo=\"" + oi_codif_ddo + "\" oi_liquidacion=\"" + this.MyLiq.Id + "\"/>";
                NomadLog.Info("Ejecutar Query: NucleusRH.Base.Liquidacion.LiquidacionDDO.LIQUIDACION_DDO.QueryLiquidacion " + Params);
                strDDO=NomadEnvironment.QueryString("CLASS.NucleusRH.Base.Liquidacion.LiquidacionDDO.LIQUIDACION_DDO.QueryLiquidacion", Params);
                strHash = MD5(strDDO);
                if (strHash != NomadEnvironment.QueryValue("LIQ99_LIQUIDACION", "h_hash", "oi_liquidacion_ddo", myliqddo.Id, "", false))
                {
                    MyBatch.Err("Cambiaron los Datos de la Liquidacion despues de la ultima ejecución.");
                    exitMethod = true;
                }
                if (exitMethod) return;

                MyBatch.Log("Liquidacion OK.");
                NomadLog.Info("Comprobando LIQUIDACION: " + (DateTime.Now - myNow).TotalMilliseconds + "ms");
            }
            MyBatch.SetPro(20);

            /////////////////////////////////////////////////////////////////////////////////////////////
            // COMPROBAR EL HASH DE LAS PERSONAS
            if (!compPerChange)
            {
                if (!liqInterfaceAFIP)
                    MyBatch.Wrn("No se comprobaran los cambios en las Personas.");
            }
            else if (!liqInterfaceAFIP)
            {
                NomadLog.Info("Comprobando Cambios en las Personas..."); myNow = DateTime.Now;
                MyBatch.SetMess("Comprobando Cambios en las Personas...");

                //Cargo los INSERTS
                NomadXML LEGEMPIns = null;
                if (MyLiq.oi_tipo_liq == "7")
                    LEGEMPIns = NomadProxy.GetProxy().SQLService().GetXML("CLASS.NucleusRH.Base.Liquidacion.LegajoEmpresaDDO.PER_EMP_DDO.QryLegajoInterface", "<FILTRO />");
                else
                    LEGEMPIns = NomadProxy.GetProxy().SQLService().GetXML("CLASS.NucleusRH.Base.Liquidacion.LegajoEmpresaDDO.PER_EMP_DDO.QryLegajoParcial", "<FILTRO />");

                //NomadXML LEGEMPIns = NomadProxy.GetProxy().SQLService().GetXML("CLASS.NucleusRH.Base.Liquidacion.LegajoEmpresaDDO.PER_EMP_DDO.QryLegajoParcial", "<FILTRO />");
                NomadXML LEGLIQIns = NomadProxy.GetProxy().SQLService().GetXML("CLASS.NucleusRH.Base.Liquidacion.PersonalLiquidacionDDO.PER_LIQ_DDO.QRYPersonalParcial", "<FILTRO />");

                //Recorro las personas
                int cnt;
                string IDLists, perId, strNroLegajo;
                string[] subIDs;
                Hashtable PERSONAS;
                Hashtable PERLIQS;
                NomadXML xmlLEGEMP, xmlLEGLIQ, xmlDATA, xmlEJEC;
                bool bFail;
                string IDInterfaceList = "";
                string IDRecibosList = "";

                Hashtable xmlRespuesta = null;
                //LIQUIDACION liq = LIQUIDACION.Get(oi_liquidacion);
                if (MyLiq.oi_tipo_liq == "7")
                {
                    xmlRespuesta = NomadEnvironment.QueryHashtable(NucleusRH.Base.Liquidacion.LegajoEmpresaDDO.PER_EMP_DDO.Resources.QRY_BUSCAR_FOTO, "<PARAM oi_liquidacion='" + MyLiq.oi_liq_genera + "' />", "oi_personal_emp");

                }

                t = 0; IDLists = ""; cnt = 0;
                for (MyCUR = XMLPersonas.FirstChild(); MyCUR != null; MyCUR = MyCUR2)
                {
                    t++;
                    MyCUR2 = MyCUR.Next();

                    if (MyLiq.oi_tipo_liq == "7")
                    {
                        NomadXML xmlTemp = new NomadXML(xmlRespuesta[MyCUR.GetAttr("oi_personal_emp").Trim()].ToString()).FirstChild();
                        IDInterfaceList += "," + (xmlTemp.GetAttr("oi_per_emp_ddo")).Trim();
                        IDRecibosList += "," + (xmlTemp.GetAttr("oi_tot_liq_per")).Trim();
                    }

                    //Extraigo el ID
                    IDLists += "," + MyCUR.GetAttr("oi_personal_emp"); cnt++;
                    NomadProxy MyProxy;
					MyProxy = NomadProxy.GetProxy().Clone();
					MyProxy.SetValue("DATA", "pool", "LIQ");
					MyProxy.SetRunPath(NomadProxy.GetProxy().RunPath);


                    //Genero la Consulta
                    if ((cnt >= cantLegajos) || (MyCUR2 == null))
                    {
                        MyBatch.SetPro(20, 40, XMLPersonas.ChildLength, t);
                        MyBatch.SetMess("Comprobando Cambios en las Personas (" + t + "/" + XMLPersonas.ChildLength + ")...");
                        NomadLog.Info("IDList: " + IDLists);

                        IDLists = IDLists.Substring(1);

                        PERSONAS = LoadLegajosEmpresa(MyProxy, LEGEMPIns, IDLists, oi_cab_conc_ddo, oi_empresa_ddo, oi_var_ejec_ddo, oi_codif_ddo, oi_liquidacion, e_periodo, legList, MyLiq.oi_tipo_liq,IDInterfaceList,MyLiq.oi_liq_genera,IDRecibosList);
                        PERLIQS = LoadLegajosLiquidacion(MyProxy, LEGLIQIns, IDLists, oi_cab_conc_ddo, oi_empresa_ddo, oi_var_ejec_ddo, oi_codif_ddo, oi_liquidacion, e_periodo, legList, MyLiq.oi_tipo_liq, MyLiq.oi_liq_genera, IDInterfaceList);

                        subIDs = IDLists.Split(',');
                        IDLists = ""; cnt = 0;
                        IDInterfaceList = "";
                        IDRecibosList = "";

                        //Recorro los Legajos y los Construyo.
                        for (int d = 0; d < subIDs.Length; d++)
                        {
                            bFail = false;
                            perId = subIDs[d];
                            strNroLegajo = XMLPersonas.FindElement2("ROW", "oi_personal_emp", perId).GetAttr("e_numero_legajo");
                            xmlDATA = XMLRecibos.FindElement2("ROW", "e_numero_legajo", strNroLegajo);
                            xmlEJEC = XMLEjecuciones.FindElement2("ROW", "e_secuencia", xmlDATA.GetAttr("e_secuencia"));

                            if (compConcChange)
                            {
                                if (xmlEJEC.GetAttr("oi_codif_ddo") != oi_codif_ddo)
                                {
                                    MyBatch.Wrn("El Recibo para el Legajo " + xmlDATA.GetAttr("e_numero_legajo") + "-" + xmlDATA.GetAttr("d_ape_y_nom") + " se liquido con una version anterior de CODIFICADORAS.");
                                    bFail = true;
                                }
                                if (xmlEJEC.GetAttr("oi_var_ejec_ddo") != oi_var_ejec_ddo)
                                {
                                    MyBatch.Wrn("El Recibo para el Legajo " + xmlDATA.GetAttr("e_numero_legajo") + "-" + xmlDATA.GetAttr("d_ape_y_nom") + " se liquido con una version anterior de VARIABLES.");
                                    bFail = true;
                                }
                                if (xmlEJEC.GetAttr("oi_cab_conc_ddo") != oi_cab_conc_ddo)
                                {
                                    MyBatch.Wrn("El Recibo para el Legajo " + xmlDATA.GetAttr("e_numero_legajo") + "-" + xmlDATA.GetAttr("d_ape_y_nom") + " se liquido con una version anterior de CONCEPTOS.");
                                    bFail = true;
                                }
                            }

                            if (compEmpChange)
                            {
                                if (xmlEJEC.GetAttr("oi_empresa_ddo") != oi_empresa_ddo)
                                {
                                    MyBatch.Wrn("El Recibo para el Legajo " + xmlDATA.GetAttr("e_numero_legajo") + "-" + xmlDATA.GetAttr("d_ape_y_nom") + " se liquido con una version anterior de EMPRESAS.");
                                    bFail = true;
                                }
                            }

                            if (!PERSONAS.ContainsKey(perId))
                            {
                                MyBatch.Wrn("No se pudo comprobar los Cambios en el Legajo Empresa de " + xmlDATA.GetAttr("e_numero_legajo") + "-" + xmlDATA.GetAttr("d_ape_y_nom") + ".");
                                bFail = true;
                            }
                            else
                                if (!PERLIQS.ContainsKey(perId))
                                {
                                    MyBatch.Wrn("No se pudo comprobar los Cambios en el Legajo Liquidacion de " + xmlDATA.GetAttr("e_numero_legajo") + "-" + xmlDATA.GetAttr("d_ape_y_nom") + ".");
                                    bFail = true;
                                }
                                else
                                {

                                    xmlLEGEMP = (NomadXML)PERSONAS[perId];
                                    xmlLEGLIQ = (NomadXML)PERLIQS[perId];

                                    //LEGAJO LIQUIDACION
                                    strDDO = "<object nmd-status=\"~i,\" class=\"NucleusRH.Base.Liquidacion.PersonalLiquidacionDDO.PER_LIQ_DDO\" >" + xmlLEGLIQ.ToString() + "</object>";
                                    strHash = MD5("<(" + oi_cab_conc_ddo + "-" + oi_empresa_ddo + "-" + oi_var_ejec_ddo + "-" + oi_codif_ddo + "-" + oi_liquidacion + ")>" + strDDO);
                                    if (xmlDATA.GetAttr("PER_LIQ_HASH") != strHash)
                                    {
                                        MyBatch.Wrn("Se detectaron Cambios en el Legajo Liquidacion de " + xmlDATA.GetAttr("e_numero_legajo") + "-" + xmlDATA.GetAttr("d_ape_y_nom") + ".");
                                        bFail = true;
                                    }

                                    //LEGAJO EMPRESA
                                    strDDO = "<object nmd-status=\"~i,\" class=\"NucleusRH.Base.Liquidacion.LegajoEmpresaDDO.PER_EMP_DDO\" >" + xmlLEGEMP.ToString() + "</object>";
                  strHash = MD5("<(" + oi_cab_conc_ddo + "-" + oi_empresa_ddo + "-" + oi_var_ejec_ddo + "-" + oi_codif_ddo + "-" + oi_liquidacion + ")>" + strDDO);
                                    if (xmlDATA.GetAttr("PER_EMP_HASH") != strHash)
                                    {
                                        MyBatch.Wrn("Se detectaron Cambios en el Legajo Empresa de " + xmlDATA.GetAttr("e_numero_legajo") + "-" + xmlDATA.GetAttr("d_ape_y_nom") + ".");
                    bFail = true;
                                    }
                                }

                            if (bFail)
                            {
                                XMLPersonas.FindElement2("ROW", "oi_personal_emp", perId).SetAttr("del", "1");
                                RecibosNoCerrados++;
                                exitMethod = true;
                            }
                        }
                    }

          GC.Collect(0);
                }
                if (exitMethod)
                  bCerrarLiquidacion = false;
                else
                  MyBatch.Log("Personas OK.");
                NomadLog.Info("Comprobando Comprobando Cambios en las Personas: " + (DateTime.Now - myNow).TotalMilliseconds + "ms - (" + ((DateTime.Now - myNow).TotalMilliseconds / XMLPersonas.ChildLength) + "ms)");
            } else
              MyBatch.Log("Comprobación de personas - omitido");

            //Quito las personas que cambiaron
            do
            {
                MyCUR = XMLPersonas.FindElement2("ROW", "del", "1");
                if (MyCUR != null) XMLPersonas.DeleteChild(MyCUR);
            } while (MyCUR != null);
            MyBatch.Log("Comprobacion Finalizada.");
            MyBatch.SetPro(40);

            if (!ActAcumuladores)
            {
                if(!liqInterfaceAFIP)
                    MyBatch.Wrn("No se actualizaran los acumuladores.");
            }

            //Inicio la actualizacion de estados
            NomadLog.Info("Actualizando los Estados..."); myNow = DateTime.Now;
            MyBatch.Log("Actualizando los Estados...");
            NomadEnvironment.GetCurrentTransaction().Begin();

            string unit;
            double myTim, myVal;
            DateTime myStart = DateTime.Now;
            string myID;
            bool acumOK;
            int perDesde, perHasta;

            double LoadLegajo = 0;
            double LoadRecibo = 0;
            double AcumComun = 0;
            double AcumPeriodo = 0;
            double AcumCargo = 0;
            double RefreshOtros = 0;
            double SaveLegajo = 0;
            double SaveRecibo = 0;
            double SaveCommit = 0;

            perDesde = ((e_periodo / 100) - 1) * 100 + 1;
            perHasta = ((e_periodo / 100)) * 100 + 12;

            Dictionary<string, PropertyInfo> diccionarioJson = GetPropiedadesJson();

            string IDPers = "";
            string Legajos = "";
            string IDRecibos = "";
            int cant = 0;

            t = 0;  MyCUR3 = null;
            MyBatch.SetMess(liqInterfaceAFIP ? "Cerrando recibos..." : "Actualizar Personas...");
            for (MyCUR = XMLPersonas.FirstChild(); MyCUR != null; MyCUR = MyCUR3)
            {
                t++; MyCUR3 = MyCUR.Next();

                //Extraigo el ID
                IDPers += "," + MyCUR.GetAttr("oi_personal_emp");
                Legajos += "," + MyCUR.GetAttr("e_numero_legajo");
                IDRecibos+= "," + XMLRecibos.FindElement2("ROW", "e_numero_legajo", MyCUR.GetAttr("e_numero_legajo")).GetAttr("oi_tot_liq_per");
                cant++;

                //Genero la Consulta
                if ((cant >= cantLegajos) || (MyCUR3 == null))
                {
                  IDPers = IDPers.Substring(1);
                  IDRecibos = IDRecibos.Substring(1);
                  Legajos = Legajos.Substring(1);

                  string[] PerIDs = IDPers.Split(',');
                  string[] RecIDs = IDRecibos.Split(',');
                  string[] Nros   = Legajos.Split(',');

                  //Cargo los legajos
                  NomadLog.Info("IDPers: " + IDPers);
                  myNow2 = DateTime.Now;
                  Dictionary<string, NucleusRH.Base.Liquidacion.Legajo_Liquidacion.PERSONAL_EMP> PEs = LoadLegajos(IDPers, ActAcumuladores && !liqInterfaceAFIP);
                  LoadLegajo+=(DateTime.Now - myNow2).TotalMilliseconds;

                  //Cargo los recibos
                  NomadLog.Info("IDRecibos: " + IDRecibos);
                  myNow2 = DateTime.Now;
                  Dictionary<string, RecData> REs = LoadRecibos(IDRecibos, ActAcumuladores && !compPerChange && !liqInterfaceAFIP);
                  LoadRecibo+=(DateTime.Now - myNow2).TotalMilliseconds;

                  //Limpio las variables
                  IDPers = "";
                  Legajos = "";
                  IDRecibos = "";
                  cant = 0;

                  //Temporal para probar mejoras de tiempos
                  //bCerrarLiquidacion = false;
                  //NomadLog.Info("PEs[0]: " + PEs[PerIDs[0]].ToString());

                  NomadLog.Info("Recorro...");
                  for(int d=0; d < PerIDs.Length; d++)
                  {
                    string strNroLegajo = Nros[d];

                    try
                    {
                      //Begin
                      NomadEnvironment.GetCurrentTransaction().Begin();

                      Hashtable ToDelete;
                      NomadXML xmlVAR;
                      Legajo_Liquidacion.VAL_VAREA varAcum;

                      strNroLegajo = Nros[d];
                      NucleusRH.Base.Liquidacion.Legajo_Liquidacion.PERSONAL_EMP PE = PEs[PerIDs[d]];
                      RecData RE = REs[RecIDs[d]];

                      //ACTUALIZAR ACUMULADORES COMUNES
                      myNow2 = DateTime.Now;
                      if (ActAcumuladores && !liqInterfaceAFIP)
                      {
                        //VERIFICAR ACUMULADORES SI NO SE REVISO LOS CAMBIOS EN LAS PERSONAS
                        if (!compPerChange)
                        {
                          //VERIFICAR ACUMULADORES
                          acumOK = true;
                          NomadLog.Info("Verificar Acumuladores para el Legajo " + strNroLegajo + " ...");
                          foreach (Legajo_Liquidacion.VAL_VAREA acum in PE.VAL_VAREA)
                          {
                            xmlVAR = DIC_OIVar_2_XML[acum.oi_variable];

                            MyCUR2 = RE.QRY_ACUM.ContainsKey(xmlVAR.GetAttr("c_variable")) ? RE.QRY_ACUM[xmlVAR.GetAttr("c_variable")] : null;
                            if (MyCUR2 == null && acum.n_valor != 0)
                            {
                              acumOK = false;
                              NomadLog.Error("Cambio el ACUMULADOR " + xmlVAR.GetAttr("c_variable") + "; valor_usado=0; valor_actual=" + acum.n_valor.ToString() + ";");
                            } else if (MyCUR2 != null && acum.n_valor != MyCUR2.GetAttrDouble("n_valor_e"))
                            {
                              acumOK = false;
                              MyCUR2.SetAttr("check", "1");
                              NomadLog.Error("Cambio el ACUMULADOR " + xmlVAR.GetAttr("c_variable") + "; valor_usado=" + MyCUR2.GetAttrDouble("n_valor_e").ToString() + "; valor_actual=" + acum.n_valor.ToString() + ";");
                            } else
                            if (MyCUR2 != null)
                              MyCUR2.SetAttr("check", "1");
                          }
                          foreach(NomadXML MyCURX in RE.QRY_ACUM.Values)
                          {
                            if ((MyCURX.GetAttr("Check") != "1") && (MyCURX.GetAttrDouble("n_valor_e") != 0))
                            {
                              acumOK = false;
                              NomadLog.Error("Cambio el ACUMULADOR " + MyCURX.GetAttr("c_variable") + "; valor_usado=" + MyCURX.GetAttrDouble("n_valor_e").ToString() + "; valor_actual=0;");
                            }
                          }
                          if (!acumOK)
                          {
                            MyBatch.Err("Problemas en los Acumuladores de " + MyCUR.GetAttr("e_numero_legajo") + "-" + MyCUR.GetAttr("d_ape_y_nom") + ".");
                            bCerrarLiquidacion = false;
                            RecibosNoCerrados++;
                            continue;
                          }
                        }

                        //LIMPIO ACUMULADORES
                        NomadLog.Info("Limpiar Acumuladores para el Legajo " + strNroLegajo + " ...");
                        Dictionary<string, Legajo_Liquidacion.VAL_VAREA> VAL_VAREAAcums = new Dictionary<string, Legajo_Liquidacion.VAL_VAREA>();
                        foreach (Legajo_Liquidacion.VAL_VAREA acum in PE.VAL_VAREA)
                        {
                          acum.n_valor = 0;
                          VAL_VAREAAcums[acum.oi_variable] = acum;
                        }

                        //ACTUALIZO LOS ACUMULADORES
                        NomadLog.Info("Actualizar Acumuladores para el Legajo " + strNroLegajo + " ...");
                        foreach (string key in RE.VAREA_TOT.Keys)
                        {
                          if (DIC_CVar_2_OIVar.ContainsKey(key))
                          {
                            string oiVar = DIC_CVar_2_OIVar[key];

                            if (!VAL_VAREAAcums.ContainsKey(oiVar))
                            {
                              varAcum = new Legajo_Liquidacion.VAL_VAREA();
                              varAcum.oi_variable = oiVar;
                              PE.VAL_VAREA.Add(varAcum);
                            } else
                              varAcum = VAL_VAREAAcums[oiVar];
                            varAcum.n_valor = RE.VAREA_TOT[key];
                          }
                          else
                            MyBatch.Err("Variable " + key + " no encontrada en el Legajo " + strNroLegajo);
                         }
                      }
                      AcumComun+=(DateTime.Now - myNow2).TotalMilliseconds;

                      //ACTUALIZAR ACUMULADORES DE PERIODO
                      myNow2 = DateTime.Now;
                      if (ActAcumuladores && !liqInterfaceAFIP)
                      {
                        if (!compPerChange)
                        {
                          //VERIFICO ACUMULADORES
                          acumOK = true;
                          NomadLog.Info("Verificar Acumuladores de Periodo (" + perDesde + "-" + perHasta + ") para el Legajo " + strNroLegajo + " ...");
                          foreach (Legajo_Liquidacion.VAL_VARPA vv in PE.VAL_VARPA)
                            if (vv.e_periodo >= perDesde && vv.e_periodo <= perHasta)
                            {
                              xmlVAR = DIC_OIVar_2_XML[vv.oi_variable];

                              MyCUR2 = RE.QRY_PER_ACUM.ContainsKey(xmlVAR.GetAttr("c_variable")+ "[" + vv.e_periodo + "]") ? RE.QRY_PER_ACUM[xmlVAR.GetAttr("c_variable")+ "[" + vv.e_periodo + "]"] : null;
                              if (MyCUR2 == null && vv.n_valor != 0)
                              {
                                acumOK = false;
                                NomadLog.Error("Cambio el ACUMULADOR " + xmlVAR.GetAttr("c_variable") + "[" + vv.e_periodo + "]" + "; valor_usado=0; valor_actual=" + vv.n_valor.ToString() + ";");
                              }
                              else
                              if (MyCUR2 != null && vv.n_valor != MyCUR2.GetAttrDouble("n_valor_e"))
                              {
                                acumOK = false;
                                MyCUR2.SetAttr("check", "1");
                                NomadLog.Error("Cambio el ACUMULADOR " + xmlVAR.GetAttr("c_variable") + "[" + vv.e_periodo + "]" + "; valor_usado=" + MyCUR2.GetAttrDouble("n_valor_e").ToString() + "; valor_actual=" + vv.n_valor.ToString() + ";");
                              }
                              else
                              if (MyCUR2 != null)
                                MyCUR2.SetAttr("check", "1");
                            }
                          foreach(NomadXML MyCURX in RE.QRY_PER_ACUM.Values)
                          {
                            if ((MyCURX.GetAttr("Check") != "1") && (MyCURX.GetAttrDouble("n_valor_e") != 0))
                            {
                              acumOK = false;
                              NomadLog.Error("Cambio el ACUMULADOR " + MyCURX.GetAttr("c_variable") + "; valor_usado=" + MyCURX.GetAttrDouble("n_valor_e").ToString() + "; valor_actual=0;");
                            }
                          }
                          if (!acumOK)
                          {
                            MyBatch.Err("Problemas en los Acumuladores de Periodo de " + MyCUR.GetAttr("e_numero_legajo") + "-" + MyCUR.GetAttr("d_ape_y_nom") + ".");
                            bCerrarLiquidacion = false;
                            RecibosNoCerrados++;
                            continue;
                          }
                        }

                        //ACUMULADORES POR PERIODO
                        ToDelete = new Hashtable();
                        foreach (Legajo_Liquidacion.VAL_VARPA vv in PE.VAL_VARPA)
                          if (vv.e_periodo >= perDesde && vv.e_periodo <= perHasta)
                            ToDelete[vv.e_periodo.ToString() + "_" + vv.oi_variable.ToString()] = vv;

                        NomadLog.Info("Actualizar Acumuladores por Periodo para el Legajo " + strNroLegajo + " ...");

                        string ultimoCodigo = null;
                        foreach (string key in RE.VARPA_ANNO.Keys)
                        {
                          string codigoActual = key.Substring(0,key.LastIndexOf('_'));
                          if (DIC_CVar_2_OIVar.ContainsKey(codigoActual))
                          {
                            string oiVar = DIC_CVar_2_OIVar[codigoActual];
                            Legajo_Liquidacion.VAL_VARPA vv;

                            string periodo = key.Substring(key.LastIndexOf('_') + 1);
                            myID = periodo + "_" + oiVar;
                            myVal = RE.VARPA_ANNO[key];

                            if (myVal >= 0.0005 || myVal <= -0.0005)
                            { //no es cero
                              if (ToDelete.ContainsKey(myID))
                              {
                                vv = (Legajo_Liquidacion.VAL_VARPA)ToDelete[myID];
                                vv.n_valor = myVal;
                                ToDelete.Remove(myID);
                              }
                              else
                              {
                                vv = new Legajo_Liquidacion.VAL_VARPA();
                                vv.oi_variable = oiVar;
                                vv.e_periodo = int.Parse(periodo);
                                vv.n_valor = myVal;
                                PE.VAL_VARPA.Add(vv);
                              }
                            }
                          }
                          ultimoCodigo = codigoActual;
                        }

                        NomadLog.Info("Eliminar Acumuladores por Periodo para el Legajo " + strNroLegajo + " ...");
                        foreach (Legajo_Liquidacion.VAL_VARPA vv in ToDelete.Values)
                          PE.VAL_VARPA.Remove(vv);
                      }
                      AcumPeriodo+=(DateTime.Now - myNow2).TotalMilliseconds;

                      //ACUMULADORES DE CARGO
                      myNow2 = DateTime.Now;
                      if (ActAcumuladores && !liqInterfaceAFIP)
                      {
                        //RECORRO LOS CARGOS
                        acumOK = true;
                        foreach (NucleusRH.Base.Personal.LegajoEmpresa.CARGO cargo in PE.CARGOS)
                        {
                            //QUERY DE ACUMULADORES
                            MyPARAM = new NomadXML("PARAM");
                            MyPARAM.SetAttr("oi_per_emp_ddo", RE.REC.oi_per_emp_ddo);
                            MyPARAM.SetAttr("oi_cargo", cargo.Id);
                            MySQL=proxy.SQLService().GetXML("CLASS.NucleusRH.Base.Liquidacion.LiquidacionDDO.LIQUIDACION_DDO.QRY_CARGO_ACUM", MyPARAM.ToString());

                            //VERIFICO SI EL CARGO PARTICIPO DE LA LIQUIADACION
                            if (!MySQL.GetAttrBool("valid")) continue;

                            //VERIFICO ACUMULADORES
                            NomadLog.Info("Verificar Acumuladores de Cargo ("+cargo.Id+" - " + perDesde + "-" + perHasta + ") para el Legajo " + strNroLegajo + " ...");
                            foreach (Legajo_Liquidacion.VARPA_CARGO vv in PE.VARPA_CARGO)
                            if (vv.oi_cargo==cargo.Id && vv.e_periodo >= perDesde && vv.e_periodo <= perHasta)
                            {
                              xmlVAR = DIC_OIVar_2_XML[vv.oi_variable];

                              MyCUR2 = MySQL.FindElement2("ROW", "c_variable", xmlVAR.GetAttr("c_variable") + "[" + vv.e_periodo + "]");
                              if (MyCUR2 == null && vv.n_valor != 0)
                              {
                                acumOK = false;
                                NomadLog.Error("Cambio el ACUMULADOR " + xmlVAR.GetAttr("c_variable") + "[" + vv.e_periodo + "]" + "; valor_usado=0; valor_actual=" + vv.n_valor.ToString() + ";");
                              } else if (MyCUR2 != null && vv.n_valor != MyCUR2.GetAttrDouble("n_valor_e"))
                              {
                                acumOK = false;
                                MyCUR2.SetAttr("check", "1");
                                NomadLog.Error("Cambio el ACUMULADOR " + xmlVAR.GetAttr("c_variable") + "[" + vv.e_periodo + "]" + "; valor_usado=" + MyCUR2.GetAttrDouble("n_valor_e").ToString() + "; valor_actual=" + vv.n_valor.ToString() + ";");
                              } else
                                if (MyCUR2 != null)
                                  MyCUR2.SetAttr("check", "1");
                            }
                            for (MyCUR2 = MySQL.FirstChild(); MyCUR2 != null; MyCUR2 = MyCUR2.Next())
                            {
                              if ((MyCUR2.GetAttr("Check") != "1") && (MyCUR2.GetAttrDouble("n_valor_e") != 0))
                              {
                                acumOK = false;
                                NomadLog.Error("Cambio el ACUMULADOR " + MyCUR2.GetAttr("c_variable") + "; valor_usado=" + MyCUR2.GetAttrDouble("n_valor_e").ToString() + "; valor_actual=0;");
                              }
                            }
                            if (!acumOK)
                            {
                              MyBatch.Err("Problemas en los Acumuladores de Cargo de " + MyCUR.GetAttr("e_numero_legajo") + "-" + MyCUR.GetAttr("d_ape_y_nom") + ".");
                              bCerrarLiquidacion = false;
                              RecibosNoCerrados++;
                              break;
                            }

                            //ACUMULADORES POR PERIODO
                            ToDelete = new Hashtable();
                            foreach (Legajo_Liquidacion.VARPA_CARGO vv in PE.VARPA_CARGO)
                              if (vv.oi_cargo==cargo.Id && vv.e_periodo >= perDesde && vv.e_periodo <= perHasta)
                                ToDelete[vv.e_periodo.ToString() + "_" + vv.oi_variable.ToString()] = vv;

                            NomadLog.Info("Actualizar Acumuladores por Cargo para el Legajo " + strNroLegajo + " ...");
                            foreach (Recibos.VARPA_CARGO recaper in RE.VARPA_CARGO)
                            if (recaper.oi_cargo==cargo.id)
                            {
                              if (DIC_CVar_2_OIVar.ContainsKey(recaper.c_variable))
                              {
                                string oiVar = DIC_CVar_2_OIVar[recaper.c_variable];
                                Legajo_Liquidacion.VARPA_CARGO vv;

                                for (int m = 1; m <= 12; m++)
                                {
                                  myVal = 0;
                                  myID = (recaper.e_anno * 100 + m) + "_" + oiVar;
                                  switch (m)
                                  {
                                    case 1: myVal = recaper.n_valor_s_1; break;
                                    case 2: myVal = recaper.n_valor_s_2; break;
                                    case 3: myVal = recaper.n_valor_s_3; break;
                                    case 4: myVal = recaper.n_valor_s_4; break;
                                    case 5: myVal = recaper.n_valor_s_5; break;
                                    case 6: myVal = recaper.n_valor_s_6; break;
                                    case 7: myVal = recaper.n_valor_s_7; break;
                                    case 8: myVal = recaper.n_valor_s_8; break;
                                    case 9: myVal = recaper.n_valor_s_9; break;
                                    case 10: myVal = recaper.n_valor_s_10; break;
                                    case 11: myVal = recaper.n_valor_s_11; break;
                                    case 12: myVal = recaper.n_valor_s_12; break;
                                  }

                                  if (myVal >= 0.0005 || myVal <= -0.0005)
                                  { //no es cero
                                    if (ToDelete.ContainsKey(myID))
                                    {
                                      vv = (Legajo_Liquidacion.VARPA_CARGO)ToDelete[myID];
                                      vv.n_valor = myVal;
                                      ToDelete.Remove(myID);
                                    } else
                                    {
                                      vv = new Legajo_Liquidacion.VARPA_CARGO();
                                      vv.oi_variable = oiVar;
                                      vv.e_periodo = recaper.e_anno * 100 + m;
                                      vv.n_valor = myVal;
                                      vv.oi_cargo = cargo.Id;
                                      PE.VARPA_CARGO.Add(vv);
                                    }
                                  }
                                }
                              }
                            }

                            NomadLog.Info("Eliminar Acumuladores por Cargo para el Legajo " + strNroLegajo + " ...");
                            foreach (Legajo_Liquidacion.VARPA_CARGO vv in ToDelete.Values)
                              PE.VARPA_CARGO.Remove(vv);

                          }

                          if (!acumOK)
                            continue;
                      }
                      AcumCargo+=(DateTime.Now - myNow2).TotalMilliseconds;

                      if (!liqInterfaceAFIP)
                      {
                        //ANTICIPOS OTORGADOS
                        myNow2 = DateTime.Now;
                        NomadLog.Info("Anticipos Otorgados...");
                        foreach (Recibos.ANT_LIQ_PER recAntOt in RE.ANT_LIQ_PER)
                        {
                          DateTime f_solicitud = Nomad.NSystem.Functions.StringUtil.str2date(NomadEnvironment.QueryValue("LIQ97_ANTICIPOS", "f_solicitud", "oi_anticipo_ddo", recAntOt.oi_anticipo_ddo, "", false));
                          Legajo_Liquidacion.ANTICIPO legAnt = (Legajo_Liquidacion.ANTICIPO)PE.ANTICIPOS.GetByAttribute("f_solicitud", f_solicitud);
                          if (legAnt != null)
                            legAnt.c_estado = "L";
                          else
                            NomadLog.Info("Anticipo con Fecha '" + f_solicitud.ToString("dd/MM/yyyy") + "' no Encontrado...");
                        }

                        //ANTICIPOS DESCONTADOS
                        NomadLog.Info("Anticipos Descontados...");
                        foreach (Recibos.DANT_LIQ_PER recAntDes in RE.DANT_LIQ_PER)
                        {
                          string oi_anticipo_ddo = NomadEnvironment.QueryValue("LIQ97_DESC_ANTI", "oi_anticipo_ddo", "oi_desc_ant_ddo", recAntDes.oi_desc_ant_ddo, "", false);
                          DateTime f_solicitud = Nomad.NSystem.Functions.StringUtil.str2date(NomadEnvironment.QueryValue("LIQ97_ANTICIPOS", "f_solicitud", "oi_anticipo_ddo", oi_anticipo_ddo, "", false));
                          Legajo_Liquidacion.ANTICIPO legAnt = (Legajo_Liquidacion.ANTICIPO)PE.ANTICIPOS.GetByAttribute("f_solicitud", f_solicitud);
                          if (legAnt != null)
                          {
                              int e_cuota = int.Parse(NomadEnvironment.QueryValue("LIQ97_DESC_ANTI", "e_cuota", "oi_desc_ant_ddo", recAntDes.oi_desc_ant_ddo, "", false));
                              Legajo_Liquidacion.DESC_ANTICIPO legAntDesc = (Legajo_Liquidacion.DESC_ANTICIPO)legAnt.DESC_ANTI.GetByAttribute("e_cuota", e_cuota);
                              if (legAntDesc != null)
                              {
                                  //Cambio el ESTADO
                                  legAntDesc.c_estado = "D";
                                  legAnt.n_imp_pendiente -= legAntDesc.n_importe;
                                  legAnt.c_estado = (legAnt.DESC_ANTI.GetByAttribute("c_estado", "P") == null ? "F" : "D");
                                  legAntDesc.oi_liquidacion = this.MyLiq.Id;
                              }
                              else
                                NomadLog.Info("Cuota '" + e_cuota.ToString() + "' del Anticipo con Fecha '" + f_solicitud.ToString("dd/MM/yyyy") + "' no Encontrado...");
                          }
                          else
                            NomadLog.Info("Anticipo con Fecha '" + f_solicitud.ToString("dd/MM/yyyy") + "' no Encontrado...");
                        }

                        //EMBARGOS DESCONTADOS
                        NomadLog.Info("Embargos...");
                        foreach (Recibos.EMB_LIQ_PER recEmdDes in RE.EMB_LIQ_PER)
                        {
                          int e_NroOficio = int.Parse(NomadEnvironment.QueryValue("LIQ97_EMBARGOS", "e_nro_oficio", "oi_embargo_ddo", recEmdDes.oi_embargo_ddo, "", false));
                          Legajo_Liquidacion.EMBARGO legEmg = (Legajo_Liquidacion.EMBARGO)PE.EMBARGOS.GetByAttribute("e_nro_oficio", e_NroOficio);

                          //Agrego el Descuento del EMBARGO
                          Legajo_Liquidacion.DESC_EMB legDescEmg = new Legajo_Liquidacion.DESC_EMB();
                          legDescEmg.f_descuento = this.MyLiq.f_liquidacion;
                          legDescEmg.n_importe = recEmdDes.n_valor;
                          legDescEmg.oi_liquidacion = this.MyLiq.Id;
                          legEmg.DESC_EMB.Add(legDescEmg);

                          //Actualizo el Estado
                          if (legEmg.c_tipo_embargo.ToUpper() == "OC" || legEmg.n_monto_pend > 0)
                          {
                            legEmg.n_monto_pend -= recEmdDes.n_valor;
                            if (legEmg.n_monto_pend < 0.01)
                            {
                                legEmg.n_monto_pend = 0;
                                legEmg.c_estado = "F";
                            }
                            else
                              legEmg.c_estado = "A";
                          }
                          else
                            legEmg.c_estado = "A";
                        }

                        //CONSUMOS DEL PERSONAL
                        NomadLog.Info("Consumos del personal...");
                        foreach (Recibos.CON_LIQ_PER recConOt in RE.CON_LIQ_PER) //busca consumos del recibo
                        {
                          string c_consumo_per = NomadEnvironment.QueryValue("LIQ97_CONSUMOS", "c_consumo_per", "oi_consumo_ddo", recConOt.oi_consumo_ddo, "", false);
                          Legajo_Liquidacion.CONSUMO legCon = (Legajo_Liquidacion.CONSUMO)PE.CONSUMOS.GetByAttribute("c_consumo_per", c_consumo_per);
                          if (legCon != null)
                          {
                            legCon.oi_liquidacion = this.MyLiq.Id;
                            legCon.c_estado = "P";
                          }
                          else
                            NomadLog.Info("Consumo con Codigo '" + c_consumo_per + "' no Encontrado...");
                        }

                        //Actualizo Campos en Legajo Liquidacion
                        if (c_tipo_liq == "3")
                        {
                          NomadLog.Info("Liquidacion Final.");
                          PE.l_liq_final = true;
                        }
                      }
                      RefreshOtros+=(DateTime.Now - myNow2).TotalMilliseconds;

                      //Guardo el Personal + Recibo
                      myNow2 = DateTime.Now;
                      NomadLog.Info("Recibo.");
                      RE.REC.f_cierre = fcierre;
                      NomadEnvironment.GetCurrentTransaction().Save(RE.REC);
                      SaveRecibo+=(DateTime.Now - myNow2).TotalMilliseconds;

                      if (!liqInterfaceAFIP)
                      {
                        NomadLog.Info("Personal.");
                        myNow2 = DateTime.Now;
                        NomadEnvironment.GetCurrentTransaction().Save(PE);
                        SaveLegajo+=(DateTime.Now - myNow2).TotalMilliseconds;
                      }

                      NomadLog.Info("Commit Parcial.");
                      myNow2 = DateTime.Now;
                      NomadEnvironment.GetCurrentTransaction().Commit();
                      SaveCommit+=(DateTime.Now - myNow2).TotalMilliseconds;

                      MyBatch.Log("Recibo " + strNroLegajo + " - " + XMLPersonas.FindElement2("ROW", "e_numero_legajo", strNroLegajo).GetAttr("d_ape_y_nom") + " Cerrado.");
                      RecibosCerrados++;
                    }
                    catch (Exception E)
                    {
                      MyBatch.Err("No se pudo Cerrar el Recibo de " + strNroLegajo + "-" + XMLPersonas.FindElement2("ROW", "e_numero_legajo", strNroLegajo).GetAttr("d_ape_y_nom") + ".");
                      NomadLog.Error(E.Message, E);
                      NomadEnvironment.GetCurrentTransaction().Rollback();
                      bCerrarLiquidacion = false;
                      RecibosNoCerrados++;

                      return;
                    }
                  }

                  ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                  //Actualizo la barra de progreso
                  MyBatch.SetPro(40, 90, XMLPersonas.ChildLength, t);
                  myTim = ((TimeSpan)(DateTime.Now - myStart)).TotalMinutes;
                  myTim = (myTim / t) * (XMLPersonas.ChildLength - t);
                  unit = "min";
                  if (myTim < 2)
                  {
                    myTim *= 60;
                    unit = "seg";
                  }
                  MyBatch.SetMess((liqInterfaceAFIP ? "Cerrando recibos" : "Actualizar Personas") + " (" + t + "/" + XMLPersonas.ChildLength + ")-(ETA " + Math.Ceiling(myTim) + " " + unit + ")...");
                }

        GC.Collect(0);
            }
            MyBatch.SetPro(90);
            NomadLog.Info("Actualizando los Estados: " + (DateTime.Now - myNow).TotalMilliseconds + "ms - (" + ((DateTime.Now - myNow).TotalMilliseconds / XMLPersonas.ChildLength) + "ms)");

            NomadLog.Info("Actualizando los LoadLegajo: " + LoadLegajo + "ms (" + (LoadLegajo / XMLPersonas.ChildLength) + "ms)");
            NomadLog.Info("Actualizando los LoadRecibo: " + LoadRecibo + "ms (" + (LoadRecibo / XMLPersonas.ChildLength) + "ms)");
            NomadLog.Info("Actualizando los AcumComun: " + AcumComun + "ms (" + (AcumComun / XMLPersonas.ChildLength) + "ms)");
            NomadLog.Info("Actualizando los AcumPeriodo: " + AcumPeriodo + "ms (" + (AcumPeriodo / XMLPersonas.ChildLength) + "ms)");
            NomadLog.Info("Actualizando los AcumCargo: " + AcumCargo + "ms (" + (AcumCargo / XMLPersonas.ChildLength) + "ms)");
            NomadLog.Info("Actualizando los RefreshOtros: " + RefreshOtros + "ms (" + (RefreshOtros / XMLPersonas.ChildLength) + "ms)");
            NomadLog.Info("Actualizando los SaveLegajo: " + SaveLegajo + "ms (" + (SaveLegajo / XMLPersonas.ChildLength) + "ms)");
            NomadLog.Info("Actualizando los SaveRecibo: " + SaveRecibo + "ms (" + (SaveRecibo / XMLPersonas.ChildLength) + "ms)");
            NomadLog.Info("Actualizando los SaveCommit: " + SaveCommit + "ms (" + (SaveCommit / XMLPersonas.ChildLength) + "ms)");

            if (!CerrarLiquidacion)
            {
                MyBatch.Wrn("No se cerrara la liquidacion.");
            }
            else
            {
                //Actualizo el Registro de Liquidacion
                if (bCerrarLiquidacion)
                {
                    NomadLog.Info("Actualizo el Estado a la Liquidacion...."); myNow = DateTime.Now;
                    this.MyLiq.c_estado = "C";
                    this.MyLiq.f_cierre = fcierre;
                    myliqddo.FechaCierre = fcierre;

                    //COMMIT
                    NomadEnvironment.GetCurrentTransaction().Begin();
                    NomadEnvironment.GetCurrentTransaction().Save(myliqddo);
                    NomadEnvironment.GetCurrentTransaction().Save(this.MyLiq);
                    NomadEnvironment.GetCurrentTransaction().Commit();
                    MyBatch.Log("Liquidacion Cerrada.");
                    NomadLog.Info("Actualizo el Estado a la Liquidacions: " + (DateTime.Now - myNow).TotalMilliseconds + "ms");
                }
                else
                {
                    MyBatch.Err("La liquidacion no puede ser cerrada.");
                }
            }

            if (RecibosCerrados != 0) MyBatch.Log(RecibosCerrados + " Recibos Cerrados.");
            if (LegajosSinRecibos != 0) MyBatch.Err(LegajosSinRecibos + " Legajos sin Recibo.");
            if (RecibosNoCerrados != 0) MyBatch.Err(RecibosNoCerrados + " Recibos No Cerrados.");
        }

        private bool LiquidacionOrigenCerrada()
        {
            string estado = NomadEnvironment.QueryValue("LIQ19_LIQUIDACION", "c_estado", "oi_liquidacion", this.MyLiq.oi_liq_genera.ToString(), "", false);
            return estado == "C";
        }

        private bool LiquidacionLSDProcesada()
        {
            NomadXML xmlLiquidacion = NomadEnvironment.QueryNomadXML(LegajoEmpresaDDO.PER_EMP_DDO.Resources.QRY_LIQ_LSD_PROCESADA, "<FILTRO oi_liq_lsd ='" + this.MyLiq.id + "' oi_liq_origen='"+this.MyLiq.oi_liq_genera+"' />").FirstChild();
            return xmlLiquidacion.GetAttrInt("oi_liq_ddo_lsd") >= xmlLiquidacion.GetAttrInt("oi_liq_ddo_origen");
        }

        private bool OrdenCorrectoDeCierre()
        {
            NomadXML xmlLiquidacion = NomadEnvironment.QueryNomadXML(LegajoEmpresaDDO.PER_EMP_DDO.Resources.QRY_LIQUIDACION_LSD, "<FILTRO e_periodo ='" + this.MyLiq.e_periodo + "'/>").FirstChild();
            if (xmlLiquidacion.FirstChild() != null)
            {
                return xmlLiquidacion.FirstChild().GetAttrInt("oi_liq_genero") == this.MyLiq.id;
            }
            return false;
        }

    public void ReopenLiq(int idLiquidacion, string idsAbrir)
    {
      NomadBatch objBatch = NomadBatch.GetBatch("Reapertura de Liquidación", "Reapertura de Liquidación");
      NomadBatch.Trace("Comienza Reapertura de Liquidación ------------------------------------ ");

      try
      {
        int rec_abiertos = 0;
        int RowsCount = 0;
        int intCurrentItem;

        LIQUIDACION Liquidacion = LIQUIDACION.Get(idLiquidacion, false);
        //NucleusRH.Base.Organizacion.Empresas.EMPRESA Empresa = NucleusRH.Base.Organizacion.Empresas.EMPRESA.Get(Liquidacion.oi_empresa, false);
                NomadXML xmlLiquidacionDDo = proxy.SQLService().GetXML("CLASS.NucleusRH.Base.Liquidacion.LiquidacionDDO.LIQUIDACION_DDO.QRY_LIQ_FINAL", "<PARAM oi_liquidacion=\"" + idLiquidacion + "\" />");
                string oi_liquidacion_ddo = null;
                if (xmlLiquidacionDDo.FirstChild() != null)
                {
                   oi_liquidacion_ddo = xmlLiquidacionDDo.FirstChild().GetAttr("oi_liquidacion_ddo");
                }

                //string oi_liquidacion_ddo = NomadEnvironment.QueryValue("LIQ99_LIQUIDACION", "oi_liquidacion_ddo", "c_liquidacion", Liquidacion.c_liquidacion, string.Format("LIQ99_LIQUIDACION.c_empresa=\\'{0}\\' and LIQ99_LIQUIDACION.c_estado=\\'F\\'", Empresa.c_empresa), false);
        LIQUIDACION_DDO LiquidacionDDO = LIQUIDACION_DDO.Get(oi_liquidacion_ddo);
        int per_desde = (LiquidacionDDO.Periodo / 100 - 1) * 100 + 1;
        int per_hasta = LiquidacionDDO.Periodo / 100 * 100 + 12;

        IList<string> ListaLegajos = idsAbrir.Split(',');
        string e_numero_legajo = string.Empty;
        objBatch.SetPro(10);
        RowsCount = ListaLegajos.Count;
        intCurrentItem = 0;
        foreach (string id in ListaLegajos)
        {
          try
          {
            intCurrentItem++;
            e_numero_legajo = NomadEnvironment.QueryValue("PER02_PERSONAL_EMP", "e_numero_legajo", "oi_personal_emp", id, string.Empty, false);
            RECIBO ReciboDDO = (RECIBO)LiquidacionDDO.Recibos.GetByAttribute("Legajo", int.Parse(e_numero_legajo));
            Recibos.TOT_LIQ_PER Recibo = Recibos.TOT_LIQ_PER.Get(ReciboDDO.Recibo, false);

            // Valida que el recibo este cerrado
            if (Recibo.f_cierreNull)
            {
              objBatch.Err(string.Format("El recibo del legajo {0} no está cerrado, se ignorará.", e_numero_legajo));
              continue;
            }
            // Valida que no tenga otra liquidacion cerrada posteriormente
            string tiene_rec = NomadEnvironment.QueryNomadXML(Legajo_Liquidacion.PERSONAL_EMP.Resources.qry_vallidar_ult_liq, string.Format(@"<DATA f_cierre=""{0}"" oi_personal_emp=""{1}""></DATA>", Recibo.f_cierre.ToString("yyyyMMddHHmmss"), id)).FirstChild().GetAttr("tiene_liq");
            if (tiene_rec == "1")
            {
              objBatch.Err(string.Format("El legajo {0} tiene otra liquidación cerrada posterior a la actual. No se puede abrir el recibo.", e_numero_legajo));
              continue;
            }

            Legajo_Liquidacion.PERSONAL_EMP Legajo = Legajo_Liquidacion.PERSONAL_EMP.Get(id, false);
            LegajoEmpresaDDO.PER_EMP_DDO LegajoDDO = LegajoEmpresaDDO.PER_EMP_DDO.Get(Recibo.oi_per_emp_ddo, false);

            // Borrar acumuladores comunes
            Legajo.VAL_VAREA.Clear();

                        //this.ConvertirStringADDO<LegajoEmpresaDDO.DDO_VAR, LegajoEmpresaDDO.VAL_VAREA_DDO>(LegajoDDO.DdoVar, LegajoDDO.VariablesAcum, propiedadesDDO);

            // Insertar acumuladores comunes
            foreach (LegajoEmpresaDDO.VAL_VAREA_DDO a in LegajoDDO.VariablesAcum)
            {

        Legajo_Liquidacion.VAL_VAREA new_acum = new Legajo_Liquidacion.VAL_VAREA();

        string auxOiVariable = NomadEnvironment.QueryValue("LIQ09_VARIABLES", "oi_variable", "c_variable", a.CodigoVariable, string.Empty, true);

        int aux_oi_variable;
        if(int.TryParse(auxOiVariable, out aux_oi_variable))
          new_acum.oi_variable = aux_oi_variable.ToString();
        else
          objBatch.Err("No se encontro '"+a.CodigoVariable+"' en las variables definidas actualmente. Verifique por favor mayusculas y minusculas.");

        new_acum.n_valor = a.ValorEntrada;
        Legajo.VAL_VAREA.Add(new_acum);
            }

            // Borrar acumuladores de periodo
            /*IEnumerator acums = Legajo.VAL_VARPA.GetEnumerator();
            while (acums.MoveNext())
            {
              Legajo_Liquidacion.VAL_VARPA varpa = (Legajo_Liquidacion.VAL_VARPA)acums.Current;
              if (varpa.e_periodo >= per_desde && varpa.e_periodo <= per_hasta)
                Legajo.VAL_VARPA.Remove(varpa);
            }*/

            ArrayList aBorrar = new ArrayList();

            foreach (Legajo_Liquidacion.VAL_VARPA varpa in Legajo.VAL_VARPA)
            {
              if (varpa.e_periodo >= per_desde && varpa.e_periodo <= per_hasta)
                aBorrar.Add(varpa);
            }

            foreach (Legajo_Liquidacion.VAL_VARPA varpa in aBorrar)
            {
              Legajo.VAL_VARPA.Remove(varpa);
            }
            // Insertar acumuladores de periodo
                       foreach (LegajoEmpresaDDO.VAL_A_ANNO_DDO a in LegajoDDO.VariablesAnno)
            {
              Legajo_Liquidacion.VAL_VARPA new_acum;
              for (int i = 1; i <= 12; i++)
              {
                new_acum = new Legajo_Liquidacion.VAL_VARPA();
                new_acum.oi_variable = NomadEnvironment.QueryValue("LIQ09_VARIABLES", "oi_variable", "c_variable", a.CodigoVariable, string.Empty, true);
                new_acum.e_periodo = a.Periodo * 100 + i;
                new_acum.n_valor = double.Parse(a.GetType().GetProperty("ValorEntrada" + i.ToString()).GetValue(a, null).ToString());
                Legajo.VAL_VARPA.Add(new_acum);
              }
            }

            // Actualizar embargos OC
            foreach (Legajo_Liquidacion.EMBARGO e in Legajo.EMBARGOS)
            {
              //if (e.c_tipo_embargo != "OC") continue;
              IList<Legajo_Liquidacion.DESC_EMB> del_desc = new List<Legajo_Liquidacion.DESC_EMB>();
              foreach (Legajo_Liquidacion.DESC_EMB d in e.DESC_EMB)
              {
                if (d.oi_liquidacion != Liquidacion.Id) continue;
                e.c_estado = "A";
                                if(e.c_tipo_embargo == "OC")
                    e.n_monto_pend += d.n_importe;
                del_desc.Add(d);
              }
              // Borrar descuentos de embargos
              foreach (Legajo_Liquidacion.DESC_EMB d in del_desc)
                e.DESC_EMB.Remove(d);
            }

                        // Actualizar Anticipos
                        foreach (Legajo_Liquidacion.ANTICIPO a in Legajo.ANTICIPOS)
                        {
                            foreach (Legajo_Liquidacion.DESC_ANTICIPO d in a.DESC_ANTI)
                            {
                                if(d.oi_liquidacion != null)
                                {
                                    if (d.oi_liquidacion != Liquidacion.Id) continue;
                                    d.c_estado = "P";
                                    //a.n_imp_pendiente += d.n_importe; //ver si hay que hacer esto o no
                                    a.c_estado = "L";
                                    d.oi_liquidacion = null;
                                }
                            }

                        }

                        //Actualizar consumos
                        foreach (Legajo_Liquidacion.CONSUMO c in Legajo.CONSUMOS)
                        {
                            if (c.oi_liquidacion != Liquidacion.Id) continue;
                            c.c_estado = "A";
                            c.oi_liquidacion = null;

                            //falta ver como manejan los montos
                        }

            // Actualizar cabecera de recibo
            Recibo.f_cierreNull = true;
            if (Liquidacion.oi_tipo_liq == "3" && !Liquidacion.l_ajuste)
              Legajo.l_liq_final = false;

            NomadEnvironment.GetCurrentTransaction().Begin();
            NomadEnvironment.GetCurrentTransaction().Save(Legajo);
            NomadEnvironment.GetCurrentTransaction().Save(Recibo);
            NomadEnvironment.GetCurrentTransaction().Commit();
            rec_abiertos++;
          }
          catch (Exception e)
          {
            objBatch.Err(string.Format("Error abriendo recibo de legajo {0}", e_numero_legajo));
            NomadBatch.Trace(string.Format("Error abriendo recibo de legajo {0}.\r\n{1}\r\n{2}", e_numero_legajo, e.Message, e.StackTrace));
          }
          objBatch.SetPro(10, 90, RowsCount, intCurrentItem);
        }

        // Actualizar cabecera de liquidacion
        if (rec_abiertos > 0 && Liquidacion.c_estado == "C")
        {

                    if (Liquidacion.oi_liq_genero != 0 || !Liquidacion.oi_liq_generoNull )
                    {
                        EliminarLSD(Liquidacion.oi_liq_genero);
                        Liquidacion.oi_liq_generoNull = true;
                    }

          Liquidacion.f_cierreNull = true;
          Liquidacion.c_estado = "I";
          NomadEnvironment.GetCurrentTransaction().Save(Liquidacion);

        }

        objBatch.SetPro(100);
        objBatch.Log(string.Format("Se abrieron {0} recibos.", rec_abiertos));
        if (RowsCount > rec_abiertos)
          objBatch.Log(string.Format("No se pudieron abrir {0} recibos.", RowsCount - rec_abiertos));
        objBatch.Log("Finalizó la reapertura de liquidación.");
      }
      catch (Exception e)
      {
        objBatch.Err(string.Format("Error guardando la liquidación. "+ e.Message));
        NomadBatch.Trace(string.Format("Error guardando la liquidación. " + e.Message + e.StackTrace));
      }
    }

        /*private void ConvertirStringADDO<T>(T coleccionDestino,LegajoEmpresaDDO.DDO_VAR[] ListaDDOVar,Dictionary<string,PropertyInfo> diccionarioDDO) where T : NomadObjectList
        {
            string ddoString = "";

            foreach(LegajoEmpresaDDO.DDO_VAR ddovar in ListaDDOVar)
            {
                if(ddovar.c_tipo == coleccionDestino.GetType().Name)
                {
                    for (int i = 1; i <= 10; i++)
                    {
                        ddoString += diccionarioDDO["d_valor_"+i].GetValue(ddovar, null);
                    }
                    break;
                }
                else continue;
            }

            if (ddoString != "")
            {
                NomadObjectList lista = new NomadObjectList(coleccionDestino.GetType().FullName, new NomadXML(ddoString));
                foreach (NomadObject obj in lista)
                    coleccionDestino.Add(obj);
            }
        }*/

        private void EliminarLSD(int oi_liq_genera)
        {
            if (oi_liq_genera != 0)
            {
                LIQUIDACION liqLSD = LIQUIDACION.Get(oi_liq_genera);
                NomadObjectList lista = NomadEnvironment.GetObjects(LIQUIDACION.Resources.QRY_LSD, "<DATA oi_liq_genero='" + oi_liq_genera + "' />", typeof(Personal_Liquidacion.PERSONAL_LIQ));
                NomadEnvironment.GetCurrentTransaction().Delete(lista);
                NomadEnvironment.GetCurrentTransaction().Delete(liqLSD);
            }

        }

        public void GenerarInt(int idLiquidacion)
        {
            string paso = "Inicio";
            NomadBatch MyBatch = NomadBatch.GetBatch("Liquidacion de Interface", "Liquidacion de Interface");

            try
            {
                NucleusRH.Base.Liquidacion.Tipo_de_Liquidacion.TIPO_LIQ tipoLiq = NucleusRH.Base.Liquidacion.Tipo_de_Liquidacion.TIPO_LIQ.Get("7",false);
            }
            catch (Exception e)
            {
                NomadException ex = NomadException.NewInternalException("RemoteGenerarInt", e);
                ex.SetValue("paso", paso + " - No existen los registros del tipo de liquidacion de interface AFIP ");
                MyBatch.Err("No existe el tipo de Liquidacion de Interface AFIP, contacte con el administrador de sistemas");
                ex.Dump();
                throw ex;
            }

            LIQUIDACION liq = new LIQUIDACION();
            string list_oi_per_emp = "";

            try
            {
                paso = "Crear Liquidacion";
                MyBatch.Log("Crear la Liquidacion...");

                LIQUIDACION liqOriginal = NucleusRH.Base.Liquidacion.Liquidacion.LIQUIDACION.Get(idLiquidacion, false);

                string codLiq = liqOriginal.c_liquidacion;

        //agrega el id de la liquidacion para evitar problemas de duplicación de codigos y se asegura que el resultado final no supere los 25 carecteres
        if ((codLiq + "_" + idLiquidacion.ToString()).Length > 25)
                {
          codLiq = codLiq.Substring(0, 25 - ("_" + idLiquidacion.ToString()).Length) + "_" +  idLiquidacion.ToString();
                }
                else
                {
          codLiq = codLiq + "_" + idLiquidacion.ToString();
                }

                liq.c_liquidacion = codLiq + "_LSD";
                liq.d_titulo = liqOriginal.d_titulo + "_LSD";
                liq.f_liquidacion = liqOriginal.f_liquidacion;
                liq.c_clase = liqOriginal.c_clase;
                if (liq.c_clase == "Q")
                    liq.e_quincena = liqOriginal.e_quincena;
                liq.e_periodo = liqOriginal.e_periodo;
                liq.f_valor = liqOriginal.f_valor;
                liq.f_valorNull = liqOriginal.f_valorNull;
                liq.f_cierre = liqOriginal.f_cierre;
                liq.f_cierreNull = liqOriginal.f_cierreNull;
                liq.f_deposito = liqOriginal.f_deposito;
                liq.f_depositoNull = liqOriginal.f_depositoNull;
                liq.c_mes_deposito = liqOriginal.c_mes_deposito;
                liq.f_pago = liqOriginal.f_pago;
                liq.f_pagoNull = liqOriginal.f_pagoNull;
                liq.c_estado = "I";
                liq.l_retroactivo = liqOriginal.l_retroactivo;
                liq.e_periodo_ret = liqOriginal.e_periodo_ret;
                liq.o_liquidacion = liqOriginal.o_liquidacion;
                liq.l_simulacion = liqOriginal.l_simulacion;
                liq.l_debug = liqOriginal.l_debug;
                liq.l_ajuste = liqOriginal.l_ajuste;
                liq.t_leyenda = liqOriginal.t_leyenda;
                liq.f_f649 = liqOriginal.f_f649;
                liq.f_f649Null = liqOriginal.f_f649Null;
                liq.oi_empresa = liqOriginal.oi_empresa;
                liq.oi_forma_pago = liqOriginal.oi_forma_pago;
                liq.oi_tipo_liq = "7";
                liq.oi_banco = liqOriginal.oi_banco;
                liq.l_confidencial = liqOriginal.l_confidencial;
                liq.oi_liq_genera = liqOriginal.id;

                NomadEnvironment.GetCurrentTransaction().SaveRefresh(liq);

                liqOriginal.oi_liq_genero = liq.id;

                NomadEnvironment.GetCurrentTransaction().SaveRefresh(liqOriginal);

                //Incluir/excluir Legajos
                paso = "Crear Liquidacion";
                MyBatch.Log("Incluir legajos...");
                NomadXML param = NomadEnvironment.QueryNomadXML(NucleusRH.Base.Liquidacion.Personal_Liquidacion.PERSONAL_LIQ.Resources.QRY_OBTENER_LEGAJOS,"<FILTRO oi_liquidacion ='"+liqOriginal.id+"' />").FirstChild();
                LIQUIDACION.AdministrarLegajos(param,liq.id.ToString());

                //Procesar Liquidacion
                paso = "Procesar Liquidacion";
                MyBatch.Log("Procesando Liquidacion...");
                for (NomadXML MyCUR = param.FirstChild().FirstChild(); MyCUR != null; MyCUR = MyCUR.Next())
                {
                    list_oi_per_emp += "," + MyCUR.GetAttr("VALUES");
                }

                StartLiq(liq.id, list_oi_per_emp.Substring(1));
            }
            catch (Exception e)
            {
                NomadException ex = NomadException.NewInternalException("RemoteGenerarInt", e);
                ex.SetValue("oi_liquidacion", liq.id.ToString());
                ex.SetValue("ois_liquidar", list_oi_per_emp);
                ex.SetValue("paso", paso);
                MyBatch.Err("Error durante el Proceso de Liquidacion. Codigo de Error:" + ex.Id);
                ex.Dump();
                throw ex;
            }
        }

    }
}



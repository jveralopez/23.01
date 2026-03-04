using System;
using System.IO;
using System.Collections;

using Nomad.NSystem.Document;
using Nomad.NSystem.Proxy;
using Nomad.NSystem.Base;
using Nomad.NSystem.Functions;

namespace NucleusRH.Base.Presupuesto.Legajos
{
  public partial class LEGAJO
  {
      

      private NomadXML QC=null;
      public NomadXML QueryConceptos
      {
        get {
          //Genero la lista con los periodos por tipos.
          if (this.QC==null)
          {
            this.QC=NomadEnvironment.QueryNomadXML(Resources.CONCEPTOS,"<DATA oi_escenario=\""+this.oi_escenario+"\" />").FirstChild();
            NomadEnvironment.GetTrace().Info("QC:"+this.QC.ToString());
          }

          return this.QC;
        }
      }

      //Obtener un Legajo (SI NO EXISTE CREARLO)
      public static LEGAJO Obtener(NucleusRH.Base.Presupuesto.Escenarios.ESCENARIO Escenario, NomadXML datos)
      {
        string oi_legajo=NomadEnvironment.QueryNomadXML(Resources.LEGAJO,"<DATA e_numero_legajo=\""+datos.GetAttr("e_numero_legajo")+"\" oi_escenario=\""+Escenario.Id+"\" />").FirstChild().GetAttr("id");
        if (oi_legajo=="")
          return Crear(Escenario, datos);

        LEGAJO retval=LEGAJO.Get(oi_legajo);
        NomadEnvironment.GetTrace().Info("Datos "+datos.ToString()+"... ");

        if (datos.GetAttr("simple")=="1")
        {
          if (datos.GetAttr("f_egreso")!="") retval.f_egreso=datos.GetAttrDateTime("f_egreso");
          retval.l_error         =false;
        } else
        {
          retval.d_legajo        =datos.GetAttr("d_legajo");
          retval.f_ingreso       =datos.GetAttrDateTime("f_ingreso");
          retval.f_egreso        =datos.GetAttrDateTime("f_egreso");
          retval.f_egresoNull    =datos.GetAttr("f_egreso")==""?true:false;
          retval.f_antiguedad    =datos.GetAttrDateTime("f_antiguedad");
          retval.f_antiguedadNull=datos.GetAttr("f_antiguedad")==""?true:false;
          retval.o_legajo        =datos.GetAttr("o_legajo");
          retval.l_error         =false;
        }

        return retval;
      }

      //Crear un Legajo Generico
      public static LEGAJO Crear(NucleusRH.Base.Presupuesto.Escenarios.ESCENARIO Escenario, NomadXML datos)
      {
        string key;
        NomadXML cur;
        NomadXML CONC;
        NomadXML DETT;
        string formula;
        LEGAJO retval=new LEGAJO();
        Hashtable tipos=new Hashtable();

        NomadEnvironment.GetTrace().Info("Valore Iniciales....");
        retval.oi_escenario    =Escenario.Id;
        retval.e_posicion      =(datos.GetAttr("e_numero_legajo_AUX")==""?datos.GetAttrInt("e_numero_legajo"):datos.GetAttrInt("e_numero_legajo_AUX"));
        retval.oi_grupo        =datos.GetAttr("oi_grupo");
        retval.e_numero_legajo =datos.GetAttrInt("e_numero_legajo");
        retval.d_legajo        =datos.GetAttr("d_legajo");
        retval.f_ingreso       =datos.GetAttrDateTime("f_ingreso");
        retval.f_egreso        =datos.GetAttrDateTime("f_egreso");
        retval.f_egresoNull    =datos.GetAttr("f_egreso")==""?true:false;
        retval.f_antiguedad    =datos.GetAttrDateTime("f_antiguedad");
        retval.f_antiguedadNull=datos.GetAttr("f_antiguedad")==""?true:false;
        retval.o_legajo        =datos.GetAttr("o_legajo");
        retval.l_error         =false;

        //Genero la lista con los periodos por tipos.
        NomadEnvironment.GetTrace().Info("Periodos por Tipos....");
        for (cur=retval.QueryConceptos.FindElement("TIPO").FirstChild(); cur!=null; cur=cur.Next())
          tipos.Add(cur.GetAttr("oi_periodo_fiscal")+"_"+cur.GetAttr("oi_concepto"), cur.GetAttr("d_formula"));
        DETT=retval.QueryConceptos.FindElement("DETT");
        CONC=retval.QueryConceptos.FindElement("CONC");

        //Recorro los Detalles
        NomadEnvironment.GetTrace().Info("Recorro los Detalles....");
        foreach (NucleusRH.Base.Presupuesto.Escenarios.DETALLE_PRES DET in Escenario.DETALLES)
        {
          //Recorro los Conceptos
          for (cur=CONC.FirstChild(); cur!=null; cur=cur.Next())
          {
            //Obtengo la formula
            key=DET.oi_periodo_fiscal+"_"+cur.GetAttr("oi_concepto");
            if (tipos.ContainsKey(key)) formula=(string)tipos[key];
                                   else formula=cur.GetAttr("d_formula");

            //Creo el Concepto
            DET_CONCEPTO newCONC=new DET_CONCEPTO();
            newCONC.oi_concepto    =cur.GetAttr("oi_concepto");
            newCONC.c_det_concepto =cur.GetAttr("e_concepto");
            newCONC.oi_detalle_pres=DET.Id;
            newCONC.oi_modalidad   =datos.GetAttr("oi_modalidad");
            newCONC.oi_categoria   =datos.GetAttr("oi_categoria");
            newCONC.c_ultimo_nivel =datos.GetAttr("c_ultimo_nivel");
            newCONC.n_valor_aux    =0;
            newCONC.n_valor_final  =0;
            newCONC.d_formula      =formula;
            newCONC.l_bloqueada    =false;
            newCONC.d_mensaje      ="Sin Calcular";

            retval.DET_CONCEPTOS.Add(newCONC);
          }
        }

        return retval;
      }

      //Crear un Legajo Generico
      public void Reiniciar()
      {
        NomadXML CONC,DETT,cur;
        string key,formula;

        Hashtable tipos=new Hashtable();

        //Genero la lista con los periodos por tipos.
        NomadEnvironment.GetTrace().Info("Periodos por Tipos....");
        for (cur=this.QueryConceptos.FindElement("TIPO").FirstChild(); cur!=null; cur=cur.Next())
          tipos.Add(cur.GetAttr("oi_periodo_fiscal")+"_"+cur.GetAttr("oi_concepto"), cur.GetAttr("d_formula"));
        DETT=this.QueryConceptos.FindElement("DETT");
        CONC=this.QueryConceptos.FindElement("CONC");

        //Recorro los Detalles
        NomadEnvironment.GetTrace().Info("Recorro los Detalles....");
        foreach (NucleusRH.Base.Presupuesto.Escenarios.DETALLE_PRES DET in this.Getoi_escenario().DETALLES)
        {
          //Recorro los Conceptos
          for (cur=CONC.FirstChild(); cur!=null; cur=cur.Next())
          {
            //Obtengo la formula
            key=DET.oi_periodo_fiscal+"_"+cur.GetAttr("oi_concepto");
            if (tipos.ContainsKey(key)) formula=(string)tipos[key];
                                   else formula=cur.GetAttr("d_formula");

            //Reseteo el Concepto
            foreach (DET_CONCEPTO newCONC in this.DET_CONCEPTOS)
            if (newCONC.oi_concepto==cur.GetAttr("oi_concepto") && newCONC.oi_detalle_pres==DET.Id && !newCONC.l_bloqueada)
              newCONC.d_formula=formula;
          }
        }
      }

      //Aplicar una Accion
      public void Aplicar(NucleusRH.Base.Presupuesto.Acciones.ACCION acc)
      {
        string newForm;
        NomadXML DETT=null;

        foreach (DET_CONCEPTO conc in this.DET_CONCEPTOS)
          if (conc.oi_detalle_pres==acc.oi_detalle_pres)
            foreach (NucleusRH.Base.Presupuesto.Acciones.ACC_CONCEPTO acc_conc in acc.ACC_CONC)
              if (conc.oi_concepto==acc_conc.oi_concepto)
              {
                    //Genero la lista con los periodos por tipos.
                    if (DETT==null)
                      DETT=this.QueryConceptos.FindElement("DETT");

                    if (!conc.l_bloqueada)
                    {
                      newForm=acc.d_formula;
                      if (acc.d_formula.Contains("{Periodo}"))
                        newForm=newForm.Replace("{Periodo}",DETT.FindElement2("ROW","oi_detalle_pres",acc.oi_detalle_pres).GetAttr("e_periodo"));
                      if (acc.d_formula.Contains("{Base}"))
                        newForm=newForm.Replace("{Base}",DETT.FindElement2("ROW","oi_detalle_pres",acc.oi_base).GetAttr("e_periodo"));
                      if (acc.d_formula.Contains("{Valor}")) 
                        newForm=newForm.Replace("{Valor}",Nomad.NSystem.Functions.StringUtil.dbl2str(acc_conc.n_valorNull?acc.n_valor:acc_conc.n_valor));
                      if (acc.d_formula.Contains("@")) 
                        newForm=newForm.Replace("@",conc.d_formula);

                      conc.d_formula     =newForm;
                    }
              }
      }

      //Aplicar un Valor
      public void Aplicar(string oi_detalle_pres, Hashtable ConcValor, string oi_modalidad, string oi_categoria, string c_ultimo_nivel)
      {
        foreach (DET_CONCEPTO conc in this.DET_CONCEPTOS)
          if (conc.oi_detalle_pres==oi_detalle_pres)
            foreach (string oi_concepto in ConcValor.Keys)
              if (conc.oi_concepto==oi_concepto)
              {
                conc.d_formula=(string)ConcValor[oi_concepto];
                conc.l_bloqueada=true;
                if (oi_modalidad  !="") conc.oi_modalidad  =oi_modalidad;
                if (oi_categoria  !="") conc.oi_categoria  =oi_categoria;
                if (c_ultimo_nivel!="") conc.c_ultimo_nivel=c_ultimo_nivel;
              }
      }

      //Actualiza todos los valores Finales
      public void Actualizar()
      {
        bool isOK=true;
        bool change;
        int  veces;
        double ValorPrev, ValorNuevo;

        //Calculo los Valores simples
         NomadEnvironment.GetTrace().Info("Calculo los Valores simples...");
        foreach (DET_CONCEPTO fnd in this.DET_CONCEPTOS)
          fnd.Load(this.QueryConceptos);
        foreach (NOV_LEGAJO nov in this.NOV_LEGAJO)
          nov.Load(this.QueryConceptos);

        //Recorro todos los conceptos y los calculos, si alguno cambio vuelve a recorrer.
        veces=100;
         do {
           NomadEnvironment.GetTrace().Info("Recorro ("+veces.ToString()+")...");
          change=false;
          foreach (DET_CONCEPTO conc in this.DET_CONCEPTOS)
          {
            ValorPrev =conc.n_valor_final;
            try {
              ValorNuevo=Evaluar(conc);

              conc.d_mensaje="";
              if (ValorPrev!=ValorNuevo)
              {
                conc.n_valor_final =ValorNuevo;
                change=true;
              }
            } catch(Exception E)
            {
              conc.d_mensaje=E.Message;
              isOK=false;
            }
          }
          veces--;
        } while (change && veces>=0);

        this.l_error=!isOK;
      }

      static Nomad.NSystem.Functions.ExprEngine MyENGINE = null;

      public static Nomad.NSystem.Functions.ExprEngine GetExprEngine()
      {
        if (MyENGINE==null)
        {
          MyENGINE=new Nomad.NSystem.Functions.ExprEngine();
          lock(MyENGINE)
          {
              MyENGINE.SetDef("NUMBER" , "( '+' '-' )? /d+ [ '.' /d* ] ");
              MyENGINE.SetDef("NUMPOR", "NUMBER '%'");
              MyENGINE.SetDef("BOOL"  , " ( 'true' 'false' EGRESO INGRESO INGRESOA ) ");
              MyENGINE.SetDef("STRING" , " '\\\"' !\"!* '\\\"' ");
              MyENGINE.SetDef("PER1"   , " /d/d '/' /d/d/d/d ");
              MyENGINE.SetDef("PER2"   , " /d/d '/' /d/d ");
              MyENGINE.SetDef("PER3"   , " /d/d ");
              MyENGINE.SetDef("PER4"   , " ('ENE' 'FEB' 'MAR' 'ABR' 'MAY' 'JUN' 'JUL' 'AGO' 'SEP' 'OCT' 'NOV' 'DIC') ");
              MyENGINE.SetDef("PER5", " ('#1' '#2' '#3' '#4' '#5' '#6') ");//EL sharp significa "en el periodo actual". El número equivale al mes del periodo actual.EJ #1 puede ser 'ENE' o 'JUL', #2 'FEB' o 'AGO'
              MyENGINE.SetDef("PERIODO", " '\\'' ( PER1 PER2 PER3 PER4 PER5)  '\\'' ");

              MyENGINE.SetDef("OPERATION_EXP", " ( '^' ) ");
              MyENGINE.SetDef("OPERATION_FAC", " ( '*' '/' '%' '\\\\' ) ");
              MyENGINE.SetDef("OPERATION_SUM", " ( '+' '-' ) ");
              MyENGINE.SetDef("OPERATION_MAX", " ( '<<' '>>' )");
              MyENGINE.SetDef("OPERATION_CMP", " ( '>=' '<=' '>' '<' '=' '!=' )");
              MyENGINE.SetDef("OPERATION_BOL", " ( '&' '|' )");

              MyENGINE.SetDef("VALOR", "/s* ( NUMPOR NUMBER PERIODO FUNC VAR { '(' EXPR_VALOR ')' } ) /s*");
              MyENGINE.SetDef("EXP", " /s* VALOR /s* { OPERATION_EXP /s* VALOR /s* }+");
              MyENGINE.SetDef("FAC", " /s* ( EXP VALOR ) /s* { OPERATION_FAC /s* ( EXP VALOR ) /s* }+");
              MyENGINE.SetDef("SUM", " /s* ( FAC EXP VALOR ) /s* { OPERATION_SUM /s* ( FAC EXP VALOR ) /s* }+");
              MyENGINE.SetDef("MAX", " /s* ( SUM FAC EXP VALOR ) /s* { OPERATION_MAX /s* ( SUM FAC EXP VALOR ) /s* }+");
              MyENGINE.SetDef("EXPR_VALOR", " /s* ( IIF MAX SUM FAC EXP VALOR ) /s*");

              MyENGINE.SetDef("CMP", "/s*  EXPR_VALOR /s* OPERATION_CMP /s* EXPR_VALOR /s*");

              MyENGINE.SetDef("VALOR_BOOL"    , "/s* ( CMP BOOL { '(' EXPR_BOOL ')' } ) /s*");
              MyENGINE.SetDef("VALOR_BOOL_NEG", "/s* '!' VALOR_BOOL /s*");
              MyENGINE.SetDef("BOL"           , "/s* ( VALOR_BOOL VALOR_BOOL_NEG ) /s* { OPERATION_BOL /s* ( VALOR_BOOL VALOR_BOOL_NEG ) /s* }+");

              MyENGINE.SetDef("EXPR_BOOL", "/s* ( BOL VALOR_BOOL ) /s*");

              MyENGINE.SetDef("IIF", " /s* 'iif' /s* '(' EXPR_BOOL ',' EXPR_VALOR ',' EXPR_VALOR ')' /s* ");

              MyENGINE.SetDef("FUNC", " ( ABS SIGN ROUND CEIL FLOOR TRUNC ANTIGUEDAD VACACIONES DIASTRABAJADOS DIASTRABAJADOSX CONTR ) ");
              MyENGINE.SetDef("ABS", " /s* 'ABS' /s* '(' EXPR_VALOR  ')' /s* ");
              MyENGINE.SetDef("SIGN", " /s* 'SGN' /s* '(' EXPR_VALOR ')' /s* ");
              MyENGINE.SetDef("ROUND", " /s* 'ROUND' /s* '(' EXPR_VALOR ',' EXPR_VALOR ')' /s* ");
              MyENGINE.SetDef("CEIL", " /s* 'CEIL'  /s* '(' EXPR_VALOR ')' /s* ");
              MyENGINE.SetDef("FLOOR", " /s* 'FLOOR' /s* '(' EXPR_VALOR ')' /s* ");
              MyENGINE.SetDef("TRUNC", " /s* 'TRUNC' /s* '(' EXPR_VALOR ')' /s* ");
              MyENGINE.SetDef("ANTIGUEDAD", " /s* 'ANTIGUEDAD' /s* '(' ANTIGUEDAD_MODO ')' /s* ");
              MyENGINE.SetDef("EGRESO", " /s* 'EGRESO' /s* '(' /s* ')' /s* ");
              MyENGINE.SetDef("INGRESO", " /s* 'INGRESO' /s* '(' /s* ')' /s* ");
              MyENGINE.SetDef("INGRESOA", " /s* 'INGRESOA' /s* '(' /s* ')' /s* ");
              MyENGINE.SetDef("VACACIONES", " /s* 'VACACIONES' /s* '(' EXPR_VALOR? ')' /s* ");
              MyENGINE.SetDef("CONTR", " /s* 'CONTR' /s* '(' CONTR_MODO  ',' /s* STRING /s* ')' /s* ");

              MyENGINE.SetDef("DIASTRABAJADOS" , " /s* 'DIASTRABAJADOS' /s* '(' EXPR_VALOR ',' EXPR_VALOR ')' /s* ");
              MyENGINE.SetDef("DIASTRABAJADOSX", " /s* 'DIASTRABAJADOS30' /s* '(' EXPR_VALOR ',' EXPR_VALOR ')' /s* ");

              MyENGINE.SetDef("ANTIGUEDAD_MODO", "/s* ( 'ANOS' 'MESES' 'DIAS' ) /s*");
              MyENGINE.SetDef("CONTR_MODO", "/s* ( 'PORC' 'TOPE' ) /s*");

              MyENGINE.SetDef("VAR", " /s* (VAR5 VAR4 VAR3 VAR2 AUX5 AUX4 AUX3 AUX2 AUX1 NOV4 NOV3 CEL5 CEL4 CEL3 CEL2 CEL1 )  /s* ");

              MyENGINE.SetDef("VAR2", " /s* 'V' /s* '[' EXPR_VALOR ']' /s* ");
              MyENGINE.SetDef("VAR3", " /s* 'V' /s* '[' STRING ']' /s* ");
              MyENGINE.SetDef("VAR4", " /s* 'V' /s* '[' EXPR_VALOR ',' /s* STRING /s* ']' /s* ");
              MyENGINE.SetDef("VAR5", " /s* 'V' /s* '[' EXPR_VALOR ',' /s* STRING /s* ',' /s* STRING /s* ']' /s* ");

              MyENGINE.SetDef("AUX1", " /s* 'A' /s* ");
              MyENGINE.SetDef("AUX2", " /s* 'A' /s* '[' EXPR_VALOR ']' /s* ");
              MyENGINE.SetDef("AUX3", " /s* 'A' /s* '[' STRING ']' /s* ");
              MyENGINE.SetDef("AUX4", " /s* 'A' /s* '[' EXPR_VALOR ',' /s* STRING /s* ']' /s* ");
              MyENGINE.SetDef("AUX5", " /s* 'A' /s* '[' EXPR_VALOR ',' /s* STRING /s* ',' /s* STRING /s* ']' /s* ");

              MyENGINE.SetDef("NOV3", " /s* 'N' /s* '[' STRING ']' /s* ");
              MyENGINE.SetDef("NOV4", " /s* 'N' /s* '[' EXPR_VALOR ',' /s* STRING /s* ']' /s* ");

              MyENGINE.SetDef("CEL1", " /s* 'C' /s* '[' /s* CELPROP /s* ']' /s* ");
              MyENGINE.SetDef("CEL2", " /s* 'C' /s* '[' /s* CELPROP /s* ',' /s* EXPR_VALOR ']' /s* ");
              MyENGINE.SetDef("CEL3", " /s* 'C' /s* '[' /s* CELPROP /s* ',' STRING ']' /s* ");
              MyENGINE.SetDef("CEL4", " /s* 'C' /s* '[' /s* CELPROP /s* ',' EXPR_VALOR ',' /s* STRING /s* ']' /s* ");
              MyENGINE.SetDef("CEL5", " /s* 'C' /s* '[' /s* CELPROP /s* ',' EXPR_VALOR ',' /s* STRING /s* ',' /s* STRING /s* ']' /s* ");

              MyENGINE.SetDef("CELPROP", " ( 'PERIODO' 'VALOR' 'AUX' )  ");
          }
        }

        return MyENGINE;
    }

    //Ejecuto la evaluacion
    Hashtable CacheFormula=null;
    public double Evaluar(DET_CONCEPTO conc)
    {
        Nomad.NSystem.Functions.ExprEngine MyENGINE=GetExprEngine();
        Nomad.NSystem.Functions.ExprResult MyRESULT;

        if (this.CacheFormula==null)
        {
          this.CacheFormula=(Hashtable)NomadProxy.GetProxy().CacheGetObj("FORMULAS");
          if (this.CacheFormula==null)
          {
            this.CacheFormula=new Hashtable();
            NomadProxy.GetProxy().CacheAdd("FORMULAS", this.CacheFormula);
          }
        }

        if (this.CacheFormula.ContainsKey(conc.d_formula))
        {
          MyRESULT=(Nomad.NSystem.Functions.ExprResult)this.CacheFormula[conc.d_formula];
        } else
        {
          lock(MyENGINE)
          {
            MyRESULT = MyENGINE.CompileExpr("EXPR_VALOR", conc.d_formula);
          }
          this.CacheFormula.Add(conc.d_formula, MyRESULT);
        }

        if (MyRESULT==null)
          throw new NomadException("Syntax Error");
        else
        if (MyRESULT.Data!=conc.d_formula)
          throw new NomadException("Syntax Error");

        return Evaluar_Double(MyRESULT,conc);
    }

    //Ejecuto la evaluacion
    public string Evaluar_String(Nomad.NSystem.Functions.ExprResult MyRESULT, DET_CONCEPTO conc)
    {
        string retval;
        switch (MyRESULT.Type.Name)
        {
            case "STRING":
              retval=MyRESULT.Data.Replace("\\\"", "\"");
              return retval.Substring(1,retval.Length-2);

            default:
                throw new Exception("ERROR INTERNO (Evaluar_String:" + MyRESULT.Type.Name + ")");
        }
    }

    public bool Evaluar_Boolean(Nomad.NSystem.Functions.ExprResult MyRESULT, DET_CONCEPTO conc)
    {
        int    Periodo =conc.Periodo;
        string Tipo    =conc.Tipo;
        string Concepto=conc.Concepto;

        Nomad.NSystem.Functions.ExprResult[] C = MyRESULT.Childs();
        switch (MyRESULT.Type.Name)
        {
            case "EGRESO":
              if (this.f_egresoNull) return false;
              if (C.Length!=0) Periodo=(int)Math.Floor(Evaluar_Double(C[0],conc));

              Periodo=PeriodoConvert(Periodo,conc);
              if (this.f_egreso.Year*100+this.f_egreso.Month==Periodo) return true;
              return false;

            case "INGRESO":
              if (this.f_ingresoNull) return false;
              if (C.Length!=0) Periodo=(int)Math.Floor(Evaluar_Double(C[0],conc));

              Periodo=PeriodoConvert(Periodo,conc);
              if (this.f_ingreso.Year*100+this.f_ingreso.Month==Periodo) return true;
              return false;

            case "INGRESOA":
              if (this.f_ingresoNull) return false;
              if (C.Length!=0) Periodo=(int)Math.Floor(Evaluar_Double(C[0],conc));

              Periodo=PeriodoConvert(Periodo,conc);
              if (this.f_ingreso.Year==Periodo/100) return true;
              return false;

            case "BOOL":
                if (C.Length!=0) return Evaluar_Boolean(C[0], conc);
                            else return (MyRESULT.Data.ToUpper()=="TRUE"?true:false);

            case "VALOR_BOOL":
            case "EXPR_BOOL":
                return Evaluar_Boolean(C[0],conc);

            case "VALOR_BOOL_NEG":
                return !Evaluar_Boolean(C[0],conc);

            case "BOL":
                {
                    bool retval = Evaluar_Boolean(C[0],conc);

                    for (int t = 1; t < C.Length; t += 2)
                    {
                        switch (C[t].Data)
                        {
                            case "&": retval &= Evaluar_Boolean(C[t + 1],conc); break;
                            case "|": retval |= Evaluar_Boolean(C[t + 1],conc); break;
                            default:
                                throw new Exception("ERROR INTERNO (Evaluar_Boolean:" + MyRESULT.Type.Name + ")");
                        }

                    }

                    return retval;
                }

            case "CMP":
                {
                    double retval1 = Evaluar_Double(C[0],conc);
                    double retval2 = Evaluar_Double(C[2],conc);

                    switch (C[1].Data)
                    {
                        case ">": return retval1 > retval2;
                        case "<": return retval1 < retval2;

                        case ">=": return retval1 >= retval2;
                        case "<=": return retval1 <= retval2;

                        case "=": return retval1 == retval2;
                        case "!=": return retval1 != retval2;

                        default:
                            throw new Exception("ERROR INTERNO (Evaluar_Boolean:" + MyRESULT.Type.Name + ")");
                    }
                }

            default:
                throw new Exception("ERROR INTERNO (Evaluar_Boolean:" + MyRESULT.Type.Name + ")");
        }
    }

    public Hashtable ConcByType=null;
    public Hashtable NoveByType=null;

    public int PeriodoConvert(int Periodo, DET_CONCEPTO conc)
    {
      if (Periodo>-100 && Periodo<100)
      {
        DateTime X=new DateTime(conc.Periodo/100,conc.Periodo%100,1);
        X=X.AddMonths(Periodo);
        Periodo=X.Year*100+X.Month;
      }

      return Periodo;
    }

    public DET_CONCEPTO ObtenerConcepto(int Periodo, string Tipo, string Concepto, DET_CONCEPTO conc)
    {
      string Key;
      Periodo=PeriodoConvert(Periodo,conc);

      //Genero la Lista CACHE
      if (this.ConcByType==null)
      {
        this.ConcByType=new Hashtable();

         foreach (DET_CONCEPTO fnd in this.DET_CONCEPTOS)
         {
           Key=fnd.Periodo.ToString()+"_"+fnd.Tipo+"_"+fnd.Concepto;
           this.ConcByType[Key]=fnd;
         }
      }

      //Obtengo la KEY
      Key=Periodo.ToString()+"_"+Tipo+"_"+Concepto;
      if (this.ConcByType.ContainsKey(Key))
        return (DET_CONCEPTO)this.ConcByType[Key];

      throw new NomadException("Celda No Encotrada (CONC:"+Concepto.ToString()+", PER:"+Periodo.ToString()+", TIP:"+Tipo.ToString()+")");
    }

    public double ObtenerConceptoPropiedad(string propname, int Periodo, string Tipo, string Concepto, DET_CONCEPTO conc)
    {
      DET_CONCEPTO MyDET=ObtenerConcepto(Periodo, Tipo, Concepto, conc);

      switch(propname.ToUpper())
      {
        case "AUX":
          return MyDET.n_valor_aux;

        case "VALOR":
          return MyDET.n_valor_final;

        case "PERIODO":
          return this.QueryConceptos.FindElement("DETT").FindElement2("ROW","oi_detalle_pres",MyDET.oi_detalle_pres).GetAttrDouble("e_periodo");
      }

      return 0.0;

    }

    public NOV_LEGAJO ObtenerNovedad(int Periodo, string Tipo, string Novedad, DET_CONCEPTO conc)
    {
      string Key;

      Periodo=PeriodoConvert(Periodo,conc);

      //Genero la Lista CACHE
      if (this.NoveByType==null)
      {
        this.NoveByType=new Hashtable();

        foreach (NOV_LEGAJO fnd in this.NOV_LEGAJO)
        {
          Key=fnd.Periodo.ToString()+"_"+fnd.Tipo+"_"+fnd.Novedad;
          this.NoveByType[Key]=fnd;
        }
      }

      //Obtengo la KEY
      Key=Periodo.ToString()+"_"+Tipo+"_"+Novedad;
      if (this.NoveByType.ContainsKey(Key))
        return (NOV_LEGAJO)this.NoveByType[Key];

      return null;
    }

    public int Antiguedad(string Type, DateTime LastDay)
    {
      int retval=0;

      DateTime FirstDay=this.f_ingreso;
      if (!this.f_antiguedadNull) FirstDay=this.f_antiguedad;

      switch(Type.ToUpper())
      {
        case "ANOS":
            retval=LastDay.Year-FirstDay.Year;
            if (LastDay.Month<FirstDay.Month) retval--;
            if (LastDay.Month==FirstDay.Month && LastDay.Day<FirstDay.Day) retval--;
        break;

        case "DIAS":
            retval=(int)Math.Floor((LastDay-FirstDay).TotalDays);
        break;

        case "MESES":
        int meses_anio = (LastDay.Year - FirstDay.Year) * 12;
        int meses = LastDay.Month - FirstDay.Month;
        int dias = LastDay.Day - FirstDay.Day;

            if (LastDay.Day == DateTime.DaysInMonth(LastDay.Year, LastDay.Month))
            {
                if (FirstDay.Day == 1) meses += 1;
            }
            else
            {
                if (dias < 0) meses -= 1;
            }
            retval = meses_anio + meses;
        break;
      }

      return retval;
    }

    public int DiasTrabajados(int Desde, int Hasta)
    {
      int retval=0;

      //Dia Inicial
      DateTime FirstDay=new DateTime(Desde/100,Desde%100,1);
      if (this.f_antiguedadNull)
      {
        if (this.f_ingreso>FirstDay) FirstDay=this.f_ingreso;
      } else
      {
        if (this.f_antiguedad>FirstDay) FirstDay=this.f_antiguedad;
      }

      //Dia Final
      DateTime LastDay =(new DateTime(Hasta/100,Hasta%100,1)).AddMonths(1).AddDays(-1);
      if (!this.f_egresoNull)
      {
        if (this.f_egreso<LastDay) LastDay=this.f_egreso;
      }

      //Dias Trabajados
      retval=(int)Math.Floor((LastDay-FirstDay).TotalDays);
      if (retval<0) retval=0;

      return retval;
    }

    static public int LastDayOfMonth(DateTime F)
    {
      DateTime retval=new DateTime(F.Year, F.Month, 1);
      retval=retval.AddMonths(1).AddDays(-1);
      return retval.Day;
    }

    public int DiasTrabajados30(int Desde, int Hasta)
    {
      int retval=0;

      //Dia Inicial
      DateTime FirstDay=new DateTime(Desde/100,Desde%100,1);
      if (this.f_antiguedadNull)
      {
        if (this.f_ingreso>FirstDay) FirstDay=this.f_ingreso;
      } else
      {
        if (this.f_antiguedad>FirstDay) FirstDay=this.f_antiguedad;
      }

      //Dia Final
      DateTime LastDay =(new DateTime(Hasta/100,Hasta%100,1)).AddMonths(1).AddDays(-1);
      if (!this.f_egresoNull)
      {
        if (this.f_egreso<LastDay) LastDay=this.f_egreso;
      }

      //Dias Trabajados
      int Meses, DiasI, DiasF;

      Meses=(LastDay.Year*12+LastDay.Month)-(FirstDay.Year*12+FirstDay.Month)+1;
      DiasI=(int)Math.Round((FirstDay.Day-1)*30.0/LastDayOfMonth(FirstDay));
      DiasF=(int)Math.Round((LastDayOfMonth(LastDay)-LastDay.Day)*30.0/LastDayOfMonth(LastDay));

      retval=Meses*30-DiasI-DiasF;
      if (retval<0) retval=0;

      return retval;
    }

    public double Evaluar_Double(Nomad.NSystem.Functions.ExprResult MyRESULT, DET_CONCEPTO conc)
    {
        int    Periodo =conc.Periodo;
        string Tipo    =conc.Tipo;
        string Concepto=conc.Concepto;
        int    Anno    =Periodo/100;
        int    Mes = Periodo % 100;
        DET_CONCEPTO celd;
        NOV_LEGAJO nov;

        Nomad.NSystem.Functions.ExprResult[] C=MyRESULT.Childs();

        switch (MyRESULT.Type.Name)
        {
            case "NUMBER":
                return Nomad.NSystem.Functions.StringUtil.str2dbl(MyRESULT.Data);
            case "PER1":
                return int.Parse(MyRESULT.Data.Substring(3,4))*100+int.Parse(MyRESULT.Data.Substring(0,2));
            case "PER2":
                return Anno*100+int.Parse(MyRESULT.Data.Substring(3, 2)) * 100 + int.Parse(MyRESULT.Data.Substring(0, 2));
            case "PER3":
                return Anno*100+int.Parse(MyRESULT.Data);
            case "PER4":
                switch(MyRESULT.Data.ToUpper())
                {
                    case "ENE":
                        return Anno*100+1;
                    case "FEB":
                        return Anno*100+2;
                    case "MAR":
                        return Anno*100+3;
                    case "ABR":
                        return Anno*100+4;
                    case "MAY":
                        return Anno*100+5;
                    case "JUN":
                        return Anno*100+6;
                    case "JUL":
                        return Anno*100+7;
                    case "AGO":
                        return Anno*100+8;
                    case "SEP":
                        return Anno*100+9;
                    case "OCT":
                        return Anno*100+10;
                    case "NOV":
                        return Anno*100+11;
                    case "DIC":
                        return Anno*100+12;
                    default:
                        throw new Exception("ERROR INTERNO (Evaluar_Double:" + MyRESULT.Type.Name + ")");
                }
            case "PER5":
                switch (MyRESULT.Data.ToUpper())
                {
                    case "#1":
                        if (Mes <= 6)
                            return Anno * 100 + 1;
                        else
                            return Anno * 100 + 7;
                    case "#2":
                        if (Mes <= 6)
                            return Anno * 100 + 2;
                        else
                            return Anno * 100 + 8;
                    case "#3":
                        if (Mes <= 6)
                            return Anno * 100 + 3;
                        else
                            return Anno * 100 + 9;
                    case "#4":
                        if (Mes <= 6)
                            return Anno * 100 + 4;
                        else
                            return Anno * 100 + 10;
                    case "#5":
                        if (Mes <= 6)
                            return Anno * 100 + 5;
                        else
                            return Anno * 100 + 11;
                    case "#6":
                        if (Mes <= 6)
                            return Anno * 100 + 6;
                        else
                            return Anno * 100 + 12;
                    default:
                        throw new Exception("ERROR INTERNO (Evaluar_Double:" + MyRESULT.Type.Name + ")");
                }

            case "NUMPOR":
                return Evaluar_Double(C[0],conc)/100.0;

            case "PERIODO":
            case "VALOR":
            case "EXPR_VALOR":
            case "FUNC":
            case "VAR":
                return Evaluar_Double(C[0],conc);

            case "EXP":
            case "FAC":
            case "SUM":
            case "MAX":
                {
                    double retval = Evaluar_Double(C[0],conc);
                    double retvalAux;

                    for (int t = 1; t < C.Length; t += 2)
                    {
                        switch(C[t].Data)
                        {
                            case "+": retval+=Evaluar_Double(C[t+1],conc); break;
                            case "-": retval-=Evaluar_Double(C[t+1],conc); break;

                            case "*": retval*=Evaluar_Double(C[t+1],conc); break;
                            case "/": retval/=Evaluar_Double(C[t+1],conc); break;
                            case "%":  retval=(int)Math.Floor(retval) % (int)Math.Floor(Evaluar_Double(C[t+1],conc)); break;
                            case "\\": retval=Math.Floor(retval/Evaluar_Double(C[t+1],conc)); break;

                            case "^": retval = Math.Pow(retval, Evaluar_Double(C[t+1],conc)); break;

                            case ">>": retvalAux = Evaluar_Double(C[t+1],conc);
                                retval = (retvalAux > retval ? retvalAux : retval);
                                break;

                            case "<<": retvalAux = Evaluar_Double(C[t+1],conc);
                                retval = (retvalAux < retval ? retvalAux : retval);
                                break;

                            default:
                                throw new Exception("ERROR INTERNO (Evaluar_Double:" + MyRESULT.Type.Name + ")");
                        }

                    }

                    return retval;
                }

            case "IIF":
                if (Evaluar_Boolean(C[0],conc)) return Evaluar_Double(C[1],conc);
                                           else return Evaluar_Double(C[2],conc);

            case "ABS": return Math.Abs(Evaluar_Double(C[0],conc));
            case "SIGN": return Math.Sign(Evaluar_Double(C[0],conc));
            case "ROUND": return Math.Round(Evaluar_Double(C[0],conc), (int)Math.Floor(Evaluar_Double(C[1],conc)));
            case "CEIL": return Math.Ceiling(Evaluar_Double(C[0],conc));
            case "FLOOR": return Math.Floor(Evaluar_Double(C[0],conc));
            case "TRUNC": return Math.Truncate(Evaluar_Double(C[0],conc));

            case "ANTIGUEDAD":
              Periodo=PeriodoConvert(Periodo,conc);
              return Antiguedad(C[0].Data, (new DateTime(Periodo/100,Periodo%100,1)).AddMonths(1).AddDays(-1));

            case "DIASTRABAJADOS":
            {
              int PeriodoDesde=PeriodoConvert((int)Math.Floor(Evaluar_Double(C[0],conc)),conc);
              int PeriodoHasta=PeriodoConvert((int)Math.Floor(Evaluar_Double(C[1],conc)),conc);
              return DiasTrabajados(PeriodoDesde, PeriodoHasta);
            }

            case "DIASTRABAJADOSX":
            {
              int PeriodoDesde=PeriodoConvert((int)Math.Floor(Evaluar_Double(C[0],conc)),conc);
              int PeriodoHasta=PeriodoConvert((int)Math.Floor(Evaluar_Double(C[1],conc)),conc);
              return DiasTrabajados30(PeriodoDesde, PeriodoHasta);
            }

            case "VACACIONES":
            {
              if (C.Length!=0) Periodo=(int)Math.Floor(Evaluar_Double(C[0],conc));
              Periodo=PeriodoConvert(Periodo,conc);
              int Ant=Antiguedad(C[0].Data, (new DateTime(Periodo/100,Periodo%100,1)).AddMonths(1).AddDays(-1));

              for (NomadXML cur=this.QueryConceptos.FindElement("VACA").FirstChild(); cur!=null; cur=cur.Next())
              if (cur.GetAttrInt("e_desde")<=Ant && cur.GetAttrInt("e_hasta")>Ant)
                return cur.GetAttrInt("e_cant");

              throw new Exception("ERROR en la TABLA de VACACIONES, Antiguedad '"+Ant.ToString()+"' no encotrada.");
            }

            case "CONTR":
            {
              string ConcName=Evaluar_String(C[1],conc);
              for (NomadXML cur=this.QueryConceptos.FindElement("CONT").FirstChild(); cur!=null; cur=cur.Next())
              {
                if (cur.GetAttr("oi_modalidad")==conc.oi_modalidad && cur.GetAttr("e_concepto")==ConcName)
                {

                  switch(C[0].Data.ToUpper())
                  {
                    case "PORC":
                      return cur.GetAttrDouble("n_porcentaje");

                    case "TOPE":
                      return cur.GetAttrDouble("n_tope");
                  }
                }
              }
              return 0;
            }

            case "VAR2":
              Periodo =(int)Math.Floor(Evaluar_Double(C[0],conc));
              celd=ObtenerConcepto(Periodo, Tipo, Concepto, conc);
              return celd.n_valor_final;

            case "VAR3":
              Concepto=Evaluar_String(C[0],conc);
              celd=ObtenerConcepto(Periodo, Tipo, Concepto, conc);
              return celd.n_valor_final;

            case "VAR4":
              Periodo =(int)Math.Floor(Evaluar_Double(C[0],conc));
              Concepto=Evaluar_String(C[1],conc);
 
              try
              {
                  celd = ObtenerConcepto(Periodo, Tipo, Concepto, conc);
              }
              catch (Exception e)
              {
                  celd = DET_CONCEPTO.New();
                  celd.n_valor_final = 0;
              }

              
              return celd.n_valor_final;

            case "VAR5":
              Periodo =(int)Math.Floor(Evaluar_Double(C[0],conc));
              Concepto=Evaluar_String(C[1],conc);
              Tipo    =Evaluar_String(C[2],conc);
              celd=ObtenerConcepto(Periodo, Tipo, Concepto, conc);
              return celd.n_valor_final;

            case "AUX1":
              return conc.n_valor_aux;

            case "AUX2":
              Periodo =(int)Math.Floor(Evaluar_Double(C[0],conc));
              celd=ObtenerConcepto(Periodo, Tipo, Concepto, conc);
              return celd.n_valor_aux;

            case "AUX3":
              Concepto=Evaluar_String(C[0],conc);
              celd=ObtenerConcepto(Periodo, Tipo, Concepto, conc);
              return celd.n_valor_aux;

            case "AUX4":
              Periodo =(int)Math.Floor(Evaluar_Double(C[0],conc));
              Concepto=Evaluar_String(C[1],conc);
              celd=ObtenerConcepto(Periodo, Tipo, Concepto, conc);
              return celd.n_valor_aux;

            case "AUX5":
              Periodo =(int)Math.Floor(Evaluar_Double(C[0],conc));
              Concepto=Evaluar_String(C[1],conc);
              Tipo    =Evaluar_String(C[2],conc);
              celd=ObtenerConcepto(Periodo, Tipo, Concepto, conc);
              return celd.n_valor_aux;

            case "CEL1":
              return ObtenerConceptoPropiedad(C[0].Data, Periodo, Tipo, Concepto, conc);

            case "CEL2":
              Periodo =(int)Math.Floor(Evaluar_Double(C[1],conc));
              return ObtenerConceptoPropiedad(C[0].Data, Periodo, Tipo, Concepto, conc);

            case "CEL3":
              Concepto=Evaluar_String(C[1],conc);
              return ObtenerConceptoPropiedad(C[0].Data, Periodo, Tipo, Concepto, conc);

            case "CEL4":
              Periodo =(int)Math.Floor(Evaluar_Double(C[1],conc));
              Concepto=Evaluar_String(C[2],conc);
              return ObtenerConceptoPropiedad(C[0].Data, Periodo, Tipo, Concepto, conc);

            case "CEL5":
              Periodo =(int)Math.Floor(Evaluar_Double(C[1],conc));
              Concepto=Evaluar_String(C[2],conc);
              Tipo    =Evaluar_String(C[3],conc);
              return ObtenerConceptoPropiedad(C[0].Data, Periodo, Tipo, Concepto, conc);

            case "NOV1":
              nov=ObtenerNovedad(Periodo, Tipo, Concepto, conc);
              return nov==null?0:nov.n_valor;

            case "NOV2":
              Periodo =(int)Math.Floor(Evaluar_Double(C[0],conc));
              nov=ObtenerNovedad(Periodo, Tipo, Concepto, conc);
              return nov==null?0:nov.n_valor;

            case "NOV3":
              Concepto=Evaluar_String(C[0],conc);
              nov=ObtenerNovedad(Periodo, Tipo, Concepto, conc);
              return nov==null?0:nov.n_valor;

            case "NOV4":
              Periodo =(int)Math.Floor(Evaluar_Double(C[0],conc));
              Concepto=Evaluar_String(C[1],conc);
              nov=ObtenerNovedad(Periodo, Tipo, Concepto, conc);
              return nov==null?0:nov.n_valor;

            case "NOV5":
              Periodo =(int)Math.Floor(Evaluar_Double(C[0],conc));
              Concepto=Evaluar_String(C[1],conc);
              Tipo    =Evaluar_String(C[2],conc);
              nov=ObtenerNovedad(Periodo, Tipo, Concepto, conc);
              return nov==null?0:nov.n_valor;

            default:
                throw new Exception("ERROR INTERNO (Evaluar_Double:" + MyRESULT.Type.Name + ")");
        }

        return 0;
    }

    public static NucleusRH.Base.Presupuesto.Legajos.LEGAJO CrearLegajo(Nomad.NSystem.Proxy.NomadXML xmlParam)
    {


        //Muevo el Elemento?
        if (xmlParam.FirstChild().Name == "ROOT") xmlParam = xmlParam.FirstChild();


        NomadXML DATA = xmlParam.FindElement("DATA");
        NucleusRH.Base.Presupuesto.Legajos.LEGAJO retval;
        Hashtable ConcValor;
        NucleusRH.Base.Presupuesto.Escenarios.ESCENARIO Escenario = NucleusRH.Base.Presupuesto.Escenarios.ESCENARIO.Get(DATA.GetAttr("oi_escenario"));

        //Creo el LEGAJO 
        NomadEnvironment.GetTrace().Info("PASO 1 - Creo el LEGAJO....");
        retval = Crear(Escenario, DATA);

        //Limpiar los MESES
        NomadEnvironment.GetTrace().Info("PASO 2 - Creo la LISTA de conceptos....");
        ConcValor = new Hashtable();
        for (NomadXML cur = retval.QueryConceptos.FindElement("CONC").FirstChild(); cur != null; cur = cur.Next())
            ConcValor[cur.GetAttr("oi_concepto")] = "0";

        //Limpio todos los detalle anteriores a la fecha inicial (ingreso al presupuesto)
        NomadXML DETT = retval.QueryConceptos.FindElement("DETT");
        int PeriodoDesde = DETT.FindElement2("ROW", "oi_detalle_pres", DATA.GetAttr("oi_detalle_desde")).GetAttrInt("e_periodo");
        int MyPeriodo;
        int PeriodoEgreso;

        NomadEnvironment.GetTrace().Info("PASO 3 - Recorro los detalles anteriores al inicio '" + PeriodoDesde.ToString() + "' y los limpio....");
        foreach (NucleusRH.Base.Presupuesto.Escenarios.DETALLE_PRES DET in Escenario.DETALLES)
        {
            MyPeriodo = DETT.FindElement2("ROW", "oi_detalle_pres", DET.Id).GetAttrInt("e_periodo");
            if (MyPeriodo < PeriodoDesde)
            {
                NomadEnvironment.GetTrace().Info("Limpiar Periodo " + MyPeriodo.ToString() + "....");
                retval.Aplicar(DET.Id, ConcValor, "", "", "");
            }
        }

        //Limpio todos los detalles posteriores a la fecha de EGRESO
        if (DATA.GetAttr("f_egreso") != "")
        {
            PeriodoEgreso = int.Parse(DATA.GetAttr("f_egreso").Substring(0, 6));
            NomadEnvironment.GetTrace().Info("PASO 4 - Recorro los detalles posteriores al Egreso '" + PeriodoEgreso.ToString() + "' los limpio....");

            foreach (NucleusRH.Base.Presupuesto.Escenarios.DETALLE_PRES DET in Escenario.DETALLES)
            {
                MyPeriodo = DETT.FindElement2("ROW", "oi_detalle_pres", DET.Id).GetAttrInt("e_periodo");
                if (PeriodoEgreso < MyPeriodo)
                {
                    NomadEnvironment.GetTrace().Info("Limpiar Periodo " + MyPeriodo.ToString() + "....");
                    retval.Aplicar(DET.Id, ConcValor, "", "", "");
                }
            }
        }

        //Valorizar el MES INICIAL                      
        NomadEnvironment.GetTrace().Info("PASO 5 - Valorizo el Periodo....");
        if (xmlParam.FindElement("CONCEPTOS") != null)
        {
            ConcValor = new Hashtable();
            for (NomadXML cur = xmlParam.FindElement("CONCEPTOS").FirstChild(); cur != null; cur = cur.Next())
                ConcValor[cur.GetAttr("oi_concepto")] = cur.GetAttr("n_valor");

            retval.Aplicar(DATA.GetAttr("oi_detalle_desde"), ConcValor, "", "", "");
        }

        //Actualiza los Valores
        NomadEnvironment.GetTrace().Info("PASO 6 - Recalculo el LEGAJO....");
        retval.Actualizar();

        NomadEnvironment.GetTrace().Info("PASO 7 - FIN....");
        return retval;

    }
    public static NucleusRH.Base.Presupuesto.Legajos.LEGAJO ModificarLegajo(Nomad.NSystem.Proxy.NomadXML xmlParam)
    {


        //Muevo el Elemento?
        if (xmlParam.FirstChild().Name == "ROOT") xmlParam = xmlParam.FirstChild();


        NomadXML DATA = xmlParam.FindElement("DATA");
        NucleusRH.Base.Presupuesto.Legajos.LEGAJO retval;
        Hashtable ConcValor;
        NucleusRH.Base.Presupuesto.Escenarios.ESCENARIO Escenario = NucleusRH.Base.Presupuesto.Escenarios.ESCENARIO.Get(DATA.GetAttr("oi_escenario"));

        //Creo el LEGAJO 
        NomadEnvironment.GetTrace().Info("PASO 1 - Creo el LEGAJO....");
        retval = Obtener(Escenario, DATA);

        //Limpiar los MESES
        NomadEnvironment.GetTrace().Info("PASO 2 - Creo la LISTA de conceptos....");
        ConcValor = new Hashtable();
        for (NomadXML cur = retval.QueryConceptos.FindElement("CONC").FirstChild(); cur != null; cur = cur.Next())
            ConcValor[cur.GetAttr("oi_concepto")] = "0";

        NomadXML DETT = retval.QueryConceptos.FindElement("DETT");
        //Limpio todos los detalles posteriores a la fecha de EGRESO
        int MyPeriodo;
        int PeriodoEgreso;

        if (DATA.GetAttr("f_egreso") != "")
        {
            PeriodoEgreso = int.Parse(DATA.GetAttr("f_egreso").Substring(0, 6));
            NomadEnvironment.GetTrace().Info("PASO 3 - Recorro los detalles posteriores al Egreso '" + PeriodoEgreso.ToString() + "' los limpio....");

            foreach (NucleusRH.Base.Presupuesto.Escenarios.DETALLE_PRES DET in Escenario.DETALLES)
            {
                MyPeriodo = DETT.FindElement2("ROW", "oi_detalle_pres", DET.Id).GetAttrInt("e_periodo");
                if (PeriodoEgreso < MyPeriodo)
                {
                    NomadEnvironment.GetTrace().Info("Limpiar Periodo " + MyPeriodo.ToString() + "....");
                    retval.Aplicar(DET.Id, ConcValor, DATA.GetAttr("oi_modalidad"), DATA.GetAttr("oi_categoria"), DATA.GetAttr("c_ultimo_nivel"));
                }
            }
        }

        //Valorizar el MES INICIAL                      
        NomadEnvironment.GetTrace().Info("PASO 4 - Valorizo el Periodo....");
        if (xmlParam.FindElement("CONCEPTOS") != null)
        {
            ConcValor = new Hashtable();
            for (NomadXML cur = xmlParam.FindElement("CONCEPTOS").FirstChild(); cur != null; cur = cur.Next())
                ConcValor[cur.GetAttr("oi_concepto")] = cur.GetAttr("n_valor");

            retval.Aplicar(DATA.GetAttr("oi_detalle_desde"), ConcValor, DATA.GetAttr("oi_modalidad"), DATA.GetAttr("oi_categoria"), DATA.GetAttr("c_ultimo_nivel"));
        }

        if (xmlParam.FindElement("NOV_ALL") != null)
        {
            for (NomadXML cur = xmlParam.FindElement("NOV_ALL").FirstChild(); cur != null; cur = cur.Next())
            {
                NOV_LEGAJO nov = null;

                //Busco el Detalle Novedad....	 	
                foreach (NOV_LEGAJO MyNov in retval.NOV_LEGAJO)
                    if (MyNov.oi_novedad == cur.GetAttr("oi_novedad") && MyNov.oi_detalle_pres == DATA.GetAttr("oi_detalle_desde"))
                    {
                        nov = MyNov;
                        break;
                    }

                //Si no existe los CREO....	 	
                if (nov == null)
                {
                    nov = new NOV_LEGAJO();
                    nov.oi_novedad = cur.GetAttr("oi_novedad");
                    nov.oi_detalle_pres = DATA.GetAttr("oi_detalle_desde");
                    nov.n_valor = cur.GetAttrDouble("n_valor");
                    retval.NOV_LEGAJO.Add(nov);
                }
                else
                    nov.n_valor = cur.GetAttrDouble("n_valor");
            }
        }

        //Actualiza los Valores
        NomadEnvironment.GetTrace().Info("PASO 5 - Recalculo el LEGAJO....");
        retval.Actualizar();

        NomadEnvironment.GetTrace().Info("PASO 6 - FIN....");
        return retval;

    }
    public void AplicarAccion(NucleusRH.Base.Presupuesto.Acciones.ACCION acc)
    {


        //Actualizo la Formula
        acc.d_formula = acc.Getoi_tipo_accion().d_formula;

        //Actualiza los Valores
        NomadEnvironment.GetTrace().Info("PASO 1 - Aplicar ACCION....");
        this.Aplicar(acc);

        //Actualiza los Valores
        NomadEnvironment.GetTrace().Info("PASO 2 - Recalculo el LEGAJO....");
        this.Actualizar();

        NomadEnvironment.GetTrace().Info("PASO 3 - FIN....");

    }
    public void Save()
    {

        this.Actualizar();
        NomadEnvironment.GetCurrentTransaction().SaveRefresh(this);

    }
      // Codigo fuente en LIB
      //public void Actualizar();

  }
}



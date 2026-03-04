using System;
using System.IO;
using System.Collections;

using Nomad.NSystem.Document;
using Nomad.NSystem.Proxy;
using Nomad.NSystem.Base;
using Nomad.NSystem.Functions;

namespace NucleusRH.Base.PresupuestoLiquidacion.Conceptos
{
    public partial class CONCEPTO
    {
        public static void Incorporar(Nomad.NSystem.Proxy.NomadXML xmlParam)
        {
            NomadBatch B=NomadBatch.GetBatch("Incorporar Liquidaciones", "Incorporar Liquidaciones");

            xmlParam=xmlParam.FirstChild();

            NucleusRH.Base.Presupuesto.Legajos.LEGAJO myLEG,MyAux;
            NucleusRH.Base.Presupuesto.Escenarios.ESCENARIO Escenario=NucleusRH.Base.Presupuesto.Escenarios.ESCENARIO.Get(xmlParam.GetAttr("oi_escenario"));
            NucleusRH.Base.Presupuesto.Escenarios.DETALLE_PRES Detalle=(NucleusRH.Base.Presupuesto.Escenarios.DETALLE_PRES)Escenario.DETALLES.GetById(xmlParam.GetAttr("oi_detalle_pres"));
            NucleusRH.Base.Presupuesto.AniosFiscales.PERIODO_FISCAL Periodo=Detalle.Getoi_periodo_fiscal();


            NomadXML cur,cur2,LEGS,ASIGS,EGRS,ASIG,ROOT,DATA,EGR,AUXCONC;

            LEGS =xmlParam.FindElement("LEGS");
            ASIGS=xmlParam.FindElement("ASIGS");
            EGRS =xmlParam.FindElement("EGRS");

            NomadEnvironment.GetTrace().Info("Comienza la Transaccion...");
            NomadEnvironment.GetCurrentTransaction().Begin();

            int i, tot;


            //Legajos Existentes
            tot=LEGS.ChildLength;
            for (i=1, cur=LEGS.FirstChild(); cur!=null; cur=cur.Next(), i++)
            {
            B.SetPro(10,70,tot,(i-1));
            B.SetMess("Analizo el LEGAJO '"+cur.GetAttr("id")+"' (Legajo "+i.ToString()+" de "+tot.ToString()+" ) ...");
            xmlParam.SetAttr("e_numero_legajo",cur.GetAttr("id"));


            ROOT=NomadEnvironment.QueryNomadXML(CONCEPTO.Resources.LIQUIDACION,xmlParam.ToString()).FirstChild();
            NomadEnvironment.GetTrace().Info("ROOT: "+ROOT.ToString()+" ...");   
  
              //Actualizo a que grupo fue Asignado (caso nuevo)
            NomadEnvironment.GetTrace().Info("Asignacion...");   	
            ASIG=ASIGS.FindElement2("ASIG","id",cur.GetAttr("id"));
            if (ASIG!=null)
              ROOT.FindElement("DATA").SetAttr("oi_grupo", ASIG.GetAttr("oi_grupo"));

            //Caso de REEMPLAZO                                   
            NomadEnvironment.GetTrace().Info("Egresos...");   	
            EGR=EGRS.FindElement2("LEG","oi_rem",cur.GetAttr("id"));
            if (EGR!=null)                                         
            {                                                            
              AUXCONC=new NomadXML("DATA");
              AUXCONC.SetAttr("e_numero_legajo",EGR.GetAttr("id"));
  
	            MyAux=NucleusRH.Base.Presupuesto.Legajos.LEGAJO.Obtener(Escenario,AUXCONC);
	            ROOT.FindElement("DATA").SetAttr("e_numero_legajo_AUX",MyAux.e_posicion);
            }                                     


            //Agrego la Lista de Conceptos Validos....	
            Hashtable lConc=new Hashtable();
            AUXCONC=ROOT.FindElement("CONC_ALL");
            for (cur2=AUXCONC.FirstChild(); cur2!=null; cur2=cur2.Next())
	            lConc.Add(cur2.GetAttr("oi_concepto"), cur2);

            //Recorro los Conceptos....	         
            AUXCONC=ROOT.FindElement("CONCEPTOS");
            foreach(NomadXML cur4 in lConc.Values)
            {
	            for (cur2=AUXCONC.FirstChild(); cur2!=null; cur2=cur2.Next())
	            if (cur4.GetAttr("oi_concepto")==cur2.GetAttr("oi_concepto"))
	            {                                                  
		            if (cur4.GetAttrInt("count")==0)
		            {
			            cur4.SetAttr("count", 1);
			            cur4.SetAttr("sum"  , cur2.GetAttrDouble("n_valor"));
			            cur4.SetAttr("min"  , cur2.GetAttrDouble("n_valor"));
			            cur4.SetAttr("max"  , cur2.GetAttrDouble("n_valor"));
		            } else
		            {
			            cur4.SetAttr("count", cur4.GetAttrInt("count")+1);   
			            cur4.SetAttr("sum"  , cur4.GetAttrDouble("sum")+cur2.GetAttrDouble("n_valor"));
			            cur4.SetAttr("max"  , cur4.GetAttrDouble("max")>cur2.GetAttrDouble("n_valor")?cur4.GetAttrDouble("max"):cur2.GetAttrDouble("n_valor"));
			            cur4.SetAttr("min"  , cur4.GetAttrDouble("min")<cur2.GetAttrDouble("n_valor")?cur4.GetAttrDouble("min"):cur2.GetAttrDouble("n_valor"));
		            }
	            }
            }

            //Lista Final de Conceptos
            ROOT.DeleteChild(ROOT.FindElement("CONC_ALL"));
            ROOT.DeleteChild(ROOT.FindElement("CONCEPTOS"));

            AUXCONC=ROOT.AddTailElement("CONCEPTOS");
            foreach(NomadXML cur4 in lConc.Values)
            {
	            switch(cur4.GetAttr("c_operacion"))
	            {
		            case "S":
			            cur4.SetAttr("n_valor", cur4.GetAttrDouble("sum"));
		            break;
		
		            case "M":
			            cur4.SetAttr("n_valor", cur4.GetAttrDouble("max"));
		            break;

		            case "N":
			            cur4.SetAttr("n_valor", cur4.GetAttrDouble("min"));
		            break;

		            case "P":
			            cur4.SetAttr("n_valor", cur4.GetAttrDouble("sum")/cur4.GetAttrDouble("count"));
		            break;

		            default:
			            NomadEnvironment.GetTraceBatch().Error("El concepto '"+cur4.GetAttr("e_concepto")+"' para el Legajo '"+ROOT.GetAttr("id")+"' esta mas de una vez...");
		            break;
	            }
	            AUXCONC.AddXML(cur4);
            }
 
            //Obtengo el LEGAJO y lo ACTUALIZO 
            B.SetMess("Calcular los Conceptos para el Legajo '"+cur.GetAttr("id")+"' (Legajo "+i.ToString()+" de "+tot.ToString()+" ) ...");
            NomadEnvironment.GetTrace().Info("ROOT: "+ROOT.ToString());   

            if (ASIG==null) myLEG=NucleusRH.Base.Presupuesto.Legajos.LEGAJO.ModificarLegajo(ROOT);
                       else myLEG=NucleusRH.Base.Presupuesto.Legajos.LEGAJO.CrearLegajo(ROOT);	 	 

            NomadEnvironment.GetTrace().Info("Guardando...");   	
            NomadEnvironment.GetCurrentTransaction().Save(myLEG);	
            }

            //Egresos
            DateTime f_egreso=(new DateTime(Periodo.e_periodo/100, Periodo.e_periodo%100, 1)).AddDays(-1);

            tot=EGRS.ChildLength;
            for (i=1, cur=EGRS.FirstChild(); cur!=null; cur=cur.Next(), i++)
            {
            B.SetPro(70,90,tot,(i-1));
            B.SetMess("Egreso el LEGAJO '"+cur.GetAttr("id")+"' (Legajo "+i.ToString()+" de "+tot.ToString()+" ) ...");

            ROOT=new NomadXML("ROOT");
            DATA=ROOT.AddTailElement("DATA");
            DATA.SetAttr("oi_escenario"   ,xmlParam.GetAttr("oi_escenario"));
            DATA.SetAttr("e_numero_legajo",cur.GetAttr("id"));
            DATA.SetAttr("f_egreso"       ,f_egreso);
            DATA.SetAttr("simple"         ,"1");        


            B.SetMess("Calcular los Conceptos para el Legajo '"+cur.GetAttr("id")+"' (Legajo "+i.ToString()+" de "+tot.ToString()+" ) ...");
            NomadEnvironment.GetTrace().Info("ROOT: "+ROOT.ToString());   
            myLEG=NucleusRH.Base.Presupuesto.Legajos.LEGAJO.ModificarLegajo(ROOT);	 

            NomadEnvironment.GetTrace().Info("Guardando...");   	
            NomadEnvironment.GetCurrentTransaction().Save(myLEG);	
            }


            B.SetPro(90);        
            B.SetMess("Guardando la Informacion en la Base de Datos...");
            NomadEnvironment.GetCurrentTransaction().Commit();

            B.SetPro(100);
            B.SetMess("Fin...");
        }

        public static void Recalcular(int oiEscenario)
        {
            NomadBatch B = NomadBatch.GetBatch("Recalcular Conceptos", "Recalcular Conceptos");

            //obtengo ABM de conceptos
            NomadXML xmlAbmConceptos = NomadEnvironment.QueryNomadXML(CONCEPTO.Resources.QRY_CONCEPTOS_ABM, "").FirstChild();
            Hashtable abmConceptos = new Hashtable();

            for (NomadXML concepto = xmlAbmConceptos.FirstChild(); concepto != null; concepto = concepto.Next())
            {
                abmConceptos[concepto.GetAttr("e_concepto")] = concepto.GetAttr("d_formula");
            }

            // obtengo las acciones del escenario

            NomadXML xmlAcciones = NomadEnvironment.QueryNomadXML(CONCEPTO.Resources.QRY_ACCIONES, "<DATA oi_escenario='" + oiEscenario + "' />").FirstChild();
            Hashtable Acciones = new Hashtable();

            for (NomadXML accion = xmlAcciones.FirstChild(); accion != null; accion = accion.Next())
            {
                //tiene el oi_det_concepto --> formula
                Acciones[accion.GetAttr("oi_det_concepto")] = accion.GetAttr("oi_accion");
            }

            //actualizar la formual del concepto con la formula del ABM de conceptos
            NomadXML xmlLegajos = NomadEnvironment.QueryNomadXML(CONCEPTO.Resources.QRY_LEGAJOS, "<DATA oi_escenario='" + oiEscenario + "' />").FirstChild();
            int totalLegajos = xmlLegajos.ChildLength,i = 1;
            if (xmlLegajos.FirstChild() != null)
            {
                for (NomadXML legajo = xmlLegajos.FirstChild(); legajo != null; legajo = legajo.Next())
                {
                    B.SetPro(10, 70, totalLegajos, i);
                    B.SetMess("Analizando el LEGAJO '" + legajo.GetAttr("oi_legajo") + "' (Legajo " + i.ToString() + " de " + totalLegajos.ToString() + " ) ...");
                    i++;

                    NomadXML xmlConceptos = NomadEnvironment.QueryNomadXML(CONCEPTO.Resources.QRY_CONCEPTOS, "<DATA oi_legajo='"+legajo.GetAttr("oi_legajo") + "' oi_escenario='" + oiEscenario + "' />").FirstChild();

                    if (xmlConceptos.FirstChild() != null)
                    {
                        for (NomadXML concepto = xmlConceptos.FirstChild(); concepto != null; concepto = concepto.Next())
                        {
                            NucleusRH.Base.Presupuesto.Legajos.DET_CONCEPTO con = NucleusRH.Base.Presupuesto.Legajos.DET_CONCEPTO.Get(concepto.GetAttr("oi_det_concepto"));

                            con.d_formula = abmConceptos[concepto.GetAttr("e_concepto")].ToString();
                            
                            if (xmlAcciones.FirstChild() != null && Acciones[con.id.ToString()] != null)
                            {
                                string oiAccion = Acciones[con.id.ToString()].ToString();
                                con.d_formula = AplicarAccion(oiAccion,con);
                            }
      
                            NomadEnvironment.GetCurrentTransaction().Save(con);
                        }
                    }
                }
                

                B.SetPro(90);
                B.SetMess("Guardando la Informacion en la Base de Datos...");
            }
            B.SetPro(100);
            B.SetMess("Fin...");
        }


        public static string AplicarAccion(string oiAccion,NucleusRH.Base.Presupuesto.Legajos.DET_CONCEPTO con)
        {
            NucleusRH.Base.Presupuesto.Acciones.ACCION accion = NucleusRH.Base.Presupuesto.Acciones.ACCION.Get(oiAccion);
                
            NomadXML xmlPeriodo = NomadEnvironment.QueryNomadXML(CONCEPTO.Resources.QRY_PERIODOS, "<DATA oi_detalle_pres='" + accion.oi_detalle_pres+"' />").FirstChild();
            NomadXML xmlPeriodoBase = NomadEnvironment.QueryNomadXML(CONCEPTO.Resources.QRY_PERIODOS, "<DATA oi_detalle_pres='" + accion.oi_base+"' />").FirstChild();

                string newFormula = accion.d_formula;
            if (newFormula.Contains("{Periodo}"))
                newFormula = newFormula.Replace("{Periodo}", xmlPeriodo.FirstChild().GetAttr("e_periodo"));
            if (newFormula.Contains("{Base}"))
                newFormula = newFormula.Replace("{Base}", xmlPeriodoBase.FirstChild().GetAttr("e_periodo"));
            if (newFormula.Contains("{Valor}"))
                newFormula = newFormula.Replace("{Valor}", Nomad.NSystem.Functions.StringUtil.dbl2str(accion.n_valor));
            if (newFormula.Contains("@"))
                newFormula = newFormula.Replace("@", con.d_formula);

            return newFormula;
        
        }
    }
}



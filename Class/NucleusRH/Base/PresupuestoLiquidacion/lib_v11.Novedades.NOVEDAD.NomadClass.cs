using System;
using System.IO;
using System.Collections;

using Nomad.NSystem.Document;
using Nomad.NSystem.Proxy;
using Nomad.NSystem.Base;
using Nomad.NSystem.Functions;
using System.Collections.Generic;

namespace NucleusRH.Base.PresupuestoLiquidacion.Novedades
{
    public partial class NOVEDAD
    {
        public static void Incorporar(Nomad.NSystem.Proxy.NomadXML xmlParam)
        {
            NomadBatch B = NomadBatch.GetBatch("Exportar Novedades de Liquidacion", "Exportar Novedades de Liquidacion");

            xmlParam = xmlParam.FirstChild();

            NucleusRH.Base.Presupuesto.Legajos.LEGAJO myLEG, MyAux;
            NucleusRH.Base.Presupuesto.Escenarios.ESCENARIO Escenario = NucleusRH.Base.Presupuesto.Escenarios.ESCENARIO.Get(xmlParam.GetAttr("oi_escenario"));
            NucleusRH.Base.Presupuesto.Escenarios.DETALLE_PRES Detalle = (NucleusRH.Base.Presupuesto.Escenarios.DETALLE_PRES)Escenario.DETALLES.GetById(xmlParam.GetAttr("oi_detalle_pres"));
            NucleusRH.Base.Presupuesto.AniosFiscales.PERIODO_FISCAL Periodo = Detalle.Getoi_periodo_fiscal();

            double auxval;
            NomadXML cur, cur2, LEGS, ROOT, DATA, CONC, AUXCONC, VARACUM;

            LEGS = xmlParam.FindElement("LEGS");

            NomadEnvironment.GetTrace().Info("Comienza la Transaccion...");
            NomadEnvironment.GetCurrentTransaction().Begin();

            int i, tot;


            //Legajos Existentes
            tot = LEGS.ChildLength;
            for (i = 1, cur = LEGS.FirstChild(); cur != null; cur = cur.Next(), i++)
            {
                B.SetPro(10, 90, tot, (i - 1));
                B.SetMess("Analizo el LEGAJO '" + cur.GetAttr("id") + "' (Legajo " + i.ToString() + " de " + tot.ToString() + " ) ...");
                xmlParam.SetAttr("e_numero_legajo", cur.GetAttr("id"));

                ROOT = NomadEnvironment.QueryNomadXML(NOVEDAD.Resources.LIQUIDACION, xmlParam.ToString()).FirstChild();
                VARACUM = NomadEnvironment.QueryNomadXML(NOVEDAD.Resources.QRY_VAR_ACUM, xmlParam.ToString()).FirstChild();

                Dictionary<string,double> dicJson = Liquidacion.RHLiq.LiqUtilBase.NomadXMLADiccionario(VARACUM);

                NomadEnvironment.GetTrace().Info("ROOT: " + ROOT.ToString() + " ...");

                //Agrego la Lista de Novedades Validas....	
                Hashtable lNov = new Hashtable();
                AUXCONC = ROOT.FindElement("NOV_ALL");
                for (cur2 = AUXCONC.FirstChild(); cur2 != null; cur2 = cur2.Next())
                    lNov.Add(cur2.GetAttr("oi_novedad"), 0.0);

                //Recorro la lista....	
                foreach (string Key in lNov.Keys)
                {
                    auxval = (double)lNov[Key];

                    AUXCONC = ROOT.FindElement("ACUM").FindElement2("NOVEDAD", "oi_novedad", Key);
                    if (AUXCONC != null && dicJson.ContainsKey(AUXCONC.GetAttr("c_variable")))
                        auxval += dicJson[AUXCONC.GetAttr("c_variable")]; //AUXCONC.GetAttrDouble("n_valor");

                    AUXCONC = ROOT.FindElement("NOVE").FindElement2("NOVEDAD", "oi_novedad", Key);
                    if (AUXCONC != null) auxval += AUXCONC.GetAttrDouble("n_valor");

                    AUXCONC = ROOT.FindElement("FIJO").FindElement2("NOVEDAD", "oi_novedad", Key);
                    if (AUXCONC != null) auxval += AUXCONC.GetAttrDouble("n_valor");


                    AUXCONC = ROOT.FindElement("NOV_ALL").FindElement2("NOVEDAD", "oi_novedad", Key);
                    AUXCONC.SetAttr("n_valor", auxval);
                }

                //Modificando el LEGAJO
                NomadEnvironment.GetTrace().Info("ROOT: " + ROOT.ToString() + " ...");
                myLEG = NucleusRH.Base.Presupuesto.Legajos.LEGAJO.ModificarLegajo(ROOT);

                //Guardando el LEGAJO	                       
                NomadEnvironment.GetTrace().Info("Guardando...");
                NomadEnvironment.GetCurrentTransaction().Save(myLEG);
            }


            B.SetPro(90);
            B.SetMess("Guardando la Informacion en la Base de Datos...");
            NomadEnvironment.GetCurrentTransaction().Commit();

            B.SetPro(100);
            B.SetMess("Fin...");
        }
    }
}



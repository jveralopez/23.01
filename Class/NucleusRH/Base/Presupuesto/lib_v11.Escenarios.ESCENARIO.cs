using System;
using System.Xml;
using System.IO;
using System.Collections;

using Nomad.NSystem.Document;
using Nomad.NSystem.Proxy;
using Nomad.NSystem.Base;
using Nomad.NSystem.Functions;

namespace NucleusRH.Base.Presupuesto.Escenarios
{

    //////////////////////////////////////////////////////////////////////////////////
    //Clase Escenarios
    public partial class ESCENARIO 
    {
        public static NucleusRH.Base.Presupuesto.Escenarios.ESCENARIO CrearEscenario(string oi_presupuesto)
        {

            ESCENARIO retval = new ESCENARIO();
            DETALLE_PRES DET;


            retval.oi_presupuesto = oi_presupuesto;
            retval.e_seguimiento = 1;
            retval.c_estado = "P";
            retval.f_alta = DateTime.Now;


            NucleusRH.Base.Presupuesto.Presupuestos.PRESUPUESTO pres = retval.Getoi_presupuesto();
            NucleusRH.Base.Presupuesto.AniosFiscales.ANIO_FISCAL annio = pres.Getoi_anio_fiscal();

            foreach (NucleusRH.Base.Presupuesto.AniosFiscales.PERIODO_FISCAL per in annio.PER_FISCAL)
            {
                DET = new DETALLE_PRES();
                DET.oi_periodo_fiscal = per.Id;
                retval.DETALLES.Add(DET);
            }

            return retval;

        }
        public static void DuplicarEscenario(string oi_escenario)
        {

            int tot, pas;
            NomadBatch b = NomadBatch.GetBatch("Duplicar Escenario...", "Duplicar Escenario...");
            b.SetMess("Duplicando Escenario...");

            ESCENARIO source = ESCENARIO.Get(oi_escenario);

            ESCENARIO retval = new ESCENARIO();
            DETALLE_PRES DET;
            //NucleusRH.Base.Presupuesto.Legajos.LEGAJO MyLeg;
            //NucleusRH.Base.Presupuesto.Acciones.ACCION MyAcc;
            NomadXML cur, XML;

            retval.oi_presupuesto = source.oi_presupuesto;
            retval.e_seguimiento = source.e_seguimiento;
            retval.oi_def_reporte = source.oi_def_reporte;
            retval.oi_est_reporte = source.oi_est_reporte;
            retval.c_estado = "P";
            retval.f_alta = DateTime.Now;
            if (source.c_estado == "A") retval.e_seguimiento++;


            //Duplicar Detalles
            foreach (DETALLE_PRES per in source.DETALLES)
            {
                DET = new DETALLE_PRES();
                DET.oi_periodo_fiscal = per.oi_periodo_fiscal;
                retval.DETALLES.Add(DET);
            }
            NomadEnvironment.GetCurrentTransaction().SaveRefresh(retval);
            b.SetPro(20);

            //Lista de OIS
            Hashtable src2trg = new Hashtable();
            foreach (DETALLE_PRES per in source.DETALLES)
            {
                DET = (DETALLE_PRES)retval.DETALLES.GetByAttribute("oi_periodo_fiscal", per.oi_periodo_fiscal);
                src2trg[per.Id] = DET.Id;
            }

            //Duplico Legajos
            b.SetMess("Duplicando Legajos...");
            Hashtable src2leg = new Hashtable();
            XML = NomadEnvironment.QueryNomadXML(Resources.LEGAJOS, "<DATA oi_escenario=\"" + oi_escenario + "\" />");

            tot = XML.FirstChild().ChildLength;
            pas = 0;
            for (cur = XML = XML.FirstChild().FirstChild(); cur != null; cur = cur.Next())
            {
                pas++;
                b.SetPro(20, 70, tot, pas);

                NucleusRH.Base.Presupuesto.Legajos.LEGAJO MyLeg = new NucleusRH.Base.Presupuesto.Legajos.LEGAJO();
                MyLeg.Load(NucleusRH.Base.Presupuesto.Legajos.LEGAJO.Get(cur.GetAttr("oi_legajo")).Duplicate().ToString());
                MyLeg.oi_escenario = retval.Id;

                AsignarDetalleNovedades(MyLeg);

                foreach (NucleusRH.Base.Presupuesto.Legajos.DET_CONCEPTO conc in MyLeg.DET_CONCEPTOS)
                    conc.oi_detalle_pres = (string)src2trg[conc.oi_detalle_pres];

                NomadEnvironment.GetCurrentTransaction().SaveRefresh(MyLeg);
                src2leg[cur.GetAttr("oi_legajo")] = MyLeg.Id;
            }
            b.SetPro(70);

            //Duplico Acciones
            b.SetMess("Duplicando Acciones...");
            XML = NomadEnvironment.QueryNomadXML(Resources.ACCIONES, "<DATA oi_escenario=\"" + oi_escenario + "\" />");
            tot = XML.FirstChild().ChildLength;
            pas = 0;
            for (cur = XML = XML.FirstChild().FirstChild(); cur != null; cur = cur.Next())
            {
                pas++;
                b.SetPro(70, 90, tot, pas);

                NucleusRH.Base.Presupuesto.Acciones.ACCION MyAcc = new NucleusRH.Base.Presupuesto.Acciones.ACCION();
                MyAcc.Load(NucleusRH.Base.Presupuesto.Acciones.ACCION.Get(cur.GetAttr("oi_accion")).Duplicate().ToString());

                MyAcc.oi_escenario = retval.Id;
                MyAcc.oi_detalle_pres = (string)src2trg[MyAcc.oi_detalle_pres];


                foreach (NucleusRH.Base.Presupuesto.Acciones.ACC_LEGAJO AccLeg in MyAcc.ACC_LEG)
                    AccLeg.oi_legajo = (string)src2leg[AccLeg.oi_legajo];

                NomadEnvironment.GetCurrentTransaction().Save(MyAcc);
            }
            b.SetPro(100);

            return;

        }

        private static void AsignarDetalleNovedades(Legajos.LEGAJO MyLeg)
        {
            foreach(NucleusRH.Base.Presupuesto.Legajos.NOV_LEGAJO nov in MyLeg.NOV_LEGAJO)
            {
                NucleusRH.Base.Presupuesto.Escenarios.DETALLE_PRES det = NucleusRH.Base.Presupuesto.Escenarios.DETALLE_PRES.Get(nov.oi_detalle_pres);
                nov.oi_detalle_pres = NomadEnvironment.QueryNomadXML(Resources.DETALLE_PRES, "<DATA oi_periodo_fiscal=\""+det.oi_periodo_fiscal+"\" oi_escenario=\"" + MyLeg.oi_escenario + "\" />").FirstChild().GetAttr("oi_detalle_pres");
            }
        }
        public void AprobarEscenario(Nomad.NSystem.Proxy.NomadXML xmlPresupuestos)
        {

            /*
Nomad.NSystem.Proxy.NomadXML xmlPresupuestos;
<DATA>
	<ROWS nmd-col="1">
		<ROW id="1" />
		<ROW id="3" />
		<ROW id="5" />
	</ROWS>
</DATA>
*/
            NomadEnvironment.GetTrace().Info("1");
            NomadEnvironment.GetTrace().Info(xmlPresupuestos.ToString());
            xmlPresupuestos = xmlPresupuestos.FirstChild();
            NomadEnvironment.GetTrace().Info(xmlPresupuestos.ToString());
            //->>>>ROWS

            for (NomadXML xmlPres = xmlPresupuestos.FirstChild(); xmlPres != null; xmlPres = xmlPres.Next())
            {
                string oi_escenario = xmlPres.GetAttr("id");
                ESCENARIO source = ESCENARIO.Get(oi_escenario);
                source.c_estado = "R";
                source.f_aprobacion = DateTime.Now;
                source.o_rechazo = "Se aprobo el Escenario: " + this.e_numero.ToString();
                NomadEnvironment.GetTrace().Info("2");
                NomadEnvironment.GetTrace().Info(xmlPres.ToString());

                NomadEnvironment.GetCurrentTransaction().SaveRefresh(source);
            }

            this.c_estado = "A";
            this.f_aprobacion = DateTime.Now;
            NomadEnvironment.GetTrace().Info("3");
            NomadEnvironment.GetTrace().Info(this.ToString());
            NomadEnvironment.GetCurrentTransaction().SaveRefresh(this);

        }
        public static void EliminarEscenario(string oi_escenario)
        {

            int tot, pas;
            NucleusRH.Base.Presupuesto.Legajos.LEGAJO MyLeg;
            NucleusRH.Base.Presupuesto.Acciones.ACCION MyAcc;
            NomadXML cur, XML;


            NomadBatch b = NomadBatch.GetBatch("Eliminando Escenario...", "Eliminando Escenario...");


            NomadEnvironment.GetCurrentTransaction().Begin();


            //Elimino Acciones
            b.SetMess("Elimino Acciones...");
            XML = NomadEnvironment.QueryNomadXML(Resources.ACCIONES, "<DATA oi_escenario=\"" + oi_escenario + "\" />");
            tot = XML.FirstChild().ChildLength;
            pas = 0;
            for (cur = XML = XML.FirstChild().FirstChild(); cur != null; cur = cur.Next())
            {
                pas++;
                b.SetPro(10, 40, tot, pas);

                MyAcc = (NucleusRH.Base.Presupuesto.Acciones.ACCION)NucleusRH.Base.Presupuesto.Acciones.ACCION.Get(cur.GetAttr("oi_accion"));
                NomadEnvironment.GetCurrentTransaction().Delete(MyAcc);

            }
            b.SetPro(40);

            //Elimino Legajos
            b.SetMess("Elimino Legajos...");
            XML = NomadEnvironment.QueryNomadXML(Resources.LEGAJOS, "<DATA oi_escenario=\"" + oi_escenario + "\" />");
            tot = XML.FirstChild().ChildLength;
            pas = 0;
            for (cur = XML = XML.FirstChild().FirstChild(); cur != null; cur = cur.Next())
            {
                pas++;
                b.SetPro(40, 80, tot, pas);

                MyLeg = (NucleusRH.Base.Presupuesto.Legajos.LEGAJO)NucleusRH.Base.Presupuesto.Legajos.LEGAJO.Get(cur.GetAttr("oi_legajo"));
                NomadEnvironment.GetCurrentTransaction().Delete(MyLeg);
            }
            b.SetPro(80);


            //Elimino Escenario
            b.SetMess("Elimino Escenario...");
            ESCENARIO source = ESCENARIO.Get(oi_escenario);
            NomadEnvironment.GetCurrentTransaction().Delete(source);
            b.SetPro(80);


            b.SetMess("Actualizando la Base de Datos...");
            NomadEnvironment.GetCurrentTransaction().Commit();
            b.SetPro(100);

            return;

        }
    }
}

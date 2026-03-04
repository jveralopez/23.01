using System;
using System.Xml;
using System.IO;
using System.Collections;

using Nomad.NSystem.Document;
using Nomad.NSystem.Proxy;
using Nomad.NSystem.Base;
using Nomad.NSystem.Functions;

namespace NucleusRH.Base.Presupuesto.Acciones
{

    //////////////////////////////////////////////////////////////////////////////////
    //Clase Acciones sobre el Escenario
    public partial class ACCION
    {
        private void Save()
        {

            NomadBatch B = NomadBatch.GetBatch("Guardar Accion", "Guardar Accion");

            this.d_formula = this.Getoi_tipo_accion().d_formula;

            NucleusRH.Base.Presupuesto.Legajos.LEGAJO LEG;
            int i, tot;

            NomadEnvironment.GetTrace().Info("Comienza la Transaccion.");
            NomadEnvironment.GetCurrentTransaction().Begin();

            i = 0; tot = this.ACC_LEG.Count;
            foreach (ACC_LEGAJO leg in this.ACC_LEG)
            {
                NomadEnvironment.GetTrace().Info("Obtengo el LEGAJO.");
                LEG = leg.Getoi_legajo();

                //Barra de Progreso
                i++;
                B.SetMess("Calculo Legajo " + LEG.e_numero_legajo.ToString() + " (" + i.ToString() + " de " + tot.ToString() + ")");
                B.SetPro(0, 90, tot, i);

                NomadEnvironment.GetTrace().Info("Aplico la ACCION.");
                LEG.Aplicar(this);

                NomadEnvironment.GetTrace().Info("Actualizo los VALORES.");
                LEG.Actualizar();

                NomadEnvironment.GetTrace().Info("Guardo el Legajo " + LEG.e_numero_legajo.ToString() + " .");
                NomadEnvironment.GetCurrentTransaction().Save(LEG);
            }

            NomadEnvironment.GetTrace().Info("Guardo la Accion.");
            NomadEnvironment.GetCurrentTransaction().SaveRefresh(this);

            NomadEnvironment.GetTrace().Info("Commit.");
            NomadEnvironment.GetCurrentTransaction().Commit();

            B.SetPro(100);
            NomadEnvironment.GetTrace().Info("Fin.");

        }
        public void ARMAR_ACCION(Nomad.NSystem.Proxy.NomadXML xmlLegajos, Nomad.NSystem.Proxy.NomadXML xmlConceptos)
        {

            /*
Nomad.NSystem.Proxy.NomadXML xmlLegajos;
<ROWS>
  <ROW id="1" e_numero_legajo="1" d_legajo="Esteban Garzon" />
</ROWS>
*/
            NomadEnvironment.GetTrace().Info("1");
            NomadEnvironment.GetTrace().Info(xmlLegajos.ToString());
            xmlLegajos = xmlLegajos.FirstChild();

            NomadEnvironment.GetTrace().Info("2");
            NomadEnvironment.GetTrace().Info(xmlLegajos.ToString());
            //->>>>ROWS

            for (NomadXML xmlLeg = xmlLegajos.FirstChild(); xmlLeg != null; xmlLeg = xmlLeg.Next())
            {
                NomadEnvironment.GetTrace().Info("3");
                NomadEnvironment.GetTrace().Info(xmlLeg.ToString());
                NucleusRH.Base.Presupuesto.Acciones.ACC_LEGAJO leg = new NucleusRH.Base.Presupuesto.Acciones.ACC_LEGAJO();
                leg.oi_legajo = xmlLeg.GetAttr("id");
                this.ACC_LEG.Add(leg);
            }

            /*
            Nomad.NSystem.Proxy.NomadXML xmlConceptos ;
            <DATA>
                <ROWS nmd-col="1">
                    <ROW id="1" />
                    <ROW id="3" />
                    <ROW id="5" />
                </ROWS>
            </DATA>
            */
            NomadEnvironment.GetTrace().Info("4");
            NomadEnvironment.GetTrace().Info(xmlConceptos.ToString());
            xmlConceptos = xmlConceptos.FirstChild();
            //-->DATA
            NomadEnvironment.GetTrace().Info("5");
            NomadEnvironment.GetTrace().Info(xmlConceptos.ToString());
            xmlConceptos = xmlConceptos.FirstChild();
            NomadEnvironment.GetTrace().Info("6");
            NomadEnvironment.GetTrace().Info(xmlConceptos.ToString());
            //-->ROWS
            if (xmlConceptos != null)
            {
                for (NomadXML xmlConc = xmlConceptos.FirstChild(); xmlConc != null; xmlConc = xmlConc.Next())
                {
                    NomadEnvironment.GetTrace().Info("7");
                    NomadEnvironment.GetTrace().Info(xmlConc.ToString());
                    NucleusRH.Base.Presupuesto.Acciones.ACC_CONCEPTO conc = new NucleusRH.Base.Presupuesto.Acciones.ACC_CONCEPTO();
                    conc.oi_concepto = xmlConc.GetAttr("id");
                    conc.n_valor = this.n_valor;
                    this.ACC_CONC.Add(conc);
                }
            }
            NomadEnvironment.GetTrace().Info("8");
            NomadEnvironment.GetTrace().Info(this.ToString());

        }
        private void Delete()
        {

            NomadBatch B = NomadBatch.GetBatch("Eliminar Accion", "Eliminar Accion");

            NucleusRH.Base.Presupuesto.Legajos.LEGAJO LEG;
            int i, tot;

            NomadEnvironment.GetTrace().Info("Comienza la Transaccion.");
            NomadEnvironment.GetCurrentTransaction().Begin();

            i = 0; tot = this.ACC_LEG.Count;
            foreach (ACC_LEGAJO leg in this.ACC_LEG)
            {
                NomadEnvironment.GetTrace().Info("Obtengo el LEGAJO.");
                LEG = leg.Getoi_legajo();

                //Barra de Progreso
                i++;
                B.SetMess("Recalculo el Legajo " + LEG.e_numero_legajo.ToString() + " (" + i.ToString() + " de " + tot.ToString() + ")");
                B.SetPro(0, 90, tot, i);

                LEG.Reiniciar();

                NomadXML lAcciones = NomadEnvironment.QueryNomadXML(Resources.ACCIONES, "<DATA oi_legajo=\"" + LEG.Id + "\" />").FirstChild();
                for (NomadXML cur = lAcciones.FirstChild(); cur != null; cur = cur.Next())
                    if (this.Id != cur.GetAttr("id"))
                        LEG.Aplicar(NucleusRH.Base.Presupuesto.Acciones.ACCION.Get(cur.GetAttr("id")));

                NomadEnvironment.GetTrace().Info("Actualizo los VALORES.");
                LEG.Actualizar();

                NomadEnvironment.GetTrace().Info("Guardo el Legajo " + LEG.e_numero_legajo.ToString() + " .");
                NomadEnvironment.GetCurrentTransaction().Save(LEG);
            }

            NomadEnvironment.GetTrace().Info("Guardo la Accion.");
            NomadEnvironment.GetCurrentTransaction().Delete(this);

            NomadEnvironment.GetTrace().Info("Commit.");
            NomadEnvironment.GetCurrentTransaction().Commit();

            B.SetPro(100);
            NomadEnvironment.GetTrace().Info("Fin.");

        }
        private void Edit()
        {

            NomadBatch B = NomadBatch.GetBatch("Actualizar Accion", "Actualizar Accion");

            NucleusRH.Base.Presupuesto.Legajos.LEGAJO LEG;
            int i, tot;

            NomadEnvironment.GetTrace().Info("Comienza la Transaccion.");
            NomadEnvironment.GetCurrentTransaction().Begin();
           // NomadEnvironment.GetCurrentTransaction().SaveRefresh(accion);

            i = 0; tot = this.ACC_LEG.Count;
            foreach (ACC_LEGAJO leg in this.ACC_LEG)
            {
                NomadEnvironment.GetTrace().Info("Obtengo el LEGAJO.");
                LEG = leg.Getoi_legajo();

                //Barra de Progreso
                i++;
                B.SetMess("Recalculo el Legajo " + LEG.e_numero_legajo.ToString() + " (" + i.ToString() + " de " + tot.ToString() + ")");
                B.SetPro(0, 90, tot, i);

                LEG.Reiniciar();

                NomadXML lAcciones = NomadEnvironment.QueryNomadXML(Resources.ACCIONES, "<DATA oi_legajo=\"" + LEG.Id + "\" />").FirstChild();
                for (NomadXML cur = lAcciones.FirstChild(); cur != null; cur = cur.Next())
                    if (this.Id != cur.GetAttr("id"))
                        LEG.Aplicar(NucleusRH.Base.Presupuesto.Acciones.ACCION.Get(cur.GetAttr("id")));
                    else
                        LEG.Aplicar(this);

                NomadEnvironment.GetTrace().Info("Actualizo los VALORES.");
                LEG.Actualizar();

                NomadEnvironment.GetTrace().Info("Guardo el Legajo " + LEG.e_numero_legajo.ToString() + " .");
                NomadEnvironment.GetCurrentTransaction().Save(LEG);
            }

            NomadEnvironment.GetTrace().Info("Guardo la Accion.");
            NomadEnvironment.GetCurrentTransaction().SaveRefresh(this);

            NomadEnvironment.GetTrace().Info("Commit.");
            NomadEnvironment.GetCurrentTransaction().Commit();

            B.SetPro(100);
            NomadEnvironment.GetTrace().Info("Fin.");

        }
    }
}



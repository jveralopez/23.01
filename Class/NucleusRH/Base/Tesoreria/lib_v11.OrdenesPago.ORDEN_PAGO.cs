using System;
using System.Xml;
using System.IO;
using System.Collections;

using Nomad.NSystem.Document;
using Nomad.NSystem.Proxy;
using Nomad.NSystem.Base;
using Nomad.NSystem.Functions;

namespace NucleusRH.Base.Tesoreria.OrdenesPago
{

    //////////////////////////////////////////////////////////////////////////////////
    //Clase Ordenes de Pago
    public partial class ORDEN_PAGO 
    {
        public static void Save(string c_liquidacion_ddo)
        {

            try
            {
                NomadBatch objbatch = NomadBatch.GetBatch("Generación Orden de Pago", "Pagos");
                objbatch.SetMess("Inicia la creación de la orden de pago.");
                objbatch.Log("Inicia la creación de la orden de pago.");

                // Comienza la transaccion
                NomadEnvironment.GetCurrentTransaction().Begin();

                // Carga el archivo
                string strParam = NomadProxy.GetProxy().FileServiceIO().LoadFile("TEMP", c_liquidacion_ddo + ".orden.xml");
                NomadXML xmlParam = new NomadXML();
                xmlParam.SetText(strParam);
                xmlParam = xmlParam.FirstChild();

                // Crea Orden de Pago
                NucleusRH.Base.Tesoreria.OrdenesPago.ORDEN_PAGO newOrden = new NucleusRH.Base.Tesoreria.OrdenesPago.ORDEN_PAGO();

                // Setea atributos de cabecera
                newOrden.d_orden_pago = xmlParam.GetAttr("d_orden_pago");
                newOrden.oi_liquidacion_ddo = xmlParam.GetAttr("oi_liquidacion_ddo");
                newOrden.oi_forma_pago = xmlParam.GetAttr("oi_forma_pago");
                newOrden.f_alta = DateTime.Today;
                newOrden.f_pago = Nomad.NSystem.Functions.StringUtil.str2date(xmlParam.GetAttr("f_pago"));
                newOrden.o_orden_pago = xmlParam.GetAttr("o_orden_pago");

                // Carga el detalle
                double monto_total = 0;
                NucleusRH.Base.Tesoreria.OrdenesPago.DETALLE_PAGO newDetalle;
                NucleusRH.Base.Liquidacion.Recibos.TOT_LIQ_PER recibo;

                int l = xmlParam.FindElement("legajos").ChildLength;
                int i = 0;
                objbatch.SetPro(10);

                for (NomadXML per = xmlParam.FindElement("legajos").FirstChild(); per != null; per = per.Next())
                {
                    i++;
                    if (i % 10 == 0)
                    {
                        objbatch.SetMess("Agregando legajos (" + i.ToString() + " de " + l.ToString() + ")");
                        objbatch.Log("Agregando legajos (" + i.ToString() + " de " + l.ToString() + ")");
                    }
                    objbatch.SetPro(10, 90, l, i);

                    for (NomadXML rec = per.FindElement("recibos").FirstChild(); rec != null; rec = rec.Next())
                    {
                        newDetalle = new NucleusRH.Base.Tesoreria.OrdenesPago.DETALLE_PAGO();
                        newDetalle.oi_tot_liq_per = rec.GetAttr("oi_tot_liq_per");
                        newDetalle.oi_per_emp_ddo = per.GetAttr("oi_per_emp_ddo");
                        double totpen_org = Nomad.NSystem.Functions.StringUtil.str2dbl(rec.GetAttr("totpen_org"));
                        double totpen = Nomad.NSystem.Functions.StringUtil.str2dbl(rec.GetAttr("totpen"));
                        double totpag = Nomad.NSystem.Functions.StringUtil.str2dbl(rec.GetAttr("totpag"));
                        newDetalle.n_monto = totpen_org - totpen;
                        monto_total += newDetalle.n_monto;

                        newOrden.DET_PAGO.Add(newDetalle);

                        recibo = NucleusRH.Base.Liquidacion.Recibos.TOT_LIQ_PER.Get(newDetalle.oi_tot_liq_per);
                        recibo.n_montopagado = totpag - totpen;
                        NomadEnvironment.GetCurrentTransaction().Save(recibo);
                    }
                }

                newOrden.n_total_original = monto_total;

                objbatch.SetMess("Guardando la orden de pago.");
                objbatch.Log("Guardando la orden de pago.");
                NomadEnvironment.GetCurrentTransaction().Save(newOrden);

                NomadEnvironment.GetCurrentTransaction().Commit();
                objbatch.SetPro(100);
                objbatch.SetMess("Proceso finalizado.");
                objbatch.Log("Proceso finalizado.");

            }
            catch (Exception e)
            {
                NomadEnvironment.GetCurrentTransaction().Rollback();
                NomadEnvironment.GetTrace().Error("Error guardando la Orden de Pago. - " + e.Message + "\n" + e.StackTrace);
                throw new NomadAppException("Error guardando la Orden de Pago. - " + e.Message + "\n" + e.StackTrace);
            }

        }
        public void Anular()
        {

            try
            {
                NomadBatch objbatch = NomadBatch.GetBatch("Anulación Orden de Pago", "Pagos");
                objbatch.SetMess("Inicia la anulación de la orden de pago.");
                objbatch.Log("Inicia la anulación de la orden de pago.");

                NomadEnvironment.GetCurrentTransaction().Begin();
                // Carga los datos en la cabecera
                this.l_anulada = true;
                this.f_anulacion = DateTime.Today;
                this.e_cant_modif += 1;
                this.n_total_modificado = 0;

                int l = this.DET_PAGO.Count;
                int i = 0;
                objbatch.SetPro(10);

                // Anula cada detalle
                foreach (DETALLE_PAGO det in this.DET_PAGO)
                {
                    i++;
                    if (i % 10 == 0)
                    {
                        objbatch.SetMess("Anulando pagos (" + i.ToString() + " de " + l.ToString() + ")");
                        objbatch.Log("Anulando pagos (" + i.ToString() + " de " + l.ToString() + ")");
                    }
                    objbatch.SetPro(10, 90, l, i);

                    if (det.l_anulado) continue;
                    det.l_anulado = true;
                    det.f_anulacion = DateTime.Today;
                    det.e_nro_modif = this.e_cant_modif;

                    // Actualiza el Recibo	
                    NucleusRH.Base.Liquidacion.Recibos.TOT_LIQ_PER rec = NucleusRH.Base.Liquidacion.Recibos.TOT_LIQ_PER.Get(det.oi_tot_liq_per);
                    rec.n_montopagado -= det.n_monto;
                    NomadEnvironment.GetCurrentTransaction().Save(rec);
                }

                objbatch.SetMess("Guardando la anulación.");
                objbatch.Log("Guardando la anulación.");

                NomadEnvironment.GetCurrentTransaction().Save(this);
                NomadEnvironment.GetCurrentTransaction().Commit();

                objbatch.SetMess("Proceso finalizado.");
                objbatch.Log("Proceso finalizado.");

            }
            catch (Exception e)
            {
                NomadEnvironment.GetCurrentTransaction().Rollback();
                NomadEnvironment.GetTrace().Error("Error anulando la Orden de pago. - " + e.Message + "\n" + e.StackTrace);
                throw new NomadAppException("Error anulando la Orden de pago. - " + e.Message + "\n" + e.StackTrace);
            }

        }
        public void AnularLegajo(string e_numero_legajo)
        {

            try
            {
                NomadEnvironment.GetCurrentTransaction().Begin();
                // Si nunca fue modificada la orden de pago, setea el monto modificado igual al original
                if (this.e_cant_modif == 0)
                    this.n_total_modificado = this.n_total_original;

                // Actualiza la cabecera de la orden de pago
                this.e_cant_modif++;

                // Obtine con un recurso los detalles del legajo
                string param = "<DATOS oi_orden_pago=\"" + this.Id + "\" e_numero_legajo=\"" + e_numero_legajo + "\" />";
                Nomad.NSystem.Document.NmdXmlDocument xml_doc;
                xml_doc = new Nomad.NSystem.Document.NmdXmlDocument(NomadEnvironment.QueryString(NucleusRH.Base.Tesoreria.OrdenesPago.ORDEN_PAGO.Resources.QRY_DET_LEGAJO, param));
                NomadEnvironment.GetTrace().Info(xml_doc.ToString());

                DETALLE_PAGO detalle;
                NucleusRH.Base.Liquidacion.Recibos.TOT_LIQ_PER recibo;
                // Recorre los detalles de pago del legajo
                foreach (Nomad.NSystem.Document.NmdXmlDocument det in xml_doc.ChildDocuments)
                {
                    // Actualiza el detalle
                    detalle = (DETALLE_PAGO)this.DET_PAGO.GetById(det.GetAttribute("id").Value);
                    if (detalle.l_anulado) continue;
                    detalle.l_anulado = true;
                    detalle.f_anulacion = DateTime.Today;
                    detalle.e_nro_modif = this.e_cant_modif;
                    // Actualiza el recibo
                    recibo = NucleusRH.Base.Liquidacion.Recibos.TOT_LIQ_PER.Get(det.GetAttribute("oi_tot_liq_per").Value);
                    recibo.n_montopagado -= detalle.n_monto;
                    // Actualiza el monto de la orden de pago	
                    this.n_total_modificado -= detalle.n_monto;
                    NomadEnvironment.GetCurrentTransaction().Save(recibo);
                }

                NomadEnvironment.GetCurrentTransaction().Save(this);
                NomadEnvironment.GetCurrentTransaction().Commit();
            }
            catch (Exception e)
            {
                NomadEnvironment.GetTrace().Info("Error anulando el legajo. - " + e.Message + "\n" + e.StackTrace);
                NomadEnvironment.GetCurrentTransaction().Rollback();
                throw new NomadAppException("Error anulando el legajo. - " + e.Message + "\n" + e.StackTrace);
            }

        }
    }
}

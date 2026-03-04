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

namespace NucleusRH.Base.Legales.Adelantos
{

    //////////////////////////////////////////////////////////////////////////////////
    //Clase Adelantos
    public partial class ADELANTO : Nomad.NSystem.Base.NomadObject
    {
        public static void SAVE(NucleusRH.Base.Legales.Adelantos.ADELANTO NEWDDO,Nomad.NSystem.Proxy.NomadXML DOC_GRILLA_DETALLES,Nomad.NSystem.Proxy.NomadXML DOC_GRILLA_PAGOS, string CREA_TAREA)
        {

            NomadEnvironment.GetTrace().Info("NEWDDO: " + NEWDDO.SerializeAll());
            NomadEnvironment.GetTrace().Info("DOC_GRILLA_DETALLES: " + DOC_GRILLA_DETALLES.ToString());
            NomadEnvironment.GetTrace().Info("DOC_GRILLA_PAGOS: " + DOC_GRILLA_PAGOS.ToString());

            ///////////////////////////////////////////////////////////////////////////////////////////////////////////
            ///////////////////////////////////////////////////DELETE//////////////////////////////////////////////////
            ///////////////////////////////////////////////////////////////////////////////////////////////////////////

            Hashtable deletedPago = new Hashtable();
            Hashtable deletedDetalle = new Hashtable();

            //RECORRO LOS MODOS DE PAGO
            foreach (NucleusRH.Base.Legales.Adelantos.MODO_DE_PAGO modo_pago in NEWDDO.MODO_PAGO)
            {
                int pagoToDel = 1;
                for (NomadXML rowPago = DOC_GRILLA_PAGOS.FirstChild().FirstChild(); rowPago != null; rowPago = rowPago.Next())
                {
                    if (modo_pago.Id == rowPago.GetAttr("id"))
                        pagoToDel = 0;
                }

                if (pagoToDel == 1)
                {
                    deletedPago.Add(modo_pago.Id, modo_pago);
                    NomadEnvironment.GetTrace().Info(":::Eliminar pago::: " + modo_pago.SerializeAll());
                }
                foreach (NucleusRH.Base.Legales.Adelantos.DETALLE detalle in modo_pago.DETALLE)
                {
                    int DetalleToDel = 1;
                    for (NomadXML rowDetalle = DOC_GRILLA_DETALLES.FirstChild().FirstChild(); rowDetalle != null; rowDetalle = rowDetalle.Next())
                    {
                        if (detalle.Id == rowDetalle.GetAttr("id"))
                            DetalleToDel = 0;
                    }
                    if (DetalleToDel == 1)
                    {
                        deletedDetalle.Add(detalle.Id, detalle);
                        NomadEnvironment.GetTrace().Info(":::Eliminar detalle::: " + detalle.SerializeAll());
                    }
                }
            }

            foreach (MODO_DE_PAGO deleted_pago in deletedPago.Values)
            {
                NomadLog.Info("deleted_pago:: " + deleted_pago.SerializeAll());
                NEWDDO.MODO_PAGO.Remove(deleted_pago);
                NomadLog.Info("NEWDDO-todelete:: " + NEWDDO.SerializeAll());
            }
            NomadEnvironment.GetTrace().Info(":::pagos eliminados::: ");

            foreach (DETALLE deleted_det in deletedDetalle.Values)
            {

                foreach (NucleusRH.Base.Legales.Adelantos.MODO_DE_PAGO modo_pago in NEWDDO.MODO_PAGO)
                {
                    if (deleted_det.oi_modo_de_pago.ToString() == modo_pago.Id)
                    {
                        NomadLog.Info("deleted_det:: " + deleted_det.SerializeAll());
                        modo_pago.DETALLE.Remove(deleted_det);
                    }
                }
            }
            NomadEnvironment.GetTrace().Info(":::detalles eliminados::: ");

            //if(deletedPago.Count != 0 || deletedDetalle.Count != 0)
            //NomadEnvironment.GetCurrentTransaction().SaveRefresh(NEWDDO);
            NomadEnvironment.GetTrace().Info("NEWDDO: " + NEWDDO.SerializeAll());

            ///////////////////////////////////////////////////////////////////////////////////////////////////////////
            ///////////////////////////////////////////////INSERT/UPDATE///////////////////////////////////////////////
            ///////////////////////////////////////////////////////////////////////////////////////////////////////////

            string strStep = "";
            try
            {

                strStep = "1";
                for (NomadXML rowPago = DOC_GRILLA_PAGOS.FirstChild().FirstChild(); rowPago != null; rowPago = rowPago.Next())
                {
                    strStep = "2";
                    NucleusRH.Base.Legales.Adelantos.MODO_DE_PAGO new_pago;
                    string IdPago = "0";
                    foreach (NucleusRH.Base.Legales.Adelantos.MODO_DE_PAGO modo_pago in NEWDDO.MODO_PAGO)
                    {
                        strStep = "3";
                        if (modo_pago.Id == rowPago.GetAttr("id"))
                            {
                                strStep = "4";
                            IdPago = rowPago.GetAttr("id");
                            }
                    }

                    if (IdPago == "0")
                    {
                        strStep = "5";
                        new_pago = new NucleusRH.Base.Legales.Adelantos.MODO_DE_PAGO();
                        NEWDDO.MODO_PAGO.Add(new_pago);
                        strStep = "6";
                    }
                    else
                    {
                        strStep = "7";
                        new_pago = (MODO_DE_PAGO)NEWDDO.MODO_PAGO.GetById(rowPago.GetAttr("id"));
                        strStep = "8";
                    }

                    strStep = "9";
                    new_pago.c_modo_de_pago = rowPago.GetAttr("c_modo_de_pago");
                    new_pago.o_modo_de_pago = rowPago.GetAttr("o_modo_de_pago");
                    //convierto de string a entero para luego setear la fecha en new_pago.f_pago
                    int dia = Int16.Parse(rowPago.GetAttr("f_pago").Substring(6,2));
                    int mes = Int16.Parse(rowPago.GetAttr("f_pago").Substring(4,2));
                    int anio = Int16.Parse(rowPago.GetAttr("f_pago").Substring(0,4));

                    new_pago.f_pago = new DateTime(anio, mes, dia, 0, 0, 0);

                    NomadEnvironment.GetTrace().Info("new_pago: " + new_pago.SerializeAll());
                    double importe_pago = 0;

                   // NomadEnvironment.GetTrace().Info("new_detalleLucho00: " + new_detalle.ToString());

                    strStep = "10";
                    for (NomadXML rowDetalle = DOC_GRILLA_DETALLES.FirstChild().FirstChild(); rowDetalle != null; rowDetalle = rowDetalle.Next())
                    {
                        strStep = "11";
                        if (rowDetalle.GetAttr("c_modo_de_pago") == new_pago.c_modo_de_pago)
                        {
                            strStep = "12";
                            NucleusRH.Base.Legales.Adelantos.DETALLE new_detalle = new NucleusRH.Base.Legales.Adelantos.DETALLE();
                            string IdDetalle = "0";
                            foreach (NucleusRH.Base.Legales.Adelantos.MODO_DE_PAGO modo_pago in NEWDDO.MODO_PAGO)
                            {
                                strStep = "13";
                                NomadEnvironment.GetTrace().Info("strStep: " + strStep.ToString());
                                foreach (NucleusRH.Base.Legales.Adelantos.DETALLE detalle in modo_pago.DETALLE)
                                {
                                    strStep = "14";
                                    NomadEnvironment.GetTrace().Info("strStep: " + strStep.ToString());
                                    if (detalle.Id == rowDetalle.GetAttr("id"))
                                        {
                                            strStep = "15";
                                            NomadEnvironment.GetTrace().Info("strStep: " + strStep.ToString());
                                        IdDetalle = rowDetalle.GetAttr("id");
                                        }

                                }
                            }
                            if (IdDetalle == "0")
                            {
                                strStep = "16";
                                NomadEnvironment.GetTrace().Info("strStep: " + strStep.ToString());
                                //new_detalle = new NucleusRH.Base.Legales.Adelantos.DETALLE();
                                new_pago.DETALLE.Add(new_detalle);
                                strStep = "17";
                                NomadEnvironment.GetTrace().Info("strStep: " + strStep.ToString());
                            }
                            else
                            {
                                strStep = "18";
                                NomadEnvironment.GetTrace().Info("strStep: " + strStep.ToString());
                                new_detalle = (DETALLE)new_pago.DETALLE.GetById(rowDetalle.GetAttr("id"));
                                strStep = "19";
                                NomadEnvironment.GetTrace().Info("strStep: " + strStep.ToString());
                            }

                            strStep = "20";
                            NomadEnvironment.GetTrace().Info("strStep: " + strStep.ToString());
                            NomadEnvironment.GetTrace().Info("strStep: " + strStep.ToString());
                            strStep = "20-a";
                            NomadEnvironment.GetTrace().Info("strStep: " + strStep.ToString());
                            //NomadEnvironment.GetTrace().Info("new_detalleLucho: " + new_detalle.ToString());
                            strStep = "20-b";
                            NomadEnvironment.GetTrace().Info("strStep: " + strStep.ToString());
                            NomadEnvironment.GetTrace().Info("rowDetalleLucho: " + rowDetalle.ToString());
                            strStep = "20-c";
                            NomadEnvironment.GetTrace().Info("strStep: " + strStep.ToString());

                            if(new_detalle==null)
                                new_detalle = new NucleusRH.Base.Legales.Adelantos.DETALLE();

                            new_detalle.oi_concepto = rowDetalle.GetAttr("oi_concepto");

                            NomadEnvironment.GetTrace().Info("strStep: " + strStep.ToString());
                            NomadEnvironment.GetTrace().Info("oi_conceptoLucho: " + rowDetalle.GetAttr("oi_concepto"));
                            strStep = "21";
                            NomadEnvironment.GetTrace().Info("strStep: " + strStep.ToString());
                            new_detalle.n_importe = rowDetalle.GetAttrDouble("n_importe");
                            strStep = "22";
                            NomadEnvironment.GetTrace().Info("strStep: " + strStep.ToString());
                            importe_pago += new_detalle.n_importe;
                            strStep = "23";
                            NomadEnvironment.GetTrace().Info("strStep: " + strStep.ToString());
                        }
                    }
                    strStep = "24";
                    new_pago.n_importe_pago = importe_pago;
                }

                NomadEnvironment.GetTrace().Info("NEWDDO-fin: " + NEWDDO.SerializeAll());
                strStep = "25";

                if (CREA_TAREA == "SI")
                {
                    //SETEO EL USUARIO QUE CREA EL ADELANTO
                    NEWDDO.c_usr_crea = NomadProxy.GetProxy().UserEtty;
                }
                //SETEO f_pago CUANDO SE CREA EL ADELANTO
                if(NEWDDO.f_pagoNull)
                {
                    NEWDDO.f_pago = DateTime.Now;
                }

                NomadEnvironment.GetCurrentTransaction().SaveRefresh(NEWDDO);
                strStep = "26";

                //------------
                //parte para agregar una tarea

                if (CREA_TAREA == "SI")
                {
                    NucleusRH.Base.Legales.Adelantos.ADELANTO DDO = NucleusRH.Base.Legales.Adelantos.ADELANTO.Get(NEWDDO.Id);
                    NucleusRH.Base.Legales.Tareas.TAREA new_tarea = NucleusRH.Base.Legales.Tareas.TAREA.New();

                    new_tarea.oi_carpeta = DDO.oi_carpeta;
                    new_tarea.f_tarea = DDO.f_adelanto;
                    new_tarea.f_alta = DateTime.Now;
                    new_tarea.d_tarea = "Rendir Adelanto - " + DDO.Id;
                    new_tarea.oi_estado_tarea = "1";
                    new_tarea.oi_tipo_tarea = "241";
                    new_tarea.oi_clasif_sujeto = DDO.oi_clasif_sujeto;
                    //validar que tenga mail?? si pero no acá.
                    new_tarea.f_vto_tarea = DDO.f_adelanto.AddDays(15);

                    NomadEnvironment.GetTrace().Info("crear tarea fin " + new_tarea.SerializeAll());
                    NomadEnvironment.GetCurrentTransaction().SaveRefresh(new_tarea);

                    //ILT 26/01/2017
                    DDO.oi_tarea = new_tarea.id.ToString();
                    NomadEnvironment.GetCurrentTransaction().Save(DDO);
                }

                //hasta acá

            }
            catch (Exception ex)
            {
                NomadException nmdEx = NomadException.NewInternalException("NucleusRH.Base.Legales.Adelantos.SAVE()", ex);
                nmdEx.SetValue("step", strStep);
                throw nmdEx;
            }
        }

        /// <summary>
        /// Devuelve un XML con la ruta de la imagen
        /// </summary>
        /// <param name="pOIImage"></param>
        /// <returns></returns>
        public static NomadXML SAVE_LOGO(string pOIImage)
        {
            NomadEnvironment.GetTrace().Info("COMENZANDO SAVE_LOGO");
            NomadEnvironment.GetTrace().Info("pOIImage: " + pOIImage.ToString());
            NomadXML xmlRuta = new NomadXML("RUTA");

            NomadProxy objProxy = NomadProxy.GetProxy();
            BINFile objFile;
            string strFileName;

            const string strClase = "NucleusRH.Base.Personal.Imagenes.HEAD";

            objFile = objProxy.BINService().GetFile(strClase, pOIImage);

            strFileName = objFile.SaveFile(objProxy.RunPath + "Nomad\\TEMP\\");

            xmlRuta.SetAttr("ruta", strFileName);
            return xmlRuta;

            //return strFileName;

        }

       /// <summary>
       /// Autor: Luciano Valía
       /// Actualiza el DDO adelanto seteandole la ruta de la imagen
       /// </summary>
       /// <param name="DDO"></param>
       /// <param name="pOIImage"></param>
        public static void SET_RUTA(ref Nomad.NSystem.Proxy.NomadXML DDO, string pOIImage)
       {

           NomadEnvironment.GetTrace().Info("COMENZANDO SAVE_LOGO");
           NomadEnvironment.GetTrace().Info("pOIImage: " + pOIImage.ToString());

           NomadProxy objProxy = NomadProxy.GetProxy();
           BINFile objFile;
           string strFileName;

           const string strClase = "NucleusRH.Base.Personal.Imagenes.HEAD";

           objFile = objProxy.BINService().GetFile(strClase, pOIImage);

           strFileName = objFile.SaveFile(objProxy.RunPath + "Nomad\\TEMP\\");

           DDO.FirstChild().SetAttr("ruta", strFileName);

           NomadXML x = DDO;
           x.FirstChild();

       }

        /// <summary>
        /// Crea el XML para el reporte de Anticipo de Fondos
        /// XML Report = DDO Adelanto + Ruta
        /// </summary>
        /// <param name="pOIImage"></param>
        /// <param name="DDO"></param>
        /// <returns></returns>
        public static Nomad.NSystem.Proxy.NomadXML GENERATE_XML_REPORT(string pOIImage, NucleusRH.Base.Legales.Adelantos.ADELANTO DDO)
        {
            NomadEnvironment.GetTrace().Info("------------------------------------------------");
            NomadEnvironment.GetTrace().Info("COMENZANDO GENERATE_XML_REPORT");
            NomadEnvironment.GetTrace().Info("pOIImage: " + pOIImage.ToString());
            NomadEnvironment.GetTrace().Info("DDO: " + DDO.ToString());
            NomadEnvironment.GetTrace().Info("------------------------------------------------");

            string strFileName = "";
            NomadXML xmlReport = new NomadXML("<REPORT>");
            xmlReport.SetOuterText(DDO.ToString());
            NomadProxy objProxy = NomadProxy.GetProxy();

            if (!System.IO.File.Exists(objProxy.RunPath + "Nomad\\TEMP\\NucleusRH.Base.Personal.Imagenes.HEAD." + pOIImage + ".jpg"))
            {
                NomadLog.Debug("GENERATE_XML_REPORT: No existe el archivo: Nomad\\TEMP\\NucleusRH.Base.Personal.Imagenes.HEAD." + pOIImage + ".jpg");

                //Si no tiene imagen, devuelvo el DDO como estaba
                if (pOIImage == "")
                {
                    NomadLog.Debug("No tiene imagen");
                    return xmlReport.FirstChild();
                }

                BINFile objFile;
                const string strClase = "NucleusRH.Base.Personal.Imagenes.HEAD";

                objFile = objProxy.BINService().GetFile(strClase, pOIImage);

                strFileName = objFile.SaveFile(objProxy.RunPath + "Nomad\\TEMP");
            }
            else
            {
                NomadLog.Debug("GENERATE_XML_REPORT: Existe el archivo: Nomad\\TEMP\\NucleusRH.Base.Personal.Imagenes.HEAD." + pOIImage + ".jpg");
                strFileName = objProxy.RunPath + "Nomad\\TEMP\\NucleusRH.Base.Personal.Imagenes.HEAD." + pOIImage + ".jpg";

            }
            xmlReport.FirstChild().SetAttr("ruta", strFileName);

            return xmlReport.FirstChild();
        }

    public static void RendirPorExceso(int id)
        {
      NucleusRH.Base.Legales.Adelantos.ADELANTO objAdelanto =
        NucleusRH.Base.Legales.Adelantos.ADELANTO.Get(id);

      objAdelanto.l_habilita_exc = !objAdelanto.l_habilita_exc;
      objAdelanto.c_usr_exceso = NomadProxy.GetProxy().UserEtty;
      objAdelanto.f_exceso = DateTime.Now;

      NomadTransaction objTran = new NomadTransaction();

      try
      {
        objTran.Begin();
        objTran.Save(objAdelanto);
        objTran.Commit();
      }
      catch (Exception ex)
      {
        objTran.Rollback();
        throw ex;
      }

        }

    public static void MarcarRendido(int id, string oi_adelanto_exc)
        {

            NucleusRH.Base.Legales.Adelantos.ADELANTO objAdelanto =
                NucleusRH.Base.Legales.Adelantos.ADELANTO.Get(id);

      NomadXML xmlQry = Nomad.NSystem.Base.NomadEnvironment.QueryNomadXML
      (NucleusRH.Base.Legales.EstadosAnt.ESTADO_ANT.Resources.GET_OI_RENDIDO, "");
      string oi_rendido = xmlQry.FirstChild().GetAttr("oi_estado_ant");

      objAdelanto.oi_estado_ant = oi_rendido;
      objAdelanto.oi_adelanto_exc = oi_adelanto_exc;
      objAdelanto.c_usr_rendido = NomadProxy.GetProxy().UserEtty;
      objAdelanto.f_rendido = DateTime.Now;

      NucleusRH.Base.Legales.Tareas.TAREA objTarea = null;
      if (!objAdelanto.oi_tareaNull) objTarea = NucleusRH.Base.Legales.Tareas.TAREA.Get(objAdelanto.oi_tarea);
      if (objTarea != null)
      {
        //Recuperar el OI del estado de tarea 3-Cumplida
        NomadXML xmlParam = new NomadXML("DATA");
        xmlParam.SetAttr("c_estado_tarea", "3");
        NomadXML xmlEstado = Nomad.NSystem.Base.NomadEnvironment.QueryNomadXML
          (NucleusRH.Base.Legales.Estados_Tarea.ESTADO_TAREA.Resources.QRY_EST_TAREA, xmlParam.ToString());

        //Setear estado
        objTarea.oi_estado_tarea = xmlEstado.FirstChild().GetAttr("oi_estado_tarea");
      }

      NomadTransaction objTran = new NomadTransaction();

      try
      {
        objTran.Begin();
        objTran.Save(objAdelanto);
        if (objTarea != null) objTran.Save(objTarea);
        objTran.Commit();
      }
      catch (Exception ex)
      {
        objTran.Rollback();
        throw ex;
      }
    }

    public static void DesmarcarRendido(int id)
        {

            NucleusRH.Base.Legales.Adelantos.ADELANTO objAdelanto =
                NucleusRH.Base.Legales.Adelantos.ADELANTO.Get(id);

      NomadXML xmlQry = Nomad.NSystem.Base.NomadEnvironment.QueryNomadXML
      (NucleusRH.Base.Legales.EstadosAnt.ESTADO_ANT.Resources.GET_OI_ACTIVO, "");
      string oi_activo = xmlQry.FirstChild().GetAttr("oi_estado_ant");

      objAdelanto.oi_estado_ant = oi_activo;
      objAdelanto.oi_adelanto_exc = null;
      objAdelanto.c_usr_rendido = NomadProxy.GetProxy().UserEtty;
      objAdelanto.f_rendido = DateTime.Now;

      NucleusRH.Base.Legales.Tareas.TAREA objTarea = null;
      if (!objAdelanto.oi_tareaNull) objTarea = NucleusRH.Base.Legales.Tareas.TAREA.Get(objAdelanto.oi_tarea);
      if (objTarea != null)
      {
        //Recuperar el OI del estado de tarea 1-A Completar
        NomadXML xmlParam = new NomadXML("DATA");
        xmlParam.SetAttr("c_estado_tarea", "1");
        NomadXML xmlEstado = Nomad.NSystem.Base.NomadEnvironment.QueryNomadXML
          (NucleusRH.Base.Legales.Estados_Tarea.ESTADO_TAREA.Resources.QRY_EST_TAREA, xmlParam.ToString());

        //Setear estado
        objTarea.oi_estado_tarea = xmlEstado.FirstChild().GetAttr("oi_estado_tarea");
      }

      NomadTransaction objTran = new NomadTransaction();

      try
      {
        objTran.Begin();
        objTran.Save(objAdelanto);
        if (objTarea != null) objTran.Save(objTarea);
        objTran.Commit();
      }
      catch (Exception ex)
      {
        objTran.Rollback();
        throw ex;
      }
    }

    //Comienza método AprobarAdelanto
        //Creado por Luciano 07/08/2018
        public static void AprobarAdelanto(int id)
        {
            NucleusRH.Base.Legales.Adelantos.ADELANTO objAdelanto = NucleusRH.Base.Legales.Adelantos.ADELANTO.Get(id);

            //Seteo usuario aprueba y fecha de aprobación
            objAdelanto.c_usr_aprueba = NomadProxy.GetProxy().UserEtty;
            objAdelanto.f_aprueba = DateTime.Now;

            NomadTransaction objTran = new NomadTransaction();

            try
            {
                objTran.Begin();
                objTran.Save(objAdelanto);
                //if (objTarea != null) objTran.Save(objTarea);
                objTran.Commit();
            }
            catch (Exception ex)
            {
                objTran.Rollback();
                throw ex;
            }
        }
        //Comienza método AnularAdelanto
        //Creado por Luciano 07/08/2018
        public static void AnularAdelanto(int id)
        {
            NomadEnvironment.GetTrace().Info("------------------------------------------------");
            NomadEnvironment.GetTrace().Info("COMENZANDO ANULAR ADELANTO");
            NomadEnvironment.GetTrace().Info("OI_ADELANTO: " + id);
            NomadEnvironment.GetTrace().Info("------------------------------------------------");

            NucleusRH.Base.Legales.Adelantos.ADELANTO objAdelanto = NucleusRH.Base.Legales.Adelantos.ADELANTO.Get(id);

            //Seteo usuario anula y fecha de anulación
            objAdelanto.c_usr_anula = NomadProxy.GetProxy().UserEtty;
            objAdelanto.f_anula = DateTime.Now;
            /*
            if (!objAdelanto.f_apruebaNull)
            {
                //Si se anula seteo la fecha en que se aprobó a nulo
                objAdelanto.f_apruebaNull = true;
                //objAdelanto.ChangeAttribute("f_aprobado", "");
            }
            */
            NomadTransaction objTran = new NomadTransaction();

            try
            {
                objTran.Begin();
                objTran.Save(objAdelanto);
                //if (objTarea != null) objTran.Save(objTarea);
                objTran.Commit();
            }
            catch (Exception ex)
            {
                objTran.Rollback();
                throw ex;
            }
        }

        public static void AprobarAdelantoMasivo(NomadXML adelantos)
        {
            NomadEnvironment.GetTrace().Info("-------------------------------------");
            NomadEnvironment.GetTrace().Info("------APROBAR_ADELANTOS_MASIVO-------");
            NomadEnvironment.GetTrace().Info("-------------------------------------");

            foreach (NomadXML adelanto in adelantos.FirstChild().GetChilds())
            {
                //Recupero el adelanto
                string oi_adelanto = adelanto.GetAttr("id");
                //Convert.ToInt32(oi_adelanto);
                NucleusRH.Base.Legales.Adelantos.ADELANTO objAdelantos = NucleusRH.Base.Legales.Adelantos.ADELANTO.Get(oi_adelanto);
                //NucleusRH.Base.Legales.Adelantos.ADELANTO objAdelantos = NucleusRH.Base.Legales.Adelantos.ADELANTO.Get(2303);

                //Seteo usuario aprueba y fecha de aprobación
                objAdelantos.c_usr_aprueba = NomadProxy.GetProxy().UserEtty;
                objAdelantos.f_aprueba = DateTime.Now;

                NomadTransaction objTran = new NomadTransaction();

                try
                {
                    objTran.Begin();
                    objTran.Save(objAdelantos);
                    objTran.Commit();
                }
                catch (Exception ex)
                {
                    objTran.Rollback();
                    throw ex;
                }
            }
        }
    }
}



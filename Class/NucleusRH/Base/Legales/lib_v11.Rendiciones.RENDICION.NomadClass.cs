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

namespace NucleusRH.Base.Legales.Rendiciones
{

    //////////////////////////////////////////////////////////////////////////////////
    //Clase Rendiciones
    public partial class RENDICION : Nomad.NSystem.Base.NomadObject
    {
      /// <summary>
      /// Anula la rendición seteando l_anulado en true y recalculando el monto rendido del adelanto,
      /// sumandole el monto de la rendicion al monto rendido del adelanto.
      /// </summary>
      /// <param name="pstrOIRendicion"></param>
        public static void AnularRendicion(string pstrOIRendicion)
      {
          NomadLog.Debug("--------------------------------------------");
          NomadLog.Debug(" RENDICION.AnularRendicion v1");
          NomadLog.Debug("--------------------------------------------");

        //Recupero el objeto rendicion
        NucleusRH.Base.Legales.Rendiciones.RENDICION objRendicion = RENDICION.Get(pstrOIRendicion);

        //Solo anula la rendicion si no esta anulada
        if (objRendicion.l_anulado)
        {
            throw NomadException.NewMessage("Legales.RENDICION.REND-ANUL-YA");
        }

        foreach (NucleusRH.Base.Legales.Rendiciones.COMP_RENDICION objComp in objRendicion.COMPS_RENDICION)
        {
            if (!objComp.f_transferidoNull)
                throw NomadException.NewMessage("Legales.RENDICION.REND-ANUL-ERROR-GP");
        }
        
        
        //Recupero el adelanto de la rendicion
        NucleusRH.Base.Legales.Adelantos.ADELANTO objAdelanto =
            NucleusRH.Base.Legales.Adelantos.ADELANTO.Get(objRendicion.oi_adelanto);
           
        //Seteo L_anulado en 1 para anular la rendicion
        objRendicion.l_anulado = true;

        //Actualizo el monto pendiente del adelanto, sumandole el monto rendido anulado
        objAdelanto.n_monto_a_rendir = objAdelanto.n_monto_a_rendir + objRendicion.n_monto_rendicion;

        string strOiAdelanto = "";
        //Actualizo el estado del anticipo
        if (objAdelanto.n_monto_a_rendir == objAdelanto.n_monto_adel)
        {
            //Estado Activo
            NomadXML xmlQry = Nomad.NSystem.Base.NomadEnvironment.QueryNomadXML
                (NucleusRH.Base.Legales.EstadosAnt.ESTADO_ANT.Resources.GET_OI_ACTIVO, "");
            NomadXML xmlResult = xmlQry.FirstChild();
            strOiAdelanto = xmlResult.GetAttr("oi_estado_ant");
                
        }
        else
        {                               
            //Sino quedo Parcialmente Rendido
            NomadXML xmlQry = Nomad.NSystem.Base.NomadEnvironment.QueryNomadXML
                (NucleusRH.Base.Legales.EstadosAnt.ESTADO_ANT.Resources.GET_OI_PARCIAL, "");
            NomadXML xmlResult = xmlQry.FirstChild();
            strOiAdelanto = xmlResult.GetAttr("oi_estado_ant");                                             
        }
        objAdelanto.oi_estado_ant = strOiAdelanto;
                    

        //Estado de Tarea asociada = 1-A Completar
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
            
        //Inicio la transaccion
        objTran.Begin();
        try
        {                           
            //Guardo el adelanto
            objTran.SaveRefresh(objAdelanto);
            //Guardo la tarea (si existe)
            if (objTarea != null) objTran.Save(objTarea);
            //Guardo la Rendicion
            objTran.SaveRefresh(objRendicion);
            //Commit de los cambios
            objTran.Commit();                                
        }
        catch (NomadException)
        {
            objTran.Rollback();
            //Error al intentar anular rendicion
            throw NomadException.NewMessage("Legales.RENDICION.REND-ANUL-ERROR");
        }
            
        //Mensaje de confirmacion de Anulacion
        //throw NomadException.NewMessage("Legales.RENDICION.REND-ANUL-OK");
   
      }


        /// <summary>
        /// Guarda la Rendición y el o los documentos adjuntos de la misma
        /// </summary>
        public static void GuardarRendicion(NucleusRH.Base.Legales.Rendiciones.RENDICION DDO)
        {
            string mensaje;

            //OBTENER EL ADELANTO
            NucleusRH.Base.Legales.Adelantos.ADELANTO objAdelanto =
                NucleusRH.Base.Legales.Adelantos.ADELANTO.Get(DDO.oi_adelanto);

            //MONTO DE RENDICION NEGATIVA
            if (DDO.n_monto_rendicion < 0)
            {
                //VALIDAR QUE EL VALOR ABSOLUTO DEL MONTO DE LA RENDICION NO SUPERE LO RENDIDO DEL ADELANTO
                double monto_rendido = objAdelanto.n_monto_adel - objAdelanto.n_monto_a_rendir;

                if ((DDO.n_monto_rendicion * -1) > monto_rendido)
                {
                    mensaje = "Monto rendido del Adelanto: $" + String.Format("{0:0.00}", monto_rendido) + "\n";
                    mensaje += "Monto total de ésta Rendición: $" + String.Format("{0:0.00}", DDO.n_monto_rendicion);
                    throw NomadException.NewMessage("Legales.RENDICION.MONTO-REND-ERROR", mensaje);                
                }
            }
            //MONTO DE RENDICION POSITIVA
            else
            {
                //VALIDAR QUE EL MONTO DE LA RENDICION NO SUPERE EL PENDIENTE DEL ADELANTO
                if (DDO.n_monto_rendicion > objAdelanto.n_monto_a_rendir)
                {
                    mensaje = "Monto por rendir del Adelanto: $" + String.Format("{0:0.00}", objAdelanto.n_monto_a_rendir) + "\n";
                    mensaje += "Monto total de ésta Rendición: $" + String.Format("{0:0.00}", DDO.n_monto_rendicion);
                    throw NomadException.NewMessage("Legales.RENDICION.MONTO-REND-ERROR", mensaje);
                }
            }

            //OBTENER CONCEPTOS DEL ADELANTO Y SUS MONTOS (AGRUPADO POR CONCEPTO)
            NomadXML xmlParam1 = new NomadXML("DATA");
            xmlParam1.SetAttr("oi_adelanto", objAdelanto.id);
            NomadXML xmlConceptos = Nomad.NSystem.Base.NomadEnvironment.QueryNomadXML
                (NucleusRH.Base.Legales.Adelantos.DETALLE.Resources.CONCEPTOS_MONTOS, xmlParam1.ToString());

            //RECORRER CADA CONCEPTO DEL ADELANTO Y OBTENER MONTO QUE SE ESTA RINDIENDO
            foreach (NomadXML concepto in xmlConceptos.FirstChild().GetChilds())
            {
                string oi_concepto = concepto.GetAttr("oi_concepto");
                double n_total = StringUtil.str2dbl(concepto.GetAttr("n_total"));
                double rindiendo = 0;
                bool IsConceptoInRend = false;

                //RECORRER CADA COMPROBANTE DE LA RENDICION
                foreach (NucleusRH.Base.Legales.Rendiciones.COMP_RENDICION comp_rend in DDO.COMPS_RENDICION)
                {
                    double total_concepto = 0;
                    bool IsConceptoInComp = false;

                    //Recuperar el Tipo de Comprobante
                    NucleusRH.Base.Legales.TiposComprobante.TIPO_COMPROB tipo_comprob =
                        NucleusRH.Base.Legales.TiposComprobante.TIPO_COMPROB.Get(comp_rend.oi_tipo_comprob);

                    //RECORRER CADA CONCEPTO DEL COMPROBANTE
                    foreach (NucleusRH.Base.Legales.Rendiciones.CONCEPTO_COMP conc_comp in comp_rend.CONCEPTOS_COMP)
                    {
                        if (conc_comp.oi_concepto == oi_concepto)
                        {
                            IsConceptoInComp = true;
                            total_concepto += conc_comp.n_total;
                        }
                    }

                    //SI EL CONCEPTO ESTA INCLUIDO EN ESTE COMPROBANTE
                    if (IsConceptoInComp)
                    {
                        //LE SUMO EL PROPORCIONAL DE OTROS IMPUESTOS
                        double total_conceptos_comp = comp_rend.n_subtotal + comp_rend.n_iva;
                        double proporcional = (total_concepto / total_conceptos_comp) * comp_rend.n_otros_impuestos;

                        IsConceptoInRend = true;

                        //Si el Tipo de Comprobante es Nota de Credito se resta
                        if (tipo_comprob.c_tipo == "NC")
                            rindiendo = rindiendo - total_concepto - proporcional;
                        //Si es otro Tipo de Comprobante se suma
			            else                        
                            rindiendo = rindiendo + total_concepto + proporcional;
                    }
                }

                //SI EL CONCEPTO ESTA INCLUIDO EN ESTA RENDICION
                if (IsConceptoInRend)
                {
                    //OBTENER RENDIDO Y PENDIENTE PARA EL CONCEPTO
                    NomadXML xmlParam2 = new NomadXML("DATA");
                    xmlParam2.SetAttr("oi_adelanto", objAdelanto.id);
                    xmlParam2.SetAttr("oi_concepto", oi_concepto);
                    NomadXML xmlRendido = Nomad.NSystem.Base.NomadEnvironment.QueryNomadXML(
                    NucleusRH.Base.Legales.Adelantos.DETALLE.Resources.CONCEPTO_RENDIDO, xmlParam2.ToString());
                    double rendido = StringUtil.str2dbl(xmlRendido.FirstChild().GetAttr("rendido"));
                    double pendiente = n_total - rendido;

                    if (rindiendo < 0)
                    {
                        //VALIDAR QUE EL VALOR ABSOLUTO DE LO QUE SE ESTA RINDIENDO DEL CONCEPTO NO SUPERE LO RENDIDO                   
                        if ((rindiendo * -1) > rendido)
                        {
                            //OBTENER CONCEPTO
                            NucleusRH.Base.Legales.Conceptos.CONCEPTO objConcepto =
                                NucleusRH.Base.Legales.Conceptos.CONCEPTO.Get(oi_concepto);

                            mensaje = "Concepto: [" + objConcepto.c_concepto + "] " + objConcepto.d_concepto + "\n";
                            mensaje += "Monto rendido del Concepto: $" + String.Format("{0:0.00}", rendido) + "\n";
                            mensaje += "Monto total del Concepto en ésta Rendición: $" + String.Format("{0:0.00}", rindiendo);
                            throw NomadException.NewMessage("Legales.RENDICION.MONTO-CONC-ERROR", mensaje);
                        }
                    }
                    else
                    {
                        //VALIDAR QUE LO QUE SE ESTA RINDIENDO DEL CONCEPTO NO SUPERE EL PENDIENTE                    
                        if (rindiendo > pendiente)
                        {
                            //OBTENER CONCEPTO
                            NucleusRH.Base.Legales.Conceptos.CONCEPTO objConcepto =
                                NucleusRH.Base.Legales.Conceptos.CONCEPTO.Get(oi_concepto);

                            mensaje = "Concepto: [" + objConcepto.c_concepto + "] " + objConcepto.d_concepto + "\n";
                            mensaje += "Monto pendiente de rendir para el Concepto: $" + String.Format("{0:0.00}", pendiente) + "\n";
                            mensaje += "Monto total del Concepto en ésta Rendición: $" + String.Format("{0:0.00}", rindiendo);
                            throw NomadException.NewMessage("Legales.RENDICION.MONTO-CONC-ERROR", mensaje);
                        }                    
                    }
                }

            }//POR CADA CONCEPTO DEL ADELANTO


            NucleusRH.Base.Legales.Tareas.TAREA objTarea = null;
            if (!objAdelanto.oi_tareaNull) objTarea = NucleusRH.Base.Legales.Tareas.TAREA.Get(objAdelanto.oi_tarea);

            NomadXML xmlQry, xmlParam, xmlEstado, xmlResult;
                        
            if (objAdelanto.n_monto_a_rendir == DDO.n_monto_rendicion)
            {                
                //Estado de Anticipo = Rendido
                xmlQry =  Nomad.NSystem.Base.NomadEnvironment.QueryNomadXML
                    (NucleusRH.Base.Legales.EstadosAnt.ESTADO_ANT.Resources.GET_OI_RENDIDO, "");
                
                //Estado de Tarea asociada = 3-Cumplida
                if (objTarea != null)
                {
                    //Recuperar el OI del estado de tarea 3-Cumplida
                    xmlParam = new NomadXML("DATA");
                    xmlParam.SetAttr("c_estado_tarea", "3");
                    xmlEstado = Nomad.NSystem.Base.NomadEnvironment.QueryNomadXML
                        (NucleusRH.Base.Legales.Estados_Tarea.ESTADO_TAREA.Resources.QRY_EST_TAREA, xmlParam.ToString());

                    //Setear estado
                    objTarea.oi_estado_tarea = xmlEstado.FirstChild().GetAttr("oi_estado_tarea");
                }               
            }
            else
            {
                //Estado de Anticipo = Parcialmente Rendido
                xmlQry = Nomad.NSystem.Base.NomadEnvironment.QueryNomadXML
                    (NucleusRH.Base.Legales.EstadosAnt.ESTADO_ANT.Resources.GET_OI_PARCIAL, "");                                    
            }

            xmlResult = xmlQry.FirstChild();            
            objAdelanto.oi_estado_ant = xmlResult.GetAttr("oi_estado_ant");
            objAdelanto.n_monto_a_rendir = objAdelanto.n_monto_a_rendir - DDO.n_monto_rendicion;

            NomadTransaction objTran = new NomadTransaction();

            try
            {   
                objTran.Begin();                
                objTran.Save(objAdelanto);
                if (objTarea != null) objTran.Save(objTarea);
                objTran.Save(DDO);               
                objTran.Commit();
            } 
            catch (Exception ex)
            {                
                if (objTran != null)
                    objTran.Rollback();

                NomadException nmdEx = NomadException.NewInternalException("NucleusRH.Base.Legales.Rendiciones.GuardarRendicion()", ex);
                throw nmdEx;
            }


            /*
            NomadLog.Debug("--------------------------------------------");
            NomadLog.Debug(" RENDICION.GuardarRendicion v1");
            NomadLog.Debug("--------------------------------------------");

            NomadLog.Info("ParametrosLucho: " + DDO.ToString());

            NomadLog.Debug("--------------------------------------------");
            NomadLog.Debug("Validaciones del lado del servidor");
            NomadLog.Debug("--------------------------------------------");
            //Validaciones del Lado del Servidor

            string strStep = "GETADELANTO";
            NomadTransaction objTrans = null;

            try
            {
                //Recupero el adelanto
                NucleusRH.Base.Legales.Adelantos.ADELANTO objAdelanto = null;
                objAdelanto = NucleusRH.Base.Legales.Adelantos.ADELANTO.Get(DDO.oi_adelanto);

                //Valido que la rendicion no este anulada
                strStep = "VALIDANDO";
                if (DDO.l_anulado)
                {
                    NomadLog.Info("Rendicion Anulada-Error" + DDO.l_anulado.ToString());
                    throw NomadException.NewMessage("Legales.RENDICION.REND-ALTA-ERROR-ANULADA");
                }

                ////Valido codigos de documento no repetidos en la lista
                //if (!(validarDocumentos(DDO)))
                //{
                //    NomadLog.Info("Documentos repetidos en la lista" + DDO.DOC_RENDICION);
                //    throw NomadException.NewMessage("Legales.RENDICION.REND-ALTA-ERROR-REPLIST");
                //}
                ////Valido codigos de documento no repetidos en la base de datos
                //if (!(validarDocumentosDB(DDO)))
                //{
                //    NomadLog.Info("Documentos repetidos en la BD" + DDO.DOC_RENDICION);
                //    throw NomadException.NewMessage("Legales.RENDICION.REND-ALTA-ERROR-REPBD");
                //}
                //Valido que haya documentos adjuntos y que su monto no supere el monto pendiente de rendir
                strStep = "CALCULANDOMONTO";
                Double dblMonto = 0;
                foreach (NucleusRH.Base.Legales.Rendiciones.DOCUMENTO docs in DDO.DOC_RENDICION)
                {
                    dblMonto = dblMonto + docs.n_documento;
                }
                if (dblMonto == 0)
                {
                    NomadLog.Info("Cargue algun documento" + dblMonto.ToString());
                    throw NomadException.NewMessage("Legales.RENDICION.REND-ALTA-ERROR-NODOCS");
                }

                //Valido que el monto totalizado de los documentos no supere el monto pendiente de rendir
                strStep = "VALIDANDOMONTO";
                double dblMontoTruncate = Math.Truncate(dblMonto*1000)/1000;
                if (dblMontoTruncate > objAdelanto.n_monto_a_rendir)
                {
                    throw NomadException.NewMessage("Legales.RENDICION.REND-ALTA-ERROR-MONTO", dblMonto.ToString(), objAdelanto.n_monto_a_rendir.ToString());
                }

                //VALIDO QUE LA SUMA DE MONTOS POR CONCEPTO NO EXCEDA EL PENDIENTE POR CONCEPTO
                strStep = "VALIDANDO-DOCS";

                strStep = "CREANDO-PARAMETRO";
                NomadXML xmlParam;
                xmlParam = new NomadXML("DATA");
                xmlParam.SetAttr("oi_adelanto", DDO.oi_adelanto);

                strStep = "CREANDO-TOTAL";
                NomadXML xmlQryTotal = Nomad.NSystem.Base.NomadEnvironment.QueryNomadXML(NucleusRH.Base.Legales.Rendiciones.DOCUMENTO.Resources.GET_TOTAL_POR_CONCEPTO, xmlParam.ToString());
                xmlQryTotal = xmlQryTotal.FirstChild();
                
                strStep = "CREANDO-RENDIDO";
                NomadXML xmlQryRendido = Nomad.NSystem.Base.NomadEnvironment.QueryNomadXML(NucleusRH.Base.Legales.Rendiciones.DOCUMENTO.Resources.GET_RENDIDO_POR_CONCEPTO,xmlParam.ToString());
                xmlQryRendido = xmlQryRendido.FirstChild();

                strStep = "CALCULANDO-PENDIENTE";
                NomadXML xmlPendiente = new NomadXML("ROWS");
                //RECORRO LOS TOTALES
                for (NomadXML rowTotal = xmlQryTotal.FirstChild(); rowTotal != null; rowTotal = rowTotal.Next())
                {
                    
                    //RECORRO LO RENDIDO
                    for (NomadXML rowRendido = xmlQryRendido.FirstChild(); rowRendido != null; rowRendido = rowRendido.Next())
                    {
                        if (rowTotal.GetAttr("oi_concepto") == rowRendido.GetAttr("oi_concepto"))
                        {
                            double dblTotal = rowTotal.GetAttrDouble("total");
                            double dblRendido = rowRendido.GetAttrDouble("rendido");
                            //string strPendiente =(dblTotal - dblRendido).ToString();
                            double dblPendiente = Math.Round(dblTotal - dblRendido,2);
                            //Ajusto el redondeo
                            double dblPendienteTruncate = Math.Truncate(dblPendiente * 1000) / 1000;
                            
                            xmlPendiente.AddHeadElement("ROW");
                            xmlPendiente.FirstChild().SetAttr("oi_concepto",rowTotal.GetAttr("oi_concepto"));
                            xmlPendiente.FirstChild().SetAttr("pendiente", dblPendienteTruncate);                       
                            //Indico que ese total se actiaulizo
                            rowTotal.SetAttr("actualizado", "1");

                            break;
                        }                                            
                    }
                  
                }
                //Agrego a xmlPendiente los no actualizados del total
                for (NomadXML rowTotal = xmlQryTotal.FirstChild(); rowTotal != null; rowTotal = rowTotal.Next())
                { 
                    if(rowTotal.GetAttr("actualizado")!="1")
                    {
                        xmlPendiente.AddHeadElement("ROW");
                        xmlPendiente.FirstChild().SetAttr("oi_concepto",rowTotal.GetAttr("oi_concepto"));
                        //Ajusto redondeo
                        double dblTotalAux = Math.Truncate(rowTotal.GetAttrDouble("total") * 1000) / 1000;

                        xmlPendiente.FirstChild().SetAttr("pendiente", dblTotalAux);                    
                    }
                }
               
                strStep = "TOTALIZANDO-CONCEPTOS";
                NomadXML xmlDocs = new NomadXML("ROWS");
                //
                bool encontro = false;
                //RECORRO LOS DOCUMENTOS DEL DDO
                foreach(NucleusRH.Base.Legales.Rendiciones.DOCUMENTO doc in DDO.DOC_RENDICION)
                {
                    encontro = false;
                   //RECORRO LOS DOCUMENTOS  
                 for (NomadXML rowDoc = xmlDocs.FirstChild(); rowDoc != null; rowDoc = rowDoc.Next())
                    {
                       if(rowDoc.GetAttr("oi_concepto")==doc.oi_concepto)
                       {
                       encontro = true;
                       double dblAux = rowDoc.GetAttrDouble("total") + doc.n_documento;
                       double dblAuxTruncate = Math.Truncate(dblAux * 1000) / 1000;
                       rowDoc.SetAttr("total", dblAuxTruncate);
                       break;                       
                       }                                      
                    }
                 if (!encontro)
                 {
                     xmlDocs.AddHeadElement("ROW");
                     xmlDocs.FirstChild().SetAttr("oi_concepto", doc.oi_concepto);
                     double dblAuxMon = Math.Truncate(doc.n_documento * 1000) / 1000;
                     xmlDocs.FirstChild().SetAttr("total", dblAuxMon);
                     continue;
                 }
                 
                }

                strStep = "COMPARANDO-DOCS";
                //RECORRO XMLDOCS
                for (NomadXML rowDoc = xmlDocs.FirstChild(); rowDoc != null; rowDoc = rowDoc.Next())
                {
                   encontro = false;
                    //RECORRO LO PENDIENTE
                   for (NomadXML rowPendiente = xmlPendiente.FirstChild(); rowPendiente != null; rowPendiente = rowPendiente.Next())
                   {
                       if (rowDoc.GetAttr("oi_concepto") == rowPendiente.GetAttr("oi_concepto"))
                       {
                           encontro = true;
                           if (rowDoc.GetAttrDouble("total") <= rowPendiente.GetAttrDouble("pendiente"))
                           {
                               break;
                           }
                           else
                           {
                               //ERROR
                               throw NomadException.NewMessage("Legales.RENDICION.REND-ERROR-DOCS");
                           }
                       }
                       
                   }
                   if (!encontro)
                   {
                       throw NomadException.NewMessage("Legales.RENDICION.REND-ERROR-DOCS");
                   }
                }


               
               
                
                              
                //Todo OK, actualizo objeto adelanto y objeto rendicion y guardo

                //Valido que el monto de la rendicion llegue bien
                if (dblMontoTruncate != DDO.n_monto_rendicion) DDO.n_monto_rendicion = dblMontoTruncate;

                //Si el monto de la rendicion es igual al pendiente, cierro el adelanto
                if (objAdelanto.n_monto_a_rendir == DDO.n_monto_rendicion)
                {
                   objAdelanto.n_monto_a_rendir = objAdelanto.n_monto_a_rendir - DDO.n_monto_rendicion;
                   
                   //Recupera el oi_estado_ant del d_estado_ant='Rendido' y setea ese oi como oi_estado_ant del Adelanto
                   strStep = "RECUPERANDO-OI-RENDIDO";
                   NomadXML xmlQry =  Nomad.NSystem.Base.NomadEnvironment.QueryNomadXML(NucleusRH.Base.Legales.EstadosAnt.ESTADO_ANT.Resources.GET_OI_RENDIDO, "");
                   NomadXML xmlResult = xmlQry.FirstChild();
                   string strOiAdelanto = xmlResult.GetAttr("oi_estado_ant");
                    
                   objAdelanto.oi_estado_ant = strOiAdelanto;
                }
                else
                {   //Sino, es parcialemente Rendido el adelanto
                    strStep = "RECUPERANDO-OI-PARCIALMENTE-RENDIDO";
                    NomadXML xmlQry = Nomad.NSystem.Base.NomadEnvironment.QueryNomadXML(NucleusRH.Base.Legales.EstadosAnt.ESTADO_ANT.Resources.GET_OI_PARCIAL, "");
                    NomadXML xmlResult = xmlQry.FirstChild();
                    string strOiAdelanto = xmlResult.GetAttr("oi_estado_ant");
                    objAdelanto.oi_estado_ant = strOiAdelanto;
                    objAdelanto.n_monto_a_rendir = objAdelanto.n_monto_a_rendir - DDO.n_monto_rendicion;
                }

                strStep = "GUARDANDO";
                objTrans = new NomadTransaction();
                objTrans.Begin();
                strStep = "GUARDANDO-ADELANTO";
                objTrans.Save(objAdelanto);
                strStep = "GUARDANDO-DDO";
                objTrans.Save(DDO);

                strStep = "GUARDANDO-COMMIT";
                objTrans.Commit();

            } 
            catch (Exception ex)
            {
                NomadException nmdEx = NomadException.NewInternalException("NucleusRH.Base.Legales.Rendiciones.GuardarRendicion()", ex);
                nmdEx.SetValue("step", strStep);

                if (objTrans != null)
                    objTrans.Rollback();

                throw nmdEx;
            }
            */
        }


        /// <summary>
        /// Valida que el codigo del documento a ingresar no este repetido en la lista de documentos
        /// 
        /// Devuelve TRUE si est atodo OK
        /// FALSE si se repite
        /// </summary>
        /// <returns></returns>
        public static bool validarDocumentos(NucleusRH.Base.Legales.Rendiciones.RENDICION DDO)
        {
            NomadLog.Debug("--------------------------------------------");
            NomadLog.Debug(" Validar Documentos");
            NomadLog.Debug("--------------------------------------------");
            
            //Valido que el documento no este repetido en la lista
            bool encontro = false;
            bool salir = false;
            for (int i = 0; i < DDO.DOC_RENDICION.Count; i++)
            {                
                for (int j = 0; j < DDO.DOC_RENDICION.Count; j++)
                {
                   string strDocI = DDO.DOC_RENDICION[i].GetAttribute("c_documento").ToString();
                   string strDocJ = DDO.DOC_RENDICION[j].GetAttribute("c_documento").ToString();
                   
                    if (( strDocI==strDocJ ) && (i != j))
                    { 
                        encontro = true;
                        salir = true;
                        break;                     
                    }
                }
                //Para salir de la segunda estructura de repeticion cuando encuentre un error
                if (salir) break;
            
            }
            if (encontro)
                return false;
            else
            {                
                return true;
            }
        }

        /// <summary>
        /// Valida que no se repita algun documento de la lista con uno existente
        /// Devuelve TRUE si esta todo OK 
        /// Devuelve FALSE si hay alguno repetido
        /// </summary>
        /// <param name="DDO"></param>
        /// <returns></returns>
        public static bool validarDocumentosDB(NucleusRH.Base.Legales.Rendiciones.RENDICION DDO)
        {
            //Valido cada documento a cargar contra la base de datos
            NomadLog.Info("Validando documentos de la lista con los c_Doc de la DB");
            foreach (NucleusRH.Base.Legales.Rendiciones.DOCUMENTO doc in DDO.DOC_RENDICION)
            {
                //Crea el parametro
                NomadXML xmlParam;
                xmlParam = new NomadXML("DATA");
                xmlParam.SetAttr("c_documento", doc.c_documento);
                //Recupero el c_documento
                NomadXML xmlQuery = Nomad.NSystem.Base.NomadEnvironment.QueryNomadXML(NucleusRH.Base.Legales.Rendiciones.DOCUMENTO.Resources.C_DOCUMENTO_REPETIDO, xmlParam.ToString());
                NomadXML xmlResult = xmlQuery.FirstChild();
                string strEncontro = xmlResult.GetAttr("flag");
                
                if (strEncontro == "1")
                {
                    return false;
                }
            }
            return true;
        }


        public static NucleusRH.Base.Legales.Rendiciones.RENDICION AddComprobante
           (NucleusRH.Base.Legales.Rendiciones.RENDICION rendicion,
            NucleusRH.Base.Legales.Rendiciones.COMP_RENDICION comprobante)
        {
            rendicion.COMPS_RENDICION.Add(comprobante);
            rendicion.n_monto_rendicion += comprobante.n_total;

            return rendicion;
        }

        public static NucleusRH.Base.Legales.Rendiciones.RENDICION CalcularTotal
           (NucleusRH.Base.Legales.Rendiciones.RENDICION rendicion)        
        {
            double n_monto_rendicion = 0;
            foreach(NucleusRH.Base.Legales.Rendiciones.COMP_RENDICION comprobante in rendicion.COMPS_RENDICION)
            {
                //Recuperar el Tipo de Comprobante
                NucleusRH.Base.Legales.TiposComprobante.TIPO_COMPROB tipo_comprob =
                    NucleusRH.Base.Legales.TiposComprobante.TIPO_COMPROB.Get(comprobante.oi_tipo_comprob);
                
                //Si el Tipo de Comprobante es Nota de Credito se resta el total del Comprobante
                if (tipo_comprob.c_tipo == "NC")
                    n_monto_rendicion = n_monto_rendicion - comprobante.n_total;
                //Si es otro Tipo de Comprobante se suma el total del Comprobante
                else
                    n_monto_rendicion = n_monto_rendicion + comprobante.n_total;
            }
            rendicion.n_monto_rendicion = n_monto_rendicion;

            return rendicion;
        }

    }


    //////////////////////////////////////////////////////////////////////////////////
    //Clase Conceptos del Comprobante
    public partial class COMP_RENDICION : Nomad.NSystem.Base.NomadObject
    {
        public static NucleusRH.Base.Legales.Rendiciones.COMP_RENDICION AddConcepto
            (NucleusRH.Base.Legales.Rendiciones.COMP_RENDICION comprobante,
             NucleusRH.Base.Legales.Rendiciones.CONCEPTO_COMP concepto)
        {
            comprobante.CONCEPTOS_COMP.Add(concepto);
            comprobante.n_subtotal += concepto.n_subtotal;
            comprobante.n_iva += concepto.n_iva;
            comprobante.n_total += concepto.n_subtotal + concepto.n_iva;

            return comprobante;
        }

        public static NucleusRH.Base.Legales.Rendiciones.COMP_RENDICION AddImpuesto
           (NucleusRH.Base.Legales.Rendiciones.COMP_RENDICION comprobante,
            NucleusRH.Base.Legales.Rendiciones.IMPUESTO_COMP impuesto)
        {
            comprobante.IMPUESTOS_COMP.Add(impuesto);
            comprobante.n_otros_impuestos += impuesto.n_importe;
            comprobante.n_total += impuesto.n_importe;

            return comprobante;
        }

        public static NucleusRH.Base.Legales.Rendiciones.COMP_RENDICION CalcularTotales
            (NucleusRH.Base.Legales.Rendiciones.COMP_RENDICION comprobante)
        {
            double n_subtotal = 0;
            double n_subtotal_grav = 0;
            double n_iva = 0;
            double n_otros_impuestos = 0;

            foreach (NucleusRH.Base.Legales.Rendiciones.CONCEPTO_COMP concepto in comprobante.CONCEPTOS_COMP)
            {
                n_subtotal = n_subtotal + concepto.n_subtotal;
                if (!concepto.oi_impuestoNull)
                {
                    n_subtotal_grav = n_subtotal_grav + concepto.n_subtotal;
                    n_iva = n_iva + concepto.n_iva;
                }
            }

            foreach (NucleusRH.Base.Legales.Rendiciones.IMPUESTO_COMP impuesto in comprobante.IMPUESTOS_COMP)
            {
                n_otros_impuestos = n_otros_impuestos + impuesto.n_importe;
            }

            comprobante.n_subtotal = n_subtotal;
            comprobante.n_subtotal_grav = n_subtotal_grav;
            comprobante.n_iva = n_iva;
            comprobante.n_otros_impuestos = n_otros_impuestos;
            comprobante.n_total = n_subtotal + n_iva + n_otros_impuestos;

            return comprobante;
        }

    }


    //////////////////////////////////////////////////////////////////////////////////
    //Clase Conceptos del Comprobante
    public partial class CONCEPTO_COMP : Nomad.NSystem.Base.NomadObject
    {        
        public static Nomad.NSystem.Proxy.NomadXML ConceptosRendicion
            (NucleusRH.Base.Legales.Rendiciones.RENDICION rendicion,
             NucleusRH.Base.Legales.Rendiciones.COMP_RENDICION comprobante,
             string idConcepto)
        {
            NomadLog.Debug("--------------------------------------------");
            NomadLog.Debug("---------- Conceptos de Rendicion ----------");
            NomadLog.Debug("--------------------------------------------");
            NomadLog.Debug("DDO: " + rendicion.ToString());
            NomadLog.Debug("COMP: " + comprobante.ToString());
            NomadLog.Debug("CONC: " + idConcepto);

            NomadXML xmlResult = new NomadXML("CONCEPTOS_REND");

            //Recorrer conceptos cargados en la rendicion (excepto comprobante actual)            
            foreach (NucleusRH.Base.Legales.Rendiciones.COMP_RENDICION comp in rendicion.COMPS_RENDICION)
            {
                if (comp.Id != comprobante.Id)
                {
                    foreach (NucleusRH.Base.Legales.Rendiciones.CONCEPTO_COMP conc in comp.CONCEPTOS_COMP)
                    {
                        NomadXML xmlConcepto = new NomadXML("CONCEPTO_REND");
                        xmlConcepto.SetAttr("oi_concepto", conc.oi_concepto);
                        xmlConcepto.SetAttr("n_total", conc.n_total);
                        xmlResult.AddTailElement(xmlConcepto);                                             
                    }                
                }            
            }
            
            //Recorrer conceptos del comprobante (excepto concepto actual)
            foreach (NucleusRH.Base.Legales.Rendiciones.CONCEPTO_COMP conc in comprobante.CONCEPTOS_COMP)
            {
                if (conc.Id != idConcepto)
                {
                    NomadXML xmlConcepto = new NomadXML("CONCEPTO_REND");
                    xmlConcepto.SetAttr("oi_concepto", conc.oi_concepto);
                    xmlConcepto.SetAttr("n_total", conc.n_total);
                    xmlResult.AddTailElement(xmlConcepto);                    
                }
            }

            NomadLog.Debug("xmlResult: " + xmlResult.ToString());

            return xmlResult;        
        }
    }
}
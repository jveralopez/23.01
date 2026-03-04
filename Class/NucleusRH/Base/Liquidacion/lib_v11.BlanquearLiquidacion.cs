using System;
using System.Collections;
using System.Text;
using System.Xml;
using System.Globalization;

using Nomad.NSystem.Base;
using Nomad.NSystem.Proxy;
using Nomad.NSystem.Functions;


namespace NucleusRH.Base.Liquidacion.Blanquear
{
    class clsBlanquear
    {

        int nError; 
        public clsBlanquear()
        {

        }



        public clsBlanquear(int oi_liquidacion)
        {
            NomadEnvironment.GetTrace().Error("clsBlanquear arranca oi_liquidacion : " + oi_liquidacion.ToString() );

            nError = 0; 
            ejecBlanquear(oi_liquidacion);
            NomadEnvironment.GetTrace().Error("clsBlanquear fin  " );

        }



        //******************************************************************************************
        //******************************************************************************************


        private void ejecBlanquear(int oi_liqui)
        {

            NucleusRH.Base.Liquidacion.Liquidacion.LIQUIDACION objLiq;
            NucleusRH.Base.Liquidacion.Liquidacion.EJECUCION objEjec;
            NucleusRH.Base.Liquidacion.PersonalLiquidacionDDO.PER_LIQ_DDO objPerLiqDDO;
            NucleusRH.Base.Liquidacion.Personal_Liquidacion.PERSONAL_LIQ objPerLiq;
            NucleusRH.Base.Liquidacion.LiquidacionDDO.LIQUIDACION_DDO objLiqDDO;

            string sOi_liq;
            objLiq = NucleusRH.Base.Liquidacion.Liquidacion.LIQUIDACION.Get(oi_liqui);
            try
            {

                if (objLiq.c_estado != "I") { return; }



                objLiq = NucleusRH.Base.Liquidacion.Liquidacion.LIQUIDACION.Get(oi_liqui);
                NomadProxy.GetProxy().Batch().Trace.Add("ifo", "Blanqueando liquidacion " + objLiq.c_liquidacion, "EjecBlanquear");
																
                for (int x = 0; x < objLiq.EJECUCIONES.Count; x++)
                {
                    // obtengo la ejecucion de la liquidacion
                    objEjec = (NucleusRH.Base.Liquidacion.Liquidacion.EJECUCION)objLiq.EJECUCIONES[x];

                    // obtengo la liquidacionDDO
                    objLiqDDO = NucleusRH.Base.Liquidacion.LiquidacionDDO.LIQUIDACION_DDO.Get(objEjec.oi_liquidacion_ddo);
                    sOi_liq = objLiqDDO.id.ToString();

                    //NomadProxy.GetProxy().Batch().Trace.Add("ifo", "Eliminando Ejecución: " + objEjec.e_secuencia, "EjecBlanquear");
                                                            
                    // Elimino los ReciboLiq
                    EliminaRecibosLiq(sOi_liq);
                    NomadProxy.GetProxy().Batch().Trace.Add("ifo", "Eliminando Recibos - Ejecución " + objEjec.e_secuencia, "EjecBlanquear");
                    Nomad.NSystem.Proxy.NomadProxy.GetProxy().Batch().SetProgress(Convert.ToInt32(30 / objLiq.EJECUCIONES.Count));
                    
                    // Elimino los PersonalDDO
                    EliminaPersLiqDDO(sOi_liq);
                    NomadProxy.GetProxy().Batch().Trace.Add("ifo", "Eliminando Datos - Ejecución " + objEjec.e_secuencia, "EjecBlanquear");
                    Nomad.NSystem.Proxy.NomadProxy.GetProxy().Batch().SetProgress(Convert.ToInt32(30 + 30 / objLiq.EJECUCIONES.Count));

                  	// Elimino los PersonalLiquidacion
                    EliminaPersLiq(sOi_liq);
                    NomadProxy.GetProxy().Batch().Trace.Add("ifo", "Eliminando Legajos - Ejecución " + objEjec.e_secuencia, "EjecBlanquear");
                    Nomad.NSystem.Proxy.NomadProxy.GetProxy().Batch().SetProgress(Convert.ToInt32(60 + 30 / objLiq.EJECUCIONES.Count));
 
                }


                // ELIMINO LA LIQUIDACION

                try
                {

                    EliminaPersLiq(oi_liqui.ToString());

                    NomadProxy.GetProxy().Batch().Trace.Add("ifo", "Eliminando Ejecuciones", "EjecBlanquear");
                
                    objLiq.EJECUCIONES.Clear();
                    if (nError == 0)
                    {
                        objLiq.c_estado = "A";
                    }
                    NomadEnvironment.GetCurrentTransaction().Save(objLiq);
                    NomadProxy.GetProxy().Batch().Trace.Add("ifo", "Blanqueando Fin de Proceso", "EjecBlanquear");
                    Nomad.NSystem.Proxy.NomadProxy.GetProxy().Batch().SetProgress(100);
                }
                catch (Exception e)
                {

                    NomadEnvironment.GetTrace().Error("EjecBlanquear detalle : " + e.Message);
                    NomadProxy.GetProxy().Batch().Trace.Add("err", "Error al blanquear la liquidacion ", "EjecBlanquear");

                }

            }
            catch (Exception e)
            {
                nError = 1; 
                NomadEnvironment.GetTrace().Error("EjecBlanquear detalle : " + e.Message);
                NomadProxy.GetProxy().Batch().Trace.Add("err", "Error al blanquear la liquidacion ", "EjecBlanquear");
            }

            NomadEnvironment.GetTrace().Error("Fin del proceso ");
        }

        //******************************************************************************************
        //******************************************************************************************

        private void EliminaRecibosLiq(string strOI_Liq)
        {

            System.Xml.XmlDocument xmlDocCal;
            string strWhere;
            string strOI;
            //arrDetalleErr = new ArrayList();

            NucleusRH.Base.Liquidacion.Recibos.TOT_LIQ_PER objRecLiq;

            Array arrCampos;
            strOI = ",";
            arrCampos = strOI.Split(',');

            xmlDocCal = EjecutarQuery(" LIQ20_TOT_LIQ_PER ", " oi_liquidacion_ddo  = " + strOI_Liq, " oi_tot_liq_per ", "", ref arrCampos);

            foreach (System.Xml.XmlNode xmlCal in xmlDocCal.DocumentElement.ChildNodes)
            {
                try
                {
                    strOI = xmlCal.Attributes.Item(0).ChildNodes.Item(0).Value;
                    // obtengo los RECIBOS realcionados con la liquidacion
                   
                    NomadEnvironment.GetTrace().Error("EliminaRecibosLiq_CHILD   : " + strOI);
                    objRecLiq = NucleusRH.Base.Liquidacion.Recibos.TOT_LIQ_PER.Get(strOI);
                    if (EliminaRecibosLiq_CHILD(strOI))
                    {
                        NomadEnvironment.GetTrace().Error("EliminaRecibosLiq  TOT_Liq_Per : " + objRecLiq.Code);

                        NomadEnvironment.GetCurrentTransaction().Begin();
                        NomadEnvironment.GetCurrentTransaction().Delete(objRecLiq);
                        NomadEnvironment.GetCurrentTransaction().Commit();
                    }
                }
                catch (Exception e)
                {
                    nError = 1; 
                    NomadEnvironment.GetTrace().Error("EliminaRecibosLiq detalle : " + e.Message);
                    NomadProxy.GetProxy().Batch().Trace.Add("err", " Error al eliminar Recibos", "EliminaRecibosLiq");
                }
            }


        }



        //******************************************************************************************
        //******************************************************************************************


        private Boolean EliminaRecibosLiq_CHILD(string strOI_Liq)
        {

            System.Xml.XmlDocument xmlDocCal;
            string strWhere;
            string strOI;
            //arrDetalleErr = new ArrayList();

            NucleusRH.Base.Liquidacion.LiquidacionDDO.RECIBO objRecLiq_CH;

            Array arrCampos;
            strOI = ",";
            arrCampos = strOI.Split(',');

            xmlDocCal = EjecutarQuery(" LIQ99_RECIBOS ", " oi_tot_liq_per  = " + strOI_Liq, " oi_recibo ", "", ref arrCampos);
            NomadEnvironment.GetCurrentTransaction().Begin();
            foreach (System.Xml.XmlNode xmlCal in xmlDocCal.DocumentElement.ChildNodes)
            {
                try
                {
                    strOI = xmlCal.Attributes.Item(0).ChildNodes.Item(0).Value;
                    NomadEnvironment.GetTrace().Error("EliminaRecibosLiq_CHILD strOI : " + strOI );
                    // obtengo los RECIBOS realcionados con la liquidacion
                    objRecLiq_CH = NucleusRH.Base.Liquidacion.LiquidacionDDO.RECIBO.Get(strOI);

                    NomadEnvironment.GetCurrentTransaction().Delete(objRecLiq_CH);
                }
                catch (Exception e)
                {
                    nError = 1; 
                    NomadEnvironment.GetTrace().Error("EliminaRecibosLiq_CHILD detalle : " + e.Message);
                    NomadProxy.GetProxy().Batch().Trace.Add("err", "Error al eliminar los recibos asociados", "EliminaRecibosLiq_CHILD");

                }
            }

            if (strOI == ",")// no entro en el ForEach
            {
                NomadEnvironment.GetTrace().Error("EliminaRecibosLiq_CHILD no encontro LIQ99_RECIBOS para oi_tot_liq_per  = " + strOI_Liq);
                NomadEnvironment.GetCurrentTransaction().Rollback(); 
                return true; 
            }

            try
            {
                NomadEnvironment.GetCurrentTransaction().Commit();
                return true;
            }
            catch (Exception e)
            {
                nError = 1; 
                NomadEnvironment.GetCurrentTransaction().Rollback();
                NomadEnvironment.GetTrace().Error("EliminaRecibosLiq_CHILD Rollback detalle : " + e.Message);
                NomadProxy.GetProxy().Batch().Trace.Add("err", "Error al eliminar los recibos asociados", "EliminaRecibosLiq_CHILD");
                return false;

            }


            NomadEnvironment.GetTrace().Error("Elimino RECIBOS OK ");

        }




        //*********************************************************************
        //*********************************************************************
        private void EliminaPersLiqDDO(string strOI_Liq)
        {

            System.Xml.XmlDocument xmlDocCal;
            string strWhere;
            string strOI;
            //arrDetalleErr = new ArrayList();

            NucleusRH.Base.Liquidacion.PersonalLiquidacionDDO.PER_LIQ_DDO objPerLiqDDO;

            Array arrCampos;
            strOI = ",";
            arrCampos = strOI.Split(',');

            xmlDocCal = EjecutarQuery(" Liq98_Personal_liq ", " oi_liquidacion_ddo  = " + strOI_Liq, " oi_per_liq_ddo ", "", ref arrCampos);

            foreach (System.Xml.XmlNode xmlCal in xmlDocCal.DocumentElement.ChildNodes)
            {
                strOI = xmlCal.Attributes.Item(0).ChildNodes.Item(0).Value;
                NomadEnvironment.GetTrace().Error("EliminaPersLiqDDO strOI : " + strOI);
                // obtengo las personas realcionadas con la ejecucion de la liquidacionDDO
                objPerLiqDDO = NucleusRH.Base.Liquidacion.PersonalLiquidacionDDO.PER_LIQ_DDO.Get(strOI);


                try
                {
                    NomadEnvironment.GetCurrentTransaction().Delete(objPerLiqDDO);
                }
                catch (Exception e)
                {
                    nError = 1; 
                    NomadEnvironment.GetTrace().Error("EliminaPersLiqDDO detalle : " + e.Message);
                    NomadProxy.GetProxy().Batch().Trace.Add("err", " Error al eliminar personas asociadas a la liquidacion ", "EliminaPersLiqDDO");
                }

                NomadEnvironment.GetTrace().Error("Elimino Personas OK");
            }
        }

        //*********************************************************************
        //*********************************************************************

        private void EliminaPersLiq(string strOI_Liq)
        {

            System.Xml.XmlDocument xmlDocCal;
            string strWhere;
            string strOI;
            //arrDetalleErr = new ArrayList();

            NucleusRH.Base.Liquidacion.Personal_Liquidacion.PERSONAL_LIQ objPerLiq;

            Array arrCampos;
            strOI = ",";
            arrCampos = strOI.Split(',');

            xmlDocCal = EjecutarQuery(" LIQ25_Personal_liq ", " oi_liquidacion  = " + strOI_Liq, " oi_personal_liq ", "", ref arrCampos);

            foreach (System.Xml.XmlNode xmlCal in xmlDocCal.DocumentElement.ChildNodes)
            {
                strOI = xmlCal.Attributes.Item(0).ChildNodes.Item(0).Value;
                // obtengo las personas de la liquidacion
                NomadEnvironment.GetTrace().Error("EliminaPersLiq strOI   " + strOI);
                objPerLiq = NucleusRH.Base.Liquidacion.Personal_Liquidacion.PERSONAL_LIQ.Get(strOI);
                try
                {
                    NomadEnvironment.GetCurrentTransaction().Delete(objPerLiq);
                }
                catch (Exception e)
                {
                    nError = 1; 
                    NomadEnvironment.GetTrace().Error("EliminaPersLiq detalle : " + e.Message);
                    NomadProxy.GetProxy().Batch().Trace.Add("err", "Error al eliminar personas asociadas a la liquidacion ", "EliminaPersLiq");

                }

                NomadEnvironment.GetTrace().Error("EliminaPersLiq   Elimino OK");
            }
        }



        //******************************************************************************************
        //******************************************************************************************


        private System.Xml.XmlDocument EjecutarQuery(string strTabla, string strWhere, string strCampos, string strCamposQryOut, ref Array arrCampos)
        {
            string varQuery;
            string varQueryParam;

            string sCamposOut;

            if (strCamposQryOut == "")
            {
                arrCampos = strCampos.Split(',');
            }
            else
            {
                arrCampos = strCamposQryOut.Split(',');
            }

            sCamposOut = "";
            for (int n = 0; n < arrCampos.Length; n++)
            {
                sCamposOut = sCamposOut + @"<qry:attribute value=""$r/@" + ((string[])(arrCampos))[n] + @""" name=""" + ((string[])(arrCampos))[n] + @"""/>";
            }

            System.Xml.XmlDocument xmlDocCal;

            //NomadEnvironment.GetTrace().Info("  Clase: BuscaHijos 		Funcion: Comparar"   ) ; 			                                                                                                                                 

            varQuery = @"                                                                                                                                                                                                        
								<qry:main doc=""PARAM"">                                                                                                                                                                                         
										<qry:append-doc name=""FILTRO"" doc-path=""#PARAM:/FILTRO""/>                                                                                                                                                
										<qry:element name=""objects"">                                                                                                                                                                               
											<qry:insert-select doc-path=""#FILTRO:"" name=""filtro_empresa""/>                                                                                                                                         
										</qry:element>                                                                                                                                                                                               
									</qry:main>                                                                                                                                                                                                    
										<qry:select doc=""PARAM"" name=""filtro_empresa"">                                                                                                                                                           
												<qry:xquery>                                                                                                                                                                                             
													for $r in table('	SELECT Distinct " + strCampos + @"	FROM "
                                        + strTabla + @" WHERE " + strWhere + @"  ')/ROWS/ROW" +
                                    @"</qry:xquery>                                                                                                                                                                              
												<qry:out>                                                                                                                                                                                                
													<qry:element name=""objeto""> "
                                                      + sCamposOut +
                                                    @"</qry:element>                                                                                                                                                             
												</qry:out>                                                                                                                                                                                               
											</qry:select>  ";


            //NomadEnvironment.GetTrace().Info("Funcion:  BuscaEspGuardada  varQuery " + varQuery) ;                                                                                                                                   
            varQueryParam = @"<FILTRO />";
            try
            {
                xmlDocCal = NomadEnvironment.QueryXML(varQuery.ToString(), varQueryParam);
                return xmlDocCal;
            }
            catch (Exception e)
            {
                NomadProxy.GetProxy().Batch().Trace.Add("err", e.Message, "EjecIntClientes");
                nError = 1; 

                NomadEnvironment.GetTrace().Error(varQuery.ToString());
                xmlDocCal = new System.Xml.XmlDocument();
                return xmlDocCal;
            }

        }

        /////////////////////////////////////////////////////////                                                                                                                                                                
        // query reutilizable para saber si exsite registro                                                                                                                                                                      
        //                                                                                                                                                                                                                       
        ////////////////////////////////////////////////////////			                                                                                                                                                           

        //******************************************************************************************
        //******************************************************************************************

        public string BuscaHijos(string strTabla, string strWhere, string strCampos, string sOutQry)
        {
            System.Xml.XmlDocument xmlDocCal;

            string strOI;
            string strGetOI;
            Array arrCampos;
            strOI = ",";
            arrCampos = strOI.Split(',');
            xmlDocCal = EjecutarQuery(strTabla, strWhere, strCampos, sOutQry, ref arrCampos);
            strGetOI = "";
            strOI = "";
            try
            {
                foreach (System.Xml.XmlElement xmlCal in xmlDocCal.DocumentElement.ChildNodes)
                {
                    for (int x = 0; x < arrCampos.Length; x++)
                    {
                        strGetOI = ((string[])(arrCampos))[x];
                        strOI = strOI + "," + xmlCal.GetAttribute(strGetOI.Trim());
                    }
                }
                if ((strOI.Length > 1) || (strOI == ","))
                {
                    if (strOI.Substring(0, 1) == ",")
                    { strOI = strOI.Substring(1, strOI.Length - 1); }
                }

                return strOI;
            }
            catch (Exception e)
            {

                strOI = "";
                return strOI;
            }

        }


    }
}











using System;
using System.Collections;
using System.Text;
using System.Xml;
using System.Globalization;

using Nomad.NSystem.Base;
using Nomad.NSystem.Proxy;
using Nomad.NSystem.Functions;


namespace NucleusRH.Base.Liquidacion.IntCtble_Out
{
    class EjecInterfaceContable
    {

        int p_o_int_cont;
        string p_c_empresa;
        string p_e_periodo;

        public EjecInterfaceContable()
        {

        }

        public EjecInterfaceContable(int o_int_cont, string c_empresa, string e_periodo)
        {
            p_o_int_cont = o_int_cont;
            p_c_empresa = c_empresa;
            p_e_periodo = e_periodo;
            ejecObtieneCtas();
        }

        private void ejecObtieneCtas()
        {
            string str_select;
            string strCamposQryOut;


            str_select = "  select liq21_d.c_cuenta, liq21_d.c_d_h ";
            str_select = str_select + " from liq21_det_int_cont as liq21_d  ";
            str_select = str_select + " where liq21_d.oi_int_cont = " + @"\'" + p_o_int_cont.ToString() + @"\'";
            str_select = str_select + " group by liq21_d.c_cuenta, liq21_d.c_d_h  ";

            strCamposQryOut = "c_cuenta,c_d_h";

            ObtenerCuentas(str_select, strCamposQryOut, strCamposQryOut);

        }


        /////////////////////////////////////////////////////////                                                                                                                                                                
        // query reutilizable para saber si exsite registro                                                                                                                                                                      
        //                                                                                                                                                                                                                       
        ////////////////////////////////////////////////////////			                                                                                                                                                           


        private void ObtenerCuentas(string str_Select, string strCampos, string sOutQry)
        {
            System.Xml.XmlDocument xmlDocCal;
            Hashtable rtaHash;
            Hashtable ObjHash;
            string strOI;
            string m_c_cuenta;
            string m_c_d_h;
            Array arrCampos;
            strOI = ",";
            arrCampos = strOI.Split(',');
            xmlDocCal = EjecutarQuery(str_Select, strCampos, sOutQry, ref arrCampos);
            strOI = "";
            int posComaI;
            int posComaD;
            string ImporteDer;
            string ImporteIzq;
            posComaI = 0;
            posComaD = 0;

            ImporteDer = "";
            ImporteIzq = "";

            NucleusRH.Base.Liquidacion.IntContableOut.IntContableOut objOut;

            try
            {
                foreach (System.Xml.XmlElement xmlCal in xmlDocCal.DocumentElement.ChildNodes)
                {
                    m_c_cuenta = xmlCal.Attributes["c_cuenta"].Value;
                    m_c_d_h = xmlCal.Attributes["c_d_h"].Value;

                    rtaHash = ObtenerConceptos(m_c_cuenta, m_c_d_h);
                    foreach (string HLIQ in rtaHash.Keys)
                    {
                        ObjHash = (Hashtable)rtaHash[HLIQ];

                        foreach (string HCC in ObjHash.Keys)
                        {
                            //bjHashCC = (Hashtable)ObjHash[HCC];
                            // Console.WriteLine ("cuenta:  " + m_c_cuenta + " d_h: " + m_c_d_h + " Total: " + ObjHash[HCC] + " CC: " + HCC + " liq: " + HLIQ);
                            objOut = new NucleusRH.Base.Liquidacion.IntContableOut.IntContableOut();
                            posComaI = ObjHash[HCC].ToString().IndexOf(",");
                            posComaD = ObjHash[HCC].ToString().Length - posComaI - 1;
                            ImporteIzq = ObjHash[HCC].ToString().Substring(0, posComaI);
                            ImporteDer = ObjHash[HCC].ToString().Substring(posComaI + 1, posComaD);
                            ImporteIzq = ImporteIzq.PadLeft(15, '0');
                            ImporteDer = ImporteDer.PadLeft(3, '0');

                            posComaI = HLIQ.IndexOf("*");
                            posComaD = HLIQ.Length - posComaI - 1;


                            //Console.WriteLine(m_c_cuenta.PadLeft(10, '0') + HLIQ.Substring(posComaI + 1, posComaD) + ImporteIzq + ImporteDer + m_c_d_h + HCC + HLIQ.Substring(0, posComaI));
                            objOut.CuentaContable = m_c_cuenta.PadLeft(10, '0');
                            objOut.FechaPase = HLIQ.Substring(posComaI + 1, posComaD);
                            objOut.Monto_entero = ImporteIzq;
                            objOut.Monto_Decimal = ImporteDer;
                            objOut.D_H = m_c_d_h;
                            objOut.CuentaContable = HCC;


                            try
                            {
                                NomadEnvironment.GetCurrentTransaction().Save(objOut);
                            }
                            catch (Exception e)
                            {
                                NomadEnvironment.GetTrace().Error(e.Message);
                                NomadEnvironment.GetTrace().Error(" detalle : " + objOut.SerializeAll());
                                NomadProxy.GetProxy().Batch().Trace.Add("err", e.Message, "ObtenerCuentas");
                            }

                        }
                    }
                }

            }
            catch (Exception e)
            {
                NomadEnvironment.GetTrace().Error(e.Message);
            }

        }

        //************************************************************************** 
        //                  ObtenerConceptos                           
        //************************************************************************** 

        private Hashtable ObtenerConceptos(string m_c_cuenta, string m_c_d_h)
        {
            System.Xml.XmlDocument xmlDocCal;
            System.Xml.XmlDocument rta;

            string strOI;
            string str_select;
            double mValor;
            double mValorAcum;
            string strCamposQryOut;

            string m_oi_concepto;
            string m_oi_tipo_concepto;
            string m_c_operador;
            string m_oi_personal_Emp;
            string m_c_liq;
            string m_c_c;
            Array arrCampos;
            Hashtable hasXLiq;
            hasXLiq = new Hashtable();

            Hashtable hasXCC;
            hasXCC = new Hashtable();

            m_c_liq = "";
            strOI = ",";
            arrCampos = strOI.Split(',');

            // 	-- por cada cuenta  Obtengo los conceptos(acumulo en dos varables DEBE o Haber)
            // c_operador -- (operacion + o - )

            str_select = "  select  t.oi_concepto ,  t.oi_tipo_concepto,  t.c_operador,  t.oi_personal_Emp  ";
            str_select = str_select + " from liq21_det_int_cont as  t ";
            str_select = str_select + " where  t.c_cuenta = " + @"\'" + m_c_cuenta + @"\' and t.c_d_h = " + @"\'" + m_c_d_h + @"\'";

            strCamposQryOut = "oi_concepto , oi_tipo_concepto, c_operador, oi_personal_Emp  ";
            xmlDocCal = EjecutarQuery(str_select, strCamposQryOut, strCamposQryOut, ref arrCampos);
            mValor = 0;
            mValorAcum = 0;

            try
            {
                foreach (System.Xml.XmlElement xmlCal in xmlDocCal.DocumentElement.ChildNodes)
                {
                    if (xmlCal.Attributes.GetNamedItem("oi_tipo_concepto") == null)
                    {
                        m_oi_concepto = (String)xmlCal.Attributes["oi_concepto"].Value;
                        m_oi_tipo_concepto = "";
                    }
                    else
                    {
                        m_oi_concepto = "";
                        m_oi_tipo_concepto = (String)xmlCal.Attributes["oi_tipo_concepto"].Value;
                    }
                    if (xmlCal.Attributes.GetNamedItem("oi_personal_Emp") == null)
                    {
                        m_oi_personal_Emp = "";
                    }
                    else
                    {
                        m_oi_personal_Emp = (String)xmlCal.Attributes["oi_personal_Emp"].Value;
                    }
                    m_c_operador = (String)xmlCal.Attributes["c_operador"].Value;

                    rta = ObtenerSuma(m_oi_concepto, m_oi_tipo_concepto, m_c_operador, m_oi_personal_Emp);

                    foreach (System.Xml.XmlElement xmlObj in rta.DocumentElement.ChildNodes)
                    {
                        if (xmlObj.Attributes.GetNamedItem("nValor") != null)
                        {
                            mValorAcum = m_c_operador == "+" ? StringUtil.str2dbl(xmlObj.Attributes["nValor"].Value) : -1 * StringUtil.str2dbl(xmlObj.Attributes["nValor"].Value);
                            m_c_liq = xmlObj.Attributes["c_liquidacion"].Value + "*" + xmlObj.Attributes["f_liquidacion"].Value;// cambiar "fecha" NACHO!!!
                            m_c_c = xmlObj.Attributes["c_centro_costo"].Value;

                            if (hasXCC.ContainsKey(m_c_liq))// c_liquidacion
                            {
                                hasXLiq = (Hashtable)hasXCC[m_c_liq];

                                if (hasXLiq.ContainsKey(m_c_c))// Centro de Costo
                                {
                                    mValor = (double)hasXLiq[m_c_c];
                                    hasXLiq[m_c_c] = mValor + mValorAcum; // si existe acumula el valor
                                }
                                else
                                {
                                    hasXLiq.Add(m_c_c, mValorAcum);// si no existe agrega el valor
                                }
                            }
                            else
                            {
                                // si no existe agrega el valor
                                hasXLiq.Add(m_c_c, mValorAcum);// crea la coleccion de Centro de Costo con su Valor correspondiente
                                hasXCC.Add(m_c_liq, hasXLiq);// agrega la colec CC a la liquidacion
                            }


                        }

                    }


                }

                return hasXCC;

            }
            catch (Exception e)
            {
                NomadEnvironment.GetTrace().Error(e.Message);
                return hasXCC;
            }

        }



        //************************************************************************** 
        //                  EJECUCION DE QUERYS DINAMICOS                           
        //************************************************************************** 

        private System.Xml.XmlDocument ObtenerSuma(string m_oi_concepto, string m_oi_tipo_concepto, string m_c_operador, string m_oi_personal_Emp)
        {
            System.Xml.XmlDocument xmlDocCal;
            string strOI;
            string str_select;
            string strCamposQryOut;


            Array arrCampos;
            strOI = ",";
            arrCampos = strOI.Split(',');


            // 	-- por cada cuenta  Obtengo los conceptos(acumulo en dos varables DEBE o Haber)
            // c_operador -- (operacion + o - )

            str_select = " select sum(liq20_CLP.n_valor) as nValor, max(L99_liq.f_valor) as f_valor, Liq97_per.c_centro_costo_ult	as c_centro_costo ,  L99_liq.c_liquidacion as c_liquidacion , max(L99_liq.f_liquidacion) as f_liquidacion ";
            str_select = str_select + "		from liq20_conc_liq_per as liq20_CLP, Liq96_conceptos as Liq96_Con, Liq14_conceptos as Liq14_Con ";
            str_select = str_select + "	, liq99_liquidacion as L99_liq		, liq99_recibos as l99_rcb 		, Liq97_personal_emp as Liq97_per";
            str_select = str_select + "		, liq20_tot_liq_per as Liq20_TLP";
            str_select = str_select + " JOIN liq20_CLP.oi_tot_liq_per  INNER l99_rcb.oi_tot_liq_per  ";
            str_select = str_select + " , Liq96_Con.oi_concepto_ddo INNER liq20_CLP.oi_concepto_ddo ";
            str_select = str_select + " , Liq14_Con.c_concepto INNER Liq96_Con.c_concepto   ";
            str_select = str_select + " , L99_liq.oi_liquidacion_ddo INNER l99_rcb.oi_liquidacion_ddo   ";
            str_select = str_select + " , Liq20_TLP.oi_tot_liq_per   INNER liq20_CLP.oi_tot_liq_per    ";
            str_select = str_select + " , Liq97_per.oi_per_emp_ddo  INNER Liq20_TLP.oi_per_emp_ddo    ";
            str_select = str_select + " , L99_liq.oi_liquidacion_ddo INNER  l99_rcb.oi_liquidacion_ddo  ";
            str_select = str_select + " where     ";
            //str_select = str_select + "  Liq97_per.c_centro_costo_ult  = " + @"\'" + m_c_centro_costo + @"\'"; 
            str_select = str_select + "  L99_liq.c_empresa = " + @"\'" + p_c_empresa + @"\'";
            str_select = str_select + " and L99_liq.e_periodo = " + @"\'" + p_e_periodo + @"\'";
            str_select = str_select + " and isnotnull(L99_liq.f_cierre ) ";
            str_select = str_select + " and L99_liq.c_estado =" + @"\'F\'";
            if (m_oi_concepto == "")
            { str_select = str_select + " and Liq14_Con.oi_tipo_concepto = " + @"\'" + m_oi_tipo_concepto + @"\'"; }
            else
            { str_select = str_select + " and Liq14_Con.oi_concepto = " + @"\'" + m_oi_concepto + @"\'"; }
            // si tiene oipersonal_emp agregar
            if (m_oi_personal_Emp != "")
            {
                str_select = str_select + " and l99_rcb.e_numero_legajo = (select e_numero_legajo from Per02_personal_emp as P, org03_empresas as E ";
                str_select = str_select + " where E.oi_empresa = P.oi_empresa and oi_personal_emp = = " + @"\'" + m_oi_personal_Emp + @"\'";
            }
            str_select = str_select + " group by Liq97_per.c_centro_costo_ult  , L99_liq.c_liquidacion  ";

            strCamposQryOut = " nValor, f_valor, c_centro_costo ,c_liquidacion , f_liquidacion ";
            xmlDocCal = EjecutarQuery(str_select, strCamposQryOut, strCamposQryOut, ref arrCampos);


            return xmlDocCal;
        }

        //**************************************************************************//
        //***************** EJECUCION DE QUERYS DINAMICOS *************************//
        //**************************************************************************//
        private System.Xml.XmlDocument EjecutarQuery(string str_Select, string strCampos, string strCamposQryOut, ref Array arrCampos)
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

            //NomadEnvironment.GetTrace().Info("  Clase: ObtenerCuentas 		Funcion: Comparar"   ) ; 			                                                                                                                                 

            varQuery = @"                                                                                                                                                                                                        
								<qry:main doc=""PARAM"">                                                                                                                                                                                         
										<qry:append-doc name=""FILTRO"" doc-path=""#PARAM:/FILTRO""/>                                                                                                                                                
										<qry:element name=""objects"">                                                                                                                                                                               
											<qry:insert-select doc-path=""#FILTRO:"" name=""filtro_empresa""/>                                                                                                                                         
										</qry:element>                                                                                                                                                                                               
									</qry:main>                                                                                                                                                                                                    
										<qry:select doc=""PARAM"" name=""filtro_empresa"">                                                                                                                                                           
												<qry:xquery>                                                                                                                                                                                             
													for $r in sql('" + str_Select + @"  ')/ROWS/ROW" +
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
                NomadProxy.GetProxy().Batch().Trace.Add("err", e.Message, "EjecutarQuery");
                NomadEnvironment.GetTrace().Error(e.Message);

                xmlDocCal = new System.Xml.XmlDocument();
                return xmlDocCal;
            }

        }
    }
}
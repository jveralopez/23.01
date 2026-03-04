using System;
using System.Collections.Generic;
using System.Text;
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

using System.Data;
using System.Data.SqlClient;
using System.Configuration;

namespace NucleusRH.Base.Indicadores.Ejecuciones
{

    class clsDimensiones
    {
        clsDataBase objDB;
        NomadBatch objBatch;
        NucleusRH.Base.Indicadores.Ejecuciones.EJECUCION objEjec;
        NucleusRH.Base.Indicadores.Ejecuciones.DET_EJE objEjecDet;
        NucleusRH.Base.Indicadores.Ejecuciones.clsTablasFact nmdSP;
        NucleusRH.Base.Indicadores.Ejecuciones.clsRefreshCube nmdRefCube;
        string sCodigoEjecucion;

        public void ConectaNomadDim(string strIndicadores, int parAño, int parMes, string strTablasToUpd)
        {
            string strMessageMail;
            string myConnString;
            NomadXML nmdXml;
            string myConn;
            string myResource;
            string mRefreshCube;
            myConnString = "";
            mRefreshCube = "";
            string strServerOlap;
            strServerOlap = "";
            sCodigoEjecucion = "";
            objBatch = NomadBatch.GetBatch("Actualizacion", "Actualizacion Indicadores ");
            NucleusRH.Base.Indicadores.Ejecuciones.clsServerPath.GetServerPath(ref  myConnString, ref strServerOlap);
            NomadBatch.Trace("  myConnString:" + myConnString);
            objDB = new clsDataBase(myConnString, objBatch);

            //---------------------------------------------
            //----- EJECUTA LA ACTUALIZACION DE DIMENSIONES
            fncUPD_Dim(strIndicadores, parAño, parMes, strTablasToUpd);
            objBatch.SetSubBatch(31, 100);

            //strServerOlap
            //---------------------------------------------
            //----- EJECUTA LA ACTUALIZACION DE TABLAS FACT
            nmdSP = new NucleusRH.Base.Indicadores.Ejecuciones.clsTablasFact();
            nmdSP.ConectaNomadFact(parAño, parMes, strTablasToUpd, sCodigoEjecucion, myConnString);
            mRefreshCube = nmdSP.strMsgMail;

            nmdRefCube = new NucleusRH.Base.Indicadores.Ejecuciones.clsRefreshCube();
            NomadBatch.Trace(" strServerOlap : " + strServerOlap);
            if (nmdRefCube != null)
            {
                string rtaFlush = nmdRefCube.Refresh(strServerOlap);
                NomadBatch.Trace("  rtaFlush:" + rtaFlush);
                if (rtaFlush.IndexOf("Flush Schema Cache") != -1)
                {
                    mRefreshCube = " Cubo Actualizado ";
                    objBatch.Log(mRefreshCube);
                    NomadBatch.Trace("  Cubo Actualizado ");
                }
                else
                {
                    mRefreshCube = " ERROR EN ACTUAIZACION DE CUBO ";
                    objBatch.Log(mRefreshCube);
                    NomadBatch.Trace(" ERROR Actualizacion Cubo  ");
                }
            }
            strMessageMail = nmdSP.strMsgMail + "\n" + mRefreshCube;
            fncSendMail(strMessageMail);

        }
        private void fncUPD_Dim(string strIndicadores, int parAño, int parMes, string strTablasToUpd)
        {
            string strCampos;
            string strCamposOut;
            string rta;
            string sNomad_TBL;
            string sDataMart_TBL;
            string sDM_TBLpk;
            string strWhere;
            int rtaQry;
            Array arr_oi_multiple;
            int CantIns;
            int CantUpd;
            int CantNA;

            // ABRE EL XML que contiene las definiciones de actualizacion de campos Origen/Destino (nomad/datamart)
            //strIndicadores = System.IO.File.ReadAllText("c:\\IndicadoresUpd.xml");
            NomadXML nmdXML_Indic;
            try
            {
                sCodigoEjecucion = "";
                CantIns = 0;
                CantUpd = 0;
                CantNA = 0;

                objEjec = new NucleusRH.Base.Indicadores.Ejecuciones.EJECUCION();
                sCodigoEjecucion = fncGetLatCode();
                objEjec.c_ejecucion = sCodigoEjecucion;
                objEjec.d_ejecucion = "actualizacion de dimensiones " + parAño.ToString() + "/" + parMes.ToString();

                objEjec.c_mes_anio = "TODOS";
                objEjec.c_modulo_tco = "DIMENSIONES";
                objEjec.f_Inicio_ejec = DateTime.Now;
                //objEjec.descr = "actualizacion de dimensiones";
                objEjec.c_estado = "E";
                //objEjec.DETALLE

                strIndicadores = strIndicadores.Replace("\r\n ", "");
                strIndicadores = strIndicadores.Replace("\r\n", "");
                strIndicadores = strIndicadores.Replace(" <", "<");
                string sQuery;
                nmdXML_Indic = new NomadXML(strIndicadores);
                int nProc;
                nProc = 10;
                objBatch.SetPro(nProc);
                if (strTablasToUpd.ToUpper() == "FACT_PROYECTOS") { return; }
                //recorre el xml y actualiza la DB DataMart
                for (NomadXML xmlDIM = nmdXML_Indic.FirstChild().FindElement("Dimension"); xmlDIM != null; xmlDIM = xmlDIM.Next())
                {
                    foreach (NomadXML xmlHierarchy in xmlDIM.GetChilds())
                    {
                        //              NomadXML xmlHierarchy= nmdXMLDim.FindElement("Hierarchy")  ;

                        sNomad_TBL = "";
                        sDM_TBLpk = "";
                        strCampos = "";
                        strCamposOut = "";
                        sDataMart_TBL = "";
                        strWhere = "";

                        sDM_TBLpk = xmlHierarchy.GetAttrString("primaryKey");
                        arr_oi_multiple = sDM_TBLpk.Split(',');

                        if (sDM_TBLpk == "") { continue; }
                        sDataMart_TBL = xmlHierarchy.FindElement("Table").GetAttrString("DataMart");
                        sNomad_TBL = xmlHierarchy.FindElement("Table").GetAttrString("Nomad");
                        strWhere = xmlHierarchy.FindElement("Table").GetAttrString("whereQry");
                        NomadBatch.Trace(" TablaDataMart : " + sDataMart_TBL + " sNomad_TBL : " + sNomad_TBL);

                        nProc += 1;
                        objBatch.SetMess("Actualizando Dimension " + sDataMart_TBL);
                        objBatch.SetPro(nProc);

                        objEjecDet = new NucleusRH.Base.Indicadores.Ejecuciones.DET_EJE();
                        objEjecDet.c_nom_tabla = sDataMart_TBL;
                        //objEjecDet.descr = sDataMart_TBL;
                        objEjecDet.c_estado = "E";

                        CantIns = 0;
                        CantUpd = 0;
                        CantNA = 0;

                        strCamposOut = sDM_TBLpk;
                        // si las tablas estan separadas por comas (mas de una) arma los alias tal como aparecen en el xml
                        if (sNomad_TBL.IndexOf(",") == -1)
                        {

                            for (int ind = 0; ind < arr_oi_multiple.Length; ind++)
                            {
                                strCampos += sNomad_TBL + "." + ((string[])(arr_oi_multiple))[ind] + ",";
                            }
                            strCampos = strCampos.Substring(0, strCampos.Length - 1);
                            foreach (NomadXML xmlColums in xmlHierarchy.FindElement("Columns").GetChilds())
                            {
                                //strCampos = strCampos + "," + sNomad_TBL + "." + xmlColums.GetAttrString("name") + "  as " + xmlColums.GetAttrString("name");
                                strCampos = strCampos + "," + fncAliasField(sNomad_TBL, xmlColums) + "  as " + xmlColums.GetAttrString("name");

                                strCamposOut = strCamposOut + "," + xmlColums.GetAttrString("name");
                            }
                        }
                        else
                        {
                            strCampos = "**";
                            strCamposOut = "**";
                            foreach (NomadXML xmlColums in xmlHierarchy.FindElement("Columns").GetChilds())
                            {
                                strCampos = strCampos + "," + xmlColums.GetAttrString("aliasName") + "  as " + xmlColums.GetAttrString("name");
                                strCamposOut = strCamposOut + "," + xmlColums.GetAttrString("name");
                            }
                            strCampos = strCampos.Replace("**,", " ");
                            strCamposOut = strCamposOut.Replace("**,", " ");

                            strWhere = strWhere.Replace(" =* ", " RIGHT OUTER ");
                            strWhere = strWhere.Replace(" *= ", " LEFT OUTER ");
                            strWhere = strWhere.Replace(" = ", " INNER ");

                            sNomad_TBL = sNomad_TBL + " JOIN " + strWhere;

                        }
                        // busca datos en Nomad y actualiza DataMart
                        rta = NomadQuery(sNomad_TBL, "1=1", strCampos, strCamposOut, sDataMart_TBL, arr_oi_multiple);

                        if (objEjecDet.n_error != 0)
                        {
                            objEjecDet.c_estado = "C";
                            objEjec.c_estado = "C";
                        }
                        else
                        {
                            objEjecDet.c_estado = "S";

                        }
                        objEjec.DETALLE.Add(objEjecDet);

                    }
                }
                objBatch.SetPro(30);
                NomadBatch.Trace(" **** Graba DDO ********");
                if (objEjec.c_estado == "E") { objEjec.c_estado = "S"; }// como no tiene errores pasa a Sin ERRORES
                objEjec.f_fin_ejec = DateTime.Now;

                NomadEnvironment.GetCurrentTransaction().Save(objEjec);
                NomadBatch.Trace(" **** DDO DIMENSIONES GUARDADO  ********");

                objBatch.SetMess("Dimensiones Actualizadas ");
                // elimina las empresas contratistas para volver a insertarlas desde la dimesion de Empresas
                rtaQry = objDB.ExecuteNonQuery(CommandType.Text, " delete FROM TCO_EMPRESAS_VINC where OI_EMPRESA_VINC < 99999 ");

                sQuery = objDB.fncInsDimEmpresas();
                rtaQry = objDB.ExecuteNonQuery(CommandType.Text, sQuery);

                for (NomadXML xmlQRY = nmdXML_Indic.FirstChild().FindElement("Querys"); xmlQRY != null; xmlQRY = xmlQRY.Next())
                {
                    foreach (NomadXML xmlQ in xmlQRY.GetChilds())
                    {

                        sQuery = xmlQ.GetAttrString("sql");
                        rtaQry = objDB.ExecuteNonQuery(CommandType.Text, sQuery);
                        // SI la respuesta es cero implica que no existe el registro
                        if (rtaQry == 0)
                        {

                        }

                    }
                }

            }
            catch (Exception e)
            {
                //Mensaje solo al archivo
                NomadBatch.Trace(" ERROR ConectaNomad " + e.Message);
                NomadBatch.Trace(" ERROR objEjec " + objEjec);
            }
        }

        //*********************************************************
        private void fncSendMail(string strMsgMail)
        {
            try
            {

                Nomad.Base.Mail.Grupos_Mail_Usuario.GRUPOS_MAIL gp_mail;
                gp_mail = Nomad.Base.Mail.Grupos_Mail_Usuario.GRUPOS_MAIL.Get("INDICADORES");//

                gp_mail.EnviaMails("NucleusRH - Actualizacion de Indicadores", strMsgMail);

                Nomad.NSystem.Base.NomadBatch.Trace("********* ENVIO LOS MAILS**************");

            }
            catch (Exception e)
            {
                objBatch.Err(" Error en el envío del correo ");
                NomadBatch.Trace(" Error en fncSendMail " + e.Message);

            }

        }

        //*****************************************************************************************************
        private string fncGetLatCode()
        {//esta funcion obtiene el ultimo oi para concatenarlo y generar un codigo de ejecucion comun a corridas de Dimensiones y Fact Tables
            string rta;
            Array arrCampos;
            System.Xml.XmlDocument xmlDocCal;
            rta = ",";
            arrCampos = rta.Split(',');
            // EjecutarQuery devuelve un xml con el resultado de la consulta y los tipos de datos por cada campo
            xmlDocCal = EjecutarQuery("TCO01_EJECUCIONES", "1=1", "MAX(TCO01_EJECUCIONES.OI_EJECUCION) as OI_EJECUCION", "OI_EJECUCION", ref arrCampos);
            rta = "0";
            foreach (System.Xml.XmlElement xmlCal in xmlDocCal.DocumentElement.FirstChild.ChildNodes)
            {

                // arma la cadena con el o los OI (en caso de multiples) que usara para el WHERE del update

                if (xmlCal.GetAttribute("OI_EJECUCION") != "")
                { rta = xmlCal.GetAttribute("OI_EJECUCION").ToString(); }
            }

            rta = "Ejecucion_" + rta;
            return rta;
        }

        //*****************************************************************************************************
        private string fncAliasField(string sNomad_TBL, NomadXML xmlColums)
        {//esta funcion arma el campo para el select de Nomad, si tiene valor en el alias omite pergarle el nombre de la
            // tabla antes , lo deja como esta
            string rta;
            rta = xmlColums.GetAttrString("aliasName");
            if (rta == "")
            {
                rta = sNomad_TBL + "." + xmlColums.GetAttrString("name");
            }
            return rta;
        }

        //*****************************************************************************************************
        private void fncLogDB(string rta, ref int CantIns, ref int CantUpd, ref int CantNA, string errStr)
        {
            try
            {
                //esta funcion arma elcuenta la cantidad de INSERT INTO Updates o Ninguna Accion de acuerdo a los resultados
                // obtenido de las llamadas a eejcuciones de sql
                if (rta == "INS") { CantIns += 1; return; }
                if (rta == "UPD") { CantUpd += 1; return; }

                NucleusRH.Base.Indicadores.Ejecuciones.ERROR objERR;
                objERR = new NucleusRH.Base.Indicadores.Ejecuciones.ERROR();
                CantNA += 1;
                objERR.c_error = DateTime.Now.ToString() + CantNA.ToString();
                //objERR.descr = rta;
                objERR.l_logUsuario = false;
                objERR.t_error = errStr;
                objEjecDet.ERRORES.Add(objERR);
                objEjecDet.c_estado = "C";
                objEjec.c_estado = "C";

                NomadBatch.Trace(" fncLogDB   logea error" + errStr);

            }

            catch (Exception e)
            {
                //Mensaje solo al archivo
                NomadBatch.Trace(" ERROR fncLogDB " + e.Message);
            }

        }

        //*****************************************************************************************************
        private string NomadQuery(string strTabla, string strWhere, string strCampos, string strCamposAlias, string strTablaDestino, Array arr_Oi)
        {
            System.Xml.XmlDocument xmlDocCal;
            string strCampo;
            string strGetOI;
            string strOI;
            int rtaQry;
            string strCamposQuery;

            string strQry;
            rtaQry = 0;
            strGetOI = "";
            strQry = "";
            strCampo = "";
            Array arrCampos;
            int CantIns;
            int CantUpd;
            int CantNA;

            try
            {
                // se pasa a UPPER por incopatibilidad con ORACLE
                strTabla = strTabla.ToUpper();
                strWhere = strWhere.ToUpper();
                strCampos = strCampos.ToUpper();
                strCamposAlias = strCamposAlias.ToUpper();
                strTablaDestino = strTablaDestino.ToUpper();
                string strUpper;
                strUpper = "";
                strUpper = string.Join(":", (string[])(arr_Oi));
                strUpper = strUpper.ToUpper();
                arr_Oi = strUpper.Split(':');
                // se pasa a UPPER por incopatibilidad con ORACLE

                arrCampos = strCamposAlias.Split(',');
                // EjecutarQuery devuelve un xml con el resultado de la consulta y los tipos de datos por cada campo
                xmlDocCal = EjecutarQuery(strTabla, strWhere, strCampos, strCamposAlias, ref arrCampos);

                if (xmlDocCal == null) { return ""; }
                strOI = "";
                strCamposQuery = "";
                NomadXML nmdXML;

                nmdXML = new NomadXML(xmlDocCal.DocumentElement.InnerXml);
                NomadXML nmdXMLCol = nmdXML.FirstChild().FindElement("COLUMNS");

                Hashtable hashColumnsValue;
                Hashtable hashColumns;
                string OrderParams;
                OrderParams = "";

                hashColumns = new Hashtable();
                hashColumnsValue = new Hashtable();

                CantIns = 0;
                CantUpd = 0;
                CantNA = 0;

                // Guarda en Hash el tipo de dato de cada columna
                foreach (NomadXML xmlCal in nmdXMLCol.GetChilds())
                {
                    hashColumns.Add(xmlCal.GetAttrString("Name"), xmlCal.GetAttrString("type"));
                }

                foreach (System.Xml.XmlElement xmlCal in xmlDocCal.DocumentElement.FirstChild.ChildNodes)
                {
                    OrderParams = "";
                    hashColumnsValue = new Hashtable();

                    strOI = "";//contiene el nombre del OI
                    strCamposQuery = "";

                    // arma la cadena con el o los OI (en caso de multiples) que usara para el WHERE del update
                    for (int item_oi = 0; item_oi < arr_Oi.Length; item_oi++)
                    {
                        strGetOI = ((string[])(arr_Oi))[item_oi]; // ejemplo oi = Valor

                        if (xmlCal.GetAttribute(strGetOI.Trim()) == "")
                        { continue; }
                        strOI += strGetOI + " = ?" + ",";
                        hashColumnsValue.Add(strGetOI, xmlCal.GetAttribute(strGetOI.Trim()));
                    }

                    if (strOI == "")
                    { continue; }
                    strOI = strOI.Substring(0, strOI.Length - 1);
                    // arma el string para el INSERT INTO o UPDATE
                    for (int x = arr_Oi.Length; x < arrCampos.Length; x++)
                    {
                        strCampo = ((string[])(arrCampos))[x];
                        //strCamposQuery = strCamposQuery + "," + strCampo + "=" + fncConverSQL(hashColumns[strCampo.Trim()].ToString(), xmlCal.GetAttribute(strCampo.Trim()));
                        strCamposQuery = strCamposQuery + "," + strCampo + "=?";
                        hashColumnsValue.Add(strCampo, fncConverSQL(hashColumns[strCampo.Trim()].ToString(), xmlCal.GetAttribute(strCampo.Trim())));
                    }
                    if (strCamposQuery.Length > 1)
                    {
                        if (strCamposQuery.Substring(0, 1) == ",")
                        { strCamposQuery = strCamposQuery.Substring(1, strCamposQuery.Length - 1); }
                    }
                    OrderParams = strCamposQuery + strOI;
                    OrderParams = OrderParams.Replace("=", "");
                    OrderParams = OrderParams.Replace(",", "");
                    Array orderPar = OrderParams.Split('?');

                    strQry = " UPDATE " + strTablaDestino;
                    strQry += " SET " + strCamposQuery;
                    strQry += " WHERE " + strOI.Replace(",", " and ");

                    rtaQry = objDB.ExecuteNonQuery(CommandType.Text, strQry, hashColumns, hashColumnsValue, orderPar);

                    // SI la respuesta es cero implica que no existe el registro

                    OrderParams = strOI + strCamposQuery;
                    OrderParams = OrderParams.Replace("=", "");
                    OrderParams = OrderParams.Replace(",", "");
                    orderPar = OrderParams.Split('?');
                    string Signo;
                    if (rtaQry == 0)
                    {

                        Signo = "";
                        for (int aa = 0; aa < orderPar.Length - 1; aa++)
                        {
                            Signo += "?,";

                        }
                        Signo += "*";
                        Signo = Signo.Replace(",*", "");
                        strQry = " INSERT INTO " + strTablaDestino;
                        strQry += " (  " + fncVerifyOI(strGetOI, strCamposAlias) + ")";
                        //strQry += " VALUES ( " + fncVerifyOI(strOI, strCamposQuery) + ")";
                        strQry += " VALUES ( " + Signo + ")";
                        rtaQry = objDB.ExecuteNonQuery(CommandType.Text, strQry, hashColumns, hashColumnsValue, orderPar);

                        if (rtaQry != 0)
                        { strOI = "INS"; }
                    }
                    else
                    {
                        strOI = "UPD";
                    }
                    // ocurrrio un error de ejecucion a nivel de clase de Base de Datos
                    if (rtaQry == -1)
                    {
                        strOI = "ERR";
                        strQry = objDB.sMsgError;
                    }
                    fncLogDB(strOI, ref CantIns, ref CantUpd, ref CantNA, strQry);

                }
                objEjecDet.n_procesados = CantIns + CantUpd + CantNA;
                objEjecDet.n_error = CantNA;
                return strOI;
            }
            catch (Exception e)
            {
                int aa;
                aa = 0;
                //Mensaje solo al archivo
                fncLogDB("Error de proceso ", ref aa, ref aa, ref aa, " ERROR NomadQuery " + e.Message);
                NomadBatch.Trace(" ERROR NomadQuery " + e.Message);
                return "";
            }
        }

        // Esta funcion arma el strind del query tanto para los Select de Tablas combinadas como para los de tablas simples
        private string fncVerifyOI(string strOI, string strTextQry)
        {
            string rta;
            rta = "";
            int nPosicion = strTextQry.IndexOf(strOI, 0);
            if (nPosicion == -1)
            {
                rta = strOI + " , " + strTextQry;
            }
            else
            {
                rta = strTextQry;
            }

            return rta;
        }

        // si los valores son texto les agrega "'" (o en casos nulos la palabra NULL)
        private string fncConverSQL(string strType, string strTextQry)
        {
            string rta;

            try
            {
                rta = "";

                if (strType == "DATE")
                {

                    if (strTextQry == "")
                    { //rta = "#01/01/1900#";
                        rta = "19000101";
                    }
                    else
                    {
                        //rta = "#" + strTextQry.Substring(4, 2) + "/" + strTextQry.Substring(6, 2) + "/" + strTextQry.Substring(0, 4) + "#";
                        rta = strTextQry;
                    }
                    return rta;
                }

                if (strType == "INT")
                {
                    rta = "" + strTextQry;
                    if (strTextQry == "") { rta = "0"; }
                }
                else
                {
                    // rta = "" + strTextQry + "";
                    rta = "" + strTextQry + "";
                }
                if (rta == "")
                {
                    rta = "NULL";

                }

                if (strType == "FLOAT")  //STR
                {
                    rta = "" + strTextQry;
                    if (strTextQry == "") { rta = "0"; }
                }

                return rta;

            }
            catch (Exception e)
            {
                NomadBatch.Trace(" Error de conversion de datos");
                NomadBatch.Trace("strType :'" + strType + "' strTextQry :'" + strTextQry + "'");
                return "";
            }
        }

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

            //Nomad.NSystem.Base.fncLogearHas( "...  Clase: NomadQuery     Funcion: Comparar"   ) ;

            varQuery = @"
                <qry:main doc=""PARAM"">
                    <qry:append-doc name=""FILTRO"" doc-path=""#PARAM:/FILTRO""/>
                    <qry:element name=""objects"">
                      <qry:insert-select doc-path=""#FILTRO:"" name=""filtro_empresa""/>
                    </qry:element>
                  </qry:main>
                    <qry:select doc=""PARAM"" name=""filtro_empresa"">
                        <qry:xquery>
                          for $r in sql ('  SELECT   " + strCampos + @"  FROM "
                                        + strTabla + @" WHERE " + strWhere + @"  ')/ROWS" +
                                    @"</qry:xquery>
                        <qry:out>

                          <qry:insert-element doc-path=""$r""/>
                        </qry:out>
                      </qry:select>  ";

            //Nomad.NSystem.Base.fncLogearHas( "...Funcion:  BuscaEspGuardada  varQuery " + varQuery) ;
            varQueryParam = @"<FILTRO />";
            try
            {
                xmlDocCal = NomadEnvironment.QueryXML(varQuery.ToString(), varQueryParam);
                return xmlDocCal;
            }

            catch (Exception e)
            {
                NomadBatch.Trace(" ERROR EjecutarQuery " + e.Message);
                NomadBatch.Trace(" ERROR EjecutarQuery " + varQuery.ToString());

                return null;
            }

        }

    }
}



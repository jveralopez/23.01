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


using System.Configuration;

using NucleusRH.Base.AnalisisRemun.InterfaceOut_LegAnalisis;
namespace NucleusRH.Base.AnalisisRemun.InterfacesOut
{

    class clsImportarPeriodos
    {   NomadBatch objBatch;
        string slegajo;
        public void fncEjecutarIntOut (string par_empresa , string par_Periodo, Nomad.NSystem.Proxy.NomadXML nmd_liq)
        {
          string strCampos  ;
          string strTbl;
          string strWHERE;
          NomadXML xmlTMP;
          bool InsertInHas;
          Hashtable hashColumns;
          SALIDA objSalida;
          try
          {

             Nomad.NSystem.Proxy.NomadXML nmdXML;

 //           Array arrLiquidaciones;
//            arrLiquidaciones = par_liquidaciones.Split(',');

//            for(int nitem=0 ; nitem < arrLiquidaciones.Length ; nitem++ )

              Nomad.NSystem.Proxy.NomadXML nmd_ROWSLiq;
              nmd_ROWSLiq=  nmd_liq.FirstChild().FindElement("ROWS")  ;
           for (Nomad.NSystem.Proxy.NomadXML xmlLiq  = nmd_liq.FirstChild().FindElement("ROW"); xmlLiq != null; xmlLiq = xmlLiq.Next())
                {
                 //Guarda en Hash el tipo de dato de cada columna
                           hashColumns= new Hashtable() ;
                strCampos  = "   Liq97_Personal_Emp.e_numero_legajo  as e_numero_legajo " ;
                strCampos += " , liq19_liquidacion.e_periodo  as e_periodo     " ;
                strCampos += " , Liq20_TOT_Liq_Per.n_sbasico as n_basico  " ;
                strCampos += " , Liq20_TOT_Liq_Per.n_sconformado as n_conformado " ;
                strCampos += " ,   0 as n_adicionales " ;
                strCampos += " , Liq20_TOT_Liq_Per.n_totsret as n_sujeto " ;
                strCampos += " , Liq20_TOT_Liq_Per.n_totnsret as    n_nosujeto " ;
                strCampos += " , Liq20_TOT_Liq_Per.n_totsret + Liq20_TOT_Liq_Per.n_totnsret  as n_bruto " ;
                strCampos += " , Liq20_TOT_Liq_Per.n_totdesc as n_descuento " ;
                strCampos += " , Liq20_TOT_Liq_Per.n_totsret + Liq20_TOT_Liq_Per.n_totnsret +  Liq20_TOT_Liq_Per.n_totdesc as n_neto " ;
                strCampos += " , Liq20_TOT_Liq_Per.n_totcont as n_contribuciones " ;
                strTbl = "  liq99_liquidacion , liq99_recibos  , Liq20_TOT_Liq_Per , Liq97_Personal_Emp, " ;
                strTbl += "  Liq19_ejecuciones , liq19_liquidacion " ;
                strTbl += " JOIN liq99_liquidacion.oi_liquidacion_ddo    INNER     liq99_recibos.oi_liquidacion_ddo , " ;
                strTbl += " liq99_recibos.oi_tot_liq_per    INNER   Liq20_TOT_Liq_Per.oi_tot_liq_per ,  " ;
                strTbl += " Liq97_Personal_Emp.oi_per_emp_ddo    INNER     Liq20_TOT_Liq_Per.oi_per_emp_ddo ,  " ;
                strTbl += " liq99_liquidacion.oi_liquidacion_ddo    INNER     Liq19_ejecuciones.oi_liquidacion_ddo ,  " ;
                strTbl += " liq19_liquidacion.oi_liquidacion     INNER     Liq19_ejecuciones.oi_liquidacion  " ;
                strWHERE = "     liq99_liquidacion.c_estado=\\'F\\'     ";
                strWHERE += " AND liq19_liquidacion.oi_liquidacion =" + xmlLiq.GetAttrString("id") ;
                strWHERE += " AND liq19_liquidacion.oi_empresa =" + par_empresa ;
                strWHERE += " AND liq19_liquidacion.e_periodo =" + par_Periodo ;
                xmlTMP = NomadQuery(strTbl, strWHERE, strCampos, "", "", ref hashColumns);

                fncCargaDatos(xmlTMP);
            }
            //InsertInHas = GenHashToDB(xmlTMP, strCampos, hashColumns, strWhere);
            //return true;
        }
        catch (Exception e)
        {

            //objBatch.Err(" Error en GenHashToDb : ");
            //fncLogearHas(" Error en GenHashToDb : " + e.Message);

            //return false;
        }

}

        private void fncCargaDatos(NomadXML xmlTMP)
        {
            SALIDA objSalida;

            if (xmlTMP == null) { return; }

            NomadXML nmdRta = xmlTMP.FirstChild().FindElement("ROW");
            if (nmdRta == null) { return; }

            for (NomadXML nmdXML = xmlTMP.FirstChild().FindElement("ROW"); nmdXML != null; nmdXML = nmdXML.Next())
            {
                if (nmdXML.Name !="ROW") { continue; }
                Nomad.NSystem.Base.NomadBatch.Trace("nmdXML: " + nmdXML);

                objSalida = new SALIDA();
                objSalida.e_periodo =  nmdXML.GetAttrInt( "e_periodo");
                objSalida.n_adicionales =   nmdXML.GetAttrDouble ("n_adicionales");
                objSalida.n_basico =nmdXML.GetAttrDouble("n_basico");
                objSalida.n_bruto =nmdXML.GetAttrDouble("n_bruto");
                objSalida.n_conformado =nmdXML.GetAttrDouble("n_conformado");
                objSalida.n_contribuciones =nmdXML.GetAttrDouble("n_contribuciones");
                objSalida.n_descuento =nmdXML.GetAttrDouble("n_descuento");
                objSalida.n_legajo = nmdXML.GetAttrInt("e_numero_legajo");
                objSalida.n_neto =nmdXML.GetAttrDouble("n_neto");
                objSalida.n_nosujeto =nmdXML.GetAttrDouble("n_nosujeto");
                objSalida.n_sujeto = nmdXML.GetAttrDouble("n_sujeto");

                try
                {
                    NomadEnvironment.GetCurrentTransaction().Save(objSalida);
                }
                catch (Exception e)
                {
                    NomadEnvironment.GetTrace().Error(e.Message);

                }

            }
}
    //*****************************************************************************************************
        private NomadXML NomadQuery(string strTabla, string strWhere, string strCampos, string strCamposAlias, string strTablaDestino, ref Hashtable hashColumns)
        {
            System.Xml.XmlDocument xmlDocCal;

            strTabla = strTabla.ToUpper();
            strWhere = strWhere.ToUpper();
            strCampos = strCampos.ToUpper();
            strCamposAlias = strCamposAlias.ToUpper();
            strTablaDestino = strTablaDestino.ToUpper();

            Array arrCampos;
            //fncUpdEstructuras();
            arrCampos = strCamposAlias.Split(',');

            // EjecutarQuery devuelve un xml con el resultado de la consulta y los tipos de datos por cada campo
            xmlDocCal = EjecutarQuery(strTabla, strWhere, strCampos, strCamposAlias, ref arrCampos);

            if (xmlDocCal == null) { return null; }

            NomadXML nmdXML;

            nmdXML = new NomadXML(xmlDocCal.DocumentElement.InnerXml);
            NomadXML nmdXMLCol = nmdXML.FirstChild().FindElement("COLUMNS");

            hashColumns.Clear();

            // Guarda en Hash el tipo de dato de cada columna
            foreach (NomadXML xmlCal in nmdXMLCol.GetChilds())
            {
                hashColumns.Add(xmlCal.GetAttrString("Name"), xmlCal.GetAttrString("type"));

            }
            NomadXML nmdXMLOut;

            nmdXMLOut = new NomadXML(xmlDocCal.DocumentElement.InnerXml);
            return nmdXMLOut;

        }
        //************************************************************************************//

        //-----------------------------------------------------------------------------------------------
        //-----------------------------------------------------------------------------------------------

        private System.Xml.XmlDocument EjecutarQuery(string strTabla, string strWhere, string strCampos, string strCamposQryOut, ref Array arrCampos)
        {
            string varQuery;
            string varQueryParam;
            string sCamposOut;

            // para compatibilidad con ORACLE se paso todo a UPPER
            strWhere = strWhere.Replace("&GT", "&gt");
            strWhere = strWhere.Replace("&LT", "&lt");

            arrCampos = strCampos.Split(',');
            arrCampos = strCamposQryOut.Split(',');

            System.Xml.XmlDocument xmlDocCal;

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

            varQueryParam = @"<FILTRO />";
            try
            {
                //strMsgErrorUser = sLegajoProc + "Error de consulta en NucleusRH ";
                xmlDocCal = NomadEnvironment.QueryXML(varQuery.ToString(), varQueryParam);
                return xmlDocCal;
            }

            catch (Exception e)
            {
                //fncLogearHas(" ERROR EjecutarQuery " + e.Message + "  " + varQuery.ToString());

                return null;
            }

        }

  }

}



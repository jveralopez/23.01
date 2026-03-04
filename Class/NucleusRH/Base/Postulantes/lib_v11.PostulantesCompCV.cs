
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

namespace NucleusRH.Base.Postulantes.CV
{

    public class clsComparaCV
    {

        public string m_strWeb;
        public string m_strInt;
        public string m_xmlResult;

        public clsComparaCV()
        {

        }

        public clsComparaCV(string pstrWeb, string pstrInt)
        {

            m_strWeb = pstrWeb;
            m_strInt = pstrInt;
            Comparar();
            NomadEnvironment.GetTrace().Info("  Clase: clsComparaCV     SALIO");

        }

        public string xmlResult { get { return m_xmlResult; } }

        public void Comparar()
        {
            NomadEnvironment.GetTrace().Info("  Clase: clsComparaCV     Funcion: Comparar");

            string strResut;
            System.Xml.XmlDocument xmlRta;

            xmlRta = Nomad.NSystem.Functions.StringUtil.xmlDiff(m_strWeb, m_strInt);
            strResut = xmlRta.DocumentElement.OuterXml;

            //          strEncab=@"<xmldiff xmlns:new=""new"" xmlns:old=""old"" xmlns:del=""del"" xmlns:ins=""ins"" xmlns:diff=""diff"">";

            //          strResutlXml= (string) BuscaHijos (xmlRta.DocumentElement);
            //          m_xmlResult = strResutlXml.Replace("<xmldiff>",strEncab);
            NomadEnvironment.GetTrace().Info("  Clase: clsComparaCV     Funcion: Comparar" + m_xmlResult);
            m_xmlResult = strResut;

        }

        public string BuscaHijos(System.Xml.XmlNode objNodeChild)
        {
            string str;
            string strResutlXml;
            string strSelementXml;
            string strResutlXmlChild;

            strResutlXml = "";
            strResutlXmlChild = "";
            strSelementXml = "";
            for (int x = 0; x < objNodeChild.Attributes.Count; x++)
            {
                if ((objNodeChild.Attributes[x].NamespaceURI == "new") || (objNodeChild.Attributes[x].NamespaceURI == "old"))
                {
                    strResutlXml = strResutlXml + "  " + objNodeChild.Attributes[x].Name + "=\"" + objNodeChild.Attributes[x].Value + "\"";
                }
            }
            str = objNodeChild.Name;
            //////// BUSCA NUEVOS ELEMENTOS
            //     if (objNodeChild.NamespaceURI=="new" )
            //      {
            strSelementXml = "<" + objNodeChild.Name;
            //      }

            foreach (System.Xml.XmlNode objNode in objNodeChild.ChildNodes)
            {
                strResutlXmlChild = strResutlXmlChild + BuscaHijos(objNode);
            }

            //     if (objNodeChild.NamespaceURI=="new" ) {
            //       strSelementXml = "< new:" +  objNodeChild.Name + ">" ;
            //     } else {
            if ((strResutlXml != "") || (strResutlXmlChild != ""))
            {
                strResutlXml = strSelementXml + strResutlXml + ">" + strResutlXmlChild + "</" + objNodeChild.Name + ">";
            }
            //     }

            return strResutlXml;

        }

    }
    // ================================================================
    // ================================================================
    //                   oCLASE DE BUSQUEDAS
    // ================================================================
    // ================================================================
    public class clsBuscarCV
    {
        string varQuery;
        string strNameAnt;
        int nTieneUnOR;
        string strResultWHERE;
        string m_c_busqueda;
        string m_d_nombre_busq;
        System.Xml.XmlDocument m_objXmlResult;

        public clsBuscarCV()
        {

        }

        public clsBuscarCV(System.Xml.XmlDocument xmlDoc)
        {
            NomadEnvironment.GetTrace().Info(" **** CLASE clsBuscarCV parametro entrada *******" + xmlDoc.OuterXml);

            strNameAnt = "";
            strResultWHERE = "";
            nTieneUnOR = 0;
            strResultWHERE = BuscaHijos(xmlDoc.DocumentElement);

            strResultWHERE = strResultWHERE.Replace("pos01_cv.oi_conocimiento", "pos01_conoc_cv.oi_conocimiento");
            strResultWHERE = strResultWHERE.Replace("pos01_cv.oi_estudio", "pos01_estudios_cv.oi_estudio_cv");
            if (strResultWHERE.Length > 8) /// si esta lleno es porque falta cerrar el OR final
            {
                strResultWHERE = " ( 1 = 1 )  " + strResultWHERE;
            }

            if (strNameAnt != "") /// si esta lleno es porque falta cerrar el OR final
            {
                strResultWHERE = strResultWHERE + " ) ";
            }

            int nCantEncontrados;
            nCantEncontrados = busCantRegistros();
            NomadEnvironment.GetTrace().Info(" **** nCantEncontrados *******" + nCantEncontrados.ToString());
            if (nCantEncontrados > 500)
                throw new NomadAppException("La busqueda trae mas de 500 registros, debe acotar más la selección"); ;

            m_objXmlResult = obtieneRegistros();

        }
        public string ResultWHERE { get { return strResultWHERE; } }
        public System.Xml.XmlDocument XmlResultBusq { get { return m_objXmlResult; } }

        public string c_busqueda { get { return m_c_busqueda; } }
        public string d_nombre_busq { get { return m_d_nombre_busq; } }

        // ================================================================
        // ================================================================

        public string BuscaHijos(System.Xml.XmlNode objNodeChild)
        {
            string strResutlXmlChild;
            int nPos1;
            string strWhere;
            string strName;

            strResutlXmlChild = "";

            nPos1 = 0;
            strWhere = "";

            System.Xml.XmlAttribute objAttr;
            strName = "";
            for (int x = 0; x < objNodeChild.Attributes.Count; x++)
            {
                objAttr = objNodeChild.Attributes[x];

                if (ValAtrribDef(objAttr.Name, objAttr.Value, ref strResutlXmlChild) == 0)
                { continue; }

                if (objAttr.Value != "")
                {
                    strName = objAttr.Name;
                    if (strName == "value") ///  toma el nombre del nodo
                    { strName = objNodeChild.Name; }

                    nPos1 = strName.IndexOf("_", 0); /// busca el tipo de campo
                    if (nPos1 < 1)
                    { continue; } /// sale, es otro atributo

                    strWhere = ArmaIgualdad(objAttr, strName, nPos1);
                    strResutlXmlChild = strResutlXmlChild + "  " + strWhere;
                }
            }

            foreach (System.Xml.XmlNode objNode in objNodeChild.ChildNodes)
            {
                strResutlXmlChild = strResutlXmlChild + BuscaHijos(objNode);
            }
            return strResutlXmlChild;
        }

        // ================================================================
        // ================================================================
        int ValAtrribDef(string sName, string sVale, ref string strResWhere)
        {

            if ((sVale == "SE") || (sVale == ""))
            {
                return 0;
            }
            switch (sName)
            {
                case "c_busqueda":
                    m_c_busqueda = sVale;
                    break;
                case "d_nombre_busq":
                    m_d_nombre_busq = sVale;
                    break;
                case "f_fechora_busq":
                    break;
                case "oi_busqueda":
                    break;
                case "l_marcado":
                    break;
                case "c_tiene_foto":
                    if (sVale == "S")
                    { strResWhere = strResWhere + " AND  oi_foto is not null "; }
                    if (sVale == "N")
                    { strResWhere = strResWhere + " AND  oi_foto is  null "; }
                    break;

                case "c_cant_hijos":
                    if (sVale == "S")
                    { strResWhere = strResWhere + " AND  e_cant_hijos > 0 "; }
                    if (sVale == "N")
                    { strResWhere = strResWhere + " AND  e_cant_hijos = 0"; }
                    break;

                case "c_postulo":
                    if (sVale == "SI")
                    { strResWhere = strResWhere + " AND  pos01_cv.oi_cv  in (select oi_cv oi from pos01_postu_cv) "; }
                    if (sVale == "NO")
                    { strResWhere = strResWhere + " AND  pos01_cv.oi_cv not in (select oi_cv oi from pos01_postu_cv) "; }
                    break;

                case "c_entrevistado":
                    if (sVale == "SI")
                    { strResWhere = strResWhere + " AND pos01_cv.oi_cv in (select oi_cv oi from pos01_entrevistas) "; }
                    if (sVale == "NO")
                    { strResWhere = strResWhere + " AND pos01_cv.oi_cv not in (select oi_cv oi from pos01_entrevistas)"; }
                    break;

                case "c_Libreta":
                    if (sVale == "SI")
                    { strResWhere = strResWhere + " AND pos01_cv.oi_cv in (select oi_cv oi from pos01_lib_sanit) "; }
                    if (sVale == "NO")
                    { strResWhere = strResWhere + " AND pos01_cv.oi_cv not in (select oi_cv oi from pos01_lib_sanit)"; }
                    break;

                case "c_ingresado_desde":
                    if (sVale == "WEB")
                    { strResWhere = strResWhere + " AND  l_alta_web = 1 "; }
                    if (sVale == "INT")
                    { strResWhere = strResWhere + " AND  l_alta_web  = 0 "; }
                    break;
                case "c_cabello_largo":
                    if (sVale == "SI")
                    { strResWhere = strResWhere + " AND  l_cabello_largo = 1  "; }
                    if (sVale == "NO")
                    { strResWhere = strResWhere + " AND  l_cabello_largo  = 0 "; }
                    break;
                case "c_lic_conducir":
                    if (sVale == "1")
                    { strResWhere = strResWhere + " AND  l_licencia = 1  "; }
                    if (sVale == "0")
                    { strResWhere = strResWhere + " AND  l_licencia  = 0 "; }
                    break;

                default:
                    return 1;
            }
            return 0;

        }
        /////////////////////////////////////////////////////////////////
        // ================================================================
        // ================================================================
        string ArmaIgualdad(System.Xml.XmlAttribute objAtributo, string strName, int nPos1)
        {
            string sValorRetorno;
            sValorRetorno = "";

            int nPos2;

            nPos2 = 0;

            nPos2 = strName.IndexOf("_", (strName.Length - 3));
            if (nPos2 > 1)
            { strName = strName.Substring(0, nPos2); } /// filtra el nombre del campo quitando los _1 _2..

            switch (strName.Substring(0, nPos1))
            {
                case "oi":
                    sValorRetorno = " AND  pos01_cv." + strName + " = " + objAtributo.Value;
                    break;
                case "n":
                    sValorRetorno = " AND  pos01_cv." + strName + " = " + objAtributo.Value;
                    break;
                case "e":
                    sValorRetorno = " AND  pos01_cv." + strName + " = " + objAtributo.Value;
                    break;
                case "f":
                    sValorRetorno = @" AND  pos01_cv." + strName + @" = \'" + objAtributo.Value.ToString().Substring(0, 8) + @"\' ";
                    break;

                default:
                    sValorRetorno = @" AND  pos01_cv." + strName + @" = \'" + objAtributo.Value + @"\' ";
                    break;
            }
            //NomadEnvironment.GetTrace().Info(" Funcion :  sValorRetorno " + sValorRetorno);

            sValorRetorno = sValorRetorno.Replace("_desde = ", "  &gt;= ");
            sValorRetorno = sValorRetorno.Replace("_hasta = ", "  &lt;= ");
            sValorRetorno = sValorRetorno.Replace("n_min_remun = ", "n_min_remun >= ");
            sValorRetorno = sValorRetorno.Replace("n_ideal_remun = ", "n_ideal_remun >= ");
            sValorRetorno = sValorRetorno.Replace("n_actual_remun = ", "n_actual_remun >= ");
            sValorRetorno = CierraCondOR(nPos2, strName, sValorRetorno);

            //NomadEnvironment.GetTrace().Info(" Funcion :  ArmaIgualdad " + sValorRetorno);

            return sValorRetorno;
        }

        //////////////////////////////////////////////////////////////////
        ////              CierraCondOR             ///////////////////////
        ////////////////////////////////////////////////////////////////
        string CierraCondOR(int nPos, string strName, string strWhere)
        {
            string strResutl;
            strResutl = "";

            if (nPos > 0)
            {  /// hay un atributo que tiene varios campos con el mismo nombre
                if (strNameAnt == strName)
                {
                    strWhere = strWhere.Replace(" AND ", " OR ");
                    strNameAnt = strName;
                }
                else/// el atributo diferente al que encontro antes, debe reiniciarlo
                {
                    strNameAnt = "";
                }

                if (strNameAnt == "")/// se guarda el nombre para compararlo con el siguiente
                {
                    strNameAnt = strName;
                    if (nTieneUnOR == 0)
                    {/// abre el paréntesis del OR y cerro el que quedo abierto
                        strWhere = strWhere.Replace(" AND ", " AND ( ");
                    }
                    else
                    {/// abre el paréntesis del OR y cerro el que quedo abierto
                        strWhere = strWhere.Replace(" AND ", ") AND ( ");
                    }
                }
                nTieneUnOR = nPos;
            }
            else
            {
                if (strNameAnt != "") /// si esta lleno es porque falta cerrar el OR de los campos con mas de uno
                {
                    strWhere = strWhere.Replace(" AND ", " ) AND "); //cerro el que quedo abierto
                }
                strNameAnt = "";
            }
            strResutl = "  " + strWhere;
            return strResutl;
        }

        //////////////////////////////////////////////////////////////////
        ////              CierraCondOR             ///////////////////////
        ////////////////////////////////////////////////////////////////

        int busCantRegistros()
        {
            System.Xml.XmlDocument xmlDoc;
            int mRta;
            string varQuery2;
            string varQueryParam;
            string varQuerySele;
            string varQueryWhereIn;

            if (strResultWHERE == "") { strResultWHERE = " 1=1 "; }

            mRta = 0;
            NomadEnvironment.GetTrace().Info(" Funcion clsBuscarCV:  busCantRegistros ENTRO ");

            varQuerySele = "";
            varQuery2 = "";
            varQueryWhereIn = "";
            varQuery = "";
            varQueryParam = "";

            varQuerySele = @" select     COUNT(pos01_cv.l_alta_web) as CantReg    from pos01_cv as pos01_cv   ";
            varQuery2 = varQuerySele + @" where    " + strResultWHERE;
            varQuery2 = varQuery2 + @" and pos01_cv.l_alta_web=0 AND ";
            varQueryWhereIn = @"  pos01_cv.oi_cv in
  (
  select     pos01_conoc_cv.oi_cv as oi_cv2     from pos01_cv as pos01_cv ,  pos01_conoc_cv     as pos01_conoc_cv       where pos01_conoc_cv.oi_cv =pos01_cv.oi_cv
  ) or pos01_cv.oi_cv in (
  select     pos01_cursos_cv.oi_cv as oi_cv2   from pos01_cv as pos01_cv ,  pos01_cursos_cv     as pos01_cursos_cv     where pos01_cursos_cv.oi_cv =pos01_cv.oi_cv
  ) or pos01_cv.oi_cv in (
  select     pos01_compet_cv.oi_cv as oi_cv2   from pos01_cv as pos01_cv ,  pos01_compet_cv     as pos01_compet_cv     where pos01_compet_cv.oi_cv =pos01_cv.oi_cv
  ) or pos01_cv.oi_cv in (
  select     pos01_idiomas_cv.oi_cv as oi_cv2   from pos01_cv as pos01_cv ,  pos01_idiomas_cv   as pos01_idiomas_cv     where pos01_idiomas_cv.oi_cv =pos01_cv.oi_cv
  ) or pos01_cv.oi_cv in (
  select     pos01_estudios_cv.oi_cv as oi_cv2 from pos01_cv as pos01_cv ,  pos01_estudios_cv  as pos01_estudios_cv    where pos01_estudios_cv.oi_cv =pos01_cv.oi_cv
  ) ";
            varQuery2 = varQuery2 + varQueryWhereIn + @" UNION ";

            varQuery2 = varQuery2 + varQuerySele;
            varQuery2 = varQuery2 + @" where  pos01_cv.l_alta_web=1 AND  " + strResultWHERE;

            varQuery2 = varQuery2 + @" AND " + varQueryWhereIn + @" and pos01_cv.c_nro_doc not in
          (   select    pos01_cv.c_nro_doc  as c_nro_doc from pos01_cv as pos01_cv
              WHERE  pos01_cv.l_alta_web=0 and    ";
            varQuery2 = varQuery2 +varQueryWhereIn + @" ) AND " + strResultWHERE;

//            varQueryRep =      @" from pos01_cv as pos01_cv
//                                  , pos01_conoc_cv  as pos01_conoc_cv
//                                  , pos01_cursos_cv   as pos01_cursos_cv
//                                  , pos01_compet_cv  as  pos01_compet_cv
//                                  , pos01_idiomas_cv  as pos01_idiomas_cv
//                                  , pos01_estudios_cv  as pos01_estudios_cv
//                                  JOIN  pos01_conoc_cv.oi_cv LEFT OUTER pos01_cv.oi_cv
//                                    ,  pos01_cursos_cv.oi_cv LEFT OUTER pos01_cv.oi_cv
//                                    ,  pos01_compet_cv.oi_cv LEFT OUTER pos01_cv.oi_cv
//                                    ,  pos01_idiomas_cv.oi_cv LEFT OUTER pos01_cv.oi_cv
//                                    ,  pos01_estudios_cv.oi_cv LEFT OUTER pos01_cv.oi_cv  " ;

            varQuery = @"
                <qry:main doc=""PARAM"">
                    <qry:append-doc name=""FILTRO"" doc-path=""#PARAM:/FILTRO""/>
                    <qry:element name=""objects"">
                      <qry:insert-select doc-path=""#FILTRO:"" name=""filtro_empresa""/>
                    </qry:element>
                  </qry:main>

                  <qry:select doc=""PARAM"" name=""filtro_empresa"">
                        <qry:xquery>
                          for $r in sql('";
//            varQuery2 = @"select   COUNT(pos01_cv.l_alta_web) as CantReg " + varQueryRep + "  WHERE   "
//                        + strResultWHERE + @" and pos01_cv.l_alta_web=0   UNION      ";

//            varQuery2 = varQuery2 + @"select   COUNT(pos01_cv.l_alta_web) as CantReg  " + varQueryRep + "  WHERE   "
//                        + strResultWHERE + @" and pos01_cv.l_alta_web=1 and pos01_cv.c_nro_doc not in
//          (  ";

//            varQuery2 = varQuery2 + @" select    pos01_cv.c_nro_doc  as c_nro_doc " + varQueryRep + "  WHERE   "
//            + strResultWHERE + @" and pos01_cv.l_alta_web=0 )  '";

            varQuery = varQuery + varQuery2 + @"')/ROWS/ROW"; // ESTE QueryString lo reutiliza en la busqueda, solo cambis los valores de salida
            varQuery2 =   @"</qry:xquery>
                <qry:out>
                  <qry:element name=""object"">
                    <qry:attribute value=""$r/@CantReg"" name=""CantReg""/>
                  </qry:element>
                </qry:out>
              </qry:select>  ";

            varQueryParam = @"<FILTRO />";
            NomadEnvironment.GetTrace().Info(" Funcion clsBuscarCV_busCantRegistros:   varQuery " + varQuery);

            xmlDoc = NomadEnvironment.QueryXML( varQuery + varQuery2     , varQueryParam);
            NomadEnvironment.GetTrace().Info(" Funcion clsBuscarCV_busCantRegistros:   xmlDoc " + xmlDoc.OuterXml);
            for (int x = 0; x < xmlDoc.DocumentElement.ChildNodes.Count;x++ )
            {
                mRta =Int32.Parse(xmlDoc.DocumentElement.ChildNodes[x].Attributes["CantReg"].Value);
            }
            return  mRta ;

        }

        //////////////////////////////////////////////////////////////////
        ////              CierraCondOR             ///////////////////////
        ////////////////////////////////////////////////////////////////

        System.Xml.XmlDocument obtieneRegistros()
        {
            System.Xml.XmlDocument xmlDoc;

            string varQueryParam;

            NomadEnvironment.GetTrace().Info(" Funcion clsBuscarCV:  obtieneRegistros ENTRO ");
            varQuery = varQuery.Replace("COUNT(pos01_cv.l_alta_web) as CantReg", " pos01_cv.oi_cv as oi_cv, left(pos01_cv.c_cv,3) as c_cv ");

//            varQuery = @"
//            <qry:main doc=""PARAM"">
//                <qry:append-doc name=""FILTRO"" doc-path=""#PARAM:/FILTRO""/>
//                  <qry:element name=""RESUL_BUSQ"">
//                    <qry:attribute value=""1"" name=""nmd-col""/>
//                    <qry:insert-select doc-path=""#FILTRO:"" name=""filtro_empresa""/>
//                  </qry:element>
//              </qry:main>
//              <qry:select doc=""PARAM"" name=""filtro_empresa"">
//                <qry:xquery>
//                  for $r in table(' select distinct pos01_cv.oi_cv, left(c_cv,3) from pos01_cv
//                                      left JOIN pos01_conoc_cv on pos01_conoc_cv.oi_cv = pos01_cv.oi_cv
//                          left JOIN pos01_cursos_cv on pos01_cursos_cv.oi_cv = pos01_cv.oi_cv
//                          left JOIN pos01_compet_cv on pos01_compet_cv.oi_cv = pos01_cv.oi_cv
//                          left JOIN pos01_idiomas_cv on pos01_idiomas_cv.oi_cv = pos01_cv.oi_cv
//                          left JOIN pos01_estudios_cv on pos01_estudios_cv.oi_cv = pos01_cv.oi_cv
//                          WHERE " + strResultWHERE + @"')/ROWS/ROW
//                        </qry:xquery>
//                  <qry:out>
//                    <qry:element name=""RES"">
//                      <qry:attribute name=""id"" value=""$r/@oi_cv""/>
//                    </qry:element>
//                  </qry:out>
//            </qry:select>";

             varQueryParam = @"<FILTRO />";
            NomadEnvironment.GetTrace().Info("varQuery " + varQuery);
            NomadEnvironment.GetTrace().Info("varQueryParam " + varQueryParam);

            varQuery = varQuery + @"</qry:xquery><qry:out>
                    <qry:element name=""RES"">
                      <qry:attribute name=""id"" value=""$r/@oi_cv""/>
                    </qry:element>
                  </qry:out></qry:select>  ";

            NomadEnvironment.GetTrace().Info(" Funcion clsBuscarCV_obtieneRegistros:   varQuery " + varQuery);

            xmlDoc = NomadEnvironment.QueryXML(varQuery.ToString(), varQueryParam);
            NomadEnvironment.GetTrace().Info(" Funcion clsBuscarCV_busCantRegistros:   xmlDoc " + xmlDoc.OuterXml);

            //System.Collections.IList objCV ;
            NomadEnvironment.GetTrace().Info("Funcion clsBuscarCV_busCantRegistros:   sale");
            //objCV = NomadEnvironment.GetObjects(varQuery,varQueryParam, typeof( NucleusRH.Base.Postulantes.CV.CV) );

            return xmlDoc;

        }

        ////////////////////////////////////////////////////////////
    }

}



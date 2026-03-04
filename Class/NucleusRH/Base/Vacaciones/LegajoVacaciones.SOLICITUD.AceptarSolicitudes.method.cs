     	string sFecDesde ;
			string sFecHasta ;
			string strResult;
			int nDiasSolic;
			NucleusRH.Base.Vacaciones.LegajoVacaciones.CTA_CTE_VAC  objCtaCte ;

			XmlElement m_nodesFlow;
			NucleusRH.Base.Vacaciones.LegajoVacaciones.PERSONAL_EMP obj_Per;
			XmlDocument domXML2;			
			
			System.Text.StringBuilder sbQuery = new System.Text.StringBuilder();			
			System.Text.StringBuilder sbQuery2 = new System.Text.StringBuilder();			

			sbQuery2.Append("<DATOS>" );
			sbQuery2.Append("<PERSONAS nmd-col=\"1\">");
			sbQuery2.Append("<PERSONA oi_solicitud =\"1\" id=\"1\" codigo=\"161\" oi_personal_emp=\"RUBINICH, MARIANA\" desde=\"20070101\" hasta=\"20070126\" dias=\"26\" motivo=\"Prueba solicitud\" c_estado_solic=\"SOLICITADA\" chk_seleccion=\"0\" />");
			sbQuery2.Append("<PERSONA oi_solicitud =\"2\" id=\"38\" codigo=\"72\" oi_personal_emp=\"MARTORELL, MARIQUENA\" desde=\"20070101\" hasta=\"20070115\" dias=\"15\" motivo=\"Prueba solicitud\" c_estado_solic=\"SOLICITADA\" chk_seleccion=\"0\" />");
			sbQuery2.Append("</PERSONAS>");
			sbQuery2.Append("</DATOS>");


			domXML2 = new XmlDocument() ;
			domXML2.LoadXml  (sbQuery2.ToString());

			m_nodesFlow  =  (XmlElement) domXML2.DocumentElement.ChildNodes.Item(0);  


foreach(XmlElement m_nodeFlow in m_nodesFlow.ChildNodes ) 
{ 
	
	nDiasSolic = int.Parse (m_nodeFlow.Attributes["dias"].Value); 
	sFecDesde  = m_nodeFlow.Attributes["desde"].Value ;
	sFecHasta  = m_nodeFlow.Attributes["hasta"].Value  ; 
    sbQuery = new System.Text.StringBuilder();         
	sbQuery.Append("<qry:main xmlns:qry=\"objects\" doc=\"PARAM\">" );
	sbQuery.Append("<qry:element name=\"horas\">");
	sbQuery.Append("<qry:insert-select name=\"qryPersonalList\" />");
	sbQuery.Append("</qry:element>");
	sbQuery.Append("</qry:main>");

	sbQuery.Append("<qry:select xmlns:qry=\"qryPersonalList\" name=\"qryPersonalList\" >");
	sbQuery.Append("<qry:xquery>");
	sbQuery.Append("for $f in table('");
	sbQuery.Append("SELECT oi_cta_cte_vac , f_desde_periodo , f_hasta_periodo  ,oi_cta_cte_vac, oi_personal_emp, VAC01_CTA_CTE_VAC.oi_periodo,  e_dias_gen, e_dias_otorg, e_dias_interrup, e_dias_cta_per_ant, e_dias_pend  ");
	sbQuery.Append(" FROM VAC01_CTA_CTE_VAC ,vac04_periodos ");
	sbQuery.Append(" WHERE VAC01_CTA_CTE_VAC.oi_periodo = vac04_periodos.oi_periodo ");
	sbQuery.Append(" AND oi_personal_emp =" + m_nodeFlow.GetAttribute("id") +   " and e_dias_pend >0");
	sbQuery.Append(" order by f_desde_periodo ");
	sbQuery.Append("')/ROWS/ROW");
	sbQuery.Append("</qry:xquery>");
        
	sbQuery.Append("<qry:out>");
	sbQuery.Append("<qry:element name=\"hora\">");
	sbQuery.Append("<qry:attribute name=\"oi_cta_cte_vac\" value=\"$f/@oi_cta_cte_vac\" />");
	sbQuery.Append("<qry:attribute name=\"oi_personal_emp\" value=\"$f/@oi_personal_emp\" />");
	sbQuery.Append("<qry:attribute name=\"oi_periodo\" value=\"$f/@oi_periodo\" />");
	sbQuery.Append("<qry:attribute name=\"f_desde_periodo\" value=\"$f/@f_desde_periodo\" />");
	sbQuery.Append("<qry:attribute name=\"f_hasta_periodo\" value=\"$f/@f_hasta_periodo\" />");
	sbQuery.Append("<qry:attribute name=\"oi_cta_cte_vac\" value=\"$f/@oi_cta_cte_vac\" />");
	sbQuery.Append("<qry:attribute name=\"e_dias_gen\" value=\"$f/@e_dias_gen\" />");
	sbQuery.Append("<qry:attribute name=\"e_dias_otorg\" value=\"$f/@e_dias_otorg\" />");
	sbQuery.Append("<qry:attribute name=\"e_dias_interrup\" value=\"$f/@e_dias_interrup\" />");
	sbQuery.Append("<qry:attribute name=\"e_dias_cta_per_ant\" value=\"$f/@e_dias_cta_per_ant\" />");
	sbQuery.Append("<qry:attribute name=\"e_dias_pend\" value=\"$f/@e_dias_pend\" />");

	sbQuery.Append("</qry:element>");
	sbQuery.Append("</qry:out>");

	sbQuery.Append("</qry:select>");
	//Ejecuta el query
	strResult=sbQuery.ToString();	
	
	domXML2 = Nomad.NSystem.Base.NomadEnvironment.QueryXML (  sbQuery.ToString(), ""  );

	NucleusRH.Base.Vacaciones.LegajoVacaciones.SOLICITUD ObjSolPer;
	obj_Per= NucleusRH.Base.Vacaciones.LegajoVacaciones.PERSONAL_EMP.Get (m_nodeFlow.GetAttribute("id"))  ;	

	ObjSolPer = obj_Per.SOLICITUDES.GetById(m_nodeFlow.GetAttribute("oi_solicitud"));
           
          

}












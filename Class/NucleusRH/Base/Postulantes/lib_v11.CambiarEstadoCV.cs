using System;
using System.Xml;
using System.Collections;

using Nomad.NSystem.Proxy;
using Nomad.NSystem.Functions;
using Nomad.NSystem.Base;

namespace NucleusRH.Base.Postulantes.CV.clsCambiosEstado
{

    public class CambioEstadoCV
    {

        private NomadProxy m_Proxy;
        private BatchService objBatch;
        private TraceService objTrace;
        private ArrayList arrDetalleErr;
        private NomadTrace trace;
        private  int  m_i_cantErr;
        public CambioEstadoCV(NomadProxy pobjProxy)
        {
            m_Proxy = pobjProxy;
            objBatch = this.m_Proxy.Batch();
            objTrace = objBatch.Trace;
            trace = NomadEnvironment.GetTrace();
            m_i_cantErr = 0;

        }
        public void Modif_EstadoCV()
        {

            string rta;
            string strOI;
            System.Xml.XmlDocument xmlDocCal;
            arrDetalleErr = new ArrayList();
            NucleusRH.Base.Postulantes.CV.CV objPosCV;

            Array arrCampos;
            strOI = ",";
            arrCampos = strOI.Split(',');
            // busca el valor del parametro
            rta = BuscaHijos(" ORG26_PARAMETROS ", "ORG26_PARAMETROS.c_parametro= " + @"\'CantDiasParaDisponible\'", " ORG26_PARAMETROS.d_valor as d_valor", " d_valor ");
            this.trace.Debug(" parametro  " +rta );

            xmlDocCal = EjecutarQuery(" POS01_CV ", " datediff(now(),  POS01_CV.f_estado) >  " + @"\'" + rta + @"\'", " POS01_CV.oi_cv as oi_cv ", "oi_cv", ref arrCampos);

            this.trace.Debug(" xml con CVs a Modificar  " + xmlDocCal.ToString() );

            // busca el motivo
            rta = BuscaHijos(" POS07_MOT_CAMBIOS ", " POS07_MOT_CAMBIOS.c_mot_cambio = " + @"\'04\'", " POS07_MOT_CAMBIOS.oi_mot_cambio as oi_mot_cambio", "oi_mot_cambio");
            this.trace.Debug(" Motivo  " + rta );

        try
        {
            foreach (System.Xml.XmlNode xmlCal in xmlDocCal.DocumentElement.ChildNodes)
            {
                // Obtiene el CV a modificar
                strOI = xmlCal.Attributes.Item(0).ChildNodes.Item(0).Value;
                objPosCV = NucleusRH.Base.Postulantes.CV.CV.Get(strOI);

                this.trace.Debug(" Cambio Estado CV   " + strOI );

                    //actualiza capos dentro del CV
                    objPosCV.c_ult_estado = objPosCV.c_estado;
                    objPosCV.f_estado = DateTime.Today;
                    objPosCV.c_estado = "D";
                    objPosCV.oi_mot_cambio = rta;
                    NomadEnvironment.GetCurrentTransaction().Save(objPosCV);
             }
        }
        catch (Exception e)
        {
            m_i_cantErr++;
            this.objTrace.Add("err", e.Message, "CambioEstadoCV");
            this.trace.Debug(" Cambio Estado CV   " + e.Message );
            arrDetalleErr.Add(e.Message);
            //return false;
        }

  }

        public void Modif_VencidoCV()
        {

            string rta;
            string strOI;
            System.Xml.XmlDocument xmlDocCal;
            arrDetalleErr = new ArrayList();
            NucleusRH.Base.Postulantes.CV.CV objPosCV;

            Array arrCampos;
            strOI = ",";
            arrCampos = strOI.Split(',');
            // busca el valor del parametro
            rta = BuscaHijos(" ORG26_PARAMETROS ", "ORG26_PARAMETROS.c_parametro= " + @"\'CantDiasParaVencidos\'", " ORG26_PARAMETROS.d_valor as d_valor", " d_valor ");
            this.trace.Debug(" parametro  " + rta);

            xmlDocCal = EjecutarQuery(" POS01_CV ", " datediff(now(),  POS01_CV.f_estado) >  " + @"\'" + rta + @"\'", " POS01_CV.oi_cv as oi_cv ", "oi_cv", ref arrCampos);
            this.trace.Debug(" xml con CV a modificar " + xmlDocCal.ToString());

            // busca el motivo
            rta = BuscaHijos(" POS07_MOT_CAMBIOS ", " POS07_MOT_CAMBIOS.c_mot_cambio = " + @"\'05\'", " POS07_MOT_CAMBIOS.oi_mot_cambio as oi_mot_cambio", "oi_mot_cambio");
            this.trace.Debug(" Motivo  " + rta);

            try
            {

                foreach (System.Xml.XmlNode xmlCal in xmlDocCal.DocumentElement.ChildNodes)
                {
                    // Obtiene el CV a modificar
                    strOI = xmlCal.Attributes.Item(0).ChildNodes.Item(0).Value;
                    objPosCV = NucleusRH.Base.Postulantes.CV.CV.Get(strOI);

                    this.trace.Debug(" Cambio Estado CV   " + strOI);
                        //actualiza capos dentro del CV
                        objPosCV.c_ult_estado = objPosCV.c_estado;
                        objPosCV.f_estado = DateTime.Today;
                        objPosCV.c_estado = "V";
                        objPosCV.oi_mot_cambio = rta;
                        NomadEnvironment.GetCurrentTransaction().Save(objPosCV);
                 }
            }
            catch (Exception e)
            {
                m_i_cantErr++;
                this.trace.Debug(e.Message);
                this.objTrace.Add("err", e.Message, "ModifVencido");
                this.trace.Debug(" Modif Vencido CV   " + e.Message );

                arrDetalleErr.Add(e.Message);
                //return false;
            }

        }

        private string BuscaHijos(string strTabla, string strWhere, string strCampos, string strCamposAlias)
        {
            System.Xml.XmlDocument xmlDocCal;
            string sOutQry;
            string strGetOI;
            string strOI;
            strGetOI = "";
            Array arrCampos;

            arrCampos = strCamposAlias.Split(',');

            xmlDocCal = EjecutarQuery(strTabla, strWhere, strCampos, strCamposAlias , ref arrCampos);

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
                  if (strOI.Length > 1)
                  {
                      if (strOI.Substring(0, 1) == ",")
                      { strOI = strOI.Substring(1, strOI.Length - 1); }
                  }

                  return strOI;
            }
            catch (Exception e)
            {
                this.trace.Debug(" BuscaHijos   " + e.Message );
                  strOI ="";
                  return strOI;
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

            //NomadEnvironment.GetTrace().Info("  Clase: BuscaHijos     Funcion: Comparar"   ) ;

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
                this.objTrace.Add("err", e.Message, "EjecIntClientes");
                arrDetalleErr.Add(e.Message);
                xmlDocCal = new System.Xml.XmlDocument();
                this.trace.Debug(" EjecutarQuery   " + e.Message );

                return xmlDocCal;
            }

        }

        }

}



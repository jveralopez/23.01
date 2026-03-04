

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

using NucleusRH.Base.Tiempos_Trabajados.Esperanza;
namespace NucleusRH.Base.Tiempos_Trabajados.Horarios
{

    public class clsCreaEsperanzaH
    {

        private DateTime m_Fecha_Des;
        private DateTime m_Fecha_Has;

        private NucleusRH.Base.Tiempos_Trabajados.Esperanza.ESPERANZA objEsp;
        private NucleusRH.Base.Tiempos_Trabajados.Horarios.HORARIO objHor;
        private NucleusRH.Base.Tiempos_Trabajados.Esperanza.DIA objEspDia;
        private NucleusRH.Base.Tiempos_Trabajados.Esperanza.DETALLE objEspDet;
        private int m_oi_Horario;

        private int m_e_semana;
        private int m_oi_calendario;
        private int m_oi_escuadra;
        private int nPosIni;
        private string strHashNomad;
        private Nomad.NSystem.Proxy.NomadXML nmdXML;
        private int mAvance;
        private string sListaPos;
        private Hashtable HashFeriados;
        public clsCreaEsperanzaH()
        {

        }
// paso 1
        public static Nomad.NSystem.Proxy.NomadXML GetHope(DateTime f_desde, DateTime f_hasta, int p_oi_Horario, int p_oi_calendario, int p_oi_escuadra)
        {
            TimeSpan difDays;
            DateTime mfecha_Init;
            int mSemanaDesde;
            int mSemanaHasta;
            string strDia;

          try
          {
            clsCreaEsperanzaH objCLSEsp;

            Nomad.NSystem.Proxy.NomadXML nmdXMLPPal;
            NucleusRH.Base.Tiempos_Trabajados.Esperanza.ESPERANZA objHope;
            Nomad.NSystem.Proxy.NomadXML nmdXML2;

            strDia = "";
            objCLSEsp = new clsCreaEsperanzaH();

            mfecha_Init = new DateTime(2000, 01, 02);

            //Busco el numero de semana en la que se encuentra la fecha desde (a partir de la fecha init)
            difDays = (f_desde - mfecha_Init);
            mSemanaDesde = difDays.Days / 7;

            //Busco el numero de la semana en la que se encuentra la fecha hasta (a partir de la fecha init)
            difDays = (f_hasta - mfecha_Init);
            mSemanaHasta = difDays.Days / 7;

            nmdXML2 = new Nomad.NSystem.Proxy.NomadXML();
            nmdXMLPPal = new Nomad.NSystem.Proxy.NomadXML();
            nmdXML2 = nmdXMLPPal.AddHeadElement("Days");

            for (int mSem = mSemanaDesde; mSem <= mSemanaHasta; mSem++)
            {
                Nomad.NSystem.Base.NomadBatch.Trace("... Ejecuta para la semana" + mSem.ToString());
                objHope = objCLSEsp.GetHopeInternal(p_oi_Horario, mSem, p_oi_calendario, p_oi_escuadra);
                foreach (NucleusRH.Base.Tiempos_Trabajados.Esperanza.DIA objDia in objHope.DIAS)
                {
                    if (objDia.f_dia >= f_desde && objDia.f_dia <= f_hasta)
                    {
                        strDia = objDia.SerializeAll();
                        nmdXML2.AddText(strDia);
                    }

                }
            }
            nmdXML2.AddTailElement("Days");

            return nmdXML2;
        }
            catch (Exception e)
            {
                Nomad.NSystem.Base.NomadBatch.Trace("error en GetHope por semana" + e.Message);
               return null;

            }

        }
////////////////////////////////////
// paso 1
////////////////////////////////////
        public static NucleusRH.Base.Tiempos_Trabajados.Esperanza.ESPERANZA GetHope(int p_oi_Horario, int p_e_semana, int p_oi_calendario, int p_oi_escuadra)
        {
          try
          {
            clsCreaEsperanzaH objCLSEsp;
            objCLSEsp = new clsCreaEsperanzaH();
            return objCLSEsp.GetHopeInternal(p_oi_Horario,  p_e_semana, p_oi_calendario, p_oi_escuadra);
          }
          catch (Exception e)
            {
                Nomad.NSystem.Base.NomadBatch.Trace("error en GetHope  " + e.Message);
                return null;

            }

        }
////////////////////////////////////
// Paso 2
////////////////////////////////////
 public NucleusRH.Base.Tiempos_Trabajados.Esperanza.ESPERANZA GetHopeInternal(int p_oi_Horario, int p_e_semana, int p_oi_calendario, int p_oi_escuadra)
   {

          try
          {
            string str;
            str = " p_oi_Horario " + p_oi_Horario.ToString();
            str += " p_e_semana " + p_e_semana.ToString();
            str += " p_oi_calendario " + p_oi_calendario.ToString();
            str += " p_oi_escuadra " + p_oi_escuadra.ToString();

            NucleusRH.Base.Tiempos_Trabajados.Esperanza.ESPERANZA objCache;
            string sClave = p_oi_Horario.ToString() + "*" + p_e_semana.ToString() + "*" + p_oi_calendario.ToString() + "*" + p_oi_escuadra.ToString();
            objCache = (NucleusRH.Base.Tiempos_Trabajados.Esperanza.ESPERANZA)NomadProxy.GetProxy().CacheGetObj(sClave);

            if (objCache != null)
            {
                //Nomad.NSystem.Base.NomadBatch.Trace("... ...  clsCreaEsperanzaH  GetHopeInternal   ESTA EN CACHE " + sClave);
                //comentado el 11/05/2009
              return objCache;
            }

            m_oi_Horario = p_oi_Horario;
            m_e_semana = p_e_semana;
            m_oi_calendario = p_oi_calendario;
            m_oi_escuadra = p_oi_escuadra;
            mAvance = 0;
            InitEsperanza(); // paso 3

            NomadProxy.GetProxy().CacheAdd(sClave, (NucleusRH.Base.Tiempos_Trabajados.Esperanza.ESPERANZA)objEsp);
            return objEsp;
          }
          catch (Exception e)
            {
                Nomad.NSystem.Base.NomadBatch.Trace("error en GetHopeInternal  " + e.Message);
                return null;

            }
        }

        private string BuscaHijos(string strTabla, string strWhere, string strCampos, string strCamposAlias)
        {
            System.Xml.XmlDocument xmlDocCal;
            string strGetOI;
            string strOI;

            strGetOI = "";
            Array arrCampos;

            arrCampos = strCamposAlias.Split(',');

            xmlDocCal = EjecutarQuery(strTabla, strWhere, strCampos, strCamposAlias, ref arrCampos);

            strOI = "";
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

            //Nomad.NSystem.Base.NomadBatch.Trace("...  Clase: BuscaHijos     Funcion: Comparar"   ) ;

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

            //Nomad.NSystem.Base.NomadBatch.Trace("...Funcion:  BuscaEspGuardada  varQuery " + varQuery) ;
            varQueryParam = @"<FILTRO />";
            xmlDocCal = NomadEnvironment.QueryXML(varQuery.ToString(), varQueryParam);
            return xmlDocCal;
        }
////////////////////////////////////
// PASO 3
////////////////////////////////////
        private void InitEsperanza()
        {
            string strOI;
            Array arrCampos;
            try
            {
            strOI = ",";
            arrCampos = strOI.Split(',');
            //
            Nomad.NSystem.Base.NomadBatch.Trace("hash");
            CargarHash();
            // busca si la esperanza ya esta cargada
/* 11/05/2009
            sWhere = "";
            sWhere = sWhere + "   TTA03_Esperanza.oi_calendario = " + m_oi_calendario.ToString();
            sWhere = sWhere + "  and TTA03_Esperanza.oi_horario  = " + m_oi_Horario.ToString();
            if (m_oi_escuadra != 0)
            {
                sWhere = sWhere + "  and TTA03_Esperanza.oi_escuadra = " + m_oi_escuadra.ToString();
            }
            sWhere = sWhere + "  and TTA03_Esperanza.e_semana    = " + m_e_semana.ToString();
            rta = BuscaHijos(" TTA03_Esperanza ", sWhere, " TTA03_Esperanza.oi_esperanza as oi_esperanza", " oi_esperanza ");
            if (rta == "")
            {
*/
                objEsp = new NucleusRH.Base.Tiempos_Trabajados.Esperanza.ESPERANZA();

                // Carga la cabecera de la Esperanza para cuando llegue al SAVE
                objEsp.e_semana = m_e_semana;
                objEsp.oi_calendario = m_oi_calendario.ToString();

                if (m_oi_escuadra.ToString() == "0")
                {
                    objEsp.oi_escuadraNull = true;
                }
                else
                {
                  objEsp.oi_escuadra = m_oi_escuadra.ToString();
                }
                  objEsp.oi_horario = m_oi_Horario.ToString();
                  GeneraNuevaEsperanza();

                  /*            }
                              else
                              {

                                  objEsp = NucleusRH.Base.Tiempos_Trabajados.Esperanza.ESPERANZA.Get(rta);

                                  if (objEsp.Hash.ToString() != strHashNomad)
                                  {
                                      objEsp.DIAS.Clear();
                                      GeneraNuevaEsperanza();
                                  }
                                  else
                                  {
                                      return;
                                  }
                              }
                  */

            objEsp.Hash = strHashNomad;

         /*            Nomad.NSystem.Base.NomadBatch.Trace("...clsCreaEsperanzaH funcion InitEsperanza ANTES DEL SAVE");
            NomadEnvironment.GetCurrentTransaction().SaveRefresh(objEsp);
            Nomad.NSystem.Base.NomadBatch.Trace("...clsCreaEsperanzaH funcion InitEsperanza DESPUES DEL SAVE");
*/

      }
    catch (Exception e)
            {
                Nomad.NSystem.Base.NomadBatch.Trace("error en InitEsperanza  " + e.Message);

            }

        }

////////////////////////////////////
// PASO 4
////////////////////////////////////
        private void CargarHash()
        {
            string strRows;
            DateTime mfecha;
            int nPos;
            DateTime mfecha_Init;
            TimeSpan dato;
            int nPosicion;
            try
            {
                  NucleusRH.Base.Tiempos_Trabajados.Horarios.ESCUADRA objEsc = null;

                  objHor = NucleusRH.Base.Tiempos_Trabajados.Horarios.HORARIO.Get(m_oi_Horario);

                  mfecha_Init = new DateTime(2000, 01, 02);

                  // paso la semana a fecha
                  mfecha = mfecha_Init;

                  m_Fecha_Des = mfecha.AddDays(m_e_semana * 7);

                  m_Fecha_Has = mfecha.AddDays((m_e_semana * 7) + 6);

                  dato = (m_Fecha_Des - mfecha_Init);

                  // la posocion se calcula diferencia de Fechas (fecha inicial - fecha de busqueda)
                  // modulo cantidad de dias del horario

                  nPosIni = (dato.Days % objHor.e_dias);
                  //Nomad.NSystem.Base.NomadBatch.Trace("H2 -- " + m_oi_escuadra);
                  if(m_oi_escuadra != 0)
                  {
                    objEsc = NucleusRH.Base.Tiempos_Trabajados.Horarios.ESCUADRA.Get(m_oi_escuadra);
                    mAvance = objEsc.e_avance;
                  }
                  else
                    mAvance = 0;

                  nPos = nPosIni + mAvance;

                  nPosicion = 0;
                  sListaPos = "*";
                  //  GRABA LA LISTA CON EL ORDEN DE LAS POSICIONES
                  Nomad.NSystem.Base.NomadBatch.Trace("J");
                  for (int n = 0; n < objHor.e_dias; n++)
                  {
                      nPosicion = ((nPos + n) % objHor.e_dias);
                      sListaPos = sListaPos + "," + nPosicion.ToString();
                  }
                  sListaPos = sListaPos.Replace("*,", "");

                  // arma la hash en base a una consulta (recurso) que ejecuta en la clase Esperanza
                  string strParametros = "<DATOS fecDesde=\"" + StringUtil.date2str(m_Fecha_Des) + "\" fecHasta=\"" + StringUtil.date2str(m_Fecha_Has) + "\"   sPosiciones=\"" + sListaPos + "\"  oi_horario=\"" + m_oi_Horario + "\"    oi_calendario=\"" + m_oi_calendario + "\" />";
                  //
                  //Nomad.NSystem.Base.NomadBatch.Trace("...CargarHash envía al recurso los parametros " + strParametros);
                  strRows = NomadEnvironment.QueryString(NucleusRH.Base.Tiempos_Trabajados.Esperanza.ESPERANZA.Resources.QRY_ObtieneHash, strParametros);
                  nmdXML = new Nomad.NSystem.Proxy.NomadXML();

                  nmdXML.SetText(strRows);
                  strHashNomad = StringUtil.GetMD5(strRows+ "||" + this.GetType().Assembly.GetFiles()[0].Name) ;
                  // Nomad.NSystem.Base.NomadBatch.Trace("...Hash obtenida " + strHashNomad);

            }
          catch (Exception e)
            {
                Nomad.NSystem.Base.NomadBatch.Trace("error en InitEsperanza  " + e.Message);

            }

        }
////////////////////////////////////////////////////////
// paso 5
////////////////////////////////////////////////////////
        private void GeneraNuevaEsperanza()
        {
            // carga el xml de calendarios que uso para buscar los feriados y armar la hash
            try
            {
                HashFeriados = new Hashtable();
                for (NomadXML cur = nmdXML.FirstChild().FindElement("Feriados").FirstChild(); cur != null; cur = cur.Next())
                {
                    HashFeriados.Add(cur.GetAttr("c_feriado"), cur.GetAttr("f_feriado"));
                }

                //objHor.HORA_TUR_DET
                ArmaOrdenDetalles();
           }
          catch (Exception e)
            {
                Nomad.NSystem.Base.NomadBatch.Trace("error en GeneraNuevaEsperanza  " + e.Message);

            }

        }
///////////////////////////////////////////
// paso 6
///////////////////////////////////////////
        private void ArmaOrdenDetalles()
        {            
            DateTime mFechaIns;

            string mTipo_hora;
            string mTipo_horario;
            string p_c_Tipo;
            DateTime mFecVerif;
          try
          {
                  Hashtable HasPosiciones;
                  p_c_Tipo = "";

                  mFechaIns = DateTime.Today;
                  HasPosiciones = new Hashtable();
                  // armo un array con el orden de las posiciones para recorrelo y armar los detalles
                  sListaPos = sListaPos + ',' + sListaPos ;
                  string[] arrPosiciones = this.sListaPos.Split(',');
                  string mFeriado;
                  NomadXML xmlPos;
                  mFeriado = "";

                  NomadXML xmlNmd = nmdXML.FirstChild().FindElement("Horarios").FirstChild();
                  mTipo_horario = xmlNmd.GetAttr("d_tipohorario");

                  // si no tiene posiciones , sale, solo guarda la cabecera
                  xmlPos = xmlNmd.FindElement2("Posicion", "e_posicion", "0");
                  if (xmlPos == null)
                  {
                      // Nomad.NSystem.Base.NomadBatch.Trace("... Funcion: ArmaEspDiaria oArmaOrdenDetalles NO TIENE POSICIONES");
                      return;
                  }

                  for (int item = 0; item < 7; item++)
                  {
                      mFeriado = arrPosiciones[item].ToString();
                      xmlPos = xmlNmd.FindElement2("Posicion", "e_posicion", mFeriado);
                      mFecVerif = m_Fecha_Des.AddDays(item);
                      objEspDia = new NucleusRH.Base.Tiempos_Trabajados.Esperanza.DIA();

                      objEspDia.e_posicion = Int32.Parse(arrPosiciones[item]);
                      objEspDia.f_dia = mFecVerif;
                      objEspDia.oi_turno = xmlPos.GetAttr("oi_turno"); ;
                      objEsp.DIAS.Add(objEspDia);

                      if (xmlPos.FirstChild() != null)
                      {
                          for (NomadXML cur = xmlPos.FirstChild(); cur != null; cur = cur.Next())
                          {

                              mTipo_hora = ObtieneTipoHora(cur, mTipo_horario, mFecVerif, ref   p_c_Tipo);
                              objEspDia.c_tipo = p_c_Tipo;

                              objEspDet = new NucleusRH.Base.Tiempos_Trabajados.Esperanza.DETALLE();
                              objEspDet.e_horafin = Int32.Parse(cur.GetAttr("e_hora_fin"));
                              objEspDet.e_horainicio = Int32.Parse(cur.GetAttr("e_hora_inicio"));
                              objEspDet.oi_estructura = xmlPos.GetAttr("oi_estructura");
                              objEspDet.oi_tipohora = mTipo_hora;
                              objEspDia.DETALLE.Add(objEspDet);
                          }
                      }
                      else
                      {
                          objEspDia.c_tipo = ObtieneTipoHoraDescanso(mFecVerif, mTipo_horario);
                      }
                  }

           }
          catch (Exception e)
            {
                Nomad.NSystem.Base.NomadBatch.Trace("error en ArmaOrdenDetalles  " + e.Message);

            }

        }

        private string ObtieneTipoHora(NomadXML objHTD, string sTipoHor, DateTime mFechaVerif, ref  string s_tipoDia)
        {
            //
            // Nomad.NSystem.Base.NomadBatch.Trace("... Funcion: ObtieneTipoHora");
            string mTipo_hora;
            DayOfWeek m_dia;
           try
           {
            mTipo_hora = "";
            mTipo_hora = objHTD.GetAttr("oi_tipo_hs_norm");
            s_tipoDia = "N";
            // Si es Domingo lo setea
            m_dia = mFechaVerif.DayOfWeek;
            if (m_dia == 0)
            {
                mTipo_hora = objHTD.GetAttr("oi_tipo_hs_dom");
                s_tipoDia = "D";
            }

            if (HashFeriados.ContainsKey("FER_" + StringUtil.date2str(mFechaVerif)) || HashFeriados.ContainsKey("_" + StringUtil.date2str(mFechaVerif)))
            {
				
                mTipo_hora = objHTD.GetAttr("oi_tipo_hs_fer");
                s_tipoDia = "F";
                if ((m_dia == 0) && (sTipoHor == "R"))
                {
                    mTipo_hora = objHTD.GetAttr("oi_tipo_hs_domfer");
                    s_tipoDia = "DF";
                }
            }
			if (HashFeriados.ContainsKey("NOLAB_" + StringUtil.date2str(mFechaVerif)))
            {
				mTipo_hora = objHTD.GetAttr("oi_tipo_hs_nolab");
                s_tipoDia = "NL";
               
			}

            return mTipo_hora.ToString();

           }
          catch (Exception e)
            {
                Nomad.NSystem.Base.NomadBatch.Trace("error en ArmaOrdenDetalles  " + e.Message);
                 return "";
            }

        }

        /////////////////////////////////////////////////
           private string ObtieneTipoHoraDescanso(DateTime mFechaVerif, string sTipoHor)
        {
            //
            string s_tipoDia;
            // Nomad.NSystem.Base.NomadBatch.Trace("... Funcion: ObtieneTipoHoraDescanso");            
            DayOfWeek m_dia;
           try
           {                
                s_tipoDia = "N";
                // Si es Domingo lo setea
                m_dia = mFechaVerif.DayOfWeek;
                if (m_dia == 0)
                {
                    s_tipoDia = "D";
                }

				if (HashFeriados.ContainsKey("FER_" + StringUtil.date2str(mFechaVerif)) || HashFeriados.ContainsKey("_" + StringUtil.date2str(mFechaVerif)))
				{
                    s_tipoDia = "F";
                    if ((m_dia == 0) && (sTipoHor == "R"))
                      {
                       s_tipoDia = "DF";
                    }
                }
				
				if (HashFeriados.ContainsKey("NOLAB_" + StringUtil.date2str(mFechaVerif)))
				{
					s_tipoDia = "NL";
				   
				}

                return s_tipoDia;

           }
          catch (Exception e)
            {
                Nomad.NSystem.Base.NomadBatch.Trace("error en ObtieneTipoHoraDescanso  " + e.Message);
                 return "";
            }

        }

    }
}



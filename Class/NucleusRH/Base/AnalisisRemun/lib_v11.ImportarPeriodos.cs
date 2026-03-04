using System;
using System.Collections;
using System.Text;
using System.Xml;
using System.Globalization;

using Nomad.NSystem.Base;
using Nomad.NSystem.Proxy;
using Nomad.NSystem.Functions;
using NucleusRH.Base.AnalisisRemun.Interface_LegAnalisis;
namespace NucleusRH.Base.AnalisisRemun.Interfaces
{

    class clsImportarPeriodos
    {   NomadBatch objBatch;
        string slegajo;
        public void ImportarRemuneracion (string n_oi_empresa , string b_chkReemplaza)
        {

             // ABRE EL XML que contiene las definiciones de actualizacion de campos Origen/Destino (nomad/datamart)

            NomadXML nmdXML_Indic;

            NucleusRH.Base.AnalisisRemun.Legajo_Analisis.PERSONAL_EMP Obj_PerEmp;

            string strOiVarArchivo;
              objBatch = NomadBatch.GetBatch("Iniciando...", "Actualizando Remuneraciones ");

            ENTRADA objArchivo;
            string oi_bus;
            DateTime d_nPeriodo_Par;
            DateTime d_PeriodoDB;

            try
            {

              NomadBatch.Trace("  entra al TRY " );
              string strOIsArchivo = NomadProxy.GetProxy().SQLService().Get(ENTRADA.Resources.qry_OiVarRemun, "");

              NomadBatch.Trace("  strOIsArchivo: " + strOIsArchivo);

              //Recorre el archivo y pide los DDO de interface
              XmlTextReader xtrOIA = new XmlTextReader(strOIsArchivo, System.Xml.XmlNodeType.Document, null);

              xtrOIA.XmlResolver = null; // ignore the DTD
              xtrOIA.WhitespaceHandling = WhitespaceHandling.None;
              xtrOIA.Read();

              while (xtrOIA.Read())
              {

                  NomadBatch.Trace("=================================================================================================================================" );
                  slegajo= "";
                  strOiVarArchivo = xtrOIA.GetAttribute("id");
                  objArchivo = ENTRADA.Get(strOiVarArchivo);
                  NomadBatch.Trace("  strOiVarArchivo: " + strOiVarArchivo);
                  NomadBatch.Trace("  objArchivo: " + objArchivo.SerializeAll());

                  if (strOiVarArchivo=="")    {return;}
                  slegajo=  objArchivo.n_legajo.ToString();
                  NomadBatch.Trace("  SELECT " +  "e_numero_legajo=" + slegajo + " and oi_empresa=" + n_oi_empresa);
                  oi_bus = BuscaHijos("per02_personal_emp", "e_numero_legajo=" + slegajo + " and oi_empresa=" + n_oi_empresa, "oi_personal_emp", "oi_personal_emp");

                  if (oi_bus=="")    {continue;}

                  Obj_PerEmp = NucleusRH.Base.AnalisisRemun.Legajo_Analisis.PERSONAL_EMP.Get(oi_bus);

                  NomadBatch.Trace("  d_PeriodoDB: " + Obj_PerEmp.e_periodo.ToString() + "  d_nPeriodo_Par: " + objArchivo.e_periodo.ToString() );

                  NomadBatch.Trace("  Obj_PerEmp: " + Obj_PerEmp.SerializeAll() );
                  NomadBatch.Trace("  ***********************************");
                  if (Obj_PerEmp.e_periodo == objArchivo.e_periodo)
                  {
                      if (b_chkReemplaza!="1")
                      {
                          NomadBatch.Trace("  Acumula REM02_Personal_emp ");
                          Obj_PerEmp.e_periodo += objArchivo.e_periodo;
                          Obj_PerEmp.n_basico += objArchivo.n_basico;
                          Obj_PerEmp.n_conformado += objArchivo.n_conformado;
                          Obj_PerEmp.n_adicionales += objArchivo.n_adicionales;
                          Obj_PerEmp.n_sujeto += objArchivo.n_sujeto;
                          Obj_PerEmp.n_nosujeto += objArchivo.n_nosujeto;
                          Obj_PerEmp.n_bruto += objArchivo.n_bruto;
                          Obj_PerEmp.n_descuento += objArchivo.n_descuento;
                          Obj_PerEmp.n_neto += objArchivo.n_neto;
                          Obj_PerEmp.n_contribuciones += objArchivo.n_contribuciones;

                      }
                  }

                  // SI NO EXISTE EL PERIODO EN LA PERSONA , DEJA LA FECHA DEL ARCHIVO Y PONE EL RESTO EN CERO PARA QUE LO COMPLETE EN EL PROXIMO IF
                  if (Obj_PerEmp.e_periodoNull ==true )
                  {
                          NomadBatch.Trace("  INGRESA REM02_Personal_emp ");
                        Obj_PerEmp.e_periodo = objArchivo.e_periodo;
                          Obj_PerEmp.e_numero_legajo = objArchivo.n_legajo;
                          Obj_PerEmp.n_basico = objArchivo.n_basico;
                          Obj_PerEmp.n_conformado = objArchivo.n_conformado;
                          Obj_PerEmp.n_adicionales = objArchivo.n_adicionales;
                          Obj_PerEmp.n_sujeto = objArchivo.n_sujeto;
                          Obj_PerEmp.n_nosujeto = objArchivo.n_nosujeto;
                          Obj_PerEmp.n_bruto = objArchivo.n_bruto;
                          Obj_PerEmp.n_descuento = objArchivo.n_descuento;
                          Obj_PerEmp.n_neto = objArchivo.n_neto;
                          Obj_PerEmp.n_contribuciones = objArchivo.n_contribuciones;
                  }

                  if (Obj_PerEmp.e_periodo <= objArchivo.e_periodo)
                  {
                      if (b_chkReemplaza=="1")
                      {
                          NomadBatch.Trace("  Reemplaza REM02_Personal_emp ");
                          Obj_PerEmp.e_periodo = objArchivo.e_periodo;
                          //Obj_PerEmp.e_numero_legajo = objArchivo.n_legajo;
                          Obj_PerEmp.n_basico = objArchivo.n_basico;
                          Obj_PerEmp.n_conformado = objArchivo.n_conformado;
                          Obj_PerEmp.n_adicionales = objArchivo.n_adicionales;
                          Obj_PerEmp.n_sujeto = objArchivo.n_sujeto;
                          Obj_PerEmp.n_nosujeto = objArchivo.n_nosujeto;
                          Obj_PerEmp.n_bruto = objArchivo.n_bruto;
                          Obj_PerEmp.n_descuento = objArchivo.n_descuento;
                          Obj_PerEmp.n_neto = objArchivo.n_neto;
                          Obj_PerEmp.n_contribuciones = objArchivo.n_contribuciones;

                      }
                   }

                  if (fncActualizaDatos(ref Obj_PerEmp, b_chkReemplaza,objArchivo ))
                  {
                       NomadBatch.Trace("  Obj_PerEmp: " + Obj_PerEmp.SerializeAll() );

                      NomadEnvironment.GetCurrentTransaction().Save(Obj_PerEmp);
                  }

                   NomadBatch.Trace("=================================================================================================================================" );

               }
                  xtrOIA.Close();

              }

            catch (Exception e)
            {
                objBatch.Err(" Error de ejecucion para el Legajo: " + slegajo +  " Empresa:" + n_oi_empresa  );
                NomadBatch.Trace("  error en ImportarRemuneracion: " + e.Message);
                return ;

            }

        }
 //-------------------------------------------------------------------------------
//-------------------------------------------------------------------------------
        private bool fncActualizaDatos(ref NucleusRH.Base.AnalisisRemun.Legajo_Analisis.PERSONAL_EMP Obj_PerEmpUpd, string b_Reemplaza, ENTRADA objArchivo_ent )
        {

            try
            {    int e_periodo_par;
                e_periodo_par= objArchivo_ent.e_periodo   ;

                int n_encontro;
                n_encontro=0;
                NucleusRH.Base.AnalisisRemun.Legajo_Analisis.PERIODO obj_Periodo;
                NomadBatch.Trace("  fncActualizaDatos PERIODOS: " + Obj_PerEmpUpd.PERIODOS.Count.ToString());

                if (Obj_PerEmpUpd.PERIODOS.Count == 0)
                {
                    NomadBatch.Trace("Sin periodo ingresa Nuevo Periodo ");
                    obj_Periodo = new NucleusRH.Base.AnalisisRemun.Legajo_Analisis.PERIODO() ;
                    obj_Periodo.e_periodo = objArchivo_ent.e_periodo ;
                    obj_Periodo.oi_personal_emp  = objArchivo_ent.id ;
                    obj_Periodo.n_basico = objArchivo_ent.n_basico;
                    obj_Periodo.n_conformado = objArchivo_ent.n_conformado;
                    obj_Periodo.n_adicionales = objArchivo_ent.n_adicionales;
                    obj_Periodo.n_sujeto = objArchivo_ent.n_sujeto;
                    obj_Periodo.n_nosujeto = objArchivo_ent.n_nosujeto;
                    obj_Periodo.n_bruto = objArchivo_ent.n_bruto;
                    obj_Periodo.n_descuento = objArchivo_ent.n_descuento;
                    obj_Periodo.n_neto = objArchivo_ent.n_neto;
                    obj_Periodo.n_contribuciones = objArchivo_ent.n_contribuciones;
                    Obj_PerEmpUpd.PERIODOS.Add( obj_Periodo);
                    return true;
                }

                for (int x = 0; x < Obj_PerEmpUpd.PERIODOS.Count; x++)
                {
                    obj_Periodo = (NucleusRH.Base.AnalisisRemun.Legajo_Analisis.PERIODO) Obj_PerEmpUpd.PERIODOS[x];

                     NomadBatch.Trace(" obj_Periodo.e_periodo " + obj_Periodo.e_periodo.ToString() );
                     NomadBatch.Trace(" e_periodo_par " + e_periodo_par.ToString() );

                    if (obj_Periodo.e_periodo == e_periodo_par)
                    {

                        n_encontro=1;
                        if (b_Reemplaza=="1")
                        {
                           NomadBatch.Trace(" Reemplaza Periodo ");
                            obj_Periodo.n_basico = objArchivo_ent.n_basico;
                            obj_Periodo.n_conformado = objArchivo_ent.n_conformado;
                            obj_Periodo.n_adicionales = objArchivo_ent.n_adicionales;
                            obj_Periodo.n_sujeto = objArchivo_ent.n_sujeto;
                            obj_Periodo.n_nosujeto = objArchivo_ent.n_nosujeto;
                            obj_Periodo.n_bruto = objArchivo_ent.n_bruto;
                            obj_Periodo.n_descuento = objArchivo_ent.n_descuento;
                            obj_Periodo.n_neto = objArchivo_ent.n_neto;
                            obj_Periodo.n_contribuciones = objArchivo_ent.n_contribuciones;
                        }
                        else
                        {
                            NomadBatch.Trace("  Acumula Periodo");

                            obj_Periodo.n_basico += objArchivo_ent.n_basico;
                            obj_Periodo.n_conformado += objArchivo_ent.n_conformado;
                            obj_Periodo.n_adicionales += objArchivo_ent.n_adicionales;
                            obj_Periodo.n_sujeto += objArchivo_ent.n_sujeto;
                            obj_Periodo.n_nosujeto += objArchivo_ent.n_nosujeto;
                            obj_Periodo.n_bruto += objArchivo_ent.n_bruto;
                            obj_Periodo.n_descuento += objArchivo_ent.n_descuento;
                            obj_Periodo.n_neto += objArchivo_ent.n_neto;
                            obj_Periodo.n_contribuciones += objArchivo_ent.n_contribuciones;
                        }

                    }
                }

                if (n_encontro == 0)
                {
                    NomadBatch.Trace("  Nuevo Periodo ");
                    obj_Periodo = new NucleusRH.Base.AnalisisRemun.Legajo_Analisis.PERIODO() ;
                    obj_Periodo.e_periodo = e_periodo_par ;
                    obj_Periodo.oi_personal_emp  = Obj_PerEmpUpd.id ;
                    obj_Periodo.n_basico = objArchivo_ent.n_basico;
                    obj_Periodo.n_conformado = objArchivo_ent.n_conformado;
                    obj_Periodo.n_adicionales = objArchivo_ent.n_adicionales;
                    obj_Periodo.n_sujeto = objArchivo_ent.n_sujeto;
                    obj_Periodo.n_nosujeto = objArchivo_ent.n_nosujeto;
                    obj_Periodo.n_bruto = objArchivo_ent.n_bruto;
                    obj_Periodo.n_descuento = objArchivo_ent.n_descuento;
                    obj_Periodo.n_neto = objArchivo_ent.n_neto;
                    obj_Periodo.n_contribuciones = objArchivo_ent.n_contribuciones;
                    Obj_PerEmpUpd.PERIODOS.Add( obj_Periodo);
                }

                return true;
            }

            catch (Exception e)
            {
                 objBatch.Err(" Error de ejecucion de Periodo para el Legajo: " + Obj_PerEmpUpd.id.ToString() +  " Periodo:" + Obj_PerEmpUpd.e_periodo.ToString()  );
                NomadBatch.Trace("  error en fncActualizaDatos: " + e.Message);
                return false;

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
                          for $r in table('  SELECT Distinct " + strCampos + @"  FROM "
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
                  objBatch.Err(" Error al obtener el legajo: " + slegajo   );

                  NomadBatch.Trace("  error en EjecutarQuery: " + e.Message);
                  NomadBatch.Trace("  varQuery: " + varQuery.ToString());
                 return null;
            }

        }

        /////////////////////////////////////////////////////////
        // query reutilizable para saber si exsite registro
        //
        ////////////////////////////////////////////////////////

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
                objBatch.Err(" Error al obtener el legajo: " + slegajo   );
                NomadBatch.Trace("  error en BuscaHijos: " + e.Message);
                strOI = "";
                return strOI;
            }

        }

    }
}



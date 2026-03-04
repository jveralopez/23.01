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

namespace NucleusRH.Base.Postulantes.Ofertas_Laborales
{

  
  public partial class OFERTA_LAB
  {

      /// <summary>
      /// Controlador de Ofertas Laborales WEB
      /// </summary>
      /// <param name="pMode"></param>
      /// <param name="pParam"></param>
      /// <returns></returns>
      public static Nomad.NSystem.Proxy.NomadXML WebOLManager(string pMode, Nomad.NSystem.Proxy.NomadXML pParam)
      {
          NomadXML xmlResult = null;
          NomadXML xmlParam = null;

          xmlParam = pParam.isDocument ? pParam.FirstChild() : pParam;

          NomadLog.Debug("-----------------------------------------------");
          NomadLog.Debug("----------WebOLManager() V1--------------------");
          NomadLog.Debug("-----------------------------------------------");

          NomadLog.Debug("pMode: " + pMode.ToString());
          NomadLog.Debug("pParam: " + pParam.ToString());

          switch (pMode.ToUpper())
          {
              case "OLMAINPAGE":
                  xmlResult = NucleusRH.Base.Postulantes.Ofertas_Laborales.OFERTA_LAB.WEB_OLMainPage(xmlParam);
                  break;
              case "OLLIST":
                  xmlResult = NucleusRH.Base.Postulantes.Ofertas_Laborales.OFERTA_LAB.WEB_OLList(xmlParam);
                  break;
              case "OLLOADFORM":
                  xmlResult = NucleusRH.Base.Postulantes.Ofertas_Laborales.OFERTA_LAB.WEB_OLLoadForm(xmlParam);
                  break;
              case "OLLOADCHILDFORM":
                  xmlResult = NucleusRH.Base.Postulantes.Ofertas_Laborales.OFERTA_LAB.WEB_OLLoadChild(xmlParam);
                  break;
              case "OLDELETE":
                  xmlResult = NucleusRH.Base.Postulantes.Ofertas_Laborales.OFERTA_LAB.WEB_OLDelete(xmlParam);
                  break;
              case "MASIVEDELETEOFERTALAB":
                  xmlResult = NucleusRH.Base.Postulantes.Ofertas_Laborales.OFERTA_LAB.WEB_OLDeleteMasivo(xmlParam);
                  break;
              case "OLLOADLIST":
                  xmlResult = NucleusRH.Base.Postulantes.Ofertas_Laborales.OFERTA_LAB.WEB_OLLoadList(xmlParam);
                  break;
              case "OLPOSTUCVLIST":
                  xmlResult = NucleusRH.Base.Postulantes.Ofertas_Laborales.OFERTA_LAB.WEB_OLPostuCVList(xmlParam);
                  break;
              case "MARCARCVOFERTALAB":
                  xmlResult = NucleusRH.Base.Postulantes.Ofertas_Laborales.OFERTA_LAB.WEB_MarcarCVPostulacion(xmlParam);
                  break;
              case "LEIDONOLEIDOOL":
                  xmlResult = NucleusRH.Base.Postulantes.Ofertas_Laborales.OFERTA_LAB.WEB_Leido_NoLeidoOL(xmlParam);
                  break;
              case "GRAPHICS":
                  xmlResult = NucleusRH.Base.Postulantes.Ofertas_Laborales.OFERTA_LAB.WEB_Graphic(xmlParam);
                  break;
              case "OLSAVE":
                  xmlResult = NucleusRH.Base.Postulantes.Ofertas_Laborales.OFERTA_LAB.WEB_SaveOfertaLab(xmlParam);
                  break;
              case "COUNTREQ":
                  xmlResult = NucleusRH.Base.Postulantes.Ofertas_Laborales.OFERTA_LAB.WEB_COUNT_REQ(xmlParam);
                  break;
              case "LOADOLSHOW":
                  xmlResult = NucleusRH.Base.Postulantes.Ofertas_Laborales.OFERTA_LAB.WEB_LoadOfertaLab_Show(xmlParam);
                  break;
              case "LOADOLHEAD":
                  xmlResult = NucleusRH.Base.Postulantes.Ofertas_Laborales.OFERTA_LAB.WEB_LoadOfertaLab_Head(xmlParam);
                  break;
              case "ISACTIVE":
                  xmlResult = NucleusRH.Base.Postulantes.Ofertas_Laborales.OFERTA_LAB.WEB_IsActive(xmlParam);
                  break;
              case  "GETRESP":
                  xmlResult = NucleusRH.Base.Postulantes.Ofertas_Laborales.OFERTA_LAB.WEB_GetRespuestas(xmlParam);
                  break;
              case "OICV4PAGINATION":
                  xmlResult = NucleusRH.Base.Postulantes.Ofertas_Laborales.OFERTA_LAB.WEB_OiCV4Pagination(xmlParam);
                  break;
              

          }

          return xmlResult;

      }


      /// <summary>
      /// Valida que el usuario logueado este asociado a una persona, para poder crear y duplicar ofertas
      /// </summary>
      /// <param name="pParam"></param>
      /// <returns></returns>
      public static NomadXML WEB_IsActive(NomadXML pParam)
      {
        NomadLog.Debug("-----------------------------------------------");
        NomadLog.Debug("----------WEB_IsActive-------------------------");
        NomadLog.Debug("-----------------------------------------------");

        NomadLog.Debug("WEB_IsActive.pParam: " + pParam.ToString());

       
        NomadXML xmlResult;
        string strMode;
        string strStep = "";

        strMode = pParam.GetAttr("mode");

        xmlResult = new NomadXML("DATA");
        xmlResult.SetAttr("mode", strMode);
        try
        {
            strStep = "QRY-ACTIVE";
            NomadXML xmlResultMain = Nomad.NSystem.Base.NomadEnvironment.QueryNomadXML(NucleusRH.Base.Postulantes.Ofertas_Laborales.OFERTA_LAB.Resources.IS_ACTIVE_USER,"");
            xmlResult.AddTailElement(xmlResultMain);
        }
        catch (Exception ex)
        {
            NomadException nmdEx = new NomadException("NucleusRH.Base.Postulantes.Ofertas_Laborales.OFERTA_LAB.WEB_OLMainPage()", ex);
            nmdEx.SetValue("Step", strStep);
            throw nmdEx;
        }

         NomadLog.Debug("xmlResultIsActive: " + xmlResult.ToString());
         return xmlResult;

      
      }

       
      
      /// <summary>
      /// Carga las 10 ultimas ofertas laborales o las 10 ultimas mias para la pantalla del main
      /// </summary>
      /// <param name="pParam"></param>
      /// <returns></returns>
      public static NomadXML WEB_OLMainPage(NomadXML pParam)
      {
        NomadLog.Debug("-----------------------------------------------");
        NomadLog.Debug("----------WEB_OLMainPage V1------------");
        NomadLog.Debug("-----------------------------------------------");

        NomadLog.Debug("WEB_OLMainPage.pParam: " + pParam.ToString());

       
        NomadXML xmlResult;
        string strMode;
        string strStep = "";

        strMode = pParam.GetAttr("mode");

        xmlResult = new NomadXML("DATA");
        xmlResult.SetAttr("mode", strMode);
        try
        {
            NomadXML xmlParam = new NomadXML("DATA");
            //Si el param es mias, agrego usuario logueado
            if(pParam.GetAttr("mias")=="1")
            {
                strStep = "GET-USUARIO-LOGEUADO";
                string strOiUsr = Nomad.NSystem.Proxy.NomadProxy.GetProxy().UserEtty;
                xmlParam.SetAttr("oi_usuario_sistema", strOiUsr);
            }
            strStep="QRY-GET";
            NomadXML xmlResultMain = Nomad.NSystem.Base.NomadEnvironment.QueryNomadXML(NucleusRH.Base.Postulantes.Ofertas_Laborales.OFERTA_LAB.Resources.GET_OFERTASLAB_MAIN,xmlParam.ToString());
            xmlResult.AddTailElement(xmlResultMain);
        }
        catch (Exception ex)
        {
            NomadException nmdEx = new NomadException("NucleusRH.Base.Postulantes.Ofertas_Laborales.OFERTA_LAB.WEB_OLMainPage()", ex);
            nmdEx.SetValue("Step", strStep);
            throw nmdEx;
        }

         NomadLog.Debug("xmlResultMainOL: " + xmlResult.ToString());
         return xmlResult;

      
      }

      
      /// <summary>
      /// Lista de Ofertas Laborales
      /// </summary>
      /// <param name="pParam"></param>
      /// <returns></returns>
      public static NomadXML WEB_OLList(NomadXML pParam)
      {
        NomadLog.Debug("-----------------------------------------------");
        NomadLog.Debug("----------WEB_OLList V1------------------------");
        NomadLog.Debug("-----------------------------------------------");

        NomadLog.Debug("WEB_OLList.pParam: " + pParam.ToString());

       
        NomadXML xmlResult;
        string strMode;
        string strStep = "";

        strMode = pParam.GetAttr("mode");

        xmlResult = new NomadXML("DATA");
        xmlResult.SetAttr("mode", strMode);
        
        try
        {
            NomadXML xmlParam = new NomadXML("DATA");
            xmlParam.SetAttr("campo",pParam.GetAttr("campo"));
            xmlParam.SetAttr("criterio",pParam.GetAttr("criterio"));
            xmlParam.SetAttr("estado", pParam.GetAttr("estado"));
            xmlParam.SetAttr("campo_buscar", pParam.GetAttr("campo_buscar"));
            xmlParam.SetAttr("f_alta_desde", pParam.GetAttr("f_alta_desde"));
            xmlParam.SetAttr("f_alta_hasta", pParam.GetAttr("f_alta_hasta"));
            xmlParam.SetAttr("f_cierre_desde", pParam.GetAttr("f_cierre_desde"));
            xmlParam.SetAttr("f_cierre_hasta", pParam.GetAttr("f_cierre_hasta"));
            xmlParam.SetAttr("filtroPersona", pParam.GetAttr("filtroPersona"));
            xmlParam.SetAttr("oi_empresa", pParam.GetAttr("oi_empresa"));
            xmlParam.SetAttr("oi_tipos_serv", pParam.GetAttr("oi_tipos_serv"));
            xmlParam.SetAttr("oi_pais", pParam.GetAttr("oi_pais"));
            xmlParam.SetAttr("oi_provincia", pParam.GetAttr("oi_provincia"));
            xmlParam.SetAttr("oi_localidad", pParam.GetAttr("oi_localidad"));

            //ILT 02.11.2016 - Paginacion
            xmlParam.SetAttr("fromrow", pParam.GetAttr("fromrow")); //fromrow es en base 0
            xmlParam.SetAttr("maxsize", pParam.GetAttrInt("pagesize") + 1);

            strStep = "GET-LIST-OL";
            xmlParam.SetAttr("qryMode", "LISTOL");
            NomadLog.Debug("WEB_OLList.xmlParamList: " + xmlParam.ToString());

            NomadXML xmlResultList = Nomad.NSystem.Base.NomadEnvironment.QueryNomadXML(NucleusRH.Base.Postulantes.Ofertas_Laborales.OFERTA_LAB.Resources.GET_OFERTAS_LIST, xmlParam.ToString());
            xmlResult.AddTailElement(xmlResultList);

            //ILT 02.11.2016 - Paginacion
            NomadXML xmlPagination = xmlResult.AddTailElement("PAGINATION");
            xmlPagination.SetAttr("name", "PAGINATION");
            if (xmlResultList.FirstChild().ChildLength > pParam.GetAttrInt("pagesize"))
            {
                NomadXML ultimoRow = xmlResultList.FirstChild().LastChild();
                xmlResultList.FirstChild().DeleteChild(ultimoRow);
                xmlPagination.SetAttr("islastpage", "0");
            }
            else
            {
                xmlPagination.SetAttr("islastpage", "1");
            }

            strStep = "GET-CANT-OL";
            xmlParam.SetAttr("qryMode", "COUNTOL");
            NomadLog.Debug("WEB_OLList.xmlParamCount: " + xmlParam.ToString());

            NomadXML xmlResultCant = Nomad.NSystem.Base.NomadEnvironment.QueryNomadXML(NucleusRH.Base.Postulantes.Ofertas_Laborales.OFERTA_LAB.Resources.GET_OFERTAS_LIST, xmlParam.ToString());
            xmlResult.AddTailElement(xmlResultCant);

        }
        catch (Exception ex)
        {
            NomadException nmdEx = new NomadException("NucleusRH.Base.Postulantes.Ofertas_Laborales.OFERTA_LAB.WEB_OLList()", ex);
            nmdEx.SetValue("Step", strStep);
            throw nmdEx;
        }

        NomadLog.Debug("xmlResultListOL: " + xmlResult.ToString());
        return xmlResult;


      }

      /// <summary>
      /// Carga los datos para combos padres del load del formulario de alta de ofertas laborales
      /// </summary>
      /// <param name="pParam"></param>
      /// <returns></returns>
      public static NomadXML WEB_OLLoadForm(NomadXML pParam)
      {
        NomadLog.Debug("-----------------------------------------------");
        NomadLog.Debug("----------WEB_OLLoadForm V1------------------------");
        NomadLog.Debug("-----------------------------------------------");

        NomadLog.Debug("WEB_OLLoadForm.pParam: " + pParam.ToString());

       
        NomadXML xmlResult;
        string strMode;
        string strStep = "";

        strMode = pParam.GetAttr("mode");

        xmlResult = new NomadXML("DATA");
        xmlResult.SetAttr("mode", strMode);
        try
        {
            strStep = "GET-LOAD-FORM";
            NomadXML xmlResultLoad = Nomad.NSystem.Base.NomadEnvironment.QueryNomadXML(NucleusRH.Base.Postulantes.Ofertas_Laborales.OFERTA_LAB.Resources.LOAD_OFERTA_LAB_FORM, "");
            xmlResult.AddTailElement(xmlResultLoad);

            if (pParam.GetAttr("oi_oferta_lab") != "")
            {

                strStep = "GET-OL-EDIT";
                NucleusRH.Base.Postulantes.Ofertas_Laborales.OFERTA_LAB objOfertaLabEdit = Postulantes.Ofertas_Laborales.OFERTA_LAB.Get(pParam.GetAttr("oi_oferta_lab"));
                NomadXML xmlOfertaLabEdit = new NomadXML("OFERTA-LAB-EDIT");
                xmlOfertaLabEdit.SetAttr("name", "OFERTA-LAB-EDIT");

                strStep = "SET-OL-CAMPOS-BASE";
                xmlOfertaLabEdit.SetAttr("c_oferta_lab", objOfertaLabEdit.c_oferta_lab);
                xmlOfertaLabEdit.SetAttr("d_oferta_lab", objOfertaLabEdit.d_oferta_lab);
                xmlOfertaLabEdit.SetAttr("o_oferta_lab", objOfertaLabEdit.o_oferta_lab);
                xmlOfertaLabEdit.SetAttr("f_oferta_lab", objOfertaLabEdit.f_oferta_lab);
                if (!objOfertaLabEdit.f_cierreNull) xmlOfertaLabEdit.SetAttr("f_cierre", objOfertaLabEdit.f_cierre);
                if (!objOfertaLabEdit.e_cantidadNull) xmlOfertaLabEdit.SetAttr("e_cantidad", objOfertaLabEdit.e_cantidad);
                if (!objOfertaLabEdit.oi_empresaNull) xmlOfertaLabEdit.SetAttr("oi_empresa", objOfertaLabEdit.oi_empresa);
                if (!objOfertaLabEdit.oi_tipos_servNull) xmlOfertaLabEdit.SetAttr("oi_tipos_serv", objOfertaLabEdit.oi_tipos_serv);
                if (!objOfertaLabEdit.oi_tipo_puestoNull) xmlOfertaLabEdit.SetAttr("oi_tipo_puesto", objOfertaLabEdit.oi_tipo_puesto);
                if (!objOfertaLabEdit.oi_seniorityNull) xmlOfertaLabEdit.SetAttr("oi_seniority", objOfertaLabEdit.oi_seniority);
                if (!objOfertaLabEdit.oi_area_labNull) xmlOfertaLabEdit.SetAttr("oi_area_lab", objOfertaLabEdit.oi_area_lab);
                if (!objOfertaLabEdit.oi_paisNull) xmlOfertaLabEdit.SetAttr("oi_pais", objOfertaLabEdit.oi_pais);
                if (!objOfertaLabEdit.oi_provinciaNull) xmlOfertaLabEdit.SetAttr("oi_provincia", objOfertaLabEdit.oi_provincia);
                if (!objOfertaLabEdit.oi_localidadNull) xmlOfertaLabEdit.SetAttr("oi_localidad", objOfertaLabEdit.oi_localidad);
                xmlOfertaLabEdit.SetAttr("oi_personal", objOfertaLabEdit.oi_personal);
                NucleusRH.Base.Personal.Legajo.PERSONAL objPersona = Personal.Legajo.PERSONAL.Get(objOfertaLabEdit.oi_personal);
                xmlOfertaLabEdit.SetAttr("c_personal", objPersona.c_personal);
                xmlOfertaLabEdit.SetAttr("d_personal", objPersona.d_ape_y_nom);
                if (!objOfertaLabEdit.d_mailNull) xmlOfertaLabEdit.SetAttr("d_mail", objOfertaLabEdit.d_mail);
                if (!objOfertaLabEdit.d_mail_alt_1Null) xmlOfertaLabEdit.SetAttr("d_mail_alt_1", objOfertaLabEdit.d_mail_alt_1);
                if (!objOfertaLabEdit.d_mail_alt_2Null) xmlOfertaLabEdit.SetAttr("d_mail_alt_2", objOfertaLabEdit.d_mail_alt_2);

                strStep = "SET-OL-CAMPOS-REQUISITOS";
                if (!objOfertaLabEdit.oi_pais_resNull) xmlOfertaLabEdit.SetAttr("oi_pais_res", objOfertaLabEdit.oi_pais_res);
                if (!objOfertaLabEdit.oi_provincia_resNull) xmlOfertaLabEdit.SetAttr("oi_provincia_res", objOfertaLabEdit.oi_provincia_res);
                if (!objOfertaLabEdit.oi_localidad_resNull) xmlOfertaLabEdit.SetAttr("oi_localidad_res", objOfertaLabEdit.oi_localidad_res);
                xmlOfertaLabEdit.SetAttr("l_exc_res", objOfertaLabEdit.l_exc_res);
                xmlOfertaLabEdit.SetAttr("l_mostrar_res", objOfertaLabEdit.l_mostrar_res);

                if (!objOfertaLabEdit.e_exp_desNull) xmlOfertaLabEdit.SetAttr("e_exp_des", objOfertaLabEdit.e_exp_des);
                if (!objOfertaLabEdit.oi_uni_tpo_desdeNull) xmlOfertaLabEdit.SetAttr("oi_uni_tpo_desde", objOfertaLabEdit.oi_uni_tpo_desde);
                if (!objOfertaLabEdit.e_exp_hastaNull) xmlOfertaLabEdit.SetAttr("e_exp_hasta", objOfertaLabEdit.e_exp_hasta);
                if (!objOfertaLabEdit.oi_uni_tpo_hastaNull) xmlOfertaLabEdit.SetAttr("oi_uni_tpo_hasta", objOfertaLabEdit.oi_uni_tpo_hasta);
                xmlOfertaLabEdit.SetAttr("l_exc_exp", objOfertaLabEdit.l_exc_exp);
                xmlOfertaLabEdit.SetAttr("l_mostrar_exp", objOfertaLabEdit.l_mostrar_exp);

                if (!objOfertaLabEdit.e_edad_desdeNull) xmlOfertaLabEdit.SetAttr("e_edad_desde", objOfertaLabEdit.e_edad_desde);
                if (!objOfertaLabEdit.e_edad_hastaNull) xmlOfertaLabEdit.SetAttr("e_edad_hasta", objOfertaLabEdit.e_edad_hasta);
                xmlOfertaLabEdit.SetAttr("l_exc_edad", objOfertaLabEdit.l_exc_edad);
                xmlOfertaLabEdit.SetAttr("l_mostrar_edad", objOfertaLabEdit.l_mostrar_edad);

                if (!objOfertaLabEdit.c_sexoNull) xmlOfertaLabEdit.SetAttr("c_sexo", objOfertaLabEdit.c_sexo);
                xmlOfertaLabEdit.SetAttr("l_exc_sexo", objOfertaLabEdit.l_exc_sexo);
                xmlOfertaLabEdit.SetAttr("l_mostrar_sexo", objOfertaLabEdit.l_mostrar_sexo);

                if (!objOfertaLabEdit.oi_estado_civilNull) xmlOfertaLabEdit.SetAttr("oi_estado_civil", objOfertaLabEdit.oi_estado_civil);
                xmlOfertaLabEdit.SetAttr("l_exc_ecivil", objOfertaLabEdit.l_exc_ecivil);
                xmlOfertaLabEdit.SetAttr("l_mostrar_ecivil", objOfertaLabEdit.l_mostrar_ecivil);

                if (!objOfertaLabEdit.oi_nivel_estudioNull) xmlOfertaLabEdit.SetAttr("oi_nivel_estudio", objOfertaLabEdit.oi_nivel_estudio);
                if (!objOfertaLabEdit.oi_estado_estNull) xmlOfertaLabEdit.SetAttr("oi_estado_est", objOfertaLabEdit.oi_estado_est);
                xmlOfertaLabEdit.SetAttr("l_exc_edu", objOfertaLabEdit.l_exc_edu);
                xmlOfertaLabEdit.SetAttr("l_mostrar_edu", objOfertaLabEdit.l_mostrar_edu);

                if (!objOfertaLabEdit.oi_nivel_ingNull) xmlOfertaLabEdit.SetAttr("oi_nivel_ing", objOfertaLabEdit.oi_nivel_ing);
                if (!objOfertaLabEdit.oi_nivel_porNull) xmlOfertaLabEdit.SetAttr("oi_nivel_por", objOfertaLabEdit.oi_nivel_por);
                if (!objOfertaLabEdit.oi_nivel_frNull) xmlOfertaLabEdit.SetAttr("oi_nivel_fr", objOfertaLabEdit.oi_nivel_fr);
                if (!objOfertaLabEdit.oi_nivel_itNull) xmlOfertaLabEdit.SetAttr("oi_nivel_it", objOfertaLabEdit.oi_nivel_it);
                if (!objOfertaLabEdit.oi_nivel_alNull) xmlOfertaLabEdit.SetAttr("oi_nivel_al", objOfertaLabEdit.oi_nivel_al);
                if (!objOfertaLabEdit.oi_nivel_japNull) xmlOfertaLabEdit.SetAttr("oi_nivel_jap", objOfertaLabEdit.oi_nivel_jap);
                xmlOfertaLabEdit.SetAttr("l_exc_idioma", objOfertaLabEdit.l_exc_idioma);
                xmlOfertaLabEdit.SetAttr("l_mostrar_idioma", objOfertaLabEdit.l_mostrar_idioma);

                if (!objOfertaLabEdit.n_salario_dNull) xmlOfertaLabEdit.SetAttr("n_salario_d", objOfertaLabEdit.n_salario_d);
                if (!objOfertaLabEdit.n_salario_hNull) xmlOfertaLabEdit.SetAttr("n_salario_h", objOfertaLabEdit.n_salario_h);
                xmlOfertaLabEdit.SetAttr("l_exc_salario", objOfertaLabEdit.l_exc_salario);
                xmlOfertaLabEdit.SetAttr("l_mostrar_en_aviso", objOfertaLabEdit.l_mostrar_en_aviso);
                xmlOfertaLabEdit.SetAttr("l_solicita_sal", objOfertaLabEdit.l_solicita_sal);

                strStep = "GET-OL-CANT-POSTU";
                NomadXML xmlParam = new NomadXML("DATA");
                xmlParam.SetAttr("oi_oferta_lab", pParam.GetAttr("oi_oferta_lab"));
                NomadXML xmlTienePostulaciones = Nomad.NSystem.Base.NomadEnvironment.QueryNomadXML(NucleusRH.Base.Postulantes.Ofertas_Laborales.OFERTA_LAB.Resources.TIENE_POSTU, xmlParam.ToString());

                xmlOfertaLabEdit.SetAttr("tienePostulaciones",xmlTienePostulaciones.FirstChild().GetAttr("tiene"));

                
                strStep = "SET-PREG";
                NomadXML xmlPreguntas = xmlOfertaLabEdit.AddTailElement("PREGUNTAS");
                xmlPreguntas.SetAttr("name", "PREGUNTAS-EDIT");
                foreach (NucleusRH.Base.Postulantes.Ofertas_Laborales.PREGUNTAS_OL objPreg in objOfertaLabEdit.PREG_OF_LAB)
                {
                    NomadXML xmlPreg = xmlPreguntas.AddTailElement("PREG-EDIT");
                    xmlPreg.SetAttr("oi_pregunta", objPreg.id);
                    xmlPreg.SetAttr("c_pregunta", objPreg.c_pregunta);
                    xmlPreg.SetAttr("d_pregunta", objPreg.d_pregunta);
                    if (objPreg.c_pregunta == "M")
                    {
                        foreach (NucleusRH.Base.Postulantes.Ofertas_Laborales.OPCIONES_PREG objOpcionPreg in objPreg.OPCION_PREG)
                        {
                            NomadXML xmlOpcionPreg = xmlPreg.AddTailElement("OPCION-PREG-EDIT");
                            xmlOpcionPreg.SetAttr("oi_pregunta_op", objOpcionPreg.id);
                            xmlOpcionPreg.SetAttr("d_pregunta_op", objOpcionPreg.d_pregunta_op);
                            xmlOpcionPreg.SetAttr("l_correcta", objOpcionPreg.l_correcta);
                        }
                    }

                }
                xmlResult.AddTailElement(xmlOfertaLabEdit);
            }

 
        }
        catch (Exception ex)
        {
            NomadException nmdEx = new NomadException("NucleusRH.Base.Postulantes.Ofertas_Laborales.OFERTA_LAB.WEB_OLLoadForm()", ex);
            nmdEx.SetValue("Step", strStep);
            throw nmdEx;
        }

        NomadLog.Debug("xmlResultOLLoadForm: " + xmlResult.ToString());
        return xmlResult;


      }

      /// <summary>
      /// Carga los childs en el load del form de oferta lab
      /// </summary>
      /// <param name="pParam"></param>
      /// <returns></returns>
      public static NomadXML WEB_OLLoadChild(NomadXML pParam)
      {
          NomadLog.Debug("-----------------------------------------------");
          NomadLog.Debug("----------WEB_OLLoadChild V1------------------------");
          NomadLog.Debug("-----------------------------------------------");

          NomadLog.Debug("WEB_OLLoadChild.pParam: " + pParam.ToString());


          NomadXML xmlResult;
          string strMode;
          string strStep = "";

          strMode = pParam.GetAttr("mode");

          xmlResult = new NomadXML("DATA");
          xmlResult.SetAttr("mode", strMode);
          try
          {
              strStep = "CREATE-PARAM";
              NomadXML xmlParam = new NomadXML("DATA");
              xmlParam.SetAttr("qryMode",pParam.GetAttr("qryMode"));
              xmlParam.SetAttr("filterParent", pParam.GetAttr("filterParent"));

              strStep = "GET-LOAD-FORM-CHILD";
              NomadXML xmlResultLoadChild = Nomad.NSystem.Base.NomadEnvironment.QueryNomadXML(NucleusRH.Base.Postulantes.Ofertas_Laborales.OFERTA_LAB.Resources.LOAD_CHILD_OL_FORM, xmlParam.ToString());
              xmlResult.AddTailElement(xmlResultLoadChild);

          }
          catch (Exception ex)
          {
              NomadException nmdEx = new NomadException("NucleusRH.Base.Postulantes.Ofertas_Laborales.OFERTA_LAB.WEB_OLLoadChild()", ex);
              nmdEx.SetValue("Step", strStep);
              throw nmdEx;
          }

          NomadLog.Debug("xmlResultOLLoadChild: " + xmlResult.ToString());
          return xmlResult;


      }

      /// <summary>
      /// Elimina de la BD la Oferta Laboral Parámetro
      /// </summary>
      /// <param name="pParam"></param>
      /// <returns></returns>
      public static NomadXML WEB_OLDelete(NomadXML pParam)
      {
          NomadLog.Debug("-----------------------------------------------");
          NomadLog.Debug("----------WEB_OLDelete V1------------");
          NomadLog.Debug("-----------------------------------------------");

          NomadLog.Debug("WEB_OLDelete.pParam: " + pParam.ToString());

          NomadXML xmlResult = new NomadXML("DATA");
          string strMode;
          string strStep = "";
          NomadTransaction objTran = null;
          strMode = pParam.GetAttr("mode");


          try
          {
              strStep = "GET-DEL";
              NucleusRH.Base.Postulantes.Ofertas_Laborales.OFERTA_LAB objOfertaLabToDel = Ofertas_Laborales.OFERTA_LAB.Get(pParam.GetAttr("oi_oferta_lab"));
              strStep = "DELETE-OL";
              objTran = new NomadTransaction();
              objTran.Begin();
              objTran.Delete(objOfertaLabToDel);
              objTran.Commit();


              //Si es show, no hago nada. Sino, veo de recuperar mainPage o ListPage
              if ((pParam.GetAttr("show") != "1"))
              {
                  //El mismo metodo retorna todas las ofertas laborales que quedan si viene desde list
                  //Si viene desde main, retorna las Ofertas Laborales desde main
                  if (pParam.GetAttr("main") == "1")
                      xmlResult = WEB_OLMainPage(pParam);
                  else
                      xmlResult = WEB_OLList(pParam);
              }


              
              
               
              
              //Indico que funciono
              xmlResult.SetAttr("resultOk", "1");
          }
          catch (Exception ex)
          {
              xmlResult.SetAttr("resultOk", "0");
              if (objTran != null)
              {
                  objTran.Rollback();
                  xmlResult.SetAttr("result", "0");
              }

              NomadException nmdEx = new NomadException("NucleusRH.Base.Postulantes.Ofertas_Laborales.OFERTA_LAB.WEB_OLDelete()", ex);
              nmdEx.SetValue("Step", strStep);
              throw nmdEx;
          }

          NomadLog.Debug("xmlResultWEB_OLDelete: " + xmlResult.ToString());
          return xmlResult;

      }


     /// <summary>
     /// Elimina todas las ofertas laborales que llegan en la tira de ids del pParam
     /// </summary>
     /// <param name="pParam"></param>
     /// <returns></returns>
      public static NomadXML WEB_OLDeleteMasivo(NomadXML pParam)
      {
          NomadLog.Debug("-----------------------------------------------");
          NomadLog.Debug("----------WEB_OLDeleteMasivo V1------------");
          NomadLog.Debug("-----------------------------------------------");

          NomadLog.Debug("WEB_OLDeleteMasivo.pParam: " + pParam.ToString());

          NomadXML xmlResult = new NomadXML("DATA");
          string strMode;
          string strStep = "";
          NomadTransaction objTran = null;
          strMode = pParam.GetAttr("mode");
          try
          {
              strStep = "ARMO-ARRAY-IDS";
              string[] arrayIds = pParam.GetAttrString("param").Split(',');

              objTran = new NomadTransaction();
              objTran.Begin();
              strStep = "RECORRO-IDS";
              for (int i = 0; i < arrayIds.Length; i++)
              {
                  strStep = "GET-OFERTA-LAB" + i.ToString();
                  string oi_oferta_lab = arrayIds[i];
                  NucleusRH.Base.Postulantes.Ofertas_Laborales.OFERTA_LAB objOfertaLabToDel = Ofertas_Laborales.OFERTA_LAB.Get(oi_oferta_lab);
                  strStep = "DELETE-OFERTA-LAB-" + i.ToString();
                  objTran.Delete(objOfertaLabToDel);
              }

              //Persisto la eliminacion
              objTran.Commit();

              //Devuelvo las que quedaron

              //El mismo metodo retorna todas las ofertas laborales que quedan
              xmlResult = WEB_OLList(pParam);


              xmlResult.SetAttr("resultOk", "1");
          }
          catch (Exception ex)
          {
              xmlResult.SetAttr("resultOk", "0");
              if (objTran != null)
              {
                  objTran.Rollback();
                  xmlResult.SetAttr("resultOk", "0");
              }

              NomadException nmdEx = new NomadException("NucleusRH.Base.Postulantes.Ofertas_Laborales.OFERTA_LAB.WEB_OLDeleteMasivo()", ex);
              nmdEx.SetValue("Step", strStep);
              throw nmdEx;
          }

          NomadLog.Debug("xmlResultWEB_OLDelete: " + xmlResult.ToString());
          return xmlResult;
      }

      /// <summary>
      /// Carga los datos para combos padres del load de la lista de ofertas laborales
      /// </summary>
      /// <param name="pParam"></param>
      /// <returns></returns>
      public static NomadXML WEB_OLLoadList(NomadXML pParam)
      {
          NomadLog.Debug("-----------------------------------------------");
          NomadLog.Debug("----------WEB_OLLoadList V1------------------------");
          NomadLog.Debug("-----------------------------------------------");

          NomadLog.Debug("WEB_OLLoadList.pParam: " + pParam.ToString());


          NomadXML xmlResult;
          string strMode;
          string strStep = "";

          strMode = pParam.GetAttr("mode");

          xmlResult = new NomadXML("DATA");
          xmlResult.SetAttr("mode", strMode);
          try
          {
              strStep = "GET-LOAD-FORM";
              NomadXML xmlResultLoad = Nomad.NSystem.Base.NomadEnvironment.QueryNomadXML(NucleusRH.Base.Postulantes.Ofertas_Laborales.OFERTA_LAB.Resources.LOAD_OFERTA_LAB_LIST, "");
              xmlResult.AddTailElement(xmlResultLoad);
          }
          catch (Exception ex)
          {
              NomadException nmdEx = new NomadException("NucleusRH.Base.Postulantes.Ofertas_Laborales.OFERTA_LAB.WEB_OLLoadList()", ex);
              nmdEx.SetValue("Step", strStep);
              throw nmdEx;
          }

          NomadLog.Debug("xmlResultOLLoadList: " + xmlResult.ToString());
          return xmlResult;


      }

     /// <summary>
     /// Lista de CVs postulados a una oferta laboral
     /// </summary>
     /// <param name="pParam"></param>
     /// <returns></returns>
      public static NomadXML WEB_OLPostuCVList(NomadXML pParam)
      {
           NomadLog.Debug("-----------------------------------------------");
           NomadLog.Debug("----------WEB_OLPostuCVList V1------------------------");
           NomadLog.Debug("-----------------------------------------------");

           NomadLog.Debug("WEB_OLPostuCVList.pParam: " + pParam.ToString());


          NomadXML xmlResult;
          string strMode;
          string strStep = "";

          strMode = pParam.GetAttr("mode");

          xmlResult = new NomadXML("DATA");
          xmlResult.SetAttr("mode", strMode);
          try
          {
              strStep = "GET-CVs-OfertaLab" + pParam.GetAttrString("oi_oferta_lab");
              NomadXML xmlParam = new NomadXML("DATA");
              xmlParam.SetAttr("oi_oferta_lab", pParam.GetAttr("oi_oferta_lab"));
              xmlParam.SetAttr("campo", pParam.GetAttr("campo"));
              xmlParam.SetAttr("criterio", pParam.GetAttr("criterio"));

              strStep = "QRY-LIST-POSTU-CV";
              xmlParam.SetAttr("qryMode", "LISTPOSTUCV");

              //Agrego parametros de leido, marca, palabras claves, notas y filtros inteligentes
              if (pParam.GetAttr("l_leido") != "")
                  xmlParam.SetAttr("l_leido", pParam.GetAttr("l_leido"));

              if (pParam.GetAttr("c_marca") != "")
                  xmlParam.SetAttr("c_marca", pParam.GetAttr("c_marca"));

           
              if (pParam.GetAttr("l_notas") != "")
                  xmlParam.SetAttr("l_notas", pParam.GetAttr("l_notas"));

              if (pParam.GetAttr("campo_buscar_1") != "")
                  xmlParam.SetAttr("campo_buscar_1", pParam.GetAttr("campo_buscar_1"));

              if (pParam.GetAttr("campo_buscar_2") != "")
                  xmlParam.SetAttr("campo_buscar_2", pParam.GetAttr("campo_buscar_2"));

              if (pParam.GetAttr("campo_buscar_3") != "")
                  xmlParam.SetAttr("campo_buscar_3", pParam.GetAttr("campo_buscar_3"));

              if (pParam.GetAttr("campo_buscar_4") != "")
                  xmlParam.SetAttr("campo_buscar_4", pParam.GetAttr("campo_buscar_4"));

              if (pParam.GetAttr("campo_buscar_5") != "")
                  xmlParam.SetAttr("campo_buscar_5", pParam.GetAttr("campo_buscar_5"));

              if (pParam.GetAttr("campo_buscar_6") != "")
                  xmlParam.SetAttr("campo_buscar_6", pParam.GetAttr("campo_buscar_6"));

              if (pParam.GetAttr("campo_buscar_7") != "")
                  xmlParam.SetAttr("campo_buscar_7", pParam.GetAttr("campo_buscar_7"));

              if (pParam.GetAttr("campo_buscar_8") != "")
                  xmlParam.SetAttr("campo_buscar_8", pParam.GetAttr("campo_buscar_8"));

              if (pParam.GetAttr("campo_buscar_9") != "")
                  xmlParam.SetAttr("campo_buscar_9", pParam.GetAttr("campo_buscar_9"));

              if (pParam.GetAttr("campo_buscar_10") != "")
                  xmlParam.SetAttr("campo_buscar_10", pParam.GetAttr("campo_buscar_10"));

              if (pParam.GetAttr("d_puesto") != "")
                  xmlParam.SetAttr("d_puesto", pParam.GetAttr("d_puesto"));

              if (pParam.GetAttr("d_empresa") != "")
                  xmlParam.SetAttr("d_empresa", pParam.GetAttr("d_empresa"));

              if (pParam.GetAttr("d_estudio") != "")
                  xmlParam.SetAttr("d_estudio", pParam.GetAttr("d_estudio"));

              if (pParam.GetAttr("d_localidad") != "")
                  xmlParam.SetAttr("d_localidad", pParam.GetAttr("d_localidad"));


              
              xmlParam.SetAttr("l_cumple_req", pParam.GetAttr("l_cumple_req"));


              //ILT 02.11.2016 - Paginacion
              xmlParam.SetAttr("fromrow", pParam.GetAttr("fromrow")); //fromrow es en base 0
              xmlParam.SetAttr("maxsize", pParam.GetAttrInt("pagesize") + 1);


              NomadLog.Debug("xmlParamListPostuCVList: " + xmlParam.ToString());
              NomadXML xmlLoad = Nomad.NSystem.Base.NomadEnvironment.QueryNomadXML(NucleusRH.Base.Postulantes.Ofertas_Laborales.OFERTA_LAB.Resources.GET_CV_OFERTA_LAB, xmlParam.ToString());
              xmlResult.AddTailElement(xmlLoad);


              //ILT 02.11.2016 - Paginacion
              NomadXML xmlPagination = xmlResult.AddTailElement("PAGINATION");
              xmlPagination.SetAttr("name", "PAGINATION");
              if (xmlLoad.FirstChild().ChildLength > pParam.GetAttrInt("pagesize"))
              {
                  NomadXML ultimoRow = xmlLoad.FirstChild().LastChild();
                  xmlLoad.FirstChild().DeleteChild(ultimoRow);
                  xmlPagination.SetAttr("islastpage", "0");
              }
              else
              {
                  xmlPagination.SetAttr("islastpage", "1");
              }


              strStep = "COUNTALL";
              xmlParam.SetAttr("qryMode", "COUNTALL");
              NomadXML xmlCant = Nomad.NSystem.Base.NomadEnvironment.QueryNomadXML(NucleusRH.Base.Postulantes.Ofertas_Laborales.OFERTA_LAB.Resources.GET_CV_OFERTA_LAB, xmlParam.ToString());
              xmlResult.AddTailElement(xmlCant);

              

              strStep = "RESUMEN";
              NomadXML xmlList = new NomadXML("DATA");

              NomadXML xmlResumen = xmlResult.AddTailElement("RESUMEN");
              xmlResumen.SetAttr("name", "RESUMEN");
              xmlResumen.SetAttr("mode", "RESUMEN");


              if (xmlCant.FirstChild().GetAttrInt("cantRegistros") > 0)
              {
                  strStep = "GET-RESUMEN";

                  //Para puestos
                  if (pParam.GetAttrInt("noCalculaPuesto") == 0)
                  {
                      strStep = "RESUMEN-PUESTO";
                      xmlParam.SetAttr("qryMode", "RESUMEN");
                      xmlParam.SetAttr("campoResumen", "d_puesto");

                      NomadLog.Debug("xmlParamListCVResumen-Puesto: " + xmlParam.ToString());

                      NomadXML xmlResultResumenPuesto = Nomad.NSystem.Base.NomadEnvironment.QueryNomadXML(NucleusRH.Base.Postulantes.Ofertas_Laborales.OFERTA_LAB.Resources.GET_CV_OFERTA_LAB, xmlParam.ToString());

                      xmlList = xmlResumen.AddTailElement("PUESTOS");
                      xmlList.SetAttr("name", "PUESTOS");
                      for (NomadXML row = xmlResultResumenPuesto.FirstChild().FirstChild(); row != null; row = row.Next())
                      {
                          NomadXML xmlTitulo = xmlList.AddTailElement("PUESTO");
                          xmlTitulo.SetAttr("desc", row.GetAttr("desc"));
                          xmlTitulo.SetAttr("cant", row.GetAttr("cant"));
                      }
                  }
                  else
                  {
                      xmlResumen.AddTailElement("PUESTOS");
                  }

                  //Para Empresa
                  if (pParam.GetAttrInt("noCalculaEmpresa") == 0)
                  {
                      strStep = "RESUMEN-EMPRESA";
                      xmlParam.SetAttr("qryMode", "RESUMEN");
                      xmlParam.SetAttr("campoResumen", "d_empresa");

                      NomadLog.Debug("xmlParamListCVResumen-Empresa: " + xmlParam.ToString());

                      NomadXML xmlResultResumenEmpresas = Nomad.NSystem.Base.NomadEnvironment.QueryNomadXML(NucleusRH.Base.Postulantes.Ofertas_Laborales.OFERTA_LAB.Resources.GET_CV_OFERTA_LAB, xmlParam.ToString());


                      xmlList = xmlResumen.AddTailElement("EMPRESAS");
                      xmlList.SetAttr("name", "EMPRESAS");

                      for (NomadXML row = xmlResultResumenEmpresas.FirstChild().FirstChild(); row != null; row = row.Next())
                      {
                          NomadXML xmlEmp = xmlList.AddTailElement("EMPRESA");
                          xmlEmp.SetAttr("desc", row.GetAttr("desc"));
                          xmlEmp.SetAttr("cant", row.GetAttr("cant"));
                      }
                  }
                  else
                  {
                      xmlResumen.AddTailElement("EMPRESAS");
                  }

                  //Para Titulos
                  if (pParam.GetAttrInt("noCalculaTitulo") == 0)
                  {
                      strStep = "RESUMEN-TITULO";
                      xmlParam.SetAttr("qryMode", "RESUMEN");
                      xmlParam.SetAttr("campoResumen", "d_estudio");

                      NomadLog.Debug("xmlParamListCVResumen-Titulos: " + xmlParam.ToString());

                      NomadXML xmlResultTitulos = Nomad.NSystem.Base.NomadEnvironment.QueryNomadXML(NucleusRH.Base.Postulantes.Ofertas_Laborales.OFERTA_LAB.Resources.GET_CV_OFERTA_LAB, xmlParam.ToString());

                      xmlList = xmlResumen.AddTailElement("TITULOS");
                      xmlList.SetAttr("name", "TITULOS");

                      for (NomadXML row = xmlResultTitulos.FirstChild().FirstChild(); row != null; row = row.Next())
                      {
                          NomadXML xmlTit = xmlList.AddTailElement("TITULO");
                          xmlTit.SetAttr("desc", row.GetAttr("desc"));
                          xmlTit.SetAttr("cant", row.GetAttr("cant"));
                      }
                  }
                  else
                  {
                      xmlResumen.AddTailElement("TITULOS");
                  }


                  //Para Localidades
                  if (pParam.GetAttrInt("noCalculaLocalidad") == 0)
                  {
                      strStep = "RESUMEN-LOCALIDAD";
                      xmlParam.SetAttr("qryMode", "RESUMEN");
                      xmlParam.SetAttr("campoResumen", "d_localidad");

                      NomadLog.Debug("xmlParamListCVResumen-Localidad: " + xmlParam.ToString());

                      NomadXML xmlResultLocalidades = Nomad.NSystem.Base.NomadEnvironment.QueryNomadXML(NucleusRH.Base.Postulantes.Ofertas_Laborales.OFERTA_LAB.Resources.GET_CV_OFERTA_LAB, xmlParam.ToString());

                      xmlList = xmlResumen.AddTailElement("LOCALIDADES");
                      xmlList.SetAttr("name", "LOCALIDADES");

                      for (NomadXML row = xmlResultLocalidades.FirstChild().FirstChild(); row != null; row = row.Next())
                      {
                          NomadXML xmlLoc = xmlList.AddTailElement("LOCALIDAD");
                          xmlLoc.SetAttr("desc", row.GetAttr("desc"));
                          xmlLoc.SetAttr("cant", row.GetAttr("cant"));
                      }
                  }
                  else
                  {
                      xmlResumen.AddTailElement("LOCALIDADES");
                  }



              }
                else
              {
                  NomadXML xmlPuestos = xmlResumen.AddHeadElement("PUESTOS");
                  NomadXML xmlEmpresa = xmlResumen.AddHeadElement("EMPRESAS");
                  NomadXML xmlTitulo = xmlResumen.AddHeadElement("TITULOS");
                  NomadXML xmlLocalidad = xmlResumen.AddHeadElement("LOCALIDADES");
              }

    
              
          }
          catch (Exception ex)
          {
              NomadException nmdEx = new NomadException("NucleusRH.Base.Postulantes.Ofertas_Laborales.OFERTA_LAB.WEB_OLPostuCVList()", ex);
              nmdEx.SetValue("Step", strStep);
              throw nmdEx;
          }

          NomadLog.Debug("xmlResultOLPostuCvList: " + xmlResult.ToString());
          return xmlResult;
      }


      /// <summary>
      /// Marca el CV de la postulacion como
      /// P: Preferido
      /// R: Rojo
      /// V: Verde
      /// A: Azul
      /// SM: Sin Marca
      /// D: Descartado
      /// </summary>
      /// <param name="pParam"></param>
      /// <returns></returns>
      public static NomadXML WEB_MarcarCVPostulacion(NomadXML pParam)
      {
          NomadLog.Debug("-----------------------------------------------");
          NomadLog.Debug("----------WEB_MarcarCVPostulacion V1------------");
          NomadLog.Debug("-----------------------------------------------");

          NomadLog.Debug("WEB_MarcarCVPostulacion.pParam: " + pParam.ToString());

          NomadXML xmlResult;
          string strMode;
          string strStep = "";
          NomadTransaction objTran = null;
          strMode = pParam.GetAttr("mode");
          xmlResult = new NomadXML("DATA");
          xmlResult.SetAttr("mode", strMode);
          try
          {
              strStep = "ARMO-ARRAY-IDS";
              string[] arrayIds = pParam.GetAttrString("paramPostulaciones").Split(',');

              strStep = "RECORRO-IDS";
              for (int i = 0; i < arrayIds.Length; i++)
              {
                  strStep = "GET-POSTU-" + arrayIds[i].ToString();
                  NucleusRH.Base.Postulantes.CV.POSTULACIONES objPostulacion = CV.POSTULACIONES.Get(arrayIds[i]);
                  objPostulacion.c_marca = pParam.GetAttr("marca");
                  objPostulacion.f_marca = DateTime.Now;
                  objPostulacion.oi_usuario_marca = Nomad.NSystem.Proxy.NomadProxy.GetProxy().UserEtty;
                  objPostulacion.d_usuario_marca = Nomad.Base.Login.Entidades.ENTIDAD.Get(objPostulacion.oi_usuario_marca).DES;
                  objTran = new NomadTransaction();
                  objTran.Begin();
                  objTran.Save(objPostulacion);
                  objTran.Commit();
              }

              xmlResult.SetAttr("resultOk", "1");
          }
          catch (Exception ex)
          {
              xmlResult.SetAttr("resultOk", "0");
              if (objTran != null)
              {
                  objTran.Rollback();
                  xmlResult.SetAttr("result", "0");
              }

              NomadException nmdEx = new NomadException("NucleusRH.Base.Postulantes.CV.WEB_MarcarCVPostulacion()", ex);
              nmdEx.SetValue("Step", strStep);
              throw nmdEx;
          }

          NomadLog.Debug("WEB_MarcarCVPostulacion: " + xmlResult.ToString());
          return xmlResult;

      }

      /// <summary>
      /// Cambia el estado del l_leido del CV en una postulacion por el estado parametro.
      /// Es independiente al l_leido del CV
      /// </summary>
      /// <param name="pParam"></param>
      /// <returns></returns>
      public static NomadXML WEB_Leido_NoLeidoOL(NomadXML pParam)
      {
          NomadLog.Debug("-----------------------------------------------");
          NomadLog.Debug("----------WEB_Leido_NoLeidoOL V1------------");
          NomadLog.Debug("-----------------------------------------------");

          NomadLog.Debug("WEB_Leido_NoLeidoOL.pParam: " + pParam.ToString());

          NomadXML xmlResult;
          string strMode;
          string strStep = "";
          NomadTransaction objTran = null;
          strMode = pParam.GetAttr("mode");
          xmlResult = new NomadXML("DATA");
          xmlResult.SetAttr("mode", strMode);
          try
          {
              strStep = "GET-POSTULACION";
              NucleusRH.Base.Postulantes.CV.POSTULACIONES objPostulacion = CV.POSTULACIONES.Get(pParam.GetAttr("oi_postulaciones"));
              strStep = "ACTUALIZO-L-LEIDO";
              objPostulacion.l_leido = pParam.GetAttr("l_leido") == "1" ? true : false;

              strStep = "PERSISTO-POSTULACION";
              objTran = new NomadTransaction();
              objTran.Begin();
              objTran.Save(objPostulacion);
              objTran.Commit();

              xmlResult.SetAttr("result", "1");
          }
          catch (Exception ex)
          {

              if (objTran != null)
              {
                  objTran.Rollback();
                  xmlResult.SetAttr("result", "0");
              }

              NomadException nmdEx = new NomadException("NucleusRH.Base.Postulantes.CV.WEB_Leido_NoLeidoOL()", ex);
              nmdEx.SetValue("Step", strStep);
              throw nmdEx;
          }

          NomadLog.Debug("xmlResultAsociaCVCarpeta: " + xmlResult.ToString());
          return xmlResult;
      }


       /// <summary>
      /// Resuelve los gráficos estadisticos para ofertas laborales
      /// Retorna atributos calculados para cada grafico
      /// </summary>
      /// <param name="pParam"></param>
      /// <returns></returns>
      public static NomadXML WEB_Graphic(NomadXML pParam)
      {
          NomadLog.Debug("-----------------------------------------------");
          NomadLog.Debug("----------WEB_Graphic V1------------");
          NomadLog.Debug("-----------------------------------------------");

          NomadLog.Debug("WEB_Graphic.pParam: " + pParam.ToString());

          NomadXML xmlResult;
          xmlResult = new NomadXML("DATA");
          string strMode;
          strMode = pParam.GetAttr("mode");
          xmlResult = new NomadXML("DATA");
          xmlResult.SetAttr("mode", strMode);

          string strStep = "";

          try
          {
            NomadXML xmlParam = new NomadXML("DATA");
            xmlParam.SetAttr("oi_oferta_lab",pParam.GetAttr("oi_oferta_lab"));

          //Viejas en 0 implica que la fecha de oferta laboral es menor o igual a 7 dias, por lo que para cada dia que tenga datos muestro sus respectivas cantidades
          //Viejas en 1 implica que la fecha de oferta laboral es mayor a 7 dias por lo que se arman 5 intervalos de fechas de postulaciones
          NucleusRH.Base.Postulantes.Ofertas_Laborales.OFERTA_LAB OFertaLabAux = Ofertas_Laborales.OFERTA_LAB.Get(pParam.GetAttr("oi_oferta_lab"));
          int viejas;
          DateTime aux = (DateTime.Now.AddDays(-7)).Date;

          if (OFertaLabAux.f_oferta_lab.Date >= (DateTime.Now.AddDays(-7)).Date)
              viejas = 0;
          else
             viejas = 1;

          
          xmlParam.SetAttr("viejas", viejas);

          
            NomadXML xmlEstadistica = Nomad.NSystem.Base.NomadEnvironment.QueryNomadXML(NucleusRH.Base.Postulantes.Ofertas_Laborales.OFERTA_LAB.Resources.GRAPHIC, xmlParam.ToString());

            strStep = "GRAPH-SEXO";
            NomadXML xmlSexo = xmlEstadistica.FirstChild().FindElement2("SEXO","name","SEXO");

            double dblCantMasculino = xmlSexo.GetAttrDouble("cantMasculino");
            double dblCantFemenino = xmlSexo.GetAttrDouble("cantFemenino");
            double dblCantTotal = dblCantMasculino + dblCantFemenino;

            strStep = "CALCULO-PORC-SEXO";
            double dblPorcMasculino = Math.Round(dblCantMasculino / dblCantTotal * 100);
            double dblPorcFemenino = Math.Round(dblCantFemenino / dblCantTotal * 100);
            if ((dblPorcMasculino + dblPorcFemenino) > 100)
            {
                if (dblPorcMasculino > dblPorcFemenino)
                    dblPorcMasculino = dblPorcMasculino - 1;
                else
                    dblPorcFemenino = dblPorcFemenino - 1;
            }
            if ((dblPorcMasculino + dblPorcFemenino) < 100)
            {
                if (dblPorcMasculino < dblPorcFemenino)
                    dblPorcMasculino = dblPorcMasculino + 1;
                else
                    dblPorcFemenino = dblPorcFemenino + 1;
            }
            NomadXML xmlResSex = xmlResult.AddTailElement("SEXO");
            xmlResSex.SetAttr("name", "RESSEX");

            xmlResSex.SetAttr("porcMasculino", dblPorcMasculino);
            xmlResSex.SetAttr("porcFemenino", dblPorcFemenino);

            strStep = "GRAPH-EDAD";
            NomadXML xmlEdad = xmlEstadistica.FirstChild().FindElement2("EDAD", "name", "EDAD");
            double dblCant1824 = Math.Round(xmlEdad.GetAttrDouble("cant1824"));
            double dblCant2536 = Math.Round(xmlEdad.GetAttrDouble("cant2536"));
            double dblCant3645 = Math.Round(xmlEdad.GetAttrDouble("cant3645"));
            double dblCant46 = Math.Round(xmlEdad.GetAttrDouble("cant46"));

            dblCantTotal = dblCant1824 + dblCant2536 + dblCant3645 + dblCant46;

            strStep = "GRAPH-EDAD-CALCULO-PORC-EDAD";
            double dblPorc1824 = Math.Round(dblCant1824 / dblCantTotal * 100);
            double dblPorc2536 = Math.Round(dblCant2536 / dblCantTotal * 100);
            double dblPorc3645 = Math.Round(dblCant3645 / dblCantTotal * 100);
            double dblPorc46 = Math.Round(dblCant46 / dblCantTotal * 100);

            strStep = "GRAPH-EDAD-AJUSTE-EDAD";
            double dblPorcTotal = dblPorc1824 + dblPorc2536 + dblPorc3645 + dblPorc46;
            //Ajusto mientras sea mayor que 100
            if (dblPorcTotal > 100)
            {
                double dblMayor = dblPorc1824;
                int intCodMayor = 1824;

                if (dblPorc2536 > dblMayor)
                {
                    dblMayor = dblPorc2536;
                    intCodMayor = 2536;
                }
                if (dblPorc3645 > dblMayor)
                {
                    dblMayor = dblPorc3645;
                    intCodMayor = 3645;
                }

                if (dblPorc46 > dblMayor)
                {
                    dblMayor = dblPorc46;
                    intCodMayor = 46;
                }

                //Al mayor le resto uno
                switch (intCodMayor)
                {
                    case 1824: { dblPorc1824 = dblPorc1824 - 1; break; }
                    case 2536: { dblPorc2536 = dblPorc2536 - 1; break; }
                    case 3645: { dblPorc3645 = dblPorc3645 - 1; break; }
                    case 46: { dblPorc46 = dblPorc46 - 1; break; }
                    default: break;
                }

                double dblMenor = dblPorc1824;
                int intCodMenor = 1824;

                if (dblPorc2536 < dblMenor)
                {
                    dblMenor = dblPorc2536;
                    intCodMenor = 2536;
                }
                if (dblPorc3645 < dblMenor)
                {
                    dblMenor = dblPorc3645;
                    intCodMenor = 3645;
                }

                if (dblPorc46 < dblMenor)
                {
                    dblMenor = dblPorc46;
                    intCodMenor = 46;
                }

                //Al mayor le resto uno
                switch (intCodMenor)
                {
                    case 1824: { dblPorc1824 = dblPorc1824 + 1; break; }
                    case 2536: { dblPorc2536 = dblPorc2536 + 1; break; }
                    case 3645: { dblPorc3645 = dblPorc3645 + 1; break; }
                    case 46: { dblPorc46 = dblPorc46 + 1; break; }
                    default: break;
                }

            }
            NomadXML xmlResEdad = xmlResult.AddTailElement("EDAD");
            xmlResEdad.SetAttr("name", "EDAD");

            xmlResEdad.SetAttr("porc1824", dblPorc1824);
            xmlResEdad.SetAttr("porc2536", dblPorc2536);
            xmlResEdad.SetAttr("porc3645", dblPorc3645);
            xmlResEdad.SetAttr("porc46", dblPorc46);

            strStep = "GRAPH-NIVEST";
            NomadXML xmlNivEst = xmlEstadistica.FirstChild().FindElement2("NIVEST", "name", "NIVEST");

            strStep = "CALCULO-TOTAL";
            int cantTotal = 0;
            for (NomadXML row = xmlNivEst.FirstChild(); row != null; row = row.Next())
                cantTotal = cantTotal + row.GetAttrInt("cant");

            strStep = "CALCULO-PORC";
            NomadXML xmlResNivEst = new NomadXML("NIVELESSTUDIOS");
            xmlResNivEst.SetAttr("name", "NIVELESSTUDIOS");

            double acum = 0;
            int id = 1;
            for (NomadXML row = xmlNivEst.FirstChild(); row != null; row = row.Next())
            {
                NomadXML xmlNivest = xmlResNivEst.AddTailElement("NIVEST");
                xmlNivest.SetAttr("label", row.GetAttr("d_nivel_estudio"));
                xmlNivest.SetAttr("value", Math.Round(row.GetAttrDouble("cant") / cantTotal * 100));
                xmlNivest.SetAttr("id", id);
                id++;
                acum = acum + Math.Round(row.GetAttrDouble("cant") / cantTotal * 100);
            }


            if (acum > 100)
            {
                strStep = "AJUSTO-MAYOR";
                double mayor = 0;
                int idMayor = 0;
                for (NomadXML row = xmlResNivEst.FirstChild(); row != null; row = row.Next())
                {
                    if (row.GetAttrDouble("value") > mayor)
                    {
                        mayor = row.GetAttrDouble("value");
                        idMayor = row.GetAttrInt("id");
                    }
                }
                //Le resto uno al que quedo como mayor
                for (NomadXML row = xmlResNivEst.FirstChild(); row != null; row = row.Next())
                {
                    if (row.GetAttrInt("id") == idMayor)
                    {
                        row.SetAttr("value", row.GetAttrDouble("value") - (acum - 100));
                    }
                }

            }
            if (acum < 100)
            {
                strStep = "AJUSTO-MENOR";
                double menor = 100;
                int idMenor = 0;
                for (NomadXML row = xmlResNivEst.FirstChild(); row != null; row = row.Next())
                {
                    if (row.GetAttrDouble("value") < menor)
                    {
                        menor = row.GetAttrDouble("value");
                        idMenor = row.GetAttrInt("id");
                    }
                }
                //Le sumo uno al que quedo como menor
                for (NomadXML row = xmlResNivEst.FirstChild(); row != null; row = row.Next())
                {
                    if (row.GetAttrInt("id") == idMenor)
                    {
                        row.SetAttr("value", row.GetAttrDouble("value") + (100-acum));
                    }
                }
                // xmlResult.AddTailElement(xmlResNivEst);
            }

            //LO asigno reajustado o no
            xmlResult.AddTailElement(xmlResNivEst);

            NomadXML xmlResSalario = new NomadXML("SALARIO");
            xmlResSalario.SetAttr("name", "SALARIO");

            strStep = "GRAPH-SALARIO";
            NomadXML xmlSalario = xmlEstadistica.FirstChild().FindElement2("SALARIO", "name", "SALARIO");
            if (xmlSalario.ChildLength == 0)
            {
                xmlResSalario.SetAttr("NODATA", "1");
            }
            else
            {
                strStep = "FIND-MAXIMO";
                double maximo = 0;
                for (NomadXML row = xmlSalario.FirstChild(); row != null; row = row.Next())
                {
                    if (row.GetAttrDouble("n_remuneracion") > maximo)
                        maximo = row.GetAttrDouble("n_remuneracion");
                }

                strStep = "INTERVALOS";
                //Tamaño de intervalo
                double v0 = Math.Round(maximo / 5);

                //Calculo extremos
                double v1 = v0 + v0;
                double v2 = v1 + v0;
                double v3 = v2 + v0;
                double v4 = v3 + v0;

                //Defino intervalos
                int I1 = 0; //I0 para de 0 a V0.
                int I2 = 0; // I2 para de V0 a V1;
                int I3 = 0; //I3 para de V1 a V2;
                int I4 = 0; // I4 para de V2 a V3
                int I5 = 0; //I5 para de V3 a V4


                strStep = "CLASIFICO-OBSERVACIONES";
                for (NomadXML row = xmlSalario.FirstChild(); row != null; row = row.Next())
                {
                    double dblValor = row.GetAttrDouble("n_remuneracion");

                    if ((dblValor > 0) && (dblValor <= v0)) I1++;

                    if ((dblValor > v0) && (dblValor <= v1)) I2++;

                    if ((dblValor > v1) && (dblValor <= v2)) I3++;

                    if ((dblValor > v2) && (dblValor <= v3)) I4++;

                    if ((dblValor > v3)) I5++;

                }

               
                xmlResSalario.SetAttr("I1", I1);
                xmlResSalario.SetAttr("label1", "De 0 a " + v0.ToString("n0"));
                xmlResSalario.SetAttr("I2", I2);
                xmlResSalario.SetAttr("label2", "De " + v0.ToString("n0") + " a " + v1.ToString("0"));
                xmlResSalario.SetAttr("I3", I3);
                xmlResSalario.SetAttr("label3", "De " + v1.ToString("n0") + " a " + v2.ToString("n0"));
                xmlResSalario.SetAttr("I4", I4);
                xmlResSalario.SetAttr("label4", "De " + v2.ToString("n0") + " a " + v3.ToString("n0"));
                xmlResSalario.SetAttr("I5", I5);
                xmlResSalario.SetAttr("label5", "Mayor a " + v3.ToString("n0"));
          }

            xmlResult.AddTailElement(xmlResSalario);


            strStep = "GRAPH-POSTULACIONES";
            NomadXML xmlPostu = xmlEstadistica.FirstChild().FindElement2("POSTULACIONES", "name", "POSTULACIONES");
            strStep = "FIND-MAXIMO-MINIMO";

            //Viejas en 0 implica que la fecha de oferta laboral es menor o igual a 7 dias, por lo que para cada dia que tenga datos muestro sus respectivas cantidades
            //Viejas en 1 implica que la fecha de oferta laboral es mayor a 7 dias por lo que se arman 5 intervalos de fechas de postulaciones
            NomadXML xmlResPostu = new NomadXML("POSTULACIONES");
            xmlResPostu.SetAttr("name", "POSTULACIONES");


            NomadLog.Debug("VIEJAS: " + viejas.ToString());
            if (viejas == 0)
            {
                xmlResPostu.SetAttr("viejas", "0");


                int i = 1;
                for (NomadXML row = xmlPostu.FirstChild(); row != null; row = row.Next())
                {
                    xmlResPostu.SetAttr("label"+i.ToString(),row.GetAttr("f_postulacion"));
                    xmlResPostu.SetAttr("I"+i.ToString(),row.GetAttr("cant"));
                    i++;
                }
            }
            else
            {

                DateTime fechaMaxima = xmlPostu.FirstChild().GetAttrDateTime("f_postulacion");
                DateTime fechaMinima = xmlPostu.FirstChild().GetAttrDateTime("f_postulacion");
                int cantidadPostu = 0;
                for (NomadXML row = xmlPostu.FirstChild(); row != null; row = row.Next())
                {
                    if (row.GetAttrDateTime("f_postulacion") > fechaMaxima)
                    {
                        fechaMaxima = row.GetAttrDateTime("f_postulacion");
                    }

                    if (row.GetAttrDateTime("f_postulacion") < fechaMinima)
                    {
                        fechaMinima = row.GetAttrDateTime("f_postulacion");
                    }

                    cantidadPostu++;
                }

                double dateDiff = Math.Round(((fechaMaxima.Subtract(fechaMinima)).TotalDays) / 5);
                if (dateDiff > 0)
                {
                    //Calculo extremos
                    DateTime d0 = fechaMinima;
                    DateTime d1 = d0.AddDays(dateDiff);
                    DateTime d2 = d1.AddDays(dateDiff);
                    DateTime d3 = d2.AddDays(dateDiff);
                    DateTime d4 = d3.AddDays(dateDiff);
                    DateTime d5 = fechaMaxima;


                    //Defino intervalos
                    int I1 = 0; //I0 para de d0 a d1.
                    int I2 = 0; // I2 para de d1 a d2;
                    int I3 = 0; //I3 para de d2 a d3;
                    int I4 = 0; // I4 para de d3 a d4
                    int I5 = 0; //I5 para de d4 a  d5

                    //Recorro y clasifico
                    for (NomadXML row = xmlPostu.FirstChild(); row != null; row = row.Next())
                    {
                        if ((row.GetAttrDateTime("f_postulacion") >= d0) && (row.GetAttrDateTime("f_postulacion") <= d1)) I1++;
                        if ((row.GetAttrDateTime("f_postulacion") > d1) && (row.GetAttrDateTime("f_postulacion") <= d2)) I2++;
                        if ((row.GetAttrDateTime("f_postulacion") > d2) && (row.GetAttrDateTime("f_postulacion") <= d3)) I3++;
                        if ((row.GetAttrDateTime("f_postulacion") > d3) && (row.GetAttrDateTime("f_postulacion") <= d4)) I4++;
                        if ((row.GetAttrDateTime("f_postulacion") > d4) && (row.GetAttrDateTime("f_postulacion") <= d5)) I5++;
                    }

                    xmlResPostu.SetAttr("viejas", "1");
                    xmlResPostu.SetAttr("I1", I1);
                    xmlResPostu.SetAttr("label1", "De  " + d0.ToShortDateString() + " a " + d1.ToShortDateString());
                    xmlResPostu.SetAttr("I2", I2);
                    xmlResPostu.SetAttr("label2", "De " + d1.ToShortDateString() + " a " + d2.ToShortDateString());
                    xmlResPostu.SetAttr("I3", I3);
                    xmlResPostu.SetAttr("label3", "De " + d2.ToShortDateString() + " a " + d3.ToShortDateString());
                    xmlResPostu.SetAttr("I4", I4);
                    xmlResPostu.SetAttr("label4", "De " + d3.ToShortDateString() + " a " + d4.ToShortDateString());
                    xmlResPostu.SetAttr("I5", I5);
                    xmlResPostu.SetAttr("label5", "De " + d4.ToShortDateString() + " a " + d5.ToShortDateString());

                }
                else
                {
                    xmlResPostu.SetAttr("viejas", "1");
                    xmlResPostu.SetAttr("I1", xmlPostu.ChildLength);
                    xmlResPostu.SetAttr("label1", "De  " + fechaMinima.ToShortDateString() + " a " + fechaMaxima.ToShortDateString());
                }

            }
            xmlResult.AddTailElement(xmlResPostu);

            strStep = "ORIGENES-AVISO";
            NomadXML xmlOrigenesAviso = xmlEstadistica.FirstChild().FindElement2("ORIGENES-AVISO", "name", "ORIGENES-AVISO");

             
            double cantTotalOrigenes = xmlOrigenesAviso.GetAttrDouble("cantTotal");
            NomadXML xmlResOrigenes = new NomadXML("ORIGENES-AVISO");
            xmlResOrigenes.SetAttr("name", "ORIGENES-AVISO");
            int sumadorOrigenes = 0;
            int idOrigen = 1;

            //Recorro los origenes
            for (NomadXML row = xmlOrigenesAviso.FirstChild(); row != null; row = row.Next())
            {
                NomadXML xmlResOrig = xmlResOrigenes.AddTailElement("ORIGEN-AVISO");
                xmlResOrig.SetAttr("label", row.GetAttr("d_origen_aviso"));
                xmlResOrig.SetAttr("value", Math.Round(row.GetAttrDouble("cant") / cantTotalOrigenes * 100));

                xmlResOrig.SetAttr("idOrigen", idOrigen);
                idOrigen++;
                sumadorOrigenes = sumadorOrigenes + xmlResOrig.GetAttrInt("value");
            }

            strStep = "AJUSTO-ORIGEN";
            if (sumadorOrigenes > 100)
            {
                double mayorOrigen = 0;
                int idMayorOrigen = 0;
                for (NomadXML row = xmlResOrigenes.FirstChild(); row != null; row = row.Next())
                {
                    if (row.GetAttrDouble("value") > mayorOrigen)
                    {
                        mayorOrigen = row.GetAttrDouble("value");
                        idMayorOrigen = row.GetAttrInt("idOrigen");
                    }
                }

                //Le resto uno al que quedo como mayor
                for (NomadXML row = xmlResOrigenes.FirstChild(); row != null; row = row.Next())
                {
                    if (row.GetAttrInt("idOrigen") == idMayorOrigen)
                    {
                        row.SetAttr("value", row.GetAttrDouble("value") - (sumadorOrigenes - 100));
                        break;
                    }
                }
            }

            if (sumadorOrigenes < 100)
            {
                double menorOrigen = 101;
                int idMenorOrigen = 0;
                for (NomadXML row = xmlResOrigenes.FirstChild(); row != null; row = row.Next())
                {
                    if (row.GetAttrDouble("value") < menorOrigen)
                    {
                        menorOrigen = row.GetAttrDouble("value");
                        idMenorOrigen = row.GetAttrInt("idOrigen");
                    }
                }

                //Le sumo  uno al que quedo como mayor
                for (NomadXML row = xmlResOrigenes.FirstChild(); row != null; row = row.Next())
                {
                    if (row.GetAttrInt("idOrigen") == idMenorOrigen)
                    {
                        row.SetAttr("value", row.GetAttrDouble("value") + (100 - sumadorOrigenes));
                        break;
                    }
                }
            }

            //Agrego origenes
            xmlResult.AddTailElement(xmlResOrigenes);


            strStep = "RESPUESTAS-CHOICE";
            NomadXML xmlRespuestasChoice = xmlEstadistica.FirstChild().FindElement2("RESPUESTAS-CHOICE", "name", "RESPUESTAS-CHOICE");
            NomadXML xmlResChoice = new NomadXML("RESPUESTAS-CHOICE");
            xmlResChoice.SetAttr("name", "RESPUESTAS-CHOICE");

            //Recorro las respuestas para cada grafico
            for (NomadXML row = xmlRespuestasChoice.FirstChild(); row != null; row = row.Next())
            {
                NomadXML xmlRespuesta = xmlResChoice.AddTailElement("RESPUESTA");
                xmlRespuesta.SetAttr("name", "RESPUESTA");
                xmlRespuesta.SetAttr("d_pregunta", row.GetAttr("d_pregunta"));

                int sumadorRespuesta = 0;
                int idRespuesta = 1;

                double cantTotalRes = row.GetAttrDouble("cantTotalResp");
                for (NomadXML rowOpcion = row.FirstChild(); rowOpcion != null; rowOpcion = rowOpcion.Next())
                {
                    NomadXML xmlRespuestaOpciones = xmlRespuesta.AddTailElement("RESP-OPCION");

                    xmlRespuestaOpciones.SetAttr("label",rowOpcion.GetAttr("d_pregunta_op"));
                    xmlRespuestaOpciones.SetAttr("l_correcta", rowOpcion.GetAttr("l_correcta"));
                    xmlRespuestaOpciones.SetAttr("value", Math.Round(rowOpcion.GetAttrDouble("cantResp")/cantTotalRes*100));

                    //Para el ajuste
                    xmlRespuestaOpciones.SetAttr("idRespuesta", idRespuesta);
                    idRespuesta++;
                    sumadorRespuesta = sumadorRespuesta + xmlRespuestaOpciones.GetAttrInt("value");
                }
                strStep = "AJUSTO-RESPUESTA";

            
               
                if (sumadorRespuesta > 100)
                {
                    int idMayorResp = 0;
                    double mayorResp = 0;

                    //Busco el mayor y le resto 1
                    for (NomadXML rowAjusto = xmlRespuesta.FirstChild(); rowAjusto != null; rowAjusto = rowAjusto.Next())
                    {
                        if (rowAjusto.GetAttrDouble("value") > mayorResp)
                        {
                            idMayorResp = rowAjusto.GetAttrInt("idRespuesta");
                            mayorResp = rowAjusto.GetAttrDouble("value");
                        }
                    }

                    //Le resto la mayor
                    for (NomadXML rowAjusto = xmlRespuesta.FirstChild(); rowAjusto != null; rowAjusto = rowAjusto.Next())
                    {
                        if (rowAjusto.GetAttrInt("idRespuesta") == idMayorResp)
                        {
                            rowAjusto.SetAttr("value", rowAjusto.GetAttrDouble("value") - (sumadorRespuesta - 100));
                            break;
                        }
                    }

                }
                if (sumadorRespuesta < 100)
                {
                    int idMenorResp = 101;
                    double menorResp = 0;

                    //Busco el mayor y le resto 1
                    for (NomadXML rowAjusto = xmlRespuesta.FirstChild(); rowAjusto != null; rowAjusto = rowAjusto.Next())
                    {
                        if ((rowAjusto.GetAttrDouble("value") < idMenorResp) && (rowAjusto.GetAttrDouble("value")!=0))
                        {
                            idMenorResp = rowAjusto.GetAttrInt("idRespuesta");
                            menorResp = rowAjusto.GetAttrDouble("value");
                        }
                    }

                    //Le resto la mayor
                    for (NomadXML rowAjusto = xmlRespuesta.FirstChild(); rowAjusto != null; rowAjusto = rowAjusto.Next())
                    {
                        if (rowAjusto.GetAttrInt("idRespuesta") == idMenorResp)
                        {
                            rowAjusto.SetAttr("value", rowAjusto.GetAttrDouble("value") + (100 - sumadorRespuesta));
                            break;
                        }
                    }

                }
            }


            //Agrego los resultados
            xmlResult.AddTailElement(xmlResChoice);

            
          
          }
          catch (Exception ex)
          {
              xmlResult.SetAttr("resultOk", "0");
              NomadException nmdEx = new NomadException("NucleusRH.Base.Postulantes.Ofertas_Laborales.WEB_Graphic()", ex);
              nmdEx.SetValue("Step", strStep);
              throw nmdEx;
          }

          NomadLog.Debug("xmlResultWEB_Graphic: " + xmlResult.ToString());
          return xmlResult;
      }


      /// <summary>
      /// Persiste la Oferta Laboral
      /// </summary>
      /// <param name="pParam"></param>
      /// <returns></returns>
      public static NomadXML WEB_SaveOfertaLab(NomadXML pParam)
      {
          NomadLog.Debug("-----------------------------------------------");
          NomadLog.Debug("----------WEB_SaveOfertaLab V1------------");
          NomadLog.Debug("-----------------------------------------------");

          NomadLog.Debug("WEB_SaveOfertaLab.pParam: " + pParam.ToString());

          NomadXML xmlResult;
          string strMode;
          string strStep = "";
          NomadTransaction objTran = null;
          strMode = pParam.GetAttr("mode");
          xmlResult = new NomadXML("DATA");
          xmlResult.SetAttr("mode", strMode);
          try
          {
              NucleusRH.Base.Postulantes.Ofertas_Laborales.OFERTA_LAB objOfertaLab;
              if (pParam.GetAttr("oi_oferta_lab") == "")
              {

                  strStep = "NEW-OL";
                  objOfertaLab = new OFERTA_LAB();
                  objOfertaLab.f_oferta_lab = DateTime.Now.Date;
              }
              else
              {
                  strStep = "EDIT-OL";
                  objOfertaLab = Ofertas_Laborales.OFERTA_LAB.Get(pParam.GetAttr("oi_oferta_lab"));
              }

               //Atributos requeridos
              objOfertaLab.d_oferta_lab = pParam.GetAttr("d_oferta_lab");
              objOfertaLab.o_oferta_lab = pParam.GetAttr("o_oferta_lab");

              
              //Siempre se publica en la web
              objOfertaLab.l_publicacion_web = true;

              //atributos no requeridos
              if (pParam.GetAttr("f_cierre") != "")
                  objOfertaLab.f_cierre = pParam.GetAttrDateTime("f_cierre");
              else
                  objOfertaLab.f_cierreNull = true;

              if (pParam.GetAttr("e_cantidad") != "")
                  objOfertaLab.e_cantidad = pParam.GetAttrInt("e_cantidad");
              else
                  objOfertaLab.e_cantidadNull = true;

              if (pParam.GetAttr("oi_empresa") != "")
                  objOfertaLab.oi_empresa = pParam.GetAttr("oi_empresa");
              else
                  objOfertaLab.oi_empresaNull = true;

              if (pParam.GetAttr("oi_tipos_serv") != "")
                  objOfertaLab.oi_tipos_serv = pParam.GetAttr("oi_tipos_serv");
              else
                  objOfertaLab.oi_tipos_servNull = true;

              if (pParam.GetAttr("oi_tipo_puesto") != "")
                  objOfertaLab.oi_tipo_puesto = pParam.GetAttr("oi_tipo_puesto");
              else
                  objOfertaLab.oi_tipo_puestoNull = true;

              if (pParam.GetAttr("oi_seniority") != "")
                  objOfertaLab.oi_seniority = pParam.GetAttr("oi_seniority");
              else
                  objOfertaLab.oi_seniorityNull = true;

              if (pParam.GetAttr("oi_area_lab") != "")
                  objOfertaLab.oi_area_lab = pParam.GetAttr("oi_area_lab");
              else
                  objOfertaLab.oi_area_labNull = true;

              if (pParam.GetAttr("oi_pais") != "")
                  objOfertaLab.oi_pais = pParam.GetAttr("oi_pais");
              else
                  objOfertaLab.oi_paisNull = true;

              if (pParam.GetAttr("oi_provincia") != "")
                  objOfertaLab.oi_provincia = pParam.GetAttr("oi_provincia");
              else
                  objOfertaLab.oi_provinciaNull = true;

              if (pParam.GetAttr("oi_localidad") != "")
                  objOfertaLab.oi_localidad = pParam.GetAttr("oi_localidad");
              else
                  objOfertaLab.oi_localidadNull = true;

              if (pParam.GetAttr("oi_personal") != "")
                  objOfertaLab.oi_personal = pParam.GetAttr("oi_personal");
              else
                  objOfertaLab.oi_personalNull = true;

              if (pParam.GetAttr("d_mail") != "")
                  objOfertaLab.d_mail = pParam.GetAttr("d_mail");
              else
                  objOfertaLab.d_mailNull = true;

              if (pParam.GetAttr("d_mail_alt_1") != "")
                  objOfertaLab.d_mail_alt_1 = pParam.GetAttr("d_mail_alt_1");
              else
                  objOfertaLab.d_mail_alt_1Null = true;

              if (pParam.GetAttr("d_mail_alt_2") != "")
                  objOfertaLab.d_mail_alt_2 = pParam.GetAttr("d_mail_alt_2");
              else
                  objOfertaLab.d_mail_alt_2Null = true;

              strStep = "REQUISITOS";
              //Requisito residencia
              if (pParam.GetAttr("oi_pais_res") != "")
                  objOfertaLab.oi_pais_res = pParam.GetAttr("oi_pais_res");
              else
                  objOfertaLab.oi_pais_resNull = true;

              if (pParam.GetAttr("oi_provincia_res") != "")
                  objOfertaLab.oi_provincia_res = pParam.GetAttr("oi_provincia_res");
              else
                  objOfertaLab.oi_provincia_resNull = true;

              if (pParam.GetAttr("oi_localidad_res") != "")
                  objOfertaLab.oi_localidad_res = pParam.GetAttr("oi_localidad_res");
              else
                  objOfertaLab.oi_localidad_resNull = true;

              objOfertaLab.l_exc_res = pParam.GetAttr("l_exc_res")=="1"?true:false;
              objOfertaLab.l_mostrar_res = pParam.GetAttr("l_mostrar_res") == "1" ? true : false;

              //Requisito experiencia
              if (pParam.GetAttr("e_exp_des") != "")
                  objOfertaLab.e_exp_des = pParam.GetAttrInt("e_exp_des");
              else
                  objOfertaLab.e_exp_desNull = true;

              if (pParam.GetAttr("oi_uni_tpo_desde") != "")
                  objOfertaLab.oi_uni_tpo_desde = pParam.GetAttr("oi_uni_tpo_desde");
              else
                  objOfertaLab.oi_uni_tpo_desdeNull = true;

              if (pParam.GetAttr("e_exp_hasta") != "")
                  objOfertaLab.e_exp_hasta = pParam.GetAttrInt("e_exp_hasta");
              else
                  objOfertaLab.e_exp_hastaNull = true;

              if (pParam.GetAttr("oi_uni_tpo_hasta") != "")
                  objOfertaLab.oi_uni_tpo_hasta = pParam.GetAttr("oi_uni_tpo_hasta");
              else
                  objOfertaLab.oi_uni_tpo_hastaNull = true;

              objOfertaLab.l_exc_exp = pParam.GetAttr("l_exc_exp")=="1"?true:false;
              objOfertaLab.l_mostrar_exp = pParam.GetAttr("l_mostrar_exp") == "1" ? true : false;

              //Requisitos edad
              if (pParam.GetAttr("e_edad_desde") != "")
                  objOfertaLab.e_edad_desde = pParam.GetAttrInt("e_edad_desde");
              else
                  objOfertaLab.e_edad_desdeNull = true;

              if (pParam.GetAttr("e_edad_hasta") != "")
                  objOfertaLab.e_edad_hasta = pParam.GetAttrInt("e_edad_hasta");
              else
                  objOfertaLab.e_edad_hastaNull = true;

              objOfertaLab.l_exc_edad = pParam.GetAttr("l_exc_edad") == "1" ? true : false;
              objOfertaLab.l_mostrar_edad = pParam.GetAttr("l_mostrar_edad") == "1" ? true : false;

              //Requisitos Sexo
              if (pParam.GetAttr("c_sexo") != "")
                  objOfertaLab.c_sexo = pParam.GetAttr("c_sexo");
              else
                  objOfertaLab.c_sexoNull = true;

              objOfertaLab.l_exc_sexo = pParam.GetAttr("l_exc_sexo") == "1" ? true : false;
              objOfertaLab.l_mostrar_sexo = pParam.GetAttr("l_mostrar_sexo") == "1" ? true : false;

              //Requisito Estado Civil
              if (pParam.GetAttr("oi_estado_civil") != "")
                  objOfertaLab.oi_estado_civil = pParam.GetAttr("oi_estado_civil");
              else
                  objOfertaLab.oi_estado_civilNull = true;

              objOfertaLab.l_exc_ecivil = pParam.GetAttr("l_exc_ecivil") == "1" ? true : false;
              objOfertaLab.l_mostrar_ecivil = pParam.GetAttr("l_mostrar_ecivil") == "1" ? true : false;

              //Requisito Educacion
              if (pParam.GetAttr("oi_nivel_estudio") != "")
                  objOfertaLab.oi_nivel_estudio = pParam.GetAttr("oi_nivel_estudio");
              else
                  objOfertaLab.oi_nivel_estudioNull = true;

              if ((pParam.GetAttr("oi_nivel_estudio") != "") && (pParam.GetAttr("oi_estado_est") != ""))
                  objOfertaLab.oi_estado_est = pParam.GetAttr("oi_estado_est");
              else
                  objOfertaLab.oi_estado_estNull = true;

              objOfertaLab.l_exc_edu = pParam.GetAttr("l_exc_edu") == "1" ? true : false;
              objOfertaLab.l_mostrar_edu = pParam.GetAttr("l_mostrar_edu") == "1" ? true : false;

              //Requisito Idioma
              if (pParam.GetAttr("oi_nivel_ing") != "")
                  objOfertaLab.oi_nivel_ing = pParam.GetAttr("oi_nivel_ing");
              else
                  objOfertaLab.oi_nivel_ingNull = true;

              if (pParam.GetAttr("oi_nivel_por") != "")
                  objOfertaLab.oi_nivel_por = pParam.GetAttr("oi_nivel_por");
              else
                  objOfertaLab.oi_nivel_porNull = true;

              if (pParam.GetAttr("oi_nivel_fr") != "")
                  objOfertaLab.oi_nivel_fr = pParam.GetAttr("oi_nivel_fr");
              else
                  objOfertaLab.oi_nivel_frNull = true;

              if (pParam.GetAttr("oi_nivel_it") != "")
                  objOfertaLab.oi_nivel_it = pParam.GetAttr("oi_nivel_it");
              else
                  objOfertaLab.oi_nivel_itNull = true;

              if (pParam.GetAttr("oi_nivel_al") != "")
                  objOfertaLab.oi_nivel_al = pParam.GetAttr("oi_nivel_al");
              else
                  objOfertaLab.oi_nivel_alNull = true;

              if (pParam.GetAttr("oi_nivel_jap") != "")
                  objOfertaLab.oi_nivel_jap = pParam.GetAttr("oi_nivel_jap");
              else
                  objOfertaLab.oi_nivel_japNull = true;

              objOfertaLab.l_exc_idioma = pParam.GetAttr("l_exc_idioma") == "1" ? true : false;
              objOfertaLab.l_mostrar_idioma = pParam.GetAttr("l_mostrar_idioma") == "1" ? true : false;

              //Requisito salario
              if (pParam.GetAttr("n_salario_d") != "")
                  objOfertaLab.n_salario_d = pParam.GetAttrDouble("n_salario_d");
              else
                  objOfertaLab.n_salario_dNull = true;

              if (pParam.GetAttr("n_salario_h") != "")
                  objOfertaLab.n_salario_h = pParam.GetAttrDouble("n_salario_h");
              else
                  objOfertaLab.n_salario_hNull = true;

              objOfertaLab.l_exc_salario = pParam.GetAttr("l_exc_salario") == "1" ? true : false;
              objOfertaLab.l_mostrar_en_aviso = pParam.GetAttr("l_mostrar_en_aviso") == "1" ? true : false;
              objOfertaLab.l_solicita_sal = pParam.GetAttr("l_solicita_sal") == "1" ? true : false;

              strStep = "PREGUNTAS";
              if (pParam.GetAttr("oi_oferta_lab") == "")
              {
                  strStep = "PREGUNTAS-NUEVA-OL";
                  for (NomadXML row = pParam.FindElement2("PREGUNTAS", "name", "PREGUNTAS").FirstChild(); row != null; row = row.Next())
                  {
                      NucleusRH.Base.Postulantes.Ofertas_Laborales.PREGUNTAS_OL objPregunta = new Ofertas_Laborales.PREGUNTAS_OL();
                      //Si es pregunta abierta
                      if (row.GetAttr("c_pregunta") == "A")
                      {
                          objPregunta.d_pregunta = row.GetAttr("d_pregunta");
                          objPregunta.c_pregunta = row.GetAttr("c_pregunta");
                      }
                      //Si es pregunta Multiple
                      if (row.GetAttr("c_pregunta") == "M")
                      {
                          objPregunta.d_pregunta = row.GetAttr("d_pregunta");
                          objPregunta.c_pregunta = row.GetAttr("c_pregunta");

                          //Para cada opcion
                          for (NomadXML rowOpt = row.FirstChild(); rowOpt != null; rowOpt = rowOpt.Next())
                          {
                              NucleusRH.Base.Postulantes.Ofertas_Laborales.OPCIONES_PREG objOpciones = new Ofertas_Laborales.OPCIONES_PREG();
                              objOpciones.d_pregunta_op = rowOpt.GetAttr("d_pregunta_op");
                              objOpciones.l_correcta = rowOpt.GetAttr("l_correcta") == "1" ? true : false;

                              objPregunta.OPCION_PREG.Add(objOpciones);
                          }
                      }
                      objOfertaLab.PREG_OF_LAB.Add(objPregunta);
                  }
              }
              else
              {
                  strStep = "PREGUNTAS-EDIT-OL";
                  NomadXML xmlParam = new NomadXML("DATA");
                  xmlParam.SetAttr("oi_oferta_lab", pParam.GetAttr("oi_oferta_lab"));
                  NomadXML xmlTienePostulaciones = Nomad.NSystem.Base.NomadEnvironment.QueryNomadXML(NucleusRH.Base.Postulantes.Ofertas_Laborales.OFERTA_LAB.Resources.TIENE_POSTU, xmlParam.ToString());

                  //Si no tiene postulaciones
                  if (xmlTienePostulaciones.FirstChild().GetAttr("tiene") == "0")
                  {
                      strStep = "PREG-EDIT-NO-TIENE-POSTU-ACTUALIZA-EXISTENTES";
                      //Primer actualizo las existentes
                      for (NomadXML row = pParam.FindElement2("PREGUNTAS", "name", "PREGUNTAS").FirstChild(); row != null; row = row.Next())
                      {
                          foreach (NucleusRH.Base.Postulantes.Ofertas_Laborales.PREGUNTAS_OL objPregunta in objOfertaLab.PREG_OF_LAB)
                          {
                              if (row.GetAttr("id") == objPregunta.Id)
                              {
                                  objPregunta.c_pregunta = row.GetAttr("c_pregunta");
                                  objPregunta.d_pregunta = row.GetAttr("d_pregunta");

                                  //Si es choice, quito todas y agrego las nuevas
                                  if (row.GetAttr("c_pregunta") == "M")
                                  {
                                      objPregunta.OPCION_PREG.Clear();
                                      for (NomadXML rowOpt = row.FirstChild(); rowOpt != null; rowOpt = rowOpt.Next())
                                      {
                                          NucleusRH.Base.Postulantes.Ofertas_Laborales.OPCIONES_PREG objOpciones = new Ofertas_Laborales.OPCIONES_PREG();
                                          objOpciones.d_pregunta_op = rowOpt.GetAttr("d_pregunta_op");
                                          objOpciones.l_correcta = rowOpt.GetAttr("l_correcta") == "1" ? true : false;

                                          objPregunta.OPCION_PREG.Add(objOpciones);
                                      }
                                  }
                                  break;
                              }

                          }
                      }

                      //Segundo elimino las que no estan
                      strStep = "PREG-EDIT-NO-TIENE-POSTU-ELIMINA-NO-ESTAN";
                      ArrayList idsTodel = new ArrayList();
                      foreach (NucleusRH.Base.Postulantes.Ofertas_Laborales.PREGUNTAS_OL objPregunta in objOfertaLab.PREG_OF_LAB)
                      {
                          bool encontro = false;
                          for (NomadXML row = pParam.FindElement2("PREGUNTAS", "name", "PREGUNTAS").FirstChild(); row != null; row = row.Next())
                          {
                              //La encontro
                              if (objPregunta.Id == row.GetAttr("id"))
                              {
                                  encontro = true;
                                  break;
                              }
                          }
                          if (!encontro)
                          {
                              idsTodel.Add(objPregunta.id);
                          }
                      }
                      //Elimino los ids a eliminar
                      for (int i = 0; i < idsTodel.Count; i++)
                      {
                          string id = idsTodel[i].ToString();
                          objOfertaLab.PREG_OF_LAB.RemoveById(id);

                      }

                      //Tercero agrego las adicionales
                      strStep = "PREG-EDIT-NO-TIENE-POSTU-AGREGO-ADICIONALES";
                      for (NomadXML row = pParam.FindElement2("PREGUNTAS", "name", "PREGUNTAS").FirstChild(); row != null; row = row.Next())
                      {
                          bool encontro = false;
                          foreach (NucleusRH.Base.Postulantes.Ofertas_Laborales.PREGUNTAS_OL objPregunta in objOfertaLab.PREG_OF_LAB)
                          {
                              //La encontro
                              if (objPregunta.Id == row.GetAttr("id"))
                              {
                                  encontro = true;
                                  break;
                              }
                          }
                          //Si no la encontro, agrego el row como pregunta
                          if (!encontro)
                          {
                              NucleusRH.Base.Postulantes.Ofertas_Laborales.PREGUNTAS_OL objPregunta = new Ofertas_Laborales.PREGUNTAS_OL();
                              //Si es pregunta abierta
                              if (row.GetAttr("c_pregunta") == "A")
                              {
                                  objPregunta.d_pregunta = row.GetAttr("d_pregunta");
                                  objPregunta.c_pregunta = row.GetAttr("c_pregunta");
                              }
                              //Si es pregunta Multiple
                              if (row.GetAttr("c_pregunta") == "M")
                              {
                                  objPregunta.d_pregunta = row.GetAttr("d_pregunta");
                                  objPregunta.c_pregunta = row.GetAttr("c_pregunta");

                                  //Para cada opcion
                                  for (NomadXML rowOpt = row.FirstChild(); rowOpt != null; rowOpt = rowOpt.Next())
                                  {
                                      NucleusRH.Base.Postulantes.Ofertas_Laborales.OPCIONES_PREG objOpciones = new Ofertas_Laborales.OPCIONES_PREG();
                                      objOpciones.d_pregunta_op = rowOpt.GetAttr("d_pregunta_op");
                                      objOpciones.l_correcta = rowOpt.GetAttr("l_correcta") == "1" ? true : false;

                                      objPregunta.OPCION_PREG.Add(objOpciones);
                                  }
                              }
                              objOfertaLab.PREG_OF_LAB.Add(objPregunta);
                          }
                      }
                  }
                  else
                  {
                      //Si  tiene postulaciones
                      strStep = "PREG-EDIT-TIENE-POSTU-ELIMINA-NO-ESTAN";
                      //Recorro las que tengo en BD y si alguna no esta en el parametro, la elimino
                      ArrayList idsTodel = new ArrayList();
                      foreach (NucleusRH.Base.Postulantes.Ofertas_Laborales.PREGUNTAS_OL objPregunta in objOfertaLab.PREG_OF_LAB)
                      {
                          bool encontro = false;
                          for (NomadXML row = pParam.FindElement2("PREGUNTAS", "name", "PREGUNTAS").FirstChild(); row != null; row = row.Next())
                          {
                              if (objPregunta.Id == row.GetAttr("id"))
                              {
                                  encontro = true;
                                  break;
                              }
                          }
                          if (!encontro)
                              idsTodel.Add(objPregunta.Id);

                      }
                      //Elimino las respuestas a preguntas y luegos las preguntas
                      strStep = "PREG-EDIT-TIENE-POSTU-ELIMINA-RESPUESTAS";
                      foreach (string idPreg in idsTodel)
                      {
                          NomadXML xmlParamDel = new NomadXML("DATA");
                          xmlParam.SetAttr("oi_pregunta", idPreg);


                          NomadXML xmlRespuestasToDel = Nomad.NSystem.Base.NomadEnvironment.QueryNomadXML(NucleusRH.Base.Postulantes.Ofertas_Laborales.OFERTA_LAB.Resources.GET_RESP_PREG, xmlParam.ToString());


                          //Primero elimino cada las respuestas
                          for (NomadXML row = xmlRespuestasToDel.FirstChild().FirstChild(); row != null; row = row.Next())
                          {

                              NomadLog.Debug("row "+row.ToString());

                              NomadLog.Debug("1");

                              string idResp = row.GetAttr("oi_postu_resp");

                              NomadLog.Debug("2");

                              NomadLog.Debug("idResp: " + idResp);

                              NucleusRH.Base.Postulantes.CV.POSTU_RESPUESTA objRespToDel = NucleusRH.Base.Postulantes.CV.POSTU_RESPUESTA.Get(idResp);

                              NomadLog.Debug("3");

                              NomadLog.Debug("objRespToDel: " + Nomad.NSystem.Functions.StringUtil.Object2JSON(objRespToDel));

                              NomadLog.Debug("4");

                              NomadTransaction objtrans2 = new NomadTransaction();

                              NomadLog.Debug("5");

                              objtrans2.Begin();

                              NomadLog.Debug("6");

                              objtrans2.Delete(objRespToDel);

                              NomadLog.Debug("7");

                              objtrans2.Commit();

                              NomadLog.Debug("8");

                          }

                          //Elimino la pregunta
                         // NucleusRH.Base.Postulantes.Ofertas_Laborales.PREGUNTAS_OL objPregToDel = NucleusRH.Base.Postulantes.Ofertas_Laborales.PREGUNTAS_OL.Get(idPreg);
                          objOfertaLab.PREG_OF_LAB.RemoveById(idPreg);

                      }

                      //Compara las preguntas parametro con las preguntas existentes, si no encuentra una la agrega
                      strStep = "PREG-EDIT-TIENE-POSTU-AGREGA-PREG";
                      for (NomadXML row = pParam.FindElement2("PREGUNTAS", "name", "PREGUNTAS").FirstChild(); row != null; row = row.Next())
                      {

                          bool encontro = false;
                          foreach (NucleusRH.Base.Postulantes.Ofertas_Laborales.PREGUNTAS_OL objPregunta in objOfertaLab.PREG_OF_LAB)
                          {
                              if (row.GetAttr("id") == objPregunta.Id)
                              {
                                  encontro = true;
                                  break;
                              }
                          }

                          if (!encontro)
                          {
                              NucleusRH.Base.Postulantes.Ofertas_Laborales.PREGUNTAS_OL objPregunta = new PREGUNTAS_OL();

                              //Si es pregunta abierta
                              if (row.GetAttr("c_pregunta") == "A")
                              {
                                  objPregunta.d_pregunta = row.GetAttr("d_pregunta");
                                  objPregunta.c_pregunta = row.GetAttr("c_pregunta");
                              }
                              //Si es pregunta Multiple
                              if (row.GetAttr("c_pregunta") == "M")
                              {
                                  objPregunta.d_pregunta = row.GetAttr("d_pregunta");
                                  objPregunta.c_pregunta = row.GetAttr("c_pregunta");

                                  //Para cada opcion
                                  for (NomadXML rowOpt = row.FirstChild(); rowOpt != null; rowOpt = rowOpt.Next())
                                  {
                                      NucleusRH.Base.Postulantes.Ofertas_Laborales.OPCIONES_PREG objOpciones = new Ofertas_Laborales.OPCIONES_PREG();
                                      objOpciones.d_pregunta_op = rowOpt.GetAttr("d_pregunta_op");
                                      objOpciones.l_correcta = rowOpt.GetAttr("l_correcta") == "1" ? true : false;

                                      objPregunta.OPCION_PREG.Add(objOpciones);
                                  }
                              }
                              objOfertaLab.PREG_OF_LAB.Add(objPregunta);
                          }
                      }
                  }

              }
              strStep = "PERSISTO-OL";
              objTran = new NomadTransaction();
              objTran.Begin();
              objTran.SaveRefresh(objOfertaLab);
              objTran.Commit();

              NomadLog.Debug("oi_ferta_lab " + pParam.GetAttr("oi_oferta_lab"));

             
            
             //Si es nuevo, ENVIO MAIL
             if(pParam.GetAttr("oi_oferta_lab")=="")
             {
                 NomadLog.Debug("Entro..");

                  strStep = "MAIL";
                  NomadXML xmlMail = Nomad.NSystem.Base.NomadEnvironment.QueryNomadXML(NucleusRH.Base.Postulantes.Ofertas_Laborales.OFERTA_LAB.Resources.GET_MAIL_OL, "");

                  string strDestino = xmlMail.FirstChild().GetAttr("d_mail");
                  string strCuerpoMail="Se creó la siguiente Oferta Laboral: \n\n";
                  strCuerpoMail = strCuerpoMail + "Nro de Referencia: " + objOfertaLab.c_oferta_lab + '\n';
                  strCuerpoMail = strCuerpoMail + "Puesto: " + objOfertaLab.d_oferta_lab+ '\n';
                  strCuerpoMail = strCuerpoMail + "Fecha de Alta: " + objOfertaLab.f_oferta_lab.ToShortDateString()+ '\n';

                  string fCierre = "";
                  if (!(objOfertaLab.f_cierreNull))
                      fCierre = objOfertaLab.f_cierre.ToShortDateString();

                  strCuerpoMail = strCuerpoMail + "Fecha de Cierre: " + fCierre + '\n';

                  string strSelector = NucleusRH.Base.Personal.Legajo.PERSONAL.Get(objOfertaLab.oi_personal).d_ape_y_nom;
                  strCuerpoMail = strCuerpoMail + "Selector: " + strSelector + '\n';

                  strCuerpoMail = strCuerpoMail + "Descripción: " + objOfertaLab.o_oferta_lab + '\n';


                  Nomad.Base.Mail.Mails.MAIL objMail = Nomad.Base.Mail.Mails.MAIL.CrearTXT("Creación de nueva Oferta Laboral", strCuerpoMail,strDestino, "contacto@nucleussa.com.ar");

               
                  objMail.Enviar();
                  xmlResult.SetAttr("oi_oferta_lab_new", objOfertaLab.Id);

          }
              xmlResult.SetAttr("resultOk", "1");

              //Actualiza las postulaciones de esa oferta
              NomadLog.Debug("oi_oferta_lab para actualizar:  "+pParam.GetAttr("oi_oferta_lab"));
              NucleusRH.Base.Postulantes.CV.CV.ActualizarPostulaciones(pParam.GetAttr("oi_oferta_lab"), "OL");

          }
          catch (Exception ex)
          {
              xmlResult.SetAttr("resultOk", "0");
              if (objTran != null)
              {
                  objTran.Rollback();
                  xmlResult.SetAttr("result", "0");
              }

              NomadException nmdEx = new NomadException("NucleusRH.Base.Postulantes.CV.WEB_SaveOfertaLab()", ex);
              nmdEx.SetValue("Step", strStep);
              throw nmdEx;
          }

          NomadLog.Debug("xmlResultSaveOfertaLab: " + xmlResult.ToString());
          return xmlResult;
      }

     /// <summary>
     /// Recupera la cantidad que cumplen requisitos y que no los cumplen como asi tambien la cantidad total
     /// de postulaciones a una oferta laboral parametro
     /// </summary>
     /// <param name="pParam"></param>
     /// <returns></returns>
      public static NomadXML  WEB_COUNT_REQ(NomadXML pParam)
      {
          NomadLog.Debug("-----------------------------------------------");
          NomadLog.Debug("----------WEB_COUNT_REQ V1------------");
          NomadLog.Debug("-----------------------------------------------");

          NomadLog.Debug("WEB_COUNT_REQ.pParam: " + pParam.ToString());

          NomadXML xmlResult;
          string strMode;
          string strStep = "";
          strMode = pParam.GetAttr("mode");
          xmlResult = new NomadXML("DATA");
          xmlResult.SetAttr("mode", strMode);
          try
          {
              strStep = "GET-COUNT-REQ";
              NomadXML xmlParam = new NomadXML("DATA");
              xmlParam.SetAttr("oi_oferta_lab",pParam.GetAttr("oi_oferta_lab"));
              xmlResult = Nomad.NSystem.Base.NomadEnvironment.QueryNomadXML(NucleusRH.Base.Postulantes.Ofertas_Laborales.OFERTA_LAB.Resources.QRY_CANT_REQ, xmlParam.ToString());
          }
          catch (Exception ex)
          {
              NomadException nmdEx = new NomadException("NucleusRH.Base.Postulantes.Ofertas_Laborales.OFERTA_LAB.WEB_COUNT_REQ()", ex);
              nmdEx.SetValue("Step", strStep);
              throw nmdEx;
          }

          NomadLog.Debug("xmlResultOLWCountReq: " + xmlResult.ToString());
          return xmlResult;
      }

      
      /// <summary>
      /// Devuelve los datos de una oferta laboral parametro para el modo show
      /// </summary>
      /// <param name="pParam"></param>
      /// <returns></returns>
      public static NomadXML WEB_LoadOfertaLab_Show(NomadXML pParam)
      {
          NomadLog.Debug("-----------------------------------------------");
          NomadLog.Debug("----------WEB_LoadOfertaLab_Show V1------------");
          NomadLog.Debug("-----------------------------------------------");

          NomadLog.Debug("WEB_LoadOfertaLab_Show.pParam: " + pParam.ToString());

          NomadXML xmlResult;
          string strMode;
          string strStep = "";
          strMode = pParam.GetAttr("mode");
          xmlResult = new NomadXML("DATA");
          xmlResult.SetAttr("mode", strMode);
          try
          {
              strStep = "ARMO-PARAM";
              NomadXML xmlParam = new NomadXML("DATA");
              xmlParam.SetAttr("oi_oferta_lab",pParam.GetAttr("oi_oferta_lab"));

              strStep = "EXEC-QRY";
              NomadXML xmlOl = Nomad.NSystem.Base.NomadEnvironment.QueryNomadXML(NucleusRH.Base.Postulantes.Ofertas_Laborales.OFERTA_LAB.Resources.LOAD_OFERTA_LAB_SHOW, xmlParam.ToString());

              xmlResult.AddTailElement(xmlOl);

          }
          catch (Exception ex)
          {
              NomadException nmdEx = new NomadException("NucleusRH.Base.Postulantes.Ofertas_Laborales.OFERTA_LAB.WEB_LoadOfertaLab_Show()", ex);
              nmdEx.SetValue("Step", strStep);
              throw nmdEx;
          }

          NomadLog.Debug("xmlResultOLShow: " + xmlResult.ToString());
          return xmlResult;
      }

      /// <summary>
      /// Carga los datos de cabecera de una oferta laboral para agregar a la lista de postulaciones
      /// </summary>
      /// <param name="pParam"></param>
      /// <returns></returns>
      public static NomadXML WEB_LoadOfertaLab_Head(NomadXML pParam)
      {
          NomadLog.Debug("-----------------------------------------------");
          NomadLog.Debug("----------WEB_LoadOfertaLab_Head V1------------");
          NomadLog.Debug("-----------------------------------------------");

          NomadLog.Debug("WEB_LoadOfertaLab_Head.pParam: " + pParam.ToString());

          NomadXML xmlResult;
          string strMode;
          string strStep = "";
          strMode = pParam.GetAttr("mode");
          xmlResult = new NomadXML("DATA");
          xmlResult.SetAttr("mode", strMode);
          try
          {
              strStep = "ARMO-PARAM";
              NomadXML xmlParam = new NomadXML("DATA");
              xmlParam.SetAttr("oi_oferta_lab", pParam.GetAttr("oi_oferta_lab"));

              strStep = "EXEC-QRY";
              NomadXML xmlOl = Nomad.NSystem.Base.NomadEnvironment.QueryNomadXML(NucleusRH.Base.Postulantes.Ofertas_Laborales.OFERTA_LAB.Resources.GET_OFERTA_LAB_HEAD, xmlParam.ToString());

              xmlResult.AddTailElement(xmlOl);
          }
          catch (Exception ex)
          {
              NomadException nmdEx = new NomadException("NucleusRH.Base.Postulantes.Ofertas_Laborales.OFERTA_LAB.WEB_LoadOfertaLab_Head()", ex);
              nmdEx.SetValue("Step", strStep);
              throw nmdEx;
          }

          NomadLog.Debug("xmlResultOLHead: " + xmlResult.ToString());
          return xmlResult;
      }

      /// <summary>
      /// Devuelve la estructura de pregutnas y respuesta sde una postulacion parametro
      /// </summary>
      /// <param name="pParam"></param>
      /// <returns></returns>
      public static NomadXML WEB_GetRespuestas(NomadXML pParam)
      {
          NomadLog.Debug("-----------------------------------------------");
          NomadLog.Debug("----------WEB_GetRespuestas V1------------");
          NomadLog.Debug("-----------------------------------------------");

          NomadLog.Debug("WEB_GetRespuestas.pParam: " + pParam.ToString());

          NomadXML xmlResult;
          string strMode;
          string strStep = "";
          strMode = pParam.GetAttr("mode");
          xmlResult = new NomadXML("DATA");
          xmlResult.SetAttr("mode", strMode);
          try
          {
              strStep = "XMLPARAM";
              NomadXML xmlParam = new NomadXML("DATA");
              xmlParam.SetAttr("oi_postulaciones", pParam.GetAttr("oi_postulaciones"));

              strStep = "GET-RESP";
              NomadXML xmlResp = Nomad.NSystem.Base.NomadEnvironment.QueryNomadXML(NucleusRH.Base.Postulantes.Ofertas_Laborales.OFERTA_LAB.Resources.GET_RESPUESTAS, xmlParam.ToString());

              for (NomadXML row = xmlResp.FirstChild().FirstChild(); row != null; row = row.Next())
              {
                  if (row.GetAttr("c_pregunta") == "A")
                  {
                      NomadXML xmlRespuesta = xmlResult.AddTailElement("RESPUESTA");
                      xmlRespuesta.SetAttr("c_pregunta",row.GetAttr("c_pregunta"));
                      xmlRespuesta.SetAttr("d_pregunta", row.GetAttr("d_pregunta"));
                      xmlRespuesta.SetAttr("d_respuesta", row.GetAttr("d_respuesta"));
                  }

                  if (row.GetAttr("c_pregunta") == "M")
                  {
                      int idResp= row.GetAttrInt("oi_respuesta");
                      NomadXML xmlRespuesta = xmlResult.AddTailElement("RESPUESTA");
                      xmlRespuesta.SetAttr("d_pregunta", row.GetAttr("d_pregunta"));
                      xmlRespuesta.SetAttr("c_pregunta", row.GetAttr("c_pregunta"));

                      //Recorro y agrego las opciones
                      for (NomadXML rowOp = row.FirstChild(); rowOp != null; rowOp = rowOp.Next())
                      {
                          NomadXML xmlOpcion = xmlRespuesta.AddTailElement("OPCION");
                          xmlOpcion.SetAttr("d_pregunta_op", rowOp.GetAttr("d_pregunta_op"));
                          xmlOpcion.SetAttr("oi_pregunta_op", rowOp.GetAttr("oi_pregunta_op"));
                          xmlOpcion.SetAttr("l_correcta", rowOp.GetAttr("l_correcta"));

                          if (rowOp.GetAttrInt("oi_pregunta_op") == idResp)
                              xmlOpcion.SetAttr("respuestaUsuario", "1");
                      }
                  }
              }
          }
          catch (Exception ex)
          {
              NomadException nmdEx = new NomadException("NucleusRH.Base.Postulantes.Ofertas_Laborales.OFERTA_LAB.WEB_GetRespuestas()", ex);
              nmdEx.SetValue("Step", strStep);
              throw nmdEx;
          }

          NomadLog.Debug("WEB_GetRespuestas: " + xmlResult.ToString());
          return xmlResult;
      }


      /// <summary>
      /// Retorna la lista de oi_cv de una oferta laboral (con filtros y ordenamientos)
      /// </summary>
      /// <param name="pParam"></param>
      /// <returns></returns>
      public static NomadXML WEB_OiCV4Pagination(NomadXML pParam)
      {
          NomadLog.Debug("-----------------------------------------------");
          NomadLog.Debug("----------WEB_OiCV4Pagination V1------------");
          NomadLog.Debug("-----------------------------------------------");

          NomadLog.Debug("WEB_OiCV4Pagination.pParam: " + pParam.ToString());

          NomadXML xmlResult;
          string strMode;
          string strStep = "";

          strMode = pParam.GetAttr("mode");
          xmlResult = new NomadXML("DATA");
          xmlResult.SetAttr("mode", strMode);

          try
          {
              strStep = "GET-CVs-OfertaLab" + pParam.GetAttrString("oi_oferta_lab");
              NomadXML xmlParam = new NomadXML("DATA");
              xmlParam.SetAttr("oi_oferta_lab", pParam.GetAttr("oi_oferta_lab"));
              xmlParam.SetAttr("campo", pParam.GetAttr("campo"));
              xmlParam.SetAttr("criterio", pParam.GetAttr("criterio"));

              strStep = "QRY-LIST-POSTU-CV";
              xmlParam.SetAttr("qryMode", "PAGINATION");
                            
              if (pParam.GetAttr("l_leido") != "")
                  xmlParam.SetAttr("l_leido", pParam.GetAttr("l_leido"));

              if (pParam.GetAttr("c_marca") != "")
                  xmlParam.SetAttr("c_marca", pParam.GetAttr("c_marca"));
              
              if (pParam.GetAttr("l_notas") != "")
                  xmlParam.SetAttr("l_notas", pParam.GetAttr("l_notas"));

              if (pParam.GetAttr("campo_buscar_1") != "")
                  xmlParam.SetAttr("campo_buscar_1", pParam.GetAttr("campo_buscar_1"));

              if (pParam.GetAttr("campo_buscar_2") != "")
                  xmlParam.SetAttr("campo_buscar_2", pParam.GetAttr("campo_buscar_2"));

              if (pParam.GetAttr("campo_buscar_3") != "")
                  xmlParam.SetAttr("campo_buscar_3", pParam.GetAttr("campo_buscar_3"));

              if (pParam.GetAttr("campo_buscar_4") != "")
                  xmlParam.SetAttr("campo_buscar_4", pParam.GetAttr("campo_buscar_4"));

              if (pParam.GetAttr("campo_buscar_5") != "")
                  xmlParam.SetAttr("campo_buscar_5", pParam.GetAttr("campo_buscar_5"));

              if (pParam.GetAttr("campo_buscar_6") != "")
                  xmlParam.SetAttr("campo_buscar_6", pParam.GetAttr("campo_buscar_6"));

              if (pParam.GetAttr("campo_buscar_7") != "")
                  xmlParam.SetAttr("campo_buscar_7", pParam.GetAttr("campo_buscar_7"));

              if (pParam.GetAttr("campo_buscar_8") != "")
                  xmlParam.SetAttr("campo_buscar_8", pParam.GetAttr("campo_buscar_8"));

              if (pParam.GetAttr("campo_buscar_9") != "")
                  xmlParam.SetAttr("campo_buscar_9", pParam.GetAttr("campo_buscar_9"));

              if (pParam.GetAttr("campo_buscar_10") != "")
                  xmlParam.SetAttr("campo_buscar_10", pParam.GetAttr("campo_buscar_10"));

              if (pParam.GetAttr("d_puesto") != "")
                  xmlParam.SetAttr("d_puesto", pParam.GetAttr("d_puesto"));

              if (pParam.GetAttr("d_empresa") != "")
                  xmlParam.SetAttr("d_empresa", pParam.GetAttr("d_empresa"));

              if (pParam.GetAttr("d_estudio") != "")
                  xmlParam.SetAttr("d_estudio", pParam.GetAttr("d_estudio"));

              if (pParam.GetAttr("d_localidad") != "")
                  xmlParam.SetAttr("d_localidad", pParam.GetAttr("d_localidad"));

              xmlParam.SetAttr("l_cumple_req", pParam.GetAttr("l_cumple_req"));

              NomadLog.Debug("xmlParamOiCV-OfertaLabPagination: " + xmlParam.ToString());
              NomadXML xmlLoad = Nomad.NSystem.Base.NomadEnvironment.QueryNomadXML(NucleusRH.Base.Postulantes.Ofertas_Laborales.OFERTA_LAB.Resources.GET_CV_OFERTA_LAB, xmlParam.ToString());
              xmlResult.AddTailElement(xmlLoad);  
          }
          catch (Exception ex)
          {
              NomadException nmdEx = new NomadException("NucleusRH.Base.Postulantes.CV.WEB_OiCV4Pagination()", ex);
              nmdEx.SetValue("Step", strStep);
              throw nmdEx;
          }

          NomadLog.Debug("xmlResultOiCV4Pagination: " + xmlResult.ToString());
          return xmlResult;
      }
  }

}

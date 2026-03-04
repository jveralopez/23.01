using System;
using System.Xml;
using System.IO;
using System.Collections;
using System.Collections.Generic;

using System.Reflection;
using Microsoft.CSharp;
using System.CodeDom.Compiler;

using Nomad.NSystem.Document;
using Nomad.NSystem.Proxy;
using Nomad.NSystem.Base;
using Nomad.NSystem.Functions;



namespace NucleusRH.Base.Postulantes.CV
{

  public partial class CV
  {
    const string tmpOpen = "{#";
    const string tmpClose = "#}";

    /// <summary>
    /// Retorna una Hashtable con el asunto y el texto del mail solicitado
    /// </summary>
    /// <returns></returns>
    public static Hashtable GetMails(string pstrCode, CV pobjCV, NomadXML pxmlInfo)
    {
      NomadXML xmlParams;
      NomadXML xmlResult;
      Hashtable htaResult;

      //Crea el objeto de Parametros
      xmlParams = new NomadXML("PARAM");
      xmlParams.SetAttr("cod", pstrCode);

      //Ejecuta el query
      xmlResult = NomadEnvironment.QueryNomadXML(NucleusRH.Base.Postulantes.Textos_Mails.TEXTO_MAIL.Resources.QRY_Mails, xmlParams.ToString());
      xmlResult = xmlResult.FirstChild();

      htaResult = new Hashtable();

      //Reemplaza los valores
      for (NomadXML xmlMail = xmlResult.FirstChild(); xmlMail != null; xmlMail = xmlMail.Next())
      {
        htaResult.Add(xmlMail.GetAttr("c_texto_mail"), GetMailText(xmlMail.GetAttr("t_texto_mail"), pobjCV, pxmlInfo));
      }

      return htaResult;

    }

    /// <summary>
    /// Retorna el texto del mail realizando los replace correspondientes
    /// </summary>
    /// <param name="pstrText">Texto a modificar</param>
    /// <param name="pobjCV">Objeto CV</param>
    /// <param name="pxmlInfo"></param>
    /// <returns></returns>
    private static string GetMailText(string pstrText, CV pobjCV, NomadXML pxmlInfo)
    {
      string strContent;
      string strBefore;
      string strToReplace;
      System.Text.StringBuilder sbResult = new System.Text.StringBuilder();
      int intControl = 0;

      try
      {
        strContent = pstrText;

        //Busca los sectores a reemplazar
        while (strContent != "" || intControl < 50)
        {

          strBefore = SubStringBefore(strContent, tmpOpen);
          strContent = SubStringAfter(strContent, tmpOpen);

          sbResult.Append(strBefore);

          if (strContent != "") {
            strToReplace = SubStringBefore(strContent, tmpClose);
            strContent = SubStringAfter(strContent, tmpClose);

            if (strToReplace != "")
            {
              //Se realiza el reemplazo del texto por lo indicado
              switch (strToReplace.ToUpper())
              {
                case "S.FECHA":
                  sbResult.Append(DateTime.Now.ToString("dd/MM/yyyy"));
                  break;

                case "S.HORA":
                  sbResult.Append(DateTime.Now.ToString("HH:mm"));
                  break;

                case "S.BIENVENIDO":
                  if (pobjCV != null) sbResult.Append(pobjCV.c_sexo == "M" ? "Bienvenido" : "Bienvenida");
                  break;

                case "S.ESTIMADO":
                  if (pobjCV != null) sbResult.Append(pobjCV.c_sexo == "M" ? "Estimado" : "Estimada");
                  break;

                case "C.NOMBRES":
                  if (pobjCV != null) sbResult.Append(pobjCV.d_nombres);
                  break;

                case "C.APELLIDO":
                  if (pobjCV != null) sbResult.Append(pobjCV.d_apellido);
                  break;

                case "C.PASSWORD":
                  if (pobjCV != null) sbResult.Append(pobjCV.c_password);
                  break;

                case "O.CODIGO":
                  if (pxmlInfo != null) sbResult.Append(pxmlInfo.GetAttr("c_oferta_lab"));
                  break;

                case "O.DECRIP":
                  if (pxmlInfo != null) sbResult.Append(pxmlInfo.GetAttr("d_oferta_lab"));
                  break;
              }

            }

          }

          intControl++;
        }

      } catch {
        return "";
      }

      return sbResult.ToString();

    }

    private static string SubStringBefore(string pSource, string pTarget)
    {
      string strResult;
      int intPos;

      intPos = pSource.IndexOf(pTarget);
      if (intPos >= 0)
        strResult = pSource.Substring(0, intPos);
      else
        strResult = pSource;

      return strResult;
    }

    private static string SubStringAfter(string pSource, string pTarget)
    {
      string strResult = "";
      int intPos;

      intPos = pSource.IndexOf(pTarget);
      if (intPos >= 0)
      {
        intPos = intPos + pTarget.Length;
        strResult = pSource.Substring(intPos, pSource.Length - intPos);
      }

      return strResult;
    }


    /// <summary>
    /// Concatena los atributos correspondientes y los setea en el campo_like de la tabla pos01_CV
    /// Además actualiza el nivel de estudio del cv como el mejor nivel de estudio
    /// Ajusta las postulacionesd e ese CV con el ActualizarPostulaciones en modo CV
    /// </summary>
    /// <param name="pstrOiCV"></param>
    public static void SET_CAMPO_LIKE(string pstrOiCV, string pstriOiNivelEstudio)
    {
        NomadLog.Debug("----------------------------------------------------------");
        NomadLog.Debug("-----------------SET_CAMPO_LIKE V2-----------------");
        NomadLog.Debug("----------------------------------------------------------");

        NomadLog.Debug("pstrOiCV: " + pstrOiCV);
        NomadLog.Debug("pstriOiNivelEstudio: " + pstriOiNivelEstudio);

            string strStep = "";
            NomadTransaction objTrans;   
            try
            {
                string strCampoLike = "";
                strStep = "ARMO-PARAMETRO";
                NomadXML xmlParam;
                xmlParam = new NomadXML("DATA");
                xmlParam.SetAttr("oi_cv",pstrOiCV);

                strStep = "GET-DATOS";
                NomadXML xmlCV = Nomad.NSystem.Base.NomadEnvironment.QueryNomadXML(NucleusRH.Base.Postulantes.CV.CV.Resources.CAMPO_LIKE,xmlParam.ToString());

                strStep = "SETEO-CV";
                strCampoLike += xmlCV.FirstChild().GetAttr("d_apellido");                
                strCampoLike += ",";
                strCampoLike += xmlCV.FirstChild().GetAttr("d_nombres");
                strCampoLike += ",";
                if (xmlCV.FirstChild().GetAttr("c_sexo") != "")
                {
                    strCampoLike += xmlCV.FirstChild().GetAttr("c_sexo");
                    strCampoLike += ",";

                    strCampoLike += xmlCV.FirstChild().GetAttr("c_sexo_txt");
                    strCampoLike += ",";
                }
                if (xmlCV.FirstChild().GetAttr("c_estado") != "")
                {
                    strCampoLike += xmlCV.FirstChild().GetAttr("c_estado");
                    strCampoLike += ",";

                    strCampoLike += xmlCV.FirstChild().GetAttr("c_estado_txt");
                    strCampoLike += ",";
                }

                if (xmlCV.FirstChild().GetAttr("d_estado_civil") != "")
                {
                    strCampoLike += xmlCV.FirstChild().GetAttr("d_estado_civil");
                    strCampoLike += ",";
                }
                if (xmlCV.FirstChild().GetAttr("d_pais") != "")
                {
                    strCampoLike += xmlCV.FirstChild().GetAttr("d_pais");
                    strCampoLike += ",";
                }
                if (xmlCV.FirstChild().GetAttr("d_provincia") != "")
                {
                    strCampoLike += xmlCV.FirstChild().GetAttr("d_provincia");
                    strCampoLike += ",";
                }
                if (xmlCV.FirstChild().GetAttr("d_localidad") != "")
                {
                    strCampoLike += xmlCV.FirstChild().GetAttr("d_localidad");
                    strCampoLike += ",";
                }

                strStep = "SETEO-ANTECENDENTE";
                for (NomadXML xmlRowAntecedentes = xmlCV.FirstChild().FindElement("ANTECEDENTES").FirstChild(); xmlRowAntecedentes != null; xmlRowAntecedentes = xmlRowAntecedentes.Next())
                {
                    strCampoLike += xmlRowAntecedentes.GetAttr("d_empresa");
                    strCampoLike += ",";
                    strCampoLike += xmlRowAntecedentes.GetAttr("d_puesto");
                    strCampoLike += ",";
                    if (xmlRowAntecedentes.GetAttr("d_area_lab") != "")
                    {
                        strCampoLike += xmlRowAntecedentes.GetAttr("d_area_lab");
                        strCampoLike += ",";
                    }
                    if (xmlRowAntecedentes.GetAttr("o_tareas") != "")
                    {
                        strCampoLike += xmlRowAntecedentes.GetAttr("o_tareas");
                        strCampoLike += ",";
                    }
                }

                strStep = "SETEO-CONOCIMIENTOS";
                for (NomadXML xmlRowConocimientos = xmlCV.FirstChild().FindElement("CONOCIMIENTOS").FirstChild(); xmlRowConocimientos != null; xmlRowConocimientos = xmlRowConocimientos.Next())
                {
                    strCampoLike += xmlRowConocimientos.GetAttr("d_conocimiento");
                    strCampoLike += ",";
                }
                strStep = "SETEO-ESTUDIOS";
                for (NomadXML xmlRowEstudios = xmlCV.FirstChild().FindElement("ESTUDIOS").FirstChild(); xmlRowEstudios != null; xmlRowEstudios = xmlRowEstudios.Next())
                {
                    strCampoLike += xmlRowEstudios.GetAttr("d_estudio");
                    strCampoLike += ",";
                    strCampoLike += xmlRowEstudios.GetAttr("d_area_est");
                    strCampoLike += ",";
                }
                strStep = "AJUSTO-CL";
                if (strCampoLike.Length >= 4000)
                {
                    strCampoLike = strCampoLike.Substring(0, 3999);
                }

                strStep = "GET-CV";
                NucleusRH.Base.Postulantes.CV.CV objCV = CV.Get(pstrOiCV);
                NomadLog.Debug("objCVLucho: " + objCV.ToString());
                strStep = "SET-CL";
                objCV.o_campo_like = strCampoLike;
                
				//Quito los espacios al correo
                objCV.d_email = objCV.d_email.Trim(' ');

                //Actualizo el nivel de estudio maximo del CV
                strStep = "SET-NIVEST";
                //Si no es A, se agrega un estudio al cv
                if (pstriOiNivelEstudio != "A")
                {

                    NomadLog.Debug("pstriOiNivelEstudio: " + pstriOiNivelEstudio);
                    if ((pstriOiNivelEstudio != null) && (pstriOiNivelEstudio != ""))
                    {
                        NomadLog.Debug("objCV.ESTUDIOS_CV.Count: " + objCV.ESTUDIOS_CV.Count.ToString());
                        //Si no tiene ninguno, lo agrego
                        if (objCV.ESTUDIOS_CV.Count == 0)
                        {
                            NucleusRH.Base.Postulantes.CV.ESTUDIO_CV objEstudio = new ESTUDIO_CV();
                            objEstudio.oi_nivel_estudio = pstriOiNivelEstudio;

                            objCV.ESTUDIOS_CV.Add(objEstudio);
                        }
                        else
                        {
                            NomadLog.Debug("pstriOiNivelEstudioV2: " + pstriOiNivelEstudio);


                            //Si tiene nivel de estudio y este no cambio, no se hace nada - Si el pstriOiNivelEstudio es # implica que no cambio.
                            //Si no es #, actualizo los niveles de estudio
                            //Si cambio, borro los que tengo y defino el nuevo nivel de estudio
                            if (pstriOiNivelEstudio != "#")
                            {
                                strStep = "DEL-ALL";
                                objCV.ESTUDIOS_CV.Clear();

                                strStep = "ADD-EST-EDIT";

                                NucleusRH.Base.Postulantes.CV.ESTUDIO_CV objEstudio = new ESTUDIO_CV();
                                objEstudio.oi_nivel_estudio = pstriOiNivelEstudio;

                                objCV.ESTUDIOS_CV.Add(objEstudio);
                            }
                        }
                    }
                    else
                    {
                        //si viene vacio, le quita todos los estudios
                        objCV.oi_nivel_estudioNull = true;
                        objCV.ESTUDIOS_CV.Clear();                    
                    }

                    strStep = "SAVE-CV-CL-EST";
                    objTrans = new NomadTransaction();
                    objTrans.Begin();
                    objTrans.SaveRefresh(objCV);
                    objTrans.Commit();
                }

                    NomadXML xmlParamNivEst = new NomadXML("DATA");
                    xmlParamNivEst.SetAttr("oi_cv", pstrOiCV);

                    NomadXML xmlNivEst = Nomad.NSystem.Base.NomadEnvironment.QueryNomadXML(NucleusRH.Base.Postulantes.CV.CV.Resources.GET_NIV_EST_CV, xmlParamNivEst.ToString());

                    objCV.oi_nivel_estudio = xmlNivEst.FirstChild().GetAttr("oi_nivel_estudio");
                
               


                strStep = "SAVE-CV-NIVEST";
                objTrans = new NomadTransaction();
                objTrans.Begin();
                objTrans.Save(objCV);
                objTrans.Commit();                


                //Invoca el actualizar postulaciones
                NucleusRH.Base.Postulantes.CV.CV.ActualizarPostulaciones(pstrOiCV, "CV");
                
            }
            catch (Exception ex)
            {
                NomadException nmdEx = new NomadException("NucleusRH.Base.Postulantes.CV.SET_CAMPO_LIKE()", ex);
                nmdEx.SetValue("Step", strStep);
                throw nmdEx;
            }
            
    
    }

    /// <summary>
    /// Metodo cotnrolador de buscar CV
    /// </summary>
    /// <param name="pMode"></param>
    /// <param name="pParam"></param>
    /// <returns></returns>
      public static NomadXML WebCVManager(string pMode, NomadXML pParam) 
      {
        NomadXML xmlResult = null;
        NomadXML xmlParam = null;
        
        xmlParam = pParam.isDocument ? pParam.FirstChild() : pParam;

        NomadLog.Debug("-----------------------------------------------");
        NomadLog.Debug("----------WebCVManager() V1--------------------");
        NomadLog.Debug("-----------------------------------------------");

        NomadLog.Debug("pMode: " + pMode.ToString());
        NomadLog.Debug("pParam: " + pParam.ToString());

        switch (pMode.ToUpper()) 
        {
            case "MAINPAGE":
                xmlResult = CV.WEB_MainPage(xmlParam);                
                break;
            case "CREATESEARCH":
                xmlResult = CV.WEB_CreateSearch(xmlParam);
                break;
            case "LISTCVSEARCH":
                xmlResult = CV.WEB_ListCVSearch(xmlParam);
                break;
            case "LOADADVSEARCH":
                    xmlResult = CV.WEB_LoadAdvancedSearch(xmlParam);
                    break;
            case "LOADCVSHOW":
                xmlResult = CV.WEB_LoadCVShow(xmlParam);
                break;
            case "SAVENOTE":
                   xmlResult = CV.WEB_SaveNote(xmlParam);
                break;
            case "DELETENOTE":
                xmlResult = CV.WEB_DeleteNote(xmlParam);
                break;
            case "SAVECARPETA":
                xmlResult = CV.WEB_SaveCarpeta(xmlParam);
                break;
            case "LOADLISTCARPETA":
                xmlResult = CV.WEB_LoadListCarpetas(xmlParam);
                break;
            case "DELETECARPETA":
                xmlResult = CV.WEB_DeleteCarpeta(xmlParam);
                break;
            case "MASIVEDELETECARPETA":
                xmlResult = CV.WEB_MasiveDeleteCarpeta(xmlParam);
                break;
            case "ASOCIACVCARPETA":
                xmlResult = CV.WEB_Asocia_CV_Carpeta(xmlParam);
                break;
            case "LEIDONOLEIDO":
                xmlResult = CV.WEB_Leido_NoLeido(xmlParam);
                break;
            case "LOADLISTCVASOCIADOS":
                xmlResult = CV.WEB_LoadListCVsAsociados(xmlParam);
                break;
            case "MARCARCVCARPETA":
                xmlResult = CV.WEB_MarcarCVCarpeta(xmlParam);
                break;
            case "DESASOCIACVCARPETA":
                xmlResult = CV.WEB_DesAsociarCVCarpeta(xmlParam);
                break;
            case "GRAPHICS":
                xmlResult = CV.WEB_Grapahic(xmlParam);
                break;
            case "CVMAIL":
                xmlResult = CV.WEB_CVMAIL(xmlParam);
                break;
            case "OFERTALAB":
                xmlResult = CV.WEB_LOAD_OFERTALAB(xmlParam);
                break;
            case "GENERALEST":
                xmlResult = CV.WEB_GENERAL_ESTADISTICAS(xmlParam);
                break;
            case "BCVPEDCOL":
                xmlResult = CV.WEB_INIT_BCV_PEDYCOL(xmlParam);
                break;
            case "PCCHANGESEARCH":
                xmlResult = CV.WEB_CHANGE_SEARCH_PYC(xmlParam);
                break;
            case "PCFILTER":
                xmlResult = CV.WEB_CV_FILTER_PYC(xmlParam);
                break;
            case "PCLOADCARPETA":
                xmlResult = CV.WEB_CV_LOADCARPETA_PYC(xmlParam);
                break;
            case "PCLOADOL":
                xmlResult = CV.WEB_CV_LOADOL_PYC(xmlParam);
                break;
            case "SETCVTODETALLE":
                xmlResult = CV.WEB_CV_SETDETALLE_PYC(xmlParam);
                break;
            case "LISTCVREPORT":
                xmlResult = CV.WEB_ListCVReport(xmlParam); 
                break;
            case "OICV4PAGINATION":
                xmlResult = CV.WEB_OiCV4Pagination(xmlParam);
                break;

        }
        
        return xmlResult;
        
    }

      #region Buscador de CVs
      /// <summary>
    /// Metodo que controla los cambios de carpeta y busquedas de  MAIN.htm del buscador de CV
    /// </summary>
    /// <param name="pParam"></param>
    /// <returns></returns>
      public static NomadXML WEB_MainPage(NomadXML pParam) 
    {

        //NomadProxy proxy = new NomadProxy();
        NomadXML xmlResult;
        NomadXML xmlList;
        NomadXML xmlItem;
        string strModeSearch;
        string strModeFolder;

        string strStep = "";

        strModeSearch = pParam.GetAttr("modeSearch");
        strModeFolder = pParam.GetAttr("modeFolder");


        NomadLog.Debug("strModeSearch: " + strModeSearch.ToString());
        NomadLog.Debug("strModeFolder: " + strModeFolder.ToString());  

        xmlResult = new NomadXML("DATA");

        xmlResult.SetAttr("modeSearch", strModeSearch);
        xmlResult.SetAttr("modeFolder", strModeFolder);

        try
        {
            if (strModeSearch == "BM")
            {                        
                strStep = "MODO:BM";

                NomadXML xmlParamMisBusquedas = new NomadXML("DATA");
                NomadProxy proxy = NomadEnvironment.GetProxy();
                xmlParamMisBusquedas.SetAttr("oi_usuario_sistema", proxy.UserEtty);
                strStep = "GET:MIS-BUSQUEDAS";
                NomadXML xmlResultMisBusquedas = Nomad.NSystem.Base.NomadEnvironment.QueryNomadXML(NucleusRH.Base.Postulantes.Busquedas.BUSQUEDA.Resources.GET_MIS_BUSQUEDAS, xmlParamMisBusquedas.ToString());

                xmlList = xmlResult.AddTailElement("LIST-SEARCH");
                xmlList.SetAttr("name", "SEARCH");
                xmlList.SetAttr("mode", "BM");
                
                //Recorro las busquedas
                for (NomadXML row = xmlResultMisBusquedas.FirstChild().FirstChild(); row != null; row = row.Next())
                {
                    xmlItem = xmlList.AddTailElement("ITEM");
                    xmlItem.SetAttr("oiBusqueda", row.GetAttrString("oi_busqueda"));
                    xmlItem.SetAttr("desc", row.GetAttrString("d_busqueda"));
                    xmlItem.SetAttr("cant", row.GetAttrInt("e_cant_res"));                   
                }        
            }

            if (strModeSearch == "BP")
            {                                 
                strStep = "MODO:BP";
                NomadXML xmlB = new NomadXML("DATA");
                NomadXML xmlResultBusquedasPopulares = Nomad.NSystem.Base.NomadEnvironment.QueryNomadXML(NucleusRH.Base.Postulantes.Busquedas.BUSQUEDA.Resources.GET_BUSQUEDAS_P, xmlB.ToString());
                xmlList = xmlResult.AddTailElement("LIST-SEARCH");
                xmlList.SetAttr("name", "SEARCH");
                xmlList.SetAttr("mode", "BP");

                for (NomadXML row = xmlResultBusquedasPopulares.FirstChild().FirstChild(); row != null; row = row.Next())
                {
                    xmlItem = xmlList.AddTailElement("ITEM");
                    xmlItem.SetAttr("oiBusqueda", row.GetAttrString("oi_busqueda"));
                    xmlItem.SetAttr("desc", row.GetAttrString("d_busqueda"));
                    xmlItem.SetAttr("cant", row.GetAttrInt("e_cant_res"));
                   // xmlItem.SetAttr("cant", row.GetAttrString("cantResult"));

                    //PARA CADA BUSQUEDA CALCULO LA CANTIDAD
                    string strCampoLike = row.GetAttrString("d_busqueda");
                    //Separo palabras y armo parametro de busqueda de cantidad
                    string[] arrayPalabras = strCampoLike.Split(',');
                    NomadXML xmlParamCant = new NomadXML("DATA");
                    for (int i = 0; (i <= arrayPalabras.Length - 1 && i != 10); i++)
                    {
                        string campo_like = arrayPalabras[i];
                        xmlParamCant.SetAttr("campo_like_" + (i + 1).ToString(), campo_like);
                    }                 

                } 

            }

            if (strModeFolder == "CM")
            {
                strStep = "MODO:CM";                

                NomadXML xmlParamMisCarpetas = new NomadXML("DATA");
                NomadProxy proxy = NomadEnvironment.GetProxy();
                xmlParamMisCarpetas.SetAttr("oi_usuario_sistema", proxy.UserEtty);
                strStep = "GET:MIS-CARPETAS";
                NomadXML xmlResultMisCarpetas = Nomad.NSystem.Base.NomadEnvironment.QueryNomadXML(NucleusRH.Base.Postulantes.Carpetas.CARPETA.Resources.GET_MIS_CARPETAS, xmlParamMisCarpetas.ToString());

                xmlList = xmlResult.AddTailElement("LIST-FOLDERS");
                xmlList.SetAttr("name", "FOLDER");
                xmlList.SetAttr("mode", "CM");
                //Recorro las carpetas
                for (NomadXML row = xmlResultMisCarpetas.FirstChild().FirstChild(); row != null; row = row.Next())
                {
                    xmlItem = xmlList.AddTailElement("ITEM");
                    xmlItem.SetAttr("id", row.GetAttrString("oi_carpeta"));
                    xmlItem.SetAttr("desc", row.GetAttrString("d_carpeta"));
                    xmlItem.SetAttr("f_carpeta", row.GetAttrString("f_carpeta")); 
                }          
            }
            if (strModeFolder == "CP")
            {
                strStep = "MODO:CP";
                NomadXML xmlResultCarpetasPopulares = Nomad.NSystem.Base.NomadEnvironment.QueryNomadXML(NucleusRH.Base.Postulantes.Carpetas.CARPETA.Resources.GET_CARPETAS_P, "");
                xmlList = xmlResult.AddTailElement("LIST-FOLDERS");
                xmlList.SetAttr("name", "FOLDER");
                xmlList.SetAttr("mode", "CP");

                for (NomadXML row = xmlResultCarpetasPopulares.FirstChild().FirstChild(); row != null; row = row.Next())
                {
                    xmlItem = xmlList.AddTailElement("ITEM");
                    xmlItem.SetAttr("id", row.GetAttrString("oi_carpeta"));
                    xmlItem.SetAttr("desc", row.GetAttrString("d_carpeta"));
                    xmlItem.SetAttr("f_carpeta", row.GetAttrString("f_carpeta")); 
                } 
            }

        }
        catch (Exception ex)
        {
            NomadException nmdEx = new NomadException("NucleusRH.Base.Postulantes.CV.WEB_MainPage()", ex);
            nmdEx.SetValue("Step", strStep);
            throw nmdEx;
        }
        NomadLog.Debug("xmlResultChanging: " + xmlResult.ToString());  
          return xmlResult;
    }

      /// <summary>
      /// Metodo que controla la busqueda partiendo de la pagina principal.
      /// Recibe como parametro el campo like, da de alta la busqueda y retorna el oi_busqueda creado
      /// </summary>
      /// <param name="pParam"></param>
      /// <returns></returns>
       public static NomadXML WEB_CreateSearch(NomadXML pParam)
      {

          NomadLog.Debug("-----------------------------------------------");
          NomadLog.Debug("-----------------WEB_CreateSearch V1-----------------");
          NomadLog.Debug("-----------------------------------------------");

          NomadLog.Debug("WEB_CreateSearch.pParam: " + pParam.ToString());  
          
          string strMode = pParam.GetAttr("mode");
          
          NomadXML xmlResult = new NomadXML("DATA");
          xmlResult.SetAttr("mode", strMode);          
          
          string strStep = "";
          NomadTransaction objTrans = null;
          try
          {
                  NomadLog.Debug("WEB_CreateSearch.CreateSearch" + pParam.ToString());
                  strStep = "CREATE-ADV-SEARCH";
                  NucleusRH.Base.Postulantes.Busquedas.BUSQUEDA objBusqueda = new Busquedas.BUSQUEDA();
                  objBusqueda.f_busqueda = DateTime.Now;
                  objBusqueda.oi_usuario_sistema = NomadEnvironment.GetProxy().UserEtty;
                  objBusqueda.c_busqueda = "A";
                  objBusqueda.d_busqueda = "";
                  strStep = "ADD-CAMPOS-d_puesto";
                  if (pParam.GetAttr("d_puesto") != "")
                  {
                      objBusqueda.d_puesto = pParam.GetAttr("d_puesto");
                      objBusqueda.d_busqueda = objBusqueda.d_busqueda + ", " + objBusqueda.d_puesto;
                  }
                  else
                      objBusqueda.d_puestoNull = true;

                  strStep = "ADD-CAMPOS-l_trab_puesto";
                  if ((pParam.GetAttr("l_trab_puesto") != "0") && (pParam.GetAttr("l_trab_puesto") != ""))
                  {
                      objBusqueda.l_trab_puesto = true;
                      objBusqueda.d_busqueda = objBusqueda.d_busqueda + ", Actualmente trabajando en el puesto";
                  }
                  else
                      objBusqueda.l_trab_puesto = false;

                  strStep = "ADD-CAMPOS-l_leido";
                  if ((pParam.GetAttr("c_leido") == "T"))
                  {
                      objBusqueda.c_leido = "T";                  
                  }
                  if ((pParam.GetAttr("c_leido") == "L"))
                  {
                      objBusqueda.c_leido = "L";
                      objBusqueda.d_busqueda = objBusqueda.d_busqueda + ", CVs Leídos";
                  }
                  
                  if ((pParam.GetAttr("c_leido") == "NL"))
                  {
                      objBusqueda.c_leido = "NL";
                      objBusqueda.d_busqueda = objBusqueda.d_busqueda + ", CVs No Leídos";
                  }
                                    
                  
                  strStep = "ADD-CAMPOS-e_anio_desde";
                  if (pParam.GetAttr("e_anio_desde") != "")
                      objBusqueda.e_anio_desde = pParam.GetAttrInt("e_anio_desde");
                  else
                      objBusqueda.e_anio_desdeNull = true;

                  strStep = "ADD-CAMPOS-e_anio_hasta";
                  if (pParam.GetAttr("e_anio_hasta") != "")
                      objBusqueda.e_anio_hasta = pParam.GetAttrInt("e_anio_hasta");
                  else
                      objBusqueda.e_anio_hastaNull = true;

                  if ((pParam.GetAttr("e_anio_desde") != "") && (pParam.GetAttr("e_anio_hasta") != ""))
                  {
                      objBusqueda.d_busqueda = objBusqueda.d_busqueda + ", Entre " + objBusqueda.e_anio_desde + " y " + objBusqueda.e_anio_hasta + " años de experiencia";
                  }
                  
                  if ((pParam.GetAttr("e_anio_desde") != "") && (pParam.GetAttr("e_anio_hasta") == ""))
                  {
                      objBusqueda.d_busqueda = objBusqueda.d_busqueda + ", Desde " + objBusqueda.e_anio_desde+" años de experiencia";
                  }
                  
                  if ((pParam.GetAttr("e_anio_desde") == "") && (pParam.GetAttr("e_anio_hasta") != ""))
                  {
                      objBusqueda.d_busqueda = objBusqueda.d_busqueda + ",Hasta " + objBusqueda.e_anio_hasta + " años de experiencia";
                  }

                  strStep = "ADD-CAMPOS-d_titulo";
                  if (pParam.GetAttr("d_titulo") != "")
                  {
                      objBusqueda.d_titulo = pParam.GetAttr("d_titulo");
                      objBusqueda.d_busqueda = objBusqueda.d_busqueda + ", " + objBusqueda.d_titulo;
                  }
                  else
                      objBusqueda.d_tituloNull = true;                  

                  strStep = "ADD-CAMPOS-d_empresa";
                  if (pParam.GetAttr("d_empresa") != "")
                  {
                      objBusqueda.d_empresa = pParam.GetAttr("d_empresa");
                      objBusqueda.d_busqueda = objBusqueda.d_busqueda + ", " + objBusqueda.d_empresa;

                  }
                  else
                      objBusqueda.d_empresaNull = true;

                  strStep = "ADD-CAMPOS-o_palabras_clave";
                  if (pParam.GetAttr("o_palabras_clave") != "")
                  {
                      objBusqueda.o_palabras_clave = pParam.GetAttr("o_palabras_clave");
                      objBusqueda.d_busqueda = objBusqueda.d_busqueda + ", " + objBusqueda.o_palabras_clave;
                  }
                  else
                      objBusqueda.o_palabras_claveNull = true;


                  //Agrego campos child
                  strStep = "ADD-CHILDS-Idiomas";
                  for (NomadXML row = pParam.FindElement("IDIOMAS").FirstChild(); row != null; row = row.Next())
                  {
                      if (row.GetAttr("nivel_general") != "I")
                      {
                          NucleusRH.Base.Postulantes.Busquedas.IDIOMAS objIdioma = new Busquedas.IDIOMAS();
                          objIdioma.oi_idioma = row.GetAttr("oi_idioma");
                          objIdioma.oi_nivel_general = row.GetAttr("nivel_general");
                          NomadLog.Debug("Que_carajo_estoy_por_addear: " + objIdioma.ToString());

                          objBusqueda.IDIOMA_BUS.Add(objIdioma);
                          NucleusRH.Base.Organizacion.Niveles_Idioma.NIVEL_IDIOMA objNivIdioma = NucleusRH.Base.Organizacion.Niveles_Idioma.NIVEL_IDIOMA.Get(row.GetAttr("nivel_general"));
                          objBusqueda.d_busqueda = objBusqueda.d_busqueda + ", " + row.GetAttr("label") + ": " + objNivIdioma.d_nivel_idioma;
                      }

                  }
                  
                  strStep = "ADD-CAMPOS-c_sexo";
                  if ((pParam.GetAttr("c_sexo") != "I") && (pParam.GetAttr("c_sexo") != ""))
                      objBusqueda.c_sexo = pParam.GetAttr("c_sexo");
                  else
                      objBusqueda.c_sexoNull = true;

                  if (objBusqueda.c_sexo == "F") { objBusqueda.d_busqueda = objBusqueda.d_busqueda + ", Femenino"; }
                  if (objBusqueda.c_sexo == "M") { objBusqueda.d_busqueda = objBusqueda.d_busqueda + ", Masculino"; }

                  strStep = "ADD-CAMPOS-e_edad_desde";
                  if (pParam.GetAttr("e_edad_desde") != "")
                      objBusqueda.e_edad_desde = pParam.GetAttrInt("e_edad_desde");
                  else
                      objBusqueda.e_edad_desdeNull = true;


                  strStep = "ADD-CAMPOS-e_edad_hasta";
                  if (pParam.GetAttr("e_edad_hasta") != "")
                      objBusqueda.e_edad_hasta = pParam.GetAttrInt("e_edad_hasta");
                  else
                      objBusqueda.e_edad_hastaNull = true;

                  if ((pParam.GetAttr("e_edad_desde") != "") && (pParam.GetAttr("e_edad_hasta") != ""))
                      objBusqueda.d_busqueda = objBusqueda.d_busqueda + ", Entre " + objBusqueda.e_edad_desde.ToString() + " y " + objBusqueda.e_edad_hasta.ToString() + " años de edad";

                  if ((pParam.GetAttr("e_edad_desde") != "") && (pParam.GetAttr("e_edad_hasta") == ""))
                      objBusqueda.d_busqueda = objBusqueda.d_busqueda + ", Mayor a " + objBusqueda.e_edad_desde.ToString() + " años de edad";

                  if ((pParam.GetAttr("e_edad_desde") == "") && (pParam.GetAttr("e_edad_hasta") != ""))
                      objBusqueda.d_busqueda = objBusqueda.d_busqueda + ", Hasta " + objBusqueda.e_edad_hasta.ToString() + " años de edad";

                  strStep = "ADD-CAMPOS-n_salario_desde";
                  if (pParam.GetAttr("n_salario_desde") != "")
                      objBusqueda.n_salario_desde = pParam.GetAttrDouble("n_salario_desde");
                  else
                      objBusqueda.n_salario_desdeNull = true;

                  strStep = "ADD-CAMPOS-n_salario_hasta";
                  if (pParam.GetAttr("n_salario_hasta") != "")
                      objBusqueda.n_salario_hasta = pParam.GetAttrDouble("n_salario_hasta");
                  else
                      objBusqueda.n_salario_hastaNull = true;

                  if ((pParam.GetAttr("n_salario_desde") != "") && (pParam.GetAttr("n_salario_hasta") != ""))
                      objBusqueda.d_busqueda = objBusqueda.d_busqueda + ", Salario entre " + objBusqueda.n_salario_desde.ToString() + " y " + objBusqueda.n_salario_hasta.ToString();

                  if ((pParam.GetAttr("n_salario_desde") != "") && (pParam.GetAttr("n_salario_hasta") == ""))
                      objBusqueda.d_busqueda = objBusqueda.d_busqueda + ", Salario mayor a  " + objBusqueda.n_salario_desde.ToString();

                  if ((pParam.GetAttr("n_salario_desde") == "") && (pParam.GetAttr("n_salario_hasta") != ""))
                      objBusqueda.d_busqueda = objBusqueda.d_busqueda + ", Salario menor a " + objBusqueda.n_salario_hasta.ToString();                  


                  strStep = "ADD-CAMPOS-c_trabajando";
                  if ((pParam.GetAttr("c_trabajando") != "I") && (pParam.GetAttr("c_trabajando") != ""))
                  {
                      objBusqueda.c_trabajando = pParam.GetAttr("c_trabajando");
                      objBusqueda.d_busqueda = objBusqueda.d_busqueda + ", Trabajando actualmente";
                  }
                  else
                      objBusqueda.c_trabajandoNull = true;
                  
                  strStep = "ADD-CAMPOS-c_persona_cargo";
                  if ((pParam.GetAttr("c_persona_cargo") != "I") && (pParam.GetAttr("c_persona_cargo") != ""))
                  {
                      objBusqueda.c_persona_cargo = pParam.GetAttr("c_persona_cargo");
                      objBusqueda.d_busqueda = objBusqueda.d_busqueda + ", Con Personas a Cargo ";
                  }
                  else
                      objBusqueda.c_persona_cargoNull = true;
                  
                  
                  strStep = "ADD-CAMPOS-oi_nivel_estudio";
                  if ((pParam.GetAttr("oi_nivel_estudio") != "I") && (pParam.GetAttr("oi_nivel_estudio") != ""))
                  {
                      objBusqueda.oi_nivel_estudio = pParam.GetAttr("oi_nivel_estudio");
                      NucleusRH.Base.Personal.Niveles_Estudio.NIVEL_ESTUDIO objNivelEst = Personal.Niveles_Estudio.NIVEL_ESTUDIO.Get(pParam.GetAttr("oi_nivel_estudio"));
                      objBusqueda.d_busqueda = objBusqueda.d_busqueda + ", " + objNivelEst.d_nivel_estudio;
                  }
                  else
                      objBusqueda.oi_nivel_estudioNull = true;

                  strStep = "ADD-CAMPOS-oi_estado_est";
                  if ((pParam.GetAttr("oi_estado_est") != "I") && (pParam.GetAttr("oi_estado_est") != ""))                  
                  {                      
                      objBusqueda.oi_estado_est = pParam.GetAttr("oi_estado_est");
                      NucleusRH.Base.Personal.Estados_Estudio.ESTADO_EST objEstadEst = Personal.Estados_Estudio.ESTADO_EST.Get(pParam.GetAttr("oi_estado_est"));
                      objBusqueda.d_busqueda = objBusqueda.d_busqueda + ", " + objEstadEst.d_estado_est;
                  }
                  else
                      objBusqueda.oi_estado_estNull = true;                  

                  strStep = "ADD-CAMPOS-d_ciudad";
                  if (pParam.GetAttr("d_ciudad") != "")
                  {
                      objBusqueda.d_ciudad = pParam.GetAttr("d_ciudad");
                      objBusqueda.d_busqueda = objBusqueda.d_busqueda + ", " + objBusqueda.d_ciudad;
                  }
                  else
                      objBusqueda.d_ciudadNull = true;

                  strStep = "ADD-CAMPOS-oi_pais";
                  if (pParam.GetAttr("oi_pais") != "")
                  {
                      objBusqueda.oi_pais = pParam.GetAttr("oi_pais");
                      NucleusRH.Base.Organizacion.Paises.PAIS objPais = NucleusRH.Base.Organizacion.Paises.PAIS.Get(objBusqueda.oi_pais);
                      objBusqueda.d_busqueda = objBusqueda.d_busqueda + ", " + objPais.d_pais;
                  }
                  else
                      objBusqueda.oi_paisNull = true;                 

                  strStep = "ADD-CAMPOS-d_nombres";
                  if (pParam.GetAttr("d_nombres") != "")
                  {
                      objBusqueda.d_nombres = pParam.GetAttr("d_nombres");
                      objBusqueda.d_busqueda = objBusqueda.d_busqueda + ", " + objBusqueda.d_nombres;
                  }
                  else
                      objBusqueda.d_nombresNull = true;

                  strStep = "ADD-CAMPOS-d_apellido";
                  if (pParam.GetAttr("d_apellido") != "")
                  {
                      objBusqueda.d_apellido = pParam.GetAttr("d_apellido");
                      objBusqueda.d_busqueda = objBusqueda.d_busqueda + ", " + objBusqueda.d_apellido;
                  }
                  else
                      objBusqueda.d_apellidoNull = true;

                  strStep = "ADD-CAMPOS-c_nro_doc";
                  if (pParam.GetAttr("c_nro_doc") != "")
                  {
                      objBusqueda.c_nro_doc = pParam.GetAttr("c_nro_doc");
                      objBusqueda.d_busqueda = objBusqueda.d_busqueda + ", DNI: " + objBusqueda.c_nro_doc;
                  
                  }
                  else
                      objBusqueda.c_nro_docNull = true;


                  strStep = "ADD-CAMPOS-l_foto";
                  if ((pParam.GetAttr("l_foto") != "0") && (pParam.GetAttr("l_foto") != ""))
                  {
                      objBusqueda.l_foto = true;
                      objBusqueda.d_busqueda = objBusqueda.d_busqueda + ", CVs con Foto";
                  }
                  else
                      objBusqueda.l_foto = false;


                  strStep = "ADD-CAMPOS-l_cv_sin_salario";
                  if ((pParam.GetAttr("l_cv_sin_salario") != "0") && (pParam.GetAttr("l_cv_sin_salario") != ""))                  
                      objBusqueda.l_cv_sin_salario = true;
                  else
                      objBusqueda.l_cv_sin_salario = false;

                  strStep = "ADD-CAMPOS-f_actualizacion";
                  if (pParam.GetAttr("f_actualizacion") != "")
                  {
                      objBusqueda.f_actualizacion = pParam.GetAttrDateTime("f_actualizacion");
                      objBusqueda.c_f_actualizacion = pParam.GetAttr("c_f_actualizacion");
                      string texto="";
                      switch (objBusqueda.c_f_actualizacion)
                      {
                          case "a": {texto = "Fecha de Actualización Menor a 15 días";} break;
                          case "b": {texto = "Fecha de Actualización Menor a 1 mes";} break;
                          case "c": {texto = "Fecha de Actualización Menor a 3 mes";} break;
                          case "d": {texto = "Fecha de Actualización Menor a 6 mes";} break;                      
                      }
                      objBusqueda.d_busqueda = objBusqueda.d_busqueda +", "+texto;
                  }
                  else
                  {
                      objBusqueda.f_actualizacionNull = true;
                      objBusqueda.c_f_actualizacionNull = true;
                  }

                  //La actualizo en el listado para evitar query
                  objBusqueda.e_cant_res = 0;
               

                  strStep = "ADD-CHILDS-AREAS-LABORALES";
                  for (NomadXML row = pParam.FindElement("AREAS-LABORALES").FirstChild(); row != null; row = row.Next())
                  {
                      NucleusRH.Base.Postulantes.Busquedas.AREA_PUESTO objAreaPuesto = new Busquedas.AREA_PUESTO();
                      objAreaPuesto.oi_area_lab = row.GetAttr("oi_area_lab");
                      objBusqueda.A_PUESTO_BUS.Add(objAreaPuesto);
                      NucleusRH.Base.Postulantes.Areas_Laborales.AREA_LAB objAreaLab = Postulantes.Areas_Laborales.AREA_LAB.Get(row.GetAttr("oi_area_lab"));
                      objBusqueda.d_busqueda = objBusqueda.d_busqueda + ", " + objAreaLab.d_area_lab;
                  }

                  strStep = "ADD-CHILDS-PROVINCIAS";
                  for (NomadXML row = pParam.FindElement("PROVINCIAS").FirstChild(); row != null; row = row.Next())
                  {
                      NucleusRH.Base.Postulantes.Busquedas.PROVINCIAS objProvincia = new Busquedas.PROVINCIAS();
                      objProvincia.oi_provincia = row.GetAttr("oi_provincia");
                      objBusqueda.PCIA_BUS.Add(objProvincia);
                      NucleusRH.Base.Organizacion.Paises.PROVINCIA objPcia = new Organizacion.Paises.PROVINCIA();
                      objPcia = Organizacion.Paises.PROVINCIA.Get(row.GetAttr("oi_provincia"));
                      objBusqueda.d_busqueda = objBusqueda.d_busqueda + ", " + objPcia.d_provincia;
                  }


                  strStep = "ADD-CHILDS-ESTADOS-CV";
                  for (NomadXML row = pParam.FindElement("ESTADOS-CV").FirstChild(); row != null; row = row.Next())
                  {
                      NucleusRH.Base.Postulantes.Busquedas.ESTADOS_CV objEstadoCV = new Busquedas.ESTADOS_CV();
                      objEstadoCV.c_estado = row.GetAttr("c_estado");
                      objEstadoCV.d_estado = row.GetAttr("d_estado");
                      objBusqueda.ESTADOS_CV_BUS.Add(objEstadoCV);
                      objBusqueda.d_busqueda = objBusqueda.d_busqueda + ", CV en Estado " + row.GetAttr("d_estado");
                  }


                  strStep = "ADD-CAMPOS-oi_oferta_lab";
                  if (pParam.GetAttr("oi_oferta_lab") != "")
                  {
                      objBusqueda.oi_oferta_lab = pParam.GetAttr("oi_oferta_lab");
                      NucleusRH.Base.Postulantes.Ofertas_Laborales.OFERTA_LAB objOfertaLab = Ofertas_Laborales.OFERTA_LAB.Get(pParam.GetAttr("oi_oferta_lab"));
                      objBusqueda.d_busqueda = objBusqueda.d_busqueda + ", " + objOfertaLab.c_oferta_lab+" - "+objOfertaLab.d_oferta_lab; 
                  }
                  else
                      objBusqueda.oi_oferta_labNull = true;   


                  //Saco la primer coma 
                  if(objBusqueda.d_busqueda.Length>0)
                    objBusqueda.d_busqueda = objBusqueda.d_busqueda.Substring(2);

                  
                  //Si es muy largo, lo corto
                  if (objBusqueda.d_busqueda.Length > 100)
                  {
                      string strCaracter = "";
                      int pos = 100;
                      while (strCaracter != ",")
                      {
                          strCaracter = objBusqueda.d_busqueda[pos].ToString();
                          pos--;
                      }
                      objBusqueda.d_busqueda = objBusqueda.d_busqueda.Substring(0, pos + 1);
                  }
                  objTrans = new NomadTransaction();
                  objTrans.Begin();
                  objTrans.SaveRefresh(objBusqueda);
                  objTrans.Commit();

                  strStep = "GET-BUSQUEDA";
                  string oi_busqueda = objBusqueda.Id;

                  strStep = "ARMO-XML-return";
                  xmlResult.SetAttr("oi_busqueda", oi_busqueda);
              //}
           }
          catch (Exception ex)
          {
              NomadException nmdEx = new NomadException("NucleusRH.Base.Postulantes.CV.WEB_CreateSearch()", ex);
              nmdEx.SetValue("Step", strStep);
              if (objTrans != null)
              {
                  objTrans.Rollback();
              }
    

              throw nmdEx;
          }

          NomadLog.Debug("WEB_CreateSearch.xmlResult: " + xmlResult.ToString());  
          return xmlResult;      
      }


      /// <summary>
      /// Metodo que recupera la lista de CVs. 
      /// Realiza busqueda de cv con parametro qryMode en LISTCV
      /// Realiza cantidad de CVs hasta 1001 con parametro COUNTALL
      /// Realiza el calculo de resumen con parametro campo RESUMEN y campo
      /// Retorna tambien las 5 carpetas mas recientes del Usuario
      /// Retorna todas las carpetas en un child separado
      /// </summary>
      /// <param name="pParam"></param>
      /// <returns></returns>
      public static NomadXML WEB_ListCVSearch(NomadXML pParam)
      {
          NomadLog.Debug("-----------------------------------------------");
          NomadLog.Debug("-----------------WEB_ListCVSearch V2-----------------");
          NomadLog.Debug("-----------------------------------------------");

          NomadLog.Debug("WEB_ListCVSearch.pParam: " + pParam.ToString());  

          NomadXML xmlResult;
          NomadXML xmlList;          
          NomadXML xmlItem;
          
          string strMode;
          string strStep = "";


          strMode = pParam.GetAttr("mode");
          xmlResult = new NomadXML("DATA");
          xmlResult.SetAttr("mode", strMode);
          try
          {

              strStep = "GET-BUSQUEDA";
              NucleusRH.Base.Postulantes.Busquedas.BUSQUEDA objBusqueda = Busquedas.BUSQUEDA.Get(pParam.GetAttr("oi_busqueda"));

              strStep = "GET-LIST";
              NomadXML xmlParamListCV = CreateXMLParamListCV(ref xmlResult, ref strStep, objBusqueda);
              
              xmlParamListCV.SetAttr("activeOrder", pParam.GetAttr("activeOrder"));
              xmlParamListCV.SetAttr("order", pParam.GetAttr("order"));   
       
              //ILT 01.11.2016 - Paginacion
              xmlParamListCV.SetAttr("fromrow", pParam.GetAttr("fromrow")); //fromrow es en base 0
              xmlParamListCV.SetAttr("maxsize", pParam.GetAttrInt("pagesize") + 1);
                            
              xmlParamListCV.SetAttr("qryMode", "LISTCV");
              NomadLog.Debug("xmlParamListCV-List: " + xmlParamListCV.ToString());
             
              strStep = "QUERY-LIST";
              NomadXML xmlResultList = Nomad.NSystem.Base.NomadEnvironment.QueryNomadXML(NucleusRH.Base.Postulantes.CV.CV.Resources.QRY_BUSCAR_CV, xmlParamListCV.ToString());
              xmlList = xmlResult.AddTailElement("LIST");
              xmlList.SetAttr("name", "LIST-CV");
              xmlList.SetAttr("mode", "LIST-CV");

              //ILT 01.11.2016 - Paginacion
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

              strStep = "READ-LIST";
              for (NomadXML row = xmlResultList.FirstChild().FirstChild(); row != null; row = row.Next())
              {
                xmlItem = xmlList.AddTailElement("CV");
                xmlItem.SetAttr("oi_cv", row.GetAttr("oi_cv"));                                    
                xmlItem.SetAttr("d_ape_y_nom", row.GetAttrString("d_ape_y_nom"));
                xmlItem.SetAttr("d_localidad", row.GetAttrString("d_localidad"));
                xmlItem.SetAttr("n_remuneracion", row.GetAttrString("n_remuneracion"));
                xmlItem.SetAttr("n_edad", row.GetAttrString("n_edad"));
                xmlItem.SetAttr("l_leido", row.GetAttrString("l_leido"));
                xmlItem.SetAttr("d_ultima_exp", row.GetAttrString("d_ultima_exp"));
                xmlItem.SetAttr("d_ultimo_estudio", row.GetAttrString("d_ultimo_estudio"));
                xmlItem.SetAttr("oi_foto", row.GetAttrString("oi_foto"));
                if (row.GetAttr("nota") != "")
                    xmlItem.SetAttr("nota", "1");
                else
                    xmlItem.SetAttr("nota", "0");
                xmlItem.SetAttr("estado_civil", row.GetAttrString("estado_civil"));
                xmlItem.SetAttr("d_email", row.GetAttrString("d_email"));
                xmlItem.SetAttr("c_pais_cel", row.GetAttrString("c_pais_cel"));
                xmlItem.SetAttr("c_area_cel", row.GetAttrString("c_area_cel"));
                xmlItem.SetAttr("te_celular", row.GetAttrString("te_celular"));
                xmlItem.SetAttr("c_pais", row.GetAttrString("c_pais"));
                xmlItem.SetAttr("c_area", row.GetAttrString("c_area"));
                xmlItem.SetAttr("te_nro", row.GetAttrString("te_nro"));
                xmlItem.SetAttr("d_pais", row.GetAttrString("d_pais"));
                xmlItem.SetAttr("d_provincia", row.GetAttrString("d_provincia"));
                xmlItem.SetAttr("d_calle", row.GetAttrString("d_calle"));
                xmlItem.SetAttr("c_nro", row.GetAttrString("c_nro"));
                xmlItem.SetAttr("c_piso", row.GetAttrString("c_piso"));
                xmlItem.SetAttr("c_departamento", row.GetAttrString("c_departamento"));
                xmlItem.SetAttr("c_tipo_documento", row.GetAttrString("c_tipo_documento"));
                xmlItem.SetAttr("c_nro_doc", row.GetAttrString("c_nro_doc"));
                xmlItem.SetAttr("c_sexo", row.GetAttrString("c_sexo"));
                xmlItem.SetAttr("f_actualizacion", row.GetAttrString("f_actualizacion"));                  
              }
               
             
              xmlList.SetAttr("campo_buscar", objBusqueda.o_palabras_clave);

              strStep = "QUERY-COUNTALL";
              xmlParamListCV.SetAttr("qryMode", "COUNTALL");
              NomadLog.Debug("xmlParamListCV-Cant: " + xmlParamListCV.ToString());
              NomadXML xmlResultCant = Nomad.NSystem.Base.NomadEnvironment.QueryNomadXML(NucleusRH.Base.Postulantes.CV.CV.Resources.QRY_BUSCAR_CV, xmlParamListCV.ToString());
              
              NomadXML xmlCant = xmlResult.AddTailElement("CANT-ALL");
              xmlCant.SetAttr("name", "CANT-ALL");              

              strStep = "READ-CANT";            
              xmlItem = xmlCant.AddTailElement("CANT");
              xmlItem.SetAttr("cantTotal", xmlResultCant.FirstChild().GetAttr("cantTotal"));                        
              
              //Actualizo cantidad del objeto busqueda
              objBusqueda.e_cant_res = xmlResultCant.FirstChild().GetAttrInt("cantTotal");
              NomadTransaction objTrans = new NomadTransaction();
              objTrans.Begin();
              objTrans.SaveRefresh(objBusqueda);
              objTrans.Commit();

              NomadXML xmlResumen = xmlResult.AddTailElement("RESUMEN");
              xmlResumen.SetAttr("name", "RESUMEN");
              xmlResumen.SetAttr("mode", "RESUMEN");

              if (objBusqueda.e_cant_res > 0)
              {
                      xmlParamListCV.SetAttr("qryMode", "RESUMEN");                  
                  
                      strStep = "GET-RESUMEN";                                 
                      //Para puestos
                      if (pParam.GetAttrInt("noCalculaPuesto") == 0)
                      {
                          strStep = "RESUMEN-PUESTO";
                          xmlParamListCV.SetAttr("campoResumen", "d_puesto");

                          NomadLog.Debug("xmlParamListCVResumen-Puesto: " + xmlParamListCV.ToString());

                          NomadXML xmlResultResumenPuesto = Nomad.NSystem.Base.NomadEnvironment.QueryNomadXML(NucleusRH.Base.Postulantes.CV.CV.Resources.QRY_BUSCAR_CV, xmlParamListCV.ToString());

                          xmlList = xmlResumen.AddTailElement("PUESTOS");
                          for (NomadXML row = xmlResultResumenPuesto.FirstChild().FirstChild(); row != null; row = row.Next())
                          {
                              NomadXML xmlTitulo = xmlList.AddTailElement("PUESTOS");
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
                          xmlParamListCV.SetAttr("campoResumen", "d_empresa");

                          NomadLog.Debug("xmlParamListCVResumen-Empresa: " + xmlParamListCV.ToString());

                          NomadXML xmlResultResumenEmpresas = Nomad.NSystem.Base.NomadEnvironment.QueryNomadXML(NucleusRH.Base.Postulantes.CV.CV.Resources.QRY_BUSCAR_CV, xmlParamListCV.ToString());

                          xmlList = xmlResumen.AddTailElement("EMPRESAS");
                          for (NomadXML row = xmlResultResumenEmpresas.FirstChild().FirstChild(); row != null; row = row.Next())
                          {
                              NomadXML xmlTitulo = xmlList.AddTailElement("EMPRESA");
                              xmlTitulo.SetAttr("desc", row.GetAttr("desc"));
                              xmlTitulo.SetAttr("cant", row.GetAttr("cant"));
                          }
                      }
                      else
                      {
                          xmlResumen.AddTailElement("EMPRESAS");
                      }

                       //Para Titulos
                      if (pParam.GetAttrInt("noCalculaTitulo") == 0)
                      {
                          strStep = "RESUMEN-TITULOS";
                          xmlParamListCV.SetAttr("campoResumen", "d_estudio");

                          NomadLog.Debug("xmlParamListCVResumen-Titulo: " + xmlParamListCV.ToString());

                          NomadXML xmlResultResumenEmpresas = Nomad.NSystem.Base.NomadEnvironment.QueryNomadXML(NucleusRH.Base.Postulantes.CV.CV.Resources.QRY_BUSCAR_CV, xmlParamListCV.ToString());

                          xmlList = xmlResumen.AddTailElement("TITULOS");
                          for (NomadXML row = xmlResultResumenEmpresas.FirstChild().FirstChild(); row != null; row = row.Next())
                          {
                              NomadXML xmlTitulo = xmlList.AddTailElement("TITULO");
                              xmlTitulo.SetAttr("desc", row.GetAttr("desc"));
                              xmlTitulo.SetAttr("cant", row.GetAttr("cant"));
                          }
                      }
                      else
                      {
                          xmlResumen.AddTailElement("TITULOS");
                      }

                      //Para Localidad
                      if (pParam.GetAttrInt("noCalculaLocalidad") == 0)
                      {
                          strStep = "RESUMEN-LOCALIDAD";
                          xmlParamListCV.SetAttr("campoResumen", "d_localidad");

                          NomadLog.Debug("xmlParamListCVResumen-Localidad: " + xmlParamListCV.ToString());

                          NomadXML xmlResultResumenEmpresas = Nomad.NSystem.Base.NomadEnvironment.QueryNomadXML(NucleusRH.Base.Postulantes.CV.CV.Resources.QRY_BUSCAR_CV, xmlParamListCV.ToString());

                          xmlList = xmlResumen.AddTailElement("LOCALIDADES");
                          for (NomadXML row = xmlResultResumenEmpresas.FirstChild().FirstChild(); row != null; row = row.Next())
                          {
                              NomadXML xmlTitulo = xmlList.AddTailElement("LOCALIDAD");
                              xmlTitulo.SetAttr("desc", row.GetAttr("desc"));
                              xmlTitulo.SetAttr("cant", row.GetAttr("cant"));
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
              
          
              //Antes de retornar, anexo ultimas 5 carpetas del usuario
              strStep = "GET-CARPETAS-USER";

              NomadXML xmlParamMisCarpetas = new NomadXML("DATA");
              NomadProxy proxy = NomadEnvironment.GetProxy();
              xmlParamMisCarpetas.SetAttr("oi_usuario_sistema", proxy.UserEtty);
              strStep = "GET:MIS-CARPETAS";
              NomadXML xmlResultMisCarpetas = Nomad.NSystem.Base.NomadEnvironment.QueryNomadXML(NucleusRH.Base.Postulantes.Carpetas.CARPETA.Resources.GET_MIS_CARPETAS, xmlParamMisCarpetas.ToString());

              xmlList = xmlResult.AddTailElement("CARPETAS");
              xmlList.SetAttr("name", "CARPETAS");             
              //Recorro las carpetas
              for (NomadXML row = xmlResultMisCarpetas.FirstChild().FirstChild(); row != null; row = row.Next())
              {
                  xmlItem = xmlList.AddTailElement("CARPETA");
                  xmlItem.SetAttr("id", row.GetAttrString("oi_carpeta"));
                  xmlItem.SetAttr("desc", row.GetAttrString("d_carpeta"));
                  xmlItem.SetAttr("f_carpeta", row.GetAttrString("f_carpeta"));
              }  
            
          }
          catch (Exception ex)
          {
              NomadException nmdEx = new NomadException("NucleusRH.Base.Postulantes.CV.WEB_ListCV()", ex);
              nmdEx.SetValue("Step", strStep);
              throw nmdEx;
          }


          NomadLog.Debug("xmlResult: " + xmlResult.ToString());
          return xmlResult;
         
      }

      /// <summary>
      /// Metodo interno privado que crea la el xml parametro para la tirar la busqueda de cv
      /// Ademas edita el xmlResult agregandoe cada una de las palabras claves asociadas a la busqueda
      /// May the force be with you
      /// </summary>
      /// <param name="xmlResult"></param>
      /// <param name="strStep"></param>
      /// <param name="objBusqueda"></param>
      /// <returns></returns>
      private static NomadXML CreateXMLParamListCV(ref NomadXML xmlResult, ref string strStep, NucleusRH.Base.Postulantes.Busquedas.BUSQUEDA objBusqueda)
      {

          NomadLog.Debug("-----------------------------------------------");
          NomadLog.Debug("-------------CreateXMLParamListCV V1------------");
          NomadLog.Debug("-----------------------------------------------");

          NomadLog.Debug("WEB_ListCVSearch.xmlResult: " + xmlResult.ToString());
          NomadLog.Debug("WEB_ListCVSearch.strStep: " + strStep.ToString());
          NomadLog.Debug("WEB_ListCVSearch.objBusqueda: " + Nomad.NSystem.Functions.StringUtil.Object2JSON(objBusqueda));  

          
          NomadXML xmlParamListCV = new NomadXML("DATA");

          //Xml para arreglo de palabras claves
          NomadXML xmlPalabrasClaves = xmlResult.AddTailElement("PALABRAS-CLAVES");
          xmlPalabrasClaves.SetAttr("name", "PCLAVES");
          xmlPalabrasClaves.SetAttr("mode", "PCLAVES");
          NomadXML xmlPClave;


          #region CampoLike
          strStep = "ARMO-PARAM-pclaves";
          if (!(objBusqueda.o_palabras_claveNull))
          {
              string strCampoLike = objBusqueda.o_palabras_clave;
              //Separo palabras
              string[] arrayPalabras = strCampoLike.Split(',');
              for (int i = 0; (i <= arrayPalabras.Length - 1 && i != 10); i++)
              {
                  string campo_like = limpiarCadena(arrayPalabras[i]);
                  xmlParamListCV.SetAttr("campo_like_" + (i + 1).ToString(), campo_like);
                  xmlPClave = xmlPalabrasClaves.AddTailElement("PCLAVE");
                  xmlPClave.SetAttr("campo", "o_palabras_claves");
                  xmlPClave.SetAttr("value", campo_like);
                  xmlPClave.SetAttr("label", campo_like);
              }
          }
          #endregion

          #region PanelGeneral
          strStep = "ARMO-PARAM-l_leido";
          if (objBusqueda.c_leido == "L")
          {
              xmlParamListCV.SetAttr("l_leido", "1");
              xmlPClave = xmlPalabrasClaves.AddTailElement("PCLAVE");
              xmlPClave.SetAttr("campo", "l_leido");
              xmlPClave.SetAttr("value", "1");
              xmlPClave.SetAttr("label", "CVs Leídos");
          }
          if (objBusqueda.c_leido == "NL")
          {
              xmlParamListCV.SetAttr("l_leido", "0");
              xmlPClave = xmlPalabrasClaves.AddTailElement("PCLAVE");
              xmlPClave.SetAttr("campo", "l_leido");
              xmlPClave.SetAttr("value", "0");
              xmlPClave.SetAttr("label", "CVs No Leídos");
          }

          strStep = "ARMO-PARAM-f_actualizacion";
          if (!(objBusqueda.f_actualizacionNull))
          {
              xmlParamListCV.SetAttr("f_actualizacion", objBusqueda.f_actualizacion);
              xmlPClave = xmlPalabrasClaves.AddTailElement("PCLAVE");
              xmlPClave.SetAttr("campo", "f_actualizacion");
              xmlPClave.SetAttr("value1", objBusqueda.f_actualizacion);
              xmlPClave.SetAttr("value2", objBusqueda.c_f_actualizacion);

              //Construyo el Label de la palabra clave en funcion dle c_f_actualizacion donde
              //a: Menor a 15 dias
              //b: Menor a 1 Mes
              //c: Menor a 3 Meses
              //d: Menor a 6 Meses
              switch (objBusqueda.c_f_actualizacion)
              {
                  case "a": { xmlPClave.SetAttr("label", "Fecha de Actualización Menor a 15 días"); } break;
                  case "b": { xmlPClave.SetAttr("label", "Fecha de Actualización Menor a 1 mes"); } break;
                  case "c": { xmlPClave.SetAttr("label", "Fecha de Actualización Menor a 3 mes"); } break;
                  case "d": { xmlPClave.SetAttr("label", "Fecha de Actualización Menor a 6 mes"); } break;
              }
          }
          #endregion

          #region ExperienciaLaboral
          strStep = "ARMO-PARAM-d_puesto";
          if (!(objBusqueda.d_puestoNull))
          {
              xmlParamListCV.SetAttr("d_puesto", limpiarCadena(objBusqueda.d_puesto));
              xmlPClave = xmlPalabrasClaves.AddTailElement("PCLAVE");
              xmlPClave.SetAttr("campo", "d_puesto");
              xmlPClave.SetAttr("value", objBusqueda.d_puesto);
              xmlPClave.SetAttr("label", objBusqueda.d_puesto);
          }

          strStep = "ARMO-PARAM-l_trab_puesto";
          if (objBusqueda.l_trab_puesto)
          {
              xmlParamListCV.SetAttr("l_trab_puesto", objBusqueda.l_trab_puesto);
              xmlPClave = xmlPalabrasClaves.AddTailElement("PCLAVE");
              xmlPClave.SetAttr("campo", "l_trab_puesto");
              xmlPClave.SetAttr("value", objBusqueda.l_trab_puesto);
              xmlPClave.SetAttr("label", "Trabajando Actualmente en el Puesto");
          }

          strStep = "ARMO-PARAM-e_anio_desde";
          if (!(objBusqueda.e_anio_desdeNull))
              xmlParamListCV.SetAttr("e_anio_desde", limpiarCadena(objBusqueda.e_anio_desde.ToString()));

          strStep = "ARMO-PARAM-e_anio_hasta";
          if (!(objBusqueda.e_anio_hastaNull))
              xmlParamListCV.SetAttr("e_anio_hasta", limpiarCadena(objBusqueda.e_anio_hasta.ToString()));


          if ((!(objBusqueda.e_anio_desdeNull)) && (!(objBusqueda.e_anio_hastaNull)))
          {
              xmlPClave = xmlPalabrasClaves.AddTailElement("PCLAVE");
              xmlPClave.SetAttr("campo", "ambosAniosExp");
              xmlPClave.SetAttr("value1", objBusqueda.e_anio_desde);
              xmlPClave.SetAttr("value2", objBusqueda.e_anio_hasta);
              xmlPClave.SetAttr("label", "Entre " + objBusqueda.e_anio_desde.ToString() + " y " + objBusqueda.e_anio_hasta.ToString() + " años de Experiencia");
          }

          if ((!(objBusqueda.e_anio_desdeNull)) && (objBusqueda.e_anio_hastaNull))
          {
              xmlPClave = xmlPalabrasClaves.AddTailElement("PCLAVE");
              xmlPClave.SetAttr("campo", "e_anio_desde");
              xmlPClave.SetAttr("value", objBusqueda.e_anio_desde);
              string palabra = objBusqueda.e_anio_desde == 1 ? "año" : "años";
              xmlPClave.SetAttr("label", "A partir de " + objBusqueda.e_anio_desde + " " + palabra + " de Experiencia");
          }

          if ((objBusqueda.e_anio_desdeNull) && (!(objBusqueda.e_anio_hastaNull)))
          {
              xmlPClave = xmlPalabrasClaves.AddTailElement("PCLAVE");
              xmlPClave.SetAttr("campo", "e_anio_hasta");
              xmlPClave.SetAttr("value", objBusqueda.e_anio_hasta);
              string palabra = objBusqueda.e_anio_hasta == 1 ? "año" : "años";
              xmlPClave.SetAttr("label", "Hasta " + objBusqueda.e_anio_hasta + " " + palabra + " de Experiencia");
          }


          strStep = "ARMO-PARAM-c_trabajando";
          if (!(objBusqueda.c_trabajandoNull))
          {
              xmlParamListCV.SetAttr("c_trabajando", objBusqueda.c_trabajando);
              xmlPClave = xmlPalabrasClaves.AddTailElement("PCLAVE");
              xmlPClave.SetAttr("campo", "c_trabajando");
              xmlPClave.SetAttr("value", objBusqueda.c_trabajando);
              xmlPClave.SetAttr("label", "Trabajando Actualmente");
          }

          strStep = "ARMO-PARAM-areasLab";
          string paramAreasLab = "IN(0";
          foreach (NucleusRH.Base.Postulantes.Busquedas.AREA_PUESTO objAreaPuesto in objBusqueda.A_PUESTO_BUS)
          {
              paramAreasLab = paramAreasLab + "," + objAreaPuesto.oi_area_lab;
          }
          paramAreasLab = paramAreasLab + ")";
          if (paramAreasLab != "IN(0)")
          {
              xmlParamListCV.SetAttr("paramAreasLab", paramAreasLab);
              foreach (NucleusRH.Base.Postulantes.Busquedas.AREA_PUESTO objAreaPuesto in objBusqueda.A_PUESTO_BUS)
              {
                  xmlPClave = xmlPalabrasClaves.AddTailElement("PCLAVE");
                  xmlPClave.SetAttr("campo", "oi_area_lab");
                  xmlPClave.SetAttr("value", objAreaPuesto.oi_area_lab);
                  NucleusRH.Base.Postulantes.Areas_Laborales.AREA_LAB objAreaLab = Areas_Laborales.AREA_LAB.Get(objAreaPuesto.oi_area_lab);
                  xmlPClave.SetAttr("label", objAreaLab.d_area_lab);
              }
          }

          NomadLog.Debug("paramAreasLab: " + paramAreasLab.ToString());

          strStep = "ARMO-PARAM-d_empresa";
          if (!(objBusqueda.d_empresaNull))
          {
              //hasta 10 empresas                                    
              string[] empresas = objBusqueda.d_empresa.Split(',');
              for (int i = 0; (i <= empresas.Length - 1 && i != 10); i++)
              {
                  string empresa = limpiarCadena(empresas[i]);
                  xmlParamListCV.SetAttr("empresa_" + (i + 1).ToString(), empresa);
                  xmlPClave = xmlPalabrasClaves.AddTailElement("PCLAVE");
                  xmlPClave.SetAttr("campo", "d_empresa");
                  xmlPClave.SetAttr("value", empresa);
                  xmlPClave.SetAttr("label", empresa);
              }
          }

          strStep = "ARMO-PARAM-c_persona_Cargo";
          if (!(objBusqueda.c_persona_cargoNull))
          {
              xmlParamListCV.SetAttr("c_persona_cargo", objBusqueda.c_persona_cargo);
              xmlPClave = xmlPalabrasClaves.AddTailElement("PCLAVE");
              xmlPClave.SetAttr("campo", "c_persona_cargo");
              xmlPClave.SetAttr("value", objBusqueda.c_persona_cargo);
              xmlPClave.SetAttr("label", "Con Personas a Cargo");
          }

          #endregion

          #region Educacion
          strStep = "ARMO-PARAM-d_titulo";
          if (!(objBusqueda.d_tituloNull))
          {
              xmlParamListCV.SetAttr("d_titulo", limpiarCadena(objBusqueda.d_titulo));
              xmlPClave = xmlPalabrasClaves.AddTailElement("PCLAVE");
              xmlPClave.SetAttr("campo", "d_titulo");
              xmlPClave.SetAttr("value", objBusqueda.d_titulo);
              xmlPClave.SetAttr("label", objBusqueda.d_titulo);
          }

          strStep = "ARMO-PARAM-oi_nivel_estudio";
          if (!(objBusqueda.oi_nivel_estudioNull))
          {
              xmlParamListCV.SetAttr("oi_nivel_estudio", objBusqueda.oi_nivel_estudio);
              xmlPClave = xmlPalabrasClaves.AddTailElement("PCLAVE");
              xmlPClave.SetAttr("campo", "oi_nivel_estudio");
              xmlPClave.SetAttr("value", objBusqueda.oi_nivel_estudio);
              NucleusRH.Base.Personal.Niveles_Estudio.NIVEL_ESTUDIO objNivelEst = Personal.Niveles_Estudio.NIVEL_ESTUDIO.Get(objBusqueda.oi_nivel_estudio);
              xmlPClave.SetAttr("label", "Estudio " + objNivelEst.d_nivel_estudio);
          }

          strStep = "ARMO-PARAM-oi_estado_est";
          if (!(objBusqueda.oi_estado_estNull))
          {
              xmlParamListCV.SetAttr("oi_estado_est", objBusqueda.oi_estado_est);
              xmlPClave = xmlPalabrasClaves.AddTailElement("PCLAVE");
              xmlPClave.SetAttr("campo", "oi_estado_est");
              xmlPClave.SetAttr("value", objBusqueda.oi_estado_est);
              NucleusRH.Base.Personal.Estados_Estudio.ESTADO_EST objEstadoEst = Personal.Estados_Estudio.ESTADO_EST.Get(objBusqueda.oi_estado_est);
              xmlPClave.SetAttr("label", "Estudio " + objEstadoEst.d_estado_est);
          }

          strStep = "ARMO-PARAM-idiomas";
          foreach (NucleusRH.Base.Postulantes.Busquedas.IDIOMAS objIdioma in objBusqueda.IDIOMA_BUS)
          {
              xmlParamListCV.SetAttr("oi_idioma_" + objIdioma.oi_idioma.ToString(), objIdioma.oi_idioma);
              xmlParamListCV.SetAttr("oi_nivel_general_" + objIdioma.oi_idioma.ToString(), objIdioma.oi_nivel_general);

              xmlPClave = xmlPalabrasClaves.AddTailElement("PCLAVE");
              xmlPClave.SetAttr("campo", "oi_idioma");
              xmlPClave.SetAttr("value1", objIdioma.oi_idioma);
              xmlPClave.SetAttr("value2", objIdioma.oi_nivel_general);
              NucleusRH.Base.Organizacion.Idiomas.IDIOMA objIdi = Organizacion.Idiomas.IDIOMA.Get(objIdioma.oi_idioma);
              NomadLog.Debug("IdiomaLabel: " + objIdi.d_idioma);
              xmlPClave.SetAttr("label", objIdi.d_idioma);

          }
          #endregion

          #region Datos Personales
          strStep = "ARMO-PARAM-d_nombres";
          if (!(objBusqueda.d_nombresNull))
          {
              xmlParamListCV.SetAttr("d_nombres", limpiarCadena(objBusqueda.d_nombres));
              xmlPClave = xmlPalabrasClaves.AddTailElement("PCLAVE");
              xmlPClave.SetAttr("campo", "d_nombres");
              xmlPClave.SetAttr("value", objBusqueda.d_nombres);
              xmlPClave.SetAttr("label", objBusqueda.d_nombres);
          }

          strStep = "ARMO-PARAM-d_apellido";
          if (!(objBusqueda.d_apellidoNull))
          {
              xmlParamListCV.SetAttr("d_apellido", limpiarCadena(objBusqueda.d_apellido));
              xmlPClave = xmlPalabrasClaves.AddTailElement("PCLAVE");
              xmlPClave.SetAttr("campo", "d_apellido");
              xmlPClave.SetAttr("value", objBusqueda.d_apellido);
              xmlPClave.SetAttr("label", objBusqueda.d_apellido);
          }


          strStep = "ARMO-PARAM-c_sexo";
          if (!(objBusqueda.c_sexoNull))
          {
              xmlParamListCV.SetAttr("c_sexo", objBusqueda.c_sexo);
              xmlPClave = xmlPalabrasClaves.AddTailElement("PCLAVE");
              xmlPClave.SetAttr("campo", "c_sexo");
              xmlPClave.SetAttr("value", objBusqueda.c_sexo);
              xmlPClave.SetAttr("label", objBusqueda.c_sexo == "F" ? "Femenino" : "Masculino");
          }

          strStep = "ARMO-PARAM-c_nro_doc";
          if (!(objBusqueda.c_nro_docNull))
          {
              xmlParamListCV.SetAttr("c_nro_doc", limpiarCadena(objBusqueda.c_nro_doc));
              xmlPClave = xmlPalabrasClaves.AddTailElement("PCLAVE");
              xmlPClave.SetAttr("campo", "c_nro_doc");
              xmlPClave.SetAttr("value", objBusqueda.c_nro_doc);
              xmlPClave.SetAttr("label", objBusqueda.c_nro_doc);
          }

          strStep = "ARMO-PARAM-e_edad_desde";
          if (!(objBusqueda.e_edad_desdeNull))
              xmlParamListCV.SetAttr("e_edad_desde", limpiarCadena(objBusqueda.e_edad_desde.ToString()));

          strStep = "ARMO-PARAM-e_edad_hasta";
          if (!(objBusqueda.e_edad_hastaNull))
              xmlParamListCV.SetAttr("e_edad_hasta", limpiarCadena(objBusqueda.e_edad_hasta.ToString()));


          if ((!(objBusqueda.e_edad_desdeNull)) && (!(objBusqueda.e_edad_hastaNull)))
          {
              xmlPClave = xmlPalabrasClaves.AddTailElement("PCLAVE");
              xmlPClave.SetAttr("campo", "ambosEdad");
              xmlPClave.SetAttr("value1", objBusqueda.e_edad_desde);
              xmlPClave.SetAttr("value2", objBusqueda.e_edad_hasta);
              xmlPClave.SetAttr("label", "Entre " + objBusqueda.e_edad_desde.ToString() + " y " + objBusqueda.e_edad_hasta.ToString() + " años de Edad");
          }

          if ((!(objBusqueda.e_edad_desdeNull)) && (objBusqueda.e_edad_hastaNull))
          {
              xmlPClave = xmlPalabrasClaves.AddTailElement("PCLAVE");
              xmlPClave.SetAttr("campo", "e_edad_desde");
              xmlPClave.SetAttr("value", objBusqueda.e_edad_desde);
              string palabra = objBusqueda.e_edad_desde == 1 ? "año" : "años";
              xmlPClave.SetAttr("label", "A partir de " + objBusqueda.e_edad_desde + " " + palabra + " de Edad");
          }

          if ((objBusqueda.e_edad_desdeNull) && (!(objBusqueda.e_edad_hastaNull)))
          {
              xmlPClave = xmlPalabrasClaves.AddTailElement("PCLAVE");
              xmlPClave.SetAttr("campo", "e_edad_hasta");
              xmlPClave.SetAttr("value", objBusqueda.e_edad_hasta);
              string palabra = objBusqueda.e_edad_hasta == 1 ? "año" : "años";
              xmlPClave.SetAttr("label", "Hasta " + objBusqueda.e_edad_hasta + " " + palabra + " de Edad");
          }

          strStep = "ARMO-PARAM-oi_pais";
          if (!(objBusqueda.oi_paisNull))
          {
              xmlParamListCV.SetAttr("oi_pais", objBusqueda.oi_pais);
              xmlPClave = xmlPalabrasClaves.AddTailElement("PCLAVE");
              xmlPClave.SetAttr("campo", "oi_pais");
              xmlPClave.SetAttr("value", objBusqueda.oi_pais);
              NucleusRH.Base.Organizacion.Paises.PAIS objPais = Organizacion.Paises.PAIS.Get(objBusqueda.oi_pais);
              xmlPClave.SetAttr("label", objPais.d_pais);
          }

          //Pcia
          strStep = "ARMO-PARAM-pcia";
          string paramProvincia = "IN(0";
          foreach (NucleusRH.Base.Postulantes.Busquedas.PROVINCIAS objProvincia in objBusqueda.PCIA_BUS)
          {
              paramProvincia = paramProvincia + "," + objProvincia.oi_provincia;
          }
          paramProvincia = paramProvincia + ")";
          if (paramProvincia != "IN(0)")
              xmlParamListCV.SetAttr("paramProvincia", paramProvincia);


          foreach (NucleusRH.Base.Postulantes.Busquedas.PROVINCIAS objProvincia in objBusqueda.PCIA_BUS)
          {
              xmlPClave = xmlPalabrasClaves.AddTailElement("PCLAVE");
              xmlPClave.SetAttr("campo", "oi_provincia");
              xmlPClave.SetAttr("value", objProvincia.oi_provincia);
              NucleusRH.Base.Organizacion.Paises.PROVINCIA objPcia = Organizacion.Paises.PROVINCIA.Get(objProvincia.oi_provincia);
              xmlPClave.SetAttr("label", objPcia.d_provincia);
          }


          strStep = "ARMO-PARAM-d_ciudad";
          if (!(objBusqueda.d_ciudadNull))
          {
              xmlParamListCV.SetAttr("d_ciudad", limpiarCadena(objBusqueda.d_ciudad));
              xmlPClave = xmlPalabrasClaves.AddTailElement("PCLAVE");
              xmlPClave.SetAttr("campo", "d_ciudad");
              xmlPClave.SetAttr("value", objBusqueda.d_ciudad);
              xmlPClave.SetAttr("label", objBusqueda.d_ciudad);
          }

          strStep = "ARMO-PARAM-l_foto";
          if (objBusqueda.l_foto)
          {
              xmlParamListCV.SetAttr("l_foto", objBusqueda.l_foto);
              xmlPClave = xmlPalabrasClaves.AddTailElement("PCLAVE");
              xmlPClave.SetAttr("campo", "l_foto");
              xmlPClave.SetAttr("value", objBusqueda.l_foto);
              xmlPClave.SetAttr("label", "Sólo CVs con Foto");
          }

          //Estados CV
          strStep = "ARMO-PARAM-estado-cv";
          string paramEstadoCV = "IN(\\'0\\'";
          foreach (NucleusRH.Base.Postulantes.Busquedas.ESTADOS_CV objEstadoCV in objBusqueda.ESTADOS_CV_BUS)
          {
              paramEstadoCV = paramEstadoCV + "," + "\\'" + objEstadoCV.c_estado + "\\'";
          }
          paramEstadoCV = paramEstadoCV + ")";
          if (paramEstadoCV != "IN(\\'0\\')")
              xmlParamListCV.SetAttr("paramEstadoCV", paramEstadoCV);


          foreach (NucleusRH.Base.Postulantes.Busquedas.ESTADOS_CV objEstadoCV in objBusqueda.ESTADOS_CV_BUS)
          {
              xmlPClave = xmlPalabrasClaves.AddTailElement("PCLAVE");
              xmlPClave.SetAttr("campo", "c_estado");
              xmlPClave.SetAttr("value", objEstadoCV.c_estado);
              xmlPClave.SetAttr("label", objEstadoCV.d_estado);
          }


          //Oferta Laboral
          strStep = "ARMO-PARAM-oi_oferta_lab";
          if (!(objBusqueda.oi_oferta_labNull))
          {
              xmlParamListCV.SetAttr("oi_oferta_lab", objBusqueda.oi_oferta_lab);
              xmlPClave = xmlPalabrasClaves.AddTailElement("PCLAVE");
              xmlPClave.SetAttr("campo", "oi_oferta_lab");
              xmlPClave.SetAttr("value", objBusqueda.oi_oferta_lab);
              NucleusRH.Base.Postulantes.Ofertas_Laborales.OFERTA_LAB objOfertaLab = Ofertas_Laborales.OFERTA_LAB.Get(objBusqueda.oi_oferta_lab);
              xmlPClave.SetAttr("label", objOfertaLab.c_oferta_lab + " - " + objOfertaLab.d_oferta_lab);
          }
          #endregion

          #region RangoSalarial
          strStep = "ARMO-PARAM-n_salario_desde";
          if (!(objBusqueda.n_salario_desdeNull))
              xmlParamListCV.SetAttr("n_salario_desde", limpiarCadena(objBusqueda.n_salario_desde.ToString()));

          strStep = "ARMO-PARAM-n_salario_hasta";
          if (!(objBusqueda.n_salario_hastaNull))
              xmlParamListCV.SetAttr("n_salario_hasta", limpiarCadena(objBusqueda.n_salario_hasta.ToString()));


          if ((!(objBusqueda.n_salario_desdeNull)) && (!(objBusqueda.n_salario_hastaNull)))
          {
              xmlPClave = xmlPalabrasClaves.AddTailElement("PCLAVE");
              xmlPClave.SetAttr("campo", "ambosSalario");
              xmlPClave.SetAttr("value1", objBusqueda.n_salario_desde);
              xmlPClave.SetAttr("value2", objBusqueda.n_salario_hasta);
              xmlPClave.SetAttr("label", "Salario entre " + objBusqueda.n_salario_desde.ToString() + " y " + objBusqueda.n_salario_hasta.ToString());
          }

          if ((!(objBusqueda.n_salario_desdeNull)) && (objBusqueda.n_salario_hastaNull))
          {
              xmlPClave = xmlPalabrasClaves.AddTailElement("PCLAVE");
              xmlPClave.SetAttr("campo", "n_salario_desde");
              xmlPClave.SetAttr("value", objBusqueda.n_salario_desde);
              xmlPClave.SetAttr("label", "Salario a partir de " + objBusqueda.n_salario_desde);
          }

          if ((objBusqueda.n_salario_desdeNull) && (!(objBusqueda.n_salario_hastaNull)))
          {
              xmlPClave = xmlPalabrasClaves.AddTailElement("PCLAVE");
              xmlPClave.SetAttr("campo", "n_salario_hasta");
              xmlPClave.SetAttr("value", objBusqueda.n_salario_hasta);
              xmlPClave.SetAttr("label", "Salario hasta " + objBusqueda.n_salario_hasta.ToString());
          }


          strStep = "ARMO-PARAM-lcv_sin_salario";
          NomadLog.Debug("l_cv_sin_salario: " + objBusqueda.l_cv_sin_salario.ToString());
          if (objBusqueda.l_cv_sin_salario)
          {
              xmlParamListCV.SetAttr("l_cv_sin_salario", objBusqueda.l_cv_sin_salario);
              xmlPClave = xmlPalabrasClaves.AddTailElement("PCLAVE");
              xmlPClave.SetAttr("campo", "l_cv_sin_salario");
              xmlPClave.SetAttr("value", objBusqueda.l_cv_sin_salario);
              xmlPClave.SetAttr("label", "Mostrando CVs sin salario");
          }
          #endregion

          NomadLog.Debug("WEB_Internal.xmlParamListCV: " + xmlParamListCV.ToString());
          return xmlParamListCV;
      }

      /// <summary>
      /// Carga los valores de los controles dinamicos de la pantalla de busqueda avanzada.
      /// Tambien maneja la edicion de la busqueda avanzada recuperando todos los campos de busqueda y setando los que estan completos en el xml a retornar
      /// -Areas de Puesto
      /// -idiomas
      /// -Provincias
      /// </summary>
      /// <param name="pParam"></param>
      /// <returns></returns>
      public static NomadXML WEB_LoadAdvancedSearch(NomadXML pParam)
      {
          NomadLog.Debug("-----------------------------------------------");
          NomadLog.Debug("----------WEB_LoadAdvancedSearch V1------------");
          NomadLog.Debug("-----------------------------------------------");

          NomadLog.Debug("WEB_LoadAdvancedSearch.pParam: " + pParam.ToString());

          NomadXML xmlResult;
          string strMode;
          string strStep="";

          strMode = pParam.GetAttr("mode");
          xmlResult = new NomadXML("DATA");          
          xmlResult.SetAttr("mode", strMode);
          
          try
          {
              strStep = "GET-LOAD";
               NomadXML xmlLoad = Nomad.NSystem.Base.NomadEnvironment.QueryNomadXML(NucleusRH.Base.Postulantes.CV.CV.Resources.QRY_LOAD_ADVSEARCH,"");

              #region AREAS-LAB
               strStep = "AREAS-LAB";
              NomadXML xmlAreasLaborales = xmlResult.AddTailElement("AREAS-LAB");
              xmlAreasLaborales.SetAttr("name", "AREASLAB");
              xmlAreasLaborales.SetAttr("mode", "LOAD-AL");
              
              for (NomadXML row = xmlLoad.FirstChild().FindElement("AREAS-LAB").FirstChild(); row != null; row = row.Next())
              {
                  NomadXML xmlAreaLab = xmlAreasLaborales.AddTailElement("AL");
                  xmlAreaLab.SetAttr("id", row.GetAttr("id"));
                  xmlAreaLab.SetAttr("desc", row.GetAttr("desc"));
              }

               #endregion

              #region NIVEL-EST
              strStep = "NIVEL-EST";           
              NomadXML xmlNivelEstudios = xmlResult.AddTailElement("NIVELES-ESTUDIO");
              xmlNivelEstudios.SetAttr("name", "NIVELESESTUDIO");
              xmlNivelEstudios.SetAttr("mode", "LOAD-NE");
              for (NomadXML row = xmlLoad.FirstChild().FindElement("NIVELES-ESTUDIO").FirstChild(); row != null; row = row.Next())
              {
                  NomadXML xmlNivelEst = xmlNivelEstudios.AddTailElement("ESTADOEST");
                  xmlNivelEst.SetAttr("id", row.GetAttr("id"));
                  xmlNivelEst.SetAttr("desc", row.GetAttr("desc"));
              }

              #endregion
              
              #region ESTADO-EST
              strStep = "ESTADO-EST";
              NomadXML xmlEstadosEstudio = xmlResult.AddTailElement("ESTADOS-ESTUDIO");  
              xmlEstadosEstudio.SetAttr("name", "ESTADOSESTUDIO");
              xmlEstadosEstudio.SetAttr("mode", "LOAD-EE");
              for (NomadXML row = xmlLoad.FirstChild().FindElement("ESTADOS-ESTUDIO").FirstChild(); row != null; row = row.Next())
              {
                  NomadXML xmlEstadoEst = xmlEstadosEstudio.AddTailElement("ESTADOEST");
                  xmlEstadoEst.SetAttr("id", row.GetAttr("id"));
                  xmlEstadoEst.SetAttr("desc", row.GetAttr("desc"));
              }

              #endregion

              #region NIVELES-IDIOMAS
              strStep = "NIVELES-IDIOMA";
              NomadXML xmlNivelesIdioma = xmlResult.AddTailElement("NIVELES-IDIOMA");
              xmlNivelesIdioma.SetAttr("name", "NIVID");
              xmlNivelesIdioma.SetAttr("mode", "LOAD-NI");
              for (NomadXML row = xmlLoad.FirstChild().FindElement("NIVELES-IDIOMA").FirstChild(); row != null; row = row.Next())
              {
                  NomadXML xmlNivelId = xmlNivelesIdioma.AddTailElement("NIVID");
                  xmlNivelId.SetAttr("id", row.GetAttr("id"));
                  xmlNivelId.SetAttr("desc", row.GetAttr("desc"));
              }

              #endregion

              #region PAISES
              strStep = "PAISES";
              NomadXML xmlPaises= xmlResult.AddTailElement("PAISES");
              xmlPaises.SetAttr("name", "PAIS");
              xmlPaises.SetAttr("mode", "LOAD-PAIS");
              for (NomadXML row = xmlLoad.FirstChild().FindElement("PAISES").FirstChild(); row != null; row = row.Next())
              {
                  NomadXML xmlPais = xmlPaises.AddTailElement("PAIS");
                  xmlPais.SetAttr("id", row.GetAttr("id"));
                  xmlPais.SetAttr("desc", row.GetAttr("desc"));
              }
              #endregion

              #region PROVINCIAS
              strStep = "PROVINCIAS";
              NomadXML xmlPcias = xmlResult.AddTailElement("PROVINCIAS");
              xmlPcias.SetAttr("name", "PROVINCIA");
              xmlPcias.SetAttr("mode", "LOAD-PROVINCIAS");
              for (NomadXML row = xmlLoad.FirstChild().FindElement("PROVINCIAS").FirstChild(); row != null; row = row.Next())
              {
                  NomadXML xmlPcia = xmlPcias.AddTailElement("PCIA");
                  xmlPcia.SetAttr("id", row.GetAttr("id"));
                  xmlPcia.SetAttr("desc", row.GetAttr("desc"));
              }
              #endregion
              
              #region EDIT-BUSQUEDA
              NomadXML xmlEditBusqueda = xmlResult.AddHeadElement("EDIT-BUSQUEDA");
              xmlEditBusqueda.SetAttr("name", "EDIT-BUSQUEDA");
              if (pParam.GetAttr("oiBusqueda") != "")
              {     
              
                int oi_busqueda = pParam.GetAttrInt("oiBusqueda");
              
                NucleusRH.Base.Postulantes.Busquedas.BUSQUEDA objEditBusqueda = Busquedas.BUSQUEDA.Get(oi_busqueda);

             
              
              #region EDIT-BUSQUEDA-EXPERIENCIA-LABORAL
              strStep = "EDIT-d_puesto";
              if (!(objEditBusqueda.d_puestoNull))
              {
                  xmlEditBusqueda.SetAttr("d_puesto", objEditBusqueda.d_puesto);
              }
              strStep = "EDIT-l_trab_puesto";
              if (objEditBusqueda.l_trab_puesto)
              {
                  xmlEditBusqueda.SetAttr("l_trab_puesto", objEditBusqueda.l_trab_puesto);
              }

              strStep = "EDIT-e_anio_desde";
              if (!(objEditBusqueda.e_anio_desdeNull))
              {
                  xmlEditBusqueda.SetAttr("e_anio_desde", objEditBusqueda.e_anio_desde);
              }

              strStep = "EDIT-e_anio_hasta";
              if (!(objEditBusqueda.e_anio_hastaNull))
              {
                  xmlEditBusqueda.SetAttr("e_anio_hasta", objEditBusqueda.e_anio_hasta);
              }

              strStep = "EDIT-c_trabajando";
              if (!(objEditBusqueda.c_trabajandoNull))
              {
                  xmlEditBusqueda.SetAttr("c_trabajando", objEditBusqueda.c_trabajando);
              }
              strStep = "EDIT-areasLab";
              if (objEditBusqueda.A_PUESTO_BUS.Count > 0)
              {
                  NomadXML xmlEditBusquedaAreaLab = xmlEditBusqueda.AddTailElement("EDIT-AREAS-LAB");
                  xmlEditBusquedaAreaLab.SetAttr("name", "EDIT-AREAS-LAB");
                  foreach (NucleusRH.Base.Postulantes.Busquedas.AREA_PUESTO objBusAreaLab in objEditBusqueda.A_PUESTO_BUS)
                  {
                      NomadXML xmlEditAreaLab = xmlEditBusquedaAreaLab.AddTailElement("EDITAL");
                      xmlEditAreaLab.SetAttr("oi_area_lab", objBusAreaLab.oi_area_lab);
                      NucleusRH.Base.Postulantes.Areas_Laborales.AREA_LAB objAuxAL = Areas_Laborales.AREA_LAB.Get(objBusAreaLab.oi_area_lab);
                      xmlEditAreaLab.SetAttr("d_area_lab", objAuxAL.d_area_lab);
                  }
              }
              else
              {
                  xmlEditBusqueda.SetAttr("sin_area_lab", "1");
              }
              
              strStep = "EDIT-d_empresa";
              if (!(objEditBusqueda.d_empresaNull))
              {
                  xmlEditBusqueda.SetAttr("d_empresa", objEditBusqueda.d_empresa);
              }

              strStep = "EDIT-c_persona_cargo";
              if (!(objEditBusqueda.c_persona_cargoNull))
              {
                  xmlEditBusqueda.SetAttr("c_persona_cargo", objEditBusqueda.c_persona_cargo);
              }
              #endregion
              
              #region EDIT-BUSQUEDA-PALABRAS-CLAVES
              strStep = "EDIT-o_palabras_clave";
              if (!(objEditBusqueda.o_palabras_claveNull))
              {
                  xmlEditBusqueda.SetAttr("o_palabras_clave", objEditBusqueda.o_palabras_clave);
              }
              #endregion

              #region EDIT-BUSQUEDA-ESTUDIOS
              strStep = "EDIT-d_titulo";
              if (!(objEditBusqueda.d_tituloNull))
              {
                  xmlEditBusqueda.SetAttr("d_titulo", objEditBusqueda.d_titulo);
              }

              strStep = "EDIT-oi_nivel_estudio";
              if (!(objEditBusqueda.oi_nivel_estudioNull))
              {
                  xmlEditBusqueda.SetAttr("oi_nivel_estudio", objEditBusqueda.oi_nivel_estudio);
              }

              strStep = "EDIT-oi_estado_est";
              if (!(objEditBusqueda.oi_estado_estNull))
              {
                  xmlEditBusqueda.SetAttr("oi_estado_est", objEditBusqueda.oi_estado_est);
              }

              strStep = "EDIT-idiomas";
              if (objEditBusqueda.IDIOMA_BUS.Count > 0)
              {
                  NomadXML xmlEditBusquedaIdioma = xmlEditBusqueda.AddTailElement("EDIT-IDIOMAS");
                  xmlEditBusquedaIdioma.SetAttr("name", "EDIT-IDIOMAS");
                  foreach (NucleusRH.Base.Postulantes.Busquedas.IDIOMAS objBusIdioma in objEditBusqueda.IDIOMA_BUS)
                  {
                      NomadXML xmlEditIdioma = xmlEditBusquedaIdioma.AddTailElement("EDITIDIOMA");
                      xmlEditIdioma.SetAttr("oi_idioma", objBusIdioma.oi_idioma);
                      xmlEditIdioma.SetAttr("oi_nivel_general", objBusIdioma.oi_nivel_general);
                  }
              }
              else
              {
                  xmlEditBusqueda.SetAttr("sin_idiomas", "1");
              }

              #endregion

              #region EDIT-BUSQUEDA-DATOS-PERSONALES
              strStep = "EDIT-d_nombres";
              if (!(objEditBusqueda.d_nombresNull))
              {
                  xmlEditBusqueda.SetAttr("d_nombres", objEditBusqueda.d_nombres);
              }

              strStep = "EDIT-d_apellido";
              if (!(objEditBusqueda.d_apellidoNull))
              {
                  xmlEditBusqueda.SetAttr("d_apellido", objEditBusqueda.d_apellido);
              }

              strStep = "EDIT-c_nro_doc";
              if (!(objEditBusqueda.c_nro_docNull))
              {
                  xmlEditBusqueda.SetAttr("c_nro_doc", objEditBusqueda.c_nro_doc);
              }

              strStep = "EDIT-c_sexo";
              if (!(objEditBusqueda.c_sexoNull))
              {
                  xmlEditBusqueda.SetAttr("c_sexo", objEditBusqueda.c_sexo);
              }
              
              strStep = "EDIT-e_edad_desde";
              if (!(objEditBusqueda.e_edad_desdeNull))
              {
                  xmlEditBusqueda.SetAttr("e_edad_desde", objEditBusqueda.e_edad_desde);
              }

              strStep = "EDIT-e_edad_hasta";
              if (!(objEditBusqueda.e_edad_hastaNull))
              {
                  xmlEditBusqueda.SetAttr("e_edad_hasta", objEditBusqueda.e_edad_hasta);
              }

              strStep = "EDIT-oi_pais";
              if (!(objEditBusqueda.oi_paisNull))
              {
                  xmlEditBusqueda.SetAttr("oi_pais", objEditBusqueda.oi_pais);
              }

              strStep = "EDIT-Provincias";
              if (objEditBusqueda.PCIA_BUS.Count > 0)
              {
                  NomadXML xmlEditBusquedaProvincia = xmlEditBusqueda.AddTailElement("EDIT-PROVINCIAS");
                  xmlEditBusquedaProvincia.SetAttr("name", "EDIT-PROVINCIAS");
                  foreach (NucleusRH.Base.Postulantes.Busquedas.PROVINCIAS objBusPcia in objEditBusqueda.PCIA_BUS)
                  {
                      NomadXML xmlEditPcia = xmlEditBusquedaProvincia.AddTailElement("EDITPCIA");
                      xmlEditPcia.SetAttr("oi_provincia", objBusPcia.oi_provincia);
                      NucleusRH.Base.Organizacion.Paises.PROVINCIA objAuxPcia = Organizacion.Paises.PROVINCIA.Get(objBusPcia.oi_provincia);
                      xmlEditPcia.SetAttr("d_provincia", objAuxPcia.d_provincia);
                  }
              }
              else
              {
                  xmlEditBusqueda.SetAttr("sin_pcia", "1");
              }

              strStep = "EDIT-d_ciudad";
              if (!(objEditBusqueda.d_ciudadNull))
              {
                  xmlEditBusqueda.SetAttr("d_ciudad", objEditBusqueda.d_ciudad);
              }


              strStep = "EDIT-l_foto";
              if (objEditBusqueda.l_foto)
              {
                  xmlEditBusqueda.SetAttr("l_foto", objEditBusqueda.l_foto);
              }


              strStep = "EDIT-EstadoCV";
              if (objEditBusqueda.ESTADOS_CV_BUS.Count > 0)
              {
                  NomadXML xmlEditBusquedaEstadoCV = xmlEditBusqueda.AddTailElement("EDIT-ESTADOS-CV");
                  xmlEditBusquedaEstadoCV.SetAttr("name", "EDIT-ESTADOS-CVS");
                  foreach (NucleusRH.Base.Postulantes.Busquedas.ESTADOS_CV objEstadoCV in objEditBusqueda.ESTADOS_CV_BUS)
                  {
                      NomadXML xmlEditECV = xmlEditBusquedaEstadoCV.AddTailElement("EDITECV");
                      xmlEditECV.SetAttr("c_estado", objEstadoCV.c_estado);
                      xmlEditECV.SetAttr("d_estado", objEstadoCV.d_estado);
                  }
              }
              else
              {
                  xmlEditBusqueda.SetAttr("sin_estado_cv", "1");
              }

              strStep = "EDIT-oi_oferta_lab";
              if (!(objEditBusqueda.oi_oferta_labNull))
              {
                  xmlEditBusqueda.SetAttr("oi_oferta_lab", objEditBusqueda.oi_oferta_lab);
                  NucleusRH.Base.Postulantes.Ofertas_Laborales.OFERTA_LAB objOfertaLab = Ofertas_Laborales.OFERTA_LAB.Get(objEditBusqueda.oi_oferta_lab);
                  xmlEditBusqueda.SetAttr("oferta_lab_mostrar", objOfertaLab.c_oferta_lab + " - " + objOfertaLab.d_oferta_lab);
              }
              #endregion

              #region EDIT-BUSQUEDA-RANGO-SALARIAL
              strStep = "EDIT-n_salario_desde";
              if (!(objEditBusqueda.n_salario_desdeNull))
              {
                  xmlEditBusqueda.SetAttr("n_salario_desde", objEditBusqueda.n_salario_desde);
              }

              strStep = "EDIT-n_salario_hasta";
              if (!(objEditBusqueda.n_salario_hastaNull))
              {
                  xmlEditBusqueda.SetAttr("n_salario_hasta", objEditBusqueda.n_salario_hasta);
              }

              strStep = "EDIT-l_cv_sin_salario";
              if (!(objEditBusqueda.l_cv_sin_salarioNull))
              {
                  xmlEditBusqueda.SetAttr("l_cv_sin_salario", objEditBusqueda.l_cv_sin_salario);
              }

              #endregion

              #region EDIT-BUSQUEDA-GENERAL

              strStep = "EDIT-f_actualizacion";
              if (!(objEditBusqueda.f_actualizacionNull))
              {
                  xmlEditBusqueda.SetAttr("f_actualizacion", objEditBusqueda.f_actualizacion);
                  xmlEditBusqueda.SetAttr("c_f_actualizacion", objEditBusqueda.c_f_actualizacion);
              }

              strStep = "EDIT-c_leido";
              xmlEditBusqueda.SetAttr("c_leido", objEditBusqueda.c_leido);                  
              
              #endregion

              }
              #endregion

          }           
          catch (Exception ex)
          {
              NomadException nmdEx = new NomadException("NucleusRH.Base.Postulantes.CV.WEB_LoadAdvancedSearch()", ex);
              nmdEx.SetValue("Step", strStep);
              throw nmdEx;
          }

          NomadLog.Debug("xmlResultLoadAdvancedSearch: " + xmlResult.ToString());
          return xmlResult;
      }

     /// <summary>
     /// Carga los datos del cv show
     /// -Nombre y Apellido
     /// - Nota del CV
     /// -V2: Marca el cv como leido por el usuario logueado
     /// </summary>
     /// <param name="pParam"></param>
     /// <returns></returns>
      public static NomadXML WEB_LoadCVShow(NomadXML pParam)
      {
           NomadLog.Debug("-----------------------------------------------");
           NomadLog.Debug("----------WEB_LoadCVShow V2------------");
           NomadLog.Debug("-----------------------------------------------");

           NomadLog.Debug("WEB_LoadCVShow.pParam: " + pParam.ToString());

          NomadXML xmlResult;
          string strMode;
          string strStep="";

          strMode = pParam.GetAttr("mode");
          xmlResult = new NomadXML("DATA");          
          xmlResult.SetAttr("mode", strMode);
          NomadTransaction objTrans = null;

          try
          {                           
              //Lectura del CV - Si es desde Buscador de CV, se marca como leido por usuario
              //               - Si es desde Ofertas Laborales, se marca como leido en la postulacion, indepnendientemente del usuario

              string strOICV = pParam.GetAttr("oi_cv");
              string strFromOfertaLab = pParam.GetAttr("fromOfertaLab");
              NomadLog.Debug("MODE-METHOD: " + strFromOfertaLab);
              if (strFromOfertaLab == "1")
              {
                  strStep = "LECTURA-OL";

                  strStep = "LECTURA-OL-GET-POSTU";
                  NucleusRH.Base.Postulantes.CV.POSTULACIONES objPostu = POSTULACIONES.Get(pParam.GetAttr("idPostu"));
                  objPostu.l_leido = true;

                  strStep = "LECTURA-OL-SAVE-POSTU";
                  objTrans = new NomadTransaction();
                  objTrans.Begin();
                  objTrans.SaveRefresh(objPostu);
                  objTrans.Commit();
              }
              else
              {
                  strStep = "LECTURA-CV";

                  strStep = "LECTURA-CV-GET";
                  NucleusRH.Base.Postulantes.CV.CV objCV = NucleusRH.Base.Postulantes.CV.CV.Get(strOICV);

                  strStep = "LECTURA-CV-GET-MARCA";
                  NomadXML xmlP = new NomadXML("DATA");
                  xmlP.SetAttr("oi_cv", objCV.id);

                  NomadXML xmlLeido = Nomad.NSystem.Base.NomadEnvironment.QueryNomadXML(NucleusRH.Base.Postulantes.CV.CV.Resources.CV_LEIDO_BY_USR, xmlP.ToString());

                  if (xmlLeido.FirstChild().GetAttr("oi_cv_leido") == "0")
                  {
                      //No lo encontro, por lo que lo agrego
                      strStep = "LECTURA-CV-AGREGO-LEIDO";
                      NucleusRH.Base.Postulantes.CV.CV_LEIDO objCvLeido = new CV_LEIDO();
                      objCvLeido.oi_usuario_sistema = Nomad.NSystem.Proxy.NomadProxy.GetProxy().UserEtty;

                      objCV.CV_LEIDO.Add(objCvLeido);

                      objTrans = new NomadTransaction();
                      objTrans.Begin();
                      objTrans.SaveRefresh(objCV);
                      objTrans.Commit();
                  }
              }
 
              strStep = "QRY-CV-SHOW";
              NomadXML xmlParam = new NomadXML("DATA");
              xmlParam.SetAttr("oi_cv", pParam.GetAttr("oi_cv"));
              NomadXML xmlQryCVShow = Nomad.NSystem.Base.NomadEnvironment.QueryNomadXML(NucleusRH.Base.Postulantes.CV.CV.Resources.QRY_LOAD_CV_SHOW, xmlParam.ToString());
              xmlResult.AddTailElement(xmlQryCVShow);
                              
              
          }
          catch (Exception ex)
          {
              NomadException nmdEx = new NomadException("NucleusRH.Base.Postulantes.CV.WEB_LoadCVShow()", ex);
              nmdEx.SetValue("Step", strStep);
              if (objTrans != null)
              {
                  objTrans.Rollback();
              }
              throw nmdEx;
          }

          NomadLog.Debug("xmlResultLoadCVShow: " + xmlResult.ToString());
          return xmlResult;
      
      }

      /// <summary>
      /// Persiste la Nota del CV
      /// Devuelve las notas de ese  CV
      /// </summary>
      /// <param name="pParam"></param>
      /// <returns></returns>
      public static NomadXML WEB_SaveNote(NomadXML pParam)
      { 
            
           NomadLog.Debug("-----------------------------------------------");
           NomadLog.Debug("----------WEB_SaveNote V2------------");
           NomadLog.Debug("-----------------------------------------------");

          NomadLog.Debug("WEB_SaveNote.pParam: " + pParam.ToString());

      
          string strMode;
          string strStep="";
          NomadXML xmlResult = new NomadXML("DATA");  
          strMode = pParam.GetAttr("mode");        
          NomadTransaction objTran= null;
          try
          {
              strStep = "GET-CV";
              string strOICV = pParam.GetAttr("oi_cv");
              NucleusRH.Base.Postulantes.CV.CV objCV = NucleusRH.Base.Postulantes.CV.CV.Get(strOICV);

              strStep = "CREO-NOTA";
              NucleusRH.Base.Postulantes.CV.NOTAS objNota = new Postulantes.CV.NOTAS();
              objNota.o_nota = pParam.GetAttr("o_nota");
              objNota.f_nota = DateTime.Now;
              objNota.oi_usuario_sistema = Nomad.NSystem.Proxy.NomadProxy.GetProxy().UserEtty;

              strStep = "ADD-NOTE";
              objCV.NOTAS_CV.Add(objNota);

              strStep = "SAVE-CV";
              objTran = new NomadTransaction();
              objTran.Begin();
              objTran.SaveRefresh(objCV);
              objTran.Commit();

              strStep = "GET-NOTES";
              NomadXML xmlNotas = xmlResult.AddTailElement("NOTAS");
              xmlNotas.SetAttr("name", "NOTAS");
              xmlNotas.SetAttr("cant", objCV.NOTAS_CV.Count);
              if (objCV.NOTAS_CV.Count > 0)
              {
                  NomadXML xmlParamNotes = new NomadXML("DATA");
                  xmlParamNotes.SetAttr("oi_cv", objCV.id);
                  NomadXML xmlQryNotas = Nomad.NSystem.Base.NomadEnvironment.QueryNomadXML(NucleusRH.Base.Postulantes.CV.NOTAS.Resources.GET_NOTES, xmlParamNotes.ToString());

                  for (NomadXML row = xmlQryNotas.FirstChild().FirstChild(); row != null; row = row.Next())
                  {
                      NomadXML xmlNota = xmlNotas.AddTailElement("NOTA");
                      xmlNota.SetAttr("oi_nota", row.GetAttr("oi_nota"));
                      xmlNota.SetAttr("o_nota", row.GetAttr("o_nota"));
                      DateTime FNota = (row.GetAttrDateTime("f_nota")).Date;
                      xmlNota.SetAttr("f_nota", FNota.ToString("d"));
                      string oi_usuario_sistema = row.GetAttrString("oi_usuario_sistema");
                      string strUser = Nomad.Base.Login.Entidades.ENTIDAD.Get(oi_usuario_sistema).GetAttribute("DES").ToString();
                      xmlNota.SetAttr("user", strUser);
                      if (row.GetAttr("oi_usuario_sistema") == Nomad.NSystem.Proxy.NomadProxy.GetProxy().UserEtty)
                      {
                          xmlNota.SetAttr("mine", "1");
                      }
                      else
                      {
                          xmlNota.SetAttr("mine", "0");
                      }
                  }
              }
         }
          catch (Exception ex)
          {
              if (objTran != null)
              {
                  objTran.Rollback();
              }
              NomadException nmdEx = new NomadException("NucleusRH.Base.Postulantes.CV.WEB_SaveNote()", ex);
              nmdEx.SetValue("Step", strStep);
              throw nmdEx;
          }
          return xmlResult;       
      }

      /// <summary>
      /// Elimina la nota del CV
      /// </summary>
      /// <param name="pParam"></param>
      /// <returns></returns>
      public static NomadXML WEB_DeleteNote(NomadXML pParam)
      { 
           NomadLog.Debug("-----------------------------------------------");
           NomadLog.Debug("----------WEB_DeleteNote V2------------");
           NomadLog.Debug("-----------------------------------------------");

           NomadLog.Debug("WEB_DeleteNote.pParam: " + pParam.ToString());      
          
          string strMode;
          string strStep="";
          NomadXML xmlResult = new NomadXML("DATA");  
          strMode = pParam.GetAttr("mode");        
          NomadTransaction objTran= null;
          try
          {
              strStep = "GET-NOTA";
              NucleusRH.Base.Postulantes.CV.NOTAS objNota =NOTAS.Get(pParam.GetAttr("oi_nota"));

              strStep = "DELETE-NOTA";
              objTran = new NomadTransaction();
              objTran.Begin();
              objTran.Delete(objNota);
              objTran.Commit();

              NucleusRH.Base.Postulantes.CV.CV objCV = CV.Get(objNota.oi_cv);
              strStep = "GET-NOTES";
              NomadXML xmlNotas = xmlResult.AddTailElement("NOTAS");
              xmlNotas.SetAttr("name", "NOTAS");
              xmlNotas.SetAttr("cant", objCV.NOTAS_CV.Count);
              if (objCV.NOTAS_CV.Count > 0)
              {
                  NomadXML xmlParamNotes = new NomadXML("DATA");
                  xmlParamNotes.SetAttr("oi_cv", objCV.id);
                  NomadXML xmlQryNotas = Nomad.NSystem.Base.NomadEnvironment.QueryNomadXML(NucleusRH.Base.Postulantes.CV.NOTAS.Resources.GET_NOTES, xmlParamNotes.ToString());

                  for (NomadXML row = xmlQryNotas.FirstChild().FirstChild(); row != null; row = row.Next())
                  {
                      NomadXML xmlNota = xmlNotas.AddTailElement("NOTA");
                      xmlNota.SetAttr("oi_nota", row.GetAttr("oi_nota"));
                      xmlNota.SetAttr("o_nota", row.GetAttr("o_nota"));
                      DateTime FNota = (row.GetAttrDateTime("f_nota")).Date;
                      xmlNota.SetAttr("f_nota", FNota.ToString("d"));
                      string oi_usuario_sistema = row.GetAttrString("oi_usuario_sistema");
                      string strUser = Nomad.Base.Login.Entidades.ENTIDAD.Get(oi_usuario_sistema).GetAttribute("DES").ToString();
                      xmlNota.SetAttr("user", strUser);
                      if (row.GetAttr("oi_usuario_sistema") == Nomad.NSystem.Proxy.NomadProxy.GetProxy().UserEtty)
                      {
                          xmlNota.SetAttr("mine", "1");
                      }
                      else
                      {
                          xmlNota.SetAttr("mine", "0");
                      }
                  }
              }

          }
          catch (Exception ex)
          {
              if (objTran != null)
              {
                  objTran.Rollback();
              }
              NomadException nmdEx = new NomadException("NucleusRH.Base.Postulantes.CV.WEB_DeleteNote()", ex);
              nmdEx.SetValue("Step", strStep);
              throw nmdEx;
          }
          return xmlResult;       
      }

      /// <summary>
      /// Persiste una carpeta nueva
      /// </summary>
      /// <param name="pParam"></param>
      /// <returns></returns>
      public static NomadXML WEB_SaveCarpeta(NomadXML pParam)
      { 
           NomadLog.Debug("-----------------------------------------------");
           NomadLog.Debug("----------WEB_SaveCarpeta V1------------");
           NomadLog.Debug("-----------------------------------------------");

           NomadLog.Debug("WEB_SaveCarpeta.pParam: " + pParam.ToString());      
          
          string strMode;
          string strStep="";
          NomadXML xmlResult = new NomadXML("DATA");  
          strMode = pParam.GetAttr("mode");        
          NomadTransaction objTran= null;
          try
          {
              NucleusRH.Base.Postulantes.Carpetas.CARPETA objCarpeta;
              if (pParam.GetAttr("oi_carpeta") == "")
              {
                  strStep = "CREATE-CARPETA";
                  objCarpeta = new Carpetas.CARPETA();
                  objCarpeta.d_carpeta = pParam.GetAttr("d_carpeta");
                  objCarpeta.oi_usuario_sistema = Nomad.NSystem.Proxy.NomadProxy.GetProxy().UserEtty;
                  objCarpeta.f_carpeta = DateTime.Now;
                  objCarpeta.d_creador = Nomad.Base.Login.Entidades.ENTIDAD.Get(objCarpeta.oi_usuario_sistema).GetAttribute("DES").ToString();
                  objCarpeta.e_cant_cv = 0;
              }
              else
              {
                  strStep = "GET-CARPETA";
                  objCarpeta = Carpetas.CARPETA.Get(pParam.GetAttr("oi_carpeta"));
                  //Actualizo el titulo de la carpeta
                  objCarpeta.d_carpeta = pParam.GetAttr("d_carpeta");              
              }
              strStep = "SAVE-CARPETA";
              objTran = new NomadTransaction();
              objTran.Begin();
              objTran.Save(objCarpeta);
              objTran.Commit();

              xmlResult.SetAttr("result", "1");


              //Antes de retornar, anexo ultimas 5 carpetas del usuario
              strStep = "GET-CARPETAS-USER";

              NomadXML xmlParamMisCarpetas = new NomadXML("DATA");
              NomadProxy proxy = NomadEnvironment.GetProxy();
              xmlParamMisCarpetas.SetAttr("oi_usuario_sistema", proxy.UserEtty);
              strStep = "GET:MIS-CARPETAS";
              NomadXML xmlResultMisCarpetas = Nomad.NSystem.Base.NomadEnvironment.QueryNomadXML(NucleusRH.Base.Postulantes.Carpetas.CARPETA.Resources.GET_MIS_CARPETAS, xmlParamMisCarpetas.ToString());

              NomadXML xmlCarpetas = xmlResult.AddTailElement("CARPETAS");
              xmlCarpetas.SetAttr("name", "CARPETAS");
              //Recorro las carpetas
              for (NomadXML row = xmlResultMisCarpetas.FirstChild().FirstChild(); row != null; row = row.Next())
              {
                 NomadXML xmlCarpeta = xmlCarpetas.AddTailElement("CARPETA");
                 xmlCarpeta.SetAttr("id", row.GetAttrString("oi_carpeta"));
                 xmlCarpeta.SetAttr("desc", row.GetAttrString("d_carpeta"));
                 xmlCarpeta.SetAttr("f_carpeta", row.GetAttrString("f_carpeta"));
              }       
          }
          catch (Exception ex)
          {
              if (objTran != null)
              {

                  objTran.Rollback();
                  xmlResult.SetAttr("result", "0");
              }
              NomadException nmdEx = new NomadException("NucleusRH.Base.Postulantes.CV.WEB_SaveCarpeta()", ex);
              nmdEx.SetValue("Step", strStep);
              throw nmdEx;
          }
          NomadLog.Debug("WEB_SaveCarpeta.xmlResult: " + xmlResult.ToString());
          return xmlResult;  
      }

     /// <summary>
     /// Retorna la lista completa de carpetas
     /// Si MIAS = 1, agrega oi_usuario_sitema del proxy para filtrar carpetas del usuario logueado y retornar solo estas
     /// </summary>
     /// <param name="pParam"></param>
     /// <returns></returns>
      public static NomadXML WEB_LoadListCarpetas(NomadXML pParam)
      { 
          NomadLog.Debug("-----------------------------------------------");
          NomadLog.Debug("----------WEB_LoadListCarpetas V1------------");
          NomadLog.Debug("-----------------------------------------------");

          NomadLog.Debug("WEB_LoadListCarpetas.pParam: " + pParam.ToString());

          NomadXML xmlResult;
          string strMode;
          string strStep="";

          strMode = pParam.GetAttr("mode");
          xmlResult = new NomadXML("DATA");          
          xmlResult.SetAttr("mode", strMode);

          try
          {
              strStep = "GET-ALL-CARPETAS";
              NomadXML xmlParam = new NomadXML("DATA");
              xmlParam.SetAttr("order", pParam.GetAttr("order"));
              xmlParam.SetAttr("activeOrder", pParam.GetAttr("activeOrder"));
              
              //Si exige las mías agrega parametro usuario sistema
              if (pParam.GetAttr("MIAS") == "1")
              {
                  NomadProxy proxy = NomadEnvironment.GetProxy();
                  xmlParam.SetAttr("oi_usuario_sistema", proxy.UserEtty);
              }
              xmlParam.SetAttr("carpeta_like", pParam.GetAttr("carpeta_like"));

              //Seteo parametro para lista
              xmlParam.SetAttr("qryMode", "LISTCAR");
              strStep = "QRY-LIST";


              //ILT 02.11.2016 - Paginacion
              xmlParam.SetAttr("fromrow", pParam.GetAttr("fromrow")); //fromrow es en base 0
              xmlParam.SetAttr("maxsize", pParam.GetAttrInt("pagesize") + 1);

              NomadLog.Debug("WEB_LoadListCarpetas.xmlParam: " + xmlParam.ToString());

              NomadXML xmlLoad = Nomad.NSystem.Base.NomadEnvironment.QueryNomadXML(NucleusRH.Base.Postulantes.Carpetas.CARPETA.Resources.GET_ALL, xmlParam.ToString());
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


              strStep = "QRY-CANT";
              //COUNTCART para contar la cantidad total de carpetas
              xmlParam.SetAttr("qryMode", "COUNTCAR");
              NomadXML xmlCant = Nomad.NSystem.Base.NomadEnvironment.QueryNomadXML(NucleusRH.Base.Postulantes.Carpetas.CARPETA.Resources.GET_ALL, xmlParam.ToString());
              xmlResult.AddTailElement(xmlCant);               
              

          }
          catch (Exception ex)
          {
              NomadException nmdEx = new NomadException("NucleusRH.Base.Postulantes.CV.WEB_LoadListCarpetas()", ex);
              nmdEx.SetValue("Step", strStep);
              throw nmdEx;
          }

          NomadLog.Debug("xmlResultLoadListcarpetas: " + xmlResult.ToString());
          return xmlResult;
      }

      /// <summary>
      /// Elimina una carpeta por oICarpeta
      /// </summary>
      /// <param name="pParam"></param>
      /// <returns></returns>
      public static NomadXML WEB_DeleteCarpeta(NomadXML pParam)
      { 
        NomadLog.Debug("-----------------------------------------------");
          NomadLog.Debug("----------WEB_DeleteCarpeta V1------------");
          NomadLog.Debug("-----------------------------------------------");

          NomadLog.Debug("WEB_DeleteCarpeta.pParam: " + pParam.ToString());

          NomadXML xmlResult;
          string strMode;
          string strStep="";
          NomadTransaction objTran = null;
          strMode = pParam.GetAttr("mode");
          xmlResult = new NomadXML("DATA");          
          xmlResult.SetAttr("mode", strMode);

          try
          {
              strStep = "GET-CARPETA";
              NucleusRH.Base.Postulantes.Carpetas.CARPETA objCarpeta = Carpetas.CARPETA.Get(pParam.GetAttr("oi_carpeta"));
              strStep = "DELETE-CARPETA";
              objTran = new NomadTransaction();
              objTran.Begin();
              objTran.Delete(objCarpeta);
              objTran.Commit();

              //El mismo metodo de delete trae todas las carpetas que quedaron
              strStep = "GET-ALL-CARPETAS";
              NomadXML xmlParam = new NomadXML("DATA");
              xmlParam.SetAttr("order", pParam.GetAttr("order"));
              xmlParam.SetAttr("activeOrder", pParam.GetAttr("activeOrder"));

              //Si exige las mías agrega parametro usuario sistema
              if (pParam.GetAttr("MIAS") == "1")
              {
                  NomadProxy proxy = NomadEnvironment.GetProxy();
                  xmlParam.SetAttr("oi_usuario_sistema", proxy.UserEtty);
              }
              xmlParam.SetAttr("carpeta_like", pParam.GetAttr("carpeta_like"));

              //Seteo parametro para lista
              xmlParam.SetAttr("qryMode", "LISTCAR");
              strStep = "QRY-LIST";
              NomadXML xmlLoad = Nomad.NSystem.Base.NomadEnvironment.QueryNomadXML(NucleusRH.Base.Postulantes.Carpetas.CARPETA.Resources.GET_ALL, xmlParam.ToString());
              xmlResult.AddTailElement(xmlLoad);

              strStep = "QRY-CANT";
              //COUNTCART para contar la cantidad total de carpetas
              xmlParam.SetAttr("qryMode", "COUNTCAR");
              NomadXML xmlCant = Nomad.NSystem.Base.NomadEnvironment.QueryNomadXML(NucleusRH.Base.Postulantes.Carpetas.CARPETA.Resources.GET_ALL, xmlParam.ToString());
              xmlResult.AddTailElement(xmlCant);               

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

              NomadException nmdEx = new NomadException("NucleusRH.Base.Postulantes.CV.WEB_DeleteCarpeta()", ex);
              nmdEx.SetValue("Step", strStep);
              throw nmdEx;
          }

          NomadLog.Debug("xmlResultDeleteCarpeta: " + xmlResult.ToString());
          return xmlResult;
      
      }

      /// <summary>
      /// Delete masivo de Carpetas. Elimina de 1 a N carpetas y retorna
      /// la lista de Carpetas remanentes
      /// </summary>
      /// <param name="pParam"></param>
      /// <returns></returns>
      public static NomadXML WEB_MasiveDeleteCarpeta(NomadXML pParam)
      { 
          NomadLog.Debug("-----------------------------------------------");
          NomadLog.Debug("----------WEB_MasiveDeleteCarpeta V1------------");
          NomadLog.Debug("-----------------------------------------------");

          NomadLog.Debug("WEB_MasiveDeleteCarpeta.pParam: " + pParam.ToString());

          NomadXML xmlResult;
          string strMode;
          string strStep="";
          NomadTransaction objTran = null;
          strMode = pParam.GetAttr("mode");
          xmlResult = new NomadXML("DATA");          
          xmlResult.SetAttr("mode", strMode);

          try
          {
              strStep = "ARMO-ARRAY-IDS";
              string[] arrayIds = pParam.GetAttrString("param").Split(',');
              
              objTran = new NomadTransaction();
              objTran.Begin();
              strStep = "RECORRO-IDS";
              for (int i = 0; i < arrayIds.Length; i++)
              {
                  strStep = "GET-CARPETA-" + i.ToString();
                  string oi_carpeta = arrayIds[i];
                  NucleusRH.Base.Postulantes.Carpetas.CARPETA objCarpeta = Carpetas.CARPETA.Get(oi_carpeta);
                  strStep = "DELETE-CARPETA-" + i.ToString();
                  objTran.Delete(objCarpeta);
              }
              
              //Persisto la eliminacion
              objTran.Commit();              
              
              //El mismo metodo de delete trae todas las carpetas que quedaron
              strStep = "GET-ALL-CARPETAS";
              NomadXML xmlParam = new NomadXML("DATA");
              xmlParam.SetAttr("order", pParam.GetAttr("order"));
              xmlParam.SetAttr("activeOrder", pParam.GetAttr("activeOrder"));

              //Si exige las mías agrega parametro usuario sistema
              if (pParam.GetAttr("MIAS") == "1")
              {
                  NomadProxy proxy = NomadEnvironment.GetProxy();
                  xmlParam.SetAttr("oi_usuario_sistema", proxy.UserEtty);
              }

              xmlParam.SetAttr("carpeta_like", pParam.GetAttr("carpeta_like"));
              //Seteo parametro para lista
              xmlParam.SetAttr("qryMode", "LISTCAR");
              strStep = "QRY-LIST";
              NomadXML xmlLoad = Nomad.NSystem.Base.NomadEnvironment.QueryNomadXML(NucleusRH.Base.Postulantes.Carpetas.CARPETA.Resources.GET_ALL, xmlParam.ToString());
              xmlResult.AddTailElement(xmlLoad);

              strStep = "QRY-CANT";
               
              //COUNTCART para contar la cantidad total de carpetas
              xmlParam.SetAttr("qryMode", "COUNTCAR");
              NomadXML xmlCant = Nomad.NSystem.Base.NomadEnvironment.QueryNomadXML(NucleusRH.Base.Postulantes.Carpetas.CARPETA.Resources.GET_ALL, xmlParam.ToString());
              xmlResult.AddTailElement(xmlCant);
               
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

              NomadException nmdEx = new NomadException("NucleusRH.Base.Postulantes.CV.WEB_MasiveDeleteCarpeta()", ex);
              nmdEx.SetValue("Step", strStep);
              throw nmdEx;
          }

          NomadLog.Debug("xmlResultMasiveDeleteCarpeta: " + xmlResult.ToString());
          return xmlResult;
      
      }

      /// <summary>
      /// Asocia una lista de CVs a una Carpeta
      /// </summary>
      /// <param name="pParam"></param>
      /// <returns></returns>
      public static NomadXML WEB_Asocia_CV_Carpeta(NomadXML pParam)
      { 
          NomadLog.Debug("-----------------------------------------------");
          NomadLog.Debug("----------WEB_Asocia_CV_Carpeta V1------------");
          NomadLog.Debug("-----------------------------------------------");

          NomadLog.Debug("WEB_Asocia_CV_Carpeta.pParam: " + pParam.ToString());

          NomadXML xmlResult;
          string strMode;
          string strStep="";
          NomadTransaction objTran = null;
          strMode = pParam.GetAttr("mode");
          xmlResult = new NomadXML("DATA");          
          xmlResult.SetAttr("mode", strMode);         
          try
          {
              bool persistir = false;
              strStep = "GET-CARPETA";
              NucleusRH.Base.Postulantes.Carpetas.CARPETA objCarpeta = Carpetas.CARPETA.Get(pParam.GetAttr("oi_carpeta"));
              
              strStep = "ARMO-ARRAY-IDS";
              string[] arrayIds = pParam.GetAttrString("paramCVs").Split(',');

              //Valido que no este asociado ya el CV, si esta pongo el id en delete y lo descarto luego
              for (int i = 0; i < arrayIds.Length; i++)
              {
                  foreach (NucleusRH.Base.Postulantes.Carpetas.CAR_CV objValCarCV in objCarpeta.ASOCIA_CV_CAR)
                  {
                      if (arrayIds[i] == objValCarCV.oi_cv)
                      {
                          arrayIds[i] = "delete";
                          break;
                      }
                  }
              }
              
              strStep = "RECORRO-IDS";
              for (int i = 0; i < arrayIds.Length; i++)
              {
                  if (arrayIds[i] != "delete")
                  {
                      strStep = "CREO-CAR-CV";
                      NucleusRH.Base.Postulantes.Carpetas.CAR_CV objCarCv = new Carpetas.CAR_CV();
                      objCarCv.f_car_cv = DateTime.Now;
                      objCarCv.oi_usuario_sistema = Nomad.NSystem.Proxy.NomadProxy.GetProxy().UserEtty;
                      objCarCv.oi_cv = arrayIds[i];
                      objCarCv.c_marca = "SM";
                      objCarCv.f_marca = DateTime.Now;
                      objCarCv.oi_usuario_marca = objCarCv.oi_usuario_sistema;
                      objCarpeta.ASOCIA_CV_CAR.Add(objCarCv);

                      //Actualizo cantidad de cvs de la carpeta
                      objCarpeta.e_cant_cv = objCarpeta.e_cant_cv + 1;

                      //Indico que tiene algo para persistir
                      persistir = true;
                  }
              }
              if (persistir)
              {
                  strStep = "PERSISTO-ASOC";
                  objTran = new NomadTransaction();
                  objTran.Begin();
                  objTran.Save(objCarpeta);
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

              NomadException nmdEx = new NomadException("NucleusRH.Base.Postulantes.CV.WEB_Asocia_CV_Carpeta()", ex);
              nmdEx.SetValue("Step", strStep);
              throw nmdEx;
          }

          NomadLog.Debug("xmlResultAsociaCVCarpeta: " + xmlResult.ToString());
          return xmlResult;
      }


     /// <summary>
     /// Borra los cvs parametros de la carpeta parametro
     /// actualiza la cantidad de cvs, restando los cvs parametros a la carpeta parametro
     /// </summary>
     /// <param name="pParam"></param>
     /// <returns></returns>
      public static NomadXML WEB_DesAsociarCVCarpeta(NomadXML pParam)
      { 
          NomadLog.Debug("-----------------------------------------------");
          NomadLog.Debug("----------WEB_DesAsociarCVCarpeta V1------------");
          NomadLog.Debug("-----------------------------------------------");

          NomadLog.Debug("WEB_DesAsociarCVCarpeta.pParam: " + pParam.ToString());

          NomadXML xmlResult;
          string strMode;
          string strStep="";
          NomadTransaction objTran = null;
          strMode = pParam.GetAttr("mode");
          xmlResult = new NomadXML("DATA");          
          xmlResult.SetAttr("mode", strMode);
          try
          {
              strStep = "GET-CARPETA";
              NucleusRH.Base.Postulantes.Carpetas.CARPETA objCarpeta = Carpetas.CARPETA.Get(pParam.GetAttr("oi_carpeta"));

              strStep = "ARMO-ARRAY-IDS";
              string[] arrayIds = pParam.GetAttrString("paramCVs").Split(',');
              
              // objTran = new NomadTransaction();
               //objTran.Begin();
              string[] arrayIdsToDel =new string[51];
              int index = 0;
              for (int i = 0; i < arrayIds.Length; i++)
              {
                  strStep = "GET-CV-TO-DEL";
                  foreach (NucleusRH.Base.Postulantes.Carpetas.CAR_CV objCarpetaCV in objCarpeta.ASOCIA_CV_CAR)
                  {
                      if (objCarpetaCV.oi_cv == arrayIds[i])
                      {
                          strStep = "GET-CAR-CV";                                                 
                          arrayIdsToDel[index] = objCarpetaCV.Id;
                          index++;
                          break;
                      }
                  }
              }

              //strStep = "DEL-CAR-CV";
              
              //objTran.SaveRefresh(objCarpeta);
              objTran = new NomadTransaction();

              int cantDesasoc = 0;
              //Para cada id
              for (int i = 0; i < arrayIdsToDel.Length; i++)
              {
                  if (arrayIdsToDel[i] != null)
                  {
                      objTran.Begin();
                      NucleusRH.Base.Postulantes.Carpetas.CAR_CV objTodel = Carpetas.CAR_CV.Get(arrayIdsToDel[i]);
                      objTran.Delete(objTodel);
                      strStep = "DEL-CAR-CV" + arrayIdsToDel[i].ToString();                         
                      objTran.Commit();
                      cantDesasoc++;
                  }
              }

              strStep = "GET-CARPETA-ACT";
              NucleusRH.Base.Postulantes.Carpetas.CARPETA objCarpetaAct = Carpetas.CARPETA.Get(pParam.GetAttr("oi_carpeta"));
             
              strStep = "ACT-CANT-CV";
              objCarpetaAct.e_cant_cv = objCarpetaAct.e_cant_cv - cantDesasoc;
              objTran.Begin();
              objTran.Save(objCarpetaAct);
              objTran.Commit();                  
         

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

              NomadException nmdEx = new NomadException("NucleusRH.Base.Postulantes.CV.WEB_DesAsociarCVCarpeta()", ex);
              nmdEx.SetValue("Step", strStep);
              throw nmdEx;
          }

          NomadLog.Debug("xmlResultAsociaCVCarpeta: " + xmlResult.ToString());
          return xmlResult;
      }

      /// <summary>
      /// Cambia el estado del l_leido del CV por el estado parametro
      /// V2: Registra la lectura del CV por usuario
      /// Si l_leido es 1 agrega un registro, si es que no está
      /// Si l_leido es 0, quita el registro
      /// </summary>
      /// <param name="pParam"></param>
      /// <returns></returns>
      public static NomadXML WEB_Leido_NoLeido(NomadXML pParam)
      { 
          NomadLog.Debug("-----------------------------------------------");
          NomadLog.Debug("----------WEB_Leido_NoLeido V2------------");
          NomadLog.Debug("-----------------------------------------------");

          NomadLog.Debug("WEB_Leido_NoLeido.pParam: " + pParam.ToString());

          NomadXML xmlResult;
          string strMode;
          string strStep="";
          NomadTransaction objTran = null;
          strMode = pParam.GetAttr("mode");
          xmlResult = new NomadXML("DATA");          
          xmlResult.SetAttr("mode", strMode);
          try
          {
              bool persistir = false;
              strStep = "GET-CV";              
              NucleusRH.Base.Postulantes.CV.CV objCV = CV.Get(pParam.GetAttr("oi_cv"));
              
              //strStep = "ACTUALIZO-L-LEIDO";
              //objCV.l_leido = pParam.GetAttr("l_leido")=="1"? true:false;              
               strStep = "TIPO-MARCA";
               NomadXML xmlParam = new NomadXML("DATA");
               xmlParam.SetAttr("oi_cv", objCV.id);
                 
               NomadXML xmlLeido = Nomad.NSystem.Base.NomadEnvironment.QueryNomadXML(NucleusRH.Base.Postulantes.CV.CV.Resources.CV_LEIDO_BY_USR, xmlParam.ToString());
              
              if (pParam.GetAttr("l_leido") == "1")
              {
                  //Lo marco como leido, si es que no esta ya marcado                  
                  if (xmlLeido.FirstChild().GetAttr("oi_cv_leido") == "0")
                  {
                      //No lo encontro, por lo que lo agrego
                      strStep = "AGREGO-LEIDO";
                      NucleusRH.Base.Postulantes.CV.CV_LEIDO objCvLeido = new CV_LEIDO();
                      objCvLeido.oi_usuario_sistema = Nomad.NSystem.Proxy.NomadProxy.GetProxy().UserEtty;

                      objCV.CV_LEIDO.Add(objCvLeido);
                      persistir = true;
                  }
              }
              else
              { 
                //Lo marco como no leido, eliminando el oi_cv_leido
                  if (xmlLeido.FirstChild().GetAttr("oi_cv_leido") != "0")
                  {
                      strStep = "QUITO-LEIDO";
                      objCV.CV_LEIDO.RemoveById(xmlLeido.FirstChild().GetAttr("oi_cv_leido"));
                      persistir = true;
                  }
              }
              
              
              strStep = "PERSISTO-CV";
              if (persistir)
              {
                  objTran = new NomadTransaction();
                  objTran.Begin();
                  objTran.Save(objCV);
                  objTran.Commit();
              }
              xmlResult.SetAttr("result", "1");
          }
          catch (Exception ex)
          {
              
              if (objTran != null)
              {
                  objTran.Rollback();
                  xmlResult.SetAttr("result", "0");
              }

              NomadException nmdEx = new NomadException("NucleusRH.Base.Postulantes.CV.WEB_Leido_NoLeido()", ex);
              nmdEx.SetValue("Step", strStep);
              throw nmdEx;
          }

          NomadLog.Debug("xmlResultAsociaCVCarpeta: " + xmlResult.ToString());
          return xmlResult;
      }
      
      
      /// <summary>
      /// Funcion que limpia un string, por la macrosustitucion empleada en los parametros
      /// Limpia caracteres: ', ",&
      /// </summary>
      /// <param name="pstrCadena"></param>
      /// <returns></returns>
      public static string limpiarCadena(string pstrCadena)
      {
          string str = pstrCadena.Replace("&","");
          str = str.Replace("'", "");
          str = str.Replace("\"",""); 
          return str;
      }


      /// <summary>
      /// Retorna la lista completa de CVs para una carpeta
      /// </summary>
      /// <param name="pParam"></param>
      /// <returns></returns>
      public static NomadXML WEB_LoadListCVsAsociados(NomadXML pParam)
      { 
          NomadLog.Debug("-----------------------------------------------");
          NomadLog.Debug("----------WEB_LoadListCVsAsociados V1-----------");
          NomadLog.Debug("-----------------------------------------------");

          NomadLog.Debug("WEB_LoadListCVsAsociados.pParam: " + pParam.ToString());

          NomadXML xmlResult;
          string strMode;
          string strStep="";

          strMode = pParam.GetAttr("mode");
          xmlResult = new NomadXML("DATA");          
          xmlResult.SetAttr("mode", strMode);

          try
          {
              strStep = "GET-CVs-Carpeta" + pParam.GetAttrString("oi_carpeta");
              NomadXML xmlParam = new NomadXML("DATA");
              xmlParam.SetAttr("oi_carpeta", pParam.GetAttr("oi_carpeta"));
              xmlParam.SetAttr("order", pParam.GetAttr("order"));
              xmlParam.SetAttr("activeOrder", pParam.GetAttr("activeOrder"));

              strStep = "QRY-LIST-CAR";
              xmlParam.SetAttr("qryMode", "LISTCAR");
              
              //Agrego parametros de leido, marca, palabras claves y filtros inteligentes
              if(pParam.GetAttr("l_leido")!="")
                    xmlParam.SetAttr("l_leido",pParam.GetAttr("l_leido"));
              
              if (pParam.GetAttr("c_marca") != "")
                  xmlParam.SetAttr("c_marca", pParam.GetAttr("c_marca"));

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


              //ILT 02.11.2016 - Paginacion
              xmlParam.SetAttr("fromrow", pParam.GetAttr("fromrow")); //fromrow es en base 0
              xmlParam.SetAttr("maxsize", pParam.GetAttrInt("pagesize") + 1);


              NomadLog.Debug("xmlParamListCVListAsociarCVCarpeta: " + xmlParam.ToString());
              NomadXML xmlLoad = Nomad.NSystem.Base.NomadEnvironment.QueryNomadXML(NucleusRH.Base.Postulantes.Carpetas.CAR_CV.Resources.GET_CVS_CAR, xmlParam.ToString());
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
              NomadXML xmlCant = Nomad.NSystem.Base.NomadEnvironment.QueryNomadXML(NucleusRH.Base.Postulantes.Carpetas.CAR_CV.Resources.GET_CVS_CAR, xmlParam.ToString());
              xmlResult.AddTailElement(xmlCant);

              NomadXML xmlList = new NomadXML("DATA");
              
              strStep = "RESUMEN";
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

                      NomadXML xmlResultResumenPuesto = Nomad.NSystem.Base.NomadEnvironment.QueryNomadXML(NucleusRH.Base.Postulantes.Carpetas.CAR_CV.Resources.GET_CVS_CAR, xmlParam.ToString());

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

                      NomadXML xmlResultResumenEmpresas = Nomad.NSystem.Base.NomadEnvironment.QueryNomadXML(NucleusRH.Base.Postulantes.Carpetas.CAR_CV.Resources.GET_CVS_CAR, xmlParam.ToString());
                  

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

                      NomadXML xmlResultTitulos = Nomad.NSystem.Base.NomadEnvironment.QueryNomadXML(NucleusRH.Base.Postulantes.Carpetas.CAR_CV.Resources.GET_CVS_CAR, xmlParam.ToString());

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

                      NomadXML xmlResultLocalidades = Nomad.NSystem.Base.NomadEnvironment.QueryNomadXML(NucleusRH.Base.Postulantes.Carpetas.CAR_CV.Resources.GET_CVS_CAR, xmlParam.ToString());

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
              
              
              //Antes de retornar, anexo ultimas 5 carpetas del usuario
              strStep = "GET-CARPETAS-USER";
              NomadXML xmlItem = new NomadXML();
              NomadXML xmlParamMisCarpetas = new NomadXML("DATA");
              NomadProxy proxy = NomadEnvironment.GetProxy();
              xmlParamMisCarpetas.SetAttr("oi_usuario_sistema", proxy.UserEtty);
              strStep = "GET:MIS-CARPETAS";
              NomadXML xmlResultMisCarpetas = Nomad.NSystem.Base.NomadEnvironment.QueryNomadXML(NucleusRH.Base.Postulantes.Carpetas.CARPETA.Resources.GET_MIS_CARPETAS, xmlParamMisCarpetas.ToString());

             NomadXML xmlCarpetas = new NomadXML();
             xmlCarpetas = xmlResult.AddTailElement("CARPETAS");
             xmlCarpetas.SetAttr("name", "CARPETAS");
              //Recorro las carpetas
              for (NomadXML row = xmlResultMisCarpetas.FirstChild().FirstChild(); row != null; row = row.Next())
              {
                  xmlItem = xmlCarpetas.AddTailElement("CARPETA");
                  xmlItem.SetAttr("id", row.GetAttrString("oi_carpeta"));
                  xmlItem.SetAttr("desc", row.GetAttrString("d_carpeta"));
                  xmlItem.SetAttr("f_carpeta", row.GetAttrString("f_carpeta"));
              }  

          }
          catch (Exception ex)
          {
              NomadException nmdEx = new NomadException("NucleusRH.Base.Postulantes.CV.WEB_LoadListCVsAsociados()", ex);
              nmdEx.SetValue("Step", strStep);
              throw nmdEx;
          }

          NomadLog.Debug("xmlResultWEB_LoadListCVsAsociados: " + xmlResult.ToString());
          return xmlResult;
      }


      /// <summary>
      /// Marca el cv dentro de una carpeta con la marca parametro
      /// P: Preferido
      /// R: Rojo
      /// A: Azul
      /// V: Verde
      /// SM: Sin Marca / Descmarcar
      /// </summary>
      /// <param name="pParam"></param>
      /// <returns></returns>
      public static NomadXML WEB_MarcarCVCarpeta(NomadXML pParam)
      {
          NomadLog.Debug("-----------------------------------------------");
          NomadLog.Debug("----------WEB_MarcarCVCarpeta V1------------");
          NomadLog.Debug("-----------------------------------------------");

          NomadLog.Debug("WEB_MarcarCVCarpeta.pParam: " + pParam.ToString());

          NomadXML xmlResult;
          string strMode;
          string strStep = "";
          NomadTransaction objTran = null;
          strMode = pParam.GetAttr("mode");
          xmlResult = new NomadXML("DATA");
          xmlResult.SetAttr("mode", strMode);
          try
          {
              strStep = "GET-CARPETA";
              NucleusRH.Base.Postulantes.Carpetas.CARPETA objCarpeta = Carpetas.CARPETA.Get(pParam.GetAttr("oi_carpeta"));
              
              strStep = "ARMO-ARRAY-IDS";
              string[] arrayIds = pParam.GetAttrString("paramCVs").Split(',');

              strStep = "RECORRO-IDS";
              for (int i = 0; i < arrayIds.Length; i++)
              {
                  strStep = "RECORRO-CVS-CARPETA";
                  foreach (NucleusRH.Base.Postulantes.Carpetas.CAR_CV objCarCV in objCarpeta.ASOCIA_CV_CAR)
                  {
                      if (objCarCV.oi_cv == arrayIds[i])
                      {
                          strStep = "MARCO-CV-CARPETA" + arrayIds[i].ToString();
                          objCarCV.c_marca = pParam.GetAttr("marca");
                          objCarCV.f_marca = DateTime.Now;
                          objCarCV.oi_usuario_marca = Nomad.NSystem.Proxy.NomadProxy.GetProxy().UserEtty;
                      }             
                  }                  
              }

              strStep = "PERSISTO-CARPETA-CV";
              objTran = new NomadTransaction();
              objTran.Begin();
              objTran.SaveRefresh(objCarpeta);
              objTran.Commit();
              
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

              NomadException nmdEx = new NomadException("NucleusRH.Base.Postulantes.CV.WEB_MarcarCVCarpeta()", ex);
              nmdEx.SetValue("Step", strStep);
              throw nmdEx;
          }

          NomadLog.Debug("xmlResultMarcarCVCarpeta: " + xmlResult.ToString());
          return xmlResult;
      
      }

      /// <summary>
      /// Resuelve los gráficos estadisticos
      /// Retorna atributos calculados para cada grafico
      /// </summary>
      /// <param name="pParam"></param>
      /// <returns></returns>
      public static NomadXML WEB_Grapahic(NomadXML pParam)
      { 
         NomadLog.Debug("-----------------------------------------------");
         NomadLog.Debug("----------WEB_Grapahic V1------------");
          NomadLog.Debug("-----------------------------------------------");

          NomadLog.Debug("WEB_Grapahic.pParam: " + pParam.ToString());

          NomadXML xmlResult;
          string strMode;
          string strStep = "";
          NomadTransaction objTran = null;
          strMode = pParam.GetAttr("mode");          
          xmlResult = new NomadXML("DATA");
          xmlResult.SetAttr("mode", strMode);
          try
          {
              NomadXML xmlParam = new NomadXML("DATA");
             
                  strStep = "SEXO";
                  xmlParam.SetAttr("qryMode", "SEXO");
                  xmlParam.SetAttr("oi_carpeta", pParam.GetAttr("oi_carpeta"));
                  NomadXML xmlSexo = Nomad.NSystem.Base.NomadEnvironment.QueryNomadXML(NucleusRH.Base.Postulantes.Carpetas.CAR_CV.Resources.GRAPHIC, xmlParam.ToString());
                  double dblCantMasculino = xmlSexo.FirstChild().GetAttrDouble("cantMasculino");
                  double dblCantFemenino = xmlSexo.FirstChild().GetAttrDouble("cantFemenino");
                  double dblCantTotal = dblCantMasculino + dblCantFemenino;

                  strStep = "CALCULO-PORC-SEXO";
                  double dblPorcMasculino = Math.Round(dblCantMasculino / dblCantTotal * 100);
                  double dblPorcFemenino = Math.Round(dblCantFemenino / dblCantTotal * 100);
                  if ((dblPorcMasculino + dblPorcFemenino)>100)
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
              
                  strStep = "EDAD";
                  xmlParam.SetAttr("qryMode","EDAD");
                  xmlParam.SetAttr("oi_carpeta", pParam.GetAttr("oi_carpeta"));
                  NomadXML xmlEdad = Nomad.NSystem.Base.NomadEnvironment.QueryNomadXML(NucleusRH.Base.Postulantes.Carpetas.CAR_CV.Resources.GRAPHIC, xmlParam.ToString());
                  double dblCant1824 = Math.Round(xmlEdad.FirstChild().GetAttrDouble("cant1824"));
                  double dblCant2536 = Math.Round(xmlEdad.FirstChild().GetAttrDouble("cant2536"));
                  double dblCant3645 = Math.Round(xmlEdad.FirstChild().GetAttrDouble("cant3645"));
                  double dblCant46 = Math.Round(xmlEdad.FirstChild().GetAttrDouble("cant46"));

                  dblCantTotal = dblCant1824 + dblCant2536 + dblCant3645 + dblCant46;

                  strStep = "CALCULO-PORC-EDAD";
                  double dblPorc1824 = Math.Round(dblCant1824 / dblCantTotal * 100);
                  double dblPorc2536 = Math.Round(dblCant2536 / dblCantTotal * 100);
                  double dblPorc3645 = Math.Round(dblCant3645 / dblCantTotal * 100);
                  double dblPorc46 = Math.Round(dblCant46 / dblCantTotal * 100);

                  strStep = "AJUSTE-EDAD";
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
                          case 46:   { dblPorc46 = dblPorc46 - 1; break; }
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
              
                  strStep = "NIVEST";
                  xmlParam.SetAttr("qryMode","NIVEST");
                  xmlParam.SetAttr("oi_carpeta", pParam.GetAttr("oi_carpeta"));
                  NomadXML xmlNivEst = Nomad.NSystem.Base.NomadEnvironment.QueryNomadXML(NucleusRH.Base.Postulantes.Carpetas.CAR_CV.Resources.GRAPHIC, xmlParam.ToString());

                  strStep = "CALCULO-TOTAL";
                  int cantTotal = 0;
                  for (NomadXML row = xmlNivEst.FirstChild().FirstChild(); row != null; row = row.Next())                  
                      cantTotal = cantTotal + row.GetAttrInt("cant");

                  strStep = "CALCULO-PORC";
                  NomadXML xmlResNivEst = new NomadXML("NIVELESSTUDIOS");
                  xmlResNivEst.SetAttr("name", "NIVELESSTUDIOS");

                  double acum = 0;
                  int id = 1;
                  for (NomadXML row = xmlNivEst.FirstChild().FirstChild(); row != null; row = row.Next())
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
                              row.SetAttr("value", row.GetAttrDouble("value") - 1);
                          }
                      }
                      //xmlResNivEst.AddTailElement(xmlResNivEst);
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
                              row.SetAttr("value", row.GetAttrDouble("value") + 1);
                          }
                      }
                     // xmlResult.AddTailElement(xmlResNivEst);
                  }
                   
                  //LO asigno reajustado o no
                  xmlResult.AddTailElement(xmlResNivEst);
                  
                  
              
                  strStep = "SALARIO";
                  xmlParam.SetAttr("qryMode", "SALARIO");
                  xmlParam.SetAttr("oi_carpeta", pParam.GetAttr("oi_carpeta"));
                  NomadXML xmlSalario = Nomad.NSystem.Base.NomadEnvironment.QueryNomadXML(NucleusRH.Base.Postulantes.Carpetas.CAR_CV.Resources.GRAPHIC, xmlParam.ToString());
                  
                  strStep = "FIND-MAXIMO";
                  double maximo = 0;
                  for (NomadXML row = xmlSalario.FirstChild().FirstChild(); row != null; row = row.Next())
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
                  for (NomadXML row = xmlSalario.FirstChild().FirstChild(); row != null; row = row.Next())
                  {
                      double dblValor = row.GetAttrDouble("n_remuneracion");
                      
                      if((dblValor>0) && (dblValor<=v0) ) I1++;

                      if ((dblValor > v0) && (dblValor <= v1)) I2++;

                      if ((dblValor > v1) && (dblValor <= v2)) I3++;

                      if ((dblValor > v2) && (dblValor <= v3)) I4++;

                      if ((dblValor > v3)) I5++;

                  }

                  NomadXML xmlResSalario = new NomadXML("SALARIO");
                  xmlResSalario.SetAttr("name", "SALARIO");
                  xmlResSalario.SetAttr("I1", I1);
                  xmlResSalario.SetAttr("label1", "De 0 a "+v0.ToString("n0"));
                  xmlResSalario.SetAttr("I2", I2);
                  xmlResSalario.SetAttr("label2", "De "+v0.ToString("n0")+" a "+v1.ToString("0"));
                  xmlResSalario.SetAttr("I3", I3);
                  xmlResSalario.SetAttr("label3", "De "+v1.ToString("n0")+" a "+v2.ToString("n0"));
                  xmlResSalario.SetAttr("I4", I4);
                  xmlResSalario.SetAttr("label4", "De "+v2.ToString("n0")+" a "+v3.ToString("n0"));
                  xmlResSalario.SetAttr("I5", I5);
                  xmlResSalario.SetAttr("label5", "Mayor a "+v3.ToString("n0"));

                  xmlResult.AddTailElement(xmlResSalario); 
                
               
          }
          catch (Exception ex)
          {
              xmlResult.SetAttr("resultOk", "0");
              if (objTran != null)
              {
                  objTran.Rollback();
                  xmlResult.SetAttr("result", "0");
              }

              NomadException nmdEx = new NomadException("NucleusRH.Base.Postulantes.CV.WEB_Grapahic()", ex);
              nmdEx.SetValue("Step", strStep);
              throw nmdEx;
          }

          NomadLog.Debug("xmlResultWEB_Grapahic: " + xmlResult.ToString());
          return xmlResult;              
      }

      /// <summary>
      /// Construye el mail del cv parametro y lo envia al email parametro
      /// </summary>
      /// <param name="pParam"></param>
      /// <returns></returns>
      public static NomadXML WEB_CVMAIL(NomadXML pParam)
      { 
         NomadLog.Debug("-----------------------------------------------");
         NomadLog.Debug("----------WEB_CVMAIL V1------------");
         NomadLog.Debug("-----------------------------------------------");

          NomadLog.Debug("WEB_CVMAIL.pParam: " + pParam.ToString());

          NomadXML xmlResult;
          string strMode;
          string strStep = "";
          string outFileName = "";
          string outFilePath = "";
           
          strMode = pParam.GetAttr("mode");          
          xmlResult = new NomadXML("DATA");
          xmlResult.SetAttr("mode", strMode);
          try
          {
              strStep = "ID-REPORTE";
              NucleusRH.Base.Postulantes.CV.CV objCv = CV.Get(pParam.GetAttr("oi_cv"));
              string idrep = "ReportMailCV_" + objCv.c_cv;

              strStep = "PARAM-REPORT";
              NomadXML xmlParam = new NomadXML("PARAMS");
              xmlParam.SetAttr("id",pParam.GetAttr("oi_cv"));
              xmlParam.SetAttr("chkLocal","1");

              strStep = "REPORT";
              //Cargo el Reporte
              Nomad.NmdFoRender objout = new Nomad.NmdFoRender("NucleusRH.Base.Postulantes.Curriculum.rpt", xmlParam);

              //Creo el Archivo de Salida
              outFileName = idrep + ".pdf";
              outFilePath = NomadProxy.GetProxy().RunPath + "Nomad\\TEMP\\";

              //Genero el Documento
              objout.Generate(outFilePath + "\\" + outFileName, Nomad.NmdFoRenderFormat.RENDER_PDF);
              
            
              //Envio de Mail a multiples destinatarios - ILT 31.10.2016              
              char[] separadores = {',',';'};
              string[] destinosMail = pParam.GetAttr("destinoMail").Split(separadores);              
              Nomad.Base.Mail.Mails.ADJUNTO objAdjunto = new Nomad.Base.Mail.Mails.ADJUNTO();              
              objAdjunto.TIPO = "FS";              
              objAdjunto.FILE = "ReporteCV.pdf";              
              objAdjunto.RECURSO = outFilePath + "\\" + outFileName;                                          
              foreach (string destino in destinosMail)
              {
                  if (destino != "")
                  {
                      Nomad.Base.Mail.Mails.MAIL objMail = Nomad.Base.Mail.Mails.MAIL.CrearTXT("Reporte CV PDF", pParam.GetAttr("cuerpoMail"), destino, "no-reply@bayton.com.ar");
                      objMail.ADJUNTOS.Add(objAdjunto);
                      objMail.Enviar();
                  }
              }

              
              /*strStep = "ENVIO-MAIL-1";
              Nomad.Base.Mail.Mails.MAIL objMail = Nomad.Base.Mail.Mails.MAIL.CrearTXT("Reporte CV PDF", pParam.GetAttr("cuerpoMail"), pParam.GetAttr("destinoMail"), "no-reply@bayton.com.ar");

              strStep = "ENVIO-MAIL-2";
              Nomad.Base.Mail.Mails.ADJUNTO objAdjunto = new Nomad.Base.Mail.Mails.ADJUNTO();

              strStep = "ENVIO-MAIL-3";
              objAdjunto.TIPO = "FS";
              
              strStep = "ENVIO-MAIL-4";
              objAdjunto.FILE = "ReporteCV.pdf";

              strStep = "ENVIO-MAIL-5";
              //objAdjunto.FILENAME = outFilePath + "\\" + outFileName;

              strStep = "ENVIO-MAIL-5-2";
              objAdjunto.RECURSO = outFilePath + "\\" + outFileName;

              strStep = "ENVIO-MAIL-6";
              objMail.ADJUNTOS.Add(objAdjunto);

              strStep = "ENVIO-MAIL-7";
              objMail.Enviar();

              strStep = "ENVIO-MAIL-8";*/
              
              xmlResult.SetAttr("resultOk", "1");
          }
          catch (Exception ex)
          {
              xmlResult.SetAttr("resultOk", "0");
              NomadException nmdEx = new NomadException("NucleusRH.Base.Postulantes.CV.WEB_CVMAIL()", ex);
              nmdEx.SetValue("Step", strStep);
              nmdEx.SetValue("outFileName", outFileName);
              nmdEx.SetValue("outFilePath", outFilePath);

              throw nmdEx;
          }


          


          NomadLog.Debug("WEB_CVMAIL: " + xmlResult.ToString());
          return xmlResult;
      }

      
      /// <summary>
      /// Carga las ofertas laborales del campo buscar parametro para el formulario modal de la busqueda avanzada del buscador de CV
      /// </summary>
      /// <param name="pParam"></param>
      /// <returns></returns>
      public static NomadXML WEB_LOAD_OFERTALAB(NomadXML pParam)
      { 
         NomadLog.Debug("-----------------------------------------------");
         NomadLog.Debug("----------WEB_LOAD_OFERTALAB------------");
         NomadLog.Debug("-----------------------------------------------");

          NomadLog.Debug("WEB_LOAD_OFERTALAB.pParam: " + pParam.ToString());

          NomadXML xmlResult;
          string strMode;
          string strStep = "";
          string outFileName = "";
          string outFilePath = "";
           
          strMode = pParam.GetAttr("mode");          
          xmlResult = new NomadXML("DATA");
          xmlResult.SetAttr("mode", strMode);
          try
          {
              NomadXML xmlParam = new NomadXML("DATA");
              xmlParam.SetAttr("campo_buscar", pParam.GetAttr("campo_buscar"));
              
              NomadLog.Debug("xmlParamWEB_LOAD_OFERTALAB: " + xmlParam.ToString());
              
              NomadXML xmlLoad = Nomad.NSystem.Base.NomadEnvironment.QueryNomadXML(NucleusRH.Base.Postulantes.CV.CV.Resources.QRY_OFERTA_LAB_ADVSEARCH, xmlParam.ToString());
              xmlResult.AddTailElement(xmlLoad);              
          }
          catch (Exception ex)
          {

              NomadException nmdEx = new NomadException("NucleusRH.Base.Postulantes.CV.WEB_LOAD_OFERTALAB()", ex);
              nmdEx.SetValue("Step", strStep);              
              throw nmdEx;
          }

          NomadLog.Debug("xmlResultWEB_LOAD_OFERTALAB: " + xmlResult.ToString());
          return xmlResult;
      }
        
      /// <summary>
      /// Estadisticas globales de CVs en el Main de Buscar CV
      /// </summary>
      /// <param name="pParam"></param>
      /// <returns></returns> 
      public static NomadXML WEB_GENERAL_ESTADISTICAS(NomadXML pParam)
      { 
         NomadLog.Debug("-----------------------------------------------");
         NomadLog.Debug("----------WEB_GENERAL_ESTADISTICAS------------");
         NomadLog.Debug("-----------------------------------------------");

         NomadLog.Debug("WEB_GENERAL_ESTADISTICAS.pParam: " + pParam.ToString());

          NomadXML xmlResult;
          string strMode;
          string strStep = "";         
           
          strMode = pParam.GetAttr("mode");          
          xmlResult = new NomadXML("DATA");
          xmlResult.SetAttr("mode", strMode);
          xmlResult.SetAttr("name", "INDICADORES");
          try
          {
              strStep = "DEFINE-INDICADORES";
              //string[] indicadores = new string[7];
              string[] indicadores = new string[13];
              
              indicadores[0] = "ALTA7";
              indicadores[1] = "ALTAM";
              indicadores[2] = "ACTCV";
              indicadores[3] = "EDAD";
              indicadores[4] = "SEXO";
              indicadores[5] = "TOTALCV";
              indicadores[6] = "TOTALCVWEB";
              indicadores[7] = "NIVEST";
              indicadores[8] = "ALTAMWEB";
              indicadores[9] = "PCIA";
              indicadores[10] = "POSTUMES";
              indicadores[11] = "POSTUSEM";
              indicadores[12] = "OFERACTSEM";

              strStep = "CALC-INDICADOR";
              for (int i = 0; i < indicadores.Length; i++)
              {
                  NomadXML xmlParam = new NomadXML("DATA");
                  xmlParam.SetAttr("c_indicador", indicadores[i].ToString());

                  NomadXML xmlLoad = Nomad.NSystem.Base.NomadEnvironment.QueryNomadXML(NucleusRH.Base.Postulantes.CV.CV.Resources.GLOBAL_ESTADISTICAS, xmlParam.ToString());
                  xmlResult.AddTailElement(xmlLoad);
              }
  

 

          }
          catch (Exception ex)
          {

              NomadException nmdEx = new NomadException("NucleusRH.Base.Postulantes.CV.WEB_GENERAL_ESTADISTICAS()", ex);
              nmdEx.SetValue("Step", strStep);
              throw nmdEx;
          }

          NomadLog.Debug("WEB_GENERAL_ESTADISTICAS: " + xmlResult.ToString());
          return xmlResult;
      }

      /// <summary>
      /// Devuelve el parametro para el reporte de la lista de cvs
      /// </summary>
      /// <param name="pParam"></param>
      /// <returns></returns>
      public static NomadXML WEB_ListCVReport(NomadXML pParam)
      {

          NomadLog.Debug("----------------------------------------------------------");
          NomadLog.Debug("-----------------WEB_ListCVReport V1--------------");
          NomadLog.Debug("----------------------------------------------------------");

          NomadLog.Debug("pParam: " + pParam.ToString());
          

          string strStep = "";
          NomadXML xmlResultFalso = new NomadXML("D");
          NomadXML xmlResultPar;
          NomadXML xmlResult = new NomadXML("DATA");
          
          try
          {
              xmlResult = new NomadXML("DATOS");
              xmlResult.SetAttr("name", "LCV");
              xmlResultPar = new NomadXML("DATA");
              xmlResultPar.SetAttr("mode", "CV");
              strStep = "GET-BUSQUEDA";
              NucleusRH.Base.Postulantes.Busquedas.BUSQUEDA objBusqueda = Busquedas.BUSQUEDA.Get(pParam.GetAttr("oi_busqueda"));

              //El xml result lo uso para que el metodo funcione, pero no es tenido en cuenta para el reporte              
              xmlResultPar = CreateXMLParamListCV(ref xmlResultFalso, ref strStep, objBusqueda);
              xmlResultPar.SetAttr("mode", "CV");
              xmlResultPar.SetAttr("oi_busqueda", objBusqueda.Id);
              xmlResult.AddTailElement(xmlResultPar);
              
          }
          catch (Exception ex)
          {
              NomadException nmdEx = new NomadException("NucleusRH.Base.Postulantes.CV.WEB_ListCVReport()", ex);
              nmdEx.SetValue("Step", strStep);
              throw nmdEx;
          }

          NomadLog.Debug("WEB_ListCVReport.xmlResult: " + xmlResult.ToString());
          return xmlResult;  
      }


      /// <summary>
      /// Retorna la lista de oi_cv de una busqueda o de una carpeta (con filtros y ordenamientos)
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
              if (strMode == "BUSQUEDA")
              {
                  strStep = "GET-BUSQUEDA";
                  NucleusRH.Base.Postulantes.Busquedas.BUSQUEDA objBusqueda = Busquedas.BUSQUEDA.Get(pParam.GetAttr("oi_busqueda"));

                  strStep = "GET-LIST";
                  NomadXML xmlParam = CreateXMLParamListCV(ref xmlResult, ref strStep, objBusqueda);

                  xmlParam.SetAttr("activeOrder", pParam.GetAttr("activeOrder"));
                  xmlParam.SetAttr("order", pParam.GetAttr("order")); 

                  xmlParam.SetAttr("qryMode", "PAGINATION");
                  NomadLog.Debug("xmlParamOiCV-BusquedaPagination: " + xmlParam.ToString());

                  strStep = "QUERY-LIST";
                  NomadXML xmlResultList = Nomad.NSystem.Base.NomadEnvironment.QueryNomadXML(NucleusRH.Base.Postulantes.CV.CV.Resources.QRY_BUSCAR_CV, xmlParam.ToString());
                  NomadXML xmlList = xmlResult.AddTailElement("LIST");
                  xmlList.SetAttr("name", "OI-CV");
                  xmlList.SetAttr("mode", "OI-CV");
                  
                  strStep = "READ-LIST";
                  for (NomadXML row = xmlResultList.FirstChild().FirstChild(); row != null; row = row.Next())
                  {
                      NomadXML xmlItem = xmlList.AddTailElement("CV");
                      xmlItem.SetAttr("oi_cv", row.GetAttr("oi_cv"));                      
                  }
              }

              if (strMode == "CARPETA")
              {
                  strStep = "GET-CVs-Carpeta" + pParam.GetAttrString("oi_carpeta");
                  NomadXML xmlParam = new NomadXML("DATA");
                  xmlParam.SetAttr("oi_carpeta", pParam.GetAttr("oi_carpeta"));
                  xmlParam.SetAttr("activeOrder", pParam.GetAttr("activeOrder"));
                  xmlParam.SetAttr("order", pParam.GetAttr("order"));                  

                  strStep = "QRY-LIST-CAR";
                  xmlParam.SetAttr("qryMode", "PAGINATION");

                  if (pParam.GetAttr("l_leido") != "")
                      xmlParam.SetAttr("l_leido", pParam.GetAttr("l_leido"));

                  if (pParam.GetAttr("c_marca") != "")
                      xmlParam.SetAttr("c_marca", pParam.GetAttr("c_marca"));

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

                  NomadLog.Debug("xmlParamOiCV-CarpetaPagination: " + xmlParam.ToString());

                  NomadXML xmlLoad = Nomad.NSystem.Base.NomadEnvironment.QueryNomadXML(NucleusRH.Base.Postulantes.Carpetas.CAR_CV.Resources.GET_CVS_CAR, xmlParam.ToString());
                  xmlResult.AddTailElement(xmlLoad);
              }
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


      #endregion
      
      /// <summary>
      /// Genera la imagen del Reporte CV PDF
      /// </summary>
      /// <param name="DDO"></param>
      /// <returns></returns>
      public static void GENERATE_XML_REPORT(ref Nomad.NSystem.Proxy.NomadXML pXMLRep)
      {
          NomadEnvironment.GetTrace().Info("------------------------------------------------");
          NomadEnvironment.GetTrace().Info("COMENZANDO GENERATE_XML_REPORT");
          NomadEnvironment.GetTrace().Info("pXMLRep: " + pXMLRep.ToString());
          NomadEnvironment.GetTrace().Info("------------------------------------------------");

          string strStep = "";
          pXMLRep = pXMLRep.FirstChild();
          try
          {

              string strFileName = "";
              NomadProxy objProxy = NomadProxy.GetProxy();

              NomadXML xmlParam = new NomadXML("DATA");
              xmlParam.SetAttr("oi_cv", pXMLRep.GetAttr("id"));

              NomadXML xmlLoad = Nomad.NSystem.Base.NomadEnvironment.QueryNomadXML(NucleusRH.Base.Postulantes.CV.CV.Resources.LOGO_CV_PDF, xmlParam.ToString());

              strStep = "LOGO";

              if (!System.IO.File.Exists(objProxy.RunPath + "Nomad\\TEMP\\NucleusRH.Base.Personal.Imagenes.HEAD." + xmlLoad.FirstChild().GetAttr("oi_logotipo") + ".jpg"))
              {
                  NomadLog.Debug("GENERATE_XML_REPORT: No existe el archivo: Nomad\\TEMP\\NucleusRH.Base.Personal.Imagenes.HEAD." + xmlLoad.FirstChild().GetAttr("oi_logotipo") + ".jpg");

                  //Si no tiene imagen, devuelvo la ruta vacia
                  if (xmlLoad.FirstChild().GetAttr("oi_logotipo") == "")
                  {
                      NomadLog.Debug("No tiene imagen");
                      pXMLRep.FirstChild().SetAttr("ruta", "");
                  }
                  else
                  {

                      BINFile objFile;
                      const string strClase = "NucleusRH.Base.Personal.Imagenes.HEAD";

                      objFile = objProxy.BINService().GetFile(strClase, xmlLoad.FirstChild().GetAttr("oi_logotipo"));

                      strFileName = objFile.SaveFile(objProxy.RunPath + "Nomad\\TEMP");
                  }
              }
              else
              {   
                  //Existe el archivo                  
                  strFileName = objProxy.RunPath + "Nomad\\TEMP\\NucleusRH.Base.Personal.Imagenes.HEAD." + xmlLoad.FirstChild().GetAttr("oi_logotipo") + ".jpg";

              }

              pXMLRep.SetAttr("ruta", strFileName);


              //Para la FOTO
              strStep = "FOTO";
              string strFoto;
              if (xmlLoad.FirstChild().GetAttr("oi_foto") == "no")
              {
                  //strFoto = "C:\\NOMAD\\INSTANCES\\Desa185\\Services\\APPLICATION\\..\\..\\Web\\Images\\Reports\\defaultimg.JPG";
                  strFoto = objProxy.RunPath + "\\Web\\Images\\Reports\\defaultimg.JPG";
              }
              else
              {
                  if (!System.IO.File.Exists(objProxy.RunPath + "Nomad\\TEMP\\NucleusRH.Base.Postulantes.DocumentosDigitales.HEAD." + xmlLoad.FirstChild().GetAttr("oi_foto") + ".jpg"))
                  {                      
                      BINFile objFile;
                      const string strClase = "NucleusRH.Base.Postulantes.DocumentosDigitales.HEAD";

                      objFile = objProxy.BINService().GetFile(strClase, xmlLoad.FirstChild().GetAttr("oi_foto"));

                      strFoto = objFile.SaveFile(objProxy.RunPath + "Nomad\\TEMP");
                  }
                  else
                  {

                      strFoto = objProxy.RunPath + "Nomad\\TEMP\\NucleusRH.Base.Postulantes.DocumentosDigitales.HEAD." + xmlLoad.FirstChild().GetAttr("oi_foto") + ".jpg";

                  }
              }

              pXMLRep.SetAttr("foto", strFoto);

              //return pXMLRep;
          }
          catch (Exception ex)
          {
              NomadException nmdEx = new NomadException("NucleusRH.Base.Postulantes.CV.GENERATE_XML_REPORT()", ex);
              nmdEx.SetValue("Step", strStep);
              throw nmdEx;
          }
      }

     
      #region Buscar CV desde Pedidos
      
      /// <summary>
      /// Carga incial de la pantalla de buscar cv desde pedidos y colaboradores
      /// </summary>
      /// <param name="pParam"></param>
      /// <returns></returns>
      public static NomadXML WEB_INIT_BCV_PEDYCOL(NomadXML pParam)
      { 
         NomadLog.Debug("-----------------------------------------------");
         NomadLog.Debug("----------WEB_INIT_BCV_PEDYCOL------------");
         NomadLog.Debug("-----------------------------------------------");

         NomadLog.Debug("WEB_INIT_BCV_PEDYCOL.pParam: " + pParam.ToString());

          NomadXML xmlResult;
          string strMode;
          string strStep = "";                    
         
          try
          {
              strMode = pParam.GetAttr("mode");          
              xmlResult = new NomadXML("DATA");
              xmlResult.SetAttr("mode", strMode);
              strStep = "LOAD-PESTAÑA-BUSQUEDAS";
              NomadXML xmlParam = new NomadXML("DATA");
              //Top 10 para reusar el recurso con otro limite
              xmlParam.SetAttr("top","10");
              
              NomadXML xmlQry = new NomadXML();
              if(pParam.GetAttr("mias")=="1")
              {
                xmlParam.SetAttr("oi_usuario_sistema",Nomad.NSystem.Base.NomadEnvironment.GetProxy().UserEtty);
                xmlQry = Nomad.NSystem.Base.NomadEnvironment.QueryNomadXML(NucleusRH.Base.Postulantes.Busquedas.BUSQUEDA.Resources.GET_MIS_BUSQUEDAS,xmlParam.ToString());                    
              }
              else
              {
                xmlQry = Nomad.NSystem.Base.NomadEnvironment.QueryNomadXML(NucleusRH.Base.Postulantes.Busquedas.BUSQUEDA.Resources.GET_BUSQUEDAS_P,xmlParam.ToString());                    
              }
              
              strStep = "SET-RESULT-BUQUEDA";
              xmlResult.AddTailElement(xmlQry);



              //ILT 28.10.2016
              strStep = "SET-OFERTA-LABORAL";              
              NomadXML xmlOfertasLab = new NomadXML("OFERTAS-LAB");
              xmlOfertasLab.SetAttr("name","OFERTAS-LAB");
              string c_oferta_lab = "";
              if (pParam.GetAttr("oi_pedido") != "")
              {
                  NucleusRH.BaytonRH.PedidosyColaboradores.Pedidos.PEDIDO objPedido = NucleusRH.BaytonRH.PedidosyColaboradores.Pedidos.PEDIDO.Get(pParam.GetAttr("oi_pedido"));
                  if (objPedido.oi_oferta_lab != "")
                  {
                      NucleusRH.Base.Postulantes.Ofertas_Laborales.OFERTA_LAB objOferta = NucleusRH.Base.Postulantes.Ofertas_Laborales.OFERTA_LAB.Get(objPedido.oi_oferta_lab);
                      c_oferta_lab = objOferta.c_oferta_lab;
                  }
              }
              xmlOfertasLab.SetAttr("c_oferta_lab",c_oferta_lab);
              xmlResult.AddTailElement(xmlOfertasLab);


              
              strStep = "LOAD-PESTAÑA-CARPETAS";
              NomadXML xmlParamCarpeta = new NomadXML("DATA");
              xmlParamCarpeta.SetAttr("order", "asc");
              xmlParamCarpeta.SetAttr("activeOrder", "TIT");
              xmlParamCarpeta.SetAttr("qryMode", "LISTCAR");
              
              //Para cargar todas
              xmlParamCarpeta.SetAttr("oi_usuario_sistema", "");
              
              //PYC para indicarle al mismo recurso que es de PYC
              xmlParamCarpeta.SetAttr("PYC", "1");

              xmlQry = Nomad.NSystem.Base.NomadEnvironment.QueryNomadXML(NucleusRH.Base.Postulantes.Carpetas.CARPETA.Resources.GET_ALL, xmlParamCarpeta.ToString());

              strStep = "SET-RESULT-CARPETA";
              xmlResult.AddTailElement(xmlQry);


              strStep = "LOAD-PESTAÑA-BUSQUEDA-AVANZADA";
              //Invoco metodo que retorna xml de carga de pantalla de busqued avanzada
              NomadXML xmlParamAdvs = new NomadXML("DATA");
              xmlParamAdvs.SetAttr("mode", "PYCADVSEARCH");
              
              //NomadXML xmlAdvancedSearch = NucleusRH.Base.Postulantes.CV.CV.WEB_LoadAdvancedSearch(xmlParamAdvs);
              
              NomadXML xmlLoad = Nomad.NSystem.Base.NomadEnvironment.QueryNomadXML(NucleusRH.Base.Postulantes.CV.CV.Resources.QRY_LOAD_ADVSEARCH, "");

              strStep = "SET-RESULT-ADVS";
              #region AREAS-LAB
              strStep = "AREAS-LAB";
              NomadXML xmlAreasLaborales = xmlResult.AddTailElement("AREAS-LAB");
              xmlAreasLaborales.SetAttr("name", "AREASLAB");
              xmlAreasLaborales.SetAttr("mode", "LOAD-AL");

              for (NomadXML row = xmlLoad.FirstChild().FindElement("AREAS-LAB").FirstChild(); row != null; row = row.Next())
              {
                  NomadXML xmlAreaLab = xmlAreasLaborales.AddTailElement("AL");
                  xmlAreaLab.SetAttr("id", row.GetAttr("id"));
                  xmlAreaLab.SetAttr("desc", row.GetAttr("desc"));
              }

              #endregion

              #region NIVEL-EST
              strStep = "NIVEL-EST";
              NomadXML xmlNivelEstudios = xmlResult.AddTailElement("NIVELES-ESTUDIO");
              xmlNivelEstudios.SetAttr("name", "NIVELESESTUDIO");
              xmlNivelEstudios.SetAttr("mode", "LOAD-NE");
              for (NomadXML row = xmlLoad.FirstChild().FindElement("NIVELES-ESTUDIO").FirstChild(); row != null; row = row.Next())
              {
                  NomadXML xmlNivelEst = xmlNivelEstudios.AddTailElement("ESTADOEST");
                  xmlNivelEst.SetAttr("id", row.GetAttr("id"));
                  xmlNivelEst.SetAttr("desc", row.GetAttr("desc"));
              }

              #endregion

              #region ESTADO-EST
              strStep = "ESTADO-EST";
              NomadXML xmlEstadosEstudio = xmlResult.AddTailElement("ESTADOS-ESTUDIO");
              xmlEstadosEstudio.SetAttr("name", "ESTADOSESTUDIO");
              xmlEstadosEstudio.SetAttr("mode", "LOAD-EE");
              for (NomadXML row = xmlLoad.FirstChild().FindElement("ESTADOS-ESTUDIO").FirstChild(); row != null; row = row.Next())
              {
                  NomadXML xmlEstadoEst = xmlEstadosEstudio.AddTailElement("ESTADOEST");
                  xmlEstadoEst.SetAttr("id", row.GetAttr("id"));
                  xmlEstadoEst.SetAttr("desc", row.GetAttr("desc"));
              }

              #endregion

              #region NIVELES-IDIOMAS
              strStep = "NIVELES-IDIOMA";
              NomadXML xmlNivelesIdioma = xmlResult.AddTailElement("NIVELES-IDIOMA");
              xmlNivelesIdioma.SetAttr("name", "NIVID");
              xmlNivelesIdioma.SetAttr("mode", "LOAD-NI");
              for (NomadXML row = xmlLoad.FirstChild().FindElement("NIVELES-IDIOMA").FirstChild(); row != null; row = row.Next())
              {
                  NomadXML xmlNivelId = xmlNivelesIdioma.AddTailElement("NIVID");
                  xmlNivelId.SetAttr("id", row.GetAttr("id"));
                  xmlNivelId.SetAttr("desc", row.GetAttr("desc"));
              }

              #endregion

              #region PAISES
              strStep = "PAISES";
              NomadXML xmlPaises = xmlResult.AddTailElement("PAISES");
              xmlPaises.SetAttr("name", "PAIS");
              xmlPaises.SetAttr("mode", "LOAD-PAIS");
              for (NomadXML row = xmlLoad.FirstChild().FindElement("PAISES").FirstChild(); row != null; row = row.Next())
              {
                  NomadXML xmlPais = xmlPaises.AddTailElement("PAIS");
                  xmlPais.SetAttr("id", row.GetAttr("id"));
                  xmlPais.SetAttr("desc", row.GetAttr("desc"));
              }
              #endregion

              #region PROVINCIAS
              strStep = "PROVINCIAS";
              NomadXML xmlPcias = xmlResult.AddTailElement("PROVINCIAS");
              xmlPcias.SetAttr("name", "PROVINCIA");
              xmlPcias.SetAttr("mode", "LOAD-PROVINCIAS");
              for (NomadXML row = xmlLoad.FirstChild().FindElement("PROVINCIAS").FirstChild(); row != null; row = row.Next())
              {
                  NomadXML xmlPcia = xmlPcias.AddTailElement("PCIA");
                  xmlPcia.SetAttr("id", row.GetAttr("id"));
                  xmlPcia.SetAttr("desc", row.GetAttr("desc"));
              }
              #endregion
              
              
             
          }
          catch (Exception ex)
          {

              NomadException nmdEx = new NomadException("NucleusRH.Base.Postulantes.CV.WEB_INIT_BCV_PEDYCOL()", ex);
              nmdEx.SetValue("Step", strStep);
              throw nmdEx;
          }

          NomadLog.Debug("xmlResultWEB_INIT_BCV_PEDYCOL: " + xmlResult.ToString());
          return xmlResult;
      }



      /// <summary>
      /// Cambia la lista de busqueda de buscar cv de pedidos y colaboradores
      /// </summary>
      /// <param name="pParam"></param>
      /// <returns></returns>
      public static NomadXML WEB_CHANGE_SEARCH_PYC(NomadXML pParam)
      {
          NomadLog.Debug("-----------------------------------------------");
          NomadLog.Debug("----------WEB_CHANGE_SEARCH_PYC------------");
          NomadLog.Debug("-----------------------------------------------");

          NomadLog.Debug("WEB_CHANGE_SEARCH_PYC.pParam: " + pParam.ToString());

          NomadXML xmlResult;
          string strMode;
          string strStep = "";

          try
          {
              strMode = pParam.GetAttr("mode");
              xmlResult = new NomadXML("DATA");
              xmlResult.SetAttr("mode", strMode);
              
              strStep = "CHANGE-SEARCH-PYC";
              NomadXML xmlParam = new NomadXML("DATA");
              //Top 10 para reusar el recurso con otro limite
              xmlParam.SetAttr("top", "10");

              NomadXML xmlQry = new NomadXML();
              if (pParam.GetAttr("mias") == "1")
              {
                  xmlParam.SetAttr("oi_usuario_sistema", Nomad.NSystem.Base.NomadEnvironment.GetProxy().UserEtty);
                  xmlQry = Nomad.NSystem.Base.NomadEnvironment.QueryNomadXML(NucleusRH.Base.Postulantes.Busquedas.BUSQUEDA.Resources.GET_MIS_BUSQUEDAS, xmlParam.ToString());
              }
              else
              {
                  xmlQry = Nomad.NSystem.Base.NomadEnvironment.QueryNomadXML(NucleusRH.Base.Postulantes.Busquedas.BUSQUEDA.Resources.GET_BUSQUEDAS_P, xmlParam.ToString());
              }


              strStep = "SET-RESULT";
              xmlResult.AddTailElement(xmlQry);
          }
          catch (Exception ex)
          {

              NomadException nmdEx = new NomadException("NucleusRH.Base.Postulantes.CV.WEB_CHANGE_SEARCH_PYC()", ex);
              nmdEx.SetValue("Step", strStep);
              throw nmdEx;
          }

          NomadLog.Debug("xmlResultWEB_CHANGE_SEARCH_PYC: " + xmlResult.ToString());
          return xmlResult;
      }
      
      /// <summary>
      /// Filtra los cvs del buscador de cv de pedidos y colaboradores
      /// TAB: - B para Busquedas
      ///      - BA para Busquedas Avanzadas
      ///      -C: Para carpetas
      ///      -O: Para oofertas laborales
      /// </summary>
      /// <param name="pParam"></param>
      /// <returns></returns>
      public static NomadXML WEB_CV_FILTER_PYC(NomadXML pParam)
      {
          NomadLog.Debug("-----------------------------------------------");
          NomadLog.Debug("----------WEB_CV_FILTER_PYC--------------------");
          NomadLog.Debug("-----------------------------------------------");

          NomadLog.Debug("WEB_CV_FILTER_PYC.pParam: " + pParam.ToString());

          NomadXML xmlResult;
          string strMode;
          string strStep = "";

          try
          {
              strMode = pParam.GetAttr("mode");
              xmlResult = new NomadXML("DATA");
              xmlResult.SetAttr("mode", strMode);
              
              NomadXML xmlParamListCV = new NomadXML("DATA");
              //PYC para buscar cv desde pedidos colaboradores 
              xmlParamListCV.SetAttr("PYC", "1");
              xmlParamListCV.SetAttr("oi_pedido", pParam.GetAttr("oi_pedido"));

              NucleusRH.Base.Postulantes.Busquedas.BUSQUEDA objBusqueda;  
              strStep = "TAB-BUSQUEDA";

              if ((pParam.GetAttr("TAB") == "B") || (pParam.GetAttr("TAB") == "BA"))
               {

                   if (pParam.GetAttr("TAB") == "B")
                   {
                       strStep = "TB-GET-BUSQUEDA";
                       objBusqueda = Busquedas.BUSQUEDA.Get(pParam.GetAttr("oi_busqueda"));
                   }
                   else
                   {//Sino, creo el objeto a pata y tiro la misma busqueda
                       objBusqueda = new Busquedas.BUSQUEDA();

                       if (pParam.GetAttr("d_puesto") != "")                       
                           objBusqueda.d_puesto = pParam.GetAttr("d_puesto");                                                  
                       else
                           objBusqueda.d_puestoNull = true;
                       
                       if ((pParam.GetAttr("l_trab_puesto") != "0") && (pParam.GetAttr("l_trab_puesto") != ""))                       
                           objBusqueda.l_trab_puesto = true;                                                  
                       else
                           objBusqueda.l_trab_puesto = false;
                       
                       if ((pParam.GetAttr("c_leido") == "T"))                       
                           objBusqueda.c_leido = "T";
                       
                       if ((pParam.GetAttr("c_leido") == "L"))                       
                           objBusqueda.c_leido = "L";                                                 
                       
                       if ((pParam.GetAttr("c_leido") == "NL"))                       
                           objBusqueda.c_leido = "NL";                       
                       
                       if (pParam.GetAttr("e_anio_desde") != "")
                           objBusqueda.e_anio_desde = pParam.GetAttrInt("e_anio_desde");
                       else
                           objBusqueda.e_anio_desdeNull = true;
                     
                       if (pParam.GetAttr("e_anio_hasta") != "")
                           objBusqueda.e_anio_hasta = pParam.GetAttrInt("e_anio_hasta");
                       else
                           objBusqueda.e_anio_hastaNull = true;
                       
                       if (pParam.GetAttr("d_titulo") != "")                       
                           objBusqueda.d_titulo = pParam.GetAttr("d_titulo");                                              
                       else
                           objBusqueda.d_tituloNull = true;                       
                       
                       if (pParam.GetAttr("d_empresa") != "")                       
                           objBusqueda.d_empresa = pParam.GetAttr("d_empresa");                       
                       else
                           objBusqueda.d_empresaNull = true;
                       
                       if (pParam.GetAttr("o_palabras_clave") != "")                       
                           objBusqueda.o_palabras_clave = pParam.GetAttr("o_palabras_clave");                           
                       else
                           objBusqueda.o_palabras_claveNull = true;

                       //Agrego campos child                       
                       for (NomadXML row = pParam.FindElement("IDIOMAS").FirstChild(); row != null; row = row.Next())
                       {
                           if (row.GetAttr("nivel_general") != "I")
                           {
                               NucleusRH.Base.Postulantes.Busquedas.IDIOMAS objIdioma = new Busquedas.IDIOMAS();
                               objIdioma.oi_idioma = row.GetAttr("oi_idioma");
                               objIdioma.oi_nivel_general = row.GetAttr("nivel_general");                               

                               objBusqueda.IDIOMA_BUS.Add(objIdioma);                               
                           }

                       }

                       if ((pParam.GetAttr("c_sexo") != "I") && (pParam.GetAttr("c_sexo") != ""))
                           objBusqueda.c_sexo = pParam.GetAttr("c_sexo");
                       else
                           objBusqueda.c_sexoNull = true;                       
                       
                       if (pParam.GetAttr("e_edad_desde") != "")
                           objBusqueda.e_edad_desde = pParam.GetAttrInt("e_edad_desde");
                       else
                           objBusqueda.e_edad_desdeNull = true;
                       
                       if (pParam.GetAttr("e_edad_hasta") != "")
                           objBusqueda.e_edad_hasta = pParam.GetAttrInt("e_edad_hasta");
                       else
                           objBusqueda.e_edad_hastaNull = true;                       

                       
                       if (pParam.GetAttr("n_salario_desde") != "")
                           objBusqueda.n_salario_desde = pParam.GetAttrDouble("n_salario_desde");
                       else
                           objBusqueda.n_salario_desdeNull = true;
                       
                       if (pParam.GetAttr("n_salario_hasta") != "")
                           objBusqueda.n_salario_hasta = pParam.GetAttrDouble("n_salario_hasta");
                       else
                           objBusqueda.n_salario_hastaNull = true;
                       
                       if ((pParam.GetAttr("c_trabajando") != "I") && (pParam.GetAttr("c_trabajando") != ""))                       
                           objBusqueda.c_trabajando = pParam.GetAttr("c_trabajando");                                              
                       else
                           objBusqueda.c_trabajandoNull = true;
                       
                       if ((pParam.GetAttr("c_persona_cargo") != "I") && (pParam.GetAttr("c_persona_cargo") != ""))                       
                           objBusqueda.c_persona_cargo = pParam.GetAttr("c_persona_cargo");                                                  
                       else
                           objBusqueda.c_persona_cargoNull = true;
                       
                       if ((pParam.GetAttr("oi_nivel_estudio") != "I") && (pParam.GetAttr("oi_nivel_estudio") != ""))                       
                           objBusqueda.oi_nivel_estudio = pParam.GetAttr("oi_nivel_estudio");
                           
                       else
                           objBusqueda.oi_nivel_estudioNull = true;
                       
                       if ((pParam.GetAttr("oi_estado_est") != "I") && (pParam.GetAttr("oi_estado_est") != ""))                       
                           objBusqueda.oi_estado_est = pParam.GetAttr("oi_estado_est");                                                
                       else
                           objBusqueda.oi_estado_estNull = true;                       
                       
                       if (pParam.GetAttr("d_ciudad") != "")                       
                           objBusqueda.d_ciudad = pParam.GetAttr("d_ciudad");                                              
                       else
                           objBusqueda.d_ciudadNull = true;

                       
                       if (pParam.GetAttr("oi_pais") != "")                       
                           objBusqueda.oi_pais = pParam.GetAttr("oi_pais");                                              
                       else
                           objBusqueda.oi_paisNull = true;                       
                       
                       if (pParam.GetAttr("d_nombres") != "")                       
                           objBusqueda.d_nombres = pParam.GetAttr("d_nombres");                                                  
                       else
                           objBusqueda.d_nombresNull = true;
                       
                       if (pParam.GetAttr("d_apellido") != "")                       
                           objBusqueda.d_apellido = pParam.GetAttr("d_apellido");
                       
                       else
                           objBusqueda.d_apellidoNull = true;
                       
                       if (pParam.GetAttr("c_nro_doc") != "")                       
                           objBusqueda.c_nro_doc = pParam.GetAttr("c_nro_doc");                                                  
                       else
                           objBusqueda.c_nro_docNull = true;
                       
                       if ((pParam.GetAttr("l_foto") != "0") && (pParam.GetAttr("l_foto") != ""))                       
                           objBusqueda.l_foto = true;                       
                       else
                           objBusqueda.l_foto = false;
                       
                       if ((pParam.GetAttr("l_cv_sin_salario") != "0") && (pParam.GetAttr("l_cv_sin_salario") != ""))
                           objBusqueda.l_cv_sin_salario = true;
                       else
                           objBusqueda.l_cv_sin_salario = false;
                       
                       if (pParam.GetAttr("f_actualizacion") != "")                       
                           objBusqueda.f_actualizacion = pParam.GetAttrDateTime("f_actualizacion");                                                
                       else                       
                           objBusqueda.f_actualizacionNull = true;
                           
                       
                       for (NomadXML row = pParam.FindElement("AREAS-LABORALES").FirstChild(); row != null; row = row.Next())
                       {
                           NucleusRH.Base.Postulantes.Busquedas.AREA_PUESTO objAreaPuesto = new Busquedas.AREA_PUESTO();
                           objAreaPuesto.oi_area_lab = row.GetAttr("oi_area_lab");
                           objBusqueda.A_PUESTO_BUS.Add(objAreaPuesto);                          
                       }
                       
                       for (NomadXML row = pParam.FindElement("PROVINCIAS").FirstChild(); row != null; row = row.Next())
                       {
                           NucleusRH.Base.Postulantes.Busquedas.PROVINCIAS objProvincia = new Busquedas.PROVINCIAS();
                           objProvincia.oi_provincia = row.GetAttr("oi_provincia");
                           objBusqueda.PCIA_BUS.Add(objProvincia);                           
                       }
                       
                       for (NomadXML row = pParam.FindElement("ESTADOS-CV").FirstChild(); row != null; row = row.Next())
                       {
                           NucleusRH.Base.Postulantes.Busquedas.ESTADOS_CV objEstadoCV = new Busquedas.ESTADOS_CV();
                           objEstadoCV.c_estado = row.GetAttr("c_estado");
                           objEstadoCV.d_estado = row.GetAttr("d_estado");
                           objBusqueda.ESTADOS_CV_BUS.Add(objEstadoCV);                          
                       }                                                                                            

                   }
                 

                    strStep = "TB-BA-GET-LIST";                   
                    #region CampoLike
                    strStep = "TB-ARMO-PARAM-pclaves";
                    if (!(objBusqueda.o_palabras_claveNull))
                    {
                        string strCampoLike = objBusqueda.o_palabras_clave;
                        //Separo palabras
                        string[] arrayPalabras = strCampoLike.Split(',');
                        for (int i = 0; (i <= arrayPalabras.Length - 1 && i != 10); i++)
                        {
                            string campo_like = limpiarCadena(arrayPalabras[i]);
                            xmlParamListCV.SetAttr("campo_like_" + (i + 1).ToString(), campo_like);                             
                        }
                    }
                    #endregion

                    #region PanelGeneral
                    strStep = "TB-ARMO-PARAM-l_leido";
                    if (objBusqueda.c_leido == "L")                     
                        xmlParamListCV.SetAttr("l_leido", "1");
                     
                    if (objBusqueda.c_leido == "NL")                     
                        xmlParamListCV.SetAttr("l_leido", "0");                     

                    strStep = "TB-ARMO-PARAM-f_actualizacion";
                    if (!(objBusqueda.f_actualizacionNull))                    
                        xmlParamListCV.SetAttr("f_actualizacion", objBusqueda.f_actualizacion);
                        
                    
                    #endregion

                    #region ExperienciaLaboral
                    strStep = "TB-ARMO-PARAM-d_puesto";
                    if (!(objBusqueda.d_puestoNull))                     
                        xmlParamListCV.SetAttr("d_puesto", limpiarCadena(objBusqueda.d_puesto));                                            

                    strStep = "TB-ARMO-PARAM-l_trab_puesto";
                    if (objBusqueda.l_trab_puesto)                    
                        xmlParamListCV.SetAttr("l_trab_puesto", objBusqueda.l_trab_puesto);                                            

                    strStep = "TB-ARMO-PARAM-e_anio_desde";
                    if (!(objBusqueda.e_anio_desdeNull))
                        xmlParamListCV.SetAttr("e_anio_desde", limpiarCadena(objBusqueda.e_anio_desde.ToString()));

                    strStep = "TB-ARMO-PARAM-e_anio_hasta";
                    if (!(objBusqueda.e_anio_hastaNull))
                        xmlParamListCV.SetAttr("e_anio_hasta", limpiarCadena(objBusqueda.e_anio_hasta.ToString()));
                   
                    strStep = "TB-ARMO-PARAM-c_trabajando";
                    if (!(objBusqueda.c_trabajandoNull))                    
                        xmlParamListCV.SetAttr("c_trabajando", objBusqueda.c_trabajando);
                         
                    

                    strStep = "TB-ARMO-PARAM-areasLab";
                    string paramAreasLab = "IN(0";
                    foreach (NucleusRH.Base.Postulantes.Busquedas.AREA_PUESTO objAreaPuesto in objBusqueda.A_PUESTO_BUS)
                    {
                        paramAreasLab = paramAreasLab + "," + objAreaPuesto.oi_area_lab;
                    }
                    paramAreasLab = paramAreasLab + ")";
                    if (paramAreasLab != "IN(0)")
                    {
                        xmlParamListCV.SetAttr("paramAreasLab", paramAreasLab);
                         
                    }

                    NomadLog.Debug("paramAreasLab: " + paramAreasLab.ToString());

                    strStep = "TB-ARMO-PARAM-d_empresa";
                    if (!(objBusqueda.d_empresaNull))
                    {
                        //hasta 10 empresas                                    
                        string[] empresas = objBusqueda.d_empresa.Split(',');
                        for (int i = 0; (i <= empresas.Length - 1 && i != 10); i++)
                        {
                            string empresa = limpiarCadena(empresas[i]);
                            xmlParamListCV.SetAttr("empresa_" + (i + 1).ToString(), empresa);                            
                        }
                    }

                    strStep = "TB-ARMO-PARAM-c_persona_Cargo";
                    if (!(objBusqueda.c_persona_cargoNull))                    
                        xmlParamListCV.SetAttr("c_persona_cargo", objBusqueda.c_persona_cargo);                        
                    

                    #endregion

                    #region Educacion
                    strStep = "TB-ARMO-PARAM-d_titulo";
                    if (!(objBusqueda.d_tituloNull))
                        xmlParamListCV.SetAttr("d_titulo", limpiarCadena(objBusqueda.d_titulo));                                            

                    strStep = "TB-ARMO-PARAM-oi_nivel_estudio";
                    if (!(objBusqueda.oi_nivel_estudioNull))                    
                        xmlParamListCV.SetAttr("oi_nivel_estudio", objBusqueda.oi_nivel_estudio);                        
                    

                    strStep = "TB-ARMO-PARAM-oi_estado_est";
                    if (!(objBusqueda.oi_estado_estNull))                    
                       xmlParamListCV.SetAttr("oi_estado_est", objBusqueda.oi_estado_est);
                        
                    

                    strStep = "TB-ARMO-PARAM-idiomas";
                    foreach (NucleusRH.Base.Postulantes.Busquedas.IDIOMAS objIdioma in objBusqueda.IDIOMA_BUS)
                    {
                        xmlParamListCV.SetAttr("oi_idioma_" + objIdioma.oi_idioma.ToString(), objIdioma.oi_idioma);
                        xmlParamListCV.SetAttr("oi_nivel_general_" + objIdioma.oi_idioma.ToString(), objIdioma.oi_nivel_general);                                               
                    }
                    #endregion

                    #region Datos Personales
                    strStep = "TB-ARMO-PARAM-d_nombres";
                    if (!(objBusqueda.d_nombresNull))                    
                        xmlParamListCV.SetAttr("d_nombres", limpiarCadena(objBusqueda.d_nombres));
                        


                    strStep = "TB-ARMO-PARAM-d_apellido";
                    if (!(objBusqueda.d_apellidoNull))                   
                       xmlParamListCV.SetAttr("d_apellido", limpiarCadena(objBusqueda.d_apellido));
                   

                    strStep = "TB-ARMO-PARAM-c_sexo";
                    if (!(objBusqueda.c_sexoNull))                     
                        xmlParamListCV.SetAttr("c_sexo", objBusqueda.c_sexo);
                         

                    strStep = "TB-ARMO-PARAM-c_nro_doc";
                    if (!(objBusqueda.c_nro_docNull))                    
                        xmlParamListCV.SetAttr("c_nro_doc", limpiarCadena(objBusqueda.c_nro_doc));
                         

                    strStep = "TB-ARMO-PARAM-e_edad_desde";
                    if (!(objBusqueda.e_edad_desdeNull))
                        xmlParamListCV.SetAttr("e_edad_desde", limpiarCadena(objBusqueda.e_edad_desde.ToString()));

                    strStep = "TB-ARMO-PARAM-e_edad_hasta";
                    if (!(objBusqueda.e_edad_hastaNull))
                        xmlParamListCV.SetAttr("e_edad_hasta", limpiarCadena(objBusqueda.e_edad_hasta.ToString()));
                  
                    strStep = "TB-ARMO-PARAM-oi_pais";
                    if (!(objBusqueda.oi_paisNull))                     
                        xmlParamListCV.SetAttr("oi_pais", objBusqueda.oi_pais);                        

                    //Pcia
                    strStep = "TB-ARMO-PARAM-pcia";
                    string paramProvincia = "IN(0";
                    foreach (NucleusRH.Base.Postulantes.Busquedas.PROVINCIAS objProvincia in objBusqueda.PCIA_BUS)
                    {
                        paramProvincia = paramProvincia + "," + objProvincia.oi_provincia;
                    }
                    paramProvincia = paramProvincia + ")";
                    if (paramProvincia != "IN(0)")
                        xmlParamListCV.SetAttr("paramProvincia", paramProvincia);

                    strStep = "TB-ARMO-PARAM-d_ciudad";
                    if (!(objBusqueda.d_ciudadNull))                     
                        xmlParamListCV.SetAttr("d_ciudad", limpiarCadena(objBusqueda.d_ciudad));
                         

                    strStep = "TB-ARMO-PARAM-l_foto";
                    if (objBusqueda.l_foto)                     
                        xmlParamListCV.SetAttr("l_foto", objBusqueda.l_foto);
                         
                    //Estados CV
                    strStep = "TB-ARMO-PARAM-estado-cv";
                    string paramEstadoCV = "IN(\\'0\\'";
                    foreach (NucleusRH.Base.Postulantes.Busquedas.ESTADOS_CV objEstadoCV in objBusqueda.ESTADOS_CV_BUS)
                    {
                        paramEstadoCV = paramEstadoCV + "," + "\\'" + objEstadoCV.c_estado + "\\'";
                    }
                    paramEstadoCV = paramEstadoCV + ")";
                    if (paramEstadoCV != "IN(\\'0\\')")
                        xmlParamListCV.SetAttr("paramEstadoCV", paramEstadoCV);
                                       

                    //Oferta Laboral
                    strStep = "TB-ARMO-PARAM-oi_oferta_lab";
                    if (!(objBusqueda.oi_oferta_labNull))                    
                        xmlParamListCV.SetAttr("oi_oferta_lab", objBusqueda.oi_oferta_lab);
                         
                    #endregion

                    #region RangoSalarial
                    strStep = "ARMO-PARAM-n_salario_desde";
                    if (!(objBusqueda.n_salario_desdeNull))
                        xmlParamListCV.SetAttr("n_salario_desde", limpiarCadena(objBusqueda.n_salario_desde.ToString()));

                    strStep = "ARMO-PARAM-n_salario_hasta";
                    if (!(objBusqueda.n_salario_hastaNull))
                        xmlParamListCV.SetAttr("n_salario_hasta", limpiarCadena(objBusqueda.n_salario_hasta.ToString()));
                   

                    strStep = "TB-ARMO-PARAM-lcv_sin_salario";
                    NomadLog.Debug("l_cv_sin_salario: " + objBusqueda.l_cv_sin_salario.ToString());
                    if (objBusqueda.l_cv_sin_salario)                    
                        xmlParamListCV.SetAttr("l_cv_sin_salario", objBusqueda.l_cv_sin_salario);
                       
                    #endregion
                                                                                                                    
                    

                    xmlParamListCV.SetAttr("qryMode", "LISTCV");
                    NomadLog.Debug("PYC-xmlParamListCV-List: " + xmlParamListCV.ToString());

                    strStep = "QUERY-LIST-BUS";
                    NomadXML xmlResultList = Nomad.NSystem.Base.NomadEnvironment.QueryNomadXML(NucleusRH.Base.Postulantes.CV.CV.Resources.QRY_BUSCAR_CV, xmlParamListCV.ToString());

                    strStep = "SET-RESULT";
                    xmlResult.AddTailElement(xmlResultList);
                }
                
                strStep = "TAB-CARPETA";
                if (pParam.GetAttr("TAB") == "C")
                {
                    
                    xmlParamListCV.SetAttr("qryMode", "LISTCAR");
                    xmlParamListCV.SetAttr("oi_carpeta", pParam.GetAttr("oi_carpeta"));
                    xmlParamListCV.SetAttr("order", "asc");
                    xmlParamListCV.SetAttr("activeOrder", "AN");                
                    
                    //Marca del CV
                    if (pParam.GetAttr("filtroMarcaCar") != "T")
                        xmlParamListCV.SetAttr("c_marca", pParam.GetAttr("filtroMarcaCar"));
                
                    //Lectura de CVs
                    if(pParam.GetAttr("filtroLecturaCar")=="L")
                        xmlParamListCV.SetAttr("l_leido",  "1");

                    
                    if (pParam.GetAttr("filtroLecturaCar") == "NL")
                        xmlParamListCV.SetAttr("l_leido", "0");        

                    NomadLog.Debug("PYC-xmlParamListCV-List: " + xmlParamListCV.ToString());
                    strStep = "QUERY-LIST-CAR";
                    NomadXML xmlResultCar = Nomad.NSystem.Base.NomadEnvironment.QueryNomadXML(NucleusRH.Base.Postulantes.Carpetas.CAR_CV.Resources.GET_CVS_CAR,xmlParamListCV.ToString());

                    strStep = "SET-RESULT";
                    xmlResult.AddTailElement(xmlResultCar);

                }
              strStep = "TAB-CARPETA";
              if (pParam.GetAttr("TAB") == "OL")
              {
                  xmlParamListCV.SetAttr("qryMode", "LISTPOSTUCV");
                  xmlParamListCV.SetAttr("oi_oferta_lab", pParam.GetAttr("oi_oferta_lab"));
                  xmlParamListCV.SetAttr("criterio", "asc");
                  xmlParamListCV.SetAttr("campo", "d_ape_y_nom");

                  //Marca del CV
                  if (pParam.GetAttr("filtroMarcaOL") != "T")
                      xmlParamListCV.SetAttr("c_marca", pParam.GetAttr("filtroMarcaOL"));

                  //Lectura de CVs
                  if (pParam.GetAttr("filtroOL") == "L")
                      xmlParamListCV.SetAttr("l_leido", "1");


                  if (pParam.GetAttr("filtroOL") == "NL")
                      xmlParamListCV.SetAttr("l_leido", "0");

                  NomadLog.Debug("PYC-xmlParamListCV-List: " + xmlParamListCV.ToString());
                  NomadXML xmlResultOL = Nomad.NSystem.Base.NomadEnvironment.QueryNomadXML(NucleusRH.Base.Postulantes.Ofertas_Laborales.OFERTA_LAB.Resources.GET_CV_OFERTA_LAB, xmlParamListCV.ToString());

                  strStep = "SET-RESULT";
                  xmlResult.AddTailElement(xmlResultOL);
              }
                
          }
          catch (Exception ex)
          {
              NomadException nmdEx = new NomadException("NucleusRH.Base.Postulantes.CV.WEB_CV_FILTER_PYC()", ex);
              nmdEx.SetValue("Step", strStep);
              throw nmdEx;
          }

          NomadLog.Debug("xmlResultWEB_CV_FILTER_PYC: " + xmlResult.ToString());
          return xmlResult;
      }


      /// <summary>
      /// Carga la lista de carpetas, todas o mias segun parametro
      /// </summary>
      /// <param name="pParam"></param>
      /// <returns></returns>
      public static NomadXML WEB_CV_LOADCARPETA_PYC(NomadXML pParam)
      { 
          NomadLog.Debug("-----------------------------------------------");
          NomadLog.Debug("----------WEB_CV_LOADCARPETA_PYC--------------------");
          NomadLog.Debug("-----------------------------------------------");

          NomadLog.Debug("WEB_CV_LOADCARPETA_PYC.pParam: " + pParam.ToString());

          NomadXML xmlResult;
          string strMode;
          string strStep = "";

          try
          {
              
              strMode = pParam.GetAttr("mode");
              xmlResult = new NomadXML("DATA");
              xmlResult.SetAttr("mode", strMode);

              NomadXML xmlParamCarpeta = new NomadXML("DATA");
              xmlParamCarpeta.SetAttr("order", "asc");
              xmlParamCarpeta.SetAttr("activeOrder", "TIT");
              xmlParamCarpeta.SetAttr("qryMode", "LISTCAR");              

              //Mias 1 desde el cliente agrega filtro de usuario logueado para qry carpetas
              if (pParam.GetAttr("mias") == "1")                                
                  xmlParamCarpeta.SetAttr("oi_usuario_sistema", Nomad.NSystem.Proxy.NomadProxy.GetProxy().UserEtty);


              NomadXML xmlQry = Nomad.NSystem.Base.NomadEnvironment.QueryNomadXML(NucleusRH.Base.Postulantes.Carpetas.CARPETA.Resources.GET_ALL, xmlParamCarpeta.ToString());
              
              xmlResult.AddTailElement(xmlQry);

          }
          catch (Exception ex)
          {
              NomadException nmdEx = new NomadException("NucleusRH.Base.Postulantes.CV.WEB_CV_LOADCARPETA_PYC()", ex);
              nmdEx.SetValue("Step", strStep);
              throw nmdEx;
          }

          NomadLog.Debug("xmlResultWEB_CV_LOADCARPETA_PYC: " + xmlResult.ToString());
          return xmlResult;
             
      }

      /// <summary>
      /// Carga la oferta laboral para el parametro requerido de campo de busqueda y  el estado de dicha oferta
      /// </summary>
      /// <param name="pParam"></param>
      /// <returns></returns>
      public static NomadXML WEB_CV_LOADOL_PYC(NomadXML pParam)
      {          
          NomadLog.Debug("-----------------------------------------------");
          NomadLog.Debug("----------WEB_CV_LOADOL_PYC--------------------");
          NomadLog.Debug("-----------------------------------------------");

          NomadLog.Debug("WEB_CV_LOADOL_PYC.pParam: " + pParam.ToString());

          NomadXML xmlResult;
          string strMode;
          string strStep = "";

          try
          {
              strMode = pParam.GetAttr("mode");
              xmlResult = new NomadXML("DATA");
              xmlResult.SetAttr("mode", strMode);

              NomadXML xmlParamOL = new NomadXML("DATA");
              xmlParamOL.SetAttr("criterio", "desc");
              xmlParamOL.SetAttr("campo", "c_oferta_lab");
              xmlParamOL.SetAttr("qryMode", "LISTOL");       
              //Campo buscar
              xmlParamOL.SetAttr("campo_buscar", pParam.GetAttr("campo_buscar"));       
              
              //Estado de la oferta
              xmlParamOL.SetAttr("estado", pParam.GetAttr("estado"));
              
              NomadLog.Debug("WEB_CV_LOADOL_PYC.xmlParamOL: " + xmlParamOL.ToString());

              NomadXML xmlQry = Nomad.NSystem.Base.NomadEnvironment.QueryNomadXML(NucleusRH.Base.Postulantes.Ofertas_Laborales.OFERTA_LAB.Resources.GET_OFERTAS_LIST, xmlParamOL.ToString());

              xmlResult.AddTailElement(xmlQry);
          }
          catch (Exception ex)
          {
              NomadException nmdEx = new NomadException("NucleusRH.Base.Postulantes.CV.WEB_CV_LOADOL_PYC()", ex);
              nmdEx.SetValue("Step", strStep);
              throw nmdEx;
          }

          NomadLog.Debug("xmlResultWEB_CV_LOADOL_PYC: " + xmlResult.ToString());
          return xmlResult;
      
      }

      
      
      /// <summary>
      /// Asigna la tira de Ids parametros al detalle del pedido con oi_pedido parametro
      /// </summary>
      /// <param name="pParam"></param>
      /// <returns></returns>
      public static NomadXML WEB_CV_SETDETALLE_PYC(NomadXML pParam)
      {
          NomadLog.Debug("-----------------------------------------------");
          NomadLog.Debug("----------WEB_CV_SETDETALLE_PYC--------------------");
          NomadLog.Debug("-----------------------------------------------");

          NomadLog.Debug("WEB_CV_SETDETALLE_PYC.pParam: " + pParam.ToString());

          NomadXML xmlResult = new NomadXML("DATA");
          string strMode;
          string strStep = "";
          NomadTransaction objTran = null;

          try
          {
              strMode = pParam.GetAttr("mode");
              
              xmlResult.SetAttr("mode", strMode);

              strStep = "CREATE-ARRAY-CVIDs";
              string[] cvIDs = pParam.GetAttr("cvIDs").Split(',');
              
              strStep = "GET-PEIDDO";
              NucleusRH.BaytonRH.PedidosyColaboradores.Pedidos.PEDIDO objPedido = BaytonRH.PedidosyColaboradores.Pedidos.PEDIDO.Get(pParam.GetAttr("oi_pedido"));

             //Para cada CV, creo un detalle
              strStep = "CREATE-DETALLE";
              for (int i = 0; i < cvIDs.Length; i++)
              {
                  
                  NucleusRH.BaytonRH.PedidosyColaboradores.Pedidos.DETALLE objDetalle = new BaytonRH.PedidosyColaboradores.Pedidos.DETALLE();
                  objDetalle.oi_cv = cvIDs[i];
                  objDetalle.f_seleccion = DateTime.Today;
                  objDetalle.l_seleccionado = false;
                  objDetalle.c_estado = "S";
                  objPedido.DETALLE.Add(objDetalle);
              }

              strStep = "SAVE-DETALLE";
              objTran = new NomadTransaction();
              objTran.Begin();
              objTran.SaveRefresh(objPedido);
              objTran.Commit();

              //Creo un registro en la tabla cambio de pedidos, para cada detalle nuevo
              strStep = "SET-CAMBIOS";
              foreach (NucleusRH.BaytonRH.PedidosyColaboradores.Pedidos.DETALLE objDetalle in objPedido.DETALLE)
              {
                  for (int i = 0; i < cvIDs.Length; i++)
                  {
                      if (objDetalle.oi_cv == cvIDs[i])
                      {
                          //Era de los nuevos, creo el cambio
                          NucleusRH.BaytonRH.PedidosyColaboradores.CambioEstadoDetalles.CAMBIO_ESTADO objCambio = new BaytonRH.PedidosyColaboradores.CambioEstadoDetalles.CAMBIO_ESTADO();

                          objCambio.oi_detalle = objDetalle.Id;
                          objCambio.c_nuevo = objDetalle.c_estado;
                          objCambio.c_anteriorNull = true; 
                          objCambio.f_cambio= DateTime.Now;

                          strStep = "SAVE-CAMBIO";
                          objTran.Begin();
                          objTran.Save(objCambio);
                          objTran.Commit();
                          
                          break;
                      }
                  }
              }
  
              //RecordCard
              string o_accion = "";
              if (cvIDs.Length == 1)
                  o_accion = "Se agregó 1 persona al detalle del Pedido";
              else
                  o_accion = "Se agregaron "+cvIDs.Length+" personas al detalle del Pedido";
              
              strStep = "RecordCard";

              NmdXmlDocument parametrosQRY = new NmdXmlDocument("<param/>");
              parametrosQRY.AddAttribute("oi_pedido", objPedido.Id);
              parametrosQRY.AddAttribute("o_accion", o_accion);
              NucleusRH.Base.RecordCard.AccionesReales.ACCION_REAL.GuardarAccion("AccionesPedidos", parametrosQRY, "NucleusRH.BaytonRH.RecordCardPedidos.AccionesPedidos.ACCION_REAL");
              
              xmlResult.SetAttr("resultOk", "1");

          }
          catch (Exception ex)
          {
              NomadException nmdEx = new NomadException("NucleusRH.Base.Postulantes.CV.WEB_CV_SETDETALLE_PYC()", ex);
              nmdEx.SetValue("Step", strStep);
              xmlResult.SetAttr("resultOk", "0");
              if (objTran != null)
              {
                  objTran.Rollback();
                  xmlResult.SetAttr("result", "0");
              }
              throw nmdEx;
          }

          NomadLog.Debug("WEB_CV_SETDETALLE_PYC: " + xmlResult.ToString());
          return xmlResult;
      }
      #endregion

      #region WebCV
      /// <summary>
      /// Persiste el CV, reemplazando los campos padres y actualizando los childs
      /// </summary>
      /// <returns></returns>
      public static bool SaveCV(WEB_CV pobjCV)
      {

          NomadLog.Debug("-----------------------------------------------");
          NomadLog.Debug("----------SaveCV V1-----------------------------");
          NomadLog.Debug("-----------------------------------------------");

          NomadLog.Debug("Web_Get_CV.pcCv: " + Nomad.NSystem.Functions.StringUtil.Object2JSON(pobjCV));
          NomadTransaction objTran = null;
          string strStep = "";

          try
          {
              strStep = "GET-CV";
              NomadXML xmlParam = new NomadXML("DATA");
              xmlParam.SetAttr("oi_tipo_documento", pobjCV.oi_tipo_documento);
              xmlParam.SetAttr("c_nro_doc", pobjCV.c_nro_doc);

              NomadXML xmlResultQry = Nomad.NSystem.Base.NomadEnvironment.QueryNomadXML(NucleusRH.Base.Postulantes.CV.CV.Resources.WEB_GET_ID_CV, xmlParam.ToString());

              string oi_cv = xmlResultQry.FirstChild().GetAttr("oi_cv");


              //Si oi_cv = 0, implica que no existe en la BD por lo que se crea un nuevo CV. Sino, lo recupero de la BD                
              Postulantes.CV.CV objNucCV;
              if (oi_cv == "0")
                  objNucCV = new CV();
              else
                  objNucCV = CV.Get(oi_cv);


              strStep = "ACTUALIZO-PADRE";
              //Si es for insert, genero el c_cv como WEB+c_nro_dc y el estado es disponible y la fecha de alta como el dia de hoy y que es un alta web
              if (oi_cv == "0")
              {
                  objNucCV.c_cv = "WEB" + pobjCV.c_nro_doc;
                  objNucCV.c_estado = "D";
                  objNucCV.f_alta_cv = DateTime.Now;
                  objNucCV.l_alta_web = true;
              }

              objNucCV.d_apellido = pobjCV.d_apellido;
              objNucCV.d_nombres = pobjCV.d_nombres;
              objNucCV.d_ape_y_nom = objNucCV.d_apellido + ", " + objNucCV.d_nombres;
              objNucCV.oi_tipo_documento = pobjCV.oi_tipo_documento.ToString();
              objNucCV.c_nro_doc = pobjCV.c_nro_doc;

              if (pobjCV.c_password == null || pobjCV.c_password == "")
                  throw new Exception("No se encuentra la clave del CV");

              objNucCV.c_password = pobjCV.c_password;


              if (pobjCV.c_nro_cuil == "")
                  objNucCV.c_nro_cuilNull = true;
              else
                  objNucCV.c_nro_cuil = pobjCV.c_nro_cuil;

              if (pobjCV.f_nacim == null)
                  objNucCV.f_nacimNull = true;
              else
                  objNucCV.f_nacim = DateTime.Parse(pobjCV.f_nacim.ToString());

              if (pobjCV.c_sexo == "")
                  objNucCV.c_sexoNull = true;
              else
                  objNucCV.c_sexo = pobjCV.c_sexo;

              if (pobjCV.oi_nacionalidad == null)
                  objNucCV.oi_nacionalidadNull = true;
              else
                  objNucCV.oi_nacionalidad = pobjCV.oi_nacionalidad.ToString();

              if (pobjCV.oi_estado_civil == null)
                  objNucCV.oi_estado_civilNull = true;
              else
                  objNucCV.oi_estado_civil = pobjCV.oi_estado_civil.ToString();

              if (pobjCV.d_email == "")
                  objNucCV.d_emailNull = true;
              else
                  objNucCV.d_email = pobjCV.d_email;

              if (pobjCV.c_pais == "")
                  objNucCV.c_paisNull = true;
              else
                  objNucCV.c_pais = pobjCV.c_pais;

              if (pobjCV.c_area == "")
                  objNucCV.c_areaNull = true;
              else
                  objNucCV.c_area = pobjCV.c_area;

              if (pobjCV.te_nro == "")
                  objNucCV.te_nroNull = true;
              else
                  objNucCV.te_nro = pobjCV.te_nro;

              if (pobjCV.c_pais_cel == "")
                  objNucCV.c_pais_celNull = true;
              else
                  objNucCV.c_pais_cel = pobjCV.c_pais_cel;

              if (pobjCV.c_area_cel == "")
                  objNucCV.c_area_celNull = true;
              else
                  objNucCV.c_area_cel = pobjCV.c_area_cel;

              if (pobjCV.te_celular == "")
                  objNucCV.te_celularNull = true;
              else
                  objNucCV.te_celular = pobjCV.te_celular;


              if (pobjCV.d_calle == "")
                  objNucCV.d_calleNull = true;
              else
                  objNucCV.d_calle = pobjCV.d_calle;

              if (pobjCV.c_nro == "")
                  objNucCV.c_nroNull = true;
              else
                  objNucCV.c_nro = pobjCV.c_nro;

              if (pobjCV.c_piso == "")
                  objNucCV.c_pisoNull = true;
              else
                  objNucCV.c_piso = pobjCV.c_piso;

              if (pobjCV.c_departamento == "")
                  objNucCV.c_departamentoNull = true;
              else
                  objNucCV.c_departamento = pobjCV.c_departamento;

              if (pobjCV.d_zona == "")
                  objNucCV.d_zonaNull = true;
              else
                  objNucCV.d_zona = pobjCV.d_zona;

              if (pobjCV.oi_localidad == null)
              {
                  objNucCV.oi_localidadNull = true;
                  objNucCV.d_localidadNull = true;
              }
              else
              {
                  NucleusRH.Base.Organizacion.Localidades.LOCALIDAD tempLocalidad = NucleusRH.Base.Organizacion.Localidades.LOCALIDAD.Get(pobjCV.oi_localidad.ToString());
                  objNucCV.oi_localidad = tempLocalidad.Id;
                  objNucCV.d_localidad = tempLocalidad.d_localidad;
                    
              }
              //if (pobjCV.d_localidad == "")
              //    objNucCV.d_localidadNull = true;
              //else
              //    objNucCV.d_localidad = pobjCV.d_localidad;

              if (pobjCV.c_codigo_postal == "")
                  objNucCV.c_codigo_postalNull = true;
              else
                  objNucCV.c_codigo_postal = pobjCV.c_codigo_postal;

              if (pobjCV.oi_pais == null)
                  objNucCV.oi_paisNull = true;
              else
                  objNucCV.oi_pais = pobjCV.oi_pais.ToString();

              if (pobjCV.oi_provincia == null)
                  objNucCV.oi_provinciaNull = true;
              else
                  objNucCV.oi_provincia = pobjCV.oi_provincia.ToString();

              if (pobjCV.n_remuneracion == null)
                  objNucCV.n_remuneracionNull = true;
              else
                  objNucCV.n_remuneracion = Double.Parse(pobjCV.n_remuneracion.ToString());

              objNucCV.l_informacion = pobjCV.l_informacion;

              //El CV es de origen WEB
              objNucCV.l_alta_web = true;
              objNucCV.c_origen = "WEB";
              objNucCV.f_actualizacion = DateTime.Now;
              strStep = "CHILD-ESTUDIO";
              //CV Nuevo - Agrego todos los estudios
              if (oi_cv == "0")
              {
                  foreach (WEB_CV_Estudios pObjEst in pobjCV.EstudiosCV)
                  {
                      NucleusRH.Base.Postulantes.CV.ESTUDIO_CV objNucEstudios = new ESTUDIO_CV();

                      //Transformacion: Para un nivel de estudio y estado de estudio, calculo el nivel de estudio de AFIP para NS                      
                      objNucEstudios.oi_estado_est = pObjEst.oi_estado_est.ToString();
                      switch (pObjEst.oi_nivel_estudio.ToString())
                      {
                          case "2":
                              {//PRIMARIO                                                                   
                                  //Nivel Primario incompleto
                                  if ((pObjEst.oi_estado_est.ToString() == "1") || (pObjEst.oi_estado_est.ToString() == "2"))
                                      objNucEstudios.oi_nivel_estudio = "3";
                                  else
                                  {
                                      if (pObjEst.oi_estado_est.ToString() == "3")
                                          objNucEstudios.oi_nivel_estudio = "2";
                                  }
                                  break;
                              }
                          case "4":
                              {
                                  //SECUNDARIO                                  
                                  //Nivel Secundario incompleto
                                  if ((pObjEst.oi_estado_est.ToString() == "1") || (pObjEst.oi_estado_est.ToString() == "2"))
                                      objNucEstudios.oi_nivel_estudio = "5";
                                  else
                                  {
                                      if (pObjEst.oi_estado_est.ToString() == "3")
                                          objNucEstudios.oi_nivel_estudio = "4";
                                  }
                                  break;

                              }
                          case "10":
                              {
                                  //TERCIARIO                                  
                                  //Nivel terciario incompleto
                                  if ((pObjEst.oi_estado_est.ToString() == "1") || (pObjEst.oi_estado_est.ToString() == "2"))
                                      objNucEstudios.oi_nivel_estudio = "11";
                                  else
                                  {
                                      if (pObjEst.oi_estado_est.ToString() == "3")
                                          objNucEstudios.oi_nivel_estudio = "10";
                                  }
                                  break;
                              }
                          case "12":
                              {
                                  //UNIVERSITARIO
                                  //Nivel universitario incompleto
                                  if ((pObjEst.oi_estado_est.ToString() == "1") || (pObjEst.oi_estado_est.ToString() == "2"))
                                      objNucEstudios.oi_nivel_estudio = "13";
                                  else
                                  {
                                      if (pObjEst.oi_estado_est.ToString() == "3")
                                          objNucEstudios.oi_nivel_estudio = "12";
                                  }
                                  break;
                              }
                      }

                      if (pObjEst.oi_estudio.ToString() != "")
                          objNucEstudios.oi_estudio = pObjEst.oi_estudio.ToString();
                      else
                          objNucEstudios.oi_estudioNull = true;

                      if (pObjEst.d_otro_est_educ != "")
                          objNucEstudios.d_otro_est_educ = pObjEst.d_otro_est_educ;
                      else
                          objNucEstudios.d_otro_est_educNull = true;

                      objNucCV.ESTUDIOS_CV.Add(objNucEstudios);
                  }
              }
              else
              {
                  //EDITAR estudios del CV                   
                  if(objNucCV.ESTUDIOS_CV.Count==0)
                  {
                    //IS FOR INSERT, no hay ninguno en BD
                      foreach (WEB_CV_Estudios pObjEst in pobjCV.EstudiosCV)
                      {
                          NucleusRH.Base.Postulantes.CV.ESTUDIO_CV objNucEstudios = new ESTUDIO_CV();

                          //Transformacion: Para un nivel de estudio y estado de estudio, calculo el nivel de estudio de AFIP para NS                      
                          objNucEstudios.oi_estado_est = pObjEst.oi_estado_est.ToString();
                          switch (pObjEst.oi_nivel_estudio.ToString())
                          {
                              case "2":
                                  {//PRIMARIO                                                                   
                                      //Nivel Primario incompleto
                                      if ((pObjEst.oi_estado_est.ToString() == "1") || (pObjEst.oi_estado_est.ToString() == "2"))
                                          objNucEstudios.oi_nivel_estudio = "3";
                                      else
                                      {
                                          if (pObjEst.oi_estado_est.ToString() == "3")
                                              objNucEstudios.oi_nivel_estudio = "2";
                                      }
                                      break;
                                  }
                              case "4":
                                  {
                                      //SECUNDARIO                                  
                                      //Nivel Secundario incompleto
                                      if ((pObjEst.oi_estado_est.ToString() == "1") || (pObjEst.oi_estado_est.ToString() == "2"))
                                          objNucEstudios.oi_nivel_estudio = "5";
                                      else
                                      {
                                          if (pObjEst.oi_estado_est.ToString() == "3")
                                              objNucEstudios.oi_nivel_estudio = "4";
                                      }
                                      break;

                                  }
                              case "10":
                                  {
                                      //TERCIARIO                                  
                                      //Nivel terciario incompleto
                                      if ((pObjEst.oi_estado_est.ToString() == "1") || (pObjEst.oi_estado_est.ToString() == "2"))
                                          objNucEstudios.oi_nivel_estudio = "11";
                                      else
                                      {
                                          if (pObjEst.oi_estado_est.ToString() == "3")
                                              objNucEstudios.oi_nivel_estudio = "10";
                                      }
                                      break;
                                  }
                              case "12":
                                  {
                                      //UNIVERSITARIO
                                      //Nivel universitario incompleto
                                      if ((pObjEst.oi_estado_est.ToString() == "1") || (pObjEst.oi_estado_est.ToString() == "2"))
                                          objNucEstudios.oi_nivel_estudio = "13";
                                      else
                                      {
                                          if (pObjEst.oi_estado_est.ToString() == "3")
                                              objNucEstudios.oi_nivel_estudio = "12";
                                      }
                                      break;
                                  }
                          }

                          if (pObjEst.oi_estudio.ToString() != "")
                              objNucEstudios.oi_estudio = pObjEst.oi_estudio.ToString();
                          else
                              objNucEstudios.oi_estudioNull = true;

                          if (pObjEst.d_otro_est_educ != "")
                              objNucEstudios.d_otro_est_educ = pObjEst.d_otro_est_educ;
                          else
                              objNucEstudios.d_otro_est_educNull = true;

                          objNucCV.ESTUDIOS_CV.Add(objNucEstudios);
                      }

                  }
                  else
                  {

                      //PASO 0: Transformo lo que esta en BD
                      foreach (NucleusRH.Base.Postulantes.CV.ESTUDIO_CV objNucEstudioCV in objNucCV.ESTUDIOS_CV)
                      {
                          //Sin estudio, continuo
                          if (objNucEstudioCV.oi_nivel_estudio == "1")
                              continue;

                          //Nivel Primario o Primario incompleto, General Basico o General Basico incompleto
                          if ((objNucEstudioCV.oi_nivel_estudio == "2") || (objNucEstudioCV.oi_nivel_estudio == "3")
                              || (objNucEstudioCV.oi_nivel_estudio == "6") || (objNucEstudioCV.oi_nivel_estudio == "7")
                              )
                              objNucEstudioCV.oi_nivel_estudio = "2";

                          //Nivel Secundario o Secundario incompleto, Polimodal o polimodal incompleto 
                          if ((objNucEstudioCV.oi_nivel_estudio == "4") || (objNucEstudioCV.oi_nivel_estudio == "5")
                              || (objNucEstudioCV.oi_nivel_estudio == "8") || (objNucEstudioCV.oi_nivel_estudio == "9") 
                              )
                              objNucEstudioCV.oi_nivel_estudio = "4";

                          //Si es Terciario o Terciario Incompleto
                          if ((objNucEstudioCV.oi_nivel_estudio == "10") || (objNucEstudioCV.oi_nivel_estudio == "11"))
                              objNucEstudioCV.oi_nivel_estudio = "10";

                          //Si es Universitario o  Universitario Incompleto
                          if ((objNucEstudioCV.oi_nivel_estudio == "12") || (objNucEstudioCV.oi_nivel_estudio == "13"))
                              objNucEstudioCV.oi_nivel_estudio = "12";
                  
                      }

                      //PASO 1 - AGREGO LOS QUE VIENEN QUE NO ESTAN EN LA BD
                      List<WEB_CV_Estudios> auxEstudios = new List<WEB_CV_Estudios>();
                      foreach (WEB_CV_Estudios objParamEstudioCV in pobjCV.EstudiosCV)
                      {
                          bool encontro = false;
                          foreach (NucleusRH.Base.Postulantes.CV.ESTUDIO_CV objNucEstudioCV in objNucCV.ESTUDIOS_CV)
                          {
                              if (objParamEstudioCV.oi_nivel_estudio.ToString() == objNucEstudioCV.oi_nivel_estudio)
                              {
                                  encontro = true;
                                  break;
                              }
                          }
                          //Si no lo encontro, lo agrego                      
                          if (!encontro)
                              auxEstudios.Add(objParamEstudioCV);
                      }               

                      //PASO 2 - Actualizo la BD con lo que VIENE
                      List<string> idsToDel = new List<string>();
                      foreach (NucleusRH.Base.Postulantes.CV.ESTUDIO_CV objNucEstudioCV in objNucCV.ESTUDIOS_CV)
                      {
                          bool encontro = false;

                          foreach (WEB_CV_Estudios objParamEstudioCV in pobjCV.EstudiosCV)
                          {
                              if (objNucEstudioCV.oi_nivel_estudio == objParamEstudioCV.oi_nivel_estudio.ToString())
                              {                                                                                       
                                 //Re-convierto el child por nivel y estado, debe guardarse correctamente por el tema del calculo del nivel de 
                                  //Si es Incompleto, el nivel debe ser incompleto
                                  if ((objParamEstudioCV.oi_estado_est.ToString() == "1") || (objParamEstudioCV.oi_estado_est.ToString() == "2"))
                                  {
                                      switch (objParamEstudioCV.oi_nivel_estudio.ToString())
                                      {
                                          case "2": { objNucEstudioCV.oi_nivel_estudio="3"; break;}
                                          case "4": { objNucEstudioCV.oi_nivel_estudio="5"; break;}
                                          case "10": { objNucEstudioCV.oi_nivel_estudio="11"; break;}
                                          case "12": { objNucEstudioCV.oi_nivel_estudio = "13"; break; }
                                      }
                                  }                                                                                        
                              
                              
                                  objNucEstudioCV.oi_estado_est = objParamEstudioCV.oi_estado_est.ToString();
                                  objNucEstudioCV.oi_estudio = objParamEstudioCV.oi_estudio.ToString();
                                  objNucEstudioCV.d_otro_est_educ = objParamEstudioCV.d_otro_est_educ;

                                  encontro = true;
                                  break;
                              }

                          }
                          if (!encontro)
                          {
                              //Si no lo encontro, lo marco para quitar
                              idsToDel.Add(objNucEstudioCV.Id);
                          }
                      }
                 
                      //PASO 3 - Actualizo el objeto a persistir
                      foreach (string oi in idsToDel)
                      {
                          objNucCV.ESTUDIOS_CV.RemoveById(oi);
                      }
                      foreach (WEB_CV_Estudios objEst in auxEstudios)
                      {
                          NucleusRH.Base.Postulantes.CV.ESTUDIO_CV objNucEst = new ESTUDIO_CV();

                          //Re-convierto el child por nivel y estado, debe guardarse correctamente por el tema del calculo del nivel de 
                          //Si es Incompleto, el nivel debe ser incompleto
                          if ((objEst.oi_estado_est.ToString() == "1") || (objEst.oi_estado_est.ToString() == "2"))
                          {
                              switch (objEst.oi_nivel_estudio.ToString())
                              {
                                  case "2": { objNucEst.oi_nivel_estudio = "3"; break; }
                                  case "4": { objNucEst.oi_nivel_estudio = "5"; break; }
                                  case "10": { objNucEst.oi_nivel_estudio = "11"; break; }
                                  case "12": { objNucEst.oi_nivel_estudio = "13"; break; }
                              }
                          }                                                                              
                          objNucEst.oi_estudio = objEst.oi_estudio.ToString();                       
                          objNucEst.oi_estado_est = objEst.oi_estado_est.ToString();
                          objNucEst.d_otro_est_educ = objEst.d_otro_est_educ;

                          objNucCV.ESTUDIOS_CV.Add(objNucEst);
                      } 
              }              
              }


              strStep = "CHILD-ANTECEDENTES";
              //CV Nuevo - Agrego todos los Antecedentes
              if (oi_cv == "0")
              {
                  foreach (WEB_CV_Exp_Lab pobjExpLab in pobjCV.ExpLaboralCV)
                  {
                      NucleusRH.Base.Postulantes.CV.ANTECEDENTE_CV objNucAntecedente = new NucleusRH.Base.Postulantes.CV.ANTECEDENTE_CV();                                          
                      objNucAntecedente.d_empresa = pobjExpLab.d_empresa;                      
                      objNucAntecedente.d_puesto = pobjExpLab.d_puesto;
                      objNucAntecedente.f_ingreso = pobjExpLab.f_ingreso;

                      if (pobjExpLab.f_egreso != null)
                          objNucAntecedente.f_egreso = DateTime.Parse(pobjExpLab.f_egreso.ToString());
                      else
                          objNucAntecedente.f_egresoNull = true;

                      objNucAntecedente.o_tareas = pobjExpLab.o_tareas;

                      if (pobjExpLab.oi_area_lab != null)
                          objNucAntecedente.oi_area_lab = pobjExpLab.oi_area_lab.ToString();
                      else
                          objNucAntecedente.oi_area_labNull = true;
                      
                      objNucCV.ANTEC_CV.Add(objNucAntecedente);                      
                  }
              }
              else
              {
                  //Edito antecedentes del CV
                  List<string> idsToDel = new List<string>();

                  //PASO 1 - Actualizo la BD con lo que VIENE
                  foreach (NucleusRH.Base.Postulantes.CV.ANTECEDENTE_CV objNucAntecedentesCV in objNucCV.ANTEC_CV)
                  {
                      bool encontro = false;

                      foreach (WEB_CV_Exp_Lab objParamExpLab in pobjCV.ExpLaboralCV)
                      {
                          if (objNucAntecedentesCV.f_ingreso == objParamExpLab.f_ingreso)
                          {
                              //Si lo encontro, actualiza la bd
                              encontro = true;

                              objNucAntecedentesCV.f_ingreso = objParamExpLab.f_ingreso;
                              if (objParamExpLab.f_egreso != null)
                                  objNucAntecedentesCV.f_egreso = DateTime.Parse(objParamExpLab.f_egreso.ToString());
                              else
                                  objNucAntecedentesCV.f_egresoNull = true;

                              objNucAntecedentesCV.d_empresa = objParamExpLab.d_empresa;
                              objNucAntecedentesCV.d_puesto = objParamExpLab.d_puesto;
                              objNucAntecedentesCV.o_tareas = objParamExpLab.o_tareas;

                              if (objParamExpLab.oi_area_lab != null)
                                  objNucAntecedentesCV.oi_area_lab = objParamExpLab.oi_area_lab.ToString();
                              else
                                  objNucAntecedentesCV.oi_area_labNull = true;

                              break;

                          }

                      }
                      if (!encontro)
                      {
                          //Si no lo encontro, lo marco para quitar
                          idsToDel.Add(objNucAntecedentesCV.Id);
                      }
                  }

                  List<WEB_CV_Exp_Lab> auxExpLab = new List<WEB_CV_Exp_Lab>();

                  //PASO 2 - AGREGO LOS QUE VIENEN QUE NO ESTAN EN LA BD
                  foreach (WEB_CV_Exp_Lab objParamExpLab in pobjCV.ExpLaboralCV)
                  {
                      bool encontro = false;
                      foreach (NucleusRH.Base.Postulantes.CV.ANTECEDENTE_CV objNucAntCV in objNucCV.ANTEC_CV)
                      {
                          if (objParamExpLab.f_ingreso == objNucAntCV.f_ingreso)
                          {
                              encontro = true;
                              break;
                          }
                      }
                      //Si no lo encontro, lo agrego
                      if (!encontro)
                      {
                          auxExpLab.Add(objParamExpLab);
                      }
                  }

                  //PASO 3 - Actualizo el objeto a persistir
                  foreach (string oi in idsToDel)
                  {
                      objNucCV.ANTEC_CV.RemoveById(oi);
                  }
                  foreach (WEB_CV_Exp_Lab objExp in auxExpLab)
                  {
                      NucleusRH.Base.Postulantes.CV.ANTECEDENTE_CV objAntc = new ANTECEDENTE_CV();

                      objAntc.d_empresa = objExp.d_empresa;
                      objAntc.d_puesto = objExp.d_puesto;
                      objAntc.f_ingreso = objExp.f_ingreso;

                      if (objExp.f_egreso != null)
                          objAntc.f_egreso = DateTime.Parse(objExp.f_egreso.ToString());
                      else
                          objAntc.f_egresoNull = true;

                      objAntc.o_tareas = objExp.o_tareas;

                      if (objExp.oi_area_lab != null)
                          objAntc.oi_area_lab = objExp.oi_area_lab.ToString();
                      else
                          objAntc.oi_area_labNull = true;

                      objNucCV.ANTEC_CV.Add(objAntc);
                  }

              }
              strStep = "CHILD-IDIOMAS";
              //CV Nuevo - Agrego todos los Idiomas
              if (oi_cv == "0")
              {
                  foreach (WEB_CV_Idiomas pobjIdiomas in pobjCV.IdiomasCV)
                  {
                      NucleusRH.Base.Postulantes.CV.IDIOMA_CV objNucIdiomas = new IDIOMA_CV();

                      objNucIdiomas.oi_idioma = pobjIdiomas.oi_idioma.ToString();
                      objNucIdiomas.oi_nivel_escribe = pobjIdiomas.oi_nivel_escribe.ToString();
                      objNucIdiomas.oi_nivel_habla = pobjIdiomas.oi_nivel_habla.ToString();
                      objNucIdiomas.oi_nivel_lee = pobjIdiomas.oi_nivel_lee.ToString();
                      objNucIdiomas.l_certificado = pobjIdiomas.l_certificado;

                      objNucCV.IDIOMAS_CV.Add(objNucIdiomas);

                  }

              }
              else
              {
                  //Edito Idiomas del CV
                  List<string> idsToDel = new List<string>();

                  //PASO 1 - Actualizo la BD con lo que VIENE
                  foreach (NucleusRH.Base.Postulantes.CV.IDIOMA_CV objNucIdiomasCV in objNucCV.IDIOMAS_CV)
                  {
                      bool encontro = false;

                      foreach (WEB_CV_Idiomas objParamIdioma in pobjCV.IdiomasCV)
                      {
                          if (objNucIdiomasCV.oi_idioma == objParamIdioma.oi_idioma.ToString())
                          {
                              //Si lo encontro, actualiza la bd
                              encontro = true;

                              objNucIdiomasCV.o_idioma = objParamIdioma.oi_idioma.ToString();
                              objNucIdiomasCV.oi_nivel_habla = objParamIdioma.oi_nivel_habla.ToString();
                              objNucIdiomasCV.oi_nivel_lee = objParamIdioma.oi_nivel_lee.ToString();
                              objNucIdiomasCV.oi_nivel_escribe = objParamIdioma.oi_nivel_escribe.ToString();
                              objNucIdiomasCV.l_certificado = objParamIdioma.l_certificado;

                              break;

                          }

                      }
                      if (!encontro)
                      {
                          //Si no lo encontro, lo marco para quitar
                          idsToDel.Add(objNucIdiomasCV.Id);
                      }
                  }

                  List<WEB_CV_Idiomas> auxIdiomas = new List<WEB_CV_Idiomas>();

                  //PASO 2 - AGREGO LOS QUE VIENEN QUE NO ESTAN EN LA BD
                  foreach (WEB_CV_Idiomas objParamIdioma in pobjCV.IdiomasCV)
                  {
                      bool encontro = false;
                      foreach (NucleusRH.Base.Postulantes.CV.IDIOMA_CV objNucIdiomasCV in objNucCV.IDIOMAS_CV)
                      {
                          if (objParamIdioma.oi_idioma.ToString() == objNucIdiomasCV.oi_idioma)
                          {
                              encontro = true;
                              break;
                          }
                      }
                      //Si no lo encontro, lo agrego
                      if (!encontro)
                      {
                          auxIdiomas.Add(objParamIdioma);
                      }
                  }

                  //PASO 3 - Actualizo el objeto a persistir
                  foreach (string oi in idsToDel)
                  {
                      objNucCV.IDIOMAS_CV.RemoveById(oi);
                  }
                  foreach (WEB_CV_Idiomas objIdi in auxIdiomas)
                  {
                      NucleusRH.Base.Postulantes.CV.IDIOMA_CV objIdioma = new IDIOMA_CV();

                      objIdioma.oi_idioma = objIdi.oi_idioma.ToString();
                      objIdioma.oi_nivel_habla = objIdi.oi_nivel_habla.ToString();
                      objIdioma.oi_nivel_lee = objIdi.oi_nivel_lee.ToString();
                      objIdioma.oi_nivel_escribe = objIdi.oi_nivel_escribe.ToString();
                      objIdioma.l_certificado = objIdi.l_certificado;

                      objNucCV.IDIOMAS_CV.Add(objIdioma);
                  }
              }

              strStep = "CHILD-CONOCIMIENTOS";
              //CV Nuevo - Agrego todos los conocimientos
              if (oi_cv == "0")
              {
                  foreach (WEB_CV_Conocimiento pobjConocimientos in pobjCV.ConocimientosCV)
                  {
                      NucleusRH.Base.Postulantes.CV.CONOC_CV objNucConoc = new CONOC_CV();

                      objNucConoc.oi_conocimiento = pobjConocimientos.oi_conocimiento.ToString();
                      objNucConoc.c_nivel = pobjConocimientos.c_nivel;

                      if (pobjConocimientos.d_conoc_cv != "")
                          objNucConoc.d_conoc_cv = pobjConocimientos.d_conoc_cv;
                      else
                          objNucConoc.d_conoc_cvNull = true;

                      objNucCV.CONOC_CV.Add(objNucConoc);

                  }
              }
              else
              {
                  //Edito Idiomas del CV
                  List<string> idsToDel = new List<string>();

                  //PASO 1 - Actualizo la BD con lo que VIENE
                  foreach (NucleusRH.Base.Postulantes.CV.CONOC_CV objNucConocCV in objNucCV.CONOC_CV)
                  {
                      bool encontro = false;

                      foreach (WEB_CV_Conocimiento objParamConoc in pobjCV.ConocimientosCV)
                      {
                          if (objNucConocCV.oi_conocimiento == objParamConoc.oi_conocimiento.ToString())
                          {
                              //Si lo encontro, actualiza la bd
                              encontro = true;

                              objNucConocCV.oi_conocimiento = objParamConoc.oi_conocimiento.ToString();
                              objNucConocCV.c_nivel = objParamConoc.c_nivel;
                              objNucConocCV.d_conoc_cv = objParamConoc.d_conoc_cv;

                              break;

                          }
                      }
                      if (!encontro)
                      {
                          //Si no lo encontro, lo marco para quitar
                          idsToDel.Add(objNucConocCV.Id);
                      }
                  }

                  List<WEB_CV_Conocimiento> auxConoc = new List<WEB_CV_Conocimiento>();

                  //PASO 2 - AGREGO LOS QUE VIENEN QUE NO ESTAN EN LA BD
                  foreach (WEB_CV_Conocimiento objParamConoc in pobjCV.ConocimientosCV)
                  {
                      bool encontro = false;
                      foreach (NucleusRH.Base.Postulantes.CV.CONOC_CV objNucConocCV in objNucCV.CONOC_CV)
                      {
                          if (objParamConoc.oi_conocimiento.ToString() == objNucConocCV.oi_conocimiento)
                          {
                              encontro = true;
                              break;
                          }
                      }
                      //Si no lo encontro, lo agrego
                      if (!encontro)
                      {
                          auxConoc.Add(objParamConoc);
                      }
                  }

                  //PASO 3 - Actualizo el objeto a persistir
                  foreach (string oi in idsToDel)
                  {
                      objNucCV.CONOC_CV.RemoveById(oi);
                  }
                  foreach (WEB_CV_Conocimiento objAuxConoc in auxConoc)
                  {
                      NucleusRH.Base.Postulantes.CV.CONOC_CV objCon = new CONOC_CV();

                      objCon.oi_conocimiento = objAuxConoc.oi_conocimiento.ToString();
                      objCon.c_nivel = objAuxConoc.c_nivel;
                      objCon.d_conoc_cv = objAuxConoc.d_conoc_cv;

                      objNucCV.CONOC_CV.Add(objCon);
                  }
              }
              strStep = "CHILD-DOCUMENTOS-DIGITALES";
              foreach (WEB_CV_DOC_DIG pobjDocDig in pobjCV.DocumentosDigitales)
              {
                  if (pobjDocDig.c_tipo.ToUpper() == "CV")
                  {
                      if (pobjDocDig.oi_doc_dig != null)
                          objNucCV.oi_doc_digital = pobjDocDig.oi_doc_dig;
                      else
                          objNucCV.oi_doc_digitalNull = true;
                  }
              }
              strStep = "PERSISTO-CV";
             
              objTran = new NomadTransaction();
              objTran.Begin();
              objTran.SaveRefresh(objNucCV);
              objTran.Commit();

              //Seteo el campo like y los niveles de estudio
              //Se envia como nivel de estudio A para indicarle que no es una alta rapida, sino que tiene que calcular el maximo nivel de estudio
              NucleusRH.Base.Postulantes.CV.CV.SET_CAMPO_LIKE(objNucCV.Id, "A");

              return true;
          }
          catch (Exception ex)
          {
              NomadException nmdEx = new NomadException("NucleusRH.Base.Postulantes.Web_CV.Web_Get_CV()", ex);
              nmdEx.SetValue("Step", strStep);
              if (objTran != null)
              {
                  objTran.Rollback();
              }

              throw nmdEx;
          }

      }          

      /// <summary>
      /// Recupera los datos del CV y sus postulaciones
      /// </summary>
      /// <param name="pcCv"></param>
      /// <returns></returns>
      public static WEB_CV GetCV(string tipoDoc, string nroDoc)
      {
          NomadLog.Debug("-----------------------------------------------");
          NomadLog.Debug("----------GetCV V1-----------------------------");
          NomadLog.Debug("-----------------------------------------------");

          NomadLog.Debug("Web_Get_CV.ptipoDoc: " + tipoDoc.ToString());
          NomadLog.Debug("Web_Get_CV.pNroDoc: " + nroDoc.ToString());

          string strStep = "";


          strStep = "NEW-CV";
          WEB_CV objCv = new WEB_CV();

          try
          {
              strStep = "PARAM";
              NomadXML xmlParam = new NomadXML("DATA");
              xmlParam.SetAttr("oi_tipo_documento", tipoDoc);
              xmlParam.SetAttr("c_nro_doc", nroDoc);

              strStep = "GET-DATOS-CV";
              NomadXML xmlResultCV = Nomad.NSystem.Base.NomadEnvironment.QueryNomadXML(NucleusRH.Base.Postulantes.CV.CV.Resources.WEB_GET_CV, xmlParam.ToString());

			  if (xmlResultCV.FirstChild().GetAttr("oi_cv") == "")
			  {
				NomadLog.Warning("Web_Get_CV: no se encontro CV para tipo doc. '" + tipoDoc + "' y nro. doc. '" + nroDoc + "'");
				return null;
			  }
			  
              //objCv.id = xmlResultCV.FirstChild().GetAttrInt("oi_cv");
              //objCv.c_cv = xmlResultCV.FirstChild().GetAttr("c_cv");
              objCv.oi_tipo_documento = xmlResultCV.FirstChild().GetAttrInt("oi_tipo_documento");
              objCv.c_nro_doc = xmlResultCV.FirstChild().GetAttr("c_nro_doc");
              objCv.d_apellido = xmlResultCV.FirstChild().GetAttr("d_apellido");
              objCv.d_nombres = xmlResultCV.FirstChild().GetAttr("d_nombres");

              if (xmlResultCV.FirstChild().GetAttr("oi_nacionalidad") != "")
                  objCv.oi_nacionalidad = xmlResultCV.FirstChild().GetAttrInt("oi_nacionalidad");
              else
                  objCv.oi_nacionalidad = null;

              objCv.c_sexo = xmlResultCV.FirstChild().GetAttr("c_sexo");
              if (xmlResultCV.FirstChild().GetAttr("f_nacim") != "")
                  objCv.f_nacim = xmlResultCV.FirstChild().GetAttrDateTime("f_nacim");
              else
                  objCv.f_nacim = null;

              objCv.c_nro_cuil = xmlResultCV.FirstChild().GetAttr("c_nro_cuil");
              objCv.d_email = xmlResultCV.FirstChild().GetAttr("d_email");

              if (xmlResultCV.FirstChild().GetAttr("oi_estado_civil") != "")
                  objCv.oi_estado_civil = xmlResultCV.FirstChild().GetAttrInt("oi_estado_civil");
              else
                  objCv.oi_estado_civil = null;

              objCv.c_pais = xmlResultCV.FirstChild().GetAttr("c_pais");
              objCv.c_area = xmlResultCV.FirstChild().GetAttr("c_area");
              objCv.te_nro = xmlResultCV.FirstChild().GetAttr("te_nro");
              objCv.c_pais_cel = xmlResultCV.FirstChild().GetAttr("c_pais_cel");
              objCv.c_area_cel = xmlResultCV.FirstChild().GetAttr("c_area_cel");
              objCv.te_celular = xmlResultCV.FirstChild().GetAttr("te_celular");
              objCv.d_calle = xmlResultCV.FirstChild().GetAttr("d_calle");
              objCv.c_nro = xmlResultCV.FirstChild().GetAttr("c_nro");
              objCv.c_piso = xmlResultCV.FirstChild().GetAttr("c_piso");
              objCv.c_departamento = xmlResultCV.FirstChild().GetAttr("c_departamento");

              objCv.oi_localidad = xmlResultCV.FirstChild().GetAttrInt("oi_localidad");
              objCv.d_localidad = xmlResultCV.FirstChild().GetAttr("d_localidad");
              objCv.c_codigo_postal = xmlResultCV.FirstChild().GetAttr("c_codigo_postal");
              objCv.d_zona = xmlResultCV.FirstChild().GetAttr("d_zona");

              if (xmlResultCV.FirstChild().GetAttr("oi_pais") != "")
                  objCv.oi_pais = xmlResultCV.FirstChild().GetAttrInt("oi_pais");
              else
                  objCv.oi_pais = null;

              if (xmlResultCV.FirstChild().GetAttr("oi_provincia") != "")
                  objCv.oi_provincia = xmlResultCV.FirstChild().GetAttrInt("oi_provincia");
              else
                  objCv.oi_provincia = null;

              if (xmlResultCV.FirstChild().GetAttr("n_remuneracion") != "")
                  objCv.n_remuneracion = xmlResultCV.FirstChild().GetAttrDouble("n_remuneracion");
              else
                  objCv.n_remuneracion = null;

              objCv.l_informacion = xmlResultCV.FirstChild().GetAttrBool("l_informacion");

              //Password
              if (xmlResultCV.FirstChild().GetAttr("c_password") != "")
              {
                  objCv.c_password = xmlResultCV.FirstChild().GetAttr("c_password");
                  
                  //No esta encriptado
                  if (!((objCv.c_password.Length == 35) && (objCv.c_password.Split('-').Length == 4)))
                  {
                      //Encripto la clave
                      objCv.c_password = StringUtil.GetMD5("|" + objCv.c_nro_doc.ToString() + "|" + objCv.c_password+"|");
                  }
              }
              else
              {
                  objCv.c_password = null;
              }

              strStep = "CHILD-DOCUMENTOS";
              for (NomadXML row = xmlResultCV.FirstChild().FindElement("DOCUMENTOS-DIGITALES").FirstChild(); row != null; row = row.Next())
              {
                  WEB_CV_DOC_DIG objDocDig = new WEB_CV_DOC_DIG();
                  objDocDig.oi_doc_dig = row.GetAttr("oi_doc_digital");
                  objDocDig.c_tipo = row.GetAttr("c_tipo");

                  objCv.DocumentosDigitales.Add(objDocDig);
              }
              strStep = "CHILD-ESTUDIOS";
              for (NomadXML row = xmlResultCV.FirstChild().FindElement("ESTUDIOS").FirstChild(); row != null; row = row.Next())
              {
                  WEB_CV_Estudios objEstudio = new WEB_CV_Estudios();
                  //Se devuelve primario para primario y primario incompleto, general basico y general basico incompleto
                  if ((row.GetAttr("oi_nivel_estudio") == "2") || (row.GetAttr("oi_nivel_estudio") == "3")
                       || (row.GetAttr("oi_nivel_estudio") == "6") || (row.GetAttr("oi_nivel_estudio") == "7") 
                       )
                      objEstudio.oi_nivel_estudio = 2;
                  else
                  {
                      //Se devuelve secundario para secundario, secundario incompleto, 
                      //polimodal y polimodal incompleto
                      if ((row.GetAttr("oi_nivel_estudio") == "4") || (row.GetAttr("oi_nivel_estudio") == "5")
                            
                            || (row.GetAttr("oi_nivel_estudio") == "8") || (row.GetAttr("oi_nivel_estudio") == "9")
                          )
                          objEstudio.oi_nivel_estudio = 4;
                      else
                      { 
                        //Se devuelve terciario
                          if ((row.GetAttr("oi_nivel_estudio") == "10") || (row.GetAttr("oi_nivel_estudio") == "11"))
                              objEstudio.oi_nivel_estudio = 10;
                          else
                          { 
                            //Se devuelve universitario
                              if ((row.GetAttr("oi_nivel_estudio") == "12") || (row.GetAttr("oi_nivel_estudio") == "13"))
                                  objEstudio.oi_nivel_estudio = 12;
                              else
                              { 
                                //Estudio mal cargado, lo descarto
                                  continue;
                              }
                          }
                      
                      }
                  }

                  //Si tiene estado cambio, lo descarto
                  if (row.GetAttr("oi_estado_est") != "4")
                      objEstudio.oi_estado_est = row.GetAttrInt("oi_estado_est");
                  else
                      continue;

                  objEstudio.oi_estudio = row.GetAttrInt("oi_estudio");                                                      
                  objEstudio.d_otro_est_educ = row.GetAttr("d_otro_est_educ");

                  objCv.EstudiosCV.Add(objEstudio);
              }
              strStep = "CHILD-ANTECEDENTES";
              for (NomadXML row = xmlResultCV.FirstChild().FindElement("ANTECEDENTES").FirstChild(); row != null; row = row.Next())
              {
                  WEB_CV_Exp_Lab objExp = new WEB_CV_Exp_Lab();
                  objExp.d_empresa = row.GetAttr("d_empresa");
                  objExp.d_puesto = row.GetAttr("d_puesto");
                  objExp.f_ingreso = row.GetAttrDateTime("f_ingreso");

                  if (row.GetAttr("f_egreso") != "")
                      objExp.f_egreso = row.GetAttrDateTime("f_egreso");
                  else
                      objExp.f_egreso = null;

                  objExp.o_tareas = row.GetAttr("o_tareas");

                  if (row.GetAttr("oi_area_lab") != "")
                      objExp.oi_area_lab = row.GetAttrInt("oi_area_lab");
                  else
                      objExp.oi_area_lab = null;

                  objCv.ExpLaboralCV.Add(objExp);
              }

              strStep = "CHILD-IDIOMAS";
              for (NomadXML row = xmlResultCV.FirstChild().FindElement("IDIOMAS").FirstChild(); row != null; row = row.Next())
              {
                  WEB_CV_Idiomas objIdioma = new WEB_CV_Idiomas();
                  objIdioma.oi_idioma = row.GetAttrInt("oi_idioma");
                  objIdioma.oi_nivel_habla = row.GetAttrInt("oi_nivel_habla");
                  objIdioma.oi_nivel_lee = row.GetAttrInt("oi_nivel_lee");
                  objIdioma.oi_nivel_escribe = row.GetAttrInt("oi_nivel_escribe");
                  objIdioma.l_certificado = row.GetAttrBool("l_certificado");

                  objCv.IdiomasCV.Add(objIdioma);
              }

              strStep = "CHILD-CONOCIMIENTOS";
              for (NomadXML row = xmlResultCV.FirstChild().FindElement("CONOCIMIENTOS").FirstChild(); row != null; row = row.Next())
              {
                  WEB_CV_Conocimiento objConoc = new WEB_CV_Conocimiento();
                  objConoc.oi_conocimiento = row.GetAttrInt("oi_conocimiento");
                  objConoc.c_nivel = row.GetAttr("c_nivel");
                  objConoc.d_conoc_cv = row.GetAttr("d_conoc_cv");

                  objCv.ConocimientosCV.Add(objConoc);
              }

              strStep = "CHILD-POSTULACIONES";
              for (NomadXML row = xmlResultCV.FirstChild().FindElement("POSTULACIONES").FirstChild(); row != null; row = row.Next())
              {
                  WEB_CV_Postulaciones objPostu = new WEB_CV_Postulaciones();
                  objPostu.oi_oferta_lab = row.GetAttrInt("oi_oferta_lab");
                  objPostu.f_postulacion = row.GetAttrDateTime("f_postulacion");
                  objCv.PostulacionesCV.Add(objPostu);
              }


          }
          catch (Exception ex)
          {
              NomadException nmdEx = new NomadException("NucleusRH.Base.Postulantes.Web_CV.Web_Get_CV()", ex);
              nmdEx.SetValue("Step", strStep);
              throw nmdEx;
          }



          return objCv;
      }

      /// <summary>
      /// Postula un CV a una oferta laboral.
      /// Calcula si cumple o no los requisitos y completa lo que no cumple
      /// l_cumple_req = 1 si cumple, 0 si no cumple
      /// d_cumple_req = "Requisitos que no cumple"
      /// </summary>
      /// <param name="tipoDoc"></param>
      /// <param name="nroDoc"></param>
      /// <param name="objPostulacion"></param>
      public static bool Postulate(string tipoDoc, string nroDoc, WEB_Postulaciones pobjPostulacion)
      {
          NomadLog.Debug("-----------------------------------------------");
          NomadLog.Debug("----------WEB_Postulate V1------------------------");
          NomadLog.Debug("-----------------------------------------------");

          NomadLog.Debug("WEB_Postulate.ptipoDoc: " + tipoDoc);
          NomadLog.Debug("WEB_Postulate.pNroDoc: " + nroDoc);
          NomadLog.Debug("WEB_Postulate.objPostulacion: " + Nomad.NSystem.Functions.StringUtil.Object2JSON(pobjPostulacion));
          
           
          NomadTransaction objTran = null;
          NomadTransaction objtr2 = null;
          string strStep = "";
          try
          {
              strStep = "GET-CV";
              NomadXML xmlParam = new NomadXML("DATA");
              xmlParam.SetAttr("oi_tipo_documento", tipoDoc);
              xmlParam.SetAttr("c_nro_doc", nroDoc);

              NomadXML xmlResultQry = Nomad.NSystem.Base.NomadEnvironment.QueryNomadXML(NucleusRH.Base.Postulantes.CV.CV.Resources.WEB_GET_ID_CV, xmlParam.ToString());

              string oi_cv = xmlResultQry.FirstChild().GetAttr("oi_cv");

              strStep = "VALIDATE-POSTU";
              NomadXML xmlParamValidate = new NomadXML("DATA");
              xmlParamValidate.SetAttr("oi_cv", oi_cv);
              xmlParamValidate.SetAttr("oi_oferta_lab", pobjPostulacion.oi_oferta_lab.ToString());
              NomadXML xmlResultValdiate = Nomad.NSystem.Base.NomadEnvironment.QueryNomadXML(NucleusRH.Base.Postulantes.CV.CV.Resources.IS_POSTULATE, xmlParamValidate.ToString());

              if (xmlResultValdiate.FirstChild().GetAttr("postulate") == "1")                  
                throw new Exception("Postulacion duplicada");

              
              strStep = "GET-NUC-CV";
              NucleusRH.Base.Postulantes.CV.CV objNucCV = CV.Get(oi_cv);

              strStep = "NEW-NUC-POSTULACION";
              NucleusRH.Base.Postulantes.CV.POSTULACIONES objNucPostulacion = new POSTULACIONES();


              strStep = "COMPLETO-POSTU";

              if (pobjPostulacion.n_remuneracion != null)
                  objNucPostulacion.n_remuneracion = double.Parse(pobjPostulacion.n_remuneracion.ToString());
              else
                  objNucPostulacion.n_remuneracionNull = true;

              if (pobjPostulacion.oi_origen_aviso != null)
                  objNucPostulacion.oi_origen_aviso = pobjPostulacion.oi_origen_aviso.ToString();
              else
                  objNucPostulacion.oi_origen_avisoNull = true;

              objNucPostulacion.oi_oferta_lab = pobjPostulacion.oi_oferta_lab.ToString();

              objNucPostulacion.f_postulacion = DateTime.Now;
              objNucPostulacion.l_leido = false;

              //CALCULAR CUMPLIMIENTO DE REQUISITOS
              //Pasar pnRemuneracion con su valor o sino 0
              strStep = "SETEO-REQUISITOS";
              
              double pRemuneracion = 0;
              if (pobjPostulacion.n_remuneracion != null)
                  pRemuneracion = double.Parse(pobjPostulacion.n_remuneracion.ToString());

              string req = InternalCumpleRequisitos(objNucCV.Id, objNucPostulacion.oi_oferta_lab, pRemuneracion);

              if (req == "")
              {
                  objNucPostulacion.l_cumple_req = true;
                  objNucPostulacion.d_cumple_reqNull = true;
              }
              else
              {
                  //Ajusto cadena  
                  if (req.Length >= 4000)
                  {
                      //Acoto la cadena a 4000
                      req = req.Substring(0, 4000);                                            

                      //Remuevo lo inconsistente que queda despues de la ultima coma
                      int pos = req.LastIndexOf(',');
                      req = req.Substring(0, pos);
                  }                   
                
                objNucPostulacion.l_cumple_req = false;
                objNucPostulacion.d_cumple_req = req;
                   
              }
                  
              //Las respuestas son una coleccion de la postulacion, agrego las mismas antes de persistir
              strStep="RECORRO-RESPUESTAS";
              foreach (WEB_Respuestas objResp in pobjPostulacion.Respuestas)
              {
                  NucleusRH.Base.Postulantes.CV.POSTU_RESPUESTA objNucRespuesta = new POSTU_RESPUESTA();

                  objNucRespuesta.oi_pregunta = objResp.ID_pregunta;

                  if ((objResp.d_respuesta != "") && (objResp.d_respuesta != null))
                  {
                      objNucRespuesta.d_respuesta = objResp.d_respuesta;
                      objNucRespuesta.oi_pregunta_opNull = true;
                      objNucRespuesta.c_pregunta = "A";
                  }
                  else
                  {
                      objNucRespuesta.d_respuestaNull = true;
                      objNucRespuesta.oi_pregunta_op = objResp.ID_Opcion_Sel;
                      objNucRespuesta.c_pregunta = "M";
                  }
                  objNucPostulacion.POSTU_RESP.Add(objNucRespuesta);
              }
              
              strStep = "ADD-POSTU";
              objNucCV.POSTU_CV.Add(objNucPostulacion);

              strStep = "SAVE-CV";
              //GUARDAR EL CV
              objTran = new NomadTransaction();
              objTran.Begin();
              objTran.SaveRefresh(objNucCV);
              objTran.Commit();
              
             // strStep = "SAVE-RESPUESTA";
             // int oiPostu = 0;
             ////Calculo oi_postulacion
             //foreach (NucleusRH.Base.Postulantes.CV.POSTULACIONES objP in objNucCV.POSTU_CV)
             //{
             //   if ((objP.oi_oferta_lab == objNucPostulacion.oi_oferta_lab) && (objP.f_postulacion == objNucPostulacion.f_postulacion))
             //   {
             //       oiPostu = objP.id;
             //       break;
             //   }
             //}

             //strStep = "SAVE-RESP";
             //objtr2 = new NomadTransaction();
             //objtr2.Begin();
              //foreach (WEB_Respuestas objResp in pobjPostulacion.Respuestas)
              //{
              //    NucleusRH.Base.Postulantes.CV.POSTU_RESPUESTA objNucRes = new POSTU_RESPUESTA();
                  
              //    objNucRes.oi_pregunta = objResp.ID_pregunta;
              //    objNucRes.oi_postulaciones = oiPostu; 
              //    if (objResp.d_respuesta != "")
              //    {
              //        objNucRes.d_respuesta = objResp.d_respuesta;
              //        objNucRes.oi_pregunta_opNull = true;
              //        objNucRes.c_pregunta = "A";
              //    }
              //    else
              //    {
              //        objNucRes.d_respuestaNull = true;
              //        objNucRes.oi_pregunta_op = objResp.ID_Opcion_Sel;
              //        objNucRes.c_pregunta = "M";
              //    }

                 
              //    objtr2.Save(objNucRes);
                                           
             // }
             // objtr2.Commit();   

              //ENVIAR MAIL AL CONTACTO Y A LOS MAIL DE LA POSTULACION(MAIL - MAILALT1 - MAILALT2)REVISAR

              return true;
          }

          catch (Exception ex)
          {
              NomadException nmdEx = new NomadException("NucleusRH.Base.Postulantes.Web_CV.Web_Get_CV()", ex);
              nmdEx.SetValue("Step", strStep);
              if (objTran != null)
              {
                  objTran.Rollback();
              }
              if (objtr2 != null)
              {
                  objtr2.Rollback();
              }

              throw nmdEx;
          }
      }

      
      /// <summary>
      ///Permite subir una archivo digital desde la WEB
      ///Retorna el ID del archivo
      /// </summary>
      /// <returns></returns>
      public static string LoadFile(string tipoDoc, string nroDoc,string tipoDD, WEB_File objFile)
      {
            NomadLog.Debug("-----------------------------------------------");
            NomadLog.Debug("----------LoadFile V1-----------------------------");
            NomadLog.Debug("-----------------------------------------------");

            NomadLog.Debug("WebLoadFile.tipoDoc: " + tipoDoc.ToString());
            NomadLog.Debug("WebLoadFile.nroDoc: " + nroDoc.ToString());
            NomadLog.Debug("WebLoadFile.tipoDD: " + tipoDD.ToString());
            NomadLog.Debug("WebLoadFile.objFile.ID: " + objFile.ID);
            NomadLog.Debug("WebLoadFile.objFile.mimeType: " + objFile.mimeType);
            NomadLog.Debug("WebLoadFile.objFile.Base64: " + objFile.Base64);
            NomadTransaction objTran = null;
            string strStep = "";          
            try
            {
              //strStep = "GET-CV";
              //NomadXML xmlParam = new NomadXML("DATA");
              //xmlParam.SetAttr("oi_tipo_documento", tipoDoc);
              //xmlParam.SetAttr("c_nro_doc", nroDoc);
              
              //NomadXML xmlQry = Nomad.NSystem.Base.NomadEnvironment.QueryNomadXML(NucleusRH.Base.Postulantes.CV.CV.Resources.WEB_GET_ID_CV, xmlParam.ToString());

              //strStep = "GET-CV";
              //NucleusRH.Base.Postulantes.CV.CV objCV = CV.Get(xmlQry.FirstChild().GetAttr("oi_cv"));   
              
              //Comienzo a crear el archivo
              strStep = "CALCULO-EXTENSION";
              string extension = "";
              switch (objFile.mimeType)
              {
                  case "application/pdf": { extension = ".pdf"; break; }
                  case "application/msword": { extension = ".doc"; break; }
                  case "application/vnd.openxmlformats-officedocument.wordprocessingml.document": { extension = ".docx"; break; }
                  case "application/rtf": { extension = ".rtf"; break; }                                  
                  default: { extension = "N"; break; }                    
              }
                
                //Si no tiene extension reconocida, no hace nada
                if (extension == "N")
                  return null;

                strStep = "ARMO-ARCHIVO";
                NucleusRH.Base.Postulantes.DocumentosDigitales.HEAD objDocDig = new DocumentosDigitales.HEAD();
                //Nombre de archivo es tipo de archivo(CV) y extension
                objDocDig.FILE = tipoDD.ToUpper() + extension;

                strStep = "AGREGO-BINS";               
                int index = 0;                
                //int inicio = 0;
                       
                for (int i = 0; i < objFile.Base64.Length; i = i + 4000)
                {
                    NucleusRH.Base.Postulantes.DocumentosDigitales.BIN objBin = new DocumentosDigitales.BIN();

                    //hay mas de 4000
                    if ((objFile.Base64.Length - i ) > 4000)
                        objBin.DATA = objFile.Base64.Substring(i, 4000);
                    
                    //Hay menos de 4000
                    else
                        objBin.DATA = objFile.Base64.Substring(i);
                    
                    objBin.POS = index;
                    index++;
                   // inicio = inicio + 4001;
                     
                    
                    objDocDig.BINS.Add(objBin);
                }
                strStep = "SAVE-FILE";
                //Fecha de creacion hoy
                objDocDig.CREATE = DateTime.Now;

                //Tamaño
                objDocDig.SIZE = (objFile.Base64.Length / 4) * 3;
                objTran = new NomadTransaction();
                objTran.Begin();
                objTran.SaveRefresh(objDocDig);
                objTran.Commit();

                //strStep = "UPDATE-CV";
                //objCV.oi_doc_digital = objDocDig.Id;

                //objTran.Begin();
                //objTran.SaveRefresh(objCV);
                //objTran.Commit();
               
                //Retorna ID de archivo
                return objDocDig.Id;
            }
            catch (Exception ex)
            {
                NomadException nmdEx = new NomadException("NucleusRH.Base.Postulantes.Web_CV.GetFile()", ex);
                nmdEx.SetValue("Step", strStep);
                if (objTran != null)
                {
                    objTran.Rollback();
                }
                throw nmdEx;
            }                             

        
        
      }

      /// <summary>
      /// Retorna el archivo del CV
      /// </summary>
      /// <param name="tipoDoc"></param>
      /// <param name="nroDoc"></param>
      /// <param name="fielId"></param>
      /// <returns></returns>
      public static WEB_File GetFile(string tipoDoc, string nroDoc, string tipoDD)
      {

           NomadLog.Debug("-----------------------------------------------");
           NomadLog.Debug("----------GetFile V1-----------------------------");
           NomadLog.Debug("-----------------------------------------------");

           NomadLog.Debug("WebGetFile.tipoDoc: " + tipoDoc.ToString());
           NomadLog.Debug("Web_GetFile.nroDoc: " + nroDoc.ToString());
           NomadLog.Debug("WebGetFile.tipoDD: " + tipoDD.ToString());

          string strStep = "";          
          try
          {
              //Si tipoDD es CV, sino retornan null
              if (tipoDD.ToUpper() != "CV")
                  return null;

              strStep = "GET-CV";
              NomadXML xmlParam = new NomadXML("DATA");
              xmlParam.SetAttr("oi_tipo_documento", tipoDoc);
              xmlParam.SetAttr("c_nro_doc", nroDoc);
              
              NomadXML xmlQry = Nomad.NSystem.Base.NomadEnvironment.QueryNomadXML(NucleusRH.Base.Postulantes.CV.CV.Resources.WEB_GET_ID_CV, xmlParam.ToString());

              strStep = "GET-CV";
              NucleusRH.Base.Postulantes.CV.CV objCV = CV.Get(xmlQry.FirstChild().GetAttr("oi_cv"));

              //Si no tiene archivo, retorno null
              if (objCV.oi_doc_digitalNull)
                  return null;

              strStep = "NEW-WEB-FILE";
              WEB_File objFile = new WEB_File();
              
              //ID es el oi_doc_digital del CV
              objFile.ID = objCV.oi_doc_digital;
              
              strStep = "GET-FILE";              
              NucleusRH.Base.Postulantes.DocumentosDigitales.HEAD objDocDic = NucleusRH.Base.Postulantes.DocumentosDigitales.HEAD.Get(objCV.oi_doc_digital);

              strStep = "CREATE_BASE64";
              string[] arr = new string[objDocDic.BINS.Count];
              foreach (NucleusRH.Base.Postulantes.DocumentosDigitales.BIN objBin in objDocDic.BINS)              
                  arr[objBin.POS] = objBin.DATA;
                   
               

              //Base 64 es el string concatenado de todos los BINs del HEAD
              objFile.Base64 = string.Join("",arr);

              strStep="CREATE-MIMETYPE";
              //El mime type lo seteo en funcion de la extension
              int pointPosittion = 0;
              int index = 0;
              foreach (char car in objDocDic.FILE)
              {
                  if (car == '.')
                      pointPosittion = index;

                  index++;
              }
              string extension = objDocDic.FILE.Substring(pointPosittion);
              
              switch(extension.ToLower())
              {
                case ".pdf":  {objFile.mimeType = "application/pdf"; break;}
                case ".doc":  {objFile.mimeType = "application/msword";break;}
                case ".docx": {objFile.mimeType ="application/vnd.openxmlformats-officedocument.wordprocessingml.document"; break;} 
                case ".rtf":  {objFile.mimeType = "application/rtf"; break;}                 
                default:{objFile.mimeType="No definido";break;}                               
              }
                                         
              //Retorna el file
              NomadLog.Debug("GetFile.objFile.ID: " + objFile.ID.ToString());
              NomadLog.Debug("GetFile.objFile.mimeType: " + objFile.mimeType.ToString());
              NomadLog.Debug("GetFile.objFile.Base64: " + objFile.Base64.ToString());
              NomadLog.Debug("GetFile.objFile.JSON: " + Nomad.NSystem.Functions.StringUtil.Object2JSON(objFile));


              
              


              return objFile;
          
          }
          catch (Exception ex)
          {
              NomadException nmdEx = new NomadException("NucleusRH.Base.Postulantes.Web_CV.GetFile()", ex);
              nmdEx.SetValue("Step", strStep);
              throw nmdEx;
          }


        
      }


     
      /// <summary>
      /// Para un oi_cv y un oi_oferta_lab, define si cumple o no los requisitos el cv postulado
      /// </summary>
      public static string InternalCumpleRequisitos(string pstrOiCV, string pstrOiOfertaLab, double pnRemuneracion)
      {
          NomadLog.Debug("-----------------------------------------------");
          NomadLog.Debug("----------WEB_Cumple_Requisitos V1--------------");
          NomadLog.Debug("-----------------------------------------------");

          NomadLog.Debug("WEB_Cumple_Requisitos.pOiCV: " + pstrOiCV);
          NomadLog.Debug("WEB_Cumple_Requisitos.pOiOfertaLab: " + pstrOiOfertaLab);


          string strStep = "";
          try
          {
              strStep = "GET-CV";
              NucleusRH.Base.Postulantes.CV.CV objCV = CV.Get(pstrOiCV);

              strStep = "GET-OL";
              NucleusRH.Base.Postulantes.Ofertas_Laborales.OFERTA_LAB objOfertaLab = Ofertas_Laborales.OFERTA_LAB.Get(pstrOiOfertaLab);

              string strResult = "";
              bool l_cumple_req = true;

              strStep = "REQUISITO-RESIDENCIA";
              if (objOfertaLab.l_exc_res)
              {
                  if (!(objCV.oi_pais == objOfertaLab.oi_pais_res))
                  {
                      strResult = strResult + ",No cumple con el país de residencia";
                      l_cumple_req = false;
                  }

                  if (!(objCV.oi_provincia == objOfertaLab.oi_provincia_res))
                  {
                      strResult = strResult + ",No cumple con la provincia de residencia";
                      l_cumple_req = false;
                  }

                  string localidad="";
                  if((objOfertaLab.oi_localidad_res!="") && (objOfertaLab.oi_localidad_res!=null)) 
                    localidad = NucleusRH.Base.Organizacion.Localidades.LOCALIDAD.Get(objOfertaLab.oi_localidad_res).d_localidad;

                  if (localidad != "")
                  {
                      if (!(objCV.d_localidad.Contains(localidad)))
                      {
                          strResult = strResult + ",No cumple con la localidad de residencia";
                          l_cumple_req = false;
                      }
                  }
                  else
                  {
                      strResult = strResult + ",No cumple con la localidad de residencia";
                      l_cumple_req = false;
                  }

              }
              strStep = "REQUISITO-EXPERIENCIA";
              if (objOfertaLab.l_exc_exp)
              {
                  strStep = "TOTALIZO-EN-MESES";
                  int TotalExpMes = 0;
                  foreach (NucleusRH.Base.Postulantes.CV.ANTECEDENTE_CV objAntecCV in objCV.ANTEC_CV)
                  {
                      //Totalizo por area laboral coincidente
                      if (objAntecCV.oi_area_lab == objOfertaLab.oi_area_lab)
                      {
                          DateTime fechaSuperior = new DateTime();
                          if (objAntecCV.f_egresoNull)
                              fechaSuperior = DateTime.Now;
                          else
                              fechaSuperior = objAntecCV.f_egreso;
                        
                        //Calcula la experiencia en meses para el area
                          TotalExpMes = TotalExpMes + Math.Abs((objAntecCV.f_ingreso.Month - fechaSuperior.Month) + 12 * (objAntecCV.f_ingreso.Year - fechaSuperior.Year));
                      }                  
                  }

                  //No tiene con que comparar, no cumple con requisitos.
                  if (TotalExpMes==0)
                  {
                      strResult = strResult + ",No cumple con la experiencia";
                      l_cumple_req = false;
                  }
                  else
                  {
                      //Comparo en meses
                      int mesesDesde = 0;
                      int mesesHasta = 0;

                      if (!(objOfertaLab.oi_uni_tpo_desdeNull))
                      {
                          if (objOfertaLab.oi_uni_tpo_desde == "1")
                              mesesDesde = objOfertaLab.e_exp_des * 12;
                          else
                              mesesDesde = objOfertaLab.e_exp_des;
                      }
                      if (!(objOfertaLab.oi_uni_tpo_hastaNull))
                      {
                          if (objOfertaLab.oi_uni_tpo_hasta == "1")
                              mesesHasta = objOfertaLab.e_exp_hasta * 12;
                          else
                              mesesHasta = objOfertaLab.e_exp_hasta;
                      }
                      //Calculo requisito

                      //Desde y hasta completos desde la oferta
                      if ((mesesDesde != 0) && (mesesHasta != 0))
                      {
                          if (!((TotalExpMes >= mesesDesde) && (TotalExpMes <= mesesHasta)))
                          {
                              strResult = strResult + ",No cumple con la experiencia";
                              l_cumple_req = false;
                          }

                      }

                      //desdecompleto, hasta incompleto
                      if ((mesesDesde != 0) && (mesesHasta == 0))
                      {
                          if (!(TotalExpMes >= mesesDesde))
                          {
                              strResult = strResult + ",No cumple con la experiencia, es menor a la mínima ";
                              l_cumple_req = false;
                          }

                      }

                      //desde incompleto, hasta completo
                      if ((mesesDesde == 0) && (mesesHasta != 0))
                      {
                          if (!(TotalExpMes <= mesesHasta))
                          {
                              strResult = strResult + ",No cumple con la experiencia, es mayor a la máxima";
                              l_cumple_req = false;
                          }

                      }
                  }                  
        
              }

              strStep = "REQUISITO-EDAD";
              if (objOfertaLab.l_exc_edad)
              {
                  //Calculo Edad y valido el rango
                  if (!objCV.f_nacimNull)
                  {
                      int edad = (DateTime.Now.Subtract(objCV.f_nacim).Days) / 365;

                      //Caso 1: Ambas edades completas
                      if ((!objOfertaLab.e_edad_desdeNull) && (!objOfertaLab.e_edad_hastaNull))
                      {
                          if (!((edad >= objOfertaLab.e_edad_desde) && (edad <= objOfertaLab.e_edad_hasta)))
                          {
                              strResult = strResult + ",No cumple con la edad";
                              l_cumple_req = false;
                          }
                      }
                      else
                      {
                          //Caso 2: Edad desde completo(not null) y Edad Hasta null
                          if ((!objOfertaLab.e_edad_desdeNull) && (objOfertaLab.e_edad_hastaNull))
                          {
                              if (!(edad > objOfertaLab.e_edad_desde))
                              {
                                  strResult = strResult + ",No cumple con la edad, es menor a la edad mínima";
                                  l_cumple_req = false;
                              }

                          }
                          else
                          {
                              //Caso 3: Edad desde null y edad hasta completa
                              if ((objOfertaLab.e_edad_desdeNull) && (!objOfertaLab.e_edad_hastaNull))
                              {
                                  if (!(edad < objOfertaLab.e_edad_hasta))
                                  {
                                      strResult = strResult + ",No cumple con la edad, es mayor a la edad máxima";
                                      l_cumple_req = false;
                                  }
                              }
                          }

                      }

                  }
                  else
                  {
                      //Si es NULL no puede comparar, por lo que no cumple con la edad
                      strResult = strResult + ",No cumple con la edad";
                      l_cumple_req = false;
                  }

              }

              strStep = "REQUISITO-SEXO";
              if (objOfertaLab.l_exc_sexo)
              {                   
                  //Comparo sexo del cv con sexo de la OL
                  if ((objCV.c_sexo != objOfertaLab.c_sexo) || (objCV.c_sexoNull))
                  {
                      strResult = strResult + ",No cumple con el sexo";
                      l_cumple_req = false;
                  }
              }

              strStep = "REQUISITO-ESTADO-CIVIL";
              if (objOfertaLab.l_exc_ecivil)
              {
                  //Comparo estado civil del cv con la OL 
                  if ((objCV.oi_estado_civil != objOfertaLab.oi_estado_civil) || (objCV.oi_estado_civilNull) )
                  {
                      strResult = strResult + ",No cumple con el estado civil";
                      l_cumple_req = false;
                  }
              }

              strStep = "REQUISITO-EDUCACION";
              if (objOfertaLab.l_exc_edu)
              {
                  //Caso 1: nivel de estudio no null y estado estudio null
                  if ((!objOfertaLab.oi_nivel_estudioNull) && (objOfertaLab.oi_estado_estNull))
                  {
                      if (objCV.oi_nivel_estudio != objOfertaLab.oi_nivel_estudio)
                      {
                          strResult = strResult + ",No cumple con el nivel de estudio";
                          l_cumple_req = false;
                      }
                  }
                  else
                  {
                      //Caso 2: ambos completos                                            
                      bool encontro = false;                                            
                      foreach (NucleusRH.Base.Postulantes.CV.ESTUDIO_CV objEstudioCV in objCV.ESTUDIOS_CV)
                      {                             
                        if ((!objOfertaLab.oi_nivel_estudioNull) && (!objOfertaLab.oi_estado_estNull))
                        {                            
                            if ((objOfertaLab.oi_nivel_estudio == objEstudioCV.oi_nivel_estudio) && (objOfertaLab.oi_estado_est == objEstudioCV.oi_estado_est))
                            {
                                encontro = true;
                                break;
                            }
                        }                          
                      }                                                                
                        if(!encontro)
                        {
                            strResult = strResult + ",No cumple con la educación";
                            l_cumple_req = false;
                        }
                      
                  }
              }
              strStep = "REQUISITO-IDIOMAS";
              if (objOfertaLab.l_exc_idioma)
              {
                  //Compara con todos los idiomas que tenga al menos el idioma requerido, para cada idioma excluyente
                  //Ingles
                  if (!objOfertaLab.oi_nivel_ingNull)
                  {
                      bool encontro = false;
                      foreach (NucleusRH.Base.Postulantes.CV.IDIOMA_CV objIdioma in objCV.IDIOMAS_CV)
                      {
                          //Idioma 1 es ingles
                          if (objIdioma.oi_idioma == "1")
                          {
                              encontro = true;

                              //Si el nivel es nativo, comparamos por igual
                              if (objOfertaLab.oi_nivel_ing == "5")
                              {
                                  if (!((objIdioma.oi_nivel_habla == objOfertaLab.oi_nivel_ing) || (objIdioma.oi_nivel_lee == objOfertaLab.oi_nivel_ing) || (objIdioma.oi_nivel_escribe == objOfertaLab.oi_nivel_ing)))
                                  {
                                      strResult = strResult + ",No cumple con el nivel de inglés";
                                      l_cumple_req = false;
                                  }
                              }
                              else
                              { 
                                  //Sino, comparamos por mayor igual
                                    if (!((int.Parse(objIdioma.oi_nivel_habla) >= int.Parse(objOfertaLab.oi_nivel_ing)) || (int.Parse(objIdioma.oi_nivel_lee) >= int.Parse(objOfertaLab.oi_nivel_ing)) || (int.Parse(objIdioma.oi_nivel_escribe) >= int.Parse(objOfertaLab.oi_nivel_ing))))
                                    {
                                        strResult = strResult + ",No cumple con el nivel de inglés";
                                        l_cumple_req = false;
                                    }
                              }
                          }

                          //Si lo encontro, salgo
                          if (encontro)
                              break;
                      }

                      //Si no lo encontro, no cumple con el nivel de ingles
                      if (!encontro)
                      {
                          strResult = strResult + ",No cumple con el nivel de inglés";
                          l_cumple_req = false;
                      }

                  }

                  //Portugues
                  if (!objOfertaLab.oi_nivel_porNull)
                  {
                      bool encontro = false;
                      foreach (NucleusRH.Base.Postulantes.CV.IDIOMA_CV objIdioma in objCV.IDIOMAS_CV)
                      {
                          //Idioma 5 es portugues
                          if (objIdioma.oi_idioma == "5")
                          {
                              encontro = true;
                              //Si el nivel es nativo, comparamos por igual
                              if (objOfertaLab.oi_nivel_por == "5")
                              {
                                  if (!((objIdioma.oi_nivel_habla == objOfertaLab.oi_nivel_por) || (objIdioma.oi_nivel_lee == objOfertaLab.oi_nivel_por) || (objIdioma.oi_nivel_escribe == objOfertaLab.oi_nivel_por)))
                                  {
                                      strResult = strResult + ",No cumple con el nivel de portugués";
                                      l_cumple_req = false;
                                  }
                              }
                              else
                              { 
                                //Sino, comparamos por mayor igual
                                  if (!((int.Parse(objIdioma.oi_nivel_habla) >= int.Parse(objOfertaLab.oi_nivel_por)) || (int.Parse(objIdioma.oi_nivel_lee) >= int.Parse(objOfertaLab.oi_nivel_por)) || (int.Parse(objIdioma.oi_nivel_escribe) >= int.Parse(objOfertaLab.oi_nivel_por))))
                                  {
                                      strResult = strResult + ",No cumple con el nivel de portugués";
                                      l_cumple_req = false;
                                  }
                              }

                          }
                          //Si lo encontro, salgo
                          if (encontro)
                              break;
                      }

                      //Si no lo encontro, no cumple con el nivel de portugues
                      if (!encontro)
                      {
                          strResult = strResult + ",No cumple con el nivel de portugués";
                          l_cumple_req = false;
                      }
                  }

                  //Frances
                  if (!objOfertaLab.oi_nivel_frNull)
                  {
                      bool encontro = false;
                      foreach (NucleusRH.Base.Postulantes.CV.IDIOMA_CV objIdioma in objCV.IDIOMAS_CV)
                      {
                          //Idioma 2 es frances
                          if (objIdioma.oi_idioma == "2")
                          {
                              encontro = true;
                              //Si el nivel es nativo, comparamos por igual
                              if (objOfertaLab.oi_nivel_fr == "5")
                              {
                                  if (!((objIdioma.oi_nivel_habla == objOfertaLab.oi_nivel_fr) || (objIdioma.oi_nivel_lee == objOfertaLab.oi_nivel_fr) || (objIdioma.oi_nivel_escribe == objOfertaLab.oi_nivel_fr)))
                                  {
                                      strResult = strResult + ",No cumple con el nivel de francés";
                                      l_cumple_req = false;
                                  }
                              }
                              else 
                              { 
                                //Sino, comparamos por mayor igual
                                  if (!((int.Parse(objIdioma.oi_nivel_habla) >= int.Parse(objOfertaLab.oi_nivel_fr)) || (int.Parse(objIdioma.oi_nivel_lee) >= int.Parse(objOfertaLab.oi_nivel_fr)) || (int.Parse(objIdioma.oi_nivel_escribe) >= int.Parse(objOfertaLab.oi_nivel_fr))))
                                  {
                                      strResult = strResult + ",No cumple con el nivel de francés";
                                      l_cumple_req = false;
                                  }
                              }
                          }
                          //Si lo encontro, salgo
                          if (encontro)
                              break;
                      }

                      //Si no lo encontro, no cumple con el nivel de frances
                      if (!encontro)
                      {
                          strResult = strResult + ",No cumple con el nivel de francés";
                          l_cumple_req = false;
                      }

                  }
                  //Italiano
                  if (!objOfertaLab.oi_nivel_itNull)
                  {
                      bool encontro = false;
                      foreach (NucleusRH.Base.Postulantes.CV.IDIOMA_CV objIdioma in objCV.IDIOMAS_CV)
                      {
                          //Idioma 3 es italiano
                          if (objIdioma.oi_idioma == "3")
                          {
                              encontro = true;
                              //Si el nivel es nativo, comparamos por igual
                              if (objOfertaLab.oi_nivel_it == "5")
                              {
                                  if (!((objIdioma.oi_nivel_habla == objOfertaLab.oi_nivel_it) || (objIdioma.oi_nivel_lee == objOfertaLab.oi_nivel_it) || (objIdioma.oi_nivel_escribe == objOfertaLab.oi_nivel_it)))
                                  {
                                      strResult = strResult + ",No cumple con el nivel de italiano";
                                      l_cumple_req = false;
                                  }
                              }
                              else
                              {
                                  //Sino, comparamos por mayor igual
                                  if (!((int.Parse(objIdioma.oi_nivel_habla) >= int.Parse(objOfertaLab.oi_nivel_it)) || (int.Parse(objIdioma.oi_nivel_lee) >= int.Parse(objOfertaLab.oi_nivel_it)) || (int.Parse(objIdioma.oi_nivel_escribe) >= int.Parse(objOfertaLab.oi_nivel_it))))
                                  {
                                      strResult = strResult + ",No cumple con el nivel de italiano";
                                      l_cumple_req = false;
                                  }
                              }
                          }
                          //Si lo encontro, salgo
                          if (encontro)
                              break;
                      }

                      //Si no lo encontro, no cumple con el nivel de italiano
                      if (!encontro)
                      {
                          strResult = strResult + ",No cumple con el nivel de italiano";
                          l_cumple_req = false;
                      }

                  }
                  //Aleman
                  if (!objOfertaLab.oi_nivel_alNull)
                  {
                      bool encontro = false;
                      foreach (NucleusRH.Base.Postulantes.CV.IDIOMA_CV objIdioma in objCV.IDIOMAS_CV)
                      {
                          //Idioma 8 es aleman
                          if (objIdioma.oi_idioma == "8")
                          {
                              encontro = true;
                              //Si el nivel es nativo, comparamos por igual
                              if (objOfertaLab.oi_nivel_al == "5")
                              {
                                  if (!((objIdioma.oi_nivel_habla == objOfertaLab.oi_nivel_al) || (objIdioma.oi_nivel_lee == objOfertaLab.oi_nivel_al) || (objIdioma.oi_nivel_escribe == objOfertaLab.oi_nivel_al)))
                                  {
                                      strResult = strResult + ",No cumple con el nivel de alemán";
                                      l_cumple_req = false;
                                  }
                              }
                              else
                              {
                                  //Sino, comparamos por mayor igual
                                  if (!((int.Parse(objIdioma.oi_nivel_habla) >= int.Parse(objOfertaLab.oi_nivel_al)) || (int.Parse(objIdioma.oi_nivel_lee) >= int.Parse(objOfertaLab.oi_nivel_al)) || (int.Parse(objIdioma.oi_nivel_escribe) >= int.Parse(objOfertaLab.oi_nivel_al))))
                                  {
                                      strResult = strResult + ",No cumple con el nivel de alemán";
                                      l_cumple_req = false;
                                  }
                              }
                          }
                          //Si lo encontro, salgo
                          if (encontro)
                              break;
                      }

                      //Si no lo encontro, no cumple con el nivel de aleman
                      if (!encontro)
                      {
                          strResult = strResult + ",No cumple con el nivel de alemán";
                          l_cumple_req = false;
                      }

                  }
                  //Japones
                  if (!objOfertaLab.oi_nivel_japNull)
                  {
                      bool encontro = false;
                      foreach (NucleusRH.Base.Postulantes.CV.IDIOMA_CV objIdioma in objCV.IDIOMAS_CV)
                      {
                          //Idioma 9 es japones
                          if (objIdioma.oi_idioma == "9")
                          {
                              encontro = true;
                               //Si el nivel es nativo, comparamos por igual
                              if (objOfertaLab.oi_nivel_jap == "5")
                              {
                                  if (!((objIdioma.oi_nivel_habla == objOfertaLab.oi_nivel_jap) || (objIdioma.oi_nivel_lee == objOfertaLab.oi_nivel_jap) || (objIdioma.oi_nivel_escribe == objOfertaLab.oi_nivel_jap)))
                                  {
                                      strResult = strResult + ",No cumple con el nivel de japones";
                                      l_cumple_req = false;
                                  }
                              }
                              else
                              {
                                  //Sino, comparamos por mayor igual
                                  if (!((int.Parse(objIdioma.oi_nivel_habla) >= int.Parse(objOfertaLab.oi_nivel_jap)) || (int.Parse(objIdioma.oi_nivel_lee) >= int.Parse(objOfertaLab.oi_nivel_jap)) || (int.Parse(objIdioma.oi_nivel_escribe) >= int.Parse(objOfertaLab.oi_nivel_jap))))
                                  {
                                      strResult = strResult + ",No cumple con el nivel de japones";
                                      l_cumple_req = false;
                                  }
                              }
                          }
                          //Si lo encontro, salgo
                          if (encontro)
                              break;
                      }
                      //Si no lo encontro, no cumple con el nivel de japones
                      if (!encontro)
                      {
                          strResult = strResult + ",No cumple con el nivel de japones";
                          l_cumple_req = false;
                      }

                  }


              }

              strStep = "REQUISITO-SALARIO";
              if (objOfertaLab.l_exc_salario)
              {
                  //Si solicitar salario es requerido, compara con n_Remuneracion de la oferta laboral.
                  //Sino, compara con n_remuneracion de la postulacion si no es null, sino con la del CV

                  if (objOfertaLab.l_solicita_sal)
                  {
                      //caso 1: ambos salarios completos
                      if ((!objOfertaLab.n_salario_dNull) && (!objOfertaLab.n_salario_hNull))
                      {
                          if (!((objOfertaLab.n_salario_d >= pnRemuneracion) && (objOfertaLab.n_salario_h <= pnRemuneracion)))
                          {
                              strResult = strResult + ",No cumple con el expectativa salarial";
                              l_cumple_req = false;
                          }
                      }
                      //Caso 2 : salario desde completo y salario hasta null
                      if ((!objOfertaLab.n_salario_dNull) && (objOfertaLab.n_salario_hNull))
                      {
                          if (pnRemuneracion < objOfertaLab.n_salario_d)
                          {
                              strResult = strResult + ",No cumple con la expectativa salarial, es menor a la mínima";
                              l_cumple_req = false;
                          }
                      }
                      //Caso 3: salario desde null y salario hasta completo
                      if ((objOfertaLab.n_salario_dNull) && (!objOfertaLab.n_salario_hNull))
                      {
                          if (pnRemuneracion > objOfertaLab.n_salario_h)
                          {
                              strResult = strResult + ",No cumple con la expectativa salarial, es mayor a la máxima";
                              l_cumple_req = false;
                          }
                      }
                  }
                  else
                  {
                      //Si no es 0, compara con la remuneracion de la postulacion
                      if ((pnRemuneracion != 0))
                      {
                          //caso 1: ambos salarios completos
                          if ((!objOfertaLab.n_salario_dNull) && (!objOfertaLab.n_salario_hNull))
                          {
                              if (!((objOfertaLab.n_salario_d >= pnRemuneracion) && (objOfertaLab.n_salario_h < pnRemuneracion)))
                              {
                                  strResult = strResult + ",No cumple con la expectativa salarial";
                                  l_cumple_req = false;
                              }
                          }
                          //Caso 2 : salario desde completo y salario hasta null
                          if ((!objOfertaLab.n_salario_dNull) && (objOfertaLab.n_salario_hNull))
                          {
                              if (pnRemuneracion < objOfertaLab.n_salario_d)
                              {
                                  strResult = strResult + ",No cumple con la expectativa salarial, es menor a la mínima";
                                  l_cumple_req = false;
                              }
                          }
                          //Caso 3: salario desde null y salario hasta completo
                          if ((objOfertaLab.n_salario_dNull) && (!objOfertaLab.n_salario_hNull))
                          {
                              if (pnRemuneracion > objOfertaLab.n_salario_h)
                              {
                                  strResult = strResult + ",No cumple con la expectativa salarial, es mayor a la máxima";
                                  l_cumple_req = false;
                              }
                          }
                      }
                      else
                      {
                          //Sino, conla del cv si la completo
                          if (!objCV.n_remuneracionNull)
                          {
                              //caso 1: ambos salarios completos
                              if ((!objOfertaLab.n_salario_dNull) && (!objOfertaLab.n_salario_hNull))
                              {
                                  if (!((objOfertaLab.n_salario_d >= objCV.n_remuneracion) && (objOfertaLab.n_salario_h < objCV.n_remuneracion)))
                                  {
                                      strResult = strResult + ",No cumple con la expectativa salarial";
                                      l_cumple_req = false;
                                  }
                              }
                              //Caso 2 : salario desde completo y salario hasta null
                              if ((!objOfertaLab.n_salario_dNull) && (objOfertaLab.n_salario_hNull))
                              {
                                  if (objCV.n_remuneracion < objOfertaLab.n_salario_d)
                                  {
                                      strResult = strResult + ",No cumple con la expectativa salarial, es menor a la mínima";
                                      l_cumple_req = false;
                                  }
                              }
                              //Caso 3: salario desde null y salario hasta completo
                              if ((objOfertaLab.n_salario_dNull) && (!objOfertaLab.n_salario_hNull))
                              {
                                  if (objCV.n_remuneracion > objOfertaLab.n_salario_h)
                                  {
                                      strResult = strResult + ",No cumple con la expectativa salarial, es mayor a la máxima";
                                      l_cumple_req = false;
                                  }
                              }

                          }
                          else
                          {
                              //Si no completo,no cumple
                              strResult = strResult + ",No cumple con la expectativa salarial";
                              l_cumple_req = false;
                          }
                      }
                  }
              }

              return strResult;
          }
          catch (Exception ex)
          {
              NomadException nmdEx = new NomadException("NucleusRH.Base.Postulantes.Web_CV.WEB_Cumple_Requisitos()", ex);
              nmdEx.SetValue("Step", strStep);


              throw nmdEx;
          }

      }


      /// <summary>
      ///Recupera todas las Ofertas Laborales 
      /// </summary>
      /// <returns></returns>
      public static List<WEB_OfertaLab> GetOfertas()
      {
          NomadLog.Debug("-----------------------------------------------");
          NomadLog.Debug("----------GetOfertas V1-----------------");
          NomadLog.Debug("-----------------------------------------------");

          string strStep = "";

          try
          {
              strStep = "QRY-GET-OLS";
              NomadXML xmlOfertasLab = Nomad.NSystem.Base.NomadEnvironment.QueryNomadXML(NucleusRH.Base.Postulantes.Ofertas_Laborales.OFERTA_LAB.Resources.WEB_GET_OFERTA_LAB, "");

              strStep = "NEW-LIST-OL";
              List<WEB_OfertaLab> OfertasLabList = new List<WEB_OfertaLab>();

              strStep = "SET-LIST-OL";
              for (NomadXML row = xmlOfertasLab.FirstChild().FirstChild(); row != null; row = row.Next())
              {
                  WEB_OfertaLab objOferta = new WEB_OfertaLab();
                  objOferta.oi_oferta_lab = row.GetAttrInt("oi_oferta_lab");
                  objOferta.f_oferta_lab = row.GetAttrDateTime("f_oferta_lab");
                  objOferta.d_oferta_lab = row.GetAttr("d_oferta_lab");
                  objOferta.o_oferta_lab = row.GetAttr("o_oferta_lab");
                  objOferta.c_oferta_lab = row.GetAttr("c_oferta_lab");
                 
                  if (row.GetAttr("f_cierre") != "")
                      objOferta.f_cierre = row.GetAttrDateTime("f_cierre");
                  else
                      objOferta.f_cierre = null;

                  if (row.GetAttr("e_cantidad") != "")
                      objOferta.e_cantidad = row.GetAttrInt("e_cantidad");
                  else
                      objOferta.e_cantidad = null;

                  if (row.GetAttr("oi_tipo_puesto") != "")
                      objOferta.oi_tipo_puesto = row.GetAttrInt("oi_tipo_puesto");
                  else
                      objOferta.oi_tipo_puesto = null;

                  if (row.GetAttr("oi_seniority") != "")
                      objOferta.oi_seniority = row.GetAttrInt("oi_seniority");
                  else
                      objOferta.oi_seniority = null;

                  if (row.GetAttr("oi_area_lab") != "")
                      objOferta.oi_area_lab = row.GetAttrInt("oi_area_lab");
                  else
                      objOferta.oi_area_lab = null;

                  if (row.GetAttr("oi_pais") != "")
                      objOferta.oi_pais = row.GetAttrInt("oi_pais");
                  else
                      objOferta.oi_pais = null;

                  if (row.GetAttr("oi_provincia") != "")
                      objOferta.oi_provincia = row.GetAttrInt("oi_provincia");
                  else
                      objOferta.oi_provincia = null;

                  if (row.GetAttr("oi_localidad") != "")
                      objOferta.oi_localidad = row.GetAttrInt("oi_localidad");
                  else
                      objOferta.oi_localidad = null;

                  if (row.GetAttr("d_coordenadas") != "")
                      objOferta.d_coordenadas = row.GetAttr("d_coordenadas");
                  else
                      objOferta.d_coordenadas = null;

                  if (row.GetAttr("l_mostrar_res") == "1")
                  {
                      if (row.GetAttr("oi_pais_res") != "")
                          objOferta.oi_pais_res = row.GetAttrInt("oi_pais_res");
                      else
                          objOferta.oi_pais_res = null;

                      if (row.GetAttr("oi_provincia_res") != "")
                          objOferta.oi_provincia_res = row.GetAttrInt("oi_provincia_res");
                      else
                          objOferta.oi_provincia_res = null;

                      if (row.GetAttr("oi_localidad_res") != "")
                          objOferta.oi_localidad_res = row.GetAttrInt("oi_localidad_res");
                      else
                          objOferta.oi_localidad_res = null;
                  }
                  else
                  {
                      objOferta.oi_pais_res = null;
                      objOferta.oi_provincia_res = null;
                      objOferta.oi_localidad_res = null;
                  }


                  if (row.GetAttr("l_mostrar_exp") == "1")
                  {
                      if (row.GetAttr("e_exp_des") != "")
                          objOferta.e_exp_des = row.GetAttrInt("e_exp_des");
                      else
                          objOferta.e_exp_des = null;

                      if (row.GetAttr("oi_uni_tpo_desde") != "")
                          objOferta.oi_uni_tpo_desde = row.GetAttrInt("oi_uni_tpo_desde");
                      else
                          objOferta.oi_uni_tpo_desde = null;

                      if (row.GetAttr("e_exp_hasta") != "")
                          objOferta.e_exp_hasta = row.GetAttrInt("e_exp_hasta");
                      else
                          objOferta.e_exp_hasta = null;

                      if (row.GetAttr("oi_uni_tpo_hasta") != "")
                          objOferta.oi_uni_tpo_hasta = row.GetAttrInt("oi_uni_tpo_hasta");
                      else
                          objOferta.oi_uni_tpo_hasta = null;
                  }
                  else
                  { 
                      objOferta.e_exp_des = null;
                      objOferta.oi_uni_tpo_desde = null;
                      objOferta.e_exp_hasta = null;
                      objOferta.oi_uni_tpo_hasta = null;
                  }


                  if (row.GetAttr("l_mostrar_edad") == "1")
                  {
                      if (row.GetAttr("e_edad_desde") != "")
                          objOferta.e_edad_desde = row.GetAttrInt("e_edad_desde");
                      else
                          objOferta.e_edad_desde = null;

                      if (row.GetAttr("e_edad_hasta") != "")
                          objOferta.e_edad_hasta = row.GetAttrInt("e_edad_hasta");
                      else
                          objOferta.e_edad_hasta = null;
                  }
                  else       
                  {
                      objOferta.e_edad_desde = null;
                      objOferta.e_edad_hasta = null;
                  }

                  if (row.GetAttr("l_mostrar_sexo") == "1")
                  {
                      if (row.GetAttr("c_sexo") != "")
                          objOferta.c_sexo = row.GetAttr("c_sexo");
                      else
                          objOferta.c_sexo = null;
                  }
                  else
                      objOferta.c_sexo = null;


                  if (row.GetAttr("l_mostrar_ecivil") == "1")
                  {
                      if (row.GetAttr("oi_estado_civil") != "")
                          objOferta.oi_estado_civil = row.GetAttrInt("oi_estado_civil");
                      else
                          objOferta.oi_estado_civil = null;
                  }
                  else
                      objOferta.oi_estado_civil = null;

                  if (row.GetAttr("l_mostrar_edu") == "1")
                  {
                      if (row.GetAttr("oi_nivel_estudio") != "")
                          objOferta.oi_nivel_estudio = row.GetAttrInt("oi_nivel_estudio");
                      else
                          objOferta.oi_nivel_estudio = null;

                      if (row.GetAttr("oi_estado_est") != "")
                          objOferta.oi_estado_est = row.GetAttrInt("oi_estado_est");
                      else
                          objOferta.oi_estado_est = null;
                  }
                  else
                  {
                      objOferta.oi_nivel_estudio = null;
                      objOferta.oi_estado_est = null;
                  }

                  if (row.GetAttr("l_mostrar_idioma") == "1")
                  {
                      if (row.GetAttr("oi_nivel_ing") != "")
                          objOferta.oi_nivel_ing = row.GetAttrInt("oi_nivel_ing");
                      else
                          objOferta.oi_nivel_ing = null;

                      if (row.GetAttr("oi_nivel_por") != "")
                          objOferta.oi_nivel_por = row.GetAttrInt("oi_nivel_por");
                      else
                          objOferta.oi_nivel_por = null;

                      if (row.GetAttr("oi_nivel_fr") != "")
                          objOferta.oi_nivel_fr = row.GetAttrInt("oi_nivel_fr");
                      else
                          objOferta.oi_nivel_fr = null;

                      if (row.GetAttr("oi_nivel_it") != "")
                          objOferta.oi_nivel_it = row.GetAttrInt("oi_nivel_it");
                      else
                          objOferta.oi_nivel_it = null;

                      if (row.GetAttr("oi_nivel_al") != "")
                          objOferta.oi_nivel_al = row.GetAttrInt("oi_nivel_al");
                      else
                          objOferta.oi_nivel_al = null;

                      if (row.GetAttr("oi_nivel_jap") != "")
                          objOferta.oi_nivel_jap = row.GetAttrInt("oi_nivel_jap");
                      else
                          objOferta.oi_nivel_jap = null;
                  }
                  else
                  {
                      objOferta.oi_nivel_ing = null;
                      objOferta.oi_nivel_por = null;
                      objOferta.oi_nivel_fr = null;
                      objOferta.oi_nivel_it = null;
                      objOferta.oi_nivel_al = null;
                      objOferta.oi_nivel_jap = null;
                  }


                  if (row.GetAttr("n_salario_d") != "")
                      objOferta.n_salario_d = row.GetAttrInt("n_salario_d");
                  else
                      objOferta.n_salario_d = null;

                  if (row.GetAttr("n_salario_h") != "")
                      objOferta.n_salario_h = row.GetAttrInt("n_salario_h");
                  else
                      objOferta.n_salario_h = null;

                  objOferta.l_exc_res = row.GetAttrBool("l_exc_res");
                  objOferta.l_exc_exp = row.GetAttrBool("l_exc_exp");
                  objOferta.l_exc_edad = row.GetAttrBool("l_exc_edad");
                  objOferta.l_exc_sexo = row.GetAttrBool("l_exc_sexo");
                  objOferta.l_exc_ecivil = row.GetAttrBool("l_exc_ecivil");
                  objOferta.l_exc_edu = row.GetAttrBool("l_exc_edu");
                  objOferta.l_exc_idioma = row.GetAttrBool("l_exc_idioma");
                  objOferta.l_exc_salario = row.GetAttrBool("l_exc_salario");

                  //Mostrar salario en aviso y solicitar salario al postulante
                  objOferta.l_mostrar_en_aviso = row.GetAttrBool("l_mostrar_en_aviso"); 
                  objOferta.l_solicita_sal = row.GetAttrBool("l_solicita_sal"); 

                  if (row.GetAttr("d_mail") != "")
                      objOferta.d_mail = row.GetAttr("d_mail");
                  else
                      objOferta.d_mail = null;

                  if (row.GetAttr("d_mail_alt_1") != "")
                      objOferta.d_mail_alt_1 = row.GetAttr("d_mail_alt_1");
                  else
                      objOferta.d_mail_alt_1 = null;


                  if (row.GetAttr("d_mail_alt_2") != "")
                      objOferta.d_mail_alt_2 = row.GetAttr("d_mail_alt_2");
                  else
                      objOferta.d_mail_alt_2 = null;


                  if (row.GetAttr("oi_empresa") != "")
                      objOferta.oi_empresa = row.GetAttrInt("oi_empresa");
                  else
                      objOferta.oi_empresa = null;

                  if (row.GetAttr("c_uni_negocio") != "")
                      objOferta.c_uni_negocio = row.GetAttr("c_uni_negocio");
                  else
                      objOferta.c_uni_negocio = null;

                  if (row.GetAttr("d_uni_negocio") != "")
                      objOferta.d_uni_negocio = row.GetAttr("d_uni_negocio");
                  else
                      objOferta.d_uni_negocio = null;

                  //Ajustes para la web:
                  //Los productos  "Tercerización TI","Búsquedas Directas TI" y "RPO TI" son unidad de negocio TEC
                  //El Producto 43 - Busquedas Directas es de unidad de negocio BPRO 
                  //Existe la unidad de negocio HOT - HOTELERIA que por ahora no se completa
                  if ((row.GetAttr("c_tipos_serv") == "40") || (row.GetAttr("c_tipos_serv") == "41") || (row.GetAttr("c_tipos_serv") == "44"))
                  {
                      objOferta.c_uni_negocio = "TEC";
                      objOferta.d_uni_negocio = "Tecnología";
                  }

                  //Ajuste 2. Si el producto es 43 - Busquedas Directas la unidad de negocio es BPRO Bayton Professional
                  if (row.GetAttr("c_tipos_serv") == "43")
                  {
                      objOferta.c_uni_negocio = "BPRO";
                      objOferta.d_uni_negocio = "Bayton Professional";
                  }

                  

                  strStep = "ADD-PREG";
                  NomadXML xmlPreguntas = row.FindElement2("PREGUNTAS","name","PREGUNTAS");
                  for (NomadXML rowPreg = xmlPreguntas.FirstChild(); rowPreg != null; rowPreg = rowPreg.Next())
                  {
                      WEB_Preguntas objPregunta = new WEB_Preguntas();
                      objPregunta.ID = rowPreg.GetAttr("oi_pregunta");
                      objPregunta.d_pregunta = rowPreg.GetAttr("d_pregunta");

                      if (rowPreg.GetAttr("c_pregunta") == "M")
                      {
                         NomadXML xmlOpciones = rowPreg.FindElement2("OPCIONES", "NAME", "OPCIONES");
                         for (NomadXML rowOp = xmlOpciones.FirstChild(); rowOp != null; rowOp = rowOp.Next())
                         {
                             WEB_Preguntas_Opciones objOpcion = new WEB_Preguntas_Opciones();
                             objOpcion.ID = rowOp.GetAttr("oi_pregunta_op");
                             objOpcion.d_pregunta_op = rowOp.GetAttr("d_pregunta_op");

                             objPregunta.Opciones.Add(objOpcion);
                         }
                      }
                      objOferta.Preguntas.Add(objPregunta);
                  }
                  
                  OfertasLabList.Add(objOferta);

              }


              return OfertasLabList;
          }

          catch (Exception ex)
          {
              NomadException nmdEx = new NomadException("NucleusRH.Base.Postulantes.Web_CV.Web_Get_CV()", ex);
              nmdEx.SetValue("Step", strStep);
              throw nmdEx;
          }

      }
      
      /// <summary>
      /// Recupera la lista de detos en formato codigo-descripcion de la codificadora parametro
      /// </summary>
      /// <param name="tipoCodificadora"></param>
      /// <returns></returns>
      public static List<WEB_Codificadora> GetCodificadora(string tipoCodificadora)
      {
          NomadLog.Debug("-----------------------------------------------");
          NomadLog.Debug("----------GetCodificadora V1-----------------------------");
          NomadLog.Debug("-----------------------------------------------");

          NomadLog.Debug("GetCodificadora.tipoCodificadora: " + tipoCodificadora);

          string strStep = "";
          List<WEB_Codificadora> listCodificadora = null;

          try
          {
              strStep = "ARMO-PARAM";
              NomadXML xmlParam = new NomadXML("DATA");
              xmlParam.SetAttr("tipoCodificadora", tipoCodificadora);

              strStep = "EJECUTO-QRY";
              NomadXML xmlCodificadoras = Nomad.NSystem.Base.NomadEnvironment.QueryNomadXML(NucleusRH.Base.Postulantes.CV.CV.Resources.WEB_CODIFICADORAS, xmlParam.ToString());


              listCodificadora = new List<WEB_Codificadora>();

              strStep = "RECORRO-XML";
              for (NomadXML row = xmlCodificadoras.FirstChild().FindElement(tipoCodificadora).FirstChild(); row != null; row = row.Next())
              {
                  strStep = "ARMO-OBJETO";
                  WEB_Codificadora objCodificadora = new WEB_Codificadora();
                  objCodificadora.ID = row.GetAttr("ID");
                  objCodificadora.Codigo = row.GetAttr("Codigo");
                  objCodificadora.Descripcion = row.GetAttr("Descripcion");

                  if (row.GetAttr("ParentID") != "")
                      objCodificadora.ParentID = row.GetAttr("ParentID");

                  strStep = "ADD-OBJ";
                  listCodificadora.Add(objCodificadora);
              }


          }
          catch (Exception ex)
          {
              NomadException nmdEx = new NomadException("NucleusRH.Base.Postulantes.Web_CV.GetCodificadora()", ex);
              nmdEx.SetValue("Step", strStep);

              throw nmdEx;
          }

          return listCodificadora;
      }


    


      #endregion

      /// <summary>
      /// Actualiza todas las postulaciones a ofertas activas del CV si modo es CV.
      /// Actualiza todos los CVs postulados a la oferta si el modo es OL
      /// Actualizar implica cambiar los
      /// </summary>
      /// <param name="oiCv"></param>
      public static void ActualizarPostulaciones(string pstrId,string strMode)
      { 
      
        NomadLog.Debug("----------------------------------------------------------");
        NomadLog.Debug("-----------------ActualizarPostulaciones V1--------------");
        NomadLog.Debug("----------------------------------------------------------");

        NomadLog.Debug("id: " + pstrId);        
        NomadLog.Debug("strMode: " + strMode); 

        string strStep = "";
        NomadTransaction objTrans;
        try
        {
            NomadXML xmlRes = new NomadXML();
            
            if (strMode == "CV")
            {
                strStep = "CREATE-PARAM-CV";
                NomadXML xmlParam = new NomadXML("DATA");
                xmlParam.SetAttr("oi_cv", pstrId);

                strStep = "GET-POSTU";
                xmlRes= Nomad.NSystem.Base.NomadEnvironment.QueryNomadXML(NucleusRH.Base.Postulantes.CV.CV.Resources.GET_POSTU_ACTIVAS, xmlParam.ToString());

                 strStep = "RECORRO-POSTU";
                 for (NomadXML xmlRow = xmlRes.FirstChild().FirstChild(); xmlRow != null; xmlRow = xmlRow.Next())
                 {
                     string oiOfertaLab = xmlRow.GetAttr("oi_oferta_lab");
                     string oiPostulacion = xmlRow.GetAttr("oi_postulaciones");

                     strStep = "GET-POSTU-" + oiPostulacion;
                     NucleusRH.Base.Postulantes.CV.POSTULACIONES objPostu = NucleusRH.Base.Postulantes.CV.POSTULACIONES.Get(oiPostulacion);
                     
                     //Invoco el metodo que calcula requisitos
                     strStep = "CUMPLE-REQ-" + pstrId + "-" + oiOfertaLab;
                     string req = InternalCumpleRequisitos(pstrId,oiOfertaLab,0);

                     if (req == "")
                     {
                         objPostu.l_cumple_req = true;
                         objPostu.d_cumple_reqNull = true;
                     }
                     else
                     {
                         //Ajusto cadena  
                         if (req.Length >= 4000)
                         {
                             //Acoto la cadena a 4000
                             req = req.Substring(0, 4000);

                             //Remuevo lo inconsistente que queda despues de la ultima coma
                             int pos = req.LastIndexOf(',');
                             req = req.Substring(0, pos);
                         }

                         objPostu.l_cumple_req = false;
                         objPostu.d_cumple_req = req;
                     }
                     
                     //Persisto la postulacion
                     strStep = "SAVE-POSTU-CV";
                     objTrans = new NomadTransaction();
                     objTrans.Begin();
                     objTrans.Save(objPostu);
                     objTrans.Commit();
                 }
            }
            if (strMode == "OL")
            {
                strStep = "CREATE-PARAM-OL";
                NomadXML xmlParam = new NomadXML("DATA");
                xmlParam.SetAttr("oi_oferta_lab", pstrId);

                strStep = "GET-POSTU";
                xmlRes = Nomad.NSystem.Base.NomadEnvironment.QueryNomadXML(NucleusRH.Base.Postulantes.Ofertas_Laborales.OFERTA_LAB.Resources.GET_POSTU_CV, xmlParam.ToString());

                 strStep = "RECORRO-POSTU";
                 for (NomadXML xmlRow = xmlRes.FirstChild().FirstChild(); xmlRow != null; xmlRow = xmlRow.Next())
                 {
                     string oiCv = xmlRow.GetAttr("oi_cv");
                     string oiPostulacion = xmlRow.GetAttr("oi_postulaciones");

                     strStep = "GET-POSTU-" + oiPostulacion;
                     NucleusRH.Base.Postulantes.CV.POSTULACIONES objPostu = NucleusRH.Base.Postulantes.CV.POSTULACIONES.Get(oiPostulacion);

                     //Invoco el metodo que calcula requisitos
                     strStep = "CUMPLE-REQ-" + oiCv + "-" + pstrId;
                     string req = InternalCumpleRequisitos(oiCv, pstrId, 0);

                     if (req == "")
                     {
                         objPostu.l_cumple_req = true;
                         objPostu.d_cumple_reqNull = true;
                     }
                     else
                     {
                         //Ajusto cadena  
                         if (req.Length >= 4000)
                         {
                             //Acoto la cadena a 4000
                             req = req.Substring(0, 4000);

                             //Remuevo lo inconsistente que queda despues de la ultima coma
                             int pos = req.LastIndexOf(',');
                             req = req.Substring(0, pos);
                         }

                         objPostu.l_cumple_req = false;
                         objPostu.d_cumple_req = req;
                     }

                     //Persisto la postulacion
                     strStep = "SAVE-POSTU-OL";
                     objTrans = new NomadTransaction();
                     objTrans.Begin();
                     objTrans.Save(objPostu);
                     objTrans.Commit();
                 }
            }        

        }
        catch (Exception ex)
        {
            NomadException nmdEx = new NomadException("NucleusRH.Base.Postulantes.CV.ActualizarPostulaciones()", ex);
            nmdEx.SetValue("Step", strStep);
            throw nmdEx;
        }
      }
  }
    
    
}





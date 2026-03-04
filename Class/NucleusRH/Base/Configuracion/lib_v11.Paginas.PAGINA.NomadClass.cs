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

namespace NucleusRH.Base.Configuracion.Paginas
{

    //////////////////////////////////////////////////////////////////////////////////
    //Clase Paginas
    public partial class PAGINA : Nomad.NSystem.Base.NomadObject
    {

    public static void ReplaceIMAGES(NomadXML check, NomadXML imgs)
    {
      for (NomadXML MyCUR=check.FirstChild(); MyCUR!=null; MyCUR=MyCUR.Next())
      {
        if (MyCUR.Name=="IMG")
        {
          NomadXML IMGSRC=imgs.FindElement2("IMG","id",MyCUR.GetAttr("img-id"));

          if (IMGSRC!=null)
            MyCUR.SetAttr("src",IMGSRC.GetAttr("src-html"));

        } else
        if ((MyCUR.Name=="LINK") || (MyCUR.Name=="E-LINK"))
        {
          NomadXML LNKSRC=imgs.FindElement2("LINK","id",MyCUR.GetAttr("link-id"));

          if (LNKSRC!=null)
          {
            string[] ext=LNKSRC.GetAttr("src-html").Split('.');
            MyCUR.SetAttr("url", LNKSRC.GetAttr("src-html"));
            MyCUR.SetAttr("ext", LNKSRC.GetAttr("src-ext"));
          }

        } else
          ReplaceIMAGES(MyCUR, imgs);
      }
    }

    public static string GenerateHTML(string reportName, NomadXML xmlParams)
    {
      //Guardo el Resultado en el STREAM
	  Nomad.NomadHTML objHtml = new Nomad.NomadHTML(reportName, xmlParams);
	  return objHtml.GenerateString();
    }

    public static string GenerateTABLE(NomadXML MyXML, string path, string strLang)
    {
      int expand, X, Y, SX, SY, X1, Y1;
      NomadXML MyCOL, MyROW, MyAUX;
	  NomadXML[,] MyTBL;
      if (MyXML.isDocument) MyXML=MyXML.FirstChild();

      System.Text.StringBuilder HTML=new System.Text.StringBuilder();

      HTML.Append("<html><head><META HTTP-EQUIV=\"PRAGMA\" CONTENT=\"NO-CACHE\" /><META HTTP-EQUIV=\"CACHE-CONTROL\" CONTENT=\"NO-CACHE\" /><script>function Goto(obj) { parent.Goto(obj); } </script></head><body style=\"width:100%;height:100%;background-color:#FFFFFF;cursor:default;PADDING-RIGHT:0px;PADDING-LEFT:0px;PADDING-BOTTOM:0px;MARGIN:0px;PADDING-TOP:0px;BORDER-TOP:none;BORDER-RIGHT:none;BORDER-LEFT:none;BORDER-BOTTOM:none;\" scroll=\"no\">");

      //Busco si hay alguna colmna
      if (MyXML.FindElement("ROWS").FindElement2("TR","height","*")!=null)
        HTML.Append("<TABLE style='background-color:FFF;table-layout:fixed;font-size:1px;width:100%;height:100%;' cellspacing='0' cellpadding='0' >");
      else
        HTML.Append("<TABLE style='background-color:FFF;table-layout:fixed;font-size:1px;width:100%;' cellspacing='0' cellpadding='0' >");
	  
	  SX=MyXML.FindElement("COLS").ChildLength;
	  SY=MyXML.FindElement("ROWS").ChildLength;

      //Agrego Columnas
      for (MyCOL=MyXML.FindElement("COLS").FirstChild(); MyCOL!=null; MyCOL=MyCOL.Next())
        HTML.Append("<COL width='"+MyCOL.GetAttr("width")+"%' />");

	  //Creo el Arrat 2D
      MyTBL = new NomadXML[SY, SX];
      for (MyROW=MyXML.FindElement("ROWS").FirstChild(), Y=0; MyROW!=null; MyROW=MyROW.Next(), Y++)
	  for (MyCOL=MyROW.FirstChild(), X=0; MyCOL!=null; MyCOL=MyCOL.Next(), X++)
	  {
		  MyTBL[Y, X] = MyCOL;
	  }

      //Recorro los ROWS
      for (MyROW=MyXML.FindElement("ROWS").FirstChild(), Y=0; MyROW!=null; MyROW=MyROW.Next(), Y++)
      {
        if (MyROW.GetAttr("height")=="*") HTML.Append("<TR>");
                                     else HTML.Append("<TR style='height:"+MyROW.GetAttr("height")+";max-height:"+MyROW.GetAttr("height")+";'  >");
													
        //Recorro las CELDAS
        //for (MyCOL=MyROW.FirstChild(), X=0; MyCOL!=null; MyCOL=MyCOL.Next(), X++)
	    for (X=0; X<SX; X++)
        {
		  MyCOL=MyTBL[Y, X];

          string url=MyCOL.GetAttr("url");
          if (url=="") url="blank.htm";
          url=url.Replace(".LANG."   ,"."+strLang+".");
          url=url.Replace("/PREVIEW/", "/"+path+"/");

          switch(MyCOL.GetAttr("union"))
          {
            case "N":
              HTML.Append("<TD align=\"left\" valign=\"top\" ><iframe src=\"../../../"+url+"\" style=\"width:100%;height:100%;border:none;padding:0px;margin:0px;\" ></iframe></TD>");
            break;

            case "R":
              expand=1;
              for (X1=X+1; X1<SX; X1++)
			  {
				  MyAUX=MyTBL[Y, X1];
				  if (MyAUX.GetAttr("union")!="RL" && MyAUX.GetAttr("union")!="L") break;
				  expand++;
			  }
              HTML.Append("<TD colspan=\""+expand+"\" align=\"left\" valign=\"top\" ><iframe src=\"../../../"+url+"\" style=\"width:100%;height:100%;border:none;padding:0px;margin:0px;\" ></iframe></TD>");
            break;

            case "B":
              expand=1;
              for (Y1=Y+1; Y1<SY; Y1++)
			  {
				  MyAUX=MyTBL[Y1, X];
				  if (MyAUX.GetAttr("union")!="BT" && MyAUX.GetAttr("union")!="T") break;
				  expand++;
			  }
              HTML.Append("<TD rowspan=\""+expand+"\" align=\"left\" valign=\"top\" ><iframe src=\"../../../"+url+"\" style=\"width:100%;height:100%;border:none;padding:0px;margin:0px;\" ></iframe></TD>");
            break;
          }
        }

        HTML.Append("</TR>");
      }

      HTML.Append("</TABLE>");
      HTML.Append("</body></html>");

      //Resultado
      return HTML.ToString();
    }

    public static void Save(NucleusRH.Base.Configuracion.Paginas.PAGINA DDO)
    {
        Nomad.NSystem.Proxy.BINFile ff = null;
        DDO.f_cambio = DateTime.Now;

        //Existe la Carpeta de PREVIEW?
        string FILEPATH = NomadProxy.GetProxy().RunPath + "\\WEB\\GenFiles\\" + NomadProxy.GetProxy().AppName + "\\PREVIEW";
        if (!System.IO.Directory.Exists(FILEPATH))
            System.IO.Directory.CreateDirectory(FILEPATH);

        if (DDO.c_tipo == "WIKI")
        {
            NomadLog.Debug("----- WIKI -----");
            //Texto
            string WIKITxt = DDO.o_text1 + DDO.o_text2 + DDO.o_text3 + DDO.o_text4;

            //Archivo a Eliminar?
            NucleusRH.Base.Configuracion.Archivos.HEAD MyDEL = null;
            if (DDO.oi_file != null && DDO.oi_file != "")
            {
                MyDEL = NucleusRH.Base.Configuracion.Archivos.HEAD.Get(DDO.oi_file);
                DDO.oi_fileNull = true;
            }

            //Genera los atributos TXT y HTML desde el campo WIKI
            if (WIKITxt != "")
            {
                NomadXML xmlResult = new NomadXML("HELP");
                NomadXML xmlResultWIKI;
                NomadXML xmlImages = new NomadXML("IMAGES");

                xmlResultWIKI = xmlResult.AddTailElement("PAGES").AddTailElement("WIKI");

                //Analizo el WIKI
                Nomad.Base.Report.GeneralReport.REPORT.WIKIEncode("Mail.WIKI", WIKITxt, xmlResultWIKI, xmlImages);

                //Actualizar las IMAGENES
                NomadLog.Debug("xmlImages: " + xmlImages.ToString());
                for (NomadXML cur = xmlImages.FirstChild(); cur != null; cur = cur.Next())
                {
                    cur.SetAttr("id-find", "ID:" + cur.GetAttr("id"));
                    switch (cur.GetAttr("type").ToUpper())
                    {
                        case "DB":
                            //Obtengo el ID                                         
                            string ID = cur.GetAttr("src").Split(':')[1];

                            //Obtengo el Adjunto				
                            ADJUNTO MyAD = (ADJUNTO)DDO.ADJUNTOS.GetByAttribute("oi_file", cur.GetAttr("src").Split(':')[1]);
                            if (MyAD == null)
                            {
                                //Agrego el Adjunto
                                BINFile MyBIN = NomadProxy.GetProxy().BINService().GetFile("NucleusRH.Base.Configuracion.Archivos.HEAD", ID);
                                MyAD = new ADJUNTO();
                                MyAD.c_adjunto = MyBIN.Name;
                                MyAD.d_adjunto = MyBIN.Name;
                                MyAD.oi_file = ID;
                                DDO.ADJUNTOS.Add(MyAD);

                                MyBIN.SaveFile(NomadProxy.GetProxy().RunPath + "\\WEB\\GenFiles\\" + NomadProxy.GetProxy().AppName + "\\PREVIEW");
                            }

                            //Obtengo la Extension
                            string[] strparts = MyAD.c_adjunto.Split('.');
                            string EXT = strparts[strparts.Length - 1];

                            //Seteo el Resultado
                            cur.SetAttr("src-html", "NucleusRH.Base.Configuracion.Archivos.HEAD." + ID + "." + EXT);
                            cur.SetAttr("src-ext", EXT);
                            break;

                        default:
                            cur.SetAttr("src-html", cur.GetAttr("src"));
                            break;
                    }
                }
                NomadLog.Debug("xmlImages: " + xmlImages.ToString());

                ReplaceIMAGES(xmlResultWIKI, xmlImages);
                NomadLog.Debug("xmlResultWIKI: " + xmlResultWIKI.ToString());

                //Genera el HTML
				Nomad.NomadHTML nmdHtml=new Nomad.NomadHTML((DDO.d_estilo == "" ? "NucleusRH.Base.Configuracion.STD.STYLE_WIKI.rpt" : DDO.d_estilo), "");

				//Guardo el Resultado en el STREAM
				MemoryStream strOut = new MemoryStream();
				nmdHtml.GenerateStream(xmlResult, strOut, System.Text.Encoding.UTF8);

                //Subo el Archivo	 
                strOut.Seek(0, SeekOrigin.Begin);
                StreamReader strREAD = new StreamReader(strOut);
                ff = NomadProxy.GetProxy().BINService().PutFile("NucleusRH.Base.Configuracion.Archivos.HEAD", "WIKI.html", strREAD.BaseStream);
                DDO.oi_file = ff.Id;
                strREAD.Close();

                ff.SaveFile(NomadProxy.GetProxy().RunPath + "\\WEB\\GenFiles\\" + NomadProxy.GetProxy().AppName + "\\PREVIEW");
            }

            //GUARDO el DDO                               
            NomadEnvironment.GetCurrentTransaction().Begin();
            NomadEnvironment.GetCurrentTransaction().SaveRefresh(DDO);
            if (MyDEL != null) NomadEnvironment.GetCurrentTransaction().Delete(MyDEL);
            NomadEnvironment.GetCurrentTransaction().Commit();

        }
        else
            if (DDO.c_tipo == "TABLE")
            {
                NomadLog.Debug("----- TABLE -----");

                //Archivo a Eliminar?
                NucleusRH.Base.Configuracion.Archivos.HEAD MyDEL = null;
                if (DDO.oi_file != null && DDO.oi_file != "")
                {
                    MyDEL = NucleusRH.Base.Configuracion.Archivos.HEAD.Get(DDO.oi_file);
                    DDO.oi_fileNull = true;
                }

                //Texto                 
                if (DDO.o_text1 != "")
                {
                    string HTMLResult = PAGINA.GenerateTABLE(new NomadXML(DDO.o_text1 + DDO.o_text2 + DDO.o_text3 + DDO.o_text4), "PREVIEW", "LANG");

                    //Guardo el Resultado en el STREAM
                    MemoryStream strOut = new MemoryStream();
                    System.IO.StreamWriter strWrt = new System.IO.StreamWriter(strOut, System.Text.Encoding.UTF8);
                    strWrt.Write(HTMLResult);
                    strWrt.Flush();

                    //Subo el Archivo	 
                    strOut.Seek(0, SeekOrigin.Begin);
                    StreamReader strREAD = new StreamReader(strOut);
                    ff = NomadProxy.GetProxy().BINService().PutFile("NucleusRH.Base.Configuracion.Archivos.HEAD", "TABLE.html", strREAD.BaseStream);
                    DDO.oi_file = ff.Id;
                    strREAD.Close();
                    strWrt.Close();

                    ff.SaveFile(NomadProxy.GetProxy().RunPath + "\\WEB\\GenFiles\\" + NomadProxy.GetProxy().AppName + "\\PREVIEW");
                }

                //GUARDO el DDO                               
                NomadEnvironment.GetCurrentTransaction().Begin();
                NomadEnvironment.GetCurrentTransaction().SaveRefresh(DDO);
                if (MyDEL != null) NomadEnvironment.GetCurrentTransaction().Delete(MyDEL);
                NomadEnvironment.GetCurrentTransaction().Commit();
            }
            else
                if (DDO.c_tipo == "GADGET")
                {
                    NomadLog.Debug("----- GADGET -----");
                    string PARAMTxt = DDO.o_text1 + DDO.o_text2 + DDO.o_text3 + DDO.o_text4;

                    //Archivo a Eliminar?
                    NucleusRH.Base.Configuracion.Archivos.HEAD MyDEL = null;
                    if (DDO.oi_file != null && DDO.oi_file != "")
                    {
                        MyDEL = NucleusRH.Base.Configuracion.Archivos.HEAD.Get(DDO.oi_file);
                        DDO.oi_fileNull = true;
                    }

                    if (DDO.d_estilo != "")
                    {
                        //Guardo el Resultado en el STREAM
                        MemoryStream strOut = new MemoryStream();
                        System.IO.StreamWriter strWrt = new System.IO.StreamWriter(strOut, System.Text.Encoding.UTF8);

                        NomadXML Params = null;
                        if (PARAMTxt != "")
                        {
                            Params = new NomadXML(PARAMTxt);
                            if (Params.isDocument) Params = Params.FirstChild();

                            NomadLog.Debug("PARAM:" + Params.ToString());
                        }

                        //Genero el TEXTO
                        if (DDO.c_modo_pub == "O")
                        {
                            string url = "../../../ReportViewer.aspx?lang=" + NomadProxy.GetProxy().Lang + "&report=" + NomadProxy.GetProxy().AppName + "." + DDO.d_estilo;
                            for (NomadXML cur = Params.FirstChild(); cur != null; cur = cur.Next())
                                url += "&" + cur.GetAttr("id") + "=" + cur.GetAttr("value");

                            //Guardo el HTML
                            strWrt.Write(PAGINA.Resources.HTML_REDIRECT.Replace("#URL#", url));
                        }
                        else
                        {
                            NomadXML PARAM = new NomadXML("PARAMS");
                            for (NomadXML cur = Params.FirstChild(); cur != null; cur = cur.Next())
                                PARAM.SetAttr(cur.GetAttr("id"), cur.GetAttr("value"));

                            //Ejecuta el reporte 
                            strWrt.Write(PAGINA.GenerateHTML(DDO.d_estilo, PARAM));
                        }
                        //FLUSH
                        strWrt.Flush();


                        //Subo el Archivo	 
                        strOut.Seek(0, SeekOrigin.Begin);
                        StreamReader strREAD = new StreamReader(strOut);
                        ff = NomadProxy.GetProxy().BINService().PutFile("NucleusRH.Base.Configuracion.Archivos.HEAD", "TABLE.html", strREAD.BaseStream);
                        DDO.oi_file = ff.Id;
                        strREAD.Close();

                        ff.SaveFile(NomadProxy.GetProxy().RunPath + "\\WEB\\GenFiles\\" + NomadProxy.GetProxy().AppName + "\\PREVIEW");
                    }

                    //GUARDO el DDO
                    NomadEnvironment.GetCurrentTransaction().Begin();
                    NomadEnvironment.GetCurrentTransaction().SaveRefresh(DDO);
                    if (MyDEL != null) NomadEnvironment.GetCurrentTransaction().Delete(MyDEL);
                    NomadEnvironment.GetCurrentTransaction().Commit();

                }
                else
                    if (DDO.c_tipo == "SWITCH")
                    {
                        NomadLog.Debug("----- SWITCH -----");

                        //Archivo a Eliminar?
                        NucleusRH.Base.Configuracion.Archivos.HEAD MyDEL = null;
                        if (DDO.oi_file != null && DDO.oi_file != "")
                        {
                            MyDEL = NucleusRH.Base.Configuracion.Archivos.HEAD.Get(DDO.oi_file);
                            DDO.oi_fileNull = true;
                        }

                        //Guardo el Resultado en el STREAM
                        MemoryStream strOut = new MemoryStream();
                        System.IO.StreamWriter strWrt = new System.IO.StreamWriter(strOut, System.Text.Encoding.UTF8);

                        //Guardo el HTML
                        strWrt.Write(PAGINA.Resources.HTML_REDIRECT.Replace("#URL#", "PAGE." + DDO.o_text1 + ".LANG.html"));

                        //FLUSH
                        strWrt.Flush();

                        //Subo el Archivo	 
                        strOut.Seek(0, SeekOrigin.Begin);
                        StreamReader strREAD = new StreamReader(strOut);
                        ff = NomadProxy.GetProxy().BINService().PutFile("NucleusRH.Base.Configuracion.Archivos.HEAD", "TABLE.html", strREAD.BaseStream);
                        DDO.oi_file = ff.Id;
                        strREAD.Close();

                        ff.SaveFile(NomadProxy.GetProxy().RunPath + "\\WEB\\GenFiles\\" + NomadProxy.GetProxy().AppName + "\\PREVIEW");

                        //GUARDO el DDO
                        NomadEnvironment.GetCurrentTransaction().Begin();
                        NomadEnvironment.GetCurrentTransaction().SaveRefresh(DDO);
                        if (MyDEL != null) NomadEnvironment.GetCurrentTransaction().Delete(MyDEL);
                        NomadEnvironment.GetCurrentTransaction().Commit();

                    }
                    else
                        if (DDO.c_tipo == "SCHEDULER")
                        {
                            NomadLog.Debug("----- SCHEDULER -----");
                            string SCHTxt = DDO.o_text1 + DDO.o_text2 + DDO.o_text3 + DDO.o_text4;


                            //Archivo a Eliminar?
                            NucleusRH.Base.Configuracion.Archivos.HEAD MyDEL = null;
                            if (DDO.oi_file != null && DDO.oi_file != "")
                            {
                                MyDEL = NucleusRH.Base.Configuracion.Archivos.HEAD.Get(DDO.oi_file);
                                DDO.oi_fileNull = true;
                            }

                            //Guardo el Resultado en el STREAM
                            MemoryStream strOut = new MemoryStream();
                            System.IO.StreamWriter strWrt = new System.IO.StreamWriter(strOut, System.Text.Encoding.UTF8);

                            //Genero el HTML
                            string html = PAGINA.Resources.HTML_SCHEDULER;

                            NomadXML xmlProg = new NomadXML(SCHTxt);
                            if (xmlProg.isDocument) xmlProg = xmlProg.FirstChild();

                            //Recorro la programacion
                            NomadLog.Debug("XMLPROG: " + xmlProg.ToString());
                            for (NomadXML cur = xmlProg.FirstChild(); cur != null; cur = cur.Next())
                            {
                                string aux = @"
obj=new Object(); 
obj.Anno=""#ANNO#""; 
obj.Inicio=""#INI#""; 
obj.Fin=""#FIN#""; 
obj.URL=""#URL#""; 
ProgList.push(obj); 

#ADD-PROG#";

                                aux = aux.Replace("#ANNO#", cur.GetAttr("txt_anno") == "*" ? "" : cur.GetAttr("txt_anno"));
                                aux = aux.Replace("#INI#", cur.GetAttr("fec_ini"));
                                aux = aux.Replace("#FIN#", cur.GetAttr("fec_fin"));
                                aux = aux.Replace("#URL#", "../../../GenFiles/" + NomadProxy.GetProxy().AppName + "/PREVIEW/PAGE." + cur.GetAttr("id_page") + ".LANG.html");

                                //Agrego la Definicion
                                html = html.Replace("#ADD-PROG#", aux);
                            }
                            html = html.Replace("#ADD-PROG#", "");


                            //Guardo el HTML
                            strWrt.Write(html);

                            //FLUSH
                            strWrt.Flush();

                            //Subo el Archivo	 
                            strOut.Seek(0, SeekOrigin.Begin);
                            StreamReader strREAD = new StreamReader(strOut);
                            ff = NomadProxy.GetProxy().BINService().PutFile("NucleusRH.Base.Configuracion.Archivos.HEAD", "TABLE.html", strREAD.BaseStream);
                            DDO.oi_file = ff.Id;
                            strREAD.Close();

                            ff.SaveFile(NomadProxy.GetProxy().RunPath + "\\WEB\\GenFiles\\" + NomadProxy.GetProxy().AppName + "\\PREVIEW");

                            //GUARDO el DDO
                            NomadEnvironment.GetCurrentTransaction().Begin();
                            NomadEnvironment.GetCurrentTransaction().SaveRefresh(DDO);
                            if (MyDEL != null) NomadEnvironment.GetCurrentTransaction().Delete(MyDEL);
                            NomadEnvironment.GetCurrentTransaction().Commit();

                        }
                        else
                        {
                            NomadLog.Debug("----- OTHER -----");
                            if (DDO.oi_file != null && DDO.oi_file != "")
                            {
                                ff = NomadProxy.GetProxy().BINService().GetFile("NucleusRH.Base.Configuracion.Archivos.HEAD", DDO.oi_file);
                                ff.SaveFile(NomadProxy.GetProxy().RunPath + "\\WEB\\GenFiles\\" + NomadProxy.GetProxy().AppName + "\\PREVIEW");
                            }

                            NomadEnvironment.GetCurrentTransaction().SaveRefresh(DDO);
                        }

        if (DDO.oi_file != null && DDO.oi_file != "" && ff != null)
        {
            string[] Ext = ff.Name.Split('.');

            if (DDO.c_tipo == "IMAGE")
            {
                string HTML = PAGINA.Resources.HTML_IMAGE;
                HTML = HTML.Replace("#URL#", "NucleusRH.Base.Configuracion.Archivos.HEAD." + DDO.oi_file + "." + Ext[Ext.Length - 1]);
                HTML = HTML.Replace("#BG-COLOR#", DDO.o_text1);
                HTML = HTML.Replace("#H-ALIGN#", DDO.o_text2);
                HTML = HTML.Replace("#V-ALIGN#", DDO.o_text3);

                System.IO.StreamWriter strWrt = new System.IO.StreamWriter(NomadProxy.GetProxy().RunPath + "\\WEB\\GenFiles\\" + NomadProxy.GetProxy().AppName + "\\PREVIEW\\PAGE." + DDO.Id + ".LANG.html", false, System.Text.Encoding.UTF8);
                strWrt.Write(HTML);
                strWrt.Close();
            }
            else
                if (Ext[Ext.Length - 1].ToUpper() == "HTML" || Ext[Ext.Length - 1].ToUpper() == "HTM")
                {
                    System.IO.File.Copy(
                        NomadProxy.GetProxy().RunPath + "\\WEB\\GenFiles\\" + NomadProxy.GetProxy().AppName + "\\PREVIEW\\NucleusRH.Base.Configuracion.Archivos.HEAD." + DDO.oi_file + "." + Ext[Ext.Length - 1],
                        NomadProxy.GetProxy().RunPath + "\\WEB\\GenFiles\\" + NomadProxy.GetProxy().AppName + "\\PREVIEW\\PAGE." + DDO.Id + ".LANG.html",
                        true
                    );
                }
                else
                {
                    System.IO.StreamWriter strWrt = new System.IO.StreamWriter(NomadProxy.GetProxy().RunPath + "\\WEB\\GenFiles\\" + NomadProxy.GetProxy().AppName + "\\PREVIEW\\PAGE." + DDO.Id + ".LANG.html", false, System.Text.Encoding.UTF8);
                    strWrt.Write(PAGINA.Resources.HTML_REDIRECT.Replace("#URL#", "NucleusRH.Base.Configuracion.Archivos.HEAD." + DDO.oi_file + "." + Ext[Ext.Length - 1]));
                    strWrt.Close();
                }
        }


        if (DDO.c_modo_pub == "D")
            Publish(DDO.Id);
    }
    public static void Publish(string ID)
    {
        PAGINA DDO = PAGINA.Get(ID, false);

        //Publico la Pagina Principal
        if (DDO.oi_fileNull)
            throw NomadException.NewMessage("CFG.PAGE.NO-FILE-PUBLISH");


        RPCService objRPC = NomadProxy.GetProxy().RPCService();
        objRPC.AddParam(new RPCParam("ID", "IN", ID));
        objRPC.Execute("Gadget_Publish_" + ID, 1, DateTime.Now, "PORTAL", "SPA", "NucleusRH.Base.Configuracion.Paginas.PAGINA", "InternalPublish");
    }
    public static void InternalPublish(string ID)
    {
        PAGINA DDO = PAGINA.Get(ID, false);
        string FileOut = "";

        //Existe la Carpeta de PORTAL?
        string FILEPATH = NomadProxy.GetProxy().RunPath + "\\WEB\\GenFiles\\" + NomadProxy.GetProxy().AppName + "\\PORTAL";
        if (!System.IO.Directory.Exists(FILEPATH))
            System.IO.Directory.CreateDirectory(FILEPATH);

        //Publico la Pagina Principal
        if (!DDO.oi_fileNull)
            FileOut = NucleusRH.Base.Configuracion.Archivos.HEAD.Publish(DDO.oi_file, "GenFiles/" + NomadProxy.GetProxy().AppName + "/PORTAL");
        else
            throw NomadException.NewMessage("CFG.PAGE.NO-FILE-PUBLISH");

        //Public los Adjuntos
        foreach (ADJUNTO objAdjunto in DDO.ADJUNTOS)
        {
            if (!objAdjunto.oi_fileNull)
                NucleusRH.Base.Configuracion.Archivos.HEAD.Publish(objAdjunto.oi_file, "GenFiles/" + NomadProxy.GetProxy().AppName + "/PORTAL");
        }


        if (DDO.c_tipo == "TABLE")
        {
            //Guardo el Resultado en el STREAM
            System.IO.StreamWriter strWrt = new System.IO.StreamWriter(NomadProxy.GetProxy().RunPath + "\\WEB\\GenFiles\\" + NomadProxy.GetProxy().AppName + "\\PORTAL\\PAGE." + DDO.Id + "." + NomadProxy.GetProxy().Lang + ".html", false, System.Text.Encoding.UTF8);

            //Ejecuta el reporte -------------------------------------------------------------
            strWrt.Write(PAGINA.GenerateTABLE(new NomadXML(DDO.o_text1 + DDO.o_text2 + DDO.o_text3 + DDO.o_text4), "PORTAL", NomadProxy.GetProxy().Lang));

            //Cerrar el archivo
            strWrt.Close();
        }
        else
            if (DDO.c_tipo == "GADGET")
            {
                string PARAMTxt = DDO.o_text1 + DDO.o_text2 + DDO.o_text3 + DDO.o_text4;

                //Guardo el Resultado en el STREAM
                System.IO.StreamWriter strWrt = new System.IO.StreamWriter(NomadProxy.GetProxy().RunPath + "\\WEB\\GenFiles\\" + NomadProxy.GetProxy().AppName + "\\PORTAL\\PAGE." + DDO.Id + "." + NomadProxy.GetProxy().Lang + ".html", false, System.Text.Encoding.UTF8);

                NomadXML Params = null;
                if (PARAMTxt != "")
                {
                    Params = new NomadXML(PARAMTxt);
                    if (Params.isDocument) Params = Params.FirstChild();
                }

                //Genero el TEXTO
                if (DDO.c_modo_pub == "O")
                {
                    string url = "../../../ReportViewer.aspx?lang=" + NomadProxy.GetProxy().Lang + "&report=" + NomadProxy.GetProxy().AppName + "." + DDO.d_estilo;
                    for (NomadXML cur = Params.FirstChild(); cur != null; cur = cur.Next())
                        url += "&" + cur.GetAttr("id") + "=" + cur.GetAttr("value");

                    //Guardo el HTML
                    strWrt.Write(PAGINA.Resources.HTML_REDIRECT.Replace("#URL#", url));
                }
                else
				if (Params!=null)
                {
                    NomadXML PARAM = new NomadXML("PARAMS");
                    for (NomadXML cur = Params.FirstChild(); cur != null; cur = cur.Next())
                        PARAM.SetAttr(cur.GetAttr("id"), cur.GetAttr("value"));

                    //Ejecuta el reporte 
                    Nomad.NomadHTML nmdHTML = new Nomad.NomadHTML(DDO.d_estilo, PARAM);
                    nmdHTML.GenerateHTML(strWrt);
                }

                //Cerrar el archivo
                strWrt.Close();

            }
            else
                if (DDO.c_tipo == "SWITCH")
                {
                    //Guardo el Resultado en el STREAM
                    System.IO.StreamWriter strWrt = new System.IO.StreamWriter(NomadProxy.GetProxy().RunPath + "\\WEB\\GenFiles\\" + NomadProxy.GetProxy().AppName + "\\PORTAL\\PAGE." + DDO.Id + "." + NomadProxy.GetProxy().Lang + ".html", false, System.Text.Encoding.UTF8);

                    switch (NomadProxy.GetProxy().Lang)
                    {
                        case "SPA":
                            //Guardo el HTML
                            strWrt.Write(PAGINA.Resources.HTML_REDIRECT.Replace("#URL#", "PAGE." + DDO.o_text1 + ".LANG.html"));
                            break;

                        case "ENG":
                            //Guardo el HTML
                            strWrt.Write(PAGINA.Resources.HTML_REDIRECT.Replace("#URL#", "PAGE." + DDO.o_text2 + ".LANG.html"));
                            break;

                        case "POR":
                            //Guardo el HTML
                            strWrt.Write(PAGINA.Resources.HTML_REDIRECT.Replace("#URL#", "PAGE." + DDO.o_text3 + ".LANG.html"));
                            break;
                    }

                    //Cerrar el archivo
                    strWrt.Close();

                }
                else
                    if (DDO.c_tipo == "SCHEDULER")
                    {
                        string SCHTxt = DDO.o_text1 + DDO.o_text2 + DDO.o_text3 + DDO.o_text4;

                        //Guardo el Resultado en el STREAM
                        System.IO.StreamWriter strWrt = new System.IO.StreamWriter(NomadProxy.GetProxy().RunPath + "\\WEB\\GenFiles\\" + NomadProxy.GetProxy().AppName + "\\PORTAL\\PAGE." + DDO.Id + "." + NomadProxy.GetProxy().Lang + ".html", false, System.Text.Encoding.UTF8);

                        //Genero el HTML
                        string html = PAGINA.Resources.HTML_SCHEDULER;

                        NomadXML xmlProg = new NomadXML(SCHTxt);
                        if (xmlProg.isDocument) xmlProg = xmlProg.FirstChild();

                        //Recorro la programacion
                        NomadLog.Debug("XMLPROG: " + xmlProg.ToString());
                        for (NomadXML cur = xmlProg.FirstChild(); cur != null; cur = cur.Next())
                        {
                            string aux = @"
obj=new Object(); 
obj.Anno=""#ANNO#""; 
obj.Inicio=""#INI#""; 
obj.Fin=""#FIN#""; 
obj.URL=""#URL#""; 
ProgList.push(obj); 

#ADD-PROG#";

                            aux = aux.Replace("#ANNO#", cur.GetAttr("txt_anno") == "*" ? "" : cur.GetAttr("txt_anno"));
                            aux = aux.Replace("#INI#", cur.GetAttr("fec_ini"));
                            aux = aux.Replace("#FIN#", cur.GetAttr("fec_fin"));
                            aux = aux.Replace("#URL#", "../../../GenFiles/" + NomadProxy.GetProxy().AppName + "/PORTAL/PAGE." + cur.GetAttr("id_page") + ".LANG.html");

                            //Agrego la Definicion
                            html = html.Replace("#ADD-PROG#", aux);
                        }
                        html = html.Replace("#ADD-PROG#", "");


                        //Guardo el HTML
                        strWrt.Write(html);

                        //Cerrar el archivo
                        strWrt.Close();
                    }
                    else
                        if (FileOut != "")
                        {
                            string[] Ext = FileOut.Split('.');

                            if (DDO.c_tipo == "IMAGE")
                            {
                                string HTML = PAGINA.Resources.HTML_IMAGE;
                                HTML = HTML.Replace("#URL#", "NucleusRH.Base.Configuracion.Archivos.HEAD." + DDO.oi_file + "." + Ext[Ext.Length - 1]);
                                HTML = HTML.Replace("#BG-COLOR#", DDO.o_text1);
                                HTML = HTML.Replace("#H-ALIGN#", DDO.o_text2);
                                HTML = HTML.Replace("#V-ALIGN#", DDO.o_text3);

                                System.IO.StreamWriter strWrt = new System.IO.StreamWriter(NomadProxy.GetProxy().RunPath + "\\WEB\\GenFiles\\" + NomadProxy.GetProxy().AppName + "\\PORTAL\\PAGE." + DDO.Id + "." + NomadProxy.GetProxy().Lang + ".html", false, System.Text.Encoding.UTF8);
                                strWrt.Write(HTML);
                                strWrt.Close();
                            }
                            else
                                if (Ext[Ext.Length - 1].ToUpper() == "HTML" || Ext[Ext.Length - 1].ToUpper() == "HTM")
                                {
                                    System.IO.File.Copy(
                                        NomadProxy.GetProxy().RunPath + "\\WEB\\" + FileOut.Replace("/", "\\"),
                                        NomadProxy.GetProxy().RunPath + "\\WEB\\GenFiles\\" + NomadProxy.GetProxy().AppName + "\\PORTAL\\PAGE." + DDO.Id + "." + NomadProxy.GetProxy().Lang + ".html",
                                        true
                                    );
                                }
                                else
                                {
                                    //Guardo el Resultado en el STREAM
                                    System.IO.StreamWriter strWrt = new System.IO.StreamWriter(NomadProxy.GetProxy().RunPath + "\\WEB\\GenFiles\\" + NomadProxy.GetProxy().AppName + "\\PORTAL\\PAGE." + DDO.Id + "." + NomadProxy.GetProxy().Lang + ".html", false, System.Text.Encoding.UTF8);

                                    //Guardo el HTML
                                    strWrt.Write(PAGINA.Resources.HTML_REDIRECT.Replace("#URL#", "NucleusRH.Base.Configuracion.Archivos.HEAD." + DDO.oi_file + "." + Ext[Ext.Length - 1]));

                                    //FLUSH
                                    strWrt.Close();
                                }


                        }


        //Guardo el DDO
        DDO.f_publicado = DateTime.Now;
        NomadEnvironment.GetCurrentTransaction().Save(DDO);

        //Reprogramo el TIMER
        RPCService objRPC;
        switch (NomadProxy.GetProxy().Lang)
        {
            case "SPA":
                objRPC = NomadProxy.GetProxy().RPCService();
                objRPC.AddParam(new RPCParam("ID", "IN", DDO.Id));
                objRPC.Execute("Gadget_Publish_" + DDO.Id + "_ENG", 1, DateTime.Now, "PORTAL", "ENG", "NucleusRH.Base.Configuracion.Paginas.PAGINA", "InternalPublish");
                break;

            case "ENG":
                objRPC = NomadProxy.GetProxy().RPCService();
                objRPC.AddParam(new RPCParam("ID", "IN", DDO.Id));
                objRPC.Execute("Gadget_Publish_" + DDO.Id + "_POR", 1, DateTime.Now, "PORTAL", "POR", "NucleusRH.Base.Configuracion.Paginas.PAGINA", "InternalPublish");
                break;

            case "POR":
                if (DDO.c_modo_pub == "D")
                {
                    DateTime pDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DDO.f_time_pub.Hour, DDO.f_time_pub.Minute, 0);
                    if (pDate < DateTime.Now) pDate = pDate.AddDays(1);

                    objRPC = NomadProxy.GetProxy().RPCService();
                    objRPC.AddParam(new RPCParam("ID", "IN", DDO.Id));
                    objRPC.Execute("Gadget_Publish_" + DDO.Id, 1, pDate, "PORTAL", "SPA", "NucleusRH.Base.Configuracion.Paginas.PAGINA", "InternalPublish");
                }
                break;
        }
    }
}
}



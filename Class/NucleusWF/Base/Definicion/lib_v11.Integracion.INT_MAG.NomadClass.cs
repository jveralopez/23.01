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

namespace NucleusWF.Base.Definicion.Integracion
{

    //////////////////////////////////////////////////////////////////////////////////
    //Clase Interfaces de Integracion
    public partial class INT_MAG : Nomad.NSystem.Base.NomadObject
    {

    public static NomadXML ExecuteIM(string cType, NomadXML xmlIn, DateTime TimeOut)
    {
      NomadProxy proxy=NomadProxy.GetProxy();
      string UUID=Nomad.NSystem.Functions.StringUtil.GenerateUUID();

      if (xmlIn.isDocument) xmlIn=xmlIn.FirstChild();

      string FileNameIn =proxy.RunPath+"TEMP\\"+UUID+".IN.XML";
      string FileNameOut=proxy.RunPath+"TEMP\\"+UUID+".OUT.XML";
      string FileNameEXE=proxy.RunPath+"App\\BIN\\IM."+cType+".EXE";

      //Guardo el XML de entrada
      NomadLog.Info("Guardar el Archivo: "+FileNameIn);
      xmlIn.ToFile(FileNameIn);

      //Verifico si existe el EXE
      if (!System.IO.File.Exists(FileNameEXE))
      {
        NomadException MyEX=NomadException.NewInternalException("INT_MAG.ExecuteIM(string,xml,date).EXE-NOT-FOUND");
        MyEX.SetValue("cType", cType);
        MyEX.SetValue("xmlIn", xmlIn.ToString());
        MyEX.SetValue("TimeOut", TimeOut.ToString());
        MyEX.SetValue("EXE-FILE", FileNameEXE);
        throw MyEX;
      }

      //Genero el PROCESO
      System.Diagnostics.ProcessStartInfo ps = new System.Diagnostics.ProcessStartInfo();
      ps.FileName = FileNameEXE;
      ps.Arguments = "\""+FileNameIn+"\" \""+FileNameOut+"\" ";
      ps.WorkingDirectory = proxy.RunPath+"TEMP";
      ps.CreateNoWindow = true;
      ps.UseShellExecute = false;
      ps.RedirectStandardOutput = false;
      ps.RedirectStandardError = true;

      //Ejecuto el PROCESO
      NomadLog.Info("Iniciar el EXE: "+FileNameEXE);
      System.Diagnostics.Process p=new System.Diagnostics.Process();
      p.StartInfo=ps;
      p.Start();

      //Espero la Finalizacion
      NomadLog.Info("Esperar: "+((TimeSpan)(TimeOut-DateTime.Now)).TotalMilliseconds+"ms");
      if (!p.WaitForExit((int)((TimeSpan)(TimeOut-DateTime.Now)).TotalMilliseconds))
      {
        NomadException MyEX=NomadException.NewInternalException("INT_MAG.ExecuteIM(string,xml,date).TIMEOUT");
        MyEX.SetValue("cType", cType);
        MyEX.SetValue("xmlIn", xmlIn.ToString());
        MyEX.SetValue("TimeOut", TimeOut.ToString());
        MyEX.SetValue("EXE-FILE", FileNameEXE);
        throw MyEX;
      }

      //Analizo el Resultado
      if (p.ExitCode!=0)
      {
        string ErrExtern=p.StandardError.ReadToEnd();
        NomadLog.Error(ErrExtern);

        NomadException MyEX=NomadException.NewInternalException("INT_MAG.ExecuteIM(string,xml,date).EXE-FAILED");
        MyEX.SetValue("cType", cType);
        MyEX.SetValue("xmlIn", xmlIn.ToString());
        MyEX.SetValue("TimeOut", TimeOut.ToString());
        MyEX.SetValue("EXE-FILE", FileNameEXE);
        MyEX.SetValue("ERROR", ErrExtern);
        throw MyEX;
      }

      //Verifico si existe el OUT
      if (!System.IO.File.Exists(FileNameOut))
      {
        NomadException MyEX=NomadException.NewInternalException("INT_MAG.ExecuteIM(string,xml,date).FILE-OUT-NOT-FOUND");
        MyEX.SetValue("cType", cType);
        MyEX.SetValue("xmlIn", xmlIn.ToString());
        MyEX.SetValue("TimeOut", TimeOut.ToString());
        MyEX.SetValue("FILE", FileNameOut);
        throw MyEX;
      }

      //Cargo el Archivo de SALIDA
      NomadLog.Info("Cargar el Archivo: "+FileNameOut);
      StreamReader srFile = File.OpenText(FileNameOut);
      NomadXML retval=new NomadXML(srFile.ReadToEnd());
      srFile.Close();

      return (retval.isDocument?retval.FirstChild():retval);
    }

    public static NomadXML GetExecuteXML(string cIntegrationManager, bool debug)
    {
      NomadProxy proxy=NomadProxy.GetProxy();
      NomadXML xmlDATOS;
      Nomad.NSystem.Proxy.BINFile objBin;
      Nomad.NSystem.NomadStream strOut;
      System.Xml.XmlTextReader xmlreader;

      //Genero el XML de PARAMETROS
    xmlDATOS=proxy.SQLService().GetXML("CLASS.NucleusWF.Base.Definicion.Integracion.INT_MAG.QRY_IM_FILE", "<DATA IM=\""+cIntegrationManager+"\" debug=\""+(debug?"1":"0")+"\"/>");
      NomadXML MD_CMD =xmlDATOS.FindElement("METADATA").FindElement("CMD");

      //Obtengo el ARCHIVO
      objBin = proxy.BINService().GetFile("NucleusWF.Base.Ejecucion.Archivos.HEAD", xmlDATOS.GetAttr("FILE"));

      //Escribo el XML
      strOut = new Nomad.NSystem.NomadStream(objBin.GetBinary());
      xmlreader = new System.Xml.XmlTextReader(strOut);
      xmlreader.XmlResolver = null;
      xmlreader.WhitespaceHandling = System.Xml.WhitespaceHandling.None;

      NomadXML doc = new NomadXML();
      doc.SetText(xmlreader);
      NomadXML infoXML = doc.FirstChild().FindElement("INFO");

      //CMD
      if (infoXML.GetAttr("") !=""  ) MD_CMD.SetAttr("", infoXML.GetAttr(""));
      if (infoXML.FirstChild()!=null) MD_CMD.AddXML(infoXML.FirstChild());
      return xmlDATOS;
    }

    public static NomadXML ExecuteGUIA(string cIntegrationManager, NomadXML xmlIn)
    {
      NomadXML xmlDATOS=null;
      NomadXML xmlRET;

      if (xmlIn.isDocument) xmlIn=xmlIn.FirstChild();

      //Genero el XML de PARAMETROS
      NomadLog.Info("Genero el PROTOTIPO para: "+cIntegrationManager);
      xmlDATOS=GetExecuteXML(cIntegrationManager, xmlIn.GetAttrBool("debug"));

      //Cargo los Parametros
      NomadXML DT_ARG =xmlDATOS.FindElement("DATA"    ).FindElement("ARG");
      NomadXML MD_ARGS=xmlDATOS.FindElement("METADATA").FindElement("ARGS");
      NomadXML MD_OBJS=xmlDATOS.FindElement("METADATA").FindElement("OBJECTS");

      for (NomadXML MD_ARG=MD_ARGS.FirstChild(); MD_ARG!=null; MD_ARG=MD_ARG.Next())
        DT_ARG.SetAttr(MD_ARG.GetAttr("name"), xmlIn.GetAttr(MD_ARG.GetAttr("name")));

      //Ejecuto el Metodo
      NomadLog.Info("Ejecuto la IM: "+xmlDATOS.GetAttr("type"));
      xmlRET=ExecuteIM(xmlDATOS.GetAttr("type"), xmlDATOS, DateTime.Now.AddSeconds(60));
      NomadLog.Info("RESULTADO: "+xmlRET.ToString());

      //Busco los Argumento de Salida
      NomadXML RS_ARGS=xmlRET.FindElement("RESULT");
      if (RS_ARGS==null)
        throw new Exception("FORMATO INCORRECTO DEL RESULTADO, se esperaba RESULT");

      RS_ARGS=RS_ARGS.FindElement("ARG");
      if (RS_ARGS==null)
        throw new Exception("FORMATO INCORRECTO DEL RESULTADO, se esperaba ARG");

      //Genero el XML de Salida
      NomadXML xmlARGRET=MD_ARGS.FindElement2("ARG","io","RET");
      if (xmlARGRET==null)
        throw new Exception("PARAM RET NOT FOUND");

      NomadXML xmlARRRET=MD_OBJS.FindElement2("ARRAY","name",xmlARGRET.GetAttr("type"));
      if (xmlARRRET==null)
        throw new Exception("ARRAY RET NOT FOUND");

      NomadXML xmlRS=RS_ARGS.FindElement(xmlARGRET.GetAttr("name"));
      if (xmlRS==null)
        throw new Exception("PARAM OUT NOT FOUND");

      //Datos
      NomadLog.Info("Genero el Resultado GUIA");
      NomadXML xmlRETVAL=new NomadXML("GUIA");

      //Agrego las Columnas
      NomadLog.Info("Agrego la Columnas");
      NomadXML xmlCOLS=xmlRETVAL.AddTailElement("COLUMNS");
      for (NomadXML cur=xmlARRRET.FirstChild(); cur!=null; cur=cur.Next())
      {
        NomadXML xmlCOL=xmlCOLS.AddTailElement("COLUMN");
        xmlCOL.SetAttr("name" , cur.GetAttr("name" ));
        xmlCOL.SetAttr("label", cur.GetAttr("label"));
        xmlCOL.SetAttr("type" , cur.GetAttr("type" ));
        xmlCOL.SetAttr("show" , (cur.GetAttr("name")=="DES" || cur.GetAttrInt("orden")!=0));
      }

      //Agrego los Rows
      NomadLog.Info("Agrego los Rows");
      NomadXML xmlROWS=xmlRETVAL.AddTailElement("ROWS");
      for (NomadXML cur=xmlRS.FirstChild(); cur!=null; cur=cur.Next())
      {
        NomadXML xmlROW=xmlROWS.AddTailElement("ROW");

        for (NomadXML cur2=xmlARRRET.FirstChild(); cur2!=null; cur2=cur2.Next())
          xmlROW.SetAttr(cur2.GetAttr("name"), cur.GetAttr(cur2.GetAttr("name")));
      }

      return xmlRETVAL;
    }

    public static NomadXML GetVariables(string cIntegrationManager)
    {
      //Genero el XML de PARAMETROS
      NomadLog.Info("Genero el PROTOTIPO para: "+cIntegrationManager);
      NomadXML xmlDATOS=GetExecuteXML(cIntegrationManager, false);

      //Cargo los Parametros
      NomadXML DT_ARG =xmlDATOS.FindElement("DATA"    ).FindElement("ARG");
      NomadXML MD_ARGS=xmlDATOS.FindElement("METADATA").FindElement("ARGS");
      NomadXML MD_OBJS=xmlDATOS.FindElement("METADATA").FindElement("OBJECTS");

      //Genero los PARAMETROS del METODO
      NomadXML xmlParams=new NomadXML("PARAMS");
      Nomad.Base.Report.Variables.AddGroupDef(xmlParams, "", "simple");

      //Creo el XMLParams (PROTOTIPO)
      for (NomadXML MD_ARG=MD_ARGS.FirstChild(); MD_ARG!=null; MD_ARG=MD_ARG.Next())
      {
        switch(MD_ARG.GetAttr("mul"))
        {
          case "0":
            Nomad.Base.Report.Variables.AddVariableDef(xmlParams, MD_ARG.GetAttr("name"), MD_ARG.GetAttr("type"), null);
            break;

          case "1":
            Nomad.Base.Report.Variables.AddGroupDef(xmlParams, MD_ARG.GetAttr("name"), "simple");
            for (NomadXML MD_STR=MD_OBJS.FindElement2("STRUCT","name",MD_ARG.GetAttr("type")).FirstChild(); MD_STR!=null; MD_STR=MD_STR.Next())
              Nomad.Base.Report.Variables.AddVariableDef(xmlParams, MD_ARG.GetAttr("name")+"."+MD_STR.GetAttr("name"), MD_STR.GetAttr("type"), null);
            break;

          case "*":
            Nomad.Base.Report.Variables.AddGroupDef(xmlParams, MD_ARG.GetAttr("name"), "coleccion");
            for (NomadXML MD_STR=MD_OBJS.FindElement2("ARRAY","name",MD_ARG.GetAttr("type")).FirstChild(); MD_STR!=null; MD_STR=MD_STR.Next())
              Nomad.Base.Report.Variables.AddVariableDef(xmlParams, MD_ARG.GetAttr("name")+"."+MD_STR.GetAttr("name"), MD_STR.GetAttr("type"), null);
            break;
        }
      }

      return xmlParams;
    }

    public static void ExecuteWF(string cIntegrationManager, string ProcessBefore, string ProcessAfter, NomadXML xmlVars)
    {
      string result;
      string paso="Iniciando";
      NomadXML xmlParams=null, xmlDATOS=null, xmlDATA, DT_ARG,MD_ARGS,MD_OBJS;
      if (xmlVars.isDocument) xmlVars=xmlVars.FirstChild();

      try
      {

        //Genero el XML de PARAMETROS
        paso="Genero el Prototipo";
        NomadLog.Info("Genero el PROTOTIPO para: "+cIntegrationManager);
        xmlDATOS=GetExecuteXML(cIntegrationManager, xmlVars.GetAttrBool("debug"));

        //Cargo los Parametros
        paso="Cargo los Parametros";
        DT_ARG =xmlDATOS.FindElement("DATA"    ).FindElement("ARG");
        MD_ARGS=xmlDATOS.FindElement("METADATA").FindElement("ARGS");
        MD_OBJS=xmlDATOS.FindElement("METADATA").FindElement("OBJECTS");

        //Genero los PARAMETROS del METODO
        xmlParams=new NomadXML("PARAMS");
        Nomad.Base.Report.Variables.AddGroupDef(xmlParams, "", "simple");

        //Creo el XMLParams (PROTOTIPO)
        for (NomadXML MD_ARG=MD_ARGS.FirstChild(); MD_ARG!=null; MD_ARG=MD_ARG.Next())
        {
          paso="Cargo los Parametros: "+MD_ARG.GetAttr("name");
          switch(MD_ARG.GetAttr("mul"))
          {
            case "0":
              Nomad.Base.Report.Variables.AddVariableDef(xmlParams, MD_ARG.GetAttr("name"), MD_ARG.GetAttr("type"), null);
              break;

            case "1":
              Nomad.Base.Report.Variables.AddGroupDef(xmlParams, MD_ARG.GetAttr("name"), "simple");
              for (NomadXML MD_STR=MD_OBJS.FindElement2("STRUCT","name",MD_ARG.GetAttr("type")).FirstChild(); MD_STR!=null; MD_STR=MD_STR.Next())
                Nomad.Base.Report.Variables.AddVariableDef(xmlParams, MD_ARG.GetAttr("name")+"."+MD_STR.GetAttr("name"), MD_STR.GetAttr("type"), null);
              break;

            case "*":
              Nomad.Base.Report.Variables.AddGroupDef(xmlParams, MD_ARG.GetAttr("name"), "coleccion");
              for (NomadXML MD_STR=MD_OBJS.FindElement2("ARRAY","name",MD_ARG.GetAttr("type")).FirstChild(); MD_STR!=null; MD_STR=MD_STR.Next())
                Nomad.Base.Report.Variables.AddVariableDef(xmlParams, MD_ARG.GetAttr("name")+"."+MD_STR.GetAttr("name"), MD_STR.GetAttr("type"), null);
              break;
          }
        }
        NomadLog.Info("PARAMS-PROTO: "+xmlParams.ToString());

        //Ejecuto el Proceso XMLVars -> XMLParams
        if (ProcessBefore!="")
        {
          paso="Ejecuto el Proceso PB";
          NomadLog.Info("EXECUTE-BEFORE: "+ProcessBefore);
          result=Nomad.Base.Report.Formula.Resolve(ProcessBefore, xmlParams, xmlVars, false);
          NomadLog.Info("RESULT: "+result);

          if (result.Substring(0,1)=="E")
            throw new Exception("Error en el Procedimiento, "+result.Substring(2));
          NomadLog.Info("PARAMS-IM: "+xmlParams.ToString());

          //Paso XMLParams -> IM
          xmlDATA=xmlParams.FindElement("DATA");
          for (NomadXML MD_ARG=MD_ARGS.FirstChild(); MD_ARG!=null; MD_ARG=MD_ARG.Next())
          {
            string name=MD_ARG.GetAttr("name");

            paso="Ejecuto el Proceso PB, asigno: "+name;
            switch(MD_ARG.GetAttr("mul"))
            {
              case "0":
                DT_ARG.SetAttr(name,xmlDATA.GetAttr(name));
                break;

              case "1":
              case "*":
                if (DT_ARG.FindElement(name)!=null)
                  DT_ARG.DeleteChild(DT_ARG.FindElement(name));
                DT_ARG.AddXML(xmlDATA.FindElement(name));
                break;
            }
          }
          NomadLog.Info("DATOS-IM: "+xmlParams.ToString());
        }

        //Ejecuto el Metodo
        paso="Ejecuto el Metodo Externo IM";
        NomadLog.Info("Ejecuto la IM: "+xmlDATOS.GetAttr("type"));
        NomadXML xmlRET=ExecuteIM(xmlDATOS.GetAttr("type"), xmlDATOS, DateTime.Now.AddMinutes(10));
        NomadXML xmlRETARG=xmlRET.FindElement("RESULT").FindElement("ARG");
        NomadLog.Info("RESULT-IM: "+xmlRET.ToString());

        //Ejecuto el Proceso XMLParams -> XMLVars
        if (ProcessAfter!="")
        {
          paso="Ejecuto el Proceso PA";
          //Paso IM -> XMLParams
          xmlDATA=xmlParams.FindElement("DATA");
          for (NomadXML MD_ARG=MD_ARGS.FirstChild(); MD_ARG!=null; MD_ARG=MD_ARG.Next())
          {
            string name=MD_ARG.GetAttr("name");
            if (MD_ARG.GetAttr("io")=="IN") continue;

            paso="Ejecuto el Proceso PA, asigno: "+name;
            switch(MD_ARG.GetAttr("mul"))
            {
              case "0":
                xmlDATA.SetAttr(name, xmlRETARG.GetAttr(name));
                break;

              case "1":
              case "*":
                if (xmlDATA.FindElement(name)!=null)
                  xmlDATA.DeleteChild(xmlDATA.FindElement(name));

                if (xmlRETARG.FindElement(name)!=null)
                  xmlDATA.AddXML(xmlRETARG.FindElement(name));
                else
                  xmlDATA.AddTailElement(name);
                break;
            }
          }
          NomadLog.Info("PARAMS-RESULT: "+xmlParams.ToString());

          NomadLog.Info("EXECUTE-AFTER: "+ProcessAfter);
          result=Nomad.Base.Report.Formula.Resolve(ProcessAfter, xmlParams, xmlVars, false);
          NomadLog.Info("RESULT: "+result);

          if (result.Substring(0,1)=="E")
            throw new Exception("Error en el Procedimiento, "+result.Substring(2));
          NomadLog.Info("PARAMS-FINAL: "+xmlParams.ToString());
        }

      } catch(Exception Err)
      {
        NomadException MyEX=NomadException.NewInternalException("INT_MAG.ExecuteWF(string,string,string,NomadXML).CATCH",Err);
        MyEX.SetValue("IM", cIntegrationManager);
        MyEX.SetValue("PB", ProcessBefore);
        MyEX.SetValue("PA", ProcessAfter);
        MyEX.SetValue("XML", xmlVars.ToString());
        MyEX.SetValue("PASO", paso);
        if (xmlParams!=null) MyEX.SetValue("xmlParams", xmlParams.ToString());
        if (xmlDATOS!=null) MyEX.SetValue("xmlDATOS", xmlDATOS.ToString());

        throw MyEX;
      }
    }

    public static void Exportar(string ID, NomadXML result)
    {
      //Obtengo el OBJETO
      INT_MAG objExp=INT_MAG.Get(ID,false);
      Nomad.NSystem.Proxy.BINFile objBin;
      Nomad.NSystem.NomadStream strOut;
      System.Xml.XmlTextReader xmlreader;

      //Agrego el ITEM a la SELECCION ACTUAL
      NomadXML newElement=result.AddTailElement("ITEM");
      newElement.SetAttr("TYPE"   , "INT");
      newElement.SetAttr("ID"     , ID);
      newElement.SetAttr("CODE"   , objExp.c_int_mag);
      newElement.SetAttr("LABEL"  , "["+objExp.c_int_mag+"]"+objExp.d_int_mag);
      newElement.SetAttr("VERSION", "N/A");

      //Obtengo el XML de datos
      objBin = NomadProxy.GetProxy().BINService().GetFile("NucleusWF.Base.Ejecucion.Archivos.HEAD", objExp.oi_file_wrk);

      //Escribo el XML
      strOut = new Nomad.NSystem.NomadStream(objBin.GetBinary());
      xmlreader = new System.Xml.XmlTextReader(strOut);
      xmlreader.XmlResolver = null;
      xmlreader.WhitespaceHandling = System.Xml.WhitespaceHandling.None;

      NomadXML doc = new NomadXML();
      doc.SetText(xmlreader);

      //Agrego el XML
      newElement.AddXML(doc.FirstChild());
    }

    public static void Importar(string itemID)
    {
      //Elementos a ELIMINAR
      INT_MAG delOBJ=null;
      NucleusWF.Base.Ejecucion.Archivos.HEAD delFIL=null;

      //Obtengo el XML
      NomadXML objXML=NomadProxy.GetProxy().FileServiceIO().LoadFileXML("TEMP","wfimportfile.txt");
      if (objXML.isDocument) objXML=objXML.FirstChild();

      //Busco el ITEM
      NomadXML objITEM=null;
      for(NomadXML cur=objXML.FirstChild(); cur!=null && objITEM==null; cur=cur.Next())
        if (cur.GetAttr("TYPE")+":"+cur.GetAttr("ID")==itemID)
          objITEM=cur;
      if (objITEM==null)
        throw new Exception("ITEM 'ID="+itemID+"' no encontrado.");

      //Existe?, lo Elimino.
      string oldID=NomadEnvironment.QueryValue("WRK04_INT_MAN","oi_int_mag","c_int_mag",objITEM.GetAttr("CODE"),"",false);
      if (oldID!=null && oldID!="")
      {
        delOBJ=INT_MAG.Get(oldID,true);
        delFIL=delOBJ.Getoi_file_wrk();

        NomadEnvironment.GetCurrentTransaction().Begin();
        NomadEnvironment.GetCurrentTransaction().Delete(delOBJ);
        NomadEnvironment.GetCurrentTransaction().Delete(delFIL);
        NomadEnvironment.GetCurrentTransaction().Commit();
      }

      //Guardo el Resultado en el STREAM
      MemoryStream strOut=new MemoryStream();
      System.Xml.XmlTextWriter xmlWrt=new System.Xml.XmlTextWriter(strOut,System.Text.Encoding.UTF8);
      xmlWrt.Formatting=System.Xml.Formatting.None;
      objITEM.FirstChild().ToString(xmlWrt);
      xmlWrt.Flush();

      //Subo el Archivo
      strOut.Seek(0,SeekOrigin.Begin);
      StreamReader strREAD=new StreamReader(strOut);
      Nomad.NSystem.Proxy.BINFile MyFILE=NomadProxy.GetProxy().BINService().PutFile("NucleusWF.Base.Ejecucion.Archivos.HEAD","wfimport.txt",strREAD.BaseStream);
      strREAD.Close();
      xmlWrt.Close();

      //Nuevo Objeto
      INT_MAG newOBJ=new INT_MAG();
      newOBJ.oi_file_wrk=MyFILE.Id;
      Crear(newOBJ);
    }

    public static void Crear(NucleusWF.Base.Definicion.Integracion.INT_MAG DDO)
    {
        Nomad.NSystem.Proxy.NomadXML row;
        Nomad.NSystem.Proxy.NomadXML row2;
        int nroError = 1;
        int intErrors = 0;
        string strValidFields = "";

        NomadXML PARAMS;
        NomadXML STRUCTS;
        //Recupero el archivo con el oi_file_wrk
        Nomad.NSystem.Proxy.BINFile objBin;
        NomadProxy pProxy = Nomad.NSystem.Base.NomadEnvironment.GetProxy();
        Nomad.NSystem.NomadStream strOut;

        //PIDO EL OBJETO ARCHIVO DE LA IMAGEN Y LA GUARDO CON EL NOMBRE GENERADO
        objBin = pProxy.BINService().GetFile("NucleusWF.Base.Ejecucion.Archivos.HEAD", DDO.oi_file_wrk);

        //Escribo el XML
        strOut = new Nomad.NSystem.NomadStream(objBin.GetBinary());
        System.Xml.XmlTextReader xmlreader = new System.Xml.XmlTextReader(strOut);
        xmlreader.XmlResolver = null;
        xmlreader.WhitespaceHandling = System.Xml.WhitespaceHandling.None;

        NomadXML doc = new NomadXML();
        doc.SetText(xmlreader);
        NomadXML intXML = doc.FirstChild();

        /////////////////////////////////////////////////////////////////////////////////////////////
        NomadLog.Debug("archivo-- " + intXML.ToString());

        /////////////////////////////////////////////////////////////////////////////////////////////
        // ANALISIS GENERAL DE PARAMETROS
        NomadLog.Info("--Validaciones Generales---");
        int param_ret = 0;
        int param_in = 0;
        int param_out = 0;
        int param_io = 0;
        string ret_type = "";

        for (row = intXML.FindElement("PARAMS").FirstChild(); row != null; row = row.Next())
        {
            switch (row.GetAttr("c_io").ToUpper())
            {
                case "IN": param_in++; break;
                case "OUT":
                    param_out++;
                    if (row.GetAttr("c_param_x") != "")
                        throw new Exception("El parametro '" + row.GetAttr("c_param") + "' tiene un modo '" + row.GetAttr("c_io") + "' y tiene un parametro Asociado.");
                    break;
                case "IO": param_io++;
                    if (row.GetAttr("c_param_x") != "")
                        throw new Exception("El parametro '" + row.GetAttr("c_param") + "' tiene un modo '" + row.GetAttr("c_io") + "' y tiene un parametro Asociado.");
                    break;
                case "RET":
                    param_ret++; ret_type = row.GetAttr("c_type").ToUpper();
                    if (row.GetAttr("c_param_x") != "")
                        throw new Exception("El parametro '" + row.GetAttr("c_param") + "' tiene un modo '" + row.GetAttr("c_io") + "' y tiene un parametro Asociado.");
                    break;
                default:
                    throw new Exception("El parametro '" + row.GetAttr("c_param") + "' tiene un modo '" + row.GetAttr("c_io") + "' no reconocido.");
            }

            switch (row.GetAttr("c_type").ToUpper())
            {
                case "BOOL":
                case "INT":
                case "DOUBLE":
                case "DATETIME":
                case "STRING":
                    break;

                case "STRUCT":
                case "ARRAY":
                    if (row.GetAttr("d_class") == "")
                        throw new Exception("El parametro '" + row.GetAttr("c_param") + "' tiene no tiene especifidado el 'd_class'.");

                    if (row.FirstChild() == null || row.FirstChild().FirstChild() == null)
                        throw new Exception("El parametro '" + row.GetAttr("c_param") + "' tiene no tiene especifidado los Campos Adicionales.");

                    for (row2 = row.FirstChild().FirstChild(); row2 != null; row2 = row2.Next())
                    {
                        switch (row2.GetAttr("c_type").ToUpper())
                        {
                            case "BOOL":
                            case "INT":
                            case "DOUBLE":
                            case "DATETIME":
                            case "STRING":
                                break;

                            default:
                                throw new Exception("El parametro '" + row.GetAttr("c_param") + "." + row2.GetAttr("c_struct") + "' tiene un tipo '" + row2.GetAttr("c_type") + "' no reconocido.");
                        }
                    }
                    break;

                default:
                    throw new Exception("El parametro '" + row.GetAttr("c_param") + "' tiene un tipo '" + row.GetAttr("c_type") + "' no reconocido.");
            }
        }
        if (param_ret > 1)
            throw new Exception("Puede haber solo un parámetro de modo RET.");

        /////////////////////////////////////////////////////////////////////////////////////////////
        // ANALISIS GENERALES
        switch (intXML.GetAttr("c_type").ToUpper())
        {
            case "DB":
                if (param_out != 0)
                    throw new Exception("El Tipo de Interface de Integración '" + intXML.GetAttr("c_type") + "' no Permite Valores modo OUT.");

                if (param_ret != 0 && (ret_type != "ARRAY" && ret_type != "STRUCT"))
                    throw new Exception("El Tipo de Interface de Integración '" + intXML.GetAttr("c_type") + "' Permite Valores modo RET de tipo ARRAY o STRUCT.");

                break;

            case "SOAP":
                break;

            case "JSON":
                break;

            default:
                throw new Exception("El Tipo de Interface de Integración '" + intXML.GetAttr("c_type") + "' no esta Implementado.");
        }

        switch (intXML.GetAttr("c_list_mode").ToUpper())
        {
            case "NO":
                break;

            case "SIMPLE":
                if (param_ret == 0)
                    throw new Exception("El modo lista SIMPLE requiere un parametro RET.");
                if (ret_type != "ARRAY")
                    throw new Exception("El modo lista SIMPLE requiere un parametro RET de tipo ARRAY.");
                break;

            default:
                throw new Exception("El modo lista de la Interface de Integración no es correcto.");
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //////////////////////////////***********   Creo los parámetros   *****************///////////////////////////////
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        PARAMS = intXML.FindElement("PARAMETROS");
        string param;
        NomadXML docParametros;
        NomadLog.Info("--Creo los Parametros---");
        for (row = PARAMS.FirstChild(); row != null; row = row.Next())
        {
            param = "<DATOS c_param=\"" + row.GetAttr("c_param") + "\" />";
            docParametros = NomadEnvironment.QueryNomadXML("CLASS.NucleusWF.Base.Definicion.Integracion.INT_MAG.QRY_Parametro", param).FirstChild();

            NomadEnvironment.GetTrace().Info("docParametros: " + docParametros.ToString());
            if (docParametros.GetAttr("oi_param") == "0")
            {
                NucleusWF.Base.Definicion.Parametros.PARAM PARAM = new NucleusWF.Base.Definicion.Parametros.PARAM();
                PARAM.c_param = row.GetAttr("c_param");
                PARAM.d_param = row.GetAttr("d_param");
                PARAM.c_type = row.GetAttr("c_type");
                PARAM.d_value = row.GetAttr("d_value");
                NomadEnvironment.GetCurrentTransaction().SaveRefresh(PARAM);
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //////////////////////////////***********   Creo la Interface   *****************///////////////////////////////
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        DDO.c_int_mag = intXML.GetAttr("c_int_mag");
        DDO.d_int_mag = intXML.GetAttr("d_int_mag");
        DDO.c_type = intXML.GetAttr("c_type");
        DDO.c_list_mode = intXML.GetAttr("c_list_mode").ToUpper();

        PARAMS = intXML.FindElement("PARAMS");
        NomadLog.Info("--Creo la Inteface---");
        for (row = PARAMS.FirstChild(); row != null; row = row.Next())
        {
            NucleusWF.Base.Definicion.Integracion.PARAM PARAM = new NucleusWF.Base.Definicion.Integracion.PARAM();
            PARAM.c_param = row.GetAttr("c_param");
            PARAM.d_param = row.GetAttr("d_param");
            PARAM.c_type = row.GetAttr("c_type");
            PARAM.c_io = row.GetAttr("c_io");
            PARAM.d_extend = row.GetAttr("d_extend");
            PARAM.d_class = row.GetAttr("d_class");

            if (row.GetAttr("c_param_x") != "" && row.GetAttr("c_io") == "IN")
            {
                param = "<DATOS c_param=\"" + row.GetAttr("c_param_x") + "\" />";
                docParametros = NomadEnvironment.QueryNomadXML("CLASS.NucleusWF.Base.Definicion.Integracion.INT_MAG.QRY_Parametro", param).FirstChild();

                if (docParametros.GetAttr("oi_param") != "0")
                {
                    NucleusWF.Base.Definicion.Parametros.PARAM objPARAM = NucleusWF.Base.Definicion.Parametros.PARAM.Get(docParametros.GetAttr("oi_param"));
                    PARAM.oi_param_x = objPARAM.Id;
                }
                else
                {
                    strValidFields = strValidFields + "<ERROR id=\"" + nroError + "\" desc=\"El parámetro " + row.GetAttrString("c_param_x") + " no existe.\" />";
                    intErrors++;
                    nroError++;
                }

            }
            if (row.GetAttr("c_type") == "array" || row.GetAttr("c_type") == "struct")
            {
                STRUCTS = row.FirstChild();
                for (row2 = STRUCTS.FirstChild(); row2 != null; row2 = row2.Next())
                {
                    NucleusWF.Base.Definicion.Integracion.STRUCT STRUCT = new NucleusWF.Base.Definicion.Integracion.STRUCT();
                    STRUCT.c_struct = row2.GetAttr("c_struct");
                    STRUCT.d_struct = row2.GetAttr("d_struct");
                    STRUCT.c_type = row2.GetAttr("c_type");
                    STRUCT.d_extend = row2.GetAttr("d_extend");
          STRUCT.e_orden = row2.GetAttrInt("e_orden");
                    PARAM.PARAM_STRUCT.Add(STRUCT);
                }
            }
            DDO.PARAMS.Add(PARAM);
        }
        NomadEnvironment.GetTrace().Info("::13:: ");
        NomadEnvironment.GetCurrentTransaction().Save(DDO);
    }
}
}



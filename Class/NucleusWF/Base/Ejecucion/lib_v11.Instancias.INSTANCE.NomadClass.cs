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

namespace NucleusWF.Base.Ejecucion.Instancias
{

    //////////////////////////////////////////////////////////////////////////////////
    //Clase Instancias de Workflow
  public partial class INSTANCE:Nomad.NSystem.Base.NomadObject
  {

    public static NomadXML GetParametros(INSTANCE ins)
    {
      NomadXML xmlVars, MD, DT, GR, VR;

      xmlVars=new NomadXML("PROCESS");
      MD=xmlVars.AddTailElement("METADATA");
      DT=xmlVars.AddTailElement("DATA");

      GR=MD.AddTailElement("GROUP");
      GR.SetAttr("type", "simple");

      foreach (PARAM par in ins.PARAMS)
      {
        VR=GR.AddTailElement("VAR");
        VR.SetAttr("type", par.c_type);
        VR.SetAttr("name", par.c_param);

        DT.SetAttr(par.c_param, par.d_valor);
      }

      //fin
      return xmlVars;
    }

    public static void SetParametros(INSTANCE ins, NomadXML xmlVars)
    {
      xmlVars=xmlVars.FindElement("DATA");
      foreach (PARAM par in ins.PARAMS)
        par.d_valor=xmlVars.GetAttr(par.c_param);
    }

    public static NucleusWF.Base.Ejecucion.Instancias.LOG AddLog(NucleusWF.Base.Ejecucion.Instancias.INSTANCE theINST, NucleusWF.Base.Ejecucion.Instancias.THREAD theTHREAD, string typ, string txt)
    {
      NucleusWF.Base.Ejecucion.Instancias.LOG newLOG=new NucleusWF.Base.Ejecucion.Instancias.LOG();
      newLOG.e_paso      =theINST.e_pasos;
      newLOG.f_accion    =System.DateTime.Now;
      newLOG.c_tipo      =typ;
      newLOG.o_comentario=txt;
      if (theTHREAD==null) newLOG.oi_nodeNull=true;
      else newLOG.oi_node    =theTHREAD.oi_node;

      //Agrego el LOG
      theINST.LOGS.Add(newLOG);

      //Problemas!!!
      return newLOG;
    }

    public static void AddEtapaLog(NucleusWF.Base.Ejecucion.Instancias.INSTANCE theINST, NucleusWF.Base.Ejecucion.Instancias.THREAD theTHREAD, string typ, string txt, string EttyID)
    {
      if (EttyID==null || EttyID=="")
        EttyID=NomadProxy.GetProxy().UserEtty;

      //Obtengo la ENTIDAD, caso DEBUG!
      NomadXML objETTY=NomadProxy.GetProxy().DDOService().GetXML("Nomad.Base.Login.Entidades.ENTIDAD", EttyID);
      if (objETTY.isDocument) objETTY=objETTY.FirstChild();
      if (objETTY.Name.ToUpper()=="OBJECT") objETTY=objETTY.FirstChild();

      NucleusWF.Base.Ejecucion.Instancias.LOG newLOG=new NucleusWF.Base.Ejecucion.Instancias.LOG();
      newLOG.e_paso      =theINST.e_pasos;
      newLOG.f_accion    =System.DateTime.Now;
      newLOG.c_tipo      =typ;
      newLOG.o_comentario=txt;
      if (theTHREAD==null) newLOG.oi_nodeNull=true;
      else newLOG.oi_node    =theTHREAD.oi_node;

      NucleusWF.Base.Ejecucion.Instancias.ETAPA newETAPA=new NucleusWF.Base.Ejecucion.Instancias.ETAPA();
      newETAPA.d_entidad    =objETTY.GetAttr("id");
      newETAPA.d_entidad_ayn=objETTY.GetAttr("DES");
      if (theTHREAD==null)
      {
        newETAPA.f_inicio     =DateTime.Now;
        newETAPA.f_resolucion =DateTime.Now;
      } else
      {
        newETAPA.f_inicio     =theTHREAD.f_ejecucion;
        newETAPA.f_resolucion =DateTime.Now;
      }

      //Agrego la ETAPA
      newLOG.ETAPA.Add(newETAPA);

      //Agrego el LOG
      theINST.LOGS.Add(newLOG);
    }

    public static void SetInternalError(NucleusWF.Base.Ejecucion.Instancias.INSTANCE theINST, NucleusWF.Base.Ejecucion.Instancias.THREAD theTHREAD, Exception Err)
    {
      NomadException MyEX=NomadException.NewInternalException("WF-ERROR", Err);
      MyEX.SetValue("DEBUG", theINST.l_debug.ToString());
      MyEX.SetValue("INST-ID", theINST.Id);
      MyEX.SetValue("INST-DES", theINST.d_instance);
      MyEX.SetValue("INST-OWNER", theINST.d_owner_ayn);
      MyEX.SetValue("INST-EST1", theINST.c_estado_wf);
      MyEX.SetValue("INST-EST2", theINST.c_estado_ej);
      MyEX.Dump();
      NomadLog.Error(Err.Message+" - "+MyEX.Id);

      theINST.c_estado_ej="E";
      theTHREAD.c_estado="E";
      foreach (NucleusWF.Base.Ejecucion.Instancias.LOG theLOG in theINST.LOGS)
      {
        if (theLOG.c_tipo=="EXE" && theLOG.oi_node==theTHREAD.oi_node)
        {
          theLOG.e_paso      =theINST.e_pasos;
          theLOG.f_accion    =System.DateTime.Now;
          theLOG.c_tipo      ="INT";
          theLOG.o_comentario=MyEX.Id;
          theLOG.oi_node     =theTHREAD.oi_node;
          if (!theTHREAD.oi_inst_childNull)
            theLOG.oi_inst_child=theTHREAD.oi_inst_child;
          return;
        }
      }
      AddLog(theINST, theTHREAD, "INT", MyEX.Id);
    }
    public static void SetError(NucleusWF.Base.Ejecucion.Instancias.INSTANCE theINST, NucleusWF.Base.Ejecucion.Instancias.THREAD theTHREAD, string txt)
    {
      NomadLog.Error(txt);

      theINST.c_estado_ej="E";
      if (theTHREAD!=null)
      {
        theTHREAD.c_estado="E";
        foreach (NucleusWF.Base.Ejecucion.Instancias.LOG theLOG in theINST.LOGS)
        {
          if (theLOG.c_tipo=="EXE" && theLOG.oi_node==theTHREAD.oi_node)
          {
            theLOG.e_paso      =theINST.e_pasos;
            theLOG.f_accion    =System.DateTime.Now;
            theLOG.c_tipo      ="ERR";
            theLOG.o_comentario=txt;
            theLOG.oi_node     =theTHREAD.oi_node;
            if (!theTHREAD.oi_inst_childNull)
              theLOG.oi_inst_child=theTHREAD.oi_inst_child;
            return;
          }
        }
      }
      AddLog(theINST, theTHREAD, "ERR", txt);
    }
    public static void SetWarn(NucleusWF.Base.Ejecucion.Instancias.INSTANCE theINST, NucleusWF.Base.Ejecucion.Instancias.THREAD theTHREAD, string txt)
    {
      NomadLog.Info(txt);
      foreach (NucleusWF.Base.Ejecucion.Instancias.LOG theLOG in theINST.LOGS)
      {
        if (theLOG.c_tipo=="EXE" && theLOG.oi_node==theTHREAD.oi_node)
        {
          theLOG.e_paso      =theINST.e_pasos;
          theLOG.f_accion    =System.DateTime.Now;
          theLOG.c_tipo      ="WRN";
          theLOG.o_comentario=txt;
          theLOG.oi_node     =theTHREAD.oi_node;
          if (!theTHREAD.oi_inst_childNull)
            theLOG.oi_inst_child=theTHREAD.oi_inst_child;
          return;
        }
      }
      AddLog(theINST, theTHREAD, "WRN", txt);
    }
    public static void SetInfo(NucleusWF.Base.Ejecucion.Instancias.INSTANCE theINST, NucleusWF.Base.Ejecucion.Instancias.THREAD theTHREAD, string txt)
    {
      NomadLog.Info(txt);
      foreach (NucleusWF.Base.Ejecucion.Instancias.LOG theLOG in theINST.LOGS)
      {
        if (theLOG.c_tipo=="EXE" && theLOG.oi_node==theTHREAD.oi_node)
        {
          theLOG.e_paso       =theINST.e_pasos;
          theLOG.f_accion     =System.DateTime.Now;
          theLOG.c_tipo       ="IFO";
          theLOG.o_comentario =txt;
          theLOG.oi_node      =theTHREAD.oi_node;
          if (!theTHREAD.oi_inst_childNull)
            theLOG.oi_inst_child=theTHREAD.oi_inst_child;
          return;
        }
      }
      AddLog(theINST, theTHREAD, "IFO", txt);
    }

    public static void AppendLog(NucleusWF.Base.Ejecucion.Instancias.INSTANCE theINST, NucleusWF.Base.Ejecucion.Instancias.THREAD theTHREAD, string txt)
    {
      NomadLog.Info(txt);
      AddLog(theINST, theTHREAD, "EXE", txt);
    }

    static public string CREAR(string oi_wf, string c_rol, int Debug, string EttyID, NomadXML Params)
    {
      return CREAR(oi_wf, "", c_rol, Debug, EttyID, Params);
    }

    static public string CREAR(string oi_wf, string oi_process, string c_rol, int Debug, string EttyID, NomadXML Params)
    {
      if (EttyID==null || EttyID=="")
        EttyID=NomadProxy.GetProxy().UserEtty;

      if (Params.isDocument)
        Params=Params.FirstChild();

      //Obtengo la ENTIDAD, caso DEBUG!
      NomadXML objETTY=NomadProxy.GetProxy().DDOService().GetXML("Nomad.Base.Login.Entidades.ENTIDAD", EttyID);
      if (objETTY.isDocument) objETTY=objETTY.FirstChild();
      if (objETTY.Name.ToUpper()=="OBJECT") objETTY=objETTY.FirstChild();

      //Obtengo la Definicion del WF
      NucleusWF.Base.Definicion.Workflows.WF defWF=INSTANCE.GetWFDef(oi_wf);
      if (!defWF.l_automatica && Debug==0)
      {
        NomadLog.Error("No se puede iniciar un Workflow NO Publicado en MODO RELEASE.");
        throw new NomadMessage("NucleusWF.EJECUCION.NO-NEW-INSTANCE");
      }

      if (Debug==0)
        if (defWF.c_estado!="R")
        {
          NomadLog.Error("El Workflow no esta Habilitado.");
          throw new NomadMessage("NucleusWF.EJECUCION.NO-NEW-INSTANCE");
        }

      //Obtengo el Organigrama RAIZ
      NucleusWF.Base.Definicion.Organigramas.ORGANIGRAMA defORG=NucleusWF.Base.Definicion.Organigramas.ORGANIGRAMA.Get(defWF.oi_organigrama, false);
      if (defORG.e_version_pub==0)
      {
        NomadLog.Error("El Organigrama '"+defORG.c_organigrama+"-"+defORG.d_organigrama+"' asociado al Workflow '"+defWF.c_wf+"-"+defWF.d_wf+"' nunca fue Publicado.");
        throw new NomadMessage("NucleusWF.EJECUCION.NO-NEW-INSTANCE");
      }

      //Obtengo el ID del Organigrama Publicado
      string oi_organigrama=NomadEnvironment.QueryValue("WRK05_ORGANIGRAMAS", "oi_organigrama", "c_organigrama", defORG.c_organigrama, "WRK05_ORGANIGRAMAS.e_version="+defORG.e_version_pub, false);
      if (oi_organigrama==null)
      {
        NomadLog.Error("La Version '"+defORG.e_version_pub+"' del Organigrama '"+defORG.c_organigrama+"-"+defORG.d_organigrama+"' no Encontrada.");
        throw new NomadMessage("NucleusWF.EJECUCION.NO-NEW-INSTANCE");
      }

      //Obtengo el ACTOR
      NucleusWF.Base.Definicion.Organigramas.ORGANIGRAMA pubORG=NucleusWF.Base.Definicion.Organigramas.ORGANIGRAMA.Get(oi_organigrama, false);
      NucleusWF.Base.Definicion.Organigramas.ROLE pubROL=(NucleusWF.Base.Definicion.Organigramas.ROLE)pubORG.ROLES.GetByAttribute("c_role", c_rol);
      if (pubROL==null)
      {
        NomadLog.Error("El Rol/Actor '"+c_rol+"' no Encontrado en el Organigrama '"+defORG.c_organigrama+"-"+defORG.d_organigrama+"-ver:"+defORG.e_version_pub+"'.");
        throw new NomadMessage("NucleusWF.EJECUCION.NO-NEW-INSTANCE");
      }

      //Obtengo la Estructuras del USUARIO/ROL
      string c_estructura=NomadProxy.GetProxy().SQLService().GetXML("CLASS.NucleusWF.Base.Ejecucion.Instancias.INSTANCE.qry_estructura", "<DATA oi_entidad=\""+objETTY.GetAttr("id")+"\" oi_organigrama=\""+oi_organigrama+"\" c_rol=\""+c_rol+"\"/>").GetAttr("c_estructura");
      if (c_estructura=="")
      {
        NomadLog.Error("La Entidad '"+objETTY.GetAttr("des")+"' no encontrada en el Organigrama '"+defWF.c_wf+"-"+defWF.d_wf+"' con el Rol/Actor '"+pubROL.c_role+"-"+pubROL.d_role+"'.");
        throw new NomadMessage("NucleusWF.EJECUCION.NO-NEW-INSTANCE");
      }

      //Recorro los Procesos y Busco si puede Iniciar alguno con ese ROL
      NucleusWF.Base.Definicion.Workflows.PROCESS defPRO=null;
      foreach (NucleusWF.Base.Definicion.Workflows.PROCESS curPRO in defWF.PROCESS)
      {
        NucleusWF.Base.Definicion.Workflows.ROLE_PROCESS MyRP=(NucleusWF.Base.Definicion.Workflows.ROLE_PROCESS)curPRO.ROLES_PRO.GetByAttribute("c_rol", c_rol);
        if (MyRP==null) continue;
        if (!MyRP.l_iniciar) continue;
        if (oi_process!="" && oi_process!=curPRO.Id) continue;

        //Verifico los Parametros
        bool isOK=true;
        foreach (NucleusWF.Base.Definicion.Workflows.PROCESS_PARAM param in curPRO.PROCESS_PARAM)
          if (param.c_io!="RET" && Params.GetAttr(param.c_process_param)=="")
          {
            isOK=false;
            break;
          }

        //Encontrado
        if (isOK)
          defPRO=curPRO;
      }
      if (defPRO==null)
      {
        NomadLog.Error("El Rol/Actor '"+pubROL.c_role+"-"+pubROL.d_role+"' no puede Iniciar ningun Proceso del Workflow '"+defWF.c_wf+"-"+defWF.d_wf+"'.");
        throw new NomadMessage("NucleusWF.EJECUCION.NO-NEW-INSTANCE");
      }

      //Validaciones sobre el PROCESO
      if (defPRO.e_automatico!=0)
        throw new NomadException("Error Interno del WF '"+defWF.c_wf+"-"+defWF.d_wf+"' - El Proceso no puede ser iniciado - no es un proceso RAIZ/MANUAL.");

      //Busco el Proceso INICIAL
      NucleusWF.Base.Definicion.Workflows.NODE NodoInicial=null;
      double secuence=-1;
      foreach (NucleusWF.Base.Definicion.Workflows.NODE cNode in defPRO.NODES)
        if (secuence==-1 || cNode.n_secuence<secuence)
        {
          secuence=cNode.n_secuence;
          NodoInicial=cNode;
        }

      //Valido el NODO INICIAL
      if (NodoInicial.c_type!="INI")
        throw new NomadException("Error Interno del WF '"+defWF.c_wf+"-"+defWF.d_wf+"' - El Proceso no puede ser iniciado - el nodo inicial no es valido.");

      //Creo la Instancia
      NucleusWF.Base.Ejecucion.Instancias.INSTANCE newINST=new NucleusWF.Base.Ejecucion.Instancias.INSTANCE();
      newINST.oi_wf        =defPRO.oi_wf.ToString();
      newINST.c_instance   =Nomad.NSystem.Functions.StringUtil.GenerateUUID();
      newINST.d_instance   =defWF.d_wf+" - "+defPRO.d_process;
      newINST.c_rol        =pubROL.c_role;
      newINST.c_estructura =c_estructura;
      newINST.d_owner      =objETTY.GetAttr("id");
      newINST.d_owner_ayn  =objETTY.GetAttr("DES");
      newINST.d_owner_rol  =pubROL.d_role;
      newINST.l_debug      =(Debug==0?false:true);

      //Verifico los Parametros
      foreach (NucleusWF.Base.Definicion.Workflows.PROCESS_PARAM param in defPRO.PROCESS_PARAM)
      {
        PARAM newParam=new PARAM();
        newParam.c_param=param.c_process_param;
        newParam.d_param=param.d_process_param;
        newParam.c_type =param.c_type;
        newParam.c_io   =param.c_io;
        if (param.c_io != "RET")
          newParam.d_valor=Params.GetAttr(param.c_process_param);

        //Nueva Instancia
        newINST.PARAMS.Add(newParam);
      }

      //Obtengo la Entidad
      NomadXML AUXxml;

      //MAIL
      AUXxml=objETTY.FindElement("MAILS");
      if (AUXxml!=null)
        AUXxml=AUXxml.FindElement2("MAIL", "PRINCIPAL", "1");
      if (AUXxml!=null)
        newINST.d_owner_mail=AUXxml.GetAttr("EMAIL");

      //TELEFONO
      AUXxml=objETTY.FindElement("TELEFONOS");
      if (AUXxml!=null)
        AUXxml=AUXxml.FindElement2("TELEFONO", "PRINCIPAL", "1");
      if (AUXxml!=null)
        newINST.d_owner_tel=AUXxml.GetAttr("TELEF");

      newINST.e_pasos      =0;
      newINST.c_estado_ej  ="R";
      newINST.c_estado_wf  ="";
      newINST.f_inicio     =DateTime.Now;
      if (defWF.n_horas>0.0)
      {
        newINST.f_advertencia=newINST.f_inicio.AddHours(defWF.n_horas * 0.60);
        newINST.f_vencimiento=newINST.f_inicio.AddHours(defWF.n_horas);
      }

      //Creo el THREAD
      NucleusWF.Base.Ejecucion.Instancias.THREAD newTHREAD=new NucleusWF.Base.Ejecucion.Instancias.THREAD();
      newTHREAD.oi_node    =NodoInicial.Id;
      newTHREAD.c_estado   ="WE";
      newTHREAD.f_ejecucion=newINST.f_inicio;
      newINST.THREADS.Add(newTHREAD);

      //Creo el LOG
      AddLog(newINST, newTHREAD, "IFO", "Comienza el proceso, "+defWF.d_wf+" - "+defPRO.d_process);

      //Agrego los Grupos de VARIABLES
      NucleusWF.Base.Ejecucion.Instancias.VALUE newVAL;
      NucleusWF.Base.Ejecucion.Instancias.VALUE_HIST newHIST;
      foreach (NucleusWF.Base.Definicion.Workflows.VAR_GROUP grp in defWF.GROUPS)
      {
        switch (grp.c_mode)
        {
          case "MAIN":
            newVAL=new NucleusWF.Base.Ejecucion.Instancias.VALUE();
            newVAL.c_var_group=grp.c_var_group;
            newINST.VALS.Add(newVAL);
            break;

          case "HIST":
            newHIST=new NucleusWF.Base.Ejecucion.Instancias.VALUE_HIST();
            newHIST.c_var_group=grp.c_var_group;
            newHIST.e_paso =0;
            newINST.VALS_HIST.Add(newHIST);
            break;

          case "CHILD":
            break;
        }
      }

      //Guardar la INSTANCIAS
      newINST.d_etty_creator=NomadProxy.GetProxy().UserEtty;
      NomadEnvironment.GetCurrentTransaction().SaveRefresh(newINST);
      Notify(newINST);

      //Resultado
      return newINST.Id;
    }

    public static void SetVariables(int oi_thread, NomadXML DATA) { SetVariables(null, oi_thread, DATA); }
    public static void SetVariables(NucleusWF.Base.Ejecucion.Instancias.INSTANCE theINST, int oi_thread, NomadXML DATA)
    {
      NucleusWF.Base.Ejecucion.Instancias.THREAD theTHREAD;
      if (DATA.isDocument) DATA=DATA.FirstChild();

      //Obtengo la Instancia si esta no fue ENVIADA
      if (theINST==null)
      {
        theTHREAD=NucleusWF.Base.Ejecucion.Instancias.THREAD.Get(oi_thread.ToString(), false);
        theINST=NucleusWF.Base.Ejecucion.Instancias.INSTANCE.Get(theTHREAD.oi_instance, false);
      }

      //Obtengo la Definicion del WF si esta no fue ENVIADA
      NucleusWF.Base.Definicion.Workflows.WF defWF=INSTANCE.GetWFDef(theINST.oi_wf);

      //Obtengo el Thread
      theTHREAD=(NucleusWF.Base.Ejecucion.Instancias.THREAD)theINST.THREADS.GetById(oi_thread.ToString());

      //Verifico el PASO
      if (theINST.e_pasos!=DATA.GetAttrInt("paso"))
      {
        NomadLog.Error("La solicitud cambio desde que fue pedida.");
        throw new NomadMessage("NucleusWF.EJECUCION.OUT-OF-VERSION");
      }

      //Actualizo las Variables de la INSTANCIA
      theINST.d_instance=Nomad.Base.Report.Variables.GetValue(DATA, -1, "Instancia.Descripcion", "string", false);
      if (Nomad.Base.Report.Variables.GetValueNull(DATA, -1, "Instancia.Fecha_Advertencia")) theINST.f_advertenciaNull=true;
      else theINST.f_advertencia    =Nomad.NSystem.Functions.StringUtil.str2date(Nomad.Base.Report.Variables.GetValue(DATA, -1, "Instancia.Fecha_Advertencia", "date", false));
      if (Nomad.Base.Report.Variables.GetValueNull(DATA, -1, "Instancia.Fecha_Vencimiento")) theINST.f_vencimientoNull=true;
      else theINST.f_vencimiento    =Nomad.NSystem.Functions.StringUtil.str2date(Nomad.Base.Report.Variables.GetValue(DATA, -1, "Instancia.Fecha_Vencimiento", "date", false));

      //Actualizo las Variables del PROCESO
      if (Nomad.Base.Report.Variables.GetValueNull(DATA, -1, "Proceso.Fecha_Advertencia")) theTHREAD.f_advertenciaNull=true;
      else theTHREAD.f_advertencia    =Nomad.NSystem.Functions.StringUtil.str2date(Nomad.Base.Report.Variables.GetValue(DATA, -1, "Proceso.Fecha_Advertencia", "date", false));
      if (Nomad.Base.Report.Variables.GetValueNull(DATA, -1, "Proceso.Fecha_Vencimiento")) theTHREAD.f_vencimientoNull=true;
      else theTHREAD.f_vencimiento    =Nomad.NSystem.Functions.StringUtil.str2date(Nomad.Base.Report.Variables.GetValue(DATA, -1, "Proceso.Fecha_Vencimiento", "date", false));

      //Variables del Usuario
      //Recorro los Indices y RELLENO el XML
      int idx, cnt;
      string Value, ValueTXT;
      SortedList MyLIST=new SortedList();

      foreach (NucleusWF.Base.Definicion.Workflows.VAR_GROUP grp in defWF.GROUPS)
      {
        switch (grp.c_mode)
        {
          case "MAIN":
            foreach (VALUE theVAL in theINST.VALS)
            {
              //Busco el Grupo
              if (theVAL.c_var_group!=grp.c_var_group)
                continue;

              //Busco la VARIABLE
              foreach (NucleusWF.Base.Definicion.Workflows.VAR svar in grp.VARS)
              {
                if (Nomad.Base.Report.Variables.GetValueNull(DATA, -1, grp.c_var_group+"."+svar.c_var))
                {
                  //Pego el VALOR
                  theVAL.SetAttributeNull(svar.c_column);
                  if (svar.c_column_desc!="")
                    theVAL.SetAttributeNull(svar.c_column_desc);
                } else
                {
                  //Pego el VALOR
                  Value=Nomad.Base.Report.Variables.GetValue(DATA, -1, grp.c_var_group+"."+svar.c_var, svar.c_type, false);
                  switch (svar.c_column.Substring(0, 2))
                  {
                    case "e_": theVAL.SetAttribute(svar.c_column, Nomad.NSystem.Functions.StringUtil.str2int(Value)); break;
                    case "n_": theVAL.SetAttribute(svar.c_column, Nomad.NSystem.Functions.StringUtil.str2dbl(Value)); break;
                    case "d_": theVAL.SetAttribute(svar.c_column, Nomad.NSystem.Functions.StringUtil.str2date(Value)); break;
                    case "o_": theVAL.SetAttribute(svar.c_column, Value); break;
                    case "c_": theVAL.SetAttribute(svar.c_column, Value); break;
                  }

                  //Pego del TEXTO
                  if (svar.c_column_desc!="")
                  {
                    ValueTXT=Nomad.Base.Report.Variables.GetValue(DATA, -1, grp.c_var_group+"."+svar.c_var, svar.c_type, true);
                    theVAL.SetAttribute(svar.c_column_desc, ValueTXT);
                  }
                }
              }
            }
            break;

          case "HIST":
            foreach (VALUE_HIST theVAL in theINST.VALS_HIST)
            {
              //Busco el Grupo
              if (theVAL.c_var_group!=grp.c_var_group)
                continue;

              if (theVAL.e_paso!=theINST.e_pasos)
                continue;

              //Busco la VARIABLE
              idx=Nomad.Base.Report.Variables.CountItem(DATA, grp.c_var_group)-1;
              foreach (NucleusWF.Base.Definicion.Workflows.VAR svar in grp.VARS)
              {
                if (Nomad.Base.Report.Variables.GetValueNull(DATA, idx, grp.c_var_group+"."+svar.c_var))
                {
                  //Pego el VALOR
                  theVAL.SetAttributeNull(svar.c_column);
                  if (svar.c_column_desc!="")
                    theVAL.SetAttributeNull(svar.c_column_desc);
                } else
                {
                  //Pego el VALOR
                  Value=Nomad.Base.Report.Variables.GetValue(DATA, idx, grp.c_var_group+"."+svar.c_var, svar.c_type, false);
                  switch (svar.c_column.Substring(0, 2))
                  {
                    case "e_": theVAL.SetAttribute(svar.c_column, Nomad.NSystem.Functions.StringUtil.str2int(Value)); break;
                    case "n_": theVAL.SetAttribute(svar.c_column, Nomad.NSystem.Functions.StringUtil.str2dbl(Value)); break;
                    case "d_": theVAL.SetAttribute(svar.c_column, Nomad.NSystem.Functions.StringUtil.str2date(Value)); break;
                    case "c_": theVAL.SetAttribute(svar.c_column, Value); break;
                    case "o_": theVAL.SetAttribute(svar.c_column, Value); break;
                  }

                  //Pego del TEXTO
                  if (svar.c_column_desc!="")
                  {
                    ValueTXT=Nomad.Base.Report.Variables.GetValue(DATA, idx, grp.c_var_group+"."+svar.c_var, svar.c_type, true);
                    theVAL.SetAttribute(svar.c_column_desc, ValueTXT);
                  }
                }
              }
            }
            break;

          case "CHILD":
            //Filtro la Lista y la Ordeno por el Indice
            MyLIST.Clear();
            foreach (VALUE_CHILD theVAL in theINST.VALS_CHILD)
            {
              //Busco el Grupo
              if (theVAL.c_var_group!=grp.c_var_group)
                continue;

              MyLIST.Add(theVAL.e_indice, theVAL);
            }

            //Reindexo los Elementos
            idx=0;
            foreach (VALUE_CHILD theVAL in MyLIST.GetValueList()) { theVAL.e_indice=idx; idx++; }

            //Elimino los Child Sobrantes
            idx=0; cnt=Nomad.Base.Report.Variables.CountItem(DATA, grp.c_var_group);
            foreach (VALUE_CHILD theVAL in MyLIST.GetValueList())
            {
              if (idx>=cnt) theINST.VALS_CHILD.Remove(theVAL);
              idx++;
            }

            //Agrego los Child Faltantes
            for (idx=MyLIST.Count; idx<cnt; idx++)
            {
              VALUE_CHILD newVAL=new VALUE_CHILD();
              newVAL.c_var_group=grp.c_var_group;
              newVAL.e_indice=idx;
              theINST.VALS_CHILD.Add(newVAL);
            }

            //Filtro la Lista y la Ordeno por el Indice
            MyLIST.Clear();
            foreach (VALUE_CHILD theVAL in theINST.VALS_CHILD)
              if (theVAL.c_var_group==grp.c_var_group)
                MyLIST.Add(theVAL.e_indice, theVAL);

            //Actualizo los ITEMS
            foreach (VALUE_CHILD theVAL in MyLIST.GetValueList())
            {
              idx=theVAL.e_indice;

              //Busco la VARIABLE
              foreach (NucleusWF.Base.Definicion.Workflows.VAR svar in grp.VARS)
              {
                if (Nomad.Base.Report.Variables.GetValueNull(DATA, idx, grp.c_var_group+"."+svar.c_var))
                {
                  //Pego el VALOR
                  theVAL.SetAttributeNull(svar.c_column);
                  if (svar.c_column_desc!="")
                    theVAL.SetAttributeNull(svar.c_column_desc);
                } else
                {
                  //Pego el VALOR
                  Value=Nomad.Base.Report.Variables.GetValue(DATA, idx, grp.c_var_group+"."+svar.c_var, svar.c_type, false);
                  switch (svar.c_column.Substring(0, 2))
                  {
                    case "e_": theVAL.SetAttribute(svar.c_column, Nomad.NSystem.Functions.StringUtil.str2int(Value)); break;
                    case "n_": theVAL.SetAttribute(svar.c_column, Nomad.NSystem.Functions.StringUtil.str2dbl(Value)); break;
                    case "d_": theVAL.SetAttribute(svar.c_column, Nomad.NSystem.Functions.StringUtil.str2date(Value)); break;
                    case "c_": theVAL.SetAttribute(svar.c_column, Value); break;
                    case "o_": theVAL.SetAttribute(svar.c_column, Value); break;
                  }

                  //Pego del TEXTO
                  if (svar.c_column_desc!="")
                  {
                    ValueTXT=Nomad.Base.Report.Variables.GetValue(DATA, idx, grp.c_var_group+"."+svar.c_var, svar.c_type, true);
                    theVAL.SetAttribute(svar.c_column_desc, ValueTXT);
                  }
                }
              }
            }
            break;
        }
      }

      //Actualizo la Lista de Archivos
      NomadXML xmlFILES=DATA.FindElement("FILES");
      NomadXML xmlFILE;
      ArrayList ToDelete=new ArrayList();

      //Busco Elementos a Eliminar
      foreach (DOCUMENT doc in theINST.DOCUMENTS)
      {
        xmlFILE=xmlFILES.FindElement2("FILE", "id", doc.oi_file_wrk);
        if (xmlFILE==null)
          ToDelete.Add(doc);
        else
          xmlFILE.SetAttr("exist", 1);
      }
      foreach (DOCUMENT doc in ToDelete)
        theINST.DOCUMENTS.Remove(doc);

      //Busco Elementos a Agregar
      for (xmlFILE=xmlFILES.FirstChild(); xmlFILE!=null; xmlFILE=xmlFILE.Next())
        if (xmlFILE.GetAttr("exist")!="1")
        {
          DOCUMENT newdoc=new DOCUMENT();
          newdoc.oi_file_wrk=xmlFILE.GetAttr("id");
          theINST.DOCUMENTS.Add(newdoc);
        }

      //Actualizo el TXTWiki de Resultado
      if (defWF.c_text!="")
      {
        NucleusWF.Base.Definicion.Workflows.TEXT txtDef=(NucleusWF.Base.Definicion.Workflows.TEXT)defWF.NODE_TEXTS.GetByAttribute("c_text", defWF.c_text);
        NucleusWF.Base.Ejecucion.Instancias.REPORT newText;
        ArrayList ele2Delete=new ArrayList();

        int paso=0;
        string WikiResult=txtDef.GetTextResult(DATA);

        while (true)
        {
          //Obtengo el primer pedaso de TEXTO
          newText=(NucleusWF.Base.Ejecucion.Instancias.REPORT)theINST.REPORT.GetByAttribute("e_orden", paso);
          if (newText==null)
          {
            newText=new NucleusWF.Base.Ejecucion.Instancias.REPORT();
            newText.e_orden=paso;
            theINST.REPORT.Add(newText);

            //obtengo el Texto Nuevo
            newText=(NucleusWF.Base.Ejecucion.Instancias.REPORT)theINST.REPORT.GetByAttribute("e_orden", paso);
          }
          paso++;

          if (WikiResult.Length>4000)
          {
            newText.o_text=WikiResult.Substring(0, 4000);
            WikiResult=WikiResult.Substring(4000);
          } else
          {
            newText.o_text=WikiResult;
            WikiResult="";
            break;
          }
        }

        //Busco los elementos a eliminar
        foreach (NucleusWF.Base.Ejecucion.Instancias.REPORT eleTest in theINST.REPORT)
          if (eleTest.e_orden>=paso) ele2Delete.Add(eleTest);

        //Elimino los Elementos
        foreach (NucleusWF.Base.Ejecucion.Instancias.REPORT ele2del in ele2Delete)
          theINST.REPORT.Remove(ele2del);
      } else
      {
        theINST.REPORT.Clear();
      }

      //Resultado
      return;
    }

    public static string GenerateReportText(string oi_instance, string oi_thread, string c_text, NomadXML Data)
    {
      return GenerateReportXML(oi_instance, oi_thread, c_text, Data).GetAttr("");
    }
    public static NomadXML GenerateReportXML(string oi_instance, string oi_thread, string c_text)
    {
      return GenerateReportXML(oi_instance, oi_thread, c_text, null);
    }

    public static NomadXML GenerateReportXML(string oi_instance, string oi_thread, string c_text, NomadXML Data)
    {
      NomadXML retval=new NomadXML("REPORT");

      //Obtengo el Primer Elemento
      if (Data!=null && Data.isDocument) Data=Data.FirstChild();

      //Busco la Instancia
      NucleusWF.Base.Ejecucion.Instancias.INSTANCE theINST=null;

      if (oi_instance!=null && oi_instance!="" && theINST==null)
      {
        theINST=NucleusWF.Base.Ejecucion.Instancias.INSTANCE.Get(oi_instance, false);
      } else
        if (oi_thread!=null && oi_thread!="" && theINST==null)
        {
          NucleusWF.Base.Ejecucion.Instancias.THREAD theTHREAD=NucleusWF.Base.Ejecucion.Instancias.THREAD.Get(oi_thread, false);
          oi_instance=theTHREAD.oi_instance.ToString();

          theINST=NucleusWF.Base.Ejecucion.Instancias.INSTANCE.Get(theTHREAD.oi_instance, false);
        }

      //Obtengo el Objeto WF
      NucleusWF.Base.Definicion.Workflows.WF defWF=NucleusWF.Base.Definicion.Workflows.WF.Get(theINST.oi_wf, false);
      NucleusWF.Base.Definicion.Workflows.TEXT txtDef=null;

      if (c_text!=null && c_text!="")
        txtDef=(NucleusWF.Base.Definicion.Workflows.TEXT)defWF.NODE_TEXTS.GetByAttribute("c_text", c_text);
      else
        txtDef=(NucleusWF.Base.Definicion.Workflows.TEXT)defWF.NODE_TEXTS.GetByAttribute("c_text", defWF.c_text);

      //Cargo el DATA desde la Base de Datos
      if (Data == null || Data.FirstChild()==null)
        Data = GetVariables(theINST, 0);

      //Text WIKI
      retval.SetAttr("", txtDef.GetTextResult(Data));

      //Tipo de TEXTO
      retval.SetAttr("type", txtDef.c_tipo);

      //Text WIKI
      return retval;
    }

    public static void EmptyThread(NucleusWF.Base.Ejecucion.Instancias.THREAD theTHREAD)
    {
      theTHREAD.f_inicioNull=true;
      theTHREAD.f_advertenciaNull=true;
      theTHREAD.f_vencimientoNull=true;

      theTHREAD.d_deleg_ettyNull=true;
      theTHREAD.d_deleg_aynNull=true;
      theTHREAD.d_deleg_mailNull=true;
      theTHREAD.d_deleg_telNull=true;
      theTHREAD.c_deleg_rolNull=true;
      theTHREAD.d_deleg_tituloNull=true;
      theTHREAD.o_deleg_comentNull=true;

      theTHREAD.d_asign_ettyNull=true;
      theTHREAD.d_asign_aynNull=true;
      theTHREAD.d_asign_mailNull=true;
      theTHREAD.d_asign_telNull=true;
      theTHREAD.c_asign_rolNull=true;
    }

    public static void NextStep(NucleusWF.Base.Ejecucion.Instancias.INSTANCE theINST, NucleusWF.Base.Ejecucion.Instancias.THREAD theTHREAD, NomadXML xmlINFO)
    {
      //Busco los NODOS Principales
      NomadXML xmlNODE=xmlINFO.FindElement("NODE");
      NomadXML xmlPROCESS=xmlINFO.FindElement("PROCESS");

      //Busco el nombre del proximo NODO
      NomadXML nextNODE=null;
      if (xmlNODE==null)
        nextNODE=xmlPROCESS.FirstChild();
      else
        for (NomadXML cur=xmlPROCESS.FirstChild(); cur!=null && nextNODE==null; cur=cur.Next())
          if (cur.GetAttr("oi_node")==xmlNODE.GetAttr("oi_node"))
            nextNODE=cur.Next();

      if (nextNODE==null)
      { //No hay mas nodos, FIN del SUBPROCESO

        string ThreadParent=theTHREAD.oi_thread_parent;
        if (ThreadParent=="")
        {
          SetError(theINST, theTHREAD, "Error, problemas con los Thread de Ejecucion...");
          return;
        }

        //Inactivo el THREAD
        EmptyThread(theTHREAD);
        theTHREAD.c_estado="I";
        theTHREAD.oi_thread_parentNull=true;
        theTHREAD.f_ejecucion=DateTime.Now;

        //Busco otros Thread Hermanos
        foreach (NucleusWF.Base.Ejecucion.Instancias.THREAD curTHREAD in theINST.THREADS)
          if (curTHREAD.oi_thread_parent==ThreadParent)
            return; //Encontro un Thread Hermano (proceso paralelo), no hace nada mas....

        //Activo el THREAD PADRE
        theTHREAD=(NucleusWF.Base.Ejecucion.Instancias.THREAD)theINST.THREADS.GetById(ThreadParent);

        //Ejecuto el Query de Proceso
        NextStep(theINST, theTHREAD, NomadProxy.GetProxy().SQLService().GetXML("CLASS.NucleusWF.Base.Ejecucion.Instancias.INSTANCE.qry_proceso", "<DATA oi_node=\""+theTHREAD.oi_node+"\" />"));
      } else
      {
        switch (nextNODE.GetAttr("c_type"))
        {
          case "WAIT_TIME":
            //Actualizo el NUEVO NODO
            EmptyThread(theTHREAD);
            theTHREAD.oi_node=nextNODE.GetAttr("oi_node");
            theTHREAD.c_estado="WE";
            theTHREAD.f_ejecucion=DateTime.Now.AddMinutes(nextNODE.GetAttrInt("c_param_1"));
            break;

          case "CALL":
          case "FORM":
            //Actualizo el NUEVO NODO
            EmptyThread(theTHREAD);
            theTHREAD.oi_node=nextNODE.GetAttr("oi_node");
            theTHREAD.c_estado="WI";
            theTHREAD.f_ejecucion=DateTime.Now;
            break;

          default:
            //Actualizo el NUEVO NODO
            EmptyThread(theTHREAD);
            theTHREAD.oi_node=nextNODE.GetAttr("oi_node");
            theTHREAD.c_estado="WE";
            theTHREAD.f_ejecucion=DateTime.Now;
            break;
        }
      }
    }

    public static void AddThread(NucleusWF.Base.Ejecucion.Instancias.INSTANCE theINST, int oi_thread_parent, string c_proceso, string e_subproceso)
    {
      NomadXML xmlINFO=NomadProxy.GetProxy().SQLService().GetXML("CLASS.NucleusWF.Base.Ejecucion.Instancias.INSTANCE.qry_proceso", "<DATA oi_wf=\""+theINST.oi_wf+"\" c_process=\""+c_proceso+"\" e_automatico=\""+e_subproceso+"\" />");
      NomadXML xmlPROC=xmlINFO.FindElement("PROCESS");
      NucleusWF.Base.Ejecucion.Instancias.THREAD newTHREAD;

      //Creo el TRHEAD
      newTHREAD=(NucleusWF.Base.Ejecucion.Instancias.THREAD)theINST.THREADS.GetByAttribute("c_estado", "I");
      if (newTHREAD==null)
      {
        newTHREAD=new NucleusWF.Base.Ejecucion.Instancias.THREAD();
        theINST.THREADS.Add(newTHREAD);
      }
      newTHREAD.oi_thread_parent=oi_thread_parent.ToString();
      newTHREAD.f_ejecucion     =DateTime.Now;
      newTHREAD.c_estado        ="R";

      //Proximo PASO
      NextStep(theINST, newTHREAD, xmlINFO);
    }

    public static NomadXML FindLabel(NucleusWF.Base.Ejecucion.Instancias.INSTANCE theINST, NucleusWF.Base.Ejecucion.Instancias.THREAD theTHREAD, string Label)
    {
      NomadXML xmlINFO=NomadProxy.GetProxy().SQLService().GetXML("CLASS.NucleusWF.Base.Ejecucion.Instancias.INSTANCE.qry_proceso", "<DATA oi_node=\""+theTHREAD.oi_node+"\" />");
      NomadXML xmlTH, retval;

      //Buscar en este THREAD
      for (NomadXML cur=xmlINFO.FindElement("PROCESS").FirstChild(); cur!=null && cur.GetAttr("oi_node")!=theTHREAD.oi_node; cur=cur.Next())
        if (cur.GetAttr("c_type").ToUpper()=="LABEL" && cur.GetAttr("d_label").ToUpper()==Label.ToUpper())
        {
          xmlTH=cur.AddTailElement("THREAD");
          xmlTH.SetAttr("id", theTHREAD.Id);
          xmlTH.SetAttr("type", "MAIN");
          return cur;
        }

      //No encontrado, Buscar en un THREAD PADRE
      if (theTHREAD.oi_thread_parent!="")
      {
        NucleusWF.Base.Ejecucion.Instancias.THREAD parTHREAD=(NucleusWF.Base.Ejecucion.Instancias.THREAD)theINST.THREADS.GetById(theTHREAD.oi_thread_parent);
        if (parTHREAD!=null)
        {
          retval=FindLabel(theINST, parTHREAD, Label);
          if (retval!=null)
          {
            xmlTH=retval.AddTailElement("THREAD");
            xmlTH.SetAttr("id", theTHREAD.Id);
            xmlTH.SetAttr("type", "CHILD");
          }
          return retval;
        }
      }

      return null;
    }

    public static void Validar(NucleusWF.Base.Ejecucion.Instancias.INSTANCE theINST, string instEstados, NucleusWF.Base.Ejecucion.Instancias.THREAD theTHREAD, string thredEstados, int iPaso, bool ChekcRunning)
    {
      //Analizo el estado de la instancia
      if (instEstados!=null)
        if (!(","+instEstados+",").Contains(","+theINST.c_estado_ej+","))
        {
          switch (instEstados)
          {
            case "R":
              throw new NomadMessage("NucleusWF.EJECUCION.INVALID-STATE-R");
            case "P":
              throw new NomadMessage("NucleusWF.EJECUCION.INVALID-STATE-P");
            case "E":
              throw new NomadMessage("NucleusWF.EJECUCION.INVALID-STATE-E");
            default:
              throw new NomadMessage("NucleusWF.EJECUCION.INVALID-STATE");
          }
        }

      //Analizo el estado de los THREADS
      if (ChekcRunning)
        foreach (NucleusWF.Base.Ejecucion.Instancias.THREAD oTH in theINST.THREADS)
          if (oTH.c_estado=="R")
            throw new NomadMessage("NucleusWF.EJECUCION.BUSSY");

      //Analizo el estado del THREAD
      if (thredEstados!=null)
        if (!(","+thredEstados+",").Contains(","+theTHREAD.c_estado+","))
          throw new NomadMessage("NucleusWF.EJECUCION.INVALID-STATE");

      //Analizo el Paso
      if (iPaso>=0)
        if (theINST.e_pasos!=iPaso)
          throw new NomadMessage("NucleusWF.EJECUCION.OUT-OF-DATE");
    }

    private static bool ConfigureUSER(Hashtable hUSER, THREAD pTHREAD, string rol, string etty, string ayn, string resolve, string notify)
    {
      bool change=false;
      USER newUSER;
      string Key = (rol + "-" + etty).ToUpper();

      //Busco el USUARIO/ROL
      newUSER = null;
      if (hUSER.ContainsKey(Key))
      {
        newUSER = (USER)hUSER[Key];
        hUSER.Remove(Key);
      }

      //Agrego el USUARIO/ROL
      if (newUSER == null)
      {
        newUSER = new USER();
        newUSER.c_rol = rol;
        newUSER.oi_etty = etty;
        newUSER.d_etty = ayn;

        pTHREAD.USERS.Add(newUSER);
        change=true;
      }

      //Calculo la Fecha de Inicio de Validez
      switch (resolve)
      {
        case "AV": //Vencido
          newUSER.l_resolve = !pTHREAD.f_vencimientoNull;
          newUSER.f_resolve = pTHREAD.f_vencimiento;
          break;
        case "PV":
          newUSER.l_resolve = !pTHREAD.f_advertenciaNull;
          newUSER.f_resolve = pTHREAD.f_advertencia;
          break;
        case "SI":
          newUSER.l_resolve = true;
          newUSER.f_resolve = DateTime.Now;
          break;
        default:
          newUSER.l_resolve = false;
          newUSER.f_resolve = DateTime.Now;
          break;
      }

      //Calculo la Fecha de Inicio de Validez
      switch (notify)
      {
        case "AV": //Vencido
          newUSER.l_notify = !pTHREAD.f_vencimientoNull;
          newUSER.f_notify = pTHREAD.f_vencimiento;
          break;
        case "PV":
          newUSER.l_notify = !pTHREAD.f_advertenciaNull;
          newUSER.f_notify = pTHREAD.f_advertencia;
          break;
        case "SI":
          newUSER.l_notify = true;
          newUSER.f_notify = DateTime.Now;
          break;
        default:
          newUSER.l_notify = false;
          newUSER.f_notify = DateTime.Now;
          break;
      }

      return change;
    }

    private static NucleusWF.Base.Definicion.Workflows.WF GetWFDef(string id)
    {
      NucleusWF.Base.Definicion.Workflows.WF pWF;

      pWF = (NucleusWF.Base.Definicion.Workflows.WF)NomadProxy.GetProxy().CacheGetObj("WORKFLOW-"+id);
      if (pWF == null)
      {
        pWF = NucleusWF.Base.Definicion.Workflows.WF.Get(id, false);
        NomadProxy.GetProxy().CacheAdd("WORKFLOW-"+id, pWF);
      }

      return pWF;
    }

    private static NucleusWF.Base.Definicion.Organigramas.ORGANIGRAMA GetORGDef(string id)
    {
      NucleusWF.Base.Definicion.Organigramas.ORGANIGRAMA pORG;

      pORG = (NucleusWF.Base.Definicion.Organigramas.ORGANIGRAMA)NomadProxy.GetProxy().CacheGetObj("ORGANIGRAMA-" + id);
      if (pORG == null)
      {
        pORG = NucleusWF.Base.Definicion.Organigramas.ORGANIGRAMA.Get(id, false);
        NomadProxy.GetProxy().CacheAdd("ORGANIGRAMA-" + id, pORG);
      }

      return pORG;
    }

    public static bool RefreshUSERS(INSTANCE pINST, THREAD pTHREAD)
    {
      int count=0;
      bool change=false;

      //Si no esta en estado WP limpio la lista de usuarios Validos.
      if (pTHREAD.c_estado!="WP" || pINST.c_estado_ej!="R")
      {
        if (pTHREAD.USERS.Count>0)
        {
          change=true;
          pTHREAD.USERS.Clear();
        }
        return change;
      }

      Hashtable hUSER = new Hashtable();
      NomadXML ROLE, ETTY;

      //Agrego la lista de usuarios
      foreach (USER curUSER in pTHREAD.USERS)
        hUSER[(curUSER.c_rol+"-"+curUSER.oi_etty).ToUpper()]=curUSER;

      //Cargo la Definicion del WF
      NucleusWF.Base.Definicion.Workflows.WF pWF = INSTANCE.GetWFDef(pINST.oi_wf);

      //Cargo la Definicion del ORG
      NucleusWF.Base.Definicion.Organigramas.ORGANIGRAMA pORG = INSTANCE.GetORGDef(pWF.oi_organigrama);

      //Actualizo la version en el THREAD
      pTHREAD.c_organigrama=pORG.c_organigrama;
      pTHREAD.e_org_version=pORG.e_version_pub;

      if (pTHREAD.d_asign_etty != "")
      { //Hay un entidad en particular asignada
        count++;
        if (ConfigureUSER(hUSER, pTHREAD, "ASSIGN", pTHREAD.d_asign_etty, pTHREAD.d_asign_ayn, "SI", "SI"))
          change=true;
      } else
        if (pTHREAD.c_asign_estr!="")
        { //Hay un entidad en particular asignada
          NomadXML xmlNODE = NomadProxy.GetProxy().SQLService().GetXML("CLASS.NucleusWF.Base.Ejecucion.Instancias.INSTANCE.qry_user_estructura", "<DATA c_estructura=\"" + pTHREAD.c_asign_estr + "\" oi_instance=\"" + pINST.Id + "\" />");
          for (ETTY = xmlNODE.FirstChild(); ETTY != null; ETTY = ETTY.Next())
          {
            count++;
            if (ConfigureUSER(hUSER, pTHREAD, "ASIGN", ETTY.GetAttr("oi_entidad"), ETTY.GetAttr("d_entidad_estr"), "SI", "SI"))
              change = true;
          }
        } else
          if (pTHREAD.c_asign_rol!="")
          { //Hay un entidad en particular asignada
            NomadXML xmlNODE = NomadProxy.GetProxy().SQLService().GetXML("CLASS.NucleusWF.Base.Ejecucion.Instancias.INSTANCE.qry_user_rol", "<DATA c_rol=\"" + pTHREAD.c_asign_rol + "\" oi_instance=\"" + pINST.Id + "\" />");
            for (ETTY = xmlNODE.FirstChild(); ETTY != null; ETTY = ETTY.Next())
            {
              count++;
              if (ConfigureUSER(hUSER, pTHREAD, "ASIGN", ETTY.GetAttr("oi_entidad"), ETTY.GetAttr("d_entidad_estr"), "SI", "SI"))
                change = true;
            }
          } else //Recorro los ROLES y VEO QUIEN PUEDE RESOLVERLO.
          {
            NomadXML xmlNODE = NomadProxy.GetProxy().SQLService().GetXML("CLASS.NucleusWF.Base.Ejecucion.Instancias.INSTANCE.qry_user", "<DATA oi_node=\"" + pTHREAD.oi_node + "\" oi_instance=\"" + pINST.Id + "\" />");
            for (ROLE = xmlNODE.FirstChild(); ROLE != null; ROLE = ROLE.Next())
            {
              bool add=false;

              //Verifico si pruede Resolver
              if (ROLE.GetAttr("c_resolver") == "SI") add=true;
              if (!pTHREAD.f_vencimientoNull && ROLE.GetAttr("c_resolver")=="AV") add=true;
              if (!pTHREAD.f_advertenciaNull && ROLE.GetAttr("c_resolver")=="PV") add=true;

              //Verifico si pruede Resolver
              if (ROLE.GetAttr("c_notificar") == "SI") add = true;
              if (!pTHREAD.f_vencimientoNull && ROLE.GetAttr("c_resolver") == "AV") add = true;
              if (!pTHREAD.f_advertenciaNull && ROLE.GetAttr("c_resolver") == "PV") add = true;

              //No es necesario agregarlo
              if (!add) continue;
              if (ROLE.GetAttr("c_rol")=="OWNER")
              {
                count++;
                if (ConfigureUSER(hUSER, pTHREAD, "OWNER", pINST.d_owner, pINST.d_owner_ayn, ROLE.GetAttr("c_resolver"), ROLE.GetAttr("c_notificar")))
                  change = true;
              } else //Lista de usuarios
                for (ETTY=ROLE.FirstChild(); ETTY!=null; ETTY=ETTY.Next())
                {
                  count++;
                  if (ConfigureUSER(hUSER, pTHREAD, ROLE.GetAttr("c_rol"), ETTY.GetAttr("oi_entidad"), ETTY.GetAttr("d_entidad_estr"), ROLE.GetAttr("c_resolver"), ROLE.GetAttr("c_notificar")))
                    change = true;
                }
            }
          }

      //Elimino los Roles que no van mas
      foreach (USER curUSER in hUSER.Values)
      {
        change=true;
        pTHREAD.USERS.Remove(curUSER);
      }

      //No hay usuarios que puedan Resolver este WF, hay que marcarlo con ERROR....
      if (count==0)
      {
        SetError(pINST, null, "No hay personas que puedan resolverlo");
      }

      //Indica si cambio algo....
      return change;
    }

    public static void SendMail(int oi_thread, int oi_node, string caso)
    {
      try
      {
        //Obtengo los DDOs
        THREAD theTHREAD = THREAD.Get(oi_thread.ToString(), false);
        INSTANCE theINST = INSTANCE.Get(theTHREAD.oi_instance, false);
        theTHREAD = (NucleusWF.Base.Ejecucion.Instancias.THREAD)theINST.THREADS.GetById(oi_thread.ToString());
      } catch (Exception E)
      {
        NomadException MyEX=NomadException.NewInternalException("WF-SendMail-ERROR", E);
        MyEX.SetValue("oi_thread", oi_thread.ToString());
        MyEX.Dump();

        NomadLog.Error(MyEX.Message+" - "+MyEX.Id);
        return;
      }
    }

    public static void SendMail(INSTANCE theINST, THREAD theTHREAD, int oi_node, string caso)
    {
      try
      {

        //Validacion
        Validar(theINST, "R", theTHREAD, "WP", -1, true);

        //Valido que el THREAD este en ese NODO
        if (theTHREAD.oi_node!=oi_node.ToString())
          throw new Exception("El nodo ya no esta activo");

        //Obtengo la Fecha segun el CASO
        bool TimeValid=false;
        DateTime myTime;
        switch (caso)
        {
          case "A": myTime=theTHREAD.f_advertencia; TimeValid=theTHREAD.f_advertenciaNull?false:true; break;
          case "V": myTime=theTHREAD.f_vencimiento; TimeValid=theTHREAD.f_vencimientoNull?false:true; break;
          default: myTime=theTHREAD.f_ejecucion; TimeValid=true; break;
        }

        //Valido la FECHA
        if (!TimeValid)
          throw new Exception("No hay fecha de advertencia Valida");

        if (myTime>DateTime.Now)
          throw new Exception("La Fecha de Aviso es FUTURA");

        //Obtengo el NODO
        NomadXML xmlNODE=NomadProxy.GetProxy().SQLService().GetXML("CLASS.NucleusWF.Base.Ejecucion.Instancias.INSTANCE.qry_mail", "<DATA oi_node=\""+theTHREAD.oi_node+"\" />");

        //Obtengo la Definicion del WF
        NucleusWF.Base.Definicion.Workflows.WF defWF=INSTANCE.GetWFDef(theINST.oi_wf);

        //Inicializo las Variables
        NomadXML xmlVARIABLES=GetVariables(theINST, int.Parse(theTHREAD.Id));

        string SUBJECT="";
        string BODY   ="";
        string TOBODY = "";
        string TYPE   ="";
        Nomad.Base.Login.Entidades.ENTIDAD MyETTY;

        //Obtengo el SUBJECT
        if (xmlNODE.GetAttr("d_param_2")!="")
        {
          //Proceso el TEXTO
          SUBJECT=Nomad.Base.Report.Text.Resolve(xmlNODE.GetAttr("d_param_2"), xmlVARIABLES);
          if (SUBJECT==null)
            throw new Exception("Error al Procesar el TEXT '"+xmlNODE.GetAttr("d_param_2")+"'.");
          NomadLog.Info("SUBJECT:"+SUBJECT);
        }

        //Obtengo el BODY
        if (xmlNODE.GetAttr("d_param_3")!="")
        {
          NucleusWF.Base.Definicion.Workflows.TEXT txt=(NucleusWF.Base.Definicion.Workflows.TEXT)defWF.NODE_TEXTS.GetByAttribute("c_text", xmlNODE.GetAttr("d_param_3"));
          if (txt==null)
            throw new Exception("No se encontro el TEXT '"+xmlNODE.GetAttr("d_param_3")+"' en la Definicion.");

          //Tipo de CUERPO
          TYPE=txt.c_tipo;

          //Proceso el TEXTO
          BODY=txt.GetTextResult(xmlVARIABLES);
          if (BODY==null)
            throw new Exception("Error al Procesar el TEXT '"+xmlNODE.GetAttr("d_param_3")+"'.");
          NomadLog.Info("BODY:"+BODY);
        }

        //Resultado
        if (SUBJECT!="" && BODY!="" && TYPE!="")
        {
          Nomad.Base.Mail.Mails.MAIL objMAIL=Nomad.Base.Mail.Mails.MAIL.Crear();

          //Agregar los PARAMETROS
          objMAIL.REMITENTE_DES=theINST.d_owner_ayn;
          objMAIL.REMITENTE_DIR=theINST.d_owner_mail;

          //Asunto
          objMAIL.ASUNTO=(theINST.l_debug?"[DEBUG]"+SUBJECT:SUBJECT);
          objMAIL.PRIORIDAD=5;

          //Obtengo la lista de personas para enviar el MAIL.....!!!!
          Hashtable MAIL = new Hashtable();

          DateTime MyNOW = DateTime.Now;
          foreach (USER MyUser in theTHREAD.USERS)
          {
            if (!MyUser.l_notify) continue;
            if (MyUser.f_notify > MyNOW) continue;
            MAIL[MyUser.oi_etty] = 1;
          }

          if (theINST.l_debug)
          {
            if (TYPE == "TXT") TOBODY="MAIL-TO:\n";
            else TOBODY="**MAIL-TO**:\n";

            //Agrego el MAIL del generador
            if (theINST.d_etty_creator!="")
            {
              MyETTY = Nomad.Base.Login.Entidades.ENTIDAD.Get(theINST.d_etty_creator);
              foreach (Nomad.Base.Login.Entidades.MAIL MyMAIL in MyETTY.MAILS)
              {
                if (!MyMAIL.PRINCIPAL) continue;
                Nomad.Base.Mail.Mails.MAIL.AgregarDestinatario(objMAIL, MyMAIL.EMAIL, MyETTY.DES);
              }
            }
          }

          //Lista de MAILS
          foreach (string oi_entidad in MAIL.Keys)
          {
            MyETTY = Nomad.Base.Login.Entidades.ENTIDAD.Get(oi_entidad);

            foreach (Nomad.Base.Login.Entidades.MAIL MyMAIL in MyETTY.MAILS)
            {
              if (!MyMAIL.PRINCIPAL) continue;
              if (theINST.l_debug) TOBODY+="  * "+MyETTY.DES+"("+MyMAIL.EMAIL+")\n";
              else Nomad.Base.Mail.Mails.MAIL.AgregarDestinatario(objMAIL, MyMAIL.EMAIL, MyETTY.DES);
            }
          }

          if (theINST.l_debug)
          {
            if (TYPE == "TXT") TOBODY+="--------------------------------------------------------------------\n";
            else TOBODY+="---\n";
          }

          if (TYPE=="TXT")
          {
            objMAIL.TIPO="T";
            objMAIL.CUERPO_TXT=TOBODY+BODY;
          } else
          {
            objMAIL.TIPO="W";
            objMAIL.CUERPO=TOBODY+BODY;
          }

          //Envio el MAIL
          if (objMAIL.DESTINATARIOS.Count>0)
            objMAIL.Enviar();
        }

      } catch (Exception E)
      {
        NomadException MyEX=NomadException.NewInternalException("WF-SendMail-ERROR", E);
        MyEX.Dump();

        NomadLog.Error(MyEX.Message+" - "+MyEX.Id);
        return;
      }
    }

    public static void SendMail(NucleusWF.Base.Ejecucion.Instancias.INSTANCE theINST, int oi_thread, NomadXML xmlNODE)
    {
      //Obtengo la Definicion del WF
      NucleusWF.Base.Definicion.Workflows.WF defWF = INSTANCE.GetWFDef(theINST.oi_wf);

      //Inicializo las Variables
      NomadXML xmlVARIABLES = GetVariables(theINST, oi_thread);

      string SUBJECT = "";
      string BODY = "";
      string TOBODY = "";
      string TYPE = "";

      //Proceso el TEXTO
      SUBJECT = Nomad.Base.Report.Text.Resolve(xmlNODE.GetAttr("d_param_1"), xmlVARIABLES);
      if (SUBJECT == null)
        throw new Exception("Error al Procesar el TEXT '" + xmlNODE.GetAttr("d_param_1") + "'.");
      NomadLog.Info("SUBJECT:" + SUBJECT);

      NucleusWF.Base.Definicion.Workflows.TEXT txt = (NucleusWF.Base.Definicion.Workflows.TEXT)defWF.NODE_TEXTS.GetByAttribute("c_text", xmlNODE.GetAttr("d_param_2"));
      if (txt == null)
        throw new Exception("No se encontro el TEXT '" + xmlNODE.GetAttr("d_param_2") + "' en la Definicion.");

      //Tipo de CUERPO
      TYPE = txt.c_tipo;

      //Proceso el TEXTO
      BODY = txt.GetTextResult(xmlVARIABLES);
      if (BODY == null)
        throw new Exception("Error al Procesar el TEXT '" + xmlNODE.GetAttr("d_param_2") + "'.");
      NomadLog.Info("BODY:" + BODY);

      Nomad.Base.Mail.Mails.MAIL objMAIL = Nomad.Base.Mail.Mails.MAIL.Crear();

      //Remitente  
	  switch (xmlNODE.GetAttr("d_param_3"))
	  {
		case "OWN":
			objMAIL.REMITENTE_DIR = theINST.d_owner_mail;
			objMAIL.REMITENTE_DES = theINST.d_owner_ayn;
			break;
		case "SYS":		
			objMAIL.REMITENTE_DIR = GetMailContext().GetAttr("user-from");
			objMAIL.REMITENTE_DES = objMAIL.REMITENTE_DIR;
			break;
		case "CUS":
			objMAIL.REMITENTE_DIR = xmlNODE.GetAttr("d_param_4");
			objMAIL.REMITENTE_DES = objMAIL.REMITENTE_DIR;
			break;
		default:
			objMAIL.REMITENTE_DIR = "nomad@nucleussa.com.ar";
			objMAIL.REMITENTE_DES = "nomad";		
			break;
	  }

      //Asunto
      objMAIL.ASUNTO = (theINST.l_debug?"[DEBUG]"+SUBJECT:SUBJECT);
      objMAIL.PRIORIDAD = 5;

      NomadXML xmlQRY;
      Nomad.Base.Login.Entidades.ENTIDAD MyETTY;
      Hashtable MAIL = new Hashtable();
      ArrayList oi_entity = new ArrayList();

      if (theINST.l_debug)
      {
        if (TYPE == "TXT") TOBODY="MAIL-TO:\n";
        else TOBODY="**MAIL-TO**:\n";

        //Agrego el MAIL del generador
        if (theINST.d_etty_creator != "")
        {
          MyETTY = Nomad.Base.Login.Entidades.ENTIDAD.Get(theINST.d_etty_creator);
          foreach (Nomad.Base.Login.Entidades.MAIL MyMAIL in MyETTY.MAILS)
          {
            if (!MyMAIL.PRINCIPAL) continue;
            Nomad.Base.Mail.Mails.MAIL.AgregarDestinatario(objMAIL, MyMAIL.EMAIL, MyETTY.DES);
          }
        }
      }

      for (NomadXML cur = xmlNODE.FirstChild(); cur != null; cur = cur.Next())
      {
        oi_entity.Clear();
        switch (cur.GetAttr("c_type"))
        {
          case "OWNER":
            oi_entity.Add(theINST.d_owner);
            break;

          case "ETTY":
            oi_entity.Add(cur.GetAttr("d_valor"));
            break;

          case "ROLE":
            //Obtengo el NODO
            xmlQRY = NomadProxy.GetProxy().SQLService().GetXML("CLASS.NucleusWF.Base.Ejecucion.Instancias.INSTANCE.qry_entidades", "<DATA c_rol=\"" + cur.GetAttr("d_valor") + "\" oi_instance=\"" + theINST.Id + "\" />");

            for (NomadXML ett=xmlQRY.FirstChild(); ett!=null; ett=ett.Next())
            {
              if (ett.GetAttr("oi_entidad")!="")
                oi_entity.Add(ett.GetAttr("oi_entidad"));
            }
            break;
        }

        foreach (string etty in oi_entity)
        {
          MyETTY = Nomad.Base.Login.Entidades.ENTIDAD.Get(etty);

          //Agrego la Entidad
          foreach (Nomad.Base.Login.Entidades.MAIL MyMAIL in MyETTY.MAILS)
          {
            if (!MyMAIL.PRINCIPAL) continue;
            if (MAIL.ContainsKey(MyMAIL.EMAIL.ToUpper())) continue;

            if (theINST.l_debug) TOBODY+="  * "+MyETTY.DES+"("+MyMAIL.EMAIL+")\n";
            else Nomad.Base.Mail.Mails.MAIL.AgregarDestinatario(objMAIL, MyMAIL.EMAIL, MyETTY.DES);
            MAIL[MyMAIL.EMAIL.ToUpper()] = 1;
          }
        }
      }

      int t;
      string mails;
      string[] pMails;
      for (NomadXML cur = xmlNODE.FirstChild(); cur != null; cur = cur.Next())
      {
        mails = "";
        switch (cur.GetAttr("c_type"))
        {
          case "MAIL":
            mails = cur.GetAttr("d_valor");
            break;
        }

        if (mails == "") continue;

        pMails = mails.Split(';');
        for (t = 0; t < pMails.Length; t++)
        {
          if (pMails[t] != "") continue;
          if (MAIL.ContainsKey(pMails[t].ToUpper())) continue;

          if (theINST.l_debug) TOBODY+="  * "+pMails[t]+"\n";
          else Nomad.Base.Mail.Mails.MAIL.AgregarDestinatario(objMAIL, pMails[t], pMails[t]);
          MAIL[pMails[t].ToUpper()] = 1;
        }
      }

      if (theINST.l_debug)
      {
        if (TYPE == "TXT") TOBODY+="--------------------------------------------------------------------\n";
        else TOBODY+="---\n";
      }

      if (TYPE == "TXT")
      {
        objMAIL.TIPO = "T";
        objMAIL.CUERPO_TXT = TOBODY+BODY;
      } else
      {
        objMAIL.TIPO = "W";
        objMAIL.CUERPO = TOBODY+BODY;
      }

      objMAIL.Enviar();
    }
	
	private static NomadXML GetMailContext()
    {
      NomadXML xmlContext;

      //Pide el context para obtener los parametros
      xmlContext = NomadProxy.GetProxy().ReadContext();
      xmlContext = xmlContext.FindElement("config");

      if (xmlContext != null) xmlContext = xmlContext.FindElement("mail");

      if (xmlContext == null)
      {
        NomadLog.Debug("No se pudo obtener el archivo de contexto.");
        throw new Exception("No se pudo obtener el archivo de contexto.");
      }

      return xmlContext;
    }

    public static void Procesar(int oi_thread)
    {
      string ThreadEstado;
      //Obtengo los DDOs
      NucleusWF.Base.Ejecucion.Instancias.INSTANCE extINST=null;
      NucleusWF.Base.Ejecucion.Instancias.THREAD theTHREAD=NucleusWF.Base.Ejecucion.Instancias.THREAD.Get(oi_thread.ToString(), false);
      NucleusWF.Base.Ejecucion.Instancias.INSTANCE theINST=NucleusWF.Base.Ejecucion.Instancias.INSTANCE.Get(theTHREAD.oi_instance, false);
      NucleusWF.Base.Definicion.Workflows.WF defWF = null;
      NucleusWF.Base.Definicion.Workflows.ESTADO defESTADO = null;
      NomadXML xmlVARIABLES = null;
      NomadXML xmlPARAMS = null;
      do
      {
        do
        {
          theTHREAD=(NucleusWF.Base.Ejecucion.Instancias.THREAD)theINST.THREADS.GetById(oi_thread.ToString());

          //Analizo el ESTADO
          ThreadEstado=theTHREAD.c_estado;

          //Validacion
          try
          {
            Validar(theINST, "R", theTHREAD, "WE,WI,WX", -1, true);
          } catch (Exception E) { NomadLog.Error(E.Message); return; }

          //Obtengo el NODO
          NomadXML xmlINFO=NomadProxy.GetProxy().SQLService().GetXML("CLASS.NucleusWF.Base.Ejecucion.Instancias.INSTANCE.qry_proceso", "<DATA oi_node=\""+theTHREAD.oi_node+"\" />");
          NomadXML xmlNODE=xmlINFO.FindElement("NODE");
          NomadXML xmlPROC=xmlINFO.FindElement("PROCESS");

          //Cambio el ESTADO y limpio la lista de usuario que pueden resolverlo
          theINST.e_pasos++;
          theTHREAD.c_estado="R";
          RefreshUSERS(theINST, theTHREAD);

          AppendLog(theINST, theTHREAD, "Procesando el Nodo '"+xmlNODE.GetAttr("d_label")+"'...");
          NomadEnvironment.GetCurrentTransaction().SaveRefresh(theINST);
          theTHREAD=(NucleusWF.Base.Ejecucion.Instancias.THREAD)theINST.THREADS.GetById(oi_thread.ToString());
          NomadLog.Info("El Thread ha sido Bloqueado, paso a estado 'R'.");

          //Variables
          string result, execute;

          if (ThreadEstado=="WX")
          {
            //Agrego LOG
            NomadLog.Info("Ignorando el Nodo '"+xmlNODE.GetAttr("d_label")+"'.");

            //Proximo Paso
            SetInfo(theINST, theTHREAD, xmlNODE.GetAttr("d_label")+" - Ignorado.");
            NextStep(theINST, theTHREAD, xmlINFO);
          } else
          {

            //Agrego LOG
            NomadLog.Info("Ejecutando Nodo '"+xmlNODE.GetAttr("d_label")+"'.");

            try
            {
              //Proceso
              switch (xmlNODE.GetAttr("c_type"))
              {

                ////////////////////////////////////////////////////////////////////////////////////////////////////
                case "INI":
                  if (theINST.e_pasos==1)
                  { //Primera VEZ
                    if (xmlNODE.ChildLength!=0)
                      throw new Exception("El tipo de NODO 'INI' con PARAMETROS no esta IMPLEMENTADO.");

                    //Obtengo la Definicion del WF
                    defWF=INSTANCE.GetWFDef(theINST.oi_wf);

                    //Inicializo las Variables
                    xmlVARIABLES=GetVariables(theINST, oi_thread);

                    foreach (NucleusWF.Base.Definicion.Workflows.VAR_GROUP defGRP in defWF.GROUPS)
                    {
                      switch (defGRP.c_mode)
                      {
                        case "MAIN":
                          foreach (NucleusWF.Base.Definicion.Workflows.VAR defVAR in defGRP.VARS)
                          {
                            if (defVAR.o_compilada!="")
                            {
                              execute=defVAR.o_compilada;

                              //Inicializo las Variables
                              NomadLog.Info("EXECUTE: "+execute);
                              result=Nomad.Base.Report.Formula.Resolve(execute, null, xmlVARIABLES, false);
                              NomadLog.Info("RESULT: "+result);

                              switch (result.Substring(0, 1))
                              {
                                case "B": Nomad.Base.Report.Variables.SetValue(xmlVARIABLES, -1, defGRP.c_var_group+"."+defVAR.c_var, "bool", result.Substring(2), null); break;
                                case "I": Nomad.Base.Report.Variables.SetValue(xmlVARIABLES, -1, defGRP.c_var_group+"."+defVAR.c_var, "int", result.Substring(2), null); break;
                                case "N": Nomad.Base.Report.Variables.SetValue(xmlVARIABLES, -1, defGRP.c_var_group+"."+defVAR.c_var, "double", result.Substring(2), null); break;
                                case "D": Nomad.Base.Report.Variables.SetValue(xmlVARIABLES, -1, defGRP.c_var_group+"."+defVAR.c_var, "date", result.Substring(2), null); break;
                                case "S": Nomad.Base.Report.Variables.SetValue(xmlVARIABLES, -1, defGRP.c_var_group+"."+defVAR.c_var, "string", result.Substring(2), null); break;
                                case "0": Nomad.Base.Report.Variables.SetValue(xmlVARIABLES, -1, defGRP.c_var_group+"."+defVAR.c_var, "null", "", null); break;
                                case "E": throw new Exception("Error en la Formula, "+result.Substring(2));
                                default: throw new Exception("Tipo de Resultado no VALIDO.");
                              }
                            }
                          }
                          break;

                        case "HIST":
                          foreach (NucleusWF.Base.Definicion.Workflows.VAR defVAR in defGRP.VARS)
                          {
                            if (defVAR.o_compilada!="")
                            {
                              execute=defVAR.o_compilada;

                              //Inicializo las Variables
                              NomadLog.Info("EXECUTE: "+execute);
                              result=Nomad.Base.Report.Formula.Resolve(execute, null, xmlVARIABLES, false);
                              NomadLog.Info("RESULT: "+result);

                              switch (result.Substring(0, 1))
                              {
                                case "B": Nomad.Base.Report.Variables.SetValue(xmlVARIABLES, 0, defGRP.c_var_group+"."+defVAR.c_var, "bool", result.Substring(2), null); break;
                                case "I": Nomad.Base.Report.Variables.SetValue(xmlVARIABLES, 0, defGRP.c_var_group+"."+defVAR.c_var, "int", result.Substring(2), null); break;
                                case "N": Nomad.Base.Report.Variables.SetValue(xmlVARIABLES, 0, defGRP.c_var_group+"."+defVAR.c_var, "double", result.Substring(2), null); break;
                                case "D": Nomad.Base.Report.Variables.SetValue(xmlVARIABLES, 0, defGRP.c_var_group+"."+defVAR.c_var, "date", result.Substring(2), null); break;
                                case "S": Nomad.Base.Report.Variables.SetValue(xmlVARIABLES, 0, defGRP.c_var_group+"."+defVAR.c_var, "string", result.Substring(2), null); break;
                                case "0": Nomad.Base.Report.Variables.SetValue(xmlVARIABLES, 0, defGRP.c_var_group+"."+defVAR.c_var, "null", "", null); break;
                                case "E": throw new Exception("Error en la Formula, "+result.Substring(2));
                                default: throw new Exception("Tipo de Resultado no VALIDO.");
                              }
                            }
                          }
                          break;
                      }
                    }

                    //Actualizo Variables
                    SetVariables(theINST, oi_thread, xmlVARIABLES);

                  } else
                  {  //No es el Primer paso, tiene que se una llamada a un Subproceso
                    if (xmlNODE.ChildLength!=0)
                      throw new Exception("El tipo de NODO 'INI' de un SubProceso no puede tener PARAMETROS.");
                  }

                  //Evento Proximo
                  if (xmlNODE.GetAttr("o_compprocess")!="")
                  {
                    //Obtengo la Definicion del WF
                    defWF = INSTANCE.GetWFDef(theINST.oi_wf);

                    //Inicializo las Variables
                    if (xmlVARIABLES==null) xmlVARIABLES=GetVariables(theINST, oi_thread);

                    //Parametros de la Instancia
                    xmlPARAMS=GetParametros(theINST);

                    //Ejecuto el Resultado
                    execute=xmlNODE.GetAttr("o_compprocess");
                    NomadLog.Info("EXECUTE: "+execute);
                    result=Nomad.Base.Report.Formula.Resolve(execute, xmlPARAMS, xmlVARIABLES, false);
                    NomadLog.Info("RESULT: "+result);

                    //Analizo el Resultado
                    if (result.Substring(0, 1)=="E")
                      throw new Exception("Error en el Procedimiento, "+result.Substring(2));

                    //Actualizo los Parametros
                    SetParametros(theINST, xmlPARAMS);

                    //Actualizo Variables
                    SetVariables(theINST, oi_thread, xmlVARIABLES);
                  }

                  //Proximo Paso
                  SetInfo(theINST, theTHREAD, xmlNODE.GetAttr("d_label")+" - Procesado OK.");
                  NextStep(theINST, theTHREAD, xmlINFO);
                  break;

                case "FIN":
                  if (theTHREAD.oi_thread_parent!="" && xmlNODE.ChildLength!=0)
                    throw new Exception("El tipo de NODO 'FIN' con PARAMETROS no puede ser llamado como subproceso.");

                  //Evento Proximo
                  if (xmlNODE.GetAttr("o_compprocess")!="")
                  {
                    //Obtengo la Definicion del WF
                    defWF = INSTANCE.GetWFDef(theINST.oi_wf);

                    //Inicializo las Variables
                    xmlVARIABLES=GetVariables(theINST, oi_thread);

                    //Parametros de la Instancia
                    xmlPARAMS=GetParametros(theINST);

                    //Ejecuto el Resultado
                    execute=xmlNODE.GetAttr("o_compprocess");
                    NomadLog.Info("EXECUTE: "+execute);
                    result=Nomad.Base.Report.Formula.Resolve(execute, xmlPARAMS, xmlVARIABLES, false);
                    NomadLog.Info("RESULT: "+result);

                    //Analizo el Resultado
                    if (result.Substring(0, 1)=="E")
                      throw new Exception("Error en el Procedimiento, "+result.Substring(2));

                    //Actualizo los Parametros
                    SetParametros(theINST, xmlPARAMS);

                    //Actualizo Variables
                    SetVariables(theINST, oi_thread, xmlVARIABLES);
                  }

                  if (theTHREAD.oi_thread_parent=="")
                  { //FIN FIN
                    SetInfo(theINST, theTHREAD, "FIN del Proceso.");

                    //Finalizando el Proceso
                    theINST.THREADS.Clear();
                    theINST.c_estado_ej="F";
                    theINST.f_fin=DateTime.Now;

                    //Busco Procesos Padres y los Activo!!!!
                    NomadXML xmlTHREADS=NomadProxy.GetProxy().SQLService().GetXML("CLASS.NucleusWF.Base.Ejecucion.Instancias.INSTANCE.qry_threads", "<DATA oi_instance=\""+theINST.Id+"\" />");
                    if (xmlTHREADS.GetAttr("oi_thread")!="")
                    {
                      //Obtengo el THREAD y lo ACTIVO
                      extINST=NucleusWF.Base.Ejecucion.Instancias.INSTANCE.Get(xmlTHREADS.GetAttr("oi_instance"), false);
                      NucleusWF.Base.Ejecucion.Instancias.THREAD th=(NucleusWF.Base.Ejecucion.Instancias.THREAD)extINST.THREADS.GetById(xmlTHREADS.GetAttr("oi_thread"));

                      //Cambio el Estado
                      th.c_estado="WE";
                      th.oi_thread_parentNull=true;
                      th.f_ejecucion=DateTime.Now;

                      //Guardo el WF
                      NomadEnvironment.GetCurrentTransaction().SaveRefresh(extINST);
                    }

                  } else
                  { //Fin de un SUBPROCESO
                    SetInfo(theINST, theTHREAD, "FIN del Subproceso '"+xmlPROC.GetAttr("d_process")+"'.");
                    NextStep(theINST, theTHREAD, xmlINFO);
                  }
                  break;

                ////////////////////////////////////////////////////////////////////////////////////////////////////
                case "LOG":
                  //Obtengo la Definicion del WF
                  defWF=INSTANCE.GetWFDef(theINST.oi_wf);

                  //Inicializo las Variables
                  xmlVARIABLES=GetVariables(theINST, oi_thread);

                  //Obtengo las TEXT
                  NucleusWF.Base.Definicion.Workflows.TEXT txt=(NucleusWF.Base.Definicion.Workflows.TEXT)defWF.NODE_TEXTS.GetByAttribute("c_text", xmlNODE.GetAttr("d_param_1"));
                  if (txt==null)
                    throw new Exception("No se encontro el TEXT '"+xmlNODE.GetAttr("d_param_1")+"' en la Definicion.");

                  //Proceso el TEXTO
                  result=txt.GetTextResult(xmlVARIABLES);
                  if (result==null)
                    throw new Exception("Error al Procesar el TEXT '"+xmlNODE.GetAttr("d_param_1")+"'.");

                  //Proximo Paso
                  SetInfo(theINST, theTHREAD, result);
                  NextStep(theINST, theTHREAD, xmlINFO);
                  break;

                ////////////////////////////////////////////////////////////////////////////////////////////////////
                case "STATUS":
                  //Obtengo la Definicion del WF
                  defWF=INSTANCE.GetWFDef(theINST.oi_wf);

                  //Busco el ESTADO
                  defESTADO=(NucleusWF.Base.Definicion.Workflows.ESTADO)defWF.ESTADOS.GetByAttribute("c_estado", xmlNODE.GetAttr("d_param_1"));
                  if (defESTADO==null)
                    throw new Exception("No se encontro el ESTADO '"+xmlNODE.GetAttr("d_param_1")+"' en la Definicion.");

                  //Eliminacion Automatica?
                  if (defESTADO.n_horas_delete>0)
                  {
                    //Programo la Eliminacion Automatica
                    RPCService MyRPC=NomadProxy.GetProxy().RPCService();
                    MyRPC.AddParam(new RPCParam("oi_instance", "IN", int.Parse(theINST.Id)));
                    MyRPC.AddParam(new RPCParam("estado", "IN", xmlNODE.GetAttr("d_param_1")));
                    MyRPC.Execute(NomadProxy.GetProxy().AppName+"-WF-AUTODELETE-"+theINST.Id, 1, DateTime.Now.AddHours(defESTADO.n_horas_delete), "WORKFLOW", "NucleusWF.Base.Ejecucion.Instancias.INSTANCE", "AutoDelete");
                  }

                  result=theINST.c_estado_wf;
                  theINST.c_estado_wf=xmlNODE.GetAttr("d_param_1");

                  //Inicializo las Variables
                  xmlVARIABLES = GetVariables(theINST, oi_thread);
                  SetVariables(theINST, oi_thread, xmlVARIABLES);

                  //Proximo Paso
                  SetInfo(theINST, theTHREAD, (result==""?"Establesco el Estado '"+xmlNODE.GetAttr("d_param_1")+"'.":"Cambio el estado '"+result+"' -> '"+xmlNODE.GetAttr("d_param_1")+"'."));
                  NextStep(theINST, theTHREAD, xmlINFO);
                  break;

                ////////////////////////////////////////////////////////////////////////////////////////////////////
                case "VENC":
                  //Obtengo la Definicion del WF
                  defWF = INSTANCE.GetWFDef(theINST.oi_wf);

                  switch (xmlNODE.GetAttr("d_param_1"))
                  {
                    case "RESET":
                      theINST.f_advertencia = theINST.f_inicio.AddHours(defWF.n_horas * 0.60);
                      theINST.f_vencimiento = theINST.f_inicio.AddHours(defWF.n_horas);
                      break;

                    case "RECALC":
                      theINST.f_advertencia = DateTime.Now.AddHours(defWF.n_horas * 0.60);
                      theINST.f_vencimiento = DateTime.Now.AddHours(defWF.n_horas);
                      break;

                    case "CALC":
                      //Inicializo las Variables
                      xmlVARIABLES = GetVariables(theINST, oi_thread);

                      //Inicializo las Variables
                      execute=xmlNODE.GetAttr("o_compformula");
                      NomadLog.Info("EXECUTE: " + execute);
                      result = Nomad.Base.Report.Formula.Resolve(execute, null, xmlVARIABLES, false);
                      NomadLog.Info("RESULT: " + result);

                      switch (result.Substring(0, 1))
                      {
                        case "D":
                          DateTime MyNow=DateTime.Now;

                          //Calculo la Fecha de Vencimeinto
                          theINST.f_vencimiento = StringUtil.str2date(result.Substring(2));

                          if (((TimeSpan)(theINST.f_vencimiento - MyNow)).TotalHours > 0)
                          {
                            theINST.f_advertencia = MyNow.AddHours(((TimeSpan)(theINST.f_vencimiento - MyNow)).TotalHours * 0.60);
                          } else
                            theINST.f_advertencia = theINST.f_vencimiento;

                          break;

                        case "E": throw new Exception("Error en la Formula, " + result.Substring(2));
                        default: throw new Exception("Tipo de Resultado no VALIDO.");
                      }

                      break;
                  }

                  //Proximo Paso
                  SetInfo(theINST, theTHREAD, xmlNODE.GetAttr("d_label") + " - Procesado OK - Nuevo Vencimiento: " + theINST.f_vencimiento.ToString("dd/MM/yyyy HH:mm"));
                  NextStep(theINST, theTHREAD, xmlINFO);
                  break;

                ////////////////////////////////////////////////////////////////////////////////////////////////////
                case "VAR":
                  //Obtengo la Definicion del WF
                  defWF=INSTANCE.GetWFDef(theINST.oi_wf);

                  //Inicializo las Variables
                  xmlVARIABLES=GetVariables(theINST, oi_thread);

                  //Ejecuto el Resultado
                  execute=xmlNODE.GetAttr("o_compprocess");
                  NomadLog.Info("EXECUTE: "+execute);
                  result=Nomad.Base.Report.Formula.Resolve(execute, null, xmlVARIABLES, false);
                  NomadLog.Info("RESULT: "+result);

                  //Analizo el Resultado
                  if (result.Substring(0, 1)=="E")
                    throw new Exception("Error en el Procedimiento, "+result.Substring(2));

                  SetVariables(theINST, oi_thread, xmlVARIABLES);

                  //Proximo Paso
                  SetInfo(theINST, theTHREAD, xmlNODE.GetAttr("d_label")+" - Procesado OK.");
                  NextStep(theINST, theTHREAD, xmlINFO);
                  break;

                case "FORM":
                  if (ThreadEstado=="WE")
                  {
                    //Evento Proximo
                    if (xmlNODE.GetAttr("o_compprocess")!="")
                    {
                      //Obtengo la Definicion del WF
                      defWF=INSTANCE.GetWFDef(theINST.oi_wf);

                      //Inicializo las Variables
                      xmlVARIABLES=GetVariables(theINST, oi_thread);

                      //Ejecuto el Resultado
                      execute=xmlNODE.GetAttr("o_compprocess");
                      NomadLog.Info("EXECUTE: "+execute);
                      result=Nomad.Base.Report.Formula.Resolve(execute, null, xmlVARIABLES, false);
                      NomadLog.Info("RESULT: "+result);

                      //Analizo el Resultado
                      if (result.Substring(0, 1)=="E")
                        throw new Exception("Error en el Procedimiento, "+result.Substring(2));

                      //Actualizo las Variables
                      SetVariables(theINST, oi_thread, xmlVARIABLES);
                    }

                    //Proximo Paso
                    SetInfo(theINST, theTHREAD, xmlNODE.GetAttr("d_label")+" - Procesado OK.");
                    NextStep(theINST, theTHREAD, xmlINFO);
                  } else
                  {
                    SetInfo(theINST, theTHREAD, xmlNODE.GetAttr("d_label")+" - Esperando la Resolucion del Usuario.");

                    //Activo el FORM!!!!
                    theTHREAD=(NucleusWF.Base.Ejecucion.Instancias.THREAD)theINST.THREADS.GetById(oi_thread.ToString());
                    theTHREAD.c_estado="WP";
                    theTHREAD.f_ejecucion=DateTime.Now;

                    //Agrego el elemento historico
                    AppendHistVariables(theINST);

                    //Calcula las fechas de Vencimiento
                    if (xmlNODE.GetAttrInt("n_horas")>0)
                    { //tiene Definido una cantidad de horas de vencimiento
                      theTHREAD.f_advertencia=DateTime.Now.AddHours(0.60*xmlNODE.GetAttrInt("n_horas"));
                      theTHREAD.f_vencimiento=DateTime.Now.AddHours(xmlNODE.GetAttrInt("n_horas"));

                      if (!theINST.f_advertenciaNull && theINST.f_advertencia<theTHREAD.f_advertencia)
                        theTHREAD.f_advertencia=theINST.f_advertencia;

                      if (!theINST.f_vencimientoNull && theINST.f_vencimiento<theTHREAD.f_vencimiento)
                        theTHREAD.f_vencimiento=theINST.f_vencimiento;

                    } else
                    {
                      theTHREAD.f_advertencia    =theINST.f_advertencia;
                      theTHREAD.f_advertenciaNull=theINST.f_advertenciaNull;
                      theTHREAD.f_vencimiento    =theINST.f_vencimiento;
                      theTHREAD.f_vencimientoNull=theINST.f_vencimientoNull;
                    }

                    //Actualizo la lista de USUARIOS
                    RefreshUSERS(theINST, theTHREAD);
                  }
                  break;

                ////////////////////////////////////////////////////////////////////////////////////////////////////
                // Ejecucion de WORKFLOW EXTERNOS
                case "CALL":
                  if (ThreadEstado=="WE")
                  {
                    //Obtengo la Definicion del WF
                    defWF=INSTANCE.GetWFDef(theINST.oi_wf);

                    //Parametros
                    xmlPARAMS=GetParametros(INSTANCE.Get(theTHREAD.oi_inst_child, false));

                    //Inicializo las Variables
                    xmlVARIABLES=GetVariables(theINST, oi_thread);

                    //Ejecuto el PROCESO OUT
                    execute=xmlNODE.FindElement2("PARAM", "c_type", "OUT").GetAttr("o_compilada");
                    NomadLog.Info("EXECUTE: "+execute);
                    result=Nomad.Base.Report.Formula.Resolve(execute, xmlPARAMS, xmlVARIABLES, false);
                    NomadLog.Info("RESULT: "+result);

                    if (result.Substring(0, 1)=="E")
                      throw new Exception("Error en el Procedimiento, "+result.Substring(2));

                    //Actualizo las Variables
                    SetVariables(theINST, oi_thread, xmlVARIABLES);

                    //Limpio el THREAD
                    theTHREAD.oi_inst_childNull=true;

                    //Proximo Paso
                    SetInfo(theINST, theTHREAD, xmlNODE.GetAttr("d_label")+" - Procesado OK.");
                    NextStep(theINST, theTHREAD, xmlINFO);
                  } else
                  {
                    //Obtengo la Definicion del WF
                    defWF=INSTANCE.GetWFDef(theINST.oi_wf);

                    NomadXML RS = NomadEnvironment.QueryNomadXML(NucleusWF.Base.Definicion.Workflows.WF.Resources.QRY_PROCESS, "<DATA c_wf=\""+xmlNODE.GetAttr("d_param_1")+"\" c_process=\""+xmlNODE.GetAttr("d_param_2")+"\" l_debug=\""+(theINST.l_debug?"1":"0")+"\" />").FirstChild();
                    if (RS.GetAttr("oi_process")=="")
                      throw new Exception("Error al Obtener los Parametros Externos");

                    //Parametros
                    NucleusWF.Base.Definicion.Workflows.WF defWFCall=NucleusWF.Base.Definicion.Workflows.WF.Get(RS.GetAttr("oi_wf"), false);
                    xmlPARAMS=NucleusWF.Base.Definicion.Workflows.PROCESS.GetVariables((NucleusWF.Base.Definicion.Workflows.PROCESS)defWFCall.PROCESS.GetById(RS.GetAttr("oi_process")));
                    if (xmlPARAMS==null)
                      throw new Exception("Error al Obtener los Parametros Externos");

                    //Inicializo las Variables
                    xmlVARIABLES=GetVariables(theINST, oi_thread);

                    //Ejecuto el PROCESO IN
                    execute=xmlNODE.FindElement2("PARAM", "c_type", "IN").GetAttr("o_compilada");
                    NomadLog.Info("EXECUTE: "+execute);
                    result=Nomad.Base.Report.Formula.Resolve(execute, xmlPARAMS, xmlVARIABLES, false);
                    NomadLog.Info("RESULT: "+result);

                    if (result.Substring(0, 1)=="E")
                      throw new Exception("Error en el Procedimiento, "+result.Substring(2));
                    NomadLog.Info("XML-RESULT: "+xmlPARAMS.FindElement("DATA").ToString());

                    //Actualizo las Variables
                    SetVariables(theINST, oi_thread, xmlVARIABLES);

                    //Creo la instancia del WF Nuevo
                    string newInstId;
                    if (defWFCall.oi_organigrama==defWF.oi_organigrama)
                    {
                      //Mismo organigrama
                      newInstId=CREAR(
                        RS.GetAttr("oi_wf"),
                        RS.GetAttr("oi_process"),
                        theINST.c_rol,
                        (theINST.l_debug?1:0),
                        theINST.d_owner,
                        xmlPARAMS.FindElement("DATA")
                      );
                    } else
                    {
                      throw new Exception("CALL-!WE, DISTINTO ORGANIGRAMA NO ANDA.");
                    }
                    NomadLog.Info("Nueva Instancia: "+newInstId);

                    //Activo la Espera!!!!
                    theTHREAD=(NucleusWF.Base.Ejecucion.Instancias.THREAD)theINST.THREADS.GetById(oi_thread.ToString());
                    theTHREAD.c_estado="WF";
                    theTHREAD.f_ejecucion=DateTime.Now;
                    theTHREAD.oi_inst_child=newInstId;

                    //Programo la Proxima Consulta
                    SetInfo(theINST, theTHREAD, xmlNODE.GetAttr("d_label")+" - Esperando el Resultado...");
                  }
                  break;

                ////////////////////////////////////////////////////////////////////////////////////////////////////
                // Ejecucion de METODOS
                case "SP":
                case "SOAP":
                case "JSON":
                case "JS":
                  //Obtengo la Definicion del WF
                  defWF=INSTANCE.GetWFDef(theINST.oi_wf);

                  //Inicializo las Variables
                  xmlVARIABLES=GetVariables(theINST, oi_thread);

                  //Ejecuto el METODO EXTERNO
                  NucleusWF.Base.Definicion.Integracion.INT_MAG.ExecuteWF(xmlNODE.GetAttr("d_param_1"), xmlNODE.FindElement2("PARAM", "c_type", "IN").GetAttr("o_compilada"), xmlNODE.FindElement2("PARAM", "c_type", "OUT").GetAttr("o_compilada"), xmlVARIABLES);

                  //Actualizo las Variables
                  SetVariables(theINST, oi_thread, xmlVARIABLES);

                  SetInfo(theINST, theTHREAD, xmlNODE.GetAttr("d_label")+" - Procesado OK.");
                  NextStep(theINST, theTHREAD, xmlINFO);
                  break;

                ////////////////////////////////////////////////////////////////////////////////////////////////////
                case "WAIT_TIME":
                case "LABEL":
                  //Proximo Paso
                  SetInfo(theINST, theTHREAD, xmlNODE.GetAttr("d_label")+" - Procesado OK.");
                  NextStep(theINST, theTHREAD, xmlINFO);
                  break;

                ////////////////////////////////////////////////////////////////////////////////////////////////////
                case "GOTO":
                  NomadXML lblFind=FindLabel(theINST, theTHREAD, xmlNODE.GetAttr("d_label"));
                  if (lblFind==null)
                    throw new Exception("No se encontro el LABEL '"+xmlNODE.GetAttr("d_label")+"' en la Definicion.");

                  //Reactivando THREADS
                  for (NomadXML cur=lblFind.FirstChild().Next(); cur!=null; cur=cur.Next())
                  {
                    NucleusWF.Base.Ejecucion.Instancias.THREAD tmpTHREAD=(NucleusWF.Base.Ejecucion.Instancias.THREAD)theINST.THREADS.GetById(cur.GetAttr("id"));

                    //Verifico si hay mas de uno corriendo
                    int cnt=0;
                    foreach (NucleusWF.Base.Ejecucion.Instancias.THREAD curTHREAD in theINST.THREADS)
                    {
                      if (curTHREAD.oi_thread_parent!=tmpTHREAD.oi_thread_parent) continue;
                      if (curTHREAD.Id==tmpTHREAD.Id) continue;
                      if (curTHREAD.c_estado=="I") continue;
                      cnt++;
                    }

                    if (cnt!=0)
                      throw new Exception("No se puede utilizar el Comando GOTO saliendo de un proceso PARALELO.");
                  }

                  //Cambios los ESTADOS
                  for (NomadXML cur=lblFind.FirstChild(); cur!=null; cur=cur.Next())
                  {
                    NucleusWF.Base.Ejecucion.Instancias.THREAD tmpTHREAD=(NucleusWF.Base.Ejecucion.Instancias.THREAD)theINST.THREADS.GetById(cur.GetAttr("id"));
                    EmptyThread(tmpTHREAD);
                    tmpTHREAD.c_estado="I";
                  }

                  //LOG
                  SetInfo(theINST, theTHREAD, xmlNODE.GetAttr("d_label")+" - Procesado OK.");

                  //Thread Principal
                  NucleusWF.Base.Ejecucion.Instancias.THREAD actTHREAD=(NucleusWF.Base.Ejecucion.Instancias.THREAD)theINST.THREADS.GetById(lblFind.FirstChild().GetAttr("id"));
                  EmptyThread(actTHREAD);
                  actTHREAD.c_estado="WE";
                  actTHREAD.oi_node=lblFind.GetAttr("oi_node");
                  actTHREAD.f_ejecucion=DateTime.Now;
                  break;

                ////////////////////////////////////////////////////////////////////////////////////////////////////
                case "PAR":
                  theTHREAD.c_estado="D";

                  //Recorro los PARAMETROS y analizo los THREADS a CREAR.
                  for (NomadXML curVAR=xmlNODE.FirstChild(); curVAR!=null; curVAR=curVAR.Next())
                    AddThread(theINST, oi_thread, xmlPROC.GetAttr("c_process"), curVAR.GetAttr("e_subproceso"));

                  SetInfo(theINST, theTHREAD, xmlNODE.GetAttr("d_label")+" - Procesado OK.");
                  break;

                ////////////////////////////////////////////////////////////////////////////////////////////////////
                case "MAIL":
                  SendMail(theINST, oi_thread, xmlNODE);
                  SetInfo(theINST, theTHREAD, xmlNODE.GetAttr("d_label") + " - Procesado OK.");
                  NextStep(theINST, theTHREAD, xmlINFO);
                  break;

                ////////////////////////////////////////////////////////////////////////////////////////////////////
                case "SUBPROCESS":
                  NucleusWF.Base.Definicion.Workflows.PROCESS childPRO=null;

                  //Obtengo la Definicion del WF
                  defWF=INSTANCE.GetWFDef(theINST.oi_wf);

                  //Buscar el Subproceso hijo
                  foreach (NucleusWF.Base.Definicion.Workflows.PROCESS cur in defWF.PROCESS)
                  {
                    if (cur.e_automatico==0 && cur.c_process==xmlNODE.GetAttr("d_param_1"))
                    {
                      childPRO=cur;
                      break;
                    }
                  }

                  //Validaciones
                  if (childPRO==null)
                    throw new Exception("No se encontro el Subproceso con CODIGO '"+xmlNODE.GetAttr("d_param_1")+"'.");

                  //Agrego el THREAD
                  theTHREAD.c_estado="D";
                  AddThread(theINST, oi_thread, childPRO.c_process, "0");
                  SetInfo(theINST, theTHREAD, xmlNODE.GetAttr("d_label")+" - Llamada OK.");
                  break;

                case "SWITCH":
                  //Obtengo la Definicion del WF
                  defWF=INSTANCE.GetWFDef(theINST.oi_wf);

                  //Inicializo las Variables
                  xmlVARIABLES=GetVariables(theINST, oi_thread);

                  //Inicializo las Variables
                  execute=xmlNODE.GetAttr("o_compformula");
                  NomadLog.Info("EXECUTE: "+execute);
                  result=Nomad.Base.Report.Formula.Resolve(execute, null, xmlVARIABLES, false);
                  NomadLog.Info("RESULT: "+result);

                  //Resultado
                  switch (result.Substring(0, 1))
                  {
                    case "B":
                    case "I":
                    case "N":
                    case "D":
                    case "S":
                      result=result.Substring(2);
                      break;
                    case "0":
                      result="";
                      break;
                    case "E": throw new Exception("Error en la Formula, "+result.Substring(2));
                    default: throw new Exception("Tipo de Resultado no VALIDO.");
                  }

                  string e_subproceso="";

                  //Recorro los PARAMETROS y analizo los THREADS a CREAR.
                  for (NomadXML curVAR=xmlNODE.FirstChild(); curVAR!=null; curVAR=curVAR.Next())
                    if (curVAR.GetAttr("c_type")=="CASE" && curVAR.GetAttr("c_param").ToUpper()==result.ToUpper())
                      e_subproceso=curVAR.GetAttr("e_subproceso");

                  //Busco el DEFAULT
                  if (e_subproceso=="")
                  {
                    NomadXML curVAR=xmlNODE.FindElement2("PARAM", "c_type", "DEFAULT");
                    if (curVAR!=null)
                      e_subproceso=curVAR.GetAttr("e_subproceso");
                  }

                  if (e_subproceso=="")
                    throw new Exception("Error en la Definicion del SWITCH.");

                  //Agrego el THREAD
                  theTHREAD.c_estado="D";
                  AddThread(theINST, oi_thread, xmlPROC.GetAttr("c_process"), e_subproceso);
                  SetInfo(theINST, theTHREAD, xmlNODE.GetAttr("d_label")+" - Procesado OK. (Resultado: "+result+")");
                  break;

                ////////////////////////////////////////////////////////////////////////////////////////////////////
                default:
                  throw new Exception("El tipo de NODO '"+xmlNODE.GetAttr("c_type")+"' no esta implementado.");
              }
            } catch (Exception Ex)
            {
              SetInternalError(theINST, theTHREAD, Ex);
            }
          }

          //GUARDO
          NomadEnvironment.GetCurrentTransaction().SaveRefresh(theINST);
          theTHREAD=(NucleusWF.Base.Ejecucion.Instancias.THREAD)theINST.THREADS.GetById(oi_thread.ToString());
        } while ((theTHREAD!=null) && ((theTHREAD.c_estado=="WE") || (theTHREAD.c_estado=="WI")) && (theTHREAD.f_ejecucion<=DateTime.Now));

        //Busco otros Threads
        theTHREAD=null;
        foreach (THREAD curTHREAD in theINST.THREADS)
        {
          if (((curTHREAD.c_estado=="WE") || (curTHREAD.c_estado=="WI")) && (curTHREAD.f_ejecucion<=DateTime.Now))
          {
            theTHREAD=curTHREAD;
            oi_thread=int.Parse(theTHREAD.Id);
            break;
          }
        }
      } while (theTHREAD!=null);

      if (extINST!=null) Notify(extINST);
      Notify(theINST);
      NomadLog.Info("El proceso a Finalizado.");
    }

    public static void Notify(NucleusWF.Base.Ejecucion.Instancias.INSTANCE theINST)
    {
      if (theINST.c_estado_ej!="R")
        return;

      //ENVIO LA NOTIFICACION DE EJECUCION
      foreach (NucleusWF.Base.Ejecucion.Instancias.THREAD curTHREAD in theINST.THREADS)
        if ((curTHREAD.c_estado=="WE") || (curTHREAD.c_estado=="WI") || (curTHREAD.c_estado=="WX"))
        {
#if NOMAD_LOCAL
          NucleusWF.Base.Ejecucion.Instancias.INSTANCE.Procesar(int.Parse(curTHREAD.Id));
          return;
#else
      RPCService MyRPC = NomadProxy.GetProxy().RPCService();
      MyRPC.AddParam(new RPCParam("oi_thread", "IN", int.Parse(curTHREAD.Id)));
      MyRPC.Execute(NomadProxy.GetProxy().AppName + "-WF-" + curTHREAD.Id, 5, curTHREAD.f_ejecucion, "WORKFLOW", "NucleusWF.Base.Ejecucion.Instancias.INSTANCE", "Procesar");
#endif
        }

      //ENVIO LA NOTIFICACION DE ENVIO DE MAIL
      foreach (NucleusWF.Base.Ejecucion.Instancias.THREAD curTHREAD in theINST.THREADS)
        if (curTHREAD.c_estado=="WP")
        {
          //Obtengo el NODO
          NomadXML xmlNODE=NomadProxy.GetProxy().SQLService().GetXML("CLASS.NucleusWF.Base.Ejecucion.Instancias.INSTANCE.qry_mail", "<DATA oi_node=\""+curTHREAD.oi_node+"\" />");
          if (xmlNODE.GetAttr("d_param_2")=="") continue;
          if (xmlNODE.GetAttr("d_param_3")=="") continue;

          ///////////////////////////////////////////////////////////////////
          //Mail de NOTIFICACION
          INSTANCE.SendMail(theINST, curTHREAD, int.Parse(curTHREAD.oi_node), "N");

#if !NOMAD_LOCAL
        RPCService MyRPC;

        NomadLog.Debug("Fecha Advertencia: "+curTHREAD.f_advertencia.ToString("dd/MM/yyyy HH:mm"));
        if (!curTHREAD.f_advertenciaNull && curTHREAD.f_advertencia>curTHREAD.f_ejecucion)
        {
          ///////////////////////////////////////////////////////////////////
          //Mail de ADVERTENCIA
          MyRPC=NomadProxy.GetProxy().RPCService();
          MyRPC.AddParam(new RPCParam("oi_thread","IN",int.Parse(curTHREAD.Id)));
          MyRPC.AddParam(new RPCParam("oi_node"  ,"IN",int.Parse(curTHREAD.oi_node)));
          MyRPC.AddParam(new RPCParam("caso"     ,"IN","A"));
          MyRPC.Execute(NomadProxy.GetProxy().AppName+"-WF-MAIL-A-"+curTHREAD.Id,1,curTHREAD.f_advertencia,"WORKFLOW","NucleusWF.Base.Ejecucion.Instancias.INSTANCE","SendMail");
        }

        NomadLog.Debug("Fecha Advertencia: "+curTHREAD.f_vencimiento.ToString("dd/MM/yyyy HH:mm"));
        if (!curTHREAD.f_vencimientoNull && curTHREAD.f_vencimiento>curTHREAD.f_ejecucion)
        {
          ///////////////////////////////////////////////////////////////////
          //Mail de VENCIMIENTO
          MyRPC=NomadProxy.GetProxy().RPCService();
          MyRPC.AddParam(new RPCParam("oi_thread","IN",int.Parse(curTHREAD.Id)));
          MyRPC.AddParam(new RPCParam("oi_node"  ,"IN",int.Parse(curTHREAD.oi_node)));
          MyRPC.AddParam(new RPCParam("caso"     ,"IN","V"));
          MyRPC.Execute(NomadProxy.GetProxy().AppName+"-WF-MAIL-V-"+curTHREAD.Id,1,curTHREAD.f_vencimiento,"WORKFLOW","NucleusWF.Base.Ejecucion.Instancias.INSTANCE","SendMail");
        }
#endif
        }
    }

    public static void AppendHistVariables(NucleusWF.Base.Ejecucion.Instancias.INSTANCE theINST)
    {
      //Obtengo la Definicion del WF si esta no fue ENVIADA
      NucleusWF.Base.Definicion.Workflows.WF defWF=INSTANCE.GetWFDef(theINST.oi_wf);

      //Busco los Grupos con historia
      foreach (NucleusWF.Base.Definicion.Workflows.VAR_GROUP grp in defWF.GROUPS)
      {
        if (grp.c_mode == "HIST")
        {
          int lastPaso = -1;
          VALUE_HIST lastVAL = null;

          //Busco el Ultimo PASO
          foreach (VALUE_HIST curVAL in theINST.VALS_HIST)
            if (curVAL.c_var_group == grp.c_var_group)
              if (curVAL.e_paso > lastPaso)
              {
                lastVAL = curVAL;
                lastPaso = curVAL.e_paso;
              }

          //Creo el Paso Nuevo
          VALUE_HIST theVAL = new VALUE_HIST();
          theVAL.e_paso = theINST.e_pasos;
          theVAL.c_var_group = grp.c_var_group;

          //Busco la VARIABLE
          foreach (NucleusWF.Base.Definicion.Workflows.VAR svar in grp.VARS)
            if (svar.l_hist_copy && lastVAL != null)
            {
              //Pego el VALOR
              if (!lastVAL.GetAttributeNull(svar.c_column)) theVAL.SetAttribute(svar.c_column, lastVAL.GetAttribute(svar.c_column));
              else theVAL.SetAttributeNull(svar.c_column);
            }

          theINST.VALS_HIST.Add(theVAL);
        }
      }
    }

    public static NomadXML GetVariables(int oi_thread) { return GetVariables(null, oi_thread, null); }
    public static NomadXML GetVariables(NucleusWF.Base.Ejecucion.Instancias.INSTANCE theINST, int oi_thread) { return GetVariables(theINST, oi_thread, null); }
    public static NomadXML GetVariables(NucleusWF.Base.Ejecucion.Instancias.INSTANCE theINST, int oi_thread, string EttyID)
    {
      NucleusWF.Base.Ejecucion.Instancias.THREAD theTHREAD;

      //Obtengo la Instancia si esta no fue ENVIADA
      if (theINST==null)
      {
        theTHREAD=NucleusWF.Base.Ejecucion.Instancias.THREAD.Get(oi_thread.ToString(), false);
        theINST=NucleusWF.Base.Ejecucion.Instancias.INSTANCE.Get(theTHREAD.oi_instance, false);
      }

      //Obtengo la Definicion del WF si esta no fue ENVIADA
      NucleusWF.Base.Definicion.Workflows.WF defWF=INSTANCE.GetWFDef(theINST.oi_wf);

      //XML de Resultado
      NomadXML DATA_WF=NucleusWF.Base.Definicion.Workflows.WF.VariablesWF(defWF);
      DATA_WF.SetAttr("paso", theINST.e_pasos);
      DATA_WF.SetAttr("debug", theINST.l_debug);

      //Valores de la INSTANCIA
      Nomad.Base.Report.Variables.SetValue(DATA_WF, "Instancia.Numero", int.Parse(theINST.Id));
      Nomad.Base.Report.Variables.SetValue(DATA_WF, "Instancia.Descripcion", theINST.d_instance);
      Nomad.Base.Report.Variables.SetValue(DATA_WF, "Instancia.Estado", theINST.c_estado_wf);
      if (!theINST.f_inicioNull) Nomad.Base.Report.Variables.SetValue(DATA_WF, "Instancia.Fecha_Inicio", theINST.f_inicio);
      if (!theINST.f_advertenciaNull)
      {
        Nomad.Base.Report.Variables.SetValue(DATA_WF, "Instancia.Fecha_Advertencia", theINST.f_advertencia);
        Nomad.Base.Report.Variables.SetValue(DATA_WF, "Instancia.Proxima_a_Vencer", (theINST.f_advertencia>DateTime.Now?false:true));
      } else
      {
        Nomad.Base.Report.Variables.SetValue(DATA_WF, "Instancia.Proxima_a_Vencer", false);
      }
      if (!theINST.f_vencimientoNull)
      {
        Nomad.Base.Report.Variables.SetValue(DATA_WF, "Instancia.Fecha_Vencimiento", theINST.f_vencimiento);
        Nomad.Base.Report.Variables.SetValue(DATA_WF, "Instancia.Vencida", (theINST.f_vencimiento>DateTime.Now?false:true));
      } else
      {
        Nomad.Base.Report.Variables.SetValue(DATA_WF, "Instancia.Vencida", false);
      }

      //Valores del SOLICITANTE
      Nomad.Base.Report.Variables.SetValue(DATA_WF, "Solicitante.ID", theINST.d_owner);
      Nomad.Base.Report.Variables.SetValue(DATA_WF, "Solicitante.Nombre", theINST.d_owner_ayn);
      Nomad.Base.Report.Variables.SetValue(DATA_WF, "Solicitante.Mail", theINST.d_owner_mail);
      Nomad.Base.Report.Variables.SetValue(DATA_WF, "Solicitante.Telefono", theINST.d_owner_tel);
      Nomad.Base.Report.Variables.SetValue(DATA_WF, "Solicitante.Rol", theINST.c_rol, theINST.d_owner_rol);

      //Obtengo el Thread
      if (oi_thread>0)
      {
        theTHREAD=(NucleusWF.Base.Ejecucion.Instancias.THREAD)theINST.THREADS.GetById(oi_thread.ToString());

        //Valores del PROCESO
        if (!theTHREAD.f_inicioNull) Nomad.Base.Report.Variables.SetValue(DATA_WF, "Proceso.Fecha_Inicio", theTHREAD.f_inicio);
        else Nomad.Base.Report.Variables.SetValue(DATA_WF, "Proceso.Fecha_Inicio", theTHREAD.f_ejecucion);
        if (!theTHREAD.f_advertenciaNull) Nomad.Base.Report.Variables.SetValue(DATA_WF, "Proceso.Fecha_Advertencia", theTHREAD.f_advertencia);
        if (!theTHREAD.f_vencimientoNull) Nomad.Base.Report.Variables.SetValue(DATA_WF, "Proceso.Fecha_Vencimiento", theTHREAD.f_vencimiento);

        //Valores del ASIGNACION
        if (theTHREAD.c_asign_rol!="")
        {
          Nomad.Base.Report.Variables.SetValue(DATA_WF, "Asignado.Fecha_Asignacion", theTHREAD.f_ejecucion);
          Nomad.Base.Report.Variables.SetValue(DATA_WF, "Asignado.ID", theTHREAD.d_asign_etty);
          Nomad.Base.Report.Variables.SetValue(DATA_WF, "Asignado.Nombre", theTHREAD.d_asign_ayn);
          Nomad.Base.Report.Variables.SetValue(DATA_WF, "Asignado.Mail", theTHREAD.d_asign_mail);
          Nomad.Base.Report.Variables.SetValue(DATA_WF, "Asignado.Telefono", theTHREAD.d_asign_tel);
          Nomad.Base.Report.Variables.SetValue(DATA_WF, "Asignado.Rol", theTHREAD.c_asign_rol, theTHREAD.d_asign_titulo);
        }

        //Valores del DELEGACION
        if (theTHREAD.d_deleg_etty!="")
        {
          Nomad.Base.Report.Variables.SetValue(DATA_WF, "Delegador.Fecha_Delegacion", theTHREAD.f_ejecucion);
          Nomad.Base.Report.Variables.SetValue(DATA_WF, "Delegador.Comentario", theTHREAD.o_deleg_coment);
          Nomad.Base.Report.Variables.SetValue(DATA_WF, "Delegador.ID", theTHREAD.d_deleg_etty);
          Nomad.Base.Report.Variables.SetValue(DATA_WF, "Delegador.Nombre", theTHREAD.d_deleg_ayn);
          Nomad.Base.Report.Variables.SetValue(DATA_WF, "Delegador.Mail", theTHREAD.d_deleg_mail);
          Nomad.Base.Report.Variables.SetValue(DATA_WF, "Delegador.Telefono", theTHREAD.d_deleg_tel);
          Nomad.Base.Report.Variables.SetValue(DATA_WF, "Delegador.Rol", theTHREAD.c_deleg_rol, theTHREAD.d_deleg_titulo);
        }
      }

      //Obtengo la ENTIDAD, caso DEBUG!
      if (EttyID!=null && EttyID!="")
      {
        NomadXML AUXxml;
        NomadXML objETTY=NomadProxy.GetProxy().DDOService().GetXML("Nomad.Base.Login.Entidades.ENTIDAD", EttyID);
        if (objETTY.isDocument) objETTY=objETTY.FirstChild();
        if (objETTY.Name.ToUpper()=="OBJECT") objETTY=objETTY.FirstChild();

        Nomad.Base.Report.Variables.SetValue(DATA_WF, "Resolucion.Fecha", DateTime.Now);
        Nomad.Base.Report.Variables.SetValue(DATA_WF, "Resolucion.ID", objETTY.GetAttr("id"));
        Nomad.Base.Report.Variables.SetValue(DATA_WF, "Resolucion.Codigo", objETTY.GetAttr("COD"));
        Nomad.Base.Report.Variables.SetValue(DATA_WF, "Resolucion.Nombre", objETTY.GetAttr("DES"));

        //MAIL
        AUXxml=objETTY.FindElement("MAILS");
        if (AUXxml!=null)
          AUXxml=AUXxml.FindElement2("MAIL", "PRINCIPAL", "1");
        if (AUXxml!=null)
          Nomad.Base.Report.Variables.SetValue(DATA_WF, "Resolucion.Mail", AUXxml.GetAttr("EMAIL"));

        //TELEFONO
        AUXxml=objETTY.FindElement("TELEFONOS");
        if (AUXxml!=null)
          AUXxml=AUXxml.FindElement2("TELEFONO", "PRINCIPAL", "1");
        if (AUXxml!=null)
          Nomad.Base.Report.Variables.SetValue(DATA_WF, "Resolucion.Telefono", AUXxml.GetAttr("TELEF"));
      }

      //Recorro los Indices y RELLENO el XML
      int idx;
      string Value, ValueTXT;
      SortedList MyLIST=new SortedList();

      foreach (NucleusWF.Base.Definicion.Workflows.VAR_GROUP grp in defWF.GROUPS)
      {
        switch (grp.c_mode)
        {
          case "MAIN":
            foreach (VALUE theVAL in theINST.VALS)
            {
              //Busco el Grupo
              if (theVAL.c_var_group!=grp.c_var_group)
                continue;

              //Busco la VARIABLE
              foreach (NucleusWF.Base.Definicion.Workflows.VAR svar in grp.VARS)
              {
                //Pego el VALOR
                Value="";
                if (!theVAL.GetAttributeNull(svar.c_column))
                {
                  switch (svar.c_column.Substring(0, 2))
                  {
                    case "e_": Value=((int)theVAL.GetAttribute(svar.c_column)).ToString(); break;
                    case "c_": Value=((string)theVAL.GetAttribute(svar.c_column)); break;
                    case "o_": Value=((string)theVAL.GetAttribute(svar.c_column)); break;
                    case "n_": Value=Nomad.NSystem.Functions.StringUtil.dbl2str(((double)theVAL.GetAttribute(svar.c_column))); break;
                    case "d_": Value=Nomad.NSystem.Functions.StringUtil.date2str(((System.DateTime)theVAL.GetAttribute(svar.c_column))); break;
                  }
                }

                //Pego el VALOR
                ValueTXT="";
                if (svar.c_column_desc!="")
                  ValueTXT=((string)theVAL.GetAttribute(svar.c_column_desc));

                //Seteo la Variable
                Nomad.Base.Report.Variables.SetValue(DATA_WF, -1, grp.c_var_group+"."+svar.c_var, svar.c_type, Value, ValueTXT);
              }
            }
            break;

          case "HIST":
            //Filtro la Lista y la Ordeno por el Indice
            MyLIST.Clear();
            foreach (VALUE_HIST theVAL in theINST.VALS_HIST)
              if (theVAL.c_var_group==grp.c_var_group)
                MyLIST.Add(theVAL.e_paso, theVAL);

            //Elementos Agregados
            idx=0;
            foreach (VALUE_HIST theVAL in MyLIST.GetValueList())
            {
              //Agrego el ITEM
              Nomad.Base.Report.Variables.AddItem(DATA_WF, grp.c_var_group);

              //Busco la VARIABLE
              foreach (NucleusWF.Base.Definicion.Workflows.VAR svar in grp.VARS)
              {
                //Pego el VALOR
                Value="";
                if (!theVAL.GetAttributeNull(svar.c_column))
                {
                  switch (svar.c_column.Substring(0, 2))
                  {
                    case "e_": Value=((int)theVAL.GetAttribute(svar.c_column)).ToString(); break;
                    case "c_": Value=((string)theVAL.GetAttribute(svar.c_column)); break;
                    case "o_": Value=((string)theVAL.GetAttribute(svar.c_column)); break;
                    case "n_": Value=Nomad.NSystem.Functions.StringUtil.dbl2str(((double)theVAL.GetAttribute(svar.c_column))); break;
                    case "d_": Value=Nomad.NSystem.Functions.StringUtil.date2str(((System.DateTime)theVAL.GetAttribute(svar.c_column))); break;
                  }
                }

                //Pego el VALOR
                ValueTXT="";
                if (svar.c_column_desc!="")
                  ValueTXT=((string)theVAL.GetAttribute(svar.c_column_desc));

                //Seteo la Variable
                Nomad.Base.Report.Variables.SetValue(DATA_WF, idx, grp.c_var_group+"."+svar.c_var, svar.c_type, Value, ValueTXT);
              }
              idx++;
            }

            Nomad.Base.Report.Variables.SetItemCursor(DATA_WF, grp.c_var_group, idx-1);
            break;

          case "CHILD":
            //Filtro la Lista y la Ordeno por el Indice
            MyLIST.Clear();
            foreach (VALUE_CHILD theVAL in theINST.VALS_CHILD)
              if (theVAL.c_var_group==grp.c_var_group)
                MyLIST.Add(theVAL.e_indice, theVAL);

            idx=0;
            foreach (VALUE_CHILD theVAL in MyLIST.GetValueList())
            {
              //Agrego el ITEM
              Nomad.Base.Report.Variables.AddItem(DATA_WF, grp.c_var_group);

              //Busco la VARIABLE
              foreach (NucleusWF.Base.Definicion.Workflows.VAR svar in grp.VARS)
              {
                //Pego el VALOR
                Value="";
                if (!theVAL.GetAttributeNull(svar.c_column))
                {
                  switch (svar.c_column.Substring(0, 2))
                  {
                    case "e_": Value=((int)theVAL.GetAttribute(svar.c_column)).ToString(); break;
                    case "c_": Value=((string)theVAL.GetAttribute(svar.c_column)); break;
                    case "o_": Value=((string)theVAL.GetAttribute(svar.c_column)); break;
                    case "n_": Value=Nomad.NSystem.Functions.StringUtil.dbl2str(((double)theVAL.GetAttribute(svar.c_column))); break;
                    case "d_": Value=Nomad.NSystem.Functions.StringUtil.date2str(((System.DateTime)theVAL.GetAttribute(svar.c_column))); break;
                  }
                }

                //Pego el VALOR
                ValueTXT="";
                if (svar.c_column_desc!="")
                  ValueTXT=((string)theVAL.GetAttribute(svar.c_column_desc));

                //Seteo la Variable
                Nomad.Base.Report.Variables.SetValue(DATA_WF, idx, grp.c_var_group+"."+svar.c_var, svar.c_type, Value, ValueTXT);
              }
              idx++;
            }
            break;
        }
      }

      NomadXML FILES=NomadProxy.GetProxy().SQLService().GetXML("CLASS.NucleusWF.Base.Ejecucion.Instancias.INSTANCE.FILESATTACH", "<PARAM id=\""+theINST.id+"\" />");
      DATA_WF.AddXML(FILES);

      return DATA_WF;
    }

    public static void Pausar(int oi_instance, string Etty)
    {
      NucleusWF.Base.Ejecucion.Instancias.INSTANCE theINST=NucleusWF.Base.Ejecucion.Instancias.INSTANCE.Get(oi_instance.ToString(), false);

      //Validacion
      Validar(theINST, "R", null, null, -1, true);

      //Agrego el LOG
      AddEtapaLog(theINST, null, "WRN", "El workflow a sido pausado.", Etty);

      //Cambio el Estado
      theINST.c_estado_ej="P";

      //Analizo el estado de los THREADS
      foreach (NucleusWF.Base.Ejecucion.Instancias.THREAD oTH in theINST.THREADS)
      {
        if (oTH.c_estado == "WP")
          AddEtapaLog(theINST, oTH, "WRN", "El workflow a sido pausado.", Etty);

        RefreshUSERS(theINST, oTH);
      }

      //GUARDO
      theINST.d_etty_creator=NomadProxy.GetProxy().UserEtty;
      NomadEnvironment.GetCurrentTransaction().Save(theINST);
    }
    public static void Continuar(int oi_instance, string Etty)
    {
      NucleusWF.Base.Ejecucion.Instancias.INSTANCE theINST=NucleusWF.Base.Ejecucion.Instancias.INSTANCE.Get(oi_instance.ToString(), false);

      //Validacion
      Validar(theINST, "P", null, null, -1, true);

      //Agrego el LOG
      AddEtapaLog(theINST, null, "IFO", "El workflow se a reanudado.", Etty);

      //Cambio el Estado
      theINST.c_estado_ej="R";

      //Analizo el estado de los THREADS
      foreach (NucleusWF.Base.Ejecucion.Instancias.THREAD oTH in theINST.THREADS)
      {
        if (oTH.c_estado == "WP")
          oTH.f_ejecucion = DateTime.Now;

        RefreshUSERS(theINST, oTH);
      }

      //GUARDO
      theINST.d_etty_creator = NomadProxy.GetProxy().UserEtty;
      NomadEnvironment.GetCurrentTransaction().SaveRefresh(theINST);
      Notify(theINST);
    }
    public static void Detener(int oi_instance, string Etty)
    {
      NucleusWF.Base.Ejecucion.Instancias.INSTANCE theINST=NucleusWF.Base.Ejecucion.Instancias.INSTANCE.Get(oi_instance.ToString(), false);

      //Validacion
      Validar(theINST, "P,R,E", null, null, -1, true);

      //Cambio el Estado
      theINST.c_estado_ej = "S";

      //Elimino Referencia entre los THREADS
      foreach (NucleusWF.Base.Ejecucion.Instancias.THREAD oTH in theINST.THREADS)
      {
        oTH.oi_thread_parentNull = true;
        RefreshUSERS(theINST, oTH);
      }
      NomadEnvironment.GetCurrentTransaction().SaveRefresh(theINST);

      //Elimino los THREADS
      theINST.THREADS.Clear();

      //Agrego el LOG
      AddEtapaLog(theINST, null, "WRN", "El workflow se a Detenido.", Etty);

      //GUARDO
      theINST.d_etty_creator = NomadProxy.GetProxy().UserEtty;
      NomadEnvironment.GetCurrentTransaction().Save(theINST);
    }
    public static void Eliminar(int oi_instance, string Etty)
    {
      NucleusWF.Base.Ejecucion.Instancias.INSTANCE theINST=NucleusWF.Base.Ejecucion.Instancias.INSTANCE.Get(oi_instance.ToString(), true);

      //Validacion
      Validar(theINST, null, null, null, -1, true);

      //Elimino Referencia entre los THREADS
      foreach (NucleusWF.Base.Ejecucion.Instancias.THREAD oTH in theINST.THREADS)
        oTH.oi_thread_parentNull = true;
      NomadEnvironment.GetCurrentTransaction().SaveRefresh(theINST);

      //Elimino los THREADS
      theINST.THREADS.Clear();
      NomadEnvironment.GetCurrentTransaction().Delete(theINST);
    }

    public static void AutoDelete(int oi_instance, string estado)
    {
      NucleusWF.Base.Ejecucion.Instancias.INSTANCE theINST=NucleusWF.Base.Ejecucion.Instancias.INSTANCE.Get(oi_instance.ToString(), true);

      //Validacion
      Validar(theINST, null, null, null, -1, true);

      //Verifico el Estado
      if (theINST.c_estado_wf!=estado)
        return;

      //Elimino Referencia entre los THREADS
      foreach (NucleusWF.Base.Ejecucion.Instancias.THREAD oTH in theINST.THREADS)
        oTH.oi_thread_parentNull = true;
      NomadEnvironment.GetCurrentTransaction().SaveRefresh(theINST);

      //Elimino los THREADS
      theINST.THREADS.Clear();
      NomadEnvironment.GetCurrentTransaction().Delete(theINST);
    }
    public static void Reintentar(int oi_instance, string Etty)
    {
      NucleusWF.Base.Ejecucion.Instancias.INSTANCE theINST=NucleusWF.Base.Ejecucion.Instancias.INSTANCE.Get(oi_instance.ToString(), false);

      //Validacion
      Validar(theINST, "E", null, null, -1, true);

      //Analizo el estado de los THREADS
      foreach (NucleusWF.Base.Ejecucion.Instancias.THREAD oTH in theINST.THREADS)
        if (oTH.c_estado=="E")
        {
          oTH.c_estado="WE";
          oTH.f_ejecucion=DateTime.Now;
          AddEtapaLog(theINST, oTH, "WRN", "Reintentar.", Etty);
        }

      //Cambio el Estado
      theINST.c_estado_ej="R";

      //GUARDO
      theINST.d_etty_creator = NomadProxy.GetProxy().UserEtty;
      NomadEnvironment.GetCurrentTransaction().SaveRefresh(theINST);
      Notify(theINST);
    }
    public static void Ignorar(int oi_instance, string Etty)
    {
      NucleusWF.Base.Ejecucion.Instancias.INSTANCE theINST=NucleusWF.Base.Ejecucion.Instancias.INSTANCE.Get(oi_instance.ToString(), false);

      //Validacion
      Validar(theINST, "E", null, null, -1, true);

      //Analizo el estado de los THREADS
      foreach (NucleusWF.Base.Ejecucion.Instancias.THREAD oTH in theINST.THREADS)
        if (oTH.c_estado=="E")
        {
          oTH.c_estado="WX";
          oTH.f_ejecucion=DateTime.Now;
          AddEtapaLog(theINST, oTH, "WRN", "Ignorar.", Etty);
        }

      //Cambio el Estado
      theINST.c_estado_ej="R";

      //GUARDO
      theINST.d_etty_creator = NomadProxy.GetProxy().UserEtty;
      NomadEnvironment.GetCurrentTransaction().Save(theINST);

      //ENVIO LA NOTIFICACION DE EJECUCION
      foreach (NucleusWF.Base.Ejecucion.Instancias.THREAD curTHREAD in theINST.THREADS)
        if ((curTHREAD.c_estado=="WE") || (curTHREAD.c_estado=="WI") || (curTHREAD.c_estado=="WX"))
        {
          RPCService MyRPC=NomadProxy.GetProxy().RPCService();
          MyRPC.AddParam(new RPCParam("oi_thread", "IN", int.Parse(curTHREAD.Id)));
          MyRPC.Execute(NomadProxy.GetProxy().AppName+"-WF-"+curTHREAD.Id, 5, curTHREAD.f_ejecucion, "WORKFLOW", "NucleusWF.Base.Ejecucion.Instancias.INSTANCE", "Procesar");
        }
    }
    public static void SetError(int oi_instance, int oi_thread, string Etty)
    {
      NucleusWF.Base.Ejecucion.Instancias.INSTANCE theINST=NucleusWF.Base.Ejecucion.Instancias.INSTANCE.Get(oi_instance.ToString(), false);
      NucleusWF.Base.Ejecucion.Instancias.THREAD theTHREAD=(NucleusWF.Base.Ejecucion.Instancias.THREAD)theINST.THREADS.GetById(oi_thread.ToString());

      //Validacion
      Validar(theINST, "R", theTHREAD, null, -1, false);

      //Cambio el Estado
      theINST.c_estado_ej="E";
      theTHREAD.c_estado="E";
      AddEtapaLog(theINST, theTHREAD, "ERR", "Marcando con ERROR.", Etty);
      RefreshUSERS(theINST, theTHREAD);

      //GUARDO
      theINST.d_etty_creator=NomadProxy.GetProxy().UserEtty;
      NomadEnvironment.GetCurrentTransaction().Save(theINST);
      Notify(theINST);
    }

    public static void RefreshThreadVersion(int oi_instance, int oi_thread)
    {
      bool change;
      NucleusWF.Base.Ejecucion.Instancias.INSTANCE theINST = NucleusWF.Base.Ejecucion.Instancias.INSTANCE.Get(oi_instance.ToString(), false);
      NucleusWF.Base.Ejecucion.Instancias.THREAD theTHREAD = (NucleusWF.Base.Ejecucion.Instancias.THREAD)theINST.THREADS.GetById(oi_thread.ToString());

      //Cambio es estado para ver si ahoro si se puede generar
      if (theINST.c_estado_ej=="E") theINST.c_estado_ej="R";

      //Validacion
      Validar(theINST, "R", theTHREAD, "WP", -1, true);
      theINST.e_pasos++;

      //Cargo la Definicion del ORG
      NucleusWF.Base.Definicion.Workflows.WF pWF = INSTANCE.GetWFDef(theINST.oi_wf);
      NucleusWF.Base.Definicion.Organigramas.ORGANIGRAMA pORG = INSTANCE.GetORGDef(pWF.oi_organigrama);

      //Cargo el LOG
      NucleusWF.Base.Ejecucion.Instancias.LOG theLOG=AddLog(theINST, theTHREAD, "IFO", "Se publico el organigrama '"+pORG.c_organigrama+"-"+pORG.e_version_pub.ToString()+"', no hay cambios detectados.");

      //Actualiza la lista de usuarios
      change=RefreshUSERS(theINST, theTHREAD);

      //Actualizo el LOG
      if (change)
      {
        theLOG.c_tipo="WRN";
        theLOG.o_comentario="Se publico el organigrama '"+pORG.c_organigrama+"-"+pORG.e_version_pub.ToString()+"', se detectaron cambios de usuarios.";
      }

      //GUARDO
      NomadEnvironment.GetCurrentTransaction().Save(theINST);
      if (change) Notify(theINST);
    }

    /// <summary>
    /// Valida si el usuario logeado puede resolver el formulario solicitado.
    /// En caso de poder resolver retorna los datos del formulario.
    /// </summary>
    /// <param name="pOIThread">OI del subproceso.</param>
    /// <param name="pOINode">OI del nodo.</param>
    /// <returns></returns>
    public static NomadXML ResolveForm(int pOIThread, int pOINode, string EttyID)
    {
      if (EttyID==null || EttyID=="")
        EttyID=NomadProxy.GetProxy().UserEtty;

      NomadXML xmlNodeData;
      NomadXML xmlMainThread;
      NomadXML xmlMainInstance;
      NomadXML xmlMainUser;
      NomadXML xmlParams;
      NomadXML xmlResult = new NomadXML("RESULT");

      string strHTMLFile;

      NucleusWF.Base.Ejecucion.Instancias.INSTANCE objInst;

      NomadLog.Info("------------------------------------------------------------------------------------------");
      NomadLog.Info(" Comienza el metodo NucleusWF.Base.Ejecucion.Instancias.INSTANCE.ResolveForm");
      NomadLog.Info("------------------------------------------------------------------------------------------");

      do
      {
        //Ejecuta el query para obtener los valores y el estado del thread
        NomadLog.Info("Ejecuta el query QRY_ThreadData.");
        xmlParams = new NomadXML("DATA");
        xmlParams.SetAttr("oi_node", pOINode.ToString());
        xmlParams.SetAttr("oi_thread", pOIThread.ToString());
        xmlParams.SetAttr("oi_etty", EttyID);
        xmlNodeData = NomadEnvironment.QueryNomadXML("CLASS.NucleusWF.Base.Ejecucion.Instancias.INSTANCE.QRY_FormThreadData", xmlParams.ToString(), false);
        if (xmlNodeData.isDocument) xmlNodeData = xmlNodeData.FirstChild();

        xmlMainThread = xmlNodeData.FindElement("MAINTRHEAD");
        xmlMainInstance = xmlNodeData.FindElement("INSTANCE");
        xmlMainUser = xmlNodeData.FindElement("USER");

        //Valida que haya retornado un resultado
        NomadLog.Info("Valida que haya retornado un resultado.");
        if (xmlMainThread==null || xmlMainInstance==null)
          throw new NomadMessage("NucleusWF.Ejecucion.NO-DISP", "PASO-1");

        //Valida que el thread esté en el estado correspondiente
        //WP = Esperando resolucion del usuario (formulario)
        NomadLog.Info("Valida que el thread esté en el estado correspondiente.");
        if (xmlMainThread.GetAttr("c_estado") != "WP" || xmlMainThread.GetAttr("oi_node") != pOINode.ToString())
          throw new NomadMessage("NucleusWF.Ejecucion.NO-DISP", "PASO-2");

        //Verifico la publicacion del Organigrama
        if (xmlMainThread.GetAttr("c_organigrama")!=xmlMainInstance.GetAttr("c_organigrama"))
        {
          NomadException MyEXEC=Nomad.NSystem.Base.NomadException.NewInternalException("NucleusWF.ResolveForm.DIF_ORG");
          MyEXEC.SetValue("MSG", "Hay diferencias entre el ORGANIGRAMA DEL WF y delegate THREAD");
          MyEXEC.SetValue("pOIThread", pOIThread.ToString());
          MyEXEC.SetValue("pOINode", pOINode.ToString());
          MyEXEC.SetValue("EttyID", EttyID);

          MyEXEC.SetValue("WF_ORG", xmlMainInstance.GetAttr("c_organigrama"));
          MyEXEC.SetValue("TH_ORG", xmlMainThread.GetAttr("c_organigrama"));
          MyEXEC.SetValue("WF_VER", xmlMainInstance.GetAttr("e_org_version"));
          MyEXEC.SetValue("TH_VER", xmlMainThread.GetAttr("e_org_version"));
          throw MyEXEC;
        }

        if (xmlMainThread.GetAttr("e_org_version")!=xmlMainInstance.GetAttr("e_org_version"))
          RefreshThreadVersion(xmlMainThread.GetAttrInt("oi_instance"), xmlMainThread.GetAttrInt("oi_thread"));

      } while (xmlMainThread.GetAttr("e_org_version")!=xmlMainInstance.GetAttr("e_org_version"));

      //Valida el usuario
      if (xmlMainUser==null)
        throw new NomadMessage("NucleusWF.Ejecucion.NO-SOLVE");

      //Obtiene la instancia
      NomadLog.Info("Obtiene la instancia.");
      objInst = NucleusWF.Base.Ejecucion.Instancias.INSTANCE.Get(xmlMainThread.GetAttr("oi_instance"), false);

      //Obtiene valores
      NomadLog.Info("Obtiene valores.");
      strHTMLFile = xmlMainThread.GetAttr("d_file_html");

      //Recorre los threads para validar si existe alguno en estado "R"
      NomadLog.Info("Recorre los threads para validar si existe alguno en estado 'R'.");
      foreach (NucleusWF.Base.Ejecucion.Instancias.THREAD objThread in objInst.THREADS)
        if (objThread.c_estado == "R")
          throw new NomadMessage("NucleusWF.Ejecucion.NO-DISP", "PASO-3");

      //Obtiene las variables y el URL del formulario. Conforma un XML con el resultado
      xmlResult = GetVariables(objInst, pOIThread, EttyID);

      xmlResult.SetAttr("result", "OK");
      xmlResult.SetAttr("d_file_html", strHTMLFile);

      //Solo modo DEBUG
      if (objInst.l_debug && !xmlMainThread.GetAttrBool("l_automatica"))
      {
        NomadLog.Info("Genera el archivo porque esta en modo DEBUG");
        string strFileName = NucleusWF.Base.Ejecucion.Instancias.INSTANCE.GenerateHTMLForm(xmlMainThread.GetAttr("oi_form"));
        xmlResult.SetAttr("d_file_html", strFileName);
        NomadLog.Info("Genera el archivo porque esta en modo DEBUG - FIN");
      }

      return xmlResult;
    }

    /// <summary>
    /// Genera el reporte que representa el formulario de carga para los Workflows
    /// Genera el reporte en modo="WORK"
    /// </summary>
    /// <param name="pOIThread">OI del subproceso.</param>
    /// <param name="pOINode">OI del nodo.</param>
    /// <returns></returns>
    public static string GenerateHTMLForm(string pstrOIForm)
    {
      string strTemplateName;
      string strFileName;
      string strRootPath;
      NomadXML xmlParams;
      Nomad.NomadHTML objHtml;

      NucleusWF.Base.Definicion.Workflows.FORM objForm;
      //NucleusWF.Base.Definicion.Workflows.WF objWF;

      NomadLog.Info("Comienza GenerateHTMLForm ---------");

      //Busca el formulario para obtener el template a utilizar
      objForm = NucleusWF.Base.Definicion.Workflows.FORM.Get(pstrOIForm, false);

      //Genera el XML de parametros para ejecutar el reporte
      xmlParams = new NomadXML("PARAMS");
      xmlParams.SetAttr("mode", "WORK");
      xmlParams.SetAttr("id", pstrOIForm); //OI del formulario

      strTemplateName = objForm.d_template.Trim().ToUpper();

      if (strTemplateName.EndsWith(".XML"))
        strTemplateName = strTemplateName.Substring(0, strTemplateName.Length - 4);

      strTemplateName = strTemplateName.Replace("DEFINICION.WFPLA_", "EJECUCION.WFGEN_"); //Obtiene la plantilla de TRABAJO
      NomadLog.Info("Plantilla de TRABAJO: " + strTemplateName);
      objHtml = new Nomad.NomadHTML(strTemplateName, xmlParams.ToString());

      //Valida que exista el directorio de los formularios HTML -----------------------------------
      strRootPath = NomadProxy.GetProxy().RunPath + "\\Web";
      if (!Directory.Exists(strRootPath + "\\GenFiles\\WFForms\\"))
      {
        //El directorio no existe. Lo crea.
        Directory.CreateDirectory(strRootPath + "\\GenFiles\\WFForms\\");
      }

      //Guarda el archivo HTML generado -----------------------------------
      strFileName = "WF" + objForm.oi_wf.ToString() + "_F" + objForm.Id + "_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".htm"; //Completar con otros valores (descripcion y/o fecha)
      NomadLog.Info("Formulario generado: " + strRootPath + "\\GenFiles\\WFForms\\" + strFileName);

      //Genera el codigo HTML
      objHtml.GenerateHTML(strRootPath + "\\GenFiles\\WFForms\\" + strFileName, System.Text.Encoding.UTF8);

      NomadLog.Info("Fin GenerateHTMLForm --------------");

      return strFileName;

    }

    //Guarda las variables y valida los estados
    public static int Save(int pintThread, NomadXML pxmlData)
    {
      NucleusWF.Base.Ejecucion.Instancias.INSTANCE objINST;
      NucleusWF.Base.Ejecucion.Instancias.THREAD objTHREAD;

      NomadLog.Info("Comienza Save ---------");

      if (pxmlData.isDocument) pxmlData = pxmlData.FirstChild();

      //Realiza las validaciones
      objTHREAD = NucleusWF.Base.Ejecucion.Instancias.THREAD.Get(pintThread.ToString(), false);
      objINST = NucleusWF.Base.Ejecucion.Instancias.INSTANCE.Get(objTHREAD.oi_instance, false);

      Validar(objINST, "R", objTHREAD, "WP", pxmlData.GetAttrInt("paso"), true);

      //Se setean las variables
      SetVariables(objINST, pintThread, pxmlData);

      //Aumenta el paso
      objINST.e_pasos++;

      //Realiza el Save
      objINST.d_etty_creator=NomadProxy.GetProxy().UserEtty;
      NomadEnvironment.GetCurrentTransaction().SaveRefresh(objINST);
      NomadLog.Info("Fin Save ---------");

      return objINST.e_pasos;
    }

    //Pasa el WF al siguiente nodo
    public static void Pass(int pintThread, int pintPaso, string Etty)
    {
      NucleusWF.Base.Ejecucion.Instancias.INSTANCE objINST;
      NucleusWF.Base.Ejecucion.Instancias.THREAD objTHREAD;

      NomadLog.Info("Comienza Pass ---------");

      //Realiza las validaciones
      NomadLog.Info("Realiza las validaciones");
      objTHREAD = NucleusWF.Base.Ejecucion.Instancias.THREAD.Get(pintThread.ToString(), false);
      objINST   = NucleusWF.Base.Ejecucion.Instancias.INSTANCE.Get(objTHREAD.oi_instance, false);
      objTHREAD = (NucleusWF.Base.Ejecucion.Instancias.THREAD)objINST.THREADS.GetById(pintThread.ToString());

      Validar(objINST, "R", objTHREAD, "WP", pintPaso, true);

      //Agrego el LOG
      NomadLog.Info("Agrega el LOG");
      AddEtapaLog(objINST, objTHREAD, "IFO", "El formulario ha sido completado.", Etty);

      //Cambia el estado del thread
      NomadLog.Info("Cambia el estado del thread");
      objTHREAD.c_estado = "WE";
      objTHREAD.f_ejecucion = DateTime.Now;
      RefreshUSERS(objINST, objTHREAD);

      //Realiza el Save
      NomadLog.Info("Realiza el Save");
      objINST.d_etty_creator=NomadProxy.GetProxy().UserEtty;
      NomadEnvironment.GetCurrentTransaction().Save(objINST);
      Notify(objINST);

      NomadLog.Info("Fin Pass ---------");
    }

    public static void Delegar(int oi_thread, int e_Paso, string Etty, string c_rol, string c_estructura, string oi_entidad, string o_comentario)
    {
      NucleusWF.Base.Ejecucion.Instancias.INSTANCE objINST;
      NucleusWF.Base.Ejecucion.Instancias.THREAD objTHREAD;

      NomadLog.Info("Comienza Delegar ---------");

      //Realiza las validaciones
      NomadLog.Info("Realiza las validaciones");
      objTHREAD = NucleusWF.Base.Ejecucion.Instancias.THREAD.Get(oi_thread.ToString(), false);
      objINST   = NucleusWF.Base.Ejecucion.Instancias.INSTANCE.Get(objTHREAD.oi_instance, false);
      objTHREAD = (NucleusWF.Base.Ejecucion.Instancias.THREAD)objINST.THREADS.GetById(oi_thread.ToString());

      Validar(objINST, "R,E", objTHREAD, "WP", e_Paso, true);
      objINST.e_pasos++;
      objINST.c_estado_ej="R";
      objTHREAD.o_deleg_coment = o_comentario;

      /////////////////////////////////////////////////////////////////
      //Obtengo el Organigrama
      NucleusWF.Base.Definicion.Workflows.WF defWF=INSTANCE.GetWFDef(objINST.oi_wf);
      NucleusWF.Base.Definicion.Organigramas.ORGANIGRAMA defORG=INSTANCE.GetORGDef(defWF.oi_organigrama);
      string oi_organigrama=NomadEnvironment.QueryValue("WRK05_ORGANIGRAMAS", "oi_organigrama", "c_organigrama", defORG.c_organigrama, "WRK05_ORGANIGRAMAS.e_version="+defORG.e_version_pub, false);
      NucleusWF.Base.Definicion.Organigramas.ORGANIGRAMA defWRK=INSTANCE.GetORGDef(oi_organigrama);

      //////////////////////////////////////////////////////////////////////////////////////////////////////////////
      //Agrego el LOG
      NomadLog.Info("Agrega el LOG");
      AddEtapaLog(objINST, objTHREAD, "IFO", "El formulario ha sido Delegado.", Etty);

      //////////////////////////////////////////////////////////////////////////////////////////////////////////////
      //Agrego la INFORMACION DEL DELEGADOR
      NomadXML objETTY=NomadProxy.GetProxy().DDOService().GetXML("Nomad.Base.Login.Entidades.ENTIDAD", Etty);
      if (objETTY.isDocument) objETTY=objETTY.FirstChild();
      if (objETTY.Name.ToUpper()=="OBJECT") objETTY=objETTY.FirstChild();

      objTHREAD.d_deleg_etty = Etty;
      objTHREAD.d_deleg_ayn  = objETTY.GetAttr("DES");
      NomadXML AUXxml;

      //MAILS
      AUXxml=objETTY.FindElement("MAILS");
      if (AUXxml!=null)
        AUXxml=AUXxml.FindElement2("MAIL", "PRINCIPAL", "1");
      if (AUXxml!=null)
        objTHREAD.d_deleg_mail=AUXxml.GetAttr("EMAIL");

      //TELEFONO
      AUXxml=objETTY.FindElement("TELEFONOS");
      if (AUXxml!=null)
        AUXxml=AUXxml.FindElement2("TELEFONO", "PRINCIPAL", "1");
      if (AUXxml!=null)
        objTHREAD.d_deleg_tel=AUXxml.GetAttr("TELEF");

      //////////////////////////////////////////////////////////////////////////////////////////////////////////////
      //AGREGO LA INFORMACION DEL ROL
      NucleusWF.Base.Definicion.Organigramas.ROLE defROL=(NucleusWF.Base.Definicion.Organigramas.ROLE)defWRK.ROLES.GetByAttribute("c_role", c_rol);
      objTHREAD.c_asign_rol = defROL.c_role;
      objTHREAD.d_asign_titulo = defROL.d_role;

      //////////////////////////////////////////////////////////////////////////////////////////////////////////////
      //AGREGO LA INFORMACION DEL LA ESTRUCTURA
      if (c_estructura!="")
      {
        NucleusWF.Base.Definicion.Organigramas.ESTRUCTURA defEST=(NucleusWF.Base.Definicion.Organigramas.ESTRUCTURA)defWRK.ESTRUCTURA.GetByAttribute("c_estructura", c_estructura);
        objTHREAD.c_asign_estr = defEST.c_estructura;
        objTHREAD.d_asign_titulo = defROL.d_role+" "+defEST.d_estructura;

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //AGREGO LA INFORMACION DEL LA ENTIDAD
        if (oi_entidad!="")
        {
          NucleusWF.Base.Definicion.Organigramas.ENTIDAD_ESTR objENTIDAD=(NucleusWF.Base.Definicion.Organigramas.ENTIDAD_ESTR)defEST.ENTIDAD_ESTR.GetByAttribute("oi_entidad", oi_entidad);
          NomadXML objETTY_ASIG=NomadProxy.GetProxy().DDOService().GetXML("Nomad.Base.Login.Entidades.ENTIDAD", objENTIDAD.oi_entidad);

          if (objETTY_ASIG.isDocument) objETTY_ASIG=objETTY_ASIG.FirstChild();
          if (objETTY_ASIG.Name.ToUpper()=="OBJECT") objETTY_ASIG=objETTY_ASIG.FirstChild();

          objTHREAD.d_asign_etty = objENTIDAD.oi_entidad;
          objTHREAD.d_asign_ayn  = objETTY_ASIG.GetAttr("DES");

          //MAILS
          AUXxml=objETTY_ASIG.FindElement("MAILS");
          if (AUXxml!=null)
            AUXxml=AUXxml.FindElement2("MAIL", "PRINCIPAL", "1");
          if (AUXxml!=null)
            objTHREAD.d_asign_mail=AUXxml.GetAttr("EMAIL");

          //TELEFONO
          AUXxml=objETTY_ASIG.FindElement("TELEFONOS");
          if (AUXxml!=null)
            AUXxml=AUXxml.FindElement2("TELEFONO", "PRINCIPAL", "1");
          if (AUXxml!=null)
            objTHREAD.d_asign_tel=AUXxml.GetAttr("TELEF");
        } else
        {
          objTHREAD.d_asign_ettyNull = true;
          objTHREAD.d_asign_aynNull = true;
          objTHREAD.d_asign_mailNull = true;
          objTHREAD.d_asign_telNull = true;
        }

      } else
      {
        objTHREAD.c_asign_estrNull=true;

        objTHREAD.d_asign_ettyNull = true;
        objTHREAD.d_asign_aynNull = true;
        objTHREAD.d_asign_mailNull = true;
        objTHREAD.d_asign_telNull = true;
      }

      objTHREAD.c_estado="WP";
      objTHREAD.f_ejecucion  =DateTime.Now;

      //Agrego el elemento historico
      AppendHistVariables(objINST);

      //Aumenta el paso
      objINST.e_pasos++;

      //Realiza el Save
      RefreshUSERS(objINST, objTHREAD);
      objINST.d_etty_creator=NomadProxy.GetProxy().UserEtty;
      NomadEnvironment.GetCurrentTransaction().SaveRefresh(objINST);

      //Enviar Mails
      Notify(objINST);
      NomadLog.Info("Fin Delegar ---------");
    }


    public static void CorregirInstancias(int idOrganigrama)
    {
        NomadXML xmlInstancias = NomadEnvironment.QueryNomadXML(NucleusWF.Base.Ejecucion.Instancias.INSTANCE.Resources.QRY_INSTANCIAS_HUERFANAS, "<PARAM oi_organigrama='" + idOrganigrama + "' />").FirstChild();
        if (xmlInstancias != null)
        {
            IList listaInstancias = xmlInstancias.GetElements("ROW");
            foreach (NomadXML xmlIns in listaInstancias)
            {
                if (xmlIns.GetAttr("c_estructura_actual") != "" && xmlIns.GetAttr("c_estructura") != xmlIns.GetAttr("c_estructura_actual"))
                {
                    CambiarUsuariosQueResuelven(xmlIns.GetAttr("oi_thread"), xmlIns.FirstChild().GetElements("ROW"));

                    NucleusWF.Base.Ejecucion.Instancias.INSTANCE instancia = NucleusWF.Base.Ejecucion.Instancias.INSTANCE.Get(xmlIns.GetAttr("oi_instance"));
                    instancia.c_estructura = xmlIns.GetAttr("c_estructura_actual");
                    NomadEnvironment.GetCurrentTransaction().Save(instancia);
                }
            }
        }
    }

    private static void CambiarUsuariosQueResuelven(string oi_thread, IList list)
    {
        NomadXML xmlUsuarios = NomadEnvironment.QueryNomadXML(NucleusWF.Base.Ejecucion.Instancias.USER.Resources.INFO_THREAD, "<FILTRO OI_THREAD='" + oi_thread + "' />").FirstChild();
        IList listaUsuarios = xmlUsuarios.GetElements("ROW");
        DateTime f_resolucion = DateTime.Now;
        DateTime f_notificacion = DateTime.Now;
        bool l_notifica = false;
        bool l_resuelve = false;
        foreach (NomadXML xmlUs in listaUsuarios)
        {
            NucleusWF.Base.Ejecucion.Instancias.USER usuario = NucleusWF.Base.Ejecucion.Instancias.USER.Get(xmlUs.GetAttr("ID"));
            f_notificacion = usuario.f_notify;
            f_resolucion = usuario.f_resolve;
            l_notifica = usuario.l_notify;
            l_resuelve = usuario.l_resolve;
            try
            {
                NomadEnvironment.GetCurrentTransaction().Delete(usuario);
            }
            catch (Exception e)
            {
                NomadLog.Error("Error al borrar los aprobadoreas antiguos " + e.Message);
            }
        }

        NucleusWF.Base.Ejecucion.Instancias.THREAD th = NucleusWF.Base.Ejecucion.Instancias.THREAD.Get(oi_thread);
        foreach (NomadXML xmlnuevo in list)
        {
            NucleusWF.Base.Ejecucion.Instancias.USER nuevoUsuario = new NucleusWF.Base.Ejecucion.Instancias.USER();
            nuevoUsuario.f_resolve = f_resolucion;
            nuevoUsuario.f_notify = f_notificacion;
            nuevoUsuario.l_resolve = l_resuelve;
            nuevoUsuario.l_notify = l_notifica;
            nuevoUsuario.oi_etty = xmlnuevo.GetAttr("oi_entidad");
            nuevoUsuario.d_etty = xmlnuevo.GetAttr("d_entidad");
            nuevoUsuario.oi_thread = int.Parse(oi_thread);
            nuevoUsuario.c_rol = "2";

            th.USERS.Add(nuevoUsuario);
            try
            {
                NomadEnvironment.GetCurrentTransaction().SaveRefresh(th);
            }
            catch (Exception e)
            {
                NomadLog.Error("Error al guardar el aprobador " + e.Message);
            }
        }
    }
  }
}



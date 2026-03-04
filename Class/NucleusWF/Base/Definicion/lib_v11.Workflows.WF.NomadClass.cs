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

namespace NucleusWF.Base.Definicion.Workflows
{

    //////////////////////////////////////////////////////////////////////////////////
    //Clase WorkFlows
    public partial class WF : Nomad.NSystem.Base.NomadObject
    {

    public static bool CODE_TEST(string name)
    {
      char[] c=name.ToCharArray();

      for (int t=0; t<c.Length; t++)
      {
        if (c[t]=='_') continue;
        if (c[t]>='A' && c[t]<='Z') continue;
        if (c[t]>='a' && c[t]<='z') continue;
        if (c[t]>='0' && c[t]<='9' && t>0) continue;
        return false;
      }

      return true;
    }

    public static bool CODE_VAR_TEST(string name) {
      char[] c=name.ToCharArray();
      int intPointCounter = 0;

      for (int t=0; t<c.Length; t++) {
        if (c[t]=='.' && (t>0 && t<(c.Length-1))) {
          intPointCounter++;
          if (intPointCounter > 1) return false;

          continue;
        }

        if (c[t]=='_') continue;
        if (c[t]>='A' && c[t]<='Z') continue;
        if (c[t]>='a' && c[t]<='z') continue;
        if (c[t]>='0' && c[t]<='9' && t>0) continue;
        return false;
      }

      return true;
    }

    public static NomadXML VariablesWF(WF wf)
    {
      NomadXML retval, xmlVars;
      Hashtable Valores=new Hashtable();

      //xml Respuesta
      retval=new NomadXML("WF");

      //Agrego Grupos Genericos
      xmlVars=NomadProxy.GetProxy().SQLService().GetXML(Resources.VariablesInternas,"");
      if (xmlVars.isDocument) xmlVars=xmlVars.FirstChild();

    //Agrego la lista de Estados
    NomadXML xmlGROUPINST=xmlVars.FindElement2("GROUP","name","Instancia");
    NomadXML xmlVARESTADO=xmlGROUPINST.FindElement2("VAR","name","Estado");

    //Recorrolos Estado y agrego la Enumeracion
      if (wf!=null)
      {
      foreach(ESTADO est in wf.ESTADOS)
      {
        NomadXML xmlVARITEM=xmlVARESTADO.AddTailElement("ITEM");
        xmlVARITEM.SetAttr("VAL", est.c_estado);
        xmlVARITEM.SetAttr("TXT", est.d_estado);
      }
    }

    //Recorro los grupos y los agrego
      for (NomadXML GROUP=xmlVars.FirstChild(); GROUP!=null; GROUP=GROUP.Next())
      {
        Nomad.Base.Report.Variables.AddGroupDef(retval, GROUP.GetAttr("NAME"), GROUP.GetAttr("TYPE"));
        for (NomadXML VAR=GROUP.FirstChild(); VAR!=null; VAR=VAR.Next())
        {

      Hashtable enumValues=null;
      if (VAR.GetAttr("TYPE")=="enum")
      {
        enumValues = new Hashtable();
        for (NomadXML ITEM=VAR.FirstChild(); ITEM!=null; ITEM=ITEM.Next())
          enumValues.Add(ITEM.GetAttr("VAL"),ITEM.GetAttr("TXT"));
      }
      Nomad.Base.Report.Variables.AddVariableDef(retval, GROUP.GetAttr("NAME")+"."+VAR.GetAttr("NAME"), VAR.GetAttr("TYPE"), enumValues);
        }
      }

      //Agrego Grupos Definidos por el Usuario
      if (wf!=null)
      {
        foreach(VAR_GROUP grp in wf.GROUPS)
        {
          switch(grp.c_mode)
          {
            case "MAIN":   Nomad.Base.Report.Variables.AddGroupDef(retval, grp.c_var_group, "simple"); break;

            case "HIST":   Nomad.Base.Report.Variables.AddGroupDef(retval, grp.c_var_group, "coleccion"); break;
            case "CHILD":  Nomad.Base.Report.Variables.AddGroupDef(retval, grp.c_var_group, "coleccion"); break;

            case "AUX":    Nomad.Base.Report.Variables.AddGroupDef(retval, grp.c_var_group, "simple"); break;
            case "ARRAUX": Nomad.Base.Report.Variables.AddGroupDef(retval, grp.c_var_group, "coleccion"); break;

            default: throw new Exception("Tipo '"+grp.c_mode+"' de grupo de variable '"+grp.c_var_group+"' no Valido.");
          }

          //Agrego las Variables Definidas por el Usuario
          foreach(VAR var in grp.VARS)
          {
            Valores.Clear();
            if (var.c_type.ToUpper()=="ENUM")
              foreach(VAR_COMBO it in var.VARS_COMBO)
                Valores[it.c_var_combo.ToUpper()]=it.d_var_combo;

            Nomad.Base.Report.Variables.AddVariableDef(retval, grp.c_var_group+"."+var.c_var, var.c_type, Valores);
          }

        }
      }

      NomadLog.Info("VAR: "+retval.ToString());

      //fin
      return retval;
   }

    public static void Exportar(string exportList)
    {
      NomadBatch MyBATCH=NomadBatch.GetBatch("Exportar", "Exportar");
      string[] aExportList=exportList.Substring(1,exportList.Length-2).Split(';');
      string[] aExportItem;
      string exportItem;
      NomadXML result=new NomadXML("WF-EXPORT");

      //Recorro los Items
      for (int t=0; t<aExportList.Length; t++)
      {
        MyBATCH.SetPro(0,90,aExportList.Length, t);
        MyBATCH.SetMess("Exportando Elemento "+(t+1).ToString()+" de "+aExportList.Length.ToString());

        exportItem=aExportList[t];
        if (exportItem=="") continue;

        //switch segun el Tipo de ITEM
        aExportItem=exportItem.Split(':');

        NomadLog.Debug("Exportando elemento: "+exportItem);

        switch(aExportItem[0])
        {
          case "ORG":
            NucleusWF.Base.Definicion.Organigramas.ORGANIGRAMA.Exportar(aExportItem[1], result);
            break;

          case "WRK":
            NucleusWF.Base.Definicion.Workflows.WF.Exportar(aExportItem[1], result);
            break;

          case "INT":
            NucleusWF.Base.Definicion.Integracion.INT_MAG.Exportar(aExportItem[1], result);
            break;
        }
      }

      MyBATCH.SetPro(90);
      MyBATCH.SetMess("Publicando...");
      NomadProxy.GetProxy().FileServiceIO().SaveFile("INTERFACES", NomadProxy.GetProxy().Batch().ID+".txt", result.ToString());
    }

    public static void Exportar(string ID, NomadXML result)
    {
      //Obtengo el OBJETO
      NomadXML CHILD;
      WF objExp=WF.Get(ID,false);
      NucleusWF.Base.Definicion.Organigramas.ORGANIGRAMA objORG=NucleusWF.Base.Definicion.Organigramas.ORGANIGRAMA.Get(objExp.oi_organigrama,false);

      //Agrego el ITEM a la SELECCION ACTUAL
      NomadXML newElement=result.AddTailElement("ITEM");
      newElement.SetAttr("TYPE"   , "WRK");
      newElement.SetAttr("ID"     , ID);
      newElement.SetAttr("CODE"   , objExp.c_wf);
      newElement.SetAttr("LABEL"  , "["+objExp.c_wf+"]"+objExp.d_wf);
      newElement.SetAttr("VERSION", objExp.l_automatica?objExp.e_version.ToString():"N/A");

      //Elemento RAIZ
      NomadXML xmlWF=newElement.AddTailElement("WF");
      xmlWF.SetAttr("CODE"   , objExp.c_wf);
      xmlWF.SetAttr("DESC"   , objExp.d_wf);
      xmlWF.SetAttr("TEXT"   , objExp.c_text);
      xmlWF.SetAttr("HOR"    , (objExp.n_horas==0?"":objExp.n_horas.ToString()));

      //Roles
      CHILD=xmlWF.AddTailElement("ROLES");
      foreach(NucleusWF.Base.Definicion.Organigramas.ROLE MyELE in objORG.ROLES)
      {
        NomadXML ELE=CHILD.AddTailElement("ROLE");
        ELE.SetAttr("CODE"  , MyELE.c_role);
        ELE.SetAttr("DESC"  , MyELE.d_role);
        ELE.SetAttr("UNIQUE", MyELE.l_unique);
        ELE.SetAttr("COLOR" , MyELE.d_color);
      }

      //Pantallas
      CHILD=xmlWF.AddTailElement("FORMS");
      foreach(FORM MyELE in objExp.FORMS)
      {
        NomadXML ELE=CHILD.AddTailElement("FORM");
        ELE.SetAttr("CODE"    , MyELE.c_form);
        ELE.SetAttr("DESC"    , MyELE.d_form);
        ELE.SetAttr("TEMPLATE", MyELE.d_template);
        ELE.SetAttr("SCRIPT_1", MyELE.o_scripts_1);
        ELE.SetAttr("SCRIPT_2", MyELE.o_scripts_2);
        ELE.SetAttr("SCRIPT_3", MyELE.o_scripts_3);
        ELE.SetAttr("SCRIPT_4", MyELE.o_scripts_4);
        ELE.SetAttr("FILE"    , MyELE.d_file_html);
        ELE.SetAttr("MODE"    , MyELE.c_file_mode);

        if (objExp.l_automatica)
        {
          //Es automatica, agregar los archivos
          NomadXML ELE3;

          int readlen;
          Byte[] MyBuff=new Byte[3000];
          StreamReader MyFILE=new StreamReader(NomadProxy.GetProxy().RunPath+"\\Web\\GenFiles\\WFForms\\"+MyELE.d_file_html);

          do
          {
            readlen=MyFILE.BaseStream.Read(MyBuff,0,3000);
            if (readlen>0)
            {
              ELE3=ELE.AddTailElement("BLOCK");
              ELE3.SetAttr("",Convert.ToBase64String(MyBuff,0,readlen));
            }
          } while (readlen==3000);
          MyFILE.Close();
        } else
        {
          //No es automatica, agregar el contenido
          foreach(FORM_GROUP MyELE2 in MyELE.FORM_GROUPS)
          {
            NomadXML ELE2=ELE.AddTailElement("GROUP");
            ELE2.SetAttr("CODE"     , MyELE2.c_form_group);
            ELE2.SetAttr("DESC"     , MyELE2.d_form_group);
            ELE2.SetAttr("ORDER"    , MyELE2.c_order);
            ELE2.SetAttr("MODE"     , MyELE2.c_mode);
            ELE2.SetAttr("GROUP"    , MyELE2.c_var_group);
            ELE2.SetAttr("PARAM_1"  , MyELE2.d_param_1);
            ELE2.SetAttr("PARAM_2"  , MyELE2.d_param_2);
            ELE2.SetAttr("PARAM_3"  , MyELE2.d_param_3);
            ELE2.SetAttr("TEXT"     , MyELE2.o_text);
            ELE2.SetAttr("ON-CHANGE", MyELE2.o_onchange);

            foreach(FIELD MyELE3 in MyELE2.FORM_FIELDS)
            {
              NomadXML ELE3=ELE2.AddTailElement("FIELD");
              ELE3.SetAttr("CODE"       , MyELE3.c_field);
              ELE3.SetAttr("DESC"       , MyELE3.d_field);
              ELE3.SetAttr("ORDER"      , MyELE3.e_order);
              ELE3.SetAttr("MODE"       , MyELE3.c_mode);
              ELE3.SetAttr("VAR"        , MyELE3.c_var);
              ELE3.SetAttr("PARENT"     , MyELE3.c_parent_field);
              ELE3.SetAttr("VALUE"      , MyELE3.c_valor);
              ELE3.SetAttr("ON-VALIDATE", MyELE3.o_onvalidate);
              ELE3.SetAttr("ON-CHANGE"  , MyELE3.o_onchange);
              ELE3.SetAttr("PARAM_1"    , MyELE3.d_param_1);
              ELE3.SetAttr("PARAM_2"    , MyELE3.d_param_2);
              ELE3.SetAttr("PARAM_3"    , MyELE3.d_param_3);
            }
          }
        }
      }

      //Procesos
      CHILD=xmlWF.AddTailElement("PROCESS");
      foreach(NucleusWF.Base.Definicion.Workflows.PROCESS MyELE in objExp.PROCESS)
      {
        NomadXML ELEC;
        NomadXML ELE=CHILD.AddTailElement("PRO");
        ELE.SetAttr("CODE"    , MyELE.c_process);
        ELE.SetAttr("DESC"    , MyELE.d_process);
        ELE.SetAttr("AUTO"    , MyELE.e_automatico);
        ELE.SetAttr("CODE2"   , MyELE.c_process+"-"+MyELE.e_automatico);

        //Roles
        ELEC=ELE.AddTailElement("ROLES");
        foreach(ROLE_PROCESS MyELE1 in MyELE.ROLES_PRO)
        {
          NomadXML ELE1=ELEC.AddTailElement("ROLE");
          ELE1.SetAttr("ROLE"  , MyELE1.c_rol);
          ELE1.SetAttr("INI"   , MyELE1.l_iniciar);
        }

        //Params
        ELEC=ELE.AddTailElement("PARAMS");
        foreach(NucleusWF.Base.Definicion.Workflows.PROCESS_PARAM MyELE1 in MyELE.PROCESS_PARAM)
        {
          NomadXML ELE1=ELEC.AddTailElement("PARAM");
          ELE1.SetAttr("CODE"  , MyELE1.c_process_param);
          ELE1.SetAttr("DESC"  , MyELE1.d_process_param);
          ELE1.SetAttr("TYPE"  , MyELE1.c_type);
          ELE1.SetAttr("IO"    , MyELE1.c_io);
        }

        //Nodos
        ELEC=ELE.AddTailElement("NODES");
        foreach(NODE MyELE1 in MyELE.NODES)
        {
          NomadXML ELE2;
          NomadXML ELE1=ELEC.AddTailElement("NODE");
          ELE1.SetAttr("TYPE"    , MyELE1.c_type);
          ELE1.SetAttr("SEC"     , MyELE1.n_secuence);
          ELE1.SetAttr("LABEL"   , MyELE1.d_label);
          ELE1.SetAttr("PARAM_1" , MyELE1.d_param_1);
          ELE1.SetAttr("PARAM_2" , MyELE1.d_param_2);
          ELE1.SetAttr("PARAM_3" , MyELE1.d_param_3);
          ELE1.SetAttr("PARAM_4" , MyELE1.d_param_4);
          ELE1.SetAttr("HOR"     , (MyELE1.n_horas==0?"":MyELE1.n_horas.ToString()));
          ELE1.SetAttr("FORMULA"      , MyELE1.o_formula);
          ELE1.SetAttr("PROCESS"      , MyELE1.o_process);
          ELE1.SetAttr("COMP-FORMULA" , MyELE1.o_compformula);
          ELE1.SetAttr("COMP-PROCESS" , MyELE1.o_compprocess);

          //Roles
          ELE2=ELE1.AddTailElement("ROLES");
          foreach(ROLE_NODE MyELE3 in MyELE1.ROLES_NODE)
          {
            NomadXML ELE3=ELE2.AddTailElement("ROLE");
            ELE3.SetAttr("ROLE"      , MyELE3.c_rol);
            ELE3.SetAttr("NOTIFY"    , MyELE3.c_notificar);
            ELE3.SetAttr("RESOLVE"   , MyELE3.c_resolver);
            ELE3.SetAttr("DELEGA"    , MyELE3.c_puede_deleg);
            ELE3.SetAttr("TO-DELEGA" , MyELE3.d_rol_delega);
          }

          //Parametros
          ELE2=ELE1.AddTailElement("PARAMS");
          foreach(PARAM MyELE3 in MyELE1.NODE_PARAMS)
          {
            NomadXML ELE3=ELE2.AddTailElement("PARAM");
            ELE3.SetAttr("CODE"      , MyELE3.c_param);
            ELE3.SetAttr("DESC"      , MyELE3.d_param);
            ELE3.SetAttr("TYPE"      , MyELE3.c_type);

            ELE3.SetAttr("ORDER"      , MyELE3.e_orden);
            ELE3.SetAttr("SUBPROCESS" , MyELE3.e_subproceso);
            ELE3.SetAttr("VAR"        , MyELE3.c_var);
            ELE3.SetAttr("VALUE"      , MyELE3.d_valor);

            ELE3.SetAttr("FORMULA"    , MyELE3.o_formula);
            ELE3.SetAttr("COMPILE"    , MyELE3.o_compilada);
          }
        }

      }

      //Grupo de Variables
      CHILD=xmlWF.AddTailElement("VAR-GROUPS");
      foreach(VAR_GROUP MyELE in objExp.GROUPS)
      {
        NomadXML ELE=CHILD.AddTailElement("GROUP");
        ELE.SetAttr("CODE"    , MyELE.c_var_group);
        ELE.SetAttr("DESC"    , MyELE.d_var_group);
        ELE.SetAttr("MODE"    , MyELE.c_mode);

        //Variables
        foreach(VAR MyELE1 in MyELE.VARS)
        {
          NomadXML ELE1=ELE.AddTailElement("VAR");
          ELE1.SetAttr("CODE"      , MyELE1.c_var);
          ELE1.SetAttr("DESC"      , MyELE1.d_var);
          ELE1.SetAttr("TYPE"      , MyELE1.c_type);
          ELE1.SetAttr("COL"       , MyELE1.c_column);
          ELE1.SetAttr("COL_DESC"  , MyELE1.c_column_desc);
          ELE1.SetAttr("ORDEN"     , MyELE1.e_order);

          ELE1.SetAttr("FORMULA"   , MyELE1.o_formula);
          ELE1.SetAttr("COMP"      , MyELE1.o_compilada);

          ELE1.SetAttr("FK"        , MyELE1.d_type_fk);
          ELE1.SetAttr("SHOW"      , MyELE1.l_show_console);
          ELE1.SetAttr("COPY"      , MyELE1.l_hist_copy);
          ELE1.SetAttr("COPY-DELEG", MyELE1.l_deleg_copy);

          //Variables Valores
          foreach(VAR_COMBO MyELE2 in MyELE1.VARS_COMBO)
          {
            NomadXML ELE2=ELE1.AddTailElement("VALUE");
            ELE2.SetAttr("CODE"      , MyELE2.c_var_combo);
            ELE2.SetAttr("DESC"      , MyELE2.d_var_combo);
          }
        }
      }

      //Estados
      CHILD=xmlWF.AddTailElement("STATES");
      foreach(ESTADO MyELE in objExp.ESTADOS)
      {
        NomadXML ELE=CHILD.AddTailElement("STATE");
        ELE.SetAttr("CODE"    , MyELE.c_estado);
        ELE.SetAttr("DESC"    , MyELE.d_estado);
        ELE.SetAttr("COLOR"   , MyELE.c_color);
    ELE.SetAttr("HORASDEL" , MyELE.n_horas_delete);
    ELE.SetAttr("PAUSE"   , MyELE.l_pause);
    ELE.SetAttr("STOP"    , MyELE.l_stop);
    ELE.SetAttr("DELETE"  , MyELE.l_delete);
  }

      //Textos
      CHILD=xmlWF.AddTailElement("TEXTS");
      foreach(TEXT MyELE in objExp.NODE_TEXTS)
      {
        NomadXML ELE=CHILD.AddTailElement("TEXT");
        ELE.SetAttr("CODE"    , MyELE.c_text);
        ELE.SetAttr("TYPE"    , MyELE.c_tipo);
        ELE.SetAttr("TEXT_1"  , MyELE.o_text);
        ELE.SetAttr("TEXT_2"  , MyELE.o_text2);
        ELE.SetAttr("TEXT_3"  , MyELE.o_text3);
        ELE.SetAttr("TEXT_4"  , MyELE.o_text4);
      }
    }

    public static void Importar(string itemID, string c_modo, string oi_wf, string c_code, string c_desc, string oi_organigrama)
    {
      NomadBatch MyBATCH=NomadBatch.GetBatch("Importar", "Importar");
      NomadXML objXML,objITEM,curs,curs2,curs3,cur,cur2,cur3;
      ArrayList arrAUX=new ArrayList();

      int c;
      bool isOK;
      WF wfTrunk=null;
      WF wfNew  =null;
      NucleusWF.Base.Definicion.Organigramas.ORGANIGRAMA orgLink=NucleusWF.Base.Definicion.Organigramas.ORGANIGRAMA.Get(oi_organigrama,false);

      //Creo el Organigrama RAIZ
      switch(c_modo)
      {
        case "N":
          wfNew=new WF();
          wfNew.c_wf         =c_code;
          wfNew.d_wf         =c_desc;
          wfNew.e_version    =1;
          wfNew.e_version_pub=0;
          wfNew.l_automatica =false;
          wfNew.c_estado     ="D";
          break;

        case "O":
          wfNew=WF.Get(oi_wf,false);
          wfNew.e_version=wfNew.e_version+1;
          break;

        case "A":
          //Incremento la Version del Organigrama Principal
          wfTrunk=WF.Get(oi_wf,false);
          wfTrunk.e_version=wfTrunk.e_version+1;

          //Creo el Publicado
          wfNew=new WF();
          wfNew.c_wf         =wfTrunk.c_wf;
          wfNew.d_wf         =wfTrunk.d_wf;
          wfNew.e_version    =wfTrunk.e_version-1;
          wfNew.e_version_pub=wfTrunk.e_version-1;
          wfNew.l_automatica =true;
          wfNew.c_estado     ="R";
          break;
      }

      //Obtengo el XML
      objXML=NomadProxy.GetProxy().FileServiceIO().LoadFileXML("TEMP","wfimportfile.txt");
      if (objXML.isDocument) objXML=objXML.FirstChild();

      //Busco el ITEM
      objITEM=null;
      for(cur=objXML.FirstChild(); cur!=null && objITEM==null; cur=cur.Next())
        if (cur.GetAttr("TYPE")+":"+cur.GetAttr("ID")==itemID)
          objITEM=cur;
      if (objITEM==null)
        throw new Exception("ITEM 'ID="+itemID+"' no encontrado.");

      //////////////////////////////////////////////////////////////////////////////////////////////////
      //Actualizar Valores Adicionales
      wfNew.c_text        =objITEM.FirstChild().GetAttr("TEXT");
      wfNew.n_horas       =objITEM.FirstChild().GetAttrInt("HOR");
      wfNew.n_horasNull   =(objITEM.FirstChild().GetAttr("HOR")=="");
      wfNew.oi_organigrama=oi_organigrama;

      //////////////////////////////////////////////////////////////////////////////////////////////////
      //Verificacion de ROLES
      isOK=true;
      curs=objITEM.FirstChild().FindElement("ROLES");
      for(cur=curs.FirstChild(); cur!=null; cur=cur.Next())
      {

        NucleusWF.Base.Definicion.Organigramas.ROLE role=(NucleusWF.Base.Definicion.Organigramas.ROLE)orgLink.ROLES.GetByAttribute("c_role",cur.GetAttr("CODE"));
        if (role==null)
        {
          isOK=false;
          MyBATCH.Err("No se encontro el ROLE '"+cur.GetAttr("CODE")+"' en el organigrama seleccionado.");
          continue;
        }

        if (role.d_role!=cur.GetAttr("DESC") || role.l_unique!=cur.GetAttrBool("UNIQUE"))
        {
          isOK=false;
          MyBATCH.Err("La definicion del ROLE '"+cur.GetAttr("CODE")+"' no coincide con la del organigrama seleccionado.");
          continue;
        }
      }
      if (!isOK)
        throw new Exception("Se detectaron inconsistencias en los ROLES.");

      //////////////////////////////////////////////////////////////////////////////////////////////////
      //Actualizacion de ESTADOS
      MyBATCH.SetMess("Copiando ESTADOS....");
      MyBATCH.Log("Copiando ESTADOS....");
      MyBATCH.SetPro(10);

      curs=objITEM.FirstChild().FindElement("STATES");

      // Eliminar
      arrAUX.Clear();
      foreach(ESTADO state in wfNew.ESTADOS)
      {
        if (curs.FindElement2("STATE","CODE",state.c_estado)==null)
        {
          NomadLog.Info("Elimino el Estado '"+state.c_estado+"' de la Lista....");
          arrAUX.Add(state);
        }
      }
      foreach(ESTADO state in arrAUX)
        wfNew.ESTADOS.Remove(state);

      // Agregar/Modificar
      for (cur=curs.FirstChild(); cur!=null; cur=cur.Next())
      {
        ESTADO state=(ESTADO)wfNew.ESTADOS.GetByAttribute("c_estado",cur.GetAttr("CODE"));
        if (state==null)
        { //No existe crear
          state=new ESTADO();
          state.c_estado=cur.GetAttr("CODE");
          state.d_estado=cur.GetAttr("DESC");
          state.c_color =cur.GetAttr("COLOR").ToUpper();
      state.n_horas_delete=cur.GetAttrInt("HORASDEL");
      state.n_horas_deleteNull=cur.GetAttrInt("HORASDEL")==0?true:false;
      state.l_pause     =cur.GetAttrBool("PAUSE");
      state.l_stop      =cur.GetAttrBool("STOP");
      state.l_delete    =cur.GetAttrBool("DELETE");
      wfNew.ESTADOS.Add(state);

          //Para la Validacion (Caso Publicada...)
          NomadLog.Info("Agregando el Estado '"+state.c_estado+"' a la Lista....");
        } else
        { //Existe modificar
          state.c_estado=cur.GetAttr("CODE");
          state.d_estado=cur.GetAttr("DESC");
          state.c_color =cur.GetAttr("COLOR").ToUpper();
      state.n_horas_delete=cur.GetAttrInt("HORASDEL");
      state.n_horas_deleteNull=cur.GetAttrInt("HORASDEL")==0?true:false;
      state.l_pause     =cur.GetAttrBool("PAUSE");
      state.l_stop      =cur.GetAttrBool("STOP");
      state.l_delete    =cur.GetAttrBool("DELETE");

          //Para la Validacion (Caso Publicada...)
          NomadLog.Info("Actualizo el Estado '"+state.c_estado+"' a la Lista....");
        }
      }

      //////////////////////////////////////////////////////////////////////////////////////////////////
      //Actualizacion de TEXTOS
      MyBATCH.SetMess("Copiando TEXTOS....");
      MyBATCH.Log("Copiando TEXTOS....");
      MyBATCH.SetPro(20);

      curs=objITEM.FirstChild().FindElement("TEXTS");

      // Eliminar
      arrAUX.Clear();
      foreach(TEXT txt in wfNew.NODE_TEXTS)
      {
        if (curs.FindElement2("TEXT","CODE",txt.c_text)==null)
        {
          NomadLog.Info("Elimino el Texto '"+txt.c_text+"' de la Lista....");
          arrAUX.Add(txt);
        }
      }
      foreach(TEXT txt in arrAUX)
        wfNew.NODE_TEXTS.Remove(txt);

      // Agregar/Modificar
      for (cur=curs.FirstChild(); cur!=null; cur=cur.Next())
      {
        TEXT txt=(TEXT)wfNew.NODE_TEXTS.GetByAttribute("c_text",cur.GetAttr("CODE"));
        if (txt==null)
        { //No existe crear
          txt=new TEXT();
          txt.c_text =cur.GetAttr("CODE");
          txt.c_tipo =cur.GetAttr("TYPE");
          txt.o_text =cur.GetAttr("TEXT_1");
          txt.o_text2=cur.GetAttr("TEXT_2");
          txt.o_text3=cur.GetAttr("TEXT_3");
          txt.o_text4=cur.GetAttr("TEXT_4");
          wfNew.NODE_TEXTS.Add(txt);

          //Para la Validacion (Caso Publicada...)
          NomadLog.Info("Agregando el Texto '"+txt.c_text+"' a la Lista....");
        } else
        { //Existe modificar
          txt.c_text =cur.GetAttr("CODE");
          txt.c_tipo =cur.GetAttr("TYPE");
          txt.o_text =cur.GetAttr("TEXT_1");
          txt.o_text2=cur.GetAttr("TEXT_2");
          txt.o_text3=cur.GetAttr("TEXT_3");
          txt.o_text4=cur.GetAttr("TEXT_4");

          //Para la Validacion (Caso Publicada...)
          NomadLog.Info("Actualizo el Texto '"+txt.c_text+"' a la Lista....");
        }
      }

      //////////////////////////////////////////////////////////////////////////////////////////////////
      //Actualizacion de VAR-GROUPS
      MyBATCH.SetMess("Copiando VAR-GROUPS....");
      MyBATCH.Log("Copiando VAR-GROUPS....");
      MyBATCH.SetPro(30);

      curs=objITEM.FirstChild().FindElement("VAR-GROUPS");

      // Eliminar
      arrAUX.Clear();
      foreach(VAR_GROUP vgroup in wfNew.GROUPS)
      {
        if (curs.FindElement2("GROUP","CODE",vgroup.c_var_group)==null)
        {
          NomadLog.Info("Elimino el Grupo de Variables '"+vgroup.c_var_group+"' de la Lista....");
          arrAUX.Add(vgroup);
        }
      }
      foreach(VAR_GROUP vgroup in arrAUX)
      {
        foreach(VAR vardata in vgroup.VARS) c=vardata.VARS_COMBO.Count;
        wfNew.GROUPS.Remove(vgroup);
      }

      // Agregar/Modificar
      for (cur=curs.FirstChild(); cur!=null; cur=cur.Next())
      {
        VAR_GROUP vgroup=(VAR_GROUP)wfNew.GROUPS.GetByAttribute("c_var_group",cur.GetAttr("CODE"));
        if (vgroup==null)
        { //No existe crear
          vgroup=new VAR_GROUP();
          vgroup.c_var_group=cur.GetAttr("CODE");
          vgroup.d_var_group=cur.GetAttr("DESC");
          vgroup.c_mode     =cur.GetAttr("MODE");

          wfNew.GROUPS.Add(vgroup);

          //Para la Validacion (Caso Publicada...)
          NomadLog.Info("Agregando el Grupo de Variables '"+vgroup.c_var_group+"' a la Lista....");
        } else
        { //Existe modificar
          vgroup.c_var_group=cur.GetAttr("CODE");
          vgroup.d_var_group=cur.GetAttr("DESC");
          vgroup.c_mode     =cur.GetAttr("MODE");

          //Para la Validacion (Caso Publicada...)
          NomadLog.Info("Actualizo el Grupo de Variables '"+vgroup.c_var_group+"' a la Lista....");
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////
        //Actualizacion de VARS

        // Eliminar
        arrAUX.Clear();
        foreach(VAR vardata in vgroup.VARS)
        {
          if (cur.FindElement2("VAR","CODE",vardata.c_var)==null)
          {
            NomadLog.Info("Elimino la Variable '"+vardata.c_var+"' de la Lista....");
            arrAUX.Add(vardata);
          }
        }
        foreach(VAR vardata in arrAUX)
        {
          c=vardata.VARS_COMBO.Count;
          vgroup.VARS.Remove(vardata);
        }

        for (cur2=cur.FirstChild(); cur2!=null; cur2=cur2.Next())
        {
         VAR vardata=(VAR)vgroup.VARS.GetByAttribute("c_var",cur2.GetAttr("CODE"));
          if (vardata==null)
          { //No existe crear
            vardata=new VAR();
            vardata.c_var         =cur2.GetAttr("CODE");
            vardata.d_var         =cur2.GetAttr("DESC");
            vardata.c_type        =cur2.GetAttr("TYPE");
            vardata.c_column      =cur2.GetAttr("COL");
            vardata.c_column_desc =cur2.GetAttr("COL_DESC");
            vardata.e_order       =cur2.GetAttrInt("ORDEN");
            vardata.o_formula     =cur2.GetAttr("FORMULA");
            vardata.o_compilada   =cur2.GetAttr("COMP");
            vardata.d_type_fk     =cur2.GetAttr("FK");
            vardata.l_show_console=cur2.GetAttrBool("SHOW");
            vardata.l_hist_copy   =cur2.GetAttrBool("COPY");
            vardata.l_deleg_copy  =cur2.GetAttrBool("COPY-DELEG");

            vgroup.VARS.Add(vardata);

            //Para la Validacion (Caso Publicada...)
            NomadLog.Info("Agregando la Variables '"+vardata.c_var+"' a la Lista....");
          } else
          { //Existe modificar
            vardata.c_var         =cur2.GetAttr("CODE");
            vardata.d_var         =cur2.GetAttr("DESC");
            vardata.c_type        =cur2.GetAttr("TYPE");
            vardata.c_column      =cur2.GetAttr("COL");
            vardata.c_column_desc =cur2.GetAttr("COL_DESC");
            vardata.e_order       =cur2.GetAttrInt("ORDEN");
            vardata.o_formula     =cur2.GetAttr("FORMULA");
            vardata.o_compilada   =cur2.GetAttr("COMP");
            vardata.d_type_fk     =cur2.GetAttr("FK");
            vardata.l_show_console=cur2.GetAttrBool("SHOW");
            vardata.l_hist_copy   =cur2.GetAttrBool("COPY");
            vardata.l_deleg_copy  =cur2.GetAttrBool("COPY-DELEG");

            //Para la Validacion (Caso Publicada...)
            NomadLog.Info("Actualizo la Variables '"+vardata.c_var+"' a la Lista....");
          }

          //////////////////////////////////////////////////////////////////////////////////////////////////
          //Actualizacion de VAR_COMBO

          // Eliminar
          arrAUX.Clear();
          foreach(VAR_COMBO varvalue in vardata.VARS_COMBO)
          {
            if (cur2.FindElement2("VALUE","CODE",varvalue.c_var_combo)==null)
            {
              NomadLog.Info("Elimino el Valor '"+varvalue.c_var_combo+"' de la Lista....");
              arrAUX.Add(varvalue);
            }
          }
          foreach(VAR_COMBO varvalue in arrAUX)
            vardata.VARS_COMBO.Remove(varvalue);

          for (cur3=cur2.FirstChild(); cur3!=null; cur3=cur3.Next())
          {
           VAR_COMBO varvalue=(VAR_COMBO)vardata.VARS_COMBO.GetByAttribute("c_var_combo",cur3.GetAttr("CODE"));
            if (varvalue==null)
            { //No existe crear
              varvalue=new VAR_COMBO();
              varvalue.c_var_combo=cur3.GetAttr("CODE");
              varvalue.d_var_combo=cur3.GetAttr("DESC");

              vardata.VARS_COMBO.Add(varvalue);

              //Para la Validacion (Caso Publicada...)
              NomadLog.Info("Agregando el Valor '"+varvalue.c_var_combo+"' a la Lista....");
            } else
            { //Existe modificar
              varvalue.c_var_combo=cur3.GetAttr("CODE");
              varvalue.d_var_combo=cur3.GetAttr("DESC");

              //Para la Validacion (Caso Publicada...)
              NomadLog.Info("Actualizo el Valor '"+varvalue.c_var_combo+"' a la Lista....");
            }
          }

        }
      }

      //////////////////////////////////////////////////////////////////////////////////////////////////
      //Guardo el DDO
      MyBATCH.SetMess("Guardando las ESTADOS, TEXTOS Y VARIABLES....");
      MyBATCH.Log("Guardando las ESTADOS, TEXTOS Y VARIABLES....");
      MyBATCH.SetPro(40);
      NomadEnvironment.GetCurrentTransaction().Begin();
      if (wfTrunk!=null) NomadEnvironment.GetCurrentTransaction().SaveRefresh(wfTrunk);
      NomadEnvironment.GetCurrentTransaction().SaveRefresh(wfNew);
      NomadEnvironment.GetCurrentTransaction().Commit();
      wfNew=WF.Get(wfNew.Id,false);

      //////////////////////////////////////////////////////////////////////////////////////////////////
      //Actualizacion de FORM
      MyBATCH.SetMess("Copiando FORMULARIOS....");
      MyBATCH.Log("Copiando FORMULARIOS....");
      MyBATCH.SetPro(20);

      curs=objITEM.FirstChild().FindElement("FORMS");

      // Eliminar
      arrAUX.Clear();
      foreach(FORM form in wfNew.FORMS)
      {
        if (curs.FindElement2("FORM","CODE",form.c_form)==null)
        {
          NomadLog.Info("Elimino el Formulario '"+form.c_form+"' de la Lista....");
          arrAUX.Add(form);
        }
      }
      foreach(FORM form in arrAUX)
      {
        foreach(FORM_GROUP group in arrAUX)
          c=group.FORM_FIELDS.Count;

        wfNew.FORMS.Remove(form);
      }

      // Agregar/Modificar
      c=0;
      for (cur=curs.FirstChild(); cur!=null; cur=cur.Next())
      {
        c++;
        FORM form=(FORM)wfNew.FORMS.GetByAttribute("c_form",cur.GetAttr("CODE"));
        if (form==null)
        { //No existe crear
          form=new FORM();
          form.c_form     =cur.GetAttr("CODE");
          form.d_form     =cur.GetAttr("DESC");
          form.d_template =cur.GetAttr("TEMPLATE");
          form.o_scripts_1=cur.GetAttr("SCRIPT_1");
          form.o_scripts_2=cur.GetAttr("SCRIPT_2");
          form.o_scripts_3=cur.GetAttr("SCRIPT_3");
          form.o_scripts_4=cur.GetAttr("SCRIPT_4");
          form.d_file_html=cur.GetAttr("FILE");
          form.c_file_mode=cur.GetAttr("MODE");
          wfNew.FORMS.Add(form);

          //Para la Validacion (Caso Publicada...)
          NomadLog.Info("Agregando el Formulario '"+form.c_form+"' a la Lista....");
        } else
        { //Existe modificar
          form.c_form     =cur.GetAttr("CODE");
          form.d_form     =cur.GetAttr("DESC");
          form.d_template =cur.GetAttr("TEMPLATE");
          form.o_scripts_1=cur.GetAttr("SCRIPT_1");
          form.o_scripts_2=cur.GetAttr("SCRIPT_2");
          form.o_scripts_3=cur.GetAttr("SCRIPT_3");
          form.o_scripts_4=cur.GetAttr("SCRIPT_4");
          form.d_file_html=cur.GetAttr("FILE");
          form.c_file_mode=cur.GetAttr("MODE");

          //Para la Validacion (Caso Publicada...)
          NomadLog.Info("Actualizo el Formulario '"+form.c_form+"' a la Lista....");
        }

        if (wfNew.l_automatica)
        { //Solo generar los Archivos

          //Nuevo nombre del Archivo
          form.d_file_html="WF"+wfNew.Id+"_F"+c+"_"+DateTime.Now.ToString("yyyyMMddHHmmss")+".htm";

          //Creo el ARCHIVO
          byte[] myBuffer;
          StreamWriter MyDATA=new StreamWriter(NomadProxy.GetProxy().RunPath+"\\Web\\GenFiles\\WFForms\\"+form.d_file_html);

          // Generar los Archivos
          for (cur2=cur.FirstChild(); cur2!=null; cur2=cur2.Next())
          {
            myBuffer=Convert.FromBase64String(cur2.GetAttr(""));
            MyDATA.BaseStream.Write(myBuffer,0,myBuffer.Length);
          }

          MyDATA.Close();

        } else
        { //Generar el Contenido

          //////////////////////////////////////////////////////////////////////////////////////////////////
          //Actualizacion de Grupos de Variables
          // Eliminar
          arrAUX.Clear();
          foreach(FORM_GROUP group in form.FORM_GROUPS)
          {
            if (cur.FindElement2("GROUP","CODE",group.c_form_group)==null)
            {
              NomadLog.Info("Elimino el Grupo '"+group.c_form_group+"' de la Lista....");
              arrAUX.Add(group);
            }
          }
          foreach(FORM_GROUP group in arrAUX)
          {
            c=group.FORM_FIELDS.Count;
            form.FORM_GROUPS.Remove(group);
          }

          // Agregar/Modificar
          for (cur2=cur.FirstChild(); cur2!=null; cur2=cur2.Next())
          {
            FORM_GROUP group=(FORM_GROUP)form.FORM_GROUPS.GetByAttribute("c_form_group",cur2.GetAttr("CODE"));

            if (group==null)
            { //No existe crear
              group=new FORM_GROUP();
              group.c_form_group=cur2.GetAttr("CODE");
              group.d_form_group=cur2.GetAttr("DESC");
              group.c_order     =cur2.GetAttr("ORDER");
              group.c_mode      =cur2.GetAttr("MODE");
              group.c_var_group =cur2.GetAttr("GROUP");
              group.d_param_1   =cur2.GetAttr("PARAM_1");
              group.d_param_2   =cur2.GetAttr("PARAM_2");
              group.d_param_3   =cur2.GetAttr("PARAM_3");
              group.o_text      =cur2.GetAttr("TEXT");
              group.o_onchange  =cur2.GetAttr("ON-CHANGE");

              form.FORM_GROUPS.Add(group);

              //Para la Validacion (Caso Publicada...)
              NomadLog.Info("Agregando el Grupo '"+group.c_form_group+"' a la Lista....");
            } else
            { //Existe modificar
              group.c_form_group=cur2.GetAttr("CODE");
              group.d_form_group=cur2.GetAttr("DESC");
              group.c_order     =cur2.GetAttr("ORDER");
              group.c_mode      =cur2.GetAttr("MODE");
              group.c_var_group =cur2.GetAttr("GROUP");
              group.d_param_1   =cur2.GetAttr("PARAM_1");
              group.d_param_2   =cur2.GetAttr("PARAM_2");
              group.d_param_3   =cur2.GetAttr("PARAM_3");
              group.o_text      =cur2.GetAttr("TEXT");
              group.o_onchange  =cur2.GetAttr("ON-CHANGE");

              //Para la Validacion (Caso Publicada...)
              NomadLog.Info("Actualizo el Grupo '"+group.c_form_group+"' a la Lista....");
            }

            //////////////////////////////////////////////////////////////////////////////////////////////////
            //Actualizacion de Campos
            // Eliminar
            arrAUX.Clear();
            foreach(FIELD field in group.FORM_FIELDS)
            {
              if (cur.FindElement2("FIELD","CODE",field.c_field)==null)
              {
                NomadLog.Info("Elimino el Campo '"+field.c_field+"' de la Lista....");
                arrAUX.Add(field);
              }
            }
            foreach(FIELD field in arrAUX)
              group.FORM_FIELDS.Remove(field);

            for (cur3=cur2.FirstChild(); cur3!=null; cur3=cur3.Next())
            {
              FIELD field=(FIELD)group.FORM_FIELDS.GetByAttribute("c_field",cur3.GetAttr("CODE"));

              if (field==null)
              { //No existe crear
                field=new FIELD();
                field.c_field       =cur3.GetAttr("CODE");
                field.d_field       =cur3.GetAttr("DESC");
                field.e_order       =cur3.GetAttrInt("ORDER");
                field.c_mode        =cur3.GetAttr("MODE");
                field.c_var         =cur3.GetAttr("VAR");
                field.c_parent_field=cur3.GetAttr("PARENT");
                field.c_valor       =cur3.GetAttr("VALUE");
                field.o_onvalidate  =cur3.GetAttr("ON-VALIDATE");
                field.o_onchange    =cur3.GetAttr("ON-CHANGE");
                field.d_param_1     =cur3.GetAttr("PARAM_1");
                field.d_param_2     =cur3.GetAttr("PARAM_2");
                field.d_param_3     =cur3.GetAttr("PARAM_3");

                group.FORM_FIELDS.Add(field);

                //Para la Validacion (Caso Publicada...)
                NomadLog.Info("Agregando el Campo '"+field.c_field+"' a la Lista....");
              } else
              { //Existe modificar
                field.c_field       =cur3.GetAttr("CODE");
                field.d_field       =cur3.GetAttr("DESC");
                field.e_order       =cur3.GetAttrInt("ORDER");
                field.c_mode        =cur3.GetAttr("MODE");
                field.c_var         =cur3.GetAttr("VAR");
                field.c_parent_field=cur3.GetAttr("PARENT");
                field.c_valor       =cur3.GetAttr("VALUE");
                field.o_onvalidate  =cur3.GetAttr("ON-VALIDATE");
                field.o_onchange    =cur3.GetAttr("ON-CHANGE");
                field.d_param_1     =cur3.GetAttr("PARAM_1");
                field.d_param_2     =cur3.GetAttr("PARAM_2");
                field.d_param_3     =cur3.GetAttr("PARAM_3");

                //Para la Validacion (Caso Publicada...)
                NomadLog.Info("Actualizo el Campo '"+field.c_field+"' a la Lista....");
              }
            }

          }
        }
      }

      //////////////////////////////////////////////////////////////////////////////////////////////////
      //Guardo el DDO
      MyBATCH.SetMess("Guardando los FORMULARIOS....");
      MyBATCH.Log("Guardando los FORMULARIOS....");
      MyBATCH.SetPro(50);
      NomadEnvironment.GetCurrentTransaction().Save(wfNew);
      wfNew=WF.Get(wfNew.Id,false);

      //////////////////////////////////////////////////////////////////////////////////////////////////
      //Actualizacion de PROCESOS
      MyBATCH.SetMess("Copiando PROCESOS....");
      MyBATCH.Log("Copiando PROCESOS....");
      MyBATCH.SetPro(60);

      curs=objITEM.FirstChild().FindElement("PROCESS");

      // Eliminar
      arrAUX.Clear();
      foreach(PROCESS proc in wfNew.PROCESS)
      {
        if (curs.FindElement2("PRO","CODE2",proc.c_process+"-"+proc.e_automatico)==null)
        {
          NomadLog.Info("Elimino el Proceso '"+proc.c_process+"-"+proc.e_automatico+"' de la Lista....");
          arrAUX.Add(proc);
        }
      }
      foreach(PROCESS proc in arrAUX)
      {
        c=proc.ROLES_PRO.Count;
        c=proc.PROCESS_PARAM.Count;
        foreach(NODE node in arrAUX)
        {
          c=node.ROLES_NODE.Count;
          c=node.NODE_PARAMS.Count;
        }
        wfNew.PROCESS.Remove(proc);
      }

      // Agregar/Modificar
      for (cur=curs.FirstChild(); cur!=null; cur=cur.Next())
      {
        PROCESS proc=null;
        foreach(PROCESS aux in wfNew.PROCESS)
          if ((aux.c_process==cur.GetAttr("CODE")) && (aux.e_automatico==cur.GetAttrInt("AUTO")))
            proc=aux;

        if (proc==null)
        { //No existe crear
          proc=new PROCESS();
          proc.c_process   =cur.GetAttr("CODE");
          proc.d_process   =cur.GetAttr("DESC");
          proc.e_automatico=cur.GetAttrInt("AUTO");

          wfNew.PROCESS.Add(proc);

          //Para la Validacion (Caso Publicada...)
          NomadLog.Info("Agregando el Proceso '"+proc.c_process+"-"+proc.e_automatico+"' a la Lista....");
        } else
        { //Existe modificar
          proc.c_process   =cur.GetAttr("CODE");
          proc.d_process   =cur.GetAttr("DESC");
          proc.e_automatico=cur.GetAttrInt("AUTO");

          //Para la Validacion (Caso Publicada...)
          NomadLog.Info("Actualizo el Proceso '"+proc.c_process+"-"+proc.e_automatico+"' a la Lista....");
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////
        //Actualizacion de ROLES
        curs2=cur.FindElement("ROLES");

        // Eliminar
        arrAUX.Clear();
        foreach(ROLE_PROCESS role in proc.ROLES_PRO)
        {
          if (curs2.FindElement2("ROLE","ROLE",role.c_rol)==null)
          {
            NomadLog.Info("Elimino el Rol '"+role.c_rol+"' de la Lista....");
            arrAUX.Add(role);
          }
        }
        foreach(ROLE_PROCESS role in arrAUX)
          proc.ROLES_PRO.Remove(role);

        // Agregar/Modificar
        for (cur2=curs2.FirstChild(); cur2!=null; cur2=cur2.Next())
        {
          ROLE_PROCESS role=(ROLE_PROCESS)proc.ROLES_PRO.GetByAttribute("c_rol",cur2.GetAttr("ROLE"));

          if (role==null)
          { //No existe crear
            role=new ROLE_PROCESS();
            role.c_rol          =cur2.GetAttr("ROLE");
            role.l_administrar  =false;
            role.l_iniciar      =cur2.GetAttrBool("INI");
            proc.ROLES_PRO.Add(role);

            //Para la Validacion (Caso Publicada...)
            NomadLog.Info("Agregando el Rol '"+role.c_rol+"' a la Lista....");
          } else
          { //Existe modificar
            role.c_rol          =cur2.GetAttr("ROLE");
            role.l_administrar  =false;
            role.l_iniciar      =cur2.GetAttrBool("INI");

            //Para la Validacion (Caso Publicada...)
            NomadLog.Info("Actualizo el Rol '"+role.c_rol+"' a la Lista....");
          }
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////
        //Actualizacion de PARAMS
        curs2=cur.FindElement("PARAMS");

        // Eliminar
        arrAUX.Clear();
        foreach(PROCESS_PARAM proc_param in proc.PROCESS_PARAM)
        {
          if (curs2.FindElement2("PARAM","CODE",proc_param.c_process_param)==null)
          {
            NomadLog.Info("Elimino el Parametro '"+proc_param.c_process_param+"' de la Lista....");
            arrAUX.Add(proc_param);
          }
        }
        foreach(PROCESS_PARAM proc_param in arrAUX)
          proc.PROCESS_PARAM.Remove(proc_param);

        // Agregar/Modificar
        for (cur2=curs2.FirstChild(); cur2!=null; cur2=cur2.Next())
        {
          PROCESS_PARAM pro_param=(PROCESS_PARAM)proc.PROCESS_PARAM.GetByAttribute("c_process_param",cur.GetAttr("CODE"));
          if (pro_param==null)
          { //No existe crear
            pro_param=new PROCESS_PARAM();
            pro_param.c_process_param=cur2.GetAttr("CODE");
            pro_param.d_process_param=cur2.GetAttr("DESC");
            pro_param.c_type         =cur2.GetAttr("TYPE");
            pro_param.c_io           =cur2.GetAttr("IO");
            proc.PROCESS_PARAM.Add(pro_param);

            //Para la Validacion (Caso Publicada...)
            NomadLog.Info("Agregando el Parametro '"+pro_param.c_process_param+"' a la Lista....");
          } else
          { //Existe modificar
            pro_param.c_process_param=cur2.GetAttr("CODE");
            pro_param.d_process_param=cur2.GetAttr("DESC");
            pro_param.c_type         =cur2.GetAttr("TYPE");
            pro_param.c_io           =cur2.GetAttr("IO");

            //Para la Validacion (Caso Publicada...)
            NomadLog.Info("Actualizo el Parametro '"+pro_param.c_process_param+"' a la Lista....");
          }
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////
        //Actualizacion de NODES
        curs2=cur.FindElement("NODES");

        // Eliminar
        arrAUX.Clear();
        foreach(NODE node in proc.NODES)
        {
          if (curs2.FindElement2("NODE","SEC",node.n_secuence.ToString())==null)
          {
            NomadLog.Info("Elimino el Nodo '"+node.n_secuence+"' de la Lista....");
            arrAUX.Add(node);
          }
        }
        foreach(NODE node in arrAUX)
        {
          c=node.ROLES_NODE.Count;
          c=node.NODE_PARAMS.Count;
          proc.NODES.Remove(node);
        }

        // Agregar/Modificar
        for (cur2=curs2.FirstChild(); cur2!=null; cur2=cur2.Next())
        {
          NODE node=(NODE)proc.NODES.GetByAttribute("n_secuence",cur2.GetAttrDouble("SEC"));

          if (node==null)
          { //No existe crear
            node=new NODE();
            node.c_type       =cur2.GetAttr("TYPE");
            node.n_secuence   =cur2.GetAttrDouble("SEC");
            node.d_label      =cur2.GetAttr("LABEL");
            node.d_param_1    =cur2.GetAttr("PARAM_1");
            node.d_param_2    =cur2.GetAttr("PARAM_2");
            node.d_param_3    =cur2.GetAttr("PARAM_3");
            node.d_param_4    =cur2.GetAttr("PARAM_4");
            node.o_formula    =cur2.GetAttr("FORMULA");
            node.o_process    =cur2.GetAttr("PROCESS");
            node.o_compformula=cur2.GetAttr("COMP-FORMULA");
            node.o_compprocess=cur2.GetAttr("COMP-PROCESS");
            node.n_horas      =cur2.GetAttrInt("HOR");
            node.n_horasNull  =(cur2.GetAttr("HOR")=="");
            proc.NODES.Add(node);

            //Para la Validacion (Caso Publicada...)
            NomadLog.Info("Agregando el Nodo '"+node.n_secuence+"' a la Lista....");
          } else
          { //Existe modificar
            node.c_type       =cur2.GetAttr("TYPE");
            node.n_secuence   =cur2.GetAttrDouble("SEC");
            node.d_label      =cur2.GetAttr("LABEL");
            node.d_param_1    =cur2.GetAttr("PARAM_1");
            node.d_param_2    =cur2.GetAttr("PARAM_2");
            node.d_param_3    =cur2.GetAttr("PARAM_3");
            node.d_param_4    =cur2.GetAttr("PARAM_4");
            node.o_formula    =cur2.GetAttr("FORMULA");
            node.o_process    =cur2.GetAttr("PROCESS");
            node.o_compformula=cur2.GetAttr("COMP-FORMULA");
            node.o_compprocess=cur2.GetAttr("COMP-PROCESS");
            node.n_horas      =cur2.GetAttrInt("HOR");
            node.n_horasNull  =(cur2.GetAttr("HOR")=="");

            //Para la Validacion (Caso Publicada...)
            NomadLog.Info("Actualizo el Nodo '"+node.n_secuence+"' a la Lista....");
          }

          //////////////////////////////////////////////////////////////////////////////////////////////////
          //Actualizacion de ROLES
          curs3=cur2.FindElement("ROLES");

          // Eliminar
          arrAUX.Clear();
          foreach(ROLE_NODE role in node.ROLES_NODE)
          {
            if (curs3.FindElement2("ROLE","ROLE",role.c_rol)==null)
            {
              NomadLog.Info("Elimino el Rol '"+role.c_rol+"' de la Lista....");
              arrAUX.Add(role);
            }
          }
          foreach(ROLE_NODE role in arrAUX)
            node.ROLES_NODE.Remove(role);

          // Agregar/Modificar
          for (cur3=curs3.FirstChild(); cur3!=null; cur3=cur3.Next())
          {
            ROLE_NODE role=(ROLE_NODE)node.ROLES_NODE.GetByAttribute("c_rol",cur3.GetAttr("ROLE"));

            if (role==null)
            { //No existe crear
              role=new ROLE_NODE();
              role.c_rol          =cur3.GetAttr("ROLE");
              role.c_notificar    =cur3.GetAttr("NOTIFY");
              role.c_resolver     =cur3.GetAttr("RESOLVE");
              role.c_puede_deleg  =cur3.GetAttr("DELEGA");
              role.d_rol_delega   =cur3.GetAttr("TO-DELEGA");
              node.ROLES_NODE.Add(role);

              //Para la Validacion (Caso Publicada...)
              NomadLog.Info("Agregando el Rol '"+role.c_rol+"' a la Lista....");
            } else
            { //Existe modificar
              role.c_rol          =cur3.GetAttr("ROLE");
              role.c_notificar    =cur3.GetAttr("NOTIFY");
              role.c_resolver     =cur3.GetAttr("RESOLVE");
        role.c_puede_deleg  =cur3.GetAttr("DELEGA");
        role.d_rol_delega   =cur3.GetAttr("TO-DELEGA");

              //Para la Validacion (Caso Publicada...)
              NomadLog.Info("Actualizo el Rol '"+role.c_rol+"' a la Lista....");
            }
          }

          //////////////////////////////////////////////////////////////////////////////////////////////////
          //Actualizacion de PARAMS
          curs3=cur2.FindElement("PARAMS");

          // Eliminar
          arrAUX.Clear(); c=0;
          foreach(PARAM param in node.NODE_PARAMS)
          {
            c++;
             if (c>curs3.ChildLength)
              arrAUX.Add(param);
          }
          foreach(PARAM param in arrAUX)
            node.NODE_PARAMS.Remove(param);

          // Agregar/Modificar
          c=0;
          for (cur3=curs3.FirstChild(); cur3!=null; cur3=cur3.Next())
          {
            PARAM param;

            if (c>=node.NODE_PARAMS.Count)
            { //No existe crear
              param=new PARAM();
              param.c_param     =cur3.GetAttr("CODE");
              param.d_param     =cur3.GetAttr("DESC");
              param.c_type      =cur3.GetAttr("TYPE");
              param.e_orden     =cur3.GetAttrInt("ORDER");
              param.e_subproceso=cur3.GetAttrInt("SUBPROCESS");
              param.c_var       =cur3.GetAttr("VAR");
              param.o_formula   =cur3.GetAttr("FORMULA");
              param.o_compilada =cur3.GetAttr("COMPILE");
              param.d_valor     =cur3.GetAttr("VALUE");

              node.NODE_PARAMS.Add(param);

              //Para la Validacion (Caso Publicada...)
              NomadLog.Info("Agregando el Param("+c+") '"+param.e_orden+"' a la Lista....");
            } else
            { //Existe modificar
              param=(PARAM)node.NODE_PARAMS[c];
              param.c_param     =cur3.GetAttr("CODE");
              param.d_param     =cur3.GetAttr("DESC");
              param.c_type      =cur3.GetAttr("TYPE");
              param.e_orden     =cur3.GetAttrInt("ORDER");
              param.e_subproceso=cur3.GetAttrInt("SUBPROCESS");
              param.c_var       =cur3.GetAttr("VAR");
              param.o_formula   =cur3.GetAttr("FORMULA");
              param.o_compilada =cur3.GetAttr("COMPILE");
              param.d_valor     =cur3.GetAttr("VALUE");

              //Para la Validacion (Caso Publicada...)
              NomadLog.Info("Actualizo el Param("+c+") '"+param.e_orden+"' a la Lista....");
            }
            c++;
          }
        }

      }

      //////////////////////////////////////////////////////////////////////////////////////////////////
      //Guardo el DDO
      MyBATCH.SetMess("Guardando los PROCESOS....");
      MyBATCH.Log("Guardando los PROCESOS....");
      MyBATCH.SetPro(70);
      NomadEnvironment.GetCurrentTransaction().Save(wfNew);
      wfNew=WF.Get(wfNew.Id,false);

    }

    public static void EliminarDebug(int oi_wf)
    {
        NomadBatch MyBATCH = NomadBatch.GetBatch("Limpiar", "Limpiar");

        //Cargo el WF
        MyBATCH.SetMess("Obtengo la lista de Instancias....");
        MyBATCH.SetPro(10);
        MyBATCH.Log("Obtengo la lista de Instancias....");

        //Obtengo la Lista de INSTANCIAS
        NomadXML xmlIns=NomadProxy.GetProxy().SQLService().GetXML(Resources.QRY_INSTANCIAS_DEBUG,"<DATA oi_wf=\""+oi_wf+"\" />");
        if (xmlIns.isDocument) xmlIns=xmlIns.FirstChild();

        ////////////////////////////////////////////////////////////////////////////////
        //Elimino las Instancias
        MyBATCH.SetMess("Eliminando las Instancias DEBUG....");
        MyBATCH.Log("Eliminando las Instancias DEBUG....");
        MyBATCH.SetPro(20);

        //Recorro las Instancias y las Elimino
        int st=0;
        for (NomadXML cur=xmlIns.FirstChild(); cur!=null; cur=cur.Next())
        {
          st++;
          MyBATCH.SetMess("Eliminando las Instancias DEBUG (" + st + "/" + xmlIns.ChildLength + ")");
          MyBATCH.SetPro(20, 90, xmlIns.ChildLength, st);

          try
          {
              NucleusWF.Base.Ejecucion.Instancias.INSTANCE.Eliminar(cur.GetAttrInt("oi_instance"), NomadProxy.GetProxy().UserEtty);
          }
          catch (Exception E)
          {
              MyBATCH.Wrn("No se puede Eliminar la Solicitud Nro " + cur.GetAttr("oi_instance") + " porque " + E.Message);
          }
        }

        //////////////////////////////////////////////////////////////////////////
        //Final
        MyBATCH.SetMess("Limpieza Finalizada....");
        MyBATCH.SetPro(90);
        MyBATCH.Log("Limpieza Finalizada....");
    }

    public static void Publicar(int oi_wf)
    {
        NomadBatch MyBATCH = NomadBatch.GetBatch("Publicar", "Publicar");
        WF wfP = null;
        string oi_wf_new = null;

        try
        {

            //Verifico el ORGANIGRAMA
            MyBATCH.SetMess("Verifico el Organigrama....");
            MyBATCH.Log("Verifico el Organigrama....");
            MyBATCH.SetPro(10);
            WF wfO = WF.Get(oi_wf, false);
            if (!NucleusWF.Base.Definicion.Organigramas.ORGANIGRAMA.Verificar(wfO.oi_organigrama))
                throw new NomadException("El Organigrama Publicado es Distinto al Principal.");

            //Cargando los Roles
            Hashtable Roles = new Hashtable();
            NucleusWF.Base.Definicion.Organigramas.ORGANIGRAMA orgO = NucleusWF.Base.Definicion.Organigramas.ORGANIGRAMA.Get(wfO.oi_organigrama, false);
            foreach (NucleusWF.Base.Definicion.Organigramas.ROLE rol in orgO.ROLES) Roles[rol.c_role] = rol;

            //Cargo el WF
            MyBATCH.SetMess("Copiando Workflow....");
            MyBATCH.SetPro(20);
            MyBATCH.Log("Copiando Workflow....");
            wfP = new WF();
            wfP.c_wf = wfO.c_wf;
            wfP.d_wf = wfO.d_wf;
            wfP.e_version = wfO.e_version;
            wfP.e_version_pub = wfO.e_version;
            wfP.l_automatica = true;
            wfP.c_text = wfO.c_text;
            wfP.n_horas = wfO.n_horas;
            wfP.oi_organigrama = wfO.oi_organigrama;
            wfP.c_estado = "F";

            wfO.e_version = wfO.e_version + 1;

            NomadEnvironment.GetCurrentTransaction().SaveRefresh(wfO);
            NomadEnvironment.GetCurrentTransaction().SaveRefresh(wfP);
            oi_wf_new = wfP.Id;

            //////////////////////////////////////////////////////////////////////////
            //Agrego los Estados
            Hashtable Estados = new Hashtable();
            MyBATCH.SetMess("Copiando ESTADOS....");
            MyBATCH.SetPro(30);
            MyBATCH.Log("Copiando ESTADOS....");

            wfP = WF.Get(oi_wf_new, false);
            foreach (ESTADO estO in wfO.ESTADOS)
            {
                NomadLog.Info("Agregando el Estado '" + estO.c_estado + "' ....");

                //Validando
                if (Estados.ContainsKey(estO.c_estado.ToUpper()))
                    throw new NomadException("El estado '" + estO.c_estado + "' esta Duplicado....");

                //Agregando
                Estados[estO.c_estado.ToUpper()] = estO;
                wfP.ESTADOS.Add(estO.Duplicate());
            }
            NomadEnvironment.GetCurrentTransaction().Save(wfP);

            //////////////////////////////////////////////////////////////////////////
            //Agrego las Variables
            Hashtable Variables = new Hashtable();
            MyBATCH.SetMess("Copiando VARIABLES....");
            MyBATCH.SetPro(40);
            MyBATCH.Log("Copiando VARIABLES....");

            wfP = WF.Get(oi_wf_new, false);
            VAR_GROUP grpP;
            foreach (VAR_GROUP grpO in wfO.GROUPS)
            {
                NomadLog.Info("Agregando el Grupo de Variables '" + grpO.c_var_group + "' ....");

                //Agregando
                grpP = VAR_GROUP.CopyFrom(grpO, Variables);
                if (grpP == null) return;

                wfP.GROUPS.Add(grpP);
            }
            NomadEnvironment.GetCurrentTransaction().Save(wfP);

            //////////////////////////////////////////////////////////////////////////
            //Agrego los Textos
            Hashtable Texts = new Hashtable();
            MyBATCH.SetMess("Copiando REPORTES....");
            MyBATCH.SetPro(50);
            MyBATCH.Log("Copiando REPORTES....");

            wfP = WF.Get(oi_wf_new, false);
            TEXT rptP;
            foreach (TEXT rptO in wfO.NODE_TEXTS)
            {
                NomadLog.Info("Agregando el REPORTE '" + rptO.c_text + "' ....");

                //Agregando
                rptP = TEXT.CopyFrom(rptO, wfO, Texts);
                if (rptP == null) return;

                wfP.NODE_TEXTS.Add(rptP);
            }
            NomadEnvironment.GetCurrentTransaction().Save(wfP);

            //////////////////////////////////////////////////////////////////////////
            //Agrego los Formularios
            Hashtable htaForms = new Hashtable();
            Hashtable htaGroupVars = new Hashtable();
            MyBATCH.SetMess("Copiando FORMULARIOS....");
            MyBATCH.SetPro(60);
            MyBATCH.Log("Copiando FORMULARIOS....");
            MyBATCH.SetSubBatch(60, 61);

            wfP = WF.Get(oi_wf_new, false);
            FORM objFormP;
            foreach (FORM objFormO in wfO.FORMS)
            {
                NomadLog.Info("Agregando el FORMULARIO '" + objFormO.c_form + "'...");

                //Agregando
                objFormP = FORM.CopyFrom(objFormO, htaForms, htaGroupVars);
                if (objFormP == null) return;

                wfP.FORMS.Add(objFormP);
            }
            NomadEnvironment.GetCurrentTransaction().Save(wfP);

            //////////////////////////////////////////////////////////////////////////
            //Agrego los Procesos
            MyBATCH.SetMess("Copiando PROCESOS....");
            MyBATCH.SetPro(70);
            MyBATCH.Log("Copiando PROCESOS....");

            Hashtable Labels = new Hashtable();
            Hashtable Procesos = new Hashtable();

            wfP = WF.Get(oi_wf_new, false);
            PROCESS proP;
            foreach (PROCESS proO in wfO.PROCESS)
            {
                NomadLog.Info("Agregando el PROCESO '" + proO.c_process + "-" + proO.e_automatico + "' ....");

                //Agregando
                proP = NucleusWF.Base.Definicion.Workflows.PROCESS.CopyFrom(proO, wfO, Estados, Roles, Texts, Variables, Labels, Procesos);
                if (proP == null) return;

                wfP.PROCESS.Add(proP);
            }
            NomadEnvironment.GetCurrentTransaction().Save(wfP);

            //////////////////////////////////////////////////////////////////////////
            //Publicacion Final
            MyBATCH.SetMess("Publicando....");
            MyBATCH.SetPro(80);
            MyBATCH.Log("Publicando....");

            wfP = WF.Get(oi_wf_new, false);
            wfP.c_estado = "R";
            NomadEnvironment.GetCurrentTransaction().Save(wfP);

            wfO = WF.Get(oi_wf, false);
            wfO.e_version_pub = wfP.e_version;
            wfO.c_estado = "R";
            NomadEnvironment.GetCurrentTransaction().Save(wfO);

        }
        catch (Exception E1)
        {
            //////////////////////////////////////////////////////////////////////////
            //ERROR
            MyBATCH.Err("No se pudo Publicar el Workflow. " + E1.Message);
            NomadLog.Error("No se pudo Publicar el Workflow. " + E1.Message, E1);

            try
            {
                if (oi_wf_new != null)
                {
                    //////////////////////////////////////////////////////////////////////////
                    //ELIMINAR PUBLICACION
                    MyBATCH.SetMess("Eliminando Publicacion....");
                    MyBATCH.SetPro(80);
                    MyBATCH.Log("Eliminando Publicacion....");
                    wfP = WF.Get(oi_wf_new, true);
                    NomadEnvironment.GetCurrentTransaction().Delete(wfP);
                }
            }
            catch (Exception E2)
            {
                MyBATCH.Err("No se pudo Eliminar la Publicacion del Workflow. " + E2.Message);
                NomadLog.Error("No se pudo Eliminar la Publicacion del Workflow " + E2.Message, E2);
            }
        }

        //////////////////////////////////////////////////////////////////////////
        //Final
        MyBATCH.SetMess("Publicacion Finalizada....");
        MyBATCH.SetPro(90);
        MyBATCH.Log("Publicacion Finalizada....");
    }

    public static string CompileExpresion(string txtData)
    {

        int LINE = 0;
        char[] chDATA = txtData.ToCharArray();
        System.Text.StringBuilder outDATA = new System.Text.StringBuilder();

        Nomad.Base.Report.Compile.Expresion('\0', chDATA, 0, chDATA.Length, outDATA, ref LINE);
        return outDATA.ToString();
    }

    public static string CompileMethod(string txtData)
    {

        int LINE = 0;
        char[] chDATA = txtData.ToCharArray();
        System.Text.StringBuilder outDATA = new System.Text.StringBuilder();

        Nomad.Base.Report.Compile.Procedure(false, '\0', chDATA, 0, chDATA.Length, outDATA, ref LINE);
        return outDATA.ToString();
    }

    public static void Eliminar(int oi_wf)
    {
        NomadBatch MyBATCH = NomadBatch.GetBatch("Eliminar", "Eliminar");

        try
        {
            bool cont;

            ////////////////////////////////////////////////////////////////////////////////
            //Obtengo el WF
            MyBATCH.SetMess("Obtengo el WORKFLOW....");
            MyBATCH.Log("Obtengo el WORKFLOW....");
            WF wf = WF.Get(oi_wf, false);
            if (!wf.l_automatica)
            { //No es un Workflow Publicado.
                throw new NomadException("FUNCION no implementada");
            }
            wf.c_estado = "F";
            NomadEnvironment.GetCurrentTransaction().Save(wf);
            MyBATCH.SetPro(10);

            ////////////////////////////////////////////////////////////////////////////////
            //Obtengo las Instancias
            NomadXML RS, INSTANCES, cur;

            MyBATCH.SetMess("Obtengo la lista de Instancias ASOCIADAS....");
            MyBATCH.Log("Obtengo la lista de Instancias ASOCIADAS....");
            INSTANCES = new NomadXML("ROWS");
            int st = 0;
            do
            {
                RS = NomadEnvironment.QueryNomadXML(Resources.QRY_INSTANCIAS, "<DATA start=\"" + st + "\" oi_wf=\"" + oi_wf + "\"/>").FirstChild();

                //Recorro las instancias
                INSTANCES.SetAttr("oi_wf", RS.GetAttr("oi_wf"));
                for (cur = RS.FirstChild(); cur != null; cur = cur.Next())
                    INSTANCES.AddXML(cur);

                st += 1000;
            } while (RS.ChildLength == 1000);
            MyBATCH.SetPro(30);

            ////////////////////////////////////////////////////////////////////////////////
            //Elimino las Instancias
            MyBATCH.SetMess("Eliminando las Instancias ASOCIADAS....");
            MyBATCH.Log("Eliminando las Instancias ASOCIADAS....");

            //Recorro las instancias
            st = 0; cont = true;
            for (cur = INSTANCES.FirstChild(); cur != null; cur = cur.Next(), st++)
            {
                MyBATCH.SetMess("Eliminando las Instancias ASOCIADAS (" + st + "/" + INSTANCES.ChildLength + ")");
                MyBATCH.SetPro(30, 90, INSTANCES.ChildLength, st);

                try
                {
                    NucleusWF.Base.Ejecucion.Instancias.INSTANCE.Eliminar(cur.GetAttrInt("oi_instance"), NomadProxy.GetProxy().UserEtty);
                }
                catch (Exception E)
                {
                    MyBATCH.Wrn("No se puede Eliminar la Solicitud Nro " + cur.GetAttr("oi_instance") + " porque " + E.Message);
                    cont = false;
                }
            }
            if (!cont)
                throw new NomadException("Existen Instancias que no se pudieron Eliminar.");

            ////////////////////////////////////////////////////////////////////////////////
            //Elimino el Proceso ACTUAL
            MyBATCH.SetMess("Eliminando el WORKFLOW....");
            MyBATCH.Log("Eliminando el WORKFLOW....");

            //Cambio el Estado del Workflow de Diseño
            if (INSTANCES.GetAttr("oi_wf") != "" && INSTANCES.GetAttrInt("oi_wf") != oi_wf)
            {
                wf = WF.Get(INSTANCES.GetAttr("oi_wf"), false);
                wf.c_estado = "D";
                wf.e_version_pub = 0;
                NomadEnvironment.GetCurrentTransaction().Save(wf);
            }

            //Elimino el Workflow actual
            wf = WF.Get(oi_wf, true);
            NomadEnvironment.GetCurrentTransaction().Delete(wf);
            MyBATCH.SetPro(95);

        }
        catch (Exception E1)
        {
            //////////////////////////////////////////////////////////////////////////
            //ERROR
            MyBATCH.Err("No se pudo Eliminar el Workflow. " + E1.Message);
            NomadLog.Error("No se pudo Eliminar el Workflow. " + E1.Message, E1);
        }

        MyBATCH.Log("Proceso Finalizado....");
        MyBATCH.SetPro(100);
    }

    public static void Finalizar(int oi_wf)
    {
        NomadBatch MyBATCH = NomadBatch.GetBatch("Finalizar", "Finalizar");

        try
        {
            bool cont;
            NomadXML RS, INSTANCES, cur;

            //Esta Publicada?
            RS = NomadEnvironment.QueryNomadXML(Resources.QRY_INSTANCIAS, "<DATA start=\"0\" oi_wf=\"" + oi_wf + "\"/>").FirstChild();
            if (RS.GetAttr("oi_wf") != "" && RS.GetAttrInt("oi_wf") != oi_wf)
                throw new NomadException("No se puede Finalizar un Workflow Publicado.");

            ////////////////////////////////////////////////////////////////////////////////
            //Obtengo el WF
            MyBATCH.SetMess("Obtengo el WORKFLOW....");
            MyBATCH.Log("Obtengo el WORKFLOW....");
            WF wf = WF.Get(oi_wf, false);
            if (!wf.l_automatica)
                throw new NomadException("No se puede Finalizar este Workflow.");
            wf.c_estado = "F";
            NomadEnvironment.GetCurrentTransaction().Save(wf);
            MyBATCH.SetPro(10);

            ////////////////////////////////////////////////////////////////////////////////
            //Obtengo las Instancias
            MyBATCH.SetMess("Obtengo la lista de Instancias ASOCIADAS....");
            MyBATCH.Log("Obtengo la lista de Instancias ASOCIADAS....");
            INSTANCES = new NomadXML("ROWS");
            int st = 0;
            do
            {
                RS = NomadEnvironment.QueryNomadXML(Resources.QRY_INSTANCIAS, "<DATA start=\"" + st + "\" oi_wf=\"" + oi_wf + "\"/>").FirstChild();

                //Recorro las instancias
                INSTANCES.SetAttr("oi_wf", RS.GetAttr("oi_wf"));
                for (cur = RS.FirstChild(); cur != null; cur = cur.Next())
                    INSTANCES.AddXML(cur);

                st += 1000;
            } while (RS.ChildLength == 1000);
            MyBATCH.SetPro(30);

            ////////////////////////////////////////////////////////////////////////////////
            //Finalizo las Instancias
            MyBATCH.SetMess("Finalizar las Instancias ASOCIADAS....");
            MyBATCH.Log("Finalizar las Instancias ASOCIADAS....");

            //Recorro las instancias
            st = 0; cont = true;
            for (cur = INSTANCES.FirstChild(); cur != null; cur = cur.Next(), st++)
            {
                MyBATCH.SetMess("Finalizar las Instancias ASOCIADAS (" + st + "/" + INSTANCES.ChildLength + ")");
                MyBATCH.SetPro(30, 90, INSTANCES.ChildLength, st);

                try
                {
                    if (cur.GetAttr("c_estado_ej") == "F") continue;
                    if (cur.GetAttr("c_estado_ej") == "S") continue;

                    NucleusWF.Base.Ejecucion.Instancias.INSTANCE.Detener(cur.GetAttrInt("oi_instance"), NomadProxy.GetProxy().UserEtty);
                }
                catch (Exception E)
                {
                    MyBATCH.Wrn("No se puede Finalizar la Solicitud Nro " + cur.GetAttr("oi_instance") + " porque " + E.Message);
                    cont = false;
                }
            }
            if (!cont)
                throw new NomadException("Existen Instancias que no se pudieron Finalizar.");

        }
        catch (Exception E1)
        {
            //////////////////////////////////////////////////////////////////////////
            //ERROR
            MyBATCH.Err("No se pudo Finalizar el Workflow. " + E1.Message);
            NomadLog.Error("No se pudo Finalizar el Workflow. " + E1.Message, E1);
        }

        MyBATCH.Log("Proceso Finalizado....");
        MyBATCH.SetPro(100);
    }

    public static void Detener(int oi_wf)
    {
        NomadBatch MyBATCH = NomadBatch.GetBatch("Detener", "Detener");

        try
        {
            int st;
            bool cont;
            NomadXML RS, INSTANCES, cur;

            ////////////////////////////////////////////////////////////////////////////////
            //Obtengo el WF
            MyBATCH.SetMess("Obtengo el WORKFLOW....");
            MyBATCH.Log("Obtengo el WORKFLOW....");
            WF wf = WF.Get(oi_wf, false);
            if (!wf.l_automatica)
            {
                RS = NomadEnvironment.QueryNomadXML(Resources.QRY_PUBLIC, "<DATA oi_wf=\"" + oi_wf + "\"/>").FirstChild();

                st = 0;
                for (cur = RS.FirstChild(); cur != null; cur = cur.Next(), st++)
                {
                    MyBATCH.SetSubBatch(10, 10 + st * 80 / RS.ChildLength);
                    if (cur.GetAttr("c_estado") != "F" && cur.GetAttr("l_automatica") == "1")
                        Detener(cur.GetAttrInt("oi_wf"));
                }

                wf.c_estado = "S";
                NomadEnvironment.GetCurrentTransaction().Save(wf);
            }
            else
            {
                if (wf.c_estado == "F")
                    throw new NomadException("No se puede Detener un Workflow Finalizado.");
                wf.c_estado = "S";
                NomadEnvironment.GetCurrentTransaction().Save(wf);
                MyBATCH.SetPro(10);

                ////////////////////////////////////////////////////////////////////////////////
                //Obtengo las Instancias
                MyBATCH.SetMess("Obtengo la lista de Instancias ASOCIADAS....");
                MyBATCH.Log("Obtengo la lista de Instancias ASOCIADAS....");
                INSTANCES = new NomadXML("ROWS");
                st = 0;
                do
                {
                    RS = NomadEnvironment.QueryNomadXML(Resources.QRY_INSTANCIAS, "<DATA start=\"" + st + "\" oi_wf=\"" + oi_wf + "\"/>").FirstChild();

                    //Recorro las instancias
                    INSTANCES.SetAttr("oi_wf", RS.GetAttr("oi_wf"));
                    for (cur = RS.FirstChild(); cur != null; cur = cur.Next())
                        INSTANCES.AddXML(cur);

                    st += 1000;
                } while (RS.ChildLength == 1000);
                MyBATCH.SetPro(30);

                ////////////////////////////////////////////////////////////////////////////////
                //Finalizo las Instancias
                MyBATCH.SetMess("Pausar las Instancias ASOCIADAS....");
                MyBATCH.Log("Pausar las Instancias ASOCIADAS....");

                //Recorro las instancias
                st = 0; cont = true;
                for (cur = INSTANCES.FirstChild(); cur != null; cur = cur.Next(), st++)
                {
                    MyBATCH.SetMess("Pausar las Instancias ASOCIADAS (" + st + "/" + INSTANCES.ChildLength + ")");
                    MyBATCH.SetPro(30, 90, INSTANCES.ChildLength, st);

                    try
                    {
                        if (cur.GetAttr("c_estado_ej") != "R") continue;

                        NucleusWF.Base.Ejecucion.Instancias.INSTANCE.Pausar(cur.GetAttrInt("oi_instance"), NomadProxy.GetProxy().UserEtty);
                    }
                    catch (Exception E)
                    {
                        MyBATCH.Wrn("No se puede Pausar la Solicitud Nro " + cur.GetAttr("oi_instance") + " porque " + E.Message);
                        cont = false;
                    }
                }
                if (!cont)
                    throw new NomadException("Existen Instancias que no se pudieron Pausar.");
            }

        }
        catch (Exception E1)
        {
            //////////////////////////////////////////////////////////////////////////
            //ERROR
            MyBATCH.Err("No se pudo Detener el Workflow. " + E1.Message);
            NomadLog.Error("No se pudo Detener el Workflow. " + E1.Message, E1);
        }

        MyBATCH.Log("Proceso Finalizado....");
        MyBATCH.SetPro(100);
    }

    public static void Activar(int oi_wf)
    {
        NomadBatch MyBATCH = NomadBatch.GetBatch("Activar", "Activar");

        try
        {
            bool cont;
            NomadXML RS, INSTANCES, cur;
            int st;

            ////////////////////////////////////////////////////////////////////////////////
            //Obtengo el WF
            MyBATCH.SetMess("Obtengo el WORKFLOW....");
            MyBATCH.Log("Obtengo el WORKFLOW....");
            WF wf = WF.Get(oi_wf, false);
            if (!wf.l_automatica)
            {
                RS = NomadEnvironment.QueryNomadXML(Resources.QRY_PUBLIC, "<DATA oi_wf=\"" + oi_wf + "\"/>").FirstChild();

                st = 0;
                for (cur = RS.FirstChild(); cur != null; cur = cur.Next(), st++)
                {
                    MyBATCH.SetSubBatch(10, 10 + st * 80 / RS.ChildLength);
                    if (cur.GetAttr("c_estado") != "F" && cur.GetAttr("l_automatica") == "1")
                        Activar(cur.GetAttrInt("oi_wf"));
                }

                wf.c_estado = "R";
                NomadEnvironment.GetCurrentTransaction().Save(wf);
            }
            else
            {
                if (wf.c_estado == "F")
                    throw new NomadException("No se puede Activar un Workflow Finalizado.");
                wf.c_estado = "R";
                NomadEnvironment.GetCurrentTransaction().Save(wf);
                MyBATCH.SetPro(10);

                ////////////////////////////////////////////////////////////////////////////////
                //Obtengo las Instancias
                MyBATCH.SetMess("Obtengo la lista de Instancias ASOCIADAS....");
                MyBATCH.Log("Obtengo la lista de Instancias ASOCIADAS....");
                INSTANCES = new NomadXML("ROWS");
                st = 0;
                do
                {
                    RS = NomadEnvironment.QueryNomadXML(Resources.QRY_INSTANCIAS, "<DATA start=\"" + st + "\" oi_wf=\"" + oi_wf + "\"/>").FirstChild();

                    //Recorro las instancias
                    INSTANCES.SetAttr("oi_wf", RS.GetAttr("oi_wf"));
                    for (cur = RS.FirstChild(); cur != null; cur = cur.Next())
                        INSTANCES.AddXML(cur);

                    st += 1000;
                } while (RS.ChildLength == 1000);
                MyBATCH.SetPro(30);

                ////////////////////////////////////////////////////////////////////////////////
                //Finalizo las Instancias
                MyBATCH.SetMess("Activar las Instancias ASOCIADAS....");
                MyBATCH.Log("Activar las Instancias ASOCIADAS....");

                //Recorro las instancias
                st = 0; cont = true;
                for (cur = INSTANCES.FirstChild(); cur != null; cur = cur.Next(), st++)
                {
                    MyBATCH.SetMess("Activar las Instancias ASOCIADAS (" + st + "/" + INSTANCES.ChildLength + ")");
                    MyBATCH.SetPro(30, 90, INSTANCES.ChildLength, st);

                    try
                    {
                        if (cur.GetAttr("c_estado_ej") != "P") continue;

                        NucleusWF.Base.Ejecucion.Instancias.INSTANCE.Continuar(cur.GetAttrInt("oi_instance"), NomadProxy.GetProxy().UserEtty);
                    }
                    catch (Exception E)
                    {
                        MyBATCH.Wrn("No se puede Activar la Solicitud Nro " + cur.GetAttr("oi_instance") + " porque " + E.Message);
                        cont = false;
                    }
                }
                if (!cont)
                    throw new NomadException("Existen Instancias que no se pudieron Activar.");
            }

        }
        catch (Exception E1)
        {
            //////////////////////////////////////////////////////////////////////////
            //ERROR
            MyBATCH.Err("No se pudo Activar el Workflow. " + E1.Message);
            NomadLog.Error("No se pudo Activar el Workflow. " + E1.Message, E1);
        }

        MyBATCH.Log("Proceso Finalizado....");
        MyBATCH.SetPro(100);
    }

    public static void Republicar(int oi_wf)
    {
        NomadBatch MyBATCH = NomadBatch.GetBatch("Finalizar", "Finalizar");

        try
        {
            NomadXML RS;

            //Esta Publicada?
            RS = NomadEnvironment.QueryNomadXML(Resources.QRY_INSTANCIAS, "<DATA start=\"0\" oi_wf=\"" + oi_wf + "\"/>").FirstChild();
            if (RS.GetAttr("oi_wf") != "" && RS.GetAttrInt("oi_wf") != oi_wf)
                throw new NomadException("No se puede Republicar un Workflow Publicado.");

            ////////////////////////////////////////////////////////////////////////////////
            //Obtengo el WF
            MyBATCH.SetMess("Obtengo el WORKFLOW....");
            MyBATCH.Log("Obtengo el WORKFLOW....");
            WF wf = WF.Get(oi_wf, false);
            if (!wf.l_automatica)
                throw new NomadException("No se puede Republicar este Workflow.");
            if (wf.c_estado != "R")
                throw new NomadException("No se puede Republicar un Workflow no Habilitado.");

            ////////////////////////////////////////////////////////////////////////////////
            //Obtengo el WF DISEÑO
            WF wfp = WF.Get(RS.GetAttrInt("oi_wf_dis"), false);
            wfp.e_version_pub = wf.e_version;
            wfp.c_estado = "R";
            NomadEnvironment.GetCurrentTransaction().Save(wfp);

        }
        catch (Exception E1)
        {
            //////////////////////////////////////////////////////////////////////////
            //ERROR
            MyBATCH.Err("No se pudo Republicar el Workflow. " + E1.Message);
            NomadLog.Error("No se pudo Republicar el Workflow. " + E1.Message, E1);
        }

        MyBATCH.Log("Proceso Finalizado....");
        MyBATCH.SetPro(100);
    }

    public static void Pausar(int oi_wf)
    {
        NomadBatch MyBATCH = NomadBatch.GetBatch("Pausar", "Pausar");

        try
        {
            NomadXML RS, cur;

            ////////////////////////////////////////////////////////////////////////////////
            //Obtengo el WF
            MyBATCH.SetMess("Obtengo el WORKFLOW....");
            MyBATCH.Log("Obtengo el WORKFLOW....");
            WF wf = WF.Get(oi_wf, false);
            if (wf.l_automatica)
                throw new NomadException("No se puede Pausar este Workflow.");

            ////////////////////////////////////////////////////////////////////////////////
            //Actualizo el WF Publicado
            RS = NomadEnvironment.QueryNomadXML(Resources.QRY_PUBLIC, "<DATA oi_wf=\"" + oi_wf + "\"/>").FirstChild();

            for (cur = RS.FirstChild(); cur != null; cur = cur.Next())
            {
                if (cur.GetAttrInt("e_version") == wf.e_version_pub && cur.GetAttr("l_automatica") == "1")
                {
                    if (cur.GetAttr("c_estado") != "R")
                        throw new NomadException("No se puede Pausar este Workflow, la version publicada no esta Habilitada.");

                    WF wfp = WF.Get(cur.GetAttrInt("oi_wf"), false);
                    wfp.c_estado = "P";
                    NomadEnvironment.GetCurrentTransaction().Save(wfp);
                }
            }

            ////////////////////////////////////////////////////////////////////////////////
            //Actualizo el ESTADO del Workflow
            wf.c_estado = "P";
            NomadEnvironment.GetCurrentTransaction().Save(wf);

        }
        catch (Exception E1)
        {
            //////////////////////////////////////////////////////////////////////////
            //ERROR
            MyBATCH.Err("No se pudo Republicar el Workflow. " + E1.Message);
            NomadLog.Error("No se pudo Republicar el Workflow. " + E1.Message, E1);
        }

        MyBATCH.Log("Proceso Finalizado....");
        MyBATCH.SetPro(100);
    }

   }
}



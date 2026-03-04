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
    public partial class PROCESS : Nomad.NSystem.Base.NomadObject
    {

    public static void DEBUG_DDO(NucleusWF.Base.Definicion.Workflows.PROCESS DOC_DDO)
    {

      ArrayList deleteItems=new ArrayList();

      //Cargo el WF
      WF wfO = WF.Get(DOC_DDO.oi_wf, false);

      //Cargo los ROLES
      Hashtable Roles=new Hashtable();
      NucleusWF.Base.Definicion.Organigramas.ORGANIGRAMA orgO = NucleusWF.Base.Definicion.Organigramas.ORGANIGRAMA.Get(wfO.oi_organigrama, false);
      foreach (NucleusWF.Base.Definicion.Organigramas.ROLE rol in orgO.ROLES) Roles[rol.c_role] = 1;

      //Recorro los Nodos
      foreach(NODE nodO in DOC_DDO.NODES)
      {

        //DEPURO LOS ROLES
         deleteItems.Clear();
        foreach(ROLE_NODE rolO in nodO.ROLES_NODE)
          if (!Roles.ContainsKey( rolO.c_rol.ToUpper()) )
            deleteItems.Add(rolO);

        foreach(ROLE_NODE rolO in deleteItems)
          nodO.ROLES_NODE.Remove(rolO);
      }
    }

    public static void SAVE_DDO(NucleusWF.Base.Definicion.Workflows.PROCESS DOC_DDO)
    {
      DEBUG_DDO(DOC_DDO);
      NomadEnvironment.GetCurrentTransaction().SaveRefresh(DOC_DDO);
    }

    public static string CheckMail(string mailAddress)
    {
      char[] cc;
      string[] parts=mailAddress.Split('@');
      string P1;

      if (parts.Length!=2) return "La direccion de Mail tiene que tener solo un '@'";

      //Verifico los Caracteres Finales
      cc=mailAddress.ToCharArray();
      for (int t=0; t<cc.Length; t++)
      {
        if (cc[t]>='A' && cc[t]<='Z') continue;
        if (cc[t]>='a' && cc[t]<='z') continue;
        if (cc[t]>='0' && cc[t]<='9') continue;
        if (cc[t]=='@' || cc[t]=='.'|| cc[t]=='_' || cc[t]=='-') continue;
        return "La direccion de Mail tiene Caracteres no Validos";
      }

      //Verifico el DOMINIO
      parts=parts[1].Split('.');
      if (parts.Length<2) return "La direccion de Mail tiene un DOMINIO no VALIDO";

      //Verifico el PAIS
      P1=parts[parts.Length-1];
      if (("|AF|AL|DZ|As|AD|AO|AI|AQ|AG|AP|AR|AM|AW|AU|AT|AZ|BS|BH|BD|BB|BY|BE|BZ|BJ|BM|BT|BO|BA|BW|BV|BR|IO|BN|BG|BF|MM|BI|KH|CM|CA|CV|KY|CF|TD|CL|CN|CX|CC|CO|KM|CG|CD|CK|CR|CI|HR|CU|CY|CZ|DK|DJ|DM|DO|TP|EC|EG|SV|GQ|ER|EE|ET|FK|FO|FJ|FI|CS|SU|FR|FX|GF|PF|TF|GA|GM|GE|DE|GH|GI|GB|GR|GL|GD|GP|GU|GT|GN|GW|GY|HT|HM|HN|HK|HU|IS|IN|ID|IR|IQ|IE|IL|IT|JM|JP|JO|KZ|KE|KI|KW|KG|LA|LV|LB|LS|LR|LY|LI|LT|LU|MO|MK|MG|MW|MY|MV|ML|MT|MH|MQ|MR|MU|YT|MX|FM|MD|MC|MN|MS|MA|MZ|NA|NR|NP|NL|AN|NT|NC|NZ|NI|NE|NG|NU|NF|KP|MP|NO|OM|PK|PW|PA|PG|PY|PE|PH|PN|PL|PT|PR|QA|RE|RO|RU|RW|GS|SH|KN|LC|PM|ST|VC|SM|SA|SN|SC|SL|SG|SK|SI|SB|SO|ZA|KR|ES|LK|SD|SR|SJ|SZ|SE|CH|SY|TJ|TW|TZ|TH|TG|TK|TO|TT|TN|TR|TM|TC|TV|UG|UA|AE|UK|US|UY|UM|UZ|VU|VA|VE|VN|VG|VI|WF|WS|EH|YE|YU|ZR|ZM|ZW|").Contains("|"+P1.ToUpper()+"|"))
      {
        P1=parts[parts.Length-2];
        if (!("|COM|EDU|GOV|GOB|NET|BIZ|ORG|TV|").Contains("|"+P1.ToUpper()+"|"))
          return "La direccion de Mail tiene un DOMINIO no VALIDO";
      } else
      {
        P1=parts[parts.Length-1];
        if (!("|COM|EDU|GOV|GOB|NET|BIZ|ORG|TV|").Contains("|"+P1.ToUpper()+"|"))
          return "La direccion de Mail tiene un DOMINIO no VALIDO";
      }

      return "";
    }

    public static NomadXML GetVariables(PROCESS pro)
    {
      NomadXML xmlVars,MD,DT,GR,VR;

       xmlVars=new NomadXML("PROCESS");
       MD=xmlVars.AddTailElement("METADATA");
       DT=xmlVars.AddTailElement("DATA");

       GR=MD.AddTailElement("GROUP");
       GR.SetAttr("type", "simple");

      foreach(PROCESS_PARAM par in pro.PROCESS_PARAM)
      {
        VR=GR.AddTailElement("VAR");
        VR.SetAttr("type", par.c_type);
        VR.SetAttr("name", par.c_process_param);
      }

      //fin
      return xmlVars;
   }

    public static NomadXML GetVariables(string c_wf, string c_process)
    {
      NomadXML RS = NomadEnvironment.QueryNomadXML(Workflows.WF.Resources.QRY_PROCESS, "<DATA c_process=\""+c_process+"\" c_wf=\""+c_wf+"\"/>").FirstChild();
      if (RS.GetAttr("oi_process")=="") return null;

      return GetVariables(PROCESS.Get(RS.GetAttr("oi_process"),false));
    }

    public static PROCESS CopyFrom(PROCESS proO, WF wfO, Hashtable Estados, Hashtable Roles, Hashtable Texts, Hashtable Variables, Hashtable Labels, Hashtable Procesos)
    {
      string Err="";
      NomadXML xmlParams, xmlVars;

      //Validaciones
      if (!WF.CODE_TEST(proO.c_process))
        throw new NomadException("El Nombre del Proceso '"+proO.c_process.ToUpper()+"-"+proO.e_automatico.ToString()+"' tiene Caracteres Invalidos....");

      if (Procesos.ContainsKey(proO.c_process.ToUpper()+"-"+proO.e_automatico.ToString()))
        throw new NomadException("El Proceso '"+proO.c_process.ToUpper()+"-"+proO.e_automatico.ToString()+"' esta Duplicado....");

      //Depuro el DDO
      DEBUG_DDO(proO);

      //Creo el Proceso
      PROCESS proP=new PROCESS();
      proP.c_process   =proO.c_process;
      proP.d_process   =proO.d_process;
      proP.e_automatico=proO.e_automatico;
      Procesos[proO.c_process.ToUpper()+"-"+proO.e_automatico.ToString()]=proO;

      //Recorro los Parametros por Proceso
      if (proO.e_automatico==0)
      {
        foreach(PROCESS_PARAM parO in proO.PROCESS_PARAM)
        {
          NomadLog.Info("Agregando la PARAMETRO '"+parO.c_process_param+"' ....");

          PROCESS_PARAM parP;

          parP=new PROCESS_PARAM();
          parP.c_process_param=parO.c_process_param;
          parP.d_process_param=parO.d_process_param;
          parP.c_io           =parO.c_io;
          parP.c_type         =parO.c_type;
          proP.PROCESS_PARAM.Add(parP);
        }
      }

      //Recorro los Roles por Proceso
      if (proO.e_automatico==0)
      {
        foreach(ROLE_PROCESS rolO in proO.ROLES_PRO)
        {
          NomadLog.Info("Agregando la ROL '"+rolO.c_rol+"' ....");

          ROLE_PROCESS rolP;
          rolP=new ROLE_PROCESS();
          rolP.c_rol    =rolO.c_rol;
          rolP.l_iniciar=rolO.l_iniciar;
          proP.ROLES_PRO.Add(rolP);
        }
      }
      xmlVars=WF.VariablesWF(wfO);

      //Recorro los Nodos
      foreach(NODE nodO in proO.NODES)
      {
        NomadLog.Info("Agregando el NODO '"+nodO.d_label+"' ....");

        //Validaciones
        switch(nodO.c_type)
        {
          ////////////////////////////////////////////////////////////////////////////////////////
          // INICIO Y FIN
          case "INI":
            if (nodO.NODE_PARAMS.Count!=0) throw new NomadException("El NODO '"+nodO.d_label+"' ("+nodO.c_type+") tiene 'Parametros' y no los Soporta....");
            if (nodO.ROLES_NODE.Count!=0) throw new NomadException("El NODO '"+nodO.d_label+"' ("+nodO.c_type+") tiene 'Roles Asignados' y no los Soporta....");

            if (nodO.o_compprocess!="")
            {
              xmlParams=PROCESS.GetVariables(proP);
              NomadLog.Info("XML-PARAM: "+xmlParams.ToString());

              Err=Nomad.Base.Report.Formula.Validate(nodO.o_compprocess, xmlParams, xmlVars);
              if (Err!="OK")
                throw new NomadException("El NODO '"+nodO.d_label+"' ("+nodO.c_type+") tiene una Formula/Proceso no Valido. "+Err);
            }

            break;
          case "FIN":
            if (nodO.NODE_PARAMS.Count!=0) throw new NomadException("El NODO '"+nodO.d_label+"' ("+nodO.c_type+") tiene 'Parametros' y no los Soporta....");
            if (nodO.ROLES_NODE.Count!=0) throw new NomadException("El NODO '"+nodO.d_label+"' ("+nodO.c_type+") tiene 'Roles Asignados' y no los Soporta....");
            if (nodO.o_compprocess!="")
            {
              xmlParams=PROCESS.GetVariables(proP);
              NomadLog.Info("XML-PARAM: "+xmlParams.ToString());

              Err=Nomad.Base.Report.Formula.Validate(nodO.o_compprocess, xmlParams, xmlVars);
              if (Err!="OK")
                throw new NomadException("El NODO '"+nodO.d_label+"' ("+nodO.c_type+") tiene una Formula/Proceso no Valido. "+Err);
            }
            break;
          case "STOP":
            if (nodO.NODE_PARAMS.Count!=0) throw new NomadException("El NODO '"+nodO.d_label+"' ("+nodO.c_type+") tiene 'Parametros' y no los Soporta....");
            if (nodO.ROLES_NODE.Count!=0) throw new NomadException("El NODO '"+nodO.d_label+"' ("+nodO.c_type+") tiene 'Roles Asignados' y no los Soporta....");
            if (nodO.o_compprocess!="")
            {
              xmlParams=PROCESS.GetVariables(proP);
              NomadLog.Info("XML-PARAM: "+xmlParams.ToString());

              Err=Nomad.Base.Report.Formula.Validate(nodO.o_compprocess, null, xmlVars);
              if (Err!="OK")
                throw new NomadException("El NODO '"+nodO.d_label+"' ("+nodO.c_type+") tiene una Formula/Proceso no Valido. "+Err);
            }
            break;

          ////////////////////////////////////////////////////////////////////////////////////////
          // LABEL-GOTO
          case "LABEL":
            if (nodO.NODE_PARAMS.Count!=0) throw new NomadException("El NODO '"+nodO.d_label+"' ("+nodO.c_type+") tiene 'Parametros' y no los Soporta....");
            if (nodO.ROLES_NODE.Count!=0) throw new NomadException("El NODO '"+nodO.d_label+"' ("+nodO.c_type+") tiene 'Roles Asignados' y no los Soporta....");

            if (Labels.ContainsKey(proO.c_process.ToUpper()+"."+nodO.d_label.ToUpper()))
              throw new NomadException("La Etiqueta '"+nodO.d_label+"' del Proceso '"+proO.c_process+"' esta Duplicada....");

            Labels[proO.c_process.ToUpper()+"."+nodO.d_label.ToUpper()]=proO;
            break;
          case "GOTO":
            if (nodO.NODE_PARAMS.Count!=0) throw new NomadException("El NODO '"+nodO.d_label+"' ("+nodO.c_type+") tiene 'Parametros' y no los Soporta....");
            if (nodO.ROLES_NODE.Count!=0) throw new NomadException("El NODO '"+nodO.d_label+"' ("+nodO.c_type+") tiene 'Roles Asignados' y no los Soporta....");
            break;

          ////////////////////////////////////////////////////////////////////////////////////////
          // COMANDOS
          case "WAIT_TIME":
            if (nodO.NODE_PARAMS.Count!=0) throw new NomadException("El NODO '"+nodO.d_label+"' ("+nodO.c_type+") tiene 'Parametros' y no los Soporta....");
            if (nodO.ROLES_NODE.Count!=0) throw new NomadException("El NODO '"+nodO.d_label+"' ("+nodO.c_type+") tiene 'Roles Asignados' y no los Soporta....");

            if (int.Parse(nodO.d_param_1)<10)
              throw new NomadException("El NODO '"+nodO.d_label+"' ("+nodO.c_type+") tiene un tiempo de espera Menor a 10 minutos...");
            break;
          case "STATUS":
            if (nodO.NODE_PARAMS.Count!=0) throw new NomadException("El NODO '"+nodO.d_label+"' ("+nodO.c_type+") tiene 'Parametros' y no los Soporta....");
            if (nodO.ROLES_NODE.Count!=0) throw new NomadException("El NODO '"+nodO.d_label+"' ("+nodO.c_type+") tiene 'Roles Asignados' y no los Soporta....");

            if (!Estados.ContainsKey(nodO.d_param_1.ToUpper()))
              throw new NomadException("El NODO '"+nodO.d_label+"' ("+nodO.c_type+") tiene un ESTADO '"+nodO.d_param_1+"' no VALIDO....");
            break;

    case "VENC":
      if (nodO.NODE_PARAMS.Count != 0) throw new NomadException("El NODO '" + nodO.d_label + "' (" + nodO.c_type + ") tiene 'Parametros' y no los Soporta....");
      if (nodO.ROLES_NODE.Count != 0) throw new NomadException("El NODO '" + nodO.d_label + "' (" + nodO.c_type + ") tiene 'Roles Asignados' y no los Soporta....");

      switch (nodO.d_param_1)
      {
        case "RESET":
          break;
        case "RECALC":
          break;
        case "CALC":
          Err = Nomad.Base.Report.Formula.Validate(nodO.o_compformula, null, xmlVars);
          if (Err!="OK")
            throw new NomadException("El NODO '" + nodO.d_label + "' (" + nodO.c_type + ") tiene una Formula no Valida " + Err);
          break;
        default:
          throw new NomadException("El NODO '" + nodO.d_label + "' (" + nodO.c_type + ") tiene un TIPO '" + nodO.d_param_1 + "' no VALIDO....");
      }
      break;

    case "LOG":
            if (nodO.NODE_PARAMS.Count!=0) throw new NomadException("El NODO '"+nodO.d_label+"' ("+nodO.c_type+") tiene 'Parametros' y no los Soporta....");
            if (nodO.ROLES_NODE.Count!=0) throw new NomadException("El NODO '"+nodO.d_label+"' ("+nodO.c_type+") tiene 'Roles Asignados' y no los Soporta....");

            if (!Texts.ContainsKey(nodO.d_param_1.ToUpper()))
              throw new NomadException("El NODO '"+nodO.d_label+"' ("+nodO.c_type+") tiene un TEXTO '"+nodO.d_param_1+"' no VALIDO....");
            break;
          case "MAIL":
            if (nodO.NODE_PARAMS.Count==0) throw new NomadException("El NODO '"+nodO.d_label+"' ("+nodO.c_type+") No tiene Destinatarios Definidos....");
            if (nodO.ROLES_NODE.Count!=0) throw new NomadException("El NODO '"+nodO.d_label+"' ("+nodO.c_type+") tiene 'Roles Asignados' y no los Soporta....");

            if (!TEXT.CheckText(nodO.d_param_1, wfO, ref Err))
              throw new NomadException("El NODO '"+nodO.d_label+"' ("+nodO.c_type+") tiene un ASUNTO "+Err);

            if (!Texts.ContainsKey(nodO.d_param_2.ToUpper()))
              throw new NomadException("El NODO '"+nodO.d_label+"' ("+nodO.c_type+") tiene un TEXTO '"+nodO.d_param_1+"' no VALIDO....");

            foreach(PARAM parO in nodO.NODE_PARAMS)
            {
              switch(parO.c_type)
              {
                case "OWNER":
                  break;

                case "ROLE":
                  if (!Roles.ContainsKey(parO.d_valor.ToUpper()))
                    throw new NomadException("El NODO '"+nodO.d_label+"' ("+nodO.c_type+") tiene un como Destinatario un Actor/Rol '"+parO.d_valor+"' no VÁLIDO....");
                  break;

                case "MAIL":
                  Err=CheckMail(parO.d_valor);
                  if (Err!="")
                    throw new NomadException("El NODO '"+nodO.d_label+"' ("+nodO.c_type+") tiene un como Destinatario un Mail no Válido "+Err);
                  break;

                case "ETTY":
                  if (!NomadProxy.GetProxy().FileServiceIO().ExistsFile("DB\\Nomad\\Base\\Login\\Entidades", parO.d_valor+".ENTIDAD.XML"))
                    throw new NomadException("El NODO '"+nodO.d_label+"' ("+nodO.c_type+") tiene un como Destinatario una Entidad '"+parO.d_valor+"' no VÁLIDA....");
                  break;

                case "FORMULA":
                  Err=Nomad.Base.Report.Formula.Validate(parO.o_formula, null, xmlVars);
                  if (Err!="OK")
                    throw new NomadException("El NODO '"+nodO.d_label+"' ("+nodO.c_type+") tiene un como Destinatario una Formula no Válida "+Err);
                  break;

                default:
                  throw new NomadException("El NODO '"+nodO.d_label+"' ("+nodO.c_type+") tiene un DESTINATARIO de TIPO '"+parO.c_type+"' no VÁLIDO....");
              }
            }
            break;

          ////////////////////////////////////////////////////////////////////////////////////////
          // FLUJO
          case "SUBPROCESS":
      NucleusWF.Base.Definicion.Workflows.PROCESS childPRO = null;
      if (nodO.NODE_PARAMS.Count != 0) throw new NomadException("El NODO '" + nodO.d_label + "' (" + nodO.c_type + ") tiene 'Parametros' y no los Soporta....");
      if (nodO.ROLES_NODE.Count!=0) throw new NomadException("El NODO '"+nodO.d_label+"' ("+nodO.c_type+") tiene 'Roles Asignados' y no los Soporta....");

      //Buscar el Subproceso hijo
      foreach (NucleusWF.Base.Definicion.Workflows.PROCESS cur in wfO.PROCESS)
      {
        if (cur.e_automatico == 0 && cur.c_process == nodO.d_param_1)
        {
          childPRO = cur;
          break;
        }
      }

      //Validaciones
      if (childPRO == null)
        throw new Exception("El NODO '"+nodO.d_label+"' ("+nodO.c_type+") llama a un Subproceso '"+nodO.d_param_1+"' que no existe....");

      //Ok
      break;

          case "PAR":
            if (nodO.NODE_PARAMS.Count==0) throw new NomadException("El NODO '"+nodO.d_label+"' ("+nodO.c_type+") no tiene 'Ramas'....");
            if (nodO.ROLES_NODE.Count!=0) throw new NomadException("El NODO '"+nodO.d_label+"' ("+nodO.c_type+") tiene 'Roles Asignados' y no los Soporta....");
            break;

          case "SWITCH":
            if (nodO.NODE_PARAMS.Count==0) throw new NomadException("El NODO '"+nodO.d_label+"' ("+nodO.c_type+") no tiene 'Ramas'....");
            if (nodO.ROLES_NODE.Count!=0) throw new NomadException("El NODO '"+nodO.d_label+"' ("+nodO.c_type+") tiene 'Roles Asignados' y no los Soporta....");

            int def=0;
            foreach(PARAM parO in nodO.NODE_PARAMS)
            {
              switch(parO.c_type)
              {
                case "CASE":
                  break;

                case "DEFAULT":
                  def++;
                  break;

                default:
                  throw new NomadException("El NODO '"+nodO.d_label+"' ("+nodO.c_type+") tiene un RAMA '"+parO.c_type+"' no VALIDO....");
              }
            }
            if (def==0)
              throw new NomadException("El NODO '"+nodO.d_label+"' ("+nodO.c_type+") NO tine ninguna RAMA para 'OTROS CASOS'....");
            if (def!=1)
              throw new NomadException("El NODO '"+nodO.d_label+"' ("+nodO.c_type+") tiene mas de una RAMA para 'OTROS CASOS'....");

            if (nodO.o_compformula!="")
            {
              Err=Nomad.Base.Report.Formula.Validate(nodO.o_compformula, null, xmlVars);
              if (Err!="OK")
                throw new NomadException("El NODO '"+nodO.d_label+"' ("+nodO.c_type+") tiene una Formula/Proceso no Valido. "+Err);
            }
            break;

          ////////////////////////////////////////////////////////////////////////////////////////
          // EXTERNOS
          case "FORM":
            if (nodO.NODE_PARAMS.Count!=0) throw new NomadException("El NODO '"+nodO.d_label+"' ("+nodO.c_type+") tiene 'Parametros' y no los Soporta....");
            if (nodO.ROLES_NODE.Count==0) throw new NomadException("El NODO '"+nodO.d_label+"' ("+nodO.c_type+") NO tiene 'Roles Asignados'...");

            foreach(ROLE_NODE rolO in nodO.ROLES_NODE)
            {
              if (!Roles.ContainsKey( rolO.c_rol.ToUpper()) )
                throw new NomadException("El NODO '"+nodO.d_label+"' ("+nodO.c_type+") tiene un Rol '"+rolO.c_rol+"' no Valido...");
            }

            if (nodO.o_compprocess!="")
            {
              Err=Nomad.Base.Report.Formula.Validate(nodO.o_compprocess, null, xmlVars);
              if (Err!="OK")
                throw new NomadException("El NODO '"+nodO.d_label+"' ("+nodO.c_type+") tiene una Formula/Proceso no Valido. "+Err);
            }
            break;

          case "VAR":
            if (nodO.NODE_PARAMS.Count!=0) throw new NomadException("El NODO '"+nodO.d_label+"' ("+nodO.c_type+") tiene 'Parametros' y no los Soporta....");
            if (nodO.ROLES_NODE.Count!=0) throw new NomadException("El NODO '"+nodO.d_label+"' ("+nodO.c_type+") tiene 'Roles Asignados' y no los Soporta....");

            if (nodO.o_compprocess!="")
            {
              Err=Nomad.Base.Report.Formula.Validate(nodO.o_compprocess, null, xmlVars);
              if (Err!="OK")
                throw new NomadException("El NODO '"+nodO.d_label+"' ("+nodO.c_type+") tiene una Formula/Proceso no Valido. "+Err);
            }
            break;

          case "SOAP":
          case "JSON":
          case "SP":
            if (nodO.NODE_PARAMS.Count==0) throw new NomadException("El NODO '"+nodO.d_label+"' ("+nodO.c_type+") No tiene definidos los Parametros de Entrada/Salida....");
            if (nodO.ROLES_NODE.Count !=0) throw new NomadException("El NODO '"+nodO.d_label+"' ("+nodO.c_type+") tiene 'Roles Asignados' y no los Soporta....");

            xmlParams=NucleusWF.Base.Definicion.Integracion.INT_MAG.GetVariables(nodO.d_param_1);
            NomadLog.Info("XML-PARAM: "+xmlParams.ToString());

            foreach(PARAM parO in nodO.NODE_PARAMS)
            {
              switch(parO.c_type)
              {
                case "IN":
                  if (parO.o_compilada!="")
                  {
                    Err=Nomad.Base.Report.Formula.Validate(parO.o_compilada, xmlParams, xmlVars);
                    if (Err!="OK")
                      throw new NomadException("El NODO '"+nodO.d_label+"' ("+nodO.c_type+") tiene una Formula/Proceso no Valido. "+Err);
                  }
                  break;

                case "OUT":
                  if (parO.o_compilada!="")
                  {
                    Err=Nomad.Base.Report.Formula.Validate(parO.o_compilada, xmlParams, xmlVars);
                    if (Err!="OK")
                      throw new NomadException("El NODO '"+nodO.d_label+"' ("+nodO.c_type+") tiene una Formula/Proceso no Valido. "+Err);
                  }
                  break;

                default:
                  throw new NomadException("El NODO '"+nodO.d_label+"' ("+nodO.c_type+") tiene un SCRIPT tipo '"+parO.c_type+"' no VALIDO....");
              }
            }
            break;

          case "CALL":
            if (nodO.NODE_PARAMS.Count==0) throw new NomadException("El NODO '"+nodO.d_label+"' ("+nodO.c_type+") No tiene definidos los Parametros de Entrada/Salida....");
            if (nodO.ROLES_NODE.Count !=0) throw new NomadException("El NODO '"+nodO.d_label+"' ("+nodO.c_type+") tiene 'Roles Asignados' y no los Soporta....");

            xmlParams=PROCESS.GetVariables(nodO.d_param_1, nodO.d_param_2);
            if (xmlParams==null)
              throw new NomadException("El NODO '"+nodO.d_label+"' ("+nodO.c_type+") Apunta a un WF no PUBLICADO. "+Err);

            NomadLog.Info("XML-PARAM: "+xmlParams.ToString());

            foreach(PARAM parO in nodO.NODE_PARAMS)
            {
              switch(parO.c_type)
              {
                case "IN":
                  if (parO.o_compilada!="")
                  {
                    Err=Nomad.Base.Report.Formula.Validate(parO.o_compilada, xmlParams, xmlVars);
                    if (Err!="OK")
                      throw new NomadException("El NODO '"+nodO.d_label+"' ("+nodO.c_type+") tiene una Formula/Proceso no Valido. "+Err);
                  }
                  break;

                case "OUT":
                  if (parO.o_compilada!="")
                  {
                    Err=Nomad.Base.Report.Formula.Validate(parO.o_compilada, xmlParams, xmlVars);
                    if (Err!="OK")
                      throw new NomadException("El NODO '"+nodO.d_label+"' ("+nodO.c_type+") tiene una Formula/Proceso no Valido. "+Err);
                  }
                  break;

                default:
                  throw new NomadException("El NODO '"+nodO.d_label+"' ("+nodO.c_type+") tiene un SCRIPT tipo '"+parO.c_type+"' no VALIDO....");
              }
            }
            break;

          ////////////////////////////////////////////////////////////////////////////////////////
          // NO IMPLEMENTADO
          case "WAIT_TO":
          case "JSCRIPT":
          case "METHOD":
          case "CSHARP":
          case "XQUERY":
          case "DB":
            throw new NomadException("El TIPO '"+nodO.c_type+"' del NODO '"+nodO.d_label+"' no esta Implementado....");

          default:
            throw new NomadException("El TIPO '"+nodO.c_type+"' del NODO '"+nodO.d_label+"' no es Valido....");
        }

        //Agregar Roles por Grupo

        //Agrego el NODO
        NODE nodP;
        nodP=new NODE();
        nodP.c_type       =nodO.c_type;
        nodP.n_secuence   =nodO.n_secuence;
        nodP.d_label      =nodO.d_label;
        nodP.d_param_1    =nodO.d_param_1;
        nodP.d_param_2    =nodO.d_param_2;
        nodP.d_param_3    =nodO.d_param_3;
        nodP.d_param_4    =nodO.d_param_4;
        nodP.o_formula    =nodO.o_formula;
        nodP.n_horas      =nodO.n_horas;
        nodP.o_process    =nodO.o_process;
        nodP.o_compformula=nodO.o_compformula;
        nodP.o_compprocess=nodO.o_compprocess;

        //Agrego los Parametros
        foreach(PARAM parO in nodO.NODE_PARAMS)
          nodP.NODE_PARAMS.Add(parO.Duplicate());

        //Agrego los Roles por NODO
        foreach(ROLE_NODE rolO in nodO.ROLES_NODE)
           nodP.ROLES_NODE.Add(rolO.Duplicate());

        proP.NODES.Add(nodP);
      }

      return proP;
    }

    public static void NodeDEL(int oi_process, int oi_node)
    {
        //Cargo el Nodo
        NODE NOD = NODE.Get(oi_node.ToString(), true);
        NOD.NODE_PARAMS.Clear();

        //Elimino el NODO
        NomadEnvironment.GetCurrentTransaction().Delete(NOD);
    }

    public static void NodeUP(int oi_process, int oi_node)
    {
        //Cargo el Proceso
        PROCESS PRO = PROCESS.Get(oi_process, false);

        //Cargo el Nodo
        NODE NOD = (NODE)PRO.NODES.GetById(oi_node.ToString());
        if (NOD.c_type == "FIN") return;
        if (NOD.c_type == "STOP") return;

        //Busco el Nodo Superior
        NODE NODUP = null;
        foreach (NODE cur in PRO.NODES)
        {
            if (cur.n_secuence >= NOD.n_secuence) continue;
            if (NODUP == null || NODUP.n_secuence < cur.n_secuence) NODUP = cur;
        }
        if (NODUP == null) return;
        if (NODUP.c_type == "INI") return;

        //Intercambio de Secuencias
        double nSec = NODUP.n_secuence;
        NODUP.n_secuence = NOD.n_secuence;
        NOD.n_secuence = nSec;

        //Guardo el Proceso
        NomadEnvironment.GetCurrentTransaction().Save(PRO);
    }

    public static void NodeDOWN(int oi_process, int oi_node)
    {
        //Cargo el Proceso
        PROCESS PRO = PROCESS.Get(oi_process, false);

        //Cargo el Nodo
        NODE NOD = (NODE)PRO.NODES.GetById(oi_node.ToString());
        if (NOD.c_type == "INI") return;

        //Busco el Nodo Superior
        NODE NODDW = null;
        foreach (NODE cur in PRO.NODES)
        {
            if (cur.n_secuence <= NOD.n_secuence) continue;
            if (NODDW == null || NODDW.n_secuence > cur.n_secuence) NODDW = cur;
        }
        if (NODDW == null) return;
        if (NODDW.c_type == "FIN") return;
        if (NODDW.c_type == "STOP") return;

        //Intercambio de Secuencias
        double nSec = NODDW.n_secuence;
        NODDW.n_secuence = NOD.n_secuence;
        NOD.n_secuence = nSec;

        //Guardo el Proceso
        NomadEnvironment.GetCurrentTransaction().Save(PRO);
    }

    public static void RoleInicia(int oi_process, int oi_role)
    {
        //Cargo el Proceso
        PROCESS PRO = PROCESS.Get(oi_process, false);
        NucleusWF.Base.Definicion.Organigramas.ROLE MyROL = NucleusWF.Base.Definicion.Organigramas.ROLE.Get(oi_role.ToString(), false);

        //Cargo el Nodo
        ROLE_PROCESS ROL = (ROLE_PROCESS)PRO.ROLES_PRO.GetByAttribute("c_rol", MyROL.c_role);
        if (ROL == null)
        {
            ROL = new ROLE_PROCESS();
            ROL.l_iniciar = true;
            ROL.c_rol = MyROL.c_role;

            PRO.ROLES_PRO.Add(ROL);
        }
        else
        {
            ROL.l_iniciar = true;
        }

        //Guardo el Proceso
        NomadEnvironment.GetCurrentTransaction().Save(PRO);
    }

    public static void RoleNoInicia(int oi_process, int oi_role)
    {
        //Cargo el Proceso
        PROCESS PRO = PROCESS.Get(oi_process, false);
        NucleusWF.Base.Definicion.Organigramas.ROLE MyROL = NucleusWF.Base.Definicion.Organigramas.ROLE.Get(oi_role.ToString(), false);

        //Cargo el Nodo
        ROLE_PROCESS ROL = (ROLE_PROCESS)PRO.ROLES_PRO.GetByAttribute("c_rol", MyROL.c_role);
        if (ROL == null)
        {
            ROL = new ROLE_PROCESS();
            ROL.l_iniciar = false;
            ROL.c_rol = MyROL.c_role;

            PRO.ROLES_PRO.Add(ROL);
        }
        else
        {
            ROL.l_iniciar = false;
        }

        //Guardo el Proceso
        NomadEnvironment.GetCurrentTransaction().Save(PRO);
    }

    public static NucleusWF.Base.Definicion.Workflows.PROCESS CREATE_DDO(string oi_wf)
    {

        NODE Ni = new NODE();
        Ni.c_type = "INI";
        Ni.n_secuence = 0.0;
        Ni.d_label = "Inicio";

        NODE Nf = new NODE();
        Nf.c_type = "FIN";
        Nf.n_secuence = 16777216.0;
        Nf.d_label = "Fin";

        PROCESS retval = new PROCESS();
        retval.e_automatico = 0;
        retval.NODES.Add(Ni);
        retval.NODES.Add(Nf);

        return retval;
    }

    public static void ParamRIGHT(int oi_node, int e_subproceso)
    {
        //Cargo el Nodo
        NODE NOD = NODE.Get(oi_node.ToString(), true);
        PARAM PAR = (PARAM)NOD.NODE_PARAMS.GetByAttribute("e_subproceso", e_subproceso);

        if (PAR == null) return;

        PARAM PRE = null;
        foreach (PARAM CUR in NOD.NODE_PARAMS)
        {
            if (CUR.e_subproceso == e_subproceso) continue;
            if (CUR.e_orden < PAR.e_orden) continue;
            if (PRE != null && CUR.e_orden > PRE.e_orden) continue;

            PRE = CUR;
        }
        if (PRE == null) return;

        //Intercambio los ordenes
        int orden = PRE.e_orden;
        PRE.e_orden = PAR.e_orden;
        PAR.e_orden = orden;

        //Guardo el NODO
        NomadEnvironment.GetCurrentTransaction().Save(NOD);
    }

    public static void ParamLEFT(int oi_node, int e_subproceso)
    {
        //Cargo el Nodo
        NODE NOD = NODE.Get(oi_node.ToString(), true);
        PARAM PAR = (PARAM)NOD.NODE_PARAMS.GetByAttribute("e_subproceso", e_subproceso);

        if (PAR == null) return;

        PARAM PRE = null;
        foreach (PARAM CUR in NOD.NODE_PARAMS)
        {
            if (CUR.e_subproceso == e_subproceso) continue;
            if (CUR.e_orden > PAR.e_orden) continue;
            if (PRE != null && CUR.e_orden < PRE.e_orden) continue;

            PRE = CUR;
        }
        if (PRE == null) return;

        //Intercambio los ordenes
        int orden = PRE.e_orden;
        PRE.e_orden = PAR.e_orden;
        PAR.e_orden = orden;

        //Guardo el NODO
        NomadEnvironment.GetCurrentTransaction().Save(NOD);
    }

    public static void ParamDEL(int oi_node, int e_subproceso)
    {
        //Cargo el Nodo
        NODE NOD = NODE.Get(oi_node.ToString(), false);
        PROCESS PRO = PROCESS.Get(NOD.oi_process.ToString(), false);
        WF WFR = WF.Get(PRO.oi_wf.ToString(), false);

        //Busco el Proceso Actual
        PRO = (PROCESS)WFR.PROCESS.GetById(NOD.oi_process.ToString());

        //Busco el Nodo Actual
        NOD = (NODE)PRO.NODES.GetById(oi_node.ToString());

        //Elimino el Parametro
        PARAM PAR = (PARAM)NOD.NODE_PARAMS.GetByAttribute("e_subproceso", e_subproceso);
        NOD.NODE_PARAMS.Remove(PAR);

        //Guardo el WF
        NomadEnvironment.GetCurrentTransaction().Save(WFR);

        //Elimino el Subproceso
        PROCESS PRODEL = null;
        foreach (PROCESS CUR in WFR.PROCESS)
        {
            if (CUR.e_automatico == e_subproceso && CUR.c_process == PRO.c_process)
            {
                PRODEL = PROCESS.Get(CUR.Id, true);
                NomadEnvironment.GetCurrentTransaction().Delete(PRODEL);
                break;
            }
        }
    }
}
}



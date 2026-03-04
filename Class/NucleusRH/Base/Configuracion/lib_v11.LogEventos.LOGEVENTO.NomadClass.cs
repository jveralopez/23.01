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

namespace NucleusRH.Base.Configuracion.LogEventos
{

    //////////////////////////////////////////////////////////////////////////////////
    //Clase Alertas
    public partial class LOGEVENTO : Nomad.NSystem.Base.NomadObject
    {
    public bool m_bSaveToDB=false;

    public static NomadXML GetTaskList(string classEvent)
    {
      //Datos
      NomadXML RS=(NomadXML)NomadProxy.GetProxy().CacheGetObj("EVE-TASK-"+classEvent.ToUpper());

      //Guarda en Base de DATOS
      if (RS==null)
      {
        string EventGroup=classEvent.Substring(0, classEvent.LastIndexOf('.'));
        string EventName=classEvent.Substring(classEvent.LastIndexOf('.')+1);

        NomadXML PARAM=new NomadXML("PARAM");
        PARAM.SetAttr("c_grupo_evento", EventGroup);
        PARAM.SetAttr("c_evento", EventName);

        RS=NomadProxy.GetProxy().SQLService().GetXML("CLASS.NucleusRH.Base.Configuracion.LogEventos.LOGEVENTO.QUERY_TASK", PARAM.ToString());
        if (RS.isDocument) RS=RS.FirstChild();

        //Actualizo el Cache
        NomadProxy.GetProxy().CacheAdd("EVE-TASK-"+classEvent.ToUpper(), RS);
      }

      return RS;
    }

    private static LOGEVENTO internalGet(string classEvent, string keys, string[] param)
    {
      try
      {
        string EventGroup=classEvent.Substring(0, classEvent.LastIndexOf('.'));
        string EventName=classEvent.Substring(classEvent.LastIndexOf('.')+1);

        //Obtengo el Archivo de Definicion
        NomadXML xmlDEFINE=NomadProxy.GetProxy().FileService().LoadFileXML("EVENT", EventGroup+".event.XML");
        if (xmlDEFINE.isDocument) xmlDEFINE=xmlDEFINE.FirstChild();

        //Verifico si hay alguna Tarea escuchando este evento....
        if (!xmlDEFINE.GetAttrBool("save-db"))
          if (GetTaskList(classEvent).FirstChild()==null)
            return null;

        //Obtengo el Evento
        NomadXML xmlEVENT=xmlDEFINE.FindElement("EVENTS").FindElement2("EVENT", "name", EventName);

        //Analizo los Parametros
        NomadXML xmlPARAMS=xmlDEFINE.FindElement("PARAMS");
        NomadXML xmlQUERY =xmlDEFINE.FindElement("QRY");

        NomadXML xmlPC, xmlPARAM;
        int inParam, outParam, cntParam;

        inParam=0; outParam=0; cntParam=0;
        xmlPARAM=new NomadXML("EVE");
        xmlPARAM.SetAttr("IDS", keys);
        xmlPARAM.SetAttr("eve_keyname", xmlDEFINE.GetAttr("key"));
        for (xmlPC=xmlPARAMS.FirstChild(); xmlPC!=null; xmlPC=xmlPC.Next())
        {
          if (xmlPC.GetAttr("TYPE").ToUpper()=="IN")
          {
            inParam++;
            cntParam++;
            if (inParam>param.Length) throw NomadException.NewInternalException("EVENT.Get.INVALID PARAM COUNT");
            xmlPARAM.SetAttr(xmlPC.GetAttr("name"), param[inParam-1]);
          } else
          {
            outParam++;
            cntParam++;
          }
        }
        if (inParam!=param.Length)
          throw NomadException.NewInternalException("EVENT.Get.INVALID PARAM COUNT");
        if (cntParam>9)
          throw NomadException.NewInternalException("EVENT.Get.INVALID PARAM COUNT");

        NomadXML xmlRESULT=null;
        if (xmlQUERY!=null && xmlQUERY.FirstChild()!=null)
        {
          //Ejecuto la Consulta
          xmlRESULT=NomadProxy.GetProxy().SQLService().GetXML(xmlQUERY, xmlPARAM);
          if (xmlRESULT.isDocument) xmlRESULT=xmlRESULT.FirstChild();
        } else
        {
          xmlRESULT=xmlPARAM;
          xmlRESULT.SetAttr("label", xmlEVENT.GetAttr("label"));
        }
        xmlRESULT.SetAttr("save-db", xmlDEFINE.GetAttrBool("save-db"));

        //Genero el EVENTO
        LOGEVENTO objEVENT=new LOGEVENTO();
        objEVENT.c_grupo_evento=EventGroup;
        objEVENT.c_evento=EventName;
        objEVENT.d_evento=xmlEVENT.GetAttr("label");
        objEVENT.d_eve_keyname=xmlDEFINE.GetAttr("key");

        objEVENT.f_logevento=DateTime.Now;
        objEVENT.c_loglevel=xmlEVENT.GetAttr("level");
        objEVENT.d_logevento=xmlRESULT.GetAttr("label");
        objEVENT.o_logdata=xmlRESULT.ToString();
        objEVENT.d_key=(keys.Length>99?keys.Substring(0, 99):keys);

        //Informacion Adicional Temporal
        objEVENT.m_bSaveToDB=xmlDEFINE.GetAttrBool("save-db");

        cntParam=0;
        for (xmlPC=xmlPARAMS.FirstChild(); xmlPC!=null; xmlPC=xmlPC.Next())
        {
          cntParam++;

          switch (cntParam)
          {
            case 1: objEVENT.d_param01=xmlRESULT.GetAttr(xmlPC.GetAttr("name")); break;
            case 2: objEVENT.d_param02=xmlRESULT.GetAttr(xmlPC.GetAttr("name")); break;
            case 3: objEVENT.d_param03=xmlRESULT.GetAttr(xmlPC.GetAttr("name")); break;
            case 4: objEVENT.d_param04=xmlRESULT.GetAttr(xmlPC.GetAttr("name")); break;
            case 5: objEVENT.d_param05=xmlRESULT.GetAttr(xmlPC.GetAttr("name")); break;
            case 6: objEVENT.d_param06=xmlRESULT.GetAttr(xmlPC.GetAttr("name")); break;
            case 7: objEVENT.d_param07=xmlRESULT.GetAttr(xmlPC.GetAttr("name")); break;
            case 8: objEVENT.d_param08=xmlRESULT.GetAttr(xmlPC.GetAttr("name")); break;
            case 9: objEVENT.d_param09=xmlRESULT.GetAttr(xmlPC.GetAttr("name")); break;
          }
        }

        //Resultado
        return objEVENT;
      } catch (Exception err)
      {
        NomadException MyEX=NomadException.NewInternalException("EVENT.Get.EXCEPTION", err);
        MyEX.SetValue("classEvent", classEvent);
        MyEX.SetValue("params", param.Length.ToString());
        throw MyEX;
      }
    }

    public static LOGEVENTO Get(string classEvent, string key, params string[] param)
        {
      return LOGEVENTO.internalGet(classEvent, key, param);
        }

    public static void Call(string classEvent, string key, params string[] param)
    {
      try
      {
        LOGEVENTO.Call(LOGEVENTO.internalGet(classEvent, key, param));
      }
      catch (Exception ex)
          {

            NomadException myEX=NomadException.NewInternalException("LOGEVENTO.Call-FAILED", ex);
            myEX.SetValue("classEvent", classEvent);
            myEX.SetValue("key", key);
            myEX.Dump();
          }
    }

    public static void Call(LOGEVENTO eve)
    {
      if (eve==null) return;

      NomadXML lstTASK=GetTaskList(eve.c_grupo_evento+"."+eve.c_evento);

      if (lstTASK.FirstChild()!=null)
      {
        NomadXML GroupXML=new NomadXML(eve.o_logdata);
        if (GroupXML.isDocument) GroupXML=GroupXML.FirstChild();

        //Realizo las Ejecuciones
        for (NomadXML curTASK=lstTASK.FirstChild(); curTASK!=null; curTASK=curTASK.Next())
        {
          DateTime exeTime;
          int iTime=curTASK.GetAttrInt("hora")/100;

          //Calculo la Fecha de Corrida
          if (curTASK.GetAttr("modo")=="T")
          {
            exeTime=DateTime.Now.AddMinutes((iTime/100)*60+(iTime%100));
          } else
          {
            exeTime=DateTime.Today.AddMinutes((iTime/100)*60+(iTime%100));
            if (exeTime<DateTime.Now) exeTime=exeTime.AddDays(1);
          }
          GroupXML.SetAttr("eve_name"   , curTASK.GetAttr("eve"));

#if (NOMADDEBUG)
          NucleusRH.Base.Configuracion.Progs.PROG.ExecuteNow(curTASK.GetAttr("oi"), GroupXML);
#else
          //Realizo la Ejecucion modo EVENTO
          RPCService objRPC = NomadProxy.GetProxy().RPCService();
          objRPC.AddParam(new RPCParam("pOI", "IN", curTASK.GetAttr("oi")));
          objRPC.AddParam(new RPCParam("pEVE", "IN", "Nomad.NSystem.Object", "Nomad.NSystem.Proxy.NomadXML", GroupXML.ToString()));
          objRPC.Execute("Programador_EVENT_"+Nomad.NSystem.Functions.StringUtil.GenerateUUID(), 1, exeTime, "SCHEDULER", "NucleusRH.Base.Configuracion.Progs.PROG", "ExecuteNow");
#endif
        }
      }

      //Guardo en Base de Datos?
      if (eve.m_bSaveToDB)
        NomadEnvironment.GetCurrentTransaction().Save(eve);
    }

  }
}



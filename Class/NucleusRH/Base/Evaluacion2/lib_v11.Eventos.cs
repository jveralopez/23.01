using System;
using System.Xml;
using System.IO;
using System.Collections;

using Nomad.NSystem.Document;
using Nomad.NSystem.Proxy;
using Nomad.NSystem.Base;
using Nomad.NSystem.Functions;

namespace NucleusRH.Base.Evaluacion2.Eventos
{

  //////////////////////////////////////////////////////////////////////////////////
  //Clase Evento Evaluativo
  public partial class EVENTO
  {
    public void RefreshPlantilla()
    {
      this.oi_escala = this.Getoi_tipo_eval().oi_escala;
      this.oi_clase_org = this.Getoi_tipo_eval().oi_clase_org;
      this.oi_clase_orgNull = this.Getoi_tipo_eval().oi_clase_orgNull;

      //Recorro las plantillas y las agrego al EVENTO.
      foreach (NucleusRH.Base.Evaluacion2.Tipos_Evaluacion.PLANTILLA PLA in this.Getoi_tipo_eval().PLANTILLAS)
      {
        PLANTILLA MyPLA = new PLANTILLA();

        MyPLA.oi_plantilla_def = PLA.Id;
        MyPLA.oi_escala = PLA.oi_escala;
        MyPLA.oi_escala_item = PLA.oi_escala_item;
        MyPLA.oi_escala_itemNull = PLA.oi_escala_itemNull;
        MyPLA.oi_escala_subitem = PLA.oi_escala_subitem;
        MyPLA.oi_escala_subitemNull = PLA.oi_escala_subitemNull;
        MyPLA.n_ponderacion = PLA.n_ponderacion;
        MyPLA.l_habilitada = PLA.l_habilitada;

        this.PLANTILLAS.Add(MyPLA);
      }
    }

    public void RefreshEtapas()
    {
      NucleusRH.Base.Evaluacion2.Tipos_Evaluacion.TIPO_EVAL MyTIPO=this.Getoi_tipo_eval();
      ETAPA MyETA;

      //Pre evaluacion
      MyETA= new ETAPA();
      MyETA.c_etapa="01";
      MyETA.d_etapa="Preevaluación";
      MyETA.c_estado_evento="I";
      MyETA.c_estado_eval="AB";
      MyETA.l_habilitado=MyTIPO.l_pree;
      MyETA.f_inicio=this.f_desde;
      MyETA.c_rol_habilitado="E";
      MyETA.c_estado_aceptarNull=true;
      this.ETAPAS.Add(MyETA);

      //Evaluacion
      MyETA= new ETAPA();
      MyETA.c_etapa="02";
      MyETA.d_etapa="Evaluar";
      MyETA.c_estado_evento="H";
      MyETA.c_estado_eval="HA";
      MyETA.l_habilitado=true;
      MyETA.f_inicio=this.f_desde;
      MyETA.c_rol_habilitado="E";
      MyETA.c_estado_aceptarNull=true;
      this.ETAPAS.Add(MyETA);

      //Autoevaluacion
      MyETA= new ETAPA();
      MyETA.c_etapa="03";
      MyETA.d_etapa="Autoevaluacion";
      MyETA.c_estado_evento="H";
      MyETA.c_estado_eval="HA";
      MyETA.l_habilitado=MyTIPO.l_eva2;
      MyETA.f_inicio=this.f_desde;
      MyETA.c_rol_habilitado="O";
      MyETA.c_estado_aceptarNull=true;
      this.ETAPAS.Add(MyETA);

      //Supervisacion
      MyETA= new ETAPA();
      MyETA.c_etapa="04";
      MyETA.d_etapa="Supervisor";
      MyETA.c_estado_evento="H";
      MyETA.c_estado_eval="HA";
      MyETA.l_habilitado=MyTIPO.l_eva3;
      MyETA.f_inicio=this.f_desde;
      MyETA.c_rol_habilitado="S";
      MyETA.c_estado_aceptarNull=true;
      this.ETAPAS.Add(MyETA);

      //Evaluacion 360
      MyETA= new ETAPA();
      MyETA.c_etapa="05";
      MyETA.d_etapa="Evaluacion 360";
      MyETA.c_estado_evento="H";
      MyETA.c_estado_eval="HA";
      MyETA.l_habilitado=MyTIPO.l_eva360;
      MyETA.f_inicio=this.f_desde;
      MyETA.c_rol_habilitado="F";
      MyETA.c_estado_aceptarNull=true;
      this.ETAPAS.Add(MyETA);

      //Confirmacion
      MyETA= new ETAPA();
      MyETA.c_etapa="06";
      MyETA.d_etapa="Confirmar Evaluacion";
      MyETA.c_estado_evento="H";
      MyETA.c_estado_eval="EV";
      MyETA.l_habilitado=true;
      MyETA.f_inicio=this.f_desde;
      MyETA.c_rol_habilitado="E";
      MyETA.c_estado_aceptarNull=true;
      this.ETAPAS.Add(MyETA);

      //Aprobacion/Rechazo
      MyETA= new ETAPA();
      MyETA.c_etapa="07";
      MyETA.d_etapa="Aprobar Evaluacion";
      MyETA.c_estado_evento="H";
      MyETA.c_estado_eval="CO";
      MyETA.l_habilitado=true;
      MyETA.f_inicio=this.f_desde;
      MyETA.c_rol_habilitado="S";
      MyETA.c_estado_aceptar="AR";
      this.ETAPAS.Add(MyETA);

      //Comentarios Finales
      MyETA= new ETAPA();
      MyETA.c_etapa="09";
      MyETA.d_etapa="Comentarios Finales";
      MyETA.c_estado_evento="H";
      MyETA.c_estado_eval="AP";
      MyETA.l_habilitado=true;
      MyETA.f_inicio=this.f_desde;
      MyETA.c_rol_habilitado="E";
      MyETA.c_estado_aceptarNull=true;
      this.ETAPAS.Add(MyETA);

      //Especial Rechazo
      MyETA= new ETAPA();
      MyETA.c_etapa="08";
      MyETA.d_etapa="Evaluacion Rechazada";
      MyETA.c_estado_evento="H";
      MyETA.c_estado_eval="RZ";
      MyETA.l_habilitado=true;
      MyETA.f_inicio=this.f_desde;
      MyETA.c_rol_habilitado="E";
      MyETA.c_estado_aceptarNull=true;
      this.ETAPAS.Add(MyETA);
    }

    public static void SAVE(EVENTO DDO)
    {
      if (DDO.IsForInsert)
      {
         string oi_clase_org = DDO.oi_clase_org;
        DDO.RefreshPlantilla();
        DDO.RefreshEtapas();

        DDO.oi_clase_org = oi_clase_org;
      }

      NomadEnvironment.GetCurrentTransaction().Save(DDO);
    }

  public static void AdministrarLegajos(Nomad.NSystem.Document.NmdXmlDocument param, string evento)
  {
    NucleusRH.Base.Evaluacion2.Evaluacion.EVALUACION ddoPER;
    System.Text.StringBuilder IDS = new System.Text.StringBuilder();
    NomadBatch MyBATCH = NomadBatch.GetBatch("Administrar Legajos", "Administrar Legajos");
    MyBATCH.Log("Comienza el proceso");

    //ARMO UN "HASHTABLE" CON TODAS LAS PERSONAS QUE ESTAN INICIALIZADAS
    MyBATCH.SetMess("Buscando Legajos...");
    Hashtable ht = new Hashtable();
    ht = NomadEnvironment.QueryHashtable(NucleusRH.Base.Evaluacion2.Eventos.EVENTO.Resources.QRY_ADMINISTRAR, "<DATO oi_evento=\"" + evento + "\"/>", "oi_personal_emp", false);
    MyBATCH.SetPro(10);

    NucleusRH.Base.Evaluacion2.Eventos.EVENTO ddoEvento = NucleusRH.Base.Evaluacion2.Eventos.EVENTO.Get(evento);
    NucleusRH.Base.Evaluacion2.Tipos_Evaluacion.TIPO_EVAL typEVA = ddoEvento.Getoi_tipo_eval();
    if (ddoEvento.c_estado != "A" && ddoEvento.c_estado != "I" && ddoEvento.c_estado != "H")
    {
      MyBATCH.Err("No se puede Administrar los Legajos de la Evaluacion.");
      return;
    }

    //GUARDO EL XML QUE ENTRA EN UN NOMADXML
    NomadXML xmlROW, xmlVALUE;
    NomadXML xmldoc = new NomadXML(param.ToString());
    xmldoc = xmldoc.FirstChild();
    int count = 0, saves = 0;

    //Inicio la Tranasaccion
    //NomadEnvironment.GetCurrentTransaction().Begin();

    //RECORRO LA HASH Y PREGUNTO SI EN ELLA HAY IDS QUE NO ESTAN EN LOS ID QUE ENTRAN
    //DE SER ASI, ESTOS IDS HAY QUE ELIMINARLOS
    MyBATCH.SetMess("Eliminando Legajos...");
    foreach (string value in ht.Keys)
    {
      xmlVALUE = ((NomadXML)ht[value]);
      xmlROW = xmldoc.FindElement2("ROW", "id", value);
      if (xmlROW == null)
      {
        //Verifico si esta Cerrado.
        if (xmlVALUE.GetAttr("c_estado") != "AB" && xmlVALUE.GetAttr("c_estado") != "HA" && xmlVALUE.GetAttr("c_estado") != "AN")
        {
          count++;
          MyBATCH.Wrn("No se elimina la evaluación de " + xmlVALUE.GetAttr("persona") + " porque está no esta ANULADA.");
          continue;
        }

        //Elimino
        ddoPER = NucleusRH.Base.Evaluacion2.Evaluacion.EVALUACION.Get(xmlVALUE.GetAttr("oi_evaluacion"));
        NomadEnvironment.GetCurrentTransaction().Delete(ddoPER);
        MyBATCH.Log("Eliminando evaluación de " + xmlVALUE.GetAttr("persona"));
      }
    }
    MyBATCH.SetPro(30);

    MyBATCH.SetMess("Agregando Legajos...");
    for (xmlROW = xmldoc.FirstChild(); xmlROW != null; xmlROW = xmlROW.Next())
    {
      //Verifico si existe la Evaluacion
      if (ht.ContainsKey(xmlROW.GetAttr("id")))
      {
        count++;
        continue;
      }

      //Obtener los Datos del Evaluador y el Supervisor.
      NomadXML MyPP = new NomadXML(NomadProxy.GetProxy().SQLService().Get(NucleusRH.Base.Evaluacion2.Eventos.EVENTO.Resources.QRY_EVALUADOR, "<DATO oi_clase_org=\"" + ddoEvento.oi_clase_org + "\"  oi_personal_emp=\"" + xmlROW.GetAttr("id") + "\" />"));
      MyPP = MyPP.FirstChild();

      NomadLog.Info("MyPP:" + MyPP.ToString());
      if (ddoEvento.oi_clase_orgNull && MyPP.GetAttr("oi_posicion") == "")
      {
        MyBATCH.Err("No se puede agregar la evaluación de " + MyPP.GetAttr("d_ape_y_nom") + " porque no tiene POSICION.");
        continue;
      }

      if (MyPP.GetAttr("oi_evaluador") == "")
      {
        MyBATCH.Wrn("No se puede agregar la evaluación de " + MyPP.GetAttr("d_ape_y_nom") + " porque no se encontro un evaluador.");
        continue;
      }

      if (ddoEvento.oi_clase_orgNull && MyPP.GetAttr("oi_pos_evaluador") == "")
      {
        MyBATCH.Err("No se puede agregar la evaluación de " + MyPP.GetAttr("d_ape_y_nom") + " porque su Evaluador no tiene POSICION.");
        continue;
      }

      //Agrego el Nuevo.
      MyBATCH.Log("Agregando la evaluación de " + MyPP.GetAttr("d_ape_y_nom"));

      //Creo la EVALUACION.
      ddoPER = new NucleusRH.Base.Evaluacion2.Evaluacion.EVALUACION();
      ddoPER.oi_evento = evento;
      ddoPER.oi_personal_emp = MyPP.GetAttr("oi_personal_emp");
      ddoPER.oi_pos_evaluado = MyPP.GetAttr("oi_posicion"); if (MyPP.GetAttr("oi_posicion") == "") ddoPER.oi_pos_evaluadoNull = true;
      ddoPER.oi_uni_evaluado = MyPP.GetAttr("oi_unidad");

      ddoPER.oi_evaluador = MyPP.GetAttr("oi_evaluador");
      ddoPER.oi_pos_evaluador = MyPP.GetAttr("oi_pos_evaluador"); if (MyPP.GetAttr("oi_pos_evaluador") == "") ddoPER.oi_pos_evaluadorNull = true;
      ddoPER.oi_uni_evaluador = MyPP.GetAttr("oi_uni_evaluador");

      if (MyPP.GetAttr("oi_supervisor") == "")
      {
        ddoPER.oi_supervisorNull = true;
        ddoPER.oi_pos_supervisorNull = true;
        ddoPER.oi_uni_supervisorNull = true;
      }
      else
      {
        ddoPER.oi_supervisor = MyPP.GetAttr("oi_supervisor");
        ddoPER.oi_pos_supervisor = MyPP.GetAttr("oi_pos_supervisor"); if (MyPP.GetAttr("oi_pos_supervisor") == "") ddoPER.oi_pos_supervisorNull = true;
        ddoPER.oi_uni_supervisor = MyPP.GetAttr("oi_uni_supervisor");
      }
      //SETEO EL ESTADO SEGUN EL EVENTO Y TIPO DE EVALUACION
      if (ddoEvento.c_estado == "H" && !typEVA.l_pree)
        ddoPER.CambiarEstado("HA");
      else
        ddoPER.CambiarEstado("AB");

     
      if (MyPP.GetAttr("oi_supervisor") == "")
      {
        ddoPER.oi_supervisorNull = true;
        ddoPER.oi_pos_supervisorNull = true;
      }

      foreach (NucleusRH.Base.Evaluacion2.Eventos.PLANTILLA myPLA in ddoEvento.PLANTILLAS)
      {
        NucleusRH.Base.Evaluacion2.Evaluacion.PLANTILLA ddoPLA = ArmarPlantilla(myPLA, typEVA, ddoPER.oi_personal_emp, ddoPER.oi_pos_evaluado);
        ddoPER.PLANTILLAS.Add(ddoPLA);
      }

      // Actualiza evaluadores 360
      ddoPER.ActualizarEvaluadores();

      NomadEnvironment.GetCurrentTransaction().SaveRefresh(ddoPER);
      if (IDS.Length != 0) IDS.Append(',');
      IDS.Append(ddoPER.id);

      saves++;
      count++;
    }
    MyBATCH.SetPro(70);

    // Setea el estado del evento
    MyBATCH.SetMess("Actualizando el evento evaluativo...");
    // En caso que el evento este habilitado y no se preevalua, no se debe deshabilitar
    if (ddoEvento.c_estado != "H" || typEVA.l_pree)
    {
      if (count == 0)
        ddoEvento.c_estado = "A";
      else if (saves > 0)
      {
          ddoEvento.c_estado = "I";
          NucleusRH.Base.Configuracion.LogEventos.LOGEVENTO.Call("NucleusRH.Base.Evaluacion2.Evaluaciones.chg_status", IDS.ToString(), "AB", "Cambio a estado " + "AB"); 
      }
       
    }
    NomadEnvironment.GetCurrentTransaction().SaveRefresh(ddoEvento);
    MyBATCH.SetPro(80);

    MyBATCH.Log("Total de Evaluaciones: " + count.ToString());

    MyBATCH.SetMess("Actualizando la Base de Datos...");
   // NomadEnvironment.GetCurrentTransaction().Commit();

    /* if (ddoEvento.c_estado != "H" || typEVA.l_pree)
    {
       if (saves > 0)
        CambiarEstado(ddoEvento.id.ToString(),"I", "AB");
    }*/

  }

    public static NucleusRH.Base.Evaluacion2.Evaluacion.PLANTILLA ArmarPlantilla(NucleusRH.Base.Evaluacion2.Eventos.PLANTILLA myPLA, NucleusRH.Base.Evaluacion2.Tipos_Evaluacion.TIPO_EVAL typEVA, string oi_personal_emp, string oi_pos_evaluado)
    {
      NucleusRH.Base.Evaluacion2.Evaluacion.PLANTILLA ddoPLA = new NucleusRH.Base.Evaluacion2.Evaluacion.PLANTILLA();
      ddoPLA.n_ponderacion = myPLA.n_ponderacion;
      ddoPLA.oi_plantilla_eve = myPLA.Id;

      NucleusRH.Base.Evaluacion2.Tipos_Evaluacion.PLANTILLA typPLA = (NucleusRH.Base.Evaluacion2.Tipos_Evaluacion.PLANTILLA)typEVA.PLANTILLAS.GetById(myPLA.oi_plantilla_def);
      if (typPLA.c_tipo_plantilla.StartsWith("QRY_"))
      {
        NomadXML QF = NomadProxy.GetProxy().FileServiceIO().LoadFileXML("EVALUACIONES", typPLA.c_tipo_plantilla + ".TYPE_PLA.XML");
        string QUERY = "";
        for (NomadXML cur = QF.FirstChild(); cur != null; cur = cur.Next())
          QUERY += cur.ToString();

        NomadXML PARAM = new NomadXML("DATA");
        PARAM.SetAttr("oi_personal_emp", oi_personal_emp);
        PARAM.SetAttr("oi_posicion", oi_pos_evaluado);

        NomadXML RS = new NomadXML(NomadProxy.GetProxy().SQLService().Get(QUERY, PARAM.ToString()));
        RS = RS.FirstChild();

        //Plantilla
        ddoPLA.d_cus1 = RS.GetAttr("CUS1");
        ddoPLA.d_cus2 = RS.GetAttr("CUS2");
        ddoPLA.d_cus3 = RS.GetAttr("CUS3");
        ddoPLA.d_cus4 = RS.GetAttr("CUS4");
        ddoPLA.d_cus5 = RS.GetAttr("CUS5");
        if (RS.GetAttr("PON") != "") ddoPLA.n_ponderacion = RS.GetAttrDouble("PON");

        //OBJETIVOS
        for (NomadXML item = RS.FirstChild(); item != null; item = item.Next())
        {
          NucleusRH.Base.Evaluacion2.Evaluacion.OBJETIVO ddoOBJ = new NucleusRH.Base.Evaluacion2.Evaluacion.OBJETIVO();
          ddoOBJ.c_objetivo = item.GetAttr("COD");
          ddoOBJ.d_objetivo = item.GetAttr("DES");
          ddoOBJ.o_objetivo = item.GetAttr("OBS");
          ddoOBJ.oi_competencia = item.GetAttr("oi_competencia");
          ddoOBJ.oi_escala = item.GetAttr("ESC");
          ddoOBJ.n_ponderacion = item.GetAttrDouble("PON");
          ddoOBJ.d_cus1 = item.GetAttr("CUS1");
          ddoOBJ.d_cus2 = item.GetAttr("CUS2");
          ddoOBJ.d_cus3 = item.GetAttr("CUS3");
          ddoOBJ.d_cus4 = item.GetAttr("CUS4");
          ddoOBJ.d_cus5 = item.GetAttr("CUS5");

          ddoOBJ.oi_competenciaNull = (item.GetAttr("oi_competencia") == "" ? true : false);
          ddoOBJ.oi_escalaNull = (item.GetAttr("ESC") == "" ? true : false);
          ddoOBJ.n_ponderacionNull = (item.GetAttr("PON") == "" ? true : false);

          ddoPLA.OBJETIVOS.Add(ddoOBJ);

          //SUBOBJETTIVOS
          for (NomadXML subitem = item.FirstChild(); subitem != null; subitem = subitem.Next())
          {
            NucleusRH.Base.Evaluacion2.Evaluacion.SUBOBJETIVO ddoSUB = new NucleusRH.Base.Evaluacion2.Evaluacion.SUBOBJETIVO();
            ddoSUB.c_subobjetivo = subitem.GetAttr("COD");
            ddoSUB.d_subobjetivo = subitem.GetAttr("DES");
            ddoSUB.o_subobjetivo = subitem.GetAttr("OBS");
            ddoSUB.oi_aptit_comp = subitem.GetAttr("oi_aptit_comp");
            ddoSUB.oi_escala = subitem.GetAttr("ESC");
            ddoSUB.n_ponderacion = subitem.GetAttrDouble("PON");

            ddoSUB.oi_aptit_compNull = (subitem.GetAttr("oi_aptit_comp") == "" ? true : false);
            ddoSUB.oi_escalaNull = (subitem.GetAttr("ESC") == "" ? true : false);

            ddoOBJ.SUBOBJETIVOS.Add(ddoSUB);
          }
        }

      }
      else
      {

        //OBJETIVOS
        foreach (NucleusRH.Base.Evaluacion2.Tipos_Evaluacion.OBJETIVO MyOBJ in typPLA.OBJETIVOS)
        {
          NucleusRH.Base.Evaluacion2.Evaluacion.OBJETIVO ddoOBJ = new NucleusRH.Base.Evaluacion2.Evaluacion.OBJETIVO();
          ddoOBJ.c_objetivo = MyOBJ.c_objetivo;
          ddoOBJ.d_objetivo = MyOBJ.d_objetivo;
          ddoOBJ.o_objetivo = MyOBJ.o_objetivo;
          ddoOBJ.oi_competencia = MyOBJ.oi_competencia;
          ddoOBJ.oi_escala = MyOBJ.oi_escala;
          ddoOBJ.n_ponderacion = MyOBJ.n_ponderacion;

          ddoOBJ.oi_competenciaNull = MyOBJ.oi_competenciaNull;
          ddoOBJ.oi_escalaNull = MyOBJ.oi_escalaNull;
          ddoOBJ.n_ponderacionNull = MyOBJ.n_ponderacionNull;

          ddoPLA.OBJETIVOS.Add(ddoOBJ);

          //SUBOBJETTIVOS
          foreach (NucleusRH.Base.Evaluacion2.Tipos_Evaluacion.SUBOBJETIVO MySUB in MyOBJ.SUBOBJETIVOS)
          {
            NucleusRH.Base.Evaluacion2.Evaluacion.SUBOBJETIVO ddoSUB = new NucleusRH.Base.Evaluacion2.Evaluacion.SUBOBJETIVO();
            ddoSUB.c_subobjetivo = MySUB.c_subobjetivo;
            ddoSUB.d_subobjetivo = MySUB.d_subobjetivo;
            ddoSUB.o_subobjetivo = MySUB.o_subobjetivo;
            ddoSUB.oi_aptit_comp = MySUB.oi_aptit_comp;
            ddoSUB.oi_escala = MySUB.oi_escala;
            ddoSUB.n_ponderacion = MySUB.n_ponderacion;

            ddoSUB.oi_aptit_compNull = MySUB.oi_aptit_compNull;
            ddoSUB.oi_escalaNull = MySUB.oi_escalaNull;

            ddoOBJ.SUBOBJETIVOS.Add(ddoSUB);
          }

        }
      }
      return ddoPLA;
    }

    public static void CambiarEstado(string evento, string estado_eve, string estado_eva)
    {
            System.Text.StringBuilder IDS = new System.Text.StringBuilder();
      NucleusRH.Base.Evaluacion2.Evaluacion.EVALUACION ddoPER;
      NomadBatch MyBATCH;

      switch (estado_eve)
      {
        case "A":
          MyBATCH = NomadBatch.GetBatch("Dehabilitar Evaluaciones", "Dehabilitar Evaluaciones");
          break;

        case "H":
          MyBATCH = NomadBatch.GetBatch("Habilitar Evaluaciones", "Habilitar Evaluaciones");
          break;

        case "C":
          MyBATCH = NomadBatch.GetBatch("Cerrar Evaluaciones", "Cerrar Evaluaciones");
          break;

        default:
          MyBATCH = NomadBatch.GetBatch("Cambiar Estado", "Cambiar Estado");
          break;
      }
      MyBATCH.Log("Comienza el proceso");

      //Inicio la Tranasaccion
      NomadEnvironment.GetCurrentTransaction().Begin();
      NucleusRH.Base.Evaluacion2.Eventos.EVENTO ddoEvento = NucleusRH.Base.Evaluacion2.Eventos.EVENTO.Get(evento, false);

      if (estado_eva != "-")
      {
        //ARMO UN "HASHTABLE" CON TODAS LAS PERSONAS QUE ESTAN INICIALIZADAS
        MyBATCH.SetMess("Buscando Legajos...");
        Hashtable ht = new Hashtable();
        ht = NomadEnvironment.QueryHashtable(NucleusRH.Base.Evaluacion2.Eventos.EVENTO.Resources.QRY_ADMINISTRAR, "<DATO oi_evento=\"" + evento + "\"/>", "oi_evaluacion", false);
        MyBATCH.SetPro(10);

        int paso = 0;
        MyBATCH.SetMess("Cambiando el estado a las Evaluaciones...");
        foreach (string docId in ht.Keys)
        {
          paso++;
          MyBATCH.SetMess("Cambiando estado Evaluación " + paso + "/" + ht.Count + "...");
          MyBATCH.SetPro(10, 70, ht.Count, paso);

          ddoPER = NucleusRH.Base.Evaluacion2.Evaluacion.EVALUACION.Get(docId, false);
          if (estado_eve == "H" && ddoPER.c_estado == "AB")
          {
                        if (IDS.Length != 0) IDS.Append(',');
                        IDS.Append(docId);
                        ddoPER.CambiarEstado(estado_eva);
            NomadEnvironment.GetCurrentTransaction().Save(ddoPER);
          }
          else if (estado_eve == "I" && (ddoPER.c_estado == "HA" || ddoPER.c_estado == "EV"))
          {
                        if (IDS.Length != 0) IDS.Append(',');
                        IDS.Append(docId);
                        ddoPER.CambiarEstado(estado_eva);
            NomadEnvironment.GetCurrentTransaction().Save(ddoPER);
          }
        }
        MyBATCH.SetPro(70);
      }

      // Setea el estado del evento
      MyBATCH.SetMess("Actualizando el evento evaluativo...");
      ddoEvento.c_estado = estado_eve;
      NomadEnvironment.GetCurrentTransaction().Save(ddoEvento);
      MyBATCH.SetPro(80);

      MyBATCH.SetMess("Actualizando la Base de Datos...");
      NomadEnvironment.GetCurrentTransaction().Commit();

            //Llamada al evento
            NucleusRH.Base.Configuracion.LogEventos.LOGEVENTO.Call("NucleusRH.Base.Evaluacion2.Evaluaciones.chg_status", IDS.ToString(), estado_eva, "Cambio a estado "+ estado_eva);
    }

    public static void EliminarEvaluaciones(string evento, string estado_eva)
    {
      NucleusRH.Base.Evaluacion2.Evaluacion.EVALUACION ddoPER;
      NomadBatch MyBATCH = NomadBatch.GetBatch("Eliminar Evaluaciones", "Eliminar Evaluaciones");
      MyBATCH.Log("Comienza el proceso");

      //ARMO UN "HASHTABLE" CON TODAS LAS PERSONAS QUE ESTAN INICIALIZADAS
      MyBATCH.SetMess("Buscando Legajos...");
      Hashtable ht = new Hashtable();
      ht = NomadEnvironment.QueryHashtable(NucleusRH.Base.Evaluacion2.Eventos.EVENTO.Resources.QRY_ADMINISTRAR, "<DATO oi_evento=\"" + evento + "\" c_estado=\"" + estado_eva + "\" />", "oi_evaluacion", false);
      MyBATCH.SetPro(10);

      //Inicio la Tranasaccion
      NomadEnvironment.GetCurrentTransaction().Begin();
      NucleusRH.Base.Evaluacion2.Eventos.EVENTO ddoEvento = NucleusRH.Base.Evaluacion2.Eventos.EVENTO.Get(evento);

      int paso = 0;
      MyBATCH.SetMess("Eliminando las Evaluaciones...");
      foreach (string docId in ht.Keys)
      {
        paso++;
        MyBATCH.SetMess("Eliminando estado Evaluación " + paso + "/" + ht.Count + "...");
        MyBATCH.SetPro(10, 90, ht.Count, paso);

        ddoPER = NucleusRH.Base.Evaluacion2.Evaluacion.EVALUACION.Get(docId);
        NomadEnvironment.GetCurrentTransaction().Delete(ddoPER);
      }
      MyBATCH.SetPro(90);

      // Setea el estado del evento
      MyBATCH.SetMess("Actualizando la Base de Datos...");
      NomadEnvironment.GetCurrentTransaction().Commit();
    }

    public static void CerrarEvaluaciones(string evento, string estado_eva)
    {
      NucleusRH.Base.Evaluacion2.Evaluacion.EVALUACION ddoPER;
      NomadBatch MyBATCH = NomadBatch.GetBatch("Eliminar Evaluaciones", "Eliminar Evaluaciones");
      MyBATCH.Log("Comienza el proceso");

      //ARMO UN "HASHTABLE" CON TODAS LAS PERSONAS QUE ESTAN INICIALIZADAS
      MyBATCH.SetMess("Buscando Legajos...");
      Hashtable ht = new Hashtable();
      ht = NomadEnvironment.QueryHashtable(NucleusRH.Base.Evaluacion2.Eventos.EVENTO.Resources.QRY_ADMINISTRAR, "<DATO oi_evento=\"" + evento + "\" c_estado=\"" + estado_eva + "\" />", "oi_evaluacion", false);
      MyBATCH.SetPro(10);

      //Inicio la Tranasaccion
      NomadEnvironment.GetCurrentTransaction().Begin();

      int paso = 0;
      MyBATCH.SetMess("Eliminando las Evaluaciones...");
      foreach (string docId in ht.Keys)
      {
        paso++;
        MyBATCH.SetMess("Eliminando estado Evaluación " + paso + "/" + ht.Count + "...");
        MyBATCH.SetPro(10, 90, ht.Count, paso);

        ddoPER = NucleusRH.Base.Evaluacion2.Evaluacion.EVALUACION.Get(docId, false);
        ddoPER.CambiarEstado("CE");
        NomadEnvironment.GetCurrentTransaction().Save(ddoPER);
      }
      MyBATCH.SetPro(90);

      // Setea el estado del evento
      MyBATCH.SetMess("Actualizando la Base de Datos...");
      NomadEnvironment.GetCurrentTransaction().Commit();
    }
  }
}



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
using System.Text;

namespace NucleusProject.Base.Project.Proyectos 
{

    public partial class PROYECTO
  {
     
    public void CambiarEstado(string nuevoEstado, NomadXML nodoXML, string oi_recurso, Hashtable MySOL)
    {
      NomadLog.Info("Cambiar Estado: PROY "+this.id+" - "+this.c_estado+"->"+nuevoEstado);

      if (this.c_estado==nuevoEstado) return;
      if (this.c_estado=="A") return;
      if (this.c_estado=="F") return;

      //Recorro todas las tareas
      for (NomadXML cur=nodoXML.FirstChild(); cur!=null; cur=cur.Next())
      {
        switch(nuevoEstado)
        {
          case "L":
            if (cur.Name=="TAREA")
            {
              NucleusProject.Base.Project.Tareas.TAREA tar=NucleusProject.Base.Project.Tareas.TAREA.Get(cur.GetAttr("id"));
              NomadLog.Info("Cambiar Estado: TAREA "+tar.id+" - "+tar.c_estado+"->"+nuevoEstado);
              tar.c_estado=nuevoEstado;
              NomadLog.Info("L "+tar.id+" - "+tar.c_estado+"->"+nuevoEstado);
              NomadEnvironment.GetCurrentTransaction().Save(tar);
            }
          break;

          case "A":
            if (cur.Name=="TAREA")
            {
              NucleusProject.Base.Project.Tareas.TAREA tar=NucleusProject.Base.Project.Tareas.TAREA.Get(cur.GetAttr("id"));
              if (tar.c_estado=="I")
              {
                NomadLog.Info("Eliminar: TAREA "+tar.id+" - "+tar.c_estado);
                NomadLog.Info("Eliminar I estado A "+tar.id+" - "+tar.c_estado+"->"+nuevoEstado);
                NomadEnvironment.GetCurrentTransaction().Delete(tar);
              } else
              {
                NomadLog.Info("Cambiar Estado: TAREA "+tar.id+" - "+tar.c_estado+"->"+nuevoEstado);

                NucleusProject.Base.Project.Tareas.TAREA_TRAZA myTrace=new NucleusProject.Base.Project.Tareas.TAREA_TRAZA();
                myTrace.oi_recurso=oi_recurso;
                myTrace.oi_recurso_asi=oi_recurso;
                myTrace.f_fecha_tarea=DateTime.Now;
                myTrace.c_causa="Anulacion Masiva";
                tar.TAREAS_TRAZA.Add(myTrace);
                tar.f_fin=DateTime.Now;
                tar.c_estado=nuevoEstado;
                NomadLog.Info("A distinto I "+tar.id+" - "+tar.c_estado+"->"+nuevoEstado);
                NomadEnvironment.GetCurrentTransaction().Save(tar);

                //Agrego la solicitud a verificar
                if (!tar.oi_solicitudNull)
                  MySOL[int.Parse(tar.oi_solicitud)]=1;
              }
            }
          break;

          case "F":
            if (cur.Name=="TAREA")
            {
              NucleusProject.Base.Project.Tareas.TAREA tar=NucleusProject.Base.Project.Tareas.TAREA.Get(cur.GetAttr("id"));
              if (tar.c_estado=="I")
              {
                NomadLog.Info("Eliminar: TAREA "+tar.id+" - "+tar.c_estado);
                NomadLog.Info("Eliminar I estado F "+tar.id+" - "+tar.c_estado+"->"+nuevoEstado);
                NomadEnvironment.GetCurrentTransaction().Delete(tar);
              } else if (tar.c_estado!="F" && tar.c_estado!="A")
              {
                NomadLog.Info("Cambiar Estado: TAREA "+tar.id+" - "+tar.c_estado+"->"+nuevoEstado);
                NucleusProject.Base.Project.Tareas.TAREA_TRAZA myTrace=new NucleusProject.Base.Project.Tareas.TAREA_TRAZA();
                myTrace.oi_recurso=oi_recurso;
                myTrace.oi_recurso_asi=oi_recurso;
                myTrace.f_fecha_tarea=DateTime.Now;
                myTrace.c_causa="Finalizacion Masiva";
                tar.TAREAS_TRAZA.Add(myTrace);
                tar.f_fin=DateTime.Now;
                tar.c_estado=nuevoEstado;
                tar.o_motivo="Cerrado automáticamente por Finalización del Proyecto";
                NomadLog.Info("F distinto A "+tar.id+" - "+tar.c_estado+"->"+nuevoEstado);
                NomadEnvironment.GetCurrentTransaction().Save(tar);

                //Agrego la solicitud a verificar
                if (!tar.oi_solicitudNull)
                  MySOL[int.Parse(tar.oi_solicitud)]=1;
              }
            }
          break;
        }

      }

      //Recorro todos los proyectos
      for (NomadXML cur=nodoXML.FirstChild(); cur!=null; cur=cur.Next())
      {
        if (cur.Name=="PROY")
          NucleusProject.Base.Project.Proyectos.PROYECTO.Get(cur.GetAttr("id")).CambiarEstado(nuevoEstado,cur,oi_recurso,MySOL);
      }

      //Guardo el DDO
      if ((nuevoEstado=="A") || (nuevoEstado=="F")) this.f_fin=DateTime.Now;
      this.c_estado=nuevoEstado;
      NomadEnvironment.GetCurrentTransaction().SaveRefresh(this);
    }

    public void CambiarEstadoMails(string nuevoEstado, Nomad.NSystem.Proxy.NomadXML mails)
    {
        //Define variables para envio de mails
        string mailsDestino="";
        mails = mails.FirstChild();
        string mail_contacto_cli="";
        string c_cliente="";
        string d_nom_cliente="";
        string nom_y_ape_lider_cli="";
        string asunto="Area Soporte - Proyecto: [" + this.c_proyecto + "] " + this.d_proyecto;
        
        //Si tiene un cliente asociado agrega el mismo al asunto del correo y deja guardado el mail de contacto del cliente
        if (!this.oi_clienteNull && this.oi_cliente !="0" && this.oi_cliente != "")
        {
            NucleusProject.Base.Project.Clientes.CLIENTE ddoCliente = NucleusProject.Base.Project.Clientes.CLIENTE.Get(this.oi_cliente);
            c_cliente = ddoCliente.c_cliente;
            d_nom_cliente = ddoCliente.d_nom_cliente;
            mail_contacto_cli = ddoCliente.d_email;
            asunto = "Area Soporte - Finalización del Proyecto [" + this.c_proyecto + "] " + this.d_proyecto + " - Cliente: [" + c_cliente + "] " + d_nom_cliente;
        }

        //Si tiene lider del proyecto cliente carga el nombre y apellido del mismo para informar en el mail
        if (!this.oi_lider_proy_cliNull && this.oi_lider_proy_cli != "0" && this.oi_lider_proy_cli != "")
        {
            NucleusProject.Base.Project.Clientes.CONTACTO liderProyCli = NucleusProject.Base.Project.Clientes.CONTACTO.Get(this.oi_lider_proy_cli);
            nom_y_ape_lider_cli = liderProyCli.d_nom_y_ape;
        }

        //Busca el mail del Lider de Proyecto, si lo eligió como destinatario
        if (mails.GetAttrInt("l_lider_proy") == 1)
        {
            if (!this.oi_recursoNull && this.oi_recurso!="0" && this.oi_recurso!="")
            {
                NucleusProject.Base.Project.Recursos.RECURSO lider_proy = NucleusProject.Base.Project.Recursos.RECURSO.Get(this.oi_recurso);
                mailsDestino = mailsDestino + lider_proy.d_email + ";";
            }
            else
                throw new NomadAppException("No es posible enviar mail al Lider de Proyecto dado que no esta ingresado en el Proyecto.");
        }

        //Busca el mail del Lider de Proyecto Cliente, si lo eligió como destinatario
        if (mails.GetAttrInt("l_lider_proy_cli")==1)
        {
            if (!this.oi_lider_proy_cliNull && this.oi_lider_proy_cli!="0" && this.oi_lider_proy_cli!="")
            {
                NucleusProject.Base.Project.Clientes.CONTACTO lider_proy_cli = NucleusProject.Base.Project.Clientes.CONTACTO.Get(this.oi_lider_proy_cli);
                mailsDestino = mailsDestino + lider_proy_cli.d_email + ";";
            }
            else
                throw new NomadAppException("No es posible enviar mail al Lider de Proyecto Cliente dado que no esta ingresado en el Proyecto.");
        }

        //Busca el mail del Responsable de Calidad, si lo eligió como destinatario
        if (mails.GetAttrInt("l_respon_calidad") == 1)
        {
            if (!this.oi_respon_calidadNull && oi_respon_calidad!="0" && oi_respon_calidad!="")
            {
                NucleusProject.Base.Project.Recursos.RECURSO responsable_calidad = NucleusProject.Base.Project.Recursos.RECURSO.Get(this.oi_respon_calidad);
                mailsDestino = mailsDestino + responsable_calidad.d_email + ";";
            }
            else
                throw new NomadAppException("No es posible enviar mail al Responsable de Calidad dado que no esta ingresado en el Proyecto.");
        }

        //Busca el mail del Contacto del Cliente, si lo eligió como destinatario
        if (mails.GetAttrInt("l_contacto_cli") == 1)
        {
            if (mail_contacto_cli != "")
                mailsDestino = mailsDestino + mail_contacto_cli + ";";
            else
            {
                if (!this.oi_clienteNull)
                    throw new NomadAppException("No es posible enviar mail al Contacto del Cliente dado que el Cliente no posee ningún mail de contacto.");
                else
                    throw new NomadAppException("No es posible enviar mail al Contacto del Cliente dado que el Proyecto no tiene ningún Cliente.");
            }
        }

        //Cambia de Estado
        CambiarEstado(nuevoEstado);

        try
        {
            //Carga las fechas de inicio y Fin
            string fechaInicio = this.f_inicio == DateTime.MinValue ? "No Especificada" : this.f_inicio.ToString("dd/MM/yyyy");
            string fechaFin = this.f_fin == DateTime.MinValue ? "" : this.f_fin.ToString("dd/MM/yyyy");

            //Cuerpo del Mail
            string cuerpo = "**Proyecto: **[" + this.c_proyecto + "] " + this.d_proyecto + @"\\ " + "\n\n\n\n"
                          + "**Cliente: **[" + c_cliente + "] " + d_nom_cliente + @"\\ " + "\n\n\n\n"
                          + "**Fecha Inicio: **" + fechaInicio + @"\\ " + "\n\n\n\n"
                          + "**Fecha Fin: **" + fechaFin + @"\\ " + "\n\n\n\n"
                          + "**Descripción del Proyecto: **" + this.d_proyecto + @"\\ " + "\n\n\n\n"
                          + "**Líder del Cliente: **" + nom_y_ape_lider_cli + @"\\ " + "\n\n\n\n"
                          + "**Comunicación de cierre: **El proyecto se pasa a estado finalizado en el día de la fecha por " + this.o_motivo + "\n\n\n\n"
                          + @"========Mail Original========" + @"\\ ";

            //Si seleccionó por lo menos uno de los responsables para enviar mails lo envia utilizando el mismo método que en las solicitudes
            if (mailsDestino != "")
            {
                NucleusProject.Base.Project.Solicitudes.SOLICITUD solicitud = new NucleusProject.Base.Project.Solicitudes.SOLICITUD();
                solicitud.send_mail_solicitante(mailsDestino, asunto, cuerpo);
            }

        }
        catch (Exception e)
        {
            throw new NomadMessage("NucleusProject.PROYECTO.FINALIZAR-MAIL-ERROR");
        }

    }

    public void CambiarEstado(string nuevoEstado)
    {
      NomadXML MyPROY=NomadEnvironment.QueryNomadXML(NucleusProject.Base.Project.Proyectos.PROYECTO.Resources.qry_proyectos_child,"<DATOS id=\""+this.id+"\"/>");
      NomadXML MyREC =NomadEnvironment.QueryNomadXML(NucleusProject.Base.Project.Proyectos.PROYECTO.Resources.FindResource       ,"<PROXY><DATA ETTY=\""+NomadProxy.GetProxy().GetEtty().GetAttr("id")+"\"/></PROXY>");
      Hashtable MySOL=new Hashtable();

      try {
        NomadEnvironment.GetCurrentTransaction().Begin();
        this.CambiarEstado(nuevoEstado, MyPROY.FirstChild(), MyREC.FirstChild().GetAttr("id_rec"), MySOL);
        NomadEnvironment.GetCurrentTransaction().Commit();
      } catch
      {
        NomadEnvironment.GetCurrentTransaction().Rollback();
        throw;
      }
      foreach(int oi_solicitud in MySOL.Keys)
          NucleusProject.Base.Project.Solicitudes.SOLICITUD.CerrarSolicitud(oi_solicitud,"Cerrado automáticamente por Finalización del Proyecto");
    }

    public bool Eliminar(NomadXML nodoXML)
    {
      NomadLog.Info("Eliminando: PROY "+this.id+" - "+this.c_estado);

       if (this.c_estado=="A") return false;
      if (this.c_estado=="F") return false;

      //Recorro todas las tareas
      for (NomadXML cur=nodoXML.FirstChild(); cur!=null; cur=cur.Next())
      {
        if (cur.Name=="TAREA")
        {
          NucleusProject.Base.Project.Tareas.TAREA tar=NucleusProject.Base.Project.Tareas.TAREA.Get(cur.GetAttr("id"));
          NomadLog.Info("Eliminar: TAREA "+tar.id+" - "+tar.c_estado);
          if (tar.c_estado=="A") return false;
          if (tar.c_estado=="F") return false;
          if (tar.c_estado=="T") return false;
          if (tar.c_estado=="P") return false;

          NomadEnvironment.GetCurrentTransaction().Delete(tar);
        } else
        if (cur.Name=="PROY")
          if (!NucleusProject.Base.Project.Proyectos.PROYECTO.Get(cur.GetAttr("id")).Eliminar(cur))
            return false;
      }

      //Guardo el DDO
      NomadEnvironment.GetCurrentTransaction().Delete(this);
      return true;
    }

    public void Eliminar()
    {
      NomadXML MyPROY=NomadEnvironment.QueryNomadXML(NucleusProject.Base.Project.Proyectos.PROYECTO.Resources.qry_proyectos_child,"<DATOS id=\""+this.id+"\"/>");

      try {
        NomadEnvironment.GetCurrentTransaction().Begin();
        if (!this.Eliminar(MyPROY.FirstChild()))
          new NomadAppException("No se puede Eliminar el Proyecto.");
        NomadEnvironment.GetCurrentTransaction().Commit();
      } catch
      {
        NomadEnvironment.GetCurrentTransaction().Rollback();
        throw;
      }
    }

    public void Activar()
    {
      if (this.c_estado=="T") return;

      if (!this.oi_proyecto_parentNull)
        this.Getoi_proyecto_parent().Activar();

      this.c_estado="T";
      this.f_inicio=DateTime.Now;

      NomadLog.Info("Guardar Proyecto: "+this.Id);
      NomadEnvironment.GetCurrentTransaction().SaveRefresh(this);
    }

    public void Copiar(NomadXML myNode, string parentId, string parentCode, string estado, bool isChild)
    {
      //Obtengo el Template y lo Duplico
      NucleusProject.Base.Project.Proyectos.PROYECTO objTemplate=NucleusProject.Base.Project.Proyectos.PROYECTO.Get(myNode.GetAttr("id"));

      this.oi_proyecto_parent=parentId;
      this.oi_tipo_proyecto  =objTemplate.oi_tipo_proyecto;
      //Si el proyecto a crear es un hijo, reemplazo las XXX de la plantilla, por el codigo parentCode
      if (isChild)
          if (objTemplate.c_proyecto.Contains("XXX"))
              this.c_proyecto = objTemplate.c_proyecto.Replace("XXX", parentCode);
          else
              this.c_proyecto = parentCode+" "+objTemplate.c_proyecto;
      //Si no es un hijo, el codigo del proyecto es parentCode
      else
          this.c_proyecto = parentCode;
      this.d_proyecto        =objTemplate.d_proyecto;
      this.oi_recurso        =objTemplate.oi_recurso;
      this.c_estado          =estado;
      this.o_proyecto        =objTemplate.o_proyecto;
      this.e_estimado        =0;
      this.oi_unidad_tiempo  ="2";

      //Guardo el PROYECTO
      NomadEnvironment.GetCurrentTransaction().SaveRefresh(this);

      //Recorro la lista de proyectos Hijos
      for (NomadXML cur=myNode.FirstChild(); cur!=null; cur=cur.Next())
      {
        if (cur.Name=="TAREA")
        {
          NucleusProject.Base.Project.Tareas.TAREA tar   =NucleusProject.Base.Project.Tareas.TAREA.Get(cur.GetAttr("id"));
          NucleusProject.Base.Project.Tareas.TAREA newtar=(NucleusProject.Base.Project.Tareas.TAREA)tar.Duplicate();

          newtar.oi_proyecto=this.Id;
          newtar.oi_pry_imputa=this.Id;
          newtar.f_inicioNull=true;
          newtar.f_finNull=true;
          newtar.c_estado=estado;

          NomadEnvironment.GetCurrentTransaction().Save(newtar);
        } else
        if (cur.Name=="PROY")
        {
          NucleusProject.Base.Project.Proyectos.PROYECTO objNew=new NucleusProject.Base.Project.Proyectos.PROYECTO();
          objNew.Copiar(cur, this.Id, parentCode, estado,true);
        }
      }

    }

    public void Copiar(string templateId, string parentId, string parentCode)
    {
      string estado="I";
      this.c_proyecto=parentCode;

      if (templateId=="0") return;
      NomadXML MyPROY=NomadEnvironment.QueryNomadXML(NucleusProject.Base.Project.Proyectos.PROYECTO.Resources.qry_proyectos_child,"<DATOS id=\""+templateId+"\"/>");

      //Calculo Estado
      if (parentId!="")
        estado=(NucleusProject.Base.Project.Proyectos.PROYECTO.Get(parentId).c_estado=="L"?"L":"I");

      Copiar(MyPROY.FirstChild(), parentId, parentCode, estado,false);
    }

    public NomadXML Sincronizar(NomadXML xmlProyecto)
    {
      NomadXML xmlTareas = new NomadXML("TAREAS");
      NomadLog.Info("Comienza Sincronizar");
      NomadXML xmlTasks = xmlProyecto.FindElement("tasks");
      NomadLog.Info("xmlTasks:: " + xmlTasks.ToString());
      //Falta hacer la validación cuando no hay tareas
      this.GetTareasFromGanttProject(xmlTasks,xmlTareas, "");
      NomadXML tasks = xmlTareas.AddTailElement("tasks");

      NucleusProject.Base.Project.Tareas.TAREA objTarea;

      //Obtengo el recurso por ahora es el que ejecuta el método
      string oiPersonal = NomadEnvironment.QueryValue("PER01_PERSONAL", "oi_personal", "oi_usuario_sistema", NomadProxy.GetProxy().GetEtty().GetAttr("id"), "", true);
      string oiRecurso = NomadEnvironment.QueryValue("PRY05_RECURSOS", "oi_recurso", "oi_personal", oiPersonal, "", true);

      for (NomadXML cur=xmlTareas.FirstChild(); cur!=null; cur=cur.Next())
      {
        if (cur.Name=="task")
        {
          cur.SetAttr("agrupamiento",cur.GetAttr("agrupamiento").Substring(3,cur.GetAttr("agrupamiento").Length-3));
          cur.SetAttr("start",cur.GetAttr("start").Replace("-",""));
          cur.SetAttr("oiRecurso",oiRecurso);

          //Ejecuto qry
          string oiTarea = NomadEnvironment.QueryValue("PRY02_TAREAS", "oi_tarea", "c_tarea", cur.GetAttr("id"), "PRY02_TAREAS.oi_proyecto = "+ this.Id, false);
          NomadLog.Info("oiTarea:: " + oiTarea);
          if (oiTarea!="")
          {
            tasks.SetAttr("ids_tareas",tasks.GetAttr("ids_tareas")==""?oiTarea:tasks.GetAttr("ids_tareas")+','+oiTarea);
            objTarea = NucleusProject.Base.Project.Tareas.TAREA.Get(oiTarea);

            //<task id="22" name="Presupuesto" color="#8cb6ce" meeting="false" start="2012-12-03" duration="1" complete="0" expand="true" agrupamiento=" | FASE 0: Iniciación | Solicitar Info del proyecto" />
            if (cur.GetAttr("name") != objTarea.d_tarea)
              cur.SetAttr("chg_d_tarea",objTarea.d_tarea + " --> " + cur.GetAttr("name"));
            NomadLog.Info("cur.GetAttr(start) -- objTarea.f_inicio.ToString(yyyyMMdd)::" + cur.GetAttr("start") +"--"+ objTarea.f_inicio.ToString("yyyyMMdd"));
            if (cur.GetAttr("start") != objTarea.f_inicio.ToString("yyyyMMdd"))
              cur.SetAttr("chg_f_inicio",objTarea.f_inicio.ToString("yyyyMMdd") + " --> " + cur.GetAttr("start"));
            if (cur.GetAttr("duration") != objTarea.e_duracion.ToString())
              cur.SetAttr("e_duracion",objTarea.e_duracion + " --> " + cur.GetAttr("duration"));
            if (cur.GetAttr("complete") != objTarea.e_avance.ToString())
              cur.SetAttr("chg_e_avance",objTarea.o_agrupador + " --> " + cur.GetAttr("complete"));
            if (cur.GetAttr("agrupamiento") != objTarea.o_agrupador)
              cur.SetAttr("chg_o_agrupador",objTarea.o_agrupador + " --> " + cur.GetAttr("agrupamiento"));
            if (cur.GetAttr("esfuerzo") != objTarea.e_estimado.ToString())
              cur.SetAttr("chg_e_estimado",objTarea.e_estimado + " --> " + cur.GetAttr("esfuerzo"));

            if (cur.GetAttr("name") != objTarea.d_tarea || cur.GetAttr("start") != objTarea.f_inicio.ToString() || cur.GetAttr("duration") != objTarea.e_duracion.ToString() || cur.GetAttr("complete") != objTarea.e_avance.ToString() || cur.GetAttr("agrupamiento") != objTarea.o_agrupador || cur.GetAttr("esfuerzo") != objTarea.e_estimado.ToString())
              cur.SetAttr("status","CHG");
            else
              cur.SetAttr("status","EQU");
          }
          else
            cur.SetAttr("status","NEW");
        }
      }

      NomadLog.Info("xmlTareas:: " + xmlTareas.ToString());
      return xmlTareas;
    }

    public void GetTareasFromGanttProject(NomadXML xmlTasks,NomadXML xmlTareas,string strAgrupamiento)
    {
      for (NomadXML cur=xmlTasks.FirstChild(); cur!=null; cur=cur.Next())
      {
        if (cur.Name!="task")
          continue;

        if (cur.FindElement("task")==null)
        {
          cur.SetAttr("agrupamiento",strAgrupamiento);

          foreach (NomadXML c in cur.GetChilds())
          {
            if (c.Name=="customproperty" && c.GetAttr("taskproperty-id")=="tpc0")
              cur.SetAttr("esfuerzo",c.GetAttr("value"));

            cur.DeleteChild(c);
          }

          xmlTareas.AddXML(cur);
          continue;
        }

        GetTareasFromGanttProject(cur, xmlTareas, strAgrupamiento +" | "+ cur.GetAttr("name"));
      }
    }

    public void SAVE_TAREA(NomadXML xmlTarea)
    {
      NucleusProject.Base.Project.Tareas.TAREA objTarea;
      NucleusProject.Base.Project.Tareas.TAREA_TRAZA objTraza;
      //Ejecuto qry
      string oiTarea = NomadEnvironment.QueryValue("PRY02_TAREAS", "oi_tarea", "c_tarea", xmlTarea.GetAttr("id"), "PRY02_TAREAS.oi_proyecto = "+ this.Id, false);
      NomadLog.Info("oiTarea:: " + oiTarea);
      if (oiTarea!="")
        objTarea = NucleusProject.Base.Project.Tareas.TAREA.Get(oiTarea);
      else
        objTarea = new NucleusProject.Base.Project.Tareas.TAREA();

      objTarea.d_tarea = xmlTarea.GetAttr("name");
      //objTarea.f_inicio = xmlTarea.GetAttr("start");
      objTarea.e_duracion = xmlTarea.GetAttrInt("duration");
      objTarea.e_avance = xmlTarea.GetAttrInt("complete");
      objTarea.o_agrupador = xmlTarea.GetAttr("agrupamiento");
      objTarea.e_estimado = xmlTarea.GetAttrInt("esfuerzo");
      objTarea.oi_unidad_tiempo = "2";//Hora
      objTarea.c_estado = "T";//Hora
      objTarea.oi_recurso = xmlTarea.GetAttr("oiRecurso");

      //crear objeto traza, setearle los atributos necesarios
      objTraza = new NucleusProject.Base.Project.Tareas.TAREA_TRAZA();

      objTraza.c_estado = "U";
      objTraza.f_fecha_tarea = DateTime.Now;
      objTraza.c_causa = "Actualización a través de Sincronizador";

      objTraza.oi_recurso = xmlTarea.GetAttr("oiRecurso");

      objTraza.oi_recurso_asi = xmlTarea.GetAttr("oiRecurso");
      NomadEnvironment.GetTrace().Info("Objeto Traza: " + objTraza.SerializeAll());

       //agregarlo a la coleccion en tarea
       objTarea.TAREAS_TRAZA.Add(objTraza);

      //hacer el save de cada tarea
      NomadEnvironment.GetCurrentTransaction().Save(objTarea);
    }

    public static void CambiarEstadoPadre(Nomad.NSystem.Proxy.NomadXML xmlOI)
    {
        NomadLog.Info("Comienza el metodo CambiarEstadoPadre");
        NomadLog.Info("Metodo Emi:: " + xmlOI.ToString());
        Nomad.NSystem.Proxy.NomadXML xml = xmlOI.FirstChild();
        NomadLog.Info("xml:: " + xml.ToString());


        for (NomadXML row = xml.FirstChild(); row != null; row = row.Next())
        {
            NomadLog.Info("ROW:: " + row.ToString());
            //Verifico el Estado

            NucleusProject.Base.Project.Proyectos.PROYECTO pryEstado = NucleusProject.Base.Project.Proyectos.PROYECTO.Get(row.GetAttr("oi_proyecto"));
            pryEstado.c_estado = "T";

            NomadLog.Info("pryEstado:: " + pryEstado.SerializeAll());


            //Guardo el estado
            NomadEnvironment.GetCurrentTransaction().Save(pryEstado);

        }
    }

    public static Nomad.NSystem.Proxy.NomadXML SincronizarProyecto()
    {
        NomadLog.Info("Comienza SincronizarProyecto");
        NomadXML xmlProyecto = NomadProxy.GetProxy().FileServiceIO().LoadFileXML("INTERFACES", "Proyecto.dat");
        NomadLog.Info("xmlProyecto:: " + xmlProyecto.ToString());
        NomadLog.Info("Proyecto name:: " + xmlProyecto.GetAttr("name").ToString());
        NomadLog.Info("Proyecto company:: " + xmlProyecto.GetAttr("company").ToString());
        NomadXML xmlProyectoProcesado = new NomadXML("PROYECTO");

        string[] arrName = xmlProyecto.GetAttr("name").Split('-');
        if (arrName.Length < 0)
            throw new Exception("No se cargó correctamente el código - nombre del proyecto");

        string strCode = arrName[0].ToString().Trim();
        string strName = arrName[1].ToString().Trim();

        NomadLog.Info("strCode:: " + strCode);
        NomadLog.Info("strName:: " + strName);

        xmlProyectoProcesado.SetAttr("c_proyecto", strCode);
        xmlProyectoProcesado.SetAttr("d_proyecto", strName);

        //Recupero el proyecto
        NucleusProject.Base.Project.Proyectos.PROYECTO objProyecto;
        string oiPRY = NomadEnvironment.QueryValue("PRY01_PROYECTOS", "oi_proyecto", "c_proyecto", strCode, "", false);
        NomadLog.Info("oiPRY:: " + oiPRY);
        if (oiPRY == "")
            objProyecto = new NucleusProject.Base.Project.Proyectos.PROYECTO();
        else
            objProyecto = NucleusProject.Base.Project.Proyectos.PROYECTO.Get(oiPRY);

        NomadLog.Info("objProyecto:: " + objProyecto.SerializeAll());
        xmlProyectoProcesado.AddXML(objProyecto.Sincronizar(xmlProyecto));

        //xmlProyectoProcesado.SetAttr("ids_tareas",xmlProyectoProcesado.FirstChild().GetAttr("ids_tareas"));
        //xmlProyectoProcesado.FirstChild().SetAttr("ids_tareas","");

        NomadLog.Info("xmlProyectoProcesado:: " + xmlProyectoProcesado.ToString());

        return xmlProyectoProcesado;
    }

      public static void SAVE_SYNC( Nomad.NSystem.Proxy.NomadXML xmlProyecto) 
      {
          NomadLog.Info("Comienza SAVE_SYNC");
          //NomadXML xmlProyecto;
          NomadLog.Info("xmlProyecto:: " + xmlProyecto.ToString());


          //Recupero el proyecto
          NucleusProject.Base.Project.Proyectos.PROYECTO objProyecto;
          string oiPRY = NomadEnvironment.QueryValue("PRY01_PROYECTOS", "oi_proyecto", "c_proyecto", xmlProyecto.FirstChild().GetAttr("c_proyecto"), "", false);
          NomadLog.Info("oiPRY:: " + oiPRY);
          if (oiPRY == "")
          {
              objProyecto = new NucleusProject.Base.Project.Proyectos.PROYECTO();
              //setear campos...
          }
          else
              objProyecto = NucleusProject.Base.Project.Proyectos.PROYECTO.Get(oiPRY);

          NomadLog.Info("xmlTareas:: " + xmlProyecto.FirstChild().FirstChild().ToString());
          for (NomadXML cur = xmlProyecto.FirstChild().FirstChild().FirstChild(); cur != null; cur = cur.Next())
          {
              if (cur.Name != "task")
                  continue;

              objProyecto.SAVE_TAREA(cur);
          }     
      }
 
      /// <summary>
      /// Une el xml Filtar Data y el xmlIds para el reporte tareas de un proyecto
      /// </summary>
      /// <param name="FILTERDATA"></param>
      /// <param name="IDsel"></param>
       public static void REPORTE_TAREAS_PROYECTO(ref Nomad.NSystem.Proxy.NomadXML FILTERDATA, Nomad.NSystem.Proxy.NomadXML IDsel)
      {
           NomadLog.Debug("----------------------------------------------------------");
           NomadLog.Debug("------------------REPORTE_TAREAS_PROYECTO V1-----------------");
           NomadLog.Debug("----------------------------------------------------------");

            NomadLog.Debug("ParametroFILTERDATA: " + FILTERDATA.ToString());
            NomadLog.Debug("ParametroIDsel: " + IDsel.ToString());         

            string strStep = "";            
            try
            {
                strStep = "RECORRO-IDSel";
                
                //Bandera para evaluar si debo combinar los dos documentos
                bool blMergear = false;
                
                if (IDsel.isDocument)
                {
                    if (IDsel.FirstChild().ChildLength != 0)
                    {
                        blMergear = true;
                    }
                }
                else
                {
                    if (IDsel.ChildLength != 0)
                    {
                        blMergear = true;
                    }
                }


                if (blMergear)
                {
                    StringBuilder strAttrID = new StringBuilder("IN ( ");



                    for (NomadXML row = IDsel.FirstChild().FirstChild(); row != null; row = row.Next())
                    {
                        string strID = row.GetAttrString("id");

                        strStep = "CONCATENO-IDS-" + strID;

                        strAttrID = strAttrID.Append("\\'" + strID + "\\'" + ",");

                    }

                    strAttrID = strAttrID.Remove(strAttrID.Length - 1, 1);
                    strAttrID = strAttrID.Append(" )");

                   
                    //IDs="IN(\'P\',\'AP\',\'T\')"            
                    string strAttrID2 = strAttrID.ToString();

                    strStep = "SETEO-IDS-FILTERDATA";

                    String nombre_IDs = IDsel.FirstChild().Name+"_IDs";

                    FILTERDATA.FirstChild().SetAttr(nombre_IDs, strAttrID2);
                    string resultado = FILTERDATA.FirstChild().GetAttrString(nombre_IDs);
                }
            }
            catch (Exception ex)
            {
                NomadException nmdEx = new NomadException("NucleusProject.Base.Project.Proyectos.REPORTE_TAREAS_PROYECTO()", ex);
                nmdEx.SetValue("Step", strStep);
                
                throw nmdEx;
            }

      }
   }
}



using System;
using System.Xml;
using System.IO;
using System.Collections;

using Nomad.NSystem.Document;
using Nomad.NSystem.Proxy;
using Nomad.NSystem.Base;
using Nomad.NSystem.Functions;
using System.Net;
using System.Collections.Generic;

namespace NucleusProject.Base.Project.Tareas
{

    //////////////////////////////////////////////////////////////////////////////////
    //Clase Tareas
    public partial class TAREA : Nomad.NSystem.Base.NomadObject
    {
        public void Activar()
        {

            try
            {
                NomadEnvironment.GetTrace().Info("Begin Transaction");
                NomadEnvironment.GetCurrentTransaction().Begin();

                this.Getoi_proyecto().Activar();
                this.c_estado = "T";
                if (this.f_inicioNull) this.f_inicio = DateTime.Now;

                NomadEnvironment.GetTrace().Info("Guardar Tarea: " + this.Id);
                NomadEnvironment.GetCurrentTransaction().SaveRefresh(this);

                if (!this.oi_solicitudNull)
                {
                    NucleusProject.Base.Project.Solicitudes.SOLICITUD objSolicitud = NucleusProject.Base.Project.Solicitudes.SOLICITUD.Get(this.oi_solicitud);
                    objSolicitud.f_estado = DateTime.Now;
                    objSolicitud.c_estado = "3";
                    NomadEnvironment.GetTrace().Info("Guardar Solicitud: " + objSolicitud.SerializeAll());
                    NomadEnvironment.GetCurrentTransaction().SaveRefresh(objSolicitud);
                }

                NomadEnvironment.GetTrace().Info("Commit Transaction");
                NomadEnvironment.GetCurrentTransaction().Commit();

                // Comienza el envio de Mails

                // guardo oi de la tarea para mandar por parametro al evento.
                System.Text.StringBuilder IDS = new System.Text.StringBuilder();
                IDS.Append(this.Id);

                // recupero oi del recurso logueado
                NomadXML log = Nomad.NSystem.Base.NomadEnvironment.QueryNomadXML(NucleusProject.Base.Project.Tareas.TAREA.Resources.qry_recurso_log, "");
                String oi_recurso_log = log.FirstChild().GetAttr("oi_recurso");

                // recupero los recursos asignados en cada una
                NucleusProject.Base.Project.Tareas.TAREA_TRAZA traza_anterior, traza_actual = NucleusProject.Base.Project.Tareas.TAREA_TRAZA.New();
                if(this.TAREAS_TRAZA.Count >= 2)
                {
                  traza_anterior = (NucleusProject.Base.Project.Tareas.TAREA_TRAZA) this.TAREAS_TRAZA[this.TAREAS_TRAZA.Count - 2];
                  traza_actual = (NucleusProject.Base.Project.Tareas.TAREA_TRAZA)this.TAREAS_TRAZA[this.TAREAS_TRAZA.Count - 1];
                  String oi_recurso_anterior = traza_anterior.oi_recurso_asi;
                  String oi_recurso_actual = traza_actual.oi_recurso_asi;

                    NomadLog.Debug("Recurso Anterior: "+oi_recurso_anterior);
                    NomadLog.Debug("Recurso Actual: "+oi_recurso_actual);

                  //si son diferentes envia el mail
                  if (oi_recurso_anterior != oi_recurso_actual)
                  {
                  NomadLog.Debug("ENVIADO");
                    NucleusRH.Base.Configuracion.LogEventos.LOGEVENTO.Call("NucleusProject.Base.Project.Tareas.activar_tarea", IDS.ToString(), oi_recurso_log, oi_recurso_anterior, oi_recurso_actual);
                  }
                }

            }
            catch
            {
                NomadEnvironment.GetCurrentTransaction().Rollback();
                throw;
            }

        }

        public void Copiar(string oi_recurso, Nomad.NSystem.Proxy.NomadXML oi_prys)
        {

            try
            {
                NomadEnvironment.GetTrace().Info("Begin Transaction");
                NomadEnvironment.GetCurrentTransaction().Begin();

                for (NomadXML cur = oi_prys.FirstChild().FirstChild(); cur != null; cur = cur.Next())
                {
                    //Verifico el Estado
                    NomadEnvironment.GetTrace().Info("Verifico estado Proyecto: " + cur.GetAttr("id"));
                    string pryEstado = NucleusProject.Base.Project.Proyectos.PROYECTO.Get(cur.GetAttr("id")).c_estado;
                    if ((pryEstado != "I") && (pryEstado != "T"))
                        throw new NomadAppException("Existen Proyectos que no se pueden Modificar.");

                    //Copiar la Tarea
                    NomadEnvironment.GetTrace().Info("Duplico Tarea: " + this.Id);
                    NucleusProject.Base.Project.Tareas.TAREA newTarea = (NucleusProject.Base.Project.Tareas.TAREA)this.Duplicate();
                    newTarea.c_estado = "I";
                    if (cur.GetAttr("id") == newTarea.oi_proyecto)
                        newTarea.c_tarea = "Copia de " + newTarea.c_tarea;
                    newTarea.oi_proyecto = cur.GetAttr("id");
                    newTarea.f_inicioNull = true;
                    newTarea.f_finNull = true;

                    //Agrego la Traza
                    newTarea.TAREAS_TRAZA.Clear();
                    NucleusProject.Base.Project.Tareas.TAREA_TRAZA newTrace = new NucleusProject.Base.Project.Tareas.TAREA_TRAZA();
                    newTrace.c_causa = "Copiar de Tarea";
                    newTrace.c_estado = "I";
                    newTrace.f_fecha_tarea = DateTime.Now;
                    newTrace.o_observacion = "";
                    newTrace.oi_recurso = oi_recurso;
                    newTrace.oi_recurso_asi = oi_recurso;
                    newTarea.TAREAS_TRAZA.Add(newTrace);

                    //Guardo el Objecto
                    NomadEnvironment.GetTrace().Info("Guardo la copia...");
                    NomadEnvironment.GetCurrentTransaction().Save(newTarea);
                }

                NomadEnvironment.GetTrace().Info("Commit Transaction");
                NomadEnvironment.GetCurrentTransaction().Commit();
            }
            catch
            {
                NomadEnvironment.GetCurrentTransaction().Rollback();
                throw;
            }

        }

        /// <summary>
        /// crea una tarea y una traza.
        /// </summary>
        /// <param name="pobjTarea"></param>
        public static void Guardar(NucleusProject.Base.Project.Tareas.TAREA pobjTarea)
        {

            NomadEnvironment.GetTrace().Info("TAREA: " + pobjTarea.ToString());

            //bool trello = pobjTarea.l_trello;
            
            string strStep = "Seteando Parámetros";
            System.Text.StringBuilder IDS = new System.Text.StringBuilder();
            NomadXML PARAM = new NomadXML("PARAMS");
            PARAM.SetAttr("xval", pobjTarea.oi_pry_imputa);

            //guarda la tarea
            NomadTransaction objTran1 = new NomadTransaction();
            try
            {
                objTran1.Begin();

                strStep = "Grabando TAREA";
                objTran1.SaveRefresh(pobjTarea);

                objTran1.Commit();
            }

            catch (Exception ex)
            {
                NomadException nmdEx = NomadException.NewInternalException("NucleusProject.Base.Project.Tareas.TAREA.Guardar()", ex);
                nmdEx.SetValue("step", strStep);
                objTran1.Rollback();
                throw nmdEx;

            }

            //guarda la traza en la tarea recien guardada
            NomadTransaction objTran2 = new NomadTransaction();
            try
            {
                objTran2.Begin();

                strStep = "Recuperando Tarea";
                NucleusProject.Base.Project.Tareas.TAREA DDO = NucleusProject.Base.Project.Tareas.TAREA.Get(pobjTarea.Id);
                IDS.Append(DDO.Id);

                //Necesito el oi del recurso que esta logueado
                NomadXML log = Nomad.NSystem.Base.NomadEnvironment.QueryNomadXML(NucleusProject.Base.Project.Tareas.TAREA.Resources.qry_recurso_log, "");

                //traza que se crea automáticamente
                NucleusProject.Base.Project.Tareas.TAREA_TRAZA objTraza1 = NucleusProject.Base.Project.Tareas.TAREA_TRAZA.New();

                strStep = "Seteando Traza1";

                objTraza1.c_estado = "I";
                objTraza1.c_causa = "Creacion de la tarea";
                objTraza1.oi_recurso_asi = DDO.oi_recurso;
                objTraza1.oi_recurso = log.FirstChild().GetAttr("oi_recurso");

                strStep = "Asignado Trazas a la Tarea";

                pobjTarea.TAREAS_TRAZA.Add(objTraza1);

                /*if (trello)
                {
                    strStep = "Creando Tarjeta Trello";
                    string c_trello = CrearTarjetaTrello(pobjTarea);
                    if (c_trello != "")
                        pobjTarea.c_externo = c_trello;
                }*/
                
                strStep = "Grabando Tarea";
                objTran2.SaveRefresh(pobjTarea);

                objTran2.Commit();

                strStep = "LLamada a Evento para envío de mails";

                NomadLog.Debug("log_Car " + strStep.ToString());

                String oi_recurso_log = log.FirstChild().GetAttr("oi_recurso");

                NucleusRH.Base.Configuracion.LogEventos.LOGEVENTO.Call("NucleusProject.Base.Project.Tareas.add_tarea", IDS.ToString(), oi_recurso_log,"","");
            }
            catch (Exception ex)
            {
                NomadException nmdEx = NomadException.NewInternalException("NucleusProject.Base.Project.Tareas.TAREA.Guardar()", ex);
                nmdEx.SetValue("step", strStep);
                objTran2.Rollback();
                throw nmdEx;

            }
        }

        public void Mail_Cambio_Responsable()
        {

            try
            {

                // guardo oi de la tarea para mandar por parámetro al evento.
                System.Text.StringBuilder IDS = new System.Text.StringBuilder();
                IDS.Append(this.Id);

                // recupero oi del recurso logueado
                NomadXML log = Nomad.NSystem.Base.NomadEnvironment.QueryNomadXML(NucleusProject.Base.Project.Tareas.TAREA.Resources.qry_recurso_log, "");
                String oi_recurso_log = log.FirstChild().GetAttr("oi_recurso");

                // creo dos trazas
                NucleusProject.Base.Project.Tareas.TAREA_TRAZA traza_anterior, traza_actual = NucleusProject.Base.Project.Tareas.TAREA_TRAZA.New();

                // les asigno la anteúltima y última trazas de la tarea
                traza_anterior = (NucleusProject.Base.Project.Tareas.TAREA_TRAZA)this.TAREAS_TRAZA[this.TAREAS_TRAZA.Count - 2];
                traza_actual = (NucleusProject.Base.Project.Tareas.TAREA_TRAZA)this.TAREAS_TRAZA[this.TAREAS_TRAZA.Count - 1];

                // recupero los recursos asignados en cada una
                String oi_recurso_anterior = traza_anterior.oi_recurso_asi;
                String oi_recurso_actual = traza_actual.oi_recurso_asi;

                //si son diferentes envío mail
                if (oi_recurso_anterior != oi_recurso_actual)
                {
                    NucleusRH.Base.Configuracion.LogEventos.LOGEVENTO.Call("NucleusProject.Base.Project.Tareas.chg_resp_tarea", IDS.ToString(), oi_recurso_log, oi_recurso_anterior, oi_recurso_actual);
                }

            }
            catch
            {
                throw;
            }

        }

        public static void GuardarTrello( NucleusProject.Base.Project.Tareas.TAREA pobjTarea, Nomad.NSystem.Proxy.NomadXML pxmlTarjeta)         
        {
            NomadEnvironment.GetTrace().Info("TAREA: " + pobjTarea.ToString());
            bool trello = pobjTarea.l_trello;

            string strStep = "Seteando Parámetros";
            System.Text.StringBuilder IDS = new System.Text.StringBuilder();
            NomadXML PARAM = new NomadXML("PARAMS");
            PARAM.SetAttr("xval", pobjTarea.oi_pry_imputa);

            //guarda la tarea
            NomadTransaction objTran1 = new NomadTransaction();
            try
            {
                objTran1.Begin();

                strStep = "Grabando TAREA";
                objTran1.SaveRefresh(pobjTarea);

                objTran1.Commit();
            }

            catch (Exception ex)
            {
                NomadException nmdEx = NomadException.NewInternalException("NucleusProject.Base.Project.Tareas.TAREA.Guardar()", ex);
                nmdEx.SetValue("step", strStep);
                objTran1.Rollback();
                throw nmdEx;

            }

            //guarda la traza en la tarea recien guardada
            NomadTransaction objTran2 = new NomadTransaction();
            try
            {
                objTran2.Begin();

                strStep = "Recuperando Tarea";
                NucleusProject.Base.Project.Tareas.TAREA DDO = NucleusProject.Base.Project.Tareas.TAREA.Get(pobjTarea.Id);
                IDS.Append(DDO.Id);

                //Necesito el oi del recurso que esta logueado
                NomadXML log = Nomad.NSystem.Base.NomadEnvironment.QueryNomadXML(NucleusProject.Base.Project.Tareas.TAREA.Resources.qry_recurso_log, "");

                //traza que se crea automáticamente
                NucleusProject.Base.Project.Tareas.TAREA_TRAZA objTraza1 = NucleusProject.Base.Project.Tareas.TAREA_TRAZA.New();

                strStep = "Seteando Traza1";

                objTraza1.c_estado = "I";
                objTraza1.c_causa = "Creacion de la tarea";
                objTraza1.oi_recurso_asi = DDO.oi_recurso;
                objTraza1.oi_recurso = log.FirstChild().GetAttr("oi_recurso");

                strStep = "Asignado Trazas a la Tarea";
                pobjTarea.TAREAS_TRAZA.Add(objTraza1);

                strStep = "Grabando Tarea";
                objTran2.SaveRefresh(pobjTarea);

                objTran2.Commit();

                strStep = "LLamada a Evento para envío de mails";

                NomadLog.Debug("log_Car " + strStep.ToString());

                String oi_recurso_log = log.FirstChild().GetAttr("oi_recurso");

                NucleusRH.Base.Configuracion.LogEventos.LOGEVENTO.Call("NucleusProject.Base.Project.Tareas.add_tarea", IDS.ToString(), oi_recurso_log, "", "");
            }
            catch (Exception ex)
            {
                NomadException nmdEx = NomadException.NewInternalException("NucleusProject.Base.Project.Tareas.TAREA.Guardar()", ex);
                nmdEx.SetValue("step", strStep);
                objTran2.Rollback();
                throw nmdEx;

            }

            if (trello)
            {
                NomadTransaction objTran3 = new NomadTransaction();
                try
                {
                    objTran3.Begin();

                    strStep = "Creando Tarjeta Trello";
                    string url = "";
                    strStep = "Recuperando Tarea";
                    NucleusProject.Base.Project.Tareas.TAREA DDO = NucleusProject.Base.Project.Tareas.TAREA.Get(pobjTarea.Id);

                    string c_trello = CrearTarjetaTrello(pxmlTarjeta, DDO, ref url);
                    if (c_trello != "")
                    {
                        pobjTarea.c_externo = c_trello;
                        pobjTarea.d_enlace = url;
                    }
                        

                    strStep = "Grabando Tarea";
                    objTran3.SaveRefresh(pobjTarea);

                    objTran3.Commit();
                }
                catch (Exception ex)
                {
                    NomadException nmdEx = NomadException.NewInternalException("NucleusProject.Base.Project.Tareas.TAREA.Guardar()", ex);
                    nmdEx.SetValue("step", strStep);
                    objTran3.Rollback();
                    throw nmdEx;

                }
            }
        }

        private static Token GetTokenCurrentUser(string idColumna)
        {
            HttpWebRequest request = null;
            List<string> teamsTrello = new List<string>();
            Token token = null;
            ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072;

            string cuenta = GetCuenta(NomadProxy.GetProxy().UserEtty);
            Nomad.Base.Login.Cuentas.CUENTA cta = new Nomad.Base.Login.Cuentas.CUENTA(cuenta);

            foreach (Nomad.Base.Login.Cuentas.USUARIO_GRUPO gr in cta.GRUPOS)
            {
                if (gr.GRUPO.Contains("TRE"))
                    teamsTrello.Add(gr.GRUPO.Replace("TRE", ""));
            }

            if (teamsTrello.Count > 0)
            {
                foreach (string team in teamsTrello)
                {
                    token = new Token(team);

                    if (token.key != null && token.token != null)
                    {
                        request = (HttpWebRequest)WebRequest.Create(@"https://api.trello.com/1/lists/" + idColumna + "?fields=name&key=" + token.key + "&token=" + token.token);
                        request.ContentType = "application/json";
                        request.Method = "GET";

                        try
                        {
                            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                            using (StreamReader streamReader = new StreamReader(response.GetResponseStream()))
                            {
                                break;
                            }
                        }
                        catch (WebException ex)
                        {
                            continue;
                        }
                        catch (Exception e)
                        {
                            continue;
                        }
                    }
                }
            }

            return token;
        }

        public static string CrearTarjetaTrello(NomadXML xmlTarjeta, NucleusProject.Base.Project.Tareas.TAREA pobjTarea, ref string url)
        {
            HttpWebRequest request = null;
            //Member miembro = null;
            Search busqueda = null;
            Card c = new Card();
            List<string> teamsTrello = new List<string>();
            string result;
            string idTrello = "";
            string idList = "";
            string strStep = "INICIO-TRELLO";
            string comillas = "\"";

            ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072; //TLS 1.2

            if (xmlTarjeta.FirstChild().GetAttr("id_columna_trello") == "") return idTrello;

            strStep = "GET-ID-TABLERO";
            idList = GetIdList(xmlTarjeta.FirstChild().GetAttr("id_columna_trello"));

            if (idList == "") return idTrello;

            strStep = "TOKEN-TRELLO";
            Token token = GetTokenCurrentUser(idList);

            if (token != null)
            {
                strStep = "NEW-CARD";
                c.name = xmlTarjeta.FirstChild().GetAttr("d_tarea_trello");
                c.desc = xmlTarjeta.FirstChild().GetAttr("o_tarea_trello");
                c.idList = idList;
                c.pos = "bottom";
                c.keepFromSource = "all";
                c.token = token.token;
                c.key = token.key;

                //recupero imagen del cliente
                if(!pobjTarea.oi_proyectoNull)
                {
                    //DIF entre PROYECTO y PROYECTO_IMPUTA
                    NucleusProject.Base.Project.Proyectos.PROYECTO proyAct = Proyectos.PROYECTO.Get(pobjTarea.oi_proyecto);
                    if(!proyAct.oi_clienteNull)
                    {
                        NucleusProject.Base.Project.Clientes.CLIENTE cliAct = Clientes.CLIENTE.Get(proyAct.oi_cliente);
                        if(cliAct!=null)
                        {
                            string proyectoTemplate = NomadEnvironment.QueryValue("ORG26_PARAMETROS", "d_valor", "c_parametro", "TemplateTrello", "", false);

                            if(proyectoTemplate!="")
                            {
                                string query = "board:" + comillas + proyectoTemplate + comillas + " " + cliAct.c_cliente;
                                request = (HttpWebRequest)WebRequest.Create(@"https://api.trello.com/1/search?query=" + query + "&modelTypes=cards&cards_limit=100&key=" + token.key + "&token=" + token.token);
                                request.ContentType = "application/json";
                                request.Method = "GET";

                                result = "";
                                try
                                {
                                    using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                                    using (StreamReader streamReader = new StreamReader(response.GetResponseStream()))
                                    {
                                        result = streamReader.ReadToEnd();
                                        busqueda = StringUtil.JSON2Object<Search>(result);
                                    }

                                    if (busqueda.cards.Count > 0)
                                    {
                                        //me quedo con la primer tarjeta encontrada
                                        if(busqueda.cards[0].cover.scaled.Count>0)
                                        {
                                            c.urlSource = busqueda.cards[0].cover.scaled[busqueda.cards[0].cover.scaled.Count - 1].url;                                            
                                        }
                                    }
                                }
                                catch (WebException ex)
                                {
                                    if (ex.Status != WebExceptionStatus.Timeout && ex.Status != WebExceptionStatus.SendFailure)
                                    {
                                        System.Net.HttpWebResponse WResponse = (System.Net.HttpWebResponse)ex.Response;

                                        result = "";
                                        using (StreamReader streamReader = new StreamReader(WResponse.GetResponseStream()))
                                        {
                                            result = streamReader.ReadToEnd();
                                        }
                                        NomadLog.Error("NucleusProject.Base.Project.Tareas.TAREA.CrearTarjetaTrello() - " + strStep + " - Error recuperando el miembro en Trello: " + result);
                                    }
                                    else
                                    {
                                        NomadLog.Error("NucleusProject.Base.Project.Tareas.TAREA.CrearTarjetaTrello() - " + strStep + " - Error recuperando el miembro en Trello: " + ex.Message);
                                    }
                                }
                                catch (Exception e)
                                {
                                    NomadLog.Error("NucleusProject.Base.Project.Tareas.TAREA.CrearTarjetaTrello() - " + strStep + " - Error recuperando el miembro en Trello: " + e.Message);
                                }
                            }
                        }
                    }
                }
                                
                try
                {
                    if (xmlTarjeta.FirstChild().GetAttr("f_fin_trello") != "")
                    {
                        //YYYYMMDDD
                        string fecha_trello = xmlTarjeta.FirstChild().GetAttr("f_fin_trello");
                        //18 + 3 (zona horaria) = 21
                        DateTime fecha = new DateTime(int.Parse(fecha_trello.Substring(0, 4)), int.Parse(fecha_trello.Substring(4, 2)), int.Parse(fecha_trello.Substring(6, 2)), 17, 00, 00);
                        c.due = fecha;
                    }
                }
                catch (Exception exce)
                {
                    NomadLog.Error("NucleusProject.Base.Project.Tareas.TAREA.CrearTarjetaTrello() - " + strStep + " - Error al transforma fecha: " + exce.Message);
                }

                strStep = "MEMBERS-CARD";
                c.idMembers = new List<string>();
                if (xmlTarjeta.FirstChild().GetAttr("oi_recurso_trello") != "")
                {
                    string userTrello = NomadEnvironment.QueryValue("PRY05_RECURSOS", "c_externo", "oi_recurso", xmlTarjeta.FirstChild().GetAttr("oi_recurso_trello"), "", false);

                    if (userTrello != "")
                    {
                        request = (HttpWebRequest)WebRequest.Create(@"https://api.trello.com/1/search?query=" + userTrello + "&modelTypes=members&members_limit=100&key=" + token.key + "&token=" + token.token);
                        request.ContentType = "application/json";
                        request.Method = "GET";

                        result = "";
                        try
                        {
                            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                            using (StreamReader streamReader = new StreamReader(response.GetResponseStream()))
                            {
                                result = streamReader.ReadToEnd();
                                busqueda = StringUtil.JSON2Object<Search>(result);
                            }

                            if (busqueda.members.Count > 0)
                            {
                                foreach (Member m in busqueda.members)
                                    c.idMembers.Add(m.id);
                            }
                        }
                        catch (WebException ex)
                        {
                            if (ex.Status != WebExceptionStatus.Timeout && ex.Status != WebExceptionStatus.SendFailure)
                            {
                                System.Net.HttpWebResponse WResponse = (System.Net.HttpWebResponse)ex.Response;

                                result = "";
                                using (StreamReader streamReader = new StreamReader(WResponse.GetResponseStream()))
                                {
                                    result = streamReader.ReadToEnd();
                                }
                                NomadLog.Error("NucleusProject.Base.Project.Tareas.TAREA.CrearTarjetaTrello() - " + strStep + " - Error recuperando el miembro en Trello: " + result);
                            }
                            else
                            {
                                NomadLog.Error("NucleusProject.Base.Project.Tareas.TAREA.CrearTarjetaTrello() - " + strStep + " - Error recuperando el miembro en Trello: " + ex.Message);
                            }
                        }
                        catch (Exception e)
                        {
                            NomadLog.Error("NucleusProject.Base.Project.Tareas.TAREA.CrearTarjetaTrello() - " + strStep + " - Error recuperando el miembro en Trello: " + e.Message);
                        }
                    }
                    else
                    {
                        NomadLog.Error("NucleusProject.Base.Project.Tareas.TAREA.CrearTarjetaTrello() - " + strStep + " - Recurso no tiene asignado usuario Trello");
                    }
                }
                //Busco si la tarea pertenece a una soli para agregar al resposable de la solicitud para agregarlo
                if(!pobjTarea.oi_solicitudNull)
                {
                    Solicitudes.SOLICITUD soliAct = Solicitudes.SOLICITUD.Get(pobjTarea.oi_solicitud);
                    if(!soliAct.oi_recursoNull)
                    {
                        if(soliAct.oi_recurso!=xmlTarjeta.FirstChild().GetAttr("oi_recurso_trello"))
                        {
                            string userTrello = NomadEnvironment.QueryValue("PRY05_RECURSOS", "c_externo", "oi_recurso", soliAct.oi_recurso, "", false);

                            if (userTrello != "")
                            {
                                request = (HttpWebRequest)WebRequest.Create(@"https://api.trello.com/1/search?query=" + userTrello + "&modelTypes=members&members_limit=100&key=" + token.key + "&token=" + token.token);
                                request.ContentType = "application/json";
                                request.Method = "GET";

                                result = "";
                                try
                                {
                                    using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                                    using (StreamReader streamReader = new StreamReader(response.GetResponseStream()))
                                    {
                                        result = streamReader.ReadToEnd();
                                        busqueda = StringUtil.JSON2Object<Search>(result);
                                    }

                                    if (busqueda.members.Count > 0)
                                    {
                                        foreach (Member m in busqueda.members)
                                            c.idMembers.Add(m.id);
                                    }
                                }
                                catch (WebException ex)
                                {
                                    if (ex.Status != WebExceptionStatus.Timeout && ex.Status != WebExceptionStatus.SendFailure)
                                    {
                                        System.Net.HttpWebResponse WResponse = (System.Net.HttpWebResponse)ex.Response;

                                        result = "";
                                        using (StreamReader streamReader = new StreamReader(WResponse.GetResponseStream()))
                                        {
                                            result = streamReader.ReadToEnd();
                                        }
                                        NomadLog.Error("NucleusProject.Base.Project.Tareas.TAREA.CrearTarjetaTrello() - " + strStep + " - Error recuperando el miembro en Trello: " + result);
                                    }
                                    else
                                    {
                                        NomadLog.Error("NucleusProject.Base.Project.Tareas.TAREA.CrearTarjetaTrello() - " + strStep + " - Error recuperando el miembro en Trello: " + ex.Message);
                                    }
                                }
                                catch (Exception e)
                                {
                                    NomadLog.Error("NucleusProject.Base.Project.Tareas.TAREA.CrearTarjetaTrello() - " + strStep + " - Error recuperando el miembro en Trello: " + e.Message);
                                }
                            }
                            else
                            {
                                NomadLog.Error("NucleusProject.Base.Project.Tareas.TAREA.CrearTarjetaTrello() - " + strStep + " - Recurso no tiene asignado usuario Trello");
                            }
                        }
                    }
                }

                try
                {
                    string json = StringUtil.Object2JSON(c);

                    strStep = "SEND-CARD";
                    request = (HttpWebRequest)WebRequest.Create(@"https://api.trello.com/1/cards");
                    request.ContentType = "application/json";
                    request.Method = "POST";

                    using (StreamWriter streamWriter = new StreamWriter(request.GetRequestStream()))
                    {
                        streamWriter.Write(json);
                        streamWriter.Flush();
                        streamWriter.Close();
                    }

                    result = "";
                    using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                    using (StreamReader streamReader = new StreamReader(response.GetResponseStream()))
                    {
                        result = streamReader.ReadToEnd();
                    }

                    if (result != "")
                    {
                        Card newCard = StringUtil.JSON2Object<Card>(result);
                        idTrello = newCard.id.ToString();
                        url = newCard.shortLink.ToString();
                        
                    }
                }
                catch (WebException ex)
                {
                    if (ex.Status != WebExceptionStatus.Timeout && ex.Status != WebExceptionStatus.SendFailure)
                    {
                        System.Net.HttpWebResponse WResponse = (System.Net.HttpWebResponse)ex.Response;

                        result = "";
                        using (StreamReader streamReader = new StreamReader(WResponse.GetResponseStream()))
                        {
                            result = streamReader.ReadToEnd();
                        }
                        NomadLog.Error("NucleusProject.Base.Project.Tareas.TAREA.CrearTarjetaTrello() - " + strStep + " - Error guardando la tarjeta en Trello: " + result);
                    }
                    else
                    {
                        NomadLog.Error("NucleusProject.Base.Project.Tareas.TAREA.CrearTarjetaTrello() - " + strStep + " - Error guardando la tarjeta en Trello: " + ex.Message);
                    }
                    throw new NomadAppException("Error al crear la tarjeta en Trello. Consulte log del proceso.");
                }
                catch (Exception e)
                {
                    NomadLog.Error("NucleusProject.Base.Project.Tareas.TAREA.CrearTarjetaTrello() - " + strStep + " - Error guardando la tarjeta en Trello: " + e.Message);
                    throw new NomadAppException("Error al crear la tarjeta en Trello. Consulte log del proceso.");
                }
            }
            else
            {
                NomadLog.Error("NucleusProject.Base.Project.Tareas.TAREA.CrearTarjetaTrello() - " + strStep + " - Error al establecer el Token por la Columna (" + xmlTarjeta.FirstChild().GetAttr("id_columna_trello") + ")");
            }

            return idTrello;
        }

        private static string GetIdList(string valueList)
        {
            NomadXML tableros = GetTableros();
            string idList = "";
            NomadXML tablero = tableros.FirstChild().FindElement2("option", "value", valueList);
            if (tablero != null)
                idList = tablero.GetAttrString("id");                
            return idList;
        }

        public static void CrearTarjetaTrello(NucleusProject.Base.Project.Tareas.TAREA pobjTarea, Nomad.NSystem.Proxy.NomadXML pxmlTarjeta) 
        {
            NomadEnvironment.GetTrace().Info("TAREA: " + pobjTarea.ToString());
            NomadEnvironment.GetTrace().Info("TARJETA: " + pxmlTarjeta.ToString());
            string strStep = "";

            NomadTransaction objTran = new NomadTransaction();
            try
            {
                objTran.Begin();

                if (pobjTarea != null)
                {
                    strStep = "Creando Tarjeta Trello";
                    string url = "";
                    string c_trello = CrearTarjetaTrello(pxmlTarjeta, pobjTarea, ref url);
                    if (c_trello != "")
                    {
                        pobjTarea.c_externo = c_trello;
                        pobjTarea.d_enlace = url;
                    }
                        
                }

                strStep = "Grabando Tarea";
                objTran.SaveRefresh(pobjTarea);

                objTran.Commit();
            }
            catch (Exception ex)
            {
                NomadException nmdEx = NomadException.NewInternalException("NucleusProject.Base.Project.Tareas.TAREA.Guardar()", ex);
                nmdEx.SetValue("step", strStep);
                objTran.Rollback();
                throw nmdEx;

            }
        }

        public static void ReporteTarjetasHechas(Nomad.NSystem.Proxy.NomadXML DOC)
        {
            ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072; //TLS 1.2

            NomadBatch objBatch = NomadBatch.GetBatch("Iniciando...", "Reporte Tarjetas");
            NomadLog.Info("DOC: " + DOC.ToString());
                        
            HttpWebRequest request = null;
            NomadXML xmlTablero;
            Search busqueda;

            List<Board> listTableros = new List<Board>();
            List<string> teamsTrello = new List<string>();
            List<string> teamsEliminar = new List<string>();

            string result = "";
            string textsearch = "";
            string pertenece = "";
            string comillas = "\"";
            
            string hecho = NomadEnvironment.QueryValue("ORG26_PARAMETROS", "d_valor", "c_parametro", "ColumnaHecho", "", false);
            if(hecho=="")
                hecho = "Hecho";

            objBatch.SetPro(0);
            
            DOC.FirstChild().SetAttr("fecha", DateTime.Now.ToString("dd/MM/yyyy HH:mm"));

            objBatch.SetPro(5);

            try
            {
                string tableros = DOC.FirstChild().GetAttr("listtableros");
                string archivadas = DOC.FirstChild().GetAttr("archivadas");

                if (archivadas == "")
                    archivadas = "0";

                string filter = "";
                if (archivadas == "0")
                    filter += "is:open";
                else
                    filter += "";

                DOC.FirstChild().SetAttr("mostrarArchivada", archivadas);

                Nomad.Base.Login.Cuentas.CUENTA cta = new Nomad.Base.Login.Cuentas.CUENTA(NomadProxy.GetProxy().UserName);
                foreach (Nomad.Base.Login.Cuentas.USUARIO_GRUPO gr in cta.GRUPOS)
                {
                    if (gr.GRUPO.Contains("TRE"))
                    {
                        teamsTrello.Add(gr.GRUPO.Replace("TRE", ""));
                        pertenece += gr.GRUPO.Replace("TRE", "") + ", ";
                    }
                }

                objBatch.Log("El usuario actual '" + NomadProxy.GetProxy().UserName + "' pertenece a los equipos Trello: " + pertenece.Substring(0, pertenece.Length - 2));

                //Valido que existan todas las credenciales de los team Trello definidas
                foreach (string team in teamsTrello)
                {
                    Token tok = new Token(team);
                    if (!(tok.key != null && tok.token != null))
                    {
                        objBatch.Wrn("No se definieron las credenciales de Trello para el equipo '" + team + "'.");
                        teamsEliminar.Add(team);
                        continue;
                    }
                }

                foreach (string team in teamsEliminar)
                {
                    teamsTrello.Remove(team);
                }

                objBatch.SetPro(15);

                if (tableros == "")
                {
                    foreach (string team in teamsTrello)
                    {
                        Token tok = new Token(team);
                        if (tok.key != null && tok.token != null)
                        {
                            string appkey = tok.key;
                            string token = tok.token;

                            //Busco todos los tableros del equipo
                            request = (HttpWebRequest)WebRequest.Create(@"https://api.trello.com/1/organizations/" + team + "/boards?filter=all&fields=name%2Cid&key=" + appkey + "&token=" + token);
                            request.ContentType = "application/json";
                            request.Method = "GET";

                            result = "";
                            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                            using (StreamReader streamReader = new StreamReader(response.GetResponseStream()))
                            {
                                result = streamReader.ReadToEnd();                                
                                listTableros = StringUtil.JSON2Object<List<Board>>(result);
                            }
                                                        
                            if (listTableros.Count > 0)
                            {
                                foreach (Board b in listTableros)
                                {
                                    xmlTablero = new NomadXML("TABLERO");
                                    xmlTablero.SetAttr("nombre", b.name);
                                    
                                    //Busco el tablero b para obtener todas las columnas que lo componen
                                    textsearch = "board:" + comillas + b.name + comillas + " list:" + comillas + hecho + comillas + filter;
                                    request = (HttpWebRequest)WebRequest.Create(@"https://api.trello.com/1/search?query=" + textsearch + "&modelTypes=cards&cards_fields=id,name,idMembers,labels,closed,due,dateLastActivity,desc&cards_limit=1000&key=" + appkey + "&token=" + token);
                                    request.ContentType = "application/json";
                                    request.Method = "GET";

                                    result = "";
                                    using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                                    using (StreamReader streamReader = new StreamReader(response.GetResponseStream()))
                                    {
                                        result = streamReader.ReadToEnd();
                                        busqueda = StringUtil.JSON2Object<Search>(result);
                                    }

                                    //Valido que haya encontrado alguna tarjeta
                                    if (busqueda.cards.Count > 0)
                                    {
                                        Dictionary<string, Member> miembrosTablero = GetAllMiembros(b.id, appkey, token, objBatch);

                                        //Recorro todas las tarjetas encontradas
                                        foreach (Card c in busqueda.cards)
                                        {
                                            NomadXML xmlTarjeta = new NomadXML("TARJETA");
                                            xmlTarjeta.SetAttr("nombre", c.name);

                                            if (archivadas == "1")
                                            {
                                                xmlTarjeta.SetAttr("mostrarArchivada", "1");
                                                if (c.closed)
                                                    xmlTarjeta.SetAttr("archivada", "1");
                                                else
                                                    xmlTarjeta.SetAttr("archivada", "0");
                                            }
                                            else
                                                xmlTarjeta.SetAttr("mostrarArchivada", "0");

                                            string labels = "";
                                            foreach (Label lab in c.labels)
                                            {
                                                labels += lab.name + ", ";
                                            }
                                            labels = labels.Trim();
                                            if (labels.Length > 0)
                                            {
                                                if (labels.Substring(labels.Length - 1) == ",")
                                                    labels = labels.Substring(0, labels.Length - 1);
                                            }
                                            xmlTarjeta.SetAttr("labels", labels);

                                            if (c.due != null && c.due.HasValue)
                                            {
                                                xmlTarjeta.SetAttr("f_vto", c.due.Value.ToString("dd/MM/yyyy HH:mm"));
                                            }

                                            string miembros = "";
                                            foreach (string m in c.idMembers)
                                            {
                                                if (miembrosTablero.ContainsKey(m))
                                                    miembros += "[" + miembrosTablero[m].initials + "] - " + miembrosTablero[m].fullName + ", ";
                                            }

                                            miembros = miembros.Trim();
                                            if (miembros.Length > 0)
                                            {
                                                if (miembros.Substring(miembros.Length - 1) == ",")
                                                    miembros = miembros.Substring(0, miembros.Length - 1);
                                            }

                                            xmlTarjeta.SetAttr("usuario", miembros);

                                            xmlTarjeta.SetAttr("desc", c.desc);

                                            xmlTablero.AddTailElement(xmlTarjeta);
                                        }

                                        DOC.FirstChild().AddTailElement(xmlTablero);
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    foreach (string team in teamsTrello)
                    {
                        Token tok = new Token(team);
                        if (tok.key != null && tok.token != null)
                        {
                            string[] listaTableros = tableros.Split(';');
                            foreach (string tab in listaTableros)
                            {
                                if (tab.Trim() != "")
                                {
                                    //Busco el tablero
                                    xmlTablero = new NomadXML("TABLERO");
                                    xmlTablero.SetAttr("nombre", tab.Trim());

                                    //Busco el tablero b para obtener todas las columnas que lo componen
                                    textsearch = "board:" + comillas + tab.Trim() + comillas + " list:" + comillas + hecho + comillas + filter;
                                    request = (HttpWebRequest)WebRequest.Create(@"https://api.trello.com/1/search?query=" + textsearch + "&modelTypes=cards&cards_fields=id,name,idMembers,labels,closed,due,dateLastActivity,desc&cards_limit=1000&key=" + tok.key + "&token=" + tok.token);
                                    request.ContentType = "application/json";
                                    request.Method = "GET";

                                    result = "";
                                    using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                                    using (StreamReader streamReader = new StreamReader(response.GetResponseStream()))
                                    {
                                        result = streamReader.ReadToEnd();
                                        busqueda = StringUtil.JSON2Object<Search>(result);
                                    }

                                    //Valido que haya encontrado alguna tarjeta
                                    if (busqueda.cards.Count > 0)
                                    {
                                        Dictionary<string, Member> miembrosTablero = GetAllMiembros(busqueda.cards[0].idBoard, tok.key, tok.token, objBatch);

                                        //Recorro todas las tarjetas encontradas
                                        foreach (Card c in busqueda.cards)
                                        {
                                            NomadXML xmlTarjeta = new NomadXML("TARJETA");
                                            xmlTarjeta.SetAttr("nombre", c.name);

                                            if (archivadas == "1")
                                            {
                                                xmlTarjeta.SetAttr("mostrarArchivada", "1");
                                                if (c.closed)
                                                    xmlTarjeta.SetAttr("archivada", "1");
                                                else
                                                    xmlTarjeta.SetAttr("archivada", "0");
                                            }
                                            else
                                                xmlTarjeta.SetAttr("mostrarArchivada", "0");

                                            string labels = "";
                                            foreach (Label lab in c.labels)
                                            {
                                                labels += lab.name + ", ";
                                            }
                                            labels = labels.Trim();
                                            if (labels.Length > 0)
                                            {
                                                if (labels.Substring(labels.Length - 1) == ",")
                                                    labels = labels.Substring(0, labels.Length - 1);
                                            }
                                            xmlTarjeta.SetAttr("labels", labels);

                                            if (c.due != null && c.due.HasValue)
                                            {
                                                xmlTarjeta.SetAttr("f_vto", c.due.Value.ToString("dd/MM/yyyy HH:mm"));
                                            }

                                            string miembros = "";
                                            foreach (string m in c.idMembers)
                                            {
                                                if (miembrosTablero.ContainsKey(m))
                                                    miembros += "[" + miembrosTablero[m].initials + "] - " + miembrosTablero[m].fullName + ", ";
                                            }

                                            miembros = miembros.Trim();
                                            if (miembros.Length > 0)
                                            {
                                                if (miembros.Substring(miembros.Length - 1) == ",")
                                                    miembros = miembros.Substring(0, miembros.Length - 1);
                                            }

                                            xmlTarjeta.SetAttr("usuario", miembros);

                                            xmlTarjeta.SetAttr("desc", c.desc);

                                            xmlTablero.AddTailElement(xmlTarjeta);
                                        }

                                        DOC.FirstChild().AddTailElement(xmlTablero);
                                    }
                                }
                            }
                        }
                    }

                }
                
            }
            catch (WebException ex)
            {
                if (ex.Status != WebExceptionStatus.Timeout && ex.Status != WebExceptionStatus.SendFailure)
                {
                    System.Net.HttpWebResponse WResponse = (System.Net.HttpWebResponse)ex.Response;

                    result = "";
                    using (StreamReader streamReader = new StreamReader(WResponse.GetResponseStream()))
                    {
                        result = streamReader.ReadToEnd();
                    }

                    objBatch.Err("Error durante el reporte - Causa: " + result);

                }
                else
                {
                    objBatch.Err("Error durante el reporte - Causa: " + ex.Message);
                }
            }
            catch (Exception ex)
            {
                objBatch.Err("Error durante el reporte - Causa: " + ex.Message);
            }

            objBatch.SetPro(65);

            objBatch.SetMess("Ejecutando el Reporte...");
            NomadBatch.Trace(DOC.ToString());
            Nomad.NomadHTML nmdHtml = new Nomad.NomadHTML("NucleusProject.Base.Project.TarjetasHechas.rpt", DOC.ToString());

            string outFileName = NomadProxy.GetProxy().Batch().ID + ".htm";
            string outFilePath = NomadProxy.GetProxy().RunPath + "Nomad\\TEMP\\";
            NomadBatch.Trace("Generando Reporte HTML en path:'" + outFilePath + outFileName + "'");

            objBatch.SetPro(85);

            StreamWriter sw = new System.IO.StreamWriter(outFilePath + "\\" + outFileName, false, System.Text.Encoding.UTF8);

            nmdHtml.GenerateHTML(sw);

            sw.Close();
        }

        public static void ArchivarTareas()
        {
            ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072; //TLS 1.2
            int ArchivadasTot, Archivadas, Errores, ErroresTot, ErroresCols;
            NomadBatch objBatch = NomadBatch.GetBatch("Iniciando...", "Archivador");
            Token tok = null;
            HttpWebRequest request = null;
            string result;
            Search busqueda;

            //V1
            //Metodo archiva tarjetas en trello que este vinculadas con alguna tarea en BaytonTecRH que se encuentre en estado Finalizadas o Anulada.
            //Tenemos que ver cómo determinar cuándo una tarea fue o no archivada en Trello, para evitar repetir el proceso.
            //Si se elimina, se elimina completamente y no habrá posibilidad de recuperarla
            //NucleusProject.Base.Project.Tareas.TAREA objRead = null;

            //V2
            //Se archivaran TODAS las tarjetas de Trello que tengan el mismo que nombre que todas las tareas finalizadas en BaytonTec
            //Cantidad maxima de tarjetas a archivar (definido por parametro)
            //0: TODOS
            //!=0: ESA CANTIDAD
            //VACIO/NO DEFINIDA: No hace nada
            
            //V3 APLICA SOLO A TRELLOADMIN
            //Se van a archivar todas las tarjetas ubicadas en el parametro "ColumnasArchivar" de formato [CODIGO] separados por ";"
            //Las columnas en todos los tableros serán: “[TE] Testing”, “[IN] A instalar” y “[HE] Hecho” por ahora
            //Por ejemplo en el parametro se guardará: [HE];[AI]
            //Esto significa que se se archivaran todas las tarjetas de las tareas finalizadas/anuladas ubicadas en las columnas Hechas y A instalar
            //De las tarjetas se recuperará el nombre (con formato 'c_tarea - d_tarea') y el id de trello
            //c_tarea debera ser de formato 'AÑO_INCREMENTAL NUMERO': dos partes, y la primera parte deberá ser un año.
            //Por cada nombre recupero el c_tarea teorico, los agrego a un diccionario: (c_tarea_teorico, id_trello)
            //Consulto sobre los tareas en BaytonTec con esa c_tarea que esten 'A' o 'F', las que recupere: se archivo.
            //Es decir, por cada tarea encontrada en el qry en BaytonTecRH, por el id_trello, archivo.
            //regex validar formato 'AÑO_4DIGITOS NUMERO' > ^[0-9]{4}(\s[0-9]{3,5})?$
            
            //V3.1 FUNCIONALIDAD QUE SE APLICA SOLO CON USUARIOS LOGUEADOS ASIGNADO A GRUPOS (NO CON CUALQUIERA)            
            //Mantengo funcionalidad V3 pero agrego equipos de trello para crear el token
            //List<string> teamsTrello = new List<string>();
            //Nomad.Base.Login.Cuentas.CUENTA cta = new Nomad.Base.Login.Cuentas.CUENTA(NomadProxy.GetProxy().UserName);

            /*foreach (Nomad.Base.Login.Cuentas.USUARIO_GRUPO gr in cta.GRUPOS)
            {
                if (gr.GRUPO.Contains("TRE"))
                    teamsTrello.Add(gr.GRUPO.Replace("TRE", ""));
            }*/

            string cantMax = NomadEnvironment.QueryValue("ORG26_PARAMETROS", "d_valor", "c_parametro", "CantMaxArchivar", "", false);
            string columnas = NomadEnvironment.QueryValue("ORG26_PARAMETROS", "d_valor", "c_parametro", "ColumnasArchivar", "", false);

            NomadLog.Info("'CantMaxArchivar': " + cantMax);
            NomadLog.Info("'ColumnasArchivar': " + columnas);
            
            //NomadLog.Info("Ejecuto archivador para todos los equipos que pertenece el usuario actual");
            //foreach(string team in teamsTrello)
            {
                //NomadLog.Info("Team: " + team);
                ArchivadasTot = 0;
                Archivadas = 0;
                Errores = 0;
                ErroresTot = 0;
                ErroresCols = 0;

                //tok = new Token(team);
                tok = new Token();

                //V3
                if (columnas != "")
                {
                    string[] cols = columnas.Split(';');
                    if (cols.Length > 0)
                    {
                        if (cantMax != "")
                        {
                            objBatch.SetPro(10);

                            if (int.Parse(cantMax) > 0)
                                objBatch.SetMess("Cantidad máxima a archivar: " + cantMax);
                            else
                                objBatch.SetMess("Se archivarán todas las tarjetas");

                            objBatch.SetMess("Recorriendo columnas");

                            for (int i = 0; i < cols.Length; i++)
                            {
                                if (cols[i] != "")
                                {
                                    try
                                    {
                                        Archivadas = 0;
                                        string textsearch = "list:" + cols[i].ToUpper() + " is:open";
                                        request = (HttpWebRequest)WebRequest.Create(@"https://api.trello.com/1/search?query=" + textsearch + "&modelTypes=cards&cards_limit=1000&card_fields=id,name&key=" + tok.key + "&token=" + tok.token);
                                        request.ContentType = "application/json";
                                        request.Method = "GET";

                                        result = "";
                                        using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                                        using (StreamReader streamReader = new StreamReader(response.GetResponseStream()))
                                        {
                                            result = streamReader.ReadToEnd();
                                            //Dictionary<object, object> dic = StringUtil.JSON2Object<Dictionary<object, object>>(result);
                                            busqueda = StringUtil.JSON2Object<Search>(result);
                                        }

                                        Dictionary<string, string> dicTarjetas = new Dictionary<string, string>();

                                        foreach (Card c in busqueda.cards)
                                        {
                                            string[] arrName = c.name.Split('-');
                                            if (arrName.Length > 1)
                                            {
                                                string code = arrName[0].TrimStart().TrimEnd();
                                                System.Text.RegularExpressions.Match match = System.Text.RegularExpressions.Regex.Match(code, @"^[0-9]{4}(\s[0-9]{3,5})?$", System.Text.RegularExpressions.RegexOptions.IgnoreCase);

                                                if (match.Success)
                                                {
                                                    if (!dicTarjetas.ContainsKey(c.id))
                                                        dicTarjetas.Add(code, c.id);
                                                }
                                            }
                                        }

                                        string[] c_tarea_in = new string[dicTarjetas.Values.Count];
                                        int j = 0;
                                        foreach (string valor in dicTarjetas.Keys)
                                        {
                                            c_tarea_in[j] = "\\'" + valor + "\\'";
                                            ++j;
                                        }

                                        string c_tarea_where = String.Join(",", c_tarea_in);
                                        string where = "PRY02_TAREAS.c_tarea IN (" + c_tarea_where + ")";
                                        where += " AND PRY02_TAREAS.c_estado IN (\\'A\\',\\'F\\')";
                                        string campos = "PRY02_TAREAS.c_tarea";

                                        NomadXML resultadoQuery = NomadQuery("PRY02_TAREAS", where, campos, "", "");

                                        if (resultadoQuery != null)
                                        {
                                            int nro;
                                            if (int.TryParse(resultadoQuery.FirstChild().GetAttr("CNT"), out nro))
                                            {
                                                if (int.Parse(resultadoQuery.FirstChild().GetAttr("CNT")) > 0)
                                                {
                                                    ArrayList lista = (ArrayList)resultadoQuery.FirstChild().GetElements("ROW");
                                                    objBatch.SetMess("Archivando tarjetas");
                                                    for (int l = 0; l < lista.Count; l++)
                                                    {

                                                        NomadXML row = (NomadXML)lista[l];
                                                        string id_trello = dicTarjetas[row.GetAttrString("c_tarea")];
                                                        if (id_trello != "")
                                                        {
                                                            try
                                                            {
                                                                //ARCHIVA TARJETA
                                                                request = (HttpWebRequest)WebRequest.Create(@"https://api.trello.com/1/cards/" + id_trello + "?closed=true&key=" + tok.key + "&token=" + tok.token);
                                                                request.ContentType = "application/json";
                                                                request.Method = "PUT";
                                                                result = "";

                                                                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                                                                using (StreamReader streamReader = new StreamReader(response.GetResponseStream()))
                                                                {
                                                                    result = streamReader.ReadToEnd();
                                                                }

                                                                if (result != "")
                                                                {
                                                                    Archivadas++;
                                                                    ArchivadasTot++;
                                                                    objBatch.SetMess("Se archivó correctamente la tarjeta vinculada a la tarea '" + row.GetAttrString("c_tarea") + "'.");
                                                                    objBatch.Log("Se archivó correctamente la tarjeta vinculada a la tarea '" + row.GetAttrString("c_tarea") + "'.");
                                                                }

                                                                if (Archivadas == int.Parse(cantMax))
                                                                    break;
                                                            }
                                                            catch (WebException ex)
                                                            {
                                                                if (ex.Status != WebExceptionStatus.Timeout && ex.Status != WebExceptionStatus.SendFailure)
                                                                {
                                                                    System.Net.HttpWebResponse WResponse = (System.Net.HttpWebResponse)ex.Response;

                                                                    result = "";
                                                                    using (StreamReader streamReader = new StreamReader(WResponse.GetResponseStream()))
                                                                    {
                                                                        result = streamReader.ReadToEnd();
                                                                    }

                                                                    objBatch.Err("Error en la Columna '" + cols[i] + "' - Tarjeta '" + row.GetAttrString("c_tarea") + "' - Causa: " + result);
                                                                    Errores++;
                                                                    ErroresTot++;
                                                                    continue;
                                                                }
                                                                else
                                                                {
                                                                    objBatch.Err("Error en la Columna '" + cols[i] + "' - Tarjeta '" + row.GetAttrString("c_tarea") + "' - Causa: " + result);
                                                                    Errores++;
                                                                    ErroresTot++;
                                                                    continue;
                                                                }
                                                            }
                                                            catch (Exception e)
                                                            {
                                                                objBatch.Err("Error en la Columna '" + cols[i] + "' - Tarjeta '" + row.GetAttrString("c_tarea") + "' - Causa: " + e.Message);
                                                                Errores++;
                                                                ErroresTot++;
                                                                continue;
                                                            }
                                                        }
                                                    }
                                                }
                                            }

                                            objBatch.SetMess("Se han archivado " + (Archivadas - Errores) + " tarjetas de la columna '" + cols[i] + "'.");
                                            objBatch.Log("Se han archivado " + (Archivadas - Errores) + " tarjetas de la columna '" + cols[i] + "'.");

                                            if (Archivadas == int.Parse(cantMax))
                                                break;
                                        }
                                    }
                                    catch (WebException ex)
                                    {
                                        if (ex.Status != WebExceptionStatus.Timeout && ex.Status != WebExceptionStatus.SendFailure)
                                        {
                                            System.Net.HttpWebResponse WResponse = (System.Net.HttpWebResponse)ex.Response;

                                            result = "";
                                            using (StreamReader streamReader = new StreamReader(WResponse.GetResponseStream()))
                                            {
                                                result = streamReader.ReadToEnd();
                                            }

                                            objBatch.Err("Error desconocido en la columna " + cols[i] + " - " + result);
                                            ErroresCols++;
                                            continue;
                                        }
                                        else
                                        {
                                            objBatch.Err("Error desconocido en la columna " + cols[i] + " - " + ex.Message);
                                            Errores++;
                                            continue;
                                        }
                                    }
                                    catch (Exception e)
                                    {
                                        objBatch.Err("Error desconocido - " + e.Message);
                                        Errores++;
                                        continue;
                                    }
                                }
                            }
                        }
                        else
                            objBatch.Err("No se definió el parámetro 'CantMaxArchivar'");
                    }
                    else
                        objBatch.Err("No se definió el parámetro 'CantMaxArchivar'");
                }
                else
                    objBatch.Err("No se definió el parámetro 'ColumnasArchivar'");

                //objBatch.Log("Team " + team);
                objBatch.Log("Se han archivado: " + ArchivadasTot);
                objBatch.Log("No se han archivado: " + ErroresTot);

            }
            
            objBatch.Log("Finalizado...");

            #region VX
            //V2
            /*if (cantMax != "")
            {
                if (int.Parse(cantMax) > 0)
                    objBatch.SetMess("Cantidad máxima a archivar: " + cantMax);
                else
                    objBatch.SetMess("Se archivarán todas las tarjetas");

                //Recupero tareas finalizadas y/o anuladas que estén vinculadas en Trello
                //V1
                //NomadXML IDList = Nomad.NSystem.Base.NomadEnvironment.QueryNomadXML(NucleusProject.Base.Project.Tareas.TAREA.Resources.GET_TARJETASFOA_TRELLO, "");
                //V2
                NomadXML IDList = Nomad.NSystem.Base.NomadEnvironment.QueryNomadXML(NucleusProject.Base.Project.Tareas.TAREA.Resources.GET_TAREAS_TERMINADAS, "");

                ArrayList lista = (ArrayList)IDList.FirstChild().GetElements("ROW");
                HttpWebRequest request = null;
                string result;

                for (int xml = 0; xml < lista.Count; xml++)
                {
                    NomadXML row = (NomadXML)lista[xml];
                    objBatch.SetPro(0, 100, lista.Count, xml);
                    objBatch.SetMess("Archivando tarjeta " + (xml + 1) + " de " + lista.Count);

                    //Inicio la Transaccion
                    try
                    {*/
                        #region V1
                        /*
                    Card tarjeta = null;
                    objRead = NucleusProject.Base.Project.Tareas.TAREA.Get(row.GetAttr("id"));

                    //Recupero tarjeta para validar si ya esta archivada (si no la encuentra en el catch valido que si no existe mas entonces no hace nada)
                    request = (HttpWebRequest)WebRequest.Create(@"https://api.trello.com/1/cards/" + objRead.c_externo + "?fields=closed,name&key=" + tok.key + "&token=" + tok.token);
                    request.ContentType = "application/json";
                    request.Method = "GET";

                    result = "";
                    using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                    using (StreamReader streamReader = new StreamReader(response.GetResponseStream()))
                    {
                        result = streamReader.ReadToEnd();
                    }

                    tarjeta = StringUtil.JSON2Object<Card>(result);

                    if (!tarjeta.closed)
                    {
                        //ARCHIVA
                        request = (HttpWebRequest)WebRequest.Create(@"https://api.trello.com/1/cards/" + objRead.c_externo + "?closed=true&key=" + tok.key + "&token=" + tok.token);
                        request.ContentType = "application/json";
                        request.Method = "PUT";

                        //ELIMINA (IRREVERSIBLE)
                        //request = (HttpWebRequest)WebRequest.Create(@"https://api.trello.com/1/cards/" + objRead.c_externo + "?&key=" + tok.key + "&token=" + tok.token);
                        //request.ContentType = "application/json";
                        //request.Method = "DELETE";

                        result = "";
                        using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                        using (StreamReader streamReader = new StreamReader(response.GetResponseStream()))
                        {
                            result = streamReader.ReadToEnd();
                        }

                        if (result != "")
                        {
                            Archivadas++;
                            objBatch.Log("Se archivó correctamente la tarjeta '" + tarjeta.name + "' vinculada a la tarea: '" + objRead.c_tarea + " - " + objRead.d_tarea + "'.");
                        }
                    }
                    else
                    {
                        //ESTO SE PUEDE SACAR PORQUE SIEMPRE LO MOSTRARÁ Y CAPAZ CONFUNDA AL USUARIO
                        objBatch.Log("La tarjeta '" + tarjeta.name + "' vinculada a la tarea: '" + objRead.c_tarea + " - " + objRead.d_tarea + "' ya se encuentra archivada de Trello.");
                    }
                    */
                        #endregion
                                    
                        /*
                        #region V2
                        Search busqueda;
                        string code = row.GetAttr("c_tarea");
                        string[] codigo_tarea = code.Split(' ');

                        //int nro;
                        //if (int.TryParse(codigo_tarea[0], out nro))
                        if (codigo_tarea.Length > 1)
                        {
                            //Recupero tarjeta para validar si ya esta archivada (si no la encuentra en el catch valido que si no existe mas entonces no hace nada)
                            request = (HttpWebRequest)WebRequest.Create(@"https://api.trello.com/1/search?query=" + code + "&modelTypes=cards&card_fields=id,closed,name&key=" + tok.key + "&token=" + tok.token);
                            request.ContentType = "application/json";
                            request.Method = "GET";

                            result = "";
                            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                            using (StreamReader streamReader = new StreamReader(response.GetResponseStream()))
                            {
                                result = streamReader.ReadToEnd();
                                //Dictionary<object, object> dic = StringUtil.JSON2Object<Dictionary<object, object>>(result);
                                busqueda = StringUtil.JSON2Object<Search>(result);
                            }

                            if (busqueda != null)
                            {
                                if (cantMax != "")
                                {
                                    if (int.Parse(cantMax) > 0 && Archivadas == int.Parse(cantMax))
                                        break;
                                }
                                else
                                    break;

                                foreach (Card c in busqueda.cards)
                                {
                                    if (c.name.Contains(code))
                                    {
                                        if (!c.closed)
                                        {
                                            //ARCHIVA TARJETA
                                            request = (HttpWebRequest)WebRequest.Create(@"https://api.trello.com/1/cards/" + c.id + "?closed=true&key=" + tok.key + "&token=" + tok.token);
                                            request.ContentType = "application/json";
                                            request.Method = "PUT";
                                            result = "";

                                            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                                            using (StreamReader streamReader = new StreamReader(response.GetResponseStream()))
                                            {
                                                result = streamReader.ReadToEnd();
                                            }

                                            if (result != "")
                                            {
                                                Archivadas++;
                                                objBatch.Log("Se archivó correctamente la tarjeta '" + c.name + "' vinculada a la tarea: '" + row.GetAttr("c_tarea") + " - " + row.GetAttr("d_tarea") + "'.");
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        #endregion
                    }
                    catch (WebException ex)
                    {
                        if (ex.Status != WebExceptionStatus.Timeout && ex.Status != WebExceptionStatus.SendFailure)
                        {
                            System.Net.HttpWebResponse WResponse = (System.Net.HttpWebResponse)ex.Response;

                            result = "";
                            using (StreamReader streamReader = new StreamReader(WResponse.GetResponseStream()))
                            {
                                result = streamReader.ReadToEnd();
                            }

                            //SI LO ENCUENTRA, SINO CONTINUA
                            if (!result.Contains("was not found"))
                            {
                                objBatch.Err("Error desconocido - " + result);
                                Errores++;
                            }
                            else
                            {
                                //ESTO SE PUEDE SACAR PORQUE SIEMPRE LO MOSTRARÁ Y CAPAZ CONFUNDA AL USUARIO
                                objBatch.Wrn("No se encuentra la tarjeta vinculada a la tarea: '" + row.GetAttr("c_tarea") + " - " + row.GetAttr("d_tarea") + "' porque ha sido eliminada de Trello.");
                            }

                            continue;
                        }
                        else
                        {
                            objBatch.Err("Error desconocido - " + ex.Message);
                            Errores++;
                            continue;
                        }
                    }
                    catch (Exception e)
                    {
                        objBatch.Err("Error desconocido - " + e.Message);
                        Errores++;
                        continue;
                    }
                }
            }
            else
                objBatch.Err("No se definió el parámetro 'CantMaxArchivar'");
                        
            objBatch.Log("Tarjetas Archivadas: " + (Archivadas - Errores).ToString());
            objBatch.Log("Finalizado...");*/

        #endregion
        }
        
        public static void ReporteTarjetas( Nomad.NSystem.Proxy.NomadXML DOC)
        {
            NomadBatch objBatch;
            HttpWebRequest request = null;
            objBatch = NomadBatch.GetBatch("Iniciando...", "Reporte Tarjetas");
            NomadLog.Info("DOC: " + DOC.ToString());
            
            string result = "";
            string pertenece = "";            
            
            Search busqueda;
            
            List<string> teamsTrello = new List<string>();
            List<string> teamsEliminar = new List<string>();

            NomadXML xmlTab = new NomadXML("TABLERO");

            objBatch.SetPro(0);

            ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072; //TLS 1.2

            DOC.FirstChild().SetAttr("fecha", DateTime.Now.ToString("dd/MM/yyyy HH:mm"));

            objBatch.SetPro(5);

            try
            {
                string tableros = DOC.FirstChild().GetAttr("listtableros");
                string columnas = DOC.FirstChild().GetAttr("listcolumnas");                
                string archivadas = DOC.FirstChild().GetAttr("archivadas");
                if (archivadas == "")
                    archivadas = "0";
                string descripcion = DOC.FirstChild().GetAttr("detalle");
                
                string filter = "";
                if (archivadas == "0")
                    filter += ""; 
                else
                    filter += "filter=all";

                DOC.FirstChild().SetAttr("mostrarArchivada", archivadas);
                
                Nomad.Base.Login.Cuentas.CUENTA cta = new Nomad.Base.Login.Cuentas.CUENTA(NomadProxy.GetProxy().UserName);
                foreach (Nomad.Base.Login.Cuentas.USUARIO_GRUPO gr in cta.GRUPOS)
                {
                    if (gr.GRUPO.Contains("TRE"))
                    {
                        teamsTrello.Add(gr.GRUPO.Replace("TRE", ""));
                        pertenece += gr.GRUPO.Replace("TRE", "") + ", ";
                    }
                }

                objBatch.Log("El usuario actual '" + NomadProxy.GetProxy().UserName + "' pertenece a los equipos Trello: " + pertenece.Substring(0, pertenece.Length - 2));

                //Valido que existan todas las credenciales de los team Trello definidas
                foreach (string team in teamsTrello)
                {
                    Token tok = new Token(team);
                    if (!(tok.key != null && tok.token != null))
                    {
                        objBatch.Wrn("No se definieron las credenciales de Trello para el equipo '" + team + "'.");
                        teamsEliminar.Add(team);
                        continue;
                    }
                }

                foreach (string team in teamsEliminar)
                {
                    teamsTrello.Remove(team);
                }

                objBatch.SetPro(15);

                foreach(string team in teamsTrello)
                {
                    Token tok = new Token(team);

                    if (tok != null & tableros != "")
                    {
                        string token = tok.token;
                        string appkey = tok.key;

                        string[] tabs = tableros.Split(';');

                        for (int i = 0; i < tabs.Length; i++)
                        {
                            string tabActual = "";
                            if (tabs[i] != "")
                            {
                                if (columnas != "")
                                {
                                    string[] cols = columnas.Split(';');
                                    //buscar tablero
                                    string textsearch = tabs[i];
                                    request = (HttpWebRequest)WebRequest.Create(@"https://api.trello.com/1/search?query=" + textsearch + "&modelTypes=boards&board_fields=id,name&key=" + appkey + "&token=" + token);
                                    request.ContentType = "application/json";
                                    request.Method = "GET";

                                    result = "";
                                    using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                                    using (StreamReader streamReader = new StreamReader(response.GetResponseStream()))
                                    {
                                        result = streamReader.ReadToEnd();
                                        //Dictionary<object, object> dic = StringUtil.JSON2Object<Dictionary<object, object>>(result);
                                        busqueda = StringUtil.JSON2Object<Search>(result);
                                    }

                                    if (busqueda.boards.Count > 0)
                                    {
                                        foreach (Board b in busqueda.boards)
                                        {
                                            Dictionary<string, CustomField> customFields = GetAllCustomFields(b.id, appkey, token, objBatch);
                                            Dictionary<string, Member> miembrosTablero = GetAllMiembros(b.id, appkey, token, objBatch);

                                            for (int j = 0; j < cols.Length; j++)
                                            {
                                                if (cols[j] != "")
                                                {
                                                    Board boardAct;
                                                    request = (HttpWebRequest)WebRequest.Create(@"https://api.trello.com/1/boards/" + b.id + "?fields=id&lists=open&key=" + appkey + "&token=" + token);
                                                    request.ContentType = "application/json";
                                                    request.Method = "GET";

                                                    result = "";
                                                    using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                                                    using (StreamReader streamReader = new StreamReader(response.GetResponseStream()))
                                                    {
                                                        result = streamReader.ReadToEnd();
                                                        //Dictionary<object, object> dic = StringUtil.JSON2Object<Dictionary<object, object>>(result);
                                                        boardAct = StringUtil.JSON2Object<Board>(result);
                                                    }

                                                    Lista listAct = null;
                                                    foreach (Lista l in boardAct.lists)
                                                    {
                                                        if (l.name == cols[j])
                                                        {
                                                            listAct = l;
                                                            break;
                                                        }
                                                    }

                                                    if (listAct != null)
                                                    {
                                                        List<Card> tarjetas = new List<Card>();

                                                        request = (HttpWebRequest)WebRequest.Create(@"https://api.trello.com/1/lists/" + listAct.id + "/cards?" + filter + "&fields=id,name,idMembers,labels,closed,due,dateLastActivity,desc&customFieldItems=true&key=" + appkey + "&token=" + token);
                                                        request.ContentType = "application/json";
                                                        request.Method = "GET";

                                                        result = "";
                                                        using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                                                        using (StreamReader streamReader = new StreamReader(response.GetResponseStream()))
                                                        {
                                                            result = streamReader.ReadToEnd();
                                                            //Dictionary<object, object> dic = StringUtil.JSON2Object<Dictionary<object, object>>(result);
                                                            tarjetas = StringUtil.JSON2Object<List<Card>>(result);
                                                        }

                                                        if (tabActual == "")
                                                        {
                                                            xmlTab = new NomadXML("TABLERO");
                                                            xmlTab.SetAttr("nombre", tabs[i]);
                                                            tabActual = tabs[i];

                                                            NomadXML xmlCustomFields = new NomadXML("CUSTOMFIELDS");
                                                            int r = 1;
                                                            foreach (string key in customFields.Keys)
                                                            {
                                                                NomadXML xmlCustom = new NomadXML("CUSTOMFIELD");
                                                                xmlCustom.SetAttr("nombre", customFields[key].name);
                                                                xmlCustom.SetAttr("campo", "campo" + r);
                                                                customFields[key].campo = "campo" + r;
                                                                xmlCustomFields.AddTailElement(xmlCustom);
                                                                r++;
                                                            }
                                                            xmlTab.AddTailElement(xmlCustomFields);
                                                        }

                                                        NomadXML xmlcol = new NomadXML("COLUMNA");
                                                        xmlcol.SetAttr("nombre", listAct.name);
                                                        if (archivadas == "1")
                                                            xmlcol.SetAttr("mostrarArchivada", "1");
                                                        else
                                                            xmlcol.SetAttr("mostrarArchivada", "0");

                                                        foreach (Card c in tarjetas)
                                                        {
                                                            NomadXML xmlTar = new NomadXML("TARJETA");
                                                            xmlTar.SetAttr("nombre", c.name);

                                                            //xmlTar.SetAttr("desc", c.desc);

                                                            string miembros = "";
                                                            foreach (string m in c.idMembers)
                                                            {
                                                                if (miembrosTablero.ContainsKey(m))
                                                                    miembros += "[" + miembrosTablero[m].initials + "] - " + miembrosTablero[m].fullName + ", ";
                                                            }

                                                            miembros = miembros.Trim();
                                                            if (miembros.Length > 0)
                                                            {
                                                                if (miembros.Substring(miembros.Length - 1) == ",")
                                                                    miembros = miembros.Substring(0, miembros.Length - 1);
                                                            }

                                                            xmlTar.SetAttr("usuario", miembros);

                                                            if (archivadas == "1")
                                                            {
                                                                xmlTar.SetAttr("mostrarArchivada", "1");
                                                                if (c.closed)
                                                                    xmlTar.SetAttr("archivada", "1");
                                                                else
                                                                    xmlTar.SetAttr("archivada", "0");
                                                            }
                                                            else
                                                                xmlTar.SetAttr("mostrarArchivada", "0");

                                                            string labels = "";
                                                            foreach (Label lab in c.labels)
                                                            {
                                                                labels += lab.name + ", ";
                                                            }
                                                            labels = labels.Trim();
                                                            if (labels.Length > 0)
                                                            {
                                                                if (labels.Substring(labels.Length - 1) == ",")
                                                                    labels = labels.Substring(0, labels.Length - 1);
                                                            }
                                                            xmlTar.SetAttr("labels", labels);

                                                            if (c.due != null && c.due.HasValue)
                                                            {
                                                                xmlTar.SetAttr("f_vto", c.due.Value.ToString("dd/MM/yyyy HH:mm"));
                                                            }

                                                            if (c.dateLastActivity != null && c.dateLastActivity.HasValue)
                                                                xmlTar.SetAttr("f_ult_act", c.dateLastActivity.Value.ToString("dd/MM/yyyy HH:mm"));

                                                            xmlTar.SetAttr("desc", c.desc);

                                                            foreach (CustomField custom in customFields.Values)
                                                            {
                                                                string valor = "";                                                                
                                                                bool encontro = false;

                                                                string valTxt = "";
                                                                double? valDbl = null;
                                                                bool? valBool = null;
                                                                DateTime? valDate = null;

                                                                foreach (CustomFieldItem customitem in c.customFieldItems)
                                                                {
                                                                    if (custom.id == customitem.idCustomField)
                                                                    {
                                                                        encontro = true;
                                                                        if (custom.type == "checkbox")
                                                                            valor = customitem.value["checked"];
                                                                        else
                                                                            valor = customitem.value[custom.type];

                                                                        try
                                                                        {
                                                                            switch (custom.type)
                                                                            {
                                                                                case "text":
                                                                                    valTxt = valor;
                                                                                    xmlTar.SetAttr(custom.campo, valTxt);
                                                                                    break;
                                                                                case "date":
                                                                                    int year = int.Parse(valor.Substring(0, 4));
                                                                                    int month = int.Parse(valor.Substring(5, 2));
                                                                                    int day = int.Parse(valor.Substring(8, 2));
                                                                                    int hour = int.Parse(valor.Substring(11, 2));
                                                                                    int minutes = int.Parse(valor.Substring(14, 2));
                                                                                    valDate = new DateTime(year, month, day, hour, minutes, 0);
                                                                                    xmlTar.SetAttr(custom.campo, valDate.Value.ToString("dd/MM/yyyy HH:mm"));
                                                                                    break;
                                                                                case "checkbox":
                                                                                    valBool = StringUtil.JSON2Object<bool>(valor);
                                                                                    xmlTar.SetAttr(custom.campo, valBool.Value ? "Si" : "No");
                                                                                    break;
                                                                                case "number":
                                                                                    valDbl = StringUtil.JSON2Object<double>(valor);
                                                                                    xmlTar.SetAttr(custom.campo, valDbl.Value);
                                                                                    break;
                                                                                default:
                                                                                    valTxt = valor;
                                                                                    xmlTar.SetAttr(custom.campo, valTxt);
                                                                                    break;
                                                                            }

                                                                            break;
                                                                        }
                                                                        catch (Exception ex)
                                                                        {
                                                                            objBatch.Wrn("Error recuperando los campos personalizados - Causa: " + ex.Message);
                                                                            continue;
                                                                        }
                                                                    }
                                                                }

                                                                if (!encontro)
                                                                    xmlTar.SetAttr(custom.campo, "");
                                                            }

                                                            xmlcol.AddTailElement(xmlTar);
                                                        }

                                                        xmlTab.AddTailElement(xmlcol);

                                                        DOC.FirstChild().AddTailElement(xmlTab);
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    Board boardAct = new Board();

                                    string textsearch = tabs[i];
                                    request = (HttpWebRequest)WebRequest.Create(@"https://api.trello.com/1/search?query=" + textsearch + "&modelTypes=boards&board_fields=id,name&key=" + appkey + "&token=" + token);
                                    request.ContentType = "application/json";
                                    request.Method = "GET";

                                    result = "";
                                    using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                                    using (StreamReader streamReader = new StreamReader(response.GetResponseStream()))
                                    {
                                        result = streamReader.ReadToEnd();
                                        //Dictionary<object, object> dic = StringUtil.JSON2Object<Dictionary<object, object>>(result);
                                        busqueda = StringUtil.JSON2Object<Search>(result);
                                    }

                                    if (busqueda.boards.Count > 0)
                                    {
                                        foreach (Board b in busqueda.boards)
                                        {
                                            tabActual = "";

                                            Dictionary<string, CustomField> customFields = GetAllCustomFields(b.id, appkey, token, objBatch);
                                            Dictionary<string, Member> miembrosTablero = GetAllMiembros(b.id, appkey, token, objBatch);

                                            request = (HttpWebRequest)WebRequest.Create(@"https://api.trello.com/1/boards/" + b.id + "?fields=id&lists=open&key=" + appkey + "&token=" + token);
                                            request.ContentType = "application/json";
                                            request.Method = "GET";

                                            result = "";
                                            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                                            using (StreamReader streamReader = new StreamReader(response.GetResponseStream()))
                                            {
                                                result = streamReader.ReadToEnd();
                                                //Dictionary<object, object> dic = StringUtil.JSON2Object<Dictionary<object, object>>(result);
                                                boardAct = StringUtil.JSON2Object<Board>(result);
                                            }

                                            foreach (Lista l in boardAct.lists)
                                            {
                                                List<Card> tarjetas = new List<Card>();
                                                request = (HttpWebRequest)WebRequest.Create(@"https://api.trello.com/1/lists/" + l.id + "/cards?" + filter + "&fields=id,name,idMembers,labels,closed,due,dateLastActivity,desc&customFieldItems=true&key=" + appkey + "&token=" + token);
                                                request.ContentType = "application/json";
                                                request.Method = "GET";

                                                result = "";
                                                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                                                using (StreamReader streamReader = new StreamReader(response.GetResponseStream()))
                                                {
                                                    result = streamReader.ReadToEnd();
                                                    //Dictionary<object, object> dic = StringUtil.JSON2Object<Dictionary<object, object>>(result);
                                                    tarjetas = StringUtil.JSON2Object<List<Card>>(result);
                                                }

                                                if (tabActual == "")
                                                {
                                                    xmlTab = new NomadXML("TABLERO");
                                                    xmlTab.SetAttr("nombre", tabs[i]);
                                                    tabActual = tabs[i];

                                                    NomadXML xmlCustomFields = new NomadXML("CUSTOMFIELDS");
                                                    int r = 1;
                                                    foreach (string key in customFields.Keys)
                                                    {
                                                        NomadXML xmlCustom = new NomadXML("CUSTOMFIELD");
                                                        xmlCustom.SetAttr("nombre", customFields[key].name);
                                                        //xmlCustom.SetAttr("auxcampo", customFields[key].name.Replace(" ", "").Replace("ñ", "ni").Replace("Ñ", "NI"));
                                                        xmlCustom.SetAttr("campo", "campo" + r);
                                                        customFields[key].campo = "campo" + r;
                                                        xmlCustomFields.AddTailElement(xmlCustom);
                                                        r++;
                                                    }
                                                    xmlTab.AddTailElement(xmlCustomFields);
                                                }

                                                NomadXML xmlcol = new NomadXML("COLUMNA");
                                                xmlcol.SetAttr("nombre", l.name);
                                                if (archivadas == "1")
                                                    xmlcol.SetAttr("mostrarArchivada", "1");
                                                else
                                                    xmlcol.SetAttr("mostrarArchivada", "0");

                                                foreach (Card c in tarjetas)
                                                {
                                                    NomadXML xmlTar = new NomadXML("TARJETA");
                                                    xmlTar.SetAttr("nombre", c.name);

                                                    //xmlTar.SetAttr("desc", c.desc);

                                                    string miembros = "";
                                                    foreach (string m in c.idMembers)
                                                    {
                                                        if (miembrosTablero.ContainsKey(m))
                                                            miembros += "[" + miembrosTablero[m].initials + "] - " + miembrosTablero[m].fullName + ", ";
                                                    }

                                                    miembros = miembros.Trim();
                                                    if (miembros.Length > 0)
                                                    {
                                                        if (miembros.Substring(miembros.Length - 1) == ",")
                                                            miembros = miembros.Substring(0, miembros.Length - 1);
                                                    }

                                                    xmlTar.SetAttr("usuario", miembros);

                                                    if (archivadas == "1")
                                                    {
                                                        xmlTar.SetAttr("mostrarArchivada", "1");
                                                        if (c.closed)
                                                            xmlTar.SetAttr("archivada", "1");
                                                        else
                                                            xmlTar.SetAttr("archivada", "0");
                                                    }
                                                    else
                                                        xmlTar.SetAttr("mostrarArchivada", "0");

                                                    string labels = "";
                                                    foreach (Label lab in c.labels)
                                                    {
                                                        labels += lab.name + ", ";
                                                    }
                                                    labels = labels.Trim();
                                                    if (labels.Length > 0)
                                                    {
                                                        if (labels.Substring(labels.Length - 1) == ",")
                                                            labels = labels.Substring(0, labels.Length - 1);
                                                    }
                                                    xmlTar.SetAttr("labels", labels);

                                                    if (c.due != null && c.due.HasValue)
                                                    {
                                                        xmlTar.SetAttr("f_vto", c.due.Value.ToString("dd/MM/yyyy HH:mm"));
                                                    }

                                                    if (c.dateLastActivity != null && c.dateLastActivity.HasValue)
                                                        xmlTar.SetAttr("f_ult_act", c.dateLastActivity.Value.ToString("dd/MM/yyyy HH:mm"));

                                                    xmlTar.SetAttr("desc", c.desc);

                                                    foreach (CustomField custom in customFields.Values)
                                                    {
                                                        string valor = "";
                                                        //string campo = "";
                                                        bool encontro = false;

                                                        string valTxt = "";
                                                        double? valDbl = null;
                                                        bool? valBool = null;
                                                        DateTime? valDate = null;

                                                        //campo = custom.name.Replace(" ", "").Replace("ñ","ni").Replace("Ñ","NI");

                                                        foreach (CustomFieldItem customitem in c.customFieldItems)
                                                        {
                                                            if (custom.id == customitem.idCustomField)
                                                            {
                                                                encontro = true;
                                                                if (custom.type == "checkbox")
                                                                    valor = customitem.value["checked"];
                                                                else
                                                                    valor = customitem.value[custom.type];

                                                                try
                                                                {
                                                                    switch (custom.type)
                                                                    {
                                                                        case "text":
                                                                            valTxt = valor;
                                                                            xmlTar.SetAttr(custom.campo, valTxt);
                                                                            break;
                                                                        case "date":
                                                                            int year = int.Parse(valor.Substring(0, 4));
                                                                            int month = int.Parse(valor.Substring(5, 2));
                                                                            int day = int.Parse(valor.Substring(8, 2));
                                                                            int hour = int.Parse(valor.Substring(11, 2));
                                                                            int minutes = int.Parse(valor.Substring(14, 2));
                                                                            valDate = new DateTime(year, month, day, hour, minutes, 0);
                                                                            xmlTar.SetAttr(custom.campo, valDate.Value.ToString("dd/MM/yyyy HH:mm"));
                                                                            break;
                                                                        case "checkbox":
                                                                            valBool = StringUtil.JSON2Object<bool>(valor);
                                                                            xmlTar.SetAttr(custom.campo, valBool.Value ? "Si" : "No");
                                                                            break;
                                                                        case "number":
                                                                            valDbl = StringUtil.JSON2Object<double>(valor);
                                                                            xmlTar.SetAttr(custom.campo, valDbl.Value);
                                                                            break;
                                                                        default:
                                                                            valTxt = valor;
                                                                            xmlTar.SetAttr(custom.campo, valTxt);
                                                                            break;
                                                                    }

                                                                    break;
                                                                }
                                                                catch (Exception ex)
                                                                {
                                                                    objBatch.Wrn("Error recuperando los campos personalizados - Causa: " + ex.Message);
                                                                    continue;
                                                                }
                                                            }
                                                        }

                                                        if (!encontro)
                                                            xmlTar.SetAttr(custom.campo, "");
                                                    }

                                                    xmlcol.AddTailElement(xmlTar);
                                                }

                                                xmlTab.AddTailElement(xmlcol);

                                                DOC.FirstChild().AddTailElement(xmlTab);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (WebException ex)
            {
                if (ex.Status != WebExceptionStatus.Timeout && ex.Status != WebExceptionStatus.SendFailure)
                {
                    System.Net.HttpWebResponse WResponse = (System.Net.HttpWebResponse)ex.Response;

                    result = "";
                    using (StreamReader streamReader = new StreamReader(WResponse.GetResponseStream()))
                    {
                        result = streamReader.ReadToEnd();
                    }

                    objBatch.Err("Error durante el reporte - Causa: " + result);

                }
                else
                {
                    
                    objBatch.Err("Error durante el reporte - Causa: " + ex.Message);
                }
            }
            catch(Exception ex)
            {
                objBatch.Err("Error durante el reporte - Causa: " + ex.Message);
            }

            objBatch.SetMess("Ejecutando el Reporte...");
            NomadBatch.Trace(DOC.ToString());
            Nomad.NomadHTML nmdHtml = new Nomad.NomadHTML("NucleusProject.Base.Project.TarjetasRoberto.rpt", DOC.ToString());

            string outFileName = NomadProxy.GetProxy().Batch().ID + ".htm";
            string outFilePath = NomadProxy.GetProxy().RunPath + "Nomad\\TEMP\\";
            NomadBatch.Trace("Generando Reporte HTML en path:'" + outFilePath + outFileName + "'");

            objBatch.SetPro(85);

            StreamWriter sw = new System.IO.StreamWriter(outFilePath + "\\" + outFileName, false, System.Text.Encoding.UTF8);

            nmdHtml.GenerateHTML(sw);

            sw.Close();
        }

        private static Dictionary<string, CustomField> GetAllCustomFields(string idBoard, string key, string token, NomadBatch objBatch)
        {
            Dictionary<string, CustomField> dicFields = new Dictionary<string, CustomField>();
            List<CustomField> cFields = null;
            HttpWebRequest request = null;
            string result = "";

            try
            {
                request = (HttpWebRequest)WebRequest.Create(@"https://api.trello.com/1/boards/" + idBoard + "/customFields?key=" + key + "&token=" + token);
                request.ContentType = "application/json";
                request.Method = "GET";

                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                using (StreamReader streamReader = new StreamReader(response.GetResponseStream()))
                {
                    result = streamReader.ReadToEnd();
                    //Dictionary<object, object> dic = StringUtil.JSON2Object<Dictionary<object, object>>(result);
                    cFields = StringUtil.JSON2Object<List<CustomField>>(result);
                }

                foreach (CustomField c in cFields)
                {
                    dicFields.Add(c.id, c);
                }

                return dicFields;
            }
            catch (WebException ex)
            {
                if (ex.Status != WebExceptionStatus.Timeout && ex.Status != WebExceptionStatus.SendFailure)
                {
                    System.Net.HttpWebResponse WResponse = (System.Net.HttpWebResponse)ex.Response;

                    result = "";
                    using (StreamReader streamReader = new StreamReader(WResponse.GetResponseStream()))
                    {
                        result = streamReader.ReadToEnd();
                    }

                    objBatch.Err("Error durante el recupero de CustomFields del tablero - Causa: " + result);
                    return dicFields;

                }
                else
                {
                    objBatch.Err("Error durante el recupero de CustomFields del tablero - Causa: " + ex.Message);
                    return dicFields;
                }
            }
            catch (Exception ex)
            {
                objBatch.Err("Error durante el reporte - Causa: " + ex.Message);
                return dicFields;
            }
        }

        public static void ReporteTarjetasPorRecurso(Nomad.NSystem.Proxy.NomadXML DOC)
        {
            ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072; //TLS 1.2

            NomadBatch objBatch;
            objBatch = NomadBatch.GetBatch("Iniciando...", "Reporte Tarjetas por Recurso");
            HttpWebRequest request = null;
            string result;
            string textsearch = "";
            Search busqueda;
            string comillas = "\"";
            objBatch.SetPro(0);

            List<string> teamsTrello = new List<string>();
            List<string> teamsEliminar = new List<string>();

            NomadXML xmlMiembro = null;
            NomadXML xmlTablero = null;
            string pertenece = "";

            string usuarios = DOC.FirstChild().GetAttr("listusuarios");

            if (usuarios.Replace(";", "").Trim().Length == 0)
            {
                objBatch.Err("No se especificaron los usuarios a filtrar.");
                return;
            }

            objBatch.SetPro(5);

            //Arreglo de usuarios
            string[] usrs = usuarios.Split(';');

            string tableros = DOC.FirstChild().GetAttr("listtableros");
            string columnas = DOC.FirstChild().GetAttr("listcolumnas");

            string archivadas = DOC.FirstChild().GetAttr("archivadas");
            if (archivadas == "")
                archivadas = "0";
            string descripcion = DOC.FirstChild().GetAttr("detalle");

            string filter = "", filter2 = "";
            if (archivadas != "0")
                filter += "filter=all";
            else
                filter2 += " is:open";

            DOC.FirstChild().SetAttr("fecha", DateTime.Now.ToString("dd/MM/yyyy HH:mm"));


            Nomad.Base.Login.Cuentas.CUENTA cta = new Nomad.Base.Login.Cuentas.CUENTA(NomadProxy.GetProxy().UserName);
            foreach (Nomad.Base.Login.Cuentas.USUARIO_GRUPO gr in cta.GRUPOS)
            {
                if (gr.GRUPO.Contains("TRE"))
                {
                    teamsTrello.Add(gr.GRUPO.Replace("TRE", ""));
                    pertenece += gr.GRUPO.Replace("TRE", "") + ", ";
                }
            }

            objBatch.Log("El usuario actual '" + NomadProxy.GetProxy().UserName + "' pertenece a los equipos Trello: " + pertenece.Substring(0, pertenece.Length - 2));

            //Valido que existan todas las credenciales de los team Trello definidas
            foreach (string team in teamsTrello)
            {
                Token tok = new Token(team);
                if (!(tok.key != null && tok.token != null))
                {
                    objBatch.Wrn("No se definieron las credenciales de Trello para el equipo '" + team + "'.");
                    teamsEliminar.Add(team);
                    continue;
                }
            }

            foreach(string team in teamsEliminar)
            {
                teamsTrello.Remove(team);
            }

            objBatch.SetPro(25);

            //Recorro usuarios
            for (int i = 0; i < usrs.Length; i++)
            {
                try
                {
                    xmlMiembro = new NomadXML("MIEMBRO");
                    string usrSearch = usrs[i].Trim();

                    if (usrSearch != "")
                    {
                        foreach (string team in teamsTrello)
                        {
                            Token tok = new Token(team);
                            if (tok.key != null && tok.token != null)
                            {
                                string appkey = tok.key;
                                string token = tok.token;

                                //Busco los datos personales del Usuario
                                request = (HttpWebRequest)WebRequest.Create(@"https://api.trello.com/1/search?query=" + usrSearch + "&modelTypes=members&member_fields=fullName,initials,username&members_limit=1&key=" + appkey + "&token=" + token);
                                request.ContentType = "application/json";
                                request.Method = "GET";

                                result = "";
                                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                                using (StreamReader streamReader = new StreamReader(response.GetResponseStream()))
                                {
                                    result = streamReader.ReadToEnd();
                                    //Dictionary<object, object> dic = StringUtil.JSON2Object<Dictionary<object, object>>(result);
                                    busqueda = StringUtil.JSON2Object<Search>(result);
                                }

                                //Por cada uno que haya coincidido en nombre de usuario (dudo que haya mas de uno)
                                if (busqueda.members != null && busqueda.members.Count > 0)
                                {
                                    Member miembroAct = busqueda.members[0];
                                    xmlMiembro.SetAttr("nombre", "[" + miembroAct.initials + "] - @" + miembroAct.userName + " - " + miembroAct.fullName);

                                    //Especifico al menos algún tablero?
                                    if (tableros != "")
                                    {
                                        //Arreglo de tableros
                                        string[] tab = tableros.Split(';');
                                        //Recorro todos los tableros
                                        for (int j = 0; j < tab.Length; j++)
                                        {
                                            textsearch = tab[j].Trim();
                                            if(textsearch!="")
                                            {
                                                //Especifico al menos alguna columna?
                                                if (columnas != "")
                                                {
                                                    //Arreglo de columnas
                                                    string[] cols = columnas.Split(';');

                                                    //Busco el tablero
                                                    textsearch = tab[j].Trim();
                                                    request = (HttpWebRequest)WebRequest.Create(@"https://api.trello.com/1/search?query=" + textsearch + "&modelTypes=boards&boards_fields=name&key=" + appkey + "&token=" + token);
                                                    request.ContentType = "application/json";
                                                    request.Method = "GET";

                                                    result = "";
                                                    using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                                                    using (StreamReader streamReader = new StreamReader(response.GetResponseStream()))
                                                    {
                                                        result = streamReader.ReadToEnd();
                                                        //Dictionary<object, object> dic = StringUtil.JSON2Object<Dictionary<object, object>>(result);
                                                        busqueda = StringUtil.JSON2Object<Search>(result);
                                                    }

                                                    //Recorro todos los tableros que coincidan con el nombre
                                                    foreach (Board b in busqueda.boards)
                                                    {
                                                        xmlTablero = null;
                                                        List<Lista> listasAct = new List<Lista>();

                                                        //Busco el tablero b para obtener todas las columnas que lo componen
                                                        request = (HttpWebRequest)WebRequest.Create(@"https://api.trello.com/1/boards/" + b.id + "/lists?key=" + appkey + "&token=" + token);
                                                        request.ContentType = "application/json";
                                                        request.Method = "GET";

                                                        result = "";
                                                        using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                                                        using (StreamReader streamReader = new StreamReader(response.GetResponseStream()))
                                                        {
                                                            result = streamReader.ReadToEnd();
                                                            //Dictionary<object, object> dic = StringUtil.JSON2Object<Dictionary<object, object>>(result);
                                                            listasAct = StringUtil.JSON2Object<List<Lista>>(result);
                                                        }

                                                        //Recorro las columnas del tablero b
                                                        foreach (Lista l in listasAct)
                                                        {
                                                            Lista listBusca = null;
                                                            //Busco si uno coincide con el ingresado (del arreglo de columnas)
                                                            for (int k = 0; k < cols.Length; k++)
                                                            {
                                                                if (l.name == cols[k])
                                                                {
                                                                    listBusca = l;
                                                                    break;
                                                                }
                                                            }

                                                            //Encontro coincidencia?
                                                            if (listBusca != null)
                                                            {
                                                                //Si > Busco (para el miembro + para el tablero + para la columna) las tarjetas que cumplan con esas condiciones
                                                                textsearch = "member:" + miembroAct.userName + " board:" + comillas + b.name + comillas + " list:" + comillas + listBusca.name + comillas + filter2;
                                                                request = (HttpWebRequest)WebRequest.Create(@"https://api.trello.com/1/search?query=" + textsearch + "&modelTypes=cards&cards_fields=id,name,idMembers,labels,closed,due,dateLastActivity,desc&cards_limit=1000&key=" + appkey + "&token=" + token);
                                                                request.ContentType = "application/json";
                                                                request.Method = "GET";

                                                                result = "";
                                                                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                                                                using (StreamReader streamReader = new StreamReader(response.GetResponseStream()))
                                                                {
                                                                    result = streamReader.ReadToEnd();
                                                                    //Dictionary<object, object> dic = StringUtil.JSON2Object<Dictionary<object, object>>(result);
                                                                    busqueda = StringUtil.JSON2Object<Search>(result);
                                                                }

                                                                //Valido que haya encontrado alguna tarjeta
                                                                if (busqueda.cards.Count > 0)
                                                                {
                                                                    //Valido si ya esta el tablero creado o lo creo (validacion para mostrarlo en el rpt)
                                                                    if (xmlTablero == null)
                                                                    {
                                                                        xmlTablero = new NomadXML("TABLERO");
                                                                        xmlTablero.SetAttr("nombre", b.name);
                                                                    }

                                                                    NomadXML xmlColumna = new NomadXML("COLUMNA");
                                                                    xmlColumna.SetAttr("nombre", l.name);

                                                                    //Recorro todas las tarjetas encontradas
                                                                    foreach (Card c in busqueda.cards)
                                                                    {
                                                                        NomadXML xmlTarjeta = new NomadXML("TARJETA");
                                                                        xmlTarjeta.SetAttr("nombre", c.name);

                                                                        if (archivadas == "1")
                                                                        {
                                                                            xmlTarjeta.SetAttr("mostrarArchivada", "1");
                                                                            if (c.closed)
                                                                                xmlTarjeta.SetAttr("archivada", "1");
                                                                            else
                                                                                xmlTarjeta.SetAttr("archivada", "0");
                                                                        }
                                                                        else
                                                                            xmlTarjeta.SetAttr("mostrarArchivada", "0");

                                                                        string labels = "";
                                                                        foreach (Label lab in c.labels)
                                                                        {
                                                                            labels += lab.name + ", ";
                                                                        }
                                                                        labels = labels.Trim();
                                                                        if (labels.Length > 0)
                                                                        {
                                                                            if (labels.Substring(labels.Length - 1) == ",")
                                                                                labels = labels.Substring(0, labels.Length - 1);
                                                                        }
                                                                        xmlTarjeta.SetAttr("labels", labels);

                                                                        if (c.due != null && c.due.HasValue)
                                                                        {
                                                                            xmlTarjeta.SetAttr("f_vto", c.due.Value.ToString("dd/MM/yyyy HH:mm"));
                                                                        }

                                                                        xmlTarjeta.SetAttr("desc", c.desc);

                                                                        //Agrego a la columna
                                                                        xmlColumna.AddTailElement(xmlTarjeta);
                                                                    }

                                                                    //Agrego al tablero
                                                                    if (xmlTablero != null)
                                                                        xmlTablero.AddTailElement(xmlColumna);
                                                                }
                                                            }
                                                        }

                                                        //Agrego el tablero al miembro
                                                        if (xmlTablero != null)
                                                            xmlMiembro.AddTailElement(xmlTablero);
                                                    }
                                                }
                                                else
                                                {
                                                    //No se especificaron las columnas > Busco todas para el tablero ingresado
                                                    //Busco el/los tableros que coincidan con el nombre ingresado (arreglo tableros)
                                                    textsearch = tab[j].Trim();
                                                    request = (HttpWebRequest)WebRequest.Create(@"https://api.trello.com/1/search?query=" + textsearch + "&modelTypes=boards&boards_fields=name&key=" + appkey + "&token=" + token);
                                                    request.ContentType = "application/json";
                                                    request.Method = "GET";

                                                    result = "";
                                                    using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                                                    using (StreamReader streamReader = new StreamReader(response.GetResponseStream()))
                                                    {
                                                        result = streamReader.ReadToEnd();
                                                        //Dictionary<object, object> dic = StringUtil.JSON2Object<Dictionary<object, object>>(result);
                                                        busqueda = StringUtil.JSON2Object<Search>(result);
                                                    }

                                                    //Recorro tableros que coincidieron con el nombre
                                                    foreach (Board b in busqueda.boards)
                                                    {
                                                        xmlTablero = null;
                                                        List<Lista> listasAct = new List<Lista>();
                                                        //Busco todas las columnas del tablero
                                                        request = (HttpWebRequest)WebRequest.Create(@"https://api.trello.com/1/boards/" + b.id + "/lists?key=" + appkey + "&token=" + token);
                                                        request.ContentType = "application/json";
                                                        request.Method = "GET";

                                                        result = "";
                                                        using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                                                        using (StreamReader streamReader = new StreamReader(response.GetResponseStream()))
                                                        {
                                                            result = streamReader.ReadToEnd();
                                                            //Dictionary<object, object> dic = StringUtil.JSON2Object<Dictionary<object, object>>(result);
                                                            listasAct = StringUtil.JSON2Object<List<Lista>>(result);
                                                        }

                                                        //Recorro arreglo de columnas
                                                        foreach (Lista l in listasAct)
                                                        {
                                                            //Busco (para el miembro + para el tablero + para la columna) las tarjetas que cumplan con esas condiciones
                                                            textsearch = "member:" + miembroAct.userName + " board:" + comillas + b.name + comillas + " list:" + comillas + l.name + comillas + filter2;
                                                            request = (HttpWebRequest)WebRequest.Create(@"https://api.trello.com/1/search?query=" + textsearch + "&modelTypes=cards&cards_fields=id,name,idMembers,labels,closed,due,dateLastActivity,desc&cards_limit=1000&key=" + appkey + "&token=" + token);
                                                            request.ContentType = "application/json";
                                                            request.Method = "GET";

                                                            result = "";
                                                            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                                                            using (StreamReader streamReader = new StreamReader(response.GetResponseStream()))
                                                            {
                                                                result = streamReader.ReadToEnd();
                                                                //Dictionary<object, object> dic = StringUtil.JSON2Object<Dictionary<object, object>>(result);
                                                                busqueda = StringUtil.JSON2Object<Search>(result);
                                                            }

                                                            //Encontro tarjetas?
                                                            if (busqueda.cards.Count > 0)
                                                            {
                                                                //Si > Valido que no este creado el tablero de antemano
                                                                if (xmlTablero == null)
                                                                {
                                                                    xmlTablero = new NomadXML("TABLERO");
                                                                    xmlTablero.SetAttr("nombre", b.name);
                                                                }

                                                                NomadXML xmlColumna = new NomadXML("COLUMNA");
                                                                xmlColumna.SetAttr("nombre", l.name);

                                                                //Recorro tarjetas filtradas
                                                                foreach (Card c in busqueda.cards)
                                                                {
                                                                    NomadXML xmlTarjeta = new NomadXML("TARJETA");
                                                                    xmlTarjeta.SetAttr("nombre", c.name);

                                                                    if (archivadas == "1")
                                                                    {
                                                                        xmlTarjeta.SetAttr("mostrarArchivada", "1");
                                                                        if (c.closed)
                                                                            xmlTarjeta.SetAttr("archivada", "1");
                                                                        else
                                                                            xmlTarjeta.SetAttr("archivada", "0");
                                                                    }
                                                                    else
                                                                        xmlTarjeta.SetAttr("mostrarArchivada", "0");

                                                                    string labels = "";
                                                                    foreach (Label lab in c.labels)
                                                                    {
                                                                        labels += lab.name + ", ";
                                                                    }
                                                                    labels = labels.Trim();
                                                                    if (labels.Length > 0)
                                                                    {
                                                                        if (labels.Substring(labels.Length - 1) == ",")
                                                                            labels = labels.Substring(0, labels.Length - 1);
                                                                    }
                                                                    xmlTarjeta.SetAttr("labels", labels);

                                                                    if (c.due != null && c.due.HasValue)
                                                                    {
                                                                        xmlTarjeta.SetAttr("f_vto", c.due.Value.ToString("dd/MM/yyyy HH:mm"));
                                                                    }

                                                                    xmlTarjeta.SetAttr("desc", c.desc);

                                                                    xmlColumna.AddTailElement(xmlTarjeta);
                                                                }

                                                                if (xmlTablero != null)
                                                                    xmlTablero.AddTailElement(xmlColumna);
                                                            }
                                                        }

                                                        if (xmlTablero != null)
                                                            xmlMiembro.AddTailElement(xmlTablero);
                                                    }
                                                }

                                                DOC.FirstChild().AddTailElement(xmlMiembro);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        //No se especificaron tableros pero si columnas?
                                        if (columnas != "")
                                        {
                                            //Arreglo de columnas
                                            string[] cols = columnas.Split(';');

                                            for (int j = 0; j < cols.Length; j++)
                                            {
                                                //Busco todas las tarjetas de la columna
                                                textsearch = "list:" + comillas + cols[j].Trim() + comillas + " @" + miembroAct.userName + filter2;
                                                request = (HttpWebRequest)WebRequest.Create(@"https://api.trello.com/1/search?query=" + textsearch + "&modelTypes=cards&card_fields=closed%2Cdesc%2CdescData%2Cdue%2CidBoard%2Clabels%2Cname&cards_limit=1000&key=" + appkey + "&token=" + token);
                                                request.ContentType = "application/json";
                                                request.Method = "GET";

                                                result = "";
                                                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                                                using (StreamReader streamReader = new StreamReader(response.GetResponseStream()))
                                                {
                                                    result = streamReader.ReadToEnd();
                                                    busqueda = StringUtil.JSON2Object<Search>(result);
                                                }

                                                Dictionary<string, Board> boards = new Dictionary<string, Board>();

                                                foreach (Card car in busqueda.cards)
                                                {
                                                    if (!boards.ContainsKey(car.idBoard))
                                                    {
                                                        Board bAct = null;
                                                        //Busco el tablero
                                                        request = (HttpWebRequest)WebRequest.Create(@"https://api.trello.com/1/boards/" + car.idBoard + "?key=" + appkey + "&token=" + token);
                                                        request.ContentType = "application/json";
                                                        request.Method = "GET";

                                                        result = "";
                                                        using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                                                        using (StreamReader streamReader = new StreamReader(response.GetResponseStream()))
                                                        {
                                                            result = streamReader.ReadToEnd();
                                                            //Dictionary<object, object> dic = StringUtil.JSON2Object<Dictionary<object, object>>(result);
                                                            bAct = StringUtil.JSON2Object<Board>(result);
                                                            if (bAct != null)
                                                                boards.Add(car.idBoard, bAct);
                                                        }
                                                    }
                                                }

                                                foreach (string keyBoard in boards.Keys)
                                                {
                                                    xmlTablero = new NomadXML("TABLERO");
                                                    xmlTablero.SetAttr("nombre", boards[keyBoard].name);

                                                    NomadXML xmlColumna = new NomadXML("COLUMNA");
                                                    xmlColumna.SetAttr("nombre", cols[j]);

                                                    //Recorro tarjetas filtradas
                                                    foreach (Card c in busqueda.cards)
                                                    {
                                                        if (c.idBoard == keyBoard)
                                                        {
                                                            NomadXML xmlTarjeta = new NomadXML("TARJETA");
                                                            xmlTarjeta.SetAttr("nombre", c.name);

                                                            if (archivadas == "1")
                                                            {
                                                                xmlTarjeta.SetAttr("mostrarArchivada", "1");
                                                                if (c.closed)
                                                                    xmlTarjeta.SetAttr("archivada", "1");
                                                                else
                                                                    xmlTarjeta.SetAttr("archivada", "0");
                                                            }
                                                            else
                                                                xmlTarjeta.SetAttr("mostrarArchivada", "0");

                                                            string labels = "";
                                                            foreach (Label lab in c.labels)
                                                            {
                                                                labels += lab.name + ", ";
                                                            }
                                                            labels = labels.Trim();
                                                            if (labels.Length > 0)
                                                            {
                                                                if (labels.Substring(labels.Length - 1) == ",")
                                                                    labels = labels.Substring(0, labels.Length - 1);
                                                            }
                                                            xmlTarjeta.SetAttr("labels", labels);

                                                            if (c.due != null && c.due.HasValue)
                                                            {
                                                                xmlTarjeta.SetAttr("f_vto", c.due.Value.ToString("dd/MM/yyyy HH:mm"));
                                                            }

                                                            xmlTarjeta.SetAttr("desc", c.desc);

                                                            xmlColumna.AddTailElement(xmlTarjeta);
                                                        }
                                                    }

                                                    if (xmlTablero != null)
                                                    {
                                                        xmlTablero.AddTailElement(xmlColumna);
                                                        xmlMiembro.AddTailElement(xmlTablero);
                                                    }
                                                        
                                                }

                                                /*if (xmlTablero != null)
                                                    xmlMiembro.AddTailElement(xmlTablero);*/
                                            }
                                        }
                                        else
                                        {
                                            //No especificó tableros ni columnas
                                            List<Board> tablerosUsuario = new List<Board>();

                                            //Busco todos los tableros+columnas del usuario (busco por id de usr)
                                            request = (HttpWebRequest)WebRequest.Create(@"https://api.trello.com/1/members/" + miembroAct.id + "/boards?fields=name%2Cid&lists=all&key=" + appkey + "&token=" + token);
                                            request.ContentType = "application/json";
                                            request.Method = "GET";

                                            result = "";
                                            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                                            using (StreamReader streamReader = new StreamReader(response.GetResponseStream()))
                                            {
                                                result = streamReader.ReadToEnd();
                                                //Dictionary<object, object> dic = StringUtil.JSON2Object<Dictionary<object, object>>(result);
                                                tablerosUsuario = StringUtil.JSON2Object<List<Board>>(result);
                                            }

                                            //Recorro todos los tableros vinculados al usuario
                                            foreach (Board b in tablerosUsuario)
                                            {
                                                bool muestroTab = true;
                                                xmlTablero = null;

                                                //Recorro columnas del tablero
                                                foreach (Lista l in b.lists)
                                                {
                                                    //Busco todas las tarjetas del (miembro+tablero+columna)
                                                    textsearch = "member:" + miembroAct.userName + " board:" + comillas + b.name + comillas + " list:" + comillas + l.name + comillas + filter2;
                                                    request = (HttpWebRequest)WebRequest.Create(@"https://api.trello.com/1/search?query=" + textsearch + "&modelTypes=cards&cards_fields=id,name,idMembers,labels,closed,due,dateLastActivity,desc&cards_limit=1000&key=" + appkey + "&token=" + token);
                                                    request.ContentType = "application/json";
                                                    request.Method = "GET";

                                                    result = "";
                                                    using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                                                    using (StreamReader streamReader = new StreamReader(response.GetResponseStream()))
                                                    {
                                                        result = streamReader.ReadToEnd();
                                                        //Dictionary<object, object> dic = StringUtil.JSON2Object<Dictionary<object, object>>(result);
                                                        busqueda = StringUtil.JSON2Object<Search>(result);
                                                    }

                                                    //Encontro tarjetas?
                                                    if (busqueda.cards.Count > 0)
                                                    {
                                                        //Valido si creo o no el tablero
                                                        if (muestroTab)
                                                        {
                                                            xmlTablero = new NomadXML("TABLERO");
                                                            xmlTablero.SetAttr("nombre", b.name);
                                                            muestroTab = false;
                                                        }

                                                        NomadXML xmlColumna = new NomadXML("COLUMNA");
                                                        xmlColumna.SetAttr("nombre", l.name);

                                                        //Recorro tarjetas filtradas
                                                        foreach (Card c in busqueda.cards)
                                                        {
                                                            NomadXML xmlTarjeta = new NomadXML("TARJETA");
                                                            xmlTarjeta.SetAttr("nombre", c.name);

                                                            if (archivadas == "1")
                                                            {
                                                                xmlTarjeta.SetAttr("mostrarArchivada", "1");
                                                                if (c.closed)
                                                                    xmlTarjeta.SetAttr("archivada", "1");
                                                                else
                                                                    xmlTarjeta.SetAttr("archivada", "0");
                                                            }
                                                            else
                                                                xmlTarjeta.SetAttr("mostrarArchivada", "0");

                                                            string labels = "";
                                                            foreach (Label lab in c.labels)
                                                            {
                                                                labels += lab.name + ", ";
                                                            }
                                                            labels = labels.Trim();
                                                            if (labels.Length > 0)
                                                            {
                                                                if (labels.Substring(labels.Length - 1) == ",")
                                                                    labels = labels.Substring(0, labels.Length - 1);
                                                            }
                                                            xmlTarjeta.SetAttr("labels", labels);

                                                            if (c.due != null && c.due.HasValue)
                                                            {
                                                                xmlTarjeta.SetAttr("f_vto", c.due.Value.ToString("dd/MM/yyyy HH:mm"));
                                                            }

                                                            xmlTarjeta.SetAttr("desc", c.desc);

                                                            xmlColumna.AddTailElement(xmlTarjeta);
                                                        }

                                                        if (xmlTablero != null)
                                                            xmlTablero.AddTailElement(xmlColumna);
                                                    }
                                                }

                                                if (xmlTablero != null)
                                                    xmlMiembro.AddTailElement(xmlTablero);
                                            }
                                        }
                                    }

                                    DOC.FirstChild().AddTailElement(xmlMiembro);
                                }
                                else
                                {
                                    objBatch.Wrn("No se encontraron resultados para el usuario '" + usrSearch + "' en el equipo '" + team + "'.");
                                    continue;
                                }

                            }
                        }

                    }
                }
                catch (WebException ex)
                {
                    if (ex.Status != WebExceptionStatus.Timeout && ex.Status != WebExceptionStatus.SendFailure)
                    {
                        System.Net.HttpWebResponse WResponse = (System.Net.HttpWebResponse)ex.Response;

                        result = "";
                        using (StreamReader streamReader = new StreamReader(WResponse.GetResponseStream()))
                        {
                            result = streamReader.ReadToEnd();
                        }

                        objBatch.Err("Error durante el reporte - Causa: " + result);
                        continue;
                    }
                    else
                    {
                        objBatch.Err("Error durante el reporte - Causa: " + ex.Message);
                        continue;
                    }
                }
                catch (Exception ex)
                {
                    objBatch.Err("Error durante el reporte - Causa: " + ex.Message);
                    continue;
                }
            }

            objBatch.SetPro(65);

            objBatch.SetMess("Ejecutando el Reporte...");
            NomadBatch.Trace(DOC.ToString());
            Nomad.NomadHTML nmdHtml = new Nomad.NomadHTML("NucleusProject.Base.Project.TarjetasPorRecurso.rpt", DOC.ToString());

            objBatch.SetPro(85);

            string outFileName = NomadProxy.GetProxy().Batch().ID + ".htm";
            string outFilePath = NomadProxy.GetProxy().RunPath + "Nomad\\TEMP\\";
            NomadBatch.Trace("Generando Reporte HTML en path:'" + outFilePath + outFileName + "'");

            objBatch.SetPro(95);

            StreamWriter sw = new System.IO.StreamWriter(outFilePath + "\\" + outFileName, false, System.Text.Encoding.UTF8);

            nmdHtml.GenerateHTML(sw);

            sw.Close();

        }

        private static Board GetBoard(string p)
        {
            throw new NotImplementedException();
        }

        private static Dictionary<string, Member> GetAllMiembros(string idBoard, string key, string token, NomadBatch objBatch)
        {
            Dictionary<string, Member> dicMiembros = new Dictionary<string, Member>();
            List<Member> miembros = null;
            HttpWebRequest request = null;
            string result = "";
            
            try
            {
                request = (HttpWebRequest)WebRequest.Create(@"https://api.trello.com/1/boards/" + idBoard + "/members?fields=initials,fullName,email&key=" + key + "&token=" + token);
                request.ContentType = "application/json";
                request.Method = "GET";

                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                using (StreamReader streamReader = new StreamReader(response.GetResponseStream()))
                {
                    result = streamReader.ReadToEnd();
                    //Dictionary<object, object> dic = StringUtil.JSON2Object<Dictionary<object, object>>(result);
                    miembros = StringUtil.JSON2Object<List<Member>>(result);
                }

                foreach (Member m in miembros)
                {
                    dicMiembros.Add(m.id, m);
                }

                return dicMiembros;
            }
            catch (WebException ex)
            {
                if (ex.Status != WebExceptionStatus.Timeout && ex.Status != WebExceptionStatus.SendFailure)
                {
                    System.Net.HttpWebResponse WResponse = (System.Net.HttpWebResponse)ex.Response;

                    result = "";
                    using (StreamReader streamReader = new StreamReader(WResponse.GetResponseStream()))
                    {
                        result = streamReader.ReadToEnd();
                    }

                    objBatch.Err("Error durante el recupero de Miembros del tablero - Causa: " + result);
                    return dicMiembros;

                }
                else
                {
                    objBatch.Err("Error durante el recupero de Miembros del tablero - Causa: " + ex.Message);
                    return dicMiembros;
                }
            }
            catch (Exception ex)
            {
                objBatch.Err("Error durante el reporte - Causa: " + ex.Message);
                return dicMiembros;
            }

            
        }

        private static string GetMiembro(string idMiembro, string key, string token, NomadBatch batch)
        {
            string miembro = "";
            string result = "";            
            HttpWebRequest request = null;
            Member m;

            try
            {   
                request = (HttpWebRequest)WebRequest.Create(@"https://api.trello.com/1/members/"+idMiembro+"?&fields=initials,fullName,email&key="+key+"&token="+token);
                request.ContentType = "application/json";
                request.Method = "GET";

                result = "";
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                using (StreamReader streamReader = new StreamReader(response.GetResponseStream()))
                {
                    result = streamReader.ReadToEnd();
                    //Dictionary<object, object> dic = StringUtil.JSON2Object<Dictionary<object, object>>(result);
                    m = StringUtil.JSON2Object<Member>(result);
                }

                miembro = "[" + m.initials + "] - " + m.fullName;
                return miembro;
            }
            catch (WebException ex)
            {
                if (ex.Status != WebExceptionStatus.Timeout && ex.Status != WebExceptionStatus.SendFailure)
                {
                    System.Net.HttpWebResponse WResponse = (System.Net.HttpWebResponse)ex.Response;

                    result = "";
                    using (StreamReader streamReader = new StreamReader(WResponse.GetResponseStream()))
                    {
                        result = streamReader.ReadToEnd();
                    }
                    batch.Err("Error recuperando el Miembro - " + ex.Message);
                    return miembro;
                }
                else
                {
                    batch.Err("Error recuperando el Miembro - " + ex.Message);
                    return miembro;
                }
            }
            catch (Exception e)
            {
                batch.Err("Error recuperando el Miembro - " + e.Message);
                return miembro;
            }
            
        }

        public static NomadXML NomadQuery(string strTabla, string strWhere, string strCampos, string strCamposAlias, string strTablaDestino)
        {
            System.Xml.XmlDocument xmlDocCal;

            strTabla = strTabla.ToUpper();
            strWhere = strWhere.ToUpper();
            strCampos = strCampos.ToUpper();
            strCamposAlias = strCamposAlias.ToUpper();
            strTablaDestino = strTablaDestino.ToUpper();

            Array arrCampos;
            //fncUpdEstructuras();
            arrCampos = strCamposAlias.Split(',');

            // EjecutarQuery devuelve un xml con el resultado de la consulta y los tipos de datos por cada campo
            xmlDocCal = EjecutarQuery(strTabla, strWhere, strCampos, strCamposAlias, ref arrCampos);

            if (xmlDocCal == null) { return null; }

            NomadXML nmdXMLOut;

            nmdXMLOut = new NomadXML(xmlDocCal.DocumentElement.InnerXml);
            return nmdXMLOut;
        }

        public static System.Xml.XmlDocument EjecutarQuery(string strTabla, string strWhere, string strCampos, string strCamposQryOut, ref Array arrCampos)
        {
            string varQuery;
            string varQueryParam;

            // para compatibilidad con ORACLE se paso todo a UPPER
            strWhere = strWhere.Replace("&GT", "&gt");
            strWhere = strWhere.Replace("&LT", "&lt");

            arrCampos = strCampos.Split(',');
            arrCampos = strCamposQryOut.Split(',');

            System.Xml.XmlDocument xmlDocCal;

            varQuery = @"
                <qry:main doc=""PARAM"">
                    <qry:append-doc name=""FILTRO"" doc-path=""#PARAM:/FILTRO""/>
                    <qry:element name=""objects"">
                      <qry:insert-select doc-path=""#FILTRO:"" name=""filtro_empresa""/>
                    </qry:element>
                  </qry:main>
                    <qry:select doc=""PARAM"" name=""filtro_empresa"">
                        <qry:xquery>
                          for $r in sql ('  SELECT   " + strCampos + @"  FROM "
                                     + strTabla + @" WHERE " + strWhere + @"  ')/ROWS" +
                                 @"</qry:xquery>
                        <qry:out>

                          <qry:insert-element doc-path=""$r""/>
                        </qry:out>
                      </qry:select>  ";

            varQueryParam = @"<FILTRO />";
            try
            {
                //strMsgErrorUser = sLegajoProc + "Error de consulta en NucleusRH ";
                xmlDocCal = NomadEnvironment.QueryXML(varQuery.ToString(), varQueryParam);
                return xmlDocCal;
            }
            catch (Exception e)
            {
                //fncLogearHas(" ERROR EjecutarQuery " + e.Message + "  " + varQuery.ToString());
                return null;
            }

        }

        public static void CACHETableros()
        {
            ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072; //TLS 1.2
            List<string> teamsTrello = new List<string>();            
            HttpWebRequest request = null;
            string result;
            string nameFile;
            List<Board> boards;
            int cont;

            try
            {   
                NomadXML filesGRUPOS = NomadProxy.GetProxy().FileSystem().List(@"Nomad\DB\Nomad\Base\Login\Grupos");

                ArrayList files = (ArrayList)filesGRUPOS.GetElements("FILE");
                for (int i = 0; i < files.Count; i++)
                {
                    NomadXML file = (NomadXML)files[i];

                    if(file.GetAttrString("NAME").Contains("TRE"))
                    {
                        teamsTrello.Add(file.GetAttrString("NAME").Replace("TRE", "").Replace(".GRUPO.XML",""));
                    }
                }

                foreach(string team in teamsTrello)
                {
                    Token token = null;
                    nameFile = NomadProxy.GetProxy().AppName + "_" + team + ".xml";
                    cont = 0;

                    token = new Token(team);
                    
                    NomadXML xmlTableros = new NomadXML("OPTS");

                    if (token.key != null && token.token != null)
                    {
                        try
                        {
                            request = (HttpWebRequest)WebRequest.Create(@"https://api.trello.com/1/organizations/" + team + "/boards?filter=all&fields=id%2Cname&key=" + token.key + "&token=" + token.token);
                            request.ContentType = "application/json";
                            request.Method = "GET";

                            result = "";
                            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                            using (StreamReader streamReader = new StreamReader(response.GetResponseStream()))
                            {
                                result = streamReader.ReadToEnd();
                                //Dictionary<object, object> dic = StringUtil.JSON2Object<Dictionary<object, object>>(result);
                                boards = StringUtil.JSON2Object<List<Board>>(result);
                            }

                            foreach (Board b in boards)
                            {
                                cont = 0;
                                Board boardAct = null;
                                request = (HttpWebRequest)WebRequest.Create(@"https://api.trello.com/1/boards/" + b.id + "?lists=open&key=" + token.key + "&token=" + token.token);
                                request.ContentType = "application/json";
                                request.Method = "GET";

                                result = "";
                                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                                using (StreamReader streamReader = new StreamReader(response.GetResponseStream()))
                                {
                                    result = streamReader.ReadToEnd();
                                    //Dictionary<object, object> dic = StringUtil.JSON2Object<Dictionary<object, object>>(result);
                                    boardAct = StringUtil.JSON2Object<Board>(result);
                                }

                                if (boardAct.lists.Count > 0)
                                {
                                    foreach (Lista l in boardAct.lists)
                                    {
                                        ++cont;
                                        NomadXML xmlColumnas = new NomadXML("option");
                                        xmlColumnas.SetAttr("id", l.id);

                                        if (boardAct.name.Length<16)
                                            xmlColumnas.SetAttr("value", boardAct.name.Replace(" ", "") + cont.ToString().PadLeft(3, '0'));
                                        else
                                            xmlColumnas.SetAttr("value", boardAct.name.Replace(" ", "").Substring(0, 16) + cont.ToString().PadLeft(3, '0'));
                                        
                                        xmlColumnas.SetAttr("text", boardAct.name + " - " + l.name);
                                        xmlTableros.AddTailElement(xmlColumnas);
                                    }
                                }
                            }
                        }
                        catch (WebException ex)
                        {
                            if (ex.Status != WebExceptionStatus.Timeout && ex.Status != WebExceptionStatus.SendFailure)
                            {
                                System.Net.HttpWebResponse WResponse = (System.Net.HttpWebResponse)ex.Response;
                                result = "";
                                using (StreamReader streamReader = new StreamReader(WResponse.GetResponseStream()))
                                {
                                    result = streamReader.ReadToEnd();
                                }
                                NomadLog.Error("Error recuperando Tablero para app '" + NomadProxy.GetProxy().AppName + "' - Causa: " + result);
                            }
                            else
                            {
                                NomadLog.Error("Error recuperando Tablero para app '" + NomadProxy.GetProxy().AppName + "' - Causa: " + ex.Message);
                            }
                        }                                            
                    }

                    if (xmlTableros.GetElements("option").Count > 0)
                        NomadProxy.GetProxy().FileServiceIO().SaveFile("TRELLO", NomadProxy.GetProxy().AppName + "_" + team + ".xml", xmlTableros.ToString(true));
                    else
                    {
                        if (NomadProxy.GetProxy().FileServiceIO().ExistsFile("TRELLO", nameFile))
                            NomadProxy.GetProxy().FileServiceIO().DeleteFile("TRELLO", nameFile);
                    }                    
                }

            }
            catch(Exception ex)
            {
                NomadLog.Error("Error recuperando Tablero para app '" + NomadProxy.GetProxy().AppName + "' - Causa: " + ex.Message);
            }
        }

        public static void CACHETableros(string team)
        {
            ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072; //TLS 1.2
            List<string> teamsTrello = new List<string>();
            HttpWebRequest request = null;
            string result;
            List<Board> boards;
            int cont = 0;

            try
            {
                Token token = null;
                string nameFile = NomadProxy.GetProxy().AppName + "_" + team + ".xml";

                token = new Token(team);

                NomadXML xmlTableros = new NomadXML("OPTS");

                if (token.key != null && token.token != null)
                {
                    try
                    {
                        request = (HttpWebRequest)WebRequest.Create(@"https://api.trello.com/1/organizations/" + team + "/boards?filter=all&fields=id%2Cname&key=" + token.key + "&token=" + token.token);
                        request.ContentType = "application/json";
                        request.Method = "GET";

                        result = "";
                        using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                        using (StreamReader streamReader = new StreamReader(response.GetResponseStream()))
                        {
                            result = streamReader.ReadToEnd();
                            //Dictionary<object, object> dic = StringUtil.JSON2Object<Dictionary<object, object>>(result);
                            boards = StringUtil.JSON2Object<List<Board>>(result);
                        }

                        foreach (Board b in boards)
                        {
                            cont = 0;
                            Board boardAct = null;
                            request = (HttpWebRequest)WebRequest.Create(@"https://api.trello.com/1/boards/" + b.id + "?lists=open&key=" + token.key + "&token=" + token.token);
                            request.ContentType = "application/json";
                            request.Method = "GET";

                            result = "";
                            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                            using (StreamReader streamReader = new StreamReader(response.GetResponseStream()))
                            {
                                result = streamReader.ReadToEnd();
                                //Dictionary<object, object> dic = StringUtil.JSON2Object<Dictionary<object, object>>(result);
                                boardAct = StringUtil.JSON2Object<Board>(result);
                            }

                            if (boardAct.lists.Count > 0)
                            {
                                foreach (Lista l in boardAct.lists)
                                {
                                     ++cont;
                                        NomadXML xmlColumnas = new NomadXML("option");
                                        xmlColumnas.SetAttr("id", l.id);

                                        if (boardAct.name.Length<16)
                                            xmlColumnas.SetAttr("value", boardAct.name.Replace(" ", "") + cont.ToString().PadLeft(3, '0'));
                                        else
                                            xmlColumnas.SetAttr("value", boardAct.name.Replace(" ", "").Substring(0, 16) + cont.ToString().PadLeft(3, '0'));
                                        
                                        xmlColumnas.SetAttr("text", boardAct.name + " - " + l.name);
                                        xmlTableros.AddTailElement(xmlColumnas);
                                }
                            }
                        }

                    }
                    catch (WebException ex)
                    {
                        if (ex.Status != WebExceptionStatus.Timeout && ex.Status != WebExceptionStatus.SendFailure)
                        {
                            System.Net.HttpWebResponse WResponse = (System.Net.HttpWebResponse)ex.Response;
                            result = "";
                            using (StreamReader streamReader = new StreamReader(WResponse.GetResponseStream()))
                            {
                                result = streamReader.ReadToEnd();
                            }
                            NomadLog.Error("Error recuperando Tablero para app '" + NomadProxy.GetProxy().AppName + "' - Causa: " + result);
                        }
                        else
                        {
                            NomadLog.Error("Error recuperando Tablero para app '" + NomadProxy.GetProxy().AppName + "' - Causa: " + ex.Message);
                        }
                    }
                }

                if (xmlTableros.GetElements("option").Count > 0)
                    NomadProxy.GetProxy().FileServiceIO().SaveFile("TRELLO", NomadProxy.GetProxy().AppName + "_" + team + ".xml", xmlTableros.ToString(true));
                else
                {
                    if (NomadProxy.GetProxy().FileServiceIO().ExistsFile("TRELLO", nameFile))
                        NomadProxy.GetProxy().FileServiceIO().DeleteFile("TRELLO", nameFile);
                }
            }
            catch (Exception ex)
            {
                NomadLog.Error("Error recuperando Tablero para app '" + NomadProxy.GetProxy().AppName + "' - Causa: " + ex.Message);
            }
        }

        public static Nomad.NSystem.Proxy.NomadXML GetTableros()
        {
            List<string> teamsTrello = new List<string>();
            int cont = 0;
            NomadXML xmlResult = new NomadXML("TRELLO");
            xmlResult.SetAttr("oi_recurso_trello", "");
            xmlResult.SetAttr("d_tarea_trello", "");
            xmlResult.SetAttr("o_tarea_trello", "");
            xmlResult.SetAttr("f_fin_trello", "");

            NomadXML xmlOptions = new NomadXML("OPTS");

            try
            {
                string cuenta = GetCuenta(NomadProxy.GetProxy().UserEtty);

                if(cuenta!="")
                {
                    Nomad.Base.Login.Cuentas.CUENTA cta = new Nomad.Base.Login.Cuentas.CUENTA(cuenta);
                    foreach (Nomad.Base.Login.Cuentas.USUARIO_GRUPO gr in cta.GRUPOS)
                    {
                        if (gr.GRUPO.Contains("TRE"))
                            teamsTrello.Add(gr.GRUPO.Replace("TRE", ""));
                    }

                    if (teamsTrello.Count > 0)
                    {
                        foreach (string team in teamsTrello)
                        {
                            string nameFile = NomadProxy.GetProxy().AppName + "_" + team + ".xml";

                            if (!NomadProxy.GetProxy().FileServiceIO().ExistsFile("TRELLO", nameFile))
                                CACHETableros(team);

                            if (NomadProxy.GetProxy().FileServiceIO().ExistsFile("TRELLO", nameFile))
                            {
                                NomadXML tablerosTeam = NomadProxy.GetProxy().FileServiceIO().LoadFileXML("TRELLO", nameFile);

                                ArrayList options = (ArrayList)tablerosTeam.GetElements("option");
                                cont += options.Count;
                                for (int i = 0; i < options.Count; i++)
                                {
                                    xmlOptions.AddTailElement((NomadXML)options[i]);
                                }
                            }
                        }
                    }  
                }
            }
            catch (Exception ex)
            {
                NomadLog.Error("Error recuperando Cuenta para el usuario: " + NomadProxy.GetProxy().UserName + " - Causa: " + ex.Message);
            }

            xmlResult.SetAttr("cont", cont);
            xmlResult.AddTailElement(xmlOptions);

            return xmlResult;
        }

        private static string GetCuenta(string idEntidad)
        {
            string idcuenta = "";

            string nameFile = NomadProxy.GetProxy().AppName + "_CUENTAS.xml";

            if (NomadProxy.GetProxy().FileServiceIO().ExistsFile("TRELLO", nameFile))
            {
                NomadXML archivoCtas = NomadProxy.GetProxy().FileServiceIO().LoadFileXML("TRELLO", nameFile);

                if(archivoCtas.GetElements2("CUENTA","id_entidad",idEntidad).Count==1)
                {
                    idcuenta = ((NomadXML)archivoCtas.GetElements2("CUENTA", "id_entidad", idEntidad)[0]).GetAttr("id_cuenta");                    
                }
            }

            return idcuenta;
        }

        public static void CACHECuentas()
        {            
            //CACHE_USUARIO
            NomadXML filesCUENTAS = NomadProxy.GetProxy().FileSystem().List(@"Nomad\DB\Nomad\Base\Login\Cuentas");
            ArrayList files = (ArrayList)filesCUENTAS.GetElements("FILE");
            NomadXML xmlCuentasEntidades = new NomadXML("CTAS");

            string nameFile = NomadProxy.GetProxy().AppName + "_CUENTAS.xml";

            for (int i = 0; i < files.Count; i++)
            {
                NomadXML file = (NomadXML)files[i];

                NomadXML xmlCta = new NomadXML("CUENTA");

                NomadXML cuenta = NomadProxy.GetProxy().FileServiceIO().LoadFileXML(@"DB\Nomad\Base\Login\Cuentas", file.GetAttr("NAME"));
                if (cuenta.GetElements("ENTIDAD").Count > 0)
                {
                    xmlCta.SetAttr("id_cuenta", cuenta.GetAttr("id"));
                    xmlCta.SetAttr("id_entidad", ((NomadXML)cuenta.GetElements("ENTIDAD")[0]).GetAttr("value"));
                    xmlCuentasEntidades.AddTailElement(xmlCta);
                }
            }

            if (xmlCuentasEntidades.ChildLength > 0)
                NomadProxy.GetProxy().FileServiceIO().SaveFile("TRELLO", NomadProxy.GetProxy().AppName + "_CUENTAS.xml", xmlCuentasEntidades.ToString(true));
            else
            {
                if (NomadProxy.GetProxy().FileServiceIO().ExistsFile("TRELLO", nameFile))
                    NomadProxy.GetProxy().FileServiceIO().DeleteFile("TRELLO", nameFile);
            }
        }


        /*public static void VincularRecursos()
        {
            ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072; //TLS 1.2
            NomadBatch objBatch;
            objBatch = NomadBatch.GetBatch("Iniciando...", "Iniciando Vinculación");

            objBatch.SetPro(0);

            string strStep = "Recuperando miembros de Nucleus";

            Token tok = new Token('M');
            string tokenTrello = "key=" + tok.key + "&token=" + tok.token;
            //ID ORGANIZACION : nucleus50

            HttpWebRequest request = null;            
            request = (HttpWebRequest)WebRequest.Create(@"https://api.trello.com/1/organizations/nucleus50/members?"+tokenTrello);
            request.ContentType = "application/json";

            objBatch.SetPro(5);

            try
            {
                string result = "";
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                using (StreamReader streamReader = new StreamReader(response.GetResponseStream()))
                {
                    result = streamReader.ReadToEnd();
                }

                objBatch.SetPro(30);

                List<Member> miembros = StringUtil.JSON2Object<List<Member>>(result);
                
                objBatch.SetMess("Armando los emails");
                strStep = "Armando los emails";
                foreach (Member m in miembros)
                {   
                    string mail = "";

                    if(m.userName.Contains("_"))
                    {
                        string[] mailAux = m.userName.Split('_');

                        switch (mailAux[1].Trim())
                        {
                            case "nucleussa":
                                mail += mailAux[0] + "@" + mailAux[1] + ".com.ar";
                                break;
                            case "bayton":
                            case "yahoo":
                                mail += mailAux[0] + "@" + mailAux[1] + ".com.ar";
                                break;
                            case "gmail":
                            case "hotmail":
                            case "outlook":
                                mail += mailAux[0] + "@" + mailAux[1] + ".com";
                                break;
                            default:
                                mail += m.email;
                                break;
                        }

                        m.email = mail;
                    }
                }

                objBatch.SetPro(80);
                objBatch.SetMess("Vinculando recursos con Trello");
                strStep = "Vinculando los recursos con los usuarios Trello";
                foreach (Member m in miembros)
                {
                    if (m.email != null)
                    {
                        string oiMiembro = NomadEnvironment.QueryValue("PRY05_RECURSOS", "oi_recurso", "d_email", m.email, "", false);
                        NucleusProject.Base.Project.Recursos.RECURSO recursoAct = NucleusProject.Base.Project.Recursos.RECURSO.Get(oiMiembro);
                        recursoAct.c_externo = m.id;
                        NomadTransaction.GetTransaction().Save(recursoAct);
                    }
                }
            }
            catch (WebException ex)
            {
                if (ex.Status != WebExceptionStatus.Timeout && ex.Status != WebExceptionStatus.SendFailure)
                {
                    System.Net.HttpWebResponse WResponse = (System.Net.HttpWebResponse)ex.Response;

                    string result = "";
                    using (StreamReader streamReader = new StreamReader(WResponse.GetResponseStream()))
                    {
                        result = streamReader.ReadToEnd();
                    }

                    NomadException nmdEx = NomadException.NewInternalException("NucleusProject.Base.Project.Tareas.TAREA.CrearTarjetaTrello() : " + result, ex);
                    nmdEx.SetValue("step", strStep);
                    throw nmdEx;

                }
                else
                {
                    NomadException nmdEx = NomadException.NewInternalException("NucleusProject.Base.Project.Tareas.TAREA.CrearTarjetaTrello()", ex);
                    nmdEx.SetValue("step", strStep);
                    throw nmdEx;
                }
            }
        }
        */

        #region Entidades
        public class Label
        {
            private string _id;
            private string _name;
            private string _color;

            public string id
            {
                get { return _id; }
                set { _id = value; }
            }
            public string name
            {
                get { return _name; }
                set { _name = value; }
            }
            public string color
            {
                get { return _color; }
                set { _color = value; }
            }

            public Label() { }
        }

        public class Card : Token
        {
            private string _id;
            private string _name;
            private bool _closed;
            private string _desc;
            private string _pos;
            private string _keepFromSource;
            private DateTime? _due;
            private DateTime? _dateLastActivity;
            private string _idList;
            private string _idBoard;
            private string _urlSource;
            private string _shortLink;
            private string _idAttachmentCover;
            private bool _manualCoverAttachment;
            private List<string> _idMembers;
            private Cover _cover;
            public List<Label> labels = new List<Label>();
            public List<CustomFieldItem> customFieldItems = new List<CustomFieldItem>();            
            
            public string id
            {
                get { return _id; }
                set { _id = value; }
            }

            public string name
            {
                get { return _name; }
                set { _name = value; }
            }

            public string desc
            {
                get { return _desc; }
                set { _desc = value; }
            }

            public bool closed
            {
                get { return _closed; }
                set { _closed = value; }
            }

            public string pos
            {
                get { return _pos; }
                set { _pos = value; }
            }

            public string keepFromSource
            {
                get { return _keepFromSource; }
                set { _keepFromSource = value; }
            }

            public DateTime? due
            {
                get { return _due; }
                set { _due = value; }
            }

            public DateTime? dateLastActivity
            {
                get { return _dateLastActivity; }
                set { _dateLastActivity = value; }
            }

            public string idList
            {
                get { return _idList; }
                set { _idList = value; }
            }

            public string idBoard
            {
                get { return _idBoard; }
                set { _idBoard = value; }
            }

            public string urlSource
            {
                get { return _urlSource; }
                set { _urlSource = value; }
            }

            public string shortLink
            {
                get { return _shortLink; }
                set { _shortLink = value; }
            }

            public bool manualCoverAttachment { get { return _manualCoverAttachment; } set { _manualCoverAttachment = value; } }

            public string idAttachmentCover { get { return _idAttachmentCover; } set { _idAttachmentCover = value;  } }

            public System.Collections.Generic.List<string> idMembers
            {
                get { return _idMembers; }
                set { _idMembers = value; }
            }

            public Cover cover { get { return _cover; } set { _cover = value; } }

            public Card() { }

            public class Cover
            {
                private string _idAttachment;
                public List<Scaled> scaled  = new List<Scaled>();

                public string idAttachment { get { return _idAttachment; } set { _idAttachment = value; } }                

                public Cover(){}

                public class Scaled
                {
                    private bool _scaled;
                    private string _url;

                    public bool scaled { get { return _scaled; } set { _scaled = value; } }
                    public string url { get { return _url; } set { _url = value; } }

                    public Scaled(){}

                }
            }
        }

        public class Token
        {
            private string _token;
            private string _key;

            public string token
            {
                get { return _token; }
                set { _token = value; }
            }

            public string key
            {
                get { return _key; }
                set { _key = value; }
            }

            public Token(string grupo)
            {
                //LOGUEO
                NomadXML Ctx = NomadProxy.GetProxy().ReadContext();
                NomadXML DBs = Ctx.FindElement("dbs");
                NomadXML DB = DBs.FindElement2("db", "id", grupo);
                
                if(DB != null)
                {
                    this.key = DB.GetAttr("key");
                    this.token = DB.GetAttr("token");
                }

                //TRELLOADMIN
                //this.key = "61465b56e660273dba47527727b88505";
                //this.token = "daafbbb375e813da09cd866ec6bdff070870c00b89e1f820aaadc3876266bba8"; 

                //FACUNDO
                //this.key = "697fd2e872bdc183ada8c96441e1691e"; 
                //this.token = "4f244ee060cbe0cc19bde7f247d16be1cdba483db5c4549a0af7f1b834f74670";

                //ROBERTO
                //this.key = "16bab4be32fb26ddcaa2f9ddaf31d037"; 
                //this.token = "44286e91893924188061e5fbabcaca56f3369b3a83a61497759009282766d998";
 
                //MATIAS
                //this.key = "32318a3e5bc6b18648226ca3b469c422"; 
                //this.token = "e941dd8986d8cd4f30095970148135ba33d14c0b3104badf60eaf9e30806ba14";
            }

            //TRELLOADMIN
            public Token()
            {
                this.key = "61465b56e660273dba47527727b88505"; //TRELLOADMIN
                this.token = "daafbbb375e813da09cd866ec6bdff070870c00b89e1f820aaadc3876266bba8"; // TRELLOADMIN
            }
        }

        public class Lista
        {
            private string _name;
            private string _id;

            public string name
            {
                get { return _name; }
                set { _name = value; }
            }

            public string id
            {
                get { return _id; }
                set { _id = value; }
            }
        }

        public class Board
        {
            private string _name;
            private string _id;
            private List<Lista> _lists;

            public List<Lista> lists
            {
                get { return _lists; }
                set { _lists = value; }
            }

            public string name
            {
                get { return _name; }
                set { _name = value; }
            }

            public string id
            {
                get { return _id; }
                set { _id = value; }
            }
        }

        public class CustomField
        {
            private string _id;
            private string _name;
            private string _type;
            private string _campo;

            public string id { get { return _id; } set { _id = value; } }
            public string name { get { return _name; } set { _name = value; } }
            public string type { get { return _type; } set { _type = value; } }
            public string campo { get { return _campo; } set { _campo = value; } }

            public CustomField() { }
        }

        public class CustomFieldItem
        {
            private string _idCustomField;
            private Dictionary<string, string> _value;

            public string idCustomField { get { return _idCustomField; } set { _idCustomField = value; } }
            public Dictionary<string, string> value { get { return _value; } set { _value = value; } }

            public CustomFieldItem() { }
        }

        public class Member : Token
        {
            private string _id;
            private string _email;
            private string _fullName;
            private string _userName;
            private string _initials;

            public string id
            {
                get { return _id; }
                set { _id = value; }
            }

            public string initials
            {
                get { return _initials; }
                set { _initials = value; }
            }

            public string email
            {
                get { return _email; }
                set { _email = value; }
            }

            public string fullName
            {
                get { return _fullName; }
                set { _fullName = value; }
            }

            public string userName
            {
                get { return _userName; }
                set { _userName = value; }
            }
        }

        public class Search
        {
            private List<Card> _cards;
            private List<Board> _boards;
            private List<Member> _members;

            public List<Card> cards
            {
                get { return _cards; }
                set { _cards = value; }
            }

            public List<Board> boards
            {
                get { return _boards; }
                set { _boards = value; }
            }

            public List<Member> members
            {
                get { return _members; }
                set { _members = value; }
            }

            public Search() { }
        }
        #endregion        
               
    }
}


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

namespace NucleusRH.Base.SeleccionDePostulantes.Avisos
{
    //////////////////////////////////////////////////////////////////////////////////
    //Clase Avisos
    public partial class AVISO : Nomad.NSystem.Base.NomadObject
    {

        //Método utilizado desde el WF de solicitud de aviso
        public static string AddAviso(SortedList<string,object> AVISO)
    {
      NomadLog.Debug("-----------------------------------------");
      NomadLog.Debug("-----------AGREGAR AVISO-----------------");
      NomadLog.Debug("-----------------------------------------");

      NucleusRH.Base.SeleccionDePostulantes.Avisos.AVISO AVI = new NucleusRH.Base.SeleccionDePostulantes.Avisos.AVISO();

      foreach (KeyValuePair<string, object> kvp in AVISO)
        {
          switch (kvp.Key)
            {
              case "TIPO":
               if (kvp.Value != null)
                AVI.c_tipo = (string)kvp.Value;
                break;
              case "TJOR":
               if (kvp.Value != null)
                 AVI.oi_tipo_jor_lab = (string)kvp.Value;
                 break;
              case "DESC":
              if (kvp.Value != null)
                 AVI.o_aviso = (string)kvp.Value;
                break;
              case "PUESTO":
                if (kvp.Value != null)
                {
                  NucleusRH.Base.SeleccionDePostulantes.Puestos.PUESTO ddoPUESTO = NucleusRH.Base.SeleccionDePostulantes.Puestos.PUESTO.Get((string)kvp.Value);
                  AVI.oi_puesto = ddoPUESTO.id.ToString();
                }
                break;
              case "DESC_PUESTO":
                if (kvp.Value != null)
                AVI.d_puesto = (string)kvp.Value;
                break;
              case "F_PUBLICA":
              if (kvp.Value != null)
                AVI.f_aviso = (DateTime)kvp.Value;
                break;
              case "F_FIN":
              if (kvp.Value != null)
                AVI.f_cierre = (DateTime)kvp.Value;
                break;
              case "WEB":
              if (kvp.Value != null)
              {
                string valor1 = (string)kvp.Value;
                if(valor1=="1")
                  AVI.l_publicar = true;
                else
                  AVI.l_publicar = false;
              }
                break;
              case "SEXO":
                 AVI.c_sexo = (string)kvp.Value;
                break;
              case "MAIL1":
              if (kvp.Value != null)
                AVI.d_mail = (string)kvp.Value;
                break;
              case "MAIL2":
              if (kvp.Value != null)
                AVI.d_mail_alt_1 = (string)kvp.Value;
                break;
              case "MAIL3":
              if (kvp.Value != null)
                AVI.d_mail_alt_2 = (string)kvp.Value;
                break;
              case "VEHICULO":
              if (kvp.Value != null)
              {
                string valor2 = (string)kvp.Value;
                if(valor2=="1")
                  AVI.l_vehiculo = true;
                else
                  AVI.l_vehiculo = false;
              }
                break;
              case "LIC":
                      if (kvp.Value != null)
              {
                string valor3 = (string)kvp.Value;
                if(valor3=="1")
                  AVI.l_lic_conduc = true;
                else
                  AVI.l_lic_conduc = false;
              }
                break;
              case "DISP_R":
              if (kvp.Value != null)
                AVI.c_radicarse = (string)kvp.Value;
                break;
              case "DISP_V":
              if (kvp.Value != null)
                AVI.c_viajar = (string)kvp.Value;
                break;
              case "INFO_MAIL":
                if (kvp.Value != null)
                {
                  string valor4 = (string)kvp.Value;
                  if(valor4=="1")
                      AVI.l_envio_mail_ok = true;
                  else
                      AVI.l_envio_mail_ok = false;
                }
                break;
              case "FEC_MAIL":
              if (kvp.Value != null)
                AVI.f_envio_mail = (DateTime)kvp.Value;
                break;
             }
        }
        try
        {
          AVI.c_estado = "P";
          AVI.f_estado = DateTime.Now;

          NomadLog.Debug("Llamada al método GuardarAviso existente en AVISO");

          if(AVI.l_envio_mail_ok)
            AVI.GuardarAviso("1",AVI.f_envio_mail);
          else
            AVI.GuardarAviso("0",AVI.f_envio_mail);

          return "1";
        }
        catch (Exception ex)
        {
          NomadLog.Debug("Error guardando AVISO: " + ex);
           return "0";
        }
    }

        public void GuardarAvisoJobs()
        {
          GuardarDependencias();
        }
        public void GuardarAviso(string l_mail_aviso, DateTime f_envio_mail)
        {
            NomadBatch objBatch;
            objBatch = NomadBatch.GetBatch("Iniciando...", "Publicacion de Aviso");

            objBatch.SetMess("Registrando datos del Aviso...");
            GuardarDependencias();
            objBatch.SetMess("Registrando la Publicacion...");
            objBatch.SetPro(20);

            //SECCION QUE SE ENCARGA DE ENVIAR MAIL A LOS CVs QUE ASI LO INDICAN
            if (l_mail_aviso == "1")
            {
                RPCService objRPC = NomadProxy.GetProxy().RPCService();
                objRPC.AddParam(new RPCParam("oi_aviso", "IN", this.Id));
                objRPC.Execute("PublicacionAviso_" + this.Id, 1, f_envio_mail, "MAIL", "NucleusRH.Base.SeleccionDePostulantes.Avisos.AVISO", "EnviarMailsBatch");
            }
            objBatch.SetPro(100);
        }

        private void GuardarDependencias()
        {
           //SECCION QUE GUARDA INFORMACION DE COSTOS, EXAMENES Y ENTREVISTAS PARA EL AVISO EN FUNCION DEL PUESTO
            if (!this.oi_puestoNull)
            {
                //RECUPERO EL PUESTO
                NucleusRH.Base.SeleccionDePostulantes.Puestos.PUESTO ddoPUESTO = NucleusRH.Base.SeleccionDePostulantes.Puestos.PUESTO.Get(this.oi_puesto);

                //SI EL PUESTO USA PLANTILLA
                //if (ddoPUESTO.l_usa_plantilla)
                //{
                    //RECORRO LOS COSTOS Y SE LOS CARGO AL AVISO
                    foreach (NucleusRH.Base.SeleccionDePostulantes.Puestos.COSTO_PUESTO ddoPUESTOCOS in ddoPUESTO.PUESTO_COSTO)
                    {
                        NucleusRH.Base.SeleccionDePostulantes.Avisos.AVISO_COSTO ddoAVISOCOS = new NucleusRH.Base.SeleccionDePostulantes.Avisos.AVISO_COSTO();
                        ddoAVISOCOS.oi_item_costo = ddoPUESTOCOS.oi_item_costo;

                        this.AVISO_COSTO.Add(ddoAVISOCOS);
                    }

                    //RECORRO LOS EXAMENES Y SE LOS CARGO AL AVISO
                    foreach (NucleusRH.Base.SeleccionDePostulantes.Puestos.PUESTO_EXAMEN ddoPUESTOEXA in ddoPUESTO.PUESTO_EXAMEN)
                    {
                        NucleusRH.Base.SeleccionDePostulantes.Avisos.AVISO_EXA ddoAVISOEXA = new NucleusRH.Base.SeleccionDePostulantes.Avisos.AVISO_EXA();
                        ddoAVISOEXA.oi_examen = ddoPUESTOEXA.oi_examen;

                        this.AVISO_EXA.Add(ddoAVISOEXA);
                    }

                    //RECORRO LAS ENTREVISTAS Y SE LAS CARGO AL AVISO
                    foreach (NucleusRH.Base.SeleccionDePostulantes.Puestos.PUESTO_ENTREV ddoPUESTOENTREV in ddoPUESTO.PUESTO_ENTREVISTA)
                    {
                        NucleusRH.Base.SeleccionDePostulantes.Avisos.AVISO_ENTREV ddoAVISOENTREV = new NucleusRH.Base.SeleccionDePostulantes.Avisos.AVISO_ENTREV();
                        ddoAVISOENTREV.oi_entrevista = ddoPUESTOENTREV.oi_entrevista;

                        this.AVISO_ENTREVISTA.Add(ddoAVISOENTREV);
                    }
                //}

            }
              //GUARDO EL AVISO
            this.l_envio_mail_ok = false;

             NomadEnvironment.GetCurrentTransaction().SaveRefresh(this);

        }
        public static void Reclutar(string oi_aviso, Nomad.NSystem.Proxy.NomadXML param)
        {
            NomadXML myXML = new NomadXML("<CVS><CV id=\"4\" merito=\"105\"/><CV id=\"40\" merito=\"105\"/><CV id=\"6\" merito=\"95\"/></CVS>");
            NomadProxy.GetProxy().FileServiceIO().SaveFile("TEMP", "RECLUTAMIENTO_" + NomadProxy.GetProxy().Batch().ID + ".xml", myXML.ToString());
        }

        public static void EnviarMailsBatch(string oi_aviso)
        {
            BatchService objRPC = NomadProxy.GetProxy().BatchService();
            objRPC.AddParam(new RPCParam("oi_aviso", "IN", oi_aviso));
            objRPC.Execute("NucleusRH.Base.SeleccionDePostulantes.Avisos.AVISO", "EnviarMails", 5);
        }

        public static void EnviarMails(string oi_aviso)
        {
            NomadEnvironment.GetTrace().Info("ENVIANDO MAIL DE AVISO...");
            NomadXML xmlInfo = new NomadXML("AVISO");

            //OBTENGO EL AVISO
            AVISO ddoAVISO = AVISO.Get(oi_aviso, false);

            //RECUPERO LA LISTA DE CVs A LA QUE HAY QUE ENVIARLES MAILS
            NomadXML xmlResult;
            xmlResult = NomadEnvironment.QueryNomadXML(AVISO.Resources.QRY_MAILLIST, "");

            //traigo la parametros
            NomadXML xmlParams = NomadEnvironment.QueryNomadXML(AVISO.Resources.QRY_PARAM, "");

            string strWebName = xmlParams.FirstChild().GetAttr("strWebName");
            string urlWeb = xmlParams.FirstChild().GetAttr("urlWeb");

            xmlInfo.SetAttr("d_puesto", ddoAVISO.d_puesto);

            //defindo el mail de respuesta
            string mailrsp = ddoAVISO.d_mail_alt_2;
            if (ddoAVISO.d_mail_alt_1 != "")
                mailrsp = ddoAVISO.d_mail_alt_1;
            if (ddoAVISO.d_mail_alt_2 != "")
                mailrsp = ddoAVISO.d_mail;

            //RECORRO LA LISTA DE CVs RECUPERADOS
            int l = xmlResult.FirstChild().ChildLength;
            int c = 0;
            for (NomadXML xmlcur = xmlResult.FirstChild().FirstChild(); xmlcur != null; xmlcur = xmlcur.Next())
            {
                c++;

                NomadLog.Info("xmlcur -- " + xmlcur.ToString());
                CVs.CV objCV = CVs.CV.Get(xmlcur.GetAttr("oi_cv"), false);

                Hashtable htaMails = CVs.CV.GetMails("AVISO", objCV, xmlInfo);

                //CREO EL MAIL
                Nomad.Base.Mail.Mails.MAIL objMAIL = Nomad.Base.Mail.Mails.MAIL.CrearTXT(htaMails.ContainsKey("AVISO_ASUNTO") ? (string)htaMails["AVISO_ASUNTO"] : "-", htaMails.ContainsKey("AVISO_TEXTO") ? (string)htaMails["AVISO_TEXTO"] : "-", xmlcur.GetAttr("d_email"), mailrsp != "" ? mailrsp : "noresponder@dominio.com");
                objMAIL.Enviar();
            }

      ddoAVISO = AVISO.Get(oi_aviso, false);
            ddoAVISO.l_envio_mail_ok = true;
            NomadEnvironment.GetCurrentTransaction().Save(ddoAVISO);
        }

        public static System.Collections.Generic.SortedList<string, string> DeleteAviso(string PAR)
        {
            NomadLog.Debug("------------------------------------");
            NomadLog.Debug("-----------DELETE AVISO-------------");
            NomadLog.Debug("------------------------------------");

            NomadLog.Debug("DeleteAviso.oi_aviso: " + PAR);

            SortedList<string, string> retorno = new SortedList<string, string>();
            retorno.Add("VAL", "FATALERR");
            retorno.Add("DES", "El método no fue procesado");

            //INICIO UNA TRANSACCION
            NomadTransaction objTran = null;

            try
            {
                //Get Aviso
                NucleusRH.Base.SeleccionDePostulantes.Avisos.AVISO AVISO = NucleusRH.Base.SeleccionDePostulantes.Avisos.AVISO.Get(PAR, true);

                if (AVISO == null)
                    throw new Exception("El aviso con Id: "+ PAR + " no se ha encontrado.");

                objTran = new NomadTransaction();
                objTran.Begin();

                //Elimino el DDO AVISO
                if (AVISO.AVISO_COSTO.Count > 0)
                {
                    NucleusRH.Base.SeleccionDePostulantes.Avisos.AVISO_COSTO aviso_costo;
                    foreach (AVISO_COSTO avi_cos in AVISO.AVISO_COSTO)
                    {
                        aviso_costo = NucleusRH.Base.SeleccionDePostulantes.Avisos.AVISO_COSTO.Get(avi_cos.id);
                        objTran.Delete(aviso_costo);
                        //NomadEnvironment.GetCurrentTransaction().Delete(aviso_costo);
                    }
                }

                if (AVISO.AVISO_ENTREVISTA.Count > 0)
                {
                    NucleusRH.Base.SeleccionDePostulantes.Avisos.AVISO_ENTREV aviso_entrevista;
                    foreach (AVISO_ENTREV avi_ent in AVISO.AVISO_ENTREVISTA)
                    {
                        aviso_entrevista = NucleusRH.Base.SeleccionDePostulantes.Avisos.AVISO_ENTREV.Get(avi_ent.id);
                        //NomadEnvironment.GetCurrentTransaction().Delete(aviso_entrevista);
                        objTran.Delete(aviso_entrevista);
                    }
                }

                if (AVISO.AVISO_EXA.Count > 0)
                {
                    NucleusRH.Base.SeleccionDePostulantes.Avisos.AVISO_EXA aviso_examen;
                    foreach (AVISO_EXA avi_exa in AVISO.AVISO_EXA)
                    {
                        aviso_examen = NucleusRH.Base.SeleccionDePostulantes.Avisos.AVISO_EXA.Get(avi_exa.id);
                        //NomadEnvironment.GetCurrentTransaction().Delete(aviso_examen);
                        objTran.Delete(aviso_examen);
                    }
                }

                //Elimino el aviso
                //NomadEnvironment.GetCurrentTransaction().Delete(AVISO);
                objTran.Delete(AVISO);
                objTran.Commit();
                retorno = NucleusRH.Base.Configuracion.Herramientas.QUERIES.CreateRTA("OK","El aviso se eliminó exitosamente");
                return retorno;
            }
            catch (Exception ex)
            {
                if (objTran != null)
                    objTran.Rollback();
                NomadLog.Debug("Error eliminando AVISO: " + ex);
                retorno = NucleusRH.Base.Configuracion.Herramientas.QUERIES.CreateRTA("ERR", ex.Message);
                return retorno;
            }
        }
    }
}


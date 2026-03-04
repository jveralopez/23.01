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

namespace NucleusRH.Base.Configuracion.Tareas
{

    //////////////////////////////////////////////////////////////////////////////////
    //Clase Alertas
    public partial class TAREA : Nomad.NSystem.Base.NomadObject
    {

		public static void SetValues(NomadXML xmlAL, NomadXML MyNODE, NomadXML xmlMDGR)
		{
			switch (xmlMDGR.GetAttr("type"))
			{
				case "simple":
					//Recorro las Variables
					for (NomadXML xmlMDVAR = xmlMDGR.FirstChild(); xmlMDVAR != null; xmlMDVAR = xmlMDVAR.Next())
						Nomad.Base.Report.Variables.SetValue(xmlAL, -1, xmlMDGR.GetAttr("name") + "." + xmlMDVAR.GetAttr("name"), xmlMDVAR.GetAttr("type"), MyNODE.GetAttr(xmlMDVAR.GetAttr("name")), null);
					break;

				case "coleccion":
					//Agrego el Grupo
					Nomad.Base.Report.Variables.AddItem(xmlAL, xmlMDGR.GetAttr("name"));

					//Nuevo Indice
					int idx = Nomad.Base.Report.Variables.CountItem(xmlAL, xmlMDGR.GetAttr("name"));

					//Recorro las Variables
					for (NomadXML xmlMDVAR = xmlMDGR.FirstChild(); xmlMDVAR != null; xmlMDVAR = xmlMDVAR.Next())
						Nomad.Base.Report.Variables.SetValue(xmlAL, idx - 1, xmlMDGR.GetAttr("name") + "." + xmlMDVAR.GetAttr("name"), xmlMDVAR.GetAttr("type"), MyNODE.GetAttr(xmlMDVAR.GetAttr("name")), null);
					break;
			}
		}

		public static NomadXML FindEtty(NomadXML xmlROOT, string ettyName)
		{
			if (xmlROOT.Name==ettyName) return xmlROOT;

			NomadXML xmlFND;

			//Esta en el RAIZ?
			xmlFND=xmlROOT.FindElement(ettyName);
			if (xmlFND!=null) return xmlFND;


			for (NomadXML xmlCUR = xmlROOT.FirstChild(); xmlCUR != null; xmlCUR = xmlCUR.Next())
			{
				xmlFND = FindEtty(xmlCUR, ettyName);
				if (xmlFND!=null) return xmlFND;
			}

			return null;
		}


		public static void RunBatch(int oi_tarea, NomadXML xmlPARAM)
		{
			DateTime f_inicio = DateTime.Now;

			//Obtengo los DDOs
			NomadLog.Debug("Obtengo la TAREA.");
			TAREA MyTAREA = TAREA.Get(oi_tarea, false);

			//Limpio las Ejecuciones Falladas
			foreach(EJECUCION testEJ in MyTAREA.EJECUCIONES)
			{
				if (testEJ.c_estado=="F") continue;
				if (testEJ.c_estado=="E") continue;
				testEJ.c_estado="E";
				testEJ.d_ejecucion = "Ejecutar la tarea, Inicio: " + f_inicio.ToString("HH:mm");
				testEJ.f_fin = DateTime.Now;
			}

			while(MyTAREA.EJECUCIONES.Count>19)
				MyTAREA.EJECUCIONES.RemoveAt(0);

			//Agrego la Ejecucion
			EJECUCION MyEJEC= new EJECUCION();
			MyEJEC.c_estado = "R";
			MyEJEC.d_ejecucion = "Ejecutar la tarea, hora: " + f_inicio.ToString("HH:mm");
			MyEJEC.f_ejecucion = DateTime.Now;
			MyEJEC.d_log = NomadProxy.GetProxy().Batch().ID;
			MyTAREA.EJECUCIONES.Add(MyEJEC);

			NomadLog.Debug("Actualizo la Tarea.");
			NomadEnvironment.GetCurrentTransaction().SaveRefresh(MyTAREA);

			string strDest = "";
			NomadXML DefTarea=null;
			Nomad.Base.Mail.Mails.MAIL sendMAIL;
			NucleusRH.Base.Configuracion.Mails.MAIL mailDef = null;
			NucleusRH.Base.Configuracion.MailTypes.MAILSUBTYPE MyMailType;
			try
			{
				//Obtengo la Ejecucion
				MyEJEC = (EJECUCION)MyTAREA.EJECUCIONES.GetByAttribute("c_estado", "R");

				// Definicion
				NomadLog.Debug("Obtengo la definicion de la TAREA.");
				DefTarea = NomadProxy.GetProxy().FileService().LoadFileXML("TASK", MyTAREA.d_tipo_tarea);
				if (DefTarea.isDocument) DefTarea=DefTarea.FirstChild();

				// Objeto MAIL
				NomadLog.Debug("Obtengo la definicion del MAIL.");
				mailDef=NucleusRH.Base.Configuracion.Mails.MAIL.Get(MyTAREA.oi_mail,false);

				Hashtable hDest = new Hashtable();
				foreach (DESTINATARIO MyDEST in MyTAREA.DESTINATARIOS)
				{
					switch (MyDEST.c_tipo)
					{
						case "ETTY":
							Nomad.Base.Login.Entidades.ENTIDAD MyETTY = Nomad.Base.Login.Entidades.ENTIDAD.Get(MyDEST.d_destinatario_id);

							foreach (Nomad.Base.Login.Entidades.MAIL MyETTYMAIL in MyETTY.MAILS)
							{
								if (!MyETTYMAIL.PRINCIPAL) continue;
								foreach (string strMails in MyETTYMAIL.EMAIL.Split(';'))
									hDest[strMails.ToUpper()] = strMails;
							}
							break;
						case "MAIL":
							foreach (string strMails in MyDEST.d_destinatario_id.Split(';'))
								hDest[strMails.ToUpper()] = strMails;
							break;
					}
				}

				foreach (string strMail in hDest.Values)
					if (strMail != "")
						strDest += (strDest == "" ? "" : ";") + strMail;


				string methodFullname = DefTarea.GetAttr("method");
				string methodClass = methodFullname.Substring(0,methodFullname.LastIndexOf('.'));
				string methodName = methodFullname.Substring(methodFullname.LastIndexOf('.') + 1);

				
				//Obtengo la CLASE
				NomadLog.Debug("Ejecuto el METODO: " + methodFullname);
              
                Type MyClassType = Nomad.Base.Scheduler.Tareas.TAREA.FindType(methodFullname);

				//Ejecuto el METODO

                NomadXML xmlNodoParam = xmlPARAM.FirstChild();
                
                object[] args = new object[1];
				args[0]=xmlPARAM;
                            
				//Resultado
				NomadXML xmlRET=null;
				try {
					xmlRET=(NomadXML)MyClassType.InvokeMember(methodName, BindingFlags.Public|BindingFlags.InvokeMethod|BindingFlags.Static, null, null, args);
				}
				catch (Exception Ex)
				{
					if (Ex.InnerException==null)
						throw;

					NomadException MyEx = NomadException.NewInternalException("TASK.InvokeMember", Ex.InnerException);
					MyEx.SetValue("class", methodClass);
					MyEx.SetValue("method", methodName);
					MyEx.SetValue("param", xmlPARAM.ToString());
					throw MyEx;
				}

				//Envio de MAIL
				if (MyTAREA.l_notificar && strDest != "")
				{
					//
					string TraceFileName=null;

					if (MyTAREA.c_log_level!="NONE")
					{
						NomadLog.Debug("Genero el Reporte de LOG: "+MyTAREA.c_log_level);

						NomadXML Params=new NomadXML("LOGS");
						Params.SetAttr("task", MyTAREA.d_tarea);
						Params.SetAttr("ini", f_inicio.ToString("dd/MM/yyyy HH:mm"));
						Params.SetAttr("fin", DateTime.Now.ToString("dd/MM/yyyy HH:mm"));

						int logLev=0;
						switch(MyTAREA.c_log_level.ToUpper())
						{
							case "ERR": Params.SetAttr("log", "Solo Errores"); logLev=1; break;
							case "WRN": Params.SetAttr("log", "Advertencias y Errores"); logLev=2; break;
							case "IFO": Params.SetAttr("log", "Información, Advertencias y Errores"); logLev=3; break;
						}


						NomadXML tempLOG=new NomadXML("LOGS");

						string TraceID=NomadProxy.GetProxy().Batch().Trace.ID;
						//string TraceID="000013F4-000013CC-20130530-164115-00000001";

						if (TraceID!="")
						{
							MemoryStream MyStream=new MemoryStream(NomadProxy.GetProxy().FileSystem().Download("./LOGS/TRACE/"+TraceID+".trace.log"));
							StreamReader MyReader=new StreamReader(MyStream);
							tempLOG.AddText(MyReader.ReadToEnd());
							MyReader.Close();

							int lev;
							for(NomadXML cur=tempLOG.FirstChild(); cur!=null; cur=cur.Next())
							{
								lev=0;
								switch(cur.GetAttr("type").ToUpper())
								{
									case "ERR": lev = 1; break;
									case "WRN": lev = 2; break;
									case "IFO": lev = 3; break;
								}

								if (lev<=logLev) Params.AddXML(cur);
							}
						}
						
						NomadLog.Debug("Registros leidos: "+Params.ChildLength);
						if (Params.ChildLength > 0)
						{
							string UUID = Nomad.NSystem.Functions.StringUtil.GenerateUUID();
							string TEMPPath = NomadProxy.GetProxy().RunPath + "TEMP\\" + UUID;
							System.IO.Directory.CreateDirectory(TEMPPath);

							//Cargo el Reporte
							NomadLog.Debug("Cargo el Reporte.");
							Nomad.NSystem.Report.NmdReport myRTP = new Nomad.NSystem.Report.NmdReport(NomadProxy.GetProxy().FileService().LoadFile("REPORTS", "NucleusRH.Base.Configuracion.TraceLog.rpt.XML"), "", null);
							myRTP.SetData(Params.ToString());

							//Aplico el XSL
							NomadLog.Info("Aplico el XSL.");
							string outFileName = TEMPPath + "\\EXPORT.tmp";
							System.IO.StreamWriter ResultFileStream = new System.IO.StreamWriter(outFileName, false, System.Text.Encoding.UTF8);
							myRTP.GenerateReport(ResultFileStream.BaseStream);
							ResultFileStream.Close();

							//Genero el PDF
							TraceFileName=TEMPPath + "\\TRACE.pdf";
							NomadLog.Info("Genero el PDF.");
							Nomad.NSystem.Report.NmdFoRender fo = new Nomad.NSystem.Report.NmdFoRender(null);
							fo.GeneratePDF(outFileName, TraceFileName);
						}
						 
					}
					NomadLog.Debug("Envio el MAIL");

					//Creo el MAIL
					sendMAIL = Nomad.Base.Mail.Mails.MAIL.Crear();
					sendMAIL.ASUNTO = mailDef.d_asunto;
					sendMAIL.CUERPO = mailDef.o_text1 + mailDef.o_text2 + mailDef.o_text3 + mailDef.o_text4;
					sendMAIL.TIPO = "W";
					sendMAIL.REMITENTE_DIR = mailDef.d_reply_mail;

					//Agrego los Destinatarios
					foreach (string strMails in strDest.Split(';'))
						Nomad.Base.Mail.Mails.MAIL.AgregarDestinatario(sendMAIL, strMails);

					//Variables
					MyMailType = NucleusRH.Base.Configuracion.MailTypes.MAILSUBTYPE.Get(mailDef.oi_subtype_mail);

					NomadXML xmlAL = new NomadXML(MyMailType.o_variables);
					if (xmlAL.isDocument) xmlAL = xmlAL.FirstChild();

					NomadXML xmlMD = xmlAL.FindElement("METADATA");
					NomadXML xmlDATA, xmlDATAITEM, xmlMDGR;

					//Agrego los elementos CHILDS...
					for (xmlMDGR = xmlMD.FirstChild(); xmlMDGR != null; xmlMDGR = xmlMDGR.Next())
					{
						xmlDATA=FindEtty(xmlRET, xmlMDGR.GetAttr("name"));

						if (xmlDATA!=null)
						{
							switch(xmlMDGR.GetAttr("type"))
							{
								case "simple":
									SetValues(xmlAL, xmlDATA, xmlMDGR);
									break;

								case "coleccion":
									for(xmlDATAITEM=xmlDATA.FirstChild(); xmlDATAITEM!=null; xmlDATAITEM=xmlDATAITEM.Next())
										SetValues(xmlAL, xmlDATAITEM, xmlMDGR);
									break;
							}
						}
					}
					NomadLog.Debug("--VARIABLES--" + xmlAL.ToString());

					//Agrego los ATTACH
					if (TraceFileName!=null)
					{
						//Creo el Adjunto
						Nomad.Base.Mail.Mails.ADJUNTO log=new Nomad.Base.Mail.Mails.ADJUNTO();
						log.TIPO="FS";
						log.RECURSO=TraceFileName;

						//Lo agrego a la lista
						sendMAIL.ADJUNTOS.Add(log);
					}

					//Envio el MAIL
					sendMAIL.Enviar(xmlAL);
				}

				//Informacion de la Ejecucion
				BatchService MyBATCH=NomadProxy.GetProxy().Batch();
				//MyBATCH.Join("00001094-00001328-20130531-102002-00000000");
				if (MyBATCH.ID!="")
				{
					NomadXML xmlBATCH = new NomadXML(MyBATCH.Refresh());
					if (xmlBATCH.isDocument) xmlBATCH = xmlBATCH.FirstChild();
					NomadXML xmlFORM=xmlBATCH.FindElement("FORM");
					MyEJEC.e_err = xmlFORM.GetAttrInt("ERR");
					MyEJEC.e_wrn = xmlFORM.GetAttrInt("WRN");
					MyEJEC.e_ifo = xmlFORM.GetAttrInt("IFO");
				}

				//Actualizo la Ejecucion
				NomadLog.Debug("Guardo la Ejecucion - OK");
				MyEJEC.d_ejecucion = "Ejecutar la tarea, Inicio: " + f_inicio.ToString("HH:mm") + ", Fin: " + DateTime.Now.ToString("HH:mm");
				MyEJEC.f_fin = DateTime.Now;
				MyEJEC.c_estado = "F";
				NomadEnvironment.GetCurrentTransaction().SaveRefresh(MyTAREA);
			} catch(Exception Ex)
			{
				NomadLog.Debug("Genero el ERROR");
				NomadException MyEx = NomadException.NewInternalException("TASK.RunBatch", Ex);
				MyEx.SetValue("oi_tarea", oi_tarea.ToString());
				MyEx.SetValue("XML", xmlPARAM.ToString());
				MyEx.Dump();

				MyEJEC.e_err = 1;
				MyEJEC.e_wrn = 0;
				MyEJEC.e_ifo = 0;
				try
				{
					//Informacion de la Ejecucion
					BatchService MyBATCH = NomadProxy.GetProxy().Batch();
					NomadXML xmlBATCH = new NomadXML(MyBATCH.Refresh());
					if (xmlBATCH.isDocument) xmlBATCH = xmlBATCH.FirstChild();
					NomadXML xmlFORM = xmlBATCH.FindElement("FORM");

					MyEJEC.e_err += xmlFORM.GetAttrInt("ERR");
					MyEJEC.e_wrn += xmlFORM.GetAttrInt("WRN");
					MyEJEC.e_ifo += xmlFORM.GetAttrInt("IFO");
				} catch (Exception) { }


				//Actualizo la Ejecucion
				NomadLog.Debug("Guardo la Ejecucion - CON ERROR - "+MyEx.Id);
				MyEJEC.d_ejecucion = "Ejecutar la tarea, Inicio: "+f_inicio.ToString("HH:mm")+", Fallo: "+DateTime.Now.ToString("HH:mm");
				MyEJEC.f_fin = DateTime.Now;
				MyEJEC.c_estado = "E";
				NomadEnvironment.GetCurrentTransaction().SaveRefresh(MyTAREA);

				//Envio de MAIL
				if (MyTAREA.l_notificar && strDest!="" && mailDef!=null && DefTarea!=null)
				{
					sendMAIL = Nomad.Base.Mail.Mails.MAIL.Crear();
					sendMAIL.ASUNTO = "Error al ejecutar la TAREA";
					sendMAIL.CUERPO_TXT = "Se produjo un error al ejecutar la Tarea "+MyTAREA.Id+"-"+MyTAREA.d_tarea+"\nGenerando el ID de Error "+MyEx.Id+"\n";
					sendMAIL.TIPO = "T";
					sendMAIL.REMITENTE_DIR = mailDef.d_reply_mail;

					foreach (string strMails in strDest.Split(';'))
						Nomad.Base.Mail.Mails.MAIL.AgregarDestinatario(sendMAIL, strMails);

					sendMAIL.Enviar();
				}
			}

			NomadLog.Debug("Fin");
		}

		public static void Run(int oi_tarea, NomadXML xmlEVENT)
        {
            //Obtengo los DDOs
            TAREA MyTAREA = TAREA.Get(oi_tarea, false);
			NucleusRH.Base.Configuracion.Params.PARAM MyPARAMS = NucleusRH.Base.Configuracion.Params.PARAM.Get(MyTAREA.oi_param, false);
			NucleusRH.Base.Configuracion.Mails.MAIL MyMAIL = NucleusRH.Base.Configuracion.Mails.MAIL.Get(MyTAREA.oi_mail, false);

            // PARAMETROS
            NomadLog.Debug("Genero los PARAMETROS.");
            NomadXML xmlPARAM = new NomadXML("PARAMS");
            foreach (NucleusRH.Base.Configuracion.Params.DETAIL MyPARAM_DET in MyPARAMS.DETAILS)
                xmlPARAM.SetAttr(MyPARAM_DET.c_detail, MyPARAM_DET.d_value);

			// INFORMACION DEL EVENTO
            if (xmlEVENT != null)
            {
                string[] aIDS = xmlEVENT.GetAttr("IDS").Split(',');
                string sIDS = "";

                for (int t = 0; t < aIDS.Length; t++)
                {
                    sIDS += (sIDS.Length > 0 ? "," : "") + aIDS[t];
                    if (sIDS.Length < 100 || t == aIDS.Length - 1)
                    {
                        xmlEVENT.SetAttr("IDS", sIDS);
                        xmlPARAM.AddXML(xmlEVENT);

                        sIDS = "";
                    }
                }

                xmlPARAM.SetAttr("eve_keyname", xmlEVENT.GetAttr("eve_keyname"));
                xmlPARAM.SetAttr("eve_name", xmlEVENT.GetAttr("eve_name"));
            }
            if (xmlPARAM.FirstChild() == null)
                xmlPARAM.AddTailElement("EVE");

			//Tengo que Ejecutar el metodo BATCH
			BatchService myRPC=NomadProxy.GetProxy().BatchService();
			myRPC.AddParam(new RPCParam("oi_tarea","IN", oi_tarea));
			myRPC.AddParam(new RPCParam("xmlIN","IN", "Nomad.NSystem.Object", "Nomad.NSystem.Proxy.NomadXML", xmlPARAM.ToString()));

			myRPC.Execute("NucleusRH.Base.Configuracion.Tareas.TAREA", "RunBatch", 5);
        }
		
        public static void Guardar(NucleusRH.Base.Configuracion.Tareas.TAREA DDO)
        {
            int t;
            NucleusRH.Base.Configuracion.Progs.PROG MyPROG;
            NucleusRH.Base.Configuracion.Mails.MAIL MyMAIL;
            NucleusRH.Base.Configuracion.Params.PARAM MyPARAM;
            NucleusRH.Base.Configuracion.Params.DETAIL MyDETAIL;


            //Guardo el DDO              
            NomadLog.Debug("Guardo el DDO...");
            NomadEnvironment.GetCurrentTransaction().SaveRefresh(DDO);
            int oi_tarea = int.Parse(DDO.Id);

            ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            //Cargo la definicion de Alerta    
            NomadLog.Debug("Obtengo la Definicion de la Tarea...");
            NomadXML xmlTASK = NomadProxy.GetProxy().FileService().LoadFileXML("TASK", DDO.d_tipo_tarea);
			if (xmlTASK.isDocument) xmlTASK = xmlTASK.FirstChild();

            ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            //Verifico si Existe el Mail Type
            bool changeMailType = false;
            NucleusRH.Base.Configuracion.MailTypes.MAILTYPE mailType = null;
            NucleusRH.Base.Configuracion.MailTypes.MAILSUBTYPE mailSubType = null;

            NomadLog.Debug("Obtengo el ID del TIPO DE MAIL 'TAREA'...");
            string oiMailType = NomadEnvironment.QueryValue("CFG06_TYPESMAIL", "oi_type_mail", "c_type_mail", "TAREA", "", false);

            if (oiMailType == null || oiMailType == "")
            {
                NomadLog.Debug("Tipo de MAIL NO Definida, CREAR...");
                changeMailType = true;
                mailType = new NucleusRH.Base.Configuracion.MailTypes.MAILTYPE();
                mailType.c_type_mail = "TAREA";
                mailType.d_type_mail = "Tareas";
            }
            else
            {
                NomadLog.Debug("Tipo de MAIL Definida, CARGARLO...");
                mailType = NucleusRH.Base.Configuracion.MailTypes.MAILTYPE.Get(oiMailType);
            }


            ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            //Verifico si Existe el Mail Sub Type
            mailSubType = (NucleusRH.Base.Configuracion.MailTypes.MAILSUBTYPE)mailType.SUBTYPES.GetByAttribute("c_subtype_mail", DDO.d_tipo_tarea);
            if (mailSubType == null)
            {
                changeMailType = true;

                NomadLog.Debug("Subtipo de MAIL NO Definida, CREAR...");
                //No existe el Subtipo, crearlo
                mailSubType = NucleusRH.Base.Configuracion.MailTypes.MAILSUBTYPE.New();
                mailSubType.c_subtype_mail = DDO.d_tipo_tarea;
                mailSubType.d_subtype_mail = xmlTASK.GetAttr("label");
                mailType.SUBTYPES.Add(mailSubType);
            }


            ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            //Recorro los Grupos y genero el XML de Variables Permitidas
            NomadLog.Debug("Analizo las Variables VALIDAS 'TAREA'...");
            NomadXML xmlMD = new NomadXML("TASK");
            NomadXML xmlVarGroups;

            xmlVarGroups = xmlTASK.FindElement("PARAMS");
            if (xmlVarGroups != null && xmlVarGroups.FirstChild() != null)
            {
				Nomad.Base.Report.Variables.AddGroupDef(xmlMD, "PARAMS", "simple");
                for (NomadXML xmlVarField = xmlVarGroups.FirstChild(); xmlVarField != null; xmlVarField = xmlVarField.Next())
					Nomad.Base.Report.Variables.AddVariableDef(xmlMD, "PARAMS." + xmlVarField.GetAttr("name"), xmlVarField.GetAttr("type"), null);
            }

            xmlVarGroups = xmlTASK.FindElement("GROUPS");
            for (NomadXML xmlVarGroup = xmlVarGroups.FirstChild(); xmlVarGroup != null; xmlVarGroup = xmlVarGroup.Next())
            {
				Nomad.Base.Report.Variables.AddGroupDef(xmlMD, xmlVarGroup.GetAttr("name"), "coleccion");
                for (NomadXML xmlVarField = xmlVarGroup.FirstChild(); xmlVarField != null; xmlVarField = xmlVarField.Next())
					Nomad.Base.Report.Variables.AddVariableDef(xmlMD, xmlVarGroup.GetAttr("name") + "." + xmlVarField.GetAttr("name"), xmlVarField.GetAttr("type"), null);
            }

            if (mailSubType.o_variables != xmlMD.ToString())
            {
                changeMailType = true;
                mailSubType.o_variables = xmlMD.ToString();
            }
            if (changeMailType)
            {
                NomadLog.Debug("Guardo TIPO DE MAIL...");
                NomadEnvironment.GetCurrentTransaction().SaveRefresh(mailType);
            }


            ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            //Verifico si tiene definido el MAIL
            if (!DDO.oi_mailNull)
            {
                NomadLog.Debug("MAIL Definida, VERIFICAR...");

                //Cargo la Programacion
                MyMAIL = NucleusRH.Base.Configuracion.Mails.MAIL.Get(DDO.oi_mail);
                if (MyMAIL.d_mail != DDO.d_tarea || MyMAIL.oi_subtype_mail != mailType.Id)
                {
                    //Actualizo la Programacion
                    MyMAIL.d_mail = DDO.d_tarea;
                    MyMAIL.oi_subtype_mail = mailSubType.Id;

                    NomadLog.Debug("Guardo MAIL...");
                    NomadEnvironment.GetCurrentTransaction().SaveRefresh(MyMAIL);
                }

            }
            else
            {
                NomadLog.Debug("MAIL NO Definida, CREAR...");

                //Creo el Mail
                MyMAIL = new NucleusRH.Base.Configuracion.Mails.MAIL();
                MyMAIL.d_mail = DDO.d_tarea;
                MyMAIL.d_reply_mail = "nombre@dominio.com";
                MyMAIL.oi_subtype_mail = mailSubType.Id;

                //Busco el MAIL en la definicion	            
                NomadLog.Debug("Busco MAIL EN LA DEFINICION...");
                NomadXML xmlMAIL = xmlTASK.FindElement("MAIL");

                if (xmlMAIL != null)
                {
                    //Genero el MAIL
                    MyMAIL.d_asunto = xmlMAIL.GetAttr("suject");
                    string text = xmlMAIL.GetAttr("");
                    MyMAIL.o_text1 = (text.Length > 4000 ? text.Substring(1, 4000) : text); text = (text.Length > 4000 ? text.Substring(4000) : "");
                    MyMAIL.o_text2 = (text.Length > 4000 ? text.Substring(1, 4000) : text); text = (text.Length > 4000 ? text.Substring(4000) : "");
                    MyMAIL.o_text3 = (text.Length > 4000 ? text.Substring(1, 4000) : text); text = (text.Length > 4000 ? text.Substring(4000) : "");
                    MyMAIL.o_text4 = (text.Length > 4000 ? text.Substring(1, 4000) : text); text = (text.Length > 4000 ? text.Substring(4000) : "");
                    NomadEnvironment.GetCurrentTransaction().SaveRefresh(MyMAIL);

                    //Actualizo la Alerta
                    NomadLog.Debug("Guardo MAIL...");
                    DDO.oi_mail = MyMAIL.Id;
                    NomadEnvironment.GetCurrentTransaction().SaveRefresh(DDO);
                }
            }


            ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            //Verifico si tiene definido los PARAMETROS
            if (!DDO.oi_paramNull)
            {
                NomadLog.Debug("PARAM Definida, VERIFICAR...");

                changeMailType = false;

                //Creo el PARAM
                MyPARAM = NucleusRH.Base.Configuracion.Params.PARAM.Get(DDO.oi_param);
                if (MyPARAM.d_param != DDO.d_tarea)
                {
                    MyPARAM.d_param = DDO.d_tarea;
                    changeMailType = true;
                }

                //Recorro los Parametros	
                xmlVarGroups = xmlTASK.FindElement("PARAMS");
                if (xmlVarGroups != null && xmlVarGroups.FirstChild() != null)
                {
                    //MARCO los DETALLES PARA ELIMINAR
                    foreach (NucleusRH.Base.Configuracion.Params.DETAIL MyDEL in MyPARAM.DETAILS)
                        MyDEL.e_order = 0;


                    t = 0;
                    //Recorro los Parametros
                    for (NomadXML xmlVarField = xmlVarGroups.FirstChild(); xmlVarField != null; xmlVarField = xmlVarField.Next())
                    {
                        t++;

                        MyDETAIL = (NucleusRH.Base.Configuracion.Params.DETAIL)MyPARAM.DETAILS.GetByAttribute("c_detail", xmlVarField.GetAttr("name"));
                        if (MyDETAIL == null)
                        {
                            changeMailType = true;

                            MyDETAIL = new NucleusRH.Base.Configuracion.Params.DETAIL();
                            MyDETAIL.c_detail = xmlVarField.GetAttr("name");
                            MyDETAIL.d_detail = xmlVarField.GetAttr("label");
                            MyDETAIL.c_multiplicity = (xmlVarField.GetAttrBool("req") ? "1" : "0..1");
                            MyDETAIL.e_order = t;
                            MyDETAIL.c_parentNull = true;
                            MyDETAIL.c_type = xmlVarField.GetAttr("type");
                            MyDETAIL.d_valueNull = true;
                            MyDETAIL.d_value_textNull = true;
                            MyDETAIL.d_type_ext = xmlVarField.GetAttr("ext");

                            MyPARAM.DETAILS.Add(MyDETAIL);
                        }
                        else
                        {
                            if (MyDETAIL.c_detail != xmlVarField.GetAttr("name")) { MyDETAIL.c_detail = xmlVarField.GetAttr("name"); changeMailType = true; }
                            if (MyDETAIL.d_detail != xmlVarField.GetAttr("label")) { MyDETAIL.d_detail = xmlVarField.GetAttr("label"); changeMailType = true; }
                            if (MyDETAIL.c_multiplicity != (xmlVarField.GetAttrBool("req") ? "1" : "0..1")) { MyDETAIL.c_multiplicity = (xmlVarField.GetAttrBool("req") ? "1" : "0..1"); changeMailType = true; }
                            MyDETAIL.e_order = t;
                            MyDETAIL.c_parentNull = true;
                            if (MyDETAIL.c_type != xmlVarField.GetAttr("type")) { MyDETAIL.c_type = xmlVarField.GetAttr("type"); changeMailType = true; }
                            if (MyDETAIL.d_type_ext != xmlVarField.GetAttr("ext")) { MyDETAIL.d_type_ext = xmlVarField.GetAttr("ext"); changeMailType = true; }
                        }
                    }

                    //Elimino los parametros que no van mas...		
                    do
                    {
                        changeMailType = true;
                        MyDETAIL = (NucleusRH.Base.Configuracion.Params.DETAIL)MyPARAM.DETAILS.GetByAttribute("e_order", 0);
                        if (MyDETAIL != null)
                            MyPARAM.DETAILS.Remove(MyDETAIL);
                    } while (MyDETAIL != null);
                }


                if (changeMailType)
                {
                    NomadLog.Debug("Guardo PARAM...");
                    NomadEnvironment.GetCurrentTransaction().SaveRefresh(MyPARAM);
                }

            }
            else
            {
                NomadLog.Debug("PARAM NO Definida, CREAR...");

                //Creo el PARAM
                MyPARAM = new NucleusRH.Base.Configuracion.Params.PARAM();
                MyPARAM.d_param = DDO.d_tarea;

                //Recorro los Parametros	
                xmlVarGroups = xmlTASK.FindElement("PARAMS");
                if (xmlVarGroups != null && xmlVarGroups.FirstChild() != null)
                {
                    t = 0;
                    for (NomadXML xmlVarField = xmlVarGroups.FirstChild(); xmlVarField != null; xmlVarField = xmlVarField.Next())
                    {
                        t++;
                        MyDETAIL = new NucleusRH.Base.Configuracion.Params.DETAIL();
                        MyDETAIL.c_detail = xmlVarField.GetAttr("name");
                        MyDETAIL.d_detail = xmlVarField.GetAttr("label");
                        MyDETAIL.c_multiplicity = (xmlVarField.GetAttrBool("req") ? "1" : "0..1");
                        MyDETAIL.e_order = t;
                        MyDETAIL.c_parentNull = true;
                        MyDETAIL.c_type = xmlVarField.GetAttr("type");
                        MyDETAIL.d_valueNull = true;
                        MyDETAIL.d_value_textNull = true;
                        MyDETAIL.d_type_ext = xmlVarField.GetAttr("ext");

                        MyPARAM.DETAILS.Add(MyDETAIL);
                    }
                }

                //Actualizo
                NomadEnvironment.GetCurrentTransaction().SaveRefresh(MyPARAM);

                //Actualizo la Alerta       
                NomadLog.Debug("Actualizo DDO...");
                DDO.oi_param = MyPARAM.Id;
                NomadEnvironment.GetCurrentTransaction().SaveRefresh(DDO);
            }

            ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            //Verifico si tiene definida la Programacion.
            if (!DDO.oi_progNull)
            {
                NomadLog.Debug("Progranacion Definida, VERIFICAR...");

                //Cargo la Programacion
                MyPROG = NucleusRH.Base.Configuracion.Progs.PROG.Get(DDO.oi_prog);
                if (MyPROG.d_descr != DDO.d_tarea || MyPROG.d_class_name != "NucleusRH.Base.Configuracion.Tareas.TAREA" || MyPROG.d_class_method != "Run" || MyPROG.oi_class_id != oi_tarea)
                {
                    //Actualizo la Programacion
                    MyPROG.d_descr = DDO.d_tarea;
                    MyPROG.d_class_method = "Run";
                    MyPROG.oi_class_id = oi_tarea;

                    NomadLog.Debug("Guardo PROGRAMACION...");
                    NomadEnvironment.GetCurrentTransaction().SaveRefresh(MyPROG);
                }
            }
            else
            {
                NomadLog.Debug("Progranacion NO Definida, CREAR...");

                //Creo la Programacion
                MyPROG = new NucleusRH.Base.Configuracion.Progs.PROG();
                MyPROG.d_descr = DDO.d_tarea;
                MyPROG.d_prog = "No se especifico ninguna programación.";
                MyPROG.c_estado = "I";
                MyPROG.f_inicio = DateTime.Now.Date;
                MyPROG.d_class_name = "NucleusRH.Base.Configuracion.Tareas.TAREA";
                MyPROG.d_class_method = "Run";              
                MyPROG.oi_class_id = oi_tarea;
                MyPROG.oi_param = DDO.oi_param;

                NomadLog.Debug("Guardo PROGRAMACION...");
                NomadEnvironment.GetCurrentTransaction().SaveRefresh(MyPROG);


                //Actualizo la Alerta       
                NomadLog.Debug("Actualizo DDO...");
                DDO.oi_prog = MyPROG.Id;
                NomadEnvironment.GetCurrentTransaction().SaveRefresh(DDO);
            }
        }

        public static void Eliminar(string ID)
        {
            NucleusRH.Base.Configuracion.Progs.PROG MyPROG = null;
            NucleusRH.Base.Configuracion.Mails.MAIL MyMAIL = null;
            NucleusRH.Base.Configuracion.Params.PARAM MyPARAM = null;
            NucleusRH.Base.Configuracion.Tareas.TAREA MyTAREA;


            //Cargo el DDO Principal
			MyTAREA = NucleusRH.Base.Configuracion.Tareas.TAREA.Get(ID, true);

            //Cargo la Programacion
			if (!MyTAREA.oi_progNull)
				MyPROG = NucleusRH.Base.Configuracion.Progs.PROG.Get(MyTAREA.oi_prog, true);

            //Cargo la Mail
			if (!MyTAREA.oi_mailNull)
				MyMAIL = NucleusRH.Base.Configuracion.Mails.MAIL.Get(MyTAREA.oi_mail, true);

			//Cargo MyTAREA Parametros
			if (!MyTAREA.oi_paramNull)
				MyPARAM = NucleusRH.Base.Configuracion.Params.PARAM.Get(MyTAREA.oi_param, true);

            /////////////////////////////////////////////////////////////////////////////////////	
            //Realizo la Transaccion
            NomadEnvironment.GetCurrentTransaction().Begin();
			NomadEnvironment.GetCurrentTransaction().Delete(MyTAREA);
            if (MyPROG != null)
                NomadEnvironment.GetCurrentTransaction().Delete(MyPROG);
            if (MyMAIL != null)
                NomadEnvironment.GetCurrentTransaction().Delete(MyMAIL);
            if (MyPARAM != null)
                NomadEnvironment.GetCurrentTransaction().Delete(MyPARAM);
            NomadEnvironment.GetCurrentTransaction().Commit();
        }

    }
}
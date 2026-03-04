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

namespace NucleusRH.Base.Configuracion.Alertas
{

    //////////////////////////////////////////////////////////////////////////////////
    //Clase Alertas
    public partial class ALERTA : Nomad.NSystem.Base.NomadObject
    {

        public static bool CheckNodes(ALERTA MyALERT, NucleusRH.Base.Configuracion.Mails.MAIL MyMAIL, NomadXML MyNODE, int deep, ArrayList DeepNodes)
        {
            bool retval = false;
            NomadXML cur;

            //Recorro los NODOS y los ANALISOS
            deep++;
            for (cur = MyNODE.FirstChild(); cur != null; cur = cur.Next())
            {
                //Actualizo el ARRAY
                while (DeepNodes.Count > deep) DeepNodes.RemoveAt(deep);
                DeepNodes.Add(cur);

                //Accion sobre el NODO
                if (cur.Name == MyALERT.c_mail_group)
                {
                    if (SendMail(MyALERT, MyMAIL, cur, deep, DeepNodes)) retval = true;
                }
                else
                {
                    if (CheckNodes(MyALERT, MyMAIL, cur, deep, DeepNodes)) retval = true;
                }
            }

            return retval;
        }

        public static bool SendMail(ALERTA MyALERT, NucleusRH.Base.Configuracion.Mails.MAIL MyMAIL, NomadXML MyNODE, int deep, ArrayList DeepNodes)
        {
            bool retval = false;

            //Recorro los Destinatarios
            NomadLog.Debug("--RECORRO LOS DESTINATARIOS--");
            Hashtable hDest = new Hashtable();
            foreach (DESTINATARIO MyDEST in MyALERT.DESTINATARIOS)
            {
                switch (MyDEST.c_tipo)
                {
                    case "OWNER":
                        foreach (string strMails in ((NomadXML)DeepNodes[deep]).GetAttr("MAIL").Split(';'))
                            hDest[strMails.ToUpper()] = strMails;
                        break;

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

            string strDest = "";
            foreach (string strMail in hDest.Values)
                if (strMail != "")
                    strDest += (strDest == "" ? "" : ";") + strMail;

            //Enviar MAIL
            if (strDest != "")
            {
                Nomad.Base.Mail.Mails.MAIL newMAIL;
                NucleusRH.Base.Configuracion.MailTypes.MAILSUBTYPE MyMailType;

                NomadLog.Debug("--CREO EL MAIL-- to:" + strDest);

                //Creo el MAIL
                newMAIL = Nomad.Base.Mail.Mails.MAIL.Crear();
                newMAIL.ASUNTO = MyMAIL.d_asunto;
                newMAIL.CUERPO = MyMAIL.o_text1 + MyMAIL.o_text2 + MyMAIL.o_text3 + MyMAIL.o_text4;
                newMAIL.TIPO = "W";
                newMAIL.REMITENTE_DIR = MyMAIL.d_reply_mail;
                foreach (string strMails in strDest.Split(';'))
                    Nomad.Base.Mail.Mails.MAIL.AgregarDestinatario(newMAIL, strMails);

                //Variables
                MyMailType = NucleusRH.Base.Configuracion.MailTypes.MAILSUBTYPE.Get(MyMAIL.oi_subtype_mail);

                NomadXML xmlAL = new NomadXML(MyMailType.o_variables);
                if (xmlAL.isDocument) xmlAL = xmlAL.FirstChild();

                NomadXML xmlMD = xmlAL.FindElement("METADATA");
                NomadXML xmlDATA, xmlMDGR;

                //Agrego los elementos CHILDS...
                for (xmlMDGR = xmlMD.FirstChild(); xmlMDGR != null; xmlMDGR = xmlMDGR.Next())
                {
                    //Busco si es UNICO
                    xmlDATA = null;
                    for (int t = 0; t <= deep; t++)
                        if (((NomadXML)DeepNodes[t]).Name == xmlMDGR.GetAttr("name"))
                            xmlDATA = (NomadXML)DeepNodes[t];

                    //Valores
                    if (xmlDATA != null)
                        SetValues(xmlAL, xmlDATA, xmlMDGR);
                    else
                        FindChilds(xmlAL, (NomadXML)DeepNodes[deep], xmlMDGR);
                }
                NomadLog.Debug("--VARIABLES--" + xmlAL.ToString());

                //Enviar el MAIL
                newMAIL.Enviar(xmlAL);
                NomadLog.Debug("--ENVIAR--");
                retval = true;
            }

            return retval;
        }

        public static void FindChilds(NomadXML xmlAL, NomadXML MyNODE, NomadXML xmlMDGR)
        {
            for (NomadXML MyCUR = MyNODE.FirstChild(); MyCUR != null; MyCUR = MyCUR.Next())
            {
                if (MyCUR.Name == xmlMDGR.GetAttr("name"))
                    SetValues(xmlAL, MyCUR, xmlMDGR);

                //Buscar en los Childs...
                FindChilds(xmlAL, MyCUR, xmlMDGR);
            }

        }

        public static void SetValues(NomadXML xmlAL, NomadXML MyNODE, NomadXML xmlMDGR)
        {
            //Agrego el Grupo
            Nomad.Base.Report.Variables.AddItem(xmlAL, xmlMDGR.GetAttr("name"));

            //Nuevo Indice
			int idx = Nomad.Base.Report.Variables.CountItem(xmlAL, xmlMDGR.GetAttr("name"));

            //Recorro las Variables
            for (NomadXML xmlMDVAR = xmlMDGR.FirstChild(); xmlMDVAR != null; xmlMDVAR = xmlMDVAR.Next())
				Nomad.Base.Report.Variables.SetValue(xmlAL, idx - 1, xmlMDGR.GetAttr("name") + "." + xmlMDVAR.GetAttr("name"), xmlMDVAR.GetAttr("type"), MyNODE.GetAttr(xmlMDVAR.GetAttr("name")), null);
        }

		public static void Run(int oi_alerta, NomadXML xmlEVENT)
        {
            //Obtengo los DDOs
            ALERTA MyALERT = ALERTA.Get(oi_alerta, false);
            NucleusRH.Base.Configuracion.Params.PARAM MyPARAMS = NucleusRH.Base.Configuracion.Params.PARAM.Get(MyALERT.oi_param, false);
            NucleusRH.Base.Configuracion.Mails.MAIL MyMAIL = NucleusRH.Base.Configuracion.Mails.MAIL.Get(MyALERT.oi_mail, false);

            /////////////////////////////////////////////////////////////////////////////////////////////////////////
            // Definicion
            NomadLog.Debug("Obtengo la definicion de la ALERTA.");
            NomadXML DefAlerta = NomadProxy.GetProxy().FileService().LoadFileXML("ALERT", MyALERT.d_tipo_alerta);

            // SQL QUERY
            NomadLog.Debug("Obtengo el QUERY.");
            string MyQUERY = DefAlerta.FindElement("QRY").ToString(false);

            // PARAMETROS
            NomadLog.Debug("Genero los PARAMETROS.");
            NomadXML xmlPARAM = new NomadXML("PARAMS");
            foreach (NucleusRH.Base.Configuracion.Params.DETAIL MyPARAM_DET in MyPARAMS.DETAILS)
                xmlPARAM.SetAttr(MyPARAM_DET.c_detail, MyPARAM_DET.d_value);

			// INFORMACION DEL EVENTO
			if (xmlEVENT!=null)
			{
				string[] aIDS=xmlEVENT.GetAttr("IDS").Split(',');
				string sIDS="";

				for (int t=0; t<aIDS.Length; t++)
				{
					sIDS+=(sIDS.Length>0?",":"")+aIDS[t];
					if (sIDS.Length<100 || t==aIDS.Length-1)
					{
						xmlEVENT.SetAttr("IDS", sIDS);
						xmlPARAM.AddXML(xmlEVENT);

						sIDS="";
					}
				}

				xmlPARAM.SetAttr("eve_keyname", xmlEVENT.GetAttr("eve_keyname"));
				xmlPARAM.SetAttr("eve_name", xmlEVENT.GetAttr("eve_name"));
			} 
			if (xmlPARAM.FirstChild()==null)
				xmlPARAM.AddTailElement("EVE");

            // Realizo la CONSULTA
            NomadLog.Debug("Realizo la CONSULTA.");
            NomadXML RESULT = NomadProxy.GetProxy().SQLService().GetXML(MyQUERY, xmlPARAM.ToString());

            //Recorro los NODOS segun el GRUPO
            ArrayList DeepNodes = new ArrayList();
            DeepNodes.Add(xmlPARAM);
            if (CheckNodes(MyALERT, MyMAIL, RESULT, 0, DeepNodes))
            {
                MyALERT.f_ult_aviso = DateTime.Now;
                NomadEnvironment.GetCurrentTransaction().Save(MyALERT);
            }
        }
        public static void Guardar(NucleusRH.Base.Configuracion.Alertas.ALERTA DDO)
        {
            int t;
            NucleusRH.Base.Configuracion.Progs.PROG MyPROG;
            NucleusRH.Base.Configuracion.Mails.MAIL MyMAIL;
            NucleusRH.Base.Configuracion.Params.PARAM MyPARAM;
            NucleusRH.Base.Configuracion.Params.DETAIL MyDETAIL;

            //Guardo el DDO
            NomadLog.Debug("Guardo el DDO...");
            NomadEnvironment.GetCurrentTransaction().SaveRefresh(DDO);
            int oi_alerta = int.Parse(DDO.Id);

            ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            //Cargo la definicion de Alerta
            NomadLog.Debug("Obtengo la Definicion de la Alerta...");
            NomadXML xmlALERT = NomadProxy.GetProxy().FileService().LoadFileXML("ALERT", DDO.d_tipo_alerta);
            if (xmlALERT.isDocument) xmlALERT = xmlALERT.FirstChild();


            ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            //Verifico si Existe el Mail Type
            bool changeMailType = false;
            NucleusRH.Base.Configuracion.MailTypes.MAILTYPE mailType = null;
            NucleusRH.Base.Configuracion.MailTypes.MAILSUBTYPE mailSubType = null;

            NomadLog.Debug("Obtengo el ID del TIPO DE MAIL 'ALERTA'...");
            string oiMailType = NomadEnvironment.QueryValue("CFG06_TYPESMAIL", "oi_type_mail", "c_type_mail", "ALERTA", "", false);

            if (oiMailType == null || oiMailType == "")
            {
                NomadLog.Debug("Tipo de MAIL NO Definida, CREAR...");
                changeMailType = true;
                mailType = new NucleusRH.Base.Configuracion.MailTypes.MAILTYPE();
                mailType.c_type_mail = "ALERTA";
                mailType.d_type_mail = "Alertas";
            }
            else
            {
                NomadLog.Debug("Tipo de MAIL Definida, CARGARLO...");
                mailType = NucleusRH.Base.Configuracion.MailTypes.MAILTYPE.Get(oiMailType);
            }

            ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            //Verifico si Existe el Mail Sub Type
            mailSubType = (NucleusRH.Base.Configuracion.MailTypes.MAILSUBTYPE)mailType.SUBTYPES.GetByAttribute("c_subtype_mail", DDO.d_tipo_alerta);
            if (mailSubType == null)
            {
                changeMailType = true;

                NomadLog.Debug("Subtipo de MAIL NO Definida, CREAR...");
                //No existe el Subtipo, crearlo
                mailSubType = new NucleusRH.Base.Configuracion.MailTypes.MAILSUBTYPE();
                mailSubType.c_subtype_mail = DDO.d_tipo_alerta;
                mailSubType.d_subtype_mail = xmlALERT.GetAttr("label");
                mailType.SUBTYPES.Add(mailSubType);
            }

            ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            //Recorro los Grupos y genero el XML de Variables Permitidas
            NomadLog.Debug("Analizo las Variables VALIDAS 'ALERTA'...");
            NomadXML xmlMD = new NomadXML("ALERT");
            NomadXML xmlVarGroups;

            xmlVarGroups = xmlALERT.FindElement("PARAMS");
            if (xmlVarGroups != null && xmlVarGroups.FirstChild() != null)
            {
				Nomad.Base.Report.Variables.AddGroupDef(xmlMD, "PARAMS", "coleccion");
                for (NomadXML xmlVarField = xmlVarGroups.FirstChild(); xmlVarField != null; xmlVarField = xmlVarField.Next())
					Nomad.Base.Report.Variables.AddVariableDef(xmlMD, "PARAMS." + xmlVarField.GetAttr("name"), xmlVarField.GetAttr("type"), null);
            }

            xmlVarGroups = xmlALERT.FindElement("GROUPS");
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
				
				//Recargo el Subtipo de MAIL
				mailSubType = (NucleusRH.Base.Configuracion.MailTypes.MAILSUBTYPE)mailType.SUBTYPES.GetByAttribute("c_subtype_mail", DDO.d_tipo_alerta);
            }

            ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            //Verifico si tiene definido el MAIL
            if (!DDO.oi_mailNull)
            {
                NomadLog.Debug("MAIL Definida, VERIFICAR...");

                //Cargo la Programacion
                MyMAIL = NucleusRH.Base.Configuracion.Mails.MAIL.Get(DDO.oi_mail);
                if (MyMAIL.d_mail != DDO.d_alerta || MyMAIL.oi_subtype_mail != mailType.Id)
                {
                    //Actualizo la Programacion
                    MyMAIL.d_mail = DDO.d_alerta;
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
                MyMAIL.d_mail = DDO.d_alerta;
                MyMAIL.d_reply_mail = "nombre@dominio.com";
                MyMAIL.oi_subtype_mail = mailSubType.Id;

                //Busco el MAIL en la definicion
                NomadLog.Debug("Busco MAIL EN LA DEFINICION...");
                NomadXML xmlMAIL = xmlALERT.FindElement("MAILS");
                if (xmlMAIL != null)
                    xmlMAIL = xmlMAIL.FindElement2("MAIL", "group", DDO.c_mail_group);

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
                if (MyPARAM.d_param != DDO.d_alerta)
                {
                    MyPARAM.d_param = DDO.d_alerta;
                    changeMailType = true;
                }

                //Recorro los Parametros
                xmlVarGroups = xmlALERT.FindElement("PARAMS");
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
                            if (xmlVarField.GetAttr("event-key") != "") MyDETAIL.d_eve_keyname = xmlVarField.GetAttr("event-key");

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
                MyPARAM.d_param = DDO.d_alerta;

                //Recorro los Parametros
                xmlVarGroups = xmlALERT.FindElement("PARAMS");
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
                        if (xmlVarField.GetAttr("event-key") != "") MyDETAIL.d_eve_keyname = xmlVarField.GetAttr("event-key");

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
                if (MyPROG.d_descr != DDO.d_alerta || MyPROG.d_class_name != "NucleusRH.Base.Configuracion.Alertas.ALERTA" || MyPROG.d_class_method != "Run" || MyPROG.oi_class_id != oi_alerta)
                {
                    //Actualizo la Programacion
                    MyPROG.d_descr = DDO.d_alerta;
                    MyPROG.d_class_name = "NucleusRH.Base.Configuracion.Alertas.ALERTA";
                    MyPROG.d_class_method = "Run";
                    MyPROG.oi_class_id = oi_alerta;

                    NomadLog.Debug("Guardo PROGRAMACION...");
                    NomadEnvironment.GetCurrentTransaction().SaveRefresh(MyPROG);
                }
            }
            else
            {
                NomadLog.Debug("Progranacion NO Definida, CREAR...");

                //Creo la Programacion
                MyPROG = new NucleusRH.Base.Configuracion.Progs.PROG();
                MyPROG.d_descr = DDO.d_alerta;
                MyPROG.d_prog = "No se especifico ninguna programación.";
                MyPROG.c_estado = "I";
                MyPROG.f_inicio = DateTime.Now.Date;
                MyPROG.d_class_name = "NucleusRH.Base.Configuracion.Alertas.ALERTA";
                MyPROG.d_class_method = "Run";
                MyPROG.oi_class_id = oi_alerta;
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
            NucleusRH.Base.Configuracion.Alertas.ALERTA MyALERT;

            //Cargo el DDO Principal
            MyALERT = NucleusRH.Base.Configuracion.Alertas.ALERTA.Get(ID, true);

            //Cargo la Programacion
            if (!MyALERT.oi_progNull)
                MyPROG = NucleusRH.Base.Configuracion.Progs.PROG.Get(MyALERT.oi_prog, true);

            //Cargo la Mail
            if (!MyALERT.oi_mailNull)
                MyMAIL = NucleusRH.Base.Configuracion.Mails.MAIL.Get(MyALERT.oi_mail, true);

            //Cargo los Parametros
            if (!MyALERT.oi_paramNull)
                MyPARAM = NucleusRH.Base.Configuracion.Params.PARAM.Get(MyALERT.oi_param, true);

            /////////////////////////////////////////////////////////////////////////////////////
            //Realizo la Transaccion
            NomadEnvironment.GetCurrentTransaction().Begin();
            NomadEnvironment.GetCurrentTransaction().Delete(MyALERT);
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

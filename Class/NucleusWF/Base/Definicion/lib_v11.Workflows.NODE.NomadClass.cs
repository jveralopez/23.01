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
    public partial class NODE : Nomad.NSystem.Base.NomadObject
    {
        public static void SAVE_DDO(NucleusWF.Base.Definicion.Workflows.NODE DOC_DDO)
        {
			NomadEnvironment.GetCurrentTransaction().SaveRefresh(DOC_DDO);
        }

        public static void ADD_METHODS(NucleusWF.Base.Definicion.Workflows.PROCESS DOC_DDO, NucleusWF.Base.Definicion.Workflows.NODE DOC_NODE, Nomad.NSystem.Proxy.NomadXML PARAMS_IN, Nomad.NSystem.Proxy.NomadXML PARAMS_OUT)
        {
            Nomad.NSystem.Proxy.NomadXML row;
            for (row = PARAMS_IN.FirstChild().FirstChild(); row != null; row = row.Next())
            {
                NucleusWF.Base.Definicion.Workflows.PARAM PARAM = new NucleusWF.Base.Definicion.Workflows.PARAM();
                PARAM.c_param = row.GetAttr("c_param");
                PARAM.c_type = row.GetAttr("c_type");
                PARAM.o_formula = row.GetAttr("o_formula");
                DOC_NODE.NODE_PARAMS.Add(PARAM);
            }

            for (row = PARAMS_OUT.FirstChild().FirstChild(); row != null; row = row.Next())
            {
                NucleusWF.Base.Definicion.Workflows.PARAM PARAM = new NucleusWF.Base.Definicion.Workflows.PARAM();
                PARAM.c_param = row.GetAttr("c_param");
                PARAM.c_type = row.GetAttr("c_type");
                PARAM.c_var = row.GetAttr("c_var");
                DOC_NODE.NODE_PARAMS.Add(PARAM);
            }

            DOC_DDO.NODES.Add(DOC_NODE);
            PROCESS.SAVE_DDO(DOC_DDO);
        }

        public static void SAVE_METHODS(NucleusWF.Base.Definicion.Workflows.NODE DOC_NODE, Nomad.NSystem.Proxy.NomadXML PARAMS_IN, Nomad.NSystem.Proxy.NomadXML PARAMS_OUT)
        {
            NomadLog.Debug(DOC_NODE.SerializeAll());
            NomadLog.Debug(PARAMS_IN.ToString());
            NomadLog.Debug(PARAMS_OUT.ToString());
            Nomad.NSystem.Proxy.NomadXML row;
            foreach (NucleusWF.Base.Definicion.Workflows.PARAM node_param in DOC_NODE.NODE_PARAMS)
            {
                for (row = PARAMS_IN.FirstChild().FirstChild(); row != null; row = row.Next())
                {
                    if (node_param.c_param == row.GetAttr("c_param"))
                        node_param.o_formula = row.GetAttr("o_formula");
                }
                for (row = PARAMS_OUT.FirstChild().FirstChild(); row != null; row = row.Next())
                {
                    if (node_param.c_param == row.GetAttr("c_param"))
                        node_param.c_var = row.GetAttr("c_var");
                }
            }
            NODE.SAVE_DDO(DOC_NODE);
        }

        public static void ADD_FORM(NucleusWF.Base.Definicion.Workflows.PROCESS DOC_DDO, NucleusWF.Base.Definicion.Workflows.NODE DOC_NODE, Nomad.NSystem.Proxy.NomadXML ROLES)
        {
            Nomad.NSystem.Proxy.NomadXML row;
            for (row = ROLES.FirstChild().FirstChild(); row != null; row = row.Next())
            {
                NucleusWF.Base.Definicion.Workflows.ROLE_NODE ROL = new NucleusWF.Base.Definicion.Workflows.ROLE_NODE();
                string[] split = row.GetAttrString("c_role").Split('[');
                string[] codigo = split[1].Split(']');
                ROL.c_rol = codigo[0].ToString();
                ROL.c_notificar = row.GetAttr("c_notificar");
                ROL.c_resolver = row.GetAttr("c_resolver");
                ROL.d_rol_delega = row.GetAttr("d_rol_delega");
                ROL.c_puede_deleg = row.GetAttr("c_puede_deleg");
                DOC_NODE.ROLES_NODE.Add(ROL);
            }

            DOC_DDO.NODES.Add(DOC_NODE);
			PROCESS.SAVE_DDO(DOC_DDO);
        }

        public static void SAVE_FORM(NucleusWF.Base.Definicion.Workflows.NODE DOC_NODE, Nomad.NSystem.Proxy.NomadXML ROLES)
        {
            Nomad.NSystem.Proxy.NomadXML row;
            int existe;
            for (row = ROLES.FirstChild().FirstChild(); row != null; row = row.Next())
            {
                existe = 0;
                foreach (NucleusWF.Base.Definicion.Workflows.ROLE_NODE rol in DOC_NODE.ROLES_NODE)
                {
                    string[] split = row.GetAttrString("c_role").Split('[');
                    NomadLog.Debug("split-0: " + split[0].ToString());
                    NomadLog.Debug("split-1: " + split[1].ToString());
                    string[] codigo = split[1].Split(']');
                    NomadLog.Debug("codigo-0: " + codigo[0].ToString());
                    NomadLog.Debug("codigo-1: " + codigo[1].ToString());
                    if (rol.c_rol == codigo[0].ToString())
                    {
                        existe = 1;
                        rol.c_notificar = row.GetAttr("c_notificar");
                        rol.c_resolver = row.GetAttr("c_resolver");
                        rol.d_rol_delega = row.GetAttr("d_rol_delega");
                        rol.c_puede_deleg = row.GetAttr("c_puede_deleg");
                    }
                }
                if (existe == 0)
                {
                    NucleusWF.Base.Definicion.Workflows.ROLE_NODE ROL = new NucleusWF.Base.Definicion.Workflows.ROLE_NODE();
                    string[] split = row.GetAttrString("c_role").Split('[');
                    string[] codigo = split[1].Split(']');
                    ROL.c_rol = codigo[0].ToString();
                    ROL.c_notificar = row.GetAttr("c_notificar");
                    ROL.c_resolver = row.GetAttr("c_resolver");
                    ROL.d_rol_delega = row.GetAttr("d_rol_delega");
                    ROL.c_puede_deleg = row.GetAttr("c_puede_deleg");
                    DOC_NODE.ROLES_NODE.Add(ROL);
                }
            }

            NODE.SAVE_DDO(DOC_NODE);
        }


		public static void MAIL_REFRESH_DESTINATARIOS(NucleusWF.Base.Definicion.Workflows.NODE DOC_NODE, Nomad.NSystem.Proxy.NomadXML DESTINATARIOS)
		{
			//Me paro en el primer elemento
			if (DESTINATARIOS.isDocument) DESTINATARIOS=DESTINATARIOS.FirstChild();

			//Limpio los Parametros
			DOC_NODE.NODE_PARAMS.Clear();

			//Recorro los destinatarios
			int idx;
			NomadXML row;
			for (row = DESTINATARIOS.FirstChild(), idx=1; row != null; row = row.Next(), idx++)
			{
				NucleusWF.Base.Definicion.Workflows.PARAM PARAM = new NucleusWF.Base.Definicion.Workflows.PARAM();

				PARAM.c_param = (idx+100).ToString().Substring(1);
				PARAM.c_type = row.GetAttr("d_param2");
				PARAM.d_valor = row.GetAttr("d_param1");
				PARAM.d_param = row.GetAttr("destinatario");
				PARAM.o_formula = row.GetAttr("o_formula");

				DOC_NODE.NODE_PARAMS.Add(PARAM);
			}
		}

		public static void ADD_MAIL(NucleusWF.Base.Definicion.Workflows.PROCESS DOC_DDO, NucleusWF.Base.Definicion.Workflows.NODE DOC_NODE, Nomad.NSystem.Proxy.NomadXML DESTINATARIOS)
		{
			MAIL_REFRESH_DESTINATARIOS(DOC_NODE, DESTINATARIOS);

			DOC_DDO.NODES.Add(DOC_NODE);
			PROCESS.SAVE_DDO(DOC_DDO);
		}
		
		public static void SAVE_MAIL(NucleusWF.Base.Definicion.Workflows.NODE DOC_NODE, Nomad.NSystem.Proxy.NomadXML DESTINATARIOS)
        {
			MAIL_REFRESH_DESTINATARIOS(DOC_NODE, DESTINATARIOS);
            SAVE_DDO(DOC_NODE);
        }
    }
}

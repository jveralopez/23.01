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

namespace NucleusRH.Base.Liquidacion.SubgrupoLSD
{

    //////////////////////////////////////////////////////////////////////////////////
    //Clase Reportes
    public partial class SUBGRUPO
    {
        public static void GuardarSubgrupoLSD(NomadXML xmlParam)
        {
            xmlParam = xmlParam.FirstChild();
            SUBGRUPO subgrupo = null;
            string oi_subgrupo = NomadEnvironment.QueryValue("LIQ32_SUBGRUPO_LSD", "oi_subgrupo", "d_subgrupo", xmlParam.FindElement("d_subgrupo").GetAttr("value"), "", false);
            if (oi_subgrupo != "")
            {
                subgrupo = SUBGRUPO.Get(oi_subgrupo);
            }
            else
            {
                subgrupo = new SUBGRUPO();
            }

			subgrupo.e_lsd_inicial = int.Parse(xmlParam.GetAttr("e_lsd_inicial"));
			subgrupo.e_lsd_final = int.Parse(xmlParam.GetAttr("e_lsd_final"));
			subgrupo.d_subgrupo = xmlParam.GetAttr("d_subgrupo");

            NomadEnvironment.GetCurrentTransaction().Save(subgrupo);
        }
		
		
		public static void ValidarCodigo(NucleusRH.Base.Liquidacion.SubgrupoLSD.SUBGRUPO DDOSUBGRUPO)
		{	
			//GENERO UN XML PARA CONTROLAR SOLAPAMIENTO DE CODIGOS
			string paramXML = "<PARAM e_lsd_inicial='"+DDOSUBGRUPO.e_lsd_inicial+"' e_lsd_final='"+DDOSUBGRUPO.e_lsd_final+"' />";
				NomadXML MyXML=NomadEnvironment.QueryNomadXML(SUBGRUPO.Resources.QRY_ValidarCodigo, paramXML, false);
			string strSolapa="";
			
			// CONTROLO SI EL REGISTRO DE SUBGRUPO ES NUEVO O SE ESTÁ MODIFICANDO UN EXISTENTE
			string codID = NomadEnvironment.QueryValue("LIQ32_SUBGRUPO_LSD", "oi_subgrupo", "oi_subgrupo", DDOSUBGRUPO.id.ToString(), "", false);
			if (codID=="")
			{
				// CONTROLO SI YA SE UTILIZA EL CODIGO DE SUBGRUPO
				string d_subgrupo = NomadEnvironment.QueryValue("LIQ32_SUBGRUPO_LSD", "d_subgrupo", "d_subgrupo", DDOSUBGRUPO.d_subgrupo, "", false);
				if(d_subgrupo != "")
					throw NomadException.NewMessage("SubgrupoLSD.VALIDACIONABM.ERR-SUBGRUPOEXIST");
				
				// CONTROLO SI EXISTE SOLAPAMIENTO DE CODIGOS
				for (NomadXML myCUR=MyXML.FirstChild().FirstChild(); myCUR!=null; myCUR=myCUR.Next())
					strSolapa=(strSolapa==""?myCUR.GetAttr("d_subgrupo"):strSolapa+", "+myCUR.GetAttr("d_subgrupo"));

				if(strSolapa != "")
					throw NomadException.NewMessage("SubgrupoLSD.VALIDACIONABM.ERR-CODIGOSOLAPADO", strSolapa);
			}
			else
			{	
				// CONTROLO SI AL MODIFICAR EL CODIGO DE SUBGRUPO YA SE UTILIZA EN OTRO
				string d_subgrupo = NomadEnvironment.QueryValue("LIQ32_SUBGRUPO_LSD", "d_subgrupo", "d_subgrupo", DDOSUBGRUPO.d_subgrupo, "", false);
				if(d_subgrupo != "" && d_subgrupo!=DDOSUBGRUPO.d_subgrupo)
					throw NomadException.NewMessage("SubgrupoLSD.VALIDACIONABM.ERR-SUBGRUPOEXIST");
				
				// CONTROLO SI EXISTE SOLAPAMIENTO DE CODIGOS
				for (NomadXML myCUR=MyXML.FirstChild().FirstChild(); myCUR!=null; myCUR=myCUR.Next())
				{
					if(myCUR.GetAttr("oi_subgrupo") != codID)
						strSolapa=(strSolapa==""?myCUR.GetAttr("d_subgrupo"):strSolapa+", "+myCUR.GetAttr("d_subgrupo"));
				}
				
				if (strSolapa != "")
					throw NomadException.NewMessage("SubgrupoLSD.VALIDACIONABM.ERR-CODIGOSOLAPADO", strSolapa);
			}	
		}
    }
}

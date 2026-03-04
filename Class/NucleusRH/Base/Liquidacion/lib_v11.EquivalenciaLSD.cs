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

namespace NucleusRH.Base.Liquidacion.EquivalenciaLSD
{

    //////////////////////////////////////////////////////////////////////////////////
    //Clase Reportes
    public partial class EQUIVALENCIA
    {
        public static void GuardarEquivalencia(NomadXML xmlParam)
        {
            xmlParam = xmlParam.FirstChild();
            EQUIVALENCIA equivalencia = null;
			int codLSD;
            string oi_equivalencia = NomadEnvironment.QueryValue("LIQ14_EQUIVALENCIA", "oi_equivalencia", "oi_concepto", xmlParam.FindElement("oi_concepto").GetAttr("value"), "", false);
            if (oi_equivalencia != "")                          
            {
                equivalencia = EQUIVALENCIA.Get(oi_equivalencia);
            }
            else
            {
                equivalencia = new EQUIVALENCIA();
            }

            equivalencia.c_equivalencia = xmlParam.GetAttr("c_equivalencia");
            equivalencia.oi_concepto = xmlParam.FindElement("oi_concepto").GetAttr("value");
            equivalencia.l_informa = xmlParam.GetAttrBool("l_informa");
			if(int.TryParse(xmlParam.GetAttr("e_lsd"), out codLSD))
				equivalencia.e_lsd = codLSD; 
			else 
				equivalencia.e_lsdNull = true;

            NomadEnvironment.GetCurrentTransaction().Save(equivalencia);
        }
    }
}

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

namespace NucleusRH.Base.Legales.Rendiciones
{

    //////////////////////////////////////////////////////////////////////////////////
    //Clase Documento
    public partial class DOCUMENTO : Nomad.NSystem.Base.NomadObject
    {
        public static bool ValidarCC(string id)
        {
            NomadXML param = new NomadXML("PARAM");
            param.SetAttr("oi_centro_costo", id);
            NomadXML resultado = NomadEnvironment.QueryNomadXML(NucleusRH.Base.Legales.Rendiciones.DOCUMENTO.Resources.QRY_VALIDAR, param.ToString());
            NomadEnvironment.GetTrace().Info("RESULADO VALIDACIÓN: " + resultado.ToString());

            if (resultado.FirstChild().ChildLength == 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}

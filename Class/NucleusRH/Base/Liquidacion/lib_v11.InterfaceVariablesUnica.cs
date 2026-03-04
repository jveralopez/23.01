using System;
using System.Xml;
using System.IO;
using System.Collections;

using Nomad.NSystem.Document;
using Nomad.NSystem.Proxy;
using Nomad.NSystem.Base;
using Nomad.NSystem.Functions;

namespace NucleusRH.Base.Liquidacion.InterfaceVariablesFijasUnica
{

    //////////////////////////////////////////////////////////////////////////////////
    //Clase efinicion de interfaz de entrada de tipo uno
    public partial class ENTRADA
    {
        public static void ImportarVariables(string poi_empresa, string pC_Variable)
        {
            NucleusRH.Base.Liquidacion_VarFijaUnica.clsImportarVariables objImportador = new NucleusRH.Base.Liquidacion_VarFijaUnica.clsImportarVariables();
            objImportador.ImportarVariablesPersona(poi_empresa, pC_Variable);
        }
    }
}

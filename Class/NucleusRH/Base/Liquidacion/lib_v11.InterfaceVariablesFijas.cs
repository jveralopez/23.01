using System;
using System.Xml;
using System.IO;
using System.Collections;

using Nomad.NSystem.Document;
using Nomad.NSystem.Proxy;
using Nomad.NSystem.Base;
using Nomad.NSystem.Functions;

namespace NucleusRH.Base.Liquidacion.InterfaceVariablesFijas
{

    //////////////////////////////////////////////////////////////////////////////////
    //Clase efinicion de interfaz de entrada de tipo uno
    public partial class ENTRADA
    {
        public static void ImportarVariables(string poi_empresa)
        {
            NucleusRH.Base.Liquidacion_VarFija.clsImportarVariables objImportador = new NucleusRH.Base.Liquidacion_VarFija.clsImportarVariables();
            objImportador.ImportarVariablesPersona(poi_empresa);
        }
    }
}

using System;
using System.Xml;
using System.IO;
using System.Collections;

using Nomad.NSystem.Document;
using Nomad.NSystem.Proxy;
using Nomad.NSystem.Base;
using Nomad.NSystem.Functions;

namespace NucleusRH.Base.Liquidacion.InterfaceVariablesAcum
{

    //////////////////////////////////////////////////////////////////////////////////
    //Clase efinicion de interfaz de entrada de tipo uno
    public partial class ENTRADA
    {
        public static void ImportarVariables(string poi_empresa)
        {
            NucleusRH.Base.Liquidacion_VarAcum.clsImportarVariables objImportador = new NucleusRH.Base.Liquidacion_VarAcum.clsImportarVariables();
            objImportador.ImportarVariablesPersona(poi_empresa);
        }
    }
}

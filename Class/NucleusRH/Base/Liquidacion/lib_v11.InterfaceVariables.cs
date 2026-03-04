using System;
using System.Xml;
using System.IO;
using System.Collections;

using Nomad.NSystem.Document;
using Nomad.NSystem.Proxy;
using Nomad.NSystem.Base;
using Nomad.NSystem.Functions;

namespace NucleusRH.Base.Liquidacion.InterfaceVariables
{

    //////////////////////////////////////////////////////////////////////////////////
    //Clase efinicion de interfaz de entrada de tipo uno
    public partial class ENTRADA
    {
        public static void ImportarVariables(string poi_liquidacion)
        {
            NucleusRH.Base.Liquidacion.clsImportarVariables objImportador = new NucleusRH.Base.Liquidacion.clsImportarVariables();
            objImportador.ImportarVariablesPersona(poi_liquidacion);
        }
    }
}

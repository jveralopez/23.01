using System;
using System.Xml;
using System.IO;
using System.Collections;

using Nomad.NSystem.Document;
using Nomad.NSystem.Proxy;
using Nomad.NSystem.Base;
using Nomad.NSystem.Functions;

namespace NucleusRH.Base.Liquidacion.InterfaceConceptosMan
{

    //////////////////////////////////////////////////////////////////////////////////
    //Clase Definición de la interfaz de Conceptos Manuales
    public partial class ENTRADA
    {
        public static void Importar(string poi_liquidacion)
        {
            NucleusRH.Base.Liquidacion_ConMan.clsImportarConMan objImportador;

            objImportador = new NucleusRH.Base.Liquidacion_ConMan.clsImportarConMan();
            objImportador.Importar(poi_liquidacion);
        }
    }
}

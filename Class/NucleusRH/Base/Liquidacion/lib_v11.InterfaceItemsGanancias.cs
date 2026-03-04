using System;
using System.Xml;
using System.IO;
using System.Collections;

using Nomad.NSystem.Document;
using Nomad.NSystem.Proxy;
using Nomad.NSystem.Base;
using Nomad.NSystem.Functions;

namespace NucleusRH.Base.Liquidacion.InterfaceItemsGanancia
{

    //////////////////////////////////////////////////////////////////////////////////
    //Clase Definición de la interfaz de Items de Ganancia
    public partial class ENTRADA
    {
        public static void Importar(string poi_empresa)
        {
            NucleusRH.Base.Liquidacion_IG.clsImportarIG objImportador;

            objImportador = new NucleusRH.Base.Liquidacion_IG.clsImportarIG();
            objImportador.Importar(poi_empresa);
        }
    }
}

using System;
using System.Xml;
using System.IO;
using System.Collections;

using Nomad.NSystem.Document;
using Nomad.NSystem.Proxy;
using Nomad.NSystem.Base;
using Nomad.NSystem.Functions;

namespace NucleusRH.Base.Liquidacion.InterfaceDistribucionCentroCosto
{

    //////////////////////////////////////////////////////////////////////////////////
    //Clase Definición de la interfaz de Distribucion de Centro de Costo
    public partial class ENTRADA
    {
        public static void Importar(string poi_empresa)
        {
            NucleusRH.Base.Liquidacion_DCC.clsImportarDCC objImportador;

            objImportador = new NucleusRH.Base.Liquidacion_DCC.clsImportarDCC();
            objImportador.Importar(poi_empresa);
        }
    }
}


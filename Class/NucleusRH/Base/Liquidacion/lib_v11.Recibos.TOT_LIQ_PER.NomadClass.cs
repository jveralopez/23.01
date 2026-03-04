using System;
using System.Xml;
using System.IO;
using System.Collections;

using Nomad.NSystem.Document;
using Nomad.NSystem.Proxy;
using Nomad.NSystem.Base;
using Nomad.NSystem.Functions;

namespace NucleusRH.Base.Liquidacion.Recibos
{

    //////////////////////////////////////////////////////////////////////////////////
    //Clase Recibos
    public partial class TOT_LIQ_PER
    {
        public NomadXML ReciboXML = null;
        public int NroLegajo = 0;
        public string Hash = "";

    }
}
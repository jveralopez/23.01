using System;
using System.IO;
using System.Collections;

using Nomad.NSystem.Document;
using Nomad.NSystem.Proxy;
using Nomad.NSystem.Base;
using Nomad.NSystem.Functions;

namespace NucleusRH.Base.Presupuesto.Legajos
{
    public partial class DET_CONCEPTO
  {
      

       public int    Periodo=0;
      public string Tipo="";
      public string Concepto="";

      public void Load(NomadXML QC)
      {
        if (Periodo==0)
        {
          NomadXML D=QC.FindElement("DETT").FindElement2("ROW","oi_detalle_pres",this.oi_detalle_pres);
          this.Periodo=D.GetAttrInt("e_periodo");
          this.Tipo   =D.GetAttr("c_tipo_periodo");

          NomadXML C=QC.FindElement("CONC").FindElement2("ROW","oi_concepto",this.oi_concepto);
          this.Concepto   =C.GetAttr("e_concepto");
        }

      }

  }
}



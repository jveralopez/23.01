using System;
using System.IO;
using System.Collections;

using Nomad.NSystem.Document;
using Nomad.NSystem.Proxy;
using Nomad.NSystem.Base;
using Nomad.NSystem.Functions;

namespace NucleusRH.Base.Presupuesto.Legajos
{
    public partial class NOV_LEGAJO
  {
      
      public int    Periodo=0;
      public string Tipo="";
      public string Novedad="";

      public void Load(NomadXML QC)
      {
        if (this.Novedad=="")
        {
          NomadXML D=QC.FindElement("DETT").FindElement2("ROW","oi_detalle_pres",this.oi_detalle_pres);
          this.Periodo=D.GetAttrInt("e_periodo");
          this.Tipo   =D.GetAttr("c_tipo_periodo");

          NomadXML C=QC.FindElement("NOVE").FindElement2("ROW","oi_novedad",this.oi_novedad);
          this.Novedad=C.GetAttr("c_novedad");
        }

      }

     
  }
}



NucleusRH.Base.Liquidacion.RHLiq.RHLiqControl rhliq = new NucleusRH.Base.Liquidacion.RHLiq.RHLiqControl();

string list_oi_per_emp = "";

Nomad.NSystem.Document.NmdDocument rows = ois_liquidar;
Nomad.NSystem.Document.NmdDocument oi_liquidar = rows.GetFirstChildDocument();
while(oi_liquidar!=null) {
  if (list_oi_per_emp.Length != 0) {
    list_oi_per_emp+=",";
  }
  list_oi_per_emp += oi_liquidar.GetAttribute("oi_personal_emp").Value;
  oi_liquidar = rows.GetNextChildDocument();
}

rhliq.StartLiq(int.Parse(oi_liquidacion), list_oi_per_emp, true);
return;



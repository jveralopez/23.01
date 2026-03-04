try
{
  NucleusRH.Base.Liquidacion.RHLiq.RHLiqControl rhliq = new NucleusRH.Base.Liquidacion.RHLiq.RHLiqControl();

  if (!NomadProxy.GetProxy().Lock().LockOBJ("Liquidacion:id="+oi_liquidacion))
  {
    NomadBatch MyLOG=new NomadBatch("Liquidacion", "Liquidacion", 0, 100);
    MyLOG.Err("La liquidación esta siendo **Procesada** en este momento.\\\\ Intente más tarde, si el problema persiste consulte con su administrador.");
    return;
  }

  string list_oi_per_emp = "";
  for (NomadXML MyCUR=ois_liquidar.FirstChild().FirstChild().FirstChild(); MyCUR!=null; MyCUR=MyCUR.Next())
  {
    list_oi_per_emp+=","+MyCUR.GetAttr("VALUES");
  }

  rhliq.StartLiq(int.Parse(oi_liquidacion), list_oi_per_emp.Substring(1), false);
  return;
}
catch(Exception e)
{
  NomadException ex = NomadException.NewInternalException("RemoteStartLiq", e);
  ex.SetValue("oi_liquidacion", oi_liquidacion);
  ex.SetValue("ois_liquidar", ois_liquidar.ToString());
  throw ex;
}



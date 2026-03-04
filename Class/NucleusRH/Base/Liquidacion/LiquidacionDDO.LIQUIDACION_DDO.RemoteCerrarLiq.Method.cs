NucleusRH.Base.Liquidacion.RHLiq.RHLiqControl rhliq = new NucleusRH.Base.Liquidacion.RHLiq.RHLiqControl();

if (!NomadProxy.GetProxy().Lock().LockOBJ("Liquidacion:id="+oi_liquidacion))
{
  NomadBatch MyLOG=new NomadBatch("Liquidacion", "Liquidacion", 0, 100);
  MyLOG.Err("La liquidación esta siendo **Procesada** en este momento.\\\\ Intente más tarde, si el problema persiste consulte con su administrador.");
  return;
}

rhliq.CloseLiq(int.Parse(oi_liquidacion), l_delPerNoLiq, l_compPerChange, l_compConcChange, l_compEmpChange, l_CerrarLiquidacion, l_ActAcumuladores);
return;



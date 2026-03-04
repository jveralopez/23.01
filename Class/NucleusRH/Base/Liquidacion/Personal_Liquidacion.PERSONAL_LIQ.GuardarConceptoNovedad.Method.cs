		//param ingreso el documento resultado de todas las elecciones.
												 	
				Nomad.NSystem.Document.NmdXmlDocument x;
				
				for (x=(Nomad.NSystem.Document.NmdXmlDocument)param.GetFirstChildDocument(); x!=null; x=(Nomad.NSystem.Document.NmdXmlDocument)param.GetNextChildDocument())
					{
						string personal = x.GetAttribute("value").Value;	
						string valvaren = x.GetAttribute("oi_val_varen").Value;	
						string valor = x.GetAttribute("n_valor").Value;	
						string observacion = x.GetAttribute("o_val_varen").Value;	
						
						NucleusRH.Base.Liquidacion.Personal_Liquidacion.VAL_VAREN ddoValVaren = NucleusRH.Base.Liquidacion.Personal_Liquidacion.VAL_VAREN.Get(valvaren,Proxy);
												
						string obsvalvaren = ddoValVaren.o_val_varen.TypedValue;
						string valorvalvaren = ddoValVaren.n_valor.TypedValue;
						
					
					
						if (valor != valorvalvaren)
							{
								ddoValVaren.n_valor.TypedValue = valor;
						NucleusRH.Base.Liquidacion.Personal_Liquidacion.PERSONAL_LIQ ddoPerLiq = NucleusRH.Base.Liquidacion.Personal_Liquidacion.PERSONAL_LIQ.Get(personal,Proxy);			
						
								string recibo = ddoPerLiq.oi_tot_liq_pers_Id.TypedValue;
										
			
							NucleusRH.Base.Liquidacion.Recibos.TOT_LIQ_PER ddoTotLiqPer = new NucleusRH.Base.Liquidacion.Recibos.TOT_LIQ_PER(this.Proxy);
							
							ddoTotLiqPer.EliminarRecibo(recibo);
								}
						if (observacion != obsvalvaren)
							{
								ddoValVaren.o_val_varen.TypedValue = observacion;								
								}				
								ddoValVaren.Save();				
					}
						
	
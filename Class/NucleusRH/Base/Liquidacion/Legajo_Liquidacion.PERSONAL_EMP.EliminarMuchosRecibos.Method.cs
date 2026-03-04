	// Llama al metodo "Eliminar Recibos" para cada recibo que posea la persona a la que se le agrega una nueva variable fija
	Nomad.NSystem.Document.NmdXmlDocument x;
				
				for (x=(Nomad.NSystem.Document.NmdXmlDocument)param.GetFirstChildDocument(); x!=null; x=(Nomad.NSystem.Document.NmdXmlDocument)param.GetNextChildDocument())
					{
				 	
						 
						 if (x.GetAttribute("value")!= null)
						 {
						 
						 string recibo = x.GetAttribute("value").Value;				 
							NucleusRH.Base.Liquidacion.Recibos.TOT_LIQ_PER ddoTotLiqPer = new NucleusRH.Base.Liquidacion.Recibos.TOT_LIQ_PER(this.Proxy);
							
							ddoTotLiqPer.EliminarRecibo(recibo);					
						}
						
		}
/*este metodo corre cuando se guarda una liquidacion */
/* modif = 1 si se cambio algun valor de la liquidacion que merezca que se borren todos los recibos */
			string modificado = param.GetAttribute("modif").Value;	
			
			if (modificado == "1") 
			 {
			 	string auxID = this.id.TypedValue;
			 	string oi_liq = "<DATOS oi_Liquidacion=\"" + auxID + "\"/>";
			 	NmdXmlDocument paramID = new NmdXmlDocument(oi_liq);	
				Nomad.NSystem.Document.NmdXmlDocument dokum = new Nomad.NSystem.Document.NmdXmlDocument(Proxy.SQLService().Get(this.GetObjectDocument().GetResource("QRY_PERSONAS_INI"),paramID.DocumentToString()));
				Nomad.NSystem.Document.NmdXmlDocument x;
				
				for (x=(Nomad.NSystem.Document.NmdXmlDocument)dokum.GetFirstChildDocument(); x!=null; x=(Nomad.NSystem.Document.NmdXmlDocument)dokum.GetNextChildDocument())
					{
						string pers = x.GetAttribute("oi_personal_liq").Value;				 	
						NucleusRH.Base.Liquidacion.Personal_Liquidacion.PERSONAL_LIQ ddoPerLiq = NucleusRH.Base.Liquidacion.Personal_Liquidacion.PERSONAL_LIQ.Get(pers,Proxy);
						if (ddoPerLiq.oi_tot_liq_pers_Id.Value!= "")
							{
								NucleusRH.Base.Liquidacion.Recibos.TOT_LIQ_PER recibo = new NucleusRH.Base.Liquidacion.Recibos.TOT_LIQ_PER(this.Proxy); 
								recibo.EliminarRecibo(ddoPerLiq.oi_tot_liq_pers_Id.Value);
							}
					}

				}		

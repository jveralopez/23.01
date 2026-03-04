			if (this.oi_tot_liq_pers_Id.Value != "")
				{
					//Console.WriteLine("**************************** 1 " + this.GetObjectDocument().InnerXmlElement.OuterXml);
					NucleusRH.Base.Liquidacion.Recibos.TOT_LIQ_PER F = new NucleusRH.Base.Liquidacion.Recibos.TOT_LIQ_PER(this.Proxy);
					F.EliminarRecibo(this.oi_tot_liq_pers_Id.Value);
				//	Console.WriteLine("**************************** 2 " + this.GetObjectDocument().InnerXmlElement.OuterXml);				
				}else
					this.Save();


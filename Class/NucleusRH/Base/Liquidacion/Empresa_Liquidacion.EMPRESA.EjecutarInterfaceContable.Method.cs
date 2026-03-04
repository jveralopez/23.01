			NmdXmlDocument param = new NmdXmlDocument(aParametro);

				/* aqui deberia llamar al store procedure con el filtro */ 
				
			string b = "<VOUCHERS><VOUCHER sAccount=\"10\" dEffecdate=\"\" nAmount=\"\" ndecimal=\"\" sColumn=\"\" ssueldo=\"\" sconcepto=\"\" sgroup=\"\" sgroup1=\"\" sgroup2=\"\" scentrocosto=\"\"/><VOUCHER sAccount=\"\" dEffecdate=\"\" nAmount=\"\" ndecimal=\"\" sColumn=\"\" ssueldo=\"\" sconcepto=\"\" sgroup=\"\" sgroup1=\"\" sgroup2=\"\" scentrocosto=\"\"/></VOUCHERS>";
			NmdXmlDocument parametro = new NmdXmlDocument(b);
			Console.WriteLine(parametro.ToString());
			
			string Salida = "<SALIDA Empresa=\'" + param.GetAttribute("oi_empresa").Value + "\'/>";
			NmdXmlDocument docSalida = new NmdXmlDocument(Salida);
			docSalida.AddAttribute("Periodo", param.GetAttribute("Periodo").Value);
			docSalida.AddAttribute("sufix", param.GetAttribute("Periodo").Value + ".txt");
			docSalida.AddAttribute("Interfaz_Contable", param.GetAttribute("Interfaz_Contable").Value);
			
			NmdXmlDocument docSalidaAUX = new NmdXmlDocument("<DATOS/>");
				Nomad.NSystem.Document.NmdXmlDocument cadaReg;
				for (cadaReg=(Nomad.NSystem.Document.NmdXmlDocument)parametro.GetFirstChildDocument(); cadaReg!=null; cadaReg=(Nomad.NSystem.Document.NmdXmlDocument)parametro.GetNextChildDocument())
				{				
					string cadaSal = "";
					cadaSal = cadaSal + LFill(cadaReg.GetAttribute("sAccount").Value, 10, '0');
					cadaSal = cadaSal + LFill(cadaReg.GetAttribute("dEffecdate").Value, 8, '0');
					cadaSal = cadaSal + LFill(cadaReg.GetAttribute("nAmount").Value, 15, '0');
					cadaSal = cadaSal + LFill(cadaReg.GetAttribute("ndecimal").Value, 3, '0');
					cadaSal = cadaSal + LFill(cadaReg.GetAttribute("sColumn").Value, 1, '0');
					cadaSal = cadaSal + LFill(cadaReg.GetAttribute("ssueldo").Value, 5, '0');
					cadaSal = cadaSal + LFill(cadaReg.GetAttribute("sconcepto").Value, 25, '0');
					cadaSal = cadaSal + LFill(cadaReg.GetAttribute("scentrocosto").Value, 150, '0');
					docSalidaAUX.AddChildDocument("<Registro valor= \"" + cadaSal + "\"/>");
				}
				docSalida.AddChildDocument(docSalidaAUX.ToString());
		
			Console.WriteLine("11 " + docSalida.ToString());
			return docSalida.ToString(); //esto le devuelve el control a la Interfaz
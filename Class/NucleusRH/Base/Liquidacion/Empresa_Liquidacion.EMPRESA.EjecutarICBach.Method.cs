	  Proxy.Batch().Trace.Add("ifo", "la ejecucion de la Interfaz Contable ha Comenzado", "InterfazContable");
		string nombreArchivo  = "InterfaceContable";
// desde aqui 
// desde aqui 
// desde aqui 
			Nomad.NSystem.Proxy.NomadProxy MyProxy = null;
			Nomad.NSystem.Proxy.BatchService MyExec = null;
			MyProxy = new Nomad.NSystem.Proxy.NomadProxy();
		
			//Paso 1) Login
			//MyProxy.GetContext("<cfg ip=\"nucleusnet\" port=\"1600\"/>"); //desa
			MyProxy.GetContext("<cfg ip=\"nucleusnet\" port=\"1700\"/>"); //test
			MyProxy.Login("NUCLEUSRH","matias","matias","192.168.1.1");

			// Paso 2) creo el batch
			MyExec=MyProxy.BatchService();

			// Paso 3) Agrego los parametros
			MyExec.AddParam(new Nomad.NSystem.Proxy.RPCParam("interface-name","IN", "NucleusRH.Base.Liquidacion.Empresa_Liquidacion.InterfaceContable"));
			MyExec.AddParam(new Nomad.NSystem.Proxy.RPCParam("interface-param","IN", "Nomad.NSystem.Object", "Nomad.NSystem.Document.NmdXmlDocument",param.ToString()));
			MyExec.AddParam(new Nomad.NSystem.Proxy.RPCParam("interface-file","IN", nombreArchivo));
			// Paso 4) ejecuto el metodo
			MyExec.Execute("Nomad.NSystem.Interface.NmdInterfaceBatch","execute");

// hasta aqui 
// hasta aqui 
// hasta aqui 
// hasta aqui 


		Proxy.Batch().Trace.Add("ifo", "la ejecucion de la Interfaz Contable termino exitosamente", "InterfazContable");
		string nombreArchivoAImp = nombreArchivo + param.GetAttribute("Periodo").Value;
		Proxy.Batch().Trace.Add("ifo", "se ha Guardado en el archivo: " + nombreArchivoAImp + ".txt", "InterfazContable");

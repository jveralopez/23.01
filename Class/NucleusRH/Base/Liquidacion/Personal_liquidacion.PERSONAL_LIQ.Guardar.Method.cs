NomadEnvironment.GetTrace().Info("Comienza el método de Inicializar");

if (estado != "C") {
    /* inicializa personas a la liquidacion */ 
    bool bolConPersonas = false;
    NucleusRH.Base.Liquidacion.Personal_Liquidacion.PERSONAL_LIQ DDOAux = new NucleusRH.Base.Liquidacion.Personal_Liquidacion.PERSONAL_LIQ();
	
    Nomad.NSystem.Document.NmdXmlDocument dokum;
    dokum = new Nomad.NSystem.Document.NmdXmlDocument(Nomad.NSystem.Proxy.NomadProxy.GetProxy().SQLService().Get(NucleusRH.Base.Liquidacion.Personal_Liquidacion.PERSONAL_LIQ.Resources.QRY_Inicializar, param.DocumentToString()));
    
  	NomadEnvironment.GetTrace().Info("Formato esperado: <DATOS><ROWS><ROW>");
  	NomadEnvironment.GetTrace().Info("Resultado dokum: " + dokum.ToString());

    string liqui = param.GetAttribute("oi_Liquidacion").Value;	
    Nomad.NSystem.Document.NmdDocument nxdPersonas = dokum.GetFirstChildDocument();
    
    NomadEnvironment.GetTrace().Info("hijo dokum: " + nxdPersonas.ToString());
    
    Nomad.NSystem.Document.NmdDocument ndoPersona;

	//Inicia la transacción
	NomadEnvironment.GetCurrentTransaction().Begin();
	bool bandera = true;
	string sError = "";	
	string sOK = "";	
	string strResult = "";                                                  
	int Contador = 1;
	int ContadorOK = 1;
	
    for (ndoPersona = nxdPersonas.GetFirstChildDocument(); ndoPersona != null; ndoPersona = nxdPersonas.GetNextChildDocument()) 
    {
    	NomadEnvironment.GetTrace().Info("por cada persona");
    	if (ndoPersona.GetAttribute("oi_afjp")==null)
			{
				sError = sError + "<ERROR id=\"" + Contador.ToString() + "\" desc=\"La persona " + ndoPersona.GetAttribute("D_APE_Y_NOM").Value + " no tiene cargada la AFJP.\" />"; 
				Contador = Contador + 1;
				bandera = false;
			}    	 	
    	if (ndoPersona.GetAttribute("oi_sindicato")==null)
			{
				sError = sError + "<ERROR id=\"" + Contador.ToString() + "\" desc=\"La persona " + ndoPersona.GetAttribute("D_APE_Y_NOM").Value + " no tiene cargado el Sindicato.\" />"; 
				Contador = Contador + 1;
				bandera = false;
			}
    	if (ndoPersona.GetAttribute("oi_obra_social")==null)
			{
				sError = sError + "<ERROR id=\"" + Contador.ToString() + "\" desc=\"La persona " + ndoPersona.GetAttribute("D_APE_Y_NOM").Value + " no tiene cargada la Obra Social.\" />"; 
				Contador = Contador + 1;
				bandera = false;
			}                                             
			NomadEnvironment.GetTrace().Info("Termino las validaciones");
			if (bandera)
			{   				
				sOK = sOK + "<OK id=\"" + ContadorOK.ToString() + "\" desc=\"" + ndoPersona.GetAttribute("D_APE_Y_NOM").Value + "\" />"; 
				NomadEnvironment.GetTrace().Info(sOK);
				ContadorOK = ContadorOK + 1;
        string pers = ndoPersona.GetAttribute("oi_personal_emp").Value;
        NucleusRH.Base.Liquidacion.Personal_Liquidacion.PERSONAL_LIQ nuevoDDO = new NucleusRH.Base.Liquidacion.Personal_Liquidacion.PERSONAL_LIQ();
        nuevoDDO.oi_liquidacion = liqui;
        nuevoDDO.oi_personal_emp = pers;
        NomadEnvironment.GetCurrentTransaction().Save(nuevoDDO);
        bolConPersonas = true;           
        NomadEnvironment.GetTrace().Info("Guardo la persona");
      }
    }

    if (bolConPersonas) {
        NucleusRH.Base.Liquidacion.Liquidacion.LIQUIDACION ddoLiq = NucleusRH.Base.Liquidacion.Liquidacion.LIQUIDACION.Get(liqui);
        if (ddoLiq.c_estado == "A") {
            ddoLiq.c_estado = "I";
            NomadEnvironment.GetCurrentTransaction().Save(ddoLiq);
            NomadEnvironment.GetTrace().Info("Cambia el estado de la liquidacion");
        }

        try {
            //Hace persistente los cambios
            NomadEnvironment.GetTrace().Info("Antes Commit");
            NomadEnvironment.GetCurrentTransaction().Commit();
            NomadEnvironment.GetTrace().Info("Despues Commit");
        } catch (Exception ex) {
            NomadEnvironment.GetCurrentTransaction().Rollback();
            throw new NomadAppException("Se produjo un error al intentar grabar los cambios. " + ex.Message, ex);
        }
    } else 
    	{
        //Hace un rollback para limpiar la transacción
        NomadEnvironment.GetCurrentTransaction().Rollback();
    	}
   	NomadEnvironment.GetTrace().Info("Va a Imprimier los errores");
   	if (sError == "")
    {
    	 strResult = "<DATOS cant_errores=\"" + (Contador - 1).ToString() + "\"><ERRORES></ERRORES><OKS>"  + sOK +  "</OKS></DATOS>";
    } else
      {
      	strResult = "<DATOS cant_errores=\"" + (Contador - 1).ToString() + "\"><ERRORES>"  + sError +  "</ERRORES><OKS>"  + sOK +  "</OKS></DATOS>";
      }
    NomadEnvironment.GetTrace().Info("Armo los errores: " + strResult);
    Nomad.NSystem.Document.NmdXmlDocument nxdResult;
		nxdResult = new Nomad.NSystem.Document.NmdXmlDocument(strResult);
		NomadEnvironment.GetTrace().Info("Lo Va a devolver");
    return nxdResult;                            
    NomadEnvironment.GetTrace().Info("Termino");
} else
    throw new NomadAppException("La liquidacion se encuentra cerrada, por lo tanto no puede inicializar personas");







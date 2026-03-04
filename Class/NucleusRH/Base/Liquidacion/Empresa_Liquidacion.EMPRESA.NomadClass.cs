   
		  
		public string EjecutarSIJP(Nomad.NSystem.Document.NmdXmlDocument param)
		{		
		
			/* esta funcion ejecuta la Interfaz del SIJP */
			
			/* Primero debo traer la empresa */
			
			string empresa = param.GetAttribute("oi_empresa").Value;
			NucleusRH.Base.Liquidacion.Empresa_Liquidacion.EMPRESA ddoEmpresa = NucleusRH.Base.Liquidacion.Empresa_Liquidacion.EMPRESA.Get(empresa,Proxy);
			
			/* AQUI LLAMO A UNA FUNCION QUE DEBERIA FIJARSE SI LA EMPRESA TIENE UN TIPO DE EMPRESA PARA 
			EL SIJP DEFINIDO SI ES ASI SE SIGUE CON LA EJECUCION SI NO SE TERMINA CON LA MISMA */
			string tipoEmp = ddoEmpresa.VerificarParametro("tipoEmpresa");
			if (tipoEmp!="noHay")
			{
				
				/* AQUI LLAMO A UNA FUNCION QUE TIENE QUE FIJARSE EN TODAS LAS UBICACIONES QUE TIENE CARGADA 
				LA EMPRESA SI TIENEN ZONAS SIJP, SI ES ASI SIGUE CON LA EJECUCION, SI NO SE TERMINA LA MISMA */
				
				if (ddoEmpresa.VerificarTodasLasUbicaciones())
					{
						
						/* si se ingresaron liquidaciones se fijaran que la fecha de liquidacion esten dentro del periodo, si no se ingresaron ninguna liquidacion se tomaran todas las del periodo. SIEMPRE QUE ESTE CERRADAS. */
						Nomad.NSystem.Document.NmdXmlDocument LiquidacionDoc = new Nomad.NSystem.Document.NmdXmlDocument(Proxy.SQLService().Get(ddoEmpresa.GetObjectDocument().GetResource("QRY_LIQUIDACIONES_SIJP"),param.DocumentToString()));						
 						/* en LiquidacionDoc Guardo todas las liquidaciones con las que voy a trabajar 
 						 el documento LiquidacionDoc  tendra la forma:
								<LIQUIDACIONES hayLiquidacion="1">
									<LIQUIDACION OI_LIQ="111113"/>
									<LIQUIDACION OI_LIQ="111213"/>
									<LIQUIDACION OI_LIQ="111313"/>
								</LIQUIDACIONES> 						
						donde hayLiquidacion sera igual a 0 cuando no haya ninguna liquidacion con la cual trabajar, por lo tanto evito buscar si esta vaciio */
						
						string preguntaLiquidacion = LiquidacionDoc.GetAttribute("hayLiquidacion").Value; 
						
						if (preguntaLiquidacion == "1")
						{
							//Console.WriteLine("Mensaje:" + LiquidacionDoc);
										Nomad.NSystem.Document.NmdXmlDocument PERSONAS_VALIDAS = new Nomad.NSystem.Document.NmdXmlDocument(Proxy.SQLService().Get(ddoEmpresa.GetObjectDocument().GetResource("VALIDAR_PERSONAS_SIJP"),LiquidacionDoc.DocumentToString()));					
										/* ESTE RECURSO SE FIJA EN LAS PERSONAS QUE PUEDEN SER PROCESADAS. 
										A DICHAS PERSONAS LAS COLOCA EN EL TAG PERSONAS_VALIDAS;
										A LAS PERSONAS QUE NO PUEDEN SER PROCESADAS LAS COLOCA EN PERSONAS_NO_VALIDAS */	
										
										Nomad.NSystem.Document.NmdXmlDocument salida = ddoEmpresa.LiquidacionSIJP(PERSONAS_VALIDAS, tipoEmp);
										//Console.WriteLine(salida.ToString());
										/**/
										return salida.ToString();				
							
						}else
							{
								Proxy.Batch().Trace.Add("ifo", "La Ejecucion de la Interfaz del SIJP ha concluido porque la empresa no tiene liquidaciones cerradas para el periodo ingresado", "InterfazSIJP");
								throw new NomadAppException("La Ejecucion de la Interfaz del SIJP ha concluido porque la empresa no tiene liquidaciones cerradas para el periodo ingresado");				
							}
						
					}else
						{
							Proxy.Batch().Trace.Add("ifo", "La Ejecucion de la Interfaz del SIJP ha sido cancelada porque la empresa tiene definida por lo menos una ubicacion donde trabajan personas y no tiene definida una zona SIJP", "InterfazSIJP");
							throw new NomadAppException("La Ejecucion de la Interfaz del SIJP ha sido cancelada porque la empresa tiene definida por lo menos una ubicacion donde trabajan personas y no tiene definida una zona SIJP");				
						}
			
			}else
				{
					Proxy.Batch().Trace.Add("ifo", "La Ejecucion de la Interfaz del SIJP ha sido cancelada porque la empresa no tiene definido un tipo de empresa para el SIJP", "InterfazSIJP");
					throw new NomadAppException("La Ejecucion de la Interfaz del SIJP ha sido cancelada porque la empresa no tiene definido un tipo de empresa para el SIJP");				
				}
 
		}
			
		public string VerificarParametro(string TIPO_PARAMETRO)
		{			
			/* Esta Funcion busca entre todos los parametros que tiene cargado la empresa
			 si tiene el que corresponde a tipo de empresa. si es asi devuelve verdadero, 
			si no devuelve falso */ 
				
				/* desde aqui */ 
				string tipoEmp = "<DATOS c_parametro=\"" + TIPO_PARAMETRO + "\"/>";
				NmdXmlDocument oi_tipo_empresa = new NmdXmlDocument(tipoEmp);
				Nomad.NSystem.Document.NmdXmlDocument oi_tipo_empresa_doc = new Nomad.NSystem.Document.NmdXmlDocument(Proxy.SQLService().Get(this.GetObjectDocument().GetResource("QRY_PARAMETRO"),oi_tipo_empresa.DocumentToString()));
				string oi_parametro = oi_tipo_empresa_doc.GetAttribute("OI_PARAMETRO").Value;
				/* hasta aqui */
				/* sirve para enviarle el codigo del parametro al qry asi este devuelve el oi del parametro */
				
				int aplicarPolitica = 0; //esta variable la usa para saber si debo aplicar una politica o no.
				
				if (oi_parametro!="noExiste")				
					{ //si el parametro esta definido
					
						/* cargo el parametro */
						NucleusRH.Base.Organizacion.Parametros.PARAMETRO elParametro = NucleusRH.Base.Organizacion.Parametros.PARAMETRO.Get(oi_parametro,Proxy);
						
						NucleusRH.Base.Liquidacion.Empresa_Liquidacion.PARAM_EMPRE cadaParametro;
						
						int preguntaTemp = 0; //esta variable la uso para saber si retorno valor o no
						
						if (this.PARAM_EMP!=null) 
							{ //si la empresa tiene algun parametro definido en forma particular
	
	
								for (cadaParametro = this.PARAM_EMP.First(); cadaParametro!=null; cadaParametro = this.PARAM_EMP.Next()) //me fijo en cada parametro 
									{
										if (cadaParametro.oi_parametro_Id.Value == oi_parametro)
											{ 
												/* este if sirve para saber si la empresa tiene cargado algun parametro en particular */					
												preguntaTemp = 1;
												return cadaParametro.d_valor.TypedValue;
											}
										
									
									}
								
							}
							
						if (preguntaTemp == 0)
							{ /*ESTO ES: SI LA EMPRESA NO TIENE ALGUN VALOR ASIGNADO PARA TIPO DE EMPRESA DEL SIJP
								SE TOMA EL VALOR QUE ESTA POR DEFECTO*/
								if (elParametro.d_valor!=null)
									{
										return elParametro.d_valor.Value;
									}else
										{
											
											aplicarPolitica = 1;
										}
							}	
							
					}else
						{
							aplicarPolitica = 1;
						}
				
				if (aplicarPolitica == 1)
					{ //aqui se aplica la politica para conseguir el parametro.
						return "noHay"; //es fija porque no existe politica definida.
					}else
						return "noHay"; //es fija porque no existe politica definida.
		}		
		
		
		public string devolverCodigoParametro(Nomad.NSystem.Document.NmdXmlDocument persona, string tipo_parametro)
		{
			/* Esta Funcion busca entre todos los parametros que tiene cargado la persona
			 si tiene el que corresponde a PARAMETRO. si es asi devuelve EL VALOR, 
			si no devuelve 'EJECUTAR_POLITICA' */ 
			/* ESTA RECIBE COMO PARAMETRO TODOS LOS DATOS DE LA PERSONA, Y EL TIPO DE PARAMETRO BUSCADO */
				
				/* desde aqui */ 
				string tipoPam = "<DATOS c_parametro=\""+ tipo_parametro + "\"/>";
				NmdXmlDocument oi_P = new NmdXmlDocument(tipoPam);
				Nomad.NSystem.Document.NmdXmlDocument parametro = new Nomad.NSystem.Document.NmdXmlDocument(Proxy.SQLService().Get(this.GetObjectDocument().GetResource("QRY_PARAMETRO"),oi_P.DocumentToString()));
				string oi_parametro = parametro.GetAttribute("OI_PARAMETRO").Value;
				/* hasta aqui */
				/* sirve para enviarle el codigo del parametro al qry asi este devuelve el oi del parametro */
				
				string aplicarPolitica = ""; //esta variable la usa para saber si debo aplicar una politica o no.
				
				if (oi_parametro!="noExiste")
					{ //si el parametro esta definido
					
						/* cargo el parametro */
						NucleusRH.Base.Organizacion.Parametros.PARAMETRO elParametro = NucleusRH.Base.Organizacion.Parametros.PARAMETRO.Get(oi_parametro,Proxy);

						int preguntaTemp = 0; //esta variable la uso para saber si retorno valor o no
						
						Nomad.NSystem.Document.NmdXmlDocument PARAMdOC = (Nomad.NSystem.Document.NmdXmlDocument)persona.GetDocumentByName("PARAMETROS"); 
						
						Nomad.NSystem.Document.NmdXmlDocument cadaParametro; 
						for (cadaParametro = (Nomad.NSystem.Document.NmdXmlDocument)PARAMdOC.GetFirstChildDocument(); cadaParametro!=null; cadaParametro = (Nomad.NSystem.Document.NmdXmlDocument)PARAMdOC.GetNextChildDocument()) //me fijo en cada parametro 
							{
								if (cadaParametro.GetAttribute("oi_parametro").Value == oi_parametro)
									{ 
										/* este if sirve para saber si la persona tiene cargado algun parametro en particular */					
										preguntaTemp = 1;
										aplicarPolitica = cadaParametro.GetAttribute("d_valor").Value;
									}
							}
							
						if (preguntaTemp == 0)
							{ /*ESTO ES: SI LA EMPRESA NO TIENE ALGUN VALOR ASIGNADO PARA TIPO DE EMPRESA DEL SIJP
								SE TOMA EL VALOR QUE ESTA POR DEFECTO*/
								if (elParametro.d_valor!=null)
									{
										if (elParametro.d_valor.Value != "")
											{
												aplicarPolitica = elParametro.d_valor.Value;
											}else
												{
													aplicarPolitica = "1";
												}
									}else
										{
											aplicarPolitica = "1";
										}
							}	
							
					}else
						{
							aplicarPolitica = "1";
						}
				
				if (aplicarPolitica == "1")
					{ //aqui se aplica la politica para conseguir el parametro.
						return "APLICAR_POLITICA"; //es fija porque no existe politica definida.
					}else
						return aplicarPolitica; //es fija porque no existe politica definida.
			
		}
		

		public string devolverCodigoParametroDefecto(string tipo_parametro)
		{
			/* Esta Funcion busca entre todos los parametros si este esta definido devuelve el valor si no 
			 devuelve  'EJECUTAR_POLITICA' */ 
				
				/* desde aqui */ 
				string tipoPam = "<DATOS c_parametro=\""+ tipo_parametro + "\"/>";
				NmdXmlDocument oi_P = new NmdXmlDocument(tipoPam);
				Nomad.NSystem.Document.NmdXmlDocument parametro = new Nomad.NSystem.Document.NmdXmlDocument(Proxy.SQLService().Get(this.GetObjectDocument().GetResource("QRY_PARAMETRO"),oi_P.DocumentToString()));
				string oi_parametro = parametro.GetAttribute("OI_PARAMETRO").Value;
				/* hasta aqui */
				/* sirve para enviarle el codigo del parametro al qry asi este devuelve el oi del parametro */
				string aplicarPolitica;
				if (oi_parametro!="noExiste")
					{ //si el parametro esta definido
					
						/* cargo el parametro */
						NucleusRH.Base.Organizacion.Parametros.PARAMETRO elParametro = NucleusRH.Base.Organizacion.Parametros.PARAMETRO.Get(oi_parametro,Proxy);

						if (elParametro.d_valor!=null)
							{
								aplicarPolitica = elParametro.d_valor.Value;
							}else
								{
									aplicarPolitica = "1";
								}
					}else
						{
							aplicarPolitica = "1";
						}
				
				if (aplicarPolitica == "1")
					{ //aqui se aplica la politica para conseguir el parametro.
						return "APLICAR_POLITICA"; //es fija porque no existe politica definida.
					}else
						return aplicarPolitica; //es fija porque no existe politica definida.
			
		}
		
		
		public bool VerificarTodasLasUbicaciones()
		{			

			/* esta funcion busca todas las ubicaciones de la empresa, una vez que las identifica se fija si todas tienen una zona SIJP cargada, si esto es valido para todas devuelve true, si alguna de las mismas no tiene cargado una zona SIJP, se fija si alguna persona que trabaje en la empresa tiene asignada esa ubicacion, si esto ultimo es verdadero, se devuelve false pues la ubicacion que no tiene zona tiene personas; si es falso devuelve verdadero pues significa que la ubicacion/es que no tiene zona SIJP tampoco tiene personas, por lo tanto no hay problemas */ 
			
			
			//Console.WriteLine("hola desde VerificarTodasLasUbicaciones");
			
			int hayUbicacion = 0; //esta variable la uso para saber si hay alguna ubicacion con una zona definida.
			
			NucleusRH.Base.Liquidacion.Empresa_Liquidacion.UBICACION cadaUbicacion;
			if (this.UBICACIONES!=null) //Si tiene alguna ubicacion definida 
			{

				for (cadaUbicacion = this.UBICACIONES.First(); cadaUbicacion!=null; cadaUbicacion = this.UBICACIONES.Next()) //Para cada ubicacion definida para la empresa
					{

						/* desde aqui */ 
						string hayUbicaciones = "<DATOS ubicacion= '" + cadaUbicacion.id.Value + "'/>";
						NmdXmlDocument TempUbicaciones = new NmdXmlDocument(hayUbicaciones);
						Nomad.NSystem.Document.NmdXmlDocument ubicacionesDoc = new Nomad.NSystem.Document.NmdXmlDocument(Proxy.SQLService().Get(this.GetObjectDocument().GetResource("QRY_UBICACION_SIJP"),TempUbicaciones.DocumentToString()));
						string Pregunta = ubicacionesDoc.GetAttribute("esta").Value;
						/* hasta aqui */
						/* aqui debo llamar un recurso.	si la ubicacion actual (que no tiene zona sijp )
						que tiene personas	el recurso me debolvera 1, sino me devolver 0 */
						
						if (Pregunta == "1") //si la ubicacion tiene personas
							{
								if (cadaUbicacion.oi_zona_sijp_Id!=null) //primero me fijo si tiene una zona SIJP definida
										{ // si tiene personas deberia tener una ubicacion definida
											hayUbicacion = 1;
										}else //si no tiene error
											{
													return false;
											}
							}
						}
			}else //Si no tiene una Ubicacion debe terminar el proceso
				{
					return false;
				}
			
			if (hayUbicacion == 1)
				{
					return true;
				}else
					{
						return false;
					}
			
					
		
		}		
		
		
		
		public ArrayList CalculoImportes(Nomad.NSystem.Document.NmdXmlDocument persona, ArrayList ArraySitua)
		{
			double remun1 = str2dbl(persona.GetAttribute("n_totsret").Value);
			double remun2 = str2dbl(persona.GetAttribute("n_totsret").Value);
			double remun3 = str2dbl(persona.GetAttribute("n_totsret").Value);
			double remun4 = str2dbl(persona.GetAttribute("n_totsret").Value);
			double remun6 = str2dbl(persona.GetAttribute("n_totsret").Value);
			double remunAdic = 0;
			Nomad.NSystem.Document.NmdXmlDocument VARIABLES = (Nomad.NSystem.Document.NmdXmlDocument)persona.GetDocumentByName("VAL_VAREF"); 
		
			int pregunta = 0;
			Nomad.NSystem.Document.NmdXmlDocument cadaVAL; 
			for (cadaVAL = (Nomad.NSystem.Document.NmdXmlDocument)VARIABLES.GetFirstChildDocument(); cadaVAL!=null; cadaVAL = (Nomad.NSystem.Document.NmdXmlDocument)VARIABLES.GetNextChildDocument()) //me fijo en cada VARIABLE SI ALGUNA ES "FP_ModContrDGI"
				{
					if (cadaVAL.GetAttribute("c_variable").Value == "FP_ModContrDGI")
						{
							pregunta = 1;
						}
				}
			if (persona.GetAttribute("C_TIPO_PERSONAL").Value == "8")
				{
					pregunta = 1;
				}else
					{
						pregunta = 0;
					}
			if (pregunta == 1)
				{
					remun1 = str2dbl(persona.GetAttribute("n_totnsret").Value);
					remun2 = str2dbl(persona.GetAttribute("n_totnsret").Value);
					remun3 = str2dbl(persona.GetAttribute("n_totnsret").Value);
					remun4 = str2dbl(persona.GetAttribute("n_totnsret").Value);
					remun6 = str2dbl(persona.GetAttribute("n_totnsret").Value);
				}
				
				if (ArraySitua.Count > 0)
					{
						if (ArraySitua[0].ToString() == this.devolverCodigoParametro(persona, "arrSituaNoFecEgreso"))
							{
								if (remun1 < 0)
									{
										remun1 = 0;
										remun2 = 0;
										remun3 = 0;
										remun4 = 0;
										remun6 = 0;
									}
							} 
					}
			ArrayList calculos = new ArrayList();
			calculos.Add(remun1);			
			calculos.Add(remun2);
			calculos.Add(remun3);
			calculos.Add(remun4);
			calculos.Add(remun6);
			calculos.Add(remunAdic);
			return calculos;
			
		}
		
		public Nomad.NSystem.Document.NmdXmlDocument LiquidacionSIJP(Nomad.NSystem.Document.NmdXmlDocument PERSONAS_VALIDAS, string tipoEmp)
		{
			
			
			Nomad.NSystem.Document.NmdXmlDocument personasDoc = new Nomad.NSystem.Document.NmdXmlDocument(Proxy.SQLService().Get(this.GetObjectDocument().GetResource("QRY_PERSONAS_SIJP"),PERSONAS_VALIDAS.ToString()));						
			
			//Console.WriteLine("Mensaje: 0" + personasDoc);
			/* este recurso trae todos los datos de las personas */
			
			/* creamos el documento donde vamos a guardar todos los datos (de salida) de la persona */
			ArrayList ArraySalidaSIJP = new ArrayList();
			Nomad.NSystem.Document.NmdXmlDocument DocSalidaPersonasValidas = new Nomad.NSystem.Document.NmdXmlDocument("<PERSONAS_VALIDAS sufix=\"" + PERSONAS_VALIDAS.GetAttribute("periodo").Value + ".txt\"/>");
			Nomad.NSystem.Document.NmdXmlDocument DocSalidaPersonas = new Nomad.NSystem.Document.NmdXmlDocument("<PERSONAS/>");
			/* cada persona */
			Nomad.NSystem.Document.NmdXmlDocument persona; 
			
			for (persona=(Nomad.NSystem.Document.NmdXmlDocument)personasDoc.GetFirstChildDocument(); persona!=null; persona=(Nomad.NSystem.Document.NmdXmlDocument)personasDoc.GetNextChildDocument())
				{
				//Console.WriteLine(" hola desde el inicio " + persona.GetAttribute("c_nro_cuil").Value);
					
int count = 0;
					ArrayList ColumnasSIJP = new ArrayList();
					string columna;
					
					
					
/* C_CUIL orden = 1 */
				/* la primera columna es el cuil de la persona */ 
				columna = persona.GetAttribute("c_nro_cuil").Value;
				if (columna.Length != 11 )
					{
						Proxy.Batch().Trace.Add("ifo", "Llamar a nacho, que debe hacer una validacion en la carga del CUIL", "InterfazSIJP");
						throw new NomadAppException("Llamar a nacho, que debe hacer una validacion en la carga del CUIL");
					}
/*------*/ColumnasSIJP.Add(columna);
count = count + 1;
//Console.WriteLine("se acaba de incorporar la columna " + count);
					

/* D_APE_Y_NOM orden = 2 */
					/* la segunda columna es el nombre y apellido de la persona */ 
					columna = persona.GetAttribute("d_ape_y_nom").Value;
					if (columna.Length > 30)
						{
							columna = columna.Substring(0,30);
						}
					columna = RFill(columna,30,' ');
/*------*/ColumnasSIJP.Add(columna);
count = count + 1;
//Console.WriteLine("se acaba de incorporar la columna " + count);


/* q_hijos orden = 3 */
					/* AQUI AGREGO EL CODIGO DE CONYUGUE - USANDO UNA FUNCION */
					columna = this.devolverCodigoParametro(persona, "CodigoConyugueSIJP");
					if (columna == "APLICAR_POLITICA")
						{ /* si se aplica politica se llama a la funcion de aplicar politica de codigo de conyugue */
							/* aqui llama a la funcion POLITICA_CONYUGUE */ 
							columna = this.POLITICA_CONYUGUE(persona);
						}
					/* la tercera columna es el codigo de la persona para el conyugue */ 
/*------*/ColumnasSIJP.Add(LFill(columna,1,'0'));
count = count + 1;
//Console.WriteLine("se acaba de incorporar la columna " + count);


/* q_hijos orden = 4 */		
					/* la 4 columna es la cantidad de hijos de de la persona a cargo */ 
					columna = this.CantidadHijos(persona);
/*------*/ColumnasSIJP.Add(LFill(columna,2,'0'));
count = count + 1;
//Console.WriteLine("se acaba de incorporar la columna " + count);

					
					/* aqui armo el arreglo de situaciones */ 
					ArrayList ArraySitua = new ArrayList();
					ArraySitua = this.ArmarArregloSituaciones(persona);

					/* aqui calculo los valores */
					ArrayList Calculos = new ArrayList();
					//Console.WriteLine("persona " + persona);
					Calculos = this.CalculoImportes(persona, ArraySitua);

/* c_situacion orden = 5 */										
					/* AQUI AGREGO EL CODIGO DE situacion - USANDO UNA FUNCION */
					columna = this.devolverCodigoParametro(persona, "CodigoSituacionSIJP");
					if (columna == "APLICAR_POLITICA")
						{ /* si se aplica politica se llama a la funcion de aplicar politica de codigo de conyugue */
							/* aqui llama a la funcion POLITICA_SITUACION */ 
							
							columna = this.POLITICA_SITUACION(persona, ArraySitua);
							//Console.WriteLine("NACHO TE ODIO ahhhhhhhhh noooooooo" + columna);
						}
					/* la tercera columna es el codigo de la persona para el conyugue */ 
										
/*------*/ColumnasSIJP.Add(LFill(columna,2,'0'));
count = count + 1;
//Console.WriteLine("se acaba de incorporar la columna " + count);


/* JUBILADO orden = 6 */
					/* AQUI AGREGO EL CODIGO DE JUBILADO - USANDO UNA FUNCION */
					columna = this.devolverCodigoParametro(persona, "CodigoJubiladoSIJP");
					if (columna == "APLICAR_POLITICA")
						{ /* si se aplica politica se llama a la funcion de aplicar politica de codigo de conyugue */
							/* aqui llama a la funcion POLITICA_Jubilado */ 
							columna = this.POLITICA_JUBILADO(persona);
						}
					/* la tercera columna es el codigo de la persona para el Jubilado */ 
/*------*/ColumnasSIJP.Add(LFill(columna,2,'0'));										
count = count + 1;
//Console.WriteLine("se acaba de incorporar la columna " + count);



/* c_actividad orden = 7 */
					/* AQUI AGREGO EL CODIGO DE ActividadEmpresa - USANDO UNA FUNCION */
					columna = this.devolverCodigoParametro(persona, "ActividadEmpresaSIJP");
					if (columna == "APLICAR_POLITICA")
						{ /* si se aplica politica se llama a la funcion de aplicar politica de codigo de ActividadEmpresa */
							/* aqui llama a la funcion ActividadEmpresa */ 
							columna = this.POLITICA_ACTIVIDAD_EMPRESA(persona);
						}
					/* la columna es el codigo de la persona para el ActividadEmpresa */ 
/*------*/ColumnasSIJP.Add(LFill(columna,2,'0'));										
count = count + 1;
//Console.WriteLine("se acaba de incorporar la columna " + count);



/* c_zona orden = 8 */					
					/* AQUI AGREGO EL CODIGO DE zona - USANDO UNA FUNCION */
					columna = this.devolverCodigoParametro(persona, "CodigoZonaSIJP");
					if (columna == "APLICAR_POLITICA")
						{ /* si se aplica politica se llama a la funcion de aplicar politica */
							
							/* aqui llama a la funcion POLITICA_ZONA_SIJP */ 
							columna = this.POLITICA_ZONA_SIJP(persona);
							
						}
/*------*/ColumnasSIJP.Add(LFill(columna,2,'0'));					
count = count + 1;
//Console.WriteLine("se acaba de incorporar la columna " + count);


/* p_ap_adic_ss orden = 9 */
					/* AQUI AGREGO EL CODIGO DE p_ap_adic_ss - USANDO UNA FUNCION */
					columna = this.devolverCodigoParametro(persona, "p_ap_adic_ss_SIJP");
					if (columna == "APLICAR_POLITICA")
						{
							columna = "00000";
						}
/*------*/ColumnasSIJP.Add(LFill(columna,5,'0'));										
count = count + 1;
//Console.WriteLine("se acaba de incorporar la columna " + count);

/* Mod_contratacion orden = 10 */
					/* AQUI AGREGO EL CODIGO DE Mod_contratacion - USANDO UNA FUNCION */
					
					columna = this.devolverCodigoParametro(persona, "ModalidadContratacionSIJP");
										
					
					if (columna == "APLICAR_POLITICA")
						{ /* si se aplica politica se llama a la funcion de aplicar politica de codigo de ActividadEmpresa */
							/* aqui llama a la funcion ActividadEmpresa */ 
							columna = this.POLITICA_MODALIDAD_CONTRATACION(persona);
						}
/*------*/ColumnasSIJP.Add(LFill(columna,3,'0'));										
count = count + 1;
//Console.WriteLine("se acaba de incorporar la columna " + count);


						
/*c_obra_social orden = 11 */
					columna = persona.GetAttribute("C_OBRA_SOCIAL").Value;
/*------*/ColumnasSIJP.Add(LFill(columna,6,'0'));					
count = count + 1;
//Console.WriteLine("se acaba de incorporar la columna " + count);


/* Cantidad Fam Padres orden = 12 */
					/* AQUI AGREGO EL CODIGO DE Cantidad Fam Padres - USANDO UNA FUNCION */
					columna = this.devolverCodigoParametro(persona, "CantidadFamPadresSIJP");
					if (columna == "APLICAR_POLITICA")
						{ /* si se aplica politica se llama a la funcion de aplicar politica */
							columna = "00";
						}
/*------*/ColumnasSIJP.Add(LFill(columna,2,'0'));			
count = count + 1;
//Console.WriteLine("se acaba de incorporar la columna " + count);

	
	
/* remun_tot orden = 13 */
					double columna23 = str2dbl(persona.GetAttribute("n_totsret").Value) + str2dbl(persona.GetAttribute("n_totnsret").Value);
					if (columna23 < 0)
						{
							columna23 = 0;
						}
					//Console.WriteLine("se acaba de incorporar la columna " + count + "****0" +  Calculos[0] + "****1" +  Calculos[1] + "****2" +  Calculos[2] + "****3" +  Calculos[3]+ "****4" +  Calculos[4]);
					if ((double)Calculos[0] > columna23)
						{
							columna23 = (double)Calculos[0];
						}
					if ((double)Calculos[1] > columna23)
						{
							columna23 = (double)Calculos[1];
						}
					if ((double)Calculos[2] > columna23)
						{
							columna23 = (double)Calculos[2];
						}
					if ((double)Calculos[3] > columna23)
						{
							columna23 = (double)Calculos[3];
						}
					if ((double)Calculos[4] > columna23)
						{
							columna23 = (double)Calculos[4];
						} 
					columna = dbl2str(columna23);
					//Console.WriteLine("se acaba de incorporar la columna " + count + "****" +  columna23 );
/*------*/ColumnasSIJP.Add(LFill(columna,9,'0'));
count = count + 1;
//Console.WriteLine("se acaba de incorporar la columna " + count + "****" +  columna23 );

/* remun_imp1 orden = 14 */
					columna23 = (double)Calculos[0];
					columna = dbl2str(columna23);
/*------*/ColumnasSIJP.Add(LFill(columna,9,'0'));
count = count + 1;
//Console.WriteLine("se acaba de incorporar la columna " + count);

/* asig_fam_pag orden = 15 */
					columna23 = str2dbl(persona.GetAttribute("n_totasig").Value) + str2dbl(persona.GetAttribute("n_totretasig").Value);
					columna = dbl2str(columna23);
/*------*/ColumnasSIJP.Add(LFill(columna,9,'0'));
count = count + 1;
//Console.WriteLine("se acaba de incorporar la columna " + count);

						
/* imp_ap_vol orden = 16 */
					/* AQUI AGREGO EL CODIGO DE imp_ap_vol - USANDO UNA FUNCION */
					columna = this.devolverCodigoParametro(persona, "imp_ap_vol_SIJP");
					if (columna == "APLICAR_POLITICA")
						{
							columna = "000000000";
						}
/*------*/ColumnasSIJP.Add(LFill(columna,9,'0'));									
count = count + 1;
//Console.WriteLine("se acaba de incorporar la columna " + count);



/* imp_adic_os orden = 17 */
					/* AQUI AGREGO EL CODIGO DE imp_adic_os - USANDO UNA FUNCION */
					columna = this.devolverCodigoParametro(persona, "imp_adic_os_SIJP");
					if (columna == "APLICAR_POLITICA")
						{
							columna = "000000000";
						}
/*------*/ColumnasSIJP.Add(LFill(columna,9,'0'));						
count = count + 1;
//Console.WriteLine("se acaba de incorporar la columna " + count);


/* imp_exc_ap_ss orden = 18 */
					/* AQUI AGREGO EL CODIGO DE imp_exc_ap_ss- USANDO UNA FUNCION */
					columna = this.devolverCodigoParametro(persona, "imp_exc_ap_ss_SIJP");
					if (columna == "APLICAR_POLITICA")
						{
							columna = "000000000";
						}
/*------*/ColumnasSIJP.Add(LFill(columna,9,'0'));
count = count + 1;
//Console.WriteLine("se acaba de incorporar la columna " + count);


/* imp_exc_ap_os orden = 19 */
					/* AQUI AGREGO EL CODIGO DE imp_exc_ap_os- USANDO UNA FUNCION */
					columna = this.devolverCodigoParametro(persona, "imp_exc_ap_os_SIJP");
					if (columna == "APLICAR_POLITICA")
						{
							columna = "000000000";
						}
/*------*/ColumnasSIJP.Add(LFill(columna,9,'0'));
count = count + 1;
//Console.WriteLine("se acaba de incorporar la columna " + count);


					

/* provincia orden = 20 */
					/* AQUI AGREGO la Provincia */
/*------*/ColumnasSIJP.Add(LFill("",50,' '));
count = count + 1;
//Console.WriteLine("se acaba de incorporar la columna " + count);

/* remun_imp2 orden = 21 */
					columna23 = (double)Calculos[1];
					columna = dbl2str(columna23);
/*------*/ColumnasSIJP.Add(LFill(columna,9,'0'));
count = count + 1;
//Console.WriteLine("se acaba de incorporar la columna " + count);

/* remun_imp3 orden = 22 */
					columna23 = (double)Calculos[2]; 
					columna = dbl2str(columna23);
/*------*/ColumnasSIJP.Add(LFill(columna,9,'0'));
count = count + 1;
//Console.WriteLine("se acaba de incorporar la columna " + count);

/* remun_imp4 orden = 23 */
					columna23 = (double)Calculos[3]; 
					columna = dbl2str(columna23);
/*------*/ColumnasSIJP.Add(LFill(columna,9,'0'));
count = count + 1;
//Console.WriteLine("se acaba de incorporar la columna " + count);


/* c_Siniestrado orden = 24 */
					/* AQUI AGREGO EL CODIGO DE Siniestrado - USANDO UNA FUNCION */
					columna = this.devolverCodigoParametro(persona, "SiniestradoSIJP");
					if (columna == "APLICAR_POLITICA")
						{ /* si se aplica politica se llama a la funcion de aplicar politica */
							columna = this.devolverCodigoParametroDefecto("SiniestradoDefectoSIJP");
							if (columna == "APLICAR_POLITICA")
							{
								Proxy.Batch().Trace.Add("ifo", "falta definir el parametro SiniestradoDefectoSIJP", "InterfazSIJP");
								throw new NomadAppException("falta definir el parametro SiniestradoDefectoSIJP");
							}
						}
/*------*/ColumnasSIJP.Add(LFill(columna,2,'0'));
count = count + 1;
//Console.WriteLine("se acaba de incorporar la columna " + count);
							
/* l_reduc orden = 25 */					
					/* AQUI AGREGO EL CODIGO DE l_reduc - USANDO UNA FUNCION */
					columna = this.devolverCodigoParametro(persona, "l_reducSIJP");
					if (columna == "APLICAR_POLITICA")
						{ /* si se aplica politica se llama a la funcion de aplicar politica */
							columna = "N";
						}
/*------*/ColumnasSIJP.Add(LFill(columna,1,'0'));
count = count + 1;
//Console.WriteLine("se acaba de incorporar la columna " + count);

/* cap_rec_lrt orden = 26 */
					/* AQUI AGREGO EL CODIGO DE cap_rec_lrt - USANDO UNA FUNCION */
					columna = this.devolverCodigoParametro(persona, "cap_rec_lrtSIJP");
					if (columna == "APLICAR_POLITICA")
						{ /* si se aplica politica se llama a la funcion de aplicar politica */
							columna = "000000000";
						}
/*------*/ColumnasSIJP.Add(LFill(columna,9,'0'));
count = count + 1;
//Console.WriteLine("se acaba de incorporar la columna " + count);
										

					
/*tipo_empresa orden = 27 */
					/* AQUI AGREGO EL CODIGO DE tipo_empresa - USANDO UNA FUNCION */
					columna = tipoEmp;
/*------*/ColumnasSIJP.Add(LFill(columna,1,'0'));
count = count + 1;
//Console.WriteLine("se acaba de incorporar la columna " + count);

/* aporte_adic_os = 28 */
					columna = this.POLITICA_APORTE_ADIC_OS(persona);
/*------*/ColumnasSIJP.Add(LFill(columna,9,'0'));							
count = count + 1;
//Console.WriteLine("se acaba de incorporar la columna " + count);


/* l_regimen orden = 29 */
					/* AQUI AGREGO EL CODIGO DE regimen - USANDO UNA FUNCION */
					columna = this.devolverCodigoParametro(persona, "RegimenPersonaSIJP");
					if (columna == "APLICAR_POLITICA")
						{ /* si se aplica politica se llama a la funcion de aplicar politica */
							/* aqui llama a la funcion */ 
							columna = this.POLITICA_REGIMEN(persona);
						}
/*------*/ColumnasSIJP.Add(LFill(columna,1,'0'));
count = count + 1;
//Console.WriteLine("se acaba de incorporar la columna " + count);




/* SituacionRevista 1 orden = 30-31 */
					/* AQUI AGREGO EL CODIGO DE situacion Revista - USANDO UNA FUNCION */
					string columna1;
					//Console.WriteLine("gabi se va " + ArraySitua.Count );
					if (ArraySitua.Count > 0)
						{
							//Console.WriteLine("gabi gabi gabi " + ArraySitua[0].ToString() );
							//Console.WriteLine("gabi gabi gabi gabigabi gabi gabi " +  ArraySitua[1].ToString().Substring(6,2) );
							columna = ArraySitua[0].ToString();
							columna1 = ArraySitua[1].ToString().Substring(6,2);
							
						}else
							{
								columna = "00";
								columna1 = "00";
							}
/*------*/ColumnasSIJP.Add(LFill(columna,2,'0'));							
count = count + 1;
//Console.WriteLine("se acaba de incorporar la columna " + count);

/*------*/ColumnasSIJP.Add(LFill(columna1,2,'0'));
count = count + 1;
//Console.WriteLine("se acaba de incorporar la columna " + count);

/* SituacionRevista 2 orden = 32-33 */
					/* AQUI AGREGO EL CODIGO DE situacion Revista - USANDO UNA FUNCION */
					if (ArraySitua.Count > 3)
						{
							columna = ArraySitua[3].ToString();
							columna1 = ArraySitua[4].ToString().Substring(6,2);
						}else
							{
								columna = "00";
								columna1 = "00";
							}
/*------*/ColumnasSIJP.Add(LFill(columna,2,'0'));							
count = count + 1;
//Console.WriteLine("se acaba de incorporar la columna " + count);

/*------*/ColumnasSIJP.Add(LFill(columna1,2,'0'));
count = count + 1;
//Console.WriteLine("se acaba de incorporar la columna " + count);

/* SituacionRevista 3 orden = 34-35 */
					/* AQUI AGREGO EL CODIGO DE situacion Revista - USANDO UNA FUNCION */
					if (ArraySitua.Count > 6)
						{
							columna = ArraySitua[6].ToString();
							columna1 = ArraySitua[7].ToString().Substring(6,2);
						}else
							{
								columna = "00";
								columna1 = "00";
							}
/*------*/ColumnasSIJP.Add(LFill(columna,2,'0'));							
count = count + 1;
//Console.WriteLine("se acaba de incorporar la columna " + count);

/*------*/ColumnasSIJP.Add(LFill(columna1,2,'0'));
count = count + 1;
//Console.WriteLine("se acaba de incorporar la columna " + count);

				/* calculo del SAC */
				double sac = 0;
					if (str2dbl(Calculos[0].ToString()) == 0)
						{
							sac = 0;
						}else
							{
								if (str2dbl(persona.GetAttribute("n_totsac").Value) < 0)
									{
										sac = 0;
									}else
										{
											sac = str2dbl(persona.GetAttribute("n_totsac").Value);
										}
							}
				/* fin del calculo del SAC */


				/* inicio del calculo del HorasExtras */
					double horasExtras ;
					if (str2dbl(Calculos[0].ToString()) == 0)
						{
							horasExtras = 0;
						}
					else
						{
							horasExtras	= str2dbl(this.CALCULO_HORAS_EXTRAS(persona));
							if (horasExtras < 0)
								{
									horasExtras = 0;
								}
						}
				/* fin del calculo del HorasExtras */
				
				
				/* inicio del calculo del Vacaciones */
				/* AQUI AGREGO EL CODIGO DE Vacaciones - USANDO UNA FUNCION */
					columna = this.devolverCodigoParametro(persona, "Vacaciones_SIJP");
					if (columna == "APLICAR_POLITICA")
						{
							columna = "000000000";
						}
					double Vacaciones = str2dbl(columna);
				/* fin del calculo del Vacaciones */	
					
				/* inicio del calculo de Premios */
					double premios ;
					if (str2dbl(Calculos[0].ToString()) == 0)
						{
							premios = 0;
						}
					else
						{
							premios	= str2dbl(this.CALCULO_PREMIOS(persona));
						}
				/* fin del calculo de Premios */
					
				/* inicio del calculo de adicionales */
					double adicionales ;
					if (str2dbl(Calculos[0].ToString()) == 0)
						{
							adicionales = 0;
						}
					else
						{
							adicionales	= str2dbl(this.CALCULO_ADICIONALES(persona));
						}
				/* fin del calculo de adicionales */
									
				
				/* inicio calculo sueldo */
					double sueldo ;
					if (str2dbl(Calculos[0].ToString()) == 0)
						{
							sueldo = 0;
						}
					else
						{
							sueldo	= str2dbl(Calculos[0].ToString()) - (sac + horasExtras + Vacaciones + adicionales + premios );
						}
				/* fin calculo sueldo */


/* sueldo_adic orden = 36 */
					columna = dbl2str(sueldo);
/*------*/ColumnasSIJP.Add(LFill(columna,9,'0'));
count = count + 1;
//Console.WriteLine("se acaba de incorporar la columna " + count);								
				
/* SAC orden = 37 */
					columna = dbl2str(sac);
/*------*/ColumnasSIJP.Add(LFill(columna,9,'0'));
count = count + 1;
//Console.WriteLine("se acaba de incorporar la columna " + count);				

/* HorasExtras orden = 38 */
					columna = dbl2str(horasExtras);
/*------*/ColumnasSIJP.Add(LFill(columna,9,'0'));
count = count + 1;
//Console.WriteLine("se acaba de incorporar la columna " + count);				

/*zona_def orden = 39 */
					columna = "";
/*------*/ColumnasSIJP.Add(LFill(columna,9,'0'));
count = count + 1;
//Console.WriteLine("se acaba de incorporar la columna " + count);				


/* Vacaciones orden = 40 */
					columna = dbl2str(Vacaciones);
/*------*/ColumnasSIJP.Add(LFill(columna,9,'0'));
count = count + 1;
//Console.WriteLine("se acaba de incorporar la columna " + count);

/* CANTIDAD DE DIAS TRABAJADOS orden = 41 */
					columna = this.POLITICA_CANTIDAD_DIAS_TRABAJADOS(persona, sueldo, horasExtras, Vacaciones);
/*------*/ColumnasSIJP.Add(LFill(columna,9,'0'));
count = count + 1;
//Console.WriteLine("se acaba de incorporar la columna " + count);


/*remun_imp5 orden = 42 */
					columna = "";
/*------*/ColumnasSIJP.Add(LFill(columna,9,'0'));
count = count + 1;
//Console.WriteLine("se acaba de incorporar la columna " + count);				

/*trab_convencionado orden = 43 */
					/* AQUI AGREGO EL CODIGO DE trab_convencionado- USANDO UNA FUNCION */
					columna = this.devolverCodigoParametro(persona, "trab_convencionado_SIJP");
					if (columna == "APLICAR_POLITICA")
						{
							columna = "1";
						}
/*------*/ColumnasSIJP.Add(LFill(columna,1,'0'));
count = count + 1;
//Console.WriteLine("se acaba de incorporar la columna " + count);

/* remun_imp6 orden = 44 */
					columna23 = (double)Calculos[4];
					columna = dbl2str(columna23);
/*------*/ColumnasSIJP.Add(LFill(columna,9,'0'));
count = count + 1;
//Console.WriteLine("se acaba de incorporar la columna " + count);


/* tip_opera orden = 45 */
					/* AQUI AGREGO EL CODIGO DE tip_opera - USANDO UNA FUNCION */
					columna = this.devolverCodigoParametro(persona, "tip_opera_SIJP");
					if (columna == "APLICAR_POLITICA")
						{
							columna = "0";
						}
/*------*/ColumnasSIJP.Add(LFill(columna,1,'0'));							
count = count + 1;
//Console.WriteLine("se acaba de incorporar la columna " + count);



/* adicionales orden = 46 */
					columna = dbl2str(adicionales);
/*------*/ColumnasSIJP.Add(LFill(columna,9,'0'));
count = count + 1;
//Console.WriteLine("se acaba de incorporar la columna " + count);				


/* premios orden = 47 */
					columna = dbl2str(premios);
/*------*/ColumnasSIJP.Add(LFill(columna,9,'0'));
count = count + 1;
//Console.WriteLine("se acaba de incorporar la columna " + count);				


/* Decreto 788/05 orden = 48 */
					/* AQUI AGREGO EL CODIGO DE Decreto 788/05 - USANDO UNA FUNCION */
					columna = this.devolverCodigoParametro(persona, "Decreto78805_SIJP");
					if (columna == "APLICAR_POLITICA")
						{
							columna = "000000000";
						}
/*------*/ColumnasSIJP.Add(LFill(columna,9,'0'));			
count = count + 1;
//Console.WriteLine("se acaba de incorporar la columna " + count);

/* Remun_imp_7 orden = 49 */
					/* AQUI AGREGO EL CODIGO DE Remun_imp_7 - USANDO UNA FUNCION */
					columna = this.devolverCodigoParametro(persona, "Remun_imp_7_SIJP");
					if (columna == "APLICAR_POLITICA")
						{
							columna = "000000000";
						}
/*------*/ColumnasSIJP.Add(LFill(columna,9,'0'));			
count = count + 1;
//Console.WriteLine("se acaba de incorporar la columna " + count);

					/* aqui voy a agregar todos los datos recien calculados al documento que voy a poner en la salida */
					int contador;
					string salidaPer = "";
					for (contador=0; contador < ColumnasSIJP.Count; contador ++)
						{
							salidaPer = salidaPer + ColumnasSIJP[contador];
						}
					//Console.WriteLine("se lindo " + salidaPer );
					
					DocSalidaPersonas.AddChildDocument("<PERSONA valor= \"" + salidaPer + "\"/>");
					//Console.WriteLine("se lindo " + DocSalidaPersonas);
				}

				DocSalidaPersonasValidas.AddChildDocument(DocSalidaPersonas.ToString());
				Console.WriteLine("Mensaje: DocSalidaPersonasValidasjj " + DocSalidaPersonasValidas.ToString());
				
				Nomad.NSystem.Document.NmdXmlDocument DocSalida = new Nomad.NSystem.Document.NmdXmlDocument("<DATOS/>");	  	
				DocSalida.AddChildDocument(DocSalidaPersonasValidas.ToString());
				
//			ArraySalidaSIJP.Add(DocSalidaPersonas);
			
			Nomad.NSystem.Document.NmdXmlDocument personasDocINV = (Nomad.NSystem.Document.NmdXmlDocument)PERSONAS_VALIDAS.GetDocumentByName("PERSONAS_NO_VALIDAS");
			if ((Nomad.NSystem.Document.NmdXmlDocument)personasDocINV.GetFirstChildDocument() != null)
				{
					
					Nomad.NSystem.Document.NmdXmlDocument DocSalidaPersonasINV = new Nomad.NSystem.Document.NmdXmlDocument("<PERSONAS_INVALIDAS sufix=\"" + PERSONAS_VALIDAS.GetAttribute("periodo").Value + "Errores.txt\"/>");
					Nomad.NSystem.Document.NmdXmlDocument DocSalidaPersonasINVALIDAS = new Nomad.NSystem.Document.NmdXmlDocument("<PERSONAS/>");
					Nomad.NSystem.Document.NmdXmlDocument personasINV;
					for (personasINV=(Nomad.NSystem.Document.NmdXmlDocument)personasDocINV.GetFirstChildDocument(); personasINV!=null; personasINV=(Nomad.NSystem.Document.NmdXmlDocument)personasDocINV.GetNextChildDocument())
						{
							
							DocSalidaPersonasINVALIDAS.AddChildDocument("<PERSONA c_liquiadcion= \"" + personasINV.GetAttribute("c_liquidacion") + "\" d_liquidacion = \"" +  personasINV.GetAttribute("d_liquidacion") + "\" c_empresa = \"" +  personasINV.GetAttribute("c_empresa") + "\" d_empresa = \"" + personasINV.GetAttribute("d_empresa") + "\" e_numero_legajo = \"" + personasINV.GetAttribute("e_numero_legajo") + "\" d_ape_y_nom = \"" + personasINV.GetAttribute("d_ape_y_nom")  + "\"/>");
							
						}
					DocSalidaPersonasINV.AddChildDocument(DocSalidaPersonasINVALIDAS.ToString());

				
					DocSalida.AddChildDocument(DocSalidaPersonasINV.ToString());
					
				}
			
			return DocSalida;
			
		}
		
		


		public string POLITICA_CANTIDAD_DIAS_TRABAJADOS(Nomad.NSystem.Document.NmdXmlDocument persona, double sueldo, double horasExtras, double vacaciones)
			{
				int devolver = 9;
				
				if (persona.GetAttribute("f_ingreso")!=null)
					{
						int fechaIng = int.Parse(persona.GetAttribute("f_ingreso").Value);
						int iniPeriodo = int.Parse(persona.GetAttribute("periodo").Value + "01");
						int periodo = int.Parse(persona.GetAttribute("periodo").Value);
						DateTime fechaFor = str2date(persona.GetAttribute("periodo").Value + "01");
						fechaFor = addMonths(fechaFor,1);
						//Console.WriteLine("hola ing " + persona.GetAttribute("f_ingreso").Value);
						
						int finPeriodo = int.Parse((year(fechaFor).ToString())+ (LFill(month(fechaFor).ToString(),2,'0')) +  (LFill(day(fechaFor).ToString(),2,'0')) );
						if (fechaIng > iniPeriodo)
							{
								devolver = 0;
								//Console.WriteLine("izquierda 1");
							}
						if (persona.GetAttribute("f_egreso")!=null)
							{
								//Console.WriteLine("izquierda 2");
								int fechaEgreso = int.Parse(persona.GetAttribute("f_egreso").Value);
								if (fechaEgreso < iniPeriodo)
									{
									//	Console.WriteLine("izquierda 3");
										devolver = 0;
									}
								if (fechaEgreso < fechaIng)
									{
									//	Console.WriteLine("izquierda 4");
										devolver = 0;
									}
							}
						if (devolver == 0)
							{
								//Console.WriteLine("izquierda 6");
								if ((horasExtras!= 0) || (sueldo!=0) || (vacaciones!=0))
									{
										//Console.WriteLine("izquierda 7");
										devolver = 1;
									}
							}else
								{
									int F1; 
									int ultimoDia = 0;
									//Console.WriteLine("izquierda 8");
									if (persona.GetAttribute("f_egreso")!=null)
										{
											Console.WriteLine("izquierda 9");
											F1 = int.Parse(persona.GetAttribute("f_egreso").Value);
											if (F1 >= finPeriodo)
												{
													Console.WriteLine("izquierda 10");
													F1 = finPeriodo;
												}else
													{
														Console.WriteLine("izquierda 11");
														ultimoDia = 1;
													}
										}else
											{
												//Console.WriteLine("izquierda 12" + finPeriodo);
												F1 = finPeriodo;
											}
									int F2;
									if (fechaIng > iniPeriodo)
										{
											//Console.WriteLine("izquierda 13");
											F2 = fechaIng;
										}else
											{
												//Console.WriteLine("izquierda 14" + iniPeriodo);
												F2 = iniPeriodo;
											}
									
									devolver = diffDays(str2date(F2.ToString()),str2date(F1.ToString())) + ultimoDia;
										
								}
					}else
						{
							devolver = 0;
						}
				
				return devolver.ToString();
			}


		public string RFill(string cadena, int longitud, char caracter)
			{
				
						if (longitud > 0)
							{ 
								string temp = cadena;
								if (cadena.Length < longitud)
									{
										int ii;
										for (ii=cadena.Length; ii < longitud; ii++)
											{
												temp = temp + caracter;
											}
									}else
										{
											if (cadena.Length > longitud)
												{
													Proxy.Batch().Trace.Add("ifo", "Mal uso de la Funcion RFill", "InterfazSIJP");
													throw new NomadAppException("Mal uso de la Funcion RFill ");
												}
										}
								return temp;
							}else
								{
									Proxy.Batch().Trace.Add("ifo", "Mal uso de la Funcion RFill", "InterfazSIJP");
									throw new NomadAppException("Mal uso de la Funcion RFill");					
								}
				
			}
		
		public string LFill(string cadena, int longitud, char caracter)
			{
				
						if (longitud > 0)
							{ 
								string temp = cadena;
								if (cadena.Length < longitud)
									{
										int ii;
										for (ii=cadena.Length; ii < longitud; ii++)
											{
												temp = caracter + temp;
											}
									}else
										{
											if (cadena.Length > longitud)
												{
													Proxy.Batch().Trace.Add("ifo", "Mal uso de la Funcion LFill", "InterfazSIJP");
													throw new NomadAppException("Mal uso de la Funcion LFill ");
												}
										}
								//Console.WriteLine("hola 1" + temp);							
								return temp;
							}else
								{
									Proxy.Batch().Trace.Add("ifo", "Mal uso de la Funcion LFill", "InterfazSIJP");
									throw new NomadAppException("Mal uso de la Funcion LFill");					
								}
				
			}			
		
		public string CALCULO_ADICIONALES(Nomad.NSystem.Document.NmdXmlDocument persona)
			{
				string docXML ="";
				docXML = "<DATOS><PERSONA OI_PERSONAL_EMP = \"" + persona.GetAttribute("OI_PERSONAL_EMP").Value +"\" FUNCION=\"CALCULO_ADICIONALES\"/></DATOS>";				
				                                                                                                                                          //3 - A CTA FUTUROS AUMENTOS
				                                                                                                                                          //5 - LICENCIA POR ENFERMEDAD
				                                                                                                                                          //7 - LICENCIA POR NACIMIENTO
				                                                                                                                                          //8 - LICENCIA POR MATRIMONIO
				                                                                                                                                          //9 - LICENCIA POR FALLECIMIENTO
				                                                                                                                                          //11 - LICENCIA POR FALLECIMIENTO
				                                                                                                                                          //12 - DIA DEL GREMIO GOZADO
//YA NO VA ESTO PORQUE NO SE BANCA INTEGER 
//<CONCEPTOS><CONCEPTO ID=\"3\"/><CONCEPTO ID=\"5\"/><CONCEPTO ID=\"7\"/><CONCEPTO ID=\"8\"/><CONCEPTO ID=\"9\"/><CONCEPTO ID=\"11\"/><CONCEPTO ID=\"12\"/></CONCEPTOS>

				Nomad.NSystem.Document.NmdXmlDocument valor = new Nomad.NSystem.Document.NmdXmlDocument(Proxy.SQLService().Get(this.GetObjectDocument().GetResource("QRY_CONCEPTOS_SIJP"),docXML));
															
				
				return valor.GetAttribute("SUMA").Value;
			}


		public string CALCULO_PREMIOS(Nomad.NSystem.Document.NmdXmlDocument persona)
			{
				string docXML ="";
				docXML = "<DATOS><PERSONA OI_PERSONAL_EMP = \"" + persona.GetAttribute("OI_PERSONAL_EMP").Value +"\" FUNCION=\"CALCULO_PREMIOS\"/></DATOS>";
					                                                                                                                               //104 Premio Estímulo
					                                                                                                                               //105 Asistencia y Puntualidad
// YA NO VA ESTO PORQUE NO SE BANCA INTEGER 
///><CONCEPTOS><CONCEPTO ID=\"104\"/><CONCEPTO ID=\"105\"/></CONCEPTOS></DATOS>";					                                                                                                                                   
				Nomad.NSystem.Document.NmdXmlDocument valor = new Nomad.NSystem.Document.NmdXmlDocument(Proxy.SQLService().Get(this.GetObjectDocument().GetResource("QRY_CONCEPTOS_SIJP"),docXML));										
				
				return valor.GetAttribute("SUMA").Value;
			}
		
				
		public string CALCULO_HORAS_EXTRAS(Nomad.NSystem.Document.NmdXmlDocument persona)
			{
				string docXML ="";
				docXML = "<DATOS><PERSONA OI_PERSONAL_EMP = \"" + persona.GetAttribute("OI_PERSONAL_EMP").Value +"\" FUNCION=\"CALCULO_HORAS_EXTRAS\"/></DATOS>";
																												//21 y 22 son horas extras a 50% y 100%
				//YA NO VA ESTO PORQUE NO SE BANCA INTEGER
				//+"\"/><CONCEPTOS><CONCEPTO ID=\"22\"/><CONCEPTO ID=\"21\"/></CONCEPTOS></DATOS>";
				//Console.WriteLine("hola " + docXML);							
				Nomad.NSystem.Document.NmdXmlDocument valor = new Nomad.NSystem.Document.NmdXmlDocument(Proxy.SQLService().Get(this.GetObjectDocument().GetResource("QRY_CONCEPTOS_SIJP"),docXML));										
				
				return valor.GetAttribute("SUMA").Value;
			}
		
		public string POLITICA_APORTE_ADIC_OS(Nomad.NSystem.Document.NmdXmlDocument persona)
			{
				int CONTADOR = 0;
				Nomad.NSystem.Document.NmdXmlDocument FamDoc = (Nomad.NSystem.Document.NmdXmlDocument)persona.GetDocumentByName("FAMILIARES"); 
						
				Nomad.NSystem.Document.NmdXmlDocument cadaFam; 
				for (cadaFam = (Nomad.NSystem.Document.NmdXmlDocument)FamDoc.GetFirstChildDocument(); cadaFam!=null; cadaFam = (Nomad.NSystem.Document.NmdXmlDocument)FamDoc.GetNextChildDocument()) //me fijo en cada familiar
					{
					if (cadaFam.GetAttribute("l_acargo_af").Value == "1")
						{	
							CONTADOR = CONTADOR + 1;
						}
					}
				double suma;
				if (this.devolverCodigoParametroDefecto("sumaAporteAdicOsSIJP")!="APLICAR_POLITICA")
					{
						suma = str2dbl(this.devolverCodigoParametroDefecto("sumaAporteAdicOsSIJP"));
					}else
						{
							suma = 0;
						}
					
						
				return dbl2str((suma * CONTADOR));
			}
				
		public string POLITICA_MODALIDAD_CONTRATACION(Nomad.NSystem.Document.NmdXmlDocument persona)
			{
				string respuesta;
				if (persona.GetAttribute("C_TIPO_PERSONAL")!= null)
					{//esto es si tiene un tipo de personal definido
						if (persona.GetAttribute("C_TIPO_PERSONAL").Value!= "")
							{//esto es si tiene un tipo de personal definido
								if (persona.GetAttribute("C_TIPO_PERSONAL").Value == "8")
									{ //[1] esto es, si la persona es pasante 
										respuesta = this.devolverCodigoParametroDefecto("modContSIJPpasante");
										if (respuesta != "APLICAR_POLITICA")
											{//esto es si esta definido un valor para el parametro del modo de contratacion de pasantes del sijp
												return respuesta; 
											}else
												{//si no tiene definido el codigo de pasante.
													Proxy.Batch().Trace.Add("ifo", "falta definir el parametro modContSIJPpasante", "InterfazSIJP");
													throw new NomadAppException("falta definir el parametro modContSIJPpasante");
												}
									}else
										{//[2] si no es pasante 
											string fec_ini = this.devolverCodigoParametroDefecto("modContSIJPfechaIni");
											
											if (fec_ini != "APLICAR_POLITICA")
												{//[3]si el parametro esta definido y la fecha de ingreso de la persona es menor que dicho valor							
													int temp_Fec_ini = int.Parse(fec_ini);
													if (int.Parse(persona.GetAttribute("f_ingreso").Value) < temp_Fec_ini)
														{
															return  this.devolverCodigoParametroDefecto("modContSIJPporFecha");
														}else
															{//[4] el modo de contratacion depende de la edad
																string f_nacim;
																string periodo;
																int edad;
																string edadMinima;
																string edadMaxima;
																string modo;
																if (persona.GetAttribute("f_nacim")!= null)
																	{
																		f_nacim = persona.GetAttribute("f_nacim").Value;
																		periodo = persona.GetAttribute("periodo").Value + 01;
																		edad = diffYears(str2date(f_nacim), date());
																		edadMinima = this.devolverCodigoParametroDefecto("modContSIJPEdadMinima");
																		if (edadMinima == "APLICAR_POLITICA")
																			{
																				Proxy.Batch().Trace.Add("ifo", "falta definir el parametro modContSIJPEdadMinima", "InterfazSIJP");
																				throw new NomadAppException("falta definir el parametro modContSIJPEdadMinima");
																			}
																		edadMaxima = this.devolverCodigoParametroDefecto("modContSIJPEdadMaxima");
																		if (edadMaxima == "APLICAR_POLITICA")
																			{
																				Proxy.Batch().Trace.Add("ifo", "falta definir el parametro modContSIJPEdadMaxima", "InterfazSIJP");
																				throw new NomadAppException("falta definir el parametro modContSIJPEdadMaxima");
																			}
																		
																		if ((edad >= int.Parse(edadMinima)) && (edad <= int.Parse(edadMaxima)))
																			{
																				modo = this.devolverCodigoParametroDefecto("modContSIJPEdad");
																				if (modo == "APLICAR_POLITICA")
																					{
																						Proxy.Batch().Trace.Add("ifo", "falta definir el parametro modContSIJPEdad", "InterfazSIJP");
																						throw new NomadAppException("falta definir el parametro modContSIJPEdad");
																					}
																			}else
																				{
																					modo = this.devolverCodigoParametroDefecto("modContSIJPEdadDefecto");
																					if (modo == "APLICAR_POLITICA")
																						{
																							Proxy.Batch().Trace.Add("ifo", "falta definir el parametro modContSIJPEdadDefecto", "InterfazSIJP");
																							throw new NomadAppException("falta definir el parametro modContSIJPEdadDefecto");
																						}																					
																				}
 																	}else
																		{
																			modo = this.devolverCodigoParametroDefecto("modContSIJPFechaNacimDefecto");
																			if (modo == "APLICAR_POLITICA")
																				{
																					Proxy.Batch().Trace.Add("ifo", "falta definir el parametro modContSIJPFechaNacimDefecto", "InterfazSIJP");
																					throw new NomadAppException("falta definir el parametro modContSIJPFechaNacimDefecto");				
																				}
																		}
															return modo;
															}
												}
											
											
										}
							}
					}
				
				Proxy.Batch().Trace.Add("ifo", "la Persona no tiene definido un tipo de personal", "InterfazSIJP");
				throw new NomadAppException("la Persona no tiene definido un tipo de personal");				
			}
			
		public string POLITICA_ACTIVIDAD_EMPRESA(Nomad.NSystem.Document.NmdXmlDocument persona)
		{
			if (persona.GetAttribute("c_actividad")!= null)
				{
					return persona.GetAttribute("c_actividad").Value;
				}else
					{
						Proxy.Batch().Trace.Add("ifo", "la empresa no tiene una actividad definida", "InterfazSIJP");
						throw new NomadAppException("la empresa no tiene una actividad definida");				
					}
		}				
		
	
		public string POLITICA_ZONA_SIJP(Nomad.NSystem.Document.NmdXmlDocument persona)
		{
			if (persona.GetAttribute("c_ZONA_SIJP")!= null)
				{
					return persona.GetAttribute("c_ZONA_SIJP").Value;
				}else
					{
						Proxy.Batch().Trace.Add("ifo", "Alguna de las personas no tiene una ubicacion defina con una Zona SIJP valida", "InterfazSIJP");
						throw new NomadAppException("Alguna de las personas no tiene una ubicacion defina con una Zona SIJP valida");				
					}
		}
		
		
		public ArrayList ArmarArregloSituaciones(Nomad.NSystem.Document.NmdXmlDocument persona)
			{
			/*esta funcion arma el arreglo de situaciones de la persona */
			
			ArrayList ArraySituaciones = new ArrayList(); //Aqui voy a guardar el codigo SIJP de la situacion
			ArrayList ArraySituacionesFecha = new ArrayList(); //Aqui voy a guardar la fecha de la situacion
			ArrayList ArraySituacionesCodigos = new ArrayList(); //Aqui voy a guardar el codigo real de la situacion
			
			int per = int.Parse(persona.GetAttribute("periodo").Value);
			
			/* desde aqui */					
			/* desde aqui */
			/* desde aqui */
			string cod_egreso; 					
			if (persona.GetAttribute("f_egreso")!=null)
				{//si tiene fecha de egreso
					string fecha_egreso = persona.GetAttribute("f_egreso").Value;
					if (fecha_egreso != "")
						{//si tiene fecha de egreso
							int fec = int.Parse(fecha_egreso.Substring(0,6));
							
							if (fec < per)
							 {//si la fecha de egreso es menor a la fecha de inicio del periodo
							 	cod_egreso = "noHay";	
							 }else
							 	{
							 	cod_egreso = "siHay";
							 	}
						}else
						 	{
						 	cod_egreso = "siHay";
						 	}
				}else
				 	{
				 	cod_egreso = "siHay";
				 	}
			/*	 	
			if (cod_egreso == "noHay")
				{
					cod_egreso = this.devolverCodigoParametro(persona, "arrSituaNoFecEgreso");
					//aqui agrego el codigo del parametro para una persona que no tiene egreso
					ArraySituaciones.Add(cod_egreso);
					ArraySituacionesCodigos.Add("Egreso");
					string tmpIngreso = persona.GetAttribute("f_ingreso").Value;
					string tmpIngresoDia = persona.GetAttribute("f_ingreso").Value;
					int periodoIngreso = int.Parse(tmpIngreso.Substring(0,6));
					int diaIngreso = int.Parse(tmpIngresoDia.Substring(6,2));
							if (per > periodoIngreso)
							{//si ingreso antes del periodo
								ArraySituacionesFecha.Add(persona.GetAttribute("periodo").Value + "01");
							}else
								{
									ArraySituacionesFecha.Add(persona.GetAttribute("f_ingreso").Value);
								}
				}		lo saco por cambios a apartir de la version 26*/
	 	
			/* hasta aqui */
			/* hasta aqui */
			/* hasta aqui es para la primera situacion, si tiene. esto es, 
			si no tiene definido el egreso o si el egreso es mayor al primer 
			dia del periodo se agrega esto, si no no. */
			
			
			/* desde aqui */					
			/* desde aqui */
			/* desde aqui */
			Nomad.NSystem.Document.NmdXmlDocument Licencias = (Nomad.NSystem.Document.NmdXmlDocument)persona.GetDocumentByName("LICENCIAS"); 
			//en Licencias guardo todas las licencias que tiene la persona
						
			Nomad.NSystem.Document.NmdXmlDocument cadaLic;
			for (cadaLic=(Nomad.NSystem.Document.NmdXmlDocument)Licencias.GetFirstChildDocument(); cadaLic!=null; cadaLic=(Nomad.NSystem.Document.NmdXmlDocument)Licencias.GetNextChildDocument())
				{
					if ((cadaLic.GetAttribute("c_licencia").Value == "M") || (cadaLic.GetAttribute("c_licencia").Value == "L") || (cadaLic.GetAttribute("c_licencia").Value == "E") || (cadaLic.GetAttribute("c_licencia").Value == "R") || (cadaLic.GetAttribute("c_licencia").Value == "V"))
							{
								ArraySituaciones.Add(cadaLic.GetAttribute("c_sit_esp_sijp").Value);
								ArraySituacionesCodigos.Add(cadaLic.GetAttribute("c_licencia").Value);
								string tmpIngreso = cadaLic.GetAttribute("f_inicio").Value;
								int periodoIngreso = int.Parse(tmpIngreso.Substring(0,6));
								if (per > periodoIngreso)
									{//si la fecha de inicio es menor al inicio del periodo 
										ArraySituacionesFecha.Add(persona.GetAttribute("periodo").Value + "01");
									}else
										{
											ArraySituacionesFecha.Add(cadaLic.GetAttribute("f_inicio").Value);
										}
							
								if (cadaLic.GetAttribute("f_fin")!=null)
									{
										string tmpFin = cadaLic.GetAttribute("f_fin").Value;
										int periodoFin = int.Parse(tmpFin.Substring(0,6));
										int fecFin = int.Parse(tmpFin) + 1;
										
										if (per == periodoFin)
											{
												ArraySituacionesCodigos.Add(cadaLic.GetAttribute("c_licencia").Value);
												ArraySituaciones.Add("01");
												ArraySituacionesFecha.Add(fecFin.ToString());
											}
											
									}
							}
				}
			/* hasta aqui */
			/* hasta aqui */
			/* se agrega tantos registros como licencias tenga cargado la persona, 
			siempre y cuando sean del tipo: MATERNIDAD (C=M); LICENCIA SIN GOCE DE SUELDO (C=l);
			EXCEDENCIA (C=E); RESERVA PUESTO (C=R); VACACIONES (C=V). 
			se agregara un registro con el codigo sijp y otro con
			la fecha de inicio de la licencia si esta es mayor que la fecha de inicio del periodo
			o con la fecha de inicio del periodo si la fecha de inicio del mismo es menor a la del periodo.  */
			
			
			/* desde aqui */
			/* desde aqui */
			/* desde aqui */
			int cantidad = ArraySituacionesFecha.Count; //Lenght
			bool pregunta = true;
			while (pregunta)
				{
				
				if (cantidad > 0)
					{
						int ii;
						int jj;
						string temp1;
						for (ii=0; ii <= cantidad-2; ii++)
							{
								for (jj=ii+1; jj <= cantidad-1; jj++)
									{
										if (int.Parse(ArraySituacionesFecha[ii].ToString()) > int.Parse(ArraySituacionesFecha[jj].ToString()) )
											{
												temp1 = ArraySituacionesFecha[ii].ToString();
												ArraySituacionesFecha[ii] = ArraySituacionesFecha[jj];
												ArraySituacionesFecha[jj] = temp1;
	
												temp1 = ArraySituaciones[ii].ToString();
												ArraySituaciones[ii] = ArraySituaciones[jj];
												ArraySituaciones[jj] = temp1;
	
												temp1 = ArraySituacionesCodigos[ii].ToString();
												ArraySituacionesCodigos[ii] = ArraySituacionesCodigos[jj];
												ArraySituacionesCodigos[jj] = temp1;											
												
											}
									}
								
							}
						// EN ESTE PUNTO YA ESTAN ORDENADOS POR FECHA 
						// AHORA TENDRIA QUE FIJARSE SI HAY DOS FECHAS IGUALES.
						int cont = 0;
						for (ii=1; ii <= cantidad-1; ii++)
							{
								if (ArraySituacionesFecha[ii] == ArraySituacionesFecha[ii-1])
								{
									if (ArraySituaciones[ii].ToString() == "01")
										{
									//esto lo que hace es que si tengo tres situaciones con la misma fecha
									//y la del medio es activo, entonces la elimino y vuelvo a ejecutar la ordenacion.
											ArraySituacionesFecha[ii] = ArraySituacionesFecha[cantidad - 1];
											ArraySituacionesFecha[cantidad - 1] = "";

											ArraySituaciones[ii] = ArraySituaciones[cantidad - 1];
											ArraySituaciones[cantidad - 1] = "";

											ArraySituacionesCodigos[ii] = ArraySituacionesCodigos[cantidad - 1];
											ArraySituacionesCodigos[cantidad - 1] = "";											
											cantidad = cantidad - 1;
											break;
										}else
											{// si no es activo algo fallo
												Proxy.Batch().Trace.Add("ifo", "El arreglo de situaciones se armo mal", "InterfazSIJP");
												throw new NomadAppException("El arreglo de situaciones se armo mal");				
											}
								}else
									{
										cont = cont + 1;
									}
							}
							if (cont == (cantidad - 1))
								{
									pregunta = false;
								}
					}else
						pregunta = false;
				}
			/* hasta aqui */
			/* hasta aqui */
			/* hasta aqui */
			/* esto tendria que ordenar el array por fecha */ 
			
			
			/* desde aqui */ 
			/* desde aqui */ 
			/* desde aqui */ 



			/* hasta aqui */
			/* hasta aqui */
			/* hasta aqui */
			/* debo agarrar el arreglo y fijarme si dos o mas situaciones tienen la misma fecha, si esto es asi, lo que debe hacerse es elegir una y agregarle un dia.  */ 
			
			
			if (ArraySituaciones.Count < 1)
			{
				//Console.WriteLine("NACHO TE ODIO ahhhhhhhhh ");
				if (cod_egreso == "siHay")
					{
						//Console.WriteLine("NACHO TE ODIO ahhhhhhhhh NACHO TE ODIO ahhhhhhhhhNACHO TE ODIO ahhhhhhhhh");
						string cod_situa;
						cod_situa = "1";//this.devolverCodigoParametro(persona, "arrSituaActivo");
						/* aqui agrego el codigo del parametro para una persona que no tiene egreso */
						ArraySituaciones.Add(cod_situa);
						ArraySituacionesCodigos.Add("Activo");
						ArraySituacionesFecha.Add(per + "01");
						cantidad = 1;
					}//este codigo sirve para cuando no hay ninguna situacion.									
			}
			
			/* desde aqui */ 
			/* desde aqui */ 
			/* desde aqui */ 
			ArrayList ArraySalida = new ArrayList(); /* en este voy a guardar tres registros por licencia, 
			el primero sera el codigo sijp de la situacion, el segundo la fecha, y el tercero el codigo 
			real de la misma. asi si yo quiero ver la primera situacion voy a tener 
			ArraySalida[0]; ArraySalida[1]; ArraySalida[2] */
			if (cantidad > 0)
				{
					int ii;
					for (ii=0; ii <= cantidad-1; ii++)
						{
							//Console.WriteLine("NACHO TE ODIO " + ArraySituaciones[ii].ToString());
							//Console.WriteLine("NACHO TE ODIO MAS " + ArraySituacionesFecha[ii].ToString());
							//Console.WriteLine("NACHO TE ODIO MUCHO MAS " + ArraySituacionesCodigos[ii].ToString());
							ArraySalida.Add(ArraySituaciones[ii].ToString());
							ArraySalida.Add(ArraySituacionesFecha[ii].ToString());
							ArraySalida.Add(ArraySituacionesCodigos[ii].ToString());
							
						}
				}
			return ArraySalida;


			/* hasta aqui */
			/* hasta aqui */
			/* hasta aqui */
			/* para armar el arreglo para devolvelo */
			}
		
		
		public string POLITICA_SITUACION(Nomad.NSystem.Document.NmdXmlDocument persona, ArrayList ArraySitua )
			{
			if (ArraySitua.Count != 0 )
				{//[1] si hay alguna licencia registrada en el arreglo de situaciones el codigo de situacion tomara el valor del primer registro de esta.
					//Console.WriteLine("NACHO TE ODIO mucho mucho mucho" + ArraySitua[0].ToString());
					return ArraySitua[0].ToString();
				}else
					{
						if (persona.GetAttribute("c_sijp")!= null)
							{//[2]si tiene un motivo de egreso definido y este tiene el codigo de SIJP definido entonces se tomara como codigo de situacion este codigo
								return persona.GetAttribute("c_sijp").Value;
							}else
								{//[3] si no pasa ninguno de estos casos se toma el valor por defecto. si este esta definido se tomara el mismo de la tabla de parametro. si no es "0".
								string tmpDefecto = "";
								tmpDefecto = devolverCodigoParametroDefecto("codigoSituacionDefectoSIJP");
								if (tmpDefecto == "APLICAR_POLITICA")
									{//cuando no tiene definido un valor por defecto
										return "00";
									}else
										{//cuando tiene definido un valor por defecto
											return tmpDefecto;
										}
								}
							
					}
				
				
			}
	
	
		public string POLITICA_REGIMEN(Nomad.NSystem.Document.NmdXmlDocument persona)
			{
				if (persona.GetAttribute("c_afjp")!=null)
					{
						if (persona.GetAttribute("c_afjp").Value == "00")
							{
								return "0"; //Este seria el codigo para reparto
							}else
								{
									return "1"; //Este seria el codigo para Capitalizacion
								}
					}else
						{
							string oo = this.devolverCodigoParametroDefecto("CodRegimenDefectoSIJP");
							if (oo == "APLICAR_POLITICA")
								{
									return "0";
								}else
									{
										return oo;
									}
						}
			}
	
		public string CantidadHijos(Nomad.NSystem.Document.NmdXmlDocument persona)
			{
			/* funcion cuenta la cantidad de hijos e hijas que tiene la persona */ 
						int preguntaTemp = 0;
						Nomad.NSystem.Document.NmdXmlDocument FamDoc = (Nomad.NSystem.Document.NmdXmlDocument)persona.GetDocumentByName("FAMILIARES"); 
						
						Nomad.NSystem.Document.NmdXmlDocument cadaFam; 
						for (cadaFam = (Nomad.NSystem.Document.NmdXmlDocument)FamDoc.GetFirstChildDocument(); cadaFam!=null; cadaFam = (Nomad.NSystem.Document.NmdXmlDocument)FamDoc.GetNextChildDocument()) //me fijo en cada familiar
							{
								if (cadaFam.GetAttribute("l_acargo_af").Value == "1")
									{
										if ((cadaFam.GetAttribute("c_tipo_familiar").Value == "2") || (cadaFam.GetAttribute("c_tipo_familiar").Value == "3"))
											{ 
												/* este if sirve para saber si la persona tiene cargado algun parametro en particular */					
												preguntaTemp = preguntaTemp + 1;
											}
									}
							}

						//Console.WriteLine("cantidad de hijos" + preguntaTemp.ToString());							
						return preguntaTemp.ToString(); /* este es el codigo definido por el AFIP para el codigo de que si tiene conyugue */

				
			}
	
		public string POLITICA_CONYUGUE(Nomad.NSystem.Document.NmdXmlDocument persona)
			{
			/* esta politica conciste en fijarse los familiares que tiene cargada la persona, si tiene alguno del tipo conyugue coloco el codigo definido para conyugue, si no coloco el codigo definido para soltero */ 
						int preguntaTemp = 0;
						Nomad.NSystem.Document.NmdXmlDocument FamDoc = (Nomad.NSystem.Document.NmdXmlDocument)persona.GetDocumentByName("FAMILIARES"); 
						
						Nomad.NSystem.Document.NmdXmlDocument cadaFam; 
						for (cadaFam = (Nomad.NSystem.Document.NmdXmlDocument)FamDoc.GetFirstChildDocument(); cadaFam!=null; cadaFam = (Nomad.NSystem.Document.NmdXmlDocument)FamDoc.GetNextChildDocument()) //me fijo en cada familiar
							{
								if (cadaFam.GetAttribute("c_tipo_familiar").Value == "1")
									{ 
										/* este if sirve para saber si la persona tiene cargado algun parametro en particular */					
										preguntaTemp = 1;
									}
							}

						if (preguntaTemp == 1)
							{
								return "1"; /* este es el codigo definido por el AFIP para el codigo de que si tiene conyugue */
							}else
								{
									return "0"; /* este es el codigo definido por el AFIP para el codigo de que no tiene conyugue */
								}

				
			}
	
	
		public string POLITICA_JUBILADO(Nomad.NSystem.Document.NmdXmlDocument persona)
			{
			/* esta politica conciste en fijarse SI LA PERSONA ESTA JUBILADO */ 
				if (persona.GetAttribute("L_JUBILADO")!= null)
					{
						if (persona.GetAttribute("L_JUBILADO").Value == "1")
							{
								return "02"; /* este es el codigo definido por el AFIP para el codigo de que si tiene jubilacion */
							}else
								{
									return "01"; /* este es el codigo definido por el AFIP para el codigo de que no tiene jubilacion */
								}
					}else
						{
							Proxy.Batch().Trace.Add("ifo", "Alguna de las Personas no tiene definido su regimen de jubilacion", "InterfazSIJP");
							throw new NomadAppException("Alguna de las Personas no tiene definido su regimen de jubilacion");				
						}
			}
	
	
	
	
	
	
	
	
	
	
	
	

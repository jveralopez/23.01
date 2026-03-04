function clsNomadScript()
{
	this.lastCheckLinea=0;
	this.Keywords=new Array();
	this.Variables=new Array();
	
	this.CharOpen="{[(";
	this.CharClose="}])";
	this.CharSymbol="+-*/%.!|&^?:,;<>=";
	this.CharNumber="0123456789.";
	this.CharVars="@#$";
	this.CharStartWord="ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz_";
	this.CharEndWord="ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz_0123456789.";
	
	this.Intelicence = null;
	
	///////////////////////////////////////////////////////////////////////////////
	this.isKeyword = function(pName)
	{
		return ContainKey(this.Keywords,"K_"+pName);
	}
	this.GetKeyword = function(pName)
	{
		return GetByKey(this.Keywords,"K_"+pName);
	}
	this.AddKeyword = function(pName, pDescription)
	{
		//Creo el objecto
		var obj=new Object();
		obj.Name=pName;
		obj.Description=pDescription;
		
		//Lo agrego al objeto de palabras claves
		this.Keywords["K_"+obj.Name]=obj;
		return true;
	}

	this.isVariable = function(pName)
	{
		return ContainKey(this.Variables,"K_"+pName.toUpperCase());
	}
	this.AddVariable = function(pName, pDescription)
	{
		//Creo el objecto
		var obj=new Object();
		obj.Name=pName.toUpperCase();
		obj.Description=pDescription;
		
		//Lo agrego al objeto de palabras claves
		this.Variables["K_"+obj.Name]=obj;
		return true;
	}

	///////////////////////////////////////////////////////////////////////////////
	this.Check=function(linea, pText, pScreen)
	{
		var objLine;
		var modo, largo, word;
		var x,L,a,i,deep;
	
		this.lastCheckLinea=(linea<this.lastCheckLinea?linea:this.lastCheckLinea);
		
		//Analizo la coloracion de Sintaxis hasta la region donde se imprime
		while(this.lastCheckLinea<pScreen.ScrollY+pScreen.SizeY && this.lastCheckLinea<pText.Lineas.length)
		{
			objLine=pText.Lineas[this.lastCheckLinea];
			modo =objLine.Mode;
			largo=objLine.Text.length; 
			deep =objLine.Deep; 

			objLine.Color="";
			for (x=0; x<largo; )
			{
				switch(modo)
				{
					case "N":
						if (objLine.Text.substr(x,2)=="/*")
						{
							objLine.Color+="CC";
							x+=2; modo="C";
							objLine.LastColor="C";
						} else
						if (objLine.Text.substr(x,2)=="//")
						{
							while (objLine.Color.length<largo)
								objLine.Color+="C";
							x=largo;
							objLine.LastColor="C";
						} else
						if (objLine.Text.substr(x,1)==" ")
						{
							objLine.Color+="N"; x++;
							objLine.LastColor="N";
						} else
						if (objLine.Text.substr(x,1)=="\"")
						{
							px=x; objLine.Color+="S"; x++;
							while(objLine.Text.substr(x,1)!="\"" && x<largo)
							{
								if (objLine.Text.substr(x,1)=="\\") { objLine.Color+="S"; x++; }
								objLine.Color+="S"; x++;
							}
							objLine.LastColor=(x>=largo?"S":"N");
							objLine.Color+="S"; x++;
							
							//No cerro las commillas, error...
							if (objLine.LastColor=="S")
							{
								x=px;
								objLine.Color=objLine.Color.substr(0,x);
								while(x<=largo) { objLine.Color+="E"; x++ }
							}
							
						} else
						if (objLine.Text.substr(x,1)=="\'")
						{
							px=x; objLine.Color+="S"; x++;
							while(objLine.Text.substr(x,1)!="\'" && x<largo)
							{
								if (objLine.Text.substr(x,1)=="\\") { objLine.Color+="S"; x++; }
								objLine.Color+="S"; x++;
							}
							objLine.LastColor=(x>=largo?"S":"N");
							objLine.Color+="S"; x++;

							//No cerro las commillas, error...
							if (objLine.LastColor=="S")
							{
								x=px;
								objLine.Color=objLine.Color.substr(0,x);
								while(x<=largo) { objLine.Color+="E"; x++ }
							}
						} else
						if (this.CharVars.indexOf(objLine.Text.substr(x,1))>=0)
						{
							word="";
							do
							{
								word+=objLine.Text.substr(x,1);
								x++;
							} while((this.CharEndWord.indexOf(objLine.Text.substr(x,1))>=0) && x<largo);
							if (word.length>1) 
							{
								if (word.substr(0,1)=="$" || this.isVariable(word)) a="X"; 
								                                               else a="E";
								
							} else 
								a="E";
							
							for (i=0; i<word.length; i++) objLine.Color+=a;
							objLine.LastColor="N";
						} else
						if (this.CharNumber.indexOf(objLine.Text.substr(x,1))>=0)
						{
							while((this.CharNumber.indexOf(objLine.Text.substr(x,1))>=0) && x<largo)
							{
								objLine.Color+="D"; x++;
							}
							objLine.LastColor="N";
						} else
						if (this.CharStartWord.indexOf(objLine.Text.substr(x,1))>=0)
						{
							word="";
							while((this.CharEndWord.indexOf(objLine.Text.substr(x,1))>=0) && x<largo)
							{
								word+=objLine.Text.substr(x,1);
								x++;
							}

							if (this.isKeyword(word)) a="K"; else a="E";
							for (i=0; i<word.length; i++) objLine.Color+=a;
							objLine.LastColor="N";
						} else
						if (this.CharOpen.indexOf(objLine.Text.substr(x,1))>=0)
						{
							objLine.Color+="P"; x++; deep++;
							objLine.LastColor="N";
						} else
						if (this.CharClose.indexOf(objLine.Text.substr(x,1))>=0)
						{
							objLine.Color+="P"; x++; deep--; if (deep<0) deep=0;
							objLine.LastColor="N";
						} else
						if (this.CharSymbol.indexOf(objLine.Text.substr(x,1))>=0)
						{
							objLine.Color+="P"; x++;
							objLine.LastColor="N";
						} else
						{
							objLine.Color+="E"; x++;
							objLine.LastColor="N";
						}
						break;

					case "C":
						a=objLine.Text.indexOf("*/",x);
						if (a<0)
						{
							while (objLine.Color.length<largo)
								objLine.Color+="C";
							x=largo;
							objLine.LastColor="C";
						} else
						{
							while (objLine.Color.length<a+2)
								objLine.Color+="C";
							modo="N"; x+=a+2;
							objLine.LastColor="N";
						}
						break;
				}
				
			}
			
			
			this.lastCheckLinea++;
			if (this.lastCheckLinea < pText.Lineas.length)
			{
				pText.Lineas[this.lastCheckLinea].Mode=(modo=="C"?"C":"N");
				pText.Lineas[this.lastCheckLinea].Deep=deep;
			}
		}
	}
	
	this.KeyPress = function(pKey, pEditor)
	{
		var modo;
		var linea=pEditor.Text.Lineas[pEditor.Cursor.Y];
		var text;
		
		if (pEditor.Cursor.X>=linea.Color.length) modo=linea.LastColor;
		                                     else modo=linea.Color.substr(pEditor.Cursor.X,1);
											
		switch(pKey)
		{
			case "{":
				if (modo=="C" || modo=="S" || modo=="E") 
					return false;
				
				//Sangria Previa
				text=linea.Text.substr(0, pEditor.Cursor.X);
				while(text.length>=1 && text.substr(0,1)==" ") text=text.substr(1);
				if (text=="") 
				{   
					pEditor.InsertText(pEditor.Cursor.Y,pEditor.Cursor.X,("                                           ".substr(0,2*pEditor.Text.Lineas[pEditor.Cursor.Y].Deep)));
					pEditor.Cursor.X=2*pEditor.Text.Lineas[pEditor.Cursor.Y].Deep;
				}

				//Caracteres
				pEditor.InsertText(pEditor.Cursor.Y,pEditor.Cursor.X,"{\n");
				pEditor.Cursor.Y++; pEditor.Cursor.X=0;
				linea=pEditor.Text.Lineas[pEditor.Cursor.Y];
				
				//Sangria posterior
				text=linea.Text.substr(0, pEditor.Cursor.X);
				while(text.length>=1 && text.substr(0,1)==" ") text=text.substr(1);
				if (text=="") 
				{   
					pEditor.InsertText(pEditor.Cursor.Y,pEditor.Cursor.X,("                                           ".substr(0,2*pEditor.Text.Lineas[pEditor.Cursor.Y].Deep)));
					pEditor.Cursor.X=2*pEditor.Text.Lineas[pEditor.Cursor.Y].Deep;
				}
				return true;

			case "}":
				if (modo=="C" || modo=="S" || modo=="E") 
					return false;
				
				text=linea.Text;
				while(text.length>=1 && text.substr(0,1)==" ") text=text.substr(1);
				if (text!="") return false;

				pEditor.Cursor.X=2*pEditor.Text.Lineas[pEditor.Cursor.Y].Deep-2;
				if (pEditor.Cursor.X<0) pEditor.Cursor.X=0;

				pEditor.InsertText(pEditor.Cursor.Y,pEditor.Cursor.X,"}\n");
				pEditor.Cursor.Y++; pEditor.Cursor.X=2*pEditor.Text.Lineas[pEditor.Cursor.Y].Deep;
				return true;
		}
		
		return false;
	}
	
	this.lastWord="";
	this.intelicenceID=0;
	this.RefreshIntelicence = function(ID, pEditor)
	{
		if (this.intelicenceID!=ID)
			return;
			
		if (this.Intelicence==null)
			return;
			
		var linea=pEditor.Text.Lineas[pEditor.Cursor.Y];
		var text;

		word=""; x=pEditor.Cursor.X; text=linea.Text;
		while(x<text.length && (this.CharEndWord.indexOf(text.substr(x,1))>=0))
		{
			word=word+text.substr(x,1);
			x++;
		}
		x=pEditor.Cursor.X-1;
		while(x>=0 && (this.CharEndWord.indexOf(text.substr(x,1))>=0))
		{
			word=text.substr(x,1)+word;
			x--;
		}
		while(x>=0 && (this.CharVars.indexOf(text.substr(x,1))>=0))
		{
			word=text.substr(x,1)+word;
			x--;
		}
		
		if (word.substr(0,1)=="#")
		{
			if (this.lastWord==word) return;
			this.lastWord=word;
			eval("parent."+this.Intelicence+"('VAR', '"+word+"')");
		} else
		if (word.substr(0,1)=="@")
		{
			if (this.lastWord==word) return;
			this.lastWord=word;
			eval("parent."+this.Intelicence+"('VAR', '"+word+"')");
		} else
		if (word.length!="" && this.CharEndWord.indexOf(word.substr(0,1))>=0)
		{
			if (this.lastWord==word) return;
			this.lastWord=word;
			eval("parent."+this.Intelicence+"('KEYWORD', '"+word+"')");
		}
	}

	/////////////////////////////////////////////////////////////////////////////////////////
	// Cargo la lista de objetos estandars...
	this.AddKeyword("if");
	this.AddKeyword("else");
	this.AddKeyword("for");
	this.AddKeyword("foreach");
	this.AddKeyword("do");
	this.AddKeyword("while");
	this.AddKeyword("break");
	this.AddKeyword("return");
	
	//Conversion
	this.AddKeyword("IsNull");
	this.AddKeyword("ToBoolean");
	this.AddKeyword("ToNumber");
	this.AddKeyword("ToDateTime");
	this.AddKeyword("ToString");
	this.AddKeyword("Format");
	
	//Logicas
	this.AddKeyword("true");
	this.AddKeyword("false");
	this.AddKeyword("Not");
	
	//Numericas
	this.AddKeyword("Abs");
	this.AddKeyword("Neg");
	this.AddKeyword("Inv");
	this.AddKeyword("Ceil");
	this.AddKeyword("Floor");
	this.AddKeyword("Round");

	//Fecha/Hora
	this.AddKeyword("Now");
	this.AddKeyword("Date");
	this.AddKeyword("Time");
	this.AddKeyword("Year");
	this.AddKeyword("Month");
	this.AddKeyword("Day");
	this.AddKeyword("WDay");
	this.AddKeyword("Hour");
	this.AddKeyword("Minute");
	this.AddKeyword("Second");

	//Texto
	this.AddKeyword("Len");
	this.AddKeyword("SubStr");
	this.AddKeyword("Left");
	this.AddKeyword("Right");
	this.AddKeyword("Normalize");
	this.AddKeyword("UCase");
	this.AddKeyword("LCase");
	this.AddKeyword("Contains");
	this.AddKeyword("StartWith");
	this.AddKeyword("EndWith");
	this.AddKeyword("SubStrBefore");
	this.AddKeyword("SubStrAfter");
	this.AddKeyword("PadLeft");
	this.AddKeyword("PadRight");
	this.AddKeyword("Replace");
	
	//Colecciones
	this.AddKeyword("SetItemCursor");
	this.AddKeyword("GetItemCursor");
	this.AddKeyword("GetLength");
	this.AddKeyword("AddItem");
	this.AddKeyword("DelItem");
	this.AddKeyword("ClearItems");
	
	//Control
	this.AddKeyword("if");
	this.AddKeyword("else");
	this.AddKeyword("foreach");
	this.AddKeyword("for");
	this.AddKeyword("while");
	this.AddKeyword("do");
	this.AddKeyword("break");
	this.AddKeyword("return");
}

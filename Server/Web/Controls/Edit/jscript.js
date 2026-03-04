function clsJScript()
{
	this.lastCheckLinea=0;
	this.Keywords=new Array();
	this.Objects=new Array();
	this.Methods=new Array();
	this.Statics=new Array();
	
	this.CharOpen="{[(";
	this.CharClose="}])";
	this.CharSymbol="+-*/%.!|&^?:,;<>=";
	this.CharNumber="0123456789.";
	this.CharStartWord="ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz_";
	this.CharEndWord="ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz_0123456789";
	
	///////////////////////////////////////////////////////////////////////////////
	this.isKeyword = function(pName)
	{
		return ContainKey(this.Keywords,"K_"+pName);
	}
	this.GetKeyword = function(pName)
	{
		return GetByKey(this.Keywords,"K_"+pName);
	}
	this.AddKeyword = function(pName, pType, pClass)
	{
		//Creo el objecto
		var obj=new Object();
		obj.Name=pName;
		obj.Type=pType;
		obj.ClassName=pClass;
		
		//Lo agrego al objeto de palabras claves
		this.Keywords["K_"+obj.Name]=obj;
		return true;
	}

	this.isObject = function(pName)
	{
		return ContainKey(this.Objects,"K_"+pName);
	}
	this.GetObject = function(pName)
	{
		return GetByKey(this.Objects,"K_"+pName);
	}
	this.AddObject = function(pName, pDescription)
	{
		//Creo el objecto
		var obj=new Object();
		obj.Name=pName;
		obj.Description=pDescription;
		
		//Lo agrego al objeto de palabras claves
		this.Objects["K_"+obj.Name]=obj;
		return true;
	}

	this.isMethod = function(pName)
	{
		return ContainKey(this.Methods,"K_"+pName);
	}
	this.GetMethod = function(pName)
	{
		return GetByKey(this.Methods,"K_"+pName);
	}
	this.AddMethod = function(pName, pMode, pRetval)
	{
		//Creo el objecto
		var obj=new Object();
		obj.Name=pName;
		obj.Mode=pMode;
		obj.Retval=pRetval;
		
		//Lo agrego al objeto de palabras claves
		this.Methods["K_"+obj.Name]=obj;
		return true;
	}

	this.isStatic = function(pName)
	{
		return ContainKey(this.Statics,"K_"+pName);
	}
	this.GetStatic = function(pName)
	{
		return GetByKey(this.Statics,"K_"+pName);
	}
	this.AddStatic = function(pName, pRetval)
	{
		//Creo el objecto
		var obj=new Object();
		obj.Name=pName;
		obj.Retval=pRetval;
		
		//Lo agrego al objeto de palabras claves
		this.Statics["K_"+obj.Name]=obj;
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

							if (this.isKeyword(word)) a="K"; 
								else 
							if (this.isObject(word)) a="X"; 
								else  a="N";
								
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
							modo="N"; x=a+2;
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
	
	/////////////////////////////////////////////////////////////////////////////////////////
	// Cargo la lista de objetos estandars...
	this.AddKeyword("if"      , "FLOW");
	this.AddKeyword("else"    , "FLOW");
	this.AddKeyword("for"     , "FLOW");
	this.AddKeyword("in"      , "FLOW");
	this.AddKeyword("break"   , "FLOW");
	this.AddKeyword("continue", "FLOW");
	this.AddKeyword("switch"  , "FLOW");
	this.AddKeyword("case"    , "FLOW");
	this.AddKeyword("default" , "FLOW");
	this.AddKeyword("do"      , "FLOW");
	this.AddKeyword("while"   , "FLOW");
	this.AddKeyword("try"     , "FLOW");
	this.AddKeyword("catch"   , "FLOW");
	this.AddKeyword("delete"  , "FLOW");
	this.AddKeyword("new"     , "FLOW");
	this.AddKeyword("function", "FLOW");
	this.AddKeyword("return"  , "FLOW");
	this.AddKeyword("this"    , "FLOW");
	this.AddKeyword("typeof"  , "FLOW");
	this.AddKeyword("var"     , "FLOW");
	this.AddKeyword("true"    , "OBJECT", "Boolean");
	this.AddKeyword("false"   , "OBJECT", "Boolean");
	this.AddKeyword("null"    , "OBJECT", "Object");

	
	/////////////////////////////////////////////////////////////////////////////////////////
	// Cargo la lista de objetos estandars...
	this.AddObject("Array");   //ok
	this.AddObject("Boolean"); //ok
	this.AddObject("Date");    //ok
	this.AddObject("Math");    //ok
	this.AddObject("Number");  //ok
	this.AddObject("String"); 
	this.AddObject("Object");  //ok

	
	/////////////////////////////////////////////////////////////////////////////////////////
	//Agrego los Metodos
	
	//ARRAY
	this.AddMethod("Array.length"          ,"Number");
	this.AddMethod("Array.concat()"        ,"Array");
	this.AddMethod("Array.join()"          ,"String");
	this.AddMethod("Array.pop()"           ,"Object");
	this.AddMethod("Array.push()"          ,"Number");
	this.AddMethod("Array.reverse()"       ,"Array");
	this.AddMethod("Array.shift()"         ,"Object");
	this.AddMethod("Array.slice()"         ,"Array");
	this.AddMethod("Array.sort()"          ,"Array");
	this.AddMethod("Array.splice()"        ,"");
	this.AddMethod("Array.toLocaleString()","String");
	this.AddMethod("Array.toString()"      ,"String");
	this.AddMethod("Array.unshift()"       ,"");
	this.AddMethod("Array.valueOf()"       ,"Array");

	//BOOLEAN
	this.AddMethod("Boolean.toJSON()"      ,"String");
	this.AddMethod("Boolean.toString()"    ,"String");
	this.AddMethod("Boolean.valueOf()"     ,"Boolean");

	//NUMBER
	this.AddStatic("Number.MAX_VALUE"         ,"Number");
	this.AddStatic("Number.MIN_VALUE"         ,"Number");
	this.AddStatic("Number.NaN"               ,"Number");
	this.AddStatic("Number.NEGATIVE_INFINITY" ,"Number");
	this.AddStatic("Number.POSITIVE_INFINITY" ,"Number");
	this.AddMethod("Number.toExponential()"   ,"String");
	this.AddMethod("Number.toFixed()"         ,"String");
	this.AddMethod("Number.toJSON()"          ,"String");
	this.AddMethod("Number.toLocaleString()"  ,"String");
	this.AddMethod("Number.toPrecision()"     ,"String");
	this.AddMethod("Number.toString()"        ,"String");
	this.AddMethod("Number.valueOf()"         ,"Number");

	//MATH
	this.AddStatic("Math.E"       ,"Number");
	this.AddStatic("Math.LN2"     ,"Number");
	this.AddStatic("Math.LN10"    ,"Number");
	this.AddStatic("Math.LOG2E"   ,"Number");
	this.AddStatic("Math.LOG10E"  ,"Number");
	this.AddStatic("Math.PI"      ,"Number");
	this.AddStatic("Math.SQRT1_2" ,"Number");
	this.AddStatic("Math.SQRT2"   ,"Number");
	this.AddStatic("Math.abs()"   ,"Number");
	this.AddStatic("Math.acos()"  ,"Number");
	this.AddStatic("Math.asin()"  ,"Number");
	this.AddStatic("Math.atan()"  ,"Number");
	this.AddStatic("Math.atan2()" ,"Number");
	this.AddStatic("Math.ceil()"  ,"Number");
	this.AddStatic("Math.cos()"   ,"Number");
	this.AddStatic("Math.exp()"   ,"Number");
	this.AddStatic("Math.floor()" ,"Number");
	this.AddStatic("Math.log()"   ,"Number");
	this.AddStatic("Math.max()"   ,"Number");
	this.AddStatic("Math.min()"   ,"Number");
	this.AddStatic("Math.pow()"   ,"Number");
	this.AddStatic("Math.random()","Number");
	this.AddStatic("Math.round()" ,"Number");
	this.AddStatic("Math.sin()"   ,"Number");
	this.AddStatic("Math.sqrt()"  ,"Number");
	this.AddStatic("Math.tan()"   ,"Number");

	//DATE
	this.AddMethod("Date.getDate()"           ,"Number");
	this.AddMethod("Date.getDay()"            ,"Number");
	this.AddMethod("Date.getFullYear()"       ,"Number");
	this.AddMethod("Date.getHours()"          ,"Number");
	this.AddMethod("Date.getMilliseconds()"   ,"Number");
	this.AddMethod("Date.getMinutes()"        ,"Number");
	this.AddMethod("Date.getMonth()"          ,"Number");
	this.AddMethod("Date.getSeconds()"        ,"Number");
	this.AddMethod("Date.getTime()"           ,"Number");
	this.AddMethod("Date.getTimezoneOffset()" ,"Number");
	this.AddMethod("Date.getUTCDate()"        ,"Number");
	this.AddMethod("Date.getUTCDay()"         ,"Number");
	this.AddMethod("Date.getUTCFullYear()"    ,"Number");
	this.AddMethod("Date.getUTCHours()"       ,"Number");
	this.AddMethod("Date.getUTCMilliseconds()","Number");
	this.AddMethod("Date.getUTCMinutes()"     ,"Number");
	this.AddMethod("Date.getUTCMonth()"       ,"Number");
	this.AddMethod("Date.getUTCSeconds()"     ,"Number");
	this.AddMethod("Date.getVarDate()"        ,"Number");
	this.AddMethod("Date.getYear()"           ,"Number");
	this.AddStatic("Date.parse()"             ,"Number");
	this.AddMethod("Date.setDate()"           ,"");
	this.AddMethod("Date.setFullYear()"       ,"");
	this.AddMethod("Date.setHours()"          ,"");
	this.AddMethod("Date.setMilliseconds()"   ,"");
	this.AddMethod("Date.setMinutes()"        ,"");
	this.AddMethod("Date.setMonth()"          ,"");
	this.AddMethod("Date.setSeconds()"        ,"");
	this.AddMethod("Date.setTime()"           ,"");
	this.AddMethod("Date.setUTCDate()"        ,"");
	this.AddMethod("Date.setUTCFullYear()"    ,"");
	this.AddMethod("Date.setUTCHours()"       ,"");
	this.AddMethod("Date.setUTCMilliseconds()","");
	this.AddMethod("Date.setUTCMinutes()"     ,"");
	this.AddMethod("Date.setUTCMonth()"       ,"");
	this.AddMethod("Date.setUTCSeconds()"     ,"");
	this.AddMethod("Date.setYear()"           ,"");
	this.AddMethod("Date.toDateString()"      ,"String");
	this.AddMethod("Date.toGMTString()"       ,"String");
	this.AddMethod("Date.toJSON()"            ,"String");
	this.AddMethod("Date.toLocaleDateString()","String");
	this.AddMethod("Date.toLocaleString()"    ,"String");
	this.AddMethod("Date.toLocaleTimeString()","String");
	this.AddMethod("Date.toString()"          ,"String");
	this.AddMethod("Date.toTimeString()"      ,"String");
	this.AddMethod("Date.toUTCString()"       ,"String");
	this.AddStatic("Date.UTC()"               ,"Number");
	this.AddMethod("Date.valueOf()"           ,"Date");
	
	//STRING
	this.AddMethod("String.length","Number");
	this.AddMethod("String.anchor()"       ,"String");
	this.AddMethod("String.big()"          ,"String");
	this.AddMethod("String.blink()"        ,"String");
	this.AddMethod("String.bold()"         ,"String");
	this.AddMethod("String.charAt()"       ,"String");
	this.AddMethod("String.charCodeAt()"   ,"Number");
	this.AddMethod("String.concat()"       ,"String");
	this.AddMethod("String.fixed()"        ,"String");
	this.AddMethod("String.fontcolor()"    ,"String");
	this.AddMethod("String.fontsize()"     ,"String");
	this.AddStatic("String.fromCharCode()" ,"String");
	this.AddMethod("String.indexOf()"      ,"Number");
	this.AddMethod("String.italics()"      ,"String");
	this.AddMethod("String.lastIndexOf()"  ,"Number");
	this.AddMethod("String.link()"         ,"String");
	this.AddMethod("String.localeCompare()","Number");
	this.AddMethod("String.match()"        ,"Array");
	this.AddMethod("String.replace()"      ,"String");
	this.AddMethod("String.search()"       ,"Number");
	this.AddMethod("String.slice()"        ,"String");
	this.AddMethod("String.small()"        ,"String");
	this.AddMethod("String.split()"        ,"Array");
	this.AddMethod("String.strike()"       ,"String");
	this.AddMethod("String.sub()"          ,"String");
	this.AddMethod("String.substr()"       ,"String");
	this.AddMethod("String.substring()"    ,"String");
	this.AddMethod("String.sup()"          ,"String");
	this.AddMethod("String.toJSON()"       ,"String");
	this.AddMethod("String.toLocaleLowerCase()","String");
	this.AddMethod("String.toLocaleUpperCase()","String");
	this.AddMethod("String.toLowerCase()"  ,"String");
	this.AddMethod("String.toUpperCase()"  ,"String");
	this.AddMethod("String.toString()"     ,"String");
	this.AddMethod("String.valueOf()"      ,"String");
	
	//OBJECT
	this.AddMethod("Object.isPrototypeOf()" ,"Booelan");
	this.AddMethod("Object.hasOwnProperty()","Booelan");
	this.AddMethod("Object.toLocaleString()","String");
	this.AddMethod("Object.toString()"      ,"String");
	this.AddMethod("Object.valueOf()"       ,"Object");
}


/*

-Bloque de Codigo
{
}

-Linea de Codigo/Formula
xxxx;

-Parametros Metodo
(xxx, xxx, xxx)

-Parametros Definicion
(xxx, xxx, xxx)

-Comandos
if else, do while, while, switch case break default, for break continue, try catch, function return, var

*/
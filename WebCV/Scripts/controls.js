function CallNothing() {}

//Retorna el valor de una varible pasada por querystring
function GetUrlVariable (pName) {
	pName = "&" + pName + "=";
	
	var strResult = "";
	var strStartVariable;
	var strQueryString = "&" + location.href.substr(location.href.indexOf("?") + 1, 512);
	
	if (strQueryString.indexOf(pName) >= 0) {
    	strStartVariable = strQueryString.substr(strQueryString.indexOf(pName)+pName.length, 512);
    	var hasta = strStartVariable.substr(0, 512).indexOf("&");
    	strResult=(hasta<0?strStartVariable:strStartVariable.substr(0, hasta));
	}
	
	strResult = strResult.replace(new RegExp("%20", "ig"), " ");
	strResult = strResult.replace(new RegExp("%3F", "ig"), "?");
	strResult = strResult.replace(new RegExp("%26", "ig"), "&");
	strResult = strResult.replace(new RegExp("%25", "ig"), "%");
	return strResult;
}

function CallTimeOut(pMess, pFunction) {
	
	parent.ShowWait(pMess);
	
	setTimeout(pFunction, 50);

}


function Replace(strin, strfnd, strrpl) {
	var strout="", lastpos;
	
	for (lastpos=0, pos=strin.indexOf(strfnd); pos!=-1; lastpos=pos+strfnd.length, pos=strin.indexOf(strfnd,lastpos))
	{                
		strout+=strin.substr(lastpos,pos-lastpos);
		strout+=strrpl;
	} 		
	strout+=strin.substr(lastpos);
	
	return strout;
} 

function ButtonChangeStyle(pobj, pnewStyle) {
	LinkChangeStyle(pobj, pnewStyle);
}

function LinkChangeStyle(pobj, pnewStyle) {
	if (pobj?false:true) return;
	
	try {
		switch(pnewStyle)
		{
			case "OVER":
				if (!pobj.disabled) {
					pobj.className = Replace(pobj.className, '_n', '_o');
					pobj.style.backgroundImage = Replace(pobj.style.backgroundImage, '_n.gif', '_o.gif');
				}
			break;
			
			case "NORMAL":
				if (!pobj.disabled) {
					pobj.className = Replace(pobj.className, '_o', '_n');
					pobj.style.backgroundImage = Replace(pobj.style.backgroundImage, '_o.gif', '_n.gif');
				}
			break;
	
			case "DISABLED":
				pobj.disabled=true;
				pobj.className=Replace(pobj.className, '_n', '_d');
				pobj.className=Replace(pobj.className, '_o', '_d');
				pobj.style.backgroundImage = Replace(pobj.style.backgroundImage, '_n.gif', '_d.gif');
				pobj.style.backgroundImage = Replace(pobj.style.backgroundImage, '_o.gif', '_d.gif');
			break;
			
			case "ENABLED":
				pobj.disabled=false;
				pobj.className=Replace(pobj.className, '_d', '_n');
				pobj.className=Replace(pobj.className, '_o', '_n');
				pobj.style.backgroundImage = Replace(pobj.style.backgroundImage, '_d.gif', '_n.gif');
				pobj.style.backgroundImage = Replace(pobj.style.backgroundImage, '_o.gif', '_n.gif');
			break;
		}

	} catch (e) {}
	
}

function clsGrid(pobjHTML, pxmlColumns, pxmlData) {
	this.objHTML = pobjHTML;
	this.xmlData = pxmlData;
	this.xmlColumns = pxmlColumns;
	this.arrColumns = new Array();
	this.arrColumnsPos = new Array();
	this.arrXMLRows;
	this.SelectedRow = null;
	this.oldSelectedRow = null;
	
	this.DeleteChild = function(pId) {
		this.xmlData.DeleteChild(this.GetChild(pId));
	}

	this.GetChild = function(pId) {
		return this.arrXMLRows[pId];
	}	
	
	this.Length = function () {
		if (this.arrXMLRows)
			return this.arrXMLRows.length;
		else
			return 0;
	}
	
	this.Draw = function() {
		var arrCols;
		var arrColPos = new Array();
		var xmlResult = new NomadXML("table");
		var xmlTR;
		var xmlTD;
		var colValue;
		var addAction = false;

		xmlResult.SetAttr("width", "100%"); 	
		xmlResult.SetAttr("class", "TableGrid");
		xmlResult.SetAttr("style", "TABLE-LAYOUT: fixed;");

		//Recorre las columnas y genera la cabecera
		xmlTR = xmlResult.AddTailElement("tr");
		xmlTD = xmlTR.AddTailElement("th");
		xmlTD.SetAttr("width", "60px");
		xmlTD.SetAttr("", "Acciones");
		for (var c = 0; c < this.arrColumnsPos.length; c++) {
			xmlCol = this.arrColumns[this.arrColumnsPos[c]];
		
			xmlTD = xmlTR.AddTailElement("th");
			xmlTD.SetAttr("width", xmlCol.GetAttr("width"));
			xmlTD.SetAttr("", xmlCol.GetAttr("title"));
		}
		
		if (this.xmlData != null) {
		
			//Recorre los datos y crea el HTML
			var Class = 1;
			var Formula;
			this.arrXMLRows = new Array();
			for (var xmlRow = this.xmlData.FirstChild(); xmlRow != null; xmlRow = xmlRow.Next()) {
				
				//Crea el registro de la tabla (TR) y agrega los TDs en orden
				xmlTR = xmlResult.AddTailElement("tr");
				xmlTR.SetAttr("id", "TR" + this.arrXMLRows.length);
				xmlTR.SetAttr("name", "TR" + this.arrXMLRows.length);
				xmlTR.SetAttr("class", "C" + Class);
				
				Class = Class == 1 ? 2 : 1;
				
				this.GetActionTD(xmlTR, this.arrXMLRows.length);
				this.arrXMLRows[this.arrXMLRows.length] = xmlRow;
				
				for (var c = 0; c < this.arrColumnsPos.length; c++) {
					xmlCol = this.arrColumns[this.arrColumnsPos[c]];
					
					xmlTD = xmlTR.AddTailElement("td");
					Formula = xmlCol.GetAttr("formula");
					
					if (Formula == "" ){
						colValue = xmlRow.GetAttr(xmlCol.GetAttr("name"));
					} else {
						colValue = eval(Formula);
					}
					
					colValue = colValue == "" ? "{nbsp}" : colValue;
					xmlTD.SetAttr("", colValue);
				}
			}
		}
		
		this.objHTML.innerHTML = Replace(xmlResult.ToString(), "{nbsp}", "&nbsp;");
	}
	
	this.GetActionTD = function (pXML, pOi) {
		var xmlTD = pXML.AddTailElement("td");
		var xmlImg;
		
		xmlTD.SetAttr("style", "width:60px;");
		xmlTD.SetAttr("class", "C3 alignCenter");
		
		xmlImg = xmlTD.AddTailElement("img");
		xmlImg.SetAttr("src", "./images/ListView/delall_n.gif");
		xmlImg.SetAttr("style", "CURSOR: pointer;");
		xmlImg.SetAttr("onClick", "DelRow(" + pOi + ");");
		xmlImg.SetAttr("title", "Eliminar el registro seleccionado");
		
		xmlImg = xmlTD.AddTailElement("span");
		xmlImg.SetAttr("", "{nbsp}");
		
		xmlImg = xmlTD.AddTailElement("img");
		xmlImg.SetAttr("src", "./images/ListView/edit_n.gif");
		xmlImg.SetAttr("style", "CURSOR: pointer; MARGIN-LEFT: 3px;");
		xmlImg.SetAttr("onClick", "EditRow(" + pOi + ");");
		xmlImg.SetAttr("title", "Editar el registro seleccionado");
	

	}

	this.SelectRow = function(pId) {
		var objTR;
		
		if (Number(this.SelectedRow) == this.SelectedRow) {
			//Pone en normal el row anterior
			objTR = document.getElementById("TR" + this.SelectedRow);
			if (objTR.oldClassName)
				objTR.className = objTR.oldClassName;
			objTR.oldClassName = "";
		}
		
		if (Number(pId) == pId) {
			objTR = document.getElementById("TR" + pId);

			objTR.oldClassName = objTR.className;
			objTR.className = "C3";			
			
			this.SelectedRow = pId;
		} else {
			this.SelectedRow = null;
		}

		
		
	}
	
	//Recorre las columnas y las guarda
	for (var xmlCol = this.xmlColumns.FirstChild(); xmlCol != null; xmlCol = xmlCol.Next()) {
		
		this.arrColumnsPos[this.arrColumnsPos.length] = xmlCol.GetAttr("name");
		this.arrColumns[xmlCol.GetAttr("name")] = xmlCol;
	}
	
}

//Funcion para mapear el registro con el XML Source
function clsFields(pxmlSource) {
	
	this.xmlSource;
	this.arrFields = new Array();
	this.arrFieldsPos = new Array();
	
	this.AddNewField = function(pLabel, pName, pSource, pType, pRequired) {
		
		var objField = new Object();
		objField.Label = pLabel;
		objField.Name = pName;
		objField.Required = pRequired;
		objField.Source = pSource;
		objField.Type = pType;
		this.arrFieldsPos[this.arrFieldsPos.length] = objField;
		this.arrFields[objField.Name] = objField;
	}

	this.AddNewDoc = function(pLabel, pObj, pSource, pRequired) {
		
		var objField = new Object();
		objField.Label = pLabel;
		objField.Name = pSource;
		objField.Required = pRequired;
		objField.Source = pSource;
		objField.Type = "DIGITAL";
		objField.Obj = pObj;
		this.arrFieldsPos[this.arrFieldsPos.length] = objField;
		this.arrFields[objField.Name] = objField;
	}

	this.SetRequired = function (pName, pRequired) {
		var objField;
		objField = this.arrFields[pName];
		
		if (objField) {
			objField.Required = pRequired;
		}
	}
	
	this.SetSource = function(pxmlSource) {
		this.xmlSource = pxmlSource;
	}

	this.HasSource = function(pxmlSource) {
		var result = false;
		
		if (this.xmlSource) {
			result = true;
		}

		return result;
	}	
	
	
	this.MapFromSource = function () {
		var R;
		var objField;
		var objHTML;
		var arrObjHTML;
		var strValue;
		
		for (R=0; R < this.arrFieldsPos.length; R++) {
			objField = this.arrFieldsPos[R];
			
			strValue = this.xmlSource.GetAttr(objField.Source);
			
			if (strValue == "") {
				this.ClearField(objField);
			} else {
				this.SetValue(objField.Name, strValue);
			}

		}
		
	}

	this.MapToSource = function () {
		var X;
		var objHTML;
		var objField;
		
		//Recorre el ARRAY y valida que esten cargados, caso contrario NO se puede guardar
		for (X = 0; X < this.arrFieldsPos.length; X++) {
			objField = this.arrFieldsPos[X];
			objHTML = this.GetHTMLObject(objField.Name);

			try {
			
				switch (objField.Type) {
					case "COMBO":
						this.xmlSource.SetAttr(objField.Source, this.GetValue(objField.Name));
						this.xmlSource.SetAttr(objField.Source + "_txt", objHTML.options[objHTML.selectedIndex].innerHTML);
						break;
	
					case "TXT":
						this.xmlSource.SetAttr(objField.Source, this.GetValue(objField.Name));
						break;

					case "NUM":
						this.xmlSource.SetAttr(objField.Source, this.GetValue(objField.Name));
						break;

					case "CHK":
						this.xmlSource.SetAttr(objField.Source, this.GetValue(objField.Name));
						break;
	
					case "RADIO":
						this.xmlSource.SetAttr(objField.Source, this.GetValue(objField.Name));
						break;
					
					case "DIGITAL":
						this.xmlSource.SetAttr(objField.Source, this.GetValue(objField.Name));
						break;		
				}

			} catch (e) {
				alert("Problemas realizando el MapToSource con el control '" + objField.Name + "'." + e.Description);
				alert("Problemas realizando el MapToSource con el control '" + this.GetValue(objField.Name) + "'.");
				return;
			}
		}
	}

	this.ValidateMandatories = function () {
		var X;
		var objField;
		
		//Recorre el ARRAY y valida que esten cargados, caso contrario NO se puede guardar
		for (X = 0; X < this.arrFieldsPos.length; X++) {
			objField = this.arrFieldsPos[X];
		
			if (objField.Required && this.GetValue(objField.Name) == '') { 
				alert("El campo " + objField.Label + " no tiene valor y es obligatorio.");
				
				if (objField.Type == "RADIO")
					this.GetHTMLObject(objField.Name)[0].focus();
				else
					this.GetHTMLObject(objField.Name).focus();
					
				return false;
			}
			
		}
		
		return true;
	}
	
	this.HasValues = function () {
		var X;
		var objField;
		
		//Recorre el ARRAY e indica si alguno de los campos tiene valor
		for (X = 0; X < this.arrFieldsPos.length; X++) {
			objField = this.arrFieldsPos[X];
		
			if (this.GetValue(objField.Name) != '') { 
				return true;
			}
			
		}
		
		return false;
	}


	this.ClearFields = function () {
		var X;
		
		//Recorre el ARRAY y valida que esten cargados, caso contrario NO se puede guardar
		for (X = 0; X < this.arrFieldsPos.length; X++) {
			objField = this.arrFieldsPos[X];
			this.ClearField(objField);
		}
	
	}

	this.ClearField = function(pobjField) {
		var objHTML;
		
		objHTML = this.GetHTMLObject(pobjField.Name);

		switch (pobjField.Type) {
			case "TXT":
				objHTML.value = "";
				break;

			case "NUM":
				objHTML.value = "";
				break;

			case "COMBO":
				objHTML.selectedIndex = 0;
				break;

			case "RADIO":
				objHTML[0].checked = true;
				break;

			case "CHK":
				objHTML.checked = false;
				break;

			case "DIGITAL":
				pobjField.Obj.Delete();
				break;				
		}

	}

	this.GetValue = function (pName) {
		var objHTML;
		var objField;
		var Result;
		
		objField = this.arrFields[pName];
		objHTML = this.GetHTMLObject(objField.Name);

		switch (objField.Type) {
			case "TXT":
				Result = objHTML.value;
				break;

			case "NUM":
				Result = objHTML.value;
				break;

			case "COMBO":
				Result = objHTML.value;
				break;

			case "CHK":
				Result = objHTML.checked ? 1 : 0;
				break;

			case "RADIO":
				Result = "";
				var X;
				for (X = 0; X < objHTML.length; X++) { 
   					if (objHTML[X].checked) {
   						Result = objHTML[X].value;
   						break; 
   					}
				}
				break;
			
			case "DIGITAL":
				Result = objField.Obj.GetOIDoc();
				break;				
				
		}
							
		return Result;
	}

	this.SetValue = function (pName, pValue) {
		var objHTML;
		var objField;
		var Result;
		
		objField = this.arrFields[pName];
		objHTML = this.GetHTMLObject(objField.Name);

		switch (objField.Type) {
			case "TXT":
				objHTML.value = pValue;
				break;

			case "NUM":
				objHTML.value = pValue;
				break;

			case "COMBO":
				objHTML.value = pValue;
				break;

			case "CHK":
				objHTML.checked = (pValue == 1 || pValue == "1");
				break;
				
			case "RADIO":
				var X;
				for (X = 0; X < objHTML.length; X++) { 
   					if (objHTML[X].value == pValue) {
   						objHTML[X].checked = true;
   						break; 
   					}
				}
				break;
			
			case "DIGITAL":
				Result = objField.Obj.SetOIDoc(pValue);
				break;
		}
							
	}	

	this.GetHTMLObject = function (pName) {
		var objField;
		var objHTML;
		
		objField = this.arrFields[pName];

		switch (objField.Type) {
			case "TXT": case "NUM": case "COMBO": case "CHK":
				objHTML = document.getElementById(objField.Name);
				break;

			case "RADIO":
				objHTML = document.getElementsByName(objField.Name);
				break;
			
			default:
				objHTML = null;
				break;
		}		

		return objHTML;
	}
	
	this.SetXMLValue = function (pName, pValue) {
		if (this.HasSource()) {
			this.xmlSource.SetAttr(pName, pValue);
		}
	}

	this.GetXMLValue = function (pName) {
		return this.HasSource() ? this.xmlSource.GetAttr(pName) : "";
	}

	this.SetEnabled = function (pName, pEnabled) {
		var obj;
		obj = this.GetHTMLObject(pName);
		
		if (obj) {
			obj.className = pEnabled ? "Box" : "BoxD";
			obj.disabled = !pEnabled;
		}
	}

	this.SetSource(pxmlSource);

}


function clsDigitalDoc(pClass, pType, pObjPrefix, pMethodAdd, pMethodDel) {
	this.OIParent;	  	//OI del parent
	this.OIDoc = "";	//OI del documento digital
	
	this.Class     = pClass;
	this.MethodAdd = pMethodAdd;
	this.MethodDel = pMethodDel;
	this.Type      = pType;
	
	this.butAdd  = document.getElementById(pObjPrefix + "Add");
	this.butDown = document.getElementById(pObjPrefix + "Down");
	this.butDel  = document.getElementById(pObjPrefix + "Del");
	this.butChg  = document.getElementById(pObjPrefix + "Chg");
	this.ifrDwon = document.getElementById(pObjPrefix + "Ifr");
	this.objImg  = document.getElementById(pObjPrefix + "Img");
	
	this.SetOIParent = function(pOIParent) {
		this.OIParent = pOIParent;
	}

	this.SetOIDoc = function(pOIDoc) {
		this.OIDoc = pOIDoc;
		
		if (this.butAdd)  this.butAdd.style.display  = "none";
		if (this.butChg)  this.butChg.style.display  = "block";
		if (this.butDown) this.butDown.style.display = "block";
		if (this.butDel)  this.butDel.style.display  = "block";
		
		switch (this.Type) {
			case "IMG":
				if (this.objImg) this.objImg.src = "Nomadfile.aspx?mode=DB&class=" + this.Class + "&id=" + this.OIDoc;
			
				break;
			
			case "DOC":
				if (this.objImg) this.objImg.src = Replace(this.objImg.src, "_d.", "_n.");
				break;
		}
		
	}			
	
	this.GetOIDoc = function() {
		return this.OIDoc;
	}
		
	this.Add = function() {
		this.OpenBrowser();
	}

	this.Download = function() {
		this.ifrDwon.src = "Nomadfile.aspx?mode=DB&class=" + this.Class + "&id=" + this.OIDoc + "&download=1";
	}
	
	this.Delete = function() {
		this.OIDoc = "";
		if (this.butAdd)  this.butAdd.style.display  = "block";
		if (this.butChg)  this.butChg.style.display  = "none";
		if (this.butDown) this.butDown.style.display = "none";
		if (this.butDel)  this.butDel.style.display  = "none";
		
		if (this.objImg) this.objImg.src = this.objImg.nmdSrc;

		switch (this.Type) {
			case "DOC":
				if (this.objImg) this.objImg.src = Replace(this.objImg.src, "_n.", "_d.");
				break;
		}
	}						

	this.OpenBrowser = function() {
		var Result;
		
		//window.open("browsePage.htm?class=" + this.Class + "&methodname=" + this.MethodAdd);
		//return;
		
		Result = window.showModalDialog('browsePage.htm?class=' + this.Class + "&methodname=" + this.MethodAdd, 'Seleccione un archivo', 'dialogHeight:150px;dialogWidth:400px');
		
		if (Result) {
			if (Result != "CLOSED") {
				var objResult = new NomadXML(Result);
				Result = objResult.GetAttr("id");
				this.SetOIDoc(Result);
			}
		} else {
			Result = "";
		}
		
		return Result;
	}

	if (this.butAdd)  this.butAdd.style.display  = "block";
	if (this.butChg)  this.butChg.style.display  = "none";
	if (this.butDown) this.butDown.style.display = "none";
	if (this.butDel)  this.butDel.style.display  = "none";
	
	if (this.objImg) this.objImg.nmdSrc = this.objImg.src;
	
	if (this.objImg && this.Type == "IMG") {
		this.objImg.onload = function() {
			if (parseInt(this.width, 10) > 120) {
				this.width = 120;
			}
		}
	}
	
}




function FillLeft(inSTR,len,STR)
{
	while (inSTR.length<len) inSTR=STR+inSTR;
	return inSTR;
}

function FillRight(inSTR,len,STR)
{
	while (inSTR.length<len) inSTR=inSTR+STR;
	return inSTR;
}

function str2datetime(inSTR)
{
	switch(inSTR.length)
	{
		case 8: //YYYYMMDD
			return str2date(inSTR);
			
		case 6: //HHMMSS
			return str2time(inSTR);
			
		case 12: //YYYYMMDDHHMM
			return new Date(parseInt(inSTR.substr(0,4),10), parseInt(inSTR.substr(4,2),10)-1, parseInt(inSTR.substr(6,2),10),parseInt(inSTR.substr(8,2),10), parseInt(inSTR.substr(10,2),10), 0);

		case 14: //YYYYMMDDHHMMSS
			return new Date(parseInt(inSTR.substr(0,4),10), parseInt(inSTR.substr(4,2),10)-1, parseInt(inSTR.substr(6,2),10),parseInt(inSTR.substr(8,2),10), parseInt(inSTR.substr(10,2),10), parseInt(inSTR.substr(12,2),10));
	}
	return null;
}

function str2date(inSTR) {
	return new Date(parseInt(inSTR.substr(0,4),10), parseInt(inSTR.substr(4,2),10)-1, parseInt(inSTR.substr(6,2),10));
}

function date2str(inDATE) {
	return FillLeft( inDATE.getFullYear().toString(),4,"0")+
		     FillLeft((inDATE.getMonth()+1).toString(),2,"0")+
		     FillLeft( inDATE.getDate().toString()    ,2,"0");
}

function time2str(inDATE) {
	return FillLeft( inDATE.getHours().toString()  ,2,"0")+
		     FillLeft( inDATE.getMinutes().toString(),2,"0")+
		     FillLeft( inDATE.getSeconds().toString(),2,"0");
}

function datetime2str(inDATE) {
	return date2str(inDATE)+time2str(inDATE);
}


function formatDate(pDate, pFormat) {
	
	//Años
	pFormat = Replace(pFormat, "yyyy", FillLeft(pDate.getFullYear().toString(), 4, "0"));
	pFormat = Replace(pFormat, "yy", FillLeft(pDate.getYear().toString(), 2, "0"));
	pFormat = Replace(pFormat, "y", pDate.getFullYear());
	
	//Meses
	pFormat = Replace(pFormat, "mmmm", "{1}");
	pFormat = Replace(pFormat, "mmm", "{2}");
	pFormat = Replace(pFormat, "mm", FillLeft((pDate.getMonth() + 1).toString(), 2, "0"));
	pFormat = Replace(pFormat, "m", (pDate.getMonth() + 1));

	//Dias
	pFormat = Replace(pFormat, "dddd", "{3}");
	pFormat = Replace(pFormat, "ddd", "{4}");
	pFormat = Replace(pFormat, "dd", FillLeft(pDate.getDate().toString(), 2, "0"));
	pFormat = Replace(pFormat, "d", pDate.getDate());

	//Horas	
	pFormat = Replace(pFormat, "HH", FillLeft(pDate.getHours().toString(), 2, "0"));
	pFormat = Replace(pFormat, "H", pDate.getHours());
	pFormat = Replace(pFormat, "hh", FillLeft(((pDate.getHours() + 11) % 12 + 1).toString(), 2, "0"));
	pFormat = Replace(pFormat, "h", (pDate.getHours() + 11) % 12 + 1 );
	pFormat = Replace(pFormat, "p", pDate.getHours() >= 12 ? "pm" : "am");
	pFormat = Replace(pFormat, "P", pDate.getHours() >= 12 ? "PM" : "AM");

	//Minutos
	pFormat = Replace(pFormat, "MM", FillLeft(pDate.getMinutes().toString(), 2, "0"));
	pFormat = Replace(pFormat, "M", pDate.getMinutes());

	//Segundos
	pFormat = Replace(pFormat, "SS", FillLeft(pDate.getSeconds().toString(), 2, "0"));
	pFormat = Replace(pFormat, "S", pDate.getSeconds());


	pFormat = Replace(pFormat, "{1}", "Enero|Febrero|Marzo|Abril|Mayo|Junio|Julio|Agosto|Sepetiembre|Octubre|Noviembre|Diciembre".split("|")[pDate.getMonth()]);
	pFormat = Replace(pFormat, "{2}", "Ene|Feb|Mar|Abr|May|Jun|Jul|Ago|Sep|Oct|Nov|Dic".split("|")[pDate.getMonth()]);
	pFormat = Replace(pFormat, "{3}", "Domingo|Lunes|Martes|Miercoles|Jueves|Viernes|Sábado".split("|")[pDate.getDay()]);	
	pFormat = Replace(pFormat, "{4}", "Dom|Lun|Mar|Mie|Jue|Vie|Sab".split("|")[pDate.getDay()]);

	
	return pFormat;
}

function isDouble(pstrNum) {
	var r = new RegExp("[-+]?([0-9]+\\.[0-9]+|[0-9]+)");
	var m = r.exec( pstrNum );
	var bolResult = false;
		
	if ( m == null ) return false;

	var x;
	for (x = 0; x < m.length; x++) 
		if (pstrNum == m[x]) {
			bolResult = true;
			break;
		}
	
	return bolResult;
}

function isInteger(pstrNum) {
	var r = new RegExp("[-+]?[0-9]+");
	var m = r.exec( pstrNum );
	var bolResult = false;
		
	if ( m == null ) return false;

	var x;
	for (x = 0; x < m.length; x++) 
		if (pstrNum == m[x]) {
			bolResult = true;
			break;
		}
	
	return bolResult;
}
			

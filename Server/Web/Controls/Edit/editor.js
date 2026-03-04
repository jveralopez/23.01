var Editor=null;

function ContainKey(arrObj, MyKey)
{
	return (arrObj[MyKey]?true:false);
}

function GetByKey(arrObj, MyKey)
{
	if(arrObj[MyKey]) return arrObj[MyKey];
	return null;
}

function Replace(txtIn,txtFind,txtRepl)
{
	var txtOut="";
	
	for (t=0; t<txtIn.length; t++)
	if (txtIn.substr(t,txtFind.length)==txtFind)
	{
		txtOut+=txtRepl;
		t+=txtFind.length-1;
	} else
		txtOut+=txtIn.substr(t,1);

	return txtOut;
}

function SubstringBefore(txtIn, txtFnd)
{
	var sp=txtIn.split(txtFnd,2);
	return sp[0];
}

function SubstringAfter(txtIn, txtFnd)
{
	var sp=txtIn.split(txtFnd,2);
	return sp.length>=1?sp[1]:"";
}

function SubstringWord(txtIn)
{
	var t=0;
	while (t<txtIn.length && "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz_0123456789".indexOf(txtIn.substr(t,1))>=0) t++;
	
	return txtIn.substr(0,t);
}

function SubstringParam(txtIn)
{
	var t=0;
	
	//parametro ANY
	if (txtIn.substr(0,3)=="...") return "...";
	
	//parametro OPCIONAL
	if (txtIn.substr(0,1)=="[") t++;
	
	//parametro
	while (t<txtIn.length && "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz_0123456789".indexOf(txtIn.substr(t,1))>=0) t++;
	
	//parametro OPCIONAL FIN
	if (txtIn.substr(0,1)=="[")
	{
		if (txtIn.substr(t,1)!="]") return "";
		t++;
	}
	
	
	
	return txtIn.substr(0,t);
}

function LTrim(txtIn)
{
	var t=0;
	
	//cuento cuantos espacios hay al comienzo
	while (t<txtIn.length && txtIn.substr(t,1)==" ") t++;
	return txtIn.substr(t);
}

function RTrim(txtIn)
{
	var t=txtIn.length;
	
	//cuento cuantos espacios hay al comienzo
	while (t>0 && txtIn.substr(t-1,1)==" ") t--;
	return txtIn.substr(0,t);
}

function Trim(txtIn)
{
	return LTrim(RTrim(txtIn));
}

//Retorna el valor de una varible pasada por querystring
function GetUrlVariable(pName) 
{
	pName = "&" + pName + "=";
	
	var strResult = "";
	var strStartVariable;
	var strQueryString = "&" + location.href.substr(location.href.indexOf("?") + 1, 512);
	
	if (strQueryString.toUpperCase().indexOf(pName.toUpperCase()) >= 0) {
    	strStartVariable = strQueryString.substr(strQueryString.toUpperCase().indexOf(pName.toUpperCase())+pName.length, 512);
    	var hasta = strStartVariable.substr(0, 512).indexOf("&");
    	strResult=(hasta<0?strStartVariable:strStartVariable.substr(0, hasta));
	}
	
	strResult = strResult.replace(new RegExp("%20", "ig"), " ");
	strResult = strResult.replace(new RegExp("%3F", "ig"), "?");
	strResult = strResult.replace(new RegExp("%26", "ig"), "&");
	strResult = strResult.replace(new RegExp("%25", "ig"), "%");
	return strResult;
}

function clsLinea(pText)
{
	//Linea actual
	this.Text=pText;
	this.Color="";
	this.Mode="N";
	this.Deep=0;
}

function clsCursor()
{
	this.X=0;
	this.Y=0;
	this.Insert=true;
	this.Show=false;
	this.showID=0;
}

function clsScreen(pSizeX, pSizeY)
{
	this.SizeX=pSizeX;
	this.SizeY=pSizeY;
	this.ScrollX=0;
	this.ScrollY=0;
}

function clsEditor(pSizeX, pSizeY, pSyntax)
{
	var x,y;
	
	/////////////////////////////////////////////////////////////////
	//Variable Global
	Editor=this;
	
	/////////////////////////////////////////////////////////////////
	//Objetos
	this.Init=false;
	this.readOnly=false;
	this.Syntax=pSyntax;
	this.Cursor=new clsCursor();
	this.Screen=new clsScreen(pSizeX, pSizeY);
	
	/////////////////////////////////////////////////////////////////
	//Pantalla Virtual
	this.Data=new Array();
	for (y=0; y<this.Screen.SizeY; y++)
	{
		this.Data[y]=new Array();
		for (x=0; x<this.Screen.SizeX; x++)
		{
			this.Data[y][x]=new Object();
			this.Data[y][x].Text="";
			this.Data[y][x].Style="N";
			this.Data[y][x].Back="N";
		}
	}
	
	/////////////////////////////////////////////////////////////////
	//Select
	this.Select=new Object();
	this.Select.Select=false;
	this.Select.X1=0;
	this.Select.Y1=0;
	this.Select.X2=0;
	this.Select.Y2=0;

	/////////////////////////////////////////////////////////////////
	//UNDO
	this.Undo=new Object();
	this.Undo.Actions=new Array();
	this.Undo.Last=0;
	
	/////////////////////////////////////////////////////////////////
	//Texto
	this.Text=new Object();
	this.Text.Text="";
	this.Text.Size=0;
	this.Text.Lineas=new Array();
	this.Text.Lineas[0]=new clsLinea("");

	/////////////////////////////////////////////////////////////////
	//Divs
	this.Divs=new Object();
	this.Divs.Cursor   =null; //document.getElementById("CursorDisplay");
	this.Divs.Text     =null; //document.getElementById("TextDisplay");
	this.Divs.Back     =null; //document.getElementById("BackgroundDisplay");
	this.Divs.Size     =null; //document.getElementById("TDInfoSize");
	this.Divs.Length   =null; //document.getElementById("TDInfoLength");
	this.Divs.PosX     =null; //document.getElementById("TDInfoPosX");
	this.Divs.PosY     =null; //document.getElementById("TDInfoPosY");
	this.Divs.SelSize  =null; //document.getElementById("TDInfoSelSize");
	this.Divs.SelLength=null; //document.getElementById("TDInfoSelLength");
	this.Divs.TextArea =null; //document.getElementById("TextArea");
	
	/////////////////////////////////////////////////////////////////
	//Escala
	this.Scale=new Object();
	this.Scale.X=1;
	this.Scale.Y=1;
	
	/////////////////////////////////////////////////////////////////
	//Refresh
	this.Refresh=new Object();
	this.Refresh.ToolBarId=0;
	this.Refresh.ScreenId=0;

	this.Initialize = function()
	{
		var txt="",y,x,line="";
		txt+='<table class="TableEditor" style="" >';
		txt+='<col /><col width="95" /><col width="75" /><col width="50" /><col width="50" /><col width="65" /><col width="75" />';
		txt+='<tr><td class="TableMainEditor" colspan="7" id="TableMainEditor" >';

		for (x=0; x<this.Screen.SizeX; x++) line+="X";
		for (y=0; y<this.Screen.SizeY; y++) txt+=line+"<br/>";
		
		txt+='</td></tr>';
		txt+='<tr height="20px" ><td>&nbsp;</td><td id="TDInfoSize" >&nbsp;</td><td id="TDInfoLength" >&nbsp;</td><td id="TDInfoPosX" >&nbsp;</td><td id="TDInfoPosY" >&nbsp;</td><td id="TDInfoSelSize" >&nbsp;</td><td id="TDInfoSelLength" >&nbsp;</td></tr>';
		txt+='</table>';
			
		document.body.innerHTML=txt;
		window.setTimeout("Editor.Initialize2(true);",500);
	}
	
	
	this.Initialize2 = function(calcSize)
	{
		if (calcSize)
		{
			var obj=document.getElementById("TableMainEditor");
				
			this.Scale.X=(obj.offsetWidth -4)/this.Screen.SizeX;
			this.Scale.Y=(obj.offsetHeight-4)/this.Screen.SizeY;
		}
		
		var txt="";
		
		if (this.readOnly)
		{
			txt+='<table class="TableEditor" style="width:'+(this.Scale.X*this.Screen.SizeX+6)+'px;height:'+(this.Scale.Y*this.Screen.SizeY+26)+'px;" >';
			txt+='<col /><col width="95" /><col width="75" />';
			txt+='<tr><td colspan="3">&nbsp;</td></tr>';
			txt+='<tr height="20px" ><td id="TDInfoHelp">&nbsp;</td><td id="TDInfoSize" >&nbsp;</td><td id="TDInfoLength" >&nbsp;</td></tr>';
			txt+='</table>';
		} else
		{
			txt+='<table class="TableEditor" style="width:'+(this.Scale.X*this.Screen.SizeX+6)+'px;height:'+(this.Scale.Y*this.Screen.SizeY+26)+'px;" >';
			txt+='<col /><col width="95" /><col width="75" /><col width="50" /><col width="50" /><col width="65" /><col width="75" />';
			txt+='<tr><td colspan="7">&nbsp;</td></tr>';
			txt+='<tr height="20px" ><td id="TDInfoHelp">&nbsp;</td><td id="TDInfoSize" >&nbsp;</td><td id="TDInfoLength" >&nbsp;</td><td id="TDInfoPosX" >&nbsp;</td><td id="TDInfoPosY" >&nbsp;</td><td id="TDInfoSelSize" >&nbsp;</td><td id="TDInfoSelLength" >&nbsp;</td></tr>';
			txt+='</table>';
		}
		if (!this.readOnly) txt+='<div class="DivEditor" ><textarea id="TextArea" style="font-size:1px;" onkeypress="Editor.KeyPress(event);" onkeydown="Editor.KeyDown(event);" onfocus="Editor.SetFocus(event);" onblur="Editor.LostFocus(event);" ></textarea></div>';
		txt+='<div class="DivEditor" style="width:'+(this.Scale.X*this.Screen.SizeX)+'px;height:'+(this.Scale.Y*this.Screen.SizeY)+'px;" id="BackgroundDisplay" ></div>';
		if (!this.readOnly) txt+='<div class="DivCursor" id="CursorDisplay" >&#160;</div>';
		txt+='<div class="DivEditor" style="width:'+(this.Scale.X*this.Screen.SizeX)+'px;height:'+(this.Scale.Y*this.Screen.SizeY)+'px;" id="TextDisplay" onmousedown="Editor.MouseDown(event);" onmouseup="Editor.MouseUp(event);"></div>';
		txt+='<div class="DivToolTip" id="ToolTip" style="display:none;" >&#160;</div>';
		txt+='<div class="DivIntelicence" id="Intelicence" style="display:none;" >&#160;</div>';
			
		document.body.innerHTML=txt;
	
		
		this.Divs.Cursor     =document.getElementById("CursorDisplay");
		this.Divs.Text       =document.getElementById("TextDisplay");
		this.Divs.Back       =document.getElementById("BackgroundDisplay");
		this.Divs.Size       =document.getElementById("TDInfoSize");
		this.Divs.Length     =document.getElementById("TDInfoLength");
		this.Divs.PosX       =document.getElementById("TDInfoPosX");
		this.Divs.PosY       =document.getElementById("TDInfoPosY");
		this.Divs.SelSize    =document.getElementById("TDInfoSelSize");
		this.Divs.SelLength  =document.getElementById("TDInfoSelLength");
		this.Divs.TextArea   =document.getElementById("TextArea");
		this.Divs.Help       =document.getElementById("TDInfoHelp");
		this.Divs.ToolTip    =document.getElementById("ToolTip");
		this.Divs.Intelicence=document.getElementById("Intelicence");
		this.Init=true;

		//Verificaciones Iniciales
		this.Check();
		this.RefreshToolBar();
		this.RefreshText();
		this.RefreshScreen();
		
		//Foco Inicial
		if (!this.readOnly)
			this.Divs.TextArea.focus();
	}
	
	this.TooltipPosition=0;
	this.ShowTooltip = function(txtShow)
	{
		var X;
		if (txtShow=="")
		{
			this.TooltipPosition=0;
			this.Divs.ToolTip.style.display="none";
		} else
		{
			this.Divs.ToolTip.innerHTML=txtShow;
			
			X=(this.Cursor.X-this.Screen.ScrollX)*this.Scale.X-100; if (X<0) X=0;
			this.Divs.ToolTip.style.left=X+"px";
			
			if ((this.IntelicencePosition==0 || this.IntelicencePosition==2) && (this.Cursor.Y-this.Screen.ScrollY<this.Screen.SizeY-2))
			{
				this.TooltipPosition=1;
				this.Divs.ToolTip.style.top=((this.Cursor.Y-this.Screen.ScrollY)*this.Scale.Y+15)+"px";
				this.Divs.ToolTip.style.display="inline";
			} else
			if ((this.IntelicencePosition==0 || this.IntelicencePosition==1) && (this.Cursor.Y-this.Screen.ScrollY>=3))
			{
				this.TooltipPosition=2;
				this.Divs.ToolTip.style.top=((this.Cursor.Y-this.Screen.ScrollY)*this.Scale.Y-35)+"px";
				this.Divs.ToolTip.style.display="inline";
			} else
				this.Divs.ToolTip.style.display="none";
		}
	}
	
	this.IntelicencePosition=0;
	this.IntelicenceSelect=0;
	this.IntelicenceScroll=0;
	this.arrIntelicence=null;
	this.ShowIntelicence = function(arrShow, keepCursor)
	{
		this.arrIntelicence=arrShow;
		if (keepCursor?false:true) this.IntelicenceSelect=0;
		
		var X,Y,txt;
		if (arrShow==null || arrShow.length==0)
		{
			this.Divs.Intelicence.style.display="none";
			this.IntelicencePosition=0;
			this.IntelicenceScroll=0;
		} else
		{
			if (this.IntelicenceScroll>this.IntelicenceSelect)
				this.IntelicenceScroll=this.IntelicenceSelect;
		
			if (this.IntelicenceScroll+6<this.IntelicenceSelect)
				this.IntelicenceScroll=this.IntelicenceSelect-6;

				txt="<table class='TableIntelicence' cellspacing='0' ><col width='16px'/><col/>";
			for (Y=0, X=this.IntelicenceScroll; X<arrShow.length && Y<7; X++, Y++)
			{
				if (this.IntelicenceSelect==X)
					txt+="<tr class='SELECT' ><td><img src='./images/"+arrShow[X].Type+".ico' width='12px' height='12px'/></td><td>"+arrShow[X].Label+"</td></tr>";
				else
					txt+="<tr><td><img src='./images/"+arrShow[X].Type+".ico' width='12px' height='12px'/></td><td>"+arrShow[X].Label+"</td></tr>";
			}
			txt+="</table>";
			
			this.Divs.Intelicence.innerHTML=txt;
			
			X=(this.Cursor.X-this.Screen.ScrollX)*this.Scale.X-20; if (X<0) X=0;
			this.Divs.Intelicence.style.left=X+"px";
			
			if (this.Cursor.Y-this.Screen.ScrollY<this.Screen.SizeY-6)
			{
				this.IntelicencePosition=1;
				this.Divs.Intelicence.style.top=((this.Cursor.Y-this.Screen.ScrollY+1)*this.Scale.Y+5)+"px";
			} else
			{
				this.IntelicencePosition=2;
				this.Divs.Intelicence.style.top=((this.Cursor.Y-this.Screen.ScrollY)*this.Scale.Y-110)+"px";
			}
				
			this.Divs.Intelicence.style.display="inline";
		}
	}
	
	this.RefreshToolBar=function()
	{
		this.Refresh.ToolBarId++;
		window.setTimeout("Editor.internalRefreshToolBar("+this.Refresh.ToolBarId+")",200);
	}
	
	this.internalRefreshToolBar = function(lastID)
	{
		if (!this.Init) return;
		if (this.Refresh.ToolBarId!=lastID) return;
		
		this.Divs.Size.innerHTML="Tama&ntilde;o: "+this.Text.Size;
		this.Divs.Length.innerHTML="Lineas: "+this.Text.Lineas.length;
		
		if (!this.readOnly)
		{
			this.Divs.PosX.innerHTML="X: "+(this.Cursor.X+1);
			this.Divs.PosY.innerHTML="Y: "+(this.Cursor.Y+1);
			if (this.Select.Select)
			{
				this.Divs.SelSize.innerHTML="Sel: "+this.Select.Size;
				this.Divs.SelLength.innerHTML="Lineas: "+(1+this.Select.Y2-this.Select.Y1);
			} else
			{
				this.Divs.SelSize.innerHTML="Sel: -";
				this.Divs.SelLength.innerHTML="Lineas: -";
			}
		}
	}

	this.Check=function()
	{
		//Cursor
		if (this.Cursor.X<  0) this.Cursor.X=0;
		if (this.Cursor.Y<  0) this.Cursor.Y=0;
		if (this.Cursor.X>998) this.Cursor.X=998;
		if (this.Cursor.Y>=this.Text.Lineas.length) this.Cursor.Y=this.Text.Lineas.length-1;
		
		//Scroll
		if (this.Cursor.X< this.Screen.ScrollX                    ) this.Screen.ScrollX=this.Cursor.X;
		if (this.Cursor.X> this.Screen.ScrollX+this.Screen.SizeX-1) this.Screen.ScrollX=this.Cursor.X-this.Screen.SizeX+1;
		if (this.Cursor.Y< this.Screen.ScrollY                    ) this.Screen.ScrollY=this.Cursor.Y;
		if (this.Cursor.Y> this.Screen.ScrollY+this.Screen.SizeY-1) this.Screen.ScrollY=this.Cursor.Y-this.Screen.SizeY+1;
		if (this.Screen.ScrollY< 0) this.Screen.ScrollY=0;
		if (this.Screen.ScrollY>=this.Text.Lineas.length) this.Screen.ScrollY=this.Text.Lineas.length-1;
	}
	
	this.ShowHelp = function(text)
	{
		this.Divs.Help.innerHTML=text;
	}
	
	this.ShowCursor = function(force)
	{
		if (!this.Init) return;
		if (this.readOnly) return;
		var x=this.Cursor.X-this.Screen.ScrollX;
		var y=this.Cursor.Y-this.Screen.ScrollY;
		
		if (force || this.Cursor.Show)
		{
			this.Divs.Cursor.className=(this.Cursor.Insert?"DivCursorShowInsert":"DivCursorShowOverwrite");
			this.Divs.Cursor.style.left=(3+x*this.Scale.X)+"px";
			this.Divs.Cursor.style.top =(3+y*this.Scale.Y)+"px";
			
			this.Cursor.Show=true;
			this.Cursor.showID++;
			window.setTimeout("Editor.HideCursor(false,"+this.Cursor.showID+")",500);
		}
		
		if (this.Syntax && this.Syntax.RefreshIntelicence)
		{
			this.Syntax.intelicenceID++;
			window.setTimeout("Editor.Syntax.RefreshIntelicence("+this.Syntax.intelicenceID+", Editor)",1000);
		}
	}	
	this.HideCursor=function(force, lastID)
	{
		if (!this.Init) return;
		if (this.readOnly) return;
		if (!force && this.Cursor.Show)
		{
			if (this.Cursor.showID!=lastID) return;
			
			this.Divs.Cursor.className="DivCursor";
			window.setTimeout("Editor.ShowCursor(false)",500);
		} else
		{
			this.Cursor.Show=false;
			this.Divs.Cursor.className="DivCursor";
		}
	}
	
	this.textCopy="";
	
	this.copyToClipboard=function(text)
	{
		if (window.clipboardData) // Internet Explorer
		{  
			window.clipboardData.setData("Text", text);
		}
		else
		{  
			textCopy=text;
		}
	}

	this.readToClipboard=function(text)
	{
		if (window.clipboardData) // Internet Explorer
		{  
			return window.clipboardData.getData("Text");
		}
		else
		{  
			return textCopy;
		}
	}

	this.callUndo=function()
	{
		if (this.Undo.Last==0) return;
		
		this.Undo.Last--;
		
		switch(this.Undo.Actions[this.Undo.Last].Tipo)
		{
			case "DEL":
				var obj=this.InsertText(this.Undo.Actions[this.Undo.Last].Y1,this.Undo.Actions[this.Undo.Last].X1,this.Undo.Actions[this.Undo.Last].Text,true);
				this.Cursor.Y=this.Undo.Actions[this.Undo.Last].Y1+obj.Y; this.Cursor.X=obj.Y==0?this.Undo.Actions[this.Undo.Last].X1+obj.X:obj.X; this.Select.Select=false;
				break;

			case "INS":
				this.DelSection(this.Undo.Actions[this.Undo.Last].Y1,this.Undo.Actions[this.Undo.Last].X1,this.Undo.Actions[this.Undo.Last].Y2,this.Undo.Actions[this.Undo.Last].X2,true);
				this.Cursor.Y=this.Undo.Actions[this.Undo.Last].Y1; this.Cursor.X=this.Undo.Actions[this.Undo.Last].X1; this.Select.Select=false;
				break;
		}
	}

	this.callRedo=function()
	{
		if (this.Undo.Last==this.Undo.Actions.length) return;
		
		switch(this.Undo.Actions[this.Undo.Last].Tipo)
		{
			case "DEL":
				this.DelSection(this.Undo.Actions[this.Undo.Last].Y1,this.Undo.Actions[this.Undo.Last].X1,this.Undo.Actions[this.Undo.Last].Y2,this.Undo.Actions[this.Undo.Last].X2,true);
				this.Cursor.Y=this.Undo.Actions[this.Undo.Last].Y1; this.Cursor.X=this.Undo.Actions[this.Undo.Last].X1; this.Select.Select=false;
				break;

			case "INS":
				var obj=this.InsertText(this.Undo.Actions[this.Undo.Last].Y1,this.Undo.Actions[this.Undo.Last].X1,this.Undo.Actions[this.Undo.Last].Text,true);
				this.Cursor.Y=this.Undo.Actions[this.Undo.Last].Y1+obj.Y; this.Cursor.X=obj.Y==0?this.Undo.Actions[this.Undo.Last].X1+obj.X:obj.X; this.Select.Select=false;
				break;
		}
		this.Undo.Last++;
	}

	this.GetText=function()
	{
		var retval=new Array();
		
		for (y=0; y<this.Text.Lineas.length; y++)
			retval.push(this.Text.Lineas[y].Text);
		
		return retval.join("\r\n");
	}

	this.SetText=function(Text)
	{
		var y,aText;

		Text=Replace(Text,"\r","");
		Text=Replace(Text,"\t","  ");
		this.Text.Text=Text;
		this.Text.Lineas=new Array();
		this.Text.Size=0;
		
		aText=Text.split("\n");
		for (y=0; y<aText.length; y++)
		{
			//Agrego la Linea
			this.Text.Lineas[y]=new clsLinea(aText[y]);
			this.Text.Size+=1+aText[y].length;
		}

		if (this.Syntax!=null) 
			this.Syntax.Check(0,this.Text,this.Screen);

		//Verificaciones Iniciales
		this.Check();
		this.RefreshToolBar();
		this.RefreshText();
		this.RefreshScreen();
	}
	
	this.ReadOnly=function()
	{
		this.readOnly=true;
		window.setTimeout("Editor.Initialize2(false);",500);
	}

	this.RefreshText=function()
	{
		var x,xr,y,yr;
		var Line,Color,L;
		for (y=this.Screen.ScrollY, yr=0; yr<this.Screen.SizeY; y++, yr++)
		{
			Line =(y<this.Text.Lineas.length?this.Text.Lineas[y].Text :""); L=Line.length;
			Color=(y<this.Text.Lineas.length?this.Text.Lineas[y].Color:"");
			
			for (x=this.Screen.ScrollX, xr=0; xr<this.Screen.SizeX; x++, xr++)
			{
				if (x<L)
				{
					this.Data[yr][xr].Text  =Line.substr(x,1);
					this.Data[yr][xr].Style=Color.substr(x,1);
					
					if (!this.Select.Select)
						this.Data[yr][xr].Back="N";
					else
						this.Data[yr][xr].Back=((this.Select.Y1<y || (this.Select.Y1==y && this.Select.X1<=x)) && (this.Select.Y2>y || (this.Select.Y2==y && this.Select.X2>x)))?"S":"N";
				} else
				{
					this.Data[yr][xr].Text="";
					this.Data[yr][xr].Style="N";
					if (!this.Select.Select)
						this.Data[yr][xr].Back="N";
					else
						this.Data[yr][xr].Back=((this.Select.Y1<y || (this.Select.Y1==y && this.Select.X1<=x)) && (this.Select.Y2>y || (this.Select.Y2==y && this.Select.X2>x)))?"S":"N";
				}
			}
		}
	}
	
	this.RefreshScreen=function()
	{
		if (!this.Init) return;
		
		var x,y,p,b;
		var Text=new Array();
		var Back=new Array();

		p="N"; b="N";
		for (y=0; y<this.Screen.SizeY; y++)
		{
			Text[y]='<span class="Text'+p+'">'; 
			Back[y]='<span class="Back'+b+'" >'; 
			
			for (x=0; x<this.Screen.SizeX; x++)
			{
				if (p!=this.Data[y][x].Style)
				{
					p=this.Data[y][x].Style;
					Text[y]+='</span><span class="Text'+p+'">';
				}
				if (b!=this.Data[y][x].Back)
				{
					b=this.Data[y][x].Back;
					Back[y]+='</span><span class="Back'+b+'">';
				}
				switch(this.Data[y][x].Text)
				{
					case "":
					case " ":
						Text[y]+="&nbsp;";
						Back[y]+="&nbsp;";
						break;
					case "&":
						Text[y]+="&amp;";
						Back[y]+="&nbsp;";
						break;
					case "<":
						Text[y]+="&lt;";
						Back[y]+="&nbsp;";
						break;
					case ">":
						Text[y]+="&gt;";
						Back[y]+="&nbsp;";
						break;
					default:
						Text[y]+=this.Data[y][x].Text;
						Back[y]+="&nbsp;";
						break;
				}
			}
			Text[y]+="</span>";
			Back[y]+="</span>";
		}
		
		this.Divs.Text.innerHTML=Text.join("<br>");
		this.Divs.Back.innerHTML=Back.join("<br>");
		if (!this.readOnly)
			this.Divs.TextArea.value="";
	}

	/////////////////////////////////////////////////////////////////////////////////
	// EDICION
	this.AddUndo_DelSection=function(Y1,X1,Y2,X2)
	{
		this.Undo.Actions.length=this.Undo.Last;
		
		//Accion
		var newAction=new Object();
		newAction.Tipo="DEL";
		newAction.X1=X1;
		newAction.Y1=Y1;
		newAction.X2=X2;
		newAction.Y2=Y2;
		newAction.Text=this.getSection(Y1,X1,Y2,X2);
		
		//Agrego la Accion
		this.Undo.Actions[this.Undo.Actions.length]=newAction;
		
		//Limito la cantidad de undos
		while(this.Undo.Actions.length>100) this.Undo.Actions.splice(0,1);
		this.Undo.Last=this.Undo.Actions.length;
	}

	this.AddUndo_InsertText=function(Y1,X1,Y2,X2,Text)
	{
		this.Undo.Actions.length=this.Undo.Last;
		
		//Accion
		var newAction=new Object();
		newAction.Tipo="INS";
		newAction.X1=X1;
		newAction.Y1=Y1;
		newAction.X2=X2;
		newAction.Y2=Y2;
		newAction.Text=Text;
		
		//Agrego la Accion
		this.Undo.Actions[this.Undo.Actions.length]=newAction;
		
		//Limito la cantidad de undos
		while(this.Undo.Actions.length>100) this.Undo.Actions.splice(0,1);
		this.Undo.Last=this.Undo.Actions.length;
	}

	this.getSection=function(Y1,X1,Y2,X2)
	{
		var txt="", txt1;
		if (Y1==Y2)
		{
			txt=this.Text.Lineas[Y1].Text;
			txt=txt.substr(X1, X2-X1);
		} else
		{
			var t;
			for (t=Y1; t<=Y2; t++)
			{
				txt1=this.Text.Lineas[t].Text;
				if (t==Y1)
				{
					txt+=txt1.substr(X1)+"\n";
				} else
				if (t==Y2)
				{
					txt+=txt1.substr(0,X2);
				} else
					txt+=txt1+"\n";
			}
		}
		return txt;
	}

	this.DelSection=function(Y1,X1,Y2,X2,isUNDO)
	{
		if (isUNDO?false:true) this.AddUndo_DelSection(Y1,X1,Y2,X2);

		var txt,txt1, t;
		if (Y1==Y2)
		{
			txt=this.Text.Lineas[Y1].Text;
			if (X2>txt.length)
			{
				if (Y1+1<this.Text.Lineas.length)
				{
					this.Text.Size-=(this.Text.Lineas[Y1].Text.length+1+this.Text.Lineas[Y1+1].Text.length+1);
					while(txt.length<X1) txt=txt+" ";
					this.Text.Lineas[Y1].Text=txt+this.Text.Lineas[Y1+1].Text;
					this.Text.Size+=this.Text.Lineas[Y1].Text.length+1;
					this.Text.Lineas.splice(Y1+1,1);
				}
			} else
			{
				txt=txt.substr(0,X1)+txt.substr(X2);
				this.Text.Size-=this.Text.Lineas[Y1].Text.length+1;
				this.Text.Lineas[Y1].Text=txt;
				this.Text.Size+=this.Text.Lineas[Y1].Text.length+1;
			}
		} else
		{
			txt =this.Text.Lineas[Y1].Text;
			txt1=this.Text.Lineas[Y2].Text;

			for(t=Y1; t<=Y2; t++) this.Text.Size-=this.Text.Lineas[t].Text.length+1; this.Text.Size++;
			
			while(txt.length<X1) txt=txt+" ";
			txt=txt.substr(0,X1)+txt1.substr(X2);
			this.Text.Lineas[Y1].Text=txt;
			this.Text.Lineas.splice(Y1+1,Y2-Y1);
			this.Text.Size+=this.Text.Lineas[Y1].Text.length;
		}
		
		//Indico desde donde falta colorer
		if (this.Syntax!=null) 
			this.Syntax.Check(Y1,this.Text,this.Screen);
	}

	this.InsertText=function(Y,X,text,isUNDO)
	{
		var tt, l;
		var txt=Replace(Replace(text,"\r",""),"\t","  ").split("\n");
		var retval=new Object();
		retval.X=0; retval.Y=0;
		
		if (txt.length==1)
		{
			tt=this.Text.Lineas[Y].Text;
			while(tt.length<X) tt=tt+" ";
			
			this.Text.Size-=this.Text.Lineas[Y].Text.length+1;
			this.Text.Lineas[Y].Text=tt.substr(0,X)+txt[0]+tt.substr(X);
			this.Text.Size+=this.Text.Lineas[Y].Text.length+1;

			retval.Y=0;
			retval.X=txt[0].length; 
		} else
		{
			tt=this.Text.Lineas[Y].Text;
			while(tt.length<X) tt=tt+" ";
			
			//Primer linea
			this.Text.Size-=this.Text.Lineas[Y].Text.length+1;
			this.Text.Lineas[Y].Text=tt.substr(0,X)+txt[0];
			this.Text.Size+=this.Text.Lineas[Y].Text.length+1;
			
			//Lineas del medio
			for (var t=1; t<txt.length-1; t++)
			{
				l=new clsLinea(txt[t]);
				this.Text.Lineas.splice(Y+t,0,l);
				this.Text.Size+=this.Text.Lineas[Y+t].Text.length+1;
			}
			
			//Ultima Linea
			l=new clsLinea(txt[txt.length-1]+tt.substr(X));
			this.Text.Lineas.splice(Y+txt.length-1,0,l);
			this.Text.Size+=this.Text.Lineas[Y+txt.length-1].Text.length+1;

			//dimenciones
			retval.Y=txt.length-1;
			retval.X=txt[txt.length-1].length; 
		}

		//Indico desde donde falta colorer
		if (this.Syntax!=null) 
			this.Syntax.Check(Y,this.Text,this.Screen);
		
		if (isUNDO?false:true) this.AddUndo_InsertText(Y,X,Y+retval.Y,retval.Y==0?X+retval.X:retval.X,text);
		return retval;
	}
	
	/////////////////////////////////////////////////////////////////////////////////
	// EVENTOS
	this.MouseDown=function(objEvent)
	{
		this.Cursor.X=this.Screen.ScrollX+Math.round(objEvent.offsetX/this.Scale.X);
		this.Cursor.Y=this.Screen.ScrollY+Math.floor(objEvent.offsetY/this.Scale.Y);
		this.Check(); 
		this.RefreshToolBar();
		this.ShowCursor(false);
	}
	this.MouseUp=function(objEvent)
	{
		if (!this.readOnly)
			this.Divs.TextArea.focus();
	}
		

	this.SetFocus=function() { this.ShowCursor(true); }
	this.LostFocus=function() { this.HideCursor(true); }
	
	this.KeyDown=function(objEvent)
	{
		var sKey="";
		
		if (objEvent.ctrlKey) sKey+="C_";
		if (objEvent.altKey) sKey+="A_";
		//if (objEvent.shiftKey) sKey+="S_";
		sKey+=objEvent.keyCode.toString();
		
		var sX, sY;
		sX=this.Cursor.X;
		sY=this.Cursor.Y;
		
		switch(sKey)
		{
			//Teclas solas...
			case "27": //ESC
				break;
			
			case "40": //DOWN 
				if (this.IntelicencePosition!=0) 
				{
					if (this.IntelicenceSelect<this.arrIntelicence.length-1)
					{
						this.IntelicenceSelect++;
						this.ShowIntelicence(this.arrIntelicence, true);
					} 
					return;
				}
				this.Cursor.Y++; 
				break;
				
			case "38": //UP
				if (this.IntelicencePosition!=0) 
				{
					if (this.IntelicenceSelect!=0)
					{
						this.IntelicenceSelect--;
						this.ShowIntelicence(this.arrIntelicence, true);
					}
					return;
				}
				this.Cursor.Y--; 
				break;
			
			case "39": this.Cursor.X++; break; //RIGHT
			case "37": this.Cursor.X--; break; //LEFT
				
			case "33": //PAGE UP
				if (this.IntelicencePosition!=0) 
				{
					if (this.IntelicenceSelect!=0)
					{
						this.IntelicenceSelect-=6; if (this.IntelicenceSelect<0) this.IntelicenceSelect=0;
						this.ShowIntelicence(this.arrIntelicence, true);
					}
					return;
				}
				this.Cursor.Y-=this.Screen.SizeY-1; 
				break;
			case "34": //PAGE DOWN
				if (this.IntelicencePosition!=0) 
				{
					if (this.IntelicenceSelect<this.arrIntelicence.length-1)
					{
						this.IntelicenceSelect+=6;
						if (this.IntelicenceSelect>this.arrIntelicence.length-1) this.IntelicenceSelect=this.arrIntelicence.length-1;
						this.ShowIntelicence(this.arrIntelicence, true);
					} 
					return;
				}
				this.Cursor.Y+=this.Screen.SizeY-1; 
				break; 

			case "36": this.Cursor.X=0;break; //HOME
			case "35": this.Cursor.X=this.Text.Lineas[this.Cursor.Y].Text.length; break; //FIN

			//Teclas con CTRL...
			case "C_40": this.Screen.ScrollY++; if (this.Cursor.Y<this.Screen.ScrollY                    ) this.Cursor.Y=this.Screen.ScrollY;                  break; //CTRL-UP
			case "C_38": this.Screen.ScrollY--; if (this.Cursor.Y>this.Screen.ScrollY+this.Screen.SizeY-1) this.Cursor.Y=this.Screen.ScrollY+this.Screen.SizeY-1; break; //CTRL-DOWN

			case "C_39": //CTRL-RIGHT
				var txt=this.Text.Lineas[this.Cursor.Y].Text;
				var txtC=txt.substr(this.Cursor.X,1);
				if (this.Cursor.X>=txt.length)
				{ //Caso final de la linea
					if (this.Cursor.Y<this.Text.Lineas.length-1)
					{
						this.Cursor.X=0; 
						this.Cursor.Y++; 
					}
				} else
				if (" ABCDEFGHIJKLMNÑOPQRSTUVWXYZabcdefghijklmnñopqrstuvwxyzáéíóú0123456789".indexOf(txtC)>=0)
				{ //Caso TEXTO
					while("ABCDEFGHIJKLMNÑOPQRSTUVWXYZabcdefghijklmnñopqrstuvwxyzáéíóú0123456789".indexOf(txtC)>=0 && this.Cursor.X<txt.length)
					{
						this.Cursor.X++; 
						txtC=txt.substr(this.Cursor.X,1);
					}
					while(txtC==" " && this.Cursor.X<txt.length)
					{
						this.Cursor.X++; 
						txtC=txt.substr(this.Cursor.X,1);
					}
				} else
				{
					while("ABCDEFGHIJKLMNÑOPQRSTUVWXYZabcdefghijklmnñopqrstuvwxyzáéíóú0123456789".indexOf(txtC)<0 && this.Cursor.X<txt.length)
					{
						this.Cursor.X++; 
						txtC=txt.substr(this.Cursor.X,1);
					}
				}
				break; 
			case "C_37": //CTRL-LEFT
				if (this.Cursor.X==0)
				{ //Caso final de la linea
					if (this.Cursor.Y>0)
					{
						this.Cursor.Y--; 
						this.Cursor.X=this.Text.Lineas[this.Cursor.Y].Text.length; 
					}
				} else
				{
					var txt=this.Text.Lineas[this.Cursor.Y].Text;
					var txtC=txt.substr(this.Cursor.X-1,1);
					
					if (" ABCDEFGHIJKLMNÑOPQRSTUVWXYZabcdefghijklmnñopqrstuvwxyzáéíóú0123456789".indexOf(txtC)>=0)
					{ //Caso TEXTO
						while(txtC==" " && this.Cursor.X>0)
						{
							this.Cursor.X--; 
							txtC=txt.substr(this.Cursor.X-1,1);
						}
						while("ABCDEFGHIJKLMNÑOPQRSTUVWXYZabcdefghijklmnñopqrstuvwxyzáéíóú0123456789".indexOf(txtC)>=0 && this.Cursor.X>0)
						{
							this.Cursor.X--; 
							txtC=txt.substr(this.Cursor.X-1,1);
						}
					} else
					{
						while("ABCDEFGHIJKLMNÑOPQRSTUVWXYZabcdefghijklmnñopqrstuvwxyzáéíóú0123456789".indexOf(txtC)<0 && this.Cursor.X>0)
						{
							this.Cursor.X--; 
							txtC=txt.substr(this.Cursor.X-1,1);
						}
					}
				}
				break; 
			
			case "C_36": this.Cursor.Y=0; this.Cursor.X=0; break; //CTRL-HOME
			case "C_35": this.Cursor.Y=this.Text.Lineas.length-1; this.Cursor.X=this.Text.Lineas[this.Cursor.Y].Text.length; break; //CTRL-FIN

			//Otras
			case "45": this.Cursor.Insert=!this.Cursor.Insert; break; //INSERT
			
			//Copiar
			case "C_45": case "C_67":
				if (this.Select.Select)
				{
					this.copyToClipboard(this.getSection(this.Select.Y1,this.Select.X1,this.Select.Y2,this.Select.X2));
					this.Select.Select=false;
				}			
				break;

			//Cortar
			case "C_88":
				if (this.Select.Select)
				{
					this.copyToClipboard(this.getSection(this.Select.Y1,this.Select.X1,this.Select.Y2,this.Select.X2));
					this.DelSection(this.Select.Y1,this.Select.X1,this.Select.Y2,this.Select.X2);
					this.Cursor.Y=this.Select.Y1; this.Cursor.X=this.Select.X1; this.Select.Select=false;
				}			
				break;

			//Pegar
			case "C_86":
				if (this.Select.Select)
				{   //Elimino el texto selecionado
					this.DelSection(this.Select.Y1,this.Select.X1,this.Select.Y2,this.Select.X2);
					this.Cursor.Y=this.Select.Y1; this.Cursor.X=this.Select.X1; this.Select.Select=false;
				}	

				//Inserto el texto del clipboard
				var obj=this.InsertText(this.Cursor.Y,this.Cursor.X,this.readToClipboard());
				this.Cursor.Y+=obj.Y; this.Cursor.X=obj.Y==0?this.Cursor.X+obj.X:obj.X; 
				break;


			//Rehacer
			case "C_89":
				this.callRedo();
				break;

				//Deshacer
			case "C_90":
				this.callUndo();
				break;

			////////////////////////////////////////////////////////////////////////////////////////////////////
			//Modifican el Texto
			case "46": //SUPRIMIR
				if (this.Select.Select)
				{
					this.DelSection(this.Select.Y1,this.Select.X1,this.Select.Y2,this.Select.X2);
					this.Cursor.Y=this.Select.Y1; this.Cursor.X=this.Select.X1; this.Select.Select=false;
				} else
				{
					this.DelSection(this.Cursor.Y,this.Cursor.X,this.Cursor.Y,this.Cursor.X+1);
				}
				this.Select.Select=false;
				break;
			
			case "8": //BACKSPACE 
				if (this.Select.Select)
				{
					this.DelSection(this.Select.Y1,this.Select.X1,this.Select.Y2,this.Select.X2);
					this.Cursor.Y=this.Select.Y1; this.Cursor.X=this.Select.X1; this.Select.Select=false;
				} else
				{
					if (this.Cursor.X==0)
					{
						if (this.Cursor.Y>0)
						{
							this.Cursor.Y--; this.Cursor.X=this.Text.Lineas[this.Cursor.Y].Text.length;
							this.DelSection(this.Cursor.Y,this.Cursor.X,this.Cursor.Y,this.Cursor.X+1);
						}
					} else
					{
						if (this.Cursor.X<=this.Text.Lineas[this.Cursor.Y].Text.length)
						{
							this.Cursor.X--;
							this.DelSection(this.Cursor.Y,this.Cursor.X,this.Cursor.Y,this.Cursor.X+1);
						} else
							this.Cursor.X--;
					}
				}
				this.Select.Select=false;
				break;
		}

		this.Check(); 
		this.RefreshToolBar();
		if (objEvent.shiftKey) 
		{
			switch(sKey)
			{
				//Teclas solas...
				case "40": //UP
				case "38": //DOWN
				case "39": //RIGHT
				case "37": //LEFT
					
				case "33": //PAGE UP
				case "34": //PAGE DOWN

				case "36": //HOME
				case "35": //FIN

				case "C_40": //CTRL-UP
				case "C_38": //CTRL-DOWN
				
				case "C_39": //CTRL-RIGHT
				case "C_37": //CTRL-LEFT
				case "C_36": //CTRL-HOME
				case "C_35": //CTRL-FIN
					if (!this.Select.Select)
					{
						this.Select.Select=true;
						this.Select.Size=0;
						this.Select.X=sX;
						this.Select.Y=sY;
					} 
					
					if ((this.Cursor.Y>this.Select.Y) || (this.Cursor.Y==this.Select.Y && this.Cursor.X>=this.Select.X))
					{
						this.Select.Y1=this.Select.Y;
						this.Select.Y2=this.Cursor.Y;
						this.Select.X1=this.Select.X;
						this.Select.X2=this.Cursor.X;
					} else
					{
						this.Select.Y1=this.Cursor.Y;
						this.Select.Y2=this.Select.Y;
						this.Select.X1=this.Cursor.X;
						this.Select.X2=this.Select.X;
					}
					if (this.Select.Y1==this.Select.Y2)
					{
						this.Select.Size=(this.Text.Lineas[this.Select.Y2].Text.length>this.Select.X2?this.Select.X2:this.Text.Lineas[this.Select.Y2].Text.length)-(this.Text.Lineas[this.Select.Y2].Text.length>this.Select.X1?this.Select.X1:this.Text.Lineas[this.Select.Y2].Text.length);
					} else
					{
						//Linea Inicial
						this.Select.Size=3+(this.Text.Lineas[this.Select.Y1].Text.length>this.Select.X1?this.Text.Lineas[this.Select.Y2].Text.length-this.Select.X1:-2);

						//linea intermedias
						for(t=this.Select.Y1+1; t<=this.Select.Y2-1; t++) 
							this.Select.Size+=this.Text.Lineas[t].Text.length+1;
							
						//Linea Final
						this.Select.Size+=(this.Text.Lineas[this.Select.Y2].Text.length>this.Select.X2?this.Select.X2:this.Text.Lineas[this.Select.Y2].Text.length);
					}
					break;
			}
		} else
		{
			switch(sKey)
			{
				//Teclas solas...
				case "40": //UP
				case "38": //DOWN
				case "39": //RIGHT
				case "37": //LEFT
					
				case "33": //PAGE UP
				case "34": //PAGE DOWN

				case "36": //HOME
				case "35": //FIN

				case "C_40": //CTRL-UP
				case "C_38": //CTRL-DOWN
				
				case "C_39": //CTRL-RIGHT
				case "C_37": //CTRL-LEFT
				case "C_36": //CTRL-HOME
				case "C_35": //CTRL-FIN
					this.Select.Select=false;
			}
		}
		
		//Indico desde donde falta colorear
		if (this.Syntax!=null) 
			this.Syntax.Check(this.Text.Lineas.length,this.Text,this.Screen);
			
		this.RefreshText();
		this.RefreshScreen();
		this.ShowCursor(false);
		
		objEvent.cancelBubble = true;
	}

	this.KeyPress=function(objEvent)
	{
		var MyCode;
		if (window.ActiveXObject)
		{
			MyCode=objEvent.keyCode;
		} else
		{
			MyCode=objEvent.charCode;
			if (MyCode?false:true) 
			{
				MyCode=objEvent.keyCode;
				if (MyCode>32) return;
			}
		}
		var MyKey=String.fromCharCode(MyCode);

		//hay algo marcado?
		if ((MyCode==13) || (MyCode>=32))
		{
			if (this.Select.Select)
			{
				this.DelSection(this.Select.Y1,this.Select.X1,this.Select.Y2,this.Select.X2);
				this.Cursor.Y=this.Select.Y1; this.Cursor.X=this.Select.X1; 
				this.Select.Select=false;
			} 
			
			if (this.Syntax!=null && this.Syntax.KeyPress(MyKey, this))
			{
				this.Check(); 
				this.RefreshToolBar();
				this.RefreshText();
				this.RefreshScreen();
				this.ShowCursor(false);
				return;
			}
		}

		if (MyCode==13)
		{
			this.InsertText(this.Cursor.Y,this.Cursor.X,"\n                            ".substr(0,1+2*this.Text.Lineas[this.Cursor.Y].Deep));
			this.Cursor.X=0; this.Cursor.Y++; 

			//Sangria automatica
			this.Cursor.X+=2*this.Text.Lineas[this.Cursor.Y].Deep;
			
			this.Check(); 
			this.RefreshToolBar();
			this.RefreshText();
			this.RefreshScreen();

		} else
		if (MyCode>=32)
		{
			if (!this.Cursor.Insert && this.Cursor.X<this.Text.Lineas[this.Cursor.Y].Text.length) this.DelSection(this.Cursor.Y,this.Cursor.X,this.Cursor.Y,this.Cursor.X+1);
			this.InsertText(this.Cursor.Y,this.Cursor.X,MyKey);
			this.Cursor.X++; 

			this.Check(); 
			this.RefreshToolBar();
			this.RefreshText();
			this.RefreshScreen();
			
		}

	  
	  this.ShowCursor(false);
	}

	
	this.Initialize();
}
using System;
using System.Xml;
using System.IO;
using System.Collections;

using System.Reflection;
using Microsoft.CSharp;
using System.CodeDom.Compiler;

using Nomad.NSystem.Document;
using Nomad.NSystem.Proxy;
using Nomad.NSystem.Base;
using Nomad.NSystem.Functions;

namespace NucleusWF.Base.Definicion.Workflows
{

    //////////////////////////////////////////////////////////////////////////////////
    //Clase WorkFlows
    public partial class TEXT : Nomad.NSystem.Base.NomadObject
    {

    public static bool CheckText(string TEXT, WF wf, ref string retError)
    {
      //Tokenizo la Cadena
      ArrayList tkzTEXT=Nomad.Base.Report.Text.Tokenize(TEXT);
      string Result,Child,TextChild;
      NomadXML GROUP, xmlVARS;

      xmlVARS=WF.VariablesWF(wf);

      //Genero el RESULTADO
      foreach(string Formula in tkzTEXT)
      {
        switch(Formula.Substring(0,1))
        {
          case "T": //Texto COMUN
            break;

          case "?":
          case "0":
          case "1":
          case "F": //Formula SIMPLE
            retError=Nomad.Base.Report.Formula.Validate(Formula.Substring(2), null, xmlVARS);
            if (retError!="OK")
            {
              retError=retError.Substring(2);
              return false;
            }
            break;

          case "E":
            Result=Formula.Substring(2);
            Child    =Result.Substring(0,Result.IndexOf(":"));
            TextChild=Result.Substring(Result.IndexOf(":")+1);

            GROUP=xmlVARS.FindElement("DATA").FindElement(Child);
            if (GROUP==null)
            {
              retError="Grupo de Variables "+Child+" no encontrado, o no es CHILD.";
              return false;
            }

             //Analizo el Contenido
            if (!CheckText(TextChild, wf, ref retError))
              return false;
            break;

          case "I":
            Result=Formula.Substring(2);
            Child    =Result.Substring(0,Result.IndexOf(":"));
            TextChild=Result.Substring(Result.IndexOf(":")+1);

            //Analizo la Formula
            retError=Nomad.Base.Report.Formula.Validate(Child, null, xmlVARS);
            if (retError!="OK")
            {
              retError=retError.Substring(2);
              return false;
            }

             //Analizo el Contenido
            if (!CheckText(TextChild, wf, ref retError))
              return false;
            break;
        }
      }
      return true;
    }

    public static TEXT CopyFrom(TEXT rptO, WF wf, Hashtable Texts)
    {
      string Err="";

      //Validaciones
      if (!WF.CODE_TEST(rptO.c_text))
        throw new NomadException("El Nombre del Reporte '"+rptO.c_text+"' tiene Caracteres Invalidos....");

      if (Texts.ContainsKey(rptO.c_text.ToUpper()))
        throw new NomadException("El Reporte '"+rptO.c_text+"' esta Duplicado....");

      if (!CheckText(rptO.o_text+rptO.o_text2+rptO.o_text3+rptO.o_text4, wf, ref Err))
        throw new NomadException("El Reporte '"+rptO.c_text+"' "+Err);

      //Creo el Grupo de Variables
      TEXT rptP=new TEXT();
      rptP.c_text =rptO.c_text;
      rptP.c_tipo =rptO.c_tipo;
      rptP.o_text =rptO.o_text;
      rptP.o_text2=rptO.o_text2;
      rptP.o_text3=rptO.o_text3;
      rptP.o_text4=rptO.o_text4;
      Texts[rptO.c_text.ToUpper()]=rptP;

      return rptP;
    }

    public string GetTextResult(NomadXML xmlVARS)
    {
      string TEXT=this.o_text+this.o_text2+this.o_text3+this.o_text4;
      return Nomad.Base.Report.Text.Resolve(TEXT, xmlVARS);
    }
  }
}



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
    public partial class VAR_GROUP : Nomad.NSystem.Base.NomadObject
    {

    public static VAR_GROUP CopyFrom(VAR_GROUP grpO, Hashtable Variables)
    {

      //Validaciones
      if (!WF.CODE_TEST(grpO.c_var_group))
        throw new NomadException("El Nombre del Grupo de Variables '"+grpO.c_var_group+"' tiene Caracteres Invalidos....");

      if (Variables.ContainsKey(grpO.c_var_group.ToUpper()+".*"))
        throw new NomadException("El Grupo de Variables '"+grpO.c_var_group+"' esta Duplicado....");

      if (!("|MAIN|HIST|CHILD|AUX|ARRAUX|").Contains("|"+grpO.c_mode+"|"))
        throw new NomadException("El MODO del Grupo de Variables '"+grpO.c_var_group+"' no es Valido....");

      //Creo el Grupo de Variables
      VAR_GROUP grpP=new VAR_GROUP();
      grpP.c_var_group=grpO.c_var_group;
      grpP.d_var_group=grpO.d_var_group;
      grpP.c_mode     =grpO.c_mode;
      Variables[grpO.c_var_group.ToUpper()+".*"]=grpP;

      string collist1, collist2;
      Hashtable colUsed=new Hashtable();

      //Recorro las variables y las Agrego.....
      foreach(VAR varO in grpO.VARS)
      {
        NomadLog.Info("Agregando la Variable '"+grpO.c_var_group+"."+varO.c_var+"' ....");

        if (!WF.CODE_TEST(varO.c_var))
          throw new NomadException("El Nombre de la Variable '"+grpO.c_var_group+"."+varO.c_var+"' tiene Caracteres Invalidos....");

        //Validaciones
        if (Variables.ContainsKey(grpO.c_var_group.ToUpper()+"."+varO.c_var.ToUpper()))
          throw new NomadException("La Variable '"+grpO.c_var_group+"."+varO.c_var+"' esta Duplicada....");

        if (!("|bool|int|double|date|string|enum|fk|text|file|").Contains("|"+varO.c_type+"|"))
          throw new NomadException("El TIPO de Variable '"+grpO.c_var_group+"."+varO.c_var+"' no es Valido....");

        //Validaciones segun el Tipo de Grupo de Variables
        if (grpO.c_mode=="MAIN" || grpO.c_mode=="HIST" || grpO.c_mode=="CHILD")
        {
          switch(varO.c_type)
          {
            case "bool":
            case "int":
              collist1="|e_valor_1|e_valor_2|e_valor_3|e_valor_4|e_valor_5|";
              collist2="";
              break;
            case "double":
              collist1="|n_valor_1|n_valor_2|n_valor_3|";
              collist2="";
              break;
            case "date":
              collist1="|d_valor_1|d_valor_2|d_valor_3|";
              collist2="";
              break;
            case "string":
              collist1="|c_valor_1|c_valor_2|c_valor_3|c_valor_4|c_valor_5|c_valor_6|c_valor_7|c_valor_8|c_valor_9|c_valor_10|";
              collist2="";
              break;
            case "enum":
              collist1="|c_valor_1|c_valor_2|c_valor_3|c_valor_4|c_valor_5|c_valor_6|c_valor_7|c_valor_8|c_valor_9|c_valor_10|";
              collist2="";
              break;
            case "fk":
              collist1="|e_valor_1|e_valor_2|e_valor_3|e_valor_4|e_valor_5|";
              collist2="|c_valor_1|c_valor_2|c_valor_3|c_valor_4|c_valor_5|c_valor_6|c_valor_7|c_valor_8|c_valor_9|c_valor_10|";
              break;
            case "text":
              collist1="|o_valor_1|o_valor_2|";
              collist2="";
              break;
            case "file":
              collist1="|oi_file_valor_1|oi_file_valor_2|";
              collist2="";
              break;
            default:
              collist1="";
              collist2="";
              break;
          }

          if (colUsed.ContainsKey(varO.c_column))
            throw new NomadException("La Columna asignada para '"+grpO.c_var_group+"."+varO.c_var+"' ya esta Asignada para otra Variable....");

          if (!collist1.Contains("|"+varO.c_column+"|"))
            throw new NomadException("La Columna asignada para '"+grpO.c_var_group+"."+varO.c_var+"' no es Valida....");

          if (collist2!="")
          {
            if (colUsed.ContainsKey(varO.c_column_desc))
              throw new NomadException("La Columna asignada para la Descripcion de '"+grpO.c_var_group+"."+varO.c_var+"' ya esta Asignada para otra Variable....");

            if (!collist2.Contains("|"+varO.c_column_desc+"|"))
              throw new NomadException("La Columna asignada para la Descripcion de '"+grpO.c_var_group+"."+varO.c_var+"' no es Valida....");
          }
        }

        //Validaciones Adicionales
        switch(varO.c_type)
        {
          case "fk":
            switch(varO.d_type_fk.Split(':')[0])
            {
              case "IM":
                string c_list_mode=NomadEnvironment.QueryValue("WRK04_INT_MAN","c_list_mode","c_int_mag",varO.d_type_fk.Split(':')[1],"",false);
                if (c_list_mode=="")
                  throw new NomadException("El tipo de clase externa '"+varO.d_type_fk+"' para '"+grpO.c_var_group+"."+varO.c_var+"' no es Valido....");

                if (c_list_mode!="SIMPLE")
                  throw new NomadException("El tipo de clase externa '"+varO.d_type_fk+"' para '"+grpO.c_var_group+"."+varO.c_var+"' no puede ser usando en una Varible....");
                break;

              default:
                throw new NomadException("El tipo de clase externa '"+varO.d_type_fk+"' para '"+grpO.c_var_group+"."+varO.c_var+"' no es Valido....");
            }

            break;

          case "enum":
            Hashtable vals=new Hashtable();

            foreach(VAR_COMBO valO in varO.VARS_COMBO)
            {
              if (!WF.CODE_TEST(varO.c_var))
                throw new NomadException("El valor '"+varO.c_var+"' para la Variable '"+grpO.c_var_group+"."+varO.c_var+"' tiene Caracteres Invalidos....");

              //Validaciones
              if (Variables.ContainsKey(grpO.c_var_group.ToUpper()+"."+varO.c_var.ToUpper()))
                throw new NomadException("El valor '"+varO.c_var+"' para la Variable '"+grpO.c_var_group+"."+varO.c_var+"' esta Duplicado....");

              //Agrego el VALOR
              vals[valO.c_var_combo]=valO;
            }
            break;
        }

        if (varO.o_formula!="")
        {
          string Err=Nomad.Base.Report.Formula.Validate(varO.o_formula, null, WF.VariablesWF(null));
          if (Err!="OK") throw new NomadException("La formula para la Variable '"+grpO.c_var_group+"."+varO.c_var+"' "+Err);
        }

        //Agrego la Variable al Grupo
        NomadObject varP=varO.Duplicate();
        grpP.VARS.Add(varP);

        Variables[grpO.c_var_group.ToUpper()+"."+varO.c_var.ToUpper()]=varP;
      }

      return grpP;
    }

   }
}



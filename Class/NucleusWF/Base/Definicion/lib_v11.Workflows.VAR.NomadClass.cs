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
    public partial class VAR : Nomad.NSystem.Base.NomadObject
    {
        public static void SAVE_DDO(string oi_var_group, NucleusWF.Base.Definicion.Workflows.VAR DDO)
        {
            ////////////////////////////////////////////	
            //Verificar CODIGO
            DDO.c_var = DDO.c_var.ToUpper();

            char[] cv = DDO.c_var.ToCharArray();
            for (int t = 0; t < cv.Length; t++)
            {
                //verifica si es una letra
                if (cv[t] >= 'A' && cv[t] <= 'Z') continue;
                if (cv[t] == '_') continue;

                //verifico si es un numero
                if (cv[t] >= '0' && cv[t] <= '9' && t != 0) continue;

                throw new NomadException("El nombre de la Variable no es Valido, solo acepta caraceteres Alfanumericos.");
            }


            if (DDO.IsForInsert)
            {
                Hashtable ListColumns;
                string collist1 = "", collist2 = "";
                VAR_GROUP VG = VAR_GROUP.Get(oi_var_group, false);

                ////////////////////////////////////////////	
                //Actualizo el ORDEN	
                int e_order = 0;
                foreach (VAR MyVAR in VG.VARS)
                    if (MyVAR.e_order > e_order)
                        e_order = MyVAR.e_order;
                DDO.e_order = e_order + 1;


                if (VG.c_mode == "MAIN" || VG.c_mode == "HIST" || VG.c_mode == "CHILD")
                {
                    ////////////////////////////////////////////	
                    //Calculo la Columna donde Almacear los Datos
                    switch (DDO.c_type)
                    {
                        case "bool":
                        case "int":
                            collist1 = "e_valor_1|e_valor_2|e_valor_3|e_valor_4|e_valor_5";
                            collist2 = "";
                            break;
                        case "double":
                            collist1 = "n_valor_1|n_valor_2|n_valor_3";
                            collist2 = "";
                            break;
                        case "date":
                            collist1 = "d_valor_1|d_valor_2|d_valor_3";
                            collist2 = "";
                            break;
                        case "string":
                            collist1 = "c_valor_1|c_valor_2|c_valor_3|c_valor_4|c_valor_5|c_valor_6|c_valor_7|c_valor_8|c_valor_9|c_valor_10";
                            collist2 = "";
                            break;
                        case "enum":
                            collist1 = "c_valor_1|c_valor_2|c_valor_3|c_valor_4|c_valor_5|c_valor_6|c_valor_7|c_valor_8|c_valor_9|c_valor_10";
                            collist2 = "";
                            break;
                        case "fk":
                            collist1 = "e_valor_1|e_valor_2|e_valor_3|e_valor_4|e_valor_5";
                            collist2 = "c_valor_1|c_valor_2|c_valor_3|c_valor_4|c_valor_5|c_valor_6|c_valor_7|c_valor_8|c_valor_9|c_valor_10";
                            break;
                        case "text":
                            collist1 = "o_valor_1|o_valor_2";
                            collist2 = "";
                            break;
                        case "file":
                            collist1 = "oi_file_valor_1|oi_file_valor_2";
                            collist2 = "";
                            break;
                    }

                    //Analizo collist 1
                    ListColumns = new Hashtable();
                    foreach (string key in collist1.Split('|'))
                        ListColumns[key] = 1;
                    foreach (VAR MyVAR in VG.VARS)
                    {
                        if (ListColumns.ContainsKey(MyVAR.c_column))
                            ListColumns.Remove(MyVAR.c_column);
                        if (ListColumns.ContainsKey(MyVAR.c_column_desc))
                            ListColumns.Remove(MyVAR.c_column_desc);
                    }
                    if (ListColumns.Count == 0)
                        throw new NomadException("No se puede agregar ese tipo de Variables, CREE un nuevo Grupo.");
                    foreach (string key in ListColumns.Keys) { DDO.c_column = key; break; }

                    //Analizo collist 2
                    if (collist2 != "")
                    {
                        ListColumns = new Hashtable();
                        foreach (string key in collist2.Split('|'))
                            ListColumns[key] = 1;
                        foreach (VAR MyVAR in VG.VARS)
                        {
                            if (ListColumns.ContainsKey(MyVAR.c_column))
                                ListColumns.Remove(MyVAR.c_column);
                            if (ListColumns.ContainsKey(MyVAR.c_column_desc))
                                ListColumns.Remove(MyVAR.c_column_desc);
                        }
                        if (ListColumns.Count == 0)
                            throw new NomadException("No se puede agregar ese tipo de Variables, CREE un nuevo Grupo.");
                        foreach (string key in ListColumns.Keys) { DDO.c_column_desc = key; break; }
                    }
                }


                //Guardo el Padre                             
                VG.VARS.Add(DDO);
                NomadEnvironment.GetCurrentTransaction().Save(VG);
            }
            else
            {
                //Guardo el Objeto
                NomadEnvironment.GetCurrentTransaction().Save(DDO);
            }
            return;
        }

        public static void MOVE(int oi_var, string pos)
        {
            //Obtengo la Variable
            VAR MyVAR = VAR.Get(oi_var.ToString(), false);
            VAR MyOTR;
            int varOrder = MyVAR.e_order;

            //Obtengo el GRUPO DE VARIABLES
            VAR_GROUP GROUP = VAR_GROUP.Get(MyVAR.oi_var_group.ToString(), true);

            //Caso
            switch (pos)
            {
                case "FIRST":
                    int min = 999;
                    foreach (VAR cur in GROUP.VARS)
                        if (cur.e_order < min)
                            min = cur.e_order;

                    foreach (VAR cur in GROUP.VARS)
                    {
                        if (cur.e_order == varOrder) cur.e_order = min;
                        else
                            if (cur.e_order < varOrder) cur.e_order++;
                    }
                    break;

                case "UP":
                    int prev = 0;
                    foreach (VAR cur in GROUP.VARS)
                        if (cur.e_order > prev && cur.e_order < varOrder)
                            prev = cur.e_order;

                    if (prev != 0)
                    {
                        MyVAR = (VAR)GROUP.VARS.GetByAttribute("e_order", prev);
                        MyOTR = (VAR)GROUP.VARS.GetByAttribute("e_order", varOrder);
                        MyVAR.e_order = varOrder;
                        MyOTR.e_order = prev;
                    }
                    break;

                case "DOWN":
                    int next = 999;
                    foreach (VAR cur in GROUP.VARS)
                        if (cur.e_order < next && cur.e_order > varOrder)
                            next = cur.e_order;

                    if (next != 999)
                    {
                        MyVAR = (VAR)GROUP.VARS.GetByAttribute("e_order", next);
                        MyOTR = (VAR)GROUP.VARS.GetByAttribute("e_order", varOrder);
                        MyVAR.e_order = varOrder;
                        MyOTR.e_order = next;
                    }
                    break;

                case "LAST":
                    int max = 0;
                    foreach (VAR cur in GROUP.VARS)
                        if (cur.e_order > max)
                            max = cur.e_order;

                    foreach (VAR cur in GROUP.VARS)
                    {
                        if (cur.e_order == varOrder) cur.e_order = max;
                        else
                            if (cur.e_order > varOrder) cur.e_order--;
                    }
                    break;
            }

            //Guardo el OBJETO
            NomadEnvironment.GetCurrentTransaction().Save(GROUP);
        }
    }
}

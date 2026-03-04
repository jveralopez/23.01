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

namespace NucleusRH.Base.Tiempos_Trabajados.IOInterfaceFichadas
{

    //////////////////////////////////////////////////////////////////////////////////
    //Clase FichadasIngresadas
    public partial class FICHADASING : Nomad.NSystem.Base.NomadObject
    {

        public static void ImportarFichadas(int oi_terminal)
        {
            //Codigo en .NET
            NomadBatch b = NomadBatch.GetBatch("Incorporar Fichadas Login/Logout...", "Incorporar Fichadas Login/Logout...");

            string lasssecc;
            int ficadd1, ficadd2, ficrec, ficerr, ficdup1, ficdup2, linea, oi_personal, totRegs;
            NucleusRH.Base.Tiempos_Trabajados.FichadasIng.FICHADASING ddoFichIng1;
            NucleusRH.Base.Tiempos_Trabajados.FichadasIng.FICHADASING ddoFichIng2;
            FICHADASING intFichIng;

            //Cargando el Query.
            NomadXML MyROW;
            NomadXML result;
            NomadXML MyXML = new NomadXML();
            MyXML.SetText(NomadProxy.GetProxy().SQLService().Get(FICHADASING.Resources.qry_fichadas, ""));
            MyXML = MyXML.FirstChild();

            //Contando la Cantidad de ROWS
            totRegs = MyXML.ChildLength;

            //Recorre los registros
            b.SetMess("Incorporando Fichadas...");
            b.Log("Incorporando Fichadas...");
            linea = 0; ficadd1 = 0; ficadd2 = 0; ficrec = 0; ficerr = 0; ficdup1 = 0; ficdup2 = 0; lasssecc = "";
            for (linea = 1, MyROW = MyXML.FirstChild(); MyROW != null; linea++, MyROW = MyROW.Next())
            {
                b.SetPro(0, 90, totRegs, linea);
                b.SetMess("Procesando la Linea " + linea + " de " + totRegs);
                try
                {
                    lasssecc = "creando fichada de entrada";
                    ddoFichIng1 = new NucleusRH.Base.Tiempos_Trabajados.FichadasIng.FICHADASING();
                    lasssecc = "creando fichada de salida";
                    ddoFichIng2 = new NucleusRH.Base.Tiempos_Trabajados.FichadasIng.FICHADASING();

                    lasssecc = "leyendo interface";
                    intFichIng = FICHADASING.Get(MyROW.GetAttr("id"));

                    //FIJAS
                    lasssecc = "asignando valores fijos";
                    ddoFichIng1.c_origen = "T";
                    ddoFichIng2.c_origen = "T";
                    ddoFichIng1.oi_terminal = oi_terminal.ToString();
                    ddoFichIng2.oi_terminal = oi_terminal.ToString();
                    ddoFichIng1.c_estado = "P";
                    ddoFichIng2.c_estado = "P";

                    //PERSONAL
                    lasssecc = "asignando personal";
                    lasssecc = "asignando personal " + intFichIng.nro_tarjeta;
                    oi_personal = NucleusRH.Base.Tiempos_Trabajados.FichadasIng.FICHADASING.GetPersonalEmpresaIDFromCard(intFichIng.nro_tarjeta);
                   
                    
                    if (oi_personal == 0)
                    {
                        ficrec++;
                        b.Wrn("Legajo " + intFichIng.nro_tarjeta + " no encontrado... Linea " + linea.ToString());
                        continue;
                    }
                    ddoFichIng1.oi_personal_emp = oi_personal.ToString(); //Tengo que objtener el PERSONAL
                    ddoFichIng2.oi_personal_emp = oi_personal.ToString(); //Tengo que objtener el PERSONAL

                    NomadXML param = new NomadXML("PARAM");
                    param.SetAttr("d_nro_tarjeta", intFichIng.nro_tarjeta);
                    result = NomadEnvironment.QueryNomadXML(NucleusRH.Base.Tiempos_Trabajados.IOInterfaceFichadas.FICHADASING.Resources.qry_nro_legajo, param.ToString());
                    ddoFichIng1.e_numero_legajo = result.FirstChild().FirstChild().GetAttrInt("e_numero_legajo"); //Legajo>
                    ddoFichIng2.e_numero_legajo = result.FirstChild().FirstChild().GetAttrInt("e_numero_legajo"); //Legajo>

                    //HORA
                    lasssecc = "asignando fecha/hora ";
                    ddoFichIng1.f_fechahora = intFichIng.fecha_login;
                    ddoFichIng2.f_fechahora = intFichIng.fecha_logout;
                    ddoFichIng1.c_fichadasing = Nomad.NSystem.Functions.StringUtil.date2str(ddoFichIng1.f_fechahora) + ddoFichIng1.e_numero_legajo.ToString();
                    ddoFichIng2.c_fichadasing = Nomad.NSystem.Functions.StringUtil.date2str(ddoFichIng2.f_fechahora) + ddoFichIng2.e_numero_legajo.ToString();

                    //TIPO
                    lasssecc = "asignando evento";
                    if (!ddoFichIng1.f_fechahoraNull && !ddoFichIng2.f_fechahoraNull)
                    {
                        ddoFichIng1.l_entrada = true;
                        ddoFichIng1.c_fichadasing += "AE";
                        ddoFichIng1.c_tipo = "E";
                        ddoFichIng2.l_entrada = false;
                        ddoFichIng2.c_fichadasing += "AS";
                        ddoFichIng2.c_tipo = "S";
                    }
                    else
                    {
                        b.Wrn("Legajo '" + intFichIng.nro_tarjeta + "' no tiene definido login/logout. Linea " + linea.ToString());
                        continue;
                    }
                    

                    //Verifico duplicidad
                    lasssecc = "Verificando duplicidad de entradas";
                    if (NucleusRH.Base.Tiempos_Trabajados.FichadasIng.FICHADASING.FinchadaExist(ddoFichIng1.c_fichadasing))
                    {
                        ficdup1++;
                        if (ficdup1 % 100 == 0) b.Wrn("Se encontraron " + ficdup1.ToString() + " Fichadas de entrada Duplicadas.");
                        continue;
                    }

                    lasssecc = "Verificando duplicidad de salidas";
                    if (NucleusRH.Base.Tiempos_Trabajados.FichadasIng.FICHADASING.FinchadaExist(ddoFichIng2.c_fichadasing))
                    {
                        ficdup2++;
                        if (ficdup2 % 100 == 0) b.Wrn("Se encontraron " + ficdup2.ToString() + " Fichadas de salida Duplicadas.");
                        continue;
                    }

                    //Grabo
                    lasssecc = "Guardando fichada de entrada";
                    NomadEnvironment.GetCurrentTransaction().SaveRefresh(ddoFichIng1); ficadd1++;
                    if (ficadd1 % 100 == 0) b.Log("Se agregaron " + ficadd1.ToString() + " Fichadas de entrada.");

                    lasssecc = "Guardando fichada de salida";
                    NomadEnvironment.GetCurrentTransaction().SaveRefresh(ddoFichIng2); ficadd2++;
                    if (ficadd2 % 100 == 0) b.Log("Se agregaron " + ficadd2.ToString() + " Fichadas de salida.");
                }
                catch (Exception e)
                {
                    ficerr++;
                    b.Err("Error desconocido. " + e.Message + " - Linea " + linea.ToString() + " - " + lasssecc);
                }
            }

            b.SetPro(100);
            b.Log("Se agregaron " + ficadd1.ToString() + " Fichadas de entrada.");
            b.Log("Se agregaron " + ficadd2.ToString() + " Fichadas de salida.");
            if (ficdup1 > 0) b.Log("Se encontraron " + ficdup1.ToString() + " Fichadas Duplicadas de entrada.");
            if (ficdup2 > 0) b.Log("Se encontraron " + ficdup2.ToString() + " Fichadas Duplicadas de salida.");
            if (ficrec > 0) b.Log("Se rechazaron " + ficrec.ToString() + " Fichadas.");
            if (ficerr > 0) b.Log("Se encontraron " + ficerr.ToString() + " Fichadas con ERROR.");
        }

    }
}

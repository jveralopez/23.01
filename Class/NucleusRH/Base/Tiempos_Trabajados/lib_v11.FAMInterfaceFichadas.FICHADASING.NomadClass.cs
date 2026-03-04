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

namespace NucleusRH.Base.Tiempos_Trabajados.FAMInterfaceFichadas
{

    //////////////////////////////////////////////////////////////////////////////////
    //Clase FichadasIngresadas
    public partial class FICHADASING : Nomad.NSystem.Base.NomadObject
    {

        public static void ImportarFichadas(int oi_terminal)
        {
            //Codigo en .NET
            NomadBatch b = NomadBatch.GetBatch("Importar Fichadas FAM", "Importar Fichadas FAM");

            int ficaddE, ficaddS, ficrec, ficerr, ficdup, linea, oi_personal, totRegs;
            
            NucleusRH.Base.Tiempos_Trabajados.FichadasIng.FICHADASING ddoFichIng;
            NucleusRH.Base.Tiempos_Trabajados.FAMInterfaceFichadas.FICHADASING intFichIng;

            //Cargando el Query.
            NomadBatch.Trace("Cargando el Query...");
            string MySTR = NomadProxy.GetProxy().SQLService().Get(FICHADASING.Resources.qry_fichadas, "");
            NomadXML MyXML = new NomadXML();
            NomadXML MyROW;

            //Contando la Cantidad de ROWS
            MyXML.SetText(MySTR);
            totRegs = MyXML.FirstChild().ChildLength;

            //Recupero el codigo de la empresa asignada a la terminal
            NomadXML codEmp = NomadEnvironment.QueryNomadXML(NucleusRH.Base.Tiempos_Trabajados.Terminales.TERMINAL.Resources.GET_EMP_TERMINAL, "<FILTRO oi_terminal=\"" + oi_terminal.ToString() + "\"/>");
            if (codEmp.FirstChild().GetAttr("c_empresa") == "")
                throw new NomadAppException("No esta definido el codigo de la empresa.");
 
            int legajo = 0;

            //Recorre los registros
            b.SetMess("Incorporando Fichadas...");
            b.Log("Incorporando Fichadas...");
            linea = 0; ficaddE = 0; ficaddS = 0; ficrec = 0; ficerr = 0; ficdup = 0;
            for (linea = 1, MyROW = MyXML.FirstChild().FirstChild(); MyROW != null; linea++, MyROW = MyROW.Next())
            {
                if (linea == 1) continue;
                b.SetPro(0, 90, totRegs, linea);
                b.SetMess("Procesando la Linea " + linea + " de " + totRegs);
                try
                {
                    intFichIng = FICHADASING.Get(MyROW.GetAttr("id"));

                    //Creo el numero del legajo reloj - 1 + codigo empresa de dos digitos - le concateno el legajo leido desde la terminal al string
                    legajo = int.Parse(intFichIng.legajo.Replace("\"",""));
                    //legajo = int.Parse(intFichIng.legajo);

                    //PERSONAL                    
                    oi_personal = NucleusRH.Base.Tiempos_Trabajados.FichadasIng.FICHADASING.GetPersonalEmpresaID(legajo.ToString());
                    if (oi_personal == 0)
                    {
                        ficrec++;
                        b.Wrn("Legajo " + intFichIng.legajo + " no encontrado... Linea " + linea.ToString());
                        continue;
                    }

                    //FECHA - HORA                    
                    if(!intFichIng.fecha_horaNull)
                    {
                        ddoFichIng = new NucleusRH.Base.Tiempos_Trabajados.FichadasIng.FICHADASING();
                        //FIJAS						
                        ddoFichIng.c_origen = "T";
                        ddoFichIng.oi_terminal = oi_terminal.ToString();
                        ddoFichIng.c_estado = "P";

                        ddoFichIng.oi_personal_emp = oi_personal.ToString();
                        ddoFichIng.e_numero_legajo = legajo; //Legajo

                        ddoFichIng.f_fechahora = Convert.ToDateTime(intFichIng.fecha_hora);

                        ddoFichIng.c_fichadasing = Nomad.NSystem.Functions.StringUtil.date2str(ddoFichIng.f_fechahora) + ddoFichIng.e_numero_legajo.ToString();

                        ddoFichIng.l_entrada = false;
                        ddoFichIng.c_fichadasing += "AI";
                        ddoFichIng.c_tipo = "I";

                           
                        //Verifico duplicidad						
                        if (!NucleusRH.Base.Tiempos_Trabajados.FichadasIng.FICHADASING.FinchadaExist(ddoFichIng.c_fichadasing))
                        {
                            //Grabo						
                            NomadEnvironment.GetCurrentTransaction().Save(ddoFichIng); ficaddE++;
                        }
                        else
                        {
                            ficdup++;
                        } 
                        
                    }
                    else
                    {
                        ficrec++;
                        b.Wrn("Legajo " + intFichIng.legajo + " no tiene fecha - hora con el formato esperado YYYY-MM-DD HH:MM:SS... Linea " + linea.ToString());
                        continue;
                    }                    
                }
                catch (Exception e)
                {
                    ficerr++;
                    b.Err("Error desconocido. " + e.Message + " - Linea " + linea.ToString());
                }
            }

            b.Log("Se agregaron " + ficaddE.ToString() + " Fichadas entrada.");
            b.Log("Se agregaron " + ficaddS.ToString() + " Fichadas salida.");
            if (ficdup > 0) b.Wrn("Se encontraron " + ficdup.ToString() + " Fichadas Duplicadas.");
            if (ficrec > 0) b.Wrn("Se rechazaron " + ficrec.ToString() + " Registros.");
            if (ficerr > 0) b.Err("Se encontraron " + ficerr.ToString() + " Registros con ERROR.");
        }

    }
}


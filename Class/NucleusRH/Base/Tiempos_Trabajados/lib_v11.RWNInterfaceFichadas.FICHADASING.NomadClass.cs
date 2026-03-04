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

namespace NucleusRH.Base.Tiempos_Trabajados.RWNInterfaceFichadas
{

    //////////////////////////////////////////////////////////////////////////////////
    //Clase FichadasIngresadas
    public partial class FICHADASING : Nomad.NSystem.Base.NomadObject
    {

        public static void ImportarFichadas(int oi_terminal)
        {
            //Codigo en .NET
            NomadBatch b = NomadBatch.GetBatch("Importar Fichadas RWN", "Importar Fichadas RWN");

            string lasssecc;
            int ficadd, ficrec, ficerr, ficdup, linea, oi_personal, totRegs;
            NucleusRH.Base.Tiempos_Trabajados.FichadasIng.FICHADASING ddoFichIng;
            NucleusRH.Base.Tiempos_Trabajados.RWNInterfaceFichadas.FICHADASING intFichIng;

            //ILT-2017.06.12
            string eventsIN = "E;E ;ET;EP"; //Eventos de tipo Entrada            
            string eventsOUT = "S;S ;ST;SP"; //Eventos de tipo Salida
            Terminales.IO_VALIDATOR IOValidator = new Terminales.IO_VALIDATOR(oi_terminal, eventsIN, eventsOUT);
            b.Log("Lista de Eventos de tipo Entrada: " + IOValidator.EventsIN);
            b.Log("Lista de Eventos de tipo Salida: " + IOValidator.EventsOUT);

            //Cargando el Query.
            NomadBatch.Trace("Cargando el Query...");
            string MySTR = NomadProxy.GetProxy().SQLService().Get(FICHADASING.Resources.qry_fichadas, "");
            NomadXML MyXML = new NomadXML();
            NomadXML MyROW;

            //Contando la Cantidad de ROWS
            MyXML.SetText(MySTR);
            totRegs = MyXML.FirstChild().ChildLength;

            //Recorre los registros
            b.SetMess("Incorporando Fichadas...");
            b.Log("Incorporando Fichadas...");
            linea = 0; ficadd = 0; ficrec = 0; ficerr = 0; ficdup = 0; lasssecc = "";
            for (linea = 1, MyROW = MyXML.FirstChild().FirstChild(); MyROW != null; linea++, MyROW = MyROW.Next())
            {
                b.SetPro(0, 90, totRegs, linea);
                b.SetMess("Procesando la Linea " + linea + " de " + totRegs);
                try
                {
                    lasssecc = "creando fichada";
                    ddoFichIng = new NucleusRH.Base.Tiempos_Trabajados.FichadasIng.FICHADASING();

                    lasssecc = "leyendo interface";

                    intFichIng = FICHADASING.Get(MyROW.GetAttr("id"));

                    //FIJAS
                    lasssecc = "asignando valores fijos";
                    ddoFichIng.c_origen = "T";
                    ddoFichIng.oi_terminal = oi_terminal.ToString();
                    ddoFichIng.c_estado = "P";

                    //PERSONAL
                    lasssecc = "asignando personal";
                    oi_personal = NucleusRH.Base.Tiempos_Trabajados.FichadasIng.FICHADASING.GetPersonalEmpresaID(intFichIng.legajo);
                    if (oi_personal == 0)
                    {
                        ficrec++;
                        b.Wrn("Legajo " + intFichIng.legajo + " no encontrado... Linea " + linea.ToString());
                        continue;
                    }
                    ddoFichIng.oi_personal_emp = oi_personal.ToString(); //Tengo que objtener el PERSONAL

                    ddoFichIng.e_numero_legajo = int.Parse(intFichIng.legajo); //Legajo

                    //HORA
                    lasssecc = "asignando fecha/hora";
                    System.DateTime F = intFichIng.fecha;
                    System.DateTime H = intFichIng.hora;
                    System.DateTime FH = new System.DateTime(F.Year, F.Month, F.Day, H.Hour, H.Minute, 0);
                    ddoFichIng.f_fechahora = FH;
                    ddoFichIng.c_fichadasing = Nomad.NSystem.Functions.StringUtil.date2str(ddoFichIng.f_fechahora) + ddoFichIng.e_numero_legajo.ToString();

                    //TIPO
                    lasssecc = "asignando evento";
                    string paramES = NomadEnvironment.QueryValue("ORG26_PARAMETROS", "d_valor", "c_parametro", "EveES", "ORG26_PARAMETROS.c_modulo = \\'TTA\\'", false);
                    if (paramES != "1")
                    {
                        ddoFichIng.l_entrada = false;
                        ddoFichIng.c_fichadasing += "AI";
                        ddoFichIng.c_tipo = "I";
                    }
                    else
                    {
                        //ILT-2017.06.12
                        if (IOValidator.IsInput(intFichIng.evento.ToUpper()))
                        {
	                        ddoFichIng.l_entrada = true;
                            ddoFichIng.c_fichadasing += "AE";
                            ddoFichIng.c_tipo = "E";
                        }
                        else if (IOValidator.IsOutput(intFichIng.evento.ToUpper()))
                        {
	                        ddoFichIng.l_entrada = false;
                            ddoFichIng.c_fichadasing += "AS";
                            ddoFichIng.c_tipo = "S";
                        }
                        else
                        {
                            ficrec++;
                            b.Wrn("El evento '" + intFichIng.evento + "' no reconocido. Linea " + linea.ToString());	
                        }

                        /* ILT-2017.06.12
                        switch (intFichIng.evento.ToUpper())
                        {
                            case "E":
                            case "E ":
                            case "ET":
                            case "EP":
                                ddoFichIng.l_entrada = true;
                                ddoFichIng.c_fichadasing += "AE";
                                ddoFichIng.c_tipo = "E";
                                break;
                            case "S":
                            case "S ":
                            case "ST":
                            case "SP":
                                ddoFichIng.l_entrada = false;
                                ddoFichIng.c_fichadasing += "AS";
                                ddoFichIng.c_tipo = "S";
                                break;
                            default:
                                ficrec++;
                                b.Wrn("El evento '" + intFichIng.evento + "' no reconocido. Linea " + linea.ToString());
                                continue;
                        }
                        */
                    }

                    //Verifico duplicidad
                    lasssecc = "Verificando duplicidad";
                    if (NucleusRH.Base.Tiempos_Trabajados.FichadasIng.FICHADASING.FinchadaExist(ddoFichIng.c_fichadasing))
                    {
                        ficdup++;
                        if (ficdup % 100 == 0) b.Wrn("Se encontraron " + ficdup.ToString() + " Fichadas Duplicadas.");
                        continue;
                    }

                    //Grabo
                    lasssecc = "Guardando fichada";
                    NomadEnvironment.GetCurrentTransaction().Save(ddoFichIng); ficadd++;
                    if (ficadd % 100 == 0) b.Log("Se agregaron " + ficadd.ToString() + " Fichadas.");
                }
                catch (Exception e)
                {
                    ficerr++;
                    b.Err("Error desconocido. " + e.Message + " - Linea " + linea.ToString() + " - " + lasssecc);
                }
            }

            b.Log("Se agregaron " + ficadd.ToString() + " Fichadas.");
            if (ficdup > 0) b.Wrn("Se encontraron " + ficdup.ToString() + " Duplicadas.");
            if (ficrec > 0) b.Wrn("Se rechazaron " + ficrec.ToString() + " Fichadas.");
            if (ficerr > 0) b.Err("Se encontraron " + ficerr.ToString() + " Fichadas con ERROR.");
        }

    }
}



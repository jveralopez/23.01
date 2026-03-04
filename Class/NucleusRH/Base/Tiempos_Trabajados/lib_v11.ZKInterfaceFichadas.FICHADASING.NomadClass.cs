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

namespace NucleusRH.Base.Tiempos_Trabajados.ZKInterfaceFichadas 
{

  //////////////////////////////////////////////////////////////////////////////////
  //Clase FichadasIngresadas
    public partial class FICHADASING : Nomad.NSystem.Base.NomadObject
    {

        public static void ImportarFichadas(int oi_terminal)
        {
            //Codigo en .NET
            NomadBatch b = NomadBatch.GetBatch("Incorporar Fichadas ZK...", "Incorporar Fichadas ZK...");

            string lasssecc;
            int ficadd, ficrec, ficerr, ficdup, linea, oi_personal, totRegs;
            NucleusRH.Base.Tiempos_Trabajados.FichadasIng.FICHADASING ddoFichIng;
            FICHADASING intFichIng;

            //ILT-2017.06.12
            string eventsIN = "I"; //Eventos de tipo Entrada            
            string eventsOUT = "O"; //Eventos de tipo Salida
            Terminales.IO_VALIDATOR IOValidator = new Terminales.IO_VALIDATOR(oi_terminal, eventsIN, eventsOUT);
            b.Log("Lista de Eventos de tipo Entrada: " + IOValidator.EventsIN);
            b.Log("Lista de Eventos de tipo Salida: " + IOValidator.EventsOUT);

            //Cargando el Query.
            NomadXML MyROW;
            NomadXML MyXML = new NomadXML();
            MyXML.SetText(NomadProxy.GetProxy().SQLService().Get(FICHADASING.Resources.qry_fichadas, ""));
            MyXML = MyXML.FirstChild();

            //Contando la Cantidad de ROWS
            totRegs = MyXML.ChildLength - 1;

            //Recorre los registros
            b.SetMess("Incorporando Fichadas...");
            b.Log("Incorporando Fichadas...");
            linea = 0; ficadd = 0; ficrec = 0; ficerr = 0; ficdup = 0; lasssecc = "";
            
            for (linea = 1, MyROW = MyXML.FirstChild().Next(); MyROW != null; linea++, MyROW = MyROW.Next())
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
                    oi_personal = NucleusRH.Base.Tiempos_Trabajados.FichadasIng.FICHADASING.GetPersonalEmpresaID(intFichIng.legajo.ToString());
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
                    int M1 = int.Parse(intFichIng.fecha.Substring(0, 2));
                    int D1 = int.Parse(intFichIng.fecha.Substring(3, 2));
                    int Y1 = int.Parse(intFichIng.fecha.Substring(6, 4));

                    int H2 = int.Parse(intFichIng.fecha.Substring(11, 2));
                    int M2 = int.Parse(intFichIng.fecha.Substring(14, 2));
                    if (intFichIng.fecha.Substring(17, 1) == "p") H2 += 12;

                    ddoFichIng.f_fechahora = new DateTime(Y1, M1, D1, H2, M2, 0);
                    ddoFichIng.c_fichadasing = Nomad.NSystem.Functions.StringUtil.date2str(ddoFichIng.f_fechahora) + ddoFichIng.e_numero_legajo.ToString();
                                        
                    //TIPO
                    //ILT-2017.06.12
                    lasssecc = "asignando evento";                    
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
                        case "I":
                            ddoFichIng.l_entrada = true;
                            ddoFichIng.c_fichadasing += "AE";
                            ddoFichIng.c_tipo = "E";
                            break;
                        case "O":
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
                }
                catch (Exception e)
                {
                    ficerr++;
                    b.Err("Error desconocido. " + e.Message + " - Linea " + linea.ToString() + " - " + lasssecc);
                }
            }

            b.SetPro(100);
            if (ficdup > 0) b.Log("Se encontraron " + ficdup.ToString() + " Duplicadas.");
            if (ficrec > 0) b.Log("Se rechazaron " + ficrec.ToString() + " Fichadas.");
            if (ficerr > 0) b.Log("Se encontraron " + ficerr.ToString() + " Fichadas con ERROR.");
        }

    }
}



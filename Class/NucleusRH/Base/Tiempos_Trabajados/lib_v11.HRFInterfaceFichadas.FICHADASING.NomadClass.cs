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

namespace NucleusRH.Base.Tiempos_Trabajados.HRFInterfaceFichadas
{

    //////////////////////////////////////////////////////////////////////////////////
    //Clase FichadasIngresadas
    public partial class FICHADASING : Nomad.NSystem.Base.NomadObject
    {
        public static void ImportarFichadas(int oi_terminal)
        {
            //Codigo en .NET
            NomadBatch b = NomadBatch.GetBatch("Importar Fichadas HRF", "Importar Fichadas HRF");

            string lasssecc;
            int ficadd, ficrec, ficerr, ficdup, linea, oi_personal, totRegs;
            NucleusRH.Base.Tiempos_Trabajados.FichadasIng.FICHADASING ddoFichIng;
            NucleusRH.Base.Tiempos_Trabajados.HRFInterfaceFichadas.FICHADASING intFichIng;

            //Cargando el Query.
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
                    System.DateTime FH = new System.DateTime(2000 + int.Parse(intFichIng.ano), int.Parse(intFichIng.mes), int.Parse(intFichIng.dia), int.Parse(intFichIng.hora.Substring(0, 2)), int.Parse(intFichIng.hora.Substring(2, 2)), 0);
                    ddoFichIng.f_fechahora = FH;
                    ddoFichIng.c_fichadasing = Nomad.NSystem.Functions.StringUtil.date2str(ddoFichIng.f_fechahora) + ddoFichIng.e_numero_legajo.ToString() + "AI";
                    ddoFichIng.l_entrada = false;
                    ddoFichIng.c_tipo = "I";

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



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

namespace NucleusRH.Base.Tiempos_Trabajados.GAFV2InterfaceFichadas
{

  //////////////////////////////////////////////////////////////////////////////////
  //Clase FichadasIngresadas
    public partial class FICHADASING : Nomad.NSystem.Base.NomadObject
    {

        public static void ImportarFichadas(int oi_terminal)
        {
            NomadBatch b = NomadBatch.GetBatch("Importar Fichadas GAFV2", "Importar Fichadas GAFV2");

            //Codigo en .NET
            string lasssecc;
            int ficadd, ficrec, ficerr, ficdup, linea, oi_personal, totRegs;
            NucleusRH.Base.Tiempos_Trabajados.FichadasIng.FICHADASING ddoFichIng;
            NucleusRH.Base.Tiempos_Trabajados.GAFV2InterfaceFichadas.FICHADASING intFichIng;
            NucleusRH.Base.Tiempos_Trabajados.Terminales.TERMINAL MyTERM = NucleusRH.Base.Tiempos_Trabajados.Terminales.TERMINAL.Get(oi_terminal);
            Hashtable Ent = new Hashtable();
            Hashtable Sal = new Hashtable();

            //Eventos Customizados.
            if (MyTERM.d_evento_entrada != "")
            {
                int t;
                string[] MyEVEList = MyTERM.d_evento_entrada.Split(';');

                for (t = 0; t < MyEVEList.Length; t++)
                    Ent.Add(MyEVEList[t].ToString().ToUpper(), MyEVEList[t].ToString().ToUpper());
            }
            if (MyTERM.d_evento_salida != "")
            {
                int t;
                string[] MyEVEList = MyTERM.d_evento_salida.Split(';');

                for (t = 0; t < MyEVEList.Length; t++)
                    Sal.Add(MyEVEList[t].ToString().ToUpper(), MyEVEList[t].ToString().ToUpper());
            }

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
                    lasssecc = "Creando fichada";
                    ddoFichIng = new NucleusRH.Base.Tiempos_Trabajados.FichadasIng.FICHADASING();

                    lasssecc = "Leyendo registro";
                    intFichIng = FICHADASING.Get(MyROW.GetAttr("id"));

                    string legajo = intFichIng.renglon.Substring(0, 5).Trim();
                    string fecha = intFichIng.renglon.Substring(14, 10).Trim();
                    string hora = intFichIng.renglon.Substring(25, 8).Trim();

                    //FIJAS
                    lasssecc = "Asignando valores fijos";
                    ddoFichIng.c_origen = "T";
                    ddoFichIng.oi_terminal = oi_terminal.ToString();
                    ddoFichIng.c_estado = "P";

                    //PERSONAL
                    lasssecc = "Asignando Personal";
                    oi_personal = NucleusRH.Base.Tiempos_Trabajados.FichadasIng.FICHADASING.GetPersonalEmpresaID(legajo);
                    if (oi_personal == 0)
                    {
                        ficrec++;
                        b.Wrn("Legajo " + intFichIng.renglon.Substring(0, 5) + " no encontrado - Linea " + linea.ToString());
                        continue;
                    }
                    ddoFichIng.oi_personal_emp = oi_personal.ToString(); //Tengo que objtener el PERSONAL
                    ddoFichIng.e_numero_legajo = int.Parse(legajo); //Legajo

                    //HORA
                    lasssecc = "Asignando Fecha/Hora";
                    string[] fechaArr = fecha.Split('-');
                    string[] hrArr = hora.Split(':');
                    if(fechaArr.Length!=3)
                    {
                        ficrec++;
                        b.Wrn("Formato de fecha incorrecto. Formato esperado: AAAA-MM-DD. Se rechaza el registro con legajo " + legajo + " - Linea " + linea.ToString());
                        continue;
                    }

                    if (hrArr.Length != 3)
                    {
                        ficrec++;
                        b.Wrn("Formato de hora incorrecto. Formato esperado: HH:MM:SS. Se rechaza el registro con legajo " + legajo + " - Linea " + linea.ToString());
                        continue;
                    }

                    string expectedFormat = "yyyy-MM-dd";
                    DateTime theDate;
                    if (!DateTime.TryParseExact(fecha, expectedFormat, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out theDate))
                    {
                        ficrec++;
                        b.Wrn("Fecha incorrecta. Formato esperado: AAAA-MM-DD. Se rechaza el registro con legajo " + legajo + " - Linea " + linea.ToString());
                        continue;
                    }

                    expectedFormat = "yyyy-MM-dd HH:mm:ss";
                    if(!DateTime.TryParseExact(fecha+" "+hora,expectedFormat,System.Globalization.CultureInfo.InvariantCulture,System.Globalization.DateTimeStyles.None,out theDate))
                    {
                        ficrec++;
                        b.Wrn("Hora incorrecto. Formato esperado: HH:MM:SS. Se rechaza el registro con legajo " + legajo + " - Linea " + linea.ToString());
                        continue;
                    }

                    System.DateTime FH = DateTime.ParseExact(fecha + " " + hora, expectedFormat, null);
                    ddoFichIng.f_fechahora = FH;
                    ddoFichIng.c_fichadasing = Nomad.NSystem.Functions.StringUtil.date2str(ddoFichIng.f_fechahora) + ddoFichIng.e_numero_legajo.ToString();                    

                    //TIPO
                    lasssecc = "Asignando evento";
                    ddoFichIng.l_entrada = false;
                    ddoFichIng.c_fichadasing += "IE";
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
                    b.Err("Error desconocido: " + e.Message + " - Linea " + linea.ToString() + " - " + lasssecc);
                }
            }

            b.Log("Se agregaron " + ficadd.ToString() + " Fichadas.");
            if (ficdup > 0) b.Wrn("Se encontraron " + ficdup.ToString() + " Duplicadas.");
            if (ficrec > 0) b.Wrn("Se rechazaron " + ficrec.ToString() + " Fichadas.");
            if (ficerr > 0) b.Err("Se encontraron " + ficerr.ToString() + " Fichadas con ERROR.");
        }

    }
}



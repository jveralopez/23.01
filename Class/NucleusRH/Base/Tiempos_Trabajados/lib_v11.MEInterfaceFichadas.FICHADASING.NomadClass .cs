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

namespace NucleusRH.Base.Tiempos_Trabajados.MEInterfaceFichadas 
{

  //////////////////////////////////////////////////////////////////////////////////
  //Clase FichadasIngresadas
    public partial class FICHADASING : Nomad.NSystem.Base.NomadObject
    {

        public static void ImportarFichadas(int oi_terminal)
        {
            //Codigo en .NET
            NomadBatch b = NomadBatch.GetBatch("Importar Fichadas ME", "Importar Fichadas ME");

            string lasssecc;
            int ficadd, ficrec, ficerr, ficdup, linea, oi_personal, totRegs;
            NucleusRH.Base.Tiempos_Trabajados.FichadasIng.FICHADASING ddoFichIng;
            NucleusRH.Base.Tiempos_Trabajados.MEInterfaceFichadas.FICHADASING intFichIng;

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
            
            string codigoEmp = "0" + codEmp.FirstChild().GetAttr("c_empresa");
            int tam_var = codigoEmp.Length;
            codigoEmp = codigoEmp.Substring((tam_var - 2), 2);                        

            string legajo_reloj = "1" + codigoEmp.ToString();                        

            int legajo = 0;

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

                    //Creo el numero del legajo reloj - 1 + codigo empresa de dos digitos - le sumo el legajo leido desde la terminal al string
                    legajo = int.Parse(legajo_reloj + intFichIng.legajo.ToString());

                    //PERSONAL - Ver otra la otra forma de recuperar al legajo
                    lasssecc = "asignando personal";
                    oi_personal = NucleusRH.Base.Tiempos_Trabajados.FichadasIng.FICHADASING.GetPersonalEmpresaID(legajo.ToString());
                    if (oi_personal == 0)
                    {
                        ficrec++;
                        b.Wrn("Legajo " + intFichIng.legajo + " no encontrado... Linea " + linea.ToString());
                        continue;
                    }
                    ddoFichIng.oi_personal_emp = oi_personal.ToString();
                    ddoFichIng.e_numero_legajo = legajo; //Legajo

                    //HORA
                    lasssecc = "asignando fecha/hora";
                    int D1 = int.Parse(intFichIng.fecha.Substring(0, 2));
                    int M1 = int.Parse(intFichIng.fecha.Substring(3, 2));
                    int Y1 = int.Parse(intFichIng.fecha.Substring(6, 4));                    

                    string hora = intFichIng.fecha.Split(' ')[1];
                    int H2 = int.Parse(hora.Split(':')[0]);
                    int M2 = int.Parse(hora.Split(':')[1]);                    

                    ddoFichIng.f_fechahora = new DateTime(Y1, M1, D1, H2, M2, 0);

                    ddoFichIng.c_fichadasing = Nomad.NSystem.Functions.StringUtil.date2str(ddoFichIng.f_fechahora) + ddoFichIng.e_numero_legajo.ToString();

                    //TIPO
                    lasssecc = "asignando evento";
                    ddoFichIng.c_fichadasing += "AI";
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



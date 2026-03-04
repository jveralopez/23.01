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

namespace NucleusRH.Base.Tiempos_Trabajados.GOFUNESInterfaceFichadas
{

  //////////////////////////////////////////////////////////////////////////////////
  //Clase FichadasIngresadas
    public partial class FICHADASING : Nomad.NSystem.Base.NomadObject
    {

        public static void ImportarFichadas(int oi_terminal)
        {
            //Codigo en .NET
            NomadBatch b = NomadBatch.GetBatch("Importar Fichadas GO FUNES", "Importar Fichadas GOFUNES");

            int ficaddE, ficaddS, ficrec, ficerr, ficdup, linea, oi_personal, totRegs;
            NucleusRH.Base.Tiempos_Trabajados.FichadasIng.FICHADASING ddoFichIng;
            NucleusRH.Base.Tiempos_Trabajados.GOFUNESInterfaceFichadas.FICHADASING intFichIng;

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

            string codigoEmp = "0" + codEmp.FirstChild().GetAttr("c_empresa").ToString();
            int tam_var = codigoEmp.Length;
            codigoEmp = codigoEmp.Substring((tam_var - 2), 2);

            string legajo_reloj = "1" + codigoEmp;

            int legajo = 0;

            //Recorre los registros
            b.SetMess("Incorporando Fichadas...");
            b.Log("Incorporando Fichadas...");
            linea = 0; ficaddE = 0; ficaddS = 0; ficrec = 0; ficerr = 0; ficdup = 0;
            for (linea = 1, MyROW = MyXML.FirstChild().FirstChild(); MyROW != null; linea++, MyROW = MyROW.Next())
            {
                b.SetPro(0, 90, totRegs, linea);
                b.SetMess("Procesando la Linea " + linea + " de " + totRegs);
                try
                {
                    intFichIng = FICHADASING.Get(MyROW.GetAttr("id"));
                    //Creo el numero del legajo reloj - 1 + codigo empresa de dos digitos - le concateno el legajo leido desde la terminal al string
                legajo = int.Parse(legajo_reloj + intFichIng.legajo);

                    //PERSONAL
                    oi_personal = NucleusRH.Base.Tiempos_Trabajados.FichadasIng.FICHADASING.GetPersonalEmpresaID(legajo.ToString());
                    if (oi_personal == 0)
                    {
                        ficrec++;
                        b.Wrn("Legajo " + intFichIng.legajo + " no encontrado... Linea " + linea.ToString());
                        continue;
                    }

                    //FECHA - HORA
                    //Formato del archivo: 2022-01-21 12:11:33
                    string strFecha;
                    strFecha = intFichIng.fecha;
                    strFecha = strFecha.Replace(" ", "");
                    strFecha = strFecha.Replace("-", "");
                    strFecha = strFecha.Replace(":", "");

                    System.DateTime dteFecha = StringUtil.str2date(strFecha);

                   //System.DateTime H = System.DateTime.Parse(intFichIng.fecha.Split(' ')[1]);
                    //System.DateTime H = StringUtil.str2date(strHora.Replace(":",""));

                    //MISMO REGISTRO - ENTRADA Y SALIDA - Instanciar dos fichadas

               //Fichada de entrada o de salida

              //Genero un DateTime para HE con cualquier fecha, Solo se utiliza la hora
               //System.DateTime fic_ent = new DateTime(Y1, M1, D1, H.Hour, H.Minute, 0);
              ddoFichIng = new NucleusRH.Base.Tiempos_Trabajados.FichadasIng.FICHADASING();

                //FIJAS
                ddoFichIng.c_origen = "T";
                ddoFichIng.oi_terminal = oi_terminal.ToString();
                ddoFichIng.c_estado = "P";

                ddoFichIng.oi_personal_emp = oi_personal.ToString();
                ddoFichIng.e_numero_legajo = legajo; //Legajo

                ddoFichIng.f_fechahora = dteFecha;

                ddoFichIng.c_fichadasing = Nomad.NSystem.Functions.StringUtil.date2str(ddoFichIng.f_fechahora) + ddoFichIng.e_numero_legajo.ToString();

                //TIPO
                ddoFichIng.l_entrada = (intFichIng.evento == "0");
                ddoFichIng.c_fichadasing += ((intFichIng.evento == "0") ? "AE" : "AS");
                ddoFichIng.c_tipo = (intFichIng.evento == "0") ? "E" : "S";

                    //Verifico duplicidad
                    if (!NucleusRH.Base.Tiempos_Trabajados.FichadasIng.FICHADASING.FinchadaExist(ddoFichIng.c_fichadasing))
                    {
                        //Grabo
                        NomadEnvironment.GetCurrentTransaction().Save(ddoFichIng);
                        if (intFichIng.evento == "0")
                          ficaddE++;
                        else
                          ficaddS++;
                    }
                    else
                    {
                        ficdup++;
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



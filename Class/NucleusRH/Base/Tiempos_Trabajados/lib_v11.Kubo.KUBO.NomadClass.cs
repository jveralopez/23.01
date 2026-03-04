using System;
using System.Xml;
using System.IO;
using System.Collections;

using Nomad.NSystem.Document;
using Nomad.NSystem.Proxy;
using Nomad.NSystem.Base;
using Nomad.NSystem.Functions;
using System.Collections.Generic;

namespace NucleusRH.Base.Tiempos_Trabajados.Kubo
{
    //////////////////////////////////////////////////////////////////////////////////
    //Clase de Metodos para Kubo
    public partial class KUBO
    {
        public static void FichadaManual(int oi_personal_emp, string c_tipo, int e_numero_legajo)
        {
          NomadLog.Debug("-------FICHADAS MANUALLES--------");

            string oiPER = "", oiTer = "";
            NucleusRH.Base.Tiempos_Trabajados.FichadasIng.FICHADASING ddoFichIng;

             //Creo la fichada
             ddoFichIng = new NucleusRH.Base.Tiempos_Trabajados.FichadasIng.FICHADASING();

             //Asigno valores fijos
             ddoFichIng.c_origen = "T";

             NomadXML xml_terminal = NomadEnvironment.QueryNomadXML(KUBO.Resources.QRY_TERMINAL, "").FirstChild();
             string c_terminal = xml_terminal.GetAttr("c_terminal");
             oiTer = NomadEnvironment.QueryValue("TTA05_TERMINALES", "oi_terminal", "c_terminal", c_terminal.ToString(), "", true);

             if (oiTer == null)
             {
                 ddoFichIng.oi_terminal = "1";
             }
             else
             {
                 ddoFichIng.oi_terminal = oiTer;
             }
             ddoFichIng.c_estado = "P";

             //PERSONAL - oi_personal_emp
             oiPER = NomadEnvironment.QueryValue("TTA04_PERSONAL", "oi_personal_emp", "oi_personal_emp", oi_personal_emp.ToString(), "", true);
             if (oiPER == null)
             {
                 throw new NomadAppException("El legajo no existe dentro del módulo de tiempos");
             }

             ddoFichIng.oi_personal_emp = oiPER;

             //Puede o NO tener numero del legajo reloj - Se utiliza para identificar a la fichada (dentro del c_fichadasing) el oi_personal_emp
             //c_fichadasing = f_fechahora + oi_personal_emp + CodigoTipoEvento
             ddoFichIng.e_numero_legajo = oi_personal_emp;

             //Fecha-HORA
             DateTime Hoy = DateTime.Now;
             ddoFichIng.f_fechahora = Hoy;
             ddoFichIng.c_fichadasing = Nomad.NSystem.Functions.StringUtil.date2str(ddoFichIng.f_fechahora) + ddoFichIng.e_numero_legajo.ToString();

             //Tengo que validar los eventos de entrada y salida?
             //Entrada
             if (c_tipo == "E")
             {
                 ddoFichIng.l_entrada = true;
                 ddoFichIng.c_fichadasing += "AE";
                 ddoFichIng.c_tipo = "E";
             }
             //Salida
             else if (c_tipo == "S")
             {
                 ddoFichIng.l_entrada = false;
                 ddoFichIng.c_fichadasing += "AS";
                 ddoFichIng.c_tipo = "S";
             }
             //Indefinida
             else
             {
                 ddoFichIng.l_entrada = false;
                 ddoFichIng.c_fichadasing += "AI";
                 ddoFichIng.c_tipo = "I";
             }

             //Verifico duplicidad
             if (NucleusRH.Base.Tiempos_Trabajados.FichadasIng.FICHADASING.FinchadaExist(ddoFichIng.c_fichadasing))
             {
                 throw new NomadAppException("La fichada se solapa con otras fichadas");
             }

             //Grabo
             NomadEnvironment.GetCurrentTransaction().Save(ddoFichIng);

        }

    }
}



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

namespace NucleusRH.Base.Tiempos_Trabajados.Esperanza
{

    //////////////////////////////////////////////////////////////////////////////////
    //Clase Esperanza- Tiempos Trabajados
    public partial class ESPERANZA : Nomad.NSystem.Base.NomadObject
    {
        public static Nomad.NSystem.Proxy.NomadXML GetHope(int p_oi_Horario, DateTime p_FecDes, DateTime p_FecHas, int p_oi_calendario, int p_oi_escuadra)
        {
            NomadEnvironment.GetTrace().Info("********* Comienza la generacion de esperanza por persona **************");
            Nomad.NSystem.Proxy.NomadXML objEsP;
            objEsP = (Nomad.NSystem.Proxy.NomadXML)NucleusRH.Base.Tiempos_Trabajados.Horarios.clsCreaEsperanzaH.GetHope(p_FecDes, p_FecHas, p_oi_Horario, p_oi_calendario, p_oi_escuadra);
            NomadEnvironment.GetTrace().Info("********* Retorna la esperanza **************");

            return objEsP;
        }
    }
}




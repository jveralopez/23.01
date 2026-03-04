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

namespace NucleusRH.Base.Tiempos_Trabajados.Tipos_Horas
{

    //////////////////////////////////////////////////////////////////////////////////
    //Clase Tipos de Horas
    public partial class TIPOHORA : Nomad.NSystem.Base.NomadObject
    {

        public static TIPOHORA GetById(string id)
        {
            TIPOHORA th = (TIPOHORA)NomadProxy.GetProxy().CacheGetObj("NucleusRH.Base.Tiempos_Trabajados.Tipos_Horas.TIPOHORA." + id);
            if (th == null)
            {
                th = TIPOHORA.Get(id);
                NomadProxy.GetProxy().CacheAdd("NucleusRH.Base.Tiempos_Trabajados.Tipos_Horas.TIPOHORA." + id, th);
            }

            return th;
        }

        public bool Presencia
        {
            get { return e_sumariza_pre > 0; }
        }

        public bool Ausencia
        {
            get { return e_sumariza_aus == 1; }
        }

        public bool Obligatorio
        {
            get { return e_sumariza_aus == 2; }
        }

    }
}



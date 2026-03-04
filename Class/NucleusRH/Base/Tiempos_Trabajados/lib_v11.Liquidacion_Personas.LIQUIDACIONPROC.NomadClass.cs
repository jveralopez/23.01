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

namespace NucleusRH.Base.Tiempos_Trabajados.Liquidacion_Personas
{
    public partial class LIQUIDACIONPROC : Nomad.NSystem.Base.NomadObject
    {
        public DateTime Day;

        public string TipoHora
        {
            get
            {
                return GetTipoHora().c_tipohora;
            }
        }

        public NucleusRH.Base.Tiempos_Trabajados.Tipos_Horas.TIPOHORA GetTipoHora()
        {
            return NucleusRH.Base.Tiempos_Trabajados.Tipos_Horas.TIPOHORA.GetById(oi_tipohora.ToString());
        }

        public string TipoHoraEsp
        {
            get
            {
                return GetTipoHoraEsp().c_tipohora;
            }
        }

        public NucleusRH.Base.Tiempos_Trabajados.Tipos_Horas.TIPOHORA GetTipoHoraEsp()
        {
            if (oi_tipohora_esp == null || oi_tipohora_esp == "")
				return null;
			
			return NucleusRH.Base.Tiempos_Trabajados.Tipos_Horas.TIPOHORA.GetById(oi_tipohora_esp.ToString());
        }

        public int Entrada
        {
            get
            {
                return (int)Math.Round((f_fechoraentrada - Day).TotalMinutes);
            }
        }

        public int Salida
        {
            get
            {
                return (int)Math.Round((f_fechorasalida - Day).TotalMinutes);
            }
        }

        public int CantMinutos
        {
            get
            {
                return Salida - Entrada;
            }
        }
    }
}



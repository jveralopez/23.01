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

namespace NucleusRH.Base.Tiempos_Trabajados.Esperanzaper
{
    public partial class DETALLE : Nomad.NSystem.Base.NomadObject
    {

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

        public int Entrada
        {
            get
            {
                return e_horainicio;
            }
        }

        public int Salida
        {
            get
            {
                return e_horafin;
            }
        }

        public int CantMinutos
        {
            get
            {
                return Salida - Entrada;
            }
        }
        
        public int HoraEntrada
        {
            get
            {
                return e_horainicio / 60 * 100 + e_horainicio % 60;
            }
        }

        public int HoraSalida
        {
            get
            {
                return e_horafin / 60 * 100 + e_horafin % 60;
            }
        }
        
    }
}
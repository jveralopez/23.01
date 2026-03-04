using System;
using System.Xml;
using System.IO;
using System.Collections;

using Nomad.NSystem.Document;
using Nomad.NSystem.Proxy;
using Nomad.NSystem.Base;
using Nomad.NSystem.Functions;

namespace NucleusRH.Base.Migracion.Interfaces
{
    public partial class INTERFACE
    {
        static public NucleusRH.Base.Organizacion.Convenios.CONVENIO GetConvenio(string code, Hashtable ht)
        {
            if (ht.ContainsKey(code))
                return (NucleusRH.Base.Organizacion.Convenios.CONVENIO)ht[code];

            string oiCONVENIO = NomadEnvironment.QueryValue("ORG18_CONVENIOS", "oi_convenio", "c_convenio", code, "", true);
            if (oiCONVENIO == null)
                return null;

            if (ht.Count > 50)
            {
                Grabar(ht);
                ht.Clear();
            }

            NucleusRH.Base.Organizacion.Convenios.CONVENIO ddoCON = null;
            ddoCON = NucleusRH.Base.Organizacion.Convenios.CONVENIO.Get(oiCONVENIO, false);
            ht[code] = ddoCON;

            return ddoCON;
        }

        static public NucleusRH.Base.Organizacion.Bandas_Salariales.BANDA_SAL GetBandaSalarial(string code, Hashtable ht)
        {
            if (ht.ContainsKey(code))
                return (NucleusRH.Base.Organizacion.Bandas_Salariales.BANDA_SAL)ht[code];

            string oiBS = NomadEnvironment.QueryValue("ORG17_BANDAS_SAL", "oi_banda_sal", "c_banda_sal", code, "", true);
            if (oiBS == null)
                return null;

            if (ht.Count > 50)
            {
                Grabar(ht);
                ht.Clear();
            }

            NucleusRH.Base.Organizacion.Bandas_Salariales.BANDA_SAL ddoBS = null;
            ddoBS = NucleusRH.Base.Organizacion.Bandas_Salariales.BANDA_SAL.Get(oiBS, false);
            ht[code] = ddoBS;

            return ddoBS;
        }

        static public NucleusRH.Base.Organizacion.Empresas.EMPRESA GetEmpresa(string code, Hashtable ht)
        {
            if (ht.ContainsKey(code))
                return (NucleusRH.Base.Organizacion.Empresas.EMPRESA)ht[code];

            string oiEMP = NomadEnvironment.QueryValue("ORG03_EMPRESAS", "oi_empresa", "c_empresa", code, "", true);
            if (oiEMP == null)
                return null;

            if (ht.Count > 50)
            {
                Grabar(ht);
                ht.Clear();
            }

            NucleusRH.Base.Organizacion.Empresas.EMPRESA ddoEMP = null;
            ddoEMP = NucleusRH.Base.Organizacion.Empresas.EMPRESA.Get(oiEMP, false);
            ht[code] = ddoEMP;

            return ddoEMP;
        }

        static public NucleusRH.Base.Personal.Legajo.PERSONAL GetPersona(string code, Hashtable ht)
        {
            if (ht.ContainsKey(code))
                return (NucleusRH.Base.Personal.Legajo.PERSONAL)ht[code];

            string oiPER = NomadEnvironment.QueryValue("PER01_PERSONAL", "oi_personal", "c_personal", code, "", true);
            if (oiPER == null)
                return null;

            if (ht.Count > 50)
            {
                Grabar(ht);
                ht.Clear();
            }

            NucleusRH.Base.Personal.Legajo.PERSONAL ddoPER = null;
            ddoPER = NucleusRH.Base.Personal.Legajo.PERSONAL.Get(oiPER, false);
            ht[code] = ddoPER;

            return ddoPER;
        }

        static public NucleusRH.Base.Personal.LegajoEmpresa.PERSONAL_EMP GetLegajo(string code, string parent, Hashtable ht)
        {
            if (ht.ContainsKey(code))
                return (NucleusRH.Base.Personal.LegajoEmpresa.PERSONAL_EMP)ht[code];

            string oiLEG = NomadEnvironment.QueryValue("PER02_PERSONAL_EMP", "oi_personal_emp", "e_numero_legajo", code, "PER02_PERSONAL_EMP.oi_empresa = " + parent, true);
            if (oiLEG == null)
                return null;

            if (ht.Count > 50)
            {
                Grabar(ht);
                ht.Clear();
            }

            NucleusRH.Base.Personal.LegajoEmpresa.PERSONAL_EMP ddoLEG = null;
            ddoLEG = NucleusRH.Base.Personal.LegajoEmpresa.PERSONAL_EMP.Get(oiLEG, false);
            ht[code] = ddoLEG;

            return ddoLEG;
        }

        static public void Grabar(Hashtable ht)
        {
            foreach (Nomad.NSystem.Base.NomadObject DDO in ht.Values)
                NomadEnvironment.GetCurrentTransaction().Save(DDO);
        }
    }
}
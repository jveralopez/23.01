using System;
using System.Xml;
using System.IO;
using System.Collections;

using Nomad.NSystem.Document;
using Nomad.NSystem.Proxy;
using Nomad.NSystem.Base;
using Nomad.NSystem.Functions;

namespace NucleusRH.Base.MedicinaLaboral.LegajoMedicina
{

    //////////////////////////////////////////////////////////////////////////////////
    //Clase Datos de Medicina por Personal
    public partial class CONSULTA_PER
    {
 
        public static NomadXML ValidarLicencia(int oi_personal_emp, int oi_licencia, DateTime f_inicio, DateTime f_fin, int dias)
        {
            NucleusRH.Base.Personal.LegajoEmpresa.LICENCIA_PER licencia = NucleusRH.Base.Personal.LegajoEmpresa.LICENCIA_PER.Get(oi_licencia,false);
            Hashtable hashIgnorar = new Hashtable();
            hashIgnorar.Add(oi_licencia,oi_licencia);
            return NucleusRH.Base.Personal.LegajoEmpresa.PERSONAL_EMP.ValidarLicencia(oi_personal_emp,int.Parse(licencia.oi_licencia), f_inicio, f_fin, dias, licencia.e_anio_corresp,hashIgnorar);
        }

     
    }
}



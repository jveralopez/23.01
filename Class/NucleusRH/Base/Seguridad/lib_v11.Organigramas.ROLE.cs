using System;
using System.Xml;
using System.IO;
using System.Collections;

using Nomad.NSystem.Document;
using Nomad.NSystem.Proxy;
using Nomad.NSystem.Base;
using Nomad.NSystem.Functions;

namespace NucleusRH.Base.Seguridad.Organigramas
{
    //////////////////////////////////////////////////////////////////////////////////
    //Clase Roles
    public partial class ROLE 
    {
        public static bool isJefe(string crol)
        {

            string idrol = NomadEnvironment.QueryValue("SEG01_ROLES", "oi_role", "c_role", crol, "", true);
            ROLE ddoROL = ROLE.Get(idrol, false);
            if (!ddoROL.l_iguales && ddoROL.l_inferiores)
                return true;
            return false;

        }
        public static bool isAdmin(string crol)
        {

            string idrol = NomadEnvironment.QueryValue("SEG01_ROLES", "oi_role", "c_role", crol, "", true);
            ROLE ddoROL = ROLE.Get(idrol, false);
            if (ddoROL.l_iguales && ddoROL.l_inferiores)
                return true;
            return false;

        }

        public static bool isPar(string crol)
        {

            string idrol = NomadEnvironment.QueryValue("SEG01_ROLES", "oi_role", "c_role", crol, "", true);
            ROLE ddoROL = ROLE.Get(idrol, false);
            if (ddoROL.l_iguales && !ddoROL.l_inferiores)
                return true;
            return false;

        }

    }
}

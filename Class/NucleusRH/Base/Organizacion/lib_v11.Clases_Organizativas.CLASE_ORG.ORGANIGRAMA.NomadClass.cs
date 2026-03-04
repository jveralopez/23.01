using System;
using System.Xml;
using System.IO;
using System.Collections;

using Nomad.NSystem.Document;
using Nomad.NSystem.Proxy;
using Nomad.NSystem.Base;
using Nomad.NSystem.Functions;

namespace NucleusRH.Base.Organizacion.Clases_Organizativas
{

    //////////////////////////////////////////////////////////////////////////////////
    //Clase Organigramas
    public partial class CLASE_ORG
    {

        private static void CopiarEstructura(string idclaseorg, ESTRUCTURA ddoEstrNEW, ESTRUCTURA ddoEstrOLD)
        {
            ddoEstrNEW.oi_unidad_org = ddoEstrOLD.oi_unidad_org;

            //COPIO TODOS LOS HIJOS DE LA ESTRUCTURA VIEJA A LA NUEVA ESTRUCTURA
            foreach (NucleusRH.Base.Organizacion.Clases_Organizativas.ESTRUC_PERS ddoPers in ddoEstrOLD.ESTRUC_PERS)
            {
                NucleusRH.Base.Organizacion.Clases_Organizativas.ESTRUC_PERS ddoPersNEW = new NucleusRH.Base.Organizacion.Clases_Organizativas.ESTRUC_PERS();
                ddoPersNEW.l_responsable = ddoPers.l_responsable;
                ddoPersNEW.l_staff = ddoPers.l_staff;
                ddoPersNEW.oi_clase_org = idclaseorg;
                ddoPersNEW.oi_personal_emp = ddoPers.oi_personal_emp;
                ddoEstrNEW.ESTRUC_PERS.Add(ddoPersNEW);
            }
            foreach (NucleusRH.Base.Organizacion.Clases_Organizativas.CLASE_HIJA ddoClaseHija in ddoEstrOLD.CLASES_HIJAS)
            {
                NucleusRH.Base.Organizacion.Clases_Organizativas.CLASE_HIJA ddoCHijaNEW = new NucleusRH.Base.Organizacion.Clases_Organizativas.CLASE_HIJA();
                ddoCHijaNEW.oi_clase_org = ddoClaseHija.oi_clase_org;
                ddoEstrNEW.CLASES_HIJAS.Add(ddoCHijaNEW);
            }
            foreach (NucleusRH.Base.Organizacion.Clases_Organizativas.ESTRUCTURA ddoEstr in ddoEstrOLD.ESTRUCTURAS)
            {
                NucleusRH.Base.Organizacion.Clases_Organizativas.ESTRUCTURA ddoEstrAUX = new NucleusRH.Base.Organizacion.Clases_Organizativas.ESTRUCTURA();
                ddoEstrNEW.ESTRUCTURAS.Add(ddoEstrAUX);
                //VUELVO A LLAMAR AL METODO PARA COPIAR LA ESTRUCTURA DEL NIVEL SIGUIENTE
                CopiarEstructura(idclaseorg, ddoEstrAUX, ddoEstr);
            }

        }
    }
}
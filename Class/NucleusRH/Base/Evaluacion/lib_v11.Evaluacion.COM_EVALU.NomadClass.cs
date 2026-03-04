using System;
using System.Xml;
using System.IO;
using System.Collections;

using Nomad.NSystem.Document;
using Nomad.NSystem.Proxy;
using Nomad.NSystem.Base;
using Nomad.NSystem.Functions;

namespace NucleusRH.Base.Evaluacion.Evaluacion
{
    public partial class COM_EVALU 
    {
        public void AddAptitudes()
        {


            NucleusRH.Base.Organizacion.Competencias.COMPETENCIA comp = this.Getoi_competencia();
            APT_COM aptitud;
            foreach (NucleusRH.Base.Organizacion.Competencias.APTIT_COMP apt_com in comp.APTIT_COMP)
            {
                aptitud = new APT_COM();
                aptitud.oi_aptit_comp = apt_com.Id;
                aptitud.n_ponderacion = apt_com.n_ponderacion;
                this.APT_COM.Add(aptitud);
            }

        }
    }
}

using System;
using System.Xml;
using System.IO;
using System.Collections;

using Nomad.NSystem.Document;
using Nomad.NSystem.Proxy;
using Nomad.NSystem.Base;
using Nomad.NSystem.Functions;

namespace NucleusRH.Base.QuejasyReclamos.Oficinas
{

    //////////////////////////////////////////////////////////////////////////////////
    //Clase Oficinas de Solucion
    public partial class OFICINA 
    {
        public void Crear_UniOrg()
        {

            NucleusRH.Base.Organizacion.Unidades_Organizativas.UNIDAD_ORG unidad = new NucleusRH.Base.Organizacion.Unidades_Organizativas.UNIDAD_ORG();
            NucleusRH.Base.Organizacion.Unidades_Organizativas.TIPO_UNI_ORG tipo_unidad = new NucleusRH.Base.Organizacion.Unidades_Organizativas.TIPO_UNI_ORG();
            tipo_unidad = NucleusRH.Base.Organizacion.Unidades_Organizativas.TIPO_UNI_ORG.Get("24");

            unidad.c_unidad_org = this.c_oficina;
            unidad.d_unidad_org = this.d_oficina;
            unidad.o_unidad_org = this.o_oficina;

            tipo_unidad.UNI_ORG.Add(unidad);
            NomadEnvironment.GetCurrentTransaction().SaveRefresh(tipo_unidad);

            this.oi_unidad_org = tipo_unidad.UNI_ORG.GetByAttribute("c_unidad_org", this.c_oficina).Id;

        }
        public void Borrar_UniOrg()
        {

            NucleusRH.Base.Organizacion.Unidades_Organizativas.UNIDAD_ORG unidad;
            NucleusRH.Base.Organizacion.Unidades_Organizativas.TIPO_UNI_ORG tipo_unidad;
            tipo_unidad = NucleusRH.Base.Organizacion.Unidades_Organizativas.TIPO_UNI_ORG.Get("24");
            unidad = (NucleusRH.Base.Organizacion.Unidades_Organizativas.UNIDAD_ORG)tipo_unidad.UNI_ORG.GetByAttribute("c_unidad_org", this.c_oficina);

            tipo_unidad.UNI_ORG.Remove(unidad);

            NomadEnvironment.GetCurrentTransaction().Save(tipo_unidad);
            NomadEnvironment.GetCurrentTransaction().Delete(this);

        }
        public void Editar_UniOrg()
        {

            NucleusRH.Base.Organizacion.Unidades_Organizativas.UNIDAD_ORG unidad;
            NucleusRH.Base.Organizacion.Unidades_Organizativas.TIPO_UNI_ORG tipo_unidad;
            tipo_unidad = NucleusRH.Base.Organizacion.Unidades_Organizativas.TIPO_UNI_ORG.Get("24");
            unidad = (NucleusRH.Base.Organizacion.Unidades_Organizativas.UNIDAD_ORG)tipo_unidad.UNI_ORG.GetById(this.oi_unidad_org);
            unidad.c_unidad_org = this.c_oficina;
            unidad.d_unidad_org = this.d_oficina;
            unidad.o_unidad_org = this.o_oficina;

            NomadEnvironment.GetCurrentTransaction().Save(tipo_unidad);

        }
    }
}

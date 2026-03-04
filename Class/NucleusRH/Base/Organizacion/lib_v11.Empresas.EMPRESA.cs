using System;
using System.Xml;
using System.IO;
using System.Collections;

using Nomad.NSystem.Document;
using Nomad.NSystem.Proxy;
using Nomad.NSystem.Base;
using Nomad.NSystem.Functions;

namespace NucleusRH.Base.Organizacion.Empresas
{

    //////////////////////////////////////////////////////////////////////////////////
    //Clase Empresas
    public partial class EMPRESA
    {
        public void Crear_Empresa()
        {
            NucleusRH.Base.Organizacion.Unidades_Organizativas.UNIDAD_ORG unidad = new NucleusRH.Base.Organizacion.Unidades_Organizativas.UNIDAD_ORG();
            NucleusRH.Base.Organizacion.Unidades_Organizativas.TIPO_UNI_ORG tipo_unidad;
            tipo_unidad = (NucleusRH.Base.Organizacion.Unidades_Organizativas.TIPO_UNI_ORG)NucleusRH.Base.Organizacion.Unidades_Organizativas.TIPO_UNI_ORG.Get("1");
            tipo_unidad.UNI_ORG.Add(unidad);
            unidad.c_unidad_org = this.c_empresa;
            unidad.d_unidad_org = this.d_empresa;
            unidad.o_unidad_org = this.o_empresa;
            NomadEnvironment.GetCurrentTransaction().SaveRefresh(tipo_unidad);
            this.oi_unidad_org = ((NucleusRH.Base.Organizacion.Unidades_Organizativas.UNIDAD_ORG)tipo_unidad.UNI_ORG[tipo_unidad.UNI_ORG.Count - 1]).id.ToString();
            //this.oi_unidad_org = unidad.id.ToString();
        }
        public void Borrar_Empresa()
        {
            try
            {
                NucleusRH.Base.Organizacion.Unidades_Organizativas.UNIDAD_ORG unidad = new NucleusRH.Base.Organizacion.Unidades_Organizativas.UNIDAD_ORG();
                NucleusRH.Base.Organizacion.Unidades_Organizativas.TIPO_UNI_ORG tipo_unidad = new NucleusRH.Base.Organizacion.Unidades_Organizativas.TIPO_UNI_ORG();
                tipo_unidad = NucleusRH.Base.Organizacion.Unidades_Organizativas.TIPO_UNI_ORG.Get("1");
                unidad = (NucleusRH.Base.Organizacion.Unidades_Organizativas.UNIDAD_ORG)tipo_unidad.UNI_ORG.GetByAttribute("c_unidad_org", this.c_empresa);

                tipo_unidad.UNI_ORG.Remove(unidad);

                NomadEnvironment.GetCurrentTransaction().Save(tipo_unidad);
            }
            catch (Exception e)
            {
                throw new NomadAppException("Error Eliminando Unidad Organizativa: " + e.Message);
            }
        }
        public void Editar_Empresa(string c_empresa_ant)
        {
            //HAY QUE CAMBIAR EL CODIGO DE LA CLASE ORGANIZATIVA DE POSICIONES CORRESPONDIENTE A LA EMPRESA
            //CON UN RECURSO RECUPERO EL OI DE LA CLASE
            if (c_empresa_ant != this.c_empresa)
            {
                string param = "<DATO c_empresa_ant=\"" + c_empresa_ant + "\" oi_empresa=\"" + this.Id + "\"/>";
                Nomad.NSystem.Document.NmdXmlDocument oi_clase_org = null;
                oi_clase_org = new Nomad.NSystem.Document.NmdXmlDocument(NomadEnvironment.QueryString(NucleusRH.Base.Organizacion.Empresas.EMPRESA.Resources.QRY_CLASE, param));

                //PREGUNTO SI PARA LA EMPRESA HAY DEFINIDA UNA CLASE ORGANIZATIVA DE POSICIONES
                if (oi_clase_org.GetAttribute("id") != null)
                {
                    //CARGO LA CLASE
                    NucleusRH.Base.Organizacion.Clases_Organizativas.CLASE_ORG ddoClase;
                    ddoClase = NucleusRH.Base.Organizacion.Clases_Organizativas.CLASE_ORG.Get(oi_clase_org.GetAttribute("id").Value, false);

                    //CAMBIO EL CODIGO DE LA CLASE PARA QUE SE CORRESPONDA CON LA EMPRESA
                    ddoClase.c_clase_org = this.c_empresa + " POS";

                    //GUARDO EL CAMBIO
                    NomadEnvironment.GetCurrentTransaction().Save(ddoClase);
                }
            }

            NucleusRH.Base.Organizacion.Unidades_Organizativas.TIPO_UNI_ORG tipo_unidad;
            tipo_unidad = NucleusRH.Base.Organizacion.Unidades_Organizativas.TIPO_UNI_ORG.Get("1",false);

            NucleusRH.Base.Organizacion.Unidades_Organizativas.UNIDAD_ORG unidad;
            unidad = (NucleusRH.Base.Organizacion.Unidades_Organizativas.UNIDAD_ORG)tipo_unidad.UNI_ORG.GetById(this.oi_unidad_org);

            if (unidad != null)
            {
                unidad.c_unidad_org = this.c_empresa;
                unidad.d_unidad_org = this.d_empresa;
                unidad.o_unidad_org = this.o_empresa;
                try
                {
                    NomadEnvironment.GetCurrentTransaction().Save(tipo_unidad);
                }
                catch (Exception e)
                {
                    throw new NomadAppException("Error Actualizando Unidad Organizativa: " + e.Message);
                }
            }
            else
            {
                this.Crear_Empresa();
                try
                {
                    NomadEnvironment.GetCurrentTransaction().Save(this);
                }
                catch (Exception e)
                {
                    throw new NomadAppException("Error Creando Unidad Organizativa: " + e.Message);
                }
            }
        }
    }
}

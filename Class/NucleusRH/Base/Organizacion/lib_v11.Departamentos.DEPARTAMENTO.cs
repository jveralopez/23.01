using System;
using System.Xml;
using System.IO;
using System.Collections;

using Nomad.NSystem.Document;
using Nomad.NSystem.Proxy;
using Nomad.NSystem.Base;
using Nomad.NSystem.Functions;

namespace NucleusRH.Base.Organizacion.Departamentos
{
    //////////////////////////////////////////////////////////////////////////////////
    //Clase Puestos
    public partial class DEPARTAMENTO
    {
        public static void Crear_Departamento(Departamentos.DEPARTAMENTO DDO)
        {
            NucleusRH.Base.Organizacion.Unidades_Organizativas.UNIDAD_ORG unidad;
            NucleusRH.Base.Organizacion.Unidades_Organizativas.TIPO_UNI_ORG tipo_unidad;
            tipo_unidad = NucleusRH.Base.Organizacion.Unidades_Organizativas.TIPO_UNI_ORG.Get("6");

            if (DDO.oi_unidad_org != "")
            {
                unidad = (NucleusRH.Base.Organizacion.Unidades_Organizativas.UNIDAD_ORG)tipo_unidad.UNI_ORG.GetById(DDO.oi_unidad_org);
            }
            else
            {
                unidad = new NucleusRH.Base.Organizacion.Unidades_Organizativas.UNIDAD_ORG();
                tipo_unidad.UNI_ORG.Add(unidad);
            }

            unidad.c_unidad_org = DDO.Getoi_empresa().c_empresa + "-" + DDO.c_departamento;
            unidad.d_unidad_org = DDO.d_departamento;
            //unidad.o_unidad_org = this.o_puesto;
            NomadEnvironment.GetCurrentTransaction().SaveRefresh(tipo_unidad);

            NucleusRH.Base.Organizacion.Unidades_Organizativas.UNIDAD_ORG uni = (NucleusRH.Base.Organizacion.Unidades_Organizativas.UNIDAD_ORG)tipo_unidad.UNI_ORG[tipo_unidad.UNI_ORG.Count - 1];
            DDO.oi_unidad_org = uni.Id;

            GuardarUnidadesSectores(DDO);
            try
            {
                NomadEnvironment.GetCurrentTransaction().Save(DDO);
            }
            catch (Exception e)
            {
                throw new NomadException("Error Creando Departamento: " + e.Message);
            }
        }

        private static void GuardarUnidadesSectores(Departamentos.DEPARTAMENTO DDO)
        {
            string c_empresa = DDO.Getoi_empresa().c_empresa;
            string c_departamento = DDO.c_departamento;
            foreach (SECTOR s in DDO.SECTOR)
            {

                NucleusRH.Base.Organizacion.Unidades_Organizativas.UNIDAD_ORG unidad;
                NucleusRH.Base.Organizacion.Unidades_Organizativas.TIPO_UNI_ORG tipo_unidad;
                tipo_unidad = NucleusRH.Base.Organizacion.Unidades_Organizativas.TIPO_UNI_ORG.Get("8");

                if (s.oi_unidad_org != null)
            {
                    unidad = (NucleusRH.Base.Organizacion.Unidades_Organizativas.UNIDAD_ORG)tipo_unidad.UNI_ORG.GetById(s.oi_unidad_org);
            }
                else
        {
            unidad = new NucleusRH.Base.Organizacion.Unidades_Organizativas.UNIDAD_ORG();
            tipo_unidad.UNI_ORG.Add(unidad);
                }

                unidad.c_unidad_org = c_empresa+ "-" + c_departamento + "-" + s.c_sector;
                unidad.d_unidad_org = s.d_sector;

            try
            {
                NomadEnvironment.GetCurrentTransaction().SaveRefresh(tipo_unidad);

            }
            catch (Exception e)
            {
                throw new NomadAppException("Error Creando Unidad Organizativa: " + e.Message);
            }
            }

       }

        public static void Borrar_Departamento(string ID)
        {
            Departamentos.DEPARTAMENTO DDO = Departamentos.DEPARTAMENTO.Get(ID);

            if (DDO.SECTOR.Count > 0)
            {
                throw new NomadAppException("El departamento tiene sectores asociados, para eliminarlo debe eliminar todos sus sectores");
            }

            string c_departamento = DDO.c_departamento;
            string c_empresa = DDO.Getoi_empresa().c_empresa;

            NomadEnvironment.GetCurrentTransaction().Delete(DDO);

            try
            {

                NucleusRH.Base.Organizacion.Unidades_Organizativas.UNIDAD_ORG unidad;
                NucleusRH.Base.Organizacion.Unidades_Organizativas.TIPO_UNI_ORG tipo_unidad;
                tipo_unidad = NucleusRH.Base.Organizacion.Unidades_Organizativas.TIPO_UNI_ORG.Get("6");
                unidad = (NucleusRH.Base.Organizacion.Unidades_Organizativas.UNIDAD_ORG)tipo_unidad.UNI_ORG.GetByAttribute("c_unidad_org", c_empresa + "-" + c_departamento, false);

                tipo_unidad.UNI_ORG.Remove(unidad);
                NomadEnvironment.GetCurrentTransaction().Save(tipo_unidad);
            }
            catch (Exception e)
            {
                throw new NomadAppException("Error Eliminando Unidad Organizativa: " + e.Message);
            }
        }
            }
                }



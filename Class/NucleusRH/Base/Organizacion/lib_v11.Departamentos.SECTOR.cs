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
    public partial class SECTOR
    {
        public static void Crear_Sector(string PARENT,Departamentos.SECTOR DDO)
        {

            Departamentos.DEPARTAMENTO ddoDepartamento = Departamentos.DEPARTAMENTO.Get(PARENT);

            SECTOR s;
            if (DDO.IsForInsert)
            {
                ddoDepartamento.SECTOR.Add(DDO);
                s = DDO;
            }
            else
            {
                s = (SECTOR)ddoDepartamento.SECTOR.GetById(DDO.id.ToString());
                s.c_sector = DDO.c_sector;
                s.d_sector = DDO.d_sector;
            }
 
            NucleusRH.Base.Organizacion.Unidades_Organizativas.UNIDAD_ORG unidad;
            NucleusRH.Base.Organizacion.Unidades_Organizativas.TIPO_UNI_ORG tipo_unidad;
            tipo_unidad = NucleusRH.Base.Organizacion.Unidades_Organizativas.TIPO_UNI_ORG.Get("8");

            if (s.oi_unidad_org != "")
            {
                unidad = (NucleusRH.Base.Organizacion.Unidades_Organizativas.UNIDAD_ORG)tipo_unidad.UNI_ORG.GetById(s.oi_unidad_org);
            }
            else
            {
                unidad = new NucleusRH.Base.Organizacion.Unidades_Organizativas.UNIDAD_ORG();
                tipo_unidad.UNI_ORG.Add(unidad);
            }

            unidad.c_unidad_org = ddoDepartamento.Getoi_empresa().c_empresa + "-" + ddoDepartamento.c_departamento + "-" +s.c_sector;
            unidad.d_unidad_org = s.d_sector;
 
            NomadEnvironment.GetCurrentTransaction().SaveRefresh(tipo_unidad);

            NucleusRH.Base.Organizacion.Unidades_Organizativas.UNIDAD_ORG uni = (NucleusRH.Base.Organizacion.Unidades_Organizativas.UNIDAD_ORG)tipo_unidad.UNI_ORG[tipo_unidad.UNI_ORG.Count - 1];
            s.oi_unidad_org = uni.Id;

            try
            {
                NomadEnvironment.GetCurrentTransaction().Save(ddoDepartamento);
            }
            catch (Exception e)
            {
                throw new NomadException("Error Creando Departamento: " + e.Message);
            }
        }

        public static void Eliminar_Sector(string ID)
        {
             Departamentos.SECTOR DDO = SECTOR.Get(ID);

            try
            {
                NucleusRH.Base.Organizacion.Unidades_Organizativas.UNIDAD_ORG unidad;
                NucleusRH.Base.Organizacion.Unidades_Organizativas.TIPO_UNI_ORG tipo_unidad;
                tipo_unidad = NucleusRH.Base.Organizacion.Unidades_Organizativas.TIPO_UNI_ORG.Get("8");
                unidad = (NucleusRH.Base.Organizacion.Unidades_Organizativas.UNIDAD_ORG)tipo_unidad.UNI_ORG.GetById(DDO.oi_unidad_org);

                tipo_unidad.UNI_ORG.Remove(unidad);

                NomadEnvironment.GetCurrentTransaction().Delete(DDO);
                NomadEnvironment.GetCurrentTransaction().Save(tipo_unidad);
            }
            catch (Exception e)
            {
                throw new NomadAppException("Error Eliminando Unidad Organizativa: " + e.Message);
            }
            
        }
       
    }
}


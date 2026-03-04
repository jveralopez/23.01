using System;
using System.Xml;
using System.IO;
using System.Collections;

using Nomad.NSystem.Document;
using Nomad.NSystem.Proxy;
using Nomad.NSystem.Base;
using Nomad.NSystem.Functions;

namespace NucleusRH.Base.Organizacion.Unidades_Organizativas 
{
    //////////////////////////////////////////////////////////////////////////////////
    //Clase Unidad Organizativa
    public partial class UNIDAD_ORG
    {
        public static void Crear_Posicion_Planificada(Nomad.NSystem.Proxy.NomadXML param)
        {
            param = param.FirstChild();
            TIPO_UNI_ORG ddoTUO = TIPO_UNI_ORG.Get(3);
            UNIDAD_ORG ddoUO;

            //Cargo el Puesto
            NucleusRH.Base.Organizacion.Puestos.PUESTO ddoPUESTO = NucleusRH.Base.Organizacion.Puestos.PUESTO.Get(param.GetAttrInt("oi_puesto"), false);
            //Cargo la Empresa
            NucleusRH.Base.Organizacion.Empresas.EMPRESA ddoEMPRESA = NucleusRH.Base.Organizacion.Empresas.EMPRESA.Get(ddoPUESTO.oi_empresa, false);

            //GENERO EL CODIGO DE UNIDAD ORGANIZATIVA A PARTIR DE LA EMPRESA, EL PUESTO Y LA POSICION
            string cod = ddoEMPRESA.c_empresa + "-" + ddoPUESTO.c_puesto + "-" + param.GetAttr("c_posicion");
         
            // OBTENGO UNA UNIDAD BASURA CON EL MISMO NOMBRE
            ddoUO = (NucleusRH.Base.Organizacion.Unidades_Organizativas.UNIDAD_ORG)ddoTUO.UNI_ORG.GetByAttribute("c_unidad_org", cod, false);

            //SI NO EXISTE LA UNIDAD ORGANIZATIVA LA CREA, CARGA LOS DATOS Y LA AGREGA A LOS TIPOS DE UNIDADES ORGANIZATIVAS
            if (ddoUO == null)
            {
                ddoUO = new UNIDAD_ORG();
                ddoUO.c_unidad_org = ddoEMPRESA.c_empresa + '-' + ddoPUESTO.c_puesto + '-' + param.GetAttr("c_posicion");
                ddoUO.d_unidad_org = param.GetAttr("d_posicion");
                ddoUO.oi_puesto = param.GetAttrInt("oi_puesto");
                ddoUO.oi_clase_org = param.GetAttrInt("oi_clase_org");

                ddoTUO.UNI_ORG.Add(ddoUO);

                //CARGA LOS DATOS DE LA NUEVA POSICION Y LA AGREGA AL PUESTO
                Organizacion.Puestos.POSICION ddoPOSICION = new Organizacion.Puestos.POSICION();
                ddoPOSICION.c_posicion = param.GetAttr("c_posicion");
                ddoPOSICION.d_posicion = param.GetAttr("d_posicion");
                ddoPOSICION.l_vigente = true;
                ddoPUESTO.POSICIONES.Add(ddoPOSICION);
            }
            else
            {
                //SI YA EXISTE LA UNIDAD ORGANIZATIVA MODIFICA LOS DATOS DE LA MISMA Y DE LA POSICION
                ddoUO.d_unidad_org = param.GetAttr("d_posicion");
                ddoUO.oi_puesto = param.GetAttrInt("oi_puesto");
                ddoUO.oi_clase_org = param.GetAttrInt("oi_clase_org");

                //MODIFICA LA INFORMACION DE LA POSICION
                Organizacion.Puestos.POSICION ddoPOSICION = (Organizacion.Puestos.POSICION)ddoPUESTO.POSICIONES.GetByAttribute("c_posicion", param.GetAttr("c_posicion"), false);
                ddoPOSICION.d_posicion = param.GetAttr("d_posicion");
                ddoPOSICION.l_vigente = true;
            }


            try
            {
                //GUARDA LA UNIDAD ORGANIZATIVA
                NomadEnvironment.GetCurrentTransaction().SaveRefresh(ddoTUO);
                //GUARDA LA POSICION
                NomadEnvironment.GetCurrentTransaction().SaveRefresh(ddoPUESTO);
                //SETEA EL ID DE LA UNIDAD ORGANIZATIVA EN LA POSICION
                Organizacion.Puestos.POSICION ddoPOSICION = (Organizacion.Puestos.POSICION)ddoPUESTO.POSICIONES.GetByAttribute("c_posicion", param.GetAttr("c_posicion"), false);
                ddoPOSICION.oi_unidad_org = ((NucleusRH.Base.Organizacion.Unidades_Organizativas.UNIDAD_ORG)ddoTUO.UNI_ORG.GetByAttribute("c_unidad_org", cod, false)).Id;
                //GUARDA LA POSICION
                NomadEnvironment.GetCurrentTransaction().SaveRefresh(ddoPUESTO);
                
            }
            catch (Exception e)
            {
                throw new NomadAppException("Error al crear la Posición. " + e.Message);
            }
            
            NucleusRH.Base.Organizacion.Clases_Organizativas.CLASE_ORG.AddHijo(param.GetAttr("OI_ESTRUCTURA"), ddoTUO.UNI_ORG.GetByAttribute("c_unidad_org", ddoUO.c_unidad_org, false).Id, false);
        }
    }
}

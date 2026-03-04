using System;
using System.Xml;
using System.IO;
using System.Collections;

using Nomad.NSystem.Document;
using Nomad.NSystem.Proxy;
using Nomad.NSystem.Base;
using Nomad.NSystem.Functions;

namespace NucleusRH.Base.Organizacion.Centros_de_Costos
{

    //////////////////////////////////////////////////////////////////////////////////
    //Clase Centros de Costo
    public partial class CENTRO_COSTO
    {
        public void Crear_CCosto()
        {
            NucleusRH.Base.Organizacion.Unidades_Organizativas.UNIDAD_ORG unidad = new NucleusRH.Base.Organizacion.Unidades_Organizativas.UNIDAD_ORG();
            NucleusRH.Base.Organizacion.Unidades_Organizativas.TIPO_UNI_ORG tipo_unidad = new NucleusRH.Base.Organizacion.Unidades_Organizativas.TIPO_UNI_ORG();
            tipo_unidad = NucleusRH.Base.Organizacion.Unidades_Organizativas.TIPO_UNI_ORG.Get("4");

            unidad.c_unidad_org = this.c_centro_costo;
            unidad.d_unidad_org = this.d_centro_costo;
            unidad.o_unidad_org = this.o_centro_costo;

            tipo_unidad.UNI_ORG.Add(unidad);
            NomadEnvironment.GetCurrentTransaction().SaveRefresh(tipo_unidad);

            this.oi_unidad_org = tipo_unidad.UNI_ORG.GetByAttribute("c_unidad_org", this.c_centro_costo).Id;
        }
        public void Borrar_CCosto()
        {
            try
            {
                NucleusRH.Base.Organizacion.Unidades_Organizativas.UNIDAD_ORG unidad;
                NucleusRH.Base.Organizacion.Unidades_Organizativas.TIPO_UNI_ORG tipo_unidad;
                tipo_unidad = NucleusRH.Base.Organizacion.Unidades_Organizativas.TIPO_UNI_ORG.Get("4");
                unidad = (NucleusRH.Base.Organizacion.Unidades_Organizativas.UNIDAD_ORG)tipo_unidad.UNI_ORG.GetByAttribute("c_unidad_org", this.c_centro_costo);

                tipo_unidad.UNI_ORG.Remove(unidad);

                NomadEnvironment.GetCurrentTransaction().Save(tipo_unidad);
            }
            catch (Exception e)
            {
                throw new NomadAppException("Error Eliminando Unidad Organizativa: " + e.Message);
            }
        }
        public void Editar_CCosto()
        {
            NucleusRH.Base.Organizacion.Unidades_Organizativas.UNIDAD_ORG unidad;
            NucleusRH.Base.Organizacion.Unidades_Organizativas.TIPO_UNI_ORG tipo_unidad;
            tipo_unidad = NucleusRH.Base.Organizacion.Unidades_Organizativas.TIPO_UNI_ORG.Get("4");
            unidad = (NucleusRH.Base.Organizacion.Unidades_Organizativas.UNIDAD_ORG)tipo_unidad.UNI_ORG.GetById(this.oi_unidad_org);
            unidad.c_unidad_org = this.c_centro_costo;
            unidad.d_unidad_org = this.d_centro_costo;
            unidad.o_unidad_org = this.o_centro_costo;
            try
            {
                NomadEnvironment.GetCurrentTransaction().Save(tipo_unidad);
            }
            catch (Exception e)
            {
                throw new NomadAppException("Error Actualizando Unidad Organizativa: " + e.Message);
            }
        }
        /// <summary>
        /// Inactivar o activar un centro de costo, dependiendo de su estado
        /// </summary>
        public void Estado_CCosto(string id)
        { 
            //Obtengo el Centro de Costo
            NucleusRH.Base.Organizacion.Centros_de_Costos.CENTRO_COSTO CC = NucleusRH.Base.Organizacion.Centros_de_Costos.CENTRO_COSTO.Get(id);

            //Centro de Costo inactivo
            if (CC.l_activo == false)
            {
                CC.l_activo = true;
                try
                {
                    NomadEnvironment.GetCurrentTransaction().SaveRefresh(CC);

                }
                catch (Exception e)
                {
                    NomadException NmdEx = new NomadException("NucleusRH.Base.Organizacion.Centros_de_Costos.CENTRO_COSTO", e);
                }
            }
            else 
            { 
                //Centro de costo activo
                NomadXML param = new NomadXML("PARAM");
                param.SetAttr("oi_centro_costo", CC.id);
                NomadXML resultado = NomadEnvironment.QueryNomadXML(NucleusRH.Base.Organizacion.Centros_de_Costos.CENTRO_COSTO.Resources.QRY_VALIDAR, param.ToString());
                NomadEnvironment.GetTrace().Info("RESULADO VALIDACIÓN: " + resultado.ToString());
                NomadEnvironment.GetTrace().Info("CANTIDAD: " + resultado.FirstChild().ChildLength.ToString());

                if (resultado.FirstChild().ChildLength == 0)
                {
                    if (PreValidar(CC.Id))
                    {
                        try
                        {    
                            CC.l_activo = false;
                            NomadEnvironment.GetCurrentTransaction().SaveRefresh(CC);
                        }
                        catch (Exception e)
                        {
                            NomadException NmdEx = new NomadException("NucleusRH.Base.Organizacion.Centros_de_Costos.CENTRO_COSTO", e);
                        } 
                    }
                    else
                    {
                        NomadEnvironment.GetTrace().Info("MENSAJE");
                        throw NomadException.NewMessage("Organizacion.CENTRO_COSTO.CCO");
                    }
                }
                else 
                {
                    switch (resultado.FirstChild().GetAttr("flag"))
                    { 
                        case "LEG":
                            throw NomadException.NewMessage("Organizacion.CENTRO_COSTO.LEG");

                        case "POS":
                            throw NomadException.NewMessage("Organizacion.CENTRO_COSTO.POS");   
                    }

                   
                }
            }
            

           
            
        }
    }
}
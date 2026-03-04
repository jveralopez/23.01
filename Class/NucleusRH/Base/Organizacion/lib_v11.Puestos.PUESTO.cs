using System;
using System.Xml;
using System.IO;
using System.Collections;

using Nomad.NSystem.Document;
using Nomad.NSystem.Proxy;
using Nomad.NSystem.Base;
using Nomad.NSystem.Functions;

namespace NucleusRH.Base.Organizacion.Puestos
{

    //////////////////////////////////////////////////////////////////////////////////
    //Clase Puestos
    public partial class PUESTO
    {
        public void Crear_Puesto()
        {
            NucleusRH.Base.Organizacion.Unidades_Organizativas.UNIDAD_ORG unidad;
            NucleusRH.Base.Organizacion.Unidades_Organizativas.TIPO_UNI_ORG tipo_unidad;
            tipo_unidad = NucleusRH.Base.Organizacion.Unidades_Organizativas.TIPO_UNI_ORG.Get("2");
            unidad = new NucleusRH.Base.Organizacion.Unidades_Organizativas.UNIDAD_ORG();
            tipo_unidad.UNI_ORG.Add(unidad);
            unidad.c_unidad_org = this.Getoi_empresa().c_empresa + "-" + this.c_puesto;
            unidad.d_unidad_org = this.d_puesto;
            unidad.o_unidad_org = this.o_puesto;
            NomadEnvironment.GetCurrentTransaction().SaveRefresh(tipo_unidad);

            NucleusRH.Base.Organizacion.Unidades_Organizativas.UNIDAD_ORG uni = (NucleusRH.Base.Organizacion.Unidades_Organizativas.UNIDAD_ORG)tipo_unidad.UNI_ORG[tipo_unidad.UNI_ORG.Count - 1];
            this.oi_unidad_org = uni.Id;

            try
            {
                NomadEnvironment.GetCurrentTransaction().Save(this);
            }
            catch (Exception e)
            {
                throw new NomadAppException("Error Creando Puesto: " + e.Message);
            }
        }
        public void Borrar_Puesto()
        {

            if (this.POSICIONES.Count > 0)
            {
                throw new NomadAppException("El puesto tiene posiciones asociadas, para eliminarlo debe eliminar todas sus posiciones");
            }

            string c_puesto = this.c_puesto;

            NomadEnvironment.GetCurrentTransaction().Delete(this);

            try
            {
                NucleusRH.Base.Organizacion.Unidades_Organizativas.UNIDAD_ORG unidad;
                NucleusRH.Base.Organizacion.Unidades_Organizativas.TIPO_UNI_ORG tipo_unidad;
                tipo_unidad = NucleusRH.Base.Organizacion.Unidades_Organizativas.TIPO_UNI_ORG.Get("2");
                unidad = (NucleusRH.Base.Organizacion.Unidades_Organizativas.UNIDAD_ORG)tipo_unidad.UNI_ORG.GetByAttribute("c_unidad_org", this.Getoi_empresa().c_empresa + "-" + c_puesto, false);

                tipo_unidad.UNI_ORG.Remove(unidad);
                NomadEnvironment.GetCurrentTransaction().Save(tipo_unidad);
            }
            catch (Exception e)
            {
                throw new NomadAppException("Error Eliminando Unidad Organizativa: " + e.Message);
            }
        }
        public void Editar_Puesto()
        {
            
            //PARA CONTROLAR LA ELIMINACION/MODIFICACION DE LAS UNIDADES ORGANIZATIVAS DE POSICION
            NucleusRH.Base.Organizacion.Unidades_Organizativas.UNIDAD_ORG unidad_posicion;
            NucleusRH.Base.Organizacion.Unidades_Organizativas.TIPO_UNI_ORG tipo_unidad_posicion;
            tipo_unidad_posicion = NucleusRH.Base.Organizacion.Unidades_Organizativas.TIPO_UNI_ORG.Get("3");

            string cod_posicion;
            //NomadEnvironment.GetTrace().Info(unidad.SerializeAll().ToString());
            foreach (NucleusRH.Base.Organizacion.Puestos.POSICION posicion in this.GetChild_POSICIONES(true,true))
            {
                unidad_posicion = null;
                
                //SI ESTA POR ELIMINAR LA POSICION
                if (posicion.IsForDelete)
                {
                    // SI TIENE UNA UNIDAD ORGANIZATIVA ASOCIADA
                    if (!posicion.oi_unidad_orgNull)
                    {
                        //BUSCA LA UNIDAD ORGANIZATIVA DE LA POSICION Y MODIFICA SUS DATOS
                        unidad_posicion = (NucleusRH.Base.Organizacion.Unidades_Organizativas.UNIDAD_ORG)tipo_unidad_posicion.UNI_ORG.GetById(posicion.oi_unidad_org);
                        tipo_unidad_posicion.UNI_ORG.Remove(unidad_posicion);
                        posicion.oi_unidad_orgNull = true;
                    }
                }

                //SI SE ESTA POR MODIFICAR LA POSICION
                if (posicion.IsForUpdate)
                {
                    //SI ESTA TIENE UNA UNIDAD ORGANIZATIVA ASOCIADA
                    if (!posicion.oi_unidad_orgNull)
                    {
                        //BUSCA LA UNIDAD ORGANIZATIVA DE LA POSICION Y MODIFICA SUS DATOS
                        unidad_posicion = (NucleusRH.Base.Organizacion.Unidades_Organizativas.UNIDAD_ORG) tipo_unidad_posicion.UNI_ORG.GetById(posicion.oi_unidad_org);
                        cod_posicion = this.Getoi_empresa().c_empresa + "-" + this.c_puesto + "-" + posicion.c_posicion;
                        unidad_posicion.c_unidad_org = cod_posicion;
                        unidad_posicion.d_unidad_org = posicion.d_posicion;
                        unidad_posicion.o_unidad_org = posicion.o_posicion;
                    }
                }

            }

            NucleusRH.Base.Organizacion.Unidades_Organizativas.UNIDAD_ORG unidad;
            NucleusRH.Base.Organizacion.Unidades_Organizativas.TIPO_UNI_ORG tipo_unidad;
            tipo_unidad = NucleusRH.Base.Organizacion.Unidades_Organizativas.TIPO_UNI_ORG.Get("2");
            unidad = (NucleusRH.Base.Organizacion.Unidades_Organizativas.UNIDAD_ORG)tipo_unidad.UNI_ORG.GetById(this.oi_unidad_org);

            if (unidad != null)
            {
                unidad.c_unidad_org = this.Getoi_empresa().c_empresa + "-" + this.c_puesto;
                unidad.d_unidad_org = this.d_puesto;
                unidad.o_unidad_org = this.o_puesto;
                try
                {
                    NomadEnvironment.GetCurrentTransaction().Save(tipo_unidad);
                }
                catch (Exception e)
                {
                    throw new NomadAppException("Error Actualizando Unidad Organizativa: " + e.Message);
                }
                try
                {
                    NomadEnvironment.GetCurrentTransaction().SaveRefresh(this);
                }
                catch (Exception e)
                {
                    throw new NomadAppException("Error Editando Puesto: " + e.Message);
                }
                try
                { 
                    NomadEnvironment.GetCurrentTransaction().SaveRefresh(tipo_unidad_posicion); 
                }
                catch (Exception e)
                { 
                    throw new NomadAppException("Error al eliminar/modificar Unidades Organizativas: " + e.Message); 
                }
            }
            else
            {
                this.Crear_Puesto();
            }

            

        }
        public void Actualizar_Costo()
        {
            try
            {
                this.Costo_Puesto(null, null);
            }
            catch (Exception) { }
        }
        public void Estructura_Posicion_Cabecera(string c_posicion, string c_clase_org)
        {
            //TENGO EL PUESTO AL QUE PERTENECE LA POSICION RECIEN INGRESADA
            NomadEnvironment.GetTrace().Info("PUESTO -- " + this.SerializeAll());

            //CARGO LA POSICION
            NucleusRH.Base.Organizacion.Puestos.POSICION ddoPos = (NucleusRH.Base.Organizacion.Puestos.POSICION)this.POSICIONES.GetByAttribute("c_posicion", c_posicion, false);
            NomadEnvironment.GetTrace().Info("POSICION -- " + ddoPos.SerializeAll());

            //RECUPERO UASANDO UN RECURSO EL OI DE LA CLASE
            string param = "<DATOS c_clase_org=\"" + c_clase_org + "\"/>";
            Nomad.NSystem.Document.NmdXmlDocument oi_clase_org = null;
            oi_clase_org = new Nomad.NSystem.Document.NmdXmlDocument(NomadEnvironment.QueryString(NucleusRH.Base.Organizacion.Puestos.PUESTO.Resources.QRY_CLASE, param));
            NomadEnvironment.GetTrace().Info(oi_clase_org.ToString());

            //CARGO LA CLASE
            NucleusRH.Base.Organizacion.Clases_Organizativas.CLASE_ORG ddoClase;
            ddoClase = NucleusRH.Base.Organizacion.Clases_Organizativas.CLASE_ORG.Get(oi_clase_org.GetAttribute("id").Value);
            NomadEnvironment.GetTrace().Info("CLASE -- " + ddoClase.SerializeAll());

            //CARGO LA ESTRUCTURA DETERMINADA POR LA CLASE RECUPERADA
            NucleusRH.Base.Organizacion.Clases_Organizativas.ESTRUCTURA ddoEstrucPadre;
            ddoEstrucPadre = ddoClase.oi_estructura_org;
            NomadEnvironment.GetTrace().Info("ESTRUCTURA PADRE 1-- " + ddoEstrucPadre.SerializeAll());

            //CREO UNA NUEVA ESTRUCTURA EN LA CLASE A LA CUAL PERTENECERA LA POSICION CABEZERA RECIEN INGRESADA


            NucleusRH.Base.Organizacion.Clases_Organizativas.ESTRUCTURA ddoEstruc;
            ddoEstruc = new NucleusRH.Base.Organizacion.Clases_Organizativas.ESTRUCTURA();
            ddoEstruc.oi_unidad_org = ddoPos.oi_unidad_org;

            NomadEnvironment.GetTrace().Info("ESTRUCTURA 1-- " + ddoEstruc.SerializeAll());

            ddoEstrucPadre.ESTRUCTURAS.Add(ddoEstruc);
            //ddoClase.oi_estructura_org.ESTRUCTURAS.Add(ddoEstrucPadre);
            NomadEnvironment.GetCurrentTransaction().Save(ddoClase);

            //NomadEnvironment.GetTrace().Info("ESTRUCTURA 2-- " + ddoEstruc.SerializeAll());
            //NomadEnvironment.GetTrace().Info("ESTRUCTURA PADRE 2-- " + ddoEstrucPadre.SerializeAll());
        }
        public void Costo_Puesto(NucleusRH.Base.Organizacion.Convenios.CATEGORIA categoria, NucleusRH.Base.Organizacion.Bandas_Salariales.BANDA_SAL banda_sal)
        {
            double costo = 0;
            if (this.l_convenio == true)
            {
                // CASO PUESTO DE CONVENIO
                IList categ_antig;
                double min;
                try
                {
                    categ_antig = (categoria == null ? this.Getoi_categoria().CATEG_ANTIG : categoria.CATEG_ANTIG);
                    min = ((NucleusRH.Base.Organizacion.Convenios.CATEG_ANTIG)categ_antig[0]).n_sueldo_basico;
                    // BUSCO EL MENOR N_SUELDO_BASICO DE LA CATEGORIA
                    foreach (NucleusRH.Base.Organizacion.Convenios.CATEG_ANTIG i in categ_antig)
                    {
                        if (i.n_sueldo_basico < min)
                        {
                            min = i.n_sueldo_basico;
                        }
                    }
                    costo = min;
                }
                catch (Exception) { };

            }
            else
            {
                // CASO PUESTO FUERA DE CONVENIO
                NucleusRH.Base.Organizacion.Bandas_Salariales.BANDA_SAL banda_salarial;
                try
                {
                    banda_salarial = (banda_sal == null ? this.Getoi_banda_sal() : banda_sal);
                    // CALCULO PROMEDIO DE SALARIO MINIMO Y MAXIMO DE LA BANDA SALARIAL
                    costo = (banda_salarial.n_sal_minimo + banda_salarial.n_sal_maximo) / 2;
                }
                catch (Exception) { };
            }

            // SUMO AL COSTO LOS BENEFICIOS DEL PUESTO
            foreach (NucleusRH.Base.Organizacion.Puestos.PUESTO_BENEF j in this.PUESTO_BENEF)
            {
                costo += j.n_valor_total;
            }
            this.n_costo = costo;

            // GRABAR PUESTO
            if (categoria != null || banda_sal != null)
            {
                NomadEnvironment.GetCurrentTransaction().Save(this);
            }
        }
        public void competenciasPosicion(Nomad.NSystem.Document.NmdXmlDocument PARAM)
        {

            POSICION posicion;
            posicion = (POSICION)this.POSICIONES.GetById(PARAM.GetAttribute("oi_posicion").Value);
            Nomad.NSystem.Document.NmdXmlDocument xmlCompet = (Nomad.NSystem.Document.NmdXmlDocument)PARAM.GetFirstChildDocument();
            Nomad.NSystem.Document.NmdXmlDocument competencia;
            POSICION_COMP comp_posic;
            string ponderacion_ant = "";

            for (competencia = (Nomad.NSystem.Document.NmdXmlDocument)xmlCompet.GetFirstChildDocument(); competencia != null; competencia = (Nomad.NSystem.Document.NmdXmlDocument)xmlCompet.GetNextChildDocument())
            {
                NomadEnvironment.GetTrace().Info("competencia-- " + competencia.ToString());
                if (competencia.GetAttribute("ponderacion_ant") != null)
                {
                    ponderacion_ant = competencia.GetAttribute("ponderacion_ant").Value;
                }
                if (ponderacion_ant == "" || competencia.GetAttribute("n_ponderacion").Value != ponderacion_ant)
                    if (competencia.GetAttribute("clase").Value == "pue")
                    {
                        NomadEnvironment.GetTrace().Info("1");
                        comp_posic = new POSICION_COMP();
                        comp_posic.oi_competencia = competencia.GetAttribute("oi_competencia").Value;
                        comp_posic.n_ponderacion = Nomad.NSystem.Functions.StringUtil.str2dbl(competencia.GetAttribute("n_ponderacion").Value);
                        comp_posic.l_excluyente = competencia.GetAttribute("l_excluyente").Value == "1";
                        comp_posic.e_peso = int.Parse(competencia.GetAttribute("e_peso").Value);
                        comp_posic.oi_nivel_comp = competencia.GetAttribute("oi_nivel_comp").Value;
                        comp_posic.l_evaluacion = competencia.GetAttribute("l_evaluacion").Value == "1";
                        posicion.POSIC_COMP.Add(comp_posic);
                    }
                    else
                    {
                        NomadEnvironment.GetTrace().Info("2");
                        comp_posic = (POSICION_COMP)posicion.POSIC_COMP.GetByAttribute("oi_competencia", competencia.GetAttribute("oi_competencia").Value);
                        comp_posic.n_ponderacion = Nomad.NSystem.Functions.StringUtil.str2dbl(competencia.GetAttribute("n_ponderacion").Value);
                    }
            }
            try
            {
                NomadEnvironment.GetCurrentTransaction().Save(this);
            }
            catch (Exception e)
            {
                throw new NomadAppException("No se pudo actulizar la Competencia de la Posición: " + e.Message);
            }
        }
    }
}

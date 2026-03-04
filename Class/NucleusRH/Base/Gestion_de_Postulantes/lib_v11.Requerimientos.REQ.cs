using System;
using System.Xml;
using System.IO;
using System.Collections;

using Nomad.NSystem.Document;
using Nomad.NSystem.Proxy;
using Nomad.NSystem.Base;
using Nomad.NSystem.Functions;

namespace NucleusRH.Base.Gestion_de_Postulantes.Requerimientos
{

    //////////////////////////////////////////////////////////////////////////////////
    //Clase Requerimientos de Personal
    public partial class REQ
    {
        public void saveReq()
        {
            REFERENCIA ddoRef = (REFERENCIA)this.REFERENCIAS[0];
            NomadLog.Info("this:: " + this.ToString());
            //RECUPERO EL PUESTO
            NucleusRH.Base.Gestion_de_Postulantes.Puestos.PUESTO ddoPuesto = ddoRef.Getoi_puesto();
            NomadLog.Info("ddoPuesto::: " + ddoPuesto.ToString());
            ddoRef.ca_estado = "A";
            ddoRef.d_referencia = this.d_req;
            ddoRef.o_referencia = this.o_req;
            //RECUPERO TODOS LOS EXAMENES ASOCIADOS AL PUESTO
            foreach (NucleusRH.Base.Gestion_de_Postulantes.Puestos.PUES_EXA ddoPuesExa in ddoPuesto.PUES_EXA)
            {
                NucleusRH.Base.Gestion_de_Postulantes.Examenes.EXAMEN ddoExamen = ddoPuesExa.Getoi_examen();
                NucleusRH.Base.Gestion_de_Postulantes.Requerimientos.REF_EXAMEN ddoRefExa;
                ddoRefExa = new NucleusRH.Base.Gestion_de_Postulantes.Requerimientos.REF_EXAMEN();
                ddoRefExa.oi_examen = ddoExamen.Id;
                ddoRefExa.e_orden = ddoExamen.e_orden;

                //RECORRO TODOS LOS RESULTADOS DE LOS EXAMENES RECUPERADOS Y LOS ASIGNO A LA REFERENCIA
                foreach (NucleusRH.Base.Gestion_de_Postulantes.Examenes.RESUL_EXA ddoResExa in ddoExamen.RESUL_EXA)
                {
                    NucleusRH.Base.Gestion_de_Postulantes.Requerimientos.REF_RES_EXA ddoRefResExa;
                    ddoRefResExa = new NucleusRH.Base.Gestion_de_Postulantes.Requerimientos.REF_RES_EXA();
                    ddoRefResExa.oi_resultado = ddoResExa.oi_resultado;
                    ddoRefResExa.o_ref_res_exa = ddoResExa.o_resul_exa;
                    ddoRefResExa.l_apto_prox_examen = ddoResExa.l_apto_prox_examen;

                    ddoRefExa.REF_RES_EXA.Add(ddoRefResExa);
                }
                NomadLog.Info("ddoRefExa:: " + ddoRefExa.ToString());
                ddoRef.REF_EXAMEN.Add(ddoRefExa);
            }

            NomadEnvironment.GetCurrentTransaction().Save(this);
        }

        public void Guardar()
        {

            foreach (NucleusRH.Base.Gestion_de_Postulantes.Requerimientos.REFERENCIA ddoRef in this.REFERENCIAS)
            {
                //GRABO LA CANTIDAD DE PERSONAS Y POSTULANTES INSCRIPTOS
                int cant_pos = 0;
                int cant_per = 0;

                foreach (NucleusRH.Base.Gestion_de_Postulantes.Requerimientos.REF_POST ddoREF_POST in ddoRef.REF_POST)
                {
                    if (ddoREF_POST.IsForInsert)
                        cant_pos = cant_pos + 1;
                }

                foreach (NucleusRH.Base.Gestion_de_Postulantes.Requerimientos.REF_PER ddoREF_PER in ddoRef.REF_PER)
                {
                    if (ddoREF_PER.IsForInsert)
                        cant_per = cant_per + 1;
                }

                ddoRef.e_cant_pos = cant_pos;
                ddoRef.e_cant_per = cant_per;
                ddoRef.d_referencia = this.d_req;
                ddoRef.o_referencia = this.o_req;

                //SETEO EL ESTADO
                if (ddoRef.ca_estado == "E")
                    ddoRef.ca_estado = "A";
            }

            NomadEnvironment.GetCurrentTransaction().Save(this);

        }
        public void IniciarReq()
        {

            if (this.ca_estado == "A")
            {
                foreach (NucleusRH.Base.Gestion_de_Postulantes.Requerimientos.REFERENCIA ddoRef in this.REFERENCIAS)
                {
                    if (ddoRef.ca_estado == "A")
                    {
                        ddoRef.ca_estado = "I";
                        ddoRef.f_inicio = DateTime.Now;
                    }
                    else if (ddoRef.ca_estado == "I")
                        continue;
                    else if (ddoRef.ca_estado == "E")
                        throw new NomadAppException("No se puede inicializar una Referencia nueva. Primero debe guardar la Referencia");
                    else
                        throw new NomadAppException("No puede haber una Referencia que este finalizada, anulada si la misma se va a inicializar");
                }
                this.ca_estado = "I";
                this.f_inicio = DateTime.Now;
                NomadEnvironment.GetCurrentTransaction().Save(this);
            }
            else
                throw new NomadAppException("No se puede Inicializar una Referencia que no se encuentre abierta");

        }
        public void AnularReq()
        {

            if (this.ca_estado == "A")
            {
                this.ca_estado = "N";
                this.f_anulacion = DateTime.Now;
                NomadEnvironment.GetCurrentTransaction().Save(this);
            }
            else
                throw new NomadAppException("No se puede anular una Referencia que no se encuentre abierta");

        }

        public void CerrarReq()
        {

            System.Text.StringBuilder IDS = new System.Text.StringBuilder();
            int cant = 0;
            string param = "";

            if (this.ca_estado == "I")
            {

                foreach (NucleusRH.Base.Gestion_de_Postulantes.Requerimientos.REFERENCIA ddoRef in this.REFERENCIAS)
                {
                    if (ddoRef.ca_estado == "I")
                    {
                        param = "<DATOS id= \"" + ddoRef.id + "\" />";
                        Nomad.NSystem.Document.NmdXmlDocument docCant = new Nomad.NSystem.Document.NmdXmlDocument(NomadEnvironment.QueryString(NucleusRH.Base.Gestion_de_Postulantes.Reclutados.RECLUTADO.Resources.CantidadContratados, param));
                        cant = int.Parse(docCant.GetAttribute("cant").Value);
                        if (!ddoRef.e_cant_personasNull)
                        {
                            if (cant >= ddoRef.e_cant_personas)
                            {
                                ddoRef.ca_estado = "C";
                                ddoRef.f_cierre = DateTime.Now;
                                ddoRef.EliminarRankeados();
                            }
                            else
                            {
                                if (cant > 0)
                                {
                                    ddoRef.ca_estado = "C";
                                    ddoRef.f_cierre = DateTime.Now;
                                    ddoRef.EliminarRankeados();

                                }
                                else
                                {
                                    ddoRef.ca_estado = "C";
                                    ddoRef.f_cierre = DateTime.Now;
                                    ddoRef.EliminarRankeados();
                                }
                            }

                        }
                        else
                        {
                            if (cant > 0)
                            {
                                ddoRef.ca_estado = "C";
                                ddoRef.f_cierre = DateTime.Now;
                                ddoRef.EliminarRankeados();
                            }
                            else
                            {
                                ddoRef.ca_estado = "C";
                                ddoRef.f_cierre = DateTime.Now;
                                ddoRef.EliminarRankeados();
                            }
                        }
                    }
                    else
                    {
                        if (ddoRef.ca_estado == "A")
                        {
                            ddoRef.ca_estado = "C";
                            ddoRef.f_cierre = DateTime.Now;
                            ddoRef.EliminarRankeados();
                        }
                    }

                    //aqui debo eliminar todos los inscriptos en las referencias
                    ddoRef.REF_POST.Clear();
                    ddoRef.REF_PER.Clear();
                }
                this.ca_estado = "C";
                this.f_cierre = DateTime.Now;
                IDS.Append(this.id);
                NomadEnvironment.GetCurrentTransaction().Save(this);
                NucleusRH.Base.Configuracion.LogEventos.LOGEVENTO.Call("NucleusRH.Base.Gestion_de_Postulantes.Requerimientos.close_req", IDS.ToString(), this.ca_estado, "Cambio a estado " + this.ca_estado);

            }
            else
            {
                throw new NomadAppException("No se puede Cerrar un requerimiento que no se encuentra inicializado");
            }
            return;

        }
        public void IniciarReferencia(string refid)
        {

            REFERENCIA ddoREF = (REFERENCIA)this.REFERENCIAS.GetById(refid);
            ddoREF.ca_estado = "I";
            NomadEnvironment.GetCurrentTransaction().SaveRefresh(this);

        }
        public void AbortarReferencia(string refid)
        {

            REFERENCIA ddoREF = (REFERENCIA)this.REFERENCIAS.GetById(refid);
            ddoREF.ca_estado = "N";
            NomadEnvironment.GetCurrentTransaction().SaveRefresh(this);

        }

    }
}



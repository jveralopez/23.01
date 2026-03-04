using System;
using System.Xml;
using System.IO;
using System.Collections;
using System.Collections.Generic;

using Nomad.NSystem.Document;
using Nomad.NSystem.Proxy;
using Nomad.NSystem.Base;
using Nomad.NSystem.Functions;

using NucleusRH.Base.Personal.Licencias;
using NucleusRH.Base.Organizacion.Convenios;
using System.Text;

namespace NucleusRH.Base.Personal.LegajoEmpresa
{

    //////////////////////////////////////////////////////////////////////////////////
    //Clase Legajo
    public partial class PERSONAL_EMP
    {
        public void Egreso_Personal(DateTime f_egreso, string o_egreso, string oi_motivo_eg_per)
        {
            try
            {

                //Hace el commit por cada Estructura PER que BORRA
                //sacar al legajo del Organigrama
                //PASO EL ID DEL LEGAJO A UN QRY PARA SABER SI ESTA ASIGNADO A UNA ESTRUCTURA DE UN ORGANIGRAMA
                NomadXML xmlRESULT;
                string param = "<DATO oi_personal_emp=\"" + this.Id + "\"/>";
                xmlRESULT = new NomadXML();
                NomadLog.Info("param -- " + param);
                xmlRESULT.SetText(NomadEnvironment.QueryString(Resources.QRY_ESTRUCTURA_PERS_ULT, param));
                NomadLog.Info("xmlRESULT -- " + xmlRESULT.ToString());

                //SI EL ATRIBUTO FLAG VUELVE EN "1" ENTONCES EL LEGAJO ESTA ASIGNADO A AL MENOS UN ORGANIGRAMA
                if (xmlRESULT.FirstChild().GetAttr("flag") == "1")
                {
                    //Me paro sobre la lista de ROWS
                    NomadXML MyXML = xmlRESULT.FirstChild();
                    NomadXML MyROW;

                    //Contando la cantidad de Registros
                    int totRegs = MyXML.ChildLength;
                    int linea;

                    MyROW = MyXML.FirstChild();
                    for (linea = 1, MyROW = MyXML.FirstChild(); MyROW != null; linea++, MyROW = MyROW.Next())
                    {
                        NomadEnvironment.GetCurrentTransaction().Begin();
                        //NucleusRH.Base.Organizacion.Clases_Organizativas.ESTRUC_PERS ddoESTRPER = NucleusRH.Base.Organizacion.Clases_Organizativas.ESTRUC_PERS.Get(MyROW.GetAttr("oi_estruc_pers"));
                        NucleusRH.Base.Organizacion.Estructuras2.ESTRUC_PERS_AUX ddoESTRPER = NucleusRH.Base.Organizacion.Estructuras2.ESTRUC_PERS_AUX.Get(MyROW.GetAttr("oi_estruc_pers"));
                        NomadEnvironment.GetCurrentTransaction().Delete(ddoESTRPER);
                        NomadEnvironment.GetCurrentTransaction().Commit();
                    }

                }

                //INICIO UNA TRANSACCION PARA GUARDAR LOS CAMBIOS DEL EGRESO
                NomadEnvironment.GetCurrentTransaction().Begin();

                // Verificar que la persona no haya egresado
                if (!this.f_egresoNull)
                    throw new NomadAppException("El legajo está egresado");

                if (f_egreso < this.f_ingreso)
                    throw new NomadAppException("La fecha de egreso no puede ser anterior a la fecha de ingreso (" + this.f_ingreso.ToString("dd/MM/yyyy") + ")");

                // Cerrar Categoria
                if (this.CATEG_PER.Count > 0)
                {
                    NucleusRH.Base.Personal.LegajoEmpresa.CATEGORIA_PER categoria;
                    categoria = this.GetUltimaCategoria();
                    categoria.f_egreso = f_egreso;
                    categoria.oi_motivo_cambio = "1";
                    categoria.o_cambio_categoria = o_egreso;
                }

                // Cerrar Centro de Costo
                if (this.CCOSTO_PER.Count > 0)
                {
                    NucleusRH.Base.Personal.LegajoEmpresa.CCOSTO_PER ccosto;
                    ccosto = this.GetUltimoCCosto();
                    ccosto.f_egreso = f_egreso;
                    ccosto.oi_motivo_cambio = "1";
                    ccosto.o_cambio_ccosto = o_egreso;
                }

                // Cerrar Posicion
                if (this.POSIC_PER.Count > 0)
                {
                    NucleusRH.Base.Personal.LegajoEmpresa.POSICION_PER posicion;
                    posicion = this.GetUltimaPosicion();
                    posicion.f_egreso = f_egreso;
                    posicion.oi_motivo_cambio = "1";
                    posicion.o_cambio_posic = o_egreso;
                }

                // Cerrar Puesto
                if (this.PUESTO_PER.Count > 0)
                {
                    NucleusRH.Base.Personal.LegajoEmpresa.PUESTO_PER puesto;
                    puesto = this.GetUltimoPuesto();
                    puesto.f_egreso = f_egreso;
                    puesto.oi_motivo_cambio = "1";
                    puesto.o_cambio_puesto = o_egreso;
                }

                // Cerrar Remuneracion
                if (this.REMUN_PER.Count > 0)
                {
                    NucleusRH.Base.Personal.LegajoEmpresa.REMUN_PER remun;
                    remun = this.GetUltimaRemuneracion();
                    remun.f_hasta = f_egreso;
                    remun.oi_motivo_cambio = "1";
                    remun.o_remun_per = o_egreso;
                }

                // Cerrar Tipo de Personal
                if (this.TIPOSP_PER.Count > 0)
                {
                    NucleusRH.Base.Personal.LegajoEmpresa.TIPOP_PER tipop;
                    tipop = this.GetUltimoTipoPersonal();
                    tipop.f_egreso = f_egreso;
                    tipop.oi_motivo_cambio = "1";
                    tipop.o_cambio_tper = o_egreso;
                }

                // Cerrar Equipo Móvil
                if (this.EQUIPO_PER.Count > 0)
                {
                    NucleusRH.Base.Personal.LegajoEmpresa.EQUIPO_PER equipo;
                    equipo = this.GetUltimoEquipoMovil();
                    equipo.f_egreso = f_egreso;
                    equipo.oi_motivo_cambio = "1";
                    equipo.o_equipo_per = o_egreso;
                }

                // Cerrar Calendario
                if (this.CALENDARIO_PER.Count > 0)
                {
                    NucleusRH.Base.Personal.LegajoEmpresa.CALENDARIO_PER calendario;
                    calendario = this.GetUltimoCalendario();
                    calendario.f_hasta = f_egreso;
                    calendario.oi_motivo_cambio = "1";
                    calendario.o_cambio_calend = o_egreso;
                }

                // Cerrar Ubicación
                if (this.UBIC_PER.Count > 0)
                {
                    NucleusRH.Base.Personal.LegajoEmpresa.UBICACION_PER ubicacion;
                    ubicacion = this.GetUltimaUbicacion();
                    ubicacion.f_hasta = f_egreso;
                    ubicacion.oi_motivo_cambio = "1";
                    ubicacion.o_cambio_ubic = o_egreso;
                }

                this.oi_puesto_ultNull = true;
                this.oi_posicion_ultNull = true;
                this.f_desde_puestoNull = true;
                this.f_desde_posicionNull = true;
                this.oi_indic_activo = "4";
                this.f_inactividad = f_egreso;
                this.f_egreso = f_egreso;
                this.oi_motivo_eg_per = oi_motivo_eg_per;
                this.o_egreso = o_egreso;

                NomadEnvironment.GetCurrentTransaction().Save(this);
                NomadEnvironment.GetCurrentTransaction().Commit();

            }
            catch (Exception e)
            {
                NomadEnvironment.GetCurrentTransaction().Rollback();
                throw new NomadAppException("Error al egresar el legajo. " + e.Message);
            }

            PostEgreso(this.Id);
        }

        public void PreEgreso_Personal()
        {
            // Verificar que la persona no haya egresado
            if (!this.f_egresoNull)
                throw new NomadAppException("Personal Egresado");

            // Si no Existe, Crear un registro de Ingresos
            if (this.INGRESOS_PER.Count == 0 && !this.f_ingresoNull)
            {
                NucleusRH.Base.Personal.LegajoEmpresa.INGRESOS_PER ingreso = new NucleusRH.Base.Personal.LegajoEmpresa.INGRESOS_PER();
                ingreso.f_ingreso = this.f_ingreso;
                this.INGRESOS_PER.Add(ingreso);
            }
            // Si no Existe, Crear un registro de Categorias
            if (this.CATEG_PER.Count == 0 && !this.oi_categoria_ultNull)
            {
                NucleusRH.Base.Personal.LegajoEmpresa.CATEGORIA_PER categoria = new NucleusRH.Base.Personal.LegajoEmpresa.CATEGORIA_PER();
                this.CATEG_PER.Add(categoria);
                categoria.f_ingreso = this.f_ingreso;
                categoria.oi_categoria = this.oi_categoria_ult;
            }
            // Si no Existe, Crear un registro de Centros de Costo
            if (this.CCOSTO_PER.Count == 0 && !this.oi_ctro_costo_ultNull)
            {
                NucleusRH.Base.Personal.LegajoEmpresa.CCOSTO_PER ccosto = new NucleusRH.Base.Personal.LegajoEmpresa.CCOSTO_PER();
                this.CCOSTO_PER.Add(ccosto);
                ccosto.f_ingreso = this.f_ingreso;
                ccosto.oi_centro_costo = this.oi_ctro_costo_ult;
            }
            // Si no Existe, Crear un registro de Posiciones
            if (this.POSIC_PER.Count == 0 && !this.oi_posicion_ultNull)
            {
                NucleusRH.Base.Personal.LegajoEmpresa.POSICION_PER posicion = new NucleusRH.Base.Personal.LegajoEmpresa.POSICION_PER();
                this.POSIC_PER.Add(posicion);
                posicion.f_ingreso = this.f_ingreso;
                posicion.oi_posicion = this.oi_posicion_ult;
            }
            // Si no Existe, Crear un registro de Puestos
            if (this.PUESTO_PER.Count == 0 && !this.oi_puesto_ultNull)
            {
                NucleusRH.Base.Personal.LegajoEmpresa.PUESTO_PER puesto = new NucleusRH.Base.Personal.LegajoEmpresa.PUESTO_PER();
                this.PUESTO_PER.Add(puesto);
                puesto.f_ingreso = this.f_ingreso;
                puesto.oi_puesto = this.oi_puesto_ult;
            }
            // Si no Existe, Crear un registro de Remuneraciones
            if (this.REMUN_PER.Count == 0 && !this.n_ult_remunNull)
            {
                NucleusRH.Base.Personal.LegajoEmpresa.REMUN_PER remun = new NucleusRH.Base.Personal.LegajoEmpresa.REMUN_PER();
                this.REMUN_PER.Add(remun);
                remun.f_desde = this.f_ingreso;
                remun.n_remun_per = this.n_ult_remun;
            }
            // Si no Existe, Crear un registro de Tipos de Personal
            if (this.TIPOSP_PER.Count == 0 && !this.oi_tipo_personalNull)
            {
                NucleusRH.Base.Personal.LegajoEmpresa.TIPOP_PER tipo_per = new NucleusRH.Base.Personal.LegajoEmpresa.TIPOP_PER();
                this.TIPOSP_PER.Add(tipo_per);
                tipo_per.f_ingreso = this.f_ingreso;
                tipo_per.oi_tipo_personal = this.oi_tipo_personal;
            }
            // Si no Existe, Crear un registro de Calendario
            if (this.CALENDARIO_PER.Count == 0 && !this.oi_calendario_ultNull)
            {
                NucleusRH.Base.Personal.LegajoEmpresa.CALENDARIO_PER cal_per = new NucleusRH.Base.Personal.LegajoEmpresa.CALENDARIO_PER();
                this.CALENDARIO_PER.Add(cal_per);
                cal_per.f_desde = this.f_ingreso;
                cal_per.oi_calendario = this.oi_calendario_ult;
            }
            // Si no Existe, Crear un registro de Equipo Móvil
            if (this.EQUIPO_PER.Count == 0 && !this.oi_equipo_mov_ultNull)
            {
                NucleusRH.Base.Personal.LegajoEmpresa.EQUIPO_PER equipo = new NucleusRH.Base.Personal.LegajoEmpresa.EQUIPO_PER();
                this.EQUIPO_PER.Add(equipo);
                equipo.f_ingreso = this.f_ingreso_equipo;
                equipo.oi_equipo_mov = this.oi_equipo_mov_ult;
            }
            // Si no Existe, Crear un registro de Ubicaciones
            if (this.UBIC_PER.Count == 0 && !this.oi_ubicacionNull)
            {
                NucleusRH.Base.Personal.LegajoEmpresa.UBICACION_PER ubicacion = new NucleusRH.Base.Personal.LegajoEmpresa.UBICACION_PER();
                this.UBIC_PER.Add(ubicacion);
                ubicacion.f_desde = this.f_ingreso;
                ubicacion.oi_ubicacion = this.oi_ubicacion;
            }

        }
        public void Ingreso_Personal()
        {
            if (this.f_ingresoNull)
                this.f_ingreso = System.DateTime.Now.Date;
            if (this.f_desde_categoriaNull && !this.oi_categoria_ultNull)
                this.f_desde_categoria = this.f_ingreso;
            if (this.f_desde_ccostoNull && !this.oi_ctro_costo_ultNull)
                this.f_desde_ccosto = this.f_ingreso;
            if (this.f_desde_puestoNull && !this.oi_puesto_ultNull)
                this.f_desde_puesto = this.f_ingreso;
            if (this.f_desde_remunNull && !this.n_ult_remunNull)
                this.f_desde_remun = this.f_ingreso;
            if (this.f_desde_posicionNull && !this.oi_posicion_ultNull)
                this.f_desde_posicion = this.f_ingreso;
            if (this.f_desde_calendarioNull && !this.oi_calendario_ultNull)
                this.f_desde_calendario = this.f_ingreso;
            if (this.f_ingreso_equipoNull && !this.oi_equipo_mov_ultNull)
                this.f_ingreso_equipo = this.f_ingreso;
            if (this.f_desde_ubicacionNull && !this.oi_ubicacionNull)
                this.f_desde_ubicacion = this.f_ingreso;

            this.oi_indic_activo = "1";
            //Se borran todos los childs for_insert
            //no se borraban los childs y los volvía a agregar nuevamente y estos pinchaban por AK
            //Así no se podía guardar el Legajo, ya que se seguían agregando childs y seguía pinchando
            ArrayList del_list = new ArrayList();
            foreach (INGRESOS_PER obj in this.INGRESOS_PER)
                if (obj.IsForInsert) del_list.Add(obj);
            foreach (INGRESOS_PER obj in del_list)
                this.INGRESOS_PER.Remove(obj);
            del_list.Clear();

            foreach (TIPOP_PER obj in this.TIPOSP_PER)
                if (obj.IsForInsert) del_list.Add(obj);
            foreach (TIPOP_PER obj in del_list)
                this.TIPOSP_PER.Remove(obj);
            del_list.Clear();

            foreach (CCOSTO_PER obj in this.CCOSTO_PER)
                if (obj.IsForInsert) del_list.Add(obj);
            foreach (CCOSTO_PER obj in del_list)
                this.CCOSTO_PER.Remove(obj);
            del_list.Clear();

            foreach (POSICION_PER obj in this.POSIC_PER)
                if (obj.IsForInsert) del_list.Add(obj);
            foreach (POSICION_PER obj in del_list)
                this.POSIC_PER.Remove(obj);
            del_list.Clear();

            foreach (PUESTO_PER obj in this.PUESTO_PER)
                if (obj.IsForInsert) del_list.Add(obj);
            foreach (PUESTO_PER obj in del_list)
                this.PUESTO_PER.Remove(obj);
            del_list.Clear();

            foreach (REMUN_PER obj in this.REMUN_PER)
                if (obj.IsForInsert) del_list.Add(obj);
            foreach (REMUN_PER obj in del_list)
                this.REMUN_PER.Remove(obj);
            del_list.Clear();

            foreach (EQUIPO_PER obj in this.EQUIPO_PER)
                if (obj.IsForInsert) del_list.Add(obj);
            foreach (EQUIPO_PER obj in del_list)
                this.EQUIPO_PER.Remove(obj);
            del_list.Clear();

            foreach (CONTRATO_PER obj in this.CONTR_PER)
                if (obj.IsForInsert) del_list.Add(obj);
            foreach (CONTRATO_PER obj in del_list)
                this.CONTR_PER.Remove(obj);
            del_list.Clear();

            foreach (CALENDARIO_PER obj in this.CALENDARIO_PER)
                if (obj.IsForInsert) del_list.Add(obj);
            foreach (CALENDARIO_PER obj in del_list)
                this.CALENDARIO_PER.Remove(obj);
            del_list.Clear();

            foreach (UBICACION_PER obj in this.UBIC_PER)
                if (obj.IsForInsert) del_list.Add(obj);
            foreach (UBICACION_PER obj in del_list)
                this.UBIC_PER.Remove(obj);
            del_list.Clear();

            NucleusRH.Base.Personal.LegajoEmpresa.INGRESOS_PER ingreso = new NucleusRH.Base.Personal.LegajoEmpresa.INGRESOS_PER();
            this.INGRESOS_PER.Add(ingreso);
            ingreso.f_ingreso = this.f_ingreso;

            if (!this.oi_tipo_personalNull)
            {
                NucleusRH.Base.Personal.LegajoEmpresa.TIPOP_PER tipoPer = new NucleusRH.Base.Personal.LegajoEmpresa.TIPOP_PER();
                this.TIPOSP_PER.Add(tipoPer);
                tipoPer.f_ingreso = this.f_ingreso;
                tipoPer.oi_tipo_personal = this.oi_tipo_personal;
            }
            if (!this.oi_categoria_ultNull)
            {
                bool existe = false;
                foreach (CATEGORIA_PER obj in this.CATEG_PER)
                {
                    if (obj.f_ingreso == this.f_desde_categoria)
                        existe = true;
                }
                if (!existe)
                {
                NucleusRH.Base.Personal.LegajoEmpresa.CATEGORIA_PER categoria = new NucleusRH.Base.Personal.LegajoEmpresa.CATEGORIA_PER();
                this.CATEG_PER.Add(categoria);
                categoria.f_ingreso = this.f_desde_categoria;
                categoria.oi_categoria = this.oi_categoria_ult;
            }
            }
            if (!this.oi_ctro_costo_ultNull)
            {
                bool existe = false;
                foreach (CCOSTO_PER obj in this.CCOSTO_PER)
                {
                    if (obj.f_ingreso == this.f_desde_ccosto)
                        existe = true;
                }
                if (!existe)
                {
                NucleusRH.Base.Personal.LegajoEmpresa.CCOSTO_PER ccosto = new NucleusRH.Base.Personal.LegajoEmpresa.CCOSTO_PER();
                this.CCOSTO_PER.Add(ccosto);
                ccosto.f_ingreso = this.f_desde_ccosto;
                ccosto.oi_centro_costo = this.oi_ctro_costo_ult;
            }
            }
            if (!this.oi_posicion_ultNull)
            {
                NucleusRH.Base.Personal.LegajoEmpresa.POSICION_PER posicion = new NucleusRH.Base.Personal.LegajoEmpresa.POSICION_PER();
                this.POSIC_PER.Add(posicion);
                posicion.f_ingreso = this.f_desde_posicion;
                posicion.oi_posicion = this.oi_posicion_ult;
            }
            if (!this.oi_puesto_ultNull)
            {
                NucleusRH.Base.Personal.LegajoEmpresa.PUESTO_PER puesto = new NucleusRH.Base.Personal.LegajoEmpresa.PUESTO_PER();
                this.PUESTO_PER.Add(puesto);
                puesto.f_ingreso = this.f_desde_puesto;
                puesto.oi_puesto = this.oi_puesto_ult;
            }
            if (!this.n_ult_remunNull)
            {
                NucleusRH.Base.Personal.LegajoEmpresa.REMUN_PER remun = new NucleusRH.Base.Personal.LegajoEmpresa.REMUN_PER();
                this.REMUN_PER.Add(remun);
                remun.f_desde = this.f_desde_remun;
                remun.n_remun_per = this.n_ult_remun;
            }
            if (!this.oi_equipo_mov_ultNull)
            {
                bool existe = false;
                foreach (EQUIPO_PER obj in this.EQUIPO_PER)
                {
                    if (obj.f_ingreso == this.f_ingreso_equipo)
                        existe = true;
                }
                if (!existe)
                {
                NucleusRH.Base.Personal.LegajoEmpresa.EQUIPO_PER equipo = new NucleusRH.Base.Personal.LegajoEmpresa.EQUIPO_PER();
                this.EQUIPO_PER.Add(equipo);
                equipo.f_ingreso = this.f_ingreso_equipo;
                equipo.oi_equipo_mov = this.oi_equipo_mov_ult;
            }
            }

            if (!this.oi_contratoNull)
            {
                bool existe = false;
                foreach (CONTRATO_PER obj in this.CONTR_PER)
                {
                    if (obj.f_inicio == this.f_inicio_contrato || obj.f_inicio == this.f_ingreso)
                        existe = true;
                }
                if (!existe)
                {

                NucleusRH.Base.Personal.LegajoEmpresa.CONTRATO_PER contrato = new NucleusRH.Base.Personal.LegajoEmpresa.CONTRATO_PER();
                this.CONTR_PER.Add(contrato);
                if (!this.f_inicio_contratoNull)
                {
                    contrato.f_inicio = this.f_inicio_contrato;
                }
                else
                {
                    contrato.f_inicio = this.f_ingreso;
                }
                if (!this.oi_unidad_tiempoNull) contrato.oi_unidad_tiempo = this.oi_unidad_tiempo;
                contrato.e_duracion = this.e_duracion_ctrato;
                contrato.f_fin = this.f_fin_contrato;
                contrato.f_finNull = this.f_fin_contratoNull;
                contrato.oi_contrato = this.oi_contrato;
                this.e_q_renov_ctrato = 1;
            }
            }
            else
            {
                this.e_q_renov_ctrato = 0;
            }

            if (!this.oi_calendario_ultNull)
            {
                bool existe = false;
                foreach (CALENDARIO_PER obj in this.CALENDARIO_PER)
                {
                    if (obj.f_desde == this.f_desde_calendario)
                        existe = true;
                }
                if (!existe)
                {
                NucleusRH.Base.Personal.LegajoEmpresa.CALENDARIO_PER calendario = new NucleusRH.Base.Personal.LegajoEmpresa.CALENDARIO_PER();
                this.CALENDARIO_PER.Add(calendario);
                calendario.f_desde = this.f_desde_calendario;
                calendario.oi_calendario = this.oi_calendario_ult;
            }
            }
            if (!this.oi_ubicacionNull)
            {
                NucleusRH.Base.Personal.LegajoEmpresa.UBICACION_PER ubicacion = new NucleusRH.Base.Personal.LegajoEmpresa.UBICACION_PER();
                this.UBIC_PER.Add(ubicacion);
                ubicacion.f_desde = this.f_desde_ubicacion;
                ubicacion.oi_ubicacion = this.oi_ubicacion;
            }

        }
        public void PreReingreso_Personal()
        {
            // Verificar que la persona haya egresado
            if (this.f_egresoNull)
                throw new NomadAppException("Personal No Egresado");
            if (this.oi_indic_activo != "4")
                throw new NomadAppException("El estado no es inactivo");
        }
        public void Reingreso_Personal()
        {
            NucleusRH.Base.Personal.LegajoEmpresa.INGRESOS_PER ingreso = null;
            //Si hay algún registro anterior de Ingresos
            if(this.INGRESOS_PER.Count>0)
            ingreso = (INGRESOS_PER)this.INGRESOS_PER[this.INGRESOS_PER.Count - 1];

            if (ingreso != null)
                if (this.f_ingreso <= ingreso.f_egreso)
                    throw new NomadAppException("La fecha de ingreso debe ser posterior a " + ingreso.f_egreso.ToString("dd/MM/yyyy"));

            // Asignar f_fin_contrato
            if (ingreso != null && this.f_fin_contratoNull)
            {
                if (!ingreso.f_fin_contratoNull)
                    this.f_fin_contrato = System.DateTime.Now.Date;
                else
                    this.f_fin_contrato = ingreso.f_fin_contrato;
            }

            // Marcar como "Activo" y borrar fecha de egreso
            this.oi_indic_activo = "1";
            this.f_egresoNull = true;
            this.o_egresoNull = true;
        }
        public void Cambio_Ingreso(NucleusRH.Base.Personal.LegajoEmpresa.INGRESOS_PER ingreso)
        {
            //Controla la fecha de egreso ingresada
            if (!ingreso.f_egresoNull && ingreso.f_ingreso > ingreso.f_egreso)
                throw new NomadAppException("La fecha de egreso debe ser posterior a la fecha de ingreso");

            // Verifica que haya un ingreso anterior
            if (this.INGRESOS_PER.Count > 0)
            {
                NucleusRH.Base.Personal.LegajoEmpresa.INGRESOS_PER ing = (INGRESOS_PER)this.INGRESOS_PER[this.INGRESOS_PER.Count - 1];

                //Validaciones
                if (ing.f_egresoNull)
                    throw new NomadAppException("Debe completarse la fecha de egreso del ingreso anterior.");
                if (ingreso.f_ingreso <= ing.f_egreso)
                    throw new NomadAppException("La fecha de ingreso debe ser posterior a " + ing.f_egreso.ToString("dd/MM/yyyy"));

            }

            // Asigna f_ingreso a la persona
            this.f_ingreso = ingreso.f_ingreso;
            this.f_egreso = ingreso.f_egreso;
            this.f_egresoNull = ingreso.f_egresoNull;
            this.oi_motivo_eg_per = ingreso.oi_motivo_eg_per;
            this.oi_motivo_eg_perNull = ingreso.oi_motivo_eg_perNull;
            this.INGRESOS_PER.Add(ingreso);
        }
        public void Cambio_Puesto(NucleusRH.Base.Personal.LegajoEmpresa.PUESTO_PER puesto)
        {
            // Verifica que haya un puesto anterior
            if (this.PUESTO_PER.Count > 0)
            {
                NucleusRH.Base.Personal.LegajoEmpresa.PUESTO_PER ddoPuestoAnterior = this.GetUltimoPuesto();
                // Valida AK
                if (ddoPuestoAnterior.f_ingreso >= puesto.f_ingreso)
                    throw new NomadAppException("La fecha de ingreso debe ser posterior a la fecha de ingreso del puesto anterior (" + ddoPuestoAnterior.f_ingreso.ToString("dd/MM/yyyy") + ")");

                // Asigna f_egreso de puesto anterior
                ddoPuestoAnterior.f_egreso = puesto.f_ingreso.AddDays(-1);

                // Asigna oi_motivo_cambio de puesto anterior
                ddoPuestoAnterior.oi_motivo_cambio = puesto.oi_motivo_cambio;

                // Asigna o_cambio_puesto de puesto anterior
                ddoPuestoAnterior.o_cambio_puesto = puesto.o_cambio_puesto;
            }
            puesto.oi_motivo_cambioNull = true;
            puesto.o_cambio_puesto = null;

            /*// Cerrar Posicion
            if (this.POSIC_PER.Count > 0)
            {
                NucleusRH.Base.Personal.LegajoEmpresa.POSICION_PER posicion;
                posicion = (POSICION_PER)this.POSIC_PER[this.POSIC_PER.Count-1];
                posicion.f_egreso = puesto.f_ingreso;
            }

            //Elimina la posicion del DDO
            this.oi_posicion_ultNull = true;
            this.f_desde_posicionNull = true;
            */
            // Asigna oi_puesto_ult, f_desde_puesto a la persona
            this.oi_puesto_ult = puesto.oi_puesto;
            this.f_desde_puesto = puesto.f_ingreso;
            this.PUESTO_PER.Add(puesto);
        }
        public void EgresarPosicion(DateTime fecha, string motivo)
        {
            // Verifica que haya una posicion anterior
            if (this.POSIC_PER.Count > 0)
            {
                //Valida el AK
                NucleusRH.Base.Personal.LegajoEmpresa.POSICION_PER posic_ant = (NucleusRH.Base.Personal.LegajoEmpresa.POSICION_PER)this.POSIC_PER[this.POSIC_PER.Count - 1];
                posic_ant.f_egreso = fecha;
                posic_ant.oi_motivo_cambio = motivo;

                //Recupero la estructura de la posicion
                string parampos = "<DATO oi_posicion=\"" + posic_ant.oi_posicion + "\"/>";
                NomadXML xmlestrId = new NomadXML();
                NomadEnvironment.GetTrace().Info(parampos.ToString());
                xmlestrId.SetText(NomadEnvironment.QueryString(NucleusRH.Base.Organizacion.Puestos.POSICION.Resources.QRY_POSICION_ESTRUCTURA, parampos));
                NomadEnvironment.GetTrace().Info(xmlestrId.ToString());
                if (xmlestrId.FirstChild().GetAttr("oi_estructura") != "")
                    NucleusRH.Base.Organizacion.Puestos.POSICION.Egresar_Pos_Legajo(xmlestrId.FirstChild().GetAttr("oi_estructura"), this.Id, fecha, motivo, "");
            }

            this.oi_posicion_ultNull = true;
            this.f_desde_posicionNull = true;
            NomadEnvironment.GetTrace().Info(this.oi_posicion_ult);
        }
        public void Cambio_CCosto(NucleusRH.Base.Personal.LegajoEmpresa.CCOSTO_PER ccosto)
        {
            // Verifica que haya un centro de costo anterior
            if (this.CCOSTO_PER.Count > 0)
            {
                // Valida el AK
                NucleusRH.Base.Personal.LegajoEmpresa.CCOSTO_PER centro_costo = this.GetUltimoCCosto();
                if (centro_costo.f_ingreso >= ccosto.f_ingreso)
                    throw new NomadAppException("La fecha de ingreso al centro de costo debe ser posterior a la del centro de costo anterior (" + centro_costo.f_ingreso.ToString("dd/MM/yyyy") + ")");

                // Asigna f_egreso de centro de costo anterior
                centro_costo.f_egreso = ccosto.f_ingreso.AddDays(-1);

                // Asigna oi_motivo_cambio de centro de costo anterior
                centro_costo.oi_motivo_cambio = ccosto.oi_motivo_cambio;

                // Asigna o_cambio_ccosto de centro de costo anterior
                centro_costo.o_cambio_ccosto = ccosto.o_cambio_ccosto;
            }
            ccosto.oi_motivo_cambioNull = true;
            ccosto.o_cambio_ccostoNull = true;

            // Asigna oi_ctro_costo_ult, f_desde_ccosto a la persona
            this.oi_ctro_costo_ult = ccosto.oi_centro_costo;
            this.f_desde_ccosto = ccosto.f_ingreso;
            this.CCOSTO_PER.Add(ccosto);
        }
        public void Cambio_Categoria(NucleusRH.Base.Personal.LegajoEmpresa.CATEGORIA_PER categoria)
        {
            // Verifica que haya una categoria anterior
            if (this.CATEG_PER.Count > 0)
            {
                // Valida el AK
                NucleusRH.Base.Personal.LegajoEmpresa.CATEGORIA_PER categ = this.GetUltimaCategoria();
                if (categ.f_ingreso >= categoria.f_ingreso)
                    throw new NomadAppException("La fecha de ingreso a la categoria debe ser posterior a la de la categoria anterior (" + categ.f_ingreso.ToString("dd/MM/yyyy") + ")");

                // Asigna f_egreso de categoria anterior
                if (categ.f_egresoNull)
                    categ.f_egreso = categoria.f_ingreso;

                // Asigna oi_motivo_cambio de categoria anterior
                if (categ.oi_motivo_cambioNull)
                    categ.oi_motivo_cambio = categoria.oi_motivo_cambio;

                // Asigna o_cambio_categoria de categoria anterior
                if (categ.o_cambio_categoriaNull)
                    categ.o_cambio_categoria = categoria.o_cambio_categoria;
            }
            categoria.oi_motivo_cambioNull = true;
            categoria.o_cambio_categoriaNull = true;

            // Asigna oi_categoria_ult, f_desde_categoria a la persona
            this.oi_categoria_ult = categoria.oi_categoria;
            this.f_desde_categoria = categoria.f_ingreso;
            this.CATEG_PER.Add(categoria);
        }
        public void Cambio_Contrato(NucleusRH.Base.Personal.LegajoEmpresa.CONTRATO_PER contrato)
        {
            // Verifica que haya un contrato anterior
            if (this.CONTR_PER.Count > 0)
            {
                // Valida el AK
                NucleusRH.Base.Personal.LegajoEmpresa.CONTRATO_PER cont = (CONTRATO_PER)this.CONTR_PER[this.CONTR_PER.Count - 1];
                if (cont.f_inicio >= contrato.f_inicio)
                    throw new NomadAppException("La fecha de ingreso del contrato debe ser posterior a la del contrato anterior (" + cont.f_inicio.ToString("dd/MM/yyyy") + ")");

                // Asigna f_egreso del contrato anterior
                if (cont.f_finNull)
                    cont.f_fin = contrato.f_inicio;

                // Asigna o_contrato del contrato anterior
                if (cont.o_contrato_perNull)
                    cont.o_contrato_per = contrato.o_contrato_per;
            }
            contrato.o_contrato_perNull = true;

            // Asigna oi_contrato, f_inicio/fin a la persona
            this.oi_contrato = contrato.oi_contrato;
            this.f_inicio_contrato = contrato.f_inicio;
            if (!contrato.f_finNull) { this.f_fin_contrato = contrato.f_fin; } else { this.f_fin_contratoNull = true; }
            this.e_duracion_ctrato = contrato.e_duracion;
            this.oi_unidad_tiempo = contrato.oi_unidad_tiempo;
            this.CONTR_PER.Add(contrato);
        }
        public void Cambio_Remun(NucleusRH.Base.Personal.LegajoEmpresa.REMUN_PER remun)
        {
            // Verifica que haya una remun anterior
            if (this.REMUN_PER.Count > 0)
            {
                // Valida el AK
                NucleusRH.Base.Personal.LegajoEmpresa.REMUN_PER old_remun = this.GetUltimaRemuneracion();
                if (old_remun.f_desde >= remun.f_desde)
                    throw new NomadAppException("La fecha de desde debe ser posterior a la remuneración anterior (" + old_remun.f_desde.ToString("dd/MM/yyyy") + ")");

                // Asigna f_hasta de remun anterior
                old_remun.f_hasta = remun.f_desde;

                // Asigna oi_motivo_cambio de remun anterior
                old_remun.oi_motivo_cambio = remun.oi_motivo_cambio;

                // Asigna o_remun_per de remun anterior
                old_remun.o_remun_per = remun.o_remun_per;
            }
            remun.oi_motivo_cambioNull = true;
            remun.o_remun_perNull = true;

            // Asigna oi_remun_ult, f_desde_remun a la persona
            this.n_ult_remun = remun.n_remun_per;
            this.f_desde_remun = remun.f_desde;
            this.REMUN_PER.Add(remun);
        }
        public void Cambio_Tipo_Per(NucleusRH.Base.Personal.LegajoEmpresa.TIPOP_PER tipo_per)
        {
            // Verifica que haya un tipop_per anterior
            if (this.TIPOSP_PER.Count > 0)
            {
                // Valida el AK
                NucleusRH.Base.Personal.LegajoEmpresa.TIPOP_PER tipo_personal = this.GetUltimoTipoPersonal();
                if (tipo_personal.f_ingreso >= tipo_per.f_ingreso)
                    throw new NomadAppException("La fecha de ingreso debe ser anterior a la fecha de ingreso del tipo de personal anterior (" + tipo_personal.f_ingreso.ToString("dd/MM/yyyy") + ")");

                // Asigna f_egreso de tipop_per anterior anterior
                tipo_personal.f_egreso = tipo_per.f_ingreso;

                // Asigna oi_motivo_cambio de tipop_per anterior
                tipo_personal.oi_motivo_cambio = tipo_per.oi_motivo_cambio;

                // Asigna o_cambio_tper de tipop_per anterior
                tipo_personal.o_cambio_tper = tipo_per.o_cambio_tper;
            }
            tipo_per.oi_motivo_cambioNull = true;
            tipo_per.o_cambio_tperNull = true;

            // Asigna oi_tipo_personal a la persona
            this.oi_tipo_personal = tipo_per.oi_tipo_personal;
            this.TIPOSP_PER.Add(tipo_per);
        }
        public void Deshacer_Cambio_Posicion()
        {
            // Verifica que haya una posicion anterior
            if (this.POSIC_PER.Count > 1)
            {
                NucleusRH.Base.Personal.LegajoEmpresa.POSICION_PER pos_2 = (POSICION_PER)this.POSIC_PER[this.POSIC_PER.Count - 2];
                // Borra f_egreso de posicion anterior
                pos_2.f_egresoNull = true;

                // Borra oi_motivo_cambio de posicion anterior
                pos_2.oi_motivo_cambioNull = true;

                // Borra o_cambio_posic de posicion anterior
                pos_2.o_cambio_posicNull = true;

                // Asigna oi_posicion_ult, f_desde_posicion a la persona
                this.oi_posicion_ult = pos_2.oi_posicion;
                this.f_desde_posicion = pos_2.f_ingreso;

                //Elimina posicion ultima
                this.POSIC_PER.RemoveAt(this.POSIC_PER.Count - 1);
            }
            else
                throw new NomadAppException("El puesto es el unico, y no puede ser eliminado");
        }
        public void Asignar_Contrato(NucleusRH.Base.Personal.LegajoEmpresa.CONTRATO_PER contrato)
        {
            //Obtengo el Parametro de CODIGO de Enfermedad
            string oi_tipo_contrato = NomadEnvironment.QueryValue("PER28_CONTRATOS", "oi_tipo_contrato", "oi_contrato", contrato.oi_contrato, "", false);
            if (oi_tipo_contrato == "")
                throw new NomadAppException("El tipo de contrato no esta definido");

            //Obtengo el Parametro de CODIGO de Enfermedad
            string l_f_fin_req = NomadEnvironment.QueryValue("PER28_TIPOS_CONTR", "l_f_fin_req", "oi_tipo_contrato", oi_tipo_contrato, "", false);

            //Valida que la fecha fin de contrato no este vacía
            if (l_f_fin_req == "1" && contrato.f_finNull)
            { throw new NomadAppException("La Fecha de Fin es obligatoria para este tipo de Contrato."); }

            //Si la fecha de fin del contrato es menor a la fecha de inicio
            if (!contrato.f_finNull && contrato.f_fin < contrato.f_inicio)
            {throw new NomadAppException("La Fecha de Fin de Contrato es menor a la Fecha de Inicio de Contrato");}

            // Verifica que haya una contrato anterior
            if (this.CONTR_PER.Count > 0)
            {
                // Valida que la fecha inicio sea posterior a la fecha de fin del último contrato
                NucleusRH.Base.Personal.LegajoEmpresa.CONTRATO_PER old_contrato = (CONTRATO_PER)this.CONTR_PER[this.CONTR_PER.Count - 1];
                if (!old_contrato.f_finNull && old_contrato.f_fin >= contrato.f_inicio)
                    throw new NomadAppException("La fecha de inicio debe ser posterior a la fecha fin del contrato anterior (" + old_contrato.f_fin.ToString("dd/MM/yyyy") + ")");

                if (old_contrato.f_finNull && !old_contrato.f_inicioNull && old_contrato.f_inicio >= contrato.f_inicio)
                    throw new NomadAppException("La fecha de inicio debe ser posterior a la fecha inicio del contrato anterior (" + old_contrato.f_inicio.ToString("dd/MM/yyyy") + ")");

                // Si la fecha de finalizacion del contrato anterior es nula, le cargo el valor de la fecha de inicio de este contrato menos un día
                if (old_contrato.f_finNull)
                {
                    old_contrato.f_fin = contrato.f_inicio.AddDays(-1);
                    old_contrato.e_duracion = Convert.ToInt32((old_contrato.f_fin - old_contrato.f_inicio).TotalDays)+1;
                    old_contrato.oi_unidad_tiempo = "6";
                }

            }

            // Asigna a la persona los datos del contrato en PER02_PERSONAL_EMP
            this.f_inicio_contrato = contrato.f_inicio;
            this.oi_contrato = contrato.oi_contrato;
            if (!contrato.f_finNull) this.f_fin_contrato = contrato.f_fin;
            else this.f_fin_contratoNull = true;
            if (!contrato.e_duracionNull) this.e_duracion_ctrato = contrato.e_duracion;
            else this.e_duracion_ctratoNull = true;
            if (!contrato.oi_unidad_tiempoNull) this.oi_unidad_tiempo = contrato.oi_unidad_tiempo;
            else this.oi_unidad_tiempoNull = true;
            this.e_q_renov_ctrato = this.e_q_renov_ctratoNull ? 0 : this.e_q_renov_ctrato + 1;

            this.CONTR_PER.Add(contrato);
        }
        public void Cambio_Calendario(NucleusRH.Base.Personal.LegajoEmpresa.CALENDARIO_PER calendario)
        {
            // Verifica que haya un calendario anterior
            if (this.CALENDARIO_PER.Count > 0)
            {
                // Valida el AK
                NucleusRH.Base.Personal.LegajoEmpresa.CALENDARIO_PER calen = this.GetUltimoCalendario();
                if (calen.f_desde >= calendario.f_desde)
                    throw new NomadAppException("La fecha desde del calendario debe ser posterior a la del calendario anterior (" + calen.f_desde.ToString("dd/MM/yyyy") + ")");

                // Asigna f_hasta de calendario anterior
                calen.f_hasta = calendario.f_desde;

                // Asigna oi_motivo_cambio de calendario anterior
                calen.oi_motivo_cambio = calendario.oi_motivo_cambio;

                // Asigna o_cambio_calend de calendario anterior
                calen.o_cambio_calend = calendario.o_cambio_calend;
            }
            calendario.oi_motivo_cambioNull = true;
            calendario.o_cambio_calendNull = true;

            // Asigna oi_calendario_ult, f_desde_calendario a la persona
            this.oi_calendario_ult = calendario.oi_calendario;
            this.f_desde_calendario = calendario.f_desde;
            this.CALENDARIO_PER.Add(calendario);
        }

        // Codigo fuente en LIB
        public void Cambio_Ubicacion(NucleusRH.Base.Personal.LegajoEmpresa.UBICACION_PER ubicacion)
        {
            //Valida que la Fecha Hasta ingresada sea mayor a la Fecha Desde ingresada
            if(!ubicacion.f_hastaNull && ubicacion.f_desde>=ubicacion.f_hasta)
            {
                throw new NomadAppException("Si ingresa una fecha hasta, la misma debe ser posterior a la fecha desde ingresada");
            }

            // Verifica que haya una ubicación anterior
            if (this.UBIC_PER.Count > 0)
            {
                // Valida el AK
                NucleusRH.Base.Personal.LegajoEmpresa.UBICACION_PER ubicacion_anterior = this.GetUltimaUbicacion();
                if (ubicacion_anterior.f_desde >= ubicacion.f_desde)
                    throw new NomadAppException("La fecha desde de la ubicación debe ser posterior a la fecha desde de la ubicación anterior (" + ubicacion_anterior.f_desde.ToString("dd/MM/yyyy") + ")");

                // Asigna f_hasta de ubicacion anterior
                ubicacion_anterior.f_hasta = ubicacion.f_desde;

                // Asigna observacion de ubicacion anterior
                ubicacion_anterior.oi_motivo_cambio = ubicacion.oi_motivo_cambio;

                // Asigna f_hasta de ubicacion anterior
                ubicacion_anterior.o_cambio_ubic = ubicacion.o_cambio_ubic;
            }

            ubicacion.oi_motivo_cambioNull = true;
            ubicacion.o_cambio_ubicNull = true;

            // Asigna oi_ubicacion, f_desde_ubicacion a la persona
            this.oi_ubicacion = ubicacion.oi_ubicacion;
            this.f_desde_ubicacion = ubicacion.f_desde;
            this.UBIC_PER.Add(ubicacion);

        }

        public void Validar_Legajo_Posicion()
        {
            //PASO EL ID DEL LEGAJO A UN QRY PARA SABER SI ESTA ASIGNADO A UNA ESTRUCTURA DEL ORGANIGRAMA JERARQUICO
            NomadXML xmlFLAG;
            string param = "<DATO oi_personal_emp=\"" + this.Id + "\"/>";
            xmlFLAG = new NomadXML();
            NomadEnvironment.GetTrace().Info(param);
            xmlFLAG.SetText(NomadEnvironment.QueryString(NucleusRH.Base.Personal.LegajoEmpresa.PERSONAL_EMP.Resources.QRY_ESTRUCTURA, param));
            NomadEnvironment.GetTrace().Info(xmlFLAG.ToString());

            //SI EL ATRIBUTO FLAG VUELVE EN "1" ENTONCES EL LEGAJO ESTA ASIGNADO AL ORGANIGRAMA
            if (xmlFLAG.FirstChild().GetAttr("flag") == "1")
            {
                throw new NomadAppException("No se puede eliminar el Legajo debido a que esta asignado y activo en una posicion de la empresa");
            }
        }
        public static void Validar_Fin_Contrato()
        {
            //RECUPERO EN LA CLASE DE RESPONSABILIDADES, LAS PERSONAS A LAS QUE HAY QUE MANDARLES EL MAIL
            NucleusRH.Base.Organizacion.Responsabilidades.RESPONSABILIDAD ddoR;
            ddoR = (NucleusRH.Base.Organizacion.Responsabilidades.RESPONSABILIDAD)NucleusRH.Base.Organizacion.Responsabilidades.RESPONSABILIDAD.Get("1");

            //RECORRO LAS REFERENCIAS DE LOS REQUERIMIENTOS RECUPERADOS
            foreach (NucleusRH.Base.Organizacion.Responsabilidades.RESPONSABLE ddoRes in ddoR.RESPONSABLES)
            {
                string oi_per = ddoRes.oi_personal;
                string oi_estr = ddoRes.oi_estructura;
                NomadEnvironment.GetTrace().Info("Parametros -- " + oi_per + " - " + oi_estr);

                //TIRO UN RECURSO QUE ME TRAIGA DE ACUERDO AL PARAMETRO CORRRESPONDIENTE QUE RECUPERO EN EL RECURSO
                //LAS PERSONAS A LAS QUE SE LES VENCERÁ EL CONTRATO
                string param = "<FILTRO oi_estructura=\"" + oi_estr + "\"/>";
                Nomad.NSystem.Document.NmdXmlDocument xml_doc = null;
                xml_doc = new Nomad.NSystem.Document.NmdXmlDocument(NomadEnvironment.QueryString(NucleusRH.Base.Personal.LegajoEmpresa.PERSONAL_EMP.Resources.QRY_VENCE_CONTRATO, param));
                NomadEnvironment.GetTrace().Info(xml_doc.ToString());

                //SI EL RECURSO DEVUELVE VALORES ARMO EL MENSAJE Y LO ENVIO
                if (xml_doc.GetFirstChildDocument() != null)
                {
                    Nomad.NSystem.Document.NmdXmlDocument doc;
                    string listado = "";

                    //PARA CADA PERSONA EN EL XML RECUPERADO
                    for (doc = (Nomad.NSystem.Document.NmdXmlDocument)xml_doc.GetFirstChildDocument(); doc != null; doc = (Nomad.NSystem.Document.NmdXmlDocument)xml_doc.GetNextChildDocument())
                    {
                        //ARMO EL LISTADO DE PERSONAS
                        listado = listado + "(" + doc.GetAttribute("c_empresa").Value + " - " + doc.GetAttribute("e_numero_legajo").Value + ") ";
                        listado = listado + doc.GetAttribute("d_ape_y_nom").Value + "\n";
                    }
                    NomadEnvironment.GetTrace().Info("LISTADO --- " + listado);
                    //GENERO EL CONTENIDO DEL MENSAJE
                    string msg = "Listado de Legajos cuyo contrato esta por vencer:\n";
                    msg = msg + "\n";
                    msg = msg + "Empresa - Legajo\n";
                    msg = msg + "\n";
                    msg = msg + listado;

                    NomadEnvironment.GetTrace().Info("MSG --- " + msg);

                    //CARGO EL PERSONAL DEL RESPONSABLE RECUPERADO
                    NucleusRH.Base.Personal.Legajo.PERSONAL ddoPer;
                    ddoPer = (NucleusRH.Base.Personal.Legajo.PERSONAL)NucleusRH.Base.Personal.Legajo.PERSONAL.Get(oi_per);
                    NomadEnvironment.GetTrace().Info("PER --- " + ddoPer.SerializeAll());

                    //CREO EL MAIL
                    Nomad.Base.Mail.OutputMails.MAIL m = new Nomad.Base.Mail.OutputMails.MAIL();

                    //CREO UN NUEVO DESTINATARIO
                    Nomad.Base.Mail.OutputMails.DESTINATARIO d = new Nomad.Base.Mail.OutputMails.DESTINATARIO();

                    //LE ASIGNO LA ENTIDAD GENERICA
                    d.ENTIDAD = "000000000000-000000000000-000000000001";
                    d.MAIL_SUSTITUTO = ddoPer.d_email;
                    m.DESTINATARIOS.Add(d);

                    //SETEO ATRIBUTOS DEL MAIL
                    m.DESDE_APLICACION = "NUCLEUS-RH";
                    m.REMITENTE = "NUCLEUS-RH";
                    m.FECHA_CREACION = DateTime.Now;
                    m.ASUNTO = "NucleusRH - Notificaciones de Vencimiento de Contratos";
                    m.CONTENIDO = msg;

                    //MANDO EL MAIL
                    NomadEnvironment.GetCurrentTransaction().Save(m);
                }
            }
        }
        public void Cambio_Equipo_Per(NucleusRH.Base.Personal.LegajoEmpresa.EQUIPO_PER equipo_per)
        {
            //Controla la fecha de egreso ingresada
            if (!equipo_per.f_egresoNull && equipo_per.f_ingreso > equipo_per.f_egreso)
                throw new NomadAppException("La fecha de egreso debe ser posterior a la fecha de ingreso");

            // Verifica que haya un equipo anterior
            if (this.EQUIPO_PER.Count > 0)
            {
                NucleusRH.Base.Personal.LegajoEmpresa.EQUIPO_PER equipo = this.GetUltimoEquipoMovil();

                // Verifico fecha de ingreso del equipo
                if (!equipo.f_egresoNull && equipo_per.f_ingreso <= equipo.f_egreso)
                    throw new NomadAppException("La fecha de ingreso debe ser posterior a " + equipo.f_egreso.ToString("dd/MM/yyyy"));
                if (equipo.f_egresoNull && equipo_per.f_ingreso <= equipo.f_ingreso)
                    throw new NomadAppException("La fecha de ingreso debe ser posterior a " + equipo.f_ingreso.ToString("dd/MM/yyyy"));

                // Asigna f_egreso de equipo anterior
                if (equipo.f_egresoNull)
                    equipo.f_egreso = equipo_per.f_ingreso.AddDays(-1);

                // Asigna oi_motivo_cambio de equipo anterior
                if (equipo.oi_motivo_cambioNull)
                {
                    equipo.oi_motivo_cambio = equipo_per.oi_motivo_cambio;
                    equipo_per.oi_motivo_cambioNull = true;
                }

            }

            // Asigna oi_equipo_per_ult y, f_desde_equipo_per a la persona
            this.oi_equipo_mov_ult = equipo_per.oi_equipo_mov;
            this.f_ingreso_equipo = equipo_per.f_ingreso;
            this.EQUIPO_PER.Add(equipo_per);
        }
        private void ReponerEpp(string epp, DateTime f_epp)
        {
            foreach (NucleusRH.Base.Personal.LegajoEmpresa.EPP_PER ddoEPP in this.EPP_PER)
            {
                if (ddoEPP.oi_elemento_pp == epp)
                {
                    if (ddoEPP.f_entrega < f_epp)
                    {
                        ddoEPP.l_repuesto = true;
                    }
                }
            }
        }
        public void EliminarCategoria()
        {
            // Obtiene el ultimo de la lista para eliminar
            NucleusRH.Base.Personal.LegajoEmpresa.CATEGORIA_PER del_obj = null;
            foreach (NucleusRH.Base.Personal.LegajoEmpresa.CATEGORIA_PER obj in this.CATEG_PER)
            {
                if (!obj.IsForDelete)
                {
                    if (del_obj == null)
                    {
                        del_obj = obj;
                        continue;
                    }

                    if (obj.f_ingreso > del_obj.f_ingreso)
                    {
                        del_obj = obj;
                    }
                }
            }

            if (del_obj == null)
                throw new NomadAppException("No hay registros a eliminar");

            this.CATEG_PER.Remove(del_obj);

            // Busco el que queda como ultimo para setear los atributos del legajo
            NucleusRH.Base.Personal.LegajoEmpresa.CATEGORIA_PER ult_obj = null;
            foreach (NucleusRH.Base.Personal.LegajoEmpresa.CATEGORIA_PER obj in this.CATEG_PER)
            {
                if (!obj.IsForDelete)
                {
                    if (ult_obj == null)
                    {
                        ult_obj = obj;
                        continue;
                    }

                    if (obj.f_ingreso > ult_obj.f_ingreso)
                    {
                        ult_obj = obj;
                    }
                }
            }

            // Si al eliminar queda otro registro actualizo el legajo
            if (ult_obj != null)
            {
                this.oi_categoria_ult = ult_obj.oi_categoria;
                this.f_desde_categoria = ult_obj.f_ingreso;
            }
            else
            {
                // Si el que se elimino era el ultimo
                this.oi_categoria_ultNull = true;
                this.f_desde_categoriaNull = true;
            }
        }
        public void EliminarCentroCosto()
        {
            // Obtiene el ultimo de la lista para eliminar
            NucleusRH.Base.Personal.LegajoEmpresa.CCOSTO_PER del_obj = null;
            foreach (NucleusRH.Base.Personal.LegajoEmpresa.CCOSTO_PER obj in this.CCOSTO_PER)
            {
                if (!obj.IsForDelete)
                {
                    if (del_obj == null)
                    {
                        del_obj = obj;
                        continue;
                    }

                    if (obj.f_ingreso > del_obj.f_ingreso)
                    {
                        del_obj = obj;
                    }
                }
            }

            if (del_obj == null)
                throw new NomadAppException("No hay registros a eliminar");

            this.CCOSTO_PER.Remove(del_obj);

            // Busco el que queda como ultimo para setear los atributos del legajo
            NucleusRH.Base.Personal.LegajoEmpresa.CCOSTO_PER ult_obj = null;
            foreach (NucleusRH.Base.Personal.LegajoEmpresa.CCOSTO_PER obj in this.CCOSTO_PER)
            {
                if (!obj.IsForDelete)
                {
                    if (ult_obj == null)
                    {
                        ult_obj = obj;
                        continue;
                    }

                    if (obj.f_ingreso > ult_obj.f_ingreso)
                    {
                        ult_obj = obj;
                    }
                }
            }

            // Si al eliminar queda otro registro actualizo el legajo
            if (ult_obj != null)
            {
                this.oi_ctro_costo_ult = ult_obj.oi_centro_costo;
                this.f_desde_ccosto = ult_obj.f_ingreso;
            }
            else
            {
                // Si el que se elimino era el ultimo
                this.oi_ctro_costo_ult = "0";
                this.f_desde_ccostoNull = true;
            }
        }
        public void EliminarContrato()
        {
            // Obtiene el ultimo de la lista para eliminar
            NucleusRH.Base.Personal.LegajoEmpresa.CONTRATO_PER del_obj = null;
            foreach (NucleusRH.Base.Personal.LegajoEmpresa.CONTRATO_PER obj in this.CONTR_PER)
            {
                if (!obj.IsForDelete)
                {
                    if (del_obj == null)
                    {
                        del_obj = obj;
                        continue;
                    }

                    if (obj.f_inicio > del_obj.f_inicio)
                    {
                        del_obj = obj;
                    }
                }
            }

            if (del_obj == null)
                throw new NomadAppException("No hay registros a eliminar");

            this.CONTR_PER.Remove(del_obj);
            //int cont = 0;
            // Busco el que queda como ultimo para setear los atributos del legajo
            NucleusRH.Base.Personal.LegajoEmpresa.CONTRATO_PER ult_obj = null;
            foreach (NucleusRH.Base.Personal.LegajoEmpresa.CONTRATO_PER obj in this.CONTR_PER)
            {
                if (!obj.IsForDelete)
                {
                    if (ult_obj == null)
                    {
                        ult_obj = obj;
                        continue;
                    }

                    if (obj.f_inicio > ult_obj.f_inicio)
                    {
                        ult_obj = obj;
                    }

                    //cont++;
                }
            }

            // Si al eliminar queda otro registro actualizo el legajo
            if (ult_obj != null)
            {
                this.oi_contrato = ult_obj.oi_contrato;
                this.f_inicio_contrato = ult_obj.f_inicio;
                if (!ult_obj.f_finNull) this.f_fin_contrato = ult_obj.f_fin;
                else this.f_fin_contratoNull = true;
                if (!ult_obj.e_duracionNull) this.e_duracion_ctrato = ult_obj.e_duracion;
                else this.e_duracion_ctratoNull = true;
                if (!ult_obj.oi_unidad_tiempoNull) this.oi_unidad_tiempo = ult_obj.oi_unidad_tiempo;
                else this.oi_unidad_tiempoNull = true;
                this.e_q_renov_ctrato = this.e_q_renov_ctratoNull ? 0 : this.e_q_renov_ctrato - 1;
            }
            else
            {
                // Si el que se elimino era el ultimo
                this.oi_contratoNull = true;
                this.f_inicio_contratoNull = true;
                this.f_fin_contratoNull = true;
                this.e_duracion_ctratoNull = true;
                this.oi_unidad_tiempoNull = true;
                this.e_q_renov_ctratoNull = true;
            }
        }

        public void ModificarContrato()
        {
            //Busco el último contrato para setear los valores al legajo (único contrato modificable)
            NucleusRH.Base.Personal.LegajoEmpresa.CONTRATO_PER ult_obj = (CONTRATO_PER)this.CONTR_PER[this.CONTR_PER.Count - 1];

            //Obtengo el Parametro de CODIGO de Enfermedad
            string oi_tipo_contrato = NomadEnvironment.QueryValue("PER28_CONTRATOS","oi_tipo_contrato", "oi_contrato", ult_obj.oi_contrato, "", false);
            if (oi_tipo_contrato == "")
                throw new NomadAppException("El tipo de contrato no esta definido");

            //Obtengo el Parametro de CODIGO de Enfermedad
            string l_f_fin_req = NomadEnvironment.QueryValue("PER28_TIPOS_CONTR", "l_f_fin_req", "oi_tipo_contrato", oi_tipo_contrato, "", false);

            //Valida que la fecha fin de contrato no este vacía
            if (l_f_fin_req=="1" && ult_obj.f_finNull)
            { throw new NomadAppException("La Fecha de Fin es obligatoria para este tipo de Contrato."); }

            //Valida que la fecha fin ingresada no sea inferior a la fecha de inicio
            if (!ult_obj.f_finNull && ult_obj.f_fin < ult_obj.f_inicio)
            { throw new NomadAppException("La Fecha de Fin de Contrato es menor a la Fecha de Inicio de Contrato"); }

            //Con el último contrato seteo los datos del legajo
            this.oi_contrato = ult_obj.oi_contrato;
            this.f_inicio_contrato = ult_obj.f_inicio;
            if (!ult_obj.f_finNull) this.f_fin_contrato = ult_obj.f_fin;
            else this.f_fin_contratoNull = true;
            if (!ult_obj.e_duracionNull) this.e_duracion_ctrato = ult_obj.e_duracion;
            else this.e_duracion_ctratoNull = true;
            if (!ult_obj.oi_unidad_tiempoNull) this.oi_unidad_tiempo = ult_obj.oi_unidad_tiempo;
            else this.oi_unidad_tiempoNull = true;
        }

        public void ValidarEquipoPer()
        {
            //Busco el último equipo para validar la modificacion del mismo (único equipo modificable)
            NucleusRH.Base.Personal.LegajoEmpresa.EQUIPO_PER ult_obj = (EQUIPO_PER)this.EQUIPO_PER[this.EQUIPO_PER.Count - 1];

            //Valida que la fecha egreso ingresada no sea inferior a la fecha de ingreso
            if (!ult_obj.f_egresoNull && ult_obj.f_egreso < ult_obj.f_ingreso)
            { throw new NomadAppException("La fecha de egreso debe ser posterior a la fecha de ingreso"); }

        }

        public void ValidarIngresos()
        {
            //Busco el último ingreso para validar la modificacion del mismo (único ingreso modificable)
            NucleusRH.Base.Personal.LegajoEmpresa.INGRESOS_PER ult_obj = (INGRESOS_PER)this.INGRESOS_PER[this.INGRESOS_PER.Count - 1];

            //Valida que la fecha egreso ingresada no sea inferior a la fecha de ingreso
            if (!ult_obj.f_egresoNull && ult_obj.f_egreso < ult_obj.f_ingreso)
            { throw new NomadAppException("La fecha de egreso debe ser posterior a la fecha de ingreso"); }

        }

        public void EliminarTipoPersonal()
        {
            // Obtiene el ultimo de la lista para eliminar
            NucleusRH.Base.Personal.LegajoEmpresa.TIPOP_PER del_obj = null;
            foreach (NucleusRH.Base.Personal.LegajoEmpresa.TIPOP_PER obj in this.TIPOSP_PER)
            {
                if (!obj.IsForDelete)
                {
                    if (del_obj == null)
                    {
                        del_obj = obj;
                        continue;
                    }

                    if (obj.f_ingreso > del_obj.f_ingreso)
                    {
                        del_obj = obj;
                    }
                }
            }

            if (del_obj == null)
                throw new NomadAppException("No hay registros a eliminar");

            this.TIPOSP_PER.Remove(del_obj);

            // Busco el que queda como ultimo para setear los atributos del legajo
            NucleusRH.Base.Personal.LegajoEmpresa.TIPOP_PER ult_obj = null;
            foreach (NucleusRH.Base.Personal.LegajoEmpresa.TIPOP_PER obj in this.TIPOSP_PER)
            {
                if (!obj.IsForDelete)
                {
                    if (ult_obj == null)
                    {
                        ult_obj = obj;
                        continue;
                    }

                    if (obj.f_ingreso > ult_obj.f_ingreso)
                    {
                        ult_obj = obj;
                    }
                }
            }

            // Si al eliminar queda otro registro actualizo el legajo
            if (ult_obj != null)
            {
                this.oi_tipo_personal = ult_obj.oi_tipo_personal;
            }
            else
            {
                // Si el que se elimino era el ultimo
                this.oi_tipo_personal = "0";
            }
        }
        public void EliminarIngreso()
        {
            // Obtiene el ultimo de la lista para eliminar
            NucleusRH.Base.Personal.LegajoEmpresa.INGRESOS_PER del_obj = null;
            foreach (NucleusRH.Base.Personal.LegajoEmpresa.INGRESOS_PER obj in this.INGRESOS_PER)
            {
                if (!obj.IsForDelete)
                {
                    if (del_obj == null)
                    {
                        del_obj = obj;
                        continue;
                    }

                    if (obj.f_ingreso > del_obj.f_ingreso)
                    {
                        del_obj = obj;
                    }
                }
            }

            if (del_obj == null)
                throw new NomadAppException("No hay registros a eliminar");

            this.INGRESOS_PER.Remove(del_obj);

            // Busco el que queda como ultimo para setear los atributos del legajo
            NucleusRH.Base.Personal.LegajoEmpresa.INGRESOS_PER ult_obj = null;
            foreach (NucleusRH.Base.Personal.LegajoEmpresa.INGRESOS_PER obj in this.INGRESOS_PER)
            {
                if (!obj.IsForDelete)
                {
                    if (ult_obj == null)
                    {
                        ult_obj = obj;
                        continue;
                    }

                    if (obj.f_ingreso > ult_obj.f_ingreso)
                    {
                        ult_obj = obj;
                    }
                }
            }

            // Si al eliminar queda otro registro actualizo el legajo
            if (ult_obj != null)
            {
                this.f_ingreso = ult_obj.f_ingreso;
                this.f_egreso = ult_obj.f_egreso;
                this.f_egresoNull = ult_obj.f_egresoNull;
                this.c_egreso_afip = ult_obj.c_egreso_afip;
                this.c_egreso_afipNull = ult_obj.c_egreso_afipNull;
            }
            else
            {
                // Si el que se elimino era el ultimo
                this.f_ingreso = new DateTime(1, 1, 1, 0, 0, 0);
                this.f_egresoNull = true;
                this.c_egreso_afipNull = true;

            }
        }
        public void EliminarCalendario()
        {
            // Obtiene el ultimo de la lista para eliminar
            NucleusRH.Base.Personal.LegajoEmpresa.CALENDARIO_PER del_obj = null;
            foreach (NucleusRH.Base.Personal.LegajoEmpresa.CALENDARIO_PER obj in this.CALENDARIO_PER)
            {
                if (!obj.IsForDelete)
                {
                    if (del_obj == null)
                    {
                        del_obj = obj;
                        continue;
                    }

                    if (obj.f_desde > del_obj.f_desde)
                    {
                        del_obj = obj;
                    }
                }
            }

            if (del_obj == null)
                throw new NomadAppException("No hay registros a eliminar");

            this.CALENDARIO_PER.Remove(del_obj);

            // Busco el que queda como ultimo para setear los atributos del legajo
            NucleusRH.Base.Personal.LegajoEmpresa.CALENDARIO_PER ult_obj = null;
            foreach (NucleusRH.Base.Personal.LegajoEmpresa.CALENDARIO_PER obj in this.CALENDARIO_PER)
            {
                if (!obj.IsForDelete)
                {
                    if (ult_obj == null)
                    {
                        ult_obj = obj;
                        continue;
                    }

                    if (obj.f_desde > ult_obj.f_desde)
                    {
                        ult_obj = obj;
                    }
                }
            }

            // Si al eliminar queda otro registro actualizo el legajo
            if (ult_obj != null)
            {
                this.oi_calendario_ult = ult_obj.oi_calendario;
                this.f_desde_calendario = ult_obj.f_desde;
            }
            else
            {
                // Si el que se elimino era el ultimo
                this.oi_calendario_ult = "0";
                this.f_desde_calendarioNull = true;
            }
        }
        public void EliminarEquipo()
        {
            // Obtiene el ultimo de la lista para eliminar
            NucleusRH.Base.Personal.LegajoEmpresa.EQUIPO_PER del_obj = null;
            foreach (NucleusRH.Base.Personal.LegajoEmpresa.EQUIPO_PER obj in this.EQUIPO_PER)
            {
                if (!obj.IsForDelete)
                {
                    if (del_obj == null)
                    {
                        del_obj = obj;
                        continue;
                    }

                    if (obj.f_ingreso > del_obj.f_ingreso)
                    {
                        del_obj = obj;
                    }
                }
            }

            if (del_obj == null)
                throw new NomadAppException("No hay registros a eliminar");

            this.EQUIPO_PER.Remove(del_obj);

            // Busco el que queda como ultimo para setear los atributos del legajo
            NucleusRH.Base.Personal.LegajoEmpresa.EQUIPO_PER ult_obj = null;
            foreach (NucleusRH.Base.Personal.LegajoEmpresa.EQUIPO_PER obj in this.EQUIPO_PER)
            {
                if (!obj.IsForDelete)
                {
                    if (ult_obj == null)
                    {
                        ult_obj = obj;
                        continue;
                    }

                    if (obj.f_ingreso > ult_obj.f_ingreso)
                    {
                        ult_obj = obj;
                    }
                }
            }

            // Si al eliminar queda otro registro actualizo el legajo
            if (ult_obj != null)
            {
                this.oi_equipo_mov_ult = ult_obj.oi_equipo_mov;
                this.f_ingreso_equipo = ult_obj.f_ingreso;
            }
            else
            {
                // Si el que se elimino era el ultimo
                this.oi_equipo_mov_ultNull = true;
                this.f_ingreso_equipoNull = true;
            }
        }
        public void EliminarRemuneracion()
        {
            // Obtiene el ultimo de la lista para eliminar
            NucleusRH.Base.Personal.LegajoEmpresa.REMUN_PER del_obj = null;
            foreach (NucleusRH.Base.Personal.LegajoEmpresa.REMUN_PER obj in this.REMUN_PER)
            {
                if (!obj.IsForDelete)
                {
                    if (del_obj == null)
                    {
                        del_obj = obj;
                        continue;
                    }

                    if (obj.f_desde > del_obj.f_desde)
                    {
                        del_obj = obj;
                    }
                }
            }

            if (del_obj == null)
                throw new NomadAppException("No hay registros a eliminar");

            this.REMUN_PER.Remove(del_obj);

            // Busco el que queda como ultimo para setear los atributos del legajo
            NucleusRH.Base.Personal.LegajoEmpresa.REMUN_PER ult_obj = null;
            foreach (NucleusRH.Base.Personal.LegajoEmpresa.REMUN_PER obj in this.REMUN_PER)
            {
                if (!obj.IsForDelete)
                {
                    if (ult_obj == null)
                    {
                        ult_obj = obj;
                        continue;
                    }

                    if (obj.f_desde > ult_obj.f_desde)
                    {
                        ult_obj = obj;
                    }
                }
            }

            // Si al eliminar queda otro registro actualizo el legajo
            if (ult_obj != null)
            {
                this.n_ult_remun = ult_obj.n_remun_per;
                this.f_desde_remun = ult_obj.f_desde;
            }
            else
            {
                // Si el que se elimino era el ultimo
                this.n_ult_remunNull = true;
                this.f_desde_remunNull = true;
            }
        }

        public void EliminarUbicacion()
        {
            // Obtiene el ultimo de la lista para eliminar
            NucleusRH.Base.Personal.LegajoEmpresa.UBICACION_PER del_obj = null;
            foreach (NucleusRH.Base.Personal.LegajoEmpresa.UBICACION_PER obj in this.UBIC_PER)
            {
                if (!obj.IsForDelete)
                {
                    if (del_obj == null)
                    {
                        del_obj = obj;
                        continue;
                    }

                    if (obj.f_desde > del_obj.f_desde)
                    {
                        del_obj = obj;
                    }
                }
            }

            if (del_obj == null)
                throw new NomadAppException("No hay registros a eliminar");

            this.UBIC_PER.Remove(del_obj);

            // Busco el que queda como ultimo para setear los atributos del legajo
            NucleusRH.Base.Personal.LegajoEmpresa.UBICACION_PER ult_obj = null;
            foreach (NucleusRH.Base.Personal.LegajoEmpresa.UBICACION_PER obj in this.UBIC_PER)
            {
                if (!obj.IsForDelete)
                {
                    if (ult_obj == null)
                    {
                        ult_obj = obj;
                        continue;
                    }

                    if (obj.f_desde > ult_obj.f_desde)
                    {
                        ult_obj = obj;
                    }
                }

            }

            // Si al eliminar queda otro registro actualizo el legajo
            if (ult_obj != null)
            {
                this.oi_ubicacion = ult_obj.oi_ubicacion;
                this.f_desde_ubicacion = ult_obj.f_desde;
            }
            else
            {
                // Si el que se elimino era el ultimo
                this.oi_ubicacion = "0";
                this.f_desde_ubicacionNull = true;
            }

        }

        public static void GuiaMultiple(Nomad.NSystem.Document.NmdXmlDocument SOURCE, ref Nomad.NSystem.Document.NmdXmlDocument OUT)
        {
            for (Nomad.NSystem.Document.NmdDocument xmlCur = SOURCE.GetFirstChildDocument().GetFirstChildDocument(); xmlCur != null; xmlCur = SOURCE.GetFirstChildDocument().GetNextChildDocument())
            {
                OUT.GetFirstChildDocument().AddChildDocument(xmlCur.ToString());
            }
        }

        public void Guardar_Reingreso()
        {
            this.Reingreso_Personal();
            this.Ingreso_Personal();
            NomadEnvironment.GetCurrentTransaction().SaveRefresh(this);
            Reingresar(this.Id);
        }

        public void GenerarLicencia(string oiLic, DateTime f_desde, DateTime f_hasta)
        {
            ValidarLicencia(f_desde, f_hasta);

            //Genero la Licencia
            TimeSpan a = new TimeSpan();
            a = f_hasta.Subtract(f_desde.Date);
            int cant = Convert.ToInt32(a.TotalDays);

            NucleusRH.Base.Personal.LegajoEmpresa.LICENCIA_PER DDOLICPER = new NucleusRH.Base.Personal.LegajoEmpresa.LICENCIA_PER();

            DDOLICPER.e_cant_dias = cant + 1;
            DDOLICPER.f_inicio = f_desde;
            DDOLICPER.f_fin = f_hasta;
            DDOLICPER.oi_licencia = oiLic;
            DDOLICPER.e_anio_corresp = f_desde.Year;
            DDOLICPER.f_carga = System.DateTime.Now;

            this.LICEN_PER.Add(DDOLICPER);
        }

        public void GenerarLicencia(string oiLic, DateTime f_desde, DateTime f_hasta, string obs)
        {
            ValidarLicencia(f_desde, f_hasta);

            //Genero la Licencia
            TimeSpan a = new TimeSpan();
            a = f_hasta.Subtract(f_desde.Date);
            int cant = Convert.ToInt32(a.TotalDays);

            NucleusRH.Base.Personal.LegajoEmpresa.LICENCIA_PER DDOLICPER = new NucleusRH.Base.Personal.LegajoEmpresa.LICENCIA_PER();

            DDOLICPER.e_cant_dias = cant + 1;
            DDOLICPER.f_inicio = f_desde;
            DDOLICPER.f_fin = f_hasta;
            DDOLICPER.oi_licencia = oiLic;
            DDOLICPER.e_anio_corresp = f_desde.Year;
            DDOLICPER.f_carga = System.DateTime.Now;
            DDOLICPER.o_licencia_per = obs;

            this.LICEN_PER.Add(DDOLICPER);
        }

        public void GenerarLicencia(string oiLic, DateTime f_desde, DateTime f_hasta, int cant_dias, DateTime f_carga)
        {
            ValidarLicencia(f_desde, f_hasta);

            //Genero la Licencia
            TimeSpan a = new TimeSpan();
            a = f_hasta.Subtract(f_desde.Date);
            int cant = Convert.ToInt32(a.TotalDays);

            NucleusRH.Base.Personal.LegajoEmpresa.LICENCIA_PER DDOLICPER = new NucleusRH.Base.Personal.LegajoEmpresa.LICENCIA_PER();

            DDOLICPER.e_cant_dias = cant_dias;
            DDOLICPER.f_inicio = f_desde;
            DDOLICPER.f_fin = f_hasta;
            DDOLICPER.oi_licencia = oiLic;
            DDOLICPER.e_anio_corresp = f_desde.Year;
            DDOLICPER.f_carga = f_carga;

            this.LICEN_PER.Add(DDOLICPER);
        }
        public void GenerarLicencia(string oiLic, DateTime f_desde, DateTime f_hasta, int cant_dias, DateTime f_carga, Nomad.NSystem.Proxy.NomadXML xmlPERIODOS)
        {
            ValidarLicencia(f_desde, f_hasta);

            //Genero la Licencia
            TimeSpan a = new TimeSpan();
            a = f_hasta.Subtract(f_desde.Date);
            int cant = Convert.ToInt32(a.TotalDays);

            NucleusRH.Base.Personal.LegajoEmpresa.LICENCIA_PER DDOLICPER = new NucleusRH.Base.Personal.LegajoEmpresa.LICENCIA_PER();

            DDOLICPER.e_cant_dias = cant_dias;
            DDOLICPER.f_inicio = f_desde;
            DDOLICPER.f_fin = f_hasta;
            DDOLICPER.oi_licencia = oiLic;
            DDOLICPER.e_anio_corresp = f_desde.Year;
            DDOLICPER.f_carga = f_carga;

            for (NomadXML xmlP = xmlPERIODOS.FirstChild(); xmlP != null; xmlP = xmlP.Next())
            {
                NucleusRH.Base.Personal.LegajoEmpresa.LIC_PERIODO ddoLICP = new NucleusRH.Base.Personal.LegajoEmpresa.LIC_PERIODO();
                ddoLICP.e_anio = xmlP.GetAttrInt("e_ano");
                ddoLICP.e_cant = xmlP.GetAttrInt("e_cant_dias");

                DDOLICPER.LIC_PERIODO.Add(ddoLICP);
            }

            this.LICEN_PER.Add(DDOLICPER);
        }

    public void GenerarLicencia(string oiLic, DateTime f_desde, DateTime f_hasta, int cant_dias, DateTime f_carga, Nomad.NSystem.Proxy.NomadXML xmlPERIODOS, string obs)
        {
            ValidarLicencia(f_desde, f_hasta);

            //Genero la Licencia
            TimeSpan a = new TimeSpan();
            a = f_hasta.Subtract(f_desde.Date);
            int cant = Convert.ToInt32(a.TotalDays);

            NucleusRH.Base.Personal.LegajoEmpresa.LICENCIA_PER DDOLICPER = new NucleusRH.Base.Personal.LegajoEmpresa.LICENCIA_PER();

            DDOLICPER.e_cant_dias = cant_dias;
            DDOLICPER.f_inicio = f_desde;
            DDOLICPER.f_fin = f_hasta;
            DDOLICPER.oi_licencia = oiLic;
            DDOLICPER.e_anio_corresp = f_desde.Year;
            DDOLICPER.f_carga = f_carga;
            DDOLICPER.o_licencia_per = obs;

            for (NomadXML xmlP = xmlPERIODOS.FirstChild(); xmlP != null; xmlP = xmlP.Next())
            {
                NucleusRH.Base.Personal.LegajoEmpresa.LIC_PERIODO ddoLICP = new NucleusRH.Base.Personal.LegajoEmpresa.LIC_PERIODO();
                ddoLICP.e_anio = xmlP.GetAttrInt("e_ano");
                ddoLICP.e_cant = xmlP.GetAttrInt("e_cant_dias");

                DDOLICPER.LIC_PERIODO.Add(ddoLICP);
            }

            this.LICEN_PER.Add(DDOLICPER);
        }

        //Se le agrega al GenerarLicencia ya existente el parametro l_interfaz, l_habiles y d_habiles
        public void GenerarLicencia(string oiLic, DateTime f_desde, DateTime f_hasta, int cant_dias, DateTime f_carga,bool l_interfaz ,Nomad.NSystem.Proxy.NomadXML xmlPERIODOS, bool l_habiles, string d_habiles)
        {
            ValidarLicencia(f_desde, f_hasta);

            //Genero la Licencia
            TimeSpan a = new TimeSpan();
            a = f_hasta.Subtract(f_desde.Date);
            int cant = Convert.ToInt32(a.TotalDays);

            NucleusRH.Base.Personal.LegajoEmpresa.LICENCIA_PER DDOLICPER = new NucleusRH.Base.Personal.LegajoEmpresa.LICENCIA_PER();

            DDOLICPER.e_cant_dias = cant_dias;
            DDOLICPER.f_inicio = f_desde;
            DDOLICPER.f_fin = f_hasta;
            DDOLICPER.oi_licencia = oiLic;
            DDOLICPER.e_anio_corresp = f_desde.Year;
            DDOLICPER.f_carga = f_carga;
            DDOLICPER.l_interfaz = l_interfaz;
            DDOLICPER.l_habiles = l_habiles;
            DDOLICPER.d_habiles = d_habiles;

            for (NomadXML xmlP = xmlPERIODOS.FirstChild(); xmlP != null; xmlP = xmlP.Next())
            {
                NucleusRH.Base.Personal.LegajoEmpresa.LIC_PERIODO ddoLICP = new NucleusRH.Base.Personal.LegajoEmpresa.LIC_PERIODO();
                ddoLICP.e_anio = xmlP.GetAttrInt("e_ano");
                ddoLICP.e_cant = xmlP.GetAttrInt("e_cant_dias");

                DDOLICPER.LIC_PERIODO.Add(ddoLICP);
            }

            this.LICEN_PER.Add(DDOLICPER);
        }
        public void ValidarLicencia(DateTime f_desde, DateTime f_hasta)
        {
            //Valido f_hasta mayor a f_desde
            if (f_hasta < f_desde)
                throw new NomadAppException("La fecha de finalización de la licencia debe ser mayor o igual a la fecha de inicio");

            //Valido solapamiento
            foreach (NucleusRH.Base.Personal.LegajoEmpresa.LICENCIA_PER DDOLP in this.LICEN_PER)
            {
                DateTime fi = DDOLP.f_inicio;
                DateTime ff = new DateTime();

                if (!DDOLP.f_interrupcionNull)
                    ff = DDOLP.f_interrupcion;
                else
                    ff = DDOLP.f_fin;

                if (f_desde > ff) continue;
                if (f_hasta < fi) continue;

                throw new NomadAppException("La licencia a generar se solapa con otra licencia");
            }
        }
        public void Asignar_Posicion(NucleusRH.Base.Personal.LegajoEmpresa.POSICION_PER posicion)
        {
            //OBTENGO LA ESTRUCTURA DE LA POSICION
            NomadXML xmlestrId;
            string parampos = "<DATO oi_posicion=\"" + posicion.oi_posicion + "\"/>";
            xmlestrId = new NomadXML();
            NomadEnvironment.GetTrace().Info(parampos.ToString());
            xmlestrId.SetText(NomadEnvironment.QueryString(NucleusRH.Base.Organizacion.Puestos.POSICION.Resources.QRY_POSICION_ESTRUCTURA, parampos));
            NomadEnvironment.GetTrace().Info(xmlestrId.ToString());
            if (xmlestrId.FirstChild().GetAttr("oi_estructura") != "")
            {
                //CARGO LA ESTRUCTURA

                NucleusRH.Base.Organizacion.Clases_Organizativas.ESTRUCTURA ddoEstr = NucleusRH.Base.Organizacion.Clases_Organizativas.ESTRUCTURA.Get(xmlestrId.FirstChild().GetAttr("oi_estructura"), false);

                //AGREGO LA PERSONA A LA ESTRUCTURA
                //CREO UN NUEVO ESTRUCPERS
                NucleusRH.Base.Organizacion.Clases_Organizativas.ESTRUC_PERS ddoEstPer;
                ddoEstPer = new NucleusRH.Base.Organizacion.Clases_Organizativas.ESTRUC_PERS();
                ddoEstPer.oi_personal_emp = this.Id;
                ddoEstPer.l_responsable = false;
                ddoEstPer.oi_clase_org = xmlestrId.FirstChild().GetAttr("oi_clase_org");
                ddoEstr.ESTRUC_PERS.Add(ddoEstPer);

                this.POSIC_PER.Add(posicion);

                //INICIO UNA TRANSACCION PARA GUARDAR LOS CAMBIOS
                NomadEnvironment.GetCurrentTransaction().Begin();
                try
                {
                    NomadEnvironment.GetCurrentTransaction().SaveRefresh(ddoEstr);
                    NomadEnvironment.GetCurrentTransaction().SaveRefresh(this);
                    NomadEnvironment.GetCurrentTransaction().Commit();
                }
                catch (Exception e)
                {
                    NomadEnvironment.GetCurrentTransaction().Rollback();
                    throw new NomadAppException("Error al asignar el legajo. " + e.Message);
                }
            }
        }
        public void Cambio_Posicion(NucleusRH.Base.Personal.LegajoEmpresa.POSICION_PER posicion)
        {
            //OBTENGO LA ESTRUCTURA DE LA POSICION ANTERIOR
            NomadXML xmlestrOldId;
            string parampos = "<DATO oi_posicion=\"" + this.oi_posicion_ult + "\"/>";
            xmlestrOldId = new NomadXML();
            NomadLog.Info(parampos.ToString());
            xmlestrOldId.SetText(NomadEnvironment.QueryString(NucleusRH.Base.Organizacion.Puestos.POSICION.Resources.QRY_POSICION_ESTRUCTURA, parampos));
            NomadLog.Info(xmlestrOldId.ToString());

            //TIRO UN QRY Y RECUPERO EL ID DEL ESTRUC PERS ANTERIOR
            NomadXML xmlperestrId;
            string paramestr = "<DATO oi_personal_emp=\"" + this.Id + "\" oi_estructura=\"" + xmlestrOldId.FirstChild().GetAttr("oi_estructura") + "\"/>";
            xmlperestrId = new NomadXML();
            NomadLog.Info(paramestr.ToString());
            xmlperestrId.SetText(NomadEnvironment.QueryString(NucleusRH.Base.Organizacion.Puestos.POSICION.Resources.QRY_ESTRUCTURA_PERSONA, paramestr));
            NomadLog.Info(xmlperestrId.ToString());

            //OBTENGO LA ESTRUCT PER ANTERIOR PARA ACTUALIZARLA
            NucleusRH.Base.Organizacion.Estructuras2.ESTRUC_PERS_AUX ddoESTRPER = NucleusRH.Base.Organizacion.Estructuras2.ESTRUC_PERS_AUX.Get(xmlperestrId.FirstChild().GetAttr("oi_estruc_pers"), false);

            //OBTENGO LA ESTRUCTURA DE LA POSICION ANTERIOR
            NomadXML xmlestrNewId;
            parampos = "<DATO oi_posicion=\"" + posicion.oi_posicion + "\"/>";
            xmlestrNewId = new NomadXML();
            NomadLog.Info(parampos.ToString());
            xmlestrNewId.SetText(NomadEnvironment.QueryString(NucleusRH.Base.Organizacion.Puestos.POSICION.Resources.QRY_POSICION_ESTRUCTURA, parampos));
            NomadLog.Info(xmlestrNewId.ToString());

            //CARGO LA ESTRUCTURA DE LA POSICION NUEVA
            NucleusRH.Base.Organizacion.Clases_Organizativas.ESTRUCTURA ddoEstr = NucleusRH.Base.Organizacion.Clases_Organizativas.ESTRUCTURA.Get(xmlestrNewId.FirstChild().GetAttr("oi_estructura"), false);

            //ASIGNO LA POSICION ULTIMA Y LA FECHA DE INGRESO A LA MISMA EN EL LEGAJO
            this.f_desde_posicion = posicion.f_ingreso;
            this.oi_posicion_ult = posicion.oi_posicion;

            //CARGO LA POSICION ANTERIOR
            NucleusRH.Base.Personal.LegajoEmpresa.POSICION_PER ddoPosicionAnterior = this.GetUltimaPosicion();

            //CIERRO LA POSICION ANTERIOR
            //ASIGNO LA FECHA DE EGRESO DEL PUESTO ANTERIOR COMO LA FECHA DE INGRESO A LA POSICION
            ddoPosicionAnterior.f_egreso = posicion.f_ingreso.AddDays(-1);
            //ASIGNO EL MOTIVO DE CAMBIO
            ddoPosicionAnterior.oi_motivo_cambio = "3";

            //AGREGO LA PERSONA A LA POSICION Y A LA ESTRUCTURA
            //Se cambia de PADRE la estruc_per anterior y se le asigna el nuevo padre
            ddoESTRPER.l_responsable = false;
            ddoESTRPER.oi_estructura = xmlestrNewId.FirstChild().GetAttrInt("oi_estructura");

            //CREO UNA PERSONA POSICION
            NucleusRH.Base.Personal.LegajoEmpresa.POSICION_PER ddoPOSPER;
            ddoPOSPER = new NucleusRH.Base.Personal.LegajoEmpresa.POSICION_PER();
            ddoPOSPER.f_ingreso = posicion.f_ingreso;
            ddoPOSPER.oi_personal_emp = this.id;
            ddoPOSPER.oi_posicion = posicion.oi_posicion;

            this.POSIC_PER.Add(ddoPOSPER);

            NomadEnvironment.GetCurrentTransaction().Begin();
            try
            {
                NomadEnvironment.GetCurrentTransaction().SaveRefresh(ddoESTRPER);
                NomadEnvironment.GetCurrentTransaction().SaveRefresh(this);
                NomadEnvironment.GetCurrentTransaction().Commit();
            }
            catch (Exception e)
            {
                NomadEnvironment.GetCurrentTransaction().Rollback();
                throw new NomadAppException("Error al cambiar de Posicion al Legajo. " + e.Message);
            }
        }
        public static void AddSeguridad(string id, string id_clase, string id_estructura)
        {
            try
            {
                NomadEnvironment.GetCurrentTransaction().Begin();
                if (id_estructura != "")
                {
                    //Tiro un qry para armar el Organigrama de segudirad hasta la estructura necesaria
                    string param = "<DATA oi_estructura=\"" + id_estructura + "\"/>";
                    NomadXML xmlCODE = new NomadXML();
                    NomadEnvironment.GetTrace().Info(param);
                    xmlCODE.SetText(NomadEnvironment.QueryString(NucleusRH.Base.Organizacion.Clases_Organizativas.CLASE_ORG.Resources.QRY_POLICY_ESTR, param));

                    //Seteo el policy al legajo
                    NucleusRH.Base.Personal.LegajoEmpresa.PERSONAL_EMP ddoLEG = NucleusRH.Base.Personal.LegajoEmpresa.PERSONAL_EMP.Get(id);
                    ddoLEG.Security.Policy = xmlCODE.FirstChild().GetAttr("coderet");

                    //Seteo el policy a la persona
                    NucleusRH.Base.Personal.Legajo.PERSONAL ddoPER = NucleusRH.Base.Personal.Legajo.PERSONAL.Get(ddoLEG.oi_personal);
                    ddoPER.Security.Policy = xmlCODE.FirstChild().GetAttr("coderet");

                    //Armo un NomadXML para pasar el id del legajo como parametro
                    NomadXML xmlleg = new NomadXML("<ROWS><ROW id=\"" + id + "\"/></ROWS>");

                    //Agrego el legajo a la estructura de seguridad
                    NucleusRH.Base.Organizacion.Clases_Organizativas.CLASE_ORG.AddLegajo(id_clase, id_estructura, xmlleg, false);

                    NomadEnvironment.GetCurrentTransaction().SaveRefresh(ddoPER);
                    NomadEnvironment.GetCurrentTransaction().SaveRefresh(ddoLEG);
                }
                NomadEnvironment.GetCurrentTransaction().Commit();
            }
            catch (Exception e)
            {
                NomadEnvironment.GetCurrentTransaction().Rollback();
                throw new NomadAppException("Error al establecer la seguridad del legajo. " + e.Message);
            }
        }
        public static void ChangeSeguridad(string id, string id_clase, string id_estructura)
        {
            //Recupero la estructura de seguridad donde se encuentra el legajo
            string param = "<DATA oi_personal_emp=\"" + id + "\"/>";
            NomadXML xmlESTR = new NomadXML();
            NomadEnvironment.GetTrace().Info(param);
            xmlESTR.SetText(NomadEnvironment.QueryString(NucleusRH.Base.Organizacion.Clases_Organizativas.CLASE_ORG.Resources.QRY_POLICY_PER, param));

            NomadLog.Info("xmlESTR -- " + xmlESTR.ToString());
            NomadLog.Info("xmlESTR.FirstChild().GetAttr -- " + xmlESTR.FirstChild().GetAttr("oi_estructura"));
            if (xmlESTR.FirstChild().GetAttr("oi_estructura") == "")
            {
                NomadLog.Info("1.a-- ");
                if (!string.IsNullOrEmpty(id_estructura))
                {
                    NomadLog.Info("1.b-- ");
                    AddSeguridad(id, id_clase, id_estructura);
                }
            }
            else if (xmlESTR.FirstChild().GetAttr("oi_estructura") != id_estructura)
            {
                //Elimino la policy
                NomadLog.Info("CleanSeguridad");
                CleanSeguridad(id, xmlESTR.FirstChild().GetAttr("oi_estructura"));
                if (!string.IsNullOrEmpty(id_estructura))
                    AddSeguridad(id, id_clase, id_estructura);
            }
        }
        public static void CleanSeguridad(string id, string id_estructura)
        {
            //Seteo el policy al legajo
            NomadLog.Info("CLEANSEG");
            NucleusRH.Base.Personal.LegajoEmpresa.PERSONAL_EMP ddoLEG = NucleusRH.Base.Personal.LegajoEmpresa.PERSONAL_EMP.Get(id);
            ddoLEG.Security.Policy = null;

            //Seteo el policy a la persona
            NucleusRH.Base.Personal.Legajo.PERSONAL ddoPER = NucleusRH.Base.Personal.Legajo.PERSONAL.Get(ddoLEG.oi_personal);
            ddoPER.Security.Policy = null;

            try
            {
                NomadEnvironment.GetCurrentTransaction().Begin();
                NomadEnvironment.GetCurrentTransaction().SaveRefresh(ddoPER);
                NomadEnvironment.GetCurrentTransaction().SaveRefresh(ddoLEG);

                //Elimino el legajo de la estructura
                NomadLog.Info("QuitarLegajo -- " + id_estructura + " | " + id);
                NucleusRH.Base.Organizacion.Clases_Organizativas.CLASE_ORG.QuitarLegajo(id_estructura, id);
                NomadEnvironment.GetCurrentTransaction().Commit();
            }
            catch (Exception e)
            {
                NomadEnvironment.GetCurrentTransaction().Rollback();
                throw new NomadAppException("Error al limpiar la seguridad del legajo. " + e.Message);
            }
        }
        public static void CargarPosicion(Nomad.NSystem.Proxy.NomadXML param)
        {
            int Linea = 0, Errores = 0;

            NomadBatch objBatch;
            objBatch = NomadBatch.GetBatch("Iniciando...", "Carga de Posiciones");

            DateTime fCompare = new DateTime(1900, 1, 1);

            ArrayList lista = (ArrayList)param.FirstChild().GetElements("ROW");

            //RECORRO LOS IDS QUE ENTRAN Y PREGUNTO SI ALGUN ID NO ESTA EN LA HASH, PORQ DE SER ASI HAY QUE AGREGARLO
            for (int xml = 0; xml < lista.Count; xml++)
            {
                Linea++;
                NomadXML row = (NomadXML)lista[xml];
                objBatch.SetPro(0, 100, lista.Count, xml);
                objBatch.SetMess("Incorporando posición " + (xml + 1) + " de " + lista.Count);

                //Inicio la Transaccion
                try
                {

                    //Valido atributos obligatorios
                    if (row.GetAttr("oi_empresa") == "")
                    {
                        objBatch.Err("No se especificó la Empresa, se rechaza el registro - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }
                    if (row.GetAttr("oi_personal_emp") == "")
                    {
                        objBatch.Err("No se especificó el Número de Legajo, se rechaza el registro - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }

                    //Cargo el Legajo
                    NucleusRH.Base.Personal.LegajoEmpresa.PERSONAL_EMP DDOLEG = NucleusRH.Base.Personal.LegajoEmpresa.PERSONAL_EMP.Get(row.GetAttr("oi_personal_emp"));
                    NomadEnvironment.GetTrace().Info("LEG -- " + DDOLEG.SerializeAll());
                    string oi_posicion_ultima = DDOLEG.oi_posicion_ult;

                    if (row.GetAttr("oi_puesto") == "")
                    {
                        objBatch.Err("No se especificó el Puesto, se rechaza el registro '" + DDOLEG.descr + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }
                    if (row.GetAttr("oi_posicion") == "")
                    {
                        objBatch.Err("No se especificó la Posición, se rechaza el registro '" + DDOLEG.descr + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }
                    if (row.GetAttr("f_desde") == "")
                    {
                        objBatch.Err("No se especificó la Fecha desde Posición, se rechaza el registro '" + DDOLEG.descr + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }

                    DateTime fecha = Nomad.NSystem.Functions.StringUtil.str2date(row.GetAttr("f_desde"));

                    if (fecha < fCompare)
                    {
                        objBatch.Err("No se especificó la Fecha desde Posición, se rechaza el registro '" + DDOLEG.descr + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }

                    //Valido que tenga un organigrama vigente la empresa
                    NomadXML vigente = NomadEnvironment.QueryNomadXML(PERSONAL_EMP.Resources.QRY_VIGENTE, "<PARAM oi_empresa=\"" + row.GetAttr("oi_empresa") + "\"/>");
                    NomadEnvironment.GetTrace().Info("VIGENTE-- " + vigente.FirstChild().GetAttr("vigente"));

                    if (vigente.FirstChild().GetAttr("vigente") == "0")
                    {
                        objBatch.Err("La empresa no tiene un Organigrama Vigente, se rechaza el registro '" + DDOLEG.descr + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }
                    //Verifico que el legajo este activo en la empresa
                    if (DDOLEG.oi_indic_activo != "1")
                    {
                        objBatch.Err("El Legajo no se encuentra Activo en la Empresa, se rechaza el registro '" + DDOLEG.descr + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }

                    //Verifico si la posición a actualizar es la actual
                    if (row.GetAttr("oi_posicion") == DDOLEG.oi_posicion_ult && DDOLEG.f_egresoNull)
                    {
                        objBatch.Err("La posición a actualizar es la actual, se rechaza el registro '" + DDOLEG.descr + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }

                    //Verifico que la fecha desde ingresada sea superior a la fecha desde posicion del legajo
                    if (DDOLEG.f_desde_posicion > fecha)
                    {
                        objBatch.Err("La fecha desde de posición indicada es inferior a la última fecha desde de la posición, se rechaza el registro '" + DDOLEG.descr + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }

                    //PUESTO
                    NucleusRH.Base.Personal.LegajoEmpresa.PUESTO_PER DDOPUESTOLEG = new NucleusRH.Base.Personal.LegajoEmpresa.PUESTO_PER();
                    DDOPUESTOLEG.oi_puesto = row.GetAttr("oi_puesto");
                    DDOPUESTOLEG.f_ingreso = fecha;

                    DDOLEG.Cambio_Puesto(DDOPUESTOLEG);

                    //POSICION
                    NucleusRH.Base.Personal.LegajoEmpresa.POSICION_PER DDOPOSICLEG = new NucleusRH.Base.Personal.LegajoEmpresa.POSICION_PER();
                    DDOPOSICLEG.oi_posicion = row.GetAttr("oi_posicion");
                    DDOPOSICLEG.f_ingreso = fecha;

                    //Si oi_posicion_ultima es null - es un alta o un reingreso del legajo
                    if (DDOLEG.POSIC_PER.Count > 0 && oi_posicion_ultima != "")
                    {
                        DDOLEG.Cambio_Posicion(DDOPOSICLEG);
                    }
                    else
                    {
                        DDOLEG.oi_posicion_ult = row.GetAttr("oi_posicion");
                        DDOLEG.f_desde_posicion = fecha;

                        DDOLEG.Asignar_Posicion(DDOPOSICLEG);

                    }

                    //Grabo
                    try
                    {
                        NomadEnvironment.GetCurrentTransaction().Save(DDOLEG);
                        if (DDOLEG.POSIC_PER.Count > 0)
                        {
                            NucleusRH.Base.Organizacion.Puestos.POSICION.Cambiar_Pos_Legajo_Sectores(oi_posicion_ultima, row.GetAttr("oi_posicion"), DDOLEG.id.ToString(), fecha, "", "");
                        }
                        else
                        {
                            NucleusRH.Base.Organizacion.Puestos.POSICION.AddLegajoSectores(DDOPOSICLEG.oi_posicion, DDOLEG.id.ToString(),fecha, new DateTime(1899, 1, 1), "", "");
                        }
                    }
                    catch (Exception e)
                    {
                        objBatch.Err("Error al grabar registro " + DDOLEG.descr + "' - Linea: " + Linea.ToString() + " - " + e.Message);
                        Errores++;
                    }
                }
                catch (Exception e)
                {
                    objBatch.Err("Error desconocido en registro, por favor verifique el formato del archivo y la correcta asignación de los campos - Linea: " + Linea.ToString() + " - " + e.Message);
                    Errores++;
                }
            }

            objBatch.Log("Registros Procesados: " + Linea.ToString() + " - Importados: " + (Linea - Errores).ToString());
            objBatch.Log("Finalizado...");
        }

        public static void CargarCategoria( Nomad.NSystem.Proxy.NomadXML param)
        {
            int Linea = 0, Errores = 0;

            NomadBatch objBatch;
            objBatch = NomadBatch.GetBatch("Iniciando...", "Carga de Categorias");

            DateTime fCompare = new DateTime(1900, 1, 1);

            ArrayList lista = (ArrayList)param.FirstChild().GetElements("ROW");

            //RECORRO LOS IDS QUE ENTRAN Y PREGUNTO SI ALGUN ID NO ESTA EN LA HASH, PORQ DE SER ASI HAY QUE AGREGARLO
            for (int xml = 0; xml < lista.Count; xml++)
            {
                Linea++;
                NomadXML row = (NomadXML)lista[xml];
                objBatch.SetPro(0, 100, lista.Count, xml);
                objBatch.SetMess("Incorporando categoría " + (xml + 1) + " de " + lista.Count);

                //Inicio la Transaccion
                try
                {

                    //Valido atributos obligatorios
                    if (row.GetAttr("oi_empresa") == "")
                    {
                        objBatch.Err("No se especificó la Empresa, se rechaza el registro - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }
                    if (row.GetAttr("oi_personal_emp") == "")
                    {
                        objBatch.Err("No se especificó el Número de Legajo, se rechaza el registro - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }

                    //Cargo el Legajo
                    NucleusRH.Base.Personal.LegajoEmpresa.PERSONAL_EMP DDOLEG = NucleusRH.Base.Personal.LegajoEmpresa.PERSONAL_EMP.Get(row.GetAttr("oi_personal_emp"));
                    NomadEnvironment.GetTrace().Info("LEG -- " + DDOLEG.SerializeAll());

                    if (row.GetAttr("oi_convenio") == "")
                    {
                        objBatch.Err("No se especificó el Convenio, se rechaza el registro '" + DDOLEG.descr + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }
                    if (row.GetAttr("oi_categoria") == "")
                    {
                        objBatch.Err("No se especificó la categoría, se rechaza el registro '" + DDOLEG.descr + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }
                    if (row.GetAttr("f_desde") == "")
                    {
                        objBatch.Err("No se especificó la Fecha desde Posición, se rechaza el registro '" + DDOLEG.descr + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }

                    DateTime fecha = Nomad.NSystem.Functions.StringUtil.str2date(row.GetAttr("f_desde"));

                    if (fecha < fCompare)
                    {
                        objBatch.Err("No se especificó la Fecha desde Categoría, se rechaza el registro '" + DDOLEG.descr + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }

                    //Verifico que el legajo este activo en la empresa
                    if (DDOLEG.oi_indic_activo != "1")
                    {
                        objBatch.Err("El Legajo no se encuentra Activo en la Empresa, se rechaza el registro '" + DDOLEG.descr + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }

                    //Verifico si la categoría a actualizar es la actual
                    if (row.GetAttr("oi_categoria") == DDOLEG.oi_categoria_ult && DDOLEG.f_egresoNull)
                    {
                        objBatch.Err("La categoría a actualizar es la actual, se rechaza el registro '" + DDOLEG.descr + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }

                    //Verifico que la fecha desde ingresada sea superior a la fecha desde categoría del legajo
                    if (DDOLEG.f_desde_categoria > fecha)
                    {
                        objBatch.Err("La fecha desde de categoría indicada es inferior a la última fecha desde de la categoría, se rechaza el registro '" + DDOLEG.descr + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }

                    //CATEGORIA
                    NucleusRH.Base.Personal.LegajoEmpresa.CATEGORIA_PER DDOCATEGLEG = new NucleusRH.Base.Personal.LegajoEmpresa.CATEGORIA_PER();
                    DDOCATEGLEG.oi_categoria = row.GetAttr("oi_categoria");
                    DDOCATEGLEG.f_ingreso = fecha;

                    if (DDOLEG.CATEG_PER.Count > 0)
                    {
                        DDOLEG.Cambio_Categoria(DDOCATEGLEG);
                    }
                    else
                    {
                        DDOCATEGLEG.oi_motivo_cambioNull = true;
                        DDOCATEGLEG.o_cambio_categoriaNull = true;

                        // Asigna oi_categoria_ult, f_desde_categoria a la persona
                        DDOLEG.oi_categoria_ult = DDOCATEGLEG.oi_categoria;
                        DDOLEG.f_desde_categoria = DDOCATEGLEG.f_ingreso;
                        DDOLEG.CATEG_PER.Add(DDOCATEGLEG);
                    }

                    //Grabo
                    try
                    {
                        NomadEnvironment.GetCurrentTransaction().Save(DDOLEG);
                    }
                    catch (Exception e)
                    {
                        objBatch.Err("Error al grabar registro " + DDOLEG.descr + "' - Linea: " + Linea.ToString() + " - " + e.Message);
                        Errores++;
                    }
                }
                catch (Exception e)
                {
                    objBatch.Err("Error desconocido en registro, por favor verifique el formato del archivo y la correcta asignación de los campos - Linea: " + Linea.ToString() + " - " + e.Message);
                    Errores++;
                }
            }

            objBatch.Log("Registros Procesados: " + Linea.ToString() + " - Importados: " + (Linea - Errores).ToString());
            objBatch.Log("Finalizado...");
        }

        public static void CargarCentrosDeCostos( Nomad.NSystem.Proxy.NomadXML param)
        {
            int Linea = 0, Errores = 0;

            NomadBatch objBatch;
            objBatch = NomadBatch.GetBatch("Iniciando...", "Carga de Centros de Costos");

            DateTime fCompare = new DateTime(1900, 1, 1);

            ArrayList lista = (ArrayList)param.FirstChild().GetElements("ROW");

            //RECORRO LOS IDS QUE ENTRAN Y PREGUNTO SI ALGUN ID NO ESTA EN LA HASH, PORQ DE SER ASI HAY QUE AGREGARLO
            for (int xml = 0; xml < lista.Count; xml++)
            {
                Linea++;
                NomadXML row = (NomadXML)lista[xml];
                objBatch.SetPro(0, 100, lista.Count, xml);
                objBatch.SetMess("Incorporando Centro de costo " + (xml + 1) + " de " + lista.Count);

                //Inicio la Transaccion
                try
                {

                    //Valido atributos obligatorios
                    if (row.GetAttr("oi_empresa") == "")
                    {
                        objBatch.Err("No se especificó la Empresa, se rechaza el registro - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }
                    if (row.GetAttr("oi_personal_emp") == "")
                    {
                        objBatch.Err("No se especificó el Número de Legajo, se rechaza el registro - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }

                    //Cargo el Legajo
                    NucleusRH.Base.Personal.LegajoEmpresa.PERSONAL_EMP DDOLEG = NucleusRH.Base.Personal.LegajoEmpresa.PERSONAL_EMP.Get(row.GetAttr("oi_personal_emp"));
                    NomadEnvironment.GetTrace().Info("LEG -- " + DDOLEG.SerializeAll());

                    if (row.GetAttr("oi_ccosto") == "")
                    {
                        objBatch.Err("No se especificó el centro de costo, se rechaza el registro '" + DDOLEG.descr + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }
                    if (row.GetAttr("f_desde") == "")
                    {
                        objBatch.Err("No se especificó la Fecha desde Centro de Costo, se rechaza el registro '" + DDOLEG.descr + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }

                    DateTime fecha = Nomad.NSystem.Functions.StringUtil.str2date(row.GetAttr("f_desde"));

                    if (fecha < fCompare)
                    {
                        objBatch.Err("No se especificó la Fecha desde Centro de Costo, se rechaza el registro '" + DDOLEG.descr + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }

                    //Verifico que el legajo este activo en la empresa
                    if (DDOLEG.oi_indic_activo != "1")
                    {
                        objBatch.Err("El Legajo no se encuentra Activo en la Empresa, se rechaza el registro '" + DDOLEG.descr + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }

                    //Verifico si el centro de costo a actualizar es la actual
                    if (row.GetAttr("oi_ccosto") == DDOLEG.oi_ctro_costo_ult && DDOLEG.f_egresoNull)
                    {
                        objBatch.Err("El centro de cosoto a actualizar es la actual, se rechaza el registro '" + DDOLEG.descr + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }

                    //Verifico que la fecha desde ingresada sea superior a la fecha desde posicion del legajo
                    if (DDOLEG.f_desde_ccosto > fecha)
                    {
                        objBatch.Err("La fecha desde del centro de costo indicada es inferior a la última fecha desde del centro de costo, se rechaza el registro '" + DDOLEG.descr + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }

                    //CENTRO DE COSTO
                    NucleusRH.Base.Personal.LegajoEmpresa.CCOSTO_PER DDOCCOSTOLEG = new NucleusRH.Base.Personal.LegajoEmpresa.CCOSTO_PER();
                    DDOCCOSTOLEG.oi_centro_costo = row.GetAttr("oi_ccosto");
                    DDOCCOSTOLEG.f_ingreso = fecha;

                    if (DDOLEG.CCOSTO_PER.Count > 0)
                    {
                        DDOLEG.Cambio_CCosto(DDOCCOSTOLEG);
                    }
                    else
                    {
                        DDOCCOSTOLEG.oi_motivo_cambioNull = true;
                        DDOCCOSTOLEG.o_cambio_ccostoNull = true;

                        // Asigna oi_ctro_costo_ult, f_desde_ccosto a la persona
                        DDOLEG.oi_ctro_costo_ult = DDOCCOSTOLEG.oi_centro_costo;
                        DDOLEG.f_desde_ccosto = DDOCCOSTOLEG.f_ingreso;
                        DDOLEG.CCOSTO_PER.Add(DDOCCOSTOLEG);
                    }

                    //Grabo
                    try
                    {
                        NomadEnvironment.GetCurrentTransaction().Save(DDOLEG);
                    }
                    catch (Exception e)
                    {
                        objBatch.Err("Error al grabar registro " + DDOLEG.descr + "' - Linea: " + Linea.ToString() + " - " + e.Message);
                        Errores++;
                    }
                }
                catch (Exception e)
                {
                    objBatch.Err("Error desconocido en registro, por favor verifique el formato del archivo y la correcta asignación de los campos - Linea: " + Linea.ToString() + " - " + e.Message);
                    Errores++;
                }
            }

            objBatch.Log("Registros Procesados: " + Linea.ToString() + " - Importados: " + (Linea - Errores).ToString());
            objBatch.Log("Finalizado...");
        }

        public static void RegistrarLicencia(Nomad.NSystem.Proxy.NomadXML param)
        {
            NomadBatch objBatch;
            objBatch = NomadBatch.GetBatch("Iniciando...", "Comprobante de Licencia");
            objBatch.SetPro(0);

            //CARGA EL LEGAJO
            NucleusRH.Base.Personal.LegajoEmpresa.PERSONAL_EMP ddoPER;
            ddoPER = NucleusRH.Base.Personal.LegajoEmpresa.PERSONAL_EMP.Get(param.FirstChild().GetAttr("oi_personal_emp"), false);

            string oiLicPER = "0";

            objBatch.SetMess("Generando la Licencia...");
            try
            {
                LICENCIA_PER.ValidarAltaLicencia(ddoPER.id.ToString(), param.FirstChild().GetAttr("oi_licencia"), param.FirstChild().GetAttrDateTime("f_inicio"), param.FirstChild().GetAttrDateTime("f_fin"), param.FirstChild().GetAttrInt("e_cant_dias"), param.FirstChild().GetAttrInt("e_anio_corresp"));
            }
            catch(Exception e)
            {
                throw NomadAppException.NewMessage(e.Message);
            }
            
            if ((param.FirstChild().GetAttr("f_carga") != "" || param.FirstChild().GetAttr("f_inicio") != "") && param.FirstChild().GetAttr("f_fin") != "")
            {
                //Seteo Valores
                DateTime f_desde = param.FirstChild().GetAttrDateTime("f_inicio");
                DateTime f_hasta = param.FirstChild().GetAttrDateTime("f_fin");

                //Obtengo el Parametro de CODIGO de Accidente
                NomadXML codLic = NomadEnvironment.QueryNomadXML(PERSONAL_EMP.Resources.QRY_LIC, "<PARAM oi_licencia=\"" + param.FirstChild().GetAttr("oi_licencia") + "\"/>");
                if (codLic.FirstChild().GetAttr("c_licencia") == "")
                    throw new NomadAppException("No esta definido el codigo de Licencia por Accidente.");

                //Obtengo el Parametro de CODIGO de Accidente
                string oiLic = NomadEnvironment.QueryValue("per16_licencias", "oi_licencia", "c_licencia", codLic.FirstChild().GetAttr("c_licencia"), "", false);
                if (oiLic == "")
                    throw new NomadAppException("No esta definido el codigo de Licencia por Accidente.");

                //consulto el id de la licencia cargada en el legajo
                NomadXML xmlqry = NomadEnvironment.QueryNomadXML(PERSONAL_EMP.Resources.QRY_LICEN_PER, "<PARAM oi_personal_emp=\"" + ddoPER.Id + "\" f_inicio=\"" + f_desde.ToString("yyyyMMdd") + "\"/>");
                oiLicPER = xmlqry.FirstChild().GetAttr("oi_licencia_per");
                NomadEnvironment.GetTrace().Info("oiLicPER -- " + oiLicPER);
            }

            objBatch.SetPro(40);
            objBatch.SetMess("Actualizando licencia del legajo...");

            NucleusRH.Base.Personal.LegajoEmpresa.LICENCIA_PER ddolic;
            ddolic = new NucleusRH.Base.Personal.LegajoEmpresa.LICENCIA_PER();

            if (param.FirstChild().GetAttr("e_cant_dias") != "")
                ddolic.e_cant_dias = param.FirstChild().GetAttrInt("e_cant_dias");

            if (param.FirstChild().GetAttr("f_inicio") != "")
                ddolic.f_inicio = param.FirstChild().GetAttrDateTime("f_inicio");

            if (param.FirstChild().GetAttr("f_fin") != "")
                ddolic.f_fin = param.FirstChild().GetAttrDateTime("f_fin");

            if (param.FirstChild().GetAttr("f_carga_int") != "")
                ddolic.f_carga_int = param.FirstChild().GetAttrDateTime("f_carga_int");

            if (param.FirstChild().GetAttr("f_interrupcion") != "")
                ddolic.f_interrupcion = param.FirstChild().GetAttrDateTime("f_interrupcion");

            if (param.FirstChild().GetAttr("f_notificacion") != "")
                ddolic.f_notificacion = param.FirstChild().GetAttrDateTime("f_notificacion");

            if (param.FirstChild().GetAttr("e_anio_corresp") != "")
                ddolic.e_anio_corresp = param.FirstChild().GetAttrInt("e_anio_corresp");

            if (param.FirstChild().GetAttr("o_licencia_per") != "")
                ddolic.o_licencia_per = param.FirstChild().GetAttr("o_licencia_per");

            if (param.FirstChild().GetAttr("oi_licencia") != "")
                ddolic.oi_licencia = param.FirstChild().GetAttr("oi_licencia");

            if (param.FirstChild().GetAttr("oi_familiar_per") != "")
                ddolic.oi_familiar_per = param.FirstChild().GetAttr("oi_familiar_per");

            objBatch.SetPro(60);

            ddoPER.LICEN_PER.Add(ddolic);

            NomadEnvironment.GetCurrentTransaction().Save(ddoPER);
            objBatch.SetPro(85);
        }
        public static Nomad.NSystem.Proxy.NomadXML Certificado(ref Nomad.NSystem.Proxy.NomadXML xmlparam)
        {
            NomadXML MyXML = xmlparam.FirstChild();
            //Guardo el logo de la empresa

            if (MyXML.GetAttr("oi_empresa") != "")
            {
                NucleusRH.Base.Organizacion.Empresas.EMPRESA ddoEMP = NucleusRH.Base.Organizacion.Empresas.EMPRESA.Get(MyXML.GetAttr("oi_empresa"), false);
                if (!ddoEMP.oi_logotipoNull)
                {
                    Nomad.NSystem.Proxy.BINFile objBin;
                    NomadProxy pProxy = Nomad.NSystem.Base.NomadEnvironment.GetProxy();
                    string path = System.IO.Path.Combine(pProxy.RunPath, "TEMP");
                    objBin = pProxy.BINService().GetFile("NucleusRH.Base.Personal.Imagenes.HEAD", ddoEMP.oi_logotipo);

                    objBin.SaveFile(path);
                    MyXML.SetAttr("logo", "url(" + path + "\\NucleusRH.Base.Personal.Imagenes.HEAD." + ddoEMP.oi_logotipo + objBin.Name.Substring(objBin.Name.Length - 4, 4) + ")");
                }
            }

            return MyXML;
        }

        static public void Calcular_Tiempo(DateTime Desde, DateTime Hasta, bool ultimo, ref int anios, ref int meses, ref int dias)
        {
            NomadEnvironment.GetTrace().Info("****** Calcular Tiempo ******");
            int month;
            DateTime AuxTime;

            //Calculo la Cantidad de Messes
            month = (Hasta.Year - Desde.Year) * 12 + (Hasta.Month - Desde.Month);
            if (Desde.Day > Hasta.Month) month--;
            anios += month / 12;
            meses += month % 12;

            //Calculo la Cantidad de Dias que Sobran
            AuxTime = Desde.AddMonths(month);

            //Se agrega un día para el cálculo para el último periodo
            if(ultimo)
              AuxTime = AuxTime.AddDays(1);

            dias += Convert.ToInt32((Hasta - AuxTime).TotalDays);

            //si dias es menor que 0, actualizo el valor para no generar error en el acumulado de reingresos
            if (dias < 0) dias = 0;
        }

        virtual public void Antiguedad(DateTime Hasta, ref int anios, ref int meses, ref int dias)
        {
            NomadEnvironment.GetTrace().Info("****** Antiguedad ******");
            anios = 0; meses = 0; dias = 0;
            bool ultimo = true;

            // Empleado Mensualizado
            if (this.TEMP_PER.Count == 0)
            {
                if (this.f_antiguedad_recNull)
                {
                  int reingresos = 0;
                  foreach (INGRESOS_PER ingreso in this.INGRESOS_PER)
                    {
                       //Determino si es la última trayectoria de ingresos, es decir, el último ingreso
                      reingresos ++;

                       if(this.INGRESOS_PER.Count == reingresos)
                        ultimo = true;
                      else ultimo = false;

                       if (!ingreso.f_egresoNull)
                            Calcular_Tiempo(ingreso.f_ingreso, ingreso.f_egreso, ultimo, ref anios, ref meses, ref dias);
                        else
                            Calcular_Tiempo(ingreso.f_ingreso, Hasta, ultimo, ref anios, ref meses, ref dias);
                     }
                }
                else
                    Calcular_Tiempo(this.f_antiguedad_rec, Hasta, ultimo, ref anios, ref meses, ref dias);
            }
        }

        virtual public int Antiguedad_Meses(DateTime Hasta)
        {
            NomadEnvironment.GetTrace().Info("****** Antiguedad Meses ******");
            int anios = 0;
            int meses = 0;
            int dias = 0;

            Antiguedad(Hasta, ref anios, ref meses, ref dias);
            return anios * 12 + meses + dias / 30;
        }

        public int Antiguedad_Meses()
        {
            NomadEnvironment.GetTrace().Info("****** Antiguedad Meses ******");
            return Antiguedad_Meses(new DateTime(DateTime.Today.Year, 12, 31));
        }

        virtual public int Antiguedad_Anios(DateTime Hasta)
        {
            NomadEnvironment.GetTrace().Info("****** Antiguedad Anios ******");
            int anios = 0;
            int meses = 0;
            int dias = 0;

            Antiguedad(Hasta, ref anios, ref meses, ref dias);
            return anios + (meses + dias / 30) / 12;
        }

        public int Antiguedad_Anios()
        {
            NomadEnvironment.GetTrace().Info("****** Antiguedad Anios ******");
            return Antiguedad_Anios(new DateTime(DateTime.Today.Year, 12, 31));
        }

        public int Dias_Trabajados(DateTime Desde, DateTime Hasta)
        {
            NomadEnvironment.GetTrace().Info("****** Dias Trabajados ******");
            DateTime f_desde = DateTime.Now;
            DateTime f_hasta = DateTime.Now;

            //Validación de fechas para cáculo de días hábiles - Empleado Mensualizado
            if (this.TEMP_PER.Count == 0)
            {
                NomadEnvironment.GetTrace().Info("Periodo a calcular: " + Desde.ToString() + " - " + Hasta.ToString());
                foreach (INGRESOS_PER ingreso in this.INGRESOS_PER)
                {
                    NomadEnvironment.GetTrace().Info("Periodo del Legajo " + this.e_numero_legajo + ": " + ingreso.f_ingreso.ToString() + " - " + ingreso.f_egreso.ToString());
                    if (ingreso.f_ingreso > Hasta)
                    {
                        NomadEnvironment.GetTrace().Info("Periodo ignorado por Fecha de Ingreso mayor a Fecha Hasta: " + ingreso.f_ingreso.ToString() + " > " + Hasta.ToString());
                        continue;
                    }
                    else
                    {
                        if (!ingreso.f_egresoNull)
                        {
                            if (ingreso.f_egreso < Desde)
                            {
                                NomadEnvironment.GetTrace().Info("Periodo ignorado por Fecha de Egreso menor a Fecha Desde: " + ingreso.f_egreso.ToString() + " > " + Desde.ToString());
                                continue;
                            }
                            else
                                f_hasta = ingreso.f_egreso > Hasta ? Hasta : ingreso.f_egreso;
                        }
                        else
                        {
                            f_hasta = Hasta;
                        }
                        f_desde = ingreso.f_ingreso > Desde ? ingreso.f_ingreso : Desde;
                    }
                }
            }
            else
                NomadEnvironment.GetBatch().Trace.Add("WRN", "El empleado es de Temporada, no se puede realizar el calculo. Legajo Ignorado", "Generacion Vacaciones");
            NomadEnvironment.GetTrace().Info("Calculo de dias trabajados con fecha desde: " + f_desde.ToString() + " y fecha hasta: " + f_hasta.ToString());
            //Cálculo de días hábiles
            int dias_habiles = this.Dias_Habiles(f_desde, f_hasta);
            //Cálculo de días de licencia
            int dias_licencia = this.Dias_Licencia(Desde, Hasta);
            //Cálculo de días trabajados
            int dias_trabajados = dias_habiles - dias_licencia;
            NomadEnvironment.GetTrace().Info("Dias Habiles: " + dias_habiles);
            NomadEnvironment.GetTrace().Info("Dias Licencia: " + dias_licencia);
            return dias_trabajados;
        }

        public int Dias_Habiles(DateTime f_desde, DateTime f_hasta)
        {
            NomadEnvironment.GetTrace().Info("****** Dias Habiles ******");
            NucleusRH.Base.Organizacion.Calendario_Feriados.CALENDARIO calendario;
            int dias_habiles = 0;
            int dias_periodo;
            TimeSpan diff;

            ArrayList calendarios = new ArrayList();
            foreach (NucleusRH.Base.Personal.LegajoEmpresa.CALENDARIO_PER cal in this.CALENDARIO_PER)
            {

                calendario = cal.Getoi_calendario();
                bool[] week = new bool[7] { calendario.l_trab_domingos, calendario.l_trab_lunes, calendario.l_trab_martes, calendario.l_trab_miercoles, calendario.l_trab_jueves, calendario.l_trab_viernes, calendario.l_trab_sabados };
                DateTime f_ini_per;
                DateTime f_fin_per;

                NomadEnvironment.GetTrace().Info("cal.fesde: " + cal.f_desde.ToString());
                NomadEnvironment.GetTrace().Info("f_desde: " + f_desde.ToString());
                NomadEnvironment.GetTrace().Info("cal.hasta: " + cal.f_hasta.ToString());
                NomadEnvironment.GetTrace().Info("f_hasta: " + f_hasta.ToString());

                if (f_hasta > cal.f_desde && (f_desde < cal.f_hasta || cal.f_hastaNull))
                {
                    if (f_desde < cal.f_desde)
                    {
                        f_ini_per = cal.f_desde;
                    }
                    else
                    {
                        f_ini_per = f_desde;
                    }
                    if (f_hasta < cal.f_hasta || cal.f_hastaNull)
                    {
                        f_fin_per = f_hasta;
                    }
                    else
                    {
                        f_fin_per = cal.f_hasta;
                    }
                    NomadEnvironment.GetTrace().Info("f_ini_per: " + f_ini_per.ToString());
                    NomadEnvironment.GetTrace().Info("f_fin_per: " + dias_habiles.ToString());
                    diff = f_fin_per - f_ini_per;
                    dias_periodo = (int)diff.TotalDays + 1;

                    DateTime fecha = f_ini_per;

                    for (int i = 0; i < 7; i++)
                    {
                        if (week[(int)fecha.DayOfWeek])
                        {
                            dias_habiles += (int)(dias_periodo / 7);
                            if (i < (dias_periodo % 7))
                            {
                                dias_habiles++;
                            }
                        }
                        fecha = fecha.AddDays(1);
                    }
                }
                NomadEnvironment.GetTrace().Info("Calendario: " + cal.Code + " - Dias habiles: " + dias_habiles);
            }
            NomadEnvironment.GetTrace().Info("Dias Habiles Total: " + dias_habiles);
            return dias_habiles;
        }

        public int Dias_Licencia(DateTime f_desde, DateTime f_hasta)
        {
            NomadEnvironment.GetTrace().Info("****** Dias Licencia ******");
            NucleusRH.Base.Organizacion.Calendario_Feriados.CALENDARIO calendario;
            int dias_licencia = 0;
            int dias_periodo;
            TimeSpan diff;

            ArrayList calendarios = new ArrayList();
            foreach (NucleusRH.Base.Personal.LegajoEmpresa.CALENDARIO_PER cal in this.CALENDARIO_PER)
            {
                calendario = cal.Getoi_calendario();
                bool[] week = new bool[7] { calendario.l_trab_domingos, calendario.l_trab_lunes, calendario.l_trab_martes, calendario.l_trab_miercoles, calendario.l_trab_jueves, calendario.l_trab_viernes, calendario.l_trab_sabados };
                DateTime f_fin_lic;
                DateTime f_ini_lic;
                if (f_hasta > cal.f_desde && (f_desde < cal.f_hasta || cal.f_hastaNull))
                {
                    NucleusRH.Base.Organizacion.Convenios.CONVENIO convenio = NucleusRH.Base.Organizacion.Convenios.CONVENIO.Get(this.Getoi_categoria_ult().oi_convenio);
                    bool l_paga = false;
                    // Dias de licencias que generan ausentismo
                    foreach (NucleusRH.Base.Personal.LegajoEmpresa.LICENCIA_PER lic in this.LICEN_PER)
                    {
                        if (f_hasta > lic.f_inicio && (f_desde < lic.f_fin || lic.f_finNull))
                        {
                            NucleusRH.Base.Personal.Licencias.LICENCIA licencia = lic.Getoi_licencia();
                            NomadEnvironment.GetTrace().Info("Licencia: " + licencia.d_licencia + " - Fecha Inicio: " + lic.f_inicio + " - Fecha Fin: " + lic.f_fin);
                            NucleusRH.Base.Personal.Licencias.LICENCIAS_CONV licencia_conv = (NucleusRH.Base.Personal.Licencias.LICENCIAS_CONV)licencia.LIC_CONV.GetByAttribute("oi_convenio", convenio.Id);
                            if (licencia_conv == null)
                                l_paga = licencia.l_paga;
                            else
                                l_paga = licencia_conv.l_paga;

                            NomadEnvironment.GetTrace().Info("Licencia Paga: " + l_paga);
                            if (l_paga == true)
                                continue;

                            if (f_desde < lic.f_inicio)
                                f_ini_lic = lic.f_inicio;
                            else
                                f_ini_lic = f_desde;

                            if (f_hasta < lic.f_fin || lic.f_finNull)
                                f_fin_lic = f_hasta;
                            else
                                f_fin_lic = lic.f_fin;

                            diff = f_fin_lic - f_ini_lic;
                            dias_periodo = (int)diff.TotalDays + 1;
                            DateTime fecha = f_ini_lic;

                            for (int i = 0; i < 7; i++)
                            {
                                if (week[(int)fecha.DayOfWeek])
                                {
                                    dias_licencia += (int)(dias_periodo / 7);
                                    if (i < (dias_periodo % 7))
                                        dias_licencia++;
                                }
                                fecha = fecha.AddDays(1);
                            }

                        }
                    }
                }
            }
            return dias_licencia;
        }

        public static void SaveUniforme(Nomad.NSystem.Proxy.NomadXML param)
        {
            NomadLog.Info("****** Salvando Uniforme ******");
            NomadLog.Info("param: " + param.ToString());

            //CARGA EL LEGAJO
            NomadLog.Info("OI_PERSONAL_EMP: " + param.FirstChild().GetAttr("oi_personal_emp"));

            NucleusRH.Base.Personal.LegajoEmpresa.UNIFORME_PER objPER;
            objPER = NucleusRH.Base.Personal.LegajoEmpresa.UNIFORME_PER.Get(param.FirstChild().GetAttr("oi_uniforme_per"), false);

            objPER.oi_uniforme = param.FirstChild().GetAttr("oi_uniforme");
            objPER.f_hasta = param.FirstChild().GetAttrDateTime("f_hasta");
            //objPER.oi_mot_devolucion = param.FirstChild().GetAttr("oi_mot_devolucion");
            //objPER.l_repone_stock = param.FirstChild().GetAttrBool("l_repone_stock");

            NomadEnvironment.GetCurrentTransaction().Save(objPER);
            NomadLog.Info("--objPER--" + objPER.SerializeAll());
        }

        public static void EntregaUniforme(Nomad.NSystem.Proxy.NomadXML param)
        {
            //CARGA EL LEGAJO
            NucleusRH.Base.Personal.LegajoEmpresa.PERSONAL_EMP ddoPER;
            ddoPER = NucleusRH.Base.Personal.LegajoEmpresa.PERSONAL_EMP.Get(param.FirstChild().GetAttr("oi_personal_emp"), false);

            NucleusRH.Base.Personal.LegajoEmpresa.UNIFORME_PER ddoUNIF;
            ddoUNIF = new NucleusRH.Base.Personal.LegajoEmpresa.UNIFORME_PER();

            if (param.FirstChild().GetAttr("oi_uniforme") != "")
                ddoUNIF.oi_uniforme = param.FirstChild().GetAttr("oi_uniforme");

            if (param.FirstChild().GetAttr("f_desde") != "")
                ddoUNIF.f_desde = param.FirstChild().GetAttrDateTime("f_desde");

            if (param.FirstChild().GetAttr("f_hasta") != "")
                ddoUNIF.f_hasta = param.FirstChild().GetAttrDateTime("f_hasta");

            if (param.FirstChild().GetAttr("oi_mot_entrega") != "")
                ddoUNIF.oi_mot_entrega = param.FirstChild().GetAttr("oi_mot_entrega");

            if (param.FirstChild().GetAttr("e_cantidad") != "")
      {
                ddoUNIF.e_cantidad = param.FirstChild().GetAttrInt("e_cantidad");
        ddoUNIF.e_pendiente = param.FirstChild().GetAttrInt("e_cantidad");
      }

            ddoPER.UNIF_PER.Add(ddoUNIF);

            NomadEnvironment.GetCurrentTransaction().Save(ddoPER);
        }

        public static void DevolucionUniforme(string oi_personal_emp, Nomad.NSystem.Proxy.NomadXML param)
        {
            NomadBatch objBatch;
            objBatch = NomadBatch.GetBatch("Devolución de uniformes", "Devolución de uniformes");

            //GUARDO EL XML QUE ENTRA EN UN NOMADXML
            NomadXML xmldoc = new NomadXML(param.ToString());
            xmldoc = xmldoc.FirstChild();

            NomadLog.Info("xmldoc--" + xmldoc.FirstChild());
            //CARGA EL LEGAJO
            NomadEnvironment.GetTrace().Info("oi_personal_emp -- " + oi_personal_emp);

            //INICIO UNA TRANSACCION
            NomadTransaction objTran = null;
            try
            {
                objTran = new NomadTransaction();
                objTran.Begin();

                for (NomadXML xmlCur = xmldoc.FirstChild(); xmlCur != null; xmlCur = xmlCur.Next())
                {
                    NucleusRH.Base.Personal.LegajoEmpresa.UNIFORME_PER ddoUNIF;
                    ddoUNIF = NucleusRH.Base.Personal.LegajoEmpresa.UNIFORME_PER.Get(xmlCur.GetAttr("id"), true);

                    NomadEnvironment.GetTrace().Info("xmlCur -- " + xmlCur.ToString());
                    if (xmlCur.GetAttrInt("e_devuelve") > 0)
                    {
                        NucleusRH.Base.Personal.LegajoEmpresa.DEV_UNIF ddoDEV = new NucleusRH.Base.Personal.LegajoEmpresa.DEV_UNIF();
                        ddoDEV.f_dev_unif = DateTime.Now;
                        ddoDEV.e_cantidad = xmlCur.GetAttrInt("e_devuelve");
                        ddoDEV.oi_mot_devolucion = xmlCur.GetAttr("oi_mot_devolucion");
                        ddoDEV.l_repone_stock = xmlCur.GetAttrBool("l_repone_stock");
                        ddoUNIF.DEV_UNIF.Add(ddoDEV);
                    }
                    ddoUNIF.e_pendiente -= xmlCur.GetAttrInt("e_devuelve");
                    if (ddoUNIF.e_pendiente <= 0)
                        ddoUNIF.f_hasta = DateTime.Now;
                    NomadEnvironment.GetCurrentTransaction().Save(ddoUNIF);
                }
                objTran.Commit();
                objBatch.Log("Devolución exitosa");

            }
            catch(Exception e)
            {
                if (objTran != null)
                    objTran.Rollback();
                objBatch.Err("Error en la devolución de uniformes: " + e.Message);
                throw;
            }
        }

        public static SortedList<string, object> GetLegajo(string PAR)
        {
            NomadLog.Debug("--------------------------------------");
            NomadLog.Debug("------------GET LEGAJO--------------");
            NomadLog.Debug("--------------------------------------");

            NomadLog.Debug("GetLeg.Usuario: " + PAR);

            SortedList<string, object> retorno = new SortedList<string, object>();
            SortedList<string, string> types = new SortedList<string, string>();
            string type = "";

            NomadXML param = new NomadXML("PARAM");

            //Agrego los parametros
            param.SetAttr("oi_usuario_sistema", PAR);

            //Ejecuto el recurso
            NomadXML resultado = NomadProxy.GetProxy().SQLService().GetXML("CLASS.NucleusRH.Base.Personal.LegajoEmpresa.PERSONAL_EMP.DATOS_LEG", param.ToString());
            NomadLog.Debug((resultado.FirstChild() == null ? "No se encontró el legajo con el usuario: " + PAR + "." : "Legajo encontrado."));

            if (resultado.FirstChild() != null)
            {
                //Armo una sorted list con los atributos del resultado y sus tipos
                for (int x = 0; x < resultado.Attrs.Count; x++)
                {
                    types.Add(resultado.Attrs[x].ToString(), resultado.GetAttr(resultado.Attrs[x].ToString()));
                }

                resultado = resultado.FirstChild();

                for (int r = 0; r < resultado.Attrs.Count; r++)
                {
                    //Busco de que tipo es el atributo
                    foreach (KeyValuePair<string, string> kvp in types)
                    {
                        if (kvp.Key == resultado.Attrs[r].ToString())
                        {
                            type = kvp.Value;
                            break;
                        }
                    }

                    //Agrego el atributo en base a su tipo
                    switch (type)
                    {
                        case "string":
                            retorno.Add(resultado.Attrs[r].ToString(), resultado.GetAttr(resultado.Attrs[r].ToString()));
                            break;
                        case "int":
                            retorno.Add(resultado.Attrs[r].ToString(), resultado.GetAttrInt(resultado.Attrs[r].ToString()));
                            break;
                        case "datetime":
                            retorno.Add(resultado.Attrs[r].ToString(), resultado.GetAttrDateTime(resultado.Attrs[r].ToString()));
                            break;
                        case "double":
                            retorno.Add(resultado.Attrs[r].ToString(), resultado.GetAttrDouble(resultado.Attrs[r].ToString()));
                            break;
                        case "bool":
                            retorno.Add(resultado.Attrs[r].ToString(), resultado.GetAttrBool(resultado.Attrs[r].ToString()));
                            break;
                        default:
                            retorno.Add(resultado.Attrs[r].ToString(), resultado.GetAttr(resultado.Attrs[r].ToString()));
                            break;
                    }

                    type = "";
                }
            }
            NomadLog.Debug("Retorno: " + retorno.ToString());
            return retorno;
        }

        public static List<SortedList<string, object>> GetLicencias(int PAR, int dias_prev, int dias_post)
        {
            NomadLog.Debug("--------------------------------------");
            NomadLog.Debug("-----------GET LICENCIAS-------------");
            NomadLog.Debug("--------------------------------------");

            NomadLog.Debug("GetLic.PersonalEMP: " + PAR);
            NomadLog.Debug("GetLic.dias_prev: " + dias_prev);
            NomadLog.Debug("GetLic.dias_post: " + dias_post);

            List<SortedList<string, object>> retorno = new List<SortedList<string, object>>();
            SortedList<string, object> row;
            SortedList<string, string> types = new SortedList<string, string>();
            string type = "";

            int linea;
            NomadXML MyROW;

            NomadXML param = new NomadXML("PARAM");

            //Agrego los parametros
            param.SetAttr("oi_personal_emp", PAR);
            param.SetAttr("dias_prev", dias_prev);
            param.SetAttr("dias_post", dias_post);

            //Ejecuto el recurso
            NomadXML resultado = NomadProxy.GetProxy().SQLService().GetXML("CLASS.NucleusRH.Base.Personal.LegajoEmpresa.PERSONAL_EMP.GET_LIC", param.ToString());
            NomadLog.Debug((resultado.FirstChild() == null ? "No se encontraron licencias para el legajo con ID: " + PAR + "." : "Licencias encontradas " + resultado.ChildLength + "."));

            if (resultado.FirstChild() != null)
            {
                //Armo una sorted list con los atributos del resultado y sus tipos
                for (int x = 0; x < resultado.Attrs.Count; x++)
                {
                    types.Add(resultado.Attrs[x].ToString(), resultado.GetAttr(resultado.Attrs[x].ToString()));
                }

                //resultado = resultado.FirstChild();
                for (linea = 1, MyROW = resultado.FirstChild(); MyROW != null; linea++, MyROW = MyROW.Next())
                {
                    row = new SortedList<string, object>();

                    for (int r = 0; r < MyROW.Attrs.Count; r++)
                    {
                        //Busco de que tipo es el atributo
                        foreach (KeyValuePair<string, string> kvp in types)
                        {
                            if (kvp.Key == MyROW.Attrs[r].ToString())
                            {
                                type = kvp.Value;
                                break;
                            }
                        }

                        //Agrego el atributo en base a su tipo
                        switch (type)
                        {
                            case "string":
                                row.Add(MyROW.Attrs[r].ToString(), MyROW.GetAttr(MyROW.Attrs[r].ToString()));
                                break;
                            case "int":
                                row.Add(MyROW.Attrs[r].ToString(), MyROW.GetAttrInt(MyROW.Attrs[r].ToString()));
                                break;
                            case "datetime":
                                row.Add(MyROW.Attrs[r].ToString(), MyROW.GetAttrDateTime(MyROW.Attrs[r].ToString()));
                                break;
                            case "double":
                                row.Add(MyROW.Attrs[r].ToString(), MyROW.GetAttrDouble(MyROW.Attrs[r].ToString()));
                                break;
                            case "bool":
                                row.Add(MyROW.Attrs[r].ToString(), MyROW.GetAttrBool(MyROW.Attrs[r].ToString()));
                                break;
                            default:
                                row.Add(MyROW.Attrs[r].ToString(), MyROW.GetAttr(MyROW.Attrs[r].ToString()));
                                break;
                        }
                        type = "";
                    }

                    //Agrego la licencia a la lista de resultados
                    retorno.Add(row);
                }
            }
            NomadLog.Debug("Retorno: " + retorno.ToString());
            return retorno;
        }

        public static string ValidaLicencia(int LEG, int LIC, string f_inicio, string f_fin)
        {
            NomadLog.Debug("--------------------------------------");
            NomadLog.Debug("-----------VALIDA LICENCIA-------------");
            NomadLog.Debug("--------------------------------------");

            NomadLog.Debug("ValidaLicencia.oi_personal_emp: " + LEG);
            NomadLog.Debug("ValidaLicencia.oi_licencia: " + LIC);
            NomadLog.Debug("ValidaLicencia.f_inicio: " + f_inicio);
            NomadLog.Debug("ValidaLicencia.f_fin: " + f_fin);

            NomadXML param = new NomadXML("PARAM");

            //Agrego los parametros
            param.SetAttr("oi_personal_emp", LEG.ToString());
            param.SetAttr("oi_licencia", LIC.ToString());
            param.SetAttr("f_inicio", f_inicio.ToString());
            param.SetAttr("f_fin", f_fin.ToString());

            //Ejecuto el recurso
            NomadXML resultado = NomadProxy.GetProxy().SQLService().GetXML("CLASS.NucleusRH.Base.Personal.LegajoEmpresa.PERSONAL_EMP.VAL_LIC", param.ToString());
            NomadLog.Debug("RESULTADO: " + resultado.ToString());
            return resultado.GetAttr("RETURN");

        }

        public static string AddLicencia(int LEG, SortedList<string, object> DATA1)
        {
            NomadLog.Debug("--------------------------------------");
            NomadLog.Debug("-----------AGREGAR LICENCIA-------------");
            NomadLog.Debug("--------------------------------------");

            NomadLog.Debug("AddLicencia.oi_personal_emp: " + LEG);
            //Get LEGAJO
            NucleusRH.Base.Personal.LegajoEmpresa.LICENCIA_PER LIC_PER;
            NucleusRH.Base.Personal.LegajoEmpresa.PERSONAL_EMP LEGAJO;
            LIC_PER = new NucleusRH.Base.Personal.LegajoEmpresa.LICENCIA_PER();

            LEGAJO = NucleusRH.Base.Personal.LegajoEmpresa.PERSONAL_EMP.Get(LEG);
            if (LEGAJO == null) return "0";

            foreach (KeyValuePair<string, object> kvp in DATA1)
            {
                switch (kvp.Key.ToUpper())
                {
                    case "OI_LICENCIA":
                        LIC_PER.oi_licencia = (string)kvp.Value;
                        break;
                    case "F_INICIO":
                        LIC_PER.f_inicio = (DateTime)kvp.Value;
                        break;
                    case "F_FIN":
                        if (kvp.Value != null)
                            LIC_PER.f_fin = (DateTime)kvp.Value;
                        break;
                    case "ANIO":
                        LIC_PER.e_anio_corresp = Convert.ToInt32(kvp.Value);
                        break;
                    case "CANT_DIAS":
                        LIC_PER.e_cant_dias = Convert.ToInt32(kvp.Value);
                        break;
                    case "OBSERVACIONES":
                        if (kvp.Value != null)
                            LIC_PER.o_licencia_per = (string)kvp.Value;
                        break;
                }
            }

            LEGAJO.LICEN_PER.Add(LIC_PER);

            try
            {
                NomadEnvironment.GetCurrentTransaction().Save(LEGAJO);
                return "1";
            }
            catch (Exception ex)
            {
                NomadLog.Debug("Error agregando LICENCIA: " + ex);
                return "0";
            }

        }

        public static string ValidarYGuardarLicencia(int LEG, SortedList<string, object> DATA1)
        {
            NomadLog.Debug("--------------------------------------");
            NomadLog.Debug("-----------VALIDAR Y GUARDAR LICENCIA-------------");
            NomadLog.Debug("--------------------------------------");

            NomadLog.Debug("ValidarYGuardarLicencia.oi_personal_emp: " + LEG);

            //Get LEGAJO
            NucleusRH.Base.Personal.LegajoEmpresa.LICENCIA_PER LIC_PER;
            NucleusRH.Base.Personal.LegajoEmpresa.PERSONAL_EMP LEGAJO;
            LIC_PER = new NucleusRH.Base.Personal.LegajoEmpresa.LICENCIA_PER();

            LEGAJO = NucleusRH.Base.Personal.LegajoEmpresa.PERSONAL_EMP.Get(LEG);
            if (LEGAJO == null) return "0";

            foreach (KeyValuePair<string, object> kvp in DATA1)
            {
                switch (kvp.Key.ToUpper())
                {
                    case "OI_LICENCIA":
                        LIC_PER.oi_licencia = (string)kvp.Value;
                        break;
                    case "F_INICIO":
                        LIC_PER.f_inicio = (DateTime)kvp.Value;
                        break;
                    case "F_FIN":
                        if (kvp.Value != null)
                            LIC_PER.f_fin = (DateTime)kvp.Value;
                        break;
                    case "ANIO":
                        LIC_PER.e_anio_corresp = Convert.ToInt32(kvp.Value);
                        break;
                    case "CANT_DIAS":
                        LIC_PER.e_cant_dias = Convert.ToInt32(kvp.Value);
                        break;
                    case "OBSERVACIONES":
                        if (kvp.Value != null)
                            LIC_PER.o_licencia_per = (string)kvp.Value;
                        break;
                }
            }

            //Validar licencia previo al registro de la misma
            NomadXML resultado;
            resultado = ValidarLicencia(LEG, Int32.Parse(LIC_PER.oi_licencia), LIC_PER.f_inicio, LIC_PER.f_fin, LIC_PER.e_cant_dias, LIC_PER.e_anio_corresp);

            if (resultado.GetAttr("valid") == "1")
            {
                LEGAJO.LICEN_PER.Add(LIC_PER);
                try
                {
                    NomadEnvironment.GetCurrentTransaction().Save(LEGAJO);

                    LEGAJO = NucleusRH.Base.Personal.LegajoEmpresa.PERSONAL_EMP.Get(LEG, true);
                    string id = LEGAJO.LICEN_PER[LEGAJO.LICEN_PER.Count - 1].Id;
                    GuardarLicenciaPorRol(id);
                    return "1";
                }
                catch (Exception ex)
                {
                    NomadLog.Debug("Error al registrar la LICENCIA: " + ex);
                    return "La licencia pasó las validaciones pero hubo un error al guardar la misma";
                }
            }
            else
            {
                if (resultado.GetAttr("alert") != "")
                {
                    return resultado.GetAttr("alert");
                }
                else //Tiene un confirm pero se debe registrar la licencia igualmente
                {
                    LEGAJO.LICEN_PER.Add(LIC_PER);
                    try
                    {
                        NomadEnvironment.GetCurrentTransaction().Save(LEGAJO);

                        LEGAJO = NucleusRH.Base.Personal.LegajoEmpresa.PERSONAL_EMP.Get(LEG, true);
                        string id = LEGAJO.LICEN_PER[LEGAJO.LICEN_PER.Count - 1].Id;
                        GuardarLicenciaPorRol(id);
                        return "1";
                    }
                    catch (Exception ex)
                    {
                        NomadLog.Debug("Error al registrar la LICENCIA: " + ex);
                        return "La licencia pasó las validaciones pero hubo un error al guardar la misma";
                    }
                }
            }
        }

        public static string DelLicencia(string PAR)
        {
            NomadLog.Debug("--------------------------------------");
            NomadLog.Debug("-----------DELETE LICENCIA-------------");
            NomadLog.Debug("--------------------------------------");

            NomadLog.Debug("DelLicencia.oi_licencia_per: " + PAR);

            //Get PERSONAL
            NucleusRH.Base.Personal.LegajoEmpresa.LICENCIA_PER LIC;
            LIC = NucleusRH.Base.Personal.LegajoEmpresa.LICENCIA_PER.Get(PAR, true);

            if (LIC == null) return "0";

            try
            {
                NomadEnvironment.GetCurrentTransaction().Delete(LIC);
                return "1";
            }
            catch (Exception ex)
            {
                NomadLog.Debug("Error eliminando Licencia: " + ex);
                return "0";
            }
        }

        public static string GetLeg(string PAR)
        {
            NomadLog.Debug("--------------------------------------");
            NomadLog.Debug("------------GET LEGAJO--------------");
            NomadLog.Debug("--------------------------------------");

            NomadLog.Debug("GetLeg.Usuario: " + PAR);

            string retorno = "0";

            NomadXML param = new NomadXML("PARAM");

            //Agrego los parametros
            param.SetAttr("oi_usuario_sistema", PAR);

            //Ejecuto el recurso
            NomadXML resultado = NomadProxy.GetProxy().SQLService().GetXML("CLASS.NucleusRH.Base.Personal.LegajoEmpresa.PERSONAL_EMP.GET_LEG", param.ToString());
            NomadLog.Debug((resultado.FirstChild() == null ? "No se encontró el Legajo con el usuario: " + PAR + "." : "Legajo encontrado."));

            if (resultado.FirstChild() != null)
            {
                if (resultado.ChildLength > 1)
                {
                    retorno = resultado.ChildLength.ToString();
                }
                else retorno = "1";

            }

            NomadLog.Debug("Retorno: " + retorno.ToString());
            return retorno;
        }

    /// <summary>
    /// Retorna el último Centro de Costo según la fecha de ingreso. Si no tiene retorna null.
    /// </summary>
    /// <returns></returns>
        public NucleusRH.Base.Personal.LegajoEmpresa.CCOSTO_PER GetUltimoCCosto()
        {
            NucleusRH.Base.Personal.LegajoEmpresa.CCOSTO_PER objResult = null;

            //Verifica que haya un centro de costo anterior
            if (this.CCOSTO_PER.Count > 0)
            {
                foreach (CCOSTO_PER objCC in this.CCOSTO_PER)
                {
                  //La primera vez toma el primero. Luego solo si el de la colección tiene fecha de ingreso mayor al tomado.
                  if (objResult == null || objCC.f_ingreso > objResult.f_ingreso)
                    objResult = objCC;
                }
                NomadLog.Debug("GetUltimoCCosto: Legajo " + this.e_numero_legajo.ToString() + " y fecha " + objResult.f_ingreso.ToString("dd/MM/yyyy"));
            }
            return objResult;
        }

        public  NucleusRH.Base.Personal.LegajoEmpresa.CATEGORIA_PER GetUltimaCategoria()
        {
            NucleusRH.Base.Personal.LegajoEmpresa.CATEGORIA_PER objResult = null;

            //Verifica que haya una categoría anterior
            if (this.CATEG_PER.Count > 0)
            {
                foreach (CATEGORIA_PER objCat in this.CATEG_PER)
                {
                    //La primera vez toma el primero. Luego solo si el de la colección tiene fecha de ingreso mayor al tomado.
                    if (objResult == null || objCat.f_ingreso > objResult.f_ingreso)
                        objResult = objCat;
                }
                NomadLog.Debug("GetUltimaCategoria: Legajo " + this.e_numero_legajo.ToString() + " y fecha " + objResult.f_ingreso.ToString("dd/MM/yyyy"));
            }
            return objResult;
        }

        public  NucleusRH.Base.Personal.LegajoEmpresa.POSICION_PER GetUltimaPosicion()
        {
            NucleusRH.Base.Personal.LegajoEmpresa.POSICION_PER objResult = null;

            //Verifica que haya una posición anterior
            if (this.POSIC_PER.Count > 0)
            {
                foreach (POSICION_PER objPosic in this.POSIC_PER)
                {
                    //La primera vez toma el primero. Luego solo si el de la colección tiene fecha de ingreso mayor al tomado.
                    if (objResult == null || objPosic.f_ingreso > objResult.f_ingreso)
                        objResult = objPosic;
                }
                NomadLog.Debug("GetUltimaPosicion: Legajo " + this.e_numero_legajo.ToString() + " y fecha " + objResult.f_ingreso.ToString("dd/MM/yyyy"));
            }
            return objResult;
        }

        public  NucleusRH.Base.Personal.LegajoEmpresa.PUESTO_PER GetUltimoPuesto()
        {
            NucleusRH.Base.Personal.LegajoEmpresa.PUESTO_PER objResult = null;

            //Verifica que haya un puesto anterior
            if (this.PUESTO_PER.Count > 0)
            {
                foreach (PUESTO_PER objPue in this.PUESTO_PER)
                {
                    //La primera vez toma el primero. Luego solo si el de la colección tiene fecha de ingreso mayor al tomado.
                    if (objResult == null || objPue.f_ingreso > objResult.f_ingreso)
                        objResult = objPue;
                }
                NomadLog.Debug("GetUltimoPuesto: Legajo " + this.e_numero_legajo.ToString() + " y fecha " + objResult.f_ingreso.ToString("dd/MM/yyyy"));
            }
            return objResult;
        }

        public  NucleusRH.Base.Personal.LegajoEmpresa.REMUN_PER GetUltimaRemuneracion()
        {
            NucleusRH.Base.Personal.LegajoEmpresa.REMUN_PER objResult = null;

            //Verifica que haya una remuneración anterior
            if (this.REMUN_PER.Count > 0)
            {
                foreach (REMUN_PER objRemun in this.REMUN_PER)
                {
                    //La primera vez toma el primero. Luego solo si el de la colección tiene fecha de ingreso mayor al tomado.
                    if (objResult == null || objRemun.f_desde > objResult.f_desde)
                        objResult = objRemun;
                }
                NomadLog.Debug("GetUltimaRemuneracion: Legajo " + this.e_numero_legajo.ToString() + " y fecha " + objResult.f_desde.ToString("dd/MM/yyyy"));
            }
            return objResult;
        }

        public  NucleusRH.Base.Personal.LegajoEmpresa.TIPOP_PER GetUltimoTipoPersonal()
        {
            NucleusRH.Base.Personal.LegajoEmpresa.TIPOP_PER objResult = null;

            //Verifica que haya un tipo personal anterior
            if (this.TIPOSP_PER.Count > 0)
            {
                foreach (TIPOP_PER objTP in this.TIPOSP_PER)
                {
                    //La primera vez toma el primero. Luego solo si el de la colección tiene fecha de ingreso mayor al tomado.
                    if (objResult == null || objTP.f_ingreso > objResult.f_ingreso)
                        objResult = objTP;
                }
                NomadLog.Debug("GetUltimoTipoPersonal: Legajo " + this.e_numero_legajo.ToString() + " y fecha " + objResult.f_ingreso.ToString("dd/MM/yyyy"));
            }
            return objResult;
        }

        public NucleusRH.Base.Personal.LegajoEmpresa.EQUIPO_PER GetUltimoEquipoMovil()
        {
            NucleusRH.Base.Personal.LegajoEmpresa.EQUIPO_PER objResult = null;

            //Verifica que haya un equipo anterior
            if (this.EQUIPO_PER.Count > 0)
            {
                foreach (EQUIPO_PER objEqu in this.EQUIPO_PER)
                {
                    //La primera vez toma el primero. Luego solo si el de la colección tiene fecha de ingreso mayor al tomado.
                    if (objResult == null || objEqu.f_ingreso > objResult.f_ingreso)
                        objResult = objEqu;
                }
                NomadLog.Debug("GetUltimoEquipoMovil: Legajo " + this.e_numero_legajo.ToString() + " y fecha " + objResult.f_ingreso.ToString("dd/MM/yyyy"));
            }
            return objResult;
        }

        public NucleusRH.Base.Personal.LegajoEmpresa.CALENDARIO_PER GetUltimoCalendario()
        {
            NucleusRH.Base.Personal.LegajoEmpresa.CALENDARIO_PER objResult = null;

            //Verifica que haya un calendario anterior
            if (this.CALENDARIO_PER.Count > 0)
            {
                foreach (CALENDARIO_PER objCal in this.CALENDARIO_PER)
                {
                    //La primera vez toma el primero. Luego solo si el de la colección tiene fecha de ingreso mayor al tomado.
                    if (objResult == null || objCal.f_desde> objResult.f_desde)
                        objResult = objCal;
                }
                NomadLog.Debug("GetUltimoCalendario: Legajo " + this.e_numero_legajo.ToString() + " y fecha " + objResult.f_desde.ToString("dd/MM/yyyy"));
            }
            return objResult;
        }

        public NucleusRH.Base.Personal.LegajoEmpresa.UBICACION_PER GetUltimaUbicacion()
        {
            NucleusRH.Base.Personal.LegajoEmpresa.UBICACION_PER objResult = null;

            //Verifica que haya una ubicacion anterior
            if (this.UBIC_PER.Count > 0)
            {
                foreach (UBICACION_PER objUbic in this.UBIC_PER)
                {
                    //La primera vez toma el primero. Luego solo si el de la colección tiene fecha de ingreso mayor al tomado.
                    if (objResult == null || objUbic.f_desde > objResult.f_desde)
                        objResult = objUbic;
                }
                NomadLog.Debug("GetUltimaUbicacion: Legajo " + this.e_numero_legajo.ToString() + " y fecha " + objResult.f_desde.ToString("dd/MM/yyyy"));
            }
            return objResult;
        }

        public NucleusRH.Base.Personal.LegajoEmpresa.INGRESOS_PER GetUltimoIngreso()
        {
            NucleusRH.Base.Personal.LegajoEmpresa.INGRESOS_PER objResult = null;

            //Verifica que haya una posición anterior
            if (this.INGRESOS_PER.Count > 0)
            {
                foreach (INGRESOS_PER objPosic in this.INGRESOS_PER)
                {
                    //La primera vez toma el primero. Luego solo si el de la colección tiene fecha de ingreso mayor al tomado.
                    if (objResult == null || objPosic.f_ingreso > objResult.f_ingreso)
                        objResult = objPosic;
                }
                NomadLog.Debug("GetUltimaPosicion: Legajo " + this.e_numero_legajo.ToString() + " y fecha " + objResult.f_ingreso.ToString("dd/MM/yyyy"));
            }
            return objResult;
        }

        public static System.Collections.Generic.SortedList<string, object> Get_LicenciaPer(int PAR)
        {
            NomadLog.Debug("------------------------------------------------------");
            NomadLog.Debug("------------GET DATOS LICENCIA DE LEGAJO--------------");
            NomadLog.Debug("------------------------------------------------------");

            NomadLog.Debug("Get_LicenciaPer.oi_licencia_per: " + PAR);

            SortedList<string, object> retorno = new SortedList<string, object>();
            SortedList<string, string> types = new SortedList<string, string>();
            string type = "";

            NomadXML param = new NomadXML("PARAM");

            //Agrego los parametros
            param.SetAttr("oi_licencia_per", PAR);

            //Ejecuto el recurso
            NomadXML resultado = NomadProxy.GetProxy().SQLService().GetXML("CLASS.NucleusRH.Base.Personal.LegajoEmpresa.LICENCIA_PER.QRY_GetLicenciaPer", param.ToString());
            NomadLog.Debug((resultado.FirstChild() == null ? "No se encontró la licencia con id: " + PAR + "." : "Licencia encontrada."));

            if (resultado.FirstChild() != null)
            {
                //Armo una sorted list con los atributos del resultado y sus tipos
                for (int x = 0; x < resultado.Attrs.Count; x++)
                {
                    types.Add(resultado.Attrs[x].ToString(), resultado.GetAttr(resultado.Attrs[x].ToString()));
                }

                resultado = resultado.FirstChild();

                for (int r = 0; r < resultado.Attrs.Count; r++)
                {
                    //Busco de que tipo es el atributo
                    foreach (KeyValuePair<string, string> kvp in types)
                    {
                        if (kvp.Key == resultado.Attrs[r].ToString())
                        {
                            type = kvp.Value;
                            break;
                        }
                    }

                    //Agrego el atributo en base a su tipo
                    switch (type)
                    {
                        case "string":
                            retorno.Add(resultado.Attrs[r].ToString(), resultado.GetAttr(resultado.Attrs[r].ToString()));
                            break;
                        case "int":
                            retorno.Add(resultado.Attrs[r].ToString(), resultado.GetAttrInt(resultado.Attrs[r].ToString()));
                            break;
                        case "datetime":
                            retorno.Add(resultado.Attrs[r].ToString(), resultado.GetAttrDateTime(resultado.Attrs[r].ToString()));
                            break;
                        case "double":
                            retorno.Add(resultado.Attrs[r].ToString(), resultado.GetAttrDouble(resultado.Attrs[r].ToString()));
                            break;
                        case "bool":
                            retorno.Add(resultado.Attrs[r].ToString(), resultado.GetAttrBool(resultado.Attrs[r].ToString()));
                            break;
                        default:
                            retorno.Add(resultado.Attrs[r].ToString(), resultado.GetAttr(resultado.Attrs[r].ToString()));
                            break;
                    }

                    type = "";
                }
            }
            NomadLog.Debug("Retorno: " + retorno.ToString());
            return retorno;
        }

        public static NomadXML ValidarLicencia(int oi_personal_emp, int oi_licencia, DateTime f_inicio, DateTime f_fin, int e_cant_dias, int e_anio_corresp)
        {
            return ValidarLicencia(oi_personal_emp, oi_licencia, f_inicio,  f_fin,  e_cant_dias,  e_anio_corresp,null);
        }
        public static NomadXML ValidarLicencia(int oi_personal_emp, int oi_licencia, DateTime f_inicio, DateTime f_fin, int e_cant_dias, int e_anio_corresp,Hashtable oisLicIgnorar)
        {
            bool segunConvenio = false;
            bool validMaxDiasLic = true;
            bool validMaxDiasAnio = true;
            bool validMinMesesAntig = true;
            string mensaje = "", msjMaxDiasLic = "", msjMaxDiasAnio = "", msjMinMesesAntig = "";

            NomadXML xmlResult = new NomadXML("RESULT");
            xmlResult.SetAttr("valid", "0");

            //-------------------------------- FECHA ----------------------------------------------//

            if (f_fin < f_inicio)
            {
                mensaje = "La fecha de fin debe ser mayor o igual a la fecha de inicio.";
                xmlResult.SetAttr("alert", mensaje);
                return xmlResult;
            }

            //------------------- SOLAPAMIENTO CON OTRAS LICENCIAS -------------------------------//

      //Recuperar Legajo
            PERSONAL_EMP ddoPerEmp = PERSONAL_EMP.Get(oi_personal_emp,false);

            DateTime f_fin_real;
      foreach (LICENCIA_PER ddoLicPer in ddoPerEmp.LICEN_PER)
      {
          if (oisLicIgnorar != null && oisLicIgnorar.ContainsKey(ddoLicPer.id))
              continue;
        if (ddoLicPer.f_interrupcionNull)
          f_fin_real = ddoLicPer.f_fin;
        else
          f_fin_real = ddoLicPer.f_interrupcion;

        if ((f_inicio >= ddoLicPer.f_inicio && f_inicio <= f_fin_real) ||
          (f_fin    >= ddoLicPer.f_inicio && f_fin    <= f_fin_real) ||
          (f_inicio <= ddoLicPer.f_inicio && f_fin    >= f_fin_real))
        {
          mensaje = "Existe un solapamiento de fechas con otras Licencias cargadas para el Legajo.";
          xmlResult.SetAttr("alert", mensaje);
          return xmlResult;
        }
      }

            //------------------- RESTRICCIONES (MAXIMOS Y MINIMOS) -------------------------------//

            //Recuperar (Tipo de) Licencia
            LICENCIA ddoLicencia = LICENCIA.Get(oi_licencia);

            //Nuevo (Tipo de) Licencia segun Convenio
            LICENCIAS_CONV ddoLicConv = new LICENCIAS_CONV();

            //Si el legajo tiene Categoria
            if (!ddoPerEmp.oi_categoria_ultNull)
            {
                //Obtener OI del Convenio de la Categoria
                string oi_convenio = CATEGORIA.Get(ddoPerEmp.oi_categoria_ult).oi_convenio.ToString();

                //Si existe, recuperar (Tipo de) Licencia segun Convenio
                foreach (LICENCIAS_CONV licConv in ddoLicencia.LIC_CONV)
                {
                    if (licConv.oi_convenio == oi_convenio)
                    {
                        ddoLicConv = licConv;
                        segunConvenio = true;
                        break;
                    }
                }
            }

            //Restricciones
            int e_max_dias_lic = 0;
            int e_max_dias_anio = 0;
            int e_min_meses_antig = 0;
            bool tieneMaxDiasLic = false;
            bool tieneMaxDiasAnio = false;
            bool tieneMinMesesAntig = false;

            //Validar restricciones de (Tipo de) Licencia segun Convenio
            if (segunConvenio)
            {
                if (!ddoLicConv.e_max_dias_licNull)
                {
                    tieneMaxDiasLic = true;
                    e_max_dias_lic = ddoLicConv.e_max_dias_lic;
                }
                if (!ddoLicConv.e_max_dias_anioNull)
                {
                    tieneMaxDiasAnio = true;
                    e_max_dias_anio = ddoLicConv.e_max_dias_anio;
                }
                if (!ddoLicConv.e_min_meses_antigNull)
                {
                    tieneMinMesesAntig = true;
                    e_min_meses_antig = ddoLicConv.e_min_meses_antig;
                }
            }
            //Validar restricciones de (Tipo de) Licencia
            else
            {
                if (!ddoLicencia.e_max_dias_licNull)
                {
                    tieneMaxDiasLic = true;
                    e_max_dias_lic = ddoLicencia.e_max_dias_lic;
                }
                if (!ddoLicencia.e_max_dias_anioNull)
                {
                    tieneMaxDiasAnio = true;
                    e_max_dias_anio = ddoLicencia.e_max_dias_anio;
                }
                if (!ddoLicencia.e_min_meses_antigNull)
                {
                    tieneMinMesesAntig = true;
                    e_min_meses_antig = ddoLicencia.e_min_meses_antig;
                }
            }

            //-----------------------------------------------------------------------------------
            //Existe restriccion de maximo de dias consecutivos
            if (tieneMaxDiasLic)
            {
                if (e_cant_dias > e_max_dias_lic)
                {
                    validMaxDiasLic = false;
                    msjMaxDiasLic = "Cantidad de días consecutivos supera el máximo: ";
                    if (e_max_dias_lic == 1)
                        msjMaxDiasLic += "1 día.\n";
                    else
                        msjMaxDiasLic += e_max_dias_lic + " días.\n";
                }
            }
            //-----------------------------------------------------------------------------------
            //Existe restriccion de maximo de dias en el año
            if (tieneMaxDiasAnio)
            {
        int saldo = e_max_dias_anio;

        foreach (LICENCIA_PER ddoLicPer in ddoPerEmp.LICEN_PER)
        {
          if (ddoLicPer.oi_licencia == oi_licencia.ToString() && ddoLicPer.e_anio_corresp == e_anio_corresp)
            saldo = saldo - ddoLicPer.e_cant_dias;
        }

        if (e_cant_dias > saldo)
        {
          validMaxDiasAnio = false;
                    msjMaxDiasAnio = "Cantidad de días supera el saldo anual: ";
                    if (saldo == 1)
                        msjMaxDiasAnio += "1 día.\n";
                    else if (saldo < 0)
                        msjMaxDiasAnio += "0 días.\n";
                    else
                        msjMaxDiasAnio += saldo + " días.\n";
        }
            }
            //-----------------------------------------------------------------------------------
            //Existe restriccion de minimo de meses de antigüedad
            if (tieneMinMesesAntig)
            {
                int antigLegajo = ddoPerEmp.Antiguedad_Meses(DateTime.Today);

                if (antigLegajo < e_min_meses_antig)
                {
                    validMinMesesAntig = false;
                    msjMinMesesAntig = "Legajo con antigüedad inferior al mínimo: ";
                    if (e_min_meses_antig == 1)
                        msjMinMesesAntig += "1 mes.\n";
                    else
                        msjMinMesesAntig += e_min_meses_antig + " meses.\n";
                }
            }

            //No tiene el minimo de meses de antigüedad y no permite ignorar la restriccion
            if (!validMinMesesAntig && !ddoLicencia.l_min_meses_antig)
            {
                xmlResult.SetAttr("alert", msjMinMesesAntig);
                return xmlResult;
            }

            //Supera el maximo de dias consecutivos y no permite ignorar la restriccion
            if (!validMaxDiasLic && !ddoLicencia.l_max_dias_lic)
            {
                xmlResult.SetAttr("alert", msjMaxDiasLic);
                return xmlResult;
            }

            //Supera el maximo de dias en el año y no permite ignorar la restriccion
            if (!validMaxDiasAnio && !ddoLicencia.l_max_dias_anio)
            {
                xmlResult.SetAttr("alert", msjMaxDiasAnio);
                return xmlResult;
            }

            //No cumple alguna de las restricciones
            if (!validMinMesesAntig || !validMaxDiasLic || !validMaxDiasAnio)
            {
                mensaje = msjMinMesesAntig + msjMaxDiasLic + msjMaxDiasAnio + "\n¿Desea guardar la Licencia de todos modos?";
                xmlResult.SetAttr("confirm", mensaje);
                return xmlResult;
            }
            //Cumple todas las restricciones
            else
            {
                xmlResult.SetAttr("valid", "1");
                return xmlResult;
            }
        }

        public static string ValidarPedidoLicencia(int OI_PERSONAL_EMP, int OI_LICENCIA, DateTime F_INICIO, DateTime F_FIN, int DIAS, int ANIO)
        {
            NomadLog.Debug("-------------------------------------------------");
            NomadLog.Debug("------------VALIDAR PEDIDO LICENCIA--------------");
            NomadLog.Debug("-------------------------------------------------");

            NomadLog.Debug("OI_PERSONAL_EMP:" + OI_PERSONAL_EMP);
            NomadLog.Debug("OI_LICENCIA:" + OI_LICENCIA);
            NomadLog.Debug("F_INICIO:" + F_INICIO);
            NomadLog.Debug("F_FIN:" + F_FIN);
            NomadLog.Debug("DIAS:" + DIAS);
            NomadLog.Debug("ANIO:" + ANIO);

            //retorno de este método
            string retorno = "";

            //resultado del metodo que valida la licencia
            NomadXML resultado;

            //Llamo al método que en realidad valida los datos de la licencia
            resultado = ValidarLicencia(OI_PERSONAL_EMP, OI_LICENCIA, F_INICIO, F_FIN, DIAS, ANIO);

            if (resultado.GetAttr("valid") == "1")
            {
                retorno = "1";
            }
            else
            {
                if (resultado.GetAttr("alert") != "")
                {
                    retorno = resultado.GetAttr("alert");
                }
                else
                {
                    retorno = resultado.GetAttr("confirm");
                }
            }
            return retorno;
        }

        public static System.Collections.Generic.SortedList<string, object> RestriccionesTipoLicenciaYPersonal(int legajo, int tipoLicencia, int anio)
        {
            NomadLog.Debug("-----------------------------------------------------");
            NomadLog.Debug("------------RESTRICCIONES TIPO LICENCIA--------------");
            NomadLog.Debug("-----------------------------------------------------");

            SortedList<string, object> retorno = new SortedList<string, object>();
            string strLicencia;
            int saldo = 0;
            int saldo_prox = 0;
            bool segunConvenio = false;

            LICENCIA licencia = LICENCIA.Get(tipoLicencia, true);

            //Nuevo (Tipo de) Licencia segun Convenio
            LICENCIAS_CONV licenciaConvenio = new LICENCIAS_CONV();

            //Recuperar Legajo
            PERSONAL_EMP personal_emp = PERSONAL_EMP.Get(legajo, false);

            //Si el legajo tiene Categoria
            if (!personal_emp.oi_categoria_ultNull)
            {
                //Obtener OI del Convenio de la Categoria
                string oi_convenio = CATEGORIA.Get(personal_emp.oi_categoria_ult).oi_convenio.ToString();

                //Si existe, recuperar (Tipo de) Licencia segun Convenio
                foreach (LICENCIAS_CONV licConv in licencia.LIC_CONV)
                {
                    if (licConv.oi_convenio == oi_convenio)
                    {
                        licenciaConvenio = licConv;
                        segunConvenio = true;
                        break;
                    }
                }
            }

            //Restricciones
            int max_dias_lic = 0;
            int e_max_dias_anio = 0;
            bool tieneMaxDiasLic = false;
            bool tieneMaxDiasAnio = false;

            //Validar restricciones de (Tipo de) Licencia segun Convenio
            if (segunConvenio)
            {
                if (!licenciaConvenio.e_max_dias_licNull)
                {
                    tieneMaxDiasLic = true;
                    max_dias_lic = licenciaConvenio.e_max_dias_lic;
                }
                if (!licenciaConvenio.e_max_dias_anioNull)
                {
                    tieneMaxDiasAnio = true;
                    e_max_dias_anio = licenciaConvenio.e_max_dias_anio;
                }
            }
            //Validar restricciones de (Tipo de) Licencia
            else
            {
                if (!licencia.e_max_dias_licNull)
                {
                    tieneMaxDiasLic = true;
                    max_dias_lic = licencia.e_max_dias_lic;
                }
                if (!licencia.e_max_dias_anioNull)
                {
                    tieneMaxDiasAnio = true;
                    e_max_dias_anio = licencia.e_max_dias_anio;
                }
            }

            //Calcular saldo si tiene maximo de dias en el año el tipo de licencia a solicitar
            if (tieneMaxDiasAnio)
            {
                //SALDO ANIO ACTUAL
                NomadXML xmlParam = new NomadXML("PARAM");
                xmlParam.SetAttr("oi_personal_emp", legajo);
                xmlParam.SetAttr("oi_licencia", tipoLicencia);
                xmlParam.SetAttr("anio", anio);

                NomadXML xmlSumAnual = NomadEnvironment.QueryNomadXML(Resources.SumAnual, xmlParam.ToString());
                int sumAnual = StringUtil.str2int(xmlSumAnual.FirstChild().GetAttr("sum"));
                saldo = e_max_dias_anio - sumAnual;

                //SALDO ANIO SIGUIENTE
                xmlParam = new NomadXML("PARAM");
                xmlParam.SetAttr("oi_personal_emp", legajo);
                xmlParam.SetAttr("oi_licencia", tipoLicencia);
                xmlParam.SetAttr("anio", anio+1);

                xmlSumAnual = NomadEnvironment.QueryNomadXML(Resources.SumAnual, xmlParam.ToString());
                sumAnual = StringUtil.str2int(xmlSumAnual.FirstChild().GetAttr("sum"));
                saldo_prox = e_max_dias_anio - sumAnual;

            }

            //Seteo la descripcion de la licencia
            strLicencia = licencia.c_licencia + " - " + licencia.d_licencia;
            retorno.Add("LICENCIA", strLicencia);

            //Seteo el saldoS
            retorno.Add("SALDO", saldo);
            retorno.Add("SALDO_PROX", saldo_prox);

            //Seteo la cantidad de dias maximos corridos que puede pedir para el tipo de licencia solicitada.
            retorno.Add("MAX_DIAS_LIC", max_dias_lic);

            return retorno;
        }

        public static System.Collections.Generic.SortedList<string, object> GetLegajoPorOI(string PAR)
        {
            NomadLog.Debug("--------------------------------------");
            NomadLog.Debug("------------GET LEGAJO POR IR---------");
            NomadLog.Debug("--------------------------------------");

            NomadLog.Debug("GetLegajoPorOI.oi_personal_emp: " + PAR);

            SortedList<string, object> retorno = new SortedList<string, object>();
            SortedList<string, string> types = new SortedList<string, string>();
            string type = "";

            NomadXML param = new NomadXML("PARAM");

            //Agrego los parametros
            param.SetAttr("oi_personal_emp", PAR);

            //Ejecuto el recurso
            NomadXML resultado = NomadProxy.GetProxy().SQLService().GetXML("CLASS.NucleusRH.Base.Personal.LegajoEmpresa.PERSONAL_EMP.DATOS_LEG_POR_OI", param.ToString());
            NomadLog.Debug((resultado.FirstChild() == null ? "No se encontró el legajo con el oi_personal_emp: " + PAR + "." : "Legajo encontrado."));

            if (resultado.FirstChild() != null)
            {
                //Armo una sorted list con los atributos del resultado y sus tipos
                for (int x = 0; x < resultado.Attrs.Count; x++)
                {
                    types.Add(resultado.Attrs[x].ToString(), resultado.GetAttr(resultado.Attrs[x].ToString()));
                }

                resultado = resultado.FirstChild();

                for (int r = 0; r < resultado.Attrs.Count; r++)
                {
                    //Busco de que tipo es el atributo
                    foreach (KeyValuePair<string, string> kvp in types)
                    {
                        if (kvp.Key == resultado.Attrs[r].ToString())
                        {
                            type = kvp.Value;
                            break;
                        }
                    }

                    //Agrego el atributo en base a su tipo
                    switch (type)
                    {
                        case "string":
                            retorno.Add(resultado.Attrs[r].ToString(), resultado.GetAttr(resultado.Attrs[r].ToString()));
                            break;
                        case "int":
                            retorno.Add(resultado.Attrs[r].ToString(), resultado.GetAttrInt(resultado.Attrs[r].ToString()));
                            break;
                        case "datetime":
                            retorno.Add(resultado.Attrs[r].ToString(), resultado.GetAttrDateTime(resultado.Attrs[r].ToString()));
                            break;
                        case "double":
                            retorno.Add(resultado.Attrs[r].ToString(), resultado.GetAttrDouble(resultado.Attrs[r].ToString()));
                            break;
                        case "bool":
                            retorno.Add(resultado.Attrs[r].ToString(), resultado.GetAttrBool(resultado.Attrs[r].ToString()));
                            break;
                        default:
                            retorno.Add(resultado.Attrs[r].ToString(), resultado.GetAttr(resultado.Attrs[r].ToString()));
                            break;
                    }

                    type = "";
                }
            }
            NomadLog.Debug("Retorno: " + retorno.ToString());
            return retorno;
        }

        public static System.Collections.Generic.SortedList<string, string> NewValidarPedidoLicencia(int OI_PERSONAL_EMP, int OI_LICENCIA, DateTime F_INICIO, DateTime F_FIN, int DIAS, int ANIO)
        {
            //Este metodo procesa los datos que devuelve el metodo ValidarLicencia() para que puedas ser usados por el Workflow.
            //TODO: ESTE METODO TIENE LA MISMA FUNCIONALIDAD QUE VALIDARPEDIDOLICENCIA() POR LO QUE CUANDO QUEDE IMPLEMENTADO ESTE, EL OTRO DEBERÁ ELIMINARSE PARA NO CREAR CONFUSIONES EN UN FUTURO
            NomadLog.Debug("-------------------------------------------------");
            NomadLog.Debug("------------NEW VALIDAR PEDIDO LICENCIA--------------");
            NomadLog.Debug("-------------------------------------------------");

            NomadLog.Debug("OI_PERSONAL_EMP:" + OI_PERSONAL_EMP);
            NomadLog.Debug("OI_LICENCIA:" + OI_LICENCIA);
            NomadLog.Debug("F_INICIO:" + F_INICIO);
            NomadLog.Debug("F_FIN:" + F_FIN);
            NomadLog.Debug("DIAS:" + DIAS);
            NomadLog.Debug("ANIO:" + ANIO);

            //retorno de este método
            SortedList<string, string> retorno = new SortedList<string, string>();

            //resultado del metodo que valida la licencia
            NomadXML resultado;

            //Llamo al método que en realidad valida los datos de la licencia
            resultado = ValidarLicencia(OI_PERSONAL_EMP, OI_LICENCIA, F_INICIO, F_FIN, DIAS, ANIO);

            if (resultado.GetAttr("valid") == "1")
            {
                retorno = NucleusRH.Base.Configuracion.Herramientas.QUERIES.CreateRTA("OK", "OK");
            }
            else
            {
                if (resultado.GetAttr("alert") != "")
                {
                    retorno = NucleusRH.Base.Configuracion.Herramientas.QUERIES.CreateRTA("ERR", resultado.GetAttr("alert"));

                }
                else
                {
                    retorno = NucleusRH.Base.Configuracion.Herramientas.QUERIES.CreateRTA("WRN", resultado.GetAttr("confirm"));

                }
            }

            return retorno;
        }

        public static System.Collections.Generic.SortedList<string, string> NewValidarYGuardarLicencia(int LEG, System.Collections.Generic.SortedList<string, object> DATA1)
        {
            //TODO: ESTE METODO TIENE LA MISMA FUNCIONALIDAD QUE VALIDARYGUARDAR() POR LO QUE CUANDO QUEDE IMPLEMENTADO ESTE, EL OTRO DEBERÁ ELIMINARSE PARA NO CREAR CONFUSIONES EN UN FUTURO
            NomadLog.Debug("--------------------------------------------------");
            NomadLog.Debug("-----------VALIDAR Y GUARDAR LICENCIA-------------");
            NomadLog.Debug("--------------------------------------------------");

            NomadLog.Debug("ValidarYGuardarLicencia.oi_personal_emp: " + LEG);

            SortedList<string, string> retorno = new SortedList<string, string>();

            //Get LEGAJO
            NucleusRH.Base.Personal.LegajoEmpresa.LICENCIA_PER LIC_PER;
            NucleusRH.Base.Personal.LegajoEmpresa.PERSONAL_EMP LEGAJO;
            LIC_PER = new NucleusRH.Base.Personal.LegajoEmpresa.LICENCIA_PER();

            LEGAJO = NucleusRH.Base.Personal.LegajoEmpresa.PERSONAL_EMP.Get(LEG,true);

            if (LEGAJO == null)
            {
                retorno = NucleusRH.Base.Configuracion.Herramientas.QUERIES.CreateRTA("FATALERR", "El legajo con oi_personal_emp " + LEG.ToString() + " no se encontró.");
                return retorno;
            }

            foreach (KeyValuePair<string, object> kvp in DATA1)
            {
                switch (kvp.Key.ToUpper())
                {
                    case "OI_LICENCIA":
                        LIC_PER.oi_licencia = kvp.Value.ToString();
                        break;
                    case "F_INICIO":
                        LIC_PER.f_inicio = (DateTime)kvp.Value;
                        break;
                    case "F_FIN":
                        if (kvp.Value != null)
                            LIC_PER.f_fin = (DateTime)kvp.Value;
                        break;
                    case "ANIO":
                        LIC_PER.e_anio_corresp = Convert.ToInt32(kvp.Value);
                        break;
                    case "CANT_DIAS":
                        LIC_PER.e_cant_dias = Convert.ToInt32(kvp.Value);
                        break;
                    case "OBSERVACIONES":
                        if (kvp.Value != null)
                            LIC_PER.o_licencia_per = kvp.Value.ToString();
                        break;
                }
            }

            //Validar licencia previo al registro de la misma
            NomadXML resultado;
            resultado = ValidarLicencia(LEG, Int32.Parse(LIC_PER.oi_licencia), LIC_PER.f_inicio, LIC_PER.f_fin, LIC_PER.e_cant_dias, LIC_PER.e_anio_corresp);

            if (resultado.GetAttr("valid") == "1")
            {
                LEGAJO.LICEN_PER.Add(LIC_PER);
                try
                {
                    NomadEnvironment.GetCurrentTransaction().Save(LEGAJO);
                    retorno = NucleusRH.Base.Configuracion.Herramientas.QUERIES.CreateRTA("OK", "La licencia fue registrada con éxito");

                    LEGAJO = NucleusRH.Base.Personal.LegajoEmpresa.PERSONAL_EMP.Get(LEG, true);
                    string id = LEGAJO.LICEN_PER[LEGAJO.LICEN_PER.Count - 1].Id;
                    GuardarLicenciaPorRol(id);
                    return retorno;
                }
                catch (Exception ex)
                {
                    NomadLog.Debug("Error al registrar la LICENCIA: " + ex);
                    retorno = NucleusRH.Base.Configuracion.Herramientas.QUERIES.CreateRTA("FATALERR", "La licencia pasó las validaciones pero hubo un error al guardar la misma\n" + ex.Message);
                    return retorno;
                }
            }
            else
            {
                if (resultado.GetAttr("alert") != "")
                {
                    retorno = NucleusRH.Base.Configuracion.Herramientas.QUERIES.CreateRTA("ERR", resultado.GetAttr("alert"));
                    return retorno;
                }
                else //Tiene un confirm pero se debe registrar la licencia igualmente
                {
                    LEGAJO.LICEN_PER.Add(LIC_PER);
                    try
                    {
                        NomadEnvironment.GetCurrentTransaction().Save(LEGAJO);
                        retorno = NucleusRH.Base.Configuracion.Herramientas.QUERIES.CreateRTA("OK", "La licencia fue registrada con éxito");

                        LEGAJO = NucleusRH.Base.Personal.LegajoEmpresa.PERSONAL_EMP.Get(LEG, true);
                        string id = LEGAJO.LICEN_PER[LEGAJO.LICEN_PER.Count - 1].Id;
                        GuardarLicenciaPorRol(id);
                        return retorno;
                    }
                    catch (Exception ex)
                    {
                        NomadLog.Debug("Error al registrar la LICENCIA: " + ex);
                        retorno = NucleusRH.Base.Configuracion.Herramientas.QUERIES.CreateRTA("FATALERR", "La licencia pasó las validaciones pero hubo un error al guardar la misma\n" + ex.Message);
                        return retorno;
                    }
                }
            }
        }

        public static System.Collections.Generic.SortedList<string,string> UpdateDatosLegajo( int PAR, SortedList<string,object> DATA1)
        {
            #region DEBUG
            NomadLog.Debug("-----------------------------------------");
            NomadLog.Debug("-----------Editar Datos Legajo-----------");
            NomadLog.Debug("-----------------------------------------");

            NomadLog.Debug("AddTrayectoriaALegajo.oi_personal_emp: " + PAR);
            foreach (KeyValuePair<string, object> kvp in DATA1)
            {
                if (kvp.Value != null) { NomadLog.Debug(kvp.Key.ToString() + ":" + kvp.Value.ToString()); }
                else { NomadLog.Debug(kvp.Key.ToString() + ":Null"); }
            }
            #endregion

            SortedList<string, string> retorno = new SortedList<string, string>();
            retorno.Add("VAL", "FATALERR");
            retorno.Add("DES", "El método no fue procesado");

            LegajoEmpresa.PERSONAL_EMP LEGAJO = LegajoEmpresa.PERSONAL_EMP.Get(PAR, true);

            if (LEGAJO == null)
            {
                retorno = NucleusRH.Base.Configuracion.Herramientas.QUERIES.CreateRTA("FATALERR", "El legajo con oi_personal_emp " + PAR.ToString() + " no se encontró.");
                return retorno;
            }

            try
            {
                //Trayectoria legajo
                LegajoEmpresa.UBICACION_PER UBI_PER = new UBICACION_PER();

                if (DATA1["UBI_VAL"] != null && DATA1["UBI_VAL"] != "0")
                {
                    DateTime fecha_desde = (DateTime)DATA1["F_DESDE"];

                    if (LEGAJO.UBIC_PER.Count == 0)
                    {
                        //Nueva trayectoria
                        UBI_PER.f_desde = fecha_desde;
                        UBI_PER.oi_ubicacion = DATA1["UBI_VAL"].ToString();

                        LEGAJO.UBIC_PER.Add(UBI_PER);

                        LEGAJO.oi_ubicacion = UBI_PER.oi_ubicacion;
                        LEGAJO.f_desde_ubicacion = fecha_desde;

                        NomadEnvironment.GetCurrentTransaction().Begin();
                        try
                        {
                            NomadEnvironment.GetCurrentTransaction().SaveRefresh(LEGAJO);
                            NomadEnvironment.GetCurrentTransaction().Commit();
                        }
                        catch (Exception ex)
                        {
                            NomadEnvironment.GetCurrentTransaction().Rollback();
                            retorno = NucleusRH.Base.Configuracion.Herramientas.QUERIES.CreateRTA("FATALERR", "No se puedo guardar la ubicacion. \n" + ex.Message);
                            return retorno;
                        }
                    }
                    else
                    {
                        //Busco la ultima trayectoria del legajo.
                        foreach (LegajoEmpresa.UBICACION_PER UBIPER in LEGAJO.UBIC_PER)
                        {
                            if (UBIPER.f_desde == LEGAJO.f_desde_ubicacion)
                            {
                                UBI_PER = UBIPER;
                                break;
                            }
                        }

                        //Si la ubicacion que manda es la misma que estaba ultima no actualizo
                        if (UBI_PER.oi_ubicacion != DATA1["UBI_VAL"].ToString())
                        {
                            //Valido que la fecha que ingresó el usuario no sea menor que la ultima trayectoria que tiene asiganada el legajo
                            if (UBI_PER.f_desde <= fecha_desde)
                            {
                                //Si es mayor agrego una nueva trayectoria,Si es igual actualizo la ubicacion solamente
                                if (UBI_PER.f_desde != fecha_desde)
                                {
                                    UBICACION_PER NEW_UBI = new UBICACION_PER();
                                    NEW_UBI.f_desde = fecha_desde;
                                    NEW_UBI.oi_ubicacion = DATA1["UBI_VAL"].ToString();

                                    LEGAJO.UBIC_PER.Add(NEW_UBI);

                                    LEGAJO.oi_ubicacion = NEW_UBI.oi_ubicacion;
                                    LEGAJO.f_desde_ubicacion = NEW_UBI.f_desde;

                                    //Actualizo los datos de la ultima posicion
                                    UBI_PER.f_hasta = fecha_desde.AddDays(-1);

                                    NomadEnvironment.GetCurrentTransaction().Begin();
                                    try
                                    {
                                        //NomadEnvironment.GetCurrentTransaction().SaveRefresh(UBI_PER);
                                        NomadEnvironment.GetCurrentTransaction().SaveRefresh(LEGAJO);
                                        NomadEnvironment.GetCurrentTransaction().Commit();
                                    }
                                    catch (Exception ex)
                                    {
                                        NomadEnvironment.GetCurrentTransaction().Rollback();
                                        retorno = NucleusRH.Base.Configuracion.Herramientas.QUERIES.CreateRTA("FATALERR", "No se puedo guardar la ubicacion. \n" + ex.Message);
                                        return retorno;
                                    }
                                }
                                else
                                {
                                    UBI_PER.oi_ubicacion = DATA1["UBI_VAL"].ToString();
                                    LEGAJO.oi_ubicacion = UBI_PER.oi_ubicacion;

                                    NomadEnvironment.GetCurrentTransaction().Begin();
                                    try
                                    {
                                        //NomadEnvironment.GetCurrentTransaction().SaveRefresh(UBI_PER);
                                        NomadEnvironment.GetCurrentTransaction().SaveRefresh(LEGAJO);
                                        NomadEnvironment.GetCurrentTransaction().Commit();
                                    }
                                    catch (Exception ex)
                                    {
                                        NomadEnvironment.GetCurrentTransaction().Rollback();
                                        retorno = NucleusRH.Base.Configuracion.Herramientas.QUERIES.CreateRTA("FATALERR", "No se puedo guardar la ubicacion. \n" + ex.Message);
                                        return retorno;
                                    }
                                }
                            }
                            else
                            {
                                retorno = NucleusRH.Base.Configuracion.Herramientas.QUERIES.CreateRTA("ERR", "La fecha desde la que va estar en la nueva ubicación tiene que ser mayor a la última ingresada.");
                                return retorno;
                            }
                        }
                    }
                }
                else { LEGAJO.oi_ubicacionNull = true; }

                foreach (KeyValuePair<string, object> kvp in DATA1)
                {
                    switch (kvp.Key)
                    {
                        case "NRO_INT":
                            if (kvp.Value != null) { LEGAJO.c_nro_interno = kvp.Value.ToString(); break; }
                            else { LEGAJO.c_nro_internoNull = true; break; }
                        case "DISP_VIA":
                            if (kvp.Value != null) { LEGAJO.l_disponib_viajar = (bool)kvp.Value; break; }
                            else { LEGAJO.l_disponib_viajar = false; break; }
                        case "AFI_SIND":
                            if (kvp.Value != null) { LEGAJO.l_afiliado_sind = (bool)kvp.Value; break; }
                            else { LEGAJO.l_afiliado_sind = false; break; }
                    }
                }
                NomadEnvironment.GetCurrentTransaction().Save(LEGAJO);

                retorno = NucleusRH.Base.Configuracion.Herramientas.QUERIES.CreateRTA("OK", "Los datos del legajo se modificaron exitosamente");
                return retorno;
            }
            catch(Exception ex)
            {
                retorno = NucleusRH.Base.Configuracion.Herramientas.QUERIES.CreateRTA("FATALERR", ex.Message);
                return retorno;
            }
        }

    public static void ReporteAntiguedad(NomadXML xmlParam)
    {
      NomadBatch objBatch = NomadBatch.GetBatch("Iniciando...", "Ejecución del Reporte");
      objBatch.SetMess("Obteniendo datos...");
      objBatch.SetPro(0);

      NomadXML xmlReporte = new NomadXML("DATA");
      NomadXML xmlFiltro = new NomadXML("FILTRO");

      DateTime f_calculo = StringUtil.str2date(xmlParam.FirstChild().GetAttr("f_calculo"));
      xmlFiltro.SetAttr("f_calculo", f_calculo.ToString("dd/MM/yyyy"));
      xmlFiltro.SetAttr("d_empresa", NomadEnvironment.QueryValue("ORG03_EMPRESAS", "d_empresa", "oi_empresa", xmlParam.FirstChild().GetAttr("oi_empresa"), "", false));

      objBatch.SetPro(10);

      ArrayList arrPersonas = xmlParam.FirstChild().FirstChild().GetChilds();

      int porcentaje = 10;
      int avance = (int)(80/arrPersonas.Count);

      foreach (NomadXML xmlRow in arrPersonas)
      {
        string[] arrLegajos = xmlRow.GetAttr("values").Split(',');

        //PARA CADA LEGAJO
        foreach (string oi_legajo in arrLegajos)
        {
          int ingresos = 0;
          int anios = 0;
          int meses = 0;
          int dias = 0;

          //Recuperar Legajo
          NucleusRH.Base.Personal.LegajoEmpresa.PERSONAL_EMP objLegajo =
            NucleusRH.Base.Personal.LegajoEmpresa.PERSONAL_EMP.Get(oi_legajo);

          NomadXML xmlLegajo = new NomadXML("LEGAJO");
          xmlLegajo.SetAttr("e_numero_legajo", objLegajo.e_numero_legajo);
          xmlLegajo.SetAttr("d_ape_y_nom", NomadEnvironment.QueryValue("PER01_PERSONAL", "d_ape_y_nom", "oi_personal", objLegajo.oi_personal, "", false));
          xmlLegajo.SetAttr("ultima_f_ingreso", objLegajo.f_ingreso.ToString("dd/MM/yyyy"));
          if (objLegajo.INGRESOS_PER.Count > 1) xmlLegajo.SetAttr("reingreso", "Si");
          else xmlLegajo.SetAttr("reingreso", "No");

          //SI TIENE FECHA RECONOCIDA DE ANTIGUEDAD
          if (!objLegajo.f_antiguedad_recNull)
          {
            xmlLegajo.SetAttr("f_antiguedad_rec", objLegajo.f_antiguedad_rec.ToString("dd/MM/yyyy"));
            CalcularAntiguedad(objLegajo.f_antiguedad_rec, f_calculo, ref anios, ref meses, ref dias);
            anios += (meses - (meses % 12)) / 12;
            meses = meses % 12;
          }
          //NO TIENE FECHA RECONOCIDA DE ANTIGUEDAD
          else
          {
            //PARA CADA INGRESO ANTERIOR A LA FECHA DE CALCULO
            foreach (LegajoEmpresa.INGRESOS_PER objIngreso in objLegajo.INGRESOS_PER)
            {
              if (objIngreso.f_ingreso < f_calculo)
              {
                ingresos++;

                if (objIngreso.f_egresoNull || f_calculo <= objIngreso.f_egreso)
                  CalcularAntiguedad(objIngreso.f_ingreso, f_calculo, ref anios, ref meses, ref dias);
                else
                  CalcularAntiguedad(objIngreso.f_ingreso, objIngreso.f_egreso, ref anios, ref meses, ref dias);
              }
            }

            anios += (meses - (meses % 12)) / 12;
            meses = meses % 12;

            //Contabilizar dias sobrantes si tiene varios ingresos
            if (ingresos > 1)
            {
              anios += (dias - (dias % 365)) / 365;
              dias = dias % 365;
              meses += (dias - (dias % 30)) / 30;
            }
          }

          string antiguedad = "";

          if (anios == 1) antiguedad += anios + " año ";
          else if (anios > 1) antiguedad += anios + " años ";

          if (meses == 1) antiguedad += meses + " mes ";
          else if (meses > 1) antiguedad += meses + " meses ";

          xmlLegajo.SetAttr("antiguedad", antiguedad);

          xmlFiltro.AddTailElement(xmlLegajo);
        }

        porcentaje += avance;
        objBatch.SetPro(porcentaje);
      }

      xmlReporte.AddTailElement(xmlFiltro);

      objBatch.SetMess("Generando Reporte...");
      objBatch.SetPro(90);

      // Generando Reporte
      Nomad.NomadHTML nmdHtml = new Nomad.NomadHTML("NucleusRH.Base.Personal.AntiguedadLegajos.rpt", xmlReporte.ToString());

      string outFileName = NomadProxy.GetProxy().Batch().ID + ".htm";
      string outFilePath = NomadProxy.GetProxy().RunPath + "Nomad\\TEMP\\";

      StreamWriter sw = new System.IO.StreamWriter(outFilePath + "\\" + outFileName, false, System.Text.Encoding.UTF8);
      nmdHtml.GenerateHTML(sw);
      sw.Close();

      objBatch.SetPro(100);
    }

    private static void CalcularAntiguedad(DateTime f_ini, DateTime f_fin, ref int anios, ref int meses, ref int dias)
    {
      DateTime fecha;

      for (fecha = f_ini.AddYears(1); fecha <= f_fin; fecha = fecha.AddYears(1))
      {
        anios++;
      }

      for (fecha = fecha.AddYears(-1).AddMonths(1); fecha <= f_fin; fecha = fecha.AddMonths(1))
      {
        meses++;
      }

      dias += f_fin.Subtract(fecha.AddMonths(-1)).Days;
    }

    public bool DiaLaboral(DateTime fecha)
    {
        NomadLog.Info("DiaLaboral:" + Nomad.NSystem.Functions.StringUtil.date2str(fecha));
        bool retval = false;

        //Obtengo el Calendario
        NucleusRH.Base.Organizacion.Calendario_Feriados.CALENDARIO obj_Cal = null;
        foreach (NucleusRH.Base.Personal.LegajoEmpresa.CALENDARIO_PER MyCAL in this.CALENDARIO_PER)
            if ((fecha >= MyCAL.f_desde) && (MyCAL.f_hastaNull || fecha <= MyCAL.f_hasta))
                obj_Cal = MyCAL.Getoi_calendario();
        if (obj_Cal == null)
            throw new NomadAppException("La persona no tiene un Calendario Definido para la Fecha Especificada.");

        //Calculo si ese Dia TRABAJA.
        if (obj_Cal.DIAS_FERIADOS.GetByAttribute("f_feriado", fecha) != null)
        {
            retval = obj_Cal.l_trab_feriados;
        }
        else
        {
            switch (fecha.DayOfWeek)
            {
                case System.DayOfWeek.Monday: retval = obj_Cal.l_trab_lunes; break;
                case System.DayOfWeek.Tuesday: retval = obj_Cal.l_trab_martes; break;
                case System.DayOfWeek.Wednesday: retval = obj_Cal.l_trab_miercoles; break;
                case System.DayOfWeek.Thursday: retval = obj_Cal.l_trab_jueves; break;
                case System.DayOfWeek.Friday: retval = obj_Cal.l_trab_viernes; break;
                case System.DayOfWeek.Saturday: retval = obj_Cal.l_trab_sabados; break;
                case System.DayOfWeek.Sunday: retval = obj_Cal.l_trab_domingos; break;
            }
        }
        return retval;
    }

            public static void Datos_Gantt(Nomad.NSystem.Document.NmdXmlDocument filtros)
        {
            NomadBatch objBatch;
            objBatch = NomadBatch.GetBatch("Iniciando...", "Gantt de Licencias");

            objBatch.SetPro(0);

            StringWriter swr = new StringWriter();
            XmlTextWriter xtw = new XmlTextWriter(swr);
            xtw.Formatting = Formatting.None;

            NucleusRH.Base.Organizacion.Calendario_Feriados.CALENDARIO calendario = NucleusRH.Base.Organizacion.Calendario_Feriados.CALENDARIO.Get(filtros.GetAttribute("oi_calendario").Value);
            bool[] week = new bool[7] { calendario.l_trab_domingos, calendario.l_trab_lunes, calendario.l_trab_martes, calendario.l_trab_miercoles, calendario.l_trab_jueves, calendario.l_trab_viernes, calendario.l_trab_sabados };

            // Recurso para obtener las personas de las solicitudes
            // XML de parametros para el query
            string xmlparam = filtros.ToString();

            // Ejecuto el recurso
            Nomad.NSystem.Document.NmdXmlDocument xml_personas = null;
            xml_personas = new Nomad.NSystem.Document.NmdXmlDocument(NomadEnvironment.QueryString(NucleusRH.Base.Personal.LegajoEmpresa.PERSONAL_EMP.Resources.QRY_PERSONAS_LIC, xmlparam));
            // FIN Recurso para obtener las personas de las solicitudes

            //Aqui formo el xml con XmlTextWriter
            xtw.WriteStartElement("DATOS");

            //Configuro los colores por estado
            Hashtable colorsTable = new Hashtable();
            ArrayList myColors = new ArrayList();
            myColors.Add("#3EB793");
            myColors.Add("#EAB541");
            myColors.Add("#8C93FF");
            myColors.Add("#E0676D");
            myColors.Add("#F7F171");
            int c=0;

            /*
            //INCORPORA LOS ESTADO EN EL REPORTE
            if (filtros.GetAttribute("oi_estado_solic").Value== "-")
            {
                // Ejecuto el recurso
                Nomad.NSystem.Document.NmdXmlDocument xml_estados = null;
                xml_estados = new Nomad.NSystem.Document.NmdXmlDocument(NomadEnvironment.QueryString(NucleusRH.Base.Vacaciones.Estados_Solicitud.ESTADO_SOLIC.Resources.INFO, "<DATA/>"));
                Nomad.NSystem.Document.NmdXmlDocument estado;
                // Cargo los estados con la info del recurso
                xtw.WriteStartElement("ESTADOS");
                for (estado = (Nomad.NSystem.Document.NmdXmlDocument)xml_estados.GetFirstChildDocument(); estado != null; estado = (Nomad.NSystem.Document.NmdXmlDocument)xml_estados.GetNextChildDocument())
                {
                    xtw.WriteStartElement("ESTADO");
                    xtw.WriteAttributeString("c_estado", estado.GetAttribute("COD").Value);
                    xtw.WriteAttributeString("d_estado", estado.GetAttribute("DES").Value);
                    xtw.WriteAttributeString("color",myColors[c].ToString());

                    colorsTable.Add(estado.GetAttribute("COD").Value, myColors[c]);
                    c++;
                    //Si se agregaron mas estados de solicitud repite colores
                    if(c==5)
                      c=0;
                    xtw.WriteEndElement();
                }
                xtw.WriteEndElement();

            }*/

            Nomad.NSystem.Document.NmdXmlDocument persona;
            for (persona = (Nomad.NSystem.Document.NmdXmlDocument)xml_personas.GetFirstChildDocument(); persona != null; persona = (Nomad.NSystem.Document.NmdXmlDocument)xml_personas.GetNextChildDocument())
            {
                xtw.WriteStartElement("PERSONA");
                xtw.WriteAttributeString("d_ape_y_nom", persona.GetAttribute("d_ape_y_nom").Value);
                xtw.WriteAttributeString("e_numero_legajo", persona.GetAttribute("e_numero_legajo").Value);
                xtw.WriteAttributeString("c_empresa", persona.GetAttribute("c_empresa").Value);
                xtw.WriteAttributeString("legajo", persona.GetAttribute("legajo").Value);
                xtw.WriteEndElement();
            }
            DateTime f_hasta = Nomad.NSystem.Functions.StringUtil.str2date(filtros.GetAttribute("f_hasta").Value);
            DateTime fecha = Nomad.NSystem.Functions.StringUtil.str2date(filtros.GetAttribute("f_desde").Value);
            DateTime f_inicio_mes;

            while (fecha <= f_hasta)
            {
                f_inicio_mes = fecha;
                int mes = fecha.Month;
                xtw.WriteStartElement("MES");
                xtw.WriteAttributeString("mes", fecha.ToString("MMM"));
                xtw.WriteAttributeString("anio", fecha.Year.ToString());
                xtw.WriteStartElement("DIAS");
                ArrayList dias_mes = new ArrayList();
                while (fecha.Month == mes && fecha <= f_hasta)
                {
                    xtw.WriteStartElement("DIA");
                    xtw.WriteAttributeString("dia", fecha.Day.ToString());
                    bool feriado = false;
                    string est;

                    // Marca cada dia como; habil, no habil, o feriado
                    foreach (NucleusRH.Base.Organizacion.Calendario_Feriados.DIA_FERIADO dia_feriado in calendario.DIAS_FERIADOS)
                    {
						if(dia_feriado.c_tipo != "NOLAB")
						{
							if (dia_feriado.f_feriado == fecha)
							{
								feriado = true;
							}							
						}
                    }
                    if (!feriado)
                    {
                        if (week[(int)fecha.DayOfWeek])
                        {
                            est = "h";
                        }
                        else
                        {
                            est = "n";
                        }
                    }
                    else
                    {
                        est = "f";
                    }
                    // Fin Marca cada dia como; habil, no habil, o feriado

                    // Marca el dia de hoy
                    if (fecha == DateTime.Today)
                    {
                        est = "hoy";
                    }
                    // Marca el dia de hoy

                    xtw.WriteAttributeString("est", est);
                    dias_mes.Add(est);
                    xtw.WriteEndElement();
                    fecha = fecha.AddDays(1);
                }
                xtw.WriteEndElement();
                xtw.WriteStartElement("PERSONAS");

                for (persona = (Nomad.NSystem.Document.NmdXmlDocument)xml_personas.GetFirstChildDocument(); persona != null; persona = (Nomad.NSystem.Document.NmdXmlDocument)xml_personas.GetNextChildDocument())
                {
                    Nomad.NSystem.Document.NmdXmlDocument licencias = (Nomad.NSystem.Document.NmdXmlDocument)persona.GetDocumentByName("LICENCIAS");
                    xtw.WriteStartElement("PERSONA");
                    fecha = f_inicio_mes;
                    int cont = 0;
                    while (fecha.Month == mes && fecha <= f_hasta)
                    {
                        string sol = "n";
                        string estado = "-";
                        Nomad.NSystem.Document.NmdXmlDocument licencia;
                        xtw.WriteStartElement("DIA");

                        // Recorro las licencias de la persona
                        // Marca cada dia como asignado o no asignado
                        for (licencia = (Nomad.NSystem.Document.NmdXmlDocument)licencias.GetFirstChildDocument(); licencia != null; licencia = (Nomad.NSystem.Document.NmdXmlDocument)licencias.GetNextChildDocument())
                        {
                            DateTime f_desde_lic = Nomad.NSystem.Functions.StringUtil.str2date(licencia.GetAttribute("f_desde").Value);
                            DateTime f_hasta_lic = Nomad.NSystem.Functions.StringUtil.str2date(licencia.GetAttribute("f_hasta").Value);

                            if (fecha >= f_desde_lic && fecha <= f_hasta_lic)
                            {
                                sol = "a";
                                xtw.WriteAttributeString("app",NomadProxy.GetProxy().Application.Name);
                                xtw.WriteAttributeString("oi_licencia_per", licencia.GetAttribute("oi_licencia_per").Value);

                                //Buscar el codigo del tipo de la licencia de personal
                                estado = licencia.GetAttribute("c_licencia").Value;
                                xtw.WriteAttributeString("color", "#F7F171");
                                break;
                            }
                        }
                        xtw.WriteAttributeString("est", dias_mes[cont].ToString() + sol);
                        xtw.WriteAttributeString("estado", estado);
                        xtw.WriteEndElement();
                        fecha = fecha.AddDays(1);
                        cont++;
                    }
                    xtw.WriteEndElement();
                    objBatch.SetMess("Recorriendo días");
                    objBatch.SetPro(0, 100, cont, DateTime.Compare(fecha, f_hasta));

                }
                xtw.WriteEndElement();
                xtw.WriteEndElement();
            }

            xtw.Flush();
            xtw.Close();

            string myXml = swr.ToString();

            Nomad.NSystem.Document.NmdXmlDocument doc = new Nomad.NSystem.Document.NmdXmlDocument(myXml);

            NomadXML xmlrep = new NomadXML(doc.ToString());

            objBatch.SetMess("Ejecutando el Reporte...");
            NomadBatch.Trace(xmlrep.ToString());
            Nomad.NomadHTML nmdHtml = new Nomad.NomadHTML("NucleusRH.Base.Personal.GanttLicencias.rpt", xmlrep.ToString());

            string outFileName = NomadProxy.GetProxy().Batch().ID + ".htm";
            string outFilePath = NomadProxy.GetProxy().RunPath + "Nomad\\TEMP\\";
            NomadBatch.Trace("Generando Reporte HTML en path:'" + outFilePath + outFileName + "'");

            StreamWriter sw = new System.IO.StreamWriter(outFilePath + "\\" + outFileName, false, System.Text.Encoding.UTF8);

            nmdHtml.GenerateHTML(sw);

            sw.Close();
        }

            public static void ProcesarEntidades()
            {
                NomadBatch objBatch;
                objBatch = NomadBatch.GetBatch("Iniciando...", "Procesar Entidades");

                objBatch.SetPro(0);

                NomadXML xml_personas = NomadEnvironment.QueryNomadXML(PERSONAL_EMP.Resources.QRY_PERSONAS_SIN_ENTIDAD, "").FirstChild();

                int tiempoEntreMails = xml_personas.GetAttrInt("c_min_mails");
                string AsuntoMails = xml_personas.GetAttr("c_aunto_mails");
                string CuerpoMails = xml_personas.GetAttr("c_cuerpo_mails");

                //Los parametros del asunto y cuerpo del mail son obligatorios
                if (AsuntoMails == "" || CuerpoMails == "")
                {
                    if (AsuntoMails == "") objBatch.Err("No está definido el parámetro del Asunto del mail - Código: 'ASUNTOMAILS'");
                    if (CuerpoMails == "") objBatch.Err("No está definido el parámetro del Cuerpo del mail - Código: 'CUERPOMAILS'");
                }
                else
                {
                    int i = 0;
                    foreach (NomadXML xml_per in xml_personas.GetChilds())
                    {
                        Nomad.Base.Login.Entidades.ENTIDAD entidad = new Nomad.Base.Login.Entidades.ENTIDAD();

                        entidad.COD = xml_per.GetAttr("c_nro_documento");
                        entidad.DES = xml_per.GetAttr("d_ape_y_nom");
                        entidad.IDIOMA = "SPA";

                        Nomad.Base.Login.Entidades.MAIL mail = new Nomad.Base.Login.Entidades.MAIL();
                        mail.EMAIL = xml_per.GetAttr("d_email");
                        mail.PRINCIPAL = true;

                        entidad.MAILS.Add(mail);

                        NomadEnvironment.GetCurrentTransaction().SaveRefresh(entidad);

                        Nomad.Base.Login.Cuentas.CUENTA cuenta = new Nomad.Base.Login.Cuentas.CUENTA();
                        cuenta.ENTIDAD = entidad.COD;
                        cuenta.POLITICA = "DEFAULT";
                        cuenta.LOGIN = entidad.COD;
                        cuenta.CLAVE = GenerarPassword();
                        cuenta.CAMBIAR_EN_PROXIMA = true;
                        cuenta.ACTIVO = true;

                        NomadEnvironment.GetCurrentTransaction().Save(cuenta);

                        NucleusRH.Base.Personal.Legajo.PERSONAL persona = Legajo.PERSONAL.Get(xml_per.GetAttr("oi_personal"));
                        persona.oi_usuario_sistema = entidad.COD;

                        NomadEnvironment.GetCurrentTransaction().Save(persona);

                        CuerpoMails = CuerpoMails.Replace("{usuario}", cuenta.LOGIN);
                        CuerpoMails = CuerpoMails.Replace("{password}", cuenta.CLAVE);

                        string mailTXT = CuerpoMails;
                        string mailSBJ = AsuntoMails;
                        string mailEnviar = mail.EMAIL;

                        Nomad.Base.Mail.Mails.MAIL objMAIL = Nomad.Base.Mail.Mails.MAIL.CrearTXT(mailSBJ, mailTXT, mailEnviar, "soporte@nucleussa.com.ar");

                    objMAIL.Enviar_Programado(DateTime.Now.AddSeconds(tiempoEntreMails * i),null);

                        i++;
                    }
                }
            }

            private static string GenerarPassword()
            {
                string pass = null;
                Random random = new Random();

                for (int i = 0; i < 6; i++)
                {
                    if (random.Next(1, 10) >= 5)
                    {
                        pass += random.Next(0, 9).ToString();
                    }
                    else
                    {
                        pass += Convert.ToChar(Convert.ToInt32(Math.Floor(26 * random.NextDouble() + 65)));
                    }
                }
                return pass;
            }

            public string MarcarDefecto()
            {
                if (this.oi_indic_activo != "4")
                {
                    NomadXML legajosXml = NomadEnvironment.QueryNomadXML(Resources.QRY_LEGAJOS, "<DATA oi_personal_emp='"+this.id+"' oi_personal='" + this.oi_personal + "' />").FirstChild();

                    if(legajosXml.FirstChild() != null)
                    {
                        for (NomadXML leg = legajosXml.FirstChild(); leg != null; leg = leg.Next())
                        {
                            string oi_personal_emp = leg.GetAttr("oi_personal_emp");
                            Personal.LegajoEmpresa.PERSONAL_EMP pEmp = Personal.LegajoEmpresa.PERSONAL_EMP.Get(oi_personal_emp);
                            pEmp.l_defecto = false;
                            try
                            {
                                NomadEnvironment.GetCurrentTransaction().Save(pEmp);
                            }
                            catch (Exception e)
                            {
                                throw new NomadAppException("Error al guardar legajo por defecto");
                            }
                        }
                    }
                    this.l_defecto = true;

                    try
                    {
                        NomadEnvironment.GetCurrentTransaction().Save(this);
                    }
                    catch (Exception e)
                    {
                        throw new NomadAppException("Error al guardar legajo por defecto");
                    }
                    return "1";
                }
                return "0";
            }
    }
}



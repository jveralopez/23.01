using System;
using System.Xml;
using System.IO;
using System.Collections;

using Nomad.NSystem.Document;
using Nomad.NSystem.Proxy;
using Nomad.NSystem.Base;
using Nomad.NSystem.Functions;

namespace NucleusRH.Base.Control_Contratistas.LegajoEmpresa
{

    //////////////////////////////////////////////////////////////////////////////////
    //Clase Personal por Empresa Contratista
    public partial class PERSONAL_EMP : NucleusRH.Base.Personal.LegajoEmpresa.PERSONAL_EMP
    {
        //guarda una persona nueva y un legajo contratista
        public bool IngresoPerCCO(Nomad.NSystem.Proxy.NomadXML paramATRIBUTOS_PER)
        {

            if (paramATRIBUTOS_PER.isDocument)
                 paramATRIBUTOS_PER = paramATRIBUTOS_PER.FirstChild();

            NomadXML rowDetalle = paramATRIBUTOS_PER;       
            NucleusRH.Base.Personal.Legajo.PERSONAL ddoPER = null;

            string codigo = rowDetalle.GetAttr("c_personal");
            string strval = "";

            //Valido el nro de Legajo en la empresa
            strval = NomadEnvironment.QueryValue("PER02_PERSONAL_EMP", "oi_personal_emp", "e_numero_legajo", this.e_numero_legajo.ToString(), "PER02_PERSONAL_EMP.oi_empresa = " + this.oi_empresa, true);
            NomadLog.Info("strval 2 -- " + strval);
            if (strval != null)
            {
                throw new NomadAppException("Ya existe un legajo con el numero de legajo especificado en la Empresa indicada");
                return false;
            }

            //Valido el c_personal        
            strval = NomadEnvironment.QueryValue("PER01_PERSONAL", "oi_personal", "c_personal", codigo, "", true);
            NomadLog.Info("strval 1 -- " + strval);
            if(strval != null)
            {
                ddoPER = NucleusRH.Base.Personal.Legajo.PERSONAL.Get(strval);
            }
            else
            {
                ddoPER = new NucleusRH.Base.Personal.Legajo.PERSONAL();
            }

            ddoPER.d_apellido = rowDetalle.GetAttr("d_apellido");
            ddoPER.d_nombres = rowDetalle.GetAttr("d_nombres");
            ddoPER.d_ape_y_nom = rowDetalle.GetAttr("d_ape_y_nom");
            ddoPER.oi_tipo_documento = rowDetalle.GetAttr("oi_tipo_documento");
            ddoPER.c_nro_documento = rowDetalle.GetAttr("c_nro_documento");
            ddoPER.c_personal = rowDetalle.GetAttr("c_personal");
            ddoPER.c_sexo = rowDetalle.GetAttr("c_sexo");
            ddoPER.oi_grupo_sanguineo = rowDetalle.GetAttr("oi_grupo_sanguineo");
            ddoPER.te_celular = rowDetalle.GetAttr("te_celular");

            ddoPER.c_nro_cuil = rowDetalle.GetAttr("c_nro_cuil");

            if (rowDetalle.GetAttrDateTime("f_nacim") != null && rowDetalle.GetAttr("f_nacim") != "")
            {
                ddoPER.f_nacim = rowDetalle.GetAttrDateTime("f_nacim");
            }
            else { ddoPER.f_nacimNull = true; }

            ddoPER.oi_nacionalidad = rowDetalle.GetAttr("oi_nacionalidad");
            ddoPER.oi_estado_civil = rowDetalle.GetAttr("oi_estado_civil");
            ddoPER.oi_localidad = rowDetalle.GetAttr("oi_localidad");

            if (rowDetalle.GetAttr("oi_foto") != null && rowDetalle.GetAttr("oi_foto") != "")
            { ddoPER.oi_foto = rowDetalle.GetAttr("oi_foto"); }

            ddoPER.Ingreso_Personal();
            NomadEnvironment.GetCurrentTransaction().SaveRefresh(ddoPER);        

            //HACER BLOQUE TRY CATCH PARA LAS TRANSACCIONES..

            //Legajo
            NomadLog.Info("ddoPER.Id -- " + ddoPER.Id);
            this.oi_personal = ddoPER.Id;
            this.Ingreso_Personal();

            NomadEnvironment.GetCurrentTransaction().SaveRefresh(this);
            return true;

        }

        //guarda un nuevo legajo contratista de una persona ya existente
        public bool IngresoLegCCO(string pstroi_personal)
        {

            //Valido el nro de Legajo en la empresa
            string strval = "";
            strval = NomadEnvironment.QueryValue("PER02_PERSONAL_EMP", "oi_personal_emp", "e_numero_legajo", this.e_numero_legajo.ToString(), "PER02_PERSONAL_EMP.oi_empresa = " + this.oi_empresa, true);
            NomadLog.Info("strval 2 -- " + strval);
            if (strval != null)
            {
                throw new NomadAppException("Ya existe un legajo con el numero de legajo especificado en la Empresa indicada");
                return false;
            }

            //Legajo
            this.oi_personal = pstroi_personal;
            this.Ingreso_Personal();

            NomadEnvironment.GetCurrentTransaction().Save(this);
            return true;

        }

        //valida que la fecha de egreso sea mayor a la de ingreso a planta
        public void Egreso_Personal_Contratista(DateTime f_egreso, string o_egreso, string oi_motivo_eg_per)
        {
            try
            {
                if (f_egreso < this.f_ingreso_planta)
                {
                    throw new NomadAppException("La fecha de egreso (" + f_egreso.ToString("dd/MM/yyyy") + ") no puede ser anterior a la fecha de ingreso a la planta (" + this.f_ingreso_planta.ToString("dd/MM/yyyy") + ")");
                }

                this.Egreso_Personal(f_egreso, o_egreso, oi_motivo_eg_per);
            }
            catch (Exception e)
            {
                throw new NomadAppException("Error al egresar el legajo. " + e.Message);
            }
        }

        //guarda los atributos de control luego de guardar un legajo en el flow
        //la parte de agregar o eliminar registros que tengan un check no se usa, queda por las dudas.
        public static void GuardarAtributosControl(NucleusRH.Base.Control_Contratistas.LegajoEmpresa.PERSONAL_EMP paramLEGAJO, Nomad.NSystem.Proxy.NomadXML paramATRIBUTOS)
        {

            NomadLog.Info("****** Guardar Atributos de Control ******");
            NomadLog.Debug("GuardarAtributosControl.paramLEGAJO " + paramLEGAJO.ToString());
            NomadLog.Debug("GuardarAtributosControl.paramATRIBUTOS " + paramATRIBUTOS.ToString());

            if (paramATRIBUTOS.isDocument)
                paramATRIBUTOS = paramATRIBUTOS.FirstChild();

            NomadTransaction objTransaccion = new NomadTransaction();
            objTransaccion.Begin();

            try
            {

                //recorro lista de atributos a controlar
                for (NomadXML rowDetalle = paramATRIBUTOS.FirstChild(); rowDetalle != null; rowDetalle = rowDetalle.Next())
                {

                    //el registro ya existe en la bd (la persona controla ese atributo)
                    if (rowDetalle.GetAttr("oi_atrib_per_con") != null && rowDetalle.GetAttr("oi_atrib_per_con") != "")
                    {
                        //ACTUALIZA EL ATRIBUTO DE LA PERSONA
                        //id
                        string oi_atrib_per_con = rowDetalle.GetAttr("oi_atrib_per_con");
                        NucleusRH.Base.Control_Contratistas.LegajoEmpresa.ATRIB_PER_CON atributo = (ATRIB_PER_CON)paramLEGAJO.ATRIB_PER_CON.GetById(oi_atrib_per_con);

                        //si completo alguno de los campos (f_vencimiento o observacion)
                        if (rowDetalle.GetAttr("f_vencimiento") != "" || rowDetalle.GetAttr("o_atrib_per_con") != "")
                        {

                            //fecha de vencimiento
                            if (rowDetalle.GetAttrDateTime("f_vencimiento") != null && rowDetalle.GetAttr("f_vencimiento") != "")
                            {
                                atributo.f_vencimiento = rowDetalle.GetAttrDateTime("f_vencimiento");
                            }
                            else { atributo.f_vencimientoNull = true; }

                            //observacion
                            if (rowDetalle.GetAttr("o_atrib_per_con") != null)
                            { atributo.o_atrib_per_con = rowDetalle.GetAttr("o_atrib_per_con"); }
                        }
                        else
                        {
                            //BORRA EL ATRIBUTO DE LA PERSONA
                            paramLEGAJO.ATRIB_PER_CON.Remove(atributo);

                        }

                    }
                    else //si el registro no existe, la persona no controla aún ese atributo
                    {
                        //AGREGA EL ATRIBUTO DE LA PERSONA
                        //si completo alguno de los campos (f_vencimiento o observacion)
                        if (rowDetalle.GetAttr("f_vencimiento") != "" || rowDetalle.GetAttr("o_atrib_per_con") != "")
                        {
                            //creo un nuevo registro
                            NucleusRH.Base.Control_Contratistas.LegajoEmpresa.ATRIB_PER_CON atributo = NucleusRH.Base.Control_Contratistas.LegajoEmpresa.ATRIB_PER_CON.New();

                            //setea el atributo de control
                            atributo.oi_atributo_ctrol = rowDetalle.GetAttr("id");

                            //fecha de vencimiento
                            if (rowDetalle.GetAttrDateTime("f_vencimiento") != null && rowDetalle.GetAttr("f_vencimiento") != "")
                            {
                                atributo.f_vencimiento = rowDetalle.GetAttrDateTime("f_vencimiento");
                            }
                            else { atributo.f_vencimientoNull = true; }

                            //observacion
                            if (rowDetalle.GetAttr("o_atrib_per_con") != null)
                            { atributo.o_atrib_per_con = rowDetalle.GetAttr("o_atrib_per_con"); }

                            //se lo agrega a al arreglo de atributos del legajo
                            paramLEGAJO.ATRIB_PER_CON.Add(atributo);

                        }

                    }

                }

                objTransaccion.Save(paramLEGAJO);
                objTransaccion.Commit();

                NomadLog.Info("******FIN Guardar Atributos de Control ******");
            }
            catch (Exception e)
            {
                objTransaccion.Rollback();
                throw new NomadAppException("Error no manejado. " + e.Message);
            }

        }

        //modifica los datos de la persona, al modificar el Legajo Contratista
        public static void ModificarPerCCO(Nomad.NSystem.Proxy.NomadXML paramATRIBUTOS_PER)
        {

            //Trae los datos del documento PER
            if (paramATRIBUTOS_PER.isDocument)
                paramATRIBUTOS_PER = paramATRIBUTOS_PER.FirstChild();

            //Carga el row con los campos
            NomadXML rowDetalle = paramATRIBUTOS_PER;

            try {

                //Busco la persona a editar
                NucleusRH.Base.Personal.Legajo.PERSONAL ddoPER = NucleusRH.Base.Personal.Legajo.PERSONAL.Get(rowDetalle.GetAttr("oi_personal"));

                //Cargo los datos básicos
                ddoPER.d_apellido = rowDetalle.GetAttr("d_apellido");
                ddoPER.d_nombres = rowDetalle.GetAttr("d_nombres");
                ddoPER.d_ape_y_nom = rowDetalle.GetAttr("d_ape_y_nom");
                ddoPER.c_sexo = rowDetalle.GetAttr("c_sexo");
                ddoPER.oi_grupo_sanguineo = rowDetalle.GetAttr("oi_grupo_sanguineo");
                ddoPER.te_celular = rowDetalle.GetAttr("te_celular");

                //fecha de nacimiento
                if (rowDetalle.GetAttrDateTime("f_nacim") != null && rowDetalle.GetAttr("f_nacim") != "")
                {
                    ddoPER.f_nacim = rowDetalle.GetAttrDateTime("f_nacim");
                }
                else { ddoPER.f_nacimNull = true; }

                ddoPER.oi_nacionalidad = rowDetalle.GetAttr("oi_nacionalidad");
                ddoPER.oi_estado_civil = rowDetalle.GetAttr("oi_estado_civil");
                ddoPER.oi_localidad = rowDetalle.GetAttr("oi_localidad");

                //foto
                if (rowDetalle.GetAttr("oi_foto") != null && rowDetalle.GetAttr("oi_foto") != "")
                { ddoPER.oi_foto = rowDetalle.GetAttr("oi_foto"); }

                //guarda los cambios en el Personal
                NomadEnvironment.GetCurrentTransaction().SaveRefresh(ddoPER);

            }
            catch (Exception e)
            {
                throw new NomadAppException("Error no manejado. " + e.Message);
            }

        }

    }
}


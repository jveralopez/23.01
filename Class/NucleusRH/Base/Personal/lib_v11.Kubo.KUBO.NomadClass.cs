using System;
using System.Xml;
using System.IO;
using System.Collections;

using Nomad.NSystem.Document;
using Nomad.NSystem.Proxy;
using Nomad.NSystem.Base;
using Nomad.NSystem.Functions;
using System.Collections.Generic;

using NucleusRH.Base.Personal.LegajoEmpresa;


namespace NucleusRH.Base.Personal.Kubo
{
    //////////////////////////////////////////////////////////////////////////////////
    //Clase de Metodos para Kubo
    public partial class KUBO
    {
        public static void EditTalle(int id, string C_NRO_BUZO, string C_NRO_CAMISA, string C_NRO_CHOMBA, string C_NRO_CAMPERA, string C_NRO_PANTALON, string C_NRO_CALZADO)
        {
            NomadLog.Debug("--------------------------------------");
            NomadLog.Debug("-----------ACTUALIZO TALLE------------");
            NomadLog.Debug("--------------------------------------");

            NucleusRH.Base.Personal.Legajo.PERSONAL PERSONAL = NucleusRH.Base.Personal.Legajo.PERSONAL.Get(id);

            if (PERSONAL == null)
            {
                throw new NomadAppException("El personal no existe");
            }

            PERSONAL.c_nro_camisa = C_NRO_CAMISA;
            PERSONAL.c_nro_pantalon = C_NRO_PANTALON;
            PERSONAL.c_nro_calzado = C_NRO_CALZADO;
            PERSONAL.c_nro_buzo = C_NRO_BUZO;
            PERSONAL.c_nro_campera = C_NRO_CAMPERA;
            PERSONAL.c_nro_chomba = C_NRO_CHOMBA;

            //grabo los cambios
            NomadEnvironment.GetCurrentTransaction().Save(PERSONAL);

        }

        public static void ValidaryGuardarLicencia(int OI_PERSONAL_EMP, int LIC, DateTime F_INICIO, DateTime F_FIN, int CANT_DIAS, int ANIO, string OBSERVACIONES, bool GUARDAR)
        {
            if(GUARDAR)
            {
                SortedList<string, object> data = new SortedList<string, object>();

                data.Add("OI_LICENCIA", LIC.ToString());               
                data.Add("F_INICIO", F_INICIO);
                data.Add("F_FIN", F_FIN);
                data.Add("ANIO", ANIO);
                data.Add("CANT_DIAS", CANT_DIAS);
                data.Add("OBSERVACIONES", OBSERVACIONES);

                SortedList<string, string> retorno = new SortedList<string, string>();
                retorno = PERSONAL_EMP.NewValidarYGuardarLicencia(OI_PERSONAL_EMP, data);                

                if (retorno["VAL"] == "ERR")
                {
                    throw new NomadAppException(retorno["DES"]);
                }
            }
            else
            {
                SortedList<string, string> retorno = new SortedList<string, string>();
                retorno = PERSONAL_EMP.NewValidarPedidoLicencia(OI_PERSONAL_EMP, LIC, F_INICIO, F_FIN, CANT_DIAS, ANIO);

                if (retorno["VAL"] == "ERR")
                {
                    throw new NomadAppException(retorno["DES"]);
                }
            }
        }

        public static void ActDatosPer(int id, string D_NOMBRES, string D_APELLIDO, DateTime F_NACIMIENTO, int GRUPO_SANGUINEO, int LOC_NAC)
        {
            //Ver de poner los campos obligatorios que van en los controles
            //Si viene vacio - pongo null

            NomadLog.Debug("--------------------------------------");
            NomadLog.Debug("-----------ACTUALIZO PERSONA------------");
            NomadLog.Debug("--------------------------------------");
           
            NomadLog.Debug("id: " + id.ToString());
            NomadLog.Debug("D_NOMBRES: " + D_NOMBRES.ToString());
            NomadLog.Debug("D_APELLIDO: " + D_APELLIDO.ToString());
            NomadLog.Debug("Fecha de nacimiento: " + F_NACIMIENTO.ToString());
            NomadLog.Debug("GRUPO_SANGUINEO: " + GRUPO_SANGUINEO.ToString());
            NomadLog.Debug("LOC_NAC: " + LOC_NAC.ToString());

            //Si es 01/01/0001 12 am --> es porque es null en Kubo
            DateTime f_comp = new DateTime(0001, 01, 01);

            NucleusRH.Base.Personal.Legajo.PERSONAL PERSONAL = NucleusRH.Base.Personal.Legajo.PERSONAL.Get(id);

            if (PERSONAL == null)
            {
                throw new NomadAppException("El personal no existe");
            }

            PERSONAL.d_nombres = D_NOMBRES.ToString();
            PERSONAL.d_apellido = D_APELLIDO.ToString();

            if (GRUPO_SANGUINEO != 0) 
            {
                PERSONAL.oi_grupo_sanguineo = GRUPO_SANGUINEO.ToString(); 
            }
            else 
            {
                PERSONAL.oi_grupo_sanguineoNull = true; 
            }

            if (f_comp == F_NACIMIENTO)
            {
                PERSONAL.f_nacimNull = true;
            }
            else
            {
                PERSONAL.f_nacim = (DateTime)F_NACIMIENTO;
            }

            if (LOC_NAC != 0) 
            {
                PERSONAL.oi_local_nacim = LOC_NAC.ToString(); 
            }
            else 
            {
                PERSONAL.oi_local_nacimNull = true; 
            }
           
            //grabo los cambios
            NomadEnvironment.GetCurrentTransaction().Save(PERSONAL);
        }
                
        public static void DatosFam(int id, int FAMILIAR_PER, bool MODIFICAR, string D_NOMBRES, string D_APELLIDO, int TIPO_FLIAR, int TIPO_DOC, string C_NRODOC, string C_SEXO, string CUIL, int ESTADO_CIVIL, int NACIONALIDAD, DateTime F_NACIMIENTO, int OI_OCUPACION, int OI_LOCALIDAD_NAC, double PORCENTAJE, bool L_DISCAPACIDAD, bool L_ACARGO_AF, bool L_CARGO_OS, bool L_ACARGO_SV)
        {            
            DateTime f_comp = new DateTime(0001, 01, 01);

            //Modificar
            if (MODIFICAR)
            {
                NomadLog.Debug("-----------ACTUALIZO FAMILIAR------------");

                //Get Familiar
                NucleusRH.Base.Personal.Legajo.FAMILIAR_PER FAMILIAR = NucleusRH.Base.Personal.Legajo.FAMILIAR_PER.Get(FAMILIAR_PER);

                if (FAMILIAR == null)
                {
                    throw new NomadAppException("El familiar con oi_familiar " + FAMILIAR_PER + "no se puede encontrar");
                }

                FAMILIAR.oi_tipo_familiar = TIPO_FLIAR.ToString();
                FAMILIAR.d_apellido = D_APELLIDO;
                FAMILIAR.d_nombres = D_NOMBRES;
                FAMILIAR.oi_tipo_documento = TIPO_DOC.ToString();
                FAMILIAR.c_nro_documento = C_NRODOC;
                FAMILIAR.d_ape_y_nom = FAMILIAR.d_apellido + ", " + FAMILIAR.d_nombres;
                FAMILIAR.l_vive = true;
                FAMILIAR.c_sexo = C_SEXO;
                if (CUIL != null && CUIL!="") { FAMILIAR.c_nro_cuil = CUIL.ToString(); } else { FAMILIAR.c_nro_cuilNull = true; }
                if (OI_OCUPACION != 0) { FAMILIAR.oi_ocupacion_fam = OI_OCUPACION.ToString(); } else { FAMILIAR.oi_ocupacion_famNull = true; }
                if (f_comp == F_NACIMIENTO) { FAMILIAR.f_nacimientoNull = true; } else { FAMILIAR.f_nacimiento = (DateTime)F_NACIMIENTO; }
                if (ESTADO_CIVIL != 0) { FAMILIAR.oi_estado_civil = ESTADO_CIVIL.ToString(); } else { FAMILIAR.oi_estado_civilNull = true; }
                if (NACIONALIDAD != 0) { FAMILIAR.oi_nacionalidad = NACIONALIDAD.ToString(); } else { FAMILIAR.oi_nacionalidadNull = true; }
                if (OI_LOCALIDAD_NAC != 0) { FAMILIAR.oi_localidad_nac = OI_LOCALIDAD_NAC.ToString(); } else { FAMILIAR.oi_localidad_nacNull = true; }
                FAMILIAR.n_porc_seguro = (double)PORCENTAJE;
                FAMILIAR.l_acargo_af = (bool)L_ACARGO_AF;
                FAMILIAR.l_acargo_os = (bool)L_CARGO_OS;
                FAMILIAR.l_acargo_SV = (bool)L_ACARGO_SV;
                FAMILIAR.l_discapacidad = (bool)L_DISCAPACIDAD;

                NomadLog.Debug("GUARDAR");
                try
                {
                    NomadEnvironment.GetCurrentTransaction().SaveRefresh(FAMILIAR);
                }
                catch (Exception ex)
                {
                    throw new NomadAppException("Error guardando FAMILIAR ", ex);
                }
            }

            //Alta            
            else
            {
                //Get PERSONAL
                NucleusRH.Base.Personal.Legajo.FAMILIAR_PER FAMILIAR = new NucleusRH.Base.Personal.Legajo.FAMILIAR_PER();
                NucleusRH.Base.Personal.Legajo.PERSONAL PERSONAL = NucleusRH.Base.Personal.Legajo.PERSONAL.Get(id, true);

                if (PERSONAL == null)
                {
                    throw new NomadAppException("El personal no existe");
                }

                //Validar que el familiar no esté agregado ya
                if (C_NRODOC != "" && C_NRODOC != null && PERSONAL.FLIARES_PER.Count > 0)
                {
                    foreach (NucleusRH.Base.Personal.Legajo.FAMILIAR_PER fam in PERSONAL.FLIARES_PER)
                    {
                        if (fam.c_nro_documento == C_NRODOC)
                        {
                            throw new NomadAppException("El familiar con DNI " + C_NRODOC + " que quiere agregar ya está cargado.");
                        }
                    }
                }

                FAMILIAR.oi_tipo_familiar = TIPO_FLIAR.ToString();
                FAMILIAR.d_apellido = D_APELLIDO;
                FAMILIAR.d_nombres = D_NOMBRES;
                FAMILIAR.oi_tipo_documento = TIPO_DOC.ToString();
                FAMILIAR.c_nro_documento = C_NRODOC;
                FAMILIAR.d_ape_y_nom = FAMILIAR.d_apellido + ", " + FAMILIAR.d_nombres;
                FAMILIAR.l_vive = true;
                FAMILIAR.c_sexo = C_SEXO;
                if (CUIL != null && CUIL!="") { FAMILIAR.c_nro_cuil = CUIL.ToString(); } else { FAMILIAR.c_nro_cuilNull = true; }
                if (OI_OCUPACION != 0) { FAMILIAR.oi_ocupacion_fam = OI_OCUPACION.ToString(); } else { FAMILIAR.oi_ocupacion_famNull = true; }                
                if (f_comp == F_NACIMIENTO) { FAMILIAR.f_nacimientoNull = true; } else { FAMILIAR.f_nacimiento = (DateTime)F_NACIMIENTO; }
                if (ESTADO_CIVIL != 0) { FAMILIAR.oi_estado_civil = ESTADO_CIVIL.ToString(); } else { FAMILIAR.oi_estado_civilNull = true; }
                if (NACIONALIDAD != 0) { FAMILIAR.oi_nacionalidad = NACIONALIDAD.ToString(); } else { FAMILIAR.oi_nacionalidadNull = true; }
                if (OI_LOCALIDAD_NAC != 0) { FAMILIAR.oi_localidad_nac = OI_LOCALIDAD_NAC.ToString(); } else { FAMILIAR.oi_localidad_nacNull = true; }
                FAMILIAR.n_porc_seguro = (double)PORCENTAJE; 
                FAMILIAR.l_acargo_af = (bool)L_ACARGO_AF;
                FAMILIAR.l_acargo_os = (bool)L_CARGO_OS;
                FAMILIAR.l_acargo_SV = (bool)L_ACARGO_SV;
                FAMILIAR.l_discapacidad = (bool)L_DISCAPACIDAD;

                PERSONAL.FLIARES_PER.Add(FAMILIAR);

                NomadLog.Debug("GUARDAR");
                try
                {
                    NomadEnvironment.GetCurrentTransaction().Save(PERSONAL);
                }
                catch (Exception ex)
                {
                    throw new NomadAppException("Error guardando FAMILIAR en la PERSONA", ex);
                }
            }
        }

    }
}



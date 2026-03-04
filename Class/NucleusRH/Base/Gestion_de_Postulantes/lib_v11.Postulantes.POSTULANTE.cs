using System;
using System.Xml;
using System.IO;
using System.Collections;

using Nomad.NSystem.Document;
using Nomad.NSystem.Proxy;
using Nomad.NSystem.Base;
using Nomad.NSystem.Functions;

namespace NucleusRH.Base.Gestion_de_Postulantes.Postulantes
{

    //////////////////////////////////////////////////////////////////////////////////
    //Clase Postulantes
    public partial class POSTULANTE 
    {
        public void CrearLegajo(string oi_empresa, string oi_puesto, string oi_posicion, DateTime f_ingreso, int e_numero_legajo, string oi_tipo_personal, string oi_ctro_costo, string oi_calendario, string o_personal_emp)
        {

            NomadEnvironment.GetTrace().Info("CREA LA PERSONA");

            //CREO LA PERSONA
            NucleusRH.Base.Personal.Legajo.PERSONAL DDOPER = new NucleusRH.Base.Personal.Legajo.PERSONAL();
            DDOPER.c_personal = "DNI" + this.c_dni;
            DDOPER.oi_tipo_documento = "1";
            DDOPER.d_apellido = this.d_apellido;
            DDOPER.d_nombres = this.d_nombre;
            DDOPER.d_ape_y_nom = this.d_postulante;
            DDOPER.d_apellido_materno = this.d_apellido_materno;
            DDOPER.c_nro_documento = this.c_dni;
            DDOPER.c_sexo = this.c_sexo;
            DDOPER.f_nacim = this.f_nacim;
            DDOPER.f_casamiento = this.f_casamiento;
            DDOPER.c_nro_cuil = this.c_nro_cuil;
            DDOPER.f_ingreso_pais = this.f_ingreso_pais;
            DDOPER.c_nro_camisa = this.c_nro_camisa;
            DDOPER.c_nro_pantalon = this.c_nro_pantalon;
            DDOPER.c_nro_calzado = this.c_nro_calzado;
            DDOPER.l_jubilado = this.l_jubilado_;
            DDOPER.f_estado_jubi = this.f_estado_jubi;
            DDOPER.c_nro_jubilacion = this.c_nro_jubilacion;
            DDOPER.f_desde_afjpNull = true;
            DDOPER.d_email = this.d_email;
            DDOPER.te_celular = this.te_celular;
            DDOPER.oi_grupo_sanguineo = this.oi_grupo_sanguineo;
            DDOPER.oi_local_nacim = this.oi_local_nacim;
            DDOPER.oi_nacionalidad = this.oi_nacionalidad;
            DDOPER.oi_idioma = this.oi_idioma;
            DDOPER.oi_estado_civil = this.oi_estado_civil;
            DDOPER.oi_titulo = this.oi_titulo;
            DDOPER.oi_estado_jubi = this.oi_estado_jubi;
            DDOPER.oi_calificacion = this.oi_calificacion;
            DDOPER.oi_localidad = this.oi_localidad;
            DDOPER.oi_afjp = this.oi_afjp;
            DDOPER.oi_foto = this.oi_foto;
            DDOPER.oi_firma = this.oi_firma;
            DDOPER.o_personal = o_personal_emp;

            //CURSOS DE LA PERSONA
            NomadEnvironment.GetTrace().Info("CURSOS");
            foreach (CURSO_EXT_POS CURPOS in this.CURSOS_POS)
            {
                NucleusRH.Base.Personal.Legajo.CURSO_EXT_PER CURPER = new NucleusRH.Base.Personal.Legajo.CURSO_EXT_PER();
                CURPER.d_curso_ext_per = CURPOS.d_curso_ext_pos;
                CURPER.f_fin_curso = CURPOS.f_fin_curso;
                CURPER.n_duracion = CURPOS.n_duracion;
                CURPER.d_lugar = CURPOS.d_lugar;
                CURPER.l_certificado = CURPOS.l_certificado;
                CURPER.l_externo = CURPOS.l_externo;
                CURPER.oi_curso = CURPOS.oi_curso;
                CURPER.oi_unidad_tiempo = CURPOS.oi_unidad_tiempo;

                DDOPER.CURSOS_PER.Add(CURPER);
            }

            //DOMICILIOS DE LA PERSONA
            NomadEnvironment.GetTrace().Info("DOMICILIOS");
            foreach (DOMIC_POS DOMPOS in this.DOMIC_POS)
            {
                NucleusRH.Base.Personal.Legajo.DOMIC_PER DOMPER = new NucleusRH.Base.Personal.Legajo.DOMIC_PER();

                DOMPER.c_postal = DOMPOS.c_postal;
                DOMPER.d_calle = DOMPOS.d_calle;
                DOMPER.e_numero = DOMPOS.e_numero;
                DOMPER.d_piso = DOMPOS.d_piso;
                DOMPER.d_departamento = DOMPOS.d_departamento;
                DOMPER.d_entre_calle_1 = DOMPOS.d_entre_calle_1;
                DOMPER.d_entre_calle_2 = DOMPOS.d_entre_calle_2;
                DOMPER.te_1 = DOMPOS.te_1;
                DOMPER.te_2 = DOMPOS.te_2;
                DOMPER.te_celular = DOMPOS.te_celular;
                DOMPER.d_email = DOMPOS.d_email;
                DOMPER.d_partido = DOMPOS.d_partido;
                DOMPER.l_domic_fiscal = DOMPOS.l_domic_fiscal;
                DOMPER.oi_tipo_domicilio = DOMPOS.oi_tipo_domicilio;
                DOMPER.oi_localidad = DOMPOS.oi_localidad;

                DDOPER.DOMIC_PER.Add(DOMPER);
            }

            //ESTUDIOS DE LA PERSONA
            NomadEnvironment.GetTrace().Info("ESTUDIOS");
            foreach (ESTUDIO_POS ESTPOS in this.ESTUDIOS_POS)
            {
                NucleusRH.Base.Personal.Legajo.ESTUDIO_PER ESTPER = new NucleusRH.Base.Personal.Legajo.ESTUDIO_PER();

                ESTPER.oi_area_est = ESTPOS.oi_area_est;
                ESTPER.f_ini_estudio = ESTPOS.f_ini_estudio;
                ESTPER.f_fin_estudio = ESTPOS.f_fin_estudio;
                ESTPER.oi_pais = ESTPOS.oi_pais;
                ESTPER.l_est_extranjero = ESTPOS.l_est_extranjero;
                //ESTPER.oi_est_educ = ESTPOS.oi_est_educ;
                ESTPER.d_otro_est_educ = ESTPOS.d_otro_est_educ;
                ESTPER.e_duracion_estudio = ESTPOS.e_duracion_estudio;
                ESTPER.e_periodo_en_curso = ESTPOS.e_periodo_en_curso;
                ESTPER.e_mat_adeudadas = ESTPOS.e_mat_adeudadas;
                ESTPER.f_actualiz = ESTPOS.f_actualiz;
                ESTPER.n_promedio = ESTPOS.n_promedio;
                ESTPER.oi_estudio = ESTPOS.oi_estudio;
                ESTPER.oi_nivel_estudio = ESTPOS.oi_nivel_estudio;
                ESTPER.oi_tipo_establecim = ESTPOS.oi_tipo_establecim;
                ESTPER.oi_estado_est = ESTPOS.oi_estado_est;
                ESTPER.oi_unidad_tiempo = ESTPOS.oi_unidad_tiempo;

                DDOPER.ESTUDIOS_PER.Add(ESTPER);
            }

            NomadEnvironment.GetTrace().Info("FAMILIARES");
            //FAMILIARES DE LA PERSONA
            foreach (FAMILIAR_POS FAMPOS in this.FLIARES_POS)
            {
                NucleusRH.Base.Personal.Legajo.FAMILIAR_PER FAMPER = new NucleusRH.Base.Personal.Legajo.FAMILIAR_PER();

                FAMPER.c_nro_documento = FAMPOS.c_nro_documento;
                FAMPER.d_apellido = FAMPOS.d_apellido;
                FAMPER.d_nombres = FAMPOS.d_nombres;
                FAMPER.d_ape_y_nom = FAMPOS.d_familiar_pos;
                FAMPER.f_nacimiento = FAMPOS.f_nacimiento;
                FAMPER.l_vive = FAMPOS.l_vive;
                FAMPER.f_fallecimiento = FAMPOS.f_fallecimiento;
                FAMPER.l_discapacidad = FAMPOS.l_discapacidad;
                FAMPER.e_duracion_estudio = FAMPOS.e_duracion_estudio;
                FAMPER.e_periodo_en_curso = FAMPOS.e_periodo_en_curso;
                FAMPER.e_anio_inic_esc = FAMPOS.e_anio_inic_esc;
                FAMPER.e_anio_fin_esc = FAMPOS.e_anio_fin_esc;
                FAMPER.c_nro_cuil = FAMPOS.c_nro_cuil;
                FAMPER.c_sexo = FAMPOS.c_sexo;
                FAMPER.l_reside_pais = FAMPOS.l_reside_pais;
                FAMPER.l_acargo_af = FAMPOS.l_acargo_af;
                FAMPER.l_acargo_os = FAMPOS.l_acargo_os;
                FAMPER.l_acargo_IG = FAMPOS.l_acargo_IG;
                FAMPER.f_desde_IG = FAMPOS.f_desde_IG;
                FAMPER.f_hasta_IG = FAMPOS.f_hasta_IG;
                FAMPER.c_nro_seguro_soc = FAMPOS.c_nro_seguro_soc;
                FAMPER.d_resp_exp_doc = FAMPOS.d_resp_exp_doc;
                FAMPER.e_sit_fam_presta = FAMPOS.e_sit_fam_presta;
                FAMPER.c_nro_prestadora = FAMPOS.c_nro_prestadora;
                FAMPER.n_porc_prestadora = FAMPOS.n_porc_prestadora;
                FAMPER.oi_tipo_documento = FAMPOS.oi_tipo_documento;
                FAMPER.oi_tipo_familiar = FAMPOS.oi_tipo_familiar;
                FAMPER.oi_ocupacion_fam = FAMPOS.oi_ocupacion_fam;
                FAMPER.oi_nivel_escol = FAMPOS.oi_nivel_escol;
                FAMPER.oi_grado_escol = FAMPOS.oi_grado_escol;
                FAMPER.oi_estudio = FAMPOS.oi_estudio;
                FAMPER.oi_unidad_tiempo = FAMPOS.oi_unidad_tiempo;
                FAMPER.oi_estado_civil = FAMPOS.oi_estado_civil;
                FAMPER.oi_nacionalidad = FAMPOS.oi_nacionalidad;

                DDOPER.FLIARES_PER.Add(FAMPER);
            }

            //IDIOMAS DE LA PERSONA                     
            NomadEnvironment.GetTrace().Info("IDIOMAS");
            foreach (IDIOMA_POS IDIPOS in this.IDIOMAS_POS)
            {
                NucleusRH.Base.Personal.Legajo.IDIOMA_PER IDIPER = new NucleusRH.Base.Personal.Legajo.IDIOMA_PER();

                IDIPER.l_certificado = IDIPOS.l_certificado;
                IDIPER.d_certificado = IDIPOS.d_certificado;
                IDIPER.e_tiempo_cursado = IDIPOS.e_tiempo_cursado;
                IDIPER.d_lugar_cursado = IDIPOS.d_lugar_cursado;
                IDIPER.oi_idioma = IDIPOS.oi_idioma;
                IDIPER.oi_nivel_habla = IDIPOS.oi_nivel_habla;
                IDIPER.oi_nivel_lee = IDIPOS.oi_nivel_lee;
                IDIPER.oi_nivel_escribe = IDIPOS.oi_nivel_escribe;
                IDIPER.oi_unidad_tiempo = IDIPOS.oi_unidad_tiempo;

                DDOPER.IDIOMAS_PER.Add(IDIPER);
            }

            //ANTECEDENTES DE LA PERSONA
            NomadEnvironment.GetTrace().Info("ANTECEDENTES");
            foreach (ANTECEDENTE ANTPOS in this.ANTECEDENTES)
            {
                NucleusRH.Base.Personal.Legajo.ANTECEDENTE ANTPER = new NucleusRH.Base.Personal.Legajo.ANTECEDENTE();

                ANTPER.f_ingreso = ANTPOS.f_ingreso;
                ANTPER.f_egreso = ANTPOS.f_egreso;
                ANTPER.d_empresa = ANTPOS.d_empresa;
                ANTPER.d_domic_empresa = ANTPOS.d_dom_empresa;
                ANTPER.te_empresa = ANTPOS.te_empresa;
                ANTPER.d_nombre_ult_sup = ANTPOS.d_nombre_ult_sup;
                ANTPER.d_cargo_ult_sup = ANTPOS.d_cargo_ult_sup;
                ANTPER.e_per_cargo_ult = ANTPOS.e_per_cargo_ult;
                ANTPER.n_ultimo_sueldo = ANTPOS.n_ultimo_sueldo;
                ANTPER.oi_motivo_eg_per = ANTPOS.oi_motivo_eg_per;

                DDOPER.ANTECEDENTES.Add(ANTPER);
            }

            //COMPETENCIAS DE LA PERSONA
            NomadEnvironment.GetTrace().Info("COMPETENCIAS");
            foreach (COMPETENCIA_POS COMPOS in this.COMPET_POS)
            {
                NucleusRH.Base.Personal.Legajo.COMPETENCIA_PER COMPER = new NucleusRH.Base.Personal.Legajo.COMPETENCIA_PER();

                COMPER.oi_competencia = COMPOS.oi_competencia;
                COMPER.oi_nivel_comp = COMPOS.oi_nivel_comp;

                DDOPER.COMPET_PER.Add(COMPER);
            }

            //DOCUMENTOS DIGITALES DE LA PERSONA
            NomadEnvironment.GetTrace().Info("DOCUMENTOS DIGITALES");
            foreach (DOCUM_DIG_POS DGPOS in this.DOC_DIG_POS)
            {
                NucleusRH.Base.Personal.Legajo.DOCUM_DIG_PER DGPER = new NucleusRH.Base.Personal.Legajo.DOCUM_DIG_PER();

                DGPER.d_docum_dig_per = DGPOS.d_docum_dig_pos;
                DGPER.o_docum_dig_per = DGPOS.o_docum_dig_pos;
                DGPER.oi_imagen_per = DGPOS.oi_doc_digital;

                DDOPER.DOC_DIG_PER.Add(DGPER);
            }

            //DOCUMENTO
            NucleusRH.Base.Personal.Legajo.DOCUMENTO_PER DOCPER = new NucleusRH.Base.Personal.Legajo.DOCUMENTO_PER();

            DOCPER.c_documento = this.c_dni;
            DOCPER.oi_tipo_documento = "1";

            DDOPER.DOCUM_PER.Add(DOCPER);

            //GRABO LA PERSONA
            NomadEnvironment.GetTrace().Info("PERSONA -- " + DDOPER.SerializeAll());
            NomadEnvironment.GetCurrentTransaction().SaveRefresh(DDOPER);

            //CREO EL LEGAJO          
            NomadEnvironment.GetTrace().Info("CREO EL LEGAJO");
            NucleusRH.Base.Personal.LegajoEmpresa.PERSONAL_EMP DDOLEG = new NucleusRH.Base.Personal.LegajoEmpresa.PERSONAL_EMP();
            DDOLEG.oi_empresa = oi_empresa;
            DDOLEG.oi_personal = DDOPER.Id;
            DDOLEG.e_numero_legajo = e_numero_legajo;
            DDOLEG.oi_puesto_ult = oi_puesto;
            DDOLEG.f_desde_puesto = f_ingreso;
            DDOLEG.oi_posicion_ult = oi_posicion;
            DDOLEG.f_desde_posicion = f_ingreso;
            DDOLEG.f_ingreso = f_ingreso;
            DDOLEG.oi_tipo_personal = oi_tipo_personal;
            DDOLEG.oi_ctro_costo_ult = oi_ctro_costo;
            DDOLEG.f_desde_ccosto = f_ingreso;
            DDOLEG.oi_calendario_ult = oi_calendario;
            DDOLEG.oi_indic_activo = "1";
            DDOLEG.f_desde_calendario = f_ingreso;
            DDOLEG.o_personal_emp = o_personal_emp;

            //TRAYECTORIA DE PUESTO
            NomadEnvironment.GetTrace().Info("PUESTO");
            NucleusRH.Base.Personal.LegajoEmpresa.PUESTO_PER PUELEG = new NucleusRH.Base.Personal.LegajoEmpresa.PUESTO_PER();
            PUELEG.oi_puesto = oi_puesto;
            PUELEG.f_ingreso = f_ingreso;

            DDOLEG.PUESTO_PER.Add(PUELEG);

            //TRAYECTORIA DE CALENDARIO
            NomadEnvironment.GetTrace().Info("CALENDARIO");
            NucleusRH.Base.Personal.LegajoEmpresa.CALENDARIO_PER CALLEG = new NucleusRH.Base.Personal.LegajoEmpresa.CALENDARIO_PER();
            CALLEG.oi_calendario = oi_calendario;
            CALLEG.f_desde = f_ingreso;

            DDOLEG.CALENDARIO_PER.Add(CALLEG);

            //TRAYECTORIA DE TIPO PERSONAL
            NomadEnvironment.GetTrace().Info("TIPO PERSONAL");
            NucleusRH.Base.Personal.LegajoEmpresa.TIPOP_PER TPLEG = new NucleusRH.Base.Personal.LegajoEmpresa.TIPOP_PER();
            TPLEG.oi_tipo_personal = oi_tipo_personal;
            TPLEG.f_ingreso = f_ingreso;

            DDOLEG.TIPOSP_PER.Add(TPLEG);

            //TRAYECTORIA DE INGRESOS
            NomadEnvironment.GetTrace().Info("INGRESOS");
            NucleusRH.Base.Personal.LegajoEmpresa.INGRESOS_PER INGLEG = new NucleusRH.Base.Personal.LegajoEmpresa.INGRESOS_PER();
            INGLEG.f_ingreso = f_ingreso;

            DDOLEG.INGRESOS_PER.Add(INGLEG);

            //TRAYECTORIA DE CENTRO DE COSTOS
            NomadEnvironment.GetTrace().Info("CENTRO COSTO");
            NucleusRH.Base.Personal.LegajoEmpresa.CCOSTO_PER CCLEG = new NucleusRH.Base.Personal.LegajoEmpresa.CCOSTO_PER();
            CCLEG.oi_centro_costo = oi_ctro_costo;
            CCLEG.f_ingreso = f_ingreso;

            DDOLEG.CCOSTO_PER.Add(CCLEG);


            //GRABO EL LEGAJO
            NomadEnvironment.GetTrace().Info("LEGAJO -- " + DDOLEG.SerializeAll());
            NomadEnvironment.GetCurrentTransaction().SaveRefresh(DDOLEG);

            //POSICION
            NomadEnvironment.GetTrace().Info("POSICION");
            NucleusRH.Base.Personal.LegajoEmpresa.POSICION_PER DDOPOSICLEG = new NucleusRH.Base.Personal.LegajoEmpresa.POSICION_PER();
            DDOPOSICLEG.f_ingreso = f_ingreso;
            DDOPOSICLEG.oi_posicion = oi_posicion;
            DDOLEG.Asignar_Posicion(DDOPOSICLEG);

        }
    }
}

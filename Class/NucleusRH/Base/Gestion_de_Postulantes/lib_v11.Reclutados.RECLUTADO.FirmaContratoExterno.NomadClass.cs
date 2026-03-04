using System;
using System.IO;
using System.Collections;

using Nomad.NSystem.Document;
using Nomad.NSystem.Proxy;
using Nomad.NSystem.Base;
using Nomad.NSystem.Functions;

namespace NucleusRH.Base.Gestion_de_Postulantes.Reclutados
{
    public partial class RECLUTADO
  {

      public static void Asignar_Datos_Pos(NucleusRH.Base.Gestion_de_Postulantes.Postulantes.POSTULANTE ddoPos, NucleusRH.Base.Personal.Legajo.PERSONAL ddoPer)
      {
          ddoPer.d_ape_y_nom = ddoPos.d_postulante;
          ddoPer.d_apellido = ddoPos.d_apellido;
          ddoPer.c_sexo = ddoPos.c_sexo;
          ddoPer.l_jubilado = ddoPos.l_jubilado_;
          ddoPer.c_nro_calzado = ddoPos.c_nro_calzado;
          ddoPer.c_nro_documento = ddoPos.c_dni;
          ddoPer.c_nro_camisa = ddoPos.c_nro_camisa;
          ddoPer.c_nro_pantalon = ddoPos.c_nro_pantalon;
          ddoPer.d_nombres = ddoPos.d_nombre;
          ddoPer.te_celular = ddoPos.te_celular;
          ddoPer.o_personal = ddoPos.o_postulante;
          ddoPer.f_nacim = ddoPos.f_nacim;
          ddoPer.c_nro_cuil = ddoPos.c_nro_cuil;
          ddoPer.d_email = ddoPos.d_email;
          ddoPer.oi_foto = ddoPos.oi_foto;
          ddoPer.oi_firma = ddoPos.oi_firma;
          ddoPer.oi_idioma = ddoPos.oi_idioma;
          ddoPer.oi_grupo_sanguineo = ddoPos.oi_grupo_sanguineo;
          ddoPer.oi_estado_civil = ddoPos.oi_estado_civil;
          ddoPer.oi_afjp = ddoPos.oi_afjp;
          ddoPer.oi_estado_jubi = ddoPos.oi_estado_jubi;
          ddoPer.oi_local_nacim = ddoPos.oi_local_nacim;
          ddoPer.oi_localidad = ddoPos.oi_localidad;
          ddoPer.oi_nacionalidad = ddoPos.oi_nacionalidad;
          ddoPer.oi_titulo = ddoPos.oi_titulo;

          //EMPIEZO A ASIGNAR CADA UNO DE LOS HIJOS DEL POSTULANTE AL PERSONAL

          //DOC_DIG_POS. LOS RECORRO.
          foreach(NucleusRH.Base.Gestion_de_Postulantes.Postulantes.DOCUM_DIG_POS ddoDocDigPos in ddoPos.DOC_DIG_POS)
          {
            //CREO UN NUEVO DOCUMENTO DIGITAL DE LA PERSONA
            NucleusRH.Base.Personal.Legajo.DOCUM_DIG_PER ddoDocDigPer;
            ddoDocDigPer = new NucleusRH.Base.Personal.Legajo.DOCUM_DIG_PER();
            //ASIGNO LOS ATRIBUTOS CORRESPONDIENTES
            NucleusRH.Base.Gestion_de_Postulantes.Reclutados.RECLUTADO.Asignar_Doc_Dig_Pos(ddoDocDigPos, ddoDocDigPer);
            //AGREGO EL HIJO AL PADRE
            ddoPer.DOC_DIG_PER.Add(ddoDocDigPer);
          }

          //ANTECEDENTES. LOS RECORRO.
          foreach(NucleusRH.Base.Gestion_de_Postulantes.Postulantes.ANTECEDENTE ddoAntecPos in ddoPos.ANTECEDENTES)
          {
            //CREO UN NUEVO DOCUMENTO DIGITAL DE LA PERSONA
            NucleusRH.Base.Personal.Legajo.ANTECEDENTE ddoAntecPer;
            ddoAntecPer = new NucleusRH.Base.Personal.Legajo.ANTECEDENTE();
            //ASIGNO LOS ATRIBUTOS CORRESPONDIENTES
            NucleusRH.Base.Gestion_de_Postulantes.Reclutados.RECLUTADO.Asignar_Antecedente_Pos(ddoAntecPos, ddoAntecPer);
            //RECORRO LOS ANTECENDENTES POR PUESTO ASOCIADOS AL ANTECENDENTE DEL POSTULANTE
            foreach(NucleusRH.Base.Gestion_de_Postulantes.Postulantes.ANTEC_PUESTO ddoAntecPuesPos in ddoAntecPos.ANTEC_PUESTO)
            {
              //CREO UN NUEVO ANTECEDENTE POR PUESTO PARA LA PERSONA
               NucleusRH.Base.Personal.Legajo.ANTEC_PUESTO ddoAntecPuesPer;
               ddoAntecPuesPer = new NucleusRH.Base.Personal.Legajo.ANTEC_PUESTO();
               //ASIGNO LOS ATRIBUTOS CORRESPONDIENTES
               NucleusRH.Base.Gestion_de_Postulantes.Reclutados.RECLUTADO.Asignar_Antec_Puesto_Pos(ddoAntecPuesPos, ddoAntecPuesPer);
               //AGREGO EL HIJO AL PADRE
               ddoAntecPer.ANTEC_PUESTO.Add(ddoAntecPuesPer);
            }
            //AGREGO EL HIJO AL PADRE
            ddoPer.ANTECEDENTES.Add(ddoAntecPer);
          }

          //ESTUDIOS_PER. LOS RECORRO.
          foreach(NucleusRH.Base.Gestion_de_Postulantes.Postulantes.ESTUDIO_POS ddoEstPos in ddoPos.ESTUDIOS_POS)
          {
            //CREO UN NUEVO ESTUDIO DE LA PERSONA
            NucleusRH.Base.Personal.Legajo.ESTUDIO_PER ddoEstPer;
            ddoEstPer = new NucleusRH.Base.Personal.Legajo.ESTUDIO_PER();
            //ASIGNO LOS ATRIBUTOS CORRESPONDIENTES
            NucleusRH.Base.Gestion_de_Postulantes.Reclutados.RECLUTADO.Asignar_Estudios_Pos(ddoEstPos, ddoEstPer);
            //AGREGO EL HIJO AL PADRE
            ddoPer.ESTUDIOS_PER.Add(ddoEstPer);
          }

          //FAMILIARES_PER. LOS RECORRO.
          foreach(NucleusRH.Base.Gestion_de_Postulantes.Postulantes.FAMILIAR_POS ddoFamPos in ddoPos.FLIARES_POS)
          {
            //CREO UN NUEVO FAMILIAR DE LA PERSONA
            NucleusRH.Base.Personal.Legajo.FAMILIAR_PER ddoFamPer;
            ddoFamPer = new NucleusRH.Base.Personal.Legajo.FAMILIAR_PER();
            //ASIGNO LOS ATRIBUTOS CORRESPONDIENTES
            NucleusRH.Base.Gestion_de_Postulantes.Reclutados.RECLUTADO.Asignar_Familiares_Pos(ddoFamPos, ddoFamPer);
            //AGREGO EL HIJO AL PADRE
            ddoPer.FLIARES_PER.Add(ddoFamPer);
          }

          //AFJP_PER. LOS RECORRO.
          foreach(NucleusRH.Base.Gestion_de_Postulantes.Postulantes.AFJP_POS ddoAfjpPos in ddoPos.AFJP_POS)
          {
            //CREO UNA NUEVA AFJP DE LA PERSONA
            NucleusRH.Base.Personal.Legajo.AFJP_PER ddoAfjpPer;
            ddoAfjpPer = new NucleusRH.Base.Personal.Legajo.AFJP_PER();
            //ASIGNO LOS ATRIBUTOS CORRESPONDIENTES
            NucleusRH.Base.Gestion_de_Postulantes.Reclutados.RECLUTADO.Asignar_Afjp_Pos(ddoAfjpPos, ddoAfjpPer);
            //AGREGO EL HIJO AL PADRE
            ddoPer.AFJP_PER.Add(ddoAfjpPer);
          }

          //CURSO_EXT_PER. LOS RECORRO.
          foreach(NucleusRH.Base.Gestion_de_Postulantes.Postulantes.CURSO_EXT_POS ddoCurPos in ddoPos.CURSOS_POS)
          {
            //CREO UN NUEVO CURSO DE LA PERSONA
            NucleusRH.Base.Personal.Legajo.CURSO_EXT_PER ddoCurPer;
            ddoCurPer = new NucleusRH.Base.Personal.Legajo.CURSO_EXT_PER();
            //ASIGNO LOS ATRIBUTOS CORRESPONDIENTES
            NucleusRH.Base.Gestion_de_Postulantes.Reclutados.RECLUTADO.Asignar_Curso_Ext_Pos(ddoCurPos, ddoCurPer);
            //AGREGO EL HIJO AL PADRE
            ddoPer.CURSOS_PER.Add(ddoCurPer);
          }

          //IDIOMAS_PER. LOS RECORRO.
          foreach(NucleusRH.Base.Gestion_de_Postulantes.Postulantes.IDIOMA_POS ddoIdiPos in ddoPos.IDIOMAS_POS)
          {
            //CREO UN NUEVO IDIOMA DE LA PERSONA
            NucleusRH.Base.Personal.Legajo.IDIOMA_PER ddoIdiPer;
            ddoIdiPer = new NucleusRH.Base.Personal.Legajo.IDIOMA_PER();
            //ASIGNO LOS ATRIBUTOS CORRESPONDIENTES
            NucleusRH.Base.Gestion_de_Postulantes.Reclutados.RECLUTADO.Asignar_Idi_Pos(ddoIdiPos, ddoIdiPer);
            //AGREGO EL HIJO AL PADRE
            ddoPer.IDIOMAS_PER.Add(ddoIdiPer);
          }

          //COMPET_PER. LOS RECORRO.
          foreach(NucleusRH.Base.Gestion_de_Postulantes.Postulantes.COMPETENCIA_POS ddoComPos in ddoPos.COMPET_POS)
          {
            //CREO UNA NUEVA COMPETENCIA DE LA PERSONA
            NucleusRH.Base.Personal.Legajo.COMPETENCIA_PER ddoComPer;
            ddoComPer = new NucleusRH.Base.Personal.Legajo.COMPETENCIA_PER();
            //ASIGNO LOS ATRIBUTOS CORRESPONDIENTES
            NucleusRH.Base.Gestion_de_Postulantes.Reclutados.RECLUTADO.Asignar_Compet_Pos(ddoComPos, ddoComPer);
            //AGREGO EL HIJO AL PADRE
            ddoPer.COMPET_PER.Add(ddoComPer);
          }

          //DOCUM_PER. LOS RECORRO.
          foreach(NucleusRH.Base.Gestion_de_Postulantes.Postulantes.DOCUMENTO_POS ddoDocPos in ddoPos.DOCUM_POS)
          {
            //CREO UN NUEVO DOCUMENTO DE LA PERSONA
            NucleusRH.Base.Personal.Legajo.DOCUMENTO_PER ddoDocPer;
            ddoDocPer = new NucleusRH.Base.Personal.Legajo.DOCUMENTO_PER();
            //ASIGNO LOS ATRIBUTOS CORRESPONDIENTES
            NucleusRH.Base.Gestion_de_Postulantes.Reclutados.RECLUTADO.Asignar_Doc_Pos(ddoDocPos, ddoDocPer);
            //AGREGO EL HIJO AL PADRE
            ddoPer.DOCUM_PER.Add(ddoDocPer);
          }

          //DOMIC_PER. LOS RECORRO.
          foreach(NucleusRH.Base.Gestion_de_Postulantes.Postulantes.DOMIC_POS ddoDomPos in ddoPos.DOMIC_POS)
          {
            //CREO UN NUEVO DOCUMENTO DE LA PERSONA
            NucleusRH.Base.Personal.Legajo.DOMIC_PER ddoDomPer;
            ddoDomPer = new NucleusRH.Base.Personal.Legajo.DOMIC_PER();
            //ASIGNO LOS ATRIBUTOS CORRESPONDIENTES
            NucleusRH.Base.Gestion_de_Postulantes.Reclutados.RECLUTADO.Asignar_Domicilio_Pos(ddoDomPos, ddoDomPer);
            //AGREGO EL HIJO AL PADRE
            ddoPer.DOMIC_PER.Add(ddoDomPer);
          }

      }

      public static void Asignar_Doc_Dig_Pos(NucleusRH.Base.Gestion_de_Postulantes.Postulantes.DOCUM_DIG_POS ddoDocDigPos, NucleusRH.Base.Personal.Legajo.DOCUM_DIG_PER ddoDocDigPer)
      {
          ddoDocDigPer.d_docum_dig_per = ddoDocDigPos.d_docum_dig_pos;
          ddoDocDigPer.o_docum_dig_per = ddoDocDigPos.o_docum_dig_pos;
          ddoDocDigPer.oi_imagen_per = ddoDocDigPos.oi_doc_digital;
      }

      public static void Asignar_Antecedente_Pos(NucleusRH.Base.Gestion_de_Postulantes.Postulantes.ANTECEDENTE ddoAntecPos, NucleusRH.Base.Personal.Legajo.ANTECEDENTE ddoAntecPer)
      {
          ddoAntecPer.n_ultimo_sueldo = ddoAntecPos.n_ultimo_sueldo;
          ddoAntecPer.d_nombre_ult_sup = ddoAntecPos.d_nombre_ult_sup;
          ddoAntecPer.te_empresa = ddoAntecPos.te_empresa;
          ddoAntecPer.d_domic_empresa = ddoAntecPos.d_dom_empresa;
          ddoAntecPer.d_empresa = ddoAntecPos.d_empresa;
          ddoAntecPer.f_egreso = ddoAntecPos.f_egreso;
          ddoAntecPer.f_ingreso = ddoAntecPos.f_ingreso;
          //ddoAntecPer.c_antecedente = ddoAntecPos.c_antecedente;
          ddoAntecPer.d_cargo_ult_sup = ddoAntecPos.d_cargo_ult_sup;
          ddoAntecPer.e_per_cargo_ult = ddoAntecPos.e_per_cargo_ult;
          ddoAntecPer.o_antecedente = ddoAntecPos.o_antecedente;
          ddoAntecPer.oi_motivo_eg_per = ddoAntecPos.oi_motivo_eg_per;
      }

      public static void Asignar_Antec_Puesto_Pos(NucleusRH.Base.Gestion_de_Postulantes.Postulantes.ANTEC_PUESTO ddoAntecPuesPos, NucleusRH.Base.Personal.Legajo.ANTEC_PUESTO ddoAntecPuesPer)
      {
           ddoAntecPuesPer.d_puesto = ddoAntecPuesPos.d_puesto;
           ddoAntecPuesPer.e_experiencia = ddoAntecPuesPos.e_experiencia;
           ddoAntecPuesPer.o_tarea_desemp = ddoAntecPuesPos.o_tarea_desemp;
           ddoAntecPuesPer.oi_antecedente = ddoAntecPuesPos.oi_antecedente;
           ddoAntecPuesPer.oi_puesto_empresa = ddoAntecPuesPos.oi_puesto_empresa;
           ddoAntecPuesPer.oi_unidad_tiempo = ddoAntecPuesPos.oi_unidad_tiempo;
      }

      public static void Asignar_Estudios_Pos(NucleusRH.Base.Gestion_de_Postulantes.Postulantes.ESTUDIO_POS ddoEstPos, NucleusRH.Base.Personal.Legajo.ESTUDIO_PER ddoEstPer)
      {
          ddoEstPer.oi_area_est = ddoEstPos.oi_area_est;
          ddoEstPer.f_ini_estudio = ddoEstPos.f_ini_estudio;
          ddoEstPer.f_fin_estudio = ddoEstPos.f_fin_estudio;
          ddoEstPer.oi_pais = ddoEstPos.oi_pais;
          ddoEstPer.l_est_extranjero = ddoEstPos.l_est_extranjero;
          //ddoEstPer.oi_est_educ = ddoEstPos.oi_est_educ;
          ddoEstPer.d_otro_est_educ = ddoEstPos.d_otro_est_educ;
          ddoEstPer.e_duracion_estudio = ddoEstPos.e_duracion_estudio;
          ddoEstPer.e_periodo_en_curso = ddoEstPos.e_periodo_en_curso;
          ddoEstPer.e_mat_adeudadas = ddoEstPos.e_mat_adeudadas;
          ddoEstPer.f_actualiz = ddoEstPos.f_actualiz;
          ddoEstPer.n_promedio = ddoEstPos.n_promedio;
          ddoEstPer.oi_estudio = ddoEstPos.oi_estudio;
          ddoEstPer.oi_nivel_estudio = ddoEstPos.oi_nivel_estudio;
          ddoEstPer.oi_tipo_establecim = ddoEstPos.oi_tipo_establecim;
          ddoEstPer.oi_estado_est = ddoEstPos.oi_estado_est;
          ddoEstPer.oi_unidad_tiempo= ddoEstPos.oi_unidad_tiempo;
      }

      public static void Asignar_Familiares_Pos(NucleusRH.Base.Gestion_de_Postulantes.Postulantes.FAMILIAR_POS ddoFamPos, NucleusRH.Base.Personal.Legajo.FAMILIAR_PER ddoFamPer)
      {
          //ddoFamPer.c_familiar = ddoFamPos.c_familiar_pos;
          ddoFamPer.c_nro_cuil = ddoFamPos.c_nro_cuil;
          ddoFamPer.c_nro_documento = ddoFamPos.c_nro_documento;
          ddoFamPer.c_nro_prestadora = ddoFamPos.c_nro_prestadora;
          ddoFamPer.c_nro_seguro_soc = ddoFamPos.c_nro_seguro_soc;
          ddoFamPer.c_sexo = ddoFamPos.c_sexo;
          ddoFamPer.d_ape_y_nom = ddoFamPos.d_familiar_pos;
          ddoFamPer.d_apellido = ddoFamPos.d_apellido;
          ddoFamPer.d_nombres = ddoFamPos.d_nombres;
          ddoFamPer.d_resp_exp_doc = ddoFamPos.d_resp_exp_doc;
          ddoFamPer.e_anio_fin_esc = ddoFamPos.e_anio_fin_esc;
          ddoFamPer.e_anio_inic_esc = ddoFamPos.e_anio_inic_esc;
          ddoFamPer.e_duracion_estudio = ddoFamPos.e_duracion_estudio;
          ddoFamPer.e_periodo_en_curso = ddoFamPos.e_periodo_en_curso;
          ddoFamPer.e_sit_fam_presta = ddoFamPos.e_sit_fam_presta;
          ddoFamPer.f_desde_IG = ddoFamPos.f_desde_IG;
          ddoFamPer.f_fallecimiento = ddoFamPos.f_fallecimiento;
          ddoFamPer.f_hasta_IG = ddoFamPos.f_hasta_IG;
          ddoFamPer.f_nacimiento = ddoFamPos.f_nacimiento;
          ddoFamPer.l_acargo_af = ddoFamPos.l_acargo_af;
          ddoFamPer.l_acargo_IG = ddoFamPos.l_acargo_IG;
          ddoFamPer.l_acargo_os = ddoFamPos.l_acargo_os;
          ddoFamPer.l_discapacidad = ddoFamPos.l_discapacidad;
          ddoFamPer.l_reside_pais = ddoFamPos.l_reside_pais;
          ddoFamPer.l_vive = ddoFamPos.l_vive;
          ddoFamPer.n_porc_prestadora = ddoFamPos.n_porc_prestadora;
          ddoFamPer.o_acargo_af = ddoFamPos.o_acargo_af;
          ddoFamPer.o_acargo_ig = ddoFamPos.o_acargo_ig;
          ddoFamPer.o_acargo_os = ddoFamPos.o_acargo_os;
          ddoFamPer.o_familiar = ddoFamPos.o_familiar;
          ddoFamPer.o_ocupacion = ddoFamPos.o_ocupacion;
          ddoFamPer.oi_estado_civil = ddoFamPos.oi_estado_civil;
          ddoFamPer.oi_estudio = ddoFamPos.oi_estudio;
          ddoFamPer.oi_grado_escol = ddoFamPos.oi_grado_escol;
          ddoFamPer.oi_nacionalidad = ddoFamPos.oi_nacionalidad;
          ddoFamPer.oi_nivel_escol = ddoFamPos.oi_nivel_escol;
          ddoFamPer.oi_ocupacion_fam = ddoFamPos.oi_ocupacion_fam;
          ddoFamPer.oi_tipo_documento = ddoFamPos.oi_tipo_documento;
          ddoFamPer.oi_tipo_familiar = ddoFamPos.oi_tipo_familiar;
          ddoFamPer.oi_unidad_tiempo = ddoFamPos.oi_unidad_tiempo;
      }

      public static void Asignar_Afjp_Pos(NucleusRH.Base.Gestion_de_Postulantes.Postulantes.AFJP_POS ddoAfjpPos, NucleusRH.Base.Personal.Legajo.AFJP_PER ddoAfjpPer)
      {
          ddoAfjpPer.c_jubilacion = ddoAfjpPos.c_jubilacion;
          ddoAfjpPer.f_egreso = ddoAfjpPos.f_egreso;
          ddoAfjpPer.f_ingreso = ddoAfjpPos.f_ingreso;
          ddoAfjpPer.o_egreso_afjp = ddoAfjpPos.o_egreso_afjp;
          ddoAfjpPer.oi_afjp = ddoAfjpPos.oi_afjp;
      }

      public static void Asignar_Curso_Ext_Pos(NucleusRH.Base.Gestion_de_Postulantes.Postulantes.CURSO_EXT_POS ddoCurPos, NucleusRH.Base.Personal.Legajo.CURSO_EXT_PER ddoCurPer)
      {
          //ddoCurPer.c_curso_ext_per = ddoCurPos.c_curso_ext_pos;
          ddoCurPer.d_curso_ext_per = ddoCurPos.d_curso_ext_pos;
          ddoCurPer.d_lugar = ddoCurPos.d_lugar;
          ddoCurPer.n_duracion = ddoCurPos.n_duracion;
          ddoCurPer.f_fin_curso = ddoCurPos.f_fin_curso;
          ddoCurPer.l_certificado = ddoCurPos.l_certificado;
          ddoCurPer.l_externo = ddoCurPos.l_externo;
          ddoCurPer.o_curso = ddoCurPos.o_curso;
          ddoCurPer.oi_curso = ddoCurPos.oi_curso;
          ddoCurPer.oi_unidad_tiempo = ddoCurPos.oi_unidad_tiempo;
      }

      public static void Asignar_Idi_Pos(NucleusRH.Base.Gestion_de_Postulantes.Postulantes.IDIOMA_POS ddoIdiPos, NucleusRH.Base.Personal.Legajo.IDIOMA_PER ddoIdiPer)
      {
          ddoIdiPer.d_certificado = ddoIdiPos.d_certificado;
          ddoIdiPer.d_lugar_cursado = ddoIdiPos.d_lugar_cursado;
          ddoIdiPer.e_tiempo_cursado = ddoIdiPos.e_tiempo_cursado;
          ddoIdiPer.l_certificado = ddoIdiPos.l_certificado;
          ddoIdiPer.o_idioma = ddoIdiPos.o_idioma;
          ddoIdiPer.oi_idioma = ddoIdiPos.oi_idioma;
          ddoIdiPer.oi_nivel_escribe = ddoIdiPos.oi_nivel_escribe;
          ddoIdiPer.oi_nivel_habla = ddoIdiPos.oi_nivel_habla;
          ddoIdiPer.oi_nivel_lee = ddoIdiPos.oi_nivel_lee;
          ddoIdiPer.oi_unidad_tiempo = ddoIdiPos.oi_unidad_tiempo;
      }

      public static void Asignar_Compet_Pos(NucleusRH.Base.Gestion_de_Postulantes.Postulantes.COMPETENCIA_POS ddoComPos,   NucleusRH.Base.Personal.Legajo.COMPETENCIA_PER ddoComPer)
      {
          ddoComPer.oi_nivel_comp = ddoComPos.oi_nivel_comp;
          ddoComPer.o_competencia = ddoComPos.o_competencia;
          ddoComPer.oi_competencia = ddoComPos.oi_competencia;
      }

      public static void Asignar_Doc_Pos(NucleusRH.Base.Gestion_de_Postulantes.Postulantes.DOCUMENTO_POS ddoDocPos, NucleusRH.Base.Personal.Legajo.DOCUMENTO_PER ddoDocPer)
      {
          ddoDocPer.c_documento = ddoDocPos.c_documento;
          ddoDocPer.d_origen_documento = ddoDocPos.d_origen_documento;
          ddoDocPer.d_resp_expedicion = ddoDocPos.d_resp_expedicion;
          ddoDocPer.f_vencimiento_doc = ddoDocPos.f_vencimiento_doc;
          ddoDocPer.o_documento_per = ddoDocPos.o_documento_pos;
          ddoDocPer.oi_provincia = ddoDocPos.oi_provincia;
          ddoDocPer.oi_tipo_documento = ddoDocPos.oi_tipo_documento;
      }

      public static void Asignar_Domicilio_Pos(NucleusRH.Base.Gestion_de_Postulantes.Postulantes.DOMIC_POS ddoDomPos, NucleusRH.Base.Personal.Legajo.DOMIC_PER ddoDomPer)
      {
          ddoDomPer.c_postal = ddoDomPos.c_postal;
          ddoDomPer.d_calle = ddoDomPos.d_calle;
          ddoDomPer.d_departamento = ddoDomPos.d_departamento;
          ddoDomPer.d_email = ddoDomPos.d_email;
          ddoDomPer.d_entre_calle_1 = ddoDomPos.d_entre_calle_1;
          ddoDomPer.d_entre_calle_2 = ddoDomPos.d_entre_calle_2;
          ddoDomPer.d_partido = ddoDomPos.d_partido;
          ddoDomPer.d_piso = ddoDomPos.d_partido;
          ddoDomPer.e_numero = ddoDomPos.e_numero;
          ddoDomPer.l_domic_fiscal = ddoDomPos.l_domic_fiscal;
          ddoDomPer.o_domicilio = ddoDomPos.o_domicilio;
          ddoDomPer.oi_localidad = ddoDomPos.oi_localidad;
          ddoDomPer.oi_tipo_domicilio = ddoDomPos.oi_tipo_domicilio;
          ddoDomPer.te_1 = ddoDomPos.te_1;
          ddoDomPer.te_2 = ddoDomPos.te_2;
          ddoDomPer.te_celular = ddoDomPos.te_celular;
      }

      public static void Competencias_Pos(NucleusRH.Base.Gestion_de_Postulantes.Postulantes.POSTULANTE ddoPos, NucleusRH.Base.Gestion_de_Postulantes.Reclutados.RECLUTADO ddoRec)
      {
          //RECORRO LOS EXAMENES QUE TIENE EL RECLUTADO
          foreach(NucleusRH.Base.Gestion_de_Postulantes.Reclutados.EXA_REC ddoExaRec in ddoRec.EXA_REC)
          {
              //TIRO UN QRY PARA SABER QUE COMPETENCIAS HABILITA CADA UNO DE LOS EXAMENES RECUPERADOS
              string param = "<DATOS oi_ref_examen=\"" + ddoExaRec.oi_ref_examen + "\"/>";
              Nomad.NSystem.Document.NmdXmlDocument docCompet = null;
              docCompet = new Nomad.NSystem.Document.NmdXmlDocument(NomadEnvironment.QueryString(NucleusRH.Base.Gestion_de_Postulantes.Reclutados.RECLUTADO.Resources.QRY_COMPETENCIAS,param));

              //RECORRO CADA UNO DE LOS OI_COMPETENCIA RECUPERADOS
              Nomad.NSystem.Document.NmdXmlDocument com;
              for (com=(Nomad.NSystem.Document.NmdXmlDocument)docCompet.GetFirstChildDocument(); com!=null; com=(Nomad.NSystem.Document.NmdXmlDocument)docCompet.GetNextChildDocument())
              {
                  int band = 0;
                  //RECORRO LAS COMPETENCIAS QUE YA TIENE CARGADAS EL POSTULANTE
                  foreach(NucleusRH.Base.Gestion_de_Postulantes.Postulantes.COMPETENCIA_POS ddoCompetPos in ddoPos.COMPET_POS)
                  {
                      //PREGUNTO SI EL POSTULANTE YA TIENE CARGADA LA COMPTENCIA RECUPERADA
                      if (ddoCompetPos.oi_competencia == com.GetAttribute("oi_competencia").Value)
                      {
                          band=1;
                      }
                  }
                  //SI BAND = 0 ES PORQUE LA COMPETENCIA NO ESTABA CARGADA EN EL POSTULANTE
                  if (band == 0)
                  {
                      //CARGO LA COMPETENcIA
                      NucleusRH.Base.Gestion_de_Postulantes.Postulantes.COMPETENCIA_POS ddoCompet;
                      ddoCompet = new NucleusRH.Base.Gestion_de_Postulantes.Postulantes.COMPETENCIA_POS();
                      ddoCompet.oi_competencia = com.GetAttribute("oi_competencia").Value;
                      ddoPos.COMPET_POS.Add(ddoCompet);
                  }
              }
          }
          NomadEnvironment.GetCurrentTransaction().SaveRefresh(ddoPos);
      }

      public static void Competencias_Per(NucleusRH.Base.Personal.Legajo.PERSONAL ddoPer, NucleusRH.Base.Gestion_de_Postulantes.Reclutados.RECLUTADO ddoRec)
      {
          //RECORRO LOS EXAMENES QUE TIENE EL RECLUTADO
          foreach(NucleusRH.Base.Gestion_de_Postulantes.Reclutados.EXA_REC ddoExaRec in ddoRec.EXA_REC)
          {
              //TIRO UN QRY PARA SABER QUE COMPETENCIAS HABILITA CADA UNO DE LOS EXAMENES RECUPERADOS
              string param = "<DATOS oi_ref_examen=\"" + ddoExaRec.oi_ref_examen + "\"/>";
              Nomad.NSystem.Document.NmdXmlDocument docCompet = null;
              docCompet = new Nomad.NSystem.Document.NmdXmlDocument(NomadEnvironment.QueryString(NucleusRH.Base.Gestion_de_Postulantes.Reclutados.RECLUTADO.Resources.QRY_COMPETENCIAS,param));

              //RECORRO CADA UNO DE LOS OI_COMPETENCIA RECUPERADOS
              Nomad.NSystem.Document.NmdXmlDocument com;
              for (com=(Nomad.NSystem.Document.NmdXmlDocument)docCompet.GetFirstChildDocument(); com!=null; com=(Nomad.NSystem.Document.NmdXmlDocument)docCompet.GetNextChildDocument())
              {
                  int band = 0;
                  //RECORRO LAS COMPETENCIAS QUE YA TIENE CARGADAS EL PERSONAL
                  foreach(NucleusRH.Base.Personal.Legajo.COMPETENCIA_PER ddoCompetPer in ddoPer.COMPET_PER)
                  {
                      //PREGUNTO SI EL POSTULANTE YA TIENE CARGADA LA COMPTENCIA RECUPERADA
                      if (ddoCompetPer.oi_competencia == com.GetAttribute("oi_competencia").Value)
                      {
                          band=1;
                      }
                  }
                  //SI BAND = 0 ES PORQUE LA COMPETENCIA NO ESTABA CARGADA EN EL PERSONAL
                  if (band == 0)
                  {
                      //CARGO LA COMPETENcIA
                      NucleusRH.Base.Personal.Legajo.COMPETENCIA_PER ddoCompet;
                      ddoCompet = new NucleusRH.Base.Personal.Legajo.COMPETENCIA_PER();
                      ddoCompet.oi_competencia = com.GetAttribute("oi_competencia").Value;
                      ddoPer.COMPET_PER.Add(ddoCompet);
                  }
              }
          }
          NomadEnvironment.GetCurrentTransaction().SaveRefresh(ddoPer);
      }

     /*public static void FirmarContratoPuesto(Nomad.NSystem.Proxy.NomadXML param)
      {

          NomadBatch objBatch;
          objBatch = NomadBatch.GetBatch("Iniciando...", "Firma de Contrato");

          param = param.FirstChild();

          objBatch.SetMess("Obteniendo el Reclutado...");
          objBatch.SetPro(10);
          //CARGO EL RECLUTADO
          NucleusRH.Base.Gestion_de_Postulantes.Reclutados.RECLUTADO DDOREC = NucleusRH.Base.Gestion_de_Postulantes.Reclutados.RECLUTADO.Get(param.GetAttr("oi_reclutado"));

          //FIRMA DE CONTRATO
          if (!DDOREC.oi_personal_empNull)
          {
              objBatch.SetMess("Redefiniendo requerimientos...");
              objBatch.SetPro(25);

              //RECUPERO TODOS LOS REQUERIMIENTOS EN DONDE ESTA INSCRIPTO EL POSTULANTE. TIRO UN QRY PARA TRAERLOS
              string qryparam = "<DATOS oi_Personal_emp=\"" + DDOREC.oi_personal_emp + "\"/>";
              NomadXML doc = new NomadXML();
              doc.SetText(NomadEnvironment.QueryString(NucleusRH.Base.Gestion_de_Postulantes.Reclutados.RECLUTADO.Resources.QRY_REQ_PERSONAL_EMP, qryparam));

              if (doc.FirstChild().ChildLength > 0)
              {
                  //CARGO CADA UNO DE LOS REQUERIMIENTOS RECUPERADOS
                  for (NomadXML req = doc.FirstChild().FirstChild(); req != null; req = req.Next())
                  {
                      NucleusRH.Base.Gestion_de_Postulantes.Requerimientos.REQ DDOREQ;
                      DDOREQ = NucleusRH.Base.Gestion_de_Postulantes.Requerimientos.REQ.Get(req.GetAttr("oi_req"));

                      //RECORRO LAS REFERENCIAS DE LOS REQUERIMIENTOS RECUPERADOS
                      foreach (NucleusRH.Base.Gestion_de_Postulantes.Requerimientos.REFERENCIA DDOREF in DDOREQ.REFERENCIAS)
                      {
                          //RECORRO LOS POSTULANTES DE LAS REFERENCIAS RECUPERADAS
                          foreach (NucleusRH.Base.Gestion_de_Postulantes.Requerimientos.REF_PER DDOREFPER in DDOREF.REF_PER)
                          {
                              //PREGUNTO SI EL POSTULANTE RECORRIDO ES EL QUE ESTOY CONTRATANDO
                              if (DDOREFPER.oi_personal_emp == DDOREC.oi_personal_emp)
                              {
                                  DDOREF.REF_PER.Remove(DDOREFPER);
                                  break;
                              }
                          }
                      }
                      //NomadEnvironment.GetTrace().Info(ddoReq.SerializeAll());
                      NomadEnvironment.GetCurrentTransaction().Save(DDOREQ);
                  }
              }

              objBatch.SetMess("Analizando Reclutado...");
              objBatch.SetPro(60);
              //EL RECLUTADO ES PERSONAL INTERNO
              NucleusRH.Base.Gestion_de_Postulantes.Personal_Seleccion.PERSONAL_EMP DDOLEG = NucleusRH.Base.Gestion_de_Postulantes.Personal_Seleccion.PERSONAL_EMP.Get(DDOREC.oi_personal_emp);
              //CARGO LA PERSONA
              NucleusRH.Base.Personal.Legajo.PERSONAL DDOPER = NucleusRH.Base.Personal.Legajo.PERSONAL.Get(DDOLEG.oi_personal);
              //ACTUALIZO LAS COMPETENCIAS DE LA PERSONA
              NucleusRH.Base.Gestion_de_Postulantes.Reclutados.RECLUTADO.Competencias_Per(DDOPER, DDOREC);

              //CREO EL PUESTO DEL LEGAJO
              NucleusRH.Base.Personal.LegajoEmpresa.PUESTO_PER DDOPUELEG = new NucleusRH.Base.Personal.LegajoEmpresa.PUESTO_PER();
              DDOPUELEG.f_ingreso = param.GetAttrDateTime("f_ingreso");
              DDOPUELEG.oi_motivo_cambio = "3";
              DDOPUELEG.oi_puesto = param.GetAttr("oi_puesto");

              if (!DDOLEG.oi_puesto_ultNull)
                  DDOLEG.Cambio_Puesto(DDOPUELEG);
              else
              {
                  DDOLEG.PUESTO_PER.Add(DDOPUELEG);
                  DDOLEG.oi_puesto_ult = param.GetAttr("oi_puesto");
                  DDOLEG.f_desde_puesto = param.GetAttrDateTime("f_ingreso");
              }

              objBatch.SetMess("Generando Legajo...");
              objBatch.SetPro(80);

              NomadEnvironment.GetCurrentTransaction().Begin();

              try
              {
                  NomadEnvironment.GetCurrentTransaction().SaveRefresh(DDOPER);
                  NomadEnvironment.GetCurrentTransaction().SaveRefresh(DDOLEG);
                  DDOREC.c_estado = "C";
                  DDOREC.f_contratacion = DateTime.Now;
                  NomadEnvironment.GetCurrentTransaction().Save(DDOREC);

                  NomadEnvironment.GetCurrentTransaction().Commit();
              }
              catch (Exception e)
              {
                  NomadEnvironment.GetCurrentTransaction().Rollback();
                  objBatch.Err("Error al firmar el contrato. " + e.Message);
              }

              //POSICION
              NucleusRH.Base.Personal.LegajoEmpresa.POSICION_PER DDOPOSICLEG = new NucleusRH.Base.Personal.LegajoEmpresa.POSICION_PER();
              DDOPOSICLEG.f_ingreso = param.GetAttrDateTime("f_ingreso");
              DDOPOSICLEG.oi_motivo_cambio = "3";
              DDOPOSICLEG.oi_posicion = param.GetAttr("oi_posicion");

              if (!DDOLEG.oi_posicion_ultNull)
                  DDOLEG.Cambio_Posicion(DDOPOSICLEG);
              else
              {
                  DDOLEG.oi_posicion_ult = param.GetAttr("oi_posicion");
                  DDOLEG.f_desde_posicion = param.GetAttrDateTime("f_ingreso");
                  DDOLEG.Asignar_Posicion(DDOPOSICLEG);
              }

          }
          else if (!DDOREC.oi_postulanteNull)
          {
              objBatch.SetMess("Redefiniendo requerimientos...");
              objBatch.SetPro(25);

              //RECUPERO TODOS LOS REQUERIMIENTOS EN DONDE ESTA INSCRIPTO EL POSTULANTE. TIRO UN QRY PARA TRAERLOS
              string qryparam = "<DATOS oi_postulante=\"" + DDOREC.oi_postulante + "\"/>";
              NomadXML doc = new NomadXML();
              doc.SetText(NomadEnvironment.QueryString(NucleusRH.Base.Gestion_de_Postulantes.Reclutados.RECLUTADO.Resources.QRY_REQ, qryparam));

              NomadEnvironment.GetTrace().Info("DOC POS -- " + doc.ToString() + " -- " + doc.FirstChild().ChildLength);

              //CARGO CADA UNO DE LOS REQUERIMIENTOS RECUPERADOS
              for (NomadXML req = doc.FirstChild().FirstChild(); req != null; req = req.Next())
              {
                  NucleusRH.Base.Gestion_de_Postulantes.Requerimientos.REQ DDOREQ;
                  DDOREQ = NucleusRH.Base.Gestion_de_Postulantes.Requerimientos.REQ.Get(req.GetAttr("oi_req"));

                  //RECORRO LAS REFERENCIAS DE LOS REQUERIMIENTOS RECUPERADOS
                  foreach (NucleusRH.Base.Gestion_de_Postulantes.Requerimientos.REFERENCIA DDOREF in DDOREQ.REFERENCIAS)
                  {
                      //RECORRO LOS POSTULANTES DE LAS REFERENCIAS RECUPERADAS
                      foreach (NucleusRH.Base.Gestion_de_Postulantes.Requerimientos.REF_PER DDOREFPER in DDOREF.REF_PER)
                      {
                          //PREGUNTO SI EL POSTULANTE RECORRIDO ES EL QUE ESTOY CONTRATANDO
                          if (DDOREFPER.oi_personal_emp == DDOREC.oi_personal_emp)
                          {
                              DDOREF.REF_PER.Remove(DDOREFPER);
                              break;
                          }
                      }
                  }
                  //NomadEnvironment.GetTrace().Info(ddoReq.SerializeAll());
                  NomadEnvironment.GetCurrentTransaction().Save(DDOREQ);
              }

              objBatch.SetMess("Generando Legajo...");
              objBatch.SetPro(75);

              //CARGO EL POSTULANTE
              NomadEnvironment.GetTrace().Info("REC -- " + DDOREC.SerializeAll());

              NucleusRH.Base.Gestion_de_Postulantes.Postulantes.POSTULANTE DDOPOS = NucleusRH.Base.Gestion_de_Postulantes.Postulantes.POSTULANTE.Get(DDOREC.oi_postulante);
              DDOPOS.CrearLegajo(param.GetAttr("oi_empresa"), param.GetAttr("oi_puesto"), param.GetAttr("oi_posicion"), param.GetAttrDateTime("f_ingreso"), param.GetAttrInt("e_numero_legajo"), param.GetAttr("oi_tipo_personal"), param.GetAttr("oi_ctro_costo"), param.GetAttr("oi_calendario"), param.GetAttr("o_personal_emp"));

              NomadEnvironment.GetCurrentTransaction().Begin();
              try
              {
                  DDOPOS.c_estado = "C";
                  DDOREC.c_estado = "C";
                  DDOREC.f_contratacion = DateTime.Now;
                  NomadEnvironment.GetCurrentTransaction().Save(DDOPOS);
                  NomadEnvironment.GetCurrentTransaction().Save(DDOREC);
                  NomadEnvironment.GetCurrentTransaction().Commit();
              }
              catch (Exception e)
              {
                  NomadEnvironment.GetCurrentTransaction().Rollback();
                  objBatch.Err("Error al firmar el contrato. " + e.Message);
              }
          }

          objBatch.Log("Se realizo la firma de contrato de manera satisfactoria");

      }
      public static void CargarResultado(Nomad.NSystem.Document.NmdXmlDocument datos, string exa)
      {

          //PERMITE LA CARGA DE RESULTADOS VALIDANDO QUE SE INGRESE EL EXAMEN CUYO ORDEN SEA EL CORRESPONDIENTE Y
          //QUE EL ULTIMO EXAMEN CARGADO HABILITE LA CARGA DEL RESULTADO DEL EXAMEN EN CUESTION
          NomadEnvironment.GetTrace().Info("Entro al metodo");

          string oi_exa_ant = null;
          string oi_res_ref_exa_ant = null;
          Nomad.NSystem.Document.NmdXmlDocument x;

          //RECORRO TODOS LOS EXAMENES CARGADOS EN EL FORMULARIO
          for (x = (Nomad.NSystem.Document.NmdXmlDocument)datos.GetFirstChildDocument(); x != null; x = (Nomad.NSystem.Document.NmdXmlDocument)datos.GetNextChildDocument())
          {
              //ME PARO EN EL EXAMEN SELECCIONADO
              if (exa == x.GetAttribute("oi_ref_examen").Value)
              {
                  NomadEnvironment.GetTrace().Info("res ref exa: " + x.GetAttribute("oi_ref_res_exa").Value);
                  //PREGUNTO SI EL EXAMEN SIGUIENTE YA TIENE CARGADO UN RESULTADO
                  Nomad.NSystem.Document.NmdXmlDocument sig;
                  sig = (Nomad.NSystem.Document.NmdXmlDocument)datos.GetNextChildDocument();
                  //PREGUNTO SI LA ASIGNACION DEVOLVIO UN DOCUMENTO VACIO, LO QUE IMPLICARIA QUE LA LINEA DONDE ESTOY PARADO ES LA ULTIMA DEL DOCUMENTO XML
                  if (sig != null)
                  {
                      //SI NO ES VACIO ES PORQUE NO ERA EL ULTIMO ENTONCES VALIDO LA CARGA DE RESULTADO PARA LA LINEA SIGUIENTE
                      NomadEnvironment.GetTrace().Info("ref_res_exa sig: " + sig.GetAttribute("oi_ref_res_exa").Value);
                      if (sig.GetAttribute("oi_ref_res_exa").Value != "")
                      {
                          //NO LE PERMITO INGRESAR EL RESULTADO. PRIMERO DEBE ELIMINAR EL RESULTADO DEL EXAMEN SIGUIENTE
                          throw new NomadAppException("No puede modificar el valor del resultado del examen seleccionado debido a que el examen siguiente tiene cargado un resultado");
                      }
                  }
                  //PREGUTNO SI ESTOY PARADO EN EL PRIMER EXAMEN
                  if (oi_res_ref_exa_ant == null)
                  {
                      //ES EL PRIMER EXAMEN, ENTONCES DEJO REALIZAR LA CARGA
                      NomadEnvironment.GetTrace().Info("es nulo --> el primero");
                  }
                  else
                  {
                      //LLAMO A UN RECURSO PARA PREGUNTAR SI EL RESULTADO DEL EXAMEN ANTERIOR ESTA CARGADO Y ESTE HABILITA LA CARGA
                      NomadEnvironment.GetTrace().Info("ref_exa ant: " + oi_exa_ant);
                      NomadEnvironment.GetTrace().Info("res ref exa ant: " + oi_res_ref_exa_ant);
                      string param = "<DATOS oi_ref_res_exa=\"" + oi_res_ref_exa_ant + "\" oi_ref_examen=\"" + oi_exa_ant + "\"/>";
                      NomadEnvironment.GetTrace().Info("PARAM: " + param);
                      //Ejecuto el recurso
                      Nomad.NSystem.Document.NmdXmlDocument apto = null;
                      apto = new Nomad.NSystem.Document.NmdXmlDocument(NomadEnvironment.QueryString(NucleusRH.Base.Gestion_de_Postulantes.Reclutados.RECLUTADO.Resources.QRY_HABILITA, param));
                      NomadEnvironment.GetTrace().Info(apto.ToString());
                      //NO ESTA CARGADO O NO HABILITA LA CARGA
                      if (apto.GetAttribute("l_apto_prox_examen").Value != "1")
                      {
                          throw new NomadAppException("No puede cargar el resultado del examen especificado. El orden no es el correcto, o el resultado del examen previo no habilita la carga del mismo");
                      }
                  }
              }
              //ASIGNO A ESTAS VARIABLES EL ULTIMO EXAMEN Y RESULTADO RECORRIDO, ASI PARA LA PROXIMA VUELTA TENGO EL ANTERIOR
              oi_res_ref_exa_ant = x.GetAttribute("oi_ref_res_exa").Value;
              oi_exa_ant = x.GetAttribute("oi_ref_examen").Value;
          }

      }*/
      public void AsignarExaRec()
      {

          //PARA ASIGNAR LOS EXAMANES SUBEXAMENES Y FACTORES A UN RECLUTADO ME TENGO QUE FIJAR A QUE REFERENCIA ESTA
          //ASOCIADO EL RECLUTADO Y DESPUES ASOCIAR TODOS LOS EXAMANES SUBEXAMENES Y FACTORES DE LA REFERENCIA AL RECLUTADO

          //PRIMERO RECUPERO LA REFERENCIA A LA QUE ESTA ASOCIADO EL RECLUTADO
          NucleusRH.Base.Gestion_de_Postulantes.Requerimientos.REFERENCIA ddoRef = this.Getoi_referencia();

          //AHORA RECUPERO TODOS LOS EXAMENES ASOCIADOS A LA REFERENCIA
          foreach (NucleusRH.Base.Gestion_de_Postulantes.Requerimientos.REF_EXAMEN ddoRefExa in ddoRef.REF_EXAMEN)
          {
              //CREO UN NUEVO EXAMEN PARA EL RECLUTADO Y LE ASIGNO EL OI_REF_EXAMEN
              NucleusRH.Base.Gestion_de_Postulantes.Reclutados.EXA_REC ddoRecExa;
              ddoRecExa = new NucleusRH.Base.Gestion_de_Postulantes.Reclutados.EXA_REC();
              ddoRecExa.oi_ref_examen = ddoRefExa.Id;
              
              //AGREGO EL EXAMEN POR RECLUTADO AL RECLUTADO
              this.EXA_REC.Add(ddoRecExa);
          }
          //NomadEnvironment.GetCurrentTransaction().Save(this);

      }   
  }
}



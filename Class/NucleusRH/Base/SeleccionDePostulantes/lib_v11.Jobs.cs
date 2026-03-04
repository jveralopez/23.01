using System;
using System.Xml;
using System.Collections;

using Nomad.NSystem.Proxy;
using Nomad.NSystem.Functions;
using Nomad.NSystem.Base;
using System.Collections.Generic;
using System.Net;
using System.IO;
using System.Reflection;
using NucleusRH.Base.SeleccionDePostulantes;
using System.Text.RegularExpressions;

namespace NucleusRH.Base.SeleccionDePostulantes.Jobs
{
    public partial class JOBS : Nomad.NSystem.Base.NomadObject
    {
        static string token = "";
        static NomadBatch objBatch = NomadBatch.GetBatch("Proceso Jobs", "Proceso Jobs");
        static Message msj = null;
        static string jobsUrl = "";
        static string jobsUsr = "";
        static string jobsPass = "";
        static Hashtable hashEndpoints = BuscarEndpoints();
        IWebProxy webProxy = WebRequest.DefaultWebProxy;

        #region Clases

        public class Usuario
        {
            private string _username;
            private string _password;
            private string _token;

            public string username { get { return _username; } set { _username = value; } }
            public string password { get { return _password; } set { _password = value; } }
            public string token { get { return _token; } set { _token = value; } }

            public Usuario(string usr, string pass)
            {
                username = usr;
                password = pass;
            }
        }

        public class Message
        {
            private string _message;
            public string message { get { return _message; } set { _message = value; } }

            public Message() { }
            public Message(string msj)
            {
                this.message = msj;
            }
        }

        public class Codificadora
        {
            public List<Data> codificadoras = new List<Data>();
            public List<Data> data = new List<Data>();

            public class Data
            {
                private string _external_id;
                private string _id;
                private string _parentId;
                private string _code;
                private string _description;
                private string _type;

                public string external_id { get { return _external_id; } set { _external_id = value; } }
                public string id { get { return _id; } set { _id = value; } }
                public string parentId { get { return _parentId; } set { _parentId = value; } }
                public string code { get { return _code; } set { _code = value; } }
                public string description { get { return _description; } set { _description = value; } }
                public string type { get { return _type; } set { _type = value; } }
            }
        }

        public class Ofertas
        {
            public List<Oferta> data = new List<Oferta>();

            public class Oferta
            {
                private DateTime? _f_cierre;
                private NucleusOffer _nucleus_offer;
                private bool _ask_income;
                private bool _show_salary;
                private bool? _is_full_time ;
                private bool? _is_part_time ;
                private bool? _is_remote ;
                private bool? _is_temporal  ;
                private double? _highest_expected_salary;
                private double? _lowest_expected_salary  ;
                private double? _place_lat ;
                private double? _place_lng;
                private int _external_id;
                private int _oi_oferta_lab;
                private int? _areaId ;
                private int? _businessUnitId ;
                private int? _cityId;
                private int? _countryId ;
                private int? _experienceId  ;
                private int? _highest_experience_years ;
                private int? _id ;
                private int? _lowest_experience_years ;
                private int? _provinceId ;
                private string _description;
                private string _summary;
                private string _title;
                
                public DateTime? f_cierre { get { return _f_cierre; } set { _f_cierre = value; } }
                public NucleusOffer nucleus_offer { get { return _nucleus_offer; } set { _nucleus_offer = value; } }
                public bool ask_income { get { return _ask_income; } set { _ask_income = value; } }
                public bool show_salary { get { return _show_salary; } set { _show_salary = value; } }
                public bool? is_full_time { get { return _is_full_time; } set { _is_full_time = value; } }
                public bool? is_part_time { get { return _is_part_time; } set { _is_part_time = value; } }
                public bool? is_remote { get { return _is_remote; } set { _is_remote = value; } }
                public bool? is_temporal { get { return _is_temporal; } set { _is_temporal = value; } }
                public double? highest_expected_salary { get { return _highest_expected_salary; } set { _highest_expected_salary = value; } }
                public double? lowest_expected_salary { get { return _lowest_expected_salary; } set { _lowest_expected_salary = value; } }
                public double? place_lat { get { return _place_lat; } set { _place_lat = value; } }
                public double? place_lng { get { return _place_lng; } set { _place_lng = value; } }
                public int external_id { get { return _external_id; } set { _external_id = value; } }
                public int oi_oferta_lab { get { return _oi_oferta_lab; } set { _oi_oferta_lab = value; } }
                public int? areaId { get { return _areaId; } set { _areaId = value; } }
                public int? businessUnitId { get { return _businessUnitId; } set { _businessUnitId = value; } }
                public int? cityId { get { return _cityId; } set { _cityId = value; } }
                public int? countryId { get { return _countryId; } set { _countryId = value; } }
                public int? experienceId { get { return _experienceId; } set { _experienceId = value; } }
                public int? highest_experience_years { get { return _highest_experience_years; } set { _highest_experience_years = value; } }
                public int? id { get { return _id; } set { _id = value; } }
                public int? lowest_experience_years { get { return _lowest_experience_years; } set { _lowest_experience_years = value; } }
                public int? provinceId { get { return _provinceId; } set { _provinceId = value; } }
                public string description { get { return _description; } set { _description = value; } }
                public string summary { get { return _summary; } set { _summary = value; } }
                public string title { get { return _title; } set { _title = value; } }

                public Oferta() { }
            }

            public class NucleusOffer
            {
                private DateTime? _f_cierre = null;
                private DateTime? _f_oferta_lab = null;
                private bool _l_clevel;
                private bool _l_digital;
                private bool _l_gerencia;
                private bool _l_junior;
                private bool _l_mamedio;
                private bool _l_oficio;
                private bool _l_primer;
                private bool _l_prof;
                private bool _l_semi;
                private bool _l_senior;
                private bool? _l_exc_ecivil;
                private bool? _l_exc_edad;
                private bool? _l_exc_edu;
                private bool? _l_exc_exp;
                private bool? _l_exc_idioma;
                private bool? _l_exc_res;
                private bool? _l_exc_salario;
                private bool? _l_exc_sexo;
                private bool? _l_mostrar_ecivil;
                private bool? _l_mostrar_edad;
                private bool? _l_mostrar_edu;
                private bool? _l_mostrar_en_aviso;
                private bool? _l_mostrar_exp;
                private bool? _l_mostrar_idioma;
                private bool? _l_mostrar_res;
                private bool? _l_mostrar_sexo;
                private bool? _l_publicacion_web;
                private bool? _l_solicita_sal;
                private double? _n_salario_d;
                private double? _n_salario_h;
                private int? _e_cantidad;
                private int? _e_edad_desde;
                private int? _e_edad_hasta;
                private int? _e_exp_des;
                private int? _e_exp_hasta;
                private int? _oi_area_lab;
                private int? _oi_empresa;
                private int? _oi_estado_civil;
                private int? _oi_estado_est;
                private int? _oi_localidad;
                private int? _oi_localidad_res;
                private int? _oi_nivel_al;
                private int? _oi_nivel_estudio;
                private int? _oi_nivel_fr;
                private int? _oi_nivel_ing;
                private int? _oi_nivel_it;
                private int? _oi_nivel_jap;
                private int? _oi_nivel_por;
                private int? _oi_oferta_lab;
                private int? _oi_pais;
                private int? _oi_pais_res;
                private int? _oi_provincia;
                private int? _oi_provincia_res;
                private int? _oi_seniority;
                private int? _oi_tipo_puesto;
                private int? _oi_uni_tpo_desde;
                private int? _oi_uni_tpo_hasta;
                private string _c_area_lab;
                private string _c_empresa;
                private string _c_estado_civil;
                private string _c_estado_est;
                private string _c_localidad;
                private string _c_localidad_res;
                private string _c_nivel_al;
                private string _c_nivel_estudio;
                private string _c_nivel_fr;
                private string _c_nivel_ing;
                private string _c_nivel_it;
                private string _c_nivel_jap;
                private string _c_nivel_por;
                private string _c_oferta_lab;
                private string _c_pais;
                private string _c_pais_res;
                private string _c_provincia;
                private string _c_provincia_res;
                private string _c_seniority;
                private string _c_sexo;
                private string _c_tipo_puesto;
                private string _c_tipos_serv;
                private string _c_uni_negocio;
                private string _c_uni_tpo_desde;
                private string _c_uni_tpo_hasta;
                private string _d_coordenadas;
                private string _d_mail;
                private string _d_mail_alt_1;
                private string _d_mail_alt_2;
                private string _d_oferta_lab;
                private string _d_uni_negocio;
                private string _o_oferta_lab;
                private string _c_destino_publi;

                public DateTime? f_cierre { get { return _f_cierre; } set { _f_cierre = value; } }
                public DateTime? f_oferta_lab { get { return _f_oferta_lab; } set { _f_oferta_lab = value; } }
                public bool l_clevel { get { return _l_clevel; } set { _l_clevel = value; } }
                public bool l_digital { get { return _l_digital; } set { _l_digital = value; } }
                public bool l_gerencia { get { return _l_gerencia; } set { _l_gerencia = value; } }
                public bool l_junior { get { return _l_junior; } set { _l_junior = value; } }
                public bool l_mamedio { get { return _l_mamedio; } set { _l_mamedio = value; } }
                public bool l_oficio { get { return _l_oficio; } set { _l_oficio = value; } }
                public bool l_primer { get { return _l_primer; } set { _l_primer = value; } }
                public bool l_prof { get { return _l_prof; } set { _l_prof = value; } }
                public bool l_semi { get { return _l_semi; } set { _l_semi = value; } }
                public bool l_senior { get { return _l_senior; } set { _l_senior = value; } }
                public bool? l_exc_ecivil { get { return _l_exc_ecivil; } set { _l_exc_ecivil = value; } }
                public bool? l_exc_edad { get { return _l_exc_edad; } set { _l_exc_edad = value; } }
                public bool? l_exc_edu { get { return _l_exc_edu; } set { _l_exc_edu = value; } }
                public bool? l_exc_exp { get { return _l_exc_exp; } set { _l_exc_exp = value; } }
                public bool? l_exc_idioma { get { return _l_exc_idioma; } set { _l_exc_idioma = value; } }
                public bool? l_exc_res { get { return _l_exc_res; } set { _l_exc_res = value; } }
                public bool? l_exc_salario { get { return _l_exc_salario; } set { _l_exc_salario = value; } }
                public bool? l_exc_sexo { get { return _l_exc_sexo; } set { _l_exc_sexo = value; } }
                public bool? l_mostrar_ecivil { get { return _l_mostrar_ecivil; } set { _l_mostrar_ecivil = value; } }
                public bool? l_mostrar_edad { get { return _l_mostrar_edad; } set { _l_mostrar_edad = value; } }
                public bool? l_mostrar_edu { get { return _l_mostrar_edu; } set { _l_mostrar_edu = value; } }
                public bool? l_mostrar_en_aviso { get { return _l_mostrar_en_aviso; } set { _l_mostrar_en_aviso = value; } }
                public bool? l_mostrar_exp { get { return _l_mostrar_exp; } set { _l_mostrar_exp = value; } }
                public bool? l_mostrar_idioma { get { return _l_mostrar_idioma; } set { _l_mostrar_idioma = value; } }
                public bool? l_mostrar_res { get { return _l_mostrar_res; } set { _l_mostrar_res = value; } }
                public bool? l_mostrar_sexo { get { return _l_mostrar_sexo; } set { _l_mostrar_sexo = value; } }
                public bool? l_publicacion_web { get { return _l_publicacion_web; } set { _l_publicacion_web = value; } }
                public bool? l_solicita_sal { get { return _l_solicita_sal; } set { _l_solicita_sal = value; } }
                public double? n_salario_d { get { return _n_salario_d; } set { _n_salario_d = value; } }
                public double? n_salario_h { get { return _n_salario_h; } set { _n_salario_h = value; } }
                public int? e_cantidad { get { return _e_cantidad; } set { _e_cantidad = value; } }
                public int? e_edad_desde { get { return _e_edad_desde; } set { _e_edad_desde = value; } }
                public int? e_edad_hasta { get { return _e_edad_hasta; } set { _e_edad_hasta = value; } }
                public int? e_exp_des { get { return _e_exp_des; } set { _e_exp_des = value; } }
                public int? e_exp_hasta { get { return _e_exp_hasta; } set { _e_exp_hasta = value; } }
                public int? oi_area_lab { get { return _oi_area_lab; } set { _oi_area_lab = value; } }
                public int? oi_empresa { get { return _oi_empresa; } set { _oi_empresa = value; } }
                public int? oi_estado_civil { get { return _oi_estado_civil; } set { _oi_estado_civil = value; } }
                public int? oi_estado_est { get { return _oi_estado_est; } set { _oi_estado_est = value; } }
                public int? oi_localidad { get { return _oi_localidad; } set { _oi_localidad = value; } }
                public int? oi_localidad_res { get { return _oi_localidad_res; } set { _oi_localidad_res = value; } }
                public int? oi_nivel_al { get { return _oi_nivel_al; } set { _oi_nivel_al = value; } }
                public int? oi_nivel_estudio { get { return _oi_nivel_estudio; } set { _oi_nivel_estudio = value; } }
                public int? oi_nivel_fr { get { return _oi_nivel_fr; } set { _oi_nivel_fr = value; } }
                public int? oi_nivel_ing { get { return _oi_nivel_ing; } set { _oi_nivel_ing = value; } }
                public int? oi_nivel_it { get { return _oi_nivel_it; } set { _oi_nivel_it = value; } }
                public int? oi_nivel_jap { get { return _oi_nivel_jap; } set { _oi_nivel_jap = value; } }
                public int? oi_nivel_por { get { return _oi_nivel_por; } set { _oi_nivel_por = value; } }
                public int? oi_oferta_lab { get { return _oi_oferta_lab; } set { _oi_oferta_lab = value; } }
                public int? oi_pais { get { return _oi_pais; } set { _oi_pais = value; } }
                public int? oi_pais_res { get { return _oi_pais_res; } set { _oi_pais_res = value; } }
                public int? oi_provincia { get { return _oi_provincia; } set { _oi_provincia = value; } }
                public int? oi_provincia_res { get { return _oi_provincia_res; } set { _oi_provincia_res = value; } }
                public int? oi_seniority { get { return _oi_seniority; } set { _oi_seniority = value; } }
                public int? oi_tipo_puesto { get { return _oi_tipo_puesto; } set { _oi_tipo_puesto = value; } }
                public int? oi_uni_tpo_desde { get { return _oi_uni_tpo_desde; } set { _oi_uni_tpo_desde = value; } }
                public int? oi_uni_tpo_hasta { get { return _oi_uni_tpo_hasta; } set { _oi_uni_tpo_hasta = value; } }
                public string c_area_lab { get { return _c_area_lab; } set { _c_area_lab = value; } }
                public string c_empresa { get { return _c_empresa; } set { _c_empresa = value; } }
                public string c_estado_civil { get { return _c_estado_civil; } set { _c_estado_civil = value; } }
                public string c_estado_est { get { return _c_estado_est; } set { _c_estado_est = value; } }
                public string c_localidad { get { return _c_localidad; } set { _c_localidad = value; } }
                public string c_localidad_res { get { return _c_localidad_res; } set { _c_localidad_res = value; } }
                public string c_nivel_al { get { return _c_nivel_al; } set { _c_nivel_al = value; } }
                public string c_nivel_estudio { get { return _c_nivel_estudio; } set { _c_nivel_estudio = value; } }
                public string c_nivel_fr { get { return _c_nivel_fr; } set { _c_nivel_fr = value; } }
                public string c_nivel_ing { get { return _c_nivel_ing; } set { _c_nivel_ing = value; } }
                public string c_nivel_it { get { return _c_nivel_it; } set { _c_nivel_it = value; } }
                public string c_nivel_jap { get { return _c_nivel_jap; } set { _c_nivel_jap = value; } }
                public string c_nivel_por { get { return _c_nivel_por; } set { _c_nivel_por = value; } }
                public string c_oferta_lab { get { return _c_oferta_lab; } set { _c_oferta_lab = value; } }
                public string c_pais { get { return _c_pais; } set { _c_pais = value; } }
                public string c_pais_res { get { return _c_pais_res; } set { _c_pais_res = value; } }
                public string c_provincia { get { return _c_provincia; } set { _c_provincia = value; } }
                public string c_provincia_res { get { return _c_provincia_res; } set { _c_provincia_res = value; } }
                public string c_seniority { get { return _c_seniority; } set { _c_seniority = value; } }
                public string c_sexo { get { return _c_sexo; } set { _c_sexo = value; } }
                public string c_tipo_puesto { get { return _c_tipo_puesto; } set { _c_tipo_puesto = value; } }
                public string c_tipos_serv { get { return _c_tipos_serv; } set { _c_tipos_serv = value; } }
                public string c_uni_negocio { get { return _c_uni_negocio; } set { _c_uni_negocio = value; } }
                public string c_uni_tpo_desde { get { return _c_uni_tpo_desde; } set { _c_uni_tpo_desde = value; } }
                public string c_uni_tpo_hasta { get { return _c_uni_tpo_hasta; } set { _c_uni_tpo_hasta = value; } }
                public string d_coordenadas { get { return _d_coordenadas; } set { _d_coordenadas = value; } }
                public string d_mail { get { return _d_mail; } set { _d_mail = value; } }
                public string d_mail_alt_1 { get { return _d_mail_alt_1; } set { _d_mail_alt_1 = value; } }
                public string d_mail_alt_2 { get { return _d_mail_alt_2; } set { _d_mail_alt_2 = value; } }
                public string d_oferta_lab { get { return _d_oferta_lab; } set { _d_oferta_lab = value; } }
                public string d_uni_negocio { get { return _d_uni_negocio; } set { _d_uni_negocio = value; } }
                public string o_oferta_lab { get { return _o_oferta_lab; } set { _o_oferta_lab = value; } }
                public string c_destino_publi { get { return _c_destino_publi; } set { _c_destino_publi = value; } }

                //public List<Pregunta> Preguntas { get; set; }
                public List<Pregunta> Preguntas = new List<Pregunta>();

                public NucleusOffer() { }
            }

            public class Pregunta
            {
                private int? _ID;
                private string _d_pregunta;

                public int? ID { get { return _ID; } set { _ID = value; } }
                public string d_pregunta { get { return _d_pregunta; } set { _d_pregunta = value; } }

                public List<Opciones> Opciones = new List<Opciones>();

                public Pregunta() { }
            }

            public class Opciones
            {
                private int? _ID;
                private string _d_pregunta_op;

                public int? ID { get { return _ID; } set { _ID = value; } }
                public string d_pregunta_op { get { return _d_pregunta_op; } set { _d_pregunta_op = value; } }

                public Opciones() { }
            }
        }

        public class Applications
        {
            public List<Apply> data = new List<Apply>();

            public class Apply
            {
                private int _offerId;
                private int _userId;
                private double? _n_remuneracion;
                private bool? _l_leido;
                private DateTime? _createdAt;
                private string _answers;

                public List<Respuesta> respuestas = new List<Respuesta>();

                public int offerId { get { return _offerId; } set { _offerId = value; } }
                public int userId { get { return _userId; } set { _userId = value; } }
                public bool? l_leido { get { return _l_leido; } set { _l_leido = value; } }
                public DateTime? createdAt { get { return _createdAt; } set { _createdAt = value; } }
                public double? n_remuneracion { get { return _n_remuneracion; } set { _n_remuneracion = value; } }
                public string answers { get { return _answers; } set { _answers = value; } }

            }

            public class Opciones
            {
                private int? _ID;
                private string _d_pregunta_op;

                public int? ID { get { return _ID; } set { _ID = value; } }
                public string d_pregunta_op { get { return _d_pregunta_op; } set { _d_pregunta_op = value; } }
            }

            public class Respuesta
            {
                private int _ID;
                private int? _ID_respuesta;
                private string _answer;
                private string _d_pregunta;

                public List<Opciones> Opciones = new List<Opciones>();
                public int ID { get { return _ID; } set { _ID = value; } }
                public int? ID_respuesta { get { return _ID_respuesta; } set { _ID_respuesta = value; } }
                public string answer { get { return _answer; } set { _answer = value; } }
                public string d_pregunta { get { return _d_pregunta; } set { _d_pregunta = value; } }
            }
        }

        public class ReadApplications
        {
            public List<Apply> data = new List<Apply>();

            public class Apply
            {
                private int _oi_oferta_lab;
                private bool _l_leido;
                private int? _userId;
                private int? _dni_type;
                private int? _dni;

                public int oi_oferta_lab { get { return _oi_oferta_lab; } set { _oi_oferta_lab = value; } }
                public int? userId { get { return _userId; } set { _userId = value; } }
                public bool l_leido { get { return _l_leido; } set { _l_leido = value; } }
                public int? dni_type { get { return _dni_type; } set { _dni_type = value; } }
                public int? dni { get { return _dni; } set { _dni = value; } }

            }
        }

        public class CVs
        {
            public class CV
            {
                private bool? _can_receive_emails;
                private int? _id;
                private int? _role;
                private string _about;
                private object _address;
                private object _age;
                private object _countryId;
                private object _cv;
                private object _cv_uploaded_at;
                private object _deletedAt;
                private object _dni;
                private object _dni_type;
                private object _educationLevelId;
                private object _facebook_id;
                private object _first_name;
                private object _google_id;
                private object _last_name;
                private object _linkedin_id;
                private object _phone;
                private object _profile_pic;
                private object _second_phone;
                private object _twitter_id;
                private object _usersEducationId;
                private object _usersProfessionalId;
                private string _email;
                private string _password;
                private string _username;
                private DateTime _createdAt;
                private DateTime? _updatedAt;
                private NucleusUser _nucleus_user;
                private InformacionProfesional _users_professional;

                public bool? can_receive_emails { get { return _can_receive_emails; } set { _can_receive_emails = value; } }
                public int? id { get { return _id; } set { _id = value; } }
                public int? role { get { return _role; } set { _role = value; } }
                public string about { get { return _about; } set { _about = value; } }
                public object address { get { return _address; } set { _address = value; } }
                public object age { get { return _age; } set { _age = value; } }
                public object countryId { get { return _countryId; } set { _countryId = value; } }
                public object cv { get { return _cv; } set { _cv = value; } }
                public object cv_uploaded_at { get { return _cv_uploaded_at; } set { _cv_uploaded_at = value; } }
                public object deletedAt { get { return _deletedAt; } set { _deletedAt = value; } }
                public object dni { get { return _dni; } set { _dni = value; } }
                public object dni_type { get { return _dni_type; } set { _dni_type = value; } }
                public object educationLevelId { get { return _educationLevelId; } set { _educationLevelId = value; } }
                public object facebook_id { get { return _facebook_id; } set { _facebook_id = value; } }
                public object first_name { get { return _first_name; } set { _first_name = value; } }
                public object google_id { get { return _google_id; } set { _google_id = value; } }
                public object last_name { get { return _last_name; } set { _last_name = value; } }
                public object linkedin_id { get { return _linkedin_id; } set { _linkedin_id = value; } }
                public object phone { get { return _phone; } set { _phone = value; } }
                public object profile_pic { get { return _profile_pic; } set { _profile_pic = value; } }
                public object second_phone { get { return _second_phone; } set { _second_phone = value; } }
                public object twitter_id { get { return _twitter_id; } set { _twitter_id = value; } }
                public object usersEducationId { get { return _usersEducationId; } set { _usersEducationId = value; } }
                public object usersProfessionalId { get { return _usersProfessionalId; } set { _usersProfessionalId = value; } }
                public string email { get { return _email; } set { _email = value; } }
                public string password { get { return _password; } set { _password = value; } }
                public string username { get { return _username; } set { _username = value; } }
                public DateTime createdAt { get { return _createdAt; } set { _createdAt = value; } } //era nuleable
                public DateTime? updatedAt { get { return _updatedAt; } set { _updatedAt = value; } }
                public NucleusUser nucleus_user { get { return _nucleus_user; } set { _nucleus_user = value; } }
                public InformacionProfesional users_professional { get { return _users_professional; } set { _users_professional = value; } }
            }

            public class NucleusUser
            {
                private DateTime? _f_nacim;
                private bool? _l_informacion;
                private double? _n_remuneracion;
                private int? _oi_estado_civil;
                private int? _oi_localidad;
                //private int? _oi_nacionalidad;
                private int? _oi_pais;
                private int? _oi_provincia;
                private int? _oi_tipo_documento;
                private string _c_area;
                private string _c_area_cel;
                private string _c_codigo_postal;
                private string _c_cv;
                private string _c_departamento;
                private string _c_estado_civil;
                private string _c_localidad;
                private string _c_nacionalidad;
                private string _c_nro;
                private string _c_nro_cuil;
                private string _c_nro_doc;
                private string _c_pais;
                private string _c_pais_cel;
                private string _c_password;
                private string _c_piso;
                private string _c_provincia;
                private string _c_sexo;
                private string _c_tipo_document;
                private string _d_apellido;
                private string _d_calle;
                private string _d_email;
                private string _d_localidad;
                private string _d_nombres;
                private string _d_zona;
                private string _json_data;
                private string _te_celular;
                private string _te_nro;

                public DateTime? f_nacim { get { return _f_nacim; } set { _f_nacim = value; } }
                public bool? l_informacion { get { return _l_informacion; } set { _l_informacion = value; } }
                public double? n_remuneracion { get { return _n_remuneracion; } set { _n_remuneracion = value; } }
                public int? oi_estado_civil { get { return _oi_estado_civil; } set { _oi_estado_civil = value; } }
                public int? oi_localidad { get { return _oi_localidad; } set { _oi_localidad = value; } }
                //public int? oi_nacionalidad { get { return _oi_nacionalidad; } set { _oi_nacionalidad = value; } }
                public int? oi_pais { get { return _oi_pais; } set { _oi_pais = value; } }
                public int? oi_provincia { get { return _oi_provincia; } set { _oi_provincia = value; } }
                public int? oi_tipo_documento { get { return _oi_tipo_documento; } set { _oi_tipo_documento = value; } }
                public string c_area { get { return _c_area; } set { _c_area = value; } }
                public string c_area_cel { get { return _c_area_cel; } set { _c_area_cel = value; } }
                public string c_codigo_postal { get { return _c_codigo_postal; } set { _c_codigo_postal = value; } }
                public string c_cv { get { return _c_cv; } set { _c_cv = value; } }
                public string c_departamento { get { return _c_departamento; } set { _c_departamento = value; } }
                public string c_estado_civil { get { return _c_estado_civil; } set { _c_estado_civil = value; } }
                public string c_localidad { get { return _c_localidad; } set { _c_localidad = value; } }
                public string c_nacionalidad { get { return _c_nacionalidad; } set { _c_nacionalidad = value; } }
                public string c_nro { get { return _c_nro; } set { _c_nro = value; } }
                public string c_nro_cuil { get { return _c_nro_cuil; } set { _c_nro_cuil = value; } }
                public string c_nro_doc { get { return _c_nro_doc; } set { _c_nro_doc = value; } }
                public string c_pais { get { return _c_pais; } set { _c_pais = value; } }
                public string c_pais_cel { get { return _c_pais_cel; } set { _c_pais_cel = value; } }
                public string c_password { get { return _c_password; } set { _c_password = value; } }
                public string c_piso { get { return _c_piso; } set { _c_piso = value; } }
                public string c_provincia { get { return _c_provincia; } set { _c_provincia = value; } }
                public string c_sexo { get { return _c_sexo; } set { _c_sexo = value; } }
                public string c_tipo_document { get { return _c_tipo_document; } set { _c_tipo_document = value; } }
                public string d_apellido { get { return _d_apellido; } set { _d_apellido = value; } }
                public string d_calle { get { return _d_calle; } set { _d_calle = value; } }
                public string d_email { get { return _d_email; } set { _d_email = value; } }
                public string d_localidad { get { return _d_localidad; } set { _d_localidad = value; } }
                public string d_nombres { get { return _d_nombres; } set { _d_nombres = value; } }
                public string d_zona { get { return _d_zona; } set { _d_zona = value; } }
                public string json_data { get { return _json_data; } set { _json_data = value; } }
                public string te_celular { get { return _te_celular; } set { _te_celular = value; } }
                public string te_nro { get { return _te_nro; } set { _te_nro = value; } }

                public List<Conocimiento> conocimientosCV = new List<Conocimiento>();
                public List<Estudio> estudiosCV = new List<Estudio>();
                public List<DocumentosDigitales> documentosDigitales = new List<DocumentosDigitales>();
                public List<ExpLaboral> expLaboralCV = new List<ExpLaboral>();
                public List<Idioma> idiomasCV = new List<Idioma>();

            }

            public class InformacionProfesional
            {
                public int _id;
                public string _profession ;
                public string _desired_place_lat;
                public string _desired_place_lng;
                public bool _is_part_time;
                public bool _is_full_time;
                public bool _is_remote;
                public bool _is_temporal;
                public int? _lowest_expected_salary;
                public int? _highest_expected_salary;
                //public bool _wants_job_offers;
                public string _createdAt;
                public string _updatedAt;
                public string _deletedAt;
                public int? _areaId;
                public int? _locationId;

                public int id { get { return _id; } set { _id = value; } }
                public string profession { get { return _profession; } set { _profession = value; } }
                public string desired_place_lat { get { return _desired_place_lat; } set { _desired_place_lat = value; } }
                public string desired_place_lng { get { return _desired_place_lng; } set { _desired_place_lng = value; } }
                public bool is_part_time { get { return _is_part_time; } set { _is_part_time = value; } }
                public bool is_full_time { get { return _is_full_time; } set { _is_full_time = value; } }
                public bool is_remote { get { return _is_remote; } set { _is_remote = value; } }
                public bool is_temporal { get { return _is_temporal; } set { _is_temporal = value; } }
                public int? lowest_expected_salary { get { return _lowest_expected_salary; } set { _lowest_expected_salary = value; } }
                public int? highest_expected_salary { get { return _highest_expected_salary; } set { _highest_expected_salary = value; } }
                //public bool wants_job_offers { get { return _wants_job_offers; } set { _wants_job_offers = value; } }
                public string createdAt { get { return _createdAt; } set { _createdAt = value; } }
                public string updatedAt { get { return _updatedAt; } set { _updatedAt = value; } }
                public string deletedAt { get { return _deletedAt; } set { _deletedAt = value; } }
                public int? areaId { get { return _areaId; } set { _areaId = value; } }
                public int? locationId { get { return _locationId; } set { _locationId = value; } }

                public InformacionProfesional() { }
            }

            public class Conocimiento
            {
                private int? _oi_conocimiento;
                private string _d_conoc_cv;
                private string _c_nivel;
                private string _c_conocimiento;

                public int? oi_conocimiento { get { return _oi_conocimiento; } set { _oi_conocimiento = value; } }
                public string d_conoc_cv { get { return _d_conoc_cv; } set { _d_conoc_cv = value; } }
                public string c_nivel { get { return _c_nivel; } set { _c_nivel = value; } }
                public string c_conocimiento { get { return _c_conocimiento; } set { _c_conocimiento = value; } }

                public Conocimiento(){}
            }

            public class DocumentosDigitales
            {
                private string _c_tipo;
                private string _o_base_64;
                private string _mimeType;
                private string _oi_doc_digital;

                public string c_tipo { get { return _c_tipo; } set { _c_tipo = value; } }
                public string o_base_64 { get { return _o_base_64; } set { _o_base_64 = value; } }
                public string mimeType { get { return _mimeType; } set { _mimeType = value; } }
                public string oi_doc_digital { get { return _oi_doc_digital; } set { _oi_doc_digital = value; } }

                public DocumentosDigitales(){}
            }

            public class Estudio
            {
                private int? _oi_estado_est;
                private int? _oi_estudio;
                private int? _oi_nivel_estudio;
                private string _c_estado_est;
                private string _c_estudio;
                private string _c_nivel_estudio;
                private string _d_otro_est_educ;

                public int? oi_estado_est { get { return _oi_estado_est; } set { _oi_estado_est = value; } }
                public int? oi_estudio { get { return _oi_estudio; } set { _oi_estudio = value; } }
                public int? oi_nivel_estudio { get { return _oi_nivel_estudio; } set { _oi_nivel_estudio = value; } }
                public string c_estado_est { get { return _c_estado_est; } set { _c_estado_est = value; } }
                public string c_estudio { get { return _c_estudio; } set { _c_estudio = value; } }
                public string c_nivel_estudio { get { return _c_nivel_estudio; } set { _c_nivel_estudio = value; } }
                public string d_otro_est_educ { get { return _d_otro_est_educ; } set { _d_otro_est_educ = value; } }

                public Estudio(){}
            }

            public class ExpLaboral
            {
                private int? _oi_area_lab;
                private string _o_tareas;
                private string _d_puesto;
                private string _d_empresa;
                private string _c_area_lab;
                private DateTime? _f_ingreso;
                private DateTime? _f_egreso;

                public int? oi_area_lab { get { return _oi_area_lab; } set { _oi_area_lab = value; } }
                public string o_tareas { get { return _o_tareas; } set { _o_tareas = value; } }
                public string d_puesto { get { return _d_puesto; } set { _d_puesto = value; } }
                public string d_empresa { get { return _d_empresa; } set { _d_empresa = value; } }
                public string c_area_lab { get { return _c_area_lab; } set { _c_area_lab = value; } }
                public DateTime? f_ingreso { get { return _f_ingreso; } set { _f_ingreso = value; } }
                public DateTime? f_egreso { get { return _f_egreso; } set { _f_egreso = value; } }

                public ExpLaboral(){}
            }

            public class Idioma
            {
                private bool? _l_certificado;
                private int? _oi_idioma;
                private int? _oi_nivel_escribe;
                private int? _oi_nivel_habla;
                private int? _oi_nivel_lee;
                private string _c_idioma;
                private string _c_nivel_escribe;
                private string _c_nivel_habla;
                private string _c_nivel_lee;

                public bool? l_certificado { get { return _l_certificado; } set { _l_certificado = value; } }
                public int? oi_idioma { get { return _oi_idioma; } set { _oi_idioma = value; } }
                public int? oi_nivel_escribe { get { return _oi_nivel_escribe; } set { _oi_nivel_escribe = value; } }
                public int? oi_nivel_habla { get { return _oi_nivel_habla; } set { _oi_nivel_habla = value; } }
                public int? oi_nivel_lee { get { return _oi_nivel_lee; } set { _oi_nivel_lee = value; } }
                public string c_idioma { get { return _c_idioma; } set { _c_idioma = value; } }
                public string c_nivel_escribe { get { return _c_nivel_escribe; } set { _c_nivel_escribe = value; } }
                public string c_nivel_habla { get { return _c_nivel_habla; } set { _c_nivel_habla = value; } }
                public string c_nivel_lee { get { return _c_nivel_lee; } set { _c_nivel_lee = value; } }

                public Idioma(){}
            }

            public List<CV> data = new List<CV>();

        }

        public class Chat
        {
            public class Message
            {
                private string _message;
                private int _oi_oferta_lab;
                private bool _read;
                private int _fromId;
                private DateTime _createdAt;
                private DateTime _updatedAt;

                private int? _id;
                private int? _toId;
                private int? _dni_type;
                private int? _dni;
                private int? _offerId;

                public string message { get { return _message; } set { _message = value; } }
                public int oi_oferta_lab { get { return _oi_oferta_lab; } set { _oi_oferta_lab = value; } }
                public bool read { get { return _read; } set { _read = value; } }
                public int fromId { get { return _fromId; } set { _fromId = value; } }
                public DateTime createdAt { get { return _createdAt; } set { _createdAt = value; } }
                public DateTime updatedAt { get { return _updatedAt; } set { _updatedAt = value; } }

                public int? id { get { return _id; } set { _id = value; } }
                public int? toId { get { return _toId; } set { _toId = value; } }
                public int? dni_type { get { return _dni_type; } set { _dni_type = value; } }
                public int? dni { get { return _dni; } set { _dni = value; } }
                public int? offerId { get { return _offerId; } set { _offerId = value; } }
            }

            public List<Message> messages = new List<Message>();
        }

        public class JsonCuando
        {
            private string _from;
            private string _to;

            public string from { get { return _from; } set { _from = value; } }
            public string to { get { return _to; } set { _to = value; } }
        }
        public class JsonPostulaciones
        {
            private bool _all;
            private bool _only_ids;
            private JsonCuando _createdAt;
            private JsonCuando _updatedAt;

            public bool all { get { return _all; } set { _all = value; } }
            public bool only_ids { get { return _only_ids; } set { _only_ids = value; } }
            public JsonCuando createdAt { get { return _createdAt; } set { _createdAt = value; } }
        }
        #endregion

        #region Metodos

        public static void Login()
        {
            if (jobsUrl == "" || jobsUsr == "" || jobsPass == "")
                GetJobsConnection();

            if (jobsUrl != "" && jobsUsr != "" && jobsPass != "")
            {
                Usuario usrAct = new Usuario(jobsUsr, jobsPass);
                try
                {
                    Usuario usuario = Post<Usuario,Usuario>(usrAct, GetHash("Login"));
                    if (usuario != null)
                        token = usuario.token;
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
            else
            {
                throw new Exception("Logueo fallido - Causa: No se encuentran definidos los parametros del Jobs en el archivo de contexto");
            }
        }

        #region Codificadoras

        public static void ActualizarCodificadoras(NomadXML xmlSeleccionadas)
        {
            xmlSeleccionadas = xmlSeleccionadas.FirstChild();
            int i = 0;
            bool todos = xmlSeleccionadas.GetAttrBool("todos");
            try
            {
                Login();
                for (NomadXML cod = xmlSeleccionadas.FirstChild(); cod != null; cod = cod.Next())
                {
                    if (todos || cod.GetAttr("value") == "1")
                    {
                        ActualizarCodificadora(cod.GetAttr("id"));
                        objBatch.SetPro(0, 100, xmlSeleccionadas.ChildLength, i + 1);
                        i++;
                    }
                }
                objBatch.Log("¡Se han actualizado todas las codificadoras exitosamente!");
            }
            catch (Exception e)
            {
                objBatch.Err("Error al actualizar codificadora: " + e.Message);
            }
        }

        public static void ActualizarCodificadora(string tipo)
        {
            objBatch.Log("Actualizando codificadora '" + tipo + "'...");

            Codificadora codificadora = new Codificadora();
            codificadora.codificadoras = new List<Codificadora.Data>();

            Codificadora codificadoraAux = null;

            ArrayList lista;
            NomadXML xmlDatos;

            NomadXML PARAM = new NomadXML("DATA");
            PARAM.SetAttr("tipoCodificadora", tipo);

            xmlDatos = Nomad.NSystem.Base.NomadEnvironment.QueryNomadXML(NucleusRH.Base.SeleccionDePostulantes.Jobs.JOBS.Resources.WEB_CODIFICADORAS, PARAM.ToString());
            lista = (ArrayList)xmlDatos.FirstChild().GetElements("ROW");
            if (xmlDatos.FirstChild().GetAttr("claseParent") != "")
            {
                try
                {
                    codificadoraAux = Get<Codificadora>(GetHash("TipoNucleus", xmlDatos.FirstChild().GetAttr("claseParent")));
                }
                catch (Exception e)
                {
                    objBatch.Err(e.Message);
                }
            }
            foreach (object l in lista)
            {
                NomadXML row = (NomadXML)l;
                Codificadora.Data newData = Crear<Codificadora.Data>(row);
                if(codificadoraAux != null)
                    newData.parentId = GetParentID(codificadoraAux, newData.parentId);

                codificadora.codificadoras.Add(newData);
            }

            try
            {
                Post<Codificadora>(codificadora,GetHash("TipoNucleus",tipo));
            }
            catch (Exception e)
            {
                objBatch.Err("Fallo la actualización de la codificadora '" + tipo + "' - Causa: " + e.Message);
            }

        }

        public static void AUTO_ActualizarCodificadoras()
        {
            objBatch = NomadBatch.GetBatch("Proceso Jobs", "AUTO_ActualizarCodificadoras(...)");
            NomadLog.Debug("~ Iniciando proceso AUTOMATICO de actualizacion de Codificadoras ~");

            NomadXML xmlCodificadoras = NomadEnvironment.QueryNomadXML(Resources.WEB_CODIFICADORAS, "");
            xmlCodificadoras.FirstChild().SetAttr("todos", "1");
            ActualizarCodificadoras(xmlCodificadoras);
        }
        #endregion

        #region Ofertas Laborales
        //cuando se testee chequear la fecha de cierre, cuando tiene que ir vacia.
        public static void CrearOfertasLaborales(string pFechaDesde, string pFechaHasta)
        {
            int cantidad = 0;
            int cantidadPublicada = 0;
            bool publicada = false;
            objBatch = NomadBatch.GetBatch("Proceso Jobs", "CrearOfertasLaborales");
            try
            {
                Login();
                NucleusRH.Base.SeleccionDePostulantes.Avisos.AVISO aviso;
                int i = 1;

                NomadXML param = new NomadXML("DATA");
                param.SetAttr("f_desde", pFechaDesde);
                param.SetAttr("f_hasta", pFechaHasta);

                NomadXML xmlAvisos = Nomad.NSystem.Base.NomadEnvironment.QueryNomadXML(NucleusRH.Base.SeleccionDePostulantes.Jobs.JOBS.Resources.QRY_AVISOS, param.ToString()).FirstChild();

                NomadLog.Debug("~ Iniciando proceso de migración de Avisos (" + xmlAvisos.ChildLength + ")~");

                for (NomadXML xmlA = xmlAvisos.FirstChild();xmlA != null; xmlA = xmlA.Next())
                {
                    Ofertas.Oferta oferta = Crear<Ofertas.Oferta>(xmlA);
                    try
                    {
                        objBatch.SetMess("Procesando Aviso " + i + "/" + xmlAvisos.ChildLength);

                        oferta = Post<Ofertas.Oferta, Ofertas.Oferta>(oferta,GetHash("Aviso"));

                        aviso = Avisos.AVISO.Get(oferta.external_id,false);
                        aviso.oi_aviso_jobs = oferta.id.Value;

                        if (xmlA.FindElement("nucleus_offer").GetAttr("l_publicacion_web") =="1" && oferta.f_cierre.GetValueOrDefault() > DateTime.Now.Date)
                        {
                            cantidadPublicada++;
                            publicada = true;
                        }
                        else
                        {
                            publicada = false;
                        }

                        objBatch.Log("Se " + (!publicada ? "des" : "") + "publico la oferta: " + xmlA.FindElement("nucleus_offer").GetAttr("c_oferta_lab") + " - " + xmlA.FindElement("nucleus_offer").GetAttr("d_oferta_lab"));

                        NomadEnvironment.GetCurrentTransaction().Save(aviso);
                        cantidad++;
                    }
                    catch (Exception e)
                    {
                        objBatch.Err(e.Message);
                    }
                    objBatch.SetPro(0, 100, xmlAvisos.ChildLength, ++i);
                }

                objBatch.Log("Avisos procesados: " + cantidad);
                objBatch.Log("Avisos publicados: " + cantidadPublicada);
                objBatch.Log("Avisos despublicados: " + (cantidad - cantidadPublicada));
                objBatch.Log("Fin de la migración de Avisos");
                objBatch.SetPro(100);

            }
            catch (Exception e)
            {
                objBatch.Err("Error en el proceso de envio de avisos: " + e.Message);
            }
        }

        public static void AUTO_CrearOfertasLaborales()
        {
            objBatch = NomadBatch.GetBatch("Proceso Jobs", "AUTO_CrearOfertasLaborales()");

            NomadLog.Debug("~ Iniciando proceso AUTOMÁTICO de migración de Avisos ~");

            string d_valor = NomadEnvironment.QueryValue("ORG26_PARAMETROS", "d_valor", "c_parametro", "f_ult_actual_AV", "ORG26_PARAMETROS.c_modulo=\\'POS\\'", false);
            string f_desde = d_valor != "" ? d_valor : new DateTime(1899, 01, 01).ToString("yyyyMMdd");
            string f_hasta = DateTime.Now.ToString("yyyyMMdd") ;

            CrearOfertasLaborales(f_desde, f_hasta);
            GuardarParametro("f_ult_actual_AV", f_hasta);
        }

        #endregion

        #region CVs
        private static bool bolGuardaDocumentos = false;
        private static string strC_NivelEstudioDefault = "0";
        private static string strOINivelEstudioDefault = "0";
        public static void GuardarCVs(string pFechaDesde, string pFechaHasta)
        {
			int cantidad = 0;
            objBatch = NomadBatch.GetBatch("Proceso Jobs", "GuardarCVs(...)");

            NomadLog.Debug("Inicia GuardarCVs");

            string f_desde = pFechaDesde.Substring(0, 4) + "-" + pFechaDesde.Substring(4, 2) + "-" + pFechaDesde.Substring(6, 2);
            string f_hasta = pFechaHasta.Substring(0, 4) + "-" + pFechaHasta.Substring(4, 2) + "-" + pFechaHasta.Substring(6, 2);

            //Recupera parámetros
            bolGuardaDocumentos = GetParametro("GuardarDocumentosDig", "1") == "1";
            strOINivelEstudioDefault = GetNivelEstudioDefault();

            if (!bolGuardaDocumentos)
                objBatch.Wrn("El proceso está parametrizado para no gaurdar documentos digitales (parámetro 'GuardarDocumentosDig').");

            try
            {
                objBatch.SetPro(10);
                Login();

                CVs nuevos = Get<CVs>(GetHash("CVsNuevosIds", new string[] { f_desde, f_hasta }));
                CVs modificados = Get<CVs>(GetHash("CVsModificadosIds", new string[] { f_desde, f_hasta }));

                nuevos.data.AddRange(modificados.data);

				Hashtable IdsCvs = GuardarIdsEnHash(nuevos);

				NomadLog.Debug("Recorre la lista IdsCvs");

                foreach(string id in IdsCvs.Keys)
                {
                    CVs.CV CvJobs = null;

                    try
                    {
                        objBatch.SetPro(0, 100, IdsCvs.Count, cantidad);
						
						NomadLog.Debug("Analizando el id de CV '" + id + "'");
						
                        string idCVNucleus = NomadEnvironment.QueryValue("SDP01_CV", "oi_cv", "c_cv", id, "", false);
                        SeleccionDePostulantes.CVs.CV CvNucleus = idCVNucleus != "" ? SeleccionDePostulantes.CVs.CV.Get(idCVNucleus, false) : new SeleccionDePostulantes.CVs.CV();

                        CvJobs = (CVs.CV)IdsCvs[id];
                        if (CvJobs.email == "")
                        {
                            objBatch.Err("El CV de documento " + CvJobs.dni + " no tiene email, se rechaza el registro");
                            continue;
                        }
                        
						if (CvJobs.username == "nucleus") continue;

                        CvNucleus = CopiarCV(CvJobs, CvNucleus);

                        NomadEnvironment.GetCurrentTransaction().Save(CvNucleus);
						NomadLog.Debug("El CV se actualizo correctamente '" + id + "'");
						
                        cantidad++;
                    }
                    catch(Exception e)
                    {
                        NomadLog.Debug("Stack Trace " + e.StackTrace);
                        objBatch.Log("Error al migrar CV '" + id + "' - " + e.Message);
                        continue;
                    }
                }

				NomadLog.Debug("Finaliza GuardarCVs");
                objBatch.SetPro(100);
                objBatch.Log("Se han migrado " + cantidad + " CVs exitosamente (de un total de " + IdsCvs.Count + ")");

            }
            catch (Exception e)
            {
                objBatch.Err("Error en el proceso de recepcion de CVs: " + e.Message);
                
            }
        }

        static Hashtable hashTipoDoc = new Hashtable();
        static Codificadora areas = null;
        /// <summary>
        /// Transfiere los datos del CV de Jobs al CV de NucleusRH
        /// </summary>
        /// <param name="CvJobs"></param>
        /// <param name="CvNucleus"></param>
        /// <returns></returns>
        private static SeleccionDePostulantes.CVs.CV CopiarCV(CVs.CV CvJobs, SeleccionDePostulantes.CVs.CV CvNucleus)
        {
            
			NomadLog.Debug("Inicia CopiarCV");
			
			CVs.NucleusUser datosParaNucleus = CvJobs.nucleus_user;

            CvNucleus.c_cv = BuscarTipoDoc(CvJobs.nucleus_user.oi_tipo_documento) +CvJobs.nucleus_user.c_nro_doc;
            CvNucleus.c_estado = "A";
            CvNucleus.f_alta_cv = CvJobs.createdAt;
            CvNucleus.f_estado = DateTime.Now;//CvJobs.updatedAt.Value;
            CvNucleus.l_alta_web = true;
            CvNucleus.f_actualizacion = CvJobs.updatedAt.Value;
            CvNucleus.d_apellido = GetTexto(datosParaNucleus.d_apellido, 100);
            CvNucleus.d_nombres = GetTexto(datosParaNucleus.d_nombres, 100);
            CvNucleus.d_ape_y_nom = GetTexto(CvNucleus.d_apellido + ", " + CvNucleus.d_nombres,200);
            CvNucleus.oi_tipo_documento = datosParaNucleus.oi_tipo_documento.ToString();
            CvNucleus.c_nro_doc = GetTexto(datosParaNucleus.c_nro_doc, 30);
            CvNucleus.c_password = "clave";
            CvNucleus.f_nacimNull = datosParaNucleus.f_nacim == null;
			//Comento esta línea, ya que creaba una fecha incorrecta cuando viene null el campo f_nacim
			//CvNucleus.f_nacim = datosParaNucleus.f_nacim != null ? DateTime.Parse(datosParaNucleus.f_nacim.ToString()) : new DateTime();
			if(!CvNucleus.f_nacimNull){				
				CvNucleus.f_nacim = DateTime.Parse(datosParaNucleus.f_nacim.ToString());
			}            
            CvNucleus.c_sexo = CvNucleus.c_sexo!="" ? CvNucleus.c_sexo : "SE";
            CvNucleus.d_email = GetTexto(datosParaNucleus.d_email, 100);
            CvNucleus.c_pais = GetTexto(datosParaNucleus.c_pais_cel, 30);
            CvNucleus.c_area = GetTexto(datosParaNucleus.c_area_cel,30);
            CvNucleus.te_nro = GetTexto(datosParaNucleus.te_celular, 100);
            CvNucleus.c_pais_cel = GetTexto(datosParaNucleus.c_pais,  30);
            CvNucleus.c_area_cel = GetTexto(datosParaNucleus.c_area,  30);
            CvNucleus.te_celular = GetTexto(datosParaNucleus.te_nro,  50);
            CvNucleus.d_calle = GetTexto(datosParaNucleus.d_calle,  100);
            CvNucleus.c_nro = GetTexto(datosParaNucleus.c_nro,  30);
            CvNucleus.c_piso = GetTexto(datosParaNucleus.c_piso,  30);
            CvNucleus.c_departamento = GetTexto(datosParaNucleus.c_departamento,  30);
            CvNucleus.oi_pais = datosParaNucleus.oi_pais == null ? null : datosParaNucleus.oi_pais.ToString();
            CvNucleus.oi_provincia = datosParaNucleus.oi_provincia == null ? null : datosParaNucleus.oi_provincia.ToString();
            CvNucleus.n_remuneracion = CvJobs.users_professional.lowest_expected_salary == null ? 0 : CvJobs.users_professional.lowest_expected_salary.GetValueOrDefault();
            CvNucleus.AREAS_CV.Clear();
            GuardarAreaDeInteres(CvNucleus,CvJobs.users_professional.areaId);
            CvNucleus.oi_tipo_jor_lab = BuscarTipoJornada(CvJobs.users_professional);
            CvNucleus.l_maillist = (CvJobs.can_receive_emails.HasValue) ? CvJobs.can_receive_emails.Value : false;
            CvNucleus.oi_sit_laboral = DeterminarSituacionLaboral(CvJobs.nucleus_user.expLaboralCV);
            CvNucleus.t_cv = GetTexto(CvJobs.about,1000);
            CvNucleus.oi_personal = NomadEnvironment.QueryValue("PER01_PERSONAL", "oi_personal", "c_personal",BuscarTipoDoc(datosParaNucleus.oi_tipo_documento) + datosParaNucleus.c_nro_doc, "", false);

            if (datosParaNucleus.oi_localidad != null)
            {
                NucleusRH.Base.Organizacion.Localidades.LOCALIDAD tempLocalidad;
                try
                {
                    tempLocalidad = NucleusRH.Base.Organizacion.Localidades.LOCALIDAD.Get(datosParaNucleus.oi_localidad.ToString(), false);
                }
                catch (Exception e)
                {
                    throw new Exception("No se encontró la 'Localidad' seleccionado", e);
                }


                CvNucleus.oi_localidad = tempLocalidad.Id;
                CvNucleus.d_localidad = tempLocalidad.d_localidad;
                CvNucleus.c_codigo_postal = tempLocalidad.c_postal;
            }

            GuardarEstudiosCV(CvNucleus, datosParaNucleus);
            GuardarExperienciaCV(CvNucleus, datosParaNucleus);
            GuardarIdiomasCV(CvNucleus, datosParaNucleus);
            GuardarConocimientosInfCV(CvNucleus, datosParaNucleus);

            //Se pregunta si se guardan los documentos
            if (bolGuardaDocumentos)
            {
                string oiDocumento;
                oiDocumento = GuardarDocumento(datosParaNucleus, "CV", CvNucleus.oi_doc_digital, CvNucleus.oi_doc_digitalNull);
                if (oiDocumento != null) CvNucleus.oi_doc_digital = oiDocumento; else CvNucleus.oi_doc_digitalNull = true;

                oiDocumento = null;
                oiDocumento = GuardarDocumento(datosParaNucleus, "FOTO", CvNucleus.oi_foto, CvNucleus.oi_fotoNull);
                if (oiDocumento != null) CvNucleus.oi_foto = oiDocumento; else CvNucleus.oi_fotoNull = true;
            }

			
			NomadLog.Debug("Finaliza CopiarCV");
			
            return CvNucleus;
        }

          static Hashtable HashTipoJornada = null;

        private static string BuscarTipoJornada(CVs.InformacionProfesional informacionProfesional)
        {
            string codigo = "";
            if(HashTipoJornada == null)
                HashTipoJornada = NomadEnvironment.QueryHashtableValue(SeleccionDePostulantes.Tipos_Jornadas_Laborales.TIPO_JOR_LAB.Resources.INFOTIPOJOR,"","COD","ID",false);

            if (informacionProfesional.is_temporal)
                codigo = "04";
            else if (informacionProfesional.is_remote)
                codigo = "03";
            else if (informacionProfesional.is_part_time)
                codigo = "02";
            else if (informacionProfesional.is_full_time)
                codigo = "01";

            return codigo != "" && HashTipoJornada.ContainsKey("JOBS_" + codigo) ? HashTipoJornada["JOBS_"+codigo].ToString() : "";
        }

        private static void GuardarAreaDeInteres(SeleccionDePostulantes.CVs.CV CvNucleus, int? idArea)
        {

            if (idArea != null)
            {
                if (areas == null)
                    areas = Get<Codificadora>(GetHash("TipoNucleus", new string[] { "AREAS_LAB" }));

                string id = idArea.ToString();
                string idEncontrado = null;
                foreach (Codificadora.Data c in areas.data)
                {
                    if (c.id == id)
                    {
                        idEncontrado = c.external_id;
                        break;
                    }
                }
                if (idEncontrado != null)
                {
                    SeleccionDePostulantes.Puestos_Exp.PUESTO_EXP puestoExpCv = BuscarPuestoExperiencia(idEncontrado);
                    string oiArea = puestoExpCv.oi_area;

                    if (CvNucleus.AREAS_CV.GetByAttribute("oi_area", oiArea) == null)// && oiArea != "0")
                    {
                        SeleccionDePostulantes.CVs.AREA_CV area = new SeleccionDePostulantes.CVs.AREA_CV();
                        area.oi_area = oiArea;
                        CvNucleus.AREAS_CV.Add(area);
                    }
                }

            }

        }

        private static string BuscarTipoDoc(int? oi_tipo_doc)
        {
			string strReturn = "";
		
			if (oi_tipo_doc != null)
            {
				strReturn = NomadEnvironment.QueryValue("PER20_TIPOS_DOC", "c_tipo_documento", "oi_tipo_documento", oi_tipo_doc.ToString(), "", true);
				/*
				if (hashTipoDoc.ContainsKey(oi_tipo_doc))
                    return hashTipoDoc[oi_tipo_doc].ToString();
                else
                {
                    Personal.Tipos_Documento.TIPO_DOCUMENTO tipo_doc = Personal.Tipos_Documento.TIPO_DOCUMENTO.Get(oi_tipo_doc.Value);
                    hashTipoDoc.Add(oi_tipo_doc.Value,tipo_doc.c_tipo_documento);
                    return tipo_doc.c_tipo_documento;
                }
				*/
            }
            return strReturn;
        }

        private static Hashtable hashSituacionesLaborales = null;
        private static string DeterminarSituacionLaboral(List<CVs.ExpLaboral> listaExperienciaLaboral)
        {
            string codigoSitLab = "";
            int cantidadTrabaja = 0;
            foreach(CVs.ExpLaboral exp in listaExperienciaLaboral)
            {
                if (exp.f_egreso == null)
                    cantidadTrabaja++;
            }
            if(cantidadTrabaja == 0)
                codigoSitLab = "JOBS_03";
            if(cantidadTrabaja == 1)
                codigoSitLab = "JOBS_01";
            if(cantidadTrabaja > 1)
                codigoSitLab = "JOBS_02";
            if(listaExperienciaLaboral.Count == 0)
                codigoSitLab = "JOBS_04";

            if (hashSituacionesLaborales == null)
                hashSituacionesLaborales = new Hashtable();
            return hashSituacionesLaborales.ContainsKey(codigoSitLab) ? hashSituacionesLaborales[codigoSitLab].ToString() : NomadEnvironment.QueryValue("SDP17_SIT_LABORALES", "oi_sit_laboral", "c_sit_laboral", codigoSitLab, "", false);
        }

        private static string GuardarDocumento(CVs.NucleusUser datosParaNucleus,string tipo,string oiBusqueda,bool oiNull)
        {
            NucleusRH.Base.SeleccionDePostulantes.DocumentosDigitales.HEAD digNuevo = new Base.SeleccionDePostulantes.DocumentosDigitales.HEAD();
            NucleusRH.Base.SeleccionDePostulantes.DocumentosDigitales.HEAD digAnterior = !oiNull ? NucleusRH.Base.SeleccionDePostulantes.DocumentosDigitales.HEAD.Get(oiBusqueda) : null;

            if (digAnterior != null)
                digAnterior.SetStatusDelete();

            CVs.DocumentosDigitales doc = null;
            foreach (CVs.DocumentosDigitales pobjDocDig in datosParaNucleus.documentosDigitales)
            {
                if (pobjDocDig.c_tipo == tipo)
                {
                    doc = pobjDocDig;
                    break;
                }
            }
            if(doc != null)
            {
                string nombre = doc.oi_doc_digital != null && doc.oi_doc_digital != "" ? doc.oi_doc_digital : doc.c_tipo.ToUpper() + "." + doc.mimeType;

                MemoryStream memory = new MemoryStream();
                string strDocId = "";

                try
                {

                    byte[] arr = System.Convert.FromBase64String(doc.o_base_64);
                    memory.Write(arr, 0, arr.Length);
                    memory.Flush();
                    memory.Position = 0;
                    BINFile docu = NomadProxy.GetProxy().BINService().PutFile("NucleusRH.Base.SeleccionDePostulantes.DocumentosDigitales.HEAD", nombre, memory);
                    strDocId = docu.Id;

                }
                catch (Exception e) {
                    throw new Exception("Se produjo un error guardando el documento '" + nombre + "' para el CV '" + datosParaNucleus.c_cv + "'.", e);
                }
                finally {
                    memory.Close();
                }

                return strDocId;
            }

            return null;
        }

        private static void GuardarConocimientosInfCV(SeleccionDePostulantes.CVs.CV CvNucleus, CVs.NucleusUser datosParaNucleus)
        {
            List<string> oisConocimientos = new List<string>();
            foreach (CVs.Conocimiento pobjConocimientos in datosParaNucleus.conocimientosCV)
            {
                NucleusRH.Base.SeleccionDePostulantes.CVs.INFORMATICA_CV informaticaNucleus = (SeleccionDePostulantes.CVs.INFORMATICA_CV)CvNucleus.INFORMATICA.GetByAttribute("oi_informatica", pobjConocimientos.oi_conocimiento.ToString());
                if (informaticaNucleus == null)
                {
                    informaticaNucleus = new SeleccionDePostulantes.CVs.INFORMATICA_CV();
                    CvNucleus.INFORMATICA.Add(informaticaNucleus);
                }

                informaticaNucleus.c_nivel = GetTexto(pobjConocimientos.c_nivel,  30);
                informaticaNucleus.o_detalle = GetTexto(pobjConocimientos.d_conoc_cv,  4000);
                informaticaNucleus.oi_informatica = pobjConocimientos.oi_conocimiento.ToString();

                oisConocimientos.Add(informaticaNucleus.Id);
            }

            foreach (NucleusRH.Base.SeleccionDePostulantes.CVs.INFORMATICA_CV objNucInformaticaCV in CvNucleus.INFORMATICA)
            {
                if (!oisConocimientos.Contains(objNucInformaticaCV.Id))
                {
                    NucleusRH.Base.SeleccionDePostulantes.CVs.INFORMATICA_CV informatica = (NucleusRH.Base.SeleccionDePostulantes.CVs.INFORMATICA_CV)CvNucleus.INFORMATICA.GetById(objNucInformaticaCV.Id);
                    informatica.SetStatusDelete();
                }
            }
        }

        private static void GuardarIdiomasCV(SeleccionDePostulantes.CVs.CV CvNucleus, CVs.NucleusUser datosParaNucleus)
        {
            List<string> oisIdiomas = new List<string>();
            foreach (CVs.Idioma pobjIdiomas in datosParaNucleus.idiomasCV)
            {
                NucleusRH.Base.SeleccionDePostulantes.CVs.IDIOMA_CV idiomaNucleus = (SeleccionDePostulantes.CVs.IDIOMA_CV)CvNucleus.IDIOMAS_CV.GetByAttribute("oi_idioma", pobjIdiomas.oi_idioma.ToString());
                if (idiomaNucleus == null)
                {
                    idiomaNucleus = new SeleccionDePostulantes.CVs.IDIOMA_CV();
                    CvNucleus.IDIOMAS_CV.Add(idiomaNucleus);
                }

                idiomaNucleus.oi_idioma = pobjIdiomas.oi_idioma.ToString();
                idiomaNucleus.c_nivel_escribe = pobjIdiomas.oi_nivel_escribe.ToString();
                idiomaNucleus.c_nivel_habla = pobjIdiomas.oi_nivel_habla.ToString();
                idiomaNucleus.c_nivel_lee = pobjIdiomas.oi_nivel_lee.ToString();

                oisIdiomas.Add(idiomaNucleus.Id);
            }

            foreach (NucleusRH.Base.SeleccionDePostulantes.CVs.IDIOMA_CV objNucIdiomaCV in CvNucleus.IDIOMAS_CV)
            {
                if (!oisIdiomas.Contains(objNucIdiomaCV.Id))
                {
                    NucleusRH.Base.SeleccionDePostulantes.CVs.IDIOMA_CV idioma = (NucleusRH.Base.SeleccionDePostulantes.CVs.IDIOMA_CV)CvNucleus.IDIOMAS_CV.GetById(objNucIdiomaCV.Id);
                    idioma.SetStatusDelete();
                }
            }

        }

        private static void GuardarExperienciaCV(SeleccionDePostulantes.CVs.CV CvNucleus, CVs.NucleusUser datosParaNucleus)
        {
            List<string> oisAreasInteres = new List<string>();

            CvNucleus.EXPERIENCIA.Clear();
            foreach (CVs.ExpLaboral pobjExpLab in datosParaNucleus.expLaboralCV)
            {
                if(pobjExpLab.oi_area_lab != null)
                {
                    NucleusRH.Base.SeleccionDePostulantes.CVs.EXPERIENCIA expNucleus = new SeleccionDePostulantes.CVs.EXPERIENCIA();
                    CvNucleus.EXPERIENCIA.Add(expNucleus);

                    expNucleus.d_empresa = GetTexto(pobjExpLab.d_empresa, 100);
                    //expNucleus.d_actividad = GetTexto(pobjExpLab.d_puesto, 100);
                    expNucleus.f_ingreso = pobjExpLab.f_ingreso.Value;
                    expNucleus.f_egreso = pobjExpLab.f_egreso == null ? new DateTime(1899, 12, 30) : pobjExpLab.f_egreso.Value;
                    expNucleus.f_egresoNull = pobjExpLab.f_egreso == null;
                    expNucleus.l_actual = pobjExpLab.f_egreso == null;
                    expNucleus.o_tareas =  GetTexto(pobjExpLab.d_puesto, 100) + ": "+ GetTexto(pobjExpLab.o_tareas, 898);
                    expNucleus.oi_puesto_exp = pobjExpLab.oi_area_lab != null ? pobjExpLab.oi_area_lab.ToString() : null;
                    SeleccionDePostulantes.Puestos_Exp.PUESTO_EXP puestoExpCv = BuscarPuestoExperiencia(expNucleus.oi_puesto_exp);
                    expNucleus.oi_area_exp = puestoExpCv.oi_area_exp;

                    if ( puestoExpCv.oi_area != "" && puestoExpCv.oi_area != null && /*puestoExpCv.oi_area != "0"  &&*/ !oisAreasInteres.Contains(puestoExpCv.oi_area ) )
                        oisAreasInteres.Add(puestoExpCv.oi_area);
                }
            }

            foreach (string oiArea in oisAreasInteres)
            {
                if (CvNucleus.AREAS_CV.GetByAttribute("oi_area", oiArea) == null)
                {
                    SeleccionDePostulantes.CVs.AREA_CV area = new SeleccionDePostulantes.CVs.AREA_CV();
                    area.oi_area = oiArea;
                    CvNucleus.AREAS_CV.Add(area);
                }
            }
        }

        static Hashtable HashPuestoExperiencia = new Hashtable();
        private static SeleccionDePostulantes.Puestos_Exp.PUESTO_EXP BuscarPuestoExperiencia(string oi_puesto_exp)
        {
            if (HashPuestoExperiencia.ContainsKey(oi_puesto_exp))
                return (SeleccionDePostulantes.Puestos_Exp.PUESTO_EXP)HashPuestoExperiencia[oi_puesto_exp];
            else
            {
                SeleccionDePostulantes.Puestos_Exp.PUESTO_EXP puesto_exp;
                try
                {
                    puesto_exp = SeleccionDePostulantes.Puestos_Exp.PUESTO_EXP.Get(oi_puesto_exp, false);
                }
                catch (Exception e) {
                    throw new Exception("No se encontró el 'Puesto Experiencia Laboral'", e);
                }
                HashPuestoExperiencia.Add(oi_puesto_exp, puesto_exp);
                return puesto_exp;
                }

        }

        static Hashtable HashEstudios = new Hashtable();
        private static void GuardarEstudiosCV(SeleccionDePostulantes.CVs.CV CvNucleus, CVs.NucleusUser datosParaNucleus)
        {
            CvNucleus.ESTUDIOS_CV.Clear();
            foreach (CVs.Estudio pObjEst in datosParaNucleus.estudiosCV)
            {
                SeleccionDePostulantes.CVs.ESTUDIO_CV estudio = new SeleccionDePostulantes.CVs.ESTUDIO_CV();
                CvNucleus.ESTUDIOS_CV.Add(estudio);

                if (pObjEst.oi_nivel_estudio != null) {
                    estudio.oi_nivel_estudio = pObjEst.oi_nivel_estudio.ToString();
                } else {
                    estudio.oi_nivel_estudio = strOINivelEstudioDefault;
                    objBatch.Wrn("El CV con código '" + CvNucleus.c_cv + "' no tiene nivel estudio cargado. Se le asigna un nivel por defecto (Código '" + strC_NivelEstudioDefault + "').");
                }
                estudio.oi_estudio = pObjEst.oi_estudio != null ? pObjEst.oi_estudio.ToString() : null;
                estudio.c_estado = pObjEst.oi_estado_est != null ? pObjEst.oi_estado_est.ToString() : null;
                estudio.d_institucion = GetTexto(pObjEst.d_otro_est_educ,100);
                if (estudio.oi_estudio != null && estudio.oi_estudio != "")
                {
                    NucleusRH.Base.Organizacion.Estudios.ESTUDIO est = BuscarEstudio(estudio.oi_estudio);
                    estudio.oi_area_est = est.oi_area_est;
                    estudio.d_nom_carrera = est.d_estudio;
                }
                else
                {
                    estudio.oi_area_est = NomadEnvironment.QueryValue("ORG43_AREAS_EST", "oi_area_est", "c_area_est", "OA", "", false);
                }

            }
        }

        private static NucleusRH.Base.Organizacion.Estudios.ESTUDIO BuscarEstudio(string oi_estudio)
        {
            if (oi_estudio != null && oi_estudio != "")
            {
                if (HashEstudios.Contains(oi_estudio))
                    return (NucleusRH.Base.Organizacion.Estudios.ESTUDIO)HashEstudios[oi_estudio];
                else
                {
                    NucleusRH.Base.Organizacion.Estudios.ESTUDIO est;
                    try
                    {
                        est = NucleusRH.Base.Organizacion.Estudios.ESTUDIO.Get(oi_estudio, false);
                    }
                    catch (Exception e)
                    {
                        throw new Exception("No se encontró el 'Estudio' seleccionado", e);
                    }

                    HashEstudios.Add(oi_estudio, est);
                    return est;
                }
            }
            return null;
        }

        /*
        private static SeleccionDePostulantes.CVs.ESTUDIO_CV BuscarEstudio(SeleccionDePostulantes.CVs.CV CvNucleus, CVs.Estudio pObjEstJobs)
        {
            foreach (NucleusRH.Base.SeleccionDePostulantes.CVs.ESTUDIO_CV objNucEstudioCV in CvNucleus.ESTUDIOS_CV)
            {
                if (objNucEstudioCV.oi_estudio != null && objNucEstudioCV.oi_estudio == pObjEstJobs.oi_estudio.ToString() && objNucEstudioCV.oi_nivel_estudio == pObjEstJobs.oi_nivel_estudio.ToString())
                {
                    return objNucEstudioCV;
                }

                if (objNucEstudioCV.oi_estudio == null && objNucEstudioCV.oi_nivel_estudio == pObjEstJobs.oi_nivel_estudio.ToString())
                {
                    return objNucEstudioCV;
                }
            }
            return null;
        }
        */
        private static Hashtable GuardarIdsEnHash(CVs nuevos)
        {
            Hashtable hash = new Hashtable();
            Hashtable hashIds = new Hashtable();
			
			NomadLog.Debug("Inicia GuardarIdsEnHash");

			foreach (CVs.CV cv in nuevos.data)
            {
                if (!hashIds.ContainsKey(cv.id))
                {
                    CVs.CV CvJobs = null;
                    try
                    {
						CvJobs = Get<CVs>(GetHash("GetCV", new string[] { cv.id.ToString() })).data[0];

						//Recupera el tipo de documento
						if (CvJobs.nucleus_user.oi_tipo_documento != null && CvJobs.nucleus_user.c_nro_doc != null)
						{
							string strTipoDoc = BuscarTipoDoc(CvJobs.nucleus_user.oi_tipo_documento);
							string id = strTipoDoc + CvJobs.nucleus_user.c_nro_doc;

							if (strTipoDoc == null)
								throw new Exception("El CV con número de documento '" + CvJobs.nucleus_user.c_nro_doc + "' no tiene un tipo de documento válido");

							hash.Add(id, CvJobs);
							hashIds.Add(cv.id, cv.id);
						}

					}
                    catch(Exception e)
                    {
						NomadLog.Error("Error al obtener CV de jobs");
						NomadLog.Error("Error: " + e.Message);
						NomadLog.Error("cv.id: " + cv.id);
                        objBatch.Err("Error al obtener CV de jobs '" + cv.id + "' - " + e.Message);
                        continue;
                    }
                    
                }
            }
			
			NomadLog.Debug("Finaliza GuardarIdsEnHash");
			
            return hash;
        }

        public static void AUTO_GuardarCVs()
        {
            objBatch = NomadBatch.GetBatch("Proceso Jobs", "AUTO_GuardarCVs()");
            NomadLog.Debug("~ Iniciando proceso AUTOMÁTICO de migración de CVs ~");

            string d_valor = NomadEnvironment.QueryValue("ORG26_PARAMETROS", "d_valor", "c_parametro", "f_ult_actual_CV", "ORG26_PARAMETROS.c_modulo=\\'POS\\'", false);
            string f_desde = d_valor != "" ? d_valor : new DateTime(1899, 01, 01).ToString("yyyyMMdd");
            string f_hasta = DateTime.Now.ToString("yyyyMMdd") ;

            GuardarCVs(f_desde, f_hasta);
            GuardarParametro("f_ult_actual_CV", f_hasta);
        }

        #endregion

        #region Postulaciones

        public static void GuardarPostulaciones(string pFechaDesde, string pFechaHasta)
        {
            objBatch = NomadBatch.GetBatch("Proceso Jobs", "GuardarPostulaciones(...)");
            int postuCorrectas = 0;
            int postuIncorrectas = 0;

            string f_desde = pFechaDesde.Substring(0, 4) + "-" + pFechaDesde.Substring(4, 2) + "-" + pFechaDesde.Substring(6, 2);
            string f_hasta = pFechaHasta.Substring(0, 4) + "-" + pFechaHasta.Substring(4, 2) + "-" + pFechaHasta.Substring(6, 2);
            Hashtable hashIdsCvs = new Hashtable();
            Hashtable hashIdsOfertas = new Hashtable();
            Hashtable hashIdsCvsNucleus = new Hashtable();
            try
            {
                objBatch.SetPro(10);
                Login();

                JsonCuando jsonCuando = new JsonCuando();
                jsonCuando.from = f_desde;
                jsonCuando.to = f_hasta;
                JsonPostulaciones jsonPostulaciones = new JsonPostulaciones();
                jsonPostulaciones.createdAt = jsonCuando;
                jsonPostulaciones.all = true;

                Applications postulaciones = Post<JsonPostulaciones, Applications>(jsonPostulaciones, GetHash("GetPostulaciones"));

                CVs.CV cvActual = null;
                Ofertas.Oferta ofActual = null;
                SeleccionDePostulantes.CVs.CV cvNucleus = null;
                int pos = 0;
                int cantidadTotal = postulaciones.data.Count;
                foreach (Applications.Apply postu in postulaciones.data)
                {
                    objBatch.SetPro(0, 100, cantidadTotal + 20, pos);
                    pos++;

                    try
                    {
                        cvActual = BuscarCVJobs(hashIdsCvs, cvActual, postu);
                    }
                    catch(Exception e)
                    {
                        objBatch.Err("Error al Buscar CV en jobs " + cvActual.nucleus_user.c_cv + " se rechaza el registro -"+e.Message);
                        continue;
                 
                    }
                   
                    string tipoDocumento = BuscarTipoDoc(cvActual.nucleus_user.oi_tipo_documento);
                    try
                    {
                        ofActual = BuscarOfertaJobs(hashIdsOfertas, ofActual, postu);
                    }
                    catch (Exception e)
                    {
                        objBatch.Err("Error al Buscar Oferta en jobs " + ofActual.title + " se rechaza el registro");
                        continue;

                    }
                   
                    //string idCVNucleus = NomadEnvironment.QueryValue("SDP01_CV", "oi_cv", "c_cv","WEB"+cvActual.id.ToString(), "", false);
                    string idCVNucleus = NomadEnvironment.QueryValue("SDP01_CV", "oi_cv", "c_cv", tipoDocumento+cvActual.nucleus_user.c_nro_doc, "", false);

                    try
                    {
                        cvNucleus = BuscarCVNucleus(hashIdsCvsNucleus,idCVNucleus,cvActual);
                        if (cvNucleus == null) continue;
                    }
                    catch(Exception e)
                    {
                        objBatch.Wrn("Error al crear el CV de codigo "+ cvActual.nucleus_user.c_cv + " se rechaza el registro");
                        continue;
                    }

                    string existeOferta = NomadEnvironment.QueryValue("SDP05_AVISOS", "oi_aviso", "oi_aviso", ofActual.oi_oferta_lab.ToString(), "", false);

                    if (existeOferta != "")
                    {
                        SeleccionDePostulantes.CVs.POSTULACIONES postulacionNueva = (SeleccionDePostulantes.CVs.POSTULACIONES)cvNucleus.POSTU_CV.GetByAttribute("oi_aviso", ofActual.oi_oferta_lab.ToString());

                        if (postulacionNueva == null)
                        {
                            postulacionNueva = new NucleusRH.Base.SeleccionDePostulantes.CVs.POSTULACIONES();
                            postulacionNueva.f_postulacion = postu.createdAt.Value;
                            postulacionNueva.oi_aviso = ofActual.oi_oferta_lab.ToString();

                            cvNucleus.POSTU_CV.Add(postulacionNueva);

                           
                            try
                            {
                                MarcarComoLeida(cvNucleus, postulacionNueva);
                            }
                            catch(Exception e)
                            {
                                objBatch.Err("Error en el proceso de marcar postulacion: " + e.Message);
                                continue;
                            }
                            if(cvNucleus.id.ToString() != "")
                                hashIdsCvsNucleus[cvNucleus.id.ToString()] = cvNucleus;

                            ++postuCorrectas;
                        }
                        else
                        {
                            ++postuIncorrectas;
                            objBatch.Wrn("El CV de codigo "+cvNucleus.c_cv + "' ya se encuentra postulado en la Oferta Laboral de codigo '" + ofActual.nucleus_offer.c_oferta_lab + "' (oi=" + ofActual.oi_oferta_lab + ").");
                            continue;
                        }
                    }
                    else
                    {
                        ++postuIncorrectas;
                        objBatch.Err("No existe la Oferta Laboral '" + ofActual.nucleus_offer.c_oferta_lab + "' (oi=" + ofActual.oi_oferta_lab + "). Se rechaza la postulación del CV de codigo " + cvActual.nucleus_user.c_cv );
                        continue;
                    }

                }
 
                NomadEnvironment.GetCurrentTransaction().Save(hashIdsCvsNucleus.Values);

                objBatch.SetPro(100);
                objBatch.Log("Se han migrado postulaciones correctas:" + postuCorrectas);
                objBatch.Log("Postulaciones incorrectas :" + postuIncorrectas);

            }
            catch (Exception e)
            {
                objBatch.Err("Error en el proceso de recepcion de Postulaciones: " + e.Message);
            }
        }

        private static void MarcarComoLeida(SeleccionDePostulantes.CVs.CV cvNucleus,SeleccionDePostulantes.CVs.POSTULACIONES cvPostulacion)
        {
             ReadApplications.Apply postuLeida = new ReadApplications.Apply();
             postuLeida.oi_oferta_lab = int.Parse(cvPostulacion.oi_aviso);
             postuLeida.dni = int.Parse(cvNucleus.c_nro_doc);
             postuLeida.dni_type = int.Parse(cvNucleus.oi_tipo_documento);
             postuLeida.userId = int.Parse(cvNucleus.c_cv.Substring(3,cvNucleus.c_cv.Length - 3));
             postuLeida.l_leido = true;

             Post<ReadApplications.Apply, ReadApplications.Apply>(postuLeida, GetHash("PostLeidas"));
        }

        private static SeleccionDePostulantes.CVs.CV BuscarCVNucleus(Hashtable hashIdsCvsNucleus, string idCVNucleus,CVs.CV cvActual)
        {
            if(idCVNucleus != "")
            {
                if (hashIdsCvsNucleus.ContainsKey(idCVNucleus))
                    return (SeleccionDePostulantes.CVs.CV)hashIdsCvsNucleus[idCVNucleus];
                else
                {
                    SeleccionDePostulantes.CVs.CV cvNucleus = (idCVNucleus == "") ? CrearCVNucleus(cvActual) : SeleccionDePostulantes.CVs.CV.Get(idCVNucleus);
                    hashIdsCvsNucleus[idCVNucleus] = cvNucleus;
                    return cvNucleus;
                }
            }
            return null;
        }

        private static SeleccionDePostulantes.CVs.CV CrearCVNucleus(CVs.CV cvActual)
        {
            SeleccionDePostulantes.CVs.CV cvNucleus = CopiarCV(cvActual, new SeleccionDePostulantes.CVs.CV());
            NomadEnvironment.GetCurrentTransaction().SaveRefresh(cvNucleus);
            return cvNucleus;
        }

        private static Ofertas.Oferta BuscarOfertaJobs(Hashtable hashIdsOfertas, Ofertas.Oferta ofActual, Applications.Apply postu)
        {

            if (!hashIdsOfertas.ContainsKey(postu.offerId.ToString()))
            {
                ofActual = Get<Ofertas>(GetHash("GetOfertas", new string[] { postu.offerId.ToString() })).data[0];

                hashIdsOfertas.Add(postu.offerId.ToString(), ofActual);
            }
            else
                ofActual = (Ofertas.Oferta)hashIdsOfertas[postu.offerId.ToString()];
            return ofActual;
        }

        private static CVs.CV BuscarCVJobs(Hashtable hashIdsCvs, CVs.CV cvActual, Applications.Apply postu)
        {
 
            if (!hashIdsCvs.ContainsKey(postu.userId.ToString()))
            {
                cvActual = Get<CVs>(GetHash("GetCV", new string[] { postu.userId.ToString() })).data[0];
                hashIdsCvs.Add(postu.userId.ToString(), cvActual);
            }
            else
                cvActual = (CVs.CV)hashIdsCvs[postu.userId.ToString()];
            return cvActual;
        }

        public static void AUTO_GuardarPostulaciones()
        {
            objBatch = NomadBatch.GetBatch("Proceso Jobs", "AUTO_GuardarPostulaciones()");
            NomadLog.Debug("~ Iniciando proceso AUTOMÁTICO de migración de Postulaciones ~");

            string d_valor = NomadEnvironment.QueryValue("ORG26_PARAMETROS", "d_valor", "c_parametro", "f_ult_actual_POSTU", "ORG26_PARAMETROS.c_modulo=\\'POS\\'", false);
            string f_desde = d_valor != "" ? d_valor : new DateTime(1899, 01, 01).ToString("yyyyMMdd");
            string f_hasta = DateTime.Now.ToString("yyyyMMdd");

            GuardarPostulaciones(f_desde, f_hasta);
            GuardarParametro("f_ult_actual_POSTU", f_hasta);

        }

        #endregion

        #region Utiles

        public static string GetHash(  string codigo)
        {
            return hashEndpoints[codigo].ToString();
        }

        static Regex regex = new Regex(Regex.Escape("#"));

        public static string GetHash( string codigo, params string[] args)
        {
            string endpoint = hashEndpoints[codigo].ToString();
            foreach (string s in args)
            {
                endpoint = regex.Replace(endpoint, s, 1);
            }
            return endpoint;
        }

        private static Hashtable BuscarEndpoints()
        {
            try
            {
                hashEndpoints = new Hashtable();
                return NomadEnvironment.QueryHashtableValue(Resources.HASH_ENDPOINTS, "", "c_endpoint", "c_url", true);
            }
            catch (Exception e)
            {
                objBatch.Err("Error al obtener los Endpoints");
                return null;
            }

        }

        private static string GetParentID(Codificadora codificadoraAux, string parent)
        {
            foreach (Codificadora.Data d in codificadoraAux.data)
            {
                if (d.external_id == parent)
                    return d.id;
            }
            return null;
        }

        private static string GetTexto(string text, int largo)
        {
            return text != null ? (text.Length < largo ? text : text.Substring(0, largo)) : null;
        }

        private static void GuardarParametro(string nombre_param, string f_hasta)
        {
            NomadLog.Debug("~ Guardando parametro: " + nombre_param + " ~");

            string oiParam = NomadEnvironment.QueryValue("ORG26_PARAMETROS", "oi_parametro", "c_parametro", nombre_param, "ORG26_PARAMETROS.c_modulo=\\'POS\\'", false);
            NucleusRH.Base.Organizacion.Parametros.PARAMETRO ddoPARAM = oiParam != "" ? ddoPARAM = NucleusRH.Base.Organizacion.Parametros.PARAMETRO.Get(oiParam) :  new NucleusRH.Base.Organizacion.Parametros.PARAMETRO();

            ddoPARAM.c_modulo = "POS";
            ddoPARAM.c_parametro = nombre_param;
            ddoPARAM.d_parametro = "Fecha ultima actualizacion Jobs";
            ddoPARAM.d_clase = "CV";
            ddoPARAM.d_tipo_parametro = "F";
            ddoPARAM.l_bloqueado = false;
            ddoPARAM.d_valor = f_hasta;

            NomadEnvironment.GetCurrentTransaction().SaveRefresh(ddoPARAM);

            NomadLog.Debug("~ Guardado el parametro: " + nombre_param + " con éxito ~");
        }

        private static string GetParametro(string pstrNombreParametro, string pstrValorDefault) {

            string strParamTemp;
            strParamTemp = GetParametro(pstrNombreParametro);
            return strParamTemp == "" ? pstrValorDefault : strParamTemp;

        }
        private static string GetParametro(string pstrNombreParametro) {
            
            string strParamTemp;
            strParamTemp = NomadEnvironment.QueryValue("ORG26_PARAMETROS", "d_valor", "c_parametro", pstrNombreParametro, "ORG26_PARAMETROS.c_modulo=\\'POS\\'", false);
            return strParamTemp == null ? "" : strParamTemp;

        }

        private static string GetNivelEstudioDefault() {

            string strNE;
            strC_NivelEstudioDefault = GetParametro("NivelEstudioDefault");
            strC_NivelEstudioDefault = strC_NivelEstudioDefault == "" ? "1" : strC_NivelEstudioDefault;

            strNE = NomadEnvironment.QueryValue("PER12_NIVELES_EST", "oi_nivel_estudio", "c_nivel_estudio", strC_NivelEstudioDefault, "", false);

            return strNE;

        }
        public static string UsaJobs()
        {
            GetJobsConnection();
            return (jobsUsr == "" || jobsPass == "" || jobsUrl == "") ? "0" : "1";
        }

        private static void GetJobsConnection()
        {
            NomadXML Ctx = NomadProxy.GetProxy().ReadContext();
            NomadXML APPs = Ctx.FindElement("apps");
            NomadXML APP = null;

            for (NomadXML xmlAPP = APPs.FirstChild(); xmlAPP != null; xmlAPP = xmlAPP.Next())
            {
                if(xmlAPP.GetChilds().Count>0)
                {
                    APP = xmlAPP.FindElement2("instance", "id", NomadProxy.GetProxy().Application.Id);
                    if(APP!=null) break;
                }
            }

            if(APP!=null)
            {
                jobsUrl = APP.GetAttrString("jobs-url");
                jobsUsr = APP.GetAttrString("jobs-usr");
                jobsPass = APP.GetAttrString("jobs-pass");
            }
        }

        private static void Post<T>(T objeto, string urlEndopoint)
        {
            Post<T,object>(objeto,urlEndopoint);
        }

        private static U Post<T,U>(T objeto, string urlEndopoint)
        {
            ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072; //TLS 1.2
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(jobsUrl + urlEndopoint);
            request.ContentType = "application/json";
            if (token != "")
                request.Headers.Add("x-access-token", token);
            request.Method = "POST";
            request.Timeout = 24000000;
            U resultadoPost;

            using (StreamWriter streamWriter = new StreamWriter(request.GetRequestStream()))
            {
                string json = StringUtil.Object2JSON(objeto);
                streamWriter.Write(json);
                streamWriter.Flush();
                streamWriter.Close();
            }
            string result = "";
            try
            {
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                using (StreamReader streamReader = new StreamReader(response.GetResponseStream()))
                {
                    result = streamReader.ReadToEnd();
                    resultadoPost = StringUtil.JSON2Object<U>(result);
                }
            }
            catch (WebException ex)
            {
                msj = new Message(ex.Message);
                if (ex.Status != WebExceptionStatus.Timeout)
                {
                    System.Net.HttpWebResponse WResponse = (System.Net.HttpWebResponse)ex.Response;
                    using (StreamReader streamReader = new StreamReader(WResponse.GetResponseStream()))
                    {
                        result = streamReader.ReadToEnd();
                        try
                        {
                            msj = StringUtil.JSON2Object<Message>(result);
                        }
                        catch (Exception e)
                        {
                            throw e;
                        }
                    }
                }
                throw new Exception(msj.message,ex);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return resultadoPost;
        }

        private static T Get<T>(string url) where T : new()
        {

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(jobsUrl + url);
            request.ContentType = "application/json";
            request.Method = "GET";
            if (token != "")
                request.Headers.Add("x-access-token", token);

            T objeto = new T();

            string result = "";
            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            using (StreamReader streamReader = new StreamReader(response.GetResponseStream()))
            {
                result = streamReader.ReadToEnd();
                NomadLog.Debug("JSON recuperado " + result);
                objeto = StringUtil.JSON2Object<T>(result);
            }

            return objeto;
        }

        public static T Crear<T>(NomadXML xmlC) where T : new()
        {
            T obj = new T();

            PropertyInfo[] properties = typeof(T).GetProperties();
            foreach (PropertyInfo p in properties)
            {
                if (xmlC.GetAttr(p.Name) != "")
                {
                    p.SetValue(obj,Convertir(xmlC,p), null);
                }
                if (xmlC.FindElement(p.Name) != null)
                {
                    NomadXML elementoNuevo = xmlC.FindElement(p.Name);
                    if (elementoNuevo.GetAttr("col") == "1")
                    {
                        object listaTemp = Activator.CreateInstance(p.PropertyType);

                        ArrayList array = elementoNuevo.GetChilds();
                        for (int i = 0; i < array.Count; i++)
                        {
                            Type myListElementType = listaTemp.GetType().GetGenericArguments()[0];

                            NomadXML ele = new NomadXML(array[i].ToString()).FirstChild();
                            object elemento = typeof(JOBS).GetMethod("Crear").MakeGenericMethod(new Type[] { myListElementType }).Invoke(null, new object[] { ele });
                            listaTemp.GetType().GetMethod("Add").Invoke(listaTemp, new object[] { elemento });

                            p.SetValue(obj, listaTemp, null);
                        }
                    }
                    else
                    {
                        object objTemp = Activator.CreateInstance(p.PropertyType);
                        object elemento = typeof(JOBS).GetMethod("Crear").MakeGenericMethod(new Type[] { p.PropertyType }).Invoke(null, new object[] { elementoNuevo });
                        p.SetValue(obj, elemento , null);
                    }

                }
            }
            return obj;
        }

        private static object Convertir(NomadXML xmlC, PropertyInfo p)
        {
            Type tipo = Nullable.GetUnderlyingType(p.PropertyType);
            string tipoNombre = tipo != null ? tipo.Name : p.PropertyType.Name;

            switch(tipoNombre)
            {
                case "Boolean":
                    return xmlC.GetAttrBool(p.Name);
                case "Double":
                    return xmlC.GetAttrDouble(p.Name);
                case "Int32":
                    return xmlC.GetAttrInt(p.Name);
                case "DateTime":
                    return xmlC.GetAttrDateTime(p.Name);
                default:
                    return xmlC.GetAttr(p.Name);
            }

        }
        #endregion
        #endregion

        public static void ImportarTags()
        {
            int Linea = 0, Errores = 0;
            NomadXML IDCur;
            NomadXML IDList = new NomadXML();
            NucleusRH.Base.SeleccionDePostulantes.Tags.TAGIMP objRead;

            NomadBatch objBatch = NomadBatch.GetBatch("Iniciando...", "Migracion de Tags");

            IDList.SetText(NomadProxy.GetProxy().SQLService().Get(NucleusRH.Base.SeleccionDePostulantes.Tags.TAGIMP.Resources.qry_rows, ""));

            NomadXML xmlTags = NomadEnvironment.QueryNomadXML(NucleusRH.Base.SeleccionDePostulantes.Tags.TAG.Resources.COUNT, "").FirstChild();
            int cantidad = xmlTags.GetAttrInt("cantidad");
            if(xmlTags.ChildLength < 10)
            {
                cantidad++;
                for (Linea = 1, IDCur = IDList.FindElement("ROWS").FirstChild(); IDCur != null; IDCur = IDCur.Next(), Linea++)
                {
                    if (cantidad > 10)
                        break;
                    objBatch.SetPro(0, 100, IDList.FirstChild().ChildLength, Linea);
                    objBatch.SetMess("Importando Tag " + Linea + " de " + IDList.FirstChild().ChildLength);
                    objRead = NucleusRH.Base.SeleccionDePostulantes.Tags.TAGIMP.Get(IDCur.GetAttr("id"));

                    try
                    {
                        //Nueva Licencia
                        NucleusRH.Base.SeleccionDePostulantes.Tags.TAG Tag = new Tags.TAG();
                        Tag.c_tag = "l_tag" + cantidad.ToString("D2");
                        Tag.d_tag = objRead.tag;
                        cantidad++;

                        NomadEnvironment.GetCurrentTransaction().Save(Tag);
                        objBatch.Log("Se guardó correctamente el tag '" + objRead.tag + "'");
                    }
                    catch (Exception e)
                    {
                        objBatch.Err("Error: " + e.Message + " - Se rechaza el registro - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }
                }
                Linea--;
            }
            else
            {
                objBatch.Log("Ya se alcanzó el máximo de 10 tags. Si necesita nuevos, solicite el borrado de los cargados.");
            }

            objBatch.Log("Registros Procesados: " + Linea.ToString() + " - Importados: " + (Linea - Errores).ToString());
            objBatch.Log("Finalizado...");
        }

    }

}



using System;
using System.Xml;
using System.IO;
using System.Collections;
using System.Collections.Generic;

using System.Reflection;
using Microsoft.CSharp;
using System.CodeDom.Compiler;

using Nomad.NSystem.Document;
using Nomad.NSystem.Proxy;
using Nomad.NSystem.Base;
using Nomad.NSystem.Functions;

namespace NucleusRH.Base.Postulantes.CV
{

    #region CV

    /// <summary>
    /// Clase padre de CVs
    /// </summary>
    public class WEB_CV
    {
       
        
        #region Constructores
        public WEB_CV()
        {

        }

        #endregion

        #region Propiedades

        public string c_cv;
        public string d_apellido;
        public string d_nombres;

        public int oi_tipo_documento;
        public string c_nro_doc;
        public string c_password;
        public string c_nro_cuil;

        public DateTime? f_nacim;
        public string c_sexo;
        public int? oi_nacionalidad;
        public int? oi_estado_civil;

        public string d_email;

        //Atributos para numero de telefono
        public string c_pais;
        public string c_area;
        public string te_nro;

        //Atributos para celular
        public string c_pais_cel;
        public string c_area_cel;
        public string te_celular;

        //Atributos para direccion
        public string d_calle;
        public string c_nro;
        public string c_piso;
        public string c_departamento;

        public string d_zona;
        public string d_localidad;
        public string c_codigo_postal;
        public int? oi_pais;
        public int? oi_provincia;
        public int? oi_localidad;


        public double? n_remuneracion;

        //Desea recibir informacion de nuevos empleos por mail
        public bool l_informacion;

        //Coleccion de Documentos Digitales
        public List<WEB_CV_DOC_DIG> DocumentosDigitales = new List<WEB_CV_DOC_DIG>();

        //Coleccion de estudios CV
        public List<WEB_CV_Estudios> EstudiosCV = new List<WEB_CV_Estudios>();

        //Coleccion de Experiencia Laboral del CV      
        public List<WEB_CV_Exp_Lab> ExpLaboralCV = new List<WEB_CV_Exp_Lab>();

        //Coleccion de Idiomas
        public List<WEB_CV_Idiomas> IdiomasCV = new List<WEB_CV_Idiomas>();

        //Coleccion de Conocimientos
        public List<WEB_CV_Conocimiento> ConocimientosCV = new List<WEB_CV_Conocimiento>();

        //Coleccion de Postulaciones
        public List<WEB_CV_Postulaciones> PostulacionesCV = new List<WEB_CV_Postulaciones>();
        #endregion

        #region Metodos
        
        #endregion

        
    }

    /// <summary>
    /// Child de Estudios del CV
    /// </summary>
    public class WEB_CV_Estudios
    {
        #region Constructores
        public WEB_CV_Estudios()
        {

        }
        #endregion

        #region Propiedades

        public int oi_nivel_estudio;
        public int oi_estado_est;
        public int? oi_estudio;        

        //Establecimiento educativo(esta en NS)
        public string d_otro_est_educ;

        #endregion

        #region Metodos
        #endregion
    }

    /// <summary>
    /// Child de Experiencia LAboral del CV
    /// </summary>
    public class WEB_CV_Exp_Lab
    {
        #region Constructores
        public WEB_CV_Exp_Lab() { }
        #endregion

        #region Propiedades

        public string d_empresa;
        public string d_puesto;
        public DateTime f_ingreso;
        public DateTime? f_egreso;

        //Descripcion de las tareas
        public string o_tareas;
        public int? oi_area_lab;

        #endregion

        #region Metodos
        #endregion

    }

    /// <summary>
    /// Child de Idiomas del CV
    /// </summary>
    public class WEB_CV_Idiomas
    {
        #region Constructores
        public WEB_CV_Idiomas() { }
        #endregion

        #region Propiedades

        public int oi_idioma;
        public int oi_nivel_habla;
        public int oi_nivel_lee;
        public int oi_nivel_escribe;

        public bool l_certificado;//Esta en NS


        #endregion

        #region Metodos
        #endregion
    }



    /// <summary>
    /// Child de Conocimientos del CV
    /// </summary>
    public class WEB_CV_Conocimiento
    {
        #region Constructores
        public WEB_CV_Conocimiento() { }
        #endregion

        #region Propiedades

        public int oi_conocimiento;

        //Nivel del conocimiento, siendo Basico, Intermedio o Avanzado
        public string c_nivel;

        //Detalle del conocimiento
        public string d_conoc_cv;

        #endregion

        #region Metodos
        #endregion
    }

    /// <summary>
    /// Postulaciones del CV
    /// </summary>
    public class WEB_CV_Postulaciones
    {
        #region Constructores
        public WEB_CV_Postulaciones() { }
        #endregion

        #region Propiedades
        //Oferta Laboral de la postulacion del cv        
        public int oi_oferta_lab;
        public DateTime f_postulacion;
        #endregion

        #region Metodos
        #endregion
    }   
    /// <summary>
    /// Clase de documentos digitales
    /// </summary>
    public class WEB_CV_DOC_DIG
    {
        #region Constructores
        public WEB_CV_DOC_DIG() { }
        #endregion

        #region Propiedades
        public string c_tipo;
        public string o_base64;
        public string oi_doc_dig;
        #endregion

        #region Metodos
        #endregion
    }

    #endregion

    #region OFERTA LABORAL
    
    /// <summary>
    /// Clase Padre de Ofertas Laborales
    /// </summary>
    public class WEB_OfertaLab
    {
        #region Constructores
        public WEB_OfertaLab() { }
        #endregion

        #region Propiedades
        //De la lista
        public int oi_oferta_lab;
        public DateTime f_oferta_lab;
        public string d_oferta_lab;
        public string o_oferta_lab;

        //Se agregan para el modo SHOW
        public string c_oferta_lab;
        public DateTime? f_cierre;
        public int? e_cantidad;
        public int? oi_tipo_puesto;
        public int? oi_seniority;
        public int? oi_area_lab;
        public int? oi_pais;
        public int? oi_provincia;
        public int? oi_localidad;
        public string d_coordenadas;

        //Muestrar salario en aviso y solicitar salario al postulante
        public bool l_mostrar_en_aviso;
        public bool l_solicita_sal;
        
        //Requisitos
        public int? oi_pais_res;
        public int? oi_provincia_res;
        public int? oi_localidad_res;
        public int? e_exp_des;
        public int? oi_uni_tpo_desde;
        public int? e_exp_hasta;
        public int? oi_uni_tpo_hasta;
        public int? e_edad_desde;
        public int? e_edad_hasta;
        public string c_sexo;
        public int? oi_estado_civil;
        public int? oi_nivel_estudio;
        public int? oi_estado_est;
        public int? oi_nivel_ing;
        public int? oi_nivel_por;
        public int? oi_nivel_fr;
        public int? oi_nivel_it;
        public int? oi_nivel_al;
        public int? oi_nivel_jap;
        public double? n_salario_d;
        public double? n_salario_h;
        public bool l_exc_res;
        public bool l_exc_exp;
        public bool l_exc_edad;
        public bool l_exc_sexo;
        public bool l_exc_ecivil;
        public bool l_exc_edu;
        public bool l_exc_idioma;
        public bool l_exc_salario;       
        public string d_mail;
        public string d_mail_alt_1;
        public string d_mail_alt_2;

        public int? oi_empresa;
        public string c_uni_negocio;
        public string d_uni_negocio;

        //Preguntas
        public List<WEB_Preguntas> Preguntas = new List<WEB_Preguntas>();

        #endregion
         
    }

    /// <summary>
    /// Child de preguntas
    /// </summary>
    public class WEB_Preguntas
    {
        #region Constructores
        public WEB_Preguntas() { }
        #endregion

        #region Propiedades

        public string ID;
        public string d_pregunta;
        public List<WEB_Preguntas_Opciones> Opciones = new List<WEB_Preguntas_Opciones>();

        #endregion

        #region Metodos

        #endregion
    }

    /// <summary>
    /// Child de opciones de preguntas
    /// </summary>
    public class WEB_Preguntas_Opciones
    {
        #region Constructores
        public WEB_Preguntas_Opciones() { }

        #endregion

        #region Propiedades
        public string ID;
        public string d_pregunta_op;
        #endregion

        #region Metodos
        #endregion
         
    }
    #endregion
   
    
    #region POSTULACIONES
    /// <summary>
    /// Clase padre postulaciones
    /// </summary>
    public class WEB_Postulaciones
    {
        #region Constructores
        public WEB_Postulaciones() { }
        #endregion

        #region Propiedades
        
        public int oi_oferta_lab;
        public int? oi_origen_aviso;
        public double? n_remuneracion;

        //Coleccion de Respuestas
        public List<WEB_Respuestas> Respuestas = new List<WEB_Respuestas>();

        #endregion      
    }


    /// <summary>
    /// Child de Respuestas a Preguntas de una Postulacion
    /// </summary>
    public class WEB_Respuestas
    {
        #region Constructores
        public WEB_Respuestas() { }
        #endregion

        #region Propiedades

        public string d_respuesta;
        public string ID_pregunta;
        public string ID_Opcion_Sel;

        #endregion
      
    }
    
    #endregion
    
    #region OTRAS
    /// <summary>
    /// Clase que recupera los datos de las codificadoras
    /// </summary>
    public class WEB_Codificadora
    {
        #region Constructores

        public WEB_Codificadora() { }

        #endregion

        #region Propiedades

        public string Codigo;
        public string Descripcion;
        public string ID;
        public string ParentID;

        #endregion
    
    }

   
    /// <summary>
    /// Clase Archivos
    /// </summary>
    public class WEB_File
    {
        #region Constructores

        public WEB_File() { }

        #endregion

        #region Propiedades

        public string ID;
        public string Base64;
        public string mimeType;

        #endregion
    
     }

    #endregion

}



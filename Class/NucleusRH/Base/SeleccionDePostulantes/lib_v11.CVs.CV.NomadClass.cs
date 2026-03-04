using System;
using System.Xml;
using System.IO;
using System.Collections;

using System.Reflection;
using Microsoft.CSharp;
using System.CodeDom.Compiler;

using Nomad.NSystem.Document;
using Nomad.NSystem.Proxy;
using Nomad.NSystem.Base;
using Nomad.NSystem.Functions;

namespace NucleusRH.Base.SeleccionDePostulantes.CVs
{

    //////////////////////////////////////////////////////////////////////////////////
    //Clase CVs
    public partial class CV : Nomad.NSystem.Base.NomadObject
    {

        const string tmpOpen = "{#";
        const string tmpClose = "#}";

        //--------------------------------------------------------------------------------------------------------
        // Métodos ESTÁTICOS
        //--------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Graba los datos para la registración
        /// </summary>
        /// <param name="pxmlData"></param>
        /// <returns></returns>
        public static NomadXML SaveRegis(NomadXML pxmlData)
        {
            NomadBatch.Trace("--------------------------------------------------------------------------");
            NomadBatch.Trace("Comienza la grabacion de los datos para la registracion");
            NomadBatch.Trace("--------------------------------------------------------------------------");

            NomadXML xmlResult = new NomadXML("UDATA");
            NomadXML xmlTipoDoc;
            NomadXML xmlParam;
            NucleusRH.Base.SeleccionDePostulantes.CVs.CV objCV;
            Nomad.Base.Mail.OutputMails.MAIL objMail;
            Nomad.Base.Mail.OutputMails.DESTINATARIO objDestino;
            string strStep,oiPER = "";

            pxmlData = pxmlData.FirstChild();
            strStep = "CD";// Cargando Datos;

            try
            {
                //----------------------------------------------------------------------------------------------------
                NomadBatch.Trace("Carga los datos...");
                objCV = new NucleusRH.Base.SeleccionDePostulantes.CVs.CV();

                oiPER = NomadEnvironment.QueryValue("PER01_PERSONAL", "oi_personal", "c_nro_documento", pxmlData.GetAttr("c_nro_doc"), "", false);
                if (oiPER != null) objCV.oi_personal = oiPER;

                objCV.d_nombres = pxmlData.GetAttr("d_nombres");
                objCV.d_apellido = pxmlData.GetAttr("d_apellido");
                objCV.oi_tipo_documento = pxmlData.GetAttr("oi_tipo_documento");
                objCV.c_nro_doc = pxmlData.GetAttr("c_nro_doc");
                objCV.f_nacim = new DateTime(pxmlData.GetAttrInt("ano_nac"), pxmlData.GetAttrInt("mes_nac"), pxmlData.GetAttrInt("dia_nac"));
                objCV.c_sexo = pxmlData.GetAttr("c_sexo");
                objCV.d_email = pxmlData.GetAttr("d_email");

                //Obtiene la descripcion del tipo de documento
                xmlParam = new NomadXML("PARAM");
                xmlParam.SetAttr("oi_tipo_documento", pxmlData.GetAttr("oi_tipo_documento"));
                xmlTipoDoc = NomadEnvironment.QueryNomadXML(NucleusRH.Base.Personal.Tipos_Documento.TIPO_DOCUMENTO.Resources.infoTipoDoc, xmlParam.ToString()).FirstChild();

                objCV.c_cv = xmlTipoDoc.GetAttr("c_tipo_documento") + pxmlData.GetAttr("c_nro_doc");

                if (pxmlData.GetAttr("te_celular") != "")
                {
                    objCV.c_pais_cel = pxmlData.GetAttr("c_pais_cel");
                    objCV.c_area_cel = pxmlData.GetAttr("c_area_cel");
                    objCV.te_celular = pxmlData.GetAttr("te_celular");
                }

                if (pxmlData.GetAttr("te_nro") != "")
                {
                    objCV.c_pais = pxmlData.GetAttr("c_pais");
                    objCV.c_area = pxmlData.GetAttr("c_area");
                    objCV.te_nro = pxmlData.GetAttr("te_nro");
                }

                objCV.d_calle = pxmlData.GetAttr("d_calle");
                objCV.c_nro = pxmlData.GetAttr("c_nro");

                if (pxmlData.GetAttr("c_piso") == "")
                    objCV.c_pisoNull = true;
                else
                    objCV.c_piso = pxmlData.GetAttr("c_piso");

                if (pxmlData.GetAttr("c_departamento") == "")
                    objCV.c_departamentoNull = true;
                else
                    objCV.c_departamento = pxmlData.GetAttr("c_departamento");

                if (pxmlData.GetAttr("oi_pais") != "")
                    objCV.oi_pais = pxmlData.GetAttr("oi_pais");
                else
                    objCV.oi_paisNull = true;

                if (pxmlData.GetAttr("oi_provincia") != "")
                    objCV.oi_provincia = pxmlData.GetAttr("oi_provincia");
                else
                    objCV.oi_provinciaNull = true;

                if (pxmlData.GetAttr("oi_localidad") != "")
                    objCV.oi_localidad = pxmlData.GetAttr("oi_localidad");
                else
                    objCV.oi_localidadNull = true;

                if (pxmlData.GetAttr("d_localidad") != "")
                    objCV.d_localidad = pxmlData.GetAttr("d_localidad");
                else
                    objCV.d_localidadNull = true;

        if (pxmlData.GetAttr("c_codigo_postal") == "")
                    objCV.c_codigo_postalNull = true;
                else
                    objCV.c_codigo_postal = pxmlData.GetAttr("c_codigo_postal");

                objCV.c_password = pxmlData.GetAttr("c_password");
                objCV.l_maillist = pxmlData.GetAttrBool("l_maillist");

                objCV.d_ape_y_nom = objCV.d_apellido + ", " + objCV.d_nombres;
                objCV.c_estado = "A";
                objCV.f_estado = DateTime.Now;
                objCV.l_alta_web = true;
                objCV.f_alta_cv = DateTime.Now;
                objCV.f_actualizacion = DateTime.Now;

                objCV.c_seguridad = StringUtil.GetMD5(objCV.d_ape_y_nom + "-" + objCV.d_email + DateTime.Now.ToString("yyyyMMddHHmmsss"));

                //----------------------------------------------------------------------------------------------------
                strStep = "SA"; // Realizando el SAVE
                NomadBatch.Trace("Realiza el SAVE...");
                NomadEnvironment.GetCurrentTransaction().Save(objCV);

                //----------------------------------------------------------------------------------------------------
                strStep = "MA"; // Enviando el MAIL
                NomadBatch.Trace("Crea el mail...");

                Hashtable htaMails = GetMails("REGIS", objCV, null);

                objMail = new Nomad.Base.Mail.OutputMails.MAIL();
                objMail.ASUNTO = htaMails.ContainsKey("REGIS_ASUNTO") ? (string)htaMails["REGIS_ASUNTO"] : "-";
                objMail.CONTENIDO = htaMails.ContainsKey("REGIS_TEXTO") ? (string)htaMails["REGIS_TEXTO"] : "-";
                objMail.DESDE_APLICACION = NomadProxy.GetProxy().AppName;
                objMail.FECHA_CREACION = DateTime.Now;

                objDestino = new Nomad.Base.Mail.OutputMails.DESTINATARIO();
                objDestino.MAIL_SUSTITUTO = objCV.d_email;
                objMail.DESTINATARIOS.Add(objDestino);

                NomadEnvironment.GetCurrentTransaction().Save(objMail);

                //----------------------------------------------------------------------------------------------------
                strStep = "RE"; // Recuperando la informacion principal para la WEB
                xmlParam = new NomadXML("PARAM");
                xmlParam.SetAttr("c_nro_doc", objCV.c_nro_doc);
                xmlParam.SetAttr("oi_tipo_documento", objCV.oi_tipo_documento);

                xmlResult = NomadEnvironment.QueryNomadXML(CV.Resources.loginUsuario, xmlParam.ToString()).FirstChild();
                xmlResult.SetAttr("out", "OK");

                NomadBatch.Trace("--------------------------------------------------------------------------");
                NomadBatch.Trace("El metodo se finalizo correctamente.");
                NomadBatch.Trace("--------------------------------------------------------------------------");

            }
            catch (Exception ex)
            {
                xmlResult.SetAttr("out", "err");
                NomadBatch.Trace("--------------------------------------------------------------------------");
                NomadBatch.Trace("ERROR");
                NomadBatch.Trace("DESCRIPTION: " + ex.Message);

                switch (strStep)
                {
                    case "CD":
                        NomadBatch.Trace("Se produjo un error Realizando la carga de los datos.");
                        break;

                    case "SA":
                        NomadBatch.Trace("Se produjo un error Realizando el Save.");
                        if (ex.Message == "DB.SQLSERVER.2627" || ex.Message == "DB.ORA.1")
                        {
                            //Violacion de clave primaria
                            xmlResult.SetAttr("out", "Ya existe el usuario con nro. doc " + pxmlData.GetAttr("c_nro_doc") + ".");
                        }

                        break;

                    case "MA":
                        NomadBatch.Trace("Se produjo un error creando el mail a enviar.");
                        break;
                }

                NomadBatch.Trace("--------------------------------------------------------------------------");
            }

            return xmlResult;

        }

        /// <summary>
        /// Graba los datos del CV
        /// </summary>
        /// <param name="pxmlData"></param>
        /// <returns></returns>
        public static NomadXML SaveCV(NomadXML pxmlData)
        {
            NomadBatch.Trace("--------------------------------------------------------------------------");
            NomadBatch.Trace("Comienza la grabacion de los datos del CV");
            NomadBatch.Trace("--------------------------------------------------------------------------");

            NomadXML xmlResult = new NomadXML("PARAM");
            NucleusRH.Base.SeleccionDePostulantes.CVs.CV objCV = null;
            string strStep;
            string strOICV = "";
            string strTemp;

            pxmlData = pxmlData.FirstChild();
            strStep = "RD";// Recuperando DDO

            try
            {
                //----------------------------------------------------------------------------------------------------
                NomadBatch.Trace("Recuperando el DDO...");
                strOICV = pxmlData.GetAttr("oi_cv");
                objCV = NucleusRH.Base.SeleccionDePostulantes.CVs.CV.Get(strOICV);

                strStep = "CD";// Cargando Datos;

                //----------------------------------------------------------------------------------------------------
                //Cargando Datos PERSONALES;
                NomadBatch.Trace("Carga los datos Personales...");

                objCV.d_nombres = pxmlData.GetAttr("d_nombres");
                objCV.d_apellido = pxmlData.GetAttr("d_apellido");

                strTemp = pxmlData.GetAttr("c_nro_cuil_1") + pxmlData.GetAttr("c_nro_cuil_2") + pxmlData.GetAttr("c_nro_cuil_3");
                if (strTemp == "")
                    objCV.c_nro_cuilNull = true;
                else
                    objCV.c_nro_cuil = strTemp;

                objCV.f_nacim = new DateTime(pxmlData.GetAttrInt("ano_nac"), pxmlData.GetAttrInt("mes_nac"), pxmlData.GetAttrInt("dia_nac"));
                objCV.c_sexo = pxmlData.GetAttr("c_sexo");

                if (pxmlData.GetAttr("oi_estado_civil") == "")
                    objCV.oi_estado_civilNull = true;
                else
                    objCV.oi_estado_civil = pxmlData.GetAttr("oi_estado_civil");

                objCV.d_email = pxmlData.GetAttr("d_email");

                if (pxmlData.GetAttr("oi_pais") == "")
                    objCV.oi_paisNull = true;
                else
                    objCV.oi_pais = pxmlData.GetAttr("oi_pais");

                if (pxmlData.GetAttr("oi_provincia") == "")
                    objCV.oi_provinciaNull = true;
                else
                    objCV.oi_provincia = pxmlData.GetAttr("oi_provincia");

                if (pxmlData.GetAttr("oi_localidad") != "")
                    objCV.oi_localidad = pxmlData.GetAttr("oi_localidad");
                else
                    objCV.oi_localidadNull = true;

                if (pxmlData.GetAttr("d_localidad") != "")
                    objCV.d_localidad = pxmlData.GetAttr("d_localidad");
                else
                    objCV.d_localidadNull = true;

                if (pxmlData.GetAttr("c_codigo_postal") == "")
                    objCV.c_codigo_postalNull = true;
                else
                    objCV.c_codigo_postal = pxmlData.GetAttr("c_codigo_postal");

                if (pxmlData.GetAttr("d_calle") == "")
                    objCV.d_calleNull = true;
                else
                    objCV.d_calle = pxmlData.GetAttr("d_calle");

                if (pxmlData.GetAttr("c_nro") == "")
                    objCV.c_nroNull = true;
                else
                    objCV.c_nro = pxmlData.GetAttr("c_nro");

                if (pxmlData.GetAttr("c_piso") == "")
                    objCV.c_pisoNull = true;
                else
                    objCV.c_piso = pxmlData.GetAttr("c_piso");

                if (pxmlData.GetAttr("c_departamento") == "")
                    objCV.c_departamentoNull = true;
                else
                    objCV.c_departamento = pxmlData.GetAttr("c_departamento");

                if (pxmlData.GetAttr("te_nro") == "")
                {
                    objCV.c_paisNull = true;
                    objCV.c_areaNull = true;
                    objCV.te_nroNull = true;
                }
                else
                {
                    objCV.c_pais = pxmlData.GetAttr("c_pais");
                    objCV.c_area = pxmlData.GetAttr("c_area");
                    objCV.te_nro = pxmlData.GetAttr("te_nro");
                }

                if (pxmlData.GetAttr("te_celular") == "")
                {
                    objCV.c_pais_celNull = true;
                    objCV.c_area_celNull = true;
                    objCV.te_celularNull = true;
                }
                else
                {
                    objCV.c_pais_cel = pxmlData.GetAttr("c_pais_cel");
                    objCV.c_area_cel = pxmlData.GetAttr("c_area_cel");
                    objCV.te_celular = pxmlData.GetAttr("te_celular");
                }

                if (pxmlData.GetAttr("oi_doc_digital") == "")
                    objCV.oi_doc_digitalNull = true;
                else
                    objCV.oi_doc_digital = pxmlData.GetAttr("oi_doc_digital");

                if (pxmlData.GetAttr("oi_foto") == "")
                    objCV.oi_fotoNull = true;
                else
                    objCV.oi_foto = pxmlData.GetAttr("oi_foto");

                objCV.d_ape_y_nom = objCV.d_apellido + ", " + objCV.d_nombres;
                objCV.f_actualizacion = DateTime.Now;

                //----------------------------------------------------------------------------------------------------
                //Cargando Datos ADICIONALES;
                NomadBatch.Trace("Carga los datos Adicionales...");

                if (pxmlData.GetAttr("c_tiene_pariente") == "")
                    objCV.c_tiene_parienteNull = true;
                else
                    objCV.c_tiene_pariente = pxmlData.GetAttr("c_tiene_pariente");

                if (pxmlData.GetAttr("c_lic_conduc") == "")
                    objCV.c_lic_conducNull = true;
                else
                    objCV.c_lic_conduc = pxmlData.GetAttr("c_lic_conduc");

                if (pxmlData.GetAttr("c_vehiculo") == "")
                    objCV.c_vehiculoNull = true;
                else
                    objCV.c_vehiculo = pxmlData.GetAttr("c_vehiculo");

                if (pxmlData.GetAttr("c_radicarse") == "")
                    objCV.c_radicarseNull = true;
                else
                    objCV.c_radicarse = pxmlData.GetAttr("c_radicarse");

                if (pxmlData.GetAttr("c_viajar") == "")
                    objCV.c_viajarNull = true;
                else
                    objCV.c_viajar = pxmlData.GetAttr("c_viajar");

                if (pxmlData.GetAttr("oi_sit_laboral") == "")
                    objCV.oi_sit_laboralNull = true;
                else
                    objCV.oi_sit_laboral = pxmlData.GetAttr("oi_sit_laboral");

                if (pxmlData.GetAttr("oi_tipo_jor_lab") == "")
                    objCV.oi_tipo_jor_labNull = true;
                else
                    objCV.oi_tipo_jor_lab = pxmlData.GetAttr("oi_tipo_jor_lab");

                if (pxmlData.GetAttr("n_remuneracion") == "")
                    objCV.n_remuneracionNull = true;
                else
                    objCV.n_remuneracion = pxmlData.GetAttrDouble("n_remuneracion");

                if (pxmlData.GetAttr("d_ape_y_nom_fam1") == "")
                    objCV.d_ape_y_nom_fam1Null = true;
                else
                    objCV.d_ape_y_nom_fam1 = pxmlData.GetAttr("d_ape_y_nom_fam1");

                if (pxmlData.GetAttr("d_email_fam1") == "")
                    objCV.d_email_fam1Null = true;
                else
                    objCV.d_email_fam1 = pxmlData.GetAttr("d_email_fam1");

                if (pxmlData.GetAttr("te_nro_fam1") == "")
                    objCV.te_nro_fam1Null = true;
                else
                    objCV.te_nro_fam1 = pxmlData.GetAttr("te_nro_fam1");

                if (pxmlData.GetAttr("oi_tipo_fam1") == "")
                    objCV.oi_tipo_fam1Null = true;
                else
                    objCV.oi_tipo_fam1 = pxmlData.GetAttr("oi_tipo_fam1");

                if (pxmlData.GetAttr("d_ape_y_nom_fam2") == "")
                    objCV.d_ape_y_nom_fam2Null = true;
                else
                    objCV.d_ape_y_nom_fam2 = pxmlData.GetAttr("d_ape_y_nom_fam2");

                if (pxmlData.GetAttr("d_email_fam2") == "")
                    objCV.d_email_fam2Null = true;
                else
                    objCV.d_email_fam2 = pxmlData.GetAttr("d_email_fam2");

                if (pxmlData.GetAttr("te_nro_fam2") == "")
                    objCV.te_nro_fam2Null = true;
                else
                    objCV.te_nro_fam2 = pxmlData.GetAttr("te_nro_fam2");

                if (pxmlData.GetAttr("oi_tipo_fam2") == "")
                    objCV.oi_tipo_fam2Null = true;
                else
                    objCV.oi_tipo_fam2 = pxmlData.GetAttr("oi_tipo_fam2");

                if (pxmlData.GetAttr("d_ape_y_nom_ref1") == "")
                    objCV.d_ape_y_nom_ref1Null = true;
                else
                    objCV.d_ape_y_nom_ref1 = pxmlData.GetAttr("d_ape_y_nom_ref1");

                if (pxmlData.GetAttr("d_email_ref1") == "")
                    objCV.d_email_ref1Null = true;
                else
                    objCV.d_email_ref1 = pxmlData.GetAttr("d_email_ref1");

                if (pxmlData.GetAttr("te_nro_ref1") == "")
                    objCV.te_nro_ref1Null = true;
                else
                    objCV.te_nro_ref1 = pxmlData.GetAttr("te_nro_ref1");

                if (pxmlData.GetAttr("d_ape_y_nom_ref2") == "")
                    objCV.d_ape_y_nom_ref2Null = true;
                else
                    objCV.d_ape_y_nom_ref2 = pxmlData.GetAttr("d_ape_y_nom_ref2");

                if (pxmlData.GetAttr("d_email_ref2") == "")
                    objCV.d_email_ref2Null = true;
                else
                    objCV.d_email_ref2 = pxmlData.GetAttr("d_email_ref2");

                if (pxmlData.GetAttr("te_nro_ref2") == "")
                    objCV.te_nro_ref2Null = true;
                else
                    objCV.te_nro_ref2 = pxmlData.GetAttr("te_nro_ref2");

                 if (pxmlData.GetAttr("o_cv") != "")
                    objCV.o_cv = pxmlData.GetAttr("o_cv");
                else
                    objCV.o_cvNull = true;

                if (pxmlData.GetAttr("d_custom_1") != "")
                    objCV.d_custom_1 = pxmlData.GetAttr("d_custom_1");
                else
                    objCV.d_custom_1Null = true;

                if (pxmlData.GetAttr("d_custom_2") != "")
                    objCV.d_custom_2 = pxmlData.GetAttr("d_custom_2");
                else
                    objCV.d_custom_2Null = true;

                if (pxmlData.GetAttr("d_custom_3") != "")
                    objCV.d_custom_3 = pxmlData.GetAttr("d_custom_3");
                else
                    objCV.d_custom_3Null = true;

                if (pxmlData.GetAttr("d_custom_4") != "")
                    objCV.d_custom_4 = pxmlData.GetAttr("d_custom_4");
                else
                    objCV.d_custom_4Null = true;

                if (pxmlData.GetAttr("d_custom_5") != "")
                    objCV.d_custom_5 = pxmlData.GetAttr("d_custom_5");
                else
                    objCV.d_custom_5Null = true;

                if (pxmlData.GetAttr("d_custom_6") != "")
                    objCV.d_custom_6 = pxmlData.GetAttr("d_custom_6");
                else
                    objCV.d_custom_6Null = true;

                if (pxmlData.GetAttr("d_custom_7") != "")
                    objCV.d_custom_7 = pxmlData.GetAttr("d_custom_7");
                else
                    objCV.d_custom_7Null = true;

                if (pxmlData.GetAttr("d_custom_8") != "")
                    objCV.d_custom_8 = pxmlData.GetAttr("d_custom_8");
                else
                    objCV.d_custom_8Null = true;

                if (pxmlData.GetAttr("d_custom_9") != "")
                    objCV.d_custom_9 = pxmlData.GetAttr("d_custom_9");
                else
                    objCV.d_custom_9Null = true;

                if (pxmlData.GetAttr("d_custom_10") != "")
                    objCV.d_custom_10 = pxmlData.GetAttr("d_custom_10");
                else
                    objCV.d_custom_10Null = true;

                //----------------------------------------------------------------------------------------------------
                //Graba las colecciones
                strStep = "MC"; // Modificando los Children
                for (NomadXML xmlChildren = pxmlData.FirstChild(); xmlChildren != null; xmlChildren = xmlChildren.Next())
                {
                    switch (xmlChildren.Name)
                    {
                        case "ESTUDIOS_CV": CV.SaveESTUDIOS_CV(objCV, xmlChildren); break;
                        case "EXPERIENCIA": CV.SaveEXPERIENCIA(objCV, xmlChildren); break;
                        case "IDIOMAS_CV": CV.SaveIDIOMAS_CV(objCV, xmlChildren); break;
                        case "INFORMATICA": CV.SaveINFORMATICA(objCV, xmlChildren); break;
                        case "AREAS_CV": CV.SaveAREAS_CV(objCV, xmlChildren); break;
                    }
                }

                //----------------------------------------------------------------------------------------------------
                strStep = "SA"; // Realizando el SAVE
                NomadBatch.Trace("--------------------------------------------------------------------------");
                NomadBatch.Trace("Realiza el SAVE...");
                NomadBatch.Trace("--------------------------------------------------------------------------");
                NomadEnvironment.GetCurrentTransaction().Save(objCV);

                //----------------------------------------------------------------------------------------------------

                NomadBatch.Trace("Obtiene el query nuevamente para actualizar el cliente...");
                xmlResult.SetAttr("oi_cv", objCV.Id);
                xmlResult.SetAttr("c_seguridad", objCV.c_seguridad);
                xmlResult = CVs.CV.GetCV(xmlResult);

                NomadBatch.Trace("--------------------------------------------------------------------------");
                NomadBatch.Trace("El metodo se finalizo correctamente.");
                NomadBatch.Trace("--------------------------------------------------------------------------");

            }
            catch (Exception ex)
            {
                NomadBatch.Trace("--------------------------------------------------------------------------");
                NomadBatch.Trace("ERROR");
                NomadBatch.Trace("DESCRIPTION: " + ex.Message);

                xmlResult.SetAttr("out", "Se produjo un error guardando los datos. Por favor inténtelo nuevamente en unos minutos.");

                switch (strStep)
                {
                    case "MC":
                        NomadBatch.Trace("Se produjo un error al recuperar moficando las colecciones.");
                        xmlResult.SetAttr("out", "Se produjo un error guardando los datos. " + ex.Message);
                        break;

                    case "RD":
                        NomadBatch.Trace("Se produjo un error al recuperar el DDO con el OI:'" + objCV + "'");
                        break;

                    case "CD":
                        NomadBatch.Trace("Se produjo un error Realizando la carga de los datos.");
                        break;

                    case "SA":
                        NomadBatch.Trace("Se produjo un error Realizando el Save.");

                        try
                        {
                            NomadBatch.Trace("CV serializado: " + objCV.SerializeAll());
                        }
                        catch (Exception) { }

                        switch (ex.Message)
                        {
                            case "DB.SQLSERVER.2627":
                            case "DB.ORA.1":
                                //Violacion de clave primaria
                                xmlResult.SetAttr("out", "Ya existe el usuario con nro. doc " + pxmlData.GetAttr("c_nro_doc") + ".");
                                break;
                        }

                        break;

                }

                NomadBatch.Trace("--------------------------------------------------------------------------");
            }

            return xmlResult;
        }

        /// <summary>
        /// Realiza la grabacion de los estudios
        /// </summary>
        /// <param name="pobjCV"></param>
        /// <param name="pxmlData"></param>
        private static void SaveESTUDIOS_CV(NucleusRH.Base.SeleccionDePostulantes.CVs.CV pobjCV, NomadXML pxmlData)
        {
            NucleusRH.Base.SeleccionDePostulantes.CVs.ESTUDIO_CV objEstudio;
            Hashtable htaMod;
            ArrayList arrNews;
            ArrayList arrDels;
            NomadXML xmlChild;

            NomadBatch.Trace("--------------------------------------------------------------------------");
            NomadBatch.Trace("Comienza la grabacion de los ESTUDIOS (Formacion)");
            NomadBatch.Trace("--------------------------------------------------------------------------");

            htaMod = new Hashtable();
            arrNews = new ArrayList();
            arrDels = new ArrayList();

            //Carga las colecciones de nuevos y modificados
            NomadBatch.Trace("Crea las colecciones temporales");
            for (NomadXML xmlC = pxmlData.FirstChild(); xmlC != null; xmlC = xmlC.Next())
            {
                if (xmlC.GetAttr("oi_estudio_cv") != "")
                {
                    htaMod.Add(xmlC.GetAttr("oi_estudio_cv"), xmlC);
                }
                else
                {
                    arrNews.Add(xmlC);
                }
            }

            //Recorre la colección de hijos. MODIFICA los que están en la lista de modificados y marca para eliminar los que no están.
            NomadBatch.Trace("Modifica los hijos existentes");
            foreach (NucleusRH.Base.SeleccionDePostulantes.CVs.ESTUDIO_CV objEstudioMod in pobjCV.ESTUDIOS_CV)
            {

                if (htaMod.ContainsKey(objEstudioMod.Id))
                {

                    xmlChild = (NomadXML)htaMod[objEstudioMod.Id];

                    //Verifica si fue realmente modificado
                    if (xmlChild.GetAttr("nmdmod") != "1") continue;

                    objEstudioMod.oi_nivel_estudio = xmlChild.GetAttr("oi_nivel_estudio");
                    objEstudioMod.oi_area_est = xmlChild.GetAttr("oi_area_est");
                    objEstudioMod.c_estado = xmlChild.GetAttr("c_estado");

                    if (xmlChild.GetAttr("d_otro_est_educ") != "")
                        objEstudioMod.d_otro_est_educ = xmlChild.GetAttr("d_otro_est_educ");
                    else
                        objEstudioMod.d_otro_est_educNull = true;

                    if (xmlChild.GetAttr("d_nom_carrera") != "")
                        objEstudioMod.d_nom_carrera = xmlChild.GetAttr("d_nom_carrera");
                    else
                        objEstudioMod.d_nom_carreraNull = true;

                    if (xmlChild.GetAttr("d_institucion") != "")
                        objEstudioMod.d_institucion = xmlChild.GetAttr("d_institucion");
                    else
                        objEstudioMod.d_institucionNull = true;

                    if (xmlChild.GetAttr("e_dur_carrera") != "")
                        objEstudioMod.e_dur_carrera = xmlChild.GetAttrInt("e_dur_carrera");
                    else
                        objEstudioMod.e_dur_carreraNull = true;

                    if (xmlChild.GetAttr("oi_unidad_tiempo") != "")
                        objEstudioMod.oi_unidad_tiempo = xmlChild.GetAttr("oi_unidad_tiempo");
                    else
                        objEstudioMod.oi_unidad_tiempoNull = true;

                    if (xmlChild.GetAttr("d_localidad") != "")
                        objEstudioMod.d_localidad = xmlChild.GetAttr("d_localidad");
                    else
                        objEstudioMod.d_localidadNull = true;

                    if (xmlChild.GetAttr("e_ano_inicio") != "")
                        objEstudioMod.e_ano_inicio = xmlChild.GetAttrInt("e_ano_inicio");
                    else
                        objEstudioMod.e_ano_inicioNull = true;

                    if (xmlChild.GetAttr("e_ano_fin") != "")
                        objEstudioMod.e_ano_fin = xmlChild.GetAttrInt("e_ano_fin");
                    else
                        objEstudioMod.e_ano_finNull = true;

                    if (xmlChild.GetAttr("e_ano_curso") != "")
                        objEstudioMod.e_ano_curso = xmlChild.GetAttrInt("e_ano_curso");
                    else
                        objEstudioMod.e_ano_cursoNull = true;

                    if (xmlChild.GetAttr("e_ult_ano_cursado") != "")
                        objEstudioMod.e_ult_ano_cursado = xmlChild.GetAttrInt("e_ult_ano_cursado");
                    else
                        objEstudioMod.e_ult_ano_cursadoNull = true;

                    if (xmlChild.GetAttr("oi_estudio") != "")
                    {
                        objEstudioMod.oi_estudio = xmlChild.GetAttr("oi_estudio");
                        if (objEstudioMod.oi_area_estNull)
                            objEstudioMod.oi_area_est = NomadEnvironment.QueryValue("ORG11_ESTUDIOS", "oi_area_est", "oi_estudio", xmlChild.GetAttr("oi_estudio"), "", true);
                    }
                    else
                        objEstudioMod.oi_estudioNull = true;

                }
                else
                {
                    arrDels.Add(objEstudioMod);
                }
            }

            //Recorre la colección de hijos a eliminar y los ELIMINA.
            NomadBatch.Trace("Elimina los hijos no existentes");
            for (int X = 0; X < arrDels.Count; X++)
            {
                objEstudio = (ESTUDIO_CV)arrDels[X];
                pobjCV.ESTUDIOS_CV.Remove(objEstudio);
            }

            //Recorre la lista de objetos nuevos
            NomadBatch.Trace("Inserta los nuevos hijos");
            for (int X = 0; X < arrNews.Count; X++)
            {
                xmlChild = (NomadXML)arrNews[X];

                objEstudio = new ESTUDIO_CV();
                objEstudio.oi_nivel_estudio = xmlChild.GetAttr("oi_nivel_estudio");
                objEstudio.oi_area_est = xmlChild.GetAttr("oi_area_est");
                objEstudio.c_estado = xmlChild.GetAttr("c_estado");

                if (xmlChild.GetAttr("d_otro_est_educ") != "")
                    objEstudio.d_otro_est_educ = xmlChild.GetAttr("d_otro_est_educ");

                if (xmlChild.GetAttr("d_nom_carrera") != "")
                    objEstudio.d_nom_carrera = xmlChild.GetAttr("d_nom_carrera");

                if (xmlChild.GetAttr("d_institucion") != "")
                    objEstudio.d_institucion = xmlChild.GetAttr("d_institucion");

                if (xmlChild.GetAttr("e_dur_carrera") != "")
                    objEstudio.e_dur_carrera = xmlChild.GetAttrInt("e_dur_carrera");

                if (xmlChild.GetAttr("oi_unidad_tiempo") != "")
                    objEstudio.oi_unidad_tiempo = xmlChild.GetAttr("oi_unidad_tiempo");

                if (xmlChild.GetAttr("d_localidad") != "")
                    objEstudio.d_localidad = xmlChild.GetAttr("d_localidad");

                if (xmlChild.GetAttr("e_ano_inicio") != "")
                    objEstudio.e_ano_inicio = xmlChild.GetAttrInt("e_ano_inicio");

                if (xmlChild.GetAttr("e_ano_fin") != "")
                    objEstudio.e_ano_fin = xmlChild.GetAttrInt("e_ano_fin");

                if (xmlChild.GetAttr("e_ano_curso") != "")
                    objEstudio.e_ano_curso = xmlChild.GetAttrInt("e_ano_curso");

                if (xmlChild.GetAttr("e_ult_ano_cursado") != "")
                    objEstudio.e_ult_ano_cursado = xmlChild.GetAttrInt("e_ult_ano_cursado");

                NomadLog.Info("ESTUDIO -- " + xmlChild.GetAttr("oi_estudio"));
                if (xmlChild.GetAttr("oi_estudio") != "")
                {
                    objEstudio.oi_estudio = xmlChild.GetAttr("oi_estudio");
                    NomadLog.Info("objEstudio.oi_area_est -- " + objEstudio.oi_area_est);
                    if (objEstudio.oi_area_estNull)
                    {
                        string area = NomadEnvironment.QueryValue("ORG11_ESTUDIOS", "oi_area_est", "oi_estudio", xmlChild.GetAttr("oi_estudio"), "", true);
                        NomadLog.Info("area -- " + area);
                        objEstudio.oi_area_est = area;
                    }
                }

                pobjCV.ESTUDIOS_CV.Add(objEstudio);

            }

            if (arrNews.Count > 0 || htaMod.Count > 0 || arrDels.Count > 0)
            {
                NomadBatch.Trace("Detalle:");
                if (arrNews.Count > 0) NomadBatch.Trace("    Nuevos:      " + arrNews.Count.ToString());
                if (htaMod.Count > 0) NomadBatch.Trace("    Modificados: " + htaMod.Count.ToString());
                if (arrDels.Count > 0) NomadBatch.Trace("    Eliminados:  " + arrDels.Count.ToString());
            }

        }

        /// <summary>
        /// Realiza la grabacion de la experiencia
        /// </summary>
        /// <param name="pobjCV"></param>
        /// <param name="pxmlData"></param>
        private static void SaveEXPERIENCIA(NucleusRH.Base.SeleccionDePostulantes.CVs.CV pobjCV, NomadXML pxmlData)
        {
            EXPERIENCIA objExpe;
            Hashtable htaMod;
            ArrayList arrNews;
            ArrayList arrDels;
            NomadXML xmlChild;

            NomadBatch.Trace("--------------------------------------------------------------------------");
            NomadBatch.Trace("Comienza la grabacion de la EXPERIENCIA");
            NomadBatch.Trace("--------------------------------------------------------------------------");

            htaMod = new Hashtable();
            arrNews = new ArrayList();
            arrDels = new ArrayList();

            //Carga las colecciones de nuevos y modificados
            NomadBatch.Trace("Crea las colecciones temporales");
            for (NomadXML xmlC = pxmlData.FirstChild(); xmlC != null; xmlC = xmlC.Next())
            {
                if (xmlC.GetAttr("oi_experiencia") != "")
                {
                    htaMod.Add(xmlC.GetAttr("oi_experiencia"), xmlC);
                }
                else
                {
                    arrNews.Add(xmlC);
                }
            }

            //Recorre la colección de hijos. MODIFICA los que se están en la lista de modificados y marca para eliminar los que no están.
            NomadBatch.Trace("Modifica los hijos existentes");
            foreach (NucleusRH.Base.SeleccionDePostulantes.CVs.EXPERIENCIA objExpeMod in pobjCV.EXPERIENCIA)
            {

                if (htaMod.ContainsKey(objExpeMod.Id))
                {

                    xmlChild = (NomadXML)htaMod[objExpeMod.Id];

                    //Verifica si fue realmente modificado
                    if (xmlChild.GetAttr("nmdmod") != "1") continue;

                    objExpeMod.oi_puesto_exp = xmlChild.GetAttr("oi_puesto_exp");

                    if (xmlChild.GetAttr("d_empresa") == "")
                        objExpeMod.d_empresaNull = true;
                    else
                        objExpeMod.d_empresa = xmlChild.GetAttr("d_empresa");

                    if (xmlChild.GetAttr("d_actividad") == "")
                        objExpeMod.d_actividadNull = true;
                    else
                        objExpeMod.d_actividad = xmlChild.GetAttr("d_actividad");

                    if (xmlChild.GetAttr("oi_area_exp") != "")
                        objExpeMod.oi_area_exp = xmlChild.GetAttr("oi_area_exp");
                    else
                        objExpeMod.oi_area_expNull = true;

                    if (xmlChild.GetAttr("o_contacto") == "")
                        objExpeMod.o_contactoNull = true;
                    else
                        objExpeMod.o_contacto = xmlChild.GetAttr("o_contacto");

                    objExpeMod.f_ingreso = new DateTime(xmlChild.GetAttrInt("ano_f_ingreso"), xmlChild.GetAttrInt("mes_f_ingreso"), xmlChild.GetAttrInt("dia_f_ingreso"));

                    objExpeMod.l_actual = xmlChild.GetAttrBool("l_actual");
                    objExpeMod.l_per_cargo = xmlChild.GetAttrBool("l_per_cargo");

                    if (xmlChild.GetAttr("dia_f_egreso") == "" || objExpeMod.l_actual)
                        objExpeMod.f_egresoNull = true;
                    else
                        objExpeMod.f_egreso = new DateTime(xmlChild.GetAttrInt("ano_f_egreso"), xmlChild.GetAttrInt("mes_f_egreso"), xmlChild.GetAttrInt("dia_f_egreso"));

                    if (xmlChild.GetAttr("o_tareas") == "")
                        objExpeMod.o_tareasNull = true;
                    else
                        objExpeMod.o_tareas = xmlChild.GetAttr("o_tareas");

                    if (xmlChild.GetAttr("d_localidad") != "")
                        objExpeMod.d_localidad = xmlChild.GetAttr("d_localidad");
                    else
                        objExpeMod.d_localidadNull = true;

                    if (xmlChild.GetAttr("oi_mod_contr") != "")
                        objExpeMod.oi_mod_contr = xmlChild.GetAttr("oi_mod_contr");
                    else
                        objExpeMod.oi_mod_contrNull = true;

                    if (xmlChild.GetAttr("oi_mot_egr") != "")
                        objExpeMod.oi_mot_egr = xmlChild.GetAttr("oi_mot_egr");
                    else
                        objExpeMod.oi_mot_egrNull = true;

                    if (xmlChild.GetAttr("d_custom_1") != "")
                        objExpeMod.d_custom_1 = xmlChild.GetAttr("d_custom_1");
                    else
                        objExpeMod.d_custom_1Null = true;

                    if (xmlChild.GetAttr("d_custom_2") != "")
                        objExpeMod.d_custom_2 = xmlChild.GetAttr("d_custom_2");
                    else
                        objExpeMod.d_custom_2Null = true;

                    if (xmlChild.GetAttr("d_custom_3") != "")
                        objExpeMod.d_custom_3 = xmlChild.GetAttr("d_custom_3");
                    else
                        objExpeMod.d_custom_3Null = true;

                    if (xmlChild.GetAttr("d_custom_4") != "")
                        objExpeMod.d_custom_4 = xmlChild.GetAttr("d_custom_4");
                    else
                        objExpeMod.d_custom_4Null = true;

                    if (xmlChild.GetAttr("d_custom_5") != "")
                        objExpeMod.d_custom_5 = xmlChild.GetAttr("d_custom_5");
                    else
                        objExpeMod.d_custom_5Null = true;

                    if (xmlChild.GetAttr("d_custom_6") != "")
                        objExpeMod.d_custom_6 = xmlChild.GetAttr("d_custom_6");
                    else
                        objExpeMod.d_custom_6Null = true;

                    if (xmlChild.GetAttr("d_custom_7") != "")
                        objExpeMod.d_custom_7 = xmlChild.GetAttr("d_custom_7");
                    else
                        objExpeMod.d_custom_7Null = true;

                    if (xmlChild.GetAttr("d_custom_8") != "")
                        objExpeMod.d_custom_8 = xmlChild.GetAttr("d_custom_8");
                    else
                        objExpeMod.d_custom_8Null = true;

                    if (xmlChild.GetAttr("d_custom_9") != "")
                        objExpeMod.d_custom_9 = xmlChild.GetAttr("d_custom_9");
                    else
                        objExpeMod.d_custom_9Null = true;

                    if (xmlChild.GetAttr("d_custom_10") != "")
                        objExpeMod.d_custom_10 = xmlChild.GetAttr("d_custom_10");
                    else
                        objExpeMod.d_custom_10Null = true;

                }
                else
                {
                    arrDels.Add(objExpeMod);
                }
            }

            //Recorre la colección de hijos a eliminar y los ELIMINA.
            NomadBatch.Trace("Elimina los hijos no existentes");
            for (int X = 0; X < arrDels.Count; X++)
            {
                objExpe = (EXPERIENCIA)arrDels[X];
                pobjCV.EXPERIENCIA.Remove(objExpe);
            }

            //Recorre la lista de objetos nuevos
            NomadBatch.Trace("Inserta los nuevos hijos");
            for (int X = 0; X < arrNews.Count; X++)
            {
                xmlChild = (NomadXML)arrNews[X];

                objExpe = new EXPERIENCIA();

                objExpe.oi_puesto_exp = xmlChild.GetAttr("oi_puesto_exp");

                if (xmlChild.GetAttr("d_empresa") != "")
                    objExpe.d_empresa = xmlChild.GetAttr("d_empresa");

                if (xmlChild.GetAttr("d_actividad") != "")
                    objExpe.d_actividad = xmlChild.GetAttr("d_actividad");

                if (xmlChild.GetAttr("oi_area_exp") != "")
                    objExpe.oi_area_exp = xmlChild.GetAttr("oi_area_exp");

                if (xmlChild.GetAttr("o_contacto") != "")
                    objExpe.o_contacto = xmlChild.GetAttr("o_contacto");

                objExpe.f_ingreso = new DateTime(xmlChild.GetAttrInt("ano_f_ingreso"), xmlChild.GetAttrInt("mes_f_ingreso"), xmlChild.GetAttrInt("dia_f_ingreso"));

                objExpe.l_actual = xmlChild.GetAttrBool("l_actual");
                objExpe.l_per_cargo = xmlChild.GetAttrBool("l_per_cargo");

                if (xmlChild.GetAttr("dia_f_egreso") != "" && objExpe.l_actual == false)
                    objExpe.f_egreso = new DateTime(xmlChild.GetAttrInt("ano_f_egreso"), xmlChild.GetAttrInt("mes_f_egreso"), xmlChild.GetAttrInt("dia_f_egreso"));

                if (xmlChild.GetAttr("o_tareas") != "")
                    objExpe.o_tareas = xmlChild.GetAttr("o_tareas");

                if (xmlChild.GetAttr("d_localidad") != "")
                    objExpe.d_localidad = xmlChild.GetAttr("d_localidad");

                if (xmlChild.GetAttr("oi_mod_contr") != "")
                    objExpe.oi_mod_contr = xmlChild.GetAttr("oi_mod_contr");

                if (xmlChild.GetAttr("oi_mot_egr") != "")
                    objExpe.oi_mot_egr = xmlChild.GetAttr("oi_mot_egr");

                if (xmlChild.GetAttr("d_custom_1") != "")
                    objExpe.d_custom_1 = xmlChild.GetAttr("d_custom_1");

                if (xmlChild.GetAttr("d_custom_2") != "")
                    objExpe.d_custom_2 = xmlChild.GetAttr("d_custom_2");

                if (xmlChild.GetAttr("d_custom_3") != "")
                    objExpe.d_custom_3 = xmlChild.GetAttr("d_custom_3");

                if (xmlChild.GetAttr("d_custom_4") != "")
                    objExpe.d_custom_4 = xmlChild.GetAttr("d_custom_4");

                if (xmlChild.GetAttr("d_custom_5") != "")
                    objExpe.d_custom_5 = xmlChild.GetAttr("d_custom_5");

                if (xmlChild.GetAttr("d_custom_6") != "")
                    objExpe.d_custom_6 = xmlChild.GetAttr("d_custom_6");

                if (xmlChild.GetAttr("d_custom_7") != "")
                    objExpe.d_custom_7 = xmlChild.GetAttr("d_custom_7");

                if (xmlChild.GetAttr("d_custom_8") != "")
                    objExpe.d_custom_8 = xmlChild.GetAttr("d_custom_8");

                if (xmlChild.GetAttr("d_custom_9") != "")
                    objExpe.d_custom_9 = xmlChild.GetAttr("d_custom_9");

                if (xmlChild.GetAttr("d_custom_10") != "")
                    objExpe.d_custom_10 = xmlChild.GetAttr("d_custom_10");

                pobjCV.EXPERIENCIA.Add(objExpe);

            }

            //Valida los AK recorriendo la colección
            NucleusRH.Base.SeleccionDePostulantes.Puestos_Exp.PUESTO_EXP objTipoPuesto;
            Hashtable htaKeys;
            string strKey;
            NomadBatch.Trace("Valida las AK");
            htaKeys = new Hashtable();
            foreach (NucleusRH.Base.SeleccionDePostulantes.CVs.EXPERIENCIA objExpeAK in pobjCV.EXPERIENCIA)
            {
                strKey = objExpeAK.oi_puesto_exp + objExpeAK.f_ingreso.ToString("yyyyMMddHHmmss");
                if (htaKeys.ContainsKey(strKey))
                {
                    objTipoPuesto = objExpeAK.Getoi_puesto_exp();
                    throw new Exception("Existen dos EXPERIENCIAS LABORALES de tipo '" + objTipoPuesto.d_puesto_exp + "' que se encuentran ingresadas más de una vez para la misma fecha de ingreso. Por favor valide sus datos y elimine los registros repetidos.");
                }
                else
                {
                    htaKeys.Add(strKey, strKey);
                }
            }

            if (arrNews.Count > 0 || htaMod.Count > 0 || arrDels.Count > 0)
            {
                NomadBatch.Trace("Detalle:");
                if (arrNews.Count > 0) NomadBatch.Trace("    Nuevos:      " + arrNews.Count.ToString());
                if (htaMod.Count > 0) NomadBatch.Trace("    Modificados: " + htaMod.Count.ToString());
                if (arrDels.Count > 0) NomadBatch.Trace("    Eliminados:  " + arrDels.Count.ToString());
            }

        }

        /// <summary>
        /// Realiza la grabacion de los idiomas
        /// </summary>
        /// <param name="pobjCV"></param>
        /// <param name="pxmlData"></param>
        private static void SaveIDIOMAS_CV(NucleusRH.Base.SeleccionDePostulantes.CVs.CV pobjCV, NomadXML pxmlData)
        {
            NucleusRH.Base.SeleccionDePostulantes.CVs.IDIOMA_CV objIdioma;
            Hashtable htaMod;
            ArrayList arrNews;
            ArrayList arrDels;
            NomadXML xmlChild;

            NomadBatch.Trace("--------------------------------------------------------------------------");
            NomadBatch.Trace("Comienza la grabacion de los IDIOMAS");
            NomadBatch.Trace("--------------------------------------------------------------------------");

            htaMod = new Hashtable();
            arrNews = new ArrayList();
            arrDels = new ArrayList();

            //Carga las colecciones de nuevos y modificados
            NomadBatch.Trace("Crea las colecciones temporales");
            for (NomadXML xmlC = pxmlData.FirstChild(); xmlC != null; xmlC = xmlC.Next())
            {
                if (xmlC.GetAttr("oi_idioma_cv") != "")
                {
                    htaMod.Add(xmlC.GetAttr("oi_idioma_cv"), xmlC);
                }
                else
                {
                    arrNews.Add(xmlC);
                }
            }

            //Recorre la colección de hijos. MODIFICA los que se están en la lista de modificados y marca para eliminar los que no están.
            NomadBatch.Trace("Modifica los hijos existentes");
            foreach (NucleusRH.Base.SeleccionDePostulantes.CVs.IDIOMA_CV objIdiomaMod in pobjCV.IDIOMAS_CV)
            {

                if (htaMod.ContainsKey(objIdiomaMod.Id))
                {

                    xmlChild = (NomadXML)htaMod[objIdiomaMod.Id];

                    //Verifica si fue realmente modificado
                    if (xmlChild.GetAttr("nmdmod") != "1") continue;

                    objIdiomaMod.oi_idioma = xmlChild.GetAttr("oi_idioma");
                    objIdiomaMod.c_nivel_habla = xmlChild.GetAttr("c_nivel_habla");
                    objIdiomaMod.c_nivel_lee = xmlChild.GetAttr("c_nivel_lee");
                    objIdiomaMod.c_nivel_escribe = xmlChild.GetAttr("c_nivel_escribe");

                    if (xmlChild.GetAttr("d_idioma") == "")
                    {
                        objIdiomaMod.d_idiomaNull = true;
                    }
                    else
                    {
                        objIdiomaMod.d_idioma = xmlChild.GetAttr("d_idioma");
                    }

                }
                else
                {
                    arrDels.Add(objIdiomaMod);
                }
            }

            //Recorre la colección de hijos a eliminar y los ELIMINA.
            NomadBatch.Trace("Elimina los hijos no existentes");
            for (int X = 0; X < arrDels.Count; X++)
            {
                objIdioma = (IDIOMA_CV)arrDels[X];
                pobjCV.IDIOMAS_CV.Remove(objIdioma);
            }

            //Recorre la lista de objetos nuevos
            NomadBatch.Trace("Inserta los nuevos hijos");
            for (int X = 0; X < arrNews.Count; X++)
            {
                xmlChild = (NomadXML)arrNews[X];

                objIdioma = new IDIOMA_CV();
                objIdioma.oi_idioma = xmlChild.GetAttr("oi_idioma");
                objIdioma.c_nivel_habla = xmlChild.GetAttr("c_nivel_habla");
                objIdioma.c_nivel_lee = xmlChild.GetAttr("c_nivel_lee");
                objIdioma.c_nivel_escribe = xmlChild.GetAttr("c_nivel_escribe");

                if (xmlChild.GetAttr("d_idioma") != "")
                {
                    objIdioma.d_idioma = xmlChild.GetAttr("d_idioma");
                }

                pobjCV.IDIOMAS_CV.Add(objIdioma);
            }

            //Valida los AK recorriendo la colección
            NucleusRH.Base.Organizacion.Idiomas.IDIOMA objTipoIdioma;
            Hashtable htaKeys;
            NomadBatch.Trace("Valida las AK");
            htaKeys = new Hashtable();
            foreach (NucleusRH.Base.SeleccionDePostulantes.CVs.IDIOMA_CV objIdiomaAK in pobjCV.IDIOMAS_CV)
            {
                if (htaKeys.ContainsKey(objIdiomaAK.oi_idioma))
                {
                    objTipoIdioma = objIdiomaAK.Getoi_idioma();
                    throw new Exception("El IDIOMA '" + objTipoIdioma.d_idioma + "' se encuentra ingresado más de una vez. Por favor valide sus datos y elimine los registros repetidos.");
                }
                else
                {
                    htaKeys.Add(objIdiomaAK.oi_idioma, objIdiomaAK.oi_idioma);
                }
            }

        }

        /// <summary>
        /// Realiza la grabacion de los conocimientos sobre informatica
        /// </summary>
        /// <param name="pobjCV"></param>
        /// <param name="pxmlData"></param>
        private static void SaveINFORMATICA(NucleusRH.Base.SeleccionDePostulantes.CVs.CV pobjCV, NomadXML pxmlData)
        {
            NucleusRH.Base.SeleccionDePostulantes.CVs.INFORMATICA_CV objInfor;
            Hashtable htaMod;
            ArrayList arrNews;
            ArrayList arrDels;
            NomadXML xmlChild;

            NomadBatch.Trace("--------------------------------------------------------------------------");
            NomadBatch.Trace("Comienza la grabacion de los conocimientos de INFORMATICA");
            NomadBatch.Trace("--------------------------------------------------------------------------");

            htaMod = new Hashtable();
            arrNews = new ArrayList();
            arrDels = new ArrayList();

            //Carga las colecciones de nuevos y modificados
            NomadBatch.Trace("Crea las colecciones temporales");
            for (NomadXML xmlC = pxmlData.FirstChild(); xmlC != null; xmlC = xmlC.Next())
            {
                if (xmlC.GetAttr("oi_informatica_cv") != "")
                {
                    htaMod.Add(xmlC.GetAttr("oi_informatica_cv"), xmlC);
                }
                else
                {
                    arrNews.Add(xmlC);
                }
            }

            //Recorre la colección de hijos. MODIFICA los que se están en la lista de modificados y marca para eliminar los que no están.
            NomadBatch.Trace("Modifica los hijos existentes");
            foreach (NucleusRH.Base.SeleccionDePostulantes.CVs.INFORMATICA_CV objInforMod in pobjCV.INFORMATICA)
            {

                if (htaMod.ContainsKey(objInforMod.Id))
                {

                    xmlChild = (NomadXML)htaMod[objInforMod.Id];

                    //Verifica si fue realmente modificado
                    if (xmlChild.GetAttr("nmdmod") != "1") continue;

                    objInforMod.oi_informatica = xmlChild.GetAttr("oi_informatica");
                    objInforMod.c_nivel = xmlChild.GetAttr("c_nivel");

                    if (xmlChild.GetAttr("o_detalle") == "")
                    {
                        objInforMod.o_detalleNull = true;
                    }
                    else
                    {
                        objInforMod.o_detalle = xmlChild.GetAttr("o_detalle");
                    }

                }
                else
                {
                    arrDels.Add(objInforMod);
                }
            }

            //Recorre la colección de hijos a eliminar y los ELIMINA.
            NomadBatch.Trace("Elimina los hijos no existentes");
            for (int X = 0; X < arrDels.Count; X++)
            {
                objInfor = (INFORMATICA_CV)arrDels[X];
                pobjCV.INFORMATICA.Remove(objInfor);
            }

            //Recorre la lista de objetos nuevos
            NomadBatch.Trace("Inserta los nuevos hijos");
            for (int X = 0; X < arrNews.Count; X++)
            {
                xmlChild = (NomadXML)arrNews[X];

                objInfor = new INFORMATICA_CV();
                objInfor.oi_informatica = xmlChild.GetAttr("oi_informatica");
                objInfor.c_nivel = xmlChild.GetAttr("c_nivel");

                if (xmlChild.GetAttr("o_detalle") != "")
                {
                    objInfor.o_detalle = xmlChild.GetAttr("o_detalle");
                }

                pobjCV.INFORMATICA.Add(objInfor);
            }

            //Valida los AK recorriendo la colección
            NucleusRH.Base.SeleccionDePostulantes.Informatica.INFORMATICA objTipoInf;
            Hashtable htaKeys;
            NomadBatch.Trace("Valida las AK");
            htaKeys = new Hashtable();
            foreach (NucleusRH.Base.SeleccionDePostulantes.CVs.INFORMATICA_CV objInforAK in pobjCV.INFORMATICA)
            {
                if (htaKeys.ContainsKey(objInforAK.oi_informatica))
                {
                    objTipoInf = objInforAK.Getoi_informatica();
                    throw new Exception("El IDIOMA '" + objTipoInf.d_informatica + "' se encuentra ingresado más de una vez. Por favor valide sus datos y elimine los registros repetidos.");
                }
                else
                {
                    htaKeys.Add(objInforAK.oi_informatica, objInforAK.oi_informatica);
                }
            }

        }

        /// <summary>
        /// Realiza la grabacion de las areas laborales
        /// </summary>
        /// <param name="pobjCV"></param>
        /// <param name="pxmlData"></param>
        private static void SaveAREAS_CV(NucleusRH.Base.SeleccionDePostulantes.CVs.CV pobjCV, NomadXML pxmlData)
        {
            NucleusRH.Base.SeleccionDePostulantes.CVs.AREA_CV objArea;
            Hashtable htaMod;
            ArrayList arrNews;
            ArrayList arrDels;
            NomadXML xmlChild;

            NomadBatch.Trace("--------------------------------------------------------------------------");
            NomadBatch.Trace("Comienza la grabacion de las AREAS LABORALES");
            NomadBatch.Trace("--------------------------------------------------------------------------");

            htaMod = new Hashtable();
            arrNews = new ArrayList();
            arrDels = new ArrayList();

            //Carga las colecciones de nuevos y modificados
            NomadBatch.Trace("Crea las colecciones temporales");
            for (NomadXML xmlC = pxmlData.FirstChild(); xmlC != null; xmlC = xmlC.Next())
            {
                if (xmlC.GetAttr("oi_area_cv") != "")
                {
                    htaMod.Add(xmlC.GetAttr("oi_area_cv"), xmlC);

                }
                else
                {
                    arrNews.Add(xmlC);
                }
            }

            //Recorre la colección de hijos. MODIFICA los que se están en la lista de modificados y marca para eliminar los que no están.
            NomadBatch.Trace("Modifica los hijos existentes");
            foreach (NucleusRH.Base.SeleccionDePostulantes.CVs.AREA_CV objAreaMod in pobjCV.AREAS_CV)
            {
                if (htaMod.ContainsKey(objAreaMod.Id))
                {
                    xmlChild = (NomadXML)htaMod[objAreaMod.Id];

                    //Verifica si fue realmente modificado
                    if (xmlChild.GetAttr("nmdmod") != "1") continue;
                    objAreaMod.oi_area = xmlChild.GetAttr("oi_area");

                    //Modificamos los atributos del custom
                    if (xmlChild.GetAttr("e_custom_1") != "")
                        objAreaMod.e_custom_1 = int.Parse(xmlChild.GetAttr("e_custom_1"));
                    else
                        objAreaMod.e_custom_1Null = true;

                    if (xmlChild.GetAttr("e_custom_2") != "")
                        objAreaMod.e_custom_2 = int.Parse(xmlChild.GetAttr("e_custom_2"));
                    else
                        objAreaMod.e_custom_2Null = true;

                    if (xmlChild.GetAttr("e_custom_3") != "")
                        objAreaMod.e_custom_3 = int.Parse(xmlChild.GetAttr("e_custom_3"));
                    else
                        objAreaMod.e_custom_3Null = true;

                    if (xmlChild.GetAttr("e_custom_4") != "")
                        objAreaMod.e_custom_4 = int.Parse(xmlChild.GetAttr("e_custom_4"));
                    else
                        objAreaMod.e_custom_4Null = true;

                    if (xmlChild.GetAttr("e_custom_5") != "")
                        objAreaMod.e_custom_5 = int.Parse(xmlChild.GetAttr("e_custom_5"));
                    else
                        objAreaMod.e_custom_5Null = true;

                    if (xmlChild.GetAttr("d_custom_6") != "")
                        objAreaMod.d_custom_6 = xmlChild.GetAttr("d_custom_6");
                    else
                        objAreaMod.d_custom_6Null = true;

                    if (xmlChild.GetAttr("d_custom_7") != "")
                        objAreaMod.d_custom_7 = xmlChild.GetAttr("d_custom_7");
                    else
                        objAreaMod.d_custom_7Null = true;

                    if (xmlChild.GetAttr("d_custom_8") != "")
                        objAreaMod.d_custom_8 = xmlChild.GetAttr("d_custom_8");
                    else
                        objAreaMod.d_custom_8Null = true;

                    if (xmlChild.GetAttr("d_custom_9") != "")
                        objAreaMod.d_custom_9 = xmlChild.GetAttr("d_custom_9");
                    else
                        objAreaMod.d_custom_9Null = true;

                    if (xmlChild.GetAttr("d_custom_10") != "")
                        objAreaMod.d_custom_10 = xmlChild.GetAttr("d_custom_10");
                    else
                        objAreaMod.d_custom_10Null = true;

                }
                else
                {
                    arrDels.Add(objAreaMod);
                }
            }

            //Recorre la colección de hijos a eliminar y los ELIMINA.
            NomadBatch.Trace("Elimina los hijos no existentes");
            for (int X = 0; X < arrDels.Count; X++)
            {
                objArea = (AREA_CV)arrDels[X];
                pobjCV.AREAS_CV.Remove(objArea);
            }

            //Recorre la lista de objetos nuevos
            NomadBatch.Trace("Inserta los nuevos hijos");
            for (int X = 0; X < arrNews.Count; X++)
            {
                xmlChild = (NomadXML)arrNews[X];
                objArea = new AREA_CV();
                objArea.oi_area = xmlChild.GetAttr("oi_area");

                //Agregamos los atributos del custom
                if (xmlChild.GetAttr("e_custom_1") != "")
                    objArea.e_custom_1 = int.Parse(xmlChild.GetAttr("e_custom_1"));

                if (xmlChild.GetAttr("e_custom_2") != "")
                    objArea.e_custom_2 = int.Parse(xmlChild.GetAttr("e_custom_2"));

                if (xmlChild.GetAttr("e_custom_3") != "")
                    objArea.e_custom_3 = int.Parse(xmlChild.GetAttr("e_custom_3"));

                if (xmlChild.GetAttr("e_custom_4") != "")
                    objArea.e_custom_4 = int.Parse(xmlChild.GetAttr("e_custom_4"));

                if (xmlChild.GetAttr("e_custom_5") != "")
                    objArea.e_custom_5 = int.Parse(xmlChild.GetAttr("e_custom_5"));

                if (xmlChild.GetAttr("d_custom_6") != "")
                    objArea.d_custom_6 = xmlChild.GetAttr("d_custom_6");

                if (xmlChild.GetAttr("d_custom_7") != "")
                    objArea.d_custom_7 = xmlChild.GetAttr("d_custom_7");

                if (xmlChild.GetAttr("d_custom_8") != "")
                    objArea.d_custom_8 = xmlChild.GetAttr("d_custom_8");

                if (xmlChild.GetAttr("d_custom_9") != "")
                    objArea.d_custom_9 = xmlChild.GetAttr("d_custom_9");

                if (xmlChild.GetAttr("d_custom_10") != "")
                    objArea.d_custom_10 = xmlChild.GetAttr("d_custom_10");

                pobjCV.AREAS_CV.Add(objArea);
            }

            //Valida los AK recorriendo la colección
            NucleusRH.Base.SeleccionDePostulantes.Areas.AREA objTipoArea;
            Hashtable htaKeys;
            NomadBatch.Trace("Valida las AK");
            htaKeys = new Hashtable();
            foreach (NucleusRH.Base.SeleccionDePostulantes.CVs.AREA_CV objAreaAK in pobjCV.AREAS_CV)
            {
                string custom_1;
                if(objAreaAK.e_custom_1Null){custom_1="";}else{custom_1=objAreaAK.e_custom_1.ToString();};
                string custom_2;
                if(objAreaAK.e_custom_1Null){custom_2="";}else{custom_2=objAreaAK.e_custom_2.ToString();};
                string clave = objAreaAK.oi_area+custom_1+custom_2;

                if (htaKeys.ContainsKey(clave))
                {
                    objTipoArea = objAreaAK.Getoi_area();
                    //Los campos custom1 y custom2 - se utilizan como claves alternativas
                    if(custom_1!="" && custom_2!="") throw new Exception("El ÁREA LABORAL DE INTERÉS '" + objTipoArea.d_area + "' en '" + custom_1 + "' y en '" + custom_2 + "' se encuentra ingresado más de una vez. Por favor valide sus datos y elimine los registros repetidos.");
                    if(custom_1!="" && custom_2=="") throw new Exception("El ÁREA LABORAL DE INTERÉS '" + objTipoArea.d_area + "' en '" + custom_1 + "' se encuentra ingresado más de una vez. Por favor valide sus datos y elimine los registros repetidos.");
                    if(custom_1=="" && custom_2=="") throw new Exception("El ÁREA LABORAL DE INTERÉS '" + objTipoArea.d_area + "' se encuentra ingresado más de una vez. Por favor valide sus datos y elimine los registros repetidos.");
                }
                else
                {
                    htaKeys.Add(clave, clave);
                }
            }

        }

        /// <summary>
        /// Postula un CV a un aviso
        /// </summary>
        /// <param name="pxmlData"></param>
        /// <returns></returns>
        public static NomadXML Postular(NomadXML pxmlData)
        {
            NomadBatch.Trace("--------------------------------------------------------------------------");
            NomadBatch.Trace("Comienza la postulacion de un CV");
            NomadBatch.Trace("--------------------------------------------------------------------------");

            NomadXML xmlResult = new NomadXML("RESULT");
            CV objCV;
            Nomad.Base.Mail.OutputMails.MAIL objMail;
            Nomad.Base.Mail.OutputMails.DESTINATARIO objDestino;
            string strRestult = "";
            string strOIAviso;

            try
            {
                if (pxmlData.FirstChild() != null)
                    pxmlData = pxmlData.FirstChild();
            }
            catch (Exception) { }

            strOIAviso = pxmlData.GetAttr("oi_aviso");
            NomadBatch.Trace("Validando el aviso: " + strOIAviso);

            //Recupera el CV
            objCV = CV.Get(pxmlData.GetAttr("oi_cv"));

            //Valida que el CV no se haya postulado al aviso antes
            NomadBatch.Trace("Validando que ya no se encuentre postulado al aviso.");
            foreach (POSTULACIONES objPostulacion in objCV.POSTU_CV)
            {

                if (objPostulacion.oi_aviso == strOIAviso)
                {
                    strRestult = "Ya está registrado a esa oferta laboral.";
                    NomadBatch.Trace("El CV ya se encuentra postulado al aviso.");
                    break;
                }
            }

            //Postula el CV al aviso
            if (strRestult == "")
            {
                NomadBatch.Trace("-----------------------------------------------------");
                NomadBatch.Trace("Postulando el CV al aviso.");
                POSTULACIONES objPostulacion = new POSTULACIONES();
                objPostulacion.oi_aviso = strOIAviso;
                objPostulacion.f_postulacion = DateTime.Now;

                objCV.POSTU_CV.Add(objPostulacion);
                NomadBatch.Trace(". . Se postulo exitosamente.");

                try
                {
                    NomadEnvironment.GetCurrentTransaction().Save(objCV);
                    strRestult = "OK";
                }
                catch (Exception ex)
                {
                    NomadBatch.Trace(". . Se ha producido un error al intentar grabar la postulacion!!!");
                    NomadBatch.Trace(". . Error: " + ex.Message);
                    strRestult = "Se produjo un error en la postulación. Por favor reintente postularse más tarde.";
                }

            }

            if (strRestult == "OK" && pxmlData.GetAttr("noeviarmail") != "1")
            {
                //----------------------------------------------------------------------------------------------------
                //Envia los mails de la registracion
                NomadBatch.Trace("-----------------------------------------------------");
                NomadBatch.Trace("Enviando los e-Mail de confirmacion");

                NomadXML xmlInfo;
                NomadXML xmlParam = new NomadXML("PARAM");
                xmlParam.SetAttr("oi_aviso", strOIAviso);

                Hashtable htaParamas = GetParams();
                string strWebName = (string)(htaParamas.ContainsKey("WebName") ? htaParamas["WebName"] : "Nucleus S.A.");

                xmlInfo = NomadEnvironment.QueryNomadXML(NucleusRH.Base.SeleccionDePostulantes.Avisos.AVISO.Resources.infoAviso, xmlParam.ToString()).FirstChild();

                Hashtable htaMails = GetMails("POSTU", objCV, xmlInfo);

                //Pregunta si debe enviar una copia del mail a algun interesado agregado al aviso
                //06/05/2020 - No se manda más a todos los interesados, sino mediante un proceso que manda alerta mediante el proceso AUTO_AlertaPostulaciones
                /*if (xmlInfo.GetAttr("d_mail") != "" || xmlInfo.GetAttr("d_mail_alt_1") != "" || xmlInfo.GetAttr("d_mail_alt_2") != "")
                {

                    NomadBatch.Trace(". . Enviando e-Mails a interesados.");

                    objMail = new Nomad.Base.Mail.OutputMails.MAIL();
                    objMail.ASUNTO = htaMails.ContainsKey("POSTU_ASUNTO") ? (string)htaMails["POSTU_ASUNTO"] : "-";
                    objMail.CONTENIDO = htaMails.ContainsKey("POSTU_TEXTO") ? (string)htaMails["POSTU_TEXTO"] : "-";
                    objMail.DESDE_APLICACION = NomadProxy.GetProxy().AppName;
                    objMail.FECHA_CREACION = DateTime.Now;

                    //Agrega los destinatarios del mail
                    if (xmlInfo.GetAttr("d_mail") != "")
                    {
                        NomadBatch.Trace(". . Enviando e-Mail a : " + xmlInfo.GetAttr("d_mail"));

                        objDestino = new Nomad.Base.Mail.OutputMails.DESTINATARIO();
                        objDestino.MAIL_SUSTITUTO = xmlInfo.GetAttr("d_mail");
                        objMail.DESTINATARIOS.Add(objDestino);
                    }

                    if (xmlInfo.GetAttr("d_mail_alt_1") != "")
                    {
                        NomadBatch.Trace(". . Enviando e-Mail alternativo 1 a : " + xmlInfo.GetAttr("d_mail_alt_1"));

                        objDestino = new Nomad.Base.Mail.OutputMails.DESTINATARIO();
                        objDestino.MAIL_SUSTITUTO = xmlInfo.GetAttr("d_mail_alt_1");
                        objMail.DESTINATARIOS.Add(objDestino);
                    }

                    if (xmlInfo.GetAttr("d_mail_alt_2") != "")
                    {
                        NomadBatch.Trace(". . Enviando e-Mail alternativo 2 a : " + xmlInfo.GetAttr("d_mail_alt_2"));

                        objDestino = new Nomad.Base.Mail.OutputMails.DESTINATARIO();
                        objDestino.MAIL_SUSTITUTO = xmlInfo.GetAttr("d_mail_alt_2");
                        objMail.DESTINATARIOS.Add(objDestino);
                    }

                    try
                    {
                        NomadEnvironment.GetCurrentTransaction().Save(objMail);
                    }
                    catch (Exception ex)
                    {
                        NomadBatch.Trace(". . Se ha producido un error al intentar enviar el mail de confirmacion de postulacion a los indicados del aviso!!!");
                        NomadBatch.Trace(". . Error: " + ex.Message);
                    }
                }*/

                //Envia el mail al CV
                if (objCV.d_email != "")
                {
                    NomadBatch.Trace(". . Enviando e-Mail a CV: " + objCV.d_email);

                    objMail = new Nomad.Base.Mail.OutputMails.MAIL();
                    objMail.ASUNTO = htaMails.ContainsKey("POSTU_ASUNTO") ? (string)htaMails["POSTU_ASUNTO"] : "-";
                    objMail.CONTENIDO = htaMails.ContainsKey("POSTU_TEXTO") ? (string)htaMails["POSTU_TEXTO"] : "-";
                    objMail.DESDE_APLICACION = NomadProxy.GetProxy().AppName;
                    objMail.FECHA_CREACION = DateTime.Now;

                    objDestino = new Nomad.Base.Mail.OutputMails.DESTINATARIO();
                    objDestino.MAIL_SUSTITUTO = objCV.d_email;
                    objMail.DESTINATARIOS.Add(objDestino);

                    try
                    {
                        NomadEnvironment.GetCurrentTransaction().Save(objMail);
                    }
                    catch (Exception ex)
                    {
                        NomadBatch.Trace(". . Se ha producido un error al intentar enviar el mail de confirmacion de postulacion al postulante!!!");
                        NomadBatch.Trace(". . Error: " + ex.Message);
                    }
                }
            }

            xmlResult.SetAttr("out", strRestult);

            return xmlResult;
        }

        /// <summary>
        /// Postula un CV a un aviso
        /// </summary>
        /// <param name="pxmlData"></param>
        /// <returns></returns>
        public static NomadXML UpdateCount(NomadXML pxmlData)
        {
            NomadBatch.Trace("--------------------------------------------------------------------------");
            NomadBatch.Trace("Comienza la Actualización de los datos de la cuenta");
            NomadBatch.Trace("--------------------------------------------------------------------------");

            NomadXML xmlResult = new NomadXML("RESULT");
            CV objCV;
            Nomad.Base.Mail.OutputMails.MAIL objMail;
            Nomad.Base.Mail.OutputMails.DESTINATARIO objDestino;
            string strRestult = "";
            bool bolPassChanged = false;

            try
            {
                if (pxmlData.FirstChild() != null)
                    pxmlData = pxmlData.FirstChild();
            }
            catch (Exception) { }

            //Recupera el CV
            objCV = CV.Get(pxmlData.GetAttr("oi_cv"));

            //Actualiza los datos principales
            objCV.d_nombres = pxmlData.GetAttr("d_nombres");
            objCV.d_apellido = pxmlData.GetAttr("d_apellido");
            objCV.d_email = pxmlData.GetAttr("d_email");
            objCV.l_maillist = pxmlData.GetAttrBool("l_maillist");
            objCV.d_ape_y_nom = objCV.d_apellido + ", " + objCV.d_nombres;
            objCV.f_actualizacion = DateTime.Now;

            //Valida las claves
            if (pxmlData.GetAttr("c_password") != "")
            {

                //Valida que la clave anterior sea la actual
                if (pxmlData.GetAttr("c_password") == objCV.c_password)
                {
                    //Cambia la clave
                    objCV.c_password = pxmlData.GetAttr("c_password_2");
                    bolPassChanged = true;

                }
                else
                {
                    NomadBatch.Trace("La clave actual no corresponde con la del CV.");
                    strRestult = "La clave actual no es válida";
                }

            }

            //Persiste los datos
            if (strRestult == "")
            {
                try
                {

                    NomadEnvironment.GetCurrentTransaction().Save(objCV);
                    strRestult = "OK";

                }
                catch (Exception ex)
                {
                    NomadBatch.Trace("Se ha producido un error al intentar grabar la nueva clave.");
                    NomadBatch.Trace("Error: " + ex.Message);
                    strRestult = "Se produjo un error realizando el cambio de contraseñas. Por favor reintentelo más tarde.";
                }
            }

            if (strRestult == "OK" && bolPassChanged)
            {
                //----------------------------------------------------------------------------------------------------
                //Envia los mails de confirmacion
                //----------------------------------------------------------------------------------------------------
                NomadBatch.Trace("Crea el mail...");

                Hashtable htaMails = GetMails("REPASS", objCV, null);

                objMail = new Nomad.Base.Mail.OutputMails.MAIL();
                objMail.ASUNTO = htaMails.ContainsKey("REPASS_ASUNTO") ? (string)htaMails["REPASS_ASUNTO"] : "-";
                objMail.CONTENIDO = htaMails.ContainsKey("REPASS_TEXTO") ? (string)htaMails["REPASS_TEXTO"] : "-";
                objMail.DESDE_APLICACION = NomadProxy.GetProxy().AppName;
                objMail.FECHA_CREACION = DateTime.Now;

                objDestino = new Nomad.Base.Mail.OutputMails.DESTINATARIO();
                objDestino.MAIL_SUSTITUTO = objCV.d_email;
                objMail.DESTINATARIOS.Add(objDestino);

                try
                {
                    NomadEnvironment.GetCurrentTransaction().Save(objMail);
                }
                catch (Exception ex)
                {
                    NomadBatch.Trace("Se ha producido un error al intentar enviar el mail de confirmacion de postulacion a los indicados del aviso.");
                    NomadBatch.Trace("Error: " + ex.Message);
                }

            }

            xmlResult.SetAttr("out", strRestult);

            return xmlResult;
        }

        /// <summary>
        /// Envia un e-mail con la clave del CV
        /// </summary>
        /// <param name="pxmlData"></param>
        /// <returns></returns>
        public static NomadXML RemindPass(NomadXML pxmlData)
        {
            NomadBatch.Trace("--------------------------------------------------------------------------");
            NomadBatch.Trace("Comienza el recupero de la clave");
            NomadBatch.Trace("--------------------------------------------------------------------------");

            NomadXML xmlResult = new NomadXML("RESULT");
            NomadXML xmlData;
            string strStep = "RE";
            NucleusRH.Base.SeleccionDePostulantes.CVs.CV objCV;
            Nomad.Base.Mail.OutputMails.MAIL objMail;
            Nomad.Base.Mail.OutputMails.DESTINATARIO objDestino;

            pxmlData = pxmlData.FirstChild();

            try
            {
                //----------------------------------------------------------------------------------------------------
                NomadBatch.Trace("Recupera los datos...");
                strStep = "RE"; // Recuperando la informacion
                xmlData = NomadEnvironment.QueryNomadXML(CV.Resources.getPass, pxmlData.ToString()).FirstChild();

                if (xmlData.GetAttr("c_password") == "")
                {
                    strStep = "NF"; //El usuario ingresado no se encuentra registrado
                    throw new Exception("El usuario ingresado no se encuentra registrado.");

                }
                else
                {
                    //----------------------------------------------------------------------------------------------------
                    NomadBatch.Trace("Recupera el CV " + xmlData.GetAttr("oi_cv") + "...");
                    strStep = "EM"; // Enviando el e-mail

                    //Recupera el CV para poder enviar el mail
                    objCV = NucleusRH.Base.SeleccionDePostulantes.CVs.CV.Get(xmlData.GetAttr("oi_cv"));

                    NomadBatch.Trace("Crea el mail para el oi_cv " + xmlData.GetAttr("oi_cv") + "...");

                    Hashtable htaMails = GetMails("PASS", objCV, null);

                    objMail = new Nomad.Base.Mail.OutputMails.MAIL();
                    objMail.ASUNTO = htaMails.ContainsKey("PASS_ASUNTO") ? (string)htaMails["PASS_ASUNTO"] : "-";
                    objMail.CONTENIDO = htaMails.ContainsKey("PASS_TEXTO") ? (string)htaMails["PASS_TEXTO"] : "-";
                    objMail.DESDE_APLICACION = NomadProxy.GetProxy().AppName;
                    objMail.FECHA_CREACION = DateTime.Now;

                    objDestino = new Nomad.Base.Mail.OutputMails.DESTINATARIO();
                    objDestino.MAIL_SUSTITUTO = objCV.d_email;
                    objMail.DESTINATARIOS.Add(objDestino);

                    NomadEnvironment.GetCurrentTransaction().Save(objMail);

                    NomadBatch.Trace("Mail enviado a la direccion: " + objCV.d_email);

                    xmlResult.SetAttr("out", "OK|" + xmlData.GetAttr("d_email"));

                    NomadBatch.Trace("--------------------------------------------------------------------------");
                    NomadBatch.Trace("El metodo se finalizo correctamente.");
                    NomadBatch.Trace("--------------------------------------------------------------------------");
                }

            }
            catch (Exception ex)
            {
                xmlResult.SetAttr("out", "err");

                NomadBatch.Trace("--------------------------------------------------------------------------");
                NomadBatch.Trace("ERROR");
                NomadBatch.Trace("DESCRIPTION: " + ex.Message);

                switch (strStep)
                {
                    case "RE":
                        xmlResult.SetAttr("out", "Se produjo un error Recuperando los datos.");
                        NomadBatch.Trace("Se produjo un error Recuperando los datos.");
                        break;

                    case "ME":
                        xmlResult.SetAttr("out", "Se produjo un error enviando el mail.");
                        NomadBatch.Trace("Se produjo un error enviando el mail.");
                        break;

                    case "NF":
                        xmlResult.SetAttr("out", "El usuario ingresado no se encuentra registrado.");
                        NomadBatch.Trace("El usuario ingresado no se encuentra registrado.");
                        break;
                }

                NomadBatch.Trace("--------------------------------------------------------------------------");
            }

            return xmlResult;

        }

        /// <summary>
        /// Retorna una Hashtable con los parametros en la base de datos el modulo
        /// </summary>
        /// <returns></returns>
        private static Hashtable GetParams()
        {
            NomadXML xmlParams;

            //Crea el objeto de Parametros
            xmlParams = new NomadXML("DATOS");
            xmlParams.SetAttr("c_modulo", "PDM");
            xmlParams.SetAttr("d_clase", "\\'WEB\\'");

            //Ejecuta el query
            return NomadEnvironment.QueryHashtableValue(NucleusRH.Base.Organizacion.Parametros.PARAMETRO.Resources.QRY_Parametros, xmlParams.ToString(), "c_parametro", "d_valor", false);
        }

        /// <summary>
        /// Retorna una Hashtable con el asunto y el texto del mail solicitado
        /// </summary>
        /// <returns></returns>
        public static Hashtable GetMails(string pstrCode, CV pobjCV, NomadXML pxmlInfo)
        {
            NomadXML xmlParams;
            NomadXML xmlResult;
            Hashtable htaResult;

            //Crea el objeto de Parametros
            xmlParams = new NomadXML("PARAM");
            xmlParams.SetAttr("cod", pstrCode);

            //Ejecuta el query
            xmlResult = NomadEnvironment.QueryNomadXML(NucleusRH.Base.SeleccionDePostulantes.Textos_Mails.TEXTO_MAIL.Resources.QRY_Mails, xmlParams.ToString());
            xmlResult = xmlResult.FirstChild();

            htaResult = new Hashtable();

            //Reemplaza los valores
            for (NomadXML xmlMail = xmlResult.FirstChild(); xmlMail != null; xmlMail = xmlMail.Next())
            {
                htaResult.Add(xmlMail.GetAttr("c_texto_mail"), GetMailText(xmlMail.GetAttr("t_texto_mail"), pobjCV, pxmlInfo));
            }

            return htaResult;

        }

        /// <summary>
        /// Retorna el texto del mail realizando los replace correspondientes
        /// </summary>
        /// <param name="pstrText">Texto a modificar</param>
        /// <param name="pobjCV">Objeto Mail</param>
        /// <param name="pxmlInfo"></param>
        /// <returns></returns>
        private static string GetMailText(string pstrText, CV pobjCV, NomadXML pxmlInfo)
        {
            string strContent;
            string strBefore;
            string strToReplace;
            System.Text.StringBuilder sbResult = new System.Text.StringBuilder();
            int intControl = 0;

            try
            {
                strContent = pstrText;

                //Busca los sectores a reemplazar
                while (strContent != "" || intControl < 50)
                {

                    strBefore = SubStringBefore(strContent, tmpOpen);
                    strContent = SubStringAfter(strContent, tmpOpen);

                    sbResult.Append(strBefore);

                    if (strContent != "")
                    {
                        strToReplace = SubStringBefore(strContent, tmpClose);
                        strContent = SubStringAfter(strContent, tmpClose);

                        if (strToReplace != "")
                        {
                            //Se realiza el reemplazo del texto por lo indicado
                            switch (strToReplace.ToUpper())
                            {
                                case "S.FECHA":
                                    sbResult.Append(DateTime.Now.ToString("dd/MM/yyyy"));
                                    break;

                                case "S.HORA":
                                    sbResult.Append(DateTime.Now.ToString("HH:mm"));
                                    break;

                                case "S.BIENVENIDO":
                                    if (pobjCV != null) sbResult.Append(pobjCV.c_sexo == "M" ? "Bienvenido" : "Bienvenida");
                                    break;

                                case "S.ESTIMADO":
                                    if (pobjCV != null) sbResult.Append(pobjCV.c_sexo == "M" ? "Estimado" : "Estimada");
                                    break;

                                case "C.NOMBRES":
                                    if (pobjCV != null) sbResult.Append(pobjCV.d_nombres);
                                    break;

                                case "C.APELLIDO":
                                    if (pobjCV != null) sbResult.Append(pobjCV.d_apellido);
                                    break;

                                case "C.PASSWORD":
                                    if (pobjCV != null) sbResult.Append(pobjCV.c_password);
                                    break;

                                case "O.CODIGO":
                                    if (pxmlInfo != null) sbResult.Append(pxmlInfo.GetAttr("c_aviso"));
                                    break;

                                case "O.DECRIP":
                                    if (pxmlInfo != null) sbResult.Append(pxmlInfo.GetAttr("d_puesto"));
                                    break;
                            }

                        }

                    }

                    intControl++;
                }

            }
            catch
            {
                return "";
            }

            return sbResult.ToString();

        }

        private static string SubStringBefore(string pSource, string pTarget)
        {
            string strResult;
            int intPos;

            intPos = pSource.IndexOf(pTarget);
            if (intPos >= 0)
            {
                strResult = pSource.Substring(0, intPos);
            }
            else
            {
                strResult = pSource;
            }

            return strResult;
        }

        private static string SubStringAfter(string pSource, string pTarget)
        {
            string strResult = "";
            int intPos;

            intPos = pSource.IndexOf(pTarget);
            if (intPos >= 0)
            {
                intPos = intPos + pTarget.Length;
                strResult = pSource.Substring(intPos, pSource.Length - intPos);
            }

            return strResult;
        }

        public static Nomad.NSystem.Proxy.NomadXML ValidatePass(Nomad.NSystem.Proxy.NomadXML pxmlParam)
        {
            NomadXML xmlResult;

            NomadEnvironment.GetTrace().Info("****** Valida el Password ******");

            xmlResult = NomadEnvironment.QueryNomadXML(CV.Resources.loginUsuario, pxmlParam.ToString()).FirstChild();

            if (xmlResult.GetAttrString("oi_cv") != "")
            {
                xmlResult.SetAttr("out", "OK");

            }
            else
            {
                xmlResult.SetAttr("out", "Ingreso fallido. Por favor valide el documento o la clave.");
            }

            return xmlResult;
        }

        /// <summary>
        /// Valida el usuario y clave y retorna OK si se puede logear
        /// </summary>
        /// <param name="pxmlParam">XML con los datos a validar</param>
        /// <returns></returns>
        public static Nomad.NSystem.Proxy.NomadXML WebCVLogin(Nomad.NSystem.Proxy.NomadXML pxmlParam) {
            NomadXML xmlResult;

            NomadLog.Info("--------------------------------------------------------------------------");
            NomadLog.Info("Comienza el login a la WebCV (WebCVLogin).");
            NomadLog.Info("--------------------------------------------------------------------------");

            /*
            Se utiliza este nuevo método para las Web's. Cuando el usuario y clave no son válidos el sistema arroja error
            para que el sistema pueda contabilizar estos fallidos, y llegado el caso, bloquear la IP.

            Esta función se agregó tras el pedido de Williner.

            Se deberían migrar las viejas CV a este nuevo sistema.
            */
            xmlResult = NomadEnvironment.QueryNomadXML(CV.Resources.loginUsuario, pxmlParam.ToString()).FirstChild();

            if (xmlResult.GetAttrString("oi_cv") != "") {
                xmlResult.SetAttr("out", "OK");
            } else {
                throw NomadException.NewMessage("NMD-LOGINFAIL");
            }

            return xmlResult;
        }

        public static Nomad.NSystem.Proxy.NomadXML GetCV(Nomad.NSystem.Proxy.NomadXML pxmlData)
        {
            NomadEnvironment.GetTrace().Info("****** Get Usuario ******");
            NomadEnvironment.GetTrace().Info("xml " + pxmlData.ToString());

            NomadXML xmlResult;
            xmlResult = NomadEnvironment.QueryNomadXML(CV.Resources.cvUsuario, pxmlData.ToString());

            if (xmlResult != null)
            {
                xmlResult = xmlResult.FirstChild();

                if (xmlResult.GetAttr("oi_cv") != "")
                {
                    xmlResult.SetAttr("out", "OK");
                    xmlResult.SetAttr("fulldata", "1");
                }
            }
            return xmlResult;
        }

        public static void PostulacionMasiva(Nomad.NSystem.Proxy.NomadXML param, string oi_aviso, bool l_mail)
        {
            NomadBatch objBatch;
            objBatch = NomadBatch.GetBatch("Iniciando...", "Postulaci&oacute;n Masiva");

            //Recupero el aviso
            NucleusRH.Base.SeleccionDePostulantes.Avisos.AVISO objAviso = Avisos.AVISO.Get(oi_aviso,false);
            if(objAviso == null)
            {
                objBatch.Err("El aviso no se encontró");
            }
            else
            {
                string tipo_aviso = objAviso.c_tipo;

                Nomad.NSystem.Proxy.NomadXML nxmRows = param.FirstChild();
                int pos = 0;

                //Cantidad de CVs a Postular
                int max = nxmRows.ChildLength;
                int correctos = 0;
                int incorrectos = 0;
                objBatch.Log("Cantidad a Postular: " + max.ToString());

                Nomad.NSystem.Proxy.NomadXML result;

                //Setear el progress al inicio
                objBatch.SetPro(5);

                string blMail = "0";
                if (!l_mail)
                    blMail = "1";

                // Para cada ROW del XML
                for (NomadXML nxmRow = nxmRows.FirstChild(); nxmRow != null; nxmRow = nxmRow.Next())
                {
                    pos++;
                    nxmRow.SetAttr("oi_cv", nxmRow.GetAttr("id"));
                    nxmRow.SetAttr("oi_aviso", oi_aviso);
                    nxmRow.SetAttr("noeviarmail", blMail);
                    NomadLog.Info("nxmRow:: " + nxmRow.ToString());
                    NucleusRH.Base.SeleccionDePostulantes.CVs.CV objCV = NucleusRH.Base.SeleccionDePostulantes.CVs.CV.Get(nxmRow.GetAttr("id"));

                    if ((tipo_aviso == "I" && objCV.oi_personalNull) || (tipo_aviso == "E" && !(objCV.oi_personalNull)))
                    {
                        incorrectos++;
                        objBatch.Wrn("[" + objCV.c_nro_doc + "] " + objCV.d_ape_y_nom + " - " + "El tipo de aviso no coincide con el CV a postular");
                        continue;
                    }

                    result = Postular(nxmRow);

                    if (result.GetAttr("out") != "OK")
                    {
                        incorrectos++;
                        objBatch.Wrn("[" + objCV.c_nro_doc + "] " + objCV.d_ape_y_nom + " - " + result.GetAttr("out").ToString());
                    }
                    else
                        correctos++;

                    objBatch.SetPro(10, 90, max, pos);
                }

                //Setear el progress al finalizar
                objBatch.Log("CVs registrados correctamente:: " + correctos.ToString());
                objBatch.Log("CVs no registrados           :: " + incorrectos.ToString());
                objBatch.SetPro(100);
            }
        }

        public void CrearPersona(string o_personal_emp,string c_sexo)
        {
            NomadLog.Info("CREA LA PERSONA");

            //DDO PERSONA
            NucleusRH.Base.Personal.Legajo.PERSONAL DDOPER;
            NucleusRH.Base.Personal.Legajo.DOMIC_PER DOMPER;
            if (this.oi_personalNull)
                DDOPER = new NucleusRH.Base.Personal.Legajo.PERSONAL();
            else
                DDOPER = NucleusRH.Base.Personal.Legajo.PERSONAL.Get(this.oi_personal);

            //Validaciones de datos obligatorios
            if (this.d_apellidoNull) throw new NomadAppException("No puede firmar el contrato - El apellido es obligatorio.");
            if (this.d_nombresNull) throw new NomadAppException("No puede firmar el contrato - El nombre es obligatorio.");
            if (this.oi_tipo_documentoNull) throw new NomadAppException("No puede firmar el contrato - El Tipo de Documento es obligatorio.");
            if (this.c_nro_docNull) throw new NomadAppException("No puede firmar el contrato - El Numero de documento es obligatorio.");
            if (c_sexo == "") throw new NomadAppException("No puede firmar el contrato - El Sexo es obligatorio.");

            DDOPER.c_personal = NomadEnvironment.QueryValue("PER20_TIPOS_DOC", "c_tipo_documento", "oi_tipo_documento", this.oi_tipo_documento, "", false) + this.c_nro_doc;
            DDOPER.oi_tipo_documento = this.oi_tipo_documento;
            DDOPER.d_apellido = this.d_apellido;
            DDOPER.d_nombres = this.d_nombres;
            DDOPER.d_ape_y_nom = this.d_ape_y_nom;
            DDOPER.c_nro_documento = this.c_nro_doc;
            DDOPER.c_sexo = c_sexo;
            DDOPER.f_nacim = this.f_nacim;
            DDOPER.c_nro_cuil = this.c_nro_cuil;
            DDOPER.d_email = this.d_email;
            DDOPER.te_celular = this.te_celular;
            if (!this.oi_estado_civilNull)
                DDOPER.oi_estado_civil = this.oi_estado_civil;

            if (!this.oi_localidadNull)
                DDOPER.oi_localidad = this.oi_localidad;

            if (!this.oi_fotoNull)
            {
                Nomad.NSystem.Proxy.BINFile objBin = NomadProxy.GetProxy().BINService().GetFile("NucleusRH.Base.SeleccionDePostulantes.DocumentosDigitales.HEAD", this.oi_foto);
                DDOPER.oi_foto = NomadProxy.GetProxy().BINService().PutFile("NucleusRH.Base.Personal.Imagenes.HEAD", objBin.Name, new MemoryStream(objBin.GetBinary())).Id;
            }

            DDOPER.o_personal = o_personal_emp;

            DDOPER.DOCUM_PER.Clear();
            //DOCUMENTO
            NucleusRH.Base.Personal.Legajo.DOCUMENTO_PER DOCPER = new NucleusRH.Base.Personal.Legajo.DOCUMENTO_PER();
            DOCPER.c_documento = this.c_nro_doc;
            DOCPER.oi_tipo_documento = this.oi_tipo_documento;
            DDOPER.DOCUM_PER.Add(DOCPER);

            DDOPER.DOMIC_PER.Clear();
            //CREO EL DOMICILIO
            DOMPER = new NucleusRH.Base.Personal.Legajo.DOMIC_PER();
            DOMPER.oi_tipo_domicilio = "1";
            DOMPER.c_postal = this.c_codigo_postal;
            DOMPER.d_calle = "Sin Calle";
            if (this.d_calle != "") DOMPER.d_calle = this.d_calle;
            DOMPER.e_numero = 0;
            if (this.c_nro != "") DOMPER.e_numero = int.Parse(this.c_nro);
            DOMPER.d_piso = this.c_piso;
            DOMPER.d_departamento = this.c_departamento;
            DOMPER.te_1 = this.te_nro;
            DOMPER.te_celular = this.te_celular;
            DOMPER.d_email = this.d_email;
            if(!this.oi_localidadNull)
                DOMPER.oi_localidad = this.oi_localidad;
            DDOPER.DOMIC_PER.Add(DOMPER);

            DDOPER.IDIOMAS_PER.Clear();
            //IDIOMAS
            foreach (IDIOMA_CV IDICV in this.IDIOMAS_CV)
            {
                if (IDICV.oi_idiomaNull) throw new NomadAppException("No puede firmar el contrato - El idioma no puede estar vacío.");

                //CREO EL IDIOMA
                NucleusRH.Base.Personal.Legajo.IDIOMA_PER IDIPER = new NucleusRH.Base.Personal.Legajo.IDIOMA_PER();
                IDIPER.oi_idioma = IDICV.oi_idioma;

                //NIVEL EQUIVALENTE
                NomadXML xmlResult;

                xmlResult = NomadEnvironment.QueryNomadXML(CV.Resources.QRY_NIVEL_IDIOMA, "<PARAM c_nivel=\"" + IDICV.c_nivel_habla + "\"/>");
                xmlResult = xmlResult.FirstChild();
                if (xmlResult.GetAttr("oi_nivel") != "")
                    IDIPER.oi_nivel_habla = xmlResult.GetAttr("oi_nivel");

                xmlResult = NomadEnvironment.QueryNomadXML(CV.Resources.QRY_NIVEL_IDIOMA, "<PARAM c_nivel=\"" + IDICV.c_nivel_lee + "\"/>");
                xmlResult = xmlResult.FirstChild();
                if (xmlResult.GetAttr("oi_nivel") != "")
                    IDIPER.oi_nivel_lee = xmlResult.GetAttr("oi_nivel");

                xmlResult = NomadEnvironment.QueryNomadXML(CV.Resources.QRY_NIVEL_IDIOMA, "<PARAM c_nivel=\"" + IDICV.c_nivel_escribe + "\"/>");
                xmlResult = xmlResult.FirstChild();
                if (xmlResult.GetAttr("oi_nivel") != "")
                    IDIPER.oi_nivel_escribe = xmlResult.GetAttr("oi_nivel");

                DDOPER.IDIOMAS_PER.Add(IDIPER);
            }

            DDOPER.ANTECEDENTES.Clear();
            //ANTECEDENTES
            foreach (EXPERIENCIA EXPCV in this.EXPERIENCIA)
            {
                if (EXPCV.f_ingresoNull) throw new NomadAppException("No puede firmar el contrato - La fecha de ingreso del Antecednete no puede estar vacía.");
                if (EXPCV.d_empresaNull) throw new NomadAppException("No puede firmar el contrato - La empresa del Antecednete no puede estar vacía.");

                //CREO EL ANTECEDENTE
                NucleusRH.Base.Personal.Legajo.ANTECEDENTE ANTPER = new NucleusRH.Base.Personal.Legajo.ANTECEDENTE();
                ANTPER.f_ingreso = EXPCV.f_ingreso;
                ANTPER.f_egreso = EXPCV.f_egreso;

                if (!EXPCV.oi_mot_egrNull)
                {
                    SeleccionDePostulantes.MotivosEgreso.MOT_EGR ME = MotivosEgreso.MOT_EGR.Get(EXPCV.oi_mot_egr);
                    if(!ME.oi_motivo_eg_perNull)
                        ANTPER.oi_motivo_eg_per = ME.oi_motivo_eg_per;
                }

                ANTPER.d_empresa = EXPCV.d_empresa;
                ANTPER.n_ultimo_sueldo = EXPCV.n_ultimo_sueldo;
                ANTPER.o_antecedente = EXPCV.o_tareas;

                DDOPER.ANTECEDENTES.Add(ANTPER);
            }

            DDOPER.ESTUDIOS_PER.Clear();
            //ESTUDIOS
            foreach (ESTUDIO_CV ESTCV in this.ESTUDIOS_CV)
            {
                if (ESTCV.oi_area_estNull) throw new NomadAppException("No puede firmar el contrato - El área de estudio no puede estar vacía.");
                if (ESTCV.oi_nivel_estudioNull) throw new NomadAppException("No puede firmar el contrato - El nivel de estudio no puede estar vacía.");

                //CREO EL ESTUDIO
                NucleusRH.Base.Personal.Legajo.ESTUDIO_PER ESTPER = new NucleusRH.Base.Personal.Legajo.ESTUDIO_PER();
                ESTPER.oi_area_est = ESTCV.oi_area_est;
                if (!ESTCV.oi_estudioNull)
                    ESTPER.oi_estudio = ESTCV.oi_estudio;
                ESTPER.oi_nivel_estudio = ESTCV.oi_nivel_estudio;
                ESTPER.e_duracion_estudio = ESTCV.e_dur_carrera;
                if (!ESTCV.oi_unidad_tiempoNull)
                    ESTPER.oi_unidad_tiempo = ESTCV.oi_unidad_tiempo;
                ESTPER.d_otro_est_educ = ESTCV.d_institucion;

                if (!ESTCV.e_ano_inicioNull && ESTCV.e_ano_inicio != 0)
                    ESTPER.f_ini_estudio = new DateTime(ESTCV.e_ano_inicio, 1, 1);
                if (!ESTCV.e_ano_finNull && ESTCV.e_ano_fin != 0)
                    ESTPER.f_fin_estudio = new DateTime(ESTCV.e_ano_fin, 1, 1);

                ESTPER.e_periodo_en_curso = ESTCV.e_ano_curso;

                DDOPER.ESTUDIOS_PER.Add(ESTPER);
            }

            //GRABO LA PERSONA
            NomadLog.Info("PERSONA -- " + DDOPER.SerializeAll());
            NomadEnvironment.GetCurrentTransaction().SaveRefresh(DDOPER);

            this.oi_personal = DDOPER.Id;
            NomadEnvironment.GetCurrentTransaction().SaveRefresh(this);
        }

        public void CrearLegajo(string oi_empresa, string oi_puesto, string oi_posicion, DateTime f_ingreso, int e_numero_legajo, string oi_tipo_personal, string oi_ctro_costo, string oi_calendario, string o_personal_emp, string o_motivo_incorp)
        {
            //CREO EL LEGAJO
            NomadLog.Info("CREO EL LEGAJO");
            NucleusRH.Base.Personal.LegajoEmpresa.PERSONAL_EMP DDOLEG = new NucleusRH.Base.Personal.LegajoEmpresa.PERSONAL_EMP();
            DDOLEG.oi_empresa = oi_empresa;
            DDOLEG.oi_personal = this.oi_personal;
            DDOLEG.e_numero_legajo = e_numero_legajo;
            if (oi_puesto != null)
                DDOLEG.oi_puesto_ult = oi_puesto;
            DDOLEG.f_desde_puesto = f_ingreso;

            //No setea la posicion en esta parte porque sino lo hace el metodo IngresoPer y este metodo no lo agrega al organigrama
            //esta asignacion la tiene que hacer el metodo AsignarPosicion
            //DDOLEG.f_desde_posicion = f_ingreso;
            //DDOLEG.oi_posicion_ult = oi_posicion;

            DDOLEG.f_ingreso = f_ingreso;
            DDOLEG.oi_tipo_personal = oi_tipo_personal;
            DDOLEG.oi_ctro_costo_ult = oi_ctro_costo;
            DDOLEG.f_desde_ccosto = f_ingreso;
            DDOLEG.oi_calendario_ult = oi_calendario;
            DDOLEG.oi_indic_activo = "1";
            DDOLEG.f_desde_calendario = f_ingreso;
            DDOLEG.o_personal_emp = o_personal_emp;
            DDOLEG.o_motivo_incorp = o_motivo_incorp;

            //TRAYECTORIA DE PUESTO
            NomadLog.Info("PUESTO");
            NucleusRH.Base.Personal.LegajoEmpresa.PUESTO_PER PUELEG = new NucleusRH.Base.Personal.LegajoEmpresa.PUESTO_PER();
            if (oi_puesto != null)
                PUELEG.oi_puesto = oi_puesto;
            PUELEG.f_ingreso = f_ingreso;

            DDOLEG.PUESTO_PER.Add(PUELEG);

            //TRAYECTORIA DE CALENDARIO
            NomadLog.Info("CALENDARIO");
            NucleusRH.Base.Personal.LegajoEmpresa.CALENDARIO_PER CALLEG = new NucleusRH.Base.Personal.LegajoEmpresa.CALENDARIO_PER();
            CALLEG.oi_calendario = oi_calendario;
            CALLEG.f_desde = f_ingreso;

            DDOLEG.CALENDARIO_PER.Add(CALLEG);

            //TRAYECTORIA DE TIPO PERSONAL
            NomadLog.Info("TIPO PERSONAL");
            NucleusRH.Base.Personal.LegajoEmpresa.TIPOP_PER TPLEG = new NucleusRH.Base.Personal.LegajoEmpresa.TIPOP_PER();
            TPLEG.oi_tipo_personal = oi_tipo_personal;
            TPLEG.f_ingreso = f_ingreso;

            DDOLEG.TIPOSP_PER.Add(TPLEG);

            //TRAYECTORIA DE INGRESOS
            NomadLog.Info("INGRESOS");
            NucleusRH.Base.Personal.LegajoEmpresa.INGRESOS_PER INGLEG = new NucleusRH.Base.Personal.LegajoEmpresa.INGRESOS_PER();
            INGLEG.f_ingreso = f_ingreso;

            DDOLEG.INGRESOS_PER.Add(INGLEG);

            //TRAYECTORIA DE CENTRO DE COSTOS
            NomadLog.Info("CENTRO COSTO");
            NucleusRH.Base.Personal.LegajoEmpresa.CCOSTO_PER CCLEG = new NucleusRH.Base.Personal.LegajoEmpresa.CCOSTO_PER();
            CCLEG.oi_centro_costo = oi_ctro_costo;
            CCLEG.f_ingreso = f_ingreso;

            DDOLEG.CCOSTO_PER.Add(CCLEG);

            NomadLog.Info("LEGAJO PRE ING -- " + DDOLEG.SerializeAll());
            DDOLEG.Ingreso_Personal();

            //GRABO EL LEGAJO
            DDOLEG.f_desde_posicion = f_ingreso;
            if (oi_posicion != null)
                DDOLEG.oi_posicion_ult = oi_posicion;

            NomadLog.Info("LEGAJO POS INGR-- " + DDOLEG.SerializeAll());
            NomadEnvironment.GetCurrentTransaction().SaveRefresh(DDOLEG);

            //POSICION
            NomadLog.Info("POSICION");
            NucleusRH.Base.Personal.LegajoEmpresa.POSICION_PER DDOPOSICLEG = new NucleusRH.Base.Personal.LegajoEmpresa.POSICION_PER();
            DDOPOSICLEG.f_ingreso = f_ingreso;
            DDOPOSICLEG.oi_posicion = oi_posicion;
            DDOPOSICLEG.o_cambio_posic = o_motivo_incorp;
            DDOLEG.Asignar_Posicion(DDOPOSICLEG);

            NucleusRH.Base.Organizacion.Puestos.POSICION.AddLegajoSectores(DDOLEG.oi_posicion_ult, DDOLEG.id.ToString(), DDOPOSICLEG.f_ingreso, new DateTime(1899, 1, 1), "", "");

        }

        public static Nomad.NSystem.Proxy.NomadXML Despostular(Nomad.NSystem.Proxy.NomadXML pxmlData)
        {
            NomadLog.Debug("-----------------------------------");
            NomadLog.Debug("-----------Desportular-------------");
            NomadLog.Debug("-----------------------------------");

            NomadXML xmlResult = new NomadXML("RESULT");

            string strRestult = "";
            string strOIAviso;
            string strOICV;

            try
            {
                if (pxmlData.FirstChild() != null)
                    pxmlData = pxmlData.FirstChild();
            }
            catch (Exception) { }

            strOIAviso = pxmlData.GetAttr("oi_aviso");
            strOICV = pxmlData.GetAttr("oi_cv");

            //Get CV
            NucleusRH.Base.SeleccionDePostulantes.CVs.CV objCV = NucleusRH.Base.SeleccionDePostulantes.CVs.CV.Get(strOICV, true);
            NucleusRH.Base.SeleccionDePostulantes.Avisos.AVISO objAviso = NucleusRH.Base.SeleccionDePostulantes.Avisos.AVISO.Get(strOIAviso, true);
            NucleusRH.Base.SeleccionDePostulantes.CVs.POSTULACIONES objPos;

            if (objCV == null)
            {
                strRestult = "No se pudo encontrar el CV con ID:" + strOICV;
                xmlResult.SetAttr("out", strRestult);
                return xmlResult;
            }
            else
            {
                try
                {
                    foreach (NucleusRH.Base.SeleccionDePostulantes.CVs.POSTULACIONES objPostu in objCV.POSTU_CV)
                    {
                        if (objPostu.oi_aviso == strOIAviso)
                        {
                            objPos = NucleusRH.Base.SeleccionDePostulantes.CVs.POSTULACIONES.Get(objPostu.Id, true);
                            NomadEnvironment.GetCurrentTransaction().Delete(objPos);
                            strRestult = "OK";
                        }
                    }

                    if(strRestult != "OK")
                        strRestult = "No se pudo encontrar la postulación de " + objCV.d_ape_y_nom + " al aviso " + objAviso.c_aviso + " - " + objAviso.d_puesto;
                }
                catch (Exception ex)
                {
                    NomadLog.Debug("Error al despostular el cv " + strOICV + " del aviso con id " + strOIAviso + " :" + ex);
                    NomadBatch.Trace(". . Se ha producido un error al intentar despostularse del aviso!!!");
                    NomadBatch.Trace(". . Error: " + ex.Message);
                }
            }

            xmlResult.SetAttr("out", strRestult);
            return xmlResult;
        }

        /// <summary>
        /// Proceso para enviar mails a los Selectores
        /// </summary>
        /// <returns></returns>
        public static void AUTO_AlertaPostulaciones()
        {
            //Proceso que se ejecuta según la programación definida que busca para cada Aviso, la cantidad de Postulaciones realizadas entre un rango de fechas.
            //Será necesario definir:
            // - Parametro: CantDiasPrevios (para determinar el rango de fechas a filtrar)
            // - POSTU_SEL_ASUNTO y POSTU_SEL_TEXTO: Textos para los mails que se enviarán a los interesados de cada aviso
            NomadBatch.Trace("--------------------------------------------------------------------------");
            NomadBatch.Trace("Comienza AUTO_AlertaPostulaciones");
            NomadBatch.Trace("--------------------------------------------------------------------------");

            NomadLog.Info("Inicio del proceso AUTO_AlertaPostulaciones");

            ArrayList listPostu;
            Hashtable htaResult = new Hashtable();

            NomadXML xmlDiasPrevios, xmlParam, xmlResult, xmlInfoMail;
            int cantDias = 0;
            DateTime fechaActual = DateTime.Now;
            DateTime fechaDesde;

            //Recupero el parametro CantDiasPrevios del modulo SDP
            xmlDiasPrevios = NomadEnvironment.QueryNomadXML(NucleusRH.Base.SeleccionDePostulantes.CVs.CV.Resources.QRY_PARAM_ALERTA, "");
            NomadLog.Info("xmlDiasPrevios = " + xmlDiasPrevios.ToString());
            NomadBatch.Trace("xmlDiasPrevios = " + xmlDiasPrevios.ToString());

            if(xmlDiasPrevios.FirstChild().GetAttr("cant")!="")
            {
                cantDias = xmlDiasPrevios.FirstChild().GetAttrInt("cant");
                fechaDesde = fechaActual.AddDays(-cantDias);

                xmlParam = new NomadXML("PARAM");
                xmlParam.SetAttr("f_desde", fechaDesde.ToString("yyyyMMdd"));
                xmlParam.SetAttr("f_hasta", fechaActual.ToString("yyyyMMdd"));

                NomadLog.Info("Buscando Avisos entre fechas - xmlParam = " + xmlParam.ToString());
                //Busco los Avisos que tiene postulaciones en el rango de fechas
                xmlResult = NomadEnvironment.QueryNomadXML(NucleusRH.Base.SeleccionDePostulantes.CVs.CV.Resources.QRY_AVISOS_ALERTA, xmlParam.ToString());

                listPostu = (ArrayList)xmlResult.FirstChild().GetElements("ROW");

                NomadBatch.Trace("Cantidad de Postulaciones = " + listPostu.Count);

                if (listPostu.Count>0)
                {
                    //Recorro para buscar: postulantes y aviso
                    for (int i = 0; i < listPostu.Count; i++)
                    {
                        string key = ((NomadXML)listPostu[i]).GetAttrString("oi_aviso");
                        if (htaResult.ContainsKey(key))
                            htaResult[key] = (int.Parse(htaResult[key].ToString()) + 1);
                        else
                            htaResult.Add(key, 1);
                    }

                    NomadLog.Info("Mando mails");
                    foreach (string clave in htaResult.Keys)
                    {
                        xmlInfoMail = new NomadXML("INFO");
                        xmlInfoMail.SetAttr("oi_aviso", clave);
                        xmlInfoMail.SetAttr("cantidad", htaResult[clave].ToString());
                        xmlInfoMail.SetAttr("cantdiasprevio", cantDias);

                        NomadXML xmlRow = (NomadXML)xmlResult.FirstChild().GetElements2("ROW", "oi_aviso", clave)[0];
                        xmlInfoMail.SetAttr("c_aviso", xmlRow.GetAttrString("c_aviso"));
                        xmlInfoMail.SetAttr("d_puesto", xmlRow.GetAttrString("d_puesto"));

                        xmlInfoMail.SetAttr("d_mail", xmlRow.GetAttrString("d_mail"));
                        xmlInfoMail.SetAttr("d_mail_alt_1", xmlRow.GetAttrString("d_mail_alt_1"));
                        xmlInfoMail.SetAttr("d_mail_alt_2", xmlRow.GetAttrString("d_mail_alt_2"));

                        EnviarAlertaPostulaciones("SEL", xmlInfoMail, listPostu);
                    }
                }
                else
                {
                    NomadLog.Warning("No se encontraron postulaciones.");
                }
            }
            else
            {
                NomadLog.Warning("No tiene definido el parametro CantDiasPrevios en el módulo SDP.");
            }

            NomadLog.Info("Fin del proceso AUTO_AlertaPostulaciones");
            NomadBatch.Trace("Fin AUTO_AlertaPostulaciones");
        }

        /// <summary>
        /// Envia Mails a los Selectores
        /// </summary>
        /// <returns></returns>
        public static void EnviarAlertaPostulaciones(string pstrCode, NomadXML pxmlInfo, ArrayList listPostu)
        {
            Nomad.Base.Mail.OutputMails.MAIL objMail;
            Nomad.Base.Mail.OutputMails.DESTINATARIO objDestino;

            NomadXML xmlParams;
            NomadXML xmlResult;

            string strContent;
            string strBefore;
            string asuntoMail = "";
            string textoMail = "";
            string strToReplace;
            System.Text.StringBuilder sbResult;
            int intControl = 0;
            int cantMaxPostu = 15;

            //Crea el objeto de Parametros
            xmlParams = new NomadXML("PARAM");
            xmlParams.SetAttr("cod", pstrCode);

            //Ejecuta el query
            xmlResult = NomadEnvironment.QueryNomadXML(NucleusRH.Base.SeleccionDePostulantes.Textos_Mails.TEXTO_MAIL.Resources.QRY_Mails, xmlParams.ToString());
            xmlResult = xmlResult.FirstChild();

            for (NomadXML xmlMail = xmlResult.FirstChild(); xmlMail != null; xmlMail = xmlMail.Next())
            {
                try
                {
                    strContent = xmlMail.GetAttr("t_texto_mail");
                    sbResult = new System.Text.StringBuilder();

                    //Busca los sectores a reemplazar
                    while (strContent != "" || intControl < 50)
                    {
                        strBefore = SubStringBefore(strContent, tmpOpen);
                        strContent = SubStringAfter(strContent, tmpOpen);

                        sbResult.Append(strBefore);

                        if (strContent != "")
                        {
                            strToReplace = SubStringBefore(strContent, tmpClose);
                            strContent = SubStringAfter(strContent, tmpClose);

                            if (strToReplace != "")
                            {
                                //Se realiza el reemplazo del texto por lo indicado
                                switch (strToReplace.Trim().ToUpper())
                                {
                                    case "O.CODIGO":
                                        if (pxmlInfo != null) sbResult.Append(pxmlInfo.GetAttr("c_aviso"));
                                        break;

                                    case "O.DESCRIP":
                                        if (pxmlInfo != null) sbResult.Append(pxmlInfo.GetAttr("d_puesto"));
                                        break;

                                    case "CANTIDAD":
                                        if (pxmlInfo != null) sbResult.Append(pxmlInfo.GetAttr("cantidad"));
                                        break;

                                    case "CANTDIASPREVIOS":
                                        if (pxmlInfo != null) sbResult.Append(pxmlInfo.GetAttr("cantdiasprevio"));
                                        break;

                                    case "TEXTO":
                                        if (pxmlInfo != null)
                                        {
                                            string texto = "";
                                            int i = 0;
                                            int j = 0;
                                            do
                                            {
                                                NomadXML postu = ((NomadXML)listPostu[i]);
                                                if (postu.GetAttrInt("oi_aviso") == pxmlInfo.GetAttrInt("oi_aviso"))
                                                {
                                                    texto += " - [" + postu.GetAttr("c_cv") + "] " + postu.GetAttr("d_ape_y_nom");
                                                    if (postu.GetAttr("d_email")!="")
                                                        texto += " - " + postu.GetAttr("d_email");
                                                    texto += "\n";
                                                    ++j;
                                                }
                                                ++i;
                                            } while (cantMaxPostu > j && i < listPostu.Count);

                                            sbResult.Append(texto);
                                        }
                                        break;
                                }
                            }
                        }
                        intControl++;
                    }

                    if (xmlMail.GetAttr("c_texto_mail").ToString().Contains("ASUNTO"))
                        asuntoMail = sbResult.ToString();
                    else
                    {
                        textoMail = sbResult.ToString();
                        if (pxmlInfo.GetAttrInt("cantidad") == 1)
                            textoMail = textoMail.Replace("postulaciones", "postulación");
                    }
                }
                catch(Exception ex)
                {
                    if (xmlMail.GetAttr("c_texto_mail").ToString().Contains("ASUNTO"))
                        asuntoMail = "";
                    else
                        textoMail = "";

                    NomadLog.Warning("Error duante el armado del mail - c_texto_mail = " + xmlMail.GetAttr("c_texto_mail") + " - " + ex.Message);
                }
            }

            if(asuntoMail!="" && textoMail!="")
            {
                NomadLog.Info("Envio mail para aviso - c_aviso = " + pxmlInfo.GetAttr("c_aviso"));

                objMail = new Nomad.Base.Mail.OutputMails.MAIL();
                objMail.ASUNTO = asuntoMail;
                objMail.CONTENIDO = textoMail;
                objMail.DESDE_APLICACION = NomadProxy.GetProxy().AppName;
                objMail.FECHA_CREACION = DateTime.Now;

                //Agrega los destinatarios del mail
                if (pxmlInfo.GetAttr("d_mail") != "")
                {
                    NomadLog.Info("Enviando e-Mail a: " + pxmlInfo.GetAttr("d_mail"));

                    objDestino = new Nomad.Base.Mail.OutputMails.DESTINATARIO();
                    objDestino.MAIL_SUSTITUTO = pxmlInfo.GetAttr("d_mail");
                    objMail.DESTINATARIOS.Add(objDestino);
                }

                if (pxmlInfo.GetAttr("d_mail_alt_1") != "" && (pxmlInfo.GetAttr("d_mail_alt_1") != pxmlInfo.GetAttr("d_mail")))
                {
                    NomadLog.Info("Enviando e-Mail alternativo 1 a : " + pxmlInfo.GetAttr("d_mail_alt_1"));

                    objDestino = new Nomad.Base.Mail.OutputMails.DESTINATARIO();
                    objDestino.MAIL_SUSTITUTO = pxmlInfo.GetAttr("d_mail_alt_1");
                    objMail.DESTINATARIOS.Add(objDestino);
                }

                if (pxmlInfo.GetAttr("d_mail_alt_2") != "" && ((pxmlInfo.GetAttr("d_mail_alt_1") != pxmlInfo.GetAttr("d_mail")) && (pxmlInfo.GetAttr("d_mail_alt_1") != pxmlInfo.GetAttr("d_mail_alt_2"))))
                {
                    NomadLog.Info("Enviando e-Mail alternativo 2 a : " + pxmlInfo.GetAttr("d_mail_alt_2"));

                    objDestino = new Nomad.Base.Mail.OutputMails.DESTINATARIO();
                    objDestino.MAIL_SUSTITUTO = pxmlInfo.GetAttr("d_mail_alt_2");
                    objMail.DESTINATARIOS.Add(objDestino);
                }

                try
                {
                    NomadLog.Info("Envio mail");
                    NomadEnvironment.GetCurrentTransaction().Save(objMail);
                }
                catch (Exception ex)
                {
                    NomadLog.Error("Se ha producido un error al intentar enviar el mail");
                    NomadLog.Error("Error: " + ex.Message);
                }
            }

        }

    }
}



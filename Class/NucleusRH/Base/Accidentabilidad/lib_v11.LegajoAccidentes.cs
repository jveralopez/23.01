using System;
using System.Xml;
using System.IO;
using System.Collections;
using System.Collections.Generic;

using Nomad.NSystem.Document;
using Nomad.NSystem.Proxy;
using Nomad.NSystem.Base;
using Nomad.NSystem.Functions;

namespace NucleusRH.Base.Accidentabilidad.LegajoAccidentes
{

    //////////////////////////////////////////////////////////////////////////////////
    //Clase Datos de Accidentes por Personal
    public partial class PERSONAL_EMP
    {


    public static void RegistrarDenuncia(Nomad.NSystem.Proxy.NomadXML param, bool genlic, bool genlicART)
        {

            NomadBatch objBatch;
            objBatch = NomadBatch.GetBatch("Iniciando...", "Registro de Denuncia de Accidente");
            objBatch.SetPro(0);

            //CARGA EL LEGAJO
            NucleusRH.Base.Accidentabilidad.LegajoAccidentes.PERSONAL_EMP ddoPER;
            ddoPER = NucleusRH.Base.Accidentabilidad.LegajoAccidentes.PERSONAL_EMP.Get(param.FirstChild().GetAttr("oi_personal_emp"));

            string oiLicPER = "0";
            string oiLicPERART = "0";   //Licencia a cargo ART

            //Genera la licencia a cargo del empleador - si es menor a 10 dias, o si es solo generar la licencia a cargo del empleador - no hace nada con la de ART

            if (genlic || (genlicART && int.Parse(param.FirstChild().GetAttr("e_dias_perdidos")) <= 10))
            {
                objBatch.SetMess("Generando Licencia por Accidente a cargo del empleador...");

                if (param.FirstChild().GetAttr("f_fechahora_alta") != "" && param.FirstChild().GetAttr("f_fechahora_baja") != "")
                {
                    //Obtengo el Parametro de CODIGO de Accidente
                    string codLic = NomadEnvironment.QueryValue("ORG26_PARAMETROS", "d_valor", "c_parametro", "CODACCEMP", "ORG26_PARAMETROS.c_modulo=\\'ACC\\'", false);
                    if (codLic == "")
                        throw new NomadAppException("No esta definido el codigo de Licencia por Accidente a cargo del empleador.");

                    //Obtengo el Parametro de CODIGO de Accidente
                    string oiLic = NomadEnvironment.QueryValue("PER16_LICENCIAS", "oi_licencia", "c_licencia", codLic, "", false);
                    if (oiLic == "")
                        throw new NomadAppException("No esta definido la licencia asociado al codigo de Licencia por Accidente a cargo del empleador.");

                    //Nueva Licencia
                    NucleusRH.Base.Personal.LegajoEmpresa.LICENCIA_PER ddoLicPer = NucleusRH.Base.Personal.LegajoEmpresa.LICENCIA_PER.New();
                    ddoLicPer.oi_licencia = oiLic;
                    ddoLicPer.f_inicio = param.FirstChild().GetAttrDateTime("f_fechahora_baja").Date;
                    ddoLicPer.f_fin = param.FirstChild().GetAttrDateTime("f_fechahora_alta").Date.AddDays(-1);
                    ddoLicPer.e_cant_dias = ddoLicPer.f_fin.Subtract(ddoLicPer.f_inicio).Days + 1;
                    ddoLicPer.e_anio_corresp = ddoLicPer.f_inicio.Year;
                    ddoLicPer.l_bloqueada = false;
                    ddoLicPer.l_interfaz = false;
                    ddoLicPer.l_habiles = false;
                    //Asigno el valor de la observacion de la denuncia del accidente en la licencia que se genera
                    if (param.FirstChild().GetAttr("o_accidente") != "") ddoLicPer.o_licencia_per = param.FirstChild().GetAttr("o_accidente");

                    //Validar Licencia
                    try
                    {
                        NucleusRH.Base.Personal.LegajoEmpresa.LICENCIA_PER.ValidarAltaLicencia
                          (ddoPER.Id, oiLic, ddoLicPer.f_inicio, ddoLicPer.f_fin, ddoLicPer.e_cant_dias, ddoLicPer.e_anio_corresp);
                    }
                    catch (Exception e)
                    {
                        switch (e.Message)
                        {
                            case "LegajoEmpresa.LICENCIA_PER.ERR-FECHA":
                                objBatch.Err("La fecha de fin debe ser mayor o igual a la fecha de inicio");
                                return;
                            case "LegajoEmpresa.LICENCIA_PER.ERR-SOLAP":
                                objBatch.Err("Existe un solapamiento de fechas con otras Licencias cargadas para el Legajo");
                                return;
                            case "Personal.NOVEDAD.ERR-LICENCIA":
                                objBatch.Err("Existe un solapamiento de fechas con Novedades cargadas para el Legajo");
                                return;
                            case "LegajoEmpresa.LICENCIA_PER.ERR-MIN-ANTIG":
                                objBatch.Err("El legajo tiene una antigüedad inferior al mínimo requerido");
                                return;
                            case "LegajoEmpresa.LICENCIA_PER.ERR-MAX-DIAS":
                                objBatch.Err("La cantidad de días consecutivos supera el máximo");
                                return;
                            case "LegajoEmpresa.LICENCIA_PER.ERR-MAX-ANIO":
                                objBatch.Err("La cantidad de días supera el saldo anual restante");
                                return;
                            default:
                                objBatch.Err("Error desconocido");
                                return;
                        }
                    }

                    //Agregar Licencia
                    ddoPER.LICEN_PER.Add(ddoLicPer);

                    //Guardar Legajo
                    NomadEnvironment.GetCurrentTransaction().SaveRefresh(ddoPER);

                    //Consulto el id de la licencia cargada en el legajo
                    NomadXML xmlParam = new NomadXML("PARAM");
                    xmlParam.SetAttr("oi_personal_emp", ddoPER.Id);
                    xmlParam.SetAttr("f_inicio", ddoLicPer.f_inicio);
                    NomadXML xmlqry = NomadEnvironment.QueryNomadXML(PERSONAL_EMP.Resources.QRY_LICEN_PER, xmlParam.ToString());
                    oiLicPER = xmlqry.FirstChild().GetAttr("oi_licencia_per");
                    NomadEnvironment.GetTrace().Info("oiLicPER -- " + oiLicPER);
                }
            }
            //Si selecciono licencia a cargo de empleador y ART - y la cantiad es mayor a 10 - debo crear dos licencias, junto con la de ART
            else if (genlicART && (int.Parse(param.FirstChild().GetAttr("e_dias_perdidos")) > 10))
            {
                objBatch.SetMess("Generando Licencia por Accidente Empleador y ART...");

                if (param.FirstChild().GetAttr("f_fechahora_alta") != "" && param.FirstChild().GetAttr("f_fechahora_baja") != "")
                {
                    //Obtengo el Parametro de CODIGO de Accidente a cargo del empleador
                    string codLic = NomadEnvironment.QueryValue("ORG26_PARAMETROS", "d_valor", "c_parametro", "CODACCEMP", "ORG26_PARAMETROS.c_modulo=\\'ACC\\'", false);
                    if (codLic == "")
                        throw new NomadAppException("No esta definido el codigo de Licencia por Accidente a Cargo del empleador.");

                    //Obtengo el oi del CODIGO de Accidente a cargo del empleador
                    string oiLic = NomadEnvironment.QueryValue("PER16_LICENCIAS", "oi_licencia", "c_licencia", codLic, "", false);
                    if (oiLic == "")
                        throw new NomadAppException("No existe licencia asociado al codigo de Licencia por Accidente a cargo del empleador.");

                    //Obtengo el Parametro de CODIGO de Accidente a cargo del la ART
                    string codLicART = NomadEnvironment.QueryValue("ORG26_PARAMETROS", "d_valor", "c_parametro", "CODACCART", "ORG26_PARAMETROS.c_modulo=\\'ACC\\'", false);
                    if (codLicART == "")
                        throw new NomadAppException("No esta definido el codigo de Licencia por Accidente a cargo de la ART.");

                    //Obtengo el oi del CODIGO de Accidente a caro de la ART
                    string oiLicART = NomadEnvironment.QueryValue("PER16_LICENCIAS", "oi_licencia", "c_licencia", codLicART, "", false);
                    if (oiLicART == "")
                        throw new NomadAppException("No existe licencia asociada al codigo de Licencia por Accidente a cargo de la ART.");

                    //Nueva Licencia a cargo del Empleador - los primeros 10 días
                    NucleusRH.Base.Personal.LegajoEmpresa.LICENCIA_PER ddoLicPer = NucleusRH.Base.Personal.LegajoEmpresa.LICENCIA_PER.New();
                    ddoLicPer.oi_licencia = oiLic;
                    ddoLicPer.f_inicio = param.FirstChild().GetAttrDateTime("f_fechahora_baja").Date;
                    ddoLicPer.f_fin = param.FirstChild().GetAttrDateTime("f_fechahora_baja").Date.AddDays(9);
                    ddoLicPer.e_cant_dias = ddoLicPer.f_fin.Subtract(ddoLicPer.f_inicio).Days + 1;
                    ddoLicPer.e_anio_corresp = ddoLicPer.f_inicio.Year;
                    ddoLicPer.l_bloqueada = false;
                    ddoLicPer.l_interfaz = false;
                    ddoLicPer.l_habiles = false;
                    //Asigno el valor de la observacion de la denuncia del accidente en la licencia que se genera
                    if (param.FirstChild().GetAttr("o_accidente") != "") ddoLicPer.o_licencia_per = param.FirstChild().GetAttr("o_accidente");

                    //Validar Licencia
                    try
                    {
                        NucleusRH.Base.Personal.LegajoEmpresa.LICENCIA_PER.ValidarAltaLicencia
                          (ddoPER.Id, oiLic, ddoLicPer.f_inicio, ddoLicPer.f_fin, ddoLicPer.e_cant_dias, ddoLicPer.e_anio_corresp);
                    }
                    catch (Exception e)
                    {
                        switch (e.Message)
                        {
                            case "LegajoEmpresa.LICENCIA_PER.ERR-FECHA":
                                objBatch.Err("La fecha de fin debe ser mayor o igual a la fecha de inicio");
                                return;
                            case "LegajoEmpresa.LICENCIA_PER.ERR-SOLAP":
                                objBatch.Err("Existe un solapamiento de fechas con otras Licencias cargadas para el Legajo");
                                return;
                            case "Personal.NOVEDAD.ERR-LICENCIA":
                                objBatch.Err("Existe un solapamiento de fechas con Novedades cargadas para el Legajo");
                                return;
                            case "LegajoEmpresa.LICENCIA_PER.ERR-MIN-ANTIG":
                                objBatch.Err("El legajo tiene una antigüedad inferior al mínimo requerido");
                                return;
                            case "LegajoEmpresa.LICENCIA_PER.ERR-MAX-DIAS":
                                objBatch.Err("La cantidad de días consecutivos supera el máximo");
                                return;
                            case "LegajoEmpresa.LICENCIA_PER.ERR-MAX-ANIO":
                                objBatch.Err("La cantidad de días supera el saldo anual restante");
                                return;
                            default:
                                objBatch.Err("Error desconocido");
                                return;
                        }
                    }

                    //Nueva Licencia a cargo de la ART - Resto de los dias solicitados
                    NucleusRH.Base.Personal.LegajoEmpresa.LICENCIA_PER ddoLicPerART = NucleusRH.Base.Personal.LegajoEmpresa.LICENCIA_PER.New();
                    ddoLicPerART.oi_licencia = oiLicART;
                    ddoLicPerART.f_inicio = param.FirstChild().GetAttrDateTime("f_fechahora_baja").Date.AddDays(10); //del dia 11 en adelante
                    ddoLicPerART.f_fin = param.FirstChild().GetAttrDateTime("f_fechahora_alta").Date.AddDays(-1);
                    ddoLicPerART.e_cant_dias = ddoLicPerART.f_fin.Subtract(ddoLicPerART.f_inicio).Days + 1;
                    ddoLicPerART.e_anio_corresp = ddoLicPerART.f_inicio.Year;
                    ddoLicPerART.l_bloqueada = false;
                    ddoLicPerART.l_interfaz = false;
                    ddoLicPerART.l_habiles = false;
                    //Asigno el valor de la observacion de la denuncia del accidente en la licencia que se genera
                    if (param.FirstChild().GetAttr("o_accidente") != "") ddoLicPerART.o_licencia_per = param.FirstChild().GetAttr("o_accidente");

                    //Validar Licencia
                    try
                    {
                        NucleusRH.Base.Personal.LegajoEmpresa.LICENCIA_PER.ValidarAltaLicencia
                          (ddoPER.Id, oiLicART, ddoLicPerART.f_inicio, ddoLicPerART.f_fin, ddoLicPerART.e_cant_dias, ddoLicPerART.e_anio_corresp);
                    }
                    catch (Exception e)
                    {
                        switch (e.Message)
                        {
                            case "LegajoEmpresa.LICENCIA_PER.ERR-FECHA":
                                objBatch.Err("La fecha de fin debe ser mayor o igual a la fecha de inicio");
                                return;
                            case "LegajoEmpresa.LICENCIA_PER.ERR-SOLAP":
                                objBatch.Err("Existe un solapamiento de fechas con otras Licencias cargadas para el Legajo");
                                return;
                            case "Personal.NOVEDAD.ERR-LICENCIA":
                                objBatch.Err("Existe un solapamiento de fechas con Novedades cargadas para el Legajo");
                                return;
                            case "LegajoEmpresa.LICENCIA_PER.ERR-MIN-ANTIG":
                                objBatch.Err("El legajo tiene una antigüedad inferior al mínimo requerido");
                                return;
                            case "LegajoEmpresa.LICENCIA_PER.ERR-MAX-DIAS":
                                objBatch.Err("La cantidad de días consecutivos supera el máximo");
                                return;
                            case "LegajoEmpresa.LICENCIA_PER.ERR-MAX-ANIO":
                                objBatch.Err("La cantidad de días supera el saldo anual restante");
                                return;
                            default:
                                objBatch.Err("Error desconocido");
                                return;
                        }
                    }

                    //Una vez validadas y creadas ambas licencias las guardo en el legajo

                    //Agregar Licencia a cargo del Empleador al legajo
                    ddoPER.LICEN_PER.Add(ddoLicPer);

                    //Agregar Licencia a cargo de la ART al legajo
                    ddoPER.LICEN_PER.Add(ddoLicPerART);

                    //Guardar Legajo
                    NomadEnvironment.GetCurrentTransaction().SaveRefresh(ddoPER);

                    //Consulto los ids de las licencias creadas
                    NomadXML xmlParam = new NomadXML("PARAM");
                    xmlParam.SetAttr("oi_personal_emp", ddoPER.Id);
                    xmlParam.SetAttr("f_inicio", ddoLicPer.f_inicio);
                    xmlParam.SetAttr("f_inicio2", ddoLicPerART.f_inicio);
                    NomadXML xmlqry = NomadEnvironment.QueryNomadXML(PERSONAL_EMP.Resources.QRY_LICEN_PER, xmlParam.ToString());
                    oiLicPER = xmlqry.FirstChild().GetAttr("oi_licencia_per");
                    oiLicPERART = xmlqry.FirstChild().GetAttr("oi_lic_per_art");
                    NomadEnvironment.GetTrace().Info("oiLicPER -- " + oiLicPER + "oiLicPERART -- " + oiLicPERART);
                }
            }

            objBatch.SetPro(40);
            objBatch.SetMess("Actualizando legajo...");

            NucleusRH.Base.Accidentabilidad.LegajoAccidentes.ACCIDENTE ddoACC;

            ddoACC = new NucleusRH.Base.Accidentabilidad.LegajoAccidentes.ACCIDENTE();

            ddoACC.f_fechahora_acc = param.FirstChild().GetAttrDateTime("f_fechahora_acc");

            if (param.FirstChild().GetAttr("oi_tipo_acc") != "")
                ddoACC.oi_tipo_acc = param.FirstChild().GetAttr("oi_tipo_acc");

            if (param.FirstChild().GetAttr("oi_acto_inseg") != "")
                ddoACC.oi_acto_inseg = param.FirstChild().GetAttr("oi_acto_inseg");

            if (param.FirstChild().GetAttr("oi_tipo_siniestro") != "")
                ddoACC.oi_tipo_siniestro = param.FirstChild().GetAttr("oi_tipo_siniestro");

            if (param.FirstChild().GetAttr("oi_agente_caus") != "")
                ddoACC.oi_agente_caus = param.FirstChild().GetAttr("oi_agente_caus");

            if (param.FirstChild().GetAttr("oi_condic_pelig") != "")
                ddoACC.oi_condic_pelig = param.FirstChild().GetAttr("oi_condic_pelig");

            if (param.FirstChild().GetAttr("oi_factor_cont") != "")
                ddoACC.oi_factor_cont = param.FirstChild().GetAttr("oi_factor_cont");

            if (param.FirstChild().GetAttr("oi_reg_cuerpo") != "")
                ddoACC.oi_reg_cuerpo = param.FirstChild().GetAttr("oi_reg_cuerpo");

            if (param.FirstChild().GetAttr("oi_natur_lesion") != "")
                ddoACC.oi_natur_lesion = param.FirstChild().GetAttr("oi_natur_lesion");

            if (param.FirstChild().GetAttr("oi_incapacidad") != "")
                ddoACC.oi_incapacidad = param.FirstChild().GetAttr("oi_incapacidad");

            if (param.FirstChild().GetAttr("oi_elemento_pp") != "")
                ddoACC.oi_elemento_pp = param.FirstChild().GetAttr("oi_elemento_pp");

            if (param.FirstChild().GetAttr("oi_ocupacion") != "")
                ddoACC.oi_ocupacion = param.FirstChild().GetAttr("oi_ocupacion");

            if (param.FirstChild().GetAttr("f_fechahora_revis") != "")
                ddoACC.f_fechahora_revis = param.FirstChild().GetAttrDateTime("f_fechahora_revis");

            if (param.FirstChild().GetAttr("e_dias_perdidos") != "")
                ddoACC.e_dias_perdidos = param.FirstChild().GetAttrInt("e_dias_perdidos");

            if (param.FirstChild().GetAttr("f_fechahora_baja") != "")
                ddoACC.f_fechahora_baja = param.FirstChild().GetAttrDateTime("f_fechahora_baja");

            if (param.FirstChild().GetAttr("f_fechahora_alta") != "")
                ddoACC.f_fechahora_alta = param.FirstChild().GetAttrDateTime("f_fechahora_alta");

            //Campos Agregados en sol 2011 1545
            if (param.FirstChild().GetAttr("e_valor_grado") != "")
                ddoACC.e_valor_grado = param.FirstChild().GetAttrInt("e_valor_grado");
            if (param.FirstChild().GetAttr("n_porc_grado") != "")
                ddoACC.n_porc_grado = param.FirstChild().GetAttrDouble("n_porc_grado");
            if (param.FirstChild().GetAttr("l_denunciado") != "")
                ddoACC.l_denunciado = param.FirstChild().GetAttrBool("l_denunciado");
            //
            if (param.FirstChild().GetAttr("o_accion_aconsej") != "")
                ddoACC.o_accion_aconsej = param.FirstChild().GetAttr("o_accion_aconsej");

            if (param.FirstChild().GetAttr("o_accidente") != "")
                ddoACC.o_accidente = param.FirstChild().GetAttr("o_accidente");

            if (param.FirstChild().GetAttr("e_testigo_1") != "")
            {
                //TESTIGO_1
                NucleusRH.Base.Accidentabilidad.LegajoAccidentes.TESTIGO ddoTES_1 = new NucleusRH.Base.Accidentabilidad.LegajoAccidentes.TESTIGO();
                ddoTES_1.e_testigo = param.FirstChild().GetAttrInt("e_testigo_1");
                ddoTES_1.d_apellido = param.FirstChild().GetAttr("d_apellido_1");
                ddoTES_1.d_nombres = param.FirstChild().GetAttr("d_nombres_1");

                if (param.FirstChild().GetAttr("oi_tipo_documento_1") != "")
                    ddoTES_1.oi_tipo_documento = param.FirstChild().GetAttr("oi_tipo_documento_1");

                if (param.FirstChild().GetAttr("c_nro_documento_1") != "")
                    ddoTES_1.c_nro_documento = param.FirstChild().GetAttr("c_nro_documento_1");

                if (param.FirstChild().GetAttr("d_domicilio_1") != "")
                    ddoTES_1.d_domicilio = param.FirstChild().GetAttr("d_domicilio_1");

                if (param.FirstChild().GetAttr("o_testigo_1") != "")
                    ddoTES_1.o_testigo = param.FirstChild().GetAttr("o_testigo_1");

                //Agrego el testigo
                ddoACC.TESTIGOS.Add(ddoTES_1);
            }

            if (param.FirstChild().GetAttr("e_testigo_2") != "")
            {
                //TESTIGO_2
                NucleusRH.Base.Accidentabilidad.LegajoAccidentes.TESTIGO ddoTES_2 = new NucleusRH.Base.Accidentabilidad.LegajoAccidentes.TESTIGO();
                ddoTES_2.e_testigo = param.FirstChild().GetAttrInt("e_testigo_2");
                ddoTES_2.d_apellido = param.FirstChild().GetAttr("d_apellido_2");
                ddoTES_2.d_nombres = param.FirstChild().GetAttr("d_nombres_2");

                if (param.FirstChild().GetAttr("oi_tipo_documento_2") != "")
                    ddoTES_2.oi_tipo_documento = param.FirstChild().GetAttr("oi_tipo_documento_2");

                if (param.FirstChild().GetAttr("c_nro_documento_2") != "")
                    ddoTES_2.c_nro_documento = param.FirstChild().GetAttr("c_nro_documento_2");

                if (param.FirstChild().GetAttr("d_domicilio_2") != "")
                    ddoTES_2.d_domicilio = param.FirstChild().GetAttr("d_domicilio_2");

                if (param.FirstChild().GetAttr("o_testigo_2") != "")
                    ddoTES_2.o_testigo = param.FirstChild().GetAttr("o_testigo_2");

                //Agrego el testigo
                ddoACC.TESTIGOS.Add(ddoTES_2);
            }

            if (param.FirstChild().GetAttr("e_testigo_3") != "")
            {
                //TESTIGO_3
                NucleusRH.Base.Accidentabilidad.LegajoAccidentes.TESTIGO ddoTES_3 = new NucleusRH.Base.Accidentabilidad.LegajoAccidentes.TESTIGO();
                ddoTES_3.e_testigo = param.FirstChild().GetAttrInt("e_testigo_3");
                ddoTES_3.d_apellido = param.FirstChild().GetAttr("d_apellido_3");
                ddoTES_3.d_nombres = param.FirstChild().GetAttr("d_nombres_3");

                if (param.FirstChild().GetAttr("oi_tipo_documento_3") != "")
                    ddoTES_3.oi_tipo_documento = param.FirstChild().GetAttr("oi_tipo_documento_3");

                if (param.FirstChild().GetAttr("c_nro_documento_3") != "")
                    ddoTES_3.c_nro_documento = param.FirstChild().GetAttr("c_nro_documento_3");

                if (param.FirstChild().GetAttr("d_domicilio_3") != "")
                    ddoTES_3.d_domicilio = param.FirstChild().GetAttr("d_domicilio_3");

                if (param.FirstChild().GetAttr("o_testigo_3") != "")
                    ddoTES_3.o_testigo = param.FirstChild().GetAttr("o_testigo_3");

                //Agrego el testigo
                ddoACC.TESTIGOS.Add(ddoTES_3);
            }

            if (param.FirstChild().GetAttr("e_testigo_4") != "")
            {
                //TESTIGO_4
                NucleusRH.Base.Accidentabilidad.LegajoAccidentes.TESTIGO ddoTES_4 = new NucleusRH.Base.Accidentabilidad.LegajoAccidentes.TESTIGO();
                ddoTES_4.e_testigo = param.FirstChild().GetAttrInt("e_testigo_4");
                ddoTES_4.d_apellido = param.FirstChild().GetAttr("d_apellido_4");
                ddoTES_4.d_nombres = param.FirstChild().GetAttr("d_nombres_4");

                if (param.FirstChild().GetAttr("oi_tipo_documento_4") != "")
                    ddoTES_4.oi_tipo_documento = param.FirstChild().GetAttr("oi_tipo_documento_4");

                if (param.FirstChild().GetAttr("c_nro_documento_4") != "")
                    ddoTES_4.c_nro_documento = param.FirstChild().GetAttr("c_nro_documento_4");

                if (param.FirstChild().GetAttr("d_domicilio_4") != "")
                    ddoTES_4.d_domicilio = param.FirstChild().GetAttr("d_domicilio_4");

                if (param.FirstChild().GetAttr("o_testigo_4") != "")
                    ddoTES_4.o_testigo = param.FirstChild().GetAttr("o_testigo_4");

                //Agrego el testigo
                ddoACC.TESTIGOS.Add(ddoTES_4);
            }

            objBatch.SetPro(60);

            //Registro la licencia a cargo del empleador en la denuncia del accidente del legajo
            if (oiLicPER != "0") ddoACC.oi_licencia = int.Parse(oiLicPER);

            //Registro la licencia a cargo de ART en la denuncia del accidente del legajo
            if (oiLicPERART != "0") ddoACC.oi_licencia_art = int.Parse(oiLicPERART);

            if (genlicART) ddoACC.l_art = true;

            ddoPER.ACCIDENTES.Add(ddoACC);

            NomadEnvironment.GetCurrentTransaction().SaveRefresh(ddoPER);
            objBatch.SetPro(85);

            //GENERA EL COMPROBANTE
            objBatch.SetMess("Ejecutando el Reporte...");

            //Armo el doc de parametros
            NomadXML xmlparam = new NomadXML("DATOS");

            foreach (NucleusRH.Base.Accidentabilidad.LegajoAccidentes.ACCIDENTE ddoA in ddoPER.ACCIDENTES)
            {
                if (ddoA.f_fechahora_acc == param.FirstChild().GetAttrDateTime("f_fechahora_acc"))
                {
                    xmlparam.SetAttr("oi_accidente", ddoA.Id);
                    continue;
                }
            }

            NomadEnvironment.GetTrace().Info("xmlparam -- " + xmlparam.ToString());
 
            objBatch.SetPro(98);

            string outFileName = NomadProxy.GetProxy().Batch().ID + ".htm";
            string outFilePath = NomadProxy.GetProxy().RunPath + "Nomad\\TEMP\\";
            NomadBatch.Trace("Generando Reporte HTML en path:'" + outFilePath + outFileName + "'");
	
			Nomad.NomadHTML nmdHtml = new Nomad.NomadHTML("NucleusRH.Base.Accidentabilidad.DenunciaAccidente.rpt", xmlparam);
			nmdHtml.GenerateHTML(outFilePath + "\\" + outFileName, System.Text.Encoding.UTF8);
        }

    //Actualizo la observacion de la licencia con la observacion de la denuncia del accidente
    //Actualizo las licencias correspondientes a cada accidente
    public void ActualizarAccidente(Nomad.NSystem.Proxy.NomadXML LICDELS)
        {
            //Guarda fecha de la nueva licencia creada para un accidente - para asignarlo al final
            Hashtable NewLicART = new Hashtable();

            //Guarda las licenicas a excluir par las modificaciones
            List<string> LicModificar = new List<string>();

            //elimino las licencias de los accidentes eliminados
            for (NomadXML lic = LICDELS.FirstChild().FirstChild(); lic != null; lic = lic.Next())
            {
                NucleusRH.Base.Personal.LegajoEmpresa.LICENCIA_PER LIC = (NucleusRH.Base.Personal.LegajoEmpresa.LICENCIA_PER)this.LICEN_PER.GetById(lic.GetAttr("id"));
                if (LIC != null)
                    this.LICEN_PER.Remove(LIC);
            }

            //Modifico las licencias de los accidentes modificados
            foreach (ACCIDENTE ACC in this.ACCIDENTES)
            {
                if (!ACC.IsForUpdate)
                    continue;

                if (!ACC.l_art) //Es solo de empleador, no hay restricciones con la ART
                {
                    if (ACC.oi_licenciaNull)
                        continue;

                    NucleusRH.Base.Personal.LegajoEmpresa.LICENCIA_PER LIC = (NucleusRH.Base.Personal.LegajoEmpresa.LICENCIA_PER)this.LICEN_PER.GetById(ACC.oi_licencia.ToString());
                    if (LIC != null)
                    {
                        LIC.f_inicio = ACC.f_fechahora_baja;
                        LIC.f_fin = ACC.f_fechahora_alta.AddDays(-1);
                        LIC.e_cant_dias = ACC.e_dias_perdidos;
                        LIC.o_licencia_per = ACC.o_accidente;

                        LicModificar.Clear();
                        LicModificar.Add(LIC.id.ToString());

                        //Validar modificacion de la licencia - excluyendose asi misma
                        NucleusRH.Base.Personal.LegajoEmpresa.LICENCIA_PER.ValidarModLicencia
                          (this, LIC.oi_licencia.ToString(), LIC.f_inicio, LIC.f_fin, LIC.e_cant_dias, LIC.e_anio_corresp, LicModificar);
                    }
                }
                //Si tiene el check l_art - se selecciono generar licencia al empleador y ART - al menos tiene el del empleador
                else
                {
                    //Si tiene ambas licencias - y tiene mayor a 10 dias - modifico ambas (por si cambia la fecha de inicio)
                    //si tiene ambas licencias - y tiene menor a 10 dias - modifico la de empleado - y la de art elimino - quito del accidente
                    if(!ACC.oi_licenciaNull && !ACC.oi_licencia_artNull)
                    {
                        if(ACC.e_dias_perdidos > 10)
                        {
                            //Modifico la licencia del empleador
                            NucleusRH.Base.Personal.LegajoEmpresa.LICENCIA_PER LIC = (NucleusRH.Base.Personal.LegajoEmpresa.LICENCIA_PER)this.LICEN_PER.GetById(ACC.oi_licencia.ToString());
                            if (LIC != null)
                            {
                                LIC.f_inicio = ACC.f_fechahora_baja;
                                LIC.f_fin = ACC.f_fechahora_baja.AddDays(9);
                                LIC.e_cant_dias = LIC.f_fin.Subtract(LIC.f_inicio).Days + 1;
                                LIC.o_licencia_per = ACC.o_accidente;
                            }

                            //Modifico la licencia de ART
                            NucleusRH.Base.Personal.LegajoEmpresa.LICENCIA_PER LIC_ART = (NucleusRH.Base.Personal.LegajoEmpresa.LICENCIA_PER)this.LICEN_PER.GetById(ACC.oi_licencia_art.ToString());
                            if (LIC_ART != null)
                            {
                                LIC_ART.f_inicio = ACC.f_fechahora_baja.AddDays(10);
                                LIC_ART.f_fin = ACC.f_fechahora_alta.AddDays(-1);
                                LIC_ART.e_cant_dias = LIC.f_fin.Subtract(LIC.f_inicio).Days + 1;
                                LIC_ART.o_licencia_per = ACC.o_accidente;
                            }

                            //Validar licencias modificadas
                            LicModificar.Clear();
                            LicModificar.Add(LIC.id.ToString());
                            LicModificar.Add(LIC_ART.id.ToString());

                            //Valido la de empleador
                            NucleusRH.Base.Personal.LegajoEmpresa.LICENCIA_PER.ValidarModLicencia
                              (this, LIC.oi_licencia.ToString(), LIC.f_inicio, LIC.f_fin, LIC.e_cant_dias, LIC.e_anio_corresp, LicModificar);

                            //Valido la de ART
                            NucleusRH.Base.Personal.LegajoEmpresa.LICENCIA_PER.ValidarModLicencia
                              (this, LIC_ART.oi_licencia.ToString(), LIC_ART.f_inicio, LIC_ART.f_fin, LIC_ART.e_cant_dias, LIC_ART.e_anio_corresp, LicModificar);
                        }
                        else
                        {
                            //Modifico la licencia del empleador
                            NucleusRH.Base.Personal.LegajoEmpresa.LICENCIA_PER LIC = (NucleusRH.Base.Personal.LegajoEmpresa.LICENCIA_PER)this.LICEN_PER.GetById(ACC.oi_licencia.ToString());
                            if (LIC != null)
                            {
                                LIC.f_inicio = ACC.f_fechahora_baja;
                                LIC.f_fin = ACC.f_fechahora_alta.AddDays(-1);
                                LIC.e_cant_dias = ACC.e_dias_perdidos;
                                LIC.o_licencia_per = ACC.o_accidente;
                            }

                            LicModificar.Clear();
                            LicModificar.Add(LIC.id.ToString());

                            //Elimino la de ART
                            NucleusRH.Base.Personal.LegajoEmpresa.LICENCIA_PER LIC_ART = (NucleusRH.Base.Personal.LegajoEmpresa.LICENCIA_PER)this.LICEN_PER.GetById(ACC.oi_licencia_art.ToString());
                            LicModificar.Add(LIC_ART.id.ToString()); //Agrego para excluir de la validacion
                            this.LICEN_PER.Remove(LIC_ART);
                            ACC.oi_licencia_artNull = true; //quito del accidente la licencia de art eliminada

                            //Valido la de empleador
                            NucleusRH.Base.Personal.LegajoEmpresa.LICENCIA_PER.ValidarModLicencia
                              (this, LIC.oi_licencia.ToString(), LIC.f_inicio, LIC.f_fin, LIC.e_cant_dias, LIC.e_anio_corresp, LicModificar);

                        }
                    }
                    //si tiene una sola licencia - y tiene menos de 10 dias - modifico la licencia
                    //si tiene una sola licencia - y tiene ne mas de 10 dias - agregar la nueva licencia de ART
                    else if (!ACC.oi_licenciaNull)
                    {
                        if (ACC.e_dias_perdidos <= 10)
                        {
                            //Modifico la licencia del empleador
                            NucleusRH.Base.Personal.LegajoEmpresa.LICENCIA_PER LIC = (NucleusRH.Base.Personal.LegajoEmpresa.LICENCIA_PER)this.LICEN_PER.GetById(ACC.oi_licencia.ToString());
                            if (LIC != null)
                            {
                                LIC.f_inicio = ACC.f_fechahora_baja;
                                LIC.f_fin = ACC.f_fechahora_alta.AddDays(-1);
                                LIC.e_cant_dias = ACC.e_dias_perdidos;
                                LIC.o_licencia_per = ACC.o_accidente;
                            }

                            LicModificar.Clear();
                            LicModificar.Add(LIC.id.ToString());

                            //Validar modificacion de la licencia - excluyendose asi misma
                            NucleusRH.Base.Personal.LegajoEmpresa.LICENCIA_PER.ValidarModLicencia
                              (this, LIC.oi_licencia.ToString(), LIC.f_inicio, LIC.f_fin, LIC.e_cant_dias, LIC.e_anio_corresp, LicModificar);
                        }
                        else
                        {
                            //Modifico la licencia del empleador
                            NucleusRH.Base.Personal.LegajoEmpresa.LICENCIA_PER LIC = (NucleusRH.Base.Personal.LegajoEmpresa.LICENCIA_PER)this.LICEN_PER.GetById(ACC.oi_licencia.ToString());
                            if (LIC != null)
                            {
                                LIC.f_inicio = ACC.f_fechahora_baja;
                                LIC.f_fin = ACC.f_fechahora_baja.AddDays(9);
                                LIC.e_cant_dias = LIC.f_fin.Subtract(LIC.f_inicio).Days + 1;
                                LIC.o_licencia_per = ACC.o_accidente;
                            }

                            //Agrego la de ART

                            //Obtengo el Parametro de CODIGO de Accidente a cargo del la ART
                            string codLicART = NomadEnvironment.QueryValue("ORG26_PARAMETROS", "d_valor", "c_parametro", "CODACCART", "ORG26_PARAMETROS.c_modulo=\\'ACC\\'", false);
                            if (codLicART == "")
                                throw new NomadAppException("No esta definido el codigo de Licencia por Accidente a cargo de la ART.");

                            //Obtengo el oi del CODIGO de Accidente a caro de la ART
                            string oiLicART = NomadEnvironment.QueryValue("PER16_LICENCIAS", "oi_licencia", "c_licencia", codLicART, "", false);
                            if (oiLicART == "")
                                throw new NomadAppException("No existe licencia asociada al codigo de Licencia por Accidente a cargo de la ART.");

                            //Nueva Licencia a cargo de la ART - Resto de los dias solicitados
                            NucleusRH.Base.Personal.LegajoEmpresa.LICENCIA_PER ddoLicPerART = NucleusRH.Base.Personal.LegajoEmpresa.LICENCIA_PER.New();
                            ddoLicPerART.oi_licencia = oiLicART;
                            ddoLicPerART.f_inicio = ACC.f_fechahora_baja.AddDays(10); //del dia 11 en adelante
                            ddoLicPerART.f_fin = ACC.f_fechahora_alta.AddDays(-1);
                            ddoLicPerART.e_cant_dias = ddoLicPerART.f_fin.Subtract(ddoLicPerART.f_inicio).Days + 1;
                            ddoLicPerART.e_anio_corresp = ddoLicPerART.f_inicio.Year;
                            ddoLicPerART.l_bloqueada = false;
                            ddoLicPerART.l_interfaz = false;
                            ddoLicPerART.l_habiles = false;
                            ddoLicPerART.o_licencia_per = ACC.o_accidente;

                            LicModificar.Clear();
                            LicModificar.Add(LIC.id.ToString());

                            //Valido la de empleador
                            NucleusRH.Base.Personal.LegajoEmpresa.LICENCIA_PER.ValidarModLicencia
                              (this, LIC.oi_licencia.ToString(), LIC.f_inicio, LIC.f_fin, LIC.e_cant_dias, LIC.e_anio_corresp, LicModificar);

                            //Valido la de ART
                            NucleusRH.Base.Personal.LegajoEmpresa.LICENCIA_PER.ValidarModLicencia
                              (this, ddoLicPerART.oi_licencia.ToString(), ddoLicPerART.f_inicio, ddoLicPerART.f_fin, ddoLicPerART.e_cant_dias, ddoLicPerART.e_anio_corresp, LicModificar);

                            //Agregar Licencia a cargo de la ART al legajo
                            this.LICEN_PER.Add(ddoLicPerART);

                            NewLicART.Add(ACC.id,ddoLicPerART.f_inicio); //Guardo el accidente - fecha de inicio de la licencia de ART a crear - para luego de guarda el DDO asignar al accidente
                        }
                    }
                }
            }

            //Guardo el DDO completo con todas las modificaciones - tanto del accidente como de las licencias
            NomadEnvironment.GetCurrentTransaction().SaveRefresh(this);

            foreach (ACCIDENTE ACC in this.ACCIDENTES)
            {
                if (NewLicART.ContainsKey(ACC.id)) //Si el accidente esta en la hashtable
                {
                    DateTime f_inicio = (DateTime)NewLicART[ACC.id];
                    //Consulto los ids de las licencias creadas
                    NomadXML xmlParam = new NomadXML("PARAM");
                    xmlParam.SetAttr("oi_personal_emp", this.id.ToString());
                    xmlParam.SetAttr("f_inicio2", f_inicio);
                    NomadXML xmlqry = NomadEnvironment.QueryNomadXML(PERSONAL_EMP.Resources.QRY_LICEN_PER, xmlParam.ToString());
                    string oi_licencia_art = xmlqry.FirstChild().GetAttr("oi_lic_per_art");

                    //Asgino al accidente la licencia de ART creada
                    ACC.oi_licencia_art = Int32.Parse(oi_licencia_art);
                    NomadEnvironment.GetCurrentTransaction().SaveRefresh(this);
                }
            }
        }

        public static NomadXML ValidarLicencia(int oi_personal_emp, int oi_licencia, DateTime f_inicio, DateTime f_fin, int dias)
        {
            NucleusRH.Base.Personal.LegajoEmpresa.LICENCIA_PER licencia = NucleusRH.Base.Personal.LegajoEmpresa.LICENCIA_PER.Get(oi_licencia,false);
            Hashtable hashIgnorar = new Hashtable();
            hashIgnorar.Add(oi_licencia,oi_licencia);
            return NucleusRH.Base.Personal.LegajoEmpresa.PERSONAL_EMP.ValidarLicencia(oi_personal_emp,int.Parse(licencia.oi_licencia), f_inicio, f_fin.AddDays(-1), dias, licencia.e_anio_corresp,hashIgnorar);
        }

    }
}



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

namespace NucleusRH.Base.Control_Contratistas.InterfaceImportacionPerCCO
{

  //////////////////////////////////////////////////////////////////////////////////
  //Clase Definicion de interfaz de entrada

  public partial class ENTRADA : Nomad.NSystem.Base.NomadObject
  {

    public static void ImportarPersonalContratista(string poi_empresa)
    {

             int Linea=0, Errores=0;

             NomadBatch objBatch;
             objBatch = NomadBatch.GetBatch("Iniciando...", "Importación masiva de Personal Contratista.");

             NomadXML IDList = new NomadXML();
             NucleusRH.Base.Control_Contratistas.InterfaceImportacionPerCCO.ENTRADA objRead;

             //obtengo datos del archivo
             IDList.SetText(NomadProxy.GetProxy().SQLService().Get(Resources.QRY_REGISTROS,""));

             ArrayList lista = (ArrayList)IDList.FirstChild().GetElements("ROW");

             //para cada linea del archivo
             for (int xml =0; xml<lista.Count; xml++)
             {
                Linea++;
                NomadXML row = (NomadXML)lista[xml];
                objBatch.SetPro(0, 100, lista.Count, xml);
                objBatch.SetMess("Incorporando registro " + (xml + 1) + " de " + lista.Count);

                //Inicio la Transaccion
                try
                {
                    objRead = NucleusRH.Base.Control_Contratistas.InterfaceImportacionPerCCO.ENTRADA.Get(row.GetAttr("id"));

                    //----------------------------------------------------------
                    //INICIO VALIDACIONES

                    //atributos obligatorios de persona (5)
                    NomadEnvironment.GetTrace().Info(":objRead.d_apellido: " + objRead.d_apellido);
                    if (objRead.d_apellido == "")
                    {
                        objBatch.Err("Linea: " + Linea.ToString() + " - No se especificó el campo apellido, se rechaza el registro.");
                        Errores++;
                        continue;
                    }
                    NomadEnvironment.GetTrace().Info(":objRead.d_nombres: " + objRead.d_nombres);
                    if (objRead.d_nombres == "")
                    {
                        objBatch.Err("Linea: " + Linea.ToString() + " - No se especificó el campo nombre, se rechaza el registro.");
                        Errores++;
                        continue;
                    }
                    NomadEnvironment.GetTrace().Info(":objRead.c_tipo_documento: " + objRead.c_tipo_documento);
                    if (objRead.c_tipo_documento == "")
                    {
                        objBatch.Err("Linea: " + Linea.ToString() + " - No se especificó el campo código tipo de documento, se rechaza el registro.");
                        Errores++;
                        continue;
                    }
                    NomadEnvironment.GetTrace().Info(":objRead.c_nro_documento: " + objRead.c_nro_documento);
                    if (objRead.c_nro_documento == "")
                    {
                        objBatch.Err("Linea: " + Linea.ToString() + " - No se especificó el campo número de documento, se rechaza el registro.");
                        Errores++;
                        continue;
                    }
                    NomadEnvironment.GetTrace().Info(":objRead.c_sexo: " + objRead.c_sexo);
                    if (objRead.c_sexo == "")
                    {
                        objBatch.Err("Linea: " + Linea.ToString() + " - No se especificó el campo sexo, se rechaza el registro. ");
                        Errores++;
                        continue;
                    }

                    //atributos obligatorios de personal_emp (5)
                    NomadEnvironment.GetTrace().Info(":objRead.f_ingreso: " + objRead.f_ingreso);
                    //nota: la comparación la hago así porque esta fecha al no poder ser nula, se le asigna ese valor inicial.
                    if (objRead.f_ingreso.ToShortDateString() == "30/12/1899")
                    {
                        objBatch.Err("Linea: " + Linea.ToString() + " - No se especificó el campo fecha de ingreso, se rechaza el registro.");
                        Errores++;
                        continue;
                    }
                    NomadEnvironment.GetTrace().Info(":objRead.e_numero_legajo: " + objRead.e_numero_legajo);
                    if (objRead.e_numero_legajo == "")
                    {
                        objBatch.Err("Linea: " + Linea.ToString() + " - No se especificó el campo legajo, se rechaza el registro.");
                        Errores++;
                        continue;
                    }
                    NomadEnvironment.GetTrace().Info(":objRead.c_tipo_personal: " + objRead.c_tipo_personal);
                    if (objRead.c_tipo_personal == "")
                    {
                        objBatch.Err("Linea: " + Linea.ToString() + " - No se especificó el campo código de tipo de personal, se rechaza el registro.");
                        Errores++;
                        continue;
                    }
                    NomadEnvironment.GetTrace().Info(":objRead.c_ctro_costo: " + objRead.c_centro_costo);
                    if (objRead.c_centro_costo == "")
                    {
                        objBatch.Err("Linea: " + Linea.ToString() + " - No se especificó el campo código de centro de costo, se rechaza el registro.");
                        Errores++;
                        continue;
                    }
                    NomadEnvironment.GetTrace().Info(":objRead.c_calendario: " + objRead.c_calendario);
                    if (objRead.c_calendario == "")
                    {
                        objBatch.Err("Linea: " + Linea.ToString() + " - No se especificó el campo código de calendario, se rechaza el registro.");
                        Errores++;
                        continue;
                    }

                    //atributos de personal contratista (1)
                    NomadEnvironment.GetTrace().Info(":objRead.f_ingreso_planta: " + objRead.f_ingreso_planta);
                    if (objRead.f_ingreso_planta.ToShortDateString() == "30/12/1899")
                    {
                        objBatch.Err("Linea: " + Linea.ToString() + " - No se especificó el campo fecha de ingreso a planta, se rechaza el registro.");
                        Errores++;
                        continue;
                    }

                    //-------------------------------------

                    string strval = "";
                    //que no exista la persona a ingresar
                    string c_personal = objRead.c_tipo_documento + objRead.c_nro_documento;
                    /*strval = NomadEnvironment.QueryValue("PER01_PERSONAL", "oi_personal", "c_personal", c_personal, "", true);
                    NomadLog.Info("strval 1 -- " + strval);

                    if (strval != null)
                    {
                        objBatch.Err("Linea: " + Linea.ToString() + " - Ya existe una persona con el tipo y número de documento especificado, se rechaza el registro.");
                        Errores++;
                        continue;
                    }*/

                    //que no exista número de legajo en la empresa que viene como parámetro
                    strval = NomadEnvironment.QueryValue("PER02_PERSONAL_EMP", "oi_personal_emp", "e_numero_legajo", objRead.e_numero_legajo, "PER02_PERSONAL_EMP.oi_empresa = " + poi_empresa, true);
                    NomadLog.Info("strval 1 -- " + strval);

                    if (strval != null)
                    {
                        objBatch.Err("Linea: " + Linea.ToString() + " - Ya existe un registro con el número de legajo especificado en la empresa indicada, se rechaza el registro.");
                        Errores++;
                        continue;
                    }

                    //que no se repitan c_personal ni legajos en el archivo de entrada
                    //en realidad compara con la bd.
                    string param = "<ROW c_personal = '" + c_personal + "' e_numero_legajo = '" + objRead.e_numero_legajo + "' oi_empresa = '" + poi_empresa + "'/>";
                    NomadEnvironment.GetTrace().Info("param1: " + param.ToString());
                    NomadXML docCOUNT = NomadEnvironment.QueryNomadXML(Resources.getCantRepetidos, param).FirstChild();
                    NomadEnvironment.GetTrace().Info("docCOUNT: " + docCOUNT.ToString());

                    /*if (docCOUNT.GetAttrInt("cant_per") > 0)
                    {
                        objBatch.Err("Linea: " + Linea.ToString() + " - Los campos tipo y número de documento han sido ingresados previamente, se rechaza el registro.");
                        Errores++;
                        continue;
                    }*/
                    if (docCOUNT.GetAttrInt("cant_leg") > 0)
                    {
                        objBatch.Err("Linea: " + Linea.ToString() + " - El campo número de legajo ha sido ingresado previamente para la empresa seleccionada, se rechaza el registro.");
                        Errores++;
                        continue;
                    }

                    //fechas de ingreso / egreso
                    if (objRead.f_ingreso_planta < objRead.f_ingreso)
                    {
                        objBatch.Err("Linea: " + Linea.ToString() + " - La fecha de ingreso a la planta debe ser mayor o igual a la fecha de ingreso, se rechaza el registro.");
                        Errores++;
                        continue;
                    }
                    if (!objRead.f_egreso_plantaNull && objRead.f_egreso_planta < objRead.f_ingreso_planta)
                    {
                        objBatch.Err("Linea: " + Linea.ToString() + " - La fecha de egreso de la planta debe ser mayor o igual a la fecha de ingreso a la planta, se rechaza el registro.");
                        Errores++;
                        continue;
                    }
                    if (!objRead.f_egreso_plantaNull && !objRead.f_egresoNull && objRead.f_egreso_planta > objRead.f_egreso)
                    {
                        objBatch.Err("Linea: " + Linea.ToString() + " - La fecha de egreso a la planta debe ser menor o igual a la fecha de egreso, se rechaza el registro.");
                        Errores++;
                        continue;
                    }
                    if (!objRead.f_egresoNull && objRead.f_ingreso_planta > objRead.f_egreso)
                    {
                        objBatch.Err("Linea: " + Linea.ToString() + " - La fecha de ingreso a la planta debe ser menor o igual a la fecha de egreso, se rechaza el registro.");
                        Errores++;
                        continue;
                    }

                    //que empresa vinculante no sea una empresa contratista
                    if (objRead.c_empresa_vinculante != "")
                    {
                        strval = NomadEnvironment.QueryValue("ORG03_EMPRESAS", "oi_empresa", "c_empresa", objRead.c_empresa_vinculante, "ORG03_EMPRESAS.l_contratista = 1 ", true);
                        NomadLog.Info("strval 3 -- " + strval);

                        if (strval != null)
                        {
                            objBatch.Err("Linea: " + Linea.ToString() + " - El campo empresa vinculante no puede ser una empresa contratista, se rechaza el registro.");
                            Errores++;
                            continue;
                        }
                    }

                    //----------------------------------------------------
                    //FIN VALIDACIONES

                    //armo parámetro con los datos de la persona para enviar en método IngresoPerCCO
                    //------------------------------------------------------------------------------
                    string oi_tipo_documento, oi_grupo_sanguineo, oi_nacionalidad, oi_estado_civil, oi_localidad;

                    Nomad.NSystem.Proxy.NomadXML paramPER = new Nomad.NSystem.Proxy.NomadXML("DATO");

                    paramPER.SetAttr("d_apellido", objRead.d_apellido);
                    paramPER.SetAttr("d_nombres", objRead.d_nombres);
                    paramPER.SetAttr("d_ape_y_nom", objRead.d_apellido + ", " + objRead.d_nombres);

                    //recupero oi_tipo_documento según el c_tipo_documento del archivo
                    oi_tipo_documento = NomadEnvironment.QueryValue("PER20_TIPOS_DOC", "oi_tipo_documento", "c_tipo_documento", objRead.c_tipo_documento, "", true);
                    if (oi_tipo_documento == "" || oi_tipo_documento == null)
                    {
                        objBatch.Err("Linea: " + Linea.ToString() + " - El campo código de tipo de documento no posee un valor válido, se rechaza el registro.");
                        Errores++;
                        continue;
                    }
                    else
                        paramPER.SetAttr("oi_tipo_documento", oi_tipo_documento);

                    paramPER.SetAttr("c_nro_documento", objRead.c_nro_documento);
                    paramPER.SetAttr("c_personal", c_personal);

                    if (objRead.c_sexo != "F" && objRead.c_sexo != "M")
                    {
                        objBatch.Err("Linea: " + Linea.ToString() + " - El campo sexo no posee un valor válido, se rechaza el registro.");
                        Errores++;
                        continue;
                    }
                    else
                        paramPER.SetAttr("c_sexo", objRead.c_sexo);

                    //recupero oi_grupo_sanguineo según el c_grupo_sanguineo del archivo
                    if (objRead.c_grupo_sanguineo != "")
                    {
                        oi_grupo_sanguineo = NomadEnvironment.QueryValue("PER10_GRUPOS_SANG", "oi_grupo_sanguineo", "c_grupo_sanguineo", objRead.c_grupo_sanguineo, "", true);
                        if (oi_grupo_sanguineo == "" || oi_grupo_sanguineo == null)
                        {
                            objBatch.Err("Linea: " + Linea.ToString() + " - El campo código de grupo sanguíneo no posee un valor válido, se rechaza el registro.");
                            Errores++;
                            continue;
                        }
                        else
                            paramPER.SetAttr("oi_grupo_sanguineo", oi_grupo_sanguineo);
                    }

                    paramPER.SetAttr("te_celular", objRead.te_celular);

                    if (objRead.f_nacim != null)
                    {
                        paramPER.SetAttr("f_nacim", objRead.f_nacim);
                    }
                    else { paramPER.SetAttr("f_nacim", ""); }

                    //valido que el cuil/cuit
                    if (objRead.c_nro_cuil != "")
                    {
                        objRead.c_nro_cuil = objRead.c_nro_cuil.Replace("-", "");

                        if (objRead.c_nro_cuil.Length != 11)
                        {
                            objBatch.Err("Linea: " + Linea.ToString() + " - El campo cuit/cuil no tiene la longitud esperada (11). Recuerde que no tiene que tener guiones.");
                            Errores++;
                            continue;
                        }
                        else
                            paramPER.SetAttr("c_nro_cuil", objRead.c_nro_cuil);

                    }

                    if (objRead.c_nacionalidad != "")
                    {
                        //recupero oi_nacionalidad según el c_nacionalidad del archivo
                        oi_nacionalidad = NomadEnvironment.QueryValue("ORG12_NACIONALID", "oi_nacionalidad", "c_nacionalidad", objRead.c_nacionalidad, "", true);
                        if (oi_nacionalidad == "" || oi_nacionalidad == null)
                        {
                            objBatch.Err("Linea: " + Linea.ToString() + " - El campo código de nacionalidad no posee un valor válido, se rechaza el registro.");
                            Errores++;
                            continue;
                        }
                        else
                            paramPER.SetAttr("oi_nacionalidad", oi_nacionalidad);

                    }

                    if (objRead.c_estado_civil != "")
                    {
                        //recupero oi_estado_civil según el c_estado_civil del archivo
                        oi_estado_civil = NomadEnvironment.QueryValue("ORG22_EST_CIVIL", "oi_estado_civil", "c_estado_civil", objRead.c_estado_civil, "", true);
                        if (oi_estado_civil == "" || oi_estado_civil == null)
                        {
                            objBatch.Err("Linea: " + Linea.ToString() + " - El campo código de estado civil no posee un valor válido, se rechaza el registro.");
                            Errores++;
                            continue;
                        }
                        else
                            paramPER.SetAttr("oi_estado_civil", oi_estado_civil);
                    }

                    if (objRead.c_localidad != "")
                    {
                        //recupero oi_localidad según el c_localidad del archivo
                        oi_localidad = NomadEnvironment.QueryValue("ORG19_LOCALIDADES", "oi_localidad", "c_localidad", objRead.c_localidad, "", true);
                        if (oi_localidad == "" || oi_localidad == null)
                        {
                            objBatch.Err("Linea: " + Linea.ToString() + " - El campo código de localidad no posee un valor válido, se rechaza el registro.");
                            Errores++;
                            continue;
                        }
                        else
                            paramPER.SetAttr("oi_localidad", oi_localidad);
                    }

                    paramPER.SetAttr("oi_foto", "");

                    //--------------------------------------------
                    //fin asignación datos de persona en parámetro

                    //guardar datos en objeto legajoCCO
                    string oi_tipo_personal, oi_centro_costo, oi_calendario, oi_categoria, oi_art, oi_sindicato,
                           oi_mod_contr, oi_empresa_vinc, oi_motivo_eg_per, oi_especialidad, oi_convenio;

                    NucleusRH.Base.Control_Contratistas.LegajoEmpresa.PERSONAL_EMP DDOLegajoCCO =
                        new NucleusRH.Base.Control_Contratistas.LegajoEmpresa.PERSONAL_EMP();

                    DDOLegajoCCO.oi_empresa = poi_empresa;
                    DDOLegajoCCO.f_ingreso = objRead.f_ingreso;
                    DDOLegajoCCO.e_numero_legajo = Convert.ToInt32(objRead.e_numero_legajo);

                    //recupero oi_tipo_personal según el c_tipo_personal del archivo
                    oi_tipo_personal = NomadEnvironment.QueryValue("PER11_TIPOS_PERS", "oi_tipo_personal", "c_tipo_personal", objRead.c_tipo_personal, "", true);
                    if (oi_tipo_personal == "" || oi_tipo_personal == null)
                    {
                        objBatch.Err("Linea: " + Linea.ToString() + " - El campo código de tipo de personal no posee un valor válido, se rechaza el registro.");
                        Errores++;
                        continue;
                    }
                    else
                        DDOLegajoCCO.oi_tipo_personal = oi_tipo_personal;

                    //recupero oi_centro_costo según el c_centro_costo del archivo
                    oi_centro_costo = NomadEnvironment.QueryValue("ORG08_CS_COSTO", "oi_centro_costo", "c_centro_costo", objRead.c_centro_costo, "", true);
                    if (oi_centro_costo == "" || oi_centro_costo == null)
                    {
                        objBatch.Err("Linea: " + Linea.ToString() + " - El campo código de centro de costo no posee un valor válido, se rechaza el registro.");
                        Errores++;
                        continue;
                    }
                    else
                        DDOLegajoCCO.oi_ctro_costo_ult = oi_centro_costo;

                    //recupero oi_calendario según el c_calendario del archivo
                    oi_calendario = NomadEnvironment.QueryValue("ORG27_CAL_FERIADOS", "oi_calendario", "c_calendario", objRead.c_calendario, "", true);
                    if (oi_calendario == "" || oi_calendario == null)
                    {
                        objBatch.Err("Linea: " + Linea.ToString() + " - El campo código de calendario no posee un valor válido, se rechaza el registro.");
                        Errores++;
                        continue;
                    }
                    else
                        DDOLegajoCCO.oi_calendario_ult = oi_calendario;

                    if (objRead.c_categoria != "")
                    {
                        if(objRead.c_convenio!="")
                        {
                            oi_convenio = NomadEnvironment.QueryValue("ORG18_CONVENIOS", "oi_convenio", "c_convenio", objRead.c_convenio, "", true);
                            if (oi_convenio == "" || oi_convenio == null)
                            {
                                objBatch.Err("Linea: " + Linea.ToString() + " - El campo código de convenio no posee un valor válido, se rechaza el registro.");
                                Errores++;
                                continue;
                            }
                            else
                            {
                                //recupero oi_categoria según el c_categoria y el c_convenio (oi_convenio) del archivo
                                oi_categoria = NomadEnvironment.QueryValue("ORG18_CATEGORIAS", "oi_categoria", "c_categoria", objRead.c_categoria, "ORG18_CATEGORIAS.oi_convenio=" + oi_convenio, true);
                                if (oi_categoria == "" || oi_categoria == null)
                                {
                                    objBatch.Err("Linea: " + Linea.ToString() + " - El campo código de categoría no corresponde a un convenio válido, se rechaza el registro.");
                                    Errores++;
                                    continue;
                                }
                                else
                                    DDOLegajoCCO.oi_categoria_ult = oi_categoria;
                            }
                        }
                        else
                        {
                            objBatch.Err("Linea: " + Linea.ToString() + " - El campo código de convenio es requerido porque se ha especificado una categoria.");
                            Errores++;
                            continue;
                        }
                    }

                    if (objRead.c_art != "")
                    {
                        //recupero oi_art según el c_art del archivo
                        oi_art = NomadEnvironment.QueryValue("PER33_ART", "oi_art", "c_art", objRead.c_art, "", true);
                        if (oi_art == "" || oi_art == null)
                        {
                            objBatch.Err("Linea: " + Linea.ToString() + " - El campo código de ART no posee un valor válido, se rechaza el registro.");
                            Errores++;
                            continue;
                        }
                        else
                            DDOLegajoCCO.oi_art = oi_art;
                    }

                    if (objRead.c_sindicato != "")
                    {
                        //recupero oi_sindicato según el c_sindicato del archivo
                        oi_sindicato = NomadEnvironment.QueryValue("PER30_SINDICATOS", "oi_sindicato", "c_sindicato", objRead.c_sindicato, "", true);
                        if (oi_sindicato == "" || oi_sindicato == null)
                        {
                            objBatch.Err("Linea: " + Linea.ToString() + " - El campo código de sindicato no posee un valor válido, se rechaza el registro.");
                            Errores++;
                            continue;
                        }
                        else
                            DDOLegajoCCO.oi_sindicato = oi_sindicato;
                    }

                    if (objRead.c_mod_contr != "")
                    {
                        //recupero oi_mod_contr según el c_mod_contr del archivo
                        oi_mod_contr = NomadEnvironment.QueryValue("PER35_MOD_CONTR", "oi_mod_contr", "c_mod_contr", objRead.c_mod_contr, "", true);
                        if (oi_mod_contr == "" || oi_mod_contr == null)
                        {
                            objBatch.Err("Linea: " + Linea.ToString() + " - El campo código de modo de contratación no posee un valor válido, se rechaza el registro.");
                            Errores++;
                             continue;
                        }
                        else
                            DDOLegajoCCO.oi_mod_contr = oi_mod_contr;
                    }

                    if (objRead.c_empresa_vinculante != "")
                    {
                        //recupero oi_empresa_vinc según el c_empresa_vinc del archivo
                        oi_empresa_vinc = NomadEnvironment.QueryValue("ORG03_EMPRESAS", "oi_empresa", "c_empresa", objRead.c_empresa_vinculante, "", true);
                        if (oi_empresa_vinc == "" || oi_empresa_vinc == null)
                        {
                            objBatch.Err("Linea: " + Linea.ToString() + " - El campo código de empresa vinculante no posee un valor válido, se rechaza el registro.");
                            Errores++;
                            continue;
                        }
                        else
                            DDOLegajoCCO.oi_empresa_vinc = oi_empresa_vinc;
                    }

                    if (!objRead.f_egresoNull)
                    {
                        DDOLegajoCCO.f_egreso = objRead.f_egreso;
                    }
                    else
                        DDOLegajoCCO.f_egresoNull = true;

                    if (objRead.c_motivo_eg_per != "")
                    {
                        //recupero oi_motivo_eg_per según el c_motivo_eg_per del archivo
                        oi_motivo_eg_per = NomadEnvironment.QueryValue("PER22_MOT_EG_PER", "oi_motivo_eg_per", "c_motivo_eg_per", objRead.c_motivo_eg_per, "", true);
                        if (oi_motivo_eg_per == "" || oi_motivo_eg_per == null)
                        {
                            objBatch.Err("Linea: " + Linea.ToString() + " - El campo código de motivo de egreso no posee un valor válido, se rechaza el registro.");
                            Errores++;
                            continue;
                        }
                        else
                            DDOLegajoCCO.oi_motivo_eg_per = oi_motivo_eg_per;
                    }

                    DDOLegajoCCO.o_personal_emp = objRead.o_personal_emp;

                    //----parte contratista----
                    DDOLegajoCCO.f_ingreso_planta = objRead.f_ingreso_planta;

                    if (objRead.c_especialidad != "")
                    {
                        //recupero oi_especialidad según el c_especialidad del archivo
                        oi_especialidad = NomadEnvironment.QueryValue("CCO09_ESPECIALIDADES", "oi_especialidad", "c_especialidad", objRead.c_especialidad, "", true);
                        if (oi_especialidad == "" || oi_especialidad == null)
                        {
                            objBatch.Err("Linea: " + Linea.ToString() + " - El campo código de especialidad no posee un valor válido, se rechaza el registro.");
                            Errores++;
                            continue;
                        }
                        else
                            DDOLegajoCCO.oi_especialidad = oi_especialidad;
                    }

                    DDOLegajoCCO.l_directivo_seg = objRead.l_directivo_seg;

                    if (!objRead.f_egreso_plantaNull)
                    {
                        DDOLegajoCCO.f_egreso_planta = objRead.f_egreso_planta;
                    }
                    else
                        DDOLegajoCCO.f_egreso_plantaNull = true;

                    DDOLegajoCCO.l_entrego_liq_fin = objRead.l_entrego_liq_fin;

                    //---------------------------------------
                    //fin guardar datos en objeto legajoCCO

                    //llamar a método IngresoPerCCO
                    DDOLegajoCCO.IngresoPerCCO(paramPER);

                }

                catch (Exception e)
                {
                    objBatch.Err("Linea: " + Linea.ToString() +" - " + e.Message + " - Error desconocido en registro, por favor verifique el formato del archivo y la correcta asignación de los campos.");
                    Errores++;
                }
             }

             objBatch.Log("Registros Procesados: "+Linea.ToString()+" - Importados: "+(Linea-Errores).ToString());
             objBatch.Log("Finalizado");

    }

  }

}



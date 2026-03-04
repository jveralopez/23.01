using System;
using System.Xml;
using System.IO;
using System.Collections;

using Nomad.NSystem.Document;
using Nomad.NSystem.Proxy;
using Nomad.NSystem.Base;
using Nomad.NSystem.Functions;

namespace NucleusRH.Base.Migracion.Personal.Legajos
{

    //////////////////////////////////////////////////////////////////////////////////
    //Clase Legajo
    public partial class PERSONAL_EMP
    {
        public static void ImportarLegajos()
        {

            int Linea = 0, Errores = 0;

            NomadBatch objBatch;
            objBatch = NomadBatch.GetBatch("Iniciando...", "Importaciòn de Legajos");

            Hashtable HashEMP = new Hashtable();
            Hashtable HashCalendarios = new Hashtable();
            NucleusRH.Base.Migracion.Personal.Legajos.PERSONAL_EMP objRead;
            DateTime fCompare = new DateTime(1900, 1, 1);

            //Cargo la lista de IDS
            Nomad.Base.Manual.HowToLoadFile<NucleusRH.Base.Migracion.Personal.Legajos.PERSONAL_EMP> MyFile = new Nomad.Base.Manual.HowToLoadFile<NucleusRH.Base.Migracion.Personal.Legajos.PERSONAL_EMP>("PER04_Legajos.dat", ';', '.');
            int[] IDS = MyFile.GetIds();

            //RECORRO LOS IDS QUE ENTRAN Y PREGUNTO SI ALGUN ID NO ESTA EN LA HASH, PORQ DE SER ASI HAY QUE AGREGARLO
            for (int xml = 0; xml < IDS.Length; xml++)
            {
                Linea++;
                if (Linea % 100 == 0 || Linea == IDS.Length)
                {
                  objBatch.SetPro(0, 100, IDS.Length, xml);
                  objBatch.SetMess("Incorporando registro " + (xml + 1) + " de " + IDS.Length);
                }

                //Inicio la Transaccion
                try
                {
                    objRead = MyFile.GetObject(IDS[xml]);

                    //Valido atributos obligatorios
                    if (objRead.c_empresa == "")
                    {
                        objBatch.Err("No se especificó la Empresa, se rechaza el registro '" + objRead.c_empresa + " - " + objRead.e_numero_legajo + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }
                    if (objRead.c_personal == "")
                    {
                        objBatch.Err("No se especificó la Persona, se rechaza el registro '" + objRead.c_empresa + " - " + objRead.e_numero_legajo + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }
                    if (objRead.e_numero_legajoNull)
                    {
                        objBatch.Err("No se especificó el Número de Legajo, se rechaza el registro '" + objRead.c_empresa + " - " + objRead.e_numero_legajo + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }
                    if (objRead.c_centro_costo == "")
                    {
                        objBatch.Err("No se especificó el Centro de Costos, se rechaza el registro '" + objRead.c_empresa + " - " + objRead.e_numero_legajo + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }
                    if (objRead.c_calendario == "")
                    {
                        objBatch.Err("No se especificó el Calendario, se rechaza el registro '" + objRead.c_empresa + " - " + objRead.e_numero_legajo + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }
                    if (objRead.c_tipo_personal == "")
                    {
                        objBatch.Err("No se especificó el Tipo de Personal, se rechaza el registro '" + objRead.c_empresa + " - " + objRead.e_numero_legajo + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }

                    if (objRead.f_ingresoNull || objRead.f_ingreso < fCompare)
                    {
                        objBatch.Err("No se especificó Fecha de Ingreso, se rechaza el registro '" + objRead.c_empresa + " - " + objRead.e_numero_legajo + "' - Linea: " + Linea.ToString());
                    }

                    //Recupero los OI de los códigos ingresados

                    string oiAGE = "", oiFUN = "", oiINDACT = "", oiUTPO = "", oiTPER = "", oiOS = "", oiMEGR = "", oiART = "", oiFPAG = "", oiSIN = "", oiPRES = "";
                    string oiEMP = "", oiPER = "", oiCC = "", oiCAL = "", oiTCON = "", oiCON = "", oiMOD = "", oiPUE = "", oiPOS = "", oiCONV = "", oiCAT = "", oiUBI = "";

                    if (objRead.c_empresa != "")
                    {
                        oiEMP = NomadEnvironment.QueryValue("ORG03_EMPRESAS", "oi_empresa", "c_empresa", objRead.c_empresa, "", true);
                        if (oiEMP == null)
                        {
                            objBatch.Err("La Empresa no existe, se rechaza el registro '" + objRead.c_empresa + " - " + objRead.e_numero_legajo + "' - Linea: " + Linea.ToString());
                            Errores++;
                            continue;
                        }
                    }

                    if (!HashEMP.Contains(oiEMP))
                    {
                        NucleusRH.Base.Organizacion.Empresas.EMPRESA EMP = NucleusRH.Base.Organizacion.Empresas.EMPRESA.Get(oiEMP, false);
                        HashEMP.Add(oiEMP, EMP);
                        NomadXML xmlCalendarios = NomadEnvironment.QueryNomadXML(Resources.QRY_CALENDARIO, "<DATA oi_empresa='"+EMP.id+"' />").FirstChild();

                        for (NomadXML MyCUR = xmlCalendarios.FirstChild(); MyCUR != null; MyCUR = MyCUR.Next())
                        {
                            if (!HashCalendarios.Contains(MyCUR.GetAttr("codigo")))
                                HashCalendarios.Add(MyCUR.GetAttr("codigo"), MyCUR.GetAttr("oi_calendario"));
                        }

                    }

                    //Recupero el oi para el calendario STD
                    //oiCAL = NucleusRH.Base.Migracion.Interfaces.INTERFACE.FindCALEMP(oiEMP);

                    if (objRead.c_calendario != "")
                    {
                        if (HashCalendarios.Contains(objRead.c_empresa + "_" + objRead.c_calendario))
                        {
                            oiCAL = HashCalendarios[objRead.c_empresa + "_" + objRead.c_calendario].ToString();
                        }
                        else
                        {
                            objBatch.Err("No se definió el Calendario " + objRead.c_calendario + " para la empresa " + objRead.c_empresa + ", se rechaza el registro '" + objRead.c_empresa + " - " + objRead.e_numero_legajo + "' - Linea: " + Linea.ToString());
                            Errores++;
                            continue;
                        }

                        //Si no ingresa fecha desde se toma la fecha actual
                        if (objRead.f_desde_calendarioNull || objRead.f_desde_calendario < fCompare)
                        {
                            objRead.f_desde_calendario = objRead.f_ingreso;
                        }
                    }

                    if (objRead.c_ubicacion != "")
                    {
                        oiUBI = NomadEnvironment.QueryValue("ORG03_UBICACIONES", "oi_ubicacion", "c_ubicacion", objRead.c_ubicacion, "ORG03_UBICACIONES.oi_empresa = " + oiEMP, true);
                        if (oiUBI == null)
                        {
                            objBatch.Err("La Ubicación no existe en la Empresa, se rechaza el registro '" + objRead.c_empresa + " - " + objRead.e_numero_legajo + "' - Linea: " + Linea.ToString());
                            Errores++;
                            continue;
                        }
                    }

                    if (objRead.c_convenio != "")
                    {
                        oiCONV = NomadEnvironment.QueryValue("ORG18_CONVENIOS", "oi_convenio", "c_convenio", objRead.c_convenio, "", true);
                        if (oiCONV == null)
                        {
                            objBatch.Err("El Convenio no existe, se rechaza el registro '" + objRead.c_empresa + " - " + objRead.e_numero_legajo + "' - Linea: " + Linea.ToString());
                            Errores++;
                            continue;
                        }
                    }

                    if (objRead.c_categoria != "")
                    {
                        oiCAT = NomadEnvironment.QueryValue("ORG18_CATEGORIAS", "oi_categoria", "c_categoria", objRead.c_categoria, "ORG18_CATEGORIAS.oi_convenio = " + oiCONV, true);
                        if (oiCAT == null)
                        {
                            objBatch.Err("La Categoría no existe en el Convenio, se rechaza el registro '" + objRead.c_empresa + " - " + objRead.e_numero_legajo + "' - Linea: " + Linea.ToString());
                            Errores++;
                            continue;
                        }
                        //Si no ingresa fecha desde se toma la fecha actual
                        if (objRead.f_desde_categoriaNull || objRead.f_desde_categoria < fCompare)
                        {
                            objRead.f_desde_categoria = objRead.f_ingreso;
                        }
                    }

                    if (objRead.c_puesto != "")
                    {
                        oiPUE = NomadEnvironment.QueryValue("ORG04_PUESTOS", "oi_puesto", "c_puesto", objRead.c_puesto, "ORG04_PUESTOS.oi_empresa = " + oiEMP, true);
                        if (oiPUE == null)
                        {
                            objBatch.Err("El Puesto no existe en la Empresa, se rechaza el registro '" + objRead.c_empresa + " - " + objRead.e_numero_legajo + "' - Linea: " + Linea.ToString());
                            Errores++;
                            continue;
                        }
                        //Si no ingresa fecha desde se toma la fecha actual
                        if (objRead.f_desde_puestoNull || objRead.f_desde_puesto < fCompare)
                        {
                            objRead.f_desde_puesto = objRead.f_ingreso;
                        }
                    }

                    if (objRead.c_posicion != "")
                    {
                        oiPOS = NomadEnvironment.QueryValue("ORG04_POSICIONES", "oi_posicion", "c_posicion", objRead.c_posicion, "ORG04_POSICIONES.oi_puesto = " + oiPUE, true);
                        if (oiPOS == null)
                        {
                            objBatch.Err("La Posición no existe en el Puesto, se rechaza el registro '" + objRead.c_empresa + " - " + objRead.e_numero_legajo + "' - Linea: " + Linea.ToString());
                            Errores++;
                            continue;
                        }
                        //Si no ingresa fecha desde se toma la fecha actual
                        if (objRead.f_desde_posicionNull || objRead.f_desde_posicion < fCompare)
                        {
                            objRead.f_desde_posicion = objRead.f_ingreso;
                        }
                    }

                    if (objRead.c_tipo_contrato != "")
                    {
                        oiTCON = NomadEnvironment.QueryValue("PER28_TIPOS_CONTR", "oi_tipo_contrato", "c_tipo_contrato", objRead.c_tipo_contrato, "", true);
                        if (oiTCON == null)
                        {
                            objBatch.Err("El Tipo de Contrato no existe, se rechaza el registro '" + objRead.c_empresa + " - " + objRead.e_numero_legajo + "' - Linea: " + Linea.ToString());
                            Errores++;
                            continue;
                        }
                    }

                    if (objRead.c_contrato != "")
                    {
                        oiCON = NomadEnvironment.QueryValue("PER28_CONTRATOS", "oi_contrato", "c_contrato", objRead.c_contrato, "PER28_CONTRATOS.oi_tipo_contrato = " + oiTCON, true);
                        if (oiCON == null)
                        {
                            objBatch.Err("El Contrato no existe en el Tipo de Contrato, se rechaza el registro '" + objRead.c_empresa + " - " + objRead.e_numero_legajo + "' - Linea: " + Linea.ToString());
                            Errores++;
                            continue;
                        }
                        //Si no ingresa fecha desde se toma la fecha actual
                        if (objRead.f_inicio_contratoNull || objRead.f_inicio_contrato < fCompare)
                        {
                            objRead.f_inicio_contrato = objRead.f_ingreso;
                        }
                    }

                    if (objRead.c_mod_contr != "")
                    {
                        oiMOD = NomadEnvironment.QueryValue("PER35_MOD_CONTR", "oi_mod_contr", "c_mod_contr", objRead.c_mod_contr, "", true);
                        if (oiMOD == null)
                        {
                            objBatch.Err("La Modalidad de Contrato no existe, se rechaza el registro '" + objRead.c_empresa + " - " + objRead.e_numero_legajo + "' - Linea: " + Linea.ToString());
                            Errores++;
                            continue;
                        }
                    }

                    if (objRead.c_centro_costo != "")
                    {
                        oiCC = NomadEnvironment.QueryValue("ORG08_CS_COSTO", "oi_centro_costo", "c_centro_costo", objRead.c_centro_costo, "", true);
                        if (oiCC == null)
                        {
                            objBatch.Err("El Centro de Costos no existe, se rechaza el registro '" + objRead.c_empresa + " - " + objRead.e_numero_legajo + "' - Linea: " + Linea.ToString());
                            Errores++;
                            continue;
                        }
                        //Si no ingresa fecha desde se toma la fecha actual
                        if (objRead.f_desde_ccostoNull || objRead.f_desde_ccosto < fCompare)
                        {
                            objRead.f_desde_ccosto = objRead.f_ingreso;
                        }
                    }

                    if (objRead.c_personal != "")
                    {
                        oiPER = NomadEnvironment.QueryValue("PER01_PERSONAL", "oi_personal", "c_personal", objRead.c_personal, "", true);
                        if (oiPER == null)
                        {
                            objBatch.Err("La Persona no existe, se rechaza el registro '" + objRead.c_empresa + " - " + objRead.e_numero_legajo + "' - Linea: " + Linea.ToString());
                            Errores++;
                            continue;
                        }
                    }

                    //Si no ingresa indicador de activa por defecto se establece en 1 (Activo)
                    if (objRead.c_indic_activo != "")
                    {
                        oiINDACT = NomadEnvironment.QueryValue("PER21_INDIC_ACTIVO", "oi_indic_activo", "c_indic_activo", objRead.c_indic_activo, "", true);
                        if (oiINDACT == null)
                        {
                            objBatch.Err("El Indicador de Activo no existe, se rechaza el registro '" + objRead.c_empresa + " - " + objRead.e_numero_legajo + "' - Linea: " + Linea.ToString());
                            Errores++;
                            continue;
                        }

                        //SI ES INACTIVO, la fecha de egreso y el motivo son obligatorios
                        if(objRead.c_indic_activo == "I")
                        {
                            if (objRead.f_egresoNull)
                            {
                                objBatch.Err("La Fecha de Egreso es obligatorio en caso de estar Inactivo, se rechaza el registro '" + objRead.c_empresa + " - " + objRead.e_numero_legajo + "' - Linea: " + Linea.ToString());
                                Errores++;
                                continue;
                            }

                            if (objRead.c_motivo_eg_per == "" || objRead.c_motivo_eg_perNull)
                            {
                                objBatch.Err("El Motivo de Egreso es obligatorio en caso de estar Inactivo, se rechaza el registro '" + objRead.c_empresa + " - " + objRead.e_numero_legajo + "' - Linea: " + Linea.ToString());
                                Errores++;
                                continue;
                            }
                            else
                            {
                                oiMEGR = NomadEnvironment.QueryValue("PER22_MOT_EG_PER", "oi_motivo_eg_per", "c_motivo_eg_per", objRead.c_motivo_eg_per, "", true);
                                if (oiMEGR == null)
                                {
                                    objBatch.Err("El Motivo de Egreso no existe, se rechaza el registro '" + objRead.c_empresa + " - " + objRead.e_numero_legajo + "' - Linea: " + Linea.ToString());
                                    Errores++;
                                    continue;
                                }
                            }
                        }

                    }
                    else
                    {
                        oiINDACT = "1";
                    }

                    if (objRead.c_unidad_tiempo != "")
                    {
                        oiUTPO = NomadEnvironment.QueryValue("ORG25_UNIDADES_TPO", "oi_unidad_tiempo", "c_unidad_tiempo", objRead.c_unidad_tiempo, "", true);
                        if (oiUTPO == null)
                        {
                            objBatch.Err("La Unidad de Tiempo no existe, se rechaza el registro '" + objRead.c_empresa + " - " + objRead.e_numero_legajo + "' - Linea: " + Linea.ToString());
                            Errores++;
                            continue;
                        }
                    }

                    if (objRead.c_tipo_personal != "")
                    {
                        oiTPER = NomadEnvironment.QueryValue("PER11_TIPOS_PERS", "oi_tipo_personal", "c_tipo_personal", objRead.c_tipo_personal, "", true);
                        if (oiTPER == null)
                        {
                            objBatch.Err("El Tipo de Personal no existe, se rechaza el registro '" + objRead.c_empresa + " - " + objRead.e_numero_legajo + "' - Linea: " + Linea.ToString());
                            Errores++;
                            continue;
                        }
                    }

                    if (objRead.c_obra_social != "")
                    {
                        oiOS = NomadEnvironment.QueryValue("PER06_OBRAS_SOC", "oi_obra_social", "c_obra_social", objRead.c_obra_social, "", true);
                        if (oiOS == null)
                        {
                            objBatch.Err("La Obra Social no existe, se rechaza el registro '" + objRead.c_empresa + " - " + objRead.e_numero_legajo + "' - Linea: " + Linea.ToString());
                            Errores++;
                            continue;
                        }
                    }

                    if (objRead.c_prestadora != "")
                    {
                        oiPRES = NomadEnvironment.QueryValue("PER17_PRESTADORAS", "oi_prestadora", "c_prestadora", objRead.c_prestadora, "", true);
                        if (oiPRES == null)
                        {
                            objBatch.Err("La Prestadora no existe, se rechaza el registro '" + objRead.c_empresa + " - " + objRead.e_numero_legajo + "' - Linea: " + Linea.ToString());
                            Errores++;
                            continue;
                        }
                    }

                    if (objRead.c_motivo_eg_per != "" && objRead.c_indic_activo != "I")
                    {
                        oiMEGR = NomadEnvironment.QueryValue("PER22_MOT_EG_PER", "oi_motivo_eg_per", "c_motivo_eg_per", objRead.c_motivo_eg_per, "", true);
                        if (oiMEGR == null)
                        {
                            objBatch.Err("El Motivo de Egreso no existe, se rechaza el registro '" + objRead.c_empresa + " - " + objRead.e_numero_legajo + "' - Linea: " + Linea.ToString());
                            Errores++;
                            continue;
                        }
                    }

                    if (objRead.c_art != "")
                    {
                        oiART = NomadEnvironment.QueryValue("PER33_ART", "oi_art", "c_art", objRead.c_art, "", true);
                        if (oiART == null)
                        {
                            objBatch.Err("La ART no existe, se rechaza el registro '" + objRead.c_empresa + " - " + objRead.e_numero_legajo + "' - Linea: " + Linea.ToString());
                            Errores++;
                            continue;
                        }
                    }

                    if (objRead.c_forma_pago != "")
                    {
                        oiFPAG = NomadEnvironment.QueryValue("PER31_FORMAS_PAGO", "oi_forma_pago", "c_forma_pago", objRead.c_forma_pago, "", true);
                        if (oiFPAG == null)
                        {
                            objBatch.Err("La Forma de Pago no existe, se rechaza el registro '" + objRead.c_empresa + " - " + objRead.e_numero_legajo + "' - Linea: " + Linea.ToString());
                            Errores++;
                            continue;
                        }
                    }

                    //Si ingresa utlima remuneracion calculo la fecha en caso que esta no ingrese
                    //Si no ingresa fecha desde se toma la fecha actual

                    if (objRead.n_ult_remun != 0d)
                    {
                        if (objRead.f_desde_remunNull || objRead.f_desde_remun < fCompare)
                        {
                            objRead.f_desde_remun = objRead.f_ingreso;
                        }
                    }

                    //Valido que la fecha de egreso NO sea anterior a la de inicio
                    if (!(objRead.f_egresoNull) && objRead.f_egreso < objRead.f_ingreso)
                    {
                        objBatch.Err("La Fecha de Egreso no puede ser anterior a la Fecha de Ingreso, se rechaza el registro '" + objRead.e_numero_legajo + " - " + objRead.f_ingreso.ToString("dd/MM/yyyy") + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }

                    //Me fijo si ya existe el Legajo en la Empresa
                    string oiVal = NomadEnvironment.QueryValue("PER02_PERSONAL_EMP", "oi_personal_emp", "e_numero_legajo", objRead.e_numero_legajo.ToString(), "PER02_PERSONAL_EMP.oi_empresa = " + oiEMP, true);
                    if (oiVal != null)
                    {
                        /*if (objRead.c_indic_activo == "I")
                        {
                            NucleusRH.Base.Personal.LegajoEmpresa.PERSONAL_EMP personal_emp = NucleusRH.Base.Personal.LegajoEmpresa.PERSONAL_EMP.Get(oiVal);
                            personal_emp.PreEgreso_Personal();

                            //Cierro Reingreso
                            if (personal_emp.INGRESOS_PER.Count > 0)
                            {
                                NucleusRH.Base.Personal.LegajoEmpresa.INGRESOS_PER ingreso = personal_emp.GetUltimoIngreso();
                                if (ingreso != null)
                                {
                                    NomadEnvironment.GetCurrentTransaction().Begin();
                                    ingreso.f_egreso = objRead.f_egreso;
                                    ingreso.oi_motivo_eg_per = oiMEGR;
                                    ingreso.o_egreso = objRead.o_personal_emp;
                                    NomadEnvironment.GetCurrentTransaction().SaveRefresh(personal_emp);
                                    NomadEnvironment.GetCurrentTransaction().Commit();
                                }
                            }

                            personal_emp.Egreso_Personal(objRead.f_egreso, objRead.o_personal_emp, oiMEGR);
                        }
                        else
                        {
                            objBatch.Err("Ya existe un registro para el Legajo en la Empresa, se rechaza el registro '" + objRead.c_empresa + " - " + objRead.e_numero_legajo + "' - Linea: " + Linea.ToString());
                            Errores++;
                            continue;
                        }*/

                        objBatch.Err("Ya existe un registro para el Legajo en la Empresa, se rechaza el registro '" + objRead.c_empresa + " - " + objRead.e_numero_legajo + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }
                    else
                    {
                        //Trabajo el Sindicato
                        if (objRead.c_sindicato != "")
                        {
                            oiSIN = NomadEnvironment.QueryValue("PER30_SINDICATOS", "oi_sindicato", "c_sindicato", objRead.c_sindicato, "", true);
                            if (oiSIN == null)
                            {
                                NucleusRH.Base.Personal.Sindicatos.SINDICATO DDOSIN;
                                DDOSIN = new NucleusRH.Base.Personal.Sindicatos.SINDICATO();

                                if (objRead.c_sindicato == "" || objRead.d_sindicato == "")
                                {
                                    objBatch.Err("El Código o la Descripción del Sindicato no es válido, se rechaza el registro '" + objRead.c_empresa + " - " + objRead.e_numero_legajo + "' - Linea: " + Linea.ToString());
                                    Errores++;
                                    continue;
                                }
                                DDOSIN.c_sindicato = objRead.c_sindicato;
                                DDOSIN.d_sindicato = objRead.d_sindicato;

                                //Grabo
                                NomadEnvironment.GetCurrentTransaction().SaveRefresh(DDOSIN);
                                oiSIN = DDOSIN.Id;
                                NomadEnvironment.QueryValueChange("PER30_SINDICATOS", "oi_sindicato", "c_sindicato", objRead.c_sindicato, "", oiSIN, true);
                            }
                        }

                        //Trabajo la Agencia
                        if (objRead.c_agencia != "")
                        {
                            oiAGE = NomadEnvironment.QueryValue("PER32_AGENCIAS", "oi_agencia", "c_agencia", objRead.c_agencia, "", true);
                            if (oiAGE == null)
                            {
                                NucleusRH.Base.Personal.Agencias.AGENCIA DDOAGE;
                                DDOAGE = new NucleusRH.Base.Personal.Agencias.AGENCIA();

                                if (objRead.c_agencia == "" || objRead.d_agencia == "")
                                {
                                    objBatch.Err("El Código o la Descripción de la Agencia no es válido, se rechaza el registro '" + objRead.c_empresa + " - " + objRead.e_numero_legajo + "' - Linea: " + Linea.ToString());
                                    Errores++;
                                    continue;
                                }
                                DDOAGE.c_agencia = objRead.c_agencia;
                                DDOAGE.d_agencia = objRead.d_agencia;
                                //Grabo
                                NomadEnvironment.GetCurrentTransaction().SaveRefresh(DDOAGE);
                                oiAGE = DDOAGE.Id;
                                NomadEnvironment.QueryValueChange("PER32_AGENCIAS", "oi_agencia", "c_agencia", objRead.c_agencia, "", oiAGE, true);
                            }
                        }

                        //Trabajo la Función
                        if (objRead.c_funcion != "")
                        {
                            oiFUN = NomadEnvironment.QueryValue("ORG03_FUNCIONES", "oi_funcion", "c_funcion", objRead.c_funcion, "ORG03_FUNCIONES.oi_empresa = " + oiEMP, true);
                            if (oiFUN == null)
                            {
                                NucleusRH.Base.Organizacion.Empresas.FUNCION DDOFUN;
                                DDOFUN = new NucleusRH.Base.Organizacion.Empresas.FUNCION();

                                if (objRead.c_funcion == "" || objRead.d_funcion == "")
                                {
                                    objBatch.Err("El Código o la Descripción de la Funcion no es válido, se rechaza el registro '" + objRead.c_empresa + " - " + objRead.e_numero_legajo + "' - Linea: " + Linea.ToString());
                                    Errores++;
                                    continue;
                                }
                                DDOFUN.c_funcion = objRead.c_funcion;
                                DDOFUN.d_funcion = objRead.d_funcion;

                                NucleusRH.Base.Organizacion.Empresas.EMPRESA DDOEMP = (NucleusRH.Base.Organizacion.Empresas.EMPRESA)HashEMP[oiEMP];
                                DDOEMP.FUNCIONES.Add(DDOFUN);

                                //Grabo
                                NomadEnvironment.GetCurrentTransaction().SaveRefresh(DDOEMP);

                                DDOFUN = null;
                                DDOFUN = (NucleusRH.Base.Organizacion.Empresas.FUNCION)DDOEMP.FUNCIONES.GetByAttribute("c_funcion", objRead.c_funcion);

                                if (DDOFUN == null) {
                                    objBatch.Err("El Código de la Funcion no es válido dentro de la empresa, se rechaza el registro '" + objRead.c_empresa + " - " + objRead.e_numero_legajo + "' - Linea: " + Linea.ToString());
                                    Errores++;
                                    continue;
                                }

                                oiFUN = DDOFUN.Id;
                                NomadEnvironment.QueryValueChange("ORG03_FUNCIONES", "oi_funcion", "c_funcion", objRead.c_funcion, "ORG03_FUNCIONES.oi_empresa = " + oiEMP, oiFUN, true);
                            }
                        }

                        //Creo el Legajo
                        NucleusRH.Base.Personal.LegajoEmpresa.PERSONAL_EMP DDOPEREMP;
                        DDOPEREMP = new NucleusRH.Base.Personal.LegajoEmpresa.PERSONAL_EMP();

                        DDOPEREMP.c_nro_insc_IG = objRead.c_nro_insc_IG;
                        DDOPEREMP.c_nro_interno = objRead.c_nro_interno;
                        DDOPEREMP.c_nro_obra_social = objRead.c_nro_obra_social;
            DDOPEREMP.f_obra_social = objRead.f_obra_social;
            DDOPEREMP.f_obra_socialNull = objRead.f_obra_socialNull;
                        DDOPEREMP.c_nro_sindicato = objRead.c_nro_sindicato;
            DDOPEREMP.f_afiliado_sind = objRead.f_afiliado_sind;
            DDOPEREMP.f_afiliado_sindNull = objRead.f_afiliado_sindNull;
                        DDOPEREMP.c_nro_prestadora = objRead.c_nro_prestadora;
            DDOPEREMP.f_prestadora = objRead.f_prestadora;
            DDOPEREMP.f_prestadoraNull = objRead.f_prestadoraNull;
                        if (objRead.e_cant_dias_vac >= 1) DDOPEREMP.e_cant_dias_vac = objRead.e_cant_dias_vac;    //Si la cantidad de dias es 0 o nula, no debe asignar ningun valor
                        DDOPEREMP.e_duracion_ctrato = objRead.e_duracion_ctrato;
                        DDOPEREMP.e_numero_legajo = objRead.e_numero_legajo;
                        DDOPEREMP.e_q_renov_ctrato = objRead.e_q_renov_ctrato;
                        DDOPEREMP.e_solic_ingreso = objRead.e_solic_ingreso;
                        DDOPEREMP.f_antiguedad_rec = objRead.f_antiguedad_rec;
                        DDOPEREMP.f_antiguedad_recNull = objRead.f_antiguedad_recNull;
                        DDOPEREMP.f_desde_calendario = objRead.f_desde_calendario;
                        DDOPEREMP.f_desde_calendarioNull = objRead.f_desde_calendarioNull;
                        DDOPEREMP.f_desde_categoria = objRead.f_desde_categoria;
                        DDOPEREMP.f_desde_categoriaNull = objRead.f_desde_categoriaNull;
                        DDOPEREMP.f_desde_ccosto = objRead.f_desde_ccosto;
                        DDOPEREMP.f_desde_ccostoNull = objRead.f_desde_ccostoNull;
                        DDOPEREMP.f_desde_posicion = objRead.f_desde_posicion;
                        DDOPEREMP.f_desde_posicionNull = objRead.f_desde_posicionNull;
                        DDOPEREMP.f_desde_puesto = objRead.f_desde_puesto;
                        DDOPEREMP.f_desde_puestoNull = objRead.f_desde_puestoNull;
                        DDOPEREMP.f_desde_remun = objRead.f_desde_remun;
                        DDOPEREMP.f_desde_remunNull = objRead.f_desde_remunNull;
                        DDOPEREMP.f_efectividad = objRead.f_efectividad;
                        DDOPEREMP.f_efectividadNull = objRead.f_efectividadNull;

                        if (objRead.c_indic_activo != "I")
                        {
                            DDOPEREMP.f_egreso = objRead.f_egreso;
                            DDOPEREMP.f_egresoNull = objRead.f_egresoNull;
                        }
                        else
                            DDOPEREMP.f_egresoNull = true;

                        DDOPEREMP.f_fin_contrato = objRead.f_fin_contrato;
                        DDOPEREMP.f_fin_contratoNull = objRead.f_fin_contratoNull;
                        DDOPEREMP.f_inactividad = objRead.f_inactividad;
                        DDOPEREMP.f_inactividadNull = objRead.f_inactividadNull;
                        DDOPEREMP.f_ingreso = objRead.f_ingreso;
                        DDOPEREMP.f_inicio_contrato = objRead.f_inicio_contrato;
                        DDOPEREMP.f_inicio_contratoNull = objRead.f_inicio_contratoNull;
                        DDOPEREMP.l_confidencial = objRead.l_confidencial;
                        DDOPEREMP.l_disponib_viajar = objRead.l_disponib_viajar;
                        DDOPEREMP.l_eventual = objRead.l_eventual;
                        DDOPEREMP.n_est_pas_mes = objRead.n_est_pas_mes;
                        DDOPEREMP.n_porc_jor_red = objRead.n_porc_jor_red;
                        DDOPEREMP.e_anios_antig = objRead.e_anios_antig;
                        DDOPEREMP.l_afiliado_sind = objRead.l_afiliado_sind;

                        if (objRead.n_ult_remun != 0d)
                        {
                            DDOPEREMP.n_ult_remun = objRead.n_ult_remun;
                        }

                        DDOPEREMP.o_personal_emp = objRead.o_personal_emp;

                        if (oiAGE != "") DDOPEREMP.oi_agencia = oiAGE;
                        if (oiART != "") DDOPEREMP.oi_art = oiART;
                        if (oiCAL != "") DDOPEREMP.oi_calendario_ult = oiCAL;
                        if (oiCAT != "") DDOPEREMP.oi_categoria_ult = oiCAT;
                        if (oiCON != "") DDOPEREMP.oi_contrato = oiCON;
                        if (oiMOD != "") DDOPEREMP.oi_mod_contr = oiMOD;
                        if (oiCC != "") DDOPEREMP.oi_ctro_costo_ult = oiCC;
                        if (oiEMP != "") DDOPEREMP.oi_empresa = oiEMP;
                        if (oiFPAG != "") DDOPEREMP.oi_forma_pago = oiFPAG;
                        if (oiFUN != "") DDOPEREMP.oi_funcion = oiFUN;

                        if (objRead.c_indic_activo != "I")
                        {
                            if (oiINDACT != "") DDOPEREMP.oi_indic_activo = oiINDACT;
                        }

                        //if (oiMEGR != "") DDOPEREMP.oi_motivo_eg_per = oiMEGR;

                        if (oiOS != "") DDOPEREMP.oi_obra_social = oiOS;
            if (oiPRES != "") DDOPEREMP.oi_prestadora = oiPRES;
                        if (oiPER != "") DDOPEREMP.oi_personal = oiPER;
                        //if(oiOS != "") DDOPEREMP.oi_plan_os = oiOS;
                        if (oiPOS != "") DDOPEREMP.oi_posicion_ult = oiPOS;
                        if (oiPUE != "") DDOPEREMP.oi_puesto_ult = oiPUE;
                        if (oiSIN != "") DDOPEREMP.oi_sindicato = oiSIN;
                        if (oiTPER != "") DDOPEREMP.oi_tipo_personal = oiTPER;
                        if (oiUBI != "") DDOPEREMP.oi_ubicacion = oiUBI;
                        if (oiUTPO != "") DDOPEREMP.oi_unidad_tiempo = oiUTPO;

                        //Creo un registro de CATEGORIA para el Legajo
                        if (objRead.c_categoria != "")
                        {
                            NucleusRH.Base.Personal.LegajoEmpresa.CATEGORIA_PER DDOCATPER;
                            DDOCATPER = new NucleusRH.Base.Personal.LegajoEmpresa.CATEGORIA_PER();
                            DDOCATPER.f_ingreso = objRead.f_desde_categoria;
                            DDOCATPER.oi_categoria = oiCAT;
                            DDOPEREMP.CATEG_PER.Add(DDOCATPER);
                        }

                        //Creo un registro de CENTRO DE COSTO para el Legajo
                        if (objRead.c_centro_costo != "")
                        {
                            NucleusRH.Base.Personal.LegajoEmpresa.CCOSTO_PER DDOCCPER;
                            DDOCCPER = new NucleusRH.Base.Personal.LegajoEmpresa.CCOSTO_PER();
                            DDOCCPER.f_ingreso = objRead.f_desde_ccosto;
                            DDOCCPER.oi_centro_costo = oiCC;
                            DDOPEREMP.CCOSTO_PER.Add(DDOCCPER);
                        }

                        //Creo un registro de PUESTO para el Legajo
                        if (objRead.c_puesto != "")
                        {
                            NucleusRH.Base.Personal.LegajoEmpresa.PUESTO_PER DDOPUEPER;
                            DDOPUEPER = new NucleusRH.Base.Personal.LegajoEmpresa.PUESTO_PER();
                            DDOPUEPER.f_ingreso = objRead.f_desde_puesto;
                            DDOPUEPER.oi_puesto = oiPUE;
                            DDOPEREMP.PUESTO_PER.Add(DDOPUEPER);
                        }

                        //Creo un registro de POSICION para el Legajo
                        if (objRead.c_posicion != "")
                        {
                            NucleusRH.Base.Personal.LegajoEmpresa.POSICION_PER DDOPOSPER;
                            DDOPOSPER = new NucleusRH.Base.Personal.LegajoEmpresa.POSICION_PER();
                            DDOPOSPER.f_ingreso = objRead.f_desde_posicion;
                            DDOPOSPER.oi_posicion = oiPOS;
                            DDOPEREMP.POSIC_PER.Add(DDOPOSPER);
                        }

                        //Creo un registro de REMUNERACION para el Legajo
                        if (objRead.n_ult_remun != 0d)
                        {
                            NucleusRH.Base.Personal.LegajoEmpresa.REMUN_PER DDOREMPER;
                            DDOREMPER = new NucleusRH.Base.Personal.LegajoEmpresa.REMUN_PER();
                            DDOREMPER.f_desde = objRead.f_desde_remun;
                            DDOREMPER.n_remun_per = objRead.n_ult_remun;
                            DDOPEREMP.REMUN_PER.Add(DDOREMPER);
                        }

                        //Creo un registro de CALENDARIO para el Legajo
                        if (objRead.c_calendario != "")
                        {
                            NucleusRH.Base.Personal.LegajoEmpresa.CALENDARIO_PER DDOCALPER;
                            DDOCALPER = new NucleusRH.Base.Personal.LegajoEmpresa.CALENDARIO_PER();
                            DDOCALPER.f_desde = objRead.f_desde_calendario;
                            DDOCALPER.oi_calendario = oiCAL;
                            DDOPEREMP.CALENDARIO_PER.Add(DDOCALPER);
                        }

                        //Creo un registro de TIPO PERSONAL para el Legajo
                        if (objRead.c_tipo_personal != "")
                        {
                            NucleusRH.Base.Personal.LegajoEmpresa.TIPOP_PER DDOTIPOPER;
                            DDOTIPOPER = new NucleusRH.Base.Personal.LegajoEmpresa.TIPOP_PER();
                            DDOTIPOPER.f_ingreso = objRead.f_ingreso;
                            DDOTIPOPER.oi_tipo_personal = oiTPER;
                            DDOPEREMP.TIPOSP_PER.Add(DDOTIPOPER);
                        }

                        //Creo un registro de UBICACION para el Legajo
                        if (objRead.c_ubicacion != "")
                        {
                            NucleusRH.Base.Personal.LegajoEmpresa.UBICACION_PER DDOUBIPER;
                            DDOUBIPER = new NucleusRH.Base.Personal.LegajoEmpresa.UBICACION_PER();
                            DDOUBIPER.f_desde = objRead.f_ingreso;
                            DDOUBIPER.oi_ubicacion = oiUBI;
                            DDOPEREMP.UBIC_PER.Add(DDOUBIPER);
                        }

                        //Creo un registro de INGRESO para el Legajo
                        NucleusRH.Base.Personal.LegajoEmpresa.INGRESOS_PER DDOINGPER;
                        DDOINGPER = new NucleusRH.Base.Personal.LegajoEmpresa.INGRESOS_PER();
                        DDOINGPER.f_ingreso = objRead.f_ingreso;
                        DDOPEREMP.INGRESOS_PER.Add(DDOINGPER);

                        //Grabo
                        try
                        {
                            NomadEnvironment.GetCurrentTransaction().SaveRefresh(DDOPEREMP);
                            NomadEnvironment.QueryValueChange("PER02_PERSONAL_EMP", "oi_personal_emp", "e_numero_legajo", objRead.e_numero_legajo.ToString(), "PER02_PERSONAL_EMP.oi_empresa = " + oiEMP,"1", true);

                            //Si se asigna posicion, hay que agregar al Legajo en la Estructura determinada por la posicion
                            if (objRead.c_posicion != "")
                            {
                                //Recupero la unidad organizativa determinada por la posicion
                                NomadXML xmldata, xmlestr;
                                string param = "<DATOS oi_empresa=\"" + oiEMP + "\" oi_posicion=\"" + oiPOS + "\"/>";
                                xmldata = new NomadXML();
                                xmlestr = new NomadXML();
                                xmldata.SetText(NomadEnvironment.QueryString(NucleusRH.Base.Migracion.Personal.Legajos.PERSONAL_EMP.Resources.QRY_DATOS_POS, param));
                                NomadEnvironment.GetTrace().Info(xmldata.ToString());

                                //Una vez recuperada la unidad organizativa y la clase tiro otro qry de la interface de posiciones
                                //que me trae la estructura determinada por ambas

                                xmlestr.SetText(NomadEnvironment.QueryString(NucleusRH.Base.Migracion.Organizacion.Posiciones.POSICION.Resources.QRY_ESTRUCTURA, xmldata.ToString()));
                                NomadEnvironment.GetTrace().Info(xmlestr.ToString());

                                //Creo un nuevo objeto de Estructuras por Persona
                                if (xmlestr.FirstChild().GetAttr("oi_estructura") == "")
                                {
                                    objBatch.Err("No existe la estructura para la posicion: '" + objRead.c_posicion + " - " + oiPOS + "' - Linea: " + Linea.ToString());
                                    Errores++;
                                    continue;
                                }
                                NucleusRH.Base.Organizacion.Clases_Organizativas.ESTRUCTURA DDOESTR = NucleusRH.Base.Organizacion.Clases_Organizativas.ESTRUCTURA.Get(xmlestr.FirstChild().GetAttr("oi_estructura"), false);
                                NucleusRH.Base.Organizacion.Clases_Organizativas.ESTRUC_PERS DDOESTRPER;
                                DDOESTRPER = new NucleusRH.Base.Organizacion.Clases_Organizativas.ESTRUC_PERS();
                                DDOESTRPER.oi_clase_org = xmldata.FirstChild().GetAttr("oi_clase_org");
                                DDOESTRPER.oi_personal_emp = DDOPEREMP.Id;

                                DDOESTR.ESTRUC_PERS.Add(DDOESTRPER);
                                //Grabo la persona en la estructura
                                NomadEnvironment.GetCurrentTransaction().Save(DDOESTR);

                            }

                            //LEGAJO INACTIVO
                            if(objRead.c_indic_activo == "I")
                            {
                                DDOPEREMP.PreEgreso_Personal();

                                //Cierro Reingreso
                                if (DDOPEREMP.INGRESOS_PER.Count > 0)
                                {
                                    NucleusRH.Base.Personal.LegajoEmpresa.INGRESOS_PER ingreso = DDOPEREMP.GetUltimoIngreso();
                                    if(ingreso != null)
                                    {
                                        NomadEnvironment.GetCurrentTransaction().Begin();
                                        ingreso.f_egreso = objRead.f_egreso;
                                        ingreso.oi_motivo_eg_per = oiMEGR;
                                        ingreso.o_egreso = objRead.o_personal_emp;
                                        NomadEnvironment.GetCurrentTransaction().SaveRefresh(DDOPEREMP);
                                    }
                                }

                                DDOPEREMP.Egreso_Personal(objRead.f_egreso, objRead.o_personal_emp, oiMEGR);
                            }

                        }
                        catch (Exception e)
                        {
                            objBatch.Err("Error al grabar registro " + objRead.c_empresa + " - " + objRead.e_numero_legajo + "' - Linea: " + Linea.ToString() + " - " + e.Message);
                            Errores++;
                        }
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

    }
}



using System;
using System.Xml;
using System.IO;
using System.Collections;

using Nomad.NSystem.Document;
using Nomad.NSystem.Proxy;
using Nomad.NSystem.Base;
using Nomad.NSystem.Functions;

namespace NucleusRH.Base.Migracion.Organizacion.Posiciones
{

    //////////////////////////////////////////////////////////////////////////////////
    //Clase Posiciones Laborales
    public partial class POSICION
    {
        public static void ImportarPosiciones()
        {

            int Linea = 0, Errores = 0;

            NomadBatch objBatch;
            objBatch = NomadBatch.GetBatch("Iniciando...", "Importacion de Posiciones");

            NomadXML IDList = new NomadXML();
            NucleusRH.Base.Migracion.Organizacion.Posiciones.POSICION objRead;
            string idestrpadre = "";
            int cpadre = 0;

            IDList.SetText(NomadProxy.GetProxy().SQLService().Get(Resources.QRY_REGISTROS, ""));

            ArrayList lista = (ArrayList)IDList.FirstChild().GetElements("ROW");
            //RECORRO LOS IDS QUE ENTRAN Y PREGUNTO SI ALGUN ID NO ESTA EN LA HASH, PORQ DE SER ASI HAY QUE AGREGARLO
            for (int xml = 0; xml < lista.Count; xml++)
            {
                Linea++;
                NomadXML row = (NomadXML)lista[xml];
                objBatch.SetPro(0, 100, lista.Count, xml);
                objBatch.SetMess("Incorporando registro " + (xml + 1) + " de " + lista.Count);

                //Inicio la Transaccion
                try
                {
                    objRead = NucleusRH.Base.Migracion.Organizacion.Posiciones.POSICION.Get(row.GetAttr("id"));
                    bool PosPadre;
                    string oiEMPRESA, oiPUESTO, oiPUESTOPADRE, oiCLASE;
                    string oiPOSPADRE = "", oiCC = "", oiBS = "", oiNS = "", oiCONV = "", oiCAT = "", oiESTCIV = "", oiDpto = null, oiSector = null;

                    //Trabajo la empresa
                    oiEMPRESA = NomadEnvironment.QueryValue("ORG03_EMPRESAS", "oi_empresa", "c_empresa", objRead.c_empresa, "", true);
                    if (oiEMPRESA == null)
                    {
                        objBatch.Err("El Codigo de la Empresa no existe, se rechaza el registro '" + objRead.c_posicion + " - " + objRead.d_posicion + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }

                    //Trabajo el puesto
                    oiPUESTO = NomadEnvironment.QueryValue("ORG04_PUESTOS", "oi_puesto", "c_puesto", objRead.c_puesto, "ORG04_PUESTOS.oi_empresa = " + oiEMPRESA, true);
                    if (oiPUESTO == null)
                    {
                        objBatch.Err("El Codigo del Puesto no existe, se rechaza el registro '" + objRead.c_posicion + " - " + objRead.d_posicion + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }

                    //Trabajo el puesto padre
                    oiPUESTOPADRE = NomadEnvironment.QueryValue("ORG04_PUESTOS", "oi_puesto", "c_puesto", objRead.c_puesto_padre, "ORG04_PUESTOS.oi_empresa = " + oiEMPRESA, true);
                    if (oiPUESTO == null)
                    {
                        objBatch.Err("El Codigo del Puesto Padre no existe, se rechaza el registro '" + objRead.c_posicion + " - " + objRead.d_posicion + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }

                    if (objRead.c_departamento != "")
                    {
                        oiDpto = NomadEnvironment.QueryValue("ORG51_DEPARTAMENTOS", "oi_departamento", "c_departamento", objRead.c_departamento, "ORG51_DEPARTAMENTOS.oi_empresa = " + oiEMPRESA, true);
                        if (oiDpto == null)
                        {
                            objBatch.Err("El Codigo del Departamento no existe, se rechaza el registro '" + objRead.c_posicion + " - " + objRead.d_posicion + "' - Linea: " + Linea.ToString());
                            Errores++;
                            continue;
                        }
                        else
                        {
                            if (objRead.c_sector != "")
                            {
                                oiSector = NomadEnvironment.QueryValue("ORG51_SECTORES", "oi_sector", "c_sector", objRead.c_sector, "ORG51_SECTORES.oi_departamento = " + oiDpto, true);
                                if (oiSector == null)
                                {
                                    objBatch.Err("El Codigo del Sector no existe, se rechaza el registro '" + objRead.c_posicion + " - " + objRead.d_posicion + "' - Linea: " + Linea.ToString());
                                    Errores++;
                                    continue;
                                }
                            }
                        }
                    }

                    //Me fijo si ya existe la Posicion en el Puesto en la Empresa
                    string oiVal = NomadEnvironment.QueryValue("ORG04_POSICIONES", "oi_posicion", "c_posicion", objRead.c_posicion, "ORG04_POSICIONES.oi_puesto= " + oiPUESTO, true);
                    if (oiVal != null)
                    {
                        objBatch.Err("Ya existe un registro para la Posicion '" + objRead.c_posicion + " - " + objRead.d_posicion + "' en el Puesto especificado - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }
                    else
                    {
                        //Levanto el puesto
                        NucleusRH.Base.Organizacion.Puestos.PUESTO DDOPuesto;
                        DDOPuesto = NucleusRH.Base.Organizacion.Puestos.PUESTO.Get(oiPUESTO, false);

                        //Ni la categoria-convenio ni la banda salarial son obligatorios
                        //En caso de que algun valor tenga valor deberá validar que ese exista o sea correcto
                        if (objRead.c_convenio != "" || objRead.c_categoria != "")
                        {
                            if (objRead.c_convenio == "")
                            {
                                objBatch.Err("Se especificó el Código de la Categoría pero no el del Convenio, se rechaza el registro '" + objRead.c_posicion + " - " + objRead.d_posicion + "' - Linea: " + Linea.ToString());
                                Errores++;
                                continue;
                            }

                            if (objRead.c_categoria == "")
                            {
                                objBatch.Err("Se especificó el Código del Convenio pero no el de la Categoría, se rechaza el registro '" + objRead.c_posicion + " - " + objRead.d_posicion + "' - Linea: " + Linea.ToString());
                                Errores++;
                                continue;
                            }

                            //Recupero el Convenio para recuperar la Categoria
                            oiCONV = NomadEnvironment.QueryValue("ORG18_CONVENIOS", "oi_convenio", "c_convenio", objRead.c_convenio, "", true);
                            if (oiCONV == null)
                            {
                                objBatch.Err("El Codigo del Convenio no existe, se rechaza el registro '" + objRead.c_posicion + " - " + objRead.d_posicion + "' - Linea: " + Linea.ToString());
                                Errores++;
                                continue;
                            }
                            //Recupero la Categoria
                            oiCAT = NomadEnvironment.QueryValue("ORG18_CATEGORIAS", "oi_categoria", "c_categoria", objRead.c_categoria, "ORG18_CATEGORIAS.oi_convenio = " + oiCONV, true);
                            if (oiCAT == null)
                            {
                                objBatch.Err("El Codigo de la Categoria no existe, se rechaza el registro '" + objRead.c_posicion + " - " + objRead.d_posicion + "' - Linea: " + Linea.ToString());
                                Errores++;
                                continue;
                            }

                        }

                        //Recupero el codigo de banda salarial para recuperar el nivel de banda salarial y asociarla a la posicion
                        if (objRead.c_banda_sal != "" || objRead.c_nivel_sal != "")
                        {
                            if (objRead.c_banda_sal == "")
                            {
                                objBatch.Err("Se especificó el Nivel Salarial pero no la Banda Salarial al que pertenece, se rechaza el registro '" + objRead.c_posicion + " - " + objRead.d_posicion + "' - Linea: " + Linea.ToString());
                                Errores++;
                                continue;
                            }

                            if (objRead.c_nivel_sal == "")
                            {
                                objBatch.Err("Se especificó la Banda Salarial pero no el Nivel Salarial, se rechaza el registro '" + objRead.c_posicion + " - " + objRead.d_posicion + "' - Linea: " + Linea.ToString());
                                Errores++;
                                continue;
                            }

                            //Recupero La Banda Salarial para recuperar el nivel Salarial
                            oiBS = NomadEnvironment.QueryValue("ORG17_BANDAS_SAL", "oi_banda_sal", "c_banda_sal", objRead.c_banda_sal, "", true);
                            if (oiBS == null)
                            {
                                objBatch.Err("El Codigo de la Banda Salarial no existe, se rechaza el registro '" + objRead.c_posicion + " - " + objRead.d_posicion + "' - Linea: " + Linea.ToString());
                                Errores++;
                                continue;
                            }

                            //Recupero nivel de la Banda Salarial
                            oiNS = NomadEnvironment.QueryValue("ORG17_NIVELES_SAL", "oi_nivel_sal", "c_nivel_sal", objRead.c_nivel_sal, "ORG17_NIVELES_SAL.oi_banda_sal = " + oiBS, true);
                            if (oiNS == null)
                            {
                                objBatch.Err("El Codigo del Nivel Salarial no existe, se rechaza el registro '" + objRead.c_posicion + " - " + objRead.d_posicion + "' - Linea: " + Linea.ToString());
                                Errores++;
                                continue;
                            }
                        }

                        if (!objRead.c_centro_costoNull)
                        {
                            oiCC = NomadEnvironment.QueryValue("ORG08_CS_COSTO", "oi_centro_costo", "c_centro_costo", objRead.c_centro_costo, "", true);
                            if (oiCC == null)
                            {
                                objBatch.Err("El Codigo del Centro de Costos no existe, se rechaza el registro '" + objRead.c_posicion + " - " + objRead.d_posicion + "' - Linea: " + Linea.ToString());
                                Errores++;
                                continue;
                            }
                        }

                        //Recupero el Estado Civil
                        if (!objRead.c_estado_civilNull)
                        {
                            oiESTCIV = NomadEnvironment.QueryValue("ORG22_EST_CIVIL", "oi_estado_civil", "c_estado_civil", objRead.c_estado_civil, "", true);
                            if (oiESTCIV == null)
                            {
                                objBatch.Err("El Codigo del Estado Civil Requerido no existe, se rechaza el registro '" + objRead.c_posicion + " - " + objRead.d_posicion + "' - Linea: " + Linea.ToString());
                                Errores++;
                                continue;
                            }
                        }

                        //Valido en caso que haya ingresado, que exista la Posicion Padre
                        if (objRead.c_posicion_padre != "" && objRead.c_puesto_padre != "")
                        {
                            //Marco que la posicion que esta ingresando tiene un padre
                            PosPadre = true;
                            oiPOSPADRE = NomadEnvironment.QueryValue("ORG04_POSICIONES", "oi_posicion", "c_posicion", objRead.c_posicion_padre, "ORG04_POSICIONES.oi_puesto = " + oiPUESTOPADRE, true);

                            if (oiPOSPADRE == null)
                            {
                                objBatch.Err("El Codigo de Posicion Padre no existe, se rechaza el registro '" + objRead.c_posicion + " - " + objRead.d_posicion + "' - Linea: " + Linea.ToString());
                                Errores++;
                                continue;
                            }
                        }
                        else
                        {
                            //Marco que la posicion que esta ingresando no tiene un padre
                            PosPadre = false;
                        }

                        //Creo la Posicion
                        NucleusRH.Base.Organizacion.Puestos.POSICION DDOPosicion;
                        DDOPosicion = new NucleusRH.Base.Organizacion.Puestos.POSICION();

                        if (objRead.c_posicion == "" || objRead.c_posicion == "")
                        {
                            objBatch.Err("El Codigo o la Descripcion de la Posicion no es valido, se rechaza el registro '" + objRead.c_posicion + " - " + objRead.d_posicion + "' - Linea: " + Linea.ToString());
                            Errores++;
                            continue;
                        }

                        DDOPosicion.c_posicion = objRead.c_posicion;
                        DDOPosicion.d_posicion = objRead.d_posicion;
                        DDOPosicion.d_sexo_req = objRead.d_sexo_req;
                        DDOPosicion.e_edad_max_req = objRead.e_edad_max_req;
                        DDOPosicion.e_edad_max_reqNull = objRead.e_edad_max_reqNull;
                        DDOPosicion.e_edad_min_req = objRead.e_edad_min_req;
                        DDOPosicion.e_edad_min_reqNull = objRead.e_edad_min_reqNull;
                        DDOPosicion.o_posicion = objRead.o_posicion;
                        if (oiDpto != null) DDOPosicion.oi_departamento = oiDpto;
                        if (oiSector != null) DDOPosicion.oi_sector = oiSector;
                        if (oiCAT != null) DDOPosicion.oi_categoria = oiCAT;
                        if (oiNS != null) DDOPosicion.oi_nivel_sal = oiNS;

                        if (oiCC != "") DDOPosicion.oi_centro_costo = oiCC;
                        if (oiESTCIV != "") DDOPosicion.oi_estado_civil = oiESTCIV;

                        //Creo la Unidad Organizativa
                        NucleusRH.Base.Organizacion.Unidades_Organizativas.TIPO_UNI_ORG ddoTU = NucleusRH.Base.Organizacion.Unidades_Organizativas.TIPO_UNI_ORG.Get(3, false);
                        NucleusRH.Base.Organizacion.Unidades_Organizativas.UNIDAD_ORG DDOUNIORG;
                        DDOUNIORG = new NucleusRH.Base.Organizacion.Unidades_Organizativas.UNIDAD_ORG();
                        DDOUNIORG.c_unidad_org = objRead.c_empresa + "-" + objRead.c_puesto + "-" + objRead.c_posicion;
                        DDOUNIORG.d_unidad_org = objRead.d_posicion;
                        DDOUNIORG.o_unidad_org = objRead.o_posicion;

                        ddoTU.UNI_ORG.Add(DDOUNIORG);

                        //Grabo la Unidad Organizativa
                        NomadEnvironment.GetCurrentTransaction().Save(ddoTU);

                        //Cargo el id de la uni org
                        string oiUNIORG = NomadEnvironment.QueryValue("ORG01_UNI_ORG", "oi_unidad_org", "c_unidad_org", objRead.c_empresa + "-" + objRead.c_puesto + "-" + objRead.c_posicion, "ORG01_UNI_ORG.oi_tipo_uni_org = 3", false);

                        DDOPosicion.oi_unidad_org = oiUNIORG;

                        DDOPuesto.POSICIONES.Add(DDOPosicion);
                        NomadEnvironment.QueryValueChange("ORG04_POSICIONES", "oi_posicion", "c_posicion", objRead.c_posicion, "ORG04_POSICIONES.oi_puesto= " + oiPUESTO, "1", true);
                        NomadEnvironment.GetCurrentTransaction().Save(DDOPuesto);

                        //Creacion del Organigrama Jerarquico de Posiciones
                        //Creo la Clase
                        NucleusRH.Base.Organizacion.Clases_Organizativas.CLASE_ORG DDOCLASE = null;
                        NucleusRH.Base.Organizacion.Clases_Organizativas.ESTRUCTURA DDOESTR = null;
                        NucleusRH.Base.Organizacion.Clases_Organizativas.ESTRUCTURA DDOESTRSAVE = null;

                        //Me fijo si ya existe la Clase Organizativa para la empresa
                        oiCLASE = NomadEnvironment.QueryValue("ORG02_CLASES_ORG", "oi_clase_org", "c_clase_org", objRead.c_empresa + " POS", "", true);
                        if (oiCLASE == null)
                        {
                            //La Clase no existe
                            DDOCLASE = new NucleusRH.Base.Organizacion.Clases_Organizativas.CLASE_ORG();
                            //Cargo la Empresa
                            NucleusRH.Base.Organizacion.Empresas.EMPRESA DDOEMPRESA;
                            DDOEMPRESA = NucleusRH.Base.Organizacion.Empresas.EMPRESA.Get(oiEMPRESA, false);

                            //Pego los valores
                            DDOCLASE.c_clase_org = objRead.c_empresa + " POS";
                            DDOCLASE.d_clase_org = "Posiciones " + DDOEMPRESA.d_empresa;
                            DDOCLASE.l_automatica = true;
                            DDOCLASE.o_clase_org = "Organigrama construido a partir de migracion inicial de Posiciones";
                            DDOCLASE.oi_estructura_org.oi_unidad_org = DDOEMPRESA.oi_unidad_org;
                            DDOCLASE.oi_estructura_org.oi_estr_padreNull = true;
                            DDOCLASE.oi_estructura_org.oi_claseNull = true;

                            //Tengo que guardar la clase para poder seguir trabajando
                            NomadEnvironment.GetCurrentTransaction().SaveRefresh(DDOCLASE);
                            oiCLASE = DDOCLASE.Id;
                            NomadEnvironment.QueryValueChange("ORG02_CLASES_ORG", "oi_clase_org", "c_clase_org", objRead.c_empresa + " POS", "", oiCLASE, true);
                        }

                        //Pregunto si ingreso el atributo "Posicion Padre"
                        if (PosPadre)
                        {
                            //Cargo la posicion padre
                            NucleusRH.Base.Organizacion.Puestos.POSICION DDOPOSPADRE;
                            DDOPOSPADRE = NucleusRH.Base.Organizacion.Puestos.POSICION.Get(oiPOSPADRE, false);

                            //Recupero la estructura determinada por la posicion padre.
                            NomadXML xmlestr;
                            string param = "<DATOS oi_clase_org=\"" + oiCLASE + "\" oi_unidad_org=\"" + DDOPOSPADRE.oi_unidad_org + "\"/>";
                            xmlestr = new NomadXML();

                            xmlestr.SetText(NomadEnvironment.QueryString(NucleusRH.Base.Migracion.Organizacion.Posiciones.POSICION.Resources.QRY_ESTRUCTURA, param));

                            //CARGO LA ESTRUCTURA PADRE
                            DDOESTR = NucleusRH.Base.Organizacion.Clases_Organizativas.ESTRUCTURA.Get(xmlestr.FirstChild().GetAttr("oi_estructura"), false);

                            //Se agrega la Unidad Organizativa de la Posicion al Organigrama
                            NucleusRH.Base.Organizacion.Clases_Organizativas.ESTRUCTURA DDOESTRNEW = new NucleusRH.Base.Organizacion.Clases_Organizativas.ESTRUCTURA();
                            DDOESTRNEW.oi_unidad_org = oiUNIORG;
                            DDOESTRNEW.oi_estr_padre = DDOESTR.id;
                            DDOESTR.ESTRUCTURAS.Add(DDOESTRNEW);

                            if (cpadre > 0 && xmlestr.FirstChild().GetAttr("oi_estructura") != idestrpadre)
                                idestrpadre = xmlestr.FirstChild().GetAttr("oi_estructura");
                            DDOESTRSAVE = DDOESTR;
                            cpadre++;
                        }
                        else
                        {
                            //La posicion no tiene padre, hay que revisar que el organigrama no tenga ya definida una posicion cabecera
                            DDOCLASE = NucleusRH.Base.Organizacion.Clases_Organizativas.CLASE_ORG.Get(oiCLASE, false);
                            NomadXML xmlflag;
                            string param = "<DATOS oi_estructura=\"" + DDOCLASE.oi_estructura_org.Id + "\"/>";
                            xmlflag = new NomadXML();
                            xmlflag.SetText(NomadEnvironment.QueryString(NucleusRH.Base.Migracion.Organizacion.Posiciones.POSICION.Resources.QRY_VALIDA_CABECERA, param));

                            //Si vuelve 1 ya existe una posicion cabecera en la clae con lo cual se rechaza el registro
                            if (xmlflag.FirstChild().GetAttr("flag") == "1")
                            {
                                objBatch.Err("La Posicion no puede ubicarse como Cabecera del Organigrama Jerarquico para la Empresa, se rechaza el registro '" + objRead.c_posicion + " - " + objRead.d_posicion + "' - Linea: " + Linea.ToString());
                                Errores++;
                                continue;
                            }
                            //Se agrega la Unidad Organizativa de la Posicion al Organigrama
                            NucleusRH.Base.Organizacion.Clases_Organizativas.ESTRUCTURA DDOESTRP = new NucleusRH.Base.Organizacion.Clases_Organizativas.ESTRUCTURA();
                            DDOESTRP.oi_unidad_org = oiUNIORG;
                            DDOESTRP.oi_estr_padre = DDOCLASE.oi_estructura_org.id;
                            DDOCLASE.oi_estructura_org.ESTRUCTURAS.Add(DDOESTRP);
                            NomadEnvironment.GetCurrentTransaction().Save(DDOCLASE);
                        }

                        if (cpadre > 0)
                        {
                            NomadEnvironment.GetCurrentTransaction().Save(DDOESTRSAVE);
                        }
                    }
                }
                catch (Exception e)
                {
                    objBatch.Err("Error desconocido en registro, por favor verifique el formato del archivo y la correcta asignacion de los campos - Linea: " + Linea.ToString() + " - " + e.Message);
                    Errores++;
                }
            }

            objBatch.Log("Registros Procesados: " + Linea.ToString() + " - Importados: " + (Linea - Errores).ToString());
            objBatch.Log("Finalizado...");

        }
    }
}



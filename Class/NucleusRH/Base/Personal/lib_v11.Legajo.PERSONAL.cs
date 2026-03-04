using System;
using System.Xml;
using System.IO;
using System.Collections;

using Nomad.NSystem.Document;
using Nomad.NSystem.Proxy;
using Nomad.NSystem.Base;
using Nomad.NSystem.Functions;
using System.Collections.Generic;

namespace NucleusRH.Base.Personal.Legajo
{

    //////////////////////////////////////////////////////////////////////////////////
    //Clase Persona
    public partial class PERSONAL
    {
        public void Cambio_AFJP(NucleusRH.Base.Personal.Legajo.AFJP_PER afjp)
        {
            // Verifica que haya una afjp anterior
            if (this.AFJP_PER.Count > 0)
            {
                NucleusRH.Base.Personal.Legajo.AFJP_PER afjp_per = (AFJP_PER)this.AFJP_PER[this.AFJP_PER.Count - 1];
                // Asigna f_egreso de AFJP anterior
                afjp_per.f_egreso = afjp.f_ingreso;

                // Asigna o_egreso_afjp de AFJP anterior
                afjp_per.o_egreso_afjp = afjp.o_egreso_afjp;
            }
            // Asigna oi_afjp, f_desde_afjp, c_nro_jubilacion a la persona
            this.oi_afjp = afjp.oi_afjp;
            this.f_desde_afjp = afjp.f_ingreso;
            this.c_nro_jubilacion = afjp.c_jubilacion;

            afjp.o_egreso_afjpNull = true;
            afjp.f_egresoNull = true;
            this.AFJP_PER.Add(afjp);
        }
        public void Ingreso_Personal()
        {
            this.Cambio_Documento();

            if (!this.oi_afjpNull)
            {
                NucleusRH.Base.Personal.Legajo.AFJP_PER afjp = new NucleusRH.Base.Personal.Legajo.AFJP_PER();
                afjp.oi_afjp = this.oi_afjp;
                if (!this.f_desde_afjpNull)
                {
                    afjp.f_ingreso = this.f_desde_afjp;
                }
                else
                {
                    afjp.f_ingreso = DateTime.Today;
                    this.f_desde_afjp = DateTime.Today;
                }
                this.AFJP_PER.Add(afjp);
            }
        }
        public void Cambio_Documento()
        {
            string oi_tipo_doc = this.oi_tipo_documento;
            NucleusRH.Base.Personal.Legajo.DOCUMENTO_PER documento = (DOCUMENTO_PER)this.DOCUM_PER.GetByAttribute("oi_tipo_documento", oi_tipo_doc.ToString());
            if (documento != null && !documento.IsForDelete)
            {
                documento.c_documento = this.c_nro_documento;
                this.c_personal = documento.Getoi_tipo_documento().c_tipo_documento + documento.c_documento;
            }
            else
            {
                //hay que crearlo
                documento = new NucleusRH.Base.Personal.Legajo.DOCUMENTO_PER();
                documento.c_documento = this.c_nro_documento;
                documento.oi_tipo_documento = this.oi_tipo_documento;
                this.DOCUM_PER.Add(documento);
            }
        }
        public static void Importar(string pstrPath, bool pbolReemplazar)
        {
            NucleusRH.Base.Personal_Fotos.clsImportarFotos objImportador;
            ArrayList arrExtensiones = new ArrayList();

            //Agrega las extensiones permitidas
            arrExtensiones.Add("jpg");

            objImportador = new NucleusRH.Base.Personal_Fotos.clsImportarFotos(NucleusRH.Base.Personal.Legajo.PERSONAL.Resources.QRY_EntradaFotos, pstrPath, (200 * 1024), arrExtensiones, "FOTO");
            objImportador.Importar(pbolReemplazar);
        }
        public void CargarAtributosAux(Nomad.NSystem.Proxy.NomadXML DATOS_AUX)
        {
            this.ATRIB_PER.Clear();
            foreach (NomadXML atrib in DATOS_AUX.FirstChild().GetChilds())
            {
                if (atrib.GetAttr("d_atrib_per") != "")
                {
                    ATRIB_PER new_atrib = new ATRIB_PER();
                    new_atrib.oi_atrib_aux = atrib.GetAttr("id");
                    new_atrib.d_atrib_per = atrib.GetAttr("d_atrib_per");
                    this.ATRIB_PER.Add(new_atrib);
                }
            }
        }

        public static SortedList<string, object> GetPer(string PAR)
        {
            NomadLog.Debug("--------------------------------------");
            NomadLog.Debug("------------GET PERSONAL--------------");
            NomadLog.Debug("--------------------------------------");

            NomadLog.Debug("GetPer.Usuario: " + PAR);

            SortedList<string, object> retorno = new SortedList<string, object>();
            SortedList<string, string> types = new SortedList<string, string>();
            string type = "";

            NomadXML param = new NomadXML("PARAM");

            //Agrego los parametros
            param.SetAttr("oi_usuario_sistema", PAR);

            //Ejecuto el recurso
            NomadXML resultado = NomadProxy.GetProxy().SQLService().GetXML("CLASS.NucleusRH.Base.Personal.Legajo.PERSONAL.DATOS_PER", param.ToString());
            NomadLog.Debug((resultado.FirstChild() == null ? "No se encontró el personal con el usuario: " + PAR + "." : "Personal encontrado."));

            if (resultado.FirstChild() != null)
            {
                //Armo una sorted list con los atributos del resultado y sus tipos
                for (int x = 0; x < resultado.Attrs.Count; x++)
                {
                    types.Add(resultado.Attrs[x].ToString(), resultado.GetAttr(resultado.Attrs[x].ToString()));
                }

                resultado = resultado.FirstChild();

                for (int r = 0; r < resultado.Attrs.Count; r++)
                {
                    //Busco de que tipo es el atributo
                    foreach (KeyValuePair<string, string> kvp in types)
                    {
                        if (kvp.Key == resultado.Attrs[r].ToString())
                        {
                            type = kvp.Value;
                            break;
                        }
                    }

                    //Agrego el atributo en base a su tipo
                    switch (type)
                    {
                        case "string":
                            retorno.Add(resultado.Attrs[r].ToString(), resultado.GetAttr(resultado.Attrs[r].ToString()));
                            break;
                        case "int":
                            retorno.Add(resultado.Attrs[r].ToString(), resultado.GetAttrInt(resultado.Attrs[r].ToString()));
                            break;
                        case "datetime":
                            retorno.Add(resultado.Attrs[r].ToString(), resultado.GetAttrDateTime(resultado.Attrs[r].ToString()));
                            break;
                        case "double":
                            retorno.Add(resultado.Attrs[r].ToString(), resultado.GetAttrDouble(resultado.Attrs[r].ToString()));
                            break;
                        case "bool":
                            retorno.Add(resultado.Attrs[r].ToString(), resultado.GetAttrBool(resultado.Attrs[r].ToString()));
                            break;
                        default:
                            retorno.Add(resultado.Attrs[r].ToString(), resultado.GetAttr(resultado.Attrs[r].ToString()));
                            break;
                    }

                    type = "";
                }
            }
            NomadLog.Debug("Retorno: " + retorno.ToString());
            return retorno;
        }

        public static List<SortedList<string, object>> GetFams(string PAR)
        {
            NomadLog.Debug("--------------------------------------");
            NomadLog.Debug("-----------GET FAMILIARES-------------");
            NomadLog.Debug("--------------------------------------");

            NomadLog.Debug("GetFams.Usuario: " + PAR);

            List<SortedList<string, object>> retorno = new List<SortedList<string, object>>();
            SortedList<string, object> row;
            SortedList<string, string> types = new SortedList<string, string>();
            string type = "";

            int linea;
            NomadXML MyROW;

            NomadXML param = new NomadXML("PARAM");

            //Agrego los parametros
            param.SetAttr("oi_usuario_sistema", PAR);

            //Ejecuto el recurso
            NomadXML resultado = NomadProxy.GetProxy().SQLService().GetXML("CLASS.NucleusRH.Base.Personal.Legajo.PERSONAL.GET_FAMS", param.ToString());
            NomadLog.Debug((resultado.FirstChild() == null ? "No se encontraron familiares para el personal con el usuario: " + PAR + "." : "Familiares encontrados " +  resultado.ChildLength + "."));

            if (resultado.FirstChild() != null)
            {
                //Armo una sorted list con los atributos del resultado y sus tipos
                for (int x = 0; x < resultado.Attrs.Count; x++)
                {
                    types.Add(resultado.Attrs[x].ToString(), resultado.GetAttr(resultado.Attrs[x].ToString()));
                }

                //resultado = resultado.FirstChild();
                for (linea = 1, MyROW = resultado.FirstChild(); MyROW != null; linea++, MyROW = MyROW.Next())
                {
                    row = new SortedList<string, object>();

                    for (int r = 0; r < MyROW.Attrs.Count; r++)
                    {
                        //Busco de que tipo es el atributo
                        foreach (KeyValuePair<string, string> kvp in types)
                        {
                            if (kvp.Key == MyROW.Attrs[r].ToString())
                            {
                                type = kvp.Value;
                                break;
                            }
                        }

                        //Agrego el atributo en base a su tipo
                        switch (type)
                        {
                            case "string":
                                row.Add(MyROW.Attrs[r].ToString(), MyROW.GetAttr(MyROW.Attrs[r].ToString()));
                                break;
                            case "int":
                                row.Add(MyROW.Attrs[r].ToString(), MyROW.GetAttrInt(MyROW.Attrs[r].ToString()));
                                break;
                            case "datetime":
                                row.Add(MyROW.Attrs[r].ToString(), MyROW.GetAttrDateTime(MyROW.Attrs[r].ToString()));
                                break;
                            case "double":
                                row.Add(MyROW.Attrs[r].ToString(), MyROW.GetAttrDouble(MyROW.Attrs[r].ToString()));
                                break;
                            case "bool":
                                row.Add(MyROW.Attrs[r].ToString(), MyROW.GetAttrBool(MyROW.Attrs[r].ToString()));
                                break;
                            default:
                                row.Add(MyROW.Attrs[r].ToString(), MyROW.GetAttr(MyROW.Attrs[r].ToString()));
                                break;
                        }
                        type = "";
                    }

                    //Agrego el familiar a la lista de resultados
                    retorno.Add(row);
                }
            }
            NomadLog.Debug("Retorno: " + retorno.ToString());
            return retorno;
        }

        public static SortedList<string, object> GetDatosFam(string PAR)
        {
            NomadLog.Debug("--------------------------------------");
            NomadLog.Debug("----------GET DATOS FAMILIAR----------");
            NomadLog.Debug("--------------------------------------");

            NomadLog.Debug("GetDatosFam.Id: " + PAR);

            SortedList<string, object> retorno = new SortedList<string, object>();
            SortedList<string, string> types = new SortedList<string, string>();
            string type = "";

            NomadXML param = new NomadXML("PARAM");

            //Agrego los parametros
            param.SetAttr("oi_familiar_per", PAR);

            //Ejecuto el recurso
            NomadXML resultado = NomadProxy.GetProxy().SQLService().GetXML("CLASS.NucleusRH.Base.Personal.Legajo.PERSONAL.DATOS_FAM", param.ToString());
            NomadLog.Debug((resultado.FirstChild() == null ? "No se encontró el familiar con id: " + PAR + "." : "Familiar encontrado."));

            if (resultado.FirstChild() != null)
            {
                //Armo una sorted list con los atributos del resultado y sus tipos
                for (int x = 0; x < resultado.Attrs.Count; x++)
                {
                    types.Add(resultado.Attrs[x].ToString(), resultado.GetAttr(resultado.Attrs[x].ToString()));
                }

                resultado = resultado.FirstChild();

                for (int r = 0; r < resultado.Attrs.Count; r++)
                {
                    //Busco de que tipo es el atributo
                    foreach (KeyValuePair<string, string> kvp in types)
                    {
                        if (kvp.Key == resultado.Attrs[r].ToString())
                        {
                            type = kvp.Value;
                            break;
                        }
                    }

                    //Agrego el atributo en base a su tipo
                    switch (type)
                    {
                        case "string":
                            retorno.Add(resultado.Attrs[r].ToString(), resultado.GetAttr(resultado.Attrs[r].ToString()));
                            break;
                        case "int":
                            retorno.Add(resultado.Attrs[r].ToString(), resultado.GetAttrInt(resultado.Attrs[r].ToString()));
                            break;
                        case "datetime":
                            retorno.Add(resultado.Attrs[r].ToString(), resultado.GetAttrDateTime(resultado.Attrs[r].ToString()));
                            break;
                        case "double":
                            retorno.Add(resultado.Attrs[r].ToString(), resultado.GetAttrDouble(resultado.Attrs[r].ToString()));
                            break;
                        case "bool":
                            retorno.Add(resultado.Attrs[r].ToString(), resultado.GetAttrBool(resultado.Attrs[r].ToString()));
                            break;
                        default:
                            retorno.Add(resultado.Attrs[r].ToString(), resultado.GetAttr(resultado.Attrs[r].ToString()));
                            break;
                    }
                    type = "";
                }
            }
            NomadLog.Debug("Retorno: " + retorno.ToString());
            return retorno;
        }

        public static System.Collections.Generic.SortedList<string, string> EditPer(string PAR, string TIPOEDIT, System.Collections.Generic.SortedList<string, object> DATA1, System.Collections.Generic.SortedList<string, object> DATA2, System.Collections.Generic.SortedList<string, object> DATA3, System.Collections.Generic.SortedList<string, object> DATA4)
        {
            #region DEBUG
            NomadLog.Debug("--------------------------------------");
            NomadLog.Debug("-----------EDIT PERSONAL-------------");
            NomadLog.Debug("--------------------------------------");

            foreach (KeyValuePair<string, object> kvp in DATA1) {
                if (kvp.Value != null) { NomadLog.Debug(kvp.Key.ToString() + ":" + kvp.Value.ToString()); }
                else { NomadLog.Debug(kvp.Key.ToString() + ":Null"); }
            }
            foreach (KeyValuePair<string, object> kvp in DATA2)
            {
                if (kvp.Value != null) { NomadLog.Debug(kvp.Key.ToString() + ":" + kvp.Value.ToString()); }
                else { NomadLog.Debug(kvp.Key.ToString() + ":Null"); }
            }
            foreach (KeyValuePair<string, object> kvp in DATA3)
            {
                if (kvp.Value != null) { NomadLog.Debug(kvp.Key.ToString() + ":" + kvp.Value.ToString()); }
                else { NomadLog.Debug(kvp.Key.ToString() + ":Null"); }
            }
            foreach (KeyValuePair<string, object> kvp in DATA4)
            {
                if (kvp.Value != null) { NomadLog.Debug(kvp.Key.ToString() + ":" + kvp.Value.ToString()); }
                else { NomadLog.Debug(kvp.Key.ToString() + ":Null"); }
            }

            NomadLog.Debug("EditPer.oi_personal: " + PAR);
            NomadLog.Debug("TIPOEDIT: " + TIPOEDIT);

            #endregion

            SortedList<string, string> retorno = new SortedList<string, string>();
            retorno.Add("VAL", "FATALERR");
            retorno.Add("DES", "El método no fue procesado");

            try
            {
                //Get PERSONAL
                NucleusRH.Base.Personal.Legajo.PERSONAL PER;
                PER = NucleusRH.Base.Personal.Legajo.PERSONAL.Get(PAR);

                if (PER == null) {
                   retorno =  NucleusRH.Base.Configuracion.Herramientas.QUERIES.CreateRTA("FATALERR", "La persona con oi_personal " + PAR + "no se puede encontrar");
                   return retorno;
                }
                
                if (TIPOEDIT == null){
                    retorno = NucleusRH.Base.Configuracion.Herramientas.QUERIES.CreateRTA("FATALERR", "El parámetro TIPOEDIT no contiene valores");
                    return retorno;
                }

                if (TIPOEDIT == "PERSONALES") {
                    foreach (KeyValuePair<string, object> kvp in DATA1)
                    {
                        switch (kvp.Key)
                        {
                            case "APE":
                                if (kvp.Value != null)
                                    PER.d_apellido = kvp.Value.ToString();
                                break;
                            case "NOM":
                                if (kvp.Value != null)
                                    PER.d_nombres = kvp.Value.ToString();
                                break;
                            case "APE_MAT":
                                if (kvp.Value != null) { PER.d_apellido_materno = kvp.Value.ToString(); break; }
                                else { PER.d_apellido_maternoNull = true; break; }
                            case "NRODOC":
                                if (kvp.Value != null)
                                    PER.c_nro_documento = kvp.Value.ToString();
                                break;
                            case "CUIT":
                                if (kvp.Value != null) { PER.c_nro_cuil = kvp.Value.ToString(); break; }
                                else { PER.c_nro_cuilNull = true; break; }
                            case "TIPODOC":
                                if (kvp.Value != null)
                                    PER.oi_tipo_documento = kvp.Value.ToString();
                                break;
                            case "GRU_SAN":
                                if (kvp.Value != null && kvp.Value.ToString() != "0") { PER.oi_grupo_sanguineo = kvp.Value.ToString(); break; }
                                else { PER.oi_grupo_sanguineoNull = true; break; }
                            case "NIV_EST":
                                if (kvp.Value != null && kvp.Value.ToString() != "0") { PER.oi_nivel_estudio = kvp.Value.ToString(); break; }
                                else { PER.oi_nivel_estudioNull = true; break; }
                            case "TIT":
                                if (kvp.Value != null && kvp.Value.ToString() != "0") { PER.oi_titulo = kvp.Value.ToString(); break; }
                                else { PER.oi_tituloNull = true; break; }
                            case "MAIL":
                                if (kvp.Value != null) { PER.d_email = kvp.Value.ToString(); break; }
                                else { PER.d_emailNull = true; break; }
                        }
                    }

                    foreach (KeyValuePair<string, object> kvp in DATA2)
                    {
                        switch (kvp.Key)
                        {
                            case "CEL":
                                if (kvp.Value != null) { PER.te_celular = kvp.Value.ToString(); break; }
                                else { PER.te_celularNull = true; break; }
                            case "SEXO":
                                if (kvp.Value != null)
                                    PER.c_sexo = kvp.Value.ToString();
                                break;
                        }
                    }

                    PER.d_ape_y_nom = PER.d_apellido + ", " + PER.d_nombres;
                }

                if (TIPOEDIT == "REFERENCIALES")
                {
                    foreach (KeyValuePair<string, object> kvp in DATA3)
                    {
                        switch (kvp.Key)
                        {
                            case "F_NAC":
                                if (kvp.Value != null) { PER.f_nacim = (DateTime)kvp.Value; break; }
                                else { PER.f_nacimNull = true; break; }
                            case "LOC_NAC":
                                if (kvp.Value != null && kvp.Value.ToString() != "0") { PER.oi_local_nacim = kvp.Value.ToString(); break; }
                                else { PER.oi_local_nacimNull = true; break; }
                            case "NAC":
                                if (kvp.Value != null && kvp.Value.ToString() != "0") { PER.oi_nacionalidad = kvp.Value.ToString(); break; }
                                else { PER.oi_nacionalidadNull = true; break; }
                            case "IDIO":
                                if (kvp.Value != null && kvp.Value.ToString() != "0") { PER.oi_idioma = kvp.Value.ToString(); break; }
                                else { PER.oi_idiomaNull = true; break; }
                            case "EST_CIV":
                                if (kvp.Value != null && kvp.Value.ToString() != "0") { PER.oi_estado_civil = kvp.Value.ToString(); break; }
                                else { PER.oi_estado_civilNull = true; break; }
                            case "F_CAS":
                                if (kvp.Value != null) { PER.f_casamiento = (DateTime)kvp.Value; break; }
                                else { PER.f_casamientoNull = true; break; }
                            case "F_ING_PAIS":
                                if (kvp.Value != null) { PER.f_ingreso_pais = (DateTime)kvp.Value; break; }
                                else { PER.f_ingreso_paisNull = true; break; }
                            case "LOC_RES":
                                if (kvp.Value != null && kvp.Value.ToString() != "0") { PER.oi_localidad = kvp.Value.ToString(); break; }
                                else { PER.oi_localidadNull = true; break; }
                        }
                    }
                }

                if (TIPOEDIT == "ADICIONALES")
                {
                    foreach (KeyValuePair<string, object> kvp in DATA4)
                    {
                        switch (kvp.Key)
                        {
                            case "T_CAM":
                                if (kvp.Value != null) { PER.c_nro_camisa = kvp.Value.ToString(); break; }
                                else { PER.c_nro_camisaNull = true; break; }
                            case "T_PANT":
                                if (kvp.Value != null) { PER.c_nro_pantalon = kvp.Value.ToString(); break; }
                                else { PER.c_nro_pantalonNull = true; break; }
                            case "T_CALZ":
                                if (kvp.Value != null) { PER.c_nro_calzado = kvp.Value.ToString(); break; }
                                else { PER.c_nro_calzadoNull = true; break; }
                            case "T_CHOM":
                                if (kvp.Value != null) { PER.c_nro_chomba = kvp.Value.ToString(); break; }
                                else { PER.c_nro_chombaNull = true; break; }
                            case "T_BUZO":
                                if (kvp.Value != null) { PER.c_nro_buzo = kvp.Value.ToString(); break; }
                                else { PER.c_nro_buzoNull = true; break; }
                            case "T_CAMP":
                                if (kvp.Value != null) { PER.c_nro_campera = kvp.Value.ToString(); break; }
                                else { PER.c_nro_camperaNull = true; break; }
                        }
                    }
                }

                NomadEnvironment.GetCurrentTransaction().Save(PER);
                retorno = NucleusRH.Base.Configuracion.Herramientas.QUERIES.CreateRTA("OK", "Modificación de los datos " + TIPOEDIT + " exitosa");
                return retorno;
            }
            catch (Exception ex)
            {
                NomadLog.Debug("Error guardando PERSONAL: " + ex);
                retorno = NucleusRH.Base.Configuracion.Herramientas.QUERIES.CreateRTA("FATALERR", ex.Message);
                return retorno;
            }

        }

        public static SortedList<string, string> EditFam(string PAR, SortedList<string, object> DATA1, SortedList<string, object> DATA2, SortedList<string, object> DATA3, SortedList<string, object> DATA4, SortedList<string, object> DATA5, SortedList<string, object> DATA6)
        {
            #region NomadLog.Debug
            NomadLog.Debug("-----------------------------------");
            NomadLog.Debug("-----------EDIT FAMILIAR-----------");
            NomadLog.Debug("-----------------------------------");

            foreach (KeyValuePair<string, object> kvp in DATA1)
            {
                if (kvp.Value != null) { NomadLog.Debug(kvp.Key.ToString() + ":" + kvp.Value.ToString()); }
                else { NomadLog.Debug(kvp.Key.ToString() + ":Null"); }
            }
            foreach (KeyValuePair<string, object> kvp in DATA2)
            {
                if (kvp.Value != null) { NomadLog.Debug(kvp.Key.ToString() + ":" + kvp.Value.ToString()); }
                else { NomadLog.Debug(kvp.Key.ToString() + ":Null"); }
            }
            foreach (KeyValuePair<string, object> kvp in DATA3)
            {
                if (kvp.Value != null) { NomadLog.Debug(kvp.Key.ToString() + ":" + kvp.Value.ToString()); }
                else { NomadLog.Debug(kvp.Key.ToString() + ":Null"); }
            }
            foreach (KeyValuePair<string, object> kvp in DATA4)
            {
                if (kvp.Value != null) { NomadLog.Debug(kvp.Key.ToString() + ":" + kvp.Value.ToString()); }
                else { NomadLog.Debug(kvp.Key.ToString() + ":Null"); }
            }
            foreach (KeyValuePair<string, object> kvp in DATA5)
            {
                if (kvp.Value != null) { NomadLog.Debug(kvp.Key.ToString() + ":" + kvp.Value.ToString()); }
                else { NomadLog.Debug(kvp.Key.ToString() + ":Null"); }
            }
            foreach (KeyValuePair<string, object> kvp in DATA6)
            {
                if (kvp.Value != null) { NomadLog.Debug(kvp.Key.ToString() + ":" + kvp.Value.ToString()); }
                else { NomadLog.Debug(kvp.Key.ToString() + ":Null"); }
            }
            
            NomadLog.Debug("EditFam.oi_familiar: " + PAR);
            #endregion

            SortedList<string, string> retorno = new SortedList<string, string>();
            retorno.Add("VAL", "FATALERR");
            retorno.Add("DES", "El método no fue procesado");

            //Get FAMILIAR
            NucleusRH.Base.Personal.Legajo.FAMILIAR_PER FAM;
            FAM = NucleusRH.Base.Personal.Legajo.FAMILIAR_PER.Get(PAR);

            if (FAM == null) {
                retorno = NucleusRH.Base.Configuracion.Herramientas.QUERIES.CreateRTA("FATALERR", "El familiar con oi_familiar " + PAR + "no se puede encontrar");
                return retorno;
            }

            NomadLog.Debug("DATA1");
            foreach (KeyValuePair<string, object> kvp in DATA1)
            {
                switch (kvp.Key)
                {
                    case "TIPOFAM":
                        if (kvp.Value != null)
                            FAM.oi_tipo_familiar = kvp.Value.ToString();
                        break;
                    case "APEL":
                        if (kvp.Value != null)
                            FAM.d_apellido = kvp.Value.ToString();
                        break;
                    case "NOMB":
                        if (kvp.Value != null)
                            FAM.d_nombres = kvp.Value.ToString();
                        break;
                    case "NRODOC":
                        if (kvp.Value != null)
                            FAM.c_nro_documento = kvp.Value.ToString();
                        break;
                    case "TIPODOC":
                        if (kvp.Value != null)
                            FAM.oi_tipo_documento = kvp.Value.ToString();
                        break;
                    case "VIVE":
                        if (kvp.Value != null)
                            FAM.l_vive = (bool)kvp.Value;
                        break;
                    case "DISC":
                        if (kvp.Value != null)
                            FAM.l_discapacidad = (bool)kvp.Value;
                        break;
                    case "OCUP":
                        if (kvp.Value != null && kvp.Value.ToString() != "0") { FAM.oi_ocupacion_fam = (string)kvp.Value; break; }
                        else { FAM.oi_ocupacion_famNull = true; break; }
                    case "OBS_OCUP":
                        if (kvp.Value != null) { FAM.o_ocupacion = (string)kvp.Value; break; }
                        else { FAM.o_ocupacionNull = true; break; }
                    case "F_NAC":
                        if (kvp.Value != null) { FAM.f_nacimiento = (DateTime)kvp.Value; break; }
                        else { FAM.f_nacimientoNull = true; break; }
                    case "FEC_FAL":
                        if (kvp.Value != null) { FAM.f_fallecimiento = (DateTime)kvp.Value; break; }
                        else { FAM.f_fallecimientoNull = true; break; }
                }
            }

            NomadLog.Debug("DATA2");
            foreach (KeyValuePair<string, object> kvp in DATA2)
            {
                switch (kvp.Key)
                {
                    case "NIV_ESC":
                        if (kvp.Value != null && kvp.Value.ToString() != "0") { FAM.oi_nivel_escol = (string)kvp.Value; break; }
                        else { FAM.oi_nivel_escolNull = true; break; }
                    case "GRA_ESC":
                        if (kvp.Value != null && kvp.Value.ToString() != "0") { FAM.oi_grado_escol = (string)kvp.Value; break; }
                        else { FAM.oi_grado_escolNull = true; break; }
                    case "EST":
                        if (kvp.Value != null && kvp.Value.ToString() != "0") { FAM.oi_estudio = (string)kvp.Value; break; }
                        else { FAM.oi_estudioNull = true; break; }
                    case "DURAC":
                        if (kvp.Value != null) { FAM.e_duracion_estudio = Convert.ToInt32(kvp.Value); break; }
                        else { FAM.e_duracion_estudioNull = true; break; }
                }
            }

            NomadLog.Debug("DATA3");
            foreach (KeyValuePair<string, object> kvp in DATA3)
            {
                switch (kvp.Key)
                {
                    case "UT":
                        if (kvp.Value != null && kvp.Value.ToString() != "0") { FAM.oi_unidad_tiempo = (string)kvp.Value; break; }
                        else { FAM.oi_unidad_tiempoNull = true; break; }
                    case "PER_CUR":
                        if (kvp.Value != null) { FAM.e_periodo_en_curso = Convert.ToInt32(kvp.Value); break; }
                        else { FAM.e_periodo_en_cursoNull = true; break; }
                    case "ANIO_INI":
                        if (kvp.Value != null) { FAM.e_anio_inic_esc = Convert.ToInt32(kvp.Value); break; }
                        else { FAM.e_anio_inic_escNull = true; break; }
                    case "ANIO_FIN":
                        if (kvp.Value != null) { FAM.e_anio_fin_esc = Convert.ToInt32(kvp.Value); break; }
                        else { FAM.e_anio_fin_escNull = true; break; }
                    case "CUIL":
                        if (kvp.Value != null) { FAM.c_nro_cuil = (string)kvp.Value; break; }
                        else { FAM.c_nro_cuilNull = true; break; }
                    case "EST_CIV":
                        if (kvp.Value != null && kvp.Value.ToString() != "0") { FAM.oi_estado_civil = (string)kvp.Value; break; }
                        else { FAM.oi_estado_civilNull = true; break; }
                }
            }

            NomadLog.Debug("DATA4");
            foreach (KeyValuePair<string, object> kvp in DATA4)
            {
                switch (kvp.Key)
                {
                    case "NAC":
                        if (kvp.Value != null && kvp.Value.ToString() != "0") { FAM.oi_nacionalidad = (string)kvp.Value; break; }
                        else { FAM.oi_nacionalidadNull = true; break; }
                    case "LOC":
                        if (kvp.Value != null && kvp.Value.ToString() != "0") { FAM.oi_localidad_nac = (string)kvp.Value; break; }
                        else { FAM.oi_localidad_nacNull = true; break; }
                    case "SEXO":
                        if (kvp.Value != null) { FAM.c_sexo = (string)kvp.Value; break; }
                        //else { FAM.c_sexoNull = true; break; }
                        else { FAM.c_sexo = ""; break; }
                    case "RES_PAIS":
                        if (kvp.Value != null)
                            FAM.l_reside_pais = (bool)kvp.Value;
                        break;
                }
            }

            NomadLog.Debug("DATA5");
            foreach (KeyValuePair<string, object> kvp in DATA5)
            {
                switch (kvp.Key)
                {
                    case "CARG_AF":
                        if (kvp.Value != null)
                            FAM.l_acargo_af = (bool)kvp.Value;
                        break;
                    case "O_AF":
                        if (kvp.Value != null) { FAM.o_acargo_af = (string)kvp.Value; break; }
                        else { FAM.o_acargo_afNull = true; break; }
                    case "A_OS":
                        if (kvp.Value != null)
                            FAM.l_acargo_os = (bool)kvp.Value;
                        break;
                    case "O_OS":
                        if (kvp.Value != null) { FAM.o_acargo_os = (string)kvp.Value; break; }
                        else { FAM.o_acargo_osNull = true; break; }
                    case "A_IG":
                        if (kvp.Value != null)
                            FAM.l_acargo_IG = (bool)kvp.Value;
                        break;
                }
            }

            NomadLog.Debug("DATA6");
            foreach (KeyValuePair<string, object> kvp in DATA6)
            {
                switch (kvp.Key)
                {
                    case "O_IG":
                        if (kvp.Value != null) { FAM.o_acargo_ig = (string)kvp.Value; break; }
                        else { FAM.o_acargo_igNull = true; break; }
                    case "F_DES_IG":
                        if (kvp.Value != null) { FAM.f_desde_IG = (DateTime)kvp.Value; break; }
                        else { FAM.f_desde_IGNull = true; break; }
                    case "F_HAST_IG":
                        if (kvp.Value != null) { FAM.f_hasta_IG = (DateTime)kvp.Value; break; }
                        else { FAM.f_hasta_IGNull = true; break; }
                    case "OBS":
                        if (kvp.Value != null) { FAM.o_familiar = (string)kvp.Value; break; }
                        else { FAM.o_familiarNull = true; break; }
                    case "PAR":
                        if (kvp.Value != null)
                            FAM.d_parentesco = (string)kvp.Value;
                        break;
                    case "A_SEG_VID":
                        if (kvp.Value != null)
                            FAM.l_acargo_SV = (bool)kvp.Value;
                        break;
                    case "PORC":
                        if (kvp.Value != null) { FAM.n_porc_seguro = (double)kvp.Value; break; }
                        else { FAM.n_porc_seguroNull = true; break; }
                }
            }

            FAM.d_ape_y_nom = FAM.d_apellido + ", " + FAM.d_nombres;
            NomadLog.Debug("GUARDAR");
            try
            {
                NomadEnvironment.GetCurrentTransaction().SaveRefresh(FAM);
                retorno = NucleusRH.Base.Configuracion.Herramientas.QUERIES.CreateRTA("OK", "Los datos de familiar se modificaron exitosamente");
                return retorno;
            }
            catch (Exception ex)
            {
                NomadLog.Debug("Error guardando FAMILIAR: " + ex);
                retorno = NucleusRH.Base.Configuracion.Herramientas.QUERIES.CreateRTA("FATALERR", ex.Message);
                return retorno;
            }

        }

        public static SortedList<string, string> DelFam(string PAR)
        {
            NomadLog.Debug("--------------------------------------");
            NomadLog.Debug("-----------DELETE FAMILIAR-------------");
            NomadLog.Debug("--------------------------------------");

            NomadLog.Debug("DelFam.oi_familiar: " + PAR);

            SortedList<string, string> retorno = new SortedList<string, string>();
            retorno.Add("VAL", "FATALERR");
            retorno.Add("DES", "El método no fue procesado");

            //Get FAMILIAR
            NucleusRH.Base.Personal.Legajo.FAMILIAR_PER FAM;
            FAM = NucleusRH.Base.Personal.Legajo.FAMILIAR_PER.Get(PAR, true);

            if (FAM == null) 
            {
                retorno = NucleusRH.Base.Configuracion.Herramientas.QUERIES.CreateRTA("FATALERR", "El familiar con oi_familiar " + PAR + "no se puede encontrar");
                return retorno;
            }

            try
            {
                NomadEnvironment.GetCurrentTransaction().Delete(FAM);
                retorno = NucleusRH.Base.Configuracion.Herramientas.QUERIES.CreateRTA("OK", "Los datos de familiar se modificaron exitosamente");
                return retorno;
            }
            catch (Exception ex)
            {
                NomadLog.Debug("Error eliminando FAMILIAR: " + ex);
                retorno = NucleusRH.Base.Configuracion.Herramientas.QUERIES.CreateRTA("FATALERR", ex.Message);
                return retorno;
            }
        }

        public static System.Collections.Generic.SortedList<string, string> AddFam(string PER, System.Collections.Generic.SortedList<string, object> DATA1, System.Collections.Generic.SortedList<string, object> DATA2, System.Collections.Generic.SortedList<string, object> DATA3, System.Collections.Generic.SortedList<string, object> DATA4, System.Collections.Generic.SortedList<string, object> DATA5, System.Collections.Generic.SortedList<string, object> DATA6)
        {
            #region NomadLog.Debug
            NomadLog.Debug("--------------------------------------");
            NomadLog.Debug("-----------AGREGAR FAMILIAR-----------");
            NomadLog.Debug("--------------------------------------");
            
            foreach (KeyValuePair<string, object> kvp in DATA1)
            {
                if (kvp.Value != null) { NomadLog.Debug(kvp.Key.ToString() + ":" + kvp.Value.ToString()); }
                else { NomadLog.Debug(kvp.Key.ToString() + ":Null"); }
            }
            foreach (KeyValuePair<string, object> kvp in DATA2)
            {
                if (kvp.Value != null) { NomadLog.Debug(kvp.Key.ToString() + ":" + kvp.Value.ToString()); }
                else { NomadLog.Debug(kvp.Key.ToString() + ":Null"); }
            }
            foreach (KeyValuePair<string, object> kvp in DATA3)
            {
                if (kvp.Value != null) { NomadLog.Debug(kvp.Key.ToString() + ":" + kvp.Value.ToString()); }
                else { NomadLog.Debug(kvp.Key.ToString() + ":Null"); }
            }
            foreach (KeyValuePair<string, object> kvp in DATA4)
            {
                if (kvp.Value != null) { NomadLog.Debug(kvp.Key.ToString() + ":" + kvp.Value.ToString()); }
                else { NomadLog.Debug(kvp.Key.ToString() + ":Null"); }
            }
            foreach (KeyValuePair<string, object> kvp in DATA5)
            {
                if (kvp.Value != null) { NomadLog.Debug(kvp.Key.ToString() + ":" + kvp.Value.ToString()); }
                else { NomadLog.Debug(kvp.Key.ToString() + ":Null"); }
            }
            foreach (KeyValuePair<string, object> kvp in DATA6)
            {
                if (kvp.Value != null) { NomadLog.Debug(kvp.Key.ToString() + ":" + kvp.Value.ToString()); }
                else { NomadLog.Debug(kvp.Key.ToString() + ":Null"); }
            }

            NomadLog.Debug("AddFam.oi_personal:" + PER);
            #endregion

            SortedList<string, string> retorno = new SortedList<string, string>();
            retorno.Add("VAL", "FATALERR");
            retorno.Add("DES", "El método no fue procesado");

            try
            {
                //Get PERSONAL
                NucleusRH.Base.Personal.Legajo.FAMILIAR_PER FAM = new NucleusRH.Base.Personal.Legajo.FAMILIAR_PER();
                NucleusRH.Base.Personal.Legajo.PERSONAL PERSONAL;
           
                PERSONAL = NucleusRH.Base.Personal.Legajo.PERSONAL.Get(PER,true);

                if (PERSONAL == null) {
                    retorno = NucleusRH.Base.Configuracion.Herramientas.QUERIES.CreateRTA("FATALERR", "La persona con oi_personal " + PER + "no se puede encontrar");
                    return retorno;
                } 
            
                //Validar que el familiar no esté agregado ya
                if (DATA1["NRODOC"] != null) {
                    string dni_familiar = DATA1["NRODOC"].ToString();
                    
                    if (dni_familiar != "" && dni_familiar != null && PERSONAL.FLIARES_PER.Count > 0)
                    {
                        foreach (FAMILIAR_PER fam in PERSONAL.FLIARES_PER)
                        {
                            if (fam.c_nro_documento == dni_familiar) {
                                retorno = NucleusRH.Base.Configuracion.Herramientas.QUERIES.CreateRTA("ERR", "El familiar con DNI " + dni_familiar + " que quiere agregar ya está cargado.");
                                return retorno;
                            }
                        }
                    }
                
                }

                foreach (KeyValuePair<string, object> kvp in DATA1)
                {
                    switch (kvp.Key)
                    {
                        case "TIPOFAM":
                            if (kvp.Value != null)
                                FAM.oi_tipo_familiar = kvp.Value.ToString();
                            break;
                        case "APEL":
                            if (kvp.Value != null)
                                FAM.d_apellido = kvp.Value.ToString();
                            break;
                        case "NOMB":
                            if (kvp.Value != null)
                                FAM.d_nombres = kvp.Value.ToString();
                            break;
                        case "NRODOC":
                            if (kvp.Value != null)
                                FAM.c_nro_documento = kvp.Value.ToString();
                            break;
                        case "TIPODOC":
                            if (kvp.Value != null)
                                FAM.oi_tipo_documento = kvp.Value.ToString();
                            break;
                        case "VIVE": 
                            if (kvp.Value != null)
                                FAM.l_vive = (bool)kvp.Value;
                            break;
                        case "DISC":
                            if (kvp.Value != null)
                                FAM.l_discapacidad = (bool)kvp.Value;
                            break;
                        case "OCUP":
                            if (kvp.Value != null && kvp.Value.ToString() != "0") { FAM.oi_ocupacion_fam = (string)kvp.Value; break; }
                            else { FAM.oi_ocupacion_famNull = true; break; }
                        case "OBS_OCUP":
                            if (kvp.Value != null) { FAM.o_ocupacion = (string)kvp.Value; break; }
                            else { FAM.o_ocupacionNull = true; break; }
                        case "F_NAC":
                            if (kvp.Value != null) { FAM.f_nacimiento = (DateTime)kvp.Value; break; }
                            else { FAM.f_nacimientoNull = true; break; }
                        case "FEC_FAL":
                            if (kvp.Value != null) { FAM.f_fallecimiento = (DateTime)kvp.Value; break; }
                            else { FAM.f_fallecimientoNull = true; break; }
                    }
                }

                foreach (KeyValuePair<string, object> kvp in DATA2)
                {
                    switch (kvp.Key)
                    {
                        case "NIV_ESC":
                            if (kvp.Value != null && kvp.Value.ToString() != "0") { FAM.oi_nivel_escol = (string)kvp.Value; break; }
                            else { FAM.oi_nivel_escolNull = true; break; }
                        case "GRA_ESC":
                            if (kvp.Value != null && kvp.Value.ToString() != "0") { FAM.oi_grado_escol = (string)kvp.Value; break; }
                            else { FAM.oi_grado_escolNull = true; break; }
                        case "EST":
                            if (kvp.Value != null && kvp.Value.ToString() != "0") { FAM.oi_estudio = (string)kvp.Value; break; }
                            else { FAM.oi_estudioNull = true; break; }
                        case "DURAC":
                            if (kvp.Value != null) { FAM.e_duracion_estudio = Convert.ToInt32(kvp.Value); break; }
                            else { FAM.e_duracion_estudioNull = true; break; }
                    }
                }

                foreach (KeyValuePair<string, object> kvp in DATA3)
                {
                    switch (kvp.Key)
                    {
                        case "UT":
                            if (kvp.Value != null && kvp.Value.ToString() != "0") { FAM.oi_unidad_tiempo = (string)kvp.Value; break; }
                            else { FAM.oi_unidad_tiempoNull = true; break; }
                        case "PER_CUR":
                            if (kvp.Value != null) { FAM.e_periodo_en_curso = Convert.ToInt32(kvp.Value); break; }
                            else { FAM.e_periodo_en_cursoNull = true; break; }
                        case "ANIO_INI":
                            if (kvp.Value != null) { FAM.e_anio_inic_esc = Convert.ToInt32(kvp.Value); break; }
                            else { FAM.e_anio_inic_escNull = true; break; }
                        case "ANIO_FIN":
                            if (kvp.Value != null) { FAM.e_anio_fin_esc = Convert.ToInt32(kvp.Value); break; }
                            else { FAM.e_anio_fin_escNull = true; break; }
                        case "CUIL":
                            if (kvp.Value != null) { FAM.c_nro_cuil = (string)kvp.Value; break; }
                            else { FAM.c_nro_cuilNull = true; break; }
                        case "EST_CIV":
                            if (kvp.Value != null && kvp.Value.ToString() != "0") { FAM.oi_estado_civil = (string)kvp.Value; break; }
                            else { FAM.oi_estado_civilNull = true; break; }
                    }
                }

                foreach (KeyValuePair<string, object> kvp in DATA4)
                {
                    switch (kvp.Key)
                    {
                        case "NAC":
                            if (kvp.Value != null && kvp.Value.ToString() != "0") { FAM.oi_nacionalidad = (string)kvp.Value; break; }
                            else { FAM.oi_nacionalidadNull = true; break; }
                        case "LOC":
                            if (kvp.Value != null && kvp.Value.ToString() != "0") { FAM.oi_localidad_nac = (string)kvp.Value; break; }
                            else { FAM.oi_localidad_nacNull = true; break; }
                        case "SEXO":
                            if (kvp.Value != null) { FAM.c_sexo = (string)kvp.Value; break; }
                            //else { FAM.c_sexoNull = true; break; }
                            else { FAM.c_sexo = ""; break; }
                        case "RES_PAIS":
                            if (kvp.Value != null)
                                FAM.l_reside_pais = (bool)kvp.Value;
                            break; 
                    }
                }

                foreach (KeyValuePair<string, object> kvp in DATA5)
                {
                    switch (kvp.Key)
                    {
                        case "CARG_AF":
                            if (kvp.Value != null)
                                FAM.l_acargo_af = (bool)kvp.Value; 
                            break;
                        case "O_AF":
                            if (kvp.Value != null) { FAM.o_acargo_af = (string)kvp.Value; break; }
                            else { FAM.o_acargo_afNull = true; break; }
                        case "A_OS":
                            if (kvp.Value != null)
                                FAM.l_acargo_os = (bool)kvp.Value;
                            break;
                        case "O_OS":
                            if (kvp.Value != null) { FAM.o_acargo_os = (string)kvp.Value; break; }
                            else { FAM.o_acargo_osNull = true; break; }
                        case "A_IG":
                            if (kvp.Value != null)
                                FAM.l_acargo_IG = (bool)kvp.Value; 
                            break;
                    }
                }

                foreach (KeyValuePair<string, object> kvp in DATA6)
                {
                    switch (kvp.Key)
                    {
                        case "O_IG":
                            if (kvp.Value != null) { FAM.o_acargo_ig = (string)kvp.Value; break; }
                            else { FAM.o_acargo_igNull = true; break; }
                        case "F_DES_IG":
                            if (kvp.Value != null) { FAM.f_desde_IG = (DateTime)kvp.Value; break; }
                            else { FAM.f_desde_IGNull = true; break; }
                        case "F_HAST_IG":
                            if (kvp.Value != null) { FAM.f_hasta_IG = (DateTime)kvp.Value; break; }
                            else { FAM.f_hasta_IGNull = true; break; }
                        case "OBS":
                            if (kvp.Value != null) { FAM.o_familiar = (string)kvp.Value; break; }
                            else { FAM.o_familiarNull = true; break; }
                        case "PAR":
                            if (kvp.Value != null)
                                FAM.d_parentesco = (string)kvp.Value; 
                            break;
                        case "A_SEG_VID":
                            if (kvp.Value != null)
                                FAM.l_acargo_SV = (bool)kvp.Value;
                            break;
                        case "PORC":
                            if (kvp.Value != null) { FAM.n_porc_seguro = (double)kvp.Value; break; }
                            else { FAM.n_porc_seguroNull = true; break; }
                    }
                }

                FAM.d_ape_y_nom = FAM.d_apellido + ", " + FAM.d_nombres;
                PERSONAL.FLIARES_PER.Add(FAM);

                NomadEnvironment.GetCurrentTransaction().Save(PERSONAL);
                retorno = NucleusRH.Base.Configuracion.Herramientas.QUERIES.CreateRTA("OK", "El familiar se agregó exitosamente");
                return retorno;
            }
            catch (Exception ex)
            {
                retorno = NucleusRH.Base.Configuracion.Herramientas.QUERIES.CreateRTA("FATALERR", ex.Message);
                return retorno;
            }

        }

        public static System.Collections.Generic.SortedList<string,object> GetEstudioPer( int PAR)
        {
            NomadLog.Debug("-----------------------------------------");
            NomadLog.Debug("----------GET DATOS ESTUDIO PER----------");
            NomadLog.Debug("-----------------------------------------");

            NomadLog.Debug("oi_estudio_per.Id: " + PAR);

            SortedList<string, object> retorno = new SortedList<string, object>();
            SortedList<string, string> types = new SortedList<string, string>();
            string type = "";

            NomadXML param = new NomadXML("PARAM");

            //Agrego los parametros
            param.SetAttr("oi_estudio_per", PAR);

            //Ejecuto el recurso
            NomadXML resultado = NomadProxy.GetProxy().SQLService().GetXML("CLASS.NucleusRH.Base.Personal.Legajo.ESTUDIO_PER.QRY_GetEstudio", param.ToString());
            NomadLog.Debug((resultado.FirstChild() == null ? "No se encontró el estudio con id: " + PAR + "." : "Estudio encontrado."));

            if (resultado.FirstChild() != null)
            {
                //Armo una sorted list con los atributos del resultado y sus tipos
                for (int x = 0; x < resultado.Attrs.Count; x++)
                {
                    types.Add(resultado.Attrs[x].ToString(), resultado.GetAttr(resultado.Attrs[x].ToString()));
                }

                resultado = resultado.FirstChild();

                for (int r = 0; r < resultado.Attrs.Count; r++)
                {
                    //Busco de que tipo es el atributo
                    foreach (KeyValuePair<string, string> kvp in types)
                    {
                        if (kvp.Key == resultado.Attrs[r].ToString())
                        {
                            type = kvp.Value;
                            break;
                        }
                    }

                    //Agrego el atributo en base a su tipo
                    switch (type)
                    {
                        case "string":
                            retorno.Add(resultado.Attrs[r].ToString(), resultado.GetAttr(resultado.Attrs[r].ToString()));
                            break;
                        case "int":
                            retorno.Add(resultado.Attrs[r].ToString(), resultado.GetAttrInt(resultado.Attrs[r].ToString()));
                            break;
                        case "datetime":
                            retorno.Add(resultado.Attrs[r].ToString(), resultado.GetAttrDateTime(resultado.Attrs[r].ToString()));
                            break;
                        case "double":
                            retorno.Add(resultado.Attrs[r].ToString(), resultado.GetAttrDouble(resultado.Attrs[r].ToString()));
                            break;
                        case "bool":
                            retorno.Add(resultado.Attrs[r].ToString(), resultado.GetAttrBool(resultado.Attrs[r].ToString()));
                            break;
                        default:
                            retorno.Add(resultado.Attrs[r].ToString(), resultado.GetAttr(resultado.Attrs[r].ToString()));
                            break;
                    }
                    type = "";
                }
            }
            NomadLog.Debug("Retorno: " + retorno.ToString());
            return retorno;
        }

        public static string AddEstudioPer(int PER, System.Collections.Generic.SortedList<string, object> DATA1)
        {
            NomadLog.Debug("-------------------------------------------------");
            NomadLog.Debug("-----------AGREGAR ESTUDIO A PERSONA-------------");
            NomadLog.Debug("-------------------------------------------------");

            NomadLog.Debug("AddEstudioPer.oi_personal: " + PER);

            try
            {
                //Get PERSONAL
                NucleusRH.Base.Personal.Legajo.ESTUDIO_PER estudio = new ESTUDIO_PER();
                NucleusRH.Base.Personal.Legajo.PERSONAL PERSONAL;

                PERSONAL = NucleusRH.Base.Personal.Legajo.PERSONAL.Get(PER);
                if (PERSONAL == null) return "0";

                foreach (KeyValuePair<string, object> kvp in DATA1)
                {
                    switch (kvp.Key)
                    {
                        case "AREA_EST":
                            estudio.oi_area_est = kvp.Value.ToString();
                            break;
                        case "NIVEL_EST":
                            estudio.oi_nivel_estudio = kvp.Value.ToString();
                            break;
                        case "EST_VAL":
                            estudio.oi_estudio = kvp.Value.ToString();
                            break;
                        case "OTRO_EST":
                            estudio.d_otro_est = kvp.Value.ToString();
                            break;
                        case "TIPO_EST_VAL":
                            estudio.oi_tipo_establecim = kvp.Value.ToString();
                            break;
                        case "F_INICIO":
                            estudio.f_ini_estudio = (DateTime)kvp.Value;
                            break;
                        case "F_FIN":
                            estudio.f_fin_estudio = (DateTime)kvp.Value;
                            break;
                        case "ESTADO":
                            estudio.oi_estado_est = kvp.Value.ToString();
                            break;
                        case "PAIS":
                            estudio.oi_pais = kvp.Value.ToString();
                            break;
                        case "EST_EXT":
                            estudio.l_est_extranjero = (bool)kvp.Value;
                            break;
                        case "OTRO_EST_EDUC":
                            estudio.d_otro_est_educ = kvp.Value.ToString();
                            break;
                        case "DURACION":
                            estudio.e_duracion_estudio = (int)kvp.Value;
                            break;
                        case "U_TIEMPO":
                            estudio.oi_unidad_tiempo = kvp.Value.ToString();
                            break;
                        case "PERIODO":
                            estudio.e_periodo_en_curso = (int)kvp.Value;
                            break;
                        case "MAT_ADE":
                            estudio.e_mat_adeudadas = (int)kvp.Value;
                            break;
                        case "F_ACT":
                            estudio.f_actualiz = (DateTime)kvp.Value;
                            break;
                        case "PROM":
                            estudio.n_promedio = (double)kvp.Value;
                            break;
                        case "OBS":
                            estudio.o_estudio = (string)kvp.Value;
                            break;
                    }
                }

                PERSONAL.ESTUDIOS_PER.Add(estudio);
                NomadEnvironment.GetCurrentTransaction().Save(PERSONAL);
                return "1";
            }
            catch (Exception ex)
            {
                NomadLog.Debug("Error agregando ESTUDIO: " + ex);
                return "0";
            }
        }

        public static System.Collections.Generic.SortedList<string, string> AddDomicilioFiscal(string oi_personal, System.Collections.Generic.SortedList<string, object> DATA1) 
        {
            #region NomadLog.Debug
            NomadLog.Debug("----------------------------------------------");
            NomadLog.Debug("-----------AGREGAR DOMICILIO FISCAL-----------");
            NomadLog.Debug("----------------------------------------------");
            
            foreach (KeyValuePair<string, object> kvp in DATA1)
            {
                if (kvp.Value != null) { NomadLog.Debug(kvp.Key.ToString() + ":" + kvp.Value.ToString()); }
                else { NomadLog.Debug(kvp.Key.ToString() + ":Null"); }
            }

            NomadLog.Debug("AddDomicilioFiscal.oi_personal:" + oi_personal);
            #endregion

            SortedList<string, string> retorno = new SortedList<string, string>();
            retorno.Add("VAL", "FATALERR");
            retorno.Add("DES", "El método no fue procesado");

            try
            {
                NucleusRH.Base.Personal.Legajo.DOMIC_PER DOM = new NucleusRH.Base.Personal.Legajo.DOMIC_PER();
                NucleusRH.Base.Personal.Legajo.PERSONAL PERSONAL;
           
                //Valido Alta domicilio fiscal
                //Agrego el OI_DOM a null para poder validar. Null ya que es un alta y no tiene id todavia el domicilio
                DATA1.Add("OI_DOM", "");
                retorno = ValidarDomicilioFiscal(oi_personal, DATA1);

                if (retorno["VAL"].ToString() != "OK")
                    return retorno;

                //Get Personal
                PERSONAL = NucleusRH.Base.Personal.Legajo.PERSONAL.Get(oi_personal, true);

                foreach (KeyValuePair<string, object> kvp in DATA1)
                {
                    switch (kvp.Key)
                    {
                        case "TIPO_VAL":
                            if (kvp.Value != null)
                                DOM.oi_tipo_domicilio = kvp.Value.ToString();
                            break;
                        case "CALLE":
                            if (kvp.Value != null)
                                DOM.d_calle = kvp.Value.ToString();
                            break;
                        case "NUMERO":
                            if (kvp.Value != null)
                                DOM.e_numero = System.Convert.ToInt32(kvp.Value.ToString());
                            break;
                        case "PISO":
                            if (kvp.Value != null && kvp.Value.ToString() != "") { DOM.d_piso = kvp.Value.ToString(); break; }
                            else { DOM.d_pisoNull = true; break; }
                        case "DEPTO":
                            if (kvp.Value != null && kvp.Value.ToString() != "") { DOM.d_departamento = kvp.Value.ToString(); break; }
                            else { DOM.d_departamentoNull = true; break; }
                        case "ENTRE_1":
                            if (kvp.Value != null && kvp.Value.ToString() != "") { DOM.d_entre_calle_1 = kvp.Value.ToString(); break; }
                            else { DOM.d_entre_calle_1Null = true; break; }
                        case "ENTRE_2":
                            if (kvp.Value != null && kvp.Value.ToString() != "") { DOM.d_entre_calle_2 = kvp.Value.ToString(); break; }
                            else { DOM.d_entre_calle_2Null = true; break; }
                        case "OI_LOC":
                            if (kvp.Value != null && kvp.Value.ToString() != "0") { DOM.oi_localidad = kvp.Value.ToString(); break; }
                            else { DOM.oi_localidadNull = true; break; }
                        case "C_POSTAL":
                            if (kvp.Value != null && kvp.Value.ToString() != "") { DOM.c_postal = (string)kvp.Value; break; }
                            else { DOM.c_postalNull = true; break; }
                        case "TE_1":
                            if (kvp.Value != null && kvp.Value.ToString() != "") { DOM.te_1 = kvp.Value.ToString(); break; }
                            else { DOM.te_1Null = true; break; }
                        case "TE_2":
                            if (kvp.Value != null && kvp.Value.ToString() != "") { DOM.te_2 = kvp.Value.ToString(); break; }
                            else { DOM.te_2Null = true; break; }
                        case "CEL":
                            if (kvp.Value != null && kvp.Value.ToString() != "") { DOM.te_celular = kvp.Value.ToString(); break; }
                            else { DOM.te_celularNull = true; break; }
                        case "PARTIDO":
                            if (kvp.Value != null && kvp.Value.ToString() != "") { DOM.d_partido = kvp.Value.ToString(); break; }
                            else { DOM.d_partidoNull = true; break; }
                        case "OBS":
                            if (kvp.Value != null && kvp.Value.ToString() != "") { DOM.o_domicilio = kvp.Value.ToString(); break; }
                            else { DOM.o_domicilioNull = true; break; }
                    }
                }

                DOM.l_domic_fiscal = true;
                PERSONAL.DOMIC_PER.Add(DOM);

                NomadEnvironment.GetCurrentTransaction().Save(PERSONAL);
                retorno = NucleusRH.Base.Configuracion.Herramientas.QUERIES.CreateRTA("OK", "El domicilio se agregó exitosamente");
                return retorno;
            }
            catch (Exception ex)
            {
                retorno = NucleusRH.Base.Configuracion.Herramientas.QUERIES.CreateRTA("FATALERR", ex.Message);
                return retorno;
            }
        }

        public static System.Collections.Generic.SortedList<string, string> EditDomicilioFiscal(string oi_domicilio_per, string oi_personal, System.Collections.Generic.SortedList<string, object> DATA1)
        {
            #region NomadLog.Debug
            NomadLog.Debug("----------------------------------------------");
            NomadLog.Debug("-----------EDITAR DOMICILIO FISCAL-----------");
            NomadLog.Debug("----------------------------------------------");
            
            foreach (KeyValuePair<string, object> kvp in DATA1)
            {
                if (kvp.Value != null) { NomadLog.Debug(kvp.Key.ToString() + ":" + kvp.Value.ToString()); }
                else { NomadLog.Debug(kvp.Key.ToString() + ":Null"); }
            }

            NomadLog.Debug("EditDomicilioFiscal.oi_domicilio_per:" + oi_domicilio_per);
            #endregion

            SortedList<string, object> DATAVALIDAR = new SortedList<string, object>();
            SortedList<string, string> retorno = new SortedList<string, string>();
            retorno.Add("VAL", "FATALERR");
            retorno.Add("DES", "El método no fue procesado");

            try
            {
                DATAVALIDAR.Add("OI_DOM",oi_domicilio_per);
                DATAVALIDAR.Add("TIPO_VAL",DATA1["TIPO_VAL"]);
                retorno = ValidarDomicilioFiscal(oi_personal,DATAVALIDAR);

                if (retorno["VAL"].ToString() != "OK")
                    return retorno;

                Personal.Legajo.DOMIC_PER DOM = Personal.Legajo.DOMIC_PER.Get(oi_domicilio_per,true);

                foreach (KeyValuePair<string, object> kvp in DATA1)
                {
                    switch (kvp.Key)
                    {
                        case "TIPO_VAL":
                            if (kvp.Value != null)
                                DOM.oi_tipo_domicilio = kvp.Value.ToString();
                            break;
                        case "CALLE":
                            if (kvp.Value != null)
                                DOM.d_calle = kvp.Value.ToString();
                            break;
                        case "NUMERO":
                            if (kvp.Value != null)
                                DOM.e_numero = System.Convert.ToInt32(kvp.Value.ToString());
                            break;
                        case "PISO":
                            if (kvp.Value != null && kvp.Value.ToString() != "") { DOM.d_piso = kvp.Value.ToString(); break; }
                            else { DOM.d_pisoNull = true; break; }
                        case "DEPTO":
                            if (kvp.Value != null && kvp.Value.ToString() != "") { DOM.d_departamento = kvp.Value.ToString(); break; }
                            else { DOM.d_departamentoNull = true; break; }
                        case "ENTRE_1":
                            if (kvp.Value != null && kvp.Value.ToString() != "") { DOM.d_entre_calle_1 = kvp.Value.ToString(); break; }
                            else { DOM.d_entre_calle_1Null = true; break; }
                        case "ENTRE_2":
                            if (kvp.Value != null && kvp.Value.ToString() != "") { DOM.d_entre_calle_2 = kvp.Value.ToString(); break; }
                            else { DOM.d_entre_calle_2Null = true; break; }
                        case "OI_LOC":
                            if (kvp.Value != null && kvp.Value.ToString() != "0") { DOM.oi_localidad = kvp.Value.ToString(); break; }
                            else { DOM.oi_localidadNull = true; break; }
                        case "C_POSTAL":
                            if (kvp.Value != null && kvp.Value.ToString() != "") { DOM.c_postal = kvp.Value.ToString(); break; }
                            else { DOM.c_postalNull = true; break; }
                        case "TE_1":
                            if (kvp.Value != null && kvp.Value.ToString() != "") { DOM.te_1 = kvp.Value.ToString(); break; }
                            else { DOM.te_1Null = true; break; }
                        case "TE_2":
                            if (kvp.Value != null && kvp.Value.ToString() != "") { DOM.te_2 = kvp.Value.ToString(); break; }
                            else { DOM.te_2Null = true; break; }
                        case "CEL":
                            if (kvp.Value != null && kvp.Value.ToString() != "") { DOM.te_celular = kvp.Value.ToString(); break; }
                            else { DOM.te_celularNull = true; break; }
                        case "PARTIDO":
                            if (kvp.Value != null && kvp.Value.ToString() != "") { DOM.d_partido = kvp.Value.ToString(); break; }
                            else { DOM.d_partidoNull = true; break; }
                        case "OBS":
                            if (kvp.Value != null && kvp.Value.ToString() != "") { DOM.o_domicilio = kvp.Value.ToString(); break; }
                            else { DOM.o_domicilioNull = true; break; }
                    }
                }

                DOM.l_domic_fiscal = true;

                NomadEnvironment.GetCurrentTransaction().SaveRefresh(DOM);
                retorno = NucleusRH.Base.Configuracion.Herramientas.QUERIES.CreateRTA("OK", "El domicilio se modificó exitosamente");
                return retorno;
            }
            catch (Exception ex)
            {
                retorno = NucleusRH.Base.Configuracion.Herramientas.QUERIES.CreateRTA("FATALERR", ex.Message);
                return retorno;
            }
        }

        public static System.Collections.Generic.SortedList<string,string> ValidarDomicilioFiscal( string oi_personal, System.Collections.Generic.SortedList<string,object> DATA1)
        {
            bool DomNull = false; 
            #region NomadLog.Debug
            NomadLog.Debug("----------------------------------------------");
            NomadLog.Debug("-----------Validar Domicilio Fiscal-----------");
            NomadLog.Debug("----------------------------------------------");

            foreach (KeyValuePair<string, object> kvp in DATA1)
            {
                if (kvp.Value != null) { NomadLog.Debug(kvp.Key.ToString() + ":" + kvp.Value.ToString()); }
                else
                { 
                    NomadLog.Debug(kvp.Key.ToString() + ":Null");
                    DomNull = true;
                }
            }

            //Esto para el caso en que se da de alta y el oi_dom viene como null lo seteo a ""
            if (DomNull)
                DATA1["OI_DOM"] = "";

            NomadLog.Debug("ValidarDomicilioFiscal.oi_personal:" + oi_personal);
            #endregion

            SortedList<string, string> retorno = new SortedList<string, string>();
            retorno.Add("VAL", "FATALERR");
            retorno.Add("DES", "El método no fue procesado");
            try
            {
                NucleusRH.Base.Personal.Legajo.DOMIC_PER DOM;

                //Get Personal
                Personal.Legajo.PERSONAL PERSONAL = Personal.Legajo.PERSONAL.Get(oi_personal, true);

                if (PERSONAL == null)
                    throw new Exception("La persona con oi_personal " + oi_personal + "no se puede encontrar");

                //Get Domicilio si es que se está editando
                if (DATA1["OI_DOM"].ToString() != "")
                {
                    DOM = NucleusRH.Base.Personal.Legajo.DOMIC_PER.Get(DATA1["OI_DOM"].ToString(), true);
                    if (DOM == null)
                        throw new Exception("El domicilio con oi_domicilio_per " + DATA1["OI_DOM"].ToString() + "no se puede encontrar");
                    else
                    {
                        //Valido que sea fiscal
                        if (!DOM.l_domic_fiscal)
                            throw new Exception("El domicilio que intenta editar no es un domicilio fiscal.");                        
                    }
                }

                foreach (Personal.Legajo.DOMIC_PER domi in PERSONAL.DOMIC_PER)
                {
                    //Cuando se esta dando de alta
                    if (DATA1["OI_DOM"].ToString() == "")
                    {
                        //Valido que no tenga domicilio fiscal
                        if (domi.l_domic_fiscal)
                            throw new Exception("No puede dar de alta un domicilio fiscal ya que actualmente tiene uno asignado.\nPor favor para establecer un domicilio fiscal póngase en contacto con el encargado de la gestión del sistema");
                        if (domi.oi_tipo_domicilio == DATA1["TIPO_VAL"].ToString())
                        {
                            //Get para traer la descripcion del tipo de domicilio que ya existe.
                            Personal.Tipos_Domicilio.TIPO_DOMICILIO tipo_dom = Personal.Tipos_Domicilio.TIPO_DOMICILIO.Get(domi.oi_tipo_domicilio, false);
                            throw new Exception("No puede dar de alta un domicilio del tipo " + tipo_dom.d_tipo_domicilio +
                                " ya que tiene uno asignado. Por favor si desea hacer fiscal el domicilio del tipo " + tipo_dom.d_tipo_domicilio +
                                " que actualmente tiene cargado contactese con el encargado de la gestión del sistema.");
                        }
                    }
                    else //Cuando se edita
                    {
                        if (DATA1["TIPO_VAL"].ToString() == domi.oi_tipo_domicilio)
                        {
                            if(!(DATA1["OI_DOM"].ToString() == domi.Id))
                            {
                                Personal.Tipos_Domicilio.TIPO_DOMICILIO tipo_dom = Personal.Tipos_Domicilio.TIPO_DOMICILIO.Get(domi.oi_tipo_domicilio, false);
                                throw new Exception("No puede editar el domicilio fiscal al tipo " + tipo_dom.d_tipo_domicilio +
                                    " ya que tiene uno asignado. Por favor si desea hacer fiscal el domicilio del tipo " + tipo_dom.d_tipo_domicilio +
                                    " que actualmente tiene cargado contactese con el encargado de la gestión del sistema.");
                            }
                        }
                    }
                }   
                retorno = Configuracion.Herramientas.QUERIES.CreateRTA("OK", "Validaciones del domicilio correctas");
                return retorno;
            }        
            catch(Exception e)
            {
                retorno = NucleusRH.Base.Configuracion.Herramientas.QUERIES.CreateRTA("ERR", e.Message);
                return retorno;
            }
        }

        public static System.Collections.Generic.SortedList<string,string> AddDomicilio( string oi_personal, System.Collections.Generic.SortedList<string,object> DATA1)
        {
            #region NomadLog.Debug
            NomadLog.Debug("---------------------------------------");
            NomadLog.Debug("-----------AGREGAR DOMICILIO-----------");
            NomadLog.Debug("---------------------------------------");

            foreach (KeyValuePair<string, object> kvp in DATA1)
            {
                if (kvp.Value != null) { NomadLog.Debug(kvp.Key.ToString() + ":" + kvp.Value.ToString()); }
                else { NomadLog.Debug(kvp.Key.ToString() + ":Null"); }
            }

            NomadLog.Debug("AddDomicilio.oi_personal:" + oi_personal);
            #endregion

            SortedList<string, string> retorno = new SortedList<string, string>();
            retorno.Add("VAL", "FATALERR");
            retorno.Add("DES", "El método no fue procesado");

            try
            {
                NucleusRH.Base.Personal.Legajo.DOMIC_PER DOM = new NucleusRH.Base.Personal.Legajo.DOMIC_PER();
                NucleusRH.Base.Personal.Legajo.PERSONAL PERSONAL;

                //Valido Alta domicilio
                //Agrego el OI_DOM a null para poder validar. Null ya que es un alta y no tiene id todavia el domicilio
                DATA1.Add("OI_DOM", "");
                retorno = ValidarDomicilio(oi_personal, DATA1);

                if (retorno["VAL"].ToString() != "OK")
                    return retorno;

                //Get Personal
                PERSONAL = NucleusRH.Base.Personal.Legajo.PERSONAL.Get(oi_personal, true);

                foreach (KeyValuePair<string, object> kvp in DATA1)
                {
                    switch (kvp.Key)
                    {
                        case "TIPO_VAL":
                            if (kvp.Value != null)
                                DOM.oi_tipo_domicilio = kvp.Value.ToString();
                            break;
                        case "CALLE":
                            if (kvp.Value != null)
                                DOM.d_calle = kvp.Value.ToString();
                            break;
                        case "NUMERO":
                            if (kvp.Value != null)
                                DOM.e_numero = System.Convert.ToInt32(kvp.Value.ToString());
                            break;
                        case "PISO":
                            if (kvp.Value != null && kvp.Value.ToString() != "") { DOM.d_piso = kvp.Value.ToString(); break; }
                            else { DOM.d_pisoNull = true; break; }
                        case "DEPTO":
                            if (kvp.Value != null && kvp.Value.ToString() != "") { DOM.d_departamento = kvp.Value.ToString(); break; }
                            else { DOM.d_departamentoNull = true; break; }
                        case "ENTRE_1":
                            if (kvp.Value != null && kvp.Value.ToString() != "") { DOM.d_entre_calle_1 = kvp.Value.ToString(); break; }
                            else { DOM.d_entre_calle_1Null = true; break; }
                        case "ENTRE_2":
                            if (kvp.Value != null && kvp.Value.ToString() != "") { DOM.d_entre_calle_2 = kvp.Value.ToString(); break; }
                            else { DOM.d_entre_calle_2Null = true; break; }
                        case "OI_LOC":
                            if (kvp.Value != null && kvp.Value.ToString() != "0") { DOM.oi_localidad = kvp.Value.ToString(); break; }
                            else { DOM.oi_localidadNull = true; break; }
                        case "C_POSTAL":
                            if (kvp.Value != null && kvp.Value.ToString() != "") { DOM.c_postal = (string)kvp.Value; break; }
                            else { DOM.c_postalNull = true; break; }
                        case "TE_1":
                            if (kvp.Value != null && kvp.Value.ToString() != "") { DOM.te_1 = kvp.Value.ToString(); break; }
                            else { DOM.te_1Null = true; break; }
                        case "TE_2":
                            if (kvp.Value != null && kvp.Value.ToString() != "") { DOM.te_2 = kvp.Value.ToString(); break; }
                            else { DOM.te_2Null = true; break; }
                        case "CEL":
                            if (kvp.Value != null && kvp.Value.ToString() != "") { DOM.te_celular = kvp.Value.ToString(); break; }
                            else { DOM.te_celularNull = true; break; }
                        case "PARTIDO":
                            if (kvp.Value != null && kvp.Value.ToString() != "") { DOM.d_partido = kvp.Value.ToString(); break; }
                            else { DOM.d_partidoNull = true; break; }
                        case "OBS":
                            if (kvp.Value != null && kvp.Value.ToString() != "") { DOM.o_domicilio = kvp.Value.ToString(); break; }
                            else { DOM.o_domicilioNull = true; break; }
                    }
                }

                DOM.l_domic_fiscal = false;
                PERSONAL.DOMIC_PER.Add(DOM);

                NomadEnvironment.GetCurrentTransaction().Save(PERSONAL);
                retorno = NucleusRH.Base.Configuracion.Herramientas.QUERIES.CreateRTA("OK", "El domicilio se agregó exitosamente");
                return retorno;
            }
            catch (Exception ex)
            {
                retorno = NucleusRH.Base.Configuracion.Herramientas.QUERIES.CreateRTA("FATALERR", ex.Message);
                return retorno;
            }
        }

        public static System.Collections.Generic.SortedList<string,string> EditDomicilio( string oi_domic_per, string oi_personal, System.Collections.Generic.SortedList<string,object> DATA1)
        {
            #region NomadLog.Debug
            NomadLog.Debug("--------------------------------------");
            NomadLog.Debug("-----------EDITAR DOMICILIO-----------");
            NomadLog.Debug("--------------------------------------");

            foreach (KeyValuePair<string, object> kvp in DATA1)
            {
                if (kvp.Value != null) { NomadLog.Debug(kvp.Key.ToString() + ":" + kvp.Value.ToString()); }
                else { NomadLog.Debug(kvp.Key.ToString() + ":Null"); }
            }

            NomadLog.Debug("EditDomicilio.oi_domicilio_per:" + oi_domic_per);
            #endregion

            SortedList<string, object> DATAVALIDAR = new SortedList<string, object>();
            SortedList<string, string> retorno = new SortedList<string, string>();
            retorno.Add("VAL", "FATALERR");
            retorno.Add("DES", "El método no fue procesado");

            try
            {
                DATAVALIDAR.Add("OI_DOM", oi_domic_per);
                DATAVALIDAR.Add("TIPO_VAL", DATA1["TIPO_VAL"]);
                retorno = ValidarDomicilio(oi_personal, DATAVALIDAR);

                if (retorno["VAL"].ToString() != "OK")
                    return retorno;

                Personal.Legajo.DOMIC_PER DOM = Personal.Legajo.DOMIC_PER.Get(oi_domic_per, true);

                foreach (KeyValuePair<string, object> kvp in DATA1)
                {
                    switch (kvp.Key)
                    {
                        case "TIPO_VAL":
                            if (kvp.Value != null)
                                DOM.oi_tipo_domicilio = kvp.Value.ToString();
                            break;
                        case "CALLE":
                            if (kvp.Value != null)
                                DOM.d_calle = kvp.Value.ToString();
                            break;
                        case "NUMERO":
                            if (kvp.Value != null)
                                DOM.e_numero = System.Convert.ToInt32(kvp.Value.ToString());
                            break;
                        case "PISO":
                            if (kvp.Value != null && kvp.Value.ToString() != "") { DOM.d_piso = kvp.Value.ToString(); break; }
                            else { DOM.d_pisoNull = true; break; }
                        case "DEPTO":
                            if (kvp.Value != null && kvp.Value.ToString() != "") { DOM.d_departamento = kvp.Value.ToString(); break; }
                            else { DOM.d_departamentoNull = true; break; }
                        case "ENTRE_1":
                            if (kvp.Value != null && kvp.Value.ToString() != "") { DOM.d_entre_calle_1 = kvp.Value.ToString(); break; }
                            else { DOM.d_entre_calle_1Null = true; break; }
                        case "ENTRE_2":
                            if (kvp.Value != null && kvp.Value.ToString() != "") { DOM.d_entre_calle_2 = kvp.Value.ToString(); break; }
                            else { DOM.d_entre_calle_2Null = true; break; }
                        case "OI_LOC":
                            if (kvp.Value != null && kvp.Value.ToString() != "0") { DOM.oi_localidad = kvp.Value.ToString(); break; }
                            else { DOM.oi_localidadNull = true; break; }
                        case "C_POSTAL":
                            if (kvp.Value != null && kvp.Value.ToString() != "") { DOM.c_postal = kvp.Value.ToString(); break; }
                            else { DOM.c_postalNull = true; break; }
                        case "TE_1":
                            if (kvp.Value != null && kvp.Value.ToString() != "") { DOM.te_1 = kvp.Value.ToString(); break; }
                            else { DOM.te_1Null = true; break; }
                        case "TE_2":
                            if (kvp.Value != null && kvp.Value.ToString() != "") { DOM.te_2 = kvp.Value.ToString(); break; }
                            else { DOM.te_2Null = true; break; }
                        case "CEL":
                            if (kvp.Value != null && kvp.Value.ToString() != "") { DOM.te_celular = kvp.Value.ToString(); break; }
                            else { DOM.te_celularNull = true; break; }
                        case "PARTIDO":
                            if (kvp.Value != null && kvp.Value.ToString() != "") { DOM.d_partido = kvp.Value.ToString(); break; }
                            else { DOM.d_partidoNull = true; break; }
                        case "OBS":
                            if (kvp.Value != null && kvp.Value.ToString() != "") { DOM.o_domicilio = kvp.Value.ToString(); break; }
                            else { DOM.o_domicilioNull = true; break; }
                    }
                }

                NomadEnvironment.GetCurrentTransaction().SaveRefresh(DOM);
                retorno = NucleusRH.Base.Configuracion.Herramientas.QUERIES.CreateRTA("OK", "El domicilio se modificó exitosamente");
                return retorno;
            }
            catch (Exception ex)
            {
                retorno = NucleusRH.Base.Configuracion.Herramientas.QUERIES.CreateRTA("FATALERR", ex.Message);
                return retorno;
            }
        }

        public static System.Collections.Generic.SortedList<string, string> DeleteDomicilio(string oi_domic_per)
        {
            NomadLog.Debug("--------------------------------------");
            NomadLog.Debug("-----------DELETE DOMICILIO-----------");
            NomadLog.Debug("--------------------------------------");

            NomadLog.Debug("DeleteDomicilio.oi_domic_per: " + oi_domic_per);

            SortedList<string, string> retorno = new SortedList<string, string>();
            retorno.Add("VAL", "FATALERR");
            retorno.Add("DES", "El método no fue procesado");

            try
            {
                //Get Domicilio
                NucleusRH.Base.Personal.Legajo.DOMIC_PER objDom = NucleusRH.Base.Personal.Legajo.DOMIC_PER.Get(oi_domic_per, true);
                if (objDom == null)
                    throw new Exception("El domicilio con oi: "+ oi_domic_per + " no fue encontrado");

                NomadEnvironment.GetCurrentTransaction().Delete(objDom);
                retorno = NucleusRH.Base.Configuracion.Herramientas.QUERIES.CreateRTA("OK", "El domicilio se eliminó exitosamente");
                return retorno;
            }
            catch (Exception ex)
            {
                retorno = NucleusRH.Base.Configuracion.Herramientas.QUERIES.CreateRTA("FATALERR", ex.Message);
                return retorno;
            }
        }

        public static System.Collections.Generic.SortedList<string,string> ValidarDomicilio( string oi_personal, System.Collections.Generic.SortedList<string,object> DATA1)
        {
            bool DomNull = false;
            #region NomadLog.Debug
            NomadLog.Debug("---------------------------------------");
            NomadLog.Debug("-----------Validar Domicilio-----------");
            NomadLog.Debug("---------------------------------------");

            foreach (KeyValuePair<string, object> kvp in DATA1)
            {
                if (kvp.Value != null) { NomadLog.Debug(kvp.Key.ToString() + ":" + kvp.Value.ToString()); }
                else
                {
                    NomadLog.Debug(kvp.Key.ToString() + ":Null");
                    DomNull = true;
                }
            }

            //Esto para el caso en que se da de alta y el oi_dom viene como null lo seteo a ""
            if (DomNull)
                DATA1["OI_DOM"] = "";

            NomadLog.Debug("ValidarDomicilio.oi_personal:" + oi_personal);
            #endregion

            SortedList<string, string> retorno = new SortedList<string, string>();
            retorno.Add("VAL", "FATALERR");
            retorno.Add("DES", "El método no fue procesado");
            try
            {
                NucleusRH.Base.Personal.Legajo.DOMIC_PER DOM;

                //Get Personal
                Personal.Legajo.PERSONAL PERSONAL = Personal.Legajo.PERSONAL.Get(oi_personal, true);

                if (PERSONAL == null)
                {
                    retorno = NucleusRH.Base.Configuracion.Herramientas.QUERIES.CreateRTA("FATALERR", "La persona con oi_personal " + oi_personal + "no se puede encontrar");
                    return retorno;
                }

                //Get Domicilio si es que se está editando
                if (DATA1["OI_DOM"].ToString() != "")
                {
                    DOM = NucleusRH.Base.Personal.Legajo.DOMIC_PER.Get(DATA1["OI_DOM"].ToString(), true);
                    if (DOM == null)
                        throw new Exception("El domicilio con oi_domic_per " + DATA1["OI_DOM"].ToString() + "no se puede encontrar");
                }

                foreach (Personal.Legajo.DOMIC_PER domi in PERSONAL.DOMIC_PER)
                {
                    //Cuando se esta dando de alta
                    if (DATA1["OI_DOM"].ToString() == "")
                    {
                        if (domi.oi_tipo_domicilio == DATA1["TIPO_VAL"].ToString())
                        {
                            //Get para traer la descripcion del tipo de domicilio que ya existe.
                            Personal.Tipos_Domicilio.TIPO_DOMICILIO tipo_dom = Personal.Tipos_Domicilio.TIPO_DOMICILIO.Get(domi.oi_tipo_domicilio, false);
                            throw new Exception("No puede dar de alta un domicilio del tipo " + tipo_dom.d_tipo_domicilio +
                                " ya que tiene uno asignado");
                        }
                    }
                    else //Cuando se edita
                    {
                        if (DATA1["TIPO_VAL"].ToString() == domi.oi_tipo_domicilio)
                        {
                            if (!(DATA1["OI_DOM"].ToString() == domi.Id))
                            {
                                Personal.Tipos_Domicilio.TIPO_DOMICILIO tipo_dom = Personal.Tipos_Domicilio.TIPO_DOMICILIO.Get(domi.oi_tipo_domicilio, false);
                                throw new Exception("No puede editar el domicilio al tipo " + tipo_dom.d_tipo_domicilio +
                                    " ya que tiene uno asignado.");
                            }
                        }
                    }
                }
                retorno = Configuracion.Herramientas.QUERIES.CreateRTA("OK", "Validaciones del domicilio correctas");
                return retorno;
            }
            catch (Exception e)
            {
                retorno = NucleusRH.Base.Configuracion.Herramientas.QUERIES.CreateRTA("ERR", e.Message);
                return retorno;
            }
        }
    }
}



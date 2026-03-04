using System;
using System.Xml;
using System.IO;
using System.Collections;

using Nomad.NSystem.Document;
using Nomad.NSystem.Proxy;
using Nomad.NSystem.Base;
using Nomad.NSystem.Functions;

namespace NucleusRH.Base.Organizacion.Puestos
{

    //////////////////////////////////////////////////////////////////////////////////
    //Clase Posiciones Laborales
    public partial class POSICION
    {
        public void Crear_Posicion(string id_puesto, string estructura_id, string oi_clase_org)
        {

            //INSTANCIO EL PUESTO
            NucleusRH.Base.Organizacion.Puestos.PUESTO ddoPUESTO;
            ddoPUESTO = NucleusRH.Base.Organizacion.Puestos.PUESTO.Get(id_puesto, false);

            //SECCION DE CODIGO PARA CREAR LA UNIDAD ORGANIZATIVA CORRESPONDIENTE A LA POSICION
            NucleusRH.Base.Organizacion.Unidades_Organizativas.UNIDAD_ORG unidad;
            NucleusRH.Base.Organizacion.Unidades_Organizativas.TIPO_UNI_ORG tipo_unidad;
            tipo_unidad = NucleusRH.Base.Organizacion.Unidades_Organizativas.TIPO_UNI_ORG.Get("3", false);

            //OBTENGO LA EMPRESA PARA PODER SACAR EL CODIGO
            NucleusRH.Base.Organizacion.Empresas.EMPRESA ddoEMPRESA;
            ddoEMPRESA = NucleusRH.Base.Organizacion.Empresas.EMPRESA.Get(ddoPUESTO.oi_empresa, false);

            //GENERO EL CODIGO DE UNIDAD ORGANIZATIVA A PARTIR DE LA EMPRESA, EL PUESTO Y LA POSICION
            string cod = ddoEMPRESA.c_empresa + "-" + ddoPUESTO.c_puesto + "-" + this.c_posicion;

            // OBTENGO UNA UNIDAD BASURA CON EL MISMO NOMBRE
            unidad = (NucleusRH.Base.Organizacion.Unidades_Organizativas.UNIDAD_ORG)tipo_unidad.UNI_ORG.GetByAttribute("c_unidad_org", cod, false);
            string id;

            // SI NO EXISTE CREO UNA NUEVA
            if (unidad == null)
            {
                unidad = new NucleusRH.Base.Organizacion.Unidades_Organizativas.UNIDAD_ORG();

                // ASIGNO LOS VALORES
                unidad.c_unidad_org = cod;
                unidad.d_unidad_org = this.d_posicion;
                unidad.o_unidad_org = this.o_posicion;
                unidad.oi_puesto = ddoPUESTO.id;
                unidad.oi_clase_org = int.Parse(oi_clase_org);
                tipo_unidad.UNI_ORG.Add(unidad);

                // GRABO
                try
                {
                    NomadEnvironment.GetCurrentTransaction().SaveRefresh(tipo_unidad);
                }
                catch (Exception e)
                {
                    throw new NomadAppException("Error Guardando la Unidad Organizativa de Posicion: " + e.Message);
                }

                this.l_vigente = true;
                this.f_hasta_vigenciaNull = true;

                //ASIGNO LA UNIDAD ORGANIZATIVA QUE POSTERIORMENTE SE SETEARA EN LA ESTRUCUTRA
                this.oi_unidad_org = ((NucleusRH.Base.Organizacion.Unidades_Organizativas.UNIDAD_ORG)tipo_unidad.UNI_ORG.GetByAttribute("c_unidad_org", cod, false)).Id;

                //AGREGO LA POSICION AL PUESTO
                ddoPUESTO.POSICIONES.Add(this);
            }
            else
            {
                id = unidad.id.ToString();
                // ASIGNO LOS VALORES
                unidad.d_unidad_org = this.d_posicion;
                unidad.o_unidad_org = this.o_posicion;
                unidad.oi_puesto = ddoPUESTO.id;
                unidad.oi_clase_org = int.Parse(oi_clase_org);

                // GRABO
                try
                {
                    NomadEnvironment.GetCurrentTransaction().Save(tipo_unidad);
                }
                catch (Exception e)
                {
                    throw new NomadAppException("Error Guardando la Unidad Organizativa de Posicion: " + e.Message);
                }

                NucleusRH.Base.Organizacion.Puestos.POSICION posicion = (NucleusRH.Base.Organizacion.Puestos.POSICION) ddoPUESTO.POSICIONES.GetByAttribute("c_posicion", this.c_posicion, false);

                //Creo la posicion
                if (posicion == null)
                {
                    this.l_vigente = true;
                    this.f_hasta_vigenciaNull = true;

                    //ASIGNO LA UNIDAD ORGANIZATIVA QUE POSTERIORMENTE SE SETEARA EN LA ESTRUCUTRA
                    this.oi_unidad_org = id;

                    //AGREGO LA POSICION AL PUESTO
                    ddoPUESTO.POSICIONES.Add(this);
                }
                else
                {
                    //MODIFICA LA INFORMACION DE LA POSICION
                    posicion.d_posicion = this.d_posicion;
                    posicion.o_posicion = this.o_posicion;
                    posicion.oi_centro_costo = this.oi_centro_costo;
                    posicion.l_vigente = true;
                    posicion.f_hasta_vigenciaNull = true;
                    if (this.oi_nivel_salNull) posicion.oi_nivel_salNull = true;
                    else posicion.oi_nivel_sal = this.oi_nivel_sal;
                    if (this.oi_categoriaNull) posicion.oi_categoriaNull = true;
                    else posicion.oi_categoria = this.oi_categoria;
                    if (this.oi_estado_civilNull) posicion.oi_estado_civilNull = true;
                    else posicion.oi_estado_civil = this.oi_estado_civil;
                    if (this.d_sexo_reqNull) posicion.d_sexo_reqNull = true;
                    else posicion.d_sexo_req = this.d_sexo_req;
                    if (this.e_edad_min_reqNull) posicion.e_edad_min_reqNull = true;
                    else posicion.e_edad_min_req = this.e_edad_min_req;
                    if (this.e_edad_max_reqNull) posicion.e_edad_max_reqNull = true;
                    else posicion.e_edad_max_req = this.e_edad_max_req;

                    //ASIGNO LA UNIDAD ORGANIZATIVA QUE POSTERIORMENTE SE SETEARA EN LA ESTRUCTURA
                    this.oi_unidad_org = id;
                    posicion.oi_unidad_org = this.oi_unidad_org;
                }
            }

            //SECCION QUE SE ENCARGA DE INSTANCIAR LA CLASE ORGANIZATIVA PARA PODER AGREGAR LA ESTRUCTURA
            //LLAMO AL METODO QUE RECORRE LA CLASE ORGANZATIVA HASTA LA ESTRUCTURA SELECCIONADA
            NucleusRH.Base.Organizacion.Clases_Organizativas.ESTRUCTURA ddoEstr = NucleusRH.Base.Organizacion.Clases_Organizativas.ESTRUCTURA.Get(estructura_id, false);

            //CREO UN NUEVO ESTRUCTURA
            NucleusRH.Base.Organizacion.Clases_Organizativas.ESTRUCTURA ddoEstNew;
            ddoEstNew = new NucleusRH.Base.Organizacion.Clases_Organizativas.ESTRUCTURA();
            ddoEstNew.oi_unidad_org = this.oi_unidad_org;
            ddoEstNew.l_staff = this.l_staff;

            //AGREGO LA ESTRUCTURA A LA ESTRUCTURA
            ddoEstr.ESTRUCTURAS.Add(ddoEstNew);

            //EN UN MISMA TRANSACCION GUARDO EL PUESTO Y LA CLASE ORGANIZATIVA
            NomadEnvironment.GetCurrentTransaction().Begin();
            try
            {
                NomadEnvironment.GetCurrentTransaction().Save(ddoEstr);
                NomadEnvironment.GetCurrentTransaction().Save(ddoPUESTO);
                NomadEnvironment.GetCurrentTransaction().Commit();
            }
            catch (Exception e)
            {
                NomadEnvironment.GetCurrentTransaction().Rollback();
                throw new NomadAppException("Error al crear la Posición. " + e.Message);
            }
        }
        public static string Eliminar_Posicion(string estructura_id)
        {

            //TIRO UN QRY PARA SABER QUE POSICION Y QUE PUESTO CORRESPONE A LA ESTRUCTURA
            NomadXML xmlposId;
            string param = "<DATO oi_estructura=\"" + estructura_id + "\"/>";
            xmlposId = new NomadXML();
            NomadEnvironment.GetTrace().Info(param);
            xmlposId.SetText(NomadEnvironment.QueryString(POSICION.Resources.QRY_ESTRUCTURA_POSICION, param));
            NomadEnvironment.GetTrace().Info(xmlposId.ToString());

            //INSTANCIO EL PUESTO PARA ELIMINAR LA POSICION
            PUESTO ddoPUESTO = PUESTO.Get(xmlposId.FirstChild().GetAttr("oi_puesto"), false);

            //TRAIGO LA EMPRESA PARA OBTENER EL CODIGO Y PODER LUEGO BORRAR LA UNIDAD ORGANIZATIVA
            NucleusRH.Base.Organizacion.Empresas.EMPRESA ddoEMPRESA;
            ddoEMPRESA = NucleusRH.Base.Organizacion.Empresas.EMPRESA.Get(ddoPUESTO.oi_empresa, false);

            POSICION ddoPOSICION = POSICION.Get(xmlposId.FirstChild().GetAttr("oi_posicion"), true);

            //CARGO LA ESTRUCTURA
            NucleusRH.Base.Organizacion.Clases_Organizativas.ESTRUCTURA ddoEstr = NucleusRH.Base.Organizacion.Clases_Organizativas.ESTRUCTURA.Get(estructura_id, false);

            //PREGUNTO SI LA ESTRUCUTRA NO ES LA CABECERA Y VUELVO A RECORRER LA CLASE PARA PARARME EN EL PADRE
            //DE LA ESTRUCTURA A ELIMINAR
            if (ddoEstr.oi_estr_padreNull)
                return "No puede eliminar la Cabecera del Organigrama Jerárquico";

            //SI LA POSICION ESTA ASIGNADA A ALGUN LEGAJO COMO TRAYECTORIA HAY QUE DEJARLA FUERA DE VIGENCIA, SINO SE ELIMINA FISICAMENTE
            string flag;
            //SI VUELVE 0 LA POSICION NO TIENE LEGAJOS ASIGNADOS
            //SI VUELVE 1 LA POSICION TIENE ASIGNADOS LEGAJOS COMO TRAYECTORIA
            //SI VUELVE 2 LA POSICION TIENE ASIGNADOS LEGAJOS ACTUALMENTE
            flag = POSICION.Validar_Legajos_Posicion(estructura_id);
            //EL METODO DEVUELVE "1" SI LA POSICION ESTA ASIGNADA EN ALGUN LEGAJO COMO TRAYECTORIA, EN ESTE CASO SE DESHABILITA
            if (flag == "2")
                return "No se puede eliminar la posición porque tiene legajos asignados";

            NucleusRH.Base.Organizacion.Unidades_Organizativas.UNIDAD_ORG unidad;
            NucleusRH.Base.Organizacion.Unidades_Organizativas.TIPO_UNI_ORG tipo_unidad;
            tipo_unidad = NucleusRH.Base.Organizacion.Unidades_Organizativas.TIPO_UNI_ORG.Get("3", false);
            unidad = (NucleusRH.Base.Organizacion.Unidades_Organizativas.UNIDAD_ORG)tipo_unidad.UNI_ORG.GetById(ddoPOSICION.oi_unidad_org);

            //Busca si la unidad organizativa esta siendo utilizada en otras estructuras vigentes o no vigentes
            string parametro = "<DATO oi_estructura=\"" + estructura_id + "\"  oi_unidad_org=\"" + unidad.Id + "\"/>";
            NomadXML result = new NomadXML();
            result.SetText(NomadEnvironment.QueryString(POSICION.Resources.QRY_UTIL_ESTRUCTURAS, parametro));

            //Variables estrvigente y estrnovigente -> 1 o 0 dependiendo si esta utilizada en otra estructura vigente o no vigente
            string estrvigente = result.FirstChild().GetAttr("estrvigente");
            string estrnovigente = result.FirstChild().GetAttr("estrnovigente");

            //Si la unidad organizativa no esta siendo utilizada por otra estructura vigente
            if (estrvigente == "0")
            {
                //Si esta siendo utilizada en una estructura no vigente o tiene legajos asignados como trayectoria solo se edita la posicion
                if (estrnovigente == "1" || flag == "1")
                {
                    try
                    {
                        //Establece la posición como no vigente
                        NomadEnvironment.GetCurrentTransaction().Begin();
                        NomadEnvironment.GetCurrentTransaction().Delete(ddoEstr);
                        ddoPOSICION.l_vigente = false;
                        ddoPOSICION.f_hasta_vigencia = DateTime.Now;
                        NomadEnvironment.GetCurrentTransaction().Save(ddoPOSICION);
                        NomadEnvironment.GetCurrentTransaction().Commit();

                    }
                    catch (Exception e)
                    {
                        NomadEnvironment.GetCurrentTransaction().Rollback();
                        return "Error al establecer la posición como no vigente. " + e.Message;
                    }

                }
                //Si no esta siendo utilizada en ninguna estructura y no tiene legajos asignados como trayectoria, se modifica posicion y se elimina la unidad organizativa
                else
                {
                    try
                    {                        
                        //Elimino primero el sector de la posicion - ya que necesito la persistencia en la base de datos
                        EliminarSectorPos(xmlposId.FirstChild().GetAttr("oi_posicion").ToString());
                        //Elimina la Posicion y remueve la estructura de la lista de los tipos de estructuras de posicion
                        NomadEnvironment.GetCurrentTransaction().Begin();
                        NomadEnvironment.GetCurrentTransaction().Delete(ddoEstr);
                        NomadEnvironment.GetCurrentTransaction().Delete(ddoPOSICION);
                        tipo_unidad.UNI_ORG.Remove(unidad);
                        NomadEnvironment.GetCurrentTransaction().Save(tipo_unidad);
                        NomadEnvironment.GetCurrentTransaction().Commit();                        
                    }
                    catch (Exception e)
                    {
                        NomadEnvironment.GetCurrentTransaction().Rollback();
                        return "Error al eliminar Posición y Unidad Organizativa asociada. " + e.Message;
                    }

                }

            }
            //Solo eliminamos la estructura
            else
            {
                try
                {
                    //Elimina la Estructura
                    NomadEnvironment.GetCurrentTransaction().Begin();
                    NomadEnvironment.GetCurrentTransaction().Delete(ddoEstr);
                    NomadEnvironment.GetCurrentTransaction().Commit();

                }
                catch (Exception e)
                {
                    NomadEnvironment.GetCurrentTransaction().Rollback();
                    return "Error al eliminar la Estructura. " + e.Message;
                }

            }

            return "";
        }
        public static void Editar_Posicion(string puesto_id, string posicion_id)
        {
            //OBTENGO EL PUESTO
            NucleusRH.Base.Organizacion.Puestos.PUESTO ddoPUESTO = NucleusRH.Base.Organizacion.Puestos.PUESTO.Get(puesto_id, false);

            //OBTENGO LA POSICION
            NucleusRH.Base.Organizacion.Puestos.POSICION ddoPOSICION = NucleusRH.Base.Organizacion.Puestos.POSICION.Get(posicion_id, false);

            //OBTENGO LA EMPRESA
            NucleusRH.Base.Organizacion.Empresas.EMPRESA ddoEMPRESA = NucleusRH.Base.Organizacion.Empresas.EMPRESA.Get(ddoPUESTO.oi_empresa, false);

            //CHEQUEA NOMBRE DE UNIDAD ORGANIZATIVA
            NucleusRH.Base.Organizacion.Unidades_Organizativas.UNIDAD_ORG unidad = NucleusRH.Base.Organizacion.Unidades_Organizativas.UNIDAD_ORG.Get(ddoPOSICION.oi_unidad_org);
            unidad.c_unidad_org = ddoEMPRESA.c_empresa + "-" + ddoPUESTO.c_puesto + "-" + ddoPOSICION.c_posicion;
            unidad.d_unidad_org = ddoPOSICION.d_posicion;
            unidad.o_unidad_org = ddoPOSICION.o_posicion;

            try
            {
                NomadEnvironment.GetCurrentTransaction().Save(unidad);
            }
            catch (Exception e)
            {
                throw new NomadAppException("Error Actualizando Unidad Organizativa: " + e.Message);
            }
        }
        public static string Validar_Hoja(string estructura_id)
        {
            //PRIMERO SE VALIDA QUE LA POSICION A ELIMINAR SEA UNA HOJA DENTRO DEL ORGANIGRAMA JERARQUICO
            //PARA SABER ESTO UTILIZO UN RECURSO
            NomadXML xmlflag;
            string param = "<DATO oi_estructura=\"" + estructura_id + "\"/>";
            xmlflag = new NomadXML();
            xmlflag.SetText(NomadEnvironment.QueryString(NucleusRH.Base.Organizacion.Puestos.POSICION.Resources.QRY_VALIDA_POSICION, param));
            NomadEnvironment.GetTrace().Info(xmlflag.ToString());

            //SI VUELVE UNO LA POSICION ES HOJA
            if (xmlflag.FirstChild().GetAttr("flag") == "1")
            {
                return "0";
            }
            else
            {
                return "1";
            }
        }
        public static string Validar_Legajos_Posicion(string estructura_id)
        {
            //SE VALIDA QUE LA POSICION A ELIMINAR NO ESTE ASGINADA EN NINGUN LEGAJO
            //PARA SABER ESTO UTILIZO UN RECURSO
            NomadXML xmlflag;
            string param = "<DATO oi_estructura=\"" + estructura_id + "\"/>";
            xmlflag = new NomadXML();
            xmlflag.SetText(NomadEnvironment.QueryString(NucleusRH.Base.Organizacion.Puestos.POSICION.Resources.QRY_POSICION_PER, param));
            NomadEnvironment.GetTrace().Info(xmlflag.ToString());

            //SI VUELVE 0 LA POSICION NO TIENE LEGAJOS ASIGNADOS
            //SI VUELVE 1 LA POSICION TIENE ASIGNADOS LEGAJOS COMO TRAYECTORIA
            //SI VUELVE 2 LA POSICION TIENE ASIGNADOS LEGAJOS ACTUALMENTE
            return xmlflag.FirstChild().GetAttr("flag");
        }
        public static void AddLegajoPlanificado(string estructura_id, string clase_id, string personal_emp_id)
        {
            //INSTANCIO EL LEGAJO
            NucleusRH.Base.Personal.LegajoEmpresa.PERSONAL_EMP ddoLEG;
            ddoLEG = NucleusRH.Base.Personal.LegajoEmpresa.PERSONAL_EMP.Get(personal_emp_id, false);

            //CARGO LA ESTRUCTURA
            NucleusRH.Base.Organizacion.Clases_Organizativas.ESTRUCTURA ddoEstr = NucleusRH.Base.Organizacion.Clases_Organizativas.ESTRUCTURA.Get(estructura_id, false);

            //CON UN QRY VERIFICO SI EL LEGAJO YA EXISTE EN LA CLASE
            NomadXML xmlflag;
            string paramflag = "<DATO oi_personal_emp=\"" + personal_emp_id + "\" oi_clase_org=\"" + clase_id + "\"/>";
            xmlflag = new NomadXML();
            xmlflag.SetText(NomadEnvironment.QueryString(NucleusRH.Base.Organizacion.Clases_Organizativas.CLASE_ORG.Resources.QRY_FLAG_LEGAJO, paramflag));
            NomadEnvironment.GetTrace().Info(xmlflag.ToString());
            //SI VUELVE UNO YA EXISTE EN EL ORGANIGRAMA
            if (xmlflag.FirstChild().GetAttr("flag") == "1")
                throw new NomadAppException("El Legajo seleccionado ya esta asignado en una Posicion de la Empresa");

            //AGREGO LA PERSONA A LA ESTRUCTURA
            //CREO UN NUEVO ESTRUCPERS
            NucleusRH.Base.Organizacion.Clases_Organizativas.ESTRUC_PERS ddoEstPer;
            ddoEstPer = new NucleusRH.Base.Organizacion.Clases_Organizativas.ESTRUC_PERS();
            ddoEstPer.oi_personal_emp = personal_emp_id;
            ddoEstPer.l_responsable = false;
            ddoEstPer.oi_clase_org = clase_id;
            ddoEstr.ESTRUC_PERS.Add(ddoEstPer);

            NomadEnvironment.GetCurrentTransaction().Save(ddoEstr);
        }
        public static void AddLegajo(string estructura_id, string clase_id, string personal_emp_id, DateTime fecha_ing, DateTime fecha_egr, string te_interno_1, string te_interno_2, string motivo_cambio_id, string o_cambio_posic)
        {
            DateTime fCompare = new DateTime(1900, 1, 1);

            //INSTANCIO EL LEGAJO
            NucleusRH.Base.Personal.LegajoEmpresa.PERSONAL_EMP ddoLEG;
            ddoLEG = NucleusRH.Base.Personal.LegajoEmpresa.PERSONAL_EMP.Get(personal_emp_id, false);

            //VALIDO LA FECHA DE INGRESO A LA POSICION CONTRA LA FECHA DE INGRESO DEL LEGAJO
            if (fecha_ing.Date < ddoLEG.f_ingreso.Date)
                throw new NomadAppException("La fecha de ingreso a la posicion debe ser posterior a la fecha de ingreso del Legajo en la Empresa (" + ddoLEG.f_ingreso.ToString("dd/MM/yyyy") + ")");

            //CARGO LA ESTRUCTURA
            NucleusRH.Base.Organizacion.Clases_Organizativas.ESTRUCTURA ddoEstr = NucleusRH.Base.Organizacion.Clases_Organizativas.ESTRUCTURA.Get(estructura_id, false);

            //CON UN QRY VERIFICO SI EL LEGAJO YA EXISTE EN LA CLASE
            NomadXML xmlflag;
            string paramflag = "<DATO oi_personal_emp=\"" + personal_emp_id + "\" oi_clase_org=\"" + clase_id + "\"/>";
            xmlflag = new NomadXML();
            xmlflag.SetText(NomadEnvironment.QueryString(NucleusRH.Base.Organizacion.Clases_Organizativas.CLASE_ORG.Resources.QRY_FLAG_LEGAJO, paramflag));
            NomadEnvironment.GetTrace().Info(xmlflag.ToString());
            //SI VUELVE UNO YA EXISTE EN EL ORGANIGRAMA
            if (xmlflag.FirstChild().GetAttr("flag") == "1")
            {
                throw new NomadAppException("El Legajo seleccionado ya esta asignado en una Posicion de la Empresa");
            }

            //TIRO UN QRY PARA SABER QUE POSICION Y QUE PUESTO CORRESPONDE A LA ESTRUCTURA
            NomadXML xmlposId;
            string param = "<DATO oi_estructura=\"" + estructura_id + "\"/>";
            xmlposId = new NomadXML();
            NomadEnvironment.GetTrace().Info(param.ToString());
            xmlposId.SetText(NomadEnvironment.QueryString(NucleusRH.Base.Organizacion.Puestos.POSICION.Resources.QRY_ESTRUCTURA_POSICION, param));
            NomadEnvironment.GetTrace().Info(xmlposId.ToString());

            //ME FIJO SI LA PERSONA TIENE CARGADA ALGUNA POSICION
            if (!ddoLEG.oi_posicion_ultNull && ddoLEG.POSIC_PER.Count>0)
            {
                //CARGO LA POSICION ULTIMAok
                NucleusRH.Base.Personal.LegajoEmpresa.POSICION_PER ddoPosicionAnterior = (NucleusRH.Base.Personal.LegajoEmpresa.POSICION_PER)ddoLEG.POSIC_PER[ddoLEG.POSIC_PER.Count - 1];
                //VALIDO LA FECHA DE INGRESO
                if (ddoPosicionAnterior.f_egreso >= fecha_ing)
                {
                    throw new NomadAppException("La fecha de ingreso a la posicion debe ser posterior a la fecha de egreso en la ultima posicion (" + ddoPosicionAnterior.f_egreso.ToString("dd/MM/yyyy") + ")");
                }
            }

            //ASIGNO EL PUESTO ULTIMO Y LA FECHA DE INGRESO AL MISMO EN EL LEGAJO
            ddoLEG.oi_puesto_ult = xmlposId.FirstChild().GetAttr("oi_puesto");
            ddoLEG.f_desde_puesto = fecha_ing;

            //ASIGNO LA POSICION ULTIMA Y LA FECHA DE INGRESO A LA MISMA EN EL LEGAJO
            ddoLEG.f_desde_posicion = fecha_ing;
            ddoLEG.oi_posicion_ult = xmlposId.FirstChild().GetAttr("oi_posicion");

            //CARGO EL NUEVO PUESTO CORRESPONDIENTE A LA POSICION AL LEGAJO
            //CREO UNA PERSONA PUESTO
            NucleusRH.Base.Personal.LegajoEmpresa.PUESTO_PER ddoPUEPER;
            ddoPUEPER = new NucleusRH.Base.Personal.LegajoEmpresa.PUESTO_PER();
            ddoPUEPER.f_ingreso = fecha_ing;
            ddoPUEPER.oi_personal_emp = int.Parse(personal_emp_id);
            ddoPUEPER.oi_puesto = xmlposId.FirstChild().GetAttr("oi_puesto");

            //CREO UNA PERSONA POSICION
            NucleusRH.Base.Personal.LegajoEmpresa.POSICION_PER ddoPOSPER;
            ddoPOSPER = new NucleusRH.Base.Personal.LegajoEmpresa.POSICION_PER();
            ddoPOSPER.f_ingreso = fecha_ing;
            ddoPOSPER.oi_personal_emp = int.Parse(personal_emp_id);
            ddoPOSPER.oi_posicion = xmlposId.FirstChild().GetAttr("oi_posicion");
            ddoPOSPER.te_interno_1 = te_interno_1;
            ddoPOSPER.te_interno_2 = te_interno_2;

            //EN CASO DE QUE LA ASIGNACION SEA CON FECHA DE EGRESO, NO AGREGO LA PERSONA AL ORGANIGRAMA
            if (fecha_egr == null || fecha_egr < fCompare)
            {
                //AGREGO LA PERSONA A LA ESTRUCTURA
                //CREO UN NUEVO ESTRUCPERS
                NucleusRH.Base.Organizacion.Clases_Organizativas.ESTRUC_PERS ddoEstPer;
                ddoEstPer = new NucleusRH.Base.Organizacion.Clases_Organizativas.ESTRUC_PERS();
                ddoEstPer.oi_personal_emp = personal_emp_id;
                ddoEstPer.l_responsable = false;
                ddoEstPer.oi_clase_org = clase_id;
                ddoEstr.ESTRUC_PERS.Add(ddoEstPer);
            }
            else
            {
                //EN CASO DE QUE LA ASIGNACION SEA CON FECHA DE EGRESO, PEGO LOS MOTIVOS DE CAMBIO
                ddoPOSPER.f_egreso = fecha_egr;
                ddoPOSPER.oi_motivo_cambio = motivo_cambio_id;
                ddoPOSPER.o_cambio_posic = o_cambio_posic;
                ddoPUEPER.f_egreso = fecha_egr;
                ddoPUEPER.oi_motivo_cambio = motivo_cambio_id;
                ddoPUEPER.o_cambio_puesto = o_cambio_posic;
            }

            ddoLEG.PUESTO_PER.Add(ddoPUEPER);
            ddoLEG.POSIC_PER.Add(ddoPOSPER);

            //INICIO UNA TRANSACCION PARA GUARDAR LOS CAMBIOS
            NomadEnvironment.GetCurrentTransaction().Begin();
            try
            {
                NomadEnvironment.GetCurrentTransaction().Save(ddoEstr);
                NomadEnvironment.GetCurrentTransaction().Save(ddoLEG);
                NomadEnvironment.GetCurrentTransaction().Commit();

        //Llamar al metodo de tipo evento
        AddLegajoSectores(xmlposId.FirstChild().GetAttr("oi_posicion"),personal_emp_id,fecha_ing,fecha_egr,motivo_cambio_id,o_cambio_posic);
            }
            catch (Exception e)
            {
                NomadEnvironment.GetCurrentTransaction().Rollback();
                throw new NomadAppException("Error al asignar el legajo. " + e.Message);
            }

        }

        public static void Cambiar_Pos_Legajo(string clase_id, string estructura_old_id, string estructura_new_id, string personal_emp_id, DateTime fecha_ing, string te_interno_1, string te_interno_2, string motivo_cambio_id, string o_cambio_posic)
        {

            //TIRO UN QRY PARA SABER QUE POSICION Y QUE PUESTO CORRESPONDE A LA ESTRUCTURA
            NomadXML xmlposId;
            string param = "<DATO oi_estructura=\"" + estructura_new_id + "\"/>";
            xmlposId = new NomadXML();
            NomadEnvironment.GetTrace().Info(param.ToString());
            xmlposId.SetText(NomadEnvironment.QueryString(NucleusRH.Base.Organizacion.Puestos.POSICION.Resources.QRY_ESTRUCTURA_POSICION, param));
            NomadEnvironment.GetTrace().Info(xmlposId.ToString());

            //INSTANCIO EL LEGAJO
            NucleusRH.Base.Personal.LegajoEmpresa.PERSONAL_EMP ddoLEG;
            ddoLEG = NucleusRH.Base.Personal.LegajoEmpresa.PERSONAL_EMP.Get(personal_emp_id, false);

            //TIRO UN QRY Y RECUPERO EL ID DEL ESTRUC PERS ANTERIOR
            NomadXML xmlperestrId;
            string paramestr = "<DATO oi_personal_emp=\"" + personal_emp_id + "\" oi_estructura=\"" + estructura_old_id + "\"/>";
            xmlperestrId = new NomadXML();
            NomadEnvironment.GetTrace().Info(paramestr.ToString());
            xmlperestrId.SetText(NomadEnvironment.QueryString(NucleusRH.Base.Organizacion.Puestos.POSICION.Resources.QRY_ESTRUCTURA_PERSONA, paramestr));
            NomadEnvironment.GetTrace().Info(xmlperestrId.ToString());

            /*
            //ELIMINO AL LEGAJO DE LA ESTRUCTURA ANTERIOR
            NucleusRH.Base.Organizacion.Clases_Organizativas.ESTRUC_PERS ddoESTRPERDEL;
            ddoESTRPERDEL = (NucleusRH.Base.Organizacion.Clases_Organizativas.ESTRUC_PERS)ddoEstrOld.ESTRUC_PERS.GetById(xmlperestrId.FirstChild().GetAttr("oi_estruc_pers"));
            ddoEstrOld.ESTRUC_PERS.Remove(ddoESTRPERDEL);
            */

            //Se obtiene la Estructura Pers ANTERIOR
            //NucleusRH.Base.Organizacion.Clases_Organizativas.ESTRUCTURA ddoEstrOld = NucleusRH.Base.Organizacion.Clases_Organizativas.ESTRUCTURA.Get(estructura_old_id, false);
            NucleusRH.Base.Organizacion.Estructuras2.ESTRUC_PERS_AUX ddoEstrPer = NucleusRH.Base.Organizacion.Estructuras2.ESTRUC_PERS_AUX.Get(xmlperestrId.FirstChild().GetAttr("oi_estruc_pers"), false);

            //ASIGNO LA POSICION ULTIMA Y LA FECHA DE INGRESO A LA MISMA EN EL LEGAJO
            ddoLEG.f_desde_posicion = fecha_ing;
            ddoLEG.oi_posicion_ult = xmlposId.FirstChild().GetAttr("oi_posicion");

            //ME FIJO SI EL PUESTO ULTIMO EN EL LEGAJO COINCIDE CON EL PUESTO DE LA POSICION
            if (ddoLEG.oi_puesto_ult != xmlposId.FirstChild().GetAttr("oi_puesto"))
            {
                //NO COINCIDE, DEBO CERRAR EL PUESTO ANTERIOR
                if (ddoLEG.PUESTO_PER.Count > 0)
                {
                    NucleusRH.Base.Personal.LegajoEmpresa.PUESTO_PER ddoPuestoAnterior = (NucleusRH.Base.Personal.LegajoEmpresa.PUESTO_PER)ddoLEG.PUESTO_PER[ddoLEG.PUESTO_PER.Count - 1];
                    //ASIGNO LA FECHA DE EGRESO DEL PUESTO ANTERIOR COMO LA FECHA DE INGRESO A LA POSICION
                    ddoPuestoAnterior.f_egreso = fecha_ing;
                    //ASIGNO EL MOTIVO DE CAMBIO POR DEFECTO 1
                    ddoPuestoAnterior.oi_motivo_cambio = motivo_cambio_id;
                    //SETEO LA OBSERVACION DEL CAMBIO
                    ddoPuestoAnterior.o_cambio_puesto = o_cambio_posic;
                }
                //CARGO EL NUEVO PUESTO CORRESPONDIENTE A LA POSICION AL LEGAJO
                //CREO UNA PERSONA PUESTO
                NucleusRH.Base.Personal.LegajoEmpresa.PUESTO_PER ddoPUEPER;
                ddoPUEPER = new NucleusRH.Base.Personal.LegajoEmpresa.PUESTO_PER();
                ddoPUEPER.f_ingreso = fecha_ing;
                ddoPUEPER.oi_personal_emp = int.Parse(personal_emp_id);
                ddoPUEPER.oi_puesto = xmlposId.FirstChild().GetAttr("oi_puesto");
                ddoLEG.PUESTO_PER.Add(ddoPUEPER);

                //ASIGNO EL PUESTO ULTIMO Y LA FECHA DE INGRESO AL MISMO EN EL LEGAJO
                ddoLEG.oi_puesto_ult = xmlposId.FirstChild().GetAttr("oi_puesto");
                ddoLEG.f_desde_puesto = fecha_ing;
            }

            //CARGO LA POSICION ANTERIOR
            NucleusRH.Base.Personal.LegajoEmpresa.POSICION_PER ddoPosicionAnterior = (NucleusRH.Base.Personal.LegajoEmpresa.POSICION_PER)ddoLEG.POSIC_PER[ddoLEG.POSIC_PER.Count - 1];
            //VALIDO LA FEHCA DE INGRESO
            if (ddoPosicionAnterior.f_ingreso >= fecha_ing)
            {
                throw new NomadAppException("La fecha de ingreso a la posicion debe ser posterior a la fecha de ingreso en la ultima posicion (" + ddoPosicionAnterior.f_ingreso.ToString("dd/MM/yyyy") + ")");
            }
            else
            {
                //CIERRO LA POSICION ANTERIOR
                //ASIGNO LA FECHA DE EGRESO DEL PUESTO ANTERIOR COMO LA FECHA DE INGRESO A LA POSICION
                ddoPosicionAnterior.f_egreso = fecha_ing;
                //ASIGNO EL MOTIVO DE CAMBIO
                ddoPosicionAnterior.oi_motivo_cambio = motivo_cambio_id;
                //SETEO LA OBSERVACION DEL CAMBIO
                ddoPosicionAnterior.o_cambio_posic = o_cambio_posic;

                //AGREGO LA PERSONA A LA POSICION Y A LA ESTRUCTURA
                //Se cambia de PADRE la estruc_per anterior y se le asigna el nuevo padre
                ddoEstrPer.l_responsable = false;
                ddoEstrPer.oi_estructura = int.Parse(estructura_new_id);
                //CREO UNA PERSONA POSICION
                NucleusRH.Base.Personal.LegajoEmpresa.POSICION_PER ddoPOSPER;
                ddoPOSPER = new NucleusRH.Base.Personal.LegajoEmpresa.POSICION_PER();
                ddoPOSPER.f_ingreso = fecha_ing;
                ddoPOSPER.oi_personal_emp = int.Parse(personal_emp_id);
                ddoPOSPER.oi_posicion = xmlposId.FirstChild().GetAttr("oi_posicion");
                ddoPOSPER.te_interno_1 = te_interno_1;
                ddoPOSPER.te_interno_2 = te_interno_2;

                ddoLEG.POSIC_PER.Add(ddoPOSPER);

                //INICIO UNA TRANSACCION PARA GUARDAR LOS CAMBIOS
                NomadEnvironment.GetCurrentTransaction().Begin();
                try {
                    NomadEnvironment.GetCurrentTransaction().Save(ddoEstrPer);
                    NomadEnvironment.GetCurrentTransaction().Save(ddoLEG);
                    NomadEnvironment.GetCurrentTransaction().Commit();
            //Llamar al metodo de tipo evento
            Cambiar_Pos_Legajo_Sectores(ddoPosicionAnterior.oi_posicion,xmlposId.FirstChild().GetAttr("oi_posicion"),personal_emp_id,fecha_ing,motivo_cambio_id,o_cambio_posic);
                } catch (Exception e) {
                    //Realiza el rollback de la transaccion
                    NomadEnvironment.GetCurrentTransaction().Rollback();

                    //Guarda los parmetros para que se realice el DUMP
                    NomadException nmdEx = NomadException.NewInternalException("POSICION.Cambiar_Pos_Legajo", e);
          nmdEx.SetValue("clase_id", clase_id);
          nmdEx.SetValue("estructura_old_id", estructura_old_id);
          nmdEx.SetValue("estructura_new_id", estructura_new_id);
          nmdEx.SetValue("personal_emp_id", personal_emp_id);
          nmdEx.SetValue("fecha_ing", fecha_ing.ToString("yyyyMMdd"));
          nmdEx.SetValue("te_interno_1", te_interno_1);
          nmdEx.SetValue("te_interno_2", te_interno_2);
          nmdEx.SetValue("motivo_cambio_id", motivo_cambio_id);
          nmdEx.SetValue("o_cambio_posic", o_cambio_posic);
                    nmdEx.SetValue("xmlposId", xmlposId.ToString());

          throw nmdEx;

                    //throw new NomadAppException("Error al cambiar de Posicion al Legajo. " + e.Message);
                }
            }
        }

        public static void Mover_Posicion(string estructura_change_id, string estructura_to_id)
        {
            //CARGO EL DDO DE LA ESTRUCTURA A MOVER
            NucleusRH.Base.Organizacion.Estructuras.ESTRUCAUX ddoESTRMOVE = NucleusRH.Base.Organizacion.Estructuras.ESTRUCAUX.Get(estructura_change_id);

            //CARGO LOS DATOS
            ddoESTRMOVE.oi_estr_padre = int.Parse(estructura_to_id);

            //GUARDO
            NomadEnvironment.GetCurrentTransaction().Save(ddoESTRMOVE);
        }
        public static void Egresar_Pos_Legajo(string estructura_id, string personal_emp_id, DateTime fecha_egr, string motivo_cambio_id, string o_cambio_posic)
        {
            //INSTANCIO EL LEGAJO
            NucleusRH.Base.Personal.LegajoEmpresa.PERSONAL_EMP ddoLEG;
            ddoLEG = NucleusRH.Base.Personal.LegajoEmpresa.PERSONAL_EMP.Get(personal_emp_id, false);

            //SETEO EL PUESTO ULTIMO Y LA POSICION ULTIMA DEL LEGAJO
            ddoLEG.oi_posicion_ultNull = true;
            ddoLEG.oi_puesto_ultNull = true;
            ddoLEG.f_desde_posicionNull = true;
            ddoLEG.f_desde_puestoNull = true;

            //OBTENGO EL ID DE LA ULTIMA Y PENULTIMA POSICION/PUESTO DEL LEGAJO
            string paramPER = "<DATO oi_personal_emp=\"" + personal_emp_id + "\"/>";
            NomadXML xmlLEGPOSId = new NomadXML();
            xmlLEGPOSId.SetText(NomadEnvironment.QueryString(NucleusRH.Base.Organizacion.Puestos.POSICION.Resources.QRY_LEGAJO_POSICION, paramPER));

            //INSTANCIO EL LEGAJO POSICION A ELIMINAR
            NucleusRH.Base.Personal.LegajoEmpresa.POSICION_PER ddoPOSPER = (NucleusRH.Base.Personal.LegajoEmpresa.POSICION_PER)ddoLEG.POSIC_PER.GetById(xmlLEGPOSId.FirstChild().GetAttr("oi_posicion_ult"));

            //VALIDO LA FECHA DE EGRESO CONTRA LA FECHA DE INGRESO
            if (fecha_egr < ddoPOSPER.f_ingreso)
                throw new NomadAppException("La fecha de egreso de la posicion debe ser posterior a la fecha de ingreso a la misma (" + ddoPOSPER.f_ingreso.ToString("dd/MM/yyyy") + ")");

            //SETEO LOS ATRIBUTOS DEL EGRESO
            ddoPOSPER.f_egreso = fecha_egr;
            ddoPOSPER.oi_motivo_cambio = motivo_cambio_id;
            ddoPOSPER.o_cambio_posic = o_cambio_posic;

            //INSTANCIO EL LEGAJO PUESTO
            NucleusRH.Base.Personal.LegajoEmpresa.PUESTO_PER ddoPUEPER = (NucleusRH.Base.Personal.LegajoEmpresa.PUESTO_PER)ddoLEG.PUESTO_PER.GetById(xmlLEGPOSId.FirstChild().GetAttr("oi_puesto_ult"));

            //SETEO LOS ATRIBUTOS DEL EGRESO
            ddoPUEPER.f_egreso = fecha_egr;
            ddoPUEPER.oi_motivo_cambio = motivo_cambio_id;
            ddoPUEPER.o_cambio_puesto = o_cambio_posic;

            //TIRO UN QRY Y RECUPERO EL ID DEL ESTRUC PERS
            NomadXML xmlperestrId;
            string paramestr = "<DATO oi_personal_emp=\"" + personal_emp_id + "\" oi_estructura=\"" + estructura_id + "\"/>";
            xmlperestrId = new NomadXML();
            xmlperestrId.SetText(NomadEnvironment.QueryString(NucleusRH.Base.Organizacion.Puestos.POSICION.Resources.QRY_ESTRUCTURA_PERSONA, paramestr));

            //QUITO A LA PERSONA DE LA ESTRUCTURA EN LA QUE ESTA
            NucleusRH.Base.Organizacion.Clases_Organizativas.ESTRUC_PERS ddoESTRPER = NucleusRH.Base.Organizacion.Clases_Organizativas.ESTRUC_PERS.Get(xmlperestrId.FirstChild().GetAttr("oi_estruc_pers"), false);

            //INICIO UNA TRANSACCION PARA GUARDAR LOS CAMBIOS
            NomadEnvironment.GetCurrentTransaction().Begin();
            try
            {
                NomadEnvironment.GetCurrentTransaction().Delete(ddoESTRPER);
                NomadEnvironment.GetCurrentTransaction().Save(ddoLEG);
                NomadEnvironment.GetCurrentTransaction().Commit();
            //Llamar al metodo de tipo evento
            Egresar_Pos_Legajo_Sectores(ddoPOSPER.oi_posicion, personal_emp_id, fecha_egr, motivo_cambio_id, o_cambio_posic);
            }
            catch (Exception e)
            {
                NomadEnvironment.GetCurrentTransaction().Rollback();
                throw new NomadAppException("Error al egresar al Legajo de la Posicion. " + e.Message);
            }
        }
        public static void Del_Pos_Legajo(string estructura_id, string clase_id, string personal_emp_id)
        {
            string oi_posicion_ult, oi_posicion_ant;

            //DEFINE LA ESTRUCTURA
            //NucleusRH.Base.Organizacion.Clases_Organizativas.ESTRUCTURA ddoEstr = null;
            NucleusRH.Base.Organizacion.Estructuras2.ESTRUC_PERS_AUX ddoEstrPerOld = null;
            NucleusRH.Base.Organizacion.Estructuras2.ESTRUC_PERS_AUX ddoEstrPerNew = null;

            //INSTANCIO EL LEGAJO
            NucleusRH.Base.Personal.LegajoEmpresa.PERSONAL_EMP ddoLEG;
            ddoLEG = NucleusRH.Base.Personal.LegajoEmpresa.PERSONAL_EMP.Get(personal_emp_id, false);

            //Busca cual es la estruc_pers en la que actualmente esta la persona
            {
                NomadXML xmlEstrucPers = null;
                try {
                    NomadXML xmlParam = new NomadXML("PARAM");
                    xmlParam.SetAttr("oi_clase_org", clase_id);
                    xmlParam.SetAttr("oi_estructura", estructura_id);
                    xmlParam.SetAttr("oi_personal_emp", personal_emp_id);

                    xmlEstrucPers = NomadEnvironment.QueryNomadXML(NucleusRH.Base.Organizacion.Estructuras2.ESTRUC_PERS_AUX.Resources.QRY_ID_ESTRUCTURA_PERS, xmlParam.ToString());

                    if (xmlEstrucPers.FirstChild().GetAttr("oi_estruc_pers") == "") throw new Exception("No se encontró el OI para la estructura_pers");

                    ddoEstrPerOld = NucleusRH.Base.Organizacion.Estructuras2.ESTRUC_PERS_AUX.Get(xmlEstrucPers.FirstChild().GetAttr("oi_estruc_pers"), false);

                } catch (Exception e) {
                    //Guarda los parmetros para que se realice el DUMP
                    NomadException nmdEx = NomadException.NewInternalException("POSICION.Del_Pos_Legajo", e);
                    nmdEx.SetValue("estructura_id", estructura_id);
                    nmdEx.SetValue("clase_id", clase_id);
                    nmdEx.SetValue("personal_emp_id", personal_emp_id);
                    if (xmlEstrucPers != null)
                        nmdEx.SetValue("xmlEstrucPers", xmlEstrucPers.ToString());
                    throw nmdEx;
                }
            }

            //ME FIJO SI EL LEGAJO TIENE UNA SOLA POSICION ASIGNADA, DE SER ASI TENGO QUE BORRAR TODOS LOS REGISTROS DE PUESTO
            //POSICION Y ESTRUCTURA EN EL ORGANIRGAMA JERARQUICO PARA EL LEGAJO
            if (ddoLEG.POSIC_PER.Count == 1)
            {
                oi_posicion_ult = ddoLEG.oi_posicion_ult;
                oi_posicion_ant = "";

                ddoLEG.oi_posicion_ultNull = true;
                ddoLEG.oi_puesto_ultNull = true;
                ddoLEG.f_desde_posicionNull = true;
                ddoLEG.f_desde_puestoNull = true;
                ddoLEG.POSIC_PER.Clear();
                ddoLEG.PUESTO_PER.Clear();

                /*
                //CARGO LA ESTRUCTURA
                ddoEstr = NucleusRH.Base.Organizacion.Clases_Organizativas.ESTRUCTURA.Get(estructura_id, false);

                //QUITO A LA PERSONA DE LA ESTRUCTURA EN LA QUE ESTA
                NucleusRH.Base.Organizacion.Clases_Organizativas.ESTRUC_PERS ddoESTRPER;
                ddoESTRPER = (NucleusRH.Base.Organizacion.Clases_Organizativas.ESTRUC_PERS)ddoEstr.ESTRUC_PERS.GetByAttribute("oi_personal_emp", personal_emp_id);
                ddoEstr.ESTRUC_PERS.Remove(ddoESTRPER);
                */

            }
            else
            {
                //OBTENGO EL ID DE LA ULTIMA Y PENULTIMA POSICION/PUESTO DEL LEGAJO
                string paramPER = "<DATO oi_personal_emp=\"" + personal_emp_id + "\"/>";
                NomadXML xmlLEGPOSId = new NomadXML();
                xmlLEGPOSId.SetText(NomadEnvironment.QueryString(NucleusRH.Base.Organizacion.Puestos.POSICION.Resources.QRY_LEGAJO_POSICION, paramPER));

                //INSTANCIO EL LEGAJO POSICION A ELIMINAR
                NucleusRH.Base.Personal.LegajoEmpresa.POSICION_PER ddoPOSPERdel = (NucleusRH.Base.Personal.LegajoEmpresa.POSICION_PER)ddoLEG.POSIC_PER.GetById(xmlLEGPOSId.FirstChild().GetAttr("oi_posicion_ult"));
                oi_posicion_ult = ddoPOSPERdel.oi_posicion;
                
                //INSTANCIO EL NUEVO LEGAJO POSICION QUE ES EL ANTERIOR AL QUE SE VA A ELIMINAR
                NucleusRH.Base.Personal.LegajoEmpresa.POSICION_PER ddoPOSPERnew = (NucleusRH.Base.Personal.LegajoEmpresa.POSICION_PER)ddoLEG.POSIC_PER.GetById(xmlLEGPOSId.FirstChild().GetAttr("oi_posicion_ant"));
                oi_posicion_ant = ddoPOSPERnew.oi_posicion;

                //ELIMINO EL LEGAJO POSICION DE LA COLECCCION
                ddoLEG.POSIC_PER.Remove(ddoPOSPERdel);

                //SETEO LOS ATRIBUTOS PARA LA NUEVA POSICION VIGENTE EN EL LEGAJO
                ddoPOSPERnew.f_egresoNull = true;
                ddoPOSPERnew.oi_motivo_cambioNull = true;

                //SETEO POSICION ULTIMA EN EL LEGAJO
                ddoLEG.oi_posicion_ult = ddoPOSPERnew.oi_posicion;
                ddoLEG.f_desde_posicion = ddoPOSPERnew.f_ingreso;

                //RECUPERO EL PUESTO PARA LAS DOS POSICIONES Y VALIDO SI ES EL MISMO
                NucleusRH.Base.Organizacion.Puestos.POSICION ddoPOSdel;
                NucleusRH.Base.Organizacion.Puestos.POSICION ddoPOSnew;
                ddoPOSdel = NucleusRH.Base.Organizacion.Puestos.POSICION.Get(ddoPOSPERdel.oi_posicion, false);
                ddoPOSnew = NucleusRH.Base.Organizacion.Puestos.POSICION.Get(ddoPOSPERnew.oi_posicion, false);

                //SI EL PUESTO CAMBIA SETEO LOS NUEVOS VALORES
                if (ddoPOSnew.oi_puesto != ddoPOSdel.oi_puesto) {
                    //INSTANCIO EL LEGAJO PUESTO A ELIMINAR
                    NucleusRH.Base.Personal.LegajoEmpresa.PUESTO_PER ddoPUEPERdel = (NucleusRH.Base.Personal.LegajoEmpresa.PUESTO_PER)ddoLEG.PUESTO_PER.GetById(xmlLEGPOSId.FirstChild().GetAttr("oi_puesto_ult"));

                    //INSTANCIO EL NUEVO LEGAJO PUESTO QUE ES EL ANTERIOR AL QUE SE VA A ELIMINAR
                    NucleusRH.Base.Personal.LegajoEmpresa.PUESTO_PER ddoPUEPERnew = (NucleusRH.Base.Personal.LegajoEmpresa.PUESTO_PER)ddoLEG.PUESTO_PER.GetById(xmlLEGPOSId.FirstChild().GetAttr("oi_puesto_ant"));

                    //ELIMINO EL LEGAJO PUESTO DE LA COLECCCION
                    ddoLEG.PUESTO_PER.Remove(ddoPUEPERdel);

                    //SETEO LOS ATRIBUTOS PARA LA NUEVA POSICION VIGENTE EN EL LEGAJO
                    ddoPUEPERnew.f_egresoNull = true;
                    ddoPUEPERnew.oi_motivo_cambioNull = true;

                    //SETEO EL ULTIMO PUESTO EN EL LEGAJO
                    ddoLEG.oi_puesto_ult = ddoPUEPERnew.oi_puesto;
                    ddoLEG.f_desde_puesto = ddoPOSPERnew.f_ingreso;
                }

                //AGREGO A LA PERSONA EN LA ESTRUCTURA DE LA NUEVA POSICION QUE TIENE VIGENTE, SI ES QUE LA POSICION ESTA EN EL ORGANIGRAMA
                //TENGO QUE RECUPERAR EL OI_ESTRUCTURA PARA ESTA POSICION TIRO UN QRY
                NomadXML xmlestrId;
                string parampos = "<DATO  oi_clase_org=\"" + clase_id + "\" oi_posicion=\"" + ddoPOSPERnew.oi_posicion + "\"/>";
                xmlestrId = new NomadXML();

                xmlestrId.SetText(NomadEnvironment.QueryString(NucleusRH.Base.Organizacion.Puestos.POSICION.Resources.QRY_POSICION_ESTRUCTURA, parampos));

                if (xmlestrId.FirstChild().GetAttr("oi_estructura") != "") {
                    /*
                    //CARGO LA ESTRUCTURA
                    NucleusRH.Base.Organizacion.Clases_Organizativas.ESTRUCTURA ddoEstrNew = NucleusRH.Base.Organizacion.Clases_Organizativas.ESTRUCTURA.Get(xmlestrId.FirstChild().GetAttr("oi_estructura"), false);

                    //CREO UN NUEVO ESTRUCPERS
                    NucleusRH.Base.Organizacion.Clases_Organizativas.ESTRUC_PERS ddoEstPer;
                    ddoEstPer = new NucleusRH.Base.Organizacion.Clases_Organizativas.ESTRUC_PERS();
                    ddoEstPer.oi_personal_emp = personal_emp_id;
                    ddoEstPer.l_responsable = false;
                    ddoEstPer.oi_clase_org = clase_id;
                    ddoEstrNew.ESTRUC_PERS.Add(ddoEstPer);

                    NomadEnvironment.GetCurrentTransaction().Save(ddoEstrNew);
                    */

                    //Se crea la estructura para la posicion en que quedo
                    ddoEstrPerNew = new NucleusRH.Base.Organizacion.Estructuras2.ESTRUC_PERS_AUX();
                    ddoEstrPerNew.oi_clase_org = int.Parse(clase_id);
                    ddoEstrPerNew.oi_estructura = int.Parse(xmlestrId.FirstChild().GetAttr("oi_estructura"));
                    ddoEstrPerNew.oi_personal_emp = int.Parse(personal_emp_id);
                    ddoEstrPerNew.l_responsable = false;
                }

                /*
                //CARGO LA ESTRUCTURA
                ddoEstr = NucleusRH.Base.Organizacion.Clases_Organizativas.ESTRUCTURA.Get(estructura_id, false);

                //QUITO A LA PERSONA DE LA ESTRUCTURA EN LA QUE ESTA
                NucleusRH.Base.Organizacion.Clases_Organizativas.ESTRUC_PERS ddoESTRPER;
                ddoESTRPER = (NucleusRH.Base.Organizacion.Clases_Organizativas.ESTRUC_PERS)ddoEstr.ESTRUC_PERS.GetByAttribute("oi_personal_emp", personal_emp_id);
                ddoEstr.ESTRUC_PERS.Remove(ddoESTRPER);
                */

            }

            //INICIO UNA TRANSACCION PARA GUARDAR LOS CAMBIOS
            NomadEnvironment.GetCurrentTransaction().Begin();

            try {
                NomadEnvironment.GetCurrentTransaction().Delete(ddoEstrPerOld);
                if (ddoEstrPerNew != null) NomadEnvironment.GetCurrentTransaction().Save(ddoEstrPerNew);
                NomadEnvironment.GetCurrentTransaction().Save(ddoLEG);
                NomadEnvironment.GetCurrentTransaction().Commit();

            //Llamar al metodo de tipo evento
                Del_Pos_Legajo_Sectores(oi_posicion_ult,oi_posicion_ant,personal_emp_id);

            } catch (Exception e) {
                NomadEnvironment.GetCurrentTransaction().Rollback();

                //Guarda los parmetros para que se realice el DUMP
                NomadException nmdEx = NomadException.NewInternalException("POSICION.Del_Pos_Legajo", e);
                nmdEx.SetValue("estructura_id", estructura_id);
                nmdEx.SetValue("clase_id", clase_id);
                nmdEx.SetValue("personal_emp_id", personal_emp_id);
                throw nmdEx;
            }
        }

        public static void Del_Pos_Legajo_Planificado(string estructura_id, string personal_emp_id)
        {
            //CARGO LA ESTRUCTURA
            NucleusRH.Base.Organizacion.Clases_Organizativas.ESTRUCTURA ddoEstr = NucleusRH.Base.Organizacion.Clases_Organizativas.ESTRUCTURA.Get(estructura_id, false);

            //QUITO A LA PERSONA DE LA ESTRUCTURA EN LA QUE ESTA
            NucleusRH.Base.Organizacion.Clases_Organizativas.ESTRUC_PERS ddoESTRPER;
            ddoESTRPER = (NucleusRH.Base.Organizacion.Clases_Organizativas.ESTRUC_PERS)ddoEstr.ESTRUC_PERS.GetByAttribute("oi_personal_emp", personal_emp_id);
            ddoEstr.ESTRUC_PERS.Remove(ddoESTRPER);

            NomadEnvironment.GetCurrentTransaction().Save(ddoEstr);
        }
        public static void SetStaff(string estructura_id, string estado)
        {
            NomadXML xmlposId;
            string param = "<DATO oi_estructura=\"" + estructura_id + "\"/>";
            xmlposId = new NomadXML();
            NomadEnvironment.GetTrace().Info(param);
            xmlposId.SetText(NomadEnvironment.QueryString(NucleusRH.Base.Organizacion.Puestos.POSICION.Resources.QRY_ESTRUCTURA_POSICION, param));
            NomadEnvironment.GetTrace().Info("xmlposId -- " + xmlposId.ToString());

            //LLAMO AL METODO QUE RECORRE LA CLASE ORGANZATIVA HASTA LA ESTRUCTURA SELECCIONADA
            NucleusRH.Base.Organizacion.Clases_Organizativas.ESTRUCTURA ddoEstr = NucleusRH.Base.Organizacion.Clases_Organizativas.ESTRUCTURA.Get(estructura_id, false);
            //Marco STAFF
            ddoEstr.l_staff = (ddoEstr.l_staff) ? false : true;

            if (estado == "V")
            {
                NucleusRH.Base.Organizacion.Puestos.POSICION ddoPOSICION = NucleusRH.Base.Organizacion.Puestos.POSICION.Get(xmlposId.FirstChild().GetAttr("oi_posicion"), false);
                ddoPOSICION.l_staff = (ddoPOSICION.l_staff) ? false : true;

                NomadEnvironment.GetCurrentTransaction().Begin();
                //GUARDO LOS CAMBIOS
                try
                {
                    NomadEnvironment.GetCurrentTransaction().Save(ddoPOSICION);
                    NomadEnvironment.GetCurrentTransaction().Save(ddoEstr);
                    NomadEnvironment.GetCurrentTransaction().Commit();
                }
                catch (Exception e)
                {
                    NomadEnvironment.GetCurrentTransaction().Rollback();
                    throw new NomadAppException("Error actualizando datos: " + e.Message);
                }
            }
            else
                NomadEnvironment.GetCurrentTransaction().Save(ddoEstr);
        }

        //Agrega una estructura hija con la posición elegida a la estructura seleccionada
        public static void Asignar_Posicion(Nomad.NSystem.Proxy.NomadXML param)
        {
            param = param.FirstChild();

            //INSTANCIO EL PUESTO
            NucleusRH.Base.Organizacion.Puestos.PUESTO ddoPUESTO;
            ddoPUESTO = NucleusRH.Base.Organizacion.Puestos.PUESTO.Get(param.GetAttrInt("oi_puesto"), false);

            //INSTANCIO LA POSICION
            NucleusRH.Base.Organizacion.Puestos.POSICION ddoPOSICION;
            ddoPOSICION = NucleusRH.Base.Organizacion.Puestos.POSICION.Get(param.GetAttrInt("oi_posicion"), false);

            //SECCION DE CODIGO PARA CREAR LA UNIDAD ORGANIZATIVA CORRESPONDIENTE A LA POSICION
            NucleusRH.Base.Organizacion.Unidades_Organizativas.UNIDAD_ORG unidad;
            NucleusRH.Base.Organizacion.Unidades_Organizativas.TIPO_UNI_ORG tipo_unidad;
            tipo_unidad = NucleusRH.Base.Organizacion.Unidades_Organizativas.TIPO_UNI_ORG.Get("3", false);

            //OBTENGO LA EMPRESA PARA PODER SACAR EL CODIGO
            NucleusRH.Base.Organizacion.Empresas.EMPRESA ddoEMPRESA;
            ddoEMPRESA = NucleusRH.Base.Organizacion.Empresas.EMPRESA.Get(ddoPUESTO.oi_empresa, false);

            //GENERO EL CODIGO DE UNIDAD ORGANIZATIVA A PARTIR DE LA EMPRESA, EL PUESTO Y LA POSICION
            string cod = ddoEMPRESA.c_empresa + "-" + ddoPUESTO.c_puesto + "-" + ddoPOSICION.c_posicion;

            // OBTENGO UNA UNIDAD BASURA EN LA MISMA POSICION
            unidad = (NucleusRH.Base.Organizacion.Unidades_Organizativas.UNIDAD_ORG)tipo_unidad.UNI_ORG.GetById(ddoPOSICION.oi_unidad_org);

            // SI NO EXISTE CREO UNA NUEVA
            if (unidad == null)
            {
                unidad = new NucleusRH.Base.Organizacion.Unidades_Organizativas.UNIDAD_ORG();

                // ASIGNO LOS VALORES
                unidad.c_unidad_org = cod;
                unidad.d_unidad_org = ddoPOSICION.d_posicion;
                unidad.o_unidad_org = ddoPOSICION.o_posicion;
                unidad.oi_puesto = ddoPUESTO.id;
                unidad.oi_clase_org = param.GetAttrInt("oi_clase_org");
                tipo_unidad.UNI_ORG.Add(unidad);

                // GRABO
                try
                {
                    NomadEnvironment.GetCurrentTransaction().SaveRefresh(tipo_unidad);
                }
                catch (Exception e)
                {
                    throw new NomadAppException("Error Guardando la Unidad Organizativa de Posicion: " + e.Message);
                }

                //MODIFICO POSICION
                ddoPOSICION.l_vigente = true;
                ddoPOSICION.f_hasta_vigenciaNull = true;
                ddoPOSICION.oi_unidad_org =((NucleusRH.Base.Organizacion.Unidades_Organizativas.UNIDAD_ORG)tipo_unidad.UNI_ORG.GetByAttribute("c_unidad_org", cod, false)).Id;

            }
            else
            {
                //SE MODIFICA LA CLASE ORGANIZATIVA ULTIMA
                unidad.oi_clase_org = param.GetAttrInt("oi_clase_org");
                ddoPOSICION.oi_unidad_org = ((NucleusRH.Base.Organizacion.Unidades_Organizativas.UNIDAD_ORG)tipo_unidad.UNI_ORG.GetByAttribute("c_unidad_org", cod, false)).Id;
                ddoPOSICION.l_vigente = true;
                ddoPOSICION.f_hasta_vigenciaNull = true;

                // GRABO
                try
                {
                    NomadEnvironment.GetCurrentTransaction().SaveRefresh(tipo_unidad);
                }
                catch (Exception e)
                {
                    throw new NomadAppException("Error Guardando la Unidad Organizativa de Posicion: " + e.Message);
                }

            }

            //SECCION QUE SE ENCARGA DE INSTANCIAR LA CLASE ORGANIZATIVA PARA PODER AGREGAR LA ESTRUCTURA
            //LLAMO AL METODO QUE RECORRE LA CLASE ORGANZATIVA HASTA LA ESTRUCTURA SELECCIONADA
            NucleusRH.Base.Organizacion.Clases_Organizativas.ESTRUCTURA ddoEstr = NucleusRH.Base.Organizacion.Clases_Organizativas.ESTRUCTURA.Get(param.GetAttrInt("oi_estructura"), false);

            //CREO UNA NUEVA ESTRUCTURA
            NucleusRH.Base.Organizacion.Clases_Organizativas.ESTRUCTURA ddoEstNew;
            ddoEstNew = new NucleusRH.Base.Organizacion.Clases_Organizativas.ESTRUCTURA();
            ddoEstNew.oi_unidad_org = ddoPOSICION.oi_unidad_org;
            ddoEstNew.l_staff = ddoPOSICION.l_staff;

            //AGREGO LA ESTRUCTURA A LA ESTRUCTURA PADRE SELECCIONADA
            ddoEstr.ESTRUCTURAS.Add(ddoEstNew);

            //GRABO POSICION Y ESTRUCTURA
            NomadEnvironment.GetCurrentTransaction().SaveRefresh(ddoPOSICION);
            NomadEnvironment.GetCurrentTransaction().SaveRefresh(ddoEstr);

        }

        //Asigna una posicion a la estructura seleccionada
        public static void Cambiar_Posicion(Nomad.NSystem.Proxy.NomadXML param)
        {
            param = param.FirstChild();

            //INSTANCIO EL PUESTO
            NucleusRH.Base.Organizacion.Puestos.PUESTO ddoPUESTO;
            ddoPUESTO = NucleusRH.Base.Organizacion.Puestos.PUESTO.Get(param.GetAttrInt("oi_puesto"), false);

            //INSTANCIO LA POSICION
            NucleusRH.Base.Organizacion.Puestos.POSICION ddoPOSICION;
            ddoPOSICION = NucleusRH.Base.Organizacion.Puestos.POSICION.Get(param.GetAttrInt("oi_posicion"), false);

            //SECCION DE CODIGO PARA CREAR LA UNIDAD ORGANIZATIVA CORRESPONDIENTE A LA POSICION
            NucleusRH.Base.Organizacion.Unidades_Organizativas.UNIDAD_ORG unidad;
            NucleusRH.Base.Organizacion.Unidades_Organizativas.TIPO_UNI_ORG tipo_unidad;
            tipo_unidad = NucleusRH.Base.Organizacion.Unidades_Organizativas.TIPO_UNI_ORG.Get("3", false);

            //OBTENGO LA EMPRESA PARA PODER SACAR EL CODIGO
            NucleusRH.Base.Organizacion.Empresas.EMPRESA ddoEMPRESA;
            ddoEMPRESA = NucleusRH.Base.Organizacion.Empresas.EMPRESA.Get(ddoPUESTO.oi_empresa, false);

            //GENERO EL CODIGO DE UNIDAD ORGANIZATIVA A PARTIR DE LA EMPRESA, EL PUESTO Y LA POSICION
            string cod = ddoEMPRESA.c_empresa + "-" + ddoPUESTO.c_puesto + "-" + ddoPOSICION.c_posicion;

            // OBTENGO UNA UNIDAD BASURA EN LA MISMA POSICION
            unidad = (NucleusRH.Base.Organizacion.Unidades_Organizativas.UNIDAD_ORG)tipo_unidad.UNI_ORG.GetById(ddoPOSICION.oi_unidad_org);

            // SI NO EXISTE CREO UNA NUEVA
            if (unidad == null)
            {
                unidad = new NucleusRH.Base.Organizacion.Unidades_Organizativas.UNIDAD_ORG();

                // ASIGNO LOS VALORES
                unidad.c_unidad_org = cod;
                unidad.d_unidad_org = ddoPOSICION.d_posicion;
                unidad.o_unidad_org = ddoPOSICION.o_posicion;
                unidad.oi_puesto = ddoPUESTO.id;
                unidad.oi_clase_org = param.GetAttrInt("oi_clase_org");
                tipo_unidad.UNI_ORG.Add(unidad);

                // GRABO
                try
                {
                    NomadEnvironment.GetCurrentTransaction().SaveRefresh(tipo_unidad);
                }
                catch (Exception e)
                {
                    throw new NomadAppException("Error Guardando la Unidad Organizativa de Posicion: " + e.Message);
                }

                //MODIFICO POSICION
                ddoPOSICION.l_vigente = true;
                ddoPOSICION.f_hasta_vigenciaNull = true;
                ddoPOSICION.oi_unidad_org = ((NucleusRH.Base.Organizacion.Unidades_Organizativas.UNIDAD_ORG)tipo_unidad.UNI_ORG.GetByAttribute("c_unidad_org", cod, false)).Id;

            }
            else
            {
                //SE MODIFICA LA CLASE ORGANIZATIVA ULTIMA
                unidad.oi_clase_org = param.GetAttrInt("oi_clase_org");
                ddoPOSICION.oi_unidad_org = ((NucleusRH.Base.Organizacion.Unidades_Organizativas.UNIDAD_ORG)tipo_unidad.UNI_ORG.GetByAttribute("c_unidad_org", cod, false)).Id;
                ddoPOSICION.l_vigente = true;
                ddoPOSICION.f_hasta_vigenciaNull = true;

                // GRABO
                try
                {
                    NomadEnvironment.GetCurrentTransaction().SaveRefresh(tipo_unidad);
                }
                catch (Exception e)
                {
                    throw new NomadAppException("Error Guardando la Unidad Organizativa de Posicion: " + e.Message);
                }

            }

            //TOMO LA ESTRUCTURA ELEGIDA
            NucleusRH.Base.Organizacion.Clases_Organizativas.ESTRUCTURA ddoEstr = NucleusRH.Base.Organizacion.Clases_Organizativas.ESTRUCTURA.Get(param.GetAttrInt("oi_estructura"), false);

            //MODIFICO LOS DATOS DE LA UNIDAD ORGANIZATIVA
            ddoEstr.oi_unidad_org = ddoPOSICION.oi_unidad_org;
            ddoEstr.l_staff = ddoPOSICION.l_staff;

            //GRABO POSICION Y ESTRUCTURA
            NomadEnvironment.GetCurrentTransaction().SaveRefresh(ddoPOSICION);
            NomadEnvironment.GetCurrentTransaction().SaveRefresh(ddoEstr);

        }

        //VALIDA SI LA POSICION ESTA ASIGNADA A UNA UNIDAD ORGANIZATIVA QUE PERTENEZCA A ALGUNA ESTRUCTURA
        public static void ValidarEliminarPosicion(string oi_posicion)
        {

            //PRIMERO EVALUO QUE EL ID QUE TRAE SEA ENTERO (SI NO ES ENTERO, CORRESPONDE A UNA POSICION CREADA ACTUALMENTE ANTES DE CONFIRMAR LOS DATOS Y NO NECESITO VALIDAR)
            int id;
            bool result = int.TryParse(oi_posicion, out id);
            if(result)
            {
                //BUSCO LA POSICION
                NucleusRH.Base.Organizacion.Puestos.POSICION ddoPOSICION = NucleusRH.Base.Organizacion.Puestos.POSICION.Get(oi_posicion);

                //CREAMOS TIPO DE UNIDAD ORGANIZATIVA Y UNIDAD ORTANIZATIVA
                NucleusRH.Base.Organizacion.Unidades_Organizativas.UNIDAD_ORG unidad;
                NucleusRH.Base.Organizacion.Unidades_Organizativas.TIPO_UNI_ORG tipo_unidad;
                tipo_unidad = NucleusRH.Base.Organizacion.Unidades_Organizativas.TIPO_UNI_ORG.Get("3", false);

                //TRAE LOS DATOS DEL PUESTO Y LA EMPRESA PARA FORMAR EL CODIGO DE UNIDAD ORGANIZATIVA
                NucleusRH.Base.Organizacion.Puestos.PUESTO ddoPUESTO = NucleusRH.Base.Organizacion.Puestos.PUESTO.Get(ddoPOSICION.oi_puesto);
                NucleusRH.Base.Organizacion.Empresas.EMPRESA ddoEMPRESA = NucleusRH.Base.Organizacion.Empresas.EMPRESA.Get(ddoPUESTO.oi_empresa);

                //GENERO LA UNIDAD ORGANIZATIVA A TRAVES DEL oi_unidad_org DE LA POSICION
                if (!ddoPOSICION.oi_unidad_orgNull)
                    unidad = (NucleusRH.Base.Organizacion.Unidades_Organizativas.UNIDAD_ORG)tipo_unidad.UNI_ORG.GetById(ddoPOSICION.oi_unidad_org);
                else
                    unidad = null;

                if (unidad != null)
                {
                    //CONSULTA SI EXISTE UNA ESTRUCTURA QUE ESTA UTILIZANDO EN ESTOS MOMENTOS ESA UNIDAD ORGANIZATIVA
                    string oi_estructura = NomadEnvironment.QueryValue("org02_estructuras", "oi_estructura", "oi_unidad_org", unidad.Id, "", false);
                    if (oi_estructura != "")
                        throw new NomadAppException("Esta Posición esta asociada a una Unidad Organizativa, que esta siendo utilizada en una Estructura. Elimine dicha Estructura para poder eliminar esta Posición.");
                }

            }

        }

    }
}



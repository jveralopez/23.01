using System;
using System.Xml;
using System.IO;
using System.Collections;

using Nomad.NSystem.Document;
using Nomad.NSystem.Proxy;
using Nomad.NSystem.Base;
using Nomad.NSystem.Functions;
using NucleusRH.Base.Liquidacion.Legajo_Liquidacion;

namespace NucleusRH.Base.Liquidacion.MigAnticipos
{

    //////////////////////////////////////////////////////////////////////////////////
    //Clase Anticipos
    public partial class ANTICIPOS
    {
        public static void ImportarAnticipos()
        {

            int Linea = 0, Errores, Importados = 0, Errores2 = 0, ErroresTotal = 0;

            NomadBatch objBatch;
            objBatch = NomadBatch.GetBatch("Iniciando...", "Importación de Anticipos");

            NomadXML IDList = new NomadXML();
            NucleusRH.Base.Liquidacion.MigAnticipos.ANTICIPOS objRead;
            ANTICIPO objANTPersona;
            DESC_ANTICIPO objANTCuota;
            PERSONAL_EMP objPersonalEmp = null;
            string CodAnt_primero = "", CodAnt_siguiente = "";

            Hashtable htaAnticipos = new Hashtable(); //guarda los anticipos con sus cuotas - se usara para realizar la validacion antes de hacer el save

            IDList.SetText(NomadProxy.GetProxy().SQLService().Get(Resources.QRY_REGISTROS, ""));

            ArrayList lista = (ArrayList)IDList.FirstChild().GetElements("ROW");
            //RECORRO LOS IDS QUE ENTRAN Y PREGUNTO SI ALGUN ID NO ESTA EN LA HASH, PORQ DE SER ASI HAY QUE AGREGARLO
            for (int xml = 0; xml < lista.Count; xml++)
            {
                Errores = 0; //Por registro leido - para saber si guardar o no en la hash
                Linea++;
                NomadXML row = (NomadXML)lista[xml];
                objBatch.SetPro(0, 100, lista.Count, xml);
                objBatch.SetMess("Incorporando registro " + (xml + 1) + " de " + lista.Count);
                
                //Inicio la Transaccion - Crear primero los anticipos en los legajos de liquidacion
                try
                {
                    objRead = NucleusRH.Base.Liquidacion.MigAnticipos.ANTICIPOS.Get(row.GetAttr("id"));

                    //Valido atributos obligatorios del anticipo
                    if (objRead.c_empresa == "")
                    {
                        objBatch.Err("No se especifico la Empresa, se rechaza el registro de la Linea:'" + Linea.ToString());
                        Errores++;
                        ErroresTotal++;
                        continue;
                    }
                    if (objRead.e_numero_legajo == "")
                    {
                        objBatch.Err("No se especifico el Numero de Legajo, se rechaza el registro de la Linea:'" + Linea.ToString());
                        Errores++;
                        ErroresTotal++;
                        continue;
                    }
                    if (objRead.f_solicitudNull)
                    {
                        objBatch.Err("No se especifico la Fecha de Solicitud, se rechaza el registro de la Linea:'" + Linea.ToString());
                        Errores++;
                        ErroresTotal++;
                        continue;
                    }
                    if (objRead.f_anticipoNull)
                    {
                        objBatch.Err("No se especifico la Fecha del Anticipo, se rechaza el registro de la Linea:'" + Linea.ToString());
                        Errores++;
                        ErroresTotal++;
                        continue;
                    }
                    if (objRead.n_total == 0)
                    {
                        objBatch.Err("No se especifico el Importe Total correspondiente al Anticipo, se rechaza el registro de la Linea:'" + Linea.ToString());
                        Errores++;
                        ErroresTotal++;
                        continue;
                    }
                    if (objRead.e_cant_cuotas == 0)
                    {
                        objBatch.Err("No se especifico la cantidad de Cuotas del Anticipo, se rechaza el registro de la Linea:'" + Linea.ToString());
                        Errores++;
                        ErroresTotal++;
                        continue;
                    }
                    if (objRead.e_periodo_com_dto == 0)
                    {
                        objBatch.Err("No se especifico el Período de comienzo del descuento del anticipo, se rechaza el registro de la Linea:'" + Linea.ToString());
                        Errores++;
                        ErroresTotal++;
                        continue;
                    }

                    //Cuotas Fijas - 1
                    //Cuotas Variables - 0
                    if (objRead.e_cuotas_fijasNull)
                    {
                        objBatch.Err("No se especifico si el anticipo tiene cuotas Fijas o Variables, se rechaza el registro de la Linea:'" + Linea.ToString());
                        Errores++;
                        ErroresTotal++;
                        continue;
                    }

                    //Recupero los OI de los c&#243;digos ingresados
                    string oiEMP = "", oiPER = "", oiANT = "", oiTIPOANT = "";

                    //Recupero la empresa
                    oiEMP = NomadEnvironment.QueryValue("ORG03_EMPRESAS", "oi_empresa", "c_empresa", objRead.c_empresa, "", true);
                    if (oiEMP == null)
                    {
                        objBatch.Err("La empresa no existe, se rechaza el registro '" + objRead.c_empresa + " - " + objRead.e_numero_legajo + "' - Linea: " + Linea.ToString());
                        Errores++;
                        ErroresTotal++;
                        continue;
                    }

                    //Recupero el Legajo en la Empresa
                    oiPER = NomadEnvironment.QueryValue("PER02_PERSONAL_EMP", "oi_personal_emp", "e_numero_legajo", objRead.e_numero_legajo.ToString(), "PER02_PERSONAL_EMP.oi_empresa = " + oiEMP, true);
                    if (oiPER == null)
                    {
                        objBatch.Err("El Legajo no existe en la Empresa, se rechaza el registro '" + objRead.c_empresa + " - " + objRead.e_numero_legajo + "' - Linea: " + Linea.ToString());
                        Errores++;
                        ErroresTotal++;
                        continue;
                    }

                    //Recupero el tipo de anticipo
                    if (!string.IsNullOrEmpty(objRead.c_tipo_anticipo))
                    {
                      oiTIPOANT = NomadEnvironment.QueryValue("LIQ33_TIPO_ANTICIPOS", "oi_tipo_anticipo", "c_tipo_anticipo", objRead.c_tipo_anticipo, "", true);
                      if (oiTIPOANT == null)
                      {
                          objBatch.Err("El tipo de anticipo '" + objRead.c_tipo_anticipo + "' no existe, se rechaza el registro '" + objRead.c_empresa + " - " + objRead.e_numero_legajo + "' - Linea: " + Linea.ToString());
                          Errores++;
                          ErroresTotal++;
                          continue;
                      }
                    }
                    

                    if (CodAnt_primero == "") //Para el primer registro del archivo - crea el anticipo con su primer cuota o sus cuotas si son fijas
                    {
                        CodAnt_primero = objRead.f_solicitud.ToString() + oiPER.ToString(); //Clave para cambiar de anticipo

                        //Me fijo si ya existe el Anticipo para el legajo ingresado
                        //No se pueden cargar dos anticipos con la misma fecha de solicitud
                        oiANT = NomadEnvironment.QueryValue("LIQ07_ANTICIPOS", "oi_anticipo", "f_solicitud", objRead.f_solicitud.ToString("yyyyMMdd"), "LIQ07_ANTICIPOS.oi_personal_emp = " + oiPER, true);

                        if (oiANT != null)
                        {
                            objBatch.Err("Ya existe un anticipo para el Legajo '" + objRead.e_numero_legajo + " con fecha de solicitud " + objRead.f_solicitud + "' - Linea: " + Linea.ToString());
                            Errores++;
                            ErroresTotal++;
                            continue;
                        }
                        else
                        {

                            //Verifica que el registro no tenga errores
                            if (Errores == 0)
                            {
                                //Crear el anticipo y agregarlo a la hashtable de anticipos - que se usara para hacer el save
                                //La clave para guardar en la hash - es la fecha_solicitud y el oi_personal_emp
                                objANTPersona = new ANTICIPO();
                                objANTPersona.oi_personal_emp = StringUtil.str2int(oiPER);
                                if (string.IsNullOrEmpty(oiTIPOANT)) objANTPersona.oi_tipo_anticipoNull = true; else objANTPersona.oi_tipo_anticipo = oiTIPOANT;
                                objANTPersona.c_estado = "O"; //Estado = Otorgado
                                objANTPersona.f_solicitud = objRead.f_solicitud;
                                objANTPersona.f_anticipo = objRead.f_anticipo;
                                objANTPersona.n_importe = objRead.n_total;
                                objANTPersona.e_cant_cuotas = objRead.e_cant_cuotas;
                                objANTPersona.e_periodo = objRead.e_periodo_com_dto;
                                objANTPersona.l_liquida = true;
                                objANTPersona.o_anticipo = objRead.o_anticipo;
                                objANTPersona.e_quincena = objRead.e_quincena_com;

                                //Crear la primer cuota varialbe o todas las fijas del anticipo antes de guardar en la hash

                                //Si es igual a uno - cuotas fijas
                                if (objRead.e_cuotas_fijas == 1)
                                {
                                    int periodo = objRead.e_periodo_com_dto; //Periodo de comienzo del descuento
                                    string primer_dia = "";

                                    //Mensuales
                                    if (objRead.e_quincena_com != 1 && objRead.e_quincena_com != 2)
                                    {
                                        primer_dia = "01";
                                    }
                                    //Quincenales
                                    else
                                    {
                                        //Comienza en la primer quincena
                                        if (objRead.e_quincena_com == 1)
                                        {
                                            primer_dia = "01";
                                        }
                                        else
                                        {
                                            //Comienza en la segunda quincena
                                            if (objRead.e_quincena_com == 2)
                                            {
                                                primer_dia = "16";
                                            }
                                        }

                                    }

                                    //El importe es igual en todas las cuotas - se calcula una sola vez
                                    double dblCuota = 0;
                                    dblCuota = objRead.n_total / objRead.e_cant_cuotas;
                                    //dblCuota = Math.Floor(dblCuota);
                                    dblCuota = (double)Decimal.Round((decimal)dblCuota, 2, MidpointRounding.ToEven);
                                    
                                    //Me quedo con el entero mas chico - cuando calculo el total me fijo de llegar al importe total

                                    for (int i = 1; i <= objRead.e_cant_cuotas; i++)
                                    {
                                        objANTCuota = new DESC_ANTICIPO();
                                        objANTCuota.oi_anticipo = objANTPersona.id;
                                        objANTCuota.e_cuota = i;
                                        objANTCuota.n_importe = dblCuota;
                                        objANTCuota.c_estado = "P";

                                        objANTCuota.f_descuento = StringUtil.str2date(periodo.ToString() + primer_dia);

                                        objANTPersona.DESC_ANTI.Add(objANTCuota); //Agrego las cuotas al anticipo

                                        //Quincenal
                                        if (objRead.e_quincena_com == 1 || objRead.e_quincena_com == 2)
                                        {
                                            if (primer_dia == "01")
                                            {
                                                primer_dia = "16";
                                            }
                                            else
                                            {
                                                primer_dia = "01";
                                                periodo = periodo + 1;
                                            }
                                        }
                                        //Mensual
                                        else
                                        {
                                            periodo = periodo + 1;
                                        }

                                        string mes = (periodo.ToString()).Substring(4, 2);
                                        string año = (periodo.ToString()).Substring(0, 4);

                                        if (mes == "13")
                                        {
                                            periodo = StringUtil.str2int((StringUtil.str2int(año) + 1) + "01");
                                        }

                                    }

                                    //Calculo el total de las cuotas - si es diferente al importe del anticipo - el problema son los decimales

                                    double diferencia = 0, importe_total = 0;                                    

                                    //Recorre las cuotas y suma sus importes
                                    foreach (DESC_ANTICIPO DESC in objANTPersona.DESC_ANTI)
                                    {
                                        importe_total = importe_total + DESC.n_importe;
                                    }

                                    diferencia = objANTPersona.n_importe - importe_total;
                                    diferencia = (double)Decimal.Round((decimal)diferencia, 2, MidpointRounding.ToEven);

                                    //Para las cuotas fijas - por los decimales en la division
                                    //Si es de cuotas fijas - y la diferencia entre el total y la suma de las cuotas no es cero - es por el tema de los decimales
                                    //Tengo que sumarle la diferencia a la primer cuota
                                    if (diferencia != 0)
                                    {
                                        //recuperar el ultimo objeto de la coleccion de cuotas - sumarle la diferencia
                                        objANTCuota = (DESC_ANTICIPO)objANTPersona.DESC_ANTI[objANTPersona.DESC_ANTI.Count - 1];
                                        objANTCuota.n_importe = objANTCuota.n_importe + diferencia;
                                    }
                                }
                                //Si no es igual a uno - son cuotas determinadas en el archivo y hay que seguir leyendo
                                else
                                {
                                    //Si es igual a 0 - son cuotas variables
                                    if (objRead.e_cuotas_fijas == 0)
                                    {
                                        //Valida los datos de la primer cuota a asignar al anticipo leido
                                        if (objRead.e_nro_cuota == 0)
                                        {
                                            objBatch.Err("No se especifico el Número de Cuota del descuento del anticipo, se rechaza el registro de la Linea:'" + Linea.ToString());
                                            Errores++;
                                            ErroresTotal++;
                                            continue;
                                        }
                                        if (objRead.f_dto_cuotaNull)
                                        {
                                            objBatch.Err("No se especifico la Fecha de Descuento de una Cuota del Anticipo, se rechaza el registro de la Linea:'" + Linea.ToString());
                                            Errores++;
                                            ErroresTotal++;
                                            continue;
                                        }
                                        if (objRead.n_importe_cuota == 0)
                                        {
                                            objBatch.Err("No se especifico el Importe Correspondiente al de una cuota del Anticipo, se rechaza el registro de la Linea:'" + Linea.ToString());
                                            Errores++;
                                            ErroresTotal++;
                                            continue;
                                        }

                                        //Verifica que el registro no tenga errores
                                        if (Errores == 0)
                                        {
                                            objANTCuota = new DESC_ANTICIPO();
                                            objANTCuota.oi_anticipo = objANTPersona.id;
                                            objANTCuota.e_cuota = objRead.e_nro_cuota;
                                            objANTCuota.f_descuento = objRead.f_dto_cuota;
                                            objANTCuota.n_importe = objRead.n_importe_cuota;

                                            if (objRead.c_estado_cuota == "")
                                            {
                                                objANTCuota.c_estado = "P";
                                            }
                                            else
                                            {
                                                objANTCuota.c_estado = objRead.c_estado_cuota;
                                            }

                                            objANTPersona.DESC_ANTI.Add(objANTCuota); //Agrego las cuotas al anticipo
                                        }
                                    }
                                }

                                //Agrego el anticipo a la hash - con clave igual a fecha_solicitud y oi_personal_emp
                                if (!htaAnticipos.ContainsKey(CodAnt_primero))
                                    htaAnticipos.Add(CodAnt_primero, objANTPersona);
                            }

                        }
                    }
                    else
                    {
                        CodAnt_siguiente = objRead.f_solicitud.ToString() + oiPER.ToString(); //Clave para cambiar de anticipo

                        //Sigo con el mismo anticipo - No cambia
                        if (CodAnt_primero == CodAnt_siguiente)
                        {
                            //Sigo en el mismo anticipo - creo una nueva cuota
                            //Crear la siguientes cuotas del anticipo que se creo antes

                            //Recuperar el anticipo guardado en la hashtable

                            //Tiene cuotas fijas o estan dentro del archivo
                            //Si es igual a uno - cuotas fijas - ya las deberia haber creado a todas
                            if (objRead.e_cuotas_fijas == 1)
                            {
                                objBatch.Err("El anticipo de cuotas fijas ya fue procesado - Se rechaza el registro de la Linea:'" + Linea.ToString());
                                Errores++;
                                ErroresTotal++;
                                continue;
                            }
                            //Si no es igual a uno - son cuotas determinadas en el archivo y hay que seguir leyendo
                            else
                            {
                                //Si es igual a 0 - son cuotas variables
                                if (objRead.e_cuotas_fijas == 0)
                                {
                                    //Valida los datos de la primer cuota a asignar al anticipo leido
                                    if (objRead.e_nro_cuota == 0)
                                    {
                                        objBatch.Err("No se especifico el Número de Cuota del descuento del anticipo, se rechaza el registro de la Linea:'" + Linea.ToString());
                                        Errores++;
                                        ErroresTotal++;
                                        continue;
                                    }
                                    if (objRead.f_dto_cuotaNull)
                                    {
                                        objBatch.Err("No se especifico la Fecha de Descuento de una Cuota del Anticipo, se rechaza el registro de la Linea:'" + Linea.ToString());
                                        Errores++;
                                        ErroresTotal++;
                                        continue;
                                    }
                                    if (objRead.n_importe_cuota == 0)
                                    {
                                        objBatch.Err("No se especifico el Importe Correspondiente al de una cuota del Anticipo, se rechaza el registro de la Linea:'" + Linea.ToString());
                                        Errores++;
                                        ErroresTotal++;
                                        continue;
                                    }

                                    //Verifica que el registro no tenga errores
                                    if (Errores == 0)
                                    {
                                        objANTPersona = (ANTICIPO)htaAnticipos[CodAnt_primero];

                                        //Creo una nueva cuota
                                        objANTCuota = new DESC_ANTICIPO();
                                        objANTCuota.oi_anticipo = objANTPersona.id;
                                        objANTCuota.e_cuota = objRead.e_nro_cuota;
                                        objANTCuota.f_descuento = objRead.f_dto_cuota;
                                        objANTCuota.n_importe = objRead.n_importe_cuota;

                                        if (objRead.c_estado_cuota == "")
                                        {
                                            objANTCuota.c_estado = "P";
                                        }
                                        else
                                        {
                                            objANTCuota.c_estado = objRead.c_estado_cuota;
                                        }

                                        objANTPersona.DESC_ANTI.Add(objANTCuota); //Agrego las cuotas al anticipo

                                        htaAnticipos[CodAnt_primero] = objANTPersona; //Guardo el anticipo con sus cuotas actualizadas en la hashtable
                                    }
                                }
                            }
                        }
                        //Cambio el anticipo - crear un nuevo anticipo
                        else
                        {
                            CodAnt_primero = CodAnt_siguiente;
                            //Me fijo si ya existe el Anticipo para el legajo ingresado
                            //No se pueden cargar dos anticipos con la misma fecha de solicitud
                            oiANT = NomadEnvironment.QueryValue("LIQ07_ANTICIPOS", "oi_anticipo", "f_solicitud", objRead.f_solicitud.ToString("yyyyMMdd"), "LIQ07_ANTICIPOS.oi_personal_emp = " + oiPER, true);

                            if (oiANT != null)
                            {
                                objBatch.Err("Ya existe un anticipo para el Legajo '" + objRead.e_numero_legajo + " con fecha de solicitud " + objRead.f_solicitud + "' - Linea: " + Linea.ToString());
                                Errores++;
                                ErroresTotal++;
                                continue;
                            }
                            else
                            {

                                //Verifica que el registro no tenga errores
                                if (Errores == 0)
                                {
                                    //Crear el anticipo y agregarlo a la hash
                                    objANTPersona = new ANTICIPO();
                                    objANTPersona.oi_personal_emp = StringUtil.str2int(oiPER);
                                    if (string.IsNullOrEmpty(oiTIPOANT)) objANTPersona.oi_tipo_anticipoNull = true; else objANTPersona.oi_tipo_anticipo = oiTIPOANT;
                                    objANTPersona.c_estado = "O"; //Estado = Otorgado
                                    objANTPersona.f_solicitud = objRead.f_solicitud;
                                    objANTPersona.f_anticipo = objRead.f_anticipo;
                                    objANTPersona.n_importe = objRead.n_total;
                                    objANTPersona.e_cant_cuotas = objRead.e_cant_cuotas;
                                    objANTPersona.e_periodo = objRead.e_periodo_com_dto;
                                    objANTPersona.l_liquida = true;
                                    objANTPersona.o_anticipo = objRead.o_anticipo;
                                    objANTPersona.e_quincena = objRead.e_quincena_com;

                                    //Si es igual a uno - cuotas fijas
                                    if (objRead.e_cuotas_fijas == 1)
                                    {
                                        int periodo = objRead.e_periodo_com_dto; //Periodo de comienzo del descuento
                                        string primer_dia = "";

                                        //Mensuales
                                        if (objRead.e_quincena_com != 1 && objRead.e_quincena_com != 2)
                                        {
                                            primer_dia = "01";
                                        }
                                        //Quincenales
                                        else
                                        {
                                            //Comienza en la primer quincena
                                            if (objRead.e_quincena_com == 1)
                                            {
                                                primer_dia = "01";
                                            }
                                            else
                                            {
                                                //Comienza en la segunda quincena
                                                if (objRead.e_quincena_com == 2)
                                                {
                                                    primer_dia = "16";
                                                }
                                            }

                                        }

                                        double dblCuota = 0;
                                        dblCuota = objRead.n_total / objRead.e_cant_cuotas;
                                        //dblCuota = Math.Floor(dblCuota);
                                        dblCuota = (double)Decimal.Round((decimal)dblCuota, 2, MidpointRounding.ToEven);

                                        for (int i = 1; i <= objRead.e_cant_cuotas; i++)
                                        {
                                            objANTCuota = new DESC_ANTICIPO();
                                            objANTCuota.oi_anticipo = objANTPersona.id;
                                            objANTCuota.e_cuota = i;
                                            objANTCuota.n_importe = dblCuota;
                                            objANTCuota.c_estado = "P";

                                            objANTCuota.f_descuento = StringUtil.str2date(periodo.ToString() + primer_dia);

                                            objANTPersona.DESC_ANTI.Add(objANTCuota); //Agrego las cuotas al anticipo

                                            //Quincenal
                                            if (objRead.e_quincena_com == 1 || objRead.e_quincena_com == 2)
                                            {
                                                if (primer_dia == "01")
                                                {
                                                    primer_dia = "16";
                                                }
                                                else
                                                {
                                                    primer_dia = "01";
                                                    periodo = periodo + 1;
                                                }
                                            }
                                            //Mensual
                                            else
                                            {
                                                periodo = periodo + 1;
                                            }

                                            string mes = (periodo.ToString()).Substring(4, 2);
                                            string año = (periodo.ToString()).Substring(0, 4);

                                            if (mes == "13")
                                            {
                                                periodo = StringUtil.str2int((StringUtil.str2int(año) + 1) + "01");
                                            }

                                        }

                                        //Calculo el total de las cuotas - si es diferente al importe del anticipo - el problema son los decimales

                                        double diferencia = 0, importe_total = 0;

                                        //Recorre las cuotas y suma sus importes
                                        foreach (DESC_ANTICIPO DESC in objANTPersona.DESC_ANTI)
                                        {
                                            importe_total = importe_total + DESC.n_importe;
                                        }

                                        diferencia = objANTPersona.n_importe - importe_total;
                                        diferencia = (double)Decimal.Round((decimal)diferencia, 2, MidpointRounding.ToEven);

                                        if (diferencia != 0)
                                        {
                                            objANTCuota = (DESC_ANTICIPO)objANTPersona.DESC_ANTI[objANTPersona.DESC_ANTI.Count - 1];
                                            objANTCuota.n_importe = objANTCuota.n_importe + diferencia;
                                        }

                                    }
                                    //Si no es igual a uno - son cuotas determinadas en el archivo y hay que seguir leyendo
                                    else
                                    {
                                        //Si es igual a 0 - son cuotas variables
                                        if (objRead.e_cuotas_fijas == 0)
                                        {
                                            //Valida los datos de la primer cuota a asignar al anticipo leido
                                            if (objRead.e_nro_cuota == 0)
                                            {
                                                objBatch.Err("No se especifico el Número de Cuota del descuento del anticipo, se rechaza el registro de la Linea:'" + Linea.ToString());
                                                Errores++;
                                                ErroresTotal++;
                                                continue;
                                            }
                                            if (objRead.f_dto_cuotaNull)
                                            {
                                                objBatch.Err("No se especifico la Fecha de Descuento de una Cuota del Anticipo, se rechaza el registro de la Linea:'" + Linea.ToString());
                                                Errores++;
                                                ErroresTotal++;
                                                continue;
                                            }
                                            if (objRead.n_importe_cuota == 0)
                                            {
                                                objBatch.Err("No se especifico el Importe Correspondiente al de una cuota del Anticipo, se rechaza el registro de la Linea:'" + Linea.ToString());
                                                Errores++;
                                                ErroresTotal++;
                                                continue;
                                            }

                                            //Verifica que el registro no tenga errores
                                            if (Errores == 0)
                                            {
                                                objANTCuota = new DESC_ANTICIPO();
                                                objANTCuota.oi_anticipo = objANTPersona.id;
                                                objANTCuota.e_cuota = objRead.e_nro_cuota;
                                                objANTCuota.f_descuento = objRead.f_dto_cuota;
                                                objANTCuota.n_importe = objRead.n_importe_cuota;

                                                if (objRead.c_estado_cuota == "")
                                                {
                                                    objANTCuota.c_estado = "P";
                                                }
                                                else
                                                {
                                                    objANTCuota.c_estado = objRead.c_estado_cuota;
                                                }

                                                objANTPersona.DESC_ANTI.Add(objANTCuota); //Agrego las cuotas al anticipo
                                            }
                                        }
                                    }

                                    //Agrego el anticipo a la hash - con clave igual a fecha_solicitud y oi_personal_emp
                                    if (!htaAnticipos.ContainsKey(CodAnt_primero))
                                        htaAnticipos.Add(CodAnt_primero, objANTPersona);

                                }

                            }
                        }

                    }

                }
                catch (Exception e)
                {
                    objBatch.Err("Error desconocido en registro, por favor verifique el formato del archivo y la correcta asignación de los campos - Linea: " + Linea.ToString() + " - " + e.Message);
                    Errores++;
                    ErroresTotal++;
                }
            }

            //Guardo el Anticipo con sus cuotas en la persona
            //Recorrer la hash de anticipos - recorro sus cuotas - valido

            //una vez que sus cuotas coinciden con lo que tiene que validar
            //recupero el oi de la persona desde el anticipo a guardar - y recupero el ddo de la persona - le asigno el anticipo - y luego hago el save
            //Validaciones - que la cantidad de cuotas y el importe sean correctos

            foreach (ANTICIPO ANT in htaAnticipos.Values)
            {
                double importe_total = 0; //Aca va la suma de los importes de cada una de las cuotas del anticipo
                double importe_total_desc = 0; //Aca va la suma de los importes de cada una de las cuotas del anticipo que estan en estado descontadas
                                               //Esto es para actualizar el importe pendiente a descontar del anticipo

                //Recorre las cuotas y suma sus importes
                foreach (DESC_ANTICIPO DESC in ANT.DESC_ANTI)
                {
                    importe_total = importe_total + DESC.n_importe;

                    if(DESC.c_estado == "D")
                    {
                        importe_total_desc = importe_total_desc + DESC.n_importe;
                    }
                }

                importe_total = (double)Decimal.Round((decimal)importe_total,2,MidpointRounding.ToEven);
                //importe_total_desc = Math.Floor(importe_total_desc);
                importe_total_desc = (double)Decimal.Round((decimal)importe_total_desc, 2, MidpointRounding.ToEven);

                objPersonalEmp = PERSONAL_EMP.Get(ANT.oi_personal_emp);
                //Valido que el total informado sea igual al sumado x todas las cuotas
                //if (importe_total == ANT.n_importe)
                if (importe_total == (double)Decimal.Round((decimal)ANT.n_importe,2,MidpointRounding.ToEven))
                {
                    //Valido que el numero de cuotas sea igual a la cantidad de cuotas informadas
                    if (ANT.e_cant_cuotas == ANT.DESC_ANTI.Count)
                    {
                        //Recupero a la persona - Agrego el anticipo en la persona y lo grabo - actualizo el importe pendiente de descuento
                        ANT.n_imp_pendiente = ANT.n_importe - importe_total_desc;                        

                        if(objPersonalEmp != null)
                        {
                            objPersonalEmp.ANTICIPOS.Add(ANT); //Agrego el anticipo a la coleccion de la persona

                            //Grabo
                            try
                            {
                                NomadEnvironment.GetCurrentTransaction().Begin();
                                NomadEnvironment.GetCurrentTransaction().Save(objPersonalEmp);
                                NomadEnvironment.GetCurrentTransaction().Commit();
                                Importados++;
                            }
                            catch (Exception e)
                            {
                                NomadEnvironment.GetCurrentTransaction().Rollback();
                                Errores2++;
                                ErroresTotal++;

                                if (e.Message == "DB.SQLSERVER.2627")
                                {
                                    //Violation of primary key. Handle Exception
                                    objBatch.Err("Ya existe un anticipo para el Legajo '" + objPersonalEmp.e_numero_legajo + " con fecha de solicitud " + ANT.f_solicitud);
                                }
                                else
                                {
                                    objBatch.Err("Error al grabar registro del Anticipo - Legajo: " + objPersonalEmp.e_numero_legajo + " - Fecha de Solcitud" + ANT.f_solicitud);

                                }
                            }
                        }
                        else
                        {
                            objBatch.Err("Error al grabar registro del Anticipo con Fecha de Solcitud: " + ANT.f_solicitud + " - Codigo: "+ANT.Code+" | No existe el Personal.");
                            Errores2++;
                            ErroresTotal++;
                        }
                        
                    }
                    else
                    {
                        objBatch.Err("Error al grabar registro del Anticipo para Legajo: " + objPersonalEmp.e_numero_legajo + " - Fecha de Solcitud: " + ANT.f_solicitud + " | La cantidad de cuotas no coincide con la cantidad total de cuotas informadas en el anticipo");
                        Errores2++;
                        ErroresTotal++;
                    }
                }
                else
                {
                    objBatch.Err("Error al grabar registro del Anticipo para Legajo: " + objPersonalEmp.e_numero_legajo + " - Fecha de Solcitud: " + ANT.f_solicitud + " | La suma de los importes de las cuotas no coincide con el importe total del anticipo.");                        
                    Errores2++;
                    ErroresTotal++;
                }

            }

            objBatch.Log("Registros Procesados: " + Linea.ToString() + " - Anticipos Importados: " + Importados.ToString() + " - Registros con Errores: " + ErroresTotal.ToString() + " - Error al guardar Anticipo: " + Errores2.ToString());
            objBatch.Log("Finalizado...");

        }
    }
}



using System;
using System.Collections.Generic;
using System.Text;
using Nomad.NSystem.Base;
using Nomad.NSystem.Proxy;
using System.Collections;

namespace NucleusRH.Base.Tiempos_Trabajados.InterfaceLegajoTTA
{
    public partial class PERSONAL : Nomad.NSystem.Base.NomadObject
    {
        //Devuelve Verdadero si no existe tarjeta asignada a otro Legajo, Falso si la tarjeta ya esta asignada a otro legajo
        public static Boolean validaTarjeta(string oi_personal_emp, string d_nro_tarjeta)
        {
            NomadXML retval = new NomadXML();
            NomadXML paramIn = new NomadXML();
            NomadXML param;
            Boolean bandera;

            param = paramIn.AddTailElement("DATO");
            param.SetAttr("oi_personal_emp", oi_personal_emp);
            param.SetAttr("d_nro_tarjeta", d_nro_tarjeta);

            retval.SetText(NomadProxy.GetProxy().SQLService().Get(Tiempos_Trabajados.Personal.PERSONAL_EMP.Resources.QRY_TARJETA, paramIn.ToString()));
            if (retval.FindElement("FLAG").GetAttr("flag") == "1")
            {
                bandera = false;
            }
            else
            {
                bandera = true;
            }

            return bandera;


        }

        public static string FindOI(string TABLE, string COLRET, string COLFND, string COLVAL)
        {
            NomadXML retval = new NomadXML();
            NomadXML paramIn = new NomadXML();
            NomadXML param;

            param = paramIn.AddTailElement("DATA");
            param.SetAttr("TABLE", TABLE);
            param.SetAttr("COLRET", COLRET);
            param.SetAttr("COLFND", COLFND);
            param.SetAttr("COLVAL", COLVAL);

            retval.SetText(NomadProxy.GetProxy().SQLService().Get(PERSONAL.Resources.qry_findOI, paramIn.ToString()));
            return retval.FindElement("RESULT").GetAttr("ID");
        }

        public static string FindOI(string TABLE, string COLRET, string COLFND1, string COLVAL1, string COLFND2, string COLVAL2)
        {
            NomadXML retval = new NomadXML();
            NomadXML paramIn = new NomadXML();
            NomadXML param;

            param = paramIn.AddTailElement("DATA");
            param.SetAttr("TABLE", TABLE);
            param.SetAttr("COLRET", COLRET);
            param.SetAttr("COLFND1", COLFND1);
            param.SetAttr("COLVAL1", COLVAL1);
            param.SetAttr("COLFND2", COLFND2);
            param.SetAttr("COLVAL2", COLVAL2);

            retval.SetText(NomadProxy.GetProxy().SQLService().Get(PERSONAL.Resources.qry_findOI2, paramIn.ToString()));
            return retval.FindElement("RESULT").GetAttr("ID");
        }

        public static void ImportarLegajosTTA() {
            //Variables
            int totRegs, linea;
            DateTime fecha = DateTime.Today;
            string lasssecc;
            

            //Objetos Necesario para la carga
            NucleusRH.Base.Tiempos_Trabajados.Personal.PERSONAL_EMP ddoLegajoReloj;
            NucleusRH.Base.Tiempos_Trabajados.InterfaceLegajoTTA.PERSONAL objRead;

            //Iniciando Proceso Batch
            NomadBatch b = NomadBatch.GetBatch("Importar Números de Legajos Reloj o Tarjetas", "Importar Números de Legajos Reloj o Tarjetas");

            //Cargando el Query.
            NomadXML MyXML = new NomadXML();
            MyXML.SetText(NomadProxy.GetProxy().SQLService().Get(PERSONAL.Resources.qry_legajos, ""));

            //Me paro sobre la lista de ROWS
            MyXML = MyXML.FirstChild();
            NomadXML MyROW;

            //Contando la Cantidad de ROWS
            totRegs = MyXML.ChildLength;

            //Recorre los registros
            b.SetMess("Incorporando Legajos Reloj o Tarjetas...");
            b.Log("Incorporando Legajos Reloj o Tarjetas...");
            for (linea = 1, MyROW = MyXML.FirstChild(); MyROW != null; linea++, MyROW = MyROW.Next())
            
            {
                //Progreso de la Importación
                b.SetPro(0, 90, totRegs, linea);
                b.SetMess("Procesando la Linea " + linea + " de " + totRegs);

                try {
                    
                    //Nueva Tarjeta y Nuevo LegajoReloj para cargar
                    lasssecc = "creando los legajos reloj y tarjeta";

                    //Recupera la linea por id
                    objRead = NucleusRH.Base.Tiempos_Trabajados.InterfaceLegajoTTA.PERSONAL.Get(MyROW.GetAttr("id"));

                    //Valida Campos Obligatorios
                    if (objRead.c_empresa != "" && objRead.e_num_legajo != 0 && objRead.nro_legajo_reloj != 0)
                    {
                        
                        lasssecc = "buscando empresa";
                        string docOIEmpresa = FindOI("ORG03_EMPRESAS", "oi_empresa", "c_empresa", objRead.c_empresa);
                        //Si la empresa ingresada existe continuamos con la operación
                        if (docOIEmpresa != "")
                        {
                            //Buscando el legajo dentro de la empresa
                            lasssecc = "buscando legajo empresa";
                            string docOIPersonalEmp = FindOI("PER02_PERSONAL_EMP", "oi_personal_emp", "oi_empresa", docOIEmpresa, "e_numero_legajo", objRead.e_num_legajo.ToString());
                            //Si el legajo existe dentro de la empresa continuamos con la operación
                            if (docOIPersonalEmp != "")
                            {
                                //Verifica si en este momento existe un legajo reloj cargado con ese oi_personal_emp
                                string modificable = FindOI("TTA04_PERSONAL", "oi_personal_emp", "oi_personal_emp", docOIPersonalEmp);
                                
                                if (modificable != "")
                                {

                                    //Modificación de un TTA04_PERSONAL existente
                                    int oi_personal_emp = int.Parse(docOIPersonalEmp);
                                    ddoLegajoReloj = NucleusRH.Base.Tiempos_Trabajados.Personal.PERSONAL_EMP.Get(oi_personal_emp);
                                    ddoLegajoReloj.e_nro_legajo_reloj = objRead.nro_legajo_reloj;
                                    ddoLegajoReloj.f_desde_tarjeta = fecha;
                                    
                                    //Si ingresó nro_tarjeta
                                    if (objRead.nro_tarjeta.ToString() != "0")
                                    {
                                        guardaLegajoyTarjeta(ddoLegajoReloj, false, objRead.nro_tarjeta, b, linea);
                                        
                                    }
                                    else
                                    {
                                        NomadEnvironment.GetCurrentTransaction().SaveRefresh(ddoLegajoReloj);
                                        b.Log("Se modificó el Legajo Reloj ingresado - Linea " + linea.ToString());
                                    }

                                    

                                }
                                else
                                {
                                   

                                    //Nuevo TTA04_PERSONAL 
                                    ddoLegajoReloj = new NucleusRH.Base.Tiempos_Trabajados.Personal.PERSONAL_EMP(docOIPersonalEmp);
                                    ddoLegajoReloj.e_nro_legajo_reloj = objRead.nro_legajo_reloj;
                                    ddoLegajoReloj.f_desde_tarjeta = fecha;

                                    //Si ingresó nro_tarjeta
                                    if (objRead.nro_tarjeta.ToString() != "0")
                                    {
                                        guardaLegajoyTarjeta(ddoLegajoReloj, true, objRead.nro_tarjeta, b, linea);
                                    }
                                    else
                                    {
                                        NomadEnvironment.GetCurrentTransaction().Save(ddoLegajoReloj);
                                        b.Log("Se agregó el nuevo Legajo Reloj ingresado - Linea " + linea.ToString());
                                    }

                                    
                                }


                            
                            }
                            else
                            {
                                Base.Organizacion.Empresas.EMPRESA empresa = Base.Organizacion.Empresas.EMPRESA.Get(docOIEmpresa);
                                b.Err("El Legajo " + objRead.e_num_legajo.ToString() + " no se encuentra dentro de la empresa "+empresa.d_empresa+" - Linea "+linea.ToString());
                               
                            }
                        }
                        else
                        {
                            b.Err("El código de empresa " + objRead.c_empresa + " no existe - Linea "+linea.ToString());
                            
                        }
                    }
                    else
                    {
                        b.Err("Debe completar todos los campos obligatorios - Linea "+linea.ToString());
                      
                    }
                    
                }
                catch (Exception e)
                {
                    b.Err("Error desconocido. " + e.Message + " - Linea " + linea.ToString());
                    
                }
            }

            b.Log("Finalización del Proceso...");


        
        }

        public static void guardaLegajoyTarjeta(Personal.PERSONAL_EMP ddoLegajoReloj, bool agregaLegajoReloj,int nro_tarjeta, NomadBatch b, int linea)
            {
                    NucleusRH.Base.Tiempos_Trabajados.Personal.TARJETA ddoTarjeta;
                    string oi_tarjeta = FindOI("TTA04_TARJETAS", "oi_tarjeta", "oi_personal_emp", ddoLegajoReloj.Id);

                    if (validaTarjeta(ddoLegajoReloj.Id, nro_tarjeta.ToString()))
                    {
                        //Si agrega Legajo Reloj
                        if (agregaLegajoReloj == true)
                        {
                            //Crea una Nueva Tarjeta
                            ddoLegajoReloj.d_nro_tarjeta = nro_tarjeta.ToString();
                            ddoTarjeta = new Personal.TARJETA();
                            ddoTarjeta.oi_personal_emp = int.Parse(ddoLegajoReloj.Id);
                            ddoTarjeta.d_nro_tarjeta = nro_tarjeta.ToString();
                            ddoTarjeta.f_desde = DateTime.Today;
                            ddoLegajoReloj.TARJETAS.Add(ddoTarjeta);

                            NomadEnvironment.GetCurrentTransaction().Save(ddoLegajoReloj);
                            b.Log("Se agrega el nuevo Legajo Reloj y la Tarjeta ingresada - Linea " + linea.ToString());

                        }
                        else
                        {
                            //Modifica un Legajo Reloj

                            //Evalua si modifica la tarjeta que posee el legajo actualmente, o debe crear un nuevo registro
                            if (oi_tarjeta != "")
                            {
                                //Modifica una Tarjeta Existente

                                ddoLegajoReloj.d_nro_tarjeta = nro_tarjeta.ToString();
                                NomadEnvironment.GetCurrentTransaction().SaveRefresh(ddoLegajoReloj);
                                ddoTarjeta = Personal.TARJETA.Get(oi_tarjeta);
                                ddoTarjeta.oi_personal_emp = int.Parse(ddoLegajoReloj.Id);
                                ddoTarjeta.d_nro_tarjeta = nro_tarjeta.ToString();
                                ddoTarjeta.f_desde = DateTime.Today;

                                NomadEnvironment.GetCurrentTransaction().SaveRefresh(ddoTarjeta);
                                b.Log("Se modificó el Legajo Reloj ingresado junto con los datos de la tarjeta - Linea " + linea.ToString());
                               

                            }
                            else
                            {
                                //Crea una Nueva Tarjeta
                                ddoLegajoReloj.d_nro_tarjeta = nro_tarjeta.ToString();
                                ddoTarjeta = new Personal.TARJETA();
                                ddoTarjeta.oi_personal_emp = int.Parse(ddoLegajoReloj.Id);
                                ddoTarjeta.d_nro_tarjeta = nro_tarjeta.ToString();
                                ddoTarjeta.f_desde = DateTime.Today;
                                ddoLegajoReloj.TARJETAS.Add(ddoTarjeta);

                                NomadEnvironment.GetCurrentTransaction().SaveRefresh(ddoLegajoReloj);
                                b.Log("Se modificó el Legajo Reloj ingresado y se creo la Tarjeta - Linea " + linea.ToString());

                            }

                        }
                    }
                    else
                    {
                        b.Err("No se puedo cargar el número de legajo reloj y la tarjeta asignada al mismo, debido a que el número de tarjeta ingresado corresponde a otro Legajo en este momento - Linea "+linea.ToString());
                        
                    }

                }
        
            
    
            }



    }



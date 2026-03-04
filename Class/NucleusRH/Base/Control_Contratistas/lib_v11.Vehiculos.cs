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

namespace NucleusRH.Base.Control_Contratistas.Vehiculos 
{


  //Clase Vehículos de Empresas Contratistas
    public partial class VEHICULO : Nomad.NSystem.Base.NomadObject
    {
        public static void GuardarAtributosControl(string pstrVehiculo, Nomad.NSystem.Proxy.NomadXML paramATRIBUTOS)
        {

            NomadLog.Info("****** Guardar Atributos de Control ******");
            NomadLog.Debug("GuardarAtributosControl.pstrVehiculo " + pstrVehiculo);
            NomadLog.Debug("GuardarAtributosControl.paramATRIBUTOS " + paramATRIBUTOS.ToString());

            if (paramATRIBUTOS.isDocument)
                paramATRIBUTOS = paramATRIBUTOS.FirstChild();

            NomadTransaction objTransaccion = new NomadTransaction();
            objTransaccion.Begin();


            try
            {
                NucleusRH.Base.Control_Contratistas.Vehiculos.VEHICULO vehic = VEHICULO.Get(pstrVehiculo);

                //recorro lista de atributos a controlar
                for (NomadXML rowDetalle = paramATRIBUTOS.FirstChild(); rowDetalle != null; rowDetalle = rowDetalle.Next())
                {

                    //el registro ya existe en la bd (el vehiculo controla ese atributo)
                    if (rowDetalle.GetAttr("oi_atrib_vehic") != "" && rowDetalle.GetAttr("oi_atrib_vehic") != "")
                    {
                        //ACTUALIZA EL ATRIBUTO DEL VEHICULO
                        //id
                        string oi_atrib_vehic = rowDetalle.GetAttr("oi_atrib_vehic");
                        NucleusRH.Base.Control_Contratistas.Vehiculos.ATRIB_VEHIC atri = (ATRIB_VEHIC)vehic.ATRIB_VEHIC.GetById(oi_atrib_vehic);

                        //si completo alguno de los campos (f_vencimiento o observacion)
                        if (rowDetalle.GetAttr("f_vencimiento") != "" || rowDetalle.GetAttr("o_atrib_vehic") != "") //actualiza
                                {
                                    //f_vencimiento
                                    if (rowDetalle.GetAttrDateTime("f_vencimiento") != null && rowDetalle.GetAttr("f_vencimiento") != "")
                                    {
                                        atri.f_vencimiento = rowDetalle.GetAttrDateTime("f_vencimiento");
                                    }
                                    else { atri.f_vencimientoNull = true; }
                                  
                                    //observacion
                                    if (rowDetalle.GetAttr("o_atrib_vehic") != null)
                                    { atri.o_atrib_vehic = rowDetalle.GetAttr("o_atrib_vehic"); }
                                }
                                else
                                {
                                    //BORRA EL ATRIBUTO DEL VEHICULO
                                    vehic.ATRIB_VEHIC.Remove(atri);
                                }
                            
                            
                    }
                    else //El vehiculo no controla ese atributo
                    {
                        //AGREGA EL ATRIBUTO DEL VEHICULO
                        //si completo alguno de los campos (f_vencimiento o observacion)
                        if (rowDetalle.GetAttr("f_vencimiento") != "" || rowDetalle.GetAttr("o_atrib_vehic") != "") //agrega
                        {
                            //creo un nuevo registro
                            NucleusRH.Base.Control_Contratistas.Vehiculos.ATRIB_VEHIC new_atrib = NucleusRH.Base.Control_Contratistas.Vehiculos.ATRIB_VEHIC.New();
                            
                            //asigna el id al vehiculo
                            new_atrib.oi_atributo_ctrol = rowDetalle.GetAttr("id");

                            //f_vencimiento
                            if (rowDetalle.GetAttrDateTime("f_vencimiento") != null && rowDetalle.GetAttr("f_vencimiento") != "")
                            {
                                new_atrib.f_vencimiento = rowDetalle.GetAttrDateTime("f_vencimiento");
                            }
                            else
                            { new_atrib.f_vencimientoNull = true; }

                            //observacion
                            if (rowDetalle.GetAttr("o_atrib_vehic") != null)
                            { new_atrib.o_atrib_vehic = rowDetalle.GetAttr("o_atrib_vehic"); }

                            //agrega el registro
                            vehic.ATRIB_VEHIC.Add(new_atrib);
                        }

                    }

                }

                objTransaccion.Save(vehic);
                objTransaccion.Commit();


                NomadLog.Info("******FIN Guardar Atributos de Control ******");
            }
            catch (Exception e)
            {
                objTransaccion.Rollback();
                throw new NomadAppException("Error no manejado. " + e.Message);
            }




        }

        public static void CargarAtributosControl (NucleusRH.Base.Control_Contratistas.Vehiculos.VEHICULO pObjVehiculo)
        {
            NomadTransaction objTran = null;

            string strStep = "";
            try
            {
                
                    objTran = new NomadTransaction();
                    objTran.Begin();

                    //Atributos de control del vehículo
                    //defino el parámetro de tipo="vehículo"
                    NomadXML xmlParam = new NomadXML("DATA");
                    xmlParam.SetAttr("tipo", "V");

                    //Ejecuto el recurso para recuperar los atributos de control
                    NomadXML XML = Nomad.NSystem.Base.NomadEnvironment.QueryNomadXML(NucleusRH.Base.Control_Contratistas.Atributos_Control.ATRIBUTO_CTROL.Resources.INFO_TIPO, xmlParam.ToString());

                    //los guardo en el vehículo
                    for (NomadXML row = XML.FirstChild().FirstChild(); row != null; row = row.Next())
                    {
                        NucleusRH.Base.Control_Contratistas.Vehiculos.ATRIB_VEHIC new_atrib = NucleusRH.Base.Control_Contratistas.Vehiculos.ATRIB_VEHIC.New();
                        new_atrib.oi_atributo_ctrol = row.GetAttr("id");
                        pObjVehiculo.ATRIB_VEHIC.Add(new_atrib);
                    }

                    objTran.Save(pObjVehiculo);
                    objTran.Commit();


            }
            catch (Exception ex)
            {
                NomadException nmdEx = new NomadException("CargarAtributosControl", ex);
                nmdEx.SetValue("Step", strStep);
                if (objTran != null)
                {
                    objTran.Rollback();
                }
                throw nmdEx;
            }

        }

    }

}

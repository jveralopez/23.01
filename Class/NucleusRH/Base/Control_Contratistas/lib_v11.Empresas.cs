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

namespace NucleusRH.Base.Control_Contratistas.Empresas
{

    //Clase Empresas Contratistas
    public partial class EMPRESA : NucleusRH.Base.Organizacion.Empresas.EMPRESA
    {
        public static void GuardarAtributosControl(NucleusRH.Base.Control_Contratistas.Empresas.EMPRESA paramEMPRESA, Nomad.NSystem.Proxy.NomadXML paramATRIBUTOS)

        {
            
            NomadLog.Info("****** Guardar Atributos de Control ******");
            NomadLog.Debug("GuardarAtributosControl.paramEMPRESA " + paramEMPRESA.ToString());
            NomadLog.Debug("GuardarAtributosControl.paramATRIBUTOS " + paramATRIBUTOS.ToString());

            if(paramATRIBUTOS.isDocument) 
                paramATRIBUTOS = paramATRIBUTOS.FirstChild();

            NomadTransaction objTransaccion = new NomadTransaction();
            objTransaccion.Begin();

          try
          {
   
            //recorro lista de atributos a controlar
            for (NomadXML rowDetalle = paramATRIBUTOS.FirstChild(); rowDetalle != null; rowDetalle = rowDetalle.Next())
            {

                //el registro ya existe en la bd (la empresa controla ese atributo)
                if (rowDetalle.GetAttr("oi_atrib_emp_con") != null && rowDetalle.GetAttr("oi_atrib_emp_con") != "")
                {
                    //ACTUALIZA EL ATRIBUTO DE LA EMPRESA
                    //id
                    string oi_atrib_emp_con = rowDetalle.GetAttr("oi_atrib_emp_con");
                    NucleusRH.Base.Control_Contratistas.Empresas.ATRIB_EMP_CON atributo = (ATRIB_EMP_CON)paramEMPRESA.ATRIB_EMP_CON.GetById(oi_atrib_emp_con);

                    //si completo alguno de los campos (f_vencimiento o observacion)
                    if (rowDetalle.GetAttr("f_vencimiento") != "" || rowDetalle.GetAttr("o_atrib_emp_con") != "")
                    {
                        // fecha de vencimiento
                        if (rowDetalle.GetAttrDateTime("f_vencimiento") != null && rowDetalle.GetAttr("f_vencimiento") != "")
                        {
                            atributo.f_vencimiento = rowDetalle.GetAttrDateTime("f_vencimiento");
                        }
                        else { atributo.f_vencimientoNull = true; }

                        //observacion
                        if (rowDetalle.GetAttr("o_atrib_emp_con") != null)
                        { atributo.o_atrib_emp_con = rowDetalle.GetAttr("o_atrib_emp_con"); }
                    }
                    else
                    {
                        //BORRA EL ATRIBUTO DE LA EMPRESA
                        paramEMPRESA.ATRIB_EMP_CON.Remove(atributo);
                    }

                }
                else //si el registro no existe, la persona no controla aún ese atributo
                {
                    //AGREGA EL ATRIBUTO DE LA EMPRESA
                    //si completo alguno de los campos (f_vencimiento o observacion)
                    if (rowDetalle.GetAttr("f_vencimiento") != "" || rowDetalle.GetAttr("o_atrib_emp_con") != "")
                    {
                        //creo un nuevo registro
                        NucleusRH.Base.Control_Contratistas.Empresas.ATRIB_EMP_CON atributo = NucleusRH.Base.Control_Contratistas.Empresas.ATRIB_EMP_CON.New();
                        
                        //setea el atributo de control
                        atributo.oi_atributo_ctrol = rowDetalle.GetAttr("id");

                        //fecha de vencimiento
                        if (rowDetalle.GetAttrDateTime("f_vencimiento") != null && rowDetalle.GetAttr("f_vencimiento") != "")
                        {
                            atributo.f_vencimiento = rowDetalle.GetAttrDateTime("f_vencimiento");
                        }
                        else { atributo.f_vencimientoNull = true; }

                        //observacion
                        if (rowDetalle.GetAttr("o_atrib_emp_con") != null)
                        { atributo.o_atrib_emp_con = rowDetalle.GetAttr("o_atrib_emp_con"); }

                        //se lo agrega a al arreglo de atributos del legajo
                        paramEMPRESA.ATRIB_EMP_CON.Add(atributo);

                    }
                
                }
               

             }

            objTransaccion.Save(paramEMPRESA);
            objTransaccion.Commit();
    

            NomadLog.Info("******FIN Guardar Atributos de Control ******");
          }
          catch (Exception e)
          {
              objTransaccion.Rollback();
              throw new NomadAppException("Error no manejado. " + e.Message);
          }
            


            
        }

        public static void CargarAtributosControl(NucleusRH.Base.Control_Contratistas.Empresas.EMPRESA paramEMPRESA)
        {
            //Atributos de control de la empresa
            //defino el parámetro de tipo="empresa"
            NomadXML xmlParam = new NomadXML("DATA");
            xmlParam.SetAttr("tipo", "E");

            //Ejecuto el recurso para recuperar los atributos de control
            NomadXML XML = Nomad.NSystem.Base.NomadEnvironment.QueryNomadXML(NucleusRH.Base.Control_Contratistas.Atributos_Control.ATRIBUTO_CTROL.Resources.INFO_TIPO, xmlParam.ToString());

            //los guardo en la empresa
            for (NomadXML row = XML.FirstChild().FirstChild(); row != null; row = row.Next())
            {
                NucleusRH.Base.Control_Contratistas.Empresas.ATRIB_EMP_CON new_atrib = NucleusRH.Base.Control_Contratistas.Empresas.ATRIB_EMP_CON.New();
                new_atrib.oi_atributo_ctrol = row.GetAttr("id");
                paramEMPRESA.ATRIB_EMP_CON.Add(new_atrib);
            }

            NomadEnvironment.GetCurrentTransaction().Save(paramEMPRESA);
        }
    
    
    }
}

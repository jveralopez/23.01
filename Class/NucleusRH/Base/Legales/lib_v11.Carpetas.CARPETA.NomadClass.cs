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

using System.Text;

namespace NucleusRH.Base.Legales.Carpetas
{

    //////////////////////////////////////////////////////////////////////////////////
    //Clase Carpetas
    public partial class CARPETA : Nomad.NSystem.Base.NomadObject
    {
        public static Nomad.NSystem.Proxy.NomadXML SAVE(Nomad.NSystem.Proxy.NomadXML DDO_CARPETAS, Nomad.NSystem.Proxy.NomadXML DDO_PARTES, Nomad.NSystem.Proxy.NomadXML DDO_OBJETOS_REC, ref Nomad.NSystem.Proxy.NomadXML DDO_MONTOS, Nomad.NSystem.Proxy.NomadXML XML_TIPO, Nomad.NSystem.Proxy.NomadXML DDO_TAREAS, Nomad.NSystem.Proxy.NomadXML DDO_DOCUMENTOS, Nomad.NSystem.Proxy.NomadXML DDO_ATRIBUTOS, Nomad.NSystem.Proxy.NomadXML DDO_RADICACIONES)
        {
            NomadEnvironment.GetTrace().Info("DDO_CARPETAS: " + DDO_CARPETAS.ToString());
            NomadEnvironment.GetTrace().Info("DDO_PARTES: " + DDO_PARTES.ToString());
            NomadEnvironment.GetTrace().Info("DDO_OBJETOS_REC: " + DDO_OBJETOS_REC.ToString());
            NomadEnvironment.GetTrace().Info("DDO_MONTOS: " + DDO_MONTOS.ToString());
            NomadEnvironment.GetTrace().Info("XML_TIPO: " + XML_TIPO.ToString());
            NomadEnvironment.GetTrace().Info("DDO_TAREAS: " + DDO_TAREAS.ToString());
            NomadEnvironment.GetTrace().Info("DDO_DOCUMENTOS: " + DDO_DOCUMENTOS.ToString());
            NomadEnvironment.GetTrace().Info("DDO_ATRIBUTOS: " + DDO_ATRIBUTOS.ToString());
            NomadEnvironment.GetTrace().Info("DDO_RADICACIONES: " + DDO_RADICACIONES.ToString());
             
            
            Nomad.NSystem.Proxy.NomadXML RESULT = new Nomad.NSystem.Proxy.NomadXML("ROW");
            //NO PUEDE VENIR SIN PARTES!! MODIFICAR CARPETA LO PERMITE ENTONCES VALIDO
            if (DDO_PARTES.FirstChild().ChildLength == 0)
            {
                RESULT.SetAttr("Error", "Debe Cargar al menos una Parte");
                return RESULT;
            }            
            
            //VALIDACIONES DEL TIPO DE CARPETA           
            NomadXML xmlParam = new NomadXML("DATA");
            xmlParam.SetAttr("oi_tipo_carpeta", DDO_CARPETAS.FirstChild().GetAttr("oi_tipo_carpeta"));
           
            NomadXML xmlObjAttrReq = Nomad.NSystem.Base.NomadEnvironment.QueryNomadXML(NucleusRH.Base.Legales.TiposCarpeta.TIPO_CARPETA.Resources.GetAttributosRequeridos, xmlParam.ToString());

            //RECORRO LAS PARTES
            for (NomadXML rowParte = DDO_PARTES.FirstChild().FirstChild(); rowParte != null; rowParte = rowParte.Next())
            {
               NomadEnvironment.GetTrace().Info("RowParte: "+rowParte.ToString());
               string strOiRol = rowParte.GetAttr("rol");
               NomadXML xmlParam2 = new NomadXML("DATA");
               xmlParam2.SetAttr("oi_rol", strOiRol);

               //si el tipo de carpeta requiere objeto de reclamo
               if (xmlObjAttrReq.FirstChild().GetAttr("c_req_obj_carpeta") == "1")
               {
                    NomadXML xmlResultParte = Nomad.NSystem.Base.NomadEnvironment.QueryNomadXML(NucleusRH.Base.Legales.Carpetas.PARTE.Resources.permiteObjeto,xmlParam2.ToString());
                   
                    //si el rol permite objeto de reclamo
                    if (xmlResultParte.FirstChild().GetAttr("c_objeto_carpeta") == "1")
                    { 
                        bool bolEncontro = false;
                        //SI PERMITE OR, VALIDO QUE LA PARTE TENGA AL MENOS 1 OR (PORQUE LO REQUIERE EL TIPO DE CARPETA)
                        for (NomadXML rowOR = DDO_OBJETOS_REC.FirstChild().FirstChild(); rowOR != null;  rowOR = rowOR.Next())
                        {
                            NomadEnvironment.GetTrace().Info("RowOR: " + rowOR.ToString());
                            if ((rowOR.GetAttr("sujeto") == rowParte.GetAttr("sujeto")) && (rowOR.GetAttr("rol") == rowParte.GetAttr("rol")) && (rowOR.GetAttr("l_inactivo") != "1"))
                            {
                                bolEncontro = true;
                                break;
                            }

                        }
                        if (!bolEncontro)
                        {
                            RESULT.SetAttr("Error", "Debe cargar objeto reclamo a una parte de la carpeta.");
                            return RESULT;
                        }

                    } //fin if (xmlResultParte.FirstChild().GetAttr("c_objeto_carpeta") == "1") (si el rol permite objeto de reclamo)

               } //fin if (xmlObjAttrReq.FirstChild().GetAttr("c_req_obj_carpeta") == "1") (si el tipo de carpeta requiere objeto de reclamo)
                 
                string mensajeOR="";

               //si permite OR, validos que el objeto de reclamo sea de los permitidos por el tipo de carpeta
               for (NomadXML rowOR = DDO_OBJETOS_REC.FirstChild().FirstChild(); rowOR != null; rowOR = rowOR.Next())
               {
                    NomadXML xmlParamObjRec = new NomadXML("DATA");
                    xmlParamObjRec.SetAttr("oi_tipo_carpeta", DDO_CARPETAS.FirstChild().GetAttr("oi_tipo_carpeta"));
                    xmlParamObjRec.SetAttr("oi_objeto_reclamo", rowOR.GetAttr("objeto_reclamo"));
                    NomadXML xmlResultObjeto = Nomad.NSystem.Base.NomadEnvironment.QueryNomadXML(NucleusRH.Base.Legales.TiposCarpeta.OBJ_REC_TIPO_CARPETA.Resources.esReclamoValido, xmlParamObjRec.ToString());

                    if (rowOR.GetAttr("sujeto") == rowParte.GetAttr("sujeto") && rowOR.GetAttr("rol") == rowParte.GetAttr("rol") && rowOR.GetAttr("l_inactivo") != "1") //si encuentra el objeto activo
                    {
                      if (xmlResultObjeto.FirstChild().GetAttr("valido") == "0")
                      {
                          string desc_or = NucleusRH.Base.Legales.Objetos.OBJETO_RECLAMO.Get(rowOR.GetAttr("objeto_reclamo")).d_objeto_reclamo;
                          mensajeOR += "El objeto de reclamo \"" + desc_or + "\" no es válido para el tipo de carpeta.\n";
                      }
                    }
                }

               if (mensajeOR != "")
               {
                   RESULT.SetAttr("Error", mensajeOR);
                   return RESULT;
               }

                   
            } //fin cada parte
            

            if (xmlObjAttrReq.FirstChild().GetAttr("c_req_carga_monto") == "1")
            {  
                //Cuento los montos vigentes del DDO_MONTOS
                int intCantMontos = 0;
                int intCantObjRec = 0;
                for (NomadXML rowMonto = DDO_MONTOS.FirstChild().FirstChild(); rowMonto != null; rowMonto = rowMonto.Next())
                {
                    if ((rowMonto.GetAttr("l_vigente") == "1") && (rowMonto.GetAttr("n_monto") != ""))
                    {
                        intCantMontos++;
                    }
                }
                //Cuento la cantidad de objetos reclamo activos
                for (NomadXML rowOC = DDO_OBJETOS_REC.FirstChild().FirstChild(); rowOC != null; rowOC = rowOC.Next())
                { 
                   if(rowOC.GetAttr("l_inactivo") != "1")
                   intCantObjRec++;
                }


                NomadEnvironment.GetTrace().Info("CantOC: " + intCantObjRec.ToString());
                NomadEnvironment.GetTrace().Info("CantMontos: " + intCantMontos.ToString());
                //Valido que tenga tantos objetos reclamo activos como montos vigentes               
                if (intCantObjRec != intCantMontos)
                {
                    RESULT.SetAttr("Error", "Debe Cargar Montos");
                    return RESULT;
                }
            } //fin if (xmlObjAttrReq.FirstChild().GetAttr("c_req_carga_monto") == "1") 
          
                 
            string mensajeMontos = "";

            //valido que el tipo de monto sea permitido por el tipo de carpeta          
            NomadXML xmlParamMonto = new NomadXML("DATA");
            xmlParamMonto.SetAttr("oi_tipo_carpeta", DDO_CARPETAS.FirstChild().GetAttr("oi_tipo_carpeta"));
            xmlParamMonto.SetAttr("oi_tipo_monto", XML_TIPO.FirstChild().GetAttr("oi_tipo_monto"));

            if(xmlParamMonto.GetAttr("oi_tipo_monto")!="")
            {
                NomadXML xmlResultMonto = Nomad.NSystem.Base.NomadEnvironment.QueryNomadXML(NucleusRH.Base.Legales.TiposCarpeta.TIPO_MONTO_TIPO_CARPETA.Resources.esMontoValido, xmlParamMonto.ToString());
            
                if (xmlResultMonto.FirstChild().GetAttr("valido") == "0")
                mensajeMontos = "El tipo de monto actual no es válido para el tipo de carpeta";
 
                if (mensajeMontos != "")
                {
                    RESULT.SetAttr("Error", mensajeMontos);
                    return RESULT;
                }
            }
                     
            if (xmlObjAttrReq.FirstChild().GetAttr("c_req_atr_proc") == "1")
            {
                if (DDO_ATRIBUTOS.FirstChild().ChildLength == 0)
                {
                    RESULT.SetAttr("Error", "Debe Cargar Atributos Procesales");
                    return RESULT;
                }
            }
            if (xmlObjAttrReq.FirstChild().GetAttr("c_req_rad_tram") == "1")
            {
                if (DDO_RADICACIONES.FirstChild().ChildLength == 0)
                {
                    RESULT.SetAttr("Error", "Debe Cargar Radicación del Tramite");
                    return RESULT;
                }
            }

            //La carpeta debe tener documentos
            if (DDO_DOCUMENTOS.FirstChild().ChildLength == 0)
            {
                RESULT.SetAttr("Error", "Debe Cargar Documentos");
                return RESULT;
            }

            //FIN VALIDACIONES TIPO DE CARPETA

            string strOiCarpeta = "";
            NucleusRH.Base.Legales.Carpetas.CARPETA carpeta;

            NomadEnvironment.GetTrace().Info(":1:");
            carpeta =  SAVE_CARPETA(DDO_CARPETAS);
            strOiCarpeta = carpeta.Id;
            DDO_PARTES = NucleusRH.Base.Legales.Carpetas.CARPETA.SAVE_PARTES(ref DDO_PARTES, strOiCarpeta);
            DDO_OBJETOS_REC = NucleusRH.Base.Legales.Carpetas.CARPETA.SAVE_OBJ_REC(ref DDO_OBJETOS_REC, DDO_PARTES, strOiCarpeta);
            NomadEnvironment.GetTrace().Info(":2: " + DDO_MONTOS.FirstChild().ChildLength);
           // DDO_MONTOS = NucleusRH.Base.Legales.Carpetas.CARPETA.SAVE_MONTOS(ref DDO_MONTOS, XML_TIPO, strOiCarpeta, DDO_PARTES, DDO_OBJETOS_REC);
            SAVE_MONTOS_EDIT(ref DDO_MONTOS, strOiCarpeta, XML_TIPO);
            DDO_TAREAS = NucleusRH.Base.Legales.Carpetas.CARPETA.SAVE_TAREAS(ref DDO_TAREAS, strOiCarpeta);
            DDO_DOCUMENTOS = NucleusRH.Base.Legales.Carpetas.CARPETA.SAVE_DOCUMENTOS(ref DDO_DOCUMENTOS, strOiCarpeta);
            DDO_ATRIBUTOS = NucleusRH.Base.Legales.Carpetas.CARPETA.SAVE_ATRIBUTOS(ref DDO_ATRIBUTOS, strOiCarpeta);
            DDO_RADICACIONES = NucleusRH.Base.Legales.Carpetas.CARPETA.SAVE_RADICACIONES(DDO_RADICACIONES, strOiCarpeta);


            /////////BORRO LOS REGISTROS QUE CORRESPONDAN.-
            //Nomad.NSystem.Proxy.NomadXML DELETE = new Nomad.NSystem.Proxy.NomadXML("ROWS");        

            carpeta = NucleusRH.Base.Legales.Carpetas.CARPETA.Get(strOiCarpeta);
            NomadLog.Info("::carpeta carmen1:: " + carpeta.SerializeAll());
            NomadLog.Info("::DDO_MONTOS:: " + DDO_MONTOS.ToString());

            Nomad.NSystem.Proxy.NomadXML row;
            Nomad.NSystem.Proxy.NomadXML rowObj;
            //Nomad.NSystem.Proxy.NomadXML rowMonto;
            Nomad.NSystem.Proxy.NomadXML rowMontoXML;
            Nomad.NSystem.Proxy.NomadXML rowDel;
            Hashtable deletedParte = new Hashtable();
            Hashtable deletedObjeto = new Hashtable();

            Nomad.NSystem.Proxy.NomadXML DELETE = new Nomad.NSystem.Proxy.NomadXML("ROWS");
            //RECORRO LAS PARTES DE LA CARPETA      
            foreach (NucleusRH.Base.Legales.Carpetas.PARTE parte in carpeta.PARTES)
            {
                int parteToDel = 1;

                //RECORRO LAS PARTES DEL XML DE PARAMETRO Y BUSCO SI ALGUNA PARTE SE ELIMINO.
                for (row = DDO_PARTES.FirstChild().FirstChild(); row != null; row = row.Next())
                {
                    if ((Convert.ToString(parte.oi_rol) == row.GetAttrString("rol")) && (Convert.ToString(parte.oi_sujeto) == row.GetAttrString("sujeto")))
                        parteToDel = 0;
                }
                //SI EXISTE ALGUNA PARTE QUE SE HAYA ELIMINADO RECORRO LOS OBJETOS DE ESA PARTE PARA ELIMINAR SUS MONTOS
                if (parteToDel != 0)
                {
                    Nomad.NSystem.Proxy.NomadXML PARTE_DEL = new Nomad.NSystem.Proxy.NomadXML("PARTE");
                    //RECORRO LOS OBJETOS DE CADA UNA DE LAS PARTES;
                    foreach (NucleusRH.Base.Legales.Carpetas.OBJ_REC_CARPETA obj_carpeta in parte.OBJ_CARPETA)
                    {
                        //BUSCO LOS MONTOS QUE EXISTEN PARA EL OBJETO CARPETA.
                        string param = "<DATOS oi_obj_rec_carpeta=\"" + obj_carpeta.id + "\"/>";
                        NomadEnvironment.GetTrace().Info("::PARAM::" + param);
                        NomadXML docIDMonto = NomadEnvironment.QueryNomadXML(NucleusRH.Base.Legales.Carpetas.CARPETA.Resources.getMontosByObjetos, param).FirstChild();
                        NomadEnvironment.GetTrace().Info("docIDMonto: " + docIDMonto.ToString());
                        if (docIDMonto.GetAttr("id") != "0")
                        {
                            //RECORRO LOS MONTOS Y LOS ELIMINO
                            //for(rowMonto=docIDMonto.FirstChild(); rowMonto!= null; rowMonto=rowMonto.Next())
                            //{                                                                        
                            NomadEnvironment.GetTrace().Info("docIDMonto: " + docIDMonto.ToString());
                            NucleusRH.Base.Legales.Montos.MONTO monto = NucleusRH.Base.Legales.Montos.MONTO.Get(docIDMonto.GetAttr("id"));
                            for (rowMontoXML = DDO_MONTOS.FirstChild().FirstChild(); rowMontoXML != null; rowMontoXML = rowMontoXML.Next())
                            {
                                if (rowMontoXML.GetAttr("id") == monto.Id)
                                    DDO_MONTOS.FirstChild().DeleteChild(rowMontoXML);
                            }
                            NomadEnvironment.GetCurrentTransaction().Delete(monto);
                            //}                                                                     			
                        }
                    }
                    NomadEnvironment.GetTrace().Info(":::Eliminar parte::: ");
                    //AGREGO LA PARTE A ELIMINAR EN EL XML 
                    deletedParte.Add(parte.Id, parte);
                    PARTE_DEL.SetAttr("oi_parte", parte.Id);
                    PARTE_DEL.SetAttr("TODELETE", "PARTE");
                    DELETE.AddXML(PARTE_DEL);
                    //carpeta.PARTES.Remove(parte);
                    //NomadEnvironment.GetCurrentTransaction().SaveRefresh(carpeta); 
                    NomadEnvironment.GetTrace().Info(":::Parte agregada al XML::: " + DELETE.ToString());
                }
                NomadEnvironment.GetTrace().Info("::carpeta::" + carpeta.SerializeAll());
            }




            //RECORRO LAS PARTES DE LA CARPETA
            foreach (NucleusRH.Base.Legales.Carpetas.PARTE parte in carpeta.PARTES)
            {

                //RECORRO LOS OBJETOS DE LAS PARTES NO ELIMINADAS
                foreach (NucleusRH.Base.Legales.Carpetas.OBJ_REC_CARPETA obj_carpeta in parte.OBJ_CARPETA)
                {
                    int objToDel = 1;
                    for (rowObj = DDO_OBJETOS_REC.FirstChild().FirstChild(); rowObj != null; rowObj = rowObj.Next())
                    {
                        if ((Convert.ToString(obj_carpeta.oi_objeto_reclamo) == rowObj.GetAttrString("objeto_reclamo")) && (Convert.ToString(parte.oi_sujeto) == rowObj.GetAttrString("sujeto")))
                            objToDel = 0;
                    }
                    //SI EXISTE ALGUN OBJETO CARPETA ELIMINADO BUSCO LOS MONTOS QUE TIENE Y LOS ELIMINO
                    if (objToDel != 0)
                    {
                        Nomad.NSystem.Proxy.NomadXML OBJ_DEL = new Nomad.NSystem.Proxy.NomadXML("OBJETO");
                        string param = "<DATOS oi_obj_rec_carpeta=\"" + obj_carpeta.id + "\"/>";
                        NomadEnvironment.GetTrace().Info("::PARAM::" + param);
                        NomadXML docIDMonto = NomadEnvironment.QueryNomadXML(NucleusRH.Base.Legales.Carpetas.CARPETA.Resources.getMontosByObjetos, param).FirstChild();
                        NomadEnvironment.GetTrace().Info("docIDMonto: " + docIDMonto.ToString());
                        if (docIDMonto.GetAttr("id") != "0")
                        {
                            //ELIMINO LOS MONTOS                        
                            NomadEnvironment.GetTrace().Info("::ELIMINA MONTOS::");
                            //for(rowMonto=docIDMonto.FirstChild().FirstChild(); rowMonto!= null; rowMonto=rowMonto.Next())
                            //{
                            NucleusRH.Base.Legales.Montos.MONTO monto = NucleusRH.Base.Legales.Montos.MONTO.Get(docIDMonto.GetAttr("id"));
                            for (rowMontoXML = DDO_MONTOS.FirstChild().FirstChild(); rowMontoXML != null; rowMontoXML = rowMontoXML.Next())
                            {
                                if (rowMontoXML.GetAttr("id") == monto.Id)
                                    DDO_MONTOS.FirstChild().DeleteChild(rowMontoXML);
                            }
                            NomadEnvironment.GetCurrentTransaction().Delete(monto);
                            //}                   
                        }
                        //ELIMINO LOS OBJETOS DE LA CARPETA
                        NomadEnvironment.GetTrace().Info(":::Eliminar objeto::: ");
                        int existeParte = 0;
                        for (rowDel = DELETE.FirstChild(); rowDel != null; rowDel = rowDel.Next())
                        {
                            if ((rowDel.GetAttrInt("oi_parte") == obj_carpeta.oi_parte) && (rowDel.GetAttr("TODELETE") == "PARTE"))
                                existeParte = 1;
                        }

                        if (existeParte == 0)
                        {
                            //AGREGO LA PARTE A ELIMINAR EN EL XML
                            deletedObjeto.Add(obj_carpeta.Id, obj_carpeta);
                            OBJ_DEL.SetAttr("oi_parte", obj_carpeta.oi_parte);
                            OBJ_DEL.SetAttr("oi_objeto", obj_carpeta.Id);
                            OBJ_DEL.SetAttr("TODELETE", "OBJ");
                            DELETE.AddXML(OBJ_DEL);
                            //carpeta.PARTES.Remove(parte);
                            //NomadEnvironment.GetCurrentTransaction().SaveRefresh(carpeta); 
                            NomadEnvironment.GetTrace().Info(":::objeto agregado al XML::: " + DELETE.ToString());
                        }
                        //parte.OBJ_CARPETA.Remove(obj_carpeta);     
                        //NomadEnvironment.GetCurrentTransaction().SaveRefresh(carpeta); 
                    }
                }
            }


            for (rowDel = DELETE.FirstChild(); rowDel != null; rowDel = rowDel.Next())
            {
                if (rowDel.GetAttr("TODELETE") == "PARTE")
                {
                    NomadEnvironment.GetTrace().Info(":::BORRAR PARTE::: ");

                    foreach (PARTE deleted_parte in deletedParte.Values)
                    {
                        if (deleted_parte.Id == rowDel.GetAttr("oi_parte"))
                        {
                            NomadLog.Info("::deleted_parte:: " + deleted_parte.SerializeAll());

                            NucleusRH.Base.Legales.HistCambios.HIST_CAMBIO histCambio = new NucleusRH.Base.Legales.HistCambios.HIST_CAMBIO();
                            histCambio.oi_carpeta = Convert.ToString(carpeta.id);
                            histCambio.f_cambio = DateTime.Now;
                            NomadXML etty = Nomad.NSystem.Proxy.NomadProxy.GetProxy().GetEtty();
                            histCambio.d_usuario = etty.FirstChild().GetAttr("COD") + " - " + etty.FirstChild().GetAttr("DES");
                            NomadXML XMLHist = new NomadXML(deleted_parte.SerializeAll());
                            XMLHist.FirstChild().SetAttr("ACCION", "DELETE");
                            histCambio.o_xml_cambio = Convert.ToString(XMLHist);

                            NomadEnvironment.GetTrace().Info("::histCambio:: " + histCambio.SerializeAll());
                            NomadEnvironment.GetCurrentTransaction().SaveRefresh(histCambio);

                            carpeta.PARTES.Remove(deleted_parte);
                        }
                    }
                    NomadLog.Info("::CARPETA-REMOVE:: " + carpeta.SerializeAll());
                }
                if (rowDel.GetAttr("TODELETE") == "OBJ")
                {
                    NomadEnvironment.GetTrace().Info(":::BORRAR OBJETO::: ");
                    foreach (NucleusRH.Base.Legales.Carpetas.PARTE parte in carpeta.PARTES)
                    {
                        if (parte.Id == rowDel.GetAttr("oi_parte"))
                        {
                            foreach (OBJ_REC_CARPETA deleted_objeto in deletedObjeto.Values)
                            {
                                NucleusRH.Base.Legales.HistCambios.HIST_CAMBIO histCambio = new NucleusRH.Base.Legales.HistCambios.HIST_CAMBIO();
                                histCambio.oi_carpeta = Convert.ToString(carpeta.id);
                                histCambio.f_cambio = DateTime.Now;
                                NomadXML etty = Nomad.NSystem.Proxy.NomadProxy.GetProxy().GetEtty();
                                histCambio.d_usuario = etty.FirstChild().GetAttr("COD") + " - " + etty.FirstChild().GetAttr("DES");
                                NomadXML XMLHist = new NomadXML(deleted_objeto.SerializeAll());
                                XMLHist.FirstChild().SetAttr("ACCION", "DELETE");
                                histCambio.o_xml_cambio = Convert.ToString(XMLHist);

                                NomadEnvironment.GetTrace().Info("::histCambio:: " + histCambio.SerializeAll());
                                NomadEnvironment.GetCurrentTransaction().SaveRefresh(histCambio);

                                parte.OBJ_CARPETA.Remove(deleted_objeto);
                            }
                        }
                    }
                    //foreach(PARTE deleted_objeto in deletedObjeto.Values)
                    //carpeta.PARTES.Remove(deleted_objeto);
                }
            }
            if ((DDO_CARPETAS.FirstChild().GetAttr("c_carpeta") == "0") || (DDO_CARPETAS.FirstChild().GetAttr("c_carpeta") == ""))
            {
                string param = "<DATOS oi_empresa=\"" + DDO_CARPETAS.FirstChild().GetAttr("oi_empresa") + "\"/>";
                NomadEnvironment.GetTrace().Info("::PARAM::" + param);
                NomadXML codigo = NomadEnvironment.QueryNomadXML(NucleusRH.Base.Legales.Carpetas.CARPETA.Resources.getCodigo, param).FirstChild();
                NomadEnvironment.GetTrace().Info("codigo: " + codigo.ToString());
                carpeta.c_carpeta = codigo.GetAttr("c_carpeta");
            }
            else
                carpeta.c_carpeta = DDO_CARPETAS.FirstChild().GetAttr("c_carpeta");
            NomadEnvironment.GetCurrentTransaction().SaveRefresh(carpeta);

           
            RESULT.SetAttr("oi_carpeta", strOiCarpeta);
            RESULT.SetAttr("c_carpeta", carpeta.c_carpeta);

            NomadLog.Debug("oi_carpeta: " + carpeta.id.ToString());

            //solo ejecuta esto si se está guardando una carpeta nueva
            if (DDO_CARPETAS.FirstChild().GetAttr("nueva") == "1")
                Carpetas.CARPETA.evePostSaveCarpeta(carpeta.id);
            
            return RESULT;
        }

        public static NucleusRH.Base.Legales.Carpetas.CARPETA SAVE_CARPETA(Nomad.NSystem.Proxy.NomadXML DDO_CARPETAS)
        {
            NomadEnvironment.GetTrace().Info("::INICIA EL METODO SAVE-CARPETA::");
            NucleusRH.Base.Legales.Carpetas.CARPETA new_carpeta;
            if (DDO_CARPETAS.FirstChild().GetAttr("oi_carpeta") == "")
                new_carpeta = new NucleusRH.Base.Legales.Carpetas.CARPETA();
            else
                new_carpeta = NucleusRH.Base.Legales.Carpetas.CARPETA.Get(DDO_CARPETAS.FirstChild().GetAttr("oi_carpeta"));

            
            if (DDO_CARPETAS.FirstChild().GetAttr("oi_carpeta") != "" && DDO_CARPETAS.FirstChild().GetAttr("oi_tipo_car_ant") != "" && DDO_CARPETAS.FirstChild().GetAttr("oi_est_car_ant") != "" && DDO_CARPETAS.FirstChild().GetAttr("o_hist_tipo_car") != "")
            {
                NomadEnvironment.GetTrace().Info("::INICIA CAMBIO DE TIPO 26.05::");
                Nomad.NSystem.Proxy.NomadXML DDO_HIST_TIPO_CAR = new NomadXML("PARAMS");
                DDO_HIST_TIPO_CAR.SetAttr("oi_tipo_car_ant",DDO_CARPETAS.FirstChild().GetAttr("oi_tipo_car_ant"));
                DDO_HIST_TIPO_CAR.SetAttr("oi_est_car_ant", DDO_CARPETAS.FirstChild().GetAttr("oi_est_car_ant"));
                DDO_HIST_TIPO_CAR.SetAttr("oi_tipo_car_nu", DDO_CARPETAS.FirstChild().GetAttr("oi_tipo_carpeta"));
                DDO_HIST_TIPO_CAR.SetAttr("oi_est_car_nu", DDO_CARPETAS.FirstChild().GetAttr("oi_estado_carpeta"));
                DDO_HIST_TIPO_CAR.SetAttr("o_hist_tipo_car", DDO_CARPETAS.FirstChild().GetAttr("o_hist_tipo_car"));

                new_carpeta = NucleusRH.Base.Legales.TiposCarpeta.TIPO_CARPETA.SAVE_TIPO_CARPETA(DDO_HIST_TIPO_CAR, DDO_CARPETAS.FirstChild().GetAttr("oi_carpeta"));
                NomadEnvironment.GetTrace().Info("::carpeta carmen2::" + new_carpeta.SerializeAll());
            }

            new_carpeta.d_carpeta = DDO_CARPETAS.FirstChild().GetAttr("d_carpeta");
            new_carpeta.oi_estado_carpeta = DDO_CARPETAS.FirstChild().GetAttr("oi_estado_carpeta");
            new_carpeta.oi_sujeto = DDO_CARPETAS.FirstChild().GetAttr("sujeto");
            new_carpeta.oi_tipo_monto = DDO_CARPETAS.FirstChild().GetAttr("oi_tipo_monto");
            new_carpeta.oi_tipo_carpeta = DDO_CARPETAS.FirstChild().GetAttr("oi_tipo_carpeta"); 
            new_carpeta.c_criticidad = DDO_CARPETAS.FirstChild().GetAttr("c_criticidad");
            new_carpeta.f_carpeta = DDO_CARPETAS.FirstChild().GetAttrDateTime("f_carpeta");
            if (DDO_CARPETAS.FirstChild().GetAttr("f_prescripcion")=="")
            {
                new_carpeta.f_prescripcionNull = true;
            }
            else 
            {
                new_carpeta.f_prescripcion = DDO_CARPETAS.FirstChild().GetAttrDateTime("f_prescripcion");
            }
            new_carpeta.c_accion = DDO_CARPETAS.FirstChild().GetAttr("c_accion");
            new_carpeta.oi_empresa = DDO_CARPETAS.FirstChild().GetAttr("oi_empresa");
            new_carpeta.oi_ubicacion = DDO_CARPETAS.FirstChild().GetAttr("oi_ubicacion");
            new_carpeta.l_instancia = DDO_CARPETAS.FirstChild().GetAttrBool("l_instancia");
            new_carpeta.o_carpeta = DDO_CARPETAS.FirstChild().GetAttr("o_carpeta");
            new_carpeta.c_enlace = DDO_CARPETAS.FirstChild().GetAttr("c_enlace");
            new_carpeta.d_custom_1 = DDO_CARPETAS.FirstChild().GetAttr("c_opcion");

            if (DDO_CARPETAS.FirstChild().GetAttr("seguridad") == "ADMIN")
            {
                new_carpeta.c_origen_carga = "LEG";
            }
            if (DDO_CARPETAS.FirstChild().GetAttr("seguridad") == "USER")
            {
                new_carpeta.c_origen_carga = "SUC";
            }
            NomadEnvironment.GetCurrentTransaction().SaveRefresh(new_carpeta);
            NomadEnvironment.GetTrace().Info("::FINALIZA EL METODO SAVE-CARPETA::");

            return new_carpeta;
        }

        public static Nomad.NSystem.Proxy.NomadXML SAVE_PARTES(ref Nomad.NSystem.Proxy.NomadXML DDO_PARTES, string strOiCarpeta)
        {
            NomadEnvironment.GetTrace().Info("::INICIA EL METODO SAVE-PARTES::");
            NucleusRH.Base.Legales.Carpetas.CARPETA carpeta = NucleusRH.Base.Legales.Carpetas.CARPETA.Get(strOiCarpeta);

            Nomad.NSystem.Proxy.NomadXML row;

            for (row = DDO_PARTES.FirstChild().FirstChild(); row != null; row = row.Next())
            {
                //BUSCO SI LA PARTE YA FUE CREADA.
                int existe_parte = 0;
                foreach (NucleusRH.Base.Legales.Carpetas.PARTE parte in carpeta.PARTES)
                {
                    //SI LA PARTE EXISTE SETEO LOS ATRIBUTOS CORRESPONDIENTES.
                    if ((Convert.ToString(parte.oi_rol) == row.GetAttrString("rol")) && (Convert.ToString(parte.oi_sujeto) == row.GetAttrString("sujeto")))
                    {
                        parte.o_parte = row.GetAttr("parte");
                        existe_parte = 1;
                    }
                }
                //EN CASO DE NO EXISTE LA PARTE, LA CREO.
                if (existe_parte == 0)
                {
                    NucleusRH.Base.Legales.Carpetas.PARTE new_parte = new NucleusRH.Base.Legales.Carpetas.PARTE();
                    new_parte.oi_rol = row.GetAttr("rol");
                    new_parte.oi_sujeto = row.GetAttr("sujeto");
                    new_parte.o_parte = row.GetAttr("parte");
                    carpeta.PARTES.Add(new_parte);
                }
            }

            NomadEnvironment.GetCurrentTransaction().SaveRefresh(carpeta);
            NomadEnvironment.GetTrace().Info("::FINALIZA EL METODO SAVE-PARTES::");
            return DDO_PARTES;
        }

        public static Nomad.NSystem.Proxy.NomadXML SAVE_OBJ_REC(ref Nomad.NSystem.Proxy.NomadXML DDO_OBJETOS_REC, Nomad.NSystem.Proxy.NomadXML DDO_PARTES, string strOiCarpeta)
        {
            NomadEnvironment.GetTrace().Info("::INICIA EL METODO SAVE-OBJETOS::");
            NucleusRH.Base.Legales.Carpetas.CARPETA carpeta = NucleusRH.Base.Legales.Carpetas.CARPETA.Get(strOiCarpeta);

            //RECORRO LAS PARTES DE LA CARPETA
            foreach (NucleusRH.Base.Legales.Carpetas.PARTE parte in carpeta.PARTES)
            {
                NomadEnvironment.GetTrace().Info("::1::");
                Nomad.NSystem.Proxy.NomadXML row;
                //RECORRO LAS PARTES DEL DOCUMENTO XML
                for (row = DDO_PARTES.FirstChild().FirstChild(); row != null; row = row.Next())
                {
                    NomadEnvironment.GetTrace().Info("::parte.oi_rol::" + Convert.ToString(parte.oi_rol));
                    NomadEnvironment.GetTrace().Info("::parte.oi_sujeto::" + Convert.ToString(parte.oi_sujeto));
                    NomadEnvironment.GetTrace().Info("::row::" + row.ToString());
                    //BUSCO LA PARTE QUE COINCIDE ENTRE EL XML Y LA PARTE DEL DDO
                    if ((Convert.ToString(parte.oi_rol) == row.GetAttrString("rol")) && (Convert.ToString(parte.oi_sujeto) == row.GetAttrString("sujeto")))
                    {
                        NomadEnvironment.GetTrace().Info("::3::");
                        Nomad.NSystem.Proxy.NomadXML rowObj;
                        //RECORRO LOS OBJETOS RECLAMO CARPETA DEL XML
                        for (rowObj = DDO_OBJETOS_REC.FirstChild().FirstChild(); rowObj != null; rowObj = rowObj.Next())
                        {
                            int existe_objeto = 0;
                            //RECORRO LOS OBJETOS DE CADA UNA DE LAS PARTES DE LA CARPETA
                            foreach (NucleusRH.Base.Legales.Carpetas.OBJ_REC_CARPETA obj_carpeta in parte.OBJ_CARPETA)
                            {
                                //BUSCO SI EL OBJETO DEL XML YA ES PARTE DEL DDO Y EN CASO DE SERLO ACTUALIZO LA CAUSA.
                                if (Convert.ToString(obj_carpeta.oi_objeto_reclamo) == rowObj.GetAttrString("objeto_reclamo"))
                                {
                                    obj_carpeta.oi_causa = rowObj.GetAttr("causa");
                                    if (rowObj.GetAttr("l_inactivo") == "1") obj_carpeta.l_inactivo = true;
                                    else obj_carpeta.l_inactivo = false;
                                    existe_objeto = 1;
                                }
                            }
                            //EN CASO DE QUE EL OBJETO DEL XML NO EXISTA EN EL DDO, LO CREO.
                            if (existe_objeto == 0)
                            {
                                NomadEnvironment.GetTrace().Info("::4::");
                                //BUSCO EL OBJETO DEL XML QUE COINCIDA CON LA PARTE
                                if (Convert.ToString(parte.oi_rol) == rowObj.GetAttrString("rol") && Convert.ToString(parte.oi_sujeto) == rowObj.GetAttrString("sujeto"))
                                {
                                    NomadEnvironment.GetTrace().Info("::5::");
                                    NucleusRH.Base.Legales.Carpetas.OBJ_REC_CARPETA new_objeto = new NucleusRH.Base.Legales.Carpetas.OBJ_REC_CARPETA();
                                    new_objeto.oi_objeto_reclamo = rowObj.GetAttr("objeto_reclamo");
                                    new_objeto.oi_causa = rowObj.GetAttr("causa");
                                    new_objeto.l_inactivo = false; //al ser nuevo se guarda como activo
                                    parte.OBJ_CARPETA.Add(new_objeto);
                                }
                            }
                        }
                    }
                }
            }
            NomadEnvironment.GetTrace().Info("carpeta: " + carpeta.SerializeAll());
            NomadEnvironment.GetCurrentTransaction().SaveRefresh(carpeta);
            NomadEnvironment.GetTrace().Info("::FINALIZA EL METODO SAVE-OBJETOS::");
            return DDO_OBJETOS_REC;
        }

        public static Nomad.NSystem.Proxy.NomadXML SAVE_MONTOS(ref Nomad.NSystem.Proxy.NomadXML DDO_MONTOS, Nomad.NSystem.Proxy.NomadXML XML_TIPO, string strOiCarpeta, Nomad.NSystem.Proxy.NomadXML DDO_PARTES, Nomad.NSystem.Proxy.NomadXML DDO_OBJETOS_REC)
        {
            NomadEnvironment.GetTrace().Info("::INICIA EL METODO SAVE-MONTOS::");
            NucleusRH.Base.Legales.Carpetas.CARPETA carpeta = NucleusRH.Base.Legales.Carpetas.CARPETA.Get(strOiCarpeta);
            NomadLog.Info("::carpeta:: "+carpeta.SerializeAll());
            NomadLog.Info("::DDO_MONTOS:: "+DDO_MONTOS.ToString());        
            Hashtable deletedMonto = new Hashtable();
            Hashtable ObjetosCarpeta = new Hashtable();
            Nomad.NSystem.Proxy.NomadXML rowMontoXML;
            NomadXML etty = Nomad.NSystem.Proxy.NomadProxy.GetProxy().GetEtty();

        //RECORRO LAS PARTES DE LA CARPETA
        foreach(NucleusRH.Base.Legales.Carpetas.PARTE parte in carpeta.PARTES)
        {                                                    
	        NomadLog.Info("::parte:: "+parte.SerializeAll());
	        Nomad.NSystem.Proxy.NomadXML row;
	        //RECORRO LAS PARTES DEL DOCUMENTO XML
          for(row=DDO_PARTES.FirstChild().FirstChild(); row!= null; row=row.Next()) {
  	        NomadLog.Info("::row-parte:: "+row.ToString());
  	        //BUSCO LA PARTE QUE COINCIDE ENTRE EL XML Y LA PARTE DEL DDO
		        if((Convert.ToString(parte.oi_rol) == row.GetAttrString("rol")) && (Convert.ToString(parte.oi_sujeto) == row.GetAttrString("sujeto")))
		        {                  
			        NomadLog.Info("::coincide:: ");
			
			        //RECORRO LOS OBJETOS DE CADA UNA DE LAS PARTES DE LA CARPETA
			        foreach(NucleusRH.Base.Legales.Carpetas.OBJ_REC_CARPETA obj_carpeta in parte.OBJ_CARPETA)
			        { 
				        NomadLog.Info("::obj_carpeta:: "+obj_carpeta.SerializeAll());
				        Nomad.NSystem.Proxy.NomadXML rowMonto;
				        ObjetosCarpeta.Add(obj_carpeta.Id, obj_carpeta);
				        NomadLog.Info("::ObjetosCarpeta:: "+ObjetosCarpeta.ToString());
				        //RECORRO LOS MONTOS A CREAR
				        for(rowMonto=DDO_MONTOS.FirstChild().FirstChild(); rowMonto!= null; rowMonto=rowMonto.Next()) 
	  		             {                   
                         NomadLog.Info("::rowMonto:: "+rowMonto.ToString());
	  			        //BUSCO EL OBJETO CARPETA CORRESPONDIENTE AL MONTO
					        if((rowMonto.GetAttrString("objeto_reclamo") ==  Convert.ToString(obj_carpeta.oi_objeto_reclamo)) && (Convert.ToString(parte.oi_sujeto) == rowMonto.GetAttrString("sujeto")))
					        {                                          
						        NomadLog.Info("::coincide-monto:: ");
						        //BUSCO SI EXISTE EL MONTO.
						        NomadEnvironment.GetTrace().Info("::XML_TIPO::" + XML_TIPO.ToString());
						        string param = "<DATOS oi_tipo_monto=\""+ rowMonto.GetAttr("oi_tipo_monto") +  "\" oi_obj_rec_carpeta=\"" + obj_carpeta.id+ "\" f_monto=\""+rowMonto.GetAttr("f_monto")+"\" />";
						        NomadEnvironment.GetTrace().Info("::PARAM::" + param);
						        NomadXML docIDMonto = NomadEnvironment.QueryNomadXML(NucleusRH.Base.Legales.Carpetas.CARPETA.Resources.getMonto, param).FirstChild();
						        NomadEnvironment.GetTrace().Info("docIDMonto: " + docIDMonto.ToString());
						    
                           
                               NucleusRH.Base.Legales.Montos.MONTO monto;
						    
                                if(docIDMonto.GetAttr("oi_monto") != "0")
						        {
	        		        //recupero el monto
	        		        monto  = NucleusRH.Base.Legales.Montos.MONTO.Get(docIDMonto.GetAttr("oi_monto"));
	        		        if(rowMonto.GetAttrString("estado") == "OLD")
	        			        monto.l_vigente = false;         
	        		        if(rowMonto.GetAttrString("estado") == "EDIT")
	        		        {
	        			            monto.oi_tipo_monto 		 = XML_TIPO.FirstChild().GetAttrString("oi_tipo_monto");
								    monto.oi_moneda   			 = rowMonto.GetAttrString("oi_moneda");
								    //new_monto.oi_obj_rec_carpeta = Convert.ToString(obj_carpeta.id);
								    monto.f_monto 					 = rowMonto.GetAttrDateTime("f_monto");
								    monto.n_monto 					 = rowMonto.GetAttrDouble("n_monto");
								    /* cambio chueka 13-02-13. Guardaba mal el historial del monto.
	        			    NucleusRH.Base.Legales.Montos.HISTORIAL historial = new NucleusRH.Base.Legales.Montos.HISTORIAL();
	        			    historial.oi_monto 		= monto.id;
	        			    historial.f_historial = DateTime.Now;
	        			    NomadXML etty = Nomad.NSystem.Proxy.NomadProxy.GetProxy().GetEtty();
	        			    NomadEnvironment.GetTrace().Info("etty: " + etty.ToString());
	        			    historial.o_historial = "Modificación realizada por "+etty.FirstChild().GetAttr("COD") +" - "+etty.FirstChild().GetAttr("DES")+".";
	        			    NomadEnvironment.GetTrace().Info("historial: " + historial.SerializeAll());
	        			    NomadEnvironment.GetCurrentTransaction().SaveRefresh(historial);
	        			    */
	        			
	        			    NucleusRH.Base.Legales.HistCambios.HIST_CAMBIO histCambio = new NucleusRH.Base.Legales.HistCambios.HIST_CAMBIO();
								    histCambio.oi_carpeta 	= Convert.ToString(carpeta.id);
								    histCambio.f_cambio   	= DateTime.Now;             
								    histCambio.d_usuario  	= etty.FirstChild().GetAttr("COD") +" - "+etty.FirstChild().GetAttr("DES");
						        NomadXML XMLHist = new NomadXML(monto.SerializeAll());
						        XMLHist.FirstChild().SetAttr("ACCION", "EDIT");
						        histCambio.o_xml_cambio = Convert.ToString(XMLHist);
						        NomadEnvironment.GetCurrentTransaction().SaveRefresh(histCambio);
	        			 
	        		    }                                                             
	        		    NomadEnvironment.GetTrace().Info("monto: " + monto.SerializeAll());
							    //NomadEnvironment.GetCurrentTransaction().SaveRefresh(monto); 
						    }                       
						    else
						    {
							    //if(rowMonto.GetAttrString("estado") != "OLD")
							    //{
								    //CREO EL NUEVO MONTO.                      
								   
                                
                                monto = new NucleusRH.Base.Legales.Montos.MONTO();
								  
                                monto.oi_obj_rec_carpeta = Convert.ToString(obj_carpeta.id);
								    monto.l_vigente					 = true;    
								    monto.oi_tipo_monto 		 = XML_TIPO.FirstChild().GetAttrString("oi_tipo_monto");
								    monto.oi_moneda   			 = rowMonto.GetAttrString("oi_moneda");
								    //new_monto.oi_obj_rec_carpeta = Convert.ToString(obj_carpeta.id);
								    monto.f_monto 					 = rowMonto.GetAttrDateTime("f_monto");
								    monto.n_monto 					 = rowMonto.GetAttrDouble("n_monto");
							    //}
							    //else							{
								    //monto  = NucleusRH.Base.Legales.Montos.MONTO.Get();
	        			    //monto.l_vigente = false;	        		}
						    }						
						
						    NucleusRH.Base.Legales.Montos.PREVISION prev;
						    if(docIDMonto.GetAttr("oi_monto") != "0")
						    {
							    prev = (NucleusRH.Base.Legales.Montos.PREVISION)monto.PREVISIONES.GetById(docIDMonto.GetAttr("oi_prevision"));
						    }
						    else
						    {
							    //CREO LA PREVISION.                 
							    prev = new NucleusRH.Base.Legales.Montos.PREVISION();
							    prev.f_prevision 			 = rowMonto.GetAttrDateTime("f_monto"); 	
						    }
						
						    prev.n_porcentaje_prev = rowMonto.GetAttrDouble("n_prevision");
						    prev.n_monto_prev 		 = rowMonto.GetAttrDouble("n_monto_prev");
						    prev.n_porcentaje_cos  = rowMonto.GetAttrDouble("n_prevision_c");
						    prev.n_monto_cos  		 = rowMonto.GetAttrDouble("n_monto_prev_c");
						    if(docIDMonto.GetAttr("oi_monto") == "0")
							    monto.PREVISIONES.Add(prev);
						
						    NomadEnvironment.GetTrace().Info("monto: " + monto.SerializeAll());
						
						
						    NomadEnvironment.GetTrace().Info("etty: " + etty.ToString());
      			    string obs;
						    if(monto.IsForInsert)
							    obs = "Creación realizada por "+etty.FirstChild().GetAttr("COD") +" - "+etty.FirstChild().GetAttr("DES")+".";
						    else
							    obs = "Modificación realizada por "+etty.FirstChild().GetAttr("COD") +" - "+etty.FirstChild().GetAttr("DES")+".";
						
						    NucleusRH.Base.Legales.Montos.HISTORIAL historial = new NucleusRH.Base.Legales.Montos.HISTORIAL();
						    //historial.oi_monto 		= docIDMonto.GetAttrInt("oi_monto");
      			    //monto.id;        
      			    historial.o_historial = obs;
      			    historial.f_historial = DateTime.Now;
      			    NomadEnvironment.GetTrace().Info("historial: " + historial.SerializeAll());
      			    //NomadEnvironment.GetCurrentTransaction().SaveRefresh(historial);
      			    monto.HISTORIAL.Add(historial);
						
						
						   
                            NomadEnvironment.GetCurrentTransaction().SaveRefresh(monto); 
						
					    }
                        
				    }
                    
			    }			
		    }
	    }
    }                 

    ///////DELETE de los montos.
    foreach(OBJ_REC_CARPETA ObjetoCarpeta in ObjetosCarpeta.Values)
    {
      NomadLog.Info("::ObjetoCarpeta:: "+ObjetoCarpeta.SerializeAll());
      //POR CARA OBJETO CARPETA BUSCO LOS MONTOS CREADOS QUE TIENE
      string param = "<DATOS oi_obj_rec_carpeta=\"" + ObjetoCarpeta.id+ "\"/>";
	    NomadEnvironment.GetTrace().Info("::PARAM::" + param);
      NomadXML XMLMontos = NomadEnvironment.QueryNomadXML(NucleusRH.Base.Legales.Carpetas.CARPETA.Resources.getMontos, param).FirstChild();
      NomadEnvironment.GetTrace().Info("::XMLMontos::" + XMLMontos.ToString());
      for(NomadXML XMLMonto=XMLMontos.FirstChild(); XMLMonto!= null; XMLMonto=XMLMonto.Next())
      {
	      int existe = 0;                                                         
	      NomadEnvironment.GetTrace().Info("::XMLMonto::" + XMLMonto.ToString());
	      for(rowMontoXML=DDO_MONTOS.FirstChild().FirstChild(); rowMontoXML!= null; rowMontoXML=rowMontoXML.Next()) 
	      {                                                                      
	  	    NomadEnvironment.GetTrace().Info("::rowMontoXML::" + rowMontoXML.ToString());
	  	    if(rowMontoXML.GetAttrString("id") == XMLMonto.GetAttrString("id"))	
	  		    existe = 1;
	  	    if((rowMontoXML.GetAttrString("objeto_reclamo") == XMLMonto.GetAttrString("oi_objeto_reclamo")) && (rowMontoXML.GetAttrString("sujeto") == XMLMonto.GetAttrString("oi_sujeto")) && (rowMontoXML.GetAttrString("f_monto") == XMLMonto.GetAttrString("f_monto")))	
	  		    existe = 1;
	      }
	      if(existe == 0 && (XMLMonto.GetAttrInt("l_vigente") == 0))
	      {                                                   
			    NomadEnvironment.GetTrace().Info("::DELETE - 1::");
	  	    NucleusRH.Base.Legales.Montos.MONTO monto = NucleusRH.Base.Legales.Montos.MONTO.Get(XMLMonto.GetAttr("id"));
	  	    NomadEnvironment.GetTrace().Info("::monto to delete:: " + monto.SerializeAll());
			
			    NucleusRH.Base.Legales.HistCambios.HIST_CAMBIO histCambio = new NucleusRH.Base.Legales.HistCambios.HIST_CAMBIO();
			    histCambio.oi_carpeta 	= Convert.ToString(carpeta.id);
			    histCambio.f_cambio   	= DateTime.Now;             
			    //NomadXML etty = Nomad.NSystem.Proxy.NomadProxy.GetProxy().GetEtty();
	        histCambio.d_usuario  	= etty.FirstChild().GetAttr("COD") +" - "+etty.FirstChild().GetAttr("DES");
	        NomadXML XMLHist = new NomadXML(monto.SerializeAll());
	        XMLHist.FirstChild().SetAttr("ACCION", "DELETE");
	        histCambio.o_xml_cambio = Convert.ToString(XMLHist);
	    
	        NomadEnvironment.GetTrace().Info("::histCambio:: " + histCambio.SerializeAll());
	    
	        NomadEnvironment.GetCurrentTransaction().SaveRefresh(histCambio);     			
			    NomadEnvironment.GetCurrentTransaction().Delete(monto);   
			    NomadEnvironment.GetTrace().Info("::DELETE - 2::");
	      }
      } 
    } 


    NomadEnvironment.GetTrace().Info("::FINALIZA EL METODO SAVE-MONTOS::");
    return DDO_MONTOS;
            }

        public static Nomad.NSystem.Proxy.NomadXML SAVE_TAREAS(ref Nomad.NSystem.Proxy.NomadXML DDO_TAREAS, string strOiCarpeta)
        {
            NomadEnvironment.GetTrace().Info("::INICIA EL METODO SAVE-TAREAS::");
            Nomad.NSystem.Proxy.NomadXML row;

            for (row = DDO_TAREAS.FirstChild().FirstChild(); row != null; row = row.Next())
            {
                //BUSCO SI LA TAREA YA FUE CREADA.

                string param = "<DATOS oi_carpeta=\"" + strOiCarpeta + "\" d_tarea=\"" + row.GetAttr("desc_tarea") + "\"/>";
                NomadEnvironment.GetTrace().Info("::PARAM::" + param);
                NomadXML docIDTarea = NomadEnvironment.QueryNomadXML(NucleusRH.Base.Legales.Carpetas.CARPETA.Resources.getTarea, param).FirstChild();
                NomadEnvironment.GetTrace().Info("::docIDTarea::" + docIDTarea.ToString());
                NucleusRH.Base.Legales.Tareas.TAREA new_tarea;
                //EN CASO DE NO EXISTE LA PARTE, LA CREO.
                if (docIDTarea.GetAttr("oi_tarea") == "0")
                {
                    NomadEnvironment.GetTrace().Info("::1::");
                    new_tarea = new NucleusRH.Base.Legales.Tareas.TAREA();
                }
                else
                {
                    new_tarea = NucleusRH.Base.Legales.Tareas.TAREA.Get(docIDTarea.GetAttr("oi_tarea"));
                }
                NomadEnvironment.GetTrace().Info("::2::");
                new_tarea.oi_carpeta = strOiCarpeta;
                new_tarea.f_tarea = row.GetAttrDateTime("f_alta");
                NomadEnvironment.GetTrace().Info("::3::");
                new_tarea.d_tarea = row.GetAttr("desc_tarea");
                new_tarea.oi_estado_tarea = row.GetAttr("estado");
                new_tarea.oi_tipo_tarea = row.GetAttr("tipo_tarea");
                NomadEnvironment.GetTrace().Info("::4::");
                new_tarea.oi_clasif_sujeto = row.GetAttr("responsable");
                new_tarea.d_lugar_tarea = row.GetAttr("lugar");
                new_tarea.f_vto_tarea = row.GetAttrDateTime("f_vencimiento");
                NomadEnvironment.GetTrace().Info("::5::");
                new_tarea.o_tarea = row.GetAttr("observacion");
                NomadEnvironment.GetCurrentTransaction().SaveRefresh(new_tarea);
                NomadEnvironment.GetTrace().Info("::6::");
            }


            NomadEnvironment.GetTrace().Info("::FINALIZA EL METODO SAVE-TAREAS::");
            return DDO_TAREAS;
        }

        public static Nomad.NSystem.Proxy.NomadXML SAVE_DOCUMENTOS(ref Nomad.NSystem.Proxy.NomadXML DDO_DOCUMENTOS, string strOiCarpeta)
        {
            //<ROW id="1" doc_dig="13" desc_doc="asdfasdfas" tarea="tarea" f_doc="20120521124948" observacion="" />

            NomadEnvironment.GetTrace().Info("::INICIA EL METODO SAVE-DOCUMENTOS::");
            Nomad.NSystem.Proxy.NomadXML row;

            //ELIMINAR DOCUMENTOS DE DB
            string strParam = "<DATA oi_carpeta=\"" + strOiCarpeta + "\"/>";
            NomadXML dbRows = NomadEnvironment.QueryNomadXML(NucleusRH.Base.Legales.Carpetas.CARPETA.Resources.getDocsCarpeta, strParam);

            for (NomadXML rowDB = dbRows.FirstChild().FirstChild(); rowDB != null; rowDB = rowDB.Next())
            {
                //Recorro cada documento de la BD
                bool blEncontro = false;
                for (row = DDO_DOCUMENTOS.FirstChild().FirstChild(); row != null; row = row.Next())
                {
                    //Comparo documentos
                    if (row.GetAttr("id") == rowDB.GetAttr("id"))
                    {
                        blEncontro = true;
                        break;
                    }
                }
                if (!blEncontro)
                {
                    NucleusRH.Base.Legales.Documentos.DOCUMENTO objDocToDel = Documentos.DOCUMENTO.Get(rowDB.GetAttr("id"));
                    NomadEnvironment.GetCurrentTransaction().Delete(objDocToDel);
                }
            }
            
            for (row = DDO_DOCUMENTOS.FirstChild().FirstChild(); row != null; row = row.Next())
            {
                //BUSCO SI EL DOCUMENTO YA FUE CREADO.

                string param = "<DATOS oi_carpeta=\"" + strOiCarpeta + "\" oi_doc_digital=\"" + row.GetAttr("doc_dig") + "\"/>";
                NomadEnvironment.GetTrace().Info("::PARAM::" + param);
                NomadXML docIDDocumento = NomadEnvironment.QueryNomadXML(NucleusRH.Base.Legales.Carpetas.CARPETA.Resources.getDocumento, param).FirstChild();
                NomadEnvironment.GetTrace().Info("::docIDDocumento::" + docIDDocumento.ToString());

                NucleusRH.Base.Legales.Documentos.DOCUMENTO new_doc;
                //EN CASO DE NO EXISTE EL DOCUMENTO, LO CREO.
                if (docIDDocumento.GetAttr("oi_doc") == "0")
                {
                    NomadEnvironment.GetTrace().Info("::1::");
                    new_doc = new NucleusRH.Base.Legales.Documentos.DOCUMENTO();
                    new_doc.oi_carpeta = strOiCarpeta;
                    new_doc.oi_doc_digital = row.GetAttr("doc_dig");
                }
                else
                {
                    NomadEnvironment.GetTrace().Info("::2::");
                    new_doc = NucleusRH.Base.Legales.Documentos.DOCUMENTO.Get(docIDDocumento.GetAttr("oi_doc"));
                    NomadEnvironment.GetTrace().Info("::3::");
                }

                if (row.GetAttr("tarea") != "")
                {
                    param = "<DATOS oi_carpeta=\"" + strOiCarpeta + "\" d_tarea=\"" + row.GetAttr("tarea") + "\"/>";
                    NomadEnvironment.GetTrace().Info("::PARAM-tarea::" + param);
                    NomadXML docIDTarea = NomadEnvironment.QueryNomadXML(NucleusRH.Base.Legales.Carpetas.CARPETA.Resources.getTareaByDesc, param).FirstChild();
                    NomadEnvironment.GetTrace().Info("::docIDTarea::" + docIDTarea.ToString());
                    new_doc.oi_tarea = docIDTarea.GetAttr("oi_tarea");
                }
                new_doc.d_doc_dig = row.GetAttr("desc_doc");
                new_doc.f_doc_dig = row.GetAttrDateTime("f_doc");
                new_doc.o_doc_dig = row.GetAttr("observacion");
                NomadEnvironment.GetCurrentTransaction().SaveRefresh(new_doc);
            }
            
            

            NomadEnvironment.GetTrace().Info("::FINALIZA EL METODO SAVE-DOCUMENTOS::");
            return DDO_DOCUMENTOS;
        }
        
        public static Nomad.NSystem.Proxy.NomadXML GenerateXMLReport(Nomad.NSystem.Proxy.NomadXML DDO_CARPETAS, Nomad.NSystem.Proxy.NomadXML DDO_PARTES, Nomad.NSystem.Proxy.NomadXML DDO_OBJETOS_REC, ref Nomad.NSystem.Proxy.NomadXML DDO_MONTOS, Nomad.NSystem.Proxy.NomadXML XML_TIPO, Nomad.NSystem.Proxy.NomadXML DDO_TAREAS, Nomad.NSystem.Proxy.NomadXML DDO_DOCUMENTOS, Nomad.NSystem.Proxy.NomadXML DDO_ATRIBUTOS, Nomad.NSystem.Proxy.NomadXML DDO_RADICACIONES)
        {
            //CREO EL PARAMETRO DE SALIDA
            Nomad.NSystem.Proxy.NomadXML XML_REPORT = new Nomad.NSystem.Proxy.NomadXML("XML");
        
        
            //CREO EL PARAMETRO DE BANDERA
            NomadXML xmlParams = new NomadXML("DATOS");
            NomadXML row = new NomadXML("DATOS");
           

            NomadEnvironment.GetTrace().Info("DDO_CARPETAS: " + DDO_CARPETAS.ToString());
            NomadEnvironment.GetTrace().Info("DDO_PARTES: " + DDO_PARTES.ToString());
            NomadEnvironment.GetTrace().Info("DDO_OBJETOS_REC: " + DDO_OBJETOS_REC.ToString());
            NomadEnvironment.GetTrace().Info("DDO_MONTOS: " + DDO_MONTOS.ToString());
            NomadEnvironment.GetTrace().Info("XML_TIPO: " + XML_TIPO.ToString());
            NomadEnvironment.GetTrace().Info("DDO_TAREAS: " + DDO_TAREAS.ToString());
            NomadEnvironment.GetTrace().Info("DDO_DOCUMENTOS: " + DDO_DOCUMENTOS.ToString());

           
            
            
            //AGREGO EL TAG CARPETA.-
            XML_REPORT.AddXML(DDO_CARPETAS);
            NomadEnvironment.GetTrace().Info("XML_REPORT-C: " + XML_REPORT.ToString());

          
            

            //AGREGO EL TAG PARTES DENTRO DE LA CARPETA.-
            XML_REPORT.FirstChild().AddXML(DDO_PARTES);
            NomadEnvironment.GetTrace().Info("XML_REPORT-P: " + XML_REPORT.ToString());
            //AGREGO EL TAG OBJETOS DENTRO DE LA CARPETA.-
            XML_REPORT.FirstChild().AddXML(DDO_OBJETOS_REC);
            NomadEnvironment.GetTrace().Info("XML_REPORT-O: " + XML_REPORT.ToString());
            //AGREGO EL TAG MONTOS.-
            Nomad.NSystem.Proxy.NomadXML rowMonto;
            Nomad.NSystem.Proxy.NomadXML rowObj;

            for (rowMonto = DDO_MONTOS.FirstChild().FirstChild(); rowMonto != null; rowMonto = rowMonto.Next())
            {
                int montoDel = 1;
                for (rowObj = DDO_OBJETOS_REC.FirstChild().FirstChild(); rowObj != null; rowObj = rowObj.Next())
                {
                    if ((rowMonto.GetAttr("sujeto") == rowObj.GetAttr("sujeto")) && (rowMonto.GetAttr("objeto_reclamo") == rowObj.GetAttr("objeto_reclamo")))
                    {
                        montoDel = 0;
                    }
                }
                if (montoDel == 1)
                    DDO_MONTOS.FirstChild().DeleteChild(rowMonto);
                if (rowMonto.GetAttrBool("l_vigente") != false)
                    rowMonto.SetAttr("l_vigente", true);
            }
            XML_REPORT.AddXML(DDO_MONTOS);
            NomadEnvironment.GetTrace().Info("XML_REPORT-M: " + XML_REPORT.ToString());


            //for(row=XML_REPORT.FindElement("OBJETOS").FirstChild(); row!= null; row=row.Next()) 
            //{                                      
            //row.SetAttr("oi_tipo_monto", XML_TIPO.FirstChild().GetAttr("oi_tipo_monto"));
            //}                                                                           
            NomadEnvironment.GetTrace().Info("XML_REPORT-M2: " + XML_REPORT.ToString());
            //AGREGO EL TAG TAREAS.-
            XML_REPORT.AddXML(DDO_TAREAS);
            NomadEnvironment.GetTrace().Info("XML_REPORT-T: " + XML_REPORT.ToString());
            //AGREGO EL TAG DOCUMENTOS.-
            XML_REPORT.AddXML(DDO_DOCUMENTOS);
            NomadEnvironment.GetTrace().Info("XML_REPORT-D: " + XML_REPORT.ToString());

            //AGREGO EL TAG ATRIBUTOS PROCESALES.-
            XML_REPORT.AddXML(DDO_ATRIBUTOS);
            NomadEnvironment.GetTrace().Info("XML_REPORT-AP: " + XML_REPORT.ToString());
            //AGREGO EL TAG RADICACION DEL TRAMITE.-
            XML_REPORT.AddXML(DDO_RADICACIONES);
            NomadEnvironment.GetTrace().Info("XML_REPORT-RT: " + XML_REPORT.ToString());

            //AGREGO EL TAG ADELANTOS  
            if (DDO_CARPETAS.FirstChild().GetAttr("oi_carpeta") != "")
            {
                NomadXML xmlParam = new NomadXML("DATOS");
                xmlParam.SetAttr("oi_carpeta", DDO_CARPETAS.FirstChild().GetAttr("oi_carpeta"));
                //NomadXML xmlQuery = Nomad.NSystem.Base.NomadEnvironment.QueryNomadXML(NucleusRH.Base.Legales.Carpetas.CARPETA.Resources.getAdelRend, xmlParam.ToString());
                NomadXML DDO_ADELANTOS = Nomad.NSystem.Base.NomadEnvironment.QueryNomadXML(NucleusRH.Base.Legales.Carpetas.CARPETA.Resources.getAdelRend, xmlParam.ToString());

                //NomadXML DDO_ADELANTOS = xmlQuery.FirstChild();

                //A LA BANDERA LE INDICO SI EXISTE UN ADELANTO
                xmlParams.SetAttr("l_adelanto", (DDO_ADELANTOS.FirstChild().ChildLength > 0));

                //LE ASIGNO UNA BANDERA DE RENDICIONES A CADA ADELANTO
                for (row = DDO_ADELANTOS.FirstChild().FirstChild(); row != null; row = row.Next())
                {
                    if (row.FirstChild().FirstChild() != null)
                    {
                        row.SetAttr("l_rendicion", 1);
                    }
                    else
                    {
                        row.SetAttr("l_rendicion", 0);
                    }
                }

                //AGREGO LOS ADELANTOS Y RENDICIONES DENTRO DEL TAG XML.-
                NomadEnvironment.GetTrace().Info("DDO_ADELANTOS: " + DDO_ADELANTOS.ToString());
                XML_REPORT.AddXML(DDO_ADELANTOS);
                NomadEnvironment.GetTrace().Info("XML_REPORT-A: " + XML_REPORT.ToString());
            }

            //AGREGO LA BANDERA
            NomadEnvironment.GetTrace().Info("xmlParams: " + xmlParams.ToString());
            XML_REPORT.AddXML(xmlParams);
            NomadEnvironment.GetTrace().Info("XML_REPORT-BAND: " + XML_REPORT.ToString());
            

            return XML_REPORT;
        }
        
        public static Nomad.NSystem.Proxy.NomadXML AddChilds(ref Nomad.NSystem.Proxy.NomadXML DDO_MONTOS, Nomad.NSystem.Proxy.NomadXML DocGrilla, Nomad.NSystem.Proxy.NomadXML MONTO_SAVE, Nomad.NSystem.Proxy.NomadXML XML_TIPO)
        {
            NomadEnvironment.GetTrace().Info("DDO_MONTOS: " + DDO_MONTOS.ToString());
            NomadEnvironment.GetTrace().Info("DocGrilla: " + DocGrilla.ToString());
            NomadEnvironment.GetTrace().Info("MONTO_SAVE: " + MONTO_SAVE.ToString());


           
            
            int cantRowsGrid = DocGrilla.FirstChild().ChildLength;
            NomadEnvironment.GetTrace().Info("cantRowsGrid: " + cantRowsGrid.ToString());
            for (NomadXML rowMonto = DDO_MONTOS.FirstChild().FirstChild(); rowMonto != null; rowMonto = rowMonto.Next())
            {
                for (NomadXML rowObj = DocGrilla.FirstChild().FirstChild(); rowObj != null; rowObj = rowObj.Next())
                {
                    if (MONTO_SAVE.FirstChild().GetAttr("ACCION") != "DEL")
                    {
                        if ((MONTO_SAVE.FirstChild().GetAttr("ACCION") == "EDIT") && (rowMonto.GetAttr("oi_monto") == rowObj.GetAttr("oi_monto")))
                        {
                            NomadEnvironment.GetTrace().Info("EDIT-MONTO: " + rowMonto.ToString());
                            rowMonto.SetAttr("oi_moneda", rowObj.GetAttr("oi_moneda"));
                            rowMonto.SetAttr("f_monto", rowObj.GetAttr("f_monto"));
                            rowMonto.SetAttr("n_monto", rowObj.GetAttr("n_monto"));
                            rowMonto.SetAttr("n_prevision", rowObj.GetAttr("n_prevision"));
                            rowMonto.SetAttr("n_monto_prev", rowObj.GetAttr("n_monto_prev"));
                            rowMonto.SetAttr("n_prevision_c", rowObj.GetAttr("n_prevision_c"));
                            rowMonto.SetAttr("n_monto_prev_c", rowObj.GetAttr("n_monto_prev_c"));
                            rowMonto.SetAttr("estado", "EDIT");
                            rowObj.SetAttr("estado", "EDIT");
                            rowMonto.SetAttr("oi_tipo_monto", XML_TIPO.FirstChild().GetAttr("oi_tipo_monto"));
                            cantRowsGrid--;
                        }
                        else
                        {
                            if ((rowMonto.GetAttr("objeto_reclamo") == rowObj.GetAttr("objeto_reclamo")) && (rowMonto.GetAttr("sujeto") == rowObj.GetAttr("sujeto")))
                            {
                                NomadEnvironment.GetTrace().Info("CAMBIO-VIGENCIA: " + rowMonto.ToString());
                                rowMonto.SetAttr("estado", "OLD");
                                rowMonto.SetAttr("l_vigente", false);
                            }
                        }
                    }
                }
            }

            if (MONTO_SAVE.FirstChild().GetAttr("ACCION") != "DEL")
            {
                NomadEnvironment.GetTrace().Info("cantRowsGrid-2: " + cantRowsGrid.ToString());
                if (cantRowsGrid != 0)
                {
                    for (NomadXML rowObj = DocGrilla.FirstChild().FirstChild(); rowObj != null; rowObj = rowObj.Next())
                    {
                        if (rowObj.GetAttr("estado") != "EDIT")
                        {
                            NomadEnvironment.GetTrace().Info("FOR-OBJ: " + rowObj.ToString());
                            rowObj.SetAttr("estado", "NEW");
                            rowObj.SetAttr("l_vigente", true);
                            rowObj.SetAttr("oi_tipo_monto", XML_TIPO.FirstChild().GetAttr("oi_tipo_monto"));
                            DDO_MONTOS.FirstChild().AddXML(rowObj);
                            NomadEnvironment.GetTrace().Info("FOR-MONTOS: " + DDO_MONTOS.ToString());
                        }
                    }
                }
            }

            ///////// DELETE DE LOS MONTOS QUE SE BORRARON DE LA GRILLA.-
            if (MONTO_SAVE.FirstChild().GetAttr("ACCION") == "DEL")
            {
                for (NomadXML rowMonto = DDO_MONTOS.FirstChild().FirstChild(); rowMonto != null; rowMonto = rowMonto.Next())
                {
                    int existe = 0;
                    for (NomadXML rowObj = DocGrilla.FirstChild().FirstChild(); rowObj != null; rowObj = rowObj.Next())
                    {
                        if (rowMonto.GetAttr("id") == rowObj.GetAttr("id"))
                            existe = 1;
                    }
                    if ((existe == 0) && rowMonto.GetAttr("l_vigente") == "0")
                    {
                        NomadEnvironment.GetTrace().Info("monto a borrar: " + rowMonto.ToString());
                        DDO_MONTOS.FirstChild().DeleteChild(rowMonto);
                    }
                }
            }
            NomadEnvironment.GetTrace().Info("RETURN: " + DDO_MONTOS.ToString());
            return DDO_MONTOS;
        }
        
        public static Nomad.NSystem.Proxy.NomadXML SAVE_ATRIBUTOS(ref Nomad.NSystem.Proxy.NomadXML DDO_ATRIBUTOS, string strOiCarpeta)
        {
            NomadEnvironment.GetTrace().Info("::INICIA EL METODO SAVE-ATRIBUTOS::");
            Nomad.NSystem.Proxy.NomadXML row;
            NucleusRH.Base.Legales.Carpetas.CARPETA carpeta = NucleusRH.Base.Legales.Carpetas.CARPETA.Get(strOiCarpeta);
            NomadEnvironment.GetTrace().Info("::DDO_ATRIBUTOS::" + DDO_ATRIBUTOS.ToString());
            //NucleusRH.Base.Personal.LegajoEmpresa.PERSONAL_EMP per;
            //per.BENEF_PER.GetById
            for (row = DDO_ATRIBUTOS.FirstChild().FirstChild(); row != null; row = row.Next())
            {
                //BUSCO SI EL ATRIBUTO YA FUE CREADO.
                NomadEnvironment.GetTrace().Info("::row::" + row.ToString());

                string param = "<DATOS oi_carpeta=\"" + strOiCarpeta + "\"/>";
                NomadEnvironment.GetTrace().Info("::PARAM::" + param);
                NomadXML docIDAtributo = NomadEnvironment.QueryNomadXML(NucleusRH.Base.Legales.Carpetas.CARPETA.Resources.getAtributo, param).FirstChild();
                NomadEnvironment.GetTrace().Info("::docIDAtributo::" + docIDAtributo.ToString());
                NucleusRH.Base.Legales.Carpetas.ATRIB_PROC atributo;

                //EN CASO DE NO EXISTE LA PARTE, LA CREO.
                if (docIDAtributo.GetAttr("oi_atributo") == "0")
                {
                    NomadEnvironment.GetTrace().Info("::1::");
                    atributo = new NucleusRH.Base.Legales.Carpetas.ATRIB_PROC();
                }
                else
                {
                    //carpeta.ATRIB_PROC.RemoveById(docIDAtributo.GetAttr("oi_atributo"));
                    atributo = (NucleusRH.Base.Legales.Carpetas.ATRIB_PROC)carpeta.ATRIB_PROC.GetById(docIDAtributo.GetAttr("oi_atributo"));
                    NomadEnvironment.GetTrace().Info("::atributo-recuperado::" + atributo.SerializeAll());
                }
                NomadEnvironment.GetTrace().Info("::2::");
                //atributo.oi_carpeta 		   = Convert.ToInt32(strOiCarpeta);
                atributo.oi_estado_juicio = row.GetAttr("oi_estado");
                //atributo.oi_estado = row.GetAttr("oi_estado");
                NomadEnvironment.GetTrace().Info("::3::");
                atributo.oi_tipo_proc = row.GetAttr("oi_tipo_proc");
                atributo.oi_eva_cont = row.GetAttr("oi_eva_cont");
                atributo.oi_cond_ingreso = row.GetAttr("oi_cond_ingreso");
                NomadEnvironment.GetTrace().Info("::4::");
                atributo.oi_materia = row.GetAttr("oi_materia");
                
                
                //Si la fecha de juicio vino "" la seteo en null
                if (row.GetAttr("f_juicio") != "")
                    atributo.f_juicio = row.GetAttrDateTime("f_juicio");
                else
                    atributo.f_juicioNull = true;

                //Si la fecha de demanda vino "" la seteo en null
                if (row.GetAttr("f_demanda") != "")
                    atributo.f_demanda = row.GetAttrDateTime("f_demanda");
                //else
                //    atributo.f_demandaNull = true;

                //Si la fecha de terminacion vino "" la seteo en null
                if (row.GetAttr("f_terminacion") != "")
                    atributo.f_terminacion = row.GetAttrDateTime("f_terminacion");
                else
                    atributo.f_terminacionNull = true;
              
              
                //atributo.f_demanda = row.GetAttrDateTime("f_demanda");
                //NomadEnvironment.GetTrace().Info("::5::");
                //atributo.f_terminacion = row.GetAttrDateTime("f_terminacion");
                //NomadEnvironment.GetCurrentTransaction().SaveRefresh(atributo);
                NomadEnvironment.GetTrace().Info("::6::");
                NomadEnvironment.GetTrace().Info("::atributo-modificado::" + atributo.SerializeAll());
                if (docIDAtributo.GetAttr("oi_atributo") == "0")
                    carpeta.ATRIB_PROC.Add(atributo);
                NomadEnvironment.GetTrace().Info("::carpeta::" + carpeta.SerializeAll());
            }

            NomadEnvironment.GetCurrentTransaction().SaveRefresh(carpeta);
            NomadEnvironment.GetTrace().Info("::FINALIZA EL METODO SAVE-ATRIBUTOS::");
            return DDO_ATRIBUTOS;
        }
        
        public static Nomad.NSystem.Proxy.NomadXML SAVE_RADICACIONES(Nomad.NSystem.Proxy.NomadXML DDO_RADICACIONES, string strOiCarpeta)
        {
            NomadEnvironment.GetTrace().Info("::INICIA EL METODO SAVE-RADICACIONES::");
            Nomad.NSystem.Proxy.NomadXML row;
            NucleusRH.Base.Legales.Carpetas.CARPETA carpeta = NucleusRH.Base.Legales.Carpetas.CARPETA.Get(strOiCarpeta);
            NomadEnvironment.GetTrace().Info("::DDO_RADICACIONES::" + DDO_RADICACIONES.ToString());
            
            #region Version-Anterior-Metodo
            ////GRABO LAS RADICACIONES
            //for (row = DDO_RADICACIONES.FirstChild().FirstChild(); row != null; row = row.Next())
            //{

            //    NomadLog.Info(row.ToString());
            //    if (row.GetAttr("grabado") != "1")
            //    {
            //        NucleusRH.Base.Legales.Carpetas.RADICACION radicacion = new NucleusRH.Base.Legales.Carpetas.RADICACION();
            //        //}
            //        NomadEnvironment.GetTrace().Info("::2::");
            //        //radicacion.oi_carpeta 		   = strOiCarpeta;
            //        radicacion.f_radicacion = row.GetAttrDateTime("f_radicacion");
            //        NomadEnvironment.GetTrace().Info("::3::");
            //        radicacion.c_expediente = row.GetAttr("c_expediente");
            //        radicacion.oi_secretaria = row.GetAttr("oi_secretaria");                    
            //        radicacion.f_alta = row.GetAttrDateTime("f_alta");
            //        NomadEnvironment.GetTrace().Info("::4::");
            //        radicacion.oi_juzgado = row.GetAttr("oi_juzgado");
            //        radicacion.oi_jurisdiccion = row.GetAttr("oi_jurisdiccion");
            //        radicacion.oi_etapa_judic = row.GetAttr("oi_etapa_judic");
            //        NomadEnvironment.GetTrace().Info("::5::");
            //        radicacion.o_radicacion = row.GetAttr("o_radicacion");
            //        radicacion.l_vigente = row.GetAttrBool("l_vigente");
            //        //NomadEnvironment.GetCurrentTransaction().SaveRefresh(radicacion);
            //        NomadEnvironment.GetTrace().Info("::6::");
            //        carpeta.RADICACION.Add(radicacion);
            //        row.SetAttr("grabado", "1");
            //    }
            //}

            //NomadEnvironment.GetCurrentTransaction().SaveRefresh(carpeta);
            //NomadEnvironment.GetTrace().Info("::FINALIZA EL METODO SAVE-RADICACIONES::");
            //NomadEnvironment.GetTrace().Info("::DDO_RADICACIONES-fin::" + DDO_RADICACIONES.ToString());
            #endregion

            //PRIMERO: BORRO TODAS LAS RADICACIONES DE ESA CARPETA DE LA BD
            carpeta.RADICACION.Clear();
            NomadEnvironment.GetCurrentTransaction().SaveRefresh(carpeta);

            //SEGUNDO: GUARDO LAS NUEVAS RADICACIONES
            for (row = DDO_RADICACIONES.FirstChild().FirstChild(); row != null; row = row.Next())
            {
                NucleusRH.Base.Legales.Carpetas.RADICACION radicacion = new NucleusRH.Base.Legales.Carpetas.RADICACION();               
                
                radicacion.f_radicacion = row.GetAttrDateTime("f_radicacion");                  
                radicacion.c_expediente = row.GetAttr("c_expediente");
                radicacion.oi_secretaria = row.GetAttr("oi_secretaria");
                radicacion.f_alta = row.GetAttrDateTime("f_alta");                 
                radicacion.oi_juzgado = row.GetAttr("oi_juzgado");
                radicacion.oi_jurisdiccion = row.GetAttr("oi_jurisdiccion");
                radicacion.oi_etapa_judic = row.GetAttr("oi_etapa_judic");                
                radicacion.o_radicacion = row.GetAttr("o_radicacion");
                radicacion.l_vigente = row.GetAttrBool("l_vigente");                                            
                
                carpeta.RADICACION.Add(radicacion); 
            }
            //GUARDO LA CARPETA
            NomadEnvironment.GetCurrentTransaction().SaveRefresh(carpeta);


            return DDO_RADICACIONES;
        }
        
        public static void NEW_MONTOS(ref NucleusRH.Base.Legales.Carpetas.CARPETA DDO, Nomad.NSystem.Proxy.NomadXML DocGrilla)
        {
            NomadEnvironment.GetTrace().Info("DDO: " + DDO.SerializeAll());
            NomadEnvironment.GetTrace().Info("DocGrilla: " + DocGrilla.ToString());

            foreach (NucleusRH.Base.Legales.Carpetas.PARTE parte in DDO.PARTES)
            {
                for (NomadXML rowObj = DocGrilla.FirstChild().FirstChild(); rowObj != null; rowObj = rowObj.Next())
                {
                    if (parte.oi_sujeto == rowObj.GetAttr("sujeto"))
                    {
                        foreach (NucleusRH.Base.Legales.Carpetas.OBJ_REC_CARPETA objeto in parte.OBJ_CARPETA)
                        {
                            if (rowObj.GetAttr("estado") == "NEW")
                            {
                                if (objeto.oi_objeto_reclamo == rowObj.GetAttr("objeto_reclamo"))
                                {
                                    //RECUPERO EL MONTO VIGENTE, EL CUAL CAMBIARE SU ESTADO A NO VIGENTE PARA LUEGO CREAR EL NUEVO MONTO.-
                                    string param = "<DATOS oi_obj_rec_carpeta=\"" + objeto.id + "\" />";
                                    NomadEnvironment.GetTrace().Info("::PARAM::" + param);
                                    NomadXML docIDMonto = NomadEnvironment.QueryNomadXML(NucleusRH.Base.Legales.Carpetas.CARPETA.Resources.getMontoVigente, param).FirstChild();
                                    NomadEnvironment.GetTrace().Info("docIDMonto: " + docIDMonto.ToString());
                                    NucleusRH.Base.Legales.Montos.MONTO monto = NucleusRH.Base.Legales.Montos.MONTO.Get(docIDMonto.GetAttr("id"));
                                    monto.l_vigente = false;
                                    NomadEnvironment.GetCurrentTransaction().SaveRefresh(monto);

                                    NucleusRH.Base.Legales.Montos.MONTO montoNew = new NucleusRH.Base.Legales.Montos.MONTO();

                                    montoNew.oi_moneda = rowObj.GetAttr("oi_moneda");
                                    montoNew.f_monto = rowObj.GetAttrDateTime("f_monto");
                                    montoNew.oi_tipo_monto = rowObj.GetAttr("oi_tipo_monto");
                                    montoNew.o_monto = rowObj.GetAttr("o_monto");
                                    montoNew.n_monto = rowObj.GetAttrDouble("n_monto");
                                    montoNew.l_vigente = true;
                                    montoNew.oi_obj_rec_carpeta = Convert.ToString(objeto.id);
                                    if (rowObj.GetAttr("l_facturable") == "1")
                                    {
                                        montoNew.l_facturable = true;
                                        montoNew.n_monto_fact = rowObj.GetAttrDouble("n_monto_fact");
                                    }
                                    NomadEnvironment.GetTrace().Info("montoNew: " + montoNew.SerializeAll());

                                    NucleusRH.Base.Legales.Montos.PREVISION prevNew = new NucleusRH.Base.Legales.Montos.PREVISION();
                                    prevNew.n_porcentaje_prev = rowObj.GetAttrDouble("n_prevision");
                                    prevNew.n_monto_cos = rowObj.GetAttrDouble("n_monto_prev_c");
                                    prevNew.f_prevision = rowObj.GetAttrDateTime("f_monto");
                                    prevNew.n_monto_prev = rowObj.GetAttrDouble("n_monto_prev");
                                    prevNew.n_porcentaje_cos = rowObj.GetAttrDouble("n_prevision_c");
                                    NomadEnvironment.GetTrace().Info("prevNew: " + prevNew.SerializeAll());
                                    montoNew.PREVISIONES.Add(prevNew);
                                    NomadEnvironment.GetTrace().Info("montoFinal: " + montoNew.SerializeAll());
                                    NomadEnvironment.GetCurrentTransaction().SaveRefresh(montoNew);
                                }
                            }
                        }
                    }
                }
            }
        }
        
        public static void UPDATE_MONTOS(ref NucleusRH.Base.Legales.Carpetas.CARPETA DDO, Nomad.NSystem.Proxy.NomadXML DocGrilla)
        {
            NomadEnvironment.GetTrace().Info("DDO: " + DDO.SerializeAll());
            NomadEnvironment.GetTrace().Info("DocGrilla: " + DocGrilla.ToString());

            foreach (NucleusRH.Base.Legales.Carpetas.PARTE parte in DDO.PARTES)
            {
                for (NomadXML rowObj = DocGrilla.FirstChild().FirstChild(); rowObj != null; rowObj = rowObj.Next())
                {
                    if (parte.oi_sujeto == rowObj.GetAttr("sujeto"))
                    {
                        foreach (NucleusRH.Base.Legales.Carpetas.OBJ_REC_CARPETA objeto in parte.OBJ_CARPETA)
                        {
                            //if(rowObj.GetAttr("estado") == "NEW")
                            //{
                            if (objeto.oi_objeto_reclamo == rowObj.GetAttr("objeto_reclamo"))
                            {
                                //RECUPERO EL MONTO VIGENTE, EL CUAL CAMBIARE SU ESTADO A NO VIGENTE PARA LUEGO CREAR EL NUEVO MONTO.-
                                string param = "<DATOS oi_obj_rec_carpeta=\"" + objeto.id + "\" />";
                                NomadEnvironment.GetTrace().Info("::PARAM::" + param);
                                NomadXML docIDMonto = NomadEnvironment.QueryNomadXML(NucleusRH.Base.Legales.Carpetas.CARPETA.Resources.getMontoVigente, param).FirstChild();
                                NomadEnvironment.GetTrace().Info("docIDMonto: " + docIDMonto.ToString());
                                NucleusRH.Base.Legales.Montos.MONTO monto = NucleusRH.Base.Legales.Montos.MONTO.Get(docIDMonto.GetAttr("id"));
                                if (rowObj.GetAttr("l_facturable") == "1")
                                {
                                    monto.l_facturable = true;
                                    monto.n_monto_fact = rowObj.GetAttrDouble("n_monto_fact");
                                }
                                NomadEnvironment.GetTrace().Info("monto: " + monto.SerializeAll());

                                NomadEnvironment.GetCurrentTransaction().SaveRefresh(monto);
                            }
                        }
                        //}		 			
                    }
                }
            }
        }

        /// <summary>
        /// Autor: Luciano Valía
        /// Save de la cabecera de la carpeta en modo EDIT- EN ESTE MOMENTO NO SE ESTA USANDO
        /// </summary>
        /// <param name="DDO_CARPETA"></param>
        public static void SAVE_EDIT(Nomad.NSystem.Proxy.NomadXML DDO_CARPETA)
        {
            NomadLog.Debug("----------------------------------------------------------");
            NomadLog.Debug("------------------SAVE_EDIT V1----------------------------");
            NomadLog.Debug("----------------------------------------------------------");

            NomadLog.Debug("ParametrosEntrada: " + DDO_CARPETA.ToString());

            string strStep = "";
            NomadTransaction objTrans = null;

            try
            {
                //Recupero la carpeta a modificar
                strStep = "RECUPERANDO-CARPETA";
                NucleusRH.Base.Legales.Carpetas.CARPETA objCarpeta =  
                NucleusRH.Base.Legales.Carpetas.CARPETA.Get(DDO_CARPETA.FirstChild().GetAttr("oi_carpeta"));

                //Actualizo al objeto
                objCarpeta.d_carpeta = DDO_CARPETA.FirstChild().GetAttr("d_carpeta");
                objCarpeta.c_criticidad = DDO_CARPETA.FirstChild().GetAttr("c_criticidad");
                objCarpeta.o_carpeta = DDO_CARPETA.FirstChild().GetAttr("o_carpeta");

                //Persisto los cambios
                strStep = "GUARDANDO-CAMBIOS-CARPETA";
                objTrans = new NomadTransaction();
                objTrans.Begin();
                objTrans.Save(objCarpeta);
                objTrans.Commit();
            }
            catch (Exception ex)
            {
                NomadException nmdEx = new NomadException("NucleusRH.Base.Legales.Carpetas.CARPETA.SAVE_EDIT()", ex);
                nmdEx.SetValue("Step", strStep);

                if (objTrans != null)
                {
                    objTrans.Rollback();
                    
                }
                throw nmdEx;
            }


            
        }

        /// <summary>
        /// Autor: Luciano Valía
        /// Actualiza DDO_MONTOS para crear nuevos montos
        /// </summary>
        /// <param name="DDO_MONTOS"></param>
        /// <param name="DocGrilla"></param>
        /// <returns></returns>
        public static Nomad.NSystem.Proxy.NomadXML MONTOS_EDIT(ref Nomad.NSystem.Proxy.NomadXML DDO_MONTOS, Nomad.NSystem.Proxy.NomadXML DocGrilla)
        {

            NomadEnvironment.GetTrace().Info("DDO_MONTOS: " + DDO_MONTOS.ToString());
            NomadEnvironment.GetTrace().Info("DocGrilla: " + DocGrilla.ToString());

            NomadXML xmlDDOMontos;
            //Hashtable htaToDelete = new Hashtable();
            ArrayList arrToDelete = new ArrayList();
            Hashtable htaEncontrados = new Hashtable();

            DocGrilla = DocGrilla.FirstChild();
            xmlDDOMontos = DDO_MONTOS.FirstChild();
            //RECORRO CADA MONTO PARA ACTUALIZAR DDO_MONTO
            for (NomadXML rowMonto = xmlDDOMontos.FirstChild(); rowMonto != null; rowMonto = rowMonto.Next())
            {
                
                //SI es vigente, recorro la grilla
                if (rowMonto.GetAttr("l_vigente")=="1")
                {                  
                    //RECORRO CADA FILA DE LA GRILLA
                    for (NomadXML rowGrid = DocGrilla.FirstChild(); rowGrid != null; rowGrid = rowGrid.Next())
                    {


                        NomadEnvironment.GetTrace().Info("Analizando rowGrid: " + rowGrid.ToString());

                        //Si cambio, actualizo el DDO_MONTO

                        if (rowMonto.GetAttr("id") == rowGrid.GetAttr("id"))
                        {
                            rowMonto.SetAttr("oi_monto", rowGrid.GetAttr("oi_monto"));
                            rowMonto.SetAttr("sujeto", rowGrid.GetAttr("sujeto"));
                            rowMonto.SetAttr("objeto_reclamo", rowGrid.GetAttr("objeto_reclamo"));
                            rowMonto.SetAttr("oi_moneda", rowGrid.GetAttr("oi_moneda"));
                            rowMonto.SetAttr("f_monto", rowGrid.GetAttr("f_monto"));
                            rowMonto.SetAttr("n_monto", rowGrid.GetAttr("n_monto"));
                            rowMonto.SetAttr("n_prevision", rowGrid.GetAttr("n_prevision"));
                            rowMonto.SetAttr("n_monto_prev", rowGrid.GetAttr("n_monto_prev"));
                            rowMonto.SetAttr("n_prevision_c", rowGrid.GetAttr("n_prevision_c"));
                            rowMonto.SetAttr("n_monto_prev_c", rowGrid.GetAttr("n_monto_prev_c"));
                            rowMonto.SetAttr("o_monto", rowGrid.GetAttr("o_monto"));
                            rowMonto.SetAttr("oi_tipo_monto", rowGrid.GetAttr("oi_tipo_monto"));
                            rowMonto.SetAttr("l_vigente", "1");
                         
                            //Indico que pase el monto de la grilla                            
                            rowGrid.SetAttr("pasado", "1");
                            //Agrego el monto como encontrado
                            
                            NomadEnvironment.GetTrace().Info("ADD: " + rowMonto.GetAttr("id") + "  -  " + rowGrid.GetAttr("id"));
                            htaEncontrados.Add(rowMonto.GetAttr("id"), rowMonto);                           
                        }
                    }
                }
                
            }
            
            //ELIMINO LOS MONTOS NO ENCONTRADOS            
            for (NomadXML rowMonto = xmlDDOMontos.FirstChild(); rowMonto != null; rowMonto = rowMonto.Next())
            {
                if (rowMonto.GetAttr("l_vigente") == "0") continue;

                //Pregunta si es nuevo. Si es nuevo lo elimina para que lo agregue desde la grilla el foreach de abajo.
                if (rowMonto.GetAttr("id") == "") {
                    arrToDelete.Add(rowMonto);
                    continue;
                }
                    
                if (!htaEncontrados.ContainsKey(rowMonto.GetAttr("id")))
                    arrToDelete.Add(rowMonto);
            }
            
            //ELIMINO LOS MONTOS DE LA HASH
            for (int intArrPos = 0; intArrPos < arrToDelete.Count; intArrPos++) {
                xmlDDOMontos.DeleteChild((NomadXML) arrToDelete[intArrPos]);
            }
            
            /*foreach (int intArrPos strKey in htaToDelete.Keys)
            {
                xmlDDOMontos.DeleteChild((NomadXML)htaToDelete[strKey]);
            }
            */
            NomadEnvironment.GetTrace().Info("DDO_MONTOS-ANTES NUEVOS: " + DDO_MONTOS.ToString());

            //ACTUALIZO DDO_MONTOS CON LOS NO PASADOS DE LA GRILLA
            for (NomadXML rowGrid = DocGrilla.FirstChild(); rowGrid != null; rowGrid = rowGrid.Next())
            {
                if (rowGrid.GetAttr("pasado") == "")
                {
                    NomadEnvironment.GetTrace().Info("DDO_MONTOS-ANTES foreach: " + rowGrid.ToString());

                    NomadXML xmlMonto;
                    xmlMonto = xmlDDOMontos.AddTailElement("ROW");

                    xmlMonto.SetAttr("oi_monto", "");
                    xmlMonto.SetAttr("sujeto", rowGrid.GetAttr("sujeto"));
                    xmlMonto.SetAttr("objeto_reclamo", rowGrid.GetAttr("objeto_reclamo"));
                    xmlMonto.SetAttr("oi_moneda", rowGrid.GetAttr("oi_moneda"));
                    xmlMonto.SetAttr("f_monto", rowGrid.GetAttr("f_monto"));
                    xmlMonto.SetAttr("n_monto", rowGrid.GetAttr("n_monto"));
                    xmlMonto.SetAttr("n_prevision", rowGrid.GetAttr("n_prevision"));
                    xmlMonto.SetAttr("n_monto_prev", rowGrid.GetAttr("n_monto_prev"));
                    xmlMonto.SetAttr("n_prevision_c", rowGrid.GetAttr("n_prevision_c"));
                    xmlMonto.SetAttr("n_monto_prev_c", rowGrid.GetAttr("n_monto_prev_c"));
                    xmlMonto.SetAttr("o_monto", rowGrid.GetAttr("o_monto"));
                    xmlMonto.SetAttr("oi_tipo_monto", rowGrid.GetAttr("oi_tipo_monto"));
                    xmlMonto.SetAttr("l_vigente", "1");
                }
            }

            NomadEnvironment.GetTrace().Info("DDO_MONTOS-Return: " + DDO_MONTOS.ToString());
            return DDO_MONTOS;          
            

        }

        /// <summary>
        /// Autor: Luciano Valía
        /// Actualiza DDO_MONTOS para eliminar montos no vigentes        
        /// </summary>
        /// <param name="DDO_MONTOS"></param>
        /// <param name="DocGrilla"></param>
        /// <returns></returns>
        public static Nomad.NSystem.Proxy.NomadXML MONTOS_NOVIGENTES_EDIT(ref Nomad.NSystem.Proxy.NomadXML DDO_MONTOS, Nomad.NSystem.Proxy.NomadXML DocGrilla)
        {
            NomadEnvironment.GetTrace().Info("DDO_MONTOS: " + DDO_MONTOS.ToString());
            NomadEnvironment.GetTrace().Info("DocGrilla: " + DocGrilla.ToString());

            Hashtable htaToDelete = new Hashtable();
            NomadXML xmlDDOMontos;

            DocGrilla = DocGrilla.FirstChild();
            xmlDDOMontos = DDO_MONTOS.FirstChild();
            
           //RECORRO CADA MONTO PARA ACTUALIZAR DDO_MONTO
            for (NomadXML rowMonto = xmlDDOMontos.FirstChild(); rowMonto != null; rowMonto = rowMonto.Next())
            {
                bool band = false;
                //SI es No vigente, recorro la grilla
                if (rowMonto.GetAttr("l_vigente") == "0")
                {
                    //RECORRO CADA FILA DE LA GRILLA
                    for (NomadXML rowGrid = DocGrilla.FirstChild(); rowGrid != null; rowGrid = rowGrid.Next())
                    {
                        if (rowMonto.GetAttr("id") == rowGrid.GetAttr("id"))
                        {
                            //Si lo encontro, salgo
                            band = true;
                            break;
                        }                        
                    }
                    if (!(band))
                    {
                        //No lo encontro, agrego el monto a borrar
                        htaToDelete.Add(rowMonto.GetAttr("id"), rowMonto);
                    }                    
                }
            }
            //ELIMINO MONTOS NO VIGENTES
            foreach (string strKey in htaToDelete.Keys)
            {
                xmlDDOMontos.DeleteChild((NomadXML)htaToDelete[strKey]);
            }

            NomadEnvironment.GetTrace().Info("DDO_MONTOS-Return: " + xmlDDOMontos.ToString());
            return DDO_MONTOS;
        }

        /// <summary>
        /// Autor: Luciano Valía
        /// Nueva version que guarda los montos
        /// </summary>
        public static void SAVE_MONTOS_EDIT(ref Nomad.NSystem.Proxy.NomadXML DDO_MONTOS, string pstrOiCarpeta, Nomad.NSystem.Proxy.NomadXML XML_TIPO)
        {
            NomadEnvironment.GetTrace().Info("DDO_MONTOS: " + DDO_MONTOS.ToString());
            NomadEnvironment.GetTrace().Info("pstrOiCarpeta: " + pstrOiCarpeta.ToString());

            NomadXML xmlDDOMontos = DDO_MONTOS.FirstChild();
            NomadEnvironment.GetTrace().Info("xmlDDOMontos: " + xmlDDOMontos.ToString());

            string strStep = "";
            bool commit = false;
            //bool procesar = false; queda comentado por que no se usa

           
           
            NomadTransaction objTran = new NomadTransaction();
            try 
            {
                NomadXML xmlParam = new NomadXML("DATA");
                xmlParam.SetAttr("oi_carpeta", pstrOiCarpeta);

                strStep = "GET-OBJREC";
                NomadXML xmlObjRec = Nomad.NSystem.Base.NomadEnvironment.QueryNomadXML(NucleusRH.Base.Legales.Carpetas.CARPETA.Resources.getObjRecCarpeta, xmlParam.ToString());
                strStep = "GET-MONTOS-DB";                
                NomadXML xmlMontosDB = Nomad.NSystem.Base.NomadEnvironment.QueryNomadXML(NucleusRH.Base.Legales.Carpetas.CARPETA.Resources.getMontosCarpeta,xmlParam.ToString());
               
                //ELIMINO MONTOS NO VIGENTES QUE ESTEN EN BD Y NO EN GRILLA
                objTran.Begin();
                for(NomadXML rowDB = xmlMontosDB.FirstChild().FirstChild(); rowDB != null; rowDB = rowDB.Next())
                {
                    bool encontro = false;
                    if (rowDB.GetAttr("l_vigente") == "0")
                    {
                        for (NomadXML rowMonto = xmlDDOMontos.FirstChild(); rowMonto != null; rowMonto = rowMonto.Next())
                        {
                            if (rowDB.GetAttr("oi_monto") == rowMonto.GetAttr("oi_monto"))
                            {
                                encontro = true;
                                break;
                            }
                        }
                        if (!encontro)
                        {  //No encontro el monto
                            strStep = "GET-MONTO-toDELETE";
                           NucleusRH.Base.Legales.Montos.MONTO objMonto = NucleusRH.Base.Legales.Montos.MONTO.Get(rowDB.GetAttr("oi_monto"));
                           NomadLog.Info("**1 -- ELIMINO MONTOS NO VIGENTES QUE ESTEN EN BD Y NO EN GRILLA**" + rowDB.GetAttr("oi_monto"));
                            objTran.Delete(objMonto);
                            commit = true;
                        }
                    }
                }

           /*     //EVALUO SI HUBO CAMBIOS EN MONTOS VIGENTES esta parte queda comentada ya que se evaluan todos los montos de la grilla
                for (NomadXML rowMonto = xmlDDOMontos.FirstChild(); rowMonto != null; rowMonto = rowMonto.Next())
                {
                    if (rowMonto.GetAttr("l_vigente") == "1")
                    {
                        strStep = "MONTO-NUEVO"; 
                        if (rowMonto.GetAttr("oi_monto") == "")
                        {
                            procesar = true;
                            break;
                        }
                        for (NomadXML rowDB = xmlMontosDB.FirstChild().FirstChild(); rowDB != null; rowDB = rowDB.Next())
                        {
                            strStep = "COMPARANDO-MONTOS";
                            if (rowMonto.GetAttr("oi_monto")==rowDB.GetAttr("oi_monto"))
                            {
                                if ((rowMonto.GetAttr("oi_tipo_monto") != rowDB.GetAttr("oi_tipo_monto")) ||
                                     (rowMonto.GetAttr("oi_moneda") != rowDB.GetAttr("oi_moneda")) ||
                                     (rowMonto.GetAttr("f_monto") != rowDB.GetAttr("f_monto")) ||
                                     (rowMonto.GetAttr("n_monto") != rowDB.GetAttr("n_monto")) ||
                                     (rowMonto.GetAttr("n_monto_prev") != rowDB.GetAttr("n_monto_prev")) ||
                                    (rowMonto.GetAttr("o_monto") != rowDB.GetAttr("o_monto"))
                                   )
                                {
                                    procesar = true;                                    
                                    break;
                                }
                            }
                        }
                        if (procesar) break;
                    }
                    
                 } */


                //PROCESO
             //   if (procesar)
              //  {
                    strStep = "PROCESANDO";
                    
                    //PASO1:Pongo los Montos vigentes actuales como NO VIGENTES  esta parte queda comentada, porque no se modifican más todos los montos
               /*     for (NomadXML rowDB = xmlMontosDB.FirstChild().FirstChild(); rowDB != null; rowDB = rowDB.Next())
                    {
                        if (rowDB.GetAttr("l_vigente") == "1")
                        {
                            strStep = "SET-MONTOS-NV";
                            NucleusRH.Base.Legales.Montos.MONTO objMonto = NucleusRH.Base.Legales.Montos.MONTO.Get(rowDB.GetAttr("oi_monto"));
                            objMonto.l_vigente = false;
                            
                            //Historial del Monto
                            NomadXML etty = Nomad.NSystem.Proxy.NomadProxy.GetProxy().GetEtty();
                            NucleusRH.Base.Legales.Montos.HISTORIAL objHistorial = new Montos.HISTORIAL();
                            objHistorial.o_historial = "SET-MONTOS-NV Edición realizada por: " + etty.FirstChild().GetAttr("COD") + " - " + etty.FirstChild().GetAttr("DES");
                            objHistorial.f_historial = DateTime.Now;
                            objMonto.HISTORIAL.Add(objHistorial);                                 
                            //Guardo el Monto
                            NomadLog.Info("**3 -- save monto:**" + rowDB.GetAttr("oi_monto"));
                            objTran.Save(objMonto);
                        }

                    } */
                    //PASO 2: Creo nuevos montos vigentes para cada monto vigente del DDO_MONTO QUE HAYA CAMBIADO
                    for (NomadXML rowMonto = xmlDDOMontos.FirstChild(); rowMonto != null; rowMonto = rowMonto.Next())
                    {
                                                
                        if (rowMonto.GetAttr("l_vigente") == "1" && rowMonto.GetAttr("oi_monto") != "") //SI ES UN MONTO VIGENTE Y EXISTENTE
                        {
                            for (NomadXML rowDB = xmlMontosDB.FirstChild().FirstChild(); rowDB != null; rowDB = rowDB.Next())
                            {
                                strStep = "COMPARANDO-MONTOS-NUEVO";
                                if (rowMonto.GetAttr("oi_monto") == rowDB.GetAttr("oi_monto")) //CUANDO LO ENCUENTRA EN LA BD
                                {
                                    if ((XML_TIPO.FirstChild().GetAttrString("oi_tipo_monto") != rowDB.GetAttr("oi_tipo_monto")) || 
                                         (rowMonto.GetAttr("oi_moneda") != rowDB.GetAttr("oi_moneda")) ||
                                         (rowMonto.GetAttr("f_monto") != rowDB.GetAttr("f_monto")) ||
                                         (rowMonto.GetAttr("n_monto") != rowDB.GetAttr("n_monto")) ||
                                         (rowMonto.GetAttr("n_monto_prev") != rowDB.GetAttr("n_monto_prev")) ||
                                        (rowMonto.GetAttr("o_monto") != rowDB.GetAttr("o_monto"))
                                       )
                                    { //SI HA CAMBIADO ALGÚN DATO PASO MONTO MODIFICADO A NO VIGENTE
                                        NucleusRH.Base.Legales.Montos.MONTO objMonto = NucleusRH.Base.Legales.Montos.MONTO.Get(rowDB.GetAttr("oi_monto"));
                                        objMonto.l_vigente = false;

                                        //Historial del Monto
                                        NomadXML etty = Nomad.NSystem.Proxy.NomadProxy.GetProxy().GetEtty();
                                        NucleusRH.Base.Legales.Montos.HISTORIAL objHistorial = new Montos.HISTORIAL();
                                        objHistorial.o_historial = "Cambio a estado no vigente. Realizado por: " + etty.FirstChild().GetAttr("COD") + " - " + etty.FirstChild().GetAttr("DES");
                                        objHistorial.f_historial = DateTime.Now;
                                        objMonto.HISTORIAL.Add(objHistorial);
                                        //Guardo el Monto
                                        NomadLog.Info("**2.1.1 -- SI ES UN MONTO VIGENTE Y EXISTENTE Y SI HA CAMBIADO ALGÚN DATO PASO MONTO MODIFICADO A NO VIGENTE**" + rowDB.GetAttr("oi_monto"));
                                        objTran.Save(objMonto);


                                        //CREO NUEVO MONTO VIGENTE
                                        NucleusRH.Base.Legales.Montos.MONTO objMontoNew = new NucleusRH.Base.Legales.Montos.MONTO();
                                        strStep = "CREANDO-MONTOS" + rowMonto.GetAttr("f_monto") + "-" + rowMonto.GetAttr("n_monto");
                                        objMontoNew.f_monto = rowMonto.GetAttrDateTime("f_monto");
                                        objMontoNew.n_monto = rowMonto.GetAttrDouble("n_monto");
                                        //objMontoNew.oi_tipo_monto = rowMonto.GetAttr("oi_tipo_monto");
                                        objMontoNew.oi_tipo_monto = XML_TIPO.FirstChild().GetAttrString("oi_tipo_monto");
                                        objMontoNew.oi_moneda = rowMonto.GetAttr("oi_moneda");
                                        objMontoNew.oi_obj_rec_carpeta = GetObjRecCarpeta(xmlObjRec, rowMonto.GetAttrString("sujeto"), rowMonto.GetAttrString("objeto_reclamo"));
                                        objMontoNew.o_monto = rowMonto.GetAttr("o_monto");
                                        //objMontoNew.oi_obj_rec_carpeta = rowMonto.GetAttr("oi_obj_rec_carpeta");
                                        objMontoNew.l_vigente = true;

                                        //Prevision del monto                       
                                        NucleusRH.Base.Legales.Montos.PREVISION objPrevision = new Montos.PREVISION();
                                        objPrevision.f_prevision = rowMonto.GetAttrDateTime("f_monto");
                                        objPrevision.n_porcentaje_prev = rowMonto.GetAttrDouble("n_prevision");
                                        objPrevision.n_monto_prev = rowMonto.GetAttrDouble("n_monto_prev");
                                        objPrevision.n_porcentaje_cos = rowMonto.GetAttrDouble("n_prevision_c");
                                        objPrevision.n_monto_cos = rowMonto.GetAttrDouble("n_monto_prev_c");
                                        objMontoNew.PREVISIONES.Add(objPrevision);

                                        //Historial del Monto
                                        NomadXML etty2 = Nomad.NSystem.Proxy.NomadProxy.GetProxy().GetEtty();
                                        NucleusRH.Base.Legales.Montos.HISTORIAL objHistorial2 = new Montos.HISTORIAL();
                                        objHistorial2.o_historial = "Actualización. Realizado por: " + etty2.FirstChild().GetAttr("COD") + " - " + etty.FirstChild().GetAttr("DES");
                                        objHistorial2.f_historial = DateTime.Now;
                                        objMontoNew.HISTORIAL.Add(objHistorial2);

                                        //Guardo el Monto
                                        NomadLog.Info("**2.1.2 -- SI ES UN MONTO VIGENTE Y EXISTENTE Y SI HA CAMBIADO ALGÚN DATO CREO NUEVO MONTO VIGENTE**");
                                        objTran.Save(objMontoNew);                                        
                                        
                                    }
                                }
                            }
                        }
                        else 
                        {
                          if(rowMonto.GetAttr("l_vigente") == "1" && rowMonto.GetAttr("oi_monto") == "") //SI ES UN MONTO VIGENTE PERO NUEVO
                          {

                              //CREO NUEVO MONTO VIGENTE
                              NucleusRH.Base.Legales.Montos.MONTO objMontoNew = new NucleusRH.Base.Legales.Montos.MONTO();
                              strStep = "CREANDO-MONTOS" + rowMonto.GetAttr("f_monto") + "-" + rowMonto.GetAttr("n_monto");
                              objMontoNew.f_monto = rowMonto.GetAttrDateTime("f_monto");
                              objMontoNew.n_monto = rowMonto.GetAttrDouble("n_monto");
                              //objMontoNew.oi_tipo_monto = rowMonto.GetAttr("oi_tipo_monto");
                              objMontoNew.oi_tipo_monto = XML_TIPO.FirstChild().GetAttrString("oi_tipo_monto");
                              objMontoNew.oi_moneda = rowMonto.GetAttr("oi_moneda");
                              objMontoNew.oi_obj_rec_carpeta = GetObjRecCarpeta(xmlObjRec, rowMonto.GetAttrString("sujeto"), rowMonto.GetAttrString("objeto_reclamo"));
                              objMontoNew.o_monto = rowMonto.GetAttr("o_monto");
                              //objMontoNew.oi_obj_rec_carpeta = rowMonto.GetAttr("oi_obj_rec_carpeta");
                              objMontoNew.l_vigente = true;

                              //Prevision del monto                       
                              NucleusRH.Base.Legales.Montos.PREVISION objPrevision = new Montos.PREVISION();
                              objPrevision.f_prevision = rowMonto.GetAttrDateTime("f_monto");
                              objPrevision.n_porcentaje_prev = rowMonto.GetAttrDouble("n_prevision");
                              objPrevision.n_monto_prev = rowMonto.GetAttrDouble("n_monto_prev");
                              objPrevision.n_porcentaje_cos = rowMonto.GetAttrDouble("n_prevision_c");
                              objPrevision.n_monto_cos = rowMonto.GetAttrDouble("n_monto_prev_c");
                              objMontoNew.PREVISIONES.Add(objPrevision);

                              //Historial del Monto
                              NomadXML etty2 = Nomad.NSystem.Proxy.NomadProxy.GetProxy().GetEtty();
                              NucleusRH.Base.Legales.Montos.HISTORIAL objHistorial2 = new Montos.HISTORIAL();
                              objHistorial2.o_historial = "Creación. Realizado por: " + etty2.FirstChild().GetAttr("COD") + " - " + etty2.FirstChild().GetAttr("DES");
                              objHistorial2.f_historial = DateTime.Now;
                              objMontoNew.HISTORIAL.Add(objHistorial2);

                              //Guardo el Monto
                              NomadLog.Info("**2.2.1 -- SI ES UN MONTO VIGENTE PERO NUEVO CREO NUEVO MONTO VIGENTE**");
                              objTran.Save(objMontoNew);         
                          
                          }

                          else 
                          {
                              if (rowMonto.GetAttr("l_vigente") != "1") //SI NO ES UN MONTO VIGENTE
                              {
                                 if (rowMonto.GetAttr("or_inactivo") == "1") //si el monto se pasó a no vigente por inactivar un objeto de reclamo, guarda historial
                                  {
                                      strStep = "PASA-MONTO-A-NO-VIGENTE" + rowMonto.GetAttr("f_monto") + "-" + rowMonto.GetAttr("n_monto");
                                      NucleusRH.Base.Legales.Montos.MONTO obj_monto_actualizar = NucleusRH.Base.Legales.Montos.MONTO.Get(rowMonto.GetAttr("id"));//obtengo el monto de la bd
                                      obj_monto_actualizar.l_vigente = false;

                                      //Historial del Monto
                                      NomadXML etty = Nomad.NSystem.Proxy.NomadProxy.GetProxy().GetEtty();
                                      NucleusRH.Base.Legales.Montos.HISTORIAL objHistorial = new Montos.HISTORIAL();
                                      objHistorial.o_historial = "Cambio de estado a no vigente por inactivación de Objeto Reclamo. Ralizado por: " + etty.FirstChild().GetAttr("COD") + " - " + etty.FirstChild().GetAttr("DES");
                                      objHistorial.f_historial = DateTime.Now;
                                      obj_monto_actualizar.HISTORIAL.Add(objHistorial);
                                     //Guardo el Monto
                                      NomadLog.Info("**2.2.2 -- SI NO ES UN MONTO VIGENTE Y PASO A VIGENTE POR INACTIVAR UN OR**");
                                     objTran.Save(obj_monto_actualizar);

                                 }
                              }//FIN MONTO NO VIGENTE
                          }  

                        }//FIN MONTO VIGENTE Y NUEVO    
    
                    }//FIN FOR                                     
                                        
                    commit = true;
             //   }// FIN IF PROCESAR

                if (commit)
                {
                    //Persisto todos los cambios
                    objTran.Commit();
                }
                
                
            }
            catch(Exception ex)
            {
                NomadException nmdEx = NomadException.NewInternalException("NucleusRH.Base.Legales.Carpetas.CARPETA.SAVE_MONTOS_EDIT()", ex);
                nmdEx.SetValue("step", strStep);                
                
                objTran.Rollback();
                
                throw nmdEx;

            }
            
        }
       
        /// <summary>
        /// Devuelve para cada sujeto y objeto reclamo el oi_obj_rec_carpeta
        /// </summary>
        /// <param name="pxmlObjetos"></param>
        /// <param name="pstrOiSujeto"></param>
        /// <param name="pstrOIObjetoReclamo"></param>
        /// <returns></returns>
        private static string GetObjRecCarpeta(NomadXML pxmlObjetos, string pstrOiSujeto, string pstrOIObjetoReclamo) 
        {
            string result ="";
            for (NomadXML rowObj = pxmlObjetos.FirstChild().FirstChild(); rowObj != null; rowObj = rowObj.Next())              
            {                
               if ((rowObj.GetAttr("oi_sujeto") == pstrOiSujeto) && (rowObj.GetAttr("oi_objeto_reclamo") == pstrOIObjetoReclamo))
               {
                   result = rowObj.GetAttrString("oi_obj_rec_carpeta");
                   break;
               }            
            }

            return result;
        }

        /// <summary>
        ///Autor: Luciano Valía
        ///  Da de Baja la Carpeta y registra en el historial de estados de la carpeta que esta dada de baja
        /// </summary>
        /// <param name="pstrOiCarpeta"></param>
        public static void BAJA_CARPETA(string pstrOiCarpeta, string pstrMotivoBaja, string pstrOiCausal)
        {
            NomadEnvironment.GetTrace().Info("------------------------------------------------------------");
            NomadEnvironment.GetTrace().Info("---------------Inicia Metodo BAJA_CARPETA v1----------------");
            NomadEnvironment.GetTrace().Info("------------------------------------------------------------");
            NomadEnvironment.GetTrace().Info("pstrOiCarpeta: " + pstrOiCarpeta.ToString());
            NomadEnvironment.GetTrace().Info("pstrMotivoBaja: " + pstrMotivoBaja.ToString());
            NomadEnvironment.GetTrace().Info("pstrOiCausal: " + pstrOiCausal.ToString());

            string strStep = "";

            NomadTransaction objTrans = null;

            try
            {
                //Recupero la carpeta por oiCarpeta
                strStep = "GET-CARPETA";
                NucleusRH.Base.Legales.Carpetas.CARPETA objCarpeta = new CARPETA();
                objCarpeta = CARPETA.Get(pstrOiCarpeta);

                //Obteniendo nuevo estado Carpeta
                strStep = "GET-ESTADO";
                NomadXML xmlParam = new NomadXML("DATA");
                xmlParam.SetAttr("estado", "Baja");
                NomadXML xmlQry = Nomad.NSystem.Base.NomadEnvironment.QueryNomadXML(NucleusRH.Base.Legales.Carpetas.CARPETA.Resources.getEstado, xmlParam.ToString());                             

                //Actualizo el historico de estados
                strStep = "CREANDO-ESTADO";
                NucleusRH.Base.Legales.Carpetas.HISEST_CARPETA objHistorialEstado = new HISEST_CARPETA();
                objHistorialEstado.oi_carpeta = objCarpeta.id;
                objHistorialEstado.f_hisest_carpeta = DateTime.Now;
                objHistorialEstado.o_hisest_carpeta = pstrMotivoBaja;
                objHistorialEstado.oi_estado_carpeta = xmlQry.FirstChild().GetAttr("oi_estado");
                objHistorialEstado.usuario_cambia = NomadProxy.GetProxy().UserName;                
                objHistorialEstado.oi_est_car_ant = objCarpeta.oi_estado_carpeta;
                objHistorialEstado.oi_causal = pstrOiCausal;

                //Actualizo la carpeta
                strStep = "ACTUALIZANDO-CARPETA";
                objCarpeta.oi_estado_carpeta = xmlQry.FirstChild().GetAttr("oi_estado");
                objCarpeta.HIST_ESTADOS.Add(objHistorialEstado);

                //Persisto Cambios
                strStep = "PERSISTIENDO";
                objTrans = new NomadTransaction();
                objTrans.Begin();
                objTrans.Save(objCarpeta);
                objTrans.Commit();                          
                                
            }
            catch (Exception ex)
            {
                NomadException nmdEx = new NomadException("NucleusRH.Base.Legales.Carpetas.BAJA_CARPETA()", ex);
                nmdEx.SetValue("Step", strStep);

                if (objTrans != null)
                {
                    objTrans.Rollback();

                }
                throw nmdEx;
            }

        
        }

        /// <summary>
        ///Autor: Luciano Valía
        /// Reapertura de la Carpeta y registra en el historial de estados de la carpeta que esta siendo reabierta
        /// </summary>
        /// <param name="pstrOiCarpeta"></param>
        public static void REAPERTURA_CARPETA(string pstrOiCarpeta, string pstrMotivoBaja)
        { 
            NomadEnvironment.GetTrace().Info("------------------------------------------------------------");
            NomadEnvironment.GetTrace().Info("---------------Inicia Metodo REAPERTURA_CARPETA v1----------------");
            NomadEnvironment.GetTrace().Info("------------------------------------------------------------");
            NomadEnvironment.GetTrace().Info("pstrOiCarpeta: " + pstrOiCarpeta.ToString());
            NomadEnvironment.GetTrace().Info("pstrMotivoBaja: " + pstrMotivoBaja.ToString());

            string strStep = "";

            NomadTransaction objTrans = null;

            try
            {
                //Recupero la carpeta por oiCarpeta
                strStep = "GET-CARPETA";
                NucleusRH.Base.Legales.Carpetas.CARPETA objCarpeta = new CARPETA();
                objCarpeta = CARPETA.Get(pstrOiCarpeta);

                //Obteniendo nuevo estado Carpeta
                strStep = "GET-ESTADO";
                NomadXML xmlParam = new NomadXML("DATA");
                xmlParam.SetAttr("estado", "Reapertura");
                NomadXML xmlQry = Nomad.NSystem.Base.NomadEnvironment.QueryNomadXML(NucleusRH.Base.Legales.Carpetas.CARPETA.Resources.getEstado, xmlParam.ToString());                             

                //Actualizo el historico de estados
                strStep = "CREANDO-ESTADO";
                NucleusRH.Base.Legales.Carpetas.HISEST_CARPETA objHistorialEstado = new HISEST_CARPETA();
                objHistorialEstado.oi_carpeta = objCarpeta.id;
                objHistorialEstado.f_hisest_carpeta = DateTime.Now;
                objHistorialEstado.o_hisest_carpeta = pstrMotivoBaja;
                objHistorialEstado.oi_estado_carpeta = xmlQry.FirstChild().GetAttr("oi_estado");
                objHistorialEstado.usuario_cambia = NomadProxy.GetProxy().UserName;                
                objHistorialEstado.oi_est_car_ant = objCarpeta.oi_estado_carpeta;

                //Actualizo la carpeta
                strStep = "ACTUALIZANDO-CARPETA";
                objCarpeta.l_cerrado = false;
                objCarpeta.f_egresoNull = true;
                objCarpeta.HIST_ESTADOS.Add(objHistorialEstado);

                //Persisto Cambios
                strStep = "PERSISTIENDO";
                objTrans = new NomadTransaction();
                objTrans.Begin();
                objTrans.Save(objCarpeta);
                objTrans.Commit();

            }
            catch (Exception ex)
            {
                NomadException nmdEx = new NomadException("NucleusRH.Base.Legales.Carpetas.REAPERTURA_CARPETA()", ex);
                nmdEx.SetValue("Step", strStep);

                if (objTrans != null)
                {
                    objTrans.Rollback();

                }
                throw nmdEx;
            }


        }

        /// <summary>
        /// Autor: Carmen García
        /// Valida antes de cerrar una carpeta: que el estado de carpeta, los objetos de reclamo y el tipo de monto sean válidos para el tipo de carpeta;
        /// que cada objeto de reclamo activo posea un monto vigente, si son requeridos por el tipo de carpeta.
        /// </summary>
        /// <param name="paramDDO"></param>
        public static Nomad.NSystem.Proxy.NomadXML VALIDAR_CIERRE(NucleusRH.Base.Legales.Carpetas.CARPETA paramDDO)
        {

            NomadEnvironment.GetTrace().Info("------------------------------------------------------------");
            NomadEnvironment.GetTrace().Info("---------------Inicia Metodo VALIDAR_CIERRE----------------");
            NomadEnvironment.GetTrace().Info("------------------------------------------------------------");

            NomadEnvironment.GetTrace().Info("DDO: " + paramDDO.SerializeAll());
            Nomad.NSystem.Proxy.NomadXML RESULT_VALIDAR_CIERRE = new Nomad.NSystem.Proxy.NomadXML("ROW");
			
			//Validaciones para Carpetas tipo 2-Acuerdo c/Renuncia
			NucleusRH.Base.Legales.TiposCarpeta.TIPO_CARPETA objTipoCarpeta = NucleusRH.Base.Legales.TiposCarpeta.TIPO_CARPETA.Get(paramDDO.oi_tipo_carpeta);
			if (objTipoCarpeta.c_tipo_carpeta == "2")
			{
				NomadXML xmlParamCar = new NomadXML("DATA");
				xmlParamCar.SetAttr("oi_carpeta", paramDDO.id);
				NomadXML xmlResult = Nomad.NSystem.Base.NomadEnvironment.QueryNomadXML(NucleusRH.Base.Legales.Carpetas.CARPETA.Resources.validarAcuerdoRenuncia, xmlParamCar.ToString());

				if (xmlResult.FirstChild().GetAttr("valido") == "0")
				{					
					RESULT_VALIDAR_CIERRE.SetAttr("Error", xmlResult.FirstChild().GetAttr("error"));
					return RESULT_VALIDAR_CIERRE;
				}
			}
			
            //Validación estado de carpeta según tipo de carpeta
            NomadXML xmlParamEstado = new NomadXML("DATA");
            xmlParamEstado.SetAttr("oi_tipo_carpeta", paramDDO.oi_tipo_carpeta);
            xmlParamEstado.SetAttr("oi_estado", paramDDO.oi_estado_carpeta);
            NomadXML xmlResultEstado = Nomad.NSystem.Base.NomadEnvironment.QueryNomadXML(NucleusRH.Base.Legales.TiposCarpeta.EST_TIPO_CARPETA.Resources.esEstadoValido, xmlParamEstado.ToString());

            if (xmlResultEstado.FirstChild().GetAttr("valido") == "0")
            {
                RESULT_VALIDAR_CIERRE.SetAttr("Error", "No se puede cerrar la carpeta seleccionada ya que el estado de la carpeta no es válido para el tipo de carpeta actual.");
                return RESULT_VALIDAR_CIERRE;
            }

            //Validación objetos de reclamo según tipo de carpeta
            foreach (NucleusRH.Base.Legales.Carpetas.PARTE parte in paramDDO.PARTES)
            {
                foreach (NucleusRH.Base.Legales.Carpetas.OBJ_REC_CARPETA objeto in parte.OBJ_CARPETA)
                {
                    if (!objeto.l_inactivo)
                    {
                        NomadXML xmlParamObjRec = new NomadXML("DATA");
                        xmlParamObjRec.SetAttr("oi_tipo_carpeta", paramDDO.oi_tipo_carpeta);
                        xmlParamObjRec.SetAttr("oi_objeto_reclamo", objeto.oi_objeto_reclamo);
                        NomadXML xmlResultObjeto = Nomad.NSystem.Base.NomadEnvironment.QueryNomadXML(NucleusRH.Base.Legales.TiposCarpeta.OBJ_REC_TIPO_CARPETA.Resources.esReclamoValido, xmlParamObjRec.ToString());

                        if (xmlResultObjeto.FirstChild().GetAttr("valido") == "0")
                        {
                            RESULT_VALIDAR_CIERRE.SetAttr("Error", "No se puede cerrar la carpeta seleccionada ya que existen objetos de reclamo activos no válidos para el tipo de carpeta actual.");
                            return RESULT_VALIDAR_CIERRE;
                        }

                    }
                }
            }

            string tipo_monto_vigente = "";

            //Validación de montos vigentes según objetos de reclamo activos
            NomadXML xmlParam = new NomadXML("DATA");
            xmlParam.SetAttr("oi_tipo_carpeta", paramDDO.oi_tipo_carpeta);
            NomadXML xmlObjAttrReq = Nomad.NSystem.Base.NomadEnvironment.QueryNomadXML(NucleusRH.Base.Legales.TiposCarpeta.TIPO_CARPETA.Resources.GetAttributosRequeridos, xmlParam.ToString());

            if (xmlObjAttrReq.FirstChild().GetAttr("c_req_carga_monto") == "1")
            {
                
                int intCantMontos = 0;
                int intCantObjRec = 0;
                

                //Cuento la cantidad de objetos reclamo activos
                foreach (NucleusRH.Base.Legales.Carpetas.PARTE parte in paramDDO.PARTES)
                {
                    foreach (NucleusRH.Base.Legales.Carpetas.OBJ_REC_CARPETA objeto in parte.OBJ_CARPETA)
                    {
                        if (!objeto.l_inactivo)
                        {
                           intCantObjRec++;

                           //Obtengo los montos de ese objeto activo desde la bd
                           string param_oi_monto = NomadEnvironment.QueryValue("LEG02_MONTOS", "oi_monto", "oi_obj_rec_carpeta", objeto.Id, "LEG02_MONTOS.l_vigente=1", true);
                           string tipo_monto = NomadEnvironment.QueryValue("LEG02_MONTOS","oi_tipo_monto","oi_obj_rec_carpeta", objeto.Id, "LEG02_MONTOS.l_vigente=1", true);

                           if (param_oi_monto != null)
                           {
                               intCantMontos++;
                               tipo_monto_vigente = tipo_monto; //obtengo tipo de monto de un monto vigente (todos los montos vigentes tienen el mismo tipo de monto)
                           }
                        }
                    }
                }
                           
                //Valido que tenga tantos objetos reclamo activos como montos vigentes               
                if (intCantObjRec != intCantMontos)
                {
                    RESULT_VALIDAR_CIERRE.SetAttr("Error", "No se puede cerrar la carpeta seleccionada ya que existen objetos de reclamo activos sin un monto vigente.");
                    return RESULT_VALIDAR_CIERRE;
                }
            }

            //Validación tipo de monto según tipo de carpeta         
            NomadXML xmlParamMonto = new NomadXML("DATA");
            xmlParamMonto.SetAttr("oi_tipo_carpeta", paramDDO.oi_tipo_carpeta);
            xmlParamMonto.SetAttr("oi_tipo_monto", tipo_monto_vigente);

            if (xmlParamMonto.GetAttr("oi_tipo_monto") != "")
            {
                NomadXML xmlResultMonto = Nomad.NSystem.Base.NomadEnvironment.QueryNomadXML(NucleusRH.Base.Legales.TiposCarpeta.TIPO_MONTO_TIPO_CARPETA.Resources.esMontoValido, xmlParamMonto.ToString());

                if (xmlResultMonto.FirstChild().GetAttr("valido") == "0")
                {
                    RESULT_VALIDAR_CIERRE.SetAttr("Error", "No se puede cerrar la carpeta seleccionada ya que el tipo de monto actual no es válido para el tipo de carpeta.");
                    return RESULT_VALIDAR_CIERRE;
                }
               
            }

            return RESULT_VALIDAR_CIERRE;
        }

        /// <summary>
        ///Autor: Luciano Valía
        /// Cierre de la Carpeta y registra en el historial de estados de la carpeta que esta siendo reabierta
        /// </summary>
        /// <param name="pstrOiCarpeta"></param>
        public static void CIERRE_CARPETA(string pstrOiCarpeta, string pstrMotivoBaja)
        {
            NomadEnvironment.GetTrace().Info("------------------------------------------------------------");
            NomadEnvironment.GetTrace().Info("---------------Inicia Metodo CIERRE_CARPETA v1----------------");
            NomadEnvironment.GetTrace().Info("------------------------------------------------------------");
            NomadEnvironment.GetTrace().Info("pstrOiCarpeta: " + pstrOiCarpeta.ToString());
            NomadEnvironment.GetTrace().Info("pstrMotivoBaja: " + pstrMotivoBaja.ToString());

             string strStep = "";

            NomadTransaction objTrans = null;

            try
            {
                //Recupero la carpeta por oiCarpeta
                strStep = "GET-CARPETA";
                NucleusRH.Base.Legales.Carpetas.CARPETA objCarpeta = new CARPETA();
                objCarpeta = CARPETA.Get(pstrOiCarpeta);


                //Obteniendo nuevo estado Carpeta
                strStep = "GET-ESTADO";
                NomadXML xmlParam = new NomadXML("DATA");
                xmlParam.SetAttr("estado", "Cerrado");
                NomadXML xmlQry = Nomad.NSystem.Base.NomadEnvironment.QueryNomadXML(NucleusRH.Base.Legales.Carpetas.CARPETA.Resources.getEstado, xmlParam.ToString());                             


                //Actualizo el historico de estados
                strStep = "CREANDO-ESTADO";
                NucleusRH.Base.Legales.Carpetas.HISEST_CARPETA objHistorialEstado = new HISEST_CARPETA();
                objHistorialEstado.oi_carpeta = objCarpeta.id;
                objHistorialEstado.f_hisest_carpeta = DateTime.Now;
                objHistorialEstado.o_hisest_carpeta = pstrMotivoBaja;
                objHistorialEstado.oi_estado_carpeta = xmlQry.FirstChild().GetAttr("oi_estado");
                objHistorialEstado.usuario_cambia = NomadProxy.GetProxy().UserName;
                objHistorialEstado.oi_est_car_ant = objCarpeta.oi_estado_carpeta;

                //Actualizo la Carpeta
                strStep = "ACTUALIZANDO-CARPETA";
                objCarpeta.l_cerrado = true;
                objCarpeta.f_egreso = DateTime.Now;
                objCarpeta.HIST_ESTADOS.Add(objHistorialEstado);

                //Persisto Cambios
                strStep = "PERSISTIENDO";
                objTrans = new NomadTransaction();
                objTrans.Begin();
                objTrans.Save(objCarpeta);
                objTrans.Commit();

            }
            catch (Exception ex)
            {
                NomadException nmdEx = new NomadException("NucleusRH.Base.Legales.Carpetas.CIERRE_CARPETA()", ex);
                nmdEx.SetValue("Step", strStep);

                if (objTrans != null)
                {
                    objTrans.Rollback();

                }
                throw nmdEx;
            }
            


        }


        /// <summary>
        ///Recibe dos XMLs FILTER e IDsel y combina en FILTER los datos de ambos.
        /// </summary>
        /// <param name="IDS"></param>
        public static void CombinarXmls(ref Nomad.NSystem.Proxy.NomadXML FILTER, Nomad.NSystem.Proxy.NomadXML IDsel)
        {
            NomadLog.Debug("----------------------------------------------------------");
            NomadLog.Debug("------------------COMBINAR_XMLS-----------------");
            NomadLog.Debug("----------------------------------------------------------");

            NomadLog.Debug("ParametroFILTERDATA: " + FILTER.ToString());
            NomadLog.Debug("ParametroIDsel: " + IDsel.ToString());

            string strStep = "";
            try
            {
                strStep = "RECORRO-IDSel";

                //Bandera para evaluar si debo combinar los dos documentos
                bool blMergear = false;

                if (IDsel.isDocument)
                {
                    if (IDsel.FirstChild().ChildLength != 0)
                    {
                        blMergear = true;
                    }
                }
                else
                {
                    if (IDsel.ChildLength != 0)
                    {
                        blMergear = true;
                    }
                }


                if (blMergear)
                {
                    StringBuilder strAttrID = new StringBuilder("IN ( ");



                    for (NomadXML row = IDsel.FirstChild().FirstChild(); row != null; row = row.Next())
                    {
                        string strID = row.GetAttrString("id");

                        strStep = "CONCATENO-IDS-" + strID;

                        strAttrID = strAttrID.Append("\\'" + strID + "\\'" + ",");

                    }

                    strAttrID = strAttrID.Remove(strAttrID.Length - 1, 1);
                    strAttrID = strAttrID.Append(" )");


                    //IDs="IN(\'P\',\'AP\',\'T\')"            
                    string strAttrID2 = strAttrID.ToString();

                    strStep = "SETEO-IDS-FILTER";

                    String nombre_IDs = IDsel.FirstChild().Name + "_IDs";

                    FILTER.FirstChild().SetAttr(nombre_IDs, strAttrID2);
                    string resultado = FILTER.FirstChild().GetAttrString(nombre_IDs);
                }
            }
            catch (Exception ex)
            {
                NomadException nmdEx = new NomadException("NucleusRH.Base.Legales.Carpetas.CARPETA.COMBINAR_XMLS()", ex);
                nmdEx.SetValue("Step", strStep);

                throw nmdEx;
            }

        }


    }

    
    //////////////////////////////////////////////////////////////////////////////////
    //Clase Carpetas
    public partial class PARTE
    {
        /// <summary>
        /// Autor:Luciano Valía
        /// Actualiza el DDO_OBJETO_RECLAMO cuando se elimina una parte, eliminando todos los OR correspondientes a esa parte
        /// </summary>
        /// <param name="DDO_PARTES"></param>
        /// <param name="DDO_OBJETOS_REC"></param>
        /// <returns></returns>
        public static Nomad.NSystem.Proxy.NomadXML DEL_OBJETOS_PARTE( Nomad.NSystem.Proxy.NomadXML DDO_PARTES,ref Nomad.NSystem.Proxy.NomadXML DDO_OBJETOS_REC,ref Nomad.NSystem.Proxy.NomadXML DDO_MONTOS)
        {
            NomadEnvironment.GetTrace().Info("::INICIA DEL_OBJETOS_PARTE v1::");
            NomadEnvironment.GetTrace().Info("DDO_PARTES: " + DDO_PARTES.ToString());
            NomadEnvironment.GetTrace().Info("DDO_OBJETOS_REC: " + DDO_OBJETOS_REC.ToString());
            NomadEnvironment.GetTrace().Info("DDO_MONTOS: " + DDO_MONTOS.ToString());
            string strStep = "";
            try
            {
                //RECORRO LOS OR PORQUE PUEDE HABER PARTES QUE NO TENGAN OR
                for (NomadXML rowOC = DDO_OBJETOS_REC.FirstChild().FirstChild(); rowOC != null; rowOC = rowOC.Next())
                {
                    strStep = "Recorriendo-OR";
                    //RECORRO LAS PARTES
                    for (NomadXML rowParte = DDO_PARTES.FirstChild().FirstChild(); rowParte != null; rowParte = rowParte.Next())
                    {
                        strStep = "Recorriendo-Partes";
                        if ((rowOC.GetAttr("rol") == rowParte.GetAttr("rol")) && (rowOC.GetAttr("sujeto") == rowParte.GetAttr("sujeto")))
                        {
                            strStep = "Dejando-OR";
                            rowOC.SetAttr("Dejar", "1");
                            break;
                        }

                    }
                }
                //LIMPIO EL DDO_OBJETOS_REC
                strStep = "Recorriendo-OR-Limpiar";
                for (NomadXML rowOC = DDO_OBJETOS_REC.FirstChild().FirstChild(); rowOC != null; rowOC = rowOC.Next())
                {
                    if (rowOC.GetAttr("Dejar") != "1")
                    {
                        strStep = "LimpiandoChild";
                        DDO_OBJETOS_REC.FirstChild().DeleteChild(rowOC);
                    }
                }
               //ACTUALIZO EL DDO MONTOS CON EL DDO_OBJETOS_REC ACTUALIZADO
                NucleusRH.Base.Legales.Carpetas.OBJ_REC_CARPETA.DEL_MONTOS_OBJ(DDO_OBJETOS_REC, ref DDO_MONTOS);

            }
            catch (Exception ex)
            {
                NomadException nmdEx = NomadException.NewInternalException("NucleusRH.Base.Legales.Carpetas.PARTE.DEL_OBJETOS_PARTE()", ex);
                nmdEx.SetValue("step", strStep);
                throw nmdEx;

            }
            return DDO_OBJETOS_REC;
        }
    }

    //////////////////////////////////////////////////////////////////////////////////
    //Clase Carpetas
    public partial class OBJ_REC_CARPETA
    {
        /// <summary>
        /// Autor:Luciano Valía
        /// Actualiza el DDO_MONTOS cuando se elimina un objeto reclamo, eliminando todos los Montos correspondientes a ese objeto reclamo
        /// </summary>
        /// <param name="DDO_PARTES"></param>
        /// <param name="DDO_OBJETOS_REC"></param>
        /// <returns></returns>
        public static Nomad.NSystem.Proxy.NomadXML DEL_MONTOS_OBJ(Nomad.NSystem.Proxy.NomadXML DDO_OBJETOS_REC, ref Nomad.NSystem.Proxy.NomadXML DDO_MONTOS)
        {
            NomadEnvironment.GetTrace().Info("::INICIA DEL_MONTOS_OBJ v1::");
            NomadEnvironment.GetTrace().Info("DDO_OR: " + DDO_OBJETOS_REC.ToString());
            NomadEnvironment.GetTrace().Info("DDO_MONTOS: " + DDO_MONTOS.ToString());
            
            string strStep = "";
            try
            { 
            //RECORRO LOS MONTOS PORQUE PUEDE HABER OR QUE NO TENGA MONTO POR EL TIPO DE CARPETA    
                for (NomadXML rowMonto = DDO_MONTOS.FirstChild().FirstChild(); rowMonto != null; rowMonto = rowMonto.Next())
                {
                    strStep = "Recoriendo-Montos";
                    //RECORRO OBJETOS RECLAMO   
                    for (NomadXML rowOR = DDO_OBJETOS_REC.FirstChild().FirstChild(); rowOR != null; rowOR = rowOR.Next())
                    {
                        strStep = "Recorriendo-OR";
                        if ((rowMonto.GetAttr("objeto_reclamo") == rowOR.GetAttr("objeto_reclamo")) && (rowMonto.GetAttr("sujeto") == rowOR.GetAttr("sujeto")))
                        {
                            strStep = "Dejando-Monto";
                            rowMonto.SetAttr("Dejar", "1");
                            break;
                        }
                    }

                }
                //LIMPIO LOS MONTOS QUE NO TIENEN AHORA OBJETO RECLAMO
                for (NomadXML rowMonto = DDO_MONTOS.FirstChild().FirstChild(); rowMonto != null; rowMonto = rowMonto.Next())
                {
                    strStep = "Recorriendo-Montos-Eliminar";
                    if (rowMonto.GetAttr("Dejar") != "1")
                    {
                        strStep = "Eliminando-Monto";
                        DDO_MONTOS.FirstChild().DeleteChild(rowMonto);
                    }
                }
            }
            catch (Exception ex)
            {
                NomadException nmdEx = NomadException.NewInternalException("NucleusRH.Base.Legales.Carpetas.PARTE.DEL_OBJETOS_PARTE()", ex);
                nmdEx.SetValue("step", strStep);
                throw nmdEx;

            }
            return DDO_MONTOS;
        }


        /// <summary>
        /// Autor:Carmen García
        /// Actualiza el DDO_MONTOS cuando se inactiva un objeto reclamo, pasando el monto vigente de ese objeto a no vigente.
        /// </summary>
        /// <param name="DDO_OBJETOS_REC"></param>
        /// <param name="DDO_MONTOS"></param>
        /// <returns></returns>
        public static Nomad.NSystem.Proxy.NomadXML PASAR_MONTO_A_NO_VIGENTE(Nomad.NSystem.Proxy.NomadXML DDO_OBJETOS_REC, ref Nomad.NSystem.Proxy.NomadXML DDO_MONTOS)
        { 
            NomadEnvironment.GetTrace().Info("::INICIA PASAR_MONTO_A_NO_VIGENTE v1::");
            NomadEnvironment.GetTrace().Info("DDO_OR: " + DDO_OBJETOS_REC.ToString());
            NomadEnvironment.GetTrace().Info("DDO_MONTOS: " + DDO_MONTOS.ToString());
            
            string strStep = "";
            NomadTransaction objTran = new NomadTransaction();
            try
            {
                    objTran.Begin();

                    //RECORRO OBJETOS RECLAMO   
                    for (NomadXML rowOR = DDO_OBJETOS_REC.FirstChild().FirstChild(); rowOR != null; rowOR = rowOR.Next())
                    {
                        strStep = "Recorriendo-OR";
                        if (rowOR.GetAttr("l_inactivo") == "1")
                        {
                            strStep = "obtener monto vigente";
                            string id_monto = NomadEnvironment.QueryValue("LEG02_MONTOS", "oi_monto", "oi_obj_rec_carpeta", rowOR.GetAttr("id"), "LEG02_MONTOS.l_vigente=1", true);
                            NucleusRH.Base.Legales.Montos.MONTO obj_monto_actualizar = new NucleusRH.Base.Legales.Montos.MONTO();

                            if (id_monto != null)
                            {
                                obj_monto_actualizar = NucleusRH.Base.Legales.Montos.MONTO.Get(id_monto);//obtengo el monto de la bd
                            
                                for (NomadXML rowMonto = DDO_MONTOS.FirstChild().FirstChild(); rowMonto != null; rowMonto = rowMonto.Next())
                                {
                                    strStep = "Recorriendo-Montos";
                                    if (rowMonto.GetAttr("oi_monto") == obj_monto_actualizar.Id)
                                    {
                                        rowMonto.SetAttr("l_vigente", "0");
                                        rowMonto.SetAttr("or_inactivo","1");//guardo en DDO_MONTOS para saber que se paso a no vigente por inactivar un objeto de reclamo
                                        break;
                                    }
                                }

                                NomadEnvironment.GetTrace().Info("MONTO_CAR de objeto  " + rowOR.GetAttr("id") + ": " + obj_monto_actualizar.ToString());
                            }
                         }
                    }

                    //Persisto todos los cambios
                    objTran.Commit();

            }
            catch (Exception ex)
            {
                NomadException nmdEx = NomadException.NewInternalException("NucleusRH.Base.Legales.Carpetas.PARTE.PASAR_MONTO_A_NO_VIGENTE()", ex);
                nmdEx.SetValue("step", strStep);
                throw nmdEx;

            }
            return DDO_MONTOS;
        
        }

    }        

}

    

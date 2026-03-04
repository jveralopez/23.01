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
using NucleusRH.Base.Configuracion;
using System.Collections.Generic;

namespace NucleusRH.Base.Configuracion.Herramientas
{
    public partial class QUERIES
    {

        public static List<SortedList<string, object>> Guia(string Recurso, string PAR)
        {
            NomadLog.Debug("--------------------------------------");
            NomadLog.Debug("---------------GUIA-----------------");
            NomadLog.Debug("--------------------------------------");

            NomadLog.Debug("LISTA.Param: " + (PAR == null ? "null" : PAR.ToString()));
            NomadLog.Debug("LISTA.Recurso: " + Recurso);

            string type = "";
            List<SortedList<string, object>> retorno = new List<SortedList<string, object>>();
            SortedList<string, object> row;
            SortedList<string, string> types = new SortedList<string,string>();

            int linea;
            NomadXML MyROW;
            NomadXML param = new NomadXML("PARAM");

            //Agrego los parametros
            param.SetAttr("PARENT", PAR);
          
            //Ejecuto el recurso
            NomadXML resultado = NomadProxy.GetProxy().SQLService().GetXML(Recurso, param.ToString());
            NomadLog.Debug("Se encontraron " + (resultado.FirstChild() == null ? "0 registros." : resultado.FirstChild().ChildLength + " registros."));

            if (resultado.FirstChild() != null)
            {
                //Armo una sorted list con los atributos del resultado y sus tipos
                for (int x = 0; x < resultado.Attrs.Count; x++)
                {
                    types.Add(resultado.Attrs[x].ToString(), resultado.GetAttr(resultado.Attrs[x].ToString()));
                }

                //Recorro la salida del recurso
                for (linea = 1, MyROW = resultado.FirstChild(); MyROW != null; linea++, MyROW = MyROW.Next())
                {
                    row = new SortedList<string, object>();
                    //Recorro los atributos y los agrego a la sorted list
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
                    //Agrego el registro a la lista
                    retorno.Add(row);
                }
            }
            NomadLog.Debug("Retorno: " + retorno.ToString());
            return retorno;
        }

        public static List<SortedList<string, object>> Lista(string Recurso, SortedList<string, object> Parametros)
        {
            NomadLog.Debug("--------------------------------------");
            NomadLog.Debug("---------------LISTA-----------------");
            NomadLog.Debug("--------------------------------------");

            NomadLog.Debug("LISTA.Recurso: " + Recurso);
            foreach (KeyValuePair<string, object> kvp in Parametros)
            {
                if (kvp.Value != null) { NomadLog.Debug(kvp.Key.ToString() + ":" + kvp.Value.ToString()); }
                else { NomadLog.Debug(kvp.Key.ToString() + ":Null"); }
            }

            string type = "";
            List<SortedList<string, object>> retorno = new List<SortedList<string, object>>();
            SortedList<string, object> row;
            SortedList<string, string> types = new SortedList<string, string>();

            int linea;
            NomadXML MyROW;
            NomadXML param = new NomadXML("PARAM");

            //Agrego los parametros
            if (Parametros != null)
            {
                foreach (KeyValuePair<string, object> kvp in Parametros)
                {
                    param.SetAttr(kvp.Key, kvp.Value.ToString());
                }
            }

            //Ejecuto el recurso
            NomadXML resultado = NomadProxy.GetProxy().SQLService().GetXML(Recurso, param.ToString());
            NomadLog.Debug("Se encontraron " + (resultado.FirstChild() == null ? "0 registros." : resultado.FirstChild().ChildLength + " registros."));

            if (resultado.FirstChild() != null)
            {
                //Armo una sorted list con los atributos del resultado y sus tipos
                for (int x = 0; x < resultado.Attrs.Count; x++)
                {
                    types.Add(resultado.Attrs[x].ToString(), resultado.GetAttr(resultado.Attrs[x].ToString()));
                }

                //Recorro la salida del recurso
                for (linea = 1, MyROW = resultado.FirstChild(); MyROW != null; linea++, MyROW = MyROW.Next())
                {
                    row = new SortedList<string, object>();
                    //Recorro los atributos y los agrego a la sorted list
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
                    //Agrego el registro a la lista
                    retorno.Add(row);
                }
            }

            NomadLog.Debug("Retorno: " + retorno.ToString());
            return retorno;
        }

        public static SortedList<string, string> CreateRTA(string VAL, string DES) 
        {
            SortedList<string, string> retorno = new SortedList<string, string>();

            //VAL puede tener cuatro valores --> "OK", "ERR", "WRN", "FATALERR"
            retorno.Add("VAL", VAL);
            retorno.Add("DES", DES);

            NomadLog.Debug("------------------------------------");
            NomadLog.Debug("-------------CREATE RTA-------------");            
            NomadLog.Debug("------------------------------------");

            NomadLog.Debug("Codigo de respuesta:" + VAL);
            NomadLog.Debug("Descripcion de respuesta:" + DES);
            return retorno;
        }

        public static SortedList<string, object> Entidad(string Recurso, SortedList<string, object> Parametros) 
        {
            NomadLog.Debug("--------------------------------------");
            NomadLog.Debug("---------------ENTIDAD-----------------");
            NomadLog.Debug("--------------------------------------");

            NomadLog.Debug("ENTIDAD.Recurso: " + Recurso);
            foreach (KeyValuePair<string, object> kvp in Parametros)
            {
                if (kvp.Value != null) { NomadLog.Debug(kvp.Key.ToString() + ":" + kvp.Value.ToString()); }
                else { NomadLog.Debug(kvp.Key.ToString() + ":Null"); }
            }

            string type = "";
            SortedList<string, object> retorno = new SortedList<string, object>();
            SortedList<string, string> types = new SortedList<string, string>();
            
            NomadXML param = new NomadXML("PARAM");

            //Agrego los parametros
            if (Parametros != null)
            {
                foreach (KeyValuePair<string, object> kvp in Parametros)
                {
                    param.SetAttr(kvp.Key, kvp.Value.ToString());
                }
            }

            //Ejecuto el recurso
            NomadXML resultado = NomadProxy.GetProxy().SQLService().GetXML(Recurso, param.ToString());
            NomadLog.Debug((resultado.FirstChild() == null ? "No se encontró la entidad." : "Entidad encontrada."));

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
    }
}

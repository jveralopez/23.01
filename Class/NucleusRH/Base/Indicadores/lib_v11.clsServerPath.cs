using System;
using System.Collections.Generic;
using System.Text;
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

using System.Data;
using System.Data.SqlClient;
using System.Configuration;

namespace NucleusRH.Base.Indicadores.Ejecuciones
{
    class clsServerPath
    {

        public static void GetServerPath(ref string myConnString ,ref string strServerOlap)
        {
            string strMessageMail;

            NomadXML nmdXml;
            string myConn;
            string myResource;
            string mRefreshCube;
            myConnString = "";
            mRefreshCube="";

            myResource = NomadProxy.GetProxy().AppName ;
            NomadBatch.Trace(" 8888 myResource:" + myResource);
            myConn = NomadProxy.GetProxy().Send("CONTEXT", "CONTEXT", "").Message;

            nmdXml = new NomadXML(myConn);
            NomadBatch.Trace("********");
            NomadBatch.Trace("DDO  myConn:" + myConn);
            NomadBatch.Trace("********");
            for (NomadXML nmdEmp = nmdXml.FirstChild().FindElement("apps").FirstChild(); nmdEmp != null; nmdEmp = nmdEmp.Next())
            {
                if (nmdEmp.GetAttr("id") == myResource)
                {
                    myConnString = nmdEmp.GetAttr("olap-id");
                    NomadBatch.Trace("********");
                    NomadBatch.Trace("myResource=" + myResource);
                    NomadBatch.Trace("myConnString=" + myConnString);

                    break;
                }
            }

            for (NomadXML nmdEmp = nmdXml.FirstChild().FindElement("dbs").FirstChild(); nmdEmp != null; nmdEmp = nmdEmp.Next())
            {
                if (nmdEmp.GetAttr("id") == myConnString)
                {

                    NomadBatch.Trace("********");
                    NomadBatch.Trace("Entra en segundo For.. myConnString=" + myConnString);

                    myConnString = "Provider=" + nmdEmp.GetAttr("provider") + ";Data Source=" + nmdEmp.GetAttr("source") + ";User ID=" + nmdEmp.GetAttr("user");
                    myConnString += ";Password=" + nmdEmp.GetAttr("password") + ";Initial Catalog=" + nmdEmp.GetAttr("database");

                    if (nmdEmp.GetAttr("provider")=="MSDAORA.1")
                    {
                    myConnString = "Provider=" + nmdEmp.GetAttr("provider") + ";Data Source=" + nmdEmp.GetAttr("source") + ";User ID=" + nmdEmp.GetAttr("user");
                    myConnString += ";Password=" + nmdEmp.GetAttr("password") ;
                    }

                    NomadBatch.Trace("myConnString=" + myConnString);
                    break;
                }
            }

            NomadBatch.Trace("  myConnString:" + myConnString);

            // ejecuta la actualizacion del cubo en base a la URL almacenada en los recursos
            NomadXML nmdRes = nmdXml.FirstChild().FindElement("olap");
             strServerOlap = nmdRes.GetAttr("server");
             NomadBatch.Trace(" strServerOlap : " + strServerOlap);

        }

    }
}



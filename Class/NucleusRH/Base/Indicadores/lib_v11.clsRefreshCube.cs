using System.Net;
using System.IO;
using System;
using System.Collections.Generic;
using System.Text;

using System.Data;
using System.Data.SqlClient;
using System.Data.OleDb;
using System.Configuration;

using Nomad.NSystem.Document;
using Nomad.NSystem.Proxy;
using Nomad.NSystem.Base;
using Nomad.NSystem.Functions;

namespace NucleusRH.Base.Indicadores.Ejecuciones
{
     class clsRefreshCube
    {// abre la pagina JSP que actualiza el cubo
         public  string  Refresh(string  args)
         {

             NomadBatch.Trace(" clsRefreshCube args: " + args  );
             NomadBatch.Trace(" clsRefreshCube : " + args + "/flushSchemaCache.jsp");
             WebRequest request = WebRequest.Create(args + "/flushSchemaCache.jsp");
             // If required by the server, set the credentials.
             request.Credentials = CredentialCache.DefaultCredentials;
             // Get the response.
             HttpWebResponse response = (HttpWebResponse)request.GetResponse();
             // Display the status.
             //Console.WriteLine(response.StatusDescription);
             // Get the stream containing content returned by the server.
             Stream dataStream = response.GetResponseStream();
             // Open the stream using a StreamReader for easy access.
             StreamReader reader = new StreamReader(dataStream);
             // Read the content.
             string responseFromServer = reader.ReadToEnd();
             // Display the content.
             //Console.WriteLine(responseFromServer);
             // Cleanup the streams and the response.
             reader.Close();
             dataStream.Close();
             response.Close();
             return responseFromServer;

         }
    }

}



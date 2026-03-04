using System;
using System.Xml;
using System.IO;
using System.Collections;

using Nomad.NSystem.Document;
using Nomad.NSystem.Proxy;
using Nomad.NSystem.Base;
using Nomad.NSystem.Functions;

namespace NucleusRH.Base.Configuracion.Archivos
{

    //////////////////////////////////////////////////////////////////////////////////
    //Clase Archivos
    public partial class HEAD 
    {
        public static string Publish(string ID, string outputPath)
        {


            NomadProxy MyPROXY = NomadProxy.GetProxy();

            BINFile MyFILE = MyPROXY.BINService().GetFile("NucleusRH.Base.Configuracion.Archivos.HEAD", ID);
            string[] EXTp = MyFILE.Name.Split('.');
            string EXT = EXTp[EXTp.Length - 1];

            //////////////////////////////////////////////////////
            //Genero la CARPETA
            string FILEPATH = MyPROXY.RunPath + "\\WEB\\" + outputPath.Replace("/", "\\");
            if (!System.IO.Directory.Exists(FILEPATH))
                System.IO.Directory.CreateDirectory(FILEPATH);

            //////////////////////////////////////////////////////
            //EXPORTO el ARCHIVO
            MyFILE.SaveFile(MyPROXY.RunPath + "\\WEB\\" + outputPath.Replace("/", "\\"));

            //////////////////////////////////////////////////////
            //REVUELVO el NOMBRE
            return outputPath.Replace("\\", "/") + "/NucleusRH.Base.Configuracion.Archivos.HEAD." + ID + "." + EXT;

        }

    }
}

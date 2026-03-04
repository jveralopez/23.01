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

namespace NucleusRH.Base.Configuracion.Paginas
{

    //////////////////////////////////////////////////////////////////////////////////
    //Clase Paginas
    public partial class ADJUNTO : Nomad.NSystem.Base.NomadObject
    {
        public static void DeleteAdjunto(string ID)
        {
            //Obtengo los Objetos
            ADJUNTO objADJ = ADJUNTO.Get(ID, true);
            NucleusRH.Base.Configuracion.Archivos.HEAD objIMG = objADJ.Getoi_file();

            //Los Elimino de la BASE de DATOS
            NomadEnvironment.GetCurrentTransaction().Begin();
            NomadEnvironment.GetCurrentTransaction().Delete(objADJ);
            NomadEnvironment.GetCurrentTransaction().Delete(objIMG);
            NomadEnvironment.GetCurrentTransaction().Commit();
        }
    }
}

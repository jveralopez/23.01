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

namespace NucleusRH.Base.Configuracion.PaginasPrincipales
{

    //////////////////////////////////////////////////////////////////////////////////
    //Clase Paginas Principales
    public partial class PAG_RAIZ : Nomad.NSystem.Base.NomadObject
    {
        public static void Save(NucleusRH.Base.Configuracion.PaginasPrincipales.PAG_RAIZ DDO)
        {
            //Genero la Pagina


            //Existe la Carpeta de PORTAL?
            string FILEPATH = NomadProxy.GetProxy().RunPath + "\\WEB\\GenFiles\\" + NomadProxy.GetProxy().AppName + "\\PUBLIC";
            if (!System.IO.Directory.Exists(FILEPATH))
                System.IO.Directory.CreateDirectory(FILEPATH);

            //Genero la Pagina Publicada                         
            if (DDO.d_pag_raiz.Contains(".LANG."))
            {
                for (int t = 0; t < 3; t++)
                {
                    string MyLANG;

                    switch (t)
                    {
                        case 0: MyLANG = "SPA"; break;
                        case 1: MyLANG = "ENG"; break;
                        case 2: MyLANG = "POR"; break;
                        default: MyLANG = "SPA"; break;
                    }

                    StreamWriter swFile = new StreamWriter(FILEPATH + "\\" + (DDO.d_pag_raiz.Replace(".LANG.", "." + MyLANG + ".")), false, System.Text.Encoding.UTF8);
                    swFile.WriteLine(NucleusRH.Base.Configuracion.Paginas.PAGINA.Resources.HTML_REDIRECT.Replace("#URL#", "../PORTAL/PAGE." + DDO.oi_pagina + ".LANG.html"));
                    swFile.Close();
                }
            }
            else
            {
                StreamWriter swFile = new StreamWriter(FILEPATH + "\\" + DDO.d_pag_raiz, false, System.Text.Encoding.UTF8);
                swFile.WriteLine(NucleusRH.Base.Configuracion.Paginas.PAGINA.Resources.HTML_REDIRECT.Replace("#URL#", "../PORTAL/PAGE." + DDO.oi_pagina + ".LANG.html"));
                swFile.Close();
            }

            //Guardo el Objeto
            NomadEnvironment.GetCurrentTransaction().SaveRefresh(DDO);
        }
    }




}


using System;
using System.Collections.Generic;
using System.Text;

using Nomad.NSystem.Proxy;

using Generico.Classes;

namespace Generico {
    class Program {
        static int Main(string[] args) {
            
            #if DEBUG 
                //Solo ejecutar cuando no hay un UUID generado
                GetUUID();
            #endif
            
            NomadProxy objProxy;
            clsArguments objArguments;
            string strFileTemplate;
            string strFileParams;
            int intResult = 0;
                
            try {            
                Console.WriteLine("4:Comienza la interfaz de salida.");
                objArguments = new clsArguments(args);

                //Obtiene el proxy y el archivo de configuración
                objProxy = NomadProxy.UUIDToProxy(objArguments.UUID);
                NomadProxy.SetProxy(objProxy);
                
                try {
                  strFileTemplate = objProxy.FileServiceIO().LoadFile("INTERFACE-DEF", objArguments.DefFileName);
                } catch (Exception ex) {
                    throw new Exception("Se produjo un error al intentar recuperar el archivo de definicion de la interface 'INTERFACE-DEF'. Puede que no tenga formato XML. " + ex.Message);
                }
                try {
                    strFileParams = objProxy.StoreService().GetSessionStore("INTERFACE-GENERICA");
                } catch (Exception ex) {
                    throw new Exception("Se produjo un error al intentar recuperar el archivo de datos cargados para la interface 'INTERFACE-GENERICA'. " + ex.Message);
                }
                    
                switch (objArguments.Class.ToUpper()) {
                    case "GENERICO":
                        clsGenericWriter objGeneric = new clsGenericWriter(strFileTemplate, strFileParams);
                        objGeneric.Save();
                        break;
                }
                

            } catch (Exception ex) {
                Console.WriteLine("2:" + ex.Message);
                intResult = 1;
            }
            
            if (intResult != 1)
                Console.WriteLine("4:El proceso culminó correctamente.");
            else
                Console.WriteLine("4:El proceso culminó con errores.");
            
            return intResult;
        }

        static void GetUUID() {
            string strApp = "Base-DESA";
            NomadProxy objProxy = new NomadProxy();
            
			objProxy.GetContext("<CFG PORT=\"1702\"  IP=\"NucleusNet\" />");
			objProxy.Login(strApp, "matias", "matias", "192.168.1.16");
            
            string strUUID = objProxy.ProxyToUUID();

            
            //Deja un arhivo de prueba Galicia
            //objProxy.StoreService().SetSessionStore("INTERFACE-GENERICA", @"<DATA oi_banco=""8"" oi_sucursal="""" f_acreditacion=""20080121"" interface=""GALICIA"" oi_empresa=""163""><LIQS><LIQ oi_liquidacion=""14""/><LIQ oi_liquidacion=""12""/></LIQS></DATA>"); 
            
            //Deja un arhivo de prueba UOM
            objProxy.StoreService().SetSessionStore("INTERFACE-GENERICA", @"<DATA oi_empresa=""80"" oi_banco=""257"" interface=""UOM"" oi_sindicato=""01"" c_empresa=""TadeoCz"" p_periodo=""200810"" />"); 
            
            //Deja un arhivo de prueba SIJP
            //objProxy.StoreService().SetSessionStore("INTERFACE-GENERICA", @"<DATA interface=""SIJP"" c_empresa=""TadeoCz"" p_periodo=""200801"" />"); 

            strUUID = strUUID;
        }

    }


    /// <summary>
    /// Clase que contiene los argumentos pasados a la aplicación
    /// </summary>
    public class clsArguments {
        private string _Class;
        private string _UUID;
        private string _DefFileName;
        
        public clsArguments(string[] args) {
            if (args.Length < 3)
                throw new Exception("No se encontraron los argumentos de entrada para la interfaz. Se esperaba UUID, Class y DefinitionFileName.");

            this._UUID = args[0];
            this._Class = args[1];
            this._DefFileName = args[2];
        }
        
        public string UUID { 
            get {return this._UUID;}
        }

        public string DefFileName { 
            get {return this._DefFileName;}
        }

        public string Class { 
            get {return this._Class;}
        }
    }

}

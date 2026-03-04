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

namespace NucleusRH.Base.Legales.Sujetos 
{

  //////////////////////////////////////////////////////////////////////////////////
  //Clase Sujetos
    public partial class SUJETO 
    {
        /// <summary>
        /// Validacion de Sujeto en codificadora. Un Sujeto debe tener Clasificacion.
        /// Autor: Luciano Valía
        /// </summary>
        /// <param name="DDO"></param>
        public static void VALIDATOR(NucleusRH.Base.Legales.Sujetos.SUJETO DDO)
        {
            NomadLog.Debug("--------------------------------------------");
            NomadLog.Debug(" Sujetos.VALIDATOR v1"+DateTime.Now.ToString());
            NomadLog.Debug("--------------------------------------------");

            NomadLog.Info("ParametrosLucho: " + DDO.ToString());
            string strStep = "";
            try 
            {
                //int intCant = 0;
                //strStep = "START-RECORRER";

                //foreach (NucleusRH.Base.Legales.Sujetos.CLASIF_SUJETO objClasif in DDO.CLASIF_SUJ)
                //{
                //    NomadLog.Debug("Estatus" + objClasif.Status.ToString());
                //    if (objClasif.Status=="d")
                //    {
                //        strStep = "CUENTO" + objClasif.ToString();
                //        intCant++;
                //    }
                //}
                //NomadLog.Debug("Contados" + intCant.ToString());
                //NomadLog.Debug("En Coleccion" + DDO.CLASIF_SUJ.Count.ToString());
                //if (intCant == DDO.CLASIF_SUJ.Count)
                //{
                //    strStep = "IRREG";
                //    NomadLog.Debug("No tiene Clasificacion" + DDO.ToString());
                //    throw NomadException.NewMessage("Legales.SUJETO.SUJ-ERROR-CLASIF");                 
                //}
                strStep = "VALIDANDO";

                NomadLog.Debug("Cantidad: " + DDO.CLASIF_SUJ.Count.ToString());                
                if (DDO.CLASIF_SUJ.Count == 0)
                {
                    strStep = "DEVOLVIENDO-ERROR";
                    NomadLog.Debug("No tiene Clasificacion" + DDO.ToString());
                    throw NomadException.NewMessage("Legales.SUJETO.SUJ-ERROR-CLASIF"); 
                }
            }
            catch (Exception ex)
            {
                NomadException nmdEx = NomadException.NewInternalException("NucleusRH.Base.Legales.Rendiciones.GuardarRendicion()", ex);
                nmdEx.SetValue("step", strStep);
                 

                throw nmdEx;
            }
        
        }
    } 



}

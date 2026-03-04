using System;
using System.Xml;
using System.IO;
using System.Collections;

using Nomad.NSystem.Document;
using Nomad.NSystem.Proxy;
using Nomad.NSystem.Base;
using Nomad.NSystem.Functions;

namespace NucleusRH.Base.Liquidacion.InterfaceEntradaNov
{

    //////////////////////////////////////////////////////////////////////////////////
    //Clase efinicion de interfaz de entrada de tipo uno
    public partial class ENTRADA
    {
        public static void ImportarVariables(string poi_liquidacion, string pC_Variable)
        {
            try
            {
                NucleusRH.Base.Liquidacion_EntNov.clsEntradaNovedades objNov;
                objNov = new NucleusRH.Base.Liquidacion_EntNov.clsEntradaNovedades();
                objNov.ImportarVariablesPersona(poi_liquidacion, pC_Variable);

            }
            catch (Exception ex)
            {
                throw new NomadAppException("Se produjo un error al ejecutar ImportarVariablesPersona liquidacion " + poi_liquidacion + " - pC_Variable " + pC_Variable + " err: " + ex.Message);
            }
        }
    }
}

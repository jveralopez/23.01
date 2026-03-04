using System;
using System.Xml;
using System.IO;
using System.Collections;

using Nomad.NSystem.Document;
using Nomad.NSystem.Proxy;
using Nomad.NSystem.Base;
using Nomad.NSystem.Functions;

namespace NucleusRH.Base.Liquidacion.IntContableOut
{

    //////////////////////////////////////////////////////////////////////////////////
    //Clase Interface Contable  de Salida
    public partial class IntContableOut
    {
        public static void EjecIntCtbleOut(int par_oi_int_cont, string par_c_Empresa, string par_e_Periodo)
        {
            NucleusRH.Base.Liquidacion.IntCtble_Out.EjecInterfaceContable objInt;
            objInt = new NucleusRH.Base.Liquidacion.IntCtble_Out.EjecInterfaceContable(par_oi_int_cont, par_c_Empresa, par_e_Periodo);
        }
    }
}

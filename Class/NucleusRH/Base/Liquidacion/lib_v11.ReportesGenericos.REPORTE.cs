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

namespace NucleusRH.Base.Liquidacion.ReportesGenericos
{

    //////////////////////////////////////////////////////////////////////////////////
    //Clase Reportes
    public partial class REPORTE
    {
        public void Guardar()
        {
            foreach (NucleusRH.Base.Liquidacion.ReportesGenericos.COLUMNA col in this.COLUMNAS)
            {
                if (col.COL_CON.Count > 40)
                {
                    throw new NomadAppException("Las columnas de conceptos no pueden tener mas de 40 conceptos");
                }
            }
        }
    }
}

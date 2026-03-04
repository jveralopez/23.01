using System;
using System.Xml;
using System.IO;
using System.Collections;

using Nomad.NSystem.Document;
using Nomad.NSystem.Proxy;
using Nomad.NSystem.Base;
using Nomad.NSystem.Functions;

namespace NucleusRH.Base.Organizacion.Convenios
{

    //////////////////////////////////////////////////////////////////////////////////
    //Clase Convenios
    public partial class CONVENIO
    {
        public void Actualizar_Costos_Categorias()
        {
            foreach (NucleusRH.Base.Organizacion.Convenios.CATEGORIA cat in this.CATEGORIAS)
            {
                try
                {
                    cat.Actualizar_Costos();
                }
                catch (Exception) { }
            }
        }
    }
}

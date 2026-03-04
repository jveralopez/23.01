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
using System.Collections.Generic;

namespace NucleusRH.Base.Configuracion.Kubo
{

    public class ListaResultado<T>
    {
        private int m_cantidad;
        private List<T> m_registros = new List<T>();

        public int Cantidad
        {
            get { return m_cantidad; }
            set { m_cantidad = value; }
        }

        public List<T> Registros
        {
            get { return m_registros; }
            set { m_registros = value; }
        }

    }
}

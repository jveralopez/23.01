using System;
using System.Xml;
using System.IO;
using System.Collections;

using Nomad.NSystem.Document;
using Nomad.NSystem.Proxy;
using Nomad.NSystem.Base;
using Nomad.NSystem.Functions;

namespace NucleusRH.Base.Liquidacion.Empresa_Liquidacion
{

    //////////////////////////////////////////////////////////////////////////////////
    //Clase Datos de Liquidación por Empresa
    public partial class EMPRESA
    {
        public static void CargarVariablesFijas(Nomad.NSystem.Document.NmdXmlDocument pobjParametros)
        {
            try
            {
                //Crea el objeto cargador y carga... así nomás.
                NucleusRH.Base.Liquidacion.clsCargaVariables objCarVar = new NucleusRH.Base.Liquidacion.clsCargaVariables();
                objCarVar.CargaFijasEmpresa(pobjParametros.ToString());
            }
            catch (Exception ex)
            {
                throw new NomadAppException("Se produjo un error al cargar las variables. " + ex.Message);
            }
        }
        
        public static void ActualizarParamLibroLey(Nomad.NSystem.Proxy.NomadXML param)
        {
            string oi_empresa = param.FirstChild().GetAttr("oi_empresa");
            string num_desde = param.FirstChild().GetAttr("e_hasta");
            string oi_param = param.FirstChild().GetAttr("oi_parametro");

            // Obtiene el parametro, si no existe lo crea
            NucleusRH.Base.Organizacion.Parametros.PARAMETRO parametro;
            if (oi_param == "")
            {
                parametro = new NucleusRH.Base.Organizacion.Parametros.PARAMETRO();
                parametro.c_modulo = "LIQ";
                parametro.c_parametro = "numDeLibroLey";
                parametro.d_clase = "LibroLey";
                parametro.d_parametro = "Proximo número de pag. para impresión de Libro Ley";
                parametro.d_valor = "1";
                NomadEnvironment.GetCurrentTransaction().SaveRefresh(parametro);
            }
            else
            {
                parametro = NucleusRH.Base.Organizacion.Parametros.PARAMETRO.Get(oi_param);
            }


            // Obtiene el valor del parametro para la empresa
            NucleusRH.Base.Liquidacion.Empresa_Liquidacion.EMPRESA empresa = NucleusRH.Base.Liquidacion.Empresa_Liquidacion.EMPRESA.Get(oi_empresa);
            NucleusRH.Base.Liquidacion.Empresa_Liquidacion.PARAM_EMPRE param_empre = (PARAM_EMPRE)empresa.PARAM_EMP.GetByAttribute("oi_parametro", parametro.Id);

            if (param_empre == null)
            {
                param_empre = new PARAM_EMPRE();
                param_empre.oi_parametro = parametro.Id;
                empresa.PARAM_EMP.Add(param_empre);
            }

            param_empre.d_valor = (int.Parse(num_desde) + 1).ToString();

            NomadEnvironment.GetCurrentTransaction().Save(empresa);
        }

        public static void CrearEmpresaLiq(int oi_empresa)
        {
            NucleusRH.Base.Liquidacion.Empresa_Liquidacion.EMPRESA emp = NucleusRH.Base.Liquidacion.Empresa_Liquidacion.EMPRESA.Get(oi_empresa);
            NomadEnvironment.GetCurrentTransaction().Save(emp);
        }
    }
}

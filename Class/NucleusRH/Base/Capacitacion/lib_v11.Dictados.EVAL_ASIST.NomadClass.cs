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

namespace NucleusRH.Base.Capacitacion.Dictados
{
    public partial class EVAL_ASIST : Nomad.NSystem.Base.NomadObject
    {
        public static void CargaEvaluacion(NomadXML xmlInscriptos)
        {
            NomadBatch batch = NomadBatch.GetBatch("Carga Masiva de Resultado de Evaluaciones", "Carga Masiva de Resultado de Evaluaciones");
            xmlInscriptos = xmlInscriptos.FirstChild();
            Hashtable inscriptosInsertar = new Hashtable();

            batch.SetMess("Analizando inscriptos...");
            int i = 1;
            int errores = 0;
            for (NomadXML xmli = xmlInscriptos.FirstChild(); xmli != null; xmli = xmli.Next())
            {
                batch.SetMess("Analizando inscripto " + i + " de " + xmlInscriptos.ChildLength);
                batch.SetPro(1, 90, xmlInscriptos.ChildLength, i);

                EVAL_ASIST eval = EVAL_ASIST.Get(xmli.GetAttr("oi_eval_asist"));
                eval.f_eval_asist = xmli.GetAttrDateTime("f_eval_asist");
                eval.n_resultado = xmli.GetAttrInt("n_resultado");
                eval.o_eval_asist = xmli.GetAttr("o_eval_asist");
                eval.oi_valor = xmli.FindElement("oi_valor").GetAttr("value");

                GuardarFactores(eval, xmli.FirstChild());

              
                try
                {

                    NomadEnvironment.GetCurrentTransaction().Save(eval);
                }
                catch (NomadException e)
                {
                    batch.Err("Error al guardar un registro " + e.Message);
                    errores++;
                }
            }
          
            batch.Log("Registros guardados: " + (xmlInscriptos.ChildLength - errores));
            batch.Log("Registros con error: " + errores);

            batch.SetMess("Finalizado.");
            batch.SetPro(100);
        }

        private static void GuardarFactores(EVAL_ASIST eval, NomadXML xmlFactores)
        {
            for (NomadXML xmlF = xmlFactores.FirstChild(); xmlF != null; xmlF = xmlF.Next())
            {
                FACT_EV_AS factor = (FACT_EV_AS)eval.FACT_EV_AS.GetById(xmlF.GetAttr("id"));
                if (factor == null)
                {
                    factor = new FACT_EV_AS();
                    eval.FACT_EV_AS.Add(factor);
                }

                factor.n_resultado = xmlF.GetAttrInt("n_resultado");
                factor.o_fact_ev_as = xmlF.GetAttr("o_fact_ev_as");
                factor.oi_valor = xmlF.FindElement("oi_valor").GetAttr("value");
                factor.oi_factor_eval = xmlF.GetAttr("oi_factor_eval");
                
            }
        }
    }
}

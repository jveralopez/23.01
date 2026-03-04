using System;
using System.Xml;
using System.IO;
using System.Collections;

using Nomad.NSystem.Document;
using Nomad.NSystem.Proxy;
using Nomad.NSystem.Base;
using Nomad.NSystem.Functions;

namespace NucleusRH.Base.Liquidacion.GeneracionNum
{

    //////////////////////////////////////////////////////////////////////////////////
    //Clase Generaciones de Numeros de Recibos
    public partial class GENERACION
    {
        public static void GenerarNumero(int idLiquidacion, int NumDesde, int idParametro)
        {
            int numero = NumDesde;
            NomadEnvironment.GetBatch().Trace.Add("ifo", "Comenzó la Generación de Números de Recibos de Sueldo", "Generación de Numeros");
            //Obtiene la liquidacion
            NucleusRH.Base.Liquidacion.LiquidacionDDO.LIQUIDACION_DDO liq = NucleusRH.Base.Liquidacion.LiquidacionDDO.LIQUIDACION_DDO.Get(idLiquidacion);

            if (liq.Recibos.Count > 0)
            {
                try
                {
                    //Comienza la Transaccion
                    NomadEnvironment.GetCurrentTransaction().Begin();
                    //para cada recibo de la liquidacion
                    foreach (NucleusRH.Base.Liquidacion.LiquidacionDDO.RECIBO Recibo in liq.Recibos)
                    {
                        NomadEnvironment.GetBatch().Trace.Add("ifo", "Procesando " + (numero - NumDesde + 1).ToString() + " de " + liq.Recibos.Count, "Generación de Numeros");
                        NomadEnvironment.GetTrace().Info("recibo" + Recibo.Recibo);
                        //Obtengo el recibo
                        NucleusRH.Base.Liquidacion.Recibos.TOT_LIQ_PER rec = NucleusRH.Base.Liquidacion.Recibos.TOT_LIQ_PER.Get(Recibo.Recibo);
                        //Asigno el numero de recibo completando con 8 0s			
                        rec.c_numero = numero.ToString().PadLeft(8, '0');
                        //Incremento en uno el numero de recibo
                        numero = numero + 1;
                        //Guardo
                        NomadEnvironment.GetCurrentTransaction().Save(rec);
                    }
                    NomadEnvironment.GetBatch().Trace.Add("ifo", "Terminó la Generación de Número de Recibos de Sueldo", "Generación de Numeros");
                    //Obtengo el Parametro
                    NucleusRH.Base.Organizacion.Parametros.PARAMETRO param = NucleusRH.Base.Organizacion.Parametros.PARAMETRO.Get(idParametro);
                    //Asigno el valor
                    param.d_valor = numero.ToString();
                    //Guardo
                    NomadEnvironment.GetCurrentTransaction().Save(param);
                    NomadEnvironment.GetBatch().Trace.Add("ifo", "Se Actualizó el parametro Número Desde", "Generación de Numeros");

                    //Obtengo la empresa ddo para poder sacar el nombre
                    NucleusRH.Base.Liquidacion.EmpresasDDO.EMPRESA_DDO emp = NucleusRH.Base.Liquidacion.EmpresasDDO.EMPRESA_DDO.Get(liq.IDEmpresa);

                    //Creo una nueva generacion
                    NucleusRH.Base.Liquidacion.GeneracionNum.GENERACION gen = new NucleusRH.Base.Liquidacion.GeneracionNum.GENERACION();
                    //Asigno los valores
                    gen.f_generacion = DateTime.Now;
                    gen.c_empresa = emp.Codigo;
                    gen.d_empresa = emp.Nombre;
                    gen.c_liquidacion = liq.Codigo;
                    gen.d_liquidacion = liq.Titulo;
                    gen.c_num_desde = NumDesde.ToString();
                    numero = numero - 1;
                    gen.c_num_hasta = numero.ToString();
                    gen.e_ult_ejecucion = liq.id;
                    //Guardo
                    NomadEnvironment.GetCurrentTransaction().Save(gen);
                    NomadEnvironment.GetBatch().Trace.Add("ifo", "Se Guardó la Generación", "Generación de Numeros");
                    NomadEnvironment.GetCurrentTransaction().Commit();
                    NomadEnvironment.GetBatch().Trace.Add("ifo", "La Generación de Números de Recibos de Sueldo Terminó Satisfactoriamente", "Generación de Numeros");
                }
                catch (Exception)
                {
                    NomadEnvironment.GetCurrentTransaction().Rollback();
                    NomadEnvironment.GetBatch().Trace.Add("err", "Hubo un error en la generación de Números", "Generación de Numeros");
                }
            }
            else
            {
                NomadEnvironment.GetBatch().Trace.Add("err", "No se genero ningún Número ya que la liquidación no tiene ningun recibo", "Generación de Numeros");
            }
        }
    }
}

using System;
using System.Xml;
using System.IO;
using System.Collections;

using Nomad.NSystem.Document;
using Nomad.NSystem.Proxy;
using Nomad.NSystem.Base;
using Nomad.NSystem.Functions;

namespace NucleusRH.Base.Liquidacion.EntradaEmbargos
{

    //////////////////////////////////////////////////////////////////////////////////
    //Clase EntradaEmbargos
    public partial class ENTRADAEMBARGOS
    {
        public static void Importar()
        {
            NucleusRH.Base.Liquidacion.EntradaEmbargos.ENTRADAEMBARGOS intEmbargos;

            NomadBatch b = NomadBatch.GetBatch("Importar embargos", "Importar embargos");

            //Cargando el Query.
            string MySTR = NomadProxy.GetProxy().SQLService().Get(ENTRADAEMBARGOS.Resources.qry_lineas, "");
            NomadXML MyXML = new NomadXML();
            NomadXML MyROW;

            //Contando la Cantidad de ROWS
            MyXML.SetText(MySTR);
            int totRegs = MyXML.FirstChild().ChildLength;

            //Recorre los registros
            b.SetMess("Incorporando Embargos...");
            b.Log("Incorporando Embargos...");
            int linea = 0; int regadd = 0; int regerr = 0; string lasssecc = "";
            for (linea = 1, MyROW = MyXML.FirstChild().FirstChild(); MyROW != null; linea++, MyROW = MyROW.Next())
            {
                b.SetPro(0, 90, totRegs, linea);
                b.SetMess("Procesando la Linea " + linea + " de " + totRegs);
                try
                {
                    lasssecc = "leyendo interface";
                    intEmbargos = ENTRADAEMBARGOS.Get(MyROW.GetAttr("id"));

                    //FIJAS
                    lasssecc = "buscando empresa";
                    string empresa = NomadEnvironment.QueryValue("ORG03_EMPRESAS", "oi_empresa", "c_empresa", intEmbargos.empresa, "", true);
                    if (empresa == null || empresa == "")
                        throw new Exception("Empresa no encontrada");

                    lasssecc = "buscando legajo";
                    string oi_personal_emp = NomadEnvironment.QueryValue("PER02_PERSONAL_EMP", "oi_personal_emp", "e_numero_legajo", intEmbargos.legajo.ToString(), "PER02_PERSONAL_EMP.oi_empresa = " + empresa, true);
                    if (oi_personal_emp == null || oi_personal_emp == "")
                        throw new Exception("Legajo no encontrado");

                    lasssecc = "creando embargo";
                    NucleusRH.Base.Liquidacion.Legajo_Liquidacion.EMBARGO ddoEmbargo = new NucleusRH.Base.Liquidacion.Legajo_Liquidacion.EMBARGO();

                    lasssecc = "cargando legajo";
                    NucleusRH.Base.Liquidacion.Legajo_Liquidacion.PERSONAL_EMP legajo = NucleusRH.Base.Liquidacion.Legajo_Liquidacion.PERSONAL_EMP.Get(oi_personal_emp);
                    legajo.EMBARGOS.Add(ddoEmbargo);

                    lasssecc = "asignando valores";
                    ddoEmbargo.e_nro_oficio = intEmbargos.nro_oficio;
                    ddoEmbargo.f_oficio = intEmbargos.fecha_oficio;
                    ddoEmbargo.f_recepcion = intEmbargos.fecha_recepcion;
                    ddoEmbargo.c_tipo_embargo = intEmbargos.tipo_embargo;
                    ddoEmbargo.l_liquida = intEmbargos.automatico;
                    ddoEmbargo.o_caratula = intEmbargos.caratula;
                    ddoEmbargo.d_juzgado = intEmbargos.juzgado;
                    ddoEmbargo.d_juez = intEmbargos.juez;
                    ddoEmbargo.d_secretaria = intEmbargos.secretaria;
                    ddoEmbargo.c_nro_expediente = intEmbargos.nro_expediente;
                    ddoEmbargo.c_nro_cuenta = intEmbargos.cuenta_bancaria;
                    ddoEmbargo.f_inicio = intEmbargos.fecha_inicio;
                    ddoEmbargo.f_inicioNull = intEmbargos.fecha_inicioNull;
                    ddoEmbargo.f_inactivo = intEmbargos.fecha_inactivo;
                    ddoEmbargo.f_inactivoNull = intEmbargos.fecha_inactivoNull;
                    ddoEmbargo.f_finalizado = intEmbargos.fecha_fin;
                    ddoEmbargo.f_finalizadoNull = intEmbargos.fecha_finNull;
                    ddoEmbargo.c_estado = intEmbargos.estado;
                    ddoEmbargo.n_porc_emb = intEmbargos.porcentaje;
                    ddoEmbargo.n_porc_embNull = intEmbargos.porcentajeNull;
                    ddoEmbargo.l_sueldo_bruto = intEmbargos.bruto;
                    ddoEmbargo.l_sueldo_neto = intEmbargos.neto;
                    ddoEmbargo.n_monto_fijo = intEmbargos.monto_fijo;
                    ddoEmbargo.n_monto_fijoNull = intEmbargos.monto_fijoNull;
                    ddoEmbargo.n_monto_total = intEmbargos.monto_total;
                    ddoEmbargo.n_monto_totalNull = intEmbargos.monto_totalNull;
                    ddoEmbargo.n_monto_pend = intEmbargos.monto_pendiente;
                    ddoEmbargo.n_monto_pendNull = intEmbargos.monto_pendienteNull;
                    ddoEmbargo.o_embargo = intEmbargos.observaciones;

                    if (intEmbargos.sucursal != "")
                    {
                        lasssecc = "buscando banco";
                        string banco = NomadEnvironment.QueryValue("LIQ22_BANCOS", "oi_banco", "c_banco", intEmbargos.banco, "", true);
                        if (banco == null || banco == "")
                            throw new Exception("Banco no encontrado");

                        lasssecc = "buscando sucursal";
                        string sucursal = NomadEnvironment.QueryValue("LIQ22_BANCOS_SUCUR", "oi_sucursal", "c_sucursal", intEmbargos.sucursal, "LIQ22_BANCOS_SUCUR.oi_banco = " + banco, true);
                        if (sucursal == null || sucursal == "")
                            throw new Exception("Sucursal bancaria no encontrada");
                        ddoEmbargo.oi_sucursal = sucursal;
                    }

                    if (intEmbargos.concepto != "")
                    {
                        lasssecc = "buscando concepto";
                        string concepto = NomadEnvironment.QueryValue("LIQ14_CONCEPTOS", "oi_concepto", "c_concepto", intEmbargos.concepto, "", true);
                        if (concepto == null || concepto == "")
                            throw new Exception("Concepto no encontrado");
                        ddoEmbargo.oi_concepto = concepto;
                    }

                    ddoEmbargo.d_actor = intEmbargos.actor;
                    ddoEmbargo.d_causa = intEmbargos.causa;


                    //Grabo
                    lasssecc = "Guardando embargo";
                    NomadEnvironment.GetCurrentTransaction().Save(legajo); regadd++;
                    if (regadd % 100 == 0) b.Log("Se agregaron " + regadd.ToString() + " embargos.");
                }
                catch (Exception e)
                {
                    regerr++;
                    b.Err("Error desconocido. " + e.Message + " - Linea " + linea.ToString() + " - " + lasssecc);
                }
            }

            b.Log("Se agregaron " + regadd.ToString() + " embargos.");
            if (regerr > 0) b.Err("Se encontraron " + regerr.ToString() + " embargos con error.");
        }
    }
}

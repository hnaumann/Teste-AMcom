using System.Globalization;

namespace Questao1
{
    class ContaBancaria
    {
        public int Numero { get; }
        public string NomeTitular { get; private set; }
        private double Saldo;

        public ContaBancaria(int numero, string nomeTitular)
        {
            Numero = numero;
            NomeTitular = nomeTitular;
            Saldo = 0.0;
        }

        public ContaBancaria(int numero, string nomeTitular, double depositoInicial) : this(numero, nomeTitular)
        {
            Deposito(depositoInicial);
        }

        public void Deposito(double valor)
        {
            Saldo += valor;
        }

        public void Saque(double valor)
        {
            Saldo -= valor + 3.50;
        }

        public override string ToString()
        {
            return $"Conta {Numero}, Titular: {NomeTitular}, Saldo: $ {Saldo.ToString("F2", CultureInfo.InvariantCulture)}";
        }
    }
}

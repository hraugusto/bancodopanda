namespace Caixa_Eletronico.Modelos
{
    public class Conta
    {
        public int Id { get; private set; }
        public int Numero { get; private set; }
        public string Titular { get; private set; }
        public decimal Saldo { get; private set; }

        private Conta(int numero, string titular)
        {
            Numero = numero;
            Titular = titular;
            Saldo = 0;
        }

        internal Conta(int id, int numero, string titular, decimal saldo)
        {
            Id = id;
            Numero = numero;
            Titular = titular;
            Saldo = saldo;
        }

        public static Conta Criar(string titular)
        {
            var numero = new Random().Next(1000, 9999);
            return new Conta(numero, titular);
        }

        public void Depositar(decimal valor)
        {
            if (valor <= 0)
                throw new ArgumentException("O valor do depósito deve ser maior que zero.");

            Saldo += valor;
        }

        public void Sacar(decimal valor)
        {
            if (valor <= 0)
                throw new ArgumentException("O valor do saque deve ser maior que zero.");

            if (Saldo < valor)
                throw new InvalidOperationException("Saldo insuficiente para realizar o saque.");

            Saldo -= valor;
        }

        public void Transferir(Conta contaDestino, decimal valor)
        {
            if (contaDestino == null)
                throw new ArgumentNullException(nameof(contaDestino));

            if (Id == contaDestino.Id)
                throw new InvalidOperationException("Não é possível transferir para a mesma conta.");

            Sacar(valor);
            contaDestino.Depositar(valor);
        }
    }
}

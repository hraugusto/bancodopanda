using Caixa_Eletronico.Data;
using Caixa_Eletronico.Modelos;

namespace Caixa_Eletronico.Servicos
{
    public class BancoService : IDisposable
    {
        private readonly BancoContext _context;

        public BancoService()
        {
            _context = new BancoContext();
            _context.InicializarBancoDados();
        }

        public Conta CriarConta(string titular)
        {
            var conta = Conta.Criar(titular);
            _context.InserirConta(conta);
            return conta;
        }

        public Conta BuscarConta(int numero)
        {
            return _context.BuscarContaPorNumero(numero)
                ?? throw new InvalidOperationException("Conta não encontrada.");
        }

        public void Depositar(int numeroConta, decimal valor)
        {
            var conta = BuscarConta(numeroConta);
            conta.Depositar(valor);
            _context.AtualizarSaldo(conta.Id, conta.Saldo);

            var transacao = new Transacao
            {
                Tipo = TipoTransacao.Deposito,
                Valor = valor,
                DataHora = DateTime.Now,
                ContaOrigemId = conta.Id
            };

            _context.InserirTransacao(transacao);
        }

        public void Sacar(int numeroConta, decimal valor)
        {
            var conta = BuscarConta(numeroConta);
            conta.Sacar(valor);
            _context.AtualizarSaldo(conta.Id, conta.Saldo);

            var contaAtualizada = BuscarConta(numeroConta);

            var transacao = new Transacao
            {
                Tipo = TipoTransacao.Saque,
                Valor = valor,
                DataHora = DateTime.Now,
                ContaOrigemId = conta.Id
            };

            _context.InserirTransacao(transacao);
        }

        public void Transferir(int numeroContaOrigem, int numeroContaDestino, decimal valor)
        {
            var contaOrigem = BuscarConta(numeroContaOrigem);
            var contaDestino = BuscarConta(numeroContaDestino);

            contaOrigem.Transferir(contaDestino, valor);
            _context.AtualizarSaldo(contaOrigem.Id, contaOrigem.Saldo);
            _context.AtualizarSaldo(contaDestino.Id, contaDestino.Saldo);

            var transacao = new Transacao
            {
                Tipo = TipoTransacao.Transferencia,
                Valor = valor,
                DataHora = DateTime.Now,
                ContaOrigemId = contaOrigem.Id,
                ContaDestinoId = contaDestino.Id
            };

            _context.InserirTransacao(transacao);
        }

        public decimal ConsultarSaldo(int numeroConta)
        {
            var conta = BuscarConta(numeroConta);
            return conta.Saldo;
        }

        public IEnumerable<Transacao> ConsultarHistorico(int numeroConta)
        {
            var conta = BuscarConta(numeroConta);
            return _context.BuscarTransacoes(conta.Id);
        }

        public void Dispose()
        {
            _context?.Dispose();
        }
    }

}

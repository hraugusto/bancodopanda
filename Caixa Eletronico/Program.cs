using Caixa_Eletronico.Modelos;
using Caixa_Eletronico.Servicos;

class Program
{
    private static BancoService _bancoService = new BancoService();

    static void Main(string[] args)
    {
        while (true)
        {
            Console.Clear();
            Console.WriteLine("*** Banco do Panda ***");
            Console.WriteLine("1 - Criar Conta");
            Console.WriteLine("2 - Depositar");
            Console.WriteLine("3 - Sacar");
            Console.WriteLine("4 - Transferir");
            Console.WriteLine("5 - Consultar Saldo");
            Console.WriteLine("6 - Consultar Histórico");
            Console.WriteLine("0 - Sair");
            Console.Write("Escolha uma opção: ");

            var opcao = Console.ReadLine();

            try
            {
                switch (opcao)
                {
                    case "1":
                        CriarConta();
                        break;
                    case "2":
                        Depositar();
                        break;
                    case "3":
                        Sacar();
                        break;
                    case "4":
                        Transferir();
                        break;
                    case "5":
                        ConsultarSaldo();
                        break;
                    case "6":
                        ConsultarHistorico();
                        break;
                    case "0":
                        _bancoService.Dispose();
                        return;
                    default:
                        Console.WriteLine("Opção inválida!");
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro: {ex.Message}");
            }

            Console.WriteLine("\nPressione qualquer tecla para continuar...");
            Console.ReadKey();
        }
    }

    static void CriarConta()
    {
        Console.Write("Digite o nome do titular: ");
        var titular = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(titular))
            throw new ArgumentException("Nome do titular é obrigatório.");

        var conta = _bancoService.CriarConta(titular);
        Console.WriteLine($"Conta criada com sucesso! Número: {conta.Numero}");
    }

    static void Depositar()
    {
        Console.Write("Digite o número da conta: ");
        if (!int.TryParse(Console.ReadLine(), out int numero))
            throw new ArgumentException("Número da conta inválido.");

        Console.Write("Digite o valor do depósito: ");
        if (!decimal.TryParse(Console.ReadLine(), out decimal valor))
            throw new ArgumentException("Valor inválido.");

        _bancoService.Depositar(numero, valor);
        Console.WriteLine("Depósito realizado com sucesso!");
    }

    static void Sacar()
    {
        Console.Write("Digite o número da conta: ");
        if (!int.TryParse(Console.ReadLine(), out int numero))
            throw new ArgumentException("Número da conta inválido.");

        Console.Write("Digite o valor do saque: ");
        if (!decimal.TryParse(Console.ReadLine(), out decimal valor))
            throw new ArgumentException("Valor inválido.");

        _bancoService.Sacar(numero, valor);
        Console.WriteLine("Saque realizado com sucesso!");
    }

    static void Transferir()
    {
        Console.Write("Digite o número da conta de origem: ");
        if (!int.TryParse(Console.ReadLine(), out int numeroOrigem))
            throw new ArgumentException("Número da conta de origem inválido.");

        Console.Write("Digite o número da conta de destino: ");
        if (!int.TryParse(Console.ReadLine(), out int numeroDestino))
            throw new ArgumentException("Número da conta de destino inválido.");

        Console.Write("Digite o valor da transferência: ");
        if (!decimal.TryParse(Console.ReadLine(), out decimal valor))
            throw new ArgumentException("Valor inválido.");

        _bancoService.Transferir(numeroOrigem, numeroDestino, valor);
        Console.WriteLine("Transferência realizada com sucesso!");
    }

    static void ConsultarSaldo()
    {
        Console.Write("Digite o número da conta: ");
        if (!int.TryParse(Console.ReadLine(), out int numero))
            throw new ArgumentException("Número da conta inválido.");

        var saldo = _bancoService.ConsultarSaldo(numero);
        Console.WriteLine($"Saldo atual: R$ {saldo:F2}");
    }

    static void ConsultarHistorico()
    {
        Console.Write("Digite o número da conta: ");
        if (!int.TryParse(Console.ReadLine(), out int numero))
            throw new ArgumentException("Número da conta inválido.");

        var transacoes = _bancoService.ConsultarHistorico(numero);

        if (!transacoes.Any())
        {
            Console.WriteLine("Nenhuma transação encontrada.");
            return;
        }

        foreach (var transacao in transacoes)
        {
            string descricao = transacao.Tipo switch
            {
                TipoTransacao.Deposito => $"Depósito de R$ {transacao.Valor:F2}",
                TipoTransacao.Saque => $"Saque de R$ {transacao.Valor:F2}",
                TipoTransacao.Transferencia => transacao.ContaOrigem.Id == transacao.ContaOrigemId
                    ? $"Transferência enviada de R$ {transacao.Valor:F2} para conta {transacao.ContaDestino?.Numero}"
                    : $"Transferência recebida de R$ {transacao.Valor:F2} da conta {transacao.ContaOrigem?.Numero}",
                _ => "Transação desconhecida"
            };

            Console.WriteLine($"{transacao.DataHora:dd/MM/yyyy HH:mm:ss} - {descricao}");
        }
    }
}
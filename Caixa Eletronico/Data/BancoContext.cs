using Caixa_Eletronico.Modelos;
using Microsoft.Data.Sqlite;

namespace Caixa_Eletronico.Data
{
    public class BancoContext : IDisposable
    {
        private readonly SqliteConnection _connection;
        private bool _initialized = false;

        public BancoContext()
        {
            var folder = Environment.SpecialFolder.LocalApplicationData;
            var path = Environment.GetFolderPath(folder);
            var dbPath = Path.Join(path, "caixa_eletronico.db");

            _connection = new SqliteConnection($"Data Source={dbPath}");
            _connection.Open();
        }

        public void InicializarBancoDados()
        {
            if (_initialized) return;

            using var command = _connection.CreateCommand();

            // Criar tabela Contas
            command.CommandText = @"
                CREATE TABLE IF NOT EXISTS Contas (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Numero INTEGER NOT NULL UNIQUE,
                    Titular TEXT NOT NULL,
                    Saldo DECIMAL NOT NULL
                )";
            command.ExecuteNonQuery();

            // Criar tabela Transacoes
            command.CommandText = @"
                CREATE TABLE IF NOT EXISTS Transacoes (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Tipo INTEGER NOT NULL,
                    Valor DECIMAL NOT NULL,
                    DataHora TEXT NOT NULL,
                    ContaOrigemId INTEGER NOT NULL,
                    ContaDestinoId INTEGER,
                    FOREIGN KEY(ContaOrigemId) REFERENCES Contas(Id),
                    FOREIGN KEY(ContaDestinoId) REFERENCES Contas(Id)
                )";
            command.ExecuteNonQuery();

            _initialized = true;
        }

        public void InserirConta(Conta conta)
        {
            using var command = _connection.CreateCommand();
            command.CommandText = @"
                INSERT INTO Contas (Numero, Titular, Saldo)
                VALUES (@Numero, @Titular, @Saldo);
                SELECT last_insert_rowid();";

            command.Parameters.AddWithValue("@Numero", conta.Numero);
            command.Parameters.AddWithValue("@Titular", conta.Titular);
            command.Parameters.AddWithValue("@Saldo", conta.Saldo);

            var id = (long)command.ExecuteScalar();
            typeof(Conta).GetProperty("Id").SetValue(conta, (int)id);
        }

        public void AtualizarSaldo(int contaId, decimal novoSaldo)
        {
            using var command = _connection.CreateCommand();
            command.CommandText = "UPDATE Contas SET Saldo = @Saldo WHERE Id = @Id";
            command.Parameters.AddWithValue("@Saldo", novoSaldo);
            command.Parameters.AddWithValue("@Id", contaId);
            command.ExecuteNonQuery();
        }

        public Conta BuscarContaPorNumero(int numero)
        {
            using var command = _connection.CreateCommand();
            command.CommandText = "SELECT Id, Numero, Titular, Saldo FROM Contas WHERE Numero = @Numero";
            command.Parameters.AddWithValue("@Numero", numero);

            using var reader = command.ExecuteReader();
            if (!reader.Read())
                return null;

            return new Conta(
                id: reader.GetInt32(0),
                numero: reader.GetInt32(1),
                titular: reader.GetString(2),
                saldo: reader.GetDecimal(3));
        }

        public void InserirTransacao(Transacao transacao)
        {
            using var command = _connection.CreateCommand();
            command.CommandText = @"
                INSERT INTO Transacoes (Tipo, Valor, DataHora, ContaOrigemId, ContaDestinoId)
                VALUES (@Tipo, @Valor, @DataHora, @ContaOrigemId, @ContaDestinoId);
                SELECT last_insert_rowid();";

            command.Parameters.AddWithValue("@Tipo", (int)transacao.Tipo);
            command.Parameters.AddWithValue("@Valor", transacao.Valor);
            command.Parameters.AddWithValue("@DataHora", transacao.DataHora.ToString("O"));
            command.Parameters.AddWithValue("@ContaOrigemId", transacao.ContaOrigemId);
            command.Parameters.AddWithValue("@ContaDestinoId", transacao.ContaDestinoId.HasValue
                ? transacao.ContaDestinoId.Value : DBNull.Value);

            var id = (long)command.ExecuteScalar();
            typeof(Transacao).GetProperty("Id").SetValue(transacao, (int)id);
        }

        public List<Transacao> BuscarTransacoes(int contaId)
        {
            var transacoes = new List<Transacao>();
            using var command = _connection.CreateCommand();
            command.CommandText = @"
                SELECT t.Id, t.Tipo, t.Valor, t.DataHora, t.ContaOrigemId, t.ContaDestinoId,
                       o.Numero as NumeroOrigem, o.Titular as TitularOrigem,
                       d.Numero as NumeroDestino, d.Titular as TitularDestino
                FROM Transacoes t
                INNER JOIN Contas o ON t.ContaOrigemId = o.Id
                LEFT JOIN Contas d ON t.ContaDestinoId = d.Id
                WHERE t.ContaOrigemId = @ContaId OR t.ContaDestinoId = @ContaId
                ORDER BY t.DataHora DESC";

            command.Parameters.AddWithValue("@ContaId", contaId);

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                var contaOrigem = new Conta(
                    id: reader.GetInt32(4),
                    numero: reader.GetInt32(6),
                    titular: reader.GetString(7),
                    saldo: 0);  // Saldo não é necessário para o histórico

                Conta contaDestino = null;
                if (!reader.IsDBNull(5))
                {
                    contaDestino = new Conta(
                        id: reader.GetInt32(5),
                        numero: reader.GetInt32(8),
                        titular: reader.GetString(9),
                        saldo: 0);
                }

                var transacao = new Transacao
                {
                    Id = reader.GetInt32(0),
                    Tipo = (TipoTransacao)reader.GetInt32(1),
                    Valor = reader.GetDecimal(2),
                    DataHora = DateTime.Parse(reader.GetString(3)),
                    ContaOrigemId = reader.GetInt32(4),
                    ContaDestinoId = reader.IsDBNull(5) ? null : (int?)reader.GetInt32(5),
                    ContaOrigem = contaOrigem,
                    ContaDestino = contaDestino
                };

                transacoes.Add(transacao);
            }

            return transacoes;
        }

        public void Dispose()
        {
            _connection?.Dispose();
        }
    }
}
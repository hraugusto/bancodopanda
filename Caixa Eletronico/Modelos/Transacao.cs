namespace Caixa_Eletronico.Modelos
{
    public enum TipoTransacao
    {
        Deposito,
        Saque,
        Transferencia
    }

    public class Transacao
    {
        public int Id { get; set; }
        public TipoTransacao Tipo { get; set; }
        public decimal Valor { get; set; }
        public DateTime DataHora { get; set; }
        public int ContaOrigemId { get; set; }
        public int? ContaDestinoId { get; set; }
        public Conta ContaOrigem { get; set; }
        public Conta ContaDestino { get; set; }
    }
}
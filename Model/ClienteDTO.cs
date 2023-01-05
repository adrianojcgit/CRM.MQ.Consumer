namespace CRM.MQ.Consumer.Model
{
	public class ClienteDTO
	{
		public int Id { get; protected set; }
		public DateTime DataCadastro { get; protected set; }
		public DateTime? DataAtualizacao { get; protected set; }
		public string CnpjNumInscricao { get; set; }
		public string NomeEmpresarial { get; set; }
		public string NomeFantasia { get; set; }
		public decimal FatBrutoAnual { get; set; }
		public bool Ativo { get; set; }
	}
}

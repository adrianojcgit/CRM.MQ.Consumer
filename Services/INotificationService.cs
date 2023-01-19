namespace CRM.MQ.Consumer.Services
{
    public interface INotificationService
    {
        void NotifyUser(string? NomeEmpresarial, string? CnpjNumInscricao, decimal? FatBrutoAnual);
    }
}

using Invoice.Repository.Entity;

namespace Invoice.Repository.Repositories
{
    public interface IInvoiceRepository
    {
        Task<InvoiceEntity> CreateInvoiceAsync(InvoiceEntity invoice);
        Task<List<InvoiceEntity>> GetInvoicesAsync();
        Task<InvoiceEntity> GetInvoiceByIdAsync(int id);
        Task UpdateInvoiceAsync(InvoiceEntity invoice);
    }
}
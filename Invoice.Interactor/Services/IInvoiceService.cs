using Invoice.Interactor.DTO;
using Invoice.Interactor.Models;

namespace InvoiceSystem.Services
{
    public interface IInvoiceService
    {
        Task<CreateInvoiceRespose> CreateInvoiceAsync(InvoiceDTO invoice);
        Task<List<InvoiceModel>> GetInvoicesAsync();
        Task ProcessPaymentsAsync(int id, decimal amount);
        Task ProcessOverdueAsync(OverdueDTO overdueDTO);

    }
}
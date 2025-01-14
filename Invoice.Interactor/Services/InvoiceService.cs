using AutoMapper;
using Invoice.Interactor.Models;
using Invoice.Interactor.DTO;
using Invoice.Repository.Repositories;
using Invoice.Repository.Entity;
using Invoice.Repository.Enum;


namespace InvoiceSystem.Services
{
    public class InvoiceService : IInvoiceService
    {
        private readonly IInvoiceRepository _invoiceRepository;
        private readonly IMapper _mapper;

        public InvoiceService(IInvoiceRepository invoiceRepository, IMapper mapper)
        {
            _invoiceRepository = invoiceRepository;
            _mapper = mapper;
        }

        public async Task<CreateInvoiceRespose> CreateInvoiceAsync(InvoiceDTO invoice)
        {
            if (invoice.Duedate < DateOnly.FromDateTime(DateTime.Today))
            {
                throw new InvalidOperationException("The due date cannot be in the past.");
            }
            var invoiceEntity = _mapper.Map<InvoiceEntity>(invoice);
            var createdInvoiceEntity = await _invoiceRepository.CreateInvoiceAsync(invoiceEntity);
            CreateInvoiceRespose result = new()
            {
                Id = createdInvoiceEntity.Id.ToString()
            };
            return result;
        }

        public async Task<List<InvoiceModel>> GetInvoicesAsync()
        {
            var invoiceEntities = await _invoiceRepository.GetInvoicesAsync();
            return _mapper.Map<List<InvoiceModel>>(invoiceEntities);
        }

        public async Task ProcessPaymentsAsync(int id, decimal amount)
        {
            var invoiceEntity = await _invoiceRepository.GetInvoiceByIdAsync(id);
           
            if (invoiceEntity == null || invoiceEntity.Status == InvoiceStatusEnum.Void.ToString()) throw new Exception("Invoice not found or not payable");
            if (invoiceEntity.Duedate < DateOnly.FromDateTime(DateTime.Today)) throw new InvalidOperationException("The due date has already passed, so payment cannot be made at this time.");
            if (invoiceEntity.Amount == 0) throw new Exception("Payment is already done");
            if (amount > invoiceEntity.Amount) throw new Exception("Overpayment is not allowed");
          
            invoiceEntity.Paid_amount += amount;
            invoiceEntity.Amount -= amount;

            if (invoiceEntity.Amount == 0)
            {
                invoiceEntity.Status = InvoiceStatusEnum.Paid.ToString();
            }
           
            await _invoiceRepository.UpdateInvoiceAsync(invoiceEntity);
        }

        public async Task ProcessOverdueAsync(OverdueDTO overdueDTO)
        {
            var overdueInvoices = await _invoiceRepository.GetInvoicesAsync();

            foreach (var invoiceEntity in overdueInvoices)
            {
                // check if invoice is not overdue, paid or void
                if (invoiceEntity.Duedate >= DateOnly.FromDateTime(DateTime.Today) ||
                    invoiceEntity.Status == InvoiceStatusEnum.Paid.ToString() ||
                    invoiceEntity.Status == InvoiceStatusEnum.Void.ToString())
                {
                    continue;
                }
                bool isPartiallyPaid = invoiceEntity.Paid_amount > 0 && invoiceEntity.Paid_amount < invoiceEntity.Amount;
                invoiceEntity.Status = isPartiallyPaid ? InvoiceStatusEnum.Paid.ToString() : InvoiceStatusEnum.Void.ToString();

                var newInvoiceEntity = new InvoiceEntity
                {
                    Amount = invoiceEntity.Amount + overdueDTO.late_fee,
                    Duedate = invoiceEntity.Duedate.AddDays(overdueDTO.overdue_days),
                    Status = InvoiceStatusEnum.Pending.ToString()
                };
                await _invoiceRepository.CreateInvoiceAsync(newInvoiceEntity);
            }
        }
    }
}

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
            if (invoiceEntity == null) throw new Exception("Invoice not found");
            if (invoiceEntity.Amount == 0) throw new Exception("Payment is already done");

            invoiceEntity.Paid_amount += amount;
            invoiceEntity.Amount -= amount;

            if (invoiceEntity.Amount == 0)
            {
                invoiceEntity.Status = InvoiceStatusEnum.Paid.ToString();
            }
            else if(invoiceEntity.Paid_amount > invoiceEntity.Amount)
            {
                throw new Exception("Overpayment is not allowed");
            }

            await _invoiceRepository.UpdateInvoiceAsync(invoiceEntity);
        }

        public async Task ProcessOverdueAsync(OverdueDTO overdueDTO)
        {
            var overdueInvoices = await _invoiceRepository.GetInvoicesAsync();
            foreach (var invoiceEntity in overdueInvoices)
            {
                if (invoiceEntity.Duedate < DateOnly.FromDateTime(DateTime.Today) && invoiceEntity.Status != InvoiceStatusEnum.Paid.ToString() && invoiceEntity.Status != InvoiceStatusEnum.Void.ToString())
                {
                    if (invoiceEntity.Paid_amount > 0 && invoiceEntity.Paid_amount < invoiceEntity.Amount)
                    {
                        invoiceEntity.Status = InvoiceStatusEnum.Paid.ToString();
                        var remainingAmount = invoiceEntity.Amount + overdueDTO.late_fee;
                        var newInvoiceEntity = new InvoiceEntity
                        {
                            Amount = remainingAmount,
                            Duedate = invoiceEntity.Duedate.AddDays(overdueDTO.overdue_days),
                            Status = InvoiceStatusEnum.Pending.ToString()
                        };
                        await _invoiceRepository.CreateInvoiceAsync(newInvoiceEntity);
                    }
                    else if (invoiceEntity.Paid_amount == 0)
                    {
                        invoiceEntity.Status = InvoiceStatusEnum.Void.ToString();
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
    }
}

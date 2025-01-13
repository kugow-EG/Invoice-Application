using Invoice.Repository.Data;
using Invoice.Repository.Entity;
using Microsoft.EntityFrameworkCore;

namespace Invoice.Repository.Repositories
{
    public class InvoiceRepository : IInvoiceRepository
    {
        private readonly InvoiceDbContext _context;

        public InvoiceRepository(InvoiceDbContext context)
        {
            _context = context;
        }

        public async Task<InvoiceEntity> CreateInvoiceAsync(InvoiceEntity invoice)
        {
            _context.InvoiceTable.Add(invoice);
            await _context.SaveChangesAsync();
            return invoice;
        }

        public async Task<List<InvoiceEntity>> GetInvoicesAsync()
        {
            return await _context.InvoiceTable.ToListAsync();
        }

        public async Task<InvoiceEntity> GetInvoiceByIdAsync(int id)
        {
            return await _context.InvoiceTable.FindAsync(id);
        }

        public async Task UpdateInvoiceAsync(InvoiceEntity invoice)
        {
            _context.InvoiceTable.Update(invoice);
            await _context.SaveChangesAsync();
        }
    }
}

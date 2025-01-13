using Microsoft.AspNetCore.Mvc;
using AutoMapper;
using InvoiceSystem.Services;
using Invoice.Interactor.DTO;

namespace Invoice.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InvoiceController : ControllerBase
    {
        private readonly IInvoiceService _invoiceService;
        private readonly IMapper _mapper;

        public InvoiceController(IInvoiceService invoiceService, IMapper mapper)
        {
            _invoiceService = invoiceService;
            _mapper = mapper;
        }

        [HttpPost]
        public async Task<IActionResult> CreateInvoice([FromBody] InvoiceDTO invoiceDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var createdInvoice = await _invoiceService.CreateInvoiceAsync(invoiceDto);

            return CreatedAtAction(nameof(CreateInvoice),createdInvoice);
        }

        [HttpGet]
        public async Task<IActionResult> GetInvoices()
        {
            var invoices = await _invoiceService.GetInvoicesAsync();
            return Ok(invoices);
        }

        [HttpPost("{id}/payments")]
        public async Task<IActionResult> ProcessPayments(int id, [FromBody] decimal amount)
        {
            try
            {
                await _invoiceService.ProcessPaymentsAsync(id, amount);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("process-overdue")]
        public async Task<IActionResult> ProcessOverdue([FromBody] OverdueDTO overdueDTO)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            await _invoiceService.ProcessOverdueAsync(overdueDTO);
            return Ok();
        }
    }
}

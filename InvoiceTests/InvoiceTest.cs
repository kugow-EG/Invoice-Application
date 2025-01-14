using AutoMapper;
using Invoice.Interactor.DTO;
using Invoice.Interactor.Models;
using Invoice.Repository.Entity;
using Invoice.Repository.Repositories;
using InvoiceSystem.Services;
using Moq;

namespace InvoiceTests
{
    public class InvoiceServiceTests
    {
        private readonly Mock<IInvoiceRepository> _mockRepository;
        private readonly Mock<IMapper> _mockMapper;
        private readonly InvoiceService _invoiceService;

        public InvoiceServiceTests()
        {
            _mockRepository = new Mock<IInvoiceRepository>();
            _mockMapper = new Mock<IMapper>();
            _invoiceService = new InvoiceService(_mockRepository.Object, _mockMapper.Object);
        }

        [Fact]
        public async Task CreateInvoiceAsync_ShouldReturnResponse_WhenInvoiceIsCreated()
        {
            // Arrange
            var invoiceDto = new InvoiceDTO { Amount = 100, Duedate = DateOnly.FromDateTime(DateTime.Today.AddDays(5)) };
            var invoiceEntity = new InvoiceEntity { Id = 1, Amount = 100, Duedate = DateOnly.FromDateTime(DateTime.Today.AddDays(5)) };
            var response = new CreateInvoiceRespose { Id = "1" };

            _mockMapper.Setup(m => m.Map<InvoiceEntity>(invoiceDto)).Returns(invoiceEntity);
            _mockRepository.Setup(r => r.CreateInvoiceAsync(invoiceEntity)).ReturnsAsync(invoiceEntity);

            // Act
            var result = await _invoiceService.CreateInvoiceAsync(invoiceDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(response.Id, result.Id);
        }

        [Fact]
        public async Task GetInvoicesAsync_ShouldReturnInvoices_WhenInvoicesExist()
        {
            // Arrange
            var invoiceEntities = new List<InvoiceEntity>
            {
                new InvoiceEntity { Id = 1, Amount = 100, Status = "pending" },
                new InvoiceEntity { Id = 2, Amount = 200, Status = "paid" }
            };

            var invoiceModels = new List<InvoiceModel>
            {
                new InvoiceModel { Id = 1, Amount = 100, Status = "pending" },
                new InvoiceModel { Id = 2, Amount = 200, Status = "paid" }
            };

            _mockRepository.Setup(r => r.GetInvoicesAsync()).ReturnsAsync(invoiceEntities);
            _mockMapper.Setup(m => m.Map<List<InvoiceModel>>(invoiceEntities)).Returns(invoiceModels);

            // Act
            var result = await _invoiceService.GetInvoicesAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
        }

        [Fact]
        public async Task ProcessPaymentsAsync_ShouldUpdateInvoice_WhenPaymentIsValid()
        {
            // Arrange
            var invoiceEntity = new InvoiceEntity { Id = 1, Amount = 100, Duedate = DateOnly.FromDateTime(DateTime.Today.AddDays(5)), Paid_amount = 0, Status = "pending" };
            var paymentAmount = 50;

            _mockRepository.Setup(r => r.GetInvoiceByIdAsync(invoiceEntity.Id)).ReturnsAsync(invoiceEntity);

            // Act
            await _invoiceService.ProcessPaymentsAsync(invoiceEntity.Id, paymentAmount);

            // Assert
            Assert.Equal(50, invoiceEntity.Paid_amount);
            Assert.Equal(50, invoiceEntity.Amount);
            Assert.Equal("pending", invoiceEntity.Status);

            _mockRepository.Verify(r => r.UpdateInvoiceAsync(invoiceEntity), Times.Once);
        }

        [Fact]
        public async Task ProcessPaymentsAsync_ShouldUpdateStatusToPaid_WhenFullPaymentIsMade()
        {
            // Arrange
            var invoiceEntity = new InvoiceEntity { Id = 1, Amount = 100, Duedate = DateOnly.FromDateTime(DateTime.Today.AddDays(5)), Paid_amount = 0, Status = "pending" };
            var paymentAmount = 100;

            _mockRepository.Setup(r => r.GetInvoiceByIdAsync(invoiceEntity.Id)).ReturnsAsync(invoiceEntity);

            // Act
            await _invoiceService.ProcessPaymentsAsync(invoiceEntity.Id, paymentAmount);

            // Assert
            Assert.Equal(100, invoiceEntity.Paid_amount);
            Assert.Equal(0, invoiceEntity.Amount);
            Assert.Equal("Paid", invoiceEntity.Status);

            _mockRepository.Verify(r => r.UpdateInvoiceAsync(invoiceEntity), Times.Once);
        }

        [Fact]
        public async Task ProcessPaymentsAsync_ShouldThrowException_WhenOverpaymentOccurs()
        {
            // Arrange
            var invoiceEntity = new InvoiceEntity { Id = 1, Amount = 100, Duedate = DateOnly.FromDateTime(DateTime.Today.AddDays(5)),  Paid_amount = 0, Status = "pending" };
            var paymentAmount = 150;

            _mockRepository.Setup(r => r.GetInvoiceByIdAsync(invoiceEntity.Id)).ReturnsAsync(invoiceEntity);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _invoiceService.ProcessPaymentsAsync(invoiceEntity.Id, paymentAmount));
            Assert.Equal("Overpayment is not allowed", exception.Message);
        }

        [Fact]
        public async Task ProcessOverdueAsync_ShouldHandleOverdueInvoices()
        {
            // Arrange
            var overdueDTO = new OverdueDTO { late_fee = 20, overdue_days = 5 };
            var overdueInvoices = new List<InvoiceEntity>
            {
                new InvoiceEntity { Id = 1, Amount = 100, Paid_amount = 0, Duedate = DateOnly.FromDateTime(DateTime.Today.AddDays(-1)), Status = "pending" },
                new InvoiceEntity { Id = 2, Amount = 200, Paid_amount = 100, Duedate = DateOnly.FromDateTime(DateTime.Today.AddDays(-2)), Status = "pending" }
            };

            _mockRepository.Setup(r => r.GetInvoicesAsync()).ReturnsAsync(overdueInvoices);

            // Act
            await _invoiceService.ProcessOverdueAsync(overdueDTO);

            // Assert
            _mockRepository.Verify(r => r.CreateInvoiceAsync(It.Is<InvoiceEntity>(e => e.Amount == 120 && e.Status == "Pending")), Times.Once);
            _mockRepository.Verify(r => r.CreateInvoiceAsync(It.Is<InvoiceEntity>(e => e.Amount == 120 && e.Status == "Pending")), Times.Once);
        }
    }
}

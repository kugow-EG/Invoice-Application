namespace Invoice.Interactor.DTO
{
    public class OverdueDTO
    {
        public decimal late_fee { get; set; }

        public int overdue_days { get; set; }
    }
}

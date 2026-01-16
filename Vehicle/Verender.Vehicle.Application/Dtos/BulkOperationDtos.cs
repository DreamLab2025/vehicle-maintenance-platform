namespace Verender.Vehicle.Application.Dtos
{
    public class BulkOperationError
    {
        public int Index { get; set; }
        public string ItemName { get; set; } = null!;
        public string ErrorMessage { get; set; } = null!;
    }
}

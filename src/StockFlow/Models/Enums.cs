namespace StockFlow.Models;

public enum StockTransactionType
{
    Received,
    Sale,
    Adjustment,
    Damaged,
    Return,
    SaleVoided
}

public enum PaymentMethod
{
    Cash,
    Card
}

public enum SaleStatus
{
    Completed,
    Cancelled
}

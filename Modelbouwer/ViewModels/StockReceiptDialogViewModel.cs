namespace Modelbouwer.ViewModels;

public class StockReceiptDialogViewModel
{
	public StockReceiptDialogModel Model { get; }

	public StockReceiptDialogViewModel( StockReceiptDialogModel model )
	{
		Model = model;
	}

	public string? Validate()
	{
		if ( Model.ReceivedAmount < 0d )
			return Lang.StockReceiptReceivedAmountNegativeWarning;

		if ( Model.ReceivedAmount > Model.OrderedAmount )
			return Lang.StockReceiptReceivedAmountTooHighWarning;

		return null;
	}

	public string? GetIncompleteCloseWarning()
	{
		return Model.IsIncompleteClose ? Lang.StockReceiptIncompleteCloseWarning : null;
	}

	public void LeaveOpen()
	{
		Model.LeaveOpen();
	}

	public void CompleteWithReceivedAmount()
	{
		Model.CompleteWithReceivedAmount();
	}
}
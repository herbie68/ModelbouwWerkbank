namespace Modelbouwer.Models;

public partial class StockReceiptDialogModel : ObservableObject
{
	private bool _suppressRecalculation;
	private bool _closedAutomaticallyByReceivedAmount;

	[ObservableProperty] private int _orderLineId;
	[ObservableProperty] private int _orderId;
	[ObservableProperty] private int _productId;
	[ObservableProperty] private string? _productNumber;
	[ObservableProperty] private string? _productDescription;
	[ObservableProperty] private double _orderedAmount;
	[ObservableProperty] private double _alreadyReceivedAmount;
	[ObservableProperty] private double _receivedAmount;
	[ObservableProperty] private double _openAmount;
	[ObservableProperty] private DateTime? _deliveryDate = DateTime.Today;
	[ObservableProperty] private bool _isOrderLineClosed;

	public double ReceivedDelta => ReceivedAmount - AlreadyReceivedAmount;
	public bool IsIncompleteClose => IsOrderLineClosed && OpenAmount > 0d;

	public static StockReceiptDialogModel Create( StockOrderLineModel line )
	{
		double received = line.Amount - line.OpenAmount;

		return new StockReceiptDialogModel
		{
			OrderLineId = line.Id,
			OrderId = line.SupplyOrderId,
			ProductId = line.ProductId,
			ProductNumber = line.DisplayProductNumber,
			ProductDescription = line.DisplayProductDescription,
			OrderedAmount = line.Amount,
			AlreadyReceivedAmount = received,
			ReceivedAmount = received,
			OpenAmount = line.OpenAmount,
			DeliveryDate = line.ClosedDate ?? DateTime.Today,
			IsOrderLineClosed = line.Closed
		};
	}

	public void LeaveOpen()
	{
		IsOrderLineClosed = false;
		RecalculateOpenAmount();
	}

	public void CompleteWithReceivedAmount()
	{
		if ( ReceivedAmount < 0d )
			ReceivedAmount = 0d;

		OrderedAmount = ReceivedAmount;
		OpenAmount = 0d;
		IsOrderLineClosed = true;
	}

	partial void OnOrderedAmountChanged( double value )
	{
		RecalculateOpenAmount();
	}

	partial void OnReceivedAmountChanged( double value )
	{
		RecalculateOpenAmount();
	}

	partial void OnOpenAmountChanged( double value )
	{
		OnPropertyChanged( nameof( IsIncompleteClose ) );
	}

	partial void OnIsOrderLineClosedChanged( bool value )
	{
		OnPropertyChanged( nameof( IsIncompleteClose ) );
	}

	private void RecalculateOpenAmount()
	{
		if ( _suppressRecalculation )
			return;

		_suppressRecalculation = true;
		try
		{
			OpenAmount = Math.Max( OrderedAmount - ReceivedAmount, 0d );
			if ( OpenAmount == 0d && OrderedAmount >= 0d )
			{
				IsOrderLineClosed = true;
				_closedAutomaticallyByReceivedAmount = true;
			}
			else if ( _closedAutomaticallyByReceivedAmount )
			{
				IsOrderLineClosed = false;
				_closedAutomaticallyByReceivedAmount = false;
			}
		}
		finally
		{
			_suppressRecalculation = false;
		}
	}
}

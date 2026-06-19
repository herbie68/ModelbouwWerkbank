namespace Modelbouwer.UnitTests.ViewModels;

[TestClass]
public class StockReceiptDialogViewModelTests
{
	[TestMethod]
	public void ReceivedAmount_WhenComplete_AutomaticallyClosesLine()
	{
		var model = new StockReceiptDialogModel
		{
			OrderedAmount = 5,
			AlreadyReceivedAmount = 2,
			ReceivedAmount = 2,
			OpenAmount = 3
		};
		var viewModel = new StockReceiptDialogViewModel( model );

		model.ReceivedAmount = 5;

		Assert.IsTrue( model.IsOrderLineClosed );
		Assert.AreEqual( 0d, model.OpenAmount );
		Assert.IsNull( viewModel.GetIncompleteCloseWarning() );
	}

	[TestMethod]
	public void ReceivedAmount_WhenReducedAfterAutomaticClose_ReopensLine()
	{
		var model = new StockReceiptDialogModel
		{
			OrderedAmount = 5,
			AlreadyReceivedAmount = 2,
			ReceivedAmount = 2,
			OpenAmount = 3
		};

		model.ReceivedAmount = 5;
		model.ReceivedAmount = 4;

		Assert.IsFalse( model.IsOrderLineClosed );
		Assert.AreEqual( 1d, model.OpenAmount );
	}

	[TestMethod]
	public void IsOrderLineClosed_WhenUserChecksIncompleteLine_StaysClosedForWarning()
	{
		var model = new StockReceiptDialogModel
		{
			OrderedAmount = 5,
			AlreadyReceivedAmount = 2,
			ReceivedAmount = 4,
			OpenAmount = 1
		};

		model.IsOrderLineClosed = true;

		Assert.IsTrue( model.IsOrderLineClosed );
		Assert.IsTrue( model.IsIncompleteClose );
	}

	[TestMethod]
	public void IsOrderLineClosed_WhenIncomplete_ReturnsWarning()
	{
		var model = new StockReceiptDialogModel
		{
			OrderedAmount = 5,
			AlreadyReceivedAmount = 1,
			ReceivedAmount = 3,
			OpenAmount = 2,
			IsOrderLineClosed = true
		};
		var viewModel = new StockReceiptDialogViewModel( model );

		var warning = viewModel.GetIncompleteCloseWarning();

		Assert.AreEqual( Lang.StockReceiptIncompleteCloseWarning, warning );
	}

	[TestMethod]
	public void LeaveOpen_KeepsIncompleteLineOpen()
	{
		var model = new StockReceiptDialogModel
		{
			OrderedAmount = 5,
			AlreadyReceivedAmount = 1,
			ReceivedAmount = 3,
			IsOrderLineClosed = true
		};
		var viewModel = new StockReceiptDialogViewModel( model );

		viewModel.LeaveOpen();

		Assert.IsFalse( model.IsOrderLineClosed );
		Assert.AreEqual( 2d, model.OpenAmount );
	}

	[TestMethod]
	public void CompleteWithReceivedAmount_ReducesOrderedAmountAndClosesLine()
	{
		var model = new StockReceiptDialogModel
		{
			OrderedAmount = 5,
			AlreadyReceivedAmount = 1,
			ReceivedAmount = 3,
			IsOrderLineClosed = true
		};
		var viewModel = new StockReceiptDialogViewModel( model );

		viewModel.CompleteWithReceivedAmount();

		Assert.AreEqual( 3d, model.OrderedAmount );
		Assert.AreEqual( 0d, model.OpenAmount );
		Assert.IsTrue( model.IsOrderLineClosed );
	}
}
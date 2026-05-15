using Modelbouwer.Converters;

namespace Modelbouwer.UnitTests.Converters;

[TestClass]
public class ByteToImageConverterTests
{
	[TestMethod]
	public void GetDecodableImageBytes_ReturnsOriginalBytes_WhenImageStartsAtBeginning()
	{
		byte[] imageData = [ 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0x01 ];

		var result = ByteToImageConverter.GetDecodableImageBytes( imageData );

		Assert.AreSame( imageData, result );
	}

	[TestMethod]
	public void GetDecodableImageBytes_StripsLegacyHeader_WhenImageStartsLaterInBlob()
	{
		byte[] imageData = [ 0x15, 0x1C, 0x02, 0x00, 0xFF, 0xD8, 0xFF, 0xE0, 0x01 ];

		var result = ByteToImageConverter.GetDecodableImageBytes( imageData );

		CollectionAssert.AreEqual( new byte[] { 0xFF, 0xD8, 0xFF, 0xE0, 0x01 }, result );
	}
}

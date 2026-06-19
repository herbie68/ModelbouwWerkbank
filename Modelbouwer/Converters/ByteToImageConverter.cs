namespace Modelbouwer.Converters;

public class ByteToImageConverter : IValueConverter
{
	public ImageSource? DefaultImage { get; set; }

	public object? Convert( object value, Type targetType, object parameter, CultureInfo culture )
	{
		try
		{
			if ( value == null || !( value is byte [ ] imageData ) || imageData.Length == 0 )
			{
				return DefaultImage;
			}

			imageData = GetDecodableImageBytes( imageData ) ?? imageData;

			BitmapImage image = new();
			using ( MemoryStream stream = new( imageData ) )
			{
				image.BeginInit();
				image.CacheOption = BitmapCacheOption.OnLoad;
				image.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
				image.StreamSource = stream;
				image.EndInit();
				image.Freeze();
			}
			return image;
		}
		catch ( Exception ex )
		{
			Debug.WriteLine( $"Error converting image: {ex.Message}" );
			// Retourneer een standaard of placeholder afbeelding
			return null; // Of een fallback afbeelding
		}
	}

	public object? ConvertBack( object value, Type targetType, object parameter, CultureInfo culture )
	{
		if ( value is BitmapImage bitmapImage )
		{
			using MemoryStream stream = new();
			BitmapEncoder encoder = new PngBitmapEncoder();
			encoder.Frames.Add( BitmapFrame.Create( bitmapImage ) );
			encoder.Save( stream );
			return stream.ToArray();
		}
		return null;
	}

	public static byte [ ]? GetDecodableImageBytes( byte [ ]? imageData )
	{
		if ( imageData == null || imageData.Length == 0 )
			return null;

		var offset = FindImageStartOffset( imageData );
		if ( offset <= 0 )
			return imageData;

		return imageData [ offset.. ];
	}

	private static int FindImageStartOffset( byte [ ] imageData )
	{
		ReadOnlySpan<byte> data = imageData;
		byte[] png = [ 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A ];
		byte[] jpg = [ 0xFF, 0xD8, 0xFF ];
		byte[] bmp = [ 0x42, 0x4D ];
		byte[] gif = [ 0x47, 0x49, 0x46, 0x38 ];
		byte[] tiffLittleEndian = [ 0x49, 0x49, 0x2A, 0x00 ];
		byte[] tiffBigEndian = [ 0x4D, 0x4D, 0x00, 0x2A ];

		int offset = FindPattern( data, png );
		if ( offset >= 0 )
			return offset;

		offset = FindPattern( data, jpg );
		if ( offset >= 0 )
			return offset;

		offset = FindPattern( data, bmp );
		if ( offset >= 0 )
			return offset;

		offset = FindPattern( data, gif );
		if ( offset >= 0 )
			return offset;

		offset = FindPattern( data, tiffLittleEndian );
		if ( offset >= 0 )
			return offset;

		return FindPattern( data, tiffBigEndian );
	}

	private static int FindPattern( ReadOnlySpan<byte> data, ReadOnlySpan<byte> pattern )
	{
		if ( pattern.Length == 0 || data.Length < pattern.Length )
			return -1;

		for ( var index = 0; index <= data.Length - pattern.Length; index++ )
		{
			if ( data.Slice( index, pattern.Length ).SequenceEqual( pattern ) )
				return index;
		}

		return -1;
	}
}
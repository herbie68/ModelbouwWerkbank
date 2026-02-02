namespace Modelbouwer.Controls;

/// <summary>
/// Interaction logic for ImageEditor.xaml
/// </summary>
public partial class ImageEditor : UserControl
{
	public ImageEditor()
	{
		InitializeComponent();
	}

	// Image byte[]
	public static readonly DependencyProperty ImageSourceProperty =
		DependencyProperty.Register(nameof(ImageSource), typeof(byte[]), typeof(ImageEditor), new PropertyMetadata(null));

	public byte [ ] ImageSource
	{
		get => ( byte [ ] ) GetValue( ImageSourceProperty );
		set => SetValue( ImageSourceProperty, value );
	}

	// Rotation angle
	public static readonly DependencyProperty RotationAngleProperty =
		DependencyProperty.Register(nameof(RotationAngle), typeof(double), typeof(ImageEditor), new PropertyMetadata(0.0));

	public double RotationAngle
	{
		get => ( double ) GetValue( RotationAngleProperty );
		set => SetValue( RotationAngleProperty, value );
	}

	// Commands
	public static readonly DependencyProperty AddImageCommandProperty =
		DependencyProperty.Register(nameof(AddImageCommand), typeof(ICommand), typeof(ImageEditor));

	public ICommand AddImageCommand
	{
		get => ( ICommand ) GetValue( AddImageCommandProperty );
		set => SetValue( AddImageCommandProperty, value );
	}

	public static readonly DependencyProperty DeleteImageCommandProperty =
		DependencyProperty.Register(nameof(DeleteImageCommand), typeof(ICommand), typeof(ImageEditor));

	public ICommand DeleteImageCommand
	{
		get => ( ICommand ) GetValue( DeleteImageCommandProperty );
		set => SetValue( DeleteImageCommandProperty, value );
	}

	public static readonly DependencyProperty RotateCommandProperty =
		DependencyProperty.Register(nameof(RotateCommand), typeof(ICommand), typeof(ImageEditor));

	public ICommand RotateCommand
	{
		get => ( ICommand ) GetValue( RotateCommandProperty );
		set => SetValue( RotateCommandProperty, value );
	}

	// Drag/Drop
	private void Image_Drop( object sender, DragEventArgs e )
	{
		if ( e.Data.GetDataPresent( DataFormats.FileDrop ) )
		{
			var files = (string[])e.Data.GetData(DataFormats.FileDrop);
			if ( files.Length > 0 )
			{
				ImageSource = File.ReadAllBytes( files [ 0 ] );
			}
		}
	}
}
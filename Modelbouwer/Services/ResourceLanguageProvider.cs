using Modelbouwer.Resources.Languages;

namespace Modelbouwer.Services
{
	public class ResourceLanguageProvider : ILanguageProvider
	{
		public string GetTranslation( string key )
		{
			// Gebruik reflection om naar Language properties te zoeken
			var property = typeof(Language).GetProperty(key);
			return property?.GetValue( null ) as string ?? key;
		}

		public string GetTranslation( string key, params object [ ] args )
		{
			var format = GetTranslation(key);
			return string.Format( format, args );
		}
	}
}
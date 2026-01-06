namespace Modelbouwer.Services
{
	public interface ILanguageProvider
	{
		string GetTranslation( string key );
		string GetTranslation( string key, params object [ ] args );
	}
}
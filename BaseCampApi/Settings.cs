using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Reflection;

namespace BaseCampApi {

	/// <summary>
	/// Minumum settings for BaseCampApi.
	/// </summary>
	public interface ISettings {

		/// <summary>
		/// Application Name is required by the api
		/// </summary>
		string ApplicationName  { get; }

		/// <summary>
		/// Contact (website or email) is required by the api - they may use this information to get in touch 
		/// if you're doing something wrong (so they can warn you before you're blacklisted) or something 
		/// awesome (so they can congratulate you).
		/// </summary>
		string Contact  { get; }

		/// <summary>
		/// Redirect uri for authorisation. Usually "http://localhost:port/". 
		/// BaseCampApi will listen on the port to pick up the redirect during the authorisation process.
		/// </summary>
		Uri RedirectUri  { get; }

		/// <summary>
		/// Page to redirect the user to after the Oauth process has logged them in.
		/// Leave empty to return PageToSendAfterLogin.
		/// </summary>
		string RedirectAfterLogin  { get; }

		/// <summary>
		/// Page to send to the user to after the Oauth process has logged them in.
		/// Leave empty to show a page that closes itself.
		/// </summary>
		string PageToSendAfterLogin  { get; }

		/// <summary>
		/// As allocated when you registered your application
		/// </summary>
		string ClientId  { get; }

		/// <summary>
		/// As allocated when you registered your application
		/// </summary>
		string ClientSecret  { get; }

		/// <summary>
		/// Authorisation returns this token, which is then used to access the api without having to login
		/// every time.
		/// </summary>
		string AccessToken { get; set; }

		/// <summary>
		/// Authorisation returns this token, which is then used to refresh the access token without having to login
		/// every time.
		/// </summary>
		string RefreshToken  { get; set; }

		/// <summary>
		/// When the AccessToken expires
		/// </summary>
		DateTime TokenExpires  { get; set; }

		/// <summary>
		/// If the access token is due to expire before this time elapses, refresh it
		/// </summary>
		TimeSpan RefreshTokenIfDueToExpireBefore { get; }

		/// <summary>
		/// The Basecamp Company to access
		/// </summary>
		int CompanyId  { get; }

		/// <summary>
		/// Set to greater than zero to log all requests going to Basecamp. 
		/// Larger numbers give more verbose logging.
		/// </summary>
		int LogRequest  { get; }

		/// <summary>
		/// Set greater than zero to log all replies coming from Basecamp. 
		/// Larger numbers give more verbose logging.
		/// </summary>
		int LogResult  { get; }

		/// <summary>
		/// After BaseCampApi has update tokens, save the infomation.
		/// </summary>
		void Save();
	}

	/// <summary>
	/// Default settings object for BaseCampApi.
	/// If you want additional settings for your app, you can derive from this. 
	/// Or, if you already have a settings system, just implement ISettings in it.
	/// If you call Load or Save on the derived object, all its properties and members will be loaded/saved
	/// (subject to JsonIgnore attributes, of course).
	/// </summary>
	public class Settings : ISettings {

		/// <summary>
		/// Application Name is required by the api
		/// </summary>
		public string ApplicationName { get; set; }

		/// <summary>
		/// Contact (website or email) is required by the api - they may use this information to get in touch 
		/// if you're doing something wrong (so they can warn you before you're blacklisted) or something 
		/// awesome (so they can congratulate you).
		/// </summary>
		public string Contact { get; set; }

		/// <summary>
		/// Redirect uri for authorisation. Usually "http://localhost:port/". 
		/// BaseCampApi will listen on the port to pick up the redirect during the authorisation process.
		/// </summary>
		public Uri RedirectUri { get; set; }

		/// <summary>
		/// Page to redirect the user to after the Oauth process has logged them in.
		/// Leave empty to return PageToSendAfterLogin.
		/// </summary>
		public string RedirectAfterLogin { get; set; }

		/// <summary>
		/// Page to send to the user to after the Oauth process has logged them in.
		/// Leave empty to show a page that closes itself.
		/// </summary>
		public string PageToSendAfterLogin { get; set; }

		/// <summary>
		/// As allocated when you registered your application
		/// </summary>
		public string ClientId { get; set; }

		/// <summary>
		/// As allocated when you registered your application
		/// </summary>
		public string ClientSecret { get; set; }

		/// <summary>
		/// Authorisation returns this token, which is then used to access the api without having to login
		/// every time.
		/// </summary>
		public string AccessToken { get; set; }

		/// <summary>
		/// Authorisation returns this token, which is then used to refresh the access token without having to login
		/// every time.
		/// </summary>
		public string RefreshToken { get; set; }

		/// <summary>
		/// When the AccessToken expires
		/// </summary>
		public DateTime TokenExpires { get; set; }

		/// <summary>
		/// If the access token is due to expire before this time elapses, refresh it
		/// </summary>
		public TimeSpan RefreshTokenIfDueToExpireBefore { get; set; } = new TimeSpan(1, 0, 0, 0);

		/// <summary>
		/// The Basecamp Company to access
		/// </summary>
		public int CompanyId { get; set; }

		[JsonIgnore]
		public bool LoggedIn {
			get { return !string.IsNullOrEmpty(AccessToken) && TokenExpires > DateTime.Now; }
		}

		/// <summary>
		/// Set to greater than zero to log all requests going to Basecamp. 
		/// Larger numbers give more verbose logging.
		/// </summary>
		public int LogRequest { get; set; }

		/// <summary>
		/// Set greater than zero to log all replies coming from Basecamp. 
		/// Larger numbers give more verbose logging.
		/// </summary>
		public int LogResult { get; set; }

		[JsonIgnore]
		public string Filename;

		public static Settings Load() {
			string dataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "BaseCampApi");
			Directory.CreateDirectory(dataPath);
			string filename = Path.Combine(dataPath, "Settings.json");
			Settings settings = new Settings();
			settings.Load(filename);
			return settings;
		}

		public virtual void Load(string filename) {
			if(File.Exists(filename))
				using (StreamReader s = new StreamReader(filename))
					JsonConvert.PopulateObject(s.ReadToEnd(), this);
			this.Filename = filename;
		}

		public virtual void Save() {
			Directory.CreateDirectory(Path.GetDirectoryName(Filename));
			using (StreamWriter w = new StreamWriter(Filename))
				w.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(this, Newtonsoft.Json.Formatting.Indented));
		}

		public void SetDefaults(string clientId, string clientSecret, string applicationName, string contact, string redirectUri, int companyId) {
			bool changed = false;
			if (string.IsNullOrEmpty(ClientId)) {
				ClientId = clientId;
				changed = true;
			}
			if (string.IsNullOrEmpty(ClientSecret)) {
				ClientSecret = clientSecret;
				changed = true;
			}
			if (string.IsNullOrEmpty(ApplicationName)) {
				ApplicationName = applicationName;
				changed = true;
			}
			if (string.IsNullOrEmpty(Contact)) {
				Contact = contact;
				changed = true;
			}
			if (RedirectUri == null) {
				RedirectUri = new Uri(redirectUri);
				changed = true;
			}
			if(CompanyId == 0) {
				CompanyId = companyId;
				changed = true;
			}
			if (changed)
				Save();
		}

		public virtual List<string> Validate() {
			List<string> errors = new List<string>();
			if (string.IsNullOrEmpty(ClientId)) {
				errors.Add("ClientId missing");
			}
			if (string.IsNullOrEmpty(ClientSecret)) {
				errors.Add("ClientSecret missing");
			}
			if (string.IsNullOrEmpty(ApplicationName)) {
				errors.Add("ApplicationName missing");
			}
			if (string.IsNullOrEmpty(Contact)) {
				errors.Add("Contact missing");
			}
			if (RedirectUri == null) {
				errors.Add("RedirectUri missing");
			}
			if (CompanyId == 0) {
				errors.Add("CompanyId missing");
			}
			return errors;
		}
	}
}

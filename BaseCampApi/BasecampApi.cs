using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Threading;
using System.Net.Sockets;
using System.Net;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Runtime.InteropServices;
using System.Linq;
using System.IO;
#if NETCORE
using Microsoft.AspNetCore.StaticFiles;
#else
using System.Web;
#endif

namespace BaseCampApi {
	/// <summary>
	/// Communicates with the BaseCamp Api
	/// </summary>
	public class Api : IDisposable {
		const string AuthUri = " https://launchpad.37signals.com/authorization/new";
		const string AuthUri2 = " https://launchpad.37signals.com/authorization/token";
		const string BaseUri = "https://3.basecampapi.com/";
		HttpClient _client;

		public Api(ISettings settings) {
			Settings = settings;
			_client = new HttpClient(new HttpClientHandler() { AllowAutoRedirect = false });
			_client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
			_client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/html"));
			_client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*"));
			_client.DefaultRequestHeaders.Add("User-Agent", Settings.ApplicationName + " (" + Settings.Contact + ")");
			if (Settings.RedirectUri.Port > 0)
				RedirectPort = Settings.RedirectUri.Port;
		}

		public void Dispose() {
			if (_client != null) {
				_client.Dispose();
				_client = null;
			}
		}

		/// <summary>
		/// The Settings object to use for this Api instance.
		/// Will be Saved every time the AccessToken changes or is refreshed.
		/// </summary>
		public ISettings Settings;

		/// <summary>
		/// Port to listen on for the redirect after a login. Set in constructor from <see cref="Settings.RedirectUrl" />,
		/// but can be overridden if you have port redictection in a router or something.
		/// </summary>
		public int RedirectPort = 80;

		/// <summary>
		/// Action to open a web browser for the user to login. Just uses operating system browser if not set.
		/// </summary>
		public Action<string> OpenBrowser = openBrowser;

		/// <summary>
		/// Function to wait for a connection to the RedirectUri, extract the code from the Get parameters and return it.
		/// Default version listens for a request and parses the paraneter.
		/// </summary>
		public Func<Api, Task<string>> WaitForRedirect = waitForRedirect;

		/// <summary>
		/// Log messages will be passed to this handler
		/// </summary>
		public delegate void LogHandler(string message);

		/// <summary>
		/// Event receives all log messages (to, for example, save them to file or display them to the user)
		/// </summary>
		public event LogHandler LogMessage;

		/// <summary>
		/// Post to the Api, returning an object
		/// </summary>
		/// <typeparam name="T">The object type expected</typeparam>
		/// <param name="application">The part of the url after the company</param>
		/// <param name="getParameters">Any get parameters to pass (in an object or JObject)</param>
		/// <param name="postParameters">Any post parameters to pass (in an object or JObject)</param>
		public async Task<T> PostAsync<T>(string application, object getParameters = null, object postParameters = null) where T : new() {
			JObject j = await PostAsync(application, getParameters, postParameters);
			return convertTo<T>(j);
		}

		/// <summary>
		/// Post to the Api, returning a JObject
		/// </summary>
		/// <param name="application">The part of the url after the company</param>
		/// <param name="getParameters">Any get parameters to pass (in an object or JObject)</param>
		/// <param name="postParameters">Any post parameters to pass (in an object or JObject)</param>
		public async Task<JObject> PostAsync(string application, object getParameters = null, object postParameters = null) {
			string uri = makeUri(application);
			uri = AddGetParams(uri, getParameters);
			return await SendMessageAsync(HttpMethod.Post, uri, postParameters);
		}

		/// <summary>
		/// Get from  the Api, returning an object
		/// </summary>
		/// <typeparam name="T">The object type expected</typeparam>
		/// <param name="application">The part of the url after the company</param>
		/// <param name="getParameters">Any get parameters to pass (in an object or JObject)</param>
		public async Task<T> GetAsync<T>(string application, object getParameters = null) where T : new() {
			JObject j = await GetAsync(application, getParameters);
			return convertTo<T>(j);
		}

		/// <summary>
		/// Get from  the Api, returning a Jobject
		/// </summary>
		/// <param name="application">The part of the url after the company</param>
		/// <param name="getParameters">Any get parameters to pass (in an object or JObject)</param>
		public async Task<JObject> GetAsync(string application, object getParameters = null) {
			string uri = makeUri(application);
			uri = AddGetParams(uri, getParameters);
			return await SendMessageAsync(HttpMethod.Get, uri);
		}

		/// <summary>
		/// Put to  the Api, returning an object
		/// </summary>
		/// <typeparam name="T">The object type expected</typeparam>
		/// <param name="application">The part of the url after the company</param>
		/// <param name="getParameters">Any get parameters to pass (in an object or JObject)</param>
		/// <param name="postParameters">Any post parameters to pass (in an object or JObject)</param>
		public async Task<T> PutAsync<T>(string application, object getParameters = null, object postParameters = null) where T : new() {
			JObject j = await PutAsync(application, getParameters, postParameters);
			return convertTo<T>(j);
		}

		/// <summary>
		/// Put to  the Api, returning a JObject
		/// </summary>
		/// <param name="application">The part of the url after the company</param>
		/// <param name="getParameters">Any get parameters to pass (in an object or JObject)</param>
		/// <param name="postParameters">Any post parameters to pass (in an object or JObject)</param>
		public async Task<JObject> PutAsync(string application, object getParameters = null, object postParameters = null) {
			string uri = makeUri(application);
			uri = AddGetParams(uri, getParameters);
			return await SendMessageAsync(HttpMethod.Put, uri, postParameters);
		}

		/// <summary>
		/// Delete to  the Api, returning an object
		/// </summary>
		/// <typeparam name="T">The object type expected</typeparam>
		/// <param name="application">The part of the url after the company</param>
		/// <param name="getParameters">Any get parameters to pass (in an object or JObject)</param>
		public async Task<T> DeleteAsync<T>(string application, object getParameters = null) where T : new() {
			JObject j = await DeleteAsync(application, getParameters);
			return convertTo<T>(j);
		}

		/// <summary>
		/// Delete to  the Api, returning a JObject
		/// </summary>
		/// <typeparam name="T">The object type expected</typeparam>
		/// <param name="application">The part of the url after the company</param>
		/// <param name="getParameters">Any get parameters to pass (in an object or JObject)</param>
		public async Task<JObject> DeleteAsync(string application, object getParameters = null) {
			string uri = makeUri(application);
			uri = AddGetParams(uri, getParameters);
			return await SendMessageAsync(HttpMethod.Delete, uri);
		}

		/// <summary>
		/// Log in - pops up a web browser (using <see cref="OpenBrowser"/>) to allow the user to log in to BaseCamp.
		/// Calls <see cref="WaitForRedirect"/> to collect the redirected Get and parse the code out of it.
		/// Then exchanges the code for a Token, and updates Settings with the Token.
		/// </summary>
		public async Task LoginAsync() {
			try {
				OpenBrowser(AddGetParams(AuthUri, new {
					type = "web_server",    // Or user_agent
					client_id = Settings.ClientId,
					redirect_uri = Settings.RedirectUri
				}));
				string code = await WaitForRedirect(this);
				var result = await SendMessageAsync(HttpMethod.Post, AddGetParams(AuthUri2, new {
					type = "web_server",
					client_id = Settings.ClientId,
					redirect_uri = Settings.RedirectUri,
					client_secret = Settings.ClientSecret,
					code
				}));
				Log(result.ToString());
				Token token = result.ToObject<Token>();
				if (!string.IsNullOrEmpty(token.MetaData.error))
					throw new ApiException(token.MetaData.error, result);
				updateToken(token);
			} catch (Exception ex) {
				Log(ex.ToString());
			}
		}

		/// <summary>
		/// If the Token has nearly expired, refresh it.
		/// </summary>
		public async Task RefreshAsync() {
			var result = await SendMessageAsync(HttpMethod.Post, AuthUri2, new {
				type = "refresh",
				client_id = Settings.ClientId,
				client_secret = Settings.ClientSecret,
				refresh_token = Settings.RefreshToken,
				format = "json"
			});
			Log(result.ToString());
			Token token = result.ToObject<Token>();
			if (!string.IsNullOrEmpty(token.MetaData.error))
				throw new ApiException(token.MetaData.error, result);
			updateToken(token);
		}

		/// <summary>
		/// Login or Refresh if the token is missing or due to expire.
		/// </summary>
		/// <returns></returns>
		public async Task LoginOrRefreshIfRequiredAsync() {
			if (string.IsNullOrEmpty(Settings.AccessToken) || Settings.TokenExpires <= DateTime.Now)
				await LoginAsync();
			else if (Settings.TokenExpires <= DateTime.Now + Settings.RefreshTokenIfDueToExpireBefore)
				await RefreshAsync();
		}

		/// <summary>
		/// Get the Authorisation object for the logged in user (logging in first, if necessary)
		/// </summary>
		public async Task<Authorization> GetAuthorization() {
			JObject j = await SendMessageAsync(HttpMethod.Get, "https://launchpad.37signals.com/authorization.json");
			return j.ToObject<Authorization>();
		}

		/// <summary>
		/// Log a message to trace and, if present, to the LogMessage event handlers
		/// </summary>
		public void  Log(string message) {
			System.Diagnostics.Trace.WriteLine(message);
			LogMessage?.Invoke(message);
		}

		/// <summary>
		/// Log a message to trace and, if present, to the LogMessage event handlers.
		/// Works like TextWriter.WriteLine or String.Format
		/// </summary>
		public void Log(string format, params object [] args) {
			try {
				Log(string.Format(format, args));
			} catch(Exception ex) {
				Log("Exception logging " + format + "\n" + ex);
			}
		}

		/// <summary>
		/// Combine a list of arguments into a string, with "/" between them
		/// </summary>
		public static string Combine(params object[] args) {
			return string.Join("/", args.Select(a => a.ToString()));
		}

		/// <summary>
		/// Add Get Paranmeters to a uri
		/// </summary>
		/// <param name="parameters">Object whose properties are the argumentgs - e.g. new {
		/// 		type = "web_server",
		/// 		client_id = Settings.ClientId,
		/// 		redirect_uri = Settings.RedirectUri
		/// 	}</param>
		/// <returns>uri?arg1=value1&amp;arg2=value2...</returns>
		public static string AddGetParams(string uri, object parameters = null) {
			if (parameters != null) {
				JObject j = parameters.ToJObject();
				List<string> p = new List<string>();
				foreach (var v in j) {
					if(!v.Value.IsNullOrEmpty())
						p.Add(v.Key + "=" + Uri.EscapeUriString(v.Value.ToString()));
				}
				uri += "?" + string.Join("&", p);
			}
			return uri;
		}

#if NETCORE
		/// <summary>
		/// Object for converting file names to mime types
		/// </summary>
		public static FileExtensionContentTypeProvider MimeTypeTranslator = new FileExtensionContentTypeProvider();
#endif

		/// <summary>
		/// General API message sending.
		/// </summary>
		/// <param name="method">Get/Post/etc.</param>
		/// <param name="uri">The full Uri you want to call (including any get parameters)</param>
		/// <param name="postParameters">Post parameters as an :-
		/// object (converted to Json, MIME type application/json)
		/// JObject (converted to Json, MIME type application/json)
		/// string (sent as is, MIME type text/plain)
		/// FielStream (sent as stream, with Attachment file name, Content-Length, and MIME type according to file extension)
		/// </param>
		/// <returns>The result as a JObject, with MetaData filled in.</returns>
		public async Task<JObject> SendMessageAsync(HttpMethod method, string uri, object postParameters = null) {
			await LoginOrRefreshIfRequiredAsync();
			JObject j = null;
			using (HttpResponseMessage result = await sendMessageAsync(method, uri, postParameters)) {
				string data = await result.Content.ReadAsStringAsync();
				if (data.StartsWith("{")) {
					j = JObject.Parse(data);
				} else if (data.StartsWith("[")) {
					j = new JObject();
					j["List"] = JArray.Parse(data);
				} else {
					j = new JObject();
					if (!string.IsNullOrEmpty(data))
						j["content"] = data;
				}
				JObject metadata;
				if (!result.IsSuccessStatusCode) {
					metadata = j;
					j = new JObject();
				} else {
					metadata = new JObject();
				}
				metadata["Uri"] = uri;
				IEnumerable<string> values;
				if (result.Headers.TryGetValues("Link", out values)) metadata["Link"] = values.FirstOrDefault();
				if (result.Headers.TryGetValues("X-Total-Count", out values)) metadata["TotalCount"] = int.Parse(values.FirstOrDefault());
				if (result.Headers.TryGetValues("ETag", out values)) metadata["ETag"] = values.FirstOrDefault();
				if (result.Headers.TryGetValues("Last-Modified", out values)) metadata["Modified"] = values.FirstOrDefault();
				j["MetaData"] = metadata;
				if (Settings.LogResult > 0)
					Log("Received Data -> " + j);
				if (!result.IsSuccessStatusCode)
					throw new ApiException(result.ReasonPhrase, j);
			}
			return j;
		}

		/// <summary>
		/// Send a message and get the result.
		/// Deal with rate limiting return values and redirects.
		/// </summary>
		/// <param name="method">Get/Post/etc.</param>
		/// <param name="uri">The full Uri you want to call (including any get parameters)</param>
		/// <param name="postParameters">Post parameters as an object or JObject</param>
		async Task<HttpResponseMessage> sendMessageAsync(HttpMethod method, string uri, object postParameters = null) {
			for (; ; ) {
				string content = null;
				string ext = Path.GetExtension(uri);
				if (string.IsNullOrEmpty(ext))
					uri += ".json";
				using (var message = new HttpRequestMessage(method, uri)) {
					if (!string.IsNullOrEmpty(Settings.AccessToken) && Settings.TokenExpires > DateTime.Now)
						message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", Settings.AccessToken);
					message.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
					message.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/html"));
					message.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*"));
					message.Headers.Add("User-Agent", Settings.ApplicationName + " (" + Settings.Contact + ")");
					if (postParameters != null) {
						if (postParameters is string) {
							content = postParameters.ToString();
							message.Content = new StringContent(content, Encoding.UTF8, "text/plain");
						} else if (postParameters is FileStream) {
							FileStream f = postParameters as FileStream;
							content = Path.GetFileName(f.Name);
							f.Position = 0;
							message.Content = new StreamContent(f);
#if NETCORE
							string contentType;
							if (!MimeTypeTranslator.TryGetContentType(content, out contentType))
								contentType = "application/octet-stream";
#else
						string contentType = MimeMapping.GetMimeMapping(content);
#endif
							message.Content.Headers.ContentType = new MediaTypeHeaderValue(contentType);
							message.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment") {
								FileName = content
							};
							message.Content.Headers.ContentLength = f.Length;
							content = "File: " + content;
						} else {
							content = postParameters.ToJson();
							message.Content = new StringContent(content, Encoding.UTF8, "application/json");
						}
					}
					HttpResponseMessage result;
					int backoff = 500;
					int delay;
					if (Settings.LogRequest > 0)
						Log("Sent -> {0}", (Settings.LogRequest > 1 ? message.ToString() : message.RequestUri.ToString()) + ":" + content);
					result = await _client.SendAsync(message);
					if (Settings.LogResult > 1 || !result.IsSuccessStatusCode)
						Log("Received -> {0}", result);
					switch (result.StatusCode) {
						case HttpStatusCode.Found:      // Redirect
							uri = result.Headers.Location.AbsoluteUri;
							delay = 1;
							break;
#if NETCORE
						case HttpStatusCode.TooManyRequests:
#else
						case (HttpStatusCode)429:       // TooManyRequests
#endif
							delay = 5000;
							break;
						case HttpStatusCode.BadGateway:
						case HttpStatusCode.ServiceUnavailable:
						case HttpStatusCode.GatewayTimeout:
							backoff *= 2;
							delay = backoff;
							if (delay > 16000)
								return result;
							break;
						default:
							return result;
					}
					result.Dispose();
					Thread.Sleep(delay);
				}
			}
		}

		/// <summary>
		/// Convert a JObject to an Object.
		/// If it is an ApiEntry, and error is not empty, throw an exception.
		/// </summary>
		/// <typeparam name="T">Object to convert to</typeparam>
		static T convertTo<T>(JObject j) where T:new() {
				T t = j.ToObject<T>();
				ApiEntry e = t as ApiEntry;
				if (e != null && e.Error)
					throw new ApiException(e.MetaData.error, j);
				return t;
		}

		/// <summary>
		/// Default <see cref="OpenBrowser"/>
		/// </summary>
		static void openBrowser(string url) {
#if NETCORE
			if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
				Process.Start(new ProcessStartInfo("cmd", $"/c start {url.Replace("&", "^&")}"));
			} else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) {
				Process.Start("xdg-open", "'" + url + "'");
			} else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) {
				Process.Start("open", "'" + url + "'");
			} else {
				throw new ApplicationException("Unknown OS platform");
			}
#else
			int plat = (int)Environment.OSVersion.Platform;
			if (plat == 4 || plat == 128) {
				Process.Start("xdg-open", url);
			} else {
				Process.Start(url);
			}
#endif
		}

		/// <summary>
		/// Default <see cref="WaitForRedirect"/>
		/// </summary>
		static async Task<string> waitForRedirect(Api api) {
			return await Task.Run(delegate () {
				IPHostEntry ipHost = Dns.GetHostEntry(api.Settings.RedirectUri.Host);
				IPAddress ipAddr = ipHost.AddressList[0];
				IPEndPoint localEndPoint = new IPEndPoint(ipAddr, api.RedirectPort);

				using (Socket listener = new Socket(ipAddr.AddressFamily,
					 SocketType.Stream, ProtocolType.Tcp)) {
					listener.Bind(localEndPoint);
					listener.Listen(10);
					for (; ; ) {
						api.Log("Waiting connection on port " + api.RedirectPort);
						// Suspend while waiting for 
						// incoming connection Using  
						// Accept() method the server  
						// will accept connection of client 
						using (Socket clientSocket = listener.Accept()) {

							// Data buffer 
							byte[] bytes = new Byte[1024];
							string data = null;

							while (true) {
								int numByte = clientSocket.Receive(bytes);
								if (numByte <= 0)
									break;
								data += Encoding.ASCII.GetString(bytes, 0, numByte);
								if (data.Replace("\r", "").IndexOf("\n\n") > -1)
									break;
							}
							if (api.Settings.LogResult > 1) {
								api.Log("Text received -> " + data);
							}

							string page = api.Settings.PageToSendAfterLogin;
							if (string.IsNullOrEmpty(page))
								page = $@"<html>
<body>
<div>Thankyou for giving access to your Basecamp account to {api.Settings.ApplicationName}.</div>
<div>Please close this window now.</div>
</body>
</html>";
							string headers = $@"HTTP/1.1 {(string.IsNullOrEmpty(api.Settings.RedirectAfterLogin) ? 200 : 303)} OK
Date: Fri, 31 May 2019 18:23:23 GMT
Server: Basecamp API for {api.Settings.ApplicationName}
Content-Type: text/html; charset=UTF-8
";
							if (!string.IsNullOrEmpty(api.Settings.RedirectAfterLogin))
								headers += "Location: " + api.Settings.RedirectAfterLogin;
							clientSocket.Send(Encoding.UTF8.GetBytes(headers + "\r\n\r\n" + page));

							// Close client Socket using the 
							// Close() method. After closing, 
							// we can use the closed Socket  
							// for a new Client Connection 
							clientSocket.Shutdown(SocketShutdown.Both);
							clientSocket.Close();
							Match m = Regex.Match(data.Split('\n')[0], "code=([^& ]+)");

							if (m.Success)
								return m.Groups[1].Value;
						}
					}
				}
			});
		}

		static Regex _http = new Regex("^https?://");

		/// <summary>
		/// Make the standard Uri (put BaseUri and CompanyId on the front)
		/// </summary>
		/// <param name="application">The remainder of the Uri</param>
		string makeUri(string application) {
			return _http.IsMatch(application) ? application : BaseUri + Settings.CompanyId + "/" + application + ".json";
		}

		/// <summary>
		/// Update Settings with info from the Token
		/// </summary>
		/// <param name="token">As returned by the auth call</param>
		void updateToken(Token token) {
			Settings.AccessToken = token.access_token;
			if(!string.IsNullOrEmpty(token.refresh_token))
				Settings.RefreshToken = token.refresh_token;
			try {
				Settings.TokenExpires = DateTime.Now.AddSeconds(token.expires_in);
			} catch {
				Settings.TokenExpires = DateTime.Now.AddDays(1);
			}
			Settings.Save();
		}

	}

	/// <summary>
	/// Exception to hold more information when an API call fails
	/// </summary>
	public class ApiException : ApplicationException {
		public ApiException(string message, JObject result) : base(message) {
			Result = result;
		}
		public JObject Result { get; private set; }
		public override string ToString() {
			return base.ToString() + "\r\nResult = " + Result;
		}
	}
}

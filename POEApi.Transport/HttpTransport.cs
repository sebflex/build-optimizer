﻿using System;
using System.IO;
using System.Net;
using System.Text;
using POEApi.Infrastructure;
using System.Security;
using POEApi.Infrastructure.Events;
using System.Text.RegularExpressions;

namespace POEApi.Transport
{
    public class HttpTransport : ITransport
    {
        private string email;
        private CookieContainer credentialCookies;

        private bool useProxy = false;
        private string proxyUser;
        private string proxyPassword;
        private string proxyDomain;
        
        private enum HttpMethod { GET, POST }

        private const string loginURL = @"https://www.pathofexile.com/login";
        private const string characterURL = @"http://www.pathofexile.com/character-window/get-characters";
        private const string stashURL = @"http://www.pathofexile.com/character-window/get-stash-items?league={0}&tabs=1&tabIndex={1}";
        private const string inventoryURL = @"http://www.pathofexile.com/character-window/get-items?character={0}";
        private const string passiveSkillsURL = @"http://www.pathofexile.com/character-window/get-passive-skills?character={0}";
        private const string hashRegEx = "name=\\\"hash\\\" value=\\\"(?<hash>[a-zA-Z0-9]{1,})\\\"";

        public event ThottledEventHandler Throttled;

        public HttpTransport(string login)
        {
            credentialCookies = new CookieContainer();
            this.email = login;
            RequestThrottle.Instance.Throttled += new ThottledEventHandler(instance_Throttled);
        }

        public HttpTransport(string login, string proxyUser, string proxyPassword, string proxyDomain)
            : this(login)
        {
            this.proxyUser = proxyUser;
            this.proxyPassword = proxyPassword;
            this.proxyDomain = proxyDomain;
            this.useProxy = true;
        }

        private void instance_Throttled(object sender, ThottledEventArgs e)
        {
            if (Throttled != null)
                Throttled(this, e);
        }

        public bool Authenticate(string email, SecureString password, bool useSessionID)
        {
            if (useSessionID)
            {
                credentialCookies.Add(new System.Net.Cookie("PHPSESSID", password.UnWrap(), "/", "www.pathofexile.com"));
                HttpWebRequest confirmAuth = getHttpRequest(HttpMethod.GET, loginURL);
                HttpWebResponse confirmAuthResponse = (HttpWebResponse)confirmAuth.GetResponse();

                if (confirmAuthResponse.ResponseUri.ToString() == loginURL)
                    throw new LogonFailedException();
                return true;
            }

            HttpWebRequest getHash = getHttpRequest(HttpMethod.GET, loginURL);
            HttpWebResponse hashResponse = (HttpWebResponse)getHash.GetResponse();
            string loginResponse = Encoding.Default.GetString(getMemoryStreamFromResponse(hashResponse).ToArray());
            string hashValue = Regex.Match(loginResponse, hashRegEx).Groups["hash"].Value;

            HttpWebRequest request = getHttpRequest(HttpMethod.POST, loginURL);
            request.AllowAutoRedirect = false;

            StringBuilder data = new StringBuilder();
            data.Append("login_email=" + Uri.EscapeDataString(email));
            data.Append("&login_password=" + Uri.EscapeDataString(password.UnWrap()));
            data.Append("&hash=" + hashValue);

            byte[] byteData = UTF8Encoding.UTF8.GetBytes(data.ToString());

            request.ContentLength = byteData.Length;

            Stream postStream = request.GetRequestStream();
            postStream.Write(byteData, 0, byteData.Length);

            HttpWebResponse response;
            response = (HttpWebResponse)request.GetResponse();

            //If we didn't get a redirect, your gonna have a bad time.
            if (response.StatusCode != HttpStatusCode.Found)
                throw new LogonFailedException(this.email);

            return true;
        }

        private HttpWebRequest getHttpRequest(HttpMethod method, string url)
        {
            HttpWebRequest request = (HttpWebRequest)RequestThrottle.Instance.Create(url);
            
            request.CookieContainer = credentialCookies;
            request.UserAgent = "User-Agent: Mozilla/4.0 (compatible; MSIE 8.0; Windows NT 6.1; WOW64; Trident/4.0; SLCC2; .NET CLR 2.0.50727; .NET CLR 3.5.30729; .NET CLR 3.0.30729; Media Center PC 6.0; InfoPath.3; .NET4.0C; .NET4.0E; .NET CLR 1.1.4322)";           
            request.Method = method.ToString();
            if (useProxy)
                request.Proxy = processProxySettings();

            request.ContentType = "application/x-www-form-urlencoded";

            return request;
        }

        public WebProxy processProxySettings()
        {
            System.Net.WebProxy proxy = System.Net.WebProxy.GetDefaultProxy();
            System.Net.NetworkCredential credentials = new System.Net.NetworkCredential(proxyUser, proxyPassword, proxyDomain);
            proxy.Credentials = credentials;

            return proxy;
        }

        public Stream GetStash(int index, string league, bool refresh)
        {
            HttpWebRequest request = getHttpRequest(HttpMethod.GET, string.Format(stashURL, league, index));
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            return getMemoryStreamFromResponse(response);
        }

        public Stream GetStash(int index, string league)
        {
            return GetStash(index, league, false);
        }

        public Stream GetCharacters()
        {
            HttpWebRequest request = getHttpRequest(HttpMethod.GET, characterURL);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            return getMemoryStreamFromResponse(response);
        }

        public Stream GetImage(string url)
        {
            WebClient client = new WebClient();
            client.Proxy = processProxySettings();
            return new MemoryStream(client.DownloadData(url));
        }

        public Stream GetInventory(string characterName)
        {
            HttpWebRequest request = getHttpRequest(HttpMethod.GET, string.Format(inventoryURL, characterName));
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            return getMemoryStreamFromResponse(response);
        }

        public Stream GetPassiveSkills(string characterName)
        {
            HttpWebRequest request = getHttpRequest(HttpMethod.GET, string.Format(passiveSkillsURL, characterName));
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            return getMemoryStreamFromResponse(response);
        }

        private MemoryStream getMemoryStreamFromResponse(HttpWebResponse response)
        {
            StreamReader reader = new StreamReader(response.GetResponseStream());
            byte[] buffer = reader.ReadAllBytes();
            RequestThrottle.Instance.Complete();

            return new MemoryStream(buffer);
        }
    }
}

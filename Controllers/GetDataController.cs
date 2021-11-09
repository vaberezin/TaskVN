using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TaskVN.Models;
using System.Net.Http;
using System.Text.Json;
using System.Text;
using System.Net;
using System.IO;

namespace TaskVN.Controllers
{
    public class GetDataController : Controller
    {
        //Create authcookie container
        CookieContainer _authCookie;
        string _appUrl;// = @"https://interview.mfi-ap.asia";
        string _authServiceUrl;// = @"https://interview.mfi-ap.asia/ServiceModel/AuthService.svc/Login";
        string _userName;// = "interview";
        string _userPassword;// = "AAaaa11!";
        private readonly ILogger<GetDataController> _logger;

        public GetDataController(ILogger<GetDataController> logger)
        {
            _logger = logger;
            //set authentication
            _appUrl = @"https://interview.mfi-ap.asia";
            _authServiceUrl = @"https://interview.mfi-ap.asia/ServiceModel/AuthService.svc/Login";
            _userName = "interview";
            _userPassword = "AAaaa11!";
            TryLogin(_userName, _userPassword, _authServiceUrl); //bad idea
        }

        //Getting authentication cookie
        [HttpGet]
        public IActionResult Mobile() //Getting Asp.NET View page to enter data to the form.
        {
            return View();
        }

        [HttpPost]
        public IActionResult Mobile(string phoneNumber) //get number, use it to retrieve JSON, then return it to View, then display it in the div.
        {
            // 1st - get JsonData 
            JsonResult JsonData = GetJsonDataFromAPI(new PhoneNumber {mobile = phoneNumber}, _appUrl).Result;            
            // 2nd - post it to view and handle it there in the view.
            return View(JsonData);
        }

        //Retrieve JSON from API
        public async Task<JsonResult>  GetJsonDataFromAPI(PhoneNumber pn, string _appUrl){
            
            HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Post, $"{_appUrl}/0/rest/InfintoPortalService/GetClientInfo");
            HttpClientHandler clientHandler = new HttpClientHandler();
            
            clientHandler.CookieContainer = _authCookie;            
            clientHandler.UseCookies = true;            

            using (HttpClient client = new HttpClient(clientHandler))
            {
                client.DefaultRequestHeaders.Add("Accept", "application/json");                     
                var convertedObject = JsonSerializer.Serialize(pn, null);                 
                requestMessage.Content = new StringContent(JsonSerializer.Serialize(pn, null),Encoding.UTF8,"application/json"); //body
                //create model of phoneNumber with server side validation, add client-side validation, get json from response
                 
                HttpResponseMessage responseMessage = await client.SendAsync(requestMessage); // Getting 403 Here
                responseMessage.EnsureSuccessStatusCode();
                String stringContent = await responseMessage.Content.ReadAsStringAsync(); // may be not needed

                JsonResult JsonData = new JsonResult(stringContent); //check this

                return JsonData;
            }
        }

        
        // Try to login and set cookie to enable Requests to external API
        public void TryLogin(string _userName, string _userPassword, string _authServiceUrl)
        {  //returns response with tokens/keys

            var authData = @"{
                ""UserName"":""" + _userName + @""",
                ""UserPassword"":""" + _userPassword + @"""
            }";
            var request = CreateRequest(_authServiceUrl, authData);
            _authCookie = new CookieContainer();
            
            request.CookieContainer = _authCookie;
            // Upon successful authentication, we save authentication cookies for
            // further use in requests to Creatio. In case of failure
            // authentication application console displays a message about the reason
            // of the mistake.
            using (var response = (HttpWebResponse)request.GetResponse())
            {
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    using (var reader = new StreamReader(response.GetResponseStream()))
                    {
                        var responseMessage = reader.ReadToEnd();
                        Console.WriteLine(responseMessage);
                        if (responseMessage.Contains("\"Code\":1"))
                        {
                            throw new UnauthorizedAccessException($"Unauthorized {_userName} for {_appUrl}");
                        }
                    }                
                }
            }
        }
                
        private HttpWebRequest CreateRequest(string url, string requestData = null)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.ContentType = "application/json";
            request.Method = "POST";
            request.KeepAlive = true;
            if (!string.IsNullOrEmpty(requestData))
            {
                using (var requestStream = request.GetRequestStream())
                {
                    using (var writer = new StreamWriter(requestStream))
                    {
                        writer.Write(requestData);
                    }
                }
            }
            return request;
        } 
    }    
}
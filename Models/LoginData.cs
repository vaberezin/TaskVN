namespace TaskVN.Models
{
    public class LoginData
    {
        public LoginData(string _userName, string _password)
        {
            this.UserName = _userName;
            this.UserPassword = _password;
        }
        public LoginData()
        {                  
        }
        public string UserName { get; set; }
        public string UserPassword { get; set; }
    }
}
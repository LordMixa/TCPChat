namespace TCPChat
{
    public class User
    {
        public string UserName { get; init; }
        public string Password { get; init; }
        public User(string _username, string _password) 
        {
            UserName = _username;
            Password = _password;
        }
    }
}

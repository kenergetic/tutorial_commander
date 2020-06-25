namespace Commander.Auth
{
    public interface IJwtAuthenticationManager
    {
        string Authenticate(string username, string password);
    }
}
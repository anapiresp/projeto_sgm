namespace SeaRise.Interfaces
{
    public interface IUser
    {
        Guid Id { get; set; }
        public string Username { get; set; }
        string Email { get; set; }
        string Password { get; set; }
    }
}
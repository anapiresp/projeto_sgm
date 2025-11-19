namespace SeaRise.Interfaces
{
    public interface IUser
    {
        Guid Id { get; set; }
        public string Name { get; set; }
        string Email { get; set; }
        string Password { get; set; }
    }
}
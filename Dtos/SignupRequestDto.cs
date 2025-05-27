namespace talk2me_dotnet_api.Dtos;

public class SignupRequestDto
{
    public string FullName { get; set; } = null!;

    public string Username { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Password { get; set; } = null!;
}

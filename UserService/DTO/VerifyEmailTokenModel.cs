namespace UserService.DTO
{
    public class VerifyEmailTokenModel: LoginModel
    {
        public Guid VerifyEmailToken { get; set; }
    }
}

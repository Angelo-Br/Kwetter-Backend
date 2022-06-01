using UserService.DTO;

namespace UserService.Helpers
{
    public class UserServiceHelper
    {
        public UsernameUpdatedDTO UserToUsernameUpdatedDTO(int id, string newUsername)
        {
            if (id == default || newUsername == default)
            {
                throw new ArgumentNullException(nameof(newUsername));
            }

            return new UsernameUpdatedDTO()
            {
                Id = id,
                Username = newUsername,
            };
        }
    }
}

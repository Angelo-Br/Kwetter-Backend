﻿using UserService.DTO;
using UserService.Models;

namespace UserService.Helpers
{
    public class UserServiceHelper
    {
        public UsernameUpdatedDTO UserToUsernameUpdatedDTO(int id, string newUsername, string oldUsername)
        {
            if (id == default || newUsername == default)
            {
                throw new ArgumentNullException(nameof(newUsername));
            }

            return new UsernameUpdatedDTO()
            {
                Id = id,
                Username = newUsername,
                oldUsername = oldUsername
            };
        }

        public UserToKweetServiceUserDTO UserToKweetServiceUserDTO(User user)
        {
            if (user == default)
            {
                throw new ArgumentNullException(nameof(user.Id));
            }

            return new UserToKweetServiceUserDTO()
            {
                Id=user.Id,
                Username = user.Username,
            };
        }
    }
}

﻿namespace PGHub.API.DTOs.User
{
    public class UserDTO
    {
        public Guid Id { get; set; }
        public string Email { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }
    }
}

﻿using System.ComponentModel.DataAnnotations;

namespace SavePassword.Core.Entities
{
    public class PassRecord : Entity
    {
        [Required]
        public string URL { get; set; }
        [Required]
        public string Login { get; set; }
        [Required]
        public string Password { get; set; }
        public string Details { get; set; }
    }
}

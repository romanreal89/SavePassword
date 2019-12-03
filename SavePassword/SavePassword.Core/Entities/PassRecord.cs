using System.ComponentModel.DataAnnotations;

namespace SavePassword.Core.Entities
{
    public class PassRecord : Entity
    {
        [Required]
        [RegularExpression(@"^http(s)?://([\w-]+.)+[\w-]+(/[\w- ./?%&=])?$")]
        public string URL { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string Login { get; set; }
        [Required]
        public string Password { get; set; }
        public string Details { get; set; }
        public string Group { get; set; }
    }
}

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PillCat.Models {
	[Table("User")]
	public class User {
		[Key]
		public int Id { get; set; }
		
		public string Username { get; set; }

		public string Email { get; set; }

		public string Password { get; set; }
	}
}

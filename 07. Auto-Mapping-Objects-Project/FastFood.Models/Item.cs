namespace FastFood.Models
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    public class Item
	{
		public int Id { get; set; }

		[StringLength(30, MinimumLength = 3)]
		public string Name { get; set; }

		public int CategoryId { get; set; }

		[Required]
		public Category Category { get; set; }

		public decimal Price { get; set; }

		public ICollection<OrderItem> OrderItems { get; set; }
	}
}
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

namespace RestaurantWithAi.Core.Entities;

[ExcludeFromCodeCoverage]
[DataContract]
public class Dish
{
    [Key]
    [DataMember]
    public Guid Id { get; set; } = Guid.NewGuid();
    
    [DataMember]
    [Required]
    public required string Name { get; set; }
    
    [DataMember]
    [Required]
    public required string Description { get; set; }
    
    [DataMember]
    [Required]
    public decimal Price { get; set; }
    
    [DataMember]
    [Required]
    public required string ImageUrl { get; set; }

    public ICollection<Restaurant> AvailableAtRestaurants { get; set; } = new List<Restaurant>();
}
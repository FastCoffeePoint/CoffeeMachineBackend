﻿using System.ComponentModel.DataAnnotations;

namespace Cmb.Database.Entities;

public class DbCoffeeRecipe : DbEntity
{
    [Key]
    public Guid Id { get; set; }
    
    [MaxLength(128)]
    public string Name { get; set; }

    public List<DbIngredient> Ingredients { get; set; }
}
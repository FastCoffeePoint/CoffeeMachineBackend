﻿using System.ComponentModel.DataAnnotations;

namespace Cmb.Database.Entities;

public class DbIngredient : DbEntity
{
    [Key]
    public Guid Id { get; set; }

    [MaxLength(128)]
    public string Name { get; set; }

    public int Amount { get; set; }

    public List<DbCoffeeRecipe> CoffeeRecipes { get; set; }
}
﻿using CSharpFunctionalExtensions;

namespace Cmb.Application.Sensors;

public interface IIngredientsSensor
{
    Task<Result<int, string>> GetAmount(string sensorId);
}
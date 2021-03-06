﻿using System.Collections.Generic;
using System.Linq;
using API.Errors;
using Core.Interfaces;
using Infrastructure.Data;
using Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace API.Extensions
{
	public static class ApplicationServicesExtensions
	{
		public static IServiceCollection AddApplicationServices(this IServiceCollection services)
		{
			services.AddScoped<ITokenService, TokenService>();
			services.AddScoped<IGenericRepository, GenericRepository>();
			services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
			services.AddScoped<IBasketRepository, BasketRepository>();
			services.AddScoped<IOrderService, OrderService>();
			services.AddScoped<IUnitOfWork, UnitOfWork>();

			services.Configure<ApiBehaviorOptions>(options =>
			{
				options.InvalidModelStateResponseFactory = actionContext =>
				{
					IEnumerable<string> errors = actionContext.ModelState
						.Where(mse => mse.Value.Errors.Count > 0)
						.SelectMany(mse => mse.Value.Errors)
						.Select(me => me.ErrorMessage);

					return new BadRequestObjectResult(new ApiValidationErrorResponse(errors));
				};
			});

			return services;
		}
	}
}

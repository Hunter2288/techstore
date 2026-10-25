using Catalog.Bll.Dto;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace Catalog.Bll.Validators
{
    public class CreateProductDtoValidator : AbstractValidator<CreateProductDto>
    {
        public CreateProductDtoValidator()
        {
            RuleFor(x => x.Sku).Matches("^[A-Z0-9-]+$")
                .WithMessage("SKU may contain only capital letters A-Z, digits and '-'");
            RuleFor(x => x.Price).PrecisionScale(18, 2, false)
                .WithMessage("Price allows at most 2 decimal places");
        }
    }
}

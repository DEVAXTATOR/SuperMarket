using System.ComponentModel.DataAnnotations;
using UseCases;

namespace WebApp.ViewModels.Validations
{
    public class SalesViewModel_EnsureProperQuantity : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            var salesViewModel = validationContext.ObjectInstance as SalesViewModel;

            if (salesViewModel != null)
            {
                if (salesViewModel.QuantityToSell <= 0)
                {
                    return new ValidationResult("Quantidade tem que ser mais do zero.");
                }
                else
                {
                    var getProductByIdUseCase = validationContext.GetService(typeof(IViewSelectedProductUseCase)) as IViewSelectedProductUseCase;

                    if (getProductByIdUseCase != null)
                    {
                        var product = getProductByIdUseCase.Execute(salesViewModel.SelectedProductId);
                        if (product != null)
                        {
                            if (product.Quantity < salesViewModel.QuantityToSell)
                                return new ValidationResult($"{product.Name} so tem {product.Quantity} nao chega.");
                        }
                        else
                        {
                            return new ValidationResult("O produto selecionado nao existe.");
                        }
                    }                    
                }
            }

            return ValidationResult.Success;
        }
    }
}

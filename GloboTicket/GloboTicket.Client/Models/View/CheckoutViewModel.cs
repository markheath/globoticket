using System.ComponentModel.DataAnnotations;

namespace GloboTicket.Web.Models.View;

public class CheckoutViewModel
{
    [Required]
    public string Name { get; set; } = string.Empty;

    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Address { get; set; } = string.Empty;

    [Required]
    public string Town { get; set; } = string.Empty;

    [Required]
    public string PostalCode { get; set; } = string.Empty;

    [Required, CreditCard]
    public string CreditCard { get; set; } = string.Empty;

    [Required]
    public string CreditCardDate { get; set; } = string.Empty;
}

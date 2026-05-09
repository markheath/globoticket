namespace GloboTicket.Web.Models.View;

public record OrderResultViewModel(Guid OrderId, string Status, string? FailureReason);

namespace Portal.Api.Models;

public class ErrorViewModel
{
    public String? RequestId { get; set; }

    public Boolean ShowRequestId => !string.IsNullOrEmpty(RequestId);
}
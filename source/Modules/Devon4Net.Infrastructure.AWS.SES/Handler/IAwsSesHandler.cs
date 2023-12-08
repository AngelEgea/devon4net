namespace Devon4Net.Infrastructure.AWS.SES.Handler
{
    public interface IAwsSesHandler : IDisposable
    {
        Task<bool> SendEmail(string senderAddress, List<string> receiverAddress, string subject, string textBody);
    }
}

using Amazon;
using Amazon.Runtime;
using Amazon.SimpleEmailV2;
using Amazon.SimpleEmailV2.Model;
using Devon4Net.Infrastructure.Common;
using System.Net;

namespace Devon4Net.Infrastructure.AWS.SES.Handler
{
    public class AwsSesHandler : IAwsSesHandler
    {
        private AmazonSimpleEmailServiceV2Client AmazonSimpleEmailServiceV2Client { get; }
        private AWSCredentials AWSCredentials { get; }
        private RegionEndpoint RegionEndpoint { get; }

        private bool _disposed = false;

        public AwsSesHandler(AWSCredentials awsCredentials = null, RegionEndpoint regionEndpoint = null)
        {
            AWSCredentials = awsCredentials;
            RegionEndpoint = regionEndpoint;
            AmazonSimpleEmailServiceV2Client = CreateSESClient();
        }

        public AwsSesHandler(AmazonSimpleEmailServiceV2Client amazonSimpleEmailServiceV2Client, AWSCredentials awsCredentials = null, RegionEndpoint regionEndpoint = null)
        {
            AWSCredentials = awsCredentials;
            RegionEndpoint = regionEndpoint;
            AmazonSimpleEmailServiceV2Client = amazonSimpleEmailServiceV2Client;
        }

        private AmazonSimpleEmailServiceV2Client CreateSESClient()
        {
            return AWSCredentials == null ? new AmazonSimpleEmailServiceV2Client(RegionEndpoint) : new AmazonSimpleEmailServiceV2Client(AWSCredentials, RegionEndpoint);
        }

        public async Task<bool> SendEmail(string senderAddress, List<string> receiverAddress, string subject, string textBody)
        {
            try
            {
                var sendRequest = new SendEmailRequest
                {
                    FromEmailAddress = senderAddress,
                    Destination = new Destination
                    {
                        ToAddresses = receiverAddress
                    },
                    Content = new EmailContent
                    {
                        Simple = new Message
                        {
                            Subject = new Content { Data = subject },
                            Body = new Body
                            {
                                Html = new Content
                                {
                                    Charset = System.Text.Encoding.UTF8.BodyName,
                                    Data = textBody
                                }
                            }
                        }
                    }
                };

                var sendResponse = await AmazonSimpleEmailServiceV2Client.SendEmailAsync(sendRequest).ConfigureAwait(false);

                return sendResponse.HttpStatusCode == HttpStatusCode.OK;
            }
            catch (Exception ex)
            {
                Devon4NetLogger.Fatal("An error ocurred while sending email. Email was not sent");
                Devon4NetLogger.Fatal(ex);
                throw;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                AmazonSimpleEmailServiceV2Client?.Dispose();
            }

            _disposed = true;
        }
    }
}

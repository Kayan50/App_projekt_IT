using System.Threading.Channels;

namespace App_projekt_IT.Services
{
    public interface IEmailSenderQueue
    {
        ValueTask QueueEmailAsync(EmailMessage message);
        ValueTask<EmailMessage> DequeueEmailAsync(CancellationToken cancellationToken);
    }

    public class EmailSenderQueue : IEmailSenderQueue
    {
        
        private readonly Channel<EmailMessage> _queue;

        public EmailSenderQueue()
        {
            var options = new BoundedChannelOptions(100)
            {
                FullMode = BoundedChannelFullMode.Wait
            };
            _queue = Channel.CreateBounded<EmailMessage>(options);
        }

        public async ValueTask QueueEmailAsync(EmailMessage message)
        {
            await _queue.Writer.WriteAsync(message);
        }

        public async ValueTask<EmailMessage> DequeueEmailAsync(CancellationToken cancellationToken)
        {
            return await _queue.Reader.ReadAsync(cancellationToken);
        }
    }
}
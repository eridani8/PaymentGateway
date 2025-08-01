using Microsoft.Extensions.Logging;
using PaymentGateway.PhoneApp.Interfaces;
using PaymentGateway.PhoneApp.Parsers;
using PaymentGateway.Shared.DTOs.Transaction;
using PaymentGateway.Shared.Enums;

namespace PaymentGateway.PhoneApp.Services;

public class SmsProcessor(ILogger<SmsProcessor> logger, ParserFactory factory, DeviceService service) : ISmsProcessor
{
    public async Task ProcessSms(string sender, string message)
    {
        logger.LogInformation("Получено SMS от: {Sender}\n{Message}", sender, message);
        
        var parser = factory.GetSmsParser(sender);
        if (parser is null)
        {
            logger.LogWarning("Нет парсера для номера {Number}", sender);
            return;
        }

        if (!parser.ParseMessage(message))
        {
            logger.LogWarning("[{ParserName}] Не удалось спарсить сообщение: {Message}", parser.GetType().Name, message);
            return;
        }
        
        logger.LogInformation("[{ParserName}] {Extracted}", parser.GetType().Name, parser.ExtractedAmount);

        await service.TransactionReceived(new TransactionReceivedDto()
        {
            Number = sender,
            ExtractedAmount = parser.ExtractedAmount,
            Source = TransactionSource.Sms,
            RawMessage = message
        });
    }
}
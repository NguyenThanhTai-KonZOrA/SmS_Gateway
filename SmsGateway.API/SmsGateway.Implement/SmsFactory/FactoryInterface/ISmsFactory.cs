using SmsGateway.Common.Enum;

namespace SmsGateway.Implement.SmsFactory.FactoryInterface
{
    public interface ISmsFactory
    {
        ISmsProvider CreateSmsProvider(SmsTypeServiceEnum smsTypeService);
    }
}
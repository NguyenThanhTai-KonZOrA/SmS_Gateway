using SmsGateway.Common.Enum;
using SmsGateway.Implement.Interface;
using SmsGateway.Implement.SmsFactory.FactoryInterface;

namespace SmsGateway.Implement.SmsFactory.FactoryService
{
    public class SmsFactory : ISmsFactory
    {
        private readonly IVnptSmsService _vnptSmsService;
        public SmsFactory(IVnptSmsService vnptSmsService)
        {
            _vnptSmsService = vnptSmsService;
        }

        public ISmsProvider CreateSmsProvider(SmsTypeServiceEnum providerName)
        {
            return providerName switch
            {
                SmsTypeServiceEnum.VNPT => new VnptSmsProvider(_vnptSmsService),
                SmsTypeServiceEnum.VIETTEL => new ViettelSmsProvider(),
                SmsTypeServiceEnum.MOBIFONE => new MobifoneSmsProvider(),
                SmsTypeServiceEnum.TWILIO => new TwilioSmsProvider(),
                _ => throw new NotImplementedException($"The SMS provider '{providerName}' is not implemented."),
            };

        }
    }
}
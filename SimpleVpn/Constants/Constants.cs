namespace SimpleVpn.Constants
{
    static class Variables
    {
        /* constants for handshake */
        public const byte EOF = 0x00;
        public const string SendMsg = "Send: ";
        public const string ReceivedMsg = "Received: ";
        public const int RaRbLength = 125;
        public const int DHCoefficientLength = 125;

        /* constants for encrypt/decrypt of messages */
        public const string hash = "SHA1";
        public const int maxSaltIVLength = 16; // max length for randomly generated salt or IV values
        public const int keySize = 256; // use 256-bit AES key
        public const int iterations = 4; // iterations to run PasswordDeriveBytes (of block cipher) for

        /*hard coded constants for DiffieHellman*/
        // P is a 1080 bit prime 
        public const string P = "121999492637070040497464880653482451122159715698431661862504934268987469885677710797799523307422120568454593141727668682332216679465216347609718241998150443969871262326615939878834844507147192404574401325870276945218845272041195113380201145626974399759092850500988371156171063899568397919181947787377580179491"; //23
        // G is a 160 bit prime 
        public const string G = "5322540848240629191790738444031509035347191367506640093778351221586257526250970342938004178185797187285021641998886132756315601193005759267241157022234467562219"; //7

    }
}
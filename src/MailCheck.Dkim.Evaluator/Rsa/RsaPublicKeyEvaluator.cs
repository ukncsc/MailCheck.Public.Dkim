using System;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;

namespace MailCheck.Dkim.Evaluator.Rsa
{
    public interface IRsaPublicKeyEvaluator
    {
        bool TryGetKeyLengthSize(string base64PublicKey, out int keyLength);
    }

    public class RsaPublicKeyEvaluator : IRsaPublicKeyEvaluator
    {
        public bool TryGetKeyLengthSize(string base64PublicKey, out int keyLength)
        {
            try
            {

                AsymmetricKeyParameter asymmetricKeyParameter =
                    PublicKeyFactory.CreateKey(Convert.FromBase64String(base64PublicKey));
                RsaKeyParameters rsaKeyParameters = (RsaKeyParameters)asymmetricKeyParameter;
                keyLength = rsaKeyParameters.Modulus.BitLength;
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine($"The public key could not be decoded {e.Message} {Environment.NewLine} {e.StackTrace}");
                keyLength = 0;
                return false;
            }

        }
    }
}

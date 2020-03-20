using Org.BouncyCastle.Asn1.CryptoPro;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Signers;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.X509;

namespace DemoPortalInternetBank.Pki
{
    public class GOSTPkiService : PkiService
    {
        protected override ISigner GetSigner()
        {
            return new Gost3410DigestSigner(new ECGost3410Signer(), new Gost3411_2012_256Digest());
        }

        protected override AsymmetricKeyParameter GetRootKey()
        {
            return (AsymmetricKeyParameter) RootCertificates.GetPrivateKeyGOST();
        }

        protected override X509Certificate GetRootCert()
        {
            return (X509Certificate) RootCertificates.GetRootCertGOST();
        }

        protected override X509Certificate GenerateCertificate(AsymmetricKeyParameter privateKey,
            X509V3CertificateGenerator certGen)
        {
            var signer = new GostSignerFactory(privateKey);
            return certGen.Generate(signer);
        }

        protected override AsymmetricCipherKeyPair GenerateKeyPair()
        {
            var oid = ECGost3410NamedCurves.GetOid("Tc26-Gost-3410-12-256-paramSetA");
            var param = new ECKeyGenerationParameters(oid, new SecureRandom());
            var engine = new ECKeyPairGenerator();

            engine.Init(param);

            return engine.GenerateKeyPair();
        }
    }
}
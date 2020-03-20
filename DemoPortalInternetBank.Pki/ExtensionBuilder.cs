using System.Linq;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.X509;
using Org.BouncyCastle.X509.Extension;

namespace DemoPortalInternetBank.Pki
{
    public abstract class ExtensionBuilder
    {
        public abstract void Build(
            X509V3CertificateGenerator certGen,
            Pkcs10CertificationRequest request,
            X509Certificate caCert);
    }

    public class DefaultExtensionBuilder : ExtensionBuilder
    {
        public override void Build(
            X509V3CertificateGenerator certGen,
            Pkcs10CertificationRequest request,
            X509Certificate caCert)
        {
        }
    }

    public class AllReqExtensionBuilder : ExtensionBuilder
    {
        public override void Build(
            X509V3CertificateGenerator certGen,
            Pkcs10CertificationRequest request,
            X509Certificate caCert)
        {
            var requestInfo = request.GetCertificationRequestInfo();

            var extensionSequence =
                requestInfo
                    .Attributes.OfType<DerSequence>()
                    .First(o => o.OfType<DerObjectIdentifier>().Any(oo => oo.Id == "1.2.840.113549.1.9.14"));

            var extensionSet = extensionSequence.OfType<DerSet>().First().OfType<DerSequence>().First();

            var exts = X509Extensions.GetInstance(extensionSet);

            var extOIDs = exts.GetExtensionOids();

            foreach (var x509ExtOid in extOIDs)
            {
                var ext = exts.GetExtension(x509ExtOid);

                certGen.AddExtension(
                    x509ExtOid,
                    ext.IsCritical,
                    ext.GetParsedValue()
                );
            }
        }
    }

    public class DemoBankExtensionBuilder : ExtensionBuilder
    {
        public override void Build(X509V3CertificateGenerator certGen, Pkcs10CertificationRequest request, X509Certificate caCert)
        {
            const string demoBankCertExtension = "1.1.1.1.1.1.2";
            
            certGen.AddExtension(X509Extensions.AuthorityKeyIdentifier, false,
                new AuthorityKeyIdentifierStructure(caCert));
            certGen.AddExtension(X509Extensions.SubjectKeyIdentifier, false,
                new SubjectKeyIdentifierStructure(request.GetPublicKey()));

            certGen.AddExtension(
                X509Extensions.ExtendedKeyUsage,
                true,
                new ExtendedKeyUsage(new[]
                {
                    new DerObjectIdentifier(demoBankCertExtension)
                })
            );
        }
    }
    
    public class MicrosoftCaExtensionBuilder : ExtensionBuilder
    {
        private readonly string upnName;
        private readonly string crlLink;

        public MicrosoftCaExtensionBuilder(string upnName, string crlLink)
        {
            this.upnName = upnName;
            this.crlLink = crlLink;
        }

        public override void Build(
            X509V3CertificateGenerator certGen,
            Pkcs10CertificationRequest request,
            X509Certificate caCert)
        {
            certGen.AddExtension(
                X509Extensions.AuthorityKeyIdentifier,
                false,
                new AuthorityKeyIdentifierStructure(caCert)
            );

            certGen.AddExtension(
                X509Extensions.SubjectKeyIdentifier,
                false,
                new SubjectKeyIdentifierStructure(request.GetPublicKey())
            );

            certGen.AddExtension(
                X509Extensions.KeyUsage,
                true,
                new KeyUsage(0xa0)
            );

            certGen.AddExtension(
                X509Extensions.ExtendedKeyUsage,
                true,
                new ExtendedKeyUsage(new[]
                {
                    new DerObjectIdentifier("1.3.6.1.4.1.311.20.2.2"),
                    new DerObjectIdentifier("1.3.6.1.5.5.7.3.2")
                })
            );

            var link = crlLink;

            var distPointOne =
                new DistributionPointName(
                    new GeneralNames(new GeneralName(GeneralName.UniformResourceIdentifier, link)));

            var distPoints = new DistributionPoint[1];
            distPoints[0] = new DistributionPoint(distPointOne, null, null);

            certGen.AddExtension(
                X509Extensions.CrlDistributionPoints,
                false,
                new CrlDistPoint(distPoints)
            );

            var otherName = new Asn1EncodableVector
            {
                new DerObjectIdentifier("1.3.6.1.4.1.311.20.2.3"),
                new DerTaggedObject(true, GeneralName.OtherName, new DerUtf8String(upnName))
            };

            var upn = new DerTaggedObject(false, 0, new DerSequence(otherName));

            var generalNames = new Asn1EncodableVector {upn};

            certGen.AddExtension(
                X509Extensions.SubjectAlternativeName,
                false,
                new DerSequence(generalNames)
            );
        }
    }
    
    
}
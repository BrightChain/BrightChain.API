namespace BrightChain.API.Helpers
{
    using System.Security.Cryptography;
    using System.Security.Cryptography.X509Certificates;
    using LettuceEncrypt;

    public class LettuceEncryptSource : ICertificateSource
    {
        public async Task<IEnumerable<X509Certificate2>> GetCertificatesAsync(CancellationToken cancellationToken)
        {
            // find and return certificate objects. Return an empty enumerable if none are found
            return new X509Certificate2[]
            {
                await LoadCertificates().ConfigureAwait(false),
            };
        }

        public static async Task<X509Certificate2> LoadPemCertificate(string certificatePath, string privateKeyPath = null)
        {
            using var publicKey = new X509Certificate2(certificatePath);

            if (privateKeyPath is null)
            {
                return publicKey;
            }

            var privateKeyText = await File.ReadAllTextAsync(privateKeyPath).ConfigureAwait(false);
            var privateKeyBlocks = privateKeyText.Split("-", StringSplitOptions.RemoveEmptyEntries);
            var privateKeyBytes = Convert.FromBase64String(privateKeyBlocks[1]);
            using var rsa = RSA.Create();

            if (privateKeyBlocks[0] == "BEGIN PRIVATE KEY")
            {
                rsa.ImportPkcs8PrivateKey(privateKeyBytes, out _);
            }
            else if (privateKeyBlocks[0] == "BEGIN RSA PRIVATE KEY")
            {
                rsa.ImportRSAPrivateKey(privateKeyBytes, out _);
            }

            var keyPair = publicKey.CopyWithPrivateKey(rsa);
            return new X509Certificate2(keyPair.Export(X509ContentType.Pfx));
        }

        public static async Task<X509Certificate2> LoadCertificates()
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false)
                .AddJsonFile($"appsettings.{Startup.Environment.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            var certificateConfiguration = configuration.GetSection("Certificate");
            var folderPath = certificateConfiguration["Folder"];
            var certificateFileName = certificateConfiguration["PemFileName"];
            var privateKeyFileName = certificateConfiguration["PrivateKeyFileName"];

            // OpenSSL / Cert-Manager / Kubernetes-style Certificates
            if (certificateFileName != null && privateKeyFileName != null)
            {
                var certificatePath = Path.Combine(folderPath, certificateFileName);
                var privateKeyPath = Path.Combine(folderPath, privateKeyFileName);
                return await LoadPemCertificate(certificatePath, privateKeyPath).ConfigureAwait(false);
            }
            else // Windows-style Certificate
            {
                var pfxPath = Path.Combine(folderPath, certificateConfiguration["PfxFileName"]);
                var pfxPassword = certificateConfiguration["PfxPassword"];
                return new X509Certificate2(pfxPath, pfxPassword);
            }
        }
    }
}

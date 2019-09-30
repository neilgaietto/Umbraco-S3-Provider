using System.Configuration;
using Amazon.S3;
using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Core.Exceptions;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Forms.Core.Components;
using Umbraco.Forms.Data.FileSystem;
using Umbraco.Storage.S3.Services;

namespace Umbraco.Storage.S3
{
    [ComposeAfter(typeof(UmbracoFormsComposer))]
    [ComposeAfter(typeof(BucketFileSystemComposer))]
    [RuntimeLevel(MinLevel = RuntimeLevel.Run)]
    public class BucketFormsFileSystemComposer : IComposer
    {
        private const string AppSettingsKey = "BucketFileSystem";
        private readonly char[] Delimiters = "/".ToCharArray();
        private const string ProviderAlias = "Forms";
        public void Compose(Composition composition)
        {
            var bucketName = ConfigurationManager.AppSettings[$"{AppSettingsKey}:BucketName:{ProviderAlias}"];
            if (bucketName != null)
            {
                var config = CreateConfiguration();

                //Reads config from AppSetting keys
                composition.RegisterUnique(config);
                composition.RegisterUniqueFor<IFileSystem, FormsFileSystemForSavedData>((f) => new BucketFileSystem(
                    config,
                    f.GetInstance<IMimeTypeResolver>(),
                    null,
                    f.GetInstance<ILogger>(),
                    new AmazonS3Client(Amazon.RegionEndpoint.GetBySystemName(config.Region))));
            }
        }

        private BucketFileSystemConfig CreateConfiguration()
        {
            var bucketName = ConfigurationManager.AppSettings[$"{AppSettingsKey}:BucketName:{ProviderAlias}"];
            var bucketPrefix = ConfigurationManager.AppSettings[$"{AppSettingsKey}:BucketPrefix:{ProviderAlias}"].Trim(Delimiters);
            var region = ConfigurationManager.AppSettings[$"{AppSettingsKey}:Region:{ProviderAlias}"];

            if (string.IsNullOrEmpty(bucketName))
                throw new ArgumentNullOrEmptyException("BucketName", $"The AWS S3 Forms Bucket File System is missing the value '{AppSettingsKey}:BucketName:{ProviderAlias}' from AppSettings");

            if (string.IsNullOrEmpty(bucketPrefix))
                throw new ArgumentNullOrEmptyException("BucketPrefix", $"The AWS S3 Forms Bucket File System is missing the value '{AppSettingsKey}:BucketPrefix:{ProviderAlias}' from AppSettings");

            if (string.IsNullOrEmpty(region))
                throw new ArgumentNullOrEmptyException("Region", $"The AWS S3 Forms Bucket File System is missing the value '{AppSettingsKey}:Region:{ProviderAlias}' from AppSettings");


            return new BucketFileSystemConfig
            {
                BucketName = bucketName,
                BucketPrefix = bucketPrefix,
                Region = region,
                CannedACL = S3CannedACL.Private,
                ServerSideEncryptionMethod = "",
                BucketHostName = "",
                DisableVirtualPathProvider = false
            };
        }
    }
}

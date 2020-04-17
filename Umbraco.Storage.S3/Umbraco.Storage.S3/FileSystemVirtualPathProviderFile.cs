using System;
using System.IO;
using System.Web;
using System.Web.Hosting;

namespace Umbraco.Storage.S3
{
    internal class FileSystemVirtualPathProviderFile : VirtualFile
    {
        private readonly Func<Stream> _stream;

        public FileSystemVirtualPathProviderFile(string virtualPath, Func<Stream> stream) : base(virtualPath)
        {
            if (stream == null)
                throw new ArgumentNullException("stream");
            _stream = stream;
        }

        public override Stream Open()
        {
            if (HttpContext.Current != null && HttpContext.Current.Response != null)
            {
                HttpContext.Current.Response.Cache.SetCacheability(HttpCacheability.Public);
                HttpContext.Current.Response.Cache.SetMaxAge(TimeSpan.FromDays(366));
            }
            return _stream();
        }

        public override bool IsDirectory
        {
            get { return false; }
        }
    }
}

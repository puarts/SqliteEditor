using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SqliteEditorTest.Utilities
{
    public class ScopedDirectoryCreator : IDisposable
    {
        public ScopedDirectoryCreator()
        {
            var name = Assembly.GetExecutingAssembly().GetName().Name;
            var workDir = Path.Combine(Path.GetTempPath(), $"{name}-{Guid.NewGuid().ToString("N")}");
            DirectoryPath = workDir;
            Directory.CreateDirectory(workDir);
        }

        public void Dispose()
        {
            var exception = DeleteDirectory();
            if (exception != null)
            {
                throw new Exception($"Failed to delete {DirectoryPath}", exception);
            }
        }

        public string DirectoryPath { get; }

        private Exception? DeleteDirectory()
        {
            Exception? exception = null;
            for (int tryCount = 0; tryCount < 30; ++tryCount)
            {
                try
                {
                    Directory.Delete(DirectoryPath, true);
                    return null;
                }
                catch (Exception e)
                {
                    exception = e;
                }

                Thread.Sleep(100);
            }

            return exception;
        }
    }
}

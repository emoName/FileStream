using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileStream.Example
{
    class FileEvents
    {
        private string _mirorPath = "D:\\FileStream\\NewFolder1\\";
        private string _mainPath = "D:\\FileStream\\NewFolder1\\";

        public FileEvents(string path, string mirorPath)
        {
            _mainPath = path;
            _mirorPath = mirorPath;
        }

        public void ActivateFileEvents()
        {
            //   Console.ReadLine();

            if ( !Directory.Exists(_mainPath) )
            {

                Console.WriteLine("Erorr Directory doesn't exist !!!");
                return;
            }
            else
            {

                using ( var inputFileWatcher = new FileSystemWatcher(_mainPath) )
                {

                    inputFileWatcher.IncludeSubdirectories = true;
                    inputFileWatcher.InternalBufferSize = 32768;
                    inputFileWatcher.Filter = "*.*";
                    inputFileWatcher.NotifyFilter = NotifyFilters.LastWrite
                                                  | NotifyFilters.FileName
                                                  | NotifyFilters.DirectoryName
                                                  | NotifyFilters.LastAccess;


                    inputFileWatcher.Created += FileCrated;
                    inputFileWatcher.Changed += FileChanged;
                    inputFileWatcher.Deleted += FileDeleted;
                    inputFileWatcher.Renamed += FileRenamed;
                    inputFileWatcher.Error += FileError;



                    inputFileWatcher.EnableRaisingEvents = true;


                    Console.WriteLine("Press Enter to exit ");

                    //   Program.semaphore.WaitOne();

                    Console.ReadLine();
                }


            }
        }

        private void FileError(object sender, ErrorEventArgs e)
        {
            Console.WriteLine($"File Error  {e.ToString()}");
        }

        private void FileRenamed(object sender, RenamedEventArgs e)
        {

            if ( CheckIsDir(e.FullPath) )
            {
                var ss = e.OldFullPath.Substring(_mainPath.Length);
                var nn = e.FullPath.Substring(_mainPath.Length);

                Console.WriteLine(ss + "  " + nn);
                Directory.Move(_mirorPath + ss, _mirorPath + nn);
                Console.WriteLine("Directory Renamed");
            }
            else
            {
                var ss = e.OldFullPath.Substring(_mainPath.Length);
                var nn = e.FullPath.Substring(_mainPath.Length);
                if ( File.Exists(_mirorPath + ss) )
                {
                    Console.WriteLine(ss + "  " + nn);
                    Directory.Move(_mirorPath + ss, _mirorPath + nn);

                    Console.WriteLine("FileRenamed ");

                }
                else
                {
                    CopyFileToMirror(e);
                }


            }

        }

        private void FileDeleted(object sender, FileSystemEventArgs e)
        {
            Console.WriteLine("Detete");
            if ( CheckIsDir(_mirorPath + e.FullPath.Substring(_mainPath.Length)) )
            {
                var ss = e.FullPath.Substring(_mainPath.Length);
                Directory.Delete((_mirorPath + ss), true);
                Console.WriteLine("Directory delated  ");
            }
            else
            {
                if ( File.Exists(_mirorPath + e.FullPath.Substring(_mainPath.Length)) )
                {

                    File.Delete(_mirorPath + e.FullPath.Substring(_mainPath.Length));
                    Console.WriteLine("FileDeleted =  "+e.FullPath);
                }
            }

        }

        private void FileChanged(object sender, FileSystemEventArgs e)
        {

            if ( CheckIsDir(e.FullPath) )
            {
                CreateDir(Path.Combine(_mirorPath, e.Name));
            }
            else
            {

                Console.WriteLine("FileChanged");
                CopyFileToMirror(e);
            }
        }

        private void FileCrated(object sender, FileSystemEventArgs e)
        {
            Console.WriteLine("FileCrated");

            if ( CheckIsDir(e.FullPath) )
            {
                CreateDir(Path.Combine(_mirorPath, e.Name));
            }
            else
            {
                CopyFileToMirror(e);
            }
        }

        private string GetSubstringMain(string e)
        {

            return e.Substring(0, _mainPath.Length);

        }

        private void CopyFileToMirror(FileSystemEventArgs e)
        {
            if ( File.Exists(Path.Combine(_mirorPath, e.Name)) )
            {

                File.Copy(e.FullPath, Path.Combine(_mirorPath, e.Name), true);
            }
            else
                File.Copy(e.FullPath, Path.Combine(_mirorPath, e.Name));
        }

        private void CreateDir(string s)
        {
            Console.WriteLine("new Directory");
            Directory.CreateDirectory(s);
        }
        private bool CheckIsDir(string s)
        {
            if ( File.Exists(s) || Directory.Exists(s) )
            {

                FileAttributes attr = File.GetAttributes(s);

                if ( attr.HasFlag(FileAttributes.Directory) )
                    return true;

            }
            return false;
        }


    }
}

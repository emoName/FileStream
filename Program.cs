using FileStream.Example;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

//  public static Semaphore semaphore = new Semaphore(0, 2);
// FileEvents file2 = new FileEvents(@"D:\FileStream\NewFolder2", @"D:\FileStream\NewFolder1");
//  Thread t1 = new Thread(file.ActivateFileEvents);
//  Thread t2 = new Thread(file2.ActivateFileEvents);
// t1.Start();

//Console.ReadLine();
//semaphore.Release();

namespace FileStream
{
    class Program
    {
        static void Main(string[] args)
        {
            string currentDir = Directory.GetCurrentDirectory();
            var directory = new DirectoryInfo(currentDir).Parent.Parent.FullName.ToString();
            var _mainPath = directory + @"\NewFolder1";
            var _mirrorPath = directory + @"\NewFolder2";


            FileEvents file = new FileEvents(_mainPath, _mirrorPath);

            CheckForNewDir(_mainPath, _mirrorPath);

            file.ActivateFileEvents();


            Console.WriteLine("Press any key");

            Console.ReadLine();

        }


        static private bool CheckForNewDir(string _mainPath, string _mirorPath)
        {

            var mainDir = GetRelativPath(Directory
                                         .GetDirectories(_mainPath, "*", SearchOption.AllDirectories)
                                         .ToList()
                                         , _mainPath);
            var mirrorDir = GetRelativPath(Directory
                                        .GetDirectories(_mirorPath, "*", SearchOption.AllDirectories)
                                        .ToList()
                                        , _mirorPath);

            SyncDir(_mirorPath, mainDir, mirrorDir,
                                                    (path, item) =>
                                                    {
                                                        Directory.CreateDirectory(path + item);
                                                        return false;
                                                    });
            var condition = true;
            do
            {
                condition = SyncDir(_mirorPath, mirrorDir, mainDir,
                                                          (path, item) =>
                                                          {
                                                              if ( Directory.Exists(path + item) )
                                                              {
                                                                  Directory.Delete((path + item), true);
                                                                  Console.WriteLine($"Directory delated  {path}  {item}");
                                                                  return true;
                                                              }
                                                              else
                                                                  Console.WriteLine($"Directory  {path}  {item}  don't exist ");
                                                              return false;
                                                          });

            } while ( condition );

            mainDir = GetRelativPath(Directory
                                         .GetDirectories(_mainPath, "*", SearchOption.AllDirectories)
                                         .ToList()
                                         , _mainPath);

            SyncFiles(_mainPath, _mirorPath, mainDir, true
                                            , (mirorPath, mainPath, file) =>
                                                                      {
                                                                          if ( File.Exists(mirorPath + file) )
                                                                          {
                                                                              File.Copy(mainPath + file, mirorPath + file, true);
                                                                          }
                                                                          else
                                                                              File.Copy(mainPath + file, mirorPath + file);
                                                                      });

            SyncFiles(_mirorPath, _mainPath, mainDir, false
                                           , (mainPath, mirorPath, file) =>
                                                                         {
                                                                             if ( File.Exists(mirorPath + file) )
                                                                             {
                                                                                 File.Delete(mirorPath + file);
                                                                                 Console.WriteLine("File was Deleted  = " + file);
                                                                             }
                                                                         });





            return false;
        }

        private static void SyncFiles(string _mainPath, string _mirorPath, List<string> mainDir, bool overide, Action<string, string, string> action)
        {
            List<string> mainFiles;
            List<string> mirorFiles;

            foreach ( var item in mainDir )
            {
                mainFiles = GetRelativPath(Directory.GetFiles(_mainPath + item).ToList(), _mainPath);
                mirorFiles = GetRelativPath(Directory.GetFiles(_mirorPath + item).ToList(), _mirorPath);

                foreach ( var file in mainFiles )
                {
                    var element = mirorFiles.Where(x => x.Equals(file)).Any();

                    Console.WriteLine(file + "  =" + element);
                    if ( !element )
                    {
                        action(_mirorPath, _mainPath, file);
                    }
                    else
                    {
                        if ( overide )
                        {
                            ReadAndCompareFiles(_mainPath, _mirorPath, file);
                        }
                    }

                }
            }
            mainFiles = GetRelativPath(Directory.GetFiles(_mainPath).ToList(), _mainPath);
            mirorFiles = GetRelativPath(Directory.GetFiles(_mirorPath).ToList(), _mirorPath);
            foreach ( var file in mainFiles )
            {
                var element = mirorFiles.Where(x => x.Equals(file)).Any();

                Console.WriteLine(file + "  =" + element);
                if ( !element )
                {
                    action(_mirorPath, _mainPath, file);
                } else
                    {
                        if ( overide )
                        {
                            ReadAndCompareFiles(_mainPath, _mirorPath, file);
                        }
                    }
            }
        }

        private static void ReadAndCompareFiles(string _mainPath, string _mirorPath, string file)
        {
            long mainLength = new System.IO.FileInfo(_mainPath + file).Length;
            long mirroLength = new System.IO.FileInfo(_mirorPath + file).Length;

            if ( mainLength == mirroLength )
            {
                // citeste si compara 
                if ( !CompareFiles(_mainPath + file, _mirorPath + file) )
                {
                    File.Copy(_mainPath + file, _mirorPath + file, true);
                    Console.WriteLine($"{_mainPath + file} overide { _mirorPath + file}  ==============Size===Eqals=========");
                }
            }
            else
            {
                // override 

                if ( File.Exists(_mirorPath + file) )
                {
                    File.Copy(_mainPath + file, _mirorPath + file, true);
                    Console.WriteLine($"{_mainPath + file} overide { _mirorPath + file}  =============Size====Diffrent=========");
                }
            }
        }

        private static bool CompareFiles(string a, string b)
        {
            bool f = true;
            if ( File.Exists(a) && File.Exists(b) )
            {
                Stream sourceA = File.OpenRead(a);
                Stream sourceB = File.OpenRead(b);

                try
                {
                    byte[] bufferA = new byte[1024 * 1024];
                    byte[] bufferB = new byte[1024 * 1024];
                    int bytesRead = 1;
                    while ( bytesRead > 0 )
                    {
                        bytesRead = sourceA.Read(bufferA, 0, bufferA.Length);
                        sourceB.Read(bufferB, 0, bufferB.Length);

                        if (! bufferA.SequenceEqual(bufferB)  )
                        {
                            return false;
                        }
                    }
                }
                catch ( FileNotFoundException ioEx )
                {
                    Console.WriteLine(ioEx.Message);
                }
                finally
                {
                    sourceA.Close();
                    sourceB.Close();
                }
            }
            return f;
        }

        private static bool SyncDir(string Path, List<string> mainDir, List<string> mirrorDir, Func<string, string, bool> action)
        {
            var condition = false;
            foreach ( var item in mainDir )
            {
                var element = mirrorDir.Where(x => x.Equals(item)).Any();
                if ( !element )
                {
                    condition = action(Path, item);
                }
                Console.WriteLine(item + "  =" + element);

            }
            return condition;
        }

        static public List<string> GetRelativPath(List<string> l, string rel)
        {
            var list = new List<string>();
            foreach ( var item in l )
            {
                list.Add(item.Substring(rel.Length));
            }
            return list;
        }


    }
}

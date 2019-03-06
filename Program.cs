using FileStream.Example;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

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

            var _mainPath = @"D:\Amdaris_Curs\FileStream\NewFolder1";
            var _mirrorPath = @"D:\Amdaris_Curs\FileStream\NewFolder2";


            FileEvents file = new FileEvents(_mainPath, _mirrorPath);
            //  file.ActivateFileEvents();

            CheckForNewDir(_mainPath, _mirrorPath);

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
                                                    });
            var condition = true;
            do
            {
                condition = SyncDir(_mirorPath, mirrorDir, mainDir,
                                                          (path, item) =>
                                                          {
                                                              Directory.Delete((path + item), true);
                                                              Console.WriteLine("Directory delated  ");
                                                          });

            } while ( condition );

            mainDir = GetRelativPath(Directory
                                         .GetDirectories(_mainPath, "*", SearchOption.AllDirectories)
                                         .ToList()
                                         , _mainPath);

            SyncFiles(_mainPath, _mirorPath, mainDir
                                            , (mirorPath, mainPath, file) =>
                                                                      {
                                                                          if ( File.Exists(mirorPath + file) )
                                                                          {
                                                                              File.Copy(mainPath + file, mirorPath + file, true);
                                                                          }
                                                                          else
                                                                              File.Copy(mainPath + file, mirorPath + file);
                                                                      });

            SyncFiles(_mirorPath, _mainPath, mainDir
                                           , (mirorPath, mainPath, file) =>
                                                                         {
                                                                             if ( File.Exists(mirorPath + file) )
                                                                             {
                                                                                 File.Delete(mirorPath + file);
                                                                                 Console.WriteLine("File was Deleted  = " + file);
                                                                             }
                                                                         });





            return false;
        }

        private static void SyncFiles(string _mainPath, string _mirorPath, List<string> mainDir, Action<string, string, string> action)
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
                }

            }
        }

        private static bool SyncDir(string Path, List<string> mainDir, List<string> mirrorDir, Action<string, string> action)
        {
            var condition = false;
            foreach ( var item in mainDir )
            {
                var element = mirrorDir.Where(x => x.Equals(item)).Any();
                if ( !element )
                {
                    condition = true;
                    action(Path, item);
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

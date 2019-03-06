using FileStream.Example;
using System;
using System.Collections.Generic;
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
            FileEvents file = new FileEvents(@"D:\FileStream\NewFolder1", @"D:\FileStream\NewFolder2");
            file.ActivateFileEvents();




        }
    }
}

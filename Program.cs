using System;

namespace UploadFilesToYD
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine(@"Введите адрес локальной директории  на компьютере, формат ввода: L:\test\folder\");
            var localDirectory = Console.ReadLine();
            Console.WriteLine("Введите адрес конечной папки на Яндекс Диске, формат ввода: Music/ (при копировании в корневой каталог оставьте поле пустым)" );
            var globalDirectory = Console.ReadLine();        
            YaDiskUploader.UploadToDisk(localDirectory, globalDirectory);
            Console.ReadLine();
        }
    }
}

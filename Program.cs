using System;

namespace UploadFilesToYD
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine(@"Введите адрес локальной директри  на компьютере, формат ввода: L:\test\folder\");
            var lDir = Console.ReadLine();
            Console.WriteLine("Введите адрес конечной папки на Яндекс диске, формат ввода: Music/ (при копировании в корневой каталог ничего вводить не нужно)" );
            var gDir = Console.ReadLine();
            YaDiskUploader.UploadToDisc(lDir, gDir);

            Console.ReadLine();
        }
    }
}

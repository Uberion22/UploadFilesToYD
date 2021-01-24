﻿using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace UploadFilesToYD
{
    /// <summary>
    /// Класс выгружает файлы из локальноый директории в указанный каталог на Яндекс Диске. 
    /// </summary>
    public static class YaDiskUploader
    {
        /// <summary>
        /// Перед работой с программой нужно указать ключ авторизации для загрузки файлов на Яндекс Диск
        /// </summary>
        private const string _token = "";
        
        /// <summary>
        /// API Яндекс Диска для загрузки файлов
        /// </summary>
        private const string _baseUri = "https://cloud-api.yandex.net/v1/disk/resources/upload/";
        
        /// <summary>
        /// Получить список  файлов хранящихся в указанном каталоге,
        /// и создать асинхронные операции выгрузки файлов на Яндекс Диск.
        /// </summary>
        /// <param name="localFileDirectory">Локальный путь к файлам в формате "L:\test\folder\".</param>
        /// <param name="diskDirecrory">Путь к папке на Яндекс Диске без учета корневого каталога. Пример: "Music/".</param>
        public static void UploadToDisk(string localFileDirectory = @"L:\test\", string diskDirectory = "")
        {
            try
            {
                // ищем все файлы в директории(в том числе вложенные) 
                string[] files = Directory.GetFiles(@localFileDirectory, "*.*", SearchOption.AllDirectories);
                PrintMessage("Получение списка файлов  в каталоге", ConsoleColor.White);
                
                for(int i = 0; i < files.Length; i++)
                {
                    var fileName = files[i].Remove(0, localFileDirectory.Length);
                    UploadAsync(files[i], fileName, diskDirectory);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        /// <summary>
        ///  Создать асинхронную задачу загрузки файла на Яндекс Диск.
        /// </summary>
        /// <param name="localAddress">Ссылка на полный адрес файла хранящегося на ПК. </param>
        /// <param name="localFileName"> Имя файла на ПК.</param>
        /// <param name="diskDir">Путь сохранения файла на Яндекс Диске.</param>
        public static async void UploadAsync(string localAddress, string localFileName, string diskDir)
        {
            await Task.Run(() => UploadToUrl(localAddress, localFileName, diskDir));
        }

        /// <summary>
        /// Сформировать запрос к Яндекс Диску с целью получения ссылки для выгрузки локального файла.
        /// При успешном выполнении метод возвращает ссылку.
        /// Требует авторизационный ключ. Указан как константа выше.
        /// </summary>
        /// <param name="fileName"> Имя выгружаемого файла.</param>
        /// <param name="overwrite">Пометка на перезапись. При нахождении файлов с одинаковыми названиями,
        /// файлы хранящиеся на диске заменяются новыми.</param>
        /// <param name="diskDir"> Конечная директория на диске.</param>
        /// <returns> Возвращает ссылку для выгрузки файла на Яндекс Диск.</returns>
        public static string GetUploadUrl(string fileName = "", bool overwrite = true, string diskDir = "")
        {
            string url = string.Empty;
            var request = WebRequest.Create(new Uri(_baseUri + "?path="
                + "disk:/" + diskDir + fileName + "&overwrite=" + overwrite.ToString()));
            // аваторизационный ключ для работы с диском
            request.Headers["Authorization"] = string.Format("OAuth {0}", _token);
            
            try
            {
                using (var myHttpWebResponse = (HttpWebResponse)request.GetResponse())
                {
                    if (myHttpWebResponse.StatusCode == HttpStatusCode.OK)
                    {
                        using (var dataStream = myHttpWebResponse.GetResponseStream())
                        {
                            StreamReader reader = new StreamReader(dataStream);
                            string strResponse = reader.ReadToEnd();
                            JObject response = JObject.Parse(strResponse);
                            url = (string)response.SelectToken("href");// выбор ссылки на URL(загрузки) по ключу
                            PrintMessage($"Place reserved: {fileName}", ConsoleColor.Blue);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return url;
        }

        /// <summary>
        /// Выгрузить файлы на Яндекс Диск с помощью специальной ссылки(полученной от ЯД). 
        /// </summary>
        /// <param name="fileName"> Имя файла</param>
        /// <param name="localFileNameWithPath">Адрес локального файла включающий его имя.</param>
        /// <param name="diskDir">Путь к папке на Яндекс Диске.</param>
        /// <param name="overwrite"> Пометка на перезапись файлов с одинаковми именами.</param>
        public static void UploadToUrl(string fileName, string localFileNameWithPath, string diskDir = "", bool overwrite = true)
        {
            try
            {
                var diskPath = GetUploadUrl(localFileNameWithPath, overwrite, diskDir);
                
                using (var client = new WebClient())
                {
                    PrintMessage($"File {localFileNameWithPath}: Upload started!", ConsoleColor.Yellow);
                    // Метод API не возвращает ответ, по этой причине не обрабатывается статус операции.
                    var response = client.UploadFile(diskPath, fileName);
                    PrintMessage($"File {localFileNameWithPath} Uploaded!", ConsoleColor.Green);
                }
            }
            catch (Exception err)
            {
                PrintMessage(err.Message, ConsoleColor.Red);
            }
        }

        /// <summary>
        /// Вывести текст заданного цвета в консоли.
        /// </summary>
        /// <param name="text">Текст выводимого сообщения.</param>
        /// <param name="color"> Цвет сообщения.</param>
        private static void PrintMessage(string text, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(text);
            Console.ResetColor();
        }
    }
}

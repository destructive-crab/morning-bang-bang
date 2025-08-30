using System;
using System.IO;
using banging_code.debug;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using Unity.VisualScripting;

namespace banging_code.json
{
    public static class SaveManager
    {
        private static string PathToSaveFolder => UnityEngine.Application.persistentDataPath;
        public static readonly System.Collections.Generic.Queue<QueueRegistry> Queue = new();

        public const string SAVE_FILE_PREFIX = ".bangsave";
        
        public static async UniTask<T> Load<T>(string saveName, string directory)
            where T : SavableData
        {
            try
            {
                string filePath = GetSaveFilePath(directory, saveName);
                if (!File.Exists(filePath)) return null;
                
                var contentTask = File.ReadAllTextAsync(filePath);
                
                await contentTask;

                T result = Newtonsoft.Json.JsonConvert.DeserializeObject<T>(contentTask.Result) as T;
                return result;
            }
            catch (Exception e)
            {
                return null;
            }
        }

        private static string GetSaveFilePath(string directory, string saveName)
        {
            return Path.Combine(PathToSaveFolder, directory, saveName + SAVE_FILE_PREFIX);
        }

        public static void PushToSaveQueue<T>(T savable)
            where T : SavableData
        {
            Queue.Enqueue(new QueueRegistry(savable, typeof(T), savable.GetSaveName(), savable.GetDirectory()));
        }

        public static async UniTask<bool> StartWritingFilesFromQueue()
        {
            try
            { 
                while (Queue.TryPeek(out QueueRegistry registry))
                {
                    await SaveImmediatly(registry); 
                }
                return true;
            }
            catch (Exception e)
            {
                LGR.PERR(e.ToString());
                return false;
            }
        }

        public static async UniTask<bool> SaveImmediatly(QueueRegistry registry)
        {
            if (!Directory.Exists(registry.DirectoryPath))
            {
                Directory.CreateDirectory(registry.DirectoryPath);
            }

//            if (!File.Exists(registry.FilePath))
//            {
//                File.Create(registry.FilePath);
//            }
            
            string content = GetSaveFileContent(registry);
            await File.WriteAllTextAsync(registry.FilePath, content);
            return true;
        }

        private static string GetSaveFileContent(QueueRegistry registry)
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(registry.TargetAsSavableData, registry.Type, Formatting.Indented, new JsonSerializerSettings());
        }

        public class QueueRegistry
        {
            public readonly string DirectoryPath;
            public readonly string FilePath;
            public readonly string FileName;
            public readonly string SaveName;

            public readonly System.Type Type;
            public readonly SavableData TargetAsSavableData;

            public QueueRegistry(SavableData target, System.Type type, string saveName, string directory)
            {
                SaveName = saveName;
                FileName = saveName + SaveManager.SAVE_FILE_PREFIX;
                DirectoryPath = Path.Combine(PathToSaveFolder, directory);
                
                FilePath = Path.Combine(DirectoryPath, FileName);

                TargetAsSavableData = target;
                Type = type;
            }
        }
    }
}
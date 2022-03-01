using System;
using System.CodeDom;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.CSharp;
using OpenNetcode.Shared.Attributes;
using UnityEditor;
using UnityEngine;

namespace OpenNetcode.Editor
{
    public class CodeGenerator
    {
        private static string ClientTemplatesPath = "OpenNetcode/Editor/Templates/Client/";
        private static string ServerTemplatesPath = "OpenNetcode/Editor/Templates/Server/";

        private static ProjectSettings LoadProjectSettings()
        {
            string path = "Assets/Project Settings.asset";
            ProjectSettings settings = AssetDatabase.LoadAssetAtPath<ProjectSettings>("Assets/Project Settings.asset");

            if (settings == null)
            {
                Debug.LogError("Could not load project settings at " + path);
            }

            if (!Directory.Exists(Path.Combine(Application.dataPath, settings.CodeGenerationPaths.Client)))
                Directory.CreateDirectory(Path.Combine(Application.dataPath, settings.CodeGenerationPaths.Client));
            
            if (!Directory.Exists(Path.Combine(Application.dataPath, settings.CodeGenerationPaths.Server)))
                Directory.CreateDirectory(Path.Combine(Application.dataPath, settings.CodeGenerationPaths.Server));
            
            if (!Directory.Exists(Path.Combine(Application.dataPath, settings.CodeGenerationPaths.Shared)))
                Directory.CreateDirectory(Path.Combine(Application.dataPath, settings.CodeGenerationPaths.Shared));

            bool directoryNotFound = false;

            if (!Directory.Exists(Path.Combine(Application.dataPath, settings.CodeGenerationPaths.Client)))
            {
                Debug.LogError("Directory does not exist at " + Path.Combine(Application.dataPath, settings.CodeGenerationPaths.Client));
                directoryNotFound = true;
            }
            
            if (!Directory.Exists(Path.Combine(Application.dataPath, settings.CodeGenerationPaths.Server)))
            {
                Debug.LogError("Directory does not exist at " + Path.Combine(Application.dataPath, settings.CodeGenerationPaths.Server));
                directoryNotFound = true;
            }
            
            if (!Directory.Exists(Path.Combine(Application.dataPath, settings.CodeGenerationPaths.Shared)))
            {
                Debug.LogError("Directory does not exist at " + Path.Combine(Application.dataPath, settings.CodeGenerationPaths.Shared));
                directoryNotFound = true;
            }

            if (directoryNotFound)
                return null;
            
            return settings;
        }

        [MenuItem("OpenNetcode/Clear Generated Code")]
        public static void ClearGeneratedCode()
        {
            ProjectSettings settings = LoadProjectSettings();

            if (settings == null)
                return;
            
            GenerateTemplate(ClientTemplatesPath, settings.CodeGenerationPaths.Client, "*.txt", ".cs");
            GenerateTemplate(ServerTemplatesPath, settings.CodeGenerationPaths.Server, "*.txt", ".cs");

            foreach (FileInfo file in new DirectoryInfo(Path.Combine(Application.dataPath, settings.CodeGenerationPaths.Shared)).GetFiles())
            {
                file.Delete();
            }
            
            AssetDatabase.Refresh();
        }

        [MenuItem("OpenNetcode/Development/Generate Templates")]
        public static void GenerateTemplates()
        {
            ProjectSettings setting = LoadProjectSettings();

            if (setting == null)
                return;
            
            GenerateTemplate(setting.CodeGenerationPaths.Client, ClientTemplatesPath, "*.cs", ".txt");
            GenerateTemplate(setting.CodeGenerationPaths.Server, ServerTemplatesPath, "*.cs", ".txt");

            AssetDatabase.Refresh();
        }

        private static void GenerateTemplate(string sourceDirectory, string targetDirectory, string sourceExtension,
            string targetExtension)
        {
            var directory = Path.Combine(Application.dataPath, sourceDirectory);
            var files = Directory.GetFiles(directory, sourceExtension);

            foreach (var file in files)
            {
                string text = File.ReadAllText(file);
                text = RemoveBlocks(text, "<generated>", "</generated>");
                string fileName = Path.GetFileNameWithoutExtension(file);
                string folder = Path.Combine(Application.dataPath, targetDirectory);
                File.WriteAllText(Path.Combine(folder, fileName + targetExtension), text);
            }
        }

        private static string ComponentTemplate = @"using System;
using Unity.Networking.Transport;
using OpenNetcode.Shared.Components;

namespace ##NAMESPACE##
{
    public partial struct ##TYPE## : ISnapshotComponent<##TYPE##>, IEquatable<##TYPE##>
    {
        public void WriteSnapshot(ref DataStreamWriter writer, in NetworkCompressionModel compressionModel, in ##TYPE## baseSnapshot)
        {
            //<write>
        }

        public void ReadSnapshot(ref DataStreamReader reader, in NetworkCompressionModel compressionModel, in ##TYPE## baseSnapshot)
        {
            //<read>
        }

        public bool Equals(##TYPE## other)
        {
            bool equals = true;
            //<equals>
            return equals;
        }

        public override bool Equals(object obj)
        {
            return obj is ##TYPE## other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                //<hash>
                return hash;
            }
        }
    }
}
";

        private static string WriteTemplate =
            @"            writer.WriteRawBits(Convert.ToUInt32(##NAME## != baseSnapshot.##NAME##), 1);
            if(!##NAME##.Equals(baseSnapshot.##NAME##)) ##NAME##.Write(ref writer, compressionModel, baseSnapshot.##NAME##);

";

        private static string ReadTemplate =
            @"            if (reader.ReadRawBits(1) == 0)
                ##NAME## = baseSnapshot.##NAME##;
            else
                ##NAME##.Read(ref reader, compressionModel, baseSnapshot.##NAME##);

";
        
        private static string WriteEnumTemplate =
            @"            writer.WriteRawBits(Convert.ToUInt32(##NAME## != baseSnapshot.##NAME##), 1);
            if(!##NAME##.Equals(baseSnapshot.##NAME##)) ((int) ##NAME##).Write(ref writer, compressionModel, (int) baseSnapshot.##NAME##);

";
        private static string ReadEnumTemplate =
            @"            if (reader.ReadRawBits(1) == 0)
                ##NAME## = baseSnapshot.##NAME##;
            else
            {
                int temp = ((int) ##NAME##);
                temp.Read(ref reader, compressionModel, (int) baseSnapshot.##NAME##);
                ##NAME## = (##TYPE##) temp;
            }
";

        private static string EqualsTemplate =
            @"equals = equals && ##NAME##.Equals(other.##NAME##);
";

        private static string HashTemplate =
            @"hash = hash * 23 + ##NAME##.GetHashCode();
";

        private static void GenerateSnapshotCodeForComponent(Type type, string generatedFolder)
        {
            string text = ComponentTemplate;
            text = text.Replace("##NAMESPACE##", type.Namespace);
            text = text.Replace("##TYPE##", type.Name);
            var fields = type.GetFields();
            
            int writeIndex = text.IndexOf("<write>", StringComparison.Ordinal) + "<write>".Length + 2;

            // Write
            foreach (var field in fields)
            {
                string write = field.FieldType.IsEnum ? WriteEnumTemplate : WriteTemplate;
                write = write.Replace("##NAME##", field.Name);
                write = write.Replace("##TYPE##", field.FieldType.Name);
                text = text.Insert(writeIndex, write);
                writeIndex += write.Length;
            }

            int readIndex = text.IndexOf("<read>", StringComparison.Ordinal) + "<read>".Length + 2;

            // Read
            foreach (var field in fields)
            {
                string read = field.FieldType.IsEnum ? ReadEnumTemplate : ReadTemplate;
                read = read.Replace("##NAME##", field.Name);
                read = read.Replace("##TYPE##", field.FieldType.Name);
                text = text.Insert(readIndex, read);
                readIndex += read.Length;
            }

            string generatedPath = Path.Combine(generatedFolder, type.Name + ".Generated.cs");
            
            using (var fs = File.Create(Path.Combine(Application.dataPath, generatedPath)))
            {
                var bytes = new UTF8Encoding(true).GetBytes(text);
                fs.Write(bytes, 0, bytes.Length);
            }

            Debug.Log(text);
        }

        private class CodeGenerationData
        {
            public List<Type> PublicSnapshots = new List<Type>();
            public List<Type> PrivateSnapshots = new List<Type>();
            public List<Type> PublicEvents = new List<Type>();
            public List<Type> Inputs = new List<Type>();
            public List<Type> Predictions = new List<Type>();
        }

        [MenuItem("OpenNetcode/Generate Code")]
        public static void Generate()
        {
            ProjectSettings settings = LoadProjectSettings();

            if (settings == null)
                return;
            
            CodeGenerationData data = new CodeGenerationData();

            
            
            foreach (FileInfo file in new DirectoryInfo(Path.Combine(Application.dataPath, settings.CodeGenerationPaths.Shared)).GetFiles())
            {
                file.Delete();
            }

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (Type type in assembly.GetTypes())
                {
                    if (type.GetCustomAttributes(typeof(PublicSnapshot), true).Length > 0)
                    {
                        GenerateSnapshotCodeForComponent(type, settings.CodeGenerationPaths.Shared);
                        data.PublicSnapshots.Add(type);
                    }

                    if (type.GetCustomAttributes(typeof(PrivateSnapshot), true).Length > 0)
                    {
                        GenerateSnapshotCodeForComponent(type, settings.CodeGenerationPaths.Shared);
                        data.PrivateSnapshots.Add(type);
                    }

                    if (type.GetCustomAttributes(typeof(PublicEvent), true).Length > 0)
                    {
                        GenerateSnapshotCodeForComponent(type, settings.CodeGenerationPaths.Shared);
                        data.PublicEvents.Add(type);
                    }
                    
                    if (type.GetCustomAttributes(typeof(Predict), true).Length > 0)
                    {
                        GenerateSnapshotCodeForComponent(type, settings.CodeGenerationPaths.Shared);
                        data.Predictions.Add(type);
                    }
                    
                    if (type.GetCustomAttributes(typeof(NetworkedInput), true).Length > 0)
                    {
                        GenerateSnapshotCodeForComponent(type, settings.CodeGenerationPaths.Shared);
                        data.Inputs.Add(type);
                    }
                }
            }

            {
                var directory = Path.Combine(Application.dataPath, ServerTemplatesPath);
                var files = Directory.GetFiles(directory, "*.txt");

                foreach (var template in files)
                {
                    string fileName = Path.GetFileNameWithoutExtension(template);
                    CreateTemplate(template, Path.Combine(settings.CodeGenerationPaths.Server, fileName + ".cs"), data);
                }
            }
            {
                var directory = Path.Combine(Application.dataPath, ClientTemplatesPath);
                var files = Directory.GetFiles(directory, "*.txt");

                foreach (var template in files)
                {
                    string fileName = Path.GetFileNameWithoutExtension(template);
                    CreateTemplate(template, Path.Combine(settings.CodeGenerationPaths.Client, fileName + ".cs"), data);
                }
            }

            // Generate snapshot settings

            AssetDatabase.Refresh();
        }

        public static int CountBits(uint value)
        {
            int count = 0;
            while (value != 0)
            {
                count++;
                value &= value - 1;
            }

            return count;
        }

        private static void CreateTemplate(string filePath, string generatedPath, CodeGenerationData data)
        {
            string text = File.ReadAllText(filePath);

            HashSet<string> namespaces = new HashSet<string>();

            foreach (var type in data.PublicSnapshots)
            {
                namespaces.Add(type.Namespace);
            }

            foreach (var type in data.PrivateSnapshots)
            {
                namespaces.Add(type.Namespace);
            }

            foreach (var type in data.PublicEvents)
            {
                namespaces.Add(type.Namespace);
            }

            text = RemoveBlocks(text, "<generated>", "</generated>");

            int namespaceIndex = text.IndexOf("<using>", StringComparison.Ordinal) + "<using>".Length + 2;

            text = text.Insert(namespaceIndex, "//<generated>\n");
            namespaceIndex += "//<generated>\n".Length;

            foreach (var n in namespaces)
            {
                string insert = "using " + n + ";\n";
                text = text.Insert(namespaceIndex, insert);
                namespaceIndex += insert.Length;
            }

            int eventMaskBits = CountBits((uint) data.PublicEvents.Count);
            int componentBufferLength = data.PublicEvents.Count + data.PublicSnapshots.Count;

            text = text.Insert(namespaceIndex, "//</generated>\n");

            text = Replace(text, "<template:publicsnapshot>", "</template>", data.PublicSnapshots, eventMaskBits, componentBufferLength);
            text = Replace(text, "<template:privatesnapshot>", "</template>", data.PrivateSnapshots, eventMaskBits, componentBufferLength, data.PublicSnapshots.Count);
            text = Replace(text, "<template:publicevent>", "</template>", data.PublicEvents, eventMaskBits, componentBufferLength, 0, data.PublicSnapshots.Count);
            text = Replace(text, "<template:input>", "</template>", data.Inputs, eventMaskBits, componentBufferLength);
            text = Replace(text, "<template:predicted>", "</template>", data.Predictions, eventMaskBits, componentBufferLength);

            Debug.Log("Generated " + Path.Combine(Application.dataPath, generatedPath));

            using (var fs = File.Create(Path.Combine(Application.dataPath, generatedPath)))
            {
                var bytes = new UTF8Encoding(true).GetBytes(text);
                fs.Write(bytes, 0, bytes.Length);
            }
        }

        private static string RemoveBlocks(string text, string tagBegin, string tagEnd)
        {
            int seek = 0;

            while (seek < text.Length)
            {
                int blockBegin = text.IndexOf(tagBegin, seek, StringComparison.Ordinal);
                if (blockBegin == -1)
                    break;

                int blockEnd = text.IndexOf(tagEnd, blockBegin, StringComparison.Ordinal);
                if (blockEnd == -1)
                    break;

                blockBegin = text.LastIndexOf('\n', blockBegin, blockBegin) + 1;
                blockEnd = text.IndexOf('\n', blockEnd) + 1;
                text = text.Remove(blockBegin, blockEnd - blockBegin);
            }

            return text;
        }

        private static string Replace(string text, string templateTagBegin, string templateTagEnd, List<Type> types,
            int eventMaskBits, int componentBufferLength, int typeOffset = 0, int indexOffset = 0)
        {
            int seek = 0;

            while (seek < text.Length)
            {
                int templateBegin = text.IndexOf(templateTagBegin, seek, StringComparison.Ordinal);
                if (templateBegin == -1)
                    break;

                int templateEnd = text.IndexOf(templateTagEnd, templateBegin, StringComparison.Ordinal);
                if (templateEnd == -1)
                    break;

                templateBegin = text.LastIndexOf('\n', templateBegin, templateBegin) + 1;
                templateEnd = text.IndexOf('\n', templateEnd);

                string template = text.Substring(templateBegin, templateEnd - templateBegin);

                int insertPosition = templateEnd + 1;

                text = text.Insert(insertPosition, "//<generated>\n");
                insertPosition += "//<generated>\n".Length;

                template = template.Substring(template.IndexOf('\n') + 1, template.Length - template.IndexOf('\n') - 1);
                template = template.Substring(0, template.LastIndexOf('\n', template.Length - 1, template.Length - 1));
                template = template.Replace("//", "");

                for (int i = 0; i < types.Count; i++)
                {
                    var type = types[i];
                    var name = GetTypeAliasName(type);
                    var lower = Char.ToLowerInvariant(name[0]) + name.Substring(1);

                    string insert = template.Replace("##TYPE##", name);
                    insert = insert.Replace("##TYPELOWER##", lower);
                    insert = insert.Replace("##INDEX##", (i + typeOffset).ToString());
                    insert = insert.Replace("##INDEXOFFSET##", indexOffset.ToString());
                    insert = insert.Replace("##EVENTMASKBITS##", eventMaskBits.ToString());
                    insert = insert.Replace("##COMPONENTBUFFERLENGTH##", componentBufferLength.ToString());
                    insert += "\n";
                    text = text.Insert(insertPosition, insert);
                    insertPosition += insert.Length;
                }

                text = text.Insert(insertPosition, "//</generated>\n");
                insertPosition += "//</generated>\n".Length;

                seek = insertPosition;
            }

            return text;
        }

        private static string GetTypeAliasName(Type type, bool fullName = false)
        {
            string typeName;
            using (var provider = new CSharpCodeProvider())
            {
                var typeRef = new CodeTypeReference(type);
                typeName = provider.GetTypeOutput(typeRef);
            }

            if (!fullName)
            {
                var index = typeName.LastIndexOf(".", StringComparison.Ordinal);
                return typeName.Substring(index + 1);
            }

            return typeName;
        }
    }
}
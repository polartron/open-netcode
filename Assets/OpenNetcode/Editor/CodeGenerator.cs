using System;
using System.CodeDom;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.CSharp;
using OpenNetcode.Shared;
using OpenNetcode.Shared.Attributes;
using OpenNetcode.Shared.Components;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEngine;

namespace OpenNetcode.Editor
{
    public class CodeGenerator
    {
        private static string ClientTemplatesPath = "OpenNetcode/Editor/Templates/Client/";
        private static string ServerTemplatesPath = "OpenNetcode/Editor/Templates/Server/";
        
        private static string ClientGeneratedPath = "ExampleGame/Client/Generated/";
        private static string ServerGeneratedPath = "ExampleGame/Server/Generated/";
        
        [MenuItem("OpenNetcode/Clear Generated Code")]
        public static void ClearGeneratedCode()
        {
            GenerateTemplate(ClientTemplatesPath, ClientGeneratedPath, "*.txt", ".cs");
            GenerateTemplate(ServerTemplatesPath, ServerGeneratedPath, "*.txt", ".cs");
            
            AssetDatabase.Refresh();
        }

        private static void Clear(string path)
        {
            
            
            AssetDatabase.Refresh();
        }

        [MenuItem("OpenNetcode/Development/Generate Templates")]
        public static void GenerateTemplates()
        {
            GenerateTemplate(ClientGeneratedPath, ClientTemplatesPath, "*.cs", ".txt");
            GenerateTemplate(ServerGeneratedPath, ServerTemplatesPath, "*.cs", ".txt");
            
            AssetDatabase.Refresh();
        }

        private static void GenerateTemplate(string sourceDirectory, string targetDirectory, string sourceExtension, string targetExtension)
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
        
        
        [MenuItem("OpenNetcode/Generate Code")]
        public static void Generate()
        {
            List<Type> publicSnapshots = new List<Type>();
            List<Type> privateSnapshots = new List<Type>();
            List<Type> publicEvents = new List<Type>();

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach(Type type in assembly.GetTypes()) 
                {
                    if (type.GetCustomAttributes(typeof(PublicSnapshot), true).Length > 0)
                    {
                        if(!type.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ISnapshotComponent<>)))
                        {
                            Debug.LogWarning($"PublicSnapshot attribute found on type {type.Name} but it does not implement the generic interface ISnapshotComponent<>");
                        }
                        else
                        {
                            publicSnapshots.Add(type);
                        }
                    }
                    
                    if (type.GetCustomAttributes(typeof(PrivateSnapshot), true).Length > 0)
                    {
                        if(!type.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ISnapshotComponent<>)))
                        {
                            Debug.LogWarning($"PrivateSnapshot attribute found on type {type.Name} but it does not implement the generic interface ISnapshotComponent<>");
                        }
                        else
                        {
                            privateSnapshots.Add(type);
                        }
                    }
                    
                    if (type.GetCustomAttributes(typeof(PublicEvent), true).Length > 0)
                    {
                        if(!type.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ISnapshotComponent<>)))
                        {
                            Debug.LogWarning($"PublicEvent attribute found on type {type.Name} but it does not implement the generic interface ISnapshotComponent<>");
                        }
                        else
                        {
                            publicEvents.Add(type);
                        }
                    }
                }
            }

            {
                var directory = Path.Combine(Application.dataPath, ServerTemplatesPath);
                var files = Directory.GetFiles(directory, "*.txt");

                foreach (var template in files)
                {
                    string fileName = Path.GetFileNameWithoutExtension(template);
                    CreateTemplate(template, Path.Combine(ServerGeneratedPath, fileName + ".cs"), publicSnapshots, privateSnapshots, publicEvents);
                }
            }
            {
                var directory = Path.Combine(Application.dataPath, ClientTemplatesPath);
                var files = Directory.GetFiles(directory, "*.txt");

                foreach (var template in files)
                {
                    string fileName = Path.GetFileNameWithoutExtension(template);
                    CreateTemplate(template, Path.Combine(ClientGeneratedPath, fileName + ".cs"), publicSnapshots, privateSnapshots, publicEvents);
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
        
        private static void CreateTemplate(string filePath, string generatedPath, List<Type> publicSnapshots, List<Type> privateSnapshots, List<Type> publicEvents)
        {
            string text = File.ReadAllText(filePath);
            
            HashSet<string> namespaces = new HashSet<string>();

            foreach (var type in publicSnapshots)
            {
                namespaces.Add(type.Namespace);
            }
            
            foreach (var type in privateSnapshots)
            {
                namespaces.Add(type.Namespace);
            }

            foreach (var type in publicEvents)
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
            
            
            int eventMaskBits = CountBits((uint) publicEvents.Count);
            int componentBufferLength = publicEvents.Count + publicSnapshots.Count;
            
            text = text.Insert(namespaceIndex, "//</generated>\n");
            
            text = Replace(text, "<template>", "</template>", publicSnapshots, eventMaskBits, componentBufferLength);
            text = Replace(text, "<privatetemplate>", "</privatetemplate>", privateSnapshots, eventMaskBits, componentBufferLength, publicSnapshots.Count);
            text = Replace(text, "<events>", "</events>", publicEvents, eventMaskBits, componentBufferLength, 0, publicSnapshots.Count);

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

        private static string Replace(string text, string templateTagBegin, string templateTagEnd, List<Type> types, int eventMaskBits, int componentBufferLength, int typeOffset = 0, int indexOffset = 0)
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

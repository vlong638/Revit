using System;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using PmSoft.Common.CommonClass;
using System.IO;

namespace MyRevit.Utilities
{
    public class ShareParameter
    {
        private string path;
        public ShareParameter(string p)
        {
            this.path = p;
        }

        public ShareParameter()
        {
            this.path = ApplicationPath.GetParentPathOfCurrent + @"\sysdata\" + "PMSharedParameters.txt";
        }

        /// <summary>
        /// 设置共享参数
        /// </summary>
        /// <param name="elem">设置需要绑定共享参数的元素</param>
        /// <param name="groupName">族的名称</param>
        /// <param name="parameterName">参数名称</param>
        public bool AddShadeParameter(UIApplication uiApp, string groupName, string parameterName, Category category, bool isInstance)
        {
            if (this.path == "")
                throw new Exception("路径无效");
            uiApp.ActiveUIDocument.Document.Application.SharedParametersFilename = this.path;
            DefinitionFile definitionFile = uiApp.ActiveUIDocument.Document.Application.OpenSharedParameterFile();
            DefinitionGroups groups = definitionFile.Groups;
            DefinitionGroup group = groups.get_Item(groupName);
            if (group == null)
                throw new Exception("没有找到对应的参数组,组名:" + groupName);
            Definition definition = group.Definitions.get_Item(parameterName);
            if (definition == null)
                throw new Exception("没有找到对应的参数,参数名:" + parameterName);
            ElementBinding binding = null;
            ElementBinding bind = uiApp.ActiveUIDocument.Document.ParameterBindings.get_Item(definition) as ElementBinding;
            CategorySet set = new CategorySet(); ;
            if (bind != null)
            {
                foreach (Category c in bind.Categories)
                {
                    set.Insert(c);
                }
            }
            var b = set.Insert(category);
            if (isInstance)
                binding = uiApp.ActiveUIDocument.Document.Application.Create.NewInstanceBinding(set);
            else
            {
                binding = uiApp.ActiveUIDocument.Document.Application.Create.NewTypeBinding(set);
            }
            uiApp.ActiveUIDocument.Document.ParameterBindings.Remove(definition);
            return uiApp.ActiveUIDocument.Document.ParameterBindings.Insert(definition, binding, BuiltInParameterGroup.PG_TEXT);
        }


        public bool AddShadeParameter(Document doc, string groupName, string parameterName, Category category, bool isInstance)
        {
            if (this.path == "")
                throw new Exception("路径无效");
            doc.Application.SharedParametersFilename = this.path;
            DefinitionFile definitionFile = doc.Application.OpenSharedParameterFile();
            DefinitionGroups groups = definitionFile.Groups;
            DefinitionGroup group = groups.get_Item(groupName);
            if (group == null)
                throw new Exception("没有找到对应的参数组,组名:" + groupName);
            Definition definition = group.Definitions.get_Item(parameterName);
            if (definition == null)
            {
                throw new Exception("没有找到对应的参数,参数名:" + parameterName);
            }

            ElementBinding binding = null;
            ElementBinding bind = doc.ParameterBindings.get_Item(definition) as ElementBinding;
            CategorySet set = new CategorySet(); ;
            if (bind != null)
            {
                foreach (Category c in bind.Categories)
                {
                    set.Insert(c);
                }
            }

            var b = set.Insert(category);
            if (isInstance)
                binding = doc.Application.Create.NewInstanceBinding(set);
            else
            {
                binding = doc.Application.Create.NewTypeBinding(set);
            }
            doc.ParameterBindings.Remove(definition);
            return doc.ParameterBindings.Insert(definition, binding, BuiltInParameterGroup.PG_TEXT);
        }

        public bool AddShadeParameter(Document doc, string groupName, string parameterName, Category category, bool isInstance, BuiltInParameterGroup parameterGroup)
        {
            if (this.path == "")
                throw new Exception("路径无效");
            doc.Application.SharedParametersFilename = this.path;
            DefinitionFile definitionFile = doc.Application.OpenSharedParameterFile();
            DefinitionGroups groups = definitionFile.Groups;
            DefinitionGroup group = groups.get_Item(groupName);
            if (group == null)
                throw new Exception("没有找到对应的参数组,组名:" + groupName);
            Definition definition = group.Definitions.get_Item(parameterName);
            if (definition == null)
                throw new Exception("没有找到对应的参数,参数名:" + parameterName);
            ElementBinding binding = null;
            ElementBinding bind = doc.ParameterBindings.get_Item(definition) as ElementBinding;
            CategorySet set = new CategorySet(); ;
            if (bind != null)
            {
                foreach (Category c in bind.Categories)
                {
                    set.Insert(c);
                }
            }
            var b = set.Insert(category);
            if (isInstance)
                binding = doc.Application.Create.NewInstanceBinding(set);
            else
            {
                binding = doc.Application.Create.NewTypeBinding(set);
            }
            doc.ParameterBindings.Remove(definition);
            return doc.ParameterBindings.Insert(definition, binding, parameterGroup);
        }



        public bool AddOrCreateShadeParameter(Document doc, string groupName, string parameterName, Category category, bool isInstance, BuiltInParameterGroup parameterGroup)
        {
            if (this.path == "")
                throw new Exception("路径无效");
            doc.Application.SharedParametersFilename = this.path;
            DefinitionFile definitionFile = doc.Application.OpenSharedParameterFile();
            DefinitionGroups groups = definitionFile.Groups;
            DefinitionGroup group = groups.get_Item(groupName);
            if (group == null)
                throw new Exception("没有找到对应的参数组,组名:" + groupName);
            Definition definition = group.Definitions.get_Item(parameterName);
            if (definition == null)
            {
                definition = group.Definitions.Create(new ExternalDefinitionCreationOptions(parameterName, ParameterType.Text));
            }

            ElementBinding binding = null;
            ElementBinding bind = doc.ParameterBindings.get_Item(definition) as ElementBinding;
            CategorySet set = new CategorySet(); ;
            if (bind != null)
            {
                foreach (Category c in bind.Categories)
                {
                    set.Insert(c);
                }
            }
            var b = set.Insert(category);
            if (isInstance)
                binding = doc.Application.Create.NewInstanceBinding(set);
            else
            {
                binding = doc.Application.Create.NewTypeBinding(set);
            }
            doc.ParameterBindings.Remove(definition);
            return doc.ParameterBindings.Insert(definition, binding, parameterGroup);
        }



        /// <summary>
        /// 删除某个类别中的共享参数
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="groupName">参数组</param>
        /// <param name="parameterName">参数名</param>
        /// <param name="moveCategory">类别</param>
        /// <param name="isInstance">false:类别， true: 实例</param>
        /// <returns></returns>
        public bool DeleteShareParameter(Document doc, string groupName, string parameterName, Category moveCategory, bool isInstance)
        {
            bool isSuccess = false;
            if (string.IsNullOrWhiteSpace(path))
                throw new Exception("无效路径");
            doc.Application.SharedParametersFilename = this.path;
            DefinitionFile file = doc.Application.OpenSharedParameterFile();
            if (file == null)
                throw new Exception("共享参数文件不存在");
            DefinitionGroups groups = file.Groups;
            DefinitionGroup group = file.Groups.get_Item(groupName);
            if (group == null)
                throw new Exception("参数组不存在");
            Definition definition = group.Definitions.get_Item(parameterName);
            if (definition == null)
                throw new Exception("共享参数不存在");
            ElementBinding elementBinding = doc.ParameterBindings.get_Item(definition) as ElementBinding;
            CategorySet set = elementBinding.Categories;
            CategorySet newSet = new CategorySet();
            foreach (Category category in set)
            {
                if (category.Name != moveCategory.Name)
                    newSet.Insert(category);
            }

            if (isInstance)
                elementBinding = doc.Application.Create.NewInstanceBinding(newSet);
            else
            {
                elementBinding = doc.Application.Create.NewTypeBinding(newSet);
            }

            doc.ParameterBindings.Remove(definition);
            if (elementBinding.Categories.Size > 0)
                return doc.ParameterBindings.Insert(definition, elementBinding, BuiltInParameterGroup.PG_TEXT);

            else
                return false;
        }



        public string GetValidatePath(UIApplication app, string SharedParametersFilename, ref bool CanAddShareParameter)
        {
            CanAddShareParameter = true;
            string path = ApplicationPath.GetParentPathOfCurrent + "\\SysData\\PMSharedParametersTemp.txt";
            try
            {
                if (app.Application.SharedParametersFilename != SharedParametersFilename)
                    app.Application.SharedParametersFilename = SharedParametersFilename;
                var definitionFile = app.Application.OpenSharedParameterFile();
            }
            catch
            {
                File.Delete(SharedParametersFilename);
                if (!File.Exists(SharedParametersFilename))

                    if (app.Application.SharedParametersFilename == path)
                        app.Application.SharedParametersFilename = null;
                if (File.Exists(path))
                    File.Delete(path);
                File.Copy(ApplicationPath.GetParentPathOfCurrent + "\\SysData\\PMSharedParameters.bak", ApplicationPath.GetParentPathOfCurrent + "\\SysData\\PMSharedParametersTemp.txt", true);
                app.Application.SharedParametersFilename = ApplicationPath.GetParentPathOfCurrent + "\\SysData\\PMSharedParametersTemp.txt";
                try
                {
                    app.Application.OpenSharedParameterFile();
                }
                catch { CanAddShareParameter = false; }
            }
            return path;
        }
    }
}

﻿

/*===================================================================================
* 
*   Copyright (c) Userware (OpenSilver.net, CSHTML5.com)
*      
*   This file is part of both the OpenSilver Compiler (https://opensilver.net), which
*   is licensed under the MIT license (https://opensource.org/licenses/MIT), and the
*   CSHTML5 Compiler (http://cshtml5.com), which is dual-licensed (MIT + commercial).
*   
*   As stated in the MIT license, "the above copyright notice and this permission
*   notice shall be included in all copies or substantial portions of the Software."
*  
\*====================================================================================*/



using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace TypeScriptDefToCSharp.Model
{
    public class Function : Declaration, Declaration.Container<Param>
    {
        public List<Param> Params { get; set; }
        public TSType ReturnType { get; set; }
        public bool Optional { get; set; }
        public bool HasString { get; set; }
        public bool Static { get; set; }

        public Function(Container<Declaration> super)
            : base(super)
        {
            this.Params = new List<Param>();
        }

        public Function(XElement elem, Container<Declaration> super, TypeScriptDefContext context)
            : this(super)
        {
            this.Name = elem.Element("function").Attribute("text").Value;

            while (this.Name.Length > 0 && this.Name[this.Name.Length - 1] == ' ')
            {
                this.Name = this.Name.Remove(this.Name.Length - 1);
            }

            if (this.Name.Length > 0 && this.Name[this.Name.Length - 1] == '?')
            {
                this.Optional = true;
                this.Name = this.Name.Remove(this.Name.Length - 1);
            }

            var paramlist = elem.Element("paramlist");
            if (paramlist != null)
                this.AddContent(paramlist, context);

            this.ReturnType = Tool.NewType(elem.Element("type"), context);
        }

        public void AddContent(XElement content, TypeScriptDefContext context)
        {
            int spread = 0;

            foreach (XElement e in content.Elements())
            {
                if (e.Name.LocalName == "variable")
                {
                    Param p = new Param(e, this, spread == 1, context);
                    if (p.hasString)
                        this.HasString = true;
                    this.Params.Add(p);
                }
                else if (e.Name.LocalName == "spreadop")
                    spread = 2;
                spread--;
            }
        }

        public override string ToString()
        {
            // Create the string builder
            var res = new StringBuilder();

            // Put the return type
            res.Append(this.ReturnType != null ? this.ReturnType.ToString() : "void")
               .Append(" ");

            // If there is a name
            if (this.Name.Length > 0)
                res.Append(Tool.ClearKeyWord(this.Name));

            // If there is no name, put Invoke as name
            else
                res.Append("Invoke");

            // Add the parameters
            string paramList = string.Join(", ", this.Params.Select(p => p.ToString()));
            res.Append("(")
               .Append(paramList)
               .AppendLine(")");

            // Function body
            res.AppendLine("{");

            // jsObj is the "Interop.ExecuteJavaScript("...", ...)" block
            var jsObj = new StringBuilder();

            // If there is a class path (for ns1.ns2.function, classPath is ns1.ns2),
            // we use it to reference the underlying JS function
            string classPath = "";
            if (this.Super is Class && this.Super != null)
                // Skip TypeScript def root namespace to avoid an infinite recursive call
                // of the function generated by JSIL from the generated C#
                classPath = this.Super.SkippedFullName(".", 1); 
            if (!String.IsNullOrWhiteSpace(classPath))
                classPath += ".";

            // Beginning of the javascript call after that
            jsObj.Append("Interop.ExecuteJavaScript(\"");
            // If the function is static, call with the full function path
            if (this.Static)
                jsObj.Append(classPath);
            // Else call with $0 ($0 will be this.UnderlyingJSInstance here)
            else
                jsObj.Append(string.IsNullOrWhiteSpace(this.Name) ? "$0" : "$0.");
            jsObj.Append(this.Name)
                 .Append("(");

            // For each param, put a $1 like this: $1, $2, ... ($0 might already be in use)
            jsObj.Append(string.Join(", ", Enumerable.Range(1, this.Params.Count).Select(i => "$" + i)));

            // Close the js function call
            jsObj.Append(")\", ");

            // If there is no need of this.UnderlyingJSInstance, put an empty string
            if (this.Static)
                jsObj.Append("\"\"");
            else
                jsObj.Append("this.UnderlyingJSInstance");

            if (this.Params.Any())
                jsObj.Append(", ")
                     .Append(string.Join(", ", this.GetJSParamsValues()));
            jsObj.Append(")");

            // If there is no return type, just put jsObj as the body
            if (this.ReturnType == null)
                res.Append(jsObj.ToString());
            // Else return a new object with jsObj as parameter
            else
                res.Append("return ")
                   .Append(this.ReturnType.New(jsObj.ToString()));
            res.AppendLine(";");

            res.AppendLine("}");

            return res.ToString();
        }

        protected IEnumerable<string> GetJSParamsValues()
        {
            return this.Params.Select(p =>
                new StringBuilder()
                .Append(Tool.ClearKeyWord(p.Name))
                .Append(" != null ? ")
                .Append(this.GetJSParamValue(p))
                .Append(" : JSObject.Undefined.UnderlyingJSInstance")
                .ToString());
        }

        protected string GetJSParamValue(Param p)
        {
            var res = new StringBuilder();

            var name = Tool.ClearKeyWord(p.Name);
            var t = p.Type;

            if (t is Model.Enum)
                res.Append("\"")
                   .Append(t.ToString())
                   .Append(".\" + ")
                   .Append(name)
                   .Append(".ToString()");
            else if (t is FunctionType
                 || (Tool.IsBasicType(t) && t.ToString() != "IJSObject"))
                res.Append(name);
            else
                res.Append(name)
                   .Append(".ToJavaScriptObject()");

            return res.ToString();
        }

        // IEnumerable implementation

        public IEnumerator<Param> GetEnumerator()
        {
            return this.Params.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.Params.GetEnumerator();
        }
    }
}

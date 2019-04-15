using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using net.adamec.lib.common.dmn.engine.parser;
using Newtonsoft.Json;
using CommandLine;
using CommandLine.Text;
using System.Text;
using net.adamec.lib.common.dmn.engine.parser.dto;

namespace dmn_test
{

    public class DmnTestCase
    {
        public DmnTestCase(string key)
        {
            this.Key = key;

        }

        public DmnTestCase(){

        }
        public string Key { get; set; }
        public List<DmnRequest> Requests = new List<DmnRequest>();
    }

    class Options
    {
        [Option('o', "operation", Required = false, HelpText = "Operation. 'create' for create test table for decision model.")]
        public string operation {get;set;}
        [Option('k', "key", Required = false, HelpText = "Key of the decision model top operate on.")]
        public string dmnKey {get;set;}
        [Option('e', "endpoint", Required = false, HelpText = "Endpoint url to the camunda rest api. i.e.: http://localhost:8080/engine-rest")]
        public string Endpoint { get; set; }
        
        [Option('m', "markdown", Required = false, HelpText = "Path to markdown file used as test input.")]
        public string Markdown { get; set; }
    }

    class Program
    {
        static string getSampleForType(string typeRef){
            switch (typeRef){
                case "double":
                    return "100.0";
                case "integer":
                    return "1";
                case "string":
                    return "\"Text\"";
            }
            return string.Empty;
        }

        static string createMarkdownTable(string key, DecisionTable table)
        {
            StringBuilder header = new StringBuilder();
            StringBuilder seperator = new StringBuilder();
            StringBuilder columns = new StringBuilder();
            foreach (var input in table.Inputs)
            {
                if (header.Length == 0)
                {
                    header.Append("| ");
                    seperator.Append("|-");
                    columns.Append("| ");
                }
                header.Append(input.Label + " |");
                seperator.Append("".PadRight(input.Label.Length, '-') + "-|");
                columns.Append(getSampleForType(input.InputExpression.TypeRef).PadRight(input.Label.Length, ' ') + " |");
            }
            foreach (var input in table.Outputs)
            {
                header.Append("*" + input.Label + " |");
                seperator.Append("-".PadRight(input.Label.Length + 1, '-') + "-|");
                columns.Append(getSampleForType(input.TypeRef).PadRight(input.Label.Length + 1, ' ') + " |");
            }
            header.Append(" ! |");
            seperator.Append(":-:|");
            columns.Append("&#x1F538;|");
            return $"## dmn:{key}\n" + header.ToString() + "\n" + seperator.ToString() + "\n" + columns + "\n";
        }

        static void Main(string[] args)
        {
            Parser.Default.ParseArguments<Options>(args).WithParsed<Options>(o=>{
                switch (o.operation)
                {
                    case "create":
                        var client = new DmnClient(o.Endpoint);
                        var res = client.GetDefinition(o.dmnKey).Result;
                        var def = DmnParser.ParseString(res.DmnXml);
                        System.IO.File.WriteAllText(o.Markdown, "# Decisions\n");                        
                        foreach (var decision in def.Decisions){
                            var table = createMarkdownTable(decision.Name,decision.DecisionTable);
                            System.IO.File.AppendAllText(o.Markdown, table + "\n");                        
                        }
                    break;
                }
                return;

            int mode = 0;
            List<DmnTestCase> cases = new List<DmnTestCase>();
            DmnTestCase current = new DmnTestCase();
            string[] fieldNames = null;
            string key = "";
            // read the markdown file.
            var j = 0;
            var text = System.IO.File.ReadAllText(o.Markdown);
            for (int i = 0; i < text.Length; i++)
            {
                switch (mode)
                {
                    case 0: // searching for test scenario
                        Console.Write("Searching for test scenario... ");
                        if (i + 7 > text.Length)
                        {
                            mode = 4;
                            break;
                        }
                        if (text.Substring(i, 7) == "## dmn:")
                        {
                            j = i + 7;
                            i = text.IndexOf('\n', j);
                            mode = 1;
                            key = text.Substring(j, i - j);
                            Console.Write($"found scenario {key} ");
                        }
                        current.Key = key;
                        break;
                    case 1: // searching for starting table
                        if (text[i] == '|') mode = 2;
                        break;
                    case 2: // header mode
                        j = i;
                        i = text.IndexOf('\n', j);
                        fieldNames = text.Substring(j, i - j - 1).Split('|').Select(x => x.Trim()).ToArray();
                        i = text.IndexOf('\n', i + 1);
                        mode = 3;
                        break;
                    case 3: // extracting test cases for the current scenario
                        if (text[i] != '|')
                        {
                            cases.Add(current);
                            current = new DmnTestCase();
                            mode = 0;
                            break;
                        }
                        j = text.IndexOf('\n', i + 1);
                        var values = text.Substring(i + 1, j - i).Split('|').Select(x => x.Trim()).ToArray();
                        var req1 = new DmnRequest();
                        for (int l = 0; l < fieldNames.Length - 1; l++)
                        {
                            double r = 0;
                            if (double.TryParse(values[l], out r))
                            {
                                req1.Variables.Add(fieldNames[l], new Variable() { Value = r });
                            }
                            else
                            {
                                req1.Variables.Add(fieldNames[l], new Variable() { Value = values[l] });
                            }
                        }
                        i = j;
                        current.Requests.Add(req1);
                        break;
                    case 4: // done.
                        i = text.Length;
                        break;
                }
            }
            });

            // var request = new DmnRequest();
            // request.Variables.Add("amount", new Variable() { Value = 200 });
            // request.Variables.Add("invoiceCategory", new Variable() { Value = "Travel Expenses" });
            // var req = JsonConvert.SerializeObject(request);
            // var client = new DmnClient("http://192.168.99.112:8080/engine-rest");

            // var res = client.GetDefinition("invoiceClassification").Result;
            // var def = DmnParser.ParseString(res.DmnXml);
            // var inp = def.Decisions[0].DecisionTable.Inputs;
            //var result = client.Evaluate("invoiceClassification",request).Result;
        }
    }
}

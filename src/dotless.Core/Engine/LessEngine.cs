using System;

namespace dotless.Core
{
    using System.Collections.Generic;
    using System.Linq;
    using Exceptions;
    using Loggers;
    using Parser.Infrastructure;

    public class LessEngine : ILessEngine
    {
        public Parser.Parser Parser { get; set; }
        public ILogger Logger { get; set; }
        public bool Compress { get; set; }
        public int RuleLimit { get; set; }

        public LessEngine(Parser.Parser parser, ILogger logger, bool compress, int ruleLimit)
        {
            Parser = parser;
            Logger = logger;
            Compress = compress;
            RuleLimit = ruleLimit;
        }

        public LessEngine(Parser.Parser parser)
            : this(parser, new ConsoleLogger(LogLevel.Error), false, 0)
        {
        }

        public LessEngine()
            : this(new Parser.Parser())
        {
        }

        public string TransformToCss(string source, string fileName)
        {
            string css = null;

            try
            {
                var tree = Parser.Parse(source, fileName);

                var env = new Env { Compress = Compress };

                css = tree.ToCSS(env);
            }
            catch (ParserException e)
            {
                Logger.Error(e.Message);
            }

            return VerifyOutput(css) ? css : String.Empty;
        }

        public bool VerifyOutput(string css)
        {
            if (css == null)
                return false;

            if (RuleLimit > 0 && Parser.NodeProvider.TotalRules > RuleLimit)
            {
                Logger.Error(String.Format("Total rule count ({0}) exceeds limit ({1}).",
                    Parser.NodeProvider.TotalRules, RuleLimit));

                return false;
            }

            return true;
        }

        public IEnumerable<string> GetImports()
        {
            return Parser.Importer.Imports.Distinct();
        }

        public void ResetImports()
        {
            Parser.Importer.Imports.Clear();
        }

    }
}
﻿namespace UglyToad.Pdf.Fonts.Parser
{
    using System;
    using Cmap;
    using IO;
    using Parts;
    using Tokenization.Scanner;
    using Tokenization.Tokens;

    public class CMapParser
    {
        private static readonly BaseFontRangeParser BaseFontRangeParser = new BaseFontRangeParser();
        private static readonly BaseFontCharacterParser BaseFontCharacterParser = new BaseFontCharacterParser();
        private static readonly CidRangeParser CidRangeParser = new CidRangeParser();
        private static readonly CidFontNameParser CidFontNameParser = new CidFontNameParser();
        private static readonly CodespaceRangeParser CodespaceRangeParser = new CodespaceRangeParser();
        private static readonly CidCharacterParser CidCharacterParser = new CidCharacterParser();

        public CMap Parse(IInputBytes inputBytes, bool isLenientParsing)
        {
            var scanner = new CoreTokenScanner(inputBytes);

            var builder = new CharacterMapBuilder();

            IToken previousToken = null;
            while (scanner.MoveNext())
            {
                var token = scanner.CurrentToken;

                if (token is OperatorToken operatorToken)
                {
                    switch (operatorToken.Data)
                    {
                        case "usecmap":
                            throw new NotImplementedException("External CMap files not yet supported, please submit a pull request!");
                        case "begincodespacerange":
                            {
                                if (previousToken is NumericToken numeric)
                                {
                                    CodespaceRangeParser.Parse(numeric, scanner, builder, isLenientParsing);
                                }
                                else
                                {
                                    throw new InvalidOperationException("Unexpected token preceding start of codespace range: " + previousToken);
                                }
                            }
                            break;
                        case "beginbfchar":
                            {
                                if (previousToken is NumericToken numeric)
                                {
                                    BaseFontCharacterParser.Parse(numeric, scanner, builder, isLenientParsing);
                                }
                                else
                                {
                                    throw new InvalidOperationException("Unexpected token preceding start of base font characters: " + previousToken);
                                }
                            }
                            break;
                        case "beginbfrange":
                            {
                                if (previousToken is NumericToken numeric)
                                {
                                    BaseFontRangeParser.Parse(numeric, scanner, builder, isLenientParsing);
                                }
                                else
                                {
                                    throw new InvalidOperationException("Unexpected token preceding start of base font character ranges: " + previousToken);
                                }
                            }
                            break;
                        case "begincidchar":
                            {
                                if (previousToken is NumericToken numeric)
                                {
                                    CidCharacterParser.Parse(numeric, scanner, builder, isLenientParsing);
                                }
                                else
                                {
                                    throw new InvalidOperationException("Unexpected token preceding start of Cid character mapping: " + previousToken);
                                }
                                break;
                            }
                        case "begincidrange":
                            {
                                if (previousToken is NumericToken numeric)
                                {
                                    CidRangeParser.Parse(numeric, scanner, builder, isLenientParsing);
                                }
                                else
                                {
                                    throw new InvalidOperationException("Unexpected token preceding start of Cid ranges: " + previousToken);
                                }
                            }
                            break;
                    }
                }
                else if (token is NameToken name)
                {
                    CidFontNameParser.Parse(name, scanner, builder, isLenientParsing);
                }

                previousToken = token;
            }

            return builder.Build();
        }
    }
}
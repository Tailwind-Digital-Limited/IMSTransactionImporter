using System.Collections.Generic;
using System.Linq;
using DocumentFormat.OpenXml.Spreadsheet;
using OpenXmlCellFormat = DocumentFormat.OpenXml.Spreadsheet.CellFormat;

namespace Audacia.Spreadsheets
{
    public class StylesheetBuilder
    {
        private readonly ICollection<TableHeaderStyle?> _distinctHeaderStyles;
        private readonly ICollection<string?> _distinctBackgroundColours;
        private readonly ICollection<string?> _distinctTextColours;

        public StylesheetBuilder(IEnumerable<Table> tables)
        {
            var allTables = tables.ToArray();

            _distinctHeaderStyles = allTables
                .Where(t => t.HeaderStyle != null)
                .Select(t => t.HeaderStyle)
                .Distinct()
                .ToArray();

            _distinctBackgroundColours = allTables
                .Where(t => t.Rows != null)
                .SelectMany(dt => dt.Rows)
                .SelectMany(r => r.Cells.Select(c => c.FillColour))
                .Where(c => !string.IsNullOrWhiteSpace(c))
                .Distinct()
                .ToArray();

            _distinctTextColours = allTables
                .Where(t => t.Rows != null)
                .SelectMany(dt => dt.Rows)
                .SelectMany(r => r.Cells.Select(c => c.TextColour))
                .Where(c => !string.IsNullOrWhiteSpace(c))
                .Distinct()
                .ToArray();
        }
        
        /// <summary>
        /// Generates a global stylesheet with dictionaries referencing common colours and fonts.
        /// </summary>
        public SharedDataTable Build()
        {
            var sharedData = new SharedDataTable();

            var numberingFormats = new NumberingFormats
            {
                Count = 4U
            };

            var numberFormats = new[]
            {
                new NumberingFormat
                {
                    NumberFormatId = (uint)CellFormat.Currency,
                    FormatCode = "\"£\"#,##0.00"
                },
                new NumberingFormat
                {
                    NumberFormatId = (uint)CellFormat.AccountingGBP,
                    FormatCode = "_-[$£-809]* #,##0.00_-;\\-[$£-809]* #,##0.00_-;_-[$£-809]* \"-\"??_-;_-@_-"
                },
                new NumberingFormat
                {
                    NumberFormatId = (uint)CellFormat.AccountingUSD,
                    FormatCode = "_-[$$-409]* #,##0.00_ ;_-[$$-409]* \\-#,##0.00\\ ;_-[$$-409]* \"-\"??_ ;_-@_ "
                },
                new NumberingFormat
                {
                    NumberFormatId = (uint)CellFormat.AccountingEUR,
                    FormatCode = "_-[$€-2]\\ * #,##0.00_-;\\-[$€-2]\\ * #,##0.00_-;_-[$€-2]\\ * \"-\"??_-;_-@_-"
                }
            };

            numberingFormats.Append(numberFormats);
            sharedData.Stylesheet = CreateStyleSheet(numberingFormats, sharedData);
            return sharedData;
        }

        private Stylesheet CreateStyleSheet(NumberingFormats numberingFormats, SharedDataTable sharedData)
        {
            var stylesheet = new Stylesheet();
            stylesheet.Append(numberingFormats);
            AddFonts(sharedData, stylesheet);
            AddBorders(stylesheet);
            AddFormats(stylesheet);
            return stylesheet;
        }

        private static void AddFormats(Stylesheet stylesheet)
        {
            // blank cell format list
            stylesheet.CellStyleFormats = new CellStyleFormats
            {
                Count = 1
            };
            var newCellFormat = new OpenXmlCellFormat();
            stylesheet.CellStyleFormats.AppendChild(newCellFormat);

            // Add formats
            var cellFormats = new CellFormats(new OpenXmlCellFormat());
            stylesheet.Append(cellFormats);
        }

        private static void AddBorders(Stylesheet stylesheet)
        {
            var borders = CreateBorders();
            stylesheet.Append(borders);
        }

        private void AddFonts(SharedDataTable sharedData, Stylesheet stylesheet)
        {
            var fonts = new Fonts();
            var fills = new Fills();

            GetTextColours(fonts, sharedData.TextColours);
            GetFillColours(fills, sharedData.FillColours);
            GetHeaderStyles(fonts, fills, sharedData.FillColours, sharedData.Fonts);

            stylesheet.Append(fonts);
            stylesheet.Append(fills);
        }

        private void GetTextColours(Fonts fonts, IDictionary<string, uint> textColourDictionary)
        {
            var defaultFonts = new[]
            {
                new Font
                {
                    Bold = new Bold { Val = false },
                    Italic = new Italic { Val = false },
                    Strike = new Strike { Val = false },
                    Underline = new Underline { Val = UnderlineValues.None },
                    FontSize = new FontSize { Val = 10D },
                    Color = new Color { Rgb = "00000000" },
                    FontName = new FontName { Val = "Calibri" }
                },
                new Font
                {
                    Bold = new Bold { Val = true },
                    Italic = new Italic { Val = false },
                    Strike = new Strike { Val = false },
                    Underline = new Underline { Val = UnderlineValues.None },
                    FontSize = new FontSize { Val = 11D },
                    Color = new Color { Rgb = "FF000000" },
                    FontName = new FontName { Val = "Calibri" }
                }
            };
            fonts.Append(defaultFonts);

            var index = 2;
            foreach (var colour in _distinctTextColours)
            {
                if (string.IsNullOrEmpty(colour) || textColourDictionary.ContainsKey(colour!))
                {
                    continue;
                }

                var newFont = new Font
                {
                    Bold = new Bold
                    {
                        Val = false
                    },
                    Italic = new Italic
                    {
                        Val = false
                    },
                    Strike = new Strike
                    {
                        Val = false
                    },
                    Underline = new Underline
                    {
                        Val = UnderlineValues.None
                    },
                    FontSize = new FontSize
                    {
                        Val = 10D
                    },
                    Color = new Color
                    {
                        Rgb = "FF" + colour
                    },
                    FontName = new FontName
                    {
                        Val = "Calibri"
                    }
                };

                fonts.Append(newFont);
                textColourDictionary.Add(colour!, (uint)index++);
            }
        }

        private void GetFillColours(Fills fills, IDictionary<string, uint> backgroundColoursDictionary)
        {
            var defaultFills = new[]
            {
                new Fill { PatternFill = new PatternFill { PatternType = PatternValues.None } },                             // none
                new Fill { PatternFill = new PatternFill { PatternType = PatternValues.Gray125 } },                          // grey
                new Fill 
                { 
                    PatternFill = new PatternFill
                    {
                        PatternType = PatternValues.Solid,
                        ForegroundColor = new ForegroundColor { Rgb = "FF79A7E3" }
                    } 
                }
            };
            fills.Append(defaultFills);
            
            var index = 3;
            foreach (var colour in _distinctBackgroundColours)
            {
                if (backgroundColoursDictionary.ContainsKey(colour!))
                {
                    continue;
                }

                var newFill = new Fill
                {
                    PatternFill = new PatternFill
                    {
                        PatternType = PatternValues.Solid,
                        ForegroundColor = new ForegroundColor { Rgb = "FF" + colour }
                    }
                };
                fills.Append(newFill);
                backgroundColoursDictionary.Add(colour!, (uint)index++);
            }
        }

#pragma warning disable ACL1002
        private void GetHeaderStyles(
            Fonts fonts, 
            Fills fills,
            IDictionary<string, uint> backgroundColoursDictionary,
            IDictionary<string, uint> fontsDictionary)
#pragma warning restore ACL1002
        {
            var headerFontsIndex = fonts.ChildElements.Count;
            var backgroundFillsIndex = fills.ChildElements.Count;

            foreach (var headerStyle in _distinctHeaderStyles)
            {
                if (headerStyle == null)
                {
                    continue;
                }

                var headerStyleKey = $"{headerStyle.FontName}:{headerStyle.TextColour}";
                if (!fontsDictionary.ContainsKey(headerStyleKey))
                {
                    var newFont = new Font
                    {
                        Bold = new Bold
                        {
                            Val = headerStyle.IsBold
                        },
                        Italic = new Italic
                        {
                            Val = headerStyle.IsItalic
                        },
                        Strike = new Strike
                        {
                            Val = headerStyle.HasStrike
                        },
                        Underline = new Underline
                        {
                            Val = UnderlineValues.None
                        },
                        FontSize = new FontSize
                        {
                            Val = headerStyle.FontSize
                        },
                        Color = new Color
                        {
                            Rgb = "FF" + headerStyle.TextColour
                        },
                        FontName = new FontName
                        {
                            Val = headerStyle.FontName
                        }
                    };

                    fonts.Append(newFont);
                    fontsDictionary.Add(headerStyleKey, (uint)headerFontsIndex++);
                }

                if (backgroundColoursDictionary.ContainsKey(headerStyle.FillColour))
                {
                    continue;
                }

                var newFill = new Fill
                {
                    PatternFill = new PatternFill
                    {
                        PatternType = PatternValues.Solid,
                        ForegroundColor = new ForegroundColor
                        {
                            Rgb = "FF" + headerStyle.FillColour
                        }
                    }
                };

                fills.Append(newFill);
                backgroundColoursDictionary.Add(headerStyle.FillColour, (uint)backgroundFillsIndex++);
            }
        }

        private static Borders CreateBorders()
        {
            var borders = new Borders();
            var borderArray = new[]
            {
                new Border(),
                new Border { TopBorder = new TopBorder { Style = BorderStyleValues.Thin } },
                new Border { RightBorder = new RightBorder { Style = BorderStyleValues.Thin } },
                new Border { TopBorder = new TopBorder { Style = BorderStyleValues.Thin }, RightBorder = new RightBorder { Style = BorderStyleValues.Thin } },
                new Border { BottomBorder = new BottomBorder { Style = BorderStyleValues.Thin } },
                new Border { TopBorder = new TopBorder { Style = BorderStyleValues.Thin }, BottomBorder = new BottomBorder { Style = BorderStyleValues.Thin } },
                new Border { RightBorder = new RightBorder { Style = BorderStyleValues.Thin }, BottomBorder = new BottomBorder { Style = BorderStyleValues.Thin } },
                new Border { TopBorder = new TopBorder { Style = BorderStyleValues.Thin }, BottomBorder = new BottomBorder { Style = BorderStyleValues.Thin }, RightBorder = new RightBorder { Style = BorderStyleValues.Thin } },
                new Border { LeftBorder = new LeftBorder { Style = BorderStyleValues.Thin } },
                new Border { LeftBorder = new LeftBorder { Style = BorderStyleValues.Thin }, TopBorder = new TopBorder { Style = BorderStyleValues.Thin } },
                new Border { RightBorder = new RightBorder { Style = BorderStyleValues.Thin }, LeftBorder = new LeftBorder { Style = BorderStyleValues.Thin } },
                new Border { RightBorder = new RightBorder { Style = BorderStyleValues.Thin }, LeftBorder = new LeftBorder { Style = BorderStyleValues.Thin }, TopBorder = new TopBorder { Style = BorderStyleValues.Thin } },
                new Border { BottomBorder = new BottomBorder { Style = BorderStyleValues.Thin }, LeftBorder = new LeftBorder { Style = BorderStyleValues.Thin } },
                new Border { TopBorder = new TopBorder { Style = BorderStyleValues.Thin }, BottomBorder = new BottomBorder { Style = BorderStyleValues.Thin }, LeftBorder = new LeftBorder { Style = BorderStyleValues.Thin } },
                new Border { RightBorder = new RightBorder { Style = BorderStyleValues.Thin }, LeftBorder = new LeftBorder { Style = BorderStyleValues.Thin }, BottomBorder = new BottomBorder { Style = BorderStyleValues.Thin } },
                new Border { RightBorder = new RightBorder { Style = BorderStyleValues.Thin }, LeftBorder = new LeftBorder { Style = BorderStyleValues.Thin }, TopBorder = new TopBorder { Style = BorderStyleValues.Thin }, BottomBorder = new BottomBorder { Style = BorderStyleValues.Thin } }
            };
            borders.Append(borderArray);
            return borders;
        }
    }
}
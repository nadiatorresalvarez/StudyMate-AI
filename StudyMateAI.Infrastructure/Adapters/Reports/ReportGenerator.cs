using System.Text;
using System.Text.Json;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using DocumentFormat.OpenXml.Vml;
using Markdig;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using StudyMateAI.Application.Common.Abstractions;
using StudyMateAI.Domain.Entities;
using Microsoft.Extensions.Logging;
using IOPath = System.IO.Path;

using WordDocument = DocumentFormat.OpenXml.Wordprocessing.Document;
using QuestDocument = QuestPDF.Fluent.Document;
using V = DocumentFormat.OpenXml.Vml;
using A = DocumentFormat.OpenXml.Drawing;
using Wp = DocumentFormat.OpenXml.Drawing.Wordprocessing;
using Pic = DocumentFormat.OpenXml.Drawing.Pictures;
using WordColor = DocumentFormat.OpenXml.Wordprocessing.Color;
using PdfColor = QuestPDF.Infrastructure.Color;

namespace StudyMateAI.Infrastructure.Adapters.Reports;

public class ReportGenerator : IReportGenerator
{
    private readonly string _logoPath;
    private readonly ILogger<ReportGenerator>? _logger;

    public ReportGenerator(string logoPath = "wwwroot/images/logo-studymate.png", ILogger<ReportGenerator>? logger = null)
    {
        _logoPath = logoPath;
        _logger = logger;
        
        if (string.IsNullOrEmpty(logoPath))
        {
            _logger?.LogWarning("⚠️ La ruta del logo está vacía");
        }
        else if (!File.Exists(logoPath))
        {
            _logger?.LogWarning("⚠️ Logo NO encontrado en: {LogoPath}", logoPath);
            _logger?.LogWarning("   Directorio actual: {CurrentDir}", Directory.GetCurrentDirectory());
            _logger?.LogWarning("   Ruta absoluta esperada: {AbsolutePath}", IOPath.GetFullPath(logoPath));
        }
        else
        {
            var fileInfo = new FileInfo(logoPath);
            _logger?.LogInformation("✅ Logo encontrado: {LogoPath} ({Size} bytes)", logoPath, fileInfo.Length);
        }
    }

    public byte[] GenerateResumenWord(Summary resumen)
    {
        using var stream = new MemoryStream();

        using (var wordDocument = WordprocessingDocument.Create(stream, WordprocessingDocumentType.Document))
        {
            var mainPart = wordDocument.AddMainDocumentPart();
            mainPart.Document = new WordDocument(); 
            var body = mainPart.Document.AppendChild(new Body());

            // ✨ APLICAR ESTILOS PERSONALIZADOS (Poppins + Negritas)
            ApplyCustomStyles(wordDocument);

            // ✨ AGREGAR MARCO DECORATIVO AL DOCUMENTO
            AddDecorativeBorder(body);

            var processedContent = ProcessMarkdownToWordElements(resumen.Content);

            // ✨ TÍTULO PRINCIPAL CON NEGRITA Y POPPINS
            var titleParagraph = CreateStyledTitle($"Resumen: {GetSummaryTypeName(resumen.Type)}");
            body.AppendChild(titleParagraph);

            // ✨ INFORMACIÓN DEL DOCUMENTO CON FORMATO
            if (resumen.Document != null)
            {
                body.AppendChild(CreateInfoParagraph($"📄 Documento Fuente: {resumen.Document.OriginalFileName}"));
            }

            if (resumen.GeneratedAt.HasValue)
            {
                body.AppendChild(CreateInfoParagraph($"📅 Generado el: {resumen.GeneratedAt.Value:dd/MM/yyyy HH:mm}"));
            }

            body.AppendChild(new Paragraph(new Run(new Break())));

            // ✨ CONTENIDO CON MARCO INTERNO
            var contentContainer = new Paragraph();
            contentContainer.ParagraphProperties = new ParagraphProperties(
                new ParagraphBorders(
                    new LeftBorder { Val = BorderValues.Single, Color = "4A90E2", Size = 12 },
                    new RightBorder { Val = BorderValues.Single, Color = "4A90E2", Size = 12 },
                    new TopBorder { Val = BorderValues.Single, Color = "4A90E2", Size = 12 },
                    new BottomBorder { Val = BorderValues.Single, Color = "4A90E2", Size = 12 }
                ),
                new Shading { Fill = "F8FAFC" }
            );
            body.AppendChild(contentContainer);

            foreach (var element in processedContent)
            {
                body.AppendChild(element);
            }

            // ✨ MARCA DE AGUA MEJORADA (Ahora funciona correctamente)
            if (!string.IsNullOrEmpty(_logoPath) && File.Exists(_logoPath))
            {
                _logger?.LogInformation("📄 Agregando marca de agua mejorada a Word desde: {LogoPath}", _logoPath);
                try
                {
                    AddImprovedWatermarkToWord(wordDocument, _logoPath);
                    _logger?.LogInformation("✅ Marca de agua agregada exitosamente a Word");
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, "❌ Error al agregar marca de agua a Word");
                }
            }
            else
            {
                _logger?.LogWarning("⚠️ No se agregó marca de agua a Word. Logo existe: {Exists}, Ruta: {Path}", 
                    File.Exists(_logoPath ?? ""), _logoPath ?? "vacía");
            }
        }

        stream.Position = 0;
        return stream.ToArray();
    }

    public byte[] GenerateCuestionarioPdf(Quiz cuestionario)
    {
        QuestPDF.Settings.License = LicenseType.Community;

        var logoExists = !string.IsNullOrEmpty(_logoPath) && File.Exists(_logoPath);
        _logger?.LogInformation("📊 Generando PDF. Logo disponible: {LogoExists}, Ruta: {LogoPath}", logoExists, _logoPath ?? "vacía");

        var document = QuestDocument.Create(container => 
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);

                // LOGO EN ESQUINA SUPERIOR DERECHA
                if (logoExists && !string.IsNullOrEmpty(_logoPath))
                {
                    try
                    {
                        _logger?.LogInformation("🖼️ Insertando logo en esquina superior derecha: {LogoPath}", _logoPath);
                        
                        page.Foreground()
                            .AlignRight()
                            .AlignTop()
                            .Padding(10)
                            .Width(80)
                            .Image(_logoPath);
                        
                        _logger?.LogInformation("✅ Logo agregado en esquina superior derecha del PDF");
                    }
                    catch (Exception ex)
                    {
                        _logger?.LogError(ex, "❌ Error al agregar logo al PDF");
                    }
                }

                // HEADER
                page.Header().Element(header =>
                {
                    header.Row(row =>
                    {
                        row.RelativeItem().Column(column =>
                        {
                            column.Item().Text(cuestionario.Title).FontSize(20).Bold();
                            if (cuestionario.Document != null)
                            {
                                column.Item().Text($"Documento Fuente: {cuestionario.Document.OriginalFileName}").FontSize(12);
                            }
                            if (cuestionario.GeneratedAt.HasValue)
                            {
                                column.Item().Text($"Generado el: {cuestionario.GeneratedAt.Value:dd/MM/yyyy HH:mm}").FontSize(10);
                            }
                        });
                    });
                });

                // CONTENT
                page.Content().Column(content =>
                {
                    content.Spacing(15);
                    var questions = cuestionario.Questions.OrderBy(q => q.OrderIndex).ToList();

                    for (int i = 0; i < questions.Count; i++)
                    {
                        var question = questions[i];
                        var questionNumber = i + 1;

                        content.Item().Column(questionColumn =>
                        {
                            questionColumn.Item().Border(1).BorderColor(Colors.Grey.Lighten2)
                                .Padding(10).Background(Colors.White)
                                .Column(innerColumn =>
                                {
                                    innerColumn.Item().Text($"{questionNumber}. {question.QuestionText}")
                                        .FontSize(14).Bold().FontColor(Colors.BlueGrey.Darken3);
                                    
                                    innerColumn.Item().PaddingTop(5)
                                        .Text($"Tipo: {GetQuestionTypeName(question.QuestionType)}")
                                        .FontSize(10).FontColor(Colors.Grey.Medium);
                                    
                                    if (question.QuestionType == "MultipleChoice" && 
                                        !string.IsNullOrWhiteSpace(question.OptionsJson))
                                    {
                                        try
                                        {
                                            var options = JsonSerializer.Deserialize<List<string>>(question.OptionsJson);
                                            if (options != null && options.Any())
                                            {
                                                innerColumn.Item().PaddingTop(8).Column(optionsColumn =>
                                                {
                                                    optionsColumn.Spacing(3);
                                                    foreach (var option in options)
                                                    {
                                                        optionsColumn.Item().Text($"  • {option}").FontSize(12);
                                                    }
                                                });
                                            }
                                        }
                                        catch { }
                                    }

                                    innerColumn.Item().PaddingTop(8)
                                        .Text($"Respuesta Correcta: {question.CorrectAnswer}")
                                        .FontSize(11).Bold().FontColor(Colors.Green.Darken2);

                                    if (!string.IsNullOrWhiteSpace(question.Explanation))
                                    {
                                        innerColumn.Item().PaddingTop(5)
                                            .Text($"Explicación: {question.Explanation}")
                                            .FontSize(11).Italic().FontColor(Colors.Blue.Darken1);
                                    }

                                    if (question.Points.HasValue)
                                    {
                                        innerColumn.Item().PaddingTop(2)
                                            .Text($"Puntos: {question.Points.Value}")
                                            .FontSize(10).FontColor(Colors.Grey.Medium);
                                    }
                                });
                        });
                    }
                });

                // FOOTER
                page.Footer().AlignCenter().Text(text =>
                {
                    text.Span($"Página ").FontSize(10);
                    text.CurrentPageNumber().FontSize(10);
                    text.Span(" de ").FontSize(10);
                    text.TotalPages().FontSize(10);
                });
            });
        });

        var pdfBytes = document.GeneratePdf();
        _logger?.LogInformation("✅ PDF generado: {Size} bytes", pdfBytes.Length);
        return pdfBytes;
    }

    #region ✨ NUEVOS MÉTODOS DE ESTILO Y FORMATO

    /// <summary>
    /// Aplica estilos personalizados al documento (Poppins, negritas, colores)
    /// </summary>
    private void ApplyCustomStyles(WordprocessingDocument wordDocument)
    {
        var mainPart = wordDocument.MainDocumentPart;
        if (mainPart == null) return;

        // Crear parte de estilos si no existe
        StyleDefinitionsPart styleDefinitionsPart;
        if (mainPart.StyleDefinitionsPart == null)
        {
            styleDefinitionsPart = mainPart.AddNewPart<StyleDefinitionsPart>();
            styleDefinitionsPart.Styles = new Styles();
        }
        else
        {
            styleDefinitionsPart = mainPart.StyleDefinitionsPart;
        }

        // ✅ VERIFICACIÓN DE NULL
        if (styleDefinitionsPart.Styles == null)
        {
            styleDefinitionsPart.Styles = new Styles();
        }

        var styles = styleDefinitionsPart.Styles;

        // ✨ ESTILO PARA TÍTULO PRINCIPAL
        var titleStyle = new Style
        {
            Type = StyleValues.Paragraph,
            StyleId = "CustomTitle",
            CustomStyle = true
        };
        titleStyle.Append(new StyleName { Val = "Custom Title" });
        titleStyle.Append(new BasedOn { Val = "Heading1" });
        titleStyle.Append(new StyleRunProperties(
            new RunFonts { Ascii = "Poppins", HighAnsi = "Poppins" },
            new Bold(),
            new FontSize { Val = "36" }, // 18pt
            new WordColor { Val = "2C3E50" }
        ));
        titleStyle.Append(new StyleParagraphProperties(
            new SpacingBetweenLines { After = "240" }
        ));

        styles.Append(titleStyle);

        // ✨ ESTILO PARA SUBTÍTULOS
        var subtitleStyle = new Style
        {
            Type = StyleValues.Paragraph,
            StyleId = "CustomSubtitle",
            CustomStyle = true
        };
        subtitleStyle.Append(new StyleName { Val = "Custom Subtitle" });
        subtitleStyle.Append(new StyleRunProperties(
            new RunFonts { Ascii = "Poppins", HighAnsi = "Poppins" },
            new Bold(),
            new FontSize { Val = "24" }, // 12pt
            new WordColor { Val = "34495E" }
        ));

        styles.Append(subtitleStyle);

        // ✨ ESTILO PARA TEXTO NORMAL CON POPPINS
        var normalStyle = new Style
        {
            Type = StyleValues.Paragraph,
            StyleId = "CustomNormal",
            CustomStyle = true,
            Default = true
        };
        normalStyle.Append(new StyleName { Val = "Custom Normal" });
        normalStyle.Append(new StyleRunProperties(
            new RunFonts { Ascii = "Poppins", HighAnsi = "Poppins" },
            new FontSize { Val = "22" } // 11pt
        ));

        styles.Append(normalStyle);
    }

    /// <summary>
    /// Crea un párrafo de título con estilo personalizado
    /// </summary>
    private Paragraph CreateStyledTitle(string text)
    {
        var paragraph = new Paragraph();
        paragraph.ParagraphProperties = new ParagraphProperties(
            new ParagraphStyleId { Val = "CustomTitle" },
            new Justification { Val = JustificationValues.Center }
        );
        
        var run = new Run(new Text(text));
        run.RunProperties = new RunProperties(
            new RunFonts { Ascii = "Poppins", HighAnsi = "Poppins" },
            new Bold(),
            new FontSize { Val = "36" },
            new WordColor { Val = "2C3E50" }
        );
        
        paragraph.Append(run);
        return paragraph;
    }

    /// <summary>
    /// Crea un párrafo de información con iconos y formato
    /// </summary>
    private Paragraph CreateInfoParagraph(string text)
    {
        var paragraph = new Paragraph();
        paragraph.ParagraphProperties = new ParagraphProperties(
            new SpacingBetweenLines { After = "120" }
        );
        
        var run = new Run(new Text(text));
        run.RunProperties = new RunProperties(
            new RunFonts { Ascii = "Poppins", HighAnsi = "Poppins" },
            new FontSize { Val = "20" },
            new WordColor { Val = "555555" }
        );
        
        paragraph.Append(run);
        return paragraph;
    }

    /// <summary>
    /// Agrega un marco decorativo alrededor del documento
    /// </summary>
    private void AddDecorativeBorder(Body body)
    {
        var sectionProps = body.Elements<SectionProperties>().FirstOrDefault();
        if (sectionProps == null)
        {
            sectionProps = new SectionProperties();
            body.Append(sectionProps);
        }

        // ✨ MARCO DECORATIVO CON DOBLE LÍNEA
        var pageBorders = new PageBorders
        {
            OffsetFrom = PageBorderOffsetValues.Page
        };

        pageBorders.Append(new TopBorder
        {
            Val = BorderValues.Double,
            Color = "4A90E2",
            Size = 24,
            Space = 24
        });

        pageBorders.Append(new LeftBorder
        {
            Val = BorderValues.Double,
            Color = "4A90E2",
            Size = 24,
            Space = 24
        });

        pageBorders.Append(new BottomBorder
        {
            Val = BorderValues.Double,
            Color = "4A90E2",
            Size = 24,
            Space = 24
        });

        pageBorders.Append(new RightBorder
        {
            Val = BorderValues.Double,
            Color = "4A90E2",
            Size = 24,
            Space = 24
        });

        sectionProps.RemoveAllChildren<PageBorders>();
        sectionProps.Append(pageBorders);
    }

    /// <summary>
    /// Agrega una marca de agua mejorada que realmente funciona en Word
    /// </summary>
    private void AddImprovedWatermarkToWord(WordprocessingDocument wordDocument, string imagePath)
    {
        var mainPart = wordDocument.MainDocumentPart;
        if (mainPart == null) return;

        // Determinar tipo de imagen
        var imagePartType = IOPath.GetExtension(imagePath).ToLower() switch
        {
            ".png" => ImagePartType.Png,
            ".jpg" => ImagePartType.Jpeg,
            ".jpeg" => ImagePartType.Jpeg,
            ".gif" => ImagePartType.Gif,
            ".bmp" => ImagePartType.Bmp,
            _ => ImagePartType.Png
        };

        // Crear parte de imagen
        var imagePart = mainPart.AddImagePart(imagePartType);
        using (var stream = new FileStream(imagePath, FileMode.Open))
        {
            imagePart.FeedData(stream);
        }
        var imagePartId = mainPart.GetIdOfPart(imagePart);

        // ✨ CREAR HEADER CON MARCA DE AGUA
        var headerPart = mainPart.HeaderParts.FirstOrDefault() ?? mainPart.AddNewPart<HeaderPart>();
        
        if (headerPart.Header == null)
        {
            headerPart.Header = new Header();
        }

        // ✨ CONFIGURAR IMAGEN COMO MARCA DE AGUA (Posición centrada y translúcida)
        var shape = new V.Shape
        {
            Id = "_x0000_s1026",
            Type = "#_x0000_t75",
            Style = "position:absolute;margin-left:0;margin-top:0;width:450pt;height:300pt;z-index:-251658240;mso-position-horizontal:center;mso-position-vertical:center",
            OptionalString = "_x0000_s1026",
            AllowInCell = false
        };

        // ✨ AGREGAR TRANSPARENCIA A LA IMAGEN
        var imageData = new V.ImageData
        {
            RelationshipId = imagePartId,
            Title = "StudyMate AI"
        };

        shape.Append(imageData);

        // ✨ APLICAR OPACIDAD
        shape.Append(new V.Fill
        {
            Opacity = "30%" // 30% de opacidad (70% transparente)
        });

        var paragraph = new Paragraph(
            new Run(
                new Picture(shape)
            )
        );

        // Limpiar contenido anterior del header
        headerPart.Header.RemoveAllChildren();
        headerPart.Header.Append(paragraph);

        var headerPartId = mainPart.GetIdOfPart(headerPart);

        // ✨ APLICAR HEADER A TODAS LAS SECCIONES
        var sections = mainPart.Document.Body?.Elements<SectionProperties>().ToList() 
            ?? new List<SectionProperties>();

        foreach (var section in sections)
        {
            section.RemoveAllChildren<HeaderReference>();
            section.PrependChild(new HeaderReference { Id = headerPartId, Type = HeaderFooterValues.Default });
        }

        if (!sections.Any() && mainPart.Document.Body != null)
        {
            var sectionProps = new SectionProperties();
            sectionProps.PrependChild(new HeaderReference { Id = headerPartId, Type = HeaderFooterValues.Default });
            mainPart.Document.Body.AppendChild(sectionProps);
        }

        _logger?.LogInformation("✅ Marca de agua aplicada con opacidad 30%");
    }

    #endregion

    #region Private Markdown Conversion Helpers (OpenXML)

    private List<OpenXmlElement> ProcessMarkdownToWordElements(string markdownContent)
    {
        var elements = new List<OpenXmlElement>();
        if (string.IsNullOrWhiteSpace(markdownContent)) return elements;

        var pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();
        var document = Markdig.Markdown.Parse(markdownContent, pipeline);

        foreach (var block in document)
        {
            var paragraph = ConvertMarkdownBlockToParagraph(block);
            if (paragraph != null) elements.Add(paragraph);
        }

        if (elements.Count == 0 && !string.IsNullOrWhiteSpace(markdownContent))
            elements.Add(CreateStyledParagraph(markdownContent));

        return elements;
    }

    private Paragraph? ConvertMarkdownBlockToParagraph(Block block) => block switch
    {
        ParagraphBlock p => ConvertParagraphBlock(p),
        HeadingBlock h => ConvertHeadingBlock(h),
        ListBlock l => ConvertListBlock(l),
        _ => CreateStyledParagraph(ExtractTextFromBlock(block))
    };

    private Paragraph ConvertParagraphBlock(ParagraphBlock paragraphBlock)
    {
        var runs = new List<Run>();
        if (paragraphBlock.Inline != null)
        {
            var current = paragraphBlock.Inline.FirstChild;
            while (current != null)
            {
                var run = ConvertInlineToRun(current);
                if (run != null) runs.Add(run);
                current = current.NextSibling;
            }
        }
        
        var paragraph = runs.Count > 0 ? new Paragraph(runs) : CreateStyledParagraph(ExtractTextFromInline(paragraphBlock.Inline?.FirstChild));
        
        // ✨ APLICAR FUENTE POPPINS A PÁRRAFOS NORMALES
        if (paragraph.ParagraphProperties == null)
        {
            paragraph.ParagraphProperties = new ParagraphProperties();
        }
        
        return paragraph;
    }

    private Paragraph ConvertHeadingBlock(HeadingBlock headingBlock)
    {
        var runs = new List<Run>();
        if (headingBlock.Inline != null)
        {
            var current = headingBlock.Inline.FirstChild;
            while (current != null)
            {
                var run = ConvertInlineToRun(current);
                if (run != null) runs.Add(run);
                current = current.NextSibling;
            }
        }
        
        var paragraph = new Paragraph(runs);
        
        // ✨ APLICAR ESTILO PERSONALIZADO A ENCABEZADOS CON POPPINS Y NEGRITA
        paragraph.ParagraphProperties = new ParagraphProperties(
            new ParagraphStyleId { Val = $"Heading{headingBlock.Level}" }
        );
        
        // ✨ FORZAR NEGRITA Y POPPINS EN ENCABEZADOS
        foreach (var run in runs)
        {
            if (run.RunProperties == null)
            {
                run.RunProperties = new RunProperties();
            }
            
            run.RunProperties.Append(new RunFonts { Ascii = "Poppins", HighAnsi = "Poppins" });
            run.RunProperties.Append(new Bold());
            
            // Tamaños según nivel
            var fontSize = headingBlock.Level switch
            {
                1 => "32",
                2 => "28",
                3 => "24",
                _ => "22"
            };
            run.RunProperties.Append(new FontSize { Val = fontSize });
        }
        
        return paragraph;
    }

    private Paragraph ConvertListBlock(ListBlock listBlock)
    {
        var textBuilder = new StringBuilder();
        var itemNumber = 1;
        foreach (var item in listBlock.OfType<ListItemBlock>())
        {
            var itemText = ExtractTextFromBlock(item);
            if (!string.IsNullOrWhiteSpace(itemText))
            {
                var prefix = listBlock.IsOrdered ? $"{itemNumber}. " : "• ";
                textBuilder.AppendLine($"{prefix}{itemText}");
                itemNumber++;
            }
        }
        return CreateStyledParagraph(textBuilder.ToString());
    }

    private Run? ConvertInlineToRun(Inline inline)
    {
        if (inline == null) return null;
        var textContent = inline is LiteralInline literal ? literal.Content.ToString() : ExtractTextFromInline(inline);
        if (string.IsNullOrWhiteSpace(textContent)) return null;

        var runProperties = new RunProperties();
        
        // ✨ SIEMPRE APLICAR POPPINS
        runProperties.AppendChild(new RunFonts { Ascii = "Poppins", HighAnsi = "Poppins" });
        
        if (inline is EmphasisInline emphasis)
        {
            if (emphasis.DelimiterCount == 2)
                runProperties.AppendChild(new Bold());
            else
                runProperties.AppendChild(new Italic());
        }
        else if (inline is CodeInline)
        {
            runProperties.AppendChild(new RunFonts { Ascii = "Courier New" });
            runProperties.AppendChild(new WordColor { Val = "0000FF" });
        }

        var run = new Run(new Text(textContent));
        run.RunProperties = runProperties;
        return run;
    }

    private string ExtractTextFromInline(Inline? inline)
    {
        if (inline == null) return string.Empty;
        var textBuilder = new StringBuilder();
        var current = inline;

        while (current != null)
        {
            switch (current)
            {
                case LiteralInline lit: textBuilder.Append(lit.Content); break;
                case CodeInline code: textBuilder.Append(code.Content); break;
                case LinkInline link when link.FirstChild != null: textBuilder.Append(ExtractTextFromInline(link.FirstChild)); break;
                case EmphasisInline emp when emp.FirstChild != null: textBuilder.Append(ExtractTextFromInline(emp.FirstChild)); break;
            }
            current = current.NextSibling;
        }
        return textBuilder.ToString();
    }

    private string ExtractTextFromBlock(Block? block)
    {
        if (block == null) return string.Empty;
        var textBuilder = new StringBuilder();

        if (block is LeafBlock leafBlock && leafBlock.Inline != null)
            textBuilder.Append(ExtractTextFromInline(leafBlock.Inline.FirstChild));

        if (block is ContainerBlock containerBlock)
            foreach (var child in containerBlock)
                textBuilder.Append(ExtractTextFromBlock(child));

        return textBuilder.ToString();
    }

    /// <summary>
    /// Crea un párrafo simple con fuente Poppins aplicada
    /// </summary>
    private Paragraph CreateStyledParagraph(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return new Paragraph();
        
        var paragraph = new Paragraph();
        var run = new Run(new Text(text));
        
        // ✨ APLICAR POPPINS
        run.RunProperties = new RunProperties(
            new RunFonts { Ascii = "Poppins", HighAnsi = "Poppins" }
        );
        
        paragraph.Append(run);
        return paragraph;
    }
    
    private string GetSummaryTypeName(string type) => type switch
    {
        "Brief" => "Resumen Breve",
        "Detailed" => "Resumen Detallado",
        "KeyConcepts" => "Conceptos Clave",
        _ => "Resumen"
    };

    private string GetQuestionTypeName(string type) => type switch
    {
        "MultipleChoice" => "Opción Múltiple",
        "TrueFalse" => "Verdadero/Falso",
        "ShortAnswer" => "Respuesta Corta",
        _ => type
    };

    #endregion
}
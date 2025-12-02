using System.Text;
using System.Text.Json;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using DocumentFormat.OpenXml.Vml; // ✨ NUEVO: Para marca de agua en Word
using Markdig;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using StudyMateAI.Application.Common.Abstractions;
using StudyMateAI.Domain.Entities;

// Alias para resolver ambigüedad entre Document de OpenXML y Document de tu Domain
using WordDocument = DocumentFormat.OpenXml.Wordprocessing.Document;
using QuestDocument = QuestPDF.Fluent.Document;
using V = DocumentFormat.OpenXml.Vml; // ✨ NUEVO: Alias para VML (marca de agua)

namespace StudyMateAI.Infrastructure.Adapters.Reports;

/// <summary>
/// Adaptador para la generación de reportes (Word y PDF) con marca de agua, implementando IReportGenerator.
/// Utiliza OpenXML y Markdig para documentos Word, y QuestPDF para documentos PDF.
/// </summary>
public class ReportGenerator : IReportGenerator
{
    private readonly string _logoPath;

    // ✨ NUEVO: Constructor con ruta configurable del logo
    public ReportGenerator(string logoPath = "wwwroot/images/logo-studymate.png")
    {
        _logoPath = logoPath;
    }

    /// <summary>
    /// Genera un documento Word (.docx) a partir de un resumen, convirtiendo el contenido Markdown.
    /// ✨ ACTUALIZADO: Ahora incluye marca de agua
    /// </summary>
    public byte[] GenerateResumenWord(Summary resumen)
    {
        using var stream = new MemoryStream();

        // 1. Crear el documento Word
        using (var wordDocument = WordprocessingDocument.Create(stream, WordprocessingDocumentType.Document))
        {
            var mainPart = wordDocument.AddMainDocumentPart();
            mainPart.Document = new WordDocument(); 
            var body = mainPart.Document.AppendChild(new Body());

            // Procesar el contenido Markdown a elementos de OpenXML (Párrafos, Títulos, Listas)
            var processedContent = ProcessMarkdownToWordElements(resumen.Content);

            // 2. Agregar Metadatos del Resumen
            
            // Título principal (Usando estilo Heading1)
            var titleParagraph = new Paragraph(new Run(new Text($"Resumen: {GetSummaryTypeName(resumen.Type)}")));
            titleParagraph.ParagraphProperties = new ParagraphProperties(new ParagraphStyleId { Val = "Heading1" });
            body.AppendChild(titleParagraph);

            if (resumen.Document != null)
            {
                body.AppendChild(new Paragraph(new Run(new Text($"Documento Fuente: {resumen.Document.OriginalFileName}"))));
            }

            if (resumen.GeneratedAt.HasValue)
            {
                body.AppendChild(new Paragraph(new Run(new Text($"Generado el: {resumen.GeneratedAt.Value:dd/MM/yyyy HH:mm}"))));
            }

            // Separador
            body.AppendChild(new Paragraph(new Run(new Break())));

            // 3. Agregar contenido procesado (Markdown)
            foreach (var element in processedContent)
            {
                body.AppendChild(element);
            }

            // ✨ NUEVO: Agregar marca de agua
            if (File.Exists(_logoPath))
            {
                AddWatermarkToWord(wordDocument, _logoPath);
            }
        }

        stream.Position = 0;
        return stream.ToArray();
    }

    /// <summary>
    /// Genera un documento PDF a partir de un cuestionario (quiz) usando QuestPDF.
    /// ✨ ACTUALIZADO: Ahora incluye marca de agua
    /// </summary>
    public byte[] GenerateCuestionarioPdf(Quiz cuestionario)
    {
        QuestPDF.Settings.License = LicenseType.Community;

        var document = QuestDocument.Create(container => 
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);

                // ✨ NUEVO: MARCA DE AGUA como Background
                if (File.Exists(_logoPath))
                {
                    page.Background().AlignCenter().AlignMiddle().Layers(layers =>
                    {
                        layers.Layer()
                            .Opacity(0.15f) // Opacidad 15% (ajustable)
                            .AlignCenter()
                            .AlignMiddle()
                            .Width(300) // Ancho en puntos (ajustable)
                            .Image(_logoPath);
                    });
                }

                // --- HEADER ---
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

                // --- CONTENT ---
                page.Content().Column(content =>
                {
                    content.Spacing(15);
                    var questions = cuestionario.Questions.OrderBy(q => q.OrderIndex).ToList();

                    // Iterar sobre cada pregunta dentro de su propio scope
                    for (int i = 0; i < questions.Count; i++)
                    {
                        var question = questions[i];
                        var questionNumber = i + 1;

                        // IMPORTANTE: Cada Item() debe tener su propia lambda/closure
                        content.Item().Column(questionColumn =>
                        {
                            // Contenedor principal de la pregunta
                            questionColumn.Item().Border(1).BorderColor(Colors.Grey.Lighten2)
                                .Padding(10).Background(Colors.White)
                                .Column(innerColumn =>
                                {
                                    // Título de la Pregunta
                                    innerColumn.Item().Text($"{questionNumber}. {question.QuestionText}")
                                        .FontSize(14).Bold().FontColor(Colors.BlueGrey.Darken3);
                                    
                                    // Tipo de Pregunta
                                    innerColumn.Item().PaddingTop(5)
                                        .Text($"Tipo: {GetQuestionTypeName(question.QuestionType)}")
                                        .FontSize(10).FontColor(Colors.Grey.Medium);
                                    
                                    // Opciones (solo para Opción Múltiple)
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
                                        catch { /* Ignorar si falla el JSON */ }
                                    }

                                    // Respuesta Correcta
                                    innerColumn.Item().PaddingTop(8)
                                        .Text($"Respuesta Correcta: {question.CorrectAnswer}")
                                        .FontSize(11).Bold().FontColor(Colors.Green.Darken2);

                                    // Explicación
                                    if (!string.IsNullOrWhiteSpace(question.Explanation))
                                    {
                                        innerColumn.Item().PaddingTop(5)
                                            .Text($"Explicación: {question.Explanation}")
                                            .FontSize(11).Italic().FontColor(Colors.Blue.Darken1);
                                    }

                                    // Puntos
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

                // --- FOOTER ---
                page.Footer().AlignCenter().Text(text =>
                {
                    text.Span($"Página ").FontSize(10);
                    text.CurrentPageNumber().FontSize(10);
                    text.Span(" de ").FontSize(10);
                    text.TotalPages().FontSize(10);
                });
            });
        });

        return document.GeneratePdf();
    }

    #region ✨ NUEVO: Watermark Helper para Word

    /// <summary>
    /// Agrega una marca de agua (imagen) al documento Word usando el header
    /// </summary>
    private void AddWatermarkToWord(WordprocessingDocument wordDocument, string imagePath)
    {
        var mainPart = wordDocument.MainDocumentPart;
        if (mainPart == null) return;

        // Determinar el tipo de imagen basado en la extensión
        var imagePartType = Path.GetExtension(imagePath).ToLower() switch
        {
            ".png" => ImagePartType.Png,
            ".jpg" or ".jpeg" => ImagePartType.Jpeg,
            ".gif" => ImagePartType.Gif,
            ".bmp" => ImagePartType.Bmp,
            _ => ImagePartType.Png
        };

        // Agregar la imagen como ImagePart
        var imagePart = mainPart.AddImagePart(imagePartType);
        
        using (var stream = new FileStream(imagePath, FileMode.Open))
        {
            imagePart.FeedData(stream);
        }

        var imagePartId = mainPart.GetIdOfPart(imagePart);

        // Crear Header Part para la marca de agua
        var headerPart = mainPart.AddNewPart<HeaderPart>();
        headerPart.Header = new Header();

        // Crear párrafo con la imagen configurada como marca de agua
        var paragraph = new Paragraph(
            new Run(
                new Picture(
                    new V.Shape(
                        new V.ImageData
                        {
                            RelationshipId = imagePartId,
                            Title = "StudyMate AI Watermark"
                        }
                    )
                    {
                        // Estilos: posición absoluta, centrado, opacidad, z-index bajo
                        Style = "position:absolute;left:0;text-align:center;z-index:-1;width:400pt;height:200pt;opacity:0.2",
                        AllowInCell = false
                    }
                )
            )
        );

        headerPart.Header.Append(paragraph);
        var headerPartId = mainPart.GetIdOfPart(headerPart);

        // Aplicar el header a todas las secciones del documento
        var sections = mainPart.Document.Body.Elements<SectionProperties>().ToList();
        
        foreach (var section in sections)
        {
            // Remover headers existentes y agregar el nuevo
            section.RemoveAllChildren<HeaderReference>();
            section.PrependChild(new HeaderReference { Id = headerPartId, Type = HeaderFooterValues.Default });
        }

        // Si no existe ninguna sección, crear una nueva con el header
        if (!sections.Any())
        {
            var sectionProps = new SectionProperties();
            sectionProps.PrependChild(new HeaderReference { Id = headerPartId, Type = HeaderFooterValues.Default });
            mainPart.Document.Body.AppendChild(sectionProps);
        }
    }

    #endregion

    #region Private Markdown Conversion Helpers (OpenXML)

    /// <summary>
    /// Convierte el contenido Markdown del resumen a una lista de elementos OpenXML (Paragraphs).
    /// </summary>
    private List<OpenXmlElement> ProcessMarkdownToWordElements(string markdownContent)
    {
        var elements = new List<OpenXmlElement>();

        if (string.IsNullOrWhiteSpace(markdownContent))
            return elements;

        // Configuración de Markdig para procesar extensiones comunes
        var pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();
        var document = Markdig.Markdown.Parse(markdownContent, pipeline);

        foreach (var block in document)
        {
            // Nota: ListBlock debe ser tratado para generar múltiples párrafos con NumberingProperties
            // Aquí se mantiene la simplificación para obtener solo Párrafos
            var paragraph = ConvertMarkdownBlockToParagraph(block);
            if (paragraph != null)
            {
                elements.Add(paragraph);
            }
        }

        // Caso de fallback si el contenido no fue reconocido como bloque estructurado
        if (elements.Count == 0 && !string.IsNullOrWhiteSpace(markdownContent))
        {
            elements.Add(new Paragraph(new Run(new Text(markdownContent))));
        }

        return elements;
    }

    /// <summary>
    /// Selecciona el conversor apropiado para cada tipo de bloque Markdig.
    /// </summary>
    private Paragraph? ConvertMarkdownBlockToParagraph(Block block)
    {
        return block switch
        {
            ParagraphBlock p => ConvertParagraphBlock(p),
            HeadingBlock h => ConvertHeadingBlock(h),
            ListBlock l => ConvertListBlock(l),
            // Default: Intenta extraer texto simple para bloques no mapeados directamente
            _ => CreateSimpleParagraph(ExtractTextFromBlock(block))
        };
    }

    /// <summary>
    /// Convierte un bloque de párrafo Markdown, analizando los elementos inline (énfasis, código).
    /// </summary>
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

        // Si no se encontraron runs, crea un párrafo simple con el texto extraído como fallback
        // Se asegura que si hay runs, se usan; si no, se intenta el fallback de texto plano.
        return runs.Count > 0 ? new Paragraph(runs) : CreateSimpleParagraph(ExtractTextFromInline(paragraphBlock.Inline?.FirstChild));
    }

    /// <summary>
    /// Convierte un bloque de título Markdown a un párrafo de OpenXML con estilo HeadingX.
    /// </summary>
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
        // Aplica el estilo Heading1, Heading2, etc., basado en el nivel Markdown (#, ##)
        paragraph.ParagraphProperties = new ParagraphProperties(new ParagraphStyleId { Val = $"Heading{headingBlock.Level}" });

        return paragraph;
    }

    /// <summary>
    /// Convierte un bloque de lista (ordenada o no) a un párrafo de texto plano con prefijos (•, 1., 2.).
    /// NOTA: Esta es una simplificación. La implementación completa de listas OpenXML es compleja.
    /// </summary>
    private Paragraph ConvertListBlock(ListBlock listBlock)
    {
        var textBuilder = new StringBuilder();
        var itemNumber = 1;

        foreach (var item in listBlock.OfType<ListItemBlock>())
        {
            // Extrae todo el texto del contenido del ítem de la lista
            var itemText = ExtractTextFromBlock(item);
            if (!string.IsNullOrWhiteSpace(itemText))
            {
                var prefix = listBlock.IsOrdered ? $"{itemNumber}. " : "• ";
                textBuilder.AppendLine($"{prefix}{itemText}");
                itemNumber++;
            }
        }

        // Retorna un párrafo simple con todo el texto de la lista
        return new Paragraph(new Run(new Text(textBuilder.ToString())));
    }

    /// <summary>
    /// Convierte un elemento inline de Markdown a un elemento Run de OpenXML con propiedades de formato.
    /// </summary>
    private Run? ConvertInlineToRun(Inline inline)
    {
        if (inline == null) return null;

        var textContent = inline is LiteralInline literal ? literal.Content.ToString() : ExtractTextFromInline(inline);
        if (string.IsNullOrWhiteSpace(textContent)) return null;

        var runProperties = new RunProperties();

        if (inline is EmphasisInline emphasis)
        {
            // La propiedad IsDouble está obsoleta. Se usa DelimiterCount (2 para negrita, 1 para cursiva)
            if (emphasis.DelimiterCount == 2)
            {
                // **Negrita**
                runProperties.AppendChild(new Bold());
            }
            else
            {
                // *Cursiva*
                runProperties.AppendChild(new Italic());
            }
        }
        else if (inline is CodeInline)
        {
            // `Código` (Usar fuente monoespaciada)
            runProperties.AppendChild(new RunFonts { Ascii = "Courier New" });
            
            // Usamos el namespace completo para especificar la clase Color de OpenXML
            runProperties.AppendChild(new DocumentFormat.OpenXml.Wordprocessing.Color { Val = "0000FF" }); 
        }
        // Nota: LinkInline (enlaces) se maneja extrayendo solo el texto, por simplicidad.

        var run = new Run(new Text(textContent));
        if (runProperties.HasChildren) run.RunProperties = runProperties;

        return run;
    }

    /// <summary>
    /// Función recursiva para extraer texto de elementos inline (necesario para manejar anidamiento como negritas en un enlace).
    /// </summary>
    private string ExtractTextFromInline(Inline? inline)
    {
        if (inline == null) return string.Empty;

        var textBuilder = new StringBuilder();
        var current = inline;

        while (current != null)
        {
            switch (current)
            {
                case LiteralInline lit:
                    // Captura texto simple
                    textBuilder.Append(lit.Content);
                    break;
                case CodeInline code:
                    // Captura contenido de código
                    textBuilder.Append(code.Content);
                    break;
                case LinkInline link when link.FirstChild != null:
                    // Recursivamente captura texto dentro de un enlace
                    textBuilder.Append(ExtractTextFromInline(link.FirstChild));
                    break;
                case EmphasisInline emp when emp.FirstChild != null:
                    // Recursivamente captura texto dentro de énfasis
                    textBuilder.Append(ExtractTextFromInline(emp.FirstChild));
                    break;
            }

            // Mueve al siguiente elemento en línea
            current = current.NextSibling;
        }

        return textBuilder.ToString();
    }

    /// <summary>
    /// Función recursiva para extraer texto de bloques (útil para listas o contenedores).
    /// </summary>
    private string ExtractTextFromBlock(Block? block)
    {
        if (block == null) return string.Empty;

        var textBuilder = new StringBuilder();

        // 1. Bloques de hoja (LeafBlock), contienen Inline elements
        if (block is LeafBlock leafBlock && leafBlock.Inline != null)
        {
            textBuilder.Append(ExtractTextFromInline(leafBlock.Inline.FirstChild));
        }

        // 2. Bloques contenedores (ContainerBlock), contienen otros bloques
        if (block is ContainerBlock containerBlock)
        {
            foreach (var child in containerBlock)
            {
                textBuilder.Append(ExtractTextFromBlock(child));
            }
        }

        return textBuilder.ToString();
    }

    /// <summary>
    /// Crea un párrafo OpenXML simple con texto.
    /// </summary>
    private Paragraph CreateSimpleParagraph(string text)
    {
        return string.IsNullOrWhiteSpace(text) ? new Paragraph() : new Paragraph(new Run(new Text(text)));
    }
    
    // --- Helper de mapeo de nombres ---

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
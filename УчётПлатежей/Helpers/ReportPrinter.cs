using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using System;
using System.Collections.Generic;
using System.Linq;
using Style = DocumentFormat.OpenXml.Wordprocessing.Style;
using System.Windows.Forms;
using System.IO;

namespace УчётПлатежей.Helpers
{
    public static class ReportPrinter
    {
        private const string FILENAME = "report_{0}.docx";
        public static void PrintReport(IEnumerable<платежи> payments, DateTime? dateFrom, DateTime? dateTo, категории selectedCategory)
        {
            string formattedFilename = string.Format(FILENAME, DateTime.Now.ToString("dd-MM-yyyy_HH-mm-ss"));
            string path;

            using (var dialog = new FolderBrowserDialog())
            {
                dialog.Description = "Выберите папку для сохранения отчета";
                dialog.ShowNewFolderButton = true;

                if (dialog.ShowDialog() != DialogResult.OK)
                    return;
                else
                    path = Path.Combine(dialog.SelectedPath, formattedFilename);
                
            }
            
            var processedPayments = payments
                .GroupBy(p => p.продукты.категории)
                .ToDictionary(g => g.Key, g => g.ToList());

            decimal totalSum = payments.Sum(p => p.сумма_со_скидкой ?? 0);

            using (var document = WordprocessingDocument.Create(path, WordprocessingDocumentType.Document))
            {
                MainDocumentPart mainPart = document.AddMainDocumentPart();
                mainPart.Document = new Document(new Body());
                DefineStyles(mainPart);
                AddTitle(mainPart);

                foreach (var category in processedPayments.Keys)
                    AddCategoryParagraph(mainPart, category);

                AddTotalSumParagraph(mainPart);

                string headerPartId = AddHeader(mainPart);
                string footerPartId = AddFooter(mainPart);
                AddSectionProperties(mainPart, headerPartId, footerPartId);

                mainPart.Document.Save();
            }

            MessageBox.Show("Отчет успешно сохранен", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);


            void DefineStyles(MainDocumentPart mainPart)
            {
                StyleDefinitionsPart stylePart = mainPart.AddNewPart<StyleDefinitionsPart>();
                stylePart.Styles = new Styles();

                Style normalStyle = new Style()
                {
                    Type = StyleValues.Paragraph,
                    StyleId = "Normal",
                    Default = true
                };

                normalStyle.Append(
                    new Name() { Val = "Normal" },
                    new RunProperties(
                        new FontSize() { Val = "28" },
                        new RunFonts() { Ascii = "Times New Roman", HighAnsi = "Times New Roman" }
                    )
                );

                stylePart.Styles.Append(normalStyle);
                stylePart.Styles.Save();
            }

            void AddTitle(MainDocumentPart mainPart)
            {
                var titleParagraph = mainPart.Document.Body.AppendChild(new Paragraph(new ParagraphProperties(
                    new ParagraphStyleId() { Val = "Normal" },
                    new Justification() { Val = JustificationValues.Center }
                    )));
                var titleRun = titleParagraph.AppendChild(new Run());
                titleRun.AppendChild(new RunProperties(
                    new Bold(),
                    new FontSize() { Val = "48" }
                    )
                );

                string titleText = "Отчёт о платежах";

                if (dateFrom != null)
                    titleText += $" с {dateFrom:dd.MM.yyyy}";

                if (dateTo != null)
                    titleText += $" по {dateTo:dd.MM.yyyy}";

                if (selectedCategory?.название != null)
                    titleText += $" по категории \"{selectedCategory.название}\"";


                titleRun.AppendChild(new Text(titleText));
            }


            void AddCategoryParagraph(MainDocumentPart mainPart, категории category)
            {
                var categoryParagraph = mainPart.Document.Body.AppendChild(new Paragraph(new ParagraphProperties(
                    new ParagraphStyleId() { Val = "Normal" },
                    new Justification() { Val = JustificationValues.Left }
                )));
                string categoryName = category.название;
                var categoryTitleRun = categoryParagraph.AppendChild(new Run());
                categoryTitleRun.AppendChild(new RunProperties(
                    new Bold(),
                    new Italic(),
                    new FontSize() { Val = "32" }
                    )
                );
                categoryTitleRun.AppendChild(new Text(categoryName));

                var paymentsInCategory = processedPayments[category].OrderBy(p => p.дата_платежа);

                foreach (var payment in paymentsInCategory)
                    AddPaymentToCategoryParagraph(mainPart, payment);

                mainPart.Document.Body.AppendChild(new Paragraph());
            }

            void AddPaymentToCategoryParagraph(MainDocumentPart mainPart, платежи payment)
            {
                var paymentParagraph = mainPart.Document.Body.AppendChild(new Paragraph(new ParagraphProperties(
                    new ParagraphStyleId() { Val = "Normal" },
                    new Justification() { Val = JustificationValues.Left },
                    new Indentation() { Left = "720" }
                )));
                string paymentData = $"{payment.дата_платежа:dd.MM.yyyy}: {payment.продукты.наименование}, {payment.количество} шт. - {payment.сумма_со_скидкой} руб.";
                var paymentRun = paymentParagraph.AppendChild(new Run());
                paymentRun.AppendChild(new RunProperties(
                    new FontSize() { Val = "28" }
                    )
                );
                paymentRun.AppendChild(new Text(paymentData));
            }

            void AddTotalSumParagraph(MainDocumentPart mainPart)
            {
                var totalSumParagraph = mainPart.Document.Body.AppendChild(new Paragraph(new ParagraphProperties(
                    new ParagraphStyleId() { Val = "Normal" },
                    new Justification() { Val = JustificationValues.Left }
                )));
                string totalSumText = $"Итоговая сумма: {totalSum} руб.";
                var totalSumRun = totalSumParagraph.AppendChild(new Run());
                totalSumRun.AppendChild(new RunProperties(
                    new Bold(),
                    new FontSize() { Val = "32" }
                    )
                );
                totalSumRun.AddChild(new Text(totalSumText));
            }

            string AddHeader(MainDocumentPart mainPart)
            {
                HeaderPart headerPart = mainPart.AddNewPart<HeaderPart>();
                string headerPartId = mainPart.GetIdOfPart(headerPart);
                headerPart.Header = new Header(
                    new Paragraph(
                        new ParagraphProperties(new Justification() { Val = JustificationValues.Left }),
                        new Run(new Text($"Пользователь: {AppState.CurrentUser.фио}"))
                    )
                );
                headerPart.Header.Save();
                return headerPartId;
            }

            string AddFooter(MainDocumentPart mainPart)
            {
                FooterPart footerPart = mainPart.AddNewPart<FooterPart>();
                string footerPartId = mainPart.GetIdOfPart(footerPart);
                var pageNumberField = new Paragraph(
                    new ParagraphProperties(new Justification() { Val = JustificationValues.Right }),
                    new Run(new FieldChar() { FieldCharType = FieldCharValues.Begin }),
                    new Run(new FieldCode(" PAGE ") { Space = SpaceProcessingModeValues.Preserve }),
                    new Run(new FieldChar() { FieldCharType = FieldCharValues.Separate }),
                    new Run(new Text()),
                    new Run(new FieldChar() { FieldCharType = FieldCharValues.End })
                );
                footerPart.Footer = new Footer(pageNumberField);
                footerPart.Footer.Save();
                return footerPartId;
            }

            void AddSectionProperties(MainDocumentPart mainPart, string headerPartId, string footerPartId)
            {
                SectionProperties sectionProps = new SectionProperties(
                                    new FooterReference() { Type = HeaderFooterValues.Default, Id = footerPartId },
                                    new HeaderReference() { Type = HeaderFooterValues.Default, Id = headerPartId },
                                    new PageSize() { Width = 11906, Height = 16838 },
                                    new PageMargin()
                                    {
                                        Top = 1440,
                                        Right = 1440,
                                        Bottom = 1440,
                                        Left = 1440,
                                        Header = 720,
                                        Footer = 720
                                    }
                                );
                mainPart.Document.Body.AppendChild(sectionProps);
            }
        }
    }
}

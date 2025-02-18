using System;
using System.IO;
using System.Linq;
using System.Reflection;
using KTruckGui.Models;
using QuestPDF.Companion;
using QuestPDF.Drawing;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using QuestPDF.Previewer;

namespace KTruckGui.Assets
{
    internal class InvoiceDocument : IDocument
    {
        public Invoice Model { get; }
        private readonly Customer FromCustomer;
        private readonly Customer ForCustomer;

        public InvoiceDocument(Invoice model, Customer fromCustomer, Customer forCustomer)
        {
            Model = model ?? throw new ArgumentNullException(nameof(model));
            FromCustomer = fromCustomer ?? throw new ArgumentNullException(nameof(fromCustomer));
            ForCustomer = forCustomer ?? throw new ArgumentNullException(nameof(forCustomer));
        }

        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;
        public DocumentSettings GetSettings() => DocumentSettings.Default;

        public void Compose(IDocumentContainer container)
        {
            container.Page(page =>
            {
                page.Margin(40);

                page.Header().Element(ComposeHeader);
                page.Content().Element(ComposeContent);
                page.Footer().Height(50)
                           .Background(Colors.Blue.Lighten4)
                           .AlignCenter()
                           .AlignMiddle()
                           .Text(text =>
                           {
                               text.Span("Thank you for your business!")
                                    .FontSize(12)
                                    .FontColor(Colors.Blue.Medium);
                           });
            });
        }

        private byte[] GetEmbeddedResource(string resourceName)
        {
            var assembly = Assembly.GetExecutingAssembly();
            using (var stream = assembly.GetManifestResourceStream(resourceName))
            {
                if (stream == null)
                    throw new FileNotFoundException($"Embedded resource '{resourceName}' not found.");

                using (var memoryStream = new MemoryStream())
                {
                    stream.CopyTo(memoryStream);
                    return memoryStream.ToArray();
                }
            }
        }

        private void ComposeHeader(IContainer container)
        {
            container.PaddingVertical(10)
                     .Background(Colors.Grey.Lighten2)
                     .BorderBottom(1)
                     .BorderColor(Colors.Grey.Medium)
                     .Row(row =>
                     {
                         // Left side: Invoice info
                         row.RelativeItem().Column(column =>
                         {
                             column.Item().Text(text =>
                             {
                                 text.Span($"Invoice #{Model.Id.Substring(Model.Id.Length - 12)}")
                                      .FontSize(22)
                                      .Bold()
                                      .FontColor(Colors.Blue.Medium);
                             });
                             column.Item().Text(text =>
                             {
                                 text.Span("Issue Date: ").SemiBold().FontSize(10);
                                 text.Span($"{Model.DateCreated:d}").FontSize(10);
                             });
                             column.Item().Text(text =>
                             {
                                 text.Span("Due Date: ").SemiBold().FontSize(10);
                                 text.Span($"{Model.DueDate:d}").FontSize(10);
                             });
                         });

                         // Right side: Logo
                         string resourceName = "KTruckGui.Assets.KLogApp.png";
                         var logoData = GetEmbeddedResource(resourceName);
                         row.ConstantItem(100)
                            .Height(50)
                            .AlignRight()
                            .AlignMiddle()
                            .Image(logoData, ImageScaling.FitArea);
                     });
        }

        private void ComposeContent(IContainer container)
        {
            const decimal taxRate = 0.0825m; // Example tax rate: 8.25%

            container.Column(column =>
            {
                column.Spacing(25);

                // Customer addresses
                column.Item().Row(row =>
                {
                    row.RelativeItem().Component(new AddressComponent("From", FromCustomer));
                    row.ConstantItem(50); // Spacer
                    row.RelativeItem().Component(new AddressComponent("For", ForCustomer));
                });

                // Invoice Items Table
                column.Item().Table(table =>
                {
                    // Define table columns
                    table.ColumnsDefinition(columns =>
                    {
                        columns.ConstantColumn(25); // Serial number column
                        columns.RelativeColumn(3);  // Item type column
                        columns.RelativeColumn();   // Quantity column
                        columns.RelativeColumn();   // Rate column
                        columns.RelativeColumn();   // Total column
                    });

                    // Table header with background and bold text
                    table.Header(header =>
                    {
                        header.Cell().Element(HeaderCellStyle).Text(text => text.Span("#"));
                        header.Cell().Element(HeaderCellStyle).Text(text => text.Span("Item"));
                        header.Cell().Element(HeaderCellStyle).AlignRight().Text(text => text.Span("Qty"));
                        header.Cell().Element(HeaderCellStyle).AlignRight().Text(text => text.Span("Rate"));
                        header.Cell().Element(HeaderCellStyle).AlignRight().Text(text => text.Span("Total"));

                        static IContainer HeaderCellStyle(IContainer container)
                        {
                            return container.DefaultTextStyle(x => x.Bold().FontColor(Colors.White))
                                            .PaddingVertical(5)
                                            .PaddingHorizontal(2)
                                            .Background(Colors.Grey.Medium)
                                            .BorderBottom(1)
                                            .BorderColor(Colors.Black);
                        }
                    });

                    // Data rows with alternating background colors
                    int index = 1;
                    foreach (var item in Model.InvoiceItems)
                    {
                        var itemTotal = item.Total ?? (item.Rate * item.Quantity);
                        var rowBackground = (index % 2 == 0) ? Colors.Grey.Lighten5 : Colors.White;

                        table.Cell().Element(cell => DataCellStyle(cell, rowBackground))
                            .Text(text => text.Span(index.ToString()));
                        table.Cell().Element(cell => DataCellStyle(cell, rowBackground))
                            .Text(text => text.Span(item.Type));
                        table.Cell().Element(cell => DataCellStyle(cell, rowBackground).AlignRight())
                            .Text(text => text.Span(item.Quantity.ToString()));
                        table.Cell().Element(cell => DataCellStyle(cell, rowBackground).AlignRight())
                            .Text(text => text.Span($"${item.Rate:F2}"));
                        table.Cell().Element(cell => DataCellStyle(cell, rowBackground).AlignRight())
                            .Text(text => text.Span($"${itemTotal:F2}"));
                        index++;
                    }

                    static IContainer DataCellStyle(IContainer container, string backgroundColor)
                    {
                        return container.Background(backgroundColor)
                                        .PaddingVertical(5)
                                        .PaddingHorizontal(2)
                                        .BorderBottom(1)
                                        .BorderColor(Colors.Grey.Lighten2);
                    }
                });

                // Totals Summary
                var subtotal = Model.InvoiceItems.Sum(x => x.Rate * x.Quantity);
                var taxTotal = Model.InvoiceItems.Sum(x => x.Type.ToLower() == "part" ? (x.Rate * x.Quantity) * taxRate : 0m);
                var grandTotal = subtotal + taxTotal;

                column.Item().AlignRight().Column(col =>
                {
                    col.Spacing(2);
                    col.Item().Text(text =>
                        text.Span($"Subtotal: ${subtotal:F2}").FontSize(12));
                    col.Item().Text(text =>
                        text.Span($"Tax (8.25%): ${taxTotal:F2}").FontSize(12));
                    col.Item().Text(text =>
                        text.Span($"Grand Total: ${grandTotal:F2}")
                            .FontSize(14)
                            .SemiBold()
                            .FontColor(Colors.Blue.Darken2));
                });
            });
        }

        public void Preview()
        {
            var document = new InvoiceDocument(Model, FromCustomer, ForCustomer);
            document.ShowInCompanion();
        }
    }
}

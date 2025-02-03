using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            container
                .Page(page =>
                {
                    page.Margin(50);

                    page.Header().Element(ComposeHeader);
                    page.Content().Element(ComposeContent);
                    page.Footer().Height(50).Background(Colors.Grey.Lighten1).Text("Your business is appreciated!").AlignCenter();
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
            container.Row(row =>
            {
                // Add the title and invoice information to the left
                row.RelativeItem().Column(column =>
                {
                    column.Item()
                        .Text($"Invoice #{Model.Id.Substring(Model.Id.Length - 12)}")
                        .FontSize(20).SemiBold().FontColor(Colors.Blue.Medium);

                    column.Item().Text(text =>
                    {
                        text.Span("Issue date: ").SemiBold();
                        text.Span($"{Model.DateCreated:d}");
                    });

                    column.Item().Text(text =>
                    {
                        text.Span("Due date: ").SemiBold();
                        text.Span($"{Model.DueDate:d}");
                    });
                });

                // Get the embedded logo
                string resourceName = "KTruckGui.Assets.KLogApp.png"; // Namespace + folder + file name
                var logoData = GetEmbeddedResource(resourceName);

                // Add the logo to the right
                row.ConstantItem(100).Height(50).AlignRight().AlignMiddle().Image(logoData, ImageScaling.FitArea);
            });
        }


        private void ComposeContent(IContainer container)
        {
            const decimal taxRate = 0.0825m; // Example tax rate: 8.25%

            container.Column(column =>
            {
                column.Spacing(25);

                // Add customer details
                column.Item().Row(row =>
                {
                    row.RelativeItem().Component(new AddressComponent("From", FromCustomer));
                    row.ConstantItem(50); // Spacer
                    row.RelativeItem().Component(new AddressComponent("For", ForCustomer));
                });

                // Add the table for invoice items
                column.Item().Table(table =>
                {
                    // Define table columns
                    table.ColumnsDefinition(columns =>
                    {
                        columns.ConstantColumn(25); // Serial number column
                        columns.RelativeColumn(3); // Item type column
                        columns.RelativeColumn();  // Quantity column
                        columns.RelativeColumn();  // Rate column
                        columns.RelativeColumn();  // Total column
                    });

                    // Define the table header
                    table.Header(header =>
                    {
                        header.Cell().Element(CellStyle).Text("#");
                        header.Cell().Element(CellStyle).Text("Item");
                        header.Cell().Element(CellStyle).AlignRight().Text("Qty");
                        header.Cell().Element(CellStyle).AlignRight().Text("Rate");
                        header.Cell().Element(CellStyle).AlignRight().Text("Total");

                        static IContainer CellStyle(IContainer container)
                        {
                            return container.DefaultTextStyle(x => x.SemiBold())
                                            .PaddingVertical(5)
                                            .BorderBottom(1)
                                            .BorderColor(Colors.Black);
                        }
                    });

                    int index = 1;

                    // Add data rows for each invoice item
                    foreach (var item in Model.InvoiceItems)
                    {
                        var itemTotal = item.Total ?? (item.Rate * item.Quantity);
                        var itemTax = item.Type.ToLower() == "part" ? itemTotal * taxRate : 0m;

                        table.Cell().Element(CellStyle).Text(index++.ToString()); // Serial number
                        table.Cell().Element(CellStyle).Text(item.Type); // Item type
                        table.Cell().Element(CellStyle).AlignRight().Text(item.Quantity.ToString()); // Quantity
                        table.Cell().Element(CellStyle).AlignRight().Text($"${item.Rate:F2}"); // Rate
                        table.Cell().Element(CellStyle).AlignRight().Text($"${itemTotal:F2}"); // Total

                        static IContainer CellStyle(IContainer container)
                        {
                            return container.BorderBottom(1)
                                            .BorderColor(Colors.Grey.Lighten2)
                                            .PaddingVertical(5);
                        }
                    }
                });

                // Calculate subtotal, tax, and grand total
                var grandTotal = Model.InvoiceItems.Sum(x => x.Total ?? (x.Rate * x.Quantity));
                column.Item().AlignRight().Text($"Grand total: ${grandTotal:F2}").FontSize(14).SemiBold();
            });
        }


        public void Preview()
        {
            var document = new InvoiceDocument(Model, FromCustomer, ForCustomer);
            document.ShowInCompanion();
        }
    }
}

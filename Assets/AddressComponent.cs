using System;
using System.Collections.Generic;
using KTruckGui.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace KTruckGui.Assets
{
    internal class AddressComponent : QuestPDF.Infrastructure.IComponent
    {
        private string Title { get; }
        public Customer Customer { get; }


        public AddressComponent(string title, Customer customer)
        {
            Title = title;
            Customer = customer;
        }

        public void Compose(QuestPDF.Infrastructure.IContainer container)
        {
            container.Column(column =>
            {
                column.Spacing(2);

                column.Item().BorderBottom(1).PaddingBottom(5).Text(Title).SemiBold();

                column.Item().Text($"{Customer.FName} {Customer.LName}");
                column.Item().Text(Customer.Address);
                column.Item().Text($"{Customer.City}, {Customer.State}, {Customer.Zip}");
                column.Item().Text(Customer.Email);
                column.Item().Text(Customer.Number);
            });
        }
    }
}

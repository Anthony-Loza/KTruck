namespace KTruckGui
{
    // Keep this namespace exactly (your XAML uses xmlns:local="clr-namespace:KTruckGui")
    public static class TypeValues
    {
        // What your XAML expects:
        public static readonly string[] AllTypes = new[]
        {
            "part",   // taxed in your totals logic
            "labor",  // not taxed
            "fee"     // not taxed (example)
        };

        // (Optional) Keep the name we discussed earlier too — harmless if unused:
        public static readonly string[] InvoiceItemTypes = AllTypes;
    }
}

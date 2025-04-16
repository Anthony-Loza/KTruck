using KTruckGui.Models;

namespace KTruckGui
{
    public partial class TruckForm // changed from 'internal' to 'public' and added 'partial'
    {
        private Vehicle selectedVehicle;

        public TruckForm()
        {
        }

        public TruckForm(Vehicle selectedVehicle)
        {
            this.selectedVehicle = selectedVehicle;
        }

        internal bool ShowDialog()
        {
            throw new NotImplementedException();
        }
    }
}

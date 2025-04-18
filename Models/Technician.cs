using KTruckGui.Models;

public partial class Technician
{
    public int Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public string Phone { get; set; }
    public string Specialty { get; set; }
    public decimal LaborRate { get; set; }
    public string Status { get; set; } // Available, Busy, Off-duty
    public DateTime? HireDate { get; set; }
    public bool IsActive { get; set; }

    // Navigation properties
    public virtual ICollection<Workorder> AssignedWorkOrders { get; set; }

}
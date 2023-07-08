namespace DS3.Models
{
    public class EditRoleViewModel
    {
        public int UserId { get; set; }
        public string Login { get; set; }
        public int? CurrentRoleId { get; set; }
        public int SelectedRoleId { get; set; }
        public List<Role> AvailableRoles { get; set; }
    }
}

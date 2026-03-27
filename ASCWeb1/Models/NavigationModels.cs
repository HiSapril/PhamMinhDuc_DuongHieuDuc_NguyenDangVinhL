namespace ASCWeb1.Models
{
    public class NavigationMenu
    {
        public List<NavigationMenuItem> MenuItems { get; set; } = new List<NavigationMenuItem>();
    }

    public class NavigationMenuItem
    {
        public string DisplayName { get; set; } = string.Empty;
        public string MaterialIcon { get; set; } = string.Empty;
        public string Link { get; set; } = string.Empty;
        public bool IsNested { get; set; }
        public int Sequence { get; set; }
        public List<string> UserRoles { get; set; } = new List<string>();
        public List<NavigationMenuItem> NestedItems { get; set; } = new List<NavigationMenuItem>();
    }
}

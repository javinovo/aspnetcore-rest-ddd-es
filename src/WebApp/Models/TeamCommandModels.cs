namespace WebApp.Models
{
    public class CreateTeam
    {
        public string Name { get; set; }
    }

    public class UpdateTeamName
    {
        public string NewName { get; set; }
        public int OriginalVersion { get; set; }
    }
}

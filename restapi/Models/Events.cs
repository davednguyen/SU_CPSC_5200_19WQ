namespace restapi.Models
{
    public abstract class Event
    {
        public int Resource { get; set; }
    }

    public class Entered : Event
    {
    }

    public class Submittal : Event
    {
    }

    public class Cancellation : Event
    {
        public string Reason { get; set; }        
    }

    public class Rejection : Event
    {
        public string Reason { get; set; }        
    }

    public class Approval : Event
    {
    }

    /// <summary>
    /// Event to delete the time card
    /// </summary>
    public class Deletion : Event
    {
        public string Reason { get; set; }
    }
}
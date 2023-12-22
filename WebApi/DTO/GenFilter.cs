namespace WebApi.DTO
{
    public class GenFilter
    {
        public string IndexName { get; set; }
        public string ?Field { get; set; }
        public object ?Value { get; set; }
    }

}

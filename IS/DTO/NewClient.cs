namespace IS.DTO
{
    public class NewClient
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public string Secret { get; set; }

        // Для wpf клиентов установить равным пустой строке
        public string Uri { get; set; }
    }
}
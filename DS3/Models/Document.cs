namespace DS3.Models
{
    public class Document
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public TypeDoc TypeDoc { get; set; }
        public string FileName { get; set; }
    }

    public class KeyWord
    {
        public int Id { get; set; }
        public string Word { get; set; }
        public Document Document { get; set; }
    }

    public class TypeDoc
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
    public class EditDocumentViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int TypeDocId { get; set; }
        public string KeyWords { get; set; }
    }
}
